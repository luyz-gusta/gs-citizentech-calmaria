using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Tests.Dominio;

[TestClass]
public sealed class HorariosCalmosTests
{
    [TestMethod]
    [DataRow("04:40-00:00", 5, true)]
    [DataRow("04:40-00:00", 23, true)]
    [DataRow("04:40-00:00", 4, false)]
    [DataRow("07:00-23:00", 22, true)]
    [DataRow("07:00-23:00", 23, false)]
    [DataRow("22:00-02:00", 23, true)]
    [DataRow("22:00-02:00", 1, true)]
    [DataRow("22:00-02:00", 2, false)]
    [DataRow("22:00-02:00", 12, false)]
    public void Horario_de_funcionamento_decide_cada_hora_cheia(string faixa, int hora, bool esperado)
    {
        Assert.AreEqual(esperado, HorarioDeFuncionamento.Ler(faixa).Inclui(hora));
    }

    [TestMethod]
    public void Sem_horario_conta_o_dia_todo()
    {
        var funcionamento = HorarioDeFuncionamento.Ler(null);

        Assert.IsTrue(Enumerable.Range(0, 24).All(funcionamento.Inclui));
    }

    [TestMethod]
    [DataRow("8h-18h")]
    [DataRow("08:00")]
    [DataRow("25:00-26:00")]
    public void Horario_mal_escrito_e_recusado(string faixa)
    {
        Assert.ThrowsExactly<DominioException>(() => HorarioDeFuncionamento.Ler(faixa));
    }

    [TestMethod]
    public void Menos_de_20_leituras_nao_e_suficiente()
    {
        var leituras = Enumerable.Range(0, 19).Select(i => new IndiceNoTempo(new DateTime(2026, 11, 10, 12, i, 0, DateTimeKind.Utc), 10)).ToList();

        var resultado = CalculadoraHorariosCalmos.Calcular(4, leituras, HorarioDeFuncionamento.DiaTodo);

        Assert.IsFalse(resultado.Suficiente);
        Assert.IsEmpty(resultado.JanelasMaisCalmas);
        Assert.IsEmpty(resultado.Grade);
    }

    [TestMethod]
    public void Escolhe_as_tres_horas_mais_calmas_no_horario_local_e_dentro_do_funcionamento()
    {
        // 10/11/2026 é terça. 12h UTC = 9h em São Paulo.
        var leituras = new List<IndiceNoTempo>();
        void Hora(int dia, int horaUtc, params int[] indices) =>
            leituras.AddRange(indices.Select((indice, i) =>
                new IndiceNoTempo(new DateTime(2026, 11, dia, horaUtc, i * 15, 0, DateTimeKind.Utc), indice)));

        Hora(10, 12, 20, 22, 24);        // terça, 9h: média 22
        Hora(11, 11, 23, 25, 27);        // quarta, 8h: média 25
        Hora(10, 17, 26, 28, 30);        // terça, 14h: média 28
        Hora(10, 6, 2, 4, 6);            // terça, 3h: média 4, mas fora do horário (06:00-22:00)
        Hora(10, 21, 60, 65, 70, 75);    // terça, 18h: cheio
        Hora(11, 21, 70, 75, 80, 85);    // quarta, 18h: cheio
        Hora(12, 14, 40, 45, 50);        // quinta, 11h

        var resultado = CalculadoraHorariosCalmos.Calcular(4, leituras, HorarioDeFuncionamento.Ler("06:00-22:00"));

        Assert.IsTrue(resultado.Suficiente);
        CollectionAssert.AreEqual(
            new[] { new JanelaCalma(DiaSemana.Terca, 9, 22), new JanelaCalma(DiaSemana.Quarta, 8, 25), new JanelaCalma(DiaSemana.Terca, 14, 28) },
            resultado.JanelasMaisCalmas.ToArray());
        Assert.IsTrue(resultado.Grade.Contains(new JanelaCalma(DiaSemana.Terca, 3, 4)), "A grade mostra todas as horas, até as fora do funcionamento.");
        Assert.HasCount(7, resultado.Grade);
    }
}
