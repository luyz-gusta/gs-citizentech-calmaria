# API Calmaria

A API em C# (.NET 10) que atende três disciplinas da GS: C# Software Development, SOA e Web Services e
Cybersecurity. Três módulos que só conversam por interface: **Sensorial** (lugares, leituras, índice),
**Comunidade** (perfil anônimo e relatos) e **Privacidade** (dado sensível, taint, Risco Cidadão). O plano
completo está em `docs/disciplinas/csharp.md`.

## Estado (12/11)
Feito: a solução, o domínio do módulo Sensorial, o PostgreSQL com EF Core, a migração `Inicial_Sensorial`, o
seed dos lugares (1 a 6; o 7 entra depois da observação), `GET /api/lugares`, `GET /api/lugares/{id}`,
`/health`, o Scalar e 40 testes. Os próximos passos seguem a ordem do `csharp.md`.

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
