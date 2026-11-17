// Botao de crise: sempre no mesmo lugar (canto inferior direito, 16 da borda e 16 acima das abas),
// 64 de altura e 176 de largura minima. Sem animacao.
import { router, usePathname } from 'expo-router';
import { LifeBuoy } from 'lucide-react-native';
import { Pressable, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { espaco, raio, tamanho, useTema } from '@/theme';

import { Texto } from './base';

export const ALTURA_ABAS = 64;
const ROTAS_COM_ABAS = ['/mapa', '/relatar', '/meus-lugares'];

export function BotaoCrise() {
  const { c } = useTema();
  const rota = usePathname();
  const insets = useSafeAreaInsets();
  if (rota === '/' || rota === '/crise') return null;
  const acimaDasAbas = ROTAS_COM_ABAS.includes(rota) ? ALTURA_ABAS : 0;
  return (
    <View
      pointerEvents="box-none"
      style={{ position: 'absolute', right: espaco[4], bottom: insets.bottom + acimaDasAbas + espaco[4] }}>
      <Pressable
        onPress={() => router.push('/crise')}
        accessibilityRole="button"
        accessibilityLabel="Preciso de calma"
        accessibilityHint="Abre a tela de crise com o lugar calmo mais perto e um roteiro de respiração"
        style={{
          height: tamanho.botaoCrise,
          minWidth: 176,
          paddingHorizontal: espaco[5],
          borderRadius: raio['2xl'],
          backgroundColor: c.botaoCriseBg,
          flexDirection: 'row',
          alignItems: 'center',
          justifyContent: 'center',
          gap: espaco[2],
          borderWidth: 2,
          borderColor: c.bgCanvas,
        }}>
        <LifeBuoy color={c.botaoCriseText} size={tamanho.icone} strokeWidth={2} />
        <Texto variante="labelLg" cor={c.botaoCriseText}>
          Preciso de calma
        </Texto>
      </Pressable>
    </View>
  );
}

// Espaco no fim das listas para o conteudo nao ficar embaixo do botao.
export const ESPACO_BOTAO_CRISE = tamanho.botaoCrise + espaco[8];
