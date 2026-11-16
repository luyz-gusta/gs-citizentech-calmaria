using System.Globalization;

namespace Calmaria.Domain.Sensorial;

/// <summary>Um lugar que pode receber a pessoa em crise: refúgio fixo ou lugar monitorado CALMO agora.</summary>
public sealed record RefugioCandidato(int Id, string Nome, TipoLugar Tipo, double Latitude, double Longitude,
    string? Descricao);

/// <summary>O refúgio escolhido para o botão de crise, com a distância e o caminho a pé.</summary>
public sealed record RefugioEscolhido(RefugioCandidato Lugar, int DistanciaM, int MinutosAPe, string LinkTrajeto);

/// <summary>A regra do botão de crise: o candidato mais perto, em linha reta.</summary>
public static class EscolhaDeRefugio
{
    /// <summary>Passo de quem anda devagar, o mesmo que o app usa sem internet.</summary>
    public const int MetrosPorMinuto = 80;

    public static RefugioEscolhido? MaisProximo(Coordenada origem, IEnumerable<RefugioCandidato> candidatos)
    {
        var maisPerto = candidatos
            .Select(c => new { Candidato = c, Distancia = origem.DistanciaEmMetrosAte(new Coordenada(c.Latitude, c.Longitude)) })
            .OrderBy(x => x.Distancia)
            .ThenBy(x => x.Candidato.Tipo == TipoLugar.Refugio ? 0 : 1)
            .FirstOrDefault();
        if (maisPerto is null)
            return null;

        var metros = (int)Math.Round(maisPerto.Distancia);
        var minutos = Math.Max(1, (int)Math.Ceiling(maisPerto.Distancia / MetrosPorMinuto));
        return new RefugioEscolhido(maisPerto.Candidato, metros, minutos, LinkAPe(maisPerto.Candidato));
    }

    /// <summary>Rota a pé no Google Maps até o lugar (sem o ponto de partida, que o celular já sabe).</summary>
    public static string LinkAPe(RefugioCandidato lugar) =>
        string.Create(CultureInfo.InvariantCulture,
            $"https://www.google.com/maps/dir/?api=1&destination={lugar.Latitude},{lugar.Longitude}&travelmode=walking");
}
