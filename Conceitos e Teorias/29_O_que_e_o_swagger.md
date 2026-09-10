````markdown
# Swagger e Documentação dos Endpoints

## 📋 Índice

1. [O que é o Swagger?](#o-que-é-o-swagger)
2. [Swagger como documentação da API](#swagger-como-documentação-da-api)
3. [Visualizando um Endpoint](#visualizando-um-endpoint)
4. [Parâmetros no Swagger](#parâmetros-no-swagger)
5. [Documentando as respostas](#documentando-as-respostas)
6. [ProducesResponseType](#producesresponsetype)
7. [Documentando uma resposta sem conteúdo](#documentando-uma-resposta-sem-conteúdo)
8. [Documentando uma resposta com objeto](#documentando-uma-resposta-com-objeto)
9. [typeof](#typeof)
10. [Documentando múltiplas respostas](#documentando-múltiplas-respostas)
11. [Resumo](#resumo)

---

## O que é o Swagger?

Ao executar uma **ASP.NET Core Web API**, podemos acessar uma página do Swagger.

Exemplo:

```text
https://localhost:7081/swagger/index.html
```

O Swagger apresenta de forma visual e organizada as funcionalidades disponibilizadas pela API.

Podemos imaginá-lo como um **guia da API**:

```text
                  API
                   │
                   ▼
                Swagger
                   │
        ┌──────────┼──────────┐
        │          │          │
        ▼          ▼          ▼
    Endpoints   Parâmetros  Respostas
```

Ele não serve apenas para mostrar quais endpoints existem.

Também pode documentar:

- quais endpoints estão disponíveis;
- quais métodos HTTP são utilizados;
- quais parâmetros precisam ser enviados;
- quais tipos de dados são recebidos;
- quais Status Codes podem ser retornados;
- quais dados podem existir no corpo da resposta.

---

## Swagger como documentação da API

Uma API pode possuir diversos endpoints.

Por exemplo:

```text
GET     /api/user
POST    /api/user
PUT     /api/user
DELETE  /api/user
```

Somente conhecer essas URLs não é suficiente.

Quem utilizar a API também precisa saber:

```text
O que preciso enviar?
        ↓
Qual formato dos dados?
        ↓
O que posso receber?
        ↓
Quais Status Codes podem ser retornados?
```

O Swagger organiza essas informações e funciona como uma **documentação da API**.

Isso é especialmente importante quando diferentes desenvolvedores trabalham no mesmo projeto.

---

## Visualizando um Endpoint

Na aula anterior, criamos um Endpoint `GET`:

```csharp
[HttpGet]
public IActionResult Get()
{
    var response = new Response
    {
        Name = "Ellison",
        Age = 7
    };

    return Ok(response);
}
```

No Swagger, esse Endpoint pode aparecer como:

```http
GET /api/user
```

Ao expandi-lo, conseguimos visualizar informações relacionadas à requisição e à resposta.

Porém, o Swagger precisa ser corretamente configurado para representar aquilo que o Endpoint realmente pode retornar.

---

## Parâmetros no Swagger

Os parâmetros recebidos pelo método também podem aparecer automaticamente na documentação.

Por exemplo:

```csharp
[HttpGet]
public IActionResult Get(string name)
{
    return Ok();
}
```

Nesse caso, o Swagger identifica que o Endpoint recebe:

```text
name
```

E disponibiliza um campo para informar esse valor durante o teste.

Exemplo:

```text
GET /api/user

Parameters
└── name: "Ellison"
```

Ao utilizar:

```text
Try it out
```

podemos preencher o parâmetro e executar a requisição.

```text
Swagger
   │
   │ name = "Ellison"
   ▼
GET /api/user
   │
   ▼
Endpoint
   │
   ▼
string name = "Ellison"
```

> A aula apenas introduz esse comportamento. Os diferentes tipos de parâmetros recebidos pelos endpoints serão estudados posteriormente.

---

## Documentando as respostas

Um ponto importante é que o Swagger precisa informar corretamente **quais respostas um Endpoint pode retornar**.

Imagine que a documentação informe:

```text
200 OK
```

Mas o código possa retornar:

```csharp
return NotFound();
```

Nesse caso, a execução pode resultar em:

```text
404 Not Found
```

mas a documentação continuará apresentando apenas:

```text
200 OK
```

Ou seja:

```text
DOCUMENTAÇÃO

200 OK


EXECUÇÃO REAL

404 Not Found
```

A documentação não está representando corretamente o comportamento do Endpoint.

Para melhorar isso, podemos declarar explicitamente as possíveis respostas.

---

## ProducesResponseType

Para documentar as respostas possíveis de um Endpoint, utilizamos o atributo:

```csharp
[ProducesResponseType]
```

Ele é colocado acima do método do Endpoint.

Exemplo:

```csharp
[HttpGet]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Get()
{
    return NoContent();
}
```

Esse atributo informa ao Swagger qual resposta aquele Endpoint pode produzir.

Estrutura:

```text
[HttpGet]
        ↓
Define o método HTTP

[ProducesResponseType(...)]
        ↓
Documenta uma possível resposta

Get()
        ↓
Implementação do Endpoint
```

---

## Documentando uma resposta sem conteúdo

Imagine um Endpoint que retorna:

```csharp
return NoContent();
```

O `NoContent()` representa:

```http
204 No Content
```

Nesse caso, não existe um objeto no corpo da resposta.

Podemos documentá-lo assim:

```csharp
[HttpGet]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Get()
{
    return NoContent();
}
```

O:

```csharp
StatusCodes
```

possui constantes que representam os códigos HTTP.

Por exemplo:

```csharp
StatusCodes.Status200OK
StatusCodes.Status201Created
StatusCodes.Status204NoContent
StatusCodes.Status400BadRequest
StatusCodes.Status404NotFound
StatusCodes.Status500InternalServerError
```

Assim, o Swagger poderá apresentar:

```text
Responses

204
No Content
```

---

## Documentando uma resposta com objeto

Agora imagine que o Endpoint retorne:

```csharp
return Ok(response);
```

Nesse caso, temos duas informações importantes:

```text
Status Code
    ↓
200 OK

Tipo do Body
    ↓
Response
```

Precisamos informar ao Swagger tanto o código quanto o tipo do objeto retornado.

Exemplo:

```csharp
[HttpGet]
[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
public IActionResult Get()
{
    var response = new Response
    {
        Name = "Ellison",
        Age = 7
    };

    return Ok(response);
}
```

Agora o Swagger sabe:

```text
Endpoint pode retornar
        │
        ├── Status Code → 200 OK
        │
        └── Body → Response
```

---

## typeof

No exemplo anterior utilizamos:

```csharp
typeof(Response)
```

O `typeof` permite obter o **tipo representado por uma classe**.

Nesse caso:

```csharp
typeof(Response)
```

está informando ao `ProducesResponseType`:

> O corpo dessa resposta possui o tipo `Response`.

Se tivermos:

```csharp
public class Response
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

e documentarmos:

```csharp
[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
```

o Swagger consegue entender a estrutura:

```text
Response
│
├── Name → string
└── Age  → int
```

E pode apresentar um exemplo semelhante a:

```json
{
  "name": "string",
  "age": 0
}
```

Assim, quem consulta a documentação consegue saber antecipadamente **qual é a estrutura dos dados retornados pela API**.

---

## Documentando múltiplas respostas

Um Endpoint não precisa retornar apenas um tipo de resposta.

Dependendo do resultado da operação, diferentes Status Codes podem ser utilizados.

Por exemplo, imagine o cadastro de um usuário.

Se tudo estiver correto:

```http
201 Created
```

Se os dados estiverem inválidos:

```http
400 Bad Request
```

Portanto:

```text
POST /api/user
       │
       ▼
Valida os dados
       │
   ┌───┴───┐
   │       │
Válido   Inválido
   │       │
   ▼       ▼
  201     400
Created  Bad Request
```

O Swagger deve documentar todas as respostas relevantes.

Para isso, podemos utilizar mais de um:

```csharp
[ProducesResponseType]
```

Exemplo:

```csharp
[HttpGet]
[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
public IActionResult Get()
{
    // implementação
}
```

Agora estamos informando duas possibilidades:

```text
200 OK
│
└── Response


400 Bad Request
│
└── string
```

No Swagger, teremos algo conceitualmente semelhante a:

```text
Responses

200
Success

{
  "name": "string",
  "age": 0
}


400
Bad Request

"string"
```

---

### Quantas respostas um Endpoint pode documentar?

Um Endpoint pode possuir várias respostas possíveis.

Por exemplo:

```csharp
[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
```

Cada atributo representa uma possibilidade de resposta.

```text
Endpoint
   │
   ├── 200 → Sucesso
   ├── 400 → Requisição inválida
   ├── 404 → Recurso não encontrado
   └── 500 → Erro interno
```

> Devemos documentar as respostas que realmente podem ocorrer naquele Endpoint.

---

## Tipo da resposta x Status Code

Ao utilizar:

```csharp
[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
```

estamos fornecendo duas informações:

```text
ProducesResponseType
        │
        ├── typeof(Response)
        │        ↓
        │   Tipo do conteúdo
        │
        └── StatusCodes.Status200OK
                 ↓
            Status Code
```

Quando não existe conteúdo para retornar, podemos informar apenas o Status Code:

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

Portanto:

```text
RESPOSTA COM BODY

[ProducesResponseType(
    typeof(Response),
    StatusCodes.Status200OK
)]


RESPOSTA SEM BODY

[ProducesResponseType(
    StatusCodes.Status204NoContent
)]
```

---

## Por que documentar corretamente?

Sem uma documentação adequada, outro desenvolvedor pode saber que existe:

```http
GET /api/user
```

mas ainda ter dúvidas:

```text
O que preciso enviar?

O que vou receber?

Qual é o formato da resposta?

Pode retornar erro?

Quais erros?

Qual Status Code representa sucesso?
```

Com o Swagger corretamente configurado:

```text
Swagger
   │
   ├── Endpoint
   ├── Método HTTP
   ├── Parâmetros
   ├── Status Codes
   └── Estrutura das respostas
```

Isso torna mais fácil entender e utilizar a API.

---

## Exemplo Completo

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult Get()
    {
        var response = new Response
        {
            Name = "Ellison",
            Age = 7
        };

        return Ok(response);
    }
}
```

O Swagger consegue representar:

```text
GET /api/user
      │
      ├── 200 OK
      │      │
      │      └── Response
      │             ├── name: string
      │             └── age: int
      │
      └── 400 Bad Request
             │
             └── string
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Swagger** | Ferramenta utilizada para visualizar e documentar a API |
| **Documentação da API** | Descreve endpoints, parâmetros e possíveis respostas |
| **Try it out** | Permite preencher informações e testar um Endpoint pelo Swagger |
| **ProducesResponseType** | Documenta uma possível resposta de um Endpoint |
| **StatusCodes** | Disponibiliza constantes correspondentes aos códigos HTTP |
| **typeof** | Permite informar o tipo de objeto relacionado à resposta |
| **200 OK** | Indica uma operação realizada com sucesso |
| **204 No Content** | Indica sucesso sem conteúdo no Body |
| **400 Bad Request** | Indica uma requisição inválida |
| **Response** | Classe utilizada no exemplo como estrutura do Body retornado |
| **Múltiplos ProducesResponseType** | Permitem documentar diferentes respostas possíveis para o mesmo Endpoint |

---

## Visão Geral

```text
                        API
                         │
                         ▼
                      Swagger
                         │
          Documentação dos Endpoints
                         │
         ┌───────────────┼────────────────┐
         │               │                │
         ▼               ▼                ▼
     Método HTTP      Parâmetros       Respostas
         │                                │
         ▼                                ▼
        GET                    [ProducesResponseType]
                                          │
                              ┌───────────┴───────────┐
                              │                       │
                              ▼                       ▼
                         Tipo do Body             Status Code
                              │                       │
                              ▼                       ▼
                      typeof(Response)              200
                              │
                              ▼
                       Estrutura JSON
```

### Relação com o código

```text
[HttpGet]
     │
     └── Define como o Endpoint será acessado

[ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
     │
     ├── Response → O que será retornado
     │
     └── 200      → Qual será o Status Code
     │
     ▼
public IActionResult Get()
     │
     ▼
return Ok(response)
```

> **Em resumo:** o Swagger funciona como a documentação visual da API. O atributo `[ProducesResponseType]` permite informar quais respostas um Endpoint pode produzir, especificando tanto o **Status Code** quanto, quando existir, o **tipo do conteúdo retornado**. Um mesmo Endpoint pode possuir várias respostas documentadas, deixando claro para quem consumir a API o que pode acontecer em cada situação.
````
