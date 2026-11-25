# Aula 115 — Exclusão de despesas: DELETE, Remove, Unit of Work e documentação XML

> **Contexto:** continuidade do CashFlow após a segregação dos repositórios (aula 113) e a refatoração polimórfica do filtro de exceções (aula 114).
>
> **Objetivo:** implementar o fluxo de exclusão de uma despesa pelo ID, utilizando IExpensesWriteOnlyRepository, IUnitOfWork e o tratamento centralizado de NotFoundException.
>
> **Resultado observado:** inicialmente, a ausência do registro de DI gera HTTP 500. Após corrigir o registro, o professor exclui a despesa **Pizza, ID 33**, recebe **HTTP 204 No Content** e confirma no MySQL que a despesa desapareceu da tabela.
>
> **Fidelidade ao material:** o endpoint, a interface e o esqueleto do caso de uso já estavam preparados. A implementação do repositório, a documentação XML, o registro de DI e os testes são desenvolvidos na gravação. Os exemplos abaixo reconstroem o código narrado. As correções técnicas sobre FindAsync, Remove e entidades sem rastreamento são explicitamente separadas das afirmações do instrutor.

## Índice

1. Objetivo e contexto
2. Contrato HTTP do endpoint DELETE
3. Por que retornar 204 sem a entidade excluída?
4. Componentes previamente preparados
5. Dependências do DeleteExpenseUseCase
6. Contrato assíncrono de exclusão
7. O significado do retorno booleano
8. Documentação XML em C#
9. Implementação do repositório
10. FirstOrDefaultAsync e FindAsync
11. Remove e o Change Tracker
12. AsNoTracking e exclusão de entidades
13. Unit of Work e a persistência da remoção
14. Tratamento de NotFoundException
15. Contexto de autorização e propriedade dos dados
16. Registro do use case na DI
17. Código consolidado por camada
18. Fluxo completo da exclusão
19. Testes no Swagger e no banco
20. Boas práticas, limites e alternativas
21. Exercícios e perguntas de revisão
22. Resumo e mapa mental

---

## 1. Objetivo e contexto

O CashFlow já possui operações para registrar, listar e consultar despesas por ID.

A partir desta aula, passa a oferecer uma operação de exclusão.

| Operação | Método HTTP | Finalidade |
|---|---|---|
| Registro | POST | Criar uma despesa |
| Listagem | GET | Recuperar despesas resumidas |
| Detalhes | GET | Recuperar uma despesa por ID |
| Exclusão | DELETE | Remover uma despesa por ID |

A arquitetura continua dividida entre API, Application, Domain, Infrastructure, Communication e o projeto de exceções.

O professor utiliza o contrato IExpensesWriteOnlyRepository, criado na aula 113, porque excluir uma despesa é uma operação de escrita: altera o estado persistido da aplicação.

A segregação do repositório e o tratamento de erros já existentes serão reutilizados.

---

## 2. Contrato HTTP do endpoint DELETE

O professor apresenta um endpoint previamente preparado, do tipo DELETE, que recebe na rota o ID da despesa.

Exemplo conceitual de requisição:

```http
DELETE /expenses/33
```

A rota completa acima é ilustrativa; a gravação confirma o método HTTP e o recebimento do identificador na rota.

O endpoint documenta duas respostas de negócio:

| Situação | Status | Corpo |
|---|---|---|
| Exclusão concluída | 204 No Content | Sem corpo |
| Despesa não encontrada | 404 Not Found | ResponseErrorJson |

### Action reconstruída

```csharp
[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(
    [FromServices] IDeleteExpenseUseCase useCase,
    [FromRoute] long id)
{
    await useCase.Execute(id);

    return NoContent();
}
```

O nome exato da action, o prefixo de rota e a ordem dos parâmetros podem variar no arquivo original.

### Observação

Os atributos ProducesResponseType descrevem o contrato da API, mas não limitam automaticamente os resultados possíveis em tempo de execução. O teste inicial da própria aula gera HTTP 500 por ausência de registro de DI, apesar de o endpoint documentar 204 e 404.

---

## 3. Por que retornar 204 sem a entidade excluída?

O professor explica que alguns desenvolvedores preferem devolver os dados do registro removido.

Ele considera isso desnecessário para o cenário atual do CashFlow.

A informação relevante é que a operação foi concluída com sucesso.

### HTTP 204 No Content

O status indica que a solicitação foi processada com sucesso e não há conteúdo de resposta a enviar.

A action executa:

```csharp
await useCase.Execute(id);
return NoContent();
```

Não há variável para receber o resultado porque o caso de uso devolve Task, sem valor de resultado.

### Comparação

| Contrato possível | Resposta |
|---|---|
| Escolhido pelo professor | 204, sem corpo |
| Alternativa com detalhes da entidade excluída | 200, com um corpo apropriado |
| Operação ainda não concluída, em outro desenho de API | 202 Accepted |

O código 202 é um complemento conceitual, não uma alternativa implementada nesta aula.

O HTTP 204 não significa que nenhuma operação foi realizada. Significa que não existe conteúdo de resposta para devolver após seu sucesso.

O professor apresenta o retorno sem corpo como uma decisão de contrato, não como uma obrigação universal de toda operação DELETE.

---

## 4. Componentes previamente preparados

No início da gravação, o professor já possui:

- A action DELETE no controller.
- A interface IDeleteExpenseUseCase.
- A classe DeleteExpenseUseCase.
- As dependências do caso de uso no construtor.
- A chamada ao repositório, a verificação do booleano e a chamada ao Unit of Work.

Ainda precisa:

1. Completar a documentação do método Delete na interface.
2. Implementar a exclusão na classe ExpensesRepository.
3. Registrar o novo caso de uso no container de DI.
4. Testar o funcionamento do endpoint e verificar a remoção no banco.

O esquecimento inicial do terceiro item é demonstrado durante os testes, tornando-se uma parte importante da aula.

---

## 5. Dependências do DeleteExpenseUseCase

O caso de uso recebe duas dependências.

```csharp
private readonly IExpensesWriteOnlyRepository _repository;
private readonly IUnitOfWork _unitOfWork;

public DeleteExpenseUseCase(
    IExpensesWriteOnlyRepository repository,
    IUnitOfWork unitOfWork)
{
    _repository = repository;
    _unitOfWork = unitOfWork;
}
```

### IExpensesWriteOnlyRepository

Disponibiliza operações de escrita relacionadas a despesas.

Na aula 113, o instrutor já havia anunciado que colocaria a futura operação Delete nessa interface.

### IUnitOfWork

Responsável por confirmar as alterações realizadas no contexto, conforme a organização definida nas aulas 103 e 106.

### Por que não IMapper?

Não existe um DTO de sucesso para construir.

A operação retorna apenas a conclusão da Task; a API transforma essa conclusão em HTTP 204.

### Por que não IExpensesReadOnlyRepository?

Nesta implementação, o repositório de escrita recebe o ID e realiza internamente a busca necessária para excluir.

O use case não consulta uma entidade separadamente.

Essa decisão depende do cenário atual: o professor informa que o projeto ainda não associa as despesas a usuários distintos.

---

## 6. Contrato assíncrono de exclusão

O contrato do caso de uso é reconstruído como:

```csharp
public interface IDeleteExpenseUseCase
{
    Task Execute(long id);
}
```

Task representa a conclusão da operação sem um valor de resultado.

O controller pode aguardar essa Task antes de devolver NoContent.

Já o método do repositório utiliza Task<bool>:

```csharp
public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);

    Task<bool> Delete(long id);
}
```

### Diferença entre Task e Task<bool>

| Método | Retorno | Significado |
|---|---|---|
| Execute(long id) | Task | Conclusão ou falha do caso de uso |
| Delete(long id) | Task<bool> | Resultado da busca e preparação da remoção |

O método do repositório precisa informar se encontrou a despesa.

O caso de uso utiliza esse resultado para decidir se prossegue ou lança uma exceção.

É importante não confundir a ausência de um valor de retorno em Task com a ausência de espera: o controller continua executando await.

---

## 7. O significado do retorno booleano

O professor chama atenção para a dificuldade de interpretar o resultado de uma função sem conhecer seu contrato.

```csharp
var result = await _repository.Delete(id);
```

A variável result contém true ou false.

Mas essas duas possibilidades, isoladamente, não explicam quais condições cada valor representa.

No código apresentado:

- false: nenhuma despesa correspondente ao ID foi encontrada;
- true: a despesa foi encontrada e a operação Remove foi chamada para preparar sua exclusão.

### Precisão importante

O professor descreve true como sucesso da remoção. Entretanto, na arquitetura implementada, o commit ocorre **depois** de Delete.

Portanto, nesse momento, true não garante que o DELETE SQL já foi executado e confirmado.

Ele comunica que o repositório encontrou a entidade e preparou a exclusão no contexto.

O sucesso definitivo do caso de uso depende também da conclusão de Commit().

Uma falha no banco durante SaveChangesAsync ainda pode ocorrer depois de o repositório devolver true.

### Código do use case

```csharp
public async Task Execute(long id)
{
    var result = await _repository.Delete(id);

    if (result == false)
        throw new NotFoundException(
            ResourceErrorMessages.ExpenseNotFound);

    await _unitOfWork.Commit();
}
```

Também seria possível escrever if (!result). A transcrição descreve uma verificação de resultado falso; a forma exata do operador é reconstruída.

O nome ResourceErrorMessages segue a nomenclatura reconstruída nas aulas 112 e 114 e deve ser ajustado à grafia literal do projeto.

---

## 8. Documentação XML em C#

O professor aproveita o retorno booleano para ensinar a documentação de métodos em C#.

No Visual Studio, acima da declaração de Delete, ele digita três barras:

```csharp
///
```

O editor gera uma estrutura de comentário de documentação XML.

### Forma apresentada conceitualmente

```csharp
/// <summary>
/// This function returns true if the removal was successful.
/// Otherwise, returns false.
/// </summary>
Task<bool> Delete(long id);
```

A redação em inglês foi reconstruída a partir do sentido narrado. A transcrição não fornece cada palavra do comentário final exibido na tela.

### Qual é o benefício?

Ao passar o cursor sobre Delete em um caso de uso, o Visual Studio apresenta o comentário na documentação rápida (Quick Info/IntelliSense).

O consumidor pode compreender a semântica do método sem precisar abrir sua implementação.

### Documentação mais precisa — complemento

É possível descrever separadamente o objetivo, os parâmetros e o valor de retorno.

```csharp
/// <summary>
/// Finds an expense and marks it for deletion.
/// </summary>
/// <param name="id">The expense identifier.</param>
/// <returns>
/// True if the expense was found and marked for deletion;
/// otherwise, false.
/// </returns>
/// <remarks>
/// The deletion is persisted when the unit of work commits.
/// </remarks>
Task<bool> Delete(long id);
```

Essa versão é uma melhoria didática que distingue a preparação da exclusão de sua persistência efetiva. Não é uma transcrição literal do comentário escrito pelo professor.

### Tags comuns

| Tag XML | Finalidade |
|---|---|
| summary | Descrever a finalidade do membro |
| param | Explicar um parâmetro |
| returns | Explicar o retorno |
| remarks | Acrescentar observações |
| exception | Documentar exceções relevantes |
| see | Referenciar outro tipo ou membro |

O professor demonstra principalmente o comentário gerado por /// e sua visualização pelo editor.

### XML documentation e Swagger

Esse comentário documenta o contrato C# do repositório. Ele não modifica automaticamente o contrato HTTP ou o comportamento do método.

A geração de arquivos de documentação XML e sua integração com ferramentas externas dependem da configuração do projeto.

---

## 9. Implementação do repositório

O professor abre ExpensesRepository, na Infrastructure, e implementa o método Delete.

A busca utiliza FirstOrDefaultAsync com um predicado sobre o ID.

### Implementação reconstruída

```csharp
public async Task<bool> Delete(long id)
{
    var result = await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);

    if (result is null)
        return false;

    _dbContext.Expenses.Remove(result);

    return true;
}
```

### Etapas

1. Consultar a despesa correspondente ao ID.
2. Se não houver correspondência, retornar false.
3. Caso exista, chamar Remove para preparar a exclusão.
4. Retornar true.

A função não chama SaveChangesAsync diretamente.

A responsabilidade de confirmar as mudanças continua sendo do Unit of Work.

### Por que async?

O método executa uma consulta assíncrona por meio de FirstOrDefaultAsync.

A chamada Remove é síncrona, mas isso não impede que Delete seja um método assíncrono.

```text
Delete(id)
    |
    v
await FirstOrDefaultAsync(...)
    |
    +-- null --> false
    |
    +-- Expense --> Remove(expense) --> true
```

---

## 10. FirstOrDefaultAsync e FindAsync

Durante a gravação, o professor inicialmente considera utilizar FindAsync.

Depois escolhe FirstOrDefaultAsync e passa o predicado:

```csharp
expense => expense.Id == id
```

### O código mantido na aula

```csharp
var result = await _dbContext.Expenses
    .FirstOrDefaultAsync(expense => expense.Id == id);
```

### Correção técnica importante sobre FindAsync

Na gravação, o professor afirma que FindAsync lançaria uma exceção se não encontrasse uma entidade com o ID informado.

**Esse comportamento não corresponde ao contrato do EF Core.** Quando uma busca válida por chave primária não encontra a entidade, FindAsync devolve null.

É possível ocorrer uma exceção por argumentos inválidos, configuração incorreta ou falhas na consulta, mas a mera inexistência da chave não é motivo para lançar uma exceção.

### Comparação dos métodos

| Método | Consulta | Ausência |
|---|---|---|
| FirstAsync(predicado) | Primeira correspondência ao predicado | Lança InvalidOperationException |
| FirstOrDefaultAsync(predicado) | Primeira correspondência ao predicado | null para uma entidade de referência |
| FindAsync(chave) | Busca específica pela chave primária | null |

FindAsync possui uma característica adicional: primeiro verifica se a entidade com aquela chave já está rastreada no contexto. Caso esteja, pode retorná-la sem consultar o banco. Caso contrário, realiza a busca e, quando encontrada, associa a entidade ao contexto.

FirstOrDefaultAsync permite expressar predicados arbitrários e normalmente executa a consulta correspondente.

### Alternativa válida

Se Id for a chave primária, esta implementação também pode ser apropriada:

```csharp
var result = await _dbContext.Expenses.FindAsync(id);

if (result is null)
    return false;

_dbContext.Expenses.Remove(result);
return true;
```

Essa alternativa é um complemento técnico, não a versão final adotada na aula.

---

## 11. Remove e o Change Tracker

A exclusão é preparada por:

```csharp
_dbContext.Expenses.Remove(result);
```

O método informa ao EF Core que uma entidade existente deve ser removida ao salvar as alterações.

### Estados relevantes

```text
Consulta com rastreamento
          |
          v
       Unchanged
          |
          | Remove(entity)
          v
        Deleted
          |
          | SaveChangesAsync()
          v
        DELETE SQL
```

O exemplo considera uma entidade existente recuperada do banco, como a Pizza do teste.

Quando a remoção é persistida com sucesso e as alterações são aceitas pelo contexto, a entidade excluída normalmente deixa de ser rastreada.

### Remove executa imediatamente um DELETE SQL?

Não no fluxo apresentado.

Remove altera o estado da entidade no Change Tracker.

A instrução de exclusão será enviada ao banco durante SaveChangesAsync, chamado posteriormente pelo Unit of Work.

### Remove possui uma versão RemoveAsync?

Não há um RemoveAsync correspondente no DbSet do EF Core para essa operação de marcação de estado.

A operação normal é síncrona porque não precisa realizar a exclusão no banco naquele momento.

### Correção técnica sobre o tipo de retorno

Na fala, o professor descreve Remove como uma função void.

Na API do EF Core, DbSet<TEntity>.Remove(TEntity) retorna EntityEntry<TEntity>, que fornece acesso a informações de rastreamento da entidade.

O código pode ignorar esse retorno:

```csharp
_dbContext.Expenses.Remove(result);
```

O método Delete do repositório continua sendo Task<bool> porque realiza uma consulta assíncrona e produz um resultado booleano.

---

## 12. AsNoTracking e exclusão de entidades

O professor retoma a aula 110, em que AsNoTracking foi utilizado nas consultas somente de leitura.

### Escolha da implementação da aula

A consulta interna de Delete não inclui AsNoTracking:

```csharp
var result = await _dbContext.Expenses
    .FirstOrDefaultAsync(expense => expense.Id == id);
```

Com o comportamento padrão de rastreamento utilizado no curso, a entidade existente é acompanhada pelo DbContext.

O repositório pode então marcá-la para exclusão.

Essa é uma estratégia válida, simples e adequada ao código mostrado.

### Correção técnica importante

O professor afirma que não seria possível passar uma entidade obtida com AsNoTracking para Remove sem gerar um erro em tempo de execução.

**Essa afirmação é excessivamente ampla.**

Remove pode começar a rastrear uma entidade anteriormente desconectada e marcá-la para exclusão. Portanto, obter uma entidade com AsNoTracking não torna sua remoção automaticamente inválida.

Exemplo complementar:

```csharp
var expense = await _dbContext.Expenses
    .AsNoTracking()
    .FirstOrDefaultAsync(e => e.Id == id);

if (expense is not null)
{
    _dbContext.Expenses.Remove(expense);
    await _dbContext.SaveChangesAsync();
}
```

Esse fluxo pode funcionar normalmente para uma entidade existente, desde que a chave e as demais condições de rastreamento sejam apropriadas.

### Quando podem ocorrer problemas?

- Já existe outra instância da mesma entidade e chave sendo rastreada pelo contexto.
- O objeto não possui uma chave válida para identificar o registro.
- Há problemas de concorrência, relacionamentos ou restrições do banco.
- A configuração ou o estado do contexto é incompatível com a operação pretendida.

### Diferença entre modificar e remover

Uma alteração simples de propriedade em uma entidade desconectada não é detectada automaticamente pelo contexto.

Já Remove é uma operação explícita que comunica ao contexto a intenção de excluir.

É justamente essa distinção que impede generalizar a explicação da aula 110 para afirmar que toda operação sobre uma entidade sem rastreamento falhará.

### Conclusão para o CashFlow

O professor mantém uma busca com rastreamento dentro do repositório de escrita. Essa escolha permanece válida, embora não seja a única maneira tecnicamente possível de excluir uma entidade.

---

## 13. Unit of Work e a persistência da remoção

O professor mantém a arquitetura de persistência estabelecida no módulo.

### No repositório

```csharp
_dbContext.Expenses.Remove(result);
return true;
```

### No caso de uso

```csharp
await _unitOfWork.Commit();
```

### Implementação já existente do Unit of Work

```csharp
public async Task Commit()
{
    await _dbContext.SaveChangesAsync();
}
```

Esse método foi introduzido na aula 103 e refatorado para programação assíncrona na aula 106.

### Por que não salvar dentro de Delete?

Na arquitetura do curso, o caso de uso controla o momento de confirmar as alterações.

Isso mantém separadas as responsabilidades:

- Repository: preparar operações sobre as entidades.
- Unit of Work: persistir as alterações pendentes.
- Application: coordenar o fluxo.

### Dependência do mesmo DbContext

Para que o Unit of Work persista a remoção preparada pelo repositório, ambos devem operar sobre o mesmo contexto associado ao escopo da operação.

O projeto utiliza injeção de dependência com DbContext e serviços Scoped, conforme aulas anteriores.

### Se a despesa não for encontrada

O repositório devolve false.

O use case lança NotFoundException antes de chamar Commit.

Não há confirmação de exclusão nesse caminho.

---

## 14. Tratamento de NotFoundException

O novo caso de uso reaproveita a exceção criada na aula 112.

```csharp
if (result == false)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);
```

O valor false é interpretado como ausência da despesa no contrato estabelecido para Delete.

A exceção informa ao filtro da API o erro correspondente.

### Integração com a aula 114

O filtro já foi refatorado para utilizar a abstração CashFlowException.

```csharp
public override int StatusCode =>
    (int)HttpStatusCode.NotFound;

public override List<string> GetErrors() =>
    new List<string> { Message };
```

Por isso, o endpoint DELETE não precisa de um tratamento de exceção exclusivo para recurso inexistente.

O filtro genérico utiliza o status e as mensagens fornecidos pela NotFoundException.

```text
Delete retorna false
        |
        v
NotFoundException
        |
        v
CashFlowException.StatusCode / GetErrors()
        |
        v
ResponseErrorJson
        |
        v
HTTP 404
```

### Teste versus contrato

O caminho de 404 é descrito pelo código e reutiliza o tratamento já validado nas aulas anteriores.

A gravação atual não demonstra um novo DELETE com ID inexistente retornando 404. O teste final realizado é a exclusão bem-sucedida do ID 33.

---

## 15. Contexto de autorização e propriedade dos dados

O professor destaca que, nesse estágio do CashFlow, as despesas não estão associadas a usuários distintos.

Por isso, a busca e a marcação para exclusão podem ocorrer dentro de Delete(id).

### Em um sistema multiusuário

A simples existência do identificador não deveria ser suficiente para autorizar a exclusão.

Seria necessário verificar se a pessoa autenticada possui permissão sobre aquela despesa.

O professor menciona a possibilidade de recuperar a entidade e executar validações adicionais no caso de uso.

### Exemplo conceitual — não implementado

```text
Identidade do usuário autenticado
              |
              v
Buscar a despesa solicitada
              |
              v
Verificar existência e autorização
              |
              v
Preparar exclusão autorizada
              |
              v
Commit
```

Uma alternativa seria incluir a propriedade do recurso no filtro de consulta ou na própria operação de exclusão, conforme a arquitetura e a política de segurança.

### Observação técnica

Conhecer ou adivinhar um ID não comprova que o consumidor tem autorização para acessar ou excluir o recurso.

Em aplicações multiusuário, a autorização em nível de objeto é necessária para evitar exclusões indevidas.

A aula não implementa autenticação, autorização ou associação da entidade Expense a usuários. Esse cenário permanece hipotético.

---

## 16. Registro do use case na DI

O professor inicialmente esquece de registrar IDeleteExpenseUseCase.

Ao executar o endpoint, o container não encontra um serviço correspondente à interface exigida pela action.

### Resultado observado

HTTP 500 com a resposta de erro desconhecido.

O depurador mostra que a exceção não é do tipo CashFlowException.

O filtro, portanto, encaminha o erro ao tratamento de exceções inesperadas.

A mensagem interna indica que IDeleteExpenseUseCase não foi registrado.

### Correção

No projeto Application, dentro de AddUseCases, o professor acrescenta:

```csharp
services.AddScoped<
    IDeleteExpenseUseCase,
    DeleteExpenseUseCase>();
```

### Por que a API compila, mas falha ao executar a action?

A existência da classe e da interface é suficiente para o compilador reconhecer os tipos.

Porém, a resolução da implementação solicitada por FromServices depende da configuração do container em tempo de execução.

A falta do registro pode aparecer apenas quando o serviço é solicitado, dependendo da configuração de validação da DI e do caminho de execução.

### Checklist de diagnóstico

1. Verificar se a interface foi registrada.
2. Verificar se a implementação correta foi vinculada ao contrato.
3. Confirmar que AddApplication() é executado na inicialização.
4. Confirmar que as dependências do construtor também podem ser resolvidas.

Neste teste, o problema confirmado pelo professor é especificamente a ausência do registro do caso de uso.

### Segurança das mensagens

A mensagem técnica de DI é examinada no debugger.

O filtro devolve um erro desconhecido ao consumidor da API, em vez de expor indiscriminadamente detalhes internos de implementação.

---

## 17. Código consolidado por camada

Os trechos a seguir preservam a estrutura da aula. Os namespaces, nomes locais e assinaturas de métodos auxiliares podem precisar de ajustes conforme os arquivos originais.

### 17.1 Domain — IExpensesWriteOnlyRepository

```csharp
public interface IExpensesWriteOnlyRepository
{
    Task Add(Expense expense);

    /// <summary>
    /// Finds an expense and marks it for deletion.
    /// </summary>
    /// <param name="id">The expense identifier.</param>
    /// <returns>
    /// True if the expense was found and marked for deletion;
    /// otherwise, false.
    /// </returns>
    Task<bool> Delete(long id);
}
```

A documentação foi aprimorada didaticamente para refletir que a persistência ocorre posteriormente no Commit.

### 17.2 Infrastructure — ExpensesRepository

```csharp
using Microsoft.EntityFrameworkCore;

// Dentro de ExpensesRepository:
public async Task<bool> Delete(long id)
{
    var result = await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);

    if (result is null)
        return false;

    _dbContext.Expenses.Remove(result);

    return true;
}
```

A classe permanece implementando IExpensesReadOnlyRepository e IExpensesWriteOnlyRepository.

Os métodos Add, GetAll e GetById continuam como anteriormente.

### 17.3 Application — IDeleteExpenseUseCase

```csharp
public interface IDeleteExpenseUseCase
{
    Task Execute(long id);
}
```

### 17.4 Application — DeleteExpenseUseCase

```csharp
public class DeleteExpenseUseCase : IDeleteExpenseUseCase
{
    private readonly IExpensesWriteOnlyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseUseCase(
        IExpensesWriteOnlyRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Execute(long id)
    {
        var result = await _repository.Delete(id);

        if (!result)
            throw new NotFoundException(
                ResourceErrorMessages.ExpenseNotFound);

        await _unitOfWork.Commit();
    }
}
```

A forma !result é equivalente à comparação result == false para um bool não anulável.

### 17.5 Application — DependencyInjectionExtension

```csharp
// Dentro de AddUseCases:
services.AddScoped<
    IDeleteExpenseUseCase,
    DeleteExpenseUseCase>();
```

O registro da interface de escrita na Infrastructure já existe desde a aula 113.

### 17.6 API — Action DELETE

```csharp
[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> Delete(
    [FromServices] IDeleteExpenseUseCase useCase,
    [FromRoute] long id)
{
    await useCase.Execute(id);

    return NoContent();
}
```

Não é necessário modificar o filtro para acrescentar outro ramo específico de NotFoundException. O tratamento genérico da aula 114 permanece.

---

## 18. Fluxo completo da exclusão

### Caminho de sucesso

```text
DELETE com ID na rota
          |
          v
Controller
          |
          | await Execute(id)
          v
DeleteExpenseUseCase
          |
          | await Delete(id)
          v
ExpensesRepository
          |
          | FirstOrDefaultAsync(e => e.Id == id)
          v
Expense encontrada
          |
          | Remove(expense)
          v
Entidade marcada como Deleted
          |
          | return true
          v
DeleteExpenseUseCase
          |
          | await UnitOfWork.Commit()
          v
SaveChangesAsync()
          |
          v
Remoção persistida no MySQL
          |
          v
Controller: NoContent()
          |
          v
HTTP 204
```

### Caminho de ausência

```text
DELETE com ID inexistente
          |
          v
FirstOrDefaultAsync retorna null
          |
          v
Delete retorna false
          |
          v
NotFoundException
          |
          v
Filtro de exceções
          |
          v
HTTP 404 + ResponseErrorJson
```

### Caminho do erro demonstrado de DI

```text
Controller solicita IDeleteExpenseUseCase
          |
          v
Serviço não registrado
          |
          v
Falha de resolução de dependência
          |
          v
Exceção inesperada
          |
          v
Filtro de exceções: fallback
          |
          v
HTTP 500
```

O terceiro caminho é corrigido pelo registro Scoped do caso de uso.

---

## 19. Testes no Swagger e no banco

### Primeiro teste — antes do registro de DI

O professor executa a API, abre o Swagger e tenta excluir a despesa de ID 33.

| Informação | Resultado |
|---|---|
| ID solicitado | 33 |
| Operação | DELETE |
| Status | 500 |
| Causa investigada | IDeleteExpenseUseCase não registrado |

Ele executa novamente com um breakpoint no filtro de exceções e verifica a mensagem interna da falha.

Em seguida, registra o caso de uso na DI.

### Segundo teste — após a correção

| Informação | Resultado |
|---|---|
| ID solicitado | 33 |
| Título encontrado | Pizza |
| Resultado da busca | Entidade não nula |
| Resultado de Delete | true |
| Commit | Concluído |
| Status HTTP | 204 No Content |

O teste confirma que Pizza, registrada na aula 113, possuía ID 33 na base utilizada pelo instrutor.

### Verificação no MySQL

O professor mostra que o resultado de uma consulta SELECT anterior ainda exibia a despesa.

Ao executar novamente o SELECT, a despesa de ID 33 desaparece.

A última despesa exibida passa a ser Mouse, já identificada na aula 113 como ID 32.

Essa verificação confirma que a remoção foi persistida no banco, e não apenas representada por um retorno HTTP de sucesso.

### O que não foi demonstrado?

- DELETE com identificador inexistente após a correção.
- Exclusão de uma despesa pertencente a outro usuário.
- Testes automatizados.
- Benchmark ou análise de desempenho.
- Exclusão em cascata ou concorrência entre requisições.

---

## 20. Boas práticas, limites e alternativas

Os itens abaixo aprofundam tecnicamente os conceitos da gravação, sem atribuir implementações adicionais ao instrutor.

### 20.1 Semântica clara do booleano

O retorno true deve comunicar exatamente o que o método garante.

Como o commit ocorre fora de Delete, é mais preciso documentar o resultado como "entidade encontrada e marcada para exclusão".

Erros de infraestrutura não deveriam ser transformados indiscriminadamente em false, porque false tem a semântica de ausência do registro nessa implementação.

### 20.2 DELETE é idempotente quanto ao efeito pretendido

Uma repetição de DELETE sobre o mesmo recurso pode produzir status diferentes: a primeira operação retorna 204 e uma repetição posterior pode retornar 404.

Isso não contradiz a idempotência do método HTTP: o efeito pretendido sobre o estado do recurso continua sendo sua ausência.

O professor não testa essa repetição durante a aula.

### 20.3 Concorrência

Uma despesa pode existir no momento da consulta e ser excluída por outra operação antes de Commit.

Nessa situação, SaveChangesAsync pode falhar, por exemplo com uma exceção de concorrência do EF Core, conforme o provider e a operação.

O retorno booleano anterior não elimina esse risco.

Nenhum tratamento específico de concorrência é implementado nesta gravação.

### 20.4 Exclusão direta com ExecuteDeleteAsync

Como alternativa técnica, versões do EF Core a partir da 7 disponibilizam ExecuteDeleteAsync para executar exclusões diretamente com uma consulta.

```csharp
var affectedRows = await _dbContext.Expenses
    .Where(e => e.Id == id)
    .ExecuteDeleteAsync();
```

Essa operação não utiliza o Change Tracker para marcar cada entidade e executa a alteração imediatamente, sem esperar SaveChangesAsync.

Por isso, **não deve ser substituída mecanicamente** no código do curso: mudaria a semântica do Unit of Work adotado e exigiria avaliar transações, concorrência e integração com as demais operações.

O professor não utiliza ExecuteDeleteAsync.

### 20.5 FindAsync é uma opção legítima

A escolha de FirstOrDefaultAsync é válida, mas a inexistência de uma chave não torna FindAsync inadequado. Ambos podem fornecer null no cenário de ausência.

### 20.6 Consulta rastreada não é requisito absoluto para Remove

Uma entidade sem rastreamento pode ser explicitamente associada pelo método Remove, observadas as condições descritas anteriormente.

A escolha de usar uma consulta rastreada internamente é uma opção de design da aula.

### 20.7 Separação de responsabilidades

A Application não precisa conhecer EntityState.Deleted ou DbSet.Remove.

Ela depende de IExpensesWriteOnlyRepository e de IUnitOfWork, mantendo os detalhes de persistência encapsulados na Infrastructure.

### 20.8 Segregação de interfaces

A inclusão de Delete modifica a interface de escrita, mas não exige acrescentar uma operação de exclusão à interface de leitura.

Essa é uma aplicação prática da decisão tomada na aula 113.

### 20.9 Open/Closed Principle

A NotFoundException já fornece o status e as mensagens necessários ao filtro genérico.

O novo endpoint reutiliza o comportamento existente, sem precisar acrescentar um tratamento específico para cada caso de uso ao filtro.

### 20.10 Documentação não substitui testes

Os comentários XML esclarecem o contrato de métodos.

Os atributos ProducesResponseType descrevem o contrato HTTP.

O teste no Swagger verifica a integração em execução.

São mecanismos complementares, não equivalentes.

---

## 21. Exercícios e perguntas de revisão

### Exercício 1 — declarar o método do repositório

Qual assinatura representa uma exclusão assíncrona por ID com retorno booleano?

```csharp
Task<bool> Delete(long id);
```

### Exercício 2 — implementar a busca e remoção

```csharp
public async Task<bool> Delete(long id)
{
    var result = await _dbContext.Expenses
        .FirstOrDefaultAsync(expense => expense.Id == id);

    if (result is null)
        return false;

    _dbContext.Expenses.Remove(result);

    return true;
}
```

### Exercício 3 — identificar a persistência

A chamada Remove já exclui o registro do banco?

**Resposta:** não no fluxo apresentado. Ela prepara a exclusão no Change Tracker. O Unit of Work chama SaveChangesAsync para persistir a mudança.

### Exercício 4 — ausência da despesa

O que acontece quando Delete devolve false?

**Resposta:** o caso de uso lança NotFoundException. O filtro traduz essa exceção em HTTP 404.

### Exercício 5 — concluir a operação

```csharp
await useCase.Execute(id);
return NoContent();
```

O status de sucesso é 204 e a resposta não possui corpo.

### Exercício 6 — registrar o caso de uso

```csharp
services.AddScoped<IDeleteExpenseUseCase, DeleteExpenseUseCase>();
```

### Exercício 7 — verificar a afirmação sobre FindAsync

FindAsync lança uma exceção apenas porque a chave não foi encontrada?

**Resposta:** não. Para uma busca válida por chave primária sem correspondência, retorna null.

### Exercício 8 — entidade sem rastreamento

Remove pode receber uma entidade desconectada?

**Resposta:** sim, em condições apropriadas. O método pode começar a rastreá-la para marcá-la como Deleted. A presença de outra instância com a mesma chave no contexto pode causar conflito.

### Exercício 9 — documentação XML

Qual estrutura documenta o retorno booleano?

```csharp
/// <returns>
/// True if the expense was found and marked for deletion;
/// otherwise, false.
/// </returns>
Task<bool> Delete(long id);
```

### Perguntas de revisão

1. Qual é a finalidade do endpoint DELETE?
2. De onde vem o identificador recebido pela action?
3. Quais respostas de negócio o endpoint documenta?
4. Por que o professor escolhe HTTP 204?
5. Qual tipo de retorno possui Execute?
6. Quais dependências o DeleteExpenseUseCase recebe?
7. Por que não é necessário IMapper?
8. Por que a exclusão pertence ao repositório de escrita?
9. Qual tipo de retorno possui Delete no repositório?
10. O que true garante antes do commit?
11. O que false representa?
12. Qual sintaxe inicia um comentário de documentação XML?
13. Onde a documentação escrita é exibida pelo Visual Studio?
14. Qual método de consulta é utilizado na versão final?
15. Qual a diferença entre FindAsync e FirstOrDefaultAsync?
16. Qual é o efeito de Remove no Change Tracker?
17. Qual é o tipo retornado por DbSet.Remove?
18. Por que não existe a necessidade de um RemoveAsync para marcar o estado?
19. AsNoTracking torna impossível uma exclusão explícita?
20. Qual componente confirma a remoção no banco?
21. Como o projeto produz HTTP 404 sem um novo ramo no filtro?
22. Qual erro ocorre inicialmente durante o teste?
23. Qual registro de DI corrige o problema?
24. Qual despesa é excluída no teste?
25. Qual ID ela possui?
26. Como o professor confirma a persistência no banco?
27. Qual teste de ausência não é executado nesta gravação?
28. Por que uma aplicação multiusuário precisaria de verificações adicionais?

---

## 22. Resumo e mapa mental

| Componente | Alteração |
|---|---|
| API | Action DELETE com ID na rota |
| Resposta de sucesso | HTTP 204 sem corpo |
| Resposta de ausência | HTTP 404 com ResponseErrorJson |
| Domain | Task<bool> Delete(long id) no contrato de escrita |
| Documentação | Comentários XML sobre o significado do retorno |
| Infrastructure | FirstOrDefaultAsync, verificação de null e Remove |
| Application | DeleteExpenseUseCase com repositório e Unit of Work |
| DI | Registro Scoped do novo caso de uso |
| Filtro | Reutilização de NotFoundException e do tratamento polimórfico |
| Teste inicial | HTTP 500 por serviço não registrado |
| Teste final | Exclusão de Pizza, ID 33, HTTP 204 |
| Banco | SELECT posterior confirma ausência do ID 33 |

```text
                       AULA 115
                    DELETE DE DESPESA
                           |
                     ID pela rota
                           |
                  DeleteExpenseUseCase
                           |
              IExpensesWriteOnlyRepository
                           |
                       Delete(id)
                           |
                 FirstOrDefaultAsync
                           |
               +-----------+-----------+
               |                       |
             null                   Expense
               |                       |
             false                   Remove()
               |                       |
       NotFoundException              true
               |                       |
      Exception Filter          UnitOfWork.Commit()
               |                       |
            HTTP 404            SaveChangesAsync()
                                       |
                                    MySQL
                                       |
                                    HTTP 204

    DI: IDeleteExpenseUseCase -> DeleteExpenseUseCase

    Teste: Pizza, ID 33 -> removida do banco

    Precisões:
    FindAsync pode retornar null.
    Remove pode associar uma entidade desconectada.
    True antes de Commit não garante persistência.
```

### Referências técnicas complementares

- [Microsoft Learn — Entity Framework Core: Tracking vs. No-Tracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)
- [Microsoft Learn — DbSet<TEntity>.FindAsync](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.findasync)
- [Microsoft Learn — DbSet<TEntity>.Remove](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1.remove)
- [Microsoft Learn — Basic SaveChanges](https://learn.microsoft.com/en-us/ef/core/saving/basic)
- [Microsoft Learn — Documentação XML em C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)

**Conclusão:** a aula 115 implementa a exclusão de despesas pelo identificador, reutilizando o repositório de escrita, o Unit of Work e as exceções customizadas. O repositório verifica a existência da entidade e prepara sua exclusão; a Application decide se deve lançar NotFoundException ou confirmar a operação; a API devolve HTTP 204 quando o fluxo termina com sucesso. A demonstração do HTTP 500 causado por DI ausente e a confirmação posterior da remoção de Pizza no MySQL completam a validação prática do endpoint. As correções técnicas deste material preservam o código escolhido pelo professor, sem reproduzir como regras gerais as afirmações incorretas sobre FindAsync e Remove.
