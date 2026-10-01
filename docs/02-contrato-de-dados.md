# 2. Contrato de dados

> Status: **Decidido** para `CatalogItemData`, `ClothingItemData` e enums de domínio (código já escrito).
> **Proposta** para os demais tipos listados em 2.2.

## 2.1 Princípios

1. **Definições imutáveis.** ScriptableObjects descrevem *o que um item é*. *O que o personagem está vestindo*
   vive no `CharacterModel`, nunca no asset.
2. **Leves por construção.** Todo o catálogo é carregado no boot, então os SOs só guardam metadados. Malhas,
   texturas, materiais e ícones são referenciados por `AssetReference` e carregados sob demanda.
3. **IDs estáveis.** Presets salvos referenciam itens por `Id`, nunca por nome de arquivo ou endereço do
   Addressables, que podem mudar.
4. **Validáveis.** Todo tipo sabe listar os próprios erros de autoria (`CollectValidationErrors`). Entradas
   inválidas, por exemplo de um pack da comunidade, são descartadas com aviso em vez de derrubar o app.
5. **Atribuição obrigatória.** Todo item carrega autor, licença (SPDX) e origem, porque assets CC-BY exigem
   crédito.

## 2.2 Hierarquia de tipos

```
ScriptableObject
├── CatalogItemData (abstract)            ✅ escrito: identidade, apresentação, atribuição
│   ├── ClothingItemData                  ✅ escrito: roupa
│   ├── HairItemData                      ⏳ Fase 3: cabelo (hair cards + cadeias de ossos)
│   └── AccessoryItemData                 ⏳ Fase 5+: acessório rígido preso a um osso
├── PhysicsProfileData (abstract)         ✅ escrito: base agnóstica de motor
│   ├── UnityClothProfile                 ⏳ Fase 7
│   ├── SpringBoneProfile                 ⏳ Fase 7
│   └── MagicaClothProfile                ⏳ Fase 7 (módulo opcional)
├── ModifierDefinition (abstract)         ⏳ Fase 5: definição de slider
│   ├── BlendShapeModifierDefinition
│   └── MacroModifierDefinition
├── BodyDefinition                        ⏳ Fase 3: corpo base (prefab, rig, mapa de regiões)
├── CatalogCategoryData                   ⏳ Fase 4: aba/filtro da UI
└── ContentPackManifest                   ⏳ Fase 9: id, versão, autor, licença, dependências do pack
```

Tipos serializáveis (não-SO): `MaterialVariant` ✅.

**Categorias filtram itens, itens não conhecem categorias.** Um `CatalogCategoryData` ("Camisetas", "Sci-Fi")
descreve um filtro (tipo de item, slots, tags). Assim a definição do item não carrega preocupações de UI, e uma
nova categoria não exige editar dezenas de itens.

## 2.3 `CatalogItemData` (base abstrata)

Arquivo: `Assets/WeldStudio/Runtime/Core/Data/CatalogItemData.cs` · Namespace: `WeldStudio.Core.Data`

| Campo | Tipo | Uso |
|-------|------|-----|
| `id` | `string` | ID estável salvo nos presets. Atribuído automaticamente a partir do GUID do asset (ver 2.7). |
| `displayName` | `string` | Nome na UI. Obrigatório. |
| `description` | `string` (TextArea) | Texto de detalhe na UI. |
| `icon` | `AssetReferenceSprite` | Miniatura no catálogo, carregada só enquanto visível. |
| `tags` | `string[]` | Busca e filtros ("casual", "sci-fi"). Comparação sem diferenciar maiúsculas. |
| `sortOrder` | `int` | Ordenação; menor aparece primeiro. |
| `author` | `string` | Crédito. |
| `license` | `string` | Identificador SPDX (`CC0-1.0`, `CC-BY-4.0`). Padrão `CC0-1.0`. Obrigatório. |
| `sourceUrl` | `string` | Origem do asset (exigida pela maioria das licenças CC-BY). |

Membros públicos: propriedades somente-leitura para cada campo, `HasTag(string)`,
`CollectValidationErrors(ICollection<string>)` (virtual) e a constante `CatalogLabel = "weld.catalog"`.

## 2.4 `ClothingItemData`

Arquivo: `Assets/WeldStudio/Runtime/Core/Data/ClothingItemData.cs` · Menu: *Create → Weld Studio → Catalog →
Clothing Item*

| Grupo | Campo | Tipo | Uso |
|-------|-------|------|-----|
| Montagem | `prefab` | `AssetReferenceGameObject` | Prefab com um ou mais `SkinnedMeshRenderer` skinnados a uma cópia do esqueleto base. Os ossos são remapeados por nome em runtime. Obrigatório. |
| | `rigId` | `string` | Esqueleto para o qual o item foi riggado. Item de outro rig é recusado. Padrão `makehuman.game_engine` (ver questão em aberto Q2 em [05](05-decisoes-e-riscos.md)). |
| | `slots` | `EquipmentSlot` (flags) | Slots ocupados. Pelo menos um. |
| | `layer` | `EquipmentLayer` | Camada (da pele para fora). |
| | `conformToBodyShape` | `bool` | Copia os pesos dos blendshapes do corpo para blendshapes de mesmo nome na peça (segue peso, músculo, proporções). Padrão `true`. |
| Oclusão | `hiddenBodyRegions` | `BodyRegion` (flags) | Regiões do corpo escondidas enquanto a peça é usada. |
| | `bodyMask` | `AssetReferenceTexture2D` | Máscara precisa opcional no espaço UV do corpo (branco = oculto), combinada com as regiões. |
| Aparência | `materialVariants` | `MaterialVariant[]` | Cores/estilos selecionáveis; o primeiro é o padrão. Vazio = materiais do próprio prefab. |
| Física | `physicsProfiles` | `PhysicsProfileData[]` | Perfis por ordem de preferência. Vazio = peça estática. |

Membros públicos além das propriedades:

| Membro | Descrição |
|--------|-----------|
| `ConflictsWith(ClothingItemData)` | `true` quando as duas peças dividem pelo menos um slot **na mesma camada**. |
| `TryGetVariant(string, out MaterialVariant)` | Busca uma variante pelo ID (usado ao aplicar presets). |
| `DefaultVariant` | Primeira variante, ou `null` quando valem os materiais do prefab. |
| `HasBodyMask` | Se há máscara de textura atribuída. |
| `CollectValidationErrors(...)` | Base + prefab atribuído, rig não vazio, pelo menos um slot, IDs de variante não vazios e únicos e, no Editor, prefab com `SkinnedMeshRenderer`. |

> **Armadilha documentada no código:** nunca chame `item.Prefab.LoadAssetAsync()`. O `AssetReference` vive num
> asset compartilhado e guarda um único handle; dois personagens vestindo o mesmo item colidiriam. Todo load
> passa pelo `IAssetProvider`, que usa a referência como *chave* em `Addressables.LoadAssetAsync`.

## 2.5 Vocabulário de domínio (enums)

Pasta: `Assets/WeldStudio/Runtime/Core/Domain/` · Namespace: `WeldStudio.Core`

**`EquipmentSlot`** (flags): `Hair`, `FacialHair`, `Head`, `Face`, `Neck`, `Torso`, `Back`, `Hands`, `Waist`,
`Legs`, `Feet`.

**`EquipmentLayer`**: `Underwear = 0`, `Base = 100`, `Mid = 200`, `Outer = 300`, `Accessory = 400`. Os valores são
espaçados para permitir novas camadas sem renumerar. A camada também desempata conflitos de profundidade entre
superfícies quase coincidentes: a camada mais externa vence.

**`BodyRegion`** (flags): `Scalp`, `Face`, `Neck`, `Chest`, `Abdomen`, `Pelvis`, `UpperArmLeft/Right`,
`ForearmLeft/Right`, `HandLeft/Right`, `ThighLeft/Right`, `LowerLegLeft/Right`, `FootLeft/Right`. **O índice do
bit é o ID de região gravado na malha do corpo**, por isso nunca muda.

### Regra de conflito

Duas peças conflitam se e somente se `(slotsA & slotsB) != 0 && camadaA == camadaB`. Equipar uma peça remove
todas as que conflitam com ela.

| Peça A | Peça B | Conflito? |
|--------|--------|-----------|
| Camiseta (`Torso`, `Base`) | Jaqueta (`Torso`, `Outer`) | Não: camadas diferentes |
| Camiseta (`Torso`, `Base`) | Vestido (`Torso \| Legs`, `Base`) | Sim: o vestido substitui a camiseta |
| Calça (`Legs`, `Base`) | Vestido (`Torso \| Legs`, `Base`) | Sim |
| Calça (`Legs`, `Base`) | Cinto (`Waist`, `Accessory`) | Não |
| Tênis (`Feet`, `Base`) | Meias (`Feet`, `Underwear`) | Não |

## 2.6 Variantes de material e perfis de física

**`MaterialVariant`** (`[Serializable]`): `id` (estável, salvo nos presets), `displayName`, `swatchColor` (amostra
na UI) e `materials` (`AssetReferenceT<Material>[]`). A entrada `i` substitui o slot de material `i` do prefab,
contando os slots de todos os renderers em ordem de hierarquia (`GetComponentsInChildren<Renderer>(true)`, depois
`sharedMaterials`). Entrada vazia mantém o material original. O validador de Editor (Fase 2) vai conferir se a
quantidade de entradas cabe nos slots do prefab.

**`PhysicsProfileData`** (abstrato): só expõe `BackendId` (ex.: `"unity.cloth"`). Cada backend define sua
subclasse no próprio assembly, então o Core nunca depende de um pacote de física. Entradas nulas na lista de
perfis são esperadas (módulo não instalado) e puladas.

## 2.7 Identidade (IDs) e validação

**Estratégia de ID (decidida):** o `id` espelha o GUID do asset (`.meta`), sincronizado em `OnValidate` no Editor.

| Situação | Comportamento |
|----------|---------------|
| Asset novo (id vazio) | Recebe o GUID do asset. |
| Asset duplicado (Ctrl+D copia o id do original, que ainda existe) | Recebe o próprio GUID. |
| Id que não corresponde a nenhum asset (`.meta` regenerado, id legado) | É **mantido**, para não quebrar presets já salvos. |

Consequência: **nunca apague arquivos `.meta`.** Como o `OnValidate` só roda quando o asset é carregado ou
editado, um `AssetPostprocessor` (Fase 2) vai reforçar a regra no momento do import.

**Onde a validação roda:**
1. Inspector (via `OnValidate`) e menu *Weld Studio → Validate Catalog* (Fase 2).
2. CI: teste EditMode que carrega todos os `CatalogItemData` e falha se houver erro ou ID duplicado.
3. Runtime: o `CatalogService` descarta entradas inválidas ou IDs duplicados, registrando um aviso.

## 2.8 Formato do preset JSON

**Proposta (schema v1)**, a implementar na Fase 1:

```json
{
  "schemaVersion": 1,
  "createdWith": "0.1.0",
  "rigId": "makehuman.game_engine",
  "metadata": {
    "name": "Meu personagem",
    "createdAt": "2026-10-01T12:00:00Z"
  },
  "equipment": [
    { "itemId": "3f2a9c0d8e7b4a6f9c1d2e3f4a5b6c7d", "variantId": "navy" },
    { "itemId": "9b8a7c6d5e4f40312a1b2c3d4e5f6a7b", "variantId": null }
  ],
  "modifiers": {
    "body.weight": 0.35,
    "face.mouthSmileLeft": 0.8
  }
}
```

Regras:

- Serialização com **Newtonsoft JSON** (`com.unity.nuget.newtonsoft-json`). O `JsonUtility` não suporta
  dicionários e dificulta a evolução do schema.
- `schemaVersion` sobe a cada mudança incompatível. Há uma migração `vN → vN+1` por versão, coberta por teste com
  arquivos-exemplo versionados no repositório.
- **IDs desconhecidos** (mod não instalado) são reportados ao usuário **e preservados** ao salvar de novo. Abrir
  e salvar um preset não pode apagar silenciosamente o que pertence a um pack ausente.
- Variante desconhecida → variante padrão, com aviso.
- Modificador desconhecido → preservado. Valor fora da faixa → limitado à faixa. Modificador ausente → valor
  padrão.
- Gravação atômica: escreve em `<arquivo>.tmp` e troca pelo definitivo.
- Extensão: `.weld.json`, numa pasta de presets do usuário (`Application.persistentDataPath/Presets` por padrão,
  com opção de "Salvar como…" em qualquer lugar).

## 2.9 Convenções de Addressables

**Labels (proposta):**

| Label | Aplicada em |
|-------|-------------|
| `weld.catalog` | Todo `CatalogItemData` (descoberta no boot). Já definida como `CatalogItemData.CatalogLabel`. |
| `weld.modifiers` | Todo `ModifierDefinition`. |
| `weld.body` | `BodyDefinition` do corpo base. |

**Grupos (proposta):**

| Grupo | Conteúdo | Bundle mode | Quando carrega |
|-------|----------|-------------|----------------|
| `Catalog` | SOs de catálogo, perfis de física, categorias, modificadores | Pack Together | Boot |
| `Body` | Corpo base, esqueleto, materiais de pele | Pack Together | Boot |
| `Clothing` | Prefabs de roupa (dependências entram junto) | Pack Separately | Ao equipar |
| `Hair` | Prefabs de cabelo | Pack Separately | Ao equipar |
| `Icons` | Miniaturas | Pack Separately (medir; pode virar atlas por categoria) | Quando visíveis |
| `Shared` | Shaders, texturas e materiais comuns | Pack Together | Sob demanda (evita duplicação) |
| `Animations` | Idles, poses | Pack Together | Boot |

`Pack Separately` dá um bundle por item, e desequipar libera a memória daquele item. A marcação de assets em
grupos e labels será automatizada por uma ferramenta de Editor (Fase 2), para que contribuidores não precisem
configurar Addressables à mão.

## 2.10 Status dos arquivos já escritos

| Arquivo | Status |
|---------|--------|
| `Runtime/Core/WeldStudio.Core.asmdef` | Escrito |
| `Runtime/Core/Domain/EquipmentSlot.cs`, `EquipmentLayer.cs`, `BodyRegion.cs` | Escritos |
| `Runtime/Core/Data/CatalogItemData.cs`, `ClothingItemData.cs`, `MaterialVariant.cs`, `PhysicsProfileData.cs` | Escritos |

Todos compilam sem erros nem avisos contra stubs da API da Unity, nas configurações player e Editor
(`UNITY_EDITOR`). Ainda **não** foram abertos numa Unity real: os arquivos `.meta` serão gerados e versionados na
Fase 0, quando o projeto for criado.
