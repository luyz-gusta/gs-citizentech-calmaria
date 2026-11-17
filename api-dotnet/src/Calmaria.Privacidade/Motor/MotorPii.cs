using Calmaria.Privacidade.Regras;

namespace Calmaria.Privacidade.Motor;

/// <summary>Roda todas as regras sobre um texto e devolve os achados, já mascarados.</summary>
public interface IMotorPii
{
    IReadOnlyList<Achado> Analisar(string texto);
}

/// <summary>
/// O motor de parsing de PII. Recebe as regras por injeção de dependência (as fixas e as do regras-pii.json) e,
/// quando dois achados se sobrepõem, fica com o de maior peso (no empate, o mais longo).
/// </summary>
public sealed class MotorPii(IEnumerable<IRegraPii> regras) : IMotorPii
{
    private readonly IReadOnlyList<IRegraPii> regras = regras.ToList();

    public IReadOnlyList<string> RegrasAtivas => regras.Select(r => r.Id).ToList();

    public IReadOnlyList<Achado> Analisar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return [];

        var candidatos = regras.SelectMany(r => r.Avaliar(texto))
            .OrderByDescending(a => a.Peso)
            .ThenByDescending(a => a.Tamanho)
            .ThenBy(a => a.Inicio);

        var escolhidos = new List<Achado>();
        foreach (var candidato in candidatos)
        {
            if (!escolhidos.Any(e => candidato.Inicio < e.Fim && e.Inicio < candidato.Fim))
                escolhidos.Add(candidato);
        }

        return escolhidos.OrderBy(a => a.Inicio).ToList();
    }
}
