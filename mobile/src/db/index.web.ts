// Banco local na web. O expo-sqlite na web precisa de SharedArrayBuffer (cabeçalhos COOP/COEP), que o
// servidor do Expo não envia; aqui os mesmos dados ficam no localStorage do navegador.
// No celular, o Metro usa index.ts (SQLite). As duas versões exportam as mesmas funções.
import type { PerfilSensorial, Refugio, TipoLugar } from '@/types';

const PREFIXO = 'calmaria:';

function ler<T>(chave: string, padrao: T): T {
  try {
    const bruto = globalThis.localStorage?.getItem(PREFIXO + chave);
    return bruto ? (JSON.parse(bruto) as T) : padrao;
  } catch {
    return padrao;
  }
}
function gravar(chave: string, valor: unknown): void {
  try {
    globalThis.localStorage?.setItem(PREFIXO + chave, JSON.stringify(valor));
  } catch {
    // navegador sem armazenamento (aba anônima, por exemplo): segue sem guardar
  }
}

export function iniciarBanco(): void {}

const agora = () => new Date().toISOString();

// config
export function lerConfig(chave: string): string | null {
  return ler<Record<string, string>>('config', {})[chave] ?? null;
}
export function gravarConfig(chave: string, valor: string): void {
  gravar('config', { ...ler<Record<string, string>>('config', {}), [chave]: valor });
}
export const CONFIG = { usuarioId: 'usuario_id', boasVindasVistas: 'boas_vindas_vistas', perfilPreenchido: 'perfil_preenchido' };

// perfil
export function lerPerfil(): PerfilSensorial | null {
  return ler<PerfilSensorial | null>('perfil', null);
}
export function gravarPerfil(p: PerfilSensorial): void {
  gravar('perfil', p);
  gravarConfig(CONFIG.perfilPreenchido, '1');
}

// refugios (modo offline do botao de crise)
export function salvarRefugios(lista: Refugio[]): void {
  if (lista.length === 0) return;
  gravar(
    'refugios',
    lista.map(({ id, nome, tipo, latitude, longitude, descricao }) => ({ id, nome, tipo, latitude, longitude, descricao })),
  );
}
export function lerRefugios(): Refugio[] {
  return ler<Refugio[]>('refugios', []);
}

// favoritos
export interface Favorito {
  lugarId: number;
  nome: string;
  tipo: TipoLugar;
}
export function lerFavoritos(): Favorito[] {
  return ler<Favorito[]>('favoritos', []);
}
export function ehFavorito(lugarId: number): boolean {
  return lerFavoritos().some((f) => f.lugarId === lugarId);
}
export function alternarFavorito(f: Favorito): boolean {
  const lista = lerFavoritos();
  if (lista.some((x) => x.lugarId === f.lugarId)) {
    gravar('favoritos', lista.filter((x) => x.lugarId !== f.lugarId));
    return false;
  }
  gravar('favoritos', [{ lugarId: f.lugarId, nome: f.nome, tipo: f.tipo }, ...lista]);
  return true;
}

// crises (so no aparelho, nunca enviado)
export interface Crise {
  id: number;
  iniciadaEm: string;
  refugioNome: string | null;
  distanciaM: number | null;
  chegou: boolean;
}
export function registrarCrise(refugioNome: string | null, distancia: number | null): number {
  const lista = lerCrises();
  const id = lista.reduce((m, c) => Math.max(m, c.id), 0) + 1;
  gravar('crises', [{ id, iniciadaEm: agora(), refugioNome, distanciaM: distancia, chegou: false }, ...lista].slice(0, 50));
  return id;
}
function mudarCrise(id: number, mudanca: Partial<Crise>): void {
  gravar(
    'crises',
    lerCrises().map((c) => (c.id === id ? { ...c, ...mudanca } : c)),
  );
}
export function atualizarCrise(id: number, refugioNome: string | null, distancia: number | null): void {
  mudarCrise(id, { refugioNome, distanciaM: distancia });
}
export function marcarChegada(id: number): void {
  mudarCrise(id, { chegou: true });
}
export function lerCrises(): Crise[] {
  return ler<Crise[]>('crises', []);
}

// Direito de exclusao (LGPD): apaga tudo o que o app guardou (os refugios ficam, como no celular).
export function apagarTudo(): void {
  for (const chave of ['config', 'perfil', 'favoritos', 'crises']) {
    try {
      globalThis.localStorage?.removeItem(PREFIXO + chave);
    } catch {
      // sem armazenamento: nada a apagar
    }
  }
}
