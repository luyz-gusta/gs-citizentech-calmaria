using Calmaria.Api.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Sensorial;

/// <summary>Os lugares do mapa, com o nível de sobrecarga de agora.</summary>
[ApiController]
[Route("api/lugares")]
[Tags("Sensorial")]
public sealed class LugaresController(
    ILugarService lugares,
    ILugarCadastro cadastro,
    ILeituraService leituras,
    IHorarioCalmoService horariosCalmos,
    TimeProvider relogio) : ControllerBase
{
    private static readonly TimeSpan HistoricoPadrao = TimeSpan.FromHours(24);

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
        return lugar is null ? LugarNaoEncontrado(id) : Ok(LugarDetalheDto.De(lugar));
    }

    /// <summary>O histórico de leituras do lugar, da mais nova para a mais velha (no máximo 2.000).</summary>
    /// <param name="id">Identificador do lugar.</param>
    /// <param name="desde">Data e hora com fuso (ex.: 2026-11-16T00:00:00-03:00). Padrão: as últimas 24 horas.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">As leituras.</response>
    /// <response code="404">Não existe lugar com esse id.</response>
    [HttpGet("{id:int}/leituras")]
    [ProducesResponseType<IReadOnlyList<LeituraDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Leituras(int id, [FromQuery] DateTimeOffset? desde, CancellationToken ct)
    {
        var inicio = desde?.UtcDateTime ?? relogio.GetUtcNow().UtcDateTime - HistoricoPadrao;
        var historico = await leituras.ListarAsync(id, inicio, ct);
        return historico is null ? LugarNaoEncontrado(id) : Ok(historico.Select(LeituraDto.De));
    }

    /// <summary>
    /// As 3 horas mais calmas da semana (dentro do horário de funcionamento) e a grade de médias, pelas leituras
    /// dos últimos 28 dias, no horário de São Paulo.
    /// </summary>
    /// <param name="id">Identificador do lugar.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Os horários calmos; com menos de 20 leituras, "suficiente" vem falso.</response>
    /// <response code="404">Não existe lugar com esse id.</response>
    [HttpGet("{id:int}/horarios-calmos")]
    [ProducesResponseType<HorariosCalmosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> HorariosCalmos(int id, CancellationToken ct)
    {
        var resultado = await horariosCalmos.CalcularAsync(id, ct);
        return resultado is null ? LugarNaoEncontrado(id) : Ok(HorariosCalmosDto.De(resultado));
    }

    /// <summary>
    /// Cadastra um lugar (uso da equipe). Com o tipo REFUGIO, vira um refúgio calmo fixo, que o botão de crise passa
    /// a considerar. Exige o cabeçalho X-Admin-Key.
    /// </summary>
    /// <param name="corpo">Os dados do lugar.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="201">O lugar criado, com o endereço dele no cabeçalho Location.</response>
    /// <response code="400">Dado inválido (nome, coordenada, distrito, horário fora do formato).</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    [HttpPost]
    [ExigeChaveAdmin]
    [ProducesResponseType<LugarDetalheDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> Criar([FromBody] LugarEntradaDto corpo, CancellationToken ct)
    {
        var criado = await cadastro.CriarAsync(corpo.ParaDados(), ct);
        return Created($"/api/lugares/{criado.Id}", LugarDetalheDto.De(criado));
    }

    /// <summary>Atualiza o cadastro de um lugar. Refúgio não vira lugar monitorado, nem o contrário.</summary>
    /// <param name="id">Identificador do lugar.</param>
    /// <param name="corpo">Os dados novos.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O lugar atualizado.</response>
    /// <response code="400">Dado inválido.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    /// <response code="404">Não existe lugar com esse id.</response>
    /// <response code="409">Tentativa de trocar entre refúgio e lugar monitorado.</response>
    [HttpPut("{id:int}")]
    [ExigeChaveAdmin]
    [ProducesResponseType<LugarDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] LugarEntradaDto corpo, CancellationToken ct)
    {
        var atualizado = await cadastro.AtualizarAsync(id, corpo.ParaDados(), ct);
        return atualizado is null ? LugarNaoEncontrado(id) : Ok(LugarDetalheDto.De(atualizado));
    }

    /// <summary>Apaga um lugar. Lugar com sensor ou com relatos não se apaga, para não perder o histórico.</summary>
    /// <param name="id">Identificador do lugar.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="204">Apagado.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    /// <response code="404">Não existe lugar com esse id.</response>
    /// <response code="409">O lugar tem sensor ou relatos.</response>
    [HttpDelete("{id:int}")]
    [ExigeChaveAdmin]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Apagar(int id, CancellationToken ct) =>
        await cadastro.ApagarAsync(id, ct) ? NoContent() : LugarNaoEncontrado(id);

    private ObjectResult LugarNaoEncontrado(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Lugar não encontrado",
            detail: $"Não existe lugar com o id {id}.");
}
