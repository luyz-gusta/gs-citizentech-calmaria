#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

// ---------- Identidade do aparelho (tabela de lugares do contrato) ----------
const int   LUGAR_ID       = 1;
const char* DISPOSITIVO_ID = "esp32-consolacao-01";   // precisa estar cadastrado na API
const char* NOME_LUGAR     = "EST. CONSOLACAO";       // sem acento: a fonte do visor não tem

// ---------- Rede e MQTT (contrato da mensagem) ----------
const char* WIFI_REDE   = "Wokwi-GUEST";   // rede aberta do simulador, que sai para a internet
const char* WIFI_SENHA  = "";
const int   WIFI_CANAL  = 6;               // canal fixo: o Wokwi conecta mais rápido
const char* MQTT_BROKER = "broker.hivemq.com";
const int   MQTT_PORTA  = 1883;
char topico[48];                           // calmaria/v1/lugares/{LUGAR_ID}/leituras
char clientId[40];                         // nome único desta conexão no broker

// O broker é público e sem senha: qualquer pessoa pode assinar o tópico.
// Por isso a mensagem leva só medidas do ambiente, nunca dado de pessoa.

// ---------- Pinos ----------
// Som e luz usam o ADC1 (GPIO 32 a 39): o ADC2 para de funcionar com o Wi-Fi ligado.
const int PINO_SOM = 34;   // potenciômetro, pino SIG
const int PINO_LUZ = 35;   // fotoresistor, pino AO (saída analógica)
const int PINO_PIR = 27;   // sensor de presença, pino OUT (saída digital)
const int LED_VERDE = 25, LED_AMARELO = 26, LED_VERMELHO = 33;
// Visor no I2C padrão do ESP32: SDA no GPIO 21 e SCL no GPIO 22.

// ---------- Conversão do som ----------
const int   ADC_MAXIMO   = 4095;   // o ADC do ESP32 tem 12 bits: lê de 0 a 4095
const float SOM_DB_MIN   = 30.0;   // potenciômetro no mínimo = 30 dB
const float SOM_DB_FAIXA = 90.0;   // potenciômetro no máximo = 30 + 90 = 120 dB

// ---------- Conversão da luz (fotoresistor do Wokwi) ----------
const float GAMMA      = 0.7;      // inclinação da curva do LDR (atributo "gamma" da peça)
const float RL10       = 50;       // resistência do LDR a 10 lux, em kOhm (atributo "rl10")
const float R_SERIE    = 10000;    // resistor de 10 kOhm que o módulo põe em série com o LDR
const float LUX_MAXIMO = 100000;   // maior valor de luz que a API aceita (contrato MQTT)

// ---------- Contrato do ISS: os mesmos números da API ----------
// Se a equipe mudar a fórmula, muda aqui e na API.
const float SOM_MIN_DB  = 40,  SOM_MAX_DB  = 100;   // abaixo de 40 dB conta 0; acima de 100 conta 1
const float LUZ_MIN_LUX = 300, LUZ_MAX_LUX = 3000;
const float MOV_MAX_MIN = 30;                       // 30 movimentos por minuto ou mais conta 1
const float PESO_SOM = 0.5, PESO_LUZ = 0.25, PESO_MOV = 0.25;   // o som pesa o dobro
const int   LIMITE_MODERADO = 40;                   // 0 a 39 CALMO
const int   LIMITE_INTENSO  = 70;                   // 40 a 69 MODERADO; 70 a 100 INTENSO

// ---------- Tempo ----------
const unsigned long INTERVALO_LEITURA_MS   = 1000;    // uma leitura por segundo
const unsigned long INTERVALO_JANELA_MS    = 5000;    // na demo; num lugar real seria 1 minuto
const unsigned long ESPERA_WIFI_INICIO_MS  = 10000;   // tempo máximo na tela de inicialização
const unsigned long INTERVALO_RECONEXAO_MS = 5000;    // de quanto em quanto tempo tenta reconectar
unsigned long ultimaLeituraMs   = 0;
unsigned long inicioJanelaMs    = 0;
unsigned long ultimaTentativaWifiMs = 0;
unsigned long ultimaTentativaMqttMs = 0;

// ---------- Janela de medição ----------
float somaSom = 0, somaLuz = 0;   // somas das leituras da janela, para tirar a média
int   amostras = 0;               // quantas leituras entraram na janela
int   deteccoes = 0;              // quantas vezes o PIR subiu de LOW para HIGH na janela
bool  pirAnterior = false;        // estado do PIR na volta anterior do loop

// ---------- Visor, LEDs e conexão ----------
const int LARGURA_VISOR = 128, ALTURA_VISOR = 64;
Adafruit_SSD1306 visor(LARGURA_VISOR, ALTURA_VISOR, &Wire, -1);
bool visorOk = false;
const int BRILHO_LED = 40;   // de 0 a 255: brilho baixo, para não ser mais um estímulo

WiFiClient redeCliente;
PubSubClient mqtt(redeCliente);

// =====================================================================
// Leituras
// =====================================================================

// Som: converte a posição do potenciômetro (0 a 4095) em decibéis (30 a 120 dB).
float leSomDb() {
  int bruto = analogRead(PINO_SOM);
  return SOM_DB_MIN + bruto / (float)ADC_MAXIMO * SOM_DB_FAIXA;
}

// Luz: converte a tensão do pino AO em lux.
// O módulo é um divisor de tensão: resistor de 10 kOhm ligado ao 3V3 e LDR ligado ao GND.
// Quanto mais luz, menor a resistência do LDR e menor a tensão em AO.
float leLuzLux() {
  int bruto = analogRead(PINO_LUZ);
  if (bruto <= 0) return LUX_MAXIMO;          // tensão zero: luz acima do que o sensor mede
  if (bruto >= ADC_MAXIMO) return 0;          // tensão máxima: escuro total

  float razao = bruto / (float)ADC_MAXIMO;                // fração da alimentação que chega em AO
  float resistencia = R_SERIE * razao / (1.0 - razao);    // resistência do LDR, em ohms
  float lux = pow(RL10 * 1e3 * pow(10, GAMMA) / resistencia, 1.0 / GAMMA);
  return constrain(lux, 0.0f, LUX_MAXIMO);
}

// Movimento: conta uma detecção cada vez que a saída do PIR sobe de LOW para HIGH.
void contaMovimento() {
  bool pirAgora = digitalRead(PINO_PIR) == HIGH;
  if (pirAgora && !pirAnterior) {
    deteccoes++;
  }
  pirAnterior = pirAgora;
}

// =====================================================================
// Índice de Sobrecarga Sensorial (contrato do ISS)
// =====================================================================

// Prende o valor entre 0 e 1.
float limita(float x) {
  if (x < 0) return 0;
  if (x > 1) return 1;
  return x;
}

int calculaIss(float somDb, float luzLux, int movimentosMin) {
  float somNorm = limita((somDb - SOM_MIN_DB) / (SOM_MAX_DB - SOM_MIN_DB));
  float luzNorm = limita((luzLux - LUZ_MIN_LUX) / (LUZ_MAX_LUX - LUZ_MIN_LUX));
  float movNorm = limita(movimentosMin / MOV_MAX_MIN);
  return (int)lroundf(100 * (PESO_SOM * somNorm + PESO_LUZ * luzNorm + PESO_MOV * movNorm));
}

const char* nivelDe(int iss) {
  if (iss < LIMITE_MODERADO) return "CALMO";
  if (iss < LIMITE_INTENSO)  return "MODERADO";
  return "INTENSO";
}

// =====================================================================
// Visor (posições das peças SE-01, SE-02 e SE-03 do design)
// Fonte padrão do Adafruit GFX: cada letra ocupa 6 x 8 px no tamanho 1.
// =====================================================================

// Largura de um texto na tela, em pixels.
int larguraTexto(const char* s, int tamanho) {
  return strlen(s) * 6 * tamanho - tamanho;
}

void escreve(const char* s, int x, int y, int tamanho) {
  visor.setTextSize(tamanho);
  visor.setCursor(x, y);
  visor.print(s);
}

void escreveCentralizado(const char* s, int y, int tamanho) {
  escreve(s, (LARGURA_VISOR - larguraTexto(s, tamanho)) / 2, y, tamanho);
}

// Símbolo da Calmaria em pixels (peça MA-04): três linhas de 40 px, 2 px de espessura,
// 8 px entre elas: uma onda de amplitude 3, uma de amplitude 1,5 e uma reta.
// As ondas vão diminuindo até a reta: do agitado ao calmo.
void desenhaSimbolo(int ox, int oy) {
  const float amplitude[3] = {3, 1.5, 0};
  for (int linha = 0; linha < 3; linha++) {
    int base = oy + 3 + linha * 8;
    int yAnterior = 0;
    for (int x = 0; x < 40; x++) {
      int y = (int)floor(base - amplitude[linha] * sin(x / 40.0 * PI * 3) + 0.5);
      int de = (x == 0) ? y : min(yAnterior, y);   // liga um ponto ao anterior,
      int ate = (x == 0) ? y : max(yAnterior, y);  // para a onda não ficar com buracos
      for (int yy = de; yy <= ate; yy++) {
        visor.drawPixel(ox + x, yy, SSD1306_WHITE);
        visor.drawPixel(ox + x, yy + 1, SSD1306_WHITE);   // segunda linha: 2 px de espessura
      }
      yAnterior = y;
    }
  }
}

// Tela de inicialização (SE-02).
void mostraInicio(const char* passo) {
  if (!visorOk) return;
  visor.clearDisplay();
  desenhaSimbolo(44, 1);
  escreveCentralizado("CALMARIA", 30, 2);
  escreveCentralizado(passo, 54, 1);
  visor.display();
}

// A última linha diz o que está acontecendo com a conexão.
// Sem conexão o aparelho continua medindo e mostrando no visor; só não publica.
const char* avisoConexao() {
  if (WiFi.status() != WL_CONNECTED) return "SEM WI-FI: SO VISOR";
  if (!mqtt.connected())             return "SEM MQTT. SO NO VISOR";
  return nullptr;   // tudo conectado: a última linha mostra as medidas
}

// Antes da primeira janela fechar ainda não há média para mostrar.
void mostraSemDado() {
  if (!visorOk) return;
  visor.clearDisplay();
  escreve(NOME_LUGAR, 0, 0, 1);
  escreveCentralizado("SEM DADO", 38, 2);
  const char* aviso = avisoConexao();
  escreveCentralizado(aviso ? aviso : "AGUARDANDO LEITURA", 56, 1);
  visor.display();
}

// Tela de medição (SE-01; SE-03 quando falta conexão). O visor é redesenhado
// inteiro a cada janela: o número troca, nada pisca nem anima.
void mostraDisplay(int iss, const char* nivel, float somDb, float luzLux, int movimentosMin) {
  if (!visorOk) return;
  visor.clearDisplay();

  // Linha 1: o lugar
  escreve(NOME_LUGAR, 0, 0, 1);

  // Meio: o índice grande e "/100" pequeno, os dois juntos centralizados
  char numero[4];
  snprintf(numero, sizeof(numero), "%d", iss);
  int larguraGrupo = larguraTexto(numero, 3) + 3 + larguraTexto("/100", 1);
  int x = (LARGURA_VISOR - larguraGrupo) / 2;
  escreve(numero, x, 11, 3);
  escreve("/100", x + larguraTexto(numero, 3) + 3, 25, 1);

  // Nível por escrito
  escreveCentralizado(nivel, 38, 2);

  // Última linha: as três medidas (no máximo 21 letras) ou o aviso de conexão
  const char* aviso = avisoConexao();
  if (aviso) {
    escreveCentralizado(aviso, 56, 1);
  } else {
    char luz[8];
    if (luzLux < 10000) snprintf(luz, sizeof(luz), "%ld", lroundf(luzLux));
    else                snprintf(luz, sizeof(luz), "%ldk", lroundf(luzLux / 1000));
    char medidas[24];
    snprintf(medidas, sizeof(medidas), "%lddB %slx %d/min", lroundf(somDb), luz, movimentosMin);
    escreveCentralizado(medidas, 56, 1);
  }

  visor.display();
}

// =====================================================================
// LEDs: um aceso por vez, fixo, com brilho baixo. Nunca pisca.
// =====================================================================
void acendeLed(const char* nivel) {
  analogWrite(LED_VERDE,    strcmp(nivel, "CALMO") == 0    ? BRILHO_LED : 0);
  analogWrite(LED_AMARELO,  strcmp(nivel, "MODERADO") == 0 ? BRILHO_LED : 0);
  analogWrite(LED_VERMELHO, strcmp(nivel, "INTENSO") == 0  ? BRILHO_LED : 0);
}

void apagaLeds() {
  analogWrite(LED_VERDE, 0);
  analogWrite(LED_AMARELO, 0);
  analogWrite(LED_VERMELHO, 0);
}

// =====================================================================
// Conexão: Wi-Fi e MQTT. Nada aqui trava a medição por muito tempo.
// =====================================================================
void conectaWifi() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_REDE, WIFI_SENHA, WIFI_CANAL);
  ultimaTentativaWifiMs = millis();
}

// Tenta conectar ao broker. Devolve true se ficou conectado.
bool conectaMqtt() {
  ultimaTentativaMqttMs = millis();
  Serial.printf("Conectando ao broker %s como %s... ", MQTT_BROKER, clientId);
  if (mqtt.connect(clientId)) {
    Serial.println("conectado.");
    return true;
  }
  Serial.printf("falhou (codigo %d). Nova tentativa em 5 s.\n", mqtt.state());
  return false;
}

// Chamada a cada volta do loop: reconecta sozinho o que tiver caído.
void mantemConexao() {
  unsigned long agora = millis();

  if (WiFi.status() != WL_CONNECTED) {
    if (agora - ultimaTentativaWifiMs >= INTERVALO_RECONEXAO_MS) {
      Serial.println("Sem Wi-Fi. Tentando reconectar...");
      WiFi.disconnect();
      conectaWifi();
    }
    return;   // sem Wi-Fi não adianta tentar o MQTT
  }

  if (!mqtt.connected()) {
    if (agora - ultimaTentativaMqttMs >= INTERVALO_RECONEXAO_MS) {
      conectaMqtt();
    }
    return;
  }

  mqtt.loop();   // mantém a conexão com o broker viva
}

// Monta o JSON do contrato e publica no tópico do lugar.
void publica(int iss, const char* nivel, float somDb, float luzLux, int movimentosMin) {
  if (!mqtt.connected()) {
    Serial.println("  nao publicado: sem conexao com o broker.");
    return;
  }

  JsonDocument doc;
  doc["dispositivoId"] = DISPOSITIVO_ID;
  doc["lugarId"]       = LUGAR_ID;
  doc["somDb"]         = roundf(somDb * 10) / 10.0;   // uma casa decimal, como no contrato
  doc["luzLux"]        = lroundf(luzLux);
  doc["movimentosMin"] = movimentosMin;
  doc["indice"]        = iss;     // informativo: a API recalcula com a mesma fórmula
  doc["nivel"]         = nivel;

  char payload[200];
  serializeJson(doc, payload, sizeof(payload));

  if (mqtt.publish(topico, payload)) {
    Serial.printf("  publicado em %s: %s\n", topico, payload);
  } else {
    Serial.println("  falha ao publicar.");
  }
}

// =====================================================================
// Janela: a cada 5 s tira as médias, calcula o ISS, mostra e publica
// =====================================================================
void fechaJanela(unsigned long duracaoMs) {
  if (amostras == 0) return;

  float somDb  = somaSom / amostras;
  float luzLux = somaLuz / amostras;
  // Converte as detecções da janela para "por minuto": em 5 s, multiplica por 12.
  int movimentosMin = (int)lroundf(deteccoes * 60000.0f / duracaoMs);

  int iss = calculaIss(somDb, luzLux, movimentosMin);
  const char* nivel = nivelDe(iss);

  mostraDisplay(iss, nivel, somDb, luzLux, movimentosMin);
  acendeLed(nivel);

  Serial.printf("janela: som %5.1f dB | luz %6.0f lx | movimento %3d/min | ISS %3d %s\n",
                somDb, luzLux, movimentosMin, iss, nivel);
  publica(iss, nivel, somDb, luzLux, movimentosMin);

  // Começa uma janela nova, zerada
  somaSom = 0;
  somaLuz = 0;
  amostras = 0;
  deteccoes = 0;
}

void setup() {
  Serial.begin(115200);
  analogReadResolution(12);

  pinMode(PINO_PIR, INPUT);
  pinMode(LED_VERDE, OUTPUT);
  pinMode(LED_AMARELO, OUTPUT);
  pinMode(LED_VERMELHO, OUTPUT);
  apagaLeds();

  Serial.println();
  Serial.println("Calmaria - sensor de sobrecarga sensorial");
  Serial.printf("Dispositivo %s | lugar %d | %s\n", DISPOSITIVO_ID, LUGAR_ID, NOME_LUGAR);
  Serial.println("Leituras simuladas. ISS, visor e LEDs a cada 5 s, publicados por MQTT.");

  visorOk = visor.begin(SSD1306_SWITCHCAPVCC, 0x3C);
  if (visorOk) {
    visor.setTextColor(SSD1306_WHITE);
    visor.setTextWrap(false);
  } else {
    Serial.println("Visor nao encontrado: confira os fios SDA (GPIO 21) e SCL (GPIO 22).");
  }

  // Tópico e nome da conexão
  snprintf(topico, sizeof(topico), "calmaria/v1/lugares/%d/leituras", LUGAR_ID);
  // O nome precisa ser único no broker público: se outra conexão usar o mesmo nome,
  // o broker derruba a anterior. No Wokwi vários projetos podem ter o mesmo MAC,
  // por isso o final do MAC vem junto com um número sorteado.
  uint8_t mac[6];
  WiFi.macAddress(mac);
  unsigned long sorteio = (random(0x10000) ^ micros()) & 0xFFFF;
  snprintf(clientId, sizeof(clientId), "calmaria-%02X%02X%02X-%04lX", mac[3], mac[4], mac[5], sorteio);
  mqtt.setServer(MQTT_BROKER, MQTT_PORTA);
  mqtt.setBufferSize(512);
  mqtt.setSocketTimeout(5);   // espera no máximo 5 s pela resposta do broker
  // Keepalive de 60 s (o padrão do PubSubClient é 15 s). O ESP32 conta esse tempo no relógio da
  // simulação, mas o broker cobra no tempo real; com o Wokwi a 20% da velocidade, 5 s de janela
  // viram 25 s reais e o broker derrubava a conexão entre uma publicação e outra.
  mqtt.setKeepAlive(60);

  // Wi-Fi: espera até 10 s na tela de inicialização; depois segue medindo de qualquer jeito
  mostraInicio("CONECTANDO WI-FI");
  Serial.print("Conectando ao Wi-Fi");
  conectaWifi();
  while (WiFi.status() != WL_CONNECTED && millis() - ultimaTentativaWifiMs < ESPERA_WIFI_INICIO_MS) {
    delay(250);
    Serial.print(".");
  }
  if (WiFi.status() == WL_CONNECTED) {
    Serial.printf(" conectado. IP %s\n", WiFi.localIP().toString().c_str());
    mostraInicio("CONECTANDO MQTT");
    conectaMqtt();
  } else {
    Serial.println(" sem resposta. Segue medindo e tenta de novo a cada 5 s.");
  }

  mostraSemDado();
  inicioJanelaMs = millis();
  ultimaLeituraMs = millis();
}

void loop() {
  mantemConexao();

  // O PIR é conferido a cada volta do loop, para não perder nenhuma subida.
  contaMovimento();

  // millis() no lugar de delay(): o programa não fica parado entre uma leitura e outra.
  unsigned long agora = millis();

  if (agora - ultimaLeituraMs >= INTERVALO_LEITURA_MS) {
    ultimaLeituraMs = agora;
    somaSom += leSomDb();
    somaLuz += leLuzLux();
    amostras++;
  }

  if (agora - inicioJanelaMs >= INTERVALO_JANELA_MS) {
    fechaJanela(agora - inicioJanelaMs);
    inicioJanelaMs = agora;
  }

  delay(10);   // pausa curta que deixa a simulação do Wokwi mais leve
}
