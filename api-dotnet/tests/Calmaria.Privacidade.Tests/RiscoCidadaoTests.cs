using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Risco;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Tests;

[TestClass]
public sealed class RiscoCidadaoTests
{
    private static readonly CalculadoraRiscoCidadao Calculadora = new();

    [TestMethod]
    public void Sem_nenhum_item_o_risco_e_zero_e_baixo()
    {
        var risco = Calculadora.Calcular([]);

        Assert.AreEqual(0, risco.Indice);
        Assert.AreEqual(FaixaRisco.Baixo, risco.Faixa);
        Assert.IsEmpty(risco.PorCategoria);
    }

    [TestMethod]
    public void Tudo_mitigado_fica_em_20_que_ainda_e_baixo()
    {
        var risco = Calculadora.Calcular(
        [
            new ItemDeRisco(CategoriaPii.Cpf, 4, DestinoFluxo.RespostaPublica, Mitigado: true),
            new ItemDeRisco(CategoriaPii.Saude, 5, DestinoFluxo.Log, Mitigado: true)
        ]);

        Assert.AreEqual(20, risco.Indice);
        Assert.AreEqual(FaixaRisco.Baixo, risco.Faixa);
        Assert.AreEqual(2, risco.Mitigados);
    }

    [TestMethod]
    public void Nada_mitigado_e_100()
    {
        var risco = Calculadora.Calcular([new ItemDeRisco(CategoriaPii.Segredo, 5, DestinoFluxo.Arquivo, Mitigado: false)]);

        Assert.AreEqual(100, risco.Indice);
        Assert.AreEqual(FaixaRisco.Alto, risco.Faixa);
    }

    [TestMethod]
    public void Formula_do_briefing_com_um_exposto_e_um_mitigado()
    {
        // CPF exposto no log: 4 x 2 x 1 = 8. Saúde mitigada na resposta pública: 5 x 3 x 0,2 = 3.
        // Máximo: 8 + 15 = 23. Risco Cidadão = 100 x 11 / 23 = 47,8, arredonda para 48 (médio).
        var risco = Calculadora.Calcular(
        [
            new ItemDeRisco(CategoriaPii.Cpf, 4, DestinoFluxo.Log, Mitigado: false),
            new ItemDeRisco(CategoriaPii.Saude, 5, DestinoFluxo.RespostaPublica, Mitigado: true)
        ]);

        Assert.AreEqual(48, risco.Indice);
        Assert.AreEqual(FaixaRisco.Medio, risco.Faixa);
        Assert.AreEqual(11, risco.Pontos);
        Assert.AreEqual(23, risco.PontosMaximos);
        Assert.AreEqual(CategoriaPii.Cpf, risco.PorCategoria[0].Categoria, "O CPF exposto pesa mais que a saúde mitigada.");
        Assert.AreEqual(DestinoFluxo.Log, risco.PorDestino[0].Destino);
    }

    [TestMethod]
    [DataRow(0, FaixaRisco.Baixo)]
    [DataRow(20, FaixaRisco.Baixo)]
    [DataRow(21, FaixaRisco.Medio)]
    [DataRow(50, FaixaRisco.Medio)]
    [DataRow(51, FaixaRisco.Alto)]
    [DataRow(100, FaixaRisco.Alto)]
    public void Faixas_do_indicador(int indice, FaixaRisco faixa)
    {
        Assert.AreEqual(faixa, CalculadoraRiscoCidadao.Faixa(indice));
    }

    [TestMethod]
    [DataRow(DestinoFluxo.BrokerMqtt, 3)]
    [DataRow(DestinoFluxo.RespostaPublica, 3)]
    [DataRow(DestinoFluxo.Llm, 3)]
    [DataRow(DestinoFluxo.Log, 2)]
    [DataRow(DestinoFluxo.Arquivo, 2)]
    [DataRow(DestinoFluxo.Banco, 1)]
    public void Peso_da_exposicao_por_destino(DestinoFluxo destino, int peso)
    {
        Assert.AreEqual(peso, PesoDaExposicao.De(destino));
    }
}

[TestClass]
public sealed class RastreadorDeFluxoTests
{
    [TestMethod]
    public void Drenar_devolve_os_eventos_e_esvazia_a_fila()
    {
        var rastreador = new RastreadorDeFluxo(TimeProvider.System);
        rastreador.Registrar("relato.comentario", DestinoFluxo.RespostaPublica, CategoriaPii.Cpf, 4, bloqueado: true);
        rastreador.Registrar("log:Teste", DestinoFluxo.Log, CategoriaPii.Saude, 5, bloqueado: true);

        var eventos = rastreador.Drenar();

        Assert.HasCount(2, eventos);
        Assert.AreEqual("relato.comentario", eventos[0].Origem);
        Assert.AreEqual(0, rastreador.Pendentes);
        Assert.IsEmpty(rastreador.Drenar());
    }

    [TestMethod]
    public void Fila_cheia_descarta_os_mais_velhos()
    {
        var rastreador = new RastreadorDeFluxo(TimeProvider.System);
        for (var i = 0; i < RastreadorDeFluxo.Limite + 5; i++)
            rastreador.Registrar($"origem {i}", DestinoFluxo.Log, CategoriaPii.Email, 3, bloqueado: true);

        var eventos = rastreador.Drenar();

        Assert.HasCount(RastreadorDeFluxo.Limite, eventos);
        Assert.AreEqual("origem 5", eventos[0].Origem);
    }
}
