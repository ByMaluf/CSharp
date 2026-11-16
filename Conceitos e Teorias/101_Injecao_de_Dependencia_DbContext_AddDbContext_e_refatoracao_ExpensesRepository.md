# Refatorando o repositório com AddDbContext e injeção de CashFlowDbContext

> **Aula 101 — Registro do DbContext na DI, organização da Infrastructure e injeção pelo construtor**
>
> Continuação da aula 100, na qual o cadastro de despesas passou a funcionar através de `IRegisterExpenseUseCase`, `IExpensesRepository` e do container de injeção de dependência. A gravação agora aprimora **como o repositório obtém o contexto do Entity Framework Core**, deixando de construí-lo manualmente com `new`.
>
> **Escopo da aula:** são criados métodos privados de organização dentro de `DependencyInjectionExtension`, registrado `CashFlowDbContext` com `AddDbContext` e refatorado `ExpensesRepository` para recebê-lo no construtor. O teste manual retorna uma entidade com **Id 28** após `SaveChanges`. **A connection string ainda permanece no `OnConfiguring` e será tratada na próxima aula.**

## Índice

1. Objetivo e contexto
2. Problema da criação manual do contexto
3. Por que vários repositórios podem precisar de um mesmo DbContext?
4. Preparando a extensão da Infrastructure
5. Separando o registro dos repositórios
6. Criando o método privado AddDbContext
7. Entendendo services.AddDbContext<CashFlowDbContext>()
8. Refatorando o construtor de ExpensesRepository
9. Campo private readonly e remoção do new
10. Comparação de código: antes e depois
11. Como o container resolve as dependências agora?
12. Teste no Swagger e no debugger
13. O significado do Id 0 antes e do Id 28 depois
14. O que permanece para a próxima aula?
15. Boas práticas e distinções técnicas
16. Resumo, perguntas de revisão e mapa mental

---

## 1. Objetivo e contexto

O professor relembra que a API já conseguiu persistir uma despesa real no MySQL na aula 100, depois da integração da injeção de dependência entre as camadas.

Entretanto, ainda existem oportunidades de refatoração relacionadas a **organização de código, manutenção e gerenciamento de recursos**.

### Situação antes desta aula

Na aula 98, a implementação concreta de repositório foi escrita de maneira semelhante a:

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

Esse código funciona no cenário mostrado, mas cria o `DbContext` dentro do método `Add`.

### Qual é a melhoria pretendida?

O repositório deverá:

- receber o `CashFlowDbContext` por **injeção no construtor**;
- usar a mesma referência nos seus métodos;
- não ficar responsável por instanciar o contexto;
- permitir que o container de DI determine o tempo de vida do objeto.

A configuração passa a existir em `CashFlow.Infrastructure`, preservando a separação arquitetural já construída.

---

## 2. Problema da criação manual do contexto

O professor aponta diretamente a linha:

```csharp
var dbContext = new CashFlowDbContext();
```

Em seguida, pergunta por que é melhor evitar essa criação dentro do próprio repositório.

### Exemplo apresentado

Imagine uma regra de negócio que precise utilizar, numa mesma operação, dois repositórios:

- um para despesas;
- outro, futuramente, para receitas.

Se cada repositório construir seu próprio contexto, teremos instâncias separadas.

```text
Regra de negócio
      |
      +-- Repositório de despesas
      |       |
      |       +-- new CashFlowDbContext() -> Contexto A
      |
      +-- Repositório de receitas
              |
              +-- new CashFlowDbContext() -> Contexto B
```

O professor argumenta que isso pode aumentar o número de objetos criados e ocupar mais memória.

### Qual é a motivação principal?

Permitir que o tempo de vida do contexto seja **gerenciado de forma centralizada**, em vez de cada repositório decidir individualmente quando criar a própria instância.

Nesse desenho, duas classes de repositório que dependam do mesmo `CashFlowDbContext` e sejam resolvidas **no mesmo escopo de DI** poderão compartilhar a mesma instância.

### Uma precisão importante

**Ter dois `DbContext` diferentes não é, por si só, um erro ou um problema de performance comprovado.**

Há situações em que diferentes unidades de trabalho requerem contextos separados. O benefício da refatoração não deve ser reduzido a “usar menos memória”: ele envolve principalmente **gerenciamento do ciclo de vida, unidade de trabalho e composição de dependências**.

O professor usa o cenário como motivação didática. O projeto mostrado ainda não possui um repositório de receitas implementado nesta gravação.

---

## 3. Por que vários repositórios podem precisar de um mesmo DbContext?

O `DbContext` é o objeto do EF Core que reúne recursos e comportamentos relacionados à persistência.

Entre outras responsabilidades, ele permite:

- consultar conjuntos de entidades;
- acompanhar entidades rastreadas pelo contexto;
- registrar inserções, atualizações e exclusões pendentes;
- aplicar as alterações por `SaveChanges`.

### Quando dois repositórios usam o mesmo contexto

```text
Requisição HTTP
      |
      v
Escopo de DI
      |
      +-- ExpensesRepository ----+
      |                          |
      +-- IncomeRepository ------+--> CashFlowDbContext
                                        |
                                        v
                                  MySQL
```

`IncomeRepository` representa apenas o cenário hipotético descrito pelo professor. Ele **não é criado nesta aula**.

### Por que isso pode ser útil?

Quando os repositórios participam da mesma unidade de trabalho, usar o mesmo contexto permite que trabalhem com o mesmo mecanismo de acompanhamento das entidades.

Isso também pode facilitar uma estratégia de persistência coordenada, dependendo de como os `SaveChanges` forem organizados.

### Limitação do exemplo

Na aula 101, `ExpensesRepository.Add` ainda chama `SaveChanges()` imediatamente.

Portanto, o simples compartilhamento do contexto **não transforma automaticamente as operações de múltiplos repositórios em uma transação de negócio coordenada**.

---

## 4. Preparando a extensão da Infrastructure

O professor abre o arquivo `DependencyInjectionExtension` do projeto Infrastructure, criado na aula 99.

O método de extensão existente permite que a API execute:

```csharp
builder.Services.AddInfrastructure();
```

### Como estava a configuração

A extensão registrava diretamente o repositório:

```csharp
public static void AddInfrastructure(this IServiceCollection services)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
}
```

### Nova organização

O professor decide dividir o registro em dois métodos privados:

1. `AddDbContext`: configura o contexto do Entity Framework Core na DI;
2. `AddRepositories`: reúne os registros dos repositórios.

O método público `AddInfrastructure` passa a chamar os dois.

```text
AddInfrastructure(services)
       |
       +-- AddDbContext(services)
       |
       +-- AddRepositories(services)
```

### Por que separar?

O instrutor espera que, à medida que o projeto crescer, outras configurações de infraestrutura sejam adicionadas.

Métodos privados com responsabilidades específicas ajudam a manter o ponto de entrada público mais enxuto e organizado.

---

## 5. Separando o registro dos repositórios

O professor cria um método privado e estático para concentrar os registros de repositórios.

### Exemplo

```csharp
private static void AddRepositories(IServiceCollection services)
{
    services.AddScoped<IExpensesRepository, ExpensesRepository>();
}
```

### Por que private?

Esse método é um detalhe interno da configuração.

A API não precisa chamar `AddRepositories` isoladamente; ela chama o método público `AddInfrastructure`.

### Por que static?

A classe `DependencyInjectionExtension` é estática, e os métodos auxiliares utilizados nessa organização também são estáticos.

### Por que o parâmetro não possui this?

O professor chama a atenção para um detalhe:

```csharp
IServiceCollection services
```

Nesse método privado, não há a palavra-chave `this`, porque ele está sendo declarado como **método auxiliar estático comum**, e não como um novo método de extensão.

### Contraste com o método público

```csharp
public static void AddInfrastructure(this IServiceCollection services)
```

A palavra-chave `this` no primeiro parâmetro permite a sintaxe:

```csharp
builder.Services.AddInfrastructure();
```

Já o método privado é chamado dentro da classe como:

```csharp
AddRepositories(services);
```

---

## 6. Criando o método privado AddDbContext

Em seguida, o instrutor cria outro método privado e estático:

```csharp
private static void AddDbContext(IServiceCollection services)
{
    services.AddDbContext<CashFlowDbContext>();
}
```

### Qual é a diferença entre os dois AddDbContext?

Aqui aparecem duas chamadas com o mesmo nome, mas papéis diferentes.

**1. Método auxiliar criado pelo professor:**

```csharp
private static void AddDbContext(IServiceCollection services)
```

É uma função privada que organiza a configuração da Infrastructure.

**2. Método oferecido pela integração EF Core + DI:**

```csharp
services.AddDbContext<CashFlowDbContext>();
```

É o método de registro específico do Entity Framework Core.

Eles não são o mesmo método.

### Por que não usar AddScoped diretamente?

O professor escolhe a API específica do EF Core, `AddDbContext<TContext>()`, para registrar o contexto.

`AddDbContext` é a abordagem convencional do EF Core para adicionar o `DbContext` à coleção de serviços, permitindo configurar seu ciclo de vida e suas opções por meio da DI.

### O que foi passado como tipo genérico?

O contexto criado na aula 90:

```csharp
CashFlowDbContext
```

A linha completa fica:

```csharp
services.AddDbContext<CashFlowDbContext>();
```

### A configuração final do método público

```csharp
public static void AddInfrastructure(this IServiceCollection services)
{
    AddDbContext(services);
    AddRepositories(services);
}
```

O instrutor coloca o registro do contexto antes do registro dos repositórios.

Ele comenta que, nesse exemplo, a ordem das duas chamadas não altera o resultado, pois ambas são feitas durante a configuração da coleção de serviços, antes da resolução do grafo de dependências.

**Nuance:** essa observação é válida para estes registros independentes e sem substituições concorrentes. No sistema de DI do .NET, **a ordem pode ser relevante** em outros cenários, como múltiplos registros do mesmo serviço e determinadas configurações de opções.

---

## 7. Entendendo services.AddDbContext<CashFlowDbContext>()

A chamada:

```csharp
services.AddDbContext<CashFlowDbContext>();
```

registra o tipo de contexto no mecanismo de DI do EF Core.

### Qual ciclo de vida é utilizado por padrão?

A sobrecarga convencional de `AddDbContext` utiliza **Scoped** por padrão para o contexto.

Em uma API ASP.NET Core típica, isso significa que o `DbContext` pode ser compartilhado por dependências resolvidas no mesmo escopo de uma requisição.

### Comparação com AddScoped

O professor destaca que agora não precisa escolher manualmente `AddScoped` ou `AddTransient` para o contexto no exemplo.

O EF Core disponibiliza o registro próprio para esse tipo de serviço.

### Por que AddDbContext é preferível ao new manual?

Porque o contexto passa a ser resolvido pelo container.

O repositório deixa de ter responsabilidade pela construção do objeto e pode receber a instância já configurada.

### O registro cria o contexto imediatamente?

Não necessariamente.

Durante a inicialização da aplicação, o registro descreve **como o serviço será resolvido**.

O contexto normalmente é criado quando necessário dentro do escopo apropriado.

### E a conexão com o MySQL?

Nesta aula, a chamada está sem uma configuração explícita:

```csharp
services.AddDbContext<CashFlowDbContext>();
```

O professor **não altera ainda** as opções do provider e a connection string.

O `CashFlowDbContext` continua contendo a configuração de MySQL que havia sido definida anteriormente em `OnConfiguring`.

A próxima aula tratará de retirar essa configuração fixa do contexto e passá-la pela DI.

---

## 8. Refatorando o construtor de ExpensesRepository

Depois de registrar o contexto no container, o professor volta à classe `ExpensesRepository`.

### Antes

O repositório construía seu próprio contexto dentro do método `Add`:

```csharp
public void Add(Expense expense)
{
    var dbContext = new CashFlowDbContext();

    dbContext.Expenses.Add(expense);
    dbContext.SaveChanges();
}
```

### Depois

O repositório passa a receber `CashFlowDbContext` no construtor:

```csharp
public ExpensesRepository(CashFlowDbContext dbContext)
{
    _dbContext = dbContext;
}
```

### Como o professor cria o construtor?

Ele utiliza o recurso de geração de construtores do Visual Studio e adiciona o parâmetro do tipo `CashFlowDbContext`.

### Qual é a consequência?

O código do repositório deixa de precisar saber **como construir** o contexto.

O repositório apenas declara a dependência:

```text
ExpensesRepository
       |
       | precisa de
       v
CashFlowDbContext
       |
       | fornecido pelo
       v
Container de DI
```

---

## 9. Campo private readonly e remoção do new

O professor precisa acessar a instância recebida no construtor dentro do método `Add`.

Para isso, cria um campo privado:

```csharp
private readonly CashFlowDbContext _dbContext;
```

E realiza a atribuição no construtor:

```csharp
public ExpensesRepository(CashFlowDbContext dbContext)
{
    _dbContext = dbContext;
}
```

### Por que private readonly?

O campo guarda a referência ao contexto fornecido pela DI.

A palavra-chave `readonly` evita que a referência seja reatribuída arbitrariamente após a inicialização, dentro das regras da linguagem.

### E o underscore?

O instrutor usa o padrão `_dbContext` para identificar um campo privado.

Essa é uma convenção de nomenclatura frequente em C#, mas não obrigatória.

### Correção de terminologia

Embora a transcrição se refira em certos momentos a uma "propriedade", a declaração:

```csharp
private readonly CashFlowDbContext _dbContext;
```

é tecnicamente um **campo privado readonly**, não uma propriedade.

### Como fica o método Add?

Ele utiliza o campo em vez de uma variável local criada com `new`:

```csharp
public void Add(Expense expense)
{
    _dbContext.Expenses.Add(expense);
    _dbContext.SaveChanges();
}
```

### O que foi removido?

```csharp
var dbContext = new CashFlowDbContext();
```

Essa é a mudança central da aula.

---

## 10. Comparação de código: antes e depois

### Antes da refatoração

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

### Depois da refatoração

```csharp
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
        _dbContext.SaveChanges();
    }
}
```

### Quais responsabilidades mudaram?

| Aspecto | Antes | Depois |
|---|---|---|
| Criação do contexto | Repositório cria com `new` | Container resolve e injeta |
| Local da dependência | Dentro do método `Add` | Construtor do repositório |
| Referência ao contexto | Variável local | Campo `_dbContext` |
| Gerenciamento de ciclo de vida | Manual | Pelo registro `AddDbContext` |
| Persistência | `Add` + `SaveChanges` | `Add` + `SaveChanges` |
| Contrato `IExpensesRepository` | Mantido | Mantido |
| Visibilidade do repositório | `internal` | `internal` |

O comportamento de persistência continua equivalente no cenário testado, mas a forma de obter o contexto é outra.

---

## 11. Como o container resolve as dependências agora?

Esta é uma conexão importante com o conteúdo das aulas 92 a 100.

### Registros disponíveis

**No projeto Application:**

```csharp
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

**No projeto Infrastructure:**

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
services.AddDbContext<CashFlowDbContext>();
```

### Como essas configurações se conectam?

```text
ExpensesController
       |
       | [FromServices]
       v
IRegisterExpenseUseCase
       |
       | implementação
       v
RegisterExpenseUseCase
       |
       | construtor solicita
       v
IExpensesRepository
       |
       | implementação
       v
ExpensesRepository
       |
       | construtor solicita
       v
CashFlowDbContext
       |
       | provider/configuração existente
       v
MySQL
```

### Passo a passo da resolução

1. A ação do controller solicita `IRegisterExpenseUseCase`.
2. O container resolve `RegisterExpenseUseCase`.
3. Seu construtor solicita `IExpensesRepository`.
4. O container resolve `ExpensesRepository`.
5. Seu construtor solicita `CashFlowDbContext`.
6. O container resolve o contexto registrado com `AddDbContext`.
7. Injeta o contexto no repositório.
8. Injeta o repositório no use case.
9. Fornece o use case à ação do controller.
10. A execução do fluxo de negócio pode prosseguir.

### O que mudou na cadeia?

Antes, o grafo de DI terminava no repositório, que criava seu próprio contexto.

Agora, o `CashFlowDbContext` também participa do grafo de dependências resolvido pelo container.

```text
ANTES:
DI -> UseCase -> Repository -> new DbContext()

DEPOIS:
DI -> UseCase -> Repository -> DbContext
                               (resolvido pela DI)
```

---

## 12. Teste no Swagger e no debugger

Depois de refatorar o registro e o repositório, o professor inicia novamente a API.

Ele configura breakpoints para verificar a resolução do contexto e a persistência da entidade.

### Dados mencionados na gravação

| Campo | Informação |
|---|---|
| Título | `MacBook` |
| Descrição | Texto de teste não especificado |
| Valor | `3000` |
| Tipo de pagamento | `0`, conforme a fala |
| Data | Não informada na transcrição |

A gravação não permite confirmar todos os campos do JSON literal. O quadro registra apenas as informações fornecidas oralmente.

### Sequência demonstrada

1. Executar a aplicação no Visual Studio.
2. Abrir o endpoint de cadastro no Swagger.
3. Clicar em **Try it out**.
4. Informar os dados de uma despesa de exemplo.
5. Executar a requisição.
6. A execução parar no breakpoint do repositório/construtor.
7. Inspecionar a referência a `CashFlowDbContext` recebida.
8. Confirmar que o container forneceu a instância.
9. Continuar até o método de adição de despesa.
10. Inspecionar a entidade antes da persistência.
11. Executar as chamadas `Add` e `SaveChanges`.
12. Inspecionar a entidade novamente.

### Resultado observado

O professor confirma pelo depurador que a instância do contexto chega corretamente ao repositório.

O método de persistência continua funcionando depois da retirada do `new`.

### O que o teste comprova?

No fluxo demonstrado, as dependências foram resolvidas corretamente e o código conseguiu salvar a despesa.

O professor não apresenta nesta gravação um relatório de benchmark, teste de carga ou medição comparativa de consumo de memória.

Por isso, os benefícios de performance mencionados são **motivação e expectativa arquitetural**, não ganhos quantitativos aferidos na aula.

---

## 13. O significado do Id 0 antes e do Id 28 depois

Um dos pontos observados no debugger é a mudança do identificador da entidade.

### Antes da operação

O professor inspeciona a entidade `Expense` antes da inserção e observa:

```text
Id = 0
```

### Depois de SaveChanges

Após o fluxo de persistência, o identificador apresentado é:

```text
Id = 28
```

### Por que isso acontece?

No cenário demonstrado, `Expense.Id` é uma propriedade numérica cujo valor inicial é 0.

O banco de dados possui geração de identificadores, e o EF Core consegue atualizar a entidade rastreada com o valor efetivamente gerado na inserção.

```text
Expense criada
    |
    v
Id = 0
    |
    v
DbSet.Add(expense)
    |
    v
SaveChanges()
    |
    v
INSERT no MySQL
    |
    v
Id = 28
```

### Relação com a aula 100

Na aula anterior, a despesa gravada recebeu **Id 27**.

Na aula atual, a nova entidade foi apresentada com **Id 28** depois de `SaveChanges`.

A sequência é compatível com a geração de identificadores pela tabela no banco.

### Nuance técnica

A observação do Id 0 não deve ser tomada como um teste universal de que "não existe nada no banco". Trata-se do valor inicial daquela entidade na demonstração.

Dependendo da estratégia de geração de chave e do provider, podem existir valores temporários rastreados pelo EF Core durante o processo.

O ponto principal da aula é que **a instância injetada do contexto foi capaz de persistir a nova entidade e refletir sua chave gerada**.

---

## 14. O que permanece para a próxima aula?

O instrutor encerra a demonstração apontando para outra parte do `CashFlowDbContext`.

### Situação atual

A configuração do provider MySQL e a connection string continuam **fixas no código do contexto**, definidas anteriormente em `OnConfiguring`.

### Problema apontado pelo professor

Esse código de configuração ainda está acoplado a informações de conexão.

O professor pretende remover essa abordagem e passar as opções necessárias pela injeção de dependência.

### Configuração antiga, conceitualmente

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Configuração fixa do provider MySQL
    // e da connection string.
}
```

O exemplo mostra apenas o formato geral; os valores e detalhes originais não são repetidos.

### O que o professor anuncia?

Na próxima aula, pretende:

- retirar a configuração fixa do contexto;
- definir as opções de banco pela DI;
- melhorar a organização do código de infraestrutura.

### O que NÃO foi realizado aqui?

- Não foi removido `OnConfiguring`.
- A connection string não foi transferida para configuração externa.
- Não foi demonstrada uma nova configuração de `UseMySql` em `AddDbContext`.
- Não foram alterados os arquivos de configuração de ambiente nesta gravação.

A aula 101 termina antes dessa próxima refatoração.

---

## 15. Boas práticas e distinções técnicas

Os pontos a seguir explicam consequências técnicas da refatoração e distinguem simplificações didáticas de regras absolutas. **Não significam que o professor realizou alterações adicionais além das descritas.**

### 15.1 AddDbContext é a API específica do EF Core para DI

O método:

```csharp
services.AddDbContext<CashFlowDbContext>();
```

registra o contexto com o mecanismo de DI do Entity Framework Core.

Na configuração convencional, o `DbContext` utiliza o ciclo de vida Scoped.

### 15.2 Scoped é por escopo, não por chamada de método

Se dois repositórios recebem o mesmo tipo `CashFlowDbContext` no mesmo escopo de DI, a instância registrada como Scoped normalmente será a mesma.

Uma nova chamada ao método `Add` de um mesmo repositório **não provoca automaticamente a criação de outro contexto**.

### 15.3 Compartilhar DbContext pode ser apropriado para uma unidade de trabalho

Contextos compartilhados podem permitir que diferentes componentes trabalhem com o mesmo rastreamento de entidades.

Mas isso precisa respeitar o design da aplicação e não implica que cada combinação de serviços deva obrigatoriamente compartilhar um contexto.

### 15.4 DbContext não é thread-safe

O EF Core não permite uso concorrente simultâneo da mesma instância de `DbContext`.

O fato de um contexto ser Scoped não significa que seja seguro utilizá-lo simultaneamente em tarefas paralelas.

Se houver concorrência, o projeto precisa adotar uma estratégia apropriada de instâncias e unidades de trabalho.

### 15.5 O container gerencia o descarte de objetos scoped

Quando o contexto é criado por meio de DI e associado a um escopo, o mecanismo de DI geralmente gerencia seu descarte ao encerrar o escopo.

Isso é uma melhoria importante em relação à criação manual sem descarte explícito, que aparecia na versão anterior do repositório.

### 15.6 readonly não torna o contexto imutável

```csharp
private readonly CashFlowDbContext _dbContext;
```

O campo não pode ser reatribuído em qualquer ponto da classe após sua inicialização, mas o `DbContext` continua mutável, rastreando e manipulando entidades.

### 15.7 O registro do DbContext não dispensa configurar o provider

`AddDbContext` é o mecanismo de registro e configuração de um contexto.

Na gravação, a configuração de MySQL ainda é feita pelo `OnConfiguring` existente.

Se essa configuração for removida posteriormente, será necessário fornecer as opções adequadas por outro caminho.

### 15.8 OnConfiguring ainda é executado pelo EF Core

O fato de o contexto ser criado por DI não significa, por si só, que o método `OnConfiguring` deixe de existir ou seja ignorado.

A refatoração anunciada para a próxima aula será justamente **transferir a responsabilidade de configuração** para o local apropriado.

### 15.9 Compartilhamento e performance não são equivalentes

O professor menciona redução de instâncias e memória.

Tecnicamente, usar menos objetos em determinados fluxos pode reduzir alocações, mas não representa automaticamente uma melhoria mensurável de performance.

É necessário analisar duração do contexto, rastreamento de entidades, consultas, concorrência e perfil de carga da aplicação.

### 15.10 Vários SaveChanges não compõem automaticamente uma operação atômica

No código atual:

```csharp
_dbContext.SaveChanges();
```

é chamado dentro do método `ExpensesRepository.Add`.

Mesmo que outro repositório compartilhe o contexto, persistir em momentos distintos não é o mesmo que coordenar todas as operações em um único ponto de commit de negócio.

### 15.11 A ordem de registros da DI pode importar em outros cenários

A aula diz que tanto faz chamar `AddDbContext` ou `AddRepositories` primeiro.

Para os registros específicos mostrados, ambos são conhecidos antes da resolução de dependências, então essa ordem não é decisiva.

Porém, múltiplos registros do mesmo serviço e regras de resolução de `IEnumerable<T>` podem tornar a ordem relevante em outros projetos.

### 15.12 O repositório continua internal

A refatoração não exige tornar `ExpensesRepository` pública.

Ela permanece dentro da Infrastructure e continua sendo conhecida pelos consumidores através de `IExpensesRepository`.

O registro `AddScoped<IExpensesRepository, ExpensesRepository>()` também continua dentro da Infrastructure, onde o tipo `internal` está acessível.

### 15.13 O DbContext também continua internal

`CashFlowDbContext` foi marcado como `internal` na aula 98.

Isso não impede sua utilização pela DI configurada dentro da própria Infrastructure.

### 15.14 O contexto não precisa ser recebido pela Application

O use case continua dependendo de `IExpensesRepository`.

É o repositório, por sua vez, que recebe `CashFlowDbContext`.

Essa separação preserva o objetivo arquitetural de evitar que a camada de negócio manipule diretamente o EF Core.

### 15.15 Uma melhoria de design não exige alterar o contrato público

A interface:

```csharp
public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

continua igual.

O que mudou foi a implementação interna do repositório e sua relação com a injeção de dependência.

Esse é um benefício do desacoplamento estudado na aula 97.

### 15.16 A escolha do tipo de pagamento deve respeitar a validação existente

A transcrição menciona `0` para o pagamento no teste.

Não é possível deduzir apenas dessa fala quais membros do enum são aceitos, nem se o valor foi exibido exatamente assim na requisição completa.

A documentação preserva o valor narrado sem inferir regras de validação não mostradas.

---

## 16. Resumo, perguntas de revisão e mapa mental

### Principais aprendizados

- O cadastro já persistia despesas antes desta aula, mas o repositório construía o contexto manualmente.
- O professor deseja reduzir essa responsabilidade da classe de repositório.
- Um cenário com múltiplos repositórios motiva o compartilhamento de um `DbContext` no mesmo escopo de DI.
- `DependencyInjectionExtension` é reorganizada em métodos privados específicos.
- `AddRepositories(IServiceCollection services)` mantém o registro `IExpensesRepository` → `ExpensesRepository`.
- `AddDbContext(IServiceCollection services)` chama `services.AddDbContext<CashFlowDbContext>()`.
- `AddInfrastructure` chama ambos os métodos auxiliares.
- O registro padrão de `AddDbContext` utiliza o ciclo de vida Scoped.
- `ExpensesRepository` passa a receber `CashFlowDbContext` por seu construtor.
- O campo `private readonly CashFlowDbContext _dbContext` armazena a referência.
- A linha `new CashFlowDbContext()` é removida do método `Add`.
- A persistência continua utilizando `Expenses.Add` e `SaveChanges`.
- O teste no Swagger com uma despesa **MacBook**, valor **3000**, mostra o contexto injetado corretamente.
- O depurador apresenta **Id 0 antes** e **Id 28 depois** da persistência.
- A configuração fixa de MySQL e connection string ainda existe em `OnConfiguring`.
- A próxima aula tratará de mover essa configuração para a DI.

### Quadro das responsabilidades

| Componente | Camada | Responsabilidade depois da aula |
|---|---|---|
| `IExpensesRepository` | Domain | Contrato para persistir despesa |
| `ExpensesRepository` | Infrastructure | Receber contexto e salvar despesa |
| `CashFlowDbContext` | Infrastructure | Acesso ao banco via EF Core |
| `AddRepositories` | Infrastructure | Registrar repositórios |
| `AddDbContext` (auxiliar) | Infrastructure | Registrar o contexto |
| `AddInfrastructure` | Infrastructure | Ponto público de configuração |
| `RegisterExpenseUseCase` | Application | Validar, mapear e solicitar persistência |
| `ExpensesController` | API | Receber e encaminhar requisição |

### Perguntas para fixar

1. Por que o professor quer remover `new CashFlowDbContext()` do repositório?
2. Qual cenário hipotético de múltiplos repositórios ele utiliza para explicar a motivação?
3. Por que a reutilização de contexto pode ser útil?
4. Em qual projeto fica `DependencyInjectionExtension`?
5. Quais métodos privados foram criados nessa classe?
6. Por que os métodos auxiliares recebem `IServiceCollection` sem `this`?
7. Qual método do EF Core registra o `CashFlowDbContext` na DI?
8. Qual ciclo de vida é utilizado por padrão por `AddDbContext`?
9. Qual dependência é adicionada ao construtor de `ExpensesRepository`?
10. Para que serve o campo `_dbContext`?
11. O que muda no método `Add` depois da refatoração?
12. O use case passou a conhecer diretamente a classe `CashFlowDbContext`?
13. Qual identificador aparece antes e depois de `SaveChanges`?
14. Por que `Id = 0` antes de salvar não constitui, por si só, um teste do banco?
15. A aula remove a connection string fixa do contexto?
16. Por que um `DbContext` Scoped não deve ser utilizado em operações concorrentes simultâneas?
17. O compartilhamento do contexto garante automaticamente uma transação única?
18. Qual é a diferença entre o método auxiliar `AddDbContext(IServiceCollection)` e `services.AddDbContext<TContext>()`?

### Mapa mental

```text
                             AULA 101
                       REFATORAÇÃO DO DBCONTEXT
                                  |
                           PROBLEMA INICIAL
                                  |
                  ExpensesRepository.Add(...)
                                  |
                      new CashFlowDbContext()
                                  |
                     Criação manual do objeto
                                  |
                                  v
                         NOVA CONFIGURAÇÃO
                                  |
                     DependencyInjectionExtension
                                  |
                       AddInfrastructure(services)
                          /               \
                         /                 \
              AddDbContext(services)  AddRepositories(services)
                       |                       |
                       v                       v
          services.AddDbContext<...>()   services.AddScoped<...>()
                       |                       |
                       v                       v
                CashFlowDbContext        ExpensesRepository
                    (Scoped)                   |
                       +-----------------------+
                                               |
                                    Injeção pelo construtor
                                               |
                                  private readonly _dbContext
                                               |
                                         Add(Expense)
                                               |
                                    _dbContext.Expenses.Add
                                               |
                                    _dbContext.SaveChanges
                                               |
                                               v
                                          MySQL
                                               |
                                       Expense Id = 28
                                               |
                                               v
                                       PRÓXIMA AULA
                                               |
                            Configuração do provider e
                            connection string via DI
```

**Conclusão:** a aula 101 refatora o acesso a dados para que `CashFlowDbContext` deixe de ser criado diretamente em `ExpensesRepository` e passe a ser fornecido pelo container de DI, utilizando `AddDbContext<CashFlowDbContext>()`. A extensão da Infrastructure é organizada em métodos separados, e o teste manual mostra a persistência da nova despesa com **Id 28**. A configuração de conexão com o MySQL ainda permanece no contexto e será refatorada em seguida.
