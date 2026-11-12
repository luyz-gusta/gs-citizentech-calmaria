# C# Software Development: a API Calmaria

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | C# Software Development (a mesma API também é entregue em SOA e em Cybersecurity) |
| Responsável | Luiz |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `api-dotnet/` |
| Você depende de | Integrante 3 (CSV de previsões, segunda 16/11); Integrante 5 (mensagens MQTT do ESP32) |
| Dependem de você | Integrante 2 (o app consome a API); Integrante 4 (a VM sobe a API; test cases) |
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

Seu papel: desenvolvedor .NET sênior (ASP.NET Core 10, EF Core com PostgreSQL, LINQ, MQTTnet, MSTest) e meu par de programação.

Vamos construir a API Calmaria em C#, que é avaliada em três disciplinas (C#, SOA e Cybersecurity). Este arquivo é o plano técnico da API; os arquivos `soa-web-services.md` e `cybersecurity.md` dizem o que as outras duas avaliam nela. Comece pelo esqueleto da solução e pelos endpoints de lugares, porque o app e a VM dependem deles.

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
Uma aplicação em C#/.NET que seja a camada de backend da CitizenTech, aplicando POO, interfaces, injeção de
dependência, Entity Framework Core, LINQ, APIs REST, programação assíncrona e testes unitários.

**Entregáveis:** repositório GitHub com o projeto C#, a Web API, o EF Core, README com instruções de execução
e descrição da solução, documentação dos endpoints (Swagger, Scalar ou equivalente) e evidências da
aplicação. **Envio:** link do GitHub no Teams até 19/11, 23h55.

| Critério | Peso | Onde isso aparece na API |
|---|---|---|
| POO | 3,0 | Hierarquia de lugares; cada regra de dado sensível como uma classe de `IRegraPii`; cada fonte de varredura como uma classe de `IFonteVarredura`; objetos de valor |
| Web API | 2,5 | Os controllers dos três módulos, com status HTTP certos, validação e `ProblemDetails` |
| EF Core e LINQ | 1,5 | PostgreSQL, relacionamentos, migrações, consultas de horários calmos, resumo de relatos e risco |
| Async/await | 1,0 | Banco, consumidor MQTT em segundo plano e chamada ao LLM com `HttpClient` |
| Qualidade do código | 2,0 | Módulos com fronteira clara, nomes, testes, README, sem código morto |

## Uma API, três módulos, três disciplinas

| Módulo | O que faz | Responsável | Disciplinas que avaliam |
|---|---|---|---|
| **Sensorial** | Lugares, dispositivos, leituras do ESP32 por MQTT, índice de sobrecarga, horários calmos, refúgio do botão de crise, previsões do ML | Luiz | C# e SOA |
| **Comunidade** | Usuário anônimo, perfil sensorial, relatos, resumo por lugar, exclusão dos dados (LGPD) | Luiz | C# e SOA |
| **Privacidade** | Motor de PII, taint analysis, varreduras, tradução do risco por IA, indicador Risco Cidadão | Luiz | Cybersecurity, C# e SOA |

**A regra que mantém os módulos separados:** um módulo só fala com outro por interface. A Comunidade
pergunta se um lugar existe por `ILugarConsulta`, nunca lendo a tabela de lugares direto; a Comunidade e o
log só recebem texto que passou pelo `Sanitizador` da Privacidade. É isso que deixa cada módulo testável
sozinho e o que mostra orientação a serviço para o professor de SOA. Se um professor exigir
um serviço separado, o módulo sai inteiro para outro projeto ASP.NET sem reescrever regra nenhuma.

## Estrutura

```
api-dotnet/
├── Calmaria.sln
├── src/
│   ├── Calmaria.Domain/            Entidades, enums, objetos de valor e interfaces dos módulos Sensorial e Comunidade
│   ├── Calmaria.Privacidade/       O módulo de Cybersecurity: sem dependência de ASP.NET, para poder ir sozinho no ZIP
│   ├── Calmaria.Infrastructure/    DbContext (PostgreSQL), configurações, migrações, worker MQTT, cliente do LLM, log com redação
│   └── Calmaria.Api/
│       ├── Sensorial/              Controllers e DTOs do módulo
│       ├── Comunidade/
│       ├── Privacidade/            Controllers, filtro de resposta pública
│       ├── wwwroot/seguranca/      Painel Risco Cidadão (HTML)
│       └── Program.cs
└── tests/
    ├── Calmaria.Tests/             Domínio, serviços e endpoints dos módulos Sensorial e Comunidade
    └── Calmaria.Privacidade.Tests/ Regras de PII, taint e cálculo de risco
```

## Domínio

### Módulo Sensorial
- **`Lugar` (abstrata):** `Id`, `Nome`, `Tipo`, `Latitude`, `Longitude`, `Endereco`, `Distrito`,
  `HorarioFuncionamento`. Método `DistanciaAte(Coordenada)` (Haversine).
- **`LugarMonitorado : Lugar`:** mercado, estação, parque. Tem `Dispositivos` e `Leituras`; método
  `NivelAtual()`.
- **`Refugio : Lugar`:** lugar calmo fixo, sem sensor, com `Descricao` do que oferece.
- As duas filhas ficam numa tabela só (TPH, discriminador `Tipo`): herança usada pelo banco.
- **`Dispositivo`:** `Id` (texto, ex. `esp32-consolacao-01`), `LugarId`, `Ativo`, `UltimaLeituraEm`.
- **`Leitura`:** `Id`, `DispositivoId`, `LugarId`, `SomDb`, `LuzLux`, `MovimentosMin`, `Indice`, `Nivel`,
  `RegistradaEm`. Método de fábrica que recusa valor fora da faixa física (som de 0 a 140 dB, luz de 0 a
  100.000 lux, movimentos de 0 a 500).
- **`PrevisaoBarulho`:** `Distrito`, `Data`, `ReclamacoesPrevistas`, `Nivel`.
- **Objetos de valor:** `Coordenada` (`record` validado) e `IndiceSobrecarga` (0 a 100, sabe o próprio
  `Nivel`).

### Módulo Comunidade
- **`Usuario`:** `Id` (`Guid`), `Apelido` opcional, `CriadoEm`. Sem nome, e-mail, CPF nem senha: o anonimato
  é decisão de projeto.
- **`PerfilSensorial`** (um para um com `Usuario`): `SensibilidadeSom`, `SensibilidadeLuz`,
  `SensibilidadeMultidao` (objeto de valor `Sensibilidade`, de 1 a 5), `ContatoApoioNome`,
  `ContatoApoioTelefone` (os dois marcados como dado sensível; ver Cybersecurity).
- **`Relato`** (muitos para um com `Usuario` e com `Lugar`): `Id`, `Nota` (objeto de valor
  `NotaSensorial`, de 1 a 5), `Gatilhos` (`BARULHO`, `LUZ_FORTE`, `MULTIDAO`, `CHEIRO`, `FILA`, `OUTRO`),
  `Comentario` (até 500 caracteres, **já sanitizado**), `VisitadoEm`, `CriadoEm`.

Regras de negócio:
- Nota fora de 1 a 5, gatilho desconhecido e `VisitadoEm` no futuro: 400 com mensagem clara.
- O mesmo usuário relatando o mesmo lugar em menos de 30 minutos: 409.
- O comentário entra como `TextoNaoConfiavel` e só é gravado depois do `Sanitizador`; a resposta diz o que
  foi removido.
- A listagem pública de relatos nunca devolve o usuário nem o apelido.
- Apagar o usuário apaga em cascata o perfil e os relatos (LGPD, art. 18).

### Módulo Privacidade
Detalhado em [cybersecurity.md](cybersecurity.md): `IRegraPii` e as regras, `MotorPii`, os tipos
`TextoNaoConfiavel` e `TextoSanitizado`, o `RastreadorDeFluxo`, as fontes de varredura, o tradutor de risco
por IA e a `CalculadoraRiscoCidadao`.

### Interfaces e serviços
- Sensorial: `ICalculadoraIndice` (implementação `CalculadoraIndicePonderada`, pesos em `IndiceOptions` no
  `appsettings.json`), `ILeituraService`, `IHorarioCalmoService`, `IRefugioService`, `IPrevisaoService` e
  `ILugarConsulta` (a porta que os outros módulos usam).
- Comunidade: `IUsuarioService`, `IRelatoService`, `IResumoRelatosService`.
- Privacidade: `IMotorPii`, `ISanitizador`, `IRastreadorDeFluxo`, `IServicoVarredura`, `ITradutorDeRisco`,
  `ICalculadoraRiscoCidadao`.

Tudo registrado por injeção de dependência, com um método de extensão por módulo (`AddModuloSensorial()`,
`AddModuloComunidade()`, `AddModuloPrivacidade()`) para o `Program.cs` ficar curto e legível.

## Banco: PostgreSQL com EF Core
- Provedor `Npgsql.EntityFrameworkCore.PostgreSQL`. PostgreSQL pesa mais que SQLite em "conexão com banco"
  na rubrica de SOA e é o que a VM de Operating Systems instala.
- **Conexão por variável de ambiente** (`ConnectionStrings__Calmaria`) e `dotnet user-secrets` em
  desenvolvimento. **Nenhuma senha no `appsettings.json` do repositório**: o próprio motor de PII procura
  isso.
- Para rodar local: PostgreSQL instalado na máquina, ou um contêiner para quem tiver Docker funcionando, ou
  um PostgreSQL gratuito na nuvem (Neon, Supabase) só com dados fictícios.
- Configurações com `IEntityTypeConfiguration<T>`; relacionamentos `Usuario` 1:1 `PerfilSensorial`,
  `Usuario` 1:N `Relato` (cascata), `Lugar` 1:N `Relato`, `Lugar` 1:N `Leitura`; gatilhos numa tabela
  `RelatoGatilho`; índices em `Leitura(LugarId, RegistradaEm)`, `Relato(LugarId, CriadoEm)` e
  `PrevisaoBarulho(Distrito, Data)`.
- **Migrações, uma por módulo,** para mostrar a evolução do esquema: `Inicial_Sensorial`, `Comunidade` e
  `Privacidade` (varreduras, achados e eventos de fluxo).
- **Seed:** os 7 lugares da tabela de lugares (Parte 3) (`HasData`), um dispositivo por lugar monitorado, 4 semanas
  de leituras simuladas (um `Seeder` com `new Random(42)`), 5 usuários e 30 relatos fictícios. O README diz
  que é tudo simulado.
- **Testes de endpoint** com SQLite em memória (o modelo fica sem nada específico do PostgreSQL para isso
  funcionar).

### As consultas LINQ que valem mostrar
- **Horários calmos:** leituras dos últimos 28 dias do lugar, agrupadas por dia da semana e hora, média do
  índice, as 3 menores dentro do horário de funcionamento.
- **Refúgio mais próximo:** retângulo de latitude e longitude no banco, distância Haversine em memória.
- **Lugares perto com o nível atual:** junção com a leitura mais recente de cada lugar.
- **Resumo dos relatos:** nota média e gatilhos mais citados (`SelectMany` e `GroupBy`).
- **Risco Cidadão:** achados da última varredura agrupados por categoria e destino.

## Endpoints
O contrato completo, com exemplos de requisição e resposta, está na Parte 3.

| Rota | Regras |
|---|---|
| `GET /api/lugares` | `lat` e `lng` opcionais; com eles, filtra por `raioM` (padrão 1500) e ordena por distância; coordenada inválida dá 400 |
| `GET /api/lugares/{id}` | 404 se não existir |
| `POST /api/lugares` | Validação; 201 com `Location`. Opcional: com `cep`, completa o endereço pelo ViaCEP |
| `POST /api/leituras` | Mesmo serviço do MQTT; 422 para valor fora de faixa, 404 para dispositivo desconhecido |
| `GET /api/lugares/{id}/horarios-calmos` | Com menos de 20 leituras, 200 com `"suficiente": false` |
| `GET /api/crise/refugio-mais-proximo` | `lugar`, `distanciaM`, `minutosAPe` (80 m por minuto) e `linkTrajeto` do Google Maps a pé; a coordenada recebida é arredondada antes de qualquer log |
| `GET /api/refugios` | Todos os refúgios, para o app guardar e usar offline |
| `GET /api/previsoes/barulho`, `POST /api/previsoes/importar` | Previsões do ML (contrato do CSV, Parte 3) |
| `POST /api/usuarios` | 201 com o `id` |
| `GET` e `PUT /api/usuarios/{id}/perfil-sensorial` | 404 se o usuário não existe; 400 com sensibilidade fora de 1 a 5 |
| `DELETE /api/usuarios/{id}` | 204; apaga perfil e relatos |
| `POST /api/relatos` | 201 com `removidos` (o que o sanitizador tirou); 400, 404 (usuário ou lugar), 409 |
| `GET /api/relatos?lugarId=&pagina=&tamanho=` | Relatos públicos, sem autor, paginados |
| `GET /api/lugares/{id}/resumo-relatos` | Nota média e gatilhos mais citados |
| `POST /api/privacidade/analisar` | Analisa um texto e devolve os achados mascarados (o app usa para avisar antes de enviar) |
| `/api/privacidade/varreduras`, `/risco-cidadao`, `/achados`, `/fluxos` | Ver [cybersecurity.md](cybersecurity.md); exigem o cabeçalho `X-Admin-Key` |
| `GET /health` | Banco e worker MQTT; a VM usa |

DTOs separados das entidades, datas em UTC no banco e em `America/Sao_Paulo` na resposta, erros em
`ProblemDetails`.

**Documentação:** `Microsoft.AspNetCore.OpenApi` com **Scalar** em `/scalar`, endpoints agrupados por módulo
(tags Sensorial, Comunidade, Privacidade), comentários XML (`GenerateDocumentationFile`) e
`[ProducesResponseType]` em cada status. **CORS** liberado para o app em desenvolvimento.

## Chamadas a APIs REST externas
- **LLM** (módulo Privacidade): cliente tipado com `IHttpClientFactory`, tempo limite, resiliência
  (`Microsoft.Extensions.Http.Resilience`, `AddStandardResilienceHandler()`), chave em `Llm__Chave`. É a
  chamada REST que a rubrica de SOA cita e o "IA na remediação" de Cybersecurity, ao mesmo tempo.
- **ViaCEP** (opcional, módulo Sensorial): completa o endereço de um lugar novo pelo CEP.

## O consumidor MQTT
- `LeiturasMqttWorker : BackgroundService` com **MQTTnet**: conecta em `broker.hivemq.com:1883`, assina
  `calmaria/v1/lugares/+/leituras`, desserializa o JSON (contrato MQTT, Parte 3) e chama
  `ILeituraService.RegistrarAsync` num escopo novo (`IServiceScopeFactory`).
- Reconecta sozinho com espera crescente; mensagem inválida vai para o log com o motivo, **sem o payload**.
- Guarda as últimas 200 mensagens num buffer em memória para a varredura de privacidade conferir que nada
  pessoal passa pelo broker público.
- Configurável (`Mqtt:Host`, `Mqtt:Porta`, `Mqtt:Topico`, `Mqtt:Habilitado`).

## Testes (MSTest)
`Calmaria.Tests`:
- `CalculadoraIndicePonderada`: bordas, um caso por nível, entradas fora de faixa.
- `Leitura.Criar`: recusa som negativo e luz acima do limite.
- `Lugar.DistanciaAte`: distância conhecida entre duas estações (tolerância de 1%).
- `RefugioService`: escolhe o mais perto entre refúgio fixo e lugar `CALMO`; ignora `INTENSO`.
- `HorarioCalmoService`: com leituras montadas à mão, devolve as 3 janelas certas.
- `RelatoService`: nota 6 recusada; relato repetido em 30 minutos dá conflito; comentário com CPF gravado
  mascarado.
- Endpoints com `WebApplicationFactory` e SQLite em memória: `GET /api/lugares/999` dá 404; `DELETE
  /api/usuarios/{id}` apaga os relatos; a listagem pública não tem campo de autor.

`Calmaria.Privacidade.Tests`: ver [cybersecurity.md](cybersecurity.md).

## Integração com as outras partes
| Com | Como | Quem combina |
|---|---|---|
| ESP32 | MQTT, payload do contrato MQTT | Integrante 5 |
| ML | `POST /api/previsoes/importar` com o CSV de previsões | Integrante 3 |
| App | O contrato da API inteiro | Integrante 2 |
| VM | `dotnet publish -r linux-x64`, pacote de migração (`dotnet ef migrations bundle`), configuração por variável de ambiente, `/health` | Integrante 4 |

## Evidências para o README
Print do Scalar com os três módulos; uma leitura chegando pelo MQTT e aparecendo em `GET /api/lugares/{id}`;
um relato com CPF gravado como `[removido]`; as tabelas no PostgreSQL; os testes passando; os comandos para
rodar do zero (`dotnet ef database update` e `dotnet run --project src/Calmaria.Api`).

## Ordem de construção
| Dia | O que fica pronto |
|---|---|
| Qui 12/11, à noite | Solução e os quatro projetos; `Domain` do módulo Sensorial; `DbContext` com PostgreSQL; migração `Inicial_Sensorial`; seed dos lugares; `GET /api/lugares` e `GET /api/lugares/{id}`. Mandar o IP e a porta ao Integrante 2 |
| Sex 13/11 | Aula: dúvidas aos professores de SOA e de Cybersecurity |
| Sáb 14/11 | Leituras, índice, worker MQTT (teste com o ESP32 do Integrante 5), horários calmos, refúgio, `/api/refugios`, `/health` |
| Dom 15/11 | Módulo Comunidade: entidades, migração, usuários, perfil, relatos, resumo e exclusão, com os testes |
| Seg 16/11 | Módulo Privacidade: regras, `Sanitizador`, tipos de taint, log com redação, migração. Previsões (importar o CSV do Integrante 3). Pacote `linux-x64` e `efbundle` para o Integrante 4 |
| Ter 17/11 | Varreduras, tradutor por IA, painel, Semgrep; Scalar com comentários XML; README com o mapa da rubrica |
| Qua 18/11 | Evidências de C#, SOA e Cybersecurity; ZIP e vídeo de Cybersecurity; tudo congelado às 20h |
| Qui 19/11 | Revisão final e envio das três disciplinas até 21h |

**Se o tempo apertar,** a ordem de corte é: ViaCEP, `FiltroRespostaPublica`, `FonteCodigo`; nunca o básico de
cada módulo.


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
| PA-03 | Favicon | Luiz | SVG 32 × 32 | `design/marca/favicon.svg`, servido pela API. |
| AD-01 | Scalar com o tema | Luiz | CSS da API | Título "API Calmaria", acento #3D5A73 (#B2C9D8 no escuro), fonte Atkinson. |
| AD-02 | Diagrama da arquitetura | Luiz | SVG e PNG 1920 × 1080 | Caixas de raio 10, traço 1,5; API em #E7EEF3 com borda #3D5A73; VM tracejada; protocolo escrito em cada seta. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |

**Scalar:** título "API Calmaria" e um CSS customizado em `wwwroot/scalar-calmaria.css` com
`--scalar-color-accent: #3D5A73` (no `.light-mode`) e `#B2C9D8` (no `.dark-mode`) e
`--scalar-font: "Atkinson Hyperlegible Next"` (conferir os nomes das variáveis na versão instalada). O
painel de Cybersecurity usa `design/tokens/tokens.css`.

## Checklist de envio (C#)
- [ ] README de `api-dotnet/` com descrição, como rodar (PostgreSQL e variáveis), endpoints, integrantes e a
  Ficha (ou link para ela)
- [ ] Migrações no repositório
- [ ] Scalar abrindo em `/scalar`
- [ ] `dotnet test` passando nos dois projetos de teste
- [ ] Nenhuma senha ou chave no repositório
- [ ] Evidências em `api-dotnet/docs/`
- [ ] Prompts de IA registrados em `prompts.md`
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

### Contrato: o que a API precisa para rodar (VM)

**O que o Luiz entrega para a VM** (até segunda, 16/11; antes disso, usar o substituto abaixo):
- `calmaria-api-linux-x64.zip`: a API publicada com `dotnet publish -c Release -r linux-x64
  --self-contained false`. Entrada: `Calmaria.Api.dll`.
- `efbundle`: o pacote de migrações do EF Core (`dotnet ef migrations bundle -r linux-x64`), que cria as
  tabelas no PostgreSQL com `./efbundle --connection "<string de conexão>"`.

**O que a VM precisa ter:** runtime do ASP.NET Core 10 (`dotnet-install.sh --channel 10.0 --runtime
aspnetcore`) e PostgreSQL (16 ou mais novo), escutando só em `localhost`.

**Variáveis de ambiente da API** (arquivo `/etc/calmaria/api.env`):

| Variável | Exemplo | Para quê |
|---|---|---|
| `ASPNETCORE_URLS` | `http://127.0.0.1:5080` | Porta interna; o nginx publica na 80 |
| `ConnectionStrings__Calmaria` | `Host=localhost;Database=calmaria;Username=calmaria;Password=<gerada>` | Banco |
| `Mqtt__Habilitado` | `true` | Liga o recebimento das leituras do ESP32 |
| `Privacidade__ChaveAdmin` | `<gerada>` | Chave das rotas administrativas |
| `Llm__Chave` | `<se houver>` | Chave do LLM; sem ela, a API usa frases prontas |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente |

**Como saber que está no ar:** `curl -fsS http://127.0.0.1:5080/health` devolve `Healthy`;
`curl http://api.calmaria.local/api/lugares` devolve a lista de lugares.

**Substituto até a API chegar:** uma API mínima criada na própria VM com `dotnet new web -o placeholder`, com
uma rota `/health` que responde `Healthy` e uma `/api/lugares` que responde `[]`. Todos os scripts (systemd,
nginx, monitoramento, teste de falha) funcionam igual com ela; na segunda é só trocar o pacote.

---

# Parte 4: com quem falar

- **Integrante 2 (app):** mandar o IP e a porta da API assim que `GET /api/lugares` funcionar; avisar qualquer mudança de contrato.
- **Integrante 3 (ML):** receber o `previsao_barulho.csv` até segunda, 16/11.
- **Integrante 4 (VM e Testing):** entregar o pacote `linux-x64` e o `efbundle` até segunda, 16/11; passar a chave
  de teste e os prints do Scalar.
- **Integrante 5 (ESP32 e pitch):** testar juntos o MQTT no sábado; gravar o trecho da API para o pitch até quarta,
  18/11, às 20h.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
