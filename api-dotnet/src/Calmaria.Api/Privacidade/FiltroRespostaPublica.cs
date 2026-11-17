using System.Text.Json;
using System.Text.Json.Nodes;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Calmaria.Api.Privacidade;

/// <summary>Põe o <see cref="FiltroRespostaPublica"/> numa rota pública.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class FiltroRespostaPublicaAttribute() : TypeFilterAttribute(typeof(FiltroRespostaPublica));

/// <summary>
/// Guarda de saída das rotas públicas (a listagem de relatos e o resumo). Antes de a resposta sair, cada texto do
/// JSON passa pelo motor de PII; o que tiver dado pessoal sai mascarado e o fluxo bloqueado é registrado. O que entra
/// pela API já vem sanitizado pelo tipo; este guarda pega o que entrou por fora (importação, registro antigo).
/// </summary>
public sealed class FiltroRespostaPublica(IMotorPii motor, IRastreadorDeFluxo rastreador, IOptions<JsonOptions> json)
    : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: { } valor } resultado && (resultado.StatusCode ?? 200) < 300)
        {
            var opcoes = json.Value.JsonSerializerOptions;
            var no = JsonSerializer.SerializeToNode(valor, valor.GetType(), opcoes);
            var achados = new List<Achado>();
            Limpar(no, achados);

            if (achados.Count > 0)
            {
                rastreador.Registrar($"resposta:{context.HttpContext.Request.Path}", DestinoFluxo.RespostaPublica,
                    achados, bloqueado: true);
                context.Result = new ContentResult
                {
                    Content = no!.ToJsonString(opcoes),
                    ContentType = "application/json; charset=utf-8",
                    StatusCode = resultado.StatusCode ?? StatusCodes.Status200OK
                };
            }
        }

        return next();
    }

    private void Limpar(JsonNode? no, List<Achado> achados)
    {
        switch (no)
        {
            case JsonObject objeto:
                foreach (var (chave, filho) in objeto.ToList())
                {
                    if (Mascarado(filho, achados) is { } novo)
                        objeto[chave] = novo;
                    else
                        Limpar(filho, achados);
                }

                break;
            case JsonArray lista:
                for (var i = 0; i < lista.Count; i++)
                {
                    if (Mascarado(lista[i], achados) is { } novo)
                        lista[i] = novo;
                    else
                        Limpar(lista[i], achados);
                }

                break;
        }
    }

    /// <summary>O texto mascarado, ou nulo se o nó não é texto ou não tem dado pessoal.</summary>
    private JsonValue? Mascarado(JsonNode? no, List<Achado> achados)
    {
        if (no is not JsonValue valor || !valor.TryGetValue<string>(out var texto))
            return null;

        var doTexto = motor.Analisar(texto);
        if (doTexto.Count == 0)
            return null;

        achados.AddRange(doTexto);
        return JsonValue.Create(Sanitizador.Mascarar(texto, doTexto));
    }
}
