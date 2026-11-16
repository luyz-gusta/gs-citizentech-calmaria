using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Regras;

/// <summary>
/// Regra que vem do regras-pii.json: padrão, peso, palavras de contexto e máscara. Dá para criar regra nova sem
/// recompilar a API (o "configurar novas regras no motor" do slide). Os exemplos do arquivo viram teste.
/// </summary>
public sealed class RegraConfiguravel : RegraBase
{
    private readonly Regex padrao;
    private readonly string[] contexto;
    private readonly string mascara;

    public override string Id { get; }
    public override CategoriaPii Categoria { get; }
    public override int Peso { get; }
    protected override Regex Padrao => padrao;

    public ExemplosDeRegra Exemplos { get; }

    public RegraConfiguravel(DefinicaoDeRegra definicao)
    {
        if (string.IsNullOrWhiteSpace(definicao.Id) || string.IsNullOrWhiteSpace(definicao.Padrao))
            throw new ArgumentException("Toda regra do arquivo precisa de id e padrão.");
        if (definicao.Peso is < 1 or > 5)
            throw new ArgumentException($"A regra {definicao.Id} precisa de peso de 1 a 5.");

        Id = definicao.Id;
        Categoria = definicao.Categoria;
        Peso = definicao.Peso;
        padrao = new Regex(definicao.Padrao, RegexOptions.None, TempoLimite);
        contexto = definicao.Contexto.Select(Normalizar).ToArray();
        mascara = string.IsNullOrWhiteSpace(definicao.Mascara) ? "[removido]" : definicao.Mascara;
        Exemplos = definicao.Exemplos ?? new ExemplosDeRegra([], []);
    }

    /// <summary>Com palavras de contexto, uma delas precisa aparecer até 30 caracteres antes do trecho.</summary>
    protected override bool Confirmar(string trecho, string texto, int posicao)
    {
        if (contexto.Length == 0)
            return true;
        var antes = Normalizar(texto[Math.Max(0, posicao - 30)..posicao]);
        return contexto.Any(antes.Contains);
    }

    protected override string Mascarar(string trecho) => mascara;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>Lê todas as regras do arquivo. Sem arquivo, nenhuma regra extra.</summary>
    public static IReadOnlyList<RegraConfiguravel> CarregarDe(string caminho)
    {
        if (!File.Exists(caminho))
            return [];
        var definicoes = JsonSerializer.Deserialize<List<DefinicaoDeRegra>>(File.ReadAllText(caminho), Json) ?? [];
        return definicoes.Select(d => new RegraConfiguravel(d)).ToList();
    }
}

/// <summary>Uma regra do regras-pii.json.</summary>
public sealed record DefinicaoDeRegra(string Id, CategoriaPii Categoria, int Peso, string Padrao,
    IReadOnlyList<string> Contexto, string? Mascara, ExemplosDeRegra? Exemplos);

/// <summary>Frases que a regra deve e não deve marcar; os testes conferem.</summary>
public sealed record ExemplosDeRegra(IReadOnlyList<string> Positivos, IReadOnlyList<string> Negativos);
