# Contribuindo com o Weld Studio

Obrigado pelo interesse! Este guia cobre o que você precisa para contribuir com código, conteúdo (roupas,
cabelos) ou documentação.

## Antes de começar

1. Leia o [README](README.md) e a [arquitetura](docs/01-arquitetura.md). As regras abaixo vêm dela.
2. Prepare a máquina seguindo o [setup](docs/06-setup.md). **Git LFS é obrigatório.**
3. Procure uma issue existente ou abra uma descrevendo o que pretende fazer. Para mudanças grandes, alinhe a
   abordagem na issue antes de escrever código.

## Fluxo de trabalho

1. Faça um fork e crie um branch a partir de `main` com um nome descritivo (`feature/catalog-search`,
   `fix/bone-remap-root`, `content/jacket-leather`).
2. Faça commits pequenos, com mensagens no imperativo em inglês (`Add catalog search box`).
3. Rode as verificações locais (abaixo) e abra um pull request preenchendo o template.
4. O CI precisa estar verde. Um mantenedor revisa o código e, quando houver conteúdo, a arte.

## Verificações locais

```bash
python3 Tools/ci/check_repo.py          # .meta, LFS, pastas proibidas
dotnet test Tools/ci/DomainTests        # testes de domínio sem Unity (.NET 8 SDK)
```

Dentro da Unity: *Window → General → Test Runner* → EditMode e PlayMode.

## Regras de código

- C# compatível com a versão da Unity 6 (C# 9). Código, identificadores e comentários em **inglês**.
- Namespaces seguem o assembly: `WeldStudio.Core`, `WeldStudio.Character`, ... Um tipo por arquivo.
- Feature assemblies dependem apenas de `WeldStudio.Core`. Se precisar de algo de outra feature, o contrato vai
  para `Core/Abstractions` ([regra de dependência](docs/01-arquitetura.md#13-assemblies-e-regra-de-dependência)).
- Sem singletons, `FindObjectOfType` ou `GameObject.Find`: dependências chegam por injeção (VContainer).
- Campos serializados são `private` com `[SerializeField]`, expostos por propriedades somente-leitura.
  ScriptableObjects não mudam em runtime.
- Nunca use a pasta `Resources/`. Conteúdo é carregado só pelo `IAssetProvider` (Addressables).
- Membros de enums persistidos (`EquipmentSlot`, `BodyRegion`, ...) nunca são renumerados, só acrescentados.
- Toda mudança no domínio vem com teste EditMode.
- **Sempre commite os arquivos `.meta`** junto com o asset, e nunca apague um `.meta` de asset existente: os IDs
  dos itens e as referências da Unity dependem deles.

## Contribuindo com conteúdo

Siga o [pipeline de conteúdo](docs/03-pipeline-de-conteudo.md). Em resumo:

- Uma pasta por item em `Assets/Content/<Pack>/<Tipo>/<Item>/`.
- O mesmo esqueleto do corpo base; blendshapes de conformidade com os mesmos nomes do corpo.
- `ClothingItemData` preenchido, inclusive **autor, licença (SPDX) e origem**.
- Licenças aceitas: CC0 e CC-BY 4.0. CC-BY-SA é avaliado caso a caso. NC, ND ou licença desconhecida não são
  aceitas ([política](docs/03-pipeline-de-conteudo.md#37-licenças-e-atribuição)).
- Fontes de arte (`.blend` etc.) vão em `SourceAssets/`, e não em `Assets/`.
- **Nunca** copie código do MakeHuman (AGPL) ou do MPFB (GPL). Usamos apenas os assets (CC0) e os formatos de
  arquivo.

## Assets pagos

O Magica Cloth 2 e qualquer outro asset pago ficam fora do repositório (`.gitignore`). Código que depende deles
fica em um módulo de `Assets/Modules/` que compila só quando o asset existe. O app precisa compilar e funcionar
sem eles.

## Decisões de arquitetura

Mudou uma decisão estrutural ou tomou uma nova? Registre uma ADR em [`docs/adr/`](docs/adr/README.md) no mesmo PR.

## Conduta

Este projeto segue o [Código de Conduta](CODE_OF_CONDUCT.md). Ao participar, você concorda em respeitá-lo.

## Licença das contribuições

Contribuições de código são licenciadas sob a [licença MIT](LICENSE) do projeto. Contribuições de conteúdo
mantêm a licença declarada no próprio item.
