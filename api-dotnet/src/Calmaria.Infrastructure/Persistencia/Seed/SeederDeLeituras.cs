using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Persistencia.Seed;

/// <summary>
/// Quatro semanas de leituras simuladas, a cada 15 minutos, para os lugares monitorados, dentro do horário de
/// funcionamento. Dá para os horários calmos e o mapa terem o que mostrar antes do ESP32. Tudo simulado (o README
/// diz isso), com semente fixa: a mesma base sai igual em qualquer computador.
/// </summary>
public static class SeederDeLeituras
{
    private const int Dias = 28;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

    /// <summary>Grava as leituras se a tabela estiver vazia. Devolve quantas gravou.</summary>
    public static async Task<int> SemearAsync(CalmariaDbContext db, ICalculadoraIndice calculadora, DateTime agoraUtc,
        CancellationToken ct = default)
    {
        if (await db.Leituras.AnyAsync(ct))
            return 0;

        var lugares = await db.Lugares.OfType<LugarMonitorado>().AsNoTracking().ToListAsync(ct);
        var dispositivos = await db.Dispositivos.ToListAsync(ct);
        var sorteio = new Random(42);
        var inicio = agoraUtc.AddDays(-Dias);
        var leituras = new List<Leitura>();

        foreach (var lugar in lugares)
        {
            var dispositivo = dispositivos.FirstOrDefault(d => d.LugarId == lugar.Id);
            if (dispositivo is null)
                continue;

            var funcionamento = HorarioDeFuncionamento.Ler(lugar.HorarioFuncionamento);
            for (var quando = inicio; quando <= agoraUtc; quando += Intervalo)
            {
                var local = HorarioDeSaoPaulo.Converter(quando);
                if (!funcionamento.Inclui(local.Hour))
                    continue;

                var f = Intensidade(lugar, local.DayOfWeek, local.Hour + local.Minute / 60.0);
                var som = Math.Clamp(50 + 42 * f + Ruido(sorteio, 4), 30, 120);
                var luz = Math.Clamp(400 + 2200 * f + Ruido(sorteio, 200), 50, 5000);
                var movimentos = (int)Math.Clamp(Math.Round(2 + 26 * f + Ruido(sorteio, 3)), 0, 60);

                leituras.Add(Leitura.Criar(dispositivo.Id, lugar.Id, Math.Round(som, 1), Math.Round(luz),
                    movimentos, calculadora, quando));
            }

            dispositivo.RegistrarLeitura(agoraUtc);
        }

        db.Leituras.AddRange(leituras);
        await db.SaveChangesAsync(ct);
        return leituras.Count;
    }

    /// <summary>De 0 (vazio e quieto) a 1 (lotado e barulhento), pelo tipo do lugar, o dia e a hora local.</summary>
    private static double Intensidade(LugarMonitorado lugar, DayOfWeek dia, double hora)
    {
        var fimDeSemana = dia is DayOfWeek.Saturday or DayOfWeek.Sunday;
        var f = lugar.Tipo switch
        {
            TipoLugar.Estacao when !fimDeSemana =>
                0.25 + 0.65 * Pico(hora, 8, 1.1) + 0.7 * Pico(hora, 18, 1.3) + 0.15 * Pico(hora, 13, 1.5),
            TipoLugar.Estacao => 0.2 + 0.35 * Pico(hora, 15, 3),
            TipoLugar.Mercado when dia == DayOfWeek.Saturday => 0.3 + 0.55 * Pico(hora, 13, 3.5),
            TipoLugar.Mercado when dia == DayOfWeek.Sunday => 0.2 + 0.4 * Pico(hora, 11.5, 2),
            TipoLugar.Mercado => 0.15 + 0.4 * Pico(hora, 12.5, 1.2) + 0.65 * Pico(hora, 18.5, 1.5),
            _ => 0.3
        };

        // As estações têm movimentos diferentes: a Consolação é a mais cheia, a Trianon-Masp a mais tranquila.
        var peso = lugar.Id switch { 1 => 1.0, 2 => 0.95, 3 => 0.8, _ => 1.0 };
        return Math.Clamp(f * peso, 0, 1);
    }

    private static double Pico(double hora, double centro, double largura) =>
        Math.Exp(-Math.Pow(hora - centro, 2) / (2 * largura * largura));

    private static double Ruido(Random sorteio, double amplitude) => (sorteio.NextDouble() * 2 - 1) * amplitude;
}
