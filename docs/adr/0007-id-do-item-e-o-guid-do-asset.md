# ADR-0007: ID do item é o GUID do asset

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Presets referenciam itens por ID. Nomes de arquivo e endereços do Addressables mudam; IDs digitados à mão colidem.

## Decisão

`CatalogItemData.id` espelha o GUID do `.meta`, sincronizado no Editor. Cópias recebem um ID novo, e IDs legados (que não correspondem a nenhum asset) são preservados.

## Consequências

- IDs únicos por construção e estáveis ao mover ou renomear.
- `.meta` não pode ser apagado; o CI verifica `.meta` faltando ou órfão.
