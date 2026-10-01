# ADR-0019: Prévia de animações com PlayableGraph próprio

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O usuário quer testar o personagem com animações prontas, pausar, buscar e mudar a velocidade, sem criar um Animator Controller por clip.

## Decisão

`IAnimationPreview` implementado com um `PlayableGraph` (mixer + `AnimationClipPlayable`) sobre o Animator do corpo. Clips Humanoid catalogados como `AnimationClipData`, por categoria, incluindo Amplitude de Movimento para checar deformações. Animações do Mixamo não são redistribuídas.

## Consequências

- Qualquer clip Humanoid compatível funciona.
- A integração com o Animation Rigging no mesmo grafo precisa de validação (Fase 6).
