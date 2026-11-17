using Calmaria.Domain.Sensorial;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Sensorial;

/// <summary>Ingestão das leituras do ESP32 por HTTP. O caminho de verdade é o MQTT; este serve para teste.</summary>
[ApiController]
[Route("api/leituras")]
[Tags("Sensorial")]
public sealed class LeiturasController(ILeituraService leituras) : ControllerBase
{
    /// <summary>Registra uma leitura, no mesmo formato da mensagem MQTT. A API recalcula o índice.</summary>
    /// <param name="corpo">A leitura.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="201">Leitura gravada, com o índice calculado pela API.</response>
    /// <response code="400">Campos faltando, ou o dispositivo é de outro lugar.</response>
    /// <response code="404">Dispositivo não cadastrado.</response>
    /// <response code="422">Valor fisicamente impossível (som, luz ou movimento fora da faixa).</response>
    [HttpPost]
    [ProducesResponseType<LeituraDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<IActionResult> Registrar([FromBody] LeituraRecebidaDto corpo, CancellationToken ct)
    {
        var leitura = await leituras.RegistrarAsync(corpo.ParaNovaLeitura(), ct);
        return Created($"/api/lugares/{corpo.LugarId}/leituras", LeituraDto.De(leitura));
    }
}
