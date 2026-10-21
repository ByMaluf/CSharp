# FluentValidation, RuleFor, sintaxe fluente e ValidationResult

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Por que substituir parte dos ifs](#por-que-substituir-parte-dos-ifs)
3. [FluentValidation como padrão de validação](#fluentvalidation-como-padrão-de-validação)
4. [O que é sintaxe fluente](#o-que-é-sintaxe-fluente)
5. [Criando o RegisterExpenseValidator](#criando-o-registerexpensevalidator)
6. [Herdando de AbstractValidator](#herdando-de-abstractvalidator)
7. [Criando regras com RuleFor](#criando-regras-com-rulefor)
8. [Validando Title](#validando-title)
9. [Validando Amount](#validando-amount)
10. [Validando Date](#validando-date)
11. [Validando PaymentType](#validando-paymenttype)
12. [Executando o validator](#executando-o-validator)
13. [ValidationResult e lista de erros](#validationresult-e-lista-de-erros)
14. [FluentValidation e ifs podem coexistir](#fluentvalidation-e-ifs-podem-coexistir)
15. [Resumo](#resumo)
16. [Visão Geral](#visão-geral)

---

## Contexto da aula

Na aula anterior, o pacote `FluentValidation` foi instalado no projeto `CashFlow.Application`.

Agora a validação manual criada anteriormente com vários `if` será refatorada para utilizar essa biblioteca.

A ideia é manter as regras de validação mais organizadas, legíveis e separadas do fluxo principal do Use Case.

A partir desta etapa, a aula passa a utilizar o FluentValidation como abordagem principal para validar classes de Request.

---

## Por que substituir parte dos ifs

As validações anteriores funcionavam, mas apresentavam dois problemas principais.

### 1. O primeiro `throw` interrompia a validação

Exemplo conceitual:

```csharp
if (string.IsNullOrWhiteSpace(request.Title))
{
    throw new ArgumentException("O título é obrigatório.");
}

if (request.Amount <= 0)
{
    throw new ArgumentException("O valor deve ser maior que zero.");
}
```

Se `Title` estiver inválido, a primeira exceção é lançada e a execução termina naquele ponto.

Assim, as validações seguintes não são executadas.

O comportamento desejado passa a ser:

```text
validar Title
validar Amount
validar Date
validar PaymentType
        ↓
coletar todos os erros
        ↓
devolver a lista completa
```

### 2. Muitos ifs fazem a classe crescer

Mesmo quando os `if` são tecnicamente corretos, várias validações simples dentro do Use Case aumentam bastante o tamanho da classe.

A proposta do FluentValidation é retirar essa responsabilidade do fluxo principal e concentrar as regras em uma classe específica de validação.

---

## FluentValidation como padrão de validação

A aula deixa claro que usar `if` para validar dados não é errado.

A estratégia apresentada é:

```text
validações simples de Request
        ↓
FluentValidation

validações mais complexas ou específicas
        ↓
ifs quando necessário
```

Ou seja, as duas abordagens podem coexistir.

O objetivo não é eliminar completamente os `if`, mas utilizar uma ferramenta mais apropriada quando as regras puderem ser expressas de forma simples e declarativa.

---

## O que é sintaxe fluente

O nome FluentValidation está relacionado ao conceito de **Fluent API**, ou sintaxe fluente.

Nesse estilo, métodos são chamados em sequência:

```csharp
objeto
    .Metodo1()
    .Metodo2()
    .Metodo3();
```

A ideia é produzir um código que possa ser lido quase como uma frase.

Exemplo:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage("O título é obrigatório.");
```

Pode ser interpretado como:

```text
Para Title
↓
não permita vazio
↓
caso seja inválido
↓
use esta mensagem
```

---

## Criando o RegisterExpenseValidator

A aula cria uma nova classe dentro da estrutura relacionada ao registro de despesas:

```text
Application
└── UseCases
    └── Expenses
        └── Register
            ├── RegisterExpenseUseCase
            └── RegisterExpenseValidator
```

Classe:

```csharp
public class RegisterExpenseValidator
{
}
```

Ela será responsável apenas pelas regras de validação da Request usada no cadastro de despesas.

---

## Herdando de AbstractValidator

Para transformar a classe em um validator do FluentValidation, ela herda de:

```csharp
AbstractValidator<T>
```

O tipo genérico `T` representa a classe que será validada.

No caso da aula:

```csharp
public class RegisterExpenseValidator
    : AbstractValidator<RequestRegisterExpenseJson>
{
}
```

Representação:

```text
RegisterExpenseValidator
        │
        └── herda de
             AbstractValidator<RequestRegisterExpenseJson>
```

Isso informa ao FluentValidation que esse validator conhece as regras de validação para:

```csharp
RequestRegisterExpenseJson
```

---

## Criando regras com RuleFor

As regras são configuradas no construtor do validator.

Estrutura:

```csharp
public class RegisterExpenseValidator
    : AbstractValidator<RequestRegisterExpenseJson>
{
    public RegisterExpenseValidator()
    {
        // regras
    }
}
```

O principal método utilizado é:

```csharp
RuleFor(...)
```

Ele recebe uma expressão indicando qual propriedade será validada.

Exemplo:

```csharp
RuleFor(expense => expense.Title)
```

A expressão:

```csharp
expense => expense.Title
```

é uma **expressão lambda**.

Na explicação da aula, ela pode ser entendida como uma função sem nome que recebe a instância da Request e seleciona a propriedade a ser validada.

```text
expense
   ↓
RequestRegisterExpenseJson
   ↓
expense.Title
```

---

## Validando Title

Antes, a validação era feita manualmente com algo semelhante a:

```csharp
string.IsNullOrWhiteSpace(request.Title)
```

Com FluentValidation:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage("O título é obrigatório.");
```

### NotEmpty

`NotEmpty()` rejeita um valor considerado vazio para a propriedade.

No contexto da string mostrado na aula, a intenção é impedir:

```text
null
""
"   "
```

### WithMessage

Permite definir a mensagem de erro da regra:

```csharp
.WithMessage("O título é obrigatório.")
```

A regra inteira fica declarativa:

```text
Title
↓
NotEmpty
↓
"O título é obrigatório."
```

---

## Validando Amount

A regra anterior verificava se o valor era menor ou igual a zero.

Com FluentValidation:

```csharp
RuleFor(expense => expense.Amount)
    .GreaterThan(0)
    .WithMessage("O valor deve ser maior que zero.");
```

### GreaterThan

```csharp
.GreaterThan(0)
```

significa:

```text
Amount > 0
```

O FluentValidation também disponibiliza outras operações de comparação, como:

```text
GreaterThan
GreaterThanOrEqualTo
LessThan
LessThanOrEqualTo
```

---

## Validando Date

A regra de negócio apresentada anteriormente determina que a despesa não pode possuir uma data futura.

Com FluentValidation:

```csharp
RuleFor(expense => expense.Date)
    .LessThanOrEqualTo(DateTime.UtcNow)
    .WithMessage("A despesa não pode ser para o futuro.");
```

Representação:

```text
expense.Date <= DateTime.UtcNow
```

Caso a data esteja no futuro, a regra falha e a mensagem configurada é adicionada ao resultado da validação.

---

## Validando PaymentType

`PaymentType` é um enum.

Anteriormente, a aula utilizava:

```csharp
Enum.IsDefined(...)
```

Com FluentValidation, a mesma intenção pode ser expressa com:

```csharp
RuleFor(expense => expense.PaymentType)
    .IsInEnum()
    .WithMessage("O tipo de pagamento não é válido.");
```

### IsInEnum

```csharp
.IsInEnum()
```

verifica se o valor recebido corresponde a um dos valores definidos pelo enum.

Exemplo conceitual:

```text
0 → válido
1 → válido
2 → válido
3 → válido
700 → inválido
```

Isso substitui a validação manual com `Enum.IsDefined` usada anteriormente.

---

## Validator completo

Conceitualmente, o validator criado na aula fica semelhante a:

```csharp
public class RegisterExpenseValidator
    : AbstractValidator<RequestRegisterExpenseJson>
{
    public RegisterExpenseValidator()
    {
        RuleFor(expense => expense.Title)
            .NotEmpty()
            .WithMessage("O título é obrigatório.");

        RuleFor(expense => expense.Amount)
            .GreaterThan(0)
            .WithMessage("O valor deve ser maior que zero.");

        RuleFor(expense => expense.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("A despesa não pode ser para o futuro.");

        RuleFor(expense => expense.PaymentType)
            .IsInEnum()
            .WithMessage("O tipo de pagamento não é válido.");
    }
}
```

A propriedade `Description` continua sem regra obrigatória porque foi definida anteriormente como opcional.

---

## Executando o validator

Depois de criar a classe de validação, o Use Case não precisa mais conter todos aqueles `if`.

A aula passa a criar uma instância do validator:

```csharp
var validator = new RegisterExpenseValidator();
```

Depois executa:

```csharp
var result = validator.Validate(request);
```

Fluxo:

```text
RequestRegisterExpenseJson
        ↓
RegisterExpenseValidator
        ↓
Validate(request)
        ↓
ValidationResult
```

A função `Validate` do Use Case, que antes continha várias regras manualmente, fica muito menor.

---

## ValidationResult e lista de erros

Uma mudança importante é que o FluentValidation não precisa interromper a execução ao encontrar o primeiro problema.

Ele executa as regras e retorna um:

```csharp
ValidationResult
```

No teste realizado na aula, foram enviados vários valores inválidos ao mesmo tempo:

```text
Title       → vazio
Amount      → negativo
Date        → futura
PaymentType → valor inexistente no enum
```

O resultado trouxe os quatro erros simultaneamente.

Conceitualmente:

```text
ValidationResult
└── Errors
    ├── "O título é obrigatório."
    ├── "O valor deve ser maior que zero."
    ├── "A despesa não pode ser para o futuro."
    └── "O tipo de pagamento não é válido."
```

Essa é uma diferença importante em relação à implementação anterior com `throw` dentro de cada `if`.

### Antes

```text
primeiro erro
↓
throw
↓
execução interrompida
↓
1 erro retornado
```

### Agora

```text
regra 1
regra 2
regra 3
regra 4
↓
ValidationResult
↓
vários erros disponíveis
```

---

## FluentValidation e ifs podem coexistir

A aula reforça que FluentValidation não elimina a necessidade de `if`.

Uma estratégia possível é:

```text
Request
  │
  ▼
FluentValidation
  │
  ├── campos obrigatórios
  ├── ranges
  ├── datas
  ├── enum
  └── regras simples
  │
  ▼
Use Case
  │
  └── validações adicionais mais complexas
         usando if quando necessário
```

O ponto principal é manter a capacidade de devolver uma lista completa de erros, mesmo quando existirem regras de tipos diferentes.

---

## Comparação: validação manual x FluentValidation

| Aspecto | Validação manual | FluentValidation |
|---|---|---|
| Estrutura | vários `if` | regras declarativas |
| Mensagem | criada no `throw` | `WithMessage(...)` |
| String obrigatória | `IsNullOrWhiteSpace` | `NotEmpty()` |
| Número positivo | comparação manual | `GreaterThan(0)` |
| Data máxima | `DateTime.Compare` ou operadores | `LessThanOrEqualTo(...)` |
| Enum | `Enum.IsDefined` | `IsInEnum()` |
| Primeiro erro | `throw` interrompe | regras podem gerar vários erros |
| Resultado | exceção | `ValidationResult` |
| Organização | dentro do Use Case | classe Validator específica |

---

## Resumo

Nesta aula, a validação do registro de despesas é refatorada para utilizar FluentValidation.

A classe criada é:

```text
RegisterExpenseValidator
```

que herda de:

```csharp
AbstractValidator<RequestRegisterExpenseJson>
```

As principais regras ficam assim:

```csharp
RuleFor(x => x.Title)
    .NotEmpty();

RuleFor(x => x.Amount)
    .GreaterThan(0);

RuleFor(x => x.Date)
    .LessThanOrEqualTo(DateTime.UtcNow);

RuleFor(x => x.PaymentType)
    .IsInEnum();
```

Cada regra recebe sua mensagem através de:

```csharp
.WithMessage(...)
```

E a validação passa a ser executada com:

```csharp
var validator = new RegisterExpenseValidator();
var result = validator.Validate(request);
```

O resultado é um `ValidationResult`, capaz de armazenar várias falhas de validação ao mesmo tempo.

---

## Visão Geral

```text
RequestRegisterExpenseJson
          │
          ▼
RegisterExpenseValidator
          │
          ├── RuleFor(Title)
          │      └── NotEmpty()
          │
          ├── RuleFor(Amount)
          │      └── GreaterThan(0)
          │
          ├── RuleFor(Date)
          │      └── LessThanOrEqualTo(DateTime.UtcNow)
          │
          └── RuleFor(PaymentType)
                 └── IsInEnum()
          │
          ▼
     Validate(request)
          │
          ▼
    ValidationResult
          │
          └── Errors
               ├── erro 1
               ├── erro 2
               ├── erro 3
               └── erro 4
```

> **Em resumo:** o FluentValidation permite retirar validações simples do Use Case e expressá-las em uma classe específica usando uma sintaxe fluente. As regras ficam menores e mais legíveis, e a execução da validação gera um `ValidationResult` capaz de reunir múltiplos erros em uma única operação. A próxima etapa é transformar essas falhas do `ValidationResult` nas mensagens que serão lançadas e devolvidas pela API.