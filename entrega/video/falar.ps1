# Gera a narração do vídeo com a voz pt-BR do próprio Windows (OneCore), sem mandar texto a serviço nenhum.
# Lê um JSON com [{ "id": "...", "fala": "..." }] e grava <pasta>\<id>.wav, um arquivo por frase.
#
# Uso:  powershell -ExecutionPolicy Bypass -File entrega\video\falar.ps1 -Frases frases.json -Pasta saida -Voz Maria

param(
    [Parameter(Mandatory)] [string]$Frases,
    [Parameter(Mandatory)] [string]$Pasta,
    [string]$Voz = 'Maria',
    [double]$Velocidade = 1.0
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Runtime.WindowsRuntime
$null = [Windows.Media.SpeechSynthesis.SpeechSynthesizer, Windows.Media.SpeechSynthesis, ContentType = WindowsRuntime]
$null = [Windows.Storage.Streams.DataReader, Windows.Storage.Streams, ContentType = WindowsRuntime]

$asTask = [System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1'
} | Select-Object -First 1

function Esperar($operacao, [Type]$tipo) {
    $tarefa = $asTask.MakeGenericMethod($tipo).Invoke($null, @($operacao))
    $tarefa.Wait(-1) | Out-Null
    $tarefa.Result
}

$sintetizador = New-Object Windows.Media.SpeechSynthesis.SpeechSynthesizer
# A voz vai pelo SSML: no PowerShell 5.1, o objeto de voz do WinRT chega embrulhado e a propriedade Voice o recusa.
$nomes = @([Windows.Media.SpeechSynthesis.SpeechSynthesizer]::AllVoices | ForEach-Object { "$($_.DisplayName)|$($_.Language)" })
$nomeDaVoz = ($nomes | Where-Object { $_ -like "*$Voz*|pt-BR" } | Select-Object -First 1)
if (-not $nomeDaVoz) { throw "Não achei a voz pt-BR '$Voz'. Vozes: $($nomes -join ', ')" }
$nomeDaVoz = $nomeDaVoz.Split('|')[0]
$taxa = '{0:0}%' -f ($Velocidade * 100)

New-Item -ItemType Directory -Force -Path $Pasta | Out-Null
$lista = Get-Content $Frases -Raw -Encoding utf8 | ConvertFrom-Json
foreach ($frase in $lista) {
    $texto = [Security.SecurityElement]::Escape($frase.fala)
    $ssml = "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='pt-BR'><voice name='$nomeDaVoz'>" +
        "<prosody rate='$taxa'>$texto</prosody></voice></speak>"
    $fluxo = Esperar ($sintetizador.SynthesizeSsmlToStreamAsync($ssml)) ([Windows.Media.SpeechSynthesis.SpeechSynthesisStream])
    $leitor = New-Object Windows.Storage.Streams.DataReader($fluxo.GetInputStreamAt(0))
    $tamanho = [uint32]$fluxo.Size
    Esperar ($leitor.LoadAsync($tamanho)) ([uint32]) | Out-Null
    $bytes = New-Object byte[] $tamanho
    $leitor.ReadBytes($bytes)
    [IO.File]::WriteAllBytes((Join-Path $Pasta "$($frase.id).wav"), $bytes)
    $leitor.Dispose()
    $fluxo.Dispose()
}
Write-Host "$(@($lista).Count) frase(s) em $Pasta com a voz $nomeDaVoz."
