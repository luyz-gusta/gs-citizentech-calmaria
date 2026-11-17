using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>
/// Um lugar do mapa. Abstrata: ou é monitorado por sensor (estação, mercado, parque) ou é um refúgio calmo
/// fixo. As duas filhas ficam na mesma tabela (TPH).
/// </summary>
public abstract class Lugar
{
    public int Id { get; protected set; }
    public string Nome { get; protected set; } = string.Empty;
    public TipoLugar Tipo { get; protected set; }
    public double Latitude { get; protected set; }
    public double Longitude { get; protected set; }
    public string? Endereco { get; protected set; }
    public string Distrito { get; protected set; } = string.Empty;

    /// <summary>Faixa no formato "HH:mm-HH:mm"; "00:00-24:00" para quem nunca fecha.</summary>
    public string? HorarioFuncionamento { get; protected set; }

    public Coordenada Coordenada => new(Latitude, Longitude);

    public bool EhRefugio => Tipo == TipoLugar.Refugio;

    protected Lugar() { }

    protected Lugar(int id, string nome, TipoLugar tipo, Coordenada coordenada, string distrito,
        string? endereco, string? horarioFuncionamento)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O lugar precisa de um nome.");
        if (string.IsNullOrWhiteSpace(distrito))
            throw new DominioException("O lugar precisa do distrito, que liga o lugar à previsão de barulho.");

        Id = id;
        Nome = nome.Trim();
        Tipo = tipo;
        Latitude = coordenada.Latitude;
        Longitude = coordenada.Longitude;
        Distrito = Sensorial.Distrito.Normalizar(distrito);
        Endereco = endereco;
        HorarioFuncionamento = horarioFuncionamento;
    }

    public double DistanciaAte(Coordenada origem) => Coordenada.DistanciaEmMetrosAte(origem);
}

/// <summary>Lugar com sensor: estação, mercado, parque. As leituras do ESP32 chegam para ele.</summary>
public sealed class LugarMonitorado : Lugar
{
    private LugarMonitorado() { }

    public LugarMonitorado(int id, string nome, TipoLugar tipo, Coordenada coordenada, string distrito,
        string? endereco = null, string? horarioFuncionamento = null)
        : base(id, nome, tipo, coordenada, distrito, endereco, horarioFuncionamento)
    {
        if (tipo == TipoLugar.Refugio)
            throw new DominioException("Refúgio é um lugar fixo, sem sensor: use Refugio.");
    }
}

/// <summary>Lugar calmo fixo, sem sensor, para onde o botão de crise leva.</summary>
public sealed class Refugio : Lugar
{
    /// <summary>O que o lugar oferece a quem está em sobrecarga (sombra, silêncio, banco para sentar).</summary>
    public string? Descricao { get; private set; }

    private Refugio() { }

    public Refugio(int id, string nome, Coordenada coordenada, string distrito, string? descricao = null,
        string? endereco = null, string? horarioFuncionamento = null)
        : base(id, nome, TipoLugar.Refugio, coordenada, distrito, endereco, horarioFuncionamento)
    {
        Descricao = descricao;
    }
}
