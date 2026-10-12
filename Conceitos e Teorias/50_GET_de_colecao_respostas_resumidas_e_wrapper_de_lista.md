# GET de Coleção, Respostas Resumidas e Wrapper de Lista

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo do endpoint GET](#objetivo-do-endpoint-get)
3. [Devolver somente as informações necessárias](#devolver-somente-as-informações-necessárias)
4. [Lista resumida x detalhes completos](#lista-resumida-x-detalhes-completos)
5. [Response resumida](#response-resumida)
6. [Endpoint GetAll](#endpoint-getall)
7. [200 OK x 204 No Content](#200-ok-x-204-no-content)
8. [Lista direta x objeto com lista](#lista-direta-x-objeto-com-lista)
9. [ResponseAllPetsJson](#responseallpetsjson)
10. [Inicialização da lista](#inicialização-da-lista)
11. [GetAllPetsUseCase](#getallpetsusecase)
12. [Uso de Any() no Controller](#uso-de-any-no-controller)
13. [Fluxo completo](#fluxo-completo)
14. [Resumo](#resumo)
15. [Visão Geral](#visão-geral)

---

## Contexto da aula

Nesta aula é criado um endpoint do tipo `GET` para devolver todos os pets associados ao usuário.

A ideia principal não é simplesmente retornar todos os dados existentes, mas pensar em quais informações realmente precisam ser exibidas em uma listagem.

---

## Objetivo do endpoint GET

O endpoint criado tem a responsabilidade de recuperar uma coleção de pets.

Exemplo de rota apresentada:

```text
GET /api/pet
```

A resposta poderá conter vários pets registrados.

Porém, a aula reforça que uma listagem não precisa necessariamente devolver todas as informações de cada item.

---

## Devolver somente as informações necessárias

Um dos principais pontos apresentados é:

```text
Não devolver dados que o front-end não precisa naquele momento.
```

Ao criar endpoints `GET`, é importante analisar quais informações realmente serão utilizadas pela tela que consumirá aquela resposta.

Isso evita retornar informações desnecessárias.

---

## Lista resumida x detalhes completos

A aula usa como exemplo o comportamento comum de aplicações que exibem listas.

Em uma tela de listagem, normalmente aparece apenas um resumo de cada item.

Exemplo:

```text
Pets
│
├── Charlie | Dog
├── Pipoca  | Cat
└── Mel     | Dog
```

Quando o usuário seleciona um item específico, uma nova chamada pode ser feita para recuperar todos os detalhes daquele registro.

```text
Lista resumida
      │
      │ usuário seleciona um item
      ▼
GET por ID
      │
      ▼
Detalhes completos
```

Para isso, o identificador do item precisa estar presente na resposta resumida.

---

## Informações retornadas na listagem

O pet completo utilizado no exemplo possui:

```text
Id
Name
Birthday
Type
```

Entretanto, para a listagem, a aula decide retornar apenas:

```text
Id
Name
Type
```

O `Id` é importante porque poderá ser utilizado posteriormente para buscar os detalhes daquele pet específico.

---

## Response resumida

Para representar essa versão reduzida do pet, é criada uma classe específica de resposta.

Conceitualmente:

```csharp
public class ResponseShortPetJson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PetType Type { get; set; }
}
```

O termo `Short` é utilizado para indicar que essa resposta contém apenas parte das informações disponíveis sobre o pet.

A ideia principal é possuir um contrato específico para cenários de listagem.

---

## Endpoint GetAll

No Controller é criado um endpoint `GET`.

Estrutura aproximada:

```csharp
[HttpGet]
public IActionResult GetAll()
{
    // execução do Use Case
}
```

Esse endpoint não precisa receber parâmetros para recuperar a coleção completa de pets do usuário.

---

## Documentando a resposta 200

Quando existem pets, o endpoint retorna sucesso com os dados.

```text
200 OK
```

Inicialmente, a resposta poderia ser documentada como uma lista:

```csharp
[ProducesResponseType(typeof(List<ResponseShortPetJson>), StatusCodes.Status200OK)]
```

Nesse caso, o JSON seria diretamente um array.

Exemplo:

```json
[
  {
    "id": 1,
    "name": "Charlie",
    "type": 1
  }
]
```

---

## 200 OK x 204 No Content

Segundo a organização apresentada na aula, o endpoint possui dois possíveis resultados principais.

### Quando existem pets

```text
200 OK
```

com os dados retornados.

### Quando não existem pets

```text
204 No Content
```

A requisição foi processada com sucesso, porém não há conteúdo para devolver.

Por isso, o Swagger é configurado para documentar as duas possibilidades.

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

---

## Lista direta x objeto com lista

A aula apresenta duas formas possíveis de estruturar a resposta.

### Opção 1 — retornar a lista diretamente

```json
[
  {
    "id": 1,
    "name": "Charlie",
    "type": 1
  }
]
```

### Opção 2 — retornar um objeto contendo a lista

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

O professor comenta que, por experiência em integrações com equipes de front-end, encontrou preferência pela segunda estrutura.

A orientação dada é conversar com a equipe responsável pelo front-end e definir em conjunto qual contrato será utilizado.

---

## ResponseAllPetsJson

Para utilizar a segunda abordagem, é criada uma classe responsável por encapsular a coleção.

Conceitualmente:

```csharp
public class ResponseAllPetsJson
{
    public List<ResponseShortPetJson> Pets { get; set; } = [];
}
```

Assim, o endpoint deixa de retornar diretamente:

```text
List<ResponseShortPetJson>
```

para retornar:

```text
ResponseAllPetsJson
```

A documentação passa a representar um objeto JSON contendo a propriedade `pets`.

---

## Inicialização da lista

A propriedade `Pets` é inicializada como uma lista vazia.

```csharp
public List<ResponseShortPetJson> Pets { get; set; } = [];
```

Dessa forma:

```text
Pets nunca começa como null
```

No pior cenário, ela será:

```text
lista vazia
```

Isso mantém o mesmo padrão utilizado anteriormente nas respostas de erro.

---

## GetAllPetsUseCase

Mantendo a separação entre API e regras de negócio, é criado um novo Use Case.

Estrutura apresentada:

```text
Application
└── UseCases
    └── Pets
        └── Get
            └── GetAllPetsUseCase
```

A classe é `public`, pois será utilizada pelo projeto da API.

Ela segue o padrão apresentado anteriormente de possuir uma função pública chamada:

```text
Execute
```

Como o endpoint busca todos os pets, esse método não precisa receber um ID ou Request como parâmetro.

Exemplo conceitual:

```csharp
public ResponseAllPetsJson Execute()
{
    return new ResponseAllPetsJson
    {
        Pets =
        [
            new ResponseShortPetJson
            {
                Id = 1,
                Name = "Charlie",
                Type = PetType.Dog
            }
        ]
    };
}
```

Nesta etapa, os dados são simulados apenas para demonstrar o fluxo entre Controller e Use Case.

---

## Chamando o Use Case no Controller

O Controller instancia o Use Case e executa sua regra.

Exemplo:

```csharp
var useCase = new GetAllPetsUseCase();
var response = useCase.Execute();
```

Agora é necessário analisar se a coleção contém algum elemento antes de decidir o status HTTP.

---

## Uso de Any() no Controller

A aula utiliza uma operação que verifica se a lista possui pelo menos um elemento.

Conceitualmente:

```csharp
response.Pets.Any()
```

`Any()` retorna um valor booleano.

```text
Possui pelo menos um elemento?
│
├── Sim → true
└── Não → false
```

Isso permite decidir qual resposta HTTP será enviada.

```csharp
if (response.Pets.Any())
    return Ok(response);

return NoContent();
```

---

## Fluxo com itens

```text
GET /api/pet
      │
      ▼
PetController.GetAll()
      │
      ▼
GetAllPetsUseCase.Execute()
      │
      ▼
ResponseAllPetsJson
      │
      ▼
Pets.Any() == true
      │
      ▼
200 OK
      │
      ▼
{
  "pets": [...]
}
```

---

## Fluxo sem itens

```text
GET /api/pet
      │
      ▼
PetController.GetAll()
      │
      ▼
GetAllPetsUseCase.Execute()
      │
      ▼
Pets = []
      │
      ▼
Pets.Any() == false
      │
      ▼
204 No Content
```

---

## Responsabilidades das classes

| Componente | Responsabilidade |
|---|---|
| `ResponseShortPetJson` | Representar apenas os dados necessários de um pet em uma listagem |
| `ResponseAllPetsJson` | Encapsular a coleção de pets |
| `GetAllPetsUseCase` | Executar a regra responsável por recuperar todos os pets |
| `PetController` | Receber a requisição, executar o Use Case e decidir a resposta HTTP |

---

## Resumo

| Conceito | Ideia apresentada |
|---|---|
| **GET de coleção** | Recupera uma lista de recursos |
| **Resposta resumida** | Uma listagem deve retornar apenas os campos necessários |
| **ID na lista** | Permite posteriormente buscar os detalhes daquele item |
| **ResponseShortPetJson** | DTO reduzido utilizado na listagem |
| **ResponseAllPetsJson** | Objeto que encapsula a lista de pets |
| **Lista inicializada** | Evita que a propriedade da coleção seja `null` |
| **200 OK** | Retornado quando existem itens |
| **204 No Content** | Retornado quando não existem itens, conforme a abordagem mostrada na aula |
| **Any()** | Verifica se uma coleção contém pelo menos um elemento |
| **GetAllPetsUseCase** | Caso de uso responsável pela recuperação da coleção |

---

## Visão Geral

```text
                    GET /api/pet
                         │
                         ▼
                    PetController
                         │
                         ▼
                 GetAllPetsUseCase
                         │
                         ▼
                ResponseAllPetsJson
                         │
                         ▼
                    Pets.Any()?
                    /         \
                  Sim         Não
                   │           │
                   ▼           ▼
               200 OK      204 No Content
                   │
                   ▼
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

> **Em resumo:** a aula cria um endpoint `GET` para listar todos os pets e reforça que endpoints de coleção devem retornar apenas as informações realmente necessárias para a tela. Para isso, é criada uma resposta resumida para cada pet e outra classe que encapsula a lista. O `GetAllPetsUseCase` devolve essa estrutura, e o Controller verifica se existem elementos para retornar `200 OK` com os dados ou `204 No Content` quando a coleção está vazia.