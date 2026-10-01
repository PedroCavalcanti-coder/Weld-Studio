# ADR-0005: Newtonsoft JSON para presets

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Presets precisam de dicionários (modificadores), evolução de schema e tolerância a campos desconhecidos.

## Decisão

Serialização com `com.unity.nuget.newtonsoft-json`: propriedades em camelCase, chaves de dicionário preservadas e migração passo a passo via `PresetMigrator` sobre `JObject`.

## Consequências

- `JsonUtility` (sem dicionários) descartado.
- O assembly `WeldStudio.Persistence` referencia `Newtonsoft.Json.dll`; o Core fica livre dele.
