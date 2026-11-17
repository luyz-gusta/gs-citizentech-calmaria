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
}
