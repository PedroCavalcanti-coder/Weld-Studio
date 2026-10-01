# ADR-0004: Task nas APIs assíncronas; Awaitable para frames e threads

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O plano inicial previa só `Awaitable`. Mas instâncias de `Awaitable` são reaproveitadas pela Unity e só podem ser aguardadas uma vez, o que dificulta compor operações (por exemplo, carregar vários itens em paralelo).

## Decisão

Interfaces públicas (`IAssetProvider`, `ICatalogService`, `IPresetRepository`) retornam `Task`. O `Awaitable` da Unity 6 é usado internamente para esperar frames e trocar de thread. Trabalho de CPU sem Unity usa `Task.Run`. Não usamos UniTask.

## Consequências

- `Task.WhenAll` e testes em .NET puro funcionam sem adaptação.
- Continuações voltam à main thread pelo `UnitySynchronizationContext`.
- Nenhuma dependência extra.
