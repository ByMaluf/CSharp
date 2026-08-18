# Métodos HTTP — GET, POST, PUT e DELETE

## 📋 Índice

1. [O que são Métodos HTTP?](#o-que-são-métodos-http)
2. [GET](#get)
3. [PUT](#put)
4. [POST](#post)
5. [DELETE](#delete)
6. [Payload](#payload)
7. [Comparação entre os Métodos HTTP](#comparação-entre-os-métodos-http)
8. [Resumo](#resumo)

---

## O que são Métodos HTTP?

Os **Métodos HTTP** indicam qual ação queremos executar sobre determinado recurso de uma API.

Os principais métodos apresentados são:

| Método | Objetivo |
|---|---|
| `GET` | Recuperar dados |
| `POST` | Criar um novo recurso |
| `PUT` | Atualizar um recurso existente |
| `DELETE` | Excluir um recurso |

O método HTTP, em conjunto com o endpoint, permite que a API saiba qual operação deve ser executada.

Exemplo:

    GET     /produtos
    POST    /produtos
    PUT     /produtos
    DELETE  /produtos

Apesar de utilizarem o mesmo recurso, cada método representa uma ação diferente.

---

## GET

O método **GET** é utilizado para **solicitar e recuperar informações do servidor**.

Exemplos:

    GET /produtos

    GET /produtos/10

    GET /usuarios/me

Pode ser utilizado para:

- recuperar um produto;
- listar produtos;
- obter informações de um usuário;
- realizar consultas;
- realizar filtros.

### Parâmetros no GET

Segundo a abordagem apresentada na aula, o GET não utiliza um **Body** para enviar os dados necessários à consulta.

Quando precisamos enviar informações para realizar filtros ou buscas, podemos utilizar parâmetros na URL.

Exemplo:

    GET /produtos?nome=shampoo

Nesse caso:

    nome=shampoo

é uma informação enviada junto à URL para realizar o filtro.

Também podem existir informações complementares nos Headers.

### Respostas do GET

Quando a informação é encontrada com sucesso, normalmente retornamos:

    200 OK

Exemplo de resposta:

    {
        "id": 10,
        "nome": "Shampoo",
        "estoque": 20
    }

Quando um recurso específico não é encontrado, podemos retornar:

    404 Not Found

Exemplo:

    GET /produtos/999

Se o produto não existir:

    404 Not Found

---

## PUT

O método **PUT** é utilizado para **atualizar um recurso existente no servidor**.

Exemplo:

    PUT /usuarios/10

Body:

    {
        "nome": "João da Silva",
        "email": "joao@email.com"
    }

Nesse exemplo, estamos atualizando os dados do usuário de ID `10`.

### Respostas do PUT

Quando a atualização é realizada e a API devolve algum conteúdo, pode retornar:

    200 OK

Se a atualização for realizada com sucesso, mas não houver nenhum conteúdo para devolver:

    204 No Content

Portanto:

    PUT
     │
     ├── Atualizou e devolveu conteúdo
     │       ↓
     │     200 OK
     │
     └── Atualizou sem devolver conteúdo
             ↓
          204 No Content

---

## POST

O método **POST** é utilizado para **criar um novo recurso no servidor**.

Exemplo:

    POST /usuarios

Body:

    {
        "nome": "João Silva",
        "email": "joao@email.com",
        "senha": "123456"
    }

Nesse caso, a API recebe as informações e cria um novo usuário.

### Resposta do POST

Quando o recurso é criado com sucesso, normalmente utilizamos:

    201 Created

É comum retornar alguma informação sobre o recurso criado.

Por exemplo:

    {
        "id": 15
    }

O ID é normalmente definido pela própria API ou pelo banco de dados.

Fluxo:

    POST
      ↓
    Criação do recurso
      ↓
    201 Created

---

## DELETE

O método **DELETE** é utilizado para **excluir um recurso no servidor**.

Exemplo:

    DELETE /usuarios/10

Quando a exclusão é realizada com sucesso e não existe conteúdo para retornar:

    204 No Content

Fluxo:

    DELETE /usuarios/10
              ↓
    API localiza o usuário
              ↓
    Executa a exclusão
              ↓
    204 No Content

### Exclusão definitiva e exclusão temporária

A exclusão de um recurso pode funcionar de maneiras diferentes dependendo das regras da aplicação.

### Exclusão definitiva

O registro é removido e não poderá ser recuperado posteriormente.

    Usuário solicita exclusão
            ↓
    Registro é eliminado
            ↓
    Não pode ser recuperado

### Exclusão temporária

Algumas aplicações adotam uma política em que o dado permanece disponível por determinado período antes de ser definitivamente eliminado.

Exemplo:

    Usuário solicita exclusão
            ↓
    Registro é movido para uma "lixeira"
            ↓
    Período para recuperação
            ↓
    Exclusão definitiva

A forma como essa exclusão funciona depende das **regras e políticas da aplicação**.

---

## Payload

Na aula, o **Payload** é apresentado como o conjunto de informações envolvidas na requisição.

Podemos visualizar a estrutura da seguinte maneira:

    PAYLOAD
    │
    ├── URL / Endpoint
    ├── Método HTTP
    ├── Headers
    └── Body

Por exemplo:

    PUT /usuarios/10

Headers:

    Content-Type: application/json
    Authorization: Bearer TOKEN

Body:

    {
        "nome": "João Silva",
        "email": "joao@email.com"
    }

O Body contém os dados que serão utilizados pela API para realizar a operação.

---

## Comparação entre os Métodos HTTP

| Método | Principal função | Body | Status de sucesso comum |
|---|---|---|---|
| `GET` | Recuperar dados | Normalmente não utilizado | `200 OK` |
| `POST` | Criar recurso | Sim | `201 Created` |
| `PUT` | Atualizar recurso | Sim | `200 OK` ou `204 No Content` |
| `DELETE` | Excluir recurso | Normalmente não necessário | `204 No Content` |

---

## Exemplo com o recurso Produtos

Imagine uma API de supermercado.

### Listar produtos

    GET /produtos

Resposta:

    200 OK

### Consultar um produto

    GET /produtos/10

Se encontrado:

    200 OK

Se não encontrado:

    404 Not Found

### Criar produto

    POST /produtos

Body:

    {
        "nome": "Caneta Azul",
        "estoque": 10
    }

Resposta:

    201 Created

### Atualizar produto

    PUT /produtos/10

Body:

    {
        "nome": "Caneta Azul",
        "estoque": 20
    }

Resposta possível:

    204 No Content

### Excluir produto

    DELETE /produtos/10

Resposta:

    204 No Content

---

## Resumo

| Método | Significado |
|---|---|
| **GET** | Solicita informações ao servidor |
| **POST** | Envia dados para criar um novo recurso |
| **PUT** | Envia dados para atualizar um recurso existente |
| **DELETE** | Solicita a exclusão de um recurso |
| **Payload** | Conjunto de informações envolvidas na requisição |
| **200 OK** | Operação realizada com sucesso e com conteúdo |
| **201 Created** | Novo recurso criado com sucesso |
| **204 No Content** | Operação realizada com sucesso e sem conteúdo |
| **404 Not Found** | Recurso solicitado não encontrado |

---

## Fluxo Geral

    RECURSO
       │
       ├── GET
       │    └── Consultar → 200 OK
       │
       ├── POST
       │    └── Criar → 201 Created
       │
       ├── PUT
       │    └── Atualizar → 200 OK / 204 No Content
       │
       └── DELETE
            └── Excluir → 204 No Content