# Validação de Request: `String.IsNullOrWhiteSpace`, `DateTime.Compare` e `Enum.IsDefined`

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Objetivo das validações](#objetivo-das-validações)
3. [Organizando a validação em um método privado](#organizando-a-validação-em-um-método-privado)
4. [Validação do título](#validação-do-título)
5. [`IsNullOrEmpty` vs `IsNullOrWhiteSpace`](#isnullorempty-vs-isnullorwhitespace)
6. [Lançando `ArgumentException`](#lançando-argumentexception)
7. [Validação do valor da despesa](#validação-do-valor-da-despesa)
8. [Validação da data](#validação-da-data)
9. [Como funciona `DateTime.Compare`](#como-funciona-datetimecompare)
10. [Regra aplicada para a data da despesa](#regra-aplicada-para-a-data-da-despesa)
11. [Validação do `PaymentType`](#validação-do-paymenttype)
12. [Por que validar um enum](#por-que-validar-um-enum)
13. [`Enum.IsDefined`](#enumisdefined)
14. [Fluxo completo de validação](#fluxo-completo-de-validação)
15. [Resumo](#resumo)
16. [Visão Geral](#visão-geral)

---

## Contexto da aula

Depois de criar o endpoint de registro de despesas e o `RegisterExpenseUseCase`, a próxima etapa é validar os dados recebidos antes que futuramente sejam persistidos no banco de dados.

A ideia apresentada na aula é começar com validações simples usando `if`, deixando técnicas mais avançadas para aulas posteriores.

Fluxo atual:

```text
POST /api/expenses
        │
        ▼
RequestExpenseJson
        │
        ▼
RegisterExpenseUseCase
        │
        ▼
Validação dos dados
        │
        ├── válido   → continuar processamento
        │
        └── inválido → lançar exceção
```

---

## Objetivo das validações

O objetivo é impedir que informações inválidas cheguem à futura etapa de persistência.

A `RequestExpenseJson` possui os seguintes dados relevantes para validação:

```text
Title
Description
Date
Amount
PaymentType
```

Segundo a regra aplicada nesta aula:

| Campo | Regra |
|---|---|
| `Title` | Obrigatório |
| `Description` | Opcional |
| `Amount` | Deve ser maior que zero |
| `Date` | Não pode estar no futuro |
| `PaymentType` | Deve existir no enum `PaymentType` |

---

## Organizando a validação em um método privado

Em vez de colocar todas as validações diretamente dentro de `Execute`, a aula cria um método auxiliar privado:

```csharp
private void Validate(RequestExpenseJson request)
{
    // validações
}
```

O método principal continua mais limpo:

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    Validate(request);

    // restante da regra de negócio
}
```

A ideia é separar responsabilidades internas da própria classe sem criar vários fluxos públicos diferentes.

O `Execute` continua sendo a operação principal do Use Case, enquanto `Validate` funciona como método auxiliar.

---

## Validação do título

O título da despesa é obrigatório.

A aula começa verificando se ele está vazio, nulo ou contém apenas espaços em branco:

```csharp
var titleIsEmpty = string.IsNullOrWhiteSpace(request.Title);

if (titleIsEmpty)
{
    throw new ArgumentException("O título é obrigatório.");
}
```

Essa validação impede entradas como:

```text
null
""
"     "
```

---

## `IsNullOrEmpty` vs `IsNullOrWhiteSpace`

A aula chama atenção para a diferença entre dois métodos da classe `string`.

### `string.IsNullOrEmpty`

Retorna `true` quando o valor é:

```text
null
```

ou:

```text
""
```

Mas uma string composta apenas por espaços não é considerada vazia por esse método.

Exemplo:

```csharp
string.IsNullOrEmpty("   ");
```

Resultado:

```text
false
```

---

### `string.IsNullOrWhiteSpace`

Também cobre strings contendo apenas espaços em branco.

```csharp
string.IsNullOrWhiteSpace(null);    // true
string.IsNullOrWhiteSpace("");      // true
string.IsNullOrWhiteSpace("   ");   // true
string.IsNullOrWhiteSpace("Conta"); // false
```

Por isso, para o título da despesa, a aula opta por:

```csharp
string.IsNullOrWhiteSpace(request.Title)
```

---

## Lançando `ArgumentException`

Quando uma regra não é atendida, a aula utiliza:

```csharp
throw new ArgumentException("mensagem");
```

Exemplo:

```csharp
if (string.IsNullOrWhiteSpace(request.Title))
{
    throw new ArgumentException("O título é obrigatório.");
}
```

A exceção interrompe o fluxo normal da execução naquele ponto.

Nesta etapa do curso, `ArgumentException` é utilizada diretamente. O tratamento mais elaborado das exceções será evoluído posteriormente.

---

## Validação do valor da despesa

Uma despesa não deve possuir valor igual ou menor que zero.

Regra:

```csharp
if (request.Amount <= 0)
{
    throw new ArgumentException("O valor deve ser maior que zero.");
}
```

São considerados inválidos:

```text
0
-1
-50.75
```

Exemplos válidos:

```text
10
29.90
1359
```

---

## Validação da data

A aula apresenta duas possibilidades para comparar datas:

```csharp
request.Date > DateTime.UtcNow
```

ou utilizar:

```csharp
DateTime.Compare(...)
```

O formato escolhido na aula é `DateTime.Compare`.

Exemplo:

```csharp
var dateComparison = DateTime.Compare(request.Date, DateTime.UtcNow);
```

---

## Como funciona `DateTime.Compare`

A assinatura recebe duas datas:

```csharp
DateTime.Compare(t1, t2)
```

O método retorna um `int`.

O significado do resultado é:

| Resultado | Significado |
|---:|---|
| `< 0` | `t1` ocorre antes de `t2` |
| `0` | `t1` e `t2` são iguais |
| `> 0` | `t1` ocorre depois de `t2` |

Exemplo conceitual:

```text
t1 = 31/12/2023
t2 = 01/01/2024

resultado < 0
```

Outro exemplo:

```text
t1 = 02/01/2024
t2 = 01/01/2024

resultado > 0
```

---

## Regra aplicada para a data da despesa

A regra adotada pelo instrutor nesta aula é:

> Uma despesa não pode ser cadastrada com uma data futura.

Portanto, são aceitas despesas cuja data seja de hoje ou esteja no passado.

A comparação fica conceitualmente assim:

```csharp
var dateComparison = DateTime.Compare(request.Date, DateTime.UtcNow);

if (dateComparison > 0)
{
    throw new ArgumentException("A data da despesa não pode estar no futuro.");
}
```

O raciocínio é:

```text
request.Date > DateTime.UtcNow
        │
        └── data futura
                │
                ▼
        lançar exceção
```

---

## Validação do `PaymentType`

Na aula anterior foi criado um enum com opções de pagamento semelhantes a:

```csharp
public enum PaymentType
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    ElectronicTransfer = 3
}
```

Intuitivamente poderia parecer que uma propriedade desse tipo aceitaria apenas `0`, `1`, `2` ou `3`.

A aula demonstra, porém, que é possível receber numericamente um valor como:

```text
7
```

mesmo que esse valor não corresponda a nenhuma opção declarada no enum.

Por isso, é necessário validar explicitamente se o valor recebido está definido.

---

## Por que validar um enum

Exemplo de JSON enviado pelo Swagger:

```json
{
  "title": "Apple",
  "description": "Novo iPhone",
  "date": "2024-01-01T10:00:00Z",
  "amount": 1359,
  "paymentType": 7
}
```

Mesmo que `7` não esteja declarado em `PaymentType`, o valor pode chegar até a propriedade.

Logo, apenas tipar a propriedade como enum não garante, por si só, que o valor recebido corresponda a uma opção definida.

---

## `Enum.IsDefined`

Para verificar se o valor pertence ao enum, a aula utiliza:

```csharp
Enum.IsDefined(...)
```

Exemplo:

```csharp
var paymentTypeIsValid = Enum.IsDefined(
    typeof(PaymentType),
    request.PaymentType);
```

O primeiro argumento indica qual enum será analisado:

```csharp
typeof(PaymentType)
```

O segundo representa o valor que será verificado:

```csharp
request.PaymentType
```

Depois:

```csharp
if (!paymentTypeIsValid)
{
    throw new ArgumentException("PaymentType inválido.");
}
```

É importante usar a negação:

```csharp
!
```

porque a exceção deve ser lançada quando o valor **não** estiver definido no enum.

---

## Validação da descrição

A descrição foi definida anteriormente como opcional.

Logo, nesta aula não é criada uma validação obrigatória para ela.

```text
Description
└── opcional
```

Ela pode ser enviada ou não sem impedir o registro.

---

## Fluxo completo de validação

A implementação conceitual da aula pode ser representada assim:

```csharp
private void Validate(RequestExpenseJson request)
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        throw new ArgumentException("O título é obrigatório.");
    }

    if (request.Amount <= 0)
    {
        throw new ArgumentException("O valor deve ser maior que zero.");
    }

    var dateComparison = DateTime.Compare(request.Date, DateTime.UtcNow);

    if (dateComparison > 0)
    {
        throw new ArgumentException("A data não pode estar no futuro.");
    }

    var paymentTypeIsValid = Enum.IsDefined(
        typeof(PaymentType),
        request.PaymentType);

    if (!paymentTypeIsValid)
    {
        throw new ArgumentException("Tipo de pagamento inválido.");
    }
}
```

E o Use Case:

```csharp
public ResponseRegisterExpenseJson Execute(RequestExpenseJson request)
{
    Validate(request);

    // futuramente:
    // criar entidade
    // persistir no banco
    // gerar resposta

    return new ResponseRegisterExpenseJson();
}
```

---

## Fluxo da requisição

```text
RequestExpenseJson
        │
        ▼
RegisterExpenseUseCase.Execute()
        │
        ▼
Validate(request)
        │
        ├── Title vazio? ──────────────► ArgumentException
        │
        ├── Amount <= 0? ─────────────► ArgumentException
        │
        ├── Date no futuro? ──────────► ArgumentException
        │
        ├── PaymentType inválido? ────► ArgumentException
        │
        └── tudo válido
                │
                ▼
        continuar regra de negócio
```

---

## Resumo

| Validação | Técnica usada |
|---|---|
| Título obrigatório | `string.IsNullOrWhiteSpace` |
| Valor positivo | `request.Amount <= 0` |
| Data não futura | `DateTime.Compare` |
| Forma de pagamento válida | `Enum.IsDefined` |
| Dado inválido | `throw new ArgumentException(...)` |
| Descrição | Não validada por ser opcional |

### Métodos importantes

```csharp
string.IsNullOrWhiteSpace(...)
DateTime.Compare(...)
Enum.IsDefined(...)
typeof(...)
```

### Operador importante

```csharp
!
```

Utilizado para inverter o booleano retornado por `Enum.IsDefined`:

```csharp
if (!paymentTypeIsValid)
```

---

## Visão Geral

```text
                 REGISTER EXPENSE
                       │
                       ▼
              RequestExpenseJson
                       │
                       ▼
             RegisterExpenseUseCase
                       │
                       ▼
                  Validate()
                       │
       ┌───────────────┼────────────────┐
       │               │                │
       ▼               ▼                ▼
     Title           Amount            Date
IsNullOrWhiteSpace   > 0       DateTime.Compare
       │               │                │
       └──────────┬────┴────────┬───────┘
                  │             │
                  ▼             ▼
             PaymentType    Description
             Enum.IsDefined    opcional
                  │
                  ▼
            tudo válido?
             │         │
            não       sim
             │         │
             ▼         ▼
      ArgumentException
                       continuar
                       fluxo
```

> **Em resumo:** a aula introduz a validação da `RequestExpenseJson` diretamente no `RegisterExpenseUseCase`. Um método privado `Validate` concentra verificações de título, valor, data e tipo de pagamento. O título usa `String.IsNullOrWhiteSpace`, o valor precisa ser maior que zero, a data não pode estar no futuro segundo a regra adotada na aula e o valor de `PaymentType` é conferido com `Enum.IsDefined`. Quando algum dado é inválido, o fluxo lança uma `ArgumentException`.
