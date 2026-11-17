# Cybersecurity: o dado de saúde não pode vazar

Calmaria, Global Solution 2026.2 da FIAP (CitizenTech). A Calmaria é um mapa sensorial para pessoas autistas em
São Paulo: sensores medem som, luz e movimento, o app mostra os horários calmos e tem um botão de crise que leva ao
refúgio mais próximo, e a comunidade relata como foi cada visita.

**Por que o cuidado.** Dizer que uma pessoa é autista é dado pessoal sensível referente à saúde (LGPD, art. 5º, II),
com tratamento restrito (art. 11). E a Calmaria tem saídas perigosas: um broker MQTT público, logs de servidor,
relatos publicados para todos e um LLM, que é serviço de outra empresa. Um vazamento expõe uma pessoa vulnerável,
a rotina dela e o telefone de quem cuida dela.

| Critério | Pontos | Onde está |
|---|---|---|
| Robustez do motor de parsing de PII | 3,0 | `modulo/Calmaria.Privacidade/Regras` e `Motor`, com os testes e a amostra rotulada |
| Eficácia do taint analysis | 3,0 | `Taint/` (tipos), os guardas em `api/Privacidade/` e as regras em `security/semgrep/` |
| Inovação com IA na remediação | 2,0 | `Remediacao/` (TradutorLlm, prompt versionado, frases prontas de reserva) |
| Dashboard executivo (Risco Cidadão) | 2,0 | `Risco/` e o painel em `api/wwwroot/seguranca/` |

No repositório, o módulo fica em `api-dotnet/src/Calmaria.Privacidade/` e os testes em
`api-dotnet/tests/Calmaria.Privacidade.Tests/`. O módulo não depende de ASP.NET nem de banco: recebe texto e devolve
achados, por isso vai sozinho no ZIP e tem testes próprios.

## 1. O motor de parsing de PII

Cada regra é uma classe (`IRegraPii`, com o comum em `RegraBase`); o `MotorPii` recebe todas por injeção de
dependência e, quando dois achados se sobrepõem, fica com o de maior peso. Cada achado sai com o trecho já
mascarado (`529.***.***-25`, `<SAUDE>`): o texto original nunca sai do motor.

| Regra | Categoria | Peso | O que pega e o que deixa passar |
|---|---|---|---|
| `cpf` | CPF | 4 | Com e sem pontuação, só com dígito verificador válido |
| `telefone` | Telefone | 3 | Fixo e celular, com e sem DDD e +55; não casa no meio de um número maior em grupos (protocolo) |
| `email` | E-mail | 3 | Endereço de e-mail |
| `saude` | Saúde | 5 | Autismo, TEA, CID F84, laudo, especialistas, remédios; "crise" só com contexto de saúde ("crise econômica" não) |
| `segredo` | Segredo | 5 | Senha em string de conexão, `api_key`, `Bearer`, `sk-`, `AKIA`; ignora variável (`$senha`), marcador (`...`) e nome de tipo |
| `localizacao-precisa` | Localização | 4 | Coordenada com 4 casas ou mais junto de um identificador de usuário |
| `quase-identificador` | CEP e nascimento | 2 | CEP e data no mesmo texto, com "nasc" |
| `rg`, `cartao-sus` | Documento | 4 | Do `regras-pii.json`: regra nova sem recompilar, com os exemplos virando teste automático |

**Robustez, em número.** A amostra `AmostraRotulada.json` tem 60 comentários fictícios (40 com dado pessoal, 20
sem, com as armadilhas: "crise econômica", "teatro", "Tea time", CPF com dígito errado, protocolo em grupos). O
teste mede precisão e revocação por regra e exige 90% em cada uma. A medição achou um erro de verdade: na primeira
rodada, o telefone teve 88% de precisão, porque marcava o protocolo "898 0012 3456 7890". A regra foi corrigida e
hoje dá 100% nas 9 regras na amostra (`relatorios/precisao-e-revocacao.md`). Na amostra: a equipe escreveu os
comentários, então 100% aqui não é garantia fora dela.

A varredura do código real também achou erro: 4 falsos positivos de segredo (um comentário de código que dizia que
o usuário não tem senha, a variável `$senha` de um script e um `Password=...` de exemplo). A regra passou a ignorar variável, marcador e
valor curto, os casos viraram teste, e a varredura das 16.858 linhas do repositório ficou com zero achados.

## 2. Taint analysis, em três camadas

**Origens:** o comentário do relato, o apelido e o contato de apoio (corpo das requisições) e a coordenada do botão
de crise. **Destinos inseguros:** o log, as respostas públicas, o LLM externo, o broker MQTT público e os arquivos
exportados. **Sanitizador:** `Sanitizador.Sanitizar(TextoNaoConfiavel)`, que roda o motor, troca o dado por
`[removido]` ou `<SAUDE>` e devolve `TextoSanitizado`.

1. **Pelo tipo, em tempo de compilação.** O dado que entra vira `TextoNaoConfiavel` no próprio DTO. O valor cru é
   interno ao módulo e o `ToString` não o mostra. Os destinos só aceitam `TextoSanitizado` (o `Relato`, o
   `RelatoPublicoDto`) ou `AchadoMascarado` (o `ITradutorDeRisco`). O único caminho de um para o outro é o
   `Sanitizador`. Quem tenta gravar, publicar ou mandar ao LLM o texto cru não compila.
2. **Na saída, em execução.** O `LoggerProviderComRedacao` embrulha todos os provedores de log: a linha com dado
   pessoal sai mascarada e o fluxo vira um `EventoDeFluxo` (origem, destino, categoria, bloqueado). O
   `FiltroRespostaPublica` faz o mesmo no JSON das rotas públicas. Esta camada pega o que o tipo não vê, como o log
   de depuração do corpo da requisição, que grava o JSON antes de virar DTO.
3. **Estático, no código.** As regras do Semgrep em modo taint (`semgrep/calmaria-taint.yaml`): origem nos campos de
   texto do cidadão, destino em log, console, arquivo e `HttpClient`, sanitizador `Sanitizar(...)`; e uma regra de
   senha escrita no código.

**As varreduras** (`POST /api/privacidade/varreduras`) passam o motor em quatro fontes, cada uma uma classe de
`IFonteVarredura`: `FonteLogs` (os arquivos de log da API), `FonteRelatos` (os comentários gravados),
`FonteMqtt` (as últimas 200 mensagens do broker) e `FonteCodigo` (o repositório, atrás de segredo). O resultado
vai para as tabelas `Varreduras`, `AchadosDeVarredura` e `EventosDeFluxo` (migração `Privacidade`).

### A Auditoria do Protótipo

| Evidência | Como gerar | Arquivo |
|---|---|---|
| O Semgrep acusa o controller vulnerável e não acusa o corrigido nem a API | `rodar-semgrep.ps1` (precisa do Docker aberto) | `relatorios/semgrep-antes.json`, `-depois.json`, `-api.json` |
| A linha vulnerável colada na API não compila | `provar-erro-de-compilacao.ps1` | `relatorios/erro-de-compilacao.txt` (CS0029, CS1061 e CS1503 nas cinco tentativas) |
| Em execução, o relato com CPF e "autista" sai mascarado no banco, no log e na listagem | `demo-antes-depois.ps1` | `relatorios/demo-antes-depois.txt` e `relatorios/log-do-depois-mascarado.log` |
| A varredura do código real e dos logs | O botão "Rodar varredura" do painel | O painel e `GET /api/privacidade/achados` |

O exemplo vulnerável e o corrigido ficam em `exemplos/`, fora da solução. A senha do exemplo vulnerável é
fictícia.

## 3. Tradução via IA

O `TradutorLlm` chama o LLM por REST com `HttpClient` tipado, tempo limite por tentativa, uma nova tentativa e
disjuntor. **Entra só mascarado:** o método recebe `AchadoMascarado` (categoria, trecho como `529.***.***-25`,
destino e se foi barrado), então o dado real não tem como chegar ao serviço externo. O prompt é versionado em
`Remediacao/prompt-alerta.md` (recurso embutido) e pede duas saídas: a frase para o cidadão, literal, curta e sem
alarme, e a correção para a equipe. A resposta do LLM fica em cache por categoria, destino, situação e modelo
(tabela `AlertasEmCache`), para não pagar a mesma pergunta duas vezes. Sem chave, sem internet ou com resposta fora
do formato, o `TradutorModelo` usa frases prontas e o painel diz o motivo. Qualquer LLM com a API de chat no formato
da OpenAI serve (Ollama local sem chave, OpenAI, Groq, Gemini, Claude); a chave vem de `Llm__Chave`, nunca do
repositório.

## 4. O painel Risco Cidadão

```
risco de um item  = peso do dado x peso da exposição x (1 se exposto, 0,2 se barrado)
peso da exposição = 3 (broker público, resposta pública, LLM externo), 2 (log, arquivos), 1 (banco)
Risco Cidadão     = 100 x soma dos riscos / soma se nada estivesse barrado        (0 a 100)
faixas: 0 a 20 baixo, 21 a 50 médio, 51 a 100 alto
```

Itens expostos são os achados da varredura; itens barrados são os fluxos que os guardas bloquearam na última
semana. Com tudo barrado, o indicador fica em 20 (baixo, mas não zero: o dado chegou a tentar sair).

O painel fica em `/seguranca`, servido pela própria API: HTML, CSS e JavaScript sem build, Chart.js por CDN com
versão fixa e hash de integridade, política de conteúdo que só deixa a página falar com a própria API, e a chave
de administrador guardada só na aba (sessionStorage), nunca na URL. Mostra o antes e o depois numa régua de 0 a 100
com as três faixas, o risco por tipo de dado e por destino, os alertas traduzidos, os fluxos bloqueados, os dados
expostos, as fontes e o histórico. Segue o tema do sistema (claro e escuro).

**A demonstração** (`demo-antes-depois.ps1`) sobe a API duas vezes. Antes, com o guarda de log desligado e o log de
depuração do corpo ligado: o relato fictício grava o CPF cru no log e o Risco Cidadão dá **52 (alto)**. Depois da
correção (log contaminado apagado, guarda ligado): o mesmo relato sai mascarado e o indicador dá **20 (baixo)**.

## Como rodar

Na pasta do repositório, com a API de pé (`api-dotnet/README.md`) e a chave de administrador definida:

```bash
dotnet test api-dotnet/tests/Calmaria.Privacidade.Tests
powershell -ExecutionPolicy Bypass -File security/medir-precisao.ps1
powershell -ExecutionPolicy Bypass -File security/provar-erro-de-compilacao.ps1
powershell -ExecutionPolicy Bypass -File security/rodar-semgrep.ps1
powershell -ExecutionPolicy Bypass -File security/demo-antes-depois.ps1
powershell -ExecutionPolicy Bypass -File security/gerar-zip.ps1 -LinkDoVideo "https://..."
```

O painel: `http://localhost:5080/seguranca`. As rotas: `POST /api/privacidade/varreduras`,
`GET /api/privacidade/risco-cidadao`, `/achados` e `/fluxos`, todas com o cabeçalho `X-Admin-Key`.

## O que fica de fora, dito com clareza
- O Semgrep não roda nativo neste computador: o Application Control do Windows barra o `semgrep-core.exe`. O
  `rodar-semgrep.ps1` usa o contêiner oficial, então precisa do Docker Desktop aberto.
- A tradução pelo LLM foi testada com um serviço simulado nos testes (resposta certa, erro 500, 401, sem rede,
  resposta fora do formato). A chamada a um LLM de verdade depende de configurar `Llm__UrlBase` e `Llm__Modelo`.
- A `FonteLogs` lê linha a linha; um dado quebrado em duas linhas do log escaparia da varredura (o guarda de log,
  que vê a mensagem inteira, continua valendo).
- Os testes e os exemplos têm segredos falsos de propósito; a conferência do ZIP os lista e não os conta como
  vazamento.
