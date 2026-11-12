# Physical Computing (IoT e IoB): o sensor de sobrecarga no ESP32

| | |
|---|---|
| Projeto | Calmaria, a CitizenTech da equipe na Global Solution 2026.2 da FIAP (Engenharia de Software, 3º ano) |
| Disciplina | Physical Computing: IoT e IoB (e você monta o pitch da equipe) |
| Responsável | Integrante 5 (também monta e publica o pitch) |
| Entrega | 19/11/2026, até 23h55, na tarefa da disciplina no Teams; meta da equipe: enviar até 21h |
| Pasta no repositório | `iot/` |
| Você depende de | Ninguém para começar; o teste ponta a ponta usa a API do Luiz; os trechos de todos para o pitch |
| Dependem de você | Luiz (a API recebe as leituras); Integrante 2 (o app mostra o índice); Integrante 4 (o link do pitch) |
| Vídeo | Dois: o de Physical, até 4 minutos, e o pitch da equipe, até 4 minutos |

> **Como usar este arquivo.** Ele foi escrito para ser entregue inteiro a uma IA (ChatGPT, Claude, Gemini,
> Copilot). Traz o projeto, o que a disciplina pede, a especificação da sua parte, os contratos com as outras
> partes e o passo a passo. Copie o prompt abaixo, cole na sua IA e, logo depois, cole (ou anexe) este arquivo
> inteiro. Revise tudo o que a IA gerar: você precisa saber explicar cada linha ao professor. Registre o
> prompt no `prompts.md` da raiz do repositório.

## Prompt para a sua IA

```text
Você vai me ajudar a fazer a minha parte de um projeto de faculdade em equipe, a Calmaria, na Global
Solution da FIAP. O briefing completo vem logo depois desta mensagem: leia tudo antes de responder.

Seu papel: engenheiro de sistemas embarcados (ESP32, Arduino em C++, Wokwi, MQTT).

Vamos montar no Wokwi o sensor da Calmaria: um ESP32 que mede som (potenciômetro), luz (fotoresistor) e movimento (PIR), calcula o índice de sobrecarga, mostra no display e nos LEDs e publica por MQTT no formato exato da Parte 3. Depois, vamos montar, editar e publicar o pitch da equipe (seção "O pitch").

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
- Conta gratuita no **Wokwi** (https://wokwi.com), novo projeto "ESP32" com Arduino.
- **MQTT Explorer** instalado no computador (ou o cliente web da HiveMQ) para ver as mensagens chegando.

## O que a disciplina pede
Um circuito com ESP32 no Wokwi, com sensores e componentes, que mostre como a tecnologia ajuda a resolver o
problema da CitizenTech. **O professor deixou claro: não é um projeto paralelo**, é o IoT agregando valor à
solução do grupo.

**Entregáveis:** link do projeto no Wokwi; link do vídeo de até 4 minutos apresentando a solução,
explicando como o circuito entra nela e mostrando o funcionamento; um arquivo `.txt` com o nome dos
integrantes. **Envio:** Teams até 19/11, 23h55. **Rubrica:** o briefing não traz; pedir ao professor na
sexta, 13/11.

## O papel desta parte na Calmaria
É **o sensor que alimenta o mapa**. Instalado num mercado ou numa estação, ele mede som, luz e movimento de
pessoas, calcula o Índice de Sobrecarga Sensorial (ISS), mostra o nível no local (para quem está ali) e
publica a leitura por MQTT. A API .NET recebe, grava e o app mostra. Sem ele, o mapa só teria dados de seed.

## Componentes no Wokwi

| Componente (peça do Wokwi) | Mede ou faz | Pino do ESP32 | Observação |
|---|---|---|---|
| ESP32 DevKit (`wokwi-esp32-devkit-v1`) | Controla tudo | | |
| Potenciômetro (`wokwi-potentiometer`) | Simula o microfone: nível de som | GPIO 34 (analógico) | Num lugar real seria um microfone como o MAX4466 ou o INMP441 |
| Sensor de luz (`wokwi-photoresistor-sensor`) | Luz em lux | GPIO 35 (analógico) | No Wokwi, a luz se ajusta num controle deslizante |
| Sensor de presença (`wokwi-pir-motion-sensor`) | Movimento de pessoas | GPIO 27 (digital) | Contar detecções por minuto como aproximação de lotação |
| Display OLED 128x64 (`board-ssd1306`) | Mostra o ISS e o nível | SDA GPIO 21, SCL GPIO 22 | Texto grande, sem animação |
| 3 LEDs (verde, amarelo, vermelho) com resistores de 220 ohms | Nível em luz, para quem não lê o display | GPIO 25, 26, 33 | Brilho baixo (PWM); **nunca piscando** |
| Botão (`wokwi-pushbutton`) | Opcional: "preciso de um lugar calmo" no totem | GPIO 14 | Publica um pedido num tópico à parte |

**Atenção ao ESP32:** com o Wi-Fi ligado, os pinos analógicos do ADC2 param de funcionar. Use só pinos do
ADC1 (GPIO 32 a 39) para o potenciômetro e o sensor de luz. GPIO 34 e 35 servem, e são só de entrada.

**Sem buzzer, de propósito.** Um aparelho para quem sofre com barulho não apita. Explicar isso no vídeo é
ponto de aderência ao problema.

## A lógica do firmware

```
a cada 1 segundo:
    lê o potenciômetro (0 a 4095) e converte para dB: 30 + valor / 4095 * 90   (faixa 30 a 120 dB)
    lê o sensor de luz e converte para lux (fórmula da documentação do Wokwi, ajustada para 12 bits e 3,3 V)
    se o PIR passou de LOW para HIGH, soma uma detecção na janela

a cada 5 segundos (na demo; num lugar real, a cada 1 minuto):
    somDb         = média do som na janela
    luzLux        = média da luz na janela
    movimentosMin = detecções na janela, convertidas para "por minuto"
    ISS e nível pela fórmula do contrato do ISS (Parte 3)
    atualiza o display e os LEDs
    publica o JSON do contrato MQTT (Parte 3) no tópico calmaria/v1/lugares/{LUGAR_ID}/leituras
```

- **Wi-Fi do Wokwi:** `WiFi.begin("Wokwi-GUEST", "", 6);` (rede aberta do simulador, que sai para a
  internet).
- **MQTT:** biblioteca `PubSubClient`, broker `broker.hivemq.com`, porta 1883, `clientId` único (por exemplo
  `calmaria-` mais o MAC), reconexão automática no `loop()`.
- **JSON:** biblioteca `ArduinoJson`. As duas vão no `libraries.txt` do projeto no Wokwi.
- **Constantes no topo do sketch:** `LUGAR_ID`, `DISPOSITIVO_ID`, `TOPICO`, pesos e faixas do ISS (iguais aos
  da API; se a equipe mudar a fórmula, muda nos dois lugares).
- **Display:** linha 1 o nome do lugar; no meio o ISS em fonte grande; embaixo o nível ("CALMO",
  "MODERADO", "INTENSO") e os três valores medidos.
- **Sem dado pessoal no payload**, porque o broker é público e qualquer um pode assinar o tópico. Esse é um
  dos pontos que o módulo de Cybersecurity confere.

### Ver a mensagem chegando (para testar sem a API)
Assinar o tópico num cliente MQTT qualquer, como o MQTT Explorer ou o websocket client da HiveMQ, em
`calmaria/v1/lugares/#`. Com a API .NET rodando, a leitura aparece em `GET /api/lugares/{id}` em poucos
segundos.

## O que entregar no repositório
```
iot/
├── sketch.ino
├── diagram.json        (exportado do Wokwi)
├── libraries.txt
├── README.md           Link do Wokwi, lista de peças, pinagem, como testar, prints
└── integrantes.txt     O .txt que a disciplina pede
```

## Roteiro do vídeo (até 4 minutos)
| Tempo | O que mostrar |
|---|---|
| 0:00 a 0:40 | O problema em uma frase e a Calmaria: onde o sensor fica (mercado, estação) e para quem |
| 0:40 a 1:30 | O circuito: cada sensor, o que ele simula, por que não há buzzer |
| 1:30 a 2:50 | Funcionando: girar o potenciômetro e mexer na luz e no PIR; o ISS subindo de "CALMO" para "INTENSO" no display e nos LEDs; a mensagem chegando no MQTT |
| 2:50 a 3:40 | A integração: o mesmo lugar mudando de cor no app (ou na resposta da API) |
| 3:40 a 4:00 | Como seria na vida real: microfone de verdade, instalação em estações parceiras |

## Ordem de construção
1. **Obrigatório:** circuito montado, leituras dos três sensores no monitor serial.
2. **Obrigatório:** cálculo do ISS, display e LEDs.
3. **Obrigatório:** Wi-Fi e MQTT publicando o JSON do contrato.
4. **Obrigatório:** teste ponta a ponta com a API .NET (combinar com o Luiz).
5. **Opcional:** botão de pedido de ajuda; média móvel para suavizar o som; dois dispositivos em lugares
   diferentes rodando ao mesmo tempo.

## Detalhes de implementação

### `libraries.txt` do projeto no Wokwi
```
PubSubClient
ArduinoJson
Adafruit SSD1306
Adafruit GFX Library
```

### O esqueleto do sketch
```cpp
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <Adafruit_SSD1306.h>

// Identidade do aparelho (tabela de lugares, Parte 3)
const int   LUGAR_ID       = 1;
const char* DISPOSITIVO_ID = "esp32-consolacao-01";
const char* NOME_LUGAR     = "Estacao Consolacao";

// Pinos
const int PINO_SOM = 34, PINO_LUZ = 35, PINO_PIR = 27;
const int LED_VERDE = 25, LED_AMARELO = 26, LED_VERMELHO = 33;

// Pesos e faixas do ISS: iguais aos da API (contrato do ISS, Parte 3)

void conectaWifi();            // "Wokwi-GUEST", sem senha, canal 6
void conectaMqtt();            // broker.hivemq.com:1883, clientId único, reconecta sozinho
float leSomDb();               // potenciômetro: 30 + valor / 4095 * 90
float leLuzLux();              // fotoresistor: fórmula abaixo
void contaMovimento();         // borda de subida do PIR soma 1 na janela
int calculaIss(float som, float luz, int movimentosMin);
const char* nivelDe(int iss);  // CALMO, MODERADO, INTENSO
void mostraDisplay(int iss, const char* nivel, float som, float luz, int mov);
void acendeLed(const char* nivel);   // brilho baixo com PWM, sem piscar
void publica(int iss, const char* nivel, float som, float luz, int mov);

void setup() { /* pinos, display, Wi-Fi, MQTT */ }
void loop()  { /* mantém o MQTT; lê a cada 1 s; a cada 5 s fecha a janela, calcula, mostra e publica */ }
```

### Luz em lux no ESP32 (fotoresistor do Wokwi)
A documentação do sensor de luz do Wokwi traz a conversão para Arduino (5 V e 10 bits). No ESP32 (3,3 V e 12
bits) fica:
```cpp
const float GAMMA = 0.7;
const float RL10  = 50;
float leLuzLux() {
  int bruto = analogRead(PINO_LUZ);
  float tensao = bruto / 4095.0 * 3.3;
  float resistencia = 2000 * tensao / (1 - tensao / 3.3);
  return pow(RL10 * 1e3 * pow(10, GAMMA) / resistencia, (1 / GAMMA));
}
```
Conferir o resultado com o controle deslizante de lux do Wokwi e ajustar se a leitura vier invertida.

### `integrantes.txt`
Nome completo e RM de cada um dos 5 integrantes, um por linha.


## O pitch (você monta e publica)
O pitch concorre ao prêmio de melhor GS: vídeo de até 4 minutos no YouTube, com a solução e as entregas que
mais somaram. O link vai na entrega de Testing, que é do Integrante 4: passe para ele até quinta, 19/11, às 18h.

| Tempo | O que mostrar | Quem grava |
|---|---|---|
| 0:00 a 0:30 | A frase da entrevista sobre a crise, a foto da estação e o número do Censo | Quem fez a entrevista |
| 0:30 a 0:50 | O problema em uma frase e quem é o cidadão | Integrante 5 |
| 0:50 a 2:30 | A demo de ponta a ponta: o potenciômetro sobe no Wokwi, o lugar fica "Intenso" no app, os horários calmos, a previsão de barulho do sábado, o botão de crise levando ao Parque Trianon | Integrantes 2 e 5, com o Luiz rodando a API |
| 2:30 a 3:20 | Como foi construído: as partes num diagrama, o dado de saúde protegido, a VM subindo sozinha, o modelo de ML | Luiz, Integrantes 3 e 4 |
| 3:20 a 4:00 | Indicadores de impacto, ODS 3, 10 e 11 e o que vem depois | Integrante 5 |

Como montar: cada um grava o seu trecho (tela e voz) até quarta, 18/11, às 20h e manda para você; você edita
num editor simples (Clipchamp, que vem no Windows, ou CapCut) com as cartelas prontas de `design/pecas/png/`,
confere o tempo, publica no YouTube e passa o link ao Integrante 4. Todo integrante aparece ou narra: o briefing cobra que todos saibam explicar o projeto.

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
| MA-04 | Símbolo em pixels | Integrante 5 | 40 × 24 px, 1 bit | Na tela de inicialização do visor: três linhas de 40 px, ondas de amplitude 3 e 1,5 e a reta, 2 px de espessura, 8 px entre elas. |
| SE-01 | Visor: medindo | Integrante 5 | 128 × 64 px, 1 bit | Lugar, índice grande, nível e as três medidas (posições abaixo). |
| SE-02 | Visor: inicializando | Integrante 5 | 128 × 64 px | Símbolo em pixels no alto, CALMARIA no tamanho 2 (y 30), CONECTANDO WI-FI no tamanho 1 (y 54). |
| SE-03 | Visor: sem MQTT | Integrante 5 | 128 × 64 px | Continua medindo; a última linha diz SEM MQTT. SO NO VISOR. |
| SE-04 | LEDs de nível | Integrante 5 | 3 LEDs, PWM 40 de 255 | Um aceso por vez, fixo, nunca piscando. Verde 25, amarelo 26, vermelho 33. |
| SE-05 | Fios no Wokwi | Integrante 5 | `diagram.json` | 3V3 `red`, GND `black`, SDA `blue`, SCL `gold`, sinais dos sensores `green`, LEDs na cor do LED; fios em ângulo reto. |
| AD-03 | Banner do repositório | Luiz | PNG 1280 × 640, até 1 MB | Prévia social do GitHub: fundo #1F2733, logotipo invertido, a frase em 52 px; 40 px livres na borda. |
| VI-01 | Kit de vídeo | Integrante 5 monta; todos usam | 1920 × 1080, 30 qps, H.264, AAC 48 kHz | Margem segura de 96 px nas laterais e 54 em cima e embaixo; texto de 40 px no mínimo; cortes secos; voz por volta de -16 LUFS; trilha 20 dB abaixo da voz. |
| VI-02 | Tarja de nome | Integrante 5 | Caixa de 140 px de altura, 96 px acima da borda; PNG transparente | Nome em 48, parte em 40, fundo #1F2733, texto #F8FBFD e #D4E1EA. |
| VI-03 | Abertura | Integrante 5 | 3 s | Logotipo invertido (letra de 120 px) e a frase do problema em 48, sobre #1F2733. |
| VI-04 | Cartela de bloco | Integrante 5 | 2 s | "3 de 5" em 40, o título do bloco em 96, o tempo em 40, sobre #F1F5F7. |
| VI-05 | Cartela final | Integrante 5 | 5 s | Indicador de impacto, ODS 3, 10 e 11 e o link do repositório, sobre #1F2733. |
| VI-06 | Miniatura do YouTube | Integrante 5 | 1280 × 720 JPG, até 2 MB | Fundo #1F2733, texto de 96 px, os três selos de nível. |
| VI-07 | Legendas | Quem grava cada vídeo | Arquivo .srt em pt-BR | Enviadas no YouTube, não gravadas na imagem; até 42 caracteres por linha. |

**Posições no visor** (fonte padrão do Adafruit GFX: 6 × 8 px por letra no tamanho 1, sem acento):

| Linha | y | Tamanho | Conteúdo | Máximo |
|---|---|---|---|---|
| Lugar | 0 a 7 | 1 | `EST. CONSOLACAO`, à esquerda | 21 letras |
| Índice | 11 a 31 | 3, e "/100" no 1 em y 25 | `78`, o grupo centralizado | 3 dígitos |
| Nível | 38 a 51 | 2 | `INTENSO`, centralizado | 10 letras |
| Medidas | 56 a 62 | 1 | `88dB 2300lx 23/min`, centralizado | 21 letras |

Largura de um texto: `strlen(s) * 6 * tamanho - tamanho`. Redesenhe a cada leitura (2 s): o número troca,
nada pisca. Sem dado: `SEM DADO` na linha do nível e `AGUARDANDO LEITURA` na última; sem Wi-Fi:
`SEM WI-FI: SO VISOR` na última.

**Pitch:** as cartelas entram assim: abertura (VI-03) antes da frase da entrevista; "2 de 5" a "5 de 5"
(VI-04) no começo de cada bloco; a final (VI-05) no fim. Tarja de nome (VI-02) na primeira vez que cada
integrante aparece. Tela do celular: gravação vertical centralizada no quadro, sobre `#1F2733`, com a frase
do que acontece à esquerda em 48 px. Se o Clipchamp não aceitar a fonte, monte as cartelas como imagem
1920 × 1080 e importe.

## Checklist de envio
- [ ] Projeto do Wokwi salvo e com link público
- [ ] `iot/` com sketch, `diagram.json`, `libraries.txt`, README e `integrantes.txt`
- [ ] Vídeo de até 4 minutos no YouTube, mostrando a integração com a solução
- [ ] No Teams: link do Wokwi, link do vídeo e o `.txt` com os integrantes, enviados por um integrante
- [ ] Pitch de até 4 minutos publicado no YouTube e o link passado ao Integrante 4 até quinta, 19/11, às 18h

---

# Parte 3: contratos com as outras partes

Estes formatos são combinados entre as partes. **Não mude nome de campo, rota, tópico nem coluna sem avisar a equipe.**

### Contrato: o Índice de Sobrecarga Sensorial (ISS)
O mesmo cálculo roda no ESP32 (para o display do aparelho) e na API (o valor que vale).

```
som_norm = limita((somDb  - 40)  / (100  - 40),  0, 1)
luz_norm = limita((luzLux - 300) / (3000 - 300), 0, 1)
mov_norm = limita(movimentosMin / 30, 0, 1)
ISS      = arredonda(100 * (0,5 * som_norm + 0,25 * luz_norm + 0,25 * mov_norm))
```
`limita(x, 0, 1)` prende o valor entre 0 e 1. O som pesa o dobro porque é o gatilho mais citado.

| ISS | `nivel` | Cor | Texto para o cidadão |
|---|---|---|---|
| 0 a 39 | `CALMO` | verde | "Calmo agora" |
| 40 a 69 | `MODERADO` | amarelo | "Moderado agora" |
| 70 a 100 | `INTENSO` | vermelho | "Intenso agora" |

Exemplo: 78,5 dB, 1.450 lux e 12 movimentos por minuto dão som 0,642, luz 0,426 e movimento 0,4, e ISS
`arredonda(100 * (0,321 + 0,106 + 0,1)) = 53`, nível `MODERADO`.

### Contrato: a mensagem MQTT (ESP32 para a API)
- **Broker:** `broker.hivemq.com`, porta `1883`. É público e sem senha: qualquer pessoa pode assinar o tópico,
  por isso **a mensagem nunca leva dado pessoal**.
- **Tópico:** `calmaria/v1/lugares/{lugarId}/leituras` (ex.: `calmaria/v1/lugares/1/leituras`).
- **Frequência:** a cada 5 segundos na demonstração (num lugar real seria 1 minuto).
- **Payload (JSON, UTF-8):**

```json
{
  "dispositivoId": "esp32-consolacao-01",
  "lugarId": 1,
  "somDb": 78.5,
  "luzLux": 1450,
  "movimentosMin": 12,
  "indice": 53,
  "nivel": "MODERADO"
}
```

| Campo | Tipo | Faixa aceita pela API |
|---|---|---|
| `dispositivoId` | texto | precisa estar cadastrado na API (um por lugar monitorado; ver a tabela de lugares) |
| `lugarId` | inteiro | o id do lugar onde o aparelho está |
| `somDb` | decimal | 0 a 140 |
| `luzLux` | decimal | 0 a 100.000 |
| `movimentosMin` | inteiro | 0 a 500 |
| `indice`, `nivel` | inteiro, texto | informativos: a API recalcula com a fórmula do ISS |

A API carimba a data e a hora de recebimento; o ESP32 não precisa de relógio. Para ver as mensagens sem a API,
assine `calmaria/v1/lugares/#` num cliente MQTT (MQTT Explorer ou o cliente web da HiveMQ).

### Contrato: os lugares do protótipo
Lugares reais da região; as **leituras são simuladas**. Coordenadas e distritos são aproximados: a equipe
confere no Google Maps e na observação antes do seed final.

| id | Lugar | Tipo | Latitude | Longitude | Distrito provável | Dispositivo |
|---|---|---|---|---|---|---|
| 1 | Estação Consolação (Linha 2) | ESTACAO | -23.5575 | -46.6609 | CONSOLACAO | `esp32-consolacao-01` |
| 2 | Estação Paulista (Linha 4) | ESTACAO | -23.5553 | -46.6622 | CONSOLACAO | `esp32-paulista-01` |
| 3 | Estação Trianon-Masp (Linha 2) | ESTACAO | -23.5614 | -46.6566 | BELA VISTA | `esp32-trianon-01` |
| 4 | Supermercado da Rua Augusta (escolhido na observação) | MERCADO | -23.5560 | -46.6580 | CONSOLACAO | `esp32-mercado-01` |
| 5 | Parque Trianon | REFUGIO | -23.5617 | -46.6573 | BELA VISTA | (sem sensor) |
| 6 | Casa das Rosas | REFUGIO | -23.5712 | -46.6446 | BELA VISTA | (sem sensor) |
| 7 | Refúgio escolhido na observação (biblioteca, igreja, sala silenciosa) | REFUGIO | (preencher) | (preencher) | (preencher) | (sem sensor) |

Ponto de referência para testes ("estou aqui"): `-23.5614, -46.6559` (Av. Paulista, em frente ao MASP).

---

# Parte 4: com quem falar

- **Luiz (API):** marcar no sábado um teste ponta a ponta: o ESP32 publica e a leitura aparece em `GET /api/lugares/{id}`.
- **Integrante 2 (app):** gravar juntos o trecho em que o potenciômetro sobe e o lugar fica "Intenso" no app.
- **Todos:** os trechos de vídeo do pitch até quarta, 18/11, às 20h (ver "O pitch").
- **Integrante 4 (Testing):** mandar o link do pitch até quinta, 19/11, às 18h, para ir no PDF e no envio.

Grupo da equipe para qualquer dúvida de contrato. Registre no `prompts.md` os prompts que usar.
