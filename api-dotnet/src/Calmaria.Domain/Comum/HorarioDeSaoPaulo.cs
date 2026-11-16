namespace Calmaria.Domain.Comum;

/// <summary>
/// O banco guarda tudo em UTC. A resposta da API sai no horário de São Paulo, com o fuso (-03:00), e os
/// horários calmos são agrupados pela hora local, que é a que a pessoa vive.
/// </summary>
public static class HorarioDeSaoPaulo
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTimeOffset Converter(DateTime utc) =>
        TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), Fuso);

    public static DateTimeOffset? Converter(DateTime? utc) => utc is null ? null : Converter(utc.Value);
}
