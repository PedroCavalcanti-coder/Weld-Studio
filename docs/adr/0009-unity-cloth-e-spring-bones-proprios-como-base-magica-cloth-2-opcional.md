# ADR-0009: Unity Cloth e spring bones próprios como base; Magica Cloth 2 opcional

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O app open-source precisa funcionar só com código aberto. A Unity não tem spring bones nativos, e o Unity Cloth não serve para cabelo.

## Decisão

Backends `UnityClothBackend` (roupas) e `SpringBoneBackend` (cabelo, jiggle), próprios do projeto. O Magica Cloth 2 vive em `Assets/Modules/MagicaClothBridge`, compilado só com o define `WELD_MAGICACLOTH2`.

## Consequências

- Qualidade melhor com o Magica, mas nunca dependência.
- O CI compila sem o Magica.
