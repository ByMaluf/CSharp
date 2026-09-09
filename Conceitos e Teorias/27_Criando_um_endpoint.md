````markdown
# Criando o primeiro Endpoint GET

## 📋 Índice

1. [O que é um Endpoint?](#o-que-é-um-endpoint)
2. [Criando um Endpoint GET](#criando-um-endpoint-get)
3. [IActionResult](#iactionresult)
4. [Atributo HttpGet](#atributo-httpget)
5. [Retornando uma resposta com Ok](#retornando-uma-resposta-com-ok)
6. [Testando pelo Swagger](#testando-pelo-swagger)
7. [Utilizando Breakpoints](#utilizando-breakpoints)
8. [Testando pelo Postman](#testando-pelo-postman)
9. [Configurando rotas em letras minúsculas](#configurando-rotas-em-letras-minúsculas)
10. [Retornando objetos](#retornando-objetos)
11. [Conversão automática para JSON](#conversão-automática-para-json)
12. [Fluxo completo do Endpoint](#fluxo-completo-do-endpoint)
13. [Resumo](#resumo)

---

## O que é um Endpoint?

Dentro de um **Controller**, criamos métodos que representam os **Endpoints** da nossa API.

Um Endpoint representa uma funcionalidade específica que pode ser acessada por aplicações externas.

Por exemplo:

```text
UserController
│
├── GET    → Consultar usuário
├── POST   → Cadastrar usuário
├── PUT    → Atualizar usuário
└── DELETE → Excluir usuário
```

No código, um Endpoint é implementado através de um **método dentro do Controller**, juntamente com atributos que informam ao ASP.NET Core como aquele método poderá ser acessado.

---

## Criando um Endpoint GET

Como estudamos anteriormente, o método HTTP:

```http
GET
```

é utilizado para **recuperar informações**.

Dentro do `UserController`, podemos começar criando um método:

```csharp
public IActionResult Get()
{
    return Ok();
}
```

Temos:

```text
public
   ↓
Método pode ser acessado

IActionResult
   ↓
Tipo da resposta do Endpoint

Get
   ↓
Nome do método

()
   ↓
Parâmetros

return Ok()
   ↓
Resposta retornada
```

Porém, somente criar o método não é suficiente.

Precisamos informar ao ASP.NET Core que esse método representa um Endpoint HTTP do tipo `GET`.

---

## IActionResult

O Endpoint pode retornar:

```csharp
IActionResult
```

O `IActionResult` representa um **resultado de uma ação executada pelo Controller**.

Exemplo:

```csharp
public IActionResult Get()
{
    return Ok();
}
```

Como declaramos:

```csharp
IActionResult
```

o método precisa retornar um resultado compatível.

Por isso utilizamos:

```csharp
return
```

Exemplo:

```csharp
return Ok();
```

O `IActionResult` permite retornar diferentes tipos de respostas HTTP, como:

```text
Ok()
BadRequest()
NotFound()
Unauthorized()
NoContent()
```

Cada uma pode representar um resultado diferente da requisição.

---

## Atributo HttpGet

Não basta criar um método dentro do Controller.

Precisamos informar **qual método HTTP será utilizado para acessar aquele Endpoint**.

Para isso, utilizamos atributos.

Para um Endpoint `GET`:

```csharp
[HttpGet]
public IActionResult Get()
{
    return Ok();
}
```

O atributo:

```csharp
[HttpGet]
```

informa ao ASP.NET Core:

> Este método representa um Endpoint HTTP do tipo GET.

Existem atributos equivalentes para outros métodos HTTP:

```csharp
[HttpGet]
[HttpPost]
[HttpPut]
[HttpDelete]
```

Relacionando com o que estudamos:

| Atributo | Método HTTP | Operação comum |
|---|---|---|
| `[HttpGet]` | GET | Recuperar informações |
| `[HttpPost]` | POST | Criar um recurso |
| `[HttpPut]` | PUT | Atualizar um recurso |
| `[HttpDelete]` | DELETE | Excluir um recurso |

---

## Retornando uma resposta com Ok

Podemos retornar:

```csharp
return Ok();
```

O método:

```csharp
Ok()
```

é disponibilizado através do `ControllerBase`, do qual nosso Controller herda.

Lembrando da estrutura:

```csharp
public class UserController : ControllerBase
```

Temos:

```text
UserController
      │
      │ herda de
      ▼
ControllerBase
      │
      └── Ok()
```

Por isso podemos utilizar diretamente:

```csharp
Ok()
```

---

### Retornando dados

Também podemos passar uma informação para o `Ok()`:

```csharp
[HttpGet]
public IActionResult Get()
{
    return Ok("Ellison");
}
```

Nesse caso, a API retornará:

```text
Status Code: 200 OK

Body:
Ellison
```

Portanto:

```csharp
Ok()
```

representa uma resposta HTTP:

```http
200 OK
```

E:

```csharp
Ok("Ellison")
```

representa:

```text
200 OK
   +
"Ellison"
```

---

## Testando pelo Swagger

Depois de criar o Endpoint, podemos executar a API e utilizar o **Swagger** para testá-lo.

O Swagger identifica automaticamente o Endpoint:

```http
GET /api/user
```

Podemos utilizar:

```text
Try it out
    ↓
Execute
```

A requisição será enviada para a API.

Fluxo:

```text
Swagger
   │
   │ GET /api/user
   ▼
UserController
   │
   ▼
Get()
   │
   ▼
return Ok("Ellison")
   │
   ▼
200 OK
   │
   ▼
"Ellison"
```

Assim, conseguimos testar nossos endpoints sem precisar desenvolver um Front-end.

---

## Utilizando Breakpoints

Também podemos utilizar **breakpoints** para acompanhar a execução do Endpoint.

Por exemplo:

```csharp
[HttpGet]
public IActionResult Get()
{
    return Ok("Ellison"); // breakpoint
}
```

Quando realizamos:

```http
GET /api/user
```

a execução será interrompida no breakpoint.

Podemos então analisar:

- valores das variáveis;
- fluxo da execução;
- dados recebidos;
- comportamento do método.

Utilizando:

```text
F10
```

podemos avançar para a próxima instrução.

Utilizando:

```text
F5
```

continuamos normalmente a execução.

Fluxo:

```text
Requisição
    ↓
Endpoint
    ↓
Breakpoint
    ↓
Código pausado
    ↓
Análise pelo desenvolvedor
    ↓
Continuação
    ↓
Resposta
```

---

## Testando pelo Postman

Além do Swagger, podemos utilizar ferramentas específicas para realizar requisições HTTP.

Uma das mais utilizadas é o **Postman**.

Para testar nosso Endpoint, precisamos informar:

### Método

```http
GET
```

### URL

```text
https://localhost:7081/api/user
```

Depois:

```text
Send
```

O Postman enviará:

```text
Postman
   │
   │ GET
   ▼
https://localhost:7081/api/user
   │
   ▼
API
```

E receberá uma resposta semelhante a:

```text
Status: 200 OK

Body:
Ellison
```

---

## Composição da URL

A URL utilizada para acessar nosso Endpoint é formada por diferentes partes.

Temos o endereço base:

```text
https://localhost:7081
```

E no `UserController`:

```csharp
[Route("api/[controller]")]
```

Como o Controller se chama:

```text
UserController
```

o `[controller]` será convertido para:

```text
User
```

Portanto:

```text
https://localhost:7081
          +
      /api/User
          ↓
https://localhost:7081/api/User
```

Temos então:

```text
URL BASE
https://localhost:7081

ROTA
/api/User

URL DO ENDPOINT
https://localhost:7081/api/User
```

---

## Configurando rotas em letras minúsculas

Por padrão, dependendo da configuração da aplicação, a rota pode aparecer como:

```text
/api/User
```

Por convenção, é muito comum utilizar URLs somente com letras minúsculas:

```text
/api/user
```

Podemos configurar isso no `Program.cs`.

Exemplo:

```csharp
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
```

Também pode ser escrito de forma mais compacta:

```csharp
builder.Services.AddRouting(options =>
    options.LowercaseUrls = true);
```

A propriedade:

```csharp
LowercaseUrls
```

é um `bool`.

Portanto:

```csharp
options.LowercaseUrls = true;
```

determina que as URLs geradas pela aplicação utilizem letras minúsculas.

Resultado:

```text
ANTES

/api/User


DEPOIS

/api/user
```

---

## Retornando objetos

Um Endpoint não precisa retornar apenas uma `string`.

Também podemos retornar objetos.

Imagine uma classe:

```csharp
public class Response
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

Podemos criar um objeto:

```csharp
var response = new Response
{
    Name = "Ellison",
    Age = 7
};
```

E retorná-lo:

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

Agora estamos retornando:

```text
Ok
 ↓
Objeto Response
```

---

## Conversão automática para JSON

Quando retornamos um objeto através de um Endpoint, o ASP.NET Core pode **serializar automaticamente esse objeto para JSON**.

Temos no C#:

```csharp
var response = new Response
{
    Name = "Ellison",
    Age = 7
};
```

E retornamos:

```csharp
return Ok(response);
```

A resposta HTTP será semelhante a:

```json
{
  "name": "Ellison",
  "age": 7
}
```

Fluxo:

```text
Objeto C#
    │
    ▼
return Ok(response)
    │
    ▼
ASP.NET Core
    │
    ▼
Serialização
    │
    ▼
JSON
    │
    ▼
Response
```

Isso significa que não precisamos converter manualmente o objeto para JSON para realizar uma resposta comum da API.

---

## Response Body

Quando retornamos:

```csharp
return Ok(response);
```

o objeto serializado é enviado no **Body da resposta HTTP**.

Lembrando:

```text
RESPONSE
│
├── Status Code
│      └── 200 OK
│
├── Headers
│
└── Body
       │
       └── JSON
```

Exemplo:

```http
200 OK
```

Body:

```json
{
  "name": "Ellison",
  "age": 7
}
```

Tanto o Swagger quanto o Postman permitem visualizar esse conteúdo.

---

## Fluxo completo do Endpoint

Podemos juntar tudo que estudamos:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
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
}
```

Fluxo da requisição:

```text
CLIENTE
Swagger / Postman / Front-end
          │
          │
          │ GET
          ▼
https://localhost:7081/api/user
          │
          ▼
     UserController
          │
          ▼
       [HttpGet]
          │
          ▼
         Get()
          │
          ▼
    Cria Response
          │
          ▼
   return Ok(response)
          │
          ▼
     ASP.NET Core
          │
          ├── Status → 200 OK
          │
          └── Body → JSON
          │
          ▼
        CLIENTE
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Endpoint** | Método do Controller que disponibiliza uma funcionalidade da API |
| **GET** | Método HTTP utilizado para recuperar informações |
| **[HttpGet]** | Identifica o método como um Endpoint GET |
| **IActionResult** | Representa o resultado retornado pela ação do Controller |
| **Ok()** | Retorna uma resposta HTTP `200 OK` |
| **Ok(valor)** | Retorna `200 OK` juntamente com dados no Body |
| **ControllerBase** | Disponibiliza métodos como `Ok()` para o Controller |
| **Swagger** | Permite visualizar e testar os endpoints da API |
| **Postman** | Ferramenta utilizada para realizar e testar requisições HTTP |
| **Breakpoint** | Interrompe temporariamente a execução para permitir análise do código |
| **LowercaseUrls** | Configuração para gerar URLs utilizando letras minúsculas |
| **Serialização** | Conversão de um objeto C# para um formato de transporte, como JSON |
| **Response Body** | Corpo da resposta que contém os dados retornados |

---

## Visão Geral

```text
Controller
    │
    ▼
[HttpGet]
    │
    ▼
IActionResult Get()
    │
    ▼
Executa a funcionalidade
    │
    ▼
return Ok(response)
    │
    ├──────────────┐
    │              │
    ▼              ▼
200 OK         Objeto C#
                   │
                   ▼
             Serialização
                   │
                   ▼
                  JSON
    │              │
    └───────┬──────┘
            ▼
        RESPONSE
            │
            ▼
Swagger / Postman / Front-end
```

> **Em resumo:** um Endpoint é implementado como um método dentro do Controller. O atributo `[HttpGet]` informa que ele responde a requisições `GET`, o `IActionResult` representa o resultado da operação e métodos como `Ok()` permitem retornar Status Codes e dados. Quando retornamos um objeto, o ASP.NET Core pode serializá-lo automaticamente para JSON e enviá-lo no Body da resposta.
````
