# Relatório de IA e Machine Learning: quando e onde São Paulo fica mais barulhenta

Calmaria, Global Solution 2026.2, FIAP. Notebook: [calmaria_ml.ipynb](calmaria_ml.ipynb). Dados:
[dados/README.md](dados/README.md).

## 1. O problema e a pergunta de ML

Para muitas pessoas autistas, barulho alto é gatilho de sobrecarga sensorial. A Calmaria mostra no app se um
lugar está barulhento agora (sensores) e **como deve estar o barulho do distrito nos próximos dias** (este
modelo).

**Pergunta:** quantas reclamações de barulho (PSIU) cada distrito de São Paulo deve receber em cada dia, e esse
dia será de barulho baixo, médio ou alto para aquele distrito?

É uma **regressão** de contagem por distrito e dia. A contagem prevista vira um nível (BAIXO, MEDIO, ALTO) pelos
tercis da história do próprio distrito; é o nível que o app mostra ("Previsão de barulho na Consolação no
sábado: alta").

## 2. Os dados

- **Fonte:** Portal de Dados Abertos da Prefeitura de São Paulo, conjunto "Dados do SP156 - Solicitações"
  (https://dados.prefeitura.sp.gov.br/dataset/dados-do-sp156). Dados **reais e públicos**.
- **Período:** oito trimestres, de 01/10/2024 a 30/09/2026 (os links de cada arquivo estão em `dados/README.md`).
- **Filtro:** `Assunto == "Poluição Sonora - PSIU"`: **104.409 reclamações**; 40 descartadas sem distrito,
  sobram 104.369 em **96 distritos**.
- **Limpeza:** os arquivos não são iguais entre si. Sete vêm em `latin-1` com `;`, e o 3º trimestre de 2026 em
  UTF-8 com `,`; o 4º de 2024 tem uma coluna a mais ("Bairro"); a data é `DD/MM/AAAA` em sete arquivos e
  `MM/DD/AAAA` no último. O notebook detecta a codificação, o separador e o formato da data de cada arquivo
  (100% das datas caem dentro do trimestre do arquivo) e lê as colunas pelo nome. Um erro real ajudou a achar
  isso: na primeira execução, o 3º trimestre de 2026 veio vazio porque o UTF-8 lido como `latin-1` estragava
  o "Poluição". Hoje o notebook para com erro se algum trimestre vier vazio.
- **Série mensal:** entre 3.151 (out/2024) e 6.066 (abr/2026) reclamações por mês. Não há quebra de formato; há
  um **aumento em 2026** (março a junho acima de 5 mil), que pesa no teste (seção 6).
- **Limitações:** a base traz **só a data**, sem a hora: o modelo responde "onde e em que dia", e a hora vem dos
  sensores na API. A reclamação mede o **incômodo relatado**, não decibéis: bairros onde se reclama menos podem
  parecer mais calmos do que são. A base não identifica o reclamante; o modelo usa só distrito e data.

## 3. O modelo e por que ele

- **Linha de base (sem ML):** a média histórica do distrito naquele dia da semana, calculada no treino.
- **Modelo principal: XGBoost** (`XGBRegressor`), testando o erro quadrático e a perda `count:poisson`, própria
  para contagem.
- **Modelo de comparação: Random Forest.**

**Por que árvores:** os atributos são de calendário e de lugar, com interações (o sábado pesa mais na
Consolação, que tem vida noturna, do que num distrito residencial). Árvores captam essas interações sem
precisar escalar os dados. MLP e LSTM pedem mais dados e mais tempo do que tínhamos. KMeans responde a outra
pergunta (agrupar distritos). E atributos de "ontem" ficaram de fora: o app precisa prever semanas à frente, e o
SP156 publica os dados por trimestre, com atraso.

## 4. A preparação e a divisão por tempo

1. Agregação por distrito e dia; **dias sem reclamação completados com zero** (96 distritos × 730 dias =
   70.080 linhas; 40% dos dias com zero; média de 1,5 reclamação por distrito por dia; máximo de 144).
2. **Atributos:** dia da semana, fim de semana, mês, dia do mês, feriado e véspera de feriado (26 feriados
   nacionais, estaduais e municipais de São Paulo no período), distrito e subprefeitura codificados, e a média
   histórica do distrito e do distrito naquele dia da semana. As médias são calculadas **dentro do pipeline,
   só com a parte de treino** de cada dobra, para não vazar o alvo.
3. **Divisão por tempo:** treino de 01/10/2024 a 31/03/2026 (52.512 linhas); teste de 01/04/2026 a 30/09/2026
   (17.568 linhas). Uma divisão aleatória misturaria dias vizinhos entre treino e teste e daria um resultado
   bom demais.

## 5. O treino e os hiperparâmetros

`GridSearchCV` com `TimeSeriesSplit(n_splits=5)`, **só no treino**, otimizando o MAE. As tabelas completas
(`cv_results_`) estão no notebook.

| Modelo | Combinações | Grade | Melhor combinação | MAE na validação |
|---|---|---|---|---|
| Random Forest | 18 | `n_estimators` (100, 300) × `max_depth` (8, 16, sem limite) × `min_samples_leaf` (1, 5, 20) | 100 árvores, profundidade 16, folha 20 | 1,162 |
| XGBoost | 54 | `n_estimators` (100, 300, 600) × `max_depth` (3, 5, 8) × `learning_rate` (0,03; 0,1; 0,3) × perda (quadrática, Poisson) | 600 árvores, profundidade 3, taxa 0,03, Poisson | 1,150 |

O XGBoost foi escolhido por ter o menor erro na validação cruzada; o teste só foi usado no fim, para medir.

Na Random Forest, cada árvore usa metade das linhas (`max_samples=0.5`) e 60% dos atributos por divisão. Isso
deixa o treino mais rápido e as árvores mais variadas.

## 6. Resultados em treino e teste

| Modelo | MAE treino | MAE teste | RMSE teste | R² teste | Deviance de Poisson teste |
|---|---|---|---|---|---|
| Linha de base | 1,159 | 1,331 | 3,062 | 0,147 | 2,263 |
| Random Forest | 1,107 | 1,336 | 3,074 | 0,140 | 2,271 |
| XGBoost | 1,140 | 1,334 | 3,067 | 0,144 | 2,250 |

Nos distritos do protótipo (MAE no teste): **Consolação** 3,41 na linha de base e 3,37 no XGBoost;
**Bela Vista** 1,65 e 1,64.

Gráficos em `figuras/`: total mensal (`ml02`), previsto contra real na Consolação, na Bela Vista e na Vila
Mariana (`ml03`), importância dos atributos (`ml04`), matriz de confusão do nível (`ml05`), erro por dia da
semana e overfitting.

**Leitura honesta:** o ganho do modelo sobre a linha de base é **pequeno**. O XGBoost melhora 0,6% na
deviance de Poisson (a métrica certa para contagem) e nos dois distritos do protótipo; no MAE geral, empata.
O motivo está nos dados: com média de 1,5 reclamação por dia e 40% dos dias com zero, o ruído diário é
grande, e quase todo o sinal previsível está em "qual distrito" e "qual dia da semana", que a linha de base já
usa. Os atributos que tentamos a mais (feriados, mês, nível recente do distrito) quase não acrescentam. Um
detalhe técnico: o MAE favorece a mediana, e a mediana histórica teria MAE de 1,295, mas ela prevê zero em
muitos dias e não serve para dizer se o dia é de barulho médio ou alto.

**Erro por dia da semana:** é maior no sábado e no domingo (MAE de 1,6 a 1,7 contra 1,1 na terça), quando há
mais reclamações.

**O aumento de 2026:** o teste (abril a setembro de 2026) teve mais reclamações que o treino. Modelos de
árvore não extrapolam tendência, então todos subestimam um pouco esse período. Na previsão final, o modelo é
treinado com todo o período, incluindo o teste.

**Acerto do nível:** 33,6% no geral, contra 34,2% da linha de base (ao acaso seria 33,3%). Nos distritos com
poucas reclamações, os tercis ficam colados (0, 1 e 2 reclamações) e o nível vira quase sorte. Na
**Consolação e na Bela Vista**, que importam para o app, o acerto sobe para **45,9%**, contra 42,3% da linha de
base: é onde o modelo mais ajuda. Erros graves (prever BAIXO num dia ALTO, ou o contrário) são 14,3% dos dias.

## 7. Overfitting

A busca mostra o padrão esperado. Na Random Forest com folha de 1 linha e profundidade 16, o MAE de treino cai
para 0,77 e o de validação sobe para 1,20: o modelo decora o treino. Com folha de 20 linhas, o treino fica em
1,02 e a validação melhora para 1,16. No XGBoost, a melhor combinação é a **mais rasa** (profundidade 3) com
taxa baixa. A diferença entre o MAE de treino e o de teste do modelo final é de +0,19 (XGBoost) e +0,23
(Random Forest). O que reduziu a diferença: árvores rasas, folhas maiores e a perda de Poisson.

## 8. A leitura para o cidadão

Previsão para 16/11 a 13/12/2026 (média por dia da semana, em reclamações por dia):

| Distrito | Seg | Ter | Qua | Qui | Sex | Sáb | Dom |
|---|---|---|---|---|---|---|---|
| Consolação | 2,1 | **1,7** | 1,9 | 2,0 | 3,0 | **3,7** | **4,0** |
| Bela Vista | 2,0 | **1,7** | 1,8 | 2,0 | 2,2 | **3,0** | **3,4** |

- **Marina** (24 anos, trabalha na Paulista, Linha 2): na Consolação, sábado e domingo têm previsão de barulho
  **alta**, com mais que o dobro de reclamações da terça. Para quem é sensível a som, o melhor dia para
  resolver coisas na região é **terça ou quarta**. No fim de semana, vale planejar antes o refúgio mais
  próximo.
- **Carla** (mãe do Theo, 8 anos): em vez de ligar para o mercado, ela vê no app a previsão da semana e escolhe
  um dia de previsão baixa ou média. Com os horários calmos medidos pelos sensores, o app responde "vá na
  terça, às 9h".

**O que o modelo não faz:** não diz a hora e não mede decibéis. Ele também ganha pouco da média histórica,
então o valor dele para o cidadão está menos no número exato e mais em transformar uma base pública enorme
num aviso simples e confiável: fim de semana na Consolação é barulhento, e terça é o dia mais calmo.

## 9. A saída para a API

`saida/previsao_barulho.csv`: 84 linhas (Consolação, Bela Vista e Jardim Paulista, de 16/11 a 13/12/2026),
colunas `distrito,data,reclamacoes_previstas,nivel`, no formato do contrato com a API
(`POST /api/previsoes/importar`). O app também usa esse CSV no mock de reserva.
