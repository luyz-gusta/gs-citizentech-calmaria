<#
  Gera o que a VM (Operating Systems) precisa para subir a API, em api-dotnet\artefatos (fora do Git):

    calmaria-api-linux-x64.zip   A API publicada para Linux. Entrada: Calmaria.Api.dll (precisa do runtime
                                 do ASP.NET Core 10 na VM).
    efbundle                     As migrações do banco, num executável para Linux:
                                 ./efbundle --connection "Host=localhost;Database=calmaria;Username=calmaria;Password=..."

  Rode da raiz do repositório:
    powershell -ExecutionPolicy Bypass -File api-dotnet\scripts\empacotar-vm.ps1
#>
$ErrorActionPreference = 'Stop'

$raiz = Split-Path $PSScriptRoot -Parent
$saida = Join-Path $raiz 'artefatos'
$publicacao = Join-Path $saida 'publicacao'
$dotnet = 'dotnet'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { $dotnet = 'C:\Program Files\dotnet\dotnet.exe' }

if (Test-Path $saida) { Remove-Item $saida -Recurse -Force }
New-Item -ItemType Directory -Path $saida | Out-Null

Write-Host 'Publicando a API para linux-x64...'
& $dotnet publish (Join-Path $raiz 'src\Calmaria.Api') -c Release -r linux-x64 --self-contained false -o $publicacao --nologo -v q
if ($LASTEXITCODE -ne 0) { throw 'A publicação falhou.' }

# Barra normal nos caminhos, que o unzip do Linux entende sem aviso. No Windows PowerShell 5.1 (.NET Framework), o
# ZipFile grava barra invertida se a chave abaixo não for desligada antes de carregar a biblioteca, e o wwwroot\seguranca
# viraria arquivos soltos com a barra no nome.
[AppContext]::SetSwitch('Switch.System.IO.Compression.ZipFile.UseBackslash', $false)
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($publicacao, (Join-Path $saida 'calmaria-api-linux-x64.zip'))
Remove-Item $publicacao -Recurse -Force

Write-Host 'Gerando o pacote de migrações (efbundle) para linux-x64...'
Push-Location $raiz
try {
    & $dotnet tool restore | Out-Null
    & $dotnet tool run dotnet-ef migrations bundle --project src/Calmaria.Infrastructure --startup-project src/Calmaria.Api `
        --target-runtime linux-x64 --self-contained --output (Join-Path $saida 'efbundle') --force
    if ($LASTEXITCODE -ne 0) { throw 'O efbundle falhou.' }
}
finally {
    Pop-Location
}

Write-Host 'Pronto:'
Get-ChildItem $saida | ForEach-Object { '  {0}  ({1:N1} MB)' -f $_.Name, ($_.Length / 1MB) }
