using System.Text.RegularExpressions;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Privacidade.Varredura;

/// <summary>Um dado pessoal achado num destino. O achado já vem mascarado do motor.</summary>
/// <param name="Fonte">LOGS, RELATOS, MQTT ou CODIGO.</param>
/// <param name="Destino">O destino que a fonte representa.</param>
/// <param name="Local">Onde estava (arquivo e linha, id do relato, tópico).</param>
/// <param name="Achado">O achado do motor.</param>
public sealed record AchadoDeVarredura(string Fonte, DestinoFluxo Destino, string Local, Achado Achado);

/// <summary>Como foi a leitura de uma fonte.</summary>
/// <param name="Fonte">O nome da fonte.</param>
/// <param name="Destino">O destino que ela representa.</param>
/// <param name="Lida">Falso se a fonte estava indisponível.</param>
/// <param name="TrechosLidos">Quantos textos foram analisados.</param>
/// <param name="Achados">Quantos dados pessoais apareceram.</param>
/// <param name="Motivo">Por que não foi lida, ou o aviso da leitura.</param>
public sealed record SituacaoDaFonte(string Fonte, DestinoFluxo Destino, bool Lida, int TrechosLidos, int Achados,
    string? Motivo);

/// <summary>O que uma varredura achou e como foi cada fonte.</summary>
public sealed record ResultadoVarredura(IReadOnlyList<AchadoDeVarredura> Achados, IReadOnlyList<SituacaoDaFonte> Fontes);

/// <summary>
/// Roda cada <see cref="IFonteVarredura"/> e passa os textos pelo motor de PII. Fonte indisponível não derruba a
/// varredura: ela entra no resultado com o motivo. O texto lido nunca sai daqui, só o achado mascarado.
/// </summary>
public sealed class ServicoVarredura(IEnumerable<IFonteVarredura> fontes, IMotorPii motor)
{
    /// <summary>Teto de achados por fonte, para um log cheio de dado não lotar o banco.</summary>
    public const int MaximoDeAchadosPorFonte = 500;

    public async Task<ResultadoVarredura> ExecutarAsync(CancellationToken ct = default)
    {
        var achados = new List<AchadoDeVarredura>();
        var situacoes = new List<SituacaoDaFonte>();

        foreach (var fonte in fontes)
        {
            var lidos = 0;
            var daFonte = new List<AchadoDeVarredura>();
            var esgotados = 0;
            string? motivo = null;
            var lida = true;

            try
            {
                await foreach (var trecho in fonte.LerAsync(ct))
                {
                    lidos++;
                    if (daFonte.Count >= MaximoDeAchadosPorFonte)
                        continue;

                    IReadOnlyList<Achado> doTrecho;
                    try
                    {
                        doTrecho = motor.Analisar(trecho.Texto);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        // Texto feito para travar a expressão: conta e segue.
                        esgotados++;
                        continue;
                    }

                    daFonte.AddRange(doTrecho
                        .Where(a => fonte.Categorias is null || fonte.Categorias.Contains(a.Categoria))
                        .Select(a => new AchadoDeVarredura(fonte.Nome, fonte.Destino, trecho.Local, a)));
                }
            }
            catch (FonteIndisponivelException ex)
            {
                lida = false;
                motivo = ex.Message;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                lida = false;
                motivo = $"Erro de leitura: {ex.Message}";
            }

            if (daFonte.Count > MaximoDeAchadosPorFonte)
                daFonte.RemoveRange(MaximoDeAchadosPorFonte, daFonte.Count - MaximoDeAchadosPorFonte);
            if (lida && daFonte.Count == MaximoDeAchadosPorFonte)
                motivo = $"Parou em {MaximoDeAchadosPorFonte} achados.";
            if (esgotados > 0)
                motivo = $"{esgotados} texto(s) passaram do tempo limite e ficaram de fora.";

            achados.AddRange(daFonte);
            situacoes.Add(new SituacaoDaFonte(fonte.Nome, fonte.Destino, lida, lidos, daFonte.Count, motivo));
        }

        return new ResultadoVarredura(achados, situacoes);
    }
}
