using System.Runtime.CompilerServices;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;

namespace Calmaria.Privacidade.Tests;

[TestClass]
public sealed class VarreduraTests
{
    private string pasta = null!;

    private static MotorPii Motor() => new(
    [
        new RegraCpf(), new RegraTelefone(), new RegraEmail(), new RegraSaude(), new RegraSegredo(),
        new RegraLocalizacaoPrecisa(), new RegraQuaseIdentificador()
    ]);

    [TestInitialize]
    public void Iniciar()
    {
        pasta = Path.Combine(Path.GetTempPath(), "calmaria-varredura-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(pasta);
    }

    [TestCleanup]
    public void Encerrar() => Directory.Delete(pasta, recursive: true);

    private void Escrever(string relativo, params string[] linhas)
    {
        var caminho = Path.Combine(pasta, relativo);
        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        File.WriteAllLines(caminho, linhas);
    }

    [TestMethod]
    public async Task Fonte_de_logs_acha_o_dado_cru_e_diz_o_arquivo_e_a_linha()
    {
        Escrever("calmaria-20261117.log",
            "2026-11-17 10:00:00 -03:00 [INF] Seed: 8179 leituras simuladas gravadas.",
            "2026-11-17 10:05:00 -03:00 [INF] RequestBody: {\"comentario\":\"Meu CPF é 529.982.247-25 e sou autista\"}");
        var servico = new ServicoVarredura([new FonteLogs(new OpcoesDaVarredura { PastaDeLogs = pasta })], Motor());

        var resultado = await servico.ExecutarAsync();

        Assert.HasCount(2, resultado.Achados);
        Assert.IsTrue(resultado.Achados.All(a => a.Local == "calmaria-20261117.log:2" && a.Destino == DestinoFluxo.Log));
        Assert.IsTrue(resultado.Achados.Any(a => a.Achado.TrechoMascarado == "529.***.***-25"));
        Assert.AreEqual(2, resultado.Fontes[0].TrechosLidos);
    }

    [TestMethod]
    public async Task Fonte_de_codigo_so_conta_segredo_e_pula_bin_testes_e_markdown()
    {
        Escrever("api/appsettings.json", "{ \"ConnectionStrings\": { \"Calmaria\": \"Host=db;Password=SenhaForte123\" } }");
        Escrever("api/Exemplo.cs", "// CPF de exemplo 529.982.247-25 no comentário não é segredo");
        Escrever("api/bin/Debug/appsettings.json", "Password=OutraSenha999");
        Escrever("api-dotnet/tests/Fixture.cs", "var chave = \"sk-abcdefghijklmnopqrstuvwxyz\";");
        Escrever("docs/guia.md", "Use Password=ExemploNoDoc no seu ambiente.");
        var opcoes = new OpcoesDaVarredura { PastaDeCodigo = pasta };
        var servico = new ServicoVarredura([new FonteCodigo(opcoes)], Motor());

        var resultado = await servico.ExecutarAsync();

        var achado = resultado.Achados.Single();
        Assert.AreEqual(CategoriaPii.Segredo, achado.Achado.Categoria);
        Assert.AreEqual("api/appsettings.json:1", achado.Local);
        Assert.AreEqual("<SEGREDO>", achado.Achado.TrechoMascarado);
        Assert.AreEqual(DestinoFluxo.Arquivo, achado.Destino);
    }

    [TestMethod]
    public async Task Fonte_indisponivel_entra_com_o_motivo_e_a_varredura_segue()
    {
        Escrever("calmaria.log", "telefone 11 98765-4321");
        var servico = new ServicoVarredura(
        [
            new FonteLogs(new OpcoesDaVarredura { PastaDeLogs = Path.Combine(pasta, "nao-existe") }),
            new FonteCodigo(new OpcoesDaVarredura { PastaDeCodigo = null }),
            new FonteLogs(new OpcoesDaVarredura { PastaDeLogs = pasta })
        ], Motor());

        var resultado = await servico.ExecutarAsync();

        Assert.IsFalse(resultado.Fontes[0].Lida);
        Assert.IsNotNull(resultado.Fontes[0].Motivo);
        Assert.IsFalse(resultado.Fontes[1].Lida);
        Assert.IsTrue(resultado.Fontes[2].Lida);
        Assert.AreEqual(CategoriaPii.Telefone, resultado.Achados.Single().Achado.Categoria);
    }

    [TestMethod]
    public async Task Para_em_500_achados_por_fonte_e_avisa()
    {
        var servico = new ServicoVarredura([new FonteEmMemoria(Enumerable.Repeat("CPF 529.982.247-25", 600))], Motor());

        var resultado = await servico.ExecutarAsync();

        Assert.HasCount(ServicoVarredura.MaximoDeAchadosPorFonte, resultado.Achados);
        Assert.AreEqual(600, resultado.Fontes[0].TrechosLidos);
        StringAssert.Contains(resultado.Fontes[0].Motivo, "500");
    }

    private sealed class FonteEmMemoria(IEnumerable<string> textos) : IFonteVarredura
    {
        public string Nome => "MEMORIA";
        public DestinoFluxo Destino => DestinoFluxo.BrokerMqtt;

        public async IAsyncEnumerable<TrechoVarrido> LerAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            await Task.CompletedTask;
            var n = 0;
            foreach (var texto in textos)
                yield return new TrechoVarrido($"mensagem {++n}", texto);
        }
    }
}
