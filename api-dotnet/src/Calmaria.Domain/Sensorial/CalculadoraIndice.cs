namespace Calmaria.Domain.Sensorial;

/// <summary>Calcula o ISS a partir das três medidas do sensor.</summary>
public interface ICalculadoraIndice
{
    IndiceSobrecarga Calcular(double somDb, double luzLux, int movimentosMin);
}

/// <summary>
/// Pesos e faixas do ISS. Os valores padrão são os do contrato (escopo global, seção 5.1); o ESP32 usa os
/// mesmos. Mudou aqui, muda no firmware.
/// </summary>
public sealed class IndiceOptions
{
    public const string Secao = "Indice";

    public double PesoSom { get; set; } = 0.5;
    public double PesoLuz { get; set; } = 0.25;
    public double PesoMovimento { get; set; } = 0.25;

    public double SomMinimoDb { get; set; } = 40;
    public double SomMaximoDb { get; set; } = 100;
    public double LuzMinimaLux { get; set; } = 300;
    public double LuzMaximaLux { get; set; } = 3000;
    public double MovimentosMaximosMin { get; set; } = 30;
}

/// <summary>
/// ISS = arredonda(100 x (pesoSom x som + pesoLuz x luz + pesoMov x mov)), cada medida normalizada entre 0 e 1.
/// O arredondamento afasta do zero, como o round() do ESP32, para os dois darem o mesmo número.
/// </summary>
public sealed class CalculadoraIndicePonderada(IndiceOptions opcoes) : ICalculadoraIndice
{
    public IndiceSobrecarga Calcular(double somDb, double luzLux, int movimentosMin)
    {
        var som = Normalizar(somDb, opcoes.SomMinimoDb, opcoes.SomMaximoDb);
        var luz = Normalizar(luzLux, opcoes.LuzMinimaLux, opcoes.LuzMaximaLux);
        var movimento = Normalizar(movimentosMin, 0, opcoes.MovimentosMaximosMin);

        var ponderado = opcoes.PesoSom * som + opcoes.PesoLuz * luz + opcoes.PesoMovimento * movimento;
        var valor = (int)Math.Round(100 * ponderado, MidpointRounding.AwayFromZero);
        return new IndiceSobrecarga(Math.Clamp(valor, 0, 100));
    }

    private static double Normalizar(double valor, double minimo, double maximo) =>
        Math.Clamp((valor - minimo) / (maximo - minimo), 0, 1);
}
