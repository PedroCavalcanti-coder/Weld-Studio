# ADR-0001: Raiz do repositório é a raiz do projeto Unity

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O repositório precisa abrir na Unity logo após o clone, e o `.gitignore` oficial de Unity (já presente) assume `Library/`, `Temp/` etc. na raiz.

## Decisão

O projeto Unity (`Assets/`, `Packages/`, `ProjectSettings/`) fica na raiz do repositório. Fontes de arte ficam em `SourceAssets/` e ferramentas externas em `Tools/`, ambas fora de `Assets/`.

## Consequências

- Clonar e abrir, sem configuração extra.
- Tudo que não é projeto Unity precisa morar fora de `Assets/` para não ser importado.
