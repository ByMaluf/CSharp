# Aula 113 — SOLID: Interface Segregation Principle (ISP) e segregação de interfaces do repositório

> **Continuação do CashFlow:** após implementar cadastro, listagem, consulta por ID e tratamento de recurso inexistente, o professor refatora o contrato IExpensesRepository em interfaces específicas de leitura e escrita.
>
> **Resultado:** IExpensesReadOnlyRepository e IExpensesWriteOnlyRepository passam a ser implementadas pela mesma classe ExpensesRepository. Cada caso de uso recebe apenas o contrato adequado ao seu contexto.
>
> **Teste apresentado:** a listagem retorna os IDs 27 a 32; a consulta do ID 32 retorna Mouse com HTTP 200; o cadastro de Pizza, valor 20 e tipo de pagamento 1, retorna HTTP 201 com o título.
>
> **Fidelidade:** o conteúdo acompanha a transcrição. Os exemplos de código reconstroem a estrutura narrada, sem reproduzir literalmente namespaces ou assinaturas não mostrados. Observações adicionais estão identificadas como complementos técnicos. As operações de atualização e exclusão são anunciadas, mas não implementadas nesta aula.

## Índice

1. Objetivo da aula
2. O I do SOLID
3. Problema ilustrado com IRegistration
4. Por que a interface original é inadequada?
5. Segregando contratos por capacidades
6. Situação anterior do CashFlow
7. Agrupamento por contexto
8. Criando IExpensesWriteOnlyRepository
9. Criando IExpensesReadOnlyRepository
10. Removendo IExpensesRepository
11. Uma classe implementando múltiplas interfaces
12. Registro das interfaces na DI
13. Alterações nos casos de uso de leitura
14. Alteração do caso de uso de cadastro
15. Código consolidado
16. O que permanece inalterado
17. Testes no Swagger
18. Benefícios, limites e distinções técnicas
19. Exclusão e atualização: o que fica para depois
20. Exercícios e revisão
21. Resumo e mapa mental

---

## 1. Objetivo da aula

O professor apresenta o princípio de segregação de interfaces e aplica esse conceito à organização dos repositórios de despesas.

Até a aula 112, o CashFlow utilizava um contrato único, IExpensesRepository, para todas as operações implementadas:

- Adicionar uma despesa.
- Recuperar todas as despesas.
- Recuperar uma despesa por ID.

O problema identificado é arquitetural: cada caso de uso que recebe esse contrato passa a ter acesso a todos os métodos, mesmo quando necessita apenas de uma parte deles.

A evolução prevista do sistema inclui atualizar e excluir despesas. Sem segregação, essas operações poderiam ampliar ainda mais a interface original.

Nesta aula, o professor separa os contratos por contexto de leitura e escrita, mantendo uma implementação concreta compartilhada.

## 2. O I do SOLID

SOLID reúne cinco princípios de design orientado a objetos.

| Letra | Princípio |
|---|---|
| S | Single Responsibility Principle |
| O | Open/Closed Principle |
| L | Liskov Substitution Principle |
| I | Interface Segregation Principle |
| D | Dependency Inversion Principle |

O foco da aula é o **Interface Segregation Principle — ISP**, em português, **Princípio da Segregação de Interfaces**.

O professor apresenta sua ideia central: classes não devem ser forçadas a depender de métodos que não utilizam.

A formulação enfatiza a perspectiva de quem consome uma abstração. Um contrato não deve impor dependências desnecessárias ao cliente que o utiliza.

A outra ideia destacada é que interfaces específicas são preferíveis a uma interface única que concentra capacidades não relacionadas às necessidades de seus consumidores.

### O que significa segregar?

Dividir contratos de acordo com capacidades coerentes, permitindo que cada consumidor dependa somente das operações apropriadas à sua finalidade.

Não significa necessariamente criar uma interface para cada método.

---

## 3. Problema ilustrado com IRegistration

O professor utiliza um exemplo conceitual de cadastro envolvendo clientes e produtos.

Imagine uma interface que declare duas operações:

```csharp
public interface IRegistration
{
    void Save();
    void SendEmail();
}
```

O método Save representa o salvamento de dados no banco. SendEmail representa o envio de uma mensagem.

### Uma classe de cliente

```csharp
public class Customer : IRegistration
{
    public void Save()
    {
        // Persistir os dados do cliente.
    }

    public void SendEmail()
    {
        // Enviar um e-mail ao cliente.
    }
}
```

No cenário apresentado, as duas operações fazem sentido para Customer.

### Uma classe de produto

```csharp
public class Product : IRegistration
{
    public void Save()
    {
        // Persistir os dados do produto.
    }

    public void SendEmail()
    {
        // Operação exigida pelo contrato,
        // mas sem significado para o produto.
    }
}
```

O professor questiona: faz sentido obrigar um produto a implementar a operação de envio de e-mail?

No exemplo, não.

A interface contém uma capacidade necessária a um tipo de consumidor, mas desnecessária a outro.

Os corpos dos métodos acima são ilustrativos, pois a transcrição descreve o comportamento sem transcrever o código completo mostrado na tela.

## 4. Por que a interface original é inadequada?

A classe Product precisa satisfazer o contrato IRegistration. Para isso, teria de implementar SendEmail, embora o método não tenha utilidade no contexto do produto.

Isso pode levar a soluções artificiais, como:

- Implementações vazias.
- Métodos que lançam NotSupportedException quando chamados.
- Operações sem significado para a classe.
- Dependência de uma capacidade que o consumidor não precisa conhecer.

Esses comportamentos são exemplos de consequências possíveis, não implementações realizadas no CashFlow.

### Precisão sobre implementação de interfaces

A aula trabalha com interfaces que declaram métodos abstratos, sem implementação padrão. Nesse cenário, classes concretas que implementam o contrato precisam fornecer os membros exigidos, diretamente ou por uma implementação válida herdada.

Como complemento, versões modernas do C# permitem certos membros com implementação padrão em interfaces. Isso não elimina o problema de design de expor capacidades irrelevantes aos consumidores.

## 5. Segregando contratos por capacidades

Uma forma de resolver o exemplo é separar as capacidades.

```csharp
public interface IRegistration
{
    void Save();
}

public interface IEmailSender
{
    void SendEmail();
}
```

O cliente pode implementar os dois contratos:

```csharp
public class Customer : IRegistration, IEmailSender
{
    public void Save()
    {
        // Persistir cliente.
    }

    public void SendEmail()
    {
        // Enviar mensagem.
    }
}
```

O produto pode implementar somente a capacidade de registro:

```csharp
public class Product : IRegistration
{
    public void Save()
    {
        // Persistir produto.
    }
}
```

**Complemento didático:** essa solução concreta é uma ilustração do princípio apresentado. O foco da implementação real da aula é o repositório de despesas. O professor não estabelece nomes definitivos para as interfaces segregadas do exemplo de cliente e produto.

Também é possível que o envio de e-mail seja responsabilidade de um serviço próprio, em vez de uma entidade de cliente. A analogia do instrutor serve para demonstrar o problema dos contratos excessivamente abrangentes, não para estabelecer uma arquitetura completa de cadastro.

---

## 6. Situação anterior do CashFlow

Na aula 111, o contrato de repositório já havia acumulado as operações de cadastro e consulta.

```csharp
public interface IExpensesRepository
{
    Task Add(Expense expense);

    Task<List<Expense>> GetAll();

    Task<Expense?> GetById(long id);
}
```

### Dependências existentes

```text
RegisterExpenseUseCase ------+
                             |
GetAllExpensesUseCase -------+-- IExpensesRepository
                             |        |
GetExpenseByIdUseCase --------+        |
                                      v
                               ExpensesRepository
```

Todos os casos de uso recebem a mesma interface.

Porém:

| Caso de uso | Operação necessária |
|---|---|
| RegisterExpenseUseCase | Add |
| GetAllExpensesUseCase | GetAll |
| GetExpenseByIdUseCase | GetById |

O professor esclarece que o CashFlow não reproduz exatamente o problema do produto obrigado a implementar SendEmail. A implementação ExpensesRepository consegue realizar todas as operações de seu contrato.

A oportunidade de melhoria está na **perspectiva dos consumidores**: cada use case tem acesso a métodos que não precisa utilizar.

## 7. Agrupamento por contexto

O professor considera exagerado, para o contexto atual, criar uma interface individual para cada operação.

Ele prefere agrupar métodos por capacidades:

- Leitura: recuperar informações sem modificá-las.
- Escrita: adicionar registros e, futuramente, excluir.
- Atualização: um contexto separado que será criado em aulas posteriores.

### Escolha do curso

```text
Antes:
IExpensesRepository
    Add
    GetAll
    GetById

Depois:
IExpensesWriteOnlyRepository
    Add

IExpensesReadOnlyRepository
    GetAll
    GetById
```

Não existe uma quantidade universalmente correta de métodos em uma interface. A granularidade deve atender aos consumidores e manter os contratos coesos.

Neste projeto, a escolha do professor é segregar por contexto, em vez de criar uma interface por operação.

---

## 8. Criando IExpensesWriteOnlyRepository

O instrutor adiciona uma nova interface pública no Domain, junto aos contratos de despesas.

### Código reconstruído

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);
}
```

O método Add é transferido da interface anterior.

Não há alteração na assinatura, no retorno assíncrono ou no funcionamento de Add.

### Por que WriteOnly?

O contrato agrupa operações relacionadas à escrita.

No momento da gravação, contém somente Add.

O professor informa que pretende colocar Delete nessa interface mais adiante, mas **Delete ainda não foi implementado**.

---

## 9. Criando IExpensesReadOnlyRepository

O instrutor cria uma segunda interface pública.

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesReadOnlyRepository
{
    Task<List<Expense>> GetAll();

    Task<Expense?> GetById(long id);
}
```

As operações de consulta são transferidas sem modificar seus nomes.

### GetAll

Devolve assincronamente uma lista de despesas.

### GetById

Devolve uma despesa ou null, de acordo com a existência do registro pesquisado.

A assinatura anulável preserva o aprendizado da aula 111.

### ReadOnly não significa imutabilidade

Como complemento técnico, o nome ReadOnly descreve as capacidades expostas pelo contrato. Ele não torna automaticamente imutáveis os objetos Expense retornados.

No CashFlow, as consultas também utilizam AsNoTracking, conforme as aulas 110 e 111. Essa configuração evita o rastreamento automático das instâncias materializadas, mas é uma decisão da implementação EF Core, distinta da segregação de interfaces.

## 10. Removendo IExpensesRepository

Depois de transferir os três métodos, o professor exclui a interface original.

Isso é importante: não se trata apenas de criar interfaces extras e continuar usando a abstração anterior.

Os componentes que dependiam de IExpensesRepository precisam ser atualizados.

### Locais afetados

1. Declaração da classe concreta ExpensesRepository.
2. Registro de serviços da Infrastructure.
3. GetAllExpensesUseCase.
4. GetExpenseByIdUseCase.
5. RegisterExpenseUseCase.

O compilador ajuda a identificar referências ao tipo excluído.

Os métodos já existentes são mantidos com os mesmos nomes, o que simplifica a refatoração.

---

## 11. Uma classe implementando múltiplas interfaces

O professor explica que uma classe C# pode implementar duas ou mais interfaces.

Por isso, não é necessário criar imediatamente uma classe ExpensesReadOnlyRepository e outra ExpensesWriteOnlyRepository.

### Nova declaração

```csharp
internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository
{
    // Implementações existentes.
}
```

A classe continua oferecendo a implementação das operações de leitura e escrita.

### Por que isso funciona?

Os métodos Add, GetAll e GetById já possuem assinaturas compatíveis com os novos contratos.

A classe passa a satisfazer as duas interfaces sem precisar duplicar seus métodos.

### Visibilidade por tipo de referência

```csharp
IExpensesReadOnlyRepository reader = repository;

// Disponíveis:
// reader.GetAll();
// reader.GetById(id);

// reader.Add(expense); // Não pertence ao contrato.
```

```csharp
IExpensesWriteOnlyRepository writer = repository;

// Disponível:
// writer.Add(expense);

// writer.GetAll(); // Não pertence ao contrato.
```

Aqui, repository representa uma instância de ExpensesRepository.

O tipo estático da referência determina os membros acessíveis diretamente por esse contrato.

### Separar contratos não exige separar implementações

O professor prefere manter uma única classe enquanto os métodos são pequenos e a classe permanece simples de manter.

Caso a implementação cresça excessivamente, ele sugere avaliar a divisão em classes de leitura e escrita.

---

## 12. Registro das interfaces na DI

Na extensão de injeção de dependência da Infrastructure, o registro anterior era:

```csharp
services.AddScoped<IExpensesRepository, ExpensesRepository>();
```

Como IExpensesRepository foi removida, esse registro precisa ser substituído.

### Novos registros

```csharp
services.AddScoped<
    IExpensesReadOnlyRepository,
    ExpensesRepository>();

services.AddScoped<
    IExpensesWriteOnlyRepository,
    ExpensesRepository>();
```

O container passa a resolver a mesma **classe de implementação** para dois contratos diferentes.

### Atenção à diferença entre classe e instância

O professor explica que ambos os contratos devolvem a implementação ExpensesRepository.

Isso não implica que os dois registros acima compartilhem necessariamente a mesma instância concreta.

No container padrão do ASP.NET Core, registros Scoped independentes por tipos de serviço diferentes podem produzir instâncias distintas de ExpensesRepository dentro de um mesmo escopo. Cada resolução continua respeitando seu registro.

O DbContext registrado com AddDbContext, normalmente Scoped, pode ser compartilhado pelas duas instâncias dentro da mesma requisição.

**Complemento técnico:** se fosse necessário garantir que os dois contratos resolvessem a mesma instância de repositório, uma estratégia seria registrar a implementação concreta e encaminhar ambas as interfaces para ela:

```csharp
services.AddScoped<ExpensesRepository>();

services.AddScoped<IExpensesReadOnlyRepository>(
    sp => sp.GetRequiredService<ExpensesRepository>());

services.AddScoped<IExpensesWriteOnlyRepository>(
    sp => sp.GetRequiredService<ExpensesRepository>());
```

O professor não apresenta essa estratégia alternativa na aula. A implementação estudada usa os dois registros diretos, suficientes para o contexto demonstrado.

O registro de IUnitOfWork e a configuração do CashFlowDbContext são preservados.

---

## 13. Alterações nos casos de uso de leitura

O professor abre os casos de uso da Application e substitui o tipo da dependência antiga.

### GetAllExpensesUseCase

```csharp
private readonly IExpensesReadOnlyRepository _repository;
private readonly IMapper _mapper;

public GetAllExpensesUseCase(
    IExpensesReadOnlyRepository repository,
    IMapper mapper)
{
    _repository = repository;
    _mapper = mapper;
}
```

O método Execute continua consultando:

```csharp
var result = await _repository.GetAll();
```

O mapeamento da coleção para ResponseExpensesJson permanece igual.

### GetExpenseByIdUseCase

```csharp
private readonly IExpensesReadOnlyRepository _repository;
private readonly IMapper _mapper;

public GetExpenseByIdUseCase(
    IExpensesReadOnlyRepository repository,
    IMapper mapper)
{
    _repository = repository;
    _mapper = mapper;
}
```

A operação continua utilizando:

```csharp
var result = await _repository.GetById(id);
```

A verificação de null, a NotFoundException e o mapeamento da resposta detalhada não são removidos.

### O que muda?

O contrato injetado no construtor e o tipo do campo privado.

### O que não muda?

As chamadas dos métodos, pois seus nomes e assinaturas foram preservados.

## 14. Alteração do caso de uso de cadastro

O professor abre RegisterExpenseUseCase e substitui IExpensesRepository por IExpensesWriteOnlyRepository.

```csharp
private readonly IExpensesWriteOnlyRepository _repository;
private readonly IUnitOfWork _unitOfWork;
private readonly IMapper _mapper;

public RegisterExpenseUseCase(
    IExpensesWriteOnlyRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
}
```

O fluxo existente de persistência continua:

```csharp
var entity = _mapper.Map<Expense>(request);

await _repository.Add(entity);
await _unitOfWork.Commit();

return _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

A validação da requisição existente deve ser preservada antes do mapeamento.

### Por que manter Unit of Work?

A segregação de interfaces não modifica a responsabilidade de persistir alterações.

Add prepara a inclusão da entidade no contexto. Commit efetiva as alterações conforme a arquitetura introduzida na aula 103 e refatorada na aula 106.

---

## 15. Código consolidado

Esta seção consolida os componentes relevantes. Os namespaces e declarações completas são reconstruções coerentes com as aulas anteriores.

### Domain — IExpensesReadOnlyRepository.cs

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesReadOnlyRepository
{
    Task<List<Expense>> GetAll();

    Task<Expense?> GetById(long id);
}
```

### Domain — IExpensesWriteOnlyRepository.cs

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);
}
```

### Infrastructure — ExpensesRepository.cs

```csharp
using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories.Expenses;
using Microsoft.EntityFrameworkCore;

internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository
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

    public async Task<List<Expense>> GetAll()
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Expense?> GetById(long id)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(expense => expense.Id == id);
    }
}
```

O código dos métodos é recuperado das implementações documentadas nas aulas anteriores. A alteração específica desta aula é a lista de interfaces implementadas.

### Infrastructure — DependencyInjectionExtension

```csharp
// Dentro do método de registro dos repositórios:

services.AddScoped<
    IExpensesReadOnlyRepository,
    ExpensesRepository>();

services.AddScoped<
    IExpensesWriteOnlyRepository,
    ExpensesRepository>();

services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Application — dependências

| Use case | Contrato de repositório após a refatoração | Outras dependências preservadas |
|---|---|---|
| GetAllExpensesUseCase | IExpensesReadOnlyRepository | IMapper |
| GetExpenseByIdUseCase | IExpensesReadOnlyRepository | IMapper |
| RegisterExpenseUseCase | IExpensesWriteOnlyRepository | IMapper, IUnitOfWork |

### Exemplo do fluxo de leitura por ID

```csharp
public async Task<ResponseExpenseJson> Execute(long id)
{
    var result = await _repository.GetById(id);

    if (result is null)
        throw new NotFoundException(
            ResourceErrorMessages.ExpenseNotFound);

    return _mapper.Map<ResponseExpenseJson>(result);
}
```

O nome do recurso de mensagem segue a reconstrução utilizada na aula 112. Deve ser conferido no código original.

A lógica do método permanece a mesma. Apenas a dependência de repositório agora é especializada para leitura.

## 16. O que permanece inalterado

A segregação modifica a organização dos contratos e a resolução de dependências, não a finalidade dos endpoints.

| Elemento | Situação |
|---|---|
| Métodos Add, GetAll e GetById | Preservados |
| AsNoTracking | Preservado nas consultas |
| Task e await | Preservados |
| AutoMapper | Configurações existentes preservadas |
| Unit of Work | Continua no fluxo de cadastro |
| Filtro de exceções | Permanece com o tratamento anterior |
| DTOs | Não são alterados |
| Endpoints | Mantêm os contratos HTTP existentes |
| Banco de dados | Nenhuma alteração de esquema é demonstrada |

O professor não cria uma migration nem adiciona propriedades à entidade Expense.

---

## 17. Testes no Swagger

Após alterar a Infrastructure e os três casos de uso, o instrutor executa a API para verificar que a refatoração não comprometeu o fluxo existente.

### Teste 1 — recuperar todas as despesas

O professor executa o endpoint GET de listagem.

São exibidas as despesas com os seguintes identificadores:

```text
27, 28, 29, 30, 31, 32
```

A listagem funciona após a troca para IExpensesReadOnlyRepository.

### Teste 2 — recuperar uma despesa por ID

| Informação | Resultado |
|---|---|
| ID consultado | 32 |
| Título | Mouse |
| Status | HTTP 200 OK |

O caso de uso GetExpenseByIdUseCase continua funcionando com o novo contrato de leitura.

Esse teste também confirma que Mouse, cadastrada na aula 108, possui ID 32 na base utilizada pelo instrutor.

### Teste 3 — registrar outra despesa

| Campo | Valor |
|---|---|
| Título | Pizza |
| Valor | 20 |
| Tipo de pagamento | 1 |
| Status | HTTP 201 Created |

O Swagger devolve corretamente o título da despesa cadastrada.

A transcrição não informa o ID gerado para Pizza nem os valores literais dos demais campos; portanto, não se deve inventá-los.

### Resultado

O professor confirma que as três operações funcionam depois da segregação.

A gravação não apresenta um teste automatizado, um benchmark ou uma nova execução do cenário HTTP 404.

---

## 18. Benefícios, limites e distinções técnicas

Os pontos abaixo aprofundam o conceito sem atribuir funcionalidades adicionais à gravação.

### 18.1 Dependências mais específicas

Um consumidor de leitura não precisa receber diretamente métodos de escrita.

Isso torna a finalidade de suas dependências mais explícita.

### 18.2 Menor impacto de mudanças

Adicionar futuramente uma operação de escrita à interface de escrita não obriga os consumidores do contrato de leitura a depender desse método.

Isso não significa que nenhuma alteração possa afetar a implementação compartilhada; a classe concreta continua responsável por satisfazer seus contratos.

### 18.3 Testes de unidade mais focados

Um teste de GetAllExpensesUseCase pode fornecer uma implementação falsa ou mock de IExpensesReadOnlyRepository sem precisar implementar operações de escrita.

O professor não implementa testes de unidade nesta gravação; esse é um benefício potencial da arquitetura.

### 18.4 ISP não é controle de autorização

O contrato limita os membros expostos diretamente ao consumidor pelo tipo estático da referência.

Não é uma barreira de segurança que impeça todo acesso a operações de escrita, por exemplo por outros serviços, referências ou conversões de tipo.

Autorização e proteção dos dados continuam exigindo mecanismos próprios.

### 18.5 ISP não torna uma consulta mais rápida por si só

A refatoração separa contratos. Ela não altera o SQL, o Change Tracker ou o banco.

AsNoTracking é a otimização de consulta estudada na aula 110, não um efeito automático do ISP.

### 18.6 ISP não exige uma classe por interface

Uma implementação concreta pode satisfazer vários contratos menores, como ExpensesRepository faz nesta aula.

### 18.7 ISP não exige uma interface por método

O objetivo é evitar dependências desnecessárias, não maximizar o número de arquivos.

A divisão por contextos é a escolha do professor para equilibrar clareza e granularidade.

### 18.8 Relação com SRP e DIP

- ISP: adequação da interface às necessidades de seus consumidores.
- SRP: coesão das responsabilidades e razões para mudança de um componente.
- DIP: dependência de abstrações em vez de detalhes concretos.

Os três princípios podem contribuir para a mesma refatoração, mas não são sinônimos.

Nesta aula, o foco explícito é ISP.

### 18.9 ReadOnly e WriteOnly não significam CQRS completo

A separação de contratos de leitura e escrita tem afinidade com a distinção entre consultas e comandos.

Porém, essa mudança isolada não demonstra a implementação completa de CQRS, modelos independentes de leitura e escrita ou bancos separados.

A mesma classe de repositório e o mesmo DbContext continuam sendo utilizados.

## 19. Exclusão e atualização: o que fica para depois

O professor anuncia que o sistema ainda ganhará operações de exclusão e atualização de despesas.

### Exclusão

A preferência apresentada é adicionar o futuro método Delete à interface IExpensesWriteOnlyRepository.

Ele reconhece que outra opção seria criar uma interface de exclusão separada.

Nenhum método Delete é criado na gravação.

### Atualização

O professor informa que pretende introduzir uma interface UpdateOnly em uma próxima etapa e menciona que ela terá pelo menos dois métodos.

A motivação completa e as assinaturas ainda não são apresentadas.

Esse anúncio se conecta à aula 111, em que o instrutor comentou que a atualização exigiria atenção à estratégia de rastreamento das entidades.

**Não se deve antecipar como código implementado uma interface de atualização, suas assinaturas ou a estratégia de tracking que o professor ainda explicará.**

---

## 20. Exercícios e revisão

### Exercício 1 — identificar o problema

Uma interface oferece Save e SendEmail. Uma classe Product precisa apenas salvar seus dados.

Qual princípio é comprometido quando Product é obrigada a depender de SendEmail?

**Resposta:** Interface Segregation Principle.

### Exercício 2 — separar os contratos

Quais métodos ficam na interface de leitura?

```csharp
Task<List<Expense>> GetAll();
Task<Expense?> GetById(long id);
```

Qual método fica na interface de escrita nesta etapa?

```csharp
Task Add(Expense expense);
```

### Exercício 3 — implementar duas interfaces

```csharp
internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository
{
    // Implementar os métodos exigidos.
}
```

Não é obrigatório criar uma classe diferente para cada interface.

### Exercício 4 — escolher dependências

- GetAllExpensesUseCase: IExpensesReadOnlyRepository.
- GetExpenseByIdUseCase: IExpensesReadOnlyRepository.
- RegisterExpenseUseCase: IExpensesWriteOnlyRepository.

### Exercício 5 — registrar na DI

```csharp
services.AddScoped<IExpensesReadOnlyRepository, ExpensesRepository>();
services.AddScoped<IExpensesWriteOnlyRepository, ExpensesRepository>();
```

### Exercício 6 — distinguir classe de instância

Esses registros garantem que as duas interfaces retornem a mesma instância concreta?

**Resposta:** não necessariamente. Os registros são independentes; compartilham o tipo de implementação, mas não garantem identidade de instância entre os dois serviços.

### Perguntas de revisão

1. Qual é o significado do I de SOLID?
2. Qual problema o exemplo de cliente e produto demonstra?
3. Por que uma interface excessivamente abrangente pode prejudicar consumidores?
4. O professor considera obrigatório criar uma interface por método?
5. Quais eram os três métodos da interface original do CashFlow?
6. Quais duas interfaces são criadas?
7. O que acontece com IExpensesRepository?
8. Por que GetById pertence à interface de leitura?
9. Por que Add pertence à interface de escrita?
10. Uma classe pode implementar vários contratos?
11. Qual classe concreta implementa os dois novos contratos?
12. Como os registros da DI são ajustados?
13. Quais casos de uso recebem a interface de leitura?
14. Qual caso de uso recebe a interface de escrita?
15. Por que Unit of Work continua no cadastro?
16. A refatoração altera as consultas SQL?
17. AsNoTracking é consequência automática da segregação?
18. ReadOnly torna a entidade imutável?
19. Quais IDs aparecem no teste de listagem?
20. Qual despesa é recuperada pelo ID 32?
21. Quais dados são utilizados no teste de cadastro de Pizza?
22. Quais respostas HTTP são observadas?
23. A exclusão e a atualização já foram implementadas?
24. Qual é a diferença entre ISP e uma arquitetura CQRS completa?

---

## 21. Resumo e mapa mental

### Antes e depois

```text
ANTES

GetAllExpensesUseCase
GetExpenseByIdUseCase
RegisterExpenseUseCase
          |
          v
IExpensesRepository
    Add
    GetAll
    GetById
          |
          v
ExpensesRepository


DEPOIS

GetAllExpensesUseCase
GetExpenseByIdUseCase
          |
          v
IExpensesReadOnlyRepository
    GetAll
    GetById
          |
          +------------------+
                             |
                             v
                      ExpensesRepository
                             ^
                             |
          +------------------+
          |
IExpensesWriteOnlyRepository
    Add
          ^
          |
RegisterExpenseUseCase
```

### Principais decisões

| Componente | Alteração |
|---|---|
| Domain | Segregação dos contratos de despesas |
| Interface antiga | IExpensesRepository removida |
| ReadOnly | GetAll e GetById |
| WriteOnly | Add |
| Infrastructure | ExpensesRepository implementa ambos os contratos |
| DI | Dois registros Scoped |
| Application | Cada use case recebe o contrato adequado |
| Testes manuais | Listagem, detalhes e cadastro confirmados |
| Futuro | Exclusão e atualização ainda serão desenvolvidas |

**Conclusão:** a aula 113 aplica o Interface Segregation Principle ao CashFlow. Em vez de fornecer uma interface que concentra todas as operações a todos os consumidores, o projeto passa a expor contratos especializados por contexto. Os casos de uso de leitura dependem de IExpensesReadOnlyRepository, e o cadastro depende de IExpensesWriteOnlyRepository. A implementação concreta permanece compartilhada, os métodos existentes são preservados e os testes no Swagger confirmam que a API continua funcionando.
