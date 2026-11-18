// Tipos das respostas da API Calmaria. Iguais aos exemplos do contrato (docs/disciplinas/mobile.md, Parte 3).

export type TipoLugar = 'MERCADO' | 'ESTACAO' | 'PARQUE' | 'REFUGIO' | 'OUTRO';
export type NivelIss = 'CALMO' | 'MODERADO' | 'INTENSO';
export type NivelPrevisao = 'BAIXO' | 'MEDIO' | 'ALTO';
export type Gatilho = 'BARULHO' | 'LUZ_FORTE' | 'MULTIDAO' | 'CHEIRO' | 'FILA' | 'OUTRO';
export type DiaSemana = 'DOMINGO' | 'SEGUNDA' | 'TERCA' | 'QUARTA' | 'QUINTA' | 'SEXTA' | 'SABADO';

export interface LugarResumo {
  id: number;
  nome: string;
  tipo: TipoLugar;
  latitude: number;
  longitude: number;
  distrito: string;
  ehRefugio: boolean;
  distanciaM: number | null;
  indiceAtual: number | null;
  nivelAtual: NivelIss | null;
  ultimaLeituraEm: string | null;
}

export interface Leitura {
  somDb: number;
  luzLux: number;
  movimentosMin: number;
  indice: number;
  nivel: NivelIss;
  registradaEm: string;
}

export interface LugarDetalhe {
  id: number;
  nome: string;
  tipo: TipoLugar;
  endereco: string | null;
  latitude: number;
  longitude: number;
  distrito: string;
  ehRefugio: boolean;
  descricao: string | null;
  horarioFuncionamento: string | null;
  ultimaLeitura: Leitura | null;
}

export interface JanelaHorario {
  diaSemana: DiaSemana;
  hora: number;
  indiceMedio: number;
}

export interface HorariosCalmos {
  lugarId: number;
  suficiente: boolean;
  janelasMaisCalmas: JanelaHorario[];
  grade: JanelaHorario[];
}

export interface Refugio {
  id: number;
  nome: string;
  tipo: TipoLugar;
  latitude: number;
  longitude: number;
  descricao: string | null;
}

export interface RefugioMaisProximo {
  lugar: Refugio;
  distanciaM: number;
  minutosAPe: number;
  linkTrajeto: string;
}

export interface Previsao {
  distrito: string;
  data: string;
  reclamacoesPrevistas: number;
  nivel: NivelPrevisao;
}

export interface PerfilSensorial {
  sensibilidadeSom: number;
  sensibilidadeLuz: number;
  sensibilidadeMultidao: number;
  contatoApoioNome: string | null;
  contatoApoioTelefone: string | null;
}

export interface NovoRelato {
  usuarioId: string;
  lugarId: number;
  nota: number;
  gatilhos: Gatilho[];
  comentario: string | null;
  visitadoEm: string;
}

export interface RelatoCriado {
  id: number;
  comentario: string | null;
  removidos: string[];
}

export interface Relato {
  id: number;
  nota: number;
  gatilhos: Gatilho[];
  comentario: string | null;
  visitadoEm: string;
}

export interface PaginaRelatos {
  pagina: number;
  tamanho: number;
  total: number;
  itens: Relato[];
}

export interface ResumoRelatos {
  lugarId: number;
  totalRelatos: number;
  notaMedia: number | null;
  gatilhosMaisCitados: { gatilho: Gatilho; vezes: number }[];
}

export interface AnalisePrivacidade {
  temDadoPessoal: boolean;
  achados: { categoria: string; trecho: string }[];
  textoSanitizado: string;
}

export interface ProblemDetails {
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
