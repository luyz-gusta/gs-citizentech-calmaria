import {
  AtkinsonHyperlegibleNext_400Regular,
  AtkinsonHyperlegibleNext_500Medium,
  AtkinsonHyperlegibleNext_600SemiBold,
  AtkinsonHyperlegibleNext_700Bold,
  useFonts,
} from '@expo-google-fonts/atkinson-hyperlegible-next';
import { AtkinsonHyperlegibleMono_400Regular } from '@expo-google-fonts/atkinson-hyperlegible-mono';
import { DarkTheme, DefaultTheme, Stack, ThemeProvider } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import { StatusBar } from 'expo-status-bar';
import { useEffect } from 'react';
import { View } from 'react-native';

import { crise } from '@/api/calmaria';
import { BotaoCrise } from '@/components/BotaoCrise';
import { iniciarBanco, salvarRefugios } from '@/db';
import { fontes, useTema } from '@/theme';

SplashScreen.preventAutoHideAsync();
iniciarBanco();

export default function RaizLayout() {
  const { c, escuro } = useTema();
  const [fontesProntas, erroFontes] = useFonts({
    AtkinsonHyperlegibleNext_400Regular,
    AtkinsonHyperlegibleNext_500Medium,
    AtkinsonHyperlegibleNext_600SemiBold,
    AtkinsonHyperlegibleNext_700Bold,
    AtkinsonHyperlegibleMono_400Regular,
  });

  useEffect(() => {
    if (fontesProntas || erroFontes) SplashScreen.hideAsync();
  }, [fontesProntas, erroFontes]);

  // A cada abertura, atualiza a lista de refugios guardada no celular (modo offline do botao de crise).
  useEffect(() => {
    crise
      .refugios()
      .then((r) => salvarRefugios(r.dados))
      .catch(() => {});
  }, []);

  if (!fontesProntas && !erroFontes) return null;

  const base = escuro ? DarkTheme : DefaultTheme;
  const tema = {
    ...base,
    colors: { ...base.colors, background: c.bgCanvas, card: c.bgSurface, text: c.textDefault, border: c.borderDefault, primary: c.primarySolid },
  };

  return (
    <ThemeProvider value={tema}>
      <StatusBar style={escuro ? 'light' : 'dark'} />
      <View style={{ flex: 1, backgroundColor: c.bgCanvas }}>
        <Stack
          screenOptions={{
            animation: 'none', // sem animacao: um produto para quem sofre com estimulo nao pode ser mais um
            headerStyle: { backgroundColor: c.bgSurface },
            headerTintColor: c.primaryText,
            headerTitleStyle: { fontFamily: fontes.semi, fontSize: 20, color: c.textDefault },
            headerBackButtonDisplayMode: 'minimal',
            contentStyle: { backgroundColor: c.bgCanvas },
          }}>
          <Stack.Screen name="index" options={{ headerShown: false }} />
          <Stack.Screen name="(abas)" options={{ headerShown: false }} />
          <Stack.Screen name="perfil" options={{ title: 'Perfil sensorial' }} />
          <Stack.Screen name="lugar/[id]" options={{ title: 'Lugar' }} />
          <Stack.Screen name="crise" options={{ headerShown: false, presentation: 'fullScreenModal' }} />
        </Stack>
        <BotaoCrise />
      </View>
    </ThemeProvider>
  );
}
