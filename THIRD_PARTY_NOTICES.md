# Avisos de terceiros

O código do Weld Studio é licenciado sob a [licença MIT](LICENSE). Este arquivo lista os componentes e assets
de terceiros usados pelo projeto e suas licenças. Mantenha-o atualizado no mesmo PR que adicionar uma
dependência ou um asset de terceiros.

## Assets

| Componente | Uso | Licença | Origem |
|------------|-----|---------|--------|
| MakeHuman: malha base, targets, esqueletos, texturas oficiais | Corpo base e blendshapes | CC0 1.0 | http://www.makehumancommunity.org/ |

O **código** do MakeHuman (AGPL-3.0) e do MPFB (GPL-3.0) **não** é usado nem incluído neste repositório.

Itens de conteúdo da comunidade declaram autor, licença e origem no próprio asset (`CatalogItemData`). Os
créditos aparecem na tela de créditos do aplicativo.

## Pacotes de código

| Pacote | Uso | Licença | Origem |
|--------|-----|---------|--------|
| VContainer | Injeção de dependência | MIT | https://github.com/hadashiA/VContainer |
| Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`) | Serialização de presets | MIT | https://www.newtonsoft.com/json |
| NUnit (via Unity Test Framework e CI) | Testes | MIT | https://nunit.org/ |

Os pacotes oficiais da Unity (URP, Addressables, Animation Rigging, Input System, Test Framework) são
distribuídos pela Unity sob os termos da Unity Companion License e não são redistribuídos neste repositório.

## Não incluído

| Componente | Observação |
|------------|------------|
| Magica Cloth 2 | Asset comercial da Unity Asset Store. Não faz parte do repositório; o módulo `Assets/Modules/MagicaClothBridge` é opcional e só compila quando o asset é instalado localmente. |
