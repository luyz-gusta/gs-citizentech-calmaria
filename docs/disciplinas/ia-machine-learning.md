# IA e Machine Learning: quando e onde a cidade fica mais barulhenta

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Inteligência Artificial e Machine Learning |
| Responsável | Integrante 3 |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `ml/` |
| Você depende de | Ninguém: os dados são públicos |
| Dependem de você | Luiz (importa o CSV de previsões na API, até segunda 16/11) |
| Vídeo | Sim: até 5 minutos, no YouTube, não listado |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: cientista de dados sênior (Python, pandas, scikit-learn, XGBoost, matplotlib) que explica o porquê de cada passo.

Vamos fazer o notebook de ML da Calmaria com dados reais do SP156: prever quantas reclamações de barulho cada distrito de São Paulo terá em cada dia. O notebook precisa rodar de ponta a ponta e gerar o CSV de previsões no formato exato da Parte 3. Dado simulado zera a nota.

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
- **Google Colab** é o caminho mais simples (nada para instalar e o download dos arquivos de 100 MB é rápido
  lá). Alternativa: Python 3.11 ou mais novo com Jupyter ou VS Code.
- No fim, baixar o notebook executado (`.ipynb` com as saídas) e o CSV de saída para a pasta `ml/` do
  repositório.

## O que a disciplina pede
Transformar o problema da CitizenTech numa pergunta de ML e respondê-la com **dados reais e públicos** da
cidade, percorrendo o ciclo inteiro: preparar os dados, treinar um modelo (XGBoost, Random Forest, MLP,
LSTM ou KMeans com PCA) com hiperparâmetros diferentes, avaliar em treino e teste e concluir o que o
resultado significa para o cidadão. **Dado simulado não é aceito.**

**Entregáveis:** pasta `ml/` com o notebook executado, `dados/README.md` e `requirements.txt`; o
`RELATORIO_ML.md`, de 2 a 4 páginas; vídeo de até 5 minutos no YouTube, não listado. **Envio:** links do
repositório e do vídeo no Teams até 19/11, 23h55.

| Critério | Peso | Como cumprir |
|---|---|---|
| Problema e dados reais | 2,0 | Pergunta clara, base do SP156 com fonte, link e período |
| Escolha justificada do modelo | 1,0 | Por que regressão, por que árvores, comparação com uma linha de base |
| Preparação e divisão treino/teste | 2,0 | Limpeza, agregação, atributos e divisão por tempo, sem vazamento |
| Treino com parâmetros diferentes | 2,0 | Pelo menos 10 combinações de 3 hiperparâmetros, validação cruzada só no treino |
| Resultados em treino e teste | 2,0 | Métricas lado a lado, gráficos, discussão de overfitting |
| Conclusão e leitura para o cidadão | 1,0 | O que a previsão muda para a Marina e para a Carla (personas da Parte 1) |

## A pergunta
**Quantas reclamações de barulho (PSIU) cada distrito de São Paulo deve receber em cada dia, e esse dia
será de barulho baixo, médio ou alto para aquele distrito?**

É uma **regressão** (prever a contagem por distrito e dia), e a contagem prevista vira um nível pelos
tercis do próprio distrito. O app usa o nível: "Previsão de barulho na Consolação no sábado: alta".

### Por que não "em que hora"
A base do SP156 traz **só a data de abertura da reclamação, sem a hora**. Então o ML responde **onde e em
que dia**; a hora do dia vem das leituras dos sensores, na API .NET. Dizer isso no relatório é ponto a
favor, porque mostra que a equipe conhece o próprio dado.

## Os dados
**Fonte:** Portal de Dados Abertos da Prefeitura de São Paulo, conjunto **"Dados do SP156 -
Solicitações"**: https://dados.prefeitura.sp.gov.br/dataset/dados-do-sp156

- Um CSV por trimestre (cerca de 100 MB cada), de 2020 até o 3º trimestre de 2026, mais o glossário
  "Dicionário dados abertos - SP156 2020 em diante".
- **Colunas:** Data de Abertura, Data de Finalização, Meio de Comunicação, Tema, Assunto, Serviço, Status,
  Órgão, Logradouro, Número, CEP, Latitude, Longitude, Prefeitura Operacional (subprefeitura), Distrito.
- **O filtro:** `Assunto == "Poluição Sonora - PSIU"` (tema "Rua e bairro").
- **O que já se sabe do 3º trimestre de 2026:** 484.232 solicitações, das quais **14.178 são de PSIU**, todas
  com latitude e longitude; 96 distritos; sábado e domingo com cerca de 2.950 cada, contra 1.376 na terça;
  média de 2,6 reclamações por distrito por dia, máximo de 51.

### Cuidados que já apareceram
- **Codificação:** o arquivo não abre como UTF-8 puro; ler com `encoding="latin-1"` e renomear as colunas
  pela posição, porque os acentos do cabeçalho vêm quebrados.
- **Formato da data:** no 3º trimestre de 2026 a data vem como `MM/DD/AAAA` (`07/01/2026` é 1º de julho).
  Conferir em cada arquivo antes de juntar e usar `format=` explícito no `pd.to_datetime`.
- **Tamanho:** ler em pedaços (`chunksize=200_000`), filtrar o PSIU e salvar só o filtrado em
  `ml/dados/psiu_filtrado.csv` (poucos MB). Os CSV brutos ficam fora do Git (`.gitignore`).
- **Sem dado pessoal:** a base não identifica o reclamante; mesmo assim, o modelo usa só distrito e data,
  não o endereço exato.

### Período sugerido
Oito trimestres: do 4º trimestre de 2024 ao 3º de 2026 (cerca de 110 mil reclamações). Antes de juntar,
olhar o total mensal num gráfico e procurar quebras de série (mudança de canal, de formulário ou de
classificação); se houver, cortar o período ou criar um atributo para ela, e explicar no relatório.

### `dados/README.md`
Fonte, link de cada arquivo usado, período, data do download, o filtro aplicado, as colunas que ficaram e os
cuidados acima.

## Preparação
1. Juntar os trimestres filtrados; converter a data; descartar linhas sem distrito.
2. **Agregar** por `distrito` e `data`: a contagem de reclamações é o alvo. **Completar com zero** os dias
   sem reclamação (produto cartesiano de distritos e datas): dia sem reclamação é informação.
3. **Atributos:**
   - calendário: dia da semana, fim de semana, mês, dia do mês, feriado (lista de feriados nacionais e de São
     Paulo no período), véspera de feriado;
   - distrito: codificado (one-hot, ou a média histórica do distrito **calculada só no treino**);
   - subprefeitura do distrito;
   - opcional e real: temperatura máxima e chuva do dia, da estação do INMET em São Paulo (Mirante de
     Santana), que pode explicar festas e janelas abertas em dia quente.
   Ficam de fora atributos de "ontem" e "semana passada": o modelo precisa prever dias futuros, para os quais
   o SP156 ainda não publicou dados.
4. **Divisão por tempo, nunca aleatória:** treino do 4º trimestre de 2024 ao 1º de 2026; teste do 2º e do 3º
   trimestres de 2026. Divisão aleatória misturaria dias vizinhos entre treino e teste e daria um resultado
   bom demais.

## Modelos
- **Linha de base (sem ML):** média histórica do distrito naquele dia da semana. O modelo só vale se for
  melhor que ela.
- **Modelo principal: XGBoost** (`XGBRegressor`), com `objective="count:poisson"` como opção a testar, por
  ser contagem.
- **Modelo de comparação: Random Forest** (`RandomForestRegressor`).

**Por que árvores:** os atributos são categóricos e de calendário, com interações (o sábado pesa mais na
Consolação que num distrito residencial), e árvores captam isso sem escalar os dados. MLP e LSTM pedem mais
dados e mais tempo do que a equipe tem; KMeans responde outra pergunta (agrupar distritos). Esse parágrafo
vai no relatório.

## Treino com hiperparâmetros
- `GridSearchCV` ou `RandomizedSearchCV` com **`TimeSeriesSplit(n_splits=5)`**, aplicado **só no treino**.
- XGBoost: `n_estimators` (100, 300, 600), `max_depth` (3, 5, 8) e `learning_rate` (0,03; 0,1; 0,3): 27
  combinações.
- Random Forest: `n_estimators` (100, 300), `max_depth` (8, 16, None) e `min_samples_leaf` (1, 5, 20): 18
  combinações.
- Mostrar a tabela de todas as combinações com o erro médio da validação cruzada (`cv_results_`).

## Avaliação
- **Métricas:** MAE (o erro médio em número de reclamações, fácil de explicar), RMSE e R².
- **Tabela de treino e teste lado a lado** para a linha de base, o Random Forest e o XGBoost.
- **Gráficos:** previsto contra real no teste para 3 distritos (Consolação, Bela Vista e um residencial);
  importância dos atributos; erro por dia da semana.
- **Overfitting:** se o MAE do treino for muito menor que o do teste, mostrar o que reduziu a diferença
  (profundidade menor, mais `min_samples_leaf`).
- **Acerto do nível:** converter previsto e real em BAIXO, MEDIO e ALTO e mostrar a matriz de confusão: é o
  que o app usa.

## A saída para a API (contrato do CSV, na Parte 3)
O notebook termina gerando `ml/saida/previsao_barulho.csv` para os distritos dos lugares do protótipo
(Consolação e Bela Vista, no mínimo) e para os dias de 16/11/2026 a 13/12/2026, com as colunas `distrito`,
`data`, `reclamacoes_previstas` e `nivel`. O Luiz importa pela rota `POST /api/previsoes/importar`.

## `RELATORIO_ML.md` (2 a 4 páginas)
1. O problema e a pergunta de ML.
2. Os dados: fonte, período, filtro, limpeza e limitações (sem hora; reclamação mede incômodo relatado, não
   decibéis).
3. O modelo e por que ele.
4. A preparação e a divisão por tempo.
5. O treino e a tabela de hiperparâmetros.
6. Os resultados de treino e teste, com os gráficos.
7. **A leitura para o cidadão:** por exemplo, "na Consolação, sábado e domingo têm previsão de barulho alto;
   para quem é sensível a som, o melhor dia para resolver coisas na região é terça ou quarta". Usar os
   números que o modelo realmente produzir.

## Vídeo (até 5 minutos, YouTube, não listado)
O problema da Calmaria (30 s); os dados e o filtro (1 min); a preparação e a divisão por tempo (1 min); o
treino e os hiperparâmetros (1 min); os resultados e o overfitting (1 min); o que muda para o cidadão e a
previsão aparecendo no app (30 s).

## Estrutura da pasta
```
ml/
├── calmaria_ml.ipynb          executado, com as saídas visíveis
├── requirements.txt           pandas, numpy, scikit-learn, xgboost, matplotlib, seaborn, jupyter
├── dados/
│   ├── README.md
│   └── psiu_filtrado.csv
├── saida/
│   └── previsao_barulho.csv
└── RELATORIO_ML.md
```

## Ordem de construção
1. **Obrigatório:** baixar 2 trimestres, filtrar, entender as colunas e a data (sábado e domingo, 14 e 15/11).
2. **Obrigatório:** juntar os 8 trimestres, agregar, completar com zero, criar os atributos.
3. **Obrigatório:** linha de base, Random Forest e XGBoost com a busca de hiperparâmetros.
4. **Obrigatório:** métricas, gráficos, CSV de saída, relatório e vídeo.
5. **Opcional:** clima do INMET; mapa de calor dos distritos com as coordenadas; KMeans com PCA para agrupar
   os distritos pelo perfil semanal de barulho, como análise extra.

## Detalhes de implementação

### Os arquivos (links diretos de download, conjunto "Dados do SP156 - Solicitações")
| Trimestre | Link do CSV |
|---|---|
| 4º 2024 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/ed357147-f15c-4bf0-ad15-edc963a9f130/download/arquivo-final-4-trim.-2024.csv |
| 1º 2025 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/7c5aeb58-2cad-40ba-9e9e-af1c17cc5070/download/arquivo-final-1-trim-25.csv |
| 2º 2025 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/f5df7f4a-d614-4f85-b4a0-263ec9370cd9/download/arquivo-final-2-trim-25.csv |
| 3º 2025 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/3b1d2648-d586-40c8-819c-24fbb8005343/download/arquivo-final-3-trim-25.csv |
| 4º 2025 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/d3a98097-8128-45d4-b8fe-21874b81c971/download/arquivo-final-4-trim-25.csv |
| 1º 2026 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/75105148-4efd-48f3-8ae0-fc6c2adf7060/download/arquivo-final-1-trim-26.csv |
| 2º 2026 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/368eb32d-ef25-4441-be8f-fce0146575a1/download/arquivo-final-2-trim-26.csv |
| 3º 2026 | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/f67fcc12-8da4-485f-92a3-d2af40cc771e/download/arquivo-final-3-trim-26.csv |

Glossário das colunas: "Dicionário dados abertos - SP156 2020 em diante", no mesmo conjunto
(https://dados.prefeitura.sp.gov.br/dataset/dados-do-sp156).

### Como ler sem estourar a memória
As 15 colunas vêm nesta ordem: Data de Abertura, Data de Finalização, Meio de Comunicação, Tema, Assunto,
Serviço, Status, Órgão, Logradouro, Número, CEP, Latitude, Longitude, Prefeitura Operacional, Distrito.

```python
import pandas as pd

COLUNAS = ["data_abertura", "data_finalizacao", "meio", "tema", "assunto", "servico", "status", "orgao",
           "logradouro", "numero", "cep", "latitude", "longitude", "subprefeitura", "distrito"]

def filtra_psiu(caminho):
    partes = []
    for pedaco in pd.read_csv(caminho, encoding="latin-1", header=0, names=COLUNAS, dtype=str,
                              chunksize=200_000, on_bad_lines="skip"):
        partes.append(pedaco[pedaco["assunto"].str.contains("PSIU", na=False)])
    return pd.concat(partes)
```
Depois, converter a data com o formato conferido em cada arquivo (no 3º trimestre de 2026 é `%m/%d/%Y`) e
guardar só `data_abertura`, `distrito`, `subprefeitura`, `latitude` e `longitude`.

### Feriados
O pacote `holidays` resolve os nacionais e os do estado (`holidays.Brazil(subdiv="SP", years=[2024, 2025,
2026])`). Somar à mão os municipais de São Paulo: 25 de janeiro (aniversário da cidade) e Corpus Christi.

### As seções do notebook
1. Problema e pergunta de ML.
2. Os dados: download, filtro do PSIU, conferência da data, gráfico do total mensal.
3. Exploração: reclamações por dia da semana, por mês e os 15 distritos com mais reclamações.
4. Preparação: agregação por distrito e dia, preenchimento com zero, atributos, divisão por tempo.
5. Linha de base.
6. Random Forest com a busca de hiperparâmetros (tabela de `cv_results_`).
7. XGBoost com a busca de hiperparâmetros.
8. Resultados: tabela de treino e teste, gráficos, importância dos atributos, matriz de confusão do nível.
9. Overfitting.
10. Conclusão para o cidadão.
11. Geração do `previsao_barulho.csv`.

### `requirements.txt`
`pandas`, `numpy`, `scikit-learn`, `xgboost`, `matplotlib`, `seaborn`, `holidays`, `jupyter`, com as versões que
rodaram (`pip freeze` filtrado).


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
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| ML-01 | Estilo dos gráficos | Integrante 3 | matplotlib, 10 × 5 pol a 150 dpi | 1500 × 750 px; rcParams abaixo; título diz a conclusão. |
| ML-02 | Total mensal de reclamações | Integrante 3 | 1500 × 750 px | Linha na cor da marca; quebras de série anotadas com texto. |
| ML-03 | Previsto contra real | Integrante 3 | 1500 × 750 px por distrito | Real #4D545B contínuo, previsto #3D5A73 tracejado; 3 distritos, mesma escala no eixo y. |
| ML-04 | Importância dos atributos | Integrante 3 | 1500 × 750 px | Barras horizontais de uma cor, ordenadas, valor na ponta, nomes legíveis. |
| ML-05 | Matriz de confusão | Integrante 3 | 900 × 900 px | Escala da marca, número em cada célula, BAIXO, MEDIO e ALTO nos eixos. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |

**Estilo dos gráficos (cole no início do notebook):**

```python
import matplotlib as mpl
mpl.rcParams.update({
    "figure.figsize": (10, 5), "figure.dpi": 150, "savefig.dpi": 150,
    "font.family": ["Atkinson Hyperlegible Next", "DejaVu Sans"],
    "font.size": 12, "axes.titlesize": 16, "axes.titleweight": "bold", "axes.labelsize": 12,
    "text.color": "#14191F", "axes.labelcolor": "#14191F",
    "xtick.color": "#4D545B", "ytick.color": "#4D545B", "axes.edgecolor": "#6B737A",
    "axes.grid": True, "grid.color": "#E0E5E9", "grid.linewidth": 0.8,
    "axes.spines.top": False, "axes.spines.right": False,
    "figure.facecolor": "#F8FBFD", "axes.facecolor": "#F8FBFD", "savefig.facecolor": "#F8FBFD",
    "axes.prop_cycle": mpl.cycler(color=["#3D5A73", "#4D545B", "#00677F"]),
})
REAL, PREVISTO = "#4D545B", "#3D5A73"
NIVEL = {"BAIXO": "#437E61", "MEDIO": "#BBA15E", "ALTO": "#B2574D"}
MATRIZ = mpl.colors.LinearSegmentedColormap.from_list("calmaria", ["#F5F8FA", "#B2C9D8", "#597590", "#3C4D61"])
```

Instale a fonte no computador para o matplotlib achar; se não der, ele cai na DejaVu Sans. No vídeo, grave o
notebook com zoom de 125%.

## Checklist de envio
- [ ] Notebook executado de ponta a ponta, sem erro, com as saídas salvas
- [ ] `dados/README.md` com fonte, link e período
- [ ] `requirements.txt`
- [ ] `RELATORIO_ML.md` de 2 a 4 páginas
- [ ] `saida/previsao_barulho.csv` entregue ao Luiz
- [ ] Vídeo de até 5 minutos, não listado
- [ ] Links do repositório e do vídeo enviados no Teams por um integrante

---

# Parte 3: contratos com as outras partes

Estes formatos são combinados entre as partes. **Não mude nome de campo, rota, tópico nem coluna sem avisar a equipe.**

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

- **Luiz (API):** entregar o `previsao_barulho.csv` até segunda, 16/11, no formato exato da Parte 3.
- **Integrante 5 (pitch):** gravar 20 a 30 segundos explicando o modelo e o que ele diz ao cidadão.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
