# Aula 116 — Atualização de despesas (parte 1): PUT, reutilização de DTOs e implementação explícita de interfaces

> **Continuação do CashFlow:** após implementar cadastro, listagem, detalhes e exclusão, o professor inicia a última operação do CRUD: atualizar uma despesa.
>
> **Objetivo:** preparar o endpoint PUT, compartilhar o DTO de entrada e o validador entre cadastro e atualização, criar IExpensesUpdateOnlyRepository e distinguir duas implementações de GetById conforme a interface utilizada.
>
> **Resultado da aula:** a solução é recompilada com sucesso nos oito projetos após a movimentação e renomeação do validador e a correção das referências dos testes de unidade. Em seguida, o professor prepara a interface de atualização e implementa explicitamente os dois métodos GetById. A conclusão do caso de uso e o teste do endpoint ficam para a aula seguinte.
>
> **Fidelidade:** a transcrição descreve código previamente preparado e código construído durante a gravação. Os exemplos são reconstruções didáticas; nomes locais, namespaces e detalhes não transcritos integralmente podem divergir do arquivo original. O termo "receita" aparece pontualmente na fala, mas o fluxo estudado é de despesas (Expense). Precisões e alternativas técnicas são identificadas separadamente.

## Índice

1. Contexto e objetivo
2. Contrato HTTP do endpoint PUT
3. Identificador na rota e dados no corpo
4. Reutilizando RequestExpenseJson
5. Quando compartilhar ou separar DTOs
6. Estrutura inicial do UpdateExpenseUseCase
7. Movendo e renomeando ExpenseValidator
8. Corrigindo as referências dos testes
9. Rebuild Solution e o resultado dos oito projetos
10. Aplicando novamente o ISP
11. Criando IExpensesUpdateOnlyRepository
12. Implementando Update no repositório
13. Tracking versus no-tracking na atualização
14. O problema de dois GetById com a mesma assinatura
15. Implementação implícita de interfaces
16. Implementação explícita de interfaces
17. Os dois GetById no ExpensesRepository
18. Integração e pontos pendentes do caso de uso
19. Código consolidado do estado da aula
20. Cuidados técnicos e distinções
21. Exercícios e perguntas de revisão
22. Resumo e mapa mental

---

## 1. Contexto e objetivo

Até a aula 115, o CashFlow permite:

| Operação | HTTP | Finalidade |
|---|---|---|
| Create | POST | Cadastrar uma despesa |
| Read | GET | Listar despesas ou recuperar uma despesa específica |
| Delete | DELETE | Excluir uma despesa |

Falta a operação **Update**, que será exposta por um endpoint PUT.

A aula conecta conceitos apresentados anteriormente:

- segregação de interfaces (ISP), estudada na aula 113;
- consultas com AsNoTracking, estudadas nas aulas 110 e 111;
- injeção de dependência;
- Unit of Work;
- validação de requisições;
- AutoMapper;
- tratamento centralizado de exceções.

O instrutor já preparou uma parte do código antes da gravação. Durante a aula, refatora componentes existentes e apresenta uma técnica nova de C#: a implementação explícita de interfaces.

**Atenção:** embora o objetivo geral seja atualizar uma despesa, a gravação é encerrada antes de finalizar e testar a operação completa.

## 2. Contrato HTTP do endpoint PUT

O professor mostra o endpoint previamente criado, do tipo PUT.

Exemplo conceitual de requisição:

```http
PUT /expenses/32
Content-Type: application/json

{
  "title": "Mouse",
  "description": "Descrição atualizada",
  "date": "2026-01-01T00:00:00",
  "amount": 60,
  "paymentType": 1
}
```

Os dados acima são ilustrativos. A aula não executa essa requisição, não apresenta um novo valor para a despesa Mouse e não informa a rota completa ou o JSON literal.

### Respostas documentadas

| Status | Condição prevista | Corpo |
|---|---|---|
| 204 No Content | Atualização concluída | Sem corpo |
| 400 Bad Request | Dados de atualização inválidos | ResponseErrorJson |
| 404 Not Found | Despesa não encontrada pelo ID | ResponseErrorJson |

A configuração reutiliza o mecanismo de documentação de respostas HTTP estudado anteriormente.

### Exemplo de atributos

```csharp
[HttpPut("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
```

O endpoint documenta esses cenários. Isso não significa que todos eles já foram implementados e testados nesta gravação.

## 3. Identificador na rota e dados no corpo

A action recebe três informações:

- o caso de uso, por injeção de dependência;
- o identificador da despesa, pela rota;
- os novos dados, pelo corpo da requisição.

### Action reconstruída

```csharp
[HttpPut("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> Update(
    [FromServices] IUpdateExpenseUseCase useCase,
    [FromRoute] long id,
    [FromBody] RequestExpenseJson request)
{
    await useCase.Execute(id, request);
    return NoContent();
}
```

O nome da action, a ordem dos argumentos e o prefixo da rota são reconstruções ilustrativas.

### Responsabilidades

- FromServices: obtém o serviço registrado no container.
- FromRoute: recebe o ID que identifica o recurso a atualizar.
- FromBody: obtém os dados da atualização por meio do model binding e da desserialização configurada.

O use case devolve uma Task sem resultado de sucesso, assim como na exclusão da aula 115.

O controller aguarda a conclusão e retorna NoContent().

### Precisão técnica

Um ID numérico que não corresponde a uma despesa pode produzir o 404 previsto pela regra de negócio. Isso é diferente de um segmento de rota malformado, que pode falhar antes de chegar ao caso de uso, conforme as restrições de rota e o model binding.

## 4. Reutilizando RequestExpenseJson

O professor explica que renomeou o DTO utilizado anteriormente pelo cadastro.

### Antes

```csharp
RequestRegisterExpenseJson
```

### Depois

```csharp
RequestExpenseJson
```

A motivação é que, neste projeto, os dados necessários para registrar uma despesa são os mesmos necessários para atualizá-la.

Portanto, a classe deixa de ter um nome específico de registro e passa a representar os dados de entrada de uma despesa.

### Exemplo estrutural

```csharp
public class RequestExpenseJson
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
}
```

Os campos correspondem ao contexto das aulas de cadastro. Tipos exatos, nulabilidade e inicializadores são ilustrativos quando não explicitados na transcrição atual.

### Impacto nos consumidores

| Componente | Antes | Depois |
|---|---|---|
| Controller de cadastro | RequestRegisterExpenseJson | RequestExpenseJson |
| Controller de atualização | Novo endpoint | RequestExpenseJson |
| Validador | Contrato de cadastro | Contrato compartilhado de despesa |
| AutoMapper | Mapa de RequestRegisterExpenseJson para Expense | Mapa do tipo renomeado para Expense |
| Testes e auxiliares | Referências ao tipo antigo, quando existentes | Referências ao tipo renomeado |

A mudança é uma renomeação de tipo, não uma alteração dos dados exigidos pela requisição. As referências ao nome antigo precisam acompanhar a refatoração; a transcrição não mostra cada uma dessas referências sendo alterada.

## 5. Quando compartilhar ou separar DTOs

O professor enfatiza que reutilizar um DTO não é uma regra universal.

### Cenário do CashFlow

Cadastro e atualização recebem os mesmos dados.

Por isso, o instrutor considera desnecessário manter duas classes com propriedades iguais naquele contexto.

### Exemplo diferente: perfil de usuário

Uma operação de cadastro pode precisar de:

- nome;
- e-mail;
- senha.

Já uma atualização de perfil pode permitir apenas a alteração do nome, ou do nome e do e-mail.

A alteração de senha frequentemente segue um fluxo específico, com requisitos próprios.

Nesse cenário, compartilhar indiscriminadamente os DTOs poderia expor campos que não deveriam ser aceitos em determinada operação.

### Complemento de design

Mesmo quando duas classes possuem hoje propriedades iguais, pode ser razoável mantê-las separadas se os contratos possuem significados ou expectativas de evolução independentes.

Reutilizar por coincidência estrutural não é sempre a melhor decisão. Nesta aula, a reutilização é uma escolha explícita do professor diante da identidade dos dados e das regras atuais.

## 6. Estrutura inicial do UpdateExpenseUseCase

O instrutor já criou a classe e a interface do novo caso de uso.

Também informa que já registrou o serviço na DI da Application.

### Registro reconstruído

```csharp
services.AddScoped<IUpdateExpenseUseCase, UpdateExpenseUseCase>();
```

A princípio, o construtor recebe:

- IMapper;
- IUnitOfWork.

A dependência de repositório será acrescentada durante a aula.

### Interface do caso de uso

```csharp
public interface IUpdateExpenseUseCase
{
    Task Execute(long id, RequestExpenseJson request);
}
```

### Estado inicial do Execute

O professor mostra a chamada de Validate e a chamada de Commit.

Conceitualmente, o esqueleto é:

```csharp
public async Task Execute(long id, RequestExpenseJson request)
{
    Validate(request);

    // A atualização efetiva ainda será implementada.

    await _unitOfWork.Commit();
}
```

Essa representação NÃO é uma implementação funcional da atualização. Apenas preserva o esqueleto descrito na gravação.

A aula termina antes de completar a busca, o tratamento de ausência, a aplicação dos novos dados e a atualização da entidade.

## 7. Movendo e renomeando ExpenseValidator

O professor percebe que o caso de uso de atualização reutiliza o validador de cadastro.

O nome e a localização do validador não representam mais sua finalidade.

### Antes

```text
Application
└── UseCases
    └── Expenses
        └── Register
            └── RegisterExpenseValidator.cs
```

### Depois

```text
Application
└── UseCases
    └── Expenses
        ├── ExpenseValidator.cs
        ├── Register
        └── Update
```

A estrutura acima reproduz a organização conceitual narrada, sem garantir todos os nomes de pastas não especificados.

### Procedimento demonstrado

1. Arrastar o arquivo RegisterExpenseValidator da pasta Register para Expenses.
2. Confirmar a movimentação no Visual Studio.
3. Aceitar o ajuste automático do namespace.
4. Renomear o arquivo para ExpenseValidator.
5. Confirmar a alteração do nome da classe e do arquivo.
6. Remover usings desnecessários com Ctrl+R, G.

O nome passa de RegisterExpenseValidator para ExpenseValidator.

### Por que mover o arquivo?

O validador não pertence mais exclusivamente ao contexto de registro.

Ele representa regras comuns aos dados de despesas, utilizadas tanto pelo cadastro quanto pela atualização.

A mudança torna essa finalidade explícita na organização do código.

### O que acontece com Validate?

O professor considera aceitável repetir a pequena função que instancia o validador, executa a validação e verifica seus resultados nos casos de uso.

O componente que contém as regras de validação passa a ser compartilhado.

A gravação não demonstra a criação de uma classe base para todos os use cases nem uma nova abstração de validação.

### Esclarecimento

Mover um arquivo no sistema de arquivos não altera necessariamente seu namespace por si só. O professor utiliza a funcionalidade de refatoração do Visual Studio para ajustar a declaração e as referências correspondentes.

## 8. Corrigindo as referências dos testes

Depois da renomeação e movimentação, o professor executa um Rebuild Solution.

O build aponta erros nos projetos de testes de unidade.

### Causa descrita

Alguns arquivos de testes ainda referenciavam o nome RegisterExpenseValidator ou o namespace anterior.

O Visual Studio não atualizou todas as referências automaticamente.

### Correção

O professor abre os erros individualmente, substitui o nome antigo por ExpenseValidator e utiliza sugestões do editor para importar o namespace correto.

Exemplo ilustrativo:

```csharp
// Antes:
var validator = new RegisterExpenseValidator();

// Depois:
var validator = new ExpenseValidator();
```

Também remove usings que deixaram de ser utilizados.

### Aprendizado

Refatorações de nomes e namespaces podem afetar vários projetos, não somente a classe que está sendo movida.

Os testes são consumidores do código e precisam acompanhar a mudança de seu contrato.

Não há evidência de que as regras de validação foram alteradas ou de que os testes foram reescritos para validar novas regras. O professor corrige principalmente as referências decorrentes da movimentação e da renomeação.

## 9. Rebuild Solution e o resultado dos oito projetos

O professor utiliza a opção Rebuild Solution do Visual Studio para verificar a consistência da solução após as mudanças.

### Resultado

Depois de corrigir as referências, o instrutor informa que todos os **oito projetos** foram compilados com sucesso.

### O que esse resultado comprova?

Naquele ponto da aula, a solução foi reconstruída sem os erros de compilação decorrentes da renomeação e da mudança de namespace.

### O que não comprova?

- Que todos os testes de unidade foram executados e passaram.
- Que o endpoint PUT funciona.
- Que a atualização foi persistida no MySQL.
- Que todo o código acrescentado posteriormente também foi recompilado.

A gravação mostra um rebuild bem-sucedido **antes** de criar a nova interface de atualização e concluir a separação dos métodos GetById.

É importante distinguir sucesso de compilação de sucesso de testes.

## 10. Aplicando novamente o ISP

Na aula 113, o professor separou o antigo IExpensesRepository em:

```text
IExpensesReadOnlyRepository
    GetAll()
    GetById(id)

IExpensesWriteOnlyRepository
    Add(expense)
    Delete(id) — acrescentado na aula 115
```

O professor já havia anunciado que a atualização exigiria outro contexto.

Nesta aula, esse contexto é criado:

```text
IExpensesUpdateOnlyRepository
    Update(expense)
    GetById(id)
```

### Por que uma interface específica?

O caso de uso de atualização precisa de duas capacidades relacionadas:

1. Recuperar uma entidade para atualização.
2. Preparar a atualização dessa entidade.

A consulta de leitura existente usa AsNoTracking, enquanto o professor deseja uma consulta rastreada para o fluxo de atualização.

Segregar os contratos permite oferecer diferentes comportamentos a consumidores de leitura e atualização, sem alterar o contrato de leitura utilizado pelos outros casos de uso.

## 11. Criando IExpensesUpdateOnlyRepository

O professor cria uma interface pública na pasta de repositórios de despesas do Domain.

### Primeiro método

```csharp
public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);
}
```

O método recebe a entidade que deverá ser atualizada.

Posteriormente, o professor acrescenta uma operação de busca.

### Contrato ao final da gravação

```csharp
using CashFlow.Domain.Entities;

public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);

    Task<Expense?> GetById(long id);
}
```

### Por que Expense?

O repositório trabalha com entidades de domínio, em vez de receber diretamente os DTOs de entrada da API.

O mapeamento dos dados da requisição e a coordenação da operação pertencem ao caso de uso.

### Por que Expense?

A anotação anulável é necessária porque o identificador pode não corresponder a uma despesa.

O professor observa um aviso de nulabilidade e corrige o contrato da nova interface, assim como havia feito na aula 111.

## 12. Implementando Update no repositório

O professor abre ExpensesRepository, na Infrastructure, e acrescenta a terceira interface à declaração da classe.

```csharp
internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository,
    IExpensesUpdateOnlyRepository
{
    // Implementações.
}
```

O Visual Studio gera o método de atualização, e o professor utiliza a operação Update do DbSet.

### Implementação

```csharp
public void Update(Expense expense)
{
    _dbContext.Expenses.Update(expense);
}
```

### Por que o método do repositório é void?

O método apenas prepara a atualização no contexto, sem precisar devolver um valor.

A chamada ao banco para efetivar as mudanças é responsabilidade de SaveChangesAsync, utilizado pelo Unit of Work.

A marcação de estado por DbSet.Update não possui uma variante UpdateAsync correspondente.

### Correção técnica

Na gravação, o professor descreve o método Update do próprio EF Core como void.

Na realidade, DbSet<TEntity>.Update(TEntity) retorna EntityEntry<TEntity>.

O contrato do repositório pode ser void porque ignora intencionalmente esse retorno.

```csharp
public void Update(Expense expense)
{
    // O EntityEntry<Expense> retornado pelo EF Core
    // não precisa ser devolvido ao caso de uso.
    _dbContext.Expenses.Update(expense);
}
```

### E UpdateRange?

O professor menciona UpdateRange para trabalhar com várias entidades.

Essa operação prepara múltiplas entidades para atualização; não representa necessariamente uma única instrução SQL em lote executada naquele momento.

A gravação utiliza somente Update para uma despesa.

## 13. Tracking versus no-tracking na atualização

A consulta GetById da interface de leitura utiliza AsNoTracking.

```csharp
return await _dbContext.Expenses
    .AsNoTracking()
    .FirstOrDefaultAsync(expense => expense.Id == id);
```

O professor quer preservar essa consulta para os casos de uso somente de leitura.

Porém, para o novo contexto de atualização, deseja recuperar uma entidade com rastreamento.

```csharp
return await _dbContext.Expenses
    .FirstOrDefaultAsync(expense => expense.Id == id);
```

No comportamento padrão de rastreamento utilizado no projeto, a segunda consulta associa a entidade materializada ao contexto.

### Por que rastrear?

O Change Tracker permite ao EF Core acompanhar o estado da entidade e detectar mudanças que deverão ser persistidas.

Uma entidade recuperada para atualização pode ter suas propriedades alteradas e, com o contexto apropriado, essas mudanças podem ser detectadas durante SaveChangesAsync.

### Precisão técnica importante

O professor afirma que uma entidade recuperada com AsNoTracking não poderia ser atualizada.

Essa afirmação precisa ser contextualizada:

- Alterar propriedades de uma entidade desconectada não faz com que o contexto detecte automaticamente essas mudanças.
- Entretanto, uma operação explícita como DbSet.Update pode associar uma entidade desconectada ao contexto e marcá-la para atualização.

Logo, uma consulta com tracking não é requisito absoluto para toda estratégia de atualização.

A escolha do curso — recuperar uma entidade rastreada para o caso de uso de atualização — é válida e torna a intenção do contrato clara. Não é a única estratégia permitida pelo EF Core.

### Cuidado com Update em entidade já rastreada

Como complemento, quando uma entidade já está rastreada, modificar suas propriedades e salvar as alterações pode ser suficiente, sem chamar Update explicitamente.

Update normalmente marca uma entidade existente e propriedades pertinentes como modificadas, podendo produzir atualizações mais amplas do que uma estratégia que detecta apenas mudanças.

O comportamento depende da configuração, do estado e do grafo de entidades.

O professor mantém o método Update no contrato desta aula; essa decisão não é substituída neste material.

## 14. O problema de dois GetById com a mesma assinatura

A interface de leitura já contém:

```csharp
Task<Expense?> GetById(long id);
```

Ao acrescentar uma assinatura equivalente à nova interface de atualização, o compilador não exige imediatamente outra implementação na classe concreta.

Por quê?

Porque um método público compatível pode implementar implicitamente ambas as interfaces.

O professor percebe que isso não atende ao objetivo: deseja que GetById tenha comportamentos diferentes conforme o contrato pelo qual seja chamado.

### Tentativa incorreta

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == id);
}

public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

O código não compila.

Os métodos possuem o mesmo nome e a mesma lista de parâmetros na mesma classe.

Não é possível distinguir esses dois membros apenas pelo conteúdo dos seus corpos.

### Isso é sobrecarga?

Não.

Sobrecarga (overloading) exige assinaturas distinguíveis, por exemplo, parâmetros com tipos ou quantidades diferentes.

A necessidade aqui é implementar separadamente dois membros de interfaces distintas, mesmo que tenham assinaturas iguais.

## 15. Implementação implícita de interfaces

Antes de explicar a solução, vale compreender o comportamento que causa a dificuldade.

Exemplo independente do CashFlow:

```csharp
public interface IReader
{
    string Get();
}

public interface IUpdater
{
    string Get();
}

public class Repository : IReader, IUpdater
{
    public string Get() => "Uma implementação comum";
}
```

O único método público Get satisfaz os dois contratos.

```csharp
var repository = new Repository();

IReader reader = repository;
IUpdater updater = repository;

Console.WriteLine(reader.Get());
Console.WriteLine(updater.Get());
```

Ambas as chamadas executam o mesmo método.

Isso é adequado quando os contratos devem compartilhar o comportamento, mas não resolve a necessidade do professor.

## 16. Implementação explícita de interfaces

A solução apresentada é a **implementação explícita de interfaces** (explicit interface implementation).

### Sintaxe

```csharp
TipoDeRetorno NomeDaInterface.NomeDoMetodo(
    Parametros)
{
    // Implementação.
}
```

O nome do membro é qualificado pelo nome da interface.

Não se utiliza o modificador public nessa forma de implementação.

### Exemplo independente

```csharp
public class Repository : IReader, IUpdater
{
    string IReader.Get() => "Leitura";

    string IUpdater.Get() => "Atualização";
}
```

Agora, cada interface possui sua própria implementação.

```csharp
var repository = new Repository();

IReader reader = repository;
IUpdater updater = repository;

Console.WriteLine(reader.Get());  // Leitura
Console.WriteLine(updater.Get()); // Atualização
```

### Como o C# escolhe o método?

A chamada é feita por meio do contrato de interface correspondente.

O runtime utiliza a implementação associada àquele membro de interface.

Não é uma escolha baseada em um if no corpo de Get, nem uma sobrecarga baseada na finalidade da chamada.

### Acesso pela classe concreta

Se a classe contém apenas as duas implementações explícitas, o seguinte não está disponível como chamada direta:

```csharp
// repository.Get(); // Não é um membro público diretamente acessível.
```

É necessário acessar o método pelo contrato correspondente:

```csharp
((IReader)repository).Get();
((IUpdater)repository).Get();
```

No CashFlow, os casos de uso já recebem as interfaces por DI. Portanto, essa forma de implementação combina com a arquitetura existente.

## 17. Os dois GetById no ExpensesRepository

O professor aplica a técnica à classe concreta.

### GetById de leitura

```csharp
async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### GetById de atualização

```csharp
async Task<Expense?> IExpensesUpdateOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### O que mudou na declaração?

1. Remoção de public.
2. Manutenção de async Task<Expense?>.
3. Inclusão do nome da interface antes do nome do método.
4. Preservação do parâmetro long id.
5. Ajuste do contrato da interface de atualização para permitir null.

### Comparação

| Contrato utilizado | Método selecionado | Rastreamento |
|---|---|---|
| IExpensesReadOnlyRepository | Implementação explícita de leitura | AsNoTracking |
| IExpensesUpdateOnlyRepository | Implementação explícita de atualização | Padrão rastreado do contexto |

### Fluxo visual

```text
ExpensesRepository
       |
       +-- IExpensesReadOnlyRepository.GetById(id)
       |      |
       |      +-- AsNoTracking()
       |      +-- FirstOrDefaultAsync(...)
       |
       +-- IExpensesUpdateOnlyRepository.GetById(id)
              |
              +-- FirstOrDefaultAsync(...)
                  sem AsNoTracking
```

### Exemplo conceitual de consumo

```csharp
Expense? readResult = await reader.GetById(id);

Expense? updateResult = await updater.GetById(id);
```

reader possui o tipo IExpensesReadOnlyRepository; updater possui o tipo IExpensesUpdateOnlyRepository.

A diferença de comportamento decorre da implementação de cada contrato.

**Atenção:** esses exemplos são explicativos. A gravação termina antes de executar um teste prático que comprove a diferença de estados do Change Tracker.

## 18. Integração e pontos pendentes do caso de uso

O professor acrescenta o novo repositório ao construtor de UpdateExpenseUseCase.

### Dependências

```csharp
private readonly IExpensesUpdateOnlyRepository _repository;
private readonly IUnitOfWork _unitOfWork;
private readonly IMapper _mapper;
```

### Construtor reconstruído

```csharp
public UpdateExpenseUseCase(
    IExpensesUpdateOnlyRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
}
```

A partir desse ponto, o caso de uso possui um contrato adequado para recuperar e atualizar despesas.

### O que já está configurado?

- Endpoint PUT.
- DTO RequestExpenseJson reutilizado.
- ExpenseValidator compartilhado.
- Registro do caso de uso na Application.
- Interface IExpensesUpdateOnlyRepository.
- Método Update na implementação concreta.
- Dois GetById explicitamente implementados.
- Nova dependência do repositório no construtor do caso de uso.

### O que ainda não é demonstrado?

- O registro de IExpensesUpdateOnlyRepository na extensão de DI da Infrastructure.
- A implementação final do método Execute.
- A busca efetiva da despesa dentro do fluxo completo de atualização.
- O tratamento de null nesse fluxo.
- A aplicação dos novos dados à entidade existente.
- A persistência da atualização em um teste real.
- O teste dos status 204, 400 e 404 do PUT.

A ausência de demonstração de um registro não prova que ele nunca tenha sido feito; apenas significa que não há confirmação na transcrição desta aula.

O professor encerra anunciando que continuará o código e fará os testes na próxima aula.

## 19. Código consolidado do estado da aula

Esta seção reúne somente os contratos e implementações de persistência já descritos, preservando o fluxo incompleto da Application.

### Domain — interface de leitura

```csharp
public interface IExpensesReadOnlyRepository
{
    Task<List<Expense>> GetAll();
    Task<Expense?> GetById(long id);
}
```

### Domain — interface de escrita

```csharp
public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);
    Task<bool> Delete(long id);
}
```

### Domain — nova interface de atualização

```csharp
public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);
    Task<Expense?> GetById(long id);
}
```

### Infrastructure — implementação dos três contratos

```csharp
internal class ExpensesRepository :
    IExpensesReadOnlyRepository,
    IExpensesWriteOnlyRepository,
    IExpensesUpdateOnlyRepository
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

    async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(expense => expense.Id == id);
    }

    public async Task<bool> Delete(long id)
    {
        var result = await _dbContext.Expenses
            .FirstOrDefaultAsync(expense => expense.Id == id);

        if (result is null)
            return false;

        _dbContext.Expenses.Remove(result);
        return true;
    }

    public void Update(Expense expense)
    {
        _dbContext.Expenses.Update(expense);
    }

    async Task<Expense?> IExpensesUpdateOnlyRepository.GetById(long id)
    {
        return await _dbContext.Expenses
            .FirstOrDefaultAsync(expense => expense.Id == id);
    }
}
```

Os métodos Add, GetAll e Delete foram desenvolvidos nas aulas anteriores. O código acima consolida os novos membros e a implementação explícita dos dois GetById.

### Application — contrato do caso de uso

```csharp
public interface IUpdateExpenseUseCase
{
    Task Execute(long id, RequestExpenseJson request);
}
```

### Application — estrutura parcial

```csharp
public class UpdateExpenseUseCase : IUpdateExpenseUseCase
{
    private readonly IExpensesUpdateOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateExpenseUseCase(
        IExpensesUpdateOnlyRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task Execute(long id, RequestExpenseJson request)
    {
        // Preservar a validação já existente.
        // Busca, verificação de ausência, mapeamento e
        // atualização ainda serão concluídos na próxima aula.

        throw new NotImplementedException();
    }
}
```

**Observação:** o throw acima é um marcador didático desta reconstrução, não uma instrução mostrada pelo professor. Ele impede que o exemplo parcial seja confundido com uma operação de atualização funcional. O esqueleto real narrado contém Validate(request) e Commit(), mas está incompleto.

O material não antecipa a implementação final do caso de uso.

## 20. Cuidados técnicos e distinções

### 20.1 Implementação explícita não é sobrecarga

Os membros possuem a mesma assinatura, mas são associados a interfaces distintas. O qualificador da interface permite fornecer implementações independentes.

### 20.2 Não utilizar public na implementação explícita

A implementação é acessada pelo contrato de interface.

Uma declaração como public async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id) é incorreta nessa sintaxe.

### 20.3 Nullable reference types

Task<Expense?> comunica que a tarefa pode produzir uma referência nula.

A anotação precisa ser coerente entre contrato e implementação.

### 20.4 Tracking não é o mesmo que permissão

IExpensesReadOnlyRepository limita as operações diretamente disponíveis pelo contrato, mas não torna os objetos retornados imutáveis nem implementa autorização.

AsNoTracking desabilita o rastreamento da consulta, mas não proíbe o uso posterior de operações explícitas de persistência.

### 20.5 Atualização desconectada é possível

DbSet.Update pode associar uma entidade anteriormente desconectada. Isso não elimina possíveis conflitos de rastreamento com outra instância da mesma chave.

A consulta rastreada adotada pelo professor evita depender de uma estratégia desconectada nesse caso.

### 20.6 DbSet.Update não é void

O retorno da API do EF Core é EntityEntry<TEntity>. O repositório opta por descartá-lo e expor um método void.

### 20.7 Commit continua necessário no desenho do curso

Update prepara mudanças; SaveChangesAsync as efetiva.

A operação de marcação de estado não precisa ser assíncrona apenas por fazer parte de um caso de uso assíncrono.

### 20.8 Reutilizar DTOs exige avaliar o contrato

A classe RequestExpenseJson é compartilhada porque os dados do cadastro e da atualização coincidem no cenário atual.

Em outros casos, a diferença de regras, campos permitidos ou evolução independente pode justificar contratos separados.

### 20.9 Rebuild não é execução de testes

O professor confirma oito projetos compilados, não oito testes aprovados.

A alteração de referências permite que os projetos de testes compilem, mas seus resultados de execução não são demonstrados.

### 20.10 Registro de dependências

Como a aula 115 demonstrou, a criação de uma interface e sua implementação concreta não são suficientes para a DI resolver automaticamente o serviço.

Quando a integração do novo repositório for concluída, será necessário garantir seu registro.

Exemplo de registro esperado, apresentado apenas como complemento:

```csharp
services.AddScoped<
    IExpensesUpdateOnlyRepository,
    ExpensesRepository>();
```

Esse registro não foi mostrado na transcrição atual.

### 20.11 Uma alternativa à implementação explícita

Poderiam ser usados nomes de método distintos, como GetByIdForUpdate, ou classes concretas separadas.

O professor escolhe preservar GetById em ambos os contratos e ensinar a implementação explícita de interfaces.

Essa escolha é legítima e não constitui uma obrigação universal de design.

### 20.12 PUT e atualização parcial

O PUT normalmente representa a substituição ou atualização da representação do recurso conforme o contrato da API; PATCH é utilizado frequentemente para mudanças parciais.

Nesta aula, cadastro e atualização compartilham o mesmo conjunto de dados. Não é desenvolvido um endpoint PATCH nem um mecanismo de atualização parcial.

---

## 21. Exercícios e perguntas de revisão

### Exercício 1 — escolher a interface

Qual interface o caso de uso de atualização deve receber?

**Resposta:** IExpensesUpdateOnlyRepository.

### Exercício 2 — declarar os métodos

```csharp
public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);
    Task<Expense?> GetById(long id);
}
```

O primeiro método prepara a atualização. O segundo recupera assincronamente a entidade, podendo retornar null.

### Exercício 3 — implementação explícita

Implemente GetById para duas interfaces com comportamentos distintos.

```csharp
async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == id);
}

async Task<Expense?> IExpensesUpdateOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

### Exercício 4 — identificar o erro

Por que a classe não pode declarar dois métodos públicos GetById(long id) com a mesma assinatura?

**Resposta:** o compilador não consegue distinguir os membros pela assinatura. A diferença de implementação interna não caracteriza sobrecarga.

### Exercício 5 — tracking

AsNoTracking torna impossível atualizar uma entidade?

**Resposta:** não. Alterações em instâncias desconectadas não são detectadas automaticamente pelo contexto, mas uma atualização explícita pode associá-las e marcar seu estado. O curso escolhe uma consulta rastreada para o fluxo de atualização.

### Exercício 6 — retorno do EF Core

DbSet.Update retorna void?

**Resposta:** não. Retorna EntityEntry<TEntity>. O contrato próprio do repositório pode ser void porque não precisa expor esse valor.

### Exercício 7 — interpretar o resultado do rebuild

Oito projetos compilados significa que todos os testes de unidade passaram?

**Resposta:** não. A gravação confirma a compilação, não a execução dos testes.

### Exercício 8 — renomeação

Por que RegisterExpenseValidator passa a ser ExpenseValidator?

**Resposta:** porque suas regras serão utilizadas em mais de um contexto de despesas, não somente no registro.

### Perguntas de revisão

1. Qual operação do CRUD faltava no CashFlow?
2. Qual método HTTP é utilizado para atualização?
3. Quais status são documentados no novo endpoint?
4. De onde vêm o ID e os dados de atualização?
5. Por que RequestRegisterExpenseJson foi renomeado?
6. Em que situações DTOs de cadastro e atualização deveriam ser separados?
7. Quais dependências o novo use case recebe?
8. Por que o validador foi movido da pasta Register para Expenses?
9. O que o professor fez para corrigir os erros nos testes de unidade?
10. Quantos projetos foram compilados com sucesso?
11. Qual nova interface foi criada?
12. Por que a operação Update do repositório pode retornar void?
13. Qual é o verdadeiro tipo de retorno de DbSet.Update?
14. Quais métodos a nova interface declara?
15. Por que GetById retorna Expense?
16. Por que a implementação pública existente inicialmente satisfaz os dois contratos?
17. Por que dois métodos públicos com assinaturas idênticas não podem coexistir?
18. O que é implementação explícita de interfaces?
19. Qual modificador é removido na implementação explícita?
20. Qual método utiliza AsNoTracking?
21. Qual método utiliza o rastreamento padrão do contexto?
22. Como o tipo da interface determina a implementação chamada?
23. O professor concluiu a atualização e testou o PUT nesta gravação?
24. Qual configuração de DI do novo repositório ainda não foi demonstrada?

---

## 22. Resumo e mapa mental

| Componente | Situação ao final da aula |
|---|---|
| API | Endpoint PUT preparado, com respostas 204/400/404 |
| Communication | RequestRegisterExpenseJson renomeado para RequestExpenseJson |
| Application | Validador movido e renomeado para ExpenseValidator |
| Testes | Referências de nome/namespace corrigidas |
| Build | Oito projetos compilados com sucesso no rebuild intermediário |
| Domain | IExpensesUpdateOnlyRepository criado |
| Infrastructure | Update e dois GetById com implementação explícita |
| DI da Application | Registro do novo caso de uso informado como concluído |
| DI da Infrastructure | Registro da nova interface ainda não demonstrado |
| Fluxo de atualização | Incompleto, continuação anunciada |
| Teste do PUT | Não realizado nesta gravação |

```text
                  AULA 116 — UPDATE (PARTE 1)
                               |
                +--------------+---------------+
                |                              |
              API                           Application
                |                              |
              PUT                      UpdateExpenseUseCase
                |                              |
        ID na rota + body            RequestExpenseJson
                |                     ExpenseValidator
        204 / 400 / 404                       |
                                               v
                                IExpensesUpdateOnlyRepository
                                     |                 |
                                 GetById(id)       Update(expense)
                                     |                 |
                                     v                 v
                                  ExpensesRepository
                                     |
                   +-----------------+------------------+
                   |                                    |
         IExpensesReadOnlyRepository         IExpensesUpdateOnlyRepository
                   |                                    |
         GetById explícito                    GetById explícito
                   |                                    |
             AsNoTracking                    Rastreamento padrão
                   |                                    |
                   +-------------+----------------------+
                                 |
                 Implementação explícita de interfaces
                                 |
                  Mesma assinatura, comportamentos distintos

                PRÓXIMA AULA:
                finalizar o use case e testar o PUT
```

### Referências técnicas complementares

- [Microsoft Learn — Implementação explícita de interfaces em C#](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- [Microsoft Learn — Tracking vs. No-Tracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)
- [Microsoft Learn — Explicit Tracking](https://learn.microsoft.com/en-us/ef/core/change-tracking/explicit-tracking)
- [Microsoft Learn — DbSet<TEntity>.Update](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.update)
- [Microsoft Learn — Basic SaveChanges](https://learn.microsoft.com/en-us/ef/core/saving/basic)

**Conclusão:** a aula 116 prepara o fluxo de atualização do CashFlow e ensina a implementação explícita de interfaces em C#. O professor reutiliza RequestExpenseJson e ExpenseValidator entre cadastro e atualização, corrige referências dos testes e cria um contrato específico de atualização. A mesma classe ExpensesRepository passa a implementar dois GetById de assinatura idêntica, mas com estratégias diferentes de rastreamento. O caso de uso de atualização recebe o novo contrato. A gravação não termina o Execute nem testa o endpoint; esses passos ficam reservados para a próxima aula.
