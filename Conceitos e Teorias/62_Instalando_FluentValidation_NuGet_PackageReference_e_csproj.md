# Instalando FluentValidation, NuGet, PackageReference e `.csproj`

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Problemas das validações atuais](#problemas-das-validações-atuais)
3. [Por que usar o FluentValidation](#por-que-usar-o-fluentvalidation)
4. [Onde instalar o pacote](#onde-instalar-o-pacote)
5. [Instalação via linha de comando](#instalação-via-linha-de-comando)
6. [Instalação pelo Visual Studio](#instalação-pelo-visual-studio)
7. [Abas do gerenciador de pacotes](#abas-do-gerenciador-de-pacotes)
8. [Pacotes já presentes no projeto de API](#pacotes-já-presentes-no-projeto-de-api)
9. [Gerenciamento de pacotes na Solution](#gerenciamento-de-pacotes-na-solution)
10. [Aba Consolidate](#aba-consolidate)
11. [O arquivo `.csproj`](#o-arquivo-csproj)
12. [ProjectReference e PackageReference](#projectreference-e-packagereference)
13. [Nullable no projeto](#nullable-no-projeto)
14. [Resumo](#resumo)
15. [Visão Geral](#visão-geral)

---

## Contexto da aula

Depois da introdução conceitual ao NuGet, a aula parte para a prática.

O objetivo é instalar um pacote que será utilizado para melhorar as validações do cadastro de despesas:

```text
FluentValidation
```

Além de instalar o pacote, a aula também mostra o que acontece no projeto quando um pacote NuGet é adicionado.

---

## Problemas das validações atuais

No `RegisterExpenseUseCase`, as validações foram implementadas manualmente com vários `if` e com `throw` para lançar exceções.

Esse modelo apresenta dois problemas principais.

### 1. A exceção interrompe a execução

Quando ocorre algo como:

```csharp
throw new ArgumentException("O título é obrigatório");
```

a execução do método é interrompida naquele ponto.

Isso significa que as validações seguintes deixam de ser executadas.

Exemplo conceitual:

```text
validar Title
↓
inválido
↓
throw
↓
fim da validação
```

Assim, se `Title`, `Amount` e `PaymentType` estiverem incorretos ao mesmo tempo, o usuário poderá receber apenas o primeiro erro encontrado.

O objetivo futuro é validar todas as propriedades necessárias e devolver uma coleção com todos os erros encontrados.

---

### 2. Muitos `if` aumentam a classe

Outro problema é deixar toda a lógica de validação diretamente dentro do Use Case.

Exemplo:

```csharp
if (...)
{
    ...
}

if (...)
{
    ...
}

if (...)
{
    ...
}
```

À medida que novas regras aparecem, a classe tende a crescer.

A intenção da aula é manter as classes menores e organizar as validações de maneira mais simples.

> A aula não afirma que `if` seja inadequado para qualquer validação. Para regras complexas ou muito específicas, ainda pode ser necessário utilizá-lo.

---

## Por que usar o FluentValidation

O pacote escolhido é:

```text
FluentValidation
```

Ele será utilizado para substituir parte das validações simples escritas manualmente.

Segundo a aula, ele permite utilizar uma sintaxe mais enxuta para situações como:

- verificar se uma string está vazia;
- validar valores;
- validar datas;
- verificar valores associados a enums;
- organizar regras de validação fora do fluxo principal do Use Case.

A instalação acontece nesta aula; a utilização das regras do FluentValidation será feita posteriormente.

---

## Onde instalar o pacote

Um pacote NuGet deve ser instalado no projeto que realmente precisa utilizá-lo.

Neste caso, as validações pertencem ao projeto:

```text
CashFlow.Application
```

porque é nele que está o `RegisterExpenseUseCase` e onde ficam as regras de negócio apresentadas até o momento.

Logo:

```text
FluentValidation
        ↓
CashFlow.Application
```

Não é necessário instalar o pacote indiscriminadamente em todos os projetos da Solution.

---

## Instalação via linha de comando

Uma forma de instalar um pacote NuGet é pela CLI do .NET.

A aula mostra a ideia de abrir o terminal diretamente na pasta do projeto `Application` e executar um comando semelhante a:

```bash
dotnet add package FluentValidation --version 11.9.0
```

A versão `11.9.0` era a versão utilizada no momento em que a aula foi gravada.

O ponto principal é que o comando deve ser executado no projeto correto.

Fluxo:

```text
CashFlow.Application/
        ↓
abrir terminal
        ↓
dotnet add package FluentValidation
```

A aula também mostra um atalho no Windows Explorer:

```text
clicar na barra de endereço
↓
digitar CMD
↓
Enter
```

Isso abre o terminal já naquele diretório.

---

## Instalação pelo Visual Studio

A segunda forma apresentada é através da interface gráfica do Visual Studio.

No projeto:

```text
CashFlow.Application
```

usa-se:

```text
botão direito
↓
Manage NuGet Packages
```

Depois é necessário selecionar a aba:

```text
Browse
```

Pesquisar por:

```text
FluentValidation
```

Selecionar o pacote e instalar.

A aula chama atenção para um erro comum: procurar um pacote novo enquanto a aba `Installed` está selecionada.

```text
Installed → mostra somente o que já está instalado
Browse    → pesquisa novos pacotes
```

---

## Abas do gerenciador de pacotes

O Visual Studio apresenta diferentes abas para o gerenciamento dos pacotes NuGet.

### Browse

Serve para pesquisar pacotes disponíveis na galeria NuGet.

```text
Browse
↓
pesquisar pacote
↓
selecionar
↓
instalar
```

---

### Installed

Lista os pacotes já instalados no projeto selecionado.

Exemplo:

```text
CashFlow.Application
└── FluentValidation
```

---

### Updates

Mostra pacotes instalados que possuem versões mais recentes disponíveis.

Exemplo conceitual:

```text
versão instalada: 6.4
versão disponível: 6.5
```

O Visual Studio permite atualizar o pacote diretamente por essa aba.

---

## Pacotes já presentes no projeto de API

A aula mostra que nem todos os pacotes precisam ter sido instalados manualmente pelo desenvolvedor.

O projeto criado a partir do template ASP.NET Core Web API já pode possuir dependências NuGet configuradas.

No exemplo da aula, existe um pacote responsável pela geração do Swagger.

Assim:

```text
CashFlow.API
└── pacote utilizado pelo Swagger
```

Esse pacote também aparece nas abas `Installed` e `Updates` do gerenciador NuGet.

---

## Gerenciamento de pacotes na Solution

Além de abrir o gerenciador para um projeto específico, o Visual Studio permite gerenciar NuGet Packages para toda a Solution.

Fluxo:

```text
botão direito na Solution
↓
Manage NuGet Packages for Solution
```

Nesse modo é possível visualizar os pacotes utilizados por vários projetos da solução.

Também é possível escolher em quais projetos um determinado pacote deve ser instalado.

Exemplo:

```text
Pacote X
├── CashFlow.Application
├── CashFlow.Infrastructure
└── CashFlow.API
```

O desenvolvedor seleciona explicitamente os projetos que deverão receber a dependência.

---

## Aba Consolidate

Ao gerenciar NuGet Packages no nível da Solution, pode aparecer a aba:

```text
Consolidate
```

Sua função é ajudar quando o mesmo pacote está instalado em mais de um projeto, mas com versões diferentes.

Exemplo:

```text
CashFlow.Application
└── Pacote X 1.0.1

CashFlow.Infrastructure
└── Pacote X 1.0.2
```

A aba `Consolidate` permite escolher uma versão e alinhar os projetos.

Objetivo:

```text
antes
Application     → 1.0.1
Infrastructure  → 1.0.2

            ↓ Consolidate

depois
Application     → 1.0.2
Infrastructure  → 1.0.2
```

---

## O arquivo `.csproj`

A aula também abre o arquivo do projeto:

```text
CashFlow.Application.csproj
```

Esse arquivo descreve várias configurações do projeto.

Exemplo conceitual:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>...</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Além das propriedades do projeto, nele também aparecem referências para outros projetos e para pacotes NuGet.

---

## ProjectReference e PackageReference

No `.csproj` podem existir tipos diferentes de referência.

### ProjectReference

Representa uma referência entre projetos da própria Solution.

Exemplo conceitual:

```xml
<ProjectReference Include="../CashFlow.Communication/CashFlow.Communication.csproj" />
<ProjectReference Include="../CashFlow.Domain/CashFlow.Domain.csproj" />
<ProjectReference Include="../CashFlow.Exception/CashFlow.Exception.csproj" />
```

Isso corresponde às dependências configuradas anteriormente no Visual Studio.

---

### PackageReference

Representa uma dependência NuGet.

Depois de instalar o FluentValidation, aparece algo conceitualmente semelhante a:

```xml
<PackageReference Include="FluentValidation" Version="11.9.0" />
```

Ou seja, o projeto passa a registrar:

```text
nome do pacote
+
versão utilizada
```

Representação:

```text
CashFlow.Application.csproj
│
├── ProjectReference
│   ├── Communication
│   ├── Domain
│   └── Exception
│
└── PackageReference
    └── FluentValidation
```

---

## Nullable no projeto

Outra propriedade mostrada no `.csproj` é:

```xml
<Nullable>enable</Nullable>
```

Ela está relacionada às verificações de nullable reference types realizadas pelo compilador.

Na prática, é o tipo de configuração associada aos avisos que aparecem quando uma propriedade `string` pode acabar ficando sem valor válido.

A aula comenta que seria possível alterar para:

```xml
<Nullable>disable</Nullable>
```

mas não recomenda desabilitar esse recurso.

---

## Resumo

| Conceito | Função |
|---|---|
| NuGet | Gerenciamento de pacotes da plataforma .NET |
| FluentValidation | Biblioteca escolhida para melhorar as validações |
| `dotnet add package` | Instala pacote via CLI |
| Browse | Pesquisa pacotes disponíveis |
| Installed | Lista pacotes instalados |
| Updates | Mostra atualizações disponíveis |
| Consolidate | Alinha versões do mesmo pacote entre projetos |
| `.csproj` | Arquivo de configuração do projeto |
| `ProjectReference` | Referência para outro projeto da Solution |
| `PackageReference` | Referência para um pacote NuGet |
| `Nullable` | Configuração relacionada a nullable reference types |

### Fluxo da aula

```text
validações com muitos ifs
        ↓
necessidade de melhorar organização
        ↓
escolher FluentValidation
        ↓
instalar em CashFlow.Application
        ↓
NuGet registra a dependência
        ↓
PackageReference aparece no .csproj
        ↓
próxima aula: utilizar FluentValidation
```

---

## Visão Geral

```text
                    CASHFLOW
                       │
             CashFlow.Application
                       │
                       ├── ProjectReference
                       │   ├── Communication
                       │   ├── Domain
                       │   └── Exception
                       │
                       └── PackageReference
                           └── FluentValidation

NuGet Package Manager
│
├── Browse      → procurar novos pacotes
├── Installed   → visualizar instalados
├── Updates     → verificar versões novas
└── Consolidate → alinhar versões entre projetos
```

> **Em resumo:** a aula transforma o conceito de NuGet apresentado anteriormente em prática. O `FluentValidation` é instalado especificamente em `CashFlow.Application`, o Visual Studio é utilizado para mostrar as formas de pesquisar, instalar e atualizar dependências e, por fim, o arquivo `.csproj` revela como o .NET registra referências de projetos (`ProjectReference`) e pacotes externos (`PackageReference`).