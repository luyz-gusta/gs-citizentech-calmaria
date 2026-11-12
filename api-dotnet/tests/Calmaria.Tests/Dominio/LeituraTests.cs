using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class LeituraTests
{
    private static readonly CalculadoraIndicePonderada Calculadora = new(new IndiceOptions());
    private static readonly DateTime Agora = new(2026, 11, 16, 21, 5, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Leitura_valida_recebe_o_indice_calculado_pela_api()
    {
        var leitura = Leitura.Criar("esp32-consolacao-01", 1, 78.5, 1450, 12, Calculadora, Agora);

        Assert.AreEqual(53, leitura.Indice);
        Assert.AreEqual(NivelSobrecarga.Moderado, leitura.Nivel);
        Assert.AreEqual(DateTimeKind.Utc, leitura.RegistradaEm.Kind);
    }

    [TestMethod]
    public void Som_negativo_e_recusado()
    {
        var erro = Assert.ThrowsExactly<ValorForaDaFaixaException>(
            () => Leitura.Criar("esp32-consolacao-01", 1, -1, 500, 5, Calculadora, Agora));

        Assert.AreEqual(nameof(Leitura.SomDb), erro.Campo);
    }

    [TestMethod]
    public void Luz_acima_de_cem_mil_lux_e_recusada()
    {
        var erro = Assert.ThrowsExactly<ValorForaDaFaixaException>(
            () => Leitura.Criar("esp32-consolacao-01", 1, 60, 100_001, 5, Calculadora, Agora));

        Assert.AreEqual(nameof(Leitura.LuzLux), erro.Campo);
    }

    [TestMethod]
    public void Movimentos_acima_de_500_sao_recusados()
    {
        Assert.ThrowsExactly<ValorForaDaFaixaException>(
            () => Leitura.Criar("esp32-consolacao-01", 1, 60, 500, 501, Calculadora, Agora));
    }

    [TestMethod]
    public void Limites_da_faixa_fisica_sao_aceitos()
    {
        var leitura = Leitura.Criar("esp32-consolacao-01", 1, 140, 100_000, 500, Calculadora, Agora);

        Assert.AreEqual(100, leitura.Indice);
    }
}
