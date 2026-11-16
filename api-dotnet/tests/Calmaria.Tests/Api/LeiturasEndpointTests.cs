using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Calmaria.Domain.Sensorial;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Infrastructure.Persistencia.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calmaria.Tests.Api;

[TestClass]
public sealed class LeiturasEndpointTests
{
    // Domingo, 15/11/2026, 16h em São Paulo.
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);

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

    private static object Payload(string dispositivo = "esp32-consolacao-01", int lugar = 1, double som = 78.5,
        double luz = 1450, int movimentos = 12) =>
        new { dispositivoId = dispositivo, lugarId = lugar, somDb = som, luzLux = luz, movimentosMin = movimentos, indice = 99, nivel = "INTENSO" };

    [TestMethod]
    public async Task Leitura_valida_e_gravada_com_o_indice_da_api_e_vira_o_nivel_atual()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/leituras", Payload());

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(53, corpo.GetProperty("indice").GetInt32(), "O índice 99 do ESP32 é ignorado: a API recalcula.");
        Assert.AreEqual("MODERADO", corpo.GetProperty("nivel").GetString());

        var lugar = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/1");
        Assert.AreEqual(53, lugar.GetProperty("ultimaLeitura").GetProperty("indice").GetInt32());
    }

    [TestMethod]
    public async Task Dispositivo_desconhecido_da_404()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/leituras", Payload(dispositivo: "esp32-fantasma"));

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Dispositivo não encontrado", problema.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task Som_impossivel_da_422()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/leituras", Payload(som: 200));

        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Dispositivo_de_outro_lugar_da_400()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/leituras", Payload(lugar: 2));

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Campo_faltando_da_400_com_o_nome_do_campo()
    {
        var resposta = await cliente.PostAsJsonAsync("/api/leituras", new { dispositivoId = "esp32-consolacao-01", lugarId = 1 });

        Assert.AreEqual(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Dados inválidos", problema.GetProperty("title").GetString());
        Assert.IsTrue(problema.GetProperty("errors").TryGetProperty("SomDb", out _));
    }

    [TestMethod]
    public async Task Historico_vem_da_mais_nova_para_a_mais_velha_e_respeita_o_desde()
    {
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 60, 500, 3, Agora.AddHours(-30));
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 70, 900, 8, Agora.AddHours(-2));
        await fabrica.AdicionarLeituraAsync(1, "esp32-consolacao-01", 90, 2500, 25, Agora.AddMinutes(-5));

        var ultimoDia = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/1/leituras");
        var desdeOntem = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/1/leituras?desde=2026-11-14T00:00:00-03:00");

        Assert.AreEqual(2, ultimoDia.GetArrayLength(), "Sem desde, só as últimas 24 horas.");
        Assert.AreEqual(90, ultimoDia[0].GetProperty("somDb").GetDouble());
        Assert.AreEqual(3, desdeOntem.GetArrayLength());
    }

    [TestMethod]
    public async Task Historico_de_lugar_que_nao_existe_da_404()
    {
        var resposta = await cliente.GetAsync("/api/lugares/999/leituras");

        Assert.AreEqual(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Horarios_calmos_com_poucas_leituras_nao_e_suficiente()
    {
        await fabrica.AdicionarLeituraAsync(4, "esp32-mercado-01", 60, 500, 3, Agora.AddHours(-1));

        var horarios = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/4/horarios-calmos");

        Assert.IsFalse(horarios.GetProperty("suficiente").GetBoolean());
        Assert.AreEqual(0, horarios.GetProperty("janelasMaisCalmas").GetArrayLength());
    }

    [TestMethod]
    public async Task Com_o_seed_os_horarios_calmos_trazem_tres_janelas_dentro_do_funcionamento()
    {
        var gravadas = await fabrica.ComEscopoAsync(sp => SeederDeLeituras.SemearAsync(
            sp.GetRequiredService<CalmariaDbContext>(), sp.GetRequiredService<ICalculadoraIndice>(), Agora.UtcDateTime));

        var horarios = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/4/horarios-calmos");

        Assert.IsGreaterThan(5_000, gravadas, $"O seed gravou {gravadas} leituras.");
        Assert.IsTrue(horarios.GetProperty("suficiente").GetBoolean());
        var janelas = horarios.GetProperty("janelasMaisCalmas").EnumerateArray().ToList();
        Assert.HasCount(3, janelas);
        // O mercado abre às 7h e fecha às 23h.
        Assert.IsTrue(janelas.All(j => j.GetProperty("hora").GetInt32() is >= 7 and <= 22));
        Assert.IsTrue(janelas.All(j => j.GetProperty("diaSemana").GetString() is "DOMINGO" or "SEGUNDA" or "TERCA" or "QUARTA" or "QUINTA" or "SEXTA" or "SABADO"));
    }

    [TestMethod]
    public async Task Seed_so_roda_com_a_tabela_vazia_e_sai_igual_em_qualquer_maquina()
    {
        var (primeira, segunda, soma) = await fabrica.ComEscopoAsync(async sp =>
        {
            var db = sp.GetRequiredService<CalmariaDbContext>();
            var calculadora = sp.GetRequiredService<ICalculadoraIndice>();
            var a = await SeederDeLeituras.SemearAsync(db, calculadora, Agora.UtcDateTime);
            var b = await SeederDeLeituras.SemearAsync(db, calculadora, Agora.UtcDateTime);
            var total = await db.Leituras.SumAsync(l => l.Indice);
            return (a, b, total);
        });

        using var outra = new CalmariaApiFactory(Agora);
        outra.PrepararBanco();
        var somaDaOutra = await outra.ComEscopoAsync(async sp =>
        {
            var db = sp.GetRequiredService<CalmariaDbContext>();
            await SeederDeLeituras.SemearAsync(db, sp.GetRequiredService<ICalculadoraIndice>(), Agora.UtcDateTime);
            return await db.Leituras.SumAsync(l => l.Indice);
        });

        Assert.IsGreaterThan(0, primeira);
        Assert.AreEqual(0, segunda);
        Assert.AreEqual(soma, somaDaOutra);
    }
}
