import * as Location from 'expo-location';

import type { Coordenada } from './geo';

// Pede a permissao e devolve a posicao, ou null se a pessoa negar ou demorar demais.
export async function obterPosicao(tempoLimiteMs = 6000): Promise<Coordenada | null> {
  try {
    const { status } = await Location.requestForegroundPermissionsAsync();
    if (status !== 'granted') return null;
    const ultima = await Location.getLastKnownPositionAsync();
    const atual = Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
    const limite = new Promise<null>((ok) => setTimeout(() => ok(null), tempoLimiteMs));
    const pos = (await Promise.race([atual, limite])) ?? ultima;
    return pos ? { latitude: pos.coords.latitude, longitude: pos.coords.longitude } : null;
  } catch {
    return null;
  }
}
