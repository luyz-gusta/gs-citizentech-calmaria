using Calmaria.Domain.Comum;
using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Privacidade.Taint;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Comunidade;

/// <summary>
/// Relatos de visita. O lugar é conferido por <see cref="ILugarConsulta"/> (a porta do módulo Sensorial), e o
/// comentário só é gravado depois do <see cref="ISanitizador"/>.
/// </summary>
internal sealed class RelatoService(
    CalmariaDbContext db,
    ILugarConsulta lugares,
    ISanitizador sanitizador,
    IRastreadorDeFluxo rastreador,
    TimeProvider relogio) : IRelatoService
{
    private static readonly TimeSpan IntervaloMinimo = TimeSpan.FromMinutes(30);
    public const int TamanhoMaximoDaPagina = 50;

    public async Task<RelatoCriado> CriarAsync(NovoRelato novo, CancellationToken ct = default)
    {
        var nota = new NotaSensorial(novo.Nota);
        if (novo.Comentario is { } texto && texto.Tamanho > Relato.TamanhoMaximoDoComentario)
            throw new DominioException($"O comentário passa de {Relato.TamanhoMaximoDoComentario} caracteres.");

        if (!await db.Usuarios.AnyAsync(u => u.Id == novo.UsuarioId, ct))
            throw UsuarioService.UsuarioNaoEncontrado(novo.UsuarioId);
        if (!await lugares.ExisteAsync(novo.LugarId, ct))
            throw new RecursoNaoEncontradoException("Lugar não encontrado", $"Não existe lugar com o id {novo.LugarId}.");

        var agora = relogio.GetUtcNow().UtcDateTime;
        var desde = agora - IntervaloMinimo;
        if (await db.Relatos.AnyAsync(r => r.UsuarioId == novo.UsuarioId && r.LugarId == novo.LugarId && r.CriadoEm >= desde, ct))
            throw new ConflitoException("Relato repetido",
                "Você já relatou este lugar nos últimos 30 minutos. Tente de novo mais tarde.");

        var sanitizado = novo.Comentario is null ? null : sanitizador.Sanitizar(novo.Comentario);
        if (sanitizado is { TinhaDadoPessoal: true })
        {
            // O comentário vai para a listagem pública: o que saiu dele é um fluxo bloqueado para o painel.
            rastreador.Registrar("relato.comentario", DestinoFluxo.RespostaPublica, sanitizado.Removidos, bloqueado: true);
        }

        var relato = Relato.Criar(novo.UsuarioId, novo.LugarId, nota, novo.Gatilhos, sanitizado?.Texto,
            novo.VisitadoEmUtc, agora);
        db.Relatos.Add(relato);
        await db.SaveChangesAsync(ct);

        return new RelatoCriado(relato.Id, relato.Comentario, sanitizado?.Removidos ?? []);
    }

    public async Task<Pagina<RelatoPublico>> ListarPublicosAsync(int? lugarId, int pagina, int tamanho,
        CancellationToken ct = default)
    {
        var consulta = db.Relatos.AsNoTracking();
        if (lugarId is { } id)
            consulta = consulta.Where(r => r.LugarId == id);

        var total = await consulta.CountAsync(ct);
        var relatos = await consulta
            .OrderByDescending(r => r.CriadoEm)
            .ThenByDescending(r => r.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(ct);

        // Sem o autor: nem id de usuário nem apelido saem na listagem pública.
        var itens = relatos.Select(r => new RelatoPublico(r.Id, r.Nota,
            r.Gatilhos.Select(g => g.Gatilho).ToList(), r.Comentario, r.VisitadoEm)).ToList();
        return new Pagina<RelatoPublico>(pagina, tamanho, total, itens);
    }

    public async Task<ResumoRelatos?> ResumirAsync(int lugarId, CancellationToken ct = default)
    {
        if (!await lugares.ExisteAsync(lugarId, ct))
            return null;

        var doLugar = db.Relatos.AsNoTracking().Where(r => r.LugarId == lugarId);
        var total = await doLugar.CountAsync(ct);
        if (total == 0)
            return new ResumoRelatos(lugarId, 0, null, []);

        var media = await doLugar.AverageAsync(r => r.Nota, ct);
        var gatilhos = await doLugar
            .SelectMany(r => r.Gatilhos)
            .GroupBy(g => g.Gatilho)
            .Select(g => new GatilhoCitado(g.Key, g.Count()))
            .ToListAsync(ct);

        return new ResumoRelatos(lugarId, total, Math.Round(media, 1, MidpointRounding.AwayFromZero),
            gatilhos.OrderByDescending(g => g.Vezes).ThenBy(g => g.Gatilho).ToList());
    }
}
