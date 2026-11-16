# Unit of Work no CashFlow: removendo SaveChanges dos repositórios e centralizando o Commit

> **Aula 103 — Padrão Unit of Work, consistência transacional e implementação de IUnitOfWork com Entity Framework Core**
>
> Continuação das aulas 97–102. Depois de criar a interface e a implementação do repositório, configurar a injeção de dependência, injetar o `CashFlowDbContext` e externalizar a connection string, o professor refatora **quem decide o momento de persistir as alterações no banco**.
>
> **Resultado da gravação:** o método `ExpensesRepository.Add` deixa de executar `SaveChanges()`, a interface `IUnitOfWork` e a classe `UnitOfWork` passam a oferecer `Commit()`, o caso de uso de cadastro chama essa operação, a DI é atualizada e o teste no Swagger grava uma despesa de exemplo com **Id 30**.
>
> **Nota de fidelidade:** os exemplos de código são reconstruções da implementação narrada. Quando um caminho, namespace ou detalhe literal da tela não está explícito na transcrição, ele é identificado como exemplo organizacional, não como cópia exata do projeto.

## Índice

1. Objetivo e contexto arquitetural
2. O problema de chamar SaveChanges dentro dos repositórios
3. Exemplo: salvar usuário e endereço
4. Duas chamadas SaveChanges e persistência parcial
5. Uma única chamada SaveChanges e atomicidade
6. O que é Unit of Work?
7. Relação entre Unit of Work e DbContext
8. Criando a interface IUnitOfWork no Domain
9. Implementando UnitOfWork na Infrastructure
10. Removendo SaveChanges de ExpensesRepository
11. Injetando IUnitOfWork no RegisterExpenseUseCase
12. Registrando a nova implementação na DI
13. Organização dos arquivos e código reconstruído
14. Fluxo completo de cadastro depois da refatoração
15. Teste prático com Swagger e debugger: despesa Id 30
16. Distinções técnicas sobre transações no EF Core
17. Vantagens, decisões arquiteturais e limites do padrão
18. Exercícios e perguntas de revisão
19. Resumo e mapa mental

---

## 1. Objetivo e contexto arquitetural

A aula começa com um alerta sobre uma linha existente no repositório de despesas:

```csharp
_dbContext.SaveChanges();
```

O professor afirma que essa chamada, embora funcione no cenário de cadastro atual, pode criar problemas à medida que uma operação de negócio passar a envolver **mais de uma gravação**.

### Como chegamos até aqui?

| Aula | Evolução no CashFlow |
|---|---|
| 97 | Interface `IExpensesRepository`, no Domain |
| 98 | Implementação `ExpensesRepository`, com `Add` e `SaveChanges` |
| 99 | Registro da implementação na DI com `AddInfrastructure` |
| 100 | Injeção do repositório no use case e teste real de persistência |
| 101 | `CashFlowDbContext` passa a ser injetado no repositório |
| 102 | Connection string sai do contexto e passa à configuração externa |
| **103** | **`SaveChanges` sai do repositório e passa a ser acionado através de Unit of Work** |

### A mudança de responsabilidade

**Antes:**

```text
RegisterExpenseUseCase
         |
         v
ExpensesRepository.Add(expense)
         |
         +-- DbSet.Add(expense)
         |
         +-- SaveChanges()
```

**Depois:**

```text
RegisterExpenseUseCase
         |
         +-- ExpensesRepository.Add(expense)
         |          |
         |          +-- DbSet.Add(expense)
         |
         +-- IUnitOfWork.Commit()
                    |
                    +-- DbContext.SaveChanges()
```

O caso de uso agora explicita o momento em que a operação de negócio deve tentar efetivar suas alterações.

---

## 2. O problema de chamar SaveChanges dentro dos repositórios

Até a aula 102, o método `Add` era aproximadamente:

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
    _dbContext.SaveChanges();
}
```

### Por que isso parece conveniente?

Porque cada chamada ao método `Add` já salva a entidade imediatamente no banco.

Em um caso simples, com uma única inserção, o comportamento funciona.

### Onde surge a dificuldade?

Quando uma mesma operação de negócio precisa executar **várias modificações relacionadas**.

Imagine que, após a primeira chamada a um repositório, uma segunda operação falhe.

Se a primeira já tiver executado e confirmado seu próprio `SaveChanges()`, a aplicação poderá terminar com **parte do processo persistida e outra parte não**.

### O ponto central

O problema não é que `SaveChanges` seja uma operação incorreta.

O problema é que **cada repositório está decidindo isoladamente quando efetivar as alterações**, mesmo que a unidade lógica do negócio envolva mais de um repositório.

### Separação desejada

- Repositórios: expressam e registram as modificações necessárias.
- Caso de uso: coordena a operação de negócio.
- Unit of Work: oferece o ponto de efetivação das mudanças pendentes.

Assim, o caso de uso pode preparar todas as modificações antes de solicitar sua persistência.

---

## 3. Exemplo: salvar usuário e endereço

O professor utiliza um cenário **hipotético** diferente do repositório de despesas.

Imagine um sistema em que **todo usuário deve possuir um endereço**.

Há um repositório de usuários e operações para adicionar endereços.

### Código ilustrativo do problema

```csharp
public void AddUser(User user)
{
    _dbContext.Users.Add(user);
    _dbContext.SaveChanges();
}

public void AddAddress(Address address)
{
    _dbContext.Addresses.Add(address);
    _dbContext.SaveChanges();
}
```

**Nota:** `User`, `Address`, `Users` e `Addresses` são apenas representações didáticas do exemplo oral. Essas classes e tabelas **não são criadas no CashFlow nesta aula**.

### Fluxo de negócio narrado

1. A regra de negócio recebe os dados do usuário.
2. Recebe os dados de endereço.
3. Realiza as validações iniciais.
4. Chama o repositório para adicionar o usuário.
5. Chama a operação de adicionar o endereço.
6. Considera a operação concluída somente se ambos existirem corretamente.

### Qual é a regra implícita?

O usuário não deve permanecer cadastrado sem endereço.

Portanto, cadastrar usuário e endereço é uma **unidade lógica de operação**: faz sentido que as duas gravações sejam concluídas juntas.

### Onde a inconsistência pode aparecer?

O professor propõe que os dados do endereço contenham um campo `null` cuja coluna no banco não aceita valores nulos.

Essa situação pode provocar uma exceção quando o banco tentar persistir o endereço, mesmo que verificações anteriores da Application não tenham detectado o problema.

---

## 4. Duas chamadas SaveChanges e persistência parcial

Vamos acompanhar a ordem dos eventos do exemplo.

### Primeiro: salvar o usuário

```csharp
_dbContext.Users.Add(user);
_dbContext.SaveChanges();
```

O banco grava os dados do usuário.

### Segundo: salvar o endereço

```csharp
_dbContext.Addresses.Add(address);
_dbContext.SaveChanges();
```

A segunda tentativa falha por causa de uma restrição da tabela de endereços.

### Estado resultante no exemplo

```text
Regra de negócio: cadastrar usuário + endereço
                 |
                 v
         AddUser(user)
                 |
                 v
         SaveChanges() OK
                 |
                 v
          Usuário gravado
                 |
                 v
       AddAddress(address)
                 |
                 v
        SaveChanges() FALHA
                 |
                 v
           Exceção
                 |
                 v
Banco contém usuário,
mas não contém endereço
```

Isso viola a regra de negócio exemplificada pelo professor.

### Por que a primeira gravação não desaparece automaticamente?

Porque a primeira chamada a `SaveChanges` já concluiu sua própria operação.

A falha na chamada seguinte **não desfaz automaticamente uma gravação anterior já confirmada**, quando ambas não estão coordenadas por uma transação explícita mais ampla.

### Um detalhe importante

O EF Core normalmente utiliza transação para o conjunto de mudanças executadas **dentro de uma chamada a `SaveChanges`**, quando o provider oferece suporte.

Isso **não torna duas chamadas independentes a `SaveChanges` automaticamente uma transação única**.

A fronteira em que a aplicação decide efetivar as mudanças faz diferença.

---

## 5. Uma única chamada SaveChanges e atomicidade

O professor apresenta a alternativa: registrar as duas entidades no contexto e executar `SaveChanges` **uma única vez, ao final**.

### Fluxo proposto

```csharp
_dbContext.Users.Add(user);
_dbContext.Addresses.Add(address);

// Apenas depois de preparar as alterações:
_dbContext.SaveChanges();
```

Exemplo conceitual, sem as classes e mapeamentos efetivamente criados no CashFlow.

### O que acontece?

O contexto acompanha as duas entidades pendentes.

Quando a aplicação chama `SaveChanges`, o EF Core tenta persistir o conjunto de alterações.

Em um banco relacional com suporte a transações, esse conjunto é, por padrão, tratado de maneira transacional.

### Resultado esperado

Se todas as operações de banco forem bem-sucedidas, o conjunto é efetivado.

Se ocorrer uma falha durante essa operação transacional, o conjunto de alterações da chamada não deve ficar parcialmente confirmado.

```text
Cadastrar usuário e endereço
                 |
                 v
         Users.Add(user)
                 |
                 v
       Addresses.Add(address)
                 |
                 v
        SaveChanges() único
                 |
          +------+------+
          |             |
          v             v
     Tudo certo       Algum erro
          |             |
          v             v
    Alterações       Rollback da
     confirmadas     transação
          |             |
          v             v
   Usuário e         Nenhuma das
   endereço          duas gravações
   persistidos       confirmada
```

### Como o instrutor explica internamente?

Ele menciona os conceitos:

- **Begin Transaction:** iniciar uma transação.
- **Commit:** confirmar as modificações.
- **Rollback:** desfazer alterações da transação que não pôde ser concluída.

O Entity Framework Core pode gerenciar essas etapas automaticamente em operações `SaveChanges` quando o provider suporta transações.

### A condição essencial

As alterações que devem ser salvas juntas precisam estar na **mesma unidade de trabalho**, por exemplo, rastreadas pelo **mesmo `DbContext`** antes da chamada ao método de persistência.

Sem isso, o simples fato de haver uma classe com nome `UnitOfWork` não garante atomicidade entre diferentes contextos.

---

## 6. O que é Unit of Work?

*Unit of Work*, ou **Unidade de Trabalho**, é um padrão de organização da persistência.

Seu objetivo é acompanhar um conjunto de alterações relacionadas e oferecer um ponto de coordenação para efetivá-las.

### Definição aplicada ao CashFlow

Na implementação desta aula, a abstração Unit of Work terá uma responsabilidade muito objetiva:

> Expor uma operação `Commit()` que solicita `SaveChanges()` ao contexto utilizado pelos repositórios.

### O que muda na arquitetura?

Antes, o repositório misturava o registro da alteração com a decisão de persistir imediatamente.

Depois, o caso de uso pode executar:

```csharp
_repository.Add(expense);
_unitOfWork.Commit();
```

O primeiro método registra a inclusão.

O segundo tenta efetivá-la.

### Por que chamar o método Commit?

É um nome que expressa a intenção do código consumidor: **confirmar as alterações pendentes**.

O instrutor diz que poderia usar outro nome, mas prefere `Commit`.

### Uma ressalva terminológica

O `Commit()` implementado na aula **não é uma chamada direta a `DbTransaction.Commit()`**.

Ele delega a:

```csharp
_dbContext.SaveChanges();
```

A gestão da transação necessária a essa chamada fica a cargo do EF Core e do provider, conforme seu comportamento e suporte.

---

## 7. Relação entre Unit of Work e DbContext

O padrão introduzido está diretamente relacionado ao funcionamento do EF Core.

### O DbContext já possui características de Unit of Work

O `DbContext` acompanha as entidades e as modificações pendentes ao longo de sua vida útil.

A chamada a `SaveChanges` concentra a persistência das mudanças rastreadas.

Nesse sentido, ele **já atua como uma implementação de unidade de trabalho** no próprio EF Core.

### Então por que criar outra classe?

A classe `UnitOfWork` do professor funciona como uma **abstração da aplicação para o momento de persistência**.

Ela permite que a Application utilize:

```csharp
IUnitOfWork
```

sem receber diretamente:

```csharp
CashFlowDbContext
```

Isso mantém o use case desacoplado do tipo concreto do EF Core.

### Visão de responsabilidades

| Elemento | Responsabilidade |
|---|---|
| `DbContext` | Rastrear entidades e executar persistência com EF Core |
| `IExpensesRepository` | Declarar operações de repositório |
| `ExpensesRepository` | Registrar alterações de despesas no contexto |
| `IUnitOfWork` | Declarar a operação para efetivar as alterações |
| `UnitOfWork` | Chamar `SaveChanges` no contexto |
| `RegisterExpenseUseCase` | Coordenar o cadastro e escolher o momento do commit |

### Isso é obrigatório em todo projeto?

Não.

Usar diretamente `DbContext` em determinado desenho de aplicação pode ser legítimo, e uma abstração adicional pode ou não trazer valor.

**A decisão do curso** é utilizar interfaces nas camadas superiores e encapsular os detalhes concretos do EF Core na Infrastructure. Nesse contexto, `IUnitOfWork` se encaixa na arquitetura escolhida.

---

## 8. Criando a interface IUnitOfWork no Domain

Na parte prática, o professor volta ao Visual Studio e cria uma nova interface.

Ele a coloca na área de repositórios do projeto Domain.

### Nome escolhido

```csharp
IUnitOfWork
```

O prefixo `I` segue a convenção de interfaces apresentada na aula 97.

### Visibilidade

O instrutor altera o tipo para `public`, permitindo que a Application utilize o contrato.

### Assinatura definida

```csharp
void Commit();
```

### Código reconstruído

**Arquivo conceitual:** `CashFlow.Domain/Repositories/IUnitOfWork.cs`

```csharp
namespace CashFlow.Domain.Repositories;

public interface IUnitOfWork
{
    void Commit();
}
```

### O que a interface define?

Somente a operação de efetivação.

A assinatura **não revela**:

- qual banco de dados será utilizado;
- qual provider do EF Core executará os comandos;
- como o `DbContext` será construído;
- se há uma transação explícita criada pela aplicação;
- quantas entidades estão pendentes de gravação.

### Por que o retorno é void?

O professor decide que, nesta etapa, a aplicação não precisa receber um resultado retornado por `Commit`.

A implementação pode ignorar o valor inteiro que `SaveChanges()` devolve.

Uma interface diferente, em outra aplicação, poderia expor um resultado ou usar métodos assíncronos; **isso não é acrescentado nesta aula**.

---

## 9. Implementando UnitOfWork na Infrastructure

Depois de criar o contrato no Domain, o professor cria a implementação concreta na Infrastructure.

Ele comenta que a classe pode ser colocada na organização de repositórios ou diretamente na área de acesso a dados.

### Nome da classe

```csharp
UnitOfWork
```

### Visibilidade

A classe permanece como `internal`, assim como `ExpensesRepository`.

Isso preserva o encapsulamento de detalhes de infraestrutura.

### Implementação da interface

```csharp
internal class UnitOfWork : IUnitOfWork
```

O Visual Studio oferece a correção **Implement Interface**, utilizada pelo professor para criar o método `Commit`.

### Injeção do contexto

A classe recebe `CashFlowDbContext` pelo construtor, seguindo o mesmo padrão da aula 101:

```csharp
private readonly CashFlowDbContext _dbContext;

public UnitOfWork(CashFlowDbContext dbContext)
{
    _dbContext = dbContext;
}
```

### Implementação de Commit

```csharp
public void Commit()
{
    _dbContext.SaveChanges();
}
```

O professor também demonstra a forma compacta de corpo de expressão:

```csharp
public void Commit() => _dbContext.SaveChanges();
```

Ambas representam a intenção da gravação: chamar `SaveChanges` ao executar `Commit`.

### Código completo reconstruído

**Arquivo ilustrativo:** `CashFlow.Infrastructure/DataAccess/UnitOfWork.cs`

```csharp
using CashFlow.Domain.Repositories;

namespace CashFlow.Infrastructure.DataAccess;

internal class UnitOfWork : IUnitOfWork
{
    private readonly CashFlowDbContext _dbContext;

    public UnitOfWork(CashFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Commit()
    {
        _dbContext.SaveChanges();
    }
}
```

**Nota de localização:** a transcrição permite confirmar que a classe é criada dentro da Infrastructure e que o professor considera a pasta de repositórios ou a área `DataAccess`; o caminho acima é uma organização didática plausível, e não a reprodução literal de uma árvore mostrada em texto.

---

## 10. Removendo SaveChanges de ExpensesRepository

Com `UnitOfWork` criado, o instrutor retorna ao repositório de despesas.

### Antes

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
    _dbContext.SaveChanges();
}
```

### Depois

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
}
```

### Qual linha foi removida?

```csharp
_dbContext.SaveChanges();
```

### O que o Add faz agora?

O método adiciona a entidade ao conjunto rastreado pelo `CashFlowDbContext`.

Ele **não solicita a gravação imediata** da alteração.

### Por que isso é intencional?

Porque a decisão de persistir é deslocada para o componente que coordena o fluxo de negócio.

```text
ExpensesRepository.Add(expense)
              |
              v
_dbContext.Expenses.Add(expense)
              |
              v
  Alteração pendente no contexto
              |
              v
      Aguarda Commit
```

### O contrato do repositório muda?

Não.

A interface `IExpensesRepository` continua oferecendo:

```csharp
void Add(Expense expense);
```

A mudança ocorre **na implementação**, não na assinatura pública do contrato.

### Por que isso reforça o desacoplamento?

O código consumidor não precisa saber se o repositório é implementado por EF Core, por um banco diferente ou por outro mecanismo.

Ele só precisa saber que `Add` registra uma despesa, e que a unidade de trabalho é responsável por solicitar a efetivação das operações.

---

## 11. Injetando IUnitOfWork no RegisterExpenseUseCase

Depois de remover `SaveChanges` do repositório, a Application precisa chamar o novo serviço.

O professor abre o caso de uso de cadastro de despesas e acrescenta a interface `IUnitOfWork` às dependências do construtor.

### Dependências existentes

Até então:

```csharp
private readonly IExpensesRepository _repository;
```

### Nova dependência

```csharp
private readonly IUnitOfWork _unitOfWork;
```

### Construtor reconstruído

```csharp
public RegisterExpenseUseCase(
    IExpensesRepository repository,
    IUnitOfWork unitOfWork)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
}
```

### Por que receber as duas interfaces?

Cada uma possui responsabilidade distinta:

- `IExpensesRepository`: registrar operações de despesas no contexto.
- `IUnitOfWork`: solicitar a gravação das operações pendentes.

### Alteração no Execute

Depois de validar a requisição e criar a entidade `Expense`, a operação passa a executar:

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

### Fluxo completo em alto nível

```text
Execute(request)
       |
       v
Validar dados
       |
       v
Criar Expense
       |
       v
_repository.Add(entity)
       |
       v
_unitOfWork.Commit()
       |
       v
Finalizar caso de uso
```

### Por que a chamada deve ocorrer depois de Add?

Porque `Commit` precisa ser solicitado **depois que as alterações desejadas foram registradas no contexto**.

O caso de uso estabelece a ordem correta das operações.

### Observação técnica

`Commit` é uma responsabilidade explícita do caso de uso nesta arquitetura.

Uma eventual exceção durante `SaveChanges` deve ser tratada ou propagada segundo as regras de erro já definidas para a aplicação; a gravação não introduz uma nova estratégia de tratamento.

---

## 12. Registrando a nova implementação na DI

O professor lembra que, antes de testar, precisa registrar a interface e sua implementação no container.

Ele abre a classe `DependencyInjectionExtension` da Infrastructure, estudada nas aulas 99–102.

### Registro já existente

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

### Novo registro

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Por que usar AddScoped?

O professor segue a configuração de repositórios e do contexto apresentada anteriormente.

Na API ASP.NET Core típica, `AddDbContext<CashFlowDbContext>()` utiliza `Scoped` por padrão.

Registrar também `IUnitOfWork` e `IExpensesRepository` como Scoped permite que **as dependências resolvidas no mesmo escopo** recebam a mesma instância Scoped de `CashFlowDbContext`.

Esse detalhe é fundamental para o comportamento pretendido.

### Como fica o método auxiliar AddRepositories

```csharp
private static void AddRepositories(IServiceCollection services)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}
```

O nome `AddRepositories` foi preservado pelo professor na gravação, mesmo registrando também a unidade de trabalho nesse método.

### De onde vem a classe UnitOfWork?

Ela está na Infrastructure e permanece `internal`.

O registro é realizado dentro do mesmo assembly, que pode acessar a classe.

### De onde vem IUnitOfWork?

A interface está no Domain e é pública.

A Application pode solicitar o serviço sem conhecer sua implementação.

---

## 13. Organização dos arquivos e código reconstruído

Esta seção reúne as alterações para facilitar a revisão do conteúdo da aula.

### 13.1 Estrutura conceitual

```text
CashFlow
├── CashFlow.Domain
│   └── Repositories
│       ├── IUnitOfWork.cs
│       └── Expenses
│           └── IExpensesRepository.cs
│
├── CashFlow.Application
│   └── UseCases
│       └── Expenses
│           └── Register
│               ├── IRegisterExpenseUseCase.cs
│               ├── RegisterExpenseUseCase.cs
│               └── RegisterExpenseValidator.cs
│
├── CashFlow.Infrastructure
│   ├── DependencyInjectionExtension.cs
│   └── DataAccess
│       ├── CashFlowDbContext.cs
│       ├── UnitOfWork.cs
│       └── Repositories
│           └── ExpensesRepository.cs
│
└── CashFlow.API
    └── Program.cs
```

O nome dos diretórios intermediários da Application e o local exato do arquivo `UnitOfWork.cs` são ilustrativos. A gravação confirma as camadas, os contratos, as classes e as responsabilidades.

### 13.2 Domain: IUnitOfWork

```csharp
namespace CashFlow.Domain.Repositories;

public interface IUnitOfWork
{
    void Commit();
}
```

### 13.3 Infrastructure: UnitOfWork

```csharp
using CashFlow.Domain.Repositories;

namespace CashFlow.Infrastructure.DataAccess;

internal class UnitOfWork : IUnitOfWork
{
    private readonly CashFlowDbContext _dbContext;

    public UnitOfWork(CashFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Commit()
    {
        _dbContext.SaveChanges();
    }
}
```

### 13.4 Infrastructure: ExpensesRepository

```csharp
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories.Expenses;

namespace CashFlow.Infrastructure.DataAccess.Repositories;

internal class ExpensesRepository : IExpensesRepository
{
    private readonly CashFlowDbContext _dbContext;

    public ExpensesRepository(CashFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Expense expense)
    {
        _dbContext.Expenses.Add(expense);
    }
}
```

### 13.5 Infrastructure: registros da DI

Dentro do arquivo `DependencyInjectionExtension.cs`, a configuração do contexto da aula 102 permanece e é adicionado o novo registro.

```csharp
using CashFlow.Domain.Repositories;
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Infrastructure.DataAccess;
using CashFlow.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

// Dentro da classe estática DependencyInjectionExtension:

private static void AddRepositories(IServiceCollection services)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();
}
```

O código dessa seção é um **trecho do método** e pressupõe a classe de extensão já definida na aula 99. A configuração `AddDbContext` da aula 102 não é reescrita aqui.

### 13.6 Application: injeção das dependências

```csharp
private readonly IExpensesRepository _repository;
private readonly IUnitOfWork _unitOfWork;

public RegisterExpenseUseCase(
    IExpensesRepository repository,
    IUnitOfWork unitOfWork)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
}
```

### 13.7 Application: persistência após o mapeamento

```csharp
// Depois de validar a requisição e construir a entidade:

_repository.Add(entity);
_unitOfWork.Commit();
```

As outras etapas do método `Execute` continuam seguindo a implementação desenvolvida nas aulas anteriores, incluindo a validação, a construção da entidade e a resposta do caso de uso.

### 13.8 Program.cs

A API continua chamando as extensões já estabelecidas:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

A mudança da aula 103 fica **dentro da configuração de Infrastructure**, onde o novo serviço é registrado. A gravação não demonstra uma nova alteração nessas duas chamadas do `Program.cs`.

---

## 14. Fluxo completo de cadastro depois da refatoração

Com todos os componentes registrados, a sequência passa a ser:

```text
Swagger / HTTP POST
        |
        v
ExpensesController
        |
        | [FromServices]
        v
IRegisterExpenseUseCase
        |
        v
RegisterExpenseUseCase
        |
        +-- IExpensesRepository
        |        |
        |        v
        |   ExpensesRepository
        |        |
        |        +-- CashFlowDbContext (Scoped)
        |
        +-- IUnitOfWork
                 |
                 v
             UnitOfWork
                 |
                 +-- CashFlowDbContext (Scoped)
                           |
                           v
                    MESMA INSTÂNCIA
                    NO MESMO ESCOPO
                           |
                           v
  1. Validate(request)
  2. Criar Expense
  3. _repository.Add(entity)
  4. _unitOfWork.Commit()
                           |
                           v
                      SaveChanges()
                           |
                           v
                         MySQL
```

### Por que a mesma instância importa?

Se `ExpensesRepository` registrar as alterações em um contexto e `UnitOfWork` chamar `SaveChanges` em **outro contexto independente**, as alterações registradas pelo primeiro não estarão automaticamente pendentes no segundo.

### O que garante o compartilhamento no projeto demonstrado?

A combinação de:

- `services.AddDbContext<CashFlowDbContext>(...)`, usando o ciclo de vida Scoped padrão;
- `services.AddScoped<IExpensesRepository, ExpensesRepository>()`;
- `services.AddScoped<IUnitOfWork, UnitOfWork>()`;
- resolução de ambos dentro do mesmo escopo de requisição.

### Por que essa arquitetura facilita extensões?

Se um caso de uso futuro precisar registrar duas entidades em repositórios diferentes, ambos poderão registrar alterações no mesmo `DbContext` e o caso de uso poderá solicitar um único `Commit` após a preparação das duas operações.

Esse é o cenário que motivou a refatoração.

---

## 15. Teste prático com Swagger e debugger: despesa Id 30

Após alterar a interface, a classe de unidade de trabalho, o repositório, o use case e a DI, o professor testa a API.

### Preparação

Ele utiliza o Visual Studio e posiciona breakpoints para acompanhar o fluxo.

Depois inicia a aplicação e abre o Swagger.

### Dados mencionados

| Campo | Valor narrado |
|---|---|
| Título | Teclado / Keyboard |
| Descrição | Texto de exemplo não especificado |
| Valor | 50 |
| Tipo de pagamento | 2 |
| Data | Não detalhada na transcrição |

### Passo a passo da demonstração

1. Abrir o endpoint de cadastro de despesas.
2. Clicar em **Try it out**.
3. Informar os dados da nova despesa.
4. Executar a requisição.
5. A execução entra no use case.
6. A validação é executada com sucesso.
7. A entidade `Expense` é criada.
8. O use case chama o repositório.
9. `ExpensesRepository.Add` apenas adiciona a entidade ao contexto.
10. O fluxo retorna ao use case.
11. O use case chama `_unitOfWork.Commit()`.
12. O depurador entra no método de `UnitOfWork`.
13. `UnitOfWork.Commit` executa `_dbContext.SaveChanges()`.
14. O professor inspeciona a entidade e observa o identificador gerado.

### Resultado

A despesa recebe **Id 30**.

O instrutor conclui que o salvamento continua funcionando após mover `SaveChanges` para a nova classe.

### O que foi efetivamente verificado?

O vídeo mostra, através da depuração, que:

- o repositório registra a entidade;
- o caso de uso invoca `Commit`;
- o código de `UnitOfWork` executa `SaveChanges`;
- a entidade recebe o identificador gerado.

A gravação **não apresenta um teste automatizado de rollback do cenário usuário/endereço**, nem uma consulta independente ao banco para esta despesa. O exemplo de consistência transacional é a justificativa conceitual; o teste prático mostrado é um cadastro simples.

---

## 16. Distinções técnicas sobre transações no EF Core

Esta seção complementa a explicação do professor para evitar conclusões excessivamente amplas. **Os cenários adicionais não são implementados no vídeo.**

### 16.1 SaveChanges normalmente já é transacional

Para providers relacionais com suporte a transações, uma chamada a `SaveChanges` normalmente persiste suas modificações dentro de uma transação.

Isso oferece atomicidade para as alterações daquela chamada: ou o conjunto de comandos da operação é confirmado, ou as mudanças da transação são revertidas em caso de falha.

Essa garantia depende do provider, do banco e do contexto transacional efetivo.

### 16.2 Um único SaveChanges não é sinônimo de uma transação distribuída

O comportamento transacional da chamada cobre as operações de banco incluídas naquela unidade de persistência.

Ele **não confirma ou desfaz automaticamente** operações que aconteceram em outros sistemas, como:

- envio de e-mail;
- chamadas a APIs externas;
- gravações em outro banco de dados independente;
- publicação de mensagens em uma fila sem integração transacional.

Esses cenários exigem estratégias específicas de consistência.

### 16.3 Vários SaveChanges separados não viram uma transação só por usar a mesma interface

Se o caso de uso chama `Commit` duas vezes, cada chamada a `SaveChanges` poderá ter sua própria fronteira de transação.

Uma falha posterior não desfaz necessariamente aquilo que já foi confirmado na chamada anterior.

### 16.4 É possível coordenar transações manualmente

O professor menciona que há maneiras de iniciar e gerenciar transações explicitamente.

O EF Core oferece APIs para esse tipo de controle, mas **não é necessário usá-las para implementar o fluxo mostrado na aula**.

A aula não cria manualmente uma transação com `BeginTransaction`, `Commit` ou `Rollback`.

### 16.5 Compartilhar DbContext é condição importante para o wrapper demonstrado

`ExpensesRepository` e `UnitOfWork` recebem `CashFlowDbContext` por DI.

Com `DbContext` registrado como Scoped e ambos resolvidos no mesmo escopo, o serviço `UnitOfWork` pode persistir as alterações rastreadas pelo repositório.

Se eles utilizarem contextos distintos, o wrapper simples não agregará automaticamente as alterações de ambos.

### 16.6 DbContext já implementa conceitos de Unit of Work

O objeto do EF Core acompanha mudanças e dispõe de `SaveChanges`.

O wrapper `IUnitOfWork` é uma **decisão arquitetural adicional** para impedir que a Application conheça diretamente esse objeto e para tornar o ponto de persistência explícito no caso de uso.

### 16.7 Add não executa imediatamente o INSERT na demonstração

A chamada:

```csharp
_dbContext.Expenses.Add(expense);
```

marca a entidade para inserção no rastreamento do EF Core.

A gravação efetiva é solicitada por:

```csharp
_dbContext.SaveChanges();
```

O fluxo novo mantém essa distinção, agora em classes diferentes.

### 16.8 Falhas antes e durante Commit têm consequências diferentes

**Falha antes do Commit:** as alterações podem permanecer apenas pendentes no contexto. Se o escopo for encerrado sem salvar, elas não serão persistidas pelo código demonstrado.

**Falha durante o Commit:** a transação padrão do provider relacional, quando aplicável, protege o conjunto de alterações dessa chamada.

**Falha depois de um Commit bem-sucedido:** não implica rollback automático do que já foi confirmado.

Essa distinção é essencial para interpretar corretamente o exemplo de usuário e endereço.

### 16.9 Não é preciso criar um novo DbContext dentro de UnitOfWork

O objetivo é justamente usar o mesmo `CashFlowDbContext` registrado na DI.

Criar uma nova instância manualmente dentro de `Commit` faria a implementação perder a referência às alterações rastreadas anteriormente pelo repositório.

### 16.10 A interface Commit é síncrona neste momento

```csharp
void Commit();
```

O método utiliza `SaveChanges()`.

Em aplicações que utilizam operações de banco assíncronas, uma interface como `Task CommitAsync(CancellationToken cancellationToken)` pode ser adequada, implementada com `SaveChangesAsync`.

**Essa alternativa não é introduzida na aula.** O resumo preserva o método síncrono exibido pelo professor.

### 16.11 Commit não garante que todas as regras de negócio estejam corretas

A gravação concentra a discussão em persistência consistente.

Ainda assim, a Application continua responsável por validar as regras necessárias antes de tentar salvar.

Uma transação protege a atomicidade das modificações incluídas nela; não substitui validação nem torna automaticamente corretas todas as regras de negócio.

### 16.12 O banco pode aplicar restrições além das validações da aplicação

O exemplo utiliza uma coluna que não aceita `null`.

Essa restrição é imposta pelo banco e pode causar falha de persistência mesmo depois de uma validação preliminar na Application.

Isso explica por que a integridade transacional é importante mesmo quando existem validadores.

---

## 17. Vantagens, decisões arquiteturais e limites do padrão

### Vantagem 1 — Um ponto explícito de efetivação

O caso de uso define quando o conjunto de alterações deve ser persistido.

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

### Vantagem 2 — Repositórios focados em sua responsabilidade

`ExpensesRepository` deixa de decidir quando ocorre `SaveChanges`.

Ele passa a registrar a operação sobre a entidade.

### Vantagem 3 — Preparação para fluxos com várias alterações

Uma operação futura pode interagir com repositórios distintos e solicitar a efetivação depois de preparar todas as mudanças.

### Vantagem 4 — Separação entre aplicação e EF Core

A Application utiliza `IUnitOfWork`, não `CashFlowDbContext`.

Os detalhes de persistência permanecem na Infrastructure.

### Vantagem 5 — Facilidade para testar o fluxo do caso de uso

Com `IExpensesRepository` e `IUnitOfWork` injetados, testes unitários podem utilizar substitutos para verificar:

- se o caso de uso solicitou a operação `Add`;
- se solicitou `Commit` quando apropriado;
- se deixou de chamar `Commit` quando uma validação falhou.

Os testes correspondentes não são implementados nesta gravação.

### Custo e limite

O professor reconhece que, no cadastro atual, a mudança acrescenta uma dependência e uma chamada a mais no caso de uso.

Para uma operação que grava somente uma despesa, isso pode parecer desnecessário.

A justificativa é estabelecer uma estrutura preparada para operações que exijam persistência coordenada.

**Nota técnica:** o padrão não deve ser aplicado mecanicamente em qualquer projeto. Seu valor depende das necessidades reais de consistência, das abstrações já existentes e da forma de organizar transações.

---

## 18. Exercícios e perguntas de revisão

### Exercício 1 — Localize a fronteira de persistência

Considere:

```csharp
repository.Add(expense);
unitOfWork.Commit();
```

**Pergunta:** qual linha solicita a gravação ao banco?

**Resposta esperada:** `unitOfWork.Commit()`, cuja implementação chama `SaveChanges()`.

### Exercício 2 — Identifique a inconsistência

Considere uma operação que exige usuário e endereço:

```csharp
SaveUser(user);        // chama SaveChanges()
SaveAddress(address);  // chama SaveChanges() e falha
```

**Pergunta:** o que pode acontecer?

**Resposta esperada:** o usuário pode permanecer salvo enquanto o endereço não foi persistido, caso as chamadas não estejam coordenadas por uma transação mais abrangente.

### Exercício 3 — Explique o motivo do Scoped

Considere os registros:

```csharp
services.AddDbContext<CashFlowDbContext>(...);
services.AddScoped<IExpensesRepository, ExpensesRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Pergunta:** por que o ciclo de vida do contexto importa?

**Resposta esperada:** porque o repositório e a unidade de trabalho devem operar sobre o mesmo contexto dentro do escopo, de modo que o `Commit` persista as alterações rastreadas pelo repositório.

### Exercício 4 — Reescreva o repositório

**Antes:**

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
    _dbContext.SaveChanges();
}
```

**Depois esperado:**

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
}
```

A chamada a `SaveChanges` fica encapsulada em `UnitOfWork.Commit`.

### Perguntas adicionais

1. Qual problema futuro o professor identifica na implementação da aula 102?
2. Por que `SaveChanges` dentro de cada repositório pode criar persistências parciais?
3. Qual regra de negócio é usada no exemplo de usuário e endereço?
4. Como uma coluna que não aceita `null` pode causar a falha da segunda gravação?
5. O que significa atomicidade de uma transação?
6. Qual é a diferença entre `Commit` e `Rollback`?
7. O EF Core pode gerenciar automaticamente a transação de uma chamada a `SaveChanges`?
8. A transação de uma chamada posterior desfaz automaticamente uma chamada anterior já concluída?
9. Onde o contrato `IUnitOfWork` é criado?
10. Qual método a interface `IUnitOfWork` declara?
11. Em qual camada fica a classe concreta `UnitOfWork`?
12. Por que ela é mantida como `internal`?
13. Qual dependência é recebida pelo construtor de `UnitOfWork`?
14. O que `UnitOfWork.Commit` faz internamente?
15. Qual linha foi removida de `ExpensesRepository.Add`?
16. Quais interfaces passam a ser injetadas em `RegisterExpenseUseCase`?
17. Por que `Commit` deve ser chamado após registrar as alterações?
18. Qual registro de DI foi acrescentado à Infrastructure?
19. O que assegura que os dois serviços obtenham o mesmo `DbContext`?
20. Qual despesa foi usada no Swagger e qual foi seu Id após a gravação?
21. O teste mostra um rollback de duas entidades?
22. O `DbContext` já possui características próprias de unidade de trabalho?
23. Em que situações é necessário um controle transacional mais amplo?

---

## 19. Resumo e mapa mental

### O que o professor efetivamente realizou

- Identificou a chamada `SaveChanges` dentro de `ExpensesRepository` como um potencial ponto de inconsistência em operações futuras.
- Explicou o exemplo hipotético de usuário e endereço persistidos em momentos diferentes.
- Introduziu o conceito de **Unit of Work**.
- Criou no Domain a interface pública **`IUnitOfWork`**, com **`void Commit();`**.
- Criou na Infrastructure a classe interna **`UnitOfWork`**, implementando a interface.
- Injetou `CashFlowDbContext` em `UnitOfWork`.
- Implementou **`Commit`** chamando **`_dbContext.SaveChanges()`**.
- Removeu a chamada a `SaveChanges` de `ExpensesRepository.Add`.
- Injetou `IUnitOfWork` em `RegisterExpenseUseCase`.
- Adicionou a chamada **`_unitOfWork.Commit()`** após **`_repository.Add(entity)`**.
- Registrou **`services.AddScoped<IUnitOfWork, UnitOfWork>()`** na Infrastructure.
- Testou o cadastro pelo Swagger com uma despesa **Teclado / Keyboard**, valor **50**, tipo de pagamento **2**.
- Confirmou no debugger o identificador **30** depois do commit.
- Encerrrou mostrando que o novo ponto de persistência funciona e prepara o CashFlow para operações mais complexas.

### Antes e depois

| Aspecto | Antes da aula 103 | Depois da aula 103 |
|---|---|---|
| `ExpensesRepository.Add` | `DbSet.Add` + `SaveChanges` | Apenas `DbSet.Add` |
| Ponto de persistência | Dentro do repositório | `UnitOfWork.Commit` |
| Contrato de unidade de trabalho | Não existia | `IUnitOfWork` |
| Implementação concreta | Não existia | `UnitOfWork`, `internal` |
| Dependências do use case | `IExpensesRepository` | `IExpensesRepository` + `IUnitOfWork` |
| DI de Unit of Work | Ausente | `AddScoped<IUnitOfWork, UnitOfWork>()` |
| `CashFlowDbContext` | Scoped por `AddDbContext` | Continua Scoped |
| Persistência | Uma despesa por vez | Uma despesa, mas com fronteira de commit explícita |
| Cenário de múltiplas alterações | Possibilidade de SaveChanges espalhados | Preparação para SaveChanges coordenado |

### Mapa mental

```text
                              AULA 103
                             UNIT OF WORK
                                  |
                        PROBLEMA ANTERIOR
                                  |
                        ExpensesRepository
                                  |
                       Add + SaveChanges
                                  |
                                  v
                  Múltiplos repositórios podem
                   confirmar alterações cedo
                                  |
                                  v
                    Exemplo: usuário + endereço
                       /                 \
                      /                   \
              Usuário salvo        Endereço falha
                      \                   /
                       \                 /
                        Banco inconsistente
                                  |
                                  v
                           NOVA ESTRUTURA
                                  |
                    +-------------+-------------+
                    |                           |
                  Domain                   Infrastructure
                    |                           |
                IUnitOfWork                  UnitOfWork
                    |                           |
               void Commit()              internal class
                    |                           |
                    +------ implementado por ---+
                                                |
                                        CashFlowDbContext
                                                |
                                          SaveChanges()
                                                |
                                                v
                                         Commit na regra
                                          de negócio
                                                |
                          +---------------------+------------------+
                          |                                        |
                  ExpensesRepository                      IUnitOfWork
                          |                                        |
                      Add(entity)                             Commit()
                          |                                        |
                          +---------------------+------------------+
                                                |
                                                v
                                           MySQL
                                                |
                                         Expense Id = 30
```

**Conclusão:** a aula 103 apresenta o padrão **Unit of Work** como uma forma de centralizar o momento da persistência e reduzir o risco de operações de negócio parcialmente gravadas. O CashFlow passa a registrar a entidade pelo repositório e a solicitar `SaveChanges` separadamente por `IUnitOfWork.Commit()`. O teste manual confirma que o cadastro continua funcionando, com uma despesa de exemplo recebendo **Id 30**. A atomicidade em operações mais complexas dependerá de as alterações participarem da mesma unidade de trabalho e das garantias transacionais do provider utilizado.
