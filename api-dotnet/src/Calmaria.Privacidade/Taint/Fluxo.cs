using System.Collections.Concurrent;
using Calmaria.Privacidade.Motor;

namespace Calmaria.Privacidade.Taint;

/// <summary>
/// Para onde o dado do cidadão estava indo. Cada destino tem um peso de exposição no Risco Cidadão
/// (<see cref="Risco.PesoDaExposicao"/>).
/// </summary>
public enum DestinoFluxo
{
    /// <summary>O log da API (arquivo e console).</summary>
    Log,

    /// <summary>Resposta de rota pública, como a listagem de relatos.</summary>
    RespostaPublica,

    /// <summary>O LLM externo da tradução de risco.</summary>
    Llm,

    /// <summary>O broker MQTT público (broker.hivemq.com).</summary>
    BrokerMqtt,

    /// <summary>O banco da API.</summary>
    Banco,

    /// <summary>Arquivos do repositório e o que se exporta deles (o ZIP da entrega).</summary>
    Arquivo
}

/// <summary>
/// Um dado do cidadão que chegou a um destino inseguro. <paramref name="Bloqueado"/> diz se um guarda tirou o
/// dado antes de ele chegar lá. Não guarda o texto, só a categoria.
/// </summary>
/// <param name="Origem">De onde veio, sem dado pessoal (ex.: relato.comentario, log:Microsoft.AspNetCore...).</param>
/// <param name="Destino">Para onde ia.</param>
/// <param name="Categoria">O tipo de dado.</param>
/// <param name="Peso">O peso do dado, de 1 a 5.</param>
/// <param name="Bloqueado">Se o guarda mascarou o dado antes de ele sair.</param>
/// <param name="OcorridoEmUtc">Quando.</param>
public sealed record EventoDeFluxo(string Origem, DestinoFluxo Destino, CategoriaPii Categoria, int Peso,
    bool Bloqueado, DateTime OcorridoEmUtc);

/// <summary>
/// Registra cada vez que um guarda de saída barra um dado do cidadão. Os guardas (log, resposta pública, relato)
/// rodam fora do banco, então os eventos ficam em memória até a próxima varredura ou consulta gravá-los.
/// </summary>
public interface IRastreadorDeFluxo
{
    void Registrar(string origem, DestinoFluxo destino, CategoriaPii categoria, int peso, bool bloqueado);

    /// <summary>Devolve os eventos ainda não gravados e esvazia a fila.</summary>
    IReadOnlyList<EventoDeFluxo> Drenar();

    int Pendentes { get; }
}

/// <summary>Atalhos para registrar todos os achados de uma vez.</summary>
public static class RastreadorDeFluxoExtensoes
{
    public static void Registrar(this IRastreadorDeFluxo rastreador, string origem, DestinoFluxo destino,
        IEnumerable<Achado> achados, bool bloqueado)
    {
        foreach (var achado in achados)
            rastreador.Registrar(origem, destino, achado.Categoria, achado.Peso, bloqueado);
    }

    public static void Registrar(this IRastreadorDeFluxo rastreador, string origem, DestinoFluxo destino,
        IEnumerable<AchadoMascarado> achados, bool bloqueado)
    {
        foreach (var achado in achados)
            rastreador.Registrar(origem, destino, achado.Categoria, achado.Peso, bloqueado);
    }
}

/// <summary>
/// A fila em memória dos eventos de fluxo, segura para várias threads. Guarda no máximo <see cref="Limite"/>
/// eventos: se ninguém drenar, os mais velhos saem primeiro.
/// </summary>
public sealed class RastreadorDeFluxo(TimeProvider relogio) : IRastreadorDeFluxo
{
    public const int Limite = 1000;

    private readonly ConcurrentQueue<EventoDeFluxo> fila = new();

    public int Pendentes => fila.Count;

    public void Registrar(string origem, DestinoFluxo destino, CategoriaPii categoria, int peso, bool bloqueado)
    {
        fila.Enqueue(new EventoDeFluxo(origem, destino, categoria, peso, bloqueado, relogio.GetUtcNow().UtcDateTime));
        while (fila.Count > Limite && fila.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<EventoDeFluxo> Drenar()
    {
        var eventos = new List<EventoDeFluxo>();
        while (fila.TryDequeue(out var evento))
            eventos.Add(evento);
        return eventos;
    }
}
