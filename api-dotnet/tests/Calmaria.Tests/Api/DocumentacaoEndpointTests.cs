using System.Net.Http.Json;
using System.Text.Json;

namespace Calmaria.Tests.Api;

/// <summary>O documento OpenAPI que o Scalar mostra: módulos, exemplos e os tipos de texto da privacidade.</summary>
[TestClass]
public sealed class DocumentacaoEndpointTests
{
    [TestMethod]
    public async Task OpenApi_tem_os_tres_modulos_os_exemplos_e_os_textos_como_string()
    {
        using var fabrica = new CalmariaApiFactory(DateTimeOffset.UtcNow);
        using var cliente = fabrica.CreateClient();

        var documento = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");

        var tags = documento.GetProperty("tags").EnumerateArray().Select(t => t.GetProperty("name").GetString()).ToList();
        CollectionAssert.IsSubsetOf(new[] { "Sensorial", "Comunidade", "Privacidade" }, tags);

        var esquemas = documento.GetProperty("components").GetProperty("schemas");
        Assert.AreEqual("string", esquemas.GetProperty("TextoNaoConfiavel").GetProperty("type").GetString());
        Assert.AreEqual("string", esquemas.GetProperty("TextoSanitizado").GetProperty("type").GetString());
        var exemplo = esquemas.GetProperty("NovoRelatoDto").GetProperty("examples")[0];
        Assert.AreEqual(4, exemplo.GetProperty("nota").GetInt32());
        Assert.IsTrue(esquemas.GetProperty("LugarEntradaDto").TryGetProperty("examples", out _));
    }

    [TestMethod]
    public async Task Corpo_da_requisicao_nao_herda_a_descricao_do_cancelamento_nem_resumo_tem_html()
    {
        using var fabrica = new CalmariaApiFactory(DateTimeOffset.UtcNow);
        using var cliente = fabrica.CreateClient();

        var documento = await cliente.GetFromJsonAsync<JsonElement>("/openapi/v1.json");

        var operacoes = documento.GetProperty("paths").EnumerateObject()
            .SelectMany(caminho => caminho.Value.EnumerateObject().Select(m => (Rota: $"{m.Name} {caminho.Name}", Operacao: m.Value)))
            .ToList();
        foreach (var (rota, operacao) in operacoes)
        {
            if (operacao.TryGetProperty("requestBody", out var corpo) && corpo.TryGetProperty("description", out var descricao))
                Assert.DoesNotContain("Cancelamento", descricao.GetString()!, $"O corpo de {rota} herdou a descrição do CancellationToken.");
            if (operacao.TryGetProperty("summary", out var resumo))
                Assert.DoesNotContain("<", resumo.GetString()!, $"O resumo de {rota} tem marcação HTML.");
        }

        var analisar = documento.GetProperty("paths").GetProperty("/api/privacidade/analisar").GetProperty("post");
        Assert.AreEqual("O texto, até 2.000 caracteres.", analisar.GetProperty("requestBody").GetProperty("description").GetString());
    }
}
