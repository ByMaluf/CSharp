# Task Manager API

API REST em C#/.NET para gerenciamento de tarefas, criada como desafio prático do módulo de organização e arquitetura em camadas.

## Arquitetura

A solução foi separada em três projetos:

```text
TaskManagerApi
├── TaskManagerApi.Api
│   └── Controllers
├── TaskManagerApi.Application
│   ├── Entities
│   └── Services
└── TaskManagerApi.Communication
    ├── Requests
    └── Responses
```

Dependências:

```text
TaskManagerApi.Api
├── TaskManagerApi.Application
└── TaskManagerApi.Communication

TaskManagerApi.Application
└── TaskManagerApi.Communication
```

- **Api**: camada de comunicação HTTP e Controllers.
- **Application**: regras de negócio, validações e serviço de tarefas.
- **Communication**: contratos de Request e Response compartilhados pelas camadas.

## Campos da tarefa

| Campo | Tipo | Obrigatório | Regra |
|---|---|---|---|
| `id` | GUID | Sim | Gerado automaticamente |
| `name` | string | Sim | Não vazio e máximo de 100 caracteres |
| `description` | string | Não | Máximo de 500 caracteres |
| `priority` | string | Sim | `high`, `medium` ou `low` |
| `dueDate` | DateTime | Sim | Não pode estar no passado |
| `status` | string | Sim | `pending`, `inProgress` ou `completed` |

## Endpoints

| Método | Endpoint | Descrição | Sucesso |
|---|---|---|---|
| POST | `/api/tasks` | Criar uma tarefa | `201 Created` |
| GET | `/api/tasks` | Listar tarefas | `200 OK` |
| GET | `/api/tasks/{id}` | Buscar tarefa pelo ID | `200 OK` |
| PUT | `/api/tasks/{id}` | Atualizar uma tarefa | `200 OK` |
| DELETE | `/api/tasks/{id}` | Excluir uma tarefa | `204 No Content` |

Erros de validação retornam `400 Bad Request`. Quando o ID não existe, os endpoints específicos retornam `404 Not Found`.

## Exemplo de requisição

```json
{
  "name": "Estudar arquitetura em camadas",
  "description": "Revisar Controllers, Services, Requests e Responses",
  "priority": "high",
  "dueDate": "2030-12-31T18:00:00",
  "status": "pending"
}
```

## Swagger

O projeto utiliza Swagger/OpenAPI para documentar e testar os endpoints.

Ao executar a API, abra a rota:

```text
/swagger
```

## Executando

Na pasta `Codigo/TaskManagerApi`:

```bash
dotnet restore
dotnet run --project TaskManagerApi.Api/TaskManagerApi.Api.csproj
```

Também é possível abrir `TaskManagerApi.slnx` no Visual Studio e definir `TaskManagerApi.Api` como projeto de inicialização.

## Persistência

O desafio não exige banco de dados. Por isso, esta implementação utiliza armazenamento **em memória** por meio de um serviço singleton. Os dados são perdidos quando a aplicação é encerrada.

## Observações

A mesma classe de request é utilizada no POST e no PUT porque os campos editáveis são os mesmos. As validações ficam na camada `Application`, mantendo o Controller responsável apenas por receber a requisição, chamar o serviço e transformar o resultado em uma resposta HTTP apropriada.
