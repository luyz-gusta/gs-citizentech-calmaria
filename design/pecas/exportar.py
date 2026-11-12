"""Exporta as peças visuais da Calmaria em PNG, JPG e PDF.

Usa o Chrome ou o Edge sem janela e um servidor local só durante a exportação. Precisa de Python 3.9+
e do Pillow (pip install pillow) para o PNG sem transparência do ícone e o JPG da miniatura.

Uso:
  python exportar.py                       exporta tudo em png/
  python exportar.py --equipe "Luiz Gustavo da Silva;Ana Souza;Bruno Lima;Carla Dias;Davi Reis" \
      --repo github.com/usuario/calmaria --azure dev.azure.com/org/calmaria --youtube youtu.be/abc123
  python exportar.py cartela --n 2 --total 6 --titulo "O provisionamento" --tempo "0:40 a 2:10" \
      --saida png/os-cartela-2.png         uma cartela de capítulo para qualquer vídeo

A ordem da equipe é a da divisão: Luiz (C#, SOA e Cybersecurity), Mobile, ML, OS e Testing, Physical e pitch.
"""
import argparse
import functools
import http.server
import os
import re
import shutil
import subprocess
import sys
import tempfile
import threading
import unicodedata
import urllib.parse

AQUI = os.path.dirname(os.path.abspath(__file__))
DESIGN = os.path.dirname(AQUI)
SAIDA = os.path.join(AQUI, "png")

NAVEGADORES = [
    r"C:\Program Files\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
    r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    "google-chrome", "chromium", "chromium-browser", "microsoft-edge",
]

PARTES = [
    "A API · C#, SOA e Cybersecurity",
    "O app · Mobile Development",
    "O modelo de ML · IA e Machine Learning",
    "A VM e os testes · Operating Systems e Testing",
    "O sensor e o pitch · Physical Computing",
]

BLOCOS = [
    ("A frase da entrevista", "0:00 a 0:30"),
    ("O problema e o cidadão", "0:30 a 0:50"),
    ("A demo de ponta a ponta", "0:50 a 2:30"),
    ("Como foi construído", "2:30 a 3:20"),
    ("Impacto e o que vem depois", "3:20 a 4:00"),
]


def achar_navegador():
    for n in NAVEGADORES:
        if os.path.isfile(n):
            return n
        w = shutil.which(n)
        if w:
            return w
    sys.exit("Não achei o Chrome nem o Edge. Instale um dos dois ou ponha o caminho em NAVEGADORES.")


class Silencioso(http.server.SimpleHTTPRequestHandler):
    def log_message(self, *args):
        pass


def subir_servidor():
    handler = functools.partial(Silencioso, directory=DESIGN)
    srv = http.server.ThreadingHTTPServer(("127.0.0.1", 0), handler)
    threading.Thread(target=srv.serve_forever, daemon=True).start()
    return srv, f"http://127.0.0.1:{srv.server_address[1]}"


def slug(s):
    s = unicodedata.normalize("NFKD", s).encode("ascii", "ignore").decode()
    return re.sub(r"[^a-z0-9]+", "-", s.lower()).strip("-")


class Exportador:
    def __init__(self):
        self.nav = achar_navegador()
        self.srv, self.base = subir_servidor()
        self.perfil = tempfile.mkdtemp(prefix="calmaria-chrome-")
        self.feitos = []

    def _rodar(self, args, saida):
        os.makedirs(os.path.dirname(saida), exist_ok=True)
        cmd = [self.nav, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--no-first-run",
               "--no-default-browser-check", f"--user-data-dir={self.perfil}", "--virtual-time-budget=4000"] + args
        r = subprocess.run(cmd, capture_output=True, text=True, timeout=180)
        if not os.path.isfile(saida):
            sys.exit(f"Falhou ao gerar {saida}:\n{r.stderr[-800:]}")
        self.feitos.append(os.path.relpath(saida, AQUI))

    def url(self, caminho, query=None):
        q = ("?" + urllib.parse.urlencode(query)) if query else ""
        return f"{self.base}/{caminho}{q}"

    def png(self, caminho, query, w, h, nome, escala=1, fundo_transparente=True):
        saida = os.path.join(SAIDA, nome)
        args = [f"--window-size={w},{h}", f"--force-device-scale-factor={escala}", f"--screenshot={saida}"]
        if fundo_transparente:
            args.insert(0, "--default-background-color=00000000")
        self._rodar(args + [self.url(caminho, query)], saida)
        return saida

    def pdf(self, caminho, query, nome):
        saida = os.path.join(SAIDA, nome)
        self._rodar(["--no-pdf-header-footer", "--print-to-pdf-no-header", f"--print-to-pdf={saida}",
                     self.url(caminho, query)], saida)
        return saida

    def fechar(self):
        self.srv.shutdown()
        shutil.rmtree(self.perfil, ignore_errors=True)


def sem_alfa(caminho, fundo="#3D5A73"):
    try:
        from PIL import Image
    except ImportError:
        print("  Pillow ausente: o ícone ficou com canal alfa (pip install pillow).")
        return
    im = Image.open(caminho).convert("RGBA")
    base = Image.new("RGBA", im.size, fundo)
    base.alpha_composite(im)
    base.convert("RGB").save(caminho, "PNG", optimize=True)


def para_jpg(caminho_png, caminho_jpg):
    try:
        from PIL import Image
    except ImportError:
        print("  Pillow ausente: a miniatura ficou só em PNG (pip install pillow).")
        return None
    Image.open(caminho_png).convert("RGB").save(caminho_jpg, "JPEG", quality=90, optimize=True)
    os.remove(caminho_png)
    return caminho_jpg


def tudo(a):
    equipe = [n.strip() for n in a.equipe.split(";") if n.strip()]
    ex = Exportador()
    try:
        # Marca e app (AP-01 a AP-04, MA-02, MA-03, PA-03)
        sem_alfa(ex.png("marca/icone-app.svg", None, 1024, 1024, "ap-01-icone-app.png"))
        ex.png("marca/icone-frente.svg", None, 1024, 1024, "ap-02-icone-frente.png")
        ex.png("marca/icone-mono.svg", None, 1024, 1024, "ap-03-icone-mono.png")
        ex.png("marca/abertura.svg", None, 1024, 1024, "ap-04-abertura.png")
        ex.png("marca/abertura-escuro.svg", None, 1024, 1024, "ap-04-abertura-escuro.png")
        ex.png("marca/favicon.svg", None, 32, 32, "pa-03-favicon-32.png")
        ex.png("pecas/moldes/logotipo.html", {"versao": "cor"}, 800, 200, "ma-02-logotipo.png")
        ex.png("pecas/moldes/logotipo.html", {"versao": "invertido"}, 880, 280, "ma-03-logotipo-invertido.png", fundo_transparente=False)
        # Repositório e diagrama (AD-02, AD-03)
        ex.png("pecas/moldes/diagrama.html", None, 1920, 1080, "ad-02-diagrama-arquitetura.png", fundo_transparente=False)
        ex.png("pecas/moldes/banner.html", None, 1280, 640, "ad-03-banner-repositorio.png", fundo_transparente=False)
        # Vídeo (VI-02 a VI-06)
        ex.png("pecas/moldes/video.html", {"peca": "abertura"}, 1920, 1080, "vi-03-abertura.png", fundo_transparente=False)
        for i, (titulo, tempo) in enumerate(BLOCOS, 1):
            ex.png("pecas/moldes/video.html", {"peca": "bloco", "n": i, "total": len(BLOCOS), "titulo": titulo, "tempo": tempo},
                   1920, 1080, f"vi-04-cartela-bloco-{i}.png", fundo_transparente=False)
        ex.png("pecas/moldes/video.html", {"peca": "final", "repo": a.repo, "equipe": " · ".join(equipe)},
               1920, 1080, "vi-05-cartela-final.png", fundo_transparente=False)
        for i, nome in enumerate(equipe):
            parte = PARTES[i] if i < len(PARTES) else ""
            ex.png("pecas/moldes/video.html", {"peca": "tarja", "nome": nome, "parte": parte},
                   1920, 1080, f"vi-02-tarja-{i + 1}-{slug(nome)}.png")
        mini = ex.png("pecas/moldes/miniatura.html", None, 1280, 720, "vi-06-miniatura-youtube.png", fundo_transparente=False)
        jpg = para_jpg(mini, mini[:-4] + ".jpg")
        if jpg:
            ex.feitos[-1] = os.path.relpath(jpg, AQUI)
        # PDF de Testing (QA-01)
        q = {"azure": a.azure, "youtube": a.youtube, "equipe": ";".join(equipe), "turma": a.turma}
        ex.pdf("pecas/moldes/capa.html", q, "qa-01-capa.pdf")
        ex.png("pecas/moldes/capa.html", q, 794, 1123, "qa-01-capa.png", escala=2, fundo_transparente=False)
    finally:
        ex.fechar()
    return ex.feitos


def cartela(a):
    ex = Exportador()
    try:
        saida = os.path.abspath(a.saida)
        global SAIDA
        SAIDA = os.path.dirname(saida)
        ex.png("pecas/moldes/video.html", {"peca": "bloco", "n": a.n, "total": a.total, "titulo": a.titulo, "tempo": a.tempo},
               1920, 1080, os.path.basename(saida), fundo_transparente=False)
    finally:
        ex.fechar()
    return ex.feitos


def main():
    p = argparse.ArgumentParser(description="Exporta as peças visuais da Calmaria.")
    p.add_argument("--equipe", default="Luiz Gustavo da Silva;Integrante 2;Integrante 3;Integrante 4;Integrante 5",
                   help="nomes separados por ponto e vírgula, na ordem da divisão")
    p.add_argument("--repo", default="github.com/luyz-gusta/gs-citizentech-calmaria")
    p.add_argument("--azure", default="dev.azure.com/(organização)/calmaria")
    p.add_argument("--youtube", default="youtu.be/(link)")
    p.add_argument("--turma", default="3ESPW")
    sub = p.add_subparsers(dest="cmd")
    c = sub.add_parser("cartela", help="exporta uma cartela de capítulo avulsa")
    c.add_argument("--n", required=True)
    c.add_argument("--total", required=True)
    c.add_argument("--titulo", required=True)
    c.add_argument("--tempo", default="")
    c.add_argument("--saida", required=True)
    a = p.parse_args()
    feitos = cartela(a) if a.cmd == "cartela" else tudo(a)
    print(f"{len(feitos)} arquivos:")
    for f in feitos:
        print("  " + f)


if __name__ == "__main__":
    main()
