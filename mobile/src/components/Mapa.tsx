// Mapa nativo (react-native-maps). Cada marcador: forma, letra e cor, contorno de 2,
// alvo de 48; o selecionado ganha um anel de 4 na cor da marca.
import MapView, { Marker } from 'react-native-maps';
import { View } from 'react-native';

import { NOME_NIVEL } from '@/lib/textos';
import { useTema } from '@/theme';
import type { LugarResumo } from '@/types';

import { FormaNivel } from './nivel';
import { Texto } from './base';

export interface PropsMapa {
  lugares: LugarResumo[];
  centro: { latitude: number; longitude: number };
  selecionado: number | null;
  aoSelecionar: (id: number) => void;
}

export function Mapa({ lugares, centro, selecionado, aoSelecionar }: PropsMapa) {
  const { c } = useTema();
  return (
    <MapView
      style={{ flex: 1 }}
      initialRegion={{ ...centro, latitudeDelta: 0.02, longitudeDelta: 0.02 }}
      showsUserLocation
      accessibilityLabel="Mapa dos lugares. Para leitor de tela, use a opção Lista.">
      {lugares.map((l) => (
        <Marker
          key={l.id}
          coordinate={{ latitude: l.latitude, longitude: l.longitude }}
          title={l.nome}
          description={l.nivelAtual ? `${NOME_NIVEL[l.nivelAtual]} agora` : 'Lugar calmo de referência'}
          onPress={() => aoSelecionar(l.id)}
          tracksViewChanges={false}>
          <View
            style={{
              width: 48,
              height: 48,
              alignItems: 'center',
              justifyContent: 'center',
              borderRadius: 24,
              borderWidth: selecionado === l.id ? 4 : 0,
              borderColor: c.primarySolid,
            }}>
            {l.nivelAtual ? (
              <FormaNivel nivel={l.nivelAtual} tamanho={36} contorno={c.bgSurface} />
            ) : (
              <View
                style={{
                  width: 36,
                  height: 36,
                  borderRadius: 18,
                  backgroundColor: c.primarySolid,
                  borderWidth: 2,
                  borderColor: c.bgSurface,
                  alignItems: 'center',
                  justifyContent: 'center',
                }}>
                <Texto variante="labelSm" cor={c.primaryOnSolid} allowFontScaling={false}>
                  R
                </Texto>
              </View>
            )}
          </View>
        </Marker>
      ))}
    </MapView>
  );
}
