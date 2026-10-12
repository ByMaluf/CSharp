# GET por ID, 200 OK, 404 Not Found e GetById Use Case

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Objetivo do endpoint GET por ID](#objetivo-do-endpoint-get-por-id)
3. [Diferença entre listar e buscar por ID](#diferença-entre-listar-e-buscar-por-id)
4. [ID recebido pela rota](#id-recebido-pela-rota)
5. [Response completa do pet](#response-completa-do-pet)
6. [Documentando 200 OK](#documentando-200-ok)
7. [Documentando 404 Not Found](#documentando-404-not-found)
8. [Reutilização de ResponseErrorsJson](#reutilização-de-responseerrorsjson)
9. [Criando o GetPetByIdUseCase](#criando-o-getpetbyidusecase)
10. [Parâmetros nomeados no DateTime](#parâmetros-nomeados-no-datetime)
11. [Responsabilidade do Controller](#responsabilidade-do-controller)
12. [Fluxo futuro com banco de dados](#fluxo-futuro-com-banco-de-dados)
13. [Tratamento do caso não encontrado](#tratamento-do-caso-não-encontrado)
14. [Fluxo completo](#fluxo-completo)
15. [Resumo](#resumo)

---

## Visão Geral

Nesta aula é criado um endpoint `GET` específico para recuperar um único pet pelo seu identificador.

A ideia é diferente do endpoint de listagem criado anteriormente:

```text
GET /api/pet
```

retorna uma coleção resumida, enquanto:

```text
GET /api/pet/{id}
```

retorna todas as informações de um pet específico.

A aula apresenta dois possíveis resultados principais:

```text
Pet encontrado     → 200 OK
Pet não encontrado → 404 Not Found
```

---

## Objetivo do endpoint GET por ID

O endpoint recebe um identificador e procura um pet correspondente.

Exemplo conceitual:

```http
GET /api/pet/7
```

O valor `7` identifica qual pet deve ser recuperado.

Se o recurso existir, a API devolve os seus dados completos.

Se não existir, a API deve informar que o recurso não foi encontrado.

---

## Diferença entre listar e buscar por ID

A aula anterior trabalhou com uma resposta resumida para uma lista de pets.

Exemplo:

```json
{
  "pets": [
    {
      "id": 1,
      "name": "Charlie",
      "type": 1
    }
  ]
}
```

Em uma consulta individual, o objetivo passa a ser devolver todas as informações relevantes daquele recurso.

Exemplo:

```json
{
  "id": 7,
  "name": "Pipoca",
  "birthday": "2023-01-01T00:00:00",
  "type": 0
}
```

A ideia apresentada é:

```text
Listagem
↓
Resposta resumida

Detalhes por ID
↓
Resposta completa
```

---

## ID recebido pela rota

O endpoint precisa receber o ID do pet pela rota.

Estrutura apresentada:

```csharp
[HttpGet]
[Route("{id}")]
public IActionResult GetById([FromRoute] int id)
{
    // ...
}
```

O fluxo fica:

```text
GET /api/pet/7
             │
             ▼
            id = 7
```

O ID informa qual recurso deve ser recuperado.

---

## Response completa do pet

Como o endpoint precisa devolver todas as informações, é criada uma classe específica de resposta.

Exemplo equivalente ao apresentado:

```csharp
public class ResponsePetJson
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime Birthday { get; set; }

    public PetType Type { get; set; }
}
```

Ela representa o contrato completo devolvido pela API para um pet individual.

### Comparação

| Response | Uso |
|---|---|
| `ResponseShortPetJson` | Listagens resumidas |
| `ResponsePetJson` | Detalhes completos de um pet |

---

## Documentando 200 OK

Quando o pet é encontrado, o endpoint devolve:

```text
200 OK
```

junto com um objeto do tipo:

```text
ResponsePetJson
```

A documentação é feita com `ProducesResponseType`.

Exemplo:

```csharp
[ProducesResponseType(typeof(ResponsePetJson), StatusCodes.Status200OK)]
```

Assim o Swagger consegue informar que uma resposta de sucesso possui aquele formato.

---

## Documentando 404 Not Found

Existe outro cenário importante:

```text
O ID informado não corresponde a nenhum pet
```

Nesse caso, a aula define o retorno:

```text
404 Not Found
```

Esse código indica que o recurso solicitado não foi encontrado.

Exemplo de documentação:

```csharp
[ProducesResponseType(typeof(ResponseErrorsJson), StatusCodes.Status404NotFound)]
```

---

## Reutilização de ResponseErrorsJson

A mesma estrutura de erro criada anteriormente pode ser utilizada também para `404`.

Exemplo:

```csharp
public class ResponseErrorsJson
{
    public List<string> Errors { get; set; } = [];
}
```

Uma resposta poderia conter:

```json
{
  "errors": [
    "Pet com ID 7 não encontrado."
  ]
}
```

O contrato de erro continua o mesmo.

O que muda é o status HTTP:

```text
400 Bad Request
```

para erros de requisição/validação, ou:

```text
404 Not Found
```

quando o recurso procurado não existe.

---

## Criando o GetPetByIdUseCase

Mantendo o padrão da camada `Application`, é criado um caso de uso específico para recuperar um pet pelo ID.

Estrutura conceitual:

```text
UseCases
└── Pets
    └── GetById
        └── GetPetByIdUseCase.cs
```

A classe é pública porque será utilizada pela API.

Exemplo:

```csharp
public class GetPetByIdUseCase
{
    public ResponsePetJson Execute(int id)
    {
        // regra de negócio
    }
}
```

O método público continua seguindo a convenção:

```text
Execute
```

---

## Retorno simulado no Use Case

Como a aula ainda não trabalha com banco de dados, o retorno é simulado.

Exemplo equivalente:

```csharp
public ResponsePetJson Execute(int id)
{
    return new ResponsePetJson
    {
        Id = id,
        Name = "Pipoca",
        Birthday = new DateTime(
            year: 2023,
            month: 1,
            day: 1),
        Type = PetType.Cat
    };
}
```

O `id` recebido pelo endpoint é repassado ao Use Case.

---

## Parâmetros nomeados no DateTime

A aula também demonstra o uso de parâmetros nomeados.

Em vez de:

```csharp
new DateTime(2023, 1, 1)
```

é utilizada uma forma mais explícita:

```csharp
new DateTime(
    year: 2023,
    month: 1,
    day: 1)
```

Isso deixa mais claro o significado de cada valor.

### Comparação

```text
new DateTime(2023, 1, 1)
                     │
                     └── exige lembrar a ordem
```

versus:

```text
year: 2023
month: 1
day: 1
```

A intenção fica explícita no próprio código.

---

## Responsabilidade do Controller

O Controller recebe a requisição e repassa os dados para a regra de negócio.

Exemplo:

```csharp
var useCase = new GetPetByIdUseCase();

var response = useCase.Execute(id);

return Ok(response);
```

Fluxo:

```text
Controller
   │
   │ recebe id
   ▼
GetPetByIdUseCase
   │
   │ devolve ResponsePetJson
   ▼
Controller
   │
   ▼
200 OK
```

A aula reforça que a regra de negócio não deve ficar dentro do endpoint.

O Controller pode fazer pequenas decisões relacionadas à resposta HTTP, mas a lógica de negócio deve permanecer no Use Case.

---

## Fluxo futuro com banco de dados

A implementação feita na aula ainda utiliza dados simulados.

Porém, o fluxo futuro apresentado é:

```text
API
 │
 │ id
 ▼
Use Case
 │
 │ consulta
 ▼
Banco de Dados
```

Se o banco encontrar o recurso:

```text
Banco
  │
  ▼
Entidade Pet
  │
  ▼
Use Case
  │
  ▼
ResponsePetJson
  │
  ▼
Controller
  │
  ▼
200 OK
```

---

## Tratamento do caso não encontrado

A aula ainda não implementa o `404` de fato, mas explica como esse fluxo será tratado futuramente.

O banco de dados poderá retornar algo equivalente a:

```text
null
```

quando não encontrar o pet.

Fluxo conceitual:

```text
Use Case consulta banco
        │
        ▼
Pet encontrado?
   ├── Sim → cria ResponsePetJson
   │          ↓
   │        200 OK
   │
   └── Não → tratamento de recurso não encontrado
              ↓
           404 Not Found
```

Segundo a aula, esse cenário será tratado por meio de exceções em conteúdos posteriores.

A ideia apresentada é:

```text
Banco retorna null
       ↓
Use Case identifica que o pet não existe
       ↓
Lança uma exceção de não encontrado
       ↓
Tratamento global da exceção
       ↓
API responde 404 Not Found
```

A implementação desse tratamento não é feita nesta aula.

---

## 200 e 404 possuem significados diferentes

| Situação | Status | Corpo |
|---|---:|---|
| Pet encontrado | `200 OK` | `ResponsePetJson` |
| Pet não encontrado | `404 Not Found` | `ResponseErrorsJson` |

Isso permite que quem consome a API saiba distinguir claramente:

```text
Requisição executada e recurso encontrado
```

ou:

```text
Recurso solicitado não existe
```

---

## Estrutura conceitual do endpoint

```csharp
[HttpGet]
[Route("{id}")]
[ProducesResponseType(typeof(ResponsePetJson), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ResponseErrorsJson), StatusCodes.Status404NotFound)]
public IActionResult GetById([FromRoute] int id)
{
    var useCase = new GetPetByIdUseCase();

    var response = useCase.Execute(id);

    return Ok(response);
}
```

Nesta etapa, esse código cobre apenas o caminho de sucesso.

O tratamento real do `404` será implementado posteriormente, conforme explicado na aula.

---

## Fluxo completo

### Pet encontrado

```text
GET /api/pet/7
       │
       ▼
PetController
       │
       │ id = 7
       ▼
GetPetByIdUseCase.Execute(7)
       │
       ▼
Busca o pet
       │
       ▼
Cria ResponsePetJson
       │
       ▼
Controller
       │
       ▼
200 OK
```

### Pet não encontrado — fluxo futuro apresentado

```text
GET /api/pet/999
       │
       ▼
Controller
       │
       ▼
GetPetByIdUseCase
       │
       ▼
Banco de dados
       │
       ▼
null
       │
       ▼
Exceção de recurso não encontrado
       │
       ▼
Tratamento da exceção
       │
       ▼
404 Not Found
       │
       ▼
ResponseErrorsJson
```

---

## Resumo

| Conceito | Aplicação na aula |
|---|---|
| `GET /{id}` | Recupera um recurso específico |
| `[FromRoute]` | Recebe o ID pela rota |
| `ResponsePetJson` | Representa os dados completos de um pet |
| `200 OK` | Pet encontrado com sucesso |
| `404 Not Found` | Pet não encontrado |
| `ResponseErrorsJson` | Contrato reutilizado para mensagens de erro |
| `GetPetByIdUseCase` | Regra de negócio responsável pela busca por ID |
| `Execute(int id)` | Recebe o identificador e executa o caso de uso |
| Parâmetros nomeados | Tornam chamadas como `DateTime` mais legíveis |
| Exceção | Estratégia futura apresentada para transformar recurso inexistente em `404` |

---

## Visão Geral

```text
                 GET /api/pet/{id}
                         │
                         ▼
                  PetController
                         │
                         │ id
                         ▼
                GetPetByIdUseCase
                         │
                         ▼
                   Busca o pet
                    /         \
                   /           \
                  ▼             ▼
             Encontrado      Não encontrado
                  │               │
                  ▼               ▼
         ResponsePetJson      Exceção futura
                  │               │
                  ▼               ▼
              200 OK          404 Not Found
                                  │
                                  ▼
                         ResponseErrorsJson
```

> **Em resumo:** o endpoint GET por ID recebe o identificador pela rota e repassa esse valor para um Use Case específico. Quando o recurso existe, o Use Case devolve uma resposta completa e a API responde `200 OK`. Quando o recurso não existe, a aula explica que futuramente o Use Case identificará esse cenário e lançará uma exceção que será tratada para produzir um `404 Not Found` com uma resposta padronizada de erro.