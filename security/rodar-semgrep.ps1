# Roda o Semgrep com as regras da Calmaria (security/semgrep/calmaria-taint.yaml) em três alvos:
#   antes   security/exemplos/RelatoControllerVulneravel.cs  (tem que acusar)
#   depois  security/exemplos/RelatoControllerCorrigido.cs   (não pode acusar)
#   api     api-dotnet/src                                    (a API de verdade; não pode acusar)
# Grava um JSON por alvo em security/relatorios/semgrep-<alvo>.json e imprime o resumo.
#
# Usa o semgrep instalado, se houver. Senão, o contêiner oficial: abra o Docker Desktop pelo menu Iniciar antes.
# Nesta máquina o semgrep-core.exe é barrado pelo Application Control do Windows, então o caminho é o Docker.
#
# Uso:  powershell -ExecutionPolicy Bypass -File security\rodar-semgrep.ps1

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$versao = '1.185.0'
$regras = 'security/semgrep/calmaria-taint.yaml'
$alvos = [ordered]@{
    'antes'  = 'security/exemplos/RelatoControllerVulneravel.cs'
    'depois' = 'security/exemplos/RelatoControllerCorrigido.cs'
    'api'    = 'api-dotnet/src'
}

New-Item -ItemType Directory -Force -Path (Join-Path $raiz 'security\relatorios') | Out-Null

$nativo = Get-Command semgrep -ErrorAction SilentlyContinue
if (-not $nativo) {
    & docker version --format '{{.Server.Version}}' *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'Sem semgrep e sem Docker. Abra o Docker Desktop pelo menu Iniciar e rode de novo.'
    }
}

Push-Location $raiz
try {
    foreach ($nome in $alvos.Keys) {
        $saida = "security/relatorios/semgrep-$nome.json"
        $argumentos = @('scan', '--config', $regras, '--metrics=off', '--disable-version-check', '--json',
            '--output', $saida, $alvos[$nome])
        # No Windows PowerShell 5.1, com 'Stop', até o progresso que o Semgrep escreve no stderr vira erro fatal.
        # Aqui quem diz se deu errado é o código de saída: 0 e 1 são análise feita, 2 ou mais é falha.
        $ErrorActionPreference = 'Continue'
        if ($nativo) {
            & semgrep @argumentos 2>$null | Out-Null
        }
        else {
            & docker run --rm -v "${raiz}:/src" -w /src "semgrep/semgrep:$versao" semgrep @argumentos 2>$null | Out-Null
        }
        $codigo = $LASTEXITCODE
        $ErrorActionPreference = 'Stop'
        if ($codigo -ge 2) {
            throw "O Semgrep falhou no alvo '$nome' (código $codigo)."
        }

        $resultado = Get-Content $saida -Raw -Encoding utf8 | ConvertFrom-Json
        Write-Host ('{0,-7} {1} achado(s) em {2}' -f $nome, $resultado.results.Count, $alvos[$nome])
        foreach ($r in $resultado.results) {
            Write-Host ('        {0}:{1}  {2}' -f $r.path, $r.start.line, ($r.check_id -replace '^.*\.', ''))
        }
        foreach ($e in $resultado.errors) {
            Write-Host ('        ERRO do Semgrep: {0}' -f $e.message)
        }
    }
}
finally {
    Pop-Location
}
