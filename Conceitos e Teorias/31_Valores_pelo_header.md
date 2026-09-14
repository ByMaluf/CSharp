# Recebendo Parâmetros com FromRoute, FromQuery e FromHeader

## 📋 Índice

1. [De onde vêm os parâmetros?](#de-onde-vêm-os-parâmetros)
2. [FromRoute](#fromroute)
3. [FromQuery](#fromquery)
4. [FromHeader](#fromheader)
5. [Por que FromHeader precisa ser explícito?](#por-que-fromheader-precisa-ser-explícito)
6. [Testando Headers no Swagger](#testando-headers-no-swagger)
7. [Testando Headers no Postman](#testando-headers-no-postman)
8. [Parâmetros obrigatórios](#parâmetros-obrigatórios)
9. [Parâmetros opcionais](#parâmetros-opcionais)
10. [Valor null](#valor-null)
11. [Comparando FromRoute, FromQuery e FromHeader](#comparando-fromroute-fromquery-e-fromheader)
12. [Nomeando melhor os Endpoints](#nomeando-melhor-os-endpoints)
13. [Resumo](#resumo)

---

## De onde vêm os parâmetros?

Na aula anterior, vimos que um Endpoint pode receber informações através da URL.

Agora vamos deixar explícito **de onde cada parâmetro deve ser obtido**.

No ASP.NET Core podemos utilizar atributos como:

```csharp
[FromRoute]
[FromQuery]
[FromHeader]
```

Cada um informa a origem do valor.

```text
Parâmetro
    │
    ├── FromRoute  → vem da rota
    ├── FromQuery  → vem da Query String
    └── FromHeader → vem do Header
```

---

## FromRoute

Quando um valor faz parte diretamente do caminho da URL, podemos utilizar:

```csharp
[FromRoute]
```

Exemplo:

```csharp
[HttpGet]
[Route("{id}/{nickname}")]
public IActionResult Get(
    [FromRoute] int id,
    [FromRoute] string nickname)
{
    return Ok();
}
```

A chamada poderia ser:

```text
GET /api/user/7/wellison
```

Temos:

```text
/api/user/7/wellison
          │    │
          │    └── nickname
          │
          └─────── id
```

O ASP.NET Core associa:

```text
{id}       → int id
{nickname} → string nickname
```

---

### O FromRoute é sempre obrigatório?

Na situação apresentada na aula, o ASP.NET Core consegue inferir que esses valores vêm da rota.

Por exemplo:

```csharp
[Route("{id}")]
public IActionResult Get(int id)
{
    return Ok();
}
```

Mesmo sem:

```csharp
[FromRoute]
```

ele consegue associar:

```text
/api/user/7
          │
          ▼
        int id
```

Porém, podemos escrever explicitamente:

```csharp
public IActionResult Get([FromRoute] int id)
```

Isso deixa claro no código de onde aquele dado está vindo.

---

## FromQuery

Quando os valores chegam através de Query String, podemos utilizar:

```csharp
[FromQuery]
```

Exemplo:

```csharp
[HttpGet]
public IActionResult Get(
    [FromQuery] int id,
    [FromQuery] string nickname)
{
    return Ok();
}
```

A requisição ficaria:

```text
GET /api/user?id=7&nickname=wellison
```

Temos:

```text
/api/user?id=7&nickname=wellison
          │
          ▼
     Query String
```

O ASP.NET Core associa:

```text
id=7
↓
int id

nickname=wellison
↓
string nickname
```

---

### Inferência do FromQuery

Na abordagem apresentada, se criarmos:

```csharp
public IActionResult Get(int id, string nickname)
```

sem definir outra origem, esses parâmetros podem ser interpretados como valores vindos da Query String.

Exemplo:

```text
/api/user?id=7&nickname=wellison
```

Porém, também podemos deixar isso explícito:

```csharp
public IActionResult Get(
    [FromQuery] int id,
    [FromQuery] string nickname)
```

---

## FromHeader

Também podemos receber valores através dos **Headers da requisição**.

Nesse caso, utilizamos:

```csharp
[FromHeader]
```

Exemplo:

```csharp
[HttpGet]
public IActionResult Get(
    [FromHeader] int id,
    [FromHeader] string nickname)
{
    return Ok();
}
```

Nesse caso, a URL pode continuar simples:

```text
GET /api/user
```

Os valores não estão no endereço.

Eles estão no cabeçalho da requisição:

```text
Headers
│
├── id: 7
└── nickname: wellison
```

Portanto:

```text
URL
/api/user

Headers
id: 7
nickname: wellison
```

---

## Por que FromHeader precisa ser explícito?

Um ponto importante da aula é que o ASP.NET Core consegue inferir algumas origens.

Por exemplo:

```text
Valor definido na rota
        ↓
Pode ser identificado como Route
```

Ou:

```text
Parâmetro simples sem rota correspondente
        ↓
Pode ser interpretado como Query String
```

Porém, para receber valores através dos Headers, precisamos informar explicitamente:

```csharp
[FromHeader]
```

Exemplo:

```csharp
public IActionResult Get([FromHeader] int id)
```

Sem isso, o framework não saberá automaticamente que queremos obter aquele valor do cabeçalho.

---

## Testando Headers no Swagger

Quando utilizamos:

```csharp
[FromHeader]
```

o Swagger também identifica a origem do parâmetro.

Exemplo:

```csharp
[HttpGet]
public IActionResult Get(
    [FromHeader] int id,
    [FromHeader] string nickname)
{
    return Ok();
}
```

No Swagger, podemos visualizar algo semelhante a:

```text
Parameters

id
integer
header

nickname
string
header
```

Ou seja, o Swagger informa que esses valores precisam ser enviados no:

```text
Header
```

Ao utilizar:

```text
Try it out
```

podemos informar os valores.

Exemplo:

```text
id = 7
nickname = wellison
```

A requisição será enviada para:

```text
GET /api/user
```

com os Headers:

```text
id: 7
nickname: wellison
```

---

## Testando Headers no Postman

Também podemos testar a mesma requisição pelo Postman.

Primeiro definimos:

```text
Método:

GET
```

Depois:

```text
URL:

https://localhost:7081/api/user
```

Em seguida, acessamos a área:

```text
Headers
```

E adicionamos:

| Key | Value |
|---|---|
| `id` | `70` |
| `nickname` | `Wellison` |

A requisição será:

```text
GET /api/user

Headers:
id: 70
nickname: Wellison
```

No código:

```csharp
public IActionResult Get(
    [FromHeader] int id,
    [FromHeader] string nickname)
```

teremos:

```text
id = 70

nickname = "Wellison"
```

---

## Parâmetros obrigatórios

No exemplo:

```csharp
public IActionResult Get(
    [FromHeader] int id,
    [FromHeader] string nickname)
```

o:

```csharp
string nickname
```

está sendo tratado como um valor obrigatório.

Se o Header:

```text
nickname
```

não for enviado, a requisição poderá ser considerada inválida.

No Swagger ou Postman, isso pode resultar em uma resposta de erro informando que o campo é obrigatório.

Conceitualmente:

```text
Requisição
    │
    ├── id enviado
    │
    └── nickname NÃO enviado
              │
              ▼
        Parâmetro obrigatório
              │
              ▼
             Erro
```

---

## Parâmetros opcionais

Se quisermos permitir que o parâmetro não seja enviado, podemos torná-lo **nullable**.

Exemplo:

```csharp
string? nickname
```

Então:

```csharp
[HttpGet]
public IActionResult Get(
    [FromHeader] int id,
    [FromHeader] string? nickname)
{
    return Ok();
}
```

O:

```text
?
```

após o tipo indica que aquela variável aceita:

```text
null
```

Portanto:

```csharp
string nickname
```

significa:

```text
Espera um valor
```

Enquanto:

```csharp
string? nickname
```

significa:

```text
Pode receber uma string
OU
pode receber null
```

---

## Valor null

Se o parâmetro opcional não for enviado:

```text
nickname
```

então seu valor será:

```csharp
null
```

Exemplo:

```csharp
[FromHeader] string? nickname
```

Se a requisição possuir apenas:

```text
id: 70
```

teremos no código:

```text
id = 70

nickname = null
```

Isso permite verificar se o cliente enviou ou não determinado dado.

Exemplo conceitual:

```text
nickname == null
        │
        ▼
O parâmetro não foi enviado
```

Assim, o `null` pode ser utilizado para diferenciar:

```text
Parâmetro enviado
        ↓
"Wellison"


Parâmetro não enviado
        ↓
null
```

---

## Comparando FromRoute, FromQuery e FromHeader

Agora podemos comparar as três formas estudadas.

---

### FromRoute

Código:

```csharp
[HttpGet]
[Route("{id}")]
public IActionResult Get([FromRoute] int id)
{
    return Ok();
}
```

Requisição:

```text
GET /api/user/7
```

O valor está no:

```text
Path da URL
```

---

### FromQuery

Código:

```csharp
[HttpGet]
public IActionResult Get([FromQuery] int id)
{
    return Ok();
}
```

Requisição:

```text
GET /api/user?id=7
```

O valor está na:

```text
Query String
```

---

### FromHeader

Código:

```csharp
[HttpGet]
public IActionResult Get([FromHeader] int id)
{
    return Ok();
}
```

Requisição:

```text
GET /api/user
```

Header:

```text
id: 7
```

O valor está no:

```text
Cabeçalho da requisição
```

---

## Visão comparativa

```text
FROM ROUTE
────────────────────────────

GET /api/user/7
              │
              ▼
           int id


FROM QUERY
────────────────────────────

GET /api/user?id=7
                  │
                  ▼
               int id


FROM HEADER
────────────────────────────

GET /api/user

Header:
id: 7
    │
    ▼
 int id
```

---

## Nomeando melhor os Endpoints

Outro ponto mostrado na aula é a importância de utilizar nomes mais significativos para os métodos.

Inicialmente tínhamos:

```csharp
Get()
```

Se o Endpoint recupera especificamente um usuário pelo ID, podemos utilizar:

```csharp
GetById()
```

Exemplo:

```csharp
[HttpGet]
[Route("{id}")]
public IActionResult GetById([FromRoute] int id)
{
    return Ok();
}
```

Isso deixa mais claro o objetivo daquele método.

Comparação:

```text
Get
↓
Nome genérico


GetById
↓
Recupera utilizando o ID
```

---

## Relação com GET, POST e PUT

Até aqui o foco está em:

```http
GET
```

e vimos formas de receber informações através de:

```text
Route
Query String
Headers
```

Mais à frente, ao estudar métodos como:

```http
POST
PUT
```

também será utilizado o:

```text
Body
```

da requisição.

Conceitualmente:

```text
GET
│
├── Route
├── Query
└── Header


POST / PUT
│
├── Route
├── Query
├── Header
└── Body
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **FromRoute** | Indica que o parâmetro será recebido através do Path da URL |
| **FromQuery** | Indica que o parâmetro será recebido pela Query String |
| **FromHeader** | Indica que o parâmetro será recebido através dos Headers |
| **Route** | Exemplo: `/api/user/7` |
| **Query String** | Exemplo: `/api/user?id=7` |
| **Header** | Informação enviada no cabeçalho HTTP |
| **Inferência** | O ASP.NET Core consegue identificar automaticamente algumas origens dos parâmetros |
| **FromHeader explícito** | Deve ser informado para indicar que o valor vem do Header |
| **string** | No exemplo da aula, representa um valor esperado/obrigatório |
| **string?** | Permite que o valor seja `null` |
| **null** | Pode indicar que um parâmetro opcional não foi enviado |
| **GetById** | Nome mais significativo para um Endpoint que busca um recurso através do ID |

---

## Visão Geral

```text
                 REQUISIÇÃO
                     │
          ┌──────────┼──────────┐
          │          │          │
          ▼          ▼          ▼
        Route      Query      Header
          │          │          │
          ▼          ▼          ▼
    [FromRoute] [FromQuery] [FromHeader]
          │          │          │
          └──────────┼──────────┘
                     ▼
                  Método
                     │
                     ▼
              Parâmetros C#
```

Exemplos:

```text
ROUTE
GET /api/user/7

[FromRoute] int id
```

```text
QUERY
GET /api/user?id=7

[FromQuery] int id
```

```text
HEADER
GET /api/user

id: 7

[FromHeader] int id
```

### Parâmetro opcional

```text
string nickname
       │
       ▼
Valor esperado
```

```text
string? nickname
        │
        ▼
Pode receber null
```

> **Em resumo:** os atributos `[FromRoute]`, `[FromQuery]` e `[FromHeader]` deixam explícito de onde os parâmetros de um Endpoint serão obtidos. Valores de rota e Query String podem ser inferidos em determinadas situações, mas informações vindas dos Headers precisam ser identificadas com `[FromHeader]`. Além disso, ao utilizar `?` em um tipo como `string?`, permitimos que o parâmetro receba `null`, tornando-o opcional na situação apresentada.