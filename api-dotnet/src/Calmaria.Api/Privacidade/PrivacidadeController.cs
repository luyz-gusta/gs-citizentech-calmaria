using System.ComponentModel.DataAnnotations;
using Calmaria.Api.Comum;
using Calmaria.Domain.Comum;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Remediacao;
using Calmaria.Privacidade.Risco;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Privacidade;

/// <summary>
/// O módulo de privacidade: o que o app consulta antes de enviar um texto do cidadão e as rotas administrativas do
/// painel Risco Cidadão (varreduras, indicador, achados e fluxos), que exigem o cabeçalho X-Admin-Key.
/// </summary>
[ApiController]
[Route("api/privacidade")]
[Tags("Privacidade")]
public sealed class PrivacidadeController(ISanitizador sanitizador, IServicoDePrivacidade privacidade,
    IConfiguration configuracao) : ControllerBase
{
    /// <summary>
    /// Analisa um texto e devolve o que seria removido, já mascarado. O app usa antes de enviar um relato, para
    /// avisar a pessoa. Nada é gravado.
    /// </summary>
    /// <param name="corpo">O texto, até 2.000 caracteres.</param>
    /// <response code="200">Os achados mascarados e o texto como ficaria.</response>
    /// <response code="400">Texto faltando ou longo demais.</response>
    [HttpPost("analisar")]
    [ProducesResponseType<AnaliseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public IActionResult Analisar([FromBody] TextoParaAnaliseDto corpo)
    {
        var resultado = sanitizador.Sanitizar(corpo.Texto!);
        return Ok(new AnaliseDto(resultado.TinhaDadoPessoal,
            resultado.Removidos.Select(AchadoDto.De).ToList(),
            resultado.Texto));
    }

    /// <summary>
    /// Roda uma varredura: lê os logs, os relatos gravados, as últimas mensagens do broker MQTT e o código, acha
    /// dado pessoal exposto, junta os fluxos que os guardas barraram nos últimos dias, calcula o Risco Cidadão e
    /// traduz cada tipo de achado em alerta (pelo LLM, ou frase pronta sem ele).
    /// </summary>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="201">A varredura, com o Risco Cidadão e os alertas.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    [HttpPost("varreduras")]
    [ExigeChaveAdmin]
    [ProducesResponseType<VarreduraDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> Varrer(CancellationToken ct)
    {
        var feita = await privacidade.VarrerAsync(ct);
        return Created("/api/privacidade/risco-cidadao", VarreduraDto.De(feita));
    }

    /// <summary>
    /// O Risco Cidadão da última varredura, de 0 a 100, com a composição por categoria e por destino, o da
    /// varredura anterior (o antes) e o histórico das últimas 10.
    /// </summary>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O indicador, o antes e o histórico.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    /// <response code="404">Nenhuma varredura ainda.</response>
    [HttpGet("risco-cidadao")]
    [ExigeChaveAdmin]
    [ProducesResponseType<PainelDeRiscoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> RiscoCidadao(CancellationToken ct)
    {
        var painel = await privacidade.PainelAsync(ct);
        return painel is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Nenhuma varredura ainda",
                detail: "Rode POST /api/privacidade/varreduras para calcular o Risco Cidadão.")
            : Ok(PainelDeRiscoDto.De(painel, configuracao.GetValue<bool>("Banco:SemearLeituras")));
    }

    /// <summary>Os dados pessoais expostos que uma varredura achou, já mascarados, com o alerta de cada tipo.</summary>
    /// <param name="varreduraId">A varredura; sem ele, a última.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Os achados.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    /// <response code="404">A varredura não existe ou nenhuma foi feita.</response>
    [HttpGet("achados")]
    [ExigeChaveAdmin]
    [ProducesResponseType<IReadOnlyList<AchadoDeVarreduraDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Achados([FromQuery] long? varreduraId, CancellationToken ct)
    {
        var achados = await privacidade.AchadosAsync(varreduraId, ct);
        return achados is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Varredura não encontrada",
                detail: varreduraId is null ? "Nenhuma varredura ainda." : $"Não existe varredura com o id {varreduraId}.")
            : Ok(achados.Select(AchadoDeVarreduraDto.De).ToList());
    }

    /// <summary>
    /// Os fluxos de dado do cidadão que chegaram a um guarda de saída (log, resposta pública, relato), do mais novo
    /// para o mais velho. Só a categoria e o destino, nunca o texto.
    /// </summary>
    /// <param name="limite">Quantos, de 1 a 500. Padrão: 100.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">Os fluxos.</response>
    /// <response code="400">Limite fora de 1 a 500.</response>
    /// <response code="401">Sem o cabeçalho X-Admin-Key ou com a chave errada.</response>
    [HttpGet("fluxos")]
    [ExigeChaveAdmin]
    [ProducesResponseType<IReadOnlyList<FluxoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> Fluxos([FromQuery] int limite = 100, CancellationToken ct = default)
    {
        if (limite is < 1 or > 500)
        {
            ModelState.AddModelError(nameof(limite), "O limite vai de 1 a 500.");
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);
        }

        var fluxos = await privacidade.FluxosAsync(limite, ct);
        return Ok(fluxos.Select(FluxoDto.De).ToList());
    }
}

/// <summary>O texto a analisar. Entra como não confiável e só sai sanitizado.</summary>
public sealed record TextoParaAnaliseDto([Required] [TamanhoDoTexto(2000)] TextoNaoConfiavel? Texto);

/// <param name="TemDadoPessoal">Se o texto tinha algum dado pessoal.</param>
/// <param name="Achados">O que seria removido: categoria e trecho mascarado (nunca o original).</param>
/// <param name="TextoSanitizado">O texto como seria gravado.</param>
public sealed record AnaliseDto(bool TemDadoPessoal, IReadOnlyList<AchadoDto> Achados, TextoSanitizado TextoSanitizado);

/// <param name="Categoria">CPF, DOCUMENTO, TELEFONE, EMAIL, SAUDE, SEGREDO, LOCALIZACAO_PRECISA ou QUASE_IDENTIFICADOR.</param>
/// <param name="Trecho">O trecho mascarado, como 529.***.***-25 ou &lt;SAUDE&gt;.</param>
public sealed record AchadoDto(CategoriaPii Categoria, string Trecho)
{
    public static AchadoDto De(AchadoMascarado a) => new(a.Categoria, a.Trecho);
}

/// <summary>Uma varredura feita.</summary>
/// <param name="Id">O id da varredura.</param>
/// <param name="ConcluidaEm">Quando terminou, no horário de São Paulo.</param>
/// <param name="Risco">O Risco Cidadão (0 a 100), a faixa e a composição.</param>
/// <param name="Fontes">Como foi cada fonte: LOGS, RELATOS, MQTT e CODIGO.</param>
/// <param name="Achados">Quantos dados pessoais estavam expostos.</param>
/// <param name="Fluxos">Quantos fluxos dos últimos dias entraram no cálculo.</param>
/// <param name="Alertas">Um alerta por tipo de achado, para o cidadão e para a equipe.</param>
public sealed record VarreduraDto(long Id, DateTimeOffset ConcluidaEm, RiscoCidadao Risco,
    IReadOnlyList<SituacaoDaFonte> Fontes, int Achados, int Fluxos, IReadOnlyList<AlertaDto> Alertas)
{
    public static VarreduraDto De(VarreduraFeita v) => new(v.Id, HorarioDeSaoPaulo.Converter(v.ConcluidaEmUtc), v.Risco,
        v.Fontes, v.Achados, v.Fluxos, v.Alertas.Select(AlertaDto.De).ToList());
}

/// <param name="Categoria">O tipo de dado.</param>
/// <param name="Destino">LOG, RESPOSTA_PUBLICA, LLM, BROKER_MQTT, BANCO ou ARQUIVO.</param>
/// <param name="Mitigado">Se um guarda barrou o dado.</param>
/// <param name="Ocorrencias">Quantas vezes apareceu.</param>
/// <param name="ParaCidadao">O alerta em linguagem literal.</param>
/// <param name="ParaEquipe">A correção sugerida no código.</param>
/// <param name="GeradoPor">LLM ou MODELO (frase pronta).</param>
/// <param name="Motivo">Por que caiu na frase pronta, se caiu.</param>
public sealed record AlertaDto(CategoriaPii Categoria, DestinoFluxo Destino, bool Mitigado, int Ocorrencias,
    string ParaCidadao, string ParaEquipe, OrigemDoAlerta GeradoPor, string? Motivo)
{
    public static AlertaDto De(AlertaDoPainel a) => new(a.Categoria, a.Destino, a.Mitigado, a.Ocorrencias,
        a.Alerta.ParaCidadao, a.Alerta.ParaEquipe, a.Alerta.GeradoPor, a.Alerta.Motivo);
}

/// <summary>O painel: a varredura atual, a anterior e o histórico.</summary>
/// <param name="Atual">A última varredura.</param>
/// <param name="Antes">A varredura anterior, para o antes e depois.</param>
/// <param name="Historico">As últimas 10, da mais velha para a mais nova.</param>
/// <param name="DadosDeExemplo">Se a API roda com as leituras simuladas do seed (o painel mostra o selo).</param>
public sealed record PainelDeRiscoDto(VarreduraDto Atual, MarcoDeRiscoDto? Antes, IReadOnlyList<MarcoDeRiscoDto> Historico,
    bool DadosDeExemplo)
{
    public static PainelDeRiscoDto De(PainelDeRisco p, bool dadosDeExemplo) => new(VarreduraDto.De(p.Atual),
        p.Antes is null ? null : MarcoDeRiscoDto.De(p.Antes), p.Historico.Select(MarcoDeRiscoDto.De).ToList(),
        dadosDeExemplo);
}

/// <summary>Um ponto do histórico do Risco Cidadão.</summary>
public sealed record MarcoDeRiscoDto(long VarreduraId, DateTimeOffset Em, int Indice, FaixaRisco Faixa)
{
    public static MarcoDeRiscoDto De(MarcoDeRisco m) =>
        new(m.VarreduraId, HorarioDeSaoPaulo.Converter(m.EmUtc), m.Indice, m.Faixa);
}

/// <summary>Um dado pessoal exposto achado na varredura. Só o trecho mascarado.</summary>
public sealed record AchadoDeVarreduraDto(long Id, long VarreduraId, string Fonte, DestinoFluxo Destino, string Local,
    string Regra, CategoriaPii Categoria, int Peso, string Trecho, string? AlertaParaCidadao, string? CorrecaoSugerida,
    OrigemDoAlerta? AlertaGeradoPor)
{
    public static AchadoDeVarreduraDto De(AchadoRegistrado a) => new(a.Id, a.VarreduraId, a.Fonte, a.Destino, a.Local,
        a.RegraId, a.Categoria, a.Peso, a.Trecho, a.Alerta?.ParaCidadao, a.Alerta?.ParaEquipe, a.Alerta?.GeradoPor);
}

/// <summary>Um fluxo de dado do cidadão que chegou a um guarda.</summary>
public sealed record FluxoDto(string Origem, DestinoFluxo Destino, CategoriaPii Categoria, int Peso, bool Bloqueado,
    DateTimeOffset OcorridoEm)
{
    public static FluxoDto De(EventoDeFluxo e) =>
        new(e.Origem, e.Destino, e.Categoria, e.Peso, e.Bloqueado, HorarioDeSaoPaulo.Converter(e.OcorridoEmUtc));
}
