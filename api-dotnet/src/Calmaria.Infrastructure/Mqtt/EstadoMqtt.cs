using System.Collections.Concurrent;

namespace Calmaria.Infrastructure.Mqtt;

/// <summary>Configuração do consumidor MQTT (seção "Mqtt" do appsettings ou variáveis Mqtt__*).</summary>
public sealed class MqttOptions
{
    public const string Secao = "Mqtt";

    public bool Habilitado { get; set; }
    public string Host { get; set; } = "broker.hivemq.com";
    public int Porta { get; set; } = 1883;
    public string Topico { get; set; } = "calmaria/v1/lugares/+/leituras";

    /// <summary>Quantas mensagens ficam em memória para a varredura de privacidade conferir o broker público.</summary>
    public int MensagensGuardadas { get; set; } = 200;
}

/// <summary>Uma mensagem recebida, guardada só em memória.</summary>
public sealed record MensagemRecebida(string Topico, string Payload, DateTime RecebidaEmUtc);

/// <summary>
/// O estado do consumidor MQTT, para o /health e para o módulo de Privacidade: se está conectado e as últimas
/// mensagens recebidas (no máximo <see cref="MqttOptions.MensagensGuardadas"/>).
/// </summary>
public sealed class EstadoMqtt(TimeProvider relogio)
{
    private readonly ConcurrentQueue<MensagemRecebida> mensagens = new();

    public bool Conectado { get; private set; }
    public DateTime? ConectadoDesdeUtc { get; private set; }
    public string? UltimoErro { get; private set; }

    public IReadOnlyCollection<MensagemRecebida> UltimasMensagens => mensagens.ToArray();

    public void MarcarConectado()
    {
        Conectado = true;
        ConectadoDesdeUtc = relogio.GetUtcNow().UtcDateTime;
        UltimoErro = null;
    }

    public void MarcarDesconectado(string motivo)
    {
        Conectado = false;
        ConectadoDesdeUtc = null;
        UltimoErro = motivo;
    }

    public void Guardar(string topico, string payload, int limite)
    {
        mensagens.Enqueue(new MensagemRecebida(topico, payload, relogio.GetUtcNow().UtcDateTime));
        while (mensagens.Count > limite && mensagens.TryDequeue(out _))
        {
        }
    }
}
