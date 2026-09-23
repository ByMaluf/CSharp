# Endpoint PUT e Atualização de Recursos

## 📋 Índice

1. [O que é um Endpoint PUT?](#o-que-é-um-endpoint-put)
2. [Criando um Endpoint PUT](#criando-um-endpoint-put)
3. [Retorno 204 No Content](#retorno-204-no-content)
4. [Criando a classe de Request](#criando-a-classe-de-request)
5. [Recebendo dados pelo Body](#recebendo-dados-pelo-body)
6. [Testando pelo Swagger](#testando-pelo-swagger)
7. [Testando pelo Postman](#testando-pelo-postman)
8. [As duas formas de identificar o usuário](#as-duas-formas-de-identificar-o-usuário)
9. [Atualizando o próprio usuário](#atualizando-o-próprio-usuário)
10. [Atualizando outro usuário](#atualizando-outro-usuário)
11. [ID na rota](#id-na-rota)
12. [ID na rota x ID no Body](#id-na-rota-x-id-no-body)
13. [Fluxo completo](#fluxo-completo)
14. [Resumo](#resumo)

---

## O que é um Endpoint PUT?

O método HTTP:

```http
PUT
```

é utilizado quando queremos **atualizar um recurso que já existe**.

No contexto de um usuário, isso pode significar atualizar informações como:

- nome;
- e-mail;
- dados de perfil.

Exemplo:

```text
Usuário já existe
      ↓
Novos dados são enviados
      ↓
PUT
      ↓
Usuário atualizado
```

---

## Criando um Endpoint PUT

Dentro do `UserController`, podemos criar um método responsável pela atualização:

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Update()
{
    return NoContent();
}
```

Temos:

```text
[HttpPut]
    ↓
Define que o Endpoint utiliza PUT

Update()
    ↓
Método responsável pela atualização

NoContent()
    ↓
Resposta sem conteúdo
```

No Swagger, podemos ter:

```text
GET  /api/user
POST /api/user
PUT  /api/user
```

O caminho pode ser o mesmo, mas o método HTTP diferencia a operação.

---

## Retorno 204 No Content

Ao atualizar um recurso, geralmente não é necessário devolver um objeto na resposta.

Nesse caso, podemos utilizar:

```http
204 No Content
```

No ASP.NET Core:

```csharp
return NoContent();
```

E para documentar no Swagger:

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

Fluxo:

```text
PUT
 ↓
Atualização concluída
 ↓
Nenhum dado precisa ser retornado
 ↓
204 No Content
```

> A aula destaca que esse é um comportamento comum, mas pode depender da necessidade do projeto.

---

## Criando a classe de Request

Como queremos atualizar apenas algumas informações do usuário, podemos criar uma classe específica para essa operação.

Exemplo:

```csharp
public class UpdateUserProfileRequestJson
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
```

Essa classe representa os novos dados do usuário:

```text
UpdateUserProfileRequestJson
│
├── Name
└── Email
```

A senha não foi incluída nesse exemplo porque a aula considera a alteração de senha como uma funcionalidade separada.

---

## Recebendo dados pelo Body

O Endpoint pode receber os dados através do corpo da requisição:

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Update(
    [FromBody] UpdateUserProfileRequestJson request)
{
    return NoContent();
}
```

O atributo:

```csharp
[FromBody]
```

indica que o objeto será obtido através do:

```text
Body da Request
```

Exemplo de requisição:

```http
PUT /api/user
```

Body:

```json
{
  "name": "Ellison",
  "email": "ellison@gmail.com"
}
```

O ASP.NET Core transforma esse JSON em:

```text
request
│
├── Name  = "Ellison"
└── Email = "ellison@gmail.com"
```

---

## Testando pelo Swagger

Ao executar a API, o Swagger passa a mostrar:

```http
PUT /api/user
```

com um:

```text
Request Body
```

esperando:

```json
{
  "name": "string",
  "email": "string"
}
```

Podemos utilizar:

```text
Try it out
```

e enviar:

```json
{
  "name": "Ellison",
  "email": "ellison@gmail.com"
}
```

No código, esses dados chegam através do objeto:

```csharp
request
```

---

## Testando pelo Postman

No Postman, precisamos alterar o método para:

```http
PUT
```

Utilizamos a URL:

```text
https://localhost:7081/api/user
```

No Body:

```json
{
  "name": "Alison Arley",
  "email": "ally@gmail.com"
}
```

Então:

```text
Send
```

O Endpoint receberá os dados normalmente.

---

## As duas formas de identificar o usuário

Ao atualizar um recurso, surge uma pergunta importante:

> Qual usuário deve ser atualizado?

A aula apresenta dois cenários.

```text
Atualização
    │
    ├── Próprio usuário
    │
    └── Outro usuário
```

A forma de identificar o recurso muda dependendo da regra de negócio.

---

## Atualizando o próprio usuário

Imagine que um usuário só pode atualizar as próprias informações.

Nesse caso, não faz sentido permitir que ele escolha livremente um ID.

Exemplo:

```http
PUT /api/user
```

Body:

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Mas então surge a pergunta:

```text
Como a API sabe qual usuário deve ser alterado?
```

A resposta apresentada na aula é através do mecanismo de **autorização**.

Conceitualmente:

```text
Usuário faz a requisição
        ↓
Envia Token
        ↓
API identifica o usuário
        ↓
Usuário logado
        ↓
Atualiza o próprio perfil
```

Nesse cenário:

```text
ID na rota
    ✕
```

não é necessário.

A API identifica o usuário através do token.

> A implementação de autorização será estudada posteriormente.

---

## Atualizando outro usuário

Em alguns sistemas, pode fazer sentido permitir que determinado usuário altere outro usuário.

Por exemplo, dependendo das regras de negócio, um administrador pode alterar dados de outra conta.

Nesse cenário, precisamos informar **qual usuário será alterado**.

Então podemos colocar o ID na rota:

```csharp
[HttpPut]
[Route("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Update(
    [FromRoute] int id,
    [FromBody] UpdateUserProfileRequestJson request)
{
    return NoContent();
}
```

A requisição poderia ser:

```http
PUT /api/user/1
```

Body:

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Nesse caso:

```text
/api/user/1
          │
          └── Qual usuário será atualizado

Body
 │
 ├── Novo nome
 └── Novo e-mail
```

---

## ID na rota

A aula recomenda utilizar o ID diretamente na rota quando precisamos identificar explicitamente o recurso.

Exemplo:

```csharp
[Route("{id}")]
```

com:

```csharp
[FromRoute] int id
```

produz:

```text
/api/user/1
```

Fluxo:

```text
PUT /api/user/1
              │
              ▼
          [FromRoute]
              │
              ▼
            int id
              │
              ▼
              1
```

---

## ID na rota x ID no Body

Tecnicamente, seria possível enviar o ID dentro do Body.

Por exemplo:

```json
{
  "id": 1,
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Porém, segundo a convenção apresentada na aula, isso não é o mais comum.

A prática apresentada é:

```text
ID
↓
Rota

Novos dados
↓
Body
```

Exemplo:

```http
PUT /api/user/1
```

Body:

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Assim temos uma separação clara:

```text
Rota
↓
Qual recurso?

Body
↓
Quais são os novos dados?
```

---

## Quando utilizar cada abordagem?

### Atualizar o próprio perfil

```http
PUT /api/user
```

Body:

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Identificação:

```text
Token de autorização
        ↓
Usuário logado
```

---

### Atualizar outro usuário

```http
PUT /api/user/1
```

Body:

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

Identificação:

```text
ID na rota
```

---

## Fluxo completo

### Cenário 1 — Próprio usuário

```text
CLIENTE
   │
   │ PUT /api/user
   │
   │ Token
   │
   │ Body
   ▼
  API
   │
   ├── Identifica usuário pelo Token
   │
   ├── Recebe Name e Email
   │
   ├── Atualiza o perfil
   │
   ▼
204 No Content
```

---

### Cenário 2 — Outro usuário

```text
CLIENTE
   │
   │ PUT /api/user/1
   │              │
   │              └── ID
   │
   │ Body
   ▼
  API
   │
   ├── Recebe ID pela rota
   │
   ├── Recebe Name e Email pelo Body
   │
   ├── Atualiza o usuário informado
   │
   ▼
204 No Content
```

---

## Exemplo Completo

### Sem ID na rota

```csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Update(
    [FromBody] UpdateUserProfileRequestJson request)
{
    return NoContent();
}
```

Utilizado quando o usuário deve alterar apenas as próprias informações e sua identidade será determinada pela autorização.

---

### Com ID na rota

```csharp
[HttpPut]
[Route("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Update(
    [FromRoute] int id,
    [FromBody] UpdateUserProfileRequestJson request)
{
    return NoContent();
}
```

Utilizado quando a operação precisa informar explicitamente qual usuário será alterado.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **PUT** | Método HTTP utilizado para atualizar um recurso existente |
| **[HttpPut]** | Identifica um Endpoint como PUT |
| **Update()** | Método responsável pela atualização no exemplo |
| **204 No Content** | Resposta comum quando a atualização ocorre sem conteúdo de retorno |
| **NoContent()** | Retorna HTTP 204 |
| **UpdateUserProfileRequestJson** | Representa os novos dados do usuário |
| **[FromBody]** | Indica que os dados vêm do Body |
| **[FromRoute]** | Indica que o valor vem da rota |
| **ID na rota** | Identifica qual recurso deverá ser alterado |
| **Body** | Contém as novas informações do recurso |
| **Token** | Pode ser utilizado para identificar o usuário logado |
| **Usuário logado** | Cenário em que o próprio usuário altera suas informações |
| **Autorização** | Mecanismo que permitirá identificar e controlar quem pode executar a operação |
| **Swagger** | Permite testar o PUT e visualizar o contrato da requisição |
| **Postman** | Permite enviar manualmente requisições PUT |

---

## Visão Geral

```text
                 PUT
                  │
                  ▼
         Atualizar um recurso
                  │
       ┌──────────┴──────────┐
       │                     │
       ▼                     ▼
Próprio usuário        Outro usuário
       │                     │
       ▼                     ▼
 Token identifica          ID na rota
 usuário logado              │
       │                     │
       └──────────┬──────────┘
                  │
                  ▼
             Request Body
                  │
            ┌─────┴─────┐
            ▼           ▼
          Name        Email
                  │
                  ▼
             Atualização
                  │
                  ▼
           204 No Content
```

> **Em resumo:** o método `PUT` é utilizado para atualizar um recurso existente. Os novos dados normalmente são enviados pelo **Body** da requisição. Quando a operação modifica o próprio usuário logado, a identificação pode ser feita posteriormente através do token de autorização. Quando é necessário atualizar explicitamente outro recurso, a prática apresentada é enviar o **ID na rota** e manter somente os novos dados no Body.
