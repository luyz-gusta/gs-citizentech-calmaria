// Painel Risco Cidadão. Fala só com a própria API, com a chave de administrador no cabeçalho X-Admin-Key.
// Todo dado entra na página por textContent: nada da API vira HTML.
"use strict";

(() => {
  const CHAVE = "calmaria-chave-admin";
  const $ = (id) => document.getElementById(id);

  const CATEGORIAS = {
    CPF: "CPF", DOCUMENTO: "Documento (RG, cartão do SUS)", TELEFONE: "Telefone", EMAIL: "E-mail",
    SAUDE: "Saúde", SEGREDO: "Senha ou chave", LOCALIZACAO_PRECISA: "Localização exata",
    QUASE_IDENTIFICADOR: "CEP e nascimento"
  };
  const DESTINOS = {
    LOG: "Log da API", RESPOSTA_PUBLICA: "Resposta pública", LLM: "LLM externo", BROKER_MQTT: "Broker MQTT",
    BANCO: "Banco", ARQUIVO: "Arquivos do repositório"
  };
  const FONTES = { LOGS: "Logs da API", RELATOS: "Relatos gravados", MQTT: "Broker MQTT", CODIGO: "Código" };
  const FAIXAS = { BAIXO: ["Baixo", "nv-baixo"], MEDIO: ["Médio", "nv-medio"], ALTO: ["Alto", "nv-alto"] };

  const dataHora = new Intl.DateTimeFormat("pt-BR", {
    day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit", timeZone: "America/Sao_Paulo"
  });
  const numero = new Intl.NumberFormat("pt-BR", { maximumFractionDigits: 1 });

  let grafico = null;

  // ---------- chave ----------

  const lerChave = () => {
    try { return sessionStorage.getItem(CHAVE) || ""; } catch { return ""; }
  };
  const gravarChave = (valor) => {
    try { sessionStorage.setItem(CHAVE, valor); } catch { /* aba sem storage: a chave vale só até recarregar */ }
    memoria = valor;
  };
  let memoria = lerChave();

  const pedirChave = (errada) => {
    $("chave").hidden = false;
    $("chave-erro").hidden = !errada;
    $("chave-valor").focus();
  };

  $("chave-form").addEventListener("submit", (evento) => {
    evento.preventDefault();
    gravarChave($("chave-valor").value.trim());
    $("chave-valor").value = "";
    $("chave").hidden = true;
    carregar();
  });

  // ---------- API ----------

  class NaoAutorizado extends Error {}

  async function api(caminho, metodo = "GET") {
    const resposta = await fetch(caminho, {
      method: metodo,
      headers: { "X-Admin-Key": memoria, Accept: "application/json" },
      cache: "no-store"
    });
    if (resposta.status === 401) throw new NaoAutorizado();
    if (resposta.status === 404) return null;
    if (!resposta.ok) throw new Error(`A API respondeu ${resposta.status}.`);
    return resposta.json();
  }

  // ---------- desenho ----------

  function el(tag, props = {}, ...filhos) {
    const no = document.createElement(tag);
    for (const [k, v] of Object.entries(props)) {
      if (k === "class") no.className = v;
      else no.setAttribute(k, v);
    }
    for (const f of filhos) no.append(f instanceof Node ? f : document.createTextNode(String(f)));
    return no;
  }

  function icone(id) {
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.setAttribute("aria-hidden", "true");
    const use = document.createElementNS("http://www.w3.org/2000/svg", "use");
    use.setAttribute("href", `#${id}`);
    svg.append(use);
    return svg;
  }

  function selo(faixa) {
    const [texto, classe] = FAIXAS[faixa] || ["-", "nv-baixo"];
    return el("span", { class: `selo ${classe}` }, icone(classe), texto);
  }

  function vazio(tbody, colunas, texto) {
    tbody.replaceChildren(el("tr", {}, el("td", { colspan: String(colunas), class: "vazio" }, texto)));
  }

  function marcar(id, indice, rotulo) {
    const marca = $(id);
    if (indice === null || indice === undefined) { marca.hidden = true; return; }
    marca.hidden = false;
    marca.textContent = `${rotulo} ${indice}`;
    marca.style.left = `${Math.min(97, Math.max(3, indice))}%`;
  }

  function barras(alvo, parcelas, nome) {
    alvo.replaceChildren();
    if (!parcelas.length) {
      alvo.append(el("span", { class: "dica" }, "Nada nesta varredura."));
      return;
    }
    const maximo = Math.max(...parcelas.map((p) => p.pontos), 1);
    for (const p of parcelas) {
      const barra = el("i");
      barra.style.width = `${(p.pontos / maximo) * 100}%`;
      alvo.append(
        el("span", {}, nome(p)),
        barra,
        el("b", {}, numero.format(p.pontos)),
        el("small", {}, `${p.expostos} exposto(s), ${p.mitigados} barrado(s)`)
      );
    }
  }

  function origem(texto) {
    if (texto === "relato.comentario") return "Comentário do relato";
    if (texto.startsWith("resposta:")) return `Resposta de ${texto.slice(9)}`;
    if (texto.startsWith("log:")) return `Log (${texto.slice(4).split(".").slice(-1)[0]})`;
    return texto;
  }

  function desenharRisco(painel) {
    const atual = painel.atual;
    const risco = atual.risco;
    $("ultima").textContent = `Última varredura: ${dataHora.format(new Date(atual.concluidaEm))}`;
    $("selo-exemplo").hidden = !painel.dadosDeExemplo;

    $("agora-valor").textContent = risco.indice;
    $("agora-selo").replaceChildren(selo(risco.faixa));
    if (painel.antes) {
      $("antes-valor").textContent = painel.antes.indice;
      $("antes-selo").replaceChildren(selo(painel.antes.faixa));
    } else {
      $("antes-valor").textContent = "-";
      $("antes-selo").replaceChildren(el("span", { class: "dica" }, "só há uma varredura"));
    }
    $("expostos").textContent = risco.expostos;
    $("mitigados").textContent = risco.mitigados;
    marcar("marca-antes", painel.antes ? painel.antes.indice : null, "antes");
    marcar("marca-agora", risco.indice, "agora");

    const [faixa] = FAIXAS[risco.faixa] || ["-"];
    $("risco-frase").textContent = painel.antes
      ? `O Risco Cidadão foi de ${painel.antes.indice} para ${risco.indice} (${faixa.toLowerCase()}): ${risco.expostos} dado(s) exposto(s) e ${risco.mitigados} fluxo(s) barrado(s) pelos guardas.`
      : `Risco Cidadão ${risco.indice} (${faixa.toLowerCase()}): ${risco.expostos} dado(s) exposto(s) e ${risco.mitigados} fluxo(s) barrado(s).`;

    barras($("por-categoria"), risco.porCategoria, (p) => CATEGORIAS[p.categoria] || p.categoria);
    barras($("por-destino"), risco.porDestino, (p) => DESTINOS[p.destino] || p.destino);

    const fontes = $("fontes");
    fontes.replaceChildren(...atual.fontes.map((f) => el("tr", {},
      el("td", {}, FONTES[f.fonte] || f.fonte),
      el("td", {}, f.trechosLidos),
      el("td", {}, f.achados),
      el("td", {}, f.lida ? (f.motivo || "Lida") : `Não lida: ${f.motivo}`))));

    const alertas = $("alertas");
    alertas.replaceChildren();
    if (!atual.alertas.length) {
      alertas.append(el("p", { class: "dica" }, "Nenhum alerta: nada exposto e nenhum fluxo barrado na janela."));
    }
    for (const a of atual.alertas) {
      const quem = a.geradoPor === "LLM" ? "traduzido pelo LLM" : `frase pronta${a.motivo ? ` (${a.motivo})` : ""}`;
      alertas.append(el("div", { class: "alerta" },
        el("p", {}, a.paraCidadao),
        el("p", { class: "equipe" }, `Para a equipe: ${a.paraEquipe}`),
        el("small", {}, `${CATEGORIAS[a.categoria] || a.categoria} · ${DESTINOS[a.destino] || a.destino} · ${a.mitigado ? "barrado" : "exposto"} · ${a.ocorrencias} vez(es) · ${quem}`)));
    }

    desenharHistorico(painel.historico);
  }

  function desenharHistorico(historico) {
    $("historico-tabela").replaceChildren(...historico.map((m) => el("tr", {},
      el("td", {}, `#${m.varreduraId}`),
      el("td", {}, dataHora.format(new Date(m.em))),
      el("td", {}, `${m.indice} (${(FAIXAS[m.faixa] || ["-"])[0]})`))));

    if (!window.Chart) return; // Sem o CDN, fica a tabela.
    const css = getComputedStyle(document.documentElement);
    const cor = (nome) => css.getPropertyValue(nome).trim();
    const corDaFaixa = { BAIXO: cor("--color-nivel-calmo-solid"), MEDIO: cor("--color-nivel-moderado-solid"), ALTO: cor("--color-nivel-intenso-solid") };
    Chart.defaults.font.family = '"Atkinson Hyperlegible Next", system-ui, sans-serif';
    Chart.defaults.font.size = 14;
    Chart.defaults.color = cor("--color-text-muted");
    Chart.defaults.borderColor = cor("--color-border-default");
    Chart.defaults.animation = false;

    if (grafico) grafico.destroy();
    grafico = new Chart($("historico"), {
      type: "bar",
      data: {
        labels: historico.map((m) => `#${m.varreduraId}`),
        datasets: [{
          label: "Risco Cidadão",
          data: historico.map((m) => m.indice),
          backgroundColor: historico.map((m) => corDaFaixa[m.faixa]),
          borderRadius: 4
        }]
      },
      options: {
        maintainAspectRatio: false,
        scales: { y: { min: 0, max: 100, ticks: { stepSize: 20 } } },
        plugins: { legend: { display: false } }
      }
    });
  }

  function desenharFluxos(fluxos) {
    const corpo = $("fluxos");
    if (!fluxos.length) { vazio(corpo, 5, "Nenhum fluxo barrado ainda."); return; }
    corpo.replaceChildren(...fluxos.map((f) => el("tr", {},
      el("td", {}, dataHora.format(new Date(f.ocorridoEm))),
      el("td", {}, origem(f.origem)),
      el("td", {}, DESTINOS[f.destino] || f.destino),
      el("td", {}, CATEGORIAS[f.categoria] || f.categoria),
      el("td", {}, f.bloqueado
        ? el("span", { class: "ok" }, icone("i-check"), "Bloqueado")
        : el("span", { class: "exposto" }, icone("i-alerta"), "Passou")))));
  }

  function desenharAchados(achados) {
    const corpo = $("achados");
    if (!achados || !achados.length) { vazio(corpo, 4, "Nenhum dado exposto na última varredura."); return; }
    corpo.replaceChildren(...achados.map((a) => el("tr", {},
      el("td", {}, FONTES[a.fonte] || a.fonte),
      el("td", {}, el("code", {}, a.local)),
      el("td", {}, CATEGORIAS[a.categoria] || a.categoria),
      el("td", {}, el("code", {}, a.trecho)))));
  }

  function avisar(texto) {
    $("aviso").hidden = !texto;
    $("aviso").textContent = texto || "";
  }

  // ---------- fluxo da página ----------

  async function carregar() {
    if (!memoria) { pedirChave(false); return; }
    try {
      const [painel, fluxos] = await Promise.all([api("/api/privacidade/risco-cidadao"), api("/api/privacidade/fluxos?limite=50")]);
      desenharFluxos(fluxos || []);
      if (!painel) {
        avisar("Nenhuma varredura ainda. Use Rodar varredura.");
        desenharAchados([]);
        return;
      }
      avisar("");
      desenharRisco(painel);
      desenharAchados(await api("/api/privacidade/achados"));
    } catch (erro) {
      if (erro instanceof NaoAutorizado) { pedirChave(true); return; }
      avisar(`Não foi possível carregar o painel: ${erro.message}`);
    }
  }

  $("varrer").addEventListener("click", async () => {
    if (!memoria) { pedirChave(false); return; }
    const botao = $("varrer");
    botao.disabled = true;
    botao.textContent = "Varredura em andamento.";
    try {
      await api("/api/privacidade/varreduras", "POST");
      await carregar();
    } catch (erro) {
      if (erro instanceof NaoAutorizado) pedirChave(true);
      else avisar(`A varredura falhou: ${erro.message}`);
    } finally {
      botao.disabled = false;
      botao.textContent = "Rodar varredura";
    }
  });

  carregar();
})();
