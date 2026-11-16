using System.ComponentModel.DataAnnotations;
using Calmaria.Api.Comum;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Privacidade;

/// <summary>O módulo de privacidade: o que o app consulta antes de enviar um texto do cidadão.</summary>
[ApiController]
[Route("api/privacidade")]
[Tags("Privacidade")]
public sealed class PrivacidadeController(ISanitizador sanitizador) : ControllerBase
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
