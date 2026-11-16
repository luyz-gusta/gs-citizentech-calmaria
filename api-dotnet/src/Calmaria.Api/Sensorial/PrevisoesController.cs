using Calmaria.Api.Comum;
using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Sensorial;

/// <summary>A previsão de reclamações de barulho por distrito e dia, feita pelo modelo de ML com o SP156.</summary>
[ApiController]
[Route("api/previsoes")]
[Tags("Sensorial")]
public sealed class PrevisoesController(IPrevisaoService previsoes, TimeProvider relogio) : ControllerBase
{
    private const int DiasPadrao = 7;
    private const int DiasMaximos = 31;
    private const long TamanhoMaximoDoCsv = 5 * 1024 * 1024;

    /// <summary>A previsão do distrito nos próximos dias, a partir de hoje (ou de um dia só, com data).</summary>
    /// <param name="distrito">Distrito como no SP156 (CONSOLACAO, BELA VISTA); acento e maiúscula não importam.</param>
    /// <param name="dias">Quantos dias a partir de hoje, de 1 a 31. Padrão: 7.</param>
    /// <param name="data">Um dia só (AAAA-MM-DD), no lugar de dias.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">As previsões, em ordem de data.</response>
    /// <response code="400">Distrito faltando ou dias fora de 1 a 31.</response>
    /// <response code="404">O distrito não tem previsão.</response>
    [HttpGet("barulho")]
    [ProducesResponseType<IReadOnlyList<PrevisaoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Barulho([FromQuery] string? distrito, [FromQuery] int? dias,
        [FromQuery] DateOnly? data, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(distrito))
            ModelState.AddModelError(nameof(distrito), "Informe o distrito.");
        if (dias is < 1 or > DiasMaximos)
            ModelState.AddModelError(nameof(dias), $"Os dias devem ir de 1 a {DiasMaximos}.");
        if (!ModelState.IsValid)
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);

        var hoje = DateOnly.FromDateTime(HorarioDeSaoPaulo.Converter(relogio.GetUtcNow().UtcDateTime).DateTime);
        var (inicio, quantos) = data is { } dia ? (dia, 1) : (hoje, dias ?? DiasPadrao);
        var resultado = await previsoes.ConsultarAsync(distrito!, inicio, quantos, ct);
        return resultado is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Distrito sem previsão",
                detail: $"Não há previsão de barulho para o distrito {Distrito.Normalizar(distrito!)}.")
            : Ok(resultado.Select(PrevisaoDto.De));
    }

    /// <summary>Importa o CSV do notebook de ML; o mesmo distrito e dia é substituído. Exige X-Admin-Key.</summary>
    /// <param name="arquivo">O previsao_barulho.csv (distrito,data,reclamacoes_previstas,nivel), até 5 MB.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Quantas linhas foram importadas.</response>
    /// <response code="400">Arquivo faltando, grande demais ou com linha errada (a mensagem diz qual).</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key certo.</response>
    [HttpPost("importar")]
    [ExigeChaveAdmin]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ImportacaoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> Importar(IFormFile? arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
            ModelState.AddModelError(nameof(arquivo), "Envie o CSV no campo arquivo.");
        else if (arquivo.Length > TamanhoMaximoDoCsv)
            ModelState.AddModelError(nameof(arquivo), "O CSV passa de 5 MB.");
        if (!ModelState.IsValid)
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);

        await using var conteudo = arquivo!.OpenReadStream();
        return Ok(new ImportacaoDto(await previsoes.ImportarAsync(conteudo, ct)));
    }
}

/// <summary>A previsão de um distrito num dia.</summary>
/// <param name="Distrito">Distrito como no SP156.</param>
/// <param name="Data">O dia (AAAA-MM-DD).</param>
/// <param name="ReclamacoesPrevistas">Reclamações de barulho previstas para o dia.</param>
/// <param name="Nivel">BAIXO, MEDIO ou ALTO, pelos tercis do próprio distrito.</param>
public sealed record PrevisaoDto(string Distrito, DateOnly Data, double ReclamacoesPrevistas, NivelPrevisao Nivel)
{
    public static PrevisaoDto De(PrevisaoBarulho p) => new(p.Distrito, p.Data, p.ReclamacoesPrevistas, p.Nivel);
}

/// <summary>Resultado da importação do CSV.</summary>
public sealed record ImportacaoDto(int Importadas);
