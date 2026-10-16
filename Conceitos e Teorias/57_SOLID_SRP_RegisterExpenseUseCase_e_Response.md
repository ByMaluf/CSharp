# SOLID, SRP, RegisterExpenseUseCase e Response

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [O que é SOLID](#o-que-é-solid)
3. [Princípio da Responsabilidade Única](#princípio-da-responsabilidade-única)
4. [Problema de uma classe que faz tudo](#problema-de-uma-classe-que-faz-tudo)
5. [Separando responsabilidades em Use Cases](#separando-responsabilidades-em-use-cases)
6. [Organização da camada Application](#organização-da-camada-application)
7. [Criando o RegisterExpenseUseCase](#criando-o-registerexpenseusecase)
8. [Método Execute](#método-execute)
9. [Criando a classe de Response](#criando-a-classe-de-response)
10. [Fluxo Controller → Use Case → Response](#fluxo-controller--use-case--response)
11. [Validação e persistência](#validação-e-persistência)
12. [Injeção de dependência será aplicada depois](#injeção-de-dependência-será-aplicada-depois)
13. [Resumo](#resumo)
14. [Visão Geral](#visão-geral)

---

## Contexto da aula

Depois de criar o endpoint `POST` e a `RequestExpenseJson`, a aula começa a implementar a regra de negócio responsável por registrar uma despesa.

O fluxo pretendido é:

```text
Endpoint
↓
RequestExpenseJson
↓
Regra de negócio
↓
Validações
↓
Persistência futura no banco de dados
↓
Response
```

Nesta etapa, a persistência ainda não é implementada, mas a estrutura é preparada para receber essa responsabilidade posteriormente.

---

## O que é SOLID

A aula introduz o termo **SOLID** como um acrônimo formado por cinco princípios relacionados à programação orientada a objetos.

A ideia do curso é apresentar esses princípios gradualmente, conforme novas partes da aplicação forem sendo implementadas.

```text
S O L I D
│
└── cada letra representa um princípio
```

Nesta aula, o foco está no primeiro princípio:

```text
S → Single Responsibility Principle
```

Em português:

```text
Princípio da Responsabilidade Única
```

---

## Princípio da Responsabilidade Única

A definição apresentada na aula é:

> Uma classe deve ter um e apenas um motivo para mudar.

A ideia é que cada classe tenha uma responsabilidade bem definida.

Se uma classe é responsável por registrar uma despesa, alterações relacionadas ao cadastro de despesas devem afetar essa classe.

Se outra classe é responsável por excluir uma despesa, mudanças relacionadas à exclusão devem afetar outra classe.

Assim:

```text
RegisterExpenseUseCase
→ motivo para mudar: regra de registro

DeleteExpenseUseCase
→ motivo para mudar: regra de exclusão

UpdateExpenseUseCase
→ motivo para mudar: regra de atualização
```

---

## Problema de uma classe que faz tudo

A aula mostra como exemplo ruim uma classe semelhante a:

```text
ExpenseService
```

contendo várias operações:

```text
RegisterExpense()
DeleteExpense()
UpdateExpense()
GetExpenseById()
...
```

Nesse cenário, a mesma classe pode precisar ser modificada por razões completamente diferentes.

Exemplo:

```text
problema no DELETE
↓
modifica ExpenseService

problema no UPDATE
↓
modifica ExpenseService

problema no REGISTER
↓
modifica ExpenseService
```

Isso significa que a classe possui vários motivos para mudar, contrariando o SRP apresentado na aula.

### Problemas citados

Classes muito grandes podem causar:

- dificuldade de leitura;
- dificuldade de manutenção;
- medo de alterar uma parte e quebrar outra;
- aumento da complexidade;
- conflitos de merge em trabalho em equipe;
- concentração de várias regras diferentes no mesmo arquivo.

A aula cita inclusive casos de classes com milhares de linhas como exemplo do tipo de estrutura que deve ser evitada.

---

## Separando responsabilidades em Use Cases

A solução adotada é criar uma classe específica para cada regra de negócio.

Exemplo:

```text
RegisterExpenseUseCase
DeleteExpenseUseCase
UpdateExpenseUseCase
GetExpenseByIdUseCase
```

Cada uma representa um **caso de uso** da aplicação.

```text
Use Case
=
uma ação/regra de negócio específica da aplicação
```

No caso desta aula:

```text
RegisterExpenseUseCase
```

possui somente a responsabilidade de registrar uma despesa.

---

## Organização da camada Application

As regras de negócio ficam no projeto:

```text
CashFlow.Application
```

A aula organiza os arquivos utilizando a seguinte estrutura:

```text
CashFlow.Application
└── UseCases
    └── Expenses
        └── Register
            └── RegisterExpenseUseCase.cs
```

Cada parte do caminho ajuda a identificar o contexto da classe:

```text
UseCases
↓
Expenses
↓
Register
↓
RegisterExpenseUseCase
```

Isso permite entender a responsabilidade da classe apenas observando sua localização e seu nome.

---

## Criando o RegisterExpenseUseCase

A classe criada é:

```csharp
public class RegisterExpenseUseCase
{
}
```

Ela precisa ser `public` porque será utilizada pelo projeto da API.

A escolha do nome é intencionalmente descritiva:

```text
Register
+
Expense
+
UseCase
```

Ou seja:

```text
caso de uso responsável por registrar uma despesa
```

---

## Método Execute

Dentro do Use Case, a aula utiliza o padrão de um método público chamado:

```csharp
Execute(...)
```

A ideia é que o código consumidor diga conceitualmente:

```text
execute esta regra de negócio
```

O método recebe a request criada anteriormente:

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    // validações
    // persistência futura
    // criação da response
}
```

### Por que receber a Request?

O Controller recebe os dados HTTP e repassa essas informações para a regra de negócio.

```text
Controller
↓
RequestExpenseJson
↓
RegisterExpenseUseCase.Execute(request)
```

### Métodos auxiliares

A aula também deixa claro que o Use Case pode possuir funções privadas auxiliares.

Exemplo conceitual:

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    Validate(request);

    // restante do fluxo
}

private void Validate(RequestExpenseJson request)
{
    // validações
}
```

Isso não viola a responsabilidade da classe porque todas essas funções continuam relacionadas ao mesmo caso de uso.

O que deve ser evitado é colocar operações diferentes, como registrar, excluir e atualizar, dentro da mesma classe.

---

## Criando a classe de Response

No projeto de comunicação, a aula cria uma nova pasta:

```text
Responses
```

Estrutura:

```text
CashFlow.Communication
├── Requests
├── Responses
└── Enums
```

A classe criada para o cadastro é:

```text
ResponseRegisterExpenseJson
```

Nesta aula, a Response contém apenas o título da despesa.

Exemplo:

```csharp
public class ResponseRegisterExpenseJson
{
    public string Title { get; set; } = string.Empty;
}
```

### E o ID?

A aula comenta que normalmente é comum retornar também o identificador do recurso criado.

Porém, o ID ainda não é adicionado porque haverá uma aula específica explicando diferentes tipos de identificadores.

Portanto, nesta etapa:

```text
ResponseRegisterExpenseJson
└── Title
```

E futuramente a classe será revisitada para incluir o ID.

---

## Fluxo Controller → Use Case → Response

O Controller passa a utilizar o Use Case.

Fluxo conceitual:

```text
POST /expenses
↓
ExpensesController
↓
RequestExpenseJson
↓
new RegisterExpenseUseCase()
↓
Execute(request)
↓
ResponseRegisterExpenseJson
↓
Created(...)
```

Exemplo equivalente ao apresentado:

```csharp
var useCase = new RegisterExpenseUseCase();

var response = useCase.Execute(request);

return Created(string.Empty, response);
```

O objeto retornado pelo Use Case é colocado no corpo da resposta HTTP.

---

## Created e corpo da resposta

O endpoint utiliza:

```csharp
Created(string.Empty, response)
```

Segundo a abordagem mostrada na aula:

```text
primeiro parâmetro
→ URI solicitada pelo método

segundo parâmetro
→ objeto colocado no body da resposta
```

Neste momento, a URI é preenchida com:

```csharp
string.Empty
```

E a Response criada pelo Use Case é devolvida no corpo.

---

## Validação e persistência

O `Execute` ainda será expandido.

A aula indica o fluxo futuro:

```text
Execute(request)
│
├── validar informações
│
├── criar/preparar entidade
│
├── persistir no banco de dados
│
└── criar Response
```

Nesta etapa, a aula apenas prepara essa estrutura.

Representação:

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    // validações

    // persistência no banco futuramente

    return new ResponseRegisterExpenseJson();
}
```

---

## Injeção de dependência será aplicada depois

Neste momento, o Controller instancia diretamente o Use Case:

```csharp
var useCase = new RegisterExpenseUseCase();
```

A aula avisa que isso será alterado mais adiante.

O objetivo futuro é utilizar:

```text
Injeção de Dependência
```

O curso também utilizará essa mudança para apresentar outro princípio do SOLID.

Portanto, nesta aula a instanciação direta é temporária.

```text
Agora:
Controller → new RegisterExpenseUseCase()

Depois:
Controller → Use Case injetado como dependência
```

---

## Resumo

### SOLID

```text
SOLID
└── conjunto de cinco princípios de orientação a objetos
```

Nesta aula:

```text
S = Single Responsibility Principle
```

Definição apresentada:

```text
Uma classe deve ter apenas um motivo para mudar.
```

### Estrutura criada

```text
CashFlow.Application
└── UseCases
    └── Expenses
        └── Register
            └── RegisterExpenseUseCase.cs
```

```text
CashFlow.Communication
├── Requests
│   └── RequestExpenseJson.cs
│
└── Responses
    └── ResponseRegisterExpenseJson.cs
```

### Responsabilidades

| Elemento | Responsabilidade |
|---|---|
| `ExpensesController` | Receber a requisição HTTP e devolver a resposta |
| `RequestExpenseJson` | Representar os dados recebidos |
| `RegisterExpenseUseCase` | Executar a regra de negócio de cadastro |
| `ResponseRegisterExpenseJson` | Representar os dados devolvidos |
| `Execute` | Ponto de entrada da regra de negócio |

### Fluxo

```text
HTTP POST
↓
Controller
↓
RequestExpenseJson
↓
RegisterExpenseUseCase.Execute()
↓
validações
↓
persistência futura
↓
ResponseRegisterExpenseJson
↓
201 Created
```

---

## Visão Geral

```text
                         POST /expenses
                               │
                               ▼
                      ExpensesController
                               │
                               │ RequestExpenseJson
                               ▼
                   RegisterExpenseUseCase
                               │
                         Execute(request)
                               │
                 ┌─────────────┴─────────────┐
                 │                           │
                 ▼                           ▼
             validações              persistência futura
                 │                           │
                 └─────────────┬─────────────┘
                               ▼
                ResponseRegisterExpenseJson
                               │
                               ▼
                         201 Created
```

> **Em resumo:** a aula começa a transformar o endpoint de cadastro em um fluxo com responsabilidades separadas. O Controller não concentra a regra de negócio; ele repassa a `RequestExpenseJson` para o `RegisterExpenseUseCase`. O Use Case representa apenas o cadastro de despesas, seguindo o princípio da responsabilidade única do SOLID, e devolve uma `ResponseRegisterExpenseJson`. Validações, persistência real e injeção de dependência serão aprofundadas posteriormente.
