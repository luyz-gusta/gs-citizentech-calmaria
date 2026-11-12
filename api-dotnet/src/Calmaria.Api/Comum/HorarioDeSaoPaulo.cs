namespace Calmaria.Api.Comum;

/// <summary>O banco guarda em UTC; a resposta sai no horário de São Paulo, com o fuso (-03:00).</summary>
public static class HorarioDeSaoPaulo
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static DateTimeOffset Converter(DateTime utc) =>
        TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), Fuso);

    public static DateTimeOffset? Converter(DateTime? utc) => utc is null ? null : Converter(utc.Value);
}
