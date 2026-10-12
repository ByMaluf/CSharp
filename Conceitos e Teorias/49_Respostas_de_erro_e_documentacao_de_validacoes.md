# Respostas de Erro e Documentação de Validações

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Por que validar uma requisição](#por-que-validar-uma-requisição)
3. [Por que retornar uma lista de erros](#por-que-retornar-uma-lista-de-erros)
4. [Criando o ResponseErrorsJson](#criando-o-responseerrorsjson)
5. [Evitando listas nulas](#evitando-listas-nulas)
6. [Inicialização simplificada de coleções](#inicialização-simplificada-de-coleções)
7. [Documentando 400 Bad Request](#documentando-400-bad-request)
8. [Aplicando no POST e no PUT](#aplicando-no-post-e-no-put)
9. [Como o Swagger representa os possíveis retornos](#como-o-swagger-representa-os-possíveis-retornos)
10. [Fluxo esperado da validação](#fluxo-esperado-da-validação)
11. [Resumo](#resumo)
12. [Visão Geral](#visão-geral)

---

## Contexto da aula

Até este ponto, a API já possui endpoints para:

```text
registrar um pet
atualizar um pet
```

A aula introduz um novo cenário:

```text
E se a API receber informações inválidas?
```

Nesse caso, a aplicação precisa validar os dados recebidos e devolver informações claras sobre os erros encontrados.

---

## Por que validar uma requisição

Uma requisição pode conter dados que não fazem sentido para a regra de negócio.

Exemplos apresentados na aula:

```text
Nome vazio
Data de nascimento no futuro
```

Essas informações não devem simplesmente continuar pelo fluxo da aplicação.

A regra de negócio deve identificar os problemas e impedir que o processamento continue normalmente.

Fluxo esperado:

```text
API recebe a requisição
        │
        ▼
Regra de negócio valida os dados
        │
        ├── válidos → continua o fluxo
        │
        └── inválidos → devolve os erros
```

---

## Por que retornar uma lista de erros

A aula apresenta como boa prática retornar uma coleção de mensagens de erro em vez de apenas uma mensagem.

Imagine uma requisição com dois problemas:

```text
Nome vazio
Data de nascimento inválida
```

Se a API devolver apenas o primeiro erro:

```text
"Nome inválido"
```

o usuário corrige o nome e envia novamente.

Só então receberia:

```text
"Data de nascimento inválida"
```

Isso gera múltiplas tentativas desnecessárias.

A proposta apresentada é:

```text
Validar tudo que precisa ser validado
↓
Agrupar as mensagens
↓
Devolver todas de uma vez
```

Exemplo conceitual:

```json
{
  "errors": [
    "O nome é obrigatório.",
    "A data de nascimento não pode estar no futuro."
  ]
}
```

---

## Criando o ResponseErrorsJson

No projeto:

```text
Petfolio.Communication
```

na pasta:

```text
Responses
```

é criada uma classe destinada a representar respostas de erro.

Estrutura apresentada:

```csharp
public class ResponseErrorsJson
{
    public List<string> Errors { get; set; } = [];
}
```

Sua responsabilidade é simples:

```text
Conter as mensagens de erro que serão devolvidas pela API
```

A propriedade principal é:

```csharp
List<string> Errors
```

Ou seja, uma lista de strings.

---

## Evitando listas nulas

Se a propriedade fosse declarada apenas assim:

```csharp
public List<string> Errors { get; set; }
```

ela poderia permanecer com valor:

```text
null
```

Nesse cenário, uma operação como:

```csharp
response.Errors.Add("Erro");
```

poderia falhar porque a lista ainda não teria sido instanciada.

Por isso, a aula inicializa a coleção desde a declaração da propriedade.

---

## Inicialização simplificada de coleções

Uma forma tradicional seria:

```csharp
public List<string> Errors { get; set; } = new List<string>();
```

A aula mostra a sintaxe simplificada utilizada nas versões mais novas do C#:

```csharp
public List<string> Errors { get; set; } = [];
```

A ideia é a mesma:

```text
Sempre que ResponseErrorsJson for criado
↓
Errors já começa como uma lista vazia
↓
Errors não começa como null
```

Exemplo:

```csharp
var response = new ResponseErrorsJson();

response.Errors.Add("Nome inválido");
```

Como `Errors` já foi inicializado, a lista pode ser utilizada imediatamente.

---

## Documentando 400 Bad Request

Além de criar a classe de resposta, a aula atualiza a documentação dos endpoints utilizando:

```csharp
[ProducesResponseType]
```

Para representar uma resposta de erro de validação:

```csharp
[ProducesResponseType(
    typeof(ResponseErrorsJson),
    StatusCodes.Status400BadRequest)]
```

Isso informa que o endpoint pode retornar:

```text
HTTP 400 Bad Request
```

com um corpo no formato:

```text
ResponseErrorsJson
```

---

## Aplicando no POST e no PUT

### POST

O endpoint de criação já possui uma resposta de sucesso:

```text
201 Created
```

Agora também passa a documentar:

```text
400 Bad Request
```

Representação conceitual:

```csharp
[ProducesResponseType(
    typeof(ResponseRegisterPetJson),
    StatusCodes.Status201Created)]

[ProducesResponseType(
    typeof(ResponseErrorsJson),
    StatusCodes.Status400BadRequest)]
```

### PUT

O endpoint de atualização retorna, em caso de sucesso:

```text
204 No Content
```

Também pode retornar:

```text
400 Bad Request
```

Exemplo conceitual:

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]

[ProducesResponseType(
    typeof(ResponseErrorsJson),
    StatusCodes.Status400BadRequest)]
```

---

## Como o Swagger representa os possíveis retornos

Após atualizar os atributos e executar a API, o Swagger passa a mostrar os diferentes resultados esperados para cada endpoint.

### POST

```text
201 Created
└── resposta de sucesso

400 Bad Request
└── ResponseErrorsJson
    └── errors: string[]
```

### PUT

```text
204 No Content
└── atualização realizada

400 Bad Request
└── ResponseErrorsJson
    └── errors: string[]
```

Assim, a documentação deixa explícito que uma operação pode ter mais de um resultado possível.

---

## Fluxo esperado da validação

Embora a aula ainda não implemente as validações propriamente ditas, ela prepara o contrato de resposta para quando elas forem adicionadas.

O fluxo apresentado pode ser representado assim:

```text
Cliente envia Request
        │
        ▼
       API
        │
        ▼
   Use Case / Regra
     de negócio
        │
        ├── dados válidos
        │       │
        │       ▼
        │   operação executada
        │       │
        │       ├── POST → 201
        │       └── PUT  → 204
        │
        └── dados inválidos
                │
                ▼
       ResponseErrorsJson
                │
                ▼
        400 Bad Request
```

A aula deixa claro que o foco neste momento é preparar a organização e a documentação das respostas; as regras de validação serão aprofundadas posteriormente.

---

## Resumo

| Conceito | Papel |
|---|---|
| **ResponseErrorsJson** | Padroniza a resposta de erro |
| **List<string>** | Permite devolver várias mensagens de erro |
| **`=[]`** | Inicializa a coleção vazia e evita `null` |
| **400 Bad Request** | Indica que os dados enviados não são válidos para a requisição |
| **ProducesResponseType** | Documenta os possíveis retornos do endpoint |
| **Swagger** | Exibe os contratos de sucesso e de erro |

### Ideia principal

```text
Não devolver apenas o primeiro erro
↓
Validar tudo
↓
Agrupar as mensagens
↓
Retornar uma lista de erros
```

---

## Visão Geral

```text
             REQUEST
                │
                ▼
               API
                │
                ▼
            USE CASE
                │
       ┌────────┴────────┐
       │                 │
       ▼                 ▼
    Válido             Inválido
       │                 │
       ▼                 ▼
POST → 201       ResponseErrorsJson
PUT  → 204               │
                         ▼
                 400 Bad Request
                         │
                         ▼
                  List<string>
```

> **Em resumo:** a aula cria uma resposta padronizada de erros no projeto `Communication`, utilizando uma lista de strings para permitir que vários problemas sejam informados de uma vez. A lista é inicializada vazia para evitar `null`, e os endpoints POST e PUT passam a documentar também o retorno `400 Bad Request` por meio de `ProducesResponseType`, fazendo com que o Swagger mostre corretamente os contratos de sucesso e de erro.
