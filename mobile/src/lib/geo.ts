// Distancia em linha reta pela formula de Haversine (raio da Terra 6.371 km).
// Minutos a pe com 80 metros por minuto: o mesmo criterio da API.

const RAIO_TERRA_M = 6_371_000;
export const METROS_POR_MINUTO = 80;

// Ponto de referencia dos testes: Av. Paulista, em frente ao MASP.
export const PONTO_REFERENCIA = { latitude: -23.5614, longitude: -46.6559 };

export interface Coordenada {
  latitude: number;
  longitude: number;
}

function rad(graus: number): number {
  return (graus * Math.PI) / 180;
}

export function distanciaM(a: Coordenada, b: Coordenada): number {
  const dLat = rad(b.latitude - a.latitude);
  const dLng = rad(b.longitude - a.longitude);
  const h =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(rad(a.latitude)) * Math.cos(rad(b.latitude)) * Math.sin(dLng / 2) ** 2;
  return Math.round(2 * RAIO_TERRA_M * Math.asin(Math.sqrt(h)));
}

export function minutosAPe(metros: number): number {
  return Math.max(1, Math.ceil(metros / METROS_POR_MINUTO));
}

export function linkTrajeto(destino: Coordenada): string {
  return `https://www.google.com/maps/dir/?api=1&destination=${destino.latitude},${destino.longitude}&travelmode=walking`;
}
