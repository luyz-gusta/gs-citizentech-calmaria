using Calmaria.Api.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Sensorial;

/// <summary>Os lugares do mapa, com o nível de sobrecarga de agora.</summary>
[ApiController]
[Route("api/lugares")]
[Tags("Sensorial")]
public sealed class LugaresController(ILugarService lugares) : ControllerBase
{
    /// <summary>Lista os lugares; com lat e lng, só os que ficam no raio, do mais perto ao mais longe.</summary>
    /// <param name="lat">Latitude de onde a pessoa está (de -90 a 90). Vai junto com lng.</param>
    /// <param name="lng">Longitude de onde a pessoa está (de -180 a 180). Vai junto com lat.</param>
    /// <param name="raioM">Raio em metros, de 1 a 50.000. Padrão: 1.500.</param>
    /// <param name="tipo">Filtra por tipo: MERCADO, ESTACAO, PARQUE, REFUGIO ou OUTRO.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Os lugares.</response>
    /// <response code="400">Coordenada ou raio inválido.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LugarResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Listar(
        [FromQuery] double? lat, [FromQuery] double? lng, [FromQuery] int? raioM, [FromQuery] TipoLugar? tipo,
        CancellationToken ct)
    {
        if (lat.HasValue != lng.HasValue)
            ModelState.AddModelError(lat.HasValue ? nameof(lng) : nameof(lat), "Informe lat e lng juntos.");
        if (lat is < -90 or > 90)
            ModelState.AddModelError(nameof(lat), "A latitude deve estar entre -90 e 90.");
        if (lng is < -180 or > 180)
            ModelState.AddModelError(nameof(lng), "A longitude deve estar entre -180 e 180.");
        if (raioM is < 1 or > FiltroLugares.RaioMaximoM)
            ModelState.AddModelError(nameof(raioM), $"O raio deve estar entre 1 e {FiltroLugares.RaioMaximoM} metros.");
        if (!ModelState.IsValid)
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);

        var origem = lat is null || lng is null ? null : new Coordenada(lat.Value, lng.Value);
        var filtro = new FiltroLugares(origem, raioM ?? FiltroLugares.RaioPadraoM, tipo);
        var resultado = await lugares.ListarAsync(filtro, ct);
        return Ok(resultado.Select(LugarResumoDto.De));
    }

    /// <summary>Um lugar com a última leitura do sensor.</summary>
    /// <param name="id">Identificador do lugar.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O lugar.</response>
    /// <response code="404">Não existe lugar com esse id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<LugarDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
    {
        var lugar = await lugares.ObterAsync(id, ct);
        return lugar is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Lugar não encontrado",
                detail: $"Não existe lugar com o id {id}.")
            : Ok(LugarDetalheDto.De(lugar));
    }
}
