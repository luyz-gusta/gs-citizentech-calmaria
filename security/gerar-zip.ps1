# Monta security/calmaria-cybersecurity.zip, a entrega de Cybersecurity:
#   modulo/   Calmaria.Privacidade (regras de PII, motor, taint, varredura, remediação, Risco Cidadão) e os testes
#   api/      os guardas e as rotas da API (PrivacidadeController, LoggerProviderComRedacao, FiltroRespostaPublica)
#             e o painel /seguranca
#   security/ as regras do Semgrep, os exemplos, os relatórios e as evidências
#   README.md e LINK-DO-VIDEO.txt
# Antes de fechar, procura segredo em tudo o que vai no ZIP (os mesmos padrões da RegraSegredo). Os testes e os
# exemplos têm segredos FALSOS de propósito; eles aparecem na lista, mas não barram. Qualquer outro barra.
#
# Uso:  powershell -ExecutionPolicy Bypass -File security\gerar-zip.ps1 -LinkDoVideo 'https://youtu.be/...'

param([string]$LinkDoVideo = '(preencher com o link do vídeo)')

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$destino = Join-Path $raiz 'security\calmaria-cybersecurity.zip'
$montagem = Join-Path ([IO.Path]::GetTempPath()) ("calmaria-zip-" + [guid]::NewGuid().ToString('N'))
$pasta = Join-Path $montagem 'calmaria-cybersecurity'

function Copiar([string]$origem, [string]$alvo) {
    $de = Join-Path $raiz $origem
    $para = Join-Path $pasta $alvo
    if (Test-Path $de -PathType Container) {
        New-Item -ItemType Directory -Force -Path $para | Out-Null
        # Sem bin, obj e resultados de teste: só o código.
        & robocopy $de $para /E /XD bin obj TestResults /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { throw "Falha ao copiar $origem." }
    }
    elseif (Test-Path $de) {
        New-Item -ItemType Directory -Force -Path (Split-Path $para) | Out-Null
        Copy-Item $de $para
    }
    else {
        Write-Host "Aviso: $origem não existe e ficou de fora."
    }
}

try {
    Copiar 'api-dotnet\src\Calmaria.Privacidade' 'modulo\Calmaria.Privacidade'
    Copiar 'api-dotnet\tests\Calmaria.Privacidade.Tests' 'modulo\Calmaria.Privacidade.Tests'
    Copiar 'api-dotnet\src\Calmaria.Api\Privacidade' 'api\Privacidade'
    Copiar 'api-dotnet\src\Calmaria.Api\wwwroot' 'api\wwwroot'
    Copiar 'api-dotnet\Directory.Build.props' 'modulo\Directory.Build.props'
    Copiar 'security\semgrep' 'security\semgrep'
    Copiar 'security\exemplos' 'security\exemplos'
    Copiar 'security\relatorios' 'security\relatorios'
    Copiar 'security\evidencias' 'security\evidencias'
    Copiar 'security\README.md' 'README.md'
    @(
        'Vídeo da atividade de Cybersecurity (Calmaria, Global Solution 2026.2):'
        $LinkDoVideo
    ) | Set-Content -Path (Join-Path $pasta 'LINK-DO-VIDEO.txt') -Encoding utf8

    # A conferência de segredo: os mesmos padrões da RegraSegredo, em todo arquivo de texto do ZIP.
    $padrao = '(?i)\b(password|pwd|senha)[''"]?\s*[=:]\s*[''"]?(?![$%{<\[(.*])(?!(string|number|bool|boolean|int|str|null|undefined|none|true|false)\b)[^\s;,''"]{4,}' +
        '|(?i)\bapi[_-]?key[''"]?\s*[=:]\s*[''"]?(?=[A-Za-z_\-]*\d)[A-Za-z0-9_\-]{8,}' +
        '|(?i)\bbearer\s+[A-Za-z0-9\-._~+/]{10,}' +
        '|(?i)\bsk-[A-Za-z0-9_-]{16,}' +
        '|\bAKIA[0-9A-Z]{16}\b'
    $deProposito = '^(modulo[\\/]Calmaria\.Privacidade\.Tests|security[\\/]exemplos)[\\/]'
    $achados = Get-ChildItem $pasta -Recurse -File |
        Where-Object { $_.Extension -notin '.png', '.jpg', '.jpeg', '.gif', '.mp4', '.zip', '.pdf' } |
        Select-String -Pattern $padrao |
        ForEach-Object { [pscustomobject]@{ Arquivo = $_.Path.Substring($pasta.Length + 1); Linha = $_.LineNumber } }
    $barram = @($achados | Where-Object { $_.Arquivo -notmatch $deProposito })
    $falsos = @($achados | Where-Object { $_.Arquivo -match $deProposito })

    Write-Host "Conferência de segredo: $($achados.Count) ocorrência(s); $($falsos.Count) falsa(s) de propósito, $($barram.Count) a corrigir."
    $falsos | ForEach-Object { Write-Host "   falso de propósito: $($_.Arquivo):$($_.Linha)" }
    if ($barram.Count -gt 0) {
        $barram | ForEach-Object { Write-Host "   SEGREDO: $($_.Arquivo):$($_.Linha)" }
        throw 'Há segredo no que iria para o ZIP. Corrija antes de enviar.'
    }

    if (Test-Path $destino) { Remove-Item $destino -Force }
    # ZipFile grava os caminhos com barra normal, que abre igual no Windows, no Linux e no macOS.
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($montagem, $destino)
    $tamanho = [math]::Round((Get-Item $destino).Length / 1KB)
    Write-Host "ZIP pronto: security\calmaria-cybersecurity.zip ($tamanho KB)."
}
finally {
    if (Test-Path $montagem) { Remove-Item $montagem -Recurse -Force }
}
