# 4. Roadmap

> Status: **Proposta**. As fases são sequenciais nas dependências, mas trabalho de conteúdo (arte) pode avançar em
> paralelo a partir da Fase 2. Não há estimativas de prazo: dependem do tamanho do time e serão definidas ao
> iniciar cada fase.

Legenda: `[x]` feito · `[ ]` a fazer

## Marcos

| Marco | Fases | O que o usuário consegue fazer |
|-------|-------|--------------------------------|
| **v0.1 – Tech demo** | 0 a 3 | Ver um personagem MakeHuman e trocar roupas por código ou por uma UI de debug, sem vazar memória. |
| **v0.5 – Editor utilizável** | 4 a 6 | Usar a UI completa: catálogo, sliders de corpo e rosto, variantes de cor, salvar e carregar, undo/redo, idle com IK. |
| **v1.0 – Sucessor do Fuse** | 7 a 9 | Física de roupas, cabelos e jiggle; pele e cabelo de qualidade; export do personagem; mods. |

---

## Fase 0 – Fundação do repositório

Objetivo: qualquer pessoa clona, abre e contribui com o mesmo setup.

- [x] Planejamento em `docs/` e README
- [x] Contrato de dados em C# (`CatalogItemData`, `ClothingItemData`, enums, `MaterialVariant`, `PhysicsProfileData`)
- [x] `.gitattributes`: LFS para binários (padrões sem diferenciar maiúsculas), Smart Merge para YAML da Unity
- [x] `.gitignore`: `Assets/MagicaCloth2/`, backups do Blender, arquivos de SO, saídas de `Tools/`
- [x] `.meta` versionados para todos os arquivos e pastas existentes (GUIDs estáveis desde o primeiro clone)
- [x] CI no GitHub Actions (`.github/workflows/ci.yml`):
  - [x] higiene do repositório (`Tools/ci/check_repo.py`): `.meta` faltando ou órfão, binário fora do LFS,
        pasta `Resources/`, asset pago versionado
  - [x] testes de domínio em .NET puro contra stubs da Unity (`Tools/ci/DomainTests`, [ADR-0014](adr/0014-testes-de-dominio-tambem-rodam-em-net-puro-no-ci.md))
  - [x] testes da Unity (GameCI), que pulam com aviso até existirem o projeto e o secret `UNITY_LICENSE`
- [x] `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `THIRD_PARTY_NOTICES.md`, templates de issue e PR
- [x] ADRs 0001 a 0014 em [`docs/adr/`](adr/README.md)
- [x] Nome do projeto definido: **Weld Studio** ([ADR-0013](adr/0013-nome-do-projeto-weld-studio.md))
- [ ] **Requer a Unity (máquina do mantenedor):** criar o projeto Unity 6.3 LTS na raiz ([Setup §6.3](06-setup.md#63-criar-o-projeto-unity-uma-única-vez)):
      URP, espaço de cor Linear, Force Text, Visible Meta Files
- [ ] **Requer a Unity:** instalar os pacotes e versionar `Packages/manifest.json` e `packages-lock.json`
- [ ] **Requer a Unity:** abrir o projeto, confirmar zero erros no console e zero `.meta` novos gerados para os
      arquivos existentes; rodar os testes EditMode no Test Runner
- [ ] Configurar os secrets `UNITY_LICENSE`, `UNITY_EMAIL` e `UNITY_PASSWORD` no GitHub

**Pronto quando:** um clone novo abre na Unity sem erros, sem `.meta` gerados localmente, e o CI fica verde.

## Fase 1 – Núcleo de domínio (sem 3D)

Objetivo: toda a lógica do personagem funcionando e testada, sem cena.

- [x] Interfaces em `Core/Abstractions`: `IEquipableDefinition`, `IEquipable`, `IEquipableFactory`,
      `ICharacterModifier`, `ICharacterRig`, `IAssetProvider` (+ `AssetLease<T>`), `ICatalogService`,
      `IPresetRepository`, `IPhysicsBackend`
- [x] `CharacterModel`: equipar e desequipar com regra de conflito, variantes, modificadores, eventos, aplicação de
      preset com diff e preservação de itens não resolvidos
- [x] `ICommand` + `CommandHistory` (undo/redo com coalescência) e comandos `Equip`, `Unequip`, `SetVariant`,
      `SetModifier`, `ApplyPreset`
- [x] `CharacterPreset` (DTO) + `JsonPresetRepository` (assíncrono, gravação atômica, `schemaVersion`) +
      `PresetMigrator`
- [x] 39 testes EditMode cobrindo o domínio e a persistência, passando em .NET 8 com C# 9
- [ ] Rodar os mesmos testes dentro da Unity (depende da Fase 0)
- [ ] Arquivos-exemplo de preset versionados por schema (entram junto com a primeira migração, v1 → v2)

**Pronto quando:** equipar, desequipar, desfazer, salvar e carregar funcionam nos testes, sem nenhuma cena.
✅ Cumprido em .NET; falta a confirmação dentro da Unity.

## Fase 2 – Conteúdo e Addressables

Objetivo: carregar e liberar conteúdo de forma confiável e tornar a contribuição de conteúdo simples.

- [ ] `AddressablesAssetProvider`: ref-count, cancelamento, release determinístico
- [ ] `CatalogService`: descoberta por label, validação, índice por ID, consultas
- [ ] Grupos, labels e perfis do Addressables conforme [02 §2.9](02-contrato-de-dados.md#29-convenções-de-addressables)
- [ ] Ferramentas de Editor:
  - [ ] *Validate Catalog*
  - [ ] `AssetPostprocessor` de IDs
  - [ ] atribuição automática de grupos e labels por pasta
  - [ ] gerador de ícones
- [ ] Conteúdo provisório (manequim e peças simples) para testar o fluxo antes do MakeHuman
- [ ] Testes PlayMode do ciclo load/release

**Pronto quando:** o Memory Profiler mostra a memória voltando ao patamar inicial depois de 50 ciclos de
carregar e liberar.

## Fase 3 – Personagem base e montagem modular

Objetivo: o coração do Fuse funcionando, com roupas presas ao esqueleto em tempo real.

- [ ] Pipeline do corpo ([03 §3.1](03-pipeline-de-conteudo.md#31-corpo-base)): FBX, Avatar Humanoid,
      `BodyDefinition`
- [ ] Decidir Q2 (rig oficial com ou sem ossos de jiggle) antes do primeiro conteúdo definitivo
- [ ] `CharacterRig`: mapa de ossos, remapeamento, reparent de ossos extras, sync de blendshapes por nome
- [ ] `SkinnedEquipable` + `CharacterAssembler` (com tokens de geração contra condições de corrida)
- [ ] Oclusão do corpo: regiões gravadas na malha + shader de pele mínimo que descarta regiões + composição de
      máscaras
- [ ] `HairItemData` + 1 cabelo de teste; 3 roupas de teste (camiseta, calça, vestido)
- [ ] Composition root (`RootLifetimeScope`, `CharacterLifetimeScope`) + `AppBootstrapper`
- [ ] UI de debug provisória (lista de itens, sem design)

**Pronto quando:** dá para trocar as peças em Play Mode sem pele atravessando na pose de inspeção e no idle, sem
erros no console e sem vazamento de memória. → **v0.1**

## Fase 4 – Interface (UI Toolkit, MVP)

Objetivo: o fluxo completo do usuário.

- [ ] Shell: viewport 3D ao centro, navegação por etapas (Corpo → Rosto → Roupas → Cabelo → Cores → Pose →
      Exportar), painel de propriedades
- [ ] Catálogo: `ListView` virtualizada (linhas com N itens), ícones carregados sob demanda, busca, filtros por
      `CatalogCategoryData`
- [ ] Painel de modificadores: sliders gerados a partir de `ModifierDefinition`, agrupados, com reset
- [ ] Seletor de variantes (amostras de cor)
- [ ] Salvar, carregar e "salvar como"; lista de presets recentes
- [ ] Undo/redo com atalhos (Input System)
- [ ] Câmera orbital (girar, zoom, enquadrar rosto ou corpo)
- [ ] Tema USS (variáveis de cor e tipografia), tela de créditos com a atribuição dos assets
- [ ] Testes dos Presenters com Views falsas

**Pronto quando:** um usuário novo cria, salva e reabre um personagem sem instrução.

## Fase 5 – Modificadores e blendshapes

- [ ] `BlendShapeModifier`, `MacroModifier`, `SkinToneModifier` + definições em dados
- [ ] Subconjunto curado de targets corporais do MakeHuman, com orçamento de memória medido
- [ ] Expressões faciais assadas como shape keys (nomenclatura ARKit), presets de expressão
- [ ] Conformidade das roupas com os morphs do corpo (script de Blender + validação de nomes no Editor)
- [ ] `AccessoryItemData` + `BoneAttachedEquipable` (óculos, chapéus rígidos)

**Pronto quando:** mudar peso, idade e proporções mantém todas as roupas de teste sem atravessar.

## Fase 6 – Animação e IK

- [ ] Animator com estados Idle (pelo menos 2 variações) e pose de inspeção (T/A)
- [ ] Animation Rigging:
  - [ ] olhar para a câmera (`MultiAimConstraint` em cabeça e pescoço, com limites)
  - [ ] alinhamento dos pés ao chão (`TwoBoneIKConstraint` + raycast), considerando altura de salto do calçado
- [ ] Liga/desliga de IK e animação na UI

**Pronto quando:** o personagem segue a câmera com naturalidade e os pés não flutuam nem afundam com nenhum
calçado de teste. → **v0.5**

## Fase 7 – Física

- [ ] `IPhysicsBackend` + seleção de perfil com fallback
- [ ] `UnityClothBackend`: colisores de cápsula gerados pelo esqueleto e recalculados quando os morphs mudam,
      pesos por cor de vértice
- [ ] `SpringBoneBackend` próprio: cabelo e jiggle (com Jobs/Burst se o profiling pedir)
- [ ] Módulo `MagicaClothBridge`: define `WELD_MAGICACLOTH2`, script de Editor que detecta o Magica e ativa o
      define, `MagicaClothProfile`
- [ ] CI compila **sem** o Magica (garantia de que o módulo é opcional)

**Pronto quando:** saia e capa não atravessam o corpo no idle nem ao girar a câmera, o cabelo reage à inércia e o
app compila e roda sem o Magica.

## Fase 8 – Renderização

- [ ] Shader de pele com SSS para URP (pre-integrated ou wrap + espessura), detalhes de poros e microssombras
- [ ] Shader de cabelo: alpha clip, bordas suaves, anisotropia (Kajiya-Kay)
- [ ] Shader de olhos (refração/parallax da íris, umidade)
- [ ] Shader de roupa (base Lit + detalhe + tingimento por máscara)
- [ ] Estúdio: HDRI, iluminação de três pontos, pós-processamento, presets de iluminação
- [ ] Orçamento de performance: 60 FPS em GPU desktop intermediária (a calibrar)

## Fase 9 – Exportação e ecossistema

- [ ] Export do personagem montado:
  - [ ] **glTF/GLB** em runtime (avaliar glTFast, `com.unity.cloud.gltfast`)
  - [ ] FBX: o FBX Exporter da Unity só roda no Editor, então avaliar alternativas (ADR)
- [ ] Opções de export: juntar malhas, remover geometria oculta, manter ou assar blendshapes, compatibilidade
      com retargeting (Mixamo e afins)
- [ ] Mods: catálogos Addressables externos, `ContentPackManifest`, projeto-modelo de SDK para criadores
- [ ] Localização (`com.unity.localization`): pt-BR e en
- [ ] Builds Windows, macOS e Linux no CI; releases no GitHub

**Pronto quando:** um personagem exportado abre corretamente no Blender e em outra engine. → **v1.0**

---

## Trabalho contínuo (todas as fases)

- Toda decisão estrutural nova vira ADR em `docs/adr/`.
- Todo PR: CI verde e validação de conteúdo verde quando tocar `Assets/Content/`.
- Profiling (CPU, memória, GPU) ao fim de cada fase, com resultado anotado no PR de fechamento da fase.
