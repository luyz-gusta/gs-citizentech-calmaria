using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class IndiceTests
{
    private readonly CalculadoraIndicePonderada calculadora = new(new IndiceOptions());

    [TestMethod]
    public void Exemplo_do_contrato_da_53_moderado()
    {
        // 78,5 dB, 1.450 lux e 12 movimentos por minuto (escopo global, seção 5.1).
        var indice = calculadora.Calcular(78.5, 1450, 12);

        Assert.AreEqual(53, indice.Valor);
        Assert.AreEqual(NivelSobrecarga.Moderado, indice.Nivel);
    }

    [TestMethod]
    public void Tudo_no_minimo_da_zero_calmo()
    {
        var indice = calculadora.Calcular(40, 300, 0);

        Assert.AreEqual(0, indice.Valor);
        Assert.AreEqual(NivelSobrecarga.Calmo, indice.Nivel);
    }

    [TestMethod]
    public void Tudo_no_maximo_da_cem_intenso()
    {
        var indice = calculadora.Calcular(100, 3000, 30);

        Assert.AreEqual(100, indice.Valor);
        Assert.AreEqual(NivelSobrecarga.Intenso, indice.Nivel);
    }

    [TestMethod]
    public void Medida_acima_da_faixa_conta_como_o_maximo()
    {
        Assert.AreEqual(100, calculadora.Calcular(140, 100_000, 500).Valor);
    }

    [TestMethod]
    public void Medida_abaixo_da_faixa_conta_como_o_minimo()
    {
        Assert.AreEqual(0, calculadora.Calcular(0, 0, 0).Valor);
    }

    [TestMethod]
    [DataRow(0, NivelSobrecarga.Calmo)]
    [DataRow(39, NivelSobrecarga.Calmo)]
    [DataRow(40, NivelSobrecarga.Moderado)]
    [DataRow(69, NivelSobrecarga.Moderado)]
    [DataRow(70, NivelSobrecarga.Intenso)]
    [DataRow(100, NivelSobrecarga.Intenso)]
    public void Nivel_respeita_as_bordas_das_faixas(int valor, NivelSobrecarga esperado)
    {
        Assert.AreEqual(esperado, new IndiceSobrecarga(valor).Nivel);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(101)]
    public void Indice_fora_de_0_a_100_e_recusado(int valor)
    {
        Assert.ThrowsExactly<DominioException>(() => new IndiceSobrecarga(valor));
    }
}
