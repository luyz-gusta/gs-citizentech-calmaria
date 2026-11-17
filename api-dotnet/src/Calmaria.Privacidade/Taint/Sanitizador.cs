using System.Text;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Taint;

/// <summary>O texto limpo e o que foi tirado dele (só a categoria e o trecho mascarado).</summary>
public sealed record ResultadoSanitizacao(TextoSanitizado Texto, IReadOnlyList<AchadoMascarado> Removidos)
{
    public bool TinhaDadoPessoal => Removidos.Count > 0;
}

/// <summary>O único caminho de <see cref="TextoNaoConfiavel"/> para <see cref="TextoSanitizado"/>.</summary>
public interface ISanitizador
{
    ResultadoSanitizacao Sanitizar(TextoNaoConfiavel texto);

    /// <summary>Coordenada do botão de crise antes de qualquer registro: 3 casas, uns 100 metros.</summary>
    double ArredondarCoordenada(double grau);
}

/// <summary>
/// Roda o motor e troca cada achado no texto: dado de saúde vira &lt;SAUDE&gt;; o resto, [removido].
/// </summary>
public sealed class Sanitizador(IMotorPii motor) : ISanitizador
{
    public const string MarcaRemovido = "[removido]";
    public const string MarcaSaude = "<SAUDE>";

    public ResultadoSanitizacao Sanitizar(TextoNaoConfiavel texto)
    {
        var original = texto.ValorCru;
        var achados = motor.Analisar(original);
        if (achados.Count == 0)
            return new ResultadoSanitizacao(TextoSanitizado.Criar(original), []);

        var removidos = achados.Select(AchadoMascarado.De).ToList();
        return new ResultadoSanitizacao(TextoSanitizado.Criar(Mascarar(original, achados)), removidos);
    }

    /// <summary>
    /// Troca cada achado no texto pela marca: saúde vira &lt;SAUDE&gt;; o resto, [removido]. Os guardas de saída
    /// (log e resposta pública) usam a mesma troca, para o texto sair igual em todo lugar.
    /// </summary>
    public static string Mascarar(string texto, IReadOnlyList<Achado> achados)
    {
        var limpo = new StringBuilder(texto);
        foreach (var achado in achados.OrderByDescending(a => a.Inicio))
        {
            var marca = achado.Categoria == CategoriaPii.Saude ? MarcaSaude : MarcaRemovido;
            limpo.Remove(achado.Inicio, achado.Tamanho).Insert(achado.Inicio, marca);
        }

        return limpo.ToString();
    }

    public double ArredondarCoordenada(double grau) => Math.Round(grau, 3, MidpointRounding.AwayFromZero);
}
