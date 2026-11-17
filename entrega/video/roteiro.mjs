// O roteiro do vídeo de Cybersecurity, na ordem de docs/disciplinas/cybersecurity.md ("Roteiro do vídeo"):
//   1. por que o dado de saúde é o pior dado para vazar   2. as regras de PII, os testes e a precisão
//   3. o taint (Semgrep, erro de compilação, correção)     4. o relato com CPF em execução
//   5. a IA traduzindo o achado mascarado                 6. o painel Risco Cidadão antes e depois
// Todo número e todo trecho mostrado vem das capturas reais (entrega/saida/capturas) e dos relatórios do Semgrep.
// Cada fala tem o "texto" (vai na legenda) e, quando a pronúncia pede, a "fala" (o que a voz lê).
import { readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { codigo, esc, lista, tabela } from '../lib/moldes.mjs';
import { RAIZ, SAIDA } from '../lib/util.mjs';

const CAP = join(SAIDA, 'capturas');
const ler = (arquivo) => readFileSync(arquivo, 'utf8').replace(/^﻿/, '');
const lerJson = (arquivo) => JSON.parse(ler(arquivo));
const img = (arquivo) => `<div class="img"><img src="${pathToFileURL(arquivo).href}" alt=""></div>`;

/** O que a voz deve ler no lugar de cada termo (a legenda continua com a grafia certa). */
const PRONUNCIA = [
  [/Semgrep/g, 'Sêmgrep'], [/\btaint\b/gi, 'têint'], [/TextoNaoConfiavel/g, 'Texto Não Confiável'],
  [/TextoSanitizado/g, 'Texto Sanitizado'], [/AchadoMascarado/g, 'Achado Mascarado'], [/\bJSON\b/g, 'jêison'],
  [/\bC#/g, 'C Sharp'], [/\[removido\]/g, 'removido'], [/<SAUDE>/g, 'saúde'], [/Tea time/g, 'Tí táime'],
  [/\bbroker\b/g, 'bróker'], [/\bLLM\b/g, 'éle éle ême'], [/\bMQTT\b/g, 'ême quê tê tê'], [/\bprompt\b/g, 'prômpt'],
];
const falar = (texto) => PRONUNCIA.reduce((t, [de, para]) => t.replace(de, para), texto);
const f = (texto) => ({ texto, fala: falar(texto) });

export function roteiro() {
  // ---------- os dados reais ----------
  const testes = ler(join(CAP, 'testes.txt'));
  const passaram = (dll) => Number((testes.match(new RegExp(`Passed:\\s+(\\d+).*${dll}`)) || [])[1] || 0);
  const testesModulo = passaram('Calmaria\\.Privacidade\\.Tests');
  const testesTotal = testesModulo + passaram('Calmaria\\.Tests\\.dll');

  const demo = ler(join(CAP, 'demo.txt'));
  const numeros = (rotulo) => {
    const m = demo.match(new RegExp(`${rotulo}\\s+Risco Cidadão (\\d+) \\((\\w+)\\): (\\d+) dado\\(s\\) exposto\\(s\\), (\\d+) barrado`));
    return { indice: Number(m[1]), faixa: m[2].toLowerCase(), expostos: Number(m[3]), barrados: Number(m[4]) };
  };
  const antes = numeros('ANTES');
  const depois = numeros('DEPOIS');

  const relatorio = (alvo) => lerJson(resolve(RAIZ, 'security', 'relatorios', `semgrep-${alvo}.json`));
  const sgAntes = relatorio('antes');
  const sgDepois = relatorio('depois');
  const sgApi = relatorio('api');
  const arquivosDaApi = sgApi.paths.scanned.length;
  const vulneravel = ler(resolve(RAIZ, 'security', 'exemplos', 'RelatoControllerVulneravel.cs')).split('\n');
  const qualSaida = (linha) => {
    const t = vulneravel[linha - 1];
    if (/Password=/.test(t)) return 'senha escrita no código';
    if (/logger\./.test(t)) return 'comentário cru no log';
    if (/Console\./.test(t)) return 'comentário cru no console';
    if (/PostAsJson|llm\./.test(t)) return 'comentário cru no LLM';
    return 'dado do cidadão em saída insegura';
  };
  const achadosSemgrep = sgAntes.results.map((r) => `  linha ${r.start.line}  ${qualSaida(r.start.line)}`);

  const tentativa = ler(resolve(RAIZ, 'security', 'exemplos', 'TentativaDeVazamentoNaApi.cs.txt')).split('\n');
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
    .map((m) => `(${m[1]},${m[2]}) ${m[3]}  ${porQue(Number(m[1]))}`);

  const relato = lerJson(join(CAP, 'relato.json'));
  const enviado = relato.pedido.comentario;
  const gravado = relato.resposta.dados.comentario;
  const trecho = (t) => t.replace(/^Relato fictício para a evidência\. /, '').replace(/ Barulho demais às 18h\.$/, '');
  const removidos = relato.resposta.dados.removidos.map((r) => `${r.categoria}  ${r.trecho}`);

  const painel = lerJson(join(CAP, 'painel.json'));
  const alerta = painel.atual.alertas.find((a) => a.categoria === 'CPF' && a.destino === 'RESPOSTA_PUBLICA') || painel.atual.alertas[0];
  const doLlm = alerta.geradoPor === 'LLM';

  const precisao = ler(join(CAP, 'precisao.md')).split('\n').filter((l) => l.startsWith('| ') && !l.startsWith('| Regra'))
    .map((l) => l.split('|').slice(1, -1).map((c) => c.trim())).map((c) => [c[0], c[4], c[5]]);

  // ---------- as cenas ----------
  const bloco = (n, titulo) => ({ n, titulo });
  const B = [null, bloco(1, 'Por que este dado'), bloco(2, 'As regras de PII'), bloco(3, 'O taint analysis'),
    bloco(4, 'Em execução'), bloco(5, 'A IA na remediação'), bloco(6, 'O painel Risco Cidadão')];
  const cab = (n) => `${n} de 6 · ${B[n].titulo}`;

  return [
    { tipo: 'peca', arquivo: 'vi-03-abertura.png', duracao: 3 },

    // 1. Por que (30 s)
    { tipo: 'cartela', n: 1, bloco: B[1], duracao: 2 },
    {
      tipo: 'slide', bloco: B[1], cabecalho: cab(1), titulo: 'O dado de saúde não pode vazar',
      corpo: lista([
        'Dizer que alguém é autista é dado de saúde: <b>LGPD, art. 5º, II</b>',
        'Saídas perigosas: o broker MQTT público, os logs, os relatos públicos e um LLM de outra empresa',
        'Um vazamento expõe a pessoa, a rotina dela e o telefone de quem cuida dela',
      ]),
      falas: [
        f('A Calmaria é um mapa sensorial para pessoas autistas em São Paulo.'),
        f('Dizer que uma pessoa é autista é dado pessoal sensível, de saúde, pela LGPD.'),
        f('E a Calmaria tem saídas perigosas: um broker MQTT público, os logs do servidor, os relatos que todo mundo lê e um LLM, que é serviço de outra empresa.'),
        f('Um vazamento expõe a pessoa, a rotina dela e o telefone de quem cuida dela. Este vídeo mostra como o módulo de Privacidade impede isso.'),
      ],
    },

    // 2. As regras de PII (1 min)
    { tipo: 'cartela', n: 2, bloco: B[2], duracao: 2 },
    {
      tipo: 'slide', bloco: B[2], cabecalho: cab(2), titulo: 'Uma classe por regra, um peso por dado',
      corpo: tabela(['Regra', 'Peso', 'O que pega'], [
        ['cpf', '4', 'Só com dígito verificador válido'],
        ['telefone', '3', 'Fixo e celular; ignora protocolo'],
        ['email', '3', 'Endereço de e-mail'],
        ['saude', '5', 'Autismo, TEA, CID F84, laudo, remédio'],
        ['segredo', '5', 'Senha, api_key, Bearer, sk-, AKIA'],
        ['localizacao-precisa', '4', 'Coordenada precisa com usuário'],
        ['quase-identificador', '2', 'CEP e nascimento juntos'],
        ['rg, cartao-sus', '4', 'Do JSON, sem recompilar'],
      ], [1]),
      falas: [
        f('O motor de parsing tem uma classe por regra, e recebe todas por injeção de dependência.'),
        f('O CPF só conta com dígito verificador válido. E a palavra crise só conta perto de um contexto de saúde: crise econômica não conta.'),
        f('Quando dois achados se sobrepõem, fica o de maior peso, e cada achado já sai mascarado: o texto original nunca sai do motor.'),
        f('RG e Cartão SUS vêm de um arquivo JSON, então uma regra nova entra sem recompilar.'),
      ],
    },
    {
      tipo: 'slide', bloco: B[2], cabecalho: cab(2), titulo: 'Precisão e revocação, medidas',
      corpo: `<div class="duas"><div class="compacta">${tabela(['Regra', 'Precisão', 'Revocação'], precisao, [1, 2])}</div>
        <div>${lista([
          '60 comentários fictícios: 40 com dado pessoal, 20 sem',
          'Armadilhas: "crise econômica", "teatro", "Tea time", CPF com dígito errado',
          'O teste exige 90% em cada regra',
          `${testesModulo} testes no módulo`,
        ])}</div></div>`,
      falas: [
        f('Para medir a robustez, montamos uma amostra de sessenta comentários fictícios, com armadilhas como teatro, Tea time e CPF com dígito errado.'),
        f('A primeira medição achou um erro de verdade: a regra de telefone marcava um número de protocolo. A regra foi corrigida.'),
        f('Hoje, as nove regras dão cem por cento de precisão e de revocação nessa amostra.'),
        f(`São ${testesModulo} testes só no módulo de Privacidade, e ${testesTotal} no projeto inteiro.`),
      ],
    },

    // 3. O taint (1 min 30 s)
    { tipo: 'cartela', n: 3, bloco: B[3], duracao: 2 },
    {
      tipo: 'slide', bloco: B[3], cabecalho: cab(3), titulo: 'Da origem à saída, em três camadas',
      corpo: lista([
        '<b>Origem:</b> o comentário, o apelido e o contato de apoio',
        '<b>Destinos inseguros:</b> log, resposta pública, LLM, broker e arquivos',
        '<b>Único caminho:</b> o Sanitizador, de TextoNaoConfiavel para TextoSanitizado',
        '<b>Camadas:</b> o tipo (compilação), os guardas (execução) e o Semgrep (estático)',
      ]),
      falas: [
        f('O taint analysis rastreia o dado do cidadão da origem até as saídas.'),
        f('A origem é o texto que o cidadão escreve: o comentário do relato, o apelido e o contato de apoio. Os destinos inseguros são o log, a resposta pública, o LLM, o broker e os arquivos.'),
        f('O único caminho entre os dois é o Sanitizador. E a verificação acontece em três camadas.'),
      ],
    },
    {
      tipo: 'slide', bloco: B[3], cabecalho: cab(3), titulo: 'Estático: o Semgrep antes e depois',
      corpo: codigo([
        '> security/rodar-semgrep.ps1',
        `antes   ${sgAntes.results.length} achados  RelatoControllerVulneravel.cs`,
        ...achadosSemgrep,
        `depois  ${sgDepois.results.length} achados  RelatoControllerCorrigido.cs`,
        `api     ${sgApi.results.length} achados  ${arquivosDaApi} arquivos da API`,
      ].join('\n'), /achados/),
      falas: [
        f('A camada estática é o Semgrep, com regras próprias em modo taint.'),
        f(`No controller vulnerável, ele acusa ${sgAntes.results.length} pontos: a senha escrita no código e o comentário cru indo para o log, para o console e para o LLM.`),
        f(`No controller corrigido, que passa pelo Sanitizador antes de qualquer saída, e nos ${arquivosDaApi} arquivos da API, não acusa nada.`),
      ],
    },
    {
      tipo: 'slide', bloco: B[3], cabecalho: cab(3), titulo: 'Pelo tipo: a linha vulnerável não compila',
      corpo: codigo(['> dotnet build  (com a tentativa colada na API)', ...erros, 'Build: FALHOU, como devia'].join('\n'), /CS\d+/),
      falas: [
        f('A primeira camada é o próprio sistema de tipos do C#. O texto do cidadão entra como TextoNaoConfiavel, e o log, o banco, a resposta pública e o LLM só aceitam o texto que passou pelo Sanitizador.'),
        f('Quando colamos as linhas do controller vulnerável dentro da API, elas nem compilam: o compilador barra cada tentativa.'),
      ],
    },
    {
      tipo: 'slide', bloco: B[3], cabecalho: cab(3), titulo: 'A correção: primeiro o Sanitizador',
      corpo: codigo([
        'var limpo = sanitizador.Sanitizar(corpo.Comentario);',
        'logger.LogInformation("{N} dado(s) removido(s)",',
        '    limpo.Removidos.Count);',
        'foreach (var achado in limpo.Removidos)',
        '    await tradutor.TraduzirAsync(achado);',
        'relato.Comentario = limpo.Texto;',
      ].join('\n'), /Sanitizar|limpo\.Texto/) +
        `<p class="apoio">Em execução, o LoggerProviderComRedacao e o FiltroRespostaPublica mascaram o que escapar.</p>`,
      falas: [
        f('A correção é sempre a mesma: primeiro o Sanitizador. Depois dele, só o texto limpo existe, e ao LLM vai apenas o achado mascarado.'),
        f('A terceira camada roda em execução: um guarda embrulha todos os provedores de log, e outro filtra o JSON das rotas públicas. Ele pega o que o tipo não vê, como o log de depuração do corpo da requisição.'),
      ],
    },

    // 4. Em execução (40 s)
    { tipo: 'cartela', n: 4, bloco: B[4], duracao: 2 },
    {
      tipo: 'slide', bloco: B[4], cabecalho: cab(4), titulo: 'Um relato fictício com CPF',
      corpo: codigo([
        '> POST /api/relatos',
        `"${trecho(enviado)}"`,
        '',
        `< ${relato.resposta.status} Created`,
        `"${trecho(gravado)}"`,
        ...removidos.map((r) => `removido: ${r}`),
      ].join('\n'), /^"Meu CPF é \[removido\]|^removido:/),
      falas: [
        f('Agora, em execução. O app envia um relato fictício com um CPF de exemplo e a palavra autista.'),
        f('A API responde com o comentário já limpo: o CPF virou removido, e o dado de saúde virou a marca saúde. A resposta diz o que saiu, mascarado.'),
      ],
    },
    {
      tipo: 'slide', bloco: B[4], cabecalho: cab(4), titulo: 'O mesmo texto em todas as saídas',
      corpo: codigo([
        'No banco:',
        `  ${trecho(gravado)}`,
        'Na listagem pública (sem o autor):',
        `  ${trecho(relato.publico.comentario)}`,
        'No log de depuração do corpo:',
        '  "comentario": "... Meu CPF é [removido] ..."',
      ].join('\n'), /\[removido\]/),
      falas: [
        f('O banco grava só o texto limpo. A listagem pública mostra o mesmo, sem o autor. E o log de depuração do corpo da requisição sai mascarado.'),
        f('Nenhuma das saídas viu o CPF.'),
      ],
    },

    // 5. A IA (40 s)
    { tipo: 'cartela', n: 5, bloco: B[5], duracao: 2 },
    {
      tipo: 'slide', bloco: B[5], cabecalho: cab(5), titulo: 'Ao LLM vai só o achado mascarado',
      corpo: codigo([
        'AchadoMascarado { categoria: CPF, trecho: 529.***.***-25,',
        '                  destino: RESPOSTA_PUBLICA, barrado: sim }',
        '  -> prompt versionado (Remediacao/prompt-alerta.md)',
        '  -> LLM: chat/completions, tempo limite, nova tentativa,',
        '     disjuntor; sem LLM, a frase pronta de reserva',
      ].join('\n'), /AchadoMascarado|prompt/),
      falas: [
        f('A tradução por IA recebe só o achado mascarado: a categoria, o trecho com asteriscos, o destino e se foi barrado. O dado real não tem como chegar ao serviço externo.'),
        f('O prompt é versionado no código e pede duas saídas: uma frase curta e literal para o cidadão, e a correção para a equipe. A resposta fica guardada, para não pagar a mesma pergunta duas vezes.'),
      ],
    },
    {
      tipo: 'slide', bloco: B[5], cabecalho: cab(5), titulo: 'O alerta que volta',
      corpo: `<p><b>Para o cidadão:</b> ${esc(alerta.paraCidadao)}</p>
        <p class="apoio"><b>Para a equipe:</b> ${esc(alerta.paraEquipe)}</p>
        <p class="apoio">${doLlm ? 'Gerado pelo LLM.' : `Gerado pela frase pronta de reserva. Motivo: ${esc(alerta.motivo)}`}</p>`,
      falas: doLlm ? [
        f('Este é o alerta que voltou do LLM: literal, curto e sem alarme para o cidadão, e com a correção para a equipe.'),
      ] : [
        f('Este é o alerta: literal, curto e sem alarme para o cidadão, e com a correção para a equipe.'),
        f('Nesta gravação não havia chave de LLM configurada, então a API usou a frase pronta de reserva, e o painel mostra o motivo. A chamada ao LLM foi testada com um serviço simulado: resposta certa, erro, falta de rede e resposta fora do formato.'),
      ],
    },

    // 6. O painel (40 s)
    { tipo: 'cartela', n: 6, bloco: B[6], duracao: 2 },
    {
      tipo: 'slide', bloco: B[6], cabecalho: cab(6), titulo: `Risco Cidadão: ${antes.indice} antes, ${depois.indice} depois`,
      corpo: img(join(CAP, 'painel-risco.png')),
      falas: [
        f('O painel Risco Cidadão junta tudo num número de zero a cem.'),
        f('Cada item vale o peso do dado vezes o peso da exposição, e cai para vinte por cento quando um guarda barrou o dado.'),
        f(`Antes da correção, com o CPF cru no log, o indicador ficou em ${antes.indice}, risco ${antes.faixa}. Depois, com o guarda ligado, o mesmo relato deu ${depois.indice}, risco ${depois.faixa}: ${depois.expostos === 0 ? 'nenhum dado exposto' : `${depois.expostos} dados expostos`} e ${depois.barrados} fluxos barrados.`),
      ],
    },
    {
      tipo: 'slide', bloco: B[6], cabecalho: cab(6), titulo: 'O risco por tipo de dado e por destino',
      corpo: `<div class="duas">${img(join(CAP, 'painel-categoria.png'))}${img(join(CAP, 'painel-destino.png'))}</div>`,
      falas: [
        f('O painel mostra o risco por tipo de dado e por destino, os alertas traduzidos, os fluxos bloqueados e o histórico das varreduras. Ele roda na própria API, sem build, e só fala com ela.'),
        f('Esse é o módulo de Privacidade da Calmaria: o dado de saúde não sai por onde não deve.'),
      ],
    },

    { tipo: 'peca', arquivo: 'vi-05-cartela-final.png', duracao: 5 },
  ];
}
