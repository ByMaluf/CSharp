# Registro de dependências por interface e métodos de extensão: AddInfrastructure

> **Aula 99 — Configurando a DI do repositório de despesas na Infrastructure**
>
> Nesta aula, o instrutor retoma a implementação de `ExpensesRepository` (aula 98), registra a associação entre `IExpensesRepository` e a implementação concreta, resolve o problema de visibilidade causado por `internal` e ensina a criar o método de extensão `AddInfrastructure()` para simplificar o `Program.cs` da API.
>
> **Limite da gravação:** a aula configura a infraestrutura e explica o mecanismo de extensão. Ela não mostra ainda a refatoração completa do use case de registro de despesa, um teste HTTP de persistência, nem o registro do `CashFlowDbContext` por `AddDbContext`.

## Sumário

1. Objetivo e ligação com as aulas anteriores
2. Contrato e implementação existentes
3. Registro com interface e implementação
4. Por que registrar a interface?
5. O conflito de visibilidade com `internal`
6. Mantendo o encapsulamento da Infrastructure
7. Criando `DependencyInjectionExtension`
8. Recebendo `IServiceCollection`
9. Movendo o registro para a Infrastructure
10. De método estático a método de extensão
11. O papel da palavra-chave `this`
12. Resultado no `Program.cs`
13. Organização final dos arquivos e dependências
14. Revisão detalhada do fluxo de DI
15. Vantagens de modularidade e testes
16. Cuidados técnicos, exceções e complementos
17. Exemplos de revisão e perguntas
18. Mapa mental e conclusão

---

## 1. Objetivo e ligação com as aulas anteriores

O professor começa explicando que o objetivo é **registrar no serviço de injeção de dependência o repositório criado na aula anterior**.

O assunto pressupõe as aulas sobre:

- Dependency Injection (DI);
- ciclos de vida `Singleton`, `Transient` e `Scoped`;
- interfaces como contratos;
- implementação de repositórios e acesso a dados com Entity Framework Core.

### Onde estamos na sequência do CashFlow?

```text
Aula 97
IExpensesRepository (Domain)
    |
    v
Aula 98
ExpensesRepository (Infrastructure)
    |
    v
Aula 99
Registro do repositório na DI
    |
    v
Próximas etapas
Application recebe IExpensesRepository
```

A aula 98 terminou com uma classe concreta **`internal`**, localizada na Infrastructure, que implementa a interface `IExpensesRepository` do Domain.

Nesta aula, o professor precisa colocar essa implementação à disposição dos consumidores da interface — mas quer preservar a regra arquitetural de **não expor a classe concreta para a API**.

---

## 2. Contrato e implementação existentes

### O contrato na camada Domain

O repositório de despesas é representado pela interface pública `IExpensesRepository`.

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

Esse código retoma o contrato documentado na aula 97. A grafia exata do nome deve ser conferida no código-fonte efetivo do projeto caso haja diferenças em relação à transcrição oral.

### A implementação na Infrastructure

A aula 98 apresentou `ExpensesRepository`, que implementa a interface e utiliza `CashFlowDbContext`.

```csharp
internal class ExpensesRepository : IExpensesRepository
{
    public void Add(Expense expense)
    {
        var dbContext = new CashFlowDbContext();

        dbContext.Expenses.Add(expense);
        dbContext.SaveChanges();
    }
}
```

**Contexto:** essa é a implementação simplificada estudada na aula 98. A gravação atual não reescreve nem altera o corpo de `Add`.

### Separação de responsabilidades

| Elemento | Projeto | Responsabilidade |
|---|---|---|
| `Expense` | Domain | Entidade de despesa |
| `IExpensesRepository` | Domain | Contrato de persistência |
| `ExpensesRepository` | Infrastructure | Implementação concreta do contrato |
| `CashFlowDbContext` | Infrastructure | Integração com Entity Framework Core |
| `Program.cs` | API | Composição e inicialização da aplicação |

A intenção é permitir que um consumidor solicite `IExpensesRepository` e receba a implementação concreta **sem depender diretamente do nome `ExpensesRepository`**.

---

## 3. Registro com interface e implementação

O professor abre o `Program.cs` no projeto API e começa tentando registrar o serviço diretamente nele.

### Sintaxe pretendida

```csharp
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

Essa chamada utiliza a sobrecarga genérica de `AddScoped` com dois tipos.

### Interpretando os parâmetros genéricos

```csharp
AddScoped<TService, TImplementation>()
```

| Parâmetro | Significado no exemplo |
|---|---|
| `TService` | `IExpensesRepository`: serviço/contrato que um consumidor solicita |
| `TImplementation` | `ExpensesRepository`: classe que fornece a implementação |

O professor resume a operação assim: se alguém solicitar a interface, o container fornecerá **uma instância da classe indicada depois da vírgula**.

### Fluxo conceitual

```text
Consumidor solicita IExpensesRepository
                 |
                 v
Container consulta registros
                 |
                 v
IExpensesRepository -> ExpensesRepository
                 |
                 v
Container obtém ou cria ExpensesRepository
                 |
                 v
Consumidor recebe referência tipada pela interface
```

### Qual ciclo de vida foi escolhido?

O instrutor utiliza **Scoped**, explicado anteriormente na sequência de aulas.

Em uma API ASP.NET Core convencional, diferentes resoluções dentro do **mesmo escopo HTTP** podem compartilhar a mesma instância desse serviço. Um novo escopo poderá receber outra instância.

O professor comenta que gosta de usar Scoped com frequência. Essa preferência pessoal não significa que Scoped seja o ciclo de vida apropriado para absolutamente todos os serviços.

---

## 4. Por que registrar a interface?

Nas aulas de DI, o professor já havia demonstrado registros diretamente pelo tipo concreto.

### Registro apenas da classe

```csharp
builder.Services.AddScoped<ExpensesRepository>();
```

Esse formato é tecnicamente válido quando o tipo é acessível e pode ser construído.

O consumidor, nesse caso, normalmente solicita `ExpensesRepository` pelo tipo concreto.

### Registro por contrato

```csharp
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

O consumidor pode solicitar `IExpensesRepository`, sem precisar saber qual classe será utilizada.

### Dois motivos destacados na aula

**1. Modularidade e flexibilidade**

Podemos associar implementações diferentes a um mesmo contrato, conforme a configuração da aplicação. O professor relembra o exemplo anterior de upload para Google Drive, Dropbox ou outros destinos.

O código de negócio declara que precisa de um contrato; o mecanismo de composição determina a implementação a ser utilizada.

**2. Facilidade nos testes unitários**

Uma classe que depende de interface pode receber um fake ou um mock no teste, permitindo verificar seu comportamento sem necessariamente executar operações reais no banco ou em outros serviços externos.

### Por que isso reduz acoplamento?

Sem o contrato, a classe consumidora pode ficar diretamente dependente do tipo e das configurações de uma implementação específica.

Com o contrato:

```text
Regra de negócio
       |
       v
IExpensesRepository
       ^
       |
ExpensesRepository
       |
       v
Entity Framework Core / MySQL
```

O consumidor conhece **a operação oferecida pelo contrato**, não os detalhes de acesso ao MySQL.

**Observação complementar:** não é necessário criar interface para toda classe por princípio. A vantagem real depende de a abstração representar um limite útil de responsabilidade, como ocorre no repositório estudado.

---

## 5. O conflito de visibilidade com internal

Ao tentar escrever o registro no `Program.cs` da API, o professor observa um erro.

### O código que provoca o problema

```csharp
// Dentro do assembly da API:
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

Por quê?

Porque `ExpensesRepository` foi declarada **`internal`** dentro do projeto Infrastructure.

```csharp
internal class ExpensesRepository : IExpensesRepository
```

### O que significa internal nesse cenário?

A classe pode ser referenciada diretamente por código do **mesmo assembly** que a contém.

O `CashFlow.Infrastructure` pode utilizar `ExpensesRepository`, mas o assembly `CashFlow.API` não pode mencionar diretamente esse tipo com as regras usuais de acessibilidade.

```text
CashFlow.Infrastructure
      |
      +-- ExpensesRepository (internal)
      |       |
      |       +-- acessível aqui dentro
      |
      v
CashFlow.API
      |
      +-- não pode referenciar diretamente
          ExpensesRepository
```

### E se a API tiver referência ao projeto Infrastructure?

A referência entre projetos permite usar **tipos acessíveis** do assembly referenciado.

Ela não torna públicos os tipos `internal`.

**Precisão técnica:** `internal` restringe a visibilidade ao assembly, não literalmente à pasta, ao namespace nem a um projeto como conceito abstrato. No curso, cada projeto normalmente produz seu próprio assembly.

---

## 6. Mantendo o encapsulamento da Infrastructure

O professor insiste em **não alterar a implementação para `public`** apenas para conseguir registrá-la no `Program.cs`.

### Qual é a intenção?

Ele quer permitir que a API acione a configuração da infraestrutura, mas não quer que controllers e outros componentes da API façam uso direto dos detalhes de persistência.

Por exemplo, deseja evitar que código externo à Infrastructure dependa de:

```csharp
new ExpensesRepository();
```

ou da classe concreta `CashFlowDbContext`, também configurada como `internal` na aula 98.

### O problema arquitetural a resolver

Temos simultaneamente dois objetivos:

1. **Registrar o serviço** para que consumidores de `IExpensesRepository` possam recebê-lo via DI.
2. **Ocultar a implementação concreta** dos outros projetos.

### A solução anunciada

Criar um **ponto de entrada público de configuração dentro da própria Infrastructure**.

Esse ponto de entrada recebe `IServiceCollection` e registra as classes concretas, que ficam acessíveis porque o código de registro reside no mesmo assembly.

```text
API
 |
 | chama método público de configuração
 v
Infrastructure
 |
 +-- registra IExpensesRepository -> ExpensesRepository
 |                                      ^
 |                                      |
 |                                  internal
 |
 v
Container de DI configurado
```

Assim, a API deixa de mencionar diretamente `ExpensesRepository`.

---

## 7. Criando DependencyInjectionExtension

O instrutor cria uma nova classe **na raiz do projeto Infrastructure**.

Na fala, o nome se aproxima de *Dependency Injection Extension*. Neste resumo, adotamos a grafia idiomática **`DependencyInjectionExtension`**, como reconstrução coerente de C#.

### Passos descritos

1. Botão direito no projeto Infrastructure.
2. Adicionar uma nova classe.
3. Nomeá-la `DependencyInjectionExtension`.
4. Alterar sua visibilidade para `public`.
5. Declarar um método chamado `AddInfrastructure`.
6. Tornar a classe e o método **`static`**.

### Estrutura inicial, antes de receber os serviços

```csharp
namespace CashFlow.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure()
    {
    }
}
```

Essa é uma etapa intermediária do raciocínio do professor.

O método ainda não tem acesso à coleção de serviços a ser configurada. A seguir, ele recebe `IServiceCollection` como argumento.

### Por que static?

O professor não quer precisar construir uma instância da classe auxiliar:

```csharp
// Não é isso que ele pretende fazer:
var extension = new DependencyInjectionExtension();
```

Em vez disso, deseja chamar diretamente um método estático de configuração.

No C#, métodos de extensão tradicionais são declarados em uma classe estática não genérica, também estáticos, e recebem o tipo estendido no primeiro parâmetro marcado com `this`.

---

## 8. Recebendo IServiceCollection

O professor observa que `builder.Services` é do tipo `IServiceCollection`.

### Tipo e namespace

```csharp
using Microsoft.Extensions.DependencyInjection;
```

`IServiceCollection` é uma abstração utilizada no ecossistema .NET para armazenar descritores de serviços e suas configurações de injeção de dependência.

### Primeira versão do método

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(IServiceCollection services)
    {
    }
}
```

Nesse ponto, o método já pode receber a coleção de serviços da API como argumento.

### Como seria a chamada antes da sintaxe de extensão?

```csharp
DependencyInjectionExtension.AddInfrastructure(builder.Services);
```

Essa forma é uma chamada estática convencional.

O professor explica o uso de `IServiceCollection` antes de refatorar o método para que ele possa ser chamado com a sintaxe de extensão.

### E se o tipo IServiceCollection não for reconhecido?

A gravação menciona a possibilidade de ser necessário adicionar o pacote NuGet apropriado ao projeto Infrastructure.

**Distinção técnica:** primeiro deve-se verificar a referência ao pacote/assembly que fornece `IServiceCollection` e os métodos `AddScoped`, além do `using Microsoft.Extensions.DependencyInjection;`. Dependendo do SDK e das referências existentes, a dependência pode já estar disponível; não é necessário presumir que o Visual Studio sempre fará a instalação automaticamente.

O instrutor não fixa a versão de nenhum pacote na transcrição.

---

## 9. Movendo o registro para a Infrastructure

Agora o professor transfere a configuração de repositório que havia tentado realizar no `Program.cs` para dentro da nova classe estática da Infrastructure.

### Registro interno

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

Note que o código já recebe a coleção pelo parâmetro `services`.

Por isso, **não** utiliza `builder.Services` dentro da Infrastructure.

### Versão intermediária completa

```csharp
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(IServiceCollection services)
    {
        services.AddScoped<IExpensesRepository, ExpensesRepository>();
    }
}
```

Os namespaces são reconstruídos com base na organização didática das aulas anteriores. O conteúdo e a localização exatos dos `using` devem acompanhar o código do projeto.

### Por que funciona agora?

Porque o código que menciona `ExpensesRepository` está **dentro do assembly Infrastructure**, onde o tipo `internal` é acessível.

### Qual regra a API passa a seguir?

A API conhece apenas um método público de configuração.

Ela não precisa conhecer o repositório concreto usado para satisfazer `IExpensesRepository`.

```text
API
 |
 +-- conhece DependencyInjectionExtension (public)
 |
 +-- não menciona ExpensesRepository (internal)
 |
 v
Infrastructure registra a implementação
```

O professor observa que, após mover o registro, o código deixa de apresentar o erro de visibilidade.

---

## 10. De método estático a método de extensão

Depois de resolver o problema de encapsulamento, o instrutor introduz um conceito adicional: **extension methods** (métodos de extensão).

### Por que métodos de extensão existem?

Imagine que desejamos adicionar uma operação de conveniência a um tipo que já existe, sem alterar o código-fonte desse tipo.

O professor utiliza `string` para ilustrar a ideia e lembra métodos como:

- `Equals`;
- `StartsWith`;
- `EndsWith`;
- `Trim`.

Ele comenta que não podemos simplesmente abrir a implementação da biblioteca do .NET e modificar o tipo `string` para acrescentar um método próprio à linguagem.

O mecanismo de extensão permite **declarar um método estático** que pode ser invocado usando uma sintaxe semelhante à de um método de instância de outro tipo.

### Exemplo complementar com string

```csharp
public static class StringExtensions
{
    public static bool TemConteudo(this string texto)
    {
        return !string.IsNullOrWhiteSpace(texto);
    }
}
```

Uso:

```csharp
string nome = "CashFlow";

bool temConteudo = nome.TemConteudo();
```

Esse exemplo **não é um código implementado pelo professor**; serve apenas para ilustrar o mecanismo que ele explica com os métodos do tipo `string`.

### O que um método de extensão não faz?

Ele **não altera o código-fonte** da classe ou interface original.

Também não adiciona fisicamente um método à definição original do tipo.

A sintaxe é disponibilizada pelo compilador, desde que o método de extensão esteja acessível no contexto.

---

## 11. O papel da palavra-chave this

A gravação descreve três passos para criar o método de extensão.

### Passo 1 — classe estática

```csharp
public static class DependencyInjectionExtension
```

### Passo 2 — método estático

```csharp
public static void AddInfrastructure(...)
```

### Passo 3 — this no primeiro parâmetro

```csharp
this IServiceCollection services
```

A palavra reproduzida na transcrição como "BIS" corresponde, pelo contexto sintático, à palavra-chave **`this`** do C#.

### Código final da extensão

```csharp
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IExpensesRepository, ExpensesRepository>();
    }
}
```

### O que o this representa?

O primeiro parâmetro é o **receptor** da chamada feita com sintaxe de método de extensão.

Quando escrevemos:

```csharp
builder.Services.AddInfrastructure();
```

o compilador associa `builder.Services` ao primeiro parâmetro `services`.

### Equivalência conceitual

```csharp
// Sintaxe de método de extensão:
builder.Services.AddInfrastructure();

// Forma estática equivalente:
DependencyInjectionExtension.AddInfrastructure(builder.Services);
```

No caso de métodos de extensão tradicionais, essa é essencialmente uma conveniência de sintaxe para chamadas estáticas.

### Esclarecimento importante

O professor diz que `IServiceCollection` passa a receber uma função "como se já existisse" no próprio tipo.

A ideia didática está correta quanto à experiência de uso, mas **o método não passa a integrar fisicamente a interface `IServiceCollection`**.

Ele continua definido na classe estática `DependencyInjectionExtension`.

---

## 12. Resultado no Program.cs

Com o método de extensão criado, o código do projeto API fica mais simples.

### Antes: tentativa direta que falha devido a internal

```csharp
builder.Services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

### Depois: um ponto público de configuração

```csharp
using CashFlow.Infrastructure;

// ... criação de builder e demais registros

builder.Services.AddInfrastructure();
```

A extensão encapsula os registros de responsabilidade da Infrastructure.

### O que acontece quando a aplicação inicializa?

```text
Program.cs (API)
       |
       v
builder.Services.AddInfrastructure()
       |
       v
Método estático de extensão
DependencyInjectionExtension.AddInfrastructure
       |
       v
services.AddScoped<IExpensesRepository, ExpensesRepository>()
       |
       v
Registro disponível para resolução futura
```

### Por que essa organização é útil?

Conforme a aplicação crescer, a Infrastructure poderá precisar registrar outros serviços:

- repositórios adicionais;
- componentes de acesso a dados;
- serviços de infraestrutura;
- outras dependências concretas.

O método público de configuração poderá centralizar essas declarações, mantendo o `Program.cs` organizado.

**Nota:** esses exemplos representam potenciais expansões. Na gravação, o registro demonstrado é o do repositório de despesas.

---

## 13. Organização final dos arquivos e dependências

### Estrutura conceitual da solução ao final da aula

```text
CashFlow
|
+-- CashFlow.Domain
|   |
|   +-- Entities/
|   |   +-- Expense.cs
|   |
|   +-- Repositories/
|       +-- Expenses/
|           +-- IExpensesRepository.cs (public)
|
+-- CashFlow.Infrastructure
|   |
|   +-- DependencyInjectionExtension.cs (public static)
|   |
|   +-- DataAccess/
|       +-- CashFlowDbContext.cs (internal)
|       |
|       +-- Repositories/
|           +-- ExpensesRepository.cs (internal)
|
+-- CashFlow.API
    |
    +-- Program.cs
        +-- builder.Services.AddInfrastructure();
```

### Direção das dependências

```text
                 CashFlow.API
                      |
                      | referência e chamada de extensão
                      v
             CashFlow.Infrastructure
                      |
                      | implementa contrato
                      v
                CashFlow.Domain
```

A API, como ponto de composição da aplicação, pode referenciar Infrastructure para montar a aplicação.

Entretanto, os controllers e as regras de negócio não precisam conhecer diretamente a classe `ExpensesRepository`.

### Encapsulamento

| Tipo | Visibilidade | Finalidade |
|---|---|---|
| `IExpensesRepository` | `public` | Contrato consumido por outras camadas |
| `ExpensesRepository` | `internal` | Detalhe concreto da Infrastructure |
| `CashFlowDbContext` | `internal` | Detalhe de acesso a dados |
| `DependencyInjectionExtension` | `public static` | Ponto público de configuração |
| `AddInfrastructure` | `public static` | Método de extensão acessível à API |

---

## 14. Revisão detalhada do fluxo de DI

Vamos conectar a nova configuração aos conceitos das aulas anteriores.

### 14.1 Registro

Durante a inicialização, o `Program.cs` chama:

```csharp
builder.Services.AddInfrastructure();
```

A extensão registra:

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

Nesse momento, o container recebe a informação sobre a associação entre contrato, implementação e ciclo de vida.

### 14.2 Resolução futura

Suponha um componente que declare:

```csharp
public class RegisterExpenseUseCase
{
    private readonly IExpensesRepository _repository;

    public RegisterExpenseUseCase(IExpensesRepository repository)
    {
        _repository = repository;
    }
}
```

**Atenção:** este é um exemplo antecipatório. A gravação da aula 99 não mostra a refatoração do use case nem confirma esse nome exato de classe em seu código final.

Quando esse consumidor for ativado por DI, o container poderá resolver `IExpensesRepository` e fornecer `ExpensesRepository`, conforme o registro.

### 14.3 Execução do contrato

O consumidor poderá chamar:

```csharp
_repository.Add(expense);
```

O objeto concreto executará a implementação descrita na aula 98.

### 14.4 Persistência

A implementação utiliza Entity Framework Core e `SaveChanges()`.

```text
Use case
   |
   v
IExpensesRepository
   |
   v
ExpensesRepository
   |
   v
CashFlowDbContext
   |
   v
DbSet.Add()
   |
   v
SaveChanges()
   |
   v
MySQL
```

O fluxo completo representa a **arquitetura pretendida**, mas esta aula se concentra no **registro da DI**, não na realização de um novo teste de ponta a ponta.

---

## 15. Vantagens de modularidade e testes

### Modularidade

Um contrato como `IExpensesRepository` permite que o consumidor dependa das operações necessárias ao negócio, sem conhecer o repositório concreto.

A camada de infraestrutura pode ser modificada internamente, desde que continue oferecendo uma implementação adequada ao contrato.

### Substituição de implementações

É possível configurar diferentes classes para um mesmo contrato, desde que respeitem a interface.

O instrutor utiliza como analogia a escolha entre serviços de armazenamento de fotos.

**Nuance:** a configuração demonstrada associa uma interface a uma implementação. Se a aplicação precisar escolher implementações diferentes dinamicamente para cada situação, será necessária alguma política adicional de resolução ou seleção. O registro simples por si só não implementa essa escolha dinâmica.

### Testabilidade

Se o use case depende de `IExpensesRepository`, um teste pode fornecê-lo com um substituto.

```csharp
// Exemplo didático: não implementado nesta aula.
public class FakeExpensesRepository : IExpensesRepository
{
    public List<Expense> Added { get; } = new();

    public void Add(Expense expense)
    {
        Added.Add(expense);
    }
}
```

Assim, testes unitários podem verificar regras de negócio sem abrir uma conexão real com o banco.

O professor comenta que mockar classes concretas também é possível em determinados cenários, mas que prefere trabalhar com interfaces.

### Conexão com DIP

A estratégia também ajuda a aplicar o **Dependency Inversion Principle**:

- consumidores de nível mais alto dependem de abstrações;
- implementações de infraestrutura dependem dos contratos que implementam;
- os detalhes concretos podem ser conectados no ponto de composição.

DI e DIP não são sinônimos. **DI é uma técnica de fornecimento de dependências**; DIP é um princípio para estruturar relações entre componentes.

---

## 16. Cuidados técnicos, exceções e complementos

Esta seção traz esclarecimentos adicionais para não confundir as simplificações da fala com regras universais da linguagem e do framework. **Os exemplos complementares não são alterações executadas pelo instrutor nesta aula.**

### 16.1 A implementação precisa ser compatível com a interface

Na sobrecarga:

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

a classe de implementação deve satisfazer o contrato exigido.

Se `ExpensesRepository` não implementar a interface corretamente, o uso da sobrecarga genérica produz um erro de compilação relacionado às restrições de tipo.

Isso não significa que o container consegue verificar antecipadamente **todas** as falhas possíveis de construção. Dependências não registradas ou configurações incorretas ainda podem produzir erros de resolução em tempo de execução ou durante a validação do provider.

### 16.2 Uma interface não é instanciada

O container resolve `IExpensesRepository`, mas o objeto concreto é uma instância de `ExpensesRepository`.

O tipo da variável ou do parâmetro pode ser a interface.

### 16.3 Scoped significa uma instância compartilhada por escopo

O registro `AddScoped` define o ciclo de vida.

No ASP.NET Core, uma requisição HTTP normalmente dispõe de seu próprio escopo, e instâncias Scoped são compartilhadas nas resoluções desse escopo.

Isso não significa que a instância é compartilhada globalmente por toda a aplicação.

### 16.4 internal significa assembly, não proteção de segurança

`internal` não é um mecanismo de segurança para bloquear reflexão ou todo acesso indireto.

No caso, ele ajuda a estruturar o acesso ao código e impedir referências diretas aos tipos internos no código convencional de outros assemblies.

### 16.5 A extensão permite registrar tipos internos

Uma classe pública na Infrastructure pode executar a configuração porque **a chamada a `AddScoped` que menciona o tipo interno ocorre dentro do próprio assembly**.

O fato de a API chamar `AddInfrastructure` não expõe automaticamente `ExpensesRepository` como tipo público.

### 16.6 Um método de extensão continua sendo estático

O código:

```csharp
builder.Services.AddInfrastructure();
```

possui uma sintaxe familiar de chamada de instância, mas representa uma chamada ao método estático da classe de extensão.

O método `AddInfrastructure` **não é inserido fisicamente** na interface `IServiceCollection`.

### 16.7 A extensão precisa estar acessível

Para utilizar o método, o projeto consumidor precisa ter acesso ao assembly que o define, e seu namespace deve estar no escopo, por meio de `using` ou outro mecanismo apropriado.

No exemplo reconstruído:

```csharp
using CashFlow.Infrastructure;
```

### 16.8 O método de extensão não precisa obrigatoriamente retornar void

Na aula, o professor escolhe `void`, pois não precisa de um retorno para demonstrar o conceito.

Em APIs de configuração, é também comum retornar `IServiceCollection` para permitir encadeamento.

Exemplo complementar:

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
    return services;
}
```

Essa alternativa **não substitui a implementação mostrada na gravação**, que utiliza `void`.

### 16.9 A escolha do pacote NuGet depende das referências do projeto

O namespace `Microsoft.Extensions.DependencyInjection` é necessário para reconhecer os tipos e métodos utilizados.

Se o projeto Infrastructure não possuir as referências adequadas, é necessário adicioná-las. Não se deve instalar pacotes arbitrários ou pressupor que todo novo projeto precise do mesmo conjunto, independentemente do SDK.

### 16.10 A DI configurada nesta aula ainda não é uma demonstração de persistência

O vídeo não mostra um novo `POST` no Swagger nem consulta ao MySQL Workbench.

O sucesso do código sem erros no editor indica que a configuração descrita foi aceita, mas não equivale a comprovar que toda a aplicação já concluiu a persistência pelo novo fluxo.

### 16.11 Persistência e ciclo de vida do DbContext ainda precisam de atenção

Na aula 98, a implementação `ExpensesRepository.Add` cria o contexto diretamente com `new CashFlowDbContext()`.

A aula atual registra o repositório via DI, mas **não mostra a mudança do construtor do repositório para receber o DbContext por DI**.

Esse é um detalhe importante para acompanhar nas próximas etapas e para raciocinar sobre a vida útil e o descarte do contexto.

### 16.12 O método de extensão não altera interfaces existentes

O professor sugere que o mesmo mecanismo pode ser empregado com tipos como `string`, `int` e `bool`.

Isso é verdade para métodos de extensão adequadamente declarados, desde que o tipo do primeiro parâmetro corresponda ao receptor desejado.

Não precisamos modificar a implementação da biblioteca base do .NET para escrever operações auxiliares.

---

## 17. Exemplos de revisão e perguntas

### Exemplo A — registro direto pelo tipo concreto

```csharp
services.AddScoped<MeuServico>();
```

Pergunta: o consumidor pede qual tipo?

Resposta: normalmente, `MeuServico`.

### Exemplo B — registro por interface

```csharp
services.AddScoped<IMeuServico, MeuServico>();
```

Pergunta: qual tipo o consumidor deve solicitar para usar a associação?

Resposta: `IMeuServico`.

### Exemplo C — implementação interna

```csharp
internal class MeuServico : IMeuServico
{
}
```

Pergunta: por que a API não consegue citar diretamente o tipo?

Resposta: porque o código está em outro assembly, e o tipo não foi exposto publicamente.

### Exemplo D — classe de composição

```csharp
public static class MeuRegistro
{
    public static void Configurar(IServiceCollection services)
    {
        services.AddScoped<IMeuServico, MeuServico>();
    }
}
```

Pergunta: por que essa configuração pode ser compilada no assembly que contém `MeuServico`?

Resposta: porque `internal` permite o acesso ao tipo dentro do mesmo assembly.

### Exemplo E — método de extensão

```csharp
public static class MeuRegistro
{
    public static void Configurar(this IServiceCollection services)
    {
        services.AddScoped<IMeuServico, MeuServico>();
    }
}
```

Pergunta: o que mudou?

Resposta: a palavra-chave `this` no primeiro parâmetro permite chamar `Configurar()` com sintaxe de extensão sobre um `IServiceCollection`.

### Perguntas para fixação

1. Por que o professor decidiu registrar `IExpensesRepository` em vez de apenas `ExpensesRepository`?
2. Qual a função de cada tipo em `AddScoped<TService, TImplementation>()`?
3. Qual ciclo de vida foi escolhido?
4. Por que o registro direto no `Program.cs` apresenta erro?
5. Qual modificador de acesso foi mantido no repositório?
6. Por que tornar `ExpensesRepository` pública não é a solução preferida na aula?
7. Onde é criada a classe auxiliar de registro?
8. Qual a finalidade de `DependencyInjectionExtension`?
9. Qual é o tipo recebido por `AddInfrastructure`?
10. Por que a coleção de serviços é acessada pelo parâmetro `services` dentro da Infrastructure?
11. Quais três elementos são necessários à declaração tradicional de um método de extensão?
12. O que significa `this IServiceCollection services`?
13. A extensão altera o código-fonte de `IServiceCollection`?
14. Qual chamada permanece no `Program.cs` da API?
15. Que relação existe entre o registro por interface e testes unitários?
16. A refatoração do use case de registro foi concluída nesta aula?
17. A gravação demonstra uma nova operação no banco de dados?
18. Como a visibilidade `internal` se relaciona ao encapsulamento arquitetural?

---

## 18. Mapa mental e conclusão

### Resumo dos principais aprendizados

- A aula registra a implementação do repositório de despesas na DI.
- O professor escolhe `AddScoped`, de acordo com sua preferência demonstrada anteriormente.
- O registro possui **dois tipos**: contrato e implementação.
- O código desejado é `AddScoped<IExpensesRepository, ExpensesRepository>()`.
- Esse registro não pode ser escrito diretamente no `Program.cs` da API, porque a implementação é `internal`.
- O professor deseja manter a classe interna para ocultar detalhes da Infrastructure.
- A solução é criar uma classe pública auxiliar dentro da própria Infrastructure.
- Essa classe se chama, neste material, `DependencyInjectionExtension`.
- Ela define o método público e estático `AddInfrastructure`.
- O método recebe `IServiceCollection` e contém o registro.
- O professor transforma o método estático em **método de extensão**.
- Para isso, utiliza classe estática, método estático e `this` no primeiro parâmetro.
- A API passa a chamar apenas `builder.Services.AddInfrastructure();`.
- O método de extensão organiza a composição sem expor a classe concreta.
- O vídeo **não** mostra ainda o use case de registro consumindo a interface em uma execução completa.

### Quadro de arquivos

| Local | Tipo ou arquivo | Papel |
|---|---|---|
| Domain | `IExpensesRepository` | Contrato público |
| Infrastructure | `ExpensesRepository` | Implementação `internal` |
| Infrastructure | `DependencyInjectionExtension` | API pública de configuração |
| API | `Program.cs` | Chama `AddInfrastructure()` |

### Mapa mental

```text
                       AULA 99
                 INJEÇÃO DE DEPENDÊNCIA
                            |
                     Registrar repositório
                            |
          AddScoped<IExpensesRepository, ExpensesRepository>()
                 |                              |
              Interface                    Implementação
              public                         internal
                 |                              |
                 +-------------+----------------+
                               |
                    Problema no Program.cs
                      API não enxerga
                     classe internal
                               |
                               v
                       Solução no assembly
                        Infrastructure
                               |
                               v
                   DependencyInjectionExtension
                         public static
                               |
                    public static void
                    AddInfrastructure(...)
                               |
                      IServiceCollection
                               |
                        palavra this
                               |
                               v
                  Método de extensão de DI
                               |
                               v
                     Dentro da Infrastructure:
                     services.AddScoped<...>()
                               |
                               v
                       Program.cs da API
                               |
                    builder.Services
                      .AddInfrastructure()
                               |
                               v
                      DI configurada
                               |
                        Próximas aulas
                   Integração com use case
```

**Conclusão:** a aula demonstra como registrar uma interface pública e sua implementação interna na coleção de serviços sem expor os detalhes do repositório à API. A solução reúne a configuração na própria Infrastructure e oferece um método de extensão público, `AddInfrastructure()`, chamado de maneira simples por `builder.Services.AddInfrastructure()`. Assim, o ponto de composição consegue configurar a aplicação enquanto a implementação concreta permanece encapsulada.
