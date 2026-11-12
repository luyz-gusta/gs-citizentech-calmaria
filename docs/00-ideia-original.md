# Ideia original da equipe

Texto como a equipe mandou em 12/11/2026, antes do escopo. É o ponto de partida de tudo em
[01-escopo-global.md](01-escopo-global.md).

> **5. Mapa sensorial para pessoas autistas**
>
> Barulho, luz forte e multidão podem provocar crises, e nenhum app de mapa mostra isso.
>
> **ESP32:** mede a "sobrecarga sensorial" de um lugar: som (um potenciômetro simula o microfone), luz
> (LDR) e movimento de pessoas (sensor de presença).
>
> **App:** mostra rotas e horários mais calmos em mercados e estações, e tem um "botão de crise" que aponta
> o lugar silencioso mais próximo.
>
> **ML:** reclamações de barulho no SP156 para prever onde e quando a cidade fica mais barulhenta.

## O que o escopo mudou em relação à ideia
- **"Rotas mais calmas" virou "horários e lugares mais calmos" + "como chegar".** Calcular rota a pé
  ponderada por barulho é um projeto inteiro de roteamento; não cabe em 7 dias. O app indica o horário
  mais calmo, o lugar alternativo mais calmo e abre o trajeto no app de mapas do celular.
- **O ML prevê "onde e em que dia", não "em que hora".** A base do SP156 traz só a data de abertura da
  reclamação, sem a hora. A hora do dia vem das leituras do ESP32. Ver
  [ia-machine-learning.md](disciplinas/ia-machine-learning.md).
