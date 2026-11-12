# Peças exportadas da Calmaria

As imagens prontas estão em `png/`. O código no começo de cada arquivo é o da lista de peças
(`design/plano-de-design.md` e `design/design-da-calmaria.html`).

| Arquivo | Peça | Tamanho | Onde usar |
|---|---|---|---|
| `ap-01-icone-app.png` | Ícone do app | 1024 × 1024, sem transparência | `expo.icon` |
| `ap-02-icone-frente.png` | Frente do ícone adaptativo | 1024 × 1024, transparente | `android.adaptiveIcon.foregroundImage`, com `backgroundColor` `#3D5A73` |
| `ap-03-icone-mono.png` | Ícone monocromático | 1024 × 1024, só alfa | `android.adaptiveIcon.monochromeImage` |
| `ap-04-abertura.png`, `ap-04-abertura-escuro.png` | Abertura (splash) | 1024 × 1024, transparente | `expo-splash-screen`, `imageWidth` 160; a escura vai em `dark.image` |
| `ma-02-logotipo.png` | Logotipo | 800 × 200, transparente | README, relatórios, slides |
| `ma-03-logotipo-invertido.png` | Logotipo invertido | 880 × 280 | Sobre fundo escuro |
| `pa-03-favicon-32.png` | Favicon | 32 × 32 | Painel e Scalar (o SVG está em `design/marca/favicon.svg`) |
| `ad-02-diagrama-arquitetura.png` | Diagrama da arquitetura | 1920 × 1080 | README, pitch (bloco 4), relatórios |
| `ad-03-banner-repositorio.png` | Banner do repositório | 1280 × 640 | GitHub, Settings > Social preview |
| `vi-03-abertura.png` | Abertura dos vídeos | 1920 × 1080 | 3 s no começo do pitch e dos vídeos |
| `vi-04-cartela-bloco-1.png` a `-5.png` | Cartelas dos blocos do pitch | 1920 × 1080 | 2 s no começo de cada bloco |
| `vi-05-cartela-final.png` | Cartela final | 1920 × 1080 | 5 s no fim do pitch |
| `vi-02-tarja-*.png` | Tarja de nome, uma por integrante | 1920 × 1080, transparente | Por cima da gravação, na primeira vez que a pessoa aparece |
| `vi-06-miniatura-youtube.jpg` | Miniatura do YouTube | 1280 × 720 | Miniatura personalizada do pitch |
| `qa-01-capa.pdf`, `qa-01-capa.png` | Capa do PDF de Testing | A4 | Primeira página do PDF do Azure Boards |

## Refazer com os nomes e os links certos
A cartela final já tem o link do repositório. As tarjas, a cartela final e a capa ainda estão com os
marcadores "Integrante 2" a "Integrante 5", "dev.azure.com/(organização)/calmaria" e "youtu.be/(link)".
Quando existirem os nomes e os links, rode de novo (precisa do Chrome ou do Edge, do Python 3.9 ou mais novo e
do Pillow):

```bash
python design/pecas/exportar.py --equipe "Luiz Gustavo da Silva;Nome 2;Nome 3;Nome 4;Nome 5" --azure dev.azure.com/org/calmaria --youtube youtu.be/abc123
```

A ordem dos nomes é a da divisão: Luiz (C#, SOA e Cybersecurity), Mobile, IA e ML, Operating Systems e Testing,
Physical Computing e pitch. Cada nome vira uma tarja com a parte da pessoa.

Cartela de capítulo para outro vídeo (o de Operating Systems, por exemplo):

```bash
python design/pecas/exportar.py cartela --n 2 --total 4 --titulo "O provisionamento automático" --tempo "0:40 a 3:10" --saida design/pecas/png/os-cartela-2.png
```

## Como funciona
Os moldes em `moldes/` são páginas HTML de tamanho fixo (1920 × 1080, 1280 × 720, 1280 × 640, A4) que leem
o texto pela URL e usam só as cores dos tokens. O `exportar.py` sobe um servidor local na pasta `design/`,
abre cada molde no Chrome sem janela e grava a imagem. Para mudar um texto fixo, edite o molde e rode de
novo; não edite os PNG.

## Fontes
`fontes/` traz a Atkinson Hyperlegible Next (400, 500, 600 e 700) e a Atkinson Hyperlegible Mono (400 e 600),
do Google Fonts, sob a SIL Open Font License 1.1 (`OFL-next.txt` e `OFL-mono.txt`). Instale as duas no
computador para usar no editor de vídeo, no matplotlib e no Figma.
