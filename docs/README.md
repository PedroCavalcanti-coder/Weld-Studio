# Documentação do Weld Studio

Planejamento técnico do projeto. Leia na ordem:

| # | Documento | Conteúdo |
|---|-----------|----------|
| 1 | [Arquitetura](01-arquitetura.md) | Estrutura do repositório, assemblies, padrão MVP + DI, classes fundamentais, fluxos principais |
| 2 | [Contrato de dados](02-contrato-de-dados.md) | ScriptableObjects do catálogo (`ClothingItemData`), IDs, convenções de Addressables, formato do preset JSON |
| 3 | [Pipeline de conteúdo](03-pipeline-de-conteudo.md) | MakeHuman/MPFB2 → Blender → Unity: corpo, roupas, cabelos, blendshapes, física, licenças |
| 4 | [Roadmap](04-roadmap.md) | Fases de desenvolvimento, entregas e critérios de "pronto" |
| 5 | [Decisões e riscos](05-decisoes-e-riscos.md) | Registro de decisões, riscos técnicos e questões em aberto |
| 6 | [Setup](06-setup.md) | Como preparar a máquina e criar/abrir o projeto Unity |
| – | [ADRs](adr/README.md) | Registro completo de cada decisão de arquitetura |

## Convenções destes documentos

- **Decidido**: vale até ser substituído por uma ADR em `docs/adr/`.
- **Proposta**: direção recomendada, a confirmar quando a fase correspondente começar.
- **Em aberto**: precisa de decisão antes da fase indicada.

O código e os comentários do projeto são escritos em inglês (alcance internacional da comunidade open-source).
A documentação de planejamento está em português por enquanto.
