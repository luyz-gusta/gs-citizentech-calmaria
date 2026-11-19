using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Application.Sensorial;

/// <summary>Busca as leituras dos últimos 28 dias e entrega o cálculo à regra do domínio.</summary>
internal sealed class HorarioCalmoService(CalmariaDbContext db, TimeProvider relogio) : IHorarioCalmoService
{
    public const int DiasAnalisados = 28;

    public async Task<HorariosCalmos?> CalcularAsync(int lugarId, CancellationToken ct = default)
    {
        var lugar = await db.Lugares.AsNoTracking()
            .Where(l => l.Id == lugarId)
            .Select(l => new { l.HorarioFuncionamento })
            .FirstOrDefaultAsync(ct);
        if (lugar is null)
            return null;

        var desde = relogio.GetUtcNow().UtcDateTime.AddDays(-DiasAnalisados);
        var leituras = await db.Leituras.AsNoTracking()
            .Where(x => x.LugarId == lugarId && x.RegistradaEm >= desde)
            .Select(x => new IndiceNoTempo(x.RegistradaEm, x.Indice))
            .ToListAsync(ct);

        return CalculadoraHorariosCalmos.Calcular(lugarId, leituras,
            HorarioDeFuncionamento.Ler(lugar.HorarioFuncionamento));
    }
}
