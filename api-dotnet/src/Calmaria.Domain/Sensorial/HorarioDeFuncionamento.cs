using System.Globalization;
using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>
/// A faixa em que o lugar funciona, no formato "HH:mm-HH:mm" do horário de São Paulo. "00:00" no fim quer dizer
/// meia-noite; fim antes do começo atravessa a meia-noite (ex.: "22:00-02:00"). Sem horário, conta o dia todo.
/// </summary>
public sealed class HorarioDeFuncionamento
{
    public static readonly HorarioDeFuncionamento DiaTodo = new(TimeSpan.Zero, TimeSpan.FromHours(24));

    public TimeSpan Abre { get; }
    public TimeSpan Fecha { get; }

    private HorarioDeFuncionamento(TimeSpan abre, TimeSpan fecha)
    {
        Abre = abre;
        Fecha = fecha;
    }

    public static HorarioDeFuncionamento Ler(string? faixa)
    {
        if (string.IsNullOrWhiteSpace(faixa))
            return DiaTodo;

        var partes = faixa.Split('-', StringSplitOptions.TrimEntries);
        if (partes.Length != 2 || !TentarHora(partes[0], out var abre) || !TentarHora(partes[1], out var fecha))
            throw new DominioException($"Horário de funcionamento inválido: \"{faixa}\". Use HH:mm-HH:mm.");

        if (fecha == TimeSpan.Zero)
            fecha = TimeSpan.FromHours(24);
        return new HorarioDeFuncionamento(abre, fecha);
    }

    /// <summary>
    /// Se a hora cheia (de h:00 a h:59) conta como horário de funcionamento: o lugar precisa estar aberto no
    /// meio dela (h:30). Um lugar que abre às 4h40 começa a contar na hora das 5.
    /// </summary>
    public bool Inclui(int hora)
    {
        var meio = TimeSpan.FromHours(hora) + TimeSpan.FromMinutes(30);
        return Abre <= Fecha
            ? meio >= Abre && meio < Fecha
            : meio >= Abre || meio < Fecha;
    }

    private static bool TentarHora(string texto, out TimeSpan hora) =>
        TimeSpan.TryParseExact(texto, @"hh\:mm", CultureInfo.InvariantCulture, out hora) && hora < TimeSpan.FromHours(24);
}
