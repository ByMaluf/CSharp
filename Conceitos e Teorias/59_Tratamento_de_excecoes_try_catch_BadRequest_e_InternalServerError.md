# Tratamento de Exceções com try-catch, BadRequest e InternalServerError

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Problema das exceções não tratadas](#problema-das-exceções-não-tratadas)
3. [Por que tratar exceções](#por-que-tratar-exceções)
4. [Estrutura try-catch](#estrutura-try-catch)
5. [Capturando exceções específicas](#capturando-exceções-específicas)
6. [Catch genérico](#catch-genérico)
7. [Bloco finally](#bloco-finally)
8. [Transformando ArgumentException em 400](#transformando-argumentexception-em-400)
9. [Tratando erros desconhecidos com 500](#tratando-erros-desconhecidos-com-500)
10. [Fluxo final](#fluxo-final)
11. [Resumo](#resumo)
12. [Visão Geral](#visão-geral)

---

## Contexto da aula

Na aula anterior, o `RegisterExpenseUseCase` passou a validar os dados recebidos pela `RequestExpenseJson` e lançar `ArgumentException` quando alguma regra não fosse atendida.

Nesta aula, o objetivo é testar essas validações e tratar corretamente as exceções antes de devolver uma resposta HTTP ao cliente.

A ideia central é:

```text
Request
↓
Controller
↓
Use Case
↓
Validate
↓
Exception
↓
tratamento da Exception
↓
Response HTTP apropriada
```

---

## Problema das exceções não tratadas

Ao enviar uma requisição com o título vazio, o método de validação lança:

```csharp
throw new ArgumentException("O título é obrigatório");
```

Sem tratamento, a API devolve:

```text
500 Internal Server Error
```

Além disso, o corpo da resposta pode conter diversas informações técnicas, como:

- nome da classe;
- nome do Use Case;
- linha onde ocorreu a exceção;
- caminho de arquivos;
- stack trace;
- detalhes internos da aplicação.

Segundo a aula, esse comportamento não deve ser exposto diretamente para quem consome a API.

---

## Por que tratar exceções

A aula destaca dois motivos principais.

### 1. Responder corretamente ao cliente

Uma validação de entrada inválida não deveria ser tratada como um erro interno inesperado.

Por exemplo:

```text
Título obrigatório não informado
```

é tratado como:

```text
400 Bad Request
```

em vez de:

```text
500 Internal Server Error
```

### 2. Não expor informações internas

Uma exceção pode carregar informações sensíveis ou desnecessárias.

O exemplo apresentado na aula é uma eventual mensagem contendo detalhes como endereço de servidor, usuário ou até credenciais.

Por isso, exceções devem ser tratadas e transformadas em respostas controladas.

---

## Estrutura try-catch

Em C#, a estrutura básica apresentada é:

```csharp
try
{
    // Código que será executado normalmente
}
catch (Exception ex)
{
    // Tratamento caso uma exceção seja lançada
}
```

O funcionamento é:

```text
try
│
├── tenta executar o código
│
├── nenhum erro → segue normalmente
│
└── ocorreu uma exceção
       ↓
     catch
       ↓
   tratamento
```

No Controller, o código que instancia e executa o Use Case passa a ficar dentro do `try`.

Exemplo conceitual:

```csharp
try
{
    var useCase = new RegisterExpenseUseCase();
    var response = useCase.Execute(request);

    return Created(string.Empty, response);
}
```

---

## Capturando exceções específicas

A aula explica que é possível capturar exceções por tipo.

Como as validações estão lançando `ArgumentException`, pode-se fazer:

```csharp
catch (ArgumentException ex)
{
    // tratamento específico
}
```

Assim, apenas exceções desse tipo entram nesse `catch`.

Também é possível ter múltiplos blocos:

```csharp
try
{
    // código
}
catch (ArgumentException ex)
{
    // tratamento de ArgumentException
}
catch (DivideByZeroException ex)
{
    // tratamento de divisão por zero
}
```

A lógica ensinada é:

```text
Exceção lançada
↓
qual é o tipo?
↓
procura um catch compatível
↓
executa o tratamento daquele catch
```

---

## A variável da exceção

No exemplo:

```csharp
catch (ArgumentException ex)
```

`ex` representa a instância da exceção capturada.

Por meio dela é possível acessar informações como:

```csharp
ex.Message
```

No caso das validações da despesa, essa mensagem foi definida ao lançar a exceção:

```csharp
throw new ArgumentException("O título é obrigatório");
```

Então:

```csharp
ex.Message
```

será:

```text
O título é obrigatório
```

---

## Catch genérico

A classe `Exception` é apresentada como classe base das exceções.

Assim:

```csharp
catch (Exception ex)
{
}
```

funciona como um tratamento mais genérico.

A ordem conceitual fica:

```text
catch específico
↓
catch específico
↓
catch genérico
```

Exemplo:

```csharp
try
{
    // código
}
catch (ArgumentException ex)
{
    // validações
}
catch (Exception ex)
{
    // demais erros
}
```

O `catch (Exception)` recebe exceções que não foram tratadas pelos blocos mais específicos anteriores.

---

## Catch sem variável

Também é possível escrever:

```csharp
catch
{
}
```

Nesse caso, o bloco captura exceções que chegarem até ele, mas não fornece uma variável como `ex` para acessar a instância da exceção.

Ou seja:

```text
catch (Exception ex)
→ captura e permite acessar a exceção

catch
→ captura, mas não fornece acesso à instância
```

---

## Bloco finally

A estrutura também pode possuir um `finally`:

```csharp
try
{
}
catch (Exception ex)
{
}
finally
{
}
```

Segundo a aula, o código dentro do `finally` será executado independentemente de o `try` terminar com sucesso ou de alguma exceção ser capturada.

```text
try executou com sucesso
           └──► finally

try lançou exceção
      ↓
    catch
      └────────► finally
```

A aula apresenta o recurso principalmente para conhecimento, observando que ele não será necessário naquele momento.

---

## Exceções não capturadas

Se uma exceção ocorrer e nenhum `catch` for compatível com seu tipo, ela permanece sem tratamento.

Exemplo conceitual:

```csharp
catch (ArgumentException ex)
{
}
catch (DivideByZeroException ex)
{
}
```

Se surgir uma exceção diferente dessas duas e não existir tratamento genérico, ela não será capturada por esses blocos.

---

## Transformando ArgumentException em 400

Como as `ArgumentException` desta etapa representam erros nas validações da requisição, a aula converte esse tipo de exceção em:

```text
400 Bad Request
```

Código apresentado conceitualmente:

```csharp
catch (ArgumentException ex)
{
    return BadRequest(ex.Message);
}
```

Assim:

```text
Validate detecta título vazio
↓
throw new ArgumentException(...)
↓
Controller captura ArgumentException
↓
BadRequest(ex.Message)
↓
HTTP 400
```

Exemplo de resposta:

```text
400 Bad Request

O título é obrigatório
```

---

## Tratando erros desconhecidos com 500

Para qualquer outro erro que não seja uma `ArgumentException`, a aula adiciona um tratamento genérico.

Em vez de devolver todos os detalhes da exceção, é retornada uma mensagem controlada:

```text
Erro desconhecido
```

Como não existe um método específico como `InternalServerError()` no exemplo mostrado, é utilizado:

```csharp
return StatusCode(
    StatusCodes.Status500InternalServerError,
    "Erro desconhecido"
);
```

A aula também chama atenção para evitar números mágicos como:

```csharp
500
```

preferindo:

```csharp
StatusCodes.Status500InternalServerError
```

---

## Controller com tratamento

A estrutura final apresentada nesta etapa fica conceitualmente assim:

```csharp
try
{
    var useCase = new RegisterExpenseUseCase();
    var response = useCase.Execute(request);

    return Created(string.Empty, response);
}
catch (ArgumentException ex)
{
    return BadRequest(ex.Message);
}
catch
{
    return StatusCode(
        StatusCodes.Status500InternalServerError,
        "Erro desconhecido"
    );
}
```

Nesta fase do curso, o tratamento é colocado diretamente no Controller.

---

## Testes no Swagger

### Título vazio

A validação lança:

```text
O título é obrigatório
```

Resultado após o tratamento:

```text
400 Bad Request
```

com apenas a mensagem controlada, sem expor o stack trace na resposta.

### Valor inválido

Se:

```text
Amount <= 0
```

a validação lança a mensagem correspondente e o `catch (ArgumentException)` transforma o erro em `400 Bad Request`.

### PaymentType inválido

Ao enviar:

```text
PaymentType = 7
```

sendo que o enum possui apenas os valores esperados pela aplicação, a validação detecta o problema e lança a exceção.

Resultado:

```text
400 Bad Request
Tipo de pagamento não é válido
```

---

## Stack Trace

Durante o debug, a aula mostra que a exceção continua contendo informações técnicas internamente, incluindo o stack trace.

A diferença é que agora essas informações não são automaticamente expostas na resposta da API.

Representação:

```text
Exception
├── Message
├── StackTrace
├── tipo da exceção
└── outras informações internas

            ↓ tratamento

Resposta HTTP
├── status code apropriado
└── mensagem controlada
```

---

## Fluxo final

```text
POST /expense
      │
      ▼
ExpensesController
      │
      ▼
try
      │
      ▼
RegisterExpenseUseCase.Execute(request)
      │
      ▼
Validate(request)
      │
 ┌────┴─────────────┐
 │                  │
 ▼                  ▼
válido             inválido
 │                  │
 ▼                  ▼
Created        ArgumentException
201                 │
                    ▼
                  catch
                    │
                    ▼
             400 Bad Request

Outro erro inesperado
        │
        ▼
   catch genérico
        │
        ▼
500 Internal Server Error
"Erro desconhecido"
```

---

## Resumo

| Conceito | Função |
|---|---|
| `try` | Tenta executar um bloco de código |
| `catch` | Captura uma exceção |
| `catch (ArgumentException ex)` | Trata especificamente `ArgumentException` |
| `catch (Exception ex)` | Tratamento genérico de exceções |
| `catch` | Captura sem acessar a instância da exceção |
| `finally` | Executa independentemente do resultado do `try` |
| `ex.Message` | Obtém a mensagem da exceção |
| `BadRequest(...)` | Retorna HTTP `400` |
| `StatusCode(...)` | Permite informar manualmente um status HTTP |
| `StatusCodes.Status500InternalServerError` | Representa HTTP `500` sem número mágico |

### Regra adotada nesta aula

```text
ArgumentException
→ erro de validação
→ 400 Bad Request

Outra Exception
→ erro desconhecido
→ 500 Internal Server Error
```

---

## Visão Geral

```text
VALIDAÇÃO
   │
   ▼
throw Exception
   │
   ▼
try / catch
   │
   ├── ArgumentException
   │      └──► 400 Bad Request
   │
   └── outro erro
          └──► 500 Internal Server Error
                "Erro desconhecido"
```

> **Em resumo:** a aula mostra que validar dados não é suficiente: também é necessário tratar as exceções geradas pelas validações. O Controller passa a utilizar `try-catch`, convertendo `ArgumentException` em `400 Bad Request` e demais erros em `500 Internal Server Error`, evitando expor diretamente detalhes internos e stack traces na resposta da API.