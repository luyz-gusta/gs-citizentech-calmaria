// Textos literais e curtos, usados em mais de uma tela.
import type { DiaSemana, Gatilho, NivelIss, NivelPrevisao, TipoLugar } from '@/types';

export const NOME_NIVEL: Record<NivelIss, string> = {
  CALMO: 'Calmo',
  MODERADO: 'Moderado',
  INTENSO: 'Intenso',
};

export const NOME_PREVISAO: Record<NivelPrevisao, string> = {
  BAIXO: 'Baixa',
  MEDIO: 'Média',
  ALTO: 'Alta',
};

// A previsao usa as mesmas cores e formas dos niveis do ISS.
export const PREVISAO_PARA_NIVEL: Record<NivelPrevisao, NivelIss> = {
  BAIXO: 'CALMO',
  MEDIO: 'MODERADO',
  ALTO: 'INTENSO',
};

export const NOME_TIPO: Record<TipoLugar, string> = {
  MERCADO: 'Mercado',
  ESTACAO: 'Estação',
  PARQUE: 'Parque',
  REFUGIO: 'Lugar calmo',
  OUTRO: 'Outro',
};

export const NOME_GATILHO: Record<Gatilho, string> = {
  BARULHO: 'Barulho',
  LUZ_FORTE: 'Luz forte',
  MULTIDAO: 'Multidão',
  CHEIRO: 'Cheiro',
  FILA: 'Fila',
  OUTRO: 'Outro',
};

export const DIAS: DiaSemana[] = ['DOMINGO', 'SEGUNDA', 'TERCA', 'QUARTA', 'QUINTA', 'SEXTA', 'SABADO'];

export const NOME_DIA: Record<DiaSemana, string> = {
  DOMINGO: 'Domingo',
  SEGUNDA: 'Segunda',
  TERCA: 'Terça',
  QUARTA: 'Quarta',
  QUINTA: 'Quinta',
  SEXTA: 'Sexta',
  SABADO: 'Sábado',
};

export const NOME_DIA_CURTO: Record<DiaSemana, string> = {
  DOMINGO: 'Dom',
  SEGUNDA: 'Seg',
  TERCA: 'Ter',
  QUARTA: 'Qua',
  QUINTA: 'Qui',
  SEXTA: 'Sex',
  SABADO: 'Sáb',
};

export const NOTA_ROTULO: Record<number, string> = {
  1: 'Muito difícil para mim',
  2: 'Difícil',
  3: 'Mais ou menos',
  4: 'Tranquilo',
  5: 'Muito tranquilo',
};

export const SENSIBILIDADE_ROTULO: Record<number, string> = {
  1: 'Quase não me incomoda',
  2: 'Incomoda um pouco',
  3: 'Incomoda',
  4: 'Incomoda muito',
  5: 'Não consigo ficar perto',
};

export function nivelDoIndice(indice: number): NivelIss {
  if (indice >= 70) return 'INTENSO';
  if (indice >= 40) return 'MODERADO';
  return 'CALMO';
}

export function fraseSom(db: number): string {
  if (db >= 85) return 'Som muito alto, como um show ou uma obra perto.';
  if (db >= 70) return 'Som alto, como uma rua movimentada.';
  if (db >= 55) return 'Som médio, como uma conversa em grupo.';
  return 'Som baixo, como uma sala silenciosa.';
}

export function fraseLuz(lux: number): string {
  if (lux >= 2000) return 'Luz muito forte, como sol direto.';
  if (lux >= 1000) return 'Luz forte, como uma loja bem iluminada.';
  if (lux >= 400) return 'Luz média, como um escritório.';
  return 'Luz fraca, como uma sala com pouca luz.';
}

export function fraseMovimento(porMin: number): string {
  if (porMin >= 20) return 'Muita gente passando, lugar lotado.';
  if (porMin >= 10) return 'Bastante gente passando.';
  if (porMin >= 4) return 'Algumas pessoas passando.';
  return 'Pouca gente.';
}

export function diaDaData(dataIso: string): DiaSemana {
  const [a, m, d] = dataIso.slice(0, 10).split('-').map(Number);
  return DIAS[new Date(a, m - 1, d).getDay()];
}

export function dataCurta(dataIso: string): string {
  const [, m, d] = dataIso.slice(0, 10).split('-');
  return `${d}/${m}`;
}

export function nomeDistrito(distrito: string): string {
  const especiais: Record<string, string> = { CONSOLACAO: 'Consolação', 'JARDIM PAULISTA': 'Jardim Paulista' };
  if (especiais[distrito]) return especiais[distrito];
  return distrito
    .toLowerCase()
    .split(' ')
    .map((p) => p.charAt(0).toUpperCase() + p.slice(1))
    .join(' ');
}

export function hojeIso(): string {
  const h = new Date();
  const m = String(h.getMonth() + 1).padStart(2, '0');
  const d = String(h.getDate()).padStart(2, '0');
  return `${h.getFullYear()}-${m}-${d}`;
}
