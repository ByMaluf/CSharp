# Biblioteca de Classes em .NET

## 📋 Índice

1. [O que é uma biblioteca de classes?](#o-que-é-uma-biblioteca-de-classes)
2. [A analogia da caixa de ferramentas](#a-analogia-da-caixa-de-ferramentas)
3. [Classes com responsabilidades específicas](#classes-com-responsabilidades-específicas)
4. [Biblioteca de classes não é um projeto executável](#biblioteca-de-classes-não-é-um-projeto-executável)
5. [Projetos executáveis estudados no curso](#projetos-executáveis-estudados-no-curso)
6. [Como projetos executáveis usam bibliotecas de classes](#como-projetos-executáveis-usam-bibliotecas-de-classes)
7. [Fluxo conceitual](#fluxo-conceitual)
8. [Resumo](#resumo)

---

## O que é uma biblioteca de classes?

Em .NET, uma **biblioteca de classes** é um tipo de projeto que pode ser criado no Visual Studio.

O objetivo desse projeto é armazenar implementações que poderão ser utilizadas por outros projetos.

Podemos representar assim:

```text
Biblioteca de Classes
        │
        ├── Classe A
        ├── Classe B
        ├── Classe C
        └── ...
```

Cada classe pode possuir uma responsabilidade específica.

---

## A analogia da caixa de ferramentas

A aula utiliza uma analogia para facilitar o entendimento.

Imagine uma biblioteca de classes como uma:

```text
Caixa de ferramentas
```

Dentro de uma caixa de ferramentas temos várias ferramentas.

Na analogia:

```text
Caixa de ferramentas
        ↓
Biblioteca de Classes

Ferramenta
        ↓
Classe
```

Ou seja:

```text
Biblioteca de Classes
│
├── Classe 1
├── Classe 2
├── Classe 3
└── Classe 4
```

Assim como cada ferramenta possui uma finalidade, cada classe também deve possuir um propósito específico.

---

## Classes com responsabilidades específicas

A biblioteca pode conter classes responsáveis por diferentes tarefas.

Exemplos apresentados na aula:

```text
Classe para enviar e-mail

Classe para conectar com banco de dados

Classe para salvar um usuário
```

Podemos visualizar:

```text
Biblioteca
│
├── EmailService
│   └── Enviar e-mail
│
├── DatabaseConnection
│   └── Conectar ao banco
│
└── UserRepository
    └── Salvar usuário
```

Cada classe funciona como uma ferramenta especializada.

---

## Biblioteca de classes não é um projeto executável

Uma biblioteca de classes, sozinha, não executa uma aplicação.

A aula compara isso com uma caixa de ferramentas parada.

```text
Caixa de ferramentas
↓
Possui ferramentas
↓
Mas não faz nada sozinha
```

O mesmo acontece com uma biblioteca de classes.

```text
Biblioteca de Classes
↓
Possui implementações
↓
Não é executada por conta própria
```

Ela precisa ser utilizada por outro projeto.

---

## Projetos executáveis estudados no curso

A aula relembra dois tipos de projetos executáveis já criados anteriormente.

### Aplicação de Console

No início do curso foi criada uma aplicação semelhante ao clássico:

```text
Hello World
```

Ela era executada e utilizada através do terminal ou console.

Fluxo:

```text
Usuário
  │
  ▼
Console
  │
  ▼
Programa executável
```

---

### API

Outro projeto executável estudado foi uma API.

Quando a API é executada, ela fica aguardando requisições.

```text
API executando
      │
      ▼
Escuta uma porta
      │
      ▼
Aguarda requisições
      │
      ▼
Processa a solicitação
      │
      ▼
Devolve uma resposta
```

A API é, portanto, um projeto executável.

---

## Como projetos executáveis usam bibliotecas de classes

Um projeto executável pode utilizar as classes existentes dentro de uma biblioteca.

Exemplo:

```text
             Biblioteca de Classes
                     │
         ┌───────────┼───────────┐
         ▼           ▼           ▼
     E-mail       Banco       Usuários
         ▲           ▲           ▲
         └───────────┴───────────┘
                     │
                     ▼
              Projeto Executável
```

Assim, o projeto executável utiliza as implementações prontas da biblioteca.

---

## Exemplo com uma API

Podemos imaginar:

```text
API
 │
 ├── recebe uma requisição
 │
 ▼
Biblioteca de Classes
 │
 ├── valida dados
 ├── envia e-mail
 └── acessa banco de dados
 │
 ▼
API devolve resposta
```

A API é responsável pela execução.

A biblioteca fornece as ferramentas necessárias para realizar determinadas tarefas.

---

## Separação de responsabilidades

Podemos pensar da seguinte maneira:

```text
Projeto Executável
↓
Responsável por executar a aplicação

Biblioteca de Classes
↓
Responsável por disponibilizar implementações reutilizáveis
```

Essa divisão permite combinar diferentes projetos para construir aplicações maiores.

---

## Biblioteca como conjunto de ferramentas

A analogia completa fica:

```text
CAIXA DE FERRAMENTAS
│
├── martelo
├── chave de fenda
└── alicate

         ↓ analogia ↓

BIBLIOTECA DE CLASSES
│
├── classe para e-mail
├── classe para banco de dados
└── classe para usuário
```

Porém, alguém precisa utilizar essas ferramentas.

```text
Pessoa
↓
utiliza a caixa de ferramentas
```

Na aplicação:

```text
Projeto executável
↓
utiliza a biblioteca de classes
```

---

## Biblioteca de classes x projeto executável

| Característica | Biblioteca de Classes | Projeto Executável |
|---|---|---|
| Contém classes | ✅ | ✅ |
| Possui implementações | ✅ | ✅ |
| Executa sozinha | ❌ | ✅ |
| Pode ser utilizada por outros projetos | ✅ | Pode utilizar bibliotecas |
| Exemplo citado | Classes de e-mail, banco, usuário | Console ou API |

---

## Fluxo conceitual

```text
USUÁRIO / CLIENTE
       │
       ▼
PROJETO EXECUTÁVEL
       │
       │ utiliza
       ▼
BIBLIOTECA DE CLASSES
       │
   ┌───┼─────────┐
   ▼   ▼         ▼
 E-mail Banco  Usuário
```

Ou no contexto de uma API:

```text
Requisição
    │
    ▼
   API
    │
    ▼
Biblioteca de Classes
    │
    ├── Classes especializadas
    │
    ▼
Processamento
    │
    ▼
Resposta
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Biblioteca de classes** | Tipo de projeto .NET utilizado para armazenar implementações e classes |
| **Classe** | Representada na analogia como uma ferramenta |
| **Caixa de ferramentas** | Analogia utilizada para representar uma biblioteca de classes |
| **Responsabilidade específica** | Cada classe pode ter um propósito determinado |
| **Projeto executável** | Projeto que efetivamente executa uma aplicação |
| **Aplicação de Console** | Exemplo de projeto executável estudado |
| **API** | Outro exemplo de projeto executável estudado |
| **Biblioteca isolada** | Não realiza nenhuma execução por conta própria |
| **Reutilização** | Projetos executáveis podem utilizar implementações disponíveis em bibliotecas |

---

## Visão Geral

```text
           PROJETO EXECUTÁVEL
                  │
                  │ utiliza
                  ▼
        BIBLIOTECA DE CLASSES
                  │
        ┌─────────┼─────────┐
        ▼         ▼         ▼
      Classe    Classe    Classe
      E-mail    Banco     Usuário
```

### Ideia principal

```text
Biblioteca de Classes
↓
Contém ferramentas
↓
Não executa sozinha

Projeto Executável
↓
Utiliza essas ferramentas
↓
Executa a aplicação
```

> **Em resumo:** uma biblioteca de classes é um projeto utilizado para armazenar classes e implementações com responsabilidades específicas. Ela funciona como uma caixa de ferramentas: possui recursos úteis, mas não executa nada sozinha. Para que essas classes sejam utilizadas, um projeto executável — como uma aplicação de console ou uma API — precisa consumir essa biblioteca.
