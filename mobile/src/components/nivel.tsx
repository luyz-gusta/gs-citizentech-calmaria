// Nivel sempre em tres sinais juntos: cor, letra e forma (C no circulo, M no quadrado, I no losango),
// mais o texto escrito. Cor nunca sozinha.
import { View } from 'react-native';

import { NOME_NIVEL, NOME_PREVISAO, PREVISAO_PARA_NIVEL } from '@/lib/textos';
import { espaco, raio, useTema, type Cores } from '@/theme';
import type { NivelIss, NivelPrevisao } from '@/types';

import { Texto } from './base';

const LETRA: Record<NivelIss, string> = { CALMO: 'C', MODERADO: 'M', INTENSO: 'I' };

export function coresDoNivel(c: Cores, nivel: NivelIss) {
  if (nivel === 'CALMO')
    return { solido: c.nivelCalmoSolid, sobre: c.nivelCalmoOnSolid, fundo: c.nivelCalmoSubtle, texto: c.nivelCalmoText, borda: c.nivelCalmoBorder };
  if (nivel === 'MODERADO')
    return { solido: c.nivelModeradoSolid, sobre: c.nivelModeradoOnSolid, fundo: c.nivelModeradoSubtle, texto: c.nivelModeradoText, borda: c.nivelModeradoBorder };
  return { solido: c.nivelIntensoSolid, sobre: c.nivelIntensoOnSolid, fundo: c.nivelIntensoSubtle, texto: c.nivelIntensoText, borda: c.nivelIntensoBorder };
}

// Forma com a letra. `tamanho` e o lado visivel.
export function FormaNivel({ nivel, tamanho = 16, contorno }: { nivel: NivelIss; tamanho?: number; contorno?: string }) {
  const { c } = useTema();
  const k = coresDoNivel(c, nivel);
  const lado = nivel === 'INTENSO' ? Math.round(tamanho / Math.SQRT2) : tamanho;
  const fonte = Math.max(10, Math.round(tamanho * 0.55));
  return (
    <View
      style={{ width: tamanho, height: tamanho, alignItems: 'center', justifyContent: 'center' }}
      importantForAccessibility="no-hide-descendants"
      accessibilityElementsHidden>
      <View
        style={{
          position: 'absolute',
          width: lado,
          height: lado,
          backgroundColor: k.solido,
          borderRadius: nivel === 'CALMO' ? tamanho / 2 : nivel === 'MODERADO' ? 2 : 1,
          borderWidth: contorno ? 2 : 0,
          borderColor: contorno,
          transform: nivel === 'INTENSO' ? [{ rotate: '45deg' }] : undefined,
        }}
      />
      <Texto variante="labelSm" cor={k.sobre} style={{ fontSize: fonte, lineHeight: fonte + 2 }} allowFontScaling={false}>
        {LETRA[nivel]}
      </Texto>
    </View>
  );
}

// Selo em pilula: forma de 16 e texto de 16 ("Calmo agora").
export function SeloNivel({ nivel, sufixo = 'agora' }: { nivel: NivelIss | null; sufixo?: string }) {
  const { c } = useTema();
  if (!nivel) {
    return (
      <View
        style={{ minHeight: 32, paddingHorizontal: espaco[3], borderRadius: raio.full, backgroundColor: c.bgSubtle, justifyContent: 'center', alignSelf: 'flex-start' }}
        accessible
        accessibilityLabel="Sem medição agora">
        <Texto variante="label" cor={c.textMuted}>
          Sem medição agora
        </Texto>
      </View>
    );
  }
  const k = coresDoNivel(c, nivel);
  const texto = `${NOME_NIVEL[nivel]}${sufixo ? ` ${sufixo}` : ''}`;
  return (
    <View
      accessible
      accessibilityLabel={texto}
      style={{
        minHeight: 32,
        paddingHorizontal: espaco[3],
        borderRadius: raio.full,
        backgroundColor: k.fundo,
        borderWidth: 1,
        borderColor: k.borda,
        flexDirection: 'row',
        alignItems: 'center',
        gap: espaco[2],
        alignSelf: 'flex-start',
      }}>
      <FormaNivel nivel={nivel} tamanho={18} />
      <Texto variante="label" cor={k.texto}>
        {texto}
      </Texto>
    </View>
  );
}

export function SeloPrevisao({ nivel }: { nivel: NivelPrevisao }) {
  const { c } = useTema();
  const iss = PREVISAO_PARA_NIVEL[nivel];
  const k = coresDoNivel(c, iss);
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco[2] }}>
      <FormaNivel nivel={iss} tamanho={18} />
      <Texto variante="label" cor={k.texto}>
        {NOME_PREVISAO[nivel]}
      </Texto>
    </View>
  );
}
