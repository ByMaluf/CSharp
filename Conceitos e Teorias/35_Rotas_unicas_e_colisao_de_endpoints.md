# Rotas Únicas, GetAll e ChangePassword

## 📋 Índice

1. [Complementando o UserController](#complementando-o-usercontroller)
2. [Criando o Endpoint GetAll](#criando-o-endpoint-getall)
3. [Retornando uma lista de usuários](#retornando-uma-lista-de-usuários)
4. [Objeto x Lista no JSON](#objeto-x-lista-no-json)
5. [Criando o Endpoint ChangePassword](#criando-o-endpoint-changepassword)
6. [Request para alteração de senha](#request-para-alteração-de-senha)
7. [Retorno 204 No Content](#retorno-204-no-content)
8. [O problema de colisão de endpoints](#o-problema-de-colisão-de-endpoints)
9. [Como a API diferencia os endpoints](#como-a-api-diferencia-os-endpoints)
10. [Rotas únicas](#rotas-únicas)
11. [Definindo uma rota específica](#definindo-uma-rota-específica)
12. [Múltiplos endpoints do mesmo método](#múltiplos-endpoints-do-mesmo-método)
13. [Detectando colisões pelo Swagger](#detectando-colisões-pelo-swagger)
14. [Resumo](#resumo)

---

## Complementando o UserController

Até este ponto, o UserController já possui endpoints responsáveis por:

~~~text
UserController
│
├── Cadastrar usuário
├── Atualizar usuário
├── Excluir usuário
└── Recuperar usuário por ID
~~~

Nesta aula, são adicionadas mais duas funcionalidades:

~~~text
UserController
│
├── Recuperar todos os usuários
└── Alterar a senha
~~~

Os métodos HTTP escolhidos são:

~~~text
GET
↓
Recuperar todos os usuários

PUT
↓
Alterar a senha
~~~

---

## Criando o Endpoint GetAll

Já existe um Endpoint responsável por recuperar um usuário específico através de um ID.

Agora precisamos criar outro Endpoint para recuperar todos os usuários.

Exemplo:

~~~csharp
[HttpGet]
[ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
public IActionResult GetAll()
{
    var response = new List<User>
    {
        new User
        {
            Id = 1,
            Age = 7,
            Name = "Wellison"
        },

        new User
        {
            Id = 2,
            Age = 7,
            Name = "Maria"
        }
    };

    return Ok(response);
}
~~~

Temos:

~~~text
GetAll()
   ↓
Recupera todos os usuários
   ↓
List<User>
   ↓
200 OK
~~~

---

## Retornando uma lista de usuários

Diferente do GetById, que retorna um único usuário, o GetAll retorna uma coleção.

Exemplo:

~~~csharp
List<User>
~~~

Podemos instanciar a lista e já adicionar elementos:

~~~csharp
var response = new List<User>
{
    new User
    {
        Id = 1,
        Age = 7,
        Name = "Wellison"
    },

    new User
    {
        Id = 2,
        Age = 7,
        Name = "Maria"
    }
};
~~~

Depois:

~~~csharp
return Ok(response);
~~~

Resultado:

~~~http
200 OK
~~~

com uma lista no Body da Response.

---

## Objeto x Lista no JSON

A aula chama atenção para uma diferença visual importante no JSON.

### Objeto

Um único objeto começa com:

~~~json
{
}
~~~

Exemplo:

~~~json
{
  "id": 1,
  "age": 7,
  "name": "Wellison"
}
~~~

### Lista

Uma lista começa com:

~~~json
[
]
~~~

Exemplo:

~~~json
[
  {
    "id": 1,
    "age": 7,
    "name": "Wellison"
  },
  {
    "id": 2,
    "age": 7,
    "name": "Maria"
  }
]
~~~

Portanto:

~~~text
{ }
↓
Objeto

[ ]
↓
Lista
~~~

No Swagger, isso permite identificar rapidamente se o Endpoint retorna um único recurso ou uma coleção.

---

## Criando o Endpoint ChangePassword

Agora queremos criar uma funcionalidade para alterar a senha de um usuário.

Como estamos modificando um dado existente, utilizamos:

~~~http
PUT
~~~

Exemplo inicial:

~~~csharp
[HttpPut]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult ChangePassword(
    [FromBody] ChangePasswordRequestJson request)
{
    return NoContent();
}
~~~

Temos:

~~~text
PUT
 ↓
Alterar dado existente
 ↓
ChangePassword
 ↓
204 No Content
~~~

---

## Request para alteração de senha

A requisição precisa receber duas informações:

~~~text
Senha atual
Nova senha
~~~

Exemplo conceitual:

~~~csharp
public class ChangePasswordRequestJson
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}
~~~

O Endpoint recebe:

~~~csharp
[FromBody] ChangePasswordRequestJson request
~~~

Body:

~~~json
{
  "currentPassword": "senha-atual",
  "newPassword": "nova-senha"
}
~~~

A ideia apresentada é:

~~~text
CurrentPassword
      ↓
Comparar com a senha atual do usuário
      ↓
Está correta?
      ↓
SIM
      ↓
Trocar por NewPassword
~~~

---

## Retorno 204 No Content

Na alteração de senha, a aula utiliza:

~~~http
204 No Content
~~~

porque não há necessidade de devolver informações no Body.

No Controller:

~~~csharp
return NoContent();
~~~

E na documentação:

~~~csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
~~~

---

## O problema de colisão de endpoints

Ao adicionar o novo PUT, a API passa a possuir dois endpoints com o mesmo método e a mesma rota base.

Já existia algo equivalente a:

~~~http
PUT /api/user
~~~

para atualizar informações do usuário.

Agora também criamos:

~~~http
PUT /api/user
~~~

para alterar a senha.

Isso gera uma colisão.

Conceitualmente:

~~~text
PUT /api/user
   │
   ├── Update()
   │
   └── ChangePassword()
~~~

A API não sabe qual dos dois métodos deve executar.

---

## O nome do método C# não resolve

Mesmo que os métodos tenham nomes diferentes:

~~~text
Update()
ChangePassword()
~~~

isso não diferencia os endpoints externamente.

O cliente envia uma combinação de:

~~~text
Método HTTP
+
Rota
~~~

Portanto:

~~~text
PUT + /api/user
~~~

precisa identificar apenas um Endpoint.

O conteúdo recebido pelo Body também não é utilizado para decidir entre dois endpoints que possuam a mesma combinação de método e rota.

---

## Como a API diferencia os endpoints

Uma requisição HTTP contém, entre outras informações:

~~~text
Método HTTP
+
URL
~~~

Exemplos:

~~~http
GET /api/user
POST /api/user
PUT /api/user
DELETE /api/user
~~~

Mesmo utilizando a mesma URL, os métodos HTTP são diferentes.

Por isso não existe conflito entre:

~~~text
GET    /api/user
POST   /api/user
PUT    /api/user
DELETE /api/user
~~~

Mas existe conflito quando temos:

~~~text
PUT /api/user
PUT /api/user
~~~

---

## Por que GetById e GetAll não colidem?

Podemos possuir dois endpoints GET porque suas rotas são diferentes.

Por exemplo:

~~~http
GET /api/user
~~~

e:

~~~http
GET /api/user/7
~~~

No primeiro caso:

~~~text
/api/user
↓
GetAll()
~~~

No segundo:

~~~text
/api/user/{id}
↓
GetById()
~~~

Portanto:

~~~text
GET /api/user
GET /api/user/{id}
~~~

são rotas distintas.

---

## Rotas únicas

Cada Endpoint precisa possuir uma combinação única de:

~~~text
Método HTTP
+
Rota
~~~

Podemos representar assim:

~~~text
Endpoint único
      =
Método HTTP
      +
Rota
~~~

Exemplos válidos:

~~~text
GET /api/user
GET /api/user/{id}

PUT /api/user
PUT /api/user/changePassword
~~~

Exemplo problemático:

~~~text
PUT /api/user
PUT /api/user
~~~

---

## Definindo uma rota específica

Uma forma simples de evitar a colisão é adicionar um segmento específico ao atributo do método HTTP.

Exemplo:

~~~csharp
[HttpPut("changePassword")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult ChangePassword(
    [FromBody] ChangePasswordRequestJson request)
{
    return NoContent();
}
~~~

Agora a rota se torna:

~~~http
PUT /api/user/changePassword
~~~

Enquanto o Update continua:

~~~http
PUT /api/user
~~~

Assim:

~~~text
PUT /api/user
↓
Atualizar perfil

PUT /api/user/changePassword
↓
Alterar senha
~~~

Agora não existe colisão.

---

## Rotas significativas

A aula também destaca que o nome utilizado na rota deve ser significativo.

Exemplo:

~~~csharp
[HttpPut("changePassword")]
~~~

gera:

~~~text
/api/user/changePassword
~~~

O objetivo da rota fica claro:

~~~text
changePassword
↓
Alterar senha
~~~

Além de evitar colisões, uma rota significativa facilita entender a finalidade do Endpoint.

---

## Múltiplos endpoints do mesmo método

É permitido possuir vários endpoints com o mesmo método HTTP.

Por exemplo:

~~~text
PUT /api/user
PUT /api/user/changePassword
PUT /api/user/outraOperacao
~~~

Isso funciona porque as rotas são diferentes.

Também podemos ter vários GET:

~~~text
GET /api/user
GET /api/user/{id}
GET /api/user/outraRota
~~~

A mesma ideia vale para GET, POST, PUT e DELETE.

A regra principal é:

~~~text
Mesmo método HTTP?
       │
       ▼
As rotas precisam ser diferentes
~~~

---

## Exemplo de colisão

Imagine:

~~~csharp
[HttpPut("changePassword")]
public IActionResult ChangePassword()
{
    return NoContent();
}

[HttpPut("changePassword")]
public IActionResult OutraFuncao()
{
    return NoContent();
}
~~~

Os nomes dos métodos são diferentes, mas ambos possuem:

~~~text
PUT /api/user/changePassword
~~~

Portanto existe colisão.

---

## Corrigindo a colisão

Podemos alterar uma das rotas:

~~~csharp
[HttpPut("changePassword")]
public IActionResult ChangePassword()
{
    return NoContent();
}

[HttpPut("outraOperacao")]
public IActionResult OutraFuncao()
{
    return NoContent();
}
~~~

Agora temos:

~~~text
PUT /api/user/changePassword

PUT /api/user/outraOperacao
~~~

As duas combinações são únicas.

---

## Detectando colisões pelo Swagger

Durante a aula, ao adicionar dois endpoints conflitantes, o Swagger apresentou erro.

Por isso, neste contexto, um erro ao carregar o Swagger pode servir como alerta para verificar se existem endpoints com a mesma combinação de método e rota.

Fluxo:

~~~text
Swagger apresenta erro
        ↓
Verificar endpoints
        ↓
Existem métodos iguais?
        ↓
Possuem a mesma rota?
        ↓
Possível colisão
~~~

---

## Estrutura final dos endpoints

Depois das alterações, o Controller pode possuir algo semelhante a:

~~~text
UserController
│
├── GET    /api/user
│          └── GetAll
│
├── GET    /api/user/{id}
│          └── GetById
│
├── POST   /api/user
│          └── Create
│
├── PUT    /api/user
│          └── Update
│
├── PUT    /api/user/changePassword
│          └── ChangePassword
│
└── DELETE /api/user
           └── Delete
~~~

Cada operação possui uma combinação identificável de:

~~~text
Método HTTP
+
Rota
~~~

---

## Fluxo do GetAll

~~~text
CLIENTE
   │
   │ GET /api/user
   ▼
UserController
   │
   ▼
GetAll()
   │
   ▼
List<User>
   │
   ▼
Ok(response)
   │
   ▼
200 OK
   │
   ▼
[
  usuário 1,
  usuário 2
]
~~~

---

## Fluxo do ChangePassword

~~~text
CLIENTE
   │
   │ PUT /api/user/changePassword
   │
   │ Body
   ▼
ChangePasswordRequestJson
   │
   ├── CurrentPassword
   └── NewPassword
   │
   ▼
ChangePassword()
   │
   ▼
Alteração da senha
   │
   ▼
204 No Content
~~~

---

## Resumo

| Conceito | Descrição |
|---|---|
| **GetAll** | Endpoint utilizado para recuperar todos os usuários |
| **List<User>** | Representa uma coleção de usuários |
| **{ } no JSON** | Representa um objeto |
| **[ ] no JSON** | Representa uma lista |
| **ChangePassword** | Endpoint responsável pela alteração da senha |
| **PUT** | Método utilizado para alterar um dado existente |
| **CurrentPassword** | Senha atual utilizada para comparação |
| **NewPassword** | Nova senha |
| **204 No Content** | Resposta utilizada quando não há conteúdo de retorno |
| **Colisão de endpoints** | Ocorre quando endpoints possuem a mesma combinação de método HTTP e rota |
| **Método HTTP + Rota** | Combinação utilizada para diferenciar os endpoints |
| **Nome do método C#** | Não diferencia endpoints externamente |
| **HttpPut("changePassword")** | Adiciona uma rota específica ao Endpoint PUT |
| **Rota significativa** | Ajuda a identificar a finalidade do Endpoint |
| **Rotas únicas** | Evitam ambiguidade entre endpoints |
| **Swagger** | Pode evidenciar uma colisão de endpoints durante a configuração |

---

## Visão Geral

~~~text
                    USER CONTROLLER
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
        ▼                 ▼                 ▼
       GET               PUT              OUTROS
        │                 │
   ┌────┴────┐       ┌────┴────┐
   │         │       │         │
   ▼         ▼       ▼         ▼
GetAll   GetById   Update   ChangePassword
   │         │       │         │
   ▼         ▼       ▼         ▼
/user   /user/{id} /user  /user/changePassword
~~~

### Regra principal

~~~text
ENDPOINT
   =
MÉTODO HTTP
   +
ROTA ÚNICA
~~~

> **Em resumo:** é possível criar vários endpoints utilizando o mesmo método HTTP, como vários GET ou vários PUT. O requisito apresentado na aula é que cada um possua uma **rota única**. O GetAll utiliza GET /api/user, enquanto o GetById utiliza GET /api/user/{id}. Para evitar colisão entre Update e ChangePassword, o segundo recebe uma rota específica, como PUT /api/user/changePassword. A combinação **método HTTP + rota** é o que diferencia esses endpoints.
