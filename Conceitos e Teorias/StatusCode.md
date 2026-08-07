````markdown
# HTTP Status Codes

## 📋 Índice

1. [O que são Status Codes?](#o-que-são-status-codes)
2. [Famílias de Status Codes](#famílias-de-status-codes)
3. [Status de Sucesso (2xx)](#status-de-sucesso-2xx)
4. [Status de Erro do Cliente (4xx)](#status-de-erro-do-cliente-4xx)
5. [Status de Erro do Servidor (5xx)](#status-de-erro-do-servidor-5xx)
6. [Quando utilizar cada Status Code](#quando-utilizar-cada-status-code)
7. [Resumo](#resumo)

---

# O que são Status Codes?

Sempre que uma API responde a uma requisição HTTP, ela **deve retornar um código de status (Status Code)**.

Esse código informa ao cliente (Front-end, aplicativo, outra API etc.) o resultado da requisição.

Dependendo da operação, além do código, a API também pode devolver um conteúdo (body da resposta).

Exemplo:

```text
Cliente
    ↓
Requisição
    ↓
API
    ↓
Status Code + Resposta
```

O **Status Code** é obrigatório em toda resposta HTTP.

---

# Famílias de Status Codes

Os códigos HTTP são organizados em famílias.

| Família | Significado |
|----------|-------------|
| **2xx** | Requisição executada com sucesso |
| **4xx** | Erro causado pelo cliente (requisição inválida, sem autorização, recurso inexistente etc.) |
| **5xx** | Erro interno do servidor |

Cada família possui diversos códigos específicos.

---

# Status de Sucesso (2xx)

Toda resposta iniciada por **2** indica que a operação foi realizada com sucesso.

## 200 — OK

Indica que a requisição foi executada com sucesso e que existe um conteúdo para retornar.

É normalmente utilizado em consultas.

Exemplo:

```
GET /produtos/1
```

Resposta:

```http
200 OK
```

```json
{
    "id": 1,
    "nome": "Caneta Azul",
    "estoque": 10
}
```

Quando utilizar:

- consultar um recurso;
- listar informações;
- retornar dados encontrados.

---

## 201 — Created

Indica que um novo recurso foi criado com sucesso.

É normalmente utilizado em operações de cadastro.

Exemplo:

```
POST /produtos
```

Resposta:

```http
201 Created
```

```json
{
    "id": 15
}
```

É recomendado retornar alguma informação sobre o recurso criado, normalmente:

- ID;
- URL do recurso;
- Token;
- Identificador.

---

## 204 — No Content

Indica que a operação foi realizada com sucesso, porém não existe conteúdo para retornar.

É muito utilizado em:

- atualização de registros;
- exclusão de registros;
- pesquisas que retornaram uma coleção vazia.

Exemplo de atualização:

```
PUT /produtos/1
```

Resposta:

```http
204 No Content
```

Exemplo de pesquisa:

```
GET /produtos?nome=shampoo
```

Se nenhum produto for encontrado:

```http
204 No Content
```

A pesquisa foi executada corretamente, apenas não existem resultados.

---

# Status de Erro do Cliente (4xx)

Os códigos iniciados por **4** indicam que o problema está na requisição enviada pelo cliente.

---

## 400 — Bad Request

Indica que a requisição contém informações inválidas.

É normalmente utilizado em erros de validação.

Exemplo:

```json
{
    "nome": "",
    "estoque": -10
}
```

Resposta:

```http
400 Bad Request
```

```json
[
    "Nome é obrigatório.",
    "Estoque não pode ser negativo."
]
```

Exemplos de validações:

- campos obrigatórios;
- e-mail inválido;
- senha inválida;
- CPF inválido;
- quantidade negativa.

---

## 401 — Unauthorized

Significa que a API **não conseguiu identificar quem está fazendo a requisição**.

Em outras palavras:

> "Não sei quem é você."

Normalmente ocorre quando:

- não existe token;
- token expirou;
- login inválido.

Resposta:

```http
401 Unauthorized
```

---

## 403 — Forbidden

Significa que a API **conhece o usuário**, porém ele **não possui permissão** para executar aquela operação.

Em outras palavras:

> "Sei quem você é, mas você não pode fazer isso."

Exemplo:

Um usuário comum tentando excluir outro usuário.

Resposta:

```http
403 Forbidden
```

---

## 404 — Not Found

Indica que o recurso solicitado não foi encontrado.

Exemplo:

```
GET /produtos/100
```

Se o produto não existir:

```http
404 Not Found
```

Também é utilizado em operações como:

- atualizar um recurso inexistente;
- excluir um recurso inexistente.

Exemplo:

```
DELETE /produtos/15
```

Se o produto não existir:

```http
404 Not Found
```

> **Importante:** o **404** é utilizado quando um recurso específico não existe.

---

# Status de Erro do Servidor (5xx)

## 500 — Internal Server Error

Indica que ocorreu uma exceção inesperada durante o processamento da requisição.

É um erro interno da aplicação.

Exemplo:

```http
500 Internal Server Error
```

Por questões de segurança, recomenda-se **não retornar detalhes técnicos da exceção** para o cliente.

O ideal é responder apenas uma mensagem genérica, como:

```json
{
    "erro": "Erro interno do servidor."
}
```

Enquanto os detalhes da exceção ficam registrados em logs da aplicação.

---

# Quando utilizar cada Status Code

| Situação | Status |
|----------|--------|
| Consultou um recurso e encontrou | **200 OK** |
| Criou um recurso | **201 Created** |
| Atualizou ou removeu um recurso sem retornar dados | **204 No Content** |
| Pesquisa retornou lista vazia | **204 No Content** |
| Dados inválidos | **400 Bad Request** |
| Usuário não autenticado | **401 Unauthorized** |
| Usuário autenticado, mas sem permissão | **403 Forbidden** |
| Recurso específico não encontrado | **404 Not Found** |
| Erro inesperado da aplicação | **500 Internal Server Error** |

---

# Resumo

| Código | Significado | Quando utilizar |
|---------|-------------|-----------------|
| **200** | OK | Consulta realizada com sucesso e há dados para retornar |
| **201** | Created | Novo recurso criado com sucesso |
| **204** | No Content | Operação realizada com sucesso, mas sem conteúdo para retornar |
| **400** | Bad Request | Dados enviados são inválidos |
| **401** | Unauthorized | Usuário não autenticado |
| **403** | Forbidden | Usuário autenticado, porém sem permissão |
| **404** | Not Found | Recurso específico não encontrado |
| **500** | Internal Server Error | Erro inesperado no servidor |

---

# Fluxo resumido

```text
Requisição
     │
     ▼
A API processa
     │
     ├── Sucesso
     │      ├── 200 → Encontrou dados
     │      ├── 201 → Criou recurso
     │      └── 204 → Sucesso sem conteúdo
     │
     ├── Erro do cliente
     │      ├── 400 → Dados inválidos
     │      ├── 401 → Não autenticado
     │      ├── 403 → Sem permissão
     │      └── 404 → Recurso não encontrado
     │
     └── Erro do servidor
            └── 500 → Erro interno
```
````
