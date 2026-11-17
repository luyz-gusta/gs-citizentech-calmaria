using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Regras;

/// <summary>CPF com ou sem pontuação, só quando o dígito verificador confere (corta falso positivo).</summary>
public sealed partial class RegraCpf : RegraBase
{
    public override string Id => "cpf";
    public override CategoriaPii Categoria => CategoriaPii.Cpf;
    public override int Peso => 4;
    protected override Regex Padrao => Expressao();

    [GeneratedRegex(@"(?<!\d)(?<!\d[.-])\d{3}\.?\d{3}\.?\d{3}-?\d{2}(?!\d)(?![.-]\d)", RegexOptions.None, 250)]
    private static partial Regex Expressao();

    protected override bool Confirmar(string trecho, string texto, int posicao) => Valido(SoDigitos(trecho));

    protected override string Mascarar(string trecho)
    {
        var d = SoDigitos(trecho);
        return $"{d[..3]}.***.***-{d[^2..]}";
    }

    /// <summary>Os dois dígitos verificadores do CPF; sequências repetidas (111.111.111-11) não valem.</summary>
    public static bool Valido(string digitos)
    {
        if (digitos.Length != 11 || digitos.All(c => c == digitos[0]))
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();
        for (var posicao = 9; posicao <= 10; posicao++)
        {
            var soma = 0;
            for (var i = 0; i < posicao; i++)
                soma += numeros[i] * (posicao + 1 - i);
            var resto = soma * 10 % 11 % 10;
            if (resto != numeros[posicao])
                return false;
        }

        return true;
    }
}

/// <summary>
/// Telefone brasileiro, fixo ou celular, com ou sem DDD e +55. Não casa no meio de um número maior escrito em
/// grupos (protocolo, cartão): grupo de dígitos logo antes, separado por espaço, desclassifica.
/// </summary>
public sealed partial class RegraTelefone : RegraBase
{
    public override string Id => "telefone";
    public override CategoriaPii Categoria => CategoriaPii.Telefone;
    public override int Peso => 3;
    protected override Regex Padrao => Expressao();

    [GeneratedRegex(@"(?<!\d)(?<!\d[.-])(?<!\d\s)(?:\+?55[\s.-]?)?(?:\(?[1-9]\d\)?[\s.-]?)?(?:9\d{4}|[2-5]\d{3})[\s.-]?\d{4}(?!\d)(?![.-]\d)", RegexOptions.None, 250)]
    private static partial Regex Expressao();

    protected override string Mascarar(string trecho) => $"****-{SoDigitos(trecho)[^4..]}";
}

/// <summary>Endereço de e-mail.</summary>
public sealed partial class RegraEmail : RegraBase
{
    public override string Id => "email";
    public override CategoriaPii Categoria => CategoriaPii.Email;
    public override int Peso => 3;
    protected override Regex Padrao => Expressao();

    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.None, 250)]
    private static partial Regex Expressao();

    protected override string Mascarar(string trecho)
    {
        var arroba = trecho.IndexOf('@');
        return $"{trecho[0]}***{trecho[arroba..]}";
    }
}

/// <summary>
/// Segredo no texto ou no código: senha em string de conexão, chave de API, token Bearer e prefixos conhecidos
/// de chave (sk-, AKIA). É o que a varredura de código procura. Para não marcar o próprio código, o valor da senha
/// precisa ter 4 caracteres ou mais e não pode ser variável ($senha, %SENHA%, {senha}), marcador (..., ***,
/// &lt;senha&gt;), expressão ([...], (...)) nem nome de tipo (string, int); a chave de API precisa ter dígito.
/// </summary>
public sealed partial class RegraSegredo : RegraBase
{
    public override string Id => "segredo";
    public override CategoriaPii Categoria => CategoriaPii.Segredo;
    public override int Peso => 5;
    protected override Regex Padrao => Expressao();

    [GeneratedRegex(
        @"(?i:\b(?:password|pwd|senha)['""]?\s*[=:]\s*['""]?(?![$%{<\[(.*])(?!(?:string|number|bool|boolean|int|str|null|undefined|none|true|false)\b)[^\s;,'""]{4,}|\bapi[_-]?key['""]?\s*[=:]\s*['""]?(?=[A-Za-z_\-]*\d)[A-Za-z0-9_\-]{8,}|\bbearer\s+[A-Za-z0-9\-._~+/]{10,}=*|\bsk-[A-Za-z0-9_-]{16,})|\bAKIA[0-9A-Z]{16}\b",
        RegexOptions.None, 250)]
    private static partial Regex Expressao();

    protected override string Mascarar(string trecho) => "<SEGREDO>";
}

/// <summary>
/// Latitude e longitude com 4 casas ou mais (uns 10 metros) no mesmo texto que um identificador de usuário: diz
/// onde uma pessoa estava. Coordenada de lugar público, sem usuário, não conta.
/// </summary>
public sealed partial class RegraLocalizacaoPrecisa : RegraBase
{
    public override string Id => "localizacao-precisa";
    public override CategoriaPii Categoria => CategoriaPii.LocalizacaoPrecisa;
    public override int Peso => 4;
    protected override Regex Padrao => Expressao();

    [GeneratedRegex(@"-?\d{1,2}\.\d{4,}\s*,\s*-?\d{1,3}\.\d{4,}", RegexOptions.None, 250)]
    private static partial Regex Expressao();

    [GeneratedRegex(@"\b(?:usuario|usuarioid|userid|user)\b|[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.None, 250)]
    private static partial Regex IdentificadorDeUsuario();

    protected override bool Confirmar(string trecho, string texto, int posicao) =>
        IdentificadorDeUsuario().IsMatch(Normalizar(texto));

    protected override string Mascarar(string trecho) => "<LOCALIZACAO>";
}

/// <summary>
/// Quase identificador: CEP e data de nascimento no mesmo texto. Sozinhos não identificam; juntos, quase.
/// </summary>
public sealed partial class RegraQuaseIdentificador : RegraBase
{
    public override string Id => "quase-identificador";
    public override CategoriaPii Categoria => CategoriaPii.QuaseIdentificador;
    public override int Peso => 2;
    protected override Regex Padrao => Cep();

    [GeneratedRegex(@"(?<!\d)(?<!\d[.-])\d{5}-?\d{3}(?!\d)(?![.-]\d)", RegexOptions.None, 250)]
    private static partial Regex Cep();

    [GeneratedRegex(@"(?<!\d)\d{2}/\d{2}/\d{4}(?!\d)", RegexOptions.None, 250)]
    private static partial Regex Data();

    public override IEnumerable<Achado> Avaliar(string texto)
    {
        if (string.IsNullOrEmpty(texto) || !Normalizar(texto).Contains("nasc"))
            yield break;

        var ceps = Cep().Matches(texto);
        var datas = Data().Matches(texto);
        if (ceps.Count == 0 || datas.Count == 0)
            yield break;

        foreach (Match m in ceps)
            yield return Criar(m.Index, m.Length, "<CEP>");
        foreach (Match m in datas)
            yield return Criar(m.Index, m.Length, "<DATA>");
    }

    protected override string Mascarar(string trecho) => "<CEP>";
}
