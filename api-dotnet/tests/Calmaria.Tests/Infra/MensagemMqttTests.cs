using Calmaria.Infrastructure.Mqtt;

namespace Calmaria.Tests.Infra;

[TestClass]
public sealed class MensagemMqttTests
{
    private const string Topico = "calmaria/v1/lugares/1/leituras";
    private const string PayloadDoContrato =
        """{"dispositivoId":"esp32-consolacao-01","lugarId":1,"somDb":78.5,"luzLux":1450,"movimentosMin":12,"indice":53,"nivel":"MODERADO"}""";

    [TestMethod]
    public void Payload_do_contrato_vira_leitura()
    {
        var ok = MensagemMqtt.TentarLer(Topico, PayloadDoContrato, out var leitura, out var motivo);

        Assert.IsTrue(ok, motivo);
        Assert.AreEqual("esp32-consolacao-01", leitura!.DispositivoId);
        Assert.AreEqual(1, leitura.LugarId);
        Assert.AreEqual(78.5, leitura.SomDb);
        Assert.AreEqual(1450, leitura.LuzLux);
        Assert.AreEqual(12, leitura.MovimentosMin);
    }

    [TestMethod]
    public void Lugar_do_payload_diferente_do_topico_e_recusado()
    {
        var ok = MensagemMqtt.TentarLer("calmaria/v1/lugares/2/leituras", PayloadDoContrato, out _, out var motivo);

        Assert.IsFalse(ok);
        StringAssert.Contains(motivo, "não bate com o do tópico");
    }

    [TestMethod]
    [DataRow("calmaria/v1/lugares/abc/leituras")]
    [DataRow("calmaria/v2/lugares/1/leituras")]
    [DataRow("outro/topico")]
    public void Topico_fora_do_padrao_e_recusado(string topico)
    {
        Assert.IsFalse(MensagemMqtt.TentarLer(topico, PayloadDoContrato, out _, out _));
    }

    [TestMethod]
    public void Payload_que_nao_e_json_e_recusado_sem_repetir_o_conteudo()
    {
        var ok = MensagemMqtt.TentarLer(Topico, "CPF 529.982.247-25", out _, out var motivo);

        Assert.IsFalse(ok);
        Assert.AreEqual("payload não é um JSON válido", motivo);
    }

    [TestMethod]
    public void Campo_faltando_e_recusado()
    {
        Assert.IsFalse(MensagemMqtt.TentarLer(Topico, """{"dispositivoId":"esp32-consolacao-01","lugarId":1,"somDb":78.5}""", out _, out _));
    }
}
