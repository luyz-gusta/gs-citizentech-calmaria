# Roteiros dos vídeos de IA e de Mobile

Dois roteiros para uma pessoa só narrar, na primeira pessoa. As falas estão no ritmo de uma leitura calma,
cerca de 130 palavras por minuto. Os números vêm da execução do notebook e do app; se mudarem, troque
aqui antes de gravar.

**Antes de gravar os dois:**
- Kit de vídeo da Calmaria (peça VI-01): 1920 × 1080, 30 qps, cortes secos, sem música alta. Abertura (VI-03) e
  cartela final (VI-05) estão em `design/pecas/png/`.
- Fale devagar. Pode gravar bloco por bloco e juntar depois.
- Legendas (VI-07): prontas em [legendas/ia.srt](legendas/ia.srt) e [legendas/mobile.srt](legendas/mobile.srt),
  geradas destas falas, com até 2 linhas de 42 caracteres. Antes de subir, troque `[seu nome]` pelo seu nome.
  Os tempos seguem os blocos do roteiro; se a sua fala sair mais rápida ou mais lenta, ajuste no editor de
  legendas do YouTube (Studio, Legendas, Editar tempo). Não grave as legendas na imagem.
- Os dois vídeos vão para o YouTube como **não listados**.

---

# Vídeo 1: IA e Machine Learning (até 5 minutos)

**O que deixar aberto:** o notebook `ml/calmaria_ml.ipynb` no VS Code ou no Jupyter, com zoom de 125%, e a
pasta `ml/figuras/`. O app aberto no celular na tela de um lugar da Consolação, para o último bloco.

### Bloco 1: o problema (0:00 a 0:30)

**Na tela:** abertura da Calmaria (VI-03) e depois a seção 1 do notebook.

> Oi, eu sou [seu nome], da equipe da Calmaria. Para muitas pessoas autistas, barulho alto é gatilho de
> sobrecarga sensorial. A Calmaria ajuda essas pessoas a saber, antes de sair, se um lugar vai estar
> barulhento. Neste vídeo eu mostro a parte de Machine Learning: um modelo que prevê quantas reclamações de
> barulho cada distrito de São Paulo vai ter em cada dia, e se esse dia vai ser de barulho baixo, médio ou
> alto.

### Bloco 2: os dados e o filtro (0:30 a 1:30)

**Na tela:** seção 2 do notebook, a tabela com os oito trimestres; depois o gráfico do total mensal
(`ml02_total_mensal.png`).

> Os dados são reais e públicos: o SP156, o canal de reclamações da Prefeitura de São Paulo, no Portal de
> Dados Abertos. Usei oito trimestres, de outubro de 2024 a setembro de 2026, e filtrei o assunto "Poluição
> Sonora - PSIU". São 104 mil reclamações em 96 distritos.
>
> Os arquivos não são iguais entre si. Sete vêm numa codificação e com ponto e vírgula, e o último vem em
> UTF-8 e com vírgula. A data também muda de formato: dia e mês trocam de lugar no último arquivo. Por isso o
> notebook detecta, arquivo por arquivo, a codificação, o separador e o formato da data, e confere que cem
> por cento das datas caem dentro do trimestre certo.
>
> Uma limitação importante: a base só tem a data da reclamação, não a hora. Então o modelo responde "onde e
> em que dia". A hora vem dos sensores, na API.

### Bloco 3: a preparação e a divisão por tempo (1:30 a 2:30)

**Na tela:** seção 4 do notebook (agregação, atributos e as duas linhas "Treino" e "Teste").

> Primeiro eu agreguei por distrito e por dia, e completei com zero os dias sem reclamação, porque dia sem
> reclamação também é informação. Ficaram 70 mil linhas, e 40% delas são zero.
>
> Os atributos são de calendário: dia da semana, fim de semana, mês, feriado e véspera de feriado. Também
> entram o distrito e a média histórica do distrito, calculada só com os dados de treino, para não vazar a
> resposta.
>
> A divisão é por tempo, nunca aleatória. Treino de outubro de 2024 a março de 2026, teste de abril a
> setembro de 2026. Se eu sorteasse os dias, o modelo veria a véspera e o dia seguinte de cada dia do teste, e
> o resultado ficaria bom demais.

### Bloco 4: o treino e os hiperparâmetros (2:30 a 3:30)

**Na tela:** seções 5, 6 e 7 do notebook (linha de base e as tabelas de `cv_results_`).

> Antes de qualquer modelo, eu criei uma linha de base sem Machine Learning: a média histórica do distrito
> naquele dia da semana. Um modelo só vale se for melhor que ela.
>
> Depois treinei dois modelos de árvore, Random Forest e XGBoost. Escolhi árvores porque os atributos são de
> calendário e de lugar, com interações: o sábado pesa mais na Consolação do que num bairro residencial.
>
> A busca de hiperparâmetros usou validação cruzada em série temporal, só no treino. Foram 18 combinações na
> Random Forest e 54 no XGBoost, variando número de árvores, profundidade, taxa de aprendizado e a função de
> perda. A melhor foi o XGBoost raso, com profundidade 3, e perda de Poisson, que é a própria para contagem.

### Bloco 5: os resultados e o overfitting (3:30 a 4:30)

**Na tela:** a tabela de treino e teste da seção 8; o gráfico previsto contra real da Consolação
(`ml03_previsto_real_consolacao.png`); o gráfico de overfitting.

> Aqui está o resultado, e eu vou ser honesto: o ganho sobre a linha de base é pequeno. No teste, o erro
> médio é de 1,33 reclamação por dia, praticamente igual ao da linha de base. Na métrica própria para
> contagem, a deviance de Poisson, o XGBoost é 0,6% melhor. Onde ele mais ajuda é na Consolação e na Bela
> Vista: acerta o nível do dia, baixo, médio ou alto, em 46% dos dias, contra 42% da linha de base.
>
> O motivo está nos dados. A média é de uma reclamação e meia por distrito por dia, com muitos zeros. Quase
> tudo o que dá para prever está em qual distrito e qual dia da semana, e a linha de base já usa isso.
>
> Sobre overfitting: com árvores fundas e folhas pequenas, o erro de treino cai muito e o de validação piora.
> O modelo decora o treino. Árvores rasas e folhas maiores reduziram essa diferença.

### Bloco 6: o que muda para o cidadão (4:30 a 5:00)

**Na tela:** a tabela da semana na seção 10; depois o celular com o app, na previsão de barulho de um lugar
da Consolação.

> Para a Marina, que trabalha na Paulista: na Consolação, sábado e domingo têm previsão de barulho alta, com
> mais que o dobro das reclamações da terça. O melhor dia para resolver coisas na região é terça ou quarta.
> E é isso que aparece no app: a previsão do distrito para os próximos dias. Obrigado.

**Na tela:** cartela final (VI-05).

---

# Vídeo 2: Mobile Development e IoT (até 3 minutos)

**Antes de gravar:**
- Grave a tela do celular (gravador nativo do Android ou do iPhone), com o app no Expo Go.
- Grave com a API rodando no computador, para não aparecer a faixa "Mostrando dados de exemplo". Da raiz do
  repositório, em dois terminais (o PostgreSQL precisa estar instalado; veja `api-dotnet/README.md`):

  ```bash
  dotnet run --project api-dotnet/src/Calmaria.Api --launch-profile http
  powershell -ExecutionPolicy Bypass -File api-dotnet\scripts\simular-leituras.ps1
  ```

  No `mobile/.env.local`, `EXPO_PUBLIC_API_URL=http://<IP do computador>:5080`; depois `npx expo start --clear`
  dentro de `mobile/`. Celular e computador na mesma rede Wi-Fi.
- O simulador faz o papel dos sensores e deixa os níveis fixos: **Estação Paulista intensa** (índice 92),
  **Estação Consolação moderada** (50), **Supermercado da Rua Augusta moderado** (47) e **Estação Trianon-Masp
  calma** (16). Sem ele, o "nível agora" some 15 minutos depois da última leitura.
- Abra o app uma vez com internet antes de gravar (é quando ele guarda os refúgios no celular). Com a API, a
  tela de crise no modo avião mostra "Sem internet. Usando os refúgios salvos no celular."
- Para mostrar o primeiro uso, apague os dados do app (Perfil, "Apagar meus dados") antes de gravar.
- No relato, escreva um comentário com um telefone inventado, por exemplo "Muito cheio, me liga no 11
  90000-0000", para aparecer o aviso do dado removido.

### Bloco 1: para quem é (0:00 a 0:20)

**Na tela:** abertura (VI-03) ou o logotipo.

> Oi, eu sou [seu nome], da equipe da Calmaria. Pessoas autistas não têm como saber, antes de sair, se um
> mercado ou uma estação vai estar barulhento, claro demais ou lotado. Este é o app da Calmaria, feito em
> Expo com React Native.

### Bloco 2: boas-vindas e perfil sensorial (0:20 a 0:50)

**Na tela:** boas-vindas, tocar em "Criar meu perfil", ajustar som, luz e multidão, preencher o contato de
apoio, salvar.

> Na primeira abertura, o app explica o que faz em três frases. O perfil é anônimo: não pedimos nome, e-mail
> nem CPF. A pessoa diz o quanto som, luz e multidão incomodam, de 1 a 5, e cada valor tem uma frase. Também dá
> para cadastrar um contato de apoio. Tudo fica salvo no SQLite do celular e vai para a API.

### Bloco 3: lugares perto e detalhe (0:50 a 1:45)

**Na tela:** a lista de lugares (Paulista intensa, Consolação e mercado moderados, Trianon-Masp calma); trocar
para o mapa; voltar para a lista; abrir a Estação Paulista; rolar até os horários calmos e a previsão de
barulho.

> Aqui estão os lugares perto, com o nível medido pelos sensores agora. Cada nível aparece em cor, em texto e
> com uma letra e uma forma: C no círculo para calmo, M no quadrado para moderado, I no losango para intenso.
> Assim a cor nunca está sozinha. A Trianon-Masp está calma; a Consolação, moderada. Tem o mapa, e tem a
> lista, que é a alternativa para o leitor de tela.
>
> Abrindo a Estação Paulista: está intensa agora. Som, luz e movimento vêm em números e com uma frase
> simples. Embaixo, os horários mais calmos da semana e a previsão de barulho do distrito, que vem do nosso
> modelo de Machine Learning com os dados reais do SP156. No sábado, a previsão é alta.

### Bloco 4: o botão de crise (1:45 a 2:35)

**Na tela:** tocar em "Preciso de calma"; mostrar o refúgio, a distância e a respiração; avançar dois passos.
Fechar. Ligar o modo avião. Tocar em "Preciso de calma" de novo.

> O botão "Preciso de calma" fica sempre no mesmo canto, em todas as telas. A tela de crise é escura e lisa,
> sem som, sem vibração e sem animação. Ela mostra o lugar calmo mais perto, a distância e os minutos a pé, e
> abre o caminho no app de mapas. Tem um roteiro de respiração, que avança no toque, e um botão para ligar
> para o contato de apoio.
>
> Agora eu ligo o modo avião. Toco de novo no botão, e o refúgio aparece mesmo sem internet, porque os
> refúgios ficam guardados no celular e a distância é calculada no próprio aparelho.

### Bloco 5: relato e acessibilidade (2:35 a 3:00)

**Na tela:** desligar o modo avião; aba Relatar; escolher o lugar, a nota e os gatilhos; escrever o
comentário com o telefone; tocar em Enviar; mostrar o aviso. Depois ligar o TalkBack ou o VoiceOver e tocar
num cartão de lugar.

> A pessoa pode relatar como foi a visita. Se ela escrever um dado pessoal, como um telefone, o app avisa
> antes de enviar que esse dado vai ser removido. E com o leitor de tela ligado, cada cartão é lido inteiro:
> o nome, o tipo e o nível agora. Obrigado.

**Na tela:** cartela final (VI-05).
