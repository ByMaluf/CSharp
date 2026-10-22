# Filtro Global de Exceções com `IExceptionFilter`

## 📋 Índice

1. [Contexto da aula](#1-contexto-da-aula)
2. [Problemas do `try-catch` dentro dos Controllers](#2-problemas-do-try-catch-dentro-dos-controllers)
3. [Por que usar um filtro de exceções](#3-por-que-usar-um-filtro-de-exceções)
4. [Criando a pasta `Filters` e a classe `ExceptionFilter`](#4-criando-a-pasta-filters-e-a-classe-exceptionfilter)
5. [`IExceptionFilter`: implementação de interface](#5-iexceptionfilter-implementação-de-interface)
6. [O método `OnException`](#6-o-método-onexception)
7. [Separando exceções do projeto e erros desconhecidos](#7-separando-exceções-do-projeto-e-erros-desconhecidos)
8. [`CashFlowException` como exceção-base do projeto](#8-cashflowexception-como-exceção-base-do-projeto)
9. [`HandleProjectException`](#9-handleprojectexception)
10. [`ThrowUnknownError`](#10-throwunknownerror)
11. [Definindo o status HTTP pelo `ExceptionContext`](#11-definindo-o-status-http-pelo-exceptioncontext)
12. [`ObjectResult` e `BadRequestObjectResult`](#12-objectresult-e-badrequestobjectresult)
13. [Casting de exceções](#13-casting-de-exceções)
14. [Cast direto x operador `as`](#14-cast-direto-x-operador-as)
15. [Fluxo geral do tratamento](#15-fluxo-geral-do-tratamento)
16. [Resumo](#16-resumo)
17. [Visão Geral](#17-visão-geral)

---

## 1. Contexto da aula

Até este momento, o tratamento de exceções estava sendo feito diretamente dentro do Controller com `try-catch`.

A aula apresenta uma nova abordagem: criar um **filtro de exceções**, responsável por centralizar o tratamento das exceções da aplicação.

A ideia é retirar essa responsabilidade dos endpoints e deixar os Controllers menores e mais focados em receber a requisição, chamar a regra de negócio e devolver a resposta.

---

## 2. Problemas do `try-catch` dentro dos Controllers

A aula destaca dois problemas principais.

### 2.1. Controllers ficam grandes

Um endpoint simples passa a ter várias linhas apenas para tratar exceções:

```csharp
try
{
    // execução do endpoint
}
catch (...)
{
    // tratamento
}
catch (...)
{
    // outro tratamento
}
```

Se a aplicação tiver vários endpoints, como:

- cadastrar despesa;
- atualizar despesa;
- excluir despesa;
- buscar despesa por ID;
- gerar relatório;

seria necessário repetir estruturas semelhantes em vários métodos.

Isso aumenta o tamanho das classes e gera repetição de código.

### 2.2. Um endpoint pode gerar diferentes exceções

Um mesmo endpoint pode produzir diferentes situações de erro.

Exemplo conceitual para atualização de uma despesa:

```text
PUT /expenses/{id}
        |
        +--> ID não existe
        |       |
        |       +--> Not Found
        |
        +--> dados enviados são inválidos
                |
                +--> Bad Request
```

Portanto, conforme a aplicação cresce, o número de `catch` também tende a crescer.

---

## 3. Por que usar um filtro de exceções

O filtro concentra a responsabilidade de tratamento em um único ponto.

Em vez de cada Controller possuir seu próprio `try-catch`, a ideia apresentada é:

```text
Controller
   |
   v
Use Case
   |
   +--> executa normalmente
   |
   +--> lança exceção
            |
            v
     ExceptionFilter
            |
            +--> identifica o tipo
            +--> define status HTTP
            +--> monta a resposta
```

Com isso, os endpoints ficam menores.

A aula também comenta que, mais adiante, a criação manual do Use Case dentro do Controller será substituída por **injeção de dependência**, deixando o endpoint ainda mais enxuto.

---

## 4. Criando a pasta `Filters` e a classe `ExceptionFilter`

Dentro do projeto de API é criada uma nova pasta:

```text
CashFlow.API
└── Filters
    └── ExceptionFilter.cs
```

A classe é criada como pública:

```csharp
public class ExceptionFilter
{
}
```

Porém, apenas criar a classe não faz dela um filtro de exceções.

Para isso, ela precisa implementar a interface apropriada do ASP.NET Core.

---

## 5. `IExceptionFilter`: implementação de interface

A classe passa a implementar `IExceptionFilter`:

```csharp
public class ExceptionFilter : IExceptionFilter
{
}
```

É necessário utilizar o namespace relacionado aos filtros do ASP.NET Core MVC.

Um ponto importante da aula é a diferença conceitual entre **herança** e **implementação de interface**.

### Herança

```csharp
public class Filho : Pai
{
}
```

Uma classe deriva de outra classe.

### Interface

```csharp
public class ExceptionFilter : IExceptionFilter
{
}
```

Nesse caso, a classe está **implementando uma interface**.

A aula apresenta a interface como uma espécie de contrato.

Ela define membros que a classe que a implementa deverá fornecer.

---

## 6. O método `OnException`

Ao implementar `IExceptionFilter`, a classe precisa implementar o método exigido pela interface:

```csharp
public void OnException(ExceptionContext context)
{
}
```

O `ExceptionContext` contém informações relacionadas à exceção e à requisição HTTP em execução.

A exceção pode ser acessada por:

```csharp
context.Exception
```

A partir desse ponto, o filtro pode identificar o tipo da exceção e decidir qual resposta HTTP devolver.

---

## 7. Separando exceções do projeto e erros desconhecidos

A aula divide o tratamento em dois grandes grupos:

```text
Exceção capturada
       |
       v
É uma exceção conhecida do CashFlow?
       |
   +---+---+
   |       |
  SIM     NÃO
   |       |
   v       v
Handle   Erro 500
Project  desconhecido
Exception
```

Para manter `OnException` pequeno, são criados métodos auxiliares privados.

Conceitualmente:

```csharp
public void OnException(ExceptionContext context)
{
    if (context.Exception is CashFlowException)
    {
        HandleProjectException(context);
    }
    else
    {
        ThrowUnknownError(context);
    }
}
```

Assim, `OnException` apenas direciona o fluxo.

---

## 8. `CashFlowException` como exceção-base do projeto

A aula utiliza `CashFlowException` como classe-base das exceções criadas pelo próprio projeto.

O raciocínio é:

```text
CashFlowException
       |
       +--> ErrorValidationException
       |
       +--> futuras exceções do projeto
              ex.: NotFoundException
```

Como `ErrorValidationException` herda de `CashFlowException`, então ela também pode ser identificada como uma `CashFlowException`.

Isso permite fazer uma verificação mais genérica dentro de `OnException`:

```csharp
if (context.Exception is CashFlowException)
{
    HandleProjectException(context);
}
```

Em vez de colocar todos os tipos específicos diretamente em `OnException`.

---

## 9. `HandleProjectException`

A responsabilidade desse método é tratar exceções conhecidas da aplicação.

```csharp
private void HandleProjectException(ExceptionContext context)
{
    // identifica o tipo específico da exceção
    // define o status HTTP correspondente
    // cria a resposta apropriada
}
```

Por enquanto, a aula trabalha com `ErrorValidationException`.

Exemplo conceitual:

```csharp
if (context.Exception is ErrorValidationException)
{
    // retornar 400 Bad Request
}
```

No futuro, esse método poderá receber novos tratamentos, por exemplo:

```text
ErrorValidationException
        -> 400 Bad Request

NotFoundException
        -> 404 Not Found

outras exceções do projeto
        -> status correspondente
```

A vantagem é concentrar essa lógica em um único lugar.

---

## 10. `ThrowUnknownError`

Se a exceção não for uma exceção conhecida do projeto, a aplicação deve devolver um erro genérico.

A aula utiliza o status:

```text
500 Internal Server Error
```

A intenção é não devolver detalhes internos de uma exceção desconhecida para quem chamou a API.

Estrutura conceitual:

```csharp
private void ThrowUnknownError(ExceptionContext context)
{
    context.HttpContext.Response.StatusCode =
        StatusCodes.Status500InternalServerError;

    // configurar a resposta de erro
}
```

---

## 11. Definindo o status HTTP pelo `ExceptionContext`

Dentro de um Controller era possível utilizar métodos como:

```csharp
return StatusCode(...);
```

Isso acontece porque o Controller herda de `ControllerBase`.

No `ExceptionFilter`, essa facilidade não existe da mesma forma.

Por isso, o status da resposta é configurado através do contexto HTTP:

```csharp
context.HttpContext.Response.StatusCode =
    StatusCodes.Status500InternalServerError;
```

Para uma validação inválida:

```csharp
context.HttpContext.Response.StatusCode =
    StatusCodes.Status400BadRequest;
```

Portanto:

```text
Controller
    -> possui helpers como BadRequest() e StatusCode()

ExceptionFilter
    -> trabalha diretamente com ExceptionContext / HttpContext
```

---

## 12. `ObjectResult` e `BadRequestObjectResult`

Além do status HTTP, é necessário configurar o corpo da resposta.

O `ExceptionContext` possui a propriedade:

```csharp
context.Result
```

Ela recebe um `IActionResult`.

Para o erro genérico, a aula utiliza a ideia de um `ObjectResult`:

```csharp
context.Result = new ObjectResult(errorResponse);
```

Para erro de validação, é utilizado um `BadRequestObjectResult`:

```csharp
context.Result = new BadRequestObjectResult(errorResponse);
```

A diferença principal no contexto da aula é o tipo de resposta associado ao cenário tratado.

---

## 13. Casting de exceções

Dentro de `HandleProjectException`, a aula verifica primeiro o tipo da exceção:

```csharp
if (context.Exception is ErrorValidationException)
{
    // sabemos que a exceção é desse tipo
}
```

Porém, apenas verificar com `is` devolve um valor booleano.

Para acessar os membros específicos de `ErrorValidationException`, é necessário converter a referência.

Exemplo de cast direto:

```csharp
var exception = (ErrorValidationException)context.Exception;
```

A lógica é:

```text
context.Exception
       |
       | tipo base: Exception
       v
verificação com `is`
       |
       v
cast para ErrorValidationException
       |
       v
acesso aos membros específicos
```

---

## 14. Cast direto x operador `as`

A aula mostra duas formas de conversão.

### Cast direto

```csharp
var exception = (ErrorValidationException)context.Exception;
```

Se o objeto não puder ser convertido para o tipo solicitado, ocorre uma exceção em tempo de execução.

No cenário da aula, isso é considerado seguro porque antes já existe uma verificação:

```csharp
if (context.Exception is ErrorValidationException)
```

### Operador `as`

Outra forma mostrada é:

```csharp
var exception = context.Exception as ErrorValidationException;
```

Se a conversão não for possível, essa variável recebe `null` em vez de provocar imediatamente uma exceção de cast.

Comparação apresentada:

| Forma | Se o tipo for compatível | Se não for compatível |
|---|---|---|
| `(ErrorValidationException)obj` | converte | lança exceção |
| `obj as ErrorValidationException` | converte | retorna `null` |

A aula mantém o cast direto porque a verificação com `is` já garante o tipo naquele fluxo.

---

## 15. Fluxo geral do tratamento

O fluxo construído na aula pode ser visualizado assim:

```text
Endpoint
   |
   v
Use Case
   |
   +--> sucesso
   |      |
   |      v
   |   resposta normal
   |
   +--> throw
          |
          v
   ExceptionFilter.OnException
          |
          v
context.Exception é CashFlowException?
          |
     +----+----+
     |         |
    SIM       NÃO
     |         |
     v         v
HandleProject  ThrowUnknownError
Exception      |
     |         +--> 500
     |
     v
qual exceção específica?
     |
     +--> ErrorValidationException
             |
             +--> 400 Bad Request
```

A principal consequência é que o Controller deixa de ser responsável pelo tratamento detalhado de exceções.

---

## 16. Resumo

Nesta aula foi introduzido o uso de um **filtro de exceções** para centralizar o tratamento de erros da API.

Os principais pontos foram:

- evitar `try-catch` repetidos em todos os endpoints;
- criar a pasta `Filters` no projeto de API;
- criar `ExceptionFilter`;
- implementar `IExceptionFilter`;
- entender interface como um contrato;
- implementar `OnException(ExceptionContext context)`;
- acessar a exceção por `context.Exception`;
- separar exceções conhecidas do projeto de erros desconhecidos;
- usar `CashFlowException` como tipo-base das exceções da aplicação;
- encaminhar exceções conhecidas para `HandleProjectException`;
- devolver `500 Internal Server Error` para erro desconhecido;
- configurar status HTTP através de `context.HttpContext.Response.StatusCode`;
- configurar o corpo por `context.Result`;
- utilizar `ObjectResult` e `BadRequestObjectResult`;
- verificar tipos com `is`;
- converter exceções através de casting;
- diferenciar cast direto do operador `as`;
- preparar o filtro para receber novos tipos, como `NotFoundException`, futuramente.

---

## 17. Visão Geral

```text
ANTES

Controller
└── Endpoint
    └── try
        ├── Use Case
        └── catch
            ├── 400
            ├── 404
            └── 500

Problema:
- repetição
- métodos grandes
- Controllers maiores
- tratamento espalhado


DEPOIS

Controller
└── Endpoint
    └── Use Case
        └── throw
            |
            v
      ExceptionFilter
      ├── CashFlowException
      │   └── HandleProjectException
      │       ├── ErrorValidationException -> 400
      │       └── futuras exceções -> outros status
      │
      └── Exception desconhecida
          └── ThrowUnknownError -> 500
```

O filtro passa a ser o ponto central para transformar exceções da aplicação em respostas HTTP adequadas, deixando os Controllers mais simples e preparando a aplicação para novos tipos de erro.