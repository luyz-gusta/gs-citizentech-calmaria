# Mede a precisão e a revocação do motor de PII na amostra rotulada (60 comentários fictícios) e grava a tabela em
# security/relatorios/precisao-e-revocacao.md. É o número que vai no vídeo.
#
# Uso:  powershell -ExecutionPolicy Bypass -File security\medir-precisao.ps1

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$testes = Join-Path $raiz 'api-dotnet\tests\Calmaria.Privacidade.Tests'
$saida = Join-Path $raiz 'security\relatorios\precisao-e-revocacao.md'

$execucao = & dotnet test $testes --nologo -v q --filter 'FullyQualifiedName~AmostraRotulada' `
    --logger 'console;verbosity=detailed' 2>&1 | Out-String
$passou = $LASTEXITCODE -eq 0

$tabela = $execucao -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '^\|' }
$erros = $execucao -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '^item \d+:' }

$texto = @(
    '# Precisão e revocação do motor de PII'
    ''
    "Amostra: api-dotnet/tests/Calmaria.Privacidade.Tests/AmostraRotulada.json, 60 comentários fictícios (40 com dado pessoal, 20 sem)."
    'Conta por comentário: a regra disparou ou não. VP verdadeiro positivo, FP falso positivo, FN falso negativo.'
    "Gerado em $(Get-Date -Format 'dd/MM/yyyy HH:mm') por security/medir-precisao.ps1. Piso do teste: 90% em cada regra."
    ''
) + $tabela + @('') + $(if ($erros) { @('Erros na amostra:', '') + ($erros | ForEach-Object { "- $_" }) } else { @('Nenhum erro na amostra.') })
$texto += ''
$texto += "Resultado do teste: $(if ($passou) { 'passou' } else { 'FALHOU' })"

$texto | Set-Content -Path $saida -Encoding utf8
$texto | ForEach-Object { Write-Host $_ }
if (-not $passou) { exit 1 }
