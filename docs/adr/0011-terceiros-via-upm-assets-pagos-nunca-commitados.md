# ADR-0011: Terceiros via UPM; assets pagos nunca commitados

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

Código de terceiros copiado para `Assets/` fica desatualizado e mistura licenças. Assets pagos não podem ser redistribuídos.

## Decisão

Dependências entram pelo `Packages/manifest.json` (UPM, OpenUPM ou git URL com tag fixa). `Assets/Plugins/` só para binários sem pacote. Assets pagos ficam no `.gitignore`.

## Consequências

- O CI rejeita `Assets/MagicaCloth2/` versionado.
- `THIRD_PARTY_NOTICES.md` é atualizado a cada dependência nova.
