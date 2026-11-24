# Aula 110 — AsNoTracking, Change Tracker e otimização de consultas no EF Core

> Continuação da aula 109: o endpoint GET de despesas já utiliza ToListAsync e AutoMapper. Nesta aula, o professor acrescenta AsNoTracking() ao GetAll() do ExpensesRepository para evitar rastreamento desnecessário em uma operação somente de leitura.
>
> A gravação não apresenta benchmark, novo teste no Swagger ou alteração de contrato HTTP. Os exemplos adicionais são complementos técnicos.

## 1. Objetivo

A listagem recupera despesas, mapeia entidades em DTOs resumidos e devolve os resultados. Ela não modifica as entidades para persistir alterações. Portanto, o rastreamento padrão do EF Core pode ser dispensado.

## 2. Pergunta que orienta a decisão

O professor recomenda perguntar: o caso de uso precisa alterar as entidades retornadas e persistir essas mudanças por meio do contexto?

- Se não, considerar AsNoTracking().
- Se sim, preservar o rastreamento quando a estratégia de atualização depender dele.

A decisão depende da finalidade da consulta e da estratégia de persistência, não simplesmente do verbo HTTP.

## 3. Antes e depois

Antes:

```csharp
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses.ToListAsync();
}
```

Depois:

```csharp
public async Task<List<Expense>> GetAll()
{
    return await _dbContext.Expenses
        .AsNoTracking()
        .ToListAsync();
}
```

O método de extensão pertence ao namespace Microsoft.EntityFrameworkCore. A assinatura foi reconstruída conforme a aula anterior. A única alteração funcional demonstrada é a inclusão de AsNoTracking().

## 4. Rastreamento padrão do EF Core

Por padrão, consultas de entidades com chave são rastreadas, salvo configuração diferente. O contexto acompanha as instâncias e pode detectar mudanças para persistência posterior. Isso permite carregar um objeto, modificar suas propriedades e chamar SaveChangesAsync().

## 5. Change Tracker

O Change Tracker mantém informações de estado e identidade das entidades associadas ao DbContext.

| Estado | Significado |
|---|---|
| Detached | Não rastreada |
| Unchanged | Rastreada, sem alterações detectadas |
| Added | Marcada para inserção |
| Modified | Marcada para atualização |
| Deleted | Marcada para exclusão |

Esses estados são um complemento técnico, não uma lista apresentada no vídeo.

## 6. O que o professor chama de cache

A palavra cache é utilizada como analogia para as informações mantidas pelo contexto. Isso não equivale a um cache geral de resultados SQL. Uma consulta LINQ rastreada pode executar SQL novamente e reutilizar instâncias já rastreadas. Na estratégia comum de snapshots, o EF Core compara valores para detectar mudanças; não é necessário imaginar uma thread monitorando continuamente todas as propriedades.

## 7. Como alterações são persistidas

Exemplo complementar:

```csharp
var expense = await _dbContext.Expenses
    .FirstAsync(e => e.Id == id);
expense.Title = "Novo título";
await _dbContext.SaveChangesAsync();
```

Com rastreamento habilitado, o EF Core pode detectar a mudança e gerar um UPDATE. O SQL concreto depende do provider e da configuração. Essa atualização não foi implementada na aula.

## 8. Como AsNoTracking funciona

O método configura a consulta para não associar as entidades materializadas ao Change Tracker do contexto. Os objetos continuam existindo e podem ser lidos, mapeados e alterados em memória. O contexto não acompanha automaticamente essas instâncias para persistência.

AsNoTracking não desanexa entidades previamente rastreadas nem desabilita todas as demais consultas do contexto.

## 9. Comparação

| Característica | Tracking | AsNoTracking |
|---|---|---|
| Materializa entidades | Sim | Sim |
| Rastreia os resultados no contexto | Sim | Não |
| Detecta alterações para persistência automática | Sim, conforme configuração | Não, enquanto desconectadas |
| Resolução de identidade do contexto | Sim | Não no modo padrão |
| Uso típico | Leitura com atualização posterior | Leitura sem atualização |

## 10. Aplicação no CashFlow

O caso de uso continua executando a consulta e o mapeamento:

```csharp
var result = await _repository.GetAll();
return new ResponseExpensesJson
{
    Expenses = _mapper.Map<List<ResponseShortExpenseJson>>(result)
};
```

Não precisa de UnitOfWork.Commit(), pois não realiza escrita. O controller mantém os retornos HTTP 200 e 204. O contrato IExpensesRepository.GetAll() continua retornando Task<List<Expense>>.

## 11. Ganhos esperados

O professor destaca menor consumo de memória e melhor desempenho. A ausência de rastreamento pode reduzir estruturas de acompanhamento, snapshots e trabalho de materialização.

**Precisão:** a otimização ocorre principalmente no EF Core, na aplicação. O SQL de uma consulta simples pode permanecer equivalente e o MySQL não necessariamente executará o SELECT mais rapidamente. Não há percentual garantido de ganho. A gravação não apresenta medições. Em alguns cenários, a resolução de identidade das consultas rastreadas pode reduzir alocações redundantes.

## 12. Ordem dos métodos

```csharp
var expenses = await _dbContext.Expenses
    .AsNoTracking()
    .Where(e => e.Amount > 0)
    .ToListAsync();
```

O Where acima é apenas ilustrativo. AsNoTracking também pode aparecer depois de Where, desde que configure a consulta antes da materialização e não seja sobrescrito. ToListAsync executa e materializa a consulta; AsNoTracking apenas define seu modo de rastreamento. A aula não adiciona filtros.

## 13. O que AsNoTracking não faz

Não torna entidades imutáveis, não filtra registros, não seleciona menos colunas, não pagina, não cria índices e não substitui ToListAsync ou AutoMapper. Também não impede atualizações explícitas posteriores.

## 14. Leitura versus atualização

Uma consulta para exibir detalhes pode usar AsNoTracking. Uma consulta destinada a modificar uma entidade e salvar via detecção automática de mudanças normalmente deve preservar o tracking.

Exemplo complementar de entidade não rastreada:

```csharp
var expense = await _dbContext.Expenses
    .AsNoTracking()
    .FirstAsync(e => e.Id == id);
expense.Title = "Novo título";
await _dbContext.SaveChangesAsync();
```

Se a instância permanecer desconectada, essa alteração não será automaticamente persistida. O objeto em memória foi alterado, mas não está sendo acompanhado pelo contexto.

## 15. Atualização explícita

É possível associar uma entidade desconectada ao contexto e marcar uma propriedade como modificada:

```csharp
_dbContext.Attach(expense);
_dbContext.Entry(expense)
    .Property(e => e.Title)
    .IsModified = true;
await _dbContext.SaveChangesAsync();
```

O exemplo pressupõe que não exista outra instância com a mesma chave rastreada. É necessário considerar autorização, validação e concorrência. Update pode marcar várias propriedades ou entidades relacionadas; não deve ser aplicado indiscriminadamente. Nenhuma dessas estratégias foi demonstrada na aula.

## 16. Resolução de identidade

Em consultas rastreadas, o contexto pode reutilizar a mesma instância para uma mesma chave. AsNoTracking padrão não realiza essa resolução de identidade. Quando necessário, existe AsNoTrackingWithIdentityResolution(), que utiliza rastreamento temporário para resolver identidades sem associar os resultados ao contexto principal.

Trata-se de complemento técnico, não de uma alteração do CashFlow nesta aula.

## 17. Arquitetura e responsabilidades

- Domain: mantém o contrato do repositório.
- Infrastructure: configura AsNoTracking e materializa a consulta.
- Application: consulta e mapeia os resultados.
- API: define a resposta HTTP.
- DI: permanece inalterada.
- Unit of Work: continua responsável pela persistência nos casos de escrita.

A otimização fica encapsulada no repositório, sem expor detalhes do EF Core ao caso de uso.

## 18. Erros comuns

1. Usar AsNoTracking em consulta de atualização e presumir que SaveChanges detectará mudanças.
2. Confundir Change Tracker com cache de resultados SQL.
3. Acreditar que AsNoTracking reduz automaticamente as colunas selecionadas.
4. Aplicar AsNoTracking após materializar a consulta.
5. Presumir ganho fixo de desempenho sem benchmark.
6. Acreditar que os objetos retornados não podem ser modificados.
7. Confundir ausência de tracking com ausência de DbContext.

## 19. Exercícios com respostas

**1. Como otimizar GetAll para leitura?**

```csharp
return await _dbContext.Expenses.AsNoTracking().ToListAsync();
```

**2. AsNoTracking executa o SQL?** Não. ToListAsync materializa a consulta.

**3. O método modifica o contrato HTTP?** Não. A resposta permanece 200 ou 204 conforme a aula anterior.

**4. Uma entidade sem rastreamento é imutável?** Não. Pode ser alterada em memória.

**5. SaveChangesAsync salva automaticamente uma alteração em uma entidade desconectada?** Não, enquanto ela não for associada ao contexto por uma estratégia apropriada.

**6. É obrigatório chamar AsNoTracking antes de Where?** Não. O requisito relevante é configurar o comportamento antes da execução da consulta.

**7. O SELECT necessariamente fica mais rápido no servidor MySQL?** Não. A principal redução de trabalho está no EF Core.

**8. Houve benchmark na aula?** Não.

## 20. Resumo e mapa mental

```text
GetAll() — consulta somente leitura
    |
    v
ExpensesRepository
    |
    v
AsNoTracking()
    |
    v
ToListAsync()
    |
    v
List<Expense> sem rastreamento
    |
    v
AutoMapper -> DTOs -> HTTP 200/204

Benefícios potenciais:
    menor overhead de rastreamento
    menor consumo de memória

Não altera:
    contrato do repositório
    formato da resposta
    SQL necessariamente
    regras de negócio
```

**Conclusão:** utilizar AsNoTracking quando não for necessário acompanhar alterações nas entidades consultadas é uma otimização apropriada para o GetAll do CashFlow. A mudança preserva a consulta assíncrona e a arquitetura desenvolvida anteriormente.
