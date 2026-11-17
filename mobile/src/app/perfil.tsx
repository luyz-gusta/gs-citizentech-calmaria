// Perfil sensorial: som, luz e multidao de 1 a 5, com a frase de cada valor; contato de apoio.
// Fica no SQLite e vai para a API (PUT /api/usuarios/{id}/perfil-sensorial).
import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, TextInput, View } from 'react-native';

import { ErroApi, usuarios } from '@/api/calmaria';
import { Botao, Cartao, Erro, Texto, Titulo } from '@/components/base';
import { Tela } from '@/components/Tela';
import { apagarTudo, CONFIG, gravarConfig, gravarPerfil, lerConfig, lerPerfil } from '@/db';
import { SENSIBILIDADE_ROTULO } from '@/lib/textos';
import { espaco, fontes, raio, tamanho, useTema } from '@/theme';
import type { PerfilSensorial } from '@/types';

function Seletor({ titulo, valor, aoMudar }: { titulo: string; valor: number; aoMudar: (v: number) => void }) {
  const { c } = useTema();
  return (
    <Cartao>
      <Titulo>{titulo}</Titulo>
      <View style={{ flexDirection: 'row', gap: espaco[2] }} accessibilityRole="radiogroup" accessibilityLabel={titulo}>
        {[1, 2, 3, 4, 5].map((n) => (
          <Botao
            key={n}
            rotulo={String(n)}
            variante="secundario"
            selecionado={valor === n}
            accessibilityRole="radio"
            accessibilityLabel={`${titulo}: ${n}, ${SENSIBILIDADE_ROTULO[n]}`}
            accessibilityState={{ checked: valor === n }}
            onPress={() => aoMudar(n)}
            style={{ flex: 1, paddingHorizontal: 0 }}
          />
        ))}
      </View>
      <Texto cor={c.textMuted}>
        {valor}: {SENSIBILIDADE_ROTULO[valor]}
      </Texto>
    </Cartao>
  );
}

export default function Perfil() {
  const { c } = useTema();
  const salvo = lerPerfil();
  const [p, setP] = useState<PerfilSensorial>(
    salvo ?? { sensibilidadeSom: 3, sensibilidadeLuz: 3, sensibilidadeMultidao: 3, contatoApoioNome: null, contatoApoioTelefone: null },
  );
  const [enviando, setEnviando] = useState(false);
  const [aviso, setAviso] = useState<string | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  const campo = {
    minHeight: tamanho.controle,
    borderWidth: 2,
    borderColor: c.borderStrong,
    borderRadius: raio.lg,
    paddingHorizontal: espaco[3],
    color: c.textDefault,
    backgroundColor: c.bgSurface,
    fontFamily: fontes.regular,
    fontSize: 18,
  };

  async function salvar() {
    setEnviando(true);
    setErro(null);
    setAviso(null);
    const perfil: PerfilSensorial = {
      ...p,
      contatoApoioNome: p.contatoApoioNome?.trim() || null,
      contatoApoioTelefone: p.contatoApoioTelefone?.replace(/\D/g, '') || null,
    };
    gravarPerfil(perfil); // salva no celular primeiro: funciona mesmo sem internet
    try {
      let id = lerConfig(CONFIG.usuarioId);
      if (!id) {
        id = (await usuarios.criar()).dados.id;
        gravarConfig(CONFIG.usuarioId, id);
      }
      await usuarios.salvarPerfil(id, perfil);
      setAviso('Perfil salvo.');
      router.back();
    } catch (e) {
      setErro(e instanceof ErroApi ? e.message : 'Salvei no celular, mas não consegui enviar agora.');
    } finally {
      setEnviando(false);
    }
  }

  function apagar() {
    Alert.alert('Apagar meus dados', 'Isto apaga o perfil, os favoritos e o histórico do celular e da Calmaria.', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Apagar',
        style: 'destructive',
        onPress: async () => {
          const id = lerConfig(CONFIG.usuarioId);
          if (id) await usuarios.apagar(id).catch(() => {});
          apagarTudo();
          router.replace('/');
        },
      },
    ]);
  }

  return (
    <Tela>
      <Texto>Diga o quanto cada coisa incomoda você. O app usa isso para escolher o que mostrar primeiro.</Texto>
      <Seletor titulo="Som" valor={p.sensibilidadeSom} aoMudar={(v) => setP({ ...p, sensibilidadeSom: v })} />
      <Seletor titulo="Luz" valor={p.sensibilidadeLuz} aoMudar={(v) => setP({ ...p, sensibilidadeLuz: v })} />
      <Seletor titulo="Multidão" valor={p.sensibilidadeMultidao} aoMudar={(v) => setP({ ...p, sensibilidadeMultidao: v })} />

      <Cartao>
        <Titulo>Contato de apoio</Titulo>
        <Texto variante="bodySm" cor={c.textMuted}>
          Pessoa para ligar na tela de crise. Opcional.
        </Texto>
        <Texto variante="label">Nome</Texto>
        <TextInput
          style={campo}
          value={p.contatoApoioNome ?? ''}
          onChangeText={(t) => setP({ ...p, contatoApoioNome: t })}
          placeholder="Ex.: Mãe"
          placeholderTextColor={c.textSubtle}
          accessibilityLabel="Nome do contato de apoio"
        />
        <Texto variante="label">Telefone</Texto>
        <TextInput
          style={campo}
          value={p.contatoApoioTelefone ?? ''}
          onChangeText={(t) => setP({ ...p, contatoApoioTelefone: t })}
          keyboardType="phone-pad"
          placeholder="Ex.: 11 90000-0000"
          placeholderTextColor={c.textSubtle}
          accessibilityLabel="Telefone do contato de apoio"
        />
      </Cartao>

      {erro && <Erro texto={erro} />}
      {aviso && <Texto cor={c.successText}>{aviso}</Texto>}
      <Botao rotulo="Salvar perfil" grande onPress={salvar} disabled={enviando} />
      <Botao rotulo="Apagar meus dados" variante="texto" onPress={apagar} accessibilityHint="Pede confirmação antes de apagar" />
    </Tela>
  );
}
