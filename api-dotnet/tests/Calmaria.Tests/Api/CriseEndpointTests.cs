using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calmaria.Tests.Api;

[TestClass]
public sealed class CriseEndpointTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);
    private const string EmFrenteAoMasp = "lat=-23.5614&lng=-46.6559";

    private CalmariaApiFactory fabrica = null!;
    private HttpClient cliente = null!;

    [TestInitialize]
    public void Iniciar()
    {
        fabrica = new CalmariaApiFactory(Agora);
        fabrica.PrepararBanco();
        cliente = fabrica.CreateClient();
    }

    [TestCleanup]
    public void Encerrar()
    {
        cliente.Dispose();
        fabrica.Dispose();
    }

    [TestMethod]
    public async Task Sem_leituras_leva_ao_refugio_fixo_mais_perto()
    {
        var refugio = await cliente.GetFromJsonAsync<JsonElement>($"/api/crise/refugio-mais-proximo?{EmFrenteAoMasp}");

        Assert.AreEqual(5, refugio.GetProperty("lugar").GetProperty("id").GetInt32());
        Assert.AreEqual("REFUGIO", refugio.GetProperty("lugar").GetProperty("tipo").GetString());
        Assert.AreEqual(2, refugio.GetProperty("minutosAPe").GetInt32());
        StringAssert.Contains(refugio.GetProperty("linkTrajeto").GetString(), "destination=-23.5617,-46.6573&travelmode=walking");
    }

    [TestMethod]
    public async Task Lugar_calmo_agora_e_mais_perto_ganha_do_refugio_fixo()
    {
        // A estação Trianon-Masp fica a uns 70 m; o Parque Trianon, a uns 150 m.
        await fabrica.AdicionarLeituraAsync(3, "esp32-trianon-01", 45, 350, 1, Agora.AddMinutes(-3));

        var refugio = await cliente.GetFromJsonAsync<JsonElement>($"/api/crise/refugio-mais-proximo?{EmFrenteAoMasp}");

        Assert.AreEqual(3, refugio.GetProperty("lugar").GetProperty("id").GetInt32());
        Assert.AreEqual("ESTACAO", refugio.GetProperty("lugar").GetProperty("tipo").GetString());
    }

    [TestMethod]
    public async Task Lugar_intenso_nunca_e_sugerido()
    {
        await fabrica.AdicionarLeituraAsync(3, "esp32-trianon-01", 95, 2900, 28, Agora.AddMinutes(-3));

        var refugio = await cliente.GetFromJsonAsync<JsonElement>($"/api/crise/refugio-mais-proximo?{EmFrenteAoMasp}");

        Assert.AreEqual(5, refugio.GetProperty("lugar").GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task Lugar_que_foi_calmo_ha_mais_de_15_minutos_nao_conta()
    {
        await fabrica.AdicionarLeituraAsync(3, "esp32-trianon-01", 45, 350, 1, Agora.AddMinutes(-40));

        var refugio = await cliente.GetFromJsonAsync<JsonElement>($"/api/crise/refugio-mais-proximo?{EmFrenteAoMasp}");

        Assert.AreEqual(5, refugio.GetProperty("lugar").GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task Longe_de_tudo_ainda_sugere_o_refugio_mais_perto()
    {
        // De Interlagos, fora do retângulo de busca (uns 5,5 km): a crise não fica sem resposta. Dali, a Casa das
        // Rosas (uns 15,4 km) fica mais perto que o Parque Trianon (uns 16 km).
        var refugio = await cliente.GetFromJsonAsync<JsonElement>("/api/crise/refugio-mais-proximo?lat=-23.7000&lng=-46.7000");

        Assert.AreEqual(6, refugio.GetProperty("lugar").GetProperty("id").GetInt32());
        Assert.IsGreaterThan(10_000, refugio.GetProperty("distanciaM").GetInt32());
    }

    [TestMethod]
    public async Task Sem_longitude_da_400()
    {
        var resposta = await cliente.GetAsync("/api/crise/refugio-mais-proximo?lat=-23.5614");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Refugios_traz_so_os_fixos_com_descricao()
    {
        var refugios = await cliente.GetFromJsonAsync<JsonElement>("/api/refugios");

        Assert.AreEqual(2, refugios.GetArrayLength());
        Assert.IsTrue(refugios.EnumerateArray().All(r => r.GetProperty("tipo").GetString() == "REFUGIO"));
        Assert.AreEqual(JsonValueKind.String, refugios[0].GetProperty("descricao").ValueKind);
    }
}
