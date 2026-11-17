// Detalhe do lugar: nivel agora, medidas com frase, 3 horarios mais calmos, grade da semana,
// previsao de barulho do distrito (ML), resumo dos relatos, Como chegar e Favoritar.
import { router, Stack, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useState } from 'react';
import { Linking, View } from 'react-native';

import { ErroApi, lugares, previsoes, relatos } from '@/api/calmaria';
import { Botao, Cartao, Carregando, Erro, Texto, Titulo, Vazio } from '@/components/base';
import { coresDoNivel, FormaNivel, SeloNivel, SeloPrevisao } from '@/components/nivel';
import { Tela } from '@/components/Tela';
import { alternarFavorito, ehFavorito } from '@/db';
import { linkTrajeto } from '@/lib/geo';
import {
  dataCurta,
  DIAS,
  diaDaData,
  fraseLuz,
  fraseMovimento,
  fraseSom,
  NOME_DIA,
  NOME_DIA_CURTO,
  NOME_GATILHO,
  NOME_NIVEL,
  NOME_PREVISAO,
  NOME_TIPO,
  nivelDoIndice,
  nomeDistrito,
} from '@/lib/textos';
import { espaco, useTema } from '@/theme';
import type { HorariosCalmos, LugarDetalhe, PaginaRelatos, Previsao, ResumoRelatos } from '@/types';

const HORAS_GRADE = [8, 10, 12, 14, 16, 18, 20];

function Medida({ rotulo, valor, frase }: { rotulo: string; valor: string; frase: string }) {
  const { c } = useTema();
  return (
    <View style={{ gap: 2 }} accessible accessibilityLabel={`${rotulo}: ${valor}. ${frase}`}>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between' }}>
        <Texto variante="label">{rotulo}</Texto>
        <Texto variante="label" mono>
          {valor}
        </Texto>
      </View>
      <Texto variante="bodySm" cor={c.textMuted}>
        {frase}
      </Texto>
    </View>
  );
}

async function buscarTudo(id: number) {
  const lugar = (await lugares.obter(id)).dados;
  const [h, p, r, rl] = await Promise.allSettled([
    lugares.horariosCalmos(id),
    previsoes.barulho(lugar.distrito, 7),
    relatos.resumo(id),
    relatos.listar(id, 1, 3),
  ]);
  return {
    lugar,
    horarios: h.status === 'fulfilled' ? h.value.dados : null,
    prev: p.status === 'fulfilled' ? p.value.dados : [],
    resumo: r.status === 'fulfilled' ? r.value.dados : null,
    lista: rl.status === 'fulfilled' ? rl.value.dados : null,
  };
}

export default function DetalheLugar() {
  const { c } = useTema();
  const id = Number(useLocalSearchParams<{ id: string }>().id);
  const [lugar, setLugar] = useState<LugarDetalhe | null>(null);
  const [horarios, setHorarios] = useState<HorariosCalmos | null>(null);
  const [prev, setPrev] = useState<Previsao[] | null>(null);
  const [resumo, setResumo] = useState<ResumoRelatos | null>(null);
  const [lista, setLista] = useState<PaginaRelatos | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [favorito, setFavorito] = useState(() => ehFavorito(id));

  // A busca nao mexe no estado; o estado so muda nos callbacks (regra do React para efeitos).
  const carregar = useCallback(
    () =>
      buscarTudo(id).then(
        (d) => {
          setErro(null);
          setLugar(d.lugar);
          setHorarios(d.horarios);
          setPrev(d.prev);
          setResumo(d.resumo);
          setLista(d.lista);
        },
        (e) =>
          setErro(e instanceof ErroApi && e.status === 404 ? 'Este lugar não existe mais.' : 'Não consegui carregar este lugar.'),
      ),
    [id],
  );

  useEffect(() => {
    carregar();
  }, [carregar]);

  if (erro) {
    return (
      <Tela>
        <Erro texto={erro} aoTentar={carregar} />
      </Tela>
    );
  }
  if (!lugar) {
    return (
      <Tela>
        <Carregando />
      </Tela>
    );
  }

  const leitura = lugar.ultimaLeitura;
  const nivel = leitura?.nivel ?? null;
  const k = nivel ? coresDoNivel(c, nivel) : null;
  const distrito = nomeDistrito(lugar.distrito);
  const proximoFimDeSemana = prev?.find((p) => ['SABADO', 'DOMINGO'].includes(diaDaData(p.data)));

  return (
    <Tela aoAtualizar={carregar}>
      <Stack.Screen options={{ title: lugar.nome }} />
      <View style={{ gap: espaco[1] }}>
        <Texto variante="titleMd" accessibilityRole="header">
          {lugar.nome}
        </Texto>
        <Texto cor={c.textMuted}>
          {NOME_TIPO[lugar.tipo]} · {distrito}
          {lugar.horarioFuncionamento ? ` · abre ${lugar.horarioFuncionamento.replace('-', ' às ')}` : ''}
        </Texto>
      </View>

      {/* Nivel agora */}
      <Cartao style={k ? { backgroundColor: k.fundo, borderColor: k.borda } : undefined}>
        {nivel && leitura ? (
          <>
            <View style={{ flexDirection: 'row', alignItems: 'center', gap: espaco[3] }} accessible accessibilityLabel={`${NOME_NIVEL[nivel]} agora, índice ${leitura.indice} de 100`}>
              <FormaNivel nivel={nivel} tamanho={40} />
              <Texto variante="titleLg" cor={k!.texto}>
                {NOME_NIVEL[nivel]} agora
              </Texto>
            </View>
            <Texto cor={k!.texto}>
              Índice <Texto mono cor={k!.texto}>{leitura.indice}</Texto> de 100
            </Texto>
            <Medida rotulo="Som" valor={`${leitura.somDb.toFixed(1).replace('.', ',')} dB`} frase={fraseSom(leitura.somDb)} />
            <Medida rotulo="Luz" valor={`${leitura.luzLux} lux`} frase={fraseLuz(leitura.luzLux)} />
            <Medida rotulo="Movimento" valor={`${leitura.movimentosMin} por min`} frase={fraseMovimento(leitura.movimentosMin)} />
            <Texto variante="caption" cor={c.textMuted}>
              Medido às {new Date(leitura.registradaEm).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}. Leituras simuladas no protótipo.
            </Texto>
          </>
        ) : (
          <>
            <SeloNivel nivel={null} />
            <Texto>{lugar.ehRefugio ? (lugar.descricao ?? 'Lugar calmo de referência.') : 'Ainda não temos medições deste lugar.'}</Texto>
          </>
        )}
      </Cartao>

      {/* Horarios mais calmos */}
      {!lugar.ehRefugio && (
        <Cartao>
          <Titulo>Horários mais calmos</Titulo>
          {!horarios ? (
            <Carregando />
          ) : !horarios.suficiente ? (
            <Texto cor={c.textMuted}>Ainda não temos medições suficientes deste lugar.</Texto>
          ) : (
            <>
              {horarios.janelasMaisCalmas.map((j, i) => (
                <View key={i} style={{ flexDirection: 'row', alignItems: 'center', gap: espaco[3] }}>
                  <FormaNivel nivel={nivelDoIndice(j.indiceMedio)} tamanho={20} />
                  <Texto variante="labelLg">
                    {NOME_DIA[j.diaSemana]}, às {j.hora}h
                  </Texto>
                  <Texto cor={c.textMuted}>{NOME_NIVEL[nivelDoIndice(j.indiceMedio)]}</Texto>
                </View>
              ))}
              <Texto variante="label" style={{ marginTop: espaco[2] }}>
                A semana toda
              </Texto>
              <GradeSemana grade={horarios.grade} />
            </>
          )}
        </Cartao>
      )}

      {/* Previsao do ML */}
      <Cartao>
        <Titulo>Previsão de barulho em {distrito}</Titulo>
        {!prev ? (
          <Carregando />
        ) : prev.length === 0 ? (
          <Texto cor={c.textMuted}>Ainda não temos previsão para este distrito.</Texto>
        ) : (
          <>
            {proximoFimDeSemana && (
              <Texto>
                Previsão de barulho em {distrito} no {NOME_DIA[diaDaData(proximoFimDeSemana.data)].toLowerCase()}:{' '}
                {NOME_PREVISAO[proximoFimDeSemana.nivel].toLowerCase()}.
              </Texto>
            )}
            {prev.map((p) => (
              <View
                key={p.data}
                style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', minHeight: 36 }}
                accessible
                accessibilityLabel={`${NOME_DIA[diaDaData(p.data)]}, ${dataCurta(p.data)}: previsão ${NOME_PREVISAO[p.nivel]}`}>
                <Texto variante="label">
                  {NOME_DIA[diaDaData(p.data)]} {dataCurta(p.data)}
                </Texto>
                <SeloPrevisao nivel={p.nivel} />
              </View>
            ))}
            <Texto variante="caption" cor={c.textMuted}>
              Feita com as reclamações de barulho do SP156 (Prefeitura de São Paulo). Diz o dia, não a hora.
            </Texto>
          </>
        )}
      </Cartao>

      {/* Relatos */}
      <Cartao>
        <Titulo>O que as pessoas contaram</Titulo>
        {!resumo || resumo.totalRelatos === 0 ? (
          <Texto cor={c.textMuted}>Ainda não há relatos deste lugar.</Texto>
        ) : (
          <>
            <Texto>
              {resumo.totalRelatos} relatos, nota média {resumo.notaMedia?.toFixed(1).replace('.', ',')} de 5.
            </Texto>
            {resumo.gatilhosMaisCitados.length > 0 && (
              <Texto cor={c.textMuted}>
                Mais citado: {resumo.gatilhosMaisCitados.map((g) => `${NOME_GATILHO[g.gatilho].toLowerCase()} (${g.vezes})`).join(', ')}.
              </Texto>
            )}
            {lista?.itens.map((r) => (
              <View key={r.id} style={{ borderTopWidth: 1, borderTopColor: c.borderDefault, paddingTop: espaco[2] }}>
                <Texto variante="label">Nota {r.nota} de 5</Texto>
                {r.comentario && <Texto>{r.comentario}</Texto>}
              </View>
            ))}
          </>
        )}
      </Cartao>

      <Botao rotulo="Como chegar" grande onPress={() => Linking.openURL(linkTrajeto(lugar))} accessibilityHint="Abre o trajeto a pé no app de mapas" />
      <Botao
        rotulo={favorito ? 'Tirar dos favoritos' : 'Favoritar'}
        variante="secundario"
        onPress={() => setFavorito(alternarFavorito({ lugarId: lugar.id, nome: lugar.nome, tipo: lugar.tipo }))}
      />
      <Botao rotulo="Relatar uma visita" variante="secundario" onPress={() => router.push({ pathname: '/relatar', params: { lugarId: String(lugar.id) } })} />
    </Tela>
  );
}

function GradeSemana({ grade }: { grade: HorariosCalmos['grade'] }) {
  const { c } = useTema();
  if (grade.length === 0) return <Vazio texto="Sem dados da semana." />;
  const valor = (dia: string, hora: number) => grade.find((g) => g.diaSemana === dia && g.hora === hora)?.indiceMedio;
  return (
    <View style={{ gap: 4 }}>
      <View style={{ flexDirection: 'row' }}>
        <View style={{ width: 44 }} />
        {HORAS_GRADE.map((h) => (
          <Texto key={h} variante="caption" cor={c.textMuted} style={{ flex: 1, textAlign: 'center' }}>
            {h}h
          </Texto>
        ))}
      </View>
      {DIAS.map((d) => (
        <View
          key={d}
          style={{ flexDirection: 'row', alignItems: 'center' }}
          accessible
          accessibilityLabel={`${NOME_DIA[d]}: ${HORAS_GRADE.map((h) => {
            const v = valor(d, h);
            return v === undefined ? '' : `${h}h ${NOME_NIVEL[nivelDoIndice(v)]}`;
          }).join(', ')}`}>
          <Texto variante="caption" style={{ width: 44 }}>
            {NOME_DIA_CURTO[d]}
          </Texto>
          {HORAS_GRADE.map((h) => {
            const v = valor(d, h);
            return (
              <View key={h} style={{ flex: 1, alignItems: 'center', paddingVertical: 2 }}>
                {v !== undefined ? <FormaNivel nivel={nivelDoIndice(v)} tamanho={20} /> : <Texto variante="caption">-</Texto>}
              </View>
            );
          })}
        </View>
      ))}
    </View>
  );
}
