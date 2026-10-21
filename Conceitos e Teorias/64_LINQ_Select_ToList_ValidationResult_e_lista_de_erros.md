# LINQ, Select, ToList, ValidationResult e Lista de Erros

## 📋 Índice

1. [Visão geral](#visão-geral)
2. [Por que LINQ é necessário aqui](#por-que-linq-é-necessário-aqui)
3. [O que é LINQ](#o-que-é-linq)
4. [O problema com o ValidationResult](#o-problema-com-o-validationresult)
5. [Usando Select para projetar os erros](#usando-select-para-projetar-os-erros)
6. [Expressões lambda no Select](#expressões-lambda-no-select)
7. [Convertendo para List<string> com ToList](#convertendo-para-liststring-com-tolist)
8. [Verificando result.IsValid](#verificando-resultisvalid)
9. [Novo problema: exceções não recebem List<string>](#novo-problema-exceções-não-recebem-liststring)
10. [Response de erro também precisa evoluir](#response-de-erro-também-precisa-evoluir)
11. [Fluxo completo desta etapa](#fluxo-completo-desta-etapa)
12. [Resumo](#resumo)
13. [Visão Geral](#visão-geral-1)

---

## Visão geral

Na aula anterior, o `RegisterExpenseValidator` passou a utilizar **FluentValidation** para validar toda a requisição de cadastro de despesas.

O FluentValidation devolve um objeto `ValidationResult` contendo os erros encontrados. O problema é que esses erros não são diretamente uma `List<string>`: eles são objetos do próprio FluentValidation.

Nesta aula, entra o **LINQ**, utilizado para transformar a coleção de erros do FluentValidation em uma lista contendo apenas as mensagens de erro.

A ideia é sair de algo conceitualmente assim:

```text
ValidationResult
└── Errors
    ├── ValidationFailure
    ├── ValidationFailure
    ├── ValidationFailure
    └── ValidationFailure
```

para:

```text
List<string>
├── "O título é obrigatório."
├── "O valor deve ser maior que zero."
├── "A despesa não pode ser para o futuro."
└── "O tipo de pagamento não é válido."
```

---

## Por que LINQ é necessário aqui

Depois da validação:

```csharp
var validator = new RegisterExpenseValidator();
var result = validator.Validate(request);
```

os erros ficam disponíveis em:

```csharp
result.Errors
```

Porém, `result.Errors` não é uma lista de strings.

Cada item é um objeto do tipo `ValidationFailure`, pertencente ao FluentValidation.

O que realmente interessa para a API é apenas a propriedade que contém a mensagem de erro:

```csharp
ErrorMessage
```

Portanto, é necessário transformar:

```text
List<ValidationFailure>
```

em:

```text
List<string>
```

É nesse ponto que o LINQ é utilizado.

---

## O que é LINQ

**LINQ** significa:

```text
Language Integrated Query
```

Ele fornece recursos de consulta diretamente na linguagem C#.

Com LINQ, podemos trabalhar com coleções utilizando operações como:

```text
Where
Select
GroupBy
Min
Max
OrderBy
Any
First
FirstOrDefault
```

Exemplo conceitual:

```csharp
var values = numbers
    .Where(number => number > 10)
    .Select(number => number * 2)
    .ToList();
```

LINQ é utilizado com muita frequência para consultar, filtrar, transformar e organizar coleções.

Nesta aula, o operador importante é o `Select`.

---

## O problema com o ValidationResult

O resultado do FluentValidation possui uma coleção semelhante a:

```csharp
result.Errors
```

Essa coleção contém objetos `ValidationFailure`.

Cada erro possui diversas informações, mas neste momento o que interessa é:

```csharp
failure.ErrorMessage
```

Ou seja:

```text
ValidationFailure
├── PropertyName
├── ErrorMessage   ← interessa aqui
├── AttemptedValue
├── ErrorCode
└── outras informações
```

O objetivo é percorrer todos os erros e selecionar somente `ErrorMessage`.

---

## Usando Select para projetar os erros

O LINQ permite fazer essa transformação com `Select`:

```csharp
result.Errors.Select(failure => failure.ErrorMessage)
```

O `Select` executa a expressão informada para cada elemento da coleção.

Neste caso:

```csharp
failure => failure.ErrorMessage
```

significa:

```text
para cada ValidationFailure
        ↓
pegue somente ErrorMessage
```

Exemplo:

```text
Entrada

ValidationFailure { ErrorMessage = "Título obrigatório" }
ValidationFailure { ErrorMessage = "Valor inválido" }
ValidationFailure { ErrorMessage = "Data inválida" }

            Select
              ↓

Saída

"Título obrigatório"
"Valor inválido"
"Data inválida"
```

Essa operação é chamada de **projeção**: cada objeto da coleção original é transformado em outro valor.

---

## Expressões lambda no Select

O `Select` recebe uma função.

Na aula, essa função é escrita utilizando uma **expressão lambda**:

```csharp
failure => failure.ErrorMessage
```

Podemos interpretar essa expressão da seguinte maneira:

```text
failure
   ↓
parâmetro recebido em cada iteração

=>
   ↓
executa / retorna

failure.ErrorMessage
   ↓
valor selecionado
```

Durante a execução, o `Select` percorre todos os elementos:

```text
1ª iteração
failure = primeiro ValidationFailure
retorna failure.ErrorMessage

2ª iteração
failure = segundo ValidationFailure
retorna failure.ErrorMessage

3ª iteração
failure = terceiro ValidationFailure
retorna failure.ErrorMessage
```

Uma variável curta também poderia ser usada:

```csharp
result.Errors.Select(f => f.ErrorMessage)
```

O significado é exatamente o mesmo.

---

## Convertendo para List<string> com ToList

Depois do `Select`, ainda temos uma sequência produzida pelo LINQ.

Como a aplicação deseja uma lista concreta de strings, é utilizado `ToList()`:

```csharp
var errorMessages = result.Errors
    .Select(failure => failure.ErrorMessage)
    .ToList();
```

Agora o tipo de `errorMessages` é:

```csharp
List<string>
```

Fluxo:

```text
result.Errors
List<ValidationFailure>
        ↓
Select(failure => failure.ErrorMessage)
        ↓
IEnumerable<string>
        ↓
ToList()
        ↓
List<string>
```

Esse código permite que a aplicação fique independente da estrutura interna dos objetos `ValidationFailure` quando precisar apenas das mensagens.

---

## Verificando result.IsValid

O `ValidationResult` possui a propriedade:

```csharp
result.IsValid
```

Ela indica se a validação passou ou não.

```text
result.IsValid == true
        ↓
nenhum erro de validação

result.IsValid == false
        ↓
existem erros de validação
```

Portanto, a exceção deve ser lançada apenas quando a validação falhar:

```csharp
if (!result.IsValid)
{
    var errorMessages = result.Errors
        .Select(failure => failure.ErrorMessage)
        .ToList();

    // lançar exceção com os erros
}
```

O operador `!` inverte o valor booleano:

```text
result.IsValid = false
!result.IsValid = true
```

Assim, o bloco do `if` será executado somente quando houver falhas de validação.

Se a requisição estiver válida:

```text
Validate(request)
      ↓
result.IsValid == true
      ↓
não entra no if
      ↓
função Validate termina normalmente
      ↓
fluxo retorna para o Execute
```

---

## Novo problema: exceções não recebem List<string>

Anteriormente, a aplicação utilizava algo semelhante a:

```csharp
throw new ArgumentException("O título é obrigatório.");
```

O construtor utilizado recebe uma única `string`.

Agora, porém, temos:

```csharp
List<string> errorMessages
```

Portanto, isso não funciona:

```csharp
throw new ArgumentException(errorMessages);
```

porque não existe conversão automática de:

```text
List<string>
```

para:

```text
string
```

A aplicação agora precisa transportar múltiplas mensagens de validação através da exceção.

A solução apresentada para a próxima etapa é criar uma **exceção personalizada**.

Conceitualmente:

```text
FluentValidation
      ↓
List<string> de erros
      ↓
Exceção personalizada
      ↓
Controller
      ↓
400 Bad Request
```

---

## Response de erro também precisa evoluir

Existe outro ponto que precisa acompanhar essa mudança.

A Response atual de erro foi criada para armazenar somente uma mensagem:

```csharp
public class ResponseErrorJson
{
    public string ErrorMessage { get; set; }
}
```

Porém, agora a validação pode encontrar vários erros simultaneamente.

Portanto, conceitualmente a resposta precisa evoluir de:

```json
{
  "errorMessage": "O título é obrigatório."
}
```

para algo semelhante a:

```json
{
  "errors": [
    "O título é obrigatório.",
    "O valor deve ser maior que zero.",
    "O tipo de pagamento não é válido."
  ]
}
```

Essa alteração será necessária para que a API consiga devolver ao cliente todas as falhas encontradas em uma única requisição.

---

## Fluxo completo desta etapa

O fluxo de validação começa a ficar assim:

```text
POST /expenses
      │
      ▼
ExpensesController
      │
      ▼
RegisterExpenseUseCase.Execute(request)
      │
      ▼
Validate(request)
      │
      ▼
RegisterExpenseValidator
      │
      ▼
validator.Validate(request)
      │
      ▼
ValidationResult
      │
      ├── IsValid = true
      │       │
      │       └── continua o fluxo
      │
      └── IsValid = false
              │
              ▼
       result.Errors
              │
              ▼
       Select(ErrorMessage)
              │
              ▼
          ToList()
              │
              ▼
       List<string>
              │
              ▼
    exceção personalizada
       (próxima aula)
```

Código principal desta etapa:

```csharp
private void Validate(RequestRegisterExpenseJson request)
{
    var validator = new RegisterExpenseValidator();

    var result = validator.Validate(request);

    if (!result.IsValid)
    {
        var errorMessages = result.Errors
            .Select(failure => failure.ErrorMessage)
            .ToList();

        // Próximo passo: lançar uma exceção personalizada
        // capaz de transportar a lista de erros.
    }
}
```

---

## Resumo

Nesta aula:

- foi introduzido o **LINQ**;
- o `ValidationResult` do FluentValidation foi analisado;
- foi visto que `result.Errors` contém objetos `ValidationFailure`;
- `Select(...)` foi utilizado para selecionar somente `ErrorMessage`;
- uma expressão lambda foi utilizada dentro do `Select`;
- `ToList()` transformou o resultado em `List<string>`;
- `result.IsValid` passou a determinar se existem erros de validação;
- o `if (!result.IsValid)` passou a executar somente quando a Request é inválida;
- surgiu a necessidade de transportar várias mensagens de erro;
- foi identificado que `ArgumentException` não atende diretamente a esse novo formato;
- a `ResponseErrorJson` também precisa evoluir para representar uma lista;
- a próxima etapa será criar uma **exceção personalizada**.

---

## Visão Geral

```text
FluentValidation
      ↓
ValidationResult
      ↓
result.Errors
      ↓
List<ValidationFailure>
      ↓
LINQ Select
failure => failure.ErrorMessage
      ↓
ToList()
      ↓
List<string>
      ↓
if (!result.IsValid)
      ↓
transportar todos os erros
      ↓
Exceção personalizada
(próxima aula)
```

A principal evolução desta aula é deixar de trabalhar diretamente com objetos internos do FluentValidation e extrair apenas os dados que interessam à aplicação: as mensagens de erro.