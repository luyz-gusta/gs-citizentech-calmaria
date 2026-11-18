import type { ReactNode } from 'react';
import { RefreshControl, ScrollView, View } from 'react-native';

import { useUsandoMock } from '@/api/calmaria';
import { espaco, raio, useTema } from '@/theme';

import { Texto } from './base';
import { ESPACO_BOTAO_CRISE } from './BotaoCrise';

export function FaixaMock() {
  const { c } = useTema();
  const usandoMock = useUsandoMock();
  if (!usandoMock) return null;
  return (
    <View
      accessible
      accessibilityLabel="Mostrando dados de exemplo"
      style={{ backgroundColor: c.infoSubtle, borderColor: c.infoBorder, borderWidth: 1, borderRadius: raio.lg, paddingHorizontal: espaco[3], paddingVertical: espaco[2] }}>
      <Texto variante="bodySm" cor={c.infoText}>
        Mostrando dados de exemplo
      </Texto>
    </View>
  );
}

export function Tela({
  children,
  aoAtualizar,
  atualizando = false,
}: {
  children: ReactNode;
  aoAtualizar?: () => void;
  atualizando?: boolean;
}) {
  const { c } = useTema();
  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: c.bgCanvas }}
      contentContainerStyle={{ padding: espaco[4], gap: espaco[4], paddingBottom: ESPACO_BOTAO_CRISE }}
      keyboardShouldPersistTaps="handled"
      refreshControl={
        aoAtualizar ? <RefreshControl refreshing={atualizando} onRefresh={aoAtualizar} tintColor={c.primarySolid} /> : undefined
      }>
      <FaixaMock />
      {children}
    </ScrollView>
  );
}
