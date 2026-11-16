# Connection string dinâmica: appsettings, IConfiguration, DbContextOptions e configuração do MySQL por DI

> **Aula 102 — Refatorando CashFlowDbContext para receber a configuração de banco externamente**
>
> Continuação da aula 101, em que `CashFlowDbContext` passou a ser registrado com `AddDbContext` e injetado no construtor de `ExpensesRepository`. Agora o professor remove a connection string e o provider definidos diretamente no `OnConfiguring` do contexto, lê a configuração pelo `appsettings.Development.json` e configura o EF Core dentro de `DependencyInjectionExtension`.
>
> **Resultado demonstrado:** a API inicia com a connection string lida por `IConfiguration`, o cadastro de uma despesa no Swagger funciona, e o depurador mostra que o identificador passou de **0 para 29** depois da persistência.
>
> **Delimitação:** os códigos abaixo reconstroem a intenção da gravação, complementada por informações já estudadas nas aulas anteriores. Os detalhes literais de conexão e as credenciais reais não são incluídos, por segurança.

## Índice

1. Objetivo e evolução do projeto
2. Por que não deixar a connection string hardcoded?
3. Onde ficam as configurações: appsettings.json e appsettings.Development.json
4. Criando a seção ConnectionStrings e a chave Connection
5. Entendendo como GetConnectionString funciona
6. Removendo OnConfiguring do CashFlowDbContext
7. Recebendo DbContextOptions no construtor
8. O papel de : base(options)
9. Configurando UseMySql dentro de AddDbContext
10. Como funciona a expressão lambda de configuração
11. Passando IConfiguration de Program.cs para a Infrastructure
12. Duas alternativas apresentadas pelo professor
13. Código completo reconstruído: arquivos envolvidos
14. Fluxo de configuração e resolução em tempo de execução
15. Teste no debugger e no Swagger
16. Id 0 antes e Id 29 depois do SaveChanges
17. Diferença entre connection string dinâmica e provider dinâmico
18. Segurança: senhas, User Secrets e variáveis de ambiente
19. Armadilhas e distinções técnicas
20. Resumo, perguntas de revisão e mapa mental

---

## 1. Objetivo e evolução do projeto

O professor começa apresentando um problema que ainda existe no `CashFlowDbContext`.

Na configuração das aulas anteriores, a conexão com o MySQL foi escrita diretamente dentro da classe de contexto, utilizando `OnConfiguring`.

Essa abordagem permitiu testar o projeto localmente e realizar inserções no banco, mas o instrutor quer que o código fique preparado para diferentes ambientes.

### Evolução até aqui

```text
Aula 90
CashFlowDbContext + OnConfiguring + UseMySql
          |
          v
Aula 91
Primeira inserção real com Add e SaveChanges
          |
          v
Aulas 97–99
Interfaces, repositório e registro das dependências
          |
          v
Aula 100
Use case e API passam a utilizar DI
          |
          v
Aula 101
ExpensesRepository recebe CashFlowDbContext por DI
          |
          v
Aula 102
Configuração do MySQL e connection string
passa para AddDbContext e IConfiguration
```

O objetivo específico não é alterar o comportamento de negócio do registro de despesas, mas **retirar os detalhes de configuração de banco da classe CashFlowDbContext** e colocá-los na composição da aplicação.

---

## 2. Por que não deixar a connection string hardcoded?

### O que significa hardcoded?

Um valor *hardcoded* é escrito diretamente no código-fonte, em vez de ser recebido por configuração externa.

Na abordagem anterior, o contexto tinha uma configuração equivalente a:

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
{
    var connectionString =
        "Server=localhost;Database=CashFlowDB;Uid=usuario;Pwd=senha;";

    var serverVersion =
        new MySqlServerVersion(new Version(8, 0, 35));

    optionsBuilder.UseMySql(connectionString, serverVersion);
}
```

O código é um exemplo reconstruído, com credenciais fictícias. A gravação não fornece nem autoriza reproduzir valores de autenticação reais.

### Qual é o problema identificado?

O professor compara dois ambientes:

| Ambiente | Exemplo de conexão necessária |
|---|---|
| Desenvolvimento | Banco MySQL na máquina local, geralmente em `localhost` |
| Produção | Banco publicado em outro endereço, com credenciais diferentes |

Manter a conexão diretamente no código obriga a alterar ou recompilar a aplicação quando os parâmetros variam entre ambientes, se não houver outro mecanismo de configuração.

### Por que separar?

Porque a **mesma aplicação** pode ser executada em diferentes ambientes, cada um com sua própria configuração.

```text
Mesmo código do CashFlow
          |
          +-- Ambiente Development
          |      |
          |      v
          |    MySQL local
          |
          +-- Ambiente Production
                 |
                 v
               Outro banco/servidor
               e outras credenciais
```

O código de negócio não deveria depender do valor literal da conexão.

---

## 3. Onde ficam as configurações: appsettings.json e appsettings.Development.json

O professor utiliza o mecanismo de configuração do ASP.NET Core baseado em arquivos JSON.

Os nomes corretos são:

- `appsettings.json`;
- `appsettings.Development.json`.

A transcrição automática apresenta grafias como "appstudies" e "AppStudies.development". Elas se referem aos arquivos **`appsettings`** do ASP.NET Core.

### Qual é a diferença entre os dois arquivos?

**`appsettings.json`** costuma conter configurações de base que podem ser compartilhadas por diferentes ambientes.

**`appsettings.Development.json`** contém configurações específicas para quando o ambiente da aplicação é **Development**.

Em uma aplicação criada pelo modelo convencional do ASP.NET Core, as configurações são carregadas de várias fontes, e o arquivo de ambiente pode sobrescrever valores definidos no arquivo base.

### O que o professor faz?

1. Abre o projeto API.
2. Expande os arquivos `appsettings`.
3. Abre `appsettings.Development.json`.
4. Limpa o conteúdo para facilitar a demonstração.
5. Adiciona uma seção de strings de conexão.
6. Move para lá a connection string que estava fixa no código.

**Cuidado técnico:** apagar todo o conteúdo anterior de um arquivo de configuração não é uma recomendação geral. Em um projeto real, é necessário preservar outras seções já utilizadas pela aplicação. O professor faz a limpeza no projeto de demonstração.

---

## 4. Criando a seção ConnectionStrings e a chave Connection

O instrutor utiliza a seção padrão de configuração de strings de conexão do .NET.

### Exemplo seguro

```json
{
  "ConnectionStrings": {
    "Connection": "Server=localhost;Database=CashFlowDB;Uid=USUARIO_LOCAL;Pwd=SENHA_LOCAL;"
  }
}
```

**Atenção:** os valores `USUARIO_LOCAL` e `SENHA_LOCAL` são placeholders ilustrativos. Não representam credenciais fornecidas pela gravação.

### Entendendo a estrutura

| Elemento | Função |
|---|---|
| `ConnectionStrings` | Seção convencional reconhecida por `GetConnectionString` |
| `Connection` | Nome escolhido pelo professor para essa conexão |
| Valor string | Parâmetros necessários para conectar ao MySQL |

### A palavra ConnectionStrings é especial?

Sim no sentido da **convenção utilizada pelo helper** `GetConnectionString`: ele procura na seção `ConnectionStrings`.

Isso não significa que o JSON só possa conter essa seção. O sistema de configuração permite outras seções e nomes.

### A palavra Connection é obrigatória?

Não.

O instrutor explica que o nome interno pode ser escolhido livremente, desde que seja utilizado de forma consistente na leitura.

Exemplos possíveis:

```json
{
  "ConnectionStrings": {
    "Connection": "...",
    "CashFlowDatabase": "..."
  }
}
```

Nesse caso, os métodos de leitura são, respectivamente:

```csharp
configuration.GetConnectionString("Connection");
configuration.GetConnectionString("CashFlowDatabase");
```

A aula utiliza o nome **`Connection`**.

### Uma diferença que pode causar erro

A seção é `ConnectionStrings`, sem espaço, no JSON.

A expressão "Connection Strings" aparece na fala para designar o conceito, mas a chave convencional é escrita como:

```text
ConnectionStrings
```

---

## 5. Entendendo como GetConnectionString funciona

O professor demonstra no `Program.cs` uma chamada:

```csharp
builder.Configuration.GetConnectionString("Connection");
```

### O que é builder.Configuration?

É a configuração disponibilizada pelo host da aplicação ASP.NET Core.

Ela pode reunir valores de arquivos `appsettings`, variáveis de ambiente, User Secrets durante desenvolvimento e outras fontes configuradas.

### O que faz GetConnectionString?

Lê uma string específica na seção `ConnectionStrings`, usando o nome informado.

Por exemplo, esta chamada:

```csharp
var connectionString =
    builder.Configuration.GetConnectionString("Connection");
```

é conceitualmente equivalente à leitura da chave:

```text
ConnectionStrings:Connection
```

### Por que o método exige um name?

Porque uma aplicação pode possuir várias conexões.

O argumento identifica qual entrada da seção `ConnectionStrings` deve ser recuperada.

### Correção de precisão em relação à explicação oral

Em um momento, o professor descreve `GetConnectionString` como se ele recuperasse "todo o objeto ConnectionStrings".

**Tecnicamente, ele retorna o valor da entrada específica solicitada**, isto é, uma `string` ou `null`, e não o objeto inteiro da seção.

### E se a chave não existir?

Em vez de uma string válida, o resultado pode ser `null`.

Na documentação real de uma aplicação, é apropriado validar a configuração antes de construir o provider.

```csharp
var connectionString =
    configuration.GetConnectionString("Connection")
    ?? throw new InvalidOperationException(
        "A connection string 'Connection' não foi configurada.");
```

Esse tratamento é uma **recomendação técnica complementar**. O professor não o implementa explicitamente na gravação.

---

## 6. Removendo OnConfiguring do CashFlowDbContext

Depois de mover a string de conexão para `appsettings.Development.json`, o professor retorna ao `CashFlowDbContext`.

A classe ainda tinha o método:

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
```

com uma chamada a `UseMySql`.

### O que ele altera?

O professor remove essa configuração da classe do contexto e a transfere para o local em que `CashFlowDbContext` é registrado na DI, dentro da Infrastructure.

### Antes

```text
CashFlowDbContext
     |
     +-- OnConfiguring
            |
            +-- Connection string fixa
            |
            +-- MySQL server version
            |
            +-- UseMySql(...)
```

### Depois

```text
DependencyInjectionExtension
     |
     +-- AddDbContext
            |
            +-- IConfiguration
            |
            +-- UseMySql(...)
                   |
                   v
          DbContextOptions
                   |
                   v
             CashFlowDbContext
```

O contexto deixa de decidir diretamente a conexão e passa a receber opções que já foram configuradas externamente.

### Um detalhe importante

Remover `OnConfiguring` é uma **decisão da refatoração da aula**.

O Entity Framework Core também permite outros arranjos em que `OnConfiguring` e opções de DI coexistem. Entretanto, o objetivo do instrutor é concentrar os detalhes de conexão e provider fora do contexto.

---

## 7. Recebendo DbContextOptions no construtor

O professor cria um construtor para o `CashFlowDbContext`.

Esse construtor receberá as opções que o sistema de DI vai fornecer ao construir o contexto.

### Tipo utilizado

O mecanismo do EF Core utiliza `DbContextOptions`.

Para contextos registrados com `AddDbContext<TContext>`, é convencional e especialmente claro utilizar a forma genérica:

```csharp
DbContextOptions<CashFlowDbContext>
```

### Código recomendado para a reconstrução

```csharp
using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

internal class CashFlowDbContext : DbContext
{
    public CashFlowDbContext(
        DbContextOptions<CashFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; } = null!;
}
```

### Fidelidade à gravação

A transcrição menciona `DbContextOptions` oralmente, sem confirmar de maneira inequívoca se o código exibido usa `DbContextOptions` ou `DbContextOptions<CashFlowDbContext>`.

A versão genérica acima é a **forma técnica recomendada** para a configuração apresentada, mas não deve ser tomada como uma cópia literal da tela do curso.

### De onde vêm essas opções?

Do registro do contexto na coleção de serviços.

Quando o container cria `CashFlowDbContext`, o EF Core fornece as opções que foram configuradas na chamada a `AddDbContext`.

### O que essas opções podem conter?

Entre outras configurações, as opções indicam o provider escolhido e os parâmetros necessários ao funcionamento do EF Core.

O contexto não precisa conhecer o arquivo `appsettings.Development.json` para utilizá-las.

---

## 8. O papel de : base(options)

O instrutor enfatiza a sintaxe:

```csharp
: base(options)
```

Ela aparece no construtor de `CashFlowDbContext`.

### Exemplo

```csharp
public CashFlowDbContext(
    DbContextOptions<CashFlowDbContext> options)
    : base(options)
{
}
```

### O que significa base?

Em um construtor de classe derivada, `base(...)` invoca um construtor da **classe base**.

No caso:

```text
CashFlowDbContext : DbContext
        |
        v
Construtor de CashFlowDbContext
        |
        | recebe options
        v
: base(options)
        |
        v
Construtor de DbContext
        |
        v
Configuração recebida pela classe base
```

### O que isso resolve?

As opções recebidas pela DI são encaminhadas para o `DbContext` do Entity Framework Core, que as utiliza internamente.

### O corpo pode ficar vazio?

Sim.

Se o construtor não precisa realizar qualquer outra inicialização, o corpo pode ser:

```csharp
{
}
```

O professor comenta que prefere mantê-lo compacto, pois o único trabalho desse construtor é encaminhar as opções à classe base.

### Não confundir com herança de propriedades

`base(options)` é uma **chamada de construtor**, não a atribuição de uma propriedade chamada `base` nem um cast.

---

## 9. Configurando UseMySql dentro de AddDbContext

A configuração do EF Core deixa de estar no contexto e passa para a extensão da Infrastructure, dentro do método auxiliar `AddDbContext` criado na aula 101.

### Chamada anterior

```csharp
services.AddDbContext<CashFlowDbContext>();
```

### Chamada refatorada

```csharp
services.AddDbContext<CashFlowDbContext>(
    options => options.UseMySql(connectionString, serverVersion));
```

### O que mudou?

Agora `AddDbContext` recebe uma ação responsável por configurar o `DbContextOptionsBuilder` usado pelo EF Core.

Essa ação informa:

1. O provider MySQL.
2. A connection string.
3. A versão do servidor MySQL.

### A versão do servidor

Nas aulas anteriores, o professor havia demonstrado a versão de exemplo:

```csharp
var serverVersion =
    new MySqlServerVersion(new Version(8, 0, 35));
```

A versão exata depende do servidor em uso. A transcrição atual indica que o valor é reaproveitado da configuração antiga; não apresenta uma nova consulta à versão do MySQL.

### Exemplo do trecho

```csharp
var connectionString =
    configuration.GetConnectionString("Connection");

var serverVersion =
    new MySqlServerVersion(new Version(8, 0, 35));

services.AddDbContext<CashFlowDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));
```

A verificação de `null` pode ser adicionada conforme a recomendação da seção 5.

### Por que essa configuração é externa?

Porque `CashFlowDbContext` recebe as opções já construídas.

Assim, os detalhes da infraestrutura podem ser escolhidos no ponto de composição.

---

## 10. Como funciona a expressão lambda de configuração

O professor utiliza uma **expressão lambda** para configurar o contexto.

### Sintaxe

```csharp
config => config.UseMySql(connectionString, serverVersion)
```

### O que significa cada parte?

| Parte | Significado |
|---|---|
| `config` | Parâmetro da lambda, representando o builder de opções |
| `=>` | Operador de expressão lambda |
| `config.UseMySql(...)` | Configura o provider MySQL no builder |
| `connectionString` | Endereço e credenciais da conexão |
| `serverVersion` | Versão do servidor suportada pelo provider |

### O nome config é obrigatório?

Não.

Poderia ser:

```csharp
options => options.UseMySql(connectionString, serverVersion)
```

O nome da variável é uma escolha do programador.

### Qual é a função dessa lambda?

A lambda permite ao EF Core aplicar as configurações apropriadas quando as opções do contexto forem construídas.

### Comparação com o código anterior

**Antes:**

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseMySql(connectionString, serverVersion);
}
```

**Depois:**

```csharp
services.AddDbContext<CashFlowDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));
```

A ação de configurar o provider foi movida do contexto para o registro da DI.

---

## 11. Passando IConfiguration de Program.cs para a Infrastructure

Depois de configurar `UseMySql`, falta fornecer a string de conexão que está no `appsettings.Development.json`.

O professor explica que a **API** já possui acesso à configuração da aplicação através de `builder.Configuration`.

A **Infrastructure**, no entanto, precisa receber os valores necessários.

### Qual é o tipo?

```csharp
IConfiguration
```

Namespace:

```csharp
using Microsoft.Extensions.Configuration;
```

### Refatoração do método de extensão

A extensão pública passa a receber `IConfiguration` como um argumento adicional:

```csharp
public static void AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
```

### O que acontece com this?

`this` continua **apenas no primeiro parâmetro**, porque o método estende `IServiceCollection`.

`IConfiguration` é um argumento comum da função.

### No Program.cs

A chamada passa a ser:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

### Dentro da Infrastructure

A configuração é encaminhada ao método privado responsável pelo contexto:

```csharp
public static void AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    AddDbContext(services, configuration);
    AddRepositories(services);
}
```

### No método privado

```csharp
private static void AddDbContext(
    IServiceCollection services,
    IConfiguration configuration)
```

O método lê:

```csharp
configuration.GetConnectionString("Connection");
```

### Por que esse desenho é útil?

A API fornece a configuração do host, mas a Infrastructure decide quais valores precisa extrair para configurar seus serviços.

```text
Program.cs (API)
       |
       +-- builder.Configuration
       |
       v
builder.Services.AddInfrastructure(configuration)
       |
       v
DependencyInjectionExtension (Infrastructure)
       |
       v
AddDbContext(services, configuration)
       |
       v
GetConnectionString("Connection")
       |
       v
UseMySql(...)
```

---

## 12. Duas alternativas apresentadas pelo professor

O instrutor compara duas formas de passar a informação para a Infrastructure.

### Alternativa A: ler a string no Program.cs

A API pode chamar `GetConnectionString` e passar apenas a string para a extensão.

```csharp
var connectionString =
    builder.Configuration.GetConnectionString("Connection");

builder.Services.AddInfrastructure(connectionString);
```

A extensão receberia:

```csharp
public static void AddInfrastructure(
    this IServiceCollection services,
    string connectionString)
```

**Vantagem:** reduz as informações disponibilizadas à Infrastructure, que recebe apenas o dado necessário.

### Alternativa B: passar IConfiguration

A API pode entregar o objeto de configuração à extensão:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

A extensão recebe:

```csharp
public static void AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
```

**Vantagem destacada pelo instrutor:** se novos valores de configuração forem necessários no futuro, eles poderão ser lidos dentro da Infrastructure sem adicionar um novo parâmetro para cada valor.

### Qual alternativa foi escolhida?

O professor **opta por passar `IConfiguration`**.

Essa é a abordagem efetivamente adotada nesta aula.

### Existe uma escolha universalmente melhor?

Não.

Passar apenas os valores necessários pode reduzir o acoplamento à infraestrutura de configuração.

Passar `IConfiguration` simplifica a organização quando vários registros precisam ler configurações diferentes.

Também existe o *Options Pattern* do .NET para configuração tipada, mas **ele não é apresentado nem implementado nesta aula**.

---

## 13. Código completo reconstruído: arquivos envolvidos

Os exemplos a seguir reúnem o fluxo narrado no vídeo. Os namespaces foram reconstruídos com base na organização das aulas anteriores, e as credenciais foram substituídas por placeholders.

### 13.1 API — appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "Connection": "Server=localhost;Database=CashFlowDB;Uid=USUARIO_LOCAL;Pwd=SENHA_LOCAL;"
  }
}
```

O conteúdo da seção corresponde à estrutura explicada pelo professor. Em projetos reais, é recomendável **não armazenar senhas reais no repositório**, mesmo em arquivos de desenvolvimento.

### 13.2 Infrastructure — CashFlowDbContext.cs

```csharp
using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

internal class CashFlowDbContext : DbContext
{
    public CashFlowDbContext(
        DbContextOptions<CashFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Expense> Expenses { get; set; } = null!;
}
```

Principais diferenças em relação à aula 101:

- não há string de conexão literal;
- não há `UseMySql` dentro da classe;
- não há `OnConfiguring`;
- o construtor recebe as opções do contexto;
- as opções são encaminhadas com `: base(options)`.

### 13.3 Infrastructure — DependencyInjectionExtension.cs

```csharp
using CashFlow.Domain.Repositories.Expenses;
using CashFlow.Infrastructure.DataAccess;
using CashFlow.Infrastructure.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddRepositories(services);
    }

    private static void AddDbContext(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Connection")
            ?? throw new InvalidOperationException(
                "Connection string 'Connection' não configurada.");

        var serverVersion =
            new MySqlServerVersion(new Version(8, 0, 35));

        services.AddDbContext<CashFlowDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IExpensesRepository, ExpensesRepository>();
    }
}
```

**Observação de fidelidade:** a validação com `?? throw` é uma melhoria complementar incluída para deixar o exemplo seguro contra configuração ausente; ela não foi detalhada pelo professor. O restante segue a lógica demonstrada: receber `IConfiguration`, ler `Connection`, configurar o provider e registrar repositórios.

### 13.4 API — Program.cs

```csharp
using CashFlow.Application;
using CashFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Outros registros já existentes na API.

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Demais configurações e inicialização do host.
```

A mudança relevante em relação à aula 101 é a passagem de `builder.Configuration` à extensão da Infrastructure.

A ordem dos demais registros da API foi omitida por não fazer parte do foco da gravação.

### 13.5 Infrastructure — ExpensesRepository.cs

A implementação continua recebendo o contexto pelo construtor, conforme a aula 101:

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

A classe de repositório não precisa conhecer o arquivo `appsettings.Development.json`.

Também não precisa chamar `UseMySql`: sua dependência é apenas o contexto já configurado pela DI.

---

## 14. Fluxo de configuração e resolução em tempo de execução

Esta aula reúne dois fluxos diferentes que devem ser compreendidos separadamente:

1. **Leitura e configuração da infraestrutura durante a inicialização.**
2. **Resolução de dependências e persistência durante uma requisição.**

### Fluxo A — inicialização

```text
API inicia
     |
     v
Carrega IConfiguration
     |
     +-- appsettings.json
     +-- appsettings.Development.json
     +-- outras fontes configuradas
     |
     v
builder.Services.AddInfrastructure(builder.Configuration)
     |
     v
AddDbContext(services, configuration)
     |
     v
GetConnectionString("Connection")
     |
     v
services.AddDbContext<CashFlowDbContext>(
    options => options.UseMySql(connectionString, serverVersion))
     |
     v
Registro do contexto preparado
```

### Fluxo B — requisição POST

```text
Swagger envia cadastro
     |
     v
ExpensesController
     |
     v
IRegisterExpenseUseCase
     |
     v
RegisterExpenseUseCase
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
     | recebe DbContextOptions configuradas
     v
DbSet.Add(expense)
     |
     v
SaveChanges()
     |
     v
MySQL
```

### Por que são fluxos separados?

O registro configura **como os serviços poderão ser construídos**.

Já a requisição faz o container resolver os serviços necessários e executar a regra de negócio.

Registrar `AddDbContext` não é o mesmo que abrir imediatamente uma conexão com o banco.

---

## 15. Teste no debugger e no Swagger

Depois da refatoração, o professor executa a API para confirmar o funcionamento.

### Primeiro breakpoint: configuração

Ele posiciona um breakpoint no trecho de configuração da Infrastructure, antes da abertura do Swagger.

Ao iniciar o projeto:

1. a execução para no método de configuração;
2. o professor inspeciona a variável de connection string;
3. confirma que ela foi recuperada da configuração do ambiente;
4. verifica que o texto contém informações de conexão com `localhost`;
5. continua a configuração de provider e versão;
6. retoma a execução até o Swagger ficar disponível.

### Qual é a conclusão desse teste?

A chamada a `GetConnectionString("Connection")` recuperou a configuração esperada e conseguiu ser utilizada na criação das opções do contexto.

**Cuidado:** em projetos reais, inspecionar ou exibir connection strings completas pode expor senhas. Não é recomendável copiá-las para logs, screenshots ou relatórios.

### Segundo breakpoint: persistência

O professor adiciona outro breakpoint no método do repositório que salva despesas.

### Dados informados

| Campo | Valor da demonstração |
|---|---|
| Título | TV |
| Descrição | Texto não especificado na transcrição |
| Valor | 1200 |
| Tipo de pagamento | 1 |
| Data | Não especificada na transcrição |

### Execução

1. Abre o endpoint de registro de despesa no Swagger.
2. Clica em **Try it out**.
3. Preenche os campos com os valores de demonstração.
4. Executa a requisição.
5. O depurador para no método `Add` do repositório.
6. O professor inspeciona `_dbContext` e confirma que a instância foi injetada.
7. Executa as operações de inclusão e persistência.
8. Inspeciona o identificador da entidade.

### Resultado da gravação

O professor confirma que a configuração externa do MySQL foi suficiente para manter a persistência funcionando.

Não há, na gravação, uma comparação de desempenho ou medição de memória. O teste demonstra **funcionamento do novo fluxo de configuração**.

---

## 16. Id 0 antes e Id 29 depois do SaveChanges

O professor acompanha o identificador da entidade durante a persistência.

### Antes

```text
Expense.Id = 0
```

### Depois de SaveChanges

```text
Expense.Id = 29
```

### Por que o valor muda?

No cenário demonstrado, a tabela de despesas utiliza uma estratégia de geração de chave pelo banco.

A entidade recém-criada ainda não tem seu identificador persistido.

Quando o EF Core executa a inserção e obtém a chave gerada pelo banco, a instância rastreada pode ser atualizada com o valor definitivo.

```text
Expense criada
      |
      v
Id = 0
      |
      v
_dbContext.Expenses.Add(expense)
      |
      v
_dbContext.SaveChanges()
      |
      v
MySQL gera chave
      |
      v
Id = 29
```

### Comparação com as aulas anteriores

| Aula | Registro observado |
|---|---|
| 100 | ID 27 |
| 101 | ID 28 |
| 102 | ID 29 |

A sequência corresponde aos testes narrados nas respectivas gravações.

### O que o teste prova?

Que a despesa foi persistida com sucesso no cenário executado e que a mudança da origem das opções do contexto não interrompeu o cadastro.

**Precisão:** `Id = 0` antes de `SaveChanges` é uma observação dessa entidade e configuração. Não deve ser interpretado como uma regra absoluta de todos os tipos de chave ou de todas as estratégias de geração de valores no EF Core.

---

## 17. Diferença entre connection string dinâmica e provider dinâmico

O professor comenta que organizar a configuração pelo `AddDbContext` abre a possibilidade de utilizar outros providers, como SQL Server ou SQLite.

### O que a aula implementa de fato?

Configuração do **MySQL** com uma connection string recuperada externamente:

```csharp
options.UseMySql(connectionString, serverVersion)
```

### O que isso permite?

A mesma aplicação pode ler uma string de conexão diferente conforme o ambiente.

```text
Development
ConnectionStrings:Connection -> MySQL local

Production
ConnectionStrings:Connection -> MySQL remoto
```

### O que não acontece automaticamente?

Essa implementação **não muda o provider conforme o ambiente**, porque a chamada continua explicitamente sendo `UseMySql`.

Se o sistema precisasse alternar entre MySQL, SQL Server ou SQLite, seria necessário implementar uma seleção condicional e incluir os providers correspondentes.

### Exemplo apenas conceitual de alternativa

```csharp
if (provider == "MySql")
{
    options.UseMySql(connectionString, serverVersion);
}
else if (provider == "SqlServer")
{
    options.UseSqlServer(connectionString);
}
```

Esse código **não faz parte da aula** e exigiria referências e configurações adequadas para cada provider, além de atenção a diferenças de migração, mapeamento e comportamento.

### Versão do servidor MySQL

Na demonstração, o professor reaproveita a especificação de versão do MySQL.

Não devemos generalizar a frase de que toda configuração SQL Server exigiria exatamente os mesmos parâmetros: cada provider disponibiliza sua API própria.

---

## 18. Segurança: senhas, User Secrets e variáveis de ambiente

Esta seção é um **complemento de boas práticas**. O professor ensina a externalizar a connection string para o arquivo de configuração, mas não apresenta um gerenciamento completo de segredos nesta gravação.

### Um arquivo JSON ainda pode conter dados sensíveis

Mover uma senha do `OnConfiguring` para `appsettings.Development.json` melhora a separação entre código e configuração, mas **não garante que as credenciais estejam seguras**.

Se o arquivo contendo a senha for versionado em um repositório, a credencial poderá ser exposta.

### Recomendações

- Não incluir senhas reais de produção no repositório de código.
- Utilizar valores fictícios ou placeholders nos exemplos.
- Em desenvolvimento, considerar o **.NET User Secrets**.
- Em implantação, usar variáveis de ambiente ou um mecanismo seguro de gerenciamento de segredos.
- Não registrar connection strings com senha em logs.
- Usar contas de banco com privilégios necessários e limitados.
- Não reutilizar credenciais de desenvolvimento em produção.

### Exemplo com variável de ambiente

A hierarquia de configuração do ASP.NET Core pode mapear o separador `__` (dois underscores) para seções hierárquicas.

A chave equivalente à connection string do exemplo é:

```text
ConnectionStrings__Connection
```

Essa variável pode fornecer o valor de `ConnectionStrings:Connection` quando o provider de variáveis de ambiente estiver presente na configuração do host.

### O código de leitura permanece igual

```csharp
configuration.GetConnectionString("Connection");
```

Assim, a Application/Infrastructure não precisa saber se a informação veio de um arquivo JSON, variável de ambiente ou outra fonte.

### Segurança também inclui tratamento de erros

Se a connection string estiver ausente ou incorreta, a aplicação deve falhar com uma mensagem útil **sem expor segredos**.

Não é adequado enviar senha ou connection string completa ao cliente na resposta de erro.

---

## 19. Armadilhas e distinções técnicas

### 19.1 appsettings.Development.json depende do ambiente

O arquivo específico será carregado pelo fluxo padrão quando o nome do ambiente corresponder ao arquivo.

Não basta criar o arquivo: o host precisa estar configurado para executar naquele ambiente, e a fonte precisa integrar a configuração.

### 19.2 GetConnectionString retorna uma string, não a seção inteira

```csharp
configuration.GetConnectionString("Connection")
```

consulta a entrada `ConnectionStrings:Connection`.

O resultado pode ser `null` se ela não estiver configurada.

### 19.3 O nome da seção e o nome da conexão têm papéis diferentes

`ConnectionStrings` é a seção convencional utilizada pelo helper.

`Connection` é o nome arbitrário escolhido no vídeo.

### 19.4 IConfiguration não é a própria connection string

`IConfiguration` representa uma **abstração de configuração**, a partir da qual valores podem ser obtidos.

A connection string é apenas um dos dados que podem ser lidos.

### 19.5 this só aparece no parâmetro receptor da extensão

```csharp
public static void AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
```

O argumento `IConfiguration` é passado normalmente.

Não é necessário nem adequado utilizar `this` em ambos os parâmetros.

### 19.6 DbContextOptions configura o DbContext, não cria uma cópia do provider

O `DbContextOptions` concentra os parâmetros usados para construir/configurar o contexto.

A lambda que chama `UseMySql` informa ao EF Core qual provider utilizar.

### 19.7 : base(options) executa o construtor da classe base

A sintaxe não é exclusiva do EF Core.

Ela pertence à linguagem C# e pode ser usada ao herdar de uma classe que possua construtor compatível.

### 19.8 A injeção do contexto continua Scoped

A refatoração não altera a escolha de ciclo de vida da aula 101: `AddDbContext`, na configuração convencional, registra o contexto como Scoped por padrão.

A configuração externa de provider não muda automaticamente esse comportamento.

### 19.9 OnConfiguring pode coexistir com DI, mas foi removido aqui

O professor escolhe retirar `OnConfiguring` para centralizar as opções.

Isso não implica que o EF Core proíba qualquer uso de `OnConfiguring` quando se configura o contexto via DI.

### 19.10 A ordem de registro é distinta da ordem de resolução

Na inicialização, serviços são registrados na coleção.

Posteriormente, o container resolve o grafo de dependências quando necessário.

O professor não demonstra a abertura imediata de uma conexão com MySQL no momento em que escreve `AddDbContext`.

### 19.11 Connection string dinâmica não é alteração automática de banco de dados

Se a connection string aponta para outro servidor MySQL compatível, o provider continua sendo MySQL.

Trocar para SQL Server ou SQLite requer alterações adicionais.

### 19.12 A versão 8.0.35 vem do cenário anterior do curso

A aula 90 utilizou essa versão como representação do servidor MySQL.

A versão precisa ser compatível com o servidor e provider efetivamente utilizados.

### 19.13 Não confundir configuração da API com arquivos da Infrastructure

O arquivo `appsettings.Development.json` pertence ao projeto API na demonstração.

A Infrastructure **recebe um objeto `IConfiguration` passado pela API**, e não lê diretamente um arquivo por caminho fixo.

Isso ajuda a manter a Infrastructure independente do local físico do arquivo de configuração.

### 19.14 A alteração não exige refazer os contratos de Domain

As interfaces `IExpensesRepository` e `IRegisterExpenseUseCase` permanecem como estavam.

A refatoração acontece na configuração e construção do contexto de persistência.

### 19.15 Não há teste automatizado novo na gravação

O professor testa manualmente com breakpoints e Swagger.

Não é correto apresentar essa etapa como um teste xUnit, pois nenhuma nova suíte de testes é criada neste vídeo.

### 19.16 Não confundir o Id gerado com o retorno HTTP

O valor do `Id` é apresentado pelo depurador após a persistência.

O valor de resposta HTTP não é detalhado na transcrição atual, de modo que não é necessário inventar um status específico para este teste.

---

## 20. Resumo, perguntas de revisão e mapa mental

### Principais aprendizados

- A connection string estava **hardcoded** dentro de `CashFlowDbContext`.
- O professor quer suportar configurações diferentes em desenvolvimento e produção.
- No projeto API, ele utiliza `appsettings.Development.json`.
- A seção convencional criada é `ConnectionStrings`.
- A chave escolhida para a conexão é `Connection`.
- A API disponibiliza suas configurações através de `builder.Configuration`.
- `GetConnectionString("Connection")` lê a entrada correspondente.
- A implementação de `OnConfiguring` é removida de `CashFlowDbContext`.
- O contexto passa a receber `DbContextOptions` pelo construtor.
- A sintaxe `: base(options)` encaminha as opções ao `DbContext` do EF Core.
- `UseMySql` passa a ser chamado dentro do registro `AddDbContext`.
- A configuração é feita com uma expressão lambda.
- A extensão `AddInfrastructure` recebe também `IConfiguration`.
- O professor prefere passar `IConfiguration`, em vez de apenas uma string.
- O `Program.cs` chama `builder.Services.AddInfrastructure(builder.Configuration)`.
- A string de conexão recuperada é validada visualmente no debugger.
- O teste pelo Swagger cadastra uma despesa chamada **TV**, com valor **1200** e pagamento **1**.
- A entidade recebe **Id 29** após a persistência.
- O código passa a ser configurável por ambiente, sem manter a connection string literal na classe de contexto.
- A implementação **continua usando MySQL**; a troca automática de providers não foi implementada.
- O material complementa a aula com cuidados para não versionar senhas reais.

### Quadro comparativo: antes e depois

| Característica | Antes (aula 101) | Depois (aula 102) |
|---|---|---|
| Local da connection string | `CashFlowDbContext.OnConfiguring` | Configuração da API |
| Provider | `UseMySql` dentro do contexto | `UseMySql` na extensão da Infrastructure |
| Fonte da conexão | String literal no código | `IConfiguration` |
| Leitura de ambientes | Não utilizada para a conexão | `appsettings.Development.json` |
| Construtor do contexto | Sem recebimento das opções demonstrado | Recebe `DbContextOptions` |
| Chamada à classe base | Não destacada | `: base(options)` |
| Registro do contexto | `AddDbContext<T>()` | `AddDbContext<T>(options => ...)` |
| Chamada da API | `AddInfrastructure()` | `AddInfrastructure(builder.Configuration)` |
| Persistência | Funcionando | Continua funcionando |
| Provider automático por ambiente | Não | Ainda não |

### Perguntas para revisão

1. O que significa uma connection string hardcoded?
2. Por que desenvolvimento e produção costumam precisar de conexões diferentes?
3. Qual arquivo o professor utiliza para a configuração de desenvolvimento?
4. Qual é a grafia correta de `appsettings.Development.json`?
5. Qual seção do JSON é utilizada por `GetConnectionString`?
6. Por que `Connection` precisa corresponder ao nome passado ao helper?
7. `GetConnectionString` recupera a seção inteira ou uma entrada específica?
8. Por que o professor remove `OnConfiguring` do contexto?
9. Qual objeto o construtor do contexto passa a receber?
10. O que faz `: base(options)`?
11. Onde `UseMySql` passa a ser chamado?
12. Qual é a finalidade da lambda de configuração?
13. Como a API fornece `IConfiguration` à Infrastructure?
14. Qual é a diferença entre passar uma string e passar `IConfiguration`?
15. Qual alternativa o professor adota nesta aula?
16. O código já alterna automaticamente entre SQL Server, SQLite e MySQL?
17. Que riscos existem em versionar uma connection string com senha?
18. Quais dados de despesa foram usados no Swagger?
19. Qual Id foi observado após `SaveChanges`?
20. Essa mudança altera as interfaces do Domain ou a lógica de negócio do use case?

### Mapa mental

```text
                                AULA 102
                       CONNECTION STRING DINÂMICA
                                    |
                         ANTES: CONFIGURAÇÃO FIXA
                                    |
                           CashFlowDbContext
                                    |
                              OnConfiguring
                                    |
                       String literal + UseMySql
                                    |
                                    v
                             REFATORAÇÃO
                                    |
                    appsettings.Development.json
                                    |
                        ConnectionStrings:Connection
                                    |
                                    v
                            builder.Configuration
                                    |
                                    v
                  builder.Services.AddInfrastructure(
                         builder.Configuration)
                                    |
                                    v
                         DependencyInjectionExtension
                                    |
                           AddDbContext(services,
                               configuration)
                                    |
                      GetConnectionString("Connection")
                                    |
                                    v
                       options => options.UseMySql(...)
                                    |
                                    v
                          DbContextOptions
                                    |
                                    v
                      CashFlowDbContext(options)
                                    |
                               : base(options)
                                    |
                                    v
                           ExpensesRepository
                                    |
                                    v
                           SaveChanges() / MySQL
                                    |
                                    v
                          Expense TV: Id = 29
```

**Conclusão:** a aula 102 transfere a configuração do MySQL do `OnConfiguring` de `CashFlowDbContext` para a extensão de DI da Infrastructure. A connection string é lida de `ConnectionStrings:Connection` através de `IConfiguration`, o provider é configurado com `UseMySql` na chamada a `AddDbContext`, e o contexto recebe as opções pelo construtor com `: base(options)`. O teste no Swagger confirma a persistência de uma despesa de exemplo, que recebe **Id 29** após `SaveChanges`.
