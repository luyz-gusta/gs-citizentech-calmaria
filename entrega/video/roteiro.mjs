// O roteiro do vídeo de Cybersecurity, na ordem de docs/disciplinas/cybersecurity.md ("Roteiro do vídeo"):
//   1. por que o dado de saúde é o pior dado para vazar   2. as regras de PII, os testes e a precisão
//   3. o taint (Semgrep, erro de compilação, correção)     4. o relato com CPF em execução
//   5. a IA traduzindo o achado mascarado                 6. o painel Risco Cidadão antes e depois
//
// Um só exemplo atravessa os seis blocos: o relato fictício com o CPF de exemplo e a frase "meu filho é autista".
// Todo número e todo trecho mostrado vem das capturas reais (entrega/saida/capturas), dos relatórios
// (security/relatorios) e do próprio código. Trecho de código na tela passa por linhaReal(), que confere letra por
// letra contra o arquivo (só o espaço, a quebra de linha, a elisão "…" e a máscara "***" podem mudar).
//
// Cada fala tem o "texto" (vai na legenda) e a "fala" (o que a voz lê: siglas soletradas, números por extenso).
import { readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { RAIZ, SAIDA } from '../lib/util.mjs';

const CAP = join(SAIDA, 'capturas');
const ler = (arquivo) => readFileSync(arquivo, 'utf8').replace(/^﻿/, '').replace(/\r\n/g, '\n');
const lerJson = (arquivo) => JSON.parse(ler(arquivo));

// ---------- a pronúncia ----------

const UNIDADES = ['zero', 'um', 'dois', 'três', 'quatro', 'cinco', 'seis', 'sete', 'oito', 'nove', 'dez', 'onze', 'doze',
  'treze', 'catorze', 'quinze', 'dezesseis', 'dezessete', 'dezoito', 'dezenove'];
const DEZENAS = ['', '', 'vinte', 'trinta', 'quarenta', 'cinquenta', 'sessenta', 'setenta', 'oitenta', 'noventa'];
const CENTENAS = ['', 'cento', 'duzentos', 'trezentos', 'quatrocentos', 'quinhentos', 'seiscentos', 'setecentos',
  'oitocentos', 'novecentos'];

/** Número inteiro de 0 a 999 por extenso. */
export function porExtenso(n) {
  if (n < 20) return UNIDADES[n];
  if (n < 100) return DEZENAS[Math.floor(n / 10)] + (n % 10 ? ` e ${UNIDADES[n % 10]}` : '');
  if (n === 100) return 'cem';
  if (n < 1000) return CENTENAS[Math.floor(n / 100)] + (n % 100 ? ` e ${porExtenso(n % 100)}` : '');
  throw new Error(`porExtenso: ${n}`);
}

/** O que a voz deve ler no lugar de cada termo (a legenda continua com a grafia certa). */
const PRONUNCIA = [
  [/Semgrep/g, 'Sêmgrep'], [/\btaint analysis\b/g, 'têint análises'], [/\btaint\b/g, 'têint'],
  [/TextoNaoConfiavel/g, 'Texto Não Confiável'], [/TextoSanitizado/g, 'Texto Sanitizado'],
  [/\bJSON\b/g, 'djêisson'], [/\bC#/g, 'cê xarpi'], [/\bLLM\b/g, 'éle éle ême'], [/\bMQTT\b/g, 'ême quê tê tê'],
  [/\bLGPD\b/g, 'éle gê pê dê'], [/\bPII\b/g, 'pê í í'], [/\bAPI\b/g, 'a pê í'], [/\bCPF\b/g, 'cê pê éfe'],
  [/\bRG\b/g, 'érre gê'], [/\bSUS\b/g, 'SUS'], [/\bIA\b/g, 'í á'], [/\bbroker\b/g, 'bróker'], [/\bbuild\b/g, 'bíld'],
  [/\bprompt\b/g, 'prômpti'], [/Tea time/g, 'tí taime'], [/\bcontroller\b/g, 'controler'],
  [/\b(\d{1,3})\b/g, (_, n) => porExtenso(Number(n))],
];
export const falar = (texto) => PRONUNCIA.reduce((t, [de, para]) => t.replace(de, para), texto);
/** Uma fala: o texto da legenda, o que a voz lê e, se pedir, uma pausa maior depois dela. */
const f = (texto, { pausa } = {}) => ({ texto, fala: falar(texto), pausa });

// ---------- trechos reais de código ----------

/**
 * Uma linha real de um arquivo, para mostrar na tela. "partes" quebra a linha em várias (a primeira leva o número);
 * cada parte pode usar "…" para cortar um pedaço e "***" para mascarar um valor. Se o que vai para a tela não
 * corresponder ao arquivo, o gerador para.
 */
function linhaReal(linhas, n, partes, extra = {}) {
  const original = linhas[n - 1].trim();
  const mostradas = partes ?? [original];
  const junto = mostradas.join(' ');
  const padrao = junto.split(/(…|\*\*\*)/).map((p) => (p === '…' ? '[\\s\\S]*?' : p === '***' ? '[^";]+' : p
    .replace(/[.*+?^${}()|[\]\\]/g, '\\$&').replace(/\s+/g, '\\s*'))).join('');
  if (!new RegExp(`^${padrao}$`).test(original)) {
    throw new Error(`O trecho da linha ${n} não confere com o arquivo:\n  tela:    ${junto}\n  arquivo: ${original}`);
  }
  return mostradas.map((t, i) => ({ n: i === 0 ? n : null, t, cont: i > 0, ...extra }));
}

// ---------- os dados reais ----------

export function dados() {
  const testes = ler(join(CAP, 'testes.txt'));
  const passaram = (dll) => Number((testes.match(new RegExp(`Passed:\\s+(\\d+).*${dll}`)) || [])[1] || 0);
  const testesModulo = passaram('Calmaria\\.Privacidade\\.Tests');
  const testesTotal = testesModulo + passaram('Calmaria\\.Tests\\.dll');

  const demo = ler(join(CAP, 'demo.txt'));
  const numeros = (rotulo) => {
    const m = demo.match(new RegExp(`${rotulo}\\s+Risco Cidadão (\\d+) \\((\\w+)\\): (\\d+) dado\\(s\\) exposto\\(s\\), (\\d+) barrado`));
    return { indice: Number(m[1]), faixa: m[2].toLowerCase(), expostos: Number(m[3]), barrados: Number(m[4]) };
  };

  const relatorio = (alvo) => lerJson(resolve(RAIZ, 'security', 'relatorios', `semgrep-${alvo}.json`));
  const sgAntes = relatorio('antes');
  const sgDepois = relatorio('depois');
  const sgApi = relatorio('api');

  const vulneravel = ler(resolve(RAIZ, 'security', 'exemplos', 'RelatoControllerVulneravel.cs')).split('\n');
  const corrigido = ler(resolve(RAIZ, 'security', 'exemplos', 'RelatoControllerCorrigido.cs')).split('\n');
  const tentativa = ler(resolve(RAIZ, 'security', 'exemplos', 'TentativaDeVazamentoNaApi.cs.txt')).split('\n');

  // O que cada achado do Semgrep pegou, pela própria linha do arquivo.
  const qualSaida = (linha) => {
    const t = vulneravel[linha - 1];
    if (/Password=/.test(t)) return 'senha no código';
    if (/logger\./.test(t)) return 'log';
    if (/Console\./.test(t)) return 'console';
    if (/PostAsJson|llm\./.test(t)) return 'LLM';
    return 'saída insegura';
  };
  const achados = new Map(sgAntes.results.map((r) => [r.start.line, qualSaida(r.start.line)]));
  const marca = (n) => (achados.has(n) ? { achado: achados.get(n) } : {});
  const codigoVulneravel = [
    ...linhaReal(vulneravel, 31, ['public const string Conexao = "…;Password=***";'], marca(31)),
    ...linhaReal(vulneravel, 59, ['logger.LogInformation(', '"Relato recebido no lugar {Lugar}: {Comentario}",',
      'corpo.LugarId, corpo.Comentario);'], marca(59)),
    ...linhaReal(vulneravel, 60, null, marca(60)),
    ...linhaReal(vulneravel, 63, ['await llm.PostAsJsonAsync("chat/completions",', 'new { texto = corpo.Comentario });'], marca(63)),
  ];
  const semMarca = [...achados.keys()].filter((n) => !codigoVulneravel.some((l) => l.n === n));
  if (semMarca.length) throw new Error(`Achado do Semgrep fora do trecho mostrado: linha(s) ${semMarca.join(', ')}`);

  const codigoCorrigido = [
    ...linhaReal(corrigido, 76, null, { foco: 'sanitizar' }),
    ...linhaReal(corrigido, 77, ['logger.LogInformation("Relato recebido no lugar {Lugar}; …",']),
    ...linhaReal(corrigido, 78),
    ...linhaReal(corrigido, 81, null, { foco: 'achado' }),
    ...linhaReal(corrigido, 82, null, { foco: 'achado' }),
    ...linhaReal(corrigido, 85, ['var relato = new Relato { LugarId = corpo.LugarId,', 'Comentario = limpo.Texto };'], { foco: 'texto' }),
  ];

  // Os erros de compilação da tentativa de vazar, com o porquê tirado do comentário logo acima da linha.
  const porQue = (linha) => {
    for (let i = linha - 2; i >= 0; i--) {
      const m = tentativa[i].match(/^\s*\/\/ \d\. [^:]+: (.+)$/);
      if (m) return m[1].replace(/\.$/, '');
    }
    return '';
  };
  const erros = ler(join(CAP, 'compilacao.txt')).split('\n')
    .map((l) => l.match(/\((\d+),(\d+)\): error (CS\d+)/))
    .filter((m) => m && m[3] !== 'CS8600')
    .map((m) => ({ linha: Number(m[1]), coluna: Number(m[2]), codigo: m[3], porque: porQue(Number(m[1])) }));
  const buildFalhou = /Resultado do build: FALHOU/.test(ler(join(CAP, 'compilacao.txt')));

  const relato = lerJson(join(CAP, 'relato.json'));
  const painel = lerJson(join(CAP, 'painel.json'));
  const alerta = painel.atual.alertas.find((a) => a.categoria === 'CPF' && a.destino === 'RESPOSTA_PUBLICA') || painel.atual.alertas[0];
  const logMascarado = ler(join(CAP, 'log-mascarado.txt')).match(/"comentario":\s*"([^"]+)"/)[1];
  const linhaDoBanco = relato.banco.split('\n').find((l) => new RegExp(`^\\s*${relato.resposta.dados.id}\\s*\\|`).test(l));
  const textoDoBanco = linhaDoBanco.split('|')[2].trim();

  const precisao = ler(join(CAP, 'precisao.md')).split('\n').filter((l) => l.startsWith('| ') && !l.startsWith('| Regra'))
    .map((l) => l.split('|').slice(1, -1).map((c) => c.trim()))
    .map((c) => ({ regra: c[0], precisao: c[4], revocacao: c[5] }));
  const amostra = ler(join(CAP, 'precisao.md')).match(/(\d+) comentários fictícios \((\d+) com dado pessoal, (\d+) sem\)/);
  const piso = Number(ler(join(CAP, 'precisao.md')).match(/Piso do teste: (\d+)%/)[1]);

  // As regras e os pesos, do próprio código do motor (as do JSON vêm do regras-pii.json).
  const regrasDir = resolve(RAIZ, 'api-dotnet', 'src', 'Calmaria.Privacidade', 'Regras');
  const regras = [];
  for (const arq of ['RegrasDeIdentificacao.cs', 'RegraSaude.cs']) {
    const fonte = ler(join(regrasDir, arq));
    for (const m of fonte.matchAll(/Id => "([^"]+)";[\s\S]*?Peso => (\d+);/g)) regras.push({ id: m[1], peso: Number(m[2]), json: false });
  }
  // O regras-pii.json aceita comentário de linha (como a API o lê); tira as linhas que começam com "//".
  const jsonDasRegras = JSON.parse(ler(join(regrasDir, 'regras-pii.json')).split('\n').filter((l) => !/^\s*\/\//.test(l)).join('\n'));
  for (const r of jsonDasRegras) regras.push({ id: r.id, peso: r.peso, json: true });
  const ordem = ['cpf', 'telefone', 'email', 'saude', 'segredo', 'localizacao-precisa', 'quase-identificador', 'rg', 'cartao-sus'];
  regras.sort((a, b) => ordem.indexOf(a.id) - ordem.indexOf(b.id));

  return {
    testesModulo, testesTotal, antes: numeros('ANTES'), depois: numeros('DEPOIS'),
    semgrep: { antes: sgAntes.results.length, depois: sgDepois.results.length, api: sgApi.results.length, arquivosDaApi: sgApi.paths.scanned.length },
    codigoVulneravel, codigoCorrigido, erros, buildFalhou,
    relato, alerta, doLlm: alerta.geradoPor === 'LLM', logMascarado, textoDoBanco,
    precisao, amostra: { total: Number(amostra[1]), com: Number(amostra[2]), sem: Number(amostra[3]) }, piso, regras,
    painel,
  };
}

// ---------- as cenas ----------

export const EQUIPE = [
  { nome: 'Guilherme Almeida', rm: '555180' },
  { nome: 'Luiz Gustavo da Silva', rm: '558358' },
  { nome: 'Rafael Duarte de Freitas', rm: '558644' },
  { nome: 'Rafael Gaspar Bragança Martins', rm: '557228' },
  { nome: 'Vinicius Monteiro Araújo', rm: '555088' },
];
export const NARRADOR = { nome: 'Luiz Gustavo da Silva', parte: 'RM 558358 · C#, SOA e Cybersecurity' };
export const REPOSITORIO = 'github.com/luyz-gusta/gs-citizentech-calmaria';

export const BLOCOS = [null, 'Por que este dado', 'As regras de PII', 'O taint analysis', 'Em execução',
  'A IA na remediação', 'O painel Risco Cidadão'];

export function roteiro(d = dados()) {
  const { antes, depois } = d;
  if (!d.buildFalhou) throw new Error('A captura da compilação não diz que o build falhou: refaça as evidências.');
  if (d.regras.length !== 9) throw new Error(`Esperava 9 regras de PII, achei ${d.regras.length}.`);
  const todas100 = d.precisao.filter((p) => p.regra !== 'TOTAL').every((p) => p.precisao === '100%' && p.revocacao === '100%');
  if (!todas100) throw new Error('A precisão mudou: reveja a fala do bloco 2 antes de gerar.');
  const extenso = (n) => String(n);

  return [
    { id: 'abertura', tipo: 'abertura', duracao: 3 },

    // 1. Por que (30 s)
    { id: 'c1', tipo: 'cartela', n: 1, duracao: 2 },
    {
      id: 'relato', tipo: 'relato', bloco: 1, titulo: 'O dado de saúde não pode vazar',
      falas: [
        f('A Calmaria é um mapa sensorial para pessoas autistas em São Paulo, com relatos de quem visitou cada lugar.'),
        f('Este relato é fictício: um CPF de exemplo e a frase meu filho é autista.'),
        f('Pela LGPD, dizer que alguém é autista é dado de saúde, um dado pessoal sensível.'),
        f('E esse texto tem quatro saídas perigosas: o broker MQTT público, os logs do servidor, a lista pública de relatos e o LLM, que é de outra empresa.'),
        f('O módulo de Privacidade fica no meio, e nenhuma delas recebe o dado.'),
      ],
    },

    // 2. As regras de PII (1 min)
    { id: 'c2', tipo: 'cartela', n: 2, duracao: 2 },
    {
      id: 'motor', tipo: 'motor', bloco: 2, titulo: 'Uma classe por regra, um peso por dado',
      falas: [
        f(`O motor de PII tem ${d.regras.length === 9 ? 'nove' : d.regras.length} regras, uma classe para cada uma, e recebe todas por injeção de dependência.`),
        f('No relato, disparam duas: a de CPF, que só conta com dígito verificador válido, e a de saúde.'),
        f('Se dois achados se sobrepõem, fica o de maior peso, e cada um já sai mascarado: o texto original não deixa o motor.'),
        f('RG e Cartão SUS vêm de um arquivo JSON, então uma regra nova entra sem recompilar.'),
      ],
    },
    {
      id: 'amostra', tipo: 'amostra', bloco: 2, titulo: 'Uma amostra com armadilhas',
      falas: [
        f(`Para medir, escrevemos ${porExtenso(d.amostra.total)} comentários fictícios: ${porExtenso(d.amostra.com)} com dado pessoal e ${porExtenso(d.amostra.sem)} sem.`),
        f('Entre eles há armadilhas, como crise econômica, teatro e Tea time.'),
        f('A primeira rodada achou um erro de verdade: a regra de telefone marcava número de protocolo. A regra foi corrigida.'),
      ],
    },
    {
      id: 'resultado', tipo: 'resultado', bloco: 2, titulo: 'Precisão e revocação, medidas',
      falas: [
        f(`Hoje as nove regras dão cem por cento de precisão e de revocação nessa amostra, e o teste falha se alguma cair abaixo de ${porExtenso(d.piso)}.`),
        f('Como foi a equipe que escreveu a amostra, esse resultado não garante nada fora dela.'),
        f(`São ${extenso(d.testesModulo)} testes no módulo e ${extenso(d.testesTotal)} no projeto.`),
      ],
    },

    // 3. O taint (1 min 30 s)
    { id: 'c3', tipo: 'cartela', n: 3, duracao: 2 },
    {
      id: 'camadas', tipo: 'camadas', bloco: 3, titulo: 'Da origem à saída, em três camadas',
      falas: [
        f('O taint analysis segue o dado do cidadão da origem até cada saída.'),
        f('A origem é o texto livre: o comentário, o apelido e o contato de apoio. As saídas são o log, a resposta pública, o LLM, o broker e os arquivos.'),
        f('Entre as pontas só existe um caminho, o Sanitizador, e três camadas conferem isso: o tipo, os guardas e o Semgrep.'),
      ],
    },
    {
      id: 'semgrep', tipo: 'semgrep', bloco: 3, titulo: 'Estático: o Semgrep antes e depois',
      falas: [
        f('A camada estática é o Semgrep, com regras próprias em modo taint.'),
        f(`No controller vulnerável, ele acusa ${porExtenso(d.semgrep.antes)} pontos: a senha escrita no código e o comentário cru indo para o log, para o console e para o LLM.`),
        f(`No controller corrigido e nos ${d.semgrep.arquivosDaApi} arquivos da API, nenhum.`),
      ],
    },
    {
      id: 'tipo', tipo: 'tipo', bloco: 3, titulo: 'Pelo tipo: a linha que vaza não compila', fim: 0.8,
      falas: [
        f('A camada do tipo é o próprio C#: o texto entra como TextoNaoConfiavel, e o log, o banco, a resposta pública e o LLM só aceitam TextoSanitizado.'),
        f('Colamos as linhas vulneráveis dentro da API, e o build falhou, com um erro em cada tentativa de vazar.'),
      ],
    },
    {
      id: 'correcao', tipo: 'correcao', bloco: 3, titulo: 'A correção: primeiro o Sanitizador',
      falas: [
        f('A correção é sempre a mesma: primeiro o Sanitizador, e ao LLM vai só o achado mascarado.'),
        f('A camada de execução tem dois guardas: um mascara o log, o outro mascara o JSON das rotas públicas. Eles pegam o que o tipo não vê.'),
      ],
    },

    // 4. Em execução (40 s)
    { id: 'c4', tipo: 'cartela', n: 4, duracao: 2 },
    {
      id: 'api', tipo: 'api', bloco: 4, titulo: 'O relato fictício, na API rodando',
      falas: [
        f('Agora, a API rodando. O app envia o relato fictício.'),
        f('A resposta já volta limpa: o CPF virou removido, autista virou a marca de saúde, e a lista do que saiu vem mascarada.'),
      ],
    },
    {
      id: 'saidas', tipo: 'saidas', bloco: 4, titulo: 'O mesmo texto limpo em cada saída',
      falas: [
        f('O banco grava o texto limpo. A listagem pública mostra o mesmo, sem o autor. E o log do corpo da requisição sai mascarado pelo guarda.'),
        f('Nenhuma saída viu o CPF.'),
      ],
    },

    // 5. A IA (40 s)
    { id: 'c5', tipo: 'cartela', n: 5, duracao: 2 },
    {
      id: 'achado', tipo: 'achado', bloco: 5, titulo: 'Ao LLM vai só o achado mascarado',
      falas: [
        f('Na remediação, a IA recebe só o achado mascarado: a categoria, o trecho com asteriscos, o destino e se foi barrado.'),
        f('O prompt é versionado no código e pede duas respostas: uma frase literal para o cidadão e a correção para a equipe.'),
      ],
    },
    {
      id: 'alerta', tipo: 'alerta', bloco: 5, titulo: 'O alerta que volta',
      falas: d.doLlm ? [
        f('Este é o alerta do CPF, como voltou do LLM: curto, literal e sem alarme.'),
        f('A chamada ao LLM também foi testada com um serviço simulado: resposta certa, erro, sem rede e resposta fora do formato.'),
      ] : [
        f('Este é o alerta do CPF: curto, literal e sem alarme.'),
        f('Nesta gravação não havia chave de LLM, então valeu a frase pronta de reserva, e o painel mostra o motivo.'),
        f('A chamada ao LLM foi testada com um serviço simulado: resposta certa, erro, sem rede e resposta fora do formato.'),
      ],
    },

    // 6. O painel (40 s)
    { id: 'c6', tipo: 'cartela', n: 6, duracao: 2 },
    {
      id: 'indice', tipo: 'indice', bloco: 6, titulo: 'O Risco Cidadão, de zero a cem',
      falas: [
        f('O painel Risco Cidadão resume tudo num número de zero a cem.'),
        f('Cada item vale o peso do dado vezes o peso da exposição, e só vinte por cento quando um guarda barrou o dado.'),
        f(`Antes da correção, com o CPF cru no log, o índice deu ${antes.indice}, risco ${antes.faixa}. ` +
          `Depois, o mesmo relato deu ${depois.indice}, risco ${depois.faixa}: ` +
          `${depois.expostos === 0 ? 'nenhum dado exposto' : `${porExtenso(depois.expostos)} dados expostos`} e ${porExtenso(depois.barrados)} fluxos barrados.`),
      ],
    },
    {
      id: 'painel', tipo: 'painel', bloco: 6, titulo: 'O painel, dentro da própria API', fim: 1.2,
      falas: [
        f('O painel roda dentro da própria API e mostra ainda o risco por tipo de dado e por destino, os alertas, os fluxos bloqueados e o histórico.'),
        f('O relato com CPF e dado de saúde passou por todas as saídas da Calmaria, e nenhuma delas recebeu o dado.'),
      ],
    },

    { id: 'final', tipo: 'final', duracao: 5 },
  ];
}
