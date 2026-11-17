# API Calmaria

A API em C# (.NET 10) que atende três disciplinas da GS: C# Software Development, SOA e Web Services e
Cybersecurity. Três módulos que só conversam por interface: **Sensorial** (lugares, leituras, índice),
**Comunidade** (perfil anônimo e relatos) e **Privacidade** (dado sensível, taint, Risco Cidadão). O plano
completo está em `docs/disciplinas/csharp.md`.

## Estado (16/11)
**Sensorial** e **Comunidade** completos; da **Privacidade**, o motor de PII, o taint pelos tipos e a análise de
texto. Faltam as varreduras, a tradução por IA, o Risco Cidadão, o painel e o Semgrep.

| Rota | O que faz |
|---|---|
| `POST /api/usuarios` | Usuário anônimo (só um id; apelido opcional, sanitizado) |
| `GET`, `PUT /api/usuarios/{id}/perfil-sensorial` | Sensibilidade a som, luz e multidão (1 a 5) e o contato de apoio |
| `DELETE /api/usuarios/{id}` | Apaga o usuário, o perfil e os relatos (LGPD, art. 18) |
| `POST /api/relatos` | Relato de visita; o comentário é sanitizado antes de gravar e a resposta diz o que saiu (409 se repetido em 30 min) |
| `GET /api/relatos` | Relatos públicos, paginados, **sem o autor** |
| `GET /api/lugares/{id}/resumo-relatos` | Nota média e gatilhos mais citados |
| `POST /api/privacidade/analisar` | O que seria removido de um texto, mascarado; nada é gravado |
| `GET /api/previsoes/barulho` | Previsão do ML por distrito (`dias` ou `data`) |
| `POST /api/previsoes/importar` | Importa o CSV do ML (exige `X-Admin-Key`) |
| `GET /api/lugares` | Lugares perto (`lat`, `lng`, `raioM`, `tipo`), com o nível de agora (leitura dos últimos 15 min) |
| `GET /api/lugares/{id}` | Um lugar com a última leitura |
| `GET /api/lugares/{id}/leituras` | Histórico (`desde`; padrão: últimas 24 horas) |
| `GET /api/lugares/{id}/horarios-calmos` | As 3 horas mais calmas da semana e a grade de médias (28 dias, horário de São Paulo) |
| `POST /api/leituras` | Ingestão por HTTP, no formato da mensagem MQTT (201, 404, 422) |
| `GET /api/crise/refugio-mais-proximo` | O refúgio fixo ou o lugar CALMO agora mais perto, com minutos a pé e o trajeto |
| `GET /api/refugios` | Os refúgios fixos, para o app guardar e usar sem internet |
| `GET /health` | Banco e MQTT (`Healthy`; `Degraded` se o MQTT cair; `Unhealthy` se o banco cair) |

O **worker MQTT** assina `calmaria/v1/lugares/+/leituras` no `broker.hivemq.com`, grava cada leitura pelo mesmo
serviço do `POST /api/leituras` e reconecta sozinho. Mensagem inválida vai para o log com o motivo, nunca com o
conteúdo. Ligado em desenvolvimento (`Mqtt:Habilitado`); na VM, `Mqtt__Habilitado=true`.

Em desenvolvimento, com a tabela vazia, a API grava **4 semanas de leituras simuladas** dos 4 lugares
monitorados (semente fixa, a mesma base em qualquer computador).

**Privacidade.** O motor (`Calmaria.Privacidade`, sem ASP.NET nem banco) roda uma classe por regra: CPF (com
dígito verificador), telefone, e-mail, saúde (com contexto, para não marcar "crise econômica"), segredos,
localização precisa e quase identificador, mais as regras do `Regras/regras-pii.json` (RG e Cartão SUS), que
mudam sem recompilar. O texto do cidadão entra como `TextoNaoConfiavel` e só o `Sanitizador` produz
`TextoSanitizado`, o único tipo que o relato grava e a listagem pública devolve: gravar o texto cru não compila.

**Rotas administrativas** exigem o cabeçalho `X-Admin-Key` igual a `Privacidade:ChaveAdmin` (user-secrets em
desenvolvimento; `Privacidade__ChaveAdmin` na VM).

São 170 testes: 110 da API e 60 do motor de privacidade (pelo menos 3 positivos e 3 negativos por regra).

## Pacote para a VM
`powershell -ExecutionPolicy Bypass -File api-dotnet/scripts/empacotar-vm.ps1` gera, em `api-dotnet/artefatos/`
(fora do Git), a API publicada para Linux (`calmaria-api-linux-x64.zip`) e o `efbundle`, que cria as tabelas:
`./efbundle --connection "Host=localhost;Database=calmaria;Username=calmaria;Password=..."`. As variáveis que a
API lê na VM estão no `csharp.md` (contrato da VM).

## Como rodar

Precisa do SDK do .NET 10 e de um PostgreSQL 16 ou mais novo.

1. Criar o usuário e o banco de desenvolvimento (uma vez). O script pede a senha do `postgres` e uma senha
   nova para o usuário `calmaria`, e guarda a conexão no user-secrets, fora do Git:

```bash
powershell -ExecutionPolicy Bypass -File api-dotnet/scripts/criar-banco-dev.ps1
```

2. Subir a API. Em desenvolvimento, as tabelas e o seed são criados ao subir:

```bash
dotnet run --project api-dotnet/src/Calmaria.Api
```

A API escuta em `http://0.0.0.0:5080`: no computador, `http://localhost:5080/scalar`; no celular, o IP do
computador na rede, como `http://192.168.0.10:5080`.

3. Rodar os testes (SQLite em memória, sem PostgreSQL):

```bash
dotnet test api-dotnet/Calmaria.sln
```

Fora do desenvolvimento, a conexão vem da variável `ConnectionStrings__Calmaria`, e as tabelas, do pacote de
migrações (`dotnet ef migrations bundle`).

## Estrutura

```
api-dotnet/
├── src/
│   ├── Calmaria.Domain/          Entidades, objetos de valor e interfaces (módulo Sensorial por enquanto)
│   ├── Calmaria.Privacidade/     O módulo de Cybersecurity, sem dependência de ASP.NET (a partir de 16/11)
│   ├── Calmaria.Infrastructure/  DbContext, configurações, migrações, seed e serviços
│   └── Calmaria.Api/             Controllers por módulo, Program.cs, Scalar
├── tests/Calmaria.Tests/         Domínio e endpoints (MSTest)
└── scripts/criar-banco-dev.ps1   Banco de desenvolvimento
```

## Dados
Os lugares são reais, mas as coordenadas são aproximadas e as leituras dos sensores serão simuladas.
Nenhum dado pessoal real entra no banco.
