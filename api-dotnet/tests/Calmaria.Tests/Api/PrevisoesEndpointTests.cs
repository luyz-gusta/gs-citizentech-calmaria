using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Calmaria.Tests.Api;

[TestClass]
public sealed class PrevisoesEndpointTests
{
    // Domingo, 15/11/2026, 16h em São Paulo.
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);

    private const string CsvDoContrato = """
        distrito,data,reclamacoes_previstas,nivel
        CONSOLACAO,2026-11-15,5.2,MEDIO
        CONSOLACAO,2026-11-21,9.4,ALTO
        CONSOLACAO,2026-11-24,4.1,MEDIO
        BELA VISTA,2026-11-21,6.0,ALTO
        """;

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

    private async Task<HttpResponseMessage> ImportarAsync(string csv, string? chave = CalmariaApiFactory.ChaveAdmin)
    {
        using var formulario = new MultipartFormDataContent();
        formulario.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(csv)), "arquivo", "previsao_barulho.csv");
        using var pedido = new HttpRequestMessage(HttpMethod.Post, "/api/previsoes/importar") { Content = formulario };
        if (chave is not null)
            pedido.Headers.Add("X-Admin-Key", chave);
        return await cliente.SendAsync(pedido);
    }

    [TestMethod]
    public async Task Importar_sem_a_chave_ou_com_a_chave_errada_da_401()
    {
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await ImportarAsync(CsvDoContrato, chave: null)).StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await ImportarAsync(CsvDoContrato, chave: "errada")).StatusCode);
    }

    [TestMethod]
    public async Task Importa_o_csv_do_contrato_e_consulta_os_proximos_7_dias()
    {
        var importacao = await ImportarAsync(CsvDoContrato);
        var proximos = await cliente.GetFromJsonAsync<JsonElement>("/api/previsoes/barulho?distrito=CONSOLACAO&dias=7");

        Assert.AreEqual(4, (await importacao.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("importadas").GetInt32());
        // De 15 a 21/11: o dia 24 fica de fora.
        Assert.AreEqual(2, proximos.GetArrayLength());
        Assert.AreEqual("2026-11-21", proximos[1].GetProperty("data").GetString());
        Assert.AreEqual("ALTO", proximos[1].GetProperty("nivel").GetString());
        Assert.AreEqual(9.4, proximos[1].GetProperty("reclamacoesPrevistas").GetDouble());
    }

    [TestMethod]
    public async Task Distrito_com_acento_e_minuscula_tambem_acha_e_um_dia_so_com_data()
    {
        await ImportarAsync(CsvDoContrato);

        var dia = await cliente.GetFromJsonAsync<JsonElement>("/api/previsoes/barulho?distrito=Consola%C3%A7%C3%A3o&data=2026-11-24");

        Assert.AreEqual(1, dia.GetArrayLength());
        Assert.AreEqual("MEDIO", dia[0].GetProperty("nivel").GetString());
    }

    [TestMethod]
    public async Task Importar_de_novo_atualiza_em_vez_de_duplicar()
    {
        await ImportarAsync(CsvDoContrato);
        await ImportarAsync("distrito,data,reclamacoes_previstas,nivel\nCONSOLACAO,2026-11-21,2.0,BAIXO");

        var dia = await cliente.GetFromJsonAsync<JsonElement>("/api/previsoes/barulho?distrito=CONSOLACAO&data=2026-11-21");

        Assert.AreEqual(1, dia.GetArrayLength());
        Assert.AreEqual("BAIXO", dia[0].GetProperty("nivel").GetString());
    }

    [TestMethod]
    public async Task Linha_errada_recusa_o_arquivo_e_diz_qual_linha()
    {
        var resposta = await ImportarAsync("distrito,data,reclamacoes_previstas,nivel\nCONSOLACAO,2026-11-21,9.4,ALTO\nCONSOLACAO,21/11/2026,9.4,ALTO");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
        StringAssert.Contains((await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("detail").GetString(), "Linha 3");
    }

    [TestMethod]
    public async Task Cabecalho_errado_e_recusado()
    {
        var resposta = await ImportarAsync("bairro;dia;quantidade\nCONSOLACAO;2026-11-21;9");

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Distrito_sem_previsao_da_404_e_sem_distrito_da_400()
    {
        await ImportarAsync(CsvDoContrato);

        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/previsoes/barulho?distrito=JARDIM%20PAULISTA")).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/previsoes/barulho")).StatusCode);
    }
}
