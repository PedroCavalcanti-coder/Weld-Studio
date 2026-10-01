# 1. Arquitetura

> Status: **Decidido** (fundação). Mudanças estruturais entram como ADR em `docs/adr/`.

## 1.1 Visão geral

O Weld Studio é uma aplicação desktop feita em Unity 6.3 LTS (URP) para criar personagens 3D modulares.
Sobre um corpo base derivado do MakeHuman, o usuário veste roupas, troca cabelos, ajusta blendshapes de corpo
e rosto e vê tudo reagir com física em tempo real, por meio de uma interface em UI Toolkit.

Princípios que guiam todas as decisões:

1. **Os dados dirigem o comportamento.** Itens, categorias, modificadores e perfis de física são ScriptableObjects.
   Adicionar uma roupa nunca exige código novo.
2. **A UI e o 3D não se conhecem.** A UI conversa com o modelo do personagem; a montagem 3D reage ao modelo.
   Trocar a UI (ou rodar sem ela, nos testes) não toca em código de malha.
3. **Memória sob demanda.** Nada pesado é carregado antes de ser usado, e tudo é liberado quando deixa de ser
   usado (Addressables com contagem de referências). A pasta `Resources/` é proibida.
4. **Extensível sem fork.** A comunidade estende o app por interfaces (`IEquipable`, `ICharacterModifier`,
   `IPhysicsBackend`), por módulos de código opcionais e por content packs.
5. **Dependências opcionais nunca quebram o build.** O Magica Cloth 2 (pago) e outros plugins ficam em módulos
   isolados, que só compilam quando o plugin está instalado.

## 1.2 Estrutura do repositório

Estrutura-alvo. As pastas são criadas conforme o código e o conteúdo chegam; pasta vazia não é versionada.

```
Weld-Studio/                              # raiz do repo = raiz do projeto Unity
├── .github/
│   ├── workflows/                        # CI: testes EditMode/PlayMode + builds (GameCI), checkout com LFS
│   ├── ISSUE_TEMPLATE/
│   └── PULL_REQUEST_TEMPLATE.md
├── docs/                                 # este planejamento
│   └── adr/                              # uma decisão arquitetural por arquivo (0001-*.md, ...)
├── SourceAssets/                         # fontes de arte em LFS, FORA de Assets/ (a Unity não importa)
│   ├── makehuman/                        # .mhm, .mhclo, .mhskel, targets originais (CC0)
│   ├── blender/                          # .blend de autoria: corpo, roupas, hair cards, shape keys
│   └── textures/                         # .psd / .spp / .sbsar de origem
├── Tools/                                # scripts que rodam fora da Unity
│   ├── blender/                          # exportadores Python (FBX, regiões do corpo, shape keys)
│   └── ci/                               # validadores de linha de comando
├── Packages/
│   ├── manifest.json                     # URP, Addressables, Animation Rigging, VContainer...
│   └── packages-lock.json
├── ProjectSettings/
├── Assets/
│   ├── WeldStudio/                       # 1st-party: código e assets do aplicativo
│   │   ├── Runtime/
│   │   │   ├── Core/                     # WeldStudio.Core: contratos, domínio, ScriptableObjects
│   │   │   │   ├── Abstractions/         #   IEquipable, ICharacterModifier, ICharacterRig, IAssetProvider...
│   │   │   │   ├── Domain/               #   CharacterModel, comandos, enums, DTOs de preset
│   │   │   │   └── Data/                 #   CatalogItemData, ClothingItemData, PhysicsProfileData...
│   │   │   ├── Content/                  # WeldStudio.Content: Addressables, catálogo, content packs
│   │   │   ├── Character/                # WeldStudio.Character: CharacterRig, bone remap, assembler
│   │   │   ├── Physics/                  # WeldStudio.Physics: Unity Cloth, spring bones, colisores
│   │   │   ├── Animation/                # WeldStudio.Animation: Animator, IK (Animation Rigging)
│   │   │   ├── Rendering/                # WeldStudio.Rendering: máscara do corpo, bridges de shader
│   │   │   ├── Persistence/              # WeldStudio.Persistence: presets JSON assíncronos
│   │   │   ├── UI/                       # WeldStudio.UI: Presenters + Views (UXML/USS ao lado do C#)
│   │   │   └── App/                      # WeldStudio.App: composition root e bootstrap
│   │   ├── Editor/                       # WeldStudio.Editor: validadores, importadores, inspectors
│   │   ├── Tests/
│   │   │   ├── EditMode/                 # domínio puro: regras, comandos, serialização
│   │   │   └── PlayMode/                 # bone remap, Addressables, física, performance
│   │   ├── Shaders/                      # pele (SSS), cabelo (alpha clip + anisotropia), olhos, includes HLSL
│   │   ├── Settings/                     # URP Assets, Renderer, Volume Profiles, PanelSettings, Input Actions
│   │   └── Scenes/                       # Boot.unity: a única cena do build
│   ├── Content/                          # conteúdo Addressable (LFS); nenhuma cena referencia direto
│   │   └── Core/                         # content pack oficial; packs da comunidade seguem o mesmo formato
│   │       ├── Body/                     #   corpo base, esqueleto, blendshapes, materiais de pele
│   │       ├── Clothing/<Item>/          #   FBX + prefab + materiais + texturas + ícone + ClothingItemData
│   │       ├── Hair/<Item>/
│   │       ├── Animations/               #   idles, poses de inspeção (T/A)
│   │       ├── Modifiers/                #   definições de sliders (corpo, rosto, macros)
│   │       └── Profiles/                 #   perfis de física, categorias da UI
│   ├── Modules/                          # extensões de código opcionais, cada uma com asmdef próprio
│   │   └── MagicaClothBridge/            # WeldStudio.Physics.MagicaCloth2 (só compila com o define)
│   ├── Plugins/                          # binários de terceiros sem pacote UPM (vazio por padrão)
│   ├── MagicaCloth2/                     # no .gitignore: asset pago, cada dev importa localmente
│   └── AddressableAssetsData/            # gerado pelo Addressables (grupos, perfis, schemas)
├── .gitattributes                        # Git LFS + Smart Merge
├── .gitignore
├── LICENSE                               # MIT (código)
├── THIRD_PARTY_NOTICES.md                # MakeHuman (CC0), VContainer (MIT), etc.
├── CONTRIBUTING.md
└── README.md
```

### Por que essa estrutura

| Decisão | Justificativa |
|---------|---------------|
| Raiz do repo = raiz do projeto Unity | Um clone e o projeto abre. O `.gitignore` atual (template oficial de Unity) já assume isso. |
| `Assets/WeldStudio/` (código) separado de `Assets/Content/` (conteúdo) | Revisões diferentes (code review × revisão de arte), pesos diferentes no LFS. Os grupos de Addressables mapeiam 1:1 para as pastas de conteúdo. |
| `SourceAssets/` fora de `Assets/` | A Unity não importa `.blend` sem o Blender instalado em toda máquina, e o import é lento. Só os FBX exportados entram em `Assets/`. As fontes continuam versionadas (LFS) para quem quiser editar. |
| `Modules/` com asmdef isolado | Extensões opcionais (como a ponte com o Magica Cloth 2) compilam só quando a dependência existe (`defineConstraints`). A comunidade contribui módulos sem tocar no Core. |
| Terceiros via UPM (`Packages/manifest.json`) | Nenhum código de terceiros copiado para `Assets/`. `Plugins/` fica só para binários sem pacote. Assets pagos nunca são commitados. |
| Uma pasta por item de conteúdo | Tudo o que pertence a uma roupa (FBX, prefab, materiais, texturas, ícone, `ClothingItemData`) fica junto: fácil de contribuir, revisar, mover e remover. |
| Uma única cena (`Boot.unity`) | Todo o resto é carregado por Addressables e DI. Cenas não acumulam referências diretas que puxariam conteúdo para a memória. |

Exemplo de pasta de item:

```
Assets/Content/Core/Clothing/TShirt_Crew/
├── TShirt_Crew.fbx
├── TShirt_Crew.prefab       # Addressable, grupo Clothing
├── TShirt_Crew.asset        # ClothingItemData, label weld.catalog, grupo Catalog
├── Icon.png                 # Addressable, grupo Icons
├── Materials/
└── Textures/
```

## 1.3 Assemblies e regra de dependência

Cada pasta de `Runtime/` é um Assembly Definition. O compilador garante a direção das dependências, e
compilações incrementais ficam mais rápidas.

```
                         ┌────────────────────────┐
                         │    WeldStudio.App      │  composition root (VContainer)
                         └───────────┬────────────┘
                                     │ conhece todos
   ┌──────────┬──────────┬───────────┼───────────┬────────────┬─────────────┐
   ▼          ▼          ▼           ▼           ▼            ▼             ▼
  .UI     .Content   .Character  .Physics   .Animation  .Persistence  .Rendering
   │          │          │           │           │            │             │
   └──────────┴──────────┴───────────┴─────┬─────┴────────────┴─────────────┘
                                           ▼
                                  WeldStudio.Core
                     (contratos, domínio, ScriptableObjects)
                                           ▲
                      Modules/* (ex.: WeldStudio.Physics.MagicaCloth2)
```

Regras:

1. Assemblies de feature dependem apenas de `WeldStudio.Core` (e de pacotes Unity), **nunca umas das outras**.
   Quando uma precisa de algo da outra, o contrato vai para `Core/Abstractions` e o `App` liga as pontas.
2. Só `WeldStudio.App` conhece todas as implementações concretas (é o composition root).
3. Módulos opcionais dependem do `Core` e, quando estendem uma feature, dessa feature (ex.: a ponte do Magica
   pode reutilizar utilitários de `WeldStudio.Physics`).
4. `WeldStudio.Editor` e os assemblies de teste podem depender de qualquer coisa.

| Assembly | Namespace | Responsabilidade | Depende de |
|----------|-----------|------------------|------------|
| `WeldStudio.Core` | `WeldStudio.Core`, `WeldStudio.Core.Data` | Interfaces, domínio (C# puro), enums, ScriptableObjects | Addressables (apenas o tipo `AssetReference`) |
| `WeldStudio.Content` | `WeldStudio.Content` | `AddressablesAssetProvider`, `CatalogService`, content packs | Core, Addressables |
| `WeldStudio.Character` | `WeldStudio.Character` | `CharacterRig`, remapeamento de ossos, `CharacterAssembler`, aplicação de modificadores | Core |
| `WeldStudio.Physics` | `WeldStudio.Physics` | Backends Unity Cloth e spring bones, geração de colisores | Core |
| `WeldStudio.Animation` | `WeldStudio.Animation` | Animator (idle), IK com Animation Rigging | Core, Animation Rigging |
| `WeldStudio.Rendering` | `WeldStudio.Rendering` | Composição da máscara do corpo, propriedades de shader | Core, URP |
| `WeldStudio.Persistence` | `WeldStudio.Persistence` | `JsonPresetRepository`, migrações de schema | Core, Newtonsoft JSON |
| `WeldStudio.UI` | `WeldStudio.UI` | Presenters, Views (UXML/USS) | Core |
| `WeldStudio.App` | `WeldStudio.App` | `LifetimeScope`s, bootstrap | Todos + VContainer |
| `WeldStudio.Editor` | `WeldStudio.Editor` | Validadores, importadores, ferramentas de conteúdo | Todos (somente Editor) |
| `WeldStudio.Physics.MagicaCloth2` | `WeldStudio.Physics.MagicaCloth2` | Backend Magica Cloth 2 | Core, Physics, Magica Cloth 2 (opcional) |

## 1.4 Padrão arquitetural: MVP + injeção de dependência (VContainer)

**Decidido:** MVP (Model-View-Presenter) na camada de apresentação, sobre um núcleo em camadas, com injeção de
dependência pelo **VContainer**.

```
┌──────────────────────────────────────────────────────────────────────────┐
│ Apresentação     Views (UXML + USS + C# fino)  ⇄  Presenters             │
├──────────────────────────────────────────────────────────────────────────┤
│ Aplicação        Comandos (Equip, SetModifier, LoadPreset), histórico    │
│                  de undo/redo, orquestradores (CharacterAssembler)       │
├──────────────────────────────────────────────────────────────────────────┤
│ Domínio          CharacterModel, regras de conflito, DTOs (C# puro)      │
├──────────────────────────────────────────────────────────────────────────┤
│ Infraestrutura   Addressables, arquivos/JSON, SkinnedMeshRenderer,       │
│ (adapters)       Unity Cloth / Magica, Animation Rigging, shaders        │
└──────────────────────────────────────────────────────────────────────────┘
```

Por que este padrão:

- **SRP e desacoplamento exigidos:** a troca de malha (infraestrutura) e a UI (apresentação) só se encontram
  através do modelo de domínio. Nenhuma das duas referencia a outra.
- **View passiva:** a View expõe eventos ("item clicado", "slider mudou") e setters ("mostrar itens"). Toda a
  lógica fica no Presenter, que é C# puro e testável com uma View falsa. Se o UI Toolkit mudar, só as Views mudam.
- **Undo/Redo:** como toda mudança passa por comandos sobre o `CharacterModel`, o histórico vem quase de graça.
  Numa ferramenta de criação, isso é obrigatório.
- **DI no lugar de singletons:** as dependências ficam explícitas no construtor. Os testes trocam implementações
  (por exemplo, um `IAssetProvider` em memória) e vários personagens podem coexistir (escopos filhos).

Por que **VContainer**: é mantido ativamente, rápido (resolução com pouca reflexão e geração de código opcional),
tem uma API pequena e entry points sem MonoBehaviour (`IStartable`, `IAsyncStartable`, `ITickable`). A licença é
MIT, compatível com a deste repositório.

Alternativas descartadas:

| Alternativa | Motivo |
|-------------|--------|
| MVC clássico | Na Unity, o Controller tende a virar um MonoBehaviour "deus" acoplado à cena e à View. |
| MVVM com runtime data binding do UI Toolkit | Funciona bem em formulários estáticos, mas nossas telas são geradas a partir de dados (centenas de sliders de blendshape, grids de catálogo). Presenters explícitos são mais fáceis de depurar e perfilar. As Views ainda podem usar binding internamente para casos triviais. |
| Zenject / Extenject | Sem releases há anos, uso pesado de reflexão no startup, API extensa. |
| Singletons / Service Locator | Dependências ocultas, testes difíceis, impede múltiplos personagens. |

## 1.5 Classes e interfaces fundamentais

As sete peças que formam a fundação. Os nomes são definitivos; o código será escrito nas fases indicadas no
[roadmap](04-roadmap.md).

### 1. `CharacterModel` (`WeldStudio.Core`, C# puro)
Fonte única da verdade do personagem: itens equipados (por slot e camada), variante de material escolhida em
cada item e valores dos modificadores. Aplica as regras de domínio (conflito de slot + camada), emite eventos
(`EquipmentChanged`, `ModifierChanged`, `PresetApplied`) e converte o estado de/para `CharacterPreset` (DTO).
Não conhece Unity, Addressables ou UI, por isso é 100% testável em EditMode. As mudanças chegam por comandos
(`ICommand` + `CommandHistory`), o que viabiliza undo/redo.

### 2. `IEquipable` (`WeldStudio.Core`)
Contrato de uma peça **instanciada** que pode ser presa ao personagem: `Attach(ICharacterRig)`, `Detach()`,
ocupação (slots e camada) e regiões do corpo que esconde. A implementação padrão `SkinnedEquipable` cobre
roupas e cabelos (remapeamento de ossos). A comunidade pode criar, por exemplo, `BoneAttachedEquipable`
(óculos e chapéus rígidos presos a um osso) ou equipáveis procedurais. Uma factory registrada no container mapeia
tipo de definição → implementação, de modo que novos tipos entram sem alterar o Core (Open/Closed).

### 3. `ICharacterModifier` (`WeldStudio.Core`)
Contrato de qualquer ajuste paramétrico serializável: `Id`, faixa (mínimo, máximo, padrão) e
`Apply(ICharacterRig, float value)`. Implementações previstas: `BlendShapeModifier` (um slider → um blendshape),
`MacroModifier` (um slider → vários blendshapes com curvas, como "Idade" ou "Peso") e `SkinToneModifier`. A UI
gera os sliders a partir da lista de modificadores sem conhecer nenhum deles.

### 4. `ICharacterRig` → `CharacterRig` (`WeldStudio.Character`, MonoBehaviour no prefab do corpo)
A "interface do SkinnedMesh". É dona do esqueleto Humanoid, do `Animator`/`RigBuilder` e do
`SkinnedMeshRenderer` do corpo. Responsabilidades:
- manter o dicionário nome → `Transform` dos ossos;
- remapear `bones` e `rootBone` de cada `SkinnedMeshRenderer` de uma peça para o esqueleto do corpo, reparentar
  ossos extras (saia, mechas de cabelo) sob o osso correspondente e descartar o esqueleto duplicado do prefab;
- sincronizar pesos de blendshape corpo → peças por nome, apenas quando mudam (nunca a cada frame);
- aplicar a máscara de ocultação do corpo.

### 5. `IAssetProvider` → `AddressablesAssetProvider` (`WeldStudio.Content`)
O único ponto do código que chama a API de Addressables. Faz carregamento assíncrono com contagem de referências
por chave (dois personagens vestindo a mesma camiseta = um load), liberação determinística ao desequipar,
cancelamento de cargas obsoletas e carregamento de catálogos de conteúdo externos (mods). Trocar Addressables
por outra solução afetaria só esta classe.

### 6. `ICatalogService` → `CatalogService` (`WeldStudio.Content`)
Na inicialização, carrega todos os `CatalogItemData` pela label `weld.catalog`. Só os metadados entram na
memória; malhas e texturas continuam descarregadas. Valida cada entrada (`CollectValidationErrors`) e descarta
as quebradas com aviso, indexa por `Id` e oferece consultas para a UI (por tipo, slot, tag e texto). Content packs
e mods registram catálogos adicionais aqui.

### 7. `IPresetRepository` → `JsonPresetRepository` (`WeldStudio.Persistence`)
Salva e carrega `CharacterPreset` em JSON de forma assíncrona: serialização em thread de background
(`Awaitable.BackgroundThreadAsync`), I/O assíncrono, gravação atômica (arquivo temporário + troca), campo
`schemaVersion` com migrações. O preset guarda só IDs e valores, nunca referências a objetos Unity: é seguro fora
da main thread e portátil entre máquinas. IDs desconhecidos (mod ausente) são reportados, não quebram o
carregamento. Formato em [02-contrato-de-dados.md](02-contrato-de-dados.md#28-formato-do-preset-json).

### Segundo nível (logo abaixo da fundação)

| Classe | Papel |
|--------|-------|
| `CharacterAssembler` | Orquestrador: escuta o `CharacterModel` e usa `IAssetProvider`, `IEquipable`, `ICharacterRig` e `IPhysicsBackend` para refletir o estado na cena. |
| `IPhysicsBackend` | Contrato de backend de física: `UnityClothBackend`, `SpringBoneBackend` (nativo do projeto), `MagicaCloth2Backend` (módulo opcional). |
| `ICommand` / `CommandHistory` | Comandos reversíveis para undo/redo, com coalescência (arrastar um slider = um passo de undo). |
| `SkinnedEquipable` | Implementação padrão de `IEquipable` para roupas e cabelos. |
| `ModifierDefinition` (SO) | Definição em dados de um slider (blendshape alvo, faixa, curva, categoria). |
| Presenters/Views | `CatalogPresenter`, `ModifierPanelPresenter`, `VariantPickerPresenter`, `PresetPresenter`, etc. |
| `AppBootstrapper` | Entry point assíncrono: inicializa Addressables, catálogo, corpo base e o preset inicial. |

## 1.6 Escopos de injeção (VContainer)

| Escopo | Vida | Registra |
|--------|------|----------|
| `RootLifetimeScope` | Aplicação inteira | `IAssetProvider`, `ICatalogService`, `IPresetRepository`, backends de física, `AppBootstrapper` |
| `CharacterLifetimeScope` (filho) | Um por personagem | `CharacterModel`, `CommandHistory`, `ICharacterRig`, `CharacterAssembler`, aplicador de modificadores |
| `UILifetimeScope` (filho) | Enquanto a UI existe | Presenters (entry points) e Views |

Escopos por personagem permitem, no futuro, comparar dois personagens lado a lado sem nenhum singleton.

## 1.7 Fluxos principais

### Inicialização
1. `Boot.unity` contém apenas o `RootLifetimeScope`.
2. O `AppBootstrapper` (`IAsyncStartable`) inicializa o Addressables e chama `CatalogService.LoadAsync()`.
3. O corpo base é carregado e instanciado, e o `CharacterLifetimeScope` filho é criado.
4. O último preset (ou o preset padrão) é aplicado e a UI é exibida.

### Equipar uma roupa
```
ClothingBrowserView ──clique(itemId)──▶ CatalogPresenter
  └─▶ EquipCommand ─▶ CharacterModel.Equip(item)
        regra: remove itens que dividem slot na mesma camada → evento EquipmentChanged(adicionados, removidos)
  └─▶ CharacterAssembler (escuta o evento)
        ├─ IAssetProvider.LoadAsync(item.Prefab)   Addressables, ref-count, cancelável
        ├─ IEquipable.Attach(ICharacterRig)        bone remap + sync de blendshapes
        ├─ ICharacterRig: atualiza a máscara do corpo
        └─ IPhysicsBackend.Apply(perfil)           primeiro perfil cujo backend está instalado
      removidos: IEquipable.Detach() + IAssetProvider.Release() → memória liberada
```
A UI nunca toca em malha, e o código de malha nunca toca na UI.

**Concorrência:** se o usuário clicar em A e logo depois em B no mesmo slot, a carga de A pode terminar depois
da de B. O assembler guarda um token de geração por item pedido. Resultado obsoleto é liberado ao chegar, sem
ser anexado.

### Mover um slider de blendshape
1. `SliderView` emite a mudança → `ModifierPanelPresenter` → `SetModifierCommand`. Durante o arraste os comandos
   são coalescidos, e soltar o slider fecha um único passo de undo.
2. `CharacterModel.SetModifier` → evento → `ICharacterModifier.Apply(rig, valor)`.
3. O rig aplica os pesos no corpo e sincroniza as peças com `conformToBodyShape` (mapas de índice de blendshape
   em cache por renderer).
4. Os backends de física recalculam colisores (com debounce), já que o volume do corpo mudou.

### Salvar e carregar preset
- **Salvar:** `CharacterModel.ToPreset()` (snapshot na main thread) → `IPresetRepository.SaveAsync` →
  serialização em background → escrita em arquivo temporário → troca atômica.
- **Carregar:** leitura assíncrona → desserialização em background → migração de schema → na main thread,
  `CharacterModel.ApplyPreset` (lote, um único evento) → o assembler calcula a diferença e carrega os itens em
  paralelo → os IDs não encontrados são exibidos ao usuário.

## 1.8 Assincronia e threading

- **Decidido:** `Awaitable` nativo da Unity 6 (sem UniTask). Handles do Addressables são aguardados via `.Task`.
- Toda API assíncrona recebe `CancellationToken`. O token de vida do escopo cancela tudo ao destruir o personagem.
- Objetos Unity só são tocados na main thread. Trabalho em background só opera sobre DTOs puros.
- Exceções em operações assíncronas são registradas e transformadas em feedback na UI, nunca engolidas.

## 1.9 Memória (regras de Addressables)

1. `Resources/` é proibida. Nada de conteúdo é referenciado diretamente por cena ou prefab do app.
2. ScriptableObjects de catálogo referenciam conteúdo pesado só por `AssetReference`. Carregar o catálogo não
   carrega malhas, texturas nem ícones.
3. Todo load passa pelo `IAssetProvider`, nunca por `AssetReference.LoadAssetAsync()` (a referência vive num
   asset compartilhado e só guarda um handle).
4. Cada load tem um release correspondente. Ícones só ficam carregados enquanto visíveis (lista virtualizada).
5. Antes de cada release, a análise do Addressables ("Check Duplicate Bundle Dependencies") deve passar.
6. Critério verificável: equipar e desequipar 50 itens seguidos deixa a memória no patamar inicial
   (Memory Profiler).

## 1.10 Física

- **Prefabs agnósticos de física.** Um item não carrega componentes de física. O backend adiciona e configura os
  componentes depois que a peça foi presa ao esqueleto, o que evita simular uma malha antes do remapeamento de
  ossos e permite backends opcionais.
- **Perfis com fallback.** `ClothingItemData.PhysicsProfiles` é uma lista ordenada por preferência (por exemplo,
  Magica → Unity Cloth). Vence o primeiro perfil cujo backend está instalado. Sem o Magica, a referência ao
  perfil Magica fica nula e é ignorada.
- **Backends previstos:**
  - `UnityClothBackend`: malhas de roupa (saias, capas, vestidos).
  - `SpringBoneBackend` (próprio, leve): cadeias de ossos para mechas de cabelo e jiggle corporal. A Unity não
    tem spring bones nativos, e o Unity Cloth não é adequado para cabelo.
  - `MagicaCloth2Backend` (módulo opcional): qualidade superior para roupas, cabelo e jiggle.
- **Não atravessar o corpo:**
  1. ocultação das regiões do corpo cobertas pela roupa;
  2. colisores de cápsula gerados a partir do esqueleto e dimensionados pela malha, recalculados quando os
     morphs do corpo mudam (o Unity Cloth só colide com esferas e cápsulas);
  3. pesos de simulação pintados por vértice limitando o quanto cada vértice se afasta da pose skinned.

## 1.11 Renderização

- O URP não tem Subsurface Scattering nativo (é recurso do HDRP). Vamos escrever um shader de pele próprio
  (Shader Graph + HLSL): pre-integrated skin ou wrap lighting, com mapa de espessura/curvatura.
- Cabelo em hair cards: alpha clipping (passe opaco) + bordas suaves, com especular anisotrópico (Kajiya-Kay).
- A ocultação do corpo é feita no shader da pele: cada vértice carrega o índice da sua região (ver
  [pipeline](03-pipeline-de-conteudo.md#33-regiões-do-corpo)), o shader descarta as regiões marcadas na máscara de
  bits e combina com a máscara de textura opcional de cada peça. Tudo isso em uma única draw call.
- Cena de estúdio: HDRI, iluminação de três pontos, pós-processamento (tonemapping, AO).

## 1.12 Pontos de extensão para a comunidade

| Quero... | Como |
|----------|------|
| Adicionar roupas, cabelos ou acessórios | Content pack: pasta em `Assets/Content/<Pack>/` seguindo o [pipeline](03-pipeline-de-conteudo.md). Zero código. |
| Criar um novo tipo de equipável | Implementar `IEquipable` + uma subclasse de `CatalogItemData` e registrar a factory num módulo. |
| Criar um novo tipo de slider | Implementar `ICharacterModifier` + `ModifierDefinition`. |
| Integrar outro motor de física | Implementar `IPhysicsBackend` + uma subclasse de `PhysicsProfileData` num módulo em `Assets/Modules/`. |
| Distribuir conteúdo sem fazer fork | Mods como catálogos Addressables externos carregados em runtime (Fase 9). |

## 1.13 Convenções de código

- Namespaces = assembly (`WeldStudio.Core`, `WeldStudio.Character`, ...). Sub-namespace só para conjuntos grandes
  (`WeldStudio.Core.Data`).
- Código, identificadores e comentários em inglês.
- Campos serializados `private` com `[SerializeField]`, expostos por propriedades somente-leitura. ScriptableObjects
  são imutáveis em runtime.
- Membros de enums persistidos nunca são renumerados, só acrescentados.
- Sem `FindObjectOfType`, `GameObject.Find` ou singletons estáticos: tudo vem por injeção.
- Um tipo por arquivo, e o nome do arquivo é o nome do tipo.

## 1.14 Estratégia de testes

| Nível | O que cobre | Onde roda |
|-------|-------------|-----------|
| EditMode | `CharacterModel`, regras de conflito, comandos e undo, serialização e migração de presets, validação de dados | CI em todo PR |
| PlayMode | Remapeamento de ossos com prefabs de teste, sync de blendshapes, ciclo load/release do Addressables, backends de física | CI em todo PR |
| Performance | Tempo de troca de item, alocações por frame, memória após ciclos de equipar/desequipar (Performance Testing package) | CI noturno |
| Validação de conteúdo | Todos os `CatalogItemData` sem erros, Addressables sem duplicatas | CI em PRs que tocam `Assets/Content/` |
