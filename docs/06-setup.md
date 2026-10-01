# 6. Setup

> Status: o código, os `.meta`, o `.gitattributes` e o CI já estão no repositório. O projeto Unity
> (`ProjectSettings/`, `Packages/`) **ainda não foi criado**: a seção 6.3 descreve como criá-lo uma única vez.
> Depois disso, contribuidores só seguem 6.1, 6.2, 6.4 e 6.6.

## 6.1 Requisitos

| Ferramenta | Versão | Para quê |
|------------|--------|----------|
| Unity Hub + **Unity 6.3 LTS** | a versão exata fica em `ProjectSettings/ProjectVersion.txt` | Editor |
| Git | recente | Versionamento |
| **Git LFS** | 3.x | Obrigatório: todo binário do projeto está no LFS |
| Python | 3.8+ | Verificador de higiene do repositório (`Tools/ci/check_repo.py`) |
| .NET SDK | 8.0 | Opcional: testes de domínio sem abrir a Unity (`dotnet test Tools/ci/DomainTests`) |
| IDE C# | Rider, Visual Studio ou VS Code (com extensão Unity) | Código |
| Blender + MPFB2 | Blender LTS atual | Apenas para quem produz conteúdo |
| Magica Cloth 2 | Asset Store | Opcional e pago: só para quem trabalha no módulo de física avançada |

## 6.2 Clonar

```bash
git lfs install                 # uma vez por máquina
git clone https://github.com/PedroCavalcanti-coder/Weld-Studio.git
cd Weld-Studio
git lfs pull                    # garante que os binários vieram
```

Se uma textura ou FBX aparecer na Unity como arquivo de texto pequeno ou corrompido, o LFS não baixou: rode
`git lfs pull`.

## 6.3 Criar o projeto Unity (uma única vez)

A Unity Hub não cria projeto em pasta que já tem arquivos. Além disso, o código existente referencia Addressables e
Newtonsoft: abrir o repositório antes de instalar esses pacotes dá erros de compilação. Por isso:

1. Na Unity Hub, crie um projeto **Unity 6.3 LTS** com o template **Universal 3D** numa pasta temporária.
2. **Nesse projeto temporário**, instale os pacotes da Fase 0 listados em 6.5 (Addressables, Newtonsoft JSON, Test
   Framework, VContainer).
3. Feche a Unity e copie para a raiz do repositório: `ProjectSettings/`, `Packages/` e `Assets/Settings/` (com
   os `.meta`, que preservam as referências do URP). Não copie nada além disso.
4. Abra a raiz do repositório na Unity Hub (*Add → Add project from disk*). O console deve ficar sem erros.
5. Dentro da Unity:
   1. mova `Assets/Settings` para `Assets/WeldStudio/Settings` (movendo pelo Editor, os GUIDs são preservados);
   2. crie `Assets/WeldStudio/Scenes/Boot.unity`;
   3. em *Window → General → Test Runner → EditMode*, rode os testes: os 39 devem passar.
6. Confira em *Project Settings*:
   - **Editor → Asset Serialization: Force Text**
   - **Version Control → Mode: Visible Meta Files**
   - **Player → Other Settings → Color Space: Linear**
   - **Graphics / Quality:** URP Asset de desktop atribuído. Os assets `Mobile_*` do template podem ser removidos
     depois de ajustar os níveis de qualidade.
7. Rode `python3 Tools/ci/check_repo.py`. Ele não deve acusar nenhum `.meta` novo para os arquivos que já
   existiam; se acusar, a Unity regenerou um GUID e isso precisa ser investigado antes do commit.
8. Commite `ProjectSettings/`, `Packages/manifest.json`, `Packages/packages-lock.json`, os settings e a cena.

## 6.4 Smart Merge (recomendado)

O `.gitattributes` (Fase 0) marca o YAML da Unity com `merge=unityyamlmerge`. Sem a configuração abaixo, o Git usa
o merge de texto comum, o que funciona mas gera mais conflitos. Para habilitar:

```bash
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver "'<CAMINHO_DO_UnityYAMLMerge>' merge -p %O %B %A %A"
git config --global merge.unityyamlmerge.recursive binary
```

Onde fica o `UnityYAMLMerge` (troque `<versão>` pela versão instalada):

| SO | Caminho |
|----|---------|
| Windows | `C:\Program Files\Unity\Hub\Editor\<versão>\Editor\Data\Tools\UnityYAMLMerge.exe` |
| macOS | `/Applications/Unity/Hub/Editor/<versão>/Unity.app/Contents/Tools/UnityYAMLMerge` |
| Linux | `~/Unity/Hub/Editor/<versão>/Editor/Data/Tools/UnityYAMLMerge` |

## 6.5 Pacotes

As versões são as compatíveis com a Unity 6.3 sugeridas pelo Package Manager, travadas no `packages-lock.json`.

| Pacote | Fase | Uso |
|--------|------|-----|
| `com.unity.render-pipelines.universal` | 0 | URP (vem com o template) |
| `com.unity.addressables` | 0 | Conteúdo sob demanda |
| `com.unity.nuget.newtonsoft-json` | 0 | Presets JSON |
| `com.unity.inputsystem` | 0 | Câmera e atalhos (vem com o template) |
| `com.unity.test-framework` | 0 | Testes EditMode e PlayMode |
| `jp.hadashikick.vcontainer` | 0 | Injeção de dependência (via OpenUPM, `openupm add jp.hadashikick.vcontainer`, ou *Add package from git URL* com `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#<tag>`, sempre fixando uma tag) |
| `com.unity.animation.rigging` | 6 | IK |
| `com.unity.memoryprofiler` | 2 | Verificação de vazamentos (desenvolvimento) |
| `com.unity.test-framework.performance` | 2 | Testes de performance |
| `com.unity.cloud.gltfast` | 9 | Export glTF/GLB (a avaliar) |
| `com.unity.localization` | 9 | Tradução da UI |

O UI Toolkit já vem embutido na Unity 6 e não precisa de pacote.

**Magica Cloth 2 (opcional):** importe pela Asset Store. Ele fica em `Assets/MagicaCloth2/`, que está no
`.gitignore`. A partir da Fase 7, um script de Editor detecta o plugin e ativa o define `WELD_MAGICACLOTH2`,
habilitando o módulo `Assets/Modules/MagicaClothBridge/`.

## 6.6 Verificações locais e CI

Antes de abrir um PR:

```bash
python3 Tools/ci/check_repo.py          # .meta faltando/órfão, binário fora do LFS, Resources/, assets pagos
dotnet test Tools/ci/DomainTests        # testes de Core e Persistence sem Unity
```

O workflow `.github/workflows/ci.yml` roda três jobs em todo PR:

| Job | O que faz | Precisa de |
|-----|-----------|------------|
| Repository hygiene | `Tools/ci/check_repo.py` | nada |
| Domain tests | `dotnet test Tools/ci/DomainTests` (.NET 8, C# 9, warnings como erro) | nada |
| Unity tests | GameCI `unity-test-runner`, EditMode + PlayMode, com LFS e cache de `Library/` | projeto Unity criado (6.3) e secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` |

Enquanto o projeto Unity ou os secrets não existirem, o job da Unity termina com sucesso e deixa um aviso dizendo
o que falta. Para gerar o `UNITY_LICENSE` de uma licença Personal, siga a documentação de ativação do GameCI.
