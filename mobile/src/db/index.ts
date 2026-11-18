// Banco local (expo-sqlite). Guarda o id anonimo, o perfil, os refugios para o modo offline,
// os favoritos e o historico de crises (que nunca sai do celular).
import * as SQLite from 'expo-sqlite';

import type { PerfilSensorial, Refugio, TipoLugar } from '@/types';

let banco: SQLite.SQLiteDatabase | null = null;

function db(): SQLite.SQLiteDatabase {
  if (!banco) {
    banco = SQLite.openDatabaseSync('calmaria.db');
    banco.execSync(`
      PRAGMA journal_mode = WAL;
      CREATE TABLE IF NOT EXISTS config (
        chave TEXT PRIMARY KEY NOT NULL,
        valor TEXT
      );
      CREATE TABLE IF NOT EXISTS perfil (
        id INTEGER PRIMARY KEY CHECK (id = 1),
        sensibilidade_som INTEGER NOT NULL,
        sensibilidade_luz INTEGER NOT NULL,
        sensibilidade_multidao INTEGER NOT NULL,
        contato_apoio_nome TEXT,
        contato_apoio_telefone TEXT,
        atualizado_em TEXT NOT NULL
      );
      CREATE TABLE IF NOT EXISTS refugios_cache (
        id INTEGER PRIMARY KEY NOT NULL,
        nome TEXT NOT NULL,
        tipo TEXT NOT NULL,
        latitude REAL NOT NULL,
        longitude REAL NOT NULL,
        descricao TEXT,
        salvo_em TEXT NOT NULL
      );
      CREATE TABLE IF NOT EXISTS favoritos (
        lugar_id INTEGER PRIMARY KEY NOT NULL,
        nome TEXT NOT NULL,
        tipo TEXT NOT NULL,
        criado_em TEXT NOT NULL
      );
      CREATE TABLE IF NOT EXISTS crises (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        iniciada_em TEXT NOT NULL,
        refugio_nome TEXT,
        distancia_m INTEGER,
        chegou INTEGER NOT NULL DEFAULT 0
      );
    `);
  }
  return banco;
}

export function iniciarBanco(): void {
  db();
}

const agora = () => new Date().toISOString();

// config
export function lerConfig(chave: string): string | null {
  return db().getFirstSync<{ valor: string | null }>('SELECT valor FROM config WHERE chave = ?', chave)?.valor ?? null;
}
export function gravarConfig(chave: string, valor: string): void {
  db().runSync('INSERT OR REPLACE INTO config (chave, valor) VALUES (?, ?)', chave, valor);
}
export const CONFIG = { usuarioId: 'usuario_id', boasVindasVistas: 'boas_vindas_vistas', perfilPreenchido: 'perfil_preenchido' };

// perfil
export function lerPerfil(): PerfilSensorial | null {
  const p = db().getFirstSync<{
    sensibilidade_som: number;
    sensibilidade_luz: number;
    sensibilidade_multidao: number;
    contato_apoio_nome: string | null;
    contato_apoio_telefone: string | null;
  }>('SELECT * FROM perfil WHERE id = 1');
  if (!p) return null;
  return {
    sensibilidadeSom: p.sensibilidade_som,
    sensibilidadeLuz: p.sensibilidade_luz,
    sensibilidadeMultidao: p.sensibilidade_multidao,
    contatoApoioNome: p.contato_apoio_nome,
    contatoApoioTelefone: p.contato_apoio_telefone,
  };
}
export function gravarPerfil(p: PerfilSensorial): void {
  db().runSync(
    `INSERT OR REPLACE INTO perfil (id, sensibilidade_som, sensibilidade_luz, sensibilidade_multidao,
       contato_apoio_nome, contato_apoio_telefone, atualizado_em) VALUES (1, ?, ?, ?, ?, ?, ?)`,
    p.sensibilidadeSom,
    p.sensibilidadeLuz,
    p.sensibilidadeMultidao,
    p.contatoApoioNome,
    p.contatoApoioTelefone,
    agora(),
  );
  gravarConfig(CONFIG.perfilPreenchido, '1');
}

// refugios (modo offline do botao de crise)
export function salvarRefugios(lista: Refugio[]): void {
  if (lista.length === 0) return;
  const d = db();
  d.withTransactionSync(() => {
    d.runSync('DELETE FROM refugios_cache');
    for (const r of lista) {
      d.runSync(
        'INSERT INTO refugios_cache (id, nome, tipo, latitude, longitude, descricao, salvo_em) VALUES (?, ?, ?, ?, ?, ?, ?)',
        r.id,
        r.nome,
        r.tipo,
        r.latitude,
        r.longitude,
        r.descricao,
        agora(),
      );
    }
  });
}
export function lerRefugios(): Refugio[] {
  return db().getAllSync<Refugio>('SELECT id, nome, tipo, latitude, longitude, descricao FROM refugios_cache');
}

// favoritos
export interface Favorito {
  lugarId: number;
  nome: string;
  tipo: TipoLugar;
}
export function lerFavoritos(): Favorito[] {
  return db().getAllSync<Favorito>(
    'SELECT lugar_id AS lugarId, nome, tipo FROM favoritos ORDER BY criado_em DESC',
  );
}
export function ehFavorito(lugarId: number): boolean {
  return !!db().getFirstSync('SELECT 1 FROM favoritos WHERE lugar_id = ?', lugarId);
}
export function alternarFavorito(f: Favorito): boolean {
  if (ehFavorito(f.lugarId)) {
    db().runSync('DELETE FROM favoritos WHERE lugar_id = ?', f.lugarId);
    return false;
  }
  db().runSync('INSERT INTO favoritos (lugar_id, nome, tipo, criado_em) VALUES (?, ?, ?, ?)', f.lugarId, f.nome, f.tipo, agora());
  return true;
}

// crises (so no celular, nunca enviado)
export interface Crise {
  id: number;
  iniciadaEm: string;
  refugioNome: string | null;
  distanciaM: number | null;
  chegou: boolean;
}
export function registrarCrise(refugioNome: string | null, distancia: number | null): number {
  const r = db().runSync(
    'INSERT INTO crises (iniciada_em, refugio_nome, distancia_m) VALUES (?, ?, ?)',
    agora(),
    refugioNome,
    distancia,
  );
  return Number(r.lastInsertRowId);
}
export function atualizarCrise(id: number, refugioNome: string | null, distancia: number | null): void {
  db().runSync('UPDATE crises SET refugio_nome = ?, distancia_m = ? WHERE id = ?', refugioNome, distancia, id);
}
export function marcarChegada(id: number): void {
  db().runSync('UPDATE crises SET chegou = 1 WHERE id = ?', id);
}
export function lerCrises(): Crise[] {
  return db()
    .getAllSync<{ id: number; iniciada_em: string; refugio_nome: string | null; distancia_m: number | null; chegou: number }>(
      'SELECT * FROM crises ORDER BY iniciada_em DESC LIMIT 50',
    )
    .map((c) => ({
      id: c.id,
      iniciadaEm: c.iniciada_em,
      refugioNome: c.refugio_nome,
      distanciaM: c.distancia_m,
      chegou: c.chegou === 1,
    }));
}

// Direito de exclusao (LGPD): apaga tudo o que o app guardou.
export function apagarTudo(): void {
  db().execSync('DELETE FROM config; DELETE FROM perfil; DELETE FROM favoritos; DELETE FROM crises;');
}
