using System.ComponentModel.DataAnnotations;
using Calmaria.Api.Comum;
using Calmaria.Api.Privacidade;
using Calmaria.Domain.Comum;
using Calmaria.Domain.Comunidade;
using Calmaria.Privacidade.Taint;
using Microsoft.AspNetCore.Mvc;

namespace Calmaria.Api.Comunidade;

/// <summary>Relatos de visita: o que a comunidade conta sobre cada lugar.</summary>
[ApiController]
[Route("api/relatos")]
[Tags("Comunidade")]
public sealed class RelatosController(IRelatoService relatos) : ControllerBase
{
    /// <summary>
    /// Grava um relato. O comentário passa pelo filtro de privacidade antes de ser gravado; a resposta diz o que
    /// foi tirado.
    /// </summary>
    /// <param name="corpo">O relato.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="201">O relato, com o comentário como foi gravado e o que foi removido.</response>
    /// <response code="400">Nota fora de 1 a 5, gatilho desconhecido, visita no futuro ou comentário longo.</response>
    /// <response code="404">Usuário ou lugar que não existe.</response>
    /// <response code="409">O mesmo usuário já relatou este lugar nos últimos 30 minutos.</response>
    [HttpPost]
    [ProducesResponseType<RelatoCriadoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<IActionResult> Criar([FromBody] NovoRelatoDto corpo, CancellationToken ct)
    {
        var criado = await relatos.CriarAsync(new NovoRelato(corpo.UsuarioId!.Value, corpo.LugarId!.Value,
            corpo.Nota!.Value, corpo.Gatilhos ?? [], corpo.Comentario, corpo.VisitadoEm!.Value.UtcDateTime), ct);
        return Created($"/api/relatos?lugarId={corpo.LugarId}",
            new RelatoCriadoDto(criado.Id, criado.Comentario, criado.Removidos.Select(AchadoDto.De).ToList()));
    }

    /// <summary>Os relatos públicos, do mais novo para o mais velho, sem o autor.</summary>
    /// <param name="lugarId">Filtra por lugar (opcional).</param>
    /// <param name="pagina">Página, a partir de 1.</param>
    /// <param name="tamanho">Itens por página, de 1 a 50. Padrão: 10.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">A página de relatos.</response>
    /// <response code="400">Página ou tamanho inválido.</response>
    [HttpGet]
    [FiltroRespostaPublica]
    [ProducesResponseType<PaginaDto<RelatoPublicoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Listar([FromQuery] int? lugarId, [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10, CancellationToken ct = default)
    {
        if (pagina < 1)
            ModelState.AddModelError(nameof(pagina), "A página começa em 1.");
        if (tamanho is < 1 or > 50)
            ModelState.AddModelError(nameof(tamanho), "O tamanho vai de 1 a 50.");
        if (!ModelState.IsValid)
            return ValidationProblem(title: ErrosDaApi.TituloDadosInvalidos);

        var resultado = await relatos.ListarPublicosAsync(lugarId, pagina, tamanho, ct);
        return Ok(new PaginaDto<RelatoPublicoDto>(resultado.Numero, resultado.Tamanho, resultado.Total,
            resultado.Itens.Select(RelatoPublicoDto.De).ToList()));
    }

    /// <summary>A nota média e os gatilhos mais citados nos relatos do lugar.</summary>
    /// <param name="id">O id do lugar.</param>
    /// <param name="ct">Cancelamento da requisição.</param>
    /// <response code="200">O resumo; sem relatos, nota média nula e lista vazia.</response>
    /// <response code="404">O lugar não existe.</response>
    [HttpGet("~/api/lugares/{id:int}/resumo-relatos")]
    [FiltroRespostaPublica]
    [ProducesResponseType<ResumoRelatosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Resumo(int id, CancellationToken ct)
    {
        var resumo = await relatos.ResumirAsync(id, ct);
        return resumo is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Lugar não encontrado",
                detail: $"Não existe lugar com o id {id}.")
            : Ok(new ResumoRelatosDto(resumo.LugarId, resumo.TotalRelatos, resumo.NotaMedia,
                resumo.GatilhosMaisCitados.Select(g => new GatilhoCitadoDto(g.Gatilho, g.Vezes)).ToList()));
    }
}

/// <summary>Um relato de visita.</summary>
/// <param name="UsuarioId">O id do usuário anônimo.</param>
/// <param name="LugarId">O lugar visitado.</param>
/// <param name="Nota">De 1 (insuportável) a 5 (muito calmo).</param>
/// <param name="Gatilhos">BARULHO, LUZ_FORTE, MULTIDAO, CHEIRO, FILA ou OUTRO.</param>
/// <param name="Comentario">Opcional, até 500 caracteres. Dado pessoal é removido antes de gravar.</param>
/// <param name="VisitadoEm">Quando foi a visita, com fuso. Não pode estar no futuro.</param>
public sealed record NovoRelatoDto(
    [Required] Guid? UsuarioId,
    [Required] int? LugarId,
    [Required] [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")] int? Nota,
    IReadOnlyList<Gatilho>? Gatilhos,
    [TamanhoDoTexto(Relato.TamanhoMaximoDoComentario)] TextoNaoConfiavel? Comentario,
    [Required] DateTimeOffset? VisitadoEm);

/// <param name="Id">O id do relato.</param>
/// <param name="Comentario">O comentário como foi gravado.</param>
/// <param name="Removidos">O que o filtro de privacidade tirou (vazio se nada).</param>
public sealed record RelatoCriadoDto(long Id, TextoSanitizado? Comentario, IReadOnlyList<AchadoDto> Removidos);

/// <summary>Um relato público. Só aceita comentário sanitizado e não tem o autor.</summary>
public sealed record RelatoPublicoDto(long Id, int Nota, IReadOnlyList<Gatilho> Gatilhos, TextoSanitizado? Comentario,
    DateTimeOffset VisitadoEm)
{
    public static RelatoPublicoDto De(RelatoPublico r) =>
        new(r.Id, r.Nota, r.Gatilhos, r.Comentario, HorarioDeSaoPaulo.Converter(r.VisitadoEm));
}

public sealed record PaginaDto<T>(int Pagina, int Tamanho, int Total, IReadOnlyList<T> Itens);

public sealed record ResumoRelatosDto(int LugarId, int TotalRelatos, double? NotaMedia, IReadOnlyList<GatilhoCitadoDto> GatilhosMaisCitados);

public sealed record GatilhoCitadoDto(Gatilho Gatilho, int Vezes);
