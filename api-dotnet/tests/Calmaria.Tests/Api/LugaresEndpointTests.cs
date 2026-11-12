using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calmaria.Tests.Api;

[TestClass]
public sealed class LugaresEndpointTests
{
    // 16/11/2026, 18h05 em São Paulo.
    private static readonly DateTimeOffset Agora = new(2026, 11, 16, 21, 5, 0, TimeSpan.Zero);

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
    public async Task Sem_coordenada_lista_os_seis_lugares_do_seed_sem_distancia()
    {
        var lugares = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares");

        Assert.AreEqual(6, lugares.GetArrayLength());
        var primeiro = lugares[0];
        Assert.AreEqual(1, primeiro.GetProperty("id").GetInt32());
        Assert.AreEqual("ESTACAO", primeiro.GetProperty("tipo").GetString());
        Assert.AreEqual(JsonValueKind.Null, primeiro.GetProperty("distanciaM").ValueKind);
        Assert.AreEqual(JsonValueKind.Null, primeiro.GetProperty("nivelAtual").ValueKind);
    }

    [TestMethod]
    public async Task Com_coordenada_ordena_por_distancia_e_respeita_o_raio()
    {
        // Em frente ao MASP: a estação Trianon-Masp é a mais perto; a Casa das Rosas, a uns 1,6 km, fica fora
        // do raio padrão de 1.500 metros.
        var lugares = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares?lat=-23.5614&lng=-46.6559");

        var ids = lugares.EnumerateArray().Select(l => l.GetProperty("id").GetInt32()).ToList();
        var distancias = lugares.EnumerateArray().Select(l => l.GetProperty("distanciaM").GetInt32()).ToList();
        Assert.AreEqual(3, ids[0]);
        CollectionAssert.DoesNotContain(ids, 6);
        CollectionAssert.AreEqual(distancias.Order().ToList(), distancias);
    }

    [TestMethod]
    public async Task Filtra_por_tipo()
    {
        var refugios = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares?tipo=REFUGIO");

        Assert.AreEqual(2, refugios.GetArrayLength());
        Assert.IsTrue(refugios.EnumerateArray().All(r => r.GetProperty("ehRefugio").GetBoolean()));
    }

    [TestMethod]
    public async Task Latitude_impossivel_da_400_em_problem_details()
    {
        var resposta = await cliente.GetAsync("/api/lugares?lat=95&lng=0");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.AreEqual("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Dados inválidos", problema.GetProperty("title").GetString());
        Assert.IsTrue(problema.GetProperty("errors").TryGetProperty("lat", out _));
    }

    [TestMethod]
    public async Task Latitude_sem_longitude_da_400()
    {
        var resposta = await cliente.GetAsync("/api/lugares?lat=-23.5614");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Tipo_desconhecido_da_400()
    {
        var resposta = await cliente.GetAsync("/api/lugares?tipo=CINEMA");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Lugar_que_nao_existe_da_404()
    {
        var resposta = await cliente.GetAsync("/api/lugares/999");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Lugar não encontrado", problema.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task Refugio_vem_com_descricao_e_sem_leitura()
    {
        var lugar = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/5");

        Assert.AreEqual("Parque Trianon", lugar.GetProperty("nome").GetString());
        Assert.IsTrue(lugar.GetProperty("ehRefugio").GetBoolean());
        Assert.AreEqual(JsonValueKind.String, lugar.GetProperty("descricao").ValueKind);
        Assert.AreEqual(JsonValueKind.Null, lugar.GetProperty("ultimaLeitura").ValueKind);
    }

    [TestMethod]
    public async Task Leitura_recente_vira_o_nivel_atual_no_horario_de_sao_paulo()
    {
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 95, 2900, 28, Agora.AddHours(-1));
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 78.5, 1450, 12, Agora.AddMinutes(-2));

        var lugares = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares");
        var consolacao = lugares.EnumerateArray().Single(l => l.GetProperty("id").GetInt32() == 1);

        Assert.AreEqual(53, consolacao.GetProperty("indiceAtual").GetInt32());
        Assert.AreEqual("MODERADO", consolacao.GetProperty("nivelAtual").GetString());
        Assert.AreEqual(
            new DateTimeOffset(2026, 11, 16, 18, 3, 0, TimeSpan.FromHours(-3)),
            consolacao.GetProperty("ultimaLeituraEm").GetDateTimeOffset());
        StringAssert.EndsWith(consolacao.GetProperty("ultimaLeituraEm").GetString(), "-03:00");
    }

    [TestMethod]
    public async Task Leitura_de_mais_de_15_minutos_nao_conta_como_agora_mas_aparece_no_detalhe()
    {
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 95, 2900, 28, Agora.AddMinutes(-40));

        var lugares = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares");
        var consolacao = lugares.EnumerateArray().Single(l => l.GetProperty("id").GetInt32() == 1);
        var detalhe = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/1");

        Assert.AreEqual(JsonValueKind.Null, consolacao.GetProperty("indiceAtual").ValueKind);
        Assert.AreEqual("INTENSO", detalhe.GetProperty("ultimaLeitura").GetProperty("nivel").GetString());
    }

    [TestMethod]
    public async Task Health_responde_healthy()
    {
        var resposta = await cliente.GetStringAsync("/health");

        Assert.AreEqual("Healthy", resposta);
    }
}
