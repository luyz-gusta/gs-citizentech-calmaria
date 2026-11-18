// Pecas basicas: texto com a fonte certa, botao com alvo de toque de 48+, estados de tela.
import type { ReactNode } from 'react';
import {
  ActivityIndicator,
  Pressable,
  Text,
  View,
  type PressableProps,
  type StyleProp,
  type TextProps,
  type TextStyle,
  type ViewStyle,
} from 'react-native';

import { espaco, fontePorPeso, fontes, raio, tamanho, tipografia, useTema } from '@/theme';

type Variante = keyof typeof tipografia;

export function Texto({
  variante = 'bodyMd',
  cor,
  mono,
  style,
  ...resto
}: TextProps & { variante?: Variante; cor?: string; mono?: boolean }) {
  const { c } = useTema();
  const t = tipografia[variante];
  return (
    <Text
      {...resto}
      style={[
        { fontSize: t.fontSize, lineHeight: t.lineHeight, color: cor ?? c.textDefault },
        { fontFamily: mono ? fontes.mono : fontePorPeso(t.fontWeight) },
        style,
      ]}
    />
  );
}

type VarianteBotao = 'primario' | 'secundario' | 'texto';

export function Botao({
  rotulo,
  variante = 'primario',
  icone,
  grande,
  selecionado,
  style,
  disabled,
  ...resto
}: Omit<PressableProps, 'style'> & {
  rotulo: string;
  variante?: VarianteBotao;
  icone?: ReactNode;
  grande?: boolean;
  selecionado?: boolean;
  style?: StyleProp<ViewStyle>;
}) {
  const { c } = useTema();
  const fundo =
    variante === 'primario' || selecionado ? c.primarySolid : variante === 'secundario' ? c.bgSurface : 'transparent';
  const corTexto = variante === 'primario' || selecionado ? c.primaryOnSolid : c.primaryText;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={rotulo}
      accessibilityState={{ disabled: !!disabled, selected: selecionado }}
      disabled={disabled}
      {...resto}
      style={({ pressed }) => [
        {
          minHeight: grande ? tamanho.controleGrande : tamanho.toqueMin,
          minWidth: tamanho.toqueMin,
          paddingHorizontal: espaco[4],
          paddingVertical: espaco[2],
          borderRadius: raio.lg,
          backgroundColor: pressed && variante === 'primario' ? c.primarySolidPressed : fundo,
          borderWidth: variante === 'secundario' ? 2 : 0,
          borderColor: selecionado ? c.primarySolid : c.primaryBorder,
          flexDirection: 'row',
          alignItems: 'center',
          justifyContent: 'center',
          gap: espaco[2],
          opacity: disabled ? 0.5 : 1,
        },
        style,
      ]}>
      {icone}
      <Texto variante="labelLg" cor={corTexto} style={{ textAlign: 'center', flexShrink: 1 }}>
        {rotulo}
      </Texto>
    </Pressable>
  );
}

export function Cartao({ children, style }: { children: ReactNode; style?: StyleProp<ViewStyle> }) {
  const { c } = useTema();
  return (
    <View
      style={[
        {
          backgroundColor: c.bgSurface,
          borderRadius: raio.xl,
          borderWidth: 1,
          borderColor: c.borderDefault,
          padding: espaco[4],
          gap: espaco[2],
        },
        style,
      ]}>
      {children}
    </View>
  );
}

export function Titulo({ children, style }: { children: ReactNode; style?: StyleProp<TextStyle> }) {
  return (
    <Texto variante="titleSm" accessibilityRole="header" style={style}>
      {children}
    </Texto>
  );
}

export function Carregando({ texto = 'Carregando...' }: { texto?: string }) {
  const { c } = useTema();
  return (
    <View style={{ padding: espaco[6], alignItems: 'center', gap: espaco[3] }} accessibilityLiveRegion="polite">
      {/* Indicador parado seria melhor; o ActivityIndicator do sistema e discreto e respeita o tema. */}
      <ActivityIndicator color={c.primarySolid} />
      <Texto cor={c.textMuted}>{texto}</Texto>
    </View>
  );
}

export function Vazio({ texto }: { texto: string }) {
  const { c } = useTema();
  return (
    <Cartao>
      <Texto cor={c.textMuted}>{texto}</Texto>
    </Cartao>
  );
}

export function Erro({ texto, aoTentar }: { texto: string; aoTentar?: () => void }) {
  const { c } = useTema();
  return (
    <Cartao style={{ borderColor: c.dangerBorder, backgroundColor: c.dangerSubtle }}>
      <Texto cor={c.dangerText} accessibilityRole="alert">
        {texto}
      </Texto>
      {aoTentar && <Botao rotulo="Tentar de novo" variante="secundario" onPress={aoTentar} />}
    </Cartao>
  );
}
