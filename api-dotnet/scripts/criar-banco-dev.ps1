<#
  Cria o usuário e o banco de desenvolvimento da Calmaria no PostgreSQL desta máquina e guarda a conexão no
  user-secrets da API (fora do Git). Rode uma vez, da raiz do repositório:

    powershell -ExecutionPolicy Bypass -File api-dotnet\scripts\criar-banco-dev.ps1

  O psql pede a senha do usuário postgres. A senha do usuário calmaria você escolhe aqui e ela não aparece na tela.
#>
param(
    [string]$Psql = 'C:\Program Files\PostgreSQL\16\bin\psql.exe',
    [string]$Usuario = 'calmaria',
    [string]$Banco = 'calmaria'
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Psql)) { throw "Não achei o psql em $Psql. Passe o caminho com -Psql." }
$dotnet = 'dotnet'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { $dotnet = 'C:\Program Files\dotnet\dotnet.exe' }

$segura = Read-Host "Escolha uma senha para o usuário $Usuario (só para desenvolvimento, sem ponto e vírgula)" -AsSecureString
$senha = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura))
if ([string]::IsNullOrWhiteSpace($senha) -or $senha.Contains(';')) { throw 'Senha vazia ou com ponto e vírgula.' }
$senhaSql = $senha.Replace("'", "''")

Write-Host 'Criando o usuário e o banco (o psql vai pedir a senha do postgres)...'
& $Psql -U postgres -h localhost -v ON_ERROR_STOP=1 `
    -c "CREATE ROLE $Usuario LOGIN PASSWORD '$senhaSql';" `
    -c "CREATE DATABASE $Banco OWNER $Usuario;"
if ($LASTEXITCODE -ne 0) { throw 'O psql falhou. Se a mensagem fala de senha, a do postgres não confere; se o usuário ou o banco já existem, apague antes ou passe -Usuario e -Banco.' }

$projeto = Join-Path $PSScriptRoot '..\src\Calmaria.Api'
& $dotnet user-secrets set 'ConnectionStrings:Calmaria' "Host=localhost;Port=5432;Database=$Banco;Username=$Usuario;Password=$senha" --project $projeto | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Não consegui gravar o user-secrets.' }

Write-Host 'Pronto. Para subir a API (as tabelas são criadas ao subir):'
Write-Host '  dotnet run --project api-dotnet\src\Calmaria.Api'
