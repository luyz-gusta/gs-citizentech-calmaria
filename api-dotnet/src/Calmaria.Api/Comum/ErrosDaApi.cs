using Calmaria.Domain.Comum;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Comum;

/// <summary>Respostas de erro no formato ProblemDetails, com títulos em português.</summary>
public static class ErrosDaApi
{
    public const string TituloDadosInvalidos = "Dados inválidos";

    /// <summary>Usado pelo [ApiController] quando o model binding falha (ex.: tipo=CINEMA).</summary>
    public static IActionResult DadosInvalidos(ActionContext contexto)
    {
        var problema = new ValidationProblemDetails(contexto.ModelState)
        {
            Title = TituloDadosInvalidos,
            Status = StatusCodes.Status400BadRequest
        };
        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    }
}

/// <summary>
/// Regra de negócio violada vira 400; recurso que não existe, 404; valor físico impossível do sensor, 422. O resto
/// segue para o tratamento padrão (500 sem detalhes internos).
/// </summary>
public sealed class TratadorDeErrosDeDominio(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excecao, CancellationToken ct)
    {
        if (excecao is not DominioException erro)
            return false;

        var (status, titulo) = erro switch
        {
            RecursoNaoEncontradoException nao => (StatusCodes.Status404NotFound, nao.Titulo),
            ValorForaDaFaixaException => (StatusCodes.Status422UnprocessableEntity, "Valor fora da faixa"),
            _ => (StatusCodes.Status400BadRequest, ErrosDaApi.TituloDadosInvalidos)
        };
        http.Response.StatusCode = status;
        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = excecao,
            ProblemDetails = new ProblemDetails { Status = status, Title = titulo, Detail = erro.Message }
        });
    }
}
