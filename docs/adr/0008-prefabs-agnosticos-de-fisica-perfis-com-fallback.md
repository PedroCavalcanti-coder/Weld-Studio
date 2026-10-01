# ADR-0008: Prefabs agnósticos de física, perfis com fallback

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Física opcional (Magica Cloth 2) não pode deixar "missing script" em prefabs, e simular uma malha antes do remapeamento de ossos causa explosões.

## Decisão

Prefabs de itens não têm componentes de física. `ClothingItemData` lista perfis (`PhysicsProfileData`) por ordem de preferência; o primeiro cujo backend está instalado é aplicado depois do bind ao esqueleto.

## Consequências

- Backends plugáveis sem tocar no conteúdo.
- Cada backend precisa saber configurar a simulação por código.
