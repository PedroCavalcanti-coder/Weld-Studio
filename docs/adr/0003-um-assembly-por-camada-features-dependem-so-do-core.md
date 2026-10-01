# ADR-0003: Um assembly por camada; features dependem só do Core

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Sem fronteiras explícitas, o código de UI e o de malha acabam se referenciando, e compilações ficam lentas.

## Decisão

Cada pasta de `Runtime/` tem um asmdef. Assemblies de feature referenciam apenas `WeldStudio.Core`; contratos compartilhados ficam em `Core/Abstractions`. Só `WeldStudio.App` conhece as implementações concretas.

## Consequências

- O compilador impede dependências proibidas.
- Compilação incremental mais rápida.
- Interfaces novas exigem um passo a mais (entrar no Core).
