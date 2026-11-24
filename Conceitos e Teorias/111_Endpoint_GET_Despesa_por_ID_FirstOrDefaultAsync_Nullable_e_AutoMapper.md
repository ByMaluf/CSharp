# Aula 111 — GET de despesa por ID: FirstOrDefaultAsync, nullable reference types e AutoMapper

> **Continuação das aulas 109 e 110:** depois de implementar a listagem de despesas e otimizar suas consultas com `AsNoTracking()`, o instrutor adiciona uma operação para recuperar os detalhes de uma única despesa pelo identificador.
>
> **Resultado da demonstração:** consulta do ID **29**, correspondente à despesa **TV**, com retorno de ID, título, descrição, data, valor e tipo de pagamento.
>
> **Escopo da aula:** a busca e o mapeamento são implementados e testados no caminho de sucesso. O tratamento explícito da ausência da despesa, com uma exceção específica e adaptação do filtro de exceções, é anunciado para a próxima aula. O HTTP 404 já está previsto no contrato documentado, mas ainda não é efetivamente demonstrado.
>
> **Fidelidade à transcrição:** apesar de o professor mencionar "receita" na abertura, o restante do código e da demonstração é sobre despesas (Expense). Os exemplos de código reproduzem a estrutura narrada; nomes de arquivos, namespaces e detalhes não transcritos integralmente são reconstruções didáticas. Os complementos técnicos são indicados como tais.

## Índice

1. Objetivo e relação com a listagem
2. Componentes previamente preparados
3. Endpoint GET com ID na rota
4. Contratos HTTP 200 e 404
5. DTO completo ResponseExpenseJson
6. Dependências do caso de uso
7. Registro do caso de uso na DI
8. Configuração do AutoMapper
9. Contrato GetById no repositório
10. Implementação com AsNoTracking e FirstOrDefaultAsync
11. Expressão lambda e execução da consulta
12. FirstAsync versus FirstOrDefaultAsync
13. Nullable reference types e Task<Expense?>
14. Implementação do use case e limite atual
15. Código consolidado
16. Teste no Swagger
17. Separação entre entidades e DTOs
18. Tratamento de ausência e evolução futura
19. Cuidados técnicos e erros comuns
20. Exercícios e perguntas de revisão
21. Resumo e mapa mental

---

## 1. Objetivo e relação com a listagem

Na aula 109, o endpoint GET de listagem foi implementado para devolver apenas os dados necessários à apresentação resumida das despesas: ID, título e valor.

Na aula 110, foi aplicado `AsNoTracking()` à consulta `GetAll()`.

Agora, a API passa a oferecer uma segunda forma de consultar despesas: **recuperar uma entidade específica e produzir uma resposta detalhada**, utilizando o identificador que a listagem já fornece.

| Operação | Entrada | Saída |
|---|---|---|
| GetAll | Nenhum ID | Lista de despesas resumidas |
| GetById | ID de uma despesa | Uma despesa detalhada |

### Cenário de utilização

```text
Dashboard / listagem
       |
       | usuário seleciona um item
       v
Id da despesa
       |
       v
GET de detalhes por ID
       |
       v
ResponseExpenseJson
```

Essa separação permite que a tela de listagem permaneça mais compacta, enquanto a tela de detalhes solicita informações adicionais apenas quando necessárias.

O projeto continua utilizando a arquitetura já construída: API, Application, Domain, Communication e Infrastructure.

---

## 2. Componentes previamente preparados

O professor explica que uma parte do código já foi escrita antes da gravação.

Ele apresenta:

- uma nova action GET no controller;
- a configuração do ID como parâmetro da rota;
- a documentação das respostas 200 e 404;
- o DTO de resposta detalhada;
- a interface e a classe do caso de uso;
- o construtor do caso de uso com repositório e AutoMapper.

Durante a aula, registra o use case na DI, acrescenta o mapa necessário ao Profile, cria o contrato do repositório, implementa a consulta, ajusta a nulabilidade e testa o caminho de sucesso.

Não é necessário reconstruir componentes anteriores, como `CashFlowDbContext`, a extensão de DI da Infrastructure e o registro inicial do AutoMapper.

---

## 3. Endpoint GET com ID na rota

A nova action utiliza o método HTTP GET e recebe o identificador da despesa pela rota.

Uma representação conceitual é:

```http
GET /expenses/29
```

**A rota completa acima é ilustrativa:** a gravação confirma que o ID é recebido pela rota, mas não transcreve integralmente o prefixo do controller.

### Estrutura do atributo

```csharp
[HttpGet("{id}")]
```

O marcador `{id}` representa um segmento variável da URL.

Uma action poderia receber esse valor por meio de:

```csharp
[FromRoute] long id
```

### Por que long?

O professor verifica expressamente o tipo do identificador no código e confirma que ele é `long`, não `int`.

Em C#, `long` representa `System.Int64`, um inteiro de 64 bits.

É importante preservar o tipo adotado na entidade e na interface do repositório. Utilizar um tipo menor poderia restringir indevidamente os identificadores aceitos.

### Injeção do caso de uso

A action recebe o serviço por `[FromServices]`, conforme o padrão estudado nas aulas anteriores:

```csharp
[FromServices] IGetExpenseByIdUseCase useCase
```

O controller executa o caso de uso e retorna a resposta:

```csharp
var response = await useCase.Execute(id);
return Ok(response);
```

---

## 4. Contratos HTTP 200 e 404

O professor apresenta duas respostas previstas para o novo endpoint.

| Status | Situação | Corpo documentado |
|---|---|---|
| 200 OK | Despesa encontrada | ResponseExpenseJson |
| 404 Not Found | Não existe despesa com o identificador solicitado | ResponseErrorJson |

### 200 OK

A consulta é bem-sucedida e devolve os detalhes da despesa encontrada.

### 404 Not Found

O recurso específico não existe. O professor exemplifica com o ID **1000**, inexistente na base de teste apresentada naquele momento.

Ele informa que as despesas existentes possuem IDs de **27 até 32**.

### Documentação com ProducesResponseType

```csharp
[ProducesResponseType(
    typeof(ResponseExpenseJson),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
```

Assim como na aula 108, esses atributos descrevem os resultados possíveis para a documentação OpenAPI.

**Atributos de documentação não implementam o tratamento de erro.** A API precisa produzir realmente a resposta correspondente.

Nesta gravação, a resposta 404 está documentada, mas a exceção específica e o tratamento pelo filtro ainda serão desenvolvidos.

### Comparação com o GetAll

Na listagem, o curso escolhe 204 No Content quando não existem itens.

Na consulta individual, a ausência do recurso identificado representa um cenário de 404 Not Found.

A diferença está no contrato da operação: uma coleção pode existir sem itens, enquanto a consulta de um recurso específico pode não encontrar o recurso solicitado.

---

## 5. DTO completo ResponseExpenseJson

O professor chama atenção para o nome no singular: `ResponseExpenseJson`.

Ele é diferente de `ResponseExpensesJson`, o envelope de listagem, e de `ResponseShortExpenseJson`, a resposta resumida de cada item.

### Campos mencionados

| Campo | Finalidade |
|---|---|
| Id | Identificador |
| Title | Título |
| Description | Descrição |
| Date | Data |
| Amount | Valor |
| PaymentType | Tipo de pagamento |

### Exemplo estrutural

```csharp
public class ResponseExpenseJson
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
}
```

Os seis campos são confirmados pela transcrição. A nulabilidade de Description, os detalhes de inicialização e os tipos exatos de Date, Amount e PaymentType no DTO acima são exemplos didáticos coerentes com o projeto; não foram soletrados integralmente na gravação.

### Comparação

| DTO | Finalidade | Campos |
|---|---|---|
| ResponseShortExpenseJson | Item de listagem | Id, Title, Amount |
| ResponseExpensesJson | Envelope de listagem | Expenses: coleção |
| ResponseExpenseJson | Detalhes de uma despesa | Id, Title, Description, Date, Amount, PaymentType |

---

## 6. Dependências do caso de uso

O instrutor apresenta o caso de uso preparado, chamado `GetExpenseByIdUseCase`.

Ele recebe:

- `IExpensesRepository`, para recuperar a entidade;
- `IMapper`, para transformá-la na resposta detalhada.

```csharp
private readonly IExpensesRepository _repository;
private readonly IMapper _mapper;

public GetExpenseByIdUseCase(
    IExpensesRepository repository,
    IMapper mapper)
{
    _repository = repository;
    _mapper = mapper;
}
```

### Por que não IUnitOfWork?

Como na listagem da aula 109, a operação é de leitura. Não existe uma alteração a confirmar por meio de Commit.

O repositório utiliza o DbContext para consultar o banco, mas o caso de uso não precisa solicitar SaveChangesAsync.

---

## 7. Registro do caso de uso na DI

O professor abre `DependencyInjectionExtension`, no projeto Application, e acrescenta o registro do novo caso de uso.

```csharp
services.AddScoped<IGetExpenseByIdUseCase, GetExpenseByIdUseCase>();
```

Esse registro é colocado junto aos demais casos de uso, dentro de AddUseCases.

Exemplo de organização:

```csharp
private static void AddUseCases(IServiceCollection services)
{
    services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
    services.AddScoped<IGetAllExpensesUseCase, GetAllExpensesUseCase>();
    services.AddScoped<IGetExpenseByIdUseCase, GetExpenseByIdUseCase>();
}
```

### Por que registrar?

O controller recebe uma interface. Sem uma configuração de DI apropriada, o container não saberia qual implementação fornecer.

A API continua utilizando o ponto de composição `builder.Services.AddApplication()` já criado anteriormente.

---

## 8. Configuração do AutoMapper

O Profile `AutoMapping` já possui configurações para cadastro e listagem.

O professor acrescenta a conversão específica da entidade para o DTO detalhado:

```csharp
CreateMap<Expense, ResponseExpenseJson>();
```

### Organização dos mapas

```csharp
private void EntityToResponse()
{
    CreateMap<Expense, ResponseRegisterExpenseJson>();
    CreateMap<Expense, ResponseShortExpenseJson>();
    CreateMap<Expense, ResponseExpenseJson>();
}
```

### Por que mais um mapa?

Embora a origem seja Expense em todos os casos, os destinos possuem contratos distintos.

O AutoMapper precisa conhecer a conversão solicitada para ResponseExpenseJson.

A configuração não consulta o banco. Ela apenas define como os dados do objeto serão transformados.

O caso de uso utilizará essa configuração por meio de:

```csharp
_mapper.Map<ResponseExpenseJson>(result)
```

---

## 9. Contrato GetById no repositório

O professor acrescenta um método à interface `IExpensesRepository`.

Inicialmente, o retorno é concebido como uma tarefa que fornece uma entidade. Durante a aula, percebe-se que a entidade pode não existir e ajusta-se a assinatura.

### Contrato final reconstruído

```csharp
Task<Expense?> GetById(long id);
```

### Significado

- `Task`: a operação pode ser aguardada assincronamente.
- `Expense?`: o resultado pode ser uma entidade ou null.
- `GetById`: operação de recuperação individual.
- `long id`: identificador recebido pelo método.

### Interface acumulada

```csharp
public interface IExpensesRepository
{
    Task Add(Expense expense);
    Task<List<Expense>> GetAll();
    Task<Expense?> GetById(long id);
}
```

A nova operação não substitui Add ou GetAll. Ela acrescenta ao contrato a capacidade de buscar uma despesa individual.

---

## 10. Implementação com AsNoTracking e FirstOrDefaultAsync

Na Infrastructure, o professor abre `ExpensesRepository`.

Como a interface ganhou uma operação, a implementação concreta também precisa implementar esse método.

### Código final

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### O papel de cada elemento

| Elemento | Função |
|---|---|
| _dbContext.Expenses | Conjunto de entidades consultável |
| AsNoTracking() | Desabilita rastreamento dos resultados desta consulta |
| FirstOrDefaultAsync(...) | Recupera a primeira correspondência ou o valor padrão |
| expense => expense.Id == id | Condição de seleção |
| await | Aguarda a conclusão assíncrona |
| Task<Expense?> | Comunica a possibilidade de ausência |

O namespace `Microsoft.EntityFrameworkCore` fornece as extensões usadas na consulta.

### Por que usar AsNoTracking?

O professor aplica diretamente o aprendizado da aula 110.

O caso de uso de detalhes não pretende alterar a entidade recuperada. Por isso, o rastreamento não é necessário para a estratégia de persistência atual.

A futura atualização de despesas precisará considerar uma forma de obter ou associar entidades rastreadas. O instrutor anuncia uma solução envolvendo SOLID, mas não a implementa nesta aula.

---

## 11. Expressão lambda e execução da consulta

O argumento de FirstOrDefaultAsync é:

```csharp
expense => expense.Id == id
```

### Leitura

Para cada despesa considerada pela consulta, o predicado verifica se o identificador da entidade corresponde ao identificador recebido como argumento do método.

```csharp
expense.Id == id
```

A parte esquerda representa o identificador da entidade.

A parte direita representa o parâmetro da operação GetById.

### Exemplo conceitual

Se o parâmetro recebido for 29, a condição busca a entidade cujo ID é 29.

### SQL ilustrativo

```sql
SELECT ...
FROM Expenses
WHERE Id = @id
LIMIT 1;
```

O SQL é uma representação didática da consulta relacional, não o texto SQL capturado na aula. O provider de MySQL e a configuração do modelo determinam os detalhes efetivamente gerados.

### Execução

FirstOrDefaultAsync é um operador terminal: solicita ao EF Core a execução da consulta e materializa o resultado individual.

Diferentemente de ToListAsync, não devolve uma coleção completa.

---

## 12. FirstAsync versus FirstOrDefaultAsync

O professor mostra o aviso de nulabilidade gerado pela chamada FirstOrDefaultAsync e compara com FirstAsync.

### FirstAsync

```csharp
await _dbContext.Expenses
    .FirstAsync(e => e.Id == id);
```

Se houver correspondência, devolve o primeiro resultado.

Se não houver, lança uma exceção.

### FirstOrDefaultAsync

```csharp
await _dbContext.Expenses
    .FirstOrDefaultAsync(e => e.Id == id);
```

Se houver correspondência, devolve o primeiro resultado.

Se não houver, devolve o valor padrão do tipo.

Como Expense é uma classe, seu valor padrão é null.

### Comparação

| Método | Encontrou | Não encontrou |
|---|---|---|
| FirstAsync | Entidade | InvalidOperationException |
| FirstOrDefaultAsync | Entidade | null, neste caso |

### Por que o professor escolhe FirstOrDefaultAsync?

A inexistência de uma despesa é uma possibilidade legítima da consulta.

O professor não quer que a Infrastructure lance uma exceção apenas porque não encontrou uma correspondência. Prefere devolver null para que o caso de uso decida como tratar a ausência.

### Precisão técnica

FirstOrDefaultAsync não elimina todas as exceções. A consulta ainda pode falhar por problemas de conexão, tradução, cancelamento ou outras falhas de execução.

O método evita especificamente a exceção decorrente de uma sequência sem elemento correspondente.

FirstAsync também não garante que o registro exista: expressa a expectativa de que exista e falha se essa expectativa não for atendida.

---

## 13. Nullable reference types e Task<Expense?>

O professor identifica um sublinhado verde no Visual Studio ao retornar FirstOrDefaultAsync.

A causa é a possibilidade de retorno nulo em um método declarado com tipo de referência não anulável.

### Antes da correção

```csharp
Task<Expense> GetById(long id);
```

Essa assinatura comunica que o resultado esperado é uma Expense não nula, considerando nullable reference types habilitados.

### Depois

```csharp
Task<Expense?> GetById(long id);
```

A anotação `?` indica que o resultado da tarefa pode ser null.

### Ajuste na implementação

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

A interface e a implementação passam a expressar a mesma possibilidade de ausência.

### O que significa o ponto de interrogação?

No contexto dos tipos de referência anuláveis, ele informa ao compilador e aos consumidores do contrato que aquela referência pode ser nula.

Ele não cria automaticamente uma entidade, não modifica o banco e não impede que null seja produzido.

### Atenção à posição do ?

| Tipo | Significado |
|---|---|
| Task<Expense> | Task cujo resultado é declarado não anulável |
| Task<Expense?> | Task cujo resultado pode ser null |
| Task<Expense>? | A própria referência à Task pode ser null |

A assinatura adequada para o cenário narrado é a segunda.

### Diferença entre aviso e tratamento

O compilador pode ajudar a identificar acessos potencialmente nulos, mas declarar Expense? não implementa a regra de negócio de erro 404.

O consumidor ainda precisa considerar o resultado ausente.

---

## 14. Implementação do use case e limite atual

O professor chama o repositório e utiliza o resultado para mapear o DTO de resposta.

### Trecho reconstruído da etapa atual

```csharp
public async Task<ResponseExpenseJson> Execute(long id)
{
    var result = await _repository.GetById(id);

    return _mapper.Map<ResponseExpenseJson>(result);
}
```

### Fluxo implementado

1. Recebe o ID da action.
2. Aguarda a consulta do repositório.
3. Solicita a transformação para ResponseExpenseJson.
4. Retorna o resultado do mapeamento.

### O que está faltando?

O professor encerra a aula reconhecendo que ainda precisa verificar se result é null.

Na etapa atual, o código não deve ser considerado uma implementação completa do contrato de ausência.

Não se pode afirmar que o AutoMapper irá lançar automaticamente uma exceção de domínio ou produzir HTTP 404. Dependendo da configuração, uma origem nula pode produzir uma resposta nula ou outro comportamento, mas isso não equivale ao tratamento explícito requerido.

---

## 15. Código consolidado

Os trechos abaixo apresentam a estrutura reconstruída da aula, com o caminho de sucesso implementado e a lacuna de tratamento de null preservada.

### Domain — IExpensesRepository

```csharp
public interface IExpensesRepository
{
    Task Add(Expense expense);
    Task<List<Expense>> GetAll();
    Task<Expense?> GetById(long id);
}
```

### Infrastructure — ExpensesRepository

```csharp
using Microsoft.EntityFrameworkCore;

// Dentro de ExpensesRepository:
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### Application — interface do use case

```csharp
public interface IGetExpenseByIdUseCase
{
    Task<ResponseExpenseJson> Execute(long id);
}
```

### Application — classe do use case

```csharp
public class GetExpenseByIdUseCase : IGetExpenseByIdUseCase
{
    private readonly IExpensesRepository _repository;
    private readonly IMapper _mapper;

    public GetExpenseByIdUseCase(
        IExpensesRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseExpenseJson> Execute(long id)
    {
        var result = await _repository.GetById(id);

        // O tratamento de result == null ainda falta nesta aula.
        return _mapper.Map<ResponseExpenseJson>(result);
    }
}
```

### Application — AutoMapping

```csharp
CreateMap<Expense, ResponseExpenseJson>();
```

### Application — DependencyInjectionExtension

```csharp
services.AddScoped<IGetExpenseByIdUseCase, GetExpenseByIdUseCase>();
```

### API — action GET

```csharp
[HttpGet("{id}")]
[ProducesResponseType(
    typeof(ResponseExpenseJson),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetExpenseById(
    [FromServices] IGetExpenseByIdUseCase useCase,
    [FromRoute] long id)
{
    var response = await useCase.Execute(id);
    return Ok(response);
}
```

A rota relativa, a ordem dos argumentos e a assinatura literal da action são reconstruções ilustrativas da descrição do professor. O exemplo não deve ser interpretado como implementação funcional do 404.

---

## 16. Teste no Swagger

O professor inicia a API, abre o Swagger e seleciona o novo endpoint GET.

### Dados observados

| Informação | Valor |
|---|---|
| ID consultado | 29 |
| Título | TV |
| Faixa de IDs existentes mencionada | 27 a 32 |
| Exemplo de ID inexistente | 1000 |
| Resposta de sucesso | 200 OK |

O professor usa breakpoints e F10/F11 para observar a execução do repositório e a transformação pelo AutoMapper.

### Campos retornados

A resposta apresenta ID 29, título TV, descrição, data, valor e tipo de pagamento.

A transcrição atual não fornece os valores literais da descrição, da data e do tipo de pagamento. Portanto, não se deve inventá-los.

O valor da TV foi mencionado como 1200 na aula 102; isso é contexto de uma aula anterior, não uma nova leitura numérica confirmada nesta gravação.

### O que foi efetivamente comprovado?

- A rota recebe um ID.
- O repositório encontra a despesa correspondente.
- O mapeamento para o DTO detalhado funciona.
- O Swagger exibe a resposta com os detalhes da TV.

O exemplo de ID 1000 é utilizado para explicar a necessidade do tratamento futuro. O vídeo não demonstra uma requisição com ID 1000 retornando 404.

---

## 17. Separação entre entidades e DTOs

O professor observa que, neste caso, o DTO detalhado contém os mesmos dados que a entidade Expense.

Isso poderia levar à dúvida: por que criar outro tipo se as propriedades são iguais?

### A justificativa

A igualdade atual é uma coincidência do modelo. O contrato externo e a entidade interna têm responsabilidades diferentes.

- Entidade: representa os dados e o modelo utilizado pela aplicação.
- DTO: representa as informações que devem ser entregues ao consumidor da API.

### Exemplo mencionado pelo professor

Uma entidade de usuário pode conter informações relacionadas à senha. Uma resposta de perfil não deve devolver esses dados.

### Complemento de segurança

Em um sistema real, senhas não devem ser armazenadas em texto puro. Mesmo hashes de senha e outros dados de autenticação não devem fazer parte do DTO público.

O DTO funciona como um contrato explícito de dados permitidos para aquela resposta. Ele não elimina, por si só, a necessidade de revisar a serialização e os mapeamentos.

### Benefício arquitetural

A entidade pode evoluir sem que toda nova propriedade seja automaticamente exposta aos clientes.

O DTO também pode mudar para atender a uma tela ou integração específica sem exigir que o modelo interno do domínio tenha a mesma estrutura.

---

## 18. Tratamento de ausência e evolução futura

Ao final, o professor anuncia a próxima etapa:

1. Verificar se a consulta retornou null.
2. Criar uma exceção específica de recurso não encontrado.
3. Lançar a exceção no caso de uso.
4. Adaptar o filtro de exceções.
5. Produzir uma resposta HTTP 404 com ResponseErrorJson.

### Fluxo pretendido

```text
GetById(id)
    |
    v
Resultado encontrado?
    |
    +-- Sim --> AutoMapper --> HTTP 200
    |
    +-- Não --> Exceção específica
                     |
                     v
              Filtro de exceções
                     |
                     v
              HTTP 404 + erro
```

**Esse fluxo completo é planejado, não concluído nesta aula.**

### Por que o repositório retorna null?

A consulta apenas informa que não encontrou dados.

O caso de uso, por sua vez, interpreta o resultado segundo a finalidade da operação. Neste endpoint, uma despesa inexistente deve ser tratada como recurso não encontrado.

O professor ainda não cria a classe de exceção nem informa todos os detalhes de sua implementação. Não devemos antecipar um nome definitivo de classe, mensagem de erro ou código do filtro como se já tivessem sido definidos.

---

## 19. Cuidados técnicos e erros comuns

Os pontos abaixo aprofundam os conceitos da aula.

### 19.1 Usar FirstAsync quando a ausência é esperada

Isso pode gerar InvalidOperationException em uma busca sem correspondência. FirstOrDefaultAsync permite expressar a ausência por null.

### 19.2 Declarar Expense não anulável e ignorar o aviso

O contrato deve comunicar a possibilidade de ausência. Task<Expense?> é apropriado neste caso.

### 19.3 Confundir ? com tratamento de exceções

A anotação de nulabilidade não transforma automaticamente o resultado em HTTP 404.

### 19.4 Mapear antes de verificar a ausência

Quando a regra exige que o recurso exista, é importante tratar null antes de continuar. Essa melhoria está reservada para a próxima aula.

### 19.5 Esquecer o registro de DI

O container precisa conhecer a implementação do novo caso de uso.

### 19.6 Esquecer o mapa específico

É necessário configurar Expense para ResponseExpenseJson, mesmo que já existam mapas para outros DTOs de despesa.

### 19.7 Usar AsNoTracking como se impedisse alterações em memória

O objeto pode ser modificado em memória. A consequência é que o contexto não acompanha automaticamente as mudanças de uma instância desconectada.

Estratégias de associação ou atualização explícita são possíveis, mas não fazem parte da implementação desta aula.

### 19.8 Confundir documentação de 404 com comportamento de 404

ProducesResponseType fornece metadados; não captura exceções nem altera automaticamente o resultado da action.

### 19.9 Esperar que o repositório retorne um DTO HTTP

No desenho apresentado, o repositório devolve Expense ou null. O caso de uso mapeia o DTO e a API produz o resultado HTTP.

### 19.10 FindAsync e SingleOrDefaultAsync

Como complemento, o EF Core também oferece outras operações de recuperação, com semânticas diferentes. FindAsync busca por chave e pode reutilizar uma entidade já rastreada; SingleOrDefaultAsync exige no máximo um resultado.

A consulta escolhida no curso é FirstOrDefaultAsync com AsNoTracking. Não há substituição por esses métodos na gravação.

### 19.11 Não inventar uma arquitetura multiusuário

As aulas anteriores descrevem um cenário sem associação de despesas a usuários. Em um sistema multiusuário, seria necessário aplicar as regras de autorização e propriedade do recurso. Essa funcionalidade não é criada nesta aula.

---

## 20. Exercícios e perguntas de revisão

### Exercício 1 — contrato da consulta

Qual assinatura representa uma busca assíncrona de despesa por identificador que pode não encontrar um registro?

**Resposta:**

```csharp
Task<Expense?> GetById(long id);
```

### Exercício 2 — consulta

Implemente a consulta usando os métodos apresentados.

**Resposta:**

```csharp
public async Task<Expense?> GetById(long id)
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(expense => expense.Id == id);
}
```

### Exercício 3 — identificar a diferença

O que FirstAsync faz quando não encontra a despesa? Lança uma exceção de sequência sem correspondência.

O que FirstOrDefaultAsync faz? Retorna null para Expense, salvo outras falhas da consulta.

### Exercício 4 — configuração de mapeamento

```csharp
CreateMap<Expense, ResponseExpenseJson>();
```

A origem é Expense e o destino é ResponseExpenseJson.

### Exercício 5 — DI

```csharp
services.AddScoped<IGetExpenseByIdUseCase, GetExpenseByIdUseCase>();
```

Esse registro permite que o container resolva o novo use case.

### Exercício 6 — status HTTP

Qual status corresponde à despesa encontrada? 200 OK.

Qual status está previsto para a despesa inexistente? 404 Not Found.

O segundo caminho já está implementado e testado? Não. A próxima aula tratará explicitamente essa ausência.

### Perguntas adicionais

1. Qual é a diferença entre GetAll e GetById?
2. Por que o ID aparece na listagem resumida?
3. Qual tipo de ID o professor confirma no código?
4. Quais são os seis campos da resposta detalhada?
5. Por que o use case recebe IMapper?
6. Em que classe a configuração do AutoMapper é adicionada?
7. Por que o novo use case deve ser registrado na DI?
8. Por que a consulta utiliza AsNoTracking?
9. Qual é o papel do predicado expense => expense.Id == id?
10. Por que FirstOrDefaultAsync é preferido no exemplo?
11. Qual é o valor padrão de uma referência a uma classe?
12. Qual é a diferença entre Task<Expense?> e Task<Expense>?
13. O ponto de interrogação torna a própria Task anulável?
14. Por que não é necessário Commit nesta consulta?
15. Qual ID foi testado no Swagger?
16. Qual era o título da despesa recuperada?
17. Por que utilizar um DTO detalhado mesmo quando suas propriedades coincidem com as da entidade?
18. O que ainda precisa ser implementado para devolver 404 de maneira explícita?
19. Qual princípio de separação aparece entre Repository, Application e API?
20. O professor já implementa o fluxo de atualização de despesas nesta gravação?

---

## 21. Resumo e mapa mental

| Componente | Alteração |
|---|---|
| API | Nova action GET com ID na rota e documentação 200/404 |
| Communication | ResponseExpenseJson detalhado |
| Domain | Task<Expense?> GetById(long id) |
| Infrastructure | AsNoTracking + FirstOrDefaultAsync |
| Application | Novo caso de uso com repositório e IMapper |
| DI | Registro Scoped do novo use case |
| Profile | CreateMap<Expense, ResponseExpenseJson>() |
| Teste | ID 29, TV |
| Pendência | Tratamento explícito de null e HTTP 404 |

```text
                       AULA 111 — GET BY ID
                                 |
                       GET com ID na rota
                                 |
                        GetExpenseByIdUseCase
                                 |
                       await repository.GetById(id)
                                 |
                        ExpensesRepository
                                 |
                          AsNoTracking()
                                 |
                   FirstOrDefaultAsync(e => e.Id == id)
                                 |
                           Task<Expense?>
                                 |
                   +-------------+-------------+
                   |                           |
               Encontrou                   Não encontrou
                   |                           |
                   v                           v
            Expense (ID 29)                   null
                   |                           |
                   v                           v
                 IMapper                tratamento ainda
                   |                     não implementado
                   v                           |
           ResponseExpenseJson                 v
                   |                    próxima aula:
                   v                    exceção + filtro
                HTTP 200                       |
                                               v
                                         HTTP 404 previsto
```

**Conclusão:** a aula 111 implementa a consulta individual assíncrona de despesas, reutilizando a arquitetura e as configurações anteriores. A Infrastructure utiliza FirstOrDefaultAsync e AsNoTracking; o contrato retorna Task<Expense?> para representar corretamente a ausência possível; a Application mapeia a entidade para um DTO detalhado. O caminho de sucesso é confirmado com a despesa TV, ID 29. A resposta 404 já está documentada, mas seu tratamento explícito será desenvolvido na próxima aula.
