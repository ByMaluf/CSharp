# DELETE por ID, 204 No Content, 404 Not Found e Delete Use Case

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo do endpoint DELETE](#objetivo-do-endpoint-delete)
3. [ID recebido pela rota](#id-recebido-pela-rota)
4. [Respostas possíveis](#respostas-possíveis)
5. [204 No Content no sucesso](#204-no-content-no-sucesso)
6. [404 Not Found quando o recurso não existe](#404-not-found-quando-o-recurso-não-existe)
7. [Documentando o DELETE no Swagger](#documentando-o-delete-no-swagger)
8. [Criando o DeletePetByIdUseCase](#criando-o-deletepetbyidusecase)
9. [Fluxo Controller → Use Case](#fluxo-controller--use-case)
10. [CRUD completo](#crud-completo)
11. [Resumo](#resumo)
12. [Visão Geral](#visão-geral)

---

## Contexto da aula

Depois de criar operações para registrar, listar, consultar por ID e atualizar um pet, falta a operação de exclusão para completar o CRUD.

A aula cria um endpoint HTTP do tipo:

```text
DELETE
```

A finalidade é remover um registro específico que já havia sido criado anteriormente.

---

## Objetivo do endpoint DELETE

O endpoint precisa saber exatamente qual pet deve ser excluído.

Por isso, ele recebe o identificador do recurso.

Exemplo conceitual:

```http
DELETE /api/pet/7
```

Nesse caso:

```text
7
```

é o ID do pet que deve ser removido.

---

## ID recebido pela rota

Assim como nos endpoints de atualização e consulta específica, o ID é recebido pela rota.

Exemplo apresentado na aula:

```csharp
[HttpDelete]
[Route("{id}")]
public IActionResult Delete([FromRoute] int id)
{
    return NoContent();
}
```

A ideia é:

```text
Cliente
   │
   │ DELETE /api/pet/7
   ▼
Controller
   │
   └── id = 7
```

A rota identifica qual registro deve ser excluído.

---

## Respostas possíveis

A aula apresenta dois resultados principais para o endpoint.

```text
DELETE por ID
│
├── recurso encontrado e removido
│      └── 204 No Content
│
└── recurso não encontrado
       └── 404 Not Found
```

---

## 204 No Content no sucesso

Quando a exclusão acontece corretamente, a aula opta por não devolver um objeto no corpo da resposta.

Por isso utiliza:

```csharp
return NoContent();
```

Resultado HTTP:

```text
204 No Content
```

O significado apresentado é:

```text
A operação foi executada com sucesso
+
Não existe conteúdo para devolver
```

A aula comenta que também seria possível utilizar `200 OK` caso existisse alguma informação relevante para retornar após a exclusão.

No cenário apresentado, porém, isso não é considerado necessário.

---

## 404 Not Found quando o recurso não existe

Como o endpoint recebe um ID específico, existe a possibilidade de não existir nenhum pet correspondente àquele identificador.

Exemplo:

```text
DELETE /api/pet/1234
```

Se o pet não existir, a resposta prevista é:

```text
404 Not Found
```

A resposta de erro pode utilizar a mesma estrutura de erro criada anteriormente:

```text
ResponseErrorsJson
```

com uma mensagem semelhante a:

```text
Pet não encontrado com o ID 1234.
```

A estrutura continua baseada em uma lista de mensagens de erro.

---

## Tratamento do 404

Nesta etapa do curso, o fluxo real do `404` ainda não é implementado dentro do endpoint.

A aula mantém a mesma ideia apresentada no GET por ID:

```text
Controller
   │
   ▼
Use Case
   │
   ▼
Busca o recurso
   │
   ├── encontrou → continua a operação
   │
   └── não encontrou → lança uma exceção de não encontrado
                         │
                         ▼
                 tratamento de exceções
                         │
                         ▼
                   404 Not Found
```

O tratamento completo de exceções fica para aulas posteriores.

---

## Documentando o DELETE no Swagger

Os possíveis retornos são documentados com `ProducesResponseType`.

Para o sucesso:

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

Para o recurso inexistente:

```csharp
[ProducesResponseType(
    typeof(ResponseErrorsJson),
    StatusCodes.Status404NotFound)]
```

Estrutura conceitual:

```csharp
[HttpDelete]
[Route("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ResponseErrorsJson),
    StatusCodes.Status404NotFound)]
public IActionResult Delete([FromRoute] int id)
{
    // regra de negócio

    return NoContent();
}
```

Com isso, o Swagger passa a documentar os dois resultados esperados.

---

## Criando o DeletePetByIdUseCase

Seguindo a organização dos casos de uso anteriores, a aula cria uma nova pasta dentro do contexto de pets:

```text
UseCases
└── Pets
    └── Delete
```

Dentro dela é criada uma classe semelhante a:

```text
DeletePetByIdUseCase
```

A classe precisa ser `public`, pois será utilizada pelo projeto da API.

Exemplo:

```csharp
public class DeletePetByIdUseCase
{
    public void Execute(int id)
    {
        // regra de negócio da exclusão
    }
}
```

---

## Por que o Execute retorna void?

Nesse caso, a regra de negócio não precisa devolver um objeto de resposta no fluxo de sucesso.

O endpoint apenas precisa saber que a exclusão foi executada corretamente.

Portanto:

```csharp
public void Execute(int id)
```

é suficiente para a implementação apresentada.

A responsabilidade futura dessa regra seria algo próximo de:

```text
Receber ID
↓
Buscar pet
↓
Validar existência
↓
Excluir registro
```

---

## Fluxo Controller → Use Case

No Controller, o caso de uso é instanciado e executado.

Exemplo:

```csharp
var useCase = new DeletePetByIdUseCase();

useCase.Execute(id);

return NoContent();
```

Fluxo:

```text
DELETE /api/pet/{id}
        │
        ▼
   PetController
        │
        │ id
        ▼
DeletePetByIdUseCase
        │
        │ executa exclusão
        ▼
   Controller
        │
        ▼
204 No Content
```

Caso o pet não exista, o fluxo futuro previsto é interrompido pela regra de negócio e convertido em `404 Not Found` pelo mecanismo de tratamento de exceções.

---

## Organização dos Use Cases

Com essa aula, o contexto de pets passa a possuir casos de uso para as principais operações do CRUD.

```text
Petfolio.Application
└── UseCases
    └── Pets
        ├── Register
        │   └── RegisterPetUseCase
        │
        ├── Get
        │   └── GetAllPetsUseCase
        │
        ├── GetById
        │   └── GetPetByIdUseCase
        │
        ├── Update
        │   └── UpdatePetUseCase
        │
        └── Delete
            └── DeletePetByIdUseCase
```

Essa estrutura mantém cada operação em uma classe responsável por um único caso de uso.

---

## CRUD completo

Ao final da aula, o conjunto de endpoints apresentado no Petfolio possui todas as operações básicas de um CRUD.

| Operação | Verbo HTTP | Objetivo |
|---|---|---|
| **Create** | `POST` | Registrar um pet |
| **Read All** | `GET` | Listar os pets |
| **Read By ID** | `GET` | Recuperar um pet específico |
| **Update** | `PUT` | Atualizar um pet |
| **Delete** | `DELETE` | Excluir um pet |

Representação:

```text
CRUD
│
├── C → Create → POST
├── R → Read   → GET
├── U → Update → PUT
└── D → Delete → DELETE
```

---

## Resumo

| Conceito | Aplicação na aula |
|---|---|
| `DELETE` | Remove um recurso |
| ID na rota | Identifica o pet que será excluído |
| `[FromRoute]` | Obtém o ID da URL |
| `204 No Content` | Exclusão executada com sucesso sem corpo de resposta |
| `404 Not Found` | Pet informado pelo ID não existe |
| `ResponseErrorsJson` | Estrutura reutilizada para documentar o erro |
| `ProducesResponseType` | Documenta os retornos possíveis no Swagger |
| `DeletePetByIdUseCase` | Contém a regra de negócio da exclusão |
| `Execute(int id)` | Executa a operação sobre o pet informado |

---

## Visão Geral

```text
Cliente
   │
   │ DELETE /api/pet/7
   ▼
PetController
   │
   │ id = 7
   ▼
DeletePetByIdUseCase
   │
   ├── pet encontrado
   │       │
   │       ├── exclui o registro
   │       │
   │       └──► 204 No Content
   │
   └── pet não encontrado
           │
           └──► exceção
                  │
                  ▼
             tratamento
                  │
                  ▼
             404 Not Found
```

> **Em resumo:** o endpoint `DELETE` recebe o ID do pet pela rota e delega a exclusão ao `DeletePetByIdUseCase`. Quando a operação é concluída com sucesso, a API retorna `204 No Content`. Caso o recurso não exista, o retorno previsto é `404 Not Found`, seguindo o fluxo de tratamento de exceções que será aprofundado posteriormente. Com essa operação, o CRUD de pets apresentado no módulo fica completo.
