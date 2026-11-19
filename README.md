# Calmaria: o mapa sensorial da cidade

Global Solution 2026.2 (CitizenTech), FIAP, Engenharia de Software, 3º ano.

## Ficha da CitizenTech

| Campo | Resposta |
|---|---|
| Nome da CitizenTech | Calmaria |
| Cidade e bairro | São Paulo, Bela Vista e Consolação |
| Cidadão beneficiado | Pessoas autistas com hipersensibilidade sensorial e as famílias que planejam as saídas delas |
| Problema, em uma frase | Pessoas autistas não têm como saber, antes de sair, se um mercado ou uma estação vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem |
| Como funciona | Sensores calculam um índice de sobrecarga de cada lugar; o app mostra o índice, os horários mais calmos e a previsão de barulho da região, e tem um botão de crise que leva ao refúgio calmo mais próximo, mesmo offline |
| O que cada parte faz | O ESP32 mede; a API em C# guarda, calcula, recebe os relatos e protege o dado de saúde com o módulo de privacidade; o ML prevê o barulho com o SP156; o app mostra; a VM hospeda e monitora; o Azure Boards garante a qualidade |
| Link do repositório | https://github.com/luyz-gusta/gs-citizentech-calmaria |
| Indicador de impacto | Tempo do toque no botão de crise até a chegada ao refúgio (meta: menos de 10 minutos a pé) e a parcela das visitas feitas em horário calmo |
| ODS conectados | 3 (meta 3.4), 10 (meta 10.2) e 11 (metas 11.2 e 11.7) |

## Onde está cada parte

| Pasta | Disciplina | Plano |
|---|---|---|
| `api-dotnet/` | C# Software Development | [csharp.md](docs/disciplinas/csharp.md) |
| `api-dotnet/` (a mesma API) | SOA e Web Services | [soa-web-services.md](docs/disciplinas/soa-web-services.md) |
| `api-dotnet/src/Calmaria.Privacidade/` e `security/` | Cybersecurity | [cybersecurity.md](docs/disciplinas/cybersecurity.md) |
| `mobile/` | Mobile Development e IoT | [mobile.md](docs/disciplinas/mobile.md) |
| `iot/` | Physical Computing | [physical-computing.md](docs/disciplinas/physical-computing.md) |
| `ml/` | IA e Machine Learning | [ia-machine-learning.md](docs/disciplinas/ia-machine-learning.md) |
| Fora do repositório: entregue à parte por Rafael Duarte de Freitas | Operating Systems | [operating-systems.md](docs/disciplinas/operating-systems.md) |
| Fora do repositório: entregue à parte por Rafael Duarte de Freitas | Testing, Compliance e QA | [testing-qa.md](docs/disciplinas/testing-qa.md) |
| `design/` | Todas (identidade, cores, fonte e o desenho de cada peça) | [plano-de-design.md](design/plano-de-design.md) e `design/design-da-calmaria.html` |
| `entrega/` | C#, SOA e Cybersecurity (as evidências e o vídeo, gerados de execuções reais) | [entrega/LEIA-ME.md](entrega/LEIA-ME.md) |

Vídeos (YouTube, não listados): IA e Machine Learning em https://youtu.be/IYpR5PEd4nM; Mobile em
https://youtu.be/9kRDjxLAlS4.

## Identidade visual
![Calmaria](design/pecas/png/ma-02-logotipo.png)

| Cor | Hex | Uso |
|---|---|---|
| Ardósia (a marca) | `#3D5A73` | Botões, links, logotipo, ícone do app, série principal dos gráficos |
| Fundo claro | `#F1F5F7` | Fundo das telas (nunca branco puro) |
| Cartão | `#F8FBFD` | Cartões, campos, fundo dos gráficos |
| Texto e fundo escuro | `#14191F` | Texto no tema claro; fundo no tema escuro |
| Apoio | `#4D545B` | Texto secundário, legendas, série "real" nos gráficos |
| Crise | `#1F2733` | Tela de crise, abertura e cartela final dos vídeos, banner, miniatura |
| Calmo | `#437E61` | Nível Calmo (C, círculo): ISS 0 a 39, previsão BAIXO, risco baixo, LED verde |
| Moderado | `#BBA15E` | Nível Moderado (M, quadrado): ISS 40 a 69, MEDIO, risco médio, LED amarelo; texto escuro por cima |
| Intenso | `#B2574D` | Nível Intenso (I, losango): ISS 70 a 100, ALTO, risco alto, LED vermelho |

- **Fonte:** Atkinson Hyperlegible Next para texto e Atkinson Hyperlegible Mono para números e código
  (Google Fonts, licença OFL; os arquivos estão em `design/pecas/fontes/`).
- **Cor só quando significa nível.** O resto do sistema é ardósia e neutros.
- **Quieto:** nada pisca nem anima, nem no LED; todo ícone vem com texto.
- Todas as combinações de texto e fundo passam no WCAG 2.2 AA, no tema claro e no escuro (conferido por script).

Especificação completa (telas do app, visor, painel, vídeos e a lista de 46 peças):
[design/plano-de-design.md](design/plano-de-design.md) e `design/design-da-calmaria.html`. Peças prontas em
`design/pecas/png/`. Cada arquivo de disciplina tem a seção "Design da sua parte".

## Mapa da rubrica: uma API, três disciplinas
C#, SOA e Cybersecurity avaliam a mesma API, organizada em três módulos (Sensorial, Comunidade e
Privacidade). Cada professor acha aqui o que avalia.

| Disciplina | Critério | Onde está |
|---|---|---|
| C# | POO | `api-dotnet/src/Calmaria.Domain/` (hierarquia de lugares) e `src/Calmaria.Privacidade/Regras/` (uma classe por regra) |
| C# | Web API | `api-dotnet/src/Calmaria.Api/` (controllers por módulo) e o Scalar em `/scalar` |
| C# | EF Core e LINQ | `api-dotnet/src/Calmaria.Infrastructure/` e os serviços de horários calmos e de resumo |
| C# | Async/await | O worker MQTT em `src/Calmaria.Infrastructure/Mqtt/` e o cliente do LLM em `src/Calmaria.Privacidade/Remediacao/` |
| C# | Qualidade | Os módulos, os dois projetos de teste e este README |
| SOA | Domínio | `api-dotnet/src/Calmaria.Domain/` |
| SOA | Banco | PostgreSQL com EF Core; conexão por variável de ambiente |
| SOA | Migrações | `api-dotnet/src/Calmaria.Infrastructure/Persistencia/Migracoes/` (`Inicial_Sensorial`, `Comunidade`, `Privacidade`) |
| SOA | Documentação automática | Scalar em `/scalar`, com exemplos; a seção "Para a avaliação de SOA" em `api-dotnet/README.md` |
| SOA | Testes | `api-dotnet/tests/` (`dotnet test`) |
| Cybersecurity | Motor de PII | `api-dotnet/src/Calmaria.Privacidade/Regras/` e `Motor/` |
| Cybersecurity | Taint analysis | `src/Calmaria.Privacidade/Taint/`, os guardas em `src/Calmaria.Api/Privacidade/`, `security/semgrep/` e a Auditoria do Protótipo em [security/README.md](security/README.md) |
| Cybersecurity | IA na remediação | `src/Calmaria.Privacidade/Remediacao/` |
| Cybersecurity | Painel Risco Cidadão | `/seguranca` (em `src/Calmaria.Api/wwwroot/seguranca/`) e `src/Calmaria.Privacidade/Risco/` |

O escopo completo, com os contratos entre as partes, está em
[docs/01-escopo-global.md](docs/01-escopo-global.md). A ideia de onde tudo partiu está em
[docs/00-ideia-original.md](docs/00-ideia-original.md).

## Dados
- **Leituras dos sensores:** simuladas no Wokwi e no seed da API.
- **Usuários e relatos de teste:** inventados e marcados como fictícios.
- **Reclamações de barulho:** reais e públicas, do conjunto "Dados do SP156 - Solicitações" do Portal de
  Dados Abertos da Prefeitura de São Paulo. Detalhes em `ml/dados/README.md`.

## Integrantes
Turma 3ESPW, FIAP, Engenharia de Software.

| Nome | RM |
|---|---|
| Guilherme Almeida | 555180 |
| Luiz Gustavo da Silva | 558358 |
| Rafael Duarte de Freitas | 558644 |
| Rafael Gaspar Bragança Martins | 557228 |
| Vinicius Monteiro Araújo | 555088 |

## Uso de IA
Os prompts usados estão em [prompts.md](prompts.md), como o briefing pede.
