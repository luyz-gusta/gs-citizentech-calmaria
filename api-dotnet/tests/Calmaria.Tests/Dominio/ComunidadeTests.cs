using Calmaria.Domain.Comum;
using Calmaria.Domain.Comunidade;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class ComunidadeTests
{
    private static readonly DateTime Agora = new(2026, 11, 15, 19, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Nota_fora_de_1_a_5_e_recusada(int nota)
    {
        Assert.ThrowsExactly<DominioException>(() => new NotaSensorial(nota));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Sensibilidade_fora_de_1_a_5_e_recusada(int valor)
    {
        Assert.ThrowsExactly<DominioException>(() => new Sensibilidade(valor, "som"));
    }

    [TestMethod]
    public void Visita_no_futuro_e_recusada_mas_cinco_minutos_de_relogio_adiantado_passam()
    {
        Assert.ThrowsExactly<DominioException>(() =>
            Relato.Criar(Guid.NewGuid(), 1, new NotaSensorial(3), [], null, Agora.AddHours(1), Agora));

        var relato = Relato.Criar(Guid.NewGuid(), 1, new NotaSensorial(3), [], null, Agora.AddMinutes(4), Agora);
        Assert.AreEqual(3, relato.Nota);
    }

    [TestMethod]
    public void Gatilho_repetido_conta_uma_vez()
    {
        var relato = Relato.Criar(Guid.NewGuid(), 1, new NotaSensorial(2),
            [Gatilho.Barulho, Gatilho.Barulho, Gatilho.Fila], null, Agora.AddHours(-1), Agora);

        Assert.HasCount(2, relato.Gatilhos);
    }

    [TestMethod]
    [DataRow("Consolação", "CONSOLACAO")]
    [DataRow("  bela   vista ", "BELA VISTA")]
    [DataRow("JARDIM PAULISTA", "JARDIM PAULISTA")]
    public void Distrito_fica_como_no_sp156(string entrada, string esperado)
    {
        Assert.AreEqual(esperado, Distrito.Normalizar(entrada));
    }
}
