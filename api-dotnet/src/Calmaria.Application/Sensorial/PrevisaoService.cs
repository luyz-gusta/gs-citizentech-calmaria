using System.Globalization;
using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Application.Sensorial;

/// <summary>Importa o CSV do notebook de ML e consulta as previsões por distrito.</summary>
internal sealed class PrevisaoService(CalmariaDbContext db) : IPrevisaoService
{
    public async Task<int> ImportarAsync(Stream csv, CancellationToken ct = default)
    {
        var linhas = await LeitorCsvPrevisoes.LerAsync(csv, ct);
        var distritos = linhas.Select(l => l.Distrito).Distinct().ToList();
        var datas = linhas.Select(l => l.Data).Distinct().ToList();

        var existentes = await db.PrevisoesBarulho
            .Where(p => distritos.Contains(p.Distrito) && datas.Contains(p.Data))
            .ToDictionaryAsync(p => (p.Distrito, p.Data), ct);

        foreach (var linha in linhas)
        {
            if (existentes.TryGetValue((linha.Distrito, linha.Data), out var previsao))
                previsao.Atualizar(linha.ReclamacoesPrevistas, linha.Nivel);
            else
                db.PrevisoesBarulho.Add(new PrevisaoBarulho(linha.Distrito, linha.Data, linha.ReclamacoesPrevistas, linha.Nivel));
        }

        await db.SaveChangesAsync(ct);
        return linhas.Count;
    }

    public async Task<IReadOnlyList<PrevisaoBarulho>?> ConsultarAsync(string distrito, DateOnly inicio, int dias,
        CancellationToken ct = default)
    {
        var chave = Distrito.Normalizar(distrito);
        if (!await db.PrevisoesBarulho.AnyAsync(p => p.Distrito == chave, ct))
            return null;

        var fim = inicio.AddDays(dias);
        return await db.PrevisoesBarulho.AsNoTracking()
            .Where(p => p.Distrito == chave && p.Data >= inicio && p.Data < fim)
            .OrderBy(p => p.Data)
            .ToListAsync(ct);
    }
}

/// <summary>Uma linha válida do CSV de previsões.</summary>
public sealed record LinhaPrevisao(string Distrito, DateOnly Data, double ReclamacoesPrevistas, NivelPrevisao Nivel);

/// <summary>
/// Lê o CSV do contrato: cabeçalho <c>distrito,data,reclamacoes_previstas,nivel</c>, vírgula como separador,
/// ponto decimal, data AAAA-MM-DD. Qualquer linha errada recusa o arquivo inteiro, com o número da linha.
/// </summary>
public static class LeitorCsvPrevisoes
{
    public const string Cabecalho = "distrito,data,reclamacoes_previstas,nivel";
    public const int LinhasMaximas = 20_000;

    public static async Task<IReadOnlyList<LinhaPrevisao>> LerAsync(Stream csv, CancellationToken ct = default)
    {
        using var leitor = new StreamReader(csv);
        var cabecalho = (await leitor.ReadLineAsync(ct))?.Trim().TrimStart('﻿');
        if (!string.Equals(cabecalho, Cabecalho, StringComparison.OrdinalIgnoreCase))
            throw new DominioException($"O CSV precisa começar com o cabeçalho \"{Cabecalho}\".");

        var linhas = new List<LinhaPrevisao>();
        var numero = 1;
        while (await leitor.ReadLineAsync(ct) is { } texto)
        {
            numero++;
            if (string.IsNullOrWhiteSpace(texto))
                continue;
            if (linhas.Count >= LinhasMaximas)
                throw new DominioException($"O CSV passa de {LinhasMaximas} linhas.");
            linhas.Add(LerLinha(texto, numero));
        }

        if (linhas.Count == 0)
            throw new DominioException("O CSV não tem nenhuma previsão.");
        return linhas;
    }

    private static LinhaPrevisao LerLinha(string texto, int numero)
    {
        var campos = texto.Split(',', StringSplitOptions.TrimEntries);
        if (campos.Length != 4)
            throw new DominioException($"Linha {numero}: esperava 4 colunas e veio {campos.Length}.");
        if (string.IsNullOrWhiteSpace(campos[0]))
            throw new DominioException($"Linha {numero}: o distrito está vazio.");
        if (!DateOnly.TryParseExact(campos[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            throw new DominioException($"Linha {numero}: a data \"{campos[1]}\" não está no formato AAAA-MM-DD.");
        if (!double.TryParse(campos[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var reclamacoes) || reclamacoes < 0)
            throw new DominioException($"Linha {numero}: \"{campos[2]}\" não é um número de reclamações válido.");
        if (!Enum.TryParse<NivelPrevisao>(campos[3], ignoreCase: true, out var nivel) || !Enum.IsDefined(nivel))
            throw new DominioException($"Linha {numero}: o nível \"{campos[3]}\" deve ser BAIXO, MEDIO ou ALTO.");

        return new LinhaPrevisao(Distrito.Normalizar(campos[0]), data, reclamacoes, nivel);
    }
}
