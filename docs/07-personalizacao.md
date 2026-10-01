# 7. Personalização: corpo, pintura, cabelo, roupas e animações

> Status: **domínio implementado e testado** (modelo, comandos, dados, presets). As partes que dependem da Unity
> (rig, shaders, pintura na GPU, UI, prévia de animação) estão **especificadas aqui** e entram nas fases do
> [roadmap](04-roadmap.md).

Este documento cobre o que o usuário pode fazer com o personagem:

1. selecionar uma parte do corpo e movê-la, esticá-la, engrossá-la ou diminuí-la;
2. pintar direto no modelo;
3. testar o personagem com animações de exemplo;
4. escolher o cabelo e mudar cor (raiz, pontas e mechas), comprimento e volume;
5. mudar as cores das roupas, parte por parte.

Antes disso, a seção 7.0 explica como avaliar modelos encontrados na internet.

---

## 7.0 Avaliando corpos, cabelos e roupas encontrados (FBX)

O sistema modular só funciona quando **todas as peças compartilham o mesmo esqueleto e foram ajustadas ao mesmo
corpo base**. Um FBX solto pode ser ótimo como referência e mesmo assim não servir como conteúdo. Antes de trazer
qualquer arquivo para o repositório, responda:

| # | Pergunta | Por que importa |
|---|----------|-----------------|
| 1 | **De onde veio e qual é a licença?** (link da página original) | Só aceitamos CC0 e CC-BY ([política](03-pipeline-de-conteudo.md#37-licenças-e-atribuição)). Sem licença clara, o arquivo não entra. |
| 2 | Foi gerado por qual programa? MakeHuman/MPFB, Adobe Fuse/Mixamo, Daz, Character Creator, VRoid, marketplace...? | Cada origem tem esqueleto, topologia e licença próprios. **Conteúdo do Adobe Fuse e do Mixamo pertence à Adobe e não pode ser redistribuído num projeto open-source.** O mesmo vale para Daz e Character Creator. |
| 3 | Qual esqueleto? Nomes dos ossos (ex.: `upperarm_l`, `mixamorig:LeftArm`) | Roupas e cabelos precisam usar o esqueleto do corpo base. Esqueletos diferentes exigem re-rig no Blender. |
| 4 | A roupa foi modelada sobre qual corpo? | Uma roupa ajustada a outro corpo vai atravessar a pele ou flutuar no nosso. Ela precisa ser re-ajustada ao corpo MakeHuman. |
| 5 | Tem blendshapes? Quais? | Corpo: blendshapes de forma e de expressão. Roupas: precisam dos blendshapes de conformidade com os mesmos nomes do corpo. |
| 6 | Polígonos, materiais e UVs | O orçamento é de ~20–40 mil triângulos por peça, poucos materiais e UVs sem sobreposição (necessárias para pintar). |
| 7 | Texturas | Resolução e se existem mapas separados (cor, normal, máscara) que permitam recolorir. |

**Como usar o que não puder entrar como conteúdo:**

- **Referência visual:** modelar ou re-ajustar uma peça nossa inspirada nele.
- **Testes locais:** em `SourceAssets/_local/` (no `.gitignore`), nunca commitado.
- **Mods do próprio usuário:** depois da Fase 9, cada usuário poderá carregar conteúdo próprio sem que ele faça
  parte do repositório.

---

## 7.1 Edição do corpo por partes

### O que o usuário faz

1. **Seleciona uma parte:** clica nela no modelo (a região se destaca) ou escolhe numa lista (cabeça, nariz,
   braço esquerdo...). A câmera enquadra a parte.
2. **Ajusta com sliders agrupados:** forma, tamanho, posição. Exemplos: comprimento do braço, espessura da coxa,
   largura do nariz, altura dos ombros, tamanho da cabeça.
3. **Ou arrasta direto no modelo:** um gizmo sobre a parte selecionada (setas para mover, alças para
   esticar/engrossar), que converte o arraste em valores dos mesmos sliders.
4. **Simetria** (ligada por padrão): editar o braço esquerdo edita o direito junto, num único passo de undo.
5. **Reset** por slider, por parte ou do corpo inteiro.

### Como funciona

| Peça | Papel |
|------|-------|
| `BodyPartData` (SO) | Uma parte selecionável: regiões do corpo (para clique e destaque), osso de foco (câmera), lista de modificadores e a parte espelhada. |
| `ModifierDefinition` (SO, implementa `ICharacterModifier`) | Um slider definido em dados, com ID estável usado nos presets (`body.upperArm.l.length`). |
| `BlendShapeModifierDefinition` | Slider que dirige blendshapes: positivo → uma forma, negativo → outra (nariz mais largo/estreito). |
| `BoneTransformModifierDefinition` | Slider que mexe em ossos: **Length** (estica no eixo do osso), **Thickness** (engrossa nos eixos perpendiculares), **UniformScale** (aumenta/diminui), **Offset** (move). |
| `BoneAdjustmentStack` | Combina as contribuições de vários sliders no mesmo osso (comprimento e espessura do braço juntos), sem que um sobrescreva o outro. |
| `SetModifierCommand` com vários IDs | Simetria: o mesmo valor nos dois lados, arraste inteiro = um passo de undo. |

As roupas e os cabelos acompanham automaticamente: ossos mexidos movem tudo que está skinnado a eles, e
blendshapes do corpo são copiados para os blendshapes de mesmo nome das peças (`conformToBodyShape`).

**Cuidado técnico (Fase 3):** a Unity não compensa escala não uniforme entre ossos pai e filho. Engrossar um
braço sem compensação deformaria antebraço e mão. Por isso o `CharacterRig` aplica a escala inversa nos filhos
diretos. Nas cadeias de membros, onde o filho segue o eixo do pai, a compensação é exata. Onde não segue (por
exemplo, clavícula → ombro), usamos **Offset** em vez de escala. Os limites de cada slider (`Min`/`Max`) são
definidos por quem cria o conteúdo, para manter proporções plausíveis; o validador recusa escalas que zeram a
malha.

**Seleção por clique:** cada vértice do corpo carrega o índice da sua região
([pipeline §3.3](03-pipeline-de-conteudo.md#33-regiões-do-corpo)). Um passe de picking na GPU (renderiza região
e UV do pixel sob o cursor e lê um pixel com `AsyncGPUReadback`) devolve a região clicada, sem colisores de
malha.

---

## 7.2 Pintura direto no modelo

### O que o usuário faz

- Escolhe onde pintar: o **corpo** ou uma **peça** vestida.
- Pinta em **camadas** (tatuagem, maquiagem, sardas, sujeira, logo), cada uma com nome, opacidade, modo de mistura
  (Normal, Multiplicar, Tela, Sobrepor) e visibilidade. As camadas podem ser reordenadas.
- Usa as ferramentas pincel e borracha, com cor, tamanho, dureza, opacidade e pressão da caneta. Pode pintar
  espelhado (simetria em X).
- Desfaz e refaz cada traço.

### Como funciona

| Peça | Papel |
|------|-------|
| `PaintLayer` + comandos `Add/Remove/Update/MovePaintLayerCommand` | Metadados das camadas no `CharacterModel` (implementado). |
| `IPaintCanvas` / `IPaintStroke` / `BrushSettings` | Contrato do motor de pintura (implementação na Fase 4+). |
| Pacote `.weld` | O preset salva cada camada como PNG anexo (`paint/<id>.png`). |

**Técnica (motor de pintura, assembly `WeldStudio.Rendering`):**

1. Cada camada é uma `RenderTexture` no **espaço UV** da malha alvo (2048² por padrão).
2. **Ponto de contato:** o mesmo picking da GPU da seção 7.1 devolve a posição no modelo sob o cursor.
3. **Pincelada sem costuras:** a malha é renderizada "desdobrada" (o vertex shader usa a UV como posição de
   tela). Cada texel recebe tinta conforme a distância, **no espaço 3D**, entre a sua posição na superfície e o
   ponto do pincel. Por isso o traço atravessa costuras de UV sem cortes, e o raio do pincel é em metros
   (igual em qualquer zoom).
4. **Dilatação** das bordas das ilhas de UV, para não aparecerem linhas no filtro de textura.
5. **Undo por blocos:** antes de um traço, salvamos só os blocos (tiles de 64²) que ele vai tocar. O
   `ICommand` devolvido por `Complete()` restaura esses blocos.
6. **Composição:** as camadas visíveis são combinadas (com os modos de mistura) numa textura final por alvo,
   amostrada pelo shader da pele ou da roupa (`_PaintTex`).

Requisito de conteúdo: as peças pintáveis precisam de UVs **sem sobreposição** (UV espelhada impede pintar só um
lado).

---

## 7.3 Animações de exemplo

### O que o usuário faz

- Abre o painel de animações por categoria: **Idle**, **Locomoção**, **Gestos**, **Poses** e **Amplitude de
  movimento** (agachar, braços para cima, torção). A última categoria serve para checar se corpo, roupas e cabelo
  deformam bem.
- Usa play, pausa, velocidade e linha do tempo para inspecionar quadro a quadro, mais a mesa giratória para ver
  de todos os lados.
- A física de roupa e cabelo continua ativa, e o olhar para a câmera pode ser ligado ou desligado.

### Como funciona

| Peça | Papel |
|------|-------|
| `AnimationClipData` (SO de catálogo) | Clip Humanoid via Addressables, categoria, loop e atribuição. |
| `IAnimationPreview` | Contrato: tocar, pausar, retomar, buscar, velocidade, parar. Não é salvo no preset. |

**Implementação (Fase 6, `WeldStudio.Animation`):** um `PlayableGraph` próprio com `AnimationMixerPlayable`
(transição suave entre o idle e o clip) e `AnimationClipPlayable` (permite pausar e buscar a qualquer momento).
Não é preciso montar um Animator Controller por clip. Como os clips são Humanoid, qualquer animação compatível
funciona no nosso esqueleto (retargeting). A integração com o Animation Rigging no mesmo grafo precisa ser
validada no início da Fase 6.

**Fontes de animação:**

| Fonte | Situação |
|-------|----------|
| Mixamo | **Não pode** ser redistribuído no repositório. O usuário poderá importar as suas (GLB) localmente (Fase 9). |
| CMU Motion Capture Database | Uso livre, com agradecimento pedido. Formato BVH; precisa de limpeza e retarget. A avaliar. |
| Pacotes CC0 (ex.: Quaternius) | A avaliar, verificando a licença de cada pacote. |
| Animações próprias | Gravadas ou animadas pela comunidade, CC0/CC-BY. |

---

## 7.4 Cabelo: escolha, cores, comprimento e volume

### O que o usuário faz

- Escolhe um cabelo no catálogo (slot `Hair`; barba e bigode em `FacialHair`).
- Muda **até três cores**: **raiz**, **pontas** (com degradê entre elas) e **mechas** (luzes).
- Ajusta: **comprimento** (encurta), **volume**, **altura da raiz** (onde o degradê começa), **suavidade** do
  degradê e **quantidade de mechas**.
- Vê o cabelo reagir à inércia ([física](01-arquitetura.md#110-física)).

### Como funciona

`HairItemData` já vem com as opções padrão ligadas ao shader de cabelo do projeto (implementado):

| Opção | ID | Shader / malha |
|-------|----|----------------|
| Cor da raiz | `root` | `_RootColor` |
| Cor das pontas | `tip` | `_TipColor` |
| Cor das mechas | `streak` | `_StreakColor` |
| Comprimento | `length` | `_Length` (0.2–1): descarta o fio além desse ponto |
| Volume | `volume` | blendshape `hair_volume` (opcional, modelado no Blender) |
| Altura da raiz | `gradientStart` | `_GradientStart` |
| Suavidade do degradê | `gradientSoftness` | `_GradientSoftness` |
| Quantidade de mechas | `streakAmount` | `_StreakAmount` |

**Shader de cabelo (Fase 8; versão mínima na Fase 3):**

- A posição ao longo do fio vem de `UV.y` dos hair cards (0 na raiz, 1 na ponta). A cor é
  `lerp(raiz, ponta, smoothstep(gradientStart, gradientStart + gradientSoftness, v))`.
- **Mechas:** cada card carrega um número aleatório (canal `UV2.x`). Cards com valor abaixo de `_StreakAmount`
  usam a cor da mecha, misturada pelo mesmo degradê. Assim as mechas seguem o desenho do penteado.
- **Comprimento:** recorte por alpha onde `v > _Length`. Só encurta: um penteado mais longo é outro item.
- A textura do cabelo é em tons de cinza (forma e alpha das mechas); a cor vem toda dos parâmetros.

Requisito de conteúdo: hair cards com `UV.y` da raiz à ponta, `UV2.x` aleatório por card e, se houver volume, o
blendshape `hair_volume`.

---

## 7.5 Cores das roupas, parte por parte

### O que o usuário faz

- Seleciona uma peça vestida e vê a lista das suas partes recolorizáveis (corpo da camiseta, gola, punhos,
  estampa; cabedal, sola e cadarço do tênis).
- Escolhe uma cor para cada parte (seletor com paleta e conta-gotas) ou volta ao padrão.
- Troca de **variante** (outro conjunto de texturas: xadrez, jeans, couro) quando a peça tiver variantes. As cores
  das partes continuam valendo sobre a variante.

### Como funciona

| Peça | Papel |
|------|-------|
| `ColorZone` (em `EquipableItemData.colorZones`) | Uma parte recolorizável: ID, nome, cor padrão, propriedade do shader e slot de material. |
| `ItemParameter` | Sliders extras da peça (comprimento da manga via blendshape, escala da estampa...). |
| `ItemAppearance` no `CharacterModel` | As cores e valores escolhidos, por item (implementado, com undo e presets). |

**Convenção do shader de roupa (Fase 3, mínimo; Fase 8, completo):** cada material tem uma **máscara de zonas**
(`_ZoneMask`) cujos canais R, G, B e A marcam até 4 partes. O shader multiplica a textura base (pintada em tons
neutros nas áreas recolorizáveis) pela cor da zona: `_ZoneColor0` a `_ZoneColor3`. Peças com mais de 4 partes
usam mais de um material.

**Sem duplicar materiais:** as cores são aplicadas com `MaterialPropertyBlock` por renderer e por slot de
material (`Renderer.SetPropertyBlock(block, materialIndex)`). Dez personagens com a mesma camiseta em cores
diferentes continuam usando um único material e uma única textura na memória.

---

## 7.6 Texturas

| Tipo | Convenção |
|------|-----------|
| Roupas | `T_<Item>_BaseColor` (neutra nas zonas recolorizáveis), `T_<Item>_Normal`, `T_<Item>_Mask` (metálico/AO/suavidade), `T_<Item>_ZoneMask` (RGBA = zonas). 2048². |
| Cabelo | `T_<Item>_Strands` (tons de cinza + alpha), opcional `T_<Item>_Normal`. 2048². |
| Pele | Base em tons neutros + parâmetros de tom de pele (`SkinToneModifier`, Fase 5), mapas de detalhe (poros), espessura para o SSS. 4096². |
| Pintura do usuário | Camadas no espaço UV, 2048² por alvo, salvas como PNG no `.weld`. |

---

## 7.7 Outras ideias (backlog)

Ideias que reaproveitam a mesma base e entram no roadmap conforme a prioridade:

- **Expressões prontas** (sorriso, raiva, surpresa) como presets de modificadores faciais.
- **Poses** para fotos e pose personalizada com IK (arrastar mãos e pés).
- **Captura de tela** em alta resolução com fundo transparente.
- **Estampas/decals** arrastáveis (projeção) além do pincel livre.
- **Comparar** o antes e depois, ou dois personagens lado a lado (o escopo por personagem já permite).
- **Aleatorizar** o personagem respeitando limites e combinações.
- **Biblioteca de cores** salvas pelo usuário (paletas).
