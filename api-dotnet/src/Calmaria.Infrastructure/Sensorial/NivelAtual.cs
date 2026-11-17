using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Sensorial;

/// <summary>O "agora" de cada lugar: a leitura mais recente dos últimos 15 minutos.</summary>
internal static class NivelAtual
{
    /// <summary>Leitura mais velha que isso não conta como "agora".</summary>
    public static readonly TimeSpan Janela = TimeSpan.FromMinutes(15);

    /// <summary>A leitura mais recente de cada lugar na janela (GroupBy com First vira ROW_NUMBER no banco).</summary>
    public static Task<Dictionary<int, Leitura>> UltimasPorLugarAsync(CalmariaDbContext db, TimeProvider relogio,
        CancellationToken ct)
    {
        var desde = relogio.GetUtcNow().UtcDateTime - Janela;
        return db.Leituras.AsNoTracking()
            .Where(x => x.RegistradaEm >= desde)
            .GroupBy(x => x.LugarId)
            .Select(g => g.OrderByDescending(x => x.RegistradaEm).First())
            .ToDictionaryAsync(x => x.LugarId, ct);
    }
}
