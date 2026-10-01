# ADR-0015: Preset salvo como pacote .weld (zip)

- **Status:** Aceita
- **Data:** 2026-10-01

## Contexto

A pintura no modelo gera imagens (uma por camada) que precisam viajar com o preset. JSON com imagens em base64 incha os arquivos e fica ilegível; arquivos soltos ao lado do preset se perdem ao compartilhar.

## Decisão

O preset é um arquivo `.weld`: zip com `preset.json` e `attachments/` (PNGs das camadas). `PresetPackage` carrega o preset e os anexos. A leitura é defensiva, porque os arquivos vêm de outros usuários: nomes de anexo restritos (sem `..` nem caminhos absolutos), limite de entradas e de bytes descompactados contados de verdade (proteção contra zip bomb).

## Consequências

- Um arquivo só para compartilhar; o JSON continua legível dentro do zip.
- O formato `.weld.json` anterior deixa de existir (nunca foi lançado); substitui a extensão citada na ADR-0013.
- Novos tipos de anexo (miniatura, por exemplo) entram sem mudar o formato.
