using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Remediacao;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;

namespace Calmaria.Privacidade.Risco;

/// <summary>Uma varredura gravada: o Risco Cidadão, como foi cada fonte e os alertas traduzidos.</summary>
/// <param name="Id">O id da varredura.</param>
/// <param name="ConcluidaEmUtc">Quando terminou.</param>
/// <param name="Risco">O indicador e a composição.</param>
/// <param name="Fontes">Como foi cada fonte.</param>
/// <param name="Achados">Quantos dados pessoais estavam expostos.</param>
/// <param name="Fluxos">Quantos fluxos (bloqueados ou não) entraram na janela.</param>
/// <param name="Alertas">Um alerta por categoria, destino e situação.</param>
public sealed record VarreduraFeita(long Id, DateTime ConcluidaEmUtc, RiscoCidadao Risco,
    IReadOnlyList<SituacaoDaFonte> Fontes, int Achados, int Fluxos, IReadOnlyList<AlertaDoPainel> Alertas);

/// <summary>Um ponto do histórico do Risco Cidadão.</summary>
public sealed record MarcoDeRisco(long VarreduraId, DateTime EmUtc, int Indice, FaixaRisco Faixa);

/// <summary>O alerta de um tipo de achado e quantas vezes ele apareceu.</summary>
/// <param name="Categoria">O tipo de dado.</param>
/// <param name="Destino">Onde estava ou para onde ia.</param>
/// <param name="Mitigado">Se um guarda barrou.</param>
/// <param name="Ocorrencias">Quantas vezes.</param>
/// <param name="Alerta">O alerta para o cidadão e a correção para a equipe.</param>
public sealed record AlertaDoPainel(CategoriaPii Categoria, DestinoFluxo Destino, bool Mitigado, int Ocorrencias,
    AlertaCidadao Alerta);

/// <summary>O que o painel mostra: a varredura atual, a anterior (o antes) e o histórico.</summary>
public sealed record PainelDeRisco(VarreduraFeita Atual, MarcoDeRisco? Antes, IReadOnlyList<MarcoDeRisco> Historico);

/// <summary>Um achado gravado de uma varredura, com o alerta da categoria e do destino dele.</summary>
public sealed record AchadoRegistrado(long Id, long VarreduraId, string Fonte, DestinoFluxo Destino, string Local,
    string RegraId, CategoriaPii Categoria, int Peso, string Trecho, AlertaCidadao? Alerta);

/// <summary>As rotas administrativas da privacidade: varrer, o Risco Cidadão, os achados e os fluxos.</summary>
public interface IServicoDePrivacidade
{
    /// <summary>Roda as fontes, calcula o Risco Cidadão, traduz os alertas e grava tudo.</summary>
    Task<VarreduraFeita> VarrerAsync(CancellationToken ct = default);

    /// <summary>A última varredura, a anterior e o histórico. Nulo se nunca houve varredura.</summary>
    Task<PainelDeRisco?> PainelAsync(CancellationToken ct = default);

    /// <summary>Os achados de uma varredura (a última, se não disser qual). Nulo se a varredura não existe.</summary>
    Task<IReadOnlyList<AchadoRegistrado>?> AchadosAsync(long? varreduraId, CancellationToken ct = default);

    /// <summary>Os fluxos mais recentes, gravando antes os que estavam em memória.</summary>
    Task<IReadOnlyList<EventoDeFluxo>> FluxosAsync(int limite, CancellationToken ct = default);
}
