using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Calmaria.Privacidade.Taint;
using Microsoft.Extensions.Logging;

namespace Calmaria.Privacidade.Remediacao;

/// <summary>
/// Configuração do LLM (seção Llm, ou as variáveis Llm__UrlBase, Llm__Modelo e Llm__Chave). Qualquer serviço com a
/// API de chat no formato da OpenAI serve: Ollama local (http://localhost:11434/v1/, sem chave), OpenAI, Groq,
/// OpenRouter, Gemini e Claude pelos endereços compatíveis. A chave nunca vai para o repositório.
/// </summary>
public sealed class OpcoesLlm
{
    public const string Secao = "Llm";

    /// <summary>O endereço base, terminado em barra (ex.: http://localhost:11434/v1/).</summary>
    public string? UrlBase { get; set; }

    /// <summary>O nome do modelo (ex.: llama3.2, gpt-4o-mini).</summary>
    public string? Modelo { get; set; }

    /// <summary>A chave da API. Vazia no Ollama.</summary>
    public string? Chave { get; set; }

    /// <summary>O tempo de cada tentativa.</summary>
    public int TempoLimiteEmSegundos { get; set; } = 20;

    public bool Configurado => !string.IsNullOrWhiteSpace(UrlBase) && !string.IsNullOrWhiteSpace(Modelo);
}

/// <summary>
/// Chama o LLM por REST com um HttpClient tipado (tempo limite e nova tentativa vêm da configuração do cliente, na
/// injeção de dependência). Só manda a categoria, o trecho mascarado, o destino e se foi mitigado. Sem
/// configuração, sem internet ou com resposta fora do formato, devolve a frase pronta do
/// <see cref="TradutorModelo"/> e diz o motivo.
/// </summary>
public sealed class TradutorLlm(HttpClient http, OpcoesLlm opcoes, TradutorModelo reserva, ILogger<TradutorLlm> log)
    : ITradutorDeRisco
{
    public const int TamanhoMaximoDaFrase = 400;

    private static readonly Lazy<string> Prompt = new(LerPrompt);

    /// <summary>A versão do prompt, para o painel e o registro de uso de IA.</summary>
    public static string VersaoDoPrompt => Prompt.Value.Split('\n')[0].Trim();

    public async Task<AlertaCidadao> TraduzirAsync(AchadoMascarado achado, DestinoFluxo destino, bool mitigado,
        CancellationToken ct = default)
    {
        if (!opcoes.Configurado)
            return reserva.Traduzir(achado.Categoria, destino, mitigado, "LLM não configurado (Llm__UrlBase e Llm__Modelo).");

        var pergunta = JsonSerializer.Serialize(new
        {
            categoria = achado.Categoria.ToString(),
            trechoMascarado = achado.Trecho,
            destino = destino.ToString(),
            mitigado
        });
        var corpo = new
        {
            model = opcoes.Modelo,
            temperature = 0.2,
            messages = new object[]
            {
                new { role = "system", content = Prompt.Value },
                new { role = "user", content = pergunta }
            }
        };

        // O log mostra o que foi ao serviço externo: só o trecho mascarado.
        log.LogInformation("LLM {Modelo}: pedindo o alerta de {Categoria} em {Destino}, com o trecho mascarado {Trecho}.",
            opcoes.Modelo, achado.Categoria, destino, achado.Trecho);
        var cronometro = Stopwatch.StartNew();
        try
        {
            using var pedido = new HttpRequestMessage(HttpMethod.Post, "chat/completions") { Content = JsonContent.Create(corpo) };
            if (!string.IsNullOrWhiteSpace(opcoes.Chave))
                pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opcoes.Chave);
            using var resposta = await http.SendAsync(pedido, ct);
            if (!resposta.IsSuccessStatusCode)
                return Reserva($"O LLM respondeu {(int)resposta.StatusCode}.");

            var json = await resposta.Content.ReadFromJsonAsync<JsonElement>(ct);
            var texto = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            var alerta = Interpretar(texto);
            if (alerta is null)
                return Reserva("A resposta do LLM não veio no formato combinado.");

            log.LogInformation("LLM respondeu o alerta de {Categoria} em {Milissegundos} ms.", achado.Categoria,
                cronometro.ElapsedMilliseconds);
            return alerta;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // Sem rede, tempo esgotado, circuito aberto ou JSON quebrado: o alerta sai da frase pronta.
            return Reserva($"Falha ao chamar o LLM ({ex.GetType().Name}).");
        }

        AlertaCidadao Reserva(string motivo)
        {
            log.LogWarning("LLM indisponível para {Categoria}: {Motivo} Usando a frase pronta.", achado.Categoria, motivo);
            return reserva.Traduzir(achado.Categoria, destino, mitigado, motivo);
        }
    }

    /// <summary>Lê o JSON da resposta, mesmo cercado de texto ou de bloco de código. Nulo se não servir.</summary>
    public static AlertaCidadao? Interpretar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var inicio = texto.IndexOf('{');
        var fim = texto.LastIndexOf('}');
        if (inicio < 0 || fim <= inicio)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(texto[inicio..(fim + 1)]);
            var raiz = doc.RootElement;
            var cidadao = raiz.TryGetProperty("paraCidadao", out var c) ? c.GetString()?.Trim() : null;
            var equipe = raiz.TryGetProperty("paraEquipe", out var e) ? e.GetString()?.Trim() : null;
            if (string.IsNullOrEmpty(cidadao) || string.IsNullOrEmpty(equipe)
                || cidadao.Length > TamanhoMaximoDaFrase || equipe.Length > TamanhoMaximoDaFrase)
                return null;

            return new AlertaCidadao(cidadao, equipe, OrigemDoAlerta.Llm);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return null;
        }
    }

    private static string LerPrompt()
    {
        using var recurso = typeof(TradutorLlm).Assembly.GetManifestResourceStream("prompt-alerta.md")
                            ?? throw new InvalidOperationException("O prompt-alerta.md não foi embutido no módulo.");
        using var leitor = new StreamReader(recurso);
        return leitor.ReadToEnd();
    }
}
