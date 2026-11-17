using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Calmaria.Api.Privacidade;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;
using Calmaria.Privacidade.Taint;
using Microsoft.Extensions.Logging;

namespace Calmaria.Tests.Api;

/// <summary>
/// O guarda de saída do log: a mensagem com dado pessoal sai mascarada e vira fluxo bloqueado. Com a API inteira e o
/// log de depuração do corpo ligado (o erro comum), o antes grava o CPF cru e o depois grava [removido].
/// </summary>
[TestClass]
public sealed class LogComRedacaoTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 15, 19, 0, 0, TimeSpan.Zero);
    private const string CpfFicticio = "529.982.247-25";

    private static MotorPii Motor() => new(
    [
        new RegraCpf(), new RegraTelefone(), new RegraEmail(), new RegraSaude(), new RegraSegredo(),
        new RegraLocalizacaoPrecisa(), new RegraQuaseIdentificador()
    ]);

    [TestMethod]
    public void Mensagem_com_cpf_sai_mascarada_e_o_fluxo_fica_registrado()
    {
        var capturador = new ProvedorQueCaptura();
        var rastreador = new RastreadorDeFluxo(TimeProvider.System);
        using var provedor = new LoggerProviderComRedacao(capturador, Motor(), rastreador);

        provedor.CreateLogger("Teste").LogInformation("Comentário recebido: {Comentario}", $"CPF {CpfFicticio}, sou autista");

        Assert.AreEqual("Comentário recebido: CPF [removido], sou <SAUDE>", capturador.Mensagens.Single());
        var eventos = rastreador.Drenar();
        Assert.HasCount(2, eventos);
        Assert.IsTrue(eventos.All(e => e is { Origem: "log:Teste", Destino: DestinoFluxo.Log, Bloqueado: true }));
    }

    [TestMethod]
    public void Mensagem_sem_dado_pessoal_passa_igual_e_sem_fluxo()
    {
        var capturador = new ProvedorQueCaptura();
        var rastreador = new RastreadorDeFluxo(TimeProvider.System);
        using var provedor = new LoggerProviderComRedacao(capturador, Motor(), rastreador);

        provedor.CreateLogger("Teste").LogInformation("Seed: {Quantidade} leituras simuladas gravadas.", 8179);

        Assert.AreEqual("Seed: 8179 leituras simuladas gravadas.", capturador.Mensagens.Single());
        Assert.AreEqual(0, rastreador.Pendentes);
    }

    [TestMethod]
    public void Excecao_com_o_dado_na_mensagem_tambem_sai_mascarada()
    {
        var capturador = new ProvedorQueCaptura();
        using var provedor = new LoggerProviderComRedacao(capturador, Motor(), new RastreadorDeFluxo(TimeProvider.System));

        provedor.CreateLogger("Teste").LogError(new InvalidOperationException($"Falhou ao gravar o telefone 11 98765-4321"),
            "Erro ao gravar o relato.");

        var linha = capturador.Mensagens.Single();
        StringAssert.Contains(linha, "Falhou ao gravar o telefone [removido]");
        Assert.DoesNotContain("98765-4321", linha);
        Assert.IsNull(capturador.Excecoes.Single(), "A exceção vai já mascarada dentro do texto.");
    }

    [TestMethod]
    public async Task Antes_sem_o_guarda_o_log_de_depuracao_grava_o_cpf_cru()
    {
        var (conteudo, fluxosDeLog) = await RelatarComLogDeDepuracaoAsync(redigirLogs: false);

        StringAssert.Contains(conteudo, CpfFicticio);
        Assert.IsEmpty(fluxosDeLog, "Sem o guarda, nada barra o dado no log.");
    }

    [TestMethod]
    public async Task Depois_com_o_guarda_o_mesmo_log_grava_removido_e_o_fluxo_aparece_uma_vez()
    {
        var (conteudo, fluxosDeLog) = await RelatarComLogDeDepuracaoAsync(redigirLogs: true);

        Assert.DoesNotContain(CpfFicticio, conteudo);
        StringAssert.Contains(conteudo, "[removido]");
        StringAssert.Contains(conteudo, "<SAUDE>");
        // Console, depuração e arquivo recebem a mesma linha, mas o fluxo conta uma vez: CPF e saúde.
        CollectionAssert.AreEquivalent(new[] { "CPF", "SAUDE" }, fluxosDeLog);
    }

    /// <summary>
    /// Sobe a API com o log em arquivo e o log do corpo ligado, manda um relato e devolve o arquivo e as categorias
    /// dos fluxos que o guarda do log registrou.
    /// </summary>
    private static async Task<(string Conteudo, string[] FluxosDeLog)> RelatarComLogDeDepuracaoAsync(bool redigirLogs)
    {
        var pasta = Path.Combine(Path.GetTempPath(), "calmaria-log-" + Guid.NewGuid().ToString("N"));
        string[] fluxosDeLog;
        try
        {
            using (var fabrica = new CalmariaApiFactory(Agora, new Dictionary<string, string>
                   {
                       ["LogEmArquivo:Pasta"] = pasta,
                       ["Privacidade:LogarCorpoDasRequisicoes"] = "true",
                       ["Privacidade:RedigirLogs"] = redigirLogs ? "true" : "false"
                   }))
            {
                fabrica.PrepararBanco();
                using var cliente = fabrica.CreateClient();
                var usuario = await cliente.PostAsJsonAsync("/api/usuarios", new { });
                var id = (await usuario.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
                var relato = await cliente.PostAsJsonAsync("/api/relatos", new
                {
                    usuarioId = id, lugarId = 4, nota = 2, gatilhos = new[] { "BARULHO" },
                    comentario = $"Meu CPF é {CpfFicticio} e meu filho é autista.", visitadoEm = "2026-11-14T18:00:00-03:00"
                });
                Assert.AreEqual(HttpStatusCode.Created, relato.StatusCode);

                using var pedido = new HttpRequestMessage(HttpMethod.Get, "/api/privacidade/fluxos");
                pedido.Headers.Add("X-Admin-Key", CalmariaApiFactory.ChaveAdmin);
                var fluxos = await (await cliente.SendAsync(pedido)).Content.ReadFromJsonAsync<JsonElement>();
                fluxosDeLog = fluxos.EnumerateArray()
                    .Where(f => f.GetProperty("origem").GetString()!.StartsWith("log:Microsoft.AspNetCore.HttpLogging"))
                    .Select(f => f.GetProperty("categoria").GetString()!)
                    .ToArray();
            }

            var conteudo = string.Concat(Directory.GetFiles(pasta, "*.log").Select(File.ReadAllText));
            StringAssert.Contains(conteudo, "RequestBody", "O log de depuração do corpo devia ter gravado a requisição.");
            return (conteudo, fluxosDeLog);
        }
        finally
        {
            if (Directory.Exists(pasta))
                Directory.Delete(pasta, recursive: true);
        }
    }

    private sealed class ProvedorQueCaptura : ILoggerProvider
    {
        public List<string> Mensagens { get; } = [];
        public List<Exception?> Excecoes { get; } = [];

        public ILogger CreateLogger(string categoryName) => new Capturador(this);

        public void Dispose()
        {
        }

        private sealed class Capturador(ProvedorQueCaptura provedor) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                provedor.Mensagens.Add(formatter(state, exception));
                provedor.Excecoes.Add(exception);
            }
        }
    }
}
