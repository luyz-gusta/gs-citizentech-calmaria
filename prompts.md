# Prompts de IA usados no projeto

O briefing permite IA como copiloto, desde que os prompts fiquem registrados aqui e cada integrante saiba
explicar o que foi aproveitado. Uma entrada por uso relevante, a mais recente no fim.

| Campo | O que escrever |
|---|---|
| Data | Quando |
| Quem | O integrante |
| Parte | A pasta ou disciplina |
| Ferramenta | A IA usada |
| Prompt | O texto enviado (ou um resumo fiel, se for longo) |
| O que foi aproveitado | O que entrou no projeto e o que foi revisado ou descartado |

---

## 12/11/2026: escopo do projeto e planos das disciplinas
- **Quem:** Luiz
- **Parte:** `docs/` (escopo global e os 8 arquivos de disciplina)
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Vou te mandar a ideia do que tivemos até agora e quero que você desenvolva essa ideia: [a ideia
  em docs/00-ideia-original.md]. Quero que você prepare o escopo ao todo, tudo que precisa para o projeto, e
  também já prepare um .md detalhado do que precisa para cada matéria com base no escopo global do projeto."
  Antes, o briefing da GS (PDF da FIAP) foi lido pela mesma ferramenta.
- **O que foi aproveitado:** a estrutura do escopo, os contratos entre as partes, o plano de cada disciplina,
  as features e os test cases. A contagem das reclamações de barulho foi feita sobre o arquivo real do SP156
  (3º trimestre de 2026). **A revisar pela equipe:** nome do projeto, pesos do índice depois da entrevista,
  divisão de tarefas e as dúvidas para os professores.

## 12/11/2026: uma API em C# para C#, SOA e Cybersecurity
- **Quem:** Luiz
- **Parte:** `docs/` (escopo global e os arquivos de C#, SOA, Cybersecurity, Mobile, Testing e Operating
  Systems)
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Estava pensando em matar três matérias com um projeto só: fazer a API em C# e já implementar
  nela o que Cybersecurity pede e o de SOA também." Depois da análise de cada rubrica: "Sim, atualiza agora."
- **O que foi aproveitado:** a API única em três módulos (Sensorial, Comunidade e Privacidade), o PostgreSQL
  no lugar do SQLite, o módulo de Cybersecurity em C# com taint pelo sistema de tipos, o mapa da rubrica no
  README e os planos B de SOA e de Cybersecurity. **A confirmar com os professores em 13/11.**

## 12/11/2026: design da Calmaria
- **Quem:** Luiz
- **Parte:** `design/` e a seção "Design da sua parte" dos 8 arquivos de disciplina
- **Ferramenta:** Claude (Anthropic), no Claude Code, com uma skill de design que calcula a paleta e confere o
  contraste por script
- **Prompt:** "Quero já transformar todo esse escopo e briefing do projeto em um design de verdade, com
  especificações, tamanhos, direção de cor e lista de peças, para as entregas acima, e pode criar um artefato
  para isso."
- **O que foi aproveitado:** a identidade (símbolo, logotipo, ardósia #3D5A73, Atkinson Hyperlegible), os
  tokens dos dois temas com o contraste conferido (WCAG 2.2 AA), o tema do app em TypeScript, as telas do app,
  o visor, o painel, o kit de vídeo e a lista de 46 peças. **A validar pela equipe:** o rótulo do botão de
  crise ("Preciso de calma") e a preferência de cor, na entrevista.

## 12/11/2026: exportação das peças
- **Quem:** Luiz
- **Parte:** `design/pecas/`
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Sim, exporta" (resposta a "quer que eu exporte agora os PNG das peças?")
- **O que foi aproveitado:** os moldes HTML das peças, o script `exportar.py` (Chrome sem janela) e os 25
  arquivos em `design/pecas/png/`. Nomes da equipe e links ainda como marcador.

## 12/11/2026: cores da marca no plano e nos documentos
- **Quem:** Luiz
- **Parte:** `README.md`, `docs/01-escopo-global.md`, `docs/disciplinas/*.md` e o artefato "Plano da Calmaria"
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Coloque agora as cores da marca no artefato que é sobre o projeto para eu enviar no grupo do
  pessoal, o artefato e os .md."
- **O que foi aproveitado:** o plano passou a usar as cores, a fonte e o símbolo da marca, com o diagrama da
  arquitetura no estilo da identidade e a seção "A cara da Calmaria"; a paleta entrou no README, no escopo
  global e na parte comum de cada arquivo de disciplina.

## 12/11/2026: base da API (módulo Sensorial)
- **Quem:** Luiz
- **Parte:** `api-dotnet/`
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Agora, vamos fazer a minha parte", seguindo a ordem de construção do `csharp.md` (quinta à noite).
- **O que foi aproveitado:** a solução com os projetos Domain, Privacidade, Infrastructure, Api e Tests; o domínio
  do módulo Sensorial (lugares com herança, leitura com faixa física, coordenada com Haversine, índice com
  nível); o EF Core com PostgreSQL, a migração `Inicial_Sensorial` e o seed dos lugares 1 a 6; `GET /api/lugares`,
  `GET /api/lugares/{id}`, `/health` e o Scalar; 40 testes (MSTest, SQLite em memória). Revisado e rodado no
  PostgreSQL local. **Mudança no plano:** o discriminador da herança é a coluna `Categoria`, não o `Tipo`.

## 15/11/2026: leituras, MQTT, horários calmos e botão de crise
- **Quem:** Luiz
- **Parte:** `api-dotnet/`
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Sim, adianta" (adiantar a etapa de sábado do `csharp.md`).
- **O que foi aproveitado:** ingestão de leituras (HTTP e MQTT pelo mesmo serviço), o worker MQTT com MQTTnet e
  reconexão, o histórico de leituras, os horários calmos (regra pura no domínio, horário de São Paulo, horário
  de funcionamento), o refúgio da crise (retângulo no banco e Haversine em memória), `/api/refugios`, o MQTT no
  `/health` e 4 semanas de leituras simuladas com semente fixa. 84 testes; conferido no PostgreSQL local e com uma
  mensagem publicada no `broker.hivemq.com`. Dois testes acharam erros meus (um comentário que contradizia a
  regra do horário de funcionamento e uma distância calculada de cabeça) e foram corrigidos.

## 16/11/2026: Comunidade, núcleo da Privacidade, previsões do ML e pacote da VM
- **Quem:** Luiz
- **Parte:** `api-dotnet/`
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Se mais coisa que eu possa fazer, faça" (seguir o `csharp.md` e o `cybersecurity.md`).
- **O que foi aproveitado:** o módulo Comunidade (usuário anônimo, perfil sensorial, relatos com a regra dos 30
  minutos, listagem pública sem autor, resumo e exclusão pela LGPD); o motor de PII (uma classe por regra e regras
  do JSON), os tipos de taint e o Sanitizador; a importação e a consulta das previsões do ML com chave de
  administrador; o script do pacote linux-x64 e do efbundle. 170 testes; conferido no PostgreSQL local.
  **Revisado:** dois exemplos errados do JSON de regras (Cartão SUS com 16 dígitos) e as expressões de CPF e
  telefone, que não achavam o número no fim de uma frase.

## 15 a 17/11/2026: notebook de ML e app Expo
- **Quem:** (preencher: integrantes de IA e de Mobile)
- **Parte:** `ml/` e `mobile/`
- **Ferramenta:** Claude (Anthropic), no Claude Code
- **Prompt:** "Na pasta docs, quero resolver as disciplinas de IA e mobile." A ferramenta leu
  `docs/disciplinas/ia-machine-learning.md` e `docs/disciplinas/mobile.md` e seguiu os dois briefings.
- **O que foi aproveitado:** no ML, o download dos 8 trimestres do SP156, o filtro do PSIU (detectando
  codificação, separador e formato de data de cada arquivo), o notebook com linha de base, Random Forest e
  XGBoost com busca por `TimeSeriesSplit`, os gráficos, o CSV de previsões e o relatório. O resultado foi
  mantido como saiu: o modelo ganha pouco da linha de base, e o relatório explica por quê. No app, o projeto
  Expo (SDK 58) com todas as telas, o cliente da API com mock de reserva, o SQLite, o botão de crise offline e
  a passada de acessibilidade. **A revisar pela equipe:** as frases da entrevista no README do app, os prints
  e os vídeos; o app ainda não foi testado num celular.
