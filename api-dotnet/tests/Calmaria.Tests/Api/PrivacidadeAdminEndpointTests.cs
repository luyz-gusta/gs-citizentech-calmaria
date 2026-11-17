using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Calmaria.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Calmaria.Tests.Api;

/// <summary>
/// As rotas administrativas da privacidade e os guardas de saída, com a API inteira: varredura, Risco Cidadão
/// antes e depois, achados, fluxos e o filtro da resposta pública.
/// </summary>
[TestClass]
public sealed class PrivacidadeAdminEndpointTests
{
    // Domingo, 15/11/2026, 16h em São Paulo.
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);
    private const string CpfFicticio = "529.982.247-25";

    private string pastaDeLogs = null!;
    private string pastaDeCodigo = null!;
    private CalmariaApiFactory fabrica = null!;
    private HttpClient cliente = null!;

    [TestInitialize]
    public void Iniciar()
    {
        var raiz = Path.Combine(Path.GetTempPath(), "calmaria-admin-" + Guid.NewGuid().ToString("N"));
        pastaDeLogs = Directory.CreateDirectory(Path.Combine(raiz, "logs")).FullName;
        pastaDeCodigo = Directory.CreateDirectory(Path.Combine(raiz, "repo")).FullName;

        fabrica = new CalmariaApiFactory(Agora, new Dictionary<string, string>
        {
            ["Privacidade:Varredura:PastaDeLogs"] = pastaDeLogs,
            ["Privacidade:Varredura:PastaDeCodigo"] = pastaDeCodigo
        });
        fabrica.PrepararBanco();
        cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Add("X-Admin-Key", CalmariaApiFactory.ChaveAdmin);
    }

    [TestCleanup]
    public void Encerrar()
    {
        cliente.Dispose();
        fabrica.Dispose();
        Directory.Delete(Path.GetDirectoryName(pastaDeLogs)!, recursive: true);
    }

    private async Task<JsonElement> VarrerAsync()
    {
        var resposta = await cliente.PostAsync("/api/privacidade/varreduras", null);
        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        return await resposta.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task RelatarAsync(string comentario)
    {
        var usuario = await cliente.PostAsJsonAsync("/api/usuarios", new { });
        var id = (await usuario.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var relato = await cliente.PostAsJsonAsync("/api/relatos", new
        {
            usuarioId = id, lugarId = 4, nota = 2, gatilhos = new[] { "BARULHO" }, comentario,
            visitadoEm = "2026-11-14T18:00:00-03:00"
        });
        Assert.AreEqual(HttpStatusCode.Created, relato.StatusCode);
    }

    private void LogSujo() => File.WriteAllLines(Path.Combine(pastaDeLogs, "calmaria-20261114.log"),
    [
        "2026-11-14 18:00:00 -03:00 [INF] Seed: 8179 leituras simuladas gravadas.",
        $"2026-11-14 18:05:00 -03:00 [INF] RequestBody: {{\"comentario\":\"Meu CPF é {CpfFicticio} e meu filho é autista\"}}"
    ]);

    [TestMethod]
    [DataRow("POST", "/api/privacidade/varreduras")]
    [DataRow("GET", "/api/privacidade/risco-cidadao")]
    [DataRow("GET", "/api/privacidade/achados")]
    [DataRow("GET", "/api/privacidade/fluxos")]
    public async Task Rota_de_admin_sem_a_chave_da_401(string metodo, string rota)
    {
        using var semChave = fabrica.CreateClient();

        var resposta = await semChave.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), rota));

        Assert.AreEqual(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [TestMethod]
    public async Task Antes_da_primeira_varredura_o_risco_da_404()
    {
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/privacidade/risco-cidadao")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await cliente.GetAsync("/api/privacidade/achados")).StatusCode);
    }

    [TestMethod]
    public async Task Varredura_acha_o_log_cru_e_o_segredo_no_codigo_sem_devolver_nada_cru()
    {
        LogSujo();
        File.WriteAllText(Path.Combine(pastaDeCodigo, "appsettings.json"),
            "{ \"ConnectionStrings\": { \"Calmaria\": \"Host=db;Password=SenhaForte123\" } }");

        var varredura = await VarrerAsync();
        var achados = await cliente.GetStringAsync("/api/privacidade/achados");

        var risco = varredura.GetProperty("risco");
        Assert.AreEqual(100, risco.GetProperty("indice").GetInt32());
        Assert.AreEqual("ALTO", risco.GetProperty("faixa").GetString());
        Assert.AreEqual(3, varredura.GetProperty("achados").GetInt32());

        var lista = JsonDocument.Parse(achados).RootElement.EnumerateArray().ToList();
        CollectionAssert.AreEquivalent(new[] { "LOGS:CPF", "LOGS:SAUDE", "CODIGO:SEGREDO" },
            lista.Select(a => $"{a.GetProperty("fonte").GetString()}:{a.GetProperty("categoria").GetString()}").ToArray());
        Assert.IsTrue(lista.Any(a => a.GetProperty("local").GetString() == "calmaria-20261114.log:2"));
        Assert.IsTrue(lista.All(a => a.GetProperty("alertaParaCidadao").GetString() is { Length: > 0 }));

        // Nada cru sai pelas rotas de admin: nem o CPF, nem a senha.
        var tudo = varredura.GetRawText() + achados;
        Assert.DoesNotContain(CpfFicticio, tudo);
        Assert.DoesNotContain("SenhaForte123", tudo);

        // Sem LLM configurado, os alertas são as frases prontas, e o motivo aparece.
        var alerta = varredura.GetProperty("alertas")[0];
        Assert.AreEqual("MODELO", alerta.GetProperty("geradoPor").GetString());
        StringAssert.Contains(alerta.GetProperty("motivo").GetString(), "não configurado");
    }

    [TestMethod]
    public async Task Relato_com_cpf_e_saude_vira_fluxo_bloqueado_e_o_risco_fica_baixo()
    {
        await RelatarAsync($"Meu CPF é {CpfFicticio} e meu filho é autista.");

        var fluxos = await cliente.GetFromJsonAsync<JsonElement>("/api/privacidade/fluxos");
        var varredura = await VarrerAsync();

        var doRelato = fluxos.EnumerateArray().Where(f => f.GetProperty("origem").GetString() == "relato.comentario").ToList();
        CollectionAssert.AreEquivalent(new[] { "CPF", "SAUDE" },
            doRelato.Select(f => f.GetProperty("categoria").GetString()).ToArray());
        Assert.IsTrue(doRelato.All(f => f.GetProperty("bloqueado").GetBoolean()
                                        && f.GetProperty("destino").GetString() == "RESPOSTA_PUBLICA"));

        var risco = varredura.GetProperty("risco");
        Assert.AreEqual(20, risco.GetProperty("indice").GetInt32());
        Assert.AreEqual("BAIXO", risco.GetProperty("faixa").GetString());
        Assert.AreEqual(2, risco.GetProperty("mitigados").GetInt32());
        Assert.AreEqual(0, risco.GetProperty("expostos").GetInt32());
    }

    [TestMethod]
    public async Task Painel_mostra_o_antes_e_o_depois_da_correcao()
    {
        LogSujo();
        await VarrerAsync();

        // A correção: o log contaminado é apagado e o relato novo passa pelo Sanitizador.
        File.Delete(Path.Combine(pastaDeLogs, "calmaria-20261114.log"));
        await RelatarAsync($"Meu CPF é {CpfFicticio}.");
        await VarrerAsync();

        var painel = await cliente.GetFromJsonAsync<JsonElement>("/api/privacidade/risco-cidadao");

        Assert.AreEqual(100, painel.GetProperty("antes").GetProperty("indice").GetInt32());
        Assert.AreEqual("ALTO", painel.GetProperty("antes").GetProperty("faixa").GetString());
        Assert.AreEqual(20, painel.GetProperty("atual").GetProperty("risco").GetProperty("indice").GetInt32());
        Assert.AreEqual(2, painel.GetProperty("historico").GetArrayLength());
        Assert.AreEqual("2026-11-15T16:00:00-03:00", painel.GetProperty("atual").GetProperty("concluidaEm").GetString());
    }

    [TestMethod]
    public async Task Relato_gravado_por_fora_da_api_sai_mascarado_na_listagem_e_a_varredura_acha()
    {
        await RelatarAsync("Muito cheio às 18h");
        // Um registro que entrou por fora da API (importação, script antigo), com o telefone cru.
        await fabrica.ComEscopoAsync(sp => sp.GetRequiredService<CalmariaDbContext>().Database
            .ExecuteSqlRawAsync("UPDATE \"Relatos\" SET \"Comentario\" = 'Liguem para 11 98765-4321'"));

        var publicos = await cliente.GetFromJsonAsync<JsonElement>("/api/relatos?lugarId=4");
        var fluxos = await cliente.GetFromJsonAsync<JsonElement>("/api/privacidade/fluxos");
        var varredura = await VarrerAsync();

        Assert.AreEqual("Liguem para [removido]", publicos.GetProperty("itens")[0].GetProperty("comentario").GetString());
        Assert.IsTrue(fluxos.EnumerateArray().Any(f => f.GetProperty("origem").GetString() == "resposta:/api/relatos"
                                                         && f.GetProperty("categoria").GetString() == "TELEFONE"));
        var fonteRelatos = varredura.GetProperty("fontes").EnumerateArray()
            .Single(f => f.GetProperty("fonte").GetString() == "RELATOS");
        Assert.AreEqual(1, fonteRelatos.GetProperty("achados").GetInt32());
    }

    [TestMethod]
    public async Task Fontes_sem_pasta_ou_desligadas_aparecem_com_o_motivo()
    {
        Directory.Delete(pastaDeLogs);

        var varredura = await VarrerAsync();

        var fontes = varredura.GetProperty("fontes").EnumerateArray()
            .ToDictionary(f => f.GetProperty("fonte").GetString()!, f => f);
        CollectionAssert.AreEquivalent(new[] { "LOGS", "RELATOS", "MQTT", "CODIGO" }, fontes.Keys.ToArray());
        Assert.IsFalse(fontes["LOGS"].GetProperty("lida").GetBoolean());
        Assert.IsFalse(fontes["MQTT"].GetProperty("lida").GetBoolean(), "O MQTT fica desligado nos testes.");
        Assert.IsTrue(fontes["RELATOS"].GetProperty("lida").GetBoolean());
        Directory.CreateDirectory(pastaDeLogs);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(501)]
    public async Task Fluxos_com_limite_fora_de_1_a_500_da_400(int limite)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync($"/api/privacidade/fluxos?limite={limite}")).StatusCode);
    }
}
