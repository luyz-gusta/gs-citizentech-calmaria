// Mock de reserva: responde no mesmo formato da API (contrato da Parte 3).
// Leituras e relatos sao simulados; as previsoes vem do CSV real gerado pelo notebook de ML (ml/saida/).
import { distanciaM, linkTrajeto, minutosAPe } from '@/lib/geo';
import { DIAS, hojeIso, nivelDoIndice } from '@/lib/textos';
import type {
  AnalisePrivacidade,
  HorariosCalmos,
  JanelaHorario,
  LugarDetalhe,
  LugarResumo,
  NovoRelato,
  PaginaRelatos,
  Previsao,
  Refugio,
  RefugioMaisProximo,
  RelatoCriado,
  ResumoRelatos,
  TipoLugar,
} from '@/types';

import lugaresJson from './lugares.json';
import previsoesJson from './previsoes.json';
import relatosJson from './relatos.json';

const lugares = lugaresJson as LugarDetalhe[];
const previsoes = previsoesJson as Previsao[];
const resumos = relatosJson.resumos as Record<string, ResumoRelatos>;
const relatos = relatosJson.itens as unknown as Record<string, PaginaRelatos['itens']>;

export class ErroMock extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

function paraResumo(l: LugarDetalhe, lat?: number, lng?: number): LugarResumo {
  const temPosicao = lat !== undefined && lng !== undefined;
  return {
    id: l.id,
    nome: l.nome,
    tipo: l.tipo,
    latitude: l.latitude,
    longitude: l.longitude,
    distrito: l.distrito,
    ehRefugio: l.ehRefugio,
    distanciaM: temPosicao ? distanciaM({ latitude: lat, longitude: lng }, l) : null,
    indiceAtual: l.ultimaLeitura?.indice ?? null,
    nivelAtual: l.ultimaLeitura?.nivel ?? null,
    ultimaLeituraEm: l.ultimaLeitura?.registradaEm ?? null,
  };
}

function paraRefugio(l: LugarDetalhe): Refugio {
  return { id: l.id, nome: l.nome, tipo: l.tipo, latitude: l.latitude, longitude: l.longitude, descricao: l.descricao };
}

export const mock = {
  lugares(lat?: number, lng?: number, tipo?: TipoLugar): LugarResumo[] {
    const lista = lugares.filter((l) => !tipo || l.tipo === tipo).map((l) => paraResumo(l, lat, lng));
    return lista.sort((a, b) => (a.distanciaM ?? 0) - (b.distanciaM ?? 0) || a.id - b.id);
  },

  lugar(id: number): LugarDetalhe {
    const l = lugares.find((x) => x.id === id);
    if (!l) throw new ErroMock(404, 'Lugar não encontrado.');
    return l;
  },

  // Grade simulada: mais cheio no pico da manha, no fim da tarde e no fim de semana.
  horariosCalmos(id: number): HorariosCalmos {
    const l = mock.lugar(id);
    if (!l.ultimaLeitura) return { lugarId: id, suficiente: false, janelasMaisCalmas: [], grade: [] };
    const base = l.ultimaLeitura.indice;
    const grade: JanelaHorario[] = [];
    DIAS.forEach((dia, d) => {
      for (let hora = 7; hora <= 21; hora++) {
        const pico = hora === 8 || hora === 18 ? 22 : hora === 7 || hora === 9 || hora === 17 || hora === 19 ? 12 : 0;
        const fimDeSemana = d === 0 || d === 6 ? (l.tipo === 'MERCADO' ? 14 : -10) : 0;
        const variacao = ((id * 7 + d * 3 + hora) % 5) * 2;
        const indice = Math.max(5, Math.min(95, Math.round(base - 18 + pico + fimDeSemana + variacao)));
        grade.push({ diaSemana: dia, hora, indiceMedio: indice });
      }
    });
    const janelasMaisCalmas = [...grade].sort((a, b) => a.indiceMedio - b.indiceMedio).slice(0, 3);
    return { lugarId: id, suficiente: true, janelasMaisCalmas, grade };
  },

  refugios(): Refugio[] {
    return lugares.filter((l) => l.ehRefugio).map(paraRefugio);
  },

  refugioMaisProximo(lat: number, lng: number): RefugioMaisProximo {
    const candidatos = lugares.filter(
      (l) => l.ehRefugio || (l.ultimaLeitura && nivelDoIndice(l.ultimaLeitura.indice) === 'CALMO'),
    );
    const aqui = { latitude: lat, longitude: lng };
    const perto = candidatos
      .map((l) => ({ l, d: distanciaM(aqui, l) }))
      .sort((a, b) => a.d - b.d)[0];
    return {
      lugar: paraRefugio(perto.l),
      distanciaM: perto.d,
      minutosAPe: minutosAPe(perto.d),
      linkTrajeto: linkTrajeto(perto.l),
    };
  },

  previsoes(distrito: string, dias: number): Previsao[] {
    const doDistrito = previsoes.filter((p) => p.distrito === distrito);
    if (doDistrito.length === 0) throw new ErroMock(404, 'Distrito sem previsão.');
    const hoje = hojeIso();
    const futuras = doDistrito.filter((p) => p.data >= hoje);
    return (futuras.length > 0 ? futuras : doDistrito).slice(0, dias);
  },

  criarUsuario(): { id: string } {
    const hex = () => Math.floor(Math.random() * 0x10000).toString(16).padStart(4, '0');
    return { id: `${hex()}${hex()}-${hex()}-4${hex().slice(1)}-a${hex().slice(1)}-${hex()}${hex()}${hex()}` };
  },

  analisar(texto: string): AnalisePrivacidade {
    // Versao simplificada do filtro da API, so para o modo de exemplo.
    const regras: { categoria: string; re: RegExp; mascara: (s: string) => string }[] = [
      { categoria: 'CPF', re: /\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b/g, mascara: (s) => `${s.slice(0, 3)}.***.***-${s.slice(-2)}` },
      { categoria: 'EMAIL', re: /\b[\w.+-]+@[\w-]+\.[\w.]+\b/g, mascara: (s) => `${s[0]}***@${s.split('@')[1]}` },
      { categoria: 'TELEFONE', re: /\(?\b\d{2}\)?\s?9?\d{4}-?\d{4}\b/g, mascara: (s) => `****${s.slice(-4)}` },
    ];
    const achados: AnalisePrivacidade['achados'] = [];
    let textoSanitizado = texto;
    for (const r of regras) {
      for (const m of texto.match(r.re) ?? []) {
        achados.push({ categoria: r.categoria, trecho: r.mascara(m) });
        textoSanitizado = textoSanitizado.replace(m, '[removido]');
      }
    }
    return { temDadoPessoal: achados.length > 0, achados, textoSanitizado };
  },

  criarRelato(r: NovoRelato): RelatoCriado {
    const a = mock.analisar(r.comentario ?? '');
    return {
      id: Math.floor(Math.random() * 1000) + 100,
      comentario: r.comentario ? a.textoSanitizado : null,
      removidos: a.achados.map((x) => x.categoria),
    };
  },

  relatos(lugarId: number): PaginaRelatos {
    const itens = relatos[String(lugarId)] ?? [];
    return { pagina: 1, tamanho: 10, total: resumos[String(lugarId)]?.totalRelatos ?? itens.length, itens };
  },

  resumoRelatos(lugarId: number): ResumoRelatos {
    return resumos[String(lugarId)] ?? { lugarId, totalRelatos: 0, notaMedia: null, gatilhosMaisCitados: [] };
  },
};
