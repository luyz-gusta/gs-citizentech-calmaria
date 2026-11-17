using System.ComponentModel.DataAnnotations;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Api.Sensorial;

/// <summary>Uma leitura enviada por HTTP, no mesmo formato da mensagem MQTT do ESP32.</summary>
/// <param name="DispositivoId">Identificador do aparelho, cadastrado na API (ex.: esp32-consolacao-01).</param>
/// <param name="LugarId">O lugar onde o aparelho está.</param>
/// <param name="SomDb">Som em decibéis, de 0 a 140.</param>
/// <param name="LuzLux">Luz em lux, de 0 a 100.000.</param>
/// <param name="MovimentosMin">Pessoas passando por minuto, de 0 a 500.</param>
/// <param name="Indice">Índice calculado no ESP32. Informativo: a API recalcula.</param>
/// <param name="Nivel">Nível calculado no ESP32. Informativo: a API recalcula.</param>
public sealed record LeituraRecebidaDto(
    [Required] string? DispositivoId,
    [Required] int? LugarId,
    [Required] double? SomDb,
    [Required] double? LuzLux,
    [Required] int? MovimentosMin,
    int? Indice,
    string? Nivel)
{
    public NovaLeitura ParaNovaLeitura() =>
        new(DispositivoId!.Trim(), LugarId!.Value, SomDb!.Value, LuzLux!.Value, MovimentosMin!.Value);
}

/// <summary>Os horários calmos de um lugar nos últimos 28 dias.</summary>
/// <param name="LugarId">O lugar.</param>
/// <param name="Suficiente">Falso com menos de 20 leituras; aí as listas vêm vazias.</param>
/// <param name="JanelasMaisCalmas">As 3 horas da semana com o menor índice médio, dentro do horário de funcionamento.</param>
/// <param name="Grade">O índice médio de cada dia da semana e hora com leitura.</param>
public sealed record HorariosCalmosDto(int LugarId, bool Suficiente, IReadOnlyList<JanelaDto> JanelasMaisCalmas,
    IReadOnlyList<JanelaDto> Grade)
{
    public static HorariosCalmosDto De(HorariosCalmos h) => new(h.LugarId, h.Suficiente,
        h.JanelasMaisCalmas.Select(JanelaDto.De).ToList(), h.Grade.Select(JanelaDto.De).ToList());
}

/// <summary>Uma hora de um dia da semana, no horário de São Paulo.</summary>
/// <param name="DiaSemana">DOMINGO a SABADO.</param>
/// <param name="Hora">Hora cheia, de 0 a 23.</param>
/// <param name="IndiceMedio">Índice de Sobrecarga Sensorial médio nessa hora.</param>
public sealed record JanelaDto(DiaSemana DiaSemana, int Hora, int IndiceMedio)
{
    public static JanelaDto De(JanelaCalma j) => new(j.DiaSemana, j.Hora, j.IndiceMedio);
}

/// <summary>Um refúgio (ou um lugar calmo agora) para onde o botão de crise pode levar.</summary>
public sealed record RefugioDto(int Id, string Nome, TipoLugar Tipo, double Latitude, double Longitude, string? Descricao)
{
    public static RefugioDto De(RefugioCandidato r) => new(r.Id, r.Nome, r.Tipo, r.Latitude, r.Longitude, r.Descricao);
}

/// <summary>O refúgio mais perto, a distância a pé e o link do trajeto no Google Maps.</summary>
/// <param name="Lugar">O refúgio fixo ou o lugar CALMO agora.</param>
/// <param name="DistanciaM">Distância em linha reta, em metros.</param>
/// <param name="MinutosAPe">Minutos a pé, a 80 metros por minuto, arredondados para cima.</param>
/// <param name="LinkTrajeto">Trajeto a pé no Google Maps.</param>
public sealed record RefugioMaisProximoDto(RefugioDto Lugar, int DistanciaM, int MinutosAPe, string LinkTrajeto)
{
    public static RefugioMaisProximoDto De(RefugioEscolhido r) =>
        new(RefugioDto.De(r.Lugar), r.DistanciaM, r.MinutosAPe, r.LinkTrajeto);
}
