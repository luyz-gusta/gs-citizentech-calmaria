// As cenas do vídeo como composições do HyperFrames (HTML com uma linha do tempo GSAP pausada, renderizada quadro a
// quadro). Cada cena recebe os tempos das próprias falas e faz cada parte aparecer quando a voz a nomeia.
//
// Kit de vídeo (VI-01): 1920 x 1080; margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no
// mínimo; corte seco entre as cenas. Cores e fontes só de design/tokens e design/pecas. Movimento quieto: nada
// pisca, nada quica, nada fica em laço; um elemento entra, assenta e fica parado.
import { BLOCOS, EQUIPE, NARRADOR, REPOSITORIO } from './roteiro.mjs';

export const COR = {
  fundo: '#F1F5F7', cartao: '#F8FBFD', texto: '#14191F', apoio: '#4D545B', marca: '#3D5A73', marcaSutil: '#E7EEF3',
  borda: '#E0E5E9', bordaForte: '#6B737A', escuro: '#14191F', escuroCartao: '#22282E', escuroSutil: '#9BA3AA',
  video: '#1F2733', videoSup: '#313D4D', videoTexto: '#F8FBFD', videoApoio: '#D4E1EA', videoAcao: '#B2C9D8',
  calmo: '#437E61', calmoTexto: '#34684F', calmoSutil: '#E5F0EA',
  moderado: '#BBA15E',
  intenso: '#B2574D', intensoTexto: '#95463E', intensoSutil: '#FCE9E5',
};

const esc = (s) => String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const n = (x) => Number(x).toFixed(3);

const SIMBOLO = '<svg viewBox="0 0 48 48" aria-hidden="true"><g fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round">' +
  '<path d="M6 14 C10 9 14 9 18 14 S26 19 30 14 S38 9 42 14"/>' +
  '<path d="M6 24 C10 21.5 14 21.5 18 24 S26 26.5 30 24 S38 21.5 42 24"/>' +
  '<path d="M6 34 H42"/></g></svg>';
const logo = (id, estilo) => `<span id="${id}" class="hf-logo" style="${estilo}">${SIMBOLO}calmaria</span>`;
const FORMA = {
  circulo: '<svg viewBox="0 0 16 16" aria-hidden="true"><circle cx="8" cy="8" r="6.6" fill="currentColor"/></svg>',
  losango: '<svg viewBox="0 0 16 16" aria-hidden="true"><path d="M8 .9 15.1 8 8 15.1 .9 8z" fill="currentColor"/></svg>',
};

const BASE_CSS = `
@font-face { font-family: "Atkinson Hyperlegible Next"; src: url("assets/fonts/next-400.ttf") format("truetype"); font-weight: 400; }
@font-face { font-family: "Atkinson Hyperlegible Next"; src: url("assets/fonts/next-500.ttf") format("truetype"); font-weight: 500; }
@font-face { font-family: "Atkinson Hyperlegible Next"; src: url("assets/fonts/next-600.ttf") format("truetype"); font-weight: 600; }
@font-face { font-family: "Atkinson Hyperlegible Next"; src: url("assets/fonts/next-700.ttf") format("truetype"); font-weight: 700; }
@font-face { font-family: "Atkinson Hyperlegible Mono"; src: url("assets/fonts/mono-400.ttf") format("truetype"); font-weight: 400; }
@font-face { font-family: "Atkinson Hyperlegible Mono"; src: url("assets/fonts/mono-600.ttf") format("truetype"); font-weight: 600; }
#root { position: absolute; inset: 0; overflow: hidden; font-family: "Atkinson Hyperlegible Next", sans-serif;
  color: ${COR.texto}; -webkit-font-smoothing: antialiased; }
#root * { box-sizing: border-box; }
#root p, #root h1, #root div { margin: 0; }
.hf-abs { position: absolute; }
.hf-fundo { position: absolute; inset: 0; }
.hf-mono { font-family: "Atkinson Hyperlegible Mono", monospace; }
.hf-cab { position: absolute; left: 96px; top: 54px; font-size: 40px; line-height: 1.2; color: ${COR.apoio}; }
.hf-tit { position: absolute; left: 96px; right: 96px; top: 112px; font-size: 64px; line-height: 1.15; font-weight: 700;
  letter-spacing: -0.01em; }
.hf-logo { display: inline-flex; align-items: center; gap: .35em; font-weight: 700; letter-spacing: -.01em; line-height: 1;
  white-space: nowrap; }
.hf-logo svg { width: 1.5em; height: 1.5em; flex: none; }
.hf-cartao { background: ${COR.cartao}; border: 2px solid ${COR.borda}; border-radius: 16px; }
.hf-painel { background: ${COR.escuro}; color: ${COR.videoTexto}; border-radius: 16px; padding: 28px 36px;
  font-family: "Atkinson Hyperlegible Mono", monospace; font-size: 40px; line-height: 56px; }
.hf-linha { position: relative; display: flex; height: 56px; white-space: pre; }
.hf-linha .num { position: relative; z-index: 1; width: 96px; flex: none; color: ${COR.escuroSutil}; }
.hf-linha .cod { position: relative; z-index: 1; }
.hf-realce { position: absolute; left: -36px; right: -36px; top: 0; bottom: 0; background: ${COR.escuroCartao};
  border-left: 8px solid ${COR.videoAcao}; z-index: 0; }
.hf-rotulo { position: absolute; right: 0; top: 4px; height: 48px; line-height: 48px; padding: 0 20px; border-radius: 999px;
  background: ${COR.videoSup}; color: ${COR.videoTexto}; font-family: "Atkinson Hyperlegible Next", sans-serif;
  font-size: 40px; z-index: 2; }
.hf-chip { display: inline-flex; align-items: center; height: 64px; padding: 0 28px; border-radius: 999px; font-size: 40px;
  border: 3px solid ${COR.bordaForte}; color: ${COR.texto}; white-space: nowrap; }
.hf-chip.marca { border-color: ${COR.marca}; color: ${COR.marca}; }
.hf-chip.cheio { background: ${COR.marca}; border-color: ${COR.marca}; color: #FFFFFF; }
.hf-m { position: relative; white-space: nowrap; }
.hf-m .bar { position: absolute; left: 0; right: 0; bottom: -4px; height: 8px; border-radius: 4px; background: ${COR.marca}; }
.hf-tira { display: inline-block; padding: 0 10px; border-radius: 8px; background: ${COR.marcaSutil}; color: ${COR.marca};
  font-weight: 600; }
.hf-tira.escura { background: ${COR.videoSup}; color: ${COR.videoTexto}; }
.hf-num { font-family: "Atkinson Hyperlegible Mono", monospace; font-weight: 600; line-height: 1; }
.hf-lbl { font-size: 40px; line-height: 1.25; color: ${COR.apoio}; }
svg.hf-fios { position: absolute; left: 0; top: 0; width: 1920px; height: 1080px; overflow: visible; }
svg.hf-fios path { fill: none; stroke: ${COR.marca}; stroke-width: 4; stroke-linecap: round; stroke-dasharray: 1; }
`;

// Os verbos do movimento, iguais em todas as cenas. Toda entrada é fromTo (a cena é revista em qualquer ordem);
// saída e deslocamento não renderizam antes da hora (immediateRender: false).
const VERBOS = `
const entra = (s, t, o = {}) => tl.fromTo(s, { opacity: 0, y: o.y ?? 14 }, { opacity: 1, y: 0, duration: o.d ?? 0.5, ease: 'power3.out', stagger: o.s ?? 0 }, t);
const surge = (s, t, o = {}) => tl.fromTo(s, { opacity: 0 }, { opacity: o.ate ?? 1, duration: o.d ?? 0.5, ease: 'power1.out', stagger: o.s ?? 0 }, t);
const some = (s, t, o = {}) => tl.fromTo(s, { opacity: o.de ?? 1 }, { opacity: o.ate ?? 0, duration: o.d ?? 0.35, ease: 'power1.in', immediateRender: false }, t);
const risca = (s, t, o = {}) => tl.fromTo(s, { scaleX: 0 }, { scaleX: 1, duration: o.d ?? 0.45, ease: 'power2.inOut', transformOrigin: 'left center', stagger: o.s ?? 0 }, t);
const desenha = (s, t, o = {}) => tl.fromTo(s, { strokeDashoffset: 1 }, { strokeDashoffset: 0, duration: o.d ?? 0.6, ease: 'power2.inOut', stagger: o.s ?? 0 }, t);
const move = (s, t, de, para, o = {}) => tl.fromTo(s, de, { ...para, duration: o.d ?? 0.9, ease: o.ease ?? 'power2.inOut', immediateRender: false }, t);
const titulo = (s) => tl.fromTo(s, { opacity: 0, color: '${COR.escuroSutil}' }, { opacity: 1, color: '${COR.texto}', duration: 0.4, ease: 'power2.out' }, 0.05);
`;

function composicao(id, { html, js = '', css = '' }) {
  return `<template>
<style>${BASE_CSS}${css}</style>
<div id="root" data-composition-id="${id}" data-width="1920" data-height="1080">
${html}
</div>
<script>
(function () {
  const tl = gsap.timeline({ paused: true });
${VERBOS}
${js}
  window.__timelines["${id}"] = tl;
})();
</script>
</template>
`;
}

const fundo = (cor) => `<div class="hf-fundo" style="background:${cor}"></div>`;
const cabecalho = (c, id) => `${fundo(COR.fundo)}
<p class="hf-cab">${c.bloco} de 6 · ${esc(BLOCOS[c.bloco])}</p>
<h1 id="${id}-tit" class="hf-tit">${esc(c.titulo)}</h1>`;

/** Um fio curvo de (x1, y1) a (x2, y2), para os diagramas. */
const fio = (id, x1, y1, x2, y2, extra = '') => {
  const mx = (x1 + x2) / 2;
  return `<path id="${id}" pathLength="1" d="M${x1} ${y1} C${mx} ${y1} ${mx} ${y2} ${x2} ${y2}" ${extra}/>`;
};

/** Texto com marcas: [[texto|id]] vira um trecho marcado com barra por baixo. */
const marcado = (texto, prefixo) => esc(texto).replace(/\[\[(.+?)\|(\w+)\]\]/g,
  (_, t, id) => `<span class="hf-m" id="${prefixo}-${id}">${t}<i class="bar" id="${prefixo}-${id}-bar"></i></span>`);

/** Texto em que [removido] e <SAUDE> viram tiras (para mostrar o que o Sanitizador pôs no lugar). */
const comTiras = (texto, prefixo, classe = '') => {
  let i = 0;
  return esc(texto).replace(/\[removido\]|&lt;SAUDE&gt;/g, (m) => `<span class="hf-tira ${classe} ${prefixo}-tira" id="${prefixo}-tira${i++}">${m}</span>`);
};

/** Linhas de código num painel escuro, com número, realce e rótulo de achado. */
function codigo(prefixo, linhas, { recuo = {} } = {}) {
  return linhas.map((l, i) => {
    const ind = (l.cont ? 1 : 0) + (recuo[l.n] || 0);
    const realce = (l.achado || l.foco) ? `<span class="hf-realce ${prefixo}-realce" id="${prefixo}-r${i}"></span>` : '';
    const rotulo = l.achado && !l.cont ?`<span class="hf-rotulo" id="${prefixo}-a${i}">${esc(l.achado)}</span>` : '';
    return `<div class="hf-linha" id="${prefixo}-l${i}">${realce}<span class="num">${l.n ?? ''}</span>` +
      `<span class="cod">${' '.repeat(ind * 4)}${esc(l.t)}</span>${rotulo}</div>`;
  }).join('\n');
}

// ---------------------------------------------------------------------------------------------------------------
// As cenas. Cada uma recebe a cena do roteiro (c), os tempos (t) e os dados reais (d).
// t.ini[i] e t.dur[i]: começo e duração da fala i; t.a(i, 'termo'): o instante em que a voz diz o termo.
// ---------------------------------------------------------------------------------------------------------------

const CENAS = {
  abertura() {
    return {
      html: `${fundo(COR.video)}
<div class="hf-abs" style="inset:0;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:28px;color:${COR.videoTexto}">
  ${logo('ab-logo', 'font-size:120px')}
  <p id="ab-frase" style="font-size:48px;line-height:1.2;color:${COR.videoApoio}">Onde está calmo agora, em São Paulo</p>
</div>
<p id="ab-rod" class="hf-abs" style="left:96px;right:96px;bottom:80px;text-align:center;font-size:40px;color:${COR.videoApoio}">Global Solution 2026.2 · FIAP · Engenharia de Software</p>`,
      js: `surge('#ab-frase', 0.2, { d: 0.6 }); surge('#ab-rod', 0.45, { d: 0.6 });`,
    };
  },

  cartela(c, t, d, extra) {
    return {
      html: `${fundo(COR.fundo)}
<div class="hf-abs" style="left:96px;right:96px;top:0;bottom:0;display:flex;flex-direction:column;justify-content:center;gap:22px">
  <p style="font-size:40px;color:${COR.apoio}">${c.n} de 6</p>
  <p style="font-size:96px;line-height:1.1;font-weight:700;letter-spacing:-.01em;max-width:1500px">${esc(BLOCOS[c.n])}</p>
  <p class="hf-mono" style="font-size:40px;color:${COR.marca}">${esc(extra.tempo)}</p>
</div>
${logo(`c${c.n}-logo`, `position:absolute;right:96px;bottom:54px;font-size:40px;color:${COR.marca}`)}`,
      js: '',
    };
  },

  final() {
    const equipe = EQUIPE.map((p, i) => `<div class="fi-p" id="fi-p${i}" style="margin-bottom:14px">
      <p style="font-size:40px;line-height:1.2;font-weight:600;color:${COR.videoTexto}">${esc(p.nome)}</p>
      <p class="hf-mono" style="font-size:40px;line-height:1.2;color:${COR.videoApoio}">RM ${esc(p.rm)}</p></div>`).join('');
    return {
      html: `${fundo(COR.video)}
${logo('fi-logo', `position:absolute;left:96px;top:54px;font-size:72px;color:${COR.videoTexto}`)}
<p id="fi-frase" class="hf-abs" style="left:96px;top:300px;width:820px;font-size:64px;line-height:1.2;font-weight:700;color:${COR.videoTexto}">Menos de 10 minutos a pé até um lugar calmo.</p>
<div id="fi-ods" class="hf-abs" style="left:96px;top:520px;display:flex;gap:16px">
  ${['ODS 3', 'ODS 10', 'ODS 11'].map((o) => `<span style="font-size:40px;padding:8px 24px;border:3px solid ${COR.videoAcao};border-radius:999px;color:${COR.videoTexto}">${o}</span>`).join('')}
</div>
<p id="fi-repo" class="hf-abs hf-mono" style="left:96px;bottom:54px;font-size:40px;color:${COR.videoApoio}">${esc(REPOSITORIO)}</p>
<div class="hf-abs" style="left:1060px;right:96px;top:180px">
  <p style="font-size:40px;color:${COR.videoAcao};margin-bottom:22px">Equipe</p>
  ${equipe}
</div>`,
      js: `surge('#fi-logo', 0.1, { d: 0.5 }); surge('#fi-frase', 0.25, { d: 0.5 }); surge('#fi-ods', 0.45, { d: 0.5 });
surge('#fi-repo', 0.6, { d: 0.5 }); entra('.fi-p', 0.5, { s: 0.12, y: 10 });`,
    };
  },

  // 1. O relato e as quatro saídas
  relato(c, t, d) {
    const comentario = d.relato.pedido.comentario.match(/Meu CPF é .+$/)[0];
    const texto = marcado(comentario.replace(/(\d{3}\.\d{3}\.\d{3}-\d{2})/, '[[$1|cpf]]').replace(/autista/, '[[autista|aut]]'), 're');
    const sinks = ['Broker MQTT público', 'Logs do servidor', 'Relatos públicos', 'LLM de outra empresa'];
    const termos = ['broker', 'logs', 'lista pública', 'LLM'];
    const ys = [360, 476, 592, 708];
    const fios = ys.map((y, i) => fio(`re-f${i}`, 860, 452, 1296, y + 48)).join('');
    const curtos = fio('re-fc', 860, 452, 905, 452);
    return {
      html: `${cabecalho(c, 're')}
<div id="re-cartao" class="hf-abs hf-cartao" style="left:96px;top:360px;width:1150px;padding:32px 40px">
  <p id="re-rot" class="hf-lbl" style="margin-bottom:8px">Comentário de um relato (fictício)</p>
  <p id="re-texto" style="font-size:62px;line-height:1.3;font-weight:500">${texto}</p>
</div>
<div id="re-leg" class="hf-abs" style="left:96px;top:768px;display:flex;flex-direction:column;gap:18px">
  <p id="re-leg-cpf" style="font-size:40px;display:flex;align-items:center;gap:20px"><i style="display:inline-block;width:56px;height:8px;border-radius:4px;background:${COR.marca}"></i>CPF</p>
  <p id="re-leg-aut" style="font-size:40px;display:flex;align-items:center;gap:20px"><i style="display:inline-block;width:56px;height:8px;border-radius:4px;background:${COR.marca}"></i>dado de saúde: dado pessoal sensível (LGPD, art. 5º, II)</p>
</div>
<svg class="hf-fios" viewBox="0 0 1920 1080">${fios}${curtos}</svg>
${sinks.map((s, i) => `<div id="re-s${i}" class="hf-abs hf-cartao" style="left:1300px;top:${ys[i]}px;width:524px;height:96px;display:flex;align-items:center;padding:0 28px;font-size:40px">${esc(s)}</div>`).join('\n')}
<div id="re-porta" class="hf-abs" style="left:900px;top:360px;width:300px;height:444px;border-radius:16px;background:${COR.marca};color:#FFFFFF;display:flex;align-items:center;justify-content:center;text-align:center;font-size:40px;line-height:1.25;font-weight:700">Módulo de<br>Privacidade</div>
<p id="re-nenhuma" class="hf-abs" style="left:1300px;top:840px;font-size:40px;color:${COR.apoio}">nenhuma recebe o dado</p>
<div id="re-tarja" class="hf-abs" style="left:96px;bottom:96px;background:${COR.video};color:${COR.videoTexto};padding:14px 32px;border-radius:12px">
  <p style="font-size:48px;line-height:1.2;font-weight:700">${esc(NARRADOR.nome)}</p>
  <p style="font-size:40px;line-height:1.2;color:${COR.videoApoio}">${esc(NARRADOR.parte)}</p>
</div>`,
      js: `titulo('#re-tit');
surge('#re-tarja', 0.3, { d: 0.5 }); some('#re-tarja', ${n(Math.min(t.ini[1] - 0.2, 6.5))}, { d: 0.4 });
entra('#re-cartao', ${n(t.a(0, 'relatos'))});
risca('#re-cpf-bar', ${n(t.a(1, 'CPF'))}); surge('#re-leg-cpf', ${n(t.a(1, 'CPF'))});
risca('#re-aut-bar', ${n(t.a(2, 'dado de saúde'))}); surge('#re-leg-aut', ${n(t.a(2, 'dado de saúde'))});
some('#re-leg, #re-rot', ${n(t.a(3, 'quatro saídas'))}, { d: 0.3 });
move('#re-cartao', ${n(t.a(3, 'quatro saídas'))}, { scale: 1, transformOrigin: '0 0' }, { scale: 0.66, transformOrigin: '0 0' }, { d: 0.9 });
${termos.map((termo, i) => `desenha('#re-f${i}', ${n(t.a(3, termo))}, { d: 0.6 }); entra('#re-s${i}', ${n(t.a(3, termo) + 0.25)}, { y: 0 });`).join('\n')}
entra('#re-porta', ${n(t.a(4, 'módulo'))}, { y: 0, d: 0.5 });
${ys.map((_, i) => `some('#re-f${i}', ${n(t.a(4, 'nenhuma'))}, { ate: 0.22, d: 0.6 });`).join(' ')}
desenha('#re-fc', ${n(t.a(4, 'nenhuma'))}, { d: 0.3 });
surge('#re-nenhuma', ${n(t.a(4, 'nenhuma') + 0.2)});`,
    };
  },

  // 2. O motor e as nove regras
  motor(c, t, d) {
    const cpf = d.relato.resposta.dados.removidos.find((r) => r.categoria === 'CPF');
    const original = d.relato.pedido.comentario.match(/Meu CPF é .+?autista\./)[0];
    const limpo = d.relato.resposta.dados.comentario.match(/Meu CPF é .+?<SAUDE>\./)[0];
    const textoA = marcado(original.replace(/(\d{3}\.\d{3}\.\d{3}-\d{2})/, '[[$1|cpf]]').replace(/autista/, '[[autista|aut]]'), 'mo');
    const cards = d.regras.map((r, i) => {
      const x = 96 + (i % 3) * 584;
      const y = 470 + Math.floor(i / 3) * 144;
      return `<div id="mo-r${i}" class="hf-abs" style="left:${x}px;top:${y}px;width:560px;height:128px">
  <div id="mo-rf${i}" class="hf-abs hf-cartao" style="inset:0"></div>
  <div id="mo-rh${i}" class="hf-abs" style="inset:0;border-radius:16px;background:${COR.marcaSutil};border:4px ${r.json ? 'dashed' : 'solid'} ${COR.marca}"></div>
  <div class="hf-abs" style="left:28px;top:12px;right:20px">
    <p class="hf-mono" style="font-size:40px;line-height:52px;font-weight:600">${esc(r.id)}</p>
    <p style="font-size:40px;line-height:52px;color:${COR.apoio}">peso ${r.peso}${r.json ? `<span id="mo-j${i}"> · do JSON</span>` : ''}</p>
  </div></div>`;
    }).join('\n');
    const idx = (id) => d.regras.findIndex((r) => r.id === id);
    const jsons = d.regras.map((r, i) => (r.json ? i : -1)).filter((i) => i >= 0);
    return {
      html: `${cabecalho(c, 'mo')}
<div class="hf-abs hf-cartao" style="left:96px;top:240px;width:1728px;height:200px;padding:24px 40px">
  <div class="hf-abs" style="left:40px;top:24px;right:40px">
    <p id="mo-a" style="font-size:48px;line-height:1.3;font-weight:500">${textoA}</p>
    <p id="mo-b" class="hf-abs" style="left:0;top:0;font-size:48px;line-height:1.3;font-weight:500">${comTiras(limpo, 'mo')}</p>
  </div>
  <p id="mo-rem" class="hf-abs hf-mono" style="left:40px;bottom:20px;font-size:40px;color:${COR.apoio}">removidos: CPF ${esc(cpf.trecho)} · SAUDE ${esc('<SAUDE>')}</p>
</div>
${cards}`,
      js: `titulo('#mo-tit');
${d.regras.map((_, i) => `tl.set('#mo-rh${i}', { opacity: 0 }, 0);`).join(' ')}
entra('${d.regras.map((_, i) => `#mo-r${i}`).join(',')}', ${n(t.a(0, 'regras'))}, { s: 0.07, y: 10 });
surge('#mo-rh${idx('cpf')}', ${n(t.a(1, 'CPF'))}); risca('#mo-cpf-bar', ${n(t.a(1, 'CPF'))});
surge('#mo-rh${idx('saude')}', ${n(t.a(1, 'saúde'))}); risca('#mo-aut-bar', ${n(t.a(1, 'saúde'))});
tl.set('#mo-b', { opacity: 0 }, 0); tl.set('#mo-rem', { opacity: 0 }, 0);
some('#mo-a', ${n(t.a(2, 'mascarado'))}, { d: 0.35 }); surge('#mo-b', ${n(t.a(2, 'mascarado') + 0.3)}, { d: 0.45 });
surge('#mo-rem', ${n(t.a(2, 'texto original'))});
some('#mo-rh${idx('cpf')}, #mo-rh${idx('saude')}', ${n(t.ini[3] - 0.1)}, { d: 0.4 });
${jsons.map((i) => `surge('#mo-rh${i}', ${n(t.a(3, i === jsons[0] ? 'RG' : 'Cartão SUS'))}); surge('#mo-j${i}', ${n(t.a(3, 'JSON'))});`).join('\n')}`,
    };
  },

  // 2. A amostra com armadilhas
  amostra(c, t, d) {
    const { total, com } = d.amostra;
    const quadrados = Array.from({ length: total }, (_, i) => {
      const x = 96 + (i % 15) * 78;
      const y = 260 + Math.floor(i / 15) * 78;
      const cheio = i < com;
      return `<i class="hf-abs am-q" style="left:${x}px;top:${y}px;width:60px;height:60px;border-radius:10px;${cheio ? `background:${COR.marca}` : `border:4px solid ${COR.bordaForte}`}"></i>`;
    }).join('');
    return {
      html: `${cabecalho(c, 'am')}
${quadrados}
<p id="am-com" class="hf-abs" style="left:1340px;top:270px;font-size:40px;display:flex;align-items:center;gap:20px"><i style="display:inline-block;width:40px;height:40px;border-radius:8px;background:${COR.marca}"></i>${com} com dado pessoal</p>
<p id="am-sem" class="hf-abs" style="left:1340px;top:350px;font-size:40px;display:flex;align-items:center;gap:20px"><i style="display:inline-block;width:40px;height:40px;border-radius:8px;border:4px solid ${COR.bordaForte}"></i>${d.amostra.sem} sem</p>
<p id="am-arm" class="hf-abs hf-lbl" style="left:96px;top:620px">Armadilhas</p>
<div class="hf-abs" style="left:96px;top:684px;display:flex;gap:16px">
  <span id="am-a0" class="hf-chip">crise econômica</span><span id="am-a1" class="hf-chip">teatro</span><span id="am-a2" class="hf-chip">Tea time</span>
</div>
<div class="hf-abs" style="left:96px;top:830px;display:flex;align-items:center;gap:28px">
  <span id="am-tel" class="hf-chip marca hf-mono">telefone</span>
  <span id="am-err" style="position:relative;font-size:40px">marcava número de protocolo<i id="am-x" class="hf-abs" style="left:-6px;right:-6px;top:50%;height:5px;background:${COR.texto}"></i></span>
  <span id="am-ok" style="font-size:40px;font-weight:700;color:${COR.marca}">regra corrigida</span>
</div>`,
      js: `titulo('#am-tit');
surge('.am-q', ${n(t.a(0, 'escrevemos'))}, { s: 0.018, d: 0.3 });
surge('#am-com', ${n(t.a(0, 'com dado'))}); surge('#am-sem', ${n(t.a(0, 'sem.'))});
surge('#am-arm', ${n(t.a(1, 'armadilhas'))});
entra('#am-a0', ${n(t.a(1, 'crise'))}, { y: 8 }); entra('#am-a1', ${n(t.a(1, 'teatro'))}, { y: 8 }); entra('#am-a2', ${n(t.a(1, 'Tea time'))}, { y: 8 });
entra('#am-tel', ${n(t.a(2, 'telefone'))}, { y: 8 }); surge('#am-err', ${n(t.a(2, 'marcava'))});
risca('#am-x', ${n(t.a(2, 'corrigida'))}); surge('#am-ok', ${n(t.a(2, 'corrigida') + 0.3)});`,
    };
  },

  // 2. O resultado medido
  resultado(c, t, d) {
    const total = d.precisao.find((p) => p.regra === 'TOTAL');
    const x90 = 1100 + 724 * (d.piso / 100);
    return {
      html: `${cabecalho(c, 'rs')}
<div id="rs-p" class="hf-abs" style="left:96px;top:250px"><p class="hf-num" style="font-size:132px">${esc(total.precisao)}</p><p class="hf-lbl" style="margin-top:12px">precisão</p></div>
<div id="rs-r" class="hf-abs" style="left:560px;top:250px"><p class="hf-num" style="font-size:132px">${esc(total.revocacao)}</p><p class="hf-lbl" style="margin-top:12px">revocação</p></div>
<p id="rs-9" class="hf-abs" style="left:1100px;top:262px;font-size:40px">nas ${d.precisao.length - 1} regras, nesta amostra</p>
<div id="rs-trilho" class="hf-abs" style="left:1100px;top:350px;width:724px;height:28px;border-radius:14px;background:${COR.borda}"></div>
<div id="rs-barra" class="hf-abs" style="left:1100px;top:350px;width:724px;height:28px;border-radius:14px;background:${COR.marca}"></div>
<div id="rs-piso" class="hf-abs" style="left:${x90 - 3}px;top:330px;width:6px;height:68px;background:${COR.texto}"></div>
<p id="rs-piso-t" class="hf-abs" style="right:96px;top:412px;font-size:40px;color:${COR.apoio}">o teste exige ${d.piso}% em cada regra</p>
<div id="rs-lim" class="hf-abs hf-cartao" style="left:96px;top:560px;width:1728px;padding:24px 36px;font-size:40px;line-height:1.3">
  <b style="color:${COR.marca}">Limite:</b> a amostra foi escrita pela equipe; o resultado vale para ela.</div>
<div id="rs-tm" class="hf-abs" style="left:96px;top:760px"><p class="hf-num" style="font-size:120px">${d.testesModulo}</p><p class="hf-lbl" style="margin-top:12px">testes no módulo de Privacidade</p></div>
<div id="rs-tp" class="hf-abs" style="left:900px;top:760px"><p class="hf-num" style="font-size:120px">${d.testesTotal}</p><p class="hf-lbl" style="margin-top:12px">testes no projeto</p></div>`,
      js: `titulo('#rs-tit');
entra('#rs-p', ${n(t.a(0, 'precisão'))}); entra('#rs-r', ${n(t.a(0, 'revocação'))});
surge('#rs-9, #rs-trilho', ${n(t.a(0, 'revocação') + 0.3)}); risca('#rs-barra', ${n(t.a(0, 'revocação') + 0.5)}, { d: 0.8 });
surge('#rs-piso, #rs-piso-t', ${n(t.a(0, 'abaixo'))});
entra('#rs-lim', ${n(t.ini[1])});
entra('#rs-tm', ${n(t.a(2, String(d.testesModulo)))}); entra('#rs-tp', ${n(t.a(2, String(d.testesTotal)))});`,
    };
  },

  // 3. As três camadas
  camadas(c, t) {
    const origens = ['comentário do relato', 'apelido', 'contato de apoio'];
    const oTermos = ['comentário', 'apelido', 'contato'];
    const oy = [330, 434, 538];
    const saidas = ['log', 'resposta pública', 'LLM', 'broker MQTT', 'arquivos'];
    const sTermos = ['o log', 'resposta pública', 'LLM', 'broker', 'arquivos'];
    const sy = [300, 392, 484, 576, 668];
    const fios = [
      ...oy.map((y, i) => fio(`ca-fo${i}`, 600, y + 44, 786, 468)),
      ...sy.map((y, i) => fio(`ca-fs${i}`, 1134, 468, 1320, y + 40)),
    ].join('');
    const camadas = [['tipo', 'compilação'], ['guardas', 'execução'], ['Semgrep', 'estático']];
    return {
      html: `${cabecalho(c, 'ca')}
<p id="ca-lo" class="hf-abs hf-lbl" style="left:96px;top:250px">Origem</p>
<p id="ca-ls" class="hf-abs hf-lbl" style="left:1324px;top:226px">Saídas inseguras</p>
<svg class="hf-fios" viewBox="0 0 1920 1080">${fios}</svg>
${origens.map((o, i) => `<div id="ca-o${i}" class="hf-abs hf-cartao" style="left:96px;top:${oy[i]}px;width:504px;height:88px;display:flex;align-items:center;padding:0 28px;font-size:40px">${esc(o)}</div>`).join('\n')}
${saidas.map((s, i) => `<div id="ca-s${i}" class="hf-abs hf-cartao" style="left:1324px;top:${sy[i]}px;width:500px;height:80px;display:flex;align-items:center;padding:0 28px;font-size:40px">${esc(s)}</div>`).join('\n')}
<div id="ca-san" class="hf-abs" style="left:790px;top:400px;width:344px;height:136px;border-radius:16px;background:${COR.marca};color:#FFFFFF;display:flex;align-items:center;justify-content:center;font-size:48px;font-weight:700">Sanitizador</div>
<div class="hf-abs" style="left:96px;top:860px;display:flex;gap:24px">
  ${camadas.map(([a, b], i) => `<span id="ca-c${i}" class="hf-chip marca">${a} · ${b}</span>`).join('')}
</div>`,
      js: `titulo('#ca-tit');
surge('#ca-lo, #ca-ls', ${n(t.a(0, 'origem'))});
${oTermos.map((o, i) => `entra('#ca-o${i}', ${n(t.a(1, o))}, { y: 8 });`).join(' ')}
${sTermos.map((s, i) => `entra('#ca-s${i}', ${n(t.a(1, s))}, { y: 8 });`).join(' ')}
entra('#ca-san', ${n(t.a(2, 'Sanitizador'))}, { y: 0 });
desenha('${oy.map((_, i) => `#ca-fo${i}`).join(',')}', ${n(t.a(2, 'Sanitizador') + 0.2)}, { s: 0.08 });
desenha('${sy.map((_, i) => `#ca-fs${i}`).join(',')}', ${n(t.a(2, 'Sanitizador') + 0.5)}, { s: 0.08 });
${['tipo', 'guardas', 'Semgrep'].map((k, i) => `entra('#ca-c${i}', ${n(t.a(2, k))}, { y: 8 });`).join(' ')}`,
    };
  },

  // 3. O Semgrep no controller vulnerável
  semgrep(c, t, d) {
    const linhas = d.codigoVulneravel;
    const termo = { 'senha no código': 'senha', log: 'o log', console: 'console', LLM: 'LLM' };
    const marcadas = linhas.map((l, i) => ({ ...l, i })).filter((l) => l.achado);
    const stats = [
      [d.semgrep.antes, 'no controller vulnerável', 'sg-n0', 'pontos'],
      [d.semgrep.depois, 'no controller corrigido', 'sg-n1', 'corrigido'],
      [d.semgrep.api, `nos ${d.semgrep.arquivosDaApi} arquivos da API`, 'sg-n2', 'arquivos'],
    ];
    return {
      html: `${cabecalho(c, 'sg')}
<div id="sg-cod" class="hf-abs hf-painel" style="left:96px;top:226px;width:1728px">
  <div class="hf-linha" style="color:${COR.videoAcao}">security/exemplos/RelatoControllerVulneravel.cs</div>
  ${codigo('sg', linhas)}
</div>
${stats.map(([v, rot, id], i) => `<div id="${id}" class="hf-abs" style="left:${96 + i * 584}px;top:800px;width:560px"><p class="hf-num" style="font-size:96px">${v}</p><p class="hf-lbl" style="margin-top:10px">${esc(rot)}</p></div>`).join('\n')}`,
      js: `titulo('#sg-tit');
entra('#sg-cod', ${n(t.a(0, 'Semgrep'))});
entra('#sg-n0', ${n(t.a(1, 'pontos'))});
${marcadas.map((l) => `surge('#sg-r${l.i}', ${n(t.a(1, termo[l.achado]))}, { d: 0.35 });${l.cont ? '' : ` entra('#sg-a${l.i}', ${n(t.a(1, termo[l.achado]))}, { y: 0, d: 0.35 });`}`).join('\n')}
entra('#sg-n1', ${n(t.a(2, 'corrigido'))}); entra('#sg-n2', ${n(t.a(2, 'arquivos'))});`,
    };
  },

  // 3. Pelo tipo
  tipo(c, t, d) {
    const linhas = d.erros.map((e, i) => `<div class="tp-e" id="tp-e${i}" style="display:flex;height:64px;align-items:center">
  <span class="hf-mono" style="width:200px;color:${COR.escuroSutil}">(${e.linha},${e.coluna})</span>
  <span class="hf-mono" style="width:200px;font-weight:600">${e.codigo}</span>
  <span style="font-family:'Atkinson Hyperlegible Next',sans-serif;color:${COR.videoApoio}">${esc(e.porque)}</span></div>`).join('\n');
    return {
      html: `${cabecalho(c, 'tp')}
<div class="hf-abs" style="left:96px;top:240px;display:flex;align-items:center;gap:24px;font-size:40px">
  <span id="tp-k0" class="hf-chip hf-mono" style="border-radius:12px">TextoNaoConfiavel</span>
  <span id="tp-s0" style="font-size:48px;color:${COR.apoio}">→</span>
  <span id="tp-k1" class="hf-chip cheio" style="border-radius:12px;font-weight:700">Sanitizador</span>
  <span id="tp-s1" style="font-size:48px;color:${COR.apoio}">→</span>
  <span id="tp-k2" class="hf-chip marca hf-mono" style="border-radius:12px">TextoSanitizado</span>
</div>
<div id="tp-cod" class="hf-abs hf-painel" style="left:96px;top:380px;width:1728px;line-height:64px">
  <div style="height:64px;color:${COR.videoAcao}">&gt; dotnet build, com a tentativa colada na API</div>
  ${linhas}
  <div id="tp-fim" style="height:64px;font-weight:600">Build: FALHOU, como devia</div>
</div>`,
      js: `titulo('#tp-tit');
entra('#tp-k0', ${n(t.a(0, 'TextoNaoConfiavel'))}, { y: 8 });
surge('#tp-s0, #tp-k1, #tp-s1', ${n(t.a(0, 'só aceitam'))}, { s: 0.12 });
entra('#tp-k2', ${n(t.a(0, 'TextoSanitizado'))}, { y: 8 });
entra('#tp-cod', ${n(t.ini[1])});
surge('.tp-e', ${n(t.a(1, 'build'))}, { s: 0.2, d: 0.3 });
surge('#tp-fim', ${n(t.a(1, 'build') + 0.2 * d.erros.length + 0.15)});`,
    };
  },

  // 3. A correção e os guardas
  correcao(c, t, d) {
    const linhas = d.codigoCorrigido;
    const focos = (k) => linhas.map((l, i) => (l.foco === k ? `#cr-r${i}` : null)).filter(Boolean).join(',');
    return {
      html: `${cabecalho(c, 'cr')}
<div id="cr-cod" class="hf-abs hf-painel" style="left:96px;top:226px;width:1728px">
  <div class="hf-linha" style="color:${COR.videoAcao}">security/exemplos/RelatoControllerCorrigido.cs</div>
  ${codigo('cr', linhas, { recuo: { 78: 1, 82: 1 } })}
</div>
<div id="cr-g0" class="hf-abs hf-cartao" style="left:96px;top:790px;width:852px;padding:24px 32px">
  <p class="hf-mono" style="font-size:40px;line-height:1.3;font-weight:600">LoggerProviderComRedacao</p>
  <p class="hf-lbl">mascara o dado na linha de log</p></div>
<div id="cr-g1" class="hf-abs hf-cartao" style="left:972px;top:790px;width:852px;padding:24px 32px">
  <p class="hf-mono" style="font-size:40px;line-height:1.3;font-weight:600">FiltroRespostaPublica</p>
  <p class="hf-lbl">mascara o JSON das rotas públicas</p></div>`,
      js: `titulo('#cr-tit');
entra('#cr-cod', 0.3);
surge('${focos('sanitizar')}', ${n(t.a(0, 'Sanitizador'))}, { d: 0.35 });
surge('${focos('achado')}', ${n(t.a(0, 'achado'))}, { d: 0.35 });
surge('${focos('texto')}', ${n(t.a(0, 'mascarado') + 0.5)}, { d: 0.35 });
entra('#cr-g0', ${n(t.a(1, 'o log'))}); entra('#cr-g1', ${n(t.a(1, 'JSON'))});`,
    };
  },

  // 4. O relato na API
  api(c, t, d) {
    const r = d.relato;
    const comentario = (txt, marcas) => (marcas ? comTiras(txt, 'ap', 'escura') : esc(txt));
    const removidos = r.resposta.dados.removidos.map((x, i) =>
      `<div class="ap-rm" id="ap-rm${i}" style="white-space:pre">    { "categoria": "${esc(x.categoria)}", "trecho": "${esc(x.trecho)}" }</div>`).join('');
    return {
      html: `${cabecalho(c, 'ap')}
<div id="ap-req" class="hf-abs hf-painel" style="left:96px;top:230px;width:1728px">
  <div style="color:${COR.videoAcao};font-weight:600">&gt; POST /api/relatos</div>
  <div style="white-space:pre-wrap">"comentario": "${comentario(r.pedido.comentario)}"</div>
</div>
<div id="ap-res" class="hf-abs hf-painel" style="left:96px;top:500px;width:1728px">
  <div style="color:${COR.videoAcao};font-weight:600">&lt; ${r.resposta.status} Created</div>
  <div style="white-space:pre-wrap">"comentario": "${comentario(r.resposta.dados.comentario, true)}"</div>
  <div id="ap-rmc">"removidos": [</div>${removidos}<div id="ap-rmf">]</div>
</div>`,
      js: `titulo('#ap-tit');
entra('#ap-req', ${n(t.a(0, 'envia'))});
entra('#ap-res', ${n(t.a(1, 'resposta'))});
tl.set('.ap-tira', { opacity: 0.55 }, 0);
tl.fromTo('#ap-tira0', { opacity: 0.55 }, { opacity: 1, duration: 0.3, immediateRender: false }, ${n(t.a(1, 'removido'))});
tl.fromTo('#ap-tira1', { opacity: 0.55 }, { opacity: 1, duration: 0.3, immediateRender: false }, ${n(t.a(1, 'marca de saúde'))});
surge('#ap-rmc, .ap-rm, #ap-rmf', ${n(t.a(1, 'lista'))}, { s: 0.15 });`,
    };
  },

  // 4. As saídas
  saidas(c, t, d) {
    const itens = [
      ['No banco (PostgreSQL)', d.textoDoBanco, 'banco'],
      ['Na listagem pública, sem o autor', d.relato.publico.comentario, 'listagem'],
      ['No log do corpo da requisição', `"comentario": "${d.logMascarado}"`, 'log'],
    ];
    return {
      html: `${cabecalho(c, 'sa')}
${itens.map(([rot, txt], i) => `<div id="sa-c${i}" class="hf-abs hf-cartao" style="left:96px;top:${236 + i * 236}px;width:1728px;height:212px;padding:22px 36px">
  <p style="font-size:40px;line-height:1.25;font-weight:600;color:${COR.marca};margin-bottom:8px">${esc(rot)}</p>
  <p style="font-size:40px;line-height:1.3">${comTiras(txt, `sa${i}`)}</p></div>`).join('\n')}`,
      js: `titulo('#sa-tit');
${itens.map(([, , termo], i) => `entra('#sa-c${i}', ${n(t.a(0, termo))});`).join(' ')}
tl.fromTo('.sa0-tira, .sa1-tira, .sa2-tira', { backgroundColor: '${COR.marcaSutil}', color: '${COR.marca}' }, { backgroundColor: '${COR.marca}', color: '#FFFFFF', duration: 0.4, stagger: 0.1, immediateRender: false }, ${n(t.ini[1])});`,
    };
  },

  // 5. O achado mascarado vai ao LLM
  achado(c, t, d) {
    const cpf = d.relato.resposta.dados.removidos.find((r) => r.categoria === d.alerta.categoria);
    const campos = [
      ['categoria', d.alerta.categoria, 'categoria'],
      ['trecho', cpf ? cpf.trecho : '', 'trecho'],
      ['destino', d.alerta.destino, 'destino'],
      ['barrado', d.alerta.mitigado ? 'sim' : 'não', 'barrado'],
    ];
    const fios = [fio('ac-f0', 780, 470, 892, 470), fio('ac-f1', 1328, 440, 1436, 392), fio('ac-f2', 1328, 500, 1436, 560)].join('');
    return {
      html: `${cabecalho(c, 'ac')}
<div id="ac-card" class="hf-abs hf-cartao" style="left:96px;top:260px;width:684px;padding:28px 32px">
  <p class="hf-mono" style="font-size:40px;line-height:1.3;font-weight:600;color:${COR.marca};margin-bottom:12px">AchadoMascarado</p>
  ${campos.map(([k, v], i) => `<div id="ac-c${i}" style="display:flex;height:64px;align-items:center;font-size:40px"><span style="width:210px;color:${COR.apoio}">${k}</span><span class="hf-mono">${esc(v)}</span></div>`).join('')}
</div>
<svg class="hf-fios" viewBox="0 0 1920 1080">${fios}</svg>
<div id="ac-llm" class="hf-abs" style="left:896px;top:330px;width:432px;height:280px;border-radius:16px;border:4px solid ${COR.marca};background:${COR.cartao};display:flex;flex-direction:column;align-items:center;justify-content:center;gap:10px">
  <p style="font-size:72px;font-weight:700;line-height:1">LLM</p><p class="hf-mono" style="font-size:40px;color:${COR.apoio}">chat/completions</p></div>
<div id="ac-o0" class="hf-abs hf-cartao" style="left:1440px;top:330px;width:384px;height:124px;padding:16px 26px">
  <p style="font-size:40px;line-height:1.2;font-weight:600">Para o cidadão</p><p class="hf-lbl">frase literal</p></div>
<div id="ac-o1" class="hf-abs hf-cartao" style="left:1440px;top:498px;width:384px;height:124px;padding:16px 26px">
  <p style="font-size:40px;line-height:1.2;font-weight:600">Para a equipe</p><p class="hf-lbl">a correção</p></div>
<div id="ac-pr" class="hf-abs" style="left:896px;top:668px;font-size:40px;line-height:1.35;white-space:nowrap">
  <p>prompt versionado em</p><p class="hf-mono">Remediacao/prompt-alerta.md</p>
  <p style="color:${COR.apoio}">tempo limite, nova tentativa e disjuntor</p>
  <p style="color:${COR.apoio}">sem LLM: a frase pronta de reserva</p></div>`,
      js: `titulo('#ac-tit');
entra('#ac-card', ${n(t.a(0, 'achado mascarado'))});
tl.set('${campos.map((_, i) => `#ac-c${i}`).join(',')}', { opacity: 0.25 }, 0);
${campos.map(([, , termo], i) => `tl.fromTo('#ac-c${i}', { opacity: 0.25 }, { opacity: 1, duration: 0.35, immediateRender: false }, ${n(t.a(0, termo))});`).join('\n')}
desenha('#ac-f0', ${n(t.a(1, 'prompt'))}, { d: 0.4 }); entra('#ac-llm', ${n(t.a(1, 'prompt') + 0.2)}, { y: 0 });
surge('#ac-pr', ${n(t.a(1, 'versionado'))});
desenha('#ac-f1', ${n(t.a(1, 'cidadão'))}, { d: 0.4 }); entra('#ac-o0', ${n(t.a(1, 'cidadão'))}, { y: 0 });
desenha('#ac-f2', ${n(t.a(1, 'equipe'))}, { d: 0.4 }); entra('#ac-o1', ${n(t.a(1, 'equipe'))}, { y: 0 });`,
    };
  },

  // 5. O alerta
  alerta(c, t, d) {
    const a = d.alerta;
    const testes = ['resposta certa', 'erro', 'sem rede', 'fora do formato'];
    const ult = c.falas.length - 1;
    return {
      html: `${cabecalho(c, 'al')}
<div id="al-c" class="hf-abs hf-cartao" style="left:96px;top:240px;width:1728px;padding:28px 40px">
  <p style="font-size:40px;font-weight:600;color:${COR.marca};margin-bottom:10px">Para o cidadão</p>
  <p style="font-size:52px;line-height:1.3;font-weight:500">${esc(a.paraCidadao)}</p></div>
<div id="al-e" class="hf-abs" style="left:136px;top:530px;right:136px">
  <p style="font-size:40px;font-weight:600;color:${COR.apoio};margin-bottom:6px">Para a equipe</p>
  <p style="font-size:40px;line-height:1.3;color:${COR.apoio}">${esc(a.paraEquipe)}</p></div>
${d.doLlm ? '' : `<div id="al-m" class="hf-abs" style="left:96px;top:770px;display:flex;align-items:center;gap:20px;font-size:40px">
  <span class="hf-chip marca">frase pronta de reserva</span><span style="color:${COR.apoio}">${esc(a.motivo)}</span></div>`}
<p id="al-tl" class="hf-abs" style="left:96px;top:850px;font-size:40px;color:${COR.apoio}">A chamada ao LLM, testada com um serviço simulado:</p>
<div class="hf-abs" style="left:96px;top:912px;display:flex;align-items:center;gap:16px">
  ${testes.map((x, i) => `<span id="al-t${i}" class="hf-chip">${x}</span>`).join('')}</div>`,
      js: `titulo('#al-tit');
entra('#al-c', ${n(t.ini[0])});
entra('#al-e', ${n(t.a(0, 'sem alarme') + 0.6)});
${d.doLlm ? '' : `entra('#al-m', ${n(t.a(1, 'frase pronta'))}, { y: 8 });`}
surge('#al-tl', ${n(t.a(ult, 'simulado'))});
${testes.map((x, i) => `entra('#al-t${i}', ${n(t.a(ult, ['resposta certa', 'erro', 'sem rede', 'fora do formato'][i]))}, { y: 8 });`).join(' ')}`,
    };
  },

  // 6. O índice, de 0 a 100
  indice(c, t, d) {
    const x = (v) => 96 + 1728 * (v / 100);
    const { antes, depois } = d;
    const selo = (faixa) => {
      const alto = faixa === 'alto';
      const medio = faixa === 'médio' || faixa === 'medio';
      const [cor, sutil, forma, nome] = alto ? [COR.intensoTexto, COR.intensoSutil, FORMA.losango, 'Alto']
        : medio ? ['#855318', '#F0EDDF', '<svg viewBox="0 0 16 16"><rect x="1.6" y="1.6" width="12.8" height="12.8" rx="2" fill="currentColor"/></svg>', 'Médio']
          : [COR.calmoTexto, COR.calmoSutil, FORMA.circulo, 'Baixo'];
      return `<span style="display:inline-flex;align-items:center;gap:12px;height:60px;padding:0 22px;border-radius:999px;border:3px solid ${cor};background:${sutil};color:${cor};font-size:40px;font-weight:600"><span style="display:inline-flex;width:28px;height:28px">${forma}</span>${nome}</span>`;
    };
    const marcador = (id, v, rot, faixa) => `<div id="${id}" class="hf-abs" style="left:${x(v) - 300}px;top:380px;width:600px;height:240px">
  <div class="hf-abs" style="left:0;right:0;top:0;display:flex;justify-content:center;align-items:center;gap:16px">
    <span class="hf-mono" style="font-size:40px;font-weight:600">${rot} ${v}</span>${selo(faixa)}</div>
  <div class="hf-abs" style="left:297px;top:90px;width:6px;height:120px;background:${COR.texto};border-radius:3px"></div></div>`;
    return {
      html: `${cabecalho(c, 'ix')}
<p id="ix-f1" class="hf-abs" style="left:96px;top:236px;font-size:44px;font-weight:600">risco do item = peso do dado × peso da exposição</p>
<p id="ix-f2" class="hf-abs" style="left:96px;top:300px;font-size:40px;color:${COR.apoio}">× 1 se exposto, × 0,2 se um guarda barrou</p>
<div id="ix-faixa" class="hf-abs" style="left:96px;top:500px;width:1728px;height:48px;border-radius:24px;overflow:hidden;display:flex">
  <i style="width:20%;background:${COR.calmo}"></i><i style="width:30%;background:${COR.moderado}"></i><i style="width:50%;background:${COR.intenso}"></i></div>
<div id="ix-tk" class="hf-abs hf-mono" style="left:0;top:610px;width:1920px;height:56px;font-size:40px;color:${COR.apoio}">
  <span class="hf-abs" style="left:96px">0</span><span class="hf-abs" style="left:${x(20) - 24}px">20</span>
  <span class="hf-abs" style="left:${x(50) - 24}px">50</span><span class="hf-abs" style="right:96px">100</span></div>
${marcador('ix-antes', antes.indice, 'antes', antes.faixa)}
${marcador('ix-depois', depois.indice, 'depois', depois.faixa)}
<div id="ix-e" class="hf-abs" style="left:96px;top:740px"><p class="hf-num" style="font-size:120px">${depois.expostos}</p><p class="hf-lbl" style="margin-top:12px">dados expostos</p></div>
<div id="ix-b" class="hf-abs" style="left:700px;top:740px"><p class="hf-num" style="font-size:120px">${depois.barrados}</p><p class="hf-lbl" style="margin-top:12px">fluxos barrados</p></div>`,
      js: `titulo('#ix-tit');
risca('#ix-faixa', ${n(t.a(0, 'zero a cem') - 0.3)}, { d: 0.9 }); surge('#ix-tk', ${n(t.a(0, 'zero a cem'))});
surge('#ix-f1', ${n(t.a(1, 'peso do dado'))}); surge('#ix-f2', ${n(t.a(1, 'vinte por cento'))});
entra('#ix-antes', ${n(t.a(2, String(antes.indice)))}, { y: 0 });
tl.set('#ix-depois', { opacity: 0 }, 0);
tl.fromTo('#ix-depois', { opacity: 0, x: ${n(x(antes.indice) - x(depois.indice))} }, { opacity: 1, x: ${n(x(antes.indice) - x(depois.indice))}, duration: 0.01, immediateRender: false }, ${n(t.a(2, 'Depois'))});
move('#ix-depois', ${n(t.a(2, 'Depois') + 0.05)}, { x: ${n(x(antes.indice) - x(depois.indice))} }, { x: 0 }, { d: 1.6 });
some('#ix-antes', ${n(t.a(2, 'Depois') + 0.4)}, { ate: 0.4, d: 0.6 });
entra('#ix-e', ${n(t.a(2, depois.expostos === 0 ? 'nenhum' : 'expostos'))}); entra('#ix-b', ${n(t.a(2, 'fluxos'))});`,
    };
  },

  // 6. O painel de verdade
  painel(c, t) {
    // Paradas da câmera na captura (2880 x 4796, mostrada com 1728 de largura): o topo de cada seção.
    const paradas = [
      { y: 0, rot: 'Risco Cidadão', termo: null },
      { y: 600, rot: 'Risco por tipo de dado e por destino; alertas', termo: 'tipo de dado' },
      { y: 1540, rot: 'Fluxos bloqueados', termo: 'fluxos' },
      { y: 2082, rot: 'Fontes e histórico das varreduras', termo: 'histórico' },
    ];
    // Uma parada só começa quando a anterior terminou: a câmera nunca briga com ela mesma.
    let js = '';
    let livre = 0;
    paradas.forEach((p, i) => {
      if (i === 0) return;
      const d = Math.min(2.2, 0.9 + Math.abs(p.y - paradas[i - 1].y) / 1600);
      const ti = Math.max(t.a(0, p.termo), livre);
      livre = ti + d + 2.2;
      js += `move('#pa-img', ${n(ti)}, { y: ${-paradas[i - 1].y} }, { y: ${-p.y} }, { d: ${n(d)} });\n`;
      js += `some('#pa-r${i - 1}', ${n(ti)}, { d: 0.25 }); tl.fromTo('#pa-r${i}', { opacity: 0 }, { opacity: 1, duration: 0.35, immediateRender: false }, ${n(ti + d - 0.3)});\n`;
    });
    return {
      html: `${cabecalho(c, 'pa')}
<div id="pa-quadro" class="hf-abs" data-layout-allow-overflow style="left:96px;top:226px;width:1728px;height:800px;border-radius:16px;border:2px solid ${COR.borda};overflow:hidden;background:${COR.fundo}">
  <img id="pa-img" class="hf-abs" src="assets/img/painel-inteiro.png" style="left:0;top:0;width:1728px" alt="">
</div>
${paradas.map((p, i) => `<p id="pa-r${i}" class="hf-abs" style="right:136px;bottom:94px;padding:10px 28px;border-radius:12px;background:${COR.video};color:${COR.videoTexto};font-size:40px">${esc(p.rot)}</p>`).join('\n')}`,
      js: `titulo('#pa-tit');
${paradas.map((_, i) => (i ? `tl.set('#pa-r${i}', { opacity: 0 }, 0);` : '')).join(' ')}
surge('#pa-r0', 0.4);
${js}`,
    };
  },
};

/** Monta a composição de uma cena. extra: dados que só o gerador sabe (o tempo de cada bloco, para a cartela). */
export function montarCena(c, t, d, extra = {}) {
  const fazer = CENAS[c.tipo];
  if (!fazer) throw new Error(`Cena sem molde: ${c.tipo}`);
  return composicao(c.id, fazer(c, t, d, extra));
}
