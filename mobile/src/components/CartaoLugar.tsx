import { router } from 'expo-router';
import { Pressable, View } from 'react-native';

import { NOME_NIVEL, NOME_TIPO } from '@/lib/textos';
import { espaco, raio, useTema } from '@/theme';
import type { LugarResumo } from '@/types';

import { Texto } from './base';
import { SeloNivel } from './nivel';

export function textoDistancia(m: number | null): string {
  if (m === null) return '';
  if (m < 1000) return `${m} m`;
  return `${(m / 1000).toFixed(1).replace('.', ',')} km`;
}

export function CartaoLugar({ lugar }: { lugar: LugarResumo }) {
  const { c } = useTema();
  const nivel = lugar.nivelAtual ? `${NOME_NIVEL[lugar.nivelAtual]} agora` : 'sem medição agora';
  const dist = lugar.distanciaM !== null ? `, a ${textoDistancia(lugar.distanciaM)}` : '';
  return (
    <Pressable
      onPress={() => router.push(`/lugar/${lugar.id}`)}
      accessibilityRole="button"
      accessibilityLabel={`${lugar.nome}, ${NOME_TIPO[lugar.tipo]}, ${nivel}${dist}`}
      accessibilityHint="Abre o detalhe do lugar"
      style={({ pressed }) => ({
        backgroundColor: pressed ? c.primarySubtle : c.bgSurface,
        borderRadius: raio.xl,
        borderWidth: 1,
        borderColor: c.borderDefault,
        padding: espaco[4],
        gap: espaco[2],
        minHeight: 48,
      })}>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', gap: espaco[2] }}>
        <Texto variante="labelLg" style={{ flex: 1 }}>
          {lugar.nome}
        </Texto>
        {lugar.distanciaM !== null && (
          <Texto variante="label" cor={c.textMuted} mono>
            {textoDistancia(lugar.distanciaM)}
          </Texto>
        )}
      </View>
      <Texto variante="bodySm" cor={c.textMuted}>
        {NOME_TIPO[lugar.tipo]}
      </Texto>
      {lugar.ehRefugio && !lugar.nivelAtual ? (
        <Texto variante="label" cor={c.textMuted}>
          Lugar calmo de referência (sem sensor)
        </Texto>
      ) : (
        <SeloNivel nivel={lugar.nivelAtual} />
      )}
    </Pressable>
  );
}
