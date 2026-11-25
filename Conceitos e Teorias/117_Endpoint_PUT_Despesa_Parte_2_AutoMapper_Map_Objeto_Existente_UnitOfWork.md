# Aula 117 — Atualização de despesas (parte 2): AutoMapper sobre entidade existente, Unit of Work e teste do PUT

> **Continuação direta da aula 116:** o DTO de entrada e o validador já foram compartilhados entre registro e atualização, e o repositório já implementa explicitamente dois métodos GetById com comportamentos diferentes de rastreamento.
>
> **Objetivo:** finalizar o UpdateExpenseUseCase, mapear os dados recebidos sobre a entidade existente, registrar IExpensesUpdateOnlyRepository na injeção de dependência e testar a persistência da alteração.
>
> **Resultado demonstrado:** a despesa de ID **31**, anteriormente **Comida**, com valor **15** e tipo de pagamento **0**, passa a ser **Pizza**, com descrição **Pizza, 4 queijos**, valor **36** e tipo de pagamento **2**. A API devolve **HTTP 204 No Content**, e uma nova consulta SELECT confirma a atualização no banco.
>
> **Fidelidade:** os exemplos reconstroem a estrutura narrada. A data exata e a grafia literal da descrição não são integralmente transcritas. O professor testa o caminho de sucesso do PUT, não os caminhos de erro 400 e 404. Complementos técnicos estão identificados separadamente.

## Índice

1. Recapitulação da aula anterior
2. O fluxo completo da atualização
3. Validando os dados recebidos
4. Recuperando a entidade para atualização
5. Tratando a despesa inexistente
6. O problema do mapeamento manual
7. AutoMapper: criando uma nova instância
8. AutoMapper: mapeando sobre uma instância existente
9. Preservação da identidade e do Change Tracker
10. Preparando a atualização e confirmando as alterações
11. Registro do repositório na DI
12. Código consolidado por camada
13. Respostas HTTP e tratamento de erros
14. Teste no Swagger e depuração
15. Verificação no banco de dados
16. Precisões técnicas e boas práticas
17. Exercícios e perguntas de revisão
18. Resumo e mapa mental

---

## 1. Recapitulação da aula anterior

A aula 116 preparou a arquitetura do fluxo de atualização.

O professor retoma três mudanças:

### DTO compartilhado

RequestRegisterExpenseJson foi renomeado para RequestExpenseJson, pois os dados utilizados no cadastro e na atualização são os mesmos no contexto atual.

### Validador compartilhado

RegisterExpenseValidator foi movido da pasta Register para o contexto geral de Expenses e renomeado para ExpenseValidator.

A mudança provocou erros de compilação nas referências dos testes de unidade, corrigidos durante a gravação anterior.

### Segregação dos contratos de repositório

A interface IExpensesUpdateOnlyRepository foi criada para oferecer:

```csharp
public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);

    Task<Expense?> GetById(long id);
}
```

A mesma classe ExpensesRepository implementa esse contrato e os contratos de leitura e escrita.

Como as interfaces de leitura e atualização possuem GetById(long id), o professor utilizou implementação explícita de interfaces para distinguir seus comportamentos.

| Contrato | Comportamento de GetById |
|---|---|
| IExpensesReadOnlyRepository | Consulta com AsNoTracking() |
| IExpensesUpdateOnlyRepository | Consulta sem AsNoTracking(), utilizando o rastreamento padrão do contexto |

Nesta aula, o foco passa da preparação da infraestrutura para a coordenação efetiva da atualização na Application.

---

## 2. O fluxo completo da atualização

O método Execute precisa coordenar as seguintes operações na ordem demonstrada:

1. Validar os novos dados.
2. Buscar a despesa existente pelo ID.
3. Verificar se a entidade foi encontrada.
4. Mapear os dados da requisição sobre a entidade existente.
5. Solicitar a atualização ao repositório.
6. Confirmar as mudanças por meio do Unit of Work.

```text
PUT com ID na rota e dados no corpo
                 |
                 v
        UpdateExpenseUseCase
                 |
           Validate(request)
                 |
                 v
        await GetById(id)
                 |
        +--------+--------+
        |                 |
       null           Expense existente
        |                 |
 NotFoundException        v
        |         Map(request, expense)
      HTTP 404            |
                          v
                   Update(expense)
                          |
                          v
                    await Commit()
                          |
                          v
                    HTTP 204
```

Uma falha na validação interrompe o fluxo antes da consulta e produz o tratamento de erro de validação já utilizado no projeto.

---

## 3. Validando os dados recebidos

O professor enfatiza que a atualização também precisa validar os dados.

A operação não deve permitir que uma despesa válida seja modificada para um estado que viola as regras aplicadas no cadastro.

### Primeira instrução do Execute

```csharp
Validate(request);
```

O método utiliza o ExpenseValidator compartilhado, preparado na aula anterior.

### Estrutura ilustrativa do mecanismo

```csharp
private void Validate(RequestExpenseJson request)
{
    var validator = new ExpenseValidator();

    var result = validator.Validate(request);

    if (!result.IsValid)
    {
        var errors = result.Errors
            .Select(error => error.ErrorMessage)
            .ToList();

        throw new ErrorOnValidationException(errors);
    }
}
```

O exemplo reproduz o padrão de validação já utilizado no CashFlow, sem afirmar que cada identificador ou linha aparece exatamente dessa maneira na gravação.

A aula não altera as regras internas do ExpenseValidator. O professor apenas reutiliza o mecanismo existente.

### Por que validar primeiro?

Na ordem adotada pelo professor, uma requisição inválida não precisa prosseguir até a consulta da despesa.

Isso preserva as regras de entrada e evita executar a atualização com dados inadequados.

Como consequência da ordem escolhida, se o corpo for inválido e o ID também não existir, a validação poderá produzir o erro 400 antes de o caso de uso verificar a existência da despesa.

Essa consequência decorre do fluxo; não foi testada na gravação.

---

## 4. Recuperando a entidade para atualização

Depois de validar a requisição, o professor consulta o repositório.

```csharp
var expense = await _repository.GetById(id);
```

### Qual repositório é utilizado?

A dependência é do tipo IExpensesUpdateOnlyRepository, e não IExpensesReadOnlyRepository.

Por isso, a chamada GetById é encaminhada à implementação explícita correspondente:

```csharp
async Task<Expense?> IExpensesUpdateOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

A consulta de leitura permanece separada:

```csharp
async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### Qual é a diferença relevante?

A implementação de atualização não utiliza AsNoTracking().

No comportamento padrão de rastreamento do EF Core utilizado pelo curso, a entidade recuperada fica associada ao Change Tracker do DbContext.

### Por que o await é necessário?

GetById retorna Task<Expense?>.

É necessário aguardar a conclusão da operação para obter a referência à entidade ou null.

O professor percebe a ausência inicial do await e corrige a chamada durante a implementação.

---

## 5. Tratando a despesa inexistente

Assim como no caso de uso de consulta por ID, o professor verifica se a despesa foi encontrada.

```csharp
if (expense is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);
```

A grafia ResourceErrorMessages é uma reconstrução utilizada nos materiais das aulas anteriores. Deve ser conferida no projeto original.

### Por que reutilizar NotFoundException?

A exceção representa precisamente o cenário em que uma despesa não existe.

Ela já foi criada na aula 112 e integrada ao tratamento polimórfico da aula 114.

Assim, o caso de uso não precisa criar um novo tipo de exceção nem retornar diretamente um objeto de resposta HTTP.

### Efeito da verificação

Se expense for null:

- a atualização não prossegue;
- o mapeamento não é executado;
- Update e Commit não são chamados;
- a exceção é encaminhada ao filtro da API, que produz o erro previsto de recurso inexistente.

Caso contrário, o fluxo continua com uma entidade válida.

---

## 6. O problema do mapeamento manual

Depois de recuperar a entidade, é necessário aplicar os dados recebidos na requisição.

O professor demonstra inicialmente a atribuição do título:

```csharp
expense.Title = request.Title;
```

Essa abordagem poderia ser estendida às demais propriedades.

Exemplo conceitual:

```csharp
expense.Title = request.Title;
expense.Description = request.Description;
expense.Date = request.Date;
expense.Amount = request.Amount;
// PaymentType também precisa ser convertido, se os tipos diferirem.
```

O professor enumera título, descrição, data, valor e tipo de pagamento.

O exemplo acima não constitui o código final: o instrutor utiliza AutoMapper para evitar escrever manualmente cada atribuição.

### Motivação

O projeto já possui a configuração de mapeamento dos dados de entrada para Expense, utilizada no cadastro.

É possível aproveitar esse mecanismo na atualização, mas existe uma distinção fundamental:

**não queremos criar outra despesa; queremos alterar aquela que foi encontrada no banco.**

---

## 7. AutoMapper: criando uma nova instância

No cadastro, o AutoMapper já era utilizado da seguinte forma:

```csharp
var entity = _mapper.Map<Expense>(request);
```

Esse método recebe a origem e o tipo de destino.

No cenário apresentado, o AutoMapper cria uma nova instância de Expense e preenche suas propriedades conforme a configuração de mapeamento.

### Aplicação no cadastro

```text
RequestExpenseJson
        |
        v
Map<Expense>(request)
        |
        v
Nova instância de Expense
        |
        v
Repository.Add(entity)
```

Isso faz sentido porque o caso de uso está criando uma despesa que ainda não possui uma entidade previamente recuperada para atualização.

### Por que não utilizar diretamente essa forma no PUT?

Na atualização, já existe um objeto Expense recuperado pelo ID.

Criar outra instância seria uma estratégia diferente, que exigiria cuidados adicionais com a identificação, o estado de rastreamento e os demais dados da entidade.

A escolha do professor é preservar a instância existente.

---

## 8. AutoMapper: mapeando sobre uma instância existente

O instrutor apresenta outra sobrecarga de Map:

```csharp
_mapper.Map(request, expense);
```

A primeira variável é a origem dos dados.

A segunda variável é o objeto de destino que já existe.

### Significado

```text
request (origem)
    |
    | Map(request, expense)
    v
expense (instância existente)
```

O AutoMapper aplica os valores da origem às propriedades do destino, de acordo com o mapeamento configurado.

Não é necessário guardar o retorno da chamada neste cenário, porque as propriedades da entidade fornecida como destino são alteradas.

### Diferença entre as duas formas

| Código | Comportamento no cenário demonstrado |
|---|---|
| _mapper.Map<Expense>(request) | Cria uma nova instância de Expense a partir da origem |
| _mapper.Map(request, expense) | Mapeia os dados para a instância de Expense existente |

### O que significa omitir os argumentos genéricos?

Na chamada Map(request, expense), os tipos de origem e destino podem ser inferidos pelo compilador a partir dos argumentos.

Isso não significa que a sobrecarga seja necessariamente um método não genérico. O ponto importante é que o destino é fornecido como argumento.

### Forma explícita equivalente

Como complemento didático, para o mapeamento convencional utilizado no projeto, a chamada pode ser expressa com os tipos:

```csharp
_mapper.Map<RequestExpenseJson, Expense>(request, expense);
```

O professor utiliza a forma mais concisa, com inferência de tipos.

### Exemplo ilustrativo

```csharp
var expense = await _repository.GetById(id);

if (expense is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);

_mapper.Map(request, expense);
```

A verificação de null garante que o fluxo só prossegue com uma instância de destino existente.

Não se deve depender de um comportamento genérico do AutoMapper para transformar um destino nulo em erro de negócio; o tratamento explícito de ausência é responsabilidade do caso de uso.

---

## 9. Preservação da identidade e do Change Tracker

O método GetById de atualização devolve a entidade que será modificada.

O AutoMapper recebe essa entidade como destino.

### Exemplo conceitual baseado no teste

Antes do mapeamento:

```text
expense
    Id = 31
    Title = Comida
    Amount = 15
    PaymentType = 0
```

Depois de Map(request, expense):

```text
expense
    Id = 31
    Title = Pizza
    Amount = 36
    PaymentType = 2
```

A entidade continua representando a mesma despesa identificada por 31.

O professor inspeciona o objeto no depurador antes e depois da chamada Map e demonstra a alteração de seus valores.

### Precisão técnica

O fato de mapear sobre uma instância existente preserva a referência ao objeto de destino nesse cenário. Entretanto, a preservação de propriedades específicas depende também da configuração de mapeamento.

O RequestExpenseJson descrito nas aulas anteriores não possui um ID de despesa entre seus campos de entrada. O identificador é recebido separadamente pela rota.

É importante que o mapeamento não permita sobrescrever inadvertidamente a chave primária ou outros dados controlados pelo servidor.

### Relação com tracking

Como a entidade existente é recuperada por uma consulta rastreada, o contexto mantém sua associação com aquela instância.

Aplicar os novos dados sobre ela permite trabalhar com o objeto já conhecido pelo DbContext.

Isso evita, nesse fluxo, a necessidade de criar uma segunda instância com a mesma chave e tentar associá-la ao mesmo contexto.

---

## 10. Preparando a atualização e confirmando as alterações

Após mapear os dados, o professor chama Update.

```csharp
_repository.Update(expense);
```

O método foi criado na aula 116:

```csharp
public void Update(Expense expense)
{
    _dbContext.Expenses.Update(expense);
}
```

A operação é síncrona porque prepara o estado da entidade no contexto.

A persistência efetiva é solicitada posteriormente:

```csharp
await _unitOfWork.Commit();
```

O Unit of Work continua utilizando SaveChangesAsync:

```csharp
public async Task Commit()
{
    await _dbContext.SaveChangesAsync();
}
```

### Ordem das operações

```text
Recuperar a entidade
         |
Mapear os novos dados
         |
Update(expense)
         |
Commit()
         |
Banco atualizado
```

Se Commit lançar uma exceção, o caso de uso não conclui com sucesso e o controller não deve executar o retorno 204 do caminho normal.

### Complemento técnico: Update em entidade rastreada

O curso chama explicitamente DbSet.Update mesmo após recuperar uma entidade com tracking.

Essa implementação é preservada neste material.

Entretanto, quando uma entidade existente está rastreada e suas propriedades são modificadas, o EF Core normalmente pode detectar essas alterações durante SaveChangesAsync sem que Update seja chamado.

DbSet.Update marca entidades e propriedades conforme sua estratégia de rastreamento, podendo gerar uma atualização mais ampla do que a detecção de alterações isoladas.

A aula não compara SQL produzido com e sem Update, nem mede desempenho. Portanto, não é possível afirmar que uma das estratégias tenha sido mais eficiente no teste realizado.

---

## 11. Registro do repositório na DI

O professor lembra que ainda precisa registrar a nova interface no container da Infrastructure.

Esse era um ponto não demonstrado na aula anterior.

### Registro

```csharp
services.AddScoped<
    IExpensesUpdateOnlyRepository,
    ExpensesRepository>();
```

### Registros acumulados do repositório

```csharp
services.AddScoped<
    IExpensesReadOnlyRepository,
    ExpensesRepository>();

services.AddScoped<
    IExpensesWriteOnlyRepository,
    ExpensesRepository>();

services.AddScoped<
    IExpensesUpdateOnlyRepository,
    ExpensesRepository>();
```

O container passa a conseguir resolver o novo contrato exigido pelo UpdateExpenseUseCase.

O registro do próprio caso de uso já havia sido informado como realizado na aula 116:

```csharp
services.AddScoped<IUpdateExpenseUseCase, UpdateExpenseUseCase>();
```

### Relação com o erro da aula 115

Na implementação da exclusão, o professor havia esquecido de registrar o novo caso de uso e observado HTTP 500.

Aqui, ele identifica o registro ausente antes de executar o teste de atualização.

### Atenção à identidade das instâncias

Os três registros podem produzir instâncias distintas de ExpensesRepository para cada contrato no mesmo escopo. Eles registram a mesma classe de implementação, não necessariamente a mesma instância.

O DbContext utilizado pelo repositório e pelo Unit of Work normalmente é compartilhado dentro do escopo da requisição, conforme a configuração Scoped estabelecida anteriormente.

---

## 12. Código consolidado por camada

Os exemplos reconstroem os componentes relevantes, preservando a implementação apresentada e os contratos estabelecidos nas aulas anteriores.

### 12.1 Domain — IExpensesUpdateOnlyRepository

```csharp
public interface IExpensesUpdateOnlyRepository
{
    void Update(Expense expense);

    Task<Expense?> GetById(long id);
}
```

### 12.2 Infrastructure — membros relevantes de ExpensesRepository

```csharp
async Task<Expense?> IExpensesReadOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}

async Task<Expense?> IExpensesUpdateOnlyRepository.GetById(long id)
{
    return await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);
}

public void Update(Expense expense)
{
    _dbContext.Expenses.Update(expense);
}
```

Os demais métodos Add, Delete e GetAll são preservados.

### 12.3 Application — IUpdateExpenseUseCase

```csharp
public interface IUpdateExpenseUseCase
{
    Task Execute(long id, RequestExpenseJson request);
}
```

### 12.4 Application — UpdateExpenseUseCase

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
        Validate(request);

        var expense = await _repository.GetById(id);

        if (expense is null)
            throw new NotFoundException(
                ResourceErrorMessages.ExpenseNotFound);

        _mapper.Map(request, expense);

        _repository.Update(expense);

        await _unitOfWork.Commit();
    }

    private void Validate(RequestExpenseJson request)
    {
        var validator = new ExpenseValidator();
        var result = validator.Validate(request);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .Select(error => error.ErrorMessage)
                .ToList();

            throw new ErrorOnValidationException(errors);
        }
    }
}
```

O corpo de Execute corresponde à sequência demonstrada. O construtor, o método Validate e o identificador da classe de recursos foram reconstruídos com base no padrão do projeto; seus detalhes literais devem ser conferidos na solução.

### 12.5 Application — AutoMapper

O mapa de requisição para entidade existente no projeto é reutilizado:

```csharp
CreateMap<RequestExpenseJson, Expense>();
```

O professor não demonstra a criação de um novo mapa específico para atualização nesta aula.

### 12.6 Infrastructure — DependencyInjectionExtension

```csharp
services.AddScoped<
    IExpensesUpdateOnlyRepository,
    ExpensesRepository>();
```

### 12.7 Application — DependencyInjectionExtension

```csharp
services.AddScoped<
    IUpdateExpenseUseCase,
    UpdateExpenseUseCase>();
```

### 12.8 API — endpoint PUT

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

O controller foi apresentado na aula 116. O nome literal da action, a rota completa e a ordem dos parâmetros não são confirmados integralmente na transcrição.

---

## 13. Respostas HTTP e tratamento de erros

O endpoint foi preparado para três resultados de negócio:

| Status | Condição | Comportamento |
|---|---|---|
| 204 No Content | Atualização concluída | Controller devolve NoContent() |
| 400 Bad Request | Dados inválidos | ErrorOnValidationException |
| 404 Not Found | Despesa não encontrada | NotFoundException |

### Validação

O ExpenseValidator verifica os dados recebidos. O tratamento de erro de validação já existente fornece as mensagens ao filtro.

### Recurso inexistente

O use case verifica expense is null e lança NotFoundException, reutilizando a mensagem ExpenseNotFound.

### Sucesso

O caso de uso termina após Commit. A action retorna HTTP 204, sem corpo.

### Relação com Open/Closed

A aula 114 tornou o filtro capaz de processar as exceções conhecidas por meio da abstração CashFlowException:

```csharp
public abstract int StatusCode { get; }

public abstract List<string> GetErrors();
```

Não é necessário criar um novo ramo no filtro para os erros de validação e de recurso inexistente do PUT.

O novo caso de uso reutiliza os tipos de exceção já existentes.

### Limite da demonstração

O professor confirma o HTTP 204 no teste final. Não executa, nesta gravação, um PUT com dados inválidos nem um PUT com ID inexistente para demonstrar os status 400 e 404.

Esses caminhos são previstos na implementação e no contrato, mas não devem ser apresentados como novos testes bem-sucedidos.

---

## 14. Teste no Swagger e depuração

Após registrar IExpensesUpdateOnlyRepository, o professor inicia a API.

Ele escolhe a despesa de ID 31, registrada anteriormente com título Comida.

Durante a fala, menciona inicialmente 33, mas se corrige e utiliza **31**. O ID 33 correspondia à Pizza excluída na aula 115.

### Valores observados

| Campo | Antes | Depois |
|---|---|---|
| ID | 31 | 31 |
| Título | Comida | Pizza |
| Descrição | Não informada integralmente | Pizza, 4 queijos |
| Valor | 15 | 36 |
| Tipo de pagamento | 0 | 2 |
| Data | Valor não especificado | Valor não especificado |

A tabela não atribui uma data específica ao teste, pois a transcrição não fornece esse valor.

### Sequência de depuração

1. Expandir o endpoint PUT no Swagger.
2. Selecionar Try it out.
3. Informar ID 31.
4. Preencher os novos dados.
5. Executar a requisição.
6. Verificar que a validação produziu um resultado válido.
7. Utilizar F11 para entrar no GetById do repositório.
8. Confirmar que o método executado é o da interface UpdateOnly, sem AsNoTracking().
9. Confirmar que a entidade não é nula.
10. Inspecionar os valores antigos da despesa.
11. Utilizar F10 para executar Map(request, expense).
12. Inspecionar os novos valores da mesma variável.
13. Executar Update(expense).
14. Executar Commit().
15. Confirmar o retorno HTTP 204.

### Principal aprendizado do teste

O professor não verifica apenas o resultado HTTP.

Ele observa diretamente a diferença entre os dados da entidade antes e depois do mapeamento e confirma a escolha correta da implementação explícita de GetById.

---

## 15. Verificação no banco de dados

Depois de receber HTTP 204, o professor abre a consulta que exibe as despesas no MySQL.

O resultado de uma consulta executada anteriormente ainda apresenta os dados antigos.

Ele executa novamente o SELECT para atualizar a visualização.

### Resultado confirmado

A linha de ID 31 passa a exibir:

- título Pizza;
- descrição correspondente a pizza de quatro queijos;
- valor 36;
- tipo de pagamento 2.

O professor informa que os dados alterados foram persistidos corretamente.

### Por que executar o SELECT novamente?

O resultado de uma consulta anterior representa os dados recuperados naquele momento.

Ele não necessariamente é atualizado automaticamente quando outra operação modifica a tabela.

Executar novamente a consulta permite observar o estado atual dos registros.

### O que foi efetivamente validado?

- A resolução do repositório pela DI.
- O fluxo de validação com dados válidos.
- A seleção da implementação de GetById para atualização.
- O mapeamento sobre a entidade existente.
- A conclusão de Update e Commit.
- O retorno HTTP 204.
- A persistência dos novos valores no MySQL.

Não foi exibido o comando SQL UPDATE gerado pelo EF Core. A gravação também não apresenta benchmark ou execução de testes automatizados.

---

## 16. Precisões técnicas e boas práticas

Esta seção contém complementos, não mudanças adicionais efetuadas pelo instrutor.

### 16.1 AsNoTracking não torna impossível a atualização

A consulta sem AsNoTracking é a estratégia escolhida pelo professor para recuperar a entidade rastreada.

Contudo, uma entidade desconectada pode ser atualizada explicitamente por mecanismos como Attach ou Update, desde que a estratégia considere chave, estado, concorrência e possíveis conflitos de rastreamento.

A ausência de tracking impede a detecção automática de mudanças em uma instância desconectada, mas não proíbe toda forma de atualização.

### 16.2 Map não substitui SaveChangesAsync

AutoMapper transforma dados entre objetos. Ele não persiste alterações no banco.

A persistência depende do EF Core e do Unit of Work.

### 16.3 Map sobre destino existente não torna todas as propriedades seguras

O destino já existir não significa que todas as propriedades devam ser modificadas.

É necessário que o DTO e o Profile representem os campos efetivamente permitidos.

Chaves, proprietários, permissões e campos de auditoria podem exigir regras específicas.

### 16.4 Mapeamento de valores nulos

O fluxo compartilha o DTO de criação e atualização.

Dependendo da configuração do AutoMapper, um valor nulo da origem pode substituir um valor existente no destino.

Isso pode ser adequado para uma atualização integral, mas exige avaliação se a operação pretende oferecer atualizações parciais.

A gravação não implementa PATCH nem regras especiais de preservação de campos omitidos.

### 16.5 Atualizar não é inserir

A operação utiliza o ID recebido pela rota para recuperar uma despesa existente.

Se ela não existir, o caso de uso lança NotFoundException em vez de criar uma nova entidade.

Essa é a regra específica do PUT do CashFlow. PUT não implica universalmente que a criação de um recurso inexistente seja proibida; o comportamento depende do contrato da API.

### 16.6 Update pode ser redundante em entidade rastreada

O curso mantém Update no contrato de repositório para representar a operação de atualização.

No EF Core, uma entidade rastreada modificada em memória pode ser persistida por SaveChangesAsync sem a chamada explícita de Update.

Não se deve remover a chamada no projeto sem considerar a arquitetura, os contratos e a estratégia de persistência adotada.

### 16.7 Validação e autorização são responsabilidades distintas

Validar título, valor e outros dados não comprova que uma pessoa possui permissão para alterar uma despesa.

O projeto atual não apresenta autorização por proprietário da despesa. Uma evolução multiusuário exigiria verificações adicionais, como discutido na aula 115.

### 16.8 Concorrência

Entre a recuperação da entidade e o commit, outra requisição pode alterar ou excluir o mesmo registro.

Uma implementação que precise detectar conflitos de atualização pode utilizar mecanismos de concorrência do EF Core e uma política apropriada de tratamento.

Não há estratégia de concorrência implementada ou testada nesta gravação.

### 16.9 Reutilização dos contratos

A aula reúne vários conceitos do módulo:

| Conceito | Aplicação |
|---|---|
| ISP | Contrato especializado de atualização |
| Implementação explícita | GetById distinto por interface |
| DI | Resolução do repositório e caso de uso |
| AutoMapper | Mapeamento para destino existente |
| Validação | Reutilização de ExpenseValidator |
| Unit of Work | Persistência ao final do fluxo |
| OCP | Reutilização do tratamento genérico de exceções |
| Async/await | Consulta e commit assíncronos |

---

## 17. Exercícios e perguntas de revisão

### Exercício 1 — buscar a despesa

Qual instrução recupera a entidade pelo repositório de atualização?

```csharp
var expense = await _repository.GetById(id);
```

### Exercício 2 — tratar ausência

```csharp
if (expense is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);
```

### Exercício 3 — escolher a sobrecarga de AutoMapper

Qual forma deve ser utilizada para aplicar a requisição a uma entidade existente?

**Resposta:**

```csharp
_mapper.Map(request, expense);
```

A segunda variável representa o destino já instanciado.

### Exercício 4 — comparar cadastro e atualização

```csharp
var newExpense = _mapper.Map<Expense>(request);
_mapper.Map(request, existingExpense);
```

**Resposta:** no cenário convencional apresentado, a primeira chamada produz uma nova instância; a segunda mapeia os dados sobre a instância existente.

### Exercício 5 — persistir

```csharp
_repository.Update(expense);
await _unitOfWork.Commit();
```

O repositório prepara a atualização e o Unit of Work confirma as mudanças no banco.

### Exercício 6 — registrar a DI

```csharp
services.AddScoped<IExpensesUpdateOnlyRepository, ExpensesRepository>();
```

### Exercício 7 — ordenar o fluxo

Organize as operações de atualização.

**Resposta:** validar, buscar pelo ID, verificar ausência, mapear, atualizar e executar commit.

### Exercício 8 — interpretar o teste

Qual despesa foi atualizada?

**Resposta:** ID 31, anteriormente Comida, alterada para Pizza, valor 36 e tipo de pagamento 2. O professor confirma HTTP 204 e os novos dados no banco.

### Perguntas de revisão

1. Qual operação do CRUD é concluída nesta aula?
2. Por que RequestExpenseJson é reutilizado?
3. Por que o mesmo validador é utilizado no cadastro e na atualização?
4. Em qual momento a validação acontece?
5. Qual interface o UpdateExpenseUseCase recebe?
6. Qual GetById é chamado por essa interface?
7. Qual diferença de tracking existe entre os dois GetById?
8. Por que utilizar await ao recuperar a despesa?
9. O que acontece quando o repositório retorna null?
10. Qual exceção é reutilizada?
11. Por que o mapeamento precisa acontecer antes de Update?
12. Qual a diferença entre Map<Expense>(request) e Map(request, expense)?
13. Qual argumento é a origem e qual é o destino?
14. A chamada Map(request, expense) necessita de uma nova instância de Expense?
15. O AutoMapper persiste a alteração no banco?
16. Qual componente realiza o commit?
17. Qual registro de DI faltava?
18. Qual status indica atualização bem-sucedida?
19. Qual era o valor anterior da despesa atualizada?
20. Qual valor foi informado no PUT?
21. Qual era o tipo de pagamento anterior e qual passou a ser o novo?
22. Por que o professor utiliza F11 no GetById?
23. O que ele observa ao executar o mapeamento com F10?
24. Como a persistência é confirmada no MySQL?
25. Os testes HTTP 400 e 404 foram executados nesta gravação?
26. Uma entidade rastreada sempre precisa de DbSet.Update para salvar mudanças?
27. Por que o ID do recurso não deve ser inadvertidamente sobrescrito no mapeamento?
28. O fluxo de atualização implementado também oferece autorização por proprietário?

---

## 18. Resumo e mapa mental

| Componente | Resultado da aula 117 |
|---|---|
| Validação | ExpenseValidator reutilizado no PUT |
| Consulta | GetById da interface UpdateOnly |
| Entidade ausente | NotFoundException |
| AutoMapper | Map(request, expense) sobre destino existente |
| Repositório | Update(expense) |
| Persistência | await _unitOfWork.Commit() |
| DI | Registro de IExpensesUpdateOnlyRepository |
| HTTP | 204 confirmado; 400 e 404 previstos |
| Teste | Comida, ID 31, atualizada para Pizza |
| Banco de dados | Novo SELECT confirma a alteração |

```text
                    AULA 117 — PUT (PARTE 2)
                                |
                   UpdateExpenseUseCase
                                |
                       Validate(request)
                                |
                 IExpensesUpdateOnlyRepository
                                |
                        GetById(id)
                                |
                        Consulta rastreada
                                |
                   +------------+------------+
                   |                         |
                  null                     Expense
                   |                         |
           NotFoundException                 v
                   |                Map(request, expense)
                HTTP 404                     |
                                     mesma instância
                                             |
                                       Update(expense)
                                             |
                                       await Commit()
                                             |
                                          MySQL
                                             |
                                          HTTP 204

                      TESTE CONFIRMADO
                 ID 31: Comida -> Pizza
                 Valor: 15 -> 36
                 Pagamento: 0 -> 2
```

**Conclusão:** a aula 117 conclui o fluxo de atualização preparado na aula anterior. O caso de uso valida os dados, recupera a despesa pela interface especializada, trata a ausência do registro e utiliza AutoMapper para modificar a entidade já existente. A atualização é preparada pelo repositório e persistida pelo Unit of Work. O professor registra a nova dependência, confirma a implementação correta de GetById durante a depuração, recebe HTTP 204 e verifica os valores atualizados no MySQL. Com isso, o módulo passa a ter os fluxos de cadastro, consulta, exclusão e atualização implementados.
