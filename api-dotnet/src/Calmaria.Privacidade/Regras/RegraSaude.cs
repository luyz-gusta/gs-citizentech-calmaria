using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Regras;

/// <summary>
/// Dado de saúde (sensível pela LGPD): diagnóstico, CID, laudo, especialista e remédios comuns no tratamento.
/// Não diferencia maiúscula nem acento. "Crise" só conta com contexto de saúde ("crise sensorial", "crise de
/// ansiedade"), para não marcar "crise econômica". "TEA" só em maiúsculas, para não marcar "tea" em inglês.
/// </summary>
public sealed partial class RegraSaude : RegraBase
{
    public override string Id => "saude";
    public override CategoriaPii Categoria => CategoriaPii.Saude;
    public override int Peso => 5;
    protected override Regex Padrao => Termos();

    // O texto já chega normalizado (minúsculo e sem acento) a esta expressão.
    [GeneratedRegex(
        @"\b(?:autistas?|autismo|espectro autista|transtorno do espectro|cid[\s-]*f84(?:\.\d)?|f84(?:\.\d)?|laudos?|neurologista|neuropediatra|psiquiatra|tdah|risperidona|aripiprazol|metilfenidato|ritalina|fluoxetina|sertralina)\b",
        RegexOptions.None, 250)]
    private static partial Regex Termos();

    [GeneratedRegex(@"\bTEA\b", RegexOptions.None, 250)]
    private static partial Regex Sigla();

    [GeneratedRegex(@"\bcrises?\b", RegexOptions.None, 250)]
    private static partial Regex Crise();

    [GeneratedRegex(@"sensoria|ansiedade|panico|nervos|meltdown|shutdown|autis|convuls|epilep|surto", RegexOptions.None, 250)]
    private static partial Regex ContextoDeSaude();

    [GeneratedRegex(@"econom|financ|politic|hidric|energ|climat", RegexOptions.None, 250)]
    private static partial Regex ContextoDeOutraCoisa();

    public override IEnumerable<Achado> Avaliar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            yield break;

        var normalizado = Normalizar(texto);
        foreach (Match m in Termos().Matches(normalizado))
            yield return Criar(m.Index, m.Length, "<SAUDE>");

        foreach (Match m in Sigla().Matches(texto))
            yield return Criar(m.Index, m.Length, "<SAUDE>");

        foreach (Match m in Crise().Matches(normalizado))
        {
            var perto = Janela(normalizado, m.Index, m.Length, margem: 40);
            if (ContextoDeSaude().IsMatch(perto) && !ContextoDeOutraCoisa().IsMatch(perto))
                yield return Criar(m.Index, m.Length, "<SAUDE>");
        }
    }

    protected override string Mascarar(string trecho) => "<SAUDE>";
}
