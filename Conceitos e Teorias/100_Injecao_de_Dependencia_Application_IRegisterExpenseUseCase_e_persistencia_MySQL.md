# Integrando repositório e use case com injeção de dependência no CashFlow

> **Aula 100 — IRegisterExpenseUseCase, AddApplication, FromServices e teste real de persistência no MySQL**
>
> Continuação da aula 99, em que a Infrastructure passou a registrar `IExpensesRepository` com `ExpensesRepository` através do método de extensão `AddInfrastructure()`. Nesta aula, o professor injeta o repositório no caso de uso, registra o próprio use case no container de DI, atualiza o controller e valida o fluxo completo por meio do Swagger e do MySQL Workbench.
>
> **Escopo real da gravação:** o professor demonstra uma inserção bem-sucedida no banco e anuncia melhorias para a aula seguinte. O registro do `CashFlowDbContext` diretamente no container de DI, o descarte desse contexto e as futuras refatorações **não são realizados nesta aula**.

## Índice

1. Objetivo e conexão com as aulas anteriores
2. Situação inicial da arquitetura
3. Por que o use case não acessa ExpensesRepository?
4. Injetando IExpensesRepository pelo construtor
5. Usando um campo private readonly
6. Chamando Add após validação e mapeamento
7. Por que a instanciação manual do use case passa a falhar?
8. Criando IRegisterExpenseUseCase
9. Implementando o contrato na classe concreta
10. Criando AddApplication na camada Application
11. Instalando Microsoft.Extensions.DependencyInjection
12. Atualizando o controller com FromServices
13. Registrando AddApplication no Program.cs
14. Fluxo completo de resolução das dependências
15. Teste prático no Swagger
16. Conferência do registro no MySQL
17. Código reconstruído da solução
18. Cuidados técnicos e boas práticas
19. Resumo, perguntas e mapa mental

---

## 1. Objetivo e conexão com as aulas anteriores

O professor começa afirmando que chegou a hora de verificar se o conjunto de configurações apresentado nas últimas aulas realmente funciona.

O objetivo é conectar os componentes já construídos:

- A **entidade `Expense`**, criada na camada Domain.
- O contrato **`IExpensesRepository`**, também no Domain.
- A classe **`ExpensesRepository`**, localizada na Infrastructure.
- O **`CashFlowDbContext`**, responsável pelo acesso ao banco com Entity Framework Core.
- O use case **`RegisterExpenseUseCase`**, responsável pelo fluxo de cadastro.
- O **`ExpensesController`**, responsável por receber a requisição HTTP.
- O container de DI configurado no `Program.cs`.

### O que já estava pronto?

| Aula | Evolução |
|---|---|
| 90 | Configuração do `CashFlowDbContext` e do MySQL |
| 91 | Primeiro teste de `Add` e `SaveChanges`, com dependência temporária |
| 92–96 | Fundamentos de DI, ciclos de vida e `[FromServices]` |
| 97 | Criação do contrato `IExpensesRepository` |
| 98 | Implementação concreta de `ExpensesRepository` |
| 99 | Registro `IExpensesRepository` → `ExpensesRepository` via `AddInfrastructure()` |
| 100 | Integração com Application e controller, seguida de teste real |

### O objetivo arquitetural

O use case deve trabalhar com uma **interface de repositório**, sem criar ou depender diretamente de uma implementação concreta da Infrastructure.

A API deve pedir ao container uma **interface de use case**, em vez de instanciá-lo manualmente.

```text
HTTP POST
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
   v
MySQL
```

A separação entre contratos e implementações é a base da refatoração demonstrada.

---

## 2. Situação inicial da arquitetura

Na aula anterior, a classe concreta do repositório já havia sido registrada no método `AddInfrastructure()`.

### Registro na Infrastructure

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

O método público `AddInfrastructure()` encapsula esse registro, mantendo `ExpensesRepository` como `internal`.

### Ponto de composição na API

```csharp
builder.Services.AddInfrastructure();
```

O `Program.cs` já sabia **como configurar a Infrastructure**.

O que ainda faltava era:

1. Fazer o use case receber `IExpensesRepository`.
2. Registrar o próprio use case no container de DI.
3. Alterar o controller para solicitar o use case pelo container.
4. Executar uma requisição e comprovar que a despesa chegava ao banco.

### Por que isso importa?

O projeto tem várias camadas com responsabilidades diferentes. O objetivo não é apenas fazer um `INSERT` funcionar, mas permitir que a persistência seja acessada sem violar a direção das dependências arquiteturais.

---

## 3. Por que o use case não acessa ExpensesRepository?

O professor abre `RegisterExpenseUseCase` e explica um problema.

Ele gostaria de utilizar a classe `ExpensesRepository`, mas essa classe foi declarada como `internal` dentro da Infrastructure.

### Consequência

A Application não consegue mencionar diretamente a classe concreta:

```csharp
// O uso direto da implementação não é desejado:
var repository = new ExpensesRepository();
```

Isso se deve à acessibilidade do tipo e à separação entre projetos.

### O contrato está acessível

A interface `IExpensesRepository` está no **Domain** e foi declarada como `public`.

A Application possui referência para o Domain, então consegue utilizar a interface:

```csharp
IExpensesRepository repository;
```

### A solução é receber a interface

Em vez de criar uma instância do repositório concreto, o use case declara no construtor que precisa de uma implementação de `IExpensesRepository`.

O container de DI, configurado na aula 99, se encarrega de fornecer `ExpensesRepository`.

```text
Application
    |
    | conhece
    v
IExpensesRepository (Domain, public)
    ^
    | implementa
    |
ExpensesRepository (Infrastructure, internal)
```

### O papel do internal

O fato de a classe concreta ser `internal` impede seu uso direto por código convencional de outros assemblies.

Entretanto, **não impede que um objeto desse tipo seja fornecido a consumidores através de uma interface pública**.

É essa a distinção que o professor enfatiza na aula.

---

## 4. Injetando IExpensesRepository pelo construtor

O professor cria um construtor em `RegisterExpenseUseCase`.

Ele utiliza o atalho do Visual Studio para gerar o construtor e adiciona um parâmetro do tipo `IExpensesRepository`.

### Primeiro passo

```csharp
public RegisterExpenseUseCase(IExpensesRepository repository)
{
}
```

O parâmetro `repository` representa a dependência necessária para salvar despesas.

### Por que isso é injeção pelo construtor?

Porque a instância é recebida **de fora da classe** durante sua construção.

O use case não decide como construir `ExpensesRepository`, nem precisa conhecer seus detalhes internos.

### Antes e depois

**Antes — dependência direta de infraestrutura (abordagem temporária da aula 91):**

```csharp
var dbContext = new CashFlowDbContext();
```

**Depois — dependência de um contrato:**

```csharp
public RegisterExpenseUseCase(IExpensesRepository repository)
```

A construção da implementação concreta é responsabilidade da composição/DI da aplicação.

---

## 5. Usando um campo private readonly

O professor quer utilizar a dependência recebida no construtor dentro do método `Execute`.

Por isso, cria um **campo privado somente leitura**.

### Código

```csharp
private readonly IExpensesRepository _repository;
```

No construtor, armazena a referência:

```csharp
public RegisterExpenseUseCase(IExpensesRepository repository)
{
    _repository = repository;
}
```

### O que significa cada parte?

| Trecho | Responsabilidade |
|---|---|
| `private` | O campo só pode ser acessado diretamente dentro da classe |
| `readonly` | Impede reatribuição do campo após as fases permitidas de inicialização |
| `IExpensesRepository` | Tipo abstrato usado pelo consumidor |
| `_repository` | Campo que armazena a dependência recebida |

### Por que readonly?

O professor explica que deseja atribuir essa dependência apenas no momento da construção do objeto.

Esse é um uso comum de `readonly`: indicar que a referência a um colaborador deve permanecer a mesma durante a vida da instância.

**Nuance:** `readonly` impede a **reatribuição do campo** fora dos pontos permitidos, mas **não torna imutável o objeto referenciado**.

### Convenção do sublinhado

O professor utiliza o nome `_repository`, com sublinhado inicial, para o campo privado.

Trata-se de uma convenção de estilo muito utilizada em C#, não de uma imposição da linguagem.

### Campo versus propriedade

A transcrição às vezes chama o membro de "propriedade privada". No código descrito — `private readonly IExpensesRepository _repository;` — trata-se tecnicamente de um **campo**, pois não possui acessores `get`/`set`.

---

## 6. Chamando Add após validação e mapeamento

Com `_repository` disponível, o professor altera o método `Execute` do caso de uso.

A gravação mostra que o fluxo:

1. recebe a requisição;
2. executa a validação;
3. cria a entidade `Expense`;
4. chama a operação de persistência através da interface.

### Operação acrescentada ao Execute

```csharp
_repository.Add(entity);
```

A variável `entity` representa a entidade de despesa construída a partir da requisição.

### Qual é a responsabilidade da interface nesse ponto?

O use case **não conhece** o método `DbSet<Expense>.Add` nem chama `SaveChanges` diretamente.

Ele conhece apenas:

```csharp
void Add(Expense expense);
```

A implementação `ExpensesRepository` é responsável por interagir com o Entity Framework Core.

### Fluxo

```text
Request de registro
        |
        v
RegisterExpenseUseCase.Execute
        |
        v
Validar Request
        |
        v
Construir Expense
        |
        v
_repository.Add(entity)
        |
        v
ExpensesRepository.Add
        |
        v
DbContext -> SaveChanges -> MySQL
```

### A validação continua sendo importante

O professor não elimina a validação já construída com FluentValidation.

A entidade continua sendo criada **após a validação da requisição**.

O repositório, conforme explicado na aula 98, recebe uma entidade considerada pronta para persistência.

---

## 7. Por que a instanciação manual do use case passa a falhar?

Depois de adicionar o construtor com `IExpensesRepository`, o professor verifica onde o método `Execute` é utilizado.

O Visual Studio o direciona ao `ExpensesController`.

### Código anterior do controller

O fluxo anterior criava o caso de uso diretamente:

```csharp
var useCase = new RegisterExpenseUseCase();
```

### O que mudou?

Agora existe um construtor que exige um parâmetro:

```csharp
public RegisterExpenseUseCase(IExpensesRepository repository)
```

Consequentemente, o `new` sem argumentos deixa de ser válido.

### Seria possível passar uma implementação manualmente?

Em princípio, seria possível construir manualmente um repositório compatível e fornecê-lo ao construtor, respeitando a acessibilidade dos tipos.

Entretanto, **não é essa a arquitetura desejada no curso**.

O professor quer que o serviço de DI resolva a dependência.

### Consequência

Será necessário registrar **o próprio use case** no container.

Isso cria uma cadeia de resolução:

```text
Controller solicita use case
         |
         v
DI cria RegisterExpenseUseCase
         |
         v
Construtor exige IExpensesRepository
         |
         v
DI resolve ExpensesRepository
```

A configuração do repositório já existe, mas ainda falta a configuração do caso de uso.

---

## 8. Criando IRegisterExpenseUseCase

O professor explica que também prefere definir interfaces para os use cases.

### Localização

Ele cria a nova interface **na mesma pasta do caso de uso de registro de despesas**.

Essa pasta já contém:

- a classe `RegisterExpenseUseCase`;
- o validador;
- a nova interface `IRegisterExpenseUseCase`.

### Organização conceitual

```text
CashFlow.Application
└── UseCases
    └── Expenses
        └── Register
            ├── RegisterExpenseUseCase.cs
            ├── RegisterExpenseValidator.cs
            └── IRegisterExpenseUseCase.cs
```

Os segmentos exatos da árvore de pastas devem ser conferidos no projeto. A gravação confirma que os três arquivos ficam juntos, mas não fornece a árvore completa de namespaces.

### Modificador de acesso

O professor altera a interface para:

```csharp
public interface IRegisterExpenseUseCase
```

Isso é necessário porque o controller da API passará a declarar a dependência usando esse tipo.

### O método declarado

A interface recebe a assinatura do método `Execute` existente na classe concreta.

Ela deve declarar:

- o tipo de resposta;
- o nome `Execute`;
- o parâmetro da requisição;
- ponto e vírgula, sem corpo de implementação.

### Exemplo reconstruído

```csharp
public interface IRegisterExpenseUseCase
{
    ResponseRegisterExpenseJson Execute(RequestRegisterExpenseJson request);
}
```

**Sobre os nomes:** `IRegisterExpenseUseCase`, `RequestRegisterExpenseJson` e `ResponseRegisterExpenseJson` são as grafias utilizadas nesta documentação para representar os tipos do fluxo. A transcrição não exibe literalmente a declaração completa, então os nomes e namespaces reais devem ser confirmados no código do curso.

### Por que uma interface para o use case?

O controller poderá depender de um **contrato da Application**, em vez de precisar citar uma implementação concreta.

O resultado é uma separação adicional entre a entrada HTTP e a execução da regra de negócio.

---

## 9. Implementando o contrato na classe concreta

Depois de criar a interface, o professor tenta registrar a associação no container.

O Visual Studio apresenta um erro: a classe `RegisterExpenseUseCase` ainda não declara que implementa `IRegisterExpenseUseCase`.

### Correção

Ele altera a declaração:

```csharp
public class RegisterExpenseUseCase : IRegisterExpenseUseCase
```

Agora o compilador consegue verificar que a implementação satisfaz o contrato.

### Por que isso é necessário?

O registro genérico:

```csharp
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

exige que `RegisterExpenseUseCase` possa ser utilizado como implementação de `IRegisterExpenseUseCase`.

Se a classe não satisfizer o contrato, o compilador acusa incompatibilidade de tipos genéricos.

### O método Execute

Como a classe já tinha o método `Execute`, não é necessário duplicar sua lógica; basta que sua assinatura seja compatível com a interface.

### Visualização

```text
IRegisterExpenseUseCase
    |
    | exige Execute(Request)
    v
RegisterExpenseUseCase
    |
    | implementa o contrato
    v
Validação + mapeamento + repositório
```

---

## 10. Criando AddApplication na camada Application

O professor reutiliza o padrão da aula 99.

Na aula anterior, foi criada a classe de extensão de DI da Infrastructure:

```csharp
builder.Services.AddInfrastructure();
```

Agora ele cria uma configuração correspondente na **Application**.

### Nome da classe

O professor cria, na raiz do projeto Application, uma classe auxiliar chamada **DependencyInjectionExtension**.

Ela pode ter o mesmo nome da classe existente na Infrastructure porque são tipos localizados em namespaces/assemblies diferentes.

### Passos

1. Adicionar uma classe no projeto Application.
2. Alterar sua visibilidade para `public`.
3. Torná-la `static`.
4. Criar um método `public static void AddApplication`.
5. Receber `this IServiceCollection services`.
6. Registrar a interface de use case e sua implementação.

### Código reconstruído

```csharp
using CashFlow.Application.UseCases.Expenses.Register;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<
            IRegisterExpenseUseCase,
            RegisterExpenseUseCase>();
    }
}
```

O namespace dos use cases é ilustrativo, pois a transcrição não informa a declaração completa do arquivo.

### Qual é o objetivo?

Centralizar dentro do próprio projeto Application os registros que pertencem à Application.

O `Program.cs` da API passa a precisar chamar apenas um método de configuração:

```csharp
builder.Services.AddApplication();
```

### Por que o método é uma extensão?

A classe e o método são estáticos, e o primeiro parâmetro utiliza `this`:

```csharp
this IServiceCollection services
```

Isso permite chamar o método com a sintaxe `builder.Services.AddApplication()`, embora ele continue sendo um método estático de `DependencyInjectionExtension`.

---

## 11. Instalando Microsoft.Extensions.DependencyInjection

Durante a implementação, o instrutor observa que o projeto Application não encontra automaticamente o namespace necessário para utilizar `IServiceCollection` e seus métodos de registro.

### Procedimento descrito

1. Abrir **Manage NuGet Packages** no projeto Application.
2. Acessar a aba **Browse**.
3. Procurar por **Microsoft.Extensions.DependencyInjection**.
4. Selecionar o pacote.
5. Instalar e confirmar as alterações.
6. Importar o namespace apropriado no arquivo.

### Namespace utilizado

```csharp
using Microsoft.Extensions.DependencyInjection;
```

### Por que a Application precisa dessa referência?

Porque o novo método de extensão será compilado **dentro do projeto Application**, e ele utiliza:

- a interface `IServiceCollection`;
- o método `AddScoped`.

O projeto precisa ter as referências corretas para esses tipos e extensões.

### Distinção técnica

Um `using` disponibiliza nomes no arquivo de código, mas não instala um pacote NuGet.

Por outro lado, a instalação do pacote fornece as referências necessárias, porém o namespace ainda precisa estar disponível no escopo do código.

Na demonstração, o professor instala o pacote e o editor passa a oferecer a correção de `using`.

**Nota:** a versão exata do pacote não é informada na transcrição. Em projetos reais, é importante manter a compatibilidade com a versão do .NET utilizada na solução.

---

## 12. Atualizando o controller com FromServices

Com o use case registrado e sua interface criada, o professor retorna ao `ExpensesController`.

Ele remove a instanciação manual:

```csharp
var useCase = new RegisterExpenseUseCase();
```

E passa a declarar a dependência diretamente nos parâmetros da ação.

### Padrão utilizado

```csharp
[FromServices] IRegisterExpenseUseCase useCase
```

Esse mecanismo foi estudado na aula 96.

### Exemplo reconstruído da ação de registro

```csharp
[HttpPost]
public IActionResult Register(
    [FromBody] RequestRegisterExpenseJson request,
    [FromServices] IRegisterExpenseUseCase useCase)
{
    var response = useCase.Execute(request);

    return Created(string.Empty, response);
}
```

**Importante:** o código acima é um exemplo didático da estrutura descrita. A transcrição não fornece o nome literal do método da ação, os atributos completos, a URL de localização nem a implementação precisa da resposta. O que está confirmado é a substituição do `new` por `[FromServices]` e a continuação da resposta HTTP 201.

### Responsabilidades dos parâmetros

| Parâmetro | Origem |
|---|---|
| `RequestRegisterExpenseJson request` | Dados da requisição HTTP |
| `IRegisterExpenseUseCase useCase` | Container de injeção de dependência |

### Por que usar a interface?

O controller depende de `IRegisterExpenseUseCase`, não da classe concreta `RegisterExpenseUseCase`.

O container fornece a instância correta com base na configuração da Application.

### O que muda no fluxo?

```text
ANTES
Controller
    |
    v
new RegisterExpenseUseCase()


DEPOIS
Controller
    |
    v
[FromServices] IRegisterExpenseUseCase
    |
    v
Container DI
    |
    v
RegisterExpenseUseCase
```

---

## 13. Registrando AddApplication no Program.cs

O professor observa que ainda falta chamar a configuração da Application durante a inicialização da API.

### Configuração já existente

Da aula 99, temos:

```csharp
builder.Services.AddInfrastructure();
```

### Configuração acrescentada agora

```csharp
builder.Services.AddApplication();
```

### Trecho conceitual do Program.cs

```csharp
using CashFlow.Application;
using CashFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Outros registros da API.

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

// Demais etapas de construção e inicialização.
```

A transcrição não apresenta a ordem completa de todas as configurações do arquivo. O trecho acima mostra somente a integração relevante.

### Por que os dois registros são necessários?

**Infrastructure:**

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

**Application:**

```csharp
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

Se um controller solicitar `IRegisterExpenseUseCase`, o container precisa resolver `RegisterExpenseUseCase`.

Mas essa classe, por sua vez, exige `IExpensesRepository` no construtor.

Por isso, a composição precisa conhecer **os dois registros**.

### Importando o namespace

O professor comenta que o Visual Studio não sugeriu automaticamente o `using` da Application e que ele precisou escrevê-lo manualmente.

```csharp
using CashFlow.Application;
```

O editor também é utilizado para organizar os `using` e remover importações não utilizadas.

---

## 14. Fluxo completo de resolução das dependências

Agora podemos conectar todas as peças da DI.

### Cadeia de resolução

```text
1. Cliente executa POST /expenses
                |
                v
2. ASP.NET Core seleciona ExpensesController
                |
                v
3. Endpoint pede IRegisterExpenseUseCase
                |
                v
4. Container resolve RegisterExpenseUseCase
                |
                v
5. Construtor exige IExpensesRepository
                |
                v
6. Container resolve ExpensesRepository
                |
                v
7. RegisterExpenseUseCase recebe o repositório
                |
                v
8. Execute(request) valida os dados
                |
                v
9. Cria Expense a partir da Request
                |
                v
10. _repository.Add(entity)
                |
                v
11. ExpensesRepository usa CashFlowDbContext
                |
                v
12. DbSet.Add + SaveChanges
                |
                v
13. MySQL recebe a despesa
                |
                v
14. Endpoint devolve resposta HTTP 201
```

### Os dois registros

```text
IRegisterExpenseUseCase
        |
        +--> RegisterExpenseUseCase
                  |
                  +--> IExpensesRepository
                                |
                                +--> ExpensesRepository
                                             |
                                             +--> CashFlowDbContext
                                                          |
                                                          +--> MySQL
```

### O que o container cria neste estágio?

O container fornece a instância de `RegisterExpenseUseCase` e resolve sua dependência `IExpensesRepository`.

Na implementação que veio da aula 98, `ExpensesRepository.Add` **ainda cria `CashFlowDbContext` manualmente com `new`**.

Portanto, a demonstração **não significa que todos os `new` do fluxo desapareceram**.

### Onde se encaixa o Scoped?

Os dois registros apresentados usam `AddScoped`.

Isso significa que, dentro de um mesmo escopo de DI, as resoluções correspondentes compartilham instâncias conforme as regras desse ciclo de vida.

Em uma API ASP.NET Core típica, cada requisição possui um escopo próprio.

### Interfaces não são instanciadas

O container não executa `new IRegisterExpenseUseCase()` nem `new IExpensesRepository()`.

Ele fornece instâncias de classes concretas compatíveis com esses contratos.

---

## 15. Teste prático no Swagger

Depois de configurar os registros, o professor inicia a API e abre o Swagger.

Ele adiciona um breakpoint ao controller para acompanhar o que acontece.

### Preparação

O instrutor abre o MySQL Workbench e mostra que havia apagado os registros de despesas criados anteriormente.

O banco é apresentado sem linhas de despesas naquele momento.

### Dados utilizados na demonstração

| Campo | Valor mencionado |
|---|---|
| Título | `iPhone 77` |
| Descrição | Texto livre de demonstração |
| Valor | `1500` |
| Tipo de pagamento | `1` |
| Data | Não especificada de forma verificável na transcrição |

**Cuidado:** o valor `1` é o identificador numérico escolhido para o campo de pagamento na demonstração. O nome exato do correspondente membro do enum não é confirmado aqui e não deve ser inventado.

### Passo a passo

1. Selecionar o endpoint de cadastro de despesas no Swagger.
2. Clicar em **Try it out**.
3. Informar os dados da despesa.
4. Executar a requisição.
5. A execução para no breakpoint do controller.
6. O professor verifica que a Request foi preenchida.
7. Inspeciona o parâmetro `useCase` e identifica uma instância válida.
8. Utiliza F11 para entrar na implementação do use case.
9. A validação é executada e aprovada.
10. O código constrói a entidade `Expense`.
11. O depurador entra no método do repositório.
12. O repositório constrói `CashFlowDbContext`.
13. A entidade é adicionada ao conjunto `Expenses`.
14. `SaveChanges()` executa a persistência.
15. A execução continua e retorna ao Swagger.

### O que foi demonstrado?

A resolução da dependência não ficou apenas na teoria.

O depurador mostrou que:

- a API recebeu um use case por `[FromServices]`;
- o use case recebeu a interface de repositório pelo construtor;
- a implementação do repositório foi chamada;
- o contexto efetuou a operação de gravação.

### Resultado HTTP

O Swagger retornou **HTTP 201 Created**.

O professor observa que a resposta pode receber melhorias mais à frente, mas mantém esse comportamento por enquanto.

**Importante:** HTTP 201 comunica o resultado da operação HTTP. Por si só, ele não é uma prova absoluta de que os dados foram persistidos; a gravação também confere o estado do banco.

---

## 16. Conferência do registro no MySQL

Após obter a resposta no Swagger, o professor volta ao MySQL Workbench.

Ele executa novamente a consulta à tabela de despesas usando o botão de execução da ferramenta.

### Resultado observado

A tabela, que estava vazia antes do teste, passa a apresentar o registro cadastrado.

O professor aponta o **ID 27** para a despesa.

### Por que o ID é 27 se os registros anteriores foram apagados?

A gravação informa que as despesas de testes anteriores foram apagadas, mas o novo registro recebeu um identificador diferente de 1.

**Complemento técnico:** em tabelas que utilizam `AUTO_INCREMENT`, excluir linhas não significa necessariamente reiniciar o contador de identificadores.

Assim, o valor observado é compatível com a continuidade do contador, mesmo após a exclusão dos registros anteriores.

### O que a verificação confirma?

Na demonstração, a despesa enviada pelo Swagger efetivamente apareceu no banco após a execução do fluxo novo, agora utilizando o repositório e os registros de DI.

```text
Antes da requisição
MySQL: sem registros exibidos
          |
          v
Swagger: POST de cadastro
          |
          v
HTTP 201 Created
          |
          v
Workbench: atualizar consulta
          |
          v
Despesa inserida com ID 27
```

### O que ainda não foi demonstrado?

O instrutor não executa testes automatizados de integração nesse momento.

A verificação é **manual**, utilizando Swagger, depuração do Visual Studio e MySQL Workbench.

---

## 17. Código reconstruído da solução

Os exemplos desta seção consolidam a estrutura descrita na transcrição. **Não representam a cópia literal dos arquivos do instrutor**. Os namespaces e alguns detalhes de retorno foram organizados de forma didática.

### 17.1 Domain: IExpensesRepository

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

### 17.2 Infrastructure: ExpensesRepository

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

Esse trecho recupera a implementação da aula 98, inclusive a construção manual do `DbContext`.

### 17.3 Infrastructure: configuração de dependências

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

Esse é o padrão demonstrado na aula 99.

### 17.4 Application: IRegisterExpenseUseCase

```csharp
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;

namespace CashFlow.Application.UseCases.Expenses.Register;

public interface IRegisterExpenseUseCase
{
    ResponseRegisterExpenseJson Execute(RequestRegisterExpenseJson request);
}
```

**Nota:** os namespaces são ilustrativos. O contrato deve usar a assinatura concreta existente em `RegisterExpenseUseCase.Execute`; a transcrição não transcreve as linhas completas do editor.

### 17.5 Application: RegisterExpenseUseCase

```csharp
using CashFlow.Domain.Repositories.Expenses;

namespace CashFlow.Application.UseCases.Expenses.Register;

public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IExpensesRepository _repository;

    public RegisterExpenseUseCase(IExpensesRepository repository)
    {
        _repository = repository;
    }

    // O método Execute preserva as validações existentes,
    // o mapeamento de Request para Expense e o retorno
    // que já haviam sido implementados em aulas anteriores.

    // Após construir a entidade:
    // _repository.Add(entity);
}
```

O trecho acima destaca apenas a parte nova de DI da aula 100.

Para compreender o fluxo sem preencher com detalhes não disponíveis, a sequência de `Execute` é:

```text
Execute(request)
    |
    +-- Validar a requisição
    |
    +-- Construir a entidade Expense
    |
    +-- _repository.Add(entity)
    |
    +-- Retornar a resposta já definida pelo use case
```

### 17.6 Application: DependencyInjectionExtension

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<
            IRegisterExpenseUseCase,
            RegisterExpenseUseCase>();
    }
}
```

Os `using` específicos dos tipos do use case devem ser ajustados conforme o namespace real. A função principal é registrar o contrato com sua implementação.

### 17.7 API: Program.cs

```csharp
using CashFlow.Application;
using CashFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Outras configurações existentes na API.

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

// Construção e execução da aplicação.
```

A ordem e o restante das configurações existentes no projeto são preservados; o código acima mostra apenas as chamadas relevantes para esta aula.

### 17.8 API: ExpensesController

```csharp
[HttpPost]
public IActionResult Register(
    [FromBody] RequestRegisterExpenseJson request,
    [FromServices] IRegisterExpenseUseCase useCase)
{
    var response = useCase.Execute(request);

    return Created(string.Empty, response);
}
```

**Exemplo conceitual:** o nome do método, os atributos completos e o conteúdo do retorno devem acompanhar o código real. A mudança demonstrada é o parâmetro `[FromServices] IRegisterExpenseUseCase useCase` e a remoção da construção direta do caso de uso.

---

## 18. Cuidados técnicos e boas práticas

### 18.1 A Application não precisa conhecer ExpensesRepository

A classe concreta permanece `internal` na Infrastructure.

O caso de uso conhece apenas `IExpensesRepository`, disponibilizada pelo Domain.

Isso preserva a intenção arquitetural construída nas aulas anteriores.

### 18.2 A interface do use case também precisa ser registrada

Não basta registrar o repositório.

Quando o controller pede `IRegisterExpenseUseCase`, o container precisa saber que deve fornecer `RegisterExpenseUseCase`.

### 18.3 A dependência é resolvida em cadeia

O container primeiro resolve o tipo solicitado pelo controller e, ao criar sua implementação, resolve as dependências exigidas pelo construtor.

Essa é a característica de **resolução de grafos de dependência** apresentada na aula 92.

### 18.4 private readonly é um campo, não uma propriedade

A declaração:

```csharp
private readonly IExpensesRepository _repository;
```

é um campo privado somente leitura.

A distinção é importante para usar corretamente a terminologia de C#.

### 18.5 AddScoped é uma escolha de ciclo de vida, não uma regra universal

O instrutor utiliza Scoped para o repositório e o use case.

Isso é coerente com muitos cenários de APIs HTTP, mas a escolha depende das responsabilidades, do estado e das dependências de cada serviço.

### 18.6 O DbContext ainda é criado manualmente

Apesar da refatoração da DI do use case e do repositório, a implementação de `ExpensesRepository` ainda possui:

```csharp
var dbContext = new CashFlowDbContext();
```

Portanto, não devemos afirmar que o `CashFlowDbContext` já foi registrado como Scoped no container nesta aula.

### 18.7 O descarte do DbContext merece uma refatoração futura

`DbContext` implementa `IDisposable`.

Criá-lo manualmente sem um descarte explícito pode não ser a abordagem mais adequada para o gerenciamento de recursos.

Uma melhoria futura poderá utilizar DI ou gerenciar o contexto explicitamente. **A gravação não implementa essa alteração**.

### 18.8 Add não persiste sem SaveChanges

A implementação do repositório reúne:

```csharp
dbContext.Expenses.Add(expense);
dbContext.SaveChanges();
```

A primeira chamada registra a entidade como pendente; a segunda solicita sua gravação.

Esse ponto foi demonstrado originalmente na aula 91.

### 18.9 O retorno HTTP 201 não substitui a verificação de persistência

A resposta 201 foi observada no Swagger.

A confirmação adicional veio da consulta ao MySQL, que exibiu o registro inserido.

### 18.10 Exclusão de registros não reinicia automaticamente AUTO_INCREMENT

O ID 27 observado não contradiz a informação de que os registros anteriores haviam sido apagados.

A tabela pode continuar com um contador de identidade avançado.

### 18.11 O registro não cria necessariamente os objetos de imediato

O registro:

```csharp
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

configura o container.

A criação da instância ocorre quando ela é resolvida no escopo apropriado, conforme a política de DI.

### 18.12 Interfaces permitem testes com substitutos

O construtor:

```csharp
public RegisterExpenseUseCase(IExpensesRepository repository)
```

facilita fornecer um fake ou mock em testes unitários, sem obrigar a regra de negócio a abrir uma conexão real com o banco.

A gravação não cria novos testes automatizados; trata-se de uma consequência técnica do desenho adotado.

### 18.13 O uso de FromServices não altera a política Scoped

O atributo `[FromServices]` determina de onde vem o parâmetro da ação.

Ele não muda o ciclo de vida definido por `AddScoped`.

### 18.14 O projeto Application passa a conhecer a biblioteca de DI

O professor adiciona `Microsoft.Extensions.DependencyInjection` à Application para criar o método de registro.

Essa é uma escolha arquitetural comum em projetos ASP.NET Core com métodos de extensão de composição. O domínio do negócio continua separado dos detalhes concretos de infraestrutura, mas a camada Application passa a conter código específico de configuração de DI.

### 18.15 Os detalhes finais da resposta ainda serão aprimorados

O professor afirma que a resposta HTTP 201 pode receber melhorias depois.

Não devemos antecipar novos DTOs, URLs de localização ou mudanças no formato de retorno que a gravação ainda não demonstrou.

---

## 19. Resumo, perguntas e mapa mental

### Principais aprendizados

- O objetivo é testar a integração entre repositório, use case, controller e DI.
- `ExpensesRepository` permanece `internal` na Infrastructure.
- `IExpensesRepository`, no Domain, está acessível à Application.
- O use case passa a receber `IExpensesRepository` no construtor.
- A instância é guardada no campo `private readonly IExpensesRepository _repository`.
- O método `Execute` continua validando e construindo a despesa.
- Após preparar a entidade, chama `_repository.Add(entity)`.
- A construção manual de `RegisterExpenseUseCase` no controller deixa de funcionar sem os argumentos exigidos.
- O professor cria `IRegisterExpenseUseCase` na mesma pasta do use case e do validator.
- A classe `RegisterExpenseUseCase` passa a implementar a interface.
- A Application ganha a classe estática `DependencyInjectionExtension`.
- O método `AddApplication(this IServiceCollection services)` registra o use case com `AddScoped`.
- O professor instala o pacote `Microsoft.Extensions.DependencyInjection` na Application.
- O controller passa a receber `[FromServices] IRegisterExpenseUseCase`.
- O `Program.cs` chama `builder.Services.AddApplication()` além de `AddInfrastructure()`.
- No Swagger, o POST com título `iPhone 77`, valor `1500` e tipo de pagamento `1` retorna **HTTP 201**.
- O MySQL Workbench mostra a despesa inserida com **ID 27**.
- A gravação comprova o funcionamento de DI e persistência manualmente.
- Outras boas práticas e pequenas refatorações são anunciadas para a aula seguinte.

### Perguntas de revisão

1. Por que `RegisterExpenseUseCase` não pode instanciar diretamente `ExpensesRepository`?
2. Como a Application consegue acessar `IExpensesRepository`?
3. Qual é a função da injeção pelo construtor?
4. Por que a dependência é guardada em um campo privado `readonly`?
5. Qual a diferença entre um campo e uma propriedade em C#?
6. O que faz `_repository.Add(entity)` no fluxo do caso de uso?
7. Por que o controller passou a apresentar erro ao usar `new RegisterExpenseUseCase()`?
8. Para que serve `IRegisterExpenseUseCase`?
9. Por que `RegisterExpenseUseCase` deve declarar que implementa a interface?
10. Em qual projeto é criada a extensão `AddApplication()`?
11. Qual pacote de DI o professor instala na Application?
12. Por que o controller recebe o use case com `[FromServices]`?
13. Por que `AddInfrastructure()` e `AddApplication()` são ambos necessários?
14. Quem resolve `IExpensesRepository` quando o use case é construído?
15. `CashFlowDbContext` já está sendo injetado por construtor na implementação da aula?
16. Qual é a diferença entre `DbSet.Add` e `SaveChanges`?
17. Que dados de exemplo foram utilizados no Swagger?
18. Por que o identificador gravado foi 27 mesmo após exclusão de registros anteriores?
19. Por que conferir o banco é diferente de apenas observar HTTP 201?
20. O que o professor deixa para a próxima aula?

### Quadro de responsabilidades

| Componente | Camada | Responsabilidade |
|---|---|---|
| `ExpensesController` | API | Receber a requisição e chamar o caso de uso |
| `IRegisterExpenseUseCase` | Application | Contrato de execução do cadastro |
| `RegisterExpenseUseCase` | Application | Validar, mapear e solicitar persistência |
| `IExpensesRepository` | Domain | Contrato para adicionar despesas |
| `ExpensesRepository` | Infrastructure | Executar persistência pelo contexto |
| `CashFlowDbContext` | Infrastructure | Integrar o EF Core com a tabela de despesas |
| `AddApplication()` | Application | Registrar o use case no container |
| `AddInfrastructure()` | Infrastructure | Registrar o repositório no container |

### Mapa mental

```text
                          AULA 100
                   INTEGRAÇÃO COMPLETA DA DI
                              |
                       Program.cs (API)
                              |
                  +-----------+-----------+
                  |                       |
             AddApplication()      AddInfrastructure()
                  |                       |
                  v                       v
        IRegisterExpenseUseCase    IExpensesRepository
                  |                       |
                  v                       v
        RegisterExpenseUseCase     ExpensesRepository
                  |                       |
                  +-----------+-----------+
                              |
                      Grafo de dependências
                              |
                       ExpensesController
                              |
               [FromServices] IRegisterExpenseUseCase
                              |
                              v
                         Execute(request)
                              |
                         Validação
                              |
                      Montar entidade
                              |
                     _repository.Add(entity)
                              |
                              v
                       ExpensesRepository
                              |
                    new CashFlowDbContext()
                              |
                   Expenses.Add + SaveChanges
                              |
                              v
                             MySQL
                              |
                       Registro ID 27
                              |
                              v
                        HTTP 201 Created
                              |
                              v
                       Próxima aula
                    Pequenas refatorações
```

**Conclusão:** esta aula completa a integração inicial da injeção de dependência entre API, Application, Domain e Infrastructure. O controller solicita `IRegisterExpenseUseCase`, o caso de uso recebe `IExpensesRepository` pelo construtor e a implementação concreta persiste a entidade pelo EF Core. O professor confirma o funcionamento com uma requisição HTTP 201 no Swagger e a consulta ao MySQL, que exibe a nova despesa com ID 27. Ainda existem melhorias a realizar, especialmente no gerenciamento do `DbContext`, anunciadas para as próximas etapas do curso.
