using System.Net;
using System.Text;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Remediacao;
using Calmaria.Privacidade.Taint;
using Microsoft.Extensions.Logging.Abstractions;

namespace Calmaria.Privacidade.Tests;

[TestClass]
public sealed class TradutorTests
{
    private static readonly AchadoMascarado CpfMascarado = new(CategoriaPii.Cpf, "529.***.***-25", 4);

    private static readonly OpcoesLlm Configurado = new()
    {
        UrlBase = "http://llm.teste/v1/", Modelo = "modelo-de-teste", Chave = "chave-falsa"
    };

    private static TradutorLlm Tradutor(OpcoesLlm opcoes, HttpMessageHandler manipulador) =>
        new(new HttpClient(manipulador) { BaseAddress = new Uri("http://llm.teste/v1/") }, opcoes, new TradutorModelo(),
            NullLogger<TradutorLlm>.Instance);

    private static string RespostaDoChat(string conteudo) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            choices = new[] { new { message = new { role = "assistant", content = conteudo } } }
        });

    [TestMethod]
    public void Frase_pronta_do_cpf_no_mapa_e_literal_e_diz_o_que_foi_feito()
    {
        var alerta = new TradutorModelo().Traduzir(CategoriaPii.Cpf, DestinoFluxo.RespostaPublica, mitigado: true);

        Assert.AreEqual(
            "Seu comentário tinha seu CPF. Isso ficaria visível para qualquer pessoa no mapa. Nós tiramos o CPF antes de salvar.",
            alerta.ParaCidadao);
        Assert.AreEqual(OrigemDoAlerta.Modelo, alerta.GeradoPor);
    }

    [TestMethod]
    public void Frase_pronta_de_saude_exposta_cita_a_lgpd_para_a_equipe()
    {
        var alerta = new TradutorModelo().Traduzir(CategoriaPii.Saude, DestinoFluxo.Log, mitigado: false);

        StringAssert.StartsWith(alerta.ParaCidadao, "Encontramos uma informação de saúde nos registros do servidor.");
        StringAssert.Contains(alerta.ParaEquipe, "LGPD, art. 11");
        Assert.DoesNotContain("!", alerta.ParaCidadao);
    }

    [TestMethod]
    public async Task Sem_configuracao_nem_chama_e_usa_a_frase_pronta()
    {
        var manipulador = new ManipuladorFalso(_ => throw new AssertFailedException("Não devia chamar o LLM."));

        var alerta = await Tradutor(new OpcoesLlm(), manipulador).TraduzirAsync(CpfMascarado, DestinoFluxo.RespostaPublica, true);

        Assert.AreEqual(OrigemDoAlerta.Modelo, alerta.GeradoPor);
        StringAssert.Contains(alerta.Motivo, "não configurado");
    }

    [TestMethod]
    public async Task Com_o_llm_respondendo_o_alerta_vem_dele_e_so_o_trecho_mascarado_sai()
    {
        string? enviado = null;
        var manipulador = new ManipuladorFalso(pedido =>
        {
            enviado = pedido.Content!.ReadAsStringAsync().Result;
            Assert.AreEqual("Bearer", pedido.Headers.Authorization?.Scheme);
            return Resposta(HttpStatusCode.OK, RespostaDoChat(
                "```json\n{\"paraCidadao\":\"Seu comentário tinha seu CPF. Nós tiramos antes de salvar.\",\"paraEquipe\":\"Use o Sanitizador.\"}\n```"));
        });

        var alerta = await Tradutor(Configurado, manipulador).TraduzirAsync(CpfMascarado, DestinoFluxo.RespostaPublica, true);

        Assert.AreEqual(OrigemDoAlerta.Llm, alerta.GeradoPor);
        Assert.AreEqual("Use o Sanitizador.", alerta.ParaEquipe);
        StringAssert.Contains(enviado, "529.***.***-25");
        StringAssert.Contains(enviado, "modelo-de-teste");
        Assert.DoesNotContain("529.982.247-25", enviado!);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.InternalServerError)]
    [DataRow(HttpStatusCode.Unauthorized)]
    public async Task Erro_do_llm_cai_na_frase_pronta_com_o_codigo(HttpStatusCode codigo)
    {
        var manipulador = new ManipuladorFalso(_ => Resposta(codigo, "{}"));

        var alerta = await Tradutor(Configurado, manipulador).TraduzirAsync(CpfMascarado, DestinoFluxo.Log, false);

        Assert.AreEqual(OrigemDoAlerta.Modelo, alerta.GeradoPor);
        StringAssert.Contains(alerta.Motivo, ((int)codigo).ToString());
    }

    [TestMethod]
    public async Task Sem_internet_cai_na_frase_pronta()
    {
        var manipulador = new ManipuladorFalso(_ => throw new HttpRequestException("sem rede"));

        var alerta = await Tradutor(Configurado, manipulador).TraduzirAsync(CpfMascarado, DestinoFluxo.Log, false);

        Assert.AreEqual(OrigemDoAlerta.Modelo, alerta.GeradoPor);
        StringAssert.Contains(alerta.Motivo, "HttpRequestException");
    }

    [TestMethod]
    [DataRow("Desculpe, não posso ajudar.")]
    [DataRow("{\"paraCidadao\": \"\"}")]
    [DataRow("{ quebrado")]
    public void Resposta_fora_do_formato_nao_vira_alerta(string texto)
    {
        Assert.IsNull(TradutorLlm.Interpretar(texto));
    }

    [TestMethod]
    public void O_prompt_versionado_esta_embutido_no_modulo()
    {
        StringAssert.Contains(TradutorLlm.VersaoDoPrompt, "prompt-alerta, versão 1");
    }

    private static HttpResponseMessage Resposta(HttpStatusCode codigo, string json) =>
        new(codigo) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class ManipuladorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
