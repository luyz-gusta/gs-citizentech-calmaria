using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;

namespace Calmaria.Privacidade.Tests;

/// <summary>Pelo menos 3 positivos e 3 negativos por regra (critério de robustez de Cybersecurity).</summary>
[TestClass]
public sealed class RegrasTests
{
    private static bool Acha(IRegraPii regra, string texto) => regra.Avaliar(texto).Any();

    [TestMethod]
    [DataRow("Meu CPF é 529.982.247-25.")]
    [DataRow("cpf 52998224725 para o cadastro")]
    [DataRow("O documento 111.444.777-35 é do responsável")]
    public void Cpf_valido_e_achado(string texto) => Assert.IsTrue(Acha(new RegraCpf(), texto));

    [TestMethod]
    [DataRow("Meu CPF é 529.982.247-26")]          // dígito verificador errado
    [DataRow("111.111.111-11")]                    // sequência repetida
    [DataRow("O pedido 1234567 chegou")]
    [DataRow("Linha 2 do metrô, 18h30")]
    public void Cpf_invalido_ou_ausente_nao_e_achado(string texto) => Assert.IsFalse(Acha(new RegraCpf(), texto));

    [TestMethod]
    public void Cpf_e_mascarado_com_os_tres_primeiros_e_os_dois_ultimos()
    {
        var achado = new RegraCpf().Avaliar("CPF 529.982.247-25").Single();

        Assert.AreEqual("529.***.***-25", achado.TrechoMascarado);
        Assert.AreEqual(4, achado.Inicio);
    }

    [TestMethod]
    [DataRow("Me liga no (11) 91234-5678.")]
    [DataRow("whats +55 11 91234-5678")]
    [DataRow("fixo 3333-4444 depois das 18h")]
    [DataRow("contato 11912345678")]
    public void Telefone_e_achado(string texto) => Assert.IsTrue(Acha(new RegraTelefone(), texto));

    [TestMethod]
    [DataRow("O CEP é 01310-100")]
    [DataRow("Cheguei às 18h30 em 2026")]
    [DataRow("Foram 12345 pessoas")]
    public void Telefone_nao_confunde_cep_hora_e_contagem(string texto) => Assert.IsFalse(Acha(new RegraTelefone(), texto));

    [TestMethod]
    [DataRow("maria.silva@gmail.com")]
    [DataRow("escreve pra contato@escola.org.br, por favor")]
    [DataRow("E-MAIL: JOAO_123@HOTMAIL.COM")]
    public void Email_e_achado(string texto) => Assert.IsTrue(Acha(new RegraEmail(), texto));

    [TestMethod]
    [DataRow("me procura no @calmaria")]
    [DataRow("arroba sozinha @ no meio")]
    [DataRow("o site calmaria.org.br")]
    public void Email_incompleto_nao_e_achado(string texto) => Assert.IsFalse(Acha(new RegraEmail(), texto));

    [TestMethod]
    public void Email_e_mascarado_mantendo_o_dominio()
    {
        Assert.AreEqual("m***@gmail.com", new RegraEmail().Avaliar("maria.silva@gmail.com").Single().TrechoMascarado);
    }

    [TestMethod]
    [DataRow("Meu filho é autista e não aguenta barulho")]
    [DataRow("Tenho laudo de TEA desde 2019")]
    [DataRow("CID F84.0, tomo risperidona")]
    [DataRow("Tive uma crise sensorial na estação")]
    [DataRow("O AUTISMO dele fica pior na multidão")]
    public void Saude_e_achada(string texto) => Assert.IsTrue(Acha(new RegraSaude(), texto));

    [TestMethod]
    [DataRow("A crise econômica deixou o mercado vazio")]
    [DataRow("I like tea in the morning")]
    [DataRow("O mercado estava cheio e barulhento")]
    [DataRow("crise hídrica na cidade")]
    public void Saude_nao_marca_crise_economica_nem_tea_em_ingles(string texto) => Assert.IsFalse(Acha(new RegraSaude(), texto));

    [TestMethod]
    [DataRow("Host=db;Username=calmaria;Password=s3nh4Forte")]
    [DataRow("senha: abc12345")]
    [DataRow("Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.abc")]
    [DataRow("chave sk-proj-AbCdEfGhIjKlMnOp1234")]
    [DataRow("AKIAIOSFODNN7EXAMPLE")]
    [DataRow("{ \"password\": \"hunter2024\" }")]
    [DataRow("api_key = 'abcd1234efgh5678'")]
    public void Segredo_e_achado(string texto) => Assert.IsTrue(Acha(new RegraSegredo(), texto));

    [TestMethod]
    [DataRow("Uma senha forte é importante")]
    [DataRow("O token do metrô acabou")]
    [DataRow("Bearer sozinho")]
    [DataRow("Sem nome, e-mail, CPF nem senha: o identificador basta")]
    [DataRow("// O broker é público e sem senha: qualquer pessoa pode assinar o tópico.")]
    [DataRow("$senha = [Runtime.InteropServices.Marshal]::PtrToStringAuto($segura)")]
    [DataRow("\"Host=localhost;Username=calmaria;Password=$senha\"")]
    [DataRow("./efbundle --connection \"Host=localhost;Username=calmaria;Password=...\"")]
    [DataRow("Password=******")]
    [DataRow("password: string;")]
    [DataRow("var apiKey = configuracao[\"Llm:Chave\"];")]
    public void Segredo_nao_e_achado_em_frase_comum(string texto) => Assert.IsFalse(Acha(new RegraSegredo(), texto));

    [TestMethod]
    [DataRow("usuario 3f2b7c1e-8a4d-4c55-9e0b-1d2a6b7c8e90 em -23.56142, -46.65591")]
    [DataRow("usuarioId=12 estava em -23.5614,-46.6559")]
    [DataRow("o usuário estava em -23.561421, -46.655912 às 18h")]
    public void Localizacao_precisa_com_usuario_e_achada(string texto) => Assert.IsTrue(Acha(new RegraLocalizacaoPrecisa(), texto));

    [TestMethod]
    [DataRow("O Parque Trianon fica em -23.5617, -46.6573")]
    [DataRow("usuario perto de -23.56, -46.65")]
    [DataRow("usuario 12 sem coordenada")]
    public void Localizacao_de_lugar_publico_ou_aproximada_nao_e_achada(string texto) => Assert.IsFalse(Acha(new RegraLocalizacaoPrecisa(), texto));

    [TestMethod]
    [DataRow("CEP 01310-100, nascida em 12/03/2014")]
    [DataRow("nasc. 01/02/2010 e mora no 04567-000")]
    [DataRow("Data de nascimento 05/05/2015; CEP 01311000")]
    public void Cep_e_nascimento_juntos_sao_quase_identificador(string texto) => Assert.IsTrue(Acha(new RegraQuaseIdentificador(), texto));

    [TestMethod]
    [DataRow("CEP 01310-100")]
    [DataRow("nasceu em 12/03/2014")]
    [DataRow("visita em 12/03/2026 no CEP 01310-100")]
    public void Cep_ou_data_sozinhos_nao_sao_quase_identificador(string texto) => Assert.IsFalse(Acha(new RegraQuaseIdentificador(), texto));

    [TestMethod]
    public void Pesos_seguem_a_gravidade()
    {
        Assert.AreEqual(Severidade.Critica, new RegraSaude().Avaliar("autista").Single().Severidade);
        Assert.AreEqual(Severidade.Alta, new RegraCpf().Avaliar("529.982.247-25").Single().Severidade);
        Assert.AreEqual(Severidade.Media, new RegraEmail().Avaliar("a@b.com").Single().Severidade);
    }
}
