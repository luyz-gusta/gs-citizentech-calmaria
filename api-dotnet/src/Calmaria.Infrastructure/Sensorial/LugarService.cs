using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Sensorial;

/// <summary>Consultas e cadastro de lugares do módulo Sensorial.</summary>
internal sealed class LugarService(CalmariaDbContext db, TimeProvider relogio) : ILugarService, ILugarConsulta, ILugarCadastro
{
    public async Task<LugarDetalhe> CriarAsync(DadosDoLugar dados, CancellationToken ct = default)
    {
        var coordenada = new Coordenada(dados.Latitude, dados.Longitude);
        Lugar lugar = dados.Tipo == TipoLugar.Refugio
            ? new Refugio(0, dados.Nome, coordenada, dados.Distrito, Limpo(dados.Descricao), dados.Endereco,
                dados.HorarioFuncionamento)
            : new LugarMonitorado(0, dados.Nome, dados.Tipo, coordenada, dados.Distrito, dados.Endereco,
                dados.HorarioFuncionamento);
        if (lugar is LugarMonitorado && !string.IsNullOrWhiteSpace(dados.Descricao))
            throw new DominioException("Só refúgio tem descrição.");

        db.Lugares.Add(lugar);
        await db.SaveChangesAsync(ct);
        return (await ObterAsync(lugar.Id, ct))!;
    }

    public async Task<LugarDetalhe?> AtualizarAsync(int id, DadosDoLugar dados, CancellationToken ct = default)
    {
        var lugar = await db.Lugares.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lugar is null)
            return null;
        if ((dados.Tipo == TipoLugar.Refugio) != lugar.EhRefugio)
            throw new ConflitoException("Troca de categoria",
                "Refúgio e lugar monitorado são cadastros diferentes. Apague este lugar e cadastre de novo.");

        var coordenada = new Coordenada(dados.Latitude, dados.Longitude);
        switch (lugar)
        {
            case Refugio refugio:
                refugio.Atualizar(dados.Nome, coordenada, dados.Distrito, dados.Descricao, dados.Endereco,
                    dados.HorarioFuncionamento);
                break;
            case LugarMonitorado monitorado:
                if (!string.IsNullOrWhiteSpace(dados.Descricao))
                    throw new DominioException("Só refúgio tem descrição.");
                monitorado.Atualizar(dados.Nome, dados.Tipo, coordenada, dados.Distrito, dados.Endereco,
                    dados.HorarioFuncionamento);
                break;
        }

        await db.SaveChangesAsync(ct);
        return await ObterAsync(id, ct);
    }

    public async Task<bool> ApagarAsync(int id, CancellationToken ct = default)
    {
        var lugar = await db.Lugares.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lugar is null)
            return false;

        // O histórico do lugar (leituras do sensor e relatos da comunidade) não some por um clique.
        if (await db.Dispositivos.AnyAsync(d => d.LugarId == id, ct) || await db.Relatos.AnyAsync(r => r.LugarId == id, ct))
            throw new ConflitoException("Lugar com histórico",
                $"O lugar {id} tem sensor ou relatos e não pode ser apagado.");

        db.Lugares.Remove(lugar);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string? Limpo(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

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

        var ultimas = await NivelAtual.UltimasPorLugarAsync(db, relogio, ct);

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
