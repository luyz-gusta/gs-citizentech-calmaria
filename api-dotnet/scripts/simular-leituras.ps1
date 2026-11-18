<#
  Simula os 4 sensores para gravar o vídeo do app: manda uma leitura de cada lugar a cada 30 segundos para
  POST /api/leituras, no mesmo formato da mensagem do ESP32. Sem isso, o "nível agora" some 15 minutos depois da
  última leitura da semente. Os valores são fixos, para o vídeo mostrar sempre os mesmos níveis:

    Estação Consolação  MODERADO (índice 50)
    Estação Paulista    INTENSO  (índice 92)
    Estação Trianon     CALMO    (índice 16)
    Mercado da Augusta  MODERADO (índice 47)

  Rode da raiz do repositório, com a API no ar, e pare com Ctrl+C:

    powershell -ExecutionPolicy Bypass -File api-dotnet\scripts\simular-leituras.ps1
#>
param(
    [string]$Api = 'http://localhost:5080',
    [int]$IntervaloSegundos = 30
)
$ErrorActionPreference = 'Stop'

$sensores = @(
    @{ dispositivoId = 'esp32-consolacao-01'; lugarId = 1; somDb = 75; luzLux = 1500; movimentosMin = 12 },
    @{ dispositivoId = 'esp32-paulista-01';   lugarId = 2; somDb = 95; luzLux = 2800; movimentosMin = 28 },
    @{ dispositivoId = 'esp32-trianon-01';    lugarId = 3; somDb = 52; luzLux = 600;  movimentosMin = 4 },
    @{ dispositivoId = 'esp32-mercado-01';    lugarId = 4; somDb = 70; luzLux = 1800; movimentosMin = 10 }
)

Write-Host "Mandando leituras para $Api a cada $IntervaloSegundos segundos. Ctrl+C para parar."
while ($true) {
    foreach ($s in $sensores) {
        try {
            $r = Invoke-RestMethod -Method Post -Uri "$Api/api/leituras" -ContentType 'application/json' `
                -Body ($s | ConvertTo-Json -Compress)
            Write-Host ("[{0:HH:mm:ss}] lugar {1}: indice {2}, {3}" -f (Get-Date), $s.lugarId, $r.indice, $r.nivel)
        } catch {
            Write-Host ("[{0:HH:mm:ss}] lugar {1}: FALHA ({2})" -f (Get-Date), $s.lugarId, $_.Exception.Message)
        }
    }
    Start-Sleep -Seconds $IntervaloSegundos
}
