// Relatar uma visita: lugar, nota com rotulo, gatilhos, comentario.
// Antes de enviar, POST /api/privacidade/analisar avisa o que sera removido do comentario.
import { useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { TextInput, View } from 'react-native';

import { ErroApi, lugares, privacidade, relatos, usuarios } from '@/api/calmaria';
import { Botao, Cartao, Carregando, Erro, Texto, Titulo } from '@/components/base';
import { Tela } from '@/components/Tela';
import { CONFIG, gravarConfig, lerConfig } from '@/db';
import { NOME_GATILHO, NOTA_ROTULO } from '@/lib/textos';
import { espaco, fontes, raio, useTema } from '@/theme';
import type { AnalisePrivacidade, Gatilho, LugarResumo, RelatoCriado } from '@/types';

const GATILHOS: Gatilho[] = ['BARULHO', 'LUZ_FORTE', 'MULTIDAO', 'CHEIRO', 'FILA', 'OUTRO'];

export default function Relatar() {
  const { c } = useTema();
  const params = useLocalSearchParams<{ lugarId?: string }>();
  const [opcoes, setOpcoes] = useState<LugarResumo[] | null>(null);
  const [lugarId, setLugarId] = useState<number | null>(params.lugarId ? Number(params.lugarId) : null);
  const [nota, setNota] = useState<number | null>(null);
  const [gatilhos, setGatilhos] = useState<Gatilho[]>([]);
  const [comentario, setComentario] = useState('');
  const [analise, setAnalise] = useState<AnalisePrivacidade | null>(null);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [enviado, setEnviado] = useState<RelatoCriado | null>(null);

  useEffect(() => {
    lugares
      .listar()
      .then((r) => setOpcoes(r.dados))
      .catch(() => setErro('Não consegui carregar os lugares.'));
  }, []);

  // Veio de outro lugar ("Relatar uma visita" no detalhe): troca a escolha durante a renderizacao.
  const [paramAnterior, setParamAnterior] = useState(params.lugarId);
  if (params.lugarId !== paramAnterior) {
    setParamAnterior(params.lugarId);
    if (params.lugarId) setLugarId(Number(params.lugarId));
  }

  const alternar = (g: Gatilho) => setGatilhos((atual) => (atual.includes(g) ? atual.filter((x) => x !== g) : [...atual, g]));

  function limpar() {
    setNota(null);
    setGatilhos([]);
    setComentario('');
    setAnalise(null);
    setEnviado(null);
    setErro(null);
  }

  // Passo 1: se houver comentario, pergunta a API o que sera removido.
  async function revisar() {
    setErro(null);
    if (!lugarId || !nota) {
      setErro('Escolha o lugar e a nota.');
      return;
    }
    if (comentario.trim()) {
      setEnviando(true);
      try {
        const a = (await privacidade.analisar(comentario.trim())).dados;
        if (a.temDadoPessoal) {
          setAnalise(a);
          return;
        }
      } catch {
        // Se a analise falhar, a API ainda filtra no envio.
      } finally {
        setEnviando(false);
      }
    }
    await enviar();
  }

  // Passo 2: envia o relato.
  async function enviar() {
    setEnviando(true);
    setErro(null);
    try {
      let usuarioId = lerConfig(CONFIG.usuarioId);
      if (!usuarioId) {
        usuarioId = (await usuarios.criar()).dados.id;
        gravarConfig(CONFIG.usuarioId, usuarioId);
      }
      const r = await relatos.criar({
        usuarioId,
        lugarId: lugarId!,
        nota: nota!,
        gatilhos,
        comentario: comentario.trim() || null,
        visitadoEm: new Date().toISOString(),
      });
      setEnviado(r.dados);
      setAnalise(null);
    } catch (e) {
      if (e instanceof ErroApi && e.status === 409) setErro('Você já relatou este lugar há menos de 30 minutos.');
      else if (e instanceof ErroApi) setErro(e.message);
      else setErro('Não consegui enviar agora. Tente de novo.');
    } finally {
      setEnviando(false);
    }
  }

  if (enviado) {
    return (
      <Tela>
        <Cartao>
          <Titulo>Relato enviado. Obrigado.</Titulo>
          {enviado.removidos.length > 0 ? (
            <Texto>Removemos do comentário: {enviado.removidos.join(', ')}. Ninguém vê esses dados.</Texto>
          ) : (
            <Texto cor={c.textMuted}>Nenhum dado pessoal foi encontrado no comentário.</Texto>
          )}
          {enviado.comentario && <Texto cor={c.textMuted}>Comentário publicado: {enviado.comentario}</Texto>}
        </Cartao>
        <Botao rotulo="Relatar outra visita" onPress={limpar} />
      </Tela>
    );
  }

  return (
    <Tela>
      <Texto>Conte como foi a visita. O relato ajuda outras pessoas. Ele não mostra quem escreveu.</Texto>

      <Cartao>
        <Titulo>Lugar</Titulo>
        {!opcoes && <Carregando />}
        <View style={{ gap: espaco[2] }}>
          {opcoes?.map((l) => (
            <Botao key={l.id} rotulo={l.nome} variante="secundario" selecionado={lugarId === l.id} onPress={() => setLugarId(l.id)} accessibilityState={{ selected: lugarId === l.id }} />
          ))}
        </View>
      </Cartao>

      <Cartao>
        <Titulo>Como foi para você?</Titulo>
        <View style={{ flexDirection: 'row', gap: espaco[2] }}>
          {[1, 2, 3, 4, 5].map((n) => (
            <Botao
              key={n}
              rotulo={String(n)}
              variante="secundario"
              selecionado={nota === n}
              accessibilityLabel={`Nota ${n}: ${NOTA_ROTULO[n]}`}
              onPress={() => setNota(n)}
              style={{ flex: 1, paddingHorizontal: 0 }}
            />
          ))}
        </View>
        <Texto cor={c.textMuted}>{nota ? `${nota}: ${NOTA_ROTULO[nota]}` : '1 é muito difícil, 5 é muito tranquilo.'}</Texto>
      </Cartao>

      <Cartao>
        <Titulo>O que incomodou?</Titulo>
        <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: espaco[2] }}>
          {GATILHOS.map((g) => (
            <Botao
              key={g}
              rotulo={NOME_GATILHO[g]}
              variante="secundario"
              grande
              selecionado={gatilhos.includes(g)}
              accessibilityRole="checkbox"
              accessibilityState={{ checked: gatilhos.includes(g) }}
              onPress={() => alternar(g)}
              style={{ minWidth: '47%', flexGrow: 1 }}
            />
          ))}
        </View>
      </Cartao>

      <Cartao>
        <Titulo>Comentário (opcional)</Titulo>
        <TextInput
          value={comentario}
          onChangeText={(t) => {
            setComentario(t);
            setAnalise(null);
          }}
          multiline
          maxLength={500}
          placeholder="Ex.: muito cheio às 18h"
          placeholderTextColor={c.textSubtle}
          accessibilityLabel="Comentário"
          style={{
            minHeight: 96,
            borderWidth: 2,
            borderColor: c.borderStrong,
            borderRadius: raio.lg,
            padding: espaco[3],
            color: c.textDefault,
            backgroundColor: c.bgSurface,
            fontFamily: fontes.regular,
            fontSize: 18,
            textAlignVertical: 'top',
          }}
        />
        <Texto variante="bodySm" cor={c.textMuted}>
          Não escreva nome, telefone, CPF ou e-mail. Se escrever, a Calmaria remove antes de publicar.
        </Texto>
      </Cartao>

      {analise && (
        <Cartao style={{ backgroundColor: c.warningSubtle, borderColor: c.warningBorder }}>
          <Titulo>Vamos remover dados pessoais</Titulo>
          {analise.achados.map((a, i) => (
            <Texto key={i}>
              {a.categoria}: {a.trecho}
            </Texto>
          ))}
          <Texto>O comentário vai ficar assim:</Texto>
          <Texto variante="labelLg">{analise.textoSanitizado}</Texto>
          <Botao rotulo="Enviar assim" onPress={enviar} disabled={enviando} />
          <Botao rotulo="Editar o comentário" variante="secundario" onPress={() => setAnalise(null)} />
        </Cartao>
      )}

      {erro && <Erro texto={erro} />}
      {!analise && <Botao rotulo={enviando ? 'Enviando...' : 'Enviar relato'} grande onPress={revisar} disabled={enviando} />}
    </Tela>
  );
}
