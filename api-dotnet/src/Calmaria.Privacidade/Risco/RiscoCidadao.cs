using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Risco;

/// <summary>As faixas do Risco Cidadão: 0 a 20 baixo, 21 a 50 médio, 51 a 100 alto.</summary>
public enum FaixaRisco
{
    Baixo,
    Medio,
    Alto
}

/// <summary>
/// Um dado do cidadão num destino inseguro: um achado da varredura (ainda exposto) ou um fluxo que um guarda
/// barrou (mitigado).
/// </summary>
/// <param name="Categoria">O tipo de dado.</param>
/// <param name="PesoDoDado">De 1 a 5, o peso da regra.</param>
/// <param name="Destino">Onde o dado estava ou para onde ia.</param>
/// <param name="Mitigado">Se um guarda tirou o dado antes de ele ficar exposto.</param>
public sealed record ItemDeRisco(CategoriaPii Categoria, int PesoDoDado, DestinoFluxo Destino, bool Mitigado);

/// <summary>Quanto uma categoria pesa no indicador.</summary>
public sealed record ParcelaPorCategoria(CategoriaPii Categoria, int Expostos, int Mitigados, double Pontos);

/// <summary>Quanto um destino pesa no indicador.</summary>
public sealed record ParcelaPorDestino(DestinoFluxo Destino, int Expostos, int Mitigados, double Pontos);

/// <summary>O indicador e a composição dele.</summary>
/// <param name="Indice">De 0 a 100.</param>
/// <param name="Faixa">Baixo, médio ou alto.</param>
/// <param name="Expostos">Itens com o dado ainda exposto.</param>
/// <param name="Mitigados">Itens que um guarda barrou.</param>
/// <param name="Pontos">A soma dos riscos.</param>
/// <param name="PontosMaximos">A soma dos riscos se nada estivesse mitigado.</param>
/// <param name="PorCategoria">A soma por tipo de dado, do que mais pesa para o que menos pesa.</param>
/// <param name="PorDestino">A soma por destino, do que mais pesa para o que menos pesa.</param>
public sealed record RiscoCidadao(int Indice, FaixaRisco Faixa, int Expostos, int Mitigados, double Pontos,
    double PontosMaximos, IReadOnlyList<ParcelaPorCategoria> PorCategoria, IReadOnlyList<ParcelaPorDestino> PorDestino);

/// <summary>O peso de cada destino: quanto mais gente pode ver, mais pesa.</summary>
public static class PesoDaExposicao
{
    /// <summary>
    /// 3 para o que qualquer pessoa ou outra empresa vê (broker público, resposta pública, LLM externo); 2 para
    /// log e arquivos exportados; 1 para o banco.
    /// </summary>
    public static int De(DestinoFluxo destino) => destino switch
    {
        DestinoFluxo.BrokerMqtt or DestinoFluxo.RespostaPublica or DestinoFluxo.Llm => 3,
        DestinoFluxo.Log or DestinoFluxo.Arquivo => 2,
        DestinoFluxo.Banco => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(destino), destino, "Destino sem peso de exposição.")
    };
}

/// <summary>Calcula o Risco Cidadão de um conjunto de itens.</summary>
public interface ICalculadoraRiscoCidadao
{
    RiscoCidadao Calcular(IEnumerable<ItemDeRisco> itens);
}

/// <summary>
/// risco de um item = peso do dado x peso da exposição x (1 se exposto, 0,2 se mitigado);
/// Risco Cidadão = 100 x soma dos riscos / soma dos riscos máximos (todos como expostos).
/// Sem nenhum item, o risco é 0. Com tudo mitigado, fica em 20: baixo, mas não zero, porque o dado chegou a
/// tentar sair.
/// </summary>
public sealed class CalculadoraRiscoCidadao : ICalculadoraRiscoCidadao
{
    public const double FatorMitigado = 0.2;

    public RiscoCidadao Calcular(IEnumerable<ItemDeRisco> itens)
    {
        var lista = itens.ToList();
        var pontos = lista.Sum(Risco);
        var maximo = lista.Sum(Maximo);
        var indice = maximo == 0 ? 0 : (int)Math.Round(100 * pontos / maximo, MidpointRounding.AwayFromZero);

        var porCategoria = lista.GroupBy(i => i.Categoria)
            .Select(g => new ParcelaPorCategoria(g.Key, g.Count(i => !i.Mitigado), g.Count(i => i.Mitigado),
                Arredondar(g.Sum(Risco))))
            .OrderByDescending(p => p.Pontos).ThenBy(p => p.Categoria)
            .ToList();
        var porDestino = lista.GroupBy(i => i.Destino)
            .Select(g => new ParcelaPorDestino(g.Key, g.Count(i => !i.Mitigado), g.Count(i => i.Mitigado),
                Arredondar(g.Sum(Risco))))
            .OrderByDescending(p => p.Pontos).ThenBy(p => p.Destino)
            .ToList();

        return new RiscoCidadao(indice, Faixa(indice), lista.Count(i => !i.Mitigado), lista.Count(i => i.Mitigado),
            Arredondar(pontos), Arredondar(maximo), porCategoria, porDestino);
    }

    public static FaixaRisco Faixa(int indice) => indice switch
    {
        <= 20 => FaixaRisco.Baixo,
        <= 50 => FaixaRisco.Medio,
        _ => FaixaRisco.Alto
    };

    private static double Maximo(ItemDeRisco item) => item.PesoDoDado * PesoDaExposicao.De(item.Destino);

    private static double Risco(ItemDeRisco item) => Maximo(item) * (item.Mitigado ? FatorMitigado : 1);

    private static double Arredondar(double valor) => Math.Round(valor, 1, MidpointRounding.AwayFromZero);
}
