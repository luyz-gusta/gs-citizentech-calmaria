using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>O Índice de Sobrecarga Sensorial (ISS), de 0 a 100, que sabe o próprio nível.</summary>
public readonly record struct IndiceSobrecarga
{
    public const int LimiteModerado = 40;
    public const int LimiteIntenso = 70;

    public int Valor { get; }

    public IndiceSobrecarga(int valor)
    {
        if (valor is < 0 or > 100)
            throw new DominioException($"O índice deve estar entre 0 e 100; veio {valor}.");
        Valor = valor;
    }

    public NivelSobrecarga Nivel => Valor switch
    {
        >= LimiteIntenso => NivelSobrecarga.Intenso,
        >= LimiteModerado => NivelSobrecarga.Moderado,
        _ => NivelSobrecarga.Calmo
    };
}
