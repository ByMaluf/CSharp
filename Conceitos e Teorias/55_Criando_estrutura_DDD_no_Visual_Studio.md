# Criando a Estrutura DDD no Visual Studio

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo da estrutura](#objetivo-da-estrutura)
3. [Criando a Solution](#criando-a-solution)
4. [Organização com Solution Folders](#organização-com-solution-folders)
5. [Diferença entre Solution Folder e pasta física](#diferença-entre-solution-folder-e-pasta-física)
6. [Criando o projeto de API](#criando-o-projeto-de-api)
7. [Criando as Class Libraries](#criando-as-class-libraries)
8. [Os seis projetos da aplicação](#os-seis-projetos-da-aplicação)
9. [Configurando as referências entre projetos](#configurando-as-referências-entre-projetos)
10. [Dependências de cada projeto](#dependências-de-cada-projeto)
11. [Estrutura final](#estrutura-final)
12. [Resumo](#resumo)
13. [Visão Geral](#visão-geral)

---

## Contexto da aula

Depois das aulas conceituais sobre Domain-Driven Design e sobre a responsabilidade de cada projeto, esta aula coloca a estrutura em prática no Visual Studio.

A aplicação utilizada no módulo é chamada:

```text
CashFlow
```

O objetivo é criar todos os projetos que formarão o back-end e configurar quais projetos podem utilizar quais outros projetos.

A aula também prepara a Solution para os futuros testes de unidade.

---

## Objetivo da estrutura

A organização apresentada separa o código em projetos com responsabilidades diferentes.

A estrutura possui:

```text
CashFlow.API
CashFlow.Application
CashFlow.Communication
CashFlow.Domain
CashFlow.Exception
CashFlow.Infrastructure
```

Além disso, a Solution é organizada em duas áreas:

```text
src

tests
```

A ideia é separar:

```text
src   → código-fonte da aplicação

tests → projetos de testes
```

---

## Criando a Solution

No Visual Studio, a aula começa com:

```text
File
↓
New Project
↓
Blank Solution
```

O nome escolhido é:

```text
CashFlow
```

Neste momento, a Solution ainda não possui nenhum projeto.

---

## Organização com Solution Folders

A aula cria duas pastas dentro da própria Solution:

```text
src

tests
```

No Visual Studio:

```text
Botão direito na Solution
↓
Add
↓
New Solution Folder
```

### src

Abreviação de:

```text
source
```

Sua função é agrupar os projetos que fazem parte do código-fonte da aplicação.

### tests

Será utilizada para organizar os projetos relacionados aos testes de unidade apresentados posteriormente no módulo.

Estrutura visual:

```text
CashFlow
├── src
└── tests
```

---

## Diferença entre Solution Folder e pasta física

A aula chama atenção para um detalhe importante do Visual Studio.

Criar uma pasta diretamente na Solution não significa, segundo o comportamento apresentado na aula, que uma pasta física correspondente será automaticamente criada no sistema de arquivos.

Ou seja:

```text
Solution
├── src
└── tests
```

não implica automaticamente em:

```text
CashFlow/
├── src/
└── tests/
```

Por isso, na aula as pastas físicas também são criadas manualmente no diretório do projeto.

Depois disso, a organização visual da Solution e a organização no sistema de arquivos permanecem equivalentes.

> Essa observação é específica para as **Solution Folders** mostradas na aula. Pastas criadas dentro de um projeto são tratadas de outra forma.

---

## Criando o projeto de API

Dentro da área `src`, o primeiro projeto criado é:

```text
ASP.NET Core Web API
```

Nome:

```text
CashFlow.API
```

A localização é ajustada para que o projeto seja criado fisicamente dentro de:

```text
CashFlow/src/
```

Estrutura:

```text
CashFlow/
└── src/
    └── CashFlow.API/
```

O projeto de API é diferente dos demais porque é o projeto Web API da aplicação.

---

## Criando as Class Libraries

Todos os demais projetos apresentados na aula são criados como:

```text
Class Library
```

Também são armazenados dentro de:

```text
CashFlow/src/
```

Depois de criar cada Class Library, a classe padrão `Class1` é removida.

---

## Os seis projetos da aplicação

Ao final da criação, a Solution possui:

```text
CashFlow.API
CashFlow.Application
CashFlow.Communication
CashFlow.Domain
CashFlow.Exception
CashFlow.Infrastructure
```

Representação:

```text
CashFlow
│
├── src
│   ├── CashFlow.API
│   ├── CashFlow.Application
│   ├── CashFlow.Communication
│   ├── CashFlow.Domain
│   ├── CashFlow.Exception
│   └── CashFlow.Infrastructure
│
└── tests
```

A responsabilidade conceitual de cada projeto foi apresentada na aula anterior; aqui o foco é criar a estrutura física e configurar as referências.

---

## Configurando as referências entre projetos

Depois de criar todos os projetos, é necessário configurar quais projetos podem enxergar outros.

No Visual Studio isso é feito através de:

```text
Dependencies
↓
Add Project Reference
```

Essas referências implementam, na prática, a direção de dependências apresentada no diagrama da arquitetura.

---

## Dependências de cada projeto

### CashFlow.API

A API recebe referência para quatro projetos:

```text
CashFlow.Application
CashFlow.Communication
CashFlow.Exception
CashFlow.Infrastructure
```

Representação:

```text
CashFlow.API
├── Application
├── Communication
├── Exception
└── Infrastructure
```

O acesso a `Infrastructure` corresponde ao detalhe prático apresentado na aula anterior sobre configuração de injeção de dependência.

---

### CashFlow.Application

O projeto de Application referencia:

```text
CashFlow.Communication
CashFlow.Domain
CashFlow.Exception
```

Representação:

```text
CashFlow.Application
├── Communication
├── Domain
└── Exception
```

---

### CashFlow.Infrastructure

Infrastructure possui apenas uma referência:

```text
CashFlow.Domain
```

Representação:

```text
CashFlow.Infrastructure
└── Domain
```

---

### CashFlow.Communication

Não possui referência para os outros projetos.

```text
CashFlow.Communication
└── independente
```

---

### CashFlow.Domain

Também permanece independente.

```text
CashFlow.Domain
└── independente
```

---

### CashFlow.Exception

Da mesma forma:

```text
CashFlow.Exception
└── independente
```

---

## Mapa das referências

```text
                    CashFlow.API
                  /      |       \
                 ▼       ▼        ▼
       Application  Communication  Exception
            │
            ├──────────► Communication
            ├──────────► Domain
            └──────────► Exception

CashFlow.API ──────────► Infrastructure
                           │
                           ▼
                        Domain
```

De forma simplificada:

```text
API
├── Application
├── Communication
├── Exception
└── Infrastructure

Application
├── Communication
├── Domain
└── Exception

Infrastructure
└── Domain

Communication
└── nenhuma referência

Domain
└── nenhuma referência

Exception
└── nenhuma referência
```

---

## Por que gerenciar essas referências?

As referências determinam quais projetos conseguem utilizar classes e tipos definidos em outros projetos.

Isso ajuda a preservar a separação de responsabilidades apresentada na arquitetura.

Exemplo:

```text
Application
```

pode utilizar contratos e elementos definidos em:

```text
Domain
```

mas não recebe referência direta para:

```text
Infrastructure
```

Já a infraestrutura conhece o Domain para implementar os contratos definidos nessa camada.

---

## Estrutura final

No sistema de arquivos, a estrutura mostrada na aula fica conceitualmente assim:

```text
CashFlow/
│
├── CashFlow.sln
│
├── src/
│   ├── CashFlow.API/
│   ├── CashFlow.Application/
│   ├── CashFlow.Communication/
│   ├── CashFlow.Domain/
│   ├── CashFlow.Exception/
│   └── CashFlow.Infrastructure/
│
└── tests/
```

E dentro da Solution:

```text
CashFlow
│
├── src
│   ├── CashFlow.API
│   ├── CashFlow.Application
│   ├── CashFlow.Communication
│   ├── CashFlow.Domain
│   ├── CashFlow.Exception
│   └── CashFlow.Infrastructure
│
└── tests
```

---

## Resumo

| Projeto | Tipo | Referências configuradas na aula |
|---|---|---|
| `CashFlow.API` | ASP.NET Core Web API | Application, Communication, Exception, Infrastructure |
| `CashFlow.Application` | Class Library | Communication, Domain, Exception |
| `CashFlow.Communication` | Class Library | Nenhuma |
| `CashFlow.Domain` | Class Library | Nenhuma |
| `CashFlow.Exception` | Class Library | Nenhuma |
| `CashFlow.Infrastructure` | Class Library | Domain |

### Organização da Solution

| Pasta | Finalidade |
|---|---|
| `src` | Código-fonte da aplicação |
| `tests` | Projetos de testes |

### Pontos principais

```text
Blank Solution
↓
criar src e tests
↓
criar as pastas físicas correspondentes
↓
criar CashFlow.API
↓
criar as cinco Class Libraries
↓
configurar Project References
↓
estrutura pronta para o módulo
```

---

## Visão Geral

```text
                         CASHFLOW
                            │
                 ┌──────────┴──────────┐
                 │                     │
                src                  tests
                 │
     ┌───────────┼─────────────────────────────────────────────┐
     │           │              │          │         │         │
     ▼           ▼              ▼          ▼         ▼         ▼
    API     Application   Communication   Domain   Exception  Infrastructure
     │           │                                      │         │
     │           ├── Communication                      │         └── Domain
     │           ├── Domain                             │
     │           └── Exception                          │
     │
     ├── Application
     ├── Communication
     ├── Exception
     └── Infrastructure
```

> **Em resumo:** a aula transforma o diagrama conceitual apresentado anteriormente em uma Solution real no Visual Studio. O projeto `CashFlow` passa a ser organizado com as pastas `src` e `tests`, seis projetos são criados dentro de `src` e as referências entre eles são configuradas conforme a direção de dependências definida para a aplicação.