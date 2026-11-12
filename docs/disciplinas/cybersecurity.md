# Cybersecurity: o dado de saúde não pode vazar

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Cybersecurity |
| Responsável | Luiz |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `api-dotnet/src/Calmaria.Privacidade/` (o módulo) e `security/` (evidências e o ZIP) |
| Você depende de | A API descrita em `csharp.md` |
| Dependem de você | Integrante 4 (test cases de privacidade) |
| Vídeo | Sim: a atividade, com o painel gerando o Risco Cidadão |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: engenheiro de segurança de aplicações .NET, com LGPD, taint analysis e Semgrep.

Vamos construir o módulo de privacidade da API Calmaria em C#: o motor de PII, o taint analysis, a tradução do risco por IA e o painel Risco Cidadão. O plano técnico da API está em `csharp.md`, que eu também vou te mandar.

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
"A Missão Técnica", em três frentes:
1. **Mapeamento de PII:** configurar novas regras no **motor de parsing** para detectar dados sensíveis
   (CPF, saúde, senhas).
2. **Taint analysis:** rastrear o fluxo das informações coletadas e bloquear vazamentos em saídas inseguras.
3. **Tradução via IA:** usar um LLM para traduzir falhas técnicas em alertas claros de risco real ao
   cidadão.

**Entregáveis (um arquivo ZIP):** o código do novo módulo com as regras de PII; as evidências do taint
analysis em ação ("Auditoria do Protótipo"); vídeo com a atividade e print do painel gerando o indicador de
**Risco Cidadão**. **Envio:** Teams até 19/11.

| Critério | Pontos | Onde está |
|---|---|---|
| Robustez do motor de parsing de PII | 3,0 | `Calmaria.Privacidade/Regras` e `Motor`, com os testes |
| Eficácia do taint analysis | 3,0 | `Calmaria.Privacidade/Taint`, os guardas de saída na API e as regras do Semgrep |
| Inovação com IA na remediação | 2,0 | `Calmaria.Privacidade/Remediacao` |
| Dashboard executivo (Risco Cidadão) | 2,0 | `Calmaria.Privacidade/Risco` e o painel em `/seguranca` |

> **Primeira coisa a resolver (sexta, 13/11):** o slide fala em "novo módulo", "motor de parsing" e
> "dashboard **atualizado**", o que sugere uma base da aula. **Se existir**, as regras deste arquivo entram
> nela e a API Calmaria vira o "protótipo auditado"; os guardas de saída dentro da API continuam valendo como
> o bloqueio do taint. **Se não existir**, o módulo abaixo é o motor.

## Por que isto importa na Calmaria
Dizer que uma pessoa é autista é **dado pessoal sensível referente à saúde** (LGPD, art. 5º, II), com
tratamento restrito (art. 11). E a Calmaria tem saídas perigosas: um **broker MQTT público**, logs de
servidor, relatos publicados para todos e o próprio LLM, que é um serviço externo. Um vazamento expõe uma
pessoa vulnerável, a rotina dela e o telefone de quem cuida dela.

| Dado | Onde aparece | Categoria | Peso |
|---|---|---|---|
| Diagnóstico e saúde ("sou autista", "TEA", "CID F84", "laudo", remédio, "tive uma crise") | Comentário do relato | Sensível, saúde (LGPD art. 5º, II) | 5 |
| CPF | Comentário do relato | Pessoal identificador | 4 |
| Telefone e nome do contato de apoio | Perfil sensorial | Pessoal de terceiro | 3 |
| Localização precisa com data e hora | Botão de crise | Pessoal; revela onde a pessoa está vulnerável | 4 |
| CEP junto com data de nascimento | Base pública da CIPTEA, se alguém juntar ao sistema | Quase identificador | 3 |
| E-mail | Comentário do relato | Pessoal | 2 |
| Senhas, tokens, strings de conexão, chaves de API | Código e configuração | Segredo; risco para todos | 5 |

## O módulo

```
api-dotnet/src/Calmaria.Privacidade/
├── Regras/         IRegraPii, RegraBase, RegraCpf, RegraTelefone, RegraEmail, RegraSaude,
│                   RegraSegredo, RegraLocalizacaoPrecisa, RegraQuaseIdentificador, regras-pii.json
├── Motor/          IMotorPii, MotorPii, Achado, CategoriaPii, Severidade
├── Taint/          TextoNaoConfiavel, TextoSanitizado, AchadoMascarado, ISanitizador, Sanitizador,
│                   IRastreadorDeFluxo, EventoDeFluxo (origem, destino, bloqueado)
├── Varredura/      IFonteVarredura, FonteLogs, FonteRelatos, FonteMqtt, FonteCodigo, ServicoVarredura
├── Remediacao/     ITradutorDeRisco, TradutorLlm, TradutorModelo, AlertaCidadao, prompt-alerta.md
└── Risco/          ICalculadoraRiscoCidadao, CalculadoraRiscoCidadao

api-dotnet/src/Calmaria.Api/Privacidade/
├── PrivacidadeController.cs          Os endpoints /api/privacidade/*
├── LoggerProviderComRedacao.cs       Guarda de saída do log
└── FiltroRespostaPublica.cs          Guarda de saída das respostas públicas
api-dotnet/src/Calmaria.Api/wwwroot/seguranca/index.html   O painel

security/
├── semgrep/calmaria-taint.yaml       Regras de taint estático para C#
├── exemplos/                         Um controller vulnerável e o mesmo corrigido (fora da solução)
├── relatorios/                       JSON de cada varredura e do Semgrep (antes e depois)
├── evidencias/                       Prints
└── gerar-zip.ps1                     Monta o ZIP da entrega
```

`Calmaria.Privacidade` não depende de ASP.NET nem do banco: recebe texto e devolve achados. Por isso vai
sozinho no ZIP e tem testes próprios, e por isso conta também como POO para a disciplina de C#.

## 1. Motor de parsing de PII (3 pontos)
- **Cada regra é uma classe** que implementa `IRegraPii` (`Id`, `Categoria`, `Peso`,
  `IEnumerable<Achado> Avaliar(string texto)`), herdando o comum de `RegraBase`. O `MotorPii` recebe todas as
  regras por injeção de dependência e roda cada uma.
- **Regras novas sem recompilar:** `regras-pii.json` traz padrão, peso, palavras de contexto e exemplos;
  a `RegraConfiguravel` lê esse arquivo. É o "configurar novas regras no motor" do slide.
- Regras mínimas:
  - **CPF** com e sem pontuação, **validado pelo dígito verificador** (corta falso positivo).
  - **Telefone** brasileiro, fixo e celular, com e sem DDD e `+55`.
  - **E-mail.**
  - **Saúde:** termos e variações ("autista", "autismo", "TEA", "espectro", "CID F84", "F84.0", "laudo",
    "neuro", "crise", remédios comuns no tratamento), sem diferenciar maiúscula e acento, com contexto para
    não marcar "crise econômica".
  - **Segredos:** `Password=` e `Pwd=` em string de conexão, `senha=`, `api_key`, `Bearer ` seguido de
    token, prefixos conhecidos de chave (`sk-`, `AKIA`).
  - **Localização precisa:** latitude e longitude com 4 casas decimais ou mais perto de um identificador de
    usuário.
  - **Quase identificador:** CEP e data de nascimento no mesmo registro.
- **Cada achado** traz regra, categoria, peso, posição e o **trecho já mascarado** (`529.***.***-25`,
  `<SAUDE>`). O texto original nunca sai do motor.

**Robustez (o que o critério mede):**
- Testes com `[DataRow]` (MSTest): pelo menos 3 positivos e 3 negativos por regra.
- Uma amostra rotulada de 60 comentários fictícios (`AmostraRotulada.json`) e um teste que calcula e imprime
  **precisão e revocação** por regra. Esse número vai no vídeo.

## 2. Taint analysis (3 pontos)
**Origens (dado do cidadão entrando):** o comentário do relato e o contato de apoio do perfil (corpo das
requisições) e a latitude e longitude do botão de crise.

**Destinos inseguros:** o log da API; as respostas de endpoints públicos (a listagem de relatos); a chamada ao
LLM externo; o broker MQTT público; arquivos exportados.

**Sanitizador:** `Sanitizador.Sanitizar(TextoNaoConfiavel)`, que roda o motor, mascara e devolve
`TextoSanitizado`; para coordenadas, o arredondamento para 3 casas (cerca de 100 metros).

**Em três camadas:**
1. **Pelo tipo, em tempo de compilação.** O dado que entra vira `TextoNaoConfiavel` no próprio DTO. Os
   destinos só aceitam `TextoSanitizado`: o `Relato` só grava `TextoSanitizado`, o `RelatoPublicoDto` só
   recebe `TextoSanitizado` e o `ITradutorDeRisco` só recebe `AchadoMascarado`. O único caminho de um para o
   outro é o `Sanitizador`. Quem tentar gravar ou mandar ao LLM o texto cru **não compila**. Essa é a parte de
   "inovação" a destacar: o rastreamento do dado contaminado está no sistema de tipos.
2. **Na saída, em tempo de execução.** O `LoggerProviderComRedacao` embrulha o provedor de log: toda mensagem
   passa pelo motor antes de ser escrita; se tiver PII, sai mascarada e o `RastreadorDeFluxo` registra um
   `EventoDeFluxo` (origem, destino `LOG`, bloqueado). O `FiltroRespostaPublica` faz o mesmo no JSON das rotas
   públicas. Opcional: o pacote `Microsoft.Extensions.Compliance.Redaction` do próprio .NET, que mascara no
   log as propriedades classificadas como sensíveis.
3. **Estático, no código.** Regras do Semgrep em modo taint (`mode: taint`, com `pattern-sources`,
   `pattern-sinks` e `pattern-sanitizers`) para C#: origem em parâmetros `[FromBody]`; destino em
   `_logger.Log*`, `Console.WriteLine` e no `HttpClient` do LLM; sanitizador `Sanitizar(...)`. Rodar em
   `api-dotnet/` e em `security/exemplos/`, salvando o JSON em `security/relatorios/`.

**Varreduras:** o `ServicoVarredura` roda as fontes, cada uma uma classe de `IFonteVarredura`:
`FonteLogs` (os arquivos de log da API), `FonteRelatos` (os comentários gravados), `FonteMqtt` (as últimas
200 mensagens recebidas do broker) e `FonteCodigo` (os arquivos do repositório, atrás de segredo). O resultado
é gravado (tabelas `Varredura`, `Achado`, `EventoDeFluxo`, na migração `Privacidade`).

**A evidência que convence (a "Auditoria do Protótipo"):**
1. `security/exemplos/RelatoControllerVulneravel.cs`, que loga o comentário cru: o Semgrep acusa a origem e o
   destino. A versão corrigida, com o `Sanitizador`: o Semgrep não acusa.
2. A mesma linha colada dentro da API **não compila**, por causa dos tipos (print do erro).
3. Em execução: um relato com CPF e "meu filho é autista" no comentário. O banco, o log e a listagem pública
   mostram `[removido]` e `<SAUDE>`; o painel registra o fluxo bloqueado.
4. A varredura sobre o código real e os logs, com o resultado.

## 3. Tradução via IA (2 pontos)
- `TradutorLlm` chama o LLM por REST com `HttpClient` tipado (a mesma chamada que conta para SOA), com tempo
  limite e resiliência.
- **Entrada só mascarada:** o método recebe `AchadoMascarado`, então o dado real não chega ao serviço externo
  (o LLM também é um destino inseguro, e isso vale mostrar no vídeo).
- **Prompt versionado** em `prompt-alerta.md` (recurso embutido), com a persona da Parte 1 e o pedido de
  linguagem literal, curta e sem alarme. Duas saídas por achado:
  - **para o cidadão:** "Seu comentário tinha seu CPF. Ele ficaria visível para qualquer pessoa no mapa. Nós
    tiramos o CPF antes de salvar.";
  - **para a equipe:** a correção sugerida no código.
- Resposta guardada com o achado (para o painel) e em cache por regra e destino, para não pagar a mesma
  pergunta duas vezes.
- **Sem chave ou sem internet,** o `TradutorModelo` usa frases prontas por categoria e o painel indica isso.
- Qualquer LLM com API REST serve; a chave vem de `Llm__Chave`, nunca do repositório. O Ollama, rodando local,
  funciona sem chave.

## 4. Painel Risco Cidadão (2 pontos)
**O indicador** (`CalculadoraRiscoCidadao`):
```
risco de um achado  = peso do dado x peso da exposição x (1 se não mitigado, 0,2 se mitigado)
peso da exposição   = 3 (broker público, resposta pública, LLM externo), 2 (log), 1 (banco)
Risco Cidadão       = 100 x soma dos riscos / soma dos riscos máximos possíveis   (0 a 100)
```
Faixas: 0 a 20 baixo, 21 a 50 médio, 51 a 100 alto.

**Endpoints** (todos com o cabeçalho `X-Admin-Key`, chave vinda de variável de ambiente; sem ela, 401):
`POST /api/privacidade/varreduras` (roda), `GET /api/privacidade/risco-cidadao` (indicador e composição),
`GET /api/privacidade/achados`, `GET /api/privacidade/fluxos`.

**O painel** em `/seguranca/index.html`, servido pela própria API: HTML com Chart.js por CDN, sem etapa de
build. Mostra o Risco Cidadão em destaque, o antes e o depois da correção, achados por categoria e por
destino, os fluxos bloqueados, os alertas traduzidos pela IA e a hora da última varredura.

## O ZIP da entrega
`security/gerar-zip.ps1` monta `calmaria-cybersecurity.zip` com: o projeto `Calmaria.Privacidade` e o de
testes; os três arquivos da API em `Privacidade/` e o painel; `security/semgrep`, `exemplos`, `relatorios` e
`evidencias`; um README com como rodar; o link do vídeo. **Conferir que não há chave dentro**: rodar a
`FonteCodigo` sobre o próprio ZIP antes de enviar.

## Roteiro do vídeo
1. Por que dado de saúde de uma pessoa autista é o pior dado para vazar (30 s).
2. As regras de PII, os testes e a precisão e revocação (1 min).
3. O taint: o Semgrep pegando o exemplo vulnerável, o erro de compilação dentro da API e a correção (1 min
   30 s).
4. O relato com CPF em execução, mascarado no banco, no log e na resposta (40 s).
5. A IA: o achado mascarado indo ao LLM e o alerta ao cidadão voltando (40 s).
6. O painel: o Risco Cidadão antes e depois (40 s).

## Ordem de construção
1. **Obrigatório (segunda, 16/11):** saber se há base da aula; `IRegraPii`, as regras e os testes.
2. **Obrigatório (segunda):** `TextoNaoConfiavel`, `TextoSanitizado` e o `Sanitizador` ligados ao módulo
   Comunidade; `LoggerProviderComRedacao`.
3. **Obrigatório (terça):** varreduras, migração `Privacidade`, tradutor por IA, endpoints e painel.
4. **Obrigatório (quarta, até 20h):** Semgrep com os exemplos, evidências, ZIP.
5. **Opcional:** `FiltroRespostaPublica`; `RegraQuaseIdentificador` aplicada à base pública da CIPTEA,
   mostrando o risco de juntar CEP e nascimento.


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
| AP-13 | Selo de nível | Integrante 2 e Luiz | 32 pt de altura, pílula | Forma de 16 e texto de 16 ("Calmo agora"). O mesmo selo no painel. |
| PA-01 | Painel Risco Cidadão | Luiz | 1440 × 900; conferir em 1280 e 768 | `wwwroot/seguranca/index.html` com `design/tokens/tokens.css`; segue o tema do sistema. |
| PA-02 | Print do painel | Luiz | PNG 2880 × 1800 (2x) | Tema claro, antes e depois visíveis, selo "Dados de exemplo" se for seed. |
| PA-03 | Favicon | Luiz | SVG 32 × 32 | `design/marca/favicon.svg`, servido pela API. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |

**Painel:** inclua `design/tokens/tokens.css` (servido em `wwwroot/`) e use só as variáveis dele, como
`var(--color-bg-canvas)`, `var(--color-primary-solid)` e `var(--color-nivel-intenso-solid)`; o escuro troca
sozinho pelo tema do sistema. No Chart.js: `Chart.defaults.animation = false`, fonte Atkinson, barras numa cor
só (`--color-primary-solid`), faixas do Risco Cidadão nas cores dos níveis. O indicador é uma faixa de 0 a 100
com as três bandas e as marcas de antes e depois, não um mostrador redondo.

## Checklist de envio (o ZIP)
- [ ] Código do módulo com as regras de PII e os testes
- [ ] Evidências do taint analysis: Semgrep antes e depois, erro de compilação, relato mascarado
- [ ] Vídeo da atividade
- [ ] Print do painel com o indicador de Risco Cidadão
- [ ] README do módulo com como rodar
- [ ] Nenhuma chave de API dentro do ZIP
- [ ] Enviado no Teams por um integrante

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

- **Integrante 4 (Testing):** os test cases CT16, CT24, CT26 e CT27 são de privacidade; passar a chave de teste e o print do painel.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
