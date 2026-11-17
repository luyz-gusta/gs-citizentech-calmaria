using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Sensorial;

/// <summary>Recebe as leituras do ESP32 (MQTT ou HTTP) e consulta o histórico.</summary>
internal sealed class LeituraService(CalmariaDbContext db, ICalculadoraIndice calculadora, TimeProvider relogio)
    : ILeituraService
{
    /// <summary>Teto do histórico numa resposta: um dia inteiro a cada 5 segundos dá 17.280; 2.000 basta para o app.</summary>
    public const int LimiteDoHistorico = 2_000;

    public async Task<LeituraResumo> RegistrarAsync(NovaLeitura nova, CancellationToken ct = default)
    {
        var dispositivo = await db.Dispositivos.FirstOrDefaultAsync(d => d.Id == nova.DispositivoId, ct)
            ?? throw new RecursoNaoEncontradoException("Dispositivo não encontrado",
                $"O dispositivo {nova.DispositivoId} não está cadastrado.");
        if (!dispositivo.Ativo)
            throw new DominioException($"O dispositivo {dispositivo.Id} está desativado.");
        if (dispositivo.LugarId != nova.LugarId)
            throw new DominioException(
                $"O dispositivo {dispositivo.Id} está instalado no lugar {dispositivo.LugarId}, não no {nova.LugarId}.");

        // O índice e o nível que vieram do ESP32 são só informativos: a API recalcula com a fórmula do contrato.
        var agora = relogio.GetUtcNow().UtcDateTime;
        var leitura = Leitura.Criar(dispositivo.Id, dispositivo.LugarId, nova.SomDb, nova.LuzLux, nova.MovimentosMin,
            calculadora, agora);
        dispositivo.RegistrarLeitura(agora);
        db.Leituras.Add(leitura);
        await db.SaveChangesAsync(ct);

        return new LeituraResumo(leitura.SomDb, leitura.LuzLux, leitura.MovimentosMin, leitura.Indice, leitura.Nivel,
            leitura.RegistradaEm);
    }

    public async Task<IReadOnlyList<LeituraResumo>?> ListarAsync(int lugarId, DateTime desdeUtc,
        CancellationToken ct = default)
    {
        if (!await db.Lugares.AnyAsync(l => l.Id == lugarId, ct))
            return null;

        var desde = DateTime.SpecifyKind(desdeUtc, DateTimeKind.Utc);
        return await db.Leituras.AsNoTracking()
            .Where(x => x.LugarId == lugarId && x.RegistradaEm >= desde)
            .OrderByDescending(x => x.RegistradaEm)
            .Take(LimiteDoHistorico)
            .Select(x => new LeituraResumo(x.SomDb, x.LuzLux, x.MovimentosMin, x.Indice, x.Nivel, x.RegistradaEm))
            .ToListAsync(ct);
    }
}
