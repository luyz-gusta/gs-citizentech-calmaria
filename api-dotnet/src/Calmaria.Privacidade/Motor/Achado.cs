namespace Calmaria.Privacidade.Motor;

/// <summary>O tipo de dado pessoal achado. Saúde é dado sensível (LGPD, art. 5º, II).</summary>
public enum CategoriaPii
{
    Cpf,
    Documento,
    Telefone,
    Email,
    Saude,
    Segredo,
    LocalizacaoPrecisa,
    QuaseIdentificador
}

/// <summary>A gravidade do achado, pelo peso da regra.</summary>
public enum Severidade
{
    Baixa,
    Media,
    Alta,
    Critica
}

/// <summary>
/// Um dado pessoal achado num texto: a regra, a posição e o trecho <b>já mascarado</b>. O texto original nunca
/// sai do motor.
/// </summary>
/// <param name="RegraId">Qual regra achou (ex.: "cpf").</param>
/// <param name="Categoria">O tipo de dado.</param>
/// <param name="Peso">De 1 a 5; entra no cálculo do Risco Cidadão.</param>
/// <param name="Inicio">Posição do primeiro caractere no texto.</param>
/// <param name="Tamanho">Quantos caracteres o trecho ocupa.</param>
/// <param name="TrechoMascarado">O trecho como pode aparecer (ex.: 529.***.***-25 ou &lt;SAUDE&gt;).</param>
public sealed record Achado(string RegraId, CategoriaPii Categoria, int Peso, int Inicio, int Tamanho,
    string TrechoMascarado)
{
    public int Fim => Inicio + Tamanho;

    public Severidade Severidade => Peso switch
    {
        >= 5 => Severidade.Critica,
        4 => Severidade.Alta,
        3 => Severidade.Media,
        _ => Severidade.Baixa
    };
}
