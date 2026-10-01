# ADR-0018: Pintura no espaço UV com pincel em 3D

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

O usuário quer pintar direto no modelo, inclusive sobre costuras de UV, com camadas e undo.

## Decisão

Camadas são `RenderTexture`s no espaço UV do alvo (corpo ou peça). O pincel é aplicado renderizando a malha desdobrada e medindo a distância 3D até o ponto do pincel, o que elimina cortes nas costuras. O ponto de contato vem de picking na GPU. O undo salva só os blocos tocados por cada traço. Metadados das camadas ficam no `CharacterModel`; pixels, no motor de pintura e nos anexos do `.weld`.

## Consequências

- Raio do pincel em metros, consistente em qualquer zoom.
- Peças pintáveis exigem UVs sem sobreposição.
- Custo de memória: ~16 MB por camada 2048² RGBA; limitar o número de camadas por alvo.
