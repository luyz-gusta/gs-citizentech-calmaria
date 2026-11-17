using System.Globalization;
using System.Text;
using System.Text.Json;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;

namespace Calmaria.Privacidade.Tests;

/// <summary>
/// A robustez do motor medida numa amostra rotulada: 60 comentários fictícios, cada um com as regras que deveriam
/// disparar. Para cada regra, precisão (do que o motor marcou, quanto estava certo) e revocação (do que deveria ser
/// marcado, quanto o motor pegou). Conta por comentário: a regra disparou ou não. A tabela sai na saída do teste.
/// </summary>
[TestClass]
public sealed class AmostraRotuladaTests
{
    private static readonly string ArquivoDeRegras = Path.Combine(AppContext.BaseDirectory, "Regras", "regras-pii.json");

    public TestContext TestContext { get; set; } = null!;

    private sealed record Item(string Texto, string[] Esperado);

    private sealed record Amostra(string Aviso, Item[] Itens);

    private sealed record Metrica(string Regra, int Verdadeiros, int FalsosPositivos, int FalsosNegativos)
    {
        public double Precisao => Verdadeiros + FalsosPositivos == 0 ? 1 : (double)Verdadeiros / (Verdadeiros + FalsosPositivos);
        public double Revocacao => Verdadeiros + FalsosNegativos == 0 ? 1 : (double)Verdadeiros / (Verdadeiros + FalsosNegativos);
    }

    private static MotorPii Motor() => new(
    [
        new RegraCpf(), new RegraTelefone(), new RegraEmail(), new RegraSaude(), new RegraSegredo(),
        new RegraLocalizacaoPrecisa(), new RegraQuaseIdentificador(), .. RegraConfiguravel.CarregarDe(ArquivoDeRegras)
    ]);

    private static Amostra Ler() =>
        JsonSerializer.Deserialize<Amostra>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "AmostraRotulada.json")),
            JsonSerializerOptions.Web)!;

    [TestMethod]
    public void Amostra_tem_60_comentarios_e_um_terco_sem_dado_pessoal()
    {
        var amostra = Ler();

        Assert.HasCount(60, amostra.Itens);
        Assert.AreEqual(20, amostra.Itens.Count(i => i.Esperado.Length == 0));
    }

    [TestMethod]
    public void Precisao_e_revocacao_por_regra_na_amostra_rotulada()
    {
        var motor = Motor();
        var itens = Ler().Itens;
        var regras = motor.RegrasAtivas;
        var contagem = regras.ToDictionary(r => r, _ => (Vp: 0, Fp: 0, Fn: 0));
        var erros = new List<string>();

        for (var n = 0; n < itens.Length; n++)
        {
            var achadas = motor.Analisar(itens[n].Texto).Select(a => a.RegraId).ToHashSet();
            var esperadas = itens[n].Esperado.ToHashSet();
            foreach (var regra in regras)
            {
                var (vp, fp, fn) = contagem[regra];
                var achou = achadas.Contains(regra);
                var devia = esperadas.Contains(regra);
                contagem[regra] = (vp + (achou && devia ? 1 : 0), fp + (achou && !devia ? 1 : 0), fn + (!achou && devia ? 1 : 0));
                if (achou != devia)
                    erros.Add($"item {n + 1}: {(achou ? "falso positivo" : "falso negativo")} de {regra}");
            }
        }

        var metricas = contagem.Select(c => new Metrica(c.Key, c.Value.Vp, c.Value.Fp, c.Value.Fn)).ToList();
        var total = new Metrica("TOTAL", metricas.Sum(m => m.Verdadeiros), metricas.Sum(m => m.FalsosPositivos),
            metricas.Sum(m => m.FalsosNegativos));
        TestContext.WriteLine(Tabela(metricas.Append(total)));
        foreach (var erro in erros)
            TestContext.WriteLine(erro);

        // O piso do critério de robustez: nenhuma regra abaixo de 90% em precisão nem em revocação.
        foreach (var m in metricas)
        {
            Assert.IsGreaterThanOrEqualTo(0.9, m.Precisao, $"Precisão de {m.Regra}: {m.Precisao:P0}");
            Assert.IsGreaterThanOrEqualTo(0.9, m.Revocacao, $"Revocação de {m.Regra}: {m.Revocacao:P0}");
        }
    }

    private static string Tabela(IEnumerable<Metrica> metricas)
    {
        var pt = CultureInfo.GetCultureInfo("pt-BR");
        var texto = new StringBuilder();
        texto.AppendLine("| Regra | VP | FP | FN | Precisão | Revocação |");
        texto.AppendLine("|---|---|---|---|---|---|");
        foreach (var m in metricas)
            texto.AppendLine(string.Create(pt,
                $"| {m.Regra} | {m.Verdadeiros} | {m.FalsosPositivos} | {m.FalsosNegativos} | {m.Precisao:P0} | {m.Revocacao:P0} |"));
        return texto.ToString();
    }
}
