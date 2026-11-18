# Dados do notebook de ML

## Fonte
Portal de Dados Abertos da Prefeitura de São Paulo, conjunto **"Dados do SP156 - Solicitações"**:
https://dados.prefeitura.sp.gov.br/dataset/dados-do-sp156

Glossário das colunas: "Dicionário dados abertos - SP156 2020 em diante", no mesmo conjunto.

São dados **reais e públicos**. A base não identifica quem reclamou; mesmo assim, o modelo usa só o distrito e
a data, não o endereço.

## Período e arquivos usados
Oito trimestres, de **01/10/2024 a 30/09/2026**. Download em **15/11/2026**.

| Trimestre | Arquivo local (`dados/brutos/`) | Link |
|---|---|---|
| 4º 2024 | `4T24.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/ed357147-f15c-4bf0-ad15-edc963a9f130/download/arquivo-final-4-trim.-2024.csv |
| 1º 2025 | `1T25.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/7c5aeb58-2cad-40ba-9e9e-af1c17cc5070/download/arquivo-final-1-trim-25.csv |
| 2º 2025 | `2T25.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/f5df7f4a-d614-4f85-b4a0-263ec9370cd9/download/arquivo-final-2-trim-25.csv |
| 3º 2025 | `3T25.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/3b1d2648-d586-40c8-819c-24fbb8005343/download/arquivo-final-3-trim-25.csv |
| 4º 2025 | `4T25.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/d3a98097-8128-45d4-b8fe-21874b81c971/download/arquivo-final-4-trim-25.csv |
| 1º 2026 | `1T26.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/75105148-4efd-48f3-8ae0-fc6c2adf7060/download/arquivo-final-1-trim-26.csv |
| 2º 2026 | `2T26.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/368eb32d-ef25-4441-be8f-fce0146575a1/download/arquivo-final-2-trim-26.csv |
| 3º 2026 | `3T26.csv` | https://dados.prefeitura.sp.gov.br/dataset/0aecfa2b-aa3a-40d4-8183-0d4351b7fd0a/resource/f67fcc12-8da4-485f-92a3-d2af40cc771e/download/arquivo-final-3-trim-26.csv |

Os brutos (cerca de 100 MB cada) ficam **fora do Git** (`.gitignore`). Para refazer, baixe os oito links acima
com esses nomes em `dados/brutos/` e rode o notebook. Sem os brutos, o notebook lê direto o
`psiu_filtrado.csv`.

## Filtro aplicado
`Assunto == "Poluição Sonora - PSIU"` (tema "Rua e bairro").

## O que fica em `psiu_filtrado.csv`
Uma linha por reclamação de PSIU:

| Coluna | Origem | Observação |
|---|---|---|
| `data` | Data de Abertura | Só a data: a base não tem a hora |
| `distrito` | Distrito | Maiúsculas e sem acento, como no SP156 (`CONSOLACAO`, `BELA VISTA`) |
| `subprefeitura` | Prefeitura Operacional | |
| `latitude`, `longitude` | Latitude, Longitude | Guardadas para análises extras; o modelo não usa |
| `trimestre` | nome do arquivo | Para conferir de onde veio cada linha |

## Cuidados que os arquivos reais exigiram
- **Codificação e separador mudam entre arquivos:** sete vêm em `latin-1` com `;`; o 3º trimestre de 2026 vem
  em **UTF-8** com `,`. Ler o UTF-8 como `latin-1` estraga o "Poluição" e o filtro não acha nada (foi o que
  aconteceu na primeira execução). O notebook detecta a codificação e o separador de cada arquivo e para com
  erro se algum trimestre vier vazio.
- **Colunas:** o 4º trimestre de 2024 tem 16 colunas (inclui "Bairro"); os outros têm 15. As colunas são
  lidas **pelo nome normalizado** (sem acento, minúsculas), não pela posição.
- **Formato da data:** `DD/MM/AAAA` na maioria dos arquivos e `MM/DD/AAAA` no 3º trimestre de 2026
  (`07/01/2026` é 1º de julho). Para cada arquivo o notebook testa os dois formatos e fica com o que coloca
  as datas dentro do trimestre; a tabela com o formato escolhido e o percentual aparece no notebook.
- **Tamanho:** leitura em pedaços de 200 mil linhas, guardando só o PSIU.
- **Limitações:** sem a hora da reclamação; a reclamação mede o incômodo relatado, não decibéis; bairros onde
  as pessoas reclamam menos podem parecer mais calmos do que são.
