// Funções comuns do gerador de evidências e do vídeo: rodar comandos, ler a conexão do user-secrets e esconder
// segredo de qualquer texto que vá virar print.
import { spawn } from 'node:child_process';
import { mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

export const RAIZ = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
export const SAIDA = resolve(RAIZ, 'entrega', 'saida');

const segredos = new Set();

/** Registra um valor que nunca pode aparecer num print, num log ou na tela. */
export function esconder(valor) {
  if (valor && valor.length >= 4) segredos.add(valor);
}

/** Troca cada segredo registrado por "***" e tira as cores de terminal. */
export function limpar(texto) {
  let t = String(texto).replace(/\u001b\[[0-9;]*[A-Za-z]/g, '').replace(/^﻿/, '').replace(/\r\n/g, '\n');
  for (const s of segredos) t = t.split(s).join('***');
  return t;
}

export const esperar = (ms) => new Promise((ok) => setTimeout(ok, ms));

export function pasta(...partes) {
  const p = resolve(...partes);
  mkdirSync(p, { recursive: true });
  return p;
}

/** Roda um comando e devolve o código de saída e a saída (stdout e stderr juntos, na ordem em que chegaram). */
export function rodar(comando, args, { cwd = RAIZ, env = {}, mostrar = false } = {}) {
  return new Promise((ok, erro) => {
    const filho = spawn(comando, args, { cwd, env: { ...process.env, ...env }, windowsHide: true });
    let saida = '';
    const juntar = (parte) => {
      const texto = parte.toString('utf8');
      saida += texto;
      if (mostrar) process.stdout.write(limpar(texto));
    };
    filho.stdout.on('data', juntar);
    filho.stderr.on('data', juntar);
    filho.on('error', erro);
    filho.on('close', (codigo) => ok({ codigo, saida: limpar(saida) }));
  });
}

/** Roda um script do Windows PowerShell 5.1 com a saída em UTF-8 (sem isso, os acentos chegam trocados). */
export function rodarPowerShell(script, { cwd = RAIZ, env = {}, mostrar = false } = {}) {
  const comando = `[Console]::OutputEncoding = [Text.Encoding]::UTF8; & '${script.replace(/'/g, "''")}'`;
  return rodar('powershell', ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', comando], { cwd, env, mostrar });
}

/** A conexão do banco de desenvolvimento, do user-secrets da API. A senha é registrada para nunca aparecer. */
export async function conexaoDeDesenvolvimento() {
  const r = await rodar('dotnet', ['user-secrets', 'list', '--json', '--project', 'api-dotnet/src/Calmaria.Api']);
  const json = r.saida.slice(r.saida.indexOf('//BEGIN') + 7, r.saida.indexOf('//END'));
  const conexao = JSON.parse(json)['ConnectionStrings:Calmaria'];
  if (!conexao) throw new Error('Sem ConnectionStrings:Calmaria no user-secrets (api-dotnet/scripts/criar-banco-dev.ps1).');
  const partes = Object.fromEntries(conexao.split(';').filter(Boolean).map((p) => {
    const i = p.indexOf('=');
    return [p.slice(0, i).trim().toLowerCase(), p.slice(i + 1).trim()];
  }));
  esconder(partes.password);
  return { conexao, partes };
}

/** A mesma conexão, apontando para um schema só (o EF cria as tabelas no primeiro schema do search_path). */
export function comSchema(conexao, schema) {
  const sem = conexao.split(';').filter((p) => p && !/^\s*search\s*path\s*=/i.test(p)).join(';');
  return `${sem};Search Path=${schema}`;
}

export function agoraEmSaoPaulo() {
  return new Intl.DateTimeFormat('pt-BR', {
    timeZone: 'America/Sao_Paulo', day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  }).format(new Date()).replace(',', '');
}
