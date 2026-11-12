# Mobile Development e IoT: o app Calmaria

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Mobile Development e IoT |
| Responsável | Integrante 2 |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `mobile/` |
| Você depende de | Luiz (a API; até ela ficar pronta, o app usa o mock com os exemplos da Parte 3) |
| Dependem de você | Integrante 4 (test cases e prints do app para o PDF); Integrante 5 (o trecho do app no pitch) |
| Vídeo | Sim: até 3 minutos, com o app rodando |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: desenvolvedor sênior de React Native com Expo, TypeScript e Expo Router, com cuidado especial com acessibilidade.

Vamos construir o app Calmaria em Expo. Comece com o app inteiro navegando e funcionando com dados mockados (os exemplos de resposta da API estão na Parte 3 do briefing); depois ligamos na API real trocando só a URL.

Regras:
1. Siga o briefing à risca: a stack, os contratos com as outras partes (nomes de campos, rotas, tópicos,
   colunas, formatos) e os critérios de avaliação. Não mude nome de campo, rota nem formato: outras pessoas
   da equipe dependem deles.
2. Antes de produzir qualquer coisa, me devolva em até 10 linhas o que entendeu da minha parte e um plano em
   etapas, na ordem do "Passo a passo" do briefing. Espere o meu ok.
3. Depois, uma etapa por vez: o conteúdo completo de cada arquivo, onde ele fica na pasta e como eu testo que
   funcionou. Não pule para a próxima etapa sem eu confirmar que a anterior funcionou.
4. Explique as decisões em linguagem simples: eu vou apresentar e responder perguntas do professor.
5. Se algo do briefing estiver ambíguo ou faltar informação, pergunte em vez de inventar.
6. Nada de dado pessoal real: use os dados fictícios ou públicos indicados no briefing.
7. Ao fim de cada etapa, diga quais itens do checklist de entrega ela cumpre e o que falta.
8. Sem emoji, no código e nos textos.

Comece pelo resumo e pelo plano.
```

O briefing tem quatro partes: **Parte 1**, o projeto; **Parte 2**, a sua parte; **Parte 3**, os contratos com
as outras partes; **Parte 4**, com quem falar. O checklist de entrega fecha a Parte 2.

---

# Parte 1: o projeto

## O projeto em poucos minutos

### A Global Solution
A Global Solution (GS) 2026.2 da FIAP, Engenharia de Software, 3º ano, tem o tema **CitizenTech**: tecnologia
criada a partir de um problema real de um cidadão. A equipe escolhe **um** problema real de uma cidade e
constrói **uma** solução que atravessa as oito disciplinas do semestre; cada disciplina avalia a sua parte, de
0 a 10. **Entrega de todas: 19/11/2026 (quinta), até 23h55, na tarefa de cada disciplina no Teams,** enviada
por um só integrante. 20/11 é feriado. De 16 a 19/11 é a semana de orientação: o professor tira dúvida, mas
não corrige.

### A Calmaria
**O problema.** Para muitas pessoas autistas, barulho alto, luz forte e multidão são gatilho de sobrecarga
sensorial, que pode virar crise. Quem convive com isso planeja cada saída, e nenhum app de mapa diz se um lugar
é barulhento, se a luz é forte ou onde fica o lugar calmo mais perto.

**Em uma frase:** pessoas autistas em São Paulo não têm como saber, antes de sair, se um mercado ou uma estação
vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem.

**Os números.**
- 2,4 milhões de brasileiros têm diagnóstico de autismo, 1,2% da população (Censo 2022 do IBGE, divulgado em
  23/05/2025).
- No 3º trimestre de 2026, o SP156 (canal de reclamações da Prefeitura) recebeu 14.178 reclamações de
  "Poluição Sonora - PSIU", todas com coordenada. Sábado e domingo têm o dobro dos dias úteis. A Consolação,
  vizinha da FIAP Paulista, é o 2º distrito com mais reclamações. Fonte: Portal de Dados Abertos da Prefeitura
  de São Paulo, conjunto "Dados do SP156 - Solicitações".
- A Lei 12.764/2012 trata a pessoa com autismo como pessoa com deficiência para todos os efeitos legais.

**Para quem.** A pessoa autista com hipersensibilidade sensorial que circula pela cidade, e o responsável por uma
criança autista. Região do protótipo: Bela Vista e Consolação, em São Paulo (Av. Paulista e Rua Augusta).
Personas de trabalho (fictícias): **Marina, 24 anos**, autista, trabalha na Paulista, usa a Linha 2 do metrô e
precisa de um lugar quieto em menos de 10 minutos quando a sobrecarga vem; **Carla, 41 anos**, mãe do Theo, 8
anos, autista, que liga para o mercado antes de sair para saber se está cheio.

**A solução.** Sensores medem som, luz e movimento de pessoas em mercados e estações e calculam um **Índice de
Sobrecarga Sensorial (ISS)**, de 0 a 100. O app mostra o índice no mapa, os **horários mais calmos** de cada
lugar, a **previsão de barulho** do distrito para os próximos dias (feita com dados reais do SP156) e tem um
**botão de crise** que leva ao refúgio calmo mais próximo, mesmo sem internet. A comunidade completa o mapa com
**relatos** de como foi a visita.

**A jornada principal do cidadão.**
1. Abre o app e, se quiser, informa o perfil sensorial (sensibilidade a som, luz e multidão, de 1 a 5) e um
   contato de apoio. Sem nome, e-mail nem CPF.
2. Vê os lugares perto, cada um com o nível atual em cor **e** em texto: Calmo, Moderado ou Intenso.
3. Abre um lugar: índice agora, os 3 horários mais calmos da semana, a previsão de barulho do distrito e os
   relatos.
4. Vai no horário calmo ou a um lugar alternativo, com o trajeto aberto no app de mapas do celular.
5. Na sobrecarga, toca no **botão de crise** (sempre visível): tela simples e escura, o refúgio mais próximo com
   distância, um roteiro de respiração em texto e um botão para ligar para o contato de apoio. Funciona offline.
6. Depois, se quiser, relata como foi.

**Princípios que valem para todas as partes.**
- Nada de som, vibração forte, flash nem animação. Um produto para quem sofre com estímulo não pode ser mais um
  estímulo. O ESP32 não tem buzzer, de propósito.
- Cor nunca sozinha: todo nível aparece em cor, em texto e com a letra e a forma (C no círculo, M no
  quadrado, I no losango).
- Cores da marca: ardósia `#3D5A73`, fundo `#F1F5F7` (nunca branco puro), texto `#14191F`, crise `#1F2733`;
  níveis Calmo `#437E61`, Moderado `#BBA15E` e Intenso `#B2574D`. Fonte Atkinson Hyperlegible. O detalhe
  está na seção "Design da sua parte".
- Linguagem literal e curta, sem metáfora nem ironia.
- Previsível: a mesma navegação em todas as telas; o botão de crise sempre no mesmo lugar.
- Privacidade por padrão: sem login; dado de saúde nunca vai para log nem para o broker MQTT público.
- Funciona com conexão ruim: o botão de crise não depende da internet.

### A Ficha da CitizenTech
Vai no topo do README da raiz e no README do app.

| Campo | Resposta |
|---|---|
| Nome da CitizenTech | Calmaria (provisório) |
| Cidade e bairro | São Paulo, Bela Vista e Consolação |
| Cidadão beneficiado | Pessoas autistas com hipersensibilidade sensorial e as famílias que planejam as saídas delas |
| Problema, em uma frase | Pessoas autistas não têm como saber, antes de sair, se um mercado ou uma estação vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem |
| Como funciona | Sensores calculam um índice de sobrecarga de cada lugar; o app mostra o índice, os horários mais calmos e a previsão de barulho da região, e tem um botão de crise que leva ao refúgio calmo mais próximo, mesmo offline |
| O que cada parte faz | O ESP32 mede; a API em C# guarda, calcula, recebe os relatos e protege o dado de saúde com o módulo de privacidade; o ML prevê o barulho com o SP156; o app mostra; a VM hospeda e monitora; o Azure Boards garante a qualidade |
| Link do repositório | (preencher) |
| Indicador de impacto | Tempo do toque no botão de crise até a chegada ao refúgio (meta: menos de 10 minutos a pé) e a parcela das visitas feitas em horário calmo |
| ODS conectados | 3 (meta 3.4, saúde mental e bem-estar), 10 (meta 10.2, inclusão independentemente de deficiência) e 11 (metas 11.2 e 11.7, transporte e espaços públicos acessíveis) |

## As partes e quem faz cada uma

| Parte | Disciplina | Responsável | O que faz |
|---|---|---|---|
| API Calmaria (C#) | C# Software Development, SOA e Web Services, Cybersecurity | Luiz | O núcleo: lugares, leituras do sensor, índice, horários calmos, botão de crise, previsões, perfil anônimo, relatos e o módulo de privacidade (dado sensível, taint analysis, Risco Cidadão) |
| App Expo | Mobile Development e IoT | Integrante 2 | A porta de entrada do cidadão |
| Notebook de ML | IA e Machine Learning | Integrante 3 | Prevê o nível de barulho por distrito e dia com o SP156 real |
| VM Linux | Operating Systems | Integrante 4 | Sobe a API e o PostgreSQL sozinha, monitora, audita e aguenta falha |
| Sensor ESP32 | Physical Computing | Integrante 5 | Mede som, luz e movimento, calcula o ISS e publica por MQTT |
| Qualidade | Testing, Compliance e QA | Integrante 4 | Features e test cases no Azure Boards e o PDF; envia o link do pitch |
| Pitch | Vai na entrega de Testing | Integrante 5 | Monta, edita e publica o vídeo de até 4 minutos |

Troque "Integrante N" pelo nome de cada um quando a equipe confirmar.

### Como as partes conversam

```
ESP32 (Wokwi) --MQTT--> broker.hivemq.com --> API Calmaria (C#) <--REST-- App Expo
                                                  |   ^
                                    PostgreSQL ---+   +--- previsao_barulho.csv (notebook de ML)
                                                  |
                                     LLM externo (REST, só texto mascarado)

VM Linux (Operating Systems): instala, sobe e vigia a API e o PostgreSQL
Azure Boards (Testing): features e test cases da solução inteira
```

## Datas da equipe

| Dia | O que fica pronto |
|---|---|
| Qui 12/11 | Divisão fechada; repositório e projeto do Azure DevOps criados; entrevista marcada; todos leram o seu arquivo |
| Sex 13/11 | Dúvidas levadas aos professores, na aula |
| Sáb 14 e Dom 15 | Observação do lugar e entrevista; cada parte com o básico funcionando sozinha (com mock quando depender de outra) |
| Seg 16 e Ter 17 | Integração entre as partes; VM provisionando a API; test cases |
| Qua 18 | Funcionalidades congeladas às 20h; vídeos gravados |
| Qui 19 | Pitch gravado; READMEs revisados; **todos os envios até 21h**, com 3 horas de margem |

## Regras do briefing que valem para todos
- **IA como copiloto, com registro:** todo prompt relevante vai para o `prompts.md` da raiz do repositório
  (data, quem, parte, ferramenta, prompt e o que foi aproveitado). **Cada um sabe explicar cada linha que
  entregou** e o projeto inteiro, não só a sua parte.
- **Dados com cuidado:** nada de dado pessoal real. Usuários e relatos de teste são inventados e marcados como
  fictícios; as leituras dos sensores são simuladas e o README diz isso; o ML usa o SP156 real, que não
  identifica quem reclamou.
- **Ficha da CitizenTech** no README (nome, cidade e bairro, cidadão, problema em uma frase, como funciona, o
  que cada parte faz, link do repositório, indicador de impacto, ODS 3, 10 e 11).
- **Simples primeiro:** o básico funcionando antes de qualquer extra.

## O repositório (um só, uma pasta por parte)

```
calmaria/
├── README.md        Ficha da CitizenTech, mapa da rubrica, integrantes
├── prompts.md       Os prompts de IA usados por todos
├── docs/            Escopo, os arquivos de cada disciplina, observação e entrevista
├── api-dotnet/      API Calmaria (Luiz)
├── mobile/          App Expo (Integrante 2)
├── ml/              Notebook de ML (Integrante 3)
├── infra/           Scripts da VM Linux (Integrante 4)
├── iot/             ESP32 no Wokwi (Integrante 5)
├── qa/              PDF do Azure Boards e prints (Integrante 4)
└── security/        Evidências de Cybersecurity (Luiz)
```

---

# Parte 2: a sua parte

## Antes de começar
- Node.js LTS e o Git instalados; o app **Expo Go** no celular (Android ou iOS), na mesma rede Wi-Fi do
  computador.
- Criar o projeto: `npx create-expo-app@latest mobile` (modelo padrão, que já vem com TypeScript e Expo
  Router).
- Dependências: `npx expo install expo-sqlite expo-location react-native-maps expo-linking`.
- Arquivo `.env` na pasta `mobile/` com `EXPO_PUBLIC_API_URL=http://<IP do computador do Luiz>:5080` (pedir o IP
  a ele; até lá, deixar vazio para o app usar o mock).

## O que a disciplina pede
O app é a porta de entrada da CitizenTech: é por ele que o cidadão consulta, registra e recebe informação.
Um app funcional em **Expo** (React Native com TypeScript, Expo Router) que resolva a **jornada principal do
cidadão definida na Ficha**, consuma dados de uma API ou de um mock, salve informações em banco de dados e
seja simples e acessível para quem não é especialista.

**Entregáveis:** repositório com o código organizado; README com a Ficha da CitizenTech, as instruções para
rodar e prints das telas; link do vídeo de até 3 minutos com o app rodando. **Envio:** link do repositório no
Teams até 19/11, 23h55.

**Critérios (6, nota de 0 a 10):** telas e navegação, consumo de dados, banco de dados, acessibilidade,
aderência ao problema e documentação. **A rubrica completa está na tarefa do Teams: ler e ajustar este
arquivo.**

## O papel desta parte na Calmaria
É onde todas as outras partes aparecem para o cidadão: o índice que o ESP32 mede, os horários calmos e o
refúgio que a API calcula, a previsão do ML, os relatos da comunidade e o aviso de dado pessoal removido. **A jornada principal** (Parte 1): ver o nível dos lugares perto, escolher o horário ou o lugar mais calmo e, na crise, chegar ao
refúgio mais próximo.

## Telas (Expo Router)

```
mobile/app/
├── _layout.tsx                 Stack + abas; o botão de crise flutua em todas as telas
├── index.tsx                   Primeira abertura: explica o app em 3 frases e pede o perfil (pode pular)
├── perfil.tsx                  Perfil sensorial
├── (abas)/
│   ├── mapa.tsx                Mapa e lista de lugares perto
│   ├── relatar.tsx             Relatar uma visita
│   └── meus-lugares.tsx        Favoritos e histórico
├── lugar/[id].tsx              Detalhe do lugar
└── crise.tsx                   Botão de crise
```

| Tela | O que tem | De onde vem o dado |
|---|---|---|
| Boas-vindas | O que o app faz, em linguagem literal; "Criar meu perfil" ou "Pular" | Cria o usuário anônimo na API (`POST /api/usuarios`) e guarda o id no SQLite |
| Perfil sensorial | Três seletores de 1 a 5 (som, luz, multidão) com texto em cada valor ("1: quase não me incomoda"); contato de apoio (nome e telefone) | SQLite e API (`PUT .../perfil-sensorial`) |
| Mapa | `react-native-maps` com um marcador por lugar, cor **e** letra (C, M, I); alternância para **lista**, ordenada por distância, com o nível escrito; filtro por tipo | API (`GET /api/lugares?lat=&lng=`) |
| Detalhe do lugar | Nível agora em texto grande; som, luz e movimento em números com uma frase ("Som alto, como uma rua movimentada"); os 3 horários mais calmos; grade simples da semana; previsão de barulho do distrito para os próximos dias; resumo dos relatos; botões "Como chegar" e "Favoritar" | API (lugar, horários calmos, previsões, resumo e relatos) |
| Botão de crise | Fundo escuro e liso; "Você está seguro. Vamos até um lugar calmo."; o refúgio mais perto com distância e minutos a pé; "Abrir trajeto"; roteiro de respiração em texto, passo a passo, que a pessoa avança no toque; "Ligar para [contato de apoio]" | API (`GET /api/crise/refugio-mais-proximo`); **sem internet:** os refúgios guardados no SQLite e a distância calculada no celular |
| Relatar | Lugar (escolhido da lista), nota de 1 a 5 com rótulo, gatilhos em botões grandes (barulho, luz forte, multidão, cheiro, fila, outro), comentário opcional | API: antes de enviar, `POST /api/privacidade/analisar` avisa o que será removido do comentário; depois, `POST /api/relatos` |
| Meus lugares | Favoritos; histórico das crises com data, hora e refúgio (só no celular, nunca enviado) | SQLite |

## Consumo de dados
- Um cliente só, em `src/api/calmaria.ts`, com `fetch`, tipos TypeScript iguais aos exemplos da Parte 3 e uma
  função por rota, agrupadas como os módulos da API (lugares, crise, previsões, usuários, relatos,
  privacidade).
- URL em variável de ambiente do Expo: `EXPO_PUBLIC_API_URL`. No celular físico, `localhost` não funciona:
  usar o IP do computador na rede ou o da VM.
- **Mock de reserva** em `src/mocks/` com os 7 lugares do seed: se a API não responder em 3 segundos, o app
  usa o mock e mostra uma faixa discreta "Mostrando dados de exemplo". Garante o vídeo mesmo se a API cair, e
  conta como "consome dados de uma API ou de um mock".
- Estados de carregando, vazio ("Ainda não temos medições deste lugar") e erro em todas as telas.

## Banco de dados no celular
`expo-sqlite`, com as tabelas:

| Tabela | Para quê |
|---|---|
| `config` | id do usuário anônimo, se o perfil foi preenchido |
| `perfil` | cópia local do perfil sensorial e do contato de apoio |
| `refugios_cache` | os refúgios de `GET /api/refugios`, atualizados a cada abertura: é o que faz o botão de crise funcionar offline |
| `favoritos` | lugares marcados |
| `crises` | histórico local: data, hora, refúgio sugerido, se chegou |

Mostrar no vídeo: desligar a internet do celular, tocar no botão de crise e o refúgio aparecer mesmo assim.

## Acessibilidade (critério da disciplina e coração do problema)
- **Sem som, sem vibração forte, sem animação.** Respeitar a configuração de reduzir movimento do sistema
  (`AccessibilityInfo.isReduceMotionEnabled`).
- **Cor nunca sozinha:** nível sempre em texto também, com a letra e a forma (C, M, I).
- **Contraste AA** (4,5:1 no texto) e fonte de pelo menos 16; respeitar o tamanho de fonte do sistema.
- **Alvos de toque** de pelo menos 48 por 48 pontos; o botão de crise bem maior, sempre no mesmo canto.
- **Leitor de tela:** `accessibilityLabel`, `accessibilityRole` e `accessibilityHint` em todo botão e ícone;
  o mapa tem a alternativa em lista, porque marcador de mapa é ruim no leitor de tela.
- **Linguagem literal:** "Está barulhento agora" em vez de "Clima agitado"; frases curtas; nada de ironia.
- **Previsibilidade:** as mesmas abas e o mesmo botão de crise em todas as telas; nenhuma tela muda sozinha.
- **Tema calmo:** cores de baixa saturação, sem branco puro no fundo, e modo escuro.
- Testar com o TalkBack (Android) ou o VoiceOver (iOS) e colocar o print no README.

## Aderência ao problema (o que mostrar ao professor)
Citar no README a frase da entrevista que justificou cada escolha: o botão de crise grande e fixo, a ausência
de som, o modo offline, o perfil anônimo.

## README de `mobile/`
1. A Ficha da CitizenTech (está na Parte 1).
2. O que o app faz e a jornada principal.
3. Como rodar: `npm install`, criar o `.env` com a URL da API, `npx expo start`; o que fazer sem a API (o
   mock entra sozinho).
4. Prints de todas as telas, incluindo o botão de crise offline e o leitor de tela.
5. Link do vídeo de até 3 minutos.
6. Integrantes.

## Roteiro do vídeo (até 3 minutos)
| Tempo | O que mostrar |
|---|---|
| 0:00 a 0:20 | Para quem é e o problema em uma frase |
| 0:20 a 0:50 | Boas-vindas e perfil sensorial |
| 0:50 a 1:40 | Mapa e lista; um lugar "Intenso" (o ESP32 ao lado mudando, se der); o detalhe com horários calmos e a previsão do sábado |
| 1:40 a 2:30 | Botão de crise: com internet, depois em modo avião |
| 2:30 a 3:00 | Relato enviado e o aviso do dado removido; acessibilidade com o leitor de tela |

## Ordem de construção
1. **Obrigatório:** projeto Expo com TypeScript e Expo Router, navegação de todas as telas com o mock.
2. **Obrigatório:** SQLite (config, perfil, favoritos, refúgios em cache).
3. **Obrigatório:** botão de crise funcionando offline.
4. **Obrigatório:** ligar na API: lugares, detalhe, horários, refúgio e previsões.
5. **Obrigatório:** ligar na API: usuário, perfil, relatos e o aviso de dado removido.
6. **Obrigatório:** passada de acessibilidade, README, prints e vídeo.
7. **Opcional:** "índice pessoal" (o ISS recalculado com os pesos do perfil da pessoa); notificação silenciosa
   quando um favorito ficar calmo.

## Detalhes de implementação

### Estrutura de pastas do código
```
mobile/
├── app/                      As telas (Expo Router), como na seção "Telas"
├── src/
│   ├── api/calmaria.ts       Uma função por rota da API; tempo limite de 3 s; cai no mock se falhar
│   ├── mocks/                lugares.json, refugios.json, horarios-calmos-4.json, previsoes.json, resumo-relatos-1.json
│   │                         (copiados dos exemplos da Parte 3)
│   ├── db/                   Abertura do SQLite, criação das tabelas e funções de leitura e escrita
│   ├── components/           BotaoCrise, CartaoLugar, SeloNivel (cor + texto), SeletorSensibilidade
│   ├── theme/                Cores, tamanhos de fonte e espaçamentos (os tokens abaixo)
│   └── types.ts              Os tipos TypeScript das respostas da API (iguais aos exemplos da Parte 3)
├── .env.exemplo
└── README.md
```

### Cores (tema calmo, contraste conferido)
Vêm de `design/codigo/tema.ts` (seção "Design da sua parte"). Os principais papéis:

| Uso | Claro | Escuro |
|---|---|---|
| Fundo | `#F1F5F7` (nunca branco puro) | `#14191F` |
| Cartão e campo | `#F8FBFD` | `#22282E` |
| Texto / apoio | `#14191F` / `#4D545B` | `#F8FBFD` / `#CED5DA` |
| Botão principal | `#3D5A73` com texto branco | `#8AA8BE` com texto `#06090E` |
| Calmo (C, círculo) | sólido `#437E61`, texto `#34684F`, fundo `#E5F0EA` | sólido `#7DAF95`, texto `#AACEBA`, fundo `#152B20` |
| Moderado (M, quadrado) | sólido `#BBA15E` com texto `#06090E`, texto `#855318`, fundo `#F0EDDF` | sólido `#BBA15E`, texto `#D2C596`, fundo `#3A210E` |
| Intenso (I, losango) | sólido `#B2574D`, texto `#95463E`, fundo `#FCE9E5` | sólido `#E08F82`, texto `#F3B7AC`, fundo `#3F1D1A` |
| Botão de crise flutuante | `#313D4D` com texto branco | `#D4E1EA` com texto `#1F2733` |
| Tela de crise (os dois temas) | fundo `#1F2733`, cartão `#313D4D`, texto `#F8FBFD`, botão `#B2C9D8` com texto `#1F2733` | igual |

Todas as combinações foram conferidas por script (WCAG 2.2 AA). Nada de vermelho vivo piscando.

### Textos da tela de crise (literais, já revisados)
- Título: "Você está seguro. Vamos até um lugar calmo."
- Cartão do refúgio: "[nome do lugar], a [distância] metros, cerca de [minutos] minutos a pé."
- Botão: "Abrir o caminho no mapa".
- Respiração, um passo por toque, com o botão "Próximo passo":
  1. "Pare onde você está, se for seguro."
  2. "Puxe o ar pelo nariz contando até 4."
  3. "Segure contando até 4."
  4. "Solte o ar pela boca contando até 6."
  5. "Repita mais 3 vezes. Não tem pressa."
- Botão de apoio: "Ligar para [nome do contato]" (abre a discagem com `Linking.openURL('tel:...')`).
- Sem internet: "Sem internet. Usando os refúgios salvos no celular."
- Sem localização: "Não consegui achar sua localização. Estes são os refúgios da região:" e a lista.

### Distância offline
Com os refúgios do SQLite e a posição do `expo-location`, calcular a distância com a fórmula de Haversine
(raio da Terra 6.371 km) e os minutos a pé com 80 metros por minuto, o mesmo critério da API.

### Como testar no celular
`npx expo start`, ler o QR code com o Expo Go. Para testar o modo offline: abrir o app uma vez com internet
(para guardar os refúgios), ligar o modo avião e tocar no botão de crise.


## Design da sua parte
O design da Calmaria foi fechado em 12/11. A fonte é `design/plano-de-design.md`; a versão visual, com as
telas, o visor, o painel e as cartelas desenhados, é `design/design-da-calmaria.html` (abrir no navegador);
cada cor em hex, nos dois temas, está em `design/tokens/resolvido.json`. As peças prontas (PNG, JPG e o PDF da capa) estão em `design/pecas/png/`, explicadas em
`design/pecas/LEIAME.md`; para refazê-las com os nomes e os links certos, `python design/pecas/exportar.py --help`. As cores foram conferidas por script
(WCAG 2.2 AA, 86 pares por tema). **Não invente cor, fonte ou tamanho fora destes.**

Regras que valem para todas as peças:
- **Nível em três sinais, sempre juntos:** Calmo = verde `#437E61`, letra C, círculo; Moderado = ocre
  `#BBA15E` (texto escuro por cima), letra M, quadrado; Intenso = tijolo `#B2574D`, letra I, losango. As
  mesmas cores para BAIXO, MEDIO e ALTO da previsão e para as faixas do Risco Cidadão. Cor só aparece quando
  significa nível.
- **Marca** `#3D5A73` (ardósia). Fundo claro `#F1F5F7` (nunca branco puro), cartão `#F8FBFD`, texto
  `#14191F`, apoio `#4D545B`. Fundo escuro `#14191F`, cartão `#22282E`, texto `#F8FBFD`. Crise e vídeo:
  `#1F2733`.
- **Fonte:** Atkinson Hyperlegible Next para texto, Atkinson Hyperlegible Mono para números de medição e
  código (Google Fonts, licença OFL).
- **Quieto:** nada pisca nem anima, todo ícone vem com texto (ícones Lucide, traço 2), frases literais.

As peças desta entrega (o código serve para avisar no grupo: "AP-09 pronta"):

| Código | Peça | Quem faz | Tamanho e formato | Especificação |
|---|---|---|---|---|
| MA-02 | Logotipo | Pronto em `design/marca/logotipo.svg` e `logotipo-invertido.svg` | SVG; mínimo 96 px de largura | Símbolo e "calmaria" em minúsculas, Atkinson Hyperlegible Next 700. Converter o texto em contorno antes de exportar PNG. |
| AP-01 | Ícone do app | Integrante 2 | 1024 × 1024 PNG, sem transparência | Exportar `design/marca/icone-app.svg`. Fundo #3D5A73, símbolo #F1F5F7. `expo.icon`. |
| AP-02 | Ícone adaptativo | Integrante 2 | Frente 1024 × 1024 PNG transparente | Exportar `icone-frente.svg`; fundo #3D5A73 em `android.adaptiveIcon.backgroundColor`. |
| AP-03 | Ícone monocromático | Integrante 2 | 1024 × 1024 PNG, só alfa | Exportar `icone-mono.svg`; `android.adaptiveIcon.monochromeImage`. |
| AP-04 | Abertura (splash) | Integrante 2 | 1024 × 1024 PNG; `imageWidth` 160 | `abertura.svg` sobre #F1F5F7; no escuro, `abertura-escuro.svg` sobre #14191F. Sem animação. |
| AP-05 | Boas-vindas | Integrante 2 | 390 × 844 pt; conferir em 360 × 800 | Três frases literais; "Criar meu perfil" (primário) e "Pular". |
| AP-06 | Perfil sensorial | Integrante 2 | 390 × 844 pt | Som, luz e multidão de 1 a 5, com a frase de cada valor; contato de apoio. |
| AP-07 | Lugares perto | Integrante 2 | 390 × 844 pt | Mapa e Lista; filtro por tipo; cartão com selo do nível e distância. |
| AP-08 | Detalhe do lugar | Integrante 2 | 390 × 844 pt | Nível em 32; medidas com frase; horários calmos; previsão do distrito; Como chegar. |
| AP-09 | Crise | Integrante 2 | 390 × 844 pt | Fundo #1F2733 nos dois temas; botões de 64; respiração passo a passo; versão sem internet. |
| AP-10 | Relatar | Integrante 2 | 390 × 844 pt | Lugar, nota com rótulo, gatilhos, comentário e o aviso do dado que será removido. |
| AP-11 | Meus lugares | Integrante 2 | 390 × 844 pt | Favoritos com selo; histórico de crises dizendo que fica só no celular. |
| AP-12 | Marcador do mapa | Integrante 2 | 40 pt visível, alvo de 48 pt | Forma, letra e cor; contorno de 2 pt; selecionado ganha anel de 4 pt na cor da marca. |
| AP-13 | Selo de nível | Integrante 2 e Luiz | 32 pt de altura, pílula | Forma de 16 e texto de 16 ("Calmo agora"). O mesmo selo no painel. |
| AP-14 | Botão de crise | Integrante 2 | 64 pt de altura, 176 de largura mínima | Canto inferior direito, 16 da borda e 16 acima das abas; ícone `life-buoy` e "Preciso de calma". |
| AP-15 | Prints do README | Integrante 2 | PNG do aparelho, sem moldura | Todas as telas, a crise sem internet e uma com o leitor de tela ligado. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |

**Código pronto:** copie `design/codigo/tema.ts` para `mobile/src/theme/`. Ele tem todas as cores dos dois
temas (`cores.claro` e `cores.escuro`, com os papéis `nivel.*`, `crise.*` e `botaoCrise.*`), a escala de texto
(`tipografia`, corpo de 18), os espaços, os raios e os tamanhos (`toqueMin` 48, `botaoCrise` 64). As SVGs do
ícone e da abertura estão em `design/marca/`.

## Checklist de envio
- [ ] App roda com `npx expo start` a partir do README
- [ ] Jornada principal completa: mapa, detalhe, horário calmo, botão de crise
- [ ] Consome a API (e cai no mock sem ela)
- [ ] Salva no SQLite
- [ ] Acessibilidade conferida com leitor de tela
- [ ] README com a Ficha, instruções e prints
- [ ] Vídeo de até 3 minutos
- [ ] Link do repositório enviado no Teams por um integrante

---

# Parte 3: contratos com as outras partes

Estes formatos são combinados entre as partes. **Não mude nome de campo, rota, tópico nem coluna sem avisar a equipe.**

### Contrato: o Índice de Sobrecarga Sensorial (ISS)
O mesmo cálculo roda no ESP32 (para o display do aparelho) e na API (o valor que vale).

```
som_norm = limita((somDb  - 40)  / (100  - 40),  0, 1)
luz_norm = limita((luzLux - 300) / (3000 - 300), 0, 1)
mov_norm = limita(movimentosMin / 30, 0, 1)
ISS      = arredonda(100 * (0,5 * som_norm + 0,25 * luz_norm + 0,25 * mov_norm))
```
`limita(x, 0, 1)` prende o valor entre 0 e 1. O som pesa o dobro porque é o gatilho mais citado.

| ISS | `nivel` | Cor | Texto para o cidadão |
|---|---|---|---|
| 0 a 39 | `CALMO` | verde | "Calmo agora" |
| 40 a 69 | `MODERADO` | amarelo | "Moderado agora" |
| 70 a 100 | `INTENSO` | vermelho | "Intenso agora" |

Exemplo: 78,5 dB, 1.450 lux e 12 movimentos por minuto dão som 0,642, luz 0,426 e movimento 0,4, e ISS
`arredonda(100 * (0,321 + 0,106 + 0,1)) = 53`, nível `MODERADO`.

### Contrato: a API Calmaria (REST)

**Endereço.** Em desenvolvimento: `http://<IP do computador do Luiz na rede>:5080` (no celular, `localhost`
não funciona). Na VM: `http://api.calmaria.local` ou `http://<IP da VM>`. Documentação interativa em
`/scalar`. Enquanto a API não estiver no ar, use os exemplos abaixo como mock: eles são o formato real.

**Convenções.**
- JSON com nomes em camelCase; datas em ISO 8601 com fuso (`2026-11-16T18:05:00-03:00`); datas sem hora como
  `2026-11-21`.
- Valores fixos em maiúsculas: tipo do lugar (`MERCADO`, `ESTACAO`, `PARQUE`, `REFUGIO`, `OUTRO`); nível do
  ISS (`CALMO`, `MODERADO`, `INTENSO`); nível da previsão (`BAIXO`, `MEDIO`, `ALTO`); gatilho (`BARULHO`,
  `LUZ_FORTE`, `MULTIDAO`, `CHEIRO`, `FILA`, `OUTRO`); dia da semana (`DOMINGO` a `SABADO`).
- **Erro** sempre no formato `ProblemDetails` (`application/problem+json`):

```json
{ "title": "Dados inválidos", "status": 400, "detail": "A nota deve estar entre 1 e 5.", "errors": { "nota": ["A nota deve estar entre 1 e 5."] } }
```

#### Lugares e leituras

**`GET /api/lugares?lat=-23.5614&lng=-46.6559&raioM=1500&tipo=MERCADO`**: lugares perto, do mais próximo ao mais
longe. Todos os parâmetros são opcionais; sem `lat` e `lng`, devolve todos e `distanciaM` vem `null`.
Coordenada inválida: 400.

```json
[
  {
    "id": 1,
    "nome": "Estação Consolação (Linha 2)",
    "tipo": "ESTACAO",
    "latitude": -23.5575,
    "longitude": -46.6609,
    "distrito": "CONSOLACAO",
    "ehRefugio": false,
    "distanciaM": 640,
    "indiceAtual": 53,
    "nivelAtual": "MODERADO",
    "ultimaLeituraEm": "2026-11-16T18:05:00-03:00"
  }
]
```
Lugar sem leitura nos últimos 15 minutos: `indiceAtual`, `nivelAtual` e `ultimaLeituraEm` vêm `null` (refúgios
fixos sempre vêm assim).

**`GET /api/lugares/{id}`**: um lugar com a última leitura. Não existe: 404.

```json
{
  "id": 1,
  "nome": "Estação Consolação (Linha 2)",
  "tipo": "ESTACAO",
  "endereco": "Av. Paulista com Rua da Consolação",
  "latitude": -23.5575,
  "longitude": -46.6609,
  "distrito": "CONSOLACAO",
  "ehRefugio": false,
  "descricao": null,
  "horarioFuncionamento": "04:40-00:00",
  "ultimaLeitura": {
    "somDb": 78.5, "luzLux": 1450, "movimentosMin": 12,
    "indice": 53, "nivel": "MODERADO", "registradaEm": "2026-11-16T18:05:00-03:00"
  }
}
```

**`GET /api/lugares/{id}/leituras?desde=2026-11-16T00:00:00-03:00`**: histórico, da mais nova para a mais
velha, no formato de `ultimaLeitura`.

**`POST /api/leituras`**: ingestão por HTTP, o mesmo caminho do MQTT (usado em teste). Corpo igual ao payload
MQTT. 201; dispositivo desconhecido: 404; valor fora da faixa: 422.

**`GET /api/lugares/{id}/horarios-calmos`**: as 3 janelas mais calmas da semana (dentro do horário de
funcionamento) e a grade de médias. Menos de 20 leituras: `"suficiente": false` e listas vazias.

```json
{
  "lugarId": 4,
  "suficiente": true,
  "janelasMaisCalmas": [
    { "diaSemana": "TERCA", "hora": 9, "indiceMedio": 22 },
    { "diaSemana": "QUARTA", "hora": 8, "indiceMedio": 24 },
    { "diaSemana": "TERCA", "hora": 14, "indiceMedio": 27 }
  ],
  "grade": [ { "diaSemana": "SEGUNDA", "hora": 8, "indiceMedio": 35 } ]
}
```

**`POST /api/lugares`, `PUT /api/lugares/{id}`, `DELETE /api/lugares/{id}`**: cadastro (uso da equipe).

#### Botão de crise

**`GET /api/crise/refugio-mais-proximo?lat=-23.5614&lng=-46.6559`**: o refúgio mais perto, que pode ser um
refúgio fixo ou um lugar `CALMO` agora.

```json
{
  "lugar": {
    "id": 5, "nome": "Parque Trianon", "tipo": "REFUGIO",
    "latitude": -23.5617, "longitude": -46.6573,
    "descricao": "Bancos na sombra e pouco barulho"
  },
  "distanciaM": 210,
  "minutosAPe": 3,
  "linkTrajeto": "https://www.google.com/maps/dir/?api=1&destination=-23.5617,-46.6573&travelmode=walking"
}
```

**`GET /api/refugios`**: todos os refúgios, no formato de `lugar` acima. O app guarda essa lista para o botão
de crise funcionar sem internet.

#### Previsão de barulho (vem do ML)

**`GET /api/previsoes/barulho?distrito=CONSOLACAO&dias=7`**: a previsão dos próximos 7 dias a partir de hoje
(ou `&data=2026-11-21` para um dia só). Distrito sem previsão: 404.

```json
[
  { "distrito": "CONSOLACAO", "data": "2026-11-21", "reclamacoesPrevistas": 9.4, "nivel": "ALTO" },
  { "distrito": "CONSOLACAO", "data": "2026-11-24", "reclamacoesPrevistas": 4.1, "nivel": "MEDIO" }
]
```

**`POST /api/previsoes/importar`** (`multipart/form-data`, campo `arquivo`, exige `X-Admin-Key`): recebe o CSV
do notebook de ML e substitui as previsões do mesmo distrito e dia. Responde `{ "importadas": 56 }`.

#### Usuário anônimo e perfil sensorial

**`POST /api/usuarios`** (corpo vazio ou `{ "apelido": "Ma" }`): 201 com `{ "id": "3f2b7c1e-8a4d-4c55-9e0b-1d2a6b7c8e90" }`.

**`GET /api/usuarios/{id}/perfil-sensorial`**: 200 ou 404. **`PUT`** com o mesmo corpo grava; sensibilidade fora
de 1 a 5: 400.

```json
{
  "sensibilidadeSom": 5,
  "sensibilidadeLuz": 3,
  "sensibilidadeMultidao": 4,
  "contatoApoioNome": "Mãe",
  "contatoApoioTelefone": "11900000000"
}
```

**`DELETE /api/usuarios/{id}`**: 204; apaga perfil e relatos (direito de exclusão da LGPD).

#### Relatos

**`POST /api/relatos`**:

```json
{
  "usuarioId": "3f2b7c1e-8a4d-4c55-9e0b-1d2a6b7c8e90",
  "lugarId": 1,
  "nota": 4,
  "gatilhos": ["BARULHO", "MULTIDAO"],
  "comentario": "Muito cheio às 18h",
  "visitadoEm": "2026-11-16T18:00:00-03:00"
}
```
Resposta 201. `removidos` lista o que o filtro de privacidade tirou do comentário (vazio se nada):

```json
{ "id": 31, "comentario": "Muito cheio às 18h", "removidos": [] }
```
Erros: nota fora de 1 a 5, gatilho desconhecido ou `visitadoEm` no futuro: 400; usuário ou lugar inexistente:
404; mesmo usuário e mesmo lugar há menos de 30 minutos: 409.

**`GET /api/relatos?lugarId=1&pagina=1&tamanho=10`**: relatos públicos, **sem o autor**.

```json
{
  "pagina": 1, "tamanho": 10, "total": 31,
  "itens": [ { "id": 31, "nota": 4, "gatilhos": ["BARULHO", "MULTIDAO"], "comentario": "Muito cheio às 18h", "visitadoEm": "2026-11-16T18:00:00-03:00" } ]
}
```

**`GET /api/lugares/{id}/resumo-relatos`**:

```json
{ "lugarId": 1, "totalRelatos": 31, "notaMedia": 3.6, "gatilhosMaisCitados": [ { "gatilho": "BARULHO", "vezes": 18 }, { "gatilho": "MULTIDAO", "vezes": 12 } ] }
```

#### Privacidade

**`POST /api/privacidade/analisar`** com `{ "texto": "Meu CPF é 529.982.247-25" }`: o app usa antes de enviar
um relato, para avisar o que será removido.

```json
{ "temDadoPessoal": true, "achados": [ { "categoria": "CPF", "trecho": "529.***.***-25" } ], "textoSanitizado": "Meu CPF é [removido]" }
```

Rotas administrativas (exigem o cabeçalho `X-Admin-Key`; sem ele, 401): `POST /api/privacidade/varreduras`,
`GET /api/privacidade/risco-cidadao`, `GET /api/privacidade/achados`, `GET /api/privacidade/fluxos`. O painel
fica em `/seguranca/index.html`.

#### Saúde

**`GET /health`**: `200` com `Healthy` quando o banco e o worker MQTT estão bem; `503` com `Unhealthy` quando o
banco cai.

### Contrato: os lugares do protótipo
Lugares reais da região; as **leituras são simuladas**. Coordenadas e distritos são aproximados: a equipe
confere no Google Maps e na observação antes do seed final.

| id | Lugar | Tipo | Latitude | Longitude | Distrito provável | Dispositivo |
|---|---|---|---|---|---|---|
| 1 | Estação Consolação (Linha 2) | ESTACAO | -23.5575 | -46.6609 | CONSOLACAO | `esp32-consolacao-01` |
| 2 | Estação Paulista (Linha 4) | ESTACAO | -23.5553 | -46.6622 | CONSOLACAO | `esp32-paulista-01` |
| 3 | Estação Trianon-Masp (Linha 2) | ESTACAO | -23.5614 | -46.6566 | BELA VISTA | `esp32-trianon-01` |
| 4 | Supermercado da Rua Augusta (escolhido na observação) | MERCADO | -23.5560 | -46.6580 | CONSOLACAO | `esp32-mercado-01` |
| 5 | Parque Trianon | REFUGIO | -23.5617 | -46.6573 | BELA VISTA | (sem sensor) |
| 6 | Casa das Rosas | REFUGIO | -23.5712 | -46.6446 | BELA VISTA | (sem sensor) |
| 7 | Refúgio escolhido na observação (biblioteca, igreja, sala silenciosa) | REFUGIO | (preencher) | (preencher) | (preencher) | (sem sensor) |

Ponto de referência para testes ("estou aqui"): `-23.5614, -46.6559` (Av. Paulista, em frente ao MASP).

---

# Parte 4: com quem falar

- **Luiz (API):** pedir o IP e a porta da API; avisar se algum exemplo da Parte 3 não bater com a resposta real.
- **Integrante 4 (VM e Testing):** mandar os prints de todas as telas; no fim, testar o app apontando para o IP
  da VM.
- **Integrante 5 (pitch):** gravar juntos o trecho do app para o pitch.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
