# NuGet: cache, global packages folder, DLLs e Package Sources

## 📋 Índice

1. [Visão geral da aula](#1-visão-geral-da-aula)
2. [O pacote NuGet fica dentro do projeto?](#2-o-pacote-nuget-fica-dentro-do-projeto)
3. [PackageReference e gerenciamento de dependências](#3-packagereference-e-gerenciamento-de-dependências)
4. [Onde os pacotes NuGet ficam armazenados](#4-onde-os-pacotes-nuget-ficam-armazenados)
5. [Estrutura da pasta global de pacotes](#5-estrutura-da-pasta-global-de-pacotes)
6. [DLLs e resultado da compilação](#6-dlls-e-resultado-da-compilação)
7. [Limpando o armazenamento do NuGet](#7-limpando-o-armazenamento-do-nuget)
8. [Restauração automática dos pacotes](#8-restauração-automática-dos-pacotes)
9. [Referenciando uma DLL diretamente](#9-referenciando-uma-dll-diretamente)
10. [NuGet x DLL dentro do projeto](#10-nuget-x-dll-dentro-do-projeto)
11. [Package Sources](#11-package-sources)
12. [Repositórios privados de pacotes](#12-repositórios-privados-de-pacotes)
13. [Quando uma empresa pode usar um servidor próprio](#13-quando-uma-empresa-pode-usar-um-servidor-próprio)
14. [Fluxos importantes](#14-fluxos-importantes)
15. [Resumo](#15-resumo)
16. [Visão geral](#16-visão-geral)

---

## 1. Visão geral da aula

A aula aprofunda alguns detalhes mais técnicos sobre **NuGet Packages** e responde principalmente duas perguntas:

1. Quando instalamos um pacote NuGet, todos os arquivos baixados ficam dentro do projeto?
2. É possível utilizar pacotes vindos de outro repositório além do `nuget.org`?

Segundo a aula, as respostas são:

- **não**, os arquivos do pacote não ficam simplesmente copiados para dentro da pasta do projeto;
- **sim**, o Visual Studio pode trabalhar com outras fontes de pacotes além do `nuget.org`.

A partir dessas duas perguntas, a aula apresenta conceitos importantes sobre:

- `PackageReference`;
- armazenamento local dos pacotes;
- DLLs;
- restauração automática de dependências;
- limpeza do cache/armazenamento do NuGet;
- referências diretas a DLLs;
- fontes alternativas de pacotes;
- servidores privados.

---

## 2. O pacote NuGet fica dentro do projeto?

Quando um pacote é instalado, como o **FluentValidation**, o projeto passa a possuir uma referência para ele.

No arquivo `.csproj`, essa relação aparece por meio de um `PackageReference`.

Exemplo conceitual:

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
</ItemGroup>
```

O ponto principal da aula é que essa declaração representa uma **referência ao pacote e à versão utilizada**.

Ela não significa que toda a biblioteca foi copiada fisicamente para dentro da pasta do projeto.

### Representação

```text
CashFlow.Application
│
├── código-fonte
├── .csproj
│   └── PackageReference
│       └── FluentValidation 11.9.0
│
└── dependência localizada externamente
```

No Visual Studio, a mesma dependência também pode ser visualizada por:

```text
Dependencies
└── Packages
    └── FluentValidation 11.9.0
```

Essa área é uma forma de visualizar e gerenciar a referência.

---

## 3. PackageReference e gerenciamento de dependências

A aula mostra diferentes maneiras de visualizar e remover uma referência de pacote.

Por exemplo:

- editar o `.csproj`;
- acessar `Dependencies > Packages`;
- utilizar `Manage NuGet Packages`;
- desinstalar o pacote pela interface do Visual Studio.

A ideia central é que todas essas opções estão lidando com a **dependência declarada no projeto**.

### Exemplo

Se esta linha for removida:

```xml
<PackageReference Include="FluentValidation" Version="11.9.0" />
```

a referência deixa de existir no projeto.

O Visual Studio atualiza a visualização da dependência de acordo com a configuração do `.csproj`.

### Remoção de referências não utilizadas

A aula também mostra uma funcionalidade do Visual Studio para identificar referências que não estão sendo utilizadas.

Em projetos grandes, isso ajuda a evitar dependências desnecessárias.

A lógica é:

```text
Projeto
  ↓
Dependências
  ↓
Verificar referências não utilizadas
  ↓
Remover o que não é necessário
```

---

## 4. Onde os pacotes NuGet ficam armazenados

Se os arquivos não ficam diretamente dentro da pasta do projeto, eles precisam ser armazenados em algum outro local.

A aula explica que o NuGet mantém os pacotes baixados em uma pasta local da máquina.

O caminho depende do sistema operacional.

No Windows, o exemplo apresentado segue a ideia:

```text
C:\Users\<usuario>\.nuget\packages
```

Essa pasta funciona como um local compartilhado para os pacotes que já foram baixados naquela máquina.

### Ideia geral

```text
Projetos diferentes
      │
      │ usam pacotes
      ▼
.nuget/packages
      │
      ├── fluentvalidation
      ├── automapper
      ├── ...
      └── outros pacotes
```

Isso evita que cada projeto precise possuir uma cópia física independente da mesma biblioteca dentro do próprio repositório.

---

## 5. Estrutura da pasta global de pacotes

Dentro da pasta do NuGet existem subpastas para os pacotes já utilizados na máquina.

A aula utiliza o **AutoMapper** como exemplo.

Uma representação simplificada seria:

```text
.nuget
└── packages
    └── automapper
        ├── 12.x.x
        └── 13.0.1
            ├── arquivos do pacote
            ├── documentação/metadados
            └── lib
                └── ...
                    └── AutoMapper.dll
```

Um mesmo pacote pode possuir diferentes versões armazenadas.

Isso é importante porque diferentes projetos podem utilizar versões diferentes de uma mesma dependência.

### Exemplo

```text
Projeto A → AutoMapper 12.x
Projeto B → AutoMapper 13.0.1
```

A máquina pode manter ambas as versões na pasta de pacotes.

---

## 6. DLLs e resultado da compilação

A aula retoma o conceito de **DLL**.

Projetos do tipo biblioteca de classes, como:

- Application;
- Domain;
- Infrastructure;
- outras Class Libraries;

podem gerar uma DLL como resultado da compilação.

### Fluxo conceitual

```text
Código C#
   ↓
build
   ↓
DLL
```

No exemplo apresentado, ao navegar pelos arquivos do pacote AutoMapper é possível encontrar DLLs dentro da estrutura do pacote.

A aplicação utiliza essas bibliotecas compiladas para acessar funcionalidades disponibilizadas pelo pacote.

---

## 7. Limpando o armazenamento do NuGet

Com o tempo, a pasta local do NuGet pode acumular muitos pacotes e várias versões.

Isso acontece porque a máquina pode ter aberto diversos projetos ao longo do tempo.

Cada projeto pode ter exigido dependências diferentes.

Consequentemente:

```text
mais projetos usados
      ↓
mais pacotes baixados
      ↓
mais arquivos armazenados localmente
      ↓
mais espaço em disco utilizado
```

A aula mostra que o Visual Studio possui uma opção de limpeza nas configurações do **NuGet Package Manager**.

O caminho apresentado é semelhante a:

```text
Tools
└── NuGet Package Manager
    └── Package Manager Settings
        └── General
            └── Clear All NuGet Storage
```

A intenção é remover arquivos locais armazenados pelo NuGet.

A aula reforça que não é necessário realizar essa limpeza constantemente, mas ela pode ser utilizada quando esse armazenamento estiver ocupando espaço demais.

---

## 8. Restauração automática dos pacotes

Uma consequência importante dessa arquitetura é a **restauração de dependências**.

O projeto conhece:

- o nome do pacote;
- a versão necessária.

Exemplo:

```xml
<PackageReference Include="FluentValidation" Version="11.9.0" />
```

Quando a solution é aberta, o ambiente pode verificar se a dependência necessária já está disponível localmente.

### Cenário 1 — pacote já existe localmente

```text
Projeto precisa do FluentValidation 11.9.0
                 ↓
A versão já existe na pasta do NuGet?
                 ↓
               Sim
                 ↓
Utiliza a dependência local
```

### Cenário 2 — pacote não existe localmente

```text
Projeto precisa do FluentValidation 11.9.0
                 ↓
A versão já existe localmente?
                 ↓
               Não
                 ↓
Download/restauração do pacote
```

Esse processo explica por que abrir uma solution pela primeira vez pode levar mais tempo.

O ambiente precisa identificar e restaurar dependências ausentes.

### Importante para Git

Como o projeto guarda a referência e a versão, não é necessário versionar no repositório todos os arquivos baixados pelo NuGet.

O repositório contém a configuração necessária para que as dependências possam ser restauradas posteriormente.

---

## 9. Referenciando uma DLL diretamente

A aula também apresenta outro cenário.

Imagine que alguém forneça diretamente uma DLL já compilada e essa biblioteca não esteja sendo distribuída como pacote NuGet.

Nesse caso, é possível adicionar uma referência diretamente à DLL.

Conceitualmente:

```text
Biblioteca.dll
      ↓
Adicionar referência
      ↓
Projeto passa a utilizar a DLL
```

A aula mostra a opção de procurar uma DLL existente na máquina e selecioná-la como referência.

Nesse cenário, a DLL pode fazer parte diretamente da estrutura utilizada pelo projeto.

Isso é diferente do comportamento mostrado para uma dependência gerenciada pelo NuGet.

---

## 10. NuGet x DLL dentro do projeto

A diferença destacada pela aula pode ser resumida assim:

| Situação | Como a dependência é representada |
|---|---|
| Pacote NuGet | `PackageReference` com nome e versão |
| Projeto da mesma solution | referência de projeto |
| DLL adicionada manualmente | referência direta ao arquivo compilado |

### NuGet

```text
Projeto
  ↓
PackageReference
  ↓
NuGet localiza/restaura dependência
```

### DLL adicionada diretamente

```text
Projeto
  ↓
Referência direta
  ↓
Biblioteca.dll
```

A aula utiliza essa diferença para explicar por que projetos que usam NuGet normalmente não precisam colocar os binários baixados diretamente no GitHub junto com o código-fonte.

---

## 11. Package Sources

A segunda grande pergunta da aula é se o NuGet pode utilizar outra fonte além do `nuget.org`.

A resposta apresentada é **sim**.

O Visual Studio permite configurar diferentes **Package Sources**.

O caminho mostrado é:

```text
Tools
└── NuGet Package Manager
    └── Package Manager Settings
        └── Package Sources
```

Ali podem existir diferentes servidores configurados.

Exemplo conceitual:

```text
Package Sources
├── nuget.org
├── Servidor interno da empresa
└── Outro repositório privado
```

Para adicionar uma nova fonte, a aula mostra a ideia de informar:

- um nome para a fonte;
- o endereço do servidor;
- atualizar/salvar a configuração.

O `nuget.org` é apresentado com um endpoint no formato de uma URL de feed.

A mesma ideia pode ser aplicada a outro servidor compatível.

---

## 12. Repositórios privados de pacotes

Depois de configurar mais de uma fonte, o Visual Studio permite escolher em qual origem procurar pacotes.

Ao abrir o gerenciamento de NuGet Packages, pode existir uma seleção semelhante a:

```text
Package Sources
├── All
├── nuget.org
└── Servidor da empresa
```

Dessa forma, o desenvolvedor pode:

- procurar em todas as fontes;
- procurar somente no `nuget.org`;
- procurar somente em um servidor privado específico.

### Fluxo

```text
Manage NuGet Packages
        ↓
Selecionar Package Source
        ↓
Browse
        ↓
Pesquisar pacotes disponíveis naquela fonte
```

---

## 13. Quando uma empresa pode usar um servidor próprio

A aula apresenta dois exemplos de uso para uma fonte privada.

### 13.1 Biblioteca interna da empresa

Uma organização pode possuir uma biblioteca utilizada em vários sistemas internos.

Exemplo:

```text
Biblioteca interna
        ↓
Servidor privado de pacotes
        ↓
Projeto A
Projeto B
Projeto C
```

Assim, os times conseguem compartilhar componentes sem necessariamente disponibilizá-los publicamente.

### 13.2 Biblioteca com acesso controlado

Outro exemplo apresentado é uma biblioteca distribuída apenas para clientes autorizados.

Nesse cenário, o servidor pode exigir autenticação.

Fluxo conceitual:

```text
Developer
   ↓
Package Source privado
   ↓
Autenticação
   ↓
Pacote liberado
```

A aula comenta que o ambiente pode solicitar credenciais para acessar uma fonte privada.

---

## 14. Fluxos importantes

### Fluxo de instalação/restauração de um pacote

```text
.csproj
  ↓
PackageReference
  ↓
NuGet verifica dependência
  ↓
Existe localmente?
  ├── Sim → utiliza
  └── Não → baixa da Package Source
```

### Fluxo de múltiplas fontes

```text
NuGet Package Manager
        ↓
Package Sources
        ↓
┌───────────────────────┐
│ nuget.org             │
│ servidor interno      │
│ servidor privado      │
└───────────────────────┘
        ↓
Seleciona a origem
        ↓
Pesquisa/instala pacote
```

### Fluxo de compartilhamento pelo Git

```text
Projeto versionado
├── código-fonte
├── .csproj
└── PackageReference

            ↓ clone

Outra máquina
            ↓
restaura dependências
            ↓
pacotes ficam disponíveis localmente
```

---

## 15. Resumo

Nesta aula foram aprofundados os detalhes de funcionamento do NuGet que ficam menos visíveis durante a instalação de uma biblioteca.

Os principais pontos foram:

- instalar um pacote não significa copiar todos os arquivos dele diretamente para a pasta do projeto;
- o `.csproj` registra a dependência através de `PackageReference`;
- o Visual Studio também exibe os pacotes em `Dependencies > Packages`;
- os pacotes baixados são armazenados localmente em uma pasta gerenciada pelo NuGet;
- no Windows, a aula mostra a pasta `.nuget/packages` dentro do usuário;
- diferentes versões de uma mesma biblioteca podem coexistir localmente;
- bibliotecas compiladas podem disponibilizar DLLs utilizadas pelo projeto;
- o armazenamento local pode ser limpo pelas configurações do NuGet Package Manager;
- dependências ausentes podem ser restauradas automaticamente ao abrir ou preparar o projeto;
- uma DLL também pode ser referenciada diretamente quando necessário;
- dependências NuGet e DLLs adicionadas manualmente possuem formas diferentes de gerenciamento;
- o NuGet pode trabalhar com outras fontes além do `nuget.org`;
- `Package Sources` permitem adicionar servidores próprios;
- organizações podem utilizar feeds privados para compartilhar bibliotecas internas ou restringir o acesso a determinados pacotes.

---

## 16. Visão geral

```text
                    NuGet
                      │
        ┌─────────────┴─────────────┐
        │                           │
 PackageReference             Package Sources
        │                           │
 nome + versão          ┌───────────┼───────────┐
        │                │           │           │
        ▼             nuget.org   privado    interno
 restauração
        │
        ▼
 armazenamento local
        │
        ▼
 .nuget/packages
        │
        ├── pacote A
        ├── pacote B
        └── versões/DLLs
```

A principal ideia é separar duas coisas:

```text
Configuração da dependência ≠ arquivos físicos do pacote dentro do projeto
```

O projeto registra **qual dependência precisa**, enquanto o NuGet cuida de localizar, armazenar e restaurar os arquivos necessários para que aquela dependência possa ser utilizada.