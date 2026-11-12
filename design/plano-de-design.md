# Plano de design: Calmaria

> Documento-fonte do design. O código e as peças derivam daqui: se divergirem, corrija este plano primeiro.
> A versão visual, com as telas, o visor, o painel e as cartelas desenhados, está em `design-da-calmaria.html`
> (abre no navegador, sem internet só perde as fontes) e no artefato "Design da Calmaria"
> (https://claude.ai/artifact/RyzvGwBWsiutNzRkboHVbm, privado até o Luiz compartilhar).

Versão: 1 · Data: 2026-11-12 · Estado: fundações aprovadas pelos scripts; telas em especificação.
Os portões de aprovação da skill de design foram pulados a pedido do Luiz ("quero já"); o que seria mostrado
em cada um está nas seções 3, 4.1 e 6.

## 0. Ficha do projeto

| Campo | Conteúdo |
|---|---|
| Produto e objetivo | Calmaria, mapa sensorial de São Paulo (Bela Vista e Consolação) para a Global Solution 2026.2 da FIAP. Objetivo: a pessoa autista sabe, antes de sair, se um lugar está barulhento, claro demais ou lotado, e chega a um lugar calmo na crise. |
| Usuários e dispositivo | Pessoas autistas com hipersensibilidade sensorial e as famílias, no celular (app Expo). A equipe e os professores, no painel web e nos vídeos. |
| Contexto físico | Na rua, em estação e mercado: barulho, luz forte, movimento, às vezes em sobrecarga, com uma mão. O visor do sensor é lido de pé, a cerca de um metro. |
| Marca existente | Nenhuma. O nome Calmaria é provisório. |
| Dono da marca | A equipe da GS (produto próprio). |
| Requisitos | WCAG 2.2 AA; cor nunca sozinha; sem som, vibração forte nem animação; tema claro e escuro; pt-BR; alvos de 48 pt; texto mínimo de 16 no app. |
| Restrições técnicas | Expo com React Native (app), HTML com Chart.js sem build (painel), Scalar (documentação), SSD1306 1 bit com a fonte do Adafruit GFX (visor), matplotlib (ML), Clipchamp ou CapCut (vídeos). |
| Telas pedidas | As 7 do app (boas-vindas, perfil, lugares perto, detalhe, crise, relatar, meus lugares), o painel Risco Cidadão, as 3 telas do visor e as peças de vídeo, PDF e repositório. |
| Fora de escopo | Site de divulgação, redes sociais, impressos além do PDF de Testing. |

## 1. Lacunas e suposições

| Lacuna | Suposição adotada | Impacto se estiver errada | Situação |
|---|---|---|---|
| Nome do produto | Calmaria | Troca o logotipo e os textos; o sistema fica | Assumida |
| Rótulo do botão de crise | "Preciso de calma" | Troca um texto em todas as telas | Assumida; validar na entrevista |
| Preferência de cor do público | Baixa saturação, sem branco puro, com escuro | Baixo: os tokens trocam de passo sem refazer telas | Assumida pela literatura; validar na entrevista |
| Fonte no editor de vídeo | O Clipchamp aceita fonte enviada | As cartelas viram PNG | Assumida |
| Rubricas de Mobile e Physical | Seguem o briefing | Muda a ênfase, não o design | Perguntar em 13/11 |

## 2. Pesquisa de referências

Não houve pesquisa web nesta versão: os padrões vêm das referências da skill de design (NN/g, GOV.UK, WCAG
2.2) e do que a entrevista do fim de semana confirmar. Fica como pendência registrar 2 a 4 produtos reais
(mapas de acessibilidade e apps de horário calmo de lojas) com o que adotamos e o que evitamos.

Convenções do domínio que o usuário já espera:
- Lista como alternativa ao mapa, porque marcador de mapa é ruim no leitor de tela.
- Semáforo de três níveis, mas com letra e forma, não só cor.
- Botão de emergência sempre no mesmo lugar.

## 3. Direção visual

| Atributo | Decisão visual | Onde aparece |
|---|---|---|
| Calmo | Ardósia apagada (#3D5A73), neutros tingidos, saturação baixa nos níveis | Tudo |
| Previsível | Mesma navegação, botão de crise fixo, nenhuma transição | App |
| Literal | Todo ícone com texto, números com unidade e frase | App, painel, visor |
| Acessível | Nível em cor, letra e forma; Atkinson Hyperlegible; corpo 18 | App, painel, gráficos |
| Acolhedor na crise | Tela escura e lisa nos dois temas, uma ação por vez | Crise, abertura dos vídeos |

Princípios:
1. Previsível antes de bonito: entre um layout melhor para uma tela e o padrão das outras, vence o padrão.
2. Literal: ícone sozinho não entra.
3. Nível em três sinais (cor, letra C, M, I e forma círculo, quadrado, losango): se não cabem, muda a peça.
4. Quieto: entre destacar e acalmar, acalma.
5. Escuro na crise: na crise, menos opções vencem mais informação.

Direções consideradas: A, ardósia e neutros frios ("mar sem vento"); B, verde-água como marca; C, neutro
quente com terracota. **Escolhida: A.** B confunde a marca com o nível Calmo; C é o visual genérico de IA e
o terracota se confunde com o Intenso.

Fora do sistema: peça de quebra-cabeça, cérebro, arco-íris, vermelho vivo, gradiente, foto de banco.

## 4. Fundações

### 4.1 Cor

- Comando: `node scripts/paleta.mjs --cor "#3D5A73" --neutro --semanticas --extra calmo=#3F7A5E --extra moderado=#9A7420 --extra intenso=#A34A40 --json design/tokens/primitivos.tokens.json`
- A marca caiu no passo 700. Contraste com branco 7,22:1; com preto 2,91:1.
- Sólido primário: a cor exata da marca (`brand.exact`, `exact-hover`, `exact-pressed`), com texto branco,
  porque a identidade pede a ardósia exata e ela passa com folga.
- O azul de informação colidia com a marca (10 graus): `info.*` aponta para a escala `cyan`.
- Os papéis `success`, `warning` e `danger` apontam para as escalas apagadas dos níveis; foram criados os papéis
  `nivel.{calmo,moderado,intenso}.*`, `crise.*` e `botao-crise.*`.

| Token | Claro | Escuro | Motivo da mudança |
|---|---|---|---|
| `bg.canvas` | neutral.100 #F1F5F7 | neutral.900 #14191F | Sem branco puro no claro; escuro menos duro que o 950 |
| `bg.surface`, `bg.raised` | neutral.50 #F8FBFD | neutral.800 #22282E | Idem |
| `bg.subtle` | neutral.200 #E0E5E9 | neutral.950 #06090E | Rebaixado continua mais escuro que a superfície |
| `text.subtle` | neutral.600 #4D545B | neutral.400 #9BA3AA | O 500 dava 4,39:1 sobre o canvas novo |
| `primary.solid` | brand.exact #3D5A73 | brand.400 #8AA8BE | Cor exata da marca |
| `success.*` / `warning.*` / `danger.*` | calmo / moderado / intenso | idem | Saturação baixa |
| `info.*` | cyan | cyan | Colisão com a marca |
| `nivel.moderado.solid` | moderado.400 #BBA15E com texto #06090E | o mesmo | Ocre escuro vira marrom; o claro lê como amarelo |
| `crise.bg`, `crise.surface` | brand.950 #1F2733, brand.900 #313D4D | os mesmos | A crise é escura nos dois temas |
| `botao-crise.bg` | brand.900 #313D4D | brand.200 #D4E1EA | Separa do fundo nos dois temas |

Resultado do `contraste.mjs --resumo`: 86 pares por tema, 0 reprovados. Menor razão de texto: 4,80:1 no
claro (`info.on-solid` sobre `info.solid`) e 5,82:1 no escuro (`text.subtle` sobre `bg.surface`).

### 4.2 Tipografia

- Família: Atkinson Hyperlegible Next (texto) e Atkinson Hyperlegible Mono (números de medição, códigos,
  terminal). Por quê: desenhada pelo Braille Institute para baixa visão, letras que não se confundem (I, l, 1;
  O, 0), pesos de 200 a 800, português completo, licença OFL, disponível no Google Fonts e no Figma.
- Medição: altura-x 0,496 (abaixo de 0,50: 17,6 px dela equivalem a 16 px da Inter); a Next tem `tnum`; a
  Mono já é tabular. Lexend foi medida e descartada: sem `tnum`.
- Comando: `node scripts/escala-tipografica.mjs --base 18 --razao 1.2 --fonte "Atkinson Hyperlegible Next" --json design/tokens/tipografia.tokens.json`

| Papel | Tamanho | Altura de linha | Peso | Espaçamento | Uso |
|---|---|---|---|---|---|
| caption | 14 | 20 | 400 | 0 | Só painel, gráficos e PDF |
| label | 16 | 24 | 500 | 0 | Rótulos, abas, chips, apoio |
| body-md | 18 | 28 | 400 | 0 | Corpo do app |
| label-lg | 18 | 28 | 600 | 0 | Nome do lugar, botões |
| title-sm | 22 | 32 | 600 | 0 | Título de bloco no painel |
| title-md | 24 | 32 | 600 a 700 | -0,01em | Título da tela |
| title-lg | 32 | 40 | 700 | -0,01em | Nível no detalhe, título da crise |
| heading-lg | 44 | 52 | 700 | -0,02em | Números do Risco Cidadão |
| display | 52 | 60 | 700 | -0,02em | Banner do repositório |

Vídeo usa medida própria no quadro de 1920: título 96, logotipo 120, frase 48, mínimo 40.

### 4.3 Espaçamento, raio, borda, elevação e movimento

- Espaçamento: base 4 (4, 8, 12, 16, 20, 24, 32, 40, 48, 64). Margem do app 16, entre cartões 12, dentro do
  cartão 16, entre blocos 24.
- Raio: 12 em cartão, campo e botão; 16 nos cartões de nível e da crise; pílula em chip, selo e botão de
  crise. Arredondado sem ser infantil.
- Elevação: só o botão de crise flutuante (`shadow.md`). O resto é plano, com borda.
- Movimento: nenhum. `duration.*` = 0; Stack com `animation: 'none'`; carregando com texto fixo.
- Ícones: Lucide (ISC), traço 2, 24 px (20 em linha de texto, 32 na crise), sempre com texto.
- Alvos: `size.touch-min` 48, `control-md` 48, `control-lg` 56, `crisis-button` 64.

### 4.4 Grid e breakpoints

| Faixa | Largura | Colunas | Margem | Calha | Contêiner | Frame de desenho |
|---|---|---|---|---|---|---|
| App | 360 a 430 | 1 | 16 | 12 | largura toda | 390 × 844 (conferir em 360 × 800) |
| Painel, estreito | até 767 | 1 | 16 | 16 | largura toda | 375 |
| Painel, médio | 768 a 1279 | 2 | 24 | 16 | largura toda | 768 × 1024 |
| Painel, largo | 1280 em diante | 12 | 32 | 24 | 1376 | 1440 × 900 |
| Visor | 128 × 64 px | não se aplica | 0 | não se aplica | não se aplica | grade de pixels, fonte 6 × 8 |
| Vídeo | 1920 × 1080 | não se aplica | 96 e 54 (margem segura) | não se aplica | não se aplica | 1920 × 1080 |

## 5. Componentes

| Componente | Telas que usam | Variantes | Tamanhos | Estados | Propriedades no Figma |
|---|---|---|---|---|---|
| Ícone | Todas | Lucide | 20, 24, 32 | padrão | instance swap |
| Botão | Todas | primário, secundário, texto, crise | 56; 64 na crise | padrão, pressionado, foco, desabilitado | variante, ícone (boolean e swap), rótulo |
| Selo de nível | Lista, detalhe, painel | calmo, moderado, intenso | 32; 28 em lista densa | padrão | variante nível, texto |
| Marcador do mapa | Mapa | calmo, moderado, intenso | 40 visível, alvo 48 | padrão, selecionado | variante nível, selecionado |
| Cartão de lugar | Lista, meus lugares | com e sem distância | largura toda | padrão, pressionado, foco | slot, boolean distância |
| Botão de crise flutuante | Todas menos crise | um | 64 × 176 mín. | padrão, pressionado, foco | rótulo |
| Alternância Mapa e Lista | Lugares perto | dois itens | 48 | ativo, inativo, foco | variante |
| Chip | Filtros, relatar | simples, selecionado | 40 de altura visível, alvo 48 | padrão, selecionado, foco | variante, texto |
| Seletor de 1 a 5 | Perfil, relatar | com rótulo por valor | 48 | padrão, selecionado, foco | texto por valor |
| Campo e área de texto | Perfil, relatar | texto, seleção, multilinha | 56; 100 de altura mínima | padrão, foco, erro, desabilitado | rótulo, ajuda, erro |
| Aviso | Relatar, faixas | privacidade, dados de exemplo, sem internet | largura toda | padrão | variante, texto |
| Linha de medida | Detalhe | som, luz, movimento | largura toda | padrão | ícone, valor, frase |
| Barra de abas | Todas menos crise | três abas | 64 mais área segura | ativa, inativa, foco | variante aba ativa |

Ordem de construção: ícone, botão, chip, selo, marcador, campo, aviso, cartão de lugar, linha de medida,
alternância, seletor, barra de abas, botão de crise.

## 6. Telas

Todas no tema claro e no escuro; a crise é escura nos dois. Casos de borda comuns a todas: nome de lugar com
60 caracteres quebra em duas linhas sem cortar; fonte do sistema em 200% faz o cartão crescer para baixo.

### 6.1 Boas-vindas (AP-05)

| Campo | Conteúdo |
|---|---|
| Objetivo | Entender o que o app faz e decidir criar o perfil ou pular |
| Usuário e dispositivo | Pessoa autista ou familiar, celular, primeira abertura |
| Ação primária | Criar meu perfil |
| Hierarquia | 1º as três frases; 2º Criar meu perfil; 3º Pular |
| Layout | Uma coluna, texto no alto, botões no rodapé |
| Seções | Logotipo, três frases ("Mostra se um lugar está barulhento, claro ou cheio agora." "Mostra os horários mais calmos." "Tem um botão que leva ao lugar calmo mais perto."), botões |
| Conteúdo real | As três frases acima |
| Vazio | Não se aplica: tela sem dado |
| Carregando | "Preparando o app." enquanto cria o usuário anônimo |
| Erro | "Não consegui falar com o servidor. Você pode usar o app mesmo assim." e segue |
| Sucesso | Vai para o perfil (ou para Lugares perto, se pulou) |
| Outros | Offline: segue sem criar usuário e tenta de novo depois |
| Casos de borda | Fonte em 200%: as frases rolam, os botões ficam visíveis |
| Acessibilidade | Título lido primeiro; botões com `accessibilityRole="button"` |
| Responsivo | Igual de 360 a 430 |

### 6.2 Perfil sensorial (AP-06)

| Campo | Conteúdo |
|---|---|
| Objetivo | Dizer quanto som, luz e multidão incomodam, e o contato de apoio |
| Usuário e dispositivo | Pessoa ou familiar, celular |
| Ação primária | Salvar perfil |
| Hierarquia | 1º os três seletores; 2º contato; 3º Salvar |
| Layout | Uma coluna com rolagem |
| Seções | Três seletores de 1 a 5 com a frase de cada valor ("1: quase não me incomoda" a "5: não consigo ficar"); nome e telefone do contato; Salvar |
| Conteúdo real | Som 5, luz 3, multidão 4; contato "Ana", (11) 91234-5678 (fictício) |
| Vazio | Seletores sem valor, Salvar desabilitado com a ajuda "Escolha os três valores" |
| Carregando | "Salvando." no botão |
| Erro | "Não consegui salvar no servidor. Ficou salvo no celular." |
| Sucesso | "Perfil salvo." e volta |
| Outros | Offline: salva no SQLite e sincroniza depois |
| Casos de borda | Nome do contato com 60 caracteres quebra |
| Acessibilidade | Cada valor lido com a frase; erro ligado ao campo |
| Responsivo | Igual |

### 6.3 Lugares perto (AP-07)

| Campo | Conteúdo |
|---|---|
| Objetivo | Ver como estão os lugares perto e escolher um |
| Usuário e dispositivo | Pessoa, celular, na rua |
| Ação primária | Abrir um lugar |
| Hierarquia | 1º o selo do nível de cada lugar; 2º o nome; 3º a distância |
| Layout | Título, alternância Mapa e Lista, filtros, lista de cartões |
| Seções | Título "Lugares perto de você"; alternância; chips Todos, Estações, Mercados, Refúgios; cartões |
| Conteúdo real | Supermercado da Rua Augusta, Moderado, 53, 150 m; Estação Consolação, Intenso, 78, 350 m; Estação Paulista, Moderado, 61, 450 m; Parque Trianon, Calmo, 18, 700 m |
| Vazio | "Nenhum lugar com medição perto daqui." e o botão "Ver todos os lugares" |
| Carregando | "Buscando os lugares perto de você." (sem spinner) |
| Erro | "Não consegui falar com o servidor." e "Tentar de novo" |
| Sucesso | A lista |
| Outros | Dados de exemplo: faixa "Mostrando dados de exemplo"; sem localização: lista de todos, por nome |
| Casos de borda | 7 lugares (o seed inteiro) rolam; um lugar sem leitura mostra "Sem medição" em neutro |
| Acessibilidade | Cartão lido como "Supermercado da Rua Augusta, Moderado agora, índice 53, 150 metros" |
| Responsivo | Igual |

### 6.4 Detalhe do lugar (AP-08)

| Campo | Conteúdo |
|---|---|
| Objetivo | Decidir se vai agora ou em outro horário |
| Usuário e dispositivo | Pessoa ou familiar, celular |
| Ação primária | Como chegar |
| Hierarquia | 1º o nível agora; 2º os horários calmos; 3º a previsão do distrito |
| Layout | Uma coluna com rolagem |
| Seções | Voltar, nome, cartão do nível, medidas com frase, horários calmos, previsão do distrito, resumo dos relatos, Como chegar e Favoritar |
| Conteúdo real | Estação Consolação (Linha 2), Intenso, 78; 88 dB, 2.300 lux, 23 por minuto; domingo 8h às 9h e terça 10h às 11h; sábado, 14/11, muitas reclamações de barulho |
| Vazio | "Ainda não temos medições deste lugar." e "Relatar uma visita" |
| Carregando | "Buscando as medições." |
| Erro | "Não consegui falar com o servidor." e "Tentar de novo" |
| Sucesso | A tela |
| Outros | Previsão indisponível: "Sem previsão para este distrito." |
| Casos de borda | Nenhum horário calmo: "Este lugar não teve horário calmo na última semana." |
| Acessibilidade | Nível lido primeiro; medidas lidas com a frase |
| Responsivo | Igual |

### 6.5 Crise (AP-09)

| Campo | Conteúdo |
|---|---|
| Objetivo | Chegar ao refúgio calmo mais perto |
| Usuário e dispositivo | Pessoa em sobrecarga, celular, possivelmente sem internet |
| Ação primária | Abrir o caminho no mapa |
| Hierarquia | 1º a frase de acolhimento; 2º o refúgio e a distância; 3º Abrir o caminho |
| Layout | Uma coluna, fundo #1F2733, botões de 64 |
| Seções | Fechar; título; cartão do refúgio; Abrir o caminho no mapa; respiração passo a passo; Ligar para o contato |
| Conteúdo real | "Parque Trianon, a 700 metros, cerca de 9 minutos a pé." |
| Vazio | Sem refúgio salvo e sem internet: "Não tenho refúgios salvos. Procure um lugar com menos gente." e Ligar |
| Carregando | O último refúgio salvo aparece na hora; a distância atualiza quando a localização chega |
| Erro | Sem localização: "Não consegui achar sua localização. Estes são os refúgios da região:" e a lista |
| Sucesso | "Você chegou ao Parque Trianon." quando a distância fica abaixo de 30 m |
| Outros | Sem internet: faixa "Sem internet. Usando os refúgios salvos no celular." |
| Casos de borda | Sem contato de apoio: o botão vira "Cadastrar um contato de apoio" |
| Acessibilidade | Foco começa no título; um passo de respiração por toque, nunca automático |
| Responsivo | Igual |

### 6.6 Relatar (AP-10)

| Campo | Conteúdo |
|---|---|
| Objetivo | Contar como foi a visita a um lugar |
| Usuário e dispositivo | Pessoa ou familiar, celular, depois da visita |
| Ação primária | Enviar relato |
| Hierarquia | 1º o lugar; 2º a nota; 3º Enviar |
| Layout | Formulário de uma coluna |
| Seções | Lugar; Como foi (1 Insuportável, 2 Difícil, 3 Deu para ficar, 4 Tranquilo, 5 Muito calmo); O que incomodou (Barulho, Luz forte, Multidão, Cheiro, Fila, Outro); comentário; aviso de privacidade; Enviar |
| Conteúdo real | Supermercado da Rua Augusta, 2 Difícil, Barulho, Multidão e Fila, comentário com um telefone fictício |
| Vazio | Formulário sem nada; Enviar desabilitado com "Escolha o lugar e a nota" |
| Carregando | "Enviando." no botão |
| Erro | "Não consegui enviar. O relato ficou guardado e vai quando a internet voltar." |
| Sucesso | "Relato enviado. Obrigado por ajudar quem vai depois." no lugar do formulário |
| Outros | Aviso de privacidade quando o `analisar` acha dado pessoal |
| Casos de borda | Comentário de 500 caracteres: contador "500 de 500" |
| Acessibilidade | Aviso de privacidade anunciado ao aparecer (`accessibilityLiveRegion="polite"`) |
| Responsivo | Igual |

### 6.7 Meus lugares (AP-11)

| Campo | Conteúdo |
|---|---|
| Objetivo | Voltar aos lugares de sempre e ver as crises passadas |
| Usuário e dispositivo | Pessoa ou familiar, celular |
| Ação primária | Abrir um favorito |
| Hierarquia | 1º favoritos com o selo; 2º histórico |
| Layout | Duas seções em uma coluna |
| Seções | Favoritos (cartões de lugar); histórico das crises com data, hora e refúgio, e a frase "Este histórico fica só no seu celular." |
| Conteúdo real | Parque Trianon e Casa das Rosas; "10/11, 18h20, Parque Trianon, chegou" |
| Vazio | "Você ainda não favoritou nenhum lugar." e "Nenhuma crise registrada." |
| Carregando | Leitura do SQLite, sem estado visível |
| Erro | Não se aplica: dado local |
| Sucesso | A lista |
| Outros | Nenhum |
| Casos de borda | 50 crises: mostra as 10 últimas e "Ver todas" |
| Acessibilidade | Seções com título; histórico lido como frase |
| Responsivo | Igual |

### 6.8 Painel Risco Cidadão (PA-01)

| Campo | Conteúdo |
|---|---|
| Objetivo | Mostrar ao professor o risco do dado do cidadão antes e depois da correção |
| Usuário e dispositivo | Equipe e professor, notebook |
| Ação primária | Rodar varredura |
| Hierarquia | 1º antes e depois do Risco Cidadão; 2º achados por categoria e destino; 3º fluxos bloqueados e alertas |
| Layout | Cabeçalho; faixa do indicador; dois cartões lado a lado; dois blocos de largura toda |
| Seções | Ver a versão visual |
| Conteúdo real | 64 (Alto) antes, 9 (Baixo) depois; 19 achados; 3 fluxos bloqueados |
| Vazio | "Nenhuma varredura ainda." e Rodar varredura |
| Carregando | "Varredura em andamento." no botão, que fica desabilitado |
| Erro | 401: "Chave de administrador ausente ou errada." |
| Sucesso | Indicador atualizado e a hora da varredura |
| Outros | Sem chave do LLM: alertas com "frase pronta" |
| Casos de borda | 0 achados: Risco Cidadão 0, Baixo |
| Acessibilidade | Cada gráfico com a tabela equivalente; números em texto |
| Responsivo | 1, 2 ou 12 colunas (seção 4.4) |

### 6.9 Visor do sensor (SE-01 a SE-03)

| Campo | Conteúdo |
|---|---|
| Objetivo | Mostrar o nível do lugar a quem está perto |
| Usuário e dispositivo | Quem passa e a equipe na demo; OLED 128 × 64 |
| Ação primária | Não se aplica: só leitura |
| Hierarquia | 1º o índice; 2º o nível; 3º as medidas |
| Layout | Quatro linhas fixas (y 0, 11, 38, 56) |
| Seções | Lugar, índice e "/100", nível, medidas |
| Conteúdo real | EST. CONSOLACAO, 78, INTENSO, 88dB 2300lx 23/min |
| Vazio | "SEM DADO" na linha do nível e "AGUARDANDO LEITURA" na última |
| Carregando | Tela de inicialização (SE-02) |
| Erro | Sem MQTT (SE-03): continua medindo |
| Sucesso | A tela medindo |
| Outros | Sem Wi-Fi: "SEM WI-FI: SO VISOR" na última linha |
| Casos de borda | ISS 100 (três dígitos) cabe; nome com mais de 21 letras é abreviado no código |
| Acessibilidade | LED de nível junto, para quem não lê o visor |
| Responsivo | Não se aplica |

## 7. Plano do Figma

Não montado: o conector do Figma não estava autorizado na sessão de 12/11. As variáveis estão prontas para
o import nativo em `figma-import/` (coleções Primitivos, Tema com Claro e Escuro, Dimensões, Tipografia e
Movimento). As telas de verificação estão desenhadas em HTML no artefato e em `design-da-calmaria.html`.

## 8. Verificação

| Checagem | Como foi feita | Resultado |
|---|---|---|
| Contraste de todos os pares, nos dois temas | `contraste.mjs` | 86 de 86 em cada tema |
| Tokens válidos | `tokens.mjs validar` | Nenhum erro |
| Hierarquia | Captura da página | Conferida no desktop |
| Nada cortado nem vazando | Página em 375 e 1280 px | Sem rolagem lateral |
| Todos os estados de cada tela | Seção 6 | Especificados; desenhados só os padrões |
| Alvos de toque | Tokens | 48 no mínimo, 64 na crise |
| Foco visível | Tokens | `border.focus` de 2 px afastado 2 px |
| Valor fixo onde existe token | Não se aplica | Sem Figma |
| Camadas e instâncias | Não se aplica | Sem Figma |

## 9. Decisões registradas

| Data | Decisão | Alternativas | Motivo |
|---|---|---|---|
| 2026-11-12 | Marca em ardósia #3D5A73 | Verde-água; neutro quente | Não ser lida como nível; calma sem cair no visual genérico |
| 2026-11-12 | Nível em cor, letra e forma (círculo, quadrado, losango) | Só cor; triângulo no Intenso | Daltonismo e leitor de tela; o triângulo lê como perigo |
| 2026-11-12 | Atkinson Hyperlegible Next e Mono, corpo 18 | Inter; Lexend | Legibilidade para baixa visão; Lexend sem `tnum` |
| 2026-11-12 | Nenhuma animação | Transições curtas | Pedido do público; previsibilidade |
| 2026-11-12 | Tela de crise escura nos dois temas | Seguir o tema | Menos estímulo na sobrecarga |
| 2026-11-12 | Risco Cidadão em faixa linear, não mostrador | Gauge redondo | Mostra a distância entre antes e depois |

## 10. Pendências

- Validar na entrevista o rótulo "Preciso de calma" e a preferência de cor (equipe, até 15/11).
- PNG exportados em 12/11 em `pecas/png/` (25 arquivos). A cartela final, as tarjas e a capa têm os nomes e
  os links como marcador: refazer com `pecas/exportar.py` quando existirem
  (cartelas e tarjas: Integrante 5, do pitch; capa: Integrante 4, de Testing).
- Pesquisa de 2 a 4 produtos reais para a seção 2 (opcional; não muda as entregas).
- Montar no Figma se alguém da equipe quiser (o import está pronto).
