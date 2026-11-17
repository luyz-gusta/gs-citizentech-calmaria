namespace Calmaria.Domain.Sensorial;

/// <summary>
/// A porta do módulo Sensorial para os outros módulos. A Comunidade pergunta por aqui se um lugar existe,
/// sem ler a tabela de lugares direto.
/// </summary>
public interface ILugarConsulta
{
    Task<bool> ExisteAsync(int lugarId, CancellationToken ct = default);
}

/// <summary>Consultas de lugares que a API expõe.</summary>
public interface ILugarService
{
    /// <summary>Todos os lugares, ou os que ficam dentro do raio da origem, do mais perto ao mais longe.</summary>
    Task<IReadOnlyList<LugarResumo>> ListarAsync(FiltroLugares filtro, CancellationToken ct = default);

    /// <summary>Um lugar com a última leitura; nulo se não existir.</summary>
    Task<LugarDetalhe?> ObterAsync(int id, CancellationToken ct = default);
}

/// <summary>O cadastro de lugares (uso da equipe, com a chave de administrador).</summary>
public interface ILugarCadastro
{
    /// <summary>Cria um lugar monitorado ou, com o tipo REFUGIO, um refúgio.</summary>
    Task<LugarDetalhe> CriarAsync(DadosDoLugar dados, CancellationToken ct = default);

    /// <summary>Atualiza o cadastro; nulo se o lugar não existe. Refúgio não vira monitorado, nem o contrário.</summary>
    Task<LugarDetalhe?> AtualizarAsync(int id, DadosDoLugar dados, CancellationToken ct = default);

    /// <summary>Apaga o lugar; falso se não existe. Lugar com sensor ou com relatos não se apaga.</summary>
    Task<bool> ApagarAsync(int id, CancellationToken ct = default);
}

/// <summary>Os dados de um lugar para cadastrar ou atualizar. A descrição só vale para refúgio.</summary>
public sealed record DadosDoLugar(string Nome, TipoLugar Tipo, double Latitude, double Longitude, string Distrito,
    string? Endereco, string? HorarioFuncionamento, string? Descricao);

/// <param name="Origem">Onde a pessoa está; sem ela, não há filtro de raio nem distância.</param>
/// <param name="RaioM">Raio em metros, usado só com origem.</param>
/// <param name="Tipo">Filtra por tipo de lugar.</param>
public sealed record FiltroLugares(Coordenada? Origem = null, int RaioM = FiltroLugares.RaioPadraoM, TipoLugar? Tipo = null)
{
    public const int RaioPadraoM = 1500;
    public const int RaioMaximoM = 50_000;
}

/// <summary>Um lugar na lista, com o nível atual quando há leitura recente.</summary>
public sealed record LugarResumo(
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
    DateTime? UltimaLeituraEm);

/// <summary>Um lugar com todos os dados e a última leitura recebida.</summary>
public sealed record LugarDetalhe(
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
    LeituraResumo? UltimaLeitura);

/// <summary>As medidas de uma leitura, sem identificadores internos.</summary>
public sealed record LeituraResumo(
    double SomDb,
    double LuzLux,
    int MovimentosMin,
    int Indice,
    NivelSobrecarga Nivel,
    DateTime RegistradaEm);
