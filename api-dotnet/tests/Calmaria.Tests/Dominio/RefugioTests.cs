using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class RefugioTests
{
    private static readonly Coordenada EmFrenteAoMasp = new(-23.5614, -46.6559);
    private static readonly RefugioCandidato ParqueTrianon = new(5, "Parque Trianon", TipoLugar.Refugio, -23.5617, -46.6573, "Sombra");
    private static readonly RefugioCandidato CasaDasRosas = new(6, "Casa das Rosas", TipoLugar.Refugio, -23.5712, -46.6446, null);

    [TestMethod]
    public void Escolhe_o_mais_perto()
    {
        var escolhido = EscolhaDeRefugio.MaisProximo(EmFrenteAoMasp, [CasaDasRosas, ParqueTrianon]);

        Assert.IsNotNull(escolhido);
        Assert.AreEqual(5, escolhido.Lugar.Id);
        Assert.AreEqual(147, escolhido.DistanciaM, 2);
    }

    [TestMethod]
    public void Minutos_a_pe_arredondam_para_cima_a_80_metros_por_minuto()
    {
        // 147 metros / 80 = 1,8 minuto: são 2.
        var escolhido = EscolhaDeRefugio.MaisProximo(EmFrenteAoMasp, [ParqueTrianon])!;

        Assert.AreEqual(2, escolhido.MinutosAPe);
    }

    [TestMethod]
    public void Mesmo_ponto_ainda_da_um_minuto()
    {
        var escolhido = EscolhaDeRefugio.MaisProximo(new Coordenada(-23.5617, -46.6573), [ParqueTrianon])!;

        Assert.AreEqual(0, escolhido.DistanciaM);
        Assert.AreEqual(1, escolhido.MinutosAPe);
    }

    [TestMethod]
    public void Link_e_o_trajeto_a_pe_do_google_maps_com_ponto_decimal()
    {
        var link = EscolhaDeRefugio.LinkAPe(ParqueTrianon);

        Assert.AreEqual("https://www.google.com/maps/dir/?api=1&destination=-23.5617,-46.6573&travelmode=walking", link);
    }

    [TestMethod]
    public void Sem_candidatos_nao_ha_refugio()
    {
        Assert.IsNull(EscolhaDeRefugio.MaisProximo(EmFrenteAoMasp, []));
    }
}
