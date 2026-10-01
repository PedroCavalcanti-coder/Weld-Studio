# ADR-0010: Esqueleto Game engine do MakeHuman como base

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Corpo, roupas e cabelos compartilham o mesmo esqueleto, que precisa ser compatível com o Humanoid da Unity.

## Decisão

Usar o esqueleto *Game engine* do MakeHuman. Expressões faciais são blendshapes, então os ossos faciais do esqueleto *default* não são necessários.

## Consequências

- Em aberto (Q2): ossos de jiggle adicionais. Se forem adotados, o rig oficial ganha um ID próprio e esta ADR é substituída.
