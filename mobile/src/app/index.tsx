// Boas-vindas: o que o app faz em 3 frases; "Criar meu perfil" ou "Pular".
// Nas duas opcoes cria o usuario anonimo (sem nome, e-mail nem CPF) e guarda o id no SQLite.
import { Redirect, router } from 'expo-router';
import { useState } from 'react';
import { Image, ScrollView, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { usuarios } from '@/api/calmaria';
import { Botao, Erro, Texto } from '@/components/base';
import { CONFIG, gravarConfig, lerConfig } from '@/db';
import { espaco, useTema } from '@/theme';

export default function BoasVindas() {
  const { c, escuro } = useTema();
  const insets = useSafeAreaInsets();
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  if (lerConfig(CONFIG.boasVindasVistas) === '1') return <Redirect href="/mapa" />;

  async function comecar(destino: '/perfil' | '/mapa') {
    setEnviando(true);
    setErro(null);
    try {
      if (!lerConfig(CONFIG.usuarioId)) {
        const r = await usuarios.criar();
        gravarConfig(CONFIG.usuarioId, r.dados.id);
      }
      gravarConfig(CONFIG.boasVindasVistas, '1');
      router.replace('/mapa');
      if (destino === '/perfil') router.push('/perfil');
    } catch {
      setErro('Não consegui começar agora. Tente de novo.');
    } finally {
      setEnviando(false);
    }
  }

  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: c.bgCanvas }}
      contentContainerStyle={{ padding: espaco[6], paddingTop: insets.top + espaco[10], gap: espaco[6], flexGrow: 1 }}>
      <Image
        source={escuro ? require('@/assets/images/logotipo-invertido.png') : require('@/assets/images/logotipo.png')}
        style={{ width: 200, height: 50 }}
        resizeMode="contain"
        accessibilityLabel="Calmaria"
      />
      <View style={{ gap: espaco[4] }}>
        <Texto variante="titleLg" accessibilityRole="header">
          Saiba antes de sair.
        </Texto>
        <Texto variante="bodyLg">Este app mostra se um mercado ou uma estação está barulhento, com luz forte ou cheio.</Texto>
        <Texto variante="bodyLg">Ele mostra os horários mais calmos de cada lugar.</Texto>
        <Texto variante="bodyLg">
          Se precisar, o botão &quot;Preciso de calma&quot; mostra o lugar calmo mais perto, mesmo sem internet.
        </Texto>
      </View>
      <Texto variante="bodySm" cor={c.textMuted}>
        Não pedimos nome, e-mail nem CPF.
      </Texto>
      {erro && <Erro texto={erro} />}
      <View style={{ gap: espaco[3], marginTop: 'auto' }}>
        <Botao rotulo="Criar meu perfil" grande onPress={() => comecar('/perfil')} disabled={enviando} />
        <Botao rotulo="Pular" variante="secundario" grande onPress={() => comecar('/mapa')} disabled={enviando} />
      </View>
    </ScrollView>
  );
}
