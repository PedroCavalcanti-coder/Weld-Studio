# Architecture Decision Records

Cada arquivo registra uma decisão estrutural: contexto, decisão e consequências. Uma ADR aceita não é editada;
se a decisão mudar, crie uma nova ADR que a substitua e marque a antiga como *Substituída por ADR-XXXX*.

| ADR | Título | Status |
|-----|--------|--------|
| [0001](0001-raiz-do-repositorio-e-a-raiz-do-projeto-unity.md) | Raiz do repositório é a raiz do projeto Unity | Aceita |
| [0002](0002-mvp-na-apresentacao-e-injecao-de-dependencia-com-vcontainer.md) | MVP na apresentação e injeção de dependência com VContainer | Aceita |
| [0003](0003-um-assembly-por-camada-features-dependem-so-do-core.md) | Um assembly por camada; features dependem só do Core | Aceita |
| [0004](0004-task-nas-apis-assincronas-awaitable-para-frames-e-threads.md) | Task nas APIs assíncronas; Awaitable para frames e threads | Aceita |
| [0005](0005-newtonsoft-json-para-presets.md) | Newtonsoft JSON para presets | Aceita |
| [0006](0006-conteudo-so-via-addressables-pasta-resources-proibida.md) | Conteúdo só via Addressables; pasta Resources proibida | Aceita |
| [0007](0007-id-do-item-e-o-guid-do-asset.md) | ID do item é o GUID do asset | Aceita |
| [0008](0008-prefabs-agnosticos-de-fisica-perfis-com-fallback.md) | Prefabs agnósticos de física, perfis com fallback | Aceita |
| [0009](0009-unity-cloth-e-spring-bones-proprios-como-base-magica-cloth-2-opcional.md) | Unity Cloth e spring bones próprios como base; Magica Cloth 2 opcional | Aceita |
| [0010](0010-esqueleto-game-engine-do-makehuman-como-base.md) | Esqueleto Game engine do MakeHuman como base | Aceita |
| [0011](0011-terceiros-via-upm-assets-pagos-nunca-commitados.md) | Terceiros via UPM; assets pagos nunca commitados | Aceita |
| [0012](0012-codigo-em-ingles-documentacao-em-portugues.md) | Código em inglês, documentação em português | Aceita |
| [0013](0013-nome-do-projeto-weld-studio.md) | Nome do projeto: Weld Studio | Aceita |
| [0014](0014-testes-de-dominio-tambem-rodam-em-net-puro-no-ci.md) | Testes de domínio também rodam em .NET puro no CI | Aceita |
| [0015](0015-preset-salvo-como-pacote-weld-zip.md) | Preset salvo como pacote .weld (zip) | Aceita |
| [0016](0016-personalizacao-de-itens-definida-em-dados-zonas-de-cor-e-parametros.md) | Personalização de itens definida em dados (zonas de cor e parâmetros) | Aceita |
| [0017](0017-edicao-do-corpo-por-ossos-e-blendshapes-combinados-por-fonte.md) | Edição do corpo por ossos e blendshapes, combinados por fonte | Aceita |
| [0018](0018-pintura-no-espaco-uv-com-pincel-em-3d.md) | Pintura no espaço UV com pincel em 3D | Aceita |
| [0019](0019-previa-de-animacoes-com-playablegraph-proprio.md) | Prévia de animações com PlayableGraph próprio | Aceita |

## Modelo

```markdown
# ADR-XXXX: Título

- **Status:** Proposta | Aceita | Substituída por ADR-YYYY
- **Data:** AAAA-MM-DD

## Contexto

Qual problema ou força motivou a decisão.

## Decisão

O que foi decidido, de forma direta.

## Consequências

O que fica mais fácil, o que fica mais difícil e o que precisa ser acompanhado.
```
