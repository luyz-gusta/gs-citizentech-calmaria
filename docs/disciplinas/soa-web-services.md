# SOA e Web Services: a API Calmaria vista pela rubrica de SOA

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | SOA e Web Services |
| Responsável | Luiz |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `api-dotnet/` (a mesma API da disciplina de C#) |
| Você depende de | A API descrita em `csharp.md` |
| Dependem de você | Ninguém além do que já depende da API |
| Vídeo | Não exigido nesta disciplina |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: desenvolvedor .NET sênior e revisor da rubrica de SOA.

A entrega de SOA é a mesma API em C# da disciplina de C# (plano técnico em `csharp.md`, que eu também vou te mandar). Seu trabalho aqui é garantir que cada critério de SOA esteja visível e bem documentado na API: domínio, banco, migrações, documentação automática, testes e a chamada REST externa.

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

## O que a disciplina pede
Uma aplicação, **em qualquer linguagem**, que seja a camada de backend da CitizenTech, com estruturação do
domínio, conexão com banco, chamadas a APIs REST e testes automatizados, mostrando boas práticas e
integração com a proposta.

**Entregáveis:** repositório GitHub com o código completo; README com instruções de execução, descrição da
solução e nomes dos integrantes; documentação dos endpoints (Swagger ou equivalente); evidências da
aplicação. **Envio:** link do GitHub no Teams até 19/11, 23h55.

> **Confirmar com o professor na sexta, 13/11:** a entrega pode ser a mesma API da disciplina de C#? Se ele
> exigir um serviço separado, o plano B está no fim deste arquivo e não exige reescrever regra nenhuma.

## Onde está cada critério

| Critério | Peso | Onde está na API | O que mostrar ao professor |
|---|---|---|---|
| **Estruturação do domínio** | 3,0 | `Calmaria.Domain`: os módulos Sensorial (lugar, refúgio, dispositivo, leitura, previsão) e Comunidade (usuário, perfil, relato), com objetos de valor e regras dentro das entidades | O diagrama de classes no README; uma regra de negócio barrando um dado inválido (nota 6, leitura de -10 dB, relato repetido) |
| **Conexão com banco de dados** | 3,0 | PostgreSQL com EF Core (Npgsql); relacionamentos 1:1 e 1:N, cascata, índices; conexão por variável de ambiente | As tabelas no PostgreSQL (`\dt` no `psql`), a string de conexão vinda do ambiente e não do código, uma consulta LINQ e o SQL que ela gerou no log |
| **Migrações** | 1,5 | Uma migração por módulo: `Inicial_Sensorial`, `Comunidade`, `Privacidade`; o pacote de migração que a VM aplica | `dotnet ef migrations list` e o banco vazio sendo criado do zero com `dotnet ef database update` |
| **Documentação automática** | 1,5 | OpenAPI gerado pelo código, Scalar em `/scalar`, comentários XML e `[ProducesResponseType]` em cada status, endpoints agrupados por módulo | O Scalar aberto, um endpoint com exemplo, testado dali mesmo |
| **Testes automatizados** | 1,0 | `Calmaria.Tests` e `Calmaria.Privacidade.Tests` (MSTest) | `dotnet test` passando, com a contagem de testes |
| Chamadas a APIs REST (no objetivo da disciplina) | | A API chama o LLM por REST com `HttpClient` tipado e resiliência; opcionalmente o ViaCEP | O log da chamada ao LLM (com o texto mascarado) e o alerta que voltou |

## Onde está a orientação a serviços
Mesmo numa API só, o projeto mostra os princípios de SOA, e vale dizer isso no README e no vídeo:
- **Serviços com fronteira e contrato:** os três módulos (Sensorial, Comunidade, Privacidade) só conversam por
  interface (`ILugarConsulta`, `ISanitizador`). Cada um tem os próprios controllers, serviços, tabelas e
  migração.
- **Contratos publicados:** o OpenAPI é o contrato que o app consome; o tópico e o payload MQTT são o contrato com o ESP32; o CSV de previsões é o contrato com o ML.
- **Consumo de serviço externo:** o LLM (e o ViaCEP, se entrar), com tempo limite e nova tentativa.
- **Integração assíncrona por mensagem:** o ESP32 publica no broker e a API assina, sem um conhecer o outro.
- **Baixo acoplamento:** qualquer módulo pode virar um serviço separado sem mudar quem o consome (ver o
  plano B).

## Seção do README para o professor de SOA
Uma seção curta no README de `api-dotnet/`, "Para a avaliação de SOA", com a tabela de critérios acima
apontando os arquivos reais (`src/Calmaria.Domain/...`, `src/Calmaria.Infrastructure/Migrations/...`), o link
do Scalar e o comando dos testes. O professor acha cada critério em um minuto.

## Evidências (em `api-dotnet/docs/soa/`)
- Diagrama de classes do domínio (pode ser Mermaid no README).
- Print do `dotnet ef migrations list` e do banco criado do zero.
- Print das tabelas no PostgreSQL.
- Print do Scalar com os três grupos de endpoints.
- Print do `dotnet test`.
- Print do log da chamada ao LLM.
- Diagrama de sequência de um relato: app, módulo Comunidade, `ILugarConsulta`, `Sanitizador`, banco.

## O que esta entrega exige além do que C# já pede
Quase nada, e esse é o ganho de juntar. O que precisa de atenção:
- As **três migrações** separadas por módulo, com nomes que contem a história.
- Os **comentários XML** e os exemplos no Scalar (a documentação vale 1,5 aqui).
- O **diagrama de classes** e o diagrama de sequência.
- A seção "Para a avaliação de SOA" no README.

## Plano B: se o professor exigir um serviço separado
Extrair o módulo Comunidade para um segundo projeto, `Calmaria.Comunidade.Api`, também em C#:
1. Mover os controllers da Comunidade para o projeto novo, com o próprio `DbContext` e as migrações da
   Comunidade.
2. Trocar a implementação de `ILugarConsulta`, que hoje lê o banco, por uma que chama `GET /api/lugares/{id}`
   da API principal por `HttpClient`.
3. O app passa a ter duas URLs.

As regras de negócio e os testes de domínio não mudam. Estimativa: uma tarde.


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
| AD-01 | Scalar com o tema | Luiz | CSS da API | Título "API Calmaria", acento #3D5A73 (#B2C9D8 no escuro), fonte Atkinson. |
| AD-02 | Diagrama da arquitetura | Luiz | SVG e PNG 1920 × 1080 | Caixas de raio 10, traço 1,5; API em #E7EEF3 com borda #3D5A73; VM tracejada; protocolo escrito em cada seta. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |

**Scalar:** título "API Calmaria", acento `#3D5A73` e fonte Atkinson, pelo CSS customizado descrito em
`csharp.md`. O diagrama (AD-02) entra no README como a figura da arquitetura.

## Checklist de envio (SOA)
- [ ] Resposta do professor sobre usar a mesma API (13/11)
- [ ] Seção "Para a avaliação de SOA" no README, com a tabela de critérios apontando os arquivos
- [ ] Três migrações no repositório e o banco criando do zero
- [ ] Scalar com comentários XML e exemplos
- [ ] `dotnet test` passando
- [ ] Evidências em `api-dotnet/docs/soa/`
- [ ] Link do repositório enviado no Teams por um integrante

---

# Parte 3: contratos com as outras partes

Estes formatos são combinados entre as partes. **Não mude nome de campo, rota, tópico nem coluna sem avisar a equipe.**

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

---

# Parte 4: com quem falar

- É a mesma API de `csharp.md`: os contatos estão lá.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
