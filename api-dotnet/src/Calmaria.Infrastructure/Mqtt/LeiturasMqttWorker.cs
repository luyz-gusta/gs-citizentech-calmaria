using Calmaria.Domain.Comum;
using Calmaria.Domain.Sensorial;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Calmaria.Infrastructure.Mqtt;

/// <summary>
/// Assina as leituras do ESP32 no broker MQTT e grava cada uma pelo mesmo serviço do POST /api/leituras.
/// Reconecta sozinho, com espera que dobra a cada falha (até 1 minuto).
/// </summary>
internal sealed class LeiturasMqttWorker(
    IOptions<MqttOptions> opcoes,
    IServiceScopeFactory escopos,
    EstadoMqtt estado,
    ILogger<LeiturasMqttWorker> log) : BackgroundService
{
    private static readonly TimeSpan IntervaloDeConferencia = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan EsperaMaxima = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var config = opcoes.Value;
        using var cliente = new MqttClientFactory().CreateMqttClient();
        cliente.ApplicationMessageReceivedAsync += e => ProcessarAsync(e, ct);

        var conexao = new MqttClientOptionsBuilder()
            .WithTcpServer(config.Host, config.Porta)
            .WithClientId($"calmaria-api-{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();
        var assinatura = new MqttClientSubscribeOptionsBuilder().WithTopicFilter(config.Topico).Build();

        var espera = TimeSpan.FromSeconds(1);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!cliente.IsConnected)
                {
                    estado.MarcarDesconectado(estado.UltimoErro ?? "conectando");
                    await cliente.ConnectAsync(conexao, ct);
                    await cliente.SubscribeAsync(assinatura, ct);
                    estado.MarcarConectado();
                    log.LogInformation("MQTT conectado em {Host}:{Porta}, assinando {Topico}.",
                        config.Host, config.Porta, config.Topico);
                    espera = TimeSpan.FromSeconds(1);
                }

                await Task.Delay(IntervaloDeConferencia, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                estado.MarcarDesconectado(ex.Message);
                log.LogWarning("MQTT sem conexão ({Motivo}); nova tentativa em {Segundos} s.",
                    ex.Message, espera.TotalSeconds);
                try
                {
                    await Task.Delay(espera, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                espera = TimeSpan.FromSeconds(Math.Min(espera.TotalSeconds * 2, EsperaMaxima.TotalSeconds));
            }
        }

        if (cliente.IsConnected)
            await cliente.DisconnectAsync();
    }

    private async Task ProcessarAsync(MqttApplicationMessageReceivedEventArgs evento, CancellationToken ct)
    {
        var topico = evento.ApplicationMessage.Topic;
        var payload = evento.ApplicationMessage.ConvertPayloadToString();
        estado.Guardar(topico, payload, opcoes.Value.MensagensGuardadas);

        if (!MensagemMqtt.TentarLer(topico, payload, out var nova, out var motivo))
        {
            log.LogWarning("Mensagem MQTT ignorada em {Topico}: {Motivo}.", topico, motivo);
            return;
        }

        try
        {
            using var escopo = escopos.CreateScope();
            await escopo.ServiceProvider.GetRequiredService<ILeituraService>().RegistrarAsync(nova!, ct);
        }
        catch (DominioException ex)
        {
            log.LogWarning("Leitura MQTT recusada em {Topico}: {Motivo}", topico, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Uma mensagem com problema não pode derrubar o consumidor.
            log.LogError(ex, "Erro ao gravar a leitura MQTT de {Topico}.", topico);
        }
    }
}

/// <summary>MQTT desligado ou conectado: saudável. Ligado e sem conexão: degradado (a API segue no ar).</summary>
internal sealed class MqttHealthCheck(IOptions<MqttOptions> opcoes, EstadoMqtt estado) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default) =>
        Task.FromResult(!opcoes.Value.Habilitado
            ? HealthCheckResult.Healthy("MQTT desligado nesta configuração.")
            : estado.Conectado
                ? HealthCheckResult.Healthy($"MQTT conectado em {opcoes.Value.Host}.")
                : HealthCheckResult.Degraded($"MQTT sem conexão: {estado.UltimoErro}"));
}
