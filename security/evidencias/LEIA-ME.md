# Evidências (prints)

Os prints da entrega de Cybersecurity. Saem de execuções reais, todos de uma vez, com
`node entrega/gerar-evidencias.mjs` (como funciona: `entrega/LEIA-ME.md`). Nenhum tem dado pessoal real: o CPF do
relato é fictício (`529.982.247-25`, o exemplo com dígito válido usado nos testes).

| Arquivo | Critério | O que mostra |
|---|---|---|
| `painel-risco-cidadao.png` | Dashboard (2,0) | PA-02: o painel `/seguranca` com o Risco Cidadão antes (52, alto) e depois (20, baixo), 2880 x 1800, tema claro |
| `demo-antes-depois.png` | Dashboard e taint | A saída do `demo-antes-depois.ps1`: o mesmo relato antes e depois da correção, com o CPF cru no log só no antes |
| `precisao-e-revocacao.png` | Motor de PII (3,0) | A precisão e a revocação das 9 regras na amostra rotulada de 60 comentários fictícios |
| `testes.png` | Motor de PII | `dotnet test`: os testes do módulo e os da API passando |
| `semgrep-antes-depois.png` | Taint (3,0), camada estática | O Semgrep acusa o controller vulnerável e não acusa o corrigido nem a API |
| `erro-de-compilacao.png` | Taint, camada do tipo | As linhas vulneráveis coladas na API não compilam (CS0029, CS1061, CS1503) |
| `relato-mascarado.png` | Taint, em execução | O relato com CPF e "autista": o pedido, a resposta, a listagem pública e o banco, já mascarados |
| `log-mascarado.png` | Taint, em execução | O log de depuração do corpo da requisição, com o `LoggerProviderComRedacao`: o dado sai mascarado |

A tradução por IA (2,0) aparece no painel, na seção "Alertas traduzidos pela IA", e no vídeo (bloco 5).
