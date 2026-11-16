namespace Calmaria.Domain.Sensorial;

/// <summary>Uma leitura que chegou do ESP32 (por MQTT ou HTTP), antes de virar entidade.</summary>
public sealed record NovaLeitura(string DispositivoId, int LugarId, double SomDb, double LuzLux, int MovimentosMin);

/// <summary>Recebe e consulta as leituras. O MQTT e o POST /api/leituras passam pelo mesmo caminho.</summary>
public interface ILeituraService
{
    /// <summary>Valida, calcula o índice e grava. Dispositivo desconhecido: 404; valor impossível: 422.</summary>
    Task<LeituraResumo> RegistrarAsync(NovaLeitura nova, CancellationToken ct = default);

    /// <summary>As leituras do lugar desde a data, da mais nova para a mais velha; nulo se o lugar não existe.</summary>
    Task<IReadOnlyList<LeituraResumo>?> ListarAsync(int lugarId, DateTime desdeUtc, CancellationToken ct = default);
}

public interface IHorarioCalmoService
{
    /// <summary>Os horários calmos dos últimos 28 dias; nulo se o lugar não existe.</summary>
    Task<HorariosCalmos?> CalcularAsync(int lugarId, CancellationToken ct = default);
}

public interface IRefugioService
{
    /// <summary>O refúgio fixo ou o lugar CALMO agora mais perto da pessoa; nulo se não há nenhum.</summary>
    Task<RefugioEscolhido?> MaisProximoAsync(Coordenada origem, CancellationToken ct = default);

    /// <summary>Todos os refúgios fixos, para o app guardar e usar sem internet.</summary>
    Task<IReadOnlyList<RefugioCandidato>> ListarFixosAsync(CancellationToken ct = default);
}
