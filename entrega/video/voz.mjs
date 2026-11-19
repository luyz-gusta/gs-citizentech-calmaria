// A narração do vídeo, uma fala por arquivo, com cache: a mesma frase com a mesma voz não é gerada de novo.
//
// Vozes (opção --voz do gerar-video.mjs):
//   voicestudio:<perfil>  o VoiceStudio rodando nesta máquina (http://localhost:3900), com um perfil de voz clonada.
//                         O texto não sai do computador. Padrão: voicestudio:LuyzVoice.
//   windows:<nome>        a voz pt-BR do próprio Windows (Maria ou Daniel), pelo falar.ps1. Serve de reserva.
//
// O cache fica em entrega/saida/voz/, fora da pasta do vídeo (que o gerador apaga a cada rodada).
import { createHash } from 'node:crypto';
import http from 'node:http';
import https from 'node:https';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { RAIZ, SAIDA, pasta, rodar } from '../lib/util.mjs';

const VOICESTUDIO = process.env.VOICESTUDIO_URL || 'http://localhost:3900';

/** Lê "voicestudio:LuyzVoice" ou "windows:Maria". */
export function lerVoz(texto) {
  const [motor, ...resto] = String(texto).split(':');
  const nome = resto.join(':');
  if (!['voicestudio', 'windows'].includes(motor) || !nome) {
    throw new Error(`Voz inválida: "${texto}". Use voicestudio:<perfil> ou windows:<Maria|Daniel>.`);
  }
  return { motor, nome };
}

const chave = (voz, opcoes, fala) =>
  createHash('sha1').update(JSON.stringify([voz.motor, voz.nome, opcoes, fala])).digest('hex').slice(0, 16);

async function perfilDoVoiceStudio(nome) {
  let saude;
  try {
    saude = await (await fetch(`${VOICESTUDIO}/health`)).json();
  } catch {
    throw new Error(`O VoiceStudio não respondeu em ${VOICESTUDIO}. Abra o app ou rode com --voz windows:Maria.`);
  }
  const perfis = await (await fetch(`${VOICESTUDIO}/profiles`)).json();
  const perfil = perfis.find((p) => p.id === nome || p.name.toLowerCase() === nome.toLowerCase());
  if (!perfil) {
    throw new Error(`Não achei o perfil "${nome}" no VoiceStudio. Perfis: ${perfis.map((p) => p.name).join(', ')}.`);
  }
  return { perfil, motorTts: saude.engine };
}

// POST sem prazo. O fetch do Node desiste depois de 300 s sem resposta, e na CPU uma fala longa passa disso.
function postar(url, corpo) {
  const cliente = url.startsWith('https:') ? https : http;
  return new Promise((ok, falha) => {
    const pedido = cliente.request(url, {
      method: 'POST', headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(corpo) },
    }, (resposta) => {
      const partes = [];
      resposta.on('data', (p) => partes.push(p));
      resposta.on('end', () => ok({ status: resposta.statusCode, bytes: Buffer.concat(partes) }));
      resposta.on('error', falha);
    });
    pedido.on('error', falha);
    pedido.end(corpo);
  });
}

async function gerarNoVoiceStudio(perfil, fala, opcoes, destino) {
  const corpo = {
    model: 'omnivoice', voice: perfil.id, input: fala, response_format: 'wav', language: 'pt',
    seed: opcoes.semente, speed: opcoes.velocidade, num_step: opcoes.passos,
  };
  const { status, bytes } = await postar(`${VOICESTUDIO}/v1/audio/speech`, JSON.stringify(corpo));
  if (status !== 200) throw new Error(`VoiceStudio respondeu ${status}: ${bytes.toString('utf8').slice(0, 400)}`);
  if (bytes.subarray(0, 4).toString('ascii') !== 'RIFF') throw new Error('O VoiceStudio não devolveu um WAV.');
  writeFileSync(destino, bytes);
}

/**
 * Gera (ou reaproveita do cache) um WAV por fala. Devolve { [id]: caminho do WAV }.
 * frases: [{ id, fala }]
 */
export async function narrar(frases, voz, opcoes = {}) {
  const op = { semente: 7, velocidade: 1.0, passos: 32, ...opcoes };
  const cache = pasta(SAIDA, 'voz', `${voz.motor}-${voz.nome}`.replace(/[^\w.-]+/g, '_'));
  const saida = {};
  const faltam = [];
  for (const f of frases) {
    const arquivo = join(cache, `${chave(voz, op, f.fala)}.wav`);
    saida[f.id] = arquivo;
    if (!existsSync(arquivo)) faltam.push({ ...f, arquivo });
  }
  console.log(`Narração (${voz.motor}:${voz.nome}): ${frases.length - faltam.length} falas no cache, ${faltam.length} a gerar.`);
  if (!faltam.length) return saida;

  if (voz.motor === 'voicestudio') {
    const { perfil, motorTts } = await perfilDoVoiceStudio(voz.nome);
    console.log(`  perfil "${perfil.name}" (${perfil.id}), motor ${motorTts}, ${op.passos} passos, semente ${op.semente}`);
    for (const [i, f] of faltam.entries()) {
      const inicio = Date.now();
      await gerarNoVoiceStudio(perfil, f.fala, op, f.arquivo);
      console.log(`  ${i + 1}/${faltam.length} ${f.id} em ${((Date.now() - inicio) / 1000).toFixed(0)} s`);
    }
  } else {
    const lista = join(cache, 'pedido.json');
    writeFileSync(lista, JSON.stringify(faltam.map((f) => ({ id: f.arquivo.split(/[\\/]/).pop().replace(/\.wav$/, ''), fala: f.fala }))), 'utf8');
    const r = await rodar('powershell', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', resolve(RAIZ, 'entrega', 'video', 'falar.ps1'),
      '-Frases', lista, '-Pasta', cache, '-Voz', voz.nome, '-Velocidade', String(op.velocidade)]);
    if (r.codigo !== 0) throw new Error(`A narração do Windows falhou:\n${r.saida}`);
  }
  for (const f of faltam) if (!existsSync(f.arquivo)) throw new Error(`Faltou o áudio da fala ${f.id}.`);
  return saida;
}

/** Converte um WAV qualquer em PCM cru de 48 kHz, mono, 16 bits, sem o silêncio do começo e do fim. */
export async function paraPcm(wav, destino) {
  const corte = 'silenceremove=start_periods=1:start_threshold=-50dB:start_silence=0.05,' +
    'areverse,silenceremove=start_periods=1:start_threshold=-50dB:start_silence=0.08,areverse';
  const r = await rodar('ffmpeg', ['-y', '-v', 'error', '-i', wav, '-af', corte, '-f', 's16le', '-ar', '48000', '-ac', '1', destino]);
  if (r.codigo !== 0) throw new Error(`ffmpeg: ${r.saida}`);
  return readFileSync(destino);
}
