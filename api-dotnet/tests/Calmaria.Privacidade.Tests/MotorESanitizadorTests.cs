using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Regras;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Tests;

[TestClass]
public sealed class MotorESanitizadorTests
{
    private static readonly string ArquivoDeRegras = Path.Combine(AppContext.BaseDirectory, "Regras", "regras-pii.json");

    private static MotorPii Motor() => new(
    [
        new RegraCpf(), new RegraTelefone(), new RegraEmail(), new RegraSaude(), new RegraSegredo(),
        new RegraLocalizacaoPrecisa(), new RegraQuaseIdentificador(), .. RegraConfiguravel.CarregarDe(ArquivoDeRegras)
    ]);

    [TestMethod]
    public void Texto_sem_dado_pessoal_passa_igual()
    {
        var resultado = new Sanitizador(Motor()).Sanitizar(new TextoNaoConfiavel("Muito cheio às 18h"));

        Assert.AreEqual("Muito cheio às 18h", resultado.Texto.Valor);
        Assert.IsFalse(resultado.TinhaDadoPessoal);
    }

    [TestMethod]
    public void Exemplo_da_auditoria_cpf_vira_removido_e_saude_vira_marca()
    {
        var texto = new TextoNaoConfiavel("Meu CPF é 529.982.247-25 e meu filho é autista.");

        var resultado = new Sanitizador(Motor()).Sanitizar(texto);

        Assert.AreEqual("Meu CPF é [removido] e meu filho é <SAUDE>.", resultado.Texto.Valor);
        CollectionAssert.AreEquivalent(
            new[] { CategoriaPii.Cpf, CategoriaPii.Saude },
            resultado.Removidos.Select(r => r.Categoria).ToArray());
        Assert.IsTrue(resultado.Removidos.Any(r => r.Trecho == "529.***.***-25"));
    }

    [TestMethod]
    public void Cpf_sem_pontuacao_nao_vira_telefone_porque_o_peso_maior_ganha()
    {
        // 52998224725 também casa com o padrão de telefone (DDD 52); o CPF pesa mais e fica.
        var achados = Motor().Analisar("documento 52998224725");

        Assert.AreEqual(CategoriaPii.Cpf, achados.Single().Categoria);
    }

    [TestMethod]
    public void Achados_saem_em_ordem_e_sem_sobreposicao()
    {
        var achados = Motor().Analisar("tel (11) 91234-5678, e-mail a@b.com, laudo de autismo");

        Assert.IsTrue(achados.Zip(achados.Skip(1)).All(par => par.First.Fim <= par.Second.Inicio));
        Assert.HasCount(4, achados);
    }

    [TestMethod]
    public void Texto_nao_confiavel_nao_mostra_o_conteudo_no_tostring()
    {
        var texto = new TextoNaoConfiavel("CPF 529.982.247-25");

        Assert.AreEqual("[texto não confiável]", texto.ToString());
        Assert.AreEqual("[texto não confiável]", $"{texto}");
    }

    [TestMethod]
    public void Coordenada_da_crise_e_arredondada_para_tres_casas()
    {
        var sanitizador = new Sanitizador(Motor());

        Assert.AreEqual(-23.561, sanitizador.ArredondarCoordenada(-23.56142));
        Assert.AreEqual(-46.656, sanitizador.ArredondarCoordenada(-46.65591));
    }

    [TestMethod]
    public void Regras_do_arquivo_sao_carregadas_e_os_exemplos_conferem()
    {
        var regras = RegraConfiguravel.CarregarDe(ArquivoDeRegras);

        Assert.IsGreaterThanOrEqualTo(2, regras.Count);
        foreach (var regra in regras)
        {
            Assert.IsGreaterThanOrEqualTo(3, regra.Exemplos.Positivos.Count, $"{regra.Id}: poucos positivos");
            Assert.IsGreaterThanOrEqualTo(3, regra.Exemplos.Negativos.Count, $"{regra.Id}: poucos negativos");
            foreach (var positivo in regra.Exemplos.Positivos)
                Assert.IsTrue(regra.Avaliar(positivo).Any(), $"{regra.Id} devia achar: {positivo}");
            foreach (var negativo in regra.Exemplos.Negativos)
                Assert.IsFalse(regra.Avaliar(negativo).Any(), $"{regra.Id} não devia achar: {negativo}");
        }
    }

    [TestMethod]
    public void Arquivo_que_nao_existe_nao_traz_regra_extra()
    {
        Assert.IsEmpty(RegraConfiguravel.CarregarDe(Path.Combine(AppContext.BaseDirectory, "nao-existe.json")));
    }
}
