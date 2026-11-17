# Prova a primeira camada do taint analysis: o dado do cidadão, cru, não compila dentro da API.
# Copia security/exemplos/TentativaDeVazamentoNaApi.cs.txt para a API, roda o build, guarda os erros em
# security/relatorios/erro-de-compilacao.txt e apaga a cópia (mesmo se algo der errado no meio).
#
# Uso, de qualquer pasta:  powershell -ExecutionPolicy Bypass -File security\provar-erro-de-compilacao.ps1

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$api = Join-Path $raiz 'api-dotnet\src\Calmaria.Api'
$copia = Join-Path $api 'Privacidade\TentativaDeVazamento.cs'
$saida = Join-Path $raiz 'security\relatorios\erro-de-compilacao.txt'

New-Item -ItemType Directory -Force -Path (Split-Path $saida) | Out-Null
Copy-Item (Join-Path $raiz 'security\exemplos\TentativaDeVazamentoNaApi.cs.txt') $copia
try {
    $build = & dotnet build $api -nologo -v q 2>&1 | Out-String
    $falhou = $LASTEXITCODE -ne 0
}
finally {
    Remove-Item $copia -Force
}

# Só as linhas de erro, sem o caminho da máquina, e uma vez cada.
$erros = $build -split "`r?`n" |
    Where-Object { $_ -match 'TentativaDeVazamento\.cs\(\d+,\d+\): error CS\d+' } |
    ForEach-Object { ($_ -replace '^.*[\\/](TentativaDeVazamento\.cs)', '$1') -replace '\s*\[[^\]]*\.csproj\]\s*$', '' } |
    Sort-Object -Unique

$texto = @(
    "Prova do taint pelo sistema de tipos: as linhas do controller vulnerável, coladas na API."
    "Gerado em $(Get-Date -Format 'dd/MM/yyyy HH:mm') por security/provar-erro-de-compilacao.ps1."
    ""
    "Resultado do build: $(if ($falhou) { 'FALHOU, como devia' } else { 'COMPILOU (a proteção pelo tipo quebrou!)' })"
    ""
) + $erros
$texto | Set-Content -Path $saida -Encoding utf8
$texto | ForEach-Object { Write-Host $_ }

if (-not $falhou) { exit 1 }
