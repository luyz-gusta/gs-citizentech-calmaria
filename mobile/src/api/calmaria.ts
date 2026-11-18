// Cliente unico da API Calmaria: uma funcao por rota, agrupadas como os modulos da API.
// Se a API nao responder em 3 segundos (ou EXPO_PUBLIC_API_URL estiver vazia), usa o mock de reserva
// e avisa a faixa "Mostrando dados de exemplo".
import { useSyncExternalStore } from 'react';

import { ErroMock, mock } from '@/mocks';
import type {
  AnalisePrivacidade,
  HorariosCalmos,
  LugarDetalhe,
  LugarResumo,
  NovoRelato,
  PaginaRelatos,
  PerfilSensorial,
  Previsao,
  ProblemDetails,
  Refugio,
  RefugioMaisProximo,
  RelatoCriado,
  ResumoRelatos,
  TipoLugar,
} from '@/types';

const URL_API = (process.env.EXPO_PUBLIC_API_URL ?? '').replace(/\/$/, '');
const TEMPO_LIMITE_MS = 3000;

export const apiConfigurada = URL_API.length > 0;

export type Origem = 'api' | 'mock';
export interface Resposta<T> {
  dados: T;
  origem: Origem;
}

// Estado global simples: a ultima resposta veio do mock?
let usandoMock = !apiConfigurada;
const ouvintes = new Set<() => void>();
function marcaOrigem(origem: Origem) {
  const novo = origem === 'mock';
  if (novo !== usandoMock) {
    usandoMock = novo;
    ouvintes.forEach((f) => f());
  }
}
export function useUsandoMock(): boolean {
  return useSyncExternalStore(
    (f) => {
      ouvintes.add(f);
      return () => ouvintes.delete(f);
    },
    () => usandoMock,
  );
}

// Erro de negocio vindo da API (ProblemDetails) ou do mock. Nao cai no mock: a API respondeu.
export class ErroApi extends Error {
  constructor(public status: number, message: string, public problema?: ProblemDetails) {
    super(message);
  }
}

class SemConexao extends Error {}

async function chamar<T>(rota: string, init?: RequestInit): Promise<T> {
  if (!apiConfigurada) throw new SemConexao('API não configurada');
  const controle = new AbortController();
  const timer = setTimeout(() => controle.abort(), TEMPO_LIMITE_MS);
  let resp: Response;
  try {
    resp = await fetch(`${URL_API}${rota}`, {
      ...init,
      signal: controle.signal,
      headers: { Accept: 'application/json', ...(init?.body ? { 'Content-Type': 'application/json' } : {}), ...init?.headers },
    });
  } catch {
    throw new SemConexao('Sem resposta da API');
  } finally {
    clearTimeout(timer);
  }
  if (resp.status >= 500) throw new SemConexao(`API com erro ${resp.status}`);
  if (!resp.ok) {
    let problema: ProblemDetails | undefined;
    try {
      problema = await resp.json();
    } catch {}
    throw new ErroApi(resp.status, problema?.detail ?? problema?.title ?? `Erro ${resp.status}`, problema);
  }
  if (resp.status === 204) return undefined as T;
  return (await resp.json()) as T;
}

// Tenta a API; se nao houver conexao, responde com o mock.
async function comReserva<T>(rota: string, reserva: () => T, init?: RequestInit): Promise<Resposta<T>> {
  try {
    const dados = await chamar<T>(rota, init);
    marcaOrigem('api');
    return { dados, origem: 'api' };
  } catch (e) {
    if (!(e instanceof SemConexao)) throw e;
    marcaOrigem('mock');
    try {
      return { dados: reserva(), origem: 'mock' };
    } catch (m) {
      if (m instanceof ErroMock) throw new ErroApi(m.status, m.message);
      throw m;
    }
  }
}

function qs(params: Record<string, string | number | undefined>): string {
  const partes = Object.entries(params)
    .filter(([, v]) => v !== undefined && v !== '')
    .map(([k, v]) => `${k}=${encodeURIComponent(String(v))}`);
  return partes.length ? `?${partes.join('&')}` : '';
}

const json = (corpo: unknown): RequestInit => ({ body: JSON.stringify(corpo) });

// Lugares e leituras
export const lugares = {
  listar: (p: { lat?: number; lng?: number; raioM?: number; tipo?: TipoLugar } = {}) =>
    comReserva<LugarResumo[]>(`/api/lugares${qs(p)}`, () => mock.lugares(p.lat, p.lng, p.tipo)),
  obter: (id: number) => comReserva<LugarDetalhe>(`/api/lugares/${id}`, () => mock.lugar(id)),
  horariosCalmos: (id: number) =>
    comReserva<HorariosCalmos>(`/api/lugares/${id}/horarios-calmos`, () => mock.horariosCalmos(id)),
};

// Botao de crise
export const crise = {
  refugioMaisProximo: (lat: number, lng: number) =>
    comReserva<RefugioMaisProximo>(`/api/crise/refugio-mais-proximo${qs({ lat, lng })}`, () =>
      mock.refugioMaisProximo(lat, lng),
    ),
  // Sem reserva de mock: quem chama decide usar o SQLite quando nao houver conexao.
  refugioMaisProximoSoApi: (lat: number, lng: number) =>
    chamar<RefugioMaisProximo>(`/api/crise/refugio-mais-proximo${qs({ lat, lng })}`),
  refugios: () => comReserva<Refugio[]>('/api/refugios', () => mock.refugios()),
};

// Previsao de barulho (vem do ML)
export const previsoes = {
  barulho: (distrito: string, dias = 7) =>
    comReserva<Previsao[]>(`/api/previsoes/barulho${qs({ distrito, dias })}`, () => mock.previsoes(distrito, dias)),
};

// Usuario anonimo e perfil sensorial
export const usuarios = {
  criar: () => comReserva<{ id: string }>('/api/usuarios', () => mock.criarUsuario(), { method: 'POST', ...json({}) }),
  salvarPerfil: (id: string, perfil: PerfilSensorial) =>
    comReserva<void>(`/api/usuarios/${id}/perfil-sensorial`, () => undefined, { method: 'PUT', ...json(perfil) }),
  apagar: (id: string) => comReserva<void>(`/api/usuarios/${id}`, () => undefined, { method: 'DELETE' }),
};

// Relatos
export const relatos = {
  criar: (r: NovoRelato) => comReserva<RelatoCriado>('/api/relatos', () => mock.criarRelato(r), { method: 'POST', ...json(r) }),
  listar: (lugarId: number, pagina = 1, tamanho = 10) =>
    comReserva<PaginaRelatos>(`/api/relatos${qs({ lugarId, pagina, tamanho })}`, () => mock.relatos(lugarId)),
  resumo: (lugarId: number) =>
    comReserva<ResumoRelatos>(`/api/lugares/${lugarId}/resumo-relatos`, () => mock.resumoRelatos(lugarId)),
};

// Privacidade
export const privacidade = {
  analisar: (texto: string) =>
    comReserva<AnalisePrivacidade>('/api/privacidade/analisar', () => mock.analisar(texto), {
      method: 'POST',
      ...json({ texto }),
    }),
};
