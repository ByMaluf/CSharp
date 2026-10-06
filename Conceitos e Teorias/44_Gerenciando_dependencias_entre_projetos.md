# Gerenciando Dependências entre Projetos em uma Solution .NET

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo da organização em múltiplos projetos](#objetivo-da-organização-em-múltiplos-projetos)
3. [Criando a Solution Petfolio](#criando-a-solution-petfolio)
4. [Criando o projeto Petfolio.API](#criando-o-projeto-petfolioapi)
5. [Criando o projeto Petfolio.Communication](#criando-o-projeto-petfoliocommunication)
6. [Criando o projeto Petfolio.Application](#criando-o-projeto-petfolioapplication)
7. [Definindo o projeto de inicialização](#definindo-o-projeto-de-inicialização)
8. [Gerenciando dependências com Project Reference](#gerenciando-dependências-com-project-reference)
9. [Dependências da API](#dependências-da-api)
10. [Dependência do Application](#dependência-do-application)
11. [Communication como projeto independente](#communication-como-projeto-independente)
12. [Dependência cíclica](#dependência-cíclica)
13. [Estrutura final](#estrutura-final)
14. [Resumo](#resumo)

---

## Contexto da aula

Nesta aula, a API passa a ser organizada em vários projetos dentro da mesma Solution.

A ideia é separar responsabilidades em vez de concentrar tudo em um único projeto.

O projeto utilizado como exemplo é chamado:

```text
Petfolio
```

A proposta é que essa API trabalhe com dados relacionados a pets e continue sendo utilizada em módulos futuros.

---

## Objetivo da organização em múltiplos projetos

A estrutura criada na aula possui três projetos principais:

```text
Petfolio.API
Petfolio.Application
Petfolio.Communication
```

Cada um possui uma responsabilidade diferente.

### Petfolio.API

Responsável por:

```text
Receber requisições HTTP
Executar a aplicação
Conter os Controllers
```

### Petfolio.Application

Responsável por:

```text
Regras de negócio
```

### Petfolio.Communication

Responsável por:

```text
Classes de Request
Classes de Response
```

---

## Criando a Solution Petfolio

No Visual Studio, a aula começa criando uma Solution vazia.

Caminho apresentado:

```text
File
↓
New
↓
Project
↓
Blank Solution
```

O nome escolhido é:

```text
Petfolio
```

Inicialmente, a Solution possui:

```text
0 projetos
```

A Solution funciona como o agrupador dos projetos que formarão a aplicação.

---

## Criando o projeto Petfolio.API

O primeiro projeto criado é do tipo:

```text
ASP.NET Core Web API
```

A aula destaca a escolha da versão com:

```text
C#
```

em vez da opção equivalente em F#.

O nome utilizado é:

```text
Petfolio.API
```

O framework selecionado na aula é:

```text
.NET 8
```

Depois da criação, os arquivos de exemplo como WeatherForecast são removidos para começar com uma estrutura mais limpa.

---

## Criando o projeto Petfolio.Communication

O segundo projeto é criado como:

```text
Class Library
```

ou, em português:

```text
Biblioteca de Classes
```

O nome utilizado é:

```text
Petfolio.Communication
```

Sua responsabilidade é concentrar as classes usadas na comunicação entre as camadas.

Exemplos:

```text
Requests
Responses
```

Na primeira API do curso, essas classes ficavam dentro de uma pasta chamada Communication.

Agora essa responsabilidade passa para um projeto separado.

---

## Criando o projeto Petfolio.Application

O terceiro projeto também é criado como:

```text
Class Library
```

O nome é:

```text
Petfolio.Application
```

Sua responsabilidade será:

```text
Conter as regras de negócio
```

Assim, a Solution passa a ter:

```text
Petfolio
│
├── Petfolio.API
├── Petfolio.Application
└── Petfolio.Communication
```

---

## Definindo o projeto de inicialização

Como apenas o projeto de API é executável, ele deve ser definido como o projeto de inicialização.

No Visual Studio:

```text
Botão direito em Petfolio.API
↓
Set as Startup Project
```

Isso faz com que, ao pressionar:

```text
F5
```

ou utilizar o botão de execução do Visual Studio, o projeto executado seja:

```text
Petfolio.API
```

As bibliotecas de classes não são executadas diretamente.

---

## Gerenciando dependências com Project Reference

Depois de criar os projetos, é necessário definir quais projetos podem enxergar outros.

Isso é feito por meio de:

```text
Project Reference
```

No Visual Studio:

```text
Dependencies
↓
Add Project Reference
```

Essa referência permite que um projeto utilize classes definidas em outro projeto da mesma Solution.

---

## Dependências da API

A API precisa enxergar:

```text
Petfolio.Application
Petfolio.Communication
```

Portanto:

```text
Petfolio.API
   │
   ├── depende de → Petfolio.Application
   │
   └── depende de → Petfolio.Communication
```

No Visual Studio, ambas as referências são marcadas em:

```text
Add Project Reference
```

Depois disso, elas aparecem em:

```text
Dependencies
└── Projects
```

---

## Dependência do Application

O projeto:

```text
Petfolio.Application
```

precisa trabalhar com as classes de comunicação.

Por isso, ele possui referência para:

```text
Petfolio.Communication
```

Estrutura:

```text
Petfolio.Application
        │
        └── depende de → Petfolio.Communication
```

Isso permite que a lógica de negócio receba e utilize os modelos definidos no projeto de comunicação.

---

## Communication como projeto independente

O projeto:

```text
Petfolio.Communication
```

não precisa enxergar os outros dois projetos.

Segundo a organização apresentada na aula, sua função é apenas conter:

```text
Requests
Responses
```

Portanto:

```text
Petfolio.Communication
        │
        └── sem dependências dos outros projetos
```

---

## Dependência cíclica

A aula chama atenção para um problema importante:

```text
Dependência cíclica
```

Imagine:

```text
API depende de Application
```

Se também fizermos:

```text
Application depende de API
```

teremos:

```text
API
 │
 ▼
Application
 │
 └──────────► API
```

Isso cria um ciclo.

A aula explica que essa relação não deve existir.

Portanto, se:

```text
Petfolio.API
↓
depende de
Petfolio.Application
```

então o Application não deve depender da API.

---

## Direção das dependências

A estrutura mostrada na aula pode ser representada assim:

```text
                    Petfolio.API
                    /          \
                   ▼            ▼
     Petfolio.Application   Petfolio.Communication
                 │
                 ▼
     Petfolio.Communication
```

Ou de forma simplificada:

```text
API
├── Application
└── Communication

Application
└── Communication

Communication
└── nenhum dos dois
```

---

## Por que separar dessa forma?

Essa organização permite que cada projeto tenha uma responsabilidade mais clara.

### API

```text
Entrada da aplicação
```

### Application

```text
Regras de negócio
```

### Communication

```text
Objetos usados para entrada e saída de dados
```

Também torna explícito quais projetos podem utilizar outros.

---

## Estrutura final

```text
Petfolio.sln
│
├── Petfolio.API
│   ├── Executável
│   ├── Controllers
│   ├── referência → Petfolio.Application
│   └── referência → Petfolio.Communication
│
├── Petfolio.Application
│   ├── Class Library
│   ├── Regras de negócio
│   └── referência → Petfolio.Communication
│
└── Petfolio.Communication
    ├── Class Library
    ├── Requests
    ├── Responses
    └── sem referência para API ou Application
```

---

## Resumo

| Projeto | Tipo | Responsabilidade | Dependências apresentadas |
|---|---|---|---|
| **Petfolio.API** | ASP.NET Core Web API | Executar a aplicação e receber requisições | Application e Communication |
| **Petfolio.Application** | Class Library | Regras de negócio | Communication |
| **Petfolio.Communication** | Class Library | Requests e Responses | Nenhuma das outras camadas |

| Conceito | Descrição |
|---|---|
| **Solution** | Agrupa os projetos da aplicação |
| **Class Library** | Projeto usado para armazenar classes e implementações reutilizadas por outros projetos |
| **Startup Project** | Projeto executado ao iniciar a Solution |
| **Project Reference** | Faz um projeto enxergar e utilizar outro projeto |
| **Dependencies** | Área do Visual Studio onde as referências são exibidas |
| **Dependência cíclica** | Situação em que projetos dependem uns dos outros formando um ciclo |

---

## Visão Geral

```text
                  PETFOLIO
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
      API       Application  Communication
        │            │            │
        │            │            ├── Requests
        │            │            └── Responses
        │            │
        │            └────────────► Communication
        │
        ├────────────► Application
        └────────────► Communication
```

### Regra principal apresentada

```text
Um projeto pode depender de outro
↓
mas devemos evitar ciclos de dependência
```

> **Em resumo:** a aula organiza o Petfolio em três projetos dentro da mesma Solution: `Petfolio.API`, `Petfolio.Application` e `Petfolio.Communication`. A API é o projeto executável e referencia Application e Communication. O Application concentra as regras de negócio e referencia Communication. Já Communication contém Requests e Responses e permanece independente dos demais. As relações entre esses projetos são configuradas por meio de Project References, evitando dependências cíclicas.
