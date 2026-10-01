# ADR-0006: Conteúdo só via Addressables; pasta Resources proibida

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Um criador de personagens tem centenas de malhas e texturas; carregar tudo ou depender de `Resources/` estoura a memória e impede mods.

## Decisão

Todo conteúdo é Addressable. ScriptableObjects de catálogo referenciam conteúdo pesado só por `AssetReference`. Todo load passa pelo `IAssetProvider`, com contagem de referências e liberação determinística.

## Consequências

- Memória proporcional ao que está em uso.
- Mods podem trazer catálogos próprios.
- O CI (`Tools/ci/check_repo.py`) rejeita pastas `Resources/`.
