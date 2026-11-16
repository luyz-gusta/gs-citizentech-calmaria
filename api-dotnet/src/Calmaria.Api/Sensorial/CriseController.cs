using Calmaria.Api.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Sensorial;

/// <summary>O botão de crise: o refúgio mais perto e a lista de refúgios para usar sem internet.</summary>
[ApiController]
[Route("api")]
[Tags("Sensorial")]
public sealed class CriseController(IRefugioService refugios) : ControllerBase
{
    /// <summary>O refúgio mais perto: um refúgio fixo ou um lugar CALMO agora. Lugar INTENSO nunca entra.</summary>
    /// <remarks>A coordenada da pessoa não é gravada nem vai para o log.</remarks>
    /// <param name="lat">Latitude de onde a pessoa está (de -90 a 90).</param>
    /// <param name="lng">Longitude de onde a pessoa está (de -180 a 180).</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O refúgio, a distância e o trajeto a pé.</response>
    /// <response code="400">Coordenada faltando ou inválida.</response>
    /// <response code="404">Não há nenhum refúgio cadastrado.</response>
    [HttpGet("crise/refugio-mais-proximo")]
    [ProducesResponseType<RefugioMaisProximoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> RefugioMaisProximo([FromQuery] double? lat, [FromQuery] double? lng,
        CancellationToken ct)
    {
        if (lat is null)
            ModelState.AddModelError(nameof(lat), "Informe a latitude.");
        else if (lat is < -90 or > 90)
            ModelState.AddModelError(nameof(lat), "A latitude deve estar entre -90 e 90.");
        if (lng is null)
            ModelState.AddModelError(nameof(lng), "Informe a longitude.");
        else if (lng is < -180 or > 180)
            ModelState.AddModelError(nameof(lng), "A longitude deve estar entre -180 e 180.");
        if (!ModelState.IsValid)
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);

        var escolhido = await refugios.MaisProximoAsync(new Coordenada(lat!.Value, lng!.Value), ct);
        return escolhido is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Nenhum refúgio cadastrado",
                detail: "Não há refúgio fixo nem lugar calmo agora para sugerir.")
            : Ok(RefugioMaisProximoDto.De(escolhido));
    }

    /// <summary>Todos os refúgios fixos. O app guarda essa lista para o botão de crise funcionar sem internet.</summary>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Os refúgios.</response>
    [HttpGet("refugios")]
    [ProducesResponseType<IReadOnlyList<RefugioDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refugios(CancellationToken ct) =>
        Ok((await refugios.ListarFixosAsync(ct)).Select(RefugioDto.De));
}
