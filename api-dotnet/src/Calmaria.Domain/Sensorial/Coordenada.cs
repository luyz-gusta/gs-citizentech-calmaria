using Calmaria.Domain.Comum;

namespace Calmaria.Domain.Sensorial;

/// <summary>Ponto no mapa, em graus decimais. Recusa latitude e longitude impossíveis.</summary>
public sealed record Coordenada
{
    private const double RaioDaTerraEmMetros = 6_371_000;

    public double Latitude { get; }
    public double Longitude { get; }

    public Coordenada(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude is < -90 or > 90)
            throw new DominioException($"A latitude deve estar entre -90 e 90; veio {latitude}.");
        if (double.IsNaN(longitude) || longitude is < -180 or > 180)
            throw new DominioException($"A longitude deve estar entre -180 e 180; veio {longitude}.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Distância em linha reta pela fórmula de Haversine, em metros.</summary>
    public double DistanciaEmMetrosAte(Coordenada outra)
    {
        var lat1 = GrausParaRadianos(Latitude);
        var lat2 = GrausParaRadianos(outra.Latitude);
        var deltaLat = lat2 - lat1;
        var deltaLng = GrausParaRadianos(outra.Longitude - Longitude);

        var a = Math.Pow(Math.Sin(deltaLat / 2), 2)
              + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLng / 2), 2);
        return 2 * RaioDaTerraEmMetros * Math.Asin(Math.Sqrt(a));
    }

    private static double GrausParaRadianos(double graus) => graus * Math.PI / 180;
}
