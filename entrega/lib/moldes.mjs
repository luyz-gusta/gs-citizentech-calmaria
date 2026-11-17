// Os moldes em HTML: o cartão de evidência (prints dos READMEs) e os slides do vídeo (1920 x 1080).
// Cores e fontes do design da Calmaria (design/tokens/resolvido.json). Nada inventado fora deles.

const FONTES = '<link rel="preconnect" href="https://fonts.googleapis.com"><link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>' +
  '<link href="https://fonts.googleapis.com/css2?family=Atkinson+Hyperlegible+Mono:wght@400;600&family=Atkinson+Hyperlegible+Next:wght@400;600;700&display=swap" rel="stylesheet">';

export const COR = {
  fundo: '#F1F5F7', cartao: '#F8FBFD', texto: '#14191F', apoio: '#4D545B', marca: '#3D5A73', marcaClara: '#B2C9D8',
  borda: '#E0E5E9', escuro: '#14191F', escuroCartao: '#22282E', escuroTexto: '#F8FBFD', escuroApoio: '#D4E1EA',
  video: '#1F2733', calmo: '#437E61', moderado: '#BBA15E', intenso: '#B2574D',
  calmoTexto: '#34684F', moderadoTexto: '#855318', intensoTexto: '#95463E',
};

export const esc = (s) => String(s ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');

function linhas(texto, destaque) {
  return String(texto).split('\n').map((l) => {
    const marcada = destaque && destaque.test(l);
    return marcada ? `<span class="d">${esc(l) || ' '}</span>` : `${esc(l)}\n`;
  }).join('');
}

/**
 * Cartão de evidência: título, uma frase do que prova e um ou mais blocos de terminal com a saída REAL do comando.
 * blocos: [{ rotulo, comando, saida, destaque: RegExp }]
 */
export function cartao({ titulo, legenda, blocos, rodape, largura = 1400 }) {
  const corpo = blocos.map((b) => `
    ${b.rotulo ? `<p class="rot">${esc(b.rotulo)}</p>` : ''}
    <pre class="t">${b.comando ? `<span class="cmd">&gt; ${esc(b.comando)}</span>\n` : ''}${linhas(b.saida, b.destaque)}</pre>`).join('');
  return `<!doctype html><html lang="pt-BR"><head><meta charset="utf-8">${FONTES}<style>
    *{box-sizing:border-box} body{margin:0;background:${COR.fundo};color:${COR.texto};font-family:"Atkinson Hyperlegible Next",system-ui,sans-serif}
    .c{width:${largura}px;padding:32px 36px}
    h1{font-size:28px;line-height:1.25;margin:0 0 8px;font-weight:700}
    .leg{margin:0 0 20px;color:${COR.apoio};font-size:19px;line-height:1.45}
    .rot{margin:18px 0 8px;font-size:17px;font-weight:600;color:${COR.marca}}
    .t{margin:0;background:${COR.escuro};color:${COR.escuroTexto};border-radius:12px;padding:20px 24px;
       font:16px/1.55 "Atkinson Hyperlegible Mono",ui-monospace,monospace;white-space:pre-wrap;word-break:break-word}
    .cmd{color:${COR.marcaClara};font-weight:600}
    .d{display:block;background:${COR.escuroCartao};border-left:3px solid ${COR.marcaClara};margin:0 -24px;padding:0 21px;font-weight:600}
    .rod{margin:16px 0 0;font-size:15px;color:${COR.apoio}}
  </style></head><body><div class="c">
    <h1>${esc(titulo)}</h1>${legenda ? `<p class="leg">${esc(legenda)}</p>` : ''}${corpo}
    ${rodape ? `<p class="rod">${esc(rodape)}</p>` : ''}
  </div></body></html>`;
}

/** Cartão com uma tabela (precisão e revocação, por exemplo). */
export function cartaoTabela({ titulo, legenda, cabecalho, linhas: dados, rodape, largura = 1400 }) {
  const ths = cabecalho.map((c) => `<th>${esc(c)}</th>`).join('');
  const trs = dados.map((l) => `<tr>${l.map((c) => `<td>${esc(c)}</td>`).join('')}</tr>`).join('');
  return `<!doctype html><html lang="pt-BR"><head><meta charset="utf-8">${FONTES}<style>
    *{box-sizing:border-box} body{margin:0;background:${COR.fundo};color:${COR.texto};font-family:"Atkinson Hyperlegible Next",system-ui,sans-serif}
    .c{width:${largura}px;padding:32px 36px}
    h1{font-size:28px;margin:0 0 8px} .leg{margin:0 0 20px;color:${COR.apoio};font-size:19px;line-height:1.45}
    table{width:100%;border-collapse:collapse;background:${COR.cartao};border:1px solid ${COR.borda};border-radius:12px;overflow:hidden;font-size:19px}
    th,td{padding:10px 16px;text-align:left;border-bottom:1px solid ${COR.borda}} th{color:${COR.apoio};font-weight:600}
    td{font-variant-numeric:tabular-nums} tr:last-child td{border-bottom:0;font-weight:700}
    .rod{margin:16px 0 0;font-size:15px;color:${COR.apoio}}
  </style></head><body><div class="c"><h1>${esc(titulo)}</h1>${legenda ? `<p class="leg">${esc(legenda)}</p>` : ''}
    <table><thead><tr>${ths}</tr></thead><tbody>${trs}</tbody></table>${rodape ? `<p class="rod">${esc(rodape)}</p>` : ''}</div></body></html>`;
}

// ---------- Slides do vídeo (VI-01: 1920 x 1080, margem segura de 96 px nas laterais e 54 em cima e embaixo,
// texto de 40 px no mínimo, nada animado) ----------

/** Slide com o cabeçalho do bloco ("2 de 6 · As regras de PII"), o título e o corpo em HTML. */
export function slide({ bloco, titulo, corpo, escuro = false }) {
  const fundo = escuro ? COR.video : COR.fundo;
  const texto = escuro ? COR.escuroTexto : COR.texto;
  const apoio = escuro ? COR.escuroApoio : COR.apoio;
  return `<!doctype html><html lang="pt-BR"><head><meta charset="utf-8">${FONTES}<style>
    *{box-sizing:border-box} html,body{margin:0;width:1920px;height:1080px;overflow:hidden}
    body{background:${fundo};color:${texto};font-family:"Atkinson Hyperlegible Next",system-ui,sans-serif;
         padding:54px 96px;display:flex;flex-direction:column;gap:28px}
    .bloco{font-size:40px;color:${apoio};margin:0}
    h1{font-size:64px;line-height:1.15;margin:0;font-weight:700;letter-spacing:-0.01em}
    .corpo{flex:1;min-height:0;display:flex;flex-direction:column;gap:28px}
    ul{margin:0;padding:0 0 0 1.1em;font-size:44px;line-height:1.35} li{margin:0 0 16px} li::marker{color:${COR.marca}}
    p{margin:0;font-size:44px;line-height:1.35} .apoio{color:${apoio}}
    pre{margin:0;background:${COR.escuro};color:${COR.escuroTexto};border-radius:16px;padding:28px 36px;
        font:40px/1.45 "Atkinson Hyperlegible Mono",ui-monospace,monospace;white-space:pre-wrap;word-break:break-word}
    pre .cmd{color:${COR.marcaClara};font-weight:600} pre .d{display:block;background:${COR.escuroCartao};
        border-left:6px solid ${COR.marcaClara};margin:0 -36px;padding:0 30px;font-weight:600}
    .img{flex:1;min-height:0;display:flex;align-items:center;justify-content:center}
    .img img{max-width:100%;max-height:100%;border-radius:16px;border:2px solid ${COR.borda};background:#fff}
    table{border-collapse:collapse;font-size:40px;width:100%} th,td{text-align:left;padding:10px 20px;border-bottom:2px solid ${COR.borda}}
    th{color:${apoio};font-weight:600} td.n{font-family:"Atkinson Hyperlegible Mono",monospace}
    .compacta th,.compacta td{padding:4px 16px}
    .duas{display:grid;grid-template-columns:1fr 1fr;gap:48px;flex:1;min-height:0}
    .pilula{display:inline-block;font-size:40px;padding:6px 24px;border-radius:999px;border:2px solid currentColor}
    .calmo{color:${COR.calmoTexto}} .moderado{color:${COR.moderadoTexto}} .intenso{color:${COR.intensoTexto}}
  </style></head><body>
    ${bloco ? `<p class="bloco">${esc(bloco)}</p>` : ''}
    ${titulo ? `<h1>${esc(titulo)}</h1>` : ''}
    <div class="corpo">${corpo}</div>
  </body></html>`;
}

export const lista = (itens) => `<ul>${itens.map((i) => `<li>${i}</li>`).join('')}</ul>`;
export const codigo = (texto, destaque) => `<pre>${linhas(texto, destaque)}</pre>`;
export const imagem = (src) => `<div class="img"><img src="${esc(src)}" alt=""></div>`;
export const tabela = (cabecalho, dados, numericas = []) =>
  `<table><thead><tr>${cabecalho.map((c) => `<th>${esc(c)}</th>`).join('')}</tr></thead><tbody>${
    dados.map((l) => `<tr>${l.map((c, i) => `<td${numericas.includes(i) ? ' class="n"' : ''}>${esc(c)}</td>`).join('')}</tr>`).join('')
  }</tbody></table>`;
