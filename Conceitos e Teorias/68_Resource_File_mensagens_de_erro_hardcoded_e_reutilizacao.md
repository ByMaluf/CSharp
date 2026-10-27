# Resource File, mensagens de erro hard-coded e reutilização

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [O problema das mensagens hard-coded](#2-o-problema-das-mensagens-hard-coded)
3. [Duplicação e dificuldade de manutenção](#3-duplicação-e-dificuldade-de-manutenção)
4. [Resource File no .NET](#4-resource-file-no-net)
5. [Criando o ResourceErrorMessages](#5-criando-o-resourceerrormessages)
6. [Modificador de acesso public](#6-modificador-de-acesso-public)
7. [Estrutura chave-valor](#7-estrutura-chave-valor)
8. [Substituindo mensagens no ExceptionFilter](#8-substituindo-mensagens-no-exceptionfilter)
9. [Substituindo mensagens no Validator](#9-substituindo-mensagens-no-validator)
10. [Benefícios da centralização](#10-benefícios-da-centralização)
11. [Preparação para múltiplos idiomas](#11-preparação-para-múltiplos-idiomas)
12. [Resumo](#12-resumo)
13. [Visão Geral](#13-visão-geral)

---

## 1. Objetivo da aula

Nesta aula, o projeto passa por mais uma refatoração. O foco deixa de ser o tratamento das exceções em si e passa a ser **onde as mensagens de erro ficam armazenadas**.

Até aqui, as mensagens estavam escritas diretamente no código, tanto no `RegisterExpenseValidator` quanto no `ExceptionFilter`.

Exemplo do problema:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage("O título é obrigatório");
```

E também:

```csharp
var errorResponse = new ResponseErrorJson("Erro desconhecido");
```

A aula mostra por que essa abordagem começa a gerar problemas conforme o sistema cresce.

---

## 2. O problema das mensagens hard-coded

Quando um texto fica escrito diretamente no código-fonte, ele está **hard-coded**.

Exemplo:

```csharp
.WithMessage("O título é obrigatório")
```

Isso funciona tecnicamente, mas dificulta a manutenção.

A ideia da aula é evitar que mensagens de erro importantes fiquem espalhadas por Validators, filtros e outras classes da aplicação.

---

## 3. Duplicação e dificuldade de manutenção

Imagine que vários Validators possuam uma propriedade `Title` e usem a mesma regra:

```text
Validator A
   └── "O título é obrigatório"

Validator B
   └── "O título é obrigatório"

Validator C
   └── "O título é obrigatório"
```

Agora imagine que seja necessário trocar a mensagem para:

```text
O título deve ser informado.
```

Com mensagens hard-coded, seria necessário procurar cada ocorrência no projeto e alterá-la individualmente.

O objetivo é passar para uma estrutura centralizada:

```text
ResourceErrorMessages
        │
        ├── TITLE_REQUIRED
        ├── AMOUNT_GREATER_THAN_ZERO
        ├── EXPENSE_CANNOT_BE_IN_FUTURE
        ├── PAYMENT_TYPE_INVALID
        └── UNKNOWN_ERROR
              │
              ▼
      usado por várias classes
```

Assim, uma alteração feita em um único lugar passa a ser refletida em todos os pontos que utilizam aquela chave.

---

## 4. Resource File no .NET

Para resolver o problema, a aula introduz o **Resource File**.

No .NET, esse arquivo normalmente possui a extensão:

```text
.resx
```

Ele funciona com uma estrutura de **chave e valor**.

Exemplo conceitual:

| Chave | Valor |
|---|---|
| `TITLE_REQUIRED` | O título é obrigatório |
| `UNKNOWN_ERROR` | Erro desconhecido |

Em vez de escrever o texto diretamente no código, passa-se a acessar a chave correspondente.

Exemplo:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

Essa propriedade devolve a `string` cadastrada no Resource File.

---

## 5. Criando o ResourceErrorMessages

Como as mensagens tratadas nessa aula estão relacionadas a erros e exceções, o Resource File é criado dentro do projeto de **Exception**.

Estrutura conceitual:

```text
CashFlow.Exception
│
└── ResourceErrorMessages.resx
```

No Visual Studio, o fluxo mostrado na aula é:

```text
Projeto Exception
   → Add
   → New Item
   → Resource File
```

O arquivo recebe um nome relacionado às mensagens de erro, como:

```text
ResourceErrorMessages.resx
```

---

## 6. Modificador de acesso public

Ao criar o Resource File, a aula chama atenção para o modificador de acesso gerado.

Inicialmente ele pode estar como:

```text
internal
```

Porém, o `RegisterExpenseValidator` está no projeto **Application**.

Logo, se o recurso continuar `internal`, ele ficará acessível somente dentro do próprio projeto de Exception.

Por isso, o modificador deve ser alterado para:

```text
public
```

O raciocínio é:

```text
CashFlow.Exception
   └── ResourceErrorMessages
          │
          │ public
          ▼
CashFlow.Application
   └── RegisterExpenseValidator
```

Assim, outros projetos que possuam referência ao projeto de Exception também conseguem acessar as mensagens.

---

## 7. Estrutura chave-valor

O editor do Resource File apresenta colunas como:

- **Name**
- **Value**
- **Comment**

Na aula, o foco fica nas duas primeiras.

### Name

Representa a chave utilizada pelo código.

Exemplo:

```text
UNKNOWN_ERROR
```

### Value

Representa o texto que será retornado.

Exemplo:

```text
Erro desconhecido
```

Conceitualmente:

```text
UNKNOWN_ERROR
      │
      ▼
"Erro desconhecido"
```

As chaves não devem conter espaços, por isso a aula utiliza nomes com `_` e recomenda nomes significativos.

Exemplos:

```text
TITLE_REQUIRED
AMOUNT_GREATER_THAN_ZERO
EXPENSE_CANNOT_BE_IN_FUTURE
PAYMENT_TYPE_INVALID
UNKNOWN_ERROR
```

---

## 8. Substituindo mensagens no ExceptionFilter

A primeira mensagem movida para o Resource File é a mensagem de erro desconhecido utilizada no filtro global de exceções.

Antes:

```csharp
var errorResponse = new ResponseErrorJson("Erro desconhecido");
```

Depois:

```csharp
var errorResponse = new ResponseErrorJson(
    ResourceErrorMessages.UNKNOWN_ERROR
);
```

Com isso, o `ExceptionFilter` deixa de conhecer o texto literal da mensagem.

Ele passa apenas a solicitar o recurso correspondente.

```text
ExceptionFilter
      │
      └── ResourceErrorMessages.UNKNOWN_ERROR
                         │
                         ▼
                  "Erro desconhecido"
```

---

## 9. Substituindo mensagens no Validator

O mesmo princípio é aplicado às mensagens do `RegisterExpenseValidator`.

Antes:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage("O título é obrigatório");
```

Depois:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage(ResourceErrorMessages.TITLE_REQUIRED);
```

O padrão é repetido para as demais regras.

Exemplo conceitual:

```csharp
RuleFor(expense => expense.Amount)
    .GreaterThan(0)
    .WithMessage(ResourceErrorMessages.AMOUNT_GREATER_THAN_ZERO);

RuleFor(expense => expense.Date)
    .LessThanOrEqualTo(DateTime.UtcNow)
    .WithMessage(ResourceErrorMessages.EXPENSE_CANNOT_BE_IN_FUTURE);

RuleFor(expense => expense.PaymentType)
    .IsInEnum()
    .WithMessage(ResourceErrorMessages.PAYMENT_TYPE_INVALID);
```

Agora o Validator define **a regra**, enquanto o Resource File concentra **o texto da mensagem**.

---

## 10. Benefícios da centralização

A principal melhoria apresentada na aula é a reutilização.

### Antes

```text
Validator A → texto escrito diretamente
Validator B → mesmo texto escrito novamente
Validator C → mesmo texto escrito novamente
```

### Depois

```text
                 ResourceErrorMessages
                         │
          ┌──────────────┼──────────────┐
          ▼              ▼              ▼
     Validator A    Validator B    Validator C
```

Isso traz duas vantagens destacadas:

### Reutilização

Várias classes podem utilizar a mesma mensagem.

### Manutenção centralizada

Uma alteração no Resource File afeta todos os pontos que utilizam a mesma chave.

Exemplo:

```text
TITLE_REQUIRED

Antes:
"O título é obrigatório"

Depois:
"O título deve ser informado"
```

Não é necessário procurar cada Validator individualmente.

---

## 11. Preparação para múltiplos idiomas

No final da aula é apresentado o segundo problema das mensagens escritas diretamente no código: **idioma**.

Se a aplicação possuir usuários de diferentes países, pode ser necessário devolver a mesma mensagem em idiomas diferentes.

Exemplo:

```text
TITLE_REQUIRED
│
├── pt-BR → "O título é obrigatório"
├── en-US → "The title is required"
├── fr-FR → mensagem em francês
└── es-ES → mensagem em espanhol
```

A aula ainda não implementa essa funcionalidade.

Ela apenas antecipa que o próprio mecanismo de Resource Files será utilizado na próxima etapa para trabalhar com mensagens em diferentes idiomas.

---

## 12. Resumo

Nesta aula foi feita uma refatoração das mensagens de erro da aplicação.

O problema inicial era manter textos diretamente no código:

```csharp
.WithMessage("...")
```

Essa abordagem dificulta a reutilização e faz com que uma mesma mensagem possa ficar duplicada em várias classes.

Para corrigir isso, foi introduzido o **Resource File**, baseado em pares de chave e valor.

Exemplo:

```text
TITLE_REQUIRED = "O título é obrigatório"
```

No código, o acesso passa a ser feito por meio da chave:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

O arquivo é criado no projeto de Exception e seu modificador de acesso é alterado para `public`, permitindo que o projeto Application utilize as mensagens.

As mensagens do `ExceptionFilter` e do `RegisterExpenseValidator` deixam de ser hard-coded e passam a vir do Resource File.

Além de melhorar a reutilização e a manutenção, essa estrutura prepara a aplicação para o próximo assunto: **mensagens de erro em múltiplos idiomas**.

---

## 13. Visão Geral

```text
ANTES

RegisterExpenseValidator
 ├── "O título é obrigatório"
 ├── "O valor deve ser maior que zero"
 └── "Tipo de pagamento inválido"

ExceptionFilter
 └── "Erro desconhecido"

            ↓ refatoração

DEPOIS

ResourceErrorMessages.resx
 ├── TITLE_REQUIRED
 ├── AMOUNT_GREATER_THAN_ZERO
 ├── EXPENSE_CANNOT_BE_IN_FUTURE
 ├── PAYMENT_TYPE_INVALID
 └── UNKNOWN_ERROR
            │
            ├───────────────┐
            ▼               ▼
RegisterExpenseValidator  ExceptionFilter

            ↓

Benefícios
 ├── menos duplicação
 ├── mensagens reutilizáveis
 ├── manutenção centralizada
 └── preparação para localização/múltiplos idiomas
```
