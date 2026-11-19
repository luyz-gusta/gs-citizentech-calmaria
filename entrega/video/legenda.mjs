// A legenda .srt (kit VI-07): pt-BR, até 42 caracteres por linha, no máximo duas linhas por legenda.
// Cada fala vira uma ou mais legendas, cortadas na pontuação (nunca no meio de uma frase curta), e o tempo de cada
// pedaço sai da proporção de texto dentro do áudio daquela fala.

const LINHA = 42;
const BLOCO = LINHA * 2;
const MINIMO = 1.2; // segundos na tela, no mínimo

/** Quebra um texto em linhas de até 42 caracteres, equilibradas (duas linhas de tamanho parecido). */
function emLinhas(texto) {
  if (texto.length <= LINHA) return [texto];
  const palavras = texto.split(' ');
  let melhor = null;
  for (let i = 1; i < palavras.length; i++) {
    const a = palavras.slice(0, i).join(' ');
    const b = palavras.slice(i).join(' ');
    if (a.length > LINHA || b.length > LINHA) continue;
    const custo = Math.abs(a.length - b.length);
    if (!melhor || custo < melhor.custo) melhor = { custo, linhas: [a, b] };
  }
  if (!melhor) throw new Error(`Não coube em duas linhas de ${LINHA}: "${texto}"`);
  return melhor.linhas;
}

/** Corta uma fala em pedaços de até 84 caracteres, preferindo o fim de frase, depois ";", ":" e ",". */
function emPedacos(texto) {
  if (texto.length <= BLOCO) return [texto];
  const cortes = [/[.!?]\s/g, /[;:]\s/g, /,\s/g, /\s(e|ou|mas|que|com|para)\s/g, /\s/g];
  for (const padrao of cortes) {
    let melhor = null;
    for (const m of texto.matchAll(padrao)) {
      const fim = padrao.source.startsWith('\\s(') ? m.index : m.index + 1;
      const a = texto.slice(0, fim).trim();
      const b = texto.slice(fim).trim();
      if (a.length > BLOCO || a.length < 18 || b.length < 12) continue;
      const custo = Math.abs(a.length - Math.min(b.length, BLOCO));
      if (!melhor || custo < melhor.custo) melhor = { custo, a, b };
    }
    if (melhor) return [melhor.a, ...emPedacos(melhor.b)];
  }
  throw new Error(`Não consegui cortar a fala: "${texto}"`);
}

/**
 * falas: [{ texto, de, ate }] em segundos do vídeo. Devolve o conteúdo do .srt.
 * O pedaço de uma fala dura o tempo proporcional ao tamanho dele; nenhuma legenda fica menos de 1,2 s, se couber
 * antes da próxima.
 */
export function montarSrt(falas) {
  const cues = [];
  for (const f of falas) {
    const pedacos = emPedacos(f.texto);
    const total = pedacos.reduce((s, p) => s + p.length, 0);
    let t = f.de;
    for (const p of pedacos) {
      const d = (f.ate - f.de) * (p.length / total);
      cues.push({ de: t, ate: t + d, linhas: emLinhas(p) });
      t += d;
    }
  }
  for (let i = 0; i < cues.length; i++) {
    const proxima = cues[i + 1]?.de ?? Infinity;
    if (cues[i].ate - cues[i].de < MINIMO) cues[i].ate = Math.min(cues[i].de + MINIMO, proxima - 0.05);
  }
  const hora = (s) => {
    const ms = Math.round(s * 1000);
    const p = (n, z = 2) => String(n).padStart(z, '0');
    return `${p(Math.floor(ms / 3600000))}:${p(Math.floor((ms % 3600000) / 60000))}:${p(Math.floor((ms % 60000) / 1000))},${p(ms % 1000, 3)}`;
  };
  return cues.map((c, i) => `${i + 1}\n${hora(c.de)} --> ${hora(c.ate)}\n${c.linhas.join('\n')}\n`).join('\n');
}
