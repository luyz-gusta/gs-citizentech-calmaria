# Entrega: as evidências e o vídeo, gerados de execuções reais

Esta pasta refaz, com um comando cada, os prints das entregas de C#, SOA e Cybersecurity e o vídeo de
Cybersecurity. Nada aqui é montado à mão: cada print sai da saída real de um comando ou de uma tela da API rodando,
e cada número do vídeo vem dessas mesmas capturas.

## 1. As evidências

```bash
node entrega/gerar-evidencias.mjs
```

Precisa do PostgreSQL local com a conexão no user-secrets (`api-dotnet/scripts/criar-banco-dev.ps1`), do Edge ou do
Chrome e, para o Semgrep, do Docker Desktop aberto. Leva uns 4 minutos e faz, nesta ordem:

| Etapa | O que roda | Print |
|---|---|---|
| Banco do zero | um schema `entrega` vazio, `dotnet ef migrations list` e `database update`, `\dt` | `api-dotnet/docs/evidencias/migracoes.png`, `tabelas-postgresql.png` |
| Testes | `dotnet test` | `testes.png` (nas duas pastas) |
| Precisão e revocação | `security/medir-precisao.ps1` | `security/evidencias/precisao-e-revocacao.png` |
| Taint pelo tipo | `security/provar-erro-de-compilacao.ps1` | `erro-de-compilacao.png` |
| Taint estático | `security/rodar-semgrep.ps1` | `semgrep-antes-depois.png` |
| Antes e depois | `security/demo-antes-depois.ps1` | `demo-antes-depois.png`, `log-mascarado.png` |
| Relato com CPF | `POST /api/relatos`, a listagem e o banco | `relato-mascarado.png` (nas duas pastas) |
| MQTT | uma leitura publicada em `broker.hivemq.com` e lida em `GET /api/lugares/1` | `api-dotnet/docs/evidencias/leitura-mqtt.png` |
| Documentação | o Scalar em `/scalar` | `scalar.png`, `scalar-relato.png` |
| Painel | `/seguranca` com o antes e o depois, 2880 x 1800 | `security/evidencias/painel-risco-cidadao.png` |

A API sobe com uma chave de administrador gerada na hora, que vale só nesta rodada. Nem ela nem a senha do banco
aparecem em print, log ou tela: o script troca as duas por `***` em qualquer texto. O banco de desenvolvimento não é
tocado: tudo acontece no schema `entrega`, apagado e recriado a cada rodada. O CPF do relato é fictício
(`529.982.247-25`, o exemplo com dígito válido usado nos testes).

As capturas para o vídeo (as respostas da API, os recortes do painel, as saídas dos scripts) ficam em
`entrega/saida/capturas/`, fora do Git.

## 2. O vídeo de Cybersecurity

```bash
node entrega/video/gerar-video.mjs
```

Depois das evidências. Segue o roteiro de `docs/disciplinas/cybersecurity.md` em seis blocos, com a abertura, as
cartelas de bloco e a cartela final do design (`design/pecas/`). A narração é a voz pt-BR do próprio Windows
("Microsoft Maria"), gerada no computador, sem mandar texto a serviço nenhum (`entrega/video/falar.ps1`). Opções:
`--voz Daniel` e `--velocidade 1.1`.

Sai em `entrega/saida/video/`:
- `calmaria-cybersecurity.mp4`: 1920 x 1080, 30 qps, H.264, AAC 48 kHz, voz normalizada em -16 LUFS (kit de vídeo
  VI-01);
- `calmaria-cybersecurity.srt`: a legenda em pt-BR, até 42 caracteres por linha, para subir junto no YouTube (VI-07).

O texto de cada fala está em `entrega/video/roteiro.mjs`. Para mudar uma frase, edite lá e rode de novo.

## 3. O ZIP

```bash
powershell -ExecutionPolicy Bypass -File security/gerar-zip.ps1 -LinkDoVideo "https://youtu.be/..."
```

Leva o vídeo e a legenda, se existirem em `entrega/saida/video/`, além do link.
