# Domain-Driven Design (DDD)

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [O que é DDD](#o-que-é-ddd)
3. [DDD não é uma arquitetura](#ddd-não-é-uma-arquitetura)
4. [Objetivo do DDD](#objetivo-do-ddd)
5. [Benefícios apresentados na aula](#benefícios-apresentados-na-aula)
6. [Começar organizado desde o início](#começar-organizado-desde-o-início)
7. [Ideia-chave 1: Linguagem Ubíqua](#ideia-chave-1-linguagem-ubíqua)
8. [Ideia-chave 2: Contextos Delimitados](#ideia-chave-2-contextos-delimitados)
9. [Origem do DDD](#origem-do-ddd)
10. [Resumo](#resumo)

---

## Visão Geral

Nesta aula é apresentado o conceito de **Domain-Driven Design**, normalmente abreviado como:

```text
DDD
```

A ideia central apresentada é utilizar o DDD como uma abordagem para organizar e modelar o software de forma alinhada aos problemas e necessidades do mundo real.

```text
Problema do mundo real
        ↓
Modelagem do domínio
        ↓
Organização do código
        ↓
Software alinhado ao negócio
```

---

## O que é DDD

Segundo a aula, o DDD é uma abordagem cujo foco é:

> modelar problemas do mundo real em código.

A aplicação deixa de ser pensada apenas como um conjunto de Controllers, Services, classes e métodos.

A estrutura do software passa a representar de maneira mais clara o domínio e as regras do negócio que estão sendo implementadas.

Exemplo conceitual:

```text
Negócio
├── Usuários
├── Pets
├── Pedidos
└── Pagamentos

            ↓ modelagem

Código
├── contexto de usuários
├── contexto de pets
├── contexto de pedidos
└── contexto de pagamentos
```

O objetivo é aproximar a estrutura do código da realidade que o sistema representa.

---

## DDD não é uma arquitetura

Um ponto enfatizado na aula é:

```text
DDD ≠ Arquitetura
```

O DDD é apresentado como uma **abordagem** ou um **guia para organização do código**.

Entretanto, a forma de aplicar essa abordagem pode influenciar diretamente a arquitetura da aplicação.

Isso ocorre porque o DDD pode afetar:

- a organização dos projetos;
- a divisão em camadas;
- a separação de responsabilidades;
- a comunicação entre essas partes;
- a forma como o domínio é representado no código.

Portanto:

```text
DDD
│
├── não é, por si só, uma arquitetura
│
└── pode influenciar a arquitetura do software
```

A aula também destaca que DDD não está limitado a APIs ou aplicações back-end.

A abordagem pode ser utilizada em outros tipos de projetos.

---

## Objetivo do DDD

O foco apresentado é fazer o design do software representar melhor o negócio.

Em vez de organizar o código apenas de acordo com detalhes técnicos, o projeto passa a refletir conceitos e responsabilidades existentes no domínio.

```text
Necessidades do negócio
        ↓
Conceitos do domínio
        ↓
Organização do software
```

Essa aproximação auxilia tanto quem desenvolve quanto quem conhece profundamente as regras do negócio.

---

## Benefícios apresentados na aula

A aula destaca alguns motivos para utilizar DDD.

### 1. Organização

O código tende a ficar mais organizado porque as responsabilidades são divididas e representadas de forma mais clara.

```text
Responsabilidades bem definidas
                ↓
Código mais organizado
```

### 2. Facilidade de compreensão

Segundo a aula, a abordagem pode ser entendida mesmo por quem ainda está começando a desenvolver software.

A intenção é criar uma estrutura que facilite identificar:

- onde determinada regra está localizada;
- qual área é responsável por determinada funcionalidade;
- como diferentes partes do sistema se relacionam.

### 3. Facilidade de expansão

A organização proposta facilita adicionar e remover funcionalidades conforme o projeto cresce.

```text
Projeto inicial
     ↓
Novas funcionalidades
     ↓
Expansão organizada
```

A ideia é evitar que o crescimento transforme o projeto em uma estrutura difícil de manter.

---

## Começar organizado desde o início

A aula recomenda aplicar boas práticas mesmo em projetos pequenos.

A justificativa é que um projeto inicialmente simples pode crescer futuramente.

Se toda a aplicação for construída sem uma organização clara, reorganizar tudo posteriormente pode se tornar muito mais difícil.

```text
Projeto pequeno
     ↓
começa organizado
     ↓
projeto cresce
     ↓
manutenção mais simples
```

Em contraste:

```text
Projeto pequeno
     ↓
sem organização
     ↓
projeto cresce muito
     ↓
refatoração mais difícil
```

A recomendação apresentada é pensar na organização desde o início.

---

## Ideia-chave 1: Linguagem Ubíqua

A primeira ideia-chave apresentada é a:

```text
Linguagem Ubíqua
```

A proposta é criar uma linguagem clara e comum entre todas as pessoas envolvidas no projeto.

Nem todas as pessoas de uma equipe possuem conhecimentos técnicos de programação.

Podem participar, por exemplo:

```text
Desenvolvedores
Designers
Managers
Especialistas do negócio
Cliente
```

Por isso, todos precisam compartilhar uma linguagem que represente os mesmos conceitos.

### Exemplo conceitual

Se o negócio utiliza o termo:

```text
Pedido
```

é interessante que esse mesmo conceito também esteja representado de maneira consistente no software.

```text
Negócio               Código
  │                      │
  └── Pedido ───────────► Order/Pedido
```

A intenção é diminuir ambiguidades e facilitar a comunicação entre especialistas técnicos e especialistas do negócio.

### Objetivo

```text
Mesmos conceitos
      ↓
Mesmos termos
      ↓
Comunicação mais clara
```

A linguagem utilizada nas conversas sobre o negócio deve estar alinhada à forma como esses conceitos aparecem no projeto.

---

## Ideia-chave 2: Contextos Delimitados

A segunda ideia-chave apresentada é:

```text
Contextos Delimitados
```

A ideia é dividir o projeto em diferentes áreas.

Cada área deve possuir:

- uma responsabilidade;
- suas próprias regras;
- seus próprios conceitos;
- um limite claro de atuação.

Exemplo:

```text
Aplicação
│
├── Contexto de Usuários
│   └── regras relacionadas a usuários
│
├── Contexto de Pets
│   └── regras relacionadas a pets
│
└── Contexto de Pagamentos
    └── regras relacionadas a pagamentos
```

Essa separação ajuda a evitar confusão entre responsabilidades diferentes.

### Benefícios apresentados

```text
Contextos bem definidos
        ↓
menos mal-entendidos
        ↓
melhor gerenciamento
        ↓
expansão mais simples
```

Cada área pode ser compreendida e evoluída de maneira mais independente.

---

## Linguagem Ubíqua + Contextos Delimitados

As duas ideias podem ser visualizadas juntas:

```text
                        DDD
                         │
             ┌───────────┴───────────┐
             │                       │
             ▼                       ▼
     Linguagem Ubíqua        Contextos Delimitados
             │                       │
             ▼                       ▼
    termos compartilhados     áreas bem definidas
    entre negócio e código    com responsabilidades
```

A linguagem ajuda as pessoas a falar sobre os mesmos conceitos.

Os contextos ajudam a determinar onde esses conceitos e regras pertencem dentro do sistema.

---

## Origem do DDD

A aula atribui a criação e popularização dessa abordagem a **Eric Evans**.

É citado o livro publicado em 2003:

```text
Domain-Driven Design
Tackling Complexity in the Heart of Software
```

A ideia destacada na aula é alinhar o design do software com as necessidades do negócio atendido pelo sistema.

```text
Design do software
        ↕
Necessidades do negócio
```

O livro é apresentado como uma referência para aprofundar o estudo do DDD.

---

## Resumo

| Conceito | Ideia principal |
|---|---|
| **DDD** | Abordagem para modelar problemas do mundo real em código |
| **Arquitetura** | Pode ser influenciada pela aplicação do DDD, mas DDD não é apresentado como uma arquitetura em si |
| **Domínio** | Área do negócio que o software representa |
| **Linguagem Ubíqua** | Linguagem comum entre todas as pessoas envolvidas no projeto |
| **Contextos Delimitados** | Divisão do sistema em áreas com responsabilidades e regras próprias |
| **Organização** | Um dos principais benefícios destacados |
| **Escalabilidade do projeto** | A estrutura facilita adicionar ou remover funcionalidades conforme o sistema cresce |

### Visão geral

```text
                    MUNDO REAL
                        │
                        ▼
                    DOMÍNIO
                        │
                        ▼
              DOMAIN-DRIVEN DESIGN
                        │
          ┌─────────────┴─────────────┐
          │                           │
          ▼                           ▼
  Linguagem Ubíqua            Contextos Delimitados
          │                           │
          ▼                           ▼
Comunicação consistente      Divisão de responsabilidades
          │                           │
          └─────────────┬─────────────┘
                        ▼
             Software mais organizado
                        │
                        ▼
             alinhado ao negócio
```

> **Em resumo:** de acordo com a aula, Domain-Driven Design é uma abordagem para organizar e modelar software de acordo com problemas e necessidades reais do negócio. Ele não é apresentado como uma arquitetura, embora possa influenciar fortemente a estrutura da aplicação. As duas ideias-chave introduzidas são a **Linguagem Ubíqua**, que cria termos compartilhados entre pessoas técnicas e especialistas do negócio, e os **Contextos Delimitados**, que dividem o sistema em áreas com responsabilidades e regras bem definidas.
