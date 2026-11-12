using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Sensorial;

/// <summary>Consultas de lugares do módulo Sensorial.</summary>
internal sealed class LugarService(CalmariaDbContext db, TimeProvider relogio) : ILugarService, ILugarConsulta
{
    /// <summary>Leitura mais velha que isso não conta como "agora": o lugar aparece sem nível atual.</summary>
    private static readonly TimeSpan JanelaDoNivelAtual = TimeSpan.FromMinutes(15);

    public Task<bool> ExisteAsync(int lugarId, CancellationToken ct = default) =>
        db.Lugares.AnyAsync(l => l.Id == lugarId, ct);

    public async Task<IReadOnlyList<LugarResumo>> ListarAsync(FiltroLugares filtro, CancellationToken ct = default)
    {
        var consulta = db.Lugares.AsNoTracking();
        if (filtro.Tipo is { } tipo)
            consulta = consulta.Where(l => l.Tipo == tipo);

        var lugares = await consulta
            .OrderBy(l => l.Id)
            .Select(l => new { l.Id, l.Nome, l.Tipo, l.Latitude, l.Longitude, l.Distrito })
            .ToListAsync(ct);

        // A leitura mais recente de cada lugar nos últimos 15 minutos (GroupBy com First vira ROW_NUMBER no banco).
        var desde = relogio.GetUtcNow().UtcDateTime - JanelaDoNivelAtual;
        var ultimas = await db.Leituras.AsNoTracking()
            .Where(x => x.RegistradaEm >= desde)
            .GroupBy(x => x.LugarId)
            .Select(g => g.OrderByDescending(x => x.RegistradaEm).First())
            .ToDictionaryAsync(x => x.LugarId, ct);

        var resumos = lugares.Select(l =>
        {
            int? distancia = filtro.Origem is null
                ? null
                : (int)Math.Round(new Coordenada(l.Latitude, l.Longitude).DistanciaEmMetrosAte(filtro.Origem));
            var ultima = ultimas.GetValueOrDefault(l.Id);
            return new LugarResumo(l.Id, l.Nome, l.Tipo, l.Latitude, l.Longitude, l.Distrito,
                EhRefugio: l.Tipo == TipoLugar.Refugio,
                DistanciaM: distancia,
                IndiceAtual: ultima?.Indice,
                NivelAtual: ultima?.Nivel,
                UltimaLeituraEm: ultima?.RegistradaEm);
        });

        if (filtro.Origem is not null)
            resumos = resumos.Where(r => r.DistanciaM <= filtro.RaioM).OrderBy(r => r.DistanciaM);

        return resumos.ToList();
    }

    public async Task<LugarDetalhe?> ObterAsync(int id, CancellationToken ct = default)
    {
        var lugar = await db.Lugares.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lugar is null)
            return null;

        var ultima = await db.Leituras.AsNoTracking()
            .Where(x => x.LugarId == id)
            .OrderByDescending(x => x.RegistradaEm)
            .Select(x => new LeituraResumo(x.SomDb, x.LuzLux, x.MovimentosMin, x.Indice, x.Nivel, x.RegistradaEm))
            .FirstOrDefaultAsync(ct);

        return new LugarDetalhe(lugar.Id, lugar.Nome, lugar.Tipo, lugar.Endereco, lugar.Latitude, lugar.Longitude,
            lugar.Distrito, lugar.EhRefugio, (lugar as Refugio)?.Descricao, lugar.HorarioFuncionamento, ultima);
    }
}
