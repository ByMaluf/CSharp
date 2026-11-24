# Aula 112 — NotFoundException: tratamento de despesa inexistente, herança de exceções e HTTP 404

> **Continuação direta da aula 111.** O endpoint GET por ID já recuperava uma despesa, mas ainda não tratava corretamente o caso em que o repositório retornava null. Nesta aula, o instrutor cria uma exceção customizada, reorganiza a classe base das exceções e adapta o filtro global para transformar a ausência do recurso em HTTP 404 Not Found.
>
> **Resultado demonstrado:** consulta de um identificador inexistente, resposta HTTP 404 e uma lista de mensagens contendo apenas a informação de que a despesa não foi encontrada.
>
> **Fidelidade:** os trechos abaixo reconstroem a estrutura descrita oralmente. A transcrição registra o identificador do teste como "Idset", provavelmente "ID sete", mas não permite confirmar sua representação literal. O nome ExpenseNotFound é citado como chave do recurso; a grafia exata da classe de recursos e o nome JSON da propriedade de erros devem ser conferidos no código original. Complementos técnicos estão identificados separadamente.

## Índice

1. Objetivo e problema deixado pela aula 111
2. Ausência de entidade: null não é uma falha de infraestrutura
3. Comparação com null: ==, !=, is null e is not null
4. Validação no GetExpenseByIdUseCase
5. Criação de NotFoundException
6. Reutilização da exceção em diferentes contextos
7. ResourceErrorMessages e ExpenseNotFound
8. Hierarquia das exceções
9. A propriedade Exception.Message
10. Encaminhamento de mensagens com base(message)
11. Alteração de CashFlowException
12. Impacto em ErrorOnValidationException
13. Pattern matching de tipo e variável
14. Atualização do filtro de exceções
15. NotFoundObjectResult e ResponseErrorJson
16. Código consolidado
17. Fluxo completo entre camadas
18. Teste no Swagger
19. Cuidados técnicos e alternativas
20. Exercícios e revisão
21. Resumo e mapa mental

---

## 1. Objetivo e problema deixado pela aula 111

Na aula anterior, o fluxo de recuperação de despesas por identificador foi implementado:

```csharp
var result = await _repository.GetById(id);
return _mapper.Map<ResponseExpenseJson>(result);
```

O repositório utilizava:

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

A consulta funcionava quando o ID correspondia a uma despesa existente. O teste com a despesa TV, ID 29, confirmou o caminho de sucesso.

Entretanto, FirstOrDefaultAsync pode devolver null. O código ainda não interpretava esse resultado como recurso inexistente.

### Problema que será resolvido

A API já documentava dois resultados possíveis:

| Situação | Status documentado |
|---|---|
| Encontrou a despesa | 200 OK |
| Não encontrou a despesa | 404 Not Found |

O segundo cenário ainda precisava ser implementado de verdade.

A finalidade da aula 112 é alinhar o **comportamento real da API ao contrato documentado**, aproveitando o filtro de exceções construído anteriormente.

## 2. Ausência de entidade: null não é uma falha de infraestrutura

O professor retoma o comportamento de FirstOrDefaultAsync.

O tipo Expense é uma classe. Quando a consulta não encontra uma correspondência, o valor padrão de uma referência a Expense é null.

```csharp
Expense? result = await _repository.GetById(id);
```

### O que esse resultado representa?

- Entidade não nula: uma despesa foi encontrada.
- Null: a consulta terminou, mas nenhuma despesa corresponde ao identificador.

É importante separar ausência de dados de problemas técnicos, como indisponibilidade do MySQL ou uma falha durante a execução da consulta.

FirstOrDefaultAsync permite que a ausência seja representada por null, mas não impede outras exceções de infraestrutura.

### Por que tratar na Application?

No desenho do curso, o repositório fornece os dados. A Application interpreta o resultado conforme a finalidade do caso de uso.

O GetExpenseByIdUseCase exige uma despesa existente para construir uma resposta detalhada. Portanto, ele é o local escolhido para decidir que null representa um recurso não encontrado.

## 3. Comparação com null: ==, !=, is null e is not null

O professor apresenta a condição clássica:

```csharp
if (result == null)
{
    // Tratar a ausência da despesa.
}
```

Depois, informa sua preferência pela sintaxe de pattern matching:

```csharp
if (result is null)
{
    // Tratar a ausência da despesa.
}
```

Para o cenário inverso:

```csharp
if (result is not null)
{
    // A referência não é nula.
}
```

| Sintaxe | Finalidade |
|---|---|
| result == null | Comparação de igualdade com null |
| result != null | Comparação de desigualdade com null |
| result is null | Correspondência com o padrão constante null |
| result is not null | Negação do padrão null |

### Precisão técnica — complemento

A preferência do instrutor tem uma vantagem além da legibilidade: is null não chama um operador de igualdade sobrecarregado pelo tipo. Já == pode ser influenciado por uma sobrecarga de operator ==.

O padrão is null está disponível a partir do C# 7. O padrão de negação is not null está disponível a partir do C# 9. A disponibilidade depende da versão da linguagem C# configurada para o projeto, não simplesmente da versão do runtime .NET.

Nesta aula, a condição escolhida é result is null.

## 4. Validação no GetExpenseByIdUseCase

Inicialmente, o professor coloca uma exceção genérica como exemplo temporário:

```csharp
if (result is null)
{
    throw new Exception();
}
```

Ele esclarece que substituirá essa exceção por uma classe específica do CashFlow.

### Por que não manter Exception?

O filtro da aplicação toma decisões de acordo com o tipo da exceção. Uma exceção genérica não comunica claramente que o problema é a ausência de um recurso.

A classe específica permitirá distinguir o cenário esperado de recurso inexistente de uma falha inesperada.

### Resultado pretendido

```csharp
var result = await _repository.GetById(id);

if (result is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);

return _mapper.Map<ResponseExpenseJson>(result);
```

O identificador ResourceErrorMessages é uma reconstrução da fala "Resource... ErrorMessages". ExpenseNotFound é a chave citada pelo instrutor.

### Por que verificar antes do AutoMapper?

O caso de uso não tem entidade válida para mapear quando o resultado é null. A verificação interrompe o caminho de sucesso e garante que a ausência seja comunicada de forma explícita.

## 5. Criação de NotFoundException

O instrutor abre o projeto de exceções do CashFlow e, dentro da pasta ExceptionBase, cria uma classe chamada NotFoundException.

Ele altera o modificador de internal para public e utiliza a exceção de validação existente como referência para a herança.

```csharp
public class NotFoundException : CashFlowException
{
}
```

Esse é o esqueleto inicial. Em seguida, será acrescentado um construtor que recebe a mensagem.

### Finalidade

NotFoundException representa uma situação em que a regra de negócio procura um recurso que deveria existir para que aquela operação possa prosseguir, mas não o encontra.

O tipo específico comunica essa situação ao filtro de exceções.

### O que não é responsabilidade da classe?

No modelo apresentado, NotFoundException não executa uma consulta, não conhece o controller e não cria diretamente uma resposta HTTP.

A tradução da exceção para HTTP 404 é realizada pelo filtro da API.

## 6. Reutilização da exceção em diferentes contextos

O professor explica por que a mensagem não deve ficar fixa dentro de NotFoundException.

A mesma classe poderá ser utilizada ao procurar recursos diferentes:

- uma despesa;
- uma receita;
- uma conta de usuário.

A condição geral é a mesma — recurso não encontrado —, mas o contexto determina qual mensagem deve ser apresentada.

### Forma adotada

```csharp
public NotFoundException(string message)
{
    // A mensagem ainda precisa ser encaminhada.
}
```

O instrutor acrescenta o parâmetro obrigatório message.

Quando o construtor muda, os lugares que criavam a exceção sem argumentos precisam ser ajustados.

### Conceito importante

O tipo identifica a categoria do erro. A mensagem informa o contexto específico do problema.

Assim, o filtro pode distinguir NotFoundException de outras exceções sem depender de comparar textos.

## 7. ResourceErrorMessages e ExpenseNotFound

O professor informa que já criou nos arquivos de recursos uma mensagem identificada como ExpenseNotFound.

Sua finalidade é comunicar que a despesa não foi encontrada.

```csharp
throw new NotFoundException(
    ResourceErrorMessages.ExpenseNotFound);
```

### Por que utilizar um recurso de mensagem?

O CashFlow já trabalha com recursos para mensagens de erro. Essa organização separa o código da regra de negócio dos textos apresentados ao consumidor.

A mensagem pode seguir a estratégia de localização existente no projeto, caso o recurso esteja configurado para a cultura correspondente.

### Limites do que foi mostrado

A gravação confirma a existência da chave ExpenseNotFound e sua finalidade. Não apresenta todas as traduções, os arquivos .resx completos nem os textos exatos em todos os idiomas.

O uso de ResourceErrorMessages acima representa a nomenclatura reconstruída do acesso ao recurso.

## 8. Hierarquia das exceções

Ao investigar como armazenar a mensagem, o professor percorre a cadeia de herança.

A hierarquia utilizada no projeto é:

```text
System.Exception
       |
       v
System.SystemException
       |
       v
CashFlowException (abstract)
       |
       +----------------------+
       |                      |
       v                      v
NotFoundException     ErrorOnValidationException
```

### Relação de tipo

Uma NotFoundException é uma CashFlowException.

Uma CashFlowException é uma SystemException no desenho atual do curso.

SystemException, por sua vez, herda de Exception.

Por isso, NotFoundException pode acessar os membros herdados e acessíveis de Exception, como Message.

### Complemento técnico

O fato de existir herança não significa que a classe derivada tenha acesso irrestrito a todos os membros privados das classes anteriores.

Além disso, construtores não são herdados como métodos comuns: cada classe precisa definir sua própria forma de construção e chamar um construtor apropriado da classe base.

Como orientação geral de design em .NET, exceções customizadas costumam derivar de Exception, diretamente ou por uma base própria. O uso de SystemException aqui é preservado porque integra a hierarquia demonstrada no curso; não é uma exigência para criar exceções customizadas.

## 9. A propriedade Exception.Message

O professor verifica que Exception já possui uma propriedade chamada Message.

Ela disponibiliza a mensagem associada à exceção.

Por isso, não é necessário criar outra propriedade de mensagem em NotFoundException.

### Tentativa incorreta

```csharp
public NotFoundException(string message)
{
    Message = message;
}
```

O professor observa que Message possui getter, mas não setter público ou protegido acessível para essa atribuição.

Portanto, o código acima não funciona.

### Solução

Fornecer o texto ao construtor apropriado da classe base.

```csharp
public NotFoundException(string message) : base(message)
{
}
```

A propriedade Message herdada poderá então fornecer o texto associado à exceção.

## 10. Encaminhamento de mensagens com base(message)

A expressão abaixo é um inicializador de construtor:

```csharp
: base(message)
```

Ela chama um construtor da classe base direta antes da execução do corpo do construtor derivado.

### Atenção: a classe base direta é CashFlowException

Ao escrever:

```csharp
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
```

o compilador procura um construtor compatível com string em CashFlowException, não diretamente em Exception.

O professor encontra um erro porque CashFlowException ainda não possuía esse construtor.

### Encaminhamento necessário

```text
new NotFoundException("Despesa não encontrada")
    |
    v
NotFoundException(string message)
    |
    | base(message)
    v
CashFlowException(string message)
    |
    | base(message)
    v
SystemException(string message)
    |
    v
Exception(string message)
    |
    v
Message disponibiliza a mensagem
```

A cadeia de inicialização preserva a mensagem sem duplicar propriedades.

## 11. Alteração de CashFlowException

O professor abre a classe CashFlowException e acrescenta o construtor que faltava.

### Estrutura reconstruída

```csharp
public abstract class CashFlowException : SystemException
{
    protected CashFlowException(string message)
        : base(message)
    {
    }
}
```

### Por que abstract?

A classe funciona como uma base comum para as exceções do projeto. Não é possível instanciá-la diretamente.

Ela estabelece uma categoria de exceções que pode ser reconhecida pelos componentes de tratamento de erros.

### Por que protected?

O construtor é destinado à construção das exceções derivadas.

Por exemplo:

```csharp
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
```

A classe derivada pode acessar o construtor protegido da base.

### O que acontece quando acrescentamos esse construtor?

Como a classe agora declara explicitamente um construtor que exige uma string, deixa de possuir o construtor padrão implícito sem parâmetros que existia antes.

Isso afeta outras exceções derivadas que dependiam do construtor sem argumentos, como ErrorOnValidationException.

## 12. Impacto em ErrorOnValidationException

O professor abre a classe de erro de validação e encontra um novo erro de compilação.

Essa classe possui uma lista de mensagens, denominada Errors.

A alteração em CashFlowException exige que ela encaminhe uma mensagem para o construtor da base.

### Alternativa apresentada e descartada

```csharp
protected CashFlowException()
{
}
```

Adicionar novamente um construtor sem parâmetros resolveria a incompatibilidade, mas o professor prefere não manter essa sobrecarga.

### Solução escolhida

O construtor de ErrorOnValidationException passa a invocar a base com string.Empty.

```csharp
public class ErrorOnValidationException : CashFlowException
{
    public List<string> Errors { get; }

    public ErrorOnValidationException(List<string> errors)
        : base(string.Empty)
    {
        Errors = errors;
    }
}
```

A declaração da propriedade e seu setter são reconstruções; o trecho confirmado pelo professor é a adição de base(string.Empty) ao construtor existente.

### Por que string.Empty?

O professor explica que a informação relevante no contexto de validação está na lista Errors, não na propriedade Message da exceção.

Assim, preserva o contrato anterior do filtro e atende ao novo construtor obrigatório da classe base.

### Comparação

| Exceção | Informação relevante para a resposta |
|---|---|
| ErrorOnValidationException | Errors: lista de strings |
| NotFoundException | Message: uma string |

**Complemento:** usar uma mensagem descritiva também na classe de validação poderia facilitar diagnóstico e logs. O curso opta por string.Empty nesta aula, e essa escolha é preservada.

## 13. Pattern matching de tipo e variável

Durante a alteração do filtro de exceções, o Visual Studio sugere a refatoração Use Pattern Matching.

O professor aceita a sugestão.

### Antes — representação conceitual

```csharp
if (context.Exception is ErrorOnValidationException)
{
    var ex = (ErrorOnValidationException)context.Exception;
    // Usa ex.Errors.
}
```

Há duas etapas: verificar o tipo e realizar o cast.

### Depois

```csharp
if (context.Exception is ErrorOnValidationException validationException)
{
    // Usa validationException.Errors.
}
```

O padrão de declaração verifica se a referência corresponde ao tipo indicado e, em caso positivo, introduz uma variável já com esse tipo.

### Aplicação à nova exceção

```csharp
else if (context.Exception is NotFoundException notFoundException)
{
    // Usa notFoundException.Message.
}
```

### O que significa "já faz o cast"?

A variável introduzida pelo padrão possui o tipo correspondente quando a verificação é satisfeita. Não é necessário escrever um cast explícito separado.

Não se trata de armazenar informações em cache; a fala da transcrição sobre "Cache" deve ser entendida no contexto de cast/conversão de tipo.

### Conflito entre variáveis

O professor inicialmente utiliza o nome ex em mais de um padrão dentro do encadeamento if/else if. O compilador sinaliza um conflito.

Ele resolve utilizando nomes diferentes e descritivos:

```csharp
context.Exception is ErrorOnValidationException validationException
context.Exception is NotFoundException notFoundException
```

O escopo de variáveis declaradas em padrões pode alcançar partes do comando condicional, mesmo quando elas só estão definitivamente atribuídas em determinados caminhos.

Usar nomes distintos evita o conflito e facilita a leitura.

## 14. Atualização do filtro de exceções

O CashFlow já possuía um filtro responsável por interpretar as exceções da aplicação e produzir respostas HTTP apropriadas.

A aula adiciona uma nova condição ao tratamento das exceções conhecidas.

### Ramo de validação

A exceção ErrorOnValidationException permanece associada ao HTTP 400 Bad Request.

O filtro utiliza a propriedade Errors para construir ResponseErrorJson.

### Novo ramo de recurso não encontrado

A exceção NotFoundException passa a ser associada ao HTTP 404 Not Found.

A mensagem é obtida da propriedade Message herdada de Exception.

### Trecho reconstruído

```csharp
if (context.Exception is ErrorOnValidationException validationException)
{
    var errorResponse = new ResponseErrorJson(validationException.Errors);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status400BadRequest;

    context.Result = new BadRequestObjectResult(errorResponse);
}
else if (context.Exception is NotFoundException notFoundException)
{
    var errorResponse = new ResponseErrorJson(notFoundException.Message);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status404NotFound;

    context.Result = new NotFoundObjectResult(errorResponse);
}
```

Este trecho representa os dois ramos alterados. O código original contém outros componentes, inclusive o tratamento das demais exceções, que devem ser preservados. Os nomes locais e a assinatura dos construtores do DTO são reconstruídos com base no uso descrito pelo professor.

### Por que manter os ramos separados?

As duas exceções possuem semânticas diferentes e devem produzir resultados HTTP diferentes.

Validação incorreta não é necessariamente recurso inexistente.

## 15. NotFoundObjectResult e ResponseErrorJson

O professor altera tanto o código de status quanto o resultado MVC:

```csharp
StatusCodes.Status404NotFound
```

```csharp
new NotFoundObjectResult(errorResponse)
```

### O que NotFoundObjectResult faz?

É um resultado de action do ASP.NET Core que representa HTTP 404 com um objeto de resposta.

O objeto é processado pelo mecanismo de formatação/serialização configurado na API.

### Por que não usar BadRequestObjectResult?

BadRequestObjectResult representa HTTP 400 e, no projeto, é utilizado para falhas de validação.

O novo erro precisa de HTTP 404 porque um recurso específico não foi encontrado.

### E a lista de mensagens?

O professor utiliza a mensagem única de NotFoundException para criar ResponseErrorJson.

O resultado mantém o mesmo formato geral de lista de erros já utilizado pela API, mas contém somente uma mensagem.

### Exemplo ilustrativo de JSON

```json
{
  "errorMessages": [
    "Despesa não encontrada."
  ]
}
```

O exemplo demonstra a estrutura de lista com um elemento. O nome serializado da propriedade e a pontuação da mensagem não são confirmados literalmente na transcrição.

### Precisão técnica — complemento

NotFoundObjectResult já representa status 404. Em condições normais, o executor MVC aplica esse código de status ao executar o resultado. O professor também atribui explicitamente o status à Response, seguindo o padrão do filtro existente. A atribuição dupla não é necessária em todo desenho de filtro.

## 16. Código consolidado

### 16.1 Projeto Exceptions — CashFlowException

```csharp
public abstract class CashFlowException : SystemException
{
    protected CashFlowException(string message)
        : base(message)
    {
    }
}
```

### 16.2 Projeto Exceptions — NotFoundException

```csharp
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
```

### 16.3 Projeto Exceptions — ErrorOnValidationException

```csharp
public class ErrorOnValidationException : CashFlowException
{
    public List<string> Errors { get; }

    public ErrorOnValidationException(List<string> errors)
        : base(string.Empty)
    {
        Errors = errors;
    }
}
```

### 16.4 Application — GetExpenseByIdUseCase

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

O identificador da classe de recursos deve ser ajustado à grafia exata utilizada na solução.

### 16.5 Infrastructure — ExpensesRepository

O código da consulta permanece como na aula 111:

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### 16.6 API — ramos do filtro de exceções

```csharp
if (context.Exception is ErrorOnValidationException validationException)
{
    var errorResponse = new ResponseErrorJson(validationException.Errors);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status400BadRequest;

    context.Result = new BadRequestObjectResult(errorResponse);
}
else if (context.Exception is NotFoundException notFoundException)
{
    var errorResponse = new ResponseErrorJson(notFoundException.Message);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status404NotFound;

    context.Result = new NotFoundObjectResult(errorResponse);
}
```

Esses blocos não substituem o arquivo integral do filtro: o tratamento das demais exceções já existentes deve continuar funcionando.

### 16.7 API — documentação existente

```csharp
[ProducesResponseType(
    typeof(ResponseExpenseJson),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
```

Os atributos já haviam sido apresentados na aula 111. Agora o caminho de erro passa a ter implementação correspondente.

## 17. Fluxo completo entre camadas

### Caminho de sucesso

```text
GET por ID
    |
    v
GetExpenseByIdUseCase
    |
    v
ExpensesRepository.GetById(id)
    |
    v
Expense encontrada
    |
    v
AutoMapper
    |
    v
ResponseExpenseJson
    |
    v
HTTP 200 OK
```

### Caminho de erro

```text
GET por ID
    |
    v
GetExpenseByIdUseCase
    |
    v
ExpensesRepository.GetById(id)
    |
    v
null
    |
    v
if (result is null)
    |
    v
throw new NotFoundException(mensagem)
    |
    v
Filtro de exceções
    |
    v
Pattern matching: NotFoundException
    |
    v
ResponseErrorJson(Message)
    |
    v
NotFoundObjectResult
    |
    v
HTTP 404 Not Found
```

### Responsabilidades arquiteturais

| Camada/componente | Responsabilidade |
|---|---|
| Infrastructure | Executar a consulta e retornar entidade ou null |
| Application | Interpretar a ausência e lançar a exceção apropriada |
| Exceptions | Definir tipos de exceção e recursos de mensagem |
| API | Converter a exceção em resposta HTTP |
| Communication | Definir o contrato da resposta de erro |

A consulta permanece sem Unit of Work porque não existe escrita a efetivar.

## 18. Teste no Swagger

Depois de resolver as incompatibilidades de construtores, o professor executa a API e abre o Swagger.

Ele escolhe um identificador que não existe no banco e aciona a operação GET por ID.

### Resultado observado

- Status HTTP: **404 Not Found**.
- Corpo: uma lista de mensagens de erro.
- Quantidade de mensagens: **uma**.
- Conteúdo: informação de que a despesa não foi encontrada.

A transcrição representa o identificador como "Idset", provavelmente a fala "ID sete". Como a gravação escrita não permite confirmar esse valor com precisão, não se fixa um número de ID como fato.

### Conclusão do teste

A aula anterior havia validado o caminho de sucesso com a TV de ID 29.

Nesta aula, o professor valida o caminho de ausência do recurso.

Com os dois comportamentos, ele considera concluído o fluxo de recuperação de despesas por identificador.

Não foi apresentado um teste automatizado, benchmark ou novo teste de validação 400 nesta gravação.

## 19. Cuidados técnicos e alternativas

Os itens a seguir são complementos para compreender corretamente os mecanismos apresentados, não alterações adicionais realizadas durante a aula.

### 19.1 ID inexistente versus ID inválido

Um valor numérico válido para long pode não corresponder a uma despesa, como no teste do professor.

Isso é diferente de uma rota contendo texto incompatível com long. A rota, as restrições de rota e o model binding podem produzir outros comportamentos antes de o use case ser executado.

### 19.2 O repositório não precisa gerar HTTP

O repositório pode representar a ausência por null. O use case interpreta a ausência, e a API define o status HTTP.

Isso evita acoplar a Infrastructure a NotFoundObjectResult.

### 19.3 Null é uma possibilidade prevista

A assinatura Task<Expense?> continua adequada. A anotação não torna a Task anulável; torna anulável o resultado representado por Expense.

### 19.4 is null e is not null

Os padrões não alteram a referência. Eles testam uma condição. Depois de uma condição que lança exceção quando result é nulo, a análise de fluxo do compilador pode reconhecer que o caminho restante possui uma referência não nula.

### 19.5 Message não precisa ser recriada

A classe Exception já define a propriedade necessária. A mensagem é encaminhada pelos construtores da hierarquia.

### 19.6 Construtores da base precisam existir

Adicionar um construtor explícito à classe base pode afetar todas as classes derivadas. O compilador auxilia a localizar os pontos que precisam ser ajustados.

### 19.7 Exceções assíncronas

O use case possui retorno Task<ResponseExpenseJson>. Ao lançar NotFoundException dentro do método assíncrono, a falha é representada pela Task e propagada quando o controller aguarda a operação.

Isso conecta a aula aos conceitos estudados nas aulas 104 a 106.

### 19.8 Filtro de exceções não cobre toda falha possível

Filtros MVC tratam exceções nos pontos pertinentes do pipeline MVC. Eles não devem ser confundidos com um tratamento universal de qualquer erro em middlewares, roteamento ou serialização.

Nesta aula, o erro é lançado pelo use case aguardado pela action, dentro do fluxo que o filtro do curso trata.

### 19.9 Erros internos não devem ser expostos indiscriminadamente

O novo NotFoundException recebe uma mensagem de recurso preparada para o consumidor.

Isso não implica que toda Exception.Message, inclusive erros técnicos do banco, deva ser devolvida publicamente.

### 19.10 Alternativas de arquitetura

Em outros projetos, a ausência poderia ser representada por um resultado de operação explícito, como um tipo Result, em vez de uma exceção.

A aula adota exceções customizadas e filtro centralizado. O resumo mantém essa decisão, sem substituir a arquitetura do curso por outra.

## 20. Exercícios e revisão

### Exercício 1 — validar a ausência

Complete a condição depois de consultar o repositório.

```csharp
var result = await _repository.GetById(id);
```

**Resposta:**

```csharp
if (result is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);
```

### Exercício 2 — criar a exceção

```csharp
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
```

### Exercício 3 — corrigir a base

Por que base(message) não funciona se CashFlowException não possui um construtor compatível?

**Resposta:** o inicializador procura o construtor da classe base direta. É necessário acrescentar o construtor CashFlowException(string message), que encaminha o argumento para SystemException.

### Exercício 4 — corrigir a exceção de validação

```csharp
public ErrorOnValidationException(List<string> errors)
    : base(string.Empty)
{
    Errors = errors;
}
```

### Exercício 5 — pattern matching

Transforme a verificação de tipo e cast explícito em uma única condição.

**Resposta:**

```csharp
if (context.Exception is NotFoundException notFoundException)
{
    var message = notFoundException.Message;
}
```

### Exercício 6 — resposta HTTP

Qual resultado MVC representa 404 com um DTO de erro?

**Resposta:** new NotFoundObjectResult(errorResponse).

### Perguntas de revisão

1. O que faltava no fluxo implementado na aula 111?
2. Por que FirstOrDefaultAsync pode retornar null?
3. Qual é a diferença entre ausência de registro e falha de conexão?
4. Por que validar result antes de utilizar AutoMapper?
5. Qual sintaxe de verificação de null o professor prefere?
6. Qual é a diferença entre is null e == null quando existe sobrecarga de igualdade?
7. Em qual projeto NotFoundException é criada?
8. De qual classe NotFoundException herda?
9. Por que a mensagem deve ser recebida no construtor?
10. Qual chave de recurso é mencionada?
11. Onde fica armazenada a mensagem da exceção?
12. Por que Message não pode receber uma atribuição direta?
13. Qual classe é a base direta de NotFoundException?
14. O que base(message) faz?
15. Por que CashFlowException recebe um construtor protected?
16. Qual impacto a mudança tem em ErrorOnValidationException?
17. Por que o professor escolhe base(string.Empty) nessa classe?
18. Qual diferença existe entre Errors e Message?
19. Como pattern matching reduz a necessidade de casts explícitos?
20. Por que o nome ex repetido causou conflito no filtro?
21. Qual status permanece associado à exceção de validação?
22. Qual status é associado à nova exceção?
23. Qual resultado MVC é usado para 404 com corpo?
24. O filtro substitui o repositório na consulta ao banco?
25. Como a exceção de um método async chega ao controller?
26. Qual resultado o professor observou no Swagger?
27. Quantas mensagens havia no corpo da resposta de erro?
28. A documentação do Swagger já previa o 404 antes desta aula?

## 21. Resumo e mapa mental

| Componente | Alteração da aula |
|---|---|
| GetExpenseByIdUseCase | Verifica result is null e lança exceção |
| NotFoundException | Novo tipo customizado para ausência de recurso |
| Arquivos de recursos | Uso da mensagem ExpenseNotFound já criada |
| CashFlowException | Construtor protected que recebe e encaminha string |
| ErrorOnValidationException | Construtor passa string.Empty para a base |
| Exception filter | Pattern matching e ramo específico de NotFoundException |
| Resposta de erro | ResponseErrorJson com uma mensagem |
| HTTP | 404 Not Found via NotFoundObjectResult |
| Swagger | Teste de identificador inexistente confirmado |

```text
                       AULA 112
               TRATAMENTO DE RECURSO AUSENTE
                            |
              GetExpenseByIdUseCase
                            |
                 await GetById(id)
                            |
                   result is null?
                            |
               +------------+------------+
               |                         |
              NÃO                       SIM
               |                         |
           AutoMapper            NotFoundException
               |                         |
         ResponseExpenseJson     ExpenseNotFound
               |                         |
            HTTP 200             base(message)
                                         |
                                  CashFlowException
                                         |
                                   SystemException
                                         |
                                      Exception
                                         |
                                       Message
                                         |
                                 Exception Filter
                                         |
                               Pattern Matching de tipo
                                         |
                       +-----------------+----------------+
                       |                                  |
             ErrorOnValidationException           NotFoundException
                       |                                  |
                     Errors                             Message
                       |                                  |
                    HTTP 400                           HTTP 404
                                                          |
                                                   ResponseErrorJson
                                                          |
                                                   Swagger confirma
                                                  uma mensagem de erro
```

**Conclusão:** a aula 112 finaliza o fluxo de consulta de despesa por identificador. A ausência, representada por null pelo repositório, passa a ser interpretada na Application e comunicada por NotFoundException. A mensagem percorre a hierarquia de construtores até Exception.Message, sem duplicação de propriedades. O filtro identifica a nova exceção por pattern matching e produz HTTP 404 com o contrato de erro do CashFlow, mantendo o tratamento de validação HTTP 400 existente. O teste no Swagger confirma o resultado de recurso inexistente.
