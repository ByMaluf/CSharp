# POST de Despesas, RequestExpenseJson e PaymentType

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo do endpoint POST](#objetivo-do-endpoint-post)
3. [Criando o ExpensesController](#criando-o-expensescontroller)
4. [Criando o endpoint de registro](#criando-o-endpoint-de-registro)
5. [Projeto Communication e Requests](#projeto-communication-e-requests)
6. [RequestExpenseJson](#requestexpensejson)
7. [Campos da despesa](#campos-da-despesa)
8. [Por que utilizar decimal](#por-que-utilizar-decimal)
9. [Criando o enum PaymentType](#criando-o-enum-paymenttype)
10. [Recebendo o body com FromBody](#recebendo-o-body-com-frombody)
11. [Teste no Swagger](#teste-no-swagger)
12. [Resumo](#resumo)
13. [Visão Geral](#visão-geral)

---

## Contexto da aula

Nesta aula começa a implementação dos endpoints da aplicação `CashFlow`.

O primeiro endpoint criado será responsável por registrar uma despesa através de uma requisição HTTP `POST`.

Além do Controller, também é criada no projeto `Communication` a classe que representa os dados recebidos no corpo da requisição.

O foco da aula é:

```text
API
↓
ExpensesController
↓
POST
↓
RequestExpenseJson
```

A regra de negócio e a classe de resposta serão adicionadas em uma aula posterior.

---

## Objetivo do endpoint POST

O endpoint criado terá como responsabilidade receber as informações necessárias para cadastrar uma nova despesa.

A operação utilizada é:

```http
POST /api/expenses
```

A ideia apresentada é:

```text
Cliente
   │
   │ JSON com os dados da despesa
   ▼
POST /api/expenses
   │
   ▼
ExpensesController
```

Como se trata da criação de um recurso, o Controller inicialmente utiliza `Created(...)` como retorno.

Nesta aula, porém, a resposta definitiva ainda não é implementada.

---

## Criando o ExpensesController

Dentro do projeto de API, na pasta `Controllers`, a aula cria um Controller vazio do tipo API.

No Visual Studio:

```text
Controllers
↓
Add
↓
Controller
↓
API
↓
API Controller - Empty
```

O nome escolhido é:

```csharp
ExpensesController
```

A aula reforça o cuidado de selecionar um Controller de **API**, e não um Controller MVC.

---

## Criando o endpoint de registro

Dentro de `ExpensesController`, é criado um endpoint `POST`:

```csharp
[HttpPost]
public IActionResult Register()
{
    return Created();
}
```

A função recebe o nome `Register`, pois sua responsabilidade será registrar uma despesa.

O retorno `Created` é colocado inicialmente para manter a ação válida enquanto o restante do fluxo ainda está sendo desenvolvido.

---

## Projeto Communication e Requests

As informações recebidas pela API não são declaradas diretamente dentro do Controller.

Seguindo a separação apresentada nas aulas anteriores, o projeto:

```text
CashFlow.Communication
```

é responsável pelos contratos usados para entrada e saída de dados.

Dentro dele é criada a pasta:

```text
Requests
```

Estrutura:

```text
CashFlow.Communication
└── Requests
```

Dentro dessa pasta é criada a classe de requisição da despesa.

---

## RequestExpenseJson

A aula utiliza uma nomenclatura que deixa claro o papel da classe.

O padrão explicado é semelhante a:

```text
Request + Expense + Json
```

Resultando em:

```csharp
RequestExpenseJson
```

A ideia é que, ao visualizar o nome da classe, já seja possível identificar:

```text
Request → dados recebidos pela API
Expense → recurso associado à despesa
Json    → formato utilizado na comunicação
```

A classe precisa ser `public`, pois será utilizada por outro projeto, o projeto de API.

---

## Campos da despesa

A classe de Request recebe as informações necessárias para registrar uma despesa.

Estrutura apresentada na aula:

```csharp
public class RequestExpenseJson
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
}
```

### Title

Representa o título da despesa.

```csharp
public string Title { get; set; } = string.Empty;
```

A inicialização com `string.Empty` evita que a propriedade comece com valor `null`.

---

### Description

A descrição é apresentada como uma informação opcional.

```csharp
public string? Description { get; set; }
```

Ela pode ser utilizada para observações, notas ou informações adicionais sobre a despesa.

---

### Date

Representa a data da despesa.

```csharp
public DateTime Date { get; set; }
```

O Swagger exibirá esse campo como data e hora.

---

### Amount

Representa o valor da despesa.

```csharp
public decimal Amount { get; set; }
```

Exemplos de valores:

```text
10.79
1359.00
250.50
```

---

### PaymentType

Representa a forma de pagamento utilizada.

```csharp
public PaymentType PaymentType { get; set; }
```

Como a forma de pagamento possui um conjunto fechado de opções, a aula utiliza um `enum`.

---

## Por que utilizar decimal

Para o valor monetário da despesa, a aula utiliza:

```csharp
decimal
```

Isso permite armazenar valores com casas decimais.

Exemplo:

```csharp
public decimal Amount { get; set; }
```

A motivação apresentada é que uma despesa pode possuir valores como:

```text
R$ 10,79
R$ 10,71
```

Portanto, um tipo inteiro não seria suficiente.

---

## Criando o enum PaymentType

Para representar as formas de pagamento, é criada no projeto `Communication` uma pasta específica para enums.

Estrutura:

```text
CashFlow.Communication
├── Requests
└── Enums
```

Dentro dessa pasta é criado o enum:

```csharp
public enum PaymentType
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    ElectronicTransfer = 3
}
```

As opções apresentadas são:

| Valor | Significado |
|---:|---|
| `0` | Dinheiro |
| `1` | Cartão de crédito |
| `2` | Cartão de débito |
| `3` | Transferência eletrônica |

---

## Por que utilizar enum

A aula apresenta `enum` como uma forma de restringir um valor a um conjunto conhecido de opções.

Em vez de utilizar números sem significado explícito:

```text
0
1
2
3
```

é possível associá-los a nomes:

```text
0 → Cash
1 → CreditCard
2 → DebitCard
3 → ElectronicTransfer
```

Assim, o código passa a expressar melhor o significado daquele valor.

---

## Importando o enum na Request

Como `PaymentType` está em outro namespace, a classe de Request precisa importar esse namespace.

Conceitualmente:

```csharp
using CashFlow.Communication.Enums;
```

Depois disso, a propriedade pode utilizar o tipo normalmente:

```csharp
public PaymentType PaymentType { get; set; }
```

---

## Recebendo o body com FromBody

Depois de criar a Request, o Controller passa a recebê-la no corpo da requisição.

Exemplo:

```csharp
[HttpPost]
public IActionResult Register([FromBody] RequestExpenseJson request)
{
    return Created();
}
```

O fluxo passa a ser:

```text
JSON enviado pelo cliente
        │
        ▼
      Body
        │
        ▼
[FromBody]
        │
        ▼
RequestExpenseJson
```

O ASP.NET Core realiza o mapeamento dos valores do JSON para as propriedades da classe.

---

## Exemplo de JSON

Com os campos apresentados na aula, a requisição pode assumir conceitualmente esta estrutura:

```json
{
  "title": "Apple",
  "description": "Novo iPhone",
  "date": "2025-01-01T00:00:00",
  "amount": 1359,
  "paymentType": 2
}
```

Nesse exemplo:

```text
paymentType = 2
```

corresponde a:

```text
DebitCard
```

---

## Teste no Swagger

A aplicação é executada e o Swagger exibe o novo endpoint `POST` de despesas.

O contrato mostra os campos:

```text
Title
Description
Date
Amount
PaymentType
```

No teste realizado na aula, são enviados valores semelhantes a:

```text
Title       → Apple
Description → Novo iPhone
Amount      → 1359
PaymentType → 2
```

Com um breakpoint no Controller, é possível verificar que o ASP.NET Core preenche corretamente a instância de `RequestExpenseJson`.

O valor:

```text
2
```

é interpretado como:

```text
DebitCard
```

confirmando o funcionamento do `enum` durante a desserialização da requisição.

---

## O que ainda não foi implementado

Nesta aula ainda não são criadas:

```text
Regra de negócio
Resposta do endpoint
Persistência da despesa
Validações
```

O foco é apenas montar o contrato de entrada e confirmar que a API consegue receber os dados corretamente.

A sequência indicada pela aula é:

```text
Request
↓
Controller
↓
próxima aula: regra de negócio
↓
próxima aula: Response
```

---

## Resumo

| Elemento | Responsabilidade |
|---|---|
| `ExpensesController` | Receber chamadas relacionadas a despesas |
| `[HttpPost]` | Definir o endpoint de criação |
| `[FromBody]` | Ler os dados enviados no corpo da requisição |
| `RequestExpenseJson` | Representar os dados necessários para registrar uma despesa |
| `Title` | Título da despesa |
| `Description` | Descrição opcional |
| `Date` | Data da despesa |
| `Amount` | Valor monetário |
| `PaymentType` | Forma de pagamento |
| `decimal` | Representar valores com casas decimais |
| `enum` | Restringir a forma de pagamento a opções conhecidas |

### PaymentType

```text
Cash               = 0
CreditCard         = 1
DebitCard          = 2
ElectronicTransfer = 3
```

---

## Visão Geral

```text
CLIENTE
   │
   │ POST /api/expenses
   │
   │ JSON
   ▼
┌─────────────────────────────┐
│       CashFlow.API          │
│                             │
│    ExpensesController       │
│          │                  │
│       [FromBody]            │
└──────────┼──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│ CashFlow.Communication      │
│                             │
│ RequestExpenseJson          │
│ ├── Title                   │
│ ├── Description             │
│ ├── Date                    │
│ ├── Amount                  │
│ └── PaymentType             │
│                             │
│ PaymentType enum            │
│ ├── Cash = 0                │
│ ├── CreditCard = 1          │
│ ├── DebitCard = 2           │
│ └── ElectronicTransfer = 3  │
└─────────────────────────────┘
```

> **Em resumo:** a aula cria o primeiro endpoint do `CashFlow`, um `POST` para registrar despesas. Os dados recebidos pelo body são representados por `RequestExpenseJson`, enquanto a forma de pagamento é restringida através do enum `PaymentType`. O Swagger é utilizado para confirmar que o JSON enviado é corretamente transformado em um objeto C# dentro do Controller.