import { useColorScheme } from 'react-native';

import { cores, espaco, raio, tamanho, tipografia } from './tema';

export { cores, espaco, raio, tamanho, tipografia };

// Os dois temas tem os mesmos papeis; o valor de cada um e so uma cor.
export type Cores = { [K in keyof typeof cores.claro]: string };

// Familias carregadas no _layout raiz (Atkinson Hyperlegible Next e Mono, licenca OFL).
export const fontes = {
  regular: 'AtkinsonHyperlegibleNext_400Regular',
  medio: 'AtkinsonHyperlegibleNext_500Medium',
  semi: 'AtkinsonHyperlegibleNext_600SemiBold',
  negrito: 'AtkinsonHyperlegibleNext_700Bold',
  mono: 'AtkinsonHyperlegibleMono_400Regular',
} as const;

export function fontePorPeso(peso: string): string {
  if (peso === '700') return fontes.negrito;
  if (peso === '600') return fontes.semi;
  if (peso === '500') return fontes.medio;
  return fontes.regular;
}

export function useTema(): { c: Cores; escuro: boolean } {
  const escuro = useColorScheme() === 'dark';
  return { c: escuro ? cores.escuro : cores.claro, escuro };
}
