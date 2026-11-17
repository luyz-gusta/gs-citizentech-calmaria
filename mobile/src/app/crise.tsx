// Tela de crise: fundo escuro e liso nos dois temas, o refugio mais perto, respiracao passo a passo
// e ligar para o contato de apoio. Sem internet, usa os refugios salvos no SQLite e calcula a distancia
// no celular (Haversine, 80 m por minuto).
import { router } from 'expo-router';
import { useEffect, useRef, useState } from 'react';
import { Linking, Pressable, ScrollView, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { apiConfigurada, crise } from '@/api/calmaria';
import { Texto } from '@/components/base';
import { atualizarCrise, lerPerfil, lerRefugios, marcarChegada, registrarCrise } from '@/db';
import { distanciaM, linkTrajeto, minutosAPe, type Coordenada } from '@/lib/geo';
import { obterPosicao } from '@/lib/localizacao';
import { mock } from '@/mocks';
import { cores, espaco, raio, tamanho } from '@/theme';
import type { Refugio, RefugioMaisProximo } from '@/types';

const k = cores.claro; // a tela de crise usa as mesmas cores nos dois temas

const PASSOS = [
  'Pare onde você está, se for seguro.',
  'Puxe o ar pelo nariz contando até 4.',
  'Segure contando até 4.',
  'Solte o ar pela boca contando até 6.',
  'Repita mais 3 vezes. Não tem pressa.',
];

type Estado =
  | { tipo: 'procurando' }
  | { tipo: 'achou'; r: RefugioMaisProximo; offline: boolean }
  | { tipo: 'semLocalizacao'; lista: Refugio[]; offline: boolean };

function maisProximoOffline(aqui: Coordenada, lista: Refugio[]): RefugioMaisProximo | null {
  if (lista.length === 0) return null;
  const [melhor] = lista.map((l) => ({ l, d: distanciaM(aqui, l) })).sort((a, b) => a.d - b.d);
  return { lugar: melhor.l, distanciaM: melhor.d, minutosAPe: minutosAPe(melhor.d), linkTrajeto: linkTrajeto(melhor.l) };
}

function BotaoCriseTela({ rotulo, onPress, dica, contorno }: { rotulo: string; onPress: () => void; dica?: string; contorno?: boolean }) {
  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={rotulo}
      accessibilityHint={dica}
      style={({ pressed }) => ({
        minHeight: tamanho.botaoCrise,
        borderRadius: raio.xl,
        paddingHorizontal: espaco[4],
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: contorno ? 'transparent' : pressed ? k.criseMuted : k.criseAction,
        borderWidth: contorno ? 2 : 0,
        borderColor: k.criseAction,
      })}>
      <Texto variante="labelLg" cor={contorno ? k.criseText : k.criseOnAction} style={{ textAlign: 'center' }}>
        {rotulo}
      </Texto>
    </Pressable>
  );
}

export default function Crise() {
  const insets = useSafeAreaInsets();
  const [estado, setEstado] = useState<Estado>({ tipo: 'procurando' });
  const [passo, setPasso] = useState(0);
  const [chegou, setChegou] = useState(false);
  const idCrise = useRef<number | null>(null);
  const perfil = lerPerfil();

  useEffect(() => {
    idCrise.current = registrarCrise(null, null);
    let ativo = true;
    (async () => {
      const salvos = lerRefugios();
      const refugiosLocais = salvos.length > 0 ? salvos : mock.refugios();
      const aqui = await obterPosicao(5000);
      if (!ativo) return;
      if (!aqui) {
        setEstado({ tipo: 'semLocalizacao', lista: refugiosLocais, offline: false });
        return;
      }
      let resultado: RefugioMaisProximo | null = null;
      let offline = false;
      if (apiConfigurada) {
        try {
          resultado = await crise.refugioMaisProximoSoApi(aqui.latitude, aqui.longitude);
        } catch {
          offline = true;
        }
      }
      if (!resultado) {
        // Sem API (ou sem internet): o mock tambem considera lugares CALMO agora; o SQLite, so refugios.
        resultado = apiConfigurada ? maisProximoOffline(aqui, refugiosLocais) : mock.refugioMaisProximo(aqui.latitude, aqui.longitude);
      }
      if (!ativo) return;
      if (resultado) {
        setEstado({ tipo: 'achou', r: resultado, offline });
        if (idCrise.current) atualizarCrise(idCrise.current, resultado.lugar.nome, resultado.distanciaM);
      } else {
        setEstado({ tipo: 'semLocalizacao', lista: refugiosLocais, offline });
      }
    })();
    return () => {
      ativo = false;
    };
  }, []);

  const offline = estado.tipo !== 'procurando' && estado.offline;

  return (
    <ScrollView
      style={{ flex: 1, backgroundColor: k.criseBg }}
      contentContainerStyle={{ padding: espaco[5], paddingTop: insets.top + espaco[6], paddingBottom: insets.bottom + espaco[8], gap: espaco[5] }}>
      <Texto variante="titleLg" cor={k.criseText} accessibilityRole="header">
        Você está seguro. Vamos até um lugar calmo.
      </Texto>

      {offline && (
        <Texto cor={k.criseMuted} accessibilityRole="alert">
          Sem internet. Usando os refúgios salvos no celular.
        </Texto>
      )}

      <View style={{ backgroundColor: k.criseSurface, borderRadius: raio.xl, padding: espaco[5], gap: espaco[4] }}>
        {estado.tipo === 'procurando' && <Texto cor={k.criseText}>Procurando o lugar calmo mais perto...</Texto>}

        {estado.tipo === 'achou' && (
          <>
            <Texto variante="bodyLg" cor={k.criseText}>
              {estado.r.lugar.nome}, a {estado.r.distanciaM} metros, cerca de {estado.r.minutosAPe} minutos a pé.
            </Texto>
            {estado.r.lugar.descricao && <Texto cor={k.criseMuted}>{estado.r.lugar.descricao}</Texto>}
            <BotaoCriseTela rotulo="Abrir o caminho no mapa" onPress={() => Linking.openURL(estado.r.linkTrajeto)} dica="Abre o trajeto a pé no app de mapas" />
            {!chegou ? (
              <BotaoCriseTela
                rotulo="Cheguei ao lugar calmo"
                contorno
                onPress={() => {
                  if (idCrise.current) marcarChegada(idCrise.current);
                  setChegou(true);
                }}
              />
            ) : (
              <Texto cor={k.criseText}>Que bom. Fique o tempo que precisar.</Texto>
            )}
          </>
        )}

        {estado.tipo === 'semLocalizacao' && (
          <>
            <Texto cor={k.criseText}>Não consegui achar sua localização. Estes são os refúgios da região:</Texto>
            {estado.lista.map((r) => (
              <Pressable
                key={r.id}
                onPress={() => Linking.openURL(linkTrajeto(r))}
                accessibilityRole="button"
                accessibilityLabel={`${r.nome}. Abrir o caminho no mapa`}
                style={{ minHeight: tamanho.toqueMin, justifyContent: 'center', borderTopWidth: 1, borderTopColor: k.criseBg, paddingVertical: espaco[2] }}>
                <Texto variante="labelLg" cor={k.criseText}>
                  {r.nome}
                </Texto>
                {r.descricao && <Texto variante="bodySm" cor={k.criseMuted}>{r.descricao}</Texto>}
              </Pressable>
            ))}
          </>
        )}
      </View>

      <View style={{ backgroundColor: k.criseSurface, borderRadius: raio.xl, padding: espaco[5], gap: espaco[4] }}>
        <Texto variante="label" cor={k.criseMuted}>
          Respiração, passo {passo + 1} de {PASSOS.length}
        </Texto>
        <Texto variante="titleSm" cor={k.criseText} accessibilityLiveRegion="polite">
          {PASSOS[passo]}
        </Texto>
        <BotaoCriseTela
          rotulo={passo < PASSOS.length - 1 ? 'Próximo passo' : 'Começar de novo'}
          onPress={() => setPasso((p) => (p + 1) % PASSOS.length)}
        />
      </View>

      {perfil?.contatoApoioTelefone && (
        <BotaoCriseTela
          rotulo={`Ligar para ${perfil.contatoApoioNome || 'meu contato'}`}
          onPress={() => Linking.openURL(`tel:${perfil.contatoApoioTelefone}`)}
          dica="Abre a discagem do telefone"
        />
      )}

      <BotaoCriseTela rotulo="Fechar" contorno onPress={() => (router.canGoBack() ? router.back() : router.replace('/mapa'))} />
    </ScrollView>
  );
}
