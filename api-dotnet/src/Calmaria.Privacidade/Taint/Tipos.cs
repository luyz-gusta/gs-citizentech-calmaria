using System.Text.Json;
using System.Text.Json.Serialization;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Taint;

/// <summary>
/// Texto que veio do cidadão (comentário do relato, apelido) e ainda não passou pelo <see cref="ISanitizador"/>.
/// Não vira string: o valor é interno e o ToString não o mostra, então não dá para gravar, logar nem mandar ao
/// LLM sem sanitizar. Quem tentar, não compila. É o taint analysis no sistema de tipos.
/// </summary>
[JsonConverter(typeof(TextoNaoConfiavelJson))]
public sealed class TextoNaoConfiavel(string? valor)
{
    /// <summary>Só o Sanitizador lê o valor cru.</summary>
    internal string ValorCru { get; } = valor ?? string.Empty;

    public int Tamanho => ValorCru.Length;
    public bool Vazio => string.IsNullOrWhiteSpace(ValorCru);

    /// <summary>Não devolve o conteúdo, para um log por engano não vazar o dado.</summary>
    public override string ToString() => "[texto não confiável]";
}

/// <summary>Texto que passou pelo Sanitizador: pode ser gravado, logado e publicado.</summary>
[JsonConverter(typeof(TextoSanitizadoJson))]
public sealed class TextoSanitizado
{
    public string Valor { get; }

    private TextoSanitizado(string valor) => Valor = valor;

    /// <summary>Só o Sanitizador cria.</summary>
    internal static TextoSanitizado Criar(string valor) => new(valor);

    /// <summary>Só a persistência usa, para ler de volta o que já foi gravado sanitizado.</summary>
    internal static TextoSanitizado Reidratar(string valor) => new(valor);

    public override string ToString() => Valor;
}

/// <summary>
/// Um achado sem posição nem texto original: o que pode ir à resposta, ao painel e ao LLM. Só o módulo cria, a
/// partir de um achado do motor, então o <see cref="Calmaria.Privacidade.Remediacao.ITradutorDeRisco"/> nunca
/// recebe o dado real.
/// </summary>
public sealed record AchadoMascarado
{
    public CategoriaPii Categoria { get; }
    public string Trecho { get; }

    /// <summary>De 1 a 5, o peso da regra que achou.</summary>
    public int Peso { get; }

    internal AchadoMascarado(CategoriaPii categoria, string trecho, int peso)
    {
        Categoria = categoria;
        Trecho = trecho;
        Peso = peso;
    }

    /// <summary>O achado do motor já traz o trecho mascarado; aqui ele perde a posição.</summary>
    internal static AchadoMascarado De(Achado achado) => new(achado.Categoria, achado.TrechoMascarado, achado.Peso);
}

internal sealed class TextoNaoConfiavelJson : JsonConverter<TextoNaoConfiavel>
{
    public override TextoNaoConfiavel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.String
            ? new TextoNaoConfiavel(reader.GetString())
            : throw new JsonException("Esperava um texto.");

    public override void Write(Utf8JsonWriter writer, TextoNaoConfiavel value, JsonSerializerOptions options) =>
        throw new JsonException("Texto não confiável não sai da API: sanitize antes.");
}

internal sealed class TextoSanitizadoJson : JsonConverter<TextoSanitizado>
{
    public override TextoSanitizado Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        throw new JsonException("Texto sanitizado só sai do Sanitizador.");

    public override void Write(Utf8JsonWriter writer, TextoSanitizado value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Valor);
}
