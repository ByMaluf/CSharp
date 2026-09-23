# Endpoint POST, Request e Response

## 📋 Índice

1. [Criando um Endpoint POST](#criando-um-endpoint-post)
2. [Status 201 Created](#status-201-created)
3. [Organizando Requests e Responses](#organizando-requests-e-responses)
4. [Criando uma classe de Request](#criando-uma-classe-de-request)
5. [Evitando valores null com string.Empty](#evitando-valores-null-com-stringempty)
6. [Recebendo dados pelo Body](#recebendo-dados-pelo-body)
7. [Testando pelo Swagger](#testando-pelo-swagger)
8. [Testando pelo Postman](#testando-pelo-postman)
9. [Sobrecarga de métodos](#sobrecarga-de-métodos)
10. [Created e retorno com conteúdo](#created-e-retorno-com-conteúdo)
11. [Criando uma classe de Response](#criando-uma-classe-de-response)
12. [Documentando a resposta no Swagger](#documentando-a-resposta-no-swagger)
13. [Fluxo completo](#fluxo-completo)
14. [Resumo](#resumo)

---

## Criando um Endpoint POST

O método HTTP:

```http
POST
```

é utilizado para **criar novos recursos**.

No contexto de um `UserController`, podemos utilizá-lo para cadastrar um novo usuário.

Exemplo:

```csharp
[HttpPost]
public IActionResult Create()
{
    return Created();
}
```

Temos:

```text
[HttpPost]
    ↓
Define que o Endpoint utiliza POST

Create()
    ↓
Método responsável pela criação

IActionResult
    ↓
Representa o resultado da operação
```

Podemos ter:

```text
GET  /api/user
POST /api/user
```

O caminho é o mesmo, porém o método HTTP define qual operação será executada.

---

## Status 201 Created

Quando um novo recurso é criado com sucesso, podemos utilizar:

```http
201 Created
```

Fluxo:

```text
POST
 ↓
Criar usuário
 ↓
Usuário criado
 ↓
201 Created
```

No ASP.NET Core, podemos utilizar:

```csharp
Created(...)
```

para representar esse tipo de resposta.

---

## Organizando Requests e Responses

Para organizar melhor as classes responsáveis pela comunicação da API, podemos criar:

```text
Communication
│
├── Requests
│
└── Responses
```

A ideia é separar:

```text
Requests
   ↓
Dados que entram na API

Responses
   ↓
Dados que saem da API
```

Fluxo:

```text
Cliente
   │
   │ Request
   ▼
  API
   │
   │ Response
   ▼
Cliente
```

---

## Criando uma classe de Request

Para cadastrar um usuário, podemos precisar receber:

```text
Nome
E-mail
Senha
```

Podemos criar uma classe:

```csharp
public class RegisterUserRequestJson
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Essa classe representa os dados esperados na requisição.

```text
RegisterUserRequestJson
│
├── Name
├── Email
└── Password
```

O nome deixa claro:

```text
RegisterUser
    ↓
Registrar usuário

Request
    ↓
Dados recebidos

Json
    ↓
Formato utilizado na comunicação
```

---

## Evitando valores null com string.Empty

Ao declarar:

```csharp
public string Name { get; set; }
```

a propriedade pode inicialmente possuir:

```csharp
null
```

Por exemplo:

```csharp
var request = new RegisterUserRequestJson();
```

Nesse caso, se nenhuma informação for atribuída, propriedades como:

```text
Name
Email
Password
```

podem não possuir valor.

Na abordagem apresentada na aula, podemos inicializar com:

```csharp
string.Empty
```

Exemplo:

```csharp
public string Name { get; set; } = string.Empty;
```

Assim, em vez de:

```csharp
null
```

o valor inicial será:

```text
""
```

ou seja, uma string vazia.

---

## null x string.Empty

Temos:

```csharp
null
```

representando ausência de valor.

Enquanto:

```csharp
string.Empty
```

representa:

```text
""
```

uma string vazia.

Conceitualmente:

```text
null
↓
Sem valor

string.Empty
↓
String existente, porém vazia
```

---

## Recebendo dados pelo Body

Agora podemos utilizar a classe de Request como parâmetro do Endpoint.

Exemplo:

```csharp
[HttpPost]
public IActionResult Create(RegisterUserRequestJson request)
{
    return Created();
}
```

Os dados serão enviados através do:

```text
Body
```

da requisição.

Essa é a terceira forma de envio de informações estudada:

```text
Dados da requisição
        │
   ┌────┼────┐
   │    │    │
   ▼    ▼    ▼
  URL Header Body
```

Nas aulas anteriores vimos:

```text
URL
├── Query String
└── Path

Headers
└── Cabeçalho
```

Agora:

```text
Body
└── Corpo da requisição
```

---

## Estrutura do Request Body

Uma requisição poderia ser:

```http
POST /api/user
```

Body:

```json
{
  "name": "Wellison",
  "email": "wellison@wellison.com",
  "password": "12345678"
}
```

O ASP.NET Core transforma esses dados no objeto:

```csharp
request
```

Assim:

```text
JSON
 ↓
RegisterUserRequestJson
 ↓
request
 ├── Name
 ├── Email
 └── Password
```

---

## Testando pelo Swagger

Ao executar a aplicação, o Swagger identifica:

```http
POST /api/user
```

e passa a exibir um:

```text
Request Body
```

com a estrutura esperada.

Exemplo:

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

Podemos utilizar:

```text
Try it out
```

e informar:

```json
{
  "name": "Wellison",
  "email": "wellison@wellison.com",
  "password": "12345678"
}
```

Depois:

```text
Execute
```

A requisição será enviada para o Endpoint.

---

## Testando pelo Postman

No Postman:

### Método HTTP

```http
POST
```

### URL

```text
https://localhost:7081/api/user
```

Depois acessamos:

```text
Body
```

Selecionamos:

```text
raw
```

e definimos o formato:

```text
JSON
```

Exemplo:

```json
{
  "name": "Alison Arley",
  "email": "arley@gmail.com",
  "password": "abc1234"
}
```

Ao clicar em:

```text
Send
```

o JSON será enviado no Body da requisição.

---

## JSON no Body

Um objeto JSON utiliza:

```text
{
}
```

Exemplo:

```json
{
  "name": "Alison"
}
```

Para várias propriedades:

```json
{
  "name": "Alison",
  "email": "arley@gmail.com",
  "password": "abc1234"
}
```

Estrutura:

```text
{
  "propriedade": "valor"
}
```

---

## Sobrecarga de métodos

Ao analisar o método:

```csharp
Created
```

existem diferentes versões desse mesmo método.

Por exemplo:

```text
Created()
Created(string uri, object value)
Created(Uri uri, object value)
```

Isso representa um conceito importante do C#:

```text
Sobrecarga de métodos
```

ou:

```text
Method Overloading
```

A sobrecarga permite criar métodos com o **mesmo nome**, desde que possuam parâmetros diferentes.

Exemplo:

```csharp
Metodo()
Metodo(string texto)
Metodo(int numero)
```

O compilador consegue identificar qual versão deve ser utilizada com base nos parâmetros enviados.

---

## Created e retorno com conteúdo

Na aula, ao utilizar:

```csharp
return Created();
```

sem parâmetros, a resposta observada foi:

```http
204 No Content
```

Ou seja:

```text
Operação concluída
        ↓
Sem conteúdo para retornar
        ↓
204 No Content
```

Para retornar um conteúdo junto com a criação, foi utilizada outra sobrecarga:

```csharp
Created(string uri, object value)
```

Como não foi utilizada uma URI específica, a aula utilizou:

```csharp
string.Empty
```

como primeiro parâmetro.

Exemplo:

```csharp
return Created(string.Empty, response);
```

Temos:

```text
string.Empty
     ↓
URI vazia

response
     ↓
Conteúdo retornado
```

---

## Criando uma classe de Response

Agora podemos criar uma classe específica para representar o que será devolvido pela API.

Exemplo:

```csharp
public class RegisterUserResponseJson
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
```

Estrutura:

```text
RegisterUserResponseJson
│
├── Id
└── Name
```

---

## Request x Response

Agora temos:

### Request

```csharp
RegisterUserRequestJson
```

contendo:

```text
Name
Email
Password
```

### Response

```csharp
RegisterUserResponseJson
```

contendo:

```text
Id
Name
```

Fluxo:

```text
CLIENTE
   │
   ▼
RegisterUserRequestJson
│
├── Name
├── Email
└── Password
   │
   ▼
  API
   │
   ▼
RegisterUserResponseJson
│
├── Id
└── Name
   │
   ▼
CLIENTE
```

---

## Criando o objeto de Response

Dentro do Endpoint:

```csharp
var response = new RegisterUserResponseJson
{
    Id = 1,
    Name = request.Name
};
```

Aqui:

```csharp
request.Name
```

vem dos dados enviados pelo cliente.

Fluxo:

```text
Request
Name = "Wellison"
      │
      ▼
Processamento
      │
      ▼
Response
Id   = 1
Name = "Wellison"
```

---

## Retornando 201 Created

Depois:

```csharp
return Created(string.Empty, response);
```

Agora temos:

```text
201 Created
      +
Response Body
```

Exemplo de resposta:

```json
{
  "id": 1,
  "name": "Wellison"
}
```

---

## Documentando a resposta no Swagger

Como estudado anteriormente, podemos utilizar:

```csharp
[ProducesResponseType]
```

Para documentar:

```text
Status Code
+
Tipo da resposta
```

Exemplo:

```csharp
[ProducesResponseType(
    typeof(RegisterUserResponseJson),
    StatusCodes.Status201Created)]
```

O Endpoint completo pode ficar semelhante a:

```csharp
[HttpPost]
[ProducesResponseType(
    typeof(RegisterUserResponseJson),
    StatusCodes.Status201Created)]
public IActionResult Create(RegisterUserRequestJson request)
{
    var response = new RegisterUserResponseJson
    {
        Id = 1,
        Name = request.Name
    };

    return Created(string.Empty, response);
}
```

Agora o Swagger consegue documentar:

```text
POST /api/user
│
├── Request Body
│   ├── name
│   ├── email
│   └── password
│
└── Response
    └── 201 Created
        ├── id
        └── name
```

---

## Swagger: entrada e saída

### Request

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

### Response

```json
{
  "id": 0,
  "name": "string"
}
```

Isso representa o contrato do Endpoint:

```text
O que enviar?
      ↓
RegisterUserRequestJson

O que receber?
      ↓
RegisterUserResponseJson

Qual Status Code?
      ↓
201 Created
```

---

## Fluxo completo

```text
CLIENTE
   │
   │ POST /api/user
   ▼
Request Body
   │
   ▼
RegisterUserRequestJson
   │
   ├── Name
   ├── Email
   └── Password
   │
   ▼
Create()
   │
   ▼
Processamento
   │
   ▼
RegisterUserResponseJson
   │
   ├── Id
   └── Name
   │
   ▼
Created(string.Empty, response)
   │
   ▼
201 Created
   │
   ▼
Response Body
   │
   ▼
CLIENTE
```

---

## As formas de envio estudadas até agora

Agora temos três formas principais estudadas:

```text
REQUEST
   │
   ├── URL
   │   ├── Query String
   │   └── Path
   │
   ├── Headers
   │
   └── Body
```

### Query String

```text
GET /api/user?id=7
```

### Path

```text
GET /api/user/7
```

### Header

```text
GET /api/user

id: 7
```

### Body

```http
POST /api/user
```

```json
{
  "name": "Wellison",
  "email": "wellison@email.com",
  "password": "12345678"
}
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **POST** | Método HTTP utilizado para criar novos recursos |
| **[HttpPost]** | Identifica um Endpoint como POST |
| **201 Created** | Status Code utilizado quando um recurso é criado com sucesso |
| **Body** | Corpo da requisição |
| **Request Body** | Dados enviados pelo cliente no corpo da requisição |
| **JSON** | Formato utilizado para enviar e receber os dados |
| **RegisterUserRequestJson** | Representa os dados recebidos para cadastrar um usuário |
| **RegisterUserResponseJson** | Representa os dados retornados após o cadastro |
| **Communication/Requests** | Organização das classes de entrada |
| **Communication/Responses** | Organização das classes de saída |
| **string.Empty** | Representa uma string vazia |
| **Sobrecarga** | Métodos com o mesmo nome e parâmetros diferentes |
| **Created(...)** | Cria uma resposta relacionada ao recurso criado |
| **ProducesResponseType** | Documenta o Status Code e o tipo da resposta no Swagger |
| **Swagger** | Permite testar e documentar a Request e a Response |
| **Postman** | Permite criar manualmente requisições HTTP com Body JSON |

---

## Visão Geral

```text
POST /api/user
      │
      ▼
   JSON Body
      │
      ▼
RegisterUserRequestJson
      │
      ▼
    Create()
      │
      ▼
Processamento
      │
      ▼
RegisterUserResponseJson
      │
      ▼
 201 Created
      │
      ▼
 JSON Response
```

> **Em resumo:** em um Endpoint `POST`, os dados podem ser enviados através do **Body da Request**. Esses dados são representados por uma classe específica de Request. Após o processamento, podemos construir uma classe de Response e retornar **201 Created** com os dados desejados. Isso cria uma separação clara entre **dados de entrada (Request)** e **dados de saída (Response)**.
