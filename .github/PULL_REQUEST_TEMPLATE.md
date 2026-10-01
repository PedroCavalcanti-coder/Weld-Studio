## O que muda

<!-- Descreva a mudança e o motivo. Referencie a issue: "Closes #123". -->

## Tipo

- [ ] Código (feature ou correção)
- [ ] Conteúdo (roupa, cabelo, animação...)
- [ ] Documentação
- [ ] CI / ferramentas

## Como foi testado

<!-- Testes adicionados, passos manuais na Unity, prints ou vídeos para mudanças visuais. -->

## Checklist

- [ ] `python3 Tools/ci/check_repo.py` passa (arquivos `.meta` commitados, binários no LFS)
- [ ] Testes EditMode/PlayMode passam (e `dotnet test Tools/ci/DomainTests` quando o PR toca `Core` ou `Persistence`)
- [ ] Nenhuma pasta `Resources/`, nenhum singleton ou `GameObject.Find` novo
- [ ] Assemblies de feature continuam dependendo só de `WeldStudio.Core`
- [ ] Decisão de arquitetura nova ou alterada registrada em `docs/adr/`
- [ ] **Conteúdo:** autor, licença (SPDX) e origem preenchidos; licença aceita pela [política](../docs/03-pipeline-de-conteudo.md#37-licenças-e-atribuição); *Validate Catalog* sem erros
- [ ] **Terceiros:** `THIRD_PARTY_NOTICES.md` atualizado; nada de asset pago ou de código AGPL/GPL copiado
