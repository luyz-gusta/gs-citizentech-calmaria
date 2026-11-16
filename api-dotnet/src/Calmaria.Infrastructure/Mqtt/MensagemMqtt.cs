using System.Text.Json;
using System.Text.RegularExpressions;
using Calmaria.Domain.Sensorial;

namespace Calmaria.Infrastructure.Mqtt;

/// <summary>
/// O payload do ESP32 no tópico calmaria/v1/lugares/{lugarId}/leituras (contrato MQTT). Índice e nível vêm só
/// como informação: a API recalcula.
/// </summary>
public sealed partial record MensagemMqtt(string? DispositivoId, int? LugarId, double? SomDb, double? LuzLux,
    int? MovimentosMin, int? Indice, string? Nivel)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [GeneratedRegex(@"^calmaria/v1/lugares/(\d+)/leituras$")]
    private static partial Regex TopicoDeLeituras();

    /// <summary>
    /// Transforma tópico e payload numa leitura nova. Se não der, devolve o motivo, que vai para o log; o payload
    /// nunca vai, porque o broker é público e não se loga o que chega dele.
    /// </summary>
    public static bool TentarLer(string topico, string payload, out NovaLeitura? leitura, out string? motivo)
    {
        leitura = null;
        var casamento = TopicoDeLeituras().Match(topico);
        if (!casamento.Success)
        {
            motivo = "tópico fora do padrão calmaria/v1/lugares/{lugarId}/leituras";
            return false;
        }

        MensagemMqtt? mensagem;
        try
        {
            mensagem = JsonSerializer.Deserialize<MensagemMqtt>(payload, Json);
        }
        catch (JsonException)
        {
            motivo = "payload não é um JSON válido";
            return false;
        }

        if (mensagem is null || string.IsNullOrWhiteSpace(mensagem.DispositivoId) || mensagem.LugarId is null ||
            mensagem.SomDb is null || mensagem.LuzLux is null || mensagem.MovimentosMin is null)
        {
            motivo = "faltam campos obrigatórios (dispositivoId, lugarId, somDb, luzLux, movimentosMin)";
            return false;
        }

        var lugarDoTopico = int.Parse(casamento.Groups[1].Value);
        if (mensagem.LugarId != lugarDoTopico)
        {
            motivo = $"o lugarId do payload ({mensagem.LugarId}) não bate com o do tópico ({lugarDoTopico})";
            return false;
        }

        leitura = new NovaLeitura(mensagem.DispositivoId.Trim(), lugarDoTopico, mensagem.SomDb.Value,
            mensagem.LuzLux.Value, mensagem.MovimentosMin.Value);
        motivo = null;
        return true;
    }
}
