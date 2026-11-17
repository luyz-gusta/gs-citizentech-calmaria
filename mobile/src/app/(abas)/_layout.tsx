import { router } from 'expo-router';
import { Tabs } from 'expo-router/js-tabs';
import { Heart, Map, MessageSquare, UserRound } from 'lucide-react-native';
import { Pressable } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { Texto } from '@/components/base';
import { ALTURA_ABAS } from '@/components/BotaoCrise';
import { fontes, tamanho, useTema } from '@/theme';

export default function AbasLayout() {
  const { c } = useTema();
  const insets = useSafeAreaInsets();
  return (
    <Tabs
      screenOptions={{
        animation: 'none',
        headerStyle: { backgroundColor: c.bgSurface },
        headerTitleStyle: { fontFamily: fontes.semi, fontSize: 20, color: c.textDefault },
        headerRight: () => (
          <Pressable
            onPress={() => router.push('/perfil')}
            accessibilityRole="button"
            accessibilityLabel="Perfil sensorial"
            style={{ minWidth: tamanho.toqueMin, minHeight: tamanho.toqueMin, flexDirection: 'row', alignItems: 'center', gap: 6, paddingHorizontal: 12 }}>
            <UserRound color={c.primaryText} size={tamanho.icone} strokeWidth={2} />
            <Texto variante="label" cor={c.primaryText}>
              Perfil
            </Texto>
          </Pressable>
        ),
        tabBarStyle: { backgroundColor: c.bgSurface, borderTopColor: c.borderDefault, height: ALTURA_ABAS + insets.bottom, paddingTop: 6 },
        tabBarActiveTintColor: c.primaryText,
        tabBarInactiveTintColor: c.textMuted,
        tabBarLabelStyle: { fontFamily: fontes.semi, fontSize: 14 },
        sceneStyle: { backgroundColor: c.bgCanvas },
      }}>
      <Tabs.Screen
        name="mapa"
        options={{ title: 'Lugares perto', tabBarLabel: 'Lugares', tabBarIcon: ({ color }) => <Map color={color} size={24} strokeWidth={2} /> }}
      />
      <Tabs.Screen
        name="relatar"
        options={{ title: 'Relatar visita', tabBarLabel: 'Relatar', tabBarIcon: ({ color }) => <MessageSquare color={color} size={24} strokeWidth={2} /> }}
      />
      <Tabs.Screen
        name="meus-lugares"
        options={{ title: 'Meus lugares', tabBarLabel: 'Meus lugares', tabBarIcon: ({ color }) => <Heart color={color} size={24} strokeWidth={2} /> }}
      />
    </Tabs>
  );
}
