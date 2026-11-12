# Testing, Compliance e QA: features e test cases no Azure Boards

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Testing, Compliance e Quality Assurance (o link do pitch também é enviado aqui) |
| Responsável | Integrante 4 (que também faz Operating Systems) |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `qa/` |
| Você depende de | Todos (prints para o PDF); Integrante 5 (o link do pitch, até quinta, 19/11, às 18h) |
| Dependem de você | Ninguém |
| Vídeo | Nenhum seu; o link do pitch, feito pelo Integrante 5, vai no envio desta disciplina |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: analista de QA sênior com Azure DevOps (Boards e Test Plans) e redação de requisitos.

Vamos cadastrar no Azure Boards as features da Calmaria com critérios de aceite e os test cases funcionais, positivos e negativos, e montar o PDF de entrega. Os textos já estão prontos na Parte 2: o trabalho é adaptá-los ao padrão ensinado em aula (eu te passo), cadastrar e documentar. O pitch é montado pelo Integrante 5; eu só ponho o link no PDF e no envio.

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
- Conta no Azure DevOps (https://dev.azure.com), com a conta Microsoft da FIAP.
- O e-mail do professor de Testing, para adicioná-lo como administrador.
- O padrão de feature, critério de aceite e test case visto em aula (slides ou exemplo do professor): passe
  para a sua IA junto com este arquivo.

## O que a disciplina pede
Planejar e qualificar o software: as **features** da aplicação, com descrição e **critérios de aceite**, nos
padrões ensinados em aula, no **Azure Boards**; e todos os **test cases funcionais, positivos e negativos,
caixa-preta**, necessários para a aceitação (homologação) da solução, cada um com objetivo, preparação,
entradas, saídas esperadas e passos.

**Formato:** incluir o professor como **Admin** no projeto do Azure DevOps e pôr o link do projeto no PDF.
Como contingência, prints de todas as telas com a lista de features, do detalhe de cada feature e de cada
test case, num único documento, exportado em PDF. **Avaliação:** features 50%, test cases 50%.
**Envio:** PDF no Teams até 19/11, 23h55, junto com o link do pitch.

> **Conferir o padrão da aula** (formato da história, do critério de aceite e dos campos do test case) e
> ajustar os textos abaixo a ele. Os textos estão prontos para colar; o formato é o que costuma ser cobrado.

## Montar o Azure DevOps (30 minutos)
1. Criar a organização e o projeto **Calmaria**, processo **Agile**.
2. Project settings, Permissions, grupo **Project Administrators**: adicionar o e-mail do professor.
3. Boards, Work items: criar as **Features** abaixo (tipo Feature), com descrição e critérios de aceite no
   campo "Acceptance Criteria".
4. Opcional, mas bem visto: um **Epic** "Calmaria: mapa sensorial" como pai das features, e as **User
   Stories** de cada feature como filhas.
5. Criar cada **Test Case** (tipo Test Case) com os passos no campo Steps (ação e resultado esperado por
   passo), o objetivo e a preparação na descrição, e ligar à feature pela relação **Tests**. Se o hub Test
   Plans não estiver liberado na conta, o Test Case pode ser criado direto pelo Boards.
6. Tirar os prints (lista de features, cada feature aberta, lista de test cases, cada test case aberto) e
   montar o PDF em `qa/`.

---

## Features

### F01: Mapa de lugares com nível sensorial
**Descrição:** como pessoa autista, quero ver no mapa os lugares perto de mim com o nível sensorial atual,
para decidir aonde ir sem me expor a um lugar intenso.
**Critérios de aceite:**
- Dado que permiti a localização, quando abro o mapa, então vejo os lugares num raio de 1,5 km com o nível
  em cor **e** em texto (Calmo, Moderado, Intenso).
- Dado que estou no mapa, quando escolho "Lista", então vejo os mesmos lugares ordenados por distância.
- Dado que não permiti a localização, quando abro o mapa, então vejo os lugares da região padrão e um aviso
  de que a distância não está disponível.
- Dado que a API não responde em 3 segundos, então o app mostra os dados de exemplo com a faixa "Mostrando
  dados de exemplo".

### F02: Detalhe do lugar com a leitura atual
**Descrição:** como pessoa autista, quero ver o som, a luz e o movimento de um lugar agora, para entender o
que pode me incomodar.
**Critérios de aceite:**
- Dado um lugar com leitura nos últimos 15 minutos, quando abro o detalhe, então vejo o índice (0 a 100), o
  nível e os três valores, cada um com uma frase explicativa.
- Dado um lugar sem leitura recente, então vejo "Sem medição recente" e a hora da última leitura.
- Dado um lugar sem nenhuma leitura, então vejo "Ainda não temos medições deste lugar".

### F03: Horários mais calmos
**Descrição:** como mãe de uma criança autista, quero saber os horários mais calmos de um lugar, para
planejar a ida.
**Critérios de aceite:**
- Dado um lugar com pelo menos 20 leituras, quando abro o detalhe, então vejo as 3 janelas mais calmas da
  semana (dia e hora) dentro do horário de funcionamento.
- Dado um lugar com menos de 20 leituras, então vejo "Ainda não há dados suficientes para sugerir
  horários".

### F04: Botão de crise
**Descrição:** como pessoa autista em sobrecarga, quero um botão que me leve ao lugar calmo mais próximo,
para me recompor rápido.
**Critérios de aceite:**
- Dado que estou em qualquer tela, então o botão de crise está visível no mesmo lugar.
- Dado que estou com internet e localização, quando toco no botão, então vejo em até 3 segundos o refúgio
  mais próximo, a distância e os minutos a pé, sem som e sem animação.
- Dado que estou sem internet, quando toco no botão, então vejo o refúgio mais próximo a partir dos refúgios
  guardados no celular.
- Dado que cadastrei um contato de apoio, então vejo o botão "Ligar para [nome]", que abre a discagem.
- Dado que estou na tela de crise, quando toco em "Abrir trajeto", então o app de mapas abre com o trajeto a
  pé.

### F05: Perfil sensorial anônimo
**Descrição:** como usuário, quero registrar o quanto sou sensível a som, luz e multidão sem me
identificar, para o app me ajudar sem expor quem eu sou.
**Critérios de aceite:**
- Dado que é a primeira abertura, quando escolho "Criar meu perfil", então o app cria um identificador
  anônimo, sem pedir nome, e-mail nem CPF.
- Dado o formulário do perfil, quando gravo valores de 1 a 5, então o perfil é salvo no celular e no
  servidor.
- Dado que peço para apagar meus dados, então perfil e relatos são apagados do servidor.

### F06: Relatos da comunidade
**Descrição:** como usuário, quero relatar como foi a visita a um lugar, para ajudar outras pessoas.
**Critérios de aceite:**
- Dado um lugar existente, quando envio nota de 1 a 5 e pelo menos um gatilho, então o relato é gravado e
  entra no resumo do lugar.
- Dado que escrevi CPF, telefone ou e-mail no comentário, então o dado é trocado por "[removido]" antes de
  gravar e o app me avisa.
- Dado um relato público, então ele não mostra quem escreveu.
- Dado que relatei o mesmo lugar há menos de 30 minutos, então o novo relato é recusado com uma mensagem
  clara.

### F07: Coleta de leituras pelo sensor
**Descrição:** como mantenedor da Calmaria, quero que o sensor envie as leituras sozinho, para o mapa estar
sempre atualizado.
**Critérios de aceite:**
- Dado um dispositivo cadastrado, quando ele publica uma leitura válida, então ela aparece na API em até 10
  segundos, com o índice recalculado pelo servidor.
- Dado um dispositivo não cadastrado, então a leitura é ignorada e registrada no log sem o conteúdo.
- Dado um valor fora da faixa física, então a leitura é recusada.

### F08: Previsão de barulho por distrito
**Descrição:** como pessoa autista, quero saber em que dias a região costuma ser mais barulhenta, para
escolher o melhor dia de sair.
**Critérios de aceite:**
- Dado um lugar de um distrito com previsão, quando abro o detalhe, então vejo o nível previsto (baixo,
  médio, alto) dos próximos 7 dias.
- Dado um distrito sem previsão, então a seção não aparece e nada quebra.
- Dado um novo CSV do modelo, quando ele é importado, então as previsões do mesmo distrito e dia são
  substituídas.

### F09: Proteção de dados pessoais
**Descrição:** como cidadão, quero que meu dado de saúde e meus contatos nunca vazem, porque a exposição me
deixaria vulnerável.
**Critérios de aceite:**
- Dado qualquer relato, perfil ou consulta de crise, então nenhum log, mensagem MQTT ou resposta pública
  contém CPF, telefone, termo de saúde do usuário ou coordenada precisa ligada a um usuário.
- Dado o painel de segurança, então o indicador Risco Cidadão é calculado a cada varredura.
- Dado um endpoint administrativo de privacidade, quando chamado sem a chave, então a resposta é 401 e não
  traz nenhum achado.

### F10: Acessibilidade
**Descrição:** como pessoa autista ou com baixa visão, quero um app previsível e legível, para usá-lo mesmo
sob estresse.
**Critérios de aceite:**
- Dado o leitor de tela ligado, então todo botão é anunciado com nome e função.
- Dado qualquer tela, então não há som, vibração forte nem animação; o texto tem contraste mínimo de 4,5:1.
- Dado o tamanho de fonte do sistema aumentado, então o texto do app aumenta junto sem cortar.

---

## Test cases

Formato de cada um: **ID, feature, tipo, objetivo, preparação, entradas, passos e resultado esperado.**
Positivos (P) confirmam o caminho certo; negativos (N) confirmam que o sistema recusa ou trata o errado.

| ID | Feature | Tipo | Objetivo |
|---|---|---|---|
| CT01 | F01 | P | Listar lugares perto com nível em cor e texto |
| CT02 | F01 | N | Recusar coordenada inválida |
| CT03 | F01 | P | Cair no mock quando a API não responde |
| CT04 | F02 | P | Mostrar a leitura atual de um lugar monitorado |
| CT05 | F02 | N | Lugar inexistente |
| CT06 | F03 | P | Sugerir as 3 janelas mais calmas |
| CT07 | F03 | N | Lugar com poucas leituras |
| CT08 | F04 | P | Refúgio mais próximo com internet |
| CT09 | F04 | P | Refúgio mais próximo sem internet |
| CT10 | F04 | N | Botão de crise sem permissão de localização |
| CT11 | F05 | P | Criar perfil anônimo e gravar sensibilidades |
| CT12 | F05 | N | Sensibilidade fora de 1 a 5 |
| CT13 | F05 | P | Apagar meus dados |
| CT14 | F06 | P | Enviar relato válido |
| CT15 | F06 | N | Nota fora de 1 a 5 |
| CT16 | F06 | N | Comentário com CPF e telefone |
| CT17 | F06 | N | Relato para lugar inexistente |
| CT18 | F06 | N | Relato repetido em menos de 30 minutos |
| CT19 | F07 | P | Leitura do sensor chega à API |
| CT20 | F07 | N | Leitura de dispositivo desconhecido |
| CT21 | F07 | N | Leitura fora da faixa física |
| CT22 | F08 | P | Ver a previsão de barulho do distrito |
| CT23 | F08 | N | Distrito sem previsão |
| CT24 | F09 | N | Dado de saúde não aparece no log |
| CT25 | F10 | P | Botão de crise com leitor de tela |
| CT26 | F09 | P | Varredura gera o Risco Cidadão |
| CT27 | F09 | N | Endpoint de privacidade sem a chave |

### CT01: Listar lugares perto com nível em cor e texto
- **Feature:** F01. **Tipo:** positivo.
- **Objetivo:** confirmar que o mapa e a lista mostram os lugares próximos com o nível em cor e em texto.
- **Preparação:** API .NET no ar com o seed; app instalado; localização simulada em -23,5614, -46,6559
  (Av. Paulista).
- **Entradas:** localização permitida.
- **Passos:** 1. Abrir o app. 2. Ir para a aba Mapa. 3. Tocar em "Lista".
- **Resultado esperado:** os 7 lugares do seed aparecem, ordenados por distância, cada um com cor e com a
  palavra Calmo, Moderado ou Intenso.

### CT02: Recusar coordenada inválida
- **Feature:** F01. **Tipo:** negativo.
- **Objetivo:** confirmar que a API recusa coordenadas impossíveis.
- **Preparação:** API .NET no ar.
- **Entradas:** `GET /api/lugares?lat=200&lng=-46,65`.
- **Passos:** 1. Enviar a requisição pelo Scalar.
- **Resultado esperado:** status 400 com mensagem dizendo que a latitude deve estar entre -90 e 90.

### CT03: Cair no mock quando a API não responde
- **Feature:** F01. **Tipo:** positivo.
- **Objetivo:** confirmar que o app continua útil com a API fora do ar.
- **Preparação:** API .NET desligada.
- **Entradas:** nenhuma.
- **Passos:** 1. Abrir o app. 2. Ir para o Mapa. 3. Esperar 3 segundos.
- **Resultado esperado:** os lugares de exemplo aparecem e a faixa "Mostrando dados de exemplo" fica
  visível.

### CT04: Mostrar a leitura atual de um lugar monitorado
- **Feature:** F02. **Tipo:** positivo.
- **Objetivo:** confirmar que o detalhe mostra o índice e os três valores.
- **Preparação:** ESP32 no Wokwi publicando para o lugar 1; potenciômetro e luz no máximo.
- **Entradas:** lugar 1 (Estação Consolação).
- **Passos:** 1. Abrir o detalhe do lugar 1. 2. Conferir os valores.
- **Resultado esperado:** índice de 70 ou mais, nível "Intenso agora", som acima de 100 dB, luz e
  movimento com os valores do simulador, cada um com uma frase explicativa.

### CT05: Lugar inexistente
- **Feature:** F02. **Tipo:** negativo.
- **Objetivo:** confirmar o tratamento de um id que não existe.
- **Preparação:** API .NET no ar.
- **Entradas:** `GET /api/lugares/999`.
- **Passos:** 1. Enviar a requisição.
- **Resultado esperado:** status 404 com `ProblemDetails` dizendo que o lugar não foi encontrado.

### CT06: Sugerir as 3 janelas mais calmas
- **Feature:** F03. **Tipo:** positivo.
- **Objetivo:** confirmar o cálculo dos horários calmos.
- **Preparação:** seed com 4 semanas de leituras simuladas no lugar 4 (supermercado).
- **Entradas:** lugar 4.
- **Passos:** 1. Abrir o detalhe do lugar 4. 2. Ler a seção "Horários mais calmos".
- **Resultado esperado:** 3 janelas com dia e hora, todas dentro do horário de funcionamento, com os menores
  índices médios da grade.

### CT07: Lugar com poucas leituras
- **Feature:** F03. **Tipo:** negativo.
- **Objetivo:** confirmar que o sistema não inventa horário com poucos dados.
- **Preparação:** lugar novo cadastrado com 5 leituras.
- **Entradas:** id do lugar novo.
- **Passos:** 1. Abrir o detalhe do lugar novo.
- **Resultado esperado:** a mensagem "Ainda não há dados suficientes para sugerir horários"; a API devolve
  `"suficiente": false`.

### CT08: Refúgio mais próximo com internet
- **Feature:** F04. **Tipo:** positivo.
- **Objetivo:** confirmar que o botão de crise leva ao refúgio certo.
- **Preparação:** internet ligada; localização em -23,5614, -46,6559.
- **Entradas:** toque no botão de crise.
- **Passos:** 1. Em qualquer tela, tocar no botão de crise. 2. Conferir o refúgio. 3. Tocar em "Abrir
  trajeto".
- **Resultado esperado:** em até 3 segundos aparece o refúgio mais próximo (Parque Trianon ou o lugar calmo
  mais perto), com distância e minutos a pé; sem som e sem animação; o app de mapas abre com o trajeto a pé.

### CT09: Refúgio mais próximo sem internet
- **Feature:** F04. **Tipo:** positivo.
- **Objetivo:** confirmar que o botão de crise funciona offline.
- **Preparação:** app aberto uma vez com internet (para guardar os refúgios); depois, modo avião.
- **Entradas:** toque no botão de crise.
- **Passos:** 1. Ativar o modo avião. 2. Tocar no botão de crise.
- **Resultado esperado:** o refúgio mais próximo aparece a partir do cache, com a distância calculada no
  celular e o aviso "Sem internet: usando os refúgios salvos".

### CT10: Botão de crise sem permissão de localização
- **Feature:** F04. **Tipo:** negativo.
- **Objetivo:** confirmar que a crise não trava sem localização.
- **Preparação:** permissão de localização negada.
- **Entradas:** toque no botão de crise.
- **Passos:** 1. Tocar no botão de crise.
- **Resultado esperado:** o app mostra a lista de refúgios da região com endereço, o roteiro de respiração e
  o botão do contato de apoio, sem mensagem de erro técnica.

### CT11: Criar perfil anônimo e gravar sensibilidades
- **Feature:** F05. **Tipo:** positivo.
- **Objetivo:** confirmar que o perfil é criado sem dado identificador.
- **Preparação:** app recém-instalado; API no ar.
- **Entradas:** som 5, luz 3, multidão 4; contato "Mãe", telefone fictício (11) 90000-0000.
- **Passos:** 1. Abrir o app. 2. Tocar em "Criar meu perfil". 3. Preencher e salvar. 4. Fechar e abrir o
  app.
- **Resultado esperado:** nenhum campo pede nome, e-mail ou CPF; o perfil reaparece igual; `GET
  /api/usuarios/{id}/perfil-sensorial` devolve os mesmos valores.

### CT12: Sensibilidade fora de 1 a 5
- **Feature:** F05. **Tipo:** negativo.
- **Objetivo:** confirmar a validação do perfil.
- **Preparação:** usuário criado.
- **Entradas:** `PUT /api/usuarios/{id}/perfil-sensorial` com `sensibilidadeSom: 7`.
- **Passos:** 1. Enviar pelo Scalar.
- **Resultado esperado:** status 400 dizendo que a sensibilidade deve estar entre 1 e 5; nada é gravado.

### CT13: Apagar meus dados
- **Feature:** F05. **Tipo:** positivo.
- **Objetivo:** confirmar o direito de exclusão.
- **Preparação:** usuário com perfil e 2 relatos.
- **Entradas:** `DELETE /api/usuarios/{id}`.
- **Passos:** 1. Enviar a exclusão. 2. Consultar o perfil. 3. Consultar os relatos do lugar.
- **Resultado esperado:** 204 na exclusão; 404 no perfil; os 2 relatos não aparecem mais.

### CT14: Enviar relato válido
- **Feature:** F06. **Tipo:** positivo.
- **Objetivo:** confirmar o envio e o resumo.
- **Preparação:** usuário criado; API no ar.
- **Entradas:** lugar 1, nota 4, gatilhos BARULHO e MULTIDAO, comentário "Muito cheio às 18h".
- **Passos:** 1. Abrir "Relatar". 2. Preencher. 3. Enviar. 4. Abrir o detalhe do lugar 1.
- **Resultado esperado:** mensagem de sucesso; o resumo do lugar 1 conta o relato e lista BARULHO entre os
  gatilhos mais citados; o relato público não mostra o autor.

### CT15: Nota fora de 1 a 5
- **Feature:** F06. **Tipo:** negativo.
- **Objetivo:** confirmar a validação da nota.
- **Preparação:** usuário criado.
- **Entradas:** `POST /api/relatos` com `nota: 0`.
- **Passos:** 1. Enviar pelo Scalar.
- **Resultado esperado:** status 400 com a mensagem "A nota deve estar entre 1 e 5".

### CT16: Comentário com CPF e telefone
- **Feature:** F06 e F09. **Tipo:** negativo.
- **Objetivo:** confirmar que dado pessoal não é gravado.
- **Preparação:** usuário criado.
- **Entradas:** comentário "Meu CPF é 529.982.247-25, me liguem no 11 91234-5678" (CPF gerado só para
  teste, válido pelo dígito verificador).
- **Passos:** 1. Enviar o relato. 2. Consultar o relato no banco e nos relatos públicos. 3. Ler o log da API.
- **Resultado esperado:** o comentário fica "Meu CPF é [removido], me liguem no [removido]"; o app avisa o
  que foi removido; o log não contém o CPF nem o telefone.

### CT17: Relato para lugar inexistente
- **Feature:** F06. **Tipo:** negativo.
- **Objetivo:** confirmar que um relato só aceita lugar existente.
- **Preparação:** API no ar.
- **Entradas:** `POST /api/relatos` com `lugarId: 999`.
- **Passos:** 1. Enviar pelo Scalar.
- **Resultado esperado:** status 404 dizendo que o lugar não existe; nada é gravado.

### CT18: Relato repetido em menos de 30 minutos
- **Feature:** F06. **Tipo:** negativo.
- **Objetivo:** confirmar a proteção contra spam.
- **Preparação:** CT14 executado há menos de 30 minutos.
- **Entradas:** o mesmo usuário e o mesmo lugar 1.
- **Passos:** 1. Enviar outro relato para o lugar 1.
- **Resultado esperado:** status 409 e a mensagem "Você já relatou este lugar há pouco. Tente de novo em
  alguns minutos."

### CT19: Leitura do sensor chega à API
- **Feature:** F07. **Tipo:** positivo.
- **Objetivo:** confirmar a integração ESP32, MQTT e API.
- **Preparação:** API .NET com o MQTT ligado; ESP32 no Wokwi com `LUGAR_ID = 2`.
- **Entradas:** potenciômetro em 50%, luz em 500 lux, PIR sem movimento.
- **Passos:** 1. Iniciar a simulação. 2. Esperar 10 segundos. 3. Consultar `GET /api/lugares/2`.
- **Resultado esperado:** a última leitura tem som perto de 75 dB e o índice calculado pela API, igual ao do
  display do ESP32.

### CT20: Leitura de dispositivo desconhecido
- **Feature:** F07. **Tipo:** negativo.
- **Objetivo:** confirmar que a API ignora fonte não cadastrada.
- **Preparação:** API .NET no ar.
- **Entradas:** mensagem MQTT com `dispositivoId: "intruso-01"`.
- **Passos:** 1. Publicar a mensagem num cliente MQTT. 2. Consultar as leituras e o log.
- **Resultado esperado:** nenhuma leitura nova; o log registra "dispositivo desconhecido" sem o conteúdo da
  mensagem.

### CT21: Leitura fora da faixa física
- **Feature:** F07. **Tipo:** negativo.
- **Objetivo:** confirmar a validação física.
- **Preparação:** API .NET no ar; dispositivo cadastrado.
- **Entradas:** `POST /api/leituras` com `somDb: -10`.
- **Passos:** 1. Enviar pelo Scalar.
- **Resultado esperado:** status 422 dizendo que o som deve estar entre 0 e 140 dB.

### CT22: Ver a previsão de barulho do distrito
- **Feature:** F08. **Tipo:** positivo.
- **Objetivo:** confirmar que a previsão do ML chega ao app.
- **Preparação:** `previsao_barulho.csv` importado.
- **Entradas:** lugar 1 (distrito Consolação).
- **Passos:** 1. Abrir o detalhe do lugar 1. 2. Ler a seção de previsão.
- **Resultado esperado:** 7 dias com nível baixo, médio ou alto, iguais aos do CSV.

### CT23: Distrito sem previsão
- **Feature:** F08. **Tipo:** negativo.
- **Objetivo:** confirmar o tratamento sem previsão.
- **Preparação:** previsões importadas só para Consolação e Bela Vista.
- **Entradas:** `GET /api/previsoes/barulho?distrito=INEXISTENTE&data=2026-11-21`.
- **Passos:** 1. Enviar a requisição. 2. Abrir no app um lugar de outro distrito.
- **Resultado esperado:** 404 na API; no app, a seção de previsão não aparece e a tela carrega normal.

### CT24: Dado de saúde não aparece no log
- **Feature:** F09. **Tipo:** negativo.
- **Objetivo:** confirmar o bloqueio do fluxo do dado sensível até o log.
- **Preparação:** APIs no ar; módulo de PII configurado.
- **Entradas:** relato com o comentário "Meu filho é autista e teve uma crise aqui".
- **Passos:** 1. Enviar o relato. 2. Rodar a varredura (`POST /api/privacidade/varreduras`) e abrir o painel em `/seguranca`.
- **Resultado esperado:** o motor não acha termo de saúde ligado a usuário nos logs; o painel mostra o fluxo
  como bloqueado.

### CT25: Botão de crise com leitor de tela
- **Feature:** F10. **Tipo:** positivo.
- **Objetivo:** confirmar que o botão de crise é acessível.
- **Preparação:** TalkBack (Android) ou VoiceOver (iOS) ligado.
- **Entradas:** navegação por gestos.
- **Passos:** 1. Navegar até o botão de crise. 2. Ativar com toque duplo.
- **Resultado esperado:** o leitor anuncia "Botão de crise, abre o lugar calmo mais próximo"; a tela abre
  sem som e sem animação.

### CT26: Varredura gera o Risco Cidadão
- **Feature:** F09. **Tipo:** positivo.
- **Objetivo:** confirmar que a varredura calcula o indicador e que o painel mostra o resultado.
- **Preparação:** API no ar com o seed; chave administrativa configurada; CT16 executado.
- **Entradas:** `POST /api/privacidade/varreduras` com o cabeçalho `X-Admin-Key`.
- **Passos:** 1. Enviar a requisição. 2. Abrir `/seguranca/index.html`.
- **Resultado esperado:** 200 com o id da varredura; o painel mostra o Risco Cidadão de 0 a 100, os achados
  por categoria e o fluxo do comentário até o log como bloqueado.

### CT27: Endpoint de privacidade sem a chave
- **Feature:** F09. **Tipo:** negativo.
- **Objetivo:** confirmar que os dados de segurança não ficam abertos.
- **Preparação:** API no ar.
- **Entradas:** `GET /api/privacidade/risco-cidadao` sem o cabeçalho `X-Admin-Key`.
- **Passos:** 1. Enviar a requisição.
- **Resultado esperado:** status 401; a resposta não traz nenhum achado nem trecho de texto.

---

## O PDF de entrega
1. Capa: Calmaria, equipe, disciplina, **link do projeto no Azure DevOps** e **link do pitch no YouTube**.
2. Print da lista de features.
3. Print de cada feature aberta, com descrição e critérios de aceite.
4. Print da lista de test cases.
5. Print de cada test case aberto, com os passos.

## O pitch (o Integrante 5 monta; você envia o link)
O pitch concorre ao prêmio de melhor GS e o link é enviado nesta disciplina. Quem monta, edita e publica é o
Integrante 5 (o roteiro está em `physical-computing.md`). Você recebe o link até quinta, 19/11, às 18h, e põe
na capa do PDF e no envio.

## Os prints que você vai pedir aos outros
- Do Integrante 2: todas as telas do app, incluindo o botão de crise offline.
- Do Luiz: o Scalar da API e o painel Risco Cidadão.
- Os test cases que dependem de outra parte (CT04, CT19 e CT20 dependem do ESP32 e da API) podem ser escritos
  agora: a rubrica avalia as features e os test cases cadastrados, não a execução deles.


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
| AP-15 | Prints do README | Integrante 2 | PNG do aparelho, sem moldura | Todas as telas, a crise sem internet e uma com o leitor de tela ligado. |
| PA-02 | Print do painel | Luiz | PNG 2880 × 1800 (2x) | Tema claro, antes e depois visíveis, selo "Dados de exemplo" se for seed. |
| AD-02 | Diagrama da arquitetura | Luiz | SVG e PNG 1920 × 1080 | Caixas de raio 10, traço 1,5; API em #E7EEF3 com borda #3D5A73; VM tracejada; protocolo escrito em cada seta. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-06 | Miniatura do YouTube | Integrante 5 | 1280 × 720 JPG, até 2 MB | Fundo #1F2733, texto de 96 px, os três selos de nível. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |
| QA-01 | PDF do Azure Boards | Integrante 4 | A4 retrato, margens de 20 mm | Capa com logotipo, disciplina, os dois links e a equipe; prints com 170 mm de largura e o ID na legenda. |

**Pitch:** as cartelas, as tarjas e a miniatura são do Integrante 5, que monta o pitch. A capa do PDF
(QA-01) é sua: refaça com os links certos por `python design/pecas/exportar.py --azure ... --youtube ...`.

## Checklist de envio
- [ ] Professor adicionado como Admin no projeto
- [ ] 10 features com descrição e critérios de aceite
- [ ] 27 test cases, positivos e negativos, ligados às features
- [ ] PDF com o link do projeto e todos os prints
- [ ] Link do pitch (até 4 minutos, YouTube) no envio
- [ ] Enviado no Teams por um integrante

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

### Contrato: a mensagem MQTT (ESP32 para a API)
- **Broker:** `broker.hivemq.com`, porta `1883`. É público e sem senha: qualquer pessoa pode assinar o tópico,
  por isso **a mensagem nunca leva dado pessoal**.
- **Tópico:** `calmaria/v1/lugares/{lugarId}/leituras` (ex.: `calmaria/v1/lugares/1/leituras`).
- **Frequência:** a cada 5 segundos na demonstração (num lugar real seria 1 minuto).
- **Payload (JSON, UTF-8):**

```json
{
  "dispositivoId": "esp32-consolacao-01",
  "lugarId": 1,
  "somDb": 78.5,
  "luzLux": 1450,
  "movimentosMin": 12,
  "indice": 53,
  "nivel": "MODERADO"
}
```

| Campo | Tipo | Faixa aceita pela API |
|---|---|---|
| `dispositivoId` | texto | precisa estar cadastrado na API (um por lugar monitorado; ver a tabela de lugares) |
| `lugarId` | inteiro | o id do lugar onde o aparelho está |
| `somDb` | decimal | 0 a 140 |
| `luzLux` | decimal | 0 a 100.000 |
| `movimentosMin` | inteiro | 0 a 500 |
| `indice`, `nivel` | inteiro, texto | informativos: a API recalcula com a fórmula do ISS |

A API carimba a data e a hora de recebimento; o ESP32 não precisa de relógio. Para ver as mensagens sem a API,
assine `calmaria/v1/lugares/#` num cliente MQTT (MQTT Explorer ou o cliente web da HiveMQ).

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

### Contrato: o CSV de previsões (notebook de ML para a API)
Arquivo `ml/saida/previsao_barulho.csv`, UTF-8, separador vírgula, ponto como separador decimal, uma linha
por distrito e dia:

```
distrito,data,reclamacoes_previstas,nivel
CONSOLACAO,2026-11-21,9.4,ALTO
CONSOLACAO,2026-11-24,4.1,MEDIO
BELA VISTA,2026-11-21,6.0,ALTO
```

| Coluna | Formato |
|---|---|
| `distrito` | Nome do distrito em maiúsculas e sem acento, exatamente como na base do SP156 (ex.: `CONSOLACAO`, `BELA VISTA`, `JARDIM PAULISTA`) |
| `data` | `AAAA-MM-DD` |
| `reclamacoes_previstas` | Decimal com uma casa |
| `nivel` | `BAIXO`, `MEDIO` ou `ALTO`, pelos tercis do próprio distrito (o terço mais baixo da história do distrito é BAIXO) |

**Cobertura mínima:** os distritos dos lugares do protótipo (Consolação e Bela Vista, e Jardim Paulista se a
observação mostrar que algum lugar fica lá), de 16/11/2026 a 13/12/2026. O Luiz importa pela rota
`POST /api/previsoes/importar`.

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

- **Todos:** pedir os prints.
- **Integrante 5:** o link do pitch até quinta, 19/11, às 18h.
- **Luiz:** a chave administrativa de teste para os casos de privacidade e o print do painel.
- **Integrante 2:** os prints das telas.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
