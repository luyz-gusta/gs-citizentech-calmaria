# Calmaria: o app

App em **Expo** (React Native, TypeScript, Expo Router) da Calmaria, a CitizenTech da equipe na Global Solution
2026.2 da FIAP. Disciplina: **Mobile Development e IoT**.

## Ficha da CitizenTech

| Campo | Resposta |
|---|---|
| Nome da CitizenTech | Calmaria |
| Cidade e bairro | São Paulo, Bela Vista e Consolação |
| Cidadão beneficiado | Pessoas autistas com hipersensibilidade sensorial e as famílias que planejam as saídas delas |
| Problema, em uma frase | Pessoas autistas não têm como saber, antes de sair, se um mercado ou uma estação vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem |
| Como funciona | Sensores calculam um índice de sobrecarga de cada lugar; o app mostra o índice, os horários mais calmos e a previsão de barulho da região, e tem um botão de crise que leva ao refúgio calmo mais próximo, mesmo offline |
| O que cada parte faz | O ESP32 mede; a API em C# guarda, calcula, recebe os relatos e protege o dado de saúde com o módulo de privacidade; o ML prevê o barulho com o SP156; o app mostra; a VM hospeda e monitora; o Azure Boards garante a qualidade |
| Link do repositório | (preencher) |
| Indicador de impacto | Tempo do toque no botão de crise até a chegada ao refúgio (meta: menos de 10 minutos a pé) e a parcela das visitas feitas em horário calmo |
| ODS conectados | 3 (meta 3.4), 10 (meta 10.2) e 11 (metas 11.2 e 11.7) |

## O que o app faz

A **jornada principal**: ver o nível dos lugares perto, escolher o horário ou o lugar mais calmo e, na crise,
chegar ao refúgio mais próximo.

1. **Boas-vindas**: o que o app faz em três frases. "Criar meu perfil" ou "Pular". Cria um usuário anônimo na
   API (sem nome, e-mail nem CPF) e guarda o id no celular.
2. **Perfil sensorial**: som, luz e multidão de 1 a 5, com uma frase em cada valor, e um contato de apoio.
   Tem "Apagar meus dados" (direito de exclusão da LGPD).
3. **Lugares perto**: lista (ordenada por distância) e mapa, com filtro por tipo. Cada nível aparece em cor,
   em texto, com letra e forma (C no círculo, M no quadrado, I no losango).
4. **Detalhe do lugar**: nível agora em texto grande; som, luz e movimento com uma frase; os 3 horários mais
   calmos; a grade da semana; a **previsão de barulho do distrito** (do notebook de ML, com dados reais do
   SP156); o resumo dos relatos; "Como chegar" e "Favoritar".
5. **Botão de crise "Preciso de calma"**: sempre no mesmo canto. Tela escura e lisa, o refúgio mais perto com
   distância e minutos a pé, respiração passo a passo (avança no toque) e "Ligar para [contato]".
   **Funciona sem internet**: usa os refúgios guardados no SQLite e calcula a distância no celular.
6. **Relatar**: lugar, nota com rótulo, gatilhos em botões grandes e comentário. Antes de enviar, a API diz o
   que será removido do comentário (CPF, telefone, e-mail).
7. **Meus lugares**: favoritos e o histórico de crises, que fica só no celular.

## Como rodar

Precisa do Node.js LTS e do app **Expo Go** no celular, na mesma rede Wi-Fi do computador.

```bash
cd mobile
npm install
cp .env.exemplo .env      # opcional: coloque a URL da API
npx expo start            # leia o QR code com o Expo Go
```

No `.env`, `EXPO_PUBLIC_API_URL=http://<IP do computador da API>:5080`. No celular, `localhost` não funciona.

**Sem a API:** deixe a variável vazia (ou a API fora do ar). Se a API não responder em 3 segundos, o app usa o
**mock de reserva** (`src/mocks/`) e mostra a faixa "Mostrando dados de exemplo". As leituras e os relatos
do mock são **simulados**; as previsões do mock são as **reais** geradas pelo notebook de ML
(`ml/saida/previsao_barulho.csv`).

**Testar o modo offline:** abra o app uma vez com internet (para guardar os refúgios), ligue o modo avião e
toque em "Preciso de calma".

Conferências de código: `npx tsc --noEmit` e `npx eslint src`.

## Como o código está organizado

```
mobile/
├── app.json                  Nome, ícones, abertura, permissão de localização
├── .env.exemplo
└── src/
    ├── app/                  As telas (Expo Router). No SDK 58 as rotas ficam em src/app/
    │   ├── _layout.tsx       Stack, fontes, banco, atualiza os refúgios; o botão de crise flutua por cima
    │   ├── index.tsx         Boas-vindas
    │   ├── perfil.tsx        Perfil sensorial
    │   ├── (abas)/           mapa.tsx, relatar.tsx, meus-lugares.tsx
    │   ├── lugar/[id].tsx    Detalhe do lugar
    │   └── crise.tsx         Botão de crise
    ├── api/calmaria.ts       Uma função por rota da API; tempo limite de 3 s; cai no mock se falhar
    ├── mocks/                lugares.json, relatos.json, previsoes.json e as regras do mock
    ├── db/index.ts           SQLite: config, perfil, refugios_cache, favoritos, crises
    ├── components/           BotaoCrise, CartaoLugar, SeloNivel e FormaNivel, Mapa, Tela, base
    ├── lib/                  geo (Haversine, 80 m/min), localização, textos literais
    ├── theme/                tema.ts (copiado de design/codigo/) e o hook useTema
    └── types.ts              Tipos iguais aos exemplos do contrato da API
```

### Consumo de dados
`src/api/calmaria.ts` tem um cliente só, com `fetch`, e funções agrupadas como os módulos da API: lugares,
crise, previsões, usuários, relatos e privacidade. Erro de negócio (400, 404, 409) vem como `ProblemDetails` e
aparece na tela; falta de conexão, tempo esgotado ou erro 5xx fazem o app usar o mock.

### Banco de dados no celular (`expo-sqlite`)

| Tabela | Para quê |
|---|---|
| `config` | id do usuário anônimo, se as boas-vindas e o perfil já foram feitos |
| `perfil` | cópia local do perfil sensorial e do contato de apoio |
| `refugios_cache` | os refúgios de `GET /api/refugios`, atualizados a cada abertura: é o que faz o botão de crise funcionar offline |
| `favoritos` | lugares marcados |
| `crises` | histórico local: data, hora, refúgio sugerido, se chegou (nunca enviado) |

## Acessibilidade

- **Sem som, sem vibração, sem animação**: as trocas de tela são instantâneas (`animation: 'none'`).
- **Cor nunca sozinha**: nível em cor, texto, letra e forma.
- **Contraste AA** conferido por script nos dois temas (`design/tokens/`); fonte Atkinson Hyperlegible, corpo
  de 18; o tamanho de fonte do sistema é respeitado.
- **Alvos de toque** de 48 pontos ou mais; o botão de crise tem 64 de altura e fica sempre no mesmo canto.
- **Leitor de tela**: `accessibilityLabel`, `accessibilityRole` e `accessibilityHint` nos botões; o mapa tem a
  alternativa em lista.
- **Linguagem literal**: "Calmo agora", "Você está seguro. Vamos até um lugar calmo."
- **Tema calmo**: fundo `#F1F5F7` (nunca branco puro) e modo escuro automático.

## Aderência ao problema

| Escolha | Frase da entrevista que justificou |
|---|---|
| Botão de crise grande e fixo | (preencher com a frase da entrevista) |
| Sem som e sem animação | (preencher) |
| Modo offline | (preencher) |
| Perfil anônimo | (preencher) |

## Prints

Prints do aparelho, sem moldura (peça AP-15), em `mobile/prints/`: boas-vindas, perfil, lista, mapa, detalhe,
crise com internet, crise sem internet, relatar com o aviso de dado removido, meus lugares e uma tela com o
TalkBack ou o VoiceOver ligado. (adicionar)

## Vídeo

Até 3 minutos, com o app rodando: (link)

## Integrantes

(preencher)
