# 3. Pipeline de conteúdo

> Status: **Proposta**. Cada etapa será validada na prática na Fase 3 com um corpo e três roupas de teste, e o
> resultado vira um guia passo a passo para contribuidores.

Fluxo geral:

```
MakeHuman 1.x / MPFB2 (Blender)          Blender (autoria)                         Unity
──────────────────────────────          ─────────────────                         ─────
corpo base (CC0) ───────────────▶  limpeza, shape keys, regiões, ──export FBX──▶  import → prefab → BodyDefinition
roupas/cabelos (.mhclo, CC0/BY) ─▶ rig, conformidade, pesos     ──export FBX──▶  import → prefab → ClothingItemData
                                         ▲                                         │
                                  Tools/blender/*.py                        Editor: validar,
                              (automatiza as etapas repetitivas)            gerar ícone, Addressables
```

Fontes de arte (`.blend`, `.mhclo`, texturas em camadas) ficam em `SourceAssets/`. Só os exports entram em
`Assets/Content/`.

## 3.1 Corpo base

1. **Origem:** malha base do MakeHuman (topologia `hm08`), gerada no MakeHuman 1.x ou no MPFB2 (plugin do
   Blender). Malha, targets e texturas oficiais são CC0.
2. **Limpeza:** remover a geometria auxiliar (*helpers* usados pelo MakeHuman para ajustar roupas). Olhos, dentes
   e língua seguem como malhas separadas no prefab do corpo. Sobrancelhas e cílios entram como cards com alpha,
   inicialmente parte do corpo com variantes de material.
3. **Esqueleto:** *Game engine* do MakeHuman (sem ossos faciais, adequado ao Humanoid da Unity). Questão em
   aberto Q2: acrescentar ossos de jiggle (seios, glúteos, barriga), o que define o rig oficial do projeto.
4. **Escala e orientação:** exportar em **metros** (o MakeHuman trabalha em decímetros por padrão), Y para cima,
   personagem olhando para +Z na Unity.
5. **Import na Unity:** Rig = **Humanoid** (Avatar criado a partir deste modelo), BlendShapes importados,
   BlendShape Normals = Import. O mapeamento do Avatar é conferido manualmente uma vez e versionado.

## 3.2 Blendshapes

**Corpo:** o MakeHuman tem centenas de targets, e exportar todos custaria memória demais. Vamos curar um
subconjunto:

- *Macros:* gênero, idade, massa muscular, peso, altura, proporções. No MakeHuman, cada macro mistura vários
  targets com pesos interpolados. Exportamos os targets componentes como blendshapes e o `MacroModifier` recalcula
  os pesos em runtime. Essa lógica será **reimplementada a partir do comportamento observado, sem portar código
  AGPL** (ver risco R1).
- *Regionais:* nariz, orelhas, queixo, mandíbula, olhos, boca, torso, membros. A lista final será definida na
  Fase 5, com orçamento de memória medido.

**Rosto (expressões):** no MakeHuman, expressões são poses de ossos faciais (esqueleto *default*), não
blendshapes. Etapa de "assar":

1. Aplicar cada pose facial no esqueleto *default*, no Blender.
2. Salvar o resultado como shape key.
3. Exportar com o esqueleto *Game engine*.

**Proposta de nomenclatura:** os 52 blendshapes do padrão **ARKit** (`eyeBlinkLeft`, `jawOpen`,
`mouthSmileLeft`, ...). Isso abre caminho para captura facial no futuro. Os que o MakeHuman não cobrir serão
esculpidos à mão.

**Convenção de nomes:** prefixo por domínio (`body.`, `face.`) no ID do modificador. O nome do blendshape na malha
é o nome sem prefixo e precisa ser idêntico no corpo e nas roupas, porque a sincronização é por nome.

## 3.3 Regiões do corpo

Cada vértice do corpo recebe o índice da sua região (`BodyRegion`; o índice do bit é o ID).

1. No Blender, uma vertex group por região (`region.Chest`, `region.ThighLeft`, ...), cobrindo o corpo inteiro
   sem sobreposição.
2. O exportador (`Tools/blender/`) grava o índice num canal de UV dedicado. **Proposta:** UV3.x; confirmar na
   Fase 3 que o canal não conflita com lightmaps ou detalhes.
3. O shader de pele lê o índice, testa o bit na máscara de regiões ocultas (enviada pelo `CharacterRig`) e
   descarta o fragmento. A máscara de textura opcional de cada peça é composta numa única textura (max) no espaço
   UV do corpo.

Fronteiras de região devem cair em lugares que as roupas típicas cobrem (cintura, meio do braço etc.). Onde não
cair, a peça usa a máscara de textura.

## 3.4 Roupas

1. **Origem:** roupas do MakeHuman (`.mhclo` + `.obj`), que podem ser CC0 ou CC-BY (verificar cada uma), ou
   modeladas no Blender sobre o corpo base em forma neutra.
2. **Rig:** mesmo esqueleto do corpo. Pesos transferidos do corpo (*Data Transfer*) e corrigidos à mão onde
   preciso. Ossos extras (saia, capa) ficam sob o osso do corpo correspondente.
3. **Conformidade com os morphs do corpo:** para cada blendshape de corpo exportado, a roupa recebe uma shape key
   de **mesmo nome** que a acompanha. Automação proposta: *Surface Deform* ligado ao corpo neutro; para cada shape
   key do corpo, ativá-la e salvar o resultado como shape key da roupa (script em `Tools/blender/`). Sem isso, a
   pele atravessa a roupa quando o usuário muda o peso do corpo.
4. **Camadas:** peças de camadas externas são modeladas com folga sobre as internas (ex.: jaqueta sobre camiseta).
5. **Oclusão:** escolher as `hiddenBodyRegions`. Se a borda da peça não coincidir com uma fronteira de região,
   pintar uma máscara no layout UV do corpo.
6. **Pesos de simulação** (peças com física): pintar a cor de vértice, canal R (0 = presa à animação, 1 = livre).
   Os backends convertem isso para coeficientes do Unity Cloth ou parâmetros do Magica.
7. **Export FBX:** escala em metros, *Apply Transform*, só ossos de deformação, shape keys incluídas, *Add Leaf
   Bones* desligado.
8. **Na Unity:**
   1. Import com Rig = Generic, sem Avatar. O esqueleto é descartado em runtime após o remapeamento.
   2. Criar o prefab.
   3. Criar o `ClothingItemData` e preencher slots, camada, regiões, variantes, perfis e atribuição.
   4. Gerar o ícone com a ferramenta de Editor (Fase 2: render automático com câmera e luz padronizadas).
   5. Rodar *Validate Catalog* e commitar (os binários vão para o LFS automaticamente).

## 3.5 Cabelos

- **Hair cards:** planos com textura de mechas (alpha). O shader usa alpha clip no passe opaco, bordas suaves e
  especular anisotrópico.
- **Movimento:** cadeias de ossos nas mechas principais, skinnadas à cabeça e às cadeias. Em runtime, o
  `SpringBoneBackend` (ou o Magica, se instalado) simula as cadeias. Os ossos extras são reparentados sob o osso
  da cabeça do personagem.
- As propostas de cabelo do MakeHuman (malhas com alpha) servem como ponto de partida.
- O cabelo usa o slot `Hair`; barba e bigode usam `FacialHair`.

## 3.6 Texturas e materiais

- Materiais URP. Shaders próprios para pele, cabelo e olhos (Fase 8). Roupas usam URP Lit até a Fase 8.
- Nomes: `T_<Item>_BaseColor`, `T_<Item>_Normal`, `T_<Item>_Mask` (canais definidos pelo shader do projeto);
  materiais `M_<Item>_<Variante>`.
- Resolução padrão de 2048 para roupas e 4096 para o corpo. Compressão definida por preset de importação
  (*Preset Manager*) aplicado automaticamente por pasta.

## 3.7 Licenças e atribuição

| Licença do asset | Aceita? |
|------------------|---------|
| CC0 | Sim |
| CC-BY 4.0 | Sim, com `author`, `license` e `sourceUrl` preenchidos (aparece na tela de créditos) |
| CC-BY-SA | Caso a caso: a peça e seus derivados continuam CC-BY-SA e isso precisa ficar marcado |
| NC (não comercial) / ND (sem derivados) | Não: incompatível com o uso livre do app e de seus exports |
| Desconhecida | Não |

O código do repositório é MIT. O **código** do MakeHuman (AGPL) e do MPFB (GPL) nunca é copiado nem portado.
Usamos apenas os **assets** (CC0) e os formatos de arquivo.

## 3.8 Git LFS

Tudo o que é binário vai para o LFS pelo `.gitattributes` (padrões sem diferenciar maiúsculas): `.fbx`, `.obj`, `.blend`, `.png`, `.jpg`,
`.tga`, `.psd`, `.exr`, `.tif`, `.spp`, `.sbsar`, áudio, vídeo, fontes, `.dll` etc. YAML da Unity (`.prefab`,
`.asset`, `.mat`, `.unity`, `.meta`) continua como texto, com Smart Merge. O CI (`Tools/ci/check_repo.py`)
recusa binários commitados sem LFS. A cota de LFS do GitHub precisa ser acompanhada (risco R6).
