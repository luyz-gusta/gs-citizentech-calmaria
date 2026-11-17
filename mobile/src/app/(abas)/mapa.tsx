// Lugares perto: mapa e lista (a lista e a alternativa para o leitor de tela), filtro por tipo.
import { useCallback, useEffect, useState } from 'react';
import { View } from 'react-native';

import { lugares as apiLugares } from '@/api/calmaria';
import { Botao, Carregando, Erro, Texto, Vazio } from '@/components/base';
import { ESPACO_BOTAO_CRISE } from '@/components/BotaoCrise';
import { CartaoLugar } from '@/components/CartaoLugar';
import { Mapa } from '@/components/Mapa';
import { FaixaMock, Tela } from '@/components/Tela';
import { PONTO_REFERENCIA, type Coordenada } from '@/lib/geo';
import { obterPosicao } from '@/lib/localizacao';
import { espaco, useTema } from '@/theme';
import type { LugarResumo, TipoLugar } from '@/types';

const FILTROS: { rotulo: string; tipo?: TipoLugar }[] = [
  { rotulo: 'Todos' },
  { rotulo: 'Mercados', tipo: 'MERCADO' },
  { rotulo: 'Estações', tipo: 'ESTACAO' },
  { rotulo: 'Lugares calmos', tipo: 'REFUGIO' },
];

async function buscarLugares(posicao: Coordenada | null, semPosicao: boolean, tipo?: TipoLugar) {
  const pos = posicao ?? (semPosicao ? null : await obterPosicao());
  const r = await apiLugares.listar({ lat: pos?.latitude, lng: pos?.longitude, tipo });
  return { pos, lista: r.dados };
}

export default function LugaresPerto() {
  const { c } = useTema();
  const [modo, setModo] = useState<'lista' | 'mapa'>('lista');
  const [tipo, setTipo] = useState<TipoLugar | undefined>();
  const [posicao, setPosicao] = useState<Coordenada | null>(null);
  const [semPosicao, setSemPosicao] = useState(false);
  const [lista, setLista] = useState<LugarResumo[] | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [selecionado, setSelecionado] = useState<number | null>(null);

  // A busca nao mexe no estado; o estado so muda nos callbacks (regra do React para efeitos).
  const buscar = useCallback(
    () =>
      buscarLugares(posicao, semPosicao, tipo)
        .then(
          (r) => {
            if (r.pos) setPosicao(r.pos);
            else setSemPosicao(true);
            setLista(r.lista);
            setErro(null);
          },
          () => setErro('Não consegui carregar os lugares.'),
        )
        .finally(() => setCarregando(false)),
    [posicao, semPosicao, tipo],
  );

  useEffect(() => {
    buscar();
  }, [buscar]);

  // Puxar para atualizar e "Tentar de novo".
  const carregar = () => {
    setCarregando(true);
    setErro(null);
    buscar();
  };
  const filtrar = (t: TipoLugar | undefined) => {
    setCarregando(true);
    setErro(null);
    setTipo(t);
  };

  const cabecalho = (
    <View style={{ gap: espaco[3] }}>
      <View style={{ flexDirection: 'row', gap: espaco[2] }} accessibilityRole="tablist">
        <Botao rotulo="Lista" variante="secundario" selecionado={modo === 'lista'} onPress={() => setModo('lista')} style={{ flex: 1 }} accessibilityRole="tab" />
        <Botao rotulo="Mapa" variante="secundario" selecionado={modo === 'mapa'} onPress={() => setModo('mapa')} style={{ flex: 1 }} accessibilityRole="tab" />
      </View>
      <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: espaco[2] }}>
        {FILTROS.map((f) => (
          <Botao
            key={f.rotulo}
            rotulo={f.rotulo}
            variante="secundario"
            selecionado={tipo === f.tipo}
            onPress={() => filtrar(f.tipo)}
            accessibilityHint="Filtra a lista por tipo de lugar"
          />
        ))}
      </View>
      {semPosicao && (
        <Texto variante="bodySm" cor={c.textMuted}>
          Não consegui achar sua localização. Mostrando todos os lugares da região.
        </Texto>
      )}
    </View>
  );

  if (modo === 'mapa') {
    return (
      <View style={{ flex: 1, backgroundColor: c.bgCanvas }}>
        <View style={{ padding: espaco[4], gap: espaco[3] }}>
          <FaixaMock />
          {cabecalho}
        </View>
        <Mapa lugares={lista ?? []} centro={posicao ?? PONTO_REFERENCIA} selecionado={selecionado} aoSelecionar={setSelecionado} />
        {selecionado !== null && lista?.find((l) => l.id === selecionado) && (
          <View style={{ padding: espaco[4], paddingBottom: ESPACO_BOTAO_CRISE }}>
            <CartaoLugar lugar={lista.find((l) => l.id === selecionado)!} />
          </View>
        )}
      </View>
    );
  }

  return (
    <Tela aoAtualizar={carregar} atualizando={carregando && lista !== null}>
      {cabecalho}
      {carregando && lista === null && <Carregando texto="Procurando lugares perto..." />}
      {erro && <Erro texto={erro} aoTentar={carregar} />}
      {lista && lista.length === 0 && <Vazio texto="Nenhum lugar deste tipo por aqui." />}
      {lista?.map((l) => <CartaoLugar key={l.id} lugar={l} />)}
    </Tela>
  );
}
