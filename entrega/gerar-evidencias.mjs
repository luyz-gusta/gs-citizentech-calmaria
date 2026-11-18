// Gera as evidências de C#, SOA e Cybersecurity a partir de execuções reais, numa rodada só:
//   1. o banco do zero: um schema "entrega" vazio no banco de desenvolvimento, as migrações e as tabelas;
//   2. os testes, a precisão e revocação, a prova do erro de compilação e o Semgrep;
//   3. a demonstração do antes e depois (security/demo-antes-depois.ps1);
//   4. a API de pé: um relato fictício com CPF, uma leitura publicada no broker MQTT, o Scalar e o painel.
// Saem os prints em api-dotnet/docs/evidencias/ e security/evidencias/, e as capturas para o vídeo em
// entrega/saida/capturas/.
//
// Precisa do PostgreSQL local com a conexão no user-secrets (api-dotnet/scripts/criar-banco-dev.ps1), do Edge ou
// do Chrome e, para o Semgrep, do Docker Desktop aberto. A chave de administrador é gerada aqui, vale só nesta
// rodada e nunca aparece: nem ela nem a senha do banco saem em print, log ou tela.
//
// Uso (da raiz do repositório):  node entrega/gerar-evidencias.mjs
import { spawn } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { copyFileSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import net from 'node:net';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { abrirNavegador } from './lib/navegador.mjs';
import { cartao, cartaoTabela } from './lib/moldes.mjs';
import {
  RAIZ, SAIDA, agoraEmSaoPaulo, comSchema, conexaoDeDesenvolvimento, esconder, esperar, limpar, pasta, rodar,
  rodarPowerShell,
} from './lib/util.mjs';

const API = resolve(RAIZ, 'api-dotnet');
const PROJETO_API = resolve(API, 'src', 'Calmaria.Api');
const DLL = resolve(PROJETO_API, 'bin', 'Debug', 'net10.0', 'Calmaria.Api.dll');
const PSQL = 'C:/Program Files/PostgreSQL/16/bin/psql.exe';
const PORTA = 5093;
const BASE = `http://localhost:${PORTA}`;
const CPF_FICTICIO = '529.982.247-25';
const BROKER = { host: 'broker.hivemq.com', porta: 1883 };

// Começa limpo. Se a pasta estiver em uso (um terminal aberto dentro dela), apaga só os arquivos: um log velho com
// "MQTT conectado" enganaria a espera pelo broker.
for (const nome of ['html', 'logs-api']) {
  const p = join(SAIDA, nome);
  try {
    rmSync(p, { recursive: true, force: true });
  } catch {
    for (const a of readdirSync(p)) rmSync(join(p, a), { recursive: true, force: true });
  }
}
const DOCS = pasta(API, 'docs', 'evidencias');
const SEC = pasta(RAIZ, 'security', 'evidencias');
const CAP = pasta(SAIDA, 'capturas');
const HTML = pasta(SAIDA, 'html');
const LOGS = pasta(SAIDA, 'logs-api');
const RODAPE = `Gerado por entrega/gerar-evidencias.mjs em ${agoraEmSaoPaulo()}, a partir da saída real do comando.`;

const etapa = (t) => console.log(`\n== ${t}`);
const semVazias = (t) => t.split('\n').filter((l) => l.trim()).join('\n');
const json = (o) => JSON.stringify(o, null, 2);

// ---------- MQTT: um PUBLISH com QoS 0, escrito à mão (o protocolo cabe em 30 linhas) ----------

function textoMqtt(s) {
  const b = Buffer.from(s, 'utf8');
  const tamanho = Buffer.alloc(2);
  tamanho.writeUInt16BE(b.length);
  return Buffer.concat([tamanho, b]);
}

function tamanhoRestante(n) {
  const bytes = [];
  do {
    let d = n % 128;
    n = Math.floor(n / 128);
    if (n > 0) d |= 128;
    bytes.push(d);
  } while (n > 0);
  return Buffer.from(bytes);
}

async function publicarMqtt(topico, carga) {
  const socket = net.connect(BROKER.porta, BROKER.host);
  await new Promise((ok, erro) => { socket.once('connect', ok); socket.once('error', erro); });
  const cabecalho = Buffer.concat([textoMqtt('MQTT'), Buffer.from([4, 0x02, 0, 30])]);
  const cliente = textoMqtt(`calmaria-evidencia-${randomBytes(4).toString('hex')}`);
  socket.write(Buffer.concat([Buffer.from([0x10]), tamanhoRestante(cabecalho.length + cliente.length), cabecalho, cliente]));
  const ack = await new Promise((ok, erro) => { socket.once('data', ok); setTimeout(() => erro(new Error('sem CONNACK')), 8000); });
  if (ack[0] !== 0x20 || ack[3] !== 0) throw new Error('O broker recusou a conexão.');
  const t = textoMqtt(topico);
  const p = Buffer.from(carga, 'utf8');
  socket.write(Buffer.concat([Buffer.from([0x30]), tamanhoRestante(t.length + p.length), t, p]));
  socket.end(Buffer.from([0xe0, 0]));
}

// ---------- a rodada ----------

async function main() {
  const { conexao, partes } = await conexaoDeDesenvolvimento();
  const conexaoEntrega = comSchema(conexao, 'entrega');
  const chave = randomBytes(24).toString('base64url');
  esconder(chave);
  const envBanco = { ConnectionStrings__Calmaria: conexaoEntrega };
  const envPg = {
    PGHOST: partes.host || 'localhost', PGPORT: partes.port || '5432',
    PGUSER: partes.username || partes['user id'] || partes.user, PGPASSWORD: partes.password,
    PGDATABASE: partes.database, PGCLIENTENCODING: 'UTF8',
  };
  const psql = (sql) => rodar(PSQL, ['-v', 'ON_ERROR_STOP=1', '-P', 'pager=off', '-c', sql], { env: envPg });

  const nav = await abrirNavegador();
  let api;

  async function imprimir(nome, html, destinos) {
    const arquivo = join(HTML, `${nome}.html`);
    writeFileSync(arquivo, html);
    // Janela baixa: o print da página inteira fica do tamanho do cartão, sem faixa em branco embaixo.
    const aba = await nav.novaAba({ largura: 1400, altura: 50, escala: 1.5 });
    await aba.ir(pathToFileURL(arquivo).href);
    const primeiro = join(destinos[0], `${nome}.png`);
    await aba.foto(primeiro, { inteira: true });
    for (const d of destinos.slice(1)) copyFileSync(primeiro, join(d, `${nome}.png`));
    await aba.fechar();
    console.log(`   ${nome}.png`);
  }

  try {
    // 1. O banco do zero
    etapa('Banco: schema "entrega" criado do zero no banco de desenvolvimento');
    const criar = await psql('DROP SCHEMA IF EXISTS entrega CASCADE; CREATE SCHEMA entrega;');
    if (criar.codigo !== 0) throw new Error(`psql: ${criar.saida}`);
    const ef = (args) => rodar('dotnet', ['tool', 'run', 'dotnet-ef', ...args, '--project', 'src/Calmaria.Infrastructure',
      '--startup-project', 'src/Calmaria.Api'], { cwd: API, env: { ...envBanco, ASPNETCORE_ENVIRONMENT: 'Development' } });
    await rodar('dotnet', ['tool', 'restore'], { cwd: API });
    const lista = await ef(['migrations', 'list']);
    const atualizar = await ef(['database', 'update']);
    if (atualizar.codigo !== 0) throw new Error(`dotnet ef database update falhou:\n${atualizar.saida}`);
    const tabelas = await psql('\\dt entrega.*');
    writeFileSync(join(CAP, 'migracoes.txt'), `${lista.saida}\n---\n${atualizar.saida}\n---\n${tabelas.saida}`);
    await imprimir('migracoes', cartao({
      titulo: 'Migrações: o banco criado do zero',
      legenda: 'Um schema vazio no PostgreSQL local; o EF Core lista as migrações (uma por módulo) e cria as tabelas. ' +
        'O "Failed executing DbCommand" é o EF procurando a tabela de histórico, que ainda não existe num banco vazio: é o esperado na primeira vez.',
      blocos: [
        { comando: 'dotnet ef migrations list', saida: semVazias(lista.saida), destaque: /^\d{14}_/ },
        { comando: 'dotnet ef database update', saida: semVazias(atualizar.saida), destaque: /Applying migration|^Done/ },
      ],
      rodape: RODAPE,
    }), [DOCS]);
    await imprimir('tabelas-postgresql', cartao({
      titulo: 'As tabelas no PostgreSQL',
      legenda: 'As tabelas dos três módulos (Sensorial, Comunidade e Privacidade) e o histórico de migrações do EF Core.',
      blocos: [{ comando: 'psql -c "\\dt entrega.*"', saida: semVazias(tabelas.saida) }],
      rodape: RODAPE,
    }), [DOCS]);

    // 2. Testes e as provas do módulo de Privacidade
    etapa('Testes (dotnet test)');
    const testes = await rodar('dotnet', ['test'], { cwd: API });
    writeFileSync(join(CAP, 'testes.txt'), testes.saida);
    const resumoTestes = testes.saida.split('\n').filter((l) => /Passed!|Failed!|Aprovado!|Com falha!/.test(l)).join('\n');
    if (testes.codigo !== 0 || !resumoTestes) throw new Error(`Os testes falharam:\n${testes.saida.slice(-3000)}`);
    await imprimir('testes', cartao({
      titulo: 'Os testes automatizados passando',
      legenda: 'MSTest: Calmaria.Privacidade.Tests (o motor de PII, o taint, o Risco Cidadão e a tradução) e Calmaria.Tests (domínio e API de ponta a ponta).',
      blocos: [{ comando: 'dotnet test', saida: resumoTestes, destaque: /Passed!/ }],
      rodape: RODAPE,
    }), [DOCS, SEC]);

    etapa('Precisão e revocação');
    await rodarPowerShell('security/medir-precisao.ps1');
    const precisao = readFileSync(join(RAIZ, 'security', 'relatorios', 'precisao-e-revocacao.md'), 'utf8');
    copyFileSync(join(RAIZ, 'security', 'relatorios', 'precisao-e-revocacao.md'), join(CAP, 'precisao.md'));
    const linhasTabela = limpar(precisao).split('\n').filter((l) => l.startsWith('| ') && !l.startsWith('| Regra') && !l.startsWith('|---'))
      .map((l) => l.split('|').slice(1, -1).map((c) => c.trim()));
    await imprimir('precisao-e-revocacao', cartaoTabela({
      titulo: 'Precisão e revocação do motor de PII',
      legenda: 'Amostra rotulada de 60 comentários fictícios (40 com dado pessoal, 20 sem, com armadilhas como "crise econômica" e CPF com dígito errado). O teste exige 90% em cada regra.',
      cabecalho: ['Regra', 'VP', 'FP', 'FN', 'Precisão', 'Revocação'],
      linhas: linhasTabela,
      rodape: 'Gerado por security/medir-precisao.ps1 (relatorios/precisao-e-revocacao.md).',
    }), [SEC]);

    etapa('Prova do erro de compilação');
    await rodarPowerShell('security/provar-erro-de-compilacao.ps1');
    const compilacao = limpar(readFileSync(join(RAIZ, 'security', 'relatorios', 'erro-de-compilacao.txt'), 'utf8'));
    writeFileSync(join(CAP, 'compilacao.txt'), compilacao);
    await imprimir('erro-de-compilacao', cartao({
      titulo: 'Taint pelo tipo: a linha vulnerável não compila dentro da API',
      legenda: 'As linhas do controller vulnerável coladas na API: o comentário é TextoNaoConfiavel, e log, banco, resposta e LLM só aceitam o texto que passou pelo Sanitizador.',
      blocos: [{ comando: 'powershell -File security/provar-erro-de-compilacao.ps1', saida: semVazias(compilacao), destaque: /error CS/ }],
      rodape: RODAPE,
    }), [SEC]);

    etapa('Semgrep (precisa do Docker aberto)');
    const semgrep = await rodarPowerShell('security/rodar-semgrep.ps1');
    if (semgrep.codigo === 0) {
      writeFileSync(join(CAP, 'semgrep.txt'), semgrep.saida);
      await imprimir('semgrep-antes-depois', cartao({
        titulo: 'Taint estático: o Semgrep antes e depois',
        legenda: 'As regras da Calmaria (security/semgrep/calmaria-taint.yaml): acusa o controller vulnerável e não acusa o corrigido nem a API.',
        blocos: [{ comando: 'powershell -File security/rodar-semgrep.ps1', saida: semVazias(semgrep.saida), destaque: /achado\(s\)/ }],
        rodape: RODAPE,
      }), [SEC]);
    } else {
      console.log('   O Semgrep não rodou (o Docker está aberto?). O print semgrep-antes-depois.png ficou como estava.');
    }

    // 3. A demonstração do antes e depois (deixa as duas varreduras no schema "entrega")
    etapa('Demonstração do antes e depois');
    const demo = await rodarPowerShell('security/demo-antes-depois.ps1', { env: { ...envBanco, Privacidade__ChaveAdmin: chave } });
    if (demo.codigo !== 0) throw new Error(`A demonstração falhou:\n${demo.saida}`);
    const resumoDemo = limpar(readFileSync(join(RAIZ, 'security', 'relatorios', 'demo-antes-depois.txt'), 'utf8'));
    writeFileSync(join(CAP, 'demo.txt'), resumoDemo);
    const logDepois = limpar(readFileSync(join(RAIZ, 'security', 'relatorios', 'log-do-depois-mascarado.log'), 'utf8'));
    // O bloco do corpo da requisição do relato, da linha "RequestBody" até o fim do JSON, já mascarado.
    const linhasDoLog = logDepois.split('\n');
    const marcada = linhasDoLog.findIndex((l) => l.includes('[removido]'));
    let linhasMascaradas = [];
    if (marcada >= 0) {
      let de = marcada;
      while (de > 0 && !linhasDoLog[de].includes('RequestBody')) de--;
      let ate = marcada;
      while (ate < linhasDoLog.length - 1 && linhasDoLog[ate].trim() !== '}') ate++;
      linhasMascaradas = linhasDoLog.slice(de, ate + 1);
    }
    writeFileSync(join(CAP, 'log-mascarado.txt'), linhasMascaradas.join('\n'));
    await imprimir('demo-antes-depois', cartao({
      titulo: 'Em execução: o Risco Cidadão antes e depois da correção',
      legenda: 'A API sobe duas vezes com o mesmo relato fictício (CPF de exemplo e "autista"): antes, sem o guarda de log e com o log do corpo ligado; depois, com o guarda.',
      blocos: [{ comando: 'powershell -File security/demo-antes-depois.ps1', saida: semVazias(resumoDemo), destaque: /^(ANTES|DEPOIS)/ }],
      rodape: RODAPE,
    }), [SEC]);
    await imprimir('log-mascarado', cartao({
      titulo: 'O log depois da correção: o CPF e o dado de saúde saem mascarados',
      legenda: 'O mesmo log de depuração do corpo da requisição, agora com o LoggerProviderComRedacao: a linha chega ao arquivo já sem o dado.',
      blocos: [{ rotulo: 'security/relatorios/log-do-depois-mascarado.log', saida: linhasMascaradas.join('\n'), destaque: /\[removido\]|<SAUDE>/ }],
      rodape: 'Do log em arquivo da demonstração (security/demo-antes-depois.ps1).',
    }), [SEC]);

    // 4. A API de pé
    etapa('API de pé para as capturas');
    api = spawn('dotnet', [DLL], {
      cwd: PROJETO_API, windowsHide: true, stdio: 'ignore',
      env: {
        ...process.env, ...envBanco, ASPNETCORE_ENVIRONMENT: 'Development', ASPNETCORE_URLS: BASE,
        Privacidade__ChaveAdmin: chave, LogEmArquivo__Pasta: LOGS, Privacidade__Varredura__PastaDeLogs: LOGS,
        Mqtt__Habilitado: 'true',
      },
    });
    for (let i = 0; ; i++) {
      try { if ((await fetch(`${BASE}/health`)).status < 500) break; } catch { /* subindo */ }
      if (i > 120) throw new Error('A API não subiu em 60 segundos.');
      await esperar(500);
    }

    const http = async (metodo, caminho, corpo, admin = false) => {
      const r = await fetch(BASE + caminho, {
        method: metodo,
        headers: { 'Content-Type': 'application/json; charset=utf-8', ...(admin ? { 'X-Admin-Key': chave } : {}) },
        body: corpo === undefined ? undefined : JSON.stringify(corpo),
      });
      const texto = await r.text();
      let dados;
      try { dados = JSON.parse(texto); } catch { dados = texto; }
      return { status: r.status, dados };
    };

    etapa('Relato fictício com CPF');
    const usuario = await http('POST', '/api/usuarios', {});
    const pedido = {
      usuarioId: usuario.dados.id, lugarId: 2, nota: 2, gatilhos: ['BARULHO', 'MULTIDAO'],
      comentario: `Relato fictício para a evidência. Meu CPF é ${CPF_FICTICIO} e meu filho é autista. Barulho demais às 18h.`,
      visitadoEm: new Date(Date.now() - 3600_000).toISOString(),
    };
    const criado = await http('POST', '/api/relatos', pedido);
    const listagem = await http('GET', '/api/relatos?tamanho=5');
    const banco = await psql('SELECT "Id", "LugarId", "Comentario" FROM entrega."Relatos" ORDER BY "Id" DESC LIMIT 3;');
    const achar = (o) => (Array.isArray(o) ? o : Object.values(o || {}).find(Array.isArray) || []);
    const publico = achar(listagem.dados).find((r) => r.id === criado.dados.id) ?? achar(listagem.dados)[0];
    writeFileSync(join(CAP, 'relato.json'), json({ pedido, resposta: criado, publico, banco: banco.saida }));
    await imprimir('relato-mascarado', cartao({
      titulo: 'Um relato com CPF e dado de saúde, do pedido à listagem pública',
      legenda: 'O comentário entra como TextoNaoConfiavel, passa pelo Sanitizador e só então é gravado e publicado. A resposta diz o que saiu, já mascarado.',
      blocos: [
        { rotulo: 'O que o app envia', comando: 'POST /api/relatos', saida: json(pedido), destaque: /comentario/ },
        { rotulo: `A resposta (${criado.status})`, saida: json(criado.dados), destaque: /\[removido\]|<SAUDE>|trecho/ },
        { rotulo: 'Na listagem pública (sem o autor)', comando: 'GET /api/relatos', saida: json(publico), destaque: /comentario/ },
        { rotulo: 'No banco', comando: 'psql -c "SELECT ... FROM entrega.\\"Relatos\\""', saida: semVazias(banco.saida), destaque: /\[removido\]/ },
      ],
      rodape: `CPF fictício (${CPF_FICTICIO}, o exemplo de dígito válido usado nos testes). ${RODAPE}`,
    }), [DOCS, SEC]);

    etapa('Leitura pelo MQTT');
    // A API leva alguns segundos para conectar e assinar o tópico: publicar antes disso é perder a mensagem.
    const conectou = () => readdirSync(LOGS).some((a) => readFileSync(join(LOGS, a), 'utf8').includes('MQTT conectado'));
    for (let i = 0; i < 60 && !conectou(); i++) await esperar(500);
    await esperar(1000);
    const leitura = { dispositivoId: 'esp32-consolacao-01', lugarId: 1, somDb: 83.4, luzLux: 1620, movimentosMin: 17, indice: 58, nivel: 'MODERADO' };
    const topico = 'calmaria/v1/lugares/1/leituras';
    rmSync(join(DOCS, 'leitura-mqtt.png'), { force: true });
    try {
      if (!conectou()) throw new Error('a API não conectou ao broker');
      await publicarMqtt(topico, JSON.stringify(leitura));
      let lugar;
      for (let i = 0; i < 40; i++) {
        lugar = await http('GET', '/api/lugares/1');
        if (lugar.dados?.ultimaLeitura?.somDb === leitura.somDb) break;
        lugar = undefined;
        await esperar(500);
      }
      if (!lugar) throw new Error('a leitura publicada não apareceu em GET /api/lugares/1');
      writeFileSync(join(CAP, 'mqtt.json'), json({ topico, leitura, lugar }));
      await imprimir('leitura-mqtt', cartao({
        titulo: 'Uma leitura chegando pelo MQTT',
        legenda: `Publicada no broker público ${BROKER.host}, no tópico do contrato com o ESP32. A API assina o tópico, recalcula o índice e grava; a leitura aparece no lugar.`,
        blocos: [
          { rotulo: `Publicado em ${topico}`, saida: json(leitura) },
          { rotulo: 'A API, segundos depois', comando: 'GET /api/lugares/1', saida: json(lugar.dados), destaque: /83\.4|"indice"|"nivel"|registradaEm/ },
        ],
        rodape: RODAPE,
      }), [DOCS]);
    } catch (erro) {
      console.log(`   A leitura pelo MQTT não deu: ${erro.message}. O print leitura-mqtt.png ficou de fora.`);
    }

    etapa('Scalar');
    const scalar = await nav.novaAba({ largura: 1440, altura: 900, escala: 2 });
    await scalar.ir(`${BASE}/scalar`);
    await scalar.esperarAte("document.body.innerText.includes('Privacidade') && document.body.innerText.includes('Comunidade')", 30000);
    await esperar(1500);
    // Os três grupos (módulos) fechados na barra lateral, para caberem na tela.
    await scalar.avaliar("[...document.querySelectorAll('button')].filter((b) => b.innerText.startsWith('Close Group')).forEach((b) => b.click()); true");
    await esperar(800);
    await scalar.foto(join(DOCS, 'scalar.png'));
    // Um endpoint com o exemplo: o POST de relatos, no grupo Comunidade.
    await scalar.avaliar("[...document.querySelectorAll('button')].find((b) => b.innerText.includes('Open Group - Comunidade'))?.click(); true");
    await scalar.esperarAte("document.getElementById('v1/tag/comunidade/POST/api/relatos')", 15000);
    await scalar.avaliar("document.getElementById('v1/tag/comunidade/POST/api/relatos').scrollIntoView(); true");
    await esperar(1500);
    await scalar.foto(join(DOCS, 'scalar-relato.png'));
    await scalar.fechar();
    console.log('   scalar.png e scalar-relato.png');

    etapa('Painel Risco Cidadão');
    const painelApi = await http('GET', '/api/privacidade/risco-cidadao', undefined, true);
    writeFileSync(join(CAP, 'painel.json'), json(painelApi.dados));
    const painel = await nav.novaAba({ largura: 1440, altura: 900, escala: 2 });
    await painel.ir(`${BASE}/seguranca/index.html`);
    await painel.avaliar(`sessionStorage.setItem('calmaria-chave-admin', ${JSON.stringify(chave)}); true`);
    await painel.recarregar();
    await painel.esperarAte("/^\\d+$/.test(document.getElementById('agora-valor').textContent.trim())", 30000);
    await esperar(2000);
    await painel.foto(join(SEC, 'painel-risco-cidadao.png'));
    await painel.foto(join(CAP, 'painel-inteiro.png'), { inteira: true });
    const recortes = { risco: 'agora-valor', categoria: 'por-categoria', destino: 'por-destino', alertas: 'alertas', fluxos: 'fluxos', historico: 'historico' };
    for (const [nome, id] of Object.entries(recortes)) {
      const r = await painel.avaliar(`(() => { const e = document.getElementById(${JSON.stringify(id)}).closest('.cartao');
        const b = e.getBoundingClientRect(); return { x: b.left + scrollX, y: b.top + scrollY, width: b.width, height: b.height }; })()`);
      await painel.foto(join(CAP, `painel-${nome}.png`), { recorte: r });
    }
    await painel.avaliar("sessionStorage.clear(); true");
    await painel.fechar();
    console.log('   painel-risco-cidadao.png e os recortes para o vídeo');
  } finally {
    if (api) api.kill();
    await nav.fechar();
  }
  console.log('\nPronto. Prints em api-dotnet/docs/evidencias/ e security/evidencias/; capturas em entrega/saida/capturas/.');
}

main().catch((erro) => {
  console.error(`\nFALHOU: ${limpar(erro.stack || erro.message)}`);
  process.exit(1);
});
