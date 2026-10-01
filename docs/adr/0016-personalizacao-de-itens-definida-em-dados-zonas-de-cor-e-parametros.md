# ADR-0016: Personalização de itens definida em dados (zonas de cor e parâmetros)

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Cada roupa e cabelo tem partes recolorizáveis e ajustes diferentes. Escrever código por item não escala com conteúdo da comunidade.

## Decisão

`EquipableItemData` (base de roupas e cabelos) declara `ColorZone`s (ID, cor padrão, propriedade de shader, slot de material) e `ItemParameter`s (faixa, padrão, propriedade de shader ou blendshape). O `CharacterModel` guarda as escolhas por item (`ItemAppearance`, cores em `ColorRgba`). Na cena, tudo é aplicado com `MaterialPropertyBlock`, sem duplicar materiais.

## Consequências

- Nova peça com novas opções = só dados.
- Shaders do projeto seguem convenções fixas (`_ZoneColor0..3`, `_RootColor`, `_TipColor`, `_StreakColor`...).
- IDs desconhecidos são preservados nos presets, como nos modificadores.
