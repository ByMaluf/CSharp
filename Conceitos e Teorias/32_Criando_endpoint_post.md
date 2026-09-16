````markdown
# Endpoint POST e Request Body

## 📋 Índice

1. [Criando um Endpoint POST](#criando-um-endpoint-post)
2. [Status 201 Created](#status-201-created)
3. [Organizando Requests e Responses](#organizando-requests-e-responses)
4. [Criando uma classe de Request](#criando-uma-classe-de-request)
5. [Inicializando Strings com String.Empty](#inicializando-strings-com-stringempty)
6. [Recebendo dados pelo Body](#recebendo-dados-pelo-body)
7. [Testando o POST pelo Swagger](#testando-o-post-pelo-swagger)
8. [Testando o POST pelo Postman](#testando-o-post-pelo-postman)
9. [JSON no Body da Request](#json-no-body-da-request)
10. [Created e o retorno 204](#created-e-o-retorno-204)
11. [Sobrecarga de métodos](#sobrecarga-de-métodos)
12. [Retornando dados com Created](#retornando-dados-com-created)
13. [Criando uma classe de Response](#criando-uma-classe-de-response)
14. [Documentando o retorno 201 no Swagger](#documentando-o-retorno-201-no-swagger)
15. [Fluxo completo do POST](#fluxo-completo-do-post)
16. [Resumo](#resumo)

---

## Criando um Endpoint POST

Até agora trabalhamos principalmente com:

```http
GET
```

utilizado para recuperar informações.

Agora vamos criar um Endpoint:

```http
POST
```

Nesse exemplo, o objetivo será **cadastrar um novo usuário**.

Dentro do `UserController`, podemos criar:

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
    │
    └── Define que o Endpoint utiliza POST

Create()
    │
    └── Método responsável pela criação

IActionResult
    │
    └── Tipo do resultado retornado
```

Como estamos dentro do:

```text
UserController
```

o contexto do método será a criação de um usuário.

---

## GET x POST

Podemos começar a visualizar a diferença:

```text
UserController
│
├── GET  /api/user
│       └── Recuperar informações
│
└── POST /api/user
        └── Criar um novo usuário
```

Observe que podemos ter:

```text
/api/user
```

para os dois Endpoints.

O que diferencia a operação é o **método HTTP**:

```http
GET /api/user
```

e:

```http
POST /api/user
```

---

## Status 201 Created

Quando uma requisição `POST` cria um novo recurso com sucesso, o Status Code utilizado no exemplo é:

```http
201 Created
```

Portanto:

```text
POST
  │
  ▼
Criar usuário
  │
  ▼
Criação realizada
  │
  ▼
201 Created
```

No Controller, temos o método:

```csharp
Created(...)
```

disponibilizado através do `ControllerBase`.

---

## Organizando Requests e Responses

Para organizar as classes utilizadas na comunicação da API, foi criada uma pasta:

```text
Communication
```

Dentro dela podemos separar:

```text
Communication
│
├── Requests
│
└── Responses
```

A ideia é:

```text
Requests
    │
    └── Dados que entram na API


Responses
    │
    └── Dados que a API devolve
```

Assim:

```text
CLIENTE
   │
   │ Request
   ▼
  API
   │
   │ Response
   ▼
CLIENTE
```

---

## Criando uma classe de Request

Como queremos cadastrar um usuário, precisamos definir quais informações o cliente deverá enviar.

No exemplo:

```text
Nome
E-mail
Senha
```

Podemos criar uma classe dentro de:

```text
Communication/Requests
```

Com um nome significativo, como apresentado na aula:

```csharp
RegisterUserRequestJson
```

A palavra:

```text
Request
```

deixa claro que essa classe representa dados que serão **recebidos pela API**.

E:

```text
Json
```

indica o formato utilizado na comunicação apresentada.

Exemplo:

```csharp
public class RegisterUserRequestJson
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Portanto:

```text
RegisterUserRequestJson
│
├── Name
├── Email
└── Password
```

---

## Inicializando Strings com String.Empty

Ao declarar:

```csharp
public string Name { get; set; }
```

o Visual Studio pode apresentar um aviso relacionado à possibilidade de a propriedade possuir:

```csharp
null
```

Imagine:

```csharp
var request = new RegisterUserRequestJson();
```

Nenhum valor foi informado para:

```text
Name
Email
Password
```

No exemplo apresentado na aula, uma forma utilizada para evitar que essas propriedades comecem como `null` é inicializá-las com:

```csharp
string.Empty
```

Exemplo:

```csharp
public string Name { get; set; } = string.Empty;
```

Fazemos o mesmo para as outras propriedades:

```csharp
public string Name { get; set; } = string.Empty;

public string Email { get; set; } = string.Empty;

public string Password { get; set; } = string.Empty;
```

Assim, inicialmente:

```text
Name
↓
""

Email
↓
""

Password
↓
""
```

em vez de:

```text
null
```

---

## null x string.Empty

Conceitualmente:

```csharp
null
```

significa ausência de uma referência/valor.

Enquanto:

```csharp
string.Empty
```

representa uma string vazia:

```text
""
```

Na situação mostrada na aula, inicializar com:

```csharp
string.Empty
```

evita tentar executar operações de `string` sobre uma propriedade que esteja `null`.

---

## Recebendo dados pelo Body

Agora podemos utilizar nossa classe como parâmetro do Endpoint:

```csharp
[HttpPost]
public IActionResult Create(RegisterUserRequestJson request)
{
    return Created();
}
```

Nesse caso, queremos receber um objeto contendo:

```text
Name
Email
Password
```

através do:

```text
Body
```

da requisição.

Essa é a terceira forma de envio de informações apresentada nas aulas:

```text
Dados para um Endpoint
         │
    ┌────┼─────┐
    │    │     │
    ▼    ▼     ▼
  URL  Header Body
```

Nas aulas anteriores vimos:

```text
URL
│
├── Query String
└── Path

Headers
└── Cabeçalho
```

Agora temos:

```text
Body
└── Corpo da Request
```

---

## Estrutura da Request

Para cadastrar um usuário, teremos conceitualmente:

```text
POST /api/user
       │
       ▼
Request Body
       │
       ▼
{
  "name": "...",
  "email": "...",
  "password": "..."
}
```

O ASP.NET Core recebe esses dados e os disponibiliza através do objeto:

```csharp
request
```

Então:

```text
JSON
 │
 ▼
RegisterUserRequestJson
 │
 ├── request.Name
 ├── request.Email
 └── request.Password
```

---

## Testando o POST pelo Swagger

Ao executar a aplicação, o Swagger identifica o Endpoint:

```http
POST /api/user
```

Ao expandi-lo, podemos visualizar:

```text
Request Body
```

com a estrutura esperada.

Por exemplo:

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

Utilizando:

```text
Try it out
```

podemos substituir os valores.

Exemplo:

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

A requisição é enviada para o Endpoint.

---

## Recebimento do JSON

Se colocarmos um breakpoint:

```csharp
[HttpPost]
public IActionResult Create(RegisterUserRequestJson request)
{
    // breakpoint

    return Created();
}
```

podemos analisar:

```csharp
request
```

e encontrar:

```text
request
│
├── Name     = "Wellison"
├── Email    = "wellison@wellison.com"
└── Password = "12345678"
```

Ou seja, o JSON enviado no Body foi transformado em um objeto C#.

Fluxo:

```text
JSON enviado
      │
      ▼
Request Body
      │
      ▼
ASP.NET Core
      │
      ▼
RegisterUserRequestJson
      │
      ▼
request
```

---

## Testando o POST pelo Postman

Também podemos realizar a mesma requisição através do Postman.

Primeiro selecionamos:

```http
POST
```

E utilizamos a URL:

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

e garantimos que o formato utilizado seja:

```text
JSON
```

Então podemos enviar:

```json
{
  "name": "Alison Arley",
  "email": "arley@gmail.com",
  "password": "abc1234"
}
```

Finalmente:

```text
Send
```

---

## JSON no Body da Request

Um objeto JSON utiliza:

```text
{
}
```

para delimitar o objeto.

Exemplo:

```json
{
  "name": "Alison"
}
```

Para múltiplas propriedades:

```json
{
  "name": "Alison",
  "email": "arley@gmail.com",
  "password": "abc1234"
}
```

A estrutura básica é:

```text
{
  "propriedade": "valor",
  "propriedade": "valor"
}
```

No nosso caso:

```text
JSON
│
├── name
├── email
└── password
```

corresponde à classe:

```text
RegisterUserRequestJson
│
├── Name
├── Email
└── Password
```

---

## Created e o retorno 204

Na situação demonstrada na aula, foi utilizado inicialmente:

```csharp
return Created();
```

sem nenhum argumento.

Ao testar, a resposta observada foi:

```http
204 No Content
```

A explicação apresentada é que essa chamada sem conteúdo representa uma operação bem-sucedida, porém sem nenhuma informação para devolver.

```text
Operação concluída
       │
       ▼
Nenhum conteúdo retornado
       │
       ▼
204 No Content
```

Para retornar efetivamente uma resposta de criação com conteúdo, a aula passa a utilizar outra sobrecarga de `Created`.

---

## Sobrecarga de métodos

Ao analisar o método:

```csharp
Created
```

dentro de `ControllerBase`, existem diferentes versões com o mesmo nome.

Por exemplo, conceitualmente:

```text
Created()
Created(string uri, object value)
Created(Uri uri, object value)
```

Isso introduz um conceito importante do C#:

> Podemos possuir métodos com o **mesmo nome**, desde que tenham listas de parâmetros diferentes.

Isso é chamado de:

```text
Sobrecarga de métodos
```

ou:

```text
Method Overloading
```

Exemplo conceitual:

```csharp
Metodo()
Metodo(string texto)
Metodo(int numero)
```

Todos possuem o mesmo nome:

```text
Metodo
```

mas parâmetros diferentes.

---

## Como o C# diferencia as sobrecargas?

O compilador observa os parâmetros.

Por exemplo:

```csharp
Created()
```

é diferente de:

```csharp
Created(string uri, object value)
```

que também é diferente de uma versão cuja assinatura utiliza:

```csharp
Uri
```

em vez de:

```csharp
string
```

Portanto:

```text
Mesmo nome
    +
Parâmetros diferentes
    ↓
Sobrecarga
```

---

## Retornando dados com Created

Na forma utilizada durante a aula, `Created` recebe uma URI e o conteúdo da resposta.

Como não será utilizada uma URI específica no exemplo, foi passado:

```csharp
string.Empty
```

como primeiro argumento.

Depois passamos o objeto da resposta:

```csharp
return Created(string.Empty, response);
```

Temos:

```text
Created(
    string.Empty,
    response
)
```

Onde:

```text
string.Empty
     │
     └── URI vazia utilizada no exemplo

response
     │
     └── Dados retornados
```

---

## Criando uma classe de Response

Agora precisamos definir quais informações serão devolvidas após cadastrar o usuário.

Para isso, dentro de:

```text
Communication
```

criamos:

```text
Responses
```

Nossa estrutura fica:

```text
Communication
│
├── Requests
│   └── RegisterUserRequestJson.cs
│
└── Responses
    └── RegisterUserResponseJson.cs
```

A classe de Response pode possuir:

```csharp
public class RegisterUserResponseJson
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
```

Portanto:

```text
RegisterUserResponseJson
│
├── Id
└── Name
```

---

## Request x Response

Agora temos uma separação clara.

### Request

Representa os dados recebidos:

```csharp
RegisterUserRequestJson
```

```text
Name
Email
Password
```

### Response

Representa os dados devolvidos:

```csharp
RegisterUserResponseJson
```

```text
Id
Name
```

Fluxo:

```text
CLIENTE
   │
   │ RegisterUserRequestJson
   │
   │ Name
   │ Email
   │ Password
   ▼
  API
   │
   │ Cadastra usuário
   ▼
RegisterUserResponseJson
   │
   │ Id
   │ Name
   ▼
CLIENTE
```

---

## Criando o objeto de Response

Dentro do Endpoint podemos criar:

```csharp
var response = new RegisterUserResponseJson
{
    Id = 1,
    Name = request.Name
};
```

Observe que:

```csharp
Name = request.Name
```

utiliza uma informação recebida através da Request.

Portanto:

```text
REQUEST

Name = "Wellison"
      │
      ▼
Processamento
      │
      ▼
RESPONSE

Id   = 1
Name = "Wellison"
```

---

## Retornando o Response

Depois:

```csharp
return Created(string.Empty, response);
```

O fluxo passa a ser:

```text
POST /api/user
      │
      ▼
Request Body
      │
      ▼
RegisterUserRequestJson
      │
      ▼
Create()
      │
      ▼
Cria RegisterUserResponseJson
      │
      ▼
Created(string.Empty, response)
      │
      ▼
201 Created
      │
      ▼
Response Body
```

---

## Documentando o retorno 201 no Swagger

Como aprendemos anteriormente, devemos documentar as respostas possíveis do Endpoint através de:

```csharp
[ProducesResponseType]
```

Como estamos retornando:

```text
201 Created
```

e o Body possui:

```csharp
RegisterUserResponseJson
```

podemos documentar:

```csharp
[ProducesResponseType(
    typeof(RegisterUserResponseJson),
    StatusCodes.Status201Created)]
```

Exemplo:

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

Agora o Swagger consegue mostrar tanto o formato da entrada quanto o formato da saída.

### Request Body

```json
{
  "name": "string",
  "email": "string",
  "password": "string"
}
```

### Response — 201 Created

```json
{
  "id": 0,
  "name": "string"
}
```

Isso documenta o contrato do Endpoint:

```text
O que enviar?
      ↓
RegisterUserRequestJson

O que posso receber?
      ↓
RegisterUserResponseJson

Qual Status Code?
      ↓
201 Created
```

---

## Fluxo completo do POST

Código conceitual completo:

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

Classe de Request:

```csharp
public class RegisterUserRequestJson
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
```

Classe de Response:

```csharp
public class RegisterUserResponseJson
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
```

---

## Visão Geral do Fluxo

```text
                  CLIENTE
                     │
                     │
                     │ POST /api/user
                     ▼
              ┌──────────────┐
              │ Request Body │
              └──────┬───────┘
                     │
                     ▼
          RegisterUserRequestJson
                     │
          ┌──────────┼──────────┐
          │          │          │
          ▼          ▼          ▼
        Name       Email     Password
                     │
                     ▼
                  Create()
                     │
                     ▼
            Processa o cadastro
                     │
                     ▼
         RegisterUserResponseJson
                     │
              ┌──────┴──────┐
              │             │
              ▼             ▼
             Id            Name
              │             │
              └──────┬──────┘
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

## As três formas estudadas até agora

Agora podemos juntar as aulas anteriores:

```text
               DADOS DA REQUEST
                      │
          ┌───────────┼───────────┐
          │           │           │
          ▼           ▼           ▼
         URL       HEADERS       BODY
          │                       │
    ┌─────┴─────┐                 │
    │           │                 │
    ▼           ▼                 ▼
  Query        Path             JSON
 String
```

Exemplos:

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
| **POST** | Método HTTP utilizado no exemplo para criar um novo recurso |
| **[HttpPost]** | Identifica o método do Controller como um Endpoint POST |
| **201 Created** | Status Code utilizado quando um recurso é criado com sucesso |
| **Body** | Corpo da requisição onde podem ser enviados dados |
| **Request Body** | Dados enviados pelo cliente no corpo da requisição |
| **JSON** | Formato utilizado no exemplo para enviar e receber objetos |
| **RegisterUserRequestJson** | Classe que representa os dados necessários para cadastrar um usuário |
| **Communication/Requests** | Pasta utilizada na organização das classes de entrada |
| **Communication/Responses** | Pasta utilizada na organização das classes de saída |
| **string.Empty** | Representa uma string vazia e foi utilizado para inicializar propriedades |
| **Sobrecarga** | Existência de métodos com o mesmo nome e diferentes parâmetros |
| **RegisterUserResponseJson** | Classe que representa os dados devolvidos após o cadastro |
| **Created(...)** | Método utilizado para produzir a resposta de criação |
| **ProducesResponseType** | Documenta no Swagger o tipo e Status Code da resposta |
| **Swagger** | Permite visualizar e testar o Request Body e a Response |
| **Postman** | Permite montar manualmente uma requisição POST com JSON no Body |

---

## Resumo do fluxo

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
Criação do usuário
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

> **Em resumo:** no Endpoint `POST`, podemos receber dados através do **Body da Request**. Esses dados em JSON são representados por uma classe de Request, como `RegisterUserRequestJson`. Depois do processamento, podemos criar uma classe específica de Response, como `RegisterUserResponseJson`, e devolver os dados juntamente com o Status Code **201 Created**. Com isso, passamos a ter uma separação clara entre **o que entra na API (Request)** e **o que sai dela (Response)**.
````
