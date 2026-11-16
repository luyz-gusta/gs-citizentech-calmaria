using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>Dia da semana no contrato (DOMINGO a SABADO).</summary>
public enum DiaSemana
{
    Domingo,
    Segunda,
    Terca,
    Quarta,
    Quinta,
    Sexta,
    Sabado
}

/// <summary>A média do índice numa hora de um dia da semana.</summary>
public sealed record JanelaCalma(DiaSemana DiaSemana, int Hora, int IndiceMedio);

/// <summary>As 3 janelas mais calmas e a grade de médias de um lugar.</summary>
public sealed record HorariosCalmos(int LugarId, bool Suficiente, IReadOnlyList<JanelaCalma> JanelasMaisCalmas,
    IReadOnlyList<JanelaCalma> Grade);

/// <summary>Uma leitura reduzida ao que o cálculo precisa.</summary>
public readonly record struct IndiceNoTempo(DateTime RegistradaEmUtc, int Indice);

/// <summary>
/// Agrupa as leituras por dia da semana e hora (no horário de São Paulo), tira a média do índice e escolhe as
/// janelas mais calmas dentro do horário de funcionamento.
/// </summary>
public static class CalculadoraHorariosCalmos
{
    public const int MinimoDeLeituras = 20;
    public const int QuantidadeDeJanelas = 3;

    public static HorariosCalmos Calcular(int lugarId, IReadOnlyCollection<IndiceNoTempo> leituras,
        HorarioDeFuncionamento funcionamento)
    {
        if (leituras.Count < MinimoDeLeituras)
            return new HorariosCalmos(lugarId, Suficiente: false, [], []);

        // DayOfWeek começa no domingo (0), como o DiaSemana.
        var grade = leituras
            .Select(l => (Local: HorarioDeSaoPaulo.Converter(l.RegistradaEmUtc), l.Indice))
            .GroupBy(l => (Dia: (DiaSemana)(int)l.Local.DayOfWeek, l.Local.Hour))
            .Select(g => new JanelaCalma(g.Key.Dia, g.Key.Hour,
                (int)Math.Round(g.Average(l => l.Indice), MidpointRounding.AwayFromZero)))
            .OrderBy(j => j.DiaSemana)
            .ThenBy(j => j.Hora)
            .ToList();

        var maisCalmas = grade
            .Where(j => funcionamento.Inclui(j.Hora))
            .OrderBy(j => j.IndiceMedio)
            .ThenBy(j => j.DiaSemana)
            .ThenBy(j => j.Hora)
            .Take(QuantidadeDeJanelas)
            .ToList();

        return new HorariosCalmos(lugarId, Suficiente: true, maisCalmas, grade);
    }
}
