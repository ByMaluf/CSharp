# Aula 109 — Endpoint GET de despesas: ToListAsync, AutoMapper de coleções e respostas HTTP 200/204

> **Tema:** implementar no CashFlow uma consulta assíncrona que recupere todas as despesas cadastradas, devolva apenas os dados necessários à listagem e apresente um contrato HTTP com `200 OK` quando houver resultados e `204 No Content` quando não houver.
>
> **Sequência:** continuação da aula 108, que colocou o AutoMapper em funcionamento no cadastro de despesas e documentou o endpoint de criação no Swagger.
>
> **Resultado demonstrado:** o professor utiliza o Swagger para consultar o banco, observa **seis despesas** retornadas pelo repositório, confirma a transformação da lista pelo AutoMapper e recebe **HTTP 200 OK** com um objeto contendo uma lista de despesas resumidas.
>
> **Fidelidade ao material:** a gravação começa com o endpoint, os DTOs e o esqueleto do use case já preparados pelo professor. Esta anotação distingue **código previamente criado** de **código implementado durante a aula**. Trechos integrais e namespaces são reconstruções didáticas quando o texto não fornece a sintaxe exata exibida na tela.
>
> **Terminologia:** a abertura menciona "receitas", mas todo o restante da demonstração trata do endpoint de **despesas** (`Expenses`), com `ExpensesRepository`, `GetAll` e respostas específicas de despesas. Portanto, o objeto estudado nesta aula é a **listagem de despesas**.

## Índice

1. Objetivo e contexto
2. O que já estava preparado antes da gravação
3. Contrato HTTP: 200 OK e 204 No Content
4. DTO resumido de uma despesa
5. DTO que envolve a lista de despesas
6. Por que devolver somente os campos necessários?
7. Por que utilizar um objeto com lista em vez de um array na raiz?
8. Fluxo do controller e do use case
9. Registro do novo use case na injeção de dependência
10. Por que esse use case não recebe IUnitOfWork?
11. Injetando IExpensesRepository no caso de uso
12. Adicionando GetAll à interface do repositório
13. Implementando GetAll com ToListAsync no EF Core
14. O exemplo hipotético de filtragem por usuário
15. O que ToListAsync faz no banco de dados?
16. Configurando o AutoMapper para a resposta resumida
17. Mapeamento de listas com IMapper
18. Construindo ResponseExpensesJson no use case
19. Código consolidado por camada
20. Teste no Swagger e os seis resultados
21. Melhorias e ressalvas arquiteturais
22. Exercícios e perguntas de revisão
23. Resumo e mapa mental

---

## 1. Objetivo e contexto

O instrutor explica que, nesta etapa do projeto, vai acelerar a apresentação dos trechos já trabalhados em aulas anteriores: em vez de reconstruí-los do zero na gravação, mostra a estrutura pronta e implementa os conceitos novos junto com o aluno.

O objetivo da aula é adicionar ao CashFlow a capacidade de **consultar todas as despesas presentes no banco de dados**.

### Operações já existentes no projeto

Até aqui, o CashFlow já possui:

- endpoint de cadastro de despesas;
- `IExpensesRepository` e `ExpensesRepository`;
- `CashFlowDbContext` configurado com Entity Framework Core e MySQL;
- `IUnitOfWork` e `UnitOfWork` para persistência;
- operações assíncronas com `Task` e `await`;
- `AutoMapping : Profile` com mapeamentos de cadastro;
- `IMapper` registrado por injeção de dependência;
- atributos `ProducesResponseType` usados para documentar endpoints.

### Qual capacidade está faltando?

Recuperar as despesas já persistidas, montar uma lista resumida e devolver um resultado HTTP apropriado.

```text
Cliente / Swagger
       |
       v
GET de despesas
       |
       v
GetAllExpensesUseCase
       |
       v
IExpensesRepository.GetAll()
       |
       v
EF Core / MySQL
       |
       v
List<Expense>
       |
       v
AutoMapper
       |
       v
ResponseExpensesJson
       |
       v
HTTP 200 ou 204
```

---

## 2. O que já estava preparado antes da gravação

O professor enfatiza que não mostrará novamente, passo a passo, tudo o que já foi aprendido.

No início da demonstração, alguns componentes já estão presentes.

### O endpoint GET

O instrutor mostra no controller uma action de consulta, mencionada como "GetAllDespesas" ou `GetAllExpenses`.

Ele já preparou a documentação de duas possíveis respostas:

- `200 OK` quando houver despesas;
- `204 No Content` quando a listagem estiver vazia.

### Os DTOs

Ele também mostra os objetos de comunicação que já havia criado:

- um DTO de **despesa resumida**, com ID, título e valor;
- um DTO de **resposta da listagem**, contendo uma lista de despesas resumidas.

### O use case

O instrutor havia preparado a interface e a classe do caso de uso, mas deixado a regra de negócio vazia.

### O que será efetivamente implementado na gravação?

- Registro do novo caso de uso na DI.
- Injeção de `IExpensesRepository` no use case.
- Criação de `GetAll()` na interface do repositório.
- Implementação de `GetAll()` com `ToListAsync()`.
- Configuração do AutoMapper para a resposta resumida.
- Injeção de `IMapper` no use case.
- Composição da resposta contendo uma lista.
- Teste pelo Swagger.

**Essa distinção evita atribuir à aula uma implementação do zero dos controllers e DTOs, algo que o professor explicitamente diz não fazer.**

---

## 3. Contrato HTTP: 200 OK e 204 No Content

A action GET possui duas respostas previstas.

### 200 OK

O professor utiliza `200 OK` quando há **pelo menos uma despesa** cadastrada.

Nesse caso, a resposta contém um objeto JSON com uma propriedade que representa a lista.

Exemplo ilustrativo:

```json
{
  "expenses": [
    {
      "id": 1,
      "title": "Mouse",
      "amount": 55
    },
    {
      "id": 2,
      "title": "Comida",
      "amount": 15
    }
  ]
}
```

**Os IDs e a estrutura literal da serialização acima são ilustrativos.** O vídeo demonstra uma lista com seis despesas, mas não transcreve o JSON completo de cada registro.

### 204 No Content

Se não houver nenhuma despesa no banco, o endpoint retorna:

```text
HTTP/1.1 204 No Content
```

Isso significa que a requisição foi processada com sucesso, mas a resposta **não contém corpo**.

### Como o controller decide?

A lógica descrita pelo professor é:

```csharp
var response = await useCase.Execute();

if (response.Expenses.Any())
    return Ok(response);

return NoContent();
```

O código é uma reconstrução didática: a transcrição informa a verificação da existência de despesas, seguida de `Ok(response)` ou `NoContent()`, mas não especifica integralmente a condição escrita no controller.

### Por que 204 e não 404?

O professor escolhe `204` para indicar que o endpoint existe, a consulta foi realizada e não há itens para devolver.

Não se trata de dizer que o recurso/rota não existe.

### Alternativa legítima

Em muitas APIs, listas vazias são retornadas como `200 OK` com `[]` ou um objeto contendo um array vazio. **Não existe obrigação universal de usar 204 para uma coleção vazia.**

Nesta aula, o contrato escolhido é **200 para lista não vazia e 204 para lista vazia**. O resumo preserva a decisão do curso.

---

## 4. DTO resumido de uma despesa

O professor explica que endpoints de listagem geralmente são usados para apresentar dados resumidos, por exemplo, em um dashboard.

Por isso, a resposta de cada item não precisa ter todas as propriedades da entidade.

### Propriedades citadas

- **ID** — identifica a despesa.
- **Title** — representa o título.
- **Amount** — representa o valor monetário.

O instrutor chama essa representação de **resposta curta/resumida**, ou `ResponseShortExpenseJson`.

### Código ilustrativo

```csharp
public class ResponseShortExpenseJson
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
```

Os tipos `long`, `string` e `decimal` refletem o desenho usual do projeto; a transcrição descreve os campos, mas não apresenta suas declarações literais completas.

### Por que incluir Id?

O professor descreve um cenário em que o usuário vê uma listagem e, ao clicar em determinado item, solicita os **detalhes completos** daquela despesa.

Assim, o ID funciona como identificador para uma futura operação de consulta por ID.

```text
Dashboard com despesas resumidas
               |
               | seleciona uma despesa
               v
           Expense.Id
               |
               v
   GET de detalhes por identificador
               |
               v
    Informações completas da despesa
```

**A consulta detalhada por ID é mencionada como parte do desenho pretendido; ela não é implementada nesta gravação.**

---

## 5. DTO que envolve a lista de despesas

O professor mostra um DTO externo, citado como `ResponseExpensesJson`, no plural.

Ele contém uma propriedade do tipo lista de respostas resumidas.

### Exemplo reconstruído

```csharp
public class ResponseExpensesJson
{
    public List<ResponseShortExpenseJson> Expenses { get; set; } = new();
}
```

### Por que inicializar a lista?

A inicialização faz com que a propriedade comece como **lista vazia**, em vez de `null`.

```csharp
public List<ResponseShortExpenseJson> Expenses { get; set; } = new();
```

### Diferença entre vazio e nulo

| Estado | Significado |
|---|---|
| `null` | Não existe uma instância de lista atribuída à propriedade |
| `new List<...>()` | Existe uma lista, atualmente com zero elementos |
| Lista com itens | Existe uma lista contendo respostas resumidas |

### Qual é o benefício para o consumidor?

Um contrato com lista inicializada permite tratar a propriedade como uma coleção, sem assumir que uma ausência de dados precisa ser representada por `null`.

**Entretanto, no fluxo HTTP da aula, o controller devolve `204` quando a coleção está vazia; nesse caso o JSON do DTO não é enviado ao cliente.**

---

## 6. Por que devolver somente os campos necessários?

O professor justifica a resposta curta com o cenário de telas de listagem.

### Exemplo

Uma tela que apresenta uma tabela de despesas pode precisar apenas de:

| ID | Título | Valor |
|---:|---|---:|
| 1 | Mouse | 55 |
| 2 | Comida | 15 |

Campos adicionais da entidade — como descrição completa, detalhes de pagamento ou outras informações — não precisam necessariamente ser enviados para a mesma tela.

### Vantagens do contrato resumido

- Reduz o tamanho do JSON enviado.
- Evita transmitir dados desnecessários.
- Mantém um contrato específico para o objetivo da listagem.
- Reduz o acoplamento do cliente ao modelo completo de domínio.
- Permite reservar detalhes completos para uma operação específica.

### Distinção técnica importante

Nesta aula, o professor utiliza:

```csharp
await _dbContext.Expenses.ToListAsync();
```

Isso carrega **entidades completas** do banco para a aplicação antes de mapeá-las em DTOs curtos.

Portanto, a economia demonstrada é principalmente no **tamanho da resposta HTTP**, não necessariamente na quantidade de colunas lidas do banco.

Uma projeção diretamente na consulta poderia reduzir também o volume de dados transferidos do banco, mas **não é realizada nesta aula**.

---

## 7. Por que utilizar um objeto com lista em vez de um array na raiz?

O instrutor explica uma decisão de contrato baseada na experiência dele trabalhando com equipes de front-end.

Em vez de devolver:

```json
[
  { "id": 1, "title": "Mouse", "amount": 55 },
  { "id": 2, "title": "Comida", "amount": 15 }
]
```

Ele escolhe devolver:

```json
{
  "expenses": [
    { "id": 1, "title": "Mouse", "amount": 55 },
    { "id": 2, "title": "Comida", "amount": 15 }
  ]
}
```

### Qual é a justificativa do professor?

Ele relata que, em projetos em que trabalhou, as pessoas responsáveis pelo front-end preferiam receber **um objeto contendo uma lista** em vez de receber uma lista diretamente na raiz.

A decisão atende à experiência e à convenção que ele deseja usar neste projeto.

### É uma preferência universal de front-end?

Não.

Ambos os formatos são válidos em APIs JSON.

A escolha depende do contrato, dos requisitos, da padronização e dos consumidores do sistema.

### Uma vantagem adicional possível do envelope

O objeto externo permite acrescentar metadados no futuro, caso necessário:

```json
{
  "expenses": [],
  "totalCount": 0
}
```

**Esse campo `totalCount` é apenas um exemplo de extensão possível; não faz parte do DTO implementado na gravação.**

---

## 8. Fluxo do controller e do use case

O professor explica que o controller receberá o caso de uso por **injeção de dependência**, executará a consulta e decidirá qual resposta HTTP devolver.

### Controller

Responsabilidades:

1. Receber o serviço de caso de uso.
2. Executar a operação assíncrona.
3. Conferir se há itens na resposta.
4. Devolver `Ok(response)` ou `NoContent()`.

### Use case

Responsabilidades:

1. Solicitar os dados ao repositório.
2. Aguardar a consulta.
3. Mapear as entidades para DTOs resumidos.
4. Devolver um objeto de resposta contendo a lista.

### Repositório

Responsabilidade:

1. Consultar o conjunto de despesas no `CashFlowDbContext`.
2. Materializar o resultado em lista com `ToListAsync()`.

```text
Controller
    |
    | await Execute()
    v
GetAllExpensesUseCase
    |
    | await GetAll()
    v
ExpensesRepository
    |
    | await ToListAsync()
    v
MySQL
    |
    v
List<Expense>
    |
    v
IMapper.Map<List<ResponseShortExpenseJson>>(...)
    |
    v
ResponseExpensesJson
    |
    v
Controller:
    - itens? 200 OK
    - vazio? 204 No Content
```

---

## 9. Registro do novo use case na injeção de dependência

O professor lembra que o novo caso de uso precisa ser registrado no serviço de DI para que o controller possa recebê-lo.

Ele abre `DependencyInjectionExtension` no projeto Application, dentro do método que organiza os registros de casos de uso, chamado `AddUseCases`.

### Registro da operação de cadastro, já existente

```csharp
services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
```

### Novo registro, reconstruído

```csharp
services.AddScoped<IGetAllExpensesUseCase, GetAllExpensesUseCase>();
```

### Por que AddScoped?

Porque o projeto já usa DI com ciclo de vida `Scoped` para use cases, repositórios e contexto.

Nesse padrão de API, os serviços da requisição compartilham o escopo necessário para resolver o `DbContext` e executar a operação.

### O que acontece se esquecer esse registro?

Ao tentar construir uma classe que depende de `IGetAllExpensesUseCase`, o container poderá falhar ao resolver a interface, caso não exista outro registro correspondente.

O instrutor destaca exatamente esse cuidado antes de implementar a regra de negócio.

### Sobre os nomes

A transcrição confirma a existência de uma interface e uma classe para buscar todas as despesas. O nome `IGetAllExpensesUseCase` é a reconstrução coerente com a ação `GetAllExpenses`; a grafia literal dos arquivos não foi soletrada no texto.

---

## 10. Por que esse use case não recebe IUnitOfWork?

O professor antecipa a dúvida: se o cadastro utilizava `IUnitOfWork`, por que a consulta utiliza somente o repositório?

### Resposta apresentada

Porque esta operação **não altera os dados do banco de dados**.

Seu objetivo é consultar despesas já registradas.

### Comparação

| Operação | Repositório | Unit of Work |
|---|---|---|
| Cadastrar despesa | Adiciona entidade ao contexto | Executa o commit |
| Listar despesas | Consulta as entidades | Não há commit a executar |

### Fluxo de escrita

```csharp
await _repository.Add(entity);
await _unitOfWork.Commit();
```

### Fluxo de leitura

```csharp
var expenses = await _repository.GetAll();
```

Não há chamada a `SaveChangesAsync()`.

### Observação técnica

Essa escolha é adequada para a operação simples de consulta mostrada. Em aplicações mais complexas, leituras podem participar de transações quando requisitos de consistência exigirem; a ausência de `IUnitOfWork` aqui não significa que leituras nunca possam usar transações.

**Nesta aula, a consulta é apenas de leitura e não necessita da abstração de commit.**

---

## 11. Injetando IExpensesRepository no caso de uso

O professor abre a classe do novo use case e adiciona `IExpensesRepository` como dependência.

### Campo privado

```csharp
private readonly IExpensesRepository _repository;
```

### Construtor

```csharp
public GetAllExpensesUseCase(IExpensesRepository repository)
{
    _repository = repository;
}
```

### Por que usar a interface?

A Application precisa conhecer as operações disponíveis, mas não deve depender diretamente da implementação concreta `ExpensesRepository`.

A interface funciona como o contrato, e a Infrastructure fornece sua implementação pelo container.

### O que o use case solicita?

Uma nova operação de consulta:

```csharp
var result = await _repository.GetAll();
```

Esse método ainda será acrescentado à interface e implementado na Infrastructure durante a própria aula.

---

## 12. Adicionando GetAll à interface do repositório

O instrutor acessa `IExpensesRepository` e cria uma nova operação para devolver **uma lista de entidades Expense**.

### Assinatura reconstruída

```csharp
Task<List<Expense>> GetAll();
```

### O que significa cada parte?

- `Task<...>`: o resultado será produzido por uma operação aguardável.
- `List<Expense>`: a operação fornece uma lista de entidades.
- `GetAll()`: obtém todas as despesas presentes na consulta.
- Ausência de argumentos: **neste projeto, a consulta não recebe filtros**.

### Código de contrato

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    Task Add(Expense expense);

    Task<List<Expense>> GetAll();
}
```

### O contrato já possuía Add

O método `Add` foi introduzido anteriormente e refatorado para `Task` na aula 106.

O novo método de consulta **não substitui** o de cadastro; os dois passam a coexistir na mesma interface.

### Por que o tipo de retorno é Task<List<Expense>>?

Porque o acesso ao banco será realizado usando `ToListAsync`, uma operação assíncrona do Entity Framework Core que materializa os resultados como uma lista.

O tipo literal da interface não é mostrado caractere por caractere na transcrição; a assinatura acima corresponde à semântica descrita pelo professor e ao padrão demonstrado.

---

## 13. Implementando GetAll com ToListAsync no EF Core

Depois de alterar a interface, o professor abre `ExpensesRepository`, na Infrastructure.

O Visual Studio indica que a classe deixou de implementar todos os membros do contrato.

O instrutor utiliza a sugestão **Implement Interface** e preenche o método.

### Implementação principal

```csharp
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses.ToListAsync();
}
```

### O que é _dbContext.Expenses?

É o `DbSet<Expense>` configurado no `CashFlowDbContext`, associado à tabela de despesas.

### O que faz ToListAsync?

Materializa assincronamente os resultados da consulta em uma lista.

No caso apresentado, sem filtros, a operação busca as despesas acessíveis pelo conjunto consultado.

### Namespace importante

Para utilizar `ToListAsync`, normalmente é necessário importar:

```csharp
using Microsoft.EntityFrameworkCore;
```

### O retorno da função

```csharp
return await _dbContext.Expenses.ToListAsync();
```

O método espera logicamente a conclusão da consulta ao banco e devolve uma `List<Expense>`.

### Onde fica a responsabilidade de mapear para DTO?

Não no repositório desta aula.

O repositório devolve **entidades de domínio**; o use case, com `IMapper`, transforma a lista em DTOs de resposta.

---

## 14. O exemplo hipotético de filtragem por usuário

O professor interrompe brevemente a implementação do repositório para apresentar um cenário **que não faz parte do projeto atual**.

Imagine que cada despesa pertença a uma pessoa usuária.

Poderia existir uma propriedade como:

```csharp
public long UserId { get; set; }
```

E o repositório poderia filtrar:

```csharp
_dbContext.Expenses
    .Where(expense => expense.UserId == userId)
    .ToListAsync();
```

### Para que serviria isso?

Para recuperar despesas pertencentes a um usuário específico, em vez de devolver registros de todos os usuários.

### Mas isso é implementado no CashFlow?

**Não.**

O professor insere `UserId` apenas para explicar a ideia e depois apaga a propriedade.

Ele afirma que o exemplo atual do curso considera uma aplicação **de usuário único**, sem associação de cada despesa a uma conta diferente.

### Qual consulta permanece no código da aula?

```csharp
_dbContext.Expenses.ToListAsync();
```

### Cuidado em aplicações multiusuário

Se um sistema real armazena dados de diferentes usuários ou organizações, a ausência de filtros de autorização pode expor informações indevidas.

Nesse caso, o escopo da consulta precisaria considerar a identidade e a autorização de quem fez a requisição.

**Essa necessidade é um complemento arquitetural, não uma funcionalidade construída no vídeo.**

---

## 15. O que ToListAsync faz no banco de dados?

O instrutor explica que o método `ToListAsync` realiza a execução efetiva da consulta SQL para recuperar as despesas.

### Construção da consulta

```csharp
_dbContext.Expenses
```

Representa um conjunto consultável de entidades.

### Execução e materialização

```csharp
await _dbContext.Expenses.ToListAsync()
```

Solicita a execução da consulta e o carregamento dos resultados em uma lista.

### SQL conceitual

```sql
SELECT *
FROM Expenses;
```

Esse SQL é uma aproximação ilustrativa. O SQL concreto produzido pelo provider do EF Core pode variar em nomes de tabelas, colunas e mapeamentos.

### Por que ToListAsync é diferente de apenas montar uma query?

Em LINQ to Entities, muitas operações como `Where` constroem a expressão da consulta.

Métodos como `ToListAsync` **disparam a execução** e materializam os resultados.

### Exemplo

```csharp
var query = _dbContext.Expenses
    .Where(expense => expense.Amount > 0);

// A execução efetiva é solicitada aqui:
var expenses = await query.ToListAsync();
```

O filtro acima é somente um complemento didático: a consulta da aula usa diretamente `Expenses.ToListAsync()`, sem `Where`.

### O que acontece quando não há registros?

A operação normalmente devolve uma **lista vazia**, e não `null`.

Esse resultado permite ao controller decidir posteriormente se responde com `204 No Content`.

---

## 16. Configurando o AutoMapper para a resposta resumida

O professor retorna à classe `AutoMapping`, criada na aula 107.

O Profile já possuía um mapa da entidade para a resposta do cadastro:

```csharp
CreateMap<Expense, ResponseRegisterExpenseJson>();
```

Agora é necessário mais um mapa, porque a listagem utiliza um DTO diferente.

### Novo mapa

```csharp
CreateMap<Expense, ResponseShortExpenseJson>();
```

### Origem

`Expense` — entidade do Domain.

### Destino

`ResponseShortExpenseJson` — DTO resumido utilizado pela listagem.

### Por que criar um mapa adicional?

Porque o DTO usado na criação e o DTO usado na listagem têm finalidades diferentes.

A resposta resumida possui somente os campos necessários à tela de listagem.

### Onde esse mapa fica?

No método auxiliar que organiza a configuração de entidade para resposta:

```csharp
private void EntityToResponse()
{
    CreateMap<Expense, ResponseRegisterExpenseJson>();
    CreateMap<Expense, ResponseShortExpenseJson>();
}
```

### O que não foi criado?

O professor informa que **não fará um mapeamento para o DTO externo `ResponseExpensesJson`**.

Ele opta por montar o objeto externo manualmente e utilizar o AutoMapper apenas para a coleção de itens resumidos.

---

## 17. Mapeamento de listas com IMapper

Com `GetAll()` implementado, o use case obtém:

```csharp
List<Expense> result = await _repository.GetAll();
```

Mas a resposta HTTP não deve devolver uma lista de entidades do Domain.

Ela deve devolver uma lista de `ResponseShortExpenseJson`.

### Conversão

```csharp
var expenses = _mapper.Map<List<ResponseShortExpenseJson>>(result);
```

### O que é passado entre os parênteses?

O objeto de **origem**, aqui `result`, do tipo `List<Expense>`.

### O que aparece no tipo genérico?

O tipo de **destino**, aqui `List<ResponseShortExpenseJson>`.

### O AutoMapper precisa de CreateMap<List<Expense>, List<ResponseShortExpenseJson>>()?

Não nesse cenário comum.

Basta configurar o mapeamento **dos elementos**:

```csharp
CreateMap<Expense, ResponseShortExpenseJson>();
```

O AutoMapper possui suporte a conversão de coleções quando o mapeamento entre os elementos é conhecido.

### Fluxo

```text
List<Expense>
    |
    | cada item usa
    | CreateMap<Expense, ResponseShortExpenseJson>()
    v
List<ResponseShortExpenseJson>
```

### Por que isso é útil?

Evita escrever um `foreach` que cria manualmente um objeto de resposta para cada entidade da lista.

---

## 18. Construindo ResponseExpensesJson no use case

O professor injeta `IMapper` no novo use case e utiliza a conversão da lista ao montar o objeto externo de resposta.

### Dependências

```csharp
private readonly IExpensesRepository _repository;
private readonly IMapper _mapper;
```

### Construtor

```csharp
public GetAllExpensesUseCase(
    IExpensesRepository repository,
    IMapper mapper)
{
    _repository = repository;
    _mapper = mapper;
}
```

### Implementação conceitual de Execute

```csharp
public async Task<ResponseExpensesJson> Execute()
{
    var result = await _repository.GetAll();

    return new ResponseExpensesJson
    {
        Expenses = _mapper.Map<List<ResponseShortExpenseJson>>(result)
    };
}
```

### Por que criar new ResponseExpensesJson?

O professor escolhe não mapear a estrutura externa inteira com AutoMapper.

O caso de uso instancia o envelope e preenche sua lista com os itens convertidos.

### Por que o resultado é Task<ResponseExpensesJson>?

A execução do repositório é assíncrona, e o método do caso de uso precisa aguardar sua conclusão.

O objeto `ResponseExpensesJson` é devolvido como resultado da Task.

### O use case usa IUnitOfWork?

Não.

Como se trata apenas de consulta, não há chamada de commit.

### Atenção à nomenclatura

A transcrição usa expressões como "responseExpenses.json" e "response short expense json"; a representação deste material utiliza `ResponseExpensesJson` para o envelope e `ResponseShortExpenseJson` para o item.

Os namespaces e nomes literais dos arquivos podem ser conferidos no projeto original se necessário.

---

## 19. Código consolidado por camada

Esta seção reúne exemplos reconstruídos a partir do conteúdo descrito.

### 19.1 Communication: item resumido

```csharp
public class ResponseShortExpenseJson
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
```

### 19.2 Communication: envelope

```csharp
public class ResponseExpensesJson
{
    public List<ResponseShortExpenseJson> Expenses { get; set; } = new();
}
```

### 19.3 Domain: contrato do repositório

```csharp
public interface IExpensesRepository
{
    Task Add(Expense expense);

    Task<List<Expense>> GetAll();
}
```

### 19.4 Infrastructure: implementação de GetAll

```csharp
using Microsoft.EntityFrameworkCore;

// Dentro da classe ExpensesRepository:

public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses.ToListAsync();
}
```

### 19.5 Application: Profile do AutoMapper

```csharp
// Dentro da classe AutoMapping : Profile:

private void EntityToResponse()
{
    CreateMap<Expense, ResponseRegisterExpenseJson>();
    CreateMap<Expense, ResponseShortExpenseJson>();
}
```

### 19.6 Application: interface do caso de uso

```csharp
public interface IGetAllExpensesUseCase
{
    Task<ResponseExpensesJson> Execute();
}
```

### 19.7 Application: implementação do caso de uso

```csharp
using AutoMapper;

public class GetAllExpensesUseCase : IGetAllExpensesUseCase
{
    private readonly IExpensesRepository _repository;
    private readonly IMapper _mapper;

    public GetAllExpensesUseCase(
        IExpensesRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseExpensesJson> Execute()
    {
        var result = await _repository.GetAll();

        return new ResponseExpensesJson
        {
            Expenses = _mapper.Map<List<ResponseShortExpenseJson>>(result)
        };
    }
}
```

### 19.8 Application: registro de DI

```csharp
// Dentro de AddUseCases(IServiceCollection services):

services.AddScoped<IGetAllExpensesUseCase, GetAllExpensesUseCase>();
```

O registro do use case de cadastro, feito em aulas anteriores, permanece.

### 19.9 API: action GET

```csharp
[HttpGet]
[ProducesResponseType(
    typeof(ResponseExpensesJson),
    StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> GetAllExpenses(
    [FromServices] IGetAllExpensesUseCase useCase)
{
    var response = await useCase.Execute();

    if (response.Expenses.Count > 0)
        return Ok(response);

    return NoContent();
}
```

**Observação:** a transcrição não especifica a rota exata, o nome literal da action ou a expressão completa de verificação da coleção. O código acima demonstra a estrutura e os comportamentos confirmados oralmente, **não uma transcrição literal de um arquivo do projeto**.

### 19.10 O Program.cs muda?

A aula não apresenta alteração no `Program.cs`.

O novo caso de uso é registrado dentro de `AddApplication()` por meio do método auxiliar `AddUseCases` já existente.

### 19.11 O CashFlowDbContext muda?

Não.

A consulta utiliza a propriedade `Expenses` que já estava configurada e acessível no contexto das aulas anteriores.

---

## 20. Teste no Swagger e os seis resultados

Depois das alterações, o professor coloca um breakpoint e inicia a API pelo Visual Studio.

O Swagger apresenta a nova operação de consulta GET.

### Passo a passo da gravação

1. Iniciar a aplicação pelo Visual Studio.
2. Abrir o Swagger.
3. Expandir o endpoint GET de listagem.
4. Clicar em **Try it out**.
5. Executar a requisição sem informar argumentos.
6. O depurador entra no caso de uso.
7. O repositório é chamado.
8. `ToListAsync()` busca as despesas no banco.
9. O resultado retorna ao use case.
10. O AutoMapper transforma as entidades em respostas resumidas.
11. O controller verifica que há despesas na coleção.
12. A action devolve `Ok(response)`.
13. O Swagger exibe o objeto de resposta.

### Quantos itens aparecem?

O professor inspeciona e relata **seis despesas**.

### Que propriedades aparecem na resposta?

Para cada item, são exibidos:

- `Id`;
- `Title`;
- `Amount`.

### Qual é o resultado HTTP?

**200 OK**.

### E o 204?

O professor confere que `204 No Content` aparece como possibilidade **documentada** no Swagger.

A gravação **não demonstra uma consulta executada contra um banco vazio para comprovar o caminho 204**. O comportamento é descrito na implementação do endpoint e na documentação.

### O que não foi testado?

- Nenhum filtro por usuário.
- Nenhuma paginação.
- Nenhum cenário com milhares de despesas.
- Nenhum endpoint de detalhes por ID.
- Nenhuma requisição de consulta para um banco vazio.
- Nenhuma medição de desempenho.
- Nenhum teste automatizado novo.

A demonstração valida a consulta e o mapeamento no cenário em que existem registros.

---

## 21. Melhorias e ressalvas arquiteturais

As observações desta seção são **complementos para aprofundamento**. O professor termina anunciando que existe algo a melhorar no endpoint, mas não informa, nesta transcrição, qual será exatamente a próxima alteração. Portanto, não devemos atribuir a ele uma melhoria específica ainda não demonstrada.

### 21.1 AsNoTracking para operações de leitura

Uma consulta que não pretende modificar entidades pode usar:

```csharp
var expenses = await _dbContext.Expenses
    .AsNoTracking()
    .ToListAsync();
```

Isso evita o acompanhamento de alterações dessas entidades pelo `Change Tracker`.

A opção pode reduzir overhead em consultas somente de leitura, dependendo do cenário.

**O professor não usa `AsNoTracking()` nesta aula.**

### 21.2 Projetar diretamente os campos necessários

A implementação demonstrada carrega a entidade inteira:

```csharp
await _dbContext.Expenses.ToListAsync();
```

Depois, o AutoMapper cria respostas resumidas.

Uma alternativa é selecionar apenas os campos necessários na consulta:

```csharp
var expenses = await _dbContext.Expenses
    .Select(e => new ResponseShortExpenseJson
    {
        Id = e.Id,
        Title = e.Title,
        Amount = e.Amount
    })
    .ToListAsync();
```

Essa abordagem poderia reduzir a transferência de colunas entre MySQL e API.

**Porém, ela representaria uma decisão diferente da arquitetura mostrada nesta aula:** o código acima é uma alternativa ilustrativa e não a implementação do curso.

### 21.3 ProjectTo e AutoMapper

O AutoMapper também dispõe de mecanismos de projeção de consultas, como `ProjectTo<TDestination>()`, que podem permitir a tradução da seleção de campos para o banco quando suportada.

Isso requer cuidados específicos com mapeamentos e tradução de expressões pelo provider do EF Core.

**O professor não aborda `ProjectTo` nesta gravação.**

### 21.4 Paginação para coleções grandes

Um `GetAll()` sem filtros ou paginação recupera todas as despesas disponíveis para a consulta.

Em aplicações com grande volume de registros, isso pode aumentar consumo de memória, transferência de dados e tempo de resposta.

Uma evolução possível seria utilizar:

- paginação com `Skip`/`Take`;
- filtros;
- ordenação estável;
- metadados de total de registros.

Nenhum desses recursos é implementado nesta aula.

### 21.5 Ordenação não foi especificada

O código:

```csharp
_dbContext.Expenses.ToListAsync();
```

não determina uma ordem específica dos resultados.

Em SQL, a ordem não deve ser presumida sem `ORDER BY`.

Se a aplicação exigir ordenação por data, título ou identificador, é preciso configurá-la explicitamente.

### 21.6 Segurança em aplicações multiusuário

O professor demonstra um possível filtro por `UserId` e depois o remove por não fazer parte do CashFlow neste momento.

Em uma aplicação multiusuário real, o filtro de acesso não deve ser opcional se for necessário evitar a exposição de despesas de outros usuários.

### 21.7 Contrato de lista vazia

Há duas escolhas comuns para uma coleção vazia:

**Contrato A — usado na aula:**

```text
204 No Content
```

**Contrato B — alternativa frequente:**

```json
{
  "expenses": []
}
```

com status `200 OK`.

Ambos são possíveis, mas o consumidor precisa conhecer a convenção adotada.

### 21.8 O DTO de listagem não deve expor entidades diretamente

Devolver `Expense` integralmente poderia acoplar clientes da API à modelagem de domínio e enviar dados desnecessários.

O curso utiliza `ResponseShortExpenseJson`, o que mantém o contrato mais específico.

### 21.9 IUnitOfWork não é requisito para executar SELECT

O padrão de Unit of Work continua útil nos casos de escrita. Nesta operação, não é necessário solicitar commit.

Isso não elimina `DbContext`; ele continua sendo utilizado pelo repositório para consultar o banco.

### 21.10 ToListAsync não é uma chamada que deve ficar sem await

O método devolve uma `Task<List<Expense>>`.

O use case precisa aguardar seu resultado antes de transformá-lo em DTOs.

A implementação mostrada preserva a programação assíncrona das aulas 104–106.

### 21.11 Entity Framework Core continua encapsulado na Infrastructure

O caso de uso conhece apenas `IExpensesRepository`.

Quem utiliza `DbSet<Expense>` e `ToListAsync()` é o repositório concreto.

Essa separação mantém o princípio arquitetural desenvolvido ao longo do módulo.

### 21.12 O envelope de resposta é uma decisão de contrato

O uso de um objeto externo contendo uma lista é uma preferência de API descrita pelo professor, não uma exigência de HTTP, JSON ou dos frameworks de front-end.

---

## 22. Exercícios e perguntas de revisão

### Exercício 1 — criar o contrato de consulta

**Pergunta:** qual assinatura permite consultar assincronamente uma lista de `Expense`?

**Resposta:**

```csharp
Task<List<Expense>> GetAll();
```

### Exercício 2 — implementar o repositório

**Pergunta:** como materializar todas as despesas utilizando EF Core?

**Resposta:**

```csharp
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses.ToListAsync();
}
```

### Exercício 3 — mapear a coleção

**Pergunta:** como converter uma `List<Expense>` em `List<ResponseShortExpenseJson>`?

**Resposta:**

```csharp
var items = _mapper.Map<List<ResponseShortExpenseJson>>(expenses);
```

### Exercício 4 — declarar o Profile

**Pergunta:** qual configuração permite mapear cada elemento da lista?

**Resposta:**

```csharp
CreateMap<Expense, ResponseShortExpenseJson>();
```

### Exercício 5 — construir o envelope

**Pergunta:** como colocar os itens resumidos dentro do DTO externo?

**Resposta:**

```csharp
return new ResponseExpensesJson
{
    Expenses = _mapper.Map<List<ResponseShortExpenseJson>>(expenses)
};
```

### Exercício 6 — escolher a resposta HTTP

**Pergunta:** qual resposta o professor escolhe para uma consulta sem despesas?

**Resposta:** `204 No Content`.

**Pergunta:** qual resposta utiliza quando existem despesas?

**Resposta:** `200 OK` com o DTO de listagem.

### Questões adicionais

1. Qual funcionalidade foi adicionada ao CashFlow?
2. Quais componentes já estavam previamente preparados no início da gravação?
3. Qual é a diferença entre `ResponseExpensesJson` e `ResponseShortExpenseJson`?
4. Quais três dados o DTO resumido contém?
5. Por que o ID aparece em uma resposta de listagem?
6. Por que o professor prefere um objeto com lista a uma lista na raiz do JSON?
7. Essa preferência é obrigatória para todo front-end?
8. Qual é a diferença entre lista vazia e lista nula?
9. Qual ciclo de vida de DI foi usado para o novo caso de uso?
10. Por que o use case utiliza `IExpensesRepository`?
11. Por que ele não precisa de `IUnitOfWork`?
12. Qual novo método foi declarado na interface do repositório?
13. O que `ToListAsync()` faz?
14. Qual é a diferença entre construir uma expressão LINQ e materializar seus resultados?
15. A consulta da aula usa `Where`?
16. Em que contexto o professor demonstra o exemplo com `UserId`?
17. A propriedade `UserId` permanece no projeto após a explicação?
18. O que seria necessário em uma aplicação multiusuário para limitar o acesso às despesas?
19. Qual mapeamento foi adicionado ao Profile?
20. É necessário configurar um mapa entre `List<Expense>` e `List<ResponseShortExpenseJson>` explicitamente?
21. Como o caso de uso transforma o resultado em um envelope?
22. Quantas despesas o professor observou no teste?
23. Qual status HTTP foi retornado no Swagger?
24. O vídeo comprova uma execução do cenário de banco vazio?
25. O professor implementa `AsNoTracking`, paginação ou projeção para DTO na consulta?
26. Por que uma listagem sem `OrderBy` não possui ordem garantida?
27. Qual é a diferença entre economizar dados no JSON HTTP e economizar dados na consulta SQL?
28. Qual responsabilidade pertence ao controller e qual pertence ao use case?
29. Quais partes da execução são assíncronas?
30. Qual questão o professor anuncia para a próxima aula, sem revelar a implementação?

---

## 23. Resumo e mapa mental

### O que foi desenvolvido

- O professor apresenta uma action GET previamente preparada para listar despesas.
- O contrato prevê **200 OK** quando houver itens e **204 No Content** quando não houver.
- O DTO externo, `ResponseExpensesJson`, contém uma lista inicializada de itens resumidos.
- Cada item, `ResponseShortExpenseJson`, contém **ID, título e valor**.
- A decisão de usar DTOs resumidos atende ao cenário de dashboard exposto pelo professor.
- A escolha de devolver um objeto com uma propriedade de lista se baseia na experiência de integração com front-end do instrutor.
- O novo use case é registrado na DI.
- O use case recebe `IExpensesRepository`, mas não `IUnitOfWork` por se tratar de uma consulta.
- `IExpensesRepository` passa a declarar `GetAll()` retornando uma tarefa com uma lista de despesas.
- `ExpensesRepository` implementa a operação usando `await _dbContext.Expenses.ToListAsync()`.
- O exemplo de filtragem por `UserId` é apenas ilustrativo e é removido.
- O Profile do AutoMapper recebe o mapeamento `Expense -> ResponseShortExpenseJson`.
- O use case também recebe `IMapper`.
- A lista de entidades é convertida com `_mapper.Map<List<ResponseShortExpenseJson>>(result)`.
- O envelope `ResponseExpensesJson` é criado diretamente, sem um mapa para ele.
- O teste no Swagger encontra **seis despesas**.
- A action devolve **200 OK** com uma lista de objetos resumidos.
- O Swagger também documenta **204 No Content**, mas esse caminho não é executado no vídeo.
- O professor finaliza mencionando que existe uma melhoria a fazer no endpoint, sem detalhá-la nesta gravação.

### Antes e depois

| Aspecto | Antes da aula | Depois da aula |
|---|---|---|
| Cadastro de despesas | Implementado | Permanece implementado |
| Listagem GET | Estrutura inicial preparada | Fluxo implementado e testado |
| Repositório | `Add` | `Add` + `GetAll` |
| Consulta SQL | Não demonstrada nessa funcionalidade | `ToListAsync` |
| DTO resumido | Preparado | Usado no mapeamento |
| AutoMapper | Maps de cadastro | Novo map para resposta resumida |
| Use case de listagem | Esqueleto vazio | Consulta e mapeia resultados |
| Unidade de trabalho | Utilizada na escrita | Não é necessária nessa leitura |
| DI | Use case ainda não registrado | Novo serviço Scoped registrado |
| Swagger | Contrato GET preparado | HTTP 200 testado com seis registros |

### Mapa mental

```text
                              AULA 109
                     GET DE TODAS AS DESPESAS
                                |
                  +-------------+--------------+
                  |                            |
               CONTROLLER                  USE CASE
                  |                            |
          GET / despesas          IGetAllExpensesUseCase
                  |                            |
           Resposta HTTP               IExpensesRepository
                  |                            |
           +------+-------+                  GetAll()
           |              |                    |
           v              v                    v
        Tem itens       Não tem            Infrastructure
           |              |                    |
           v              v                    v
         200 OK      204 No Content       ExpensesRepository
           |                                   |
           v                                   v
    ResponseExpensesJson               DbSet<Expense>
           |                                   |
           v                                   v
    Expenses: lista                       ToListAsync()
           |                                   |
           v                                   v
   ResponseShortExpenseJson                MySQL
           |
           +-- Id
           +-- Title
           +-- Amount
           |
           v
     AutoMapper:
     CreateMap<Expense, ResponseShortExpenseJson>()
           |
           v
     _mapper.Map<List<ResponseShortExpenseJson>>(...)
           |
           v
          TESTE
           |
        6 despesas
           |
           v
        HTTP 200
```

**Conclusão:** a aula 109 adiciona ao CashFlow um endpoint assíncrono de listagem de despesas, utilizando `ToListAsync()` para recuperar as entidades do banco e AutoMapper para transformá-las em uma lista de respostas resumidas. O controller retorna `200 OK` quando existem despesas e `204 No Content` quando não existem. O teste demonstrado confirma seis itens e o corpo de resposta com ID, título e valor. A aula mantém a separação entre **consulta**, **mapeamento** e **contrato HTTP**, sem adicionar commit a uma operação somente de leitura.
