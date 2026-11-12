# Implementando ExpensesRepository: interface, internal e persistência com Entity Framework Core

> **Aula 98 — Implementação concreta do repositório de despesas na Infrastructure**
>
> Continuação da aula 97, em que criamos o contrato `IExpensesRepository` na camada Domain. Nesta aula, o instrutor cria uma classe concreta na Infrastructure, implementa o método `Add(Expense expense)` e usa o `CashFlowDbContext` para gravar a despesa no banco de dados.
>
> **Escopo:** a aula termina com a implementação do método do repositório. O uso desse contrato pelo use case da Application e o registro de injeção de dependência ficam para a próxima aula.

## Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [Recapitulando o contrato IExpensesRepository](#2-recapitulando-o-contrato-iexpensesrepository)
3. [Por que criar uma classe de repositório?](#3-por-que-criar-uma-classe-de-repositório)
4. [Criando a pasta Repositories na Infrastructure](#4-criando-a-pasta-repositories-na-infrastructure)
5. [Implementando a interface](#5-implementando-a-interface)
6. [Por que a classe permanece internal?](#6-por-que-a-classe-permanece-internal)
7. [Restringindo também o CashFlowDbContext](#7-restringindo-também-o-cashflowdbcontext)
8. [Implementação do método Add](#8-implementação-do-método-add)
9. [Diferença entre Add e SaveChanges](#9-diferença-entre-add-e-savechanges)
10. [Código completo reconstruído](#10-código-completo-reconstruído)
11. [Organização das camadas e direção das dependências](#11-organização-das-camadas-e-direção-das-dependências)
12. [O que será feito na próxima aula?](#12-o-que-será-feito-na-próxima-aula)
13. [Observações técnicas complementares](#13-observações-técnicas-complementares)
14. [Perguntas de revisão](#14-perguntas-de-revisão)
15. [Resumo e mapa mental](#15-resumo-e-mapa-mental)

---

## 1. Objetivo da aula

Na aula 97, o professor introduziu o conceito de interfaces e criou um contrato para o repositório de despesas:

```csharp
public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

O contrato descreve **a operação que a aplicação precisa**, mas não determina como ela será executada.

Nesta aula, o professor cria a classe que implementa esse contrato e delega a persistência ao Entity Framework Core.

### Passos realizados

1. Criar uma pasta `Repositories` dentro de `CashFlow.Infrastructure/DataAccess`.
2. Criar a classe de repositório de despesas, com nome pluralizado.
3. Manter a classe com o modificador `internal`.
4. Declarar que ela implementa `IExpensesRepository`.
5. Gerar a implementação obrigatória do método `Add`.
6. Alterar a visibilidade do `CashFlowDbContext` para `internal`.
7. Criar um `CashFlowDbContext` dentro de `Add`.
8. Executar `dbContext.Expenses.Add(expense)`.
9. Executar `dbContext.SaveChanges()`.

O instrutor anuncia que a utilização do repositório no use case da Application e a configuração da DI serão feitas **na próxima aula**, não nesta.

---

## 2. Recapitulando o contrato IExpensesRepository

O contrato foi criado dentro do projeto `CashFlow.Domain`, no diretório `Repositories/Expenses`.

### Estrutura estabelecida na aula 97

```text
CashFlow.Domain
├── Entities
│   └── Expense.cs
├── Enums
│   └── PaymentType.cs
└── Repositories
    └── Expenses
        └── IExpensesRepository.cs
```

### Código do contrato

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

**Nota sobre fidelidade:** a grafia `IExpensesRepository` segue a representação adotada no resumo da aula 97. A gravação atual se refere ao contrato criado anteriormente, mas não permite inspecionar a declaração exata na tela.

### O que significa a assinatura?

| Trecho | Significado |
|---|---|
| `public interface` | O contrato pode ser acessado por outros assemblies que referenciam o Domain |
| `IExpensesRepository` | Nome do contrato de repositório |
| `void` | A operação não tem valor de retorno |
| `Add` | Nome da operação |
| `Expense expense` | Recebe uma entidade Expense |

A interface define **o que pode ser solicitado ao repositório**.

O método ainda não contém código para acessar o banco de dados.

```text
IExpensesRepository
        |
        v
"Deve existir uma operação Add(Expense)"
        |
        v
Não define a tecnologia de persistência
```

Essa separação permite trabalhar com uma abstração em vez de ligar o código consumidor diretamente ao Entity Framework Core.

---

## 3. Por que criar uma classe de repositório?

O professor explica que a nova classe ficará responsável pelas operações de acesso aos dados de despesas.

A primeira operação será **adicionar uma despesa**, mas ele menciona que o repositório poderá futuramente contemplar outras responsabilidades:

- recuperar despesas;
- atualizar despesas;
- excluir despesas;
- filtrar despesas.

### Contrato versus implementação

```text
CONTRATO
IExpensesRepository
    |
    | Declara o que deve existir
    v
void Add(Expense expense)


IMPLEMENTAÇÃO
ExpensesRepository
    |
    | Define como a operação acontece
    v
Entity Framework Core
    |
    v
MySQL
```

A interface declara o comportamento exigido.

A classe concreta utiliza a infraestrutura disponível para executar esse comportamento.

### Qual é a vantagem?

A Application poderá solicitar a operação `Add` usando o contrato, sem precisar conhecer os detalhes de construção de `CashFlowDbContext` e de uso do EF Core.

Esse é o objetivo arquitetural, embora o professor ainda não o conecte ao use case nesta aula.

---

## 4. Criando a pasta Repositories na Infrastructure

O instrutor navega até o projeto de infraestrutura no Visual Studio.

### Passo a passo

1. Expandir `CashFlow.Infrastructure`.
2. Expandir `DataAccess`, criada na aula 90.
3. Criar uma nova pasta `Repositories`.
4. Adicionar uma classe dentro dessa pasta.
5. Nomear a classe de acordo com seu papel de repositório de despesas.

A fala começa com `ExpenseRepository` e depois o professor utiliza o recurso de renomeação do Visual Studio para deixar o nome **no plural**.

Neste resumo, utilizamos `ExpensesRepository`, em conformidade com essa correção verbal.

### Estrutura resultante

```text
CashFlow.Infrastructure
└── DataAccess
    ├── CashFlowDbContext.cs
    └── Repositories
        └── ExpensesRepository.cs
```

O `CashFlowDbContext` continua na pasta `DataAccess`; o novo arquivo do repositório fica dentro da subpasta `Repositories`.

O professor também remove os `using` desnecessários do arquivo gerado.

---

## 5. Implementando a interface

O projeto Infrastructure já possui uma referência para `CashFlow.Domain`.

Isso permite que o código da Infrastructure utilize a interface `IExpensesRepository`, que está declarada como `public` no Domain.

### Sintaxe de implementação

```csharp
internal class ExpensesRepository : IExpensesRepository
{
}
```

### Entendendo os dois pontos

O trecho:

```csharp
: IExpensesRepository
```

indica que a classe implementa a interface.

```text
IExpensesRepository
      ^
      |
      | contrato implementado
      |
ExpensesRepository
```

### Por que aparece um erro no Visual Studio?

A interface exige que a classe implemente o método:

```csharp
void Add(Expense expense);
```

Enquanto a classe não possuir uma implementação compatível, o compilador acusa que o contrato não foi satisfeito.

### Usando a correção automática

O instrutor:

1. posiciona o mouse sobre o erro;
2. abre a opção **Show Potential Fix**;
3. escolhe **Implement Interface**;
4. deixa o Visual Studio gerar a estrutura inicial do método.

O resultado típico seria semelhante a:

```csharp
public void Add(Expense expense)
{
    throw new NotImplementedException();
}
```

A exceção é apenas um marcador provisório gerado pelo editor. O professor a remove para escrever a implementação real.

### Por que o método é public?

Mesmo que a classe seja `internal`, o método que implementa implicitamente um contrato público precisa ser `public` na classe.

```csharp
internal class ExpensesRepository : IExpensesRepository
{
    public void Add(Expense expense)
    {
        // Implementação.
    }
}
```

**Distinção:** `internal` controla a acessibilidade da **classe** a outros assemblies; `public` no método atende às exigências da implementação pública do contrato.

---

## 6. Por que a classe permanece internal?

O professor interrompe a implementação para justificar uma escolha de visibilidade.

Ele **não** altera `ExpensesRepository` de `internal` para `public`.

### O que internal significa?

No contexto usual do C#, `internal` permite utilizar o tipo dentro do **mesmo assembly** em que ele foi declarado.

Na estrutura do curso, o projeto `CashFlow.Infrastructure` normalmente gera um assembly próprio.

Portanto:

```csharp
internal class ExpensesRepository
{
}
```

fica acessível a código da própria Infrastructure, mas não fica disponível diretamente para consumo por outros assemblies apenas porque eles referenciam o projeto.

### O objetivo do professor

A API precisará possuir uma referência para a Infrastructure em sua futura configuração.

Mesmo assim, o professor não deseja que a API utilize diretamente classes internas de persistência.

Ele usa `internal` como uma forma de **encapsulamento de infraestrutura**.

```text
CashFlow.Infrastructure
        |
        +-- ExpensesRepository (internal)
        |
        +-- CashFlowDbContext (internal)
        |
        v
Tipos não expostos diretamente
aos outros assemblies
```

### O que a API poderá conhecer?

Ela poderá conhecer os pontos de integração que a Infrastructure decidir expor publicamente, sem precisar construir diretamente o repositório concreto.

A forma exata dessa integração será apresentada em uma aula posterior.

### Correção de precisão

O professor usa a expressão "dentro do projeto" para explicar `internal`.

Tecnicamente, a fronteira de acessibilidade é o **assembly**, não a pasta nem o namespace. Em projetos .NET convencionais, cada projeto costuma gerar um assembly.

Também existe o recurso `InternalsVisibleTo`, que permite conceder acesso a tipos internos a assemblies específicos. Ele não é utilizado nesta aula.

---

## 7. Restringindo também o CashFlowDbContext

Na aula 90, a classe do contexto foi declarada `public`, porque seria utilizada temporariamente fora da Infrastructure durante os testes da aula 91.

Agora, o professor retorna a essa classe e sugere alterar a visibilidade para `internal`.

### Antes

```csharp
public class CashFlowDbContext : DbContext
{
    // ...
}
```

### Depois

```csharp
internal class CashFlowDbContext : DbContext
{
    // ...
}
```

### Por que fazer isso?

Para reforçar que o contexto do banco de dados pertence à camada Infrastructure.

Os use cases da Application não deveriam precisar manipular diretamente o EF Core nem conhecer a classe concreta do contexto.

### Representação

```text
API / Application
       |
       v
Contrato público do Domain
       |
       v
Implementação interna na Infrastructure
       |
       v
CashFlowDbContext (internal)
       |
       v
MySQL
```

O professor está restringindo a superfície de acesso das classes concretas de persistência.

**Importante:** isso não significa que o próprio Entity Framework Core impeça outros projetos de utilizar um DbContext. É uma decisão de design adotada na solução.

---

## 8. Implementação do método Add

Com a interface implementada, o professor preenche o corpo do método `Add`.

Ele considera que a entidade `Expense` já chega pronta e válida para persistência.

### Etapa 1: criar o DbContext

```csharp
var dbContext = new CashFlowDbContext();
```

A classe `CashFlowDbContext` foi criada na aula 90 e possui uma propriedade para o conjunto de despesas.

```csharp
public DbSet<Expense> Expenses { get; set; }
```

Na aula atual, o contexto foi marcado como `internal`.

### Etapa 2: registrar a despesa no conjunto

```csharp
dbContext.Expenses.Add(expense);
```

A instância `expense` é a entidade recebida como parâmetro do método do repositório.

```csharp
public void Add(Expense expense)
```

### Etapa 3: persistir a alteração

```csharp
dbContext.SaveChanges();
```

Essa chamada solicita que o EF Core persista as alterações pendentes rastreadas pelo contexto.

### Fluxo completo

```text
Add(Expense expense)
          |
          v
new CashFlowDbContext()
          |
          v
dbContext.Expenses.Add(expense)
          |
          v
Entidade marcada como Added
          |
          v
dbContext.SaveChanges()
          |
          v
Comandos SQL executados pelo provider
          |
          v
Tabela expenses no MySQL
```

### O que o repositório espera receber?

O instrutor ressalta que a entidade **já deve estar preparada** para salvar.

Isso significa que a responsabilidade assumida por esse método, na gravação, é a persistência; a validação e o mapeamento dos dados recebidos na API pertencem a etapas anteriores do fluxo.

Essa é a premissa adotada na demonstração, e não uma garantia automática de que toda entidade recebida esteja correta.

---

## 9. Diferença entre Add e SaveChanges

Essa distinção já foi demonstrada na aula 91 e é reutilizada aqui.

### Add

```csharp
dbContext.Expenses.Add(expense);
```

Registra a entidade no contexto e marca seu estado para inserção.

Isso, sozinho, não executa necessariamente o `INSERT` no banco.

### SaveChanges

```csharp
dbContext.SaveChanges();
```

Efetiva as alterações pendentes daquele contexto.

### Por que as duas chamadas aparecem juntas?

O método `Add` do repositório precisa cumprir, nesta implementação, a finalidade de **persistir** uma despesa.

Portanto, não basta marcar a entidade como adicionada; o professor chama também `SaveChanges()`.

### Comparação

| Operação | Efeito |
|---|---|
| `Expenses.Add(expense)` | Rastreia a entidade e marca sua inserção como pendente |
| `SaveChanges()` | Solicita a gravação das alterações pendentes |
| Método `ExpensesRepository.Add` | Reúne as duas operações na implementação da aula |

### E o Id da despesa?

Na aula 91, o identificador começou como 0 e recebeu o valor gerado pelo MySQL depois de `SaveChanges()`.

A mesma expectativa pode se aplicar quando o repositório salva uma nova entidade com a configuração já estudada, mas **não há novo teste de inserção no banco nesta gravação**.

---

## 10. Código completo reconstruído

A estrutura abaixo representa a implementação descrita pelo instrutor.

A transcrição fornece as operações e decisões de visibilidade, mas não mostra integralmente a declaração de namespaces no editor. Os namespaces a seguir foram reconstruídos conforme a organização já estabelecida nas aulas anteriores.

### Arquivo: CashFlow.Domain/Repositories/Expenses/IExpensesRepository.cs

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

O contrato foi criado na aula 97 e permanece inalterado nesta aula.

### Arquivo: CashFlow.Infrastructure/DataAccess/Repositories/ExpensesRepository.cs

```csharp
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories.Expenses;

namespace CashFlow.Infrastructure.DataAccess.Repositories;

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

### Arquivo: CashFlow.Infrastructure/DataAccess/CashFlowDbContext.cs

Neste arquivo, a mudança demonstrada é no modificador de acesso:

```csharp
internal class CashFlowDbContext : DbContext
{
    // DbSet<Expense> Expenses e OnConfiguring,
    // configurados na aula 90.
}
```

O restante da configuração do contexto, incluindo `UseMySql` e a connection string, **não é reimplementado nesta gravação**.

### O que deve compilar nessa estrutura?

O projeto Infrastructure referencia o Domain e, por isso, pode implementar a interface pública e utilizar a entidade `Expense`.

Como o repositório e o contexto estão no mesmo assembly, o repositório pode utilizar `CashFlowDbContext` apesar de seu modificador `internal`.

### O que esse exemplo não contém?

Não há, na gravação:

- construtor com injeção de `CashFlowDbContext`;
- uso de `IServiceProvider`;
- registro de `IExpensesRepository` no container de DI;
- integração do repositório ao use case da Application;
- novos testes pelo Swagger ou Workbench;
- tratamento adicional de exceções ou transações.

Essas funcionalidades não devem ser apresentadas como se já tivessem sido implementadas nesta aula.

---

## 11. Organização das camadas e direção das dependências

A principal evolução arquitetural é a divisão entre **contrato** e **implementação**.

### Domain

O Domain declara a entidade e o contrato do repositório.

```text
CashFlow.Domain
├── Entities
│   └── Expense
└── Repositories
    └── Expenses
        └── IExpensesRepository
```

### Infrastructure

A Infrastructure conhece os tipos públicos do Domain e implementa o contrato.

```text
CashFlow.Infrastructure
└── DataAccess
    ├── CashFlowDbContext
    └── Repositories
        └── ExpensesRepository
            implementa IExpensesRepository
```

### Application

O use case poderá depender do contrato, sem precisar instanciar diretamente `CashFlowDbContext`.

```text
                        Application
                            |
                            | conhece o contrato
                            v
                     IExpensesRepository
                      (Domain, public)
                            ^
                            |
                            | implementa
                            |
                     ExpensesRepository
                  (Infrastructure, internal)
                            |
                            v
                     CashFlowDbContext
                  (Infrastructure, internal)
                            |
                            v
                      Entity Framework
                            |
                            v
                          MySQL
```

### Por que Infrastructure referencia Domain?

Porque a implementação precisa conhecer a interface e a entidade.

O professor utiliza esse fato para declarar `ExpensesRepository : IExpensesRepository`.

### Por que Application não precisa referenciar Infrastructure diretamente?

O contrato público está localizado no Domain. Assim, a Application pode expressar a dependência da operação de persistência sem conhecer a implementação específica.

**Complemento importante:** essa separação será completada por uma configuração de composição/DI que determine qual implementação satisfaz a interface. Essa configuração ainda não é realizada nesta aula.

---

## 12. O que será feito na próxima aula?

O professor termina dizendo que voltará ao use case de registro de despesas na Application.

A intenção é utilizar o repositório que acabamos de implementar e configurar as dependências corretamente.

### Situação ao final desta aula

```text
Contrato criado?                  SIM
IExpensesRepository

Implementação criada?             SIM
ExpensesRepository

Método Add implementado?           SIM
DbSet.Add + SaveChanges

Contexto restrito a Infrastructure? SIM
internal CashFlowDbContext

Repositório restrito a Infrastructure? SIM
internal ExpensesRepository

Use case integrado?                NÃO
Registro DI implementado?          NÃO
Teste HTTP realizado nesta aula?   NÃO
```

### Próxima etapa anunciada

```text
Application
    |
    v
Receber IExpensesRepository
    |
    v
Configurar serviço de DI
    |
    v
Resolver ExpensesRepository
    |
    v
Persistir a entidade
```

Essa sequência é uma **antecipação conceitual da direção descrita pelo professor**, não código já implementado.

O professor menciona também que ensinará um "truque" na próxima aula, sem explicar do que se trata nesta gravação.

---

## 13. Observações técnicas complementares

As observações abaixo adicionam precisão técnica ao que foi efetivamente demonstrado. **Não representam alterações feitas pelo instrutor nesta aula.**

### 13.1 internal é uma restrição de acesso por assembly

O modificador `internal` impede acesso direto ao tipo por código em outros assemblies, salvo mecanismos específicos que concedam visibilidade.

Ele não cria isolamento de memória, não impede reflexão e não transforma a classe em uma fronteira de segurança.

Sua principal finalidade no caso é **encapsulamento de implementação**.

### 13.2 Um tipo internal pode implementar uma interface public

Isso é válido:

```csharp
internal class ExpensesRepository : IExpensesRepository
{
    public void Add(Expense expense)
    {
        // Implementação.
    }
}
```

A interface é pública e pode ser conhecida pelos consumidores.

A classe concreta continua interna ao assembly.

### 13.3 O registro de DI de uma classe internal é possível, mas exige uma organização apropriada

O fato de a classe ser `internal` não impede que a Infrastructure registre uma associação entre a interface pública e o tipo concreto dentro do próprio assembly.

Entretanto, uma API que referencie a Infrastructure **não poderá escrever diretamente** um registro que cite `ExpensesRepository` se a classe não estiver acessível à API.

Uma forma comum de organizar isso é expor um método público de configuração na Infrastructure que realize internamente os registros.

Esse é apenas um **exemplo de estratégia possível**, não o "truque" anunciado nem uma implementação mostrada nesta aula.

### 13.4 O método da aula cria o DbContext com new

A implementação narrada é:

```csharp
var dbContext = new CashFlowDbContext();
```

Ela não utiliza, neste momento, injeção do contexto pelo construtor.

Não devemos substituir essa linha silenciosamente por uma implementação com DI ao documentar a aula, pois isso anteciparia uma refatoração que ainda não foi demonstrada.

### 13.5 O ciclo de vida do contexto precisa ser administrado

`DbContext` implementa `IDisposable` e representa uma unidade de trabalho.

A implementação da gravação instancia o contexto sem mostrar um descarte explícito.

Uma melhoria possível no código de criação manual seria:

```csharp
using var dbContext = new CashFlowDbContext();

dbContext.Expenses.Add(expense);
dbContext.SaveChanges();
```

Esse trecho é uma **sugestão técnica**, não o código mostrado pelo professor. Em arquiteturas que configuram o contexto pela DI, normalmente o escopo apropriado gerencia sua vida útil.

### 13.6 O SaveChanges dentro do método Add define uma escolha de persistência imediata

Nesta implementação, cada chamada a `ExpensesRepository.Add` cria um contexto e chama `SaveChanges` imediatamente.

Isso significa que a operação foi desenhada, por enquanto, para tentar persistir a entidade já durante o `Add`.

Em aplicações mais elaboradas, repositórios e uma unidade de trabalho podem ser organizados de modo diferente, com controle de transações e persistência coordenada.

O professor não discute essa alternativa nesta aula.

### 13.7 DbContext não deve ser compartilhado indiscriminadamente

O EF Core acompanha alterações de entidades em seu contexto e não suporta operações simultâneas concorrentes na mesma instância de `DbContext`.

Quando o curso evoluir para DI, a escolha do ciclo de vida deverá considerar esse comportamento.

A gravação ainda não define o registro do contexto no container.

### 13.8 Add não equivale necessariamente a uma transação de negócio completa

O código executa a operação de persistência da despesa.

Ele não define regras adicionais para possíveis operações relacionadas, como gravar outros registros ou garantir atomicidade entre vários repositórios.

O conceito de transações de negócio poderá ser estudado separadamente.

### 13.9 A validação fica fora deste método na demonstração

O professor assume que `Expense` já está pronta para salvar.

Isso é coerente com a existência de validações anteriores no fluxo de cadastro.

Entretanto, a assinatura da interface não impede que um consumidor passe um objeto inválido; a integridade final também depende das validações da aplicação, do mapeamento do EF e das restrições do banco.

### 13.10 Uma interface é um contrato, não um objeto de repositório instanciável

É válido declarar:

```csharp
IExpensesRepository repository;
```

Mas não é possível executar:

```csharp
// Inválido:
var repository = new IExpensesRepository();
```

O tipo concreto precisa ser criado ou fornecido por um mecanismo externo.

Isso retoma o conceito de interfaces da aula 97.

### 13.11 O nome no plural não define o mapeamento de tabela

`ExpensesRepository` é apenas o nome escolhido para a classe do repositório.

O acesso à tabela acontece pelo `DbSet<Expense> Expenses` configurado no contexto, juntamente com as convenções e configurações de mapeamento do EF Core.

O nome da classe do repositório não é uma instrução SQL.

---

## 14. Perguntas de revisão

1. Qual interface foi implementada nesta aula?
2. Em qual projeto a interface está localizada?
3. Por que a Infrastructure consegue enxergar `IExpensesRepository`?
4. Onde o professor criou a pasta `Repositories` da implementação concreta?
5. Qual é a função de `ExpensesRepository`?
6. O que significa `internal class ExpensesRepository`?
7. Por que o professor também altera `CashFlowDbContext` para `internal`?
8. Uma classe `internal` pode implementar uma interface `public`?
9. Por que o Visual Studio apresenta erro antes de implementar o método `Add`?
10. Qual é a finalidade da opção **Implement Interface**?
11. Por que o método `Add` da classe é declarado `public`?
12. O que faz `dbContext.Expenses.Add(expense)`?
13. Por que também chamamos `dbContext.SaveChanges()`?
14. O método da aula utiliza `DbContext` por injeção de dependência?
15. Por que a Application poderá depender apenas de `IExpensesRepository`?
16. A integração da interface ao use case já foi realizada?
17. Por que o descarte de um `DbContext` criado manualmente merece atenção?
18. Que relação existe entre esse repositório e o princípio de inversão de dependência?

---

## 15. Resumo e mapa mental

### Principais aprendizados

- O professor cria o primeiro repositório concreto de despesas.
- A nova classe fica em `CashFlow.Infrastructure/DataAccess/Repositories`.
- Seu nome é corrigido para o plural, representado neste material por `ExpensesRepository`.
- A classe mantém o modificador `internal`.
- A Infrastructure já referencia o projeto Domain, então consegue acessar sua interface pública.
- `ExpensesRepository` implementa `IExpensesRepository`.
- O Visual Studio auxilia na geração da implementação obrigatória de `Add`.
- `CashFlowDbContext` também passa a ser `internal`.
- Dentro de `Add(Expense expense)`, o repositório instancia `CashFlowDbContext`.
- `dbContext.Expenses.Add(expense)` marca a entidade para inserção.
- `dbContext.SaveChanges()` persiste as alterações pendentes.
- O repositório concentra o código de acesso aos dados de despesas.
- A classe ainda não está registrada na DI nem integrada ao use case da Application.
- A próxima aula tratará dessa integração e da configuração de dependências.

### Quadro de responsabilidades

| Elemento | Camada | Responsabilidade |
|---|---|---|
| `Expense` | Domain | Representar a entidade de despesa |
| `IExpensesRepository` | Domain | Declarar o contrato `Add(Expense)` |
| `ExpensesRepository` | Infrastructure | Implementar a operação de persistência |
| `CashFlowDbContext` | Infrastructure | Configurar o EF Core e expor o conjunto de despesas |
| `DbSet<Expense>` | Infrastructure / EF Core | Rastrear e consultar entidades de despesas |
| `SaveChanges()` | EF Core | Persistir as alterações pendentes |
| Use case de cadastro | Application | Executar o fluxo de negócio, a ser integrado na sequência |

### Mapa mental

```text
                           AULA 98
                   REPOSITÓRIO DE DESPESAS
                              |
              +---------------+----------------+
              |                                |
           DOMAIN                         INFRASTRUCTURE
              |                                |
   IExpensesRepository                        DataAccess
              |                                |
   void Add(Expense expense)              Repositories
              |                                |
              |                         ExpensesRepository
              |                              internal
              |                                |
              +-------- implementado por ------+
                                               |
                                    public void Add(Expense)
                                               |
                                     new CashFlowDbContext()
                                               |
                                    dbContext.Expenses.Add()
                                               |
                                    dbContext.SaveChanges()
                                               |
                                               v
                                             MySQL

                              ENCAPSULAMENTO
                                    |
                   ExpensesRepository -> internal
                   CashFlowDbContext  -> internal
                                    |
                          PRÓXIMA AULA
                                    |
                    Application + DI + repositório
```

**Conclusão:** nesta aula, o contrato `IExpensesRepository` ganha sua primeira implementação concreta na Infrastructure. A classe `ExpensesRepository` recebe uma `Expense`, utiliza `CashFlowDbContext` e executa `Add` seguido de `SaveChanges` para persistir a entidade. Ao manter o repositório e o contexto como `internal`, o professor limita sua exposição fora da Infrastructure, preparando a integração por interface e injeção de dependência nas próximas aulas.
