# Endpoint PUT, reutilização de Request e Update Use Case

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Atualização de um recurso](#atualização-de-um-recurso)
3. [Por que usar PUT](#por-que-usar-put)
4. [ID pela rota](#id-pela-rota)
5. [Novos dados pelo body](#novos-dados-pelo-body)
6. [Retorno 204 No Content](#retorno-204-no-content)
7. [Reutilizando a mesma classe de Request](#reutilizando-a-mesma-classe-de-request)
8. [Quando não reutilizar a mesma Request](#quando-não-reutilizar-a-mesma-request)
9. [Renomeando a Request no Visual Studio](#renomeando-a-request-no-visual-studio)
10. [ProducesResponseType](#producesresponsetype)
11. [Criando o UpdatePetUseCase](#criando-o-updatepetusecase)
12. [Fluxo entre Controller e Use Case](#fluxo-entre-controller-e-use-case)
13. [Estrutura final](#estrutura-final)
14. [Resumo](#resumo)
15. [Visão Geral](#visão-geral)

---

## Contexto da aula

A aula parte de um cenário em que um pet já foi cadastrado, mas alguma informação foi preenchida incorretamente.

Exemplos apresentados:

```text
Nome incorreto
Data de nascimento incorreta
Tipo do pet incorreto
```

Para corrigir essas informações, a API precisa disponibilizar um endpoint específico de atualização.

A aula utiliza um endpoint HTTP:

```http
PUT
```

---

## Atualização de um recurso

Para atualizar um pet, a aula define que duas informações são necessárias:

```text
1. ID do pet que será atualizado
2. Novas informações que substituirão as antigas
```

Fluxo conceitual:

```text
ID
↓
localiza o pet existente
↓
novos dados recebidos no body
↓
substituem os dados anteriores
```

---

## Por que usar PUT

O endpoint criado na aula utiliza:

```csharp
[HttpPut]
```

A ideia apresentada é atualizar as informações de um recurso já existente.

Exemplo estrutural:

```csharp
[HttpPut]
public IActionResult Update(...)
{
    // atualização do pet
}
```

---

## ID pela rota

Segundo a organização adotada na aula, o identificador do recurso deve ser recebido pela rota.

Exemplo:

```csharp
[HttpPut("{id}")]
public IActionResult Update([FromRoute] int id)
{
}
```

A aula reforça a convenção:

```text
IDs → rota
```

Assim, o endpoint identifica exatamente qual pet deverá ser atualizado.

Exemplo conceitual:

```http
PUT /api/pet/7
```

Nesse caso:

```text
id = 7
```

---

## Novos dados pelo body

Além do ID, o endpoint precisa receber as novas informações.

Esses dados são enviados no corpo da requisição:

```csharp
[FromBody] RequestPetJson request
```

Portanto, o endpoint passa a receber dados de duas fontes:

```text
Route
└── ID

Body
└── novas informações
```

Exemplo:

```csharp
[HttpPut("{id}")]
public IActionResult Update(
    [FromRoute] int id,
    [FromBody] RequestPetJson request)
{
}
```

---

## Retorno 204 No Content

Na implementação apresentada, o endpoint não precisa devolver um objeto após a atualização.

Por isso, a aula utiliza:

```csharp
return NoContent();
```

Esse retorno gera:

```text
204 No Content
```

Significado apresentado:

```text
A operação foi executada com sucesso
↓
mas não existe conteúdo para devolver na resposta
```

---

## Reutilizando a mesma classe de Request

Na aula, os dados utilizados para cadastrar e atualizar o pet são os mesmos.

A Request possui informações como:

```text
Name
Birthday
Type
```

Como todas essas propriedades podem ser usadas tanto no cadastro quanto na atualização, o instrutor decide não criar uma classe separada apenas para o PUT.

Em vez de manter algo específico como:

```text
RequestRegisterPetJson
```

ela é renomeada para algo mais genérico:

```text
RequestPetJson
```

Assim, a mesma classe pode ser utilizada por mais de um caso de uso.

```text
POST Register
      │
      └──────► RequestPetJson

PUT Update
      │
      └──────► RequestPetJson
```

---

## Quando não reutilizar a mesma Request

A aula deixa claro que reutilizar a mesma Request não é uma regra absoluta.

Isso depende do contexto.

O exemplo utilizado é o cadastro e atualização de usuários.

### Cadastro

Pode exigir:

```text
Nome
E-mail
Senha
```

### Atualização de perfil

Pode exigir apenas:

```text
Nome
E-mail
```

A senha pode ter um fluxo separado, com informações diferentes, como:

```text
Senha atual
Nova senha
Código adicional
```

Portanto:

```text
Se os dados necessários forem iguais
→ pode fazer sentido reutilizar a Request

Se os dados necessários forem diferentes
→ crie Requests diferentes
```

---

## Renomeando a Request no Visual Studio

A aula também demonstra o recurso de refatoração do Visual Studio.

Ao renomear o arquivo:

```text
RequestRegisterPetJson
```

para:

```text
RequestPetJson
```

O Visual Studio pergunta se também deve atualizar:

```text
Nome da classe
Referências existentes
```

Ao confirmar, as referências são modificadas automaticamente.

Isso evita alterações manuais em vários pontos do projeto.

---

## ProducesResponseType

Como o endpoint de atualização devolve apenas um status HTTP, a aula adiciona:

```csharp
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

Não existe `typeof(...)` nesse caso porque não existe corpo de resposta.

Comparação:

```csharp
// Com objeto de resposta
[ProducesResponseType(typeof(ResponsePetJson), StatusCodes.Status201Created)]

// Sem objeto de resposta
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

---

## Criando o UpdatePetUseCase

Seguindo o padrão apresentado nas aulas anteriores, a regra de negócio de atualização recebe uma classe própria.

Estrutura:

```text
Petfolio.Application
└── UseCases
    └── Pets
        ├── Register
        │   └── RegisterPetUseCase
        │
        └── Update
            └── UpdatePetUseCase
```

A classe precisa ser pública porque será usada pela API:

```csharp
public class UpdatePetUseCase
{
}
```

---

## Método Execute do Update Use Case

A aula mantém a convenção definida anteriormente:

```text
Cada Use Case possui apenas um método público principal
↓
Execute
```

Como a atualização não devolve conteúdo, o método utiliza:

```csharp
public void Execute(int id, RequestPetJson request)
{
}
```

Ele recebe exatamente os dados necessários para realizar a operação:

```text
id
request
```

Ou seja:

```text
ID do recurso
+
Novos dados
```

---

## Fluxo entre Controller e Use Case

No Controller, o Use Case é instanciado:

```csharp
var useCase = new UpdatePetUseCase();
```

Depois é executado:

```csharp
useCase.Execute(id, request);
```

E por fim o Controller devolve:

```csharp
return NoContent();
```

Fluxo completo:

```text
Cliente
  │
  │ PUT /api/pet/{id}
  │ Body: novos dados
  ▼
Controller
  │
  ├── recebe ID pela rota
  ├── recebe Request pelo body
  │
  ▼
UpdatePetUseCase.Execute(id, request)
  │
  ▼
Regra de negócio de atualização
  │
  ▼
Controller
  │
  ▼
204 No Content
```

---

## Teste no Swagger

Na aula, o endpoint é executado pelo Swagger.

Exemplo utilizado:

```text
ID = 7
Name = Pipoca
Type = 1
```

Durante o debug, é verificado que:

```text
ID chegou corretamente
Request chegou corretamente
Use Case recebeu os dois parâmetros
```

Depois da execução, o Swagger recebe:

```text
204 No Content
```

---

## Estrutura final

```text
Petfolio
│
├── Petfolio.API
│   └── Controllers
│       └── PetController
│           ├── POST Register
│           └── PUT Update
│
├── Petfolio.Application
│   └── UseCases
│       └── Pets
│           ├── Register
│           │   └── RegisterPetUseCase
│           │
│           └── Update
│               └── UpdatePetUseCase
│
└── Petfolio.Communication
    └── Requests
        └── RequestPetJson
```

---

## Resumo

| Conceito | Aplicação na aula |
|---|---|
| `PUT` | Atualização de um pet existente |
| ID | Recebido pela rota |
| `[FromRoute]` | Faz o binding do ID recebido na URL |
| Body | Contém as novas informações |
| `[FromBody]` | Desserializa o body para a Request |
| `RequestPetJson` | Request reutilizada entre cadastro e atualização |
| `204 No Content` | Sucesso sem conteúdo de resposta |
| `ProducesResponseType` | Documenta o status 204 |
| `UpdatePetUseCase` | Classe responsável pela regra de negócio de atualização |
| `Execute` | Método público principal do Use Case |
| Rename/Refactor | Atualiza classe e referências automaticamente no Visual Studio |

---

## Visão Geral

```text
                   PUT /api/pet/{id}
                           │
             ┌─────────────┴─────────────┐
             │                           │
             ▼                           ▼
       ID pela rota              Dados pelo body
      [FromRoute]               [FromBody]
             │                           │
             └─────────────┬─────────────┘
                           ▼
                    PetController
                           │
                           ▼
                 UpdatePetUseCase
                           │
                     Execute(id,
                       request)
                           │
                           ▼
                    Atualização
                           │
                           ▼
                   204 No Content
```

> **Em resumo:** a aula cria um endpoint `PUT` para atualizar um pet. O ID do recurso é recebido pela rota e os novos dados chegam pelo body. Como cadastro e atualização usam o mesmo conjunto de propriedades, a classe de Request é reutilizada e renomeada para um nome mais genérico. A regra de negócio de atualização fica isolada em `UpdatePetUseCase`, mantendo o padrão de um Use Case por responsabilidade e um método público `Execute`.