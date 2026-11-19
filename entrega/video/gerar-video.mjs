// Gera o vídeo de Cybersecurity (roteiro de docs/disciplinas/cybersecurity.md, seis blocos) a partir das capturas
// reais de entrega/gerar-evidencias.mjs. As cenas são composições do HyperFrames (HTML com movimento, renderizado
// quadro a quadro no Chrome), cada parte entrando quando a voz a nomeia; a narração é gerada nesta máquina; a
// legenda .srt sai do tempo de cada fala; o MP4 segue o kit de vídeo (VI-01 e VI-07).
//
// Uso (da raiz, depois do gerar-evidencias):
//   node entrega/video/gerar-video.mjs [--voz voicestudio:LuyzVoice | --voz windows:Maria] [--velocidade 1.4]
//                                      [--passos 32] [--qualidade delivery|draft] [--sem-render]
// Sai em entrega/saida/video/: calmaria-cybersecurity.mp4, calmaria-cybersecurity.srt, folha-de-contato.png e o
// projeto do HyperFrames em projeto/ (para abrir no preview: npx hyperframes@0.9.65 preview, dentro dele).
import { copyFileSync, existsSync, rmSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { BLOCOS, dados, falar, roteiro } from './roteiro.mjs';
import { montarCena } from './cenas.mjs';
import { montarSrt } from './legenda.mjs';
import { lerVoz, narrar, paraPcm } from './voz.mjs';
import { RAIZ, SAIDA, pasta, rodar } from '../lib/util.mjs';

const args = process.argv.slice(2);
const opcao = (nome, padrao) => { const i = args.indexOf(`--${nome}`); return i >= 0 ? args[i + 1] : padrao; };
const VOZ = lerVoz(opcao('voz', 'voicestudio:LuyzVoice'));
// A voz clonada sai lenta (uns 12 caracteres por segundo): em 1,4 fica num ritmo de fala normal e o vídeo cabe em
// menos de 5 minutos. A voz do Windows já é rápida e fica em 1,0.
const VELOCIDADE = Number(opcao('velocidade', VOZ.motor === 'voicestudio' ? '1.4' : '1.0'));
const PASSOS = Number(opcao('passos', '32'));
const QUALIDADE = opcao('qualidade', 'delivery');
const RENDERIZAR = !args.includes('--sem-render');
const HYPERFRAMES = 'hyperframes@0.9.65';

const AMOSTRAS = 48000;           // por segundo, mono, 16 bits
const POR_QUADRO = AMOSTRAS / 30; // cada cena dura um número inteiro de quadros
const PAUSA_NO_COMECO = 0.45;
const PAUSA_ENTRE_FALAS = 0.35;
const PAUSA_NO_FIM = 0.95;

const VIDEO = resolve(SAIDA, 'video');
const tempo = (s) => `${Math.floor(s / 60)}:${String(Math.floor(s % 60)).padStart(2, '0')}`;

function hyperframes(comando, cwd, mostrar = false) {
  return rodar('cmd', ['/c', 'npx', '--yes', HYPERFRAMES, ...comando], {
    // CI=1: sem o processo de check "quente", que segura a pasta do projeto por 5 minutos depois de terminar.
    cwd, mostrar, env: { HYPERFRAMES_NO_TELEMETRY: '1', DO_NOT_TRACK: '1', CI: '1' },
  });
}

/** O instante (segundos da cena) em que a voz diz um termo da fala i: proporção do texto dentro do áudio. */
function tempos(cena, ini, dur) {
  return {
    ini, dur,
    a(i, termo, ocorrencia = 1) {
      const texto = cena.falas[i].texto;
      let idx = -1;
      for (let k = 0; k < ocorrencia; k++) idx = texto.indexOf(termo, idx + 1);
      if (idx < 0) throw new Error(`Cena ${cena.id}, fala ${i}: não achei "${termo}" em "${texto}"`);
      const fracao = falar(texto.slice(0, idx)).length / falar(texto).length;
      return Math.max(ini[i], ini[i] + dur[i] * fracao - 0.12);
    },
  };
}

async function main() {
  const d = dados();
  const cenas = roteiro(d);

  rmSync(VIDEO, { recursive: true, force: true });
  pasta(VIDEO);
  const TMP = pasta(VIDEO, 'tmp');
  const PROJETO = pasta(VIDEO, 'projeto');

  // 1. A narração: um áudio por fala (do cache, se já existe), sem o silêncio das pontas.
  const frases = [];
  cenas.forEach((c) => (c.falas || []).forEach((f, i) => frases.push({ id: `${c.id}-${i}`, fala: f.fala })));
  const wavs = await narrar(frases, VOZ, { velocidade: VELOCIDADE, passos: PASSOS });
  const pcm = {};
  for (const f of frases) pcm[f.id] = await paraPcm(wavs[f.id], join(TMP, `${f.id}.pcm`));

  // 2. A linha do tempo: cada cena com o áudio dela, arredondada para quadros inteiros.
  const silencio = (s) => Buffer.alloc(Math.round(s * AMOSTRAS) * 2);
  const partes = [];
  const linha = [];
  const falasNoVideo = [];
  let inicio = 0;
  for (const c of cenas) {
    const pedacos = [];
    const ini = [];
    const dur = [];
    if (c.falas?.length) {
      let t = PAUSA_NO_COMECO;
      pedacos.push(silencio(t));
      c.falas.forEach((f, i) => {
        if (i > 0) { const p = PAUSA_ENTRE_FALAS + (c.falas[i - 1].pausa || 0); pedacos.push(silencio(p)); t += p; }
        const audio = pcm[`${c.id}-${i}`];
        const s = audio.length / 2 / AMOSTRAS;
        ini.push(t); dur.push(s);
        falasNoVideo.push({ texto: f.texto, de: inicio + t, ate: inicio + t + s });
        pedacos.push(audio);
        t += s;
      });
      pedacos.push(silencio(PAUSA_NO_FIM + (c.fim || 0)));
    } else {
      pedacos.push(silencio(c.duracao));
    }
    let bloco = Buffer.concat(pedacos);
    const quadros = Math.ceil(bloco.length / 2 / POR_QUADRO);
    bloco = Buffer.concat([bloco, Buffer.alloc(quadros * POR_QUADRO * 2 - bloco.length)]);
    const duracao = quadros / 30;
    partes.push(bloco);
    linha.push({ c, inicio, duracao, t: tempos(c, ini, dur) });
    inicio += duracao;
  }
  const TOTAL = inicio;
  writeFileSync(join(TMP, 'narracao.pcm'), Buffer.concat(partes));
  console.log(`Duração: ${tempo(TOTAL)} (${TOTAL.toFixed(2)} s), ${frases.length} falas, ${cenas.length} cenas.`);

  // 3. O áudio: passa-alta, compressão leve e loudnorm em duas passadas, para -16 LUFS (VI-01).
  const cadeia = 'highpass=f=70,acompressor=threshold=-22dB:ratio=2.5:attack=8:release=120:makeup=1';
  const crua = ['-f', 's16le', '-ar', String(AMOSTRAS), '-ac', '1', '-i', join(TMP, 'narracao.pcm')];
  const medida = await rodar('ffmpeg', ['-hide_banner', ...crua, '-af', `${cadeia},loudnorm=I=-16:TP=-1.5:LRA=11:print_format=json`, '-f', 'null', '-']);
  const m = JSON.parse(medida.saida.slice(medida.saida.lastIndexOf('{'), medida.saida.lastIndexOf('}') + 1));
  const narracao = join(PROJETO, 'assets', 'narracao.wav');
  pasta(PROJETO, 'assets');
  const norm = `loudnorm=I=-16:TP=-1.5:LRA=11:measured_I=${m.input_i}:measured_TP=${m.input_tp}:measured_LRA=${m.input_lra}:measured_thresh=${m.input_thresh}:offset=${m.target_offset}:linear=true`;
  const a = await rodar('ffmpeg', ['-y', '-v', 'error', ...crua, '-af', `${cadeia},${norm}`, '-ar', '48000', '-ac', '2', '-c:a', 'pcm_s16le', narracao]);
  if (a.codigo !== 0) throw new Error(`ffmpeg (áudio): ${a.saida}`);

  // 4. A legenda.
  writeFileSync(join(VIDEO, 'calmaria-cybersecurity.srt'), montarSrt(falasNoVideo), 'utf8');

  // 5. O projeto do HyperFrames: fontes e imagem do design e das capturas, uma composição por cena.
  const fontes = pasta(PROJETO, 'assets', 'fonts');
  for (const f of ['next-400', 'next-500', 'next-600', 'next-700', 'mono-400', 'mono-600']) {
    copyFileSync(resolve(RAIZ, 'design', 'pecas', 'fontes', `${f}.ttf`), join(fontes, `${f}.ttf`));
  }
  copyFileSync(join(SAIDA, 'capturas', 'painel-inteiro.png'), join(pasta(PROJETO, 'assets', 'img'), 'painel-inteiro.png'));
  const COMPOSICOES = pasta(PROJETO, 'compositions');
  const tempoDoBloco = (n) => {
    const doBloco = linha.filter((l) => l.c.bloco === n || (l.c.tipo === 'cartela' && l.c.n === n));
    return `${tempo(doBloco[0].inicio)} a ${tempo(doBloco.at(-1).inicio + doBloco.at(-1).duracao)}`;
  };
  for (const l of linha) {
    const extra = l.c.tipo === 'cartela' ? { tempo: tempoDoBloco(l.c.n) } : {};
    writeFileSync(join(COMPOSICOES, `${l.c.id}.html`), montarCena(l.c, { ...l.t, total: l.duracao }, d, extra), 'utf8');
  }
  const hosts = linha.map((l) => `      <div id="${l.c.id}" data-composition-id="${l.c.id}" data-composition-src="compositions/${l.c.id}.html"
        data-start="${l.inicio.toFixed(3)}" data-duration="${l.duracao.toFixed(3)}" data-track-index="0" data-track-kind="graphics"
        data-width="1920" data-height="1080"></div>`).join('\n');
  writeFileSync(join(PROJETO, 'index.html'), `<!doctype html>
<html lang="pt-BR">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=1920, height=1080" />
    <title>Calmaria: o dado de saúde não pode vazar</title>
    <script src="https://cdn.jsdelivr.net/npm/gsap@3.15.0/dist/gsap.min.js"></script>
    <style>
      html, body { margin: 0; width: 1920px; height: 1080px; overflow: hidden; background: #1F2733; }
    </style>
  </head>
  <body>
    <div id="root" data-composition-id="main" data-start="0" data-duration="${TOTAL.toFixed(3)}" data-width="1920"
      data-height="1080" data-fps="30">
${hosts}
      <audio id="narracao" class="clip" src="assets/narracao.wav" data-start="0" data-duration="${TOTAL.toFixed(3)}"
        data-track-index="1" data-volume="1"></audio>
    </div>
    <script>
      const tl = gsap.timeline({ paused: true });
      window.__timelines["main"] = tl;
    </script>
  </body>
</html>
`, 'utf8');
  writeFileSync(join(PROJETO, 'package.json'), JSON.stringify({
    name: 'calmaria-cybersecurity', private: true, type: 'module',
    scripts: { dev: `npx --yes ${HYPERFRAMES} preview`, check: `npx --yes ${HYPERFRAMES} check`, render: `npx --yes ${HYPERFRAMES} render` },
  }, null, 2));
  writeFileSync(join(VIDEO, 'linha-do-tempo.json'), JSON.stringify(linha.map((l) => ({
    cena: l.c.id, bloco: l.c.bloco ?? null, inicio: Number(l.inicio.toFixed(3)), duracao: Number(l.duracao.toFixed(3)),
  })), null, 2));
  for (const n of [1, 2, 3, 4, 5, 6]) console.log(`  bloco ${n} (${BLOCOS[n]}): ${tempoDoBloco(n)}`);

  // 6. A conferência do HyperFrames (contrato, leiaute, fontes).
  console.log('Conferindo o projeto (hyperframes check)...');
  const chk = await hyperframes(['check', '--json'], PROJETO);
  writeFileSync(join(VIDEO, 'check.json'), chk.saida);
  if (chk.codigo !== 0) console.log(`  o check terminou com código ${chk.codigo}: veja entrega/saida/video/check.json`);
  if (!RENDERIZAR) { console.log('Sem render (--sem-render). Projeto em entrega/saida/video/projeto.'); return; }

  // 7. O render (quadro a quadro) e a mixagem final com a narração já nivelada.
  console.log(`Renderizando (${QUALIDADE})...`);
  const mudo = join(TMP, 'render.mp4');
  const r = await hyperframes(['render', '--quality', QUALIDADE, '--output', mudo], PROJETO, true);
  if (r.codigo !== 0 || !existsSync(mudo)) throw new Error(`O render falhou:\n${r.saida.slice(-3000)}`);
  const mp4 = join(VIDEO, 'calmaria-cybersecurity.mp4');
  const f = await rodar('ffmpeg', ['-y', '-v', 'error', '-i', mudo, '-i', narracao, '-map', '0:v:0', '-map', '1:a:0',
    '-vf', 'fps=30,scale=1920:1080:flags=lanczos,format=yuv420p', '-c:v', 'libx264', '-preset', 'slow', '-crf', '18',
    '-profile:v', 'high', '-c:a', 'aac', '-b:a', '192k', '-ar', '48000', '-ac', '2', '-movflags', '+faststart',
    '-t', TOTAL.toFixed(3), mp4]);
  if (f.codigo !== 0) throw new Error(`ffmpeg (mixagem): ${f.saida}`);

  // 8. A folha de contato: um quadro do meio de cada cena.
  const quadros = pasta(TMP, 'quadros');
  for (const [i, l] of linha.entries()) {
    const t = l.c.falas ? l.inicio + l.duracao - 0.4 : l.inicio + l.duracao * 0.6;
    await rodar('ffmpeg', ['-y', '-v', 'error', '-ss', t.toFixed(3), '-i', mp4, '-frames:v', '1', '-vf', 'scale=480:270', join(quadros, `q${String(i).padStart(2, '0')}.png`)]);
  }
  const linhas = Math.ceil(linha.length / 4);
  await rodar('ffmpeg', ['-y', '-v', 'error', '-i', join(quadros, 'q%02d.png'), '-vf', `tile=4x${linhas}:padding=4:color=0x1F2733`,
    '-frames:v', '1', join(VIDEO, 'folha-de-contato.png')]);
  console.log(`Pronto: entrega/saida/video/calmaria-cybersecurity.mp4 e .srt (${tempo(TOTAL)}).`);
}

main().catch((erro) => { console.error(`FALHOU: ${erro.stack || erro.message}`); process.exit(1); });
