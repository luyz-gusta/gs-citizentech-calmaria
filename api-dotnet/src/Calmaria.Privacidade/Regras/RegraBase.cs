using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Regras;

/// <summary>Uma regra de dado pessoal. Cada uma é uma classe; o motor recebe todas por injeção de dependência.</summary>
public interface IRegraPii
{
    string Id { get; }
    CategoriaPii Categoria { get; }

    /// <summary>De 1 a 5: quanto o vazamento desse dado pesa no Risco Cidadão.</summary>
    int Peso { get; }

    IEnumerable<Achado> Avaliar(string texto);
}

/// <summary>O comum às regras: rodar uma expressão regular, filtrar falso positivo e mascarar.</summary>
public abstract class RegraBase : IRegraPii
{
    protected static readonly TimeSpan TempoLimite = TimeSpan.FromMilliseconds(250);

    public abstract string Id { get; }
    public abstract CategoriaPii Categoria { get; }
    public abstract int Peso { get; }

    /// <summary>A expressão que acha os candidatos. O tempo limite evita travar com texto feito para isso.</summary>
    protected abstract Regex Padrao { get; }

    public virtual IEnumerable<Achado> Avaliar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            yield break;

        foreach (Match m in Padrao.Matches(texto))
        {
            if (Confirmar(m.Value, texto, m.Index))
                yield return Criar(m.Index, m.Length, Mascarar(m.Value));
        }
    }

    /// <summary>Segunda checagem do candidato (dígito verificador, contexto). Por padrão, aceita.</summary>
    protected virtual bool Confirmar(string trecho, string texto, int posicao) => true;

    protected abstract string Mascarar(string trecho);

    protected Achado Criar(int inicio, int tamanho, string mascarado) =>
        new(Id, Categoria, Peso, inicio, tamanho, mascarado);

    /// <summary>
    /// O texto em minúsculas e sem acento, com o mesmo tamanho do original (letra a letra), para as posições
    /// continuarem valendo.
    /// </summary>
    protected internal static string Normalizar(string texto)
    {
        var resultado = new StringBuilder(texto.Length);
        foreach (var c in texto)
        {
            var decomposto = c.ToString().Normalize(NormalizationForm.FormD);
            resultado.Append(char.ToLowerInvariant(decomposto[0]));
        }

        return resultado.ToString();
    }

    protected static string SoDigitos(string texto) =>
        string.Concat(texto.Where(char.IsAsciiDigit));

    protected static string Janela(string texto, int posicao, int tamanho, int margem)
    {
        var inicio = Math.Max(0, posicao - margem);
        var fim = Math.Min(texto.Length, posicao + tamanho + margem);
        return texto[inicio..fim];
    }

    protected static string Invariante(FormattableString f) => f.ToString(CultureInfo.InvariantCulture);
}
