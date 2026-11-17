using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Remediacao;

/// <summary>Quem escreveu o alerta: o LLM ou as frases prontas do <see cref="TradutorModelo"/>.</summary>
public enum OrigemDoAlerta
{
    Llm,
    Modelo
}

/// <summary>Um achado traduzido em risco real: uma frase para o cidadão e a correção para a equipe.</summary>
/// <param name="ParaCidadao">Literal e curta, sem alarme.</param>
/// <param name="ParaEquipe">A correção sugerida no código.</param>
/// <param name="GeradoPor">LLM ou frase pronta.</param>
/// <param name="Motivo">Por que caiu na frase pronta (sem chave, sem internet, resposta fora do formato).</param>
public sealed record AlertaCidadao(string ParaCidadao, string ParaEquipe, OrigemDoAlerta GeradoPor, string? Motivo = null);

/// <summary>
/// Traduz uma falha técnica em alerta de risco ao cidadão. Recebe só <see cref="AchadoMascarado"/>: o dado real
/// não tem como chegar ao serviço externo, porque o tipo não deixa.
/// </summary>
public interface ITradutorDeRisco
{
    Task<AlertaCidadao> TraduzirAsync(AchadoMascarado achado, DestinoFluxo destino, bool mitigado,
        CancellationToken ct = default);
}

/// <summary>
/// As frases prontas, por categoria e destino. É o que vale sem chave do LLM ou sem internet, e o que o
/// <see cref="TradutorLlm"/> usa quando a chamada falha.
/// </summary>
public sealed class TradutorModelo : ITradutorDeRisco
{
    public Task<AlertaCidadao> TraduzirAsync(AchadoMascarado achado, DestinoFluxo destino, bool mitigado,
        CancellationToken ct = default) =>
        Task.FromResult(Traduzir(achado.Categoria, destino, mitigado));

    public AlertaCidadao Traduzir(CategoriaPii categoria, DestinoFluxo destino, bool mitigado, string? motivo = null) =>
        new(ParaCidadao(categoria, destino, mitigado), ParaEquipe(categoria, destino, mitigado), OrigemDoAlerta.Modelo,
            motivo);

    private static string ParaCidadao(CategoriaPii categoria, DestinoFluxo destino, bool mitigado)
    {
        if (categoria == CategoriaPii.Segredo)
            return mitigado
                ? "Uma senha do sistema quase saiu junto com os dados. Nós tiramos a senha antes. Seus dados continuam protegidos."
                : $"Uma senha do sistema estava {OndeEstava(destino)}. Com ela, alguém poderia entrar nos dados da Calmaria. A equipe vai trocar a senha.";

        var (dado, curto) = Dado(categoria);
        return mitigado
            ? $"{Origem(destino)} tinha {dado}. Isso ficaria visível para {QuemVe(destino)}. Nós tiramos {curto} antes de {Verbo(destino)}."
            : $"Encontramos {dado} {OndeEstava(destino)}. Isso pode ser visto por {QuemVe(destino)}. A equipe vai apagar esse registro.";
    }

    private static string ParaEquipe(CategoriaPii categoria, DestinoFluxo destino, bool mitigado)
    {
        var correcao = (destino, mitigado) switch
        {
            (DestinoFluxo.Arquivo, _) =>
                "Tire o segredo do código, troque a chave no serviço (ela já vazou) e leia de variável de ambiente ou user-secrets.",
            (DestinoFluxo.Log, true) =>
                "O LoggerProviderComRedacao mascarou a linha. Ache quem loga o corpo da requisição e tire o campo do log.",
            (DestinoFluxo.Log, false) =>
                "Ligue o LoggerProviderComRedacao, pare de logar o corpo da requisição e apague os arquivos de log que já têm o dado.",
            (DestinoFluxo.RespostaPublica, true) =>
                "Nada a corrigir: o dado saiu antes de gravar. Mantenha o comentário como TextoSanitizado e o FiltroRespostaPublica na rota.",
            (DestinoFluxo.RespostaPublica, false) =>
                "A rota pública devolveu o dado cru. Grave só TextoSanitizado, mascare os registros antigos e ponha o FiltroRespostaPublica na rota.",
            (DestinoFluxo.BrokerMqtt, _) =>
                "O broker é público: o payload só pode ter as leituras. Tire o campo da mensagem no ESP32 e no app.",
            (DestinoFluxo.Llm, _) =>
                "Mande ao LLM só AchadoMascarado; o ITradutorDeRisco não aceita texto cru.",
            (DestinoFluxo.Banco, _) =>
                "Grave só TextoSanitizado e mascare por migração o que já está gravado cru.",
            _ => "Passe o texto pelo Sanitizador antes da saída."
        };

        return categoria == CategoriaPii.Saude
            ? $"{correcao} Dado de saúde é sensível (LGPD, art. 11)."
            : correcao;
    }

    private static (string Dado, string Curto) Dado(CategoriaPii categoria) => categoria switch
    {
        CategoriaPii.Cpf => ("seu CPF", "o CPF"),
        CategoriaPii.Documento => ("o número de um documento seu", "o número"),
        CategoriaPii.Telefone => ("um número de telefone", "o telefone"),
        CategoriaPii.Email => ("um e-mail", "o e-mail"),
        CategoriaPii.Saude => ("uma informação de saúde", "a informação de saúde"),
        CategoriaPii.LocalizacaoPrecisa => ("o lugar exato onde você estava", "a localização"),
        CategoriaPii.QuaseIdentificador => ("seu CEP junto com a data de nascimento", "o CEP e a data"),
        _ => ("um dado pessoal", "o dado")
    };

    private static string Origem(DestinoFluxo destino) =>
        destino == DestinoFluxo.RespostaPublica ? "Seu comentário" : "Um texto enviado ao app";

    private static string QuemVe(DestinoFluxo destino) => destino switch
    {
        DestinoFluxo.RespostaPublica => "qualquer pessoa no mapa",
        DestinoFluxo.Log => "as pessoas que cuidam do servidor",
        DestinoFluxo.Llm => "uma empresa de inteligência artificial",
        DestinoFluxo.BrokerMqtt => "qualquer pessoa conectada ao servidor público de mensagens",
        DestinoFluxo.Banco => "quem tem acesso ao banco de dados",
        _ => "quem recebesse os arquivos do projeto"
    };

    private static string OndeEstava(DestinoFluxo destino) => destino switch
    {
        DestinoFluxo.RespostaPublica => "num comentário publicado no mapa",
        DestinoFluxo.Log => "nos registros do servidor",
        DestinoFluxo.Llm => "num envio para a inteligência artificial",
        DestinoFluxo.BrokerMqtt => "numa mensagem do servidor público de mensagens",
        DestinoFluxo.Banco => "no banco de dados",
        _ => "nos arquivos do projeto"
    };

    private static string Verbo(DestinoFluxo destino) => destino switch
    {
        DestinoFluxo.RespostaPublica or DestinoFluxo.Banco => "salvar",
        DestinoFluxo.Log => "gravar no registro",
        DestinoFluxo.Arquivo => "exportar",
        _ => "enviar"
    };
}
