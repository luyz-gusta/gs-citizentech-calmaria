using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Calmaria.Api.Comum;

/// <summary>
/// Rotas administrativas (importar previsões, varreduras, Risco Cidadão): exigem o cabeçalho X-Admin-Key igual à
/// configuração Privacidade:ChaveAdmin (na VM, a variável Privacidade__ChaveAdmin). Sem chave configurada, nada
/// passa. A comparação é em tempo constante, para não vazar a chave pelo tempo de resposta.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ExigeChaveAdminAttribute : Attribute, IAuthorizationFilter
{
    public const string Cabecalho = "X-Admin-Key";
    public const string Configuracao = "Privacidade:ChaveAdmin";

    public void OnAuthorization(AuthorizationFilterContext contexto)
    {
        var esperada = contexto.HttpContext.RequestServices.GetRequiredService<IConfiguration>()[Configuracao];
        var enviada = contexto.HttpContext.Request.Headers[Cabecalho].ToString();
        if (!string.IsNullOrEmpty(esperada) && Iguais(esperada, enviada))
            return;

        contexto.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Chave de administrador ausente ou errada",
            Detail = $"Esta rota exige o cabeçalho {Cabecalho}."
        })
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static bool Iguais(string esperada, string enviada) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(esperada), Encoding.UTF8.GetBytes(enviada));
}
