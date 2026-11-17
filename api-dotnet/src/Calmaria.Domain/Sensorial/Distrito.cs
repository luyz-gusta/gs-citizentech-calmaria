using System.Globalization;
using System.Text;

namespace Calmaria.Domain.Sensorial;

/// <summary>
/// Distrito como na base do SP156: maiúsculas, sem acento e com um espaço só entre as palavras
/// ("Consolação" vira "CONSOLACAO"; "bela  vista", "BELA VISTA"). É a chave que liga o lugar à previsão.
/// </summary>
public static class Distrito
{
    public static string Normalizar(string distrito)
    {
        var semAcento = new StringBuilder(distrito.Length);
        foreach (var c in distrito.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(c);
        }

        var palavras = semAcento.ToString().Normalize(NormalizationForm.FormC)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(' ', palavras).ToUpperInvariant();
    }
}
