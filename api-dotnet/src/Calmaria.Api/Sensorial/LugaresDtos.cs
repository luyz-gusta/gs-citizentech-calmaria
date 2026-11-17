using System.ComponentModel.DataAnnotations;
using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Api.Sensorial;

/// <summary>Um lugar na lista. Sem leitura nos últimos 15 minutos, índice, nível e horário vêm nulos.</summary>
/// <param name="Id">Identificador do lugar.</param>
/// <param name="Nome">Nome do lugar.</param>
/// <param name="Tipo">MERCADO, ESTACAO, PARQUE, REFUGIO ou OUTRO.</param>
/// <param name="Latitude">Latitude em graus decimais.</param>
/// <param name="Longitude">Longitude em graus decimais.</param>
/// <param name="Distrito">Distrito da cidade, como no SP156 (ex.: CONSOLACAO).</param>
/// <param name="EhRefugio">Se é um refúgio calmo fixo, sem sensor.</param>
/// <param name="DistanciaM">Distância em metros até lat e lng; nula sem coordenada.</param>
/// <param name="IndiceAtual">Índice de Sobrecarga Sensorial de 0 a 100.</param>
/// <param name="NivelAtual">CALMO, MODERADO ou INTENSO.</param>
/// <param name="UltimaLeituraEm">Quando chegou a última leitura, no horário de São Paulo.</param>
public sealed record LugarResumoDto(
    int Id,
    string Nome,
    TipoLugar Tipo,
    double Latitude,
    double Longitude,
    string Distrito,
    bool EhRefugio,
    int? DistanciaM,
    int? IndiceAtual,
    NivelSobrecarga? NivelAtual,
    DateTimeOffset? UltimaLeituraEm)
{
    public static LugarResumoDto De(LugarResumo r) => new(r.Id, r.Nome, r.Tipo, r.Latitude, r.Longitude,
        r.Distrito, r.EhRefugio, r.DistanciaM, r.IndiceAtual, r.NivelAtual,
        HorarioDeSaoPaulo.Converter(r.UltimaLeituraEm));
}

/// <summary>Um lugar com todos os dados e a última leitura recebida (nula se nunca houve).</summary>
public sealed record LugarDetalheDto(
    int Id,
    string Nome,
    TipoLugar Tipo,
    string? Endereco,
    double Latitude,
    double Longitude,
    string Distrito,
    bool EhRefugio,
    string? Descricao,
    string? HorarioFuncionamento,
    LeituraDto? UltimaLeitura)
{
    public static LugarDetalheDto De(LugarDetalhe d) => new(d.Id, d.Nome, d.Tipo, d.Endereco, d.Latitude,
        d.Longitude, d.Distrito, d.EhRefugio, d.Descricao, d.HorarioFuncionamento,
        d.UltimaLeitura is null ? null : LeituraDto.De(d.UltimaLeitura));
}

/// <summary>Uma leitura do sensor, com o índice calculado pela API.</summary>
public sealed record LeituraDto(
    double SomDb,
    double LuzLux,
    int MovimentosMin,
    int Indice,
    NivelSobrecarga Nivel,
    DateTimeOffset RegistradaEm)
{
    public static LeituraDto De(LeituraResumo l) => new(l.SomDb, l.LuzLux, l.MovimentosMin, l.Indice, l.Nivel,
        HorarioDeSaoPaulo.Converter(l.RegistradaEm));
}

/// <summary>Os dados de um lugar para cadastrar (POST) ou atualizar (PUT).</summary>
/// <param name="Nome">Nome do lugar, de 2 a 120 caracteres.</param>
/// <param name="Tipo">MERCADO, ESTACAO, PARQUE, OUTRO ou REFUGIO (refúgio calmo fixo, sem sensor).</param>
/// <param name="Latitude">Latitude em graus decimais, de -90 a 90.</param>
/// <param name="Longitude">Longitude em graus decimais, de -180 a 180.</param>
/// <param name="Distrito">Distrito como no SP156; acento e minúscula são normalizados (Consolação vira CONSOLACAO).</param>
/// <param name="Endereco">Endereço, até 200 caracteres.</param>
/// <param name="HorarioFuncionamento">Faixa HH:mm-HH:mm (ex.: 07:00-23:00); vazio para quem nunca fecha.</param>
/// <param name="Descricao">Só para refúgio: o que ele oferece a quem está em sobrecarga. Até 300 caracteres.</param>
public sealed record LugarEntradaDto(
    [Required] [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter de 2 a 120 caracteres.")] string? Nome,
    [Required] TipoLugar? Tipo,
    [Required] [Range(-90.0, 90.0, ErrorMessage = "A latitude deve estar entre -90 e 90.")] double? Latitude,
    [Required] [Range(-180.0, 180.0, ErrorMessage = "A longitude deve estar entre -180 e 180.")] double? Longitude,
    [Required] [StringLength(60, ErrorMessage = "O distrito tem até 60 caracteres.")] string? Distrito,
    [StringLength(200, ErrorMessage = "O endereço tem até 200 caracteres.")] string? Endereco,
    [StringLength(20, ErrorMessage = "Use HH:mm-HH:mm.")] string? HorarioFuncionamento,
    [StringLength(300, ErrorMessage = "A descrição tem até 300 caracteres.")] string? Descricao)
{
    public DadosDoLugar ParaDados() => new(Nome!, Tipo!.Value, Latitude!.Value, Longitude!.Value, Distrito!, Endereco,
        HorarioFuncionamento, Descricao);
}
