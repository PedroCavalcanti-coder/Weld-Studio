# ADR-0002: MVP na apresentação e injeção de dependência com VContainer

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

A lógica de troca de malhas não pode depender da UI, e vice-versa. A ferramenta precisa de undo/redo, testes sem cena e, no futuro, vários personagens simultâneos.

## Decisão

Views passivas (UXML/USS + C# fino) e Presenters em C# puro. Toda mudança passa por comandos sobre o `CharacterModel`. As dependências são injetadas pelo VContainer, com escopos raiz, por personagem e de UI.

## Consequências

- Presenters testáveis com Views falsas; undo/redo centralizado.
- Sem singletons nem `Find`.
- Alternativas descartadas: MVC (controller acoplado à cena), MVVM com runtime binding (difícil de depurar em telas geradas por dados), Zenject (sem manutenção) e Service Locator (dependências ocultas).
