using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Risco;
using Calmaria.Privacidade.Taint;

namespace Calmaria.Infrastructure.Privacidade;

/// <summary>
/// Uma varredura gravada (tabela Varreduras). A composição, as fontes e os alertas ficam em JSON: são lidos
/// inteiros pelo painel e nunca filtrados.
/// </summary>
public sealed class VarreduraRegistro
{
    public long Id { get; set; }
    public DateTime IniciadaEm { get; set; }
    public DateTime ConcluidaEm { get; set; }
    public int Indice { get; set; }
    public FaixaRisco Faixa { get; set; }
    public int Expostos { get; set; }
    public int Mitigados { get; set; }
    public int Fluxos { get; set; }
    public string ComposicaoJson { get; set; } = "{}";
    public string FontesJson { get; set; } = "[]";
    public string AlertasJson { get; set; } = "[]";
    public List<AchadoRegistro> Achados { get; set; } = [];
}

/// <summary>Um dado pessoal exposto que a varredura achou. Só o trecho mascarado é gravado.</summary>
public sealed class AchadoRegistro
{
    public long Id { get; set; }
    public long VarreduraId { get; set; }
    public string Fonte { get; set; } = "";
    public DestinoFluxo Destino { get; set; }
    public string Local { get; set; } = "";
    public string RegraId { get; set; } = "";
    public CategoriaPii Categoria { get; set; }
    public int Peso { get; set; }
    public string Trecho { get; set; } = "";
}

/// <summary>Um fluxo de dado do cidadão que chegou a um guarda de saída (tabela EventosDeFluxo).</summary>
public sealed class EventoDeFluxoRegistro
{
    public long Id { get; set; }
    public string Origem { get; set; } = "";
    public DestinoFluxo Destino { get; set; }
    public CategoriaPii Categoria { get; set; }
    public int Peso { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime OcorridoEm { get; set; }

    public EventoDeFluxo ParaEvento() => new(Origem, Destino, Categoria, Peso, Bloqueado, OcorridoEm);

    public static EventoDeFluxoRegistro De(EventoDeFluxo e) => new()
    {
        Origem = e.Origem, Destino = e.Destino, Categoria = e.Categoria, Peso = e.Peso, Bloqueado = e.Bloqueado,
        OcorridoEm = e.OcorridoEmUtc
    };
}

/// <summary>
/// A resposta do LLM guardada por categoria, destino, situação e modelo, para não pagar a mesma pergunta duas
/// vezes (tabela AlertasEmCache). Frase pronta não entra: ela não custa nada.
/// </summary>
public sealed class AlertaEmCache
{
    public int Id { get; set; }
    public CategoriaPii Categoria { get; set; }
    public DestinoFluxo Destino { get; set; }
    public bool Mitigado { get; set; }
    public string Modelo { get; set; } = "";
    public string ParaCidadao { get; set; } = "";
    public string ParaEquipe { get; set; } = "";
    public DateTime GeradoEm { get; set; }
}
