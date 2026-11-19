# Aula 106 — Refatoração assíncrona do CashFlow com Task, async, await e Entity Framework Core

> **Sequência:** após as aulas 104 e 105 sobre Tasks, o instrutor aplica programação assíncrona ao fluxo de cadastro de despesas do CashFlow. A alteração começa no repositório e no Unit of Work, segue pelas interfaces e pelo use case e termina no controller da API.
>
> **Demonstração da gravação:** cadastro pelo Swagger de uma despesa com título **Comida**, valor **15** e tipo de pagamento **0**. No depurador, o professor observa o identificador **31** depois da persistência.
>
> **Fidelidade:** os exemplos a seguir reconstroem os trechos descritos oralmente. A gravação não mostra todo o código preexistente de validação, mapeamento ou formatação da resposta. As observações específicas sobre as APIs do EF Core são complementos técnicos e estão identificadas como tal.

## Índice

1. Objetivo e ligação com aulas anteriores
2. O que muda no fluxo completo
3. Antes da refatoração: código síncrono
4. Atualizando a interface IExpensesRepository
5. Atualizando a implementação ExpensesRepository
6. Atualizando IUnitOfWork e UnitOfWork
7. Alterando o método Execute do use case
8. Atualizando a interface IRegisterExpenseUseCase
9. Tornando o controller assíncrono
10. Código consolidado por camada
11. Por que await deve percorrer a cadeia?
12. Diferença entre AddAsync e SaveChangesAsync
13. Task, Task<T> e ValueTask<T>
14. Teste no Swagger e análise do Id 31
15. Impacto na DI e no ciclo de vida Scoped
16. Cuidados com concorrência, erros e cancelamento
17. Erros comuns e correções técnicas
18. Exercícios e perguntas de revisão
19. Resumo e mapa mental

---

## 1. Objetivo e ligação com aulas anteriores

Nas aulas **104** e **105**, o instrutor explicou o que são `Task`, `Task<T>`, `async` e `await`, além de apresentar os conceitos de ThreadPool e os riscos de `async void`.

Agora, o objetivo é **aplicar esses fundamentos ao código real do CashFlow**.

A refatoração preserva a arquitetura construída anteriormente:

- **Domain:** contratos `IExpensesRepository` e `IUnitOfWork`.
- **Infrastructure:** implementações `ExpensesRepository` e `UnitOfWork`, utilizando `CashFlowDbContext`.
- **Application:** `IRegisterExpenseUseCase` e `RegisterExpenseUseCase`.
- **API:** controller responsável pelo cadastro de despesas.

A aula **103** havia separado a preparação da inserção do momento de efetivar as alterações:

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

Essas operações agora passam a ser chamadas assincronamente:

```csharp
await _repository.Add(entity);
await _unitOfWork.Commit();
```

O comportamento funcional desejado continua sendo cadastrar uma despesa; a alteração é no **modelo de execução e propagação da conclusão**.

## 2. O que muda no fluxo completo

A mudança de um método para retorno `Task` tem consequências sobre quem o chama.

| Componente | Antes | Depois |
|---|---|---|
| `IExpensesRepository.Add` | `void` | `Task` |
| `ExpensesRepository.Add` | `DbSet.Add` | `async Task` e `await DbSet.AddAsync` |
| `IUnitOfWork.Commit` | `void` | `Task` |
| `UnitOfWork.Commit` | `SaveChanges` | `async Task` e `await SaveChangesAsync` |
| `IRegisterExpenseUseCase.Execute` | Tipo de resposta | `Task<TipoDeResposta>` |
| `RegisterExpenseUseCase.Execute` | Retorno síncrono | `async Task<TipoDeResposta>` |
| Controller | `IActionResult` | `async Task<IActionResult>` |

O encadeamento resultante é:

```text
POST de despesa
    |
    v
ExpensesController
    | await Execute(request)
    v
RegisterExpenseUseCase
    | await _repository.Add(entity)
    v
ExpensesRepository
    | await DbSet<Expense>.AddAsync(entity)
    v
Change Tracker do CashFlowDbContext

RegisterExpenseUseCase
    | await _unitOfWork.Commit()
    v
UnitOfWork
    | await DbContext.SaveChangesAsync()
    v
MySQL
    |
    v
Resposta ao cliente
```

**Observação:** o repositório continua responsável por adicionar a entidade ao contexto, enquanto o Unit of Work continua responsável pela efetivação, conforme a divisão introduzida na aula 103.

## 3. Antes da refatoração: código síncrono

O contrato do repositório era:

```csharp
public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

Sua implementação:

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
}
```

O contrato de unidade de trabalho era:

```csharp
public interface IUnitOfWork
{
    void Commit();
}
```

E a implementação efetivava as alterações:

```csharp
public void Commit()
{
    _dbContext.SaveChanges();
}
```

No caso de uso, as chamadas eram síncronas e a operação `SaveChanges()` poderia esperar o servidor MySQL usando a thread chamadora durante o acesso bloqueante ao banco.

**Objetivo técnico:** utilizar a API assíncrona do EF Core para que a espera por I/O possa ocorrer sem bloquear desnecessariamente uma thread durante todo o período.

## 4. Atualizando a interface IExpensesRepository

O professor altera a interface na camada Domain, substituindo o retorno `void` por `Task`.

**Antes:**

```csharp
void Add(Expense expense);
```

**Depois:**

```csharp
Task Add(Expense expense);
```

Declaração reconstruída:

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    Task Add(Expense expense);
}
```

### Por que a interface não usa async?

A interface declara o contrato e não possui o corpo da operação demonstrada. Ela informa apenas que `Add` produzirá uma `Task`.

O modificador `async` é utilizado no método concreto **quando seu corpo contém `await`**, não na declaração abstrata do contrato.

### Por que Task e não Task<Expense>?

O método não tem a finalidade de devolver a entidade. Seu contrato precisa apenas representar a **conclusão da operação de registro**.

A assinatura continua com o nome `Add`, mesmo que a implementação utilize internamente `AddAsync`.

## 5. Atualizando a implementação ExpensesRepository

Na Infrastructure, o instrutor substitui a chamada síncrona do `DbSet` pela operação `AddAsync`.

**Antes:**

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
}
```

**Depois:**

```csharp
public async Task Add(Expense expense)
{
    await _dbContext.Expenses.AddAsync(expense);
}
```

Mudanças realizadas:

1. `void` passa a `Task`;
2. o método concreto recebe `async`;
3. `_dbContext.Expenses.Add` passa a `_dbContext.Expenses.AddAsync`;
4. a nova chamada é aguardada com `await`.

A operação **não executa `SaveChangesAsync`**. Essa responsabilidade permanece na unidade de trabalho.

### Por que ocorre erro na interface?

A classe implementa `IExpensesRepository`. Se a classe retorna `Task` e a interface ainda declara `void`, os contratos não coincidem. O professor acessa a interface e corrige sua assinatura.

## 6. Atualizando IUnitOfWork e UnitOfWork

O mesmo processo é aplicado ao Unit of Work.

**Interface antes:**

```csharp
public interface IUnitOfWork
{
    void Commit();
}
```

**Interface depois:**

```csharp
public interface IUnitOfWork
{
    Task Commit();
}
```

**Implementação antes:**

```csharp
public void Commit()
{
    _dbContext.SaveChanges();
}
```

**Implementação depois:**

```csharp
public async Task Commit()
{
    await _dbContext.SaveChangesAsync();
}
```

### O que SaveChangesAsync faz?

Ele solicita ao EF Core que persista assincronamente as alterações pendentes daquele contexto.

No provider relacional, isso envolve envio de comandos e espera por operações de I/O com o banco.

### O retorno da API do EF Core é idêntico ao retorno do UnitOfWork?

Não exatamente. O método `DbContext.SaveChangesAsync()` retorna `Task<int>`. O valor inteiro representa a quantidade de entradas de estado gravadas conforme a API do EF Core.

O código mostrado pelo professor **ignora esse inteiro** e expõe `Task` ao restante da aplicação, o que é válido.

## 7. Alterando o método Execute do use case

O professor passa para o projeto Application.

A classe `RegisterExpenseUseCase` executa a validação, constrói a despesa e chama o repositório e a unidade de trabalho.

Como esse método devolve um objeto de resposta, sua nova assinatura deve utilizar `Task<T>`.

**Antes, em formato conceitual:**

```csharp
public ResponseRegisterExpenseJson Execute(
    RequestRegisterExpenseJson request)
```

**Depois:**

```csharp
public async Task<ResponseRegisterExpenseJson> Execute(
    RequestRegisterExpenseJson request)
```

Os nomes de DTO acima representam o fluxo já documentado nas aulas anteriores; a transcrição atual não apresenta integralmente sua grafia no editor.

### Novas chamadas

```csharp
await _repository.Add(entity);
await _unitOfWork.Commit();
```

O `await` da primeira chamada garante que a etapa de registro assíncrono tenha concluído antes de solicitar o commit.

O `await` da segunda garante que o caso de uso acompanhe o término da persistência antes de devolver sua resposta.

### Por que não retornar apenas Task?

Porque `Execute` tem um resultado para o controller. `Task<ResponseRegisterExpenseJson>` representa a conclusão **e** o objeto de resposta.

## 8. Atualizando IRegisterExpenseUseCase

A interface pública do caso de uso também precisa refletir a assinatura assíncrona.

**Antes:**

```csharp
public interface IRegisterExpenseUseCase
{
    ResponseRegisterExpenseJson Execute(
        RequestRegisterExpenseJson request);
}
```

**Depois:**

```csharp
public interface IRegisterExpenseUseCase
{
    Task<ResponseRegisterExpenseJson> Execute(
        RequestRegisterExpenseJson request);
}
```

Novamente, **não é necessário declarar `async` na interface**.

### Por que essa mudança importa?

O controller depende de `IRegisterExpenseUseCase`, não necessariamente da implementação concreta. É por meio da interface que ele precisa conhecer a operação assíncrona e seu resultado.

## 9. Tornando o controller assíncrono

A última alteração acontece no projeto API.

A action HTTP retorna um `IActionResult`. Como agora chamará `await useCase.Execute(request)`, ela passa a retornar `Task<IActionResult>`.

### Mudança na assinatura

**Antes:**

```csharp
public IActionResult Register(...)
```

**Depois:**

```csharp
public async Task<IActionResult> Register(...)
```

### Mudança na execução

**Antes:**

```csharp
var response = useCase.Execute(request);
```

**Depois:**

```csharp
var response = await useCase.Execute(request);
```

### Estrutura ilustrativa

```csharp
[HttpPost]
public async Task<IActionResult> Register(
    [FromBody] RequestRegisterExpenseJson request,
    [FromServices] IRegisterExpenseUseCase useCase)
{
    var response = await useCase.Execute(request);

    return Created(string.Empty, response);
}
```

**Nota de fidelidade:** o nome literal da action e os detalhes de construção do HTTP 201 não são apresentados integralmente na transcrição da aula 106. Esse exemplo demonstra a forma correta das alterações de `async Task<IActionResult>` e `await`, preservando o padrão de `[FromServices]` das aulas anteriores.

## 10. Código consolidado por camada

### Domain — IExpensesRepository

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    Task Add(Expense expense);
}
```

### Domain — IUnitOfWork

```csharp
namespace CashFlow.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
}
```

### Infrastructure — ExpensesRepository

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

    public async Task Add(Expense expense)
    {
        await _dbContext.Expenses.AddAsync(expense);
    }
}
```

### Infrastructure — UnitOfWork

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

    public async Task Commit()
    {
        await _dbContext.SaveChangesAsync();
    }
}
```

### Application — IRegisterExpenseUseCase

```csharp
public interface IRegisterExpenseUseCase
{
    Task<ResponseRegisterExpenseJson> Execute(
        RequestRegisterExpenseJson request);
}
```

### Application — trechos alterados do RegisterExpenseUseCase

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

// Dentro de Execute:
// 1. Validar a request (implementação anterior).
// 2. Criar a entidade Expense (implementação anterior).
// 3. Aguardar as operações abaixo:

await _repository.Add(entity);
await _unitOfWork.Commit();

// 4. Produzir o DTO de resposta já existente.
```

**Esse último bloco é um conjunto de trechos de referência, não um arquivo compilável completo.** Os passos de validação, mapeamento e resposta devem permanecer exatamente como foram implementados nas aulas anteriores.

### API — trecho alterado da action

```csharp
public async Task<IActionResult> Register(
    [FromBody] RequestRegisterExpenseJson request,
    [FromServices] IRegisterExpenseUseCase useCase)
{
    var response = await useCase.Execute(request);
    return Created(string.Empty, response);
}
```

## 11. Por que await deve percorrer a cadeia?

Uma chamada que retorna Task pode terminar depois que o método que a chamou recuperou o controle.

Por isso, a Application e a API precisam **coordenar a conclusão** das operações.

```text
Ação HTTP
   |
   | await Execute
   v
Caso de uso
   |
   | await Add
   v
Repositório
   |
   | await AddAsync
   v
Change Tracker do EF Core

Caso de uso
   |
   | await Commit
   v
UnitOfWork
   |
   | await SaveChangesAsync
   v
Banco de dados
```

### E se faltar await?

Quando uma operação essencial é iniciada sem que sua Task seja aguardada ou retornada adequadamente, o código pode continuar antes da sua conclusão.

Isso também dificulta a observação de falhas da operação.

### Await bloqueia a thread?

Não necessariamente. Em operações assíncronas de I/O, a continuação do método pode ficar suspensa, liberando a thread enquanto o serviço externo processa a requisição.

Essa é uma das conexões principais com as aulas 104 e 105.

## 12. Diferença entre AddAsync e SaveChangesAsync

Esta é uma **distinção técnica importante**, não desenvolvida em detalhes pelo professor nessa gravação.

### DbSet.Add / DbSet.AddAsync

Esses métodos normalmente **registram a entidade no Change Tracker** e marcam seu estado para inserção.

Eles não significam, por si só, que foi executado um INSERT na tabela.

### DbContext.SaveChanges / SaveChangesAsync

Esses métodos solicitam a persistência das alterações pendentes no banco.

### Quando AddAsync é necessário?

Na maioria dos cenários comuns de entidades com chaves geradas por bancos relacionais, `DbSet.Add` já é suficiente e não executa I/O relevante.

A documentação do EF Core ressalta que `AddAsync` existe especialmente para permitir **geradores de valor assíncronos** que possam precisar acessar o banco, como certas estratégias de geração de chaves (por exemplo, HiLo).

Assim, é incorreto concluir que **trocar `Add` por `AddAsync` sempre gera ganho de performance**.

### Onde o ganho de assincronia tende a ser mais relevante?

Na persistência com `SaveChangesAsync` e em consultas que executam I/O, pois essas operações normalmente aguardam o banco de dados.

### Tabela

| Operação | Responsabilidade | I/O de banco normalmente? |
|---|---|---|
| `DbSet.Add(entity)` | Marcar entidade como Added | Não |
| `DbSet.AddAsync(entity)` | Adicionar com suporte a geração de valores assíncrona | Normalmente não; depende do gerador |
| `DbContext.SaveChanges()` | Persistir alterações | Sim, síncrono |
| `DbContext.SaveChangesAsync()` | Persistir alterações | Sim, assíncrono |

### Importante

**A aula implementa `AddAsync`**, e é essa a mudança preservada neste resumo.

A distinção acima serve para compreender tecnicamente por que esse método existe e onde os benefícios assíncronos são realmente esperados.

## 13. Task, Task<T> e ValueTask<T>

### Task

Representa uma operação aguardável sem valor de retorno diretamente exposto ao chamador.

Exemplo do curso:

```csharp
Task Commit();
```

### Task<T>

Representa uma operação aguardável que produz um valor do tipo `T`.

Exemplo:

```csharp
Task<ResponseRegisterExpenseJson> Execute(
    RequestRegisterExpenseJson request);
```

### ValueTask<T>

O EF Core usa um tipo semelhante na operação de adição:

```csharp
ValueTask<EntityEntry<Expense>>
```

Esse é o retorno típico de `DbSet<Expense>.AddAsync(expense)`.

O código do professor não precisa trabalhar diretamente com `EntityEntry<Expense>`, pois apenas aguarda a operação.

### O retorno da implementação do repositório é compatível?

Sim:

```csharp
public async Task Add(Expense expense)
{
    await _dbContext.Expenses.AddAsync(expense);
}
```

A implementação aguarda o `ValueTask<EntityEntry<Expense>>` e termina sem devolver um valor próprio. Para o consumidor, o retorno é apenas `Task`.

## 14. Teste no Swagger e análise do Id 31

Com os contratos e as implementações atualizados, o professor adiciona breakpoints e executa a API pelo Visual Studio.

### Dados do teste

| Campo | Valor mencionado |
|---|---|
| Título | Comida |
| Descrição | Texto de teste, não especificado literalmente |
| Valor | 15 |
| Tipo de pagamento | 0 |
| Data | Não informada na transcrição |

### Sequência observada

1. O professor inicia a API com F5.
2. Abre o Swagger.
3. Seleciona o endpoint de cadastro.
4. Utiliza **Try it out**.
5. Preenche a despesa de exemplo.
6. Executa a requisição.
7. O depurador entra no fluxo do controller e do use case.
8. A validação é realizada.
9. A entidade é construída.
10. O use case aguarda a operação do repositório.
11. O use case aguarda a operação da Unit of Work.
12. A entidade aparece com identificador **31**.
13. A execução retorna ao controller e continua.

### O que foi comprovado?

O código **continua persistindo a entidade no fluxo demonstrado**, agora com as operações aguardadas de forma assíncrona.

### O que a aula não demonstrou?

- Nenhum benchmark comparando versões síncrona e assíncrona.
- Nenhuma medição de CPU, threads ou throughput.
- Nenhum teste automatizado novo.
- Nenhuma alteração das configurações do MySQL.
- Nenhuma nova regra de negócio para o cadastro.

O professor mostra uma verificação manual por Swagger e depuração.

### Por que o ID é 31?

O número 31 é o identificador observado nesse teste específico, após as inserções anteriores do curso.

Não é um valor fixo esperado em qualquer ambiente nem deve aparecer hardcoded no código da API.

## 15. Impacto na DI e no ciclo de vida Scoped

A refatoração **não exige trocar os serviços já registrados na injeção de dependência**.

Os registros continuam conceitualmente:

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

O contexto continua sendo registrado por `AddDbContext<CashFlowDbContext>(...)` com opções de MySQL, conforme a aula 102.

### O que muda?

As assinaturas e o código assíncrono dos métodos.

### O que não muda?

- A interface continua sendo o contrato injetado.
- O repositório permanece como implementação de Infrastructure.
- A Unit of Work continua centralizando o commit.
- O container continua resolvendo o grafo de dependências.
- O padrão Scoped continua relevante para compartilhar o mesmo contexto no escopo da requisição.

## 16. Cuidados com concorrência, erros e cancelamento

Esta seção é um complemento técnico para aprofundar a implementação.

### DbContext não é thread-safe

Um `DbContext` não pode executar operações concorrentes simultâneas de maneira segura.

**Usar métodos assíncronos não torna um contexto thread-safe**.

Portanto, para o mesmo contexto, não se deve iniciar múltiplas operações de EF Core em paralelo sem aguardar a conclusão apropriada de cada uma.

### Por que o código sequencial é adequado?

```csharp
await _repository.Add(entity);
await _unitOfWork.Commit();
```

Esse fluxo exige que uma etapa termine antes de começar a próxima.

### Erros de banco chegam ao chamador?

Sim. Quando `SaveChangesAsync` falha, a Task pode terminar com erro.

O `await` na Unit of Work, no use case e no controller permite a propagação da exceção, segundo a política de tratamento de erros da aplicação.

### E se o cliente cancelar a requisição?

Uma melhoria comum é propagar um `CancellationToken` até as operações de EF Core.

Por exemplo, uma assinatura alternativa:

```csharp
Task Commit(CancellationToken cancellationToken);
```

E sua implementação:

```csharp
public async Task Commit(CancellationToken cancellationToken)
{
    await _dbContext.SaveChangesAsync(cancellationToken);
}
```

**Essa melhoria não é realizada pelo professor nesta aula.** O código registrado mantém as assinaturas simples com `Task`.

### Unit of Work e transações

Trocar `SaveChanges` por `SaveChangesAsync` não elimina as garantias transacionais do EF Core aplicáveis a uma operação de persistência.

Mas a atomicidade continua limitada às alterações e à transação efetivamente envolvidas. O conceito de Unit of Work estudado na aula 103 permanece necessário para compreender a fronteira de commit.

## 17. Erros comuns e correções técnicas

### Erro 1 — colocar async na interface

Para um contrato simples sem implementação:

```csharp
Task Add(Expense expense);
```

Não é necessário escrever `async` na declaração.

### Erro 2 — retornar void após chamar AddAsync

Se a operação precisa ser aguardada pelo consumidor, `void` não representa corretamente sua conclusão.

O professor utiliza `Task`.

### Erro 3 — esquecer de alterar a interface quando a implementação muda

A classe concreta precisa satisfazer a assinatura contratada.

Se a interface continuar com `void Add` enquanto a implementação passar a `Task Add`, o compilador sinalizará incompatibilidade.

### Erro 4 — fazer uma chamada assíncrona sem await

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

Depois da refatoração, ambas as operações retornam Tasks. Ignorá-las pode prejudicar a ordem de execução e a observação de erros.

### Erro 5 — usar Task quando existe uma resposta

Para `Execute`, o retorno deve carregar o tipo da resposta:

```csharp
Task<ResponseRegisterExpenseJson>
```

Não apenas `Task`, pois o controller precisa do DTO retornado.

### Erro 6 — usar IActionResult em método que contém await

O método do controller demonstrado passa a retornar:

```csharp
Task<IActionResult>
```

e recebe `async` para permitir o uso de `await` no corpo.

### Erro 7 — supor que AddAsync já salvou no MySQL

`AddAsync` normalmente registra a entidade no contexto; `SaveChangesAsync` é responsável por persistir as alterações.

### Erro 8 — acreditar que async significa uma nova thread

O mecanismo de `async/await` permite suspender a continuação e aguardar operações de I/O sem necessariamente criar threads adicionais.

### Erro 9 — achar que a mudança garante melhor desempenho por requisição

A vantagem mais comum de I/O assíncrono em servidores é aproveitar melhor as threads sob concorrência.

O tempo de uma requisição isolada pode continuar semelhante. A gravação não contém medições de performance.

### Erro 10 — renomear tudo sem necessidade

O curso mantém os nomes `Add`, `Commit` e `Execute`. Embora o sufixo `Async` seja uma convenção recomendada em muitos códigos C#, a transcrição não mostra essa renomeação.

O resumo preserva a implementação mostrada e explica a convenção separadamente.

## 18. Exercícios e perguntas de revisão

### Exercício 1 — corrigir um contrato

Dada a interface:

```csharp
public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

Transforme a operação em aguardável sem alterar seu nome.

**Resposta:**

```csharp
Task Add(Expense expense);
```

### Exercício 2 — corrigir a implementação

Dado:

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
}
```

Reescreva conforme a gravação.

**Resposta:**

```csharp
public async Task Add(Expense expense)
{
    await _dbContext.Expenses.AddAsync(expense);
}
```

### Exercício 3 — persistência

Dado:

```csharp
public void Commit()
{
    _dbContext.SaveChanges();
}
```

Converta para a versão assíncrona.

**Resposta:**

```csharp
public async Task Commit()
{
    await _dbContext.SaveChangesAsync();
}
```

### Exercício 4 — resultado assíncrono

Um método devolve `ResponseRegisterExpenseJson` e precisa utilizar `await`.

Qual deve ser o retorno?

**Resposta:** `Task<ResponseRegisterExpenseJson>`.

### Perguntas adicionais

1. Qual é a finalidade desta refatoração?
2. Qual é a diferença entre `Task` e `Task<T>`?
3. Por que a interface do repositório não precisa da palavra-chave `async`?
4. Qual método do EF Core o professor utiliza para adicionar uma entidade?
5. O repositório passa a executar `SaveChangesAsync`?
6. Onde fica a responsabilidade de persistir no banco?
7. Qual tipo `SaveChangesAsync` retorna na API do EF Core?
8. Por que `UnitOfWork.Commit` pode retornar simplesmente `Task`?
9. Quais duas operações o use case precisa aguardar?
10. Por que o controller precisa passar a retornar `Task<IActionResult>`?
11. O que acontece se `Execute` for chamado sem `await` na action?
12. Qual foi o título da despesa criada no teste?
13. Qual valor e tipo de pagamento foram utilizados?
14. Qual identificador foi observado depois da persistência?
15. Trocar `DbSet.Add` por `AddAsync` sempre representa ganho de performance?
16. Por que `SaveChangesAsync` costuma ser mais relevante para I/O?
17. A refatoração altera os registros Scoped da DI?
18. `DbContext` passa a permitir operações paralelas sobre a mesma instância?
19. Qual a relação desta aula com o padrão Unit of Work?
20. Quais alterações futuras poderiam permitir cancelamento da operação?

## 19. Resumo e mapa mental

### O que o professor implementou

- Substituiu `IExpensesRepository.Add` de `void` para `Task`.
- Utilizou `async Task` e `await _dbContext.Expenses.AddAsync(expense)` na implementação.
- Alterou `IUnitOfWork.Commit` para `Task`.
- Passou a utilizar `await _dbContext.SaveChangesAsync()` na unidade de trabalho.
- Atualizou o retorno de `RegisterExpenseUseCase.Execute` para `Task<ResponseRegisterExpenseJson>`.
- Acrescentou `await` nas chamadas do repositório e do Unit of Work.
- Atualizou o contrato `IRegisterExpenseUseCase` para o mesmo retorno assíncrono.
- Tornou a action HTTP `async Task<IActionResult>`.
- Passou a usar `await useCase.Execute(request)` no controller.
- Testou no Swagger uma despesa **Comida**, de **15**, pagamento **0**, com **Id 31** observado no depurador.

### Comparação final

| Etapa | Código anterior | Código refatorado |
|---|---|---|
| Interface de repositório | `void Add(...)` | `Task Add(...)` |
| Repositório | `Expenses.Add(...)` | `await Expenses.AddAsync(...)` |
| Interface Unit of Work | `void Commit()` | `Task Commit()` |
| Unit of Work | `SaveChanges()` | `await SaveChangesAsync()` |
| Use case | `Response... Execute(...)` | `Task<Response...> Execute(...)` |
| Chamada do repositório | `_repository.Add(entity)` | `await _repository.Add(entity)` |
| Chamada do commit | `_unitOfWork.Commit()` | `await _unitOfWork.Commit()` |
| Controller | `IActionResult` | `Task<IActionResult>` |

### Mapa mental

```text
                              AULA 106
                        ASYNC / AWAIT NO CASHFLOW
                                   |
                +------------------+-------------------+
                |                                      |
            INFRASTRUCTURE                           DOMAIN
                |                                      |
          ExpensesRepository                     IExpensesRepository
                |                                      |
      await DbSet.AddAsync()                       Task Add(...)
                |                                      |
          UnitOfWork                             IUnitOfWork
                |                                      |
      await SaveChangesAsync()                    Task Commit()
                |
                +------------------+-------------------+
                                   |
                               APPLICATION
                                   |
                    RegisterExpenseUseCase
                                   |
                    async Task<Response...>
                                   |
                     await _repository.Add
                                   |
                     await _unitOfWork.Commit
                                   |
                                   v
                                  API
                                   |
                      async Task<IActionResult>
                                   |
                      await useCase.Execute
                                   |
                                   v
                              Swagger / MySQL
                                   |
                       Comida, valor 15, pagamento 0
                                   |
                                   v
                                 Id 31
```

**Conclusão:** a aula 106 conclui a primeira refatoração assíncrona do fluxo de cadastro de despesas. Interfaces, implementações, caso de uso e controller passam a trabalhar com `Task`, `Task<T>` e `await`. A responsabilidade da Unit of Work permanece intacta, mas agora a persistência é feita com `SaveChangesAsync`. O teste manual no Swagger demonstra que a operação continua funcionando, com a despesa recebendo o identificador **31**.
