# ADR-0014: Testes de domínio também rodam em .NET puro no CI

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Rodar testes da Unity no CI exige um projeto Unity e secrets de licença, que ainda não existem. Mesmo depois, o runner da Unity é lento para o ciclo de feedback.

## Decisão

`Tools/ci/DomainTests` compila `Runtime/Core`, `Runtime/Persistence` e `Tests/EditMode` com o .NET 8 (C# 9, warnings como erro) contra stubs mínimos da API da Unity, e roda os mesmos testes NUnit em todo PR.

## Consequências

- Feedback em segundos e sem licença.
- Os stubs precisam acompanhar o Core: tipo ou membro novo da Unity usado no Core exige um stub.
- Os testes EditMode dentro da Unity continuam sendo a referência final.
