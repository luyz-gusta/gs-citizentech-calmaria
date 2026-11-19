using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Application.Sensorial;

/// <summary>O refúgio do botão de crise: um refúgio fixo ou um lugar CALMO agora, o que estiver mais perto.</summary>
internal sealed class RefugioService(CalmariaDbContext db, TimeProvider relogio) : IRefugioService
{
    /// <summary>Meia-largura do retângulo de busca no banco, em graus (uns 5,5 km em São Paulo).</summary>
    private const double MargemEmGraus = 0.05;

    public async Task<RefugioEscolhido?> MaisProximoAsync(Coordenada origem, CancellationToken ct = default)
    {
        // Primeiro o retângulo em volta da pessoa, filtrado no banco; a distância exata (Haversine) é em memória.
        // Se não houver nada perto, procura em todos: na crise, um refúgio longe é melhor que nenhum.
        var candidatos = await CandidatosAsync(origem, MargemEmGraus, ct);
        if (candidatos.Count == 0)
            candidatos = await CandidatosAsync(origem, margem: null, ct);
        return EscolhaDeRefugio.MaisProximo(origem, candidatos);
    }

    public async Task<IReadOnlyList<RefugioCandidato>> ListarFixosAsync(CancellationToken ct = default) =>
        await db.Lugares.AsNoTracking()
            .OfType<Refugio>()
            .OrderBy(r => r.Id)
            .Select(r => new RefugioCandidato(r.Id, r.Nome, r.Tipo, r.Latitude, r.Longitude, r.Descricao))
            .ToListAsync(ct);

    private async Task<List<RefugioCandidato>> CandidatosAsync(Coordenada origem, double? margem, CancellationToken ct)
    {
        var lugares = db.Lugares.AsNoTracking();
        if (margem is { } m)
            lugares = lugares.Where(l =>
                l.Latitude >= origem.Latitude - m && l.Latitude <= origem.Latitude + m &&
                l.Longitude >= origem.Longitude - m && l.Longitude <= origem.Longitude + m);

        var ultimas = await NivelAtual.UltimasPorLugarAsync(db, relogio, ct);
        var calmosAgora = ultimas.Values.Where(x => x.Nivel == NivelSobrecarga.Calmo).Select(x => x.LugarId).ToList();

        return await lugares
            .Where(l => l.Tipo == TipoLugar.Refugio || calmosAgora.Contains(l.Id))
            .Select(l => new RefugioCandidato(l.Id, l.Nome, l.Tipo, l.Latitude, l.Longitude,
                l is Refugio ? ((Refugio)l).Descricao : null))
            .ToListAsync(ct);
    }
}
