# Weld Studio

**Criador de personagens 3D modular e open-source: um sucessor espiritual do Adobe Fuse.**

Monte um personagem a partir de um corpo humano base, vista roupas, troque cabelos, ajuste corpo e rosto com
sliders e veja tudo reagir com física, em tempo real. Feito em Unity 6.3 LTS (URP), com modelos base do projeto
[MakeHuman](http://www.makehumancommunity.org/) (CC0).

> **Status: planejamento / Fase 0.** O projeto Unity ainda não foi criado. A arquitetura, o roadmap e o contrato
> de dados estão definidos em [`docs/`](docs/README.md).

---

## Funcionalidades planejadas

- **Modularidade total.** Corpo, roupas e cabelos compartilham o mesmo esqueleto Humanoid. As peças se prendem ao
  corpo em tempo real por remapeamento de ossos do `SkinnedMeshRenderer`.
- **Corpo e rosto paramétricos.** Sliders de blendshapes para forma do corpo (peso, idade, proporções) e
  expressões faciais. As roupas acompanham as mudanças do corpo.
- **Física.** Saias e capas com simulação de tecido que não atravessa o corpo; mechas de cabelo e partes do corpo
  com inércia. Unity Cloth e spring bones próprios por padrão, Magica Cloth 2 como módulo opcional.
- **Animação e IK.** Idle com Animator; olhar para a câmera e alinhamento dos pés com Animation Rigging.
- **Renderização dedicada.** Pele com subsurface scattering e cabelo em hair cards com brilho anisotrópico, no URP.
- **Presets.** Salve e carregue personagens em JSON, com undo/redo em toda edição.
- **Extensível.** Roupas e cabelos novos entram como dados, sem código. Novos comportamentos entram por interfaces
  (`IEquipable`, `ICharacterModifier`, `IPhysicsBackend`) e módulos.
- **Export** (v1.0). Personagem montado em glTF/GLB para usar em outras engines e ferramentas.

## Stack

| Área | Tecnologia |
|------|------------|
| Engine | Unity 6.3 LTS |
| Renderização | URP, com shaders próprios para pele (SSS) e cabelo |
| Interface | UI Toolkit (padrão MVP) |
| Arquitetura | Injeção de dependência com VContainer, assemblies por camada |
| Conteúdo | Addressables (a pasta `Resources/` não é usada) + ScriptableObjects |
| Animação | Animator + Animation Rigging |
| Física | Unity Cloth, spring bones próprios, Magica Cloth 2 (opcional) |
| Dados | JSON (Newtonsoft) assíncrono e versionado |
| Versionamento | Git + Git LFS (obrigatório) |

## Arquitetura em resumo

```
UI (Views + Presenters) ──comandos──▶ CharacterModel (domínio, C# puro) ──eventos──▶ CharacterAssembler
                                                                                      ├─ IAssetProvider (Addressables)
                                                                                      ├─ ICharacterRig (esqueleto, blendshapes)
                                                                                      └─ IPhysicsBackend (Cloth / Magica)
```

A interface e o código 3D não se conhecem: os dois falam só com o modelo do personagem. Detalhes em
[docs/01-arquitetura.md](docs/01-arquitetura.md).

## Documentação

| Documento | Conteúdo |
|-----------|----------|
| [Arquitetura](docs/01-arquitetura.md) | Estrutura de pastas, assemblies, padrão MVP + DI, classes fundamentais, fluxos |
| [Contrato de dados](docs/02-contrato-de-dados.md) | `ClothingItemData` e demais ScriptableObjects, IDs, Addressables, formato de preset |
| [Pipeline de conteúdo](docs/03-pipeline-de-conteudo.md) | MakeHuman → Blender → Unity: corpo, roupas, cabelos, blendshapes |
| [Roadmap](docs/04-roadmap.md) | Fases, entregas e critérios de "pronto" |
| [Decisões e riscos](docs/05-decisoes-e-riscos.md) | Por que cada escolha foi feita, riscos e questões em aberto |
| [Setup](docs/06-setup.md) | Requisitos, clone com LFS, criação do projeto, pacotes |

## Roadmap resumido

| Marco | Entrega |
|-------|---------|
| **v0.1 – Tech demo** | Fundação, domínio, Addressables, personagem MakeHuman trocando roupas em tempo real |
| **v0.5 – Editor utilizável** | UI completa, sliders de corpo e rosto, variantes de cor, presets, undo/redo, idle com IK |
| **v1.0 – Sucessor do Fuse** | Física, shaders de pele e cabelo, export glTF, suporte a mods |

## Começando

Requisitos: **Unity 6.3 LTS**, **Git LFS** e, para produzir conteúdo, Blender + MPFB2.

```bash
git lfs install
git clone https://github.com/PedroCavalcanti-coder/Weld-Studio.git
```

O passo a passo completo, incluindo a criação do projeto Unity na Fase 0, está em
[docs/06-setup.md](docs/06-setup.md).

## Estrutura do repositório

```
Assets/WeldStudio/   código do aplicativo (Runtime, Editor, Tests, Shaders, Settings, Scenes)
Assets/Content/      conteúdo Addressable: corpo, roupas, cabelos, animações (content packs)
Assets/Modules/      extensões opcionais (ex.: ponte com o Magica Cloth 2)
SourceAssets/        fontes de arte (.blend, MakeHuman) em LFS, fora da Unity
Tools/               scripts de Blender e de CI
docs/                planejamento e decisões de arquitetura
```

## Contribuindo

O projeto está na fase de fundação. Um guia de contribuição (`CONTRIBUTING.md`) faz parte da Fase 0. Até lá,
issues com ideias, dúvidas e críticas à arquitetura são bem-vindas. Comece pelo [roadmap](docs/04-roadmap.md) e
pelas [questões em aberto](docs/05-decisoes-e-riscos.md#53-questões-em-aberto).

Regras que valem desde já:

- Código, identificadores e comentários em inglês.
- Nunca usar a pasta `Resources/`; conteúdo só via Addressables.
- Todo binário vai para o Git LFS.
- Todo asset de terceiros precisa de licença compatível e atribuição (ver
  [política de licenças](docs/03-pipeline-de-conteudo.md#37-licenças-e-atribuição)).

## Licenças

- **Código:** [MIT](LICENSE).
- **Modelos base do MakeHuman:** CC0. O projeto usa apenas os *assets* do MakeHuman, nunca o seu código (AGPL).
- **Conteúdo da comunidade:** cada item declara autor, licença e origem; os créditos aparecem no aplicativo.
- **Magica Cloth 2** é um produto comercial de terceiros. **Não** faz parte deste repositório e não é necessário
  para usar o Weld Studio.

## Créditos

- [MakeHuman](http://www.makehumancommunity.org/): malha base, targets e esqueleto.
- Inspirado no Adobe Fuse, descontinuado pela Adobe.
