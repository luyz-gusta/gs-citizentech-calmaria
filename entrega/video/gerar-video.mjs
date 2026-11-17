// Gera o vídeo de Cybersecurity (roteiro de docs/disciplinas/cybersecurity.md, seis blocos) a partir das capturas
// reais de entrega/gerar-evidencias.mjs: slides em 1920 x 1080, narração com a voz pt-BR do Windows, legenda .srt
// e o MP4 (H.264, 30 qps, AAC 48 kHz, voz normalizada em -16 LUFS), como pede o kit de vídeo (VI-01 e VI-07).
//
// Uso (da raiz, depois do gerar-evidencias):  node entrega/video/gerar-video.mjs [--voz Maria] [--velocidade 1.0]
// Sai em entrega/saida/video/: calmaria-cybersecurity.mp4 e calmaria-cybersecurity.srt.
import { readFileSync, rmSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { roteiro } from './roteiro.mjs';
import { abrirNavegador } from '../lib/navegador.mjs';
import { slide } from '../lib/moldes.mjs';
import { RAIZ, SAIDA, pasta, rodar } from '../lib/util.mjs';

const args = process.argv.slice(2);
const opcao = (nome, padrao) => { const i = args.indexOf(`--${nome}`); return i >= 0 ? args[i + 1] : padrao; };
const VOZ = opcao('voz', 'Maria');
const VELOCIDADE = Number(opcao('velocidade', '1.0'));

const AMOSTRAS = 48000;           // por segundo, mono, 16 bits
const POR_QUADRO = AMOSTRAS / 30; // 1600 amostras por quadro: cada cena dura um número inteiro de quadros
const PAUSA_ENTRE_FRASES = 0.35;
const PAUSA_NO_COMECO = 0.4;
const PAUSA_NO_FIM = 0.7;

rmSync(join(SAIDA, 'video'), { recursive: true, force: true });
const VIDEO = pasta(SAIDA, 'video');
const SLIDES = pasta(VIDEO, 'slides');
const FALAS = pasta(VIDEO, 'falas');
const PECAS = resolve(RAIZ, 'design', 'pecas', 'png');

const tempo = (s) => `${Math.floor(s / 60)}:${String(Math.floor(s % 60)).padStart(2, '0')}`;
const srtTempo = (s) => {
  const ms = Math.round(s * 1000);
  const h = Math.floor(ms / 3600000); const m = Math.floor((ms % 3600000) / 60000);
  const seg = Math.floor((ms % 60000) / 1000); const resto = ms % 1000;
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(seg).padStart(2, '0')},${String(resto).padStart(3, '0')}`;
};

/** Quebra uma frase em legendas de até duas linhas de 42 caracteres. */
function legendas(texto) {
  const palavras = texto.split(/\s+/);
  const linhas = [];
  let atual = '';
  for (const p of palavras) {
    if ((atual ? `${atual} ${p}` : p).length > 42 && atual) { linhas.push(atual); atual = p; }
    else atual = atual ? `${atual} ${p}` : p;
  }
  if (atual) linhas.push(atual);
  const blocos = [];
  for (let i = 0; i < linhas.length; i += 2) blocos.push(linhas.slice(i, i + 2).join('\n'));
  return blocos;
}

async function main() {
  const cenas = roteiro();

  // 1. A narração: um WAV por frase, convertido para PCM cru de 48 kHz.
  const frases = [];
  cenas.forEach((c, ci) => (c.falas || []).forEach((f, fi) => frases.push({ id: `c${ci}-f${fi}`, fala: f.fala || f.texto })));
  writeFileSync(join(VIDEO, 'frases.json'), JSON.stringify(frases), 'utf8');
  console.log(`Narração: ${frases.length} frases com a voz ${VOZ}...`);
  const r = await rodar('powershell', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', resolve(RAIZ, 'entrega', 'video', 'falar.ps1'),
    '-Frases', join(VIDEO, 'frases.json'), '-Pasta', FALAS, '-Voz', VOZ, '-Velocidade', String(VELOCIDADE)]);
  if (r.codigo !== 0) throw new Error(`A narração falhou:\n${r.saida}`);

  const pcm = {};
  for (const f of frases) {
    const cru = join(FALAS, `${f.id}.pcm`);
    const c = await rodar('ffmpeg', ['-y', '-v', 'error', '-i', join(FALAS, `${f.id}.wav`), '-f', 's16le', '-ar', String(AMOSTRAS), '-ac', '1', cru]);
    if (c.codigo !== 0) throw new Error(`ffmpeg: ${c.saida}`);
    pcm[f.id] = readFileSync(cru);
  }

  // 2. A linha do tempo: cada cena com o áudio dela, arredondado para quadros inteiros.
  const silencio = (seg) => Buffer.alloc(Math.round(seg * AMOSTRAS) * 2);
  const partesDoAudio = [];
  const linhaDoTempo = [];
  const srt = [];
  let inicio = 0;
  cenas.forEach((c, ci) => {
    const pedacos = [];
    const cues = [];
    if (c.falas?.length) {
      pedacos.push(silencio(PAUSA_NO_COMECO));
      let t = PAUSA_NO_COMECO;
      c.falas.forEach((f, fi) => {
        if (fi > 0) { pedacos.push(silencio(PAUSA_ENTRE_FRASES)); t += PAUSA_ENTRE_FRASES; }
        const audio = pcm[`c${ci}-f${fi}`];
        const dur = audio.length / 2 / AMOSTRAS;
        const blocos = legendas(f.texto);
        const total = blocos.reduce((s, b) => s + b.length, 0);
        let tb = t;
        for (const b of blocos) { const d = dur * (b.length / total); cues.push({ de: tb, ate: tb + d, texto: b }); tb += d; }
        pedacos.push(audio);
        t += dur;
      });
      pedacos.push(silencio(PAUSA_NO_FIM));
    } else {
      pedacos.push(silencio(c.duracao));
    }
    let bloco = Buffer.concat(pedacos);
    const quadros = Math.ceil(bloco.length / 2 / POR_QUADRO);
    bloco = Buffer.concat([bloco, Buffer.alloc(quadros * POR_QUADRO * 2 - bloco.length)]);
    const duracao = quadros / 30;
    partesDoAudio.push(bloco);
    linhaDoTempo.push({ cena: c, inicio, duracao });
    for (const q of cues) srt.push({ de: inicio + q.de, ate: inicio + q.ate, texto: q.texto });
    inicio += duracao;
  });
  writeFileSync(join(VIDEO, 'narracao.pcm'), Buffer.concat(partesDoAudio));
  console.log(`Duração: ${tempo(inicio)} (${inicio.toFixed(1)} s).`);

  // 3. As cartelas de bloco (VI-04) com o tempo de cada bloco, pelo exportador do design.
  const blocos = [...new Set(cenas.filter((c) => c.bloco).map((c) => c.bloco.n))];
  for (const n of blocos) {
    const doBloco = linhaDoTempo.filter((l) => l.cena.bloco?.n === n);
    const de = doBloco[0].inicio;
    const ate = doBloco.at(-1).inicio + doBloco.at(-1).duracao;
    const cena = doBloco[0].cena;
    const saida = join(SLIDES, `cartela-${n}.png`);
    const e = await rodar('python', [resolve(RAIZ, 'design', 'pecas', 'exportar.py'), 'cartela', '--n', String(n), '--total',
      String(blocos.length), '--titulo', cena.bloco.titulo, '--tempo', `${tempo(de)} a ${tempo(ate)}`, '--saida', saida]);
    if (e.codigo !== 0) throw new Error(`exportar.py cartela: ${e.saida}`);
  }

  // 4. Os slides (as cenas com HTML), num navegador sem janela.
  const nav = await abrirNavegador();
  try {
    for (const [i, l] of linhaDoTempo.entries()) {
      const c = l.cena;
      if (c.tipo === 'cartela') { c.imagem = join(SLIDES, `cartela-${c.n}.png`); continue; }
      if (c.tipo === 'peca') { c.imagem = join(PECAS, c.arquivo); continue; }
      const html = join(SLIDES, `slide-${String(i).padStart(2, '0')}.html`);
      writeFileSync(html, slide({ bloco: c.cabecalho, titulo: c.titulo, corpo: c.corpo, escuro: c.escuro }));
      const aba = await nav.novaAba({ largura: 1920, altura: 1080, escala: 1 });
      await aba.ir(pathToFileURL(html).href);
      c.imagem = await aba.foto(html.replace(/\.html$/, '.png'));
      await aba.fechar();
    }
  } finally {
    await nav.fechar();
  }

  // 5. O vídeo: as imagens com a duração exata de cada cena e a narração por baixo.
  const lista = linhaDoTempo.map((l) => `file '${l.cena.imagem.replace(/\\/g, '/').replace(/'/g, "'\\''")}'\nduration ${l.duracao.toFixed(6)}`);
  lista.push(`file '${linhaDoTempo.at(-1).cena.imagem.replace(/\\/g, '/')}'`);
  writeFileSync(join(VIDEO, 'imagens.txt'), lista.join('\n'));
  writeFileSync(join(VIDEO, 'calmaria-cybersecurity.srt'),
    srt.map((q, i) => `${i + 1}\n${srtTempo(q.de)} --> ${srtTempo(q.ate)}\n${q.texto}\n`).join('\n'), 'utf8');
  const mp4 = join(VIDEO, 'calmaria-cybersecurity.mp4');
  console.log('Montando o MP4...');
  const f = await rodar('ffmpeg', ['-y', '-v', 'error',
    '-f', 'concat', '-safe', '0', '-i', join(VIDEO, 'imagens.txt'),
    '-f', 's16le', '-ar', String(AMOSTRAS), '-ac', '1', '-i', join(VIDEO, 'narracao.pcm'),
    '-vf', 'fps=30,scale=1920:1080:flags=lanczos,format=yuv420p',
    '-c:v', 'libx264', '-preset', 'medium', '-crf', '20', '-tune', 'stillimage',
    '-af', 'loudnorm=I=-16:TP=-1.5:LRA=11', '-c:a', 'aac', '-b:a', '160k', '-ar', '48000', '-ac', '2',
    '-movflags', '+faststart', '-t', inicio.toFixed(3), mp4]);
  if (f.codigo !== 0) throw new Error(`ffmpeg: ${f.saida}`);
  console.log(`Pronto: entrega/saida/video/calmaria-cybersecurity.mp4 e .srt (${tempo(inicio)}).`);
}

main().catch((erro) => { console.error(`FALHOU: ${erro.stack || erro.message}`); process.exit(1); });
