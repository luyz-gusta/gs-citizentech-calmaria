using System.Runtime.CompilerServices;
using Calmaria.Infrastructure.Mqtt;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Calmaria.Infrastructure.Privacidade;

/// <summary>
/// Os comentários gravados, que a listagem pública devolve a qualquer pessoa. O Sanitizador limpa o que entra pela
/// API; esta fonte acha o que entrou por fora dela (importação, script, registro antigo).
/// </summary>
internal sealed class FonteRelatos(CalmariaDbContext db) : IFonteVarredura
{
    public string Nome => "RELATOS";
    public DestinoFluxo Destino => DestinoFluxo.RespostaPublica;

    public async IAsyncEnumerable<TrechoVarrido> LerAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        var comentarios = db.Relatos.AsNoTracking()
            .Where(r => r.Comentario != null)
            .OrderBy(r => r.Id)
            .Select(r => new { r.Id, r.Comentario })
            .AsAsyncEnumerable()
            .WithCancellation(ct);

        await foreach (var relato in comentarios)
            yield return new TrechoVarrido($"relato {relato.Id}", relato.Comentario!.Valor);
    }
}

/// <summary>
/// As últimas mensagens recebidas do broker MQTT público. Qualquer pessoa assinando o tópico vê o mesmo que a API,
/// então dado pessoal aqui está exposto.
/// </summary>
internal sealed class FonteMqtt(EstadoMqtt estado, IOptions<MqttOptions> opcoes) : IFonteVarredura
{
    public string Nome => "MQTT";
    public DestinoFluxo Destino => DestinoFluxo.BrokerMqtt;

    public async IAsyncEnumerable<TrechoVarrido> LerAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!opcoes.Value.Habilitado)
            throw new FonteIndisponivelException("O consumidor MQTT está desligado (Mqtt:Habilitado).");

        await Task.CompletedTask;
        foreach (var mensagem in estado.UltimasMensagens)
        {
            ct.ThrowIfCancellationRequested();
            yield return new TrechoVarrido($"mqtt {mensagem.Topico}", mensagem.Payload);
        }
    }
}
