using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calmaria.Tests.Api;

[TestClass]
public sealed class ComunidadeEndpointTests
{
    // Domingo, 15/11/2026, 16h em São Paulo.
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);
    private const string Ontem = "2026-11-14T18:00:00-03:00";

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

    private async Task<Guid> NovoUsuarioAsync(string? apelido = null)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/usuarios", new { apelido });
        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    private Task<HttpResponseMessage> RelatarAsync(Guid usuario, int lugar = 4, int nota = 2, string? comentario = null,
        string[]? gatilhos = null, string visitadoEm = Ontem) =>
        cliente.PostAsJsonAsync("/api/relatos", new
        {
            usuarioId = usuario, lugarId = lugar, nota, gatilhos = gatilhos ?? ["BARULHO", "MULTIDAO"], comentario, visitadoEm
        });

    [TestMethod]
    public async Task Perfil_sensorial_e_gravado_e_lido_de_volta()
    {
        var usuario = await NovoUsuarioAsync();

        var gravar = await cliente.PutAsJsonAsync($"/api/usuarios/{usuario}/perfil-sensorial", new
        {
            sensibilidadeSom = 5, sensibilidadeLuz = 3, sensibilidadeMultidao = 4, contatoApoioNome = "Mãe", contatoApoioTelefone = "11900000000"
        });
        var perfil = await cliente.GetFromJsonAsync<JsonElement>($"/api/usuarios/{usuario}/perfil-sensorial");

        Assert.AreEqual(HttpStatusCode.NoContent, gravar.StatusCode);
        Assert.AreEqual(5, perfil.GetProperty("sensibilidadeSom").GetInt32());
        Assert.AreEqual("Mãe", perfil.GetProperty("contatoApoioNome").GetString());
    }

    [TestMethod]
    public async Task Sensibilidade_fora_de_1_a_5_da_400_e_usuario_que_nao_existe_da_404()
    {
        var usuario = await NovoUsuarioAsync();

        var foraDaFaixa = await cliente.PutAsJsonAsync($"/api/usuarios/{usuario}/perfil-sensorial",
            new { sensibilidadeSom = 6, sensibilidadeLuz = 3, sensibilidadeMultidao = 4 });
        var semUsuario = await cliente.PutAsJsonAsync($"/api/usuarios/{Guid.NewGuid()}/perfil-sensorial",
            new { sensibilidadeSom = 3, sensibilidadeLuz = 3, sensibilidadeMultidao = 3 });
        var semPerfil = await cliente.GetAsync($"/api/usuarios/{usuario}/perfil-sensorial");

        Assert.AreEqual(HttpStatusCode.BadRequest, foraDaFaixa.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, semUsuario.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, semPerfil.StatusCode);
    }

    [TestMethod]
    public async Task Relato_com_cpf_e_saude_e_gravado_sanitizado_e_a_resposta_diz_o_que_saiu()
    {
        var usuario = await NovoUsuarioAsync();

        var resposta = await RelatarAsync(usuario, comentario: "Meu CPF é 529.982.247-25 e meu filho é autista.");

        Assert.AreEqual(HttpStatusCode.Created, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Meu CPF é [removido] e meu filho é <SAUDE>.", corpo.GetProperty("comentario").GetString());
        var removidos = corpo.GetProperty("removidos").EnumerateArray().Select(r => r.GetProperty("categoria").GetString()).ToList();
        CollectionAssert.AreEquivalent(new[] { "CPF", "SAUDE" }, removidos);

        var publicos = await cliente.GetFromJsonAsync<JsonElement>("/api/relatos?lugarId=4");
        Assert.AreEqual("Meu CPF é [removido] e meu filho é <SAUDE>.", publicos.GetProperty("itens")[0].GetProperty("comentario").GetString());
    }

    [TestMethod]
    public async Task Listagem_publica_nao_tem_o_autor()
    {
        var usuario = await NovoUsuarioAsync("Ma");
        await RelatarAsync(usuario, comentario: "Muito cheio às 18h");

        var texto = await cliente.GetStringAsync("/api/relatos?lugarId=4");

        StringAssert.DoesNotMatch(texto, new System.Text.RegularExpressions.Regex("usuario|apelido", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
        Assert.IsFalse(texto.Contains(usuario.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Mesmo_usuario_e_lugar_em_menos_de_30_minutos_da_409()
    {
        var usuario = await NovoUsuarioAsync();

        var primeiro = await RelatarAsync(usuario);
        var repetido = await RelatarAsync(usuario);
        var outroLugar = await RelatarAsync(usuario, lugar: 1);

        Assert.AreEqual(HttpStatusCode.Created, primeiro.StatusCode);
        Assert.AreEqual(HttpStatusCode.Conflict, repetido.StatusCode);
        Assert.AreEqual(HttpStatusCode.Created, outroLugar.StatusCode);
    }

    [TestMethod]
    public async Task Regras_do_relato_dao_400_e_404()
    {
        var usuario = await NovoUsuarioAsync();

        Assert.AreEqual(HttpStatusCode.BadRequest, (await RelatarAsync(usuario, nota: 6)).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await RelatarAsync(usuario, gatilhos: ["CINEMA"])).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await RelatarAsync(usuario, visitadoEm: "2026-11-20T18:00:00-03:00")).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await RelatarAsync(usuario, comentario: new string('a', 501))).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await RelatarAsync(Guid.NewGuid())).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await RelatarAsync(usuario, lugar: 999)).StatusCode);
    }

    [TestMethod]
    public async Task Resumo_traz_nota_media_e_gatilhos_mais_citados()
    {
        await RelatarAsync(await NovoUsuarioAsync(), nota: 2, gatilhos: ["BARULHO", "MULTIDAO"]);
        await RelatarAsync(await NovoUsuarioAsync(), nota: 3, gatilhos: ["BARULHO", "LUZ_FORTE"]);
        await RelatarAsync(await NovoUsuarioAsync(), nota: 5, gatilhos: ["BARULHO"]);

        var resumo = await cliente.GetFromJsonAsync<JsonElement>("/api/lugares/4/resumo-relatos");

        Assert.AreEqual(3, resumo.GetProperty("totalRelatos").GetInt32());
        Assert.AreEqual(3.3, resumo.GetProperty("notaMedia").GetDouble());
        var primeiro = resumo.GetProperty("gatilhosMaisCitados")[0];
        Assert.AreEqual("BARULHO", primeiro.GetProperty("gatilho").GetString());
        Assert.AreEqual(3, primeiro.GetProperty("vezes").GetInt32());
    }

    [TestMethod]
    public async Task Paginacao_conta_o_total_e_respeita_o_tamanho()
    {
        for (var i = 0; i < 3; i++)
            await RelatarAsync(await NovoUsuarioAsync());

        var pagina = await cliente.GetFromJsonAsync<JsonElement>("/api/relatos?lugarId=4&pagina=2&tamanho=2");

        Assert.AreEqual(3, pagina.GetProperty("total").GetInt32());
        Assert.AreEqual(1, pagina.GetProperty("itens").GetArrayLength());
        Assert.AreEqual(HttpStatusCode.BadRequest, (await cliente.GetAsync("/api/relatos?tamanho=100")).StatusCode);
    }

    [TestMethod]
    public async Task Apagar_o_usuario_apaga_o_perfil_e_os_relatos()
    {
        var usuario = await NovoUsuarioAsync();
        await cliente.PutAsJsonAsync($"/api/usuarios/{usuario}/perfil-sensorial",
            new { sensibilidadeSom = 3, sensibilidadeLuz = 3, sensibilidadeMultidao = 3 });
        await RelatarAsync(usuario);

        var apagar = await cliente.DeleteAsync($"/api/usuarios/{usuario}");
        var perfil = await cliente.GetAsync($"/api/usuarios/{usuario}/perfil-sensorial");
        var relatos = await cliente.GetFromJsonAsync<JsonElement>("/api/relatos?lugarId=4");
        var deNovo = await cliente.DeleteAsync($"/api/usuarios/{usuario}");

        Assert.AreEqual(HttpStatusCode.NoContent, apagar.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, perfil.StatusCode);
        Assert.AreEqual(0, relatos.GetProperty("total").GetInt32());
        Assert.AreEqual(HttpStatusCode.NotFound, deNovo.StatusCode);
    }

    [TestMethod]
    public async Task Analisar_avisa_o_que_seria_removido_sem_gravar()
    {
        var analise = await cliente.PostAsJsonAsync("/api/privacidade/analisar", new { texto = "Meu CPF é 529.982.247-25" });
        var corpo = await analise.Content.ReadFromJsonAsync<JsonElement>();

        Assert.IsTrue(corpo.GetProperty("temDadoPessoal").GetBoolean());
        Assert.AreEqual("CPF", corpo.GetProperty("achados")[0].GetProperty("categoria").GetString());
        Assert.AreEqual("529.***.***-25", corpo.GetProperty("achados")[0].GetProperty("trecho").GetString());
        Assert.AreEqual("Meu CPF é [removido]", corpo.GetProperty("textoSanitizado").GetString());
    }
}
