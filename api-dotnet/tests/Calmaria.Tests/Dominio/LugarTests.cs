using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class LugarTests
{
    private static readonly Coordenada EstacaoConsolacao = new(-23.5575, -46.6609);
    private static readonly Coordenada EstacaoTrianonMasp = new(-23.5614, -46.6566);

    [TestMethod]
    public void Um_grau_de_longitude_no_equador_mede_111_km()
    {
        var distancia = new Coordenada(0, 0).DistanciaEmMetrosAte(new Coordenada(0, 1));

        Assert.AreEqual(111_195, distancia, 111_195 * 0.01);
    }

    [TestMethod]
    public void Distancia_entre_duas_estacoes_da_linha_2()
    {
        // Consolação e Trianon-Masp ficam a uns 620 metros em linha reta.
        var distancia = EstacaoConsolacao.DistanciaEmMetrosAte(EstacaoTrianonMasp);

        Assert.AreEqual(617, distancia, 617 * 0.01);
    }

    [TestMethod]
    public void Distancia_e_a_mesma_nos_dois_sentidos()
    {
        Assert.AreEqual(
            EstacaoConsolacao.DistanciaEmMetrosAte(EstacaoTrianonMasp),
            EstacaoTrianonMasp.DistanciaEmMetrosAte(EstacaoConsolacao),
            1e-6);
    }

    [TestMethod]
    [DataRow(91.0, 0.0)]
    [DataRow(-91.0, 0.0)]
    [DataRow(0.0, 181.0)]
    [DataRow(double.NaN, 0.0)]
    public void Coordenada_impossivel_e_recusada(double latitude, double longitude)
    {
        Assert.ThrowsExactly<DominioException>(() => new Coordenada(latitude, longitude));
    }

    [TestMethod]
    public void Lugar_guarda_o_distrito_em_maiusculas()
    {
        var lugar = new LugarMonitorado(1, "Estação Consolação", TipoLugar.Estacao, EstacaoConsolacao, " consolacao ");

        Assert.AreEqual("CONSOLACAO", lugar.Distrito);
        Assert.IsFalse(lugar.EhRefugio);
    }

    [TestMethod]
    public void Lugar_monitorado_nao_pode_ser_do_tipo_refugio()
    {
        Assert.ThrowsExactly<DominioException>(
            () => new LugarMonitorado(9, "Praça", TipoLugar.Refugio, EstacaoConsolacao, "CONSOLACAO"));
    }

    [TestMethod]
    public void Refugio_e_sempre_do_tipo_refugio()
    {
        var refugio = new Refugio(5, "Parque Trianon", new Coordenada(-23.5617, -46.6573), "BELA VISTA");

        Assert.AreEqual(TipoLugar.Refugio, refugio.Tipo);
        Assert.IsTrue(refugio.EhRefugio);
    }

    [TestMethod]
    public void Lugar_sem_nome_e_recusado()
    {
        Assert.ThrowsExactly<DominioException>(() => new Refugio(5, " ", EstacaoConsolacao, "BELA VISTA"));
    }
}
