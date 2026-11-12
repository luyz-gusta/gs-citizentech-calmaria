# Operating Systems: a VM Linux que sobe a Calmaria sozinha

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Operating Systems |
| Responsável | Integrante 4 (que também faz Testing, Compliance e QA, em `testing-qa.md`) |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `infra/` |
| Você depende de | Luiz (o pacote da API e o de migrações, até segunda 16/11; até lá, use o substituto da Parte 3) |
| Dependem de você | Integrante 2 (o app pode apontar para a VM no vídeo) |
| Vídeo | Sim: até 7 minutos, mostrando o provisionamento automático |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: administrador Linux sênior (Ubuntu Server, Bash, systemd, nginx, PostgreSQL, auditd).

Vamos escrever os scripts Bash que transformam uma VM Ubuntu recém-instalada num servidor da Calmaria, sem nenhum passo manual: rede, DNS, PostgreSQL, a API em .NET como serviço, nginx, monitoramento, auditoria e teste de falha. Scripts idempotentes, com `set -euo pipefail` e log de tudo.

Regras:
1. Siga o briefing à risca: a stack, os contratos com as outras partes (nomes de campos, rotas, tópicos,
   colunas, formatos) e os critérios de avaliação. Não mude nome de campo, rota nem formato: outras pessoas
   da equipe dependem deles.
2. Antes de produzir qualquer coisa, me devolva em até 10 linhas o que entendeu da minha parte e um plano em
   etapas, na ordem do "Passo a passo" do briefing. Espere o meu ok.
3. Depois, uma etapa por vez: o conteúdo completo de cada arquivo, onde ele fica na pasta e como eu testo que
   funcionou. Não pule para a próxima etapa sem eu confirmar que a anterior funcionou.
4. Explique as decisões em linguagem simples: eu vou apresentar e responder perguntas do professor.
5. Se algo do briefing estiver ambíguo ou faltar informação, pergunte em vez de inventar.
6. Nada de dado pessoal real: use os dados fictícios ou públicos indicados no briefing.
7. Ao fim de cada etapa, diga quais itens do checklist de entrega ela cumpre e o que falta.
8. Sem emoji, no código e nos textos.

Comece pelo resumo e pelo plano.
```

O briefing tem quatro partes: **Parte 1**, o projeto; **Parte 2**, a sua parte; **Parte 3**, os contratos com
as outras partes; **Parte 4**, com quem falar. O checklist de entrega fecha a Parte 2.

---

# Parte 1: o projeto

## O projeto em poucos minutos

### A Global Solution
A Global Solution (GS) 2026.2 da FIAP, Engenharia de Software, 3º ano, tem o tema **CitizenTech**: tecnologia
criada a partir de um problema real de um cidadão. A equipe escolhe **um** problema real de uma cidade e
constrói **uma** solução que atravessa as oito disciplinas do semestre; cada disciplina avalia a sua parte, de
0 a 10. **Entrega de todas: 19/11/2026 (quinta), até 23h55, na tarefa de cada disciplina no Teams,** enviada
por um só integrante. 20/11 é feriado. De 16 a 19/11 é a semana de orientação: o professor tira dúvida, mas
não corrige.

### A Calmaria
**O problema.** Para muitas pessoas autistas, barulho alto, luz forte e multidão são gatilho de sobrecarga
sensorial, que pode virar crise. Quem convive com isso planeja cada saída, e nenhum app de mapa diz se um lugar
é barulhento, se a luz é forte ou onde fica o lugar calmo mais perto.

**Em uma frase:** pessoas autistas em São Paulo não têm como saber, antes de sair, se um mercado ou uma estação
vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem.

**Os números.**
- 2,4 milhões de brasileiros têm diagnóstico de autismo, 1,2% da população (Censo 2022 do IBGE, divulgado em
  23/05/2025).
- No 3º trimestre de 2026, o SP156 (canal de reclamações da Prefeitura) recebeu 14.178 reclamações de
  "Poluição Sonora - PSIU", todas com coordenada. Sábado e domingo têm o dobro dos dias úteis. A Consolação,
  vizinha da FIAP Paulista, é o 2º distrito com mais reclamações. Fonte: Portal de Dados Abertos da Prefeitura
  de São Paulo, conjunto "Dados do SP156 - Solicitações".
- A Lei 12.764/2012 trata a pessoa com autismo como pessoa com deficiência para todos os efeitos legais.

**Para quem.** A pessoa autista com hipersensibilidade sensorial que circula pela cidade, e o responsável por uma
criança autista. Região do protótipo: Bela Vista e Consolação, em São Paulo (Av. Paulista e Rua Augusta).
Personas de trabalho (fictícias): **Marina, 24 anos**, autista, trabalha na Paulista, usa a Linha 2 do metrô e
precisa de um lugar quieto em menos de 10 minutos quando a sobrecarga vem; **Carla, 41 anos**, mãe do Theo, 8
anos, autista, que liga para o mercado antes de sair para saber se está cheio.

**A solução.** Sensores medem som, luz e movimento de pessoas em mercados e estações e calculam um **Índice de
Sobrecarga Sensorial (ISS)**, de 0 a 100. O app mostra o índice no mapa, os **horários mais calmos** de cada
lugar, a **previsão de barulho** do distrito para os próximos dias (feita com dados reais do SP156) e tem um
**botão de crise** que leva ao refúgio calmo mais próximo, mesmo sem internet. A comunidade completa o mapa com
**relatos** de como foi a visita.

**A jornada principal do cidadão.**
1. Abre o app e, se quiser, informa o perfil sensorial (sensibilidade a som, luz e multidão, de 1 a 5) e um
   contato de apoio. Sem nome, e-mail nem CPF.
2. Vê os lugares perto, cada um com o nível atual em cor **e** em texto: Calmo, Moderado ou Intenso.
3. Abre um lugar: índice agora, os 3 horários mais calmos da semana, a previsão de barulho do distrito e os
   relatos.
4. Vai no horário calmo ou a um lugar alternativo, com o trajeto aberto no app de mapas do celular.
5. Na sobrecarga, toca no **botão de crise** (sempre visível): tela simples e escura, o refúgio mais próximo com
   distância, um roteiro de respiração em texto e um botão para ligar para o contato de apoio. Funciona offline.
6. Depois, se quiser, relata como foi.

**Princípios que valem para todas as partes.**
- Nada de som, vibração forte, flash nem animação. Um produto para quem sofre com estímulo não pode ser mais um
  estímulo. O ESP32 não tem buzzer, de propósito.
- Cor nunca sozinha: todo nível aparece em cor, em texto e com a letra e a forma (C no círculo, M no
  quadrado, I no losango).
- Cores da marca: ardósia `#3D5A73`, fundo `#F1F5F7` (nunca branco puro), texto `#14191F`, crise `#1F2733`;
  níveis Calmo `#437E61`, Moderado `#BBA15E` e Intenso `#B2574D`. Fonte Atkinson Hyperlegible. O detalhe
  está na seção "Design da sua parte".
- Linguagem literal e curta, sem metáfora nem ironia.
- Previsível: a mesma navegação em todas as telas; o botão de crise sempre no mesmo lugar.
- Privacidade por padrão: sem login; dado de saúde nunca vai para log nem para o broker MQTT público.
- Funciona com conexão ruim: o botão de crise não depende da internet.

### A Ficha da CitizenTech
Vai no topo do README da raiz e no README do app.

| Campo | Resposta |
|---|---|
| Nome da CitizenTech | Calmaria (provisório) |
| Cidade e bairro | São Paulo, Bela Vista e Consolação |
| Cidadão beneficiado | Pessoas autistas com hipersensibilidade sensorial e as famílias que planejam as saídas delas |
| Problema, em uma frase | Pessoas autistas não têm como saber, antes de sair, se um mercado ou uma estação vai estar barulhento, claro demais ou lotado, nem onde fica o lugar calmo mais próximo quando a sobrecarga vem |
| Como funciona | Sensores calculam um índice de sobrecarga de cada lugar; o app mostra o índice, os horários mais calmos e a previsão de barulho da região, e tem um botão de crise que leva ao refúgio calmo mais próximo, mesmo offline |
| O que cada parte faz | O ESP32 mede; a API em C# guarda, calcula, recebe os relatos e protege o dado de saúde com o módulo de privacidade; o ML prevê o barulho com o SP156; o app mostra; a VM hospeda e monitora; o Azure Boards garante a qualidade |
| Link do repositório | (preencher) |
| Indicador de impacto | Tempo do toque no botão de crise até a chegada ao refúgio (meta: menos de 10 minutos a pé) e a parcela das visitas feitas em horário calmo |
| ODS conectados | 3 (meta 3.4, saúde mental e bem-estar), 10 (meta 10.2, inclusão independentemente de deficiência) e 11 (metas 11.2 e 11.7, transporte e espaços públicos acessíveis) |

## As partes e quem faz cada uma

| Parte | Disciplina | Responsável | O que faz |
|---|---|---|---|
| API Calmaria (C#) | C# Software Development, SOA e Web Services, Cybersecurity | Luiz | O núcleo: lugares, leituras do sensor, índice, horários calmos, botão de crise, previsões, perfil anônimo, relatos e o módulo de privacidade (dado sensível, taint analysis, Risco Cidadão) |
| App Expo | Mobile Development e IoT | Integrante 2 | A porta de entrada do cidadão |
| Notebook de ML | IA e Machine Learning | Integrante 3 | Prevê o nível de barulho por distrito e dia com o SP156 real |
| VM Linux | Operating Systems | Integrante 4 | Sobe a API e o PostgreSQL sozinha, monitora, audita e aguenta falha |
| Sensor ESP32 | Physical Computing | Integrante 5 | Mede som, luz e movimento, calcula o ISS e publica por MQTT |
| Qualidade | Testing, Compliance e QA | Integrante 4 | Features e test cases no Azure Boards e o PDF; envia o link do pitch |
| Pitch | Vai na entrega de Testing | Integrante 5 | Monta, edita e publica o vídeo de até 4 minutos |

Troque "Integrante N" pelo nome de cada um quando a equipe confirmar.

### Como as partes conversam

```
ESP32 (Wokwi) --MQTT--> broker.hivemq.com --> API Calmaria (C#) <--REST-- App Expo
                                                  |   ^
                                    PostgreSQL ---+   +--- previsao_barulho.csv (notebook de ML)
                                                  |
                                     LLM externo (REST, só texto mascarado)

VM Linux (Operating Systems): instala, sobe e vigia a API e o PostgreSQL
Azure Boards (Testing): features e test cases da solução inteira
```

## Datas da equipe

| Dia | O que fica pronto |
|---|---|
| Qui 12/11 | Divisão fechada; repositório e projeto do Azure DevOps criados; entrevista marcada; todos leram o seu arquivo |
| Sex 13/11 | Dúvidas levadas aos professores, na aula |
| Sáb 14 e Dom 15 | Observação do lugar e entrevista; cada parte com o básico funcionando sozinha (com mock quando depender de outra) |
| Seg 16 e Ter 17 | Integração entre as partes; VM provisionando a API; test cases |
| Qua 18 | Funcionalidades congeladas às 20h; vídeos gravados |
| Qui 19 | Pitch gravado; READMEs revisados; **todos os envios até 21h**, com 3 horas de margem |

## Regras do briefing que valem para todos
- **IA como copiloto, com registro:** todo prompt relevante vai para o `prompts.md` da raiz do repositório
  (data, quem, parte, ferramenta, prompt e o que foi aproveitado). **Cada um sabe explicar cada linha que
  entregou** e o projeto inteiro, não só a sua parte.
- **Dados com cuidado:** nada de dado pessoal real. Usuários e relatos de teste são inventados e marcados como
  fictícios; as leituras dos sensores são simuladas e o README diz isso; o ML usa o SP156 real, que não
  identifica quem reclamou.
- **Ficha da CitizenTech** no README (nome, cidade e bairro, cidadão, problema em uma frase, como funciona, o
  que cada parte faz, link do repositório, indicador de impacto, ODS 3, 10 e 11).
- **Simples primeiro:** o básico funcionando antes de qualquer extra.

## O repositório (um só, uma pasta por parte)

```
calmaria/
├── README.md        Ficha da CitizenTech, mapa da rubrica, integrantes
├── prompts.md       Os prompts de IA usados por todos
├── docs/            Escopo, os arquivos de cada disciplina, observação e entrevista
├── api-dotnet/      API Calmaria (Luiz)
├── mobile/          App Expo (Integrante 2)
├── ml/              Notebook de ML (Integrante 3)
├── infra/           Scripts da VM Linux (Integrante 4)
├── iot/             ESP32 no Wokwi (Integrante 5)
├── qa/              PDF do Azure Boards e prints (Integrante 4)
└── security/        Evidências de Cybersecurity (Luiz)
```

---

# Parte 2: a sua parte

## Antes de começar
- VirtualBox (ou Hyper-V, no Windows Pro) e a ISO do **Ubuntu Server LTS**. Instalação mínima com o OpenSSH
  marcado; usuário administrador `aluno`.
- Os scripts são escritos no computador, no repositório, e copiados para a VM (`scp` ou pasta compartilhada).
- Tirar um **snapshot da VM logo depois da instalação**: é o "estado zero" para provar no vídeo que o
  `provisiona.sh` funciona do nada, quantas vezes for preciso.

## O que a disciplina pede
Uma infraestrutura Linux, numa VM, que **prepare o ambiente de forma automatizada por Shell Script** e
execute uma das aplicações web da equipe: rede, DNS, hostname, dependências, variáveis, serviço gerenciado
pelo systemd, monitoramento de recursos com geração de logs, auditoria e **teste de falha**.

**Entregáveis:** repositório com scripts Bash, configurações, evidências técnicas e README; infraestrutura
funcional; scripts de monitoramento com logs; teste de falha; vídeo de até 7 minutos mostrando o
provisionamento automatizado e a aplicação rodando. **Envio:** link do repositório no Teams até 19/11, 23h55.

| Critério | Pontos | Script |
|---|---|---|
| Provisionamento da infra (rede, DNS, IP) | 2 | `01-provisiona-infra.sh` |
| Provisionamento da aplicação (dependências, configurações, orquestração) | 2 | `02-provisiona-app.sh` |
| Monitoramento com alertas de CPU, disco e memória | 2 | `monitor.sh` |
| Monitoramento como serviço tolerante a falhas | 1 | `calmaria-monitor.service` e `.timer` |
| Logs de auditoria e de execução dos scripts | 1 | `lib/log.sh` e regras do auditd |
| Funcionamento completo | 2 | `provisiona.sh` de ponta a ponta e `05-teste-falha.sh` |

## O papel desta parte na Calmaria
É **onde a API Calmaria e o PostgreSQL moram**: uma VM que sai do zero, instala tudo, cria o banco, sobe a
API como serviço e se vigia. É a
prova de que a Calmaria roda fora do computador de quem programou. O app pode apontar para o IP da VM no
vídeo final.

## A VM
- **Ubuntu Server LTS** (24.04 ou 26.04), no VirtualBox ou no Hyper-V; 2 vCPU, 2 GB de RAM, 20 GB de disco.
- Rede: uma placa em modo bridge (ou NAT com redirecionamento das portas 22 e 80), para o celular da demo
  enxergar a VM.
- **Tudo depois da instalação do sistema é script.** Nada de configurar à mão; se precisou, vira linha de
  script.

## Estrutura da pasta

```
infra/
├── README.md
├── calmaria.env.exemplo         Variáveis (IP, gateway, DNS, hostname, versão do .NET, limites de alerta)
├── provisiona.sh                Orquestra: roda 01, 02, 03 e 04 em ordem e para no primeiro erro
├── lib/
│   └── log.sh                   Funções de log e auditoria usadas por todos os scripts
├── 01-provisiona-infra.sh
├── 02-provisiona-app.sh
├── 03-provisiona-monitoramento.sh
├── 04-auditoria.sh
├── 05-teste-falha.sh
├── monitor.sh
├── healthcheck.sh
├── systemd/
│   ├── calmaria-api.service
│   ├── calmaria-monitor.service
│   ├── calmaria-monitor.timer
│   ├── calmaria-health.service
│   └── calmaria-health.timer
├── nginx/
│   └── calmaria.conf
└── evidencias/                  Prints e trechos de log
```

Todos os scripts começam com `set -euo pipefail`, checam se rodam como root, carregam o `calmaria.env` e são
**idempotentes**: rodar duas vezes não quebra nada (checar antes de criar usuário, antes de escrever
arquivo, antes de instalar).

## Os scripts

### `lib/log.sh` (auditoria da execução)
- `log_info`, `log_erro`, `log_passo`: cada linha com data e hora ISO, hostname, usuário (`$SUDO_USER`), nome
  do script e mensagem, gravada em `/var/log/calmaria/provisionamento.log` e enviada ao journal (`logger -t
  calmaria`).
- `trap` de `ERR` que registra a linha e o comando que falharam; `trap` de `EXIT` que registra a duração e o
  código de saída.

### `01-provisiona-infra.sh` (rede, DNS, IP)
- Hostname `calmaria-srv` (`hostnamectl`) e `/etc/hosts` com `calmaria-srv` e `api.calmaria.local`.
- **IP fixo** pelo Netplan (`/etc/netplan/60-calmaria.yaml`, endereço, gateway e DNS do `calmaria.env`),
  aplicado com `netplan apply` e conferido com `ip -4 addr` e `ip route`.
- **DNS:** resolvedores no `systemd-resolved` e um nome local `api.calmaria.local` (no `/etc/hosts` ou com o
  `dnsmasq`, que deixa outras máquinas da rede resolverem o nome). Conferir com `resolvectl query` e `dig`.
- Fuso `America/Sao_Paulo` e NTP (`timedatectl`).
- Atualização e pacotes base: `curl`, `jq`, `unzip`, `git`, `ufw`, `nginx`, `sysstat`, `auditd`,
  `stress-ng`, `dnsutils`.
- Firewall `ufw`: libera 22 e 80, nega o resto.
- Usuário de sistema `calmaria`, sem shell, dono de `/opt/calmaria`.

### `02-provisiona-app.sh` (dependências, configuração, orquestração)
- Instala o **PostgreSQL**, que escuta só em `localhost`, e cria o usuário e o banco `calmaria` com uma senha
  gerada na hora (`openssl rand -base64 24`), guardada só no `api.env`. Se o banco já existe, não recria.
- Instala o runtime do ASP.NET Core 10 com o `dotnet-install.sh` oficial da Microsoft (`--channel 10.0
  --runtime aspnetcore`) em `/usr/share/dotnet`, se ainda não estiver.
- Clona ou atualiza o repositório em `/opt/calmaria/src` (ou copia o pacote publicado), publica a API para
  `linux-x64` em `/opt/calmaria/api`. Alternativa mais leve: publicar no Windows (`dotnet publish -r
  linux-x64`) e o script só copiar e instalar o runtime.
- Variáveis em `/etc/calmaria/api.env` (permissão 640, dono root e grupo calmaria):
  `ASPNETCORE_URLS=http://127.0.0.1:5080`, `ConnectionStrings__Calmaria=Host=localhost;Database=calmaria;Username=calmaria;Password=<gerada>`,
  `Mqtt__Habilitado=true`, `Privacidade__ChaveAdmin=<gerada>` e, se houver, `Llm__Chave`.
- Aplica as migrações (o pacote de migração do EF, `efbundle`, gerado pelo Luiz, ou a API aplicando
  ao iniciar).
- Instala `calmaria-api.service` (abaixo), `systemctl daemon-reload`, `enable --now`.
- Configura o **nginx** como proxy reverso: porta 80 para `127.0.0.1:5080`, com `server_name
  api.calmaria.local`; `nginx -t` antes de recarregar.
- Termina chamando `curl -fsS http://api.calmaria.local/health`; se falhar, o script falha.

`calmaria-api.service`:

```ini
[Unit]
Description=Calmaria API (.NET)
After=network-online.target postgresql.service
Wants=network-online.target postgresql.service
StartLimitIntervalSec=60
StartLimitBurst=5

[Service]
User=calmaria
WorkingDirectory=/opt/calmaria/api
EnvironmentFile=/etc/calmaria/api.env
ExecStart=/usr/share/dotnet/dotnet /opt/calmaria/api/Calmaria.Api.dll
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

### `monitor.sh` e `03-provisiona-monitoramento.sh` (alertas de CPU, disco e memória)
- `monitor.sh` lê CPU (`mpstat 1 1` ou `/proc/stat`), memória (`free`) e disco (`df -P /`), compara com os
  limites do `calmaria.env` (padrão: CPU 80%, memória 85%, disco 90%) e grava em
  `/var/log/calmaria/monitor.log` uma linha por coleta (`OK` ou `ALERTA`, com os valores).
- Em alerta: `logger -p user.warning -t calmaria-monitor`, mensagem no terminal dos usuários logados
  (`wall`) e, opcionalmente, um webhook (Discord ou Teams) se a URL estiver no `calmaria.env`.
- Também confere se `calmaria-api` e `postgresql` estão ativos (`systemctl is-active`) e registra.
- **Como serviço tolerante a falha:** `calmaria-monitor.service` (`Type=oneshot`) disparado por
  `calmaria-monitor.timer` a cada minuto (`OnUnitActiveSec=1min`, `Persistent=true`). Um timer não "morre":
  se uma coleta falha, a próxima roda no minuto seguinte, e a falha fica no journal.
- `healthcheck.sh` com o seu timer: chama `/health`; com 3 falhas seguidas, reinicia a API
  (`systemctl restart calmaria-api`) e registra o reinício.
- Rotação: `/etc/logrotate.d/calmaria` (diário, 7 cópias, compactado).

### `04-auditoria.sh` (logs de auditoria)
- Regras do `auditd` em `/etc/audit/rules.d/calmaria.rules`:
  `-w /etc/calmaria/ -p wa -k calmaria-config`, `-w /opt/calmaria/api/ -p wa -k calmaria-binarios`,
  `-w /etc/systemd/system/calmaria-api.service -p wa -k calmaria-servico`,
  `-w /etc/postgresql/ -p wa -k calmaria-banco`.
- Mostrar no vídeo: alterar o `api.env` e achar o registro com `ausearch -k calmaria-config`.
- O `provisionamento.log` é a trilha de quem rodou o quê e quando.

### `05-teste-falha.sh` (o teste de falha)
Cada cenário registra antes, durante e depois no log:
1. **Processo morto:** `kill -9` no PID da API; o systemd sobe de novo em 5 segundos; `/health` volta.
2. **CPU no limite:** `stress-ng --cpu 2 --timeout 90s`; o monitor registra `ALERTA CPU`.
3. **Disco quase cheio:** `fallocate` de um arquivo grande em `/var/tmp` até passar do limite; alerta de
   disco; o arquivo é apagado no fim.
4. **API travada:** `systemctl stop calmaria-api` simulando queda; o healthcheck detecta e religa.
5. **Banco fora do ar:** `systemctl stop postgresql`; o `/health` passa a acusar o banco e o healthcheck
   registra; ao religar o PostgreSQL, a API volta a responder.
6. **Reinício da VM:** `reboot`; tudo volta sozinho (serviços com `enable`).

## Evidências (`infra/evidencias/`)
Saída do `provisiona.sh` do início ao fim numa VM recém-instalada; `systemctl status` dos serviços e timers;
trechos do `monitor.log` com `OK` e `ALERTA`; `journalctl -u calmaria-api` mostrando o reinício depois do
`kill -9`; `ausearch -k calmaria-config`; `curl http://api.calmaria.local/api/lugares` respondendo; as tabelas no `psql`.

## Roteiro do vídeo (até 7 minutos)
| Tempo | O que mostrar |
|---|---|
| 0:00 a 0:40 | A Calmaria em uma frase e o papel da VM |
| 0:40 a 3:00 | VM recém-instalada; `sudo ./provisiona.sh`; comentar cada etapa enquanto roda (rede, DNS, pacotes, PostgreSQL, .NET, serviço, nginx) |
| 3:00 a 3:40 | A API respondendo pelo nome `api.calmaria.local`; o app ou o navegador consumindo |
| 3:40 a 5:00 | Monitoramento: timers, `monitor.log`, alerta de CPU com o `stress-ng` |
| 5:00 a 6:20 | Teste de falha: `kill -9` e o systemd religando; disco cheio; auditoria com `ausearch` |
| 6:20 a 7:00 | O que foi provado e os logs gerados |

## Ordem de construção
1. **Obrigatório:** VM instalada; `lib/log.sh`; `01` com hostname, IP, DNS e firewall.
2. **Obrigatório:** `02` subindo o PostgreSQL e a API (pedir ao Luiz o pacote publicado para `linux-x64` cedo,
   mesmo incompleto).
3. **Obrigatório:** `monitor.sh` com o timer; `healthcheck.sh`.
4. **Obrigatório:** `04` (auditd) e `05` (teste de falha).
5. **Obrigatório:** evidências, README e vídeo.
6. **Opcional:** backup diário do banco com `pg_dump` num timer; alerta por webhook; HTTPS com certificado
   autoassinado.

## Detalhes de implementação

### `calmaria.env.exemplo`
```bash
# Rede
CALMARIA_HOSTNAME=calmaria-srv
CALMARIA_DOMINIO=api.calmaria.local
CALMARIA_INTERFACE=enp0s3
CALMARIA_IP=192.168.0.50/24
CALMARIA_GATEWAY=192.168.0.1
CALMARIA_DNS="1.1.1.1 8.8.8.8"
# Aplicação
CALMARIA_DOTNET_CANAL=10.0
CALMARIA_PACOTE_API=/opt/calmaria/pacotes/calmaria-api-linux-x64.zip
CALMARIA_PORTA_INTERNA=5080
# Monitoramento (porcentagem)
LIMITE_CPU=80
LIMITE_MEMORIA=85
LIMITE_DISCO=90
WEBHOOK_ALERTA=
```
O arquivo real (`calmaria.env`) fica fora do Git, no `.gitignore`. A senha do banco e a chave administrativa
não ficam nele: o `02` gera as duas e grava só no `/etc/calmaria/api.env`.

### `nginx/calmaria.conf`
```nginx
server {
    listen 80;
    server_name api.calmaria.local _;
    access_log /var/log/nginx/calmaria.access.log;

    location / {
        proxy_pass http://127.0.0.1:5080;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Modelo de linha de log
```
2026-11-16T20:14:03-03:00 calmaria-srv aluno 02-provisiona-app.sh PASSO "Instalando o runtime do ASP.NET Core 10"
2026-11-16T20:15:41-03:00 calmaria-srv aluno 02-provisiona-app.sh OK "API respondeu /health em 1,2 s"
2026-11-16T20:31:00-03:00 calmaria-srv root monitor.sh ALERTA "CPU 93% (limite 80%)"
```

### Para o celular da demo enxergar a VM
Placa de rede em modo bridge, IP fixo da faixa da sua rede. No app, `EXPO_PUBLIC_API_URL=http://<IP da VM>`
(o nome `api.calmaria.local` só resolve dentro da VM e nas máquinas que tiverem a linha no `hosts`).


## Design da sua parte
O design da Calmaria foi fechado em 12/11. A fonte é `design/plano-de-design.md`; a versão visual, com as
telas, o visor, o painel e as cartelas desenhados, é `design/design-da-calmaria.html` (abrir no navegador);
cada cor em hex, nos dois temas, está em `design/tokens/resolvido.json`. As peças prontas (PNG, JPG e o PDF da capa) estão em `design/pecas/png/`, explicadas em
`design/pecas/LEIAME.md`; para refazê-las com os nomes e os links certos, `python design/pecas/exportar.py --help`. As cores foram conferidas por script
(WCAG 2.2 AA, 86 pares por tema). **Não invente cor, fonte ou tamanho fora destes.**

Regras que valem para todas as peças:
- **Nível em três sinais, sempre juntos:** Calmo = verde `#437E61`, letra C, círculo; Moderado = ocre
  `#BBA15E` (texto escuro por cima), letra M, quadrado; Intenso = tijolo `#B2574D`, letra I, losango. As
  mesmas cores para BAIXO, MEDIO e ALTO da previsão e para as faixas do Risco Cidadão. Cor só aparece quando
  significa nível.
- **Marca** `#3D5A73` (ardósia). Fundo claro `#F1F5F7` (nunca branco puro), cartão `#F8FBFD`, texto
  `#14191F`, apoio `#4D545B`. Fundo escuro `#14191F`, cartão `#22282E`, texto `#F8FBFD`. Crise e vídeo:
  `#1F2733`.
- **Fonte:** Atkinson Hyperlegible Next para texto, Atkinson Hyperlegible Mono para números de medição e
  código (Google Fonts, licença OFL).
- **Quieto:** nada pisca nem anima, todo ícone vem com texto (ícones Lucide, traço 2), frases literais.

As peças desta entrega (o código serve para avisar no grupo: "AP-09 pronta"):

| Código | Peça | Quem faz | Tamanho e formato | Especificação |
|---|---|---|---|---|
| MA-02 | Logotipo | Pronto em `design/marca/logotipo.svg` e `logotipo-invertido.svg` | SVG; mínimo 96 px de largura | Símbolo e "calmaria" em minúsculas, Atkinson Hyperlegible Next 700. Converter o texto em contorno antes de exportar PNG. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VM-01 | Perfil do terminal | Integrante 4 | Mono 20 pt, janela 1920 × 1080 | Esquema Calmaria (abaixo); hostname `calmaria-vm`, prompt curto. |
| VM-02 | Saída dos scripts | Integrante 4 | Texto no terminal e no log | `[AAAA-MM-DD hh:mm:ss] OK|AVISO|FALHA  mensagem`: o status escrito, além da cor. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |

**Esquema do terminal** (Windows Terminal, em `schemes`; no GNOME Terminal, as mesmas cores na paleta):

```json
{ "name": "Calmaria", "background": "#14191F", "foreground": "#F8FBFD",
  "cursorColor": "#B2C9D8", "selectionBackground": "#3A4148",
  "black": "#06090E", "red": "#E08F82", "green": "#7DAF95", "yellow": "#BBA15E",
  "blue": "#8AA8BE", "purple": "#B2C9D8", "cyan": "#55D6E7", "white": "#CED5DA",
  "brightBlack": "#6B737A", "brightRed": "#F3B7AC", "brightGreen": "#AACEBA", "brightYellow": "#D2C596",
  "brightBlue": "#B2C9D8", "brightPurple": "#D4E1EA", "brightCyan": "#A9EAF3", "brightWhite": "#F8FBFD" }
```

Fonte Atkinson Hyperlegible Mono em 20 pt, janela maximizada em 1920 × 1080. Na saída dos scripts, OK em
verde, AVISO em amarelo, FALHA em vermelho, sempre com a palavra escrita.

## Checklist de envio
- [ ] `provisiona.sh` roda do zero numa VM nova sem intervenção
- [ ] Scripts idempotentes e com log
- [ ] Monitoramento como serviço (timer) gerando `OK` e `ALERTA`
- [ ] Auditoria com auditd e com o log de provisionamento
- [ ] Teste de falha documentado
- [ ] README com como recriar a VM, as variáveis e as evidências
- [ ] Vídeo de até 7 minutos
- [ ] Link do repositório enviado no Teams por um integrante

---

# Parte 3: contratos com as outras partes

Estes formatos são combinados entre as partes. **Não mude nome de campo, rota, tópico nem coluna sem avisar a equipe.**

### Contrato: o que a API precisa para rodar (VM)

**O que o Luiz entrega para a VM** (até segunda, 16/11; antes disso, usar o substituto abaixo):
- `calmaria-api-linux-x64.zip`: a API publicada com `dotnet publish -c Release -r linux-x64
  --self-contained false`. Entrada: `Calmaria.Api.dll`.
- `efbundle`: o pacote de migrações do EF Core (`dotnet ef migrations bundle -r linux-x64`), que cria as
  tabelas no PostgreSQL com `./efbundle --connection "<string de conexão>"`.

**O que a VM precisa ter:** runtime do ASP.NET Core 10 (`dotnet-install.sh --channel 10.0 --runtime
aspnetcore`) e PostgreSQL (16 ou mais novo), escutando só em `localhost`.

**Variáveis de ambiente da API** (arquivo `/etc/calmaria/api.env`):

| Variável | Exemplo | Para quê |
|---|---|---|
| `ASPNETCORE_URLS` | `http://127.0.0.1:5080` | Porta interna; o nginx publica na 80 |
| `ConnectionStrings__Calmaria` | `Host=localhost;Database=calmaria;Username=calmaria;Password=<gerada>` | Banco |
| `Mqtt__Habilitado` | `true` | Liga o recebimento das leituras do ESP32 |
| `Privacidade__ChaveAdmin` | `<gerada>` | Chave das rotas administrativas |
| `Llm__Chave` | `<se houver>` | Chave do LLM; sem ela, a API usa frases prontas |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ambiente |

**Como saber que está no ar:** `curl -fsS http://127.0.0.1:5080/health` devolve `Healthy`;
`curl http://api.calmaria.local/api/lugares` devolve a lista de lugares.

**Substituto até a API chegar:** uma API mínima criada na própria VM com `dotnet new web -o placeholder`, com
uma rota `/health` que responde `Healthy` e uma `/api/lugares` que responde `[]`. Todos os scripts (systemd,
nginx, monitoramento, teste de falha) funcionam igual com ela; na segunda é só trocar o pacote.

---

# Parte 4: com quem falar

- **Luiz (API):** receber o pacote `linux-x64` e o `efbundle` até segunda, 16/11. Até lá, trabalhar com o substituto da Parte 3.
- **Integrante 2 (app):** passar o IP da VM para o app apontar para ela no vídeo.
- **Integrante 5 (pitch):** gravar 20 a 30 segundos da VM subindo sozinha.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
