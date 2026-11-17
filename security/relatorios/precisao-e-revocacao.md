# Precisão e revocação do motor de PII

Amostra: api-dotnet/tests/Calmaria.Privacidade.Tests/AmostraRotulada.json, 60 comentários fictícios (40 com dado pessoal, 20 sem).
Conta por comentário: a regra disparou ou não. VP verdadeiro positivo, FP falso positivo, FN falso negativo.
Gerado em 17/11/2026 12:09 por security/medir-precisao.ps1. Piso do teste: 90% em cada regra.

| Regra | VP | FP | FN | Precisão | Revocação |
|---|---|---|---|---|---|
| cpf | 6 | 0 | 0 | 100% | 100% |
| telefone | 7 | 0 | 0 | 100% | 100% |
| email | 5 | 0 | 0 | 100% | 100% |
| saude | 17 | 0 | 0 | 100% | 100% |
| segredo | 1 | 0 | 0 | 100% | 100% |
| localizacao-precisa | 1 | 0 | 0 | 100% | 100% |
| quase-identificador | 2 | 0 | 0 | 100% | 100% |
| rg | 3 | 0 | 0 | 100% | 100% |
| cartao-sus | 3 | 0 | 0 | 100% | 100% |
| TOTAL | 45 | 0 | 0 | 100% | 100% |

Nenhum erro na amostra.

Resultado do teste: passou
