# Use Cases, Responsabilidade Única e Camada Application

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Responsabilidade do projeto Petfolio.Application](#responsabilidade-do-projeto-petfolioapplication)
3. [O que é uma regra de negócio](#o-que-é-uma-regra-de-negócio)
4. [Use Case](#use-case)
5. [Princípio da Responsabilidade Única](#princípio-da-responsabilidade-única)
6. [Relação com SOLID](#relação-com-solid)
7. [Uma classe por caso de uso](#uma-classe-por-caso-de-uso)
8. [Organização das pastas de Use Cases](#organização-das-pastas-de-use-cases)
9. [Classe RegisterPetUseCase](#classe-registerpetusecase)
10. [Por que o Use Case precisa ser public](#por-que-o-use-case-precisa-ser-public)
11. [Uma única função pública](#uma-única-função-pública)
12. [Método Execute](#método-execute)
13. [Entrada e saída do Use Case](#entrada-e-saída-do-use-case)
14. [Fluxo entre API e Application](#fluxo-entre-api-e-application)
15. [Instanciando e executando o Use Case](#instanciando-e-executando-o-use-case)
16. [Teste pelo Swagger](#teste-pelo-swagger)
17. [Resumo](#resumo)
18. [Visão Geral](#visão-geral)

---

## Contexto da aula

Nesta aula, o foco passa para o projeto:

```text
Petfolio.Application
```

Esse projeto foi criado anteriormente como uma biblioteca de classes responsável por concentrar as regras de negócio da aplicação.

A aula mostra como começar a estruturar essas regras de negócio por meio de classes chamadas de:

```text
Use Cases
```

> Observação: a transcrição utiliza várias vezes a palavra `patch`, mas pelo contexto do projeto `Petfolio` e das aulas anteriores, o exemplo se refere a `pet`.

---

## Responsabilidade do projeto Petfolio.Application

O projeto:

```text
Petfolio.Application
```

tem como responsabilidade principal:

```text
Conter as regras de negócio da aplicação
```

Na organização apresentada até aqui:

```text
Petfolio.API
    ↓
Petfolio.Application
    ↓
Petfolio.Communication
```

A API recebe a requisição e delega o processamento para a camada Application.

---

## O que é uma regra de negócio

Segundo a aula, uma regra de negócio contém o código responsável por validar uma requisição e tomar decisões com base nos dados recebidos.

Exemplo do cadastro de um pet:

```text
Request recebida
↓
Validar os dados
↓
Decidir o que fazer
```

Algumas ações que a regra de negócio pode executar futuramente:

```text
Enviar e-mail
Salvar em banco de dados
Gerar documento PDF
Realizar outras operações da aplicação
```

Nesta aula, porém, o foco não é criar validações complexas, persistência ou envio de e-mails.

O objetivo é entender a organização das regras de negócio dentro do projeto.

---

## Use Case

O instrutor explica que prefere chamar cada regra de negócio de:

```text
Use Case
```

ou:

```text
Caso de Uso
```

Assim, uma operação específica da aplicação recebe sua própria classe.

Exemplos:

```text
Registrar pet
Excluir pet
Atualizar pet
Buscar pet por ID
```

Cada uma dessas operações pode ser representada por um Use Case diferente.

---

## Princípio da Responsabilidade Única

A aula introduz o princípio da:

```text
Responsabilidade Única
```

A ideia aplicada aos Use Cases é:

```text
Cada classe deve representar apenas uma regra de negócio
```

Portanto, não se cria uma única classe responsável por:

```text
Registrar
Excluir
Atualizar
Buscar
```

Tudo ao mesmo tempo.

Em vez disso:

```text
RegisterPetUseCase
DeletePetUseCase
UpdatePetUseCase
GetPetByIdUseCase
```

Cada classe possui uma responsabilidade específica.

---

## Relação com SOLID

A aula apresenta o termo:

```text
SOLID
```

como um conjunto de princípios de programação.

Cada letra representa um princípio.

O princípio trabalhado nesta aula é o:

```text
S
```

que corresponde ao princípio da responsabilidade única.

Representação:

```text
SOLID
│
└── S → Single Responsibility Principle
```

Na forma como o princípio é aplicado na aula:

```text
Uma classe
↓
Uma responsabilidade principal
```

---

## Uma classe por caso de uso

A regra apresentada é:

```text
Cada Use Case representa uma operação específica
```

Por exemplo:

```text
RegisterPetUseCase
```

fica responsável apenas pelo fluxo relacionado ao cadastro de um pet.

Não deve possuir métodos públicos responsáveis por operações completamente diferentes, como:

```text
Excluir pet
Atualizar pet
Buscar pet
```

Essas operações devem possuir seus próprios Use Cases.

---

## Organização das pastas de Use Cases

Dentro de:

```text
Petfolio.Application
```

é criada a pasta:

```text
UseCases
```

Dentro dela, uma pasta para o domínio ou recurso:

```text
Pet
```

E dentro de `Pet`, uma pasta para a operação:

```text
Register
```

Estrutura apresentada:

```text
Petfolio.Application
└── UseCases
    └── Pet
        └── Register
```

Essa estrutura permite crescer futuramente para algo como:

```text
UseCases
├── Pet
│   ├── Register
│   ├── Delete
│   ├── Update
│   └── GetById
│
└── User
    ├── Register
    ├── Delete
    └── Update
```

A organização evita misturar regras de negócio diferentes no mesmo local.

---

## Classe RegisterPetUseCase

Dentro da pasta de registro é criada uma classe responsável pelo cadastro do pet.

Conceitualmente:

```csharp
public class RegisterPetUseCase
{
}
```

Ela representa:

```text
Caso de uso para registrar um pet
```

---

## Por que o Use Case precisa ser public

A classe precisa ser utilizada pelo projeto:

```text
Petfolio.API
```

Como `API` e `Application` são projetos diferentes, uma classe declarada como:

```csharp
internal
```

não ficaria acessível externamente ao projeto `Petfolio.Application`.

Por isso, a aula altera o modificador para:

```csharp
public
```

Fluxo:

```text
Petfolio.API
↓ precisa acessar
RegisterPetUseCase
↓ definido em
Petfolio.Application
```

Portanto:

```text
A classe precisa ser pública
```

---

## Uma única função pública

A aula apresenta uma convenção para os Use Cases:

```text
Cada Use Case terá apenas uma função pública
```

Essa função representa a operação principal do caso de uso.

Exemplo:

```text
RegisterPetUseCase
└── Execute()
```

O Use Case pode possuir outros métodos auxiliares, porém eles devem ser privados quando fizer sentido.

Exemplo:

```csharp
public class RegisterPetUseCase
{
    public void Execute()
    {
    }

    private void Validate()
    {
    }
}
```

Segundo a organização apresentada, não faria sentido colocar dentro do mesmo Use Case métodos públicos como:

```text
Register()
Delete()
Update()
```

porque isso quebraria a ideia de uma responsabilidade por classe.

---

## Método Execute

Como cada Use Case possui apenas uma operação pública, a aula utiliza o nome:

```text
Execute
```

A ideia é tornar a leitura intuitiva.

Exemplo:

```csharp
var useCase = new RegisterPetUseCase();
useCase.Execute();
```

Leitura conceitual:

```text
Instanciar o caso de uso
↓
Executar a regra de negócio
```

---

## Entrada e saída do Use Case

A API já recebe uma Request criada no projeto:

```text
Petfolio.Communication
```

A regra de negócio também precisa receber essa Request.

Fluxo:

```text
HTTP Request
↓
Controller
↓
RequestRegisterPetJson
↓
RegisterPetUseCase.Execute(request)
```

O método `Execute` também devolve uma Response.

Conceitualmente:

```csharp
public ResponseRegisterPetJson Execute(RequestRegisterPetJson request)
{
    return new ResponseRegisterPetJson
    {
        Id = 7,
        Name = request.Name
    };
}
```

Nesta aula, o valor do `Id` é apenas simulado.

A persistência em banco ainda não é implementada.

---

## Fluxo entre API e Application

A responsabilidade apresentada para a API é:

```text
Receber uma requisição
↓
Repassar para a regra de negócio
↓
Receber a resposta
↓
Devolver a resposta HTTP
```

Representação completa:

```text
Cliente
  │
  ▼
Petfolio.API
  │
  │ RequestRegisterPetJson
  ▼
RegisterPetUseCase
  │
  │ ResponseRegisterPetJson
  ▼
Petfolio.API
  │
  ▼
201 Created
```

A regra de negócio deixa de ficar dentro do Controller.

---

## Instanciando e executando o Use Case

No Controller, a aula instancia o Use Case manualmente.

Forma mais explícita:

```csharp
var useCase = new RegisterPetUseCase();
var response = useCase.Execute(request);
```

A aula também mostra que seria possível encadear:

```csharp
var response = new RegisterPetUseCase().Execute(request);
```

As duas formas representam a mesma sequência:

```text
Criar objeto
↓
Executar método
↓
Receber resposta
```

A forma com uma variável intermediária é mantida na aula por ser mais clara para acompanhar o fluxo.

---

## Retornando a resposta no Controller

Após a execução do Use Case:

```csharp
var response = useCase.Execute(request);
```

o Controller utiliza a resposta para retornar o resultado HTTP.

Conceitualmente:

```csharp
return Created(string.Empty, response);
```

Fluxo:

```text
Use Case
↓
ResponseRegisterPetJson
↓
Controller
↓
201 Created
```

---

## Teste pelo Swagger

A aula executa a API e testa o endpoint de cadastro pelo Swagger.

Um exemplo utilizado contém dados semelhantes a:

```text
Nome: Charlie
Tipo: Dog
Data de nascimento: 02/08/2020
```

O fluxo observado durante o debug é:

```text
Controller recebe Request
↓
RegisterPetUseCase é instanciado
↓
Execute(request) é chamado
↓
Use Case recebe os dados
↓
Use Case cria Response
↓
Controller recebe Response
↓
API retorna 201 Created
```

A resposta simulada contém:

```text
Id: 7
Name: Charlie
```

---

## Separação de responsabilidades nesta aula

A arquitetura começa a ficar mais clara:

### Petfolio.API

```text
Receber requisições HTTP
Chamar os Use Cases
Converter o resultado em resposta HTTP
```

### Petfolio.Application

```text
Conter regras de negócio
Organizar os Use Cases
Executar o processamento da aplicação
```

### Petfolio.Communication

```text
Conter Requests
Conter Responses
Conter tipos compartilhados como enums
```

Representação:

```text
            Petfolio.API
                 │
                 ▼
       Petfolio.Application
                 │
                 ▼
      Petfolio.Communication

API também utiliza Communication
para receber Requests e devolver Responses
```

---

## Resumo

| Conceito | Papel apresentado na aula |
|---|---|
| **Petfolio.Application** | Projeto responsável pelas regras de negócio |
| **Regra de negócio** | Código que valida dados e toma decisões da aplicação |
| **Use Case** | Classe que representa uma operação/regra de negócio específica |
| **Responsabilidade Única** | Cada classe deve possuir uma responsabilidade principal |
| **SOLID** | Conjunto de princípios; o `S` representa responsabilidade única |
| **RegisterPetUseCase** | Caso de uso responsável pelo cadastro de um pet |
| **public** | Necessário para que a API consiga acessar o Use Case de outro projeto |
| **Execute** | Nome utilizado para a única operação pública do Use Case |
| **Métodos privados** | Podem auxiliar internamente a regra de negócio |
| **Request** | Dados que entram no Use Case |
| **Response** | Dados devolvidos pelo Use Case |

---

## Visão Geral

```text
CLIENTE
   │
   │ HTTP Request
   ▼
┌────────────────────────────┐
│       Petfolio.API         │
│                            │
│ Controller recebe Request  │
└──────────────┬─────────────┘
               │
               │ Execute(request)
               ▼
┌────────────────────────────┐
│  Petfolio.Application      │
│                            │
│ RegisterPetUseCase         │
│                            │
│ Regra de negócio           │
└──────────────┬─────────────┘
               │
               │ ResponseRegisterPetJson
               ▼
┌────────────────────────────┐
│       Petfolio.API         │
│                            │
│ return Created(...)        │
└──────────────┬─────────────┘
               │
               ▼
          201 Created
```

### Regra principal da aula

```text
Uma operação da aplicação
↓
Um Use Case específico
↓
Uma classe com responsabilidade bem definida
↓
Uma função pública Execute
```

> **Em resumo:** a aula começa a implementar a camada `Petfolio.Application`, organizando as regras de negócio em Use Cases. Cada Use Case representa uma operação específica e segue a ideia de responsabilidade única apresentada como o `S` do SOLID. O Controller recebe a Request, instancia o Use Case, chama `Execute`, recebe uma Response e devolve o resultado HTTP. Dessa forma, a regra de negócio deixa de ficar concentrada dentro do Controller e passa para uma camada própria da aplicação.
