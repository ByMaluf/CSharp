# Registro do ExceptionFilter no Program.cs, testes e fallback

## 📋 Índice

1. [Visão geral](#1-visão-geral)
2. [Removendo o try-catch do Controller](#2-removendo-o-try-catch-do-controller)
3. [Por que registrar o filtro no Program.cs](#3-por-que-registrar-o-filtro-no-programcs)
4. [Registrando o ExceptionFilter com AddMvc](#4-registrando-o-exceptionfilter-com-addmvc)
5. [Entendendo a expressão lambda de configuração](#5-entendendo-a-expressão-lambda-de-configuração)
6. [MvcOptions e a coleção Filters](#6-mvcoptions-e-a-coleção-filters)
7. [Uso de typeof](#7-uso-de-typeof)
8. [Fluxo completo de uma exceção de validação](#8-fluxo-completo-de-uma-exceção-de-validação)
9. [Teste no Swagger](#9-teste-no-swagger)
10. [Por que o try-catch deixa de ser necessário nos Controllers](#10-por-que-o-try-catch-deixa-de-ser-necessário-nos-controllers)
11. [E se a CashFlowException não for um erro conhecido?](#11-e-se-a-cashflowexception-não-for-um-erro-conhecido)
12. [Fallback dentro do tratamento das exceções do projeto](#12-fallback-dentro-do-tratamento-das-exceções-do-projeto)
13. [Fluxo final](#13-fluxo-final)
14. [Resumo](#14-resumo)
15. [Visão Geral](#15-visão-geral)

---

## 1. Visão geral

Na aula anterior foi criado um filtro global de exceções para retirar dos Controllers a responsabilidade de tratar erros com `try-catch`.

Nesta aula o objetivo é completar esse fluxo:

- remover o `try-catch` do Controller;
- registrar o filtro no `Program.cs`;
- fazer a API reconhecer o `ExceptionFilter`;
- testar o comportamento no Swagger;
- verificar o fluxo até `OnException`;
- confirmar o retorno `400 Bad Request` com várias mensagens;
- adicionar uma proteção para exceções do projeto que ainda não possuam tratamento específico.

A ideia central passa a ser:

```text
Controller
   ↓
Use Case
   ↓
Validação
   ↓
throw
   ↓
ExceptionFilter.OnException
   ↓
Tratamento centralizado
   ↓
Resposta HTTP
```

---

## 2. Removendo o try-catch do Controller

Antes do filtro, o Controller possuía uma estrutura semelhante a:

```csharp
try
{
    var useCase = new RegisterExpenseUseCase();
    var response = useCase.Execute(request);

    return Created(string.Empty, response);
}
catch (...)
{
    // tratamento
}
```

Depois da criação do filtro global, essa estrutura deixa de ser necessária no endpoint.

O Controller volta a ficar focado somente em sua responsabilidade principal:

```csharp
var useCase = new RegisterExpenseUseCase();
var response = useCase.Execute(request);

return Created(string.Empty, response);
```

Isso reduz bastante o tamanho dos métodos e evita repetir o mesmo tratamento em todos os endpoints.

### Antes

```text
Controller
├── recebe request
├── executa Use Case
├── trata validação
├── trata erro desconhecido
└── devolve response
```

### Depois

```text
Controller
├── recebe request
├── executa Use Case
└── devolve response

ExceptionFilter
└── trata exceções
```

---

## 3. Por que registrar o filtro no Program.cs

Criar a classe `ExceptionFilter` não é suficiente.

A API ainda precisa ser informada de que esse filtro existe e deve fazer parte do pipeline do MVC.

Essa configuração é feita no `Program.cs`, onde ficam registradas várias configurações necessárias antes da aplicação começar a receber requisições.

De forma simplificada:

```text
Program.cs
   ↓
Configuração de serviços
   ↓
Configuração do MVC
   ↓
Registro dos filtros
   ↓
Build da aplicação
   ↓
app.Run()
   ↓
API pronta para receber requisições
```

---

## 4. Registrando o ExceptionFilter com AddMvc

O filtro é registrado na configuração do MVC.

A estrutura apresentada na aula é equivalente a:

```csharp
builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ExceptionFilter));
});
```

A partir desse registro, o MVC passa a conhecer o filtro criado pela aplicação.

O ponto importante é que o filtro não precisa ser chamado manualmente pelos Controllers.

Depois de registrado, ele participa do fluxo da aplicação e é acionado quando ocorre uma exceção dentro da execução controlada pelo MVC.

---

## 5. Entendendo a expressão lambda de configuração

O método `AddMvc` recebe uma configuração por meio de uma função lambda.

Exemplo:

```csharp
builder.Services.AddMvc(options =>
{
    // configurações do MVC
});
```

O parâmetro `options` representa as opções utilizadas para configurar o MVC.

A expressão:

```csharp
options =>
{
    // configuração
}
```

é uma função anônima.

O fluxo pode ser entendido assim:

```text
AddMvc(...)
   ↓
recebe uma função
   ↓
essa função recebe options
   ↓
options contém configurações do MVC
```

---

## 6. MvcOptions e a coleção Filters

Dentro de `options` existe uma propriedade chamada `Filters`.

Ela representa uma coleção de filtros configurados para o MVC.

Por isso é possível fazer:

```csharp
options.Filters.Add(...);
```

Conceitualmente:

```text
MvcOptions
└── Filters
    ├── Filter A
    ├── Filter B
    └── ExceptionFilter
```

Na aplicação atual, o objetivo é adicionar o filtro responsável pelo tratamento global das exceções.

---

## 7. Uso de typeof

O método `Add` utilizado na coleção de filtros recebe o tipo do filtro.

Passar diretamente o nome da classe não corresponde ao valor esperado.

Por isso é utilizado `typeof`:

```csharp
options.Filters.Add(typeof(ExceptionFilter));
```

`typeof` devolve uma instância de `System.Type` que representa aquele tipo em tempo de execução.

Exemplo:

```csharp
Type filterType = typeof(ExceptionFilter);
```

### Comparação

| Expressão | Representa |
|---|---|
| `ExceptionFilter` | nome do tipo usado no código |
| `new ExceptionFilter()` | uma instância da classe |
| `typeof(ExceptionFilter)` | objeto `System.Type` que descreve o tipo |

Nesse caso, a coleção de filtros precisa da representação do tipo.

---

## 8. Fluxo completo de uma exceção de validação

Depois do registro, a aula testa novamente uma requisição inválida.

O fluxo observado é:

```text
Swagger
   ↓
ExpensesController
   ↓
RegisterExpenseUseCase
   ↓
Validate(request)
   ↓
FluentValidation
   ↓
ValidationResult.IsValid == false
   ↓
lista de mensagens de erro
   ↓
throw ErrorOnValidationException
   ↓
ExceptionFilter.OnException
   ↓
context.Exception é uma CashFlowException?
   ↓ sim
HandleProjectException(context)
   ↓
É ErrorOnValidationException?
   ↓ sim
ResponseErrorJson
   ↓
400 Bad Request
```

Essa centralização permite que o Controller fique completamente separado do tratamento das exceções.

---

## 9. Teste no Swagger

Para testar o comportamento, a requisição é enviada com dados inválidos.

Por exemplo:

- título vazio;
- valor inválido.

O FluentValidation identifica mais de um problema e a exceção de validação carrega essas mensagens.

O filtro recebe a exceção e devolve uma resposta semelhante a:

```json
{
  "errors": [
    "O título é obrigatório",
    "O valor deve ser maior que zero"
  ]
}
```

Com status HTTP:

```text
400 Bad Request
```

O ponto mais importante é que agora a API consegue devolver várias mensagens de validação de uma única vez sem que o Controller possua `try-catch`.

---

## 10. Por que o try-catch deixa de ser necessário nos Controllers

Antes:

```text
Endpoint A → try-catch
Endpoint B → try-catch
Endpoint C → try-catch
Endpoint D → try-catch
```

Depois do filtro:

```text
Endpoint A ─┐
Endpoint B ─┤
Endpoint C ─┼──→ ExceptionFilter
Endpoint D ─┘
```

Isso traz algumas vantagens importantes dentro da organização apresentada no curso:

- Controllers menores;
- menos repetição;
- tratamento centralizado;
- respostas de erro mais consistentes;
- mais facilidade para adicionar novos tipos de exceção posteriormente.

---

## 11. E se a CashFlowException não for um erro conhecido?

No `OnException`, a aplicação verifica primeiro se a exceção pertence à hierarquia de exceções do próprio projeto.

Conceitualmente:

```csharp
if (context.Exception is CashFlowException)
{
    HandleProjectException(context);
}
else
{
    ThrowUnknownError(context);
}
```

Isso separa dois grupos:

```text
Exception
├── CashFlowException
│   ├── ErrorOnValidationException
│   ├── futuras exceções do projeto
│   └── ...
│
└── outras exceções
    └── erro desconhecido / 500
```

Porém surge outra questão:

> E se a exceção for uma `CashFlowException`, mas ainda não existir um `if` específico dentro de `HandleProjectException` para aquele subtipo?

Se nenhuma condição específica for atendida, o método poderia terminar sem construir adequadamente uma resposta.

---

## 12. Fallback dentro do tratamento das exceções do projeto

Para evitar esse cenário, a aula adiciona um `else` como proteção.

A ideia conceitual é:

```csharp
private void HandleProjectException(ExceptionContext context)
{
    if (context.Exception is ErrorOnValidationException)
    {
        // tratamento específico da validação
        return;
    }

    // fallback para uma CashFlowException ainda não tratada especificamente
}
```

Na abordagem da aula, esse fallback pode gerar um `Bad Request` utilizando a mensagem da própria exceção:

```csharp
context.Exception.Message
```

A razão para isso é que outras exceções personalizadas poderão carregar uma única mensagem em vez de uma lista de erros.

Assim, a aplicação passa a ter dois níveis de fallback:

```text
Exception
   ↓
É CashFlowException?
   ├── NÃO → erro desconhecido / 500
   │
   └── SIM → HandleProjectException
              ↓
              É subtipo conhecido?
              ├── SIM → tratamento específico
              └── NÃO → fallback de exceção do projeto
```

---

## 13. Fluxo final

Após essa aula, o fluxo fica organizado da seguinte forma:

```text
REQUISIÇÃO HTTP
      ↓
Controller
      ↓
Use Case
      ↓
Validação / Regra de negócio
      ↓
      ├── sucesso
      │      ↓
      │   resposta normal
      │
      └── exceção
             ↓
     ExceptionFilter.OnException
             ↓
   ┌─────────┴─────────┐
   │                   │
CashFlowException   outra Exception
   │                   │
   ↓                   ↓
HandleProject      erro desconhecido
Exception              │
   │                    ↓
   ↓                   500
subtipo conhecido?
   │
   ├── ErrorOnValidationException
   │        ↓
   │       400
   │
   └── fallback do projeto
            ↓
           400
```

---

## 14. Resumo

Nesta aula o filtro de exceções criado anteriormente passa a ser efetivamente utilizado pela API.

Os pontos principais foram:

- remoção do `try-catch` do Controller;
- registro do `ExceptionFilter` no `Program.cs`;
- uso de `builder.Services.AddMvc(...)`;
- configuração por meio de uma expressão lambda;
- acesso a `options.Filters`;
- registro usando `typeof(ExceptionFilter)`;
- execução automática do `OnException` quando ocorre uma exceção;
- identificação de exceções derivadas de `CashFlowException`;
- tratamento específico de `ErrorOnValidationException`;
- retorno de `400 Bad Request` com múltiplas mensagens;
- manutenção dos Controllers sem `try-catch` repetido;
- criação de um fallback para exceções do projeto ainda não tratadas especificamente.

---

## 15. Visão Geral

```text
Program.cs
└── AddMvc(options =>
    └── options.Filters.Add(typeof(ExceptionFilter))

                    ↓

              ExceptionFilter
              └── OnException
                  │
                  ├── CashFlowException
                  │   └── HandleProjectException
                  │       ├── ErrorOnValidationException → 400
                  │       └── fallback → 400
                  │
                  └── Exception desconhecida
                      └── 500 Internal Server Error
```

O resultado é uma aplicação com Controllers menores e um ponto central para transformar exceções em respostas HTTP adequadas.