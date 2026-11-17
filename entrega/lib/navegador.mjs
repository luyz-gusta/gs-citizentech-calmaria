// O Edge (ou o Chrome) sem janela, controlado pelo protocolo de depuração (CDP) com o WebSocket do próprio Node.
// Serve para os prints: tamanho de tela, tema, escala, página inteira ou um recorte de um elemento.
import { spawn } from 'node:child_process';
import { existsSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import net from 'node:net';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { esperar } from './util.mjs';

const NAVEGADORES = [
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
];

function portaLivre() {
  return new Promise((ok) => {
    const s = net.createServer();
    s.listen(0, '127.0.0.1', () => {
      const { port } = s.address();
      s.close(() => ok(port));
    });
  });
}

export async function abrirNavegador() {
  const exe = NAVEGADORES.find((n) => existsSync(n));
  if (!exe) throw new Error('Não achei o Edge nem o Chrome.');
  const porta = await portaLivre();
  const perfil = mkdtempSync(join(tmpdir(), 'calmaria-cdp-'));
  const processo = spawn(exe, [
    '--headless=new', `--remote-debugging-port=${porta}`, `--user-data-dir=${perfil}`, '--no-first-run',
    '--no-default-browser-check', '--hide-scrollbars', '--force-color-profile=srgb', '--mute-audio', 'about:blank',
  ], { stdio: 'ignore', windowsHide: true });

  let enderecoWs;
  for (let i = 0; i < 100 && !enderecoWs; i++) {
    try {
      enderecoWs = (await (await fetch(`http://127.0.0.1:${porta}/json/version`)).json()).webSocketDebuggerUrl;
    } catch {
      await esperar(150);
    }
  }
  if (!enderecoWs) throw new Error('O navegador sem janela não abriu.');

  const ws = new WebSocket(enderecoWs);
  await new Promise((ok, erro) => { ws.onopen = ok; ws.onerror = erro; });
  let ultimo = 0;
  const pendentes = new Map();
  const ouvintes = new Set();
  ws.onmessage = (evento) => {
    const m = JSON.parse(evento.data);
    if (m.id && pendentes.has(m.id)) {
      const { ok, erro } = pendentes.get(m.id);
      pendentes.delete(m.id);
      if (m.error) erro(new Error(m.error.message));
      else ok(m.result);
    } else if (m.method) {
      for (const o of ouvintes) o(m);
    }
  };
  const enviar = (method, params = {}, sessionId) => new Promise((ok, erro) => {
    const id = ++ultimo;
    pendentes.set(id, { ok, erro });
    ws.send(JSON.stringify({ id, method, params, sessionId }));
  });

  async function novaAba({ largura = 1440, altura = 900, escala = 1, tema = 'light' } = {}) {
    const { targetId } = await enviar('Target.createTarget', { url: 'about:blank' });
    const { sessionId } = await enviar('Target.attachToTarget', { targetId, flatten: true });
    const cmd = (m, p) => enviar(m, p, sessionId);
    await cmd('Page.enable');
    await cmd('Runtime.enable');
    await cmd('Emulation.setDeviceMetricsOverride', { width: largura, height: altura, deviceScaleFactor: escala, mobile: false });
    await cmd('Emulation.setEmulatedMedia', {
      features: [{ name: 'prefers-color-scheme', value: tema }, { name: 'prefers-reduced-motion', value: 'reduce' }],
    });

    const carregou = () => new Promise((ok) => {
      const o = (m) => {
        if (m.sessionId === sessionId && m.method === 'Page.loadEventFired') { ouvintes.delete(o); ok(); }
      };
      ouvintes.add(o);
    });

    const aba = {
      async ir(url) {
        const pronto = carregou();
        await cmd('Page.navigate', { url });
        await Promise.race([pronto, esperar(30000)]);
        await aba.avaliar('document.fonts ? document.fonts.ready.then(() => true) : true');
      },
      async recarregar() {
        const pronto = carregou();
        await cmd('Page.reload', {});
        await Promise.race([pronto, esperar(30000)]);
        await aba.avaliar('document.fonts ? document.fonts.ready.then(() => true) : true');
      },
      async avaliar(expressao) {
        const r = await cmd('Runtime.evaluate', { expression: expressao, awaitPromise: true, returnByValue: true });
        if (r.exceptionDetails) throw new Error(r.exceptionDetails.exception?.description || r.exceptionDetails.text);
        return r.result.value;
      },
      async esperarAte(expressao, limiteMs = 20000) {
        const fim = Date.now() + limiteMs;
        while (Date.now() < fim) {
          if (await aba.avaliar(`Boolean(${expressao})`)) return true;
          await esperar(250);
        }
        throw new Error(`A página não ficou pronta: ${expressao}`);
      },
      /** Print da tela visível, da página inteira ({ inteira: true }) ou de um retângulo ({ recorte }). */
      async foto(caminho, { inteira = false, recorte } = {}) {
        const params = { format: 'png', captureBeyondViewport: inteira || Boolean(recorte) };
        if (inteira) {
          const t = await aba.avaliar('({ w: document.documentElement.scrollWidth, h: document.documentElement.scrollHeight })');
          params.clip = { x: 0, y: 0, width: t.w, height: t.h, scale: 1 };
        } else if (recorte) {
          params.clip = { ...recorte, scale: 1 };
        }
        const { data } = await cmd('Page.captureScreenshot', params);
        writeFileSync(caminho, Buffer.from(data, 'base64'));
        return caminho;
      },
      /** O retângulo de um elemento na página (para recortar só ele). */
      async retangulo(seletor, margem = 0) {
        return aba.avaliar(`(() => {
          const r = document.querySelector(${JSON.stringify(seletor)}).getBoundingClientRect();
          return { x: r.left + scrollX - ${margem}, y: r.top + scrollY - ${margem},
                   width: r.width + ${2 * margem}, height: r.height + ${2 * margem} };
        })()`);
      },
      fechar: () => enviar('Target.closeTarget', { targetId }),
    };
    return aba;
  }

  async function fechar() {
    try { await enviar('Browser.close'); } catch { /* já fechou */ }
    ws.close();
    await esperar(500);
    try { processo.kill(); } catch { /* já saiu */ }
    rmSync(perfil, { recursive: true, force: true, maxRetries: 5, retryDelay: 300 });
  }

  return { novaAba, fechar };
}
