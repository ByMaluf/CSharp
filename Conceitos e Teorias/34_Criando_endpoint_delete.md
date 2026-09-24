# Endpoint DELETE e Exclusão de Recursos

## 📋 Índice

1. [O que é um Endpoint DELETE?](#o-que-é-um-endpoint-delete)
2. [As duas formas de identificar o recurso](#as-duas-formas-de-identificar-o-recurso)
3. [Excluindo o próprio usuário](#excluindo-o-próprio-usuário)
4. [Excluindo outro recurso pelo ID](#excluindo-outro-recurso-pelo-id)
5. [DELETE e Request Body](#delete-e-request-body)
6. [Criando o Endpoint DELETE](#criando-o-endpoint-delete)
7. [Retorno 204 No Content](#retorno-204-no-content)
8. [Testando pelo Swagger](#testando-pelo-swagger)
9. [Recebendo ID pela rota](#recebendo-id-pela-rota)
10. [Testando pelo Postman](#testando-pelo-postman)
11. [Comparação com GET e PUT](#comparação-com-get-e-put)
12. [Fluxo completo](#fluxo-completo)
13. [Resumo](#resumo)

---

## O que é um Endpoint DELETE?

O método HTTP:

```http
DELETE
```

é utilizado para **excluir um recurso existente**.

No contexto de um `UserController`, podemos utilizá-lo para remover uma conta de usuário.

Exemplo conceitual:

```text
Usuário existe
      ↓
DELETE
      ↓
Usuário removido
```

---

## As duas formas de identificar o recurso

Assim como foi discutido no Endpoint `PUT`, ao criar um `DELETE` precisamos responder:

> Faz sentido receber um ID através da rota?

Existem dois cenários principais:

```text
DELETE
  │
  ├── Excluir o próprio recurso
  │
  └── Excluir outro recurso identificado por ID
```

A escolha depende da regra de negócio.

---

## Excluindo o próprio usuário

Imagine que um usuário só possa excluir a própria conta.

Nesse caso, não faz sentido permitir que ele informe livremente o ID de outro usuário.

Podemos ter:

```http
DELETE /api/user
```

Sem nenhum ID na rota.

Mas então surge a pergunta:

```text
Como a API sabe qual usuário deve ser excluído?
```

A ideia apresentada na aula é identificar o:

```text
Usuário logado
```

através do mecanismo de autorização.

Conceitualmente:

```text
Usuário envia requisição
        ↓
API identifica usuário logado
        ↓
DELETE /api/user
        ↓
Exclui a própria conta
```

Nesse cenário:

```text
ID na rota
    ✕
```

não é necessário.

---

## Excluindo outro recurso pelo ID

Em outros contextos, pode fazer sentido informar explicitamente qual recurso deve ser excluído.

Por exemplo:

```text
Excluir produto
```

Nesse caso, podemos informar o ID através da rota:

```http
DELETE /api/product/7
```

Assim:

```text
/api/product/7
             │
             └── ID do produto
```

A API sabe exatamente qual recurso deverá ser removido.

---

## DELETE e Request Body

A aula também retoma uma discussão feita anteriormente sobre o método `GET`.

Dependendo da linguagem, biblioteca ou cliente HTTP:

- algumas podem permitir Body em uma requisição DELETE;
- outras podem não permitir;
- algumas APIs podem aceitar;
- outras podem ignorar.

Por causa dessas diferenças, a convenção adotada na aula é:

```text
GET
↓
Sem Body

DELETE
↓
Sem Body
```

A ideia é evitar comportamentos que possam variar entre diferentes clientes e linguagens.

---

## Por que evitar Body no DELETE?

Uma API pode ser utilizada por diferentes tipos de aplicações:

```text
API
│
├── Site
├── Aplicativo Android
├── Aplicativo iOS
├── Aplicação Windows
├── Aplicação macOS
└── Outros sistemas
```

Se algumas tecnologias tratam o Body de DELETE de maneira diferente, isso pode gerar problemas de interoperabilidade.

Por isso, na abordagem da aula:

```text
DELETE
│
├── Pode receber informações pela rota
└── Não utilizará Body
```

---

## Criando o Endpoint DELETE

Dentro do `UserController`, podemos criar:

```csharp
[HttpDelete]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Delete()
{
    return NoContent();
}
```

Temos:

```text
[HttpDelete]
      ↓
Define que o Endpoint utiliza DELETE

Delete()
      ↓
Método responsável pela exclusão

NoContent()
      ↓
Resposta HTTP 204
```

No Swagger podemos ter:

```text
GET     /api/user
POST    /api/user
PUT     /api/user
DELETE  /api/user
```

O mesmo recurso pode possuir vários métodos HTTP diferentes.

---

## Retorno 204 No Content

Ao excluir um recurso, geralmente não precisamos devolver informações no Body.

Por isso, podemos utilizar:

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
DELETE
   ↓
Recurso excluído
   ↓
Nenhum conteúdo precisa ser retornado
   ↓
204 No Content
```

---

## Testando pelo Swagger

Ao executar a API, o Swagger identifica o Endpoint:

```http
DELETE /api/user
```

Podemos utilizar:

```text
Try it out
```

e depois:

```text
Execute
```

A requisição será enviada para o método:

```csharp
Delete()
```

Podemos utilizar um breakpoint para confirmar a execução.

Fluxo:

```text
Swagger
   │
   │ DELETE /api/user
   ▼
UserController
   │
   ▼
Delete()
   │
   ▼
NoContent()
   │
   ▼
204 No Content
```

---

## Recebendo ID pela rota

Se a regra de negócio exigir que o Endpoint identifique explicitamente o recurso a ser excluído, podemos adicionar um parâmetro de rota.

Exemplo:

```csharp
[HttpDelete]
[Route("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Delete([FromRoute] int id)
{
    return NoContent();
}
```

A requisição seria:

```http
DELETE /api/user/7
```

Temos:

```text
/api/user/7
          │
          ▼
         {id}
          │
          ▼
[FromRoute] int id
          │
          ▼
          7
```

No Swagger, o parâmetro aparecerá como:

```text
Path
```

---

## Quando usar ID na rota?

A lógica apresentada é semelhante à aula de `PUT`.

### Cenário 1 — Próprio usuário

Se o usuário só pode excluir a própria conta:

```http
DELETE /api/user
```

A API identifica quem está logado.

---

### Cenário 2 — Outro recurso

Se faz sentido informar qual recurso deve ser removido:

```http
DELETE /api/product/7
```

O ID vai na rota.

Portanto:

```text
Faz sentido escolher qual recurso excluir?
        │
    ┌───┴───┐
    │       │
   NÃO     SIM
    │       │
    ▼       ▼
Usuário   ID na
logado    rota
```

---

## Testando pelo Postman

No Postman, selecionamos:

```http
DELETE
```

Exemplo:

```text
https://localhost:7081/api/user
```

Na área do Body, a abordagem da aula é utilizar:

```text
none
```

ou seja:

```text
Nenhum Body
```

Depois:

```text
Send
```

A API executará o Endpoint e poderá retornar:

```http
204 No Content
```

---

## E se um Body for enviado?

Na demonstração da aula, mesmo enviando um Body pelo Postman, o Endpoint não estava configurado para receber nenhum parâmetro do Body.

Nesse caso, o conteúdo foi simplesmente ignorado.

Conceitualmente:

```text
DELETE /api/user

Body enviado
     │
     ▼
Endpoint não recebe Body
     │
     ▼
Conteúdo não utilizado
```

Por isso, o recomendado na abordagem apresentada é configurar a requisição sem Body.

---

## Comparação com GET e PUT

Agora podemos comparar os métodos estudados.

### GET

```text
Objetivo
↓
Recuperar informações

Dados comuns
↓
Route
Query String
Headers

Body
↓
Não utilizado na abordagem da aula
```

---

### PUT

```text
Objetivo
↓
Atualizar recurso

ID
↓
Pode ir na rota

Novos dados
↓
Body
```

Exemplo:

```http
PUT /api/user/7
```

```json
{
  "name": "Novo Nome",
  "email": "novo@email.com"
}
```

---

### DELETE

```text
Objetivo
↓
Excluir recurso

ID
↓
Pode ir na rota

Body
↓
Não utilizado na abordagem da aula
```

Exemplo:

```http
DELETE /api/user/7
```

---

## Fluxo completo

### Sem ID na rota

```text
CLIENTE
   │
   │ DELETE /api/user
   ▼
  API
   │
   ├── Identifica usuário logado
   │
   ├── Exclui a conta
   │
   ▼
204 No Content
```

---

### Com ID na rota

```text
CLIENTE
   │
   │ DELETE /api/user/7
   │                 │
   │                 └── ID
   ▼
  API
   │
   ├── Recebe ID pela rota
   │
   ├── Localiza o recurso
   │
   ├── Exclui
   │
   ▼
204 No Content
```

---

## Exemplo Completo

### Excluir o próprio usuário

```csharp
[HttpDelete]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Delete()
{
    return NoContent();
}
```

---

### Excluir recurso pelo ID

```csharp
[HttpDelete]
[Route("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Delete([FromRoute] int id)
{
    return NoContent();
}
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **DELETE** | Método HTTP utilizado para excluir um recurso |
| **[HttpDelete]** | Identifica um Endpoint como DELETE |
| **Delete()** | Método responsável pela exclusão no exemplo |
| **204 No Content** | Resposta comum quando a exclusão é realizada sem conteúdo de retorno |
| **NoContent()** | Retorna HTTP 204 |
| **ID na rota** | Utilizado quando é necessário identificar explicitamente o recurso |
| **[FromRoute]** | Indica que o ID vem da rota |
| **Usuário logado** | Pode ser utilizado para identificar o próprio usuário sem enviar ID |
| **Request Body** | Não é utilizado no DELETE na abordagem adotada na aula |
| **Swagger** | Permite visualizar e testar o Endpoint DELETE |
| **Postman** | Permite enviar manualmente requisições DELETE |
| **none** | Opção utilizada no Postman quando não há Body |
| **Interoperabilidade** | Motivo apresentado para evitar comportamentos diferentes entre clientes e linguagens |

---

## Visão Geral

```text
                DELETE
                  │
                  ▼
           Excluir recurso
                  │
       ┌──────────┴──────────┐
       │                     │
       ▼                     ▼
Próprio usuário       Outro recurso
       │                     │
       ▼                     ▼
Usuário logado           ID na rota
       │                     │
       └──────────┬──────────┘
                  │
                  ▼
              Exclusão
                  │
                  ▼
          204 No Content
```

### Regra adotada na aula

```text
GET
└── Sem Body

DELETE
└── Sem Body

PUT
└── Body com dados de atualização
```

> **Em resumo:** o método `DELETE` é utilizado para excluir recursos. Quando o próprio usuário exclui sua conta, a identificação pode ser feita posteriormente através do usuário logado, sem necessidade de enviar um ID na rota. Quando é necessário escolher explicitamente qual recurso remover, o ID pode ser enviado no Path. Na abordagem apresentada na aula, requisições `DELETE` não utilizam Body, favorecendo compatibilidade entre diferentes clientes e linguagens.
