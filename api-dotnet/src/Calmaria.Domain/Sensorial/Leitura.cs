using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>O aparelho ESP32 instalado num lugar monitorado.</summary>
public sealed class Dispositivo
{
    /// <summary>Identificador gravado no firmware, como "esp32-consolacao-01".</summary>
    public string Id { get; private set; } = string.Empty;
    public int LugarId { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime? UltimaLeituraEm { get; private set; }

    private Dispositivo() { }

    public Dispositivo(string id, int lugarId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DominioException("O dispositivo precisa de um identificador.");
        Id = id.Trim();
        LugarId = lugarId;
    }

    public void RegistrarLeitura(DateTime registradaEmUtc) => UltimaLeituraEm = registradaEmUtc;
}

/// <summary>Uma medição do sensor, com o índice já calculado pela API (o valor do ESP32 é só informativo).</summary>
public sealed class Leitura
{
    public const double SomMaximoDb = 140;
    public const double LuzMaximaLux = 100_000;
    public const int MovimentosMaximosMin = 500;

    public long Id { get; private set; }
    public string DispositivoId { get; private set; } = string.Empty;
    public int LugarId { get; private set; }
    public double SomDb { get; private set; }
    public double LuzLux { get; private set; }
    public int MovimentosMin { get; private set; }
    public int Indice { get; private set; }
    public NivelSobrecarga Nivel { get; private set; }

    /// <summary>Quando a API recebeu a leitura, em UTC. O ESP32 não tem relógio.</summary>
    public DateTime RegistradaEm { get; private set; }

    private Leitura() { }

    /// <summary>
    /// Cria a leitura e recusa valor fisicamente impossível: som de 0 a 140 dB, luz de 0 a 100.000 lux e
    /// movimentos de 0 a 500 por minuto.
    /// </summary>
    public static Leitura Criar(string dispositivoId, int lugarId, double somDb, double luzLux,
        int movimentosMin, ICalculadoraIndice calculadora, DateTime registradaEmUtc)
    {
        Validar(nameof(SomDb), somDb, 0, SomMaximoDb);
        Validar(nameof(LuzLux), luzLux, 0, LuzMaximaLux);
        Validar(nameof(MovimentosMin), movimentosMin, 0, MovimentosMaximosMin);

        var indice = calculadora.Calcular(somDb, luzLux, movimentosMin);
        return new Leitura
        {
            DispositivoId = dispositivoId,
            LugarId = lugarId,
            SomDb = somDb,
            LuzLux = luzLux,
            MovimentosMin = movimentosMin,
            Indice = indice.Valor,
            Nivel = indice.Nivel,
            RegistradaEm = DateTime.SpecifyKind(registradaEmUtc, DateTimeKind.Utc)
        };
    }

    private static void Validar(string campo, double valor, double minimo, double maximo)
    {
        if (double.IsNaN(valor) || valor < minimo || valor > maximo)
            throw new ValorForaDaFaixaException(campo, valor, minimo, maximo);
    }
}

/// <summary>Reclamações de barulho previstas pelo modelo de ML para um distrito num dia.</summary>
public sealed class PrevisaoBarulho
{
    public long Id { get; private set; }
    public string Distrito { get; private set; } = string.Empty;
    public DateOnly Data { get; private set; }
    public double ReclamacoesPrevistas { get; private set; }
    public NivelPrevisao Nivel { get; private set; }

    private PrevisaoBarulho() { }

    public PrevisaoBarulho(string distrito, DateOnly data, double reclamacoesPrevistas, NivelPrevisao nivel)
    {
        if (string.IsNullOrWhiteSpace(distrito))
            throw new DominioException("A previsão precisa do distrito.");

        Distrito = Sensorial.Distrito.Normalizar(distrito);
        Data = data;
        Atualizar(reclamacoesPrevistas, nivel);
    }

    /// <summary>Uma importação nova do CSV troca a previsão do mesmo distrito e dia, em vez de duplicar.</summary>
    public void Atualizar(double reclamacoesPrevistas, NivelPrevisao nivel)
    {
        if (double.IsNaN(reclamacoesPrevistas) || reclamacoesPrevistas < 0)
            throw new DominioException("O número de reclamações previstas não pode ser negativo.");
        ReclamacoesPrevistas = reclamacoesPrevistas;
        Nivel = nivel;
    }
}
