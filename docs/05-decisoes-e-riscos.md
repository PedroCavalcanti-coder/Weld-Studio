# 5. Decisões, riscos e questões em aberto

## 5.1 Registro de decisões

Cada decisão tem uma ADR completa em [`docs/adr/`](adr/README.md) (D1 = ADR-0001, D2 = ADR-0002, ...). Esta
tabela é o resumo.

| # | Decisão | Motivo principal | Alternativa descartada |
|---|---------|------------------|------------------------|
| D1 | Raiz do repo = raiz do projeto Unity | Clonar e abrir; `.gitignore` oficial já assume isso | Projeto Unity em subpasta |
| D2 | MVP + DI com VContainer | UI desacoplada e testável, undo/redo, sem singletons | MVC, MVVM, Zenject, Service Locator ([01 §1.4](01-arquitetura.md#14-padrão-arquitetural-mvp--injeção-de-dependência-vcontainer)) |
| D3 | Assemblies por camada; features dependem só do Core | Fronteiras garantidas pelo compilador, builds incrementais rápidos | Tudo em `Assembly-CSharp` |
| D4 | `Task` nas APIs assíncronas; `Awaitable` para frames e threads | Composição (`Task.WhenAll`), testes em .NET puro, zero dependência extra | Só `Awaitable`; UniTask |
| D5 | Newtonsoft JSON para presets | Dicionários, conversores, evolução de schema | `JsonUtility` |
| D6 | Somente Addressables; `Resources/` proibida | Memória sob demanda, release determinístico, mods | `Resources`, AssetBundles manuais |
| D7 | ID do item = GUID do asset (com preservação de IDs legados) | Único por construção, sobrevive a renomear e mover | ID digitado à mão, nome do arquivo |
| D8 | Prefabs agnósticos de física + perfis com fallback | Backends opcionais sem "missing script", física aplicada após o remapeamento de ossos | Componentes de física dentro do prefab |
| D9 | Unity Cloth + spring bones próprios como base; Magica Cloth 2 opcional | O app completo precisa funcionar só com código aberto | Exigir Magica |
| D10 | Esqueleto *Game engine* do MakeHuman como base | Compatível com Humanoid, mais leve que o *default* | Esqueleto *default* (ossos faciais desnecessários com blendshapes) |
| D11 | Terceiros via UPM; assets pagos nunca commitados | Licenças e tamanho do repo | Copiar plugins para `Assets/` |
| D12 | Código em inglês; docs de planejamento em português | Alcance internacional sem perder clareza agora | Tudo em português |
| D13 | Nome do projeto: **Weld Studio** (namespaces `WeldStudio`, extensão `.weld`, labels `weld.`) | Mesmo nome do repositório | Nome provisório |
| D14 | Testes de domínio também em .NET puro no CI, contra stubs da Unity | Feedback rápido e sem licença Unity | Só testes dentro da Unity |
| D15 | Preset como pacote `.weld` (zip: JSON + PNGs), leitura defensiva | Pinturas viajam no mesmo arquivo; arquivos de terceiros são entrada não confiável | JSON com base64; arquivos soltos |
| D16 | Zonas de cor e parâmetros de itens definidos em dados, aplicados via MaterialPropertyBlock | Conteúdo novo sem código, sem duplicar materiais | Código por item; material por cor |
| D17 | Corpo editado por blendshapes (forma) e ossos (proporção), combinados por fonte | Cobre mover, esticar, engrossar e diminuir sem conflito entre sliders | Só blendshapes; só ossos |
| D18 | Pintura no espaço UV com pincel medido em 3D e undo por blocos | Sem cortes nas costuras, memória de undo pequena | Pintura projetada em textura de tela; snapshot inteiro por traço |
| D19 | Prévia de animações com PlayableGraph próprio | Pausa, busca e velocidade sem Animator Controller por clip | Um estado de Animator por clip |

## 5.2 Riscos técnicos

| # | Risco | Impacto | Mitigação |
|---|-------|---------|-----------|
| R1 | O **código** do MakeHuman é AGPL e o do MPFB é GPL; o repositório é MIT | Alto (jurídico) | Usar só os **assets** (CC0) e formatos de arquivo. Nunca copiar nem portar código, por exemplo o algoritmo de fitting de `.mhclo` ou a mistura de macros: reimplementar a partir do comportamento. Revisar em PRs que mexem em importadores. |
| R2 | Expressões faciais do MakeHuman são poses de ossos, não blendshapes | Médio | Assar as poses em shape keys no Blender ([03 §3.2](03-pipeline-de-conteudo.md#32-blendshapes)); esculpir o que faltar do padrão ARKit. |
| R3 | O URP não tem SSS nativo | Médio | Shader de pele próprio (Fase 8). Até lá, pele com URP Lit. |
| R4 | O Unity Cloth só colide com esferas e cápsulas | Médio | Cápsulas geradas a partir do esqueleto + oclusão do corpo + pesos de simulação por vértice. |
| R5 | O Magica Cloth 2 é pago e não pode estar no repositório | Médio | Módulo opcional com `defineConstraints`, pasta no `.gitignore`, CI compilando sem ele. |
| R6 | Cota e banda do Git LFS no GitHub | Médio | Acompanhar o uso. Fontes pesadas só quando necessárias. No futuro, distribuir conteúdo como bundles remotos ou releases. |
| R7 | Roupas que não acompanham os morphs do corpo atravessam a pele | Alto | Shape keys de conformidade geradas por script + validador que compara nomes de blendshapes corpo × roupa. |
| R8 | Muitos blendshapes custam memória e tempo de skinning | Médio | Subconjunto curado de targets, medição de orçamento na Fase 5. Avaliar shapes esparsos e LOD de blendshapes. |
| R9 | Mapeamento Humanoid do esqueleto MakeHuman (twist bones, dedos, eixo dos ossos) | Médio | Validar o Avatar logo no início da Fase 3, com animações de teste de outras fontes. |
| R10 | Export FBX em runtime não existe pronto (o FBX Exporter da Unity é só Editor) | Médio | glTF/GLB primeiro (glTFast); FBX decidido em ADR na Fase 9. |
| R11 | Condições de corrida em cargas assíncronas (cliques rápidos) | Médio | Tokens de geração no assembler, cancelamento por escopo, testes PlayMode específicos. |
| R12 | Conteúdo da comunidade com qualidade ou licença inadequada | Médio | Validador obrigatório no CI, política de licenças ([03 §3.7](03-pipeline-de-conteudo.md#37-licenças-e-atribuição)), revisão de arte. |
| R13 | Merge de cenas e prefabs entre vários contribuidores | Baixo/Médio | Uma única cena enxuta, conteúdo em prefabs pequenos por item, Smart Merge configurado. |
| R14 | FBX encontrados na internet sem licença clara ou de origem proprietária (Adobe Fuse/Mixamo, Daz, CC) | Alto (jurídico) | Checklist de [07 §7.0](07-personalizacao.md#70-avaliando-corpos-cabelos-e-roupas-encontrados-fbx); sem licença CC0/CC-BY, só como referência ou teste local em `SourceAssets/_local/` (ignorado pelo git). |
| R15 | Escala não uniforme de ossos deforma os filhos (a Unity não compensa) | Médio | Compensação inversa nos filhos alinhados; `Offset` onde não alinham; limites de slider; validar na Fase 3. |
| R16 | Memória das camadas de pintura (~16 MB por camada 2048² RGBA) | Médio | Limite de camadas por alvo, composição em uma textura final, liberação de camadas fora do histórico de undo. |
| R17 | Preset `.weld` malicioso (zip bomb, caminhos `..`) | Médio (segurança) | Nomes de anexo restritos, limite de entradas e de bytes reais descompactados; testes cobrem os casos. |

## 5.3 Questões em aberto

Resolvidas: **Q1** (nome do projeto) → Weld Studio, ver D13.

| # | Questão | Decidir antes de |
|---|---------|------------------|
| Q2 | Rig oficial: *Game engine* puro ou com ossos de jiggle (seios, glúteos, barriga)? Se houver ossos extras, `ClothingItemData.DefaultRigId` passa a ser um ID próprio (ex.: `weld.humanoid.v1`), porque roupas sobre essas regiões precisam de peso nesses ossos. | Primeiro conteúdo definitivo (Fase 3) |
| Q3 | Canal de vértice para o ID de região (proposta: UV3.x) | Fase 3 |
| Q4 | Formatos de export suportados na v1.0 (GLB certo; FBX? VRM?) | Fase 9 |
| Q5 | Sistemas operacionais e GPU mínimos suportados | Fase 8 |
| Q6 | Distribuição de mods: só local, ou repositório/curadoria central? | Fase 9 |
| Q7 | Idiomas da UI na v1.0 (proposta: pt-BR e en) | Fase 4 |
| Q8 | Política de versionamento (SemVer) e de compatibilidade de presets entre versões | Fase 1 |
