# Bookstore API

API REST em ASP.NET Core para gerenciamento de livros, criada a partir do desafio opcional do módulo.

## Requisitos atendidos

- Criar livro.
- Listar todos os livros.
- Buscar livro por ID.
- Atualizar livro.
- Excluir livro.
- Validar título, autor, gênero, preço e estoque.
- Gerar o ID automaticamente com GUID.
- Registrar `CreatedAt` na criação e `UpdatedAt` nas alterações.
- Retornar `409 Conflict` quando já existir um livro com a mesma combinação de título e autor.
- Retornar status HTTP apropriados para validação, criação, consulta, conflito, recurso inexistente, atualização, exclusão e erros inesperados.

## Decisões do desafio

O enunciado informa que o gênero deve pertencer a uma lista válida, mas apresenta apenas exemplos e não fecha a lista completa. Nesta implementação foram definidos:

- ficção
- romance
- mistério
- fantasia
- terror
- biografia
- história
- tecnologia
- autoajuda
- infantil

O endpoint de listagem também pede filtros opcionais sem especificar quais. Foram implementados:

- `title`
- `author`
- `genre`
- `minPrice`
- `maxPrice`
- `inStock`

A regra de duplicidade foi interpretada como **mesmo título + mesmo autor**, ignorando diferenças entre maiúsculas e minúsculas. Isso permite que o mesmo autor tenha vários livros diferentes.

## Persistência

O desafio não exige banco de dados. Por isso, os dados são armazenados em memória enquanto a aplicação está em execução.

Ao reiniciar a API, os livros cadastrados são perdidos.

## Executando

Na pasta `Codigo/BookstoreApi`:

```bash
dotnet run --project BookstoreApi/BookstoreApi.csproj
```

O perfil HTTP utiliza:

```text
http://localhost:5164
```

## Endpoints

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/books` | Cria um livro |
| GET | `/api/books` | Lista livros e aceita filtros opcionais |
| GET | `/api/books/{id}` | Busca um livro pelo GUID |
| PUT | `/api/books/{id}` | Atualiza um livro |
| DELETE | `/api/books/{id}` | Exclui um livro |

## Exemplo de criação

```json
{
  "title": "O Hobbit",
  "author": "J. R. R. Tolkien",
  "genre": "fantasia",
  "price": 49.90,
  "stock": 10
}
```

A criação retorna `201 Created`, o livro criado e um header `Location` apontando para `GET /api/books/{id}`.

## Listagem com filtros

```http
GET /api/books?genre=fantasia
GET /api/books?author=Tolkien
GET /api/books?minPrice=20&maxPrice=100
GET /api/books?inStock=true
GET /api/books?title=Hobbit&genre=fantasia
```

## Status HTTP

| Status | Uso nesta API |
|---|---|
| 200 | Consulta ou atualização com conteúdo |
| 201 | Livro criado |
| 204 | Livro excluído |
| 400 | DTO ou filtros inválidos |
| 404 | Livro não encontrado ou rota inexistente |
| 409 | Mesmo título + autor já cadastrado |
| 500 | Erro inesperado não tratado |

## Conceitos do módulo aplicados

- Controller base abstrato.
- Herança de atributos de Controller.
- DTO base abstrato para compartilhar validações.
- DTOs concretos marcados como `sealed`.
- Data Annotations e validação automática com `[ApiController]`.
- Rotas REST com métodos HTTP distintos.
- `CreatedAtAction` na criação do recurso.

## Estrutura

```text
BookstoreApi
├── Controllers
├── Domain
├── Entities
├── Models/Requests
├── Services
├── Validation
├── Properties
├── BookstoreApi.csproj
├── BookstoreApi.http
└── Program.cs
```
