# ADR-0017: Edição do corpo por ossos e blendshapes, combinados por fonte

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O usuário quer mover, esticar, engrossar e diminuir partes do corpo. Só blendshapes não cobrem proporções (comprimento de membros); só ossos não cobrem formas (nariz, mandíbula).

## Decisão

Modificadores em dados: `BlendShapeModifierDefinition` para formas e `BoneTransformModifierDefinition` (Length, Thickness, UniformScale, Offset) para proporções. `BodyPartData` agrupa os modificadores de cada parte selecionável, com parte espelhada para simetria. Contribuições no mesmo osso são combinadas por fonte (`BoneAdjustmentStack`); o rig compensa a escala nos ossos filhos.

## Consequências

- Roupas e cabelos acompanham pelos ossos e blendshapes compartilhados.
- A compensação de escala precisa ser validada no esqueleto real (Fase 3).
- IDs de modificadores são escritos à mão e fazem parte do formato do preset.
