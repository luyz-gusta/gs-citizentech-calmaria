# Demonstração do antes e do depois da correção, para o vídeo e para o print do painel (PA-02).
#
# Sobe a API duas vezes, numa porta só dela, com o log em arquivo numa pasta só da demonstração:
#   ANTES   o guarda de log desligado (Privacidade__RedigirLogs=false) e o log de depuração do corpo ligado
#           (Privacidade__LogarCorpoDasRequisicoes=true), o erro comum. Um relato FICTÍCIO com CPF e "autista"
#           grava o texto cru no log; a varredura acha e o Risco Cidadão sobe.
#   CORREÇÃO o log contaminado é apagado e o guarda volta a valer.
#   DEPOIS  o mesmo relato sai mascarado no log e vira fluxo bloqueado; a varredura dá Risco Cidadão baixo.
# No fim, o painel em /seguranca mostra o antes e o depois.
#
# Precisa do banco de desenvolvimento (api-dotnet/scripts/criar-banco-dev.ps1), da API parada na mesma pasta (o
# build não roda com a DLL aberta) e da chave de administrador na variável de ambiente:
#   $env:Privacidade__ChaveAdmin = '<a mesma chave que você digita no painel>'
#   powershell -ExecutionPolicy Bypass -File security\demo-antes-depois.ps1

param([int]$Porta = 5091)

$ErrorActionPreference = 'Stop'
if (-not $env:Privacidade__ChaveAdmin) {
    throw 'Defina $env:Privacidade__ChaveAdmin antes de rodar (a mesma chave do painel).'
}

$raiz = Split-Path -Parent $PSScriptRoot
$projeto = Join-Path $raiz 'api-dotnet\src\Calmaria.Api'
$dll = Join-Path $projeto 'bin\Debug\net10.0\Calmaria.Api.dll'
$pastaDeLogs = Join-Path $raiz 'security\relatorios\demo-logs'
$base = "http://localhost:$Porta"
$cabecalho = @{ 'X-Admin-Key' = $env:Privacidade__ChaveAdmin }
$cpfFicticio = '529.982.247-25'

function Subir([bool]$comGuarda) {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = $base
    $env:LogEmArquivo__Pasta = $pastaDeLogs
    $env:Privacidade__Varredura__PastaDeLogs = $pastaDeLogs
    $env:Privacidade__RedigirLogs = if ($comGuarda) { 'true' } else { 'false' }
    $env:Privacidade__LogarCorpoDasRequisicoes = 'true'
    $env:Mqtt__Habilitado = 'false'
    # Só os fluxos desta demonstração entram na conta, não os de testes anteriores no mesmo banco.
    $env:Privacidade__Varredura__JanelaDeFluxos = '00:02:00'
    $processo = Start-Process dotnet -ArgumentList "`"$dll`"" -WorkingDirectory $projeto -PassThru -WindowStyle Hidden
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Seconds 1
        try {
            Invoke-WebRequest "$base/health" -UseBasicParsing -TimeoutSec 2 | Out-Null
            return $processo
        }
        catch {
            if ($_.Exception.Response) { return $processo }
        }
    }
    Stop-Process -Id $processo.Id -Force -ErrorAction SilentlyContinue
    throw 'A API não subiu em 60 segundos.'
}

function Relatar {
    $usuario = Invoke-RestMethod "$base/api/usuarios" -Method Post -ContentType 'application/json' -Body '{}'
    $corpo = @{
        usuarioId = $usuario.id
        lugarId = 4
        nota = 2
        gatilhos = @('BARULHO', 'MULTIDAO')
        comentario = "Relato fictício da demonstração. Meu CPF é $cpfFicticio e meu filho é autista. Muito cheio às 18h."
        visitadoEm = (Get-Date).AddHours(-1).ToString('yyyy-MM-ddTHH:mm:sszzz')
    } | ConvertTo-Json
    Invoke-RestMethod "$base/api/relatos" -Method Post -ContentType 'application/json; charset=utf-8' `
        -Body ([Text.Encoding]::UTF8.GetBytes($corpo)) | Out-Null
}

function Varrer {
    (Invoke-RestMethod "$base/api/privacidade/varreduras" -Method Post -Headers $cabecalho).risco
}

function LogTemCpfCru {
    $arquivos = Get-ChildItem $pastaDeLogs -Filter '*.log' -ErrorAction SilentlyContinue
    [bool]($arquivos | Select-String -SimpleMatch $cpfFicticio -Quiet)
}

Write-Host 'Compilando a API...'
& dotnet build $projeto -nologo -v q | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'O build falhou. A API está rodando? Pare-a e rode de novo.' }
if (Test-Path $pastaDeLogs) { Remove-Item $pastaDeLogs -Recurse -Force }

$guardadas = 'ASPNETCORE_ENVIRONMENT', 'ASPNETCORE_URLS', 'LogEmArquivo__Pasta', 'Privacidade__Varredura__PastaDeLogs',
    'Privacidade__RedigirLogs', 'Privacidade__LogarCorpoDasRequisicoes', 'Mqtt__Habilitado',
    'Privacidade__Varredura__JanelaDeFluxos' |
    ForEach-Object { @{ Nome = $_; Valor = [Environment]::GetEnvironmentVariable($_) } }
try {
    Write-Host 'ANTES: guarda de log desligado e log de depuração do corpo ligado.'
    $api = Subir $false
    try { Relatar; Start-Sleep -Seconds 1; $antes = Varrer } finally { Stop-Process -Id $api.Id -Force }
    $cruAntes = LogTemCpfCru

    Write-Host 'CORREÇÃO: apagando o log contaminado e ligando o guarda.'
    Get-ChildItem $pastaDeLogs -Filter '*.log' | Remove-Item -Force

    Write-Host 'DEPOIS: o mesmo relato com o guarda ligado.'
    $api = Subir $true
    try { Relatar; Start-Sleep -Seconds 1; $depois = Varrer } finally { Stop-Process -Id $api.Id -Force }
    $cruDepois = LogTemCpfCru
}
finally {
    foreach ($v in $guardadas) { [Environment]::SetEnvironmentVariable($v.Nome, $v.Valor) }
}

$resumo = @(
    'Demonstração do antes e depois (security/demo-antes-depois.ps1)'
    "Rodada em $(Get-Date -Format 'dd/MM/yyyy HH:mm'), com um relato fictício (CPF de exemplo e a palavra autista)."
    ''
    "ANTES   Risco Cidadão $($antes.indice) ($($antes.faixa)): $($antes.expostos) dado(s) exposto(s), $($antes.mitigados) barrado(s). CPF cru no log: $(if ($cruAntes) { 'sim' } else { 'não' })."
    "DEPOIS  Risco Cidadão $($depois.indice) ($($depois.faixa)): $($depois.expostos) dado(s) exposto(s), $($depois.mitigados) barrado(s). CPF cru no log: $(if ($cruDepois) { 'sim' } else { 'não' })."
)
$resumo | Set-Content -Path (Join-Path $raiz 'security\relatorios\demo-antes-depois.txt') -Encoding utf8
$resumo | ForEach-Object { Write-Host $_ }
# O log do depois (só mascarado) fica como evidência; o do antes, com o CPF cru, já foi apagado na correção.
if (-not $cruDepois) {
    Get-ChildItem $pastaDeLogs -Filter '*.log' | Get-Content -Encoding utf8 |
        Set-Content -Path (Join-Path $raiz 'security\relatorios\log-do-depois-mascarado.log') -Encoding utf8
}
Remove-Item $pastaDeLogs -Recurse -Force
