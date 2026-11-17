// Meus lugares: favoritos com o selo do nivel atual e o historico de crises (so no celular).
import { useFocusEffect } from 'expo-router';
import { useCallback, useState } from 'react';
import { View } from 'react-native';

import { lugares } from '@/api/calmaria';
import { Cartao, Texto, Titulo, Vazio } from '@/components/base';
import { CartaoLugar } from '@/components/CartaoLugar';
import { Tela } from '@/components/Tela';
import { lerCrises, lerFavoritos, type Crise, type Favorito } from '@/db';
import { espaco, useTema } from '@/theme';
import type { LugarResumo } from '@/types';

export default function MeusLugares() {
  const { c } = useTema();
  const [favoritos, setFavoritos] = useState<Favorito[]>([]);
  const [atuais, setAtuais] = useState<LugarResumo[]>([]);
  const [crises, setCrises] = useState<Crise[]>([]);

  const carregar = useCallback(() => {
    setFavoritos(lerFavoritos());
    setCrises(lerCrises());
    lugares
      .listar()
      .then((r) => setAtuais(r.dados))
      .catch(() => {});
  }, []);

  useFocusEffect(carregar);

  return (
    <Tela aoAtualizar={carregar}>
      <Titulo>Favoritos</Titulo>
      {favoritos.length === 0 && <Vazio texto="Você ainda não favoritou nenhum lugar. Abra um lugar e toque em Favoritar." />}
      {favoritos.map((f) => {
        const atual = atuais.find((l) => l.id === f.lugarId);
        const lugar: LugarResumo = atual ?? {
          id: f.lugarId,
          nome: f.nome,
          tipo: f.tipo,
          latitude: 0,
          longitude: 0,
          distrito: '',
          ehRefugio: f.tipo === 'REFUGIO',
          distanciaM: null,
          indiceAtual: null,
          nivelAtual: null,
          ultimaLeituraEm: null,
        };
        return <CartaoLugar key={f.lugarId} lugar={{ ...lugar, distanciaM: null }} />;
      })}

      <Titulo style={{ marginTop: espaco[4] }}>Minhas crises</Titulo>
      <Texto variante="bodySm" cor={c.textMuted}>
        Este histórico fica só no seu celular. Ele nunca é enviado.
      </Texto>
      {crises.length === 0 && <Vazio texto="Nenhuma crise registrada." />}
      {crises.map((cr) => {
        const d = new Date(cr.iniciadaEm);
        return (
          <Cartao key={cr.id}>
            <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
              <Texto variante="labelLg">{d.toLocaleDateString('pt-BR')}</Texto>
              <Texto variante="label" mono>
                {d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
              </Texto>
            </View>
            <Texto cor={c.textMuted}>
              {cr.refugioNome ? `Lugar sugerido: ${cr.refugioNome}${cr.distanciaM ? `, a ${cr.distanciaM} m` : ''}.` : 'Sem lugar sugerido.'}
            </Texto>
            <Texto cor={cr.chegou ? c.successText : c.textMuted}>{cr.chegou ? 'Chegou ao lugar calmo.' : 'Chegada não marcada.'}</Texto>
          </Cartao>
        );
      })}
    </Tela>
  );
}
