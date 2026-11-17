using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calmaria.Tests.Api;

/// <summary>O cadastro de lugares (POST, PUT e DELETE em /api/lugares), uso da equipe com a chave de admin.</summary>
[TestClass]
public sealed class LugaresCadastroEndpointTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);

    private CalmariaApiFactory fabrica = null!;
    private HttpClient cliente = null!;

    [TestInitialize]
    public void Iniciar()
    {
        fabrica = new CalmariaApiFactory(Agora);
        fabrica.PrepararBanco();
        cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Admin-Key", CalmariaApiFactory.ChaveAdmin);
    }

    [TestCleanup]
    public void Encerrar()
    {
        cliente.Dispose();
        fabrica.Dispose();
    }

    private static object Mercado(string nome = "Mercado da Rua Frei Caneca", string? horario = "08:00-22:00") => new
    {
        nome, tipo = "MERCADO", latitude = -23.5531, longitude = -46.6545, distrito = "Consolação",
        endereco = "Rua Frei Caneca, 500", horarioFuncionamento = horario
    };

    [TestMethod]
    [DataRow("POST", "/api/lugares")]
    [DataRow("PUT", "/api/lugares/1")]
    [DataRow("DELETE", "/api/lugares/1")]
    public async Task Cadastro_sem_a_chave_da_401(string metodo, string rota)
    {
        using var semChave = fabrica.CreateClient();
        using var pedido = new HttpRequestMessage(new HttpMethod(metodo), rota) { Content = JsonContent.Create(Mercado()) };

        Assert.AreEqual(HttpStatusCode.Unauthorized, (await semChave.SendAsync(pedido)).StatusCode);
    }

    [TestMethod]
    public async Task Cria_um_lugar_monitorado_com_location_e_distrito_normalizado()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/lugares", Mercado());
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        var id = corpo.GetProperty("id").GetInt32();
        Assert.AreEqual($"/api/lugares/{id}", resposta.Headers.Location!.OriginalString);
        Assert.AreEqual("CONSOLACAO", corpo.GetProperty("distrito").GetString());
        Assert.IsFalse(corpo.GetProperty("ehRefugio").GetBoolean());
        Assert.AreEqual(HttpStatusCode.OK, (await cliente.GetAsync($"/api/lugares/{id}")).StatusCode);
    }

    [TestMethod]
    public async Task Refugio_cadastrado_entra_na_lista_do_botao_de_crise()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/lugares", new
        {
            nome = "Biblioteca Mário de Andrade, sala de leitura", tipo = "REFUGIO", latitude = -23.5467,
            longitude = -46.6431, distrito = "REPUBLICA", descricao = "Sala silenciosa com poltronas."
        });
        var id = (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var refugios = await cliente.GetFromJsonAsync<JsonElement>("/api/refugios");

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        Assert.IsTrue(refugios.EnumerateArray().Any(r => r.GetProperty("id").GetInt32() == id
                                                         && r.GetProperty("descricao").GetString() == "Sala silenciosa com poltronas."));
    }

    [TestMethod]
    public async Task Dados_invalidos_dao_400()
    {
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await cliente.PostAsJsonAsync("/api/lugares", new { nome = "X", tipo = "MERCADO", latitude = 91, longitude = 0, distrito = "SE" })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await cliente.PostAsJsonAsync("/api/lugares", new { tipo = "MERCADO", latitude = -23.5, longitude = -46.6, distrito = "SE" })).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await cliente.PostAsJsonAsync("/api/lugares", Mercado(horario: "25:00-26:00"))).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest,
            (await cliente.PostAsJsonAsync("/api/lugares", new
            {
                nome = "Mercado com descrição", tipo = "MERCADO", latitude = -23.5, longitude = -46.6, distrito = "SE",
                descricao = "Só refúgio tem descrição."
            })).StatusCode);
    }

    [TestMethod]
    public async Task Atualiza_o_cadastro_e_nao_troca_refugio_por_monitorado()
    {
        var criado = await cliente.PostAsJsonAsync("/api/lugares", Mercado());
        var id = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var atualizado = await cliente.PutAsJsonAsync($"/api/lugares/{id}", Mercado(nome: "Mercado Frei Caneca"));
        var trocaDeCategoria = await cliente.PutAsJsonAsync("/api/lugares/5", Mercado());
        var naoExiste = await cliente.PutAsJsonAsync("/api/lugares/999", Mercado());

        Assert.AreEqual(HttpStatusCode.OK, atualizado.StatusCode);
        Assert.AreEqual("Mercado Frei Caneca", (await atualizado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("nome").GetString());
        Assert.AreEqual(HttpStatusCode.Conflict, trocaDeCategoria.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, naoExiste.StatusCode);
    }

    [TestMethod]
    public async Task Apaga_lugar_novo_mas_nao_lugar_com_sensor()
    {
        var criado = await cliente.PostAsJsonAsync("/api/lugares", Mercado());
        var id = (await criado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        Assert.AreEqual(HttpStatusCode.NoContent, (await cliente.DeleteAsync($"/api/lugares/{id}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/lugares/{id}")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.DeleteAsync($"/api/lugares/{id}")).StatusCode);
        // O lugar 1 tem o sensor esp32-consolacao-01.
        Assert.AreEqual(HttpStatusCode.Conflict, (await cliente.DeleteAsync("/api/lugares/1")).StatusCode);
    }
}
