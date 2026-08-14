# API REST — Representational State Transfer

## 📋 Índice

1. [O que é REST?](#o-que-é-rest)
2. [API REST](#api-rest)
3. [Comunicação através do HTTP](#comunicação-através-do-http)
4. [Recursos e URLs](#recursos-e-urls)
5. [Stateless](#stateless)
6. [Separação entre Cliente e Servidor](#separação-entre-cliente-e-servidor)
7. [REST x RESTful](#rest-x-restful)
8. [Resumo](#resumo)

---

## O que é REST?

**REST (Representational State Transfer)** é um **estilo arquitetural** utilizado na construção de sistemas Web.

Ele define um conjunto de princípios que orientam como sistemas devem se comunicar.

Para que uma API seja considerada **REST**, ela precisa seguir determinados princípios e características desse estilo arquitetural.

De forma simplificada:

    REST
      ↓
    Estilo arquitetural
      ↓
    Define princípios para comunicação
    entre sistemas Web

> REST não é uma linguagem de programação ou um protocolo. É um estilo arquitetural que estabelece princípios para a construção de serviços Web.

---

## API REST

Uma **API REST** é uma API construída seguindo os princípios definidos pelo REST.

A comunicação normalmente ocorre entre:

    CLIENTE
       │
       │ Requisição HTTP
       ▼
      API
       │
       │ Resposta HTTP
       ▼
    CLIENTE

O cliente pode ser:

- uma aplicação Web;
- um aplicativo Android;
- um aplicativo iOS;
- um sistema Desktop;
- outra API.

A API fica responsável por processar as solicitações e executar as operações necessárias.

---

## Comunicação através do HTTP

Um dos princípios apresentados para uma API REST é utilizar o **HTTP** para realizar a comunicação entre cliente e servidor.

Essa comunicação utiliza os métodos HTTP estudados anteriormente, como:

| Método | Operação |
|---|---|
| `GET` | Recuperar informações |
| `POST` | Criar um recurso |
| `PUT` | Atualizar um recurso |
| `DELETE` | Excluir um recurso |

Exemplo:

    Cliente
       │
       │ GET /usuarios/10
       ▼
      API
       │
       │ 200 OK
       ▼
    Cliente

Outro exemplo:

    Cliente
       │
       │ POST /usuarios
       ▼
      API
       │
       │ 201 Created
       ▼
    Cliente

Dessa forma, a comunicação ocorre utilizando:

    HTTP
     │
     ├── GET
     ├── POST
     ├── PUT
     └── DELETE

---

## Recursos e URLs

Em uma API REST, os recursos disponibilizados pela API são acessados através de **URLs**.

Um recurso representa algo que a API disponibiliza ou permite manipular.

Por exemplo:

- usuários;
- produtos;
- pedidos;
- documentos;
- relatórios;
- arquivos.

Exemplo:

    /usuarios

Esse endereço pode representar o recurso **Usuários**.

Combinando a URL com os métodos HTTP, podemos realizar diferentes operações:

    GET     /usuarios
    POST    /usuarios
    PUT     /usuarios
    DELETE  /usuarios

Também podemos trabalhar com um recurso específico.

Exemplo:

    GET /usuarios/10

Nesse caso, estamos solicitando informações relacionadas ao usuário de ID `10`.

---

### Funcionalidades disponibilizadas pela API

A API pode disponibilizar diferentes funcionalidades através de seus recursos.

Por exemplo:

    API
     │
     ├── Cadastrar usuário
     ├── Consultar usuário
     ├── Excluir usuário
     ├── Enviar e-mail
     ├── Gerar relatório
     └── Realizar download de arquivo

Essas funcionalidades são acessadas pelo cliente através das URLs disponibilizadas pela API e utilizando o protocolo HTTP.

---

## Stateless

Outro princípio importante do REST é o **Stateless**.

**Stateless** significa que a API **não mantém o estado das requisições anteriores para processar uma nova requisição**.

Cada requisição deve possuir todas as informações necessárias para que possa ser compreendida e processada de forma independente.

O funcionamento pode ser representado assim:

    Requisição 1
         ↓
       API
         ↓
    Validação
         ↓
    Processamento
         ↓
     Resposta
         ↓
    Requisição finalizada

Quando uma nova requisição chegar:

    Requisição 2
         ↓
       API
         ↓
    Validação
         ↓
    Processamento
         ↓
     Resposta

A **Requisição 2 não deve depender da Requisição 1** para ser compreendida.

---

### Requisições independentes

Em uma arquitetura Stateless:

    Requisição A
         │
         ▼
        API
         │
         ▼
      Resposta A


    Requisição B
         │
         ▼
        API
         │
         ▼
      Resposta B

Cada requisição é independente.

Portanto:

    Requisição atual
           ↓
    Possui as informações
    necessárias para ser
    processada
           ↓
          API

E não:

    Requisição anterior
           ↓
    Requisição atual
           ↓
          API

> Uma API Stateless não depende do histórico de requisições anteriores para processar a requisição atual.

---

## Separação entre Cliente e Servidor

Outro princípio importante é possuir uma **separação clara entre as responsabilidades do cliente e do servidor**.

O cliente não precisa conhecer os detalhes internos de implementação da API.

Por exemplo, o cliente não precisa saber:

- qual banco de dados está sendo utilizado;
- como os dados são armazenados;
- como as regras de negócio foram implementadas;
- quais tecnologias são utilizadas internamente;
- como a API executa determinada operação.

Para o cliente, essas informações devem estar abstraídas.

Exemplo:

    CLIENTE
       │
       │ "Quero salvar estes dados"
       ▼
      API
       │
       ├── SQL Server
       │
       ├── MySQL
       │
       ├── Banco não relacional
       │
       └── Outro mecanismo de armazenamento

O cliente simplesmente realiza a solicitação.

A API decide **como aquela operação será executada internamente**.

---

### Independência entre Cliente e Servidor

Essa separação permite que cliente e servidor evoluam de maneira mais independente.

Por exemplo, podemos inicialmente possuir:

    Aplicativo Android
            │
            ▼
           API

Posteriormente, podemos adicionar outros clientes:

    Android ─────┐
                 │
    iOS ─────────┤
                 ├──→ API
    Site ────────┤
                 │
    Outro sistema┘

Todos podem utilizar a mesma API.

Da mesma forma, a implementação interna da API pode mudar sem que o cliente precise conhecer todos os detalhes dessa mudança.

Por exemplo:

    CLIENTE
       │
       ▼
      API
       │
       ▼
    SQL Server

Pode posteriormente se tornar:

    CLIENTE
       │
       ▼
      API
       │
       ▼
    Outro banco

O cliente continua preocupado apenas com o **contrato estabelecido pela API**, e não com a forma como ela implementa internamente suas funcionalidades.

---

### Responsabilidades bem definidas

Podemos visualizar essa divisão da seguinte maneira:

    CLIENTE
       │
       ├── Interface
       ├── Interação com usuário
       └── Realiza requisições
                │
                ▼
               API
                │
                ├── Processa requisições
                ├── Aplica regras de negócio
                ├── Manipula dados
                └── Comunica-se com outros serviços

Essa separação reduz o acoplamento entre as partes da aplicação.

---

## REST x RESTful

Os termos **REST** e **RESTful** estão relacionados, mas possuem significados diferentes.

### REST

**REST** é o estilo arquitetural.

Ele estabelece os princípios e características que devem ser seguidos.

    REST
      ↓
    Estilo arquitetural
      ↓
    Conjunto de princípios

---

### RESTful

**RESTful** é utilizado para descrever um serviço que **segue os princípios do REST**.

Portanto:

    REST
      ↓
    Define os princípios

    RESTful
      ↓
    Serviço que segue
    esses princípios

Exemplo:

> Uma API que segue os princípios REST pode ser chamada de uma **API RESTful**.

A diferença é essencialmente a forma como os termos são utilizados:

| Termo | Significado |
|---|---|
| **REST** | Estilo arquitetural |
| **RESTful** | Serviço que segue os princípios REST |

---

## Exemplo de uma API REST

Imagine uma API responsável pelo gerenciamento de usuários.

Podemos possuir:

    GET /usuarios

Responsabilidade:

    Listar usuários

---

    GET /usuarios/10

Responsabilidade:

    Consultar o usuário 10

---

    POST /usuarios

Responsabilidade:

    Criar um novo usuário

---

    PUT /usuarios/10

Responsabilidade:

    Atualizar o usuário 10

---

    DELETE /usuarios/10

Responsabilidade:

    Excluir o usuário 10

O cliente utiliza os recursos disponibilizados pela API sem precisar saber como essas operações são implementadas internamente.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **REST** | Estilo arquitetural utilizado em sistemas Web |
| **API REST** | API construída seguindo os princípios REST |
| **HTTP** | Protocolo utilizado para comunicação entre cliente e servidor |
| **Recurso** | Informação ou funcionalidade disponibilizada pela API |
| **URL** | Endereço utilizado para acessar um recurso |
| **Stateless** | Cada requisição é processada de maneira independente |
| **Cliente** | Sistema que realiza requisições para a API |
| **Servidor** | Sistema responsável por processar as requisições |
| **Cliente-Servidor** | Separação clara entre as responsabilidades de cada parte |
| **RESTful** | Serviço que segue os princípios definidos pelo REST |

---

## Visão Geral

    REST
     │
     ├── Comunicação através do HTTP
     │
     │      ├── GET
     │      ├── POST
     │      ├── PUT
     │      └── DELETE
     │
     ├── Recursos acessados através de URLs
     │
     ├── Stateless
     │      │
     │      └── Cada requisição é independente
     │
     └── Separação Cliente-Servidor
            │
            ├── Cliente não conhece
            │   detalhes internos da API
            │
            └── Servidor não depende
                da implementação do cliente

---

## Fluxo de uma API REST

    ┌─────────────────┐
    │     CLIENTE     │
    │                 │
    │ Web / Mobile /  │
    │ Outro sistema   │
    └────────┬────────┘
             │
             │ HTTP Request
             │
             │ GET / POST
             │ PUT / DELETE
             ▼
    ┌─────────────────┐
    │    API REST     │
    │                 │
    │ Processamento   │
    │ Regras          │
    │ Validações      │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐
    │ Banco de Dados  │
    │ Outros Serviços │
    └─────────────────┘
             │
             ▼
        HTTP Response
             │
             ▼
          CLIENTE