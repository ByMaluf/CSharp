# Communication: Requests, Responses e Modificadores de Acesso

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Estrutura utilizada no Petfolio](#estrutura-utilizada-no-petfolio)
3. [Criando o Controller da API](#criando-o-controller-da-api)
4. [Criando Requests e Responses no Communication](#criando-requests-e-responses-no-communication)
5. [Criando um enum compartilhado](#criando-um-enum-compartilhado)
6. [Por que usar `string.Empty`](#por-que-usar-stringempty)
7. [`internal` x `public` entre projetos](#internal-x-public-entre-projetos)
8. [Usando o Request no endpoint](#usando-o-request-no-endpoint)
9. [Usando uma Response](#usando-uma-response)
10. [`ProducesResponseType`](#producesresponsetype)
11. [Teste no Swagger](#teste-no-swagger)
12. [Fluxo entre os projetos](#fluxo-entre-os-projetos)
13. [Resumo](#resumo)
14. [Visão Geral](#visão-geral)

---

## Contexto da aula

A aula continua diretamente a estrutura criada anteriormente com três projetos:

```text
Petfolio.API
Petfolio.Application
Petfolio.Communication
```

Cada um possui uma responsabilidade diferente:

```text
API           → recebe e devolve dados HTTP
Application   → conterá as regras de negócio
Communication → contém contratos de Request e Response
```

Nesta aula, o objetivo é comprovar na prática que a API consegue utilizar classes definidas no projeto `Communication` por meio das referências configuradas anteriormente.

> **Observação sobre a transcrição:** em alguns trechos o reconhecimento de voz registra nomes como "Patch". Pelo contexto do projeto `Petfolio`, esses trechos se referem ao domínio de **pets**. O resumo mantém o conceito apresentado e utiliza `Pet` nos exemplos para deixar a estrutura coerente.

---

## Estrutura utilizada no Petfolio

A organização começa assim:

```text
Petfolio
│
├── Petfolio.API
│   └── Controllers
│
├── Petfolio.Application
│
└── Petfolio.Communication
```

Na aula, o projeto `Communication` passa a conter pastas específicas para os contratos usados na troca de dados:

```text
Petfolio.Communication
│
├── Requests
├── Responses
└── Enums
```

A ideia é não deixar as classes de entrada e saída dentro do projeto de API.

Elas ficam em um projeto compartilhado, que pode ser enxergado tanto pela API quanto pelo Application.

---

## Criando o Controller da API

Dentro do projeto:

```text
Petfolio.API
```

é criado um Controller do tipo API.

O exemplo da aula cria um endpoint de registro de pet com:

```text
POST
```

Estrutura conceitual:

```csharp
[HttpPost]
public IActionResult Register(...)
{
    return Created(...);
}
```

A função do endpoint é receber os dados do pet e, futuramente, repassá-los para a camada de regras de negócio.

---

## Criando Requests e Responses no Communication

No projeto:

```text
Petfolio.Communication
```

são criadas duas pastas:

```text
Requests
Responses
```

### Request

A classe de Request representa os dados que chegam na requisição.

No exemplo da aula, ela contém informações como:

```text
Name
Birthday
Type
```

Um formato equivalente seria:

```csharp
public class RequestRegisterPetJson
{
    public string Name { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public PetType Type { get; set; }
}
```

Essa classe funciona como o contrato de entrada do endpoint.

### Response

Na pasta `Responses` é criada uma classe para representar os dados devolvidos pela aplicação.

No exemplo da aula, ela possui campos como:

```text
Id
Name
```

Exemplo equivalente:

```csharp
public class ResponseRegisterPetJson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

A aula apresenta a Response como aquilo que poderá ser devolvido pela regra de negócio para a API, e então retornado ao cliente.

---

## Criando um enum compartilhado

Também é criada uma pasta para enums:

```text
Enums
```

Dentro dela é criado um enum para representar o tipo do pet.

A aula trabalha apenas com:

```text
Cat
Dog
```

Exemplo:

```csharp
public enum PetType
{
    Cat,
    Dog
}
```

A classe de Request utiliza esse enum:

```csharp
public PetType Type { get; set; }
```

Como o enum está em outro namespace, é necessário importar o namespace correspondente por meio de `using`.

---

## Por que usar `string.Empty`

Ao declarar uma propriedade do tipo `string`, a aula mostra um warning relacionado à possibilidade de o valor permanecer nulo.

Exemplo sem inicialização:

```csharp
public string Name { get; set; }
```

A solução utilizada na aula é inicializar com:

```csharp
public string Name { get; set; } = string.Empty;
```

Assim, no pior caso, a propriedade começa como:

```text
""
```

em vez de `null`.

---

## `internal` x `public` entre projetos

Este é um dos pontos centrais da aula.

Ao criar uma classe em uma Class Library, a aula mostra que ela estava declarada como:

```csharp
internal class RequestRegisterPetJson
```

Nesse caso, o Controller da API não conseguia utilizar essa classe mesmo existindo uma referência de projeto.

### `internal`

Segundo a explicação da aula:

```text
internal
↓
a classe só pode ser utilizada dentro do projeto onde foi declarada
```

Portanto:

```text
Petfolio.Communication
```

consegue utilizar a classe, mas:

```text
Petfolio.API
```

não consegue acessá-la externamente.

### `public`

Para permitir o uso da classe por outro projeto, ela é alterada para:

```csharp
public class RequestRegisterPetJson
```

O mesmo é feito com o enum:

```csharp
public enum PetType
```

Depois disso, o Visual Studio consegue sugerir o `using` e a API passa a enxergar os tipos definidos em `Petfolio.Communication`.

### Ideia principal

```text
Project Reference
        +
Tipo acessível publicamente
        ↓
Outro projeto consegue utilizar a classe
```

A referência entre os projetos, sozinha, não torna todos os tipos acessíveis.

O modificador de acesso também precisa permitir esse uso.

---

## Usando o Request no endpoint

Depois que a classe se torna `public`, o Controller consegue recebê-la no endpoint.

Conceitualmente:

```csharp
[HttpPost]
public IActionResult Register(
    [FromBody] RequestRegisterPetJson request)
{
    // processamento
}
```

O atributo:

```csharp
[FromBody]
```

indica que os dados enviados no corpo da requisição devem ser convertidos para o objeto de Request.

O fluxo passa a ser:

```text
JSON enviado pelo cliente
        ↓
HTTP Body
        ↓
[FromBody]
        ↓
RequestRegisterPetJson
```

---

## Usando uma Response

Depois de receber os dados, a aula prepara também uma classe de resposta.

A ideia apresentada é:

```text
API recebe Request
        ↓
repassa para Application
        ↓
regra de negócio processa
        ↓
Application devolve Response
        ↓
API retorna a Response ao cliente
```

Nesta aula, o foco ainda está na comunicação e no teste das classes compartilhadas.

A regra de negócio propriamente dita será criada posteriormente.

---

## `ProducesResponseType`

A aula também utiliza:

```csharp
[ProducesResponseType(...)]
```

para informar qual tipo de resposta o endpoint produz e qual status HTTP está associado a ela.

Estrutura apresentada:

```csharp
[ProducesResponseType(
    typeof(ResponseRegisterPetJson),
    StatusCodes.Status201Created)]
```

Isso permite que ferramentas como Swagger exibam melhor a documentação do endpoint.

O endpoint de criação trabalha com:

```text
201 Created
```

---

## Teste no Swagger

Depois da implementação, a API é executada com:

```text
F5
```

No Swagger aparece o endpoint:

```text
POST /api/pet
```

O Swagger mostra os campos do Request, incluindo:

```text
name
birthday
type
```

O enum aparece representado numericamente no exemplo da aula.

Durante o teste é enviado um pet com dados equivalentes a:

```text
Name     → Charlie
Type     → Dog
Birthday → 02/08/2020
```

Um breakpoint confirma que o objeto `request` foi preenchido corretamente.

A aula também observa que a forma como a data aparece durante a depuração pode variar conforme o idioma/configuração regional do computador.

---

## Fluxo entre os projetos

Com as classes criadas, temos o seguinte cenário:

```text
Cliente
   │
   │ JSON
   ▼
Petfolio.API
   │
   │ utiliza
   ▼
RequestRegisterPetJson
   │
   │ definido em
   ▼
Petfolio.Communication
```

A arquitetura preparada para as próximas aulas é:

```text
Cliente
   │
   ▼
Petfolio.API
   │
   │ Request
   ▼
Petfolio.Application
   │
   │ Response
   ▼
Petfolio.API
   │
   ▼
Cliente
```

Os contratos de Request e Response ficam em:

```text
Petfolio.Communication
```

para poderem ser compartilhados entre as camadas.

---

## Conceitos principais extraídos da aula

### 1. Separação dos contratos

```text
Request/Response não precisam ficar dentro da API
```

Eles podem ser colocados em uma Class Library específica de comunicação.

### 2. Project Reference não ignora modificadores de acesso

Mesmo com a referência configurada:

```text
internal → não acessível externamente
public   → acessível por outros projetos
```

### 3. Request e Response possuem responsabilidades diferentes

```text
Request  → dados recebidos
Response → dados devolvidos
```

### 4. Enums também podem ser contratos compartilhados

Se o Request depende de um enum, esse enum pode ficar no mesmo projeto de comunicação.

### 5. A API é apenas a porta de entrada

O Controller recebe os dados, mas a arquitetura está sendo preparada para que a regra de negócio fique no projeto `Application`.

---

## Resumo

| Elemento | Responsabilidade na aula |
|---|---|
| `Petfolio.API` | Receber a requisição HTTP e expor o endpoint |
| `Petfolio.Communication` | Guardar Requests, Responses e enums compartilhados |
| `Petfolio.Application` | Receberá as regras de negócio nas próximas etapas |
| Request | Representar os dados recebidos pelo endpoint |
| Response | Representar os dados devolvidos pela aplicação |
| Enum | Representar valores pré-definidos, como `Cat` e `Dog` |
| `[FromBody]` | Indicar que o Request vem do corpo da requisição |
| `internal` | Restringir o uso do tipo ao projeto em que foi declarado |
| `public` | Permitir que o tipo seja utilizado por outros projetos |
| `string.Empty` | Inicializar a string evitando valor inicial nulo |
| `[ProducesResponseType]` | Descrever o tipo e status de resposta do endpoint |

---

## Visão Geral

```text
                    PETFOLIO
                       │
         ┌─────────────┼─────────────┐
         │             │             │
         ▼             ▼             ▼
       API        Application   Communication
         │                           │
         │                           ├── Requests
         │                           ├── Responses
         │                           └── Enums
         │
         └────────────── usa tipos ───────►
```

### Fluxo do endpoint

```text
HTTP POST
   │
   ▼
PetController
   │
   │ [FromBody]
   ▼
RequestRegisterPetJson
   │
   ▼
Application   ← próxima etapa do curso
   │
   ▼
ResponseRegisterPetJson
   │
   ▼
201 Created
```

> **Em resumo:** a aula mostra como utilizar, dentro da API, classes definidas em outro projeto da Solution. Requests, Responses e enums ficam em `Petfolio.Communication`, mas precisam estar acessíveis como `public` para que a API consiga utilizá-los. O endpoint recebe o Request via `[FromBody]`, documenta sua Response com `[ProducesResponseType]` e é testado no Swagger, preparando o fluxo para a futura camada de regras de negócio em `Petfolio.Application`.
