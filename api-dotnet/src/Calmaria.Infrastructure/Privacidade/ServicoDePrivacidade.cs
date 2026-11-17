using System.Text.Json;
using System.Text.Json.Serialization;
using Calmaria.Infrastructure.Persistencia;
using Calmaria.Privacidade.Motor;
using Calmaria.Privacidade.Remediacao;
using Calmaria.Privacidade.Risco;
using Calmaria.Privacidade.Taint;
using Calmaria.Privacidade.Varredura;
using Microsoft.EntityFrameworkCore;

namespace Calmaria.Infrastructure.Privacidade;

/// <summary>
/// Junta as peças do módulo: grava os fluxos que os guardas registraram, roda a varredura, calcula o Risco Cidadão
/// com os achados expostos e os fluxos da janela, traduz os alertas (com cache) e grava tudo.
/// </summary>
internal sealed class ServicoDePrivacidade(
    CalmariaDbContext db,
    ServicoVarredura varredura,
    IRastreadorDeFluxo rastreador,
    ICalculadoraRiscoCidadao calculadora,
    ITradutorDeRisco tradutor,
    OpcoesDaVarredura opcoes,
    OpcoesLlm opcoesLlm,
    TimeProvider relogio) : IServicoDePrivacidade
{
    /// <summary>Quantas perguntas diferentes vão ao LLM numa varredura; o resto usa a frase pronta.</summary>
    public const int MaximoDePerguntasAoLlm = 12;

    private const int TamanhoDoHistorico = 10;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<VarreduraFeita> VarrerAsync(CancellationToken ct = default)
    {
        var iniciadaEm = relogio.GetUtcNow().UtcDateTime;
        await GravarFluxosPendentesAsync(ct);

        var resultado = await varredura.ExecutarAsync(ct);
        var desde = iniciadaEm - opcoes.JanelaDeFluxos;
        var fluxos = await db.EventosDeFluxo.AsNoTracking().Where(f => f.OcorridoEm >= desde).ToListAsync(ct);

        var itens = resultado.Achados
            .Select(a => new ItemDeRisco(a.Achado.Categoria, a.Achado.Peso, a.Destino, Mitigado: false))
            .Concat(fluxos.Select(f => new ItemDeRisco(f.Categoria, f.Peso, f.Destino, Mitigado: f.Bloqueado)))
            .ToList();
        var risco = calculadora.Calcular(itens);
        var alertas = await TraduzirAsync(resultado.Achados, fluxos, ct);

        var registro = new VarreduraRegistro
        {
            IniciadaEm = iniciadaEm,
            ConcluidaEm = relogio.GetUtcNow().UtcDateTime,
            Indice = risco.Indice,
            Faixa = risco.Faixa,
            Expostos = risco.Expostos,
            Mitigados = risco.Mitigados,
            Fluxos = fluxos.Count,
            ComposicaoJson = JsonSerializer.Serialize(risco, Json),
            FontesJson = JsonSerializer.Serialize(resultado.Fontes, Json),
            AlertasJson = JsonSerializer.Serialize(alertas, Json),
            Achados = resultado.Achados.Select(a => new AchadoRegistro
            {
                Fonte = a.Fonte, Destino = a.Destino, Local = Cortar(a.Local, 300), RegraId = a.Achado.RegraId,
                Categoria = a.Achado.Categoria, Peso = a.Achado.Peso, Trecho = Cortar(a.Achado.TrechoMascarado, 120)
            }).ToList()
        };
        db.Varreduras.Add(registro);
        await db.SaveChangesAsync(ct);

        return ParaVarreduraFeita(registro);
    }

    public async Task<PainelDeRisco?> PainelAsync(CancellationToken ct = default)
    {
        var historico = await db.Varreduras.AsNoTracking()
            .OrderByDescending(v => v.Id)
            .Take(TamanhoDoHistorico)
            .Select(v => new MarcoDeRisco(v.Id, v.ConcluidaEm, v.Indice, v.Faixa))
            .ToListAsync(ct);
        if (historico.Count == 0)
            return null;

        var atual = await db.Varreduras.AsNoTracking().SingleAsync(v => v.Id == historico[0].VarreduraId, ct);
        return new PainelDeRisco(ParaVarreduraFeita(atual), historico.Skip(1).FirstOrDefault(),
            historico.OrderBy(m => m.VarreduraId).ToList());
    }

    public async Task<IReadOnlyList<AchadoRegistrado>?> AchadosAsync(long? varreduraId, CancellationToken ct = default)
    {
        var varreduraAlvo = varreduraId is { } id
            ? await db.Varreduras.AsNoTracking().SingleOrDefaultAsync(v => v.Id == id, ct)
            : await db.Varreduras.AsNoTracking().OrderByDescending(v => v.Id).FirstOrDefaultAsync(ct);
        if (varreduraAlvo is null)
            return null;

        var alertas = LerAlertas(varreduraAlvo);
        var achados = await db.AchadosDeVarredura.AsNoTracking()
            .Where(a => a.VarreduraId == varreduraAlvo.Id)
            .OrderBy(a => a.Id)
            .ToListAsync(ct);

        return achados.Select(a => new AchadoRegistrado(a.Id, a.VarreduraId, a.Fonte, a.Destino, a.Local, a.RegraId,
                a.Categoria, a.Peso, a.Trecho,
                alertas.FirstOrDefault(x => x.Categoria == a.Categoria && x.Destino == a.Destino && !x.Mitigado)?.Alerta))
            .ToList();
    }

    public async Task<IReadOnlyList<EventoDeFluxo>> FluxosAsync(int limite, CancellationToken ct = default)
    {
        await GravarFluxosPendentesAsync(ct);
        var fluxos = await db.EventosDeFluxo.AsNoTracking()
            .OrderByDescending(f => f.OcorridoEm)
            .ThenByDescending(f => f.Id)
            .Take(limite)
            .ToListAsync(ct);
        return fluxos.Select(f => f.ParaEvento()).ToList();
    }

    /// <summary>Os guardas registram em memória (o log não pode depender do banco); aqui eles vão para a tabela.</summary>
    private async Task GravarFluxosPendentesAsync(CancellationToken ct)
    {
        var pendentes = rastreador.Drenar();
        if (pendentes.Count == 0)
            return;

        db.EventosDeFluxo.AddRange(pendentes.Select(EventoDeFluxoRegistro.De));
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Um alerta por categoria, destino e situação (exposto ou mitigado). A resposta do LLM fica em cache por modelo;
    /// a frase pronta é refeita a cada vez.
    /// </summary>
    private async Task<IReadOnlyList<AlertaDoPainel>> TraduzirAsync(IReadOnlyList<AchadoDeVarredura> achados,
        IReadOnlyList<EventoDeFluxoRegistro> fluxos, CancellationToken ct)
    {
        var grupos = achados
            .Select(a => (a.Achado.Categoria, a.Destino, Mitigado: false, Exemplo: AchadoMascarado.De(a.Achado)))
            .Concat(fluxos.Select(f => (f.Categoria, f.Destino, Mitigado: f.Bloqueado,
                Exemplo: new AchadoMascarado(f.Categoria, Marca(f.Categoria), f.Peso))))
            .GroupBy(x => (x.Categoria, x.Destino, x.Mitigado))
            .OrderByDescending(g => g.Count())
            .ToList();

        var modelo = opcoesLlm.Configurado ? opcoesLlm.Modelo! : "";
        var alertas = new List<AlertaDoPainel>();
        var perguntas = 0;
        foreach (var grupo in grupos)
        {
            var (categoria, destino, mitigado) = grupo.Key;
            var emCache = opcoesLlm.Configurado
                ? await db.AlertasEmCache.AsNoTracking().FirstOrDefaultAsync(a => a.Categoria == categoria
                    && a.Destino == destino && a.Mitigado == mitigado && a.Modelo == modelo, ct)
                : null;

            AlertaCidadao alerta;
            if (emCache is not null)
            {
                alerta = new AlertaCidadao(emCache.ParaCidadao, emCache.ParaEquipe, OrigemDoAlerta.Llm);
            }
            else if (opcoesLlm.Configurado && perguntas >= MaximoDePerguntasAoLlm)
            {
                alerta = new TradutorModelo().Traduzir(categoria, destino, mitigado,
                    $"Passou de {MaximoDePerguntasAoLlm} perguntas ao LLM nesta varredura.");
            }
            else
            {
                perguntas++;
                alerta = await tradutor.TraduzirAsync(grupo.First().Exemplo, destino, mitigado, ct);
                if (alerta.GeradoPor == OrigemDoAlerta.Llm)
                {
                    db.AlertasEmCache.Add(new AlertaEmCache
                    {
                        Categoria = categoria, Destino = destino, Mitigado = mitigado, Modelo = modelo,
                        ParaCidadao = Cortar(alerta.ParaCidadao, 500), ParaEquipe = Cortar(alerta.ParaEquipe, 500),
                        GeradoEm = relogio.GetUtcNow().UtcDateTime
                    });
                }
            }

            alertas.Add(new AlertaDoPainel(categoria, destino, mitigado, grupo.Count(), alerta));
        }

        return alertas;
    }

    private static VarreduraFeita ParaVarreduraFeita(VarreduraRegistro v) => new(
        v.Id,
        v.ConcluidaEm,
        JsonSerializer.Deserialize<RiscoCidadao>(v.ComposicaoJson, Json)!,
        JsonSerializer.Deserialize<List<SituacaoDaFonte>>(v.FontesJson, Json) ?? [],
        v.Expostos,
        v.Fluxos,
        LerAlertas(v));

    private static List<AlertaDoPainel> LerAlertas(VarreduraRegistro v) =>
        JsonSerializer.Deserialize<List<AlertaDoPainel>>(v.AlertasJson, Json) ?? [];

    private static string Marca(CategoriaPii categoria) =>
        categoria == CategoriaPii.Saude ? Sanitizador.MarcaSaude : Sanitizador.MarcaRemovido;

    private static string Cortar(string texto, int maximo) => texto.Length <= maximo ? texto : texto[..maximo];
}
