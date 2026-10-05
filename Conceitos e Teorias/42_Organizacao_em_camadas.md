# Organização em Camadas e Separação de Responsabilidades

## 📋 Índice

1. [Objetivo do módulo](#objetivo-do-módulo)
2. [Relembrando a primeira API](#relembrando-a-primeira-api)
3. [Requests e Responses](#requests-e-responses)
4. [O problema de concentrar tudo em um único projeto](#o-problema-de-concentrar-tudo-em-um-único-projeto)
5. [Divisão da aplicação em camadas](#divisão-da-aplicação-em-camadas)
6. [Camada de API](#camada-de-api)
7. [Camada de lógica de negócio](#camada-de-lógica-de-negócio)
8. [Camada de acesso a dados](#camada-de-acesso-a-dados)
9. [Camada de comunicação](#camada-de-comunicação)
10. [Fluxo entre as camadas](#fluxo-entre-as-camadas)
11. [Dependências entre os projetos](#dependências-entre-os-projetos)
12. [Benefícios da divisão em camadas](#benefícios-da-divisão-em-camadas)
13. [Resumo](#resumo)

---

## Objetivo do módulo

O novo módulo começa com foco em **organização de projetos e código**.

A proposta é estudar:

- técnicas de organização;
- princípios;
- boas práticas;
- organização do projeto como um todo;
- organização das próprias linhas de código.

A ideia é sair de uma aplicação pequena e concentrada em um único projeto para uma estrutura que consiga crescer de forma mais organizada.

---

## Relembrando a primeira API

Na primeira API criada no curso, várias responsabilidades ficaram dentro do mesmo projeto.

Ali foram trabalhados conceitos como:

```text
API
│
├── Controllers
├── Endpoints
├── Dados pela rota
├── Dados pelo Header
├── Dados pelo Body
├── Requests
└── Responses
```

Essa estrutura funcionava para uma aplicação pequena e didática.

Mas, conforme a aplicação cresce, começam a surgir novas responsabilidades.

---

## Requests e Responses

Quando a API precisava receber informações pelo Body da requisição, foram criadas classes de Request.

Exemplo conceitual:

```csharp
public class RegisterUserRequestJson
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Depois, o Endpoint recebia essa classe:

```csharp
[FromBody] RegisterUserRequestJson request
```

O .NET fazia a conversão dos dados enviados no Body para o objeto utilizado pela aplicação.

Para devolver informações, foi utilizada a mesma ideia com classes de Response.

Exemplo conceitual:

```csharp
public class RegisterUserResponseJson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

Na primeira API, essas classes ficaram organizadas dentro de uma pasta de comunicação, com separação entre Requests e Responses.

```text
API
└── Communication
    ├── Requests
    └── Responses
```

---

## O problema de concentrar tudo em um único projeto

Conforme a aplicação evolui, novas responsabilidades começam a aparecer.

Por exemplo:

```text
Receber requisição
       ↓
Validar dados
       ↓
Aplicar regra de negócio
       ↓
Enviar e-mail
       ↓
Criar documento
       ↓
Conectar ao banco de dados
       ↓
Persistir informações
```

Se tudo isso continuar dentro do mesmo projeto de API, o código cresce e fica mais difícil de organizar e localizar.

A aula apresenta como boa prática dividir a aplicação em **camadas**, em vez de manter tudo em um único projeto.

---

## Divisão da aplicação em camadas

Cada camada passa a possuir uma responsabilidade específica.

A estrutura apresentada na aula pode ser visualizada assim:

```text
                API
                 │
                 ▼
       Lógica de Negócio
                 │
                 ▼
       Acesso a Dados
                 │
                 ▼
          Banco de Dados
```

Além disso, existe uma camada de comunicação utilizada para transportar os dados entre partes da aplicação.

```text
             Communication
              ▲          ▲
              │          │
             API    Lógica de Negócio
```

---

## Camada de API

A API é o ponto de entrada das requisições.

Ela recebe informações vindas do cliente e encaminha essas informações para a camada responsável pela lógica de negócio.

Fluxo inicial:

```text
CLIENTE
   │
   ▼
  API
   │
   ▼
Lógica de Negócio
```

A API não deve concentrar toda a regra da aplicação.

Na estrutura apresentada, ela recebe a requisição e chama a camada seguinte.

---

## Camada de lógica de negócio

A camada de lógica de negócio fica responsável pelas regras da aplicação.

Exemplo apresentado:

```text
API recebe dados
       ↓
Lógica de Negócio
       ↓
Os dados são válidos?
```

Se os dados forem inválidos:

```text
Lógica de Negócio
       ↓
Retorna erros
       ↓
      API
```

Se os dados forem válidos:

```text
Lógica de Negócio
       ↓
Acesso a Dados
```

Portanto, essa camada funciona como o local onde as regras são avaliadas antes de continuar o processamento.

---

## Camada de acesso a dados

A camada de acesso a dados fica responsável pela comunicação com a persistência.

Na aula, ela é apresentada como a camada que conecta com o banco de dados e persiste as informações.

```text
Lógica de Negócio
       │
       ▼
Acesso a Dados
       │
       ▼
Banco de Dados
```

Uma observação importante da estrutura apresentada é que a API **não conhece diretamente** essa camada.

Quem conhece e utiliza a camada de acesso a dados é a camada de lógica de negócio.

---

## Camada de comunicação

Na primeira API, Requests e Responses ficavam dentro de uma pasta de comunicação no próprio projeto da API.

Com a divisão em camadas, a aula propõe transformar essa comunicação em um projeto separado.

Exemplo:

```text
Communication
│
├── Requests
└── Responses
```

Esse projeto pode ser utilizado tanto pela API quanto pela lógica de negócio.

Assim:

```text
             Communication
              ▲          ▲
              │          │
             API    Business Logic
```

Isso permite que as duas camadas utilizem os mesmos objetos para trocar informações.

---

## Fluxo entre as camadas

O fluxo completo apresentado pode ser representado assim:

```text
CLIENTE
   │
   ▼
  API
   │
   │ Request
   ▼
LÓGICA DE NEGÓCIO
   │
   ├── Dados inválidos
   │        │
   │        ▼
   │       API
   │        │
   │        ▼
   │      Erros
   │
   └── Dados válidos
            │
            ▼
      ACESSO A DADOS
            │
            ▼
      BANCO DE DADOS
```

A camada de comunicação participa da troca de dados entre a API e a lógica de negócio.

---

## Dependências entre os projetos

A relação explicada na aula é importante para entender quem conhece quem.

```text
API
│
├── conhece Communication
└── conhece Lógica de Negócio

Lógica de Negócio
│
├── conhece Communication
└── conhece Acesso a Dados

Acesso a Dados
│
└── conecta ao Banco de Dados
```

A API não precisa conhecer diretamente a camada de acesso a dados.

Visualmente:

```text
API ───────────────► Communication
 │
 ▼
Lógica de Negócio ─► Communication
 │
 ▼
Acesso a Dados
 │
 ▼
Banco de Dados
```

Essa divisão mantém cada parte focada na sua responsabilidade.

---

## Benefícios da divisão em camadas

A aula apresenta cinco benefícios principais.

### 1. Reutilização

Uma biblioteca criada para uma responsabilidade específica pode ser reutilizada em outros projetos.

Exemplo apresentado:

```text
Biblioteca de envio de e-mail
          │
      ┌───┴────┐
      ▼        ▼
 Projeto A  Projeto B
```

Assim, não é necessário reescrever o mesmo código em vários lugares.

---

### 2. Capacidade de manutenção

Separar responsabilidades facilita alterar apenas a parte necessária.

```text
Aplicação em camadas
       │
       ├── API
       ├── Negócio
       └── Dados
```

Se uma mudança pertence à camada de acesso a dados, por exemplo, é possível trabalhar naquela parte específica sem precisar alterar todo o restante da aplicação.

---

### 3. Escalabilidade

À medida que a aplicação cresce, novas funcionalidades podem ser adicionadas em estruturas separadas.

```text
Aplicação
│
├── API
├── Negócio
├── Dados
├── E-mail
└── Novas funcionalidades
```

Isso ajuda a evitar que todo o código seja concentrado no mesmo lugar.

---

### 4. Encapsulamento

Uma biblioteca pode esconder seu funcionamento interno e expor apenas o que precisa ser utilizado externamente.

```text
Código externo
     │
     ▼
Interface pública da biblioteca
     │
     ▼
Implementação interna
```

Quem utiliza a biblioteca não precisa conhecer todos os detalhes de como ela funciona internamente.

---

### 5. Colaboração

Em projetos com várias pessoas, a divisão facilita distribuir responsabilidades.

Em vez de todos trabalharem sobre a mesma parte do código:

```text
Pessoa A ─► API
Pessoa B ─► Negócio
Pessoa C ─► Dados
```

Cada pessoa pode trabalhar em uma parte diferente e depois integrar o resultado.

---

## Projeto único x aplicação em camadas

### Estrutura simples

```text
Projeto API
│
├── Controllers
├── Requests
├── Responses
├── Regras
├── Dados
└── Outras responsabilidades
```

Tudo fica concentrado em um projeto.

### Estrutura em camadas

```text
Solução
│
├── API
├── Communication
├── Business Logic
└── Data Access
```

Cada projeto possui uma responsabilidade mais específica.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Organização em camadas** | Divisão da aplicação em partes com responsabilidades específicas |
| **API** | Recebe requisições e chama a lógica de negócio |
| **Lógica de Negócio** | Aplica validações e regras da aplicação |
| **Acesso a Dados** | Responsável pela comunicação com a persistência/banco de dados |
| **Communication** | Projeto compartilhado para transportar Requests e Responses |
| **Request** | Representa informações recebidas pela aplicação |
| **Response** | Representa informações devolvidas pela aplicação |
| **Reutilização** | Permite utilizar bibliotecas em diferentes projetos |
| **Manutenção** | Facilita alterações em partes específicas da aplicação |
| **Escalabilidade** | Facilita adicionar novas funcionalidades conforme o sistema cresce |
| **Encapsulamento** | Esconde detalhes internos e expõe apenas o necessário |
| **Colaboração** | Facilita dividir responsabilidades entre pessoas do time |

---

## Visão Geral

```text
                       CLIENTE
                          │
                          ▼
                         API
                          │
            ┌─────────────┴─────────────┐
            │                           │
            ▼                           ▼
     Communication              Lógica de Negócio
                                        │
                              ┌─────────┴─────────┐
                              │                   │
                         inválido              válido
                              │                   │
                              ▼                   ▼
                             API          Acesso a Dados
                                                  │
                                                  ▼
                                            Banco de Dados
```

### Organização proposta

```text
Solução
│
├── API
├── Communication
├── Lógica de Negócio
└── Acesso a Dados
```

> **Em resumo:** conforme a aplicação cresce, concentrar todas as responsabilidades em um único projeto torna a organização mais difícil. A proposta apresentada é dividir a aplicação em camadas, onde a API recebe as requisições, a lógica de negócio aplica as regras, a camada de acesso a dados cuida da persistência e um projeto de comunicação compartilha Requests e Responses entre as partes necessárias. Essa separação traz benefícios como reutilização, manutenção, escalabilidade, encapsulamento e colaboração.
