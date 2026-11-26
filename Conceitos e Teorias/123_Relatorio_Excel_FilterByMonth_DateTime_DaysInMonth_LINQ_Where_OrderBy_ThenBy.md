# Aula 123 — Relatório Excel (parte 6): FilterByMonth, intervalos de datas, Where, OrderBy e ThenBy

> **Contexto:** na aula 122, o CashFlow gerou e entregou um XLSX válido contendo os cabeçalhos. Agora o professor interrompe a construção visual do documento para implementar a consulta das despesas que deverão preenchê-lo.
>
> **Objetivo:** criar uma operação de leitura reutilizável para os relatórios Excel e PDF, filtrando as despesas por ano e mês e ordenando-as cronologicamente.
>
> **Escopo:** o instrutor prepara dados no MySQL, injeta o repositório no caso de uso, implementa FilterByMonth e chama o método no início de Execute. O teste da consulta e a discussão sobre a responsabilidade pela construção do intervalo ficam para a próxima aula.
>
> **Fidelidade:** os trechos foram reconstruídos a partir da transcrição. A implementação principal preserva o limite final inclusivo às 23:59:59, utilizado pelo professor. As limitações dessa estratégia, a alternativa de intervalo semiaberto e a ordenação determinística são complementos técnicos, não alterações demonstradas na aula. A utilização de ThenBy por título é explicada pelo instrutor; a transcrição não permite afirmar com certeza se ele manteve essa chamada no código final.

## Índice

1. Recapitulação e objetivo
2. Preparação do banco para testes
3. Injeção do repositório no caso de uso
4. Novo contrato de leitura FilterByMonth
5. Implementação na Infrastructure
6. AsNoTracking em consultas de relatório
7. LINQ, IQueryable e execução da consulta
8. DateOnly versus DateTime no filtro
9. Construção da data inicial
10. DateTime.Date e parâmetros nomeados
11. DateTime.DaysInMonth e anos bissextos
12. Construção da data final
13. Where e expressão lambda
14. Ordenação com OrderBy e OrderByDescending
15. Ordenação secundária com ThenBy
16. Código consolidado e integração
17. Precisões técnicas: intervalos, SQL e responsabilidades
18. Exercícios e revisão
19. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

O módulo de relatórios possui a seguinte estrutura após as aulas anteriores:

| Aula | Etapa concluída |
|---|---|
| 118 | Endpoint de download com File() |
| 119 | Recebimento do período com FromHeader e DateOnly |
| 120 | Instalação do ClosedXML e criação do workbook |
| 121 | Cabeçalhos traduzidos e estilizados |
| 122 | Serialização com MemoryStream e download do Excel |
| 123 | Implementação da consulta mensal de despesas |

O arquivo gerado até a aula 122 continha somente os cabeçalhos.

Nesta aula, o professor prepara a obtenção dos dados reais.

O filtro precisa utilizar o período solicitado pelo consumidor da API, recuperando somente as despesas pertencentes ao mês e ao ano recebidos.

A consulta deverá ser aproveitada tanto pelo relatório Excel quanto pelo futuro relatório PDF.

```text
ReportController
       |
       v
Use case de Excel / futuro use case de PDF
       |
       v
IExpensesReadOnlyRepository.FilterByMonth
       |
       v
ExpensesRepository
       |
       v
MySQL: despesas do período, ordenadas
```

Ainda não ocorre a escrita dessas despesas nas células.

## 2. Preparação do banco para testes

Antes da gravação, o professor executa uma instrução SQL para inserir despesas em diferentes meses.

Os registros contêm as informações já conhecidas da entidade Expense:

- Título.
- Descrição.
- Data e hora.
- Valor.
- Tipo de pagamento.

O objetivo é possuir uma massa de dados que permita distinguir registros pertencentes ao período solicitado de registros pertencentes a outros períodos.

A transcrição não informa a instrução SQL literal, os IDs, os valores de todas as despesas nem a quantidade exata de registros inseridos.

Não se deve reconstruir uma massa de dados fictícia como se fosse aquela efetivamente utilizada pelo instrutor.

### Exemplo didático de dados de teste

| Título | Data e hora | Pertence a março de 2024? |
|---|---|---|
| Mercado | 2024-02-29 23:59:59 | Não |
| Padaria | 2024-03-01 08:00:00 | Sim |
| Farmácia | 2024-03-15 14:30:00 | Sim |
| Transporte | 2024-03-31 23:59:59 | Sim |
| Restaurante | 2024-04-01 00:00:00 | Não |

A tabela é ilustrativa, não uma reprodução do banco do professor.

## 3. Injeção do repositório no caso de uso

O professor apresenta um construtor previamente preparado no caso de uso de geração de relatório Excel.

Por meio de injeção de dependência, a classe recebe IExpensesReadOnlyRepository.

Exemplo reconstruído:

```csharp
private readonly IExpensesReadOnlyRepository _repository;

public GenerateExpensesReportExcelUseCase(
    IExpensesReadOnlyRepository repository)
{
    _repository = repository;
}
```

O nome completo da classe é ilustrativo, conforme registrado na aula 120. O contrato IExpensesReadOnlyRepository corresponde à estrutura estudada anteriormente no CashFlow.

### Por que utilizar a interface de leitura?

O relatório não precisa adicionar, alterar ou excluir despesas.

Seu caso de uso depende de um contrato que fornece operações de leitura.

Isso retoma o Interface Segregation Principle estudado na aula 113.

| Contrato | Finalidade |
|---|---|
| IExpensesReadOnlyRepository | Consultar despesas |
| IExpensesWriteOnlyRepository | Operações de escrita previstas pelo contrato |
| IExpensesUpdateOnlyRepository | Recuperar e atualizar despesas |

A nova consulta pertence ao contrato de leitura.

O registro de DI desse repositório já existia nas aulas anteriores. A gravação não mostra a criação de uma nova inscrição para FilterByMonth, pois o método é acrescentado à interface e implementação existentes.

## 4. Novo contrato de leitura FilterByMonth

O professor acrescenta uma operação ao repositório.

```csharp
Task<List<Expense>> FilterByMonth(DateOnly date);
```

### Interface consolidada

```csharp
public interface IExpensesReadOnlyRepository
{
    Task<List<Expense>> GetAll();

    Task<Expense?> GetById(long id);

    Task<List<Expense>> FilterByMonth(DateOnly date);
}
```

GetAll e GetById já pertenciam à interface.

FilterByMonth é o novo método.

### Entendendo o contrato

| Elemento | Significado |
|---|---|
| Task | Representa uma operação assíncrona |
| List<Expense> | Coleção de despesas recuperadas |
| FilterByMonth | Operação de consulta mensal |
| DateOnly date | Data de referência contendo ano, mês e dia |

A regra utiliza somente Year e Month do argumento. O dia recebido não define o início ou fim da consulta.

A expectativa é devolver uma lista de despesas do período.

Uma consulta sem registros materializada por ToListAsync devolve uma lista vazia, e não uma lista nula. Esse comportamento será útil para a futura decisão entre gerar o relatório e retornar HTTP 204.

A aula não testa essa condição.

## 5. Implementação na Infrastructure

Depois de acrescentar o método ao contrato, ExpensesRepository apresenta erro de compilação, pois ainda não fornece a implementação exigida.

O professor utiliza o recurso Show Potential Fixes do Visual Studio e seleciona a opção de implementar a interface.

A IDE cria a estrutura inicial do método.

A implementação é então substituída pela consulta que será construída durante a aula.

### Relação com interfaces

Acrescentar um membro abstrato a uma interface exige que suas implementações concretas forneçam esse membro, salvo situações específicas, como uma implementação padrão aplicável.

O erro observado é coerente com essa regra.

O mesmo ExpensesRepository que já implementava as operações de leitura, escrita e atualização receberá FilterByMonth.

## 6. AsNoTracking em consultas de relatório

O professor começa a consulta com:

```csharp
_dbContext.Expenses.AsNoTracking()
```

Ele relembra que a consulta pertence ao repositório de leitura e não pretende alterar as despesas carregadas.

AsNoTracking orienta o Entity Framework Core a não rastrear as entidades retornadas pela consulta.

### Relação com as aulas anteriores

Na aula 110, essa técnica foi utilizada para consultas que não precisam participar de atualizações pelo ChangeTracker.

O relatório tem essa característica: recupera dados e os utiliza na construção de documentos.

### O que AsNoTracking não faz?

- Não filtra os registros.
- Não ordena os resultados.
- Não torna as propriedades dos objetos imutáveis.
- Não garante uma redução específica do tempo de execução no MySQL.

Pode reduzir o trabalho de rastreamento e o consumo de recursos associados ao EF Core, especialmente em consultas de leitura.

A aula não apresenta benchmark de desempenho.

## 7. LINQ, IQueryable e execução da consulta

Inicialmente, o professor considera a estrutura:

```csharp
return await _dbContext.Expenses
    .AsNoTracking()
    .ToListAsync();
```

Essa consulta recuperaria todas as despesas disponíveis no DbSet, respeitando eventuais filtros globais já configurados.

Não é o resultado desejado.

### Composição da consulta

O objetivo é acrescentar:

```text
AsNoTracking
      |
    Where
      |
   OrderBy
      |
  ToListAsync
```

Where e OrderBy compõem a expressão da consulta. ToListAsync realiza sua execução e materializa os resultados em uma lista.

### Complemento técnico: execução diferida

As operações LINQ aplicadas ao IQueryable do EF Core normalmente constroem uma expressão que será traduzida pelo provider quando a consulta for executada.

Assim, o objetivo não é carregar todas as despesas em memória para depois filtrar e ordenar.

A filtragem e a ordenação são formuladas antes da materialização, permitindo que o provider relacional as traduza para uma consulta SQL compatível.

Não há captura de SQL ou plano de execução na gravação.

## 8. DateOnly versus DateTime no filtro

O parâmetro recebido pelo método é DateOnly.

A propriedade Date da despesa, por sua vez, é DateTime.

| Tipo | Informação |
|---|---|
| DateOnly | Ano, mês e dia |
| DateTime | Data e horário |

O professor explica que conservar o horário da despesa permite análises futuras, como identificar despesas ocorridas em diferentes períodos do dia.

A gravação não comprova que Date seja necessariamente um timestamp de criação gerado pelo servidor. O campo é a data e hora armazenada da despesa, e sua semântica deve acompanhar o contrato do projeto.

### Por que construir limites DateTime?

É necessário comparar a propriedade Date com valores temporalmente compatíveis.

O mês não começa na hora de uma despesa específica, mas no início do primeiro dia.

O dia recebido em DateOnly deve ser ignorado, pois a operação é mensal.

Por exemplo, 2024-03-01 e 2024-03-25 devem indicar março de 2024.

## 9. Construção da data inicial

O professor cria uma variável auxiliar denominada startDate.

```csharp
var startDate = new DateTime(
    year: date.Year,
    month: date.Month,
    day: 1).Date;
```

### O que cada componente representa?

| Argumento | Valor |
|---|---|
| year | Ano recebido |
| month | Mês recebido |
| day | 1, independentemente do dia recebido |

O resultado representa o primeiro dia do mês às 00:00:00.

### Exemplo

```csharp
var date = new DateOnly(2024, 3, 25);

var startDate = new DateTime(
    year: date.Year,
    month: date.Month,
    day: 1).Date;
```

Resultado conceitual:

```text
2024-03-01 00:00:00
```

O valor 25 não participa da construção do limite.

## 10. DateTime.Date e parâmetros nomeados

### Parâmetros nomeados

O professor utiliza nomes de argumentos para facilitar a leitura do construtor.

```csharp
new DateTime(
    year: date.Year,
    month: date.Month,
    day: 1)
```

A sintaxe year: indica qual parâmetro formal recebe o argumento fornecido.

É possível utilizar argumentos posicionais equivalentes:

```csharp
new DateTime(date.Year, date.Month, 1)
```

O resultado é o mesmo nesse caso.

### Propriedade Date

O instrutor destaca DateTime.Date.

```csharp
var original = new DateTime(2024, 3, 25, 14, 30, 45);

var dateAtMidnight = original.Date;
```

Resultado:

```text
original:       2024-03-25 14:30:45
dateAtMidnight: 2024-03-25 00:00:00
```

Date devolve outro valor DateTime com a mesma data e o horário zerado. Não altera o valor original e não devolve DateOnly.

### Precisão técnica

O construtor DateTime que recebe somente ano, mês e dia já inicializa o horário à meia-noite.

Por isso, o .Date utilizado no startDate é redundante nesse caso específico.

Sua presença é preservada no código principal porque faz parte da explicação e da implementação do professor.

## 11. DateTime.DaysInMonth e anos bissextos

Para construir o limite final, é necessário descobrir qual é o último dia do mês.

O professor utiliza o método estático:

```csharp
DateTime.DaysInMonth(date.Year, date.Month)
```

O método recebe ano e mês e retorna um int correspondente ao número de dias desse mês.

### Por que informar o ano?

Fevereiro pode ter 28 ou 29 dias, dependendo do ano.

| Ano | Mês | Resultado |
|---|---|---:|
| 2023 | Fevereiro | 28 |
| 2024 | Fevereiro | 29 |
| 2024 | Março | 31 |
| 2024 | Abril | 30 |
| 2024 | Dezembro | 31 |

### Exemplo

```csharp
var lastDay = DateTime.DaysInMonth(
    year: 2024,
    month: 2);

// lastDay = 29
```

O professor explica que o .NET resolve essa variação sem que a aplicação precise escrever manualmente uma condição para cada mês.

### Complemento: regra do calendário gregoriano

Um ano divisível por 4 é bissexto, exceto os anos divisíveis por 100 que não sejam também divisíveis por 400.

DateTime.DaysInMonth utiliza o calendário gregoriano e já considera essa regra.

Não é necessário implementar a regra manualmente no CashFlow.

## 12. Construção da data final

O professor define a data final com o último dia do mês e horário 23:59:59.

```csharp
var endDate = new DateTime(
    year: date.Year,
    month: date.Month,
    day: DateTime.DaysInMonth(date.Year, date.Month),
    hour: 23,
    minute: 59,
    second: 59);
```

### Exemplo com fevereiro de 2024

```text
startDate = 2024-02-01 00:00:00
endDate   = 2024-02-29 23:59:59
```

O limite inicial é inclusivo e o limite final também é inclusivo na abordagem do professor.

### Atenção

A precisão de DateTime pode representar frações de segundo posteriores a 23:59:59.

Por isso, esse limite pode excluir despesas que ocorram no último segundo do mês, se a propriedade persistida possuir precisão fracionária suficiente.

O professor não aborda esse detalhe nesta aula.

A seção 17 apresenta uma alternativa que evita essa lacuna, mantendo o código original como referência do curso.

## 13. Where e expressão lambda

Com os limites calculados, o professor acrescenta Where à consulta.

```csharp
.Where(expense =>
    expense.Date >= startDate &&
    expense.Date <= endDate)
```

### Como interpretar a expressão?

A variável expense representa um elemento da entidade Expense avaliado pela condição.

A expressão exige que duas comparações sejam verdadeiras:

1. A data da despesa é maior ou igual ao início do período.
2. A data da despesa é menor ou igual ao fim do período.

O operador && representa uma conjunção lógica.

### Operadores

| Operador | Significado |
|---|---|
| >= | Maior ou igual |
| <= | Menor ou igual |
| && | E lógico |

### Exemplo conceitual para março de 2024

| Data da despesa | >= início | <= fim | Incluída |
|---|---|---|---|
| 2024-02-29 23:59:59 | Não | Sim | Não |
| 2024-03-01 00:00:00 | Sim | Sim | Sim |
| 2024-03-20 08:30:00 | Sim | Sim | Sim |
| 2024-03-31 23:59:59 | Sim | Sim | Sim |
| 2024-04-01 00:00:00 | Sim | Não | Não |

Essa tabela é ilustrativa; não corresponde a um teste executado pelo professor nesta aula.

## 14. Ordenação com OrderBy e OrderByDescending

Filtrar não garante que as despesas sejam retornadas em ordem cronológica.

Por isso, o professor utiliza:

```csharp
.OrderBy(expense => expense.Date)
```

OrderBy ordena crescentemente pela chave selecionada.

Como Date inclui horário, duas despesas do mesmo dia, uma às 08:00 e outra às 10:00, serão ordenadas cronologicamente.

### Ordem crescente

```csharp
.OrderBy(expense => expense.Date)
```

```text
01/03/2024 08:00
01/03/2024 10:00
15/03/2024 09:00
31/03/2024 18:00
```

### Ordem decrescente

O professor apresenta também:

```csharp
.OrderByDescending(expense => expense.Date)
```

```text
31/03/2024 18:00
15/03/2024 09:00
01/03/2024 10:00
01/03/2024 08:00
```

OrderByDescending é explicado como alternativa. A direção escolhida para o relatório é crescente, do início ao final do mês.

### Precisão técnica

Sem ORDER BY, um banco de dados relacional não garante a ordem dos registros retornados.

A gravação sugere que a ordem sem ordenação seria a ordem de inserção, mas essa ordem também não é garantida. Ela pode variar de acordo com o plano de execução, índices e outros fatores.

Por isso, a decisão de ordenar explicitamente é correta.

## 15. Ordenação secundária com ThenBy

O instrutor considera o caso de duas despesas com exatamente a mesma data e hora.

Para estabelecer um critério adicional, demonstra:

```csharp
.OrderBy(expense => expense.Date)
.ThenBy(expense => expense.Title)
```

### Qual é a diferença?

OrderBy define a chave primária de ordenação.

ThenBy adiciona uma chave secundária, aplicada quando os elementos possuem o mesmo valor na chave anterior.

### Exemplo apresentado pelo professor

| Título | Data e hora |
|---|---|
| Padaria | 2024-03-01 08:00:00 |
| Açougue | 2024-03-01 08:00:00 |

Como as datas são iguais, a ordem secundária por título pode posicionar Açougue antes de Padaria.

Em consultas MySQL, a ordenação textual depende da collation utilizada.

### Complemento: não substituir ThenBy por outro OrderBy

```csharp
// Ordenação primária por data e secundária por título.
.OrderBy(expense => expense.Date)
.ThenBy(expense => expense.Title)
```

Uma nova chamada de OrderBy estabelece outra ordenação primária, em vez de expressar explicitamente uma chave secundária.

### Ordenação totalmente determinística

Se também houver títulos iguais na mesma data, pode ser necessário um terceiro critério único, como Id.

```csharp
.OrderBy(expense => expense.Date)
.ThenBy(expense => expense.Title)
.ThenBy(expense => expense.Id)
```

Esse terceiro critério não é implementado pelo professor. É uma possibilidade complementar caso seja necessária uma ordem determinística entre todos os registros.

A transcrição demonstra ThenBy por título, mas não informa inequivocamente se ele permanece no código final. A implementação consolidada a seguir utiliza esse trecho como variante ilustrada.

## 16. Código consolidado e integração

### Contrato

```csharp
Task<List<Expense>> FilterByMonth(DateOnly date);
```

### Implementação fiel à estratégia da aula

```csharp
public async Task<List<Expense>> FilterByMonth(DateOnly date)
{
    var startDate = new DateTime(
        year: date.Year,
        month: date.Month,
        day: 1).Date;

    var endDate = new DateTime(
        year: date.Year,
        month: date.Month,
        day: DateTime.DaysInMonth(date.Year, date.Month),
        hour: 23,
        minute: 59,
        second: 59);

    return await _dbContext.Expenses
        .AsNoTracking()
        .Where(expense =>
            expense.Date >= startDate &&
            expense.Date <= endDate)
        .OrderBy(expense => expense.Date)
        .ToListAsync();
}
```

### Variante com a ordenação secundária demonstrada

```csharp
return await _dbContext.Expenses
    .AsNoTracking()
    .Where(expense =>
        expense.Date >= startDate &&
        expense.Date <= endDate)
    .OrderBy(expense => expense.Date)
    .ThenBy(expense => expense.Title)
    .ToListAsync();
```

O código preserva o limite inclusivo e a ordenação crescente explicados na gravação.

### Async e await

Ao adicionar return await, o professor percebe que o método ainda não foi marcado como async.

A correção consiste em adicionar o modificador à declaração:

```csharp
public async Task<List<Expense>> FilterByMonth(DateOnly date)
```

Isso retoma o estudo de operações assíncronas.

ToListAsync devolve uma Task que representa a execução assíncrona da consulta e sua materialização.

O método pode aguardar o resultado sem bloquear uma thread durante a espera por I/O, conforme o suporte do provider.

Como complemento, um método que apenas devolvesse diretamente a Task, sem await, poderia ser declarado sem async. Essa não é a forma utilizada pelo professor.

### Chamada no caso de uso

No fim da aula, o professor coloca a chamada no início de Execute.

```csharp
var expenses = await _repository.FilterByMonth(month);
```

O nome month corresponde ao período recebido pelo caso de uso de relatório.

O professor anuncia que explicará na próxima aula por que colocou essa consulta no início do método.

Não se deve apresentar a explicação futura como se já tivesse sido dada.

### Estado do relatório

A lista já pode ser recuperada por meio do contrato criado, mas a gravação não mostra o teste de execução nem a escrita de suas despesas nas células.

## 17. Precisões técnicas: intervalos, SQL e responsabilidades

Esta seção é um aprofundamento separado da implementação original.

### 17.1 Intervalo semiaberto para cobrir frações de segundo

O código da aula usa:

```csharp
expense.Date >= startDate &&
expense.Date <= endDate
```

Com endDate fixado em 23:59:59, um valor como 2024-03-31 23:59:59.500 pode ficar fora do resultado, embora pertença a março.

Isso depende da precisão da coluna e do mapeamento adotado.

Uma alternativa robusta consiste em utilizar o primeiro instante do mês seguinte como limite superior exclusivo.

```csharp
var startDate = new DateTime(
    date.Year,
    date.Month,
    1);

var endExclusive = startDate.AddMonths(1);

return await _dbContext.Expenses
    .AsNoTracking()
    .Where(expense =>
        expense.Date >= startDate &&
        expense.Date < endExclusive)
    .OrderBy(expense => expense.Date)
    .ToListAsync();
```

### Comparação

| Estratégia | Início | Fim |
|---|---|---|
| Aula | Primeiro dia, 00:00:00 | Último dia, 23:59:59, inclusivo |
| Intervalo semiaberto | Primeiro dia, 00:00:00 | Primeiro dia do próximo mês, exclusivo |

```text
Março de 2024
[2024-03-01 00:00:00, 2024-04-01 00:00:00)
```

O colchete indica inclusão do início e o parêntese indica exclusão do fim.

AddMonths(1) trata automaticamente a mudança de mês e, quando necessário, de ano, dentro do intervalo de datas suportado pelo tipo.

Em aplicações que aceitam datas extremas, deve-se considerar também o limite máximo representável.

A alternativa não foi implementada nesta aula.

### 17.2 Tradução para SQL

Uma tradução SQL conceitual da consulta original poderia ter a seguinte estrutura:

```sql
SELECT ...
FROM Expenses
WHERE Date >= @startDate
  AND Date <= @endDate
ORDER BY Date ASC;
```

Com o critério secundário:

```sql
ORDER BY Date ASC, Title ASC
```

O SQL acima é ilustrativo. A forma literal gerada depende do provider, do mapeamento e da configuração do EF Core. O professor não inspeciona a instrução SQL nesta gravação.

### 17.3 Filtro por intervalo e índices

Comparar a coluna Date com limites calculados previamente pode favorecer o uso de um índice sobre essa coluna.

Uma abordagem que extraísse Month e Year diretamente da coluna também poderia expressar a regra mensal, mas sua tradução SQL e o aproveitamento de índices dependeriam do provider e do banco.

Não se pode garantir um plano de execução ou ganho de desempenho sem verificar a consulta real.

### 17.4 Fuso horário e semântica temporal

O exemplo do curso compara valores DateTime conforme a interpretação adotada pelo projeto.

Se as datas estiverem armazenadas em UTC e o período solicitado representar um mês no fuso horário do usuário, os limites devem ser construídos considerando essa regra temporal.

A transcrição não estabelece a política de fusos do CashFlow.

### 17.5 Responsabilidade pelo intervalo

A implementação demonstrada calcula startDate e endDate dentro do repositório.

Essa escolha é válida como estrutura atual do curso.

É possível discutir se a determinação do período deve pertencer à Application ou Domain, deixando o repositório receber limites já calculados. Contudo, o professor anuncia que discutirá especificamente esse ponto na próxima aula.

Por isso, esta documentação não atribui ao instrutor uma decisão arquitetural futura.

### 17.6 Ausência de despesas

ToListAsync pode devolver uma lista vazia.

Uma possibilidade na evolução do relatório seria verificar expenses.Count antes de construir e serializar o documento, retornando um array vazio quando não houver registros, para respeitar o contrato de HTTP 204.

Isso é uma antecipação conceitual baseada no contrato estabelecido na aula 118, não um trecho já implementado na aula 123.

## 18. Exercícios e revisão

### Exercício 1 — contrato

```csharp
Task<List<Expense>> FilterByMonth(DateOnly date);
```

Por que o método pertence ao contrato ReadOnly? Porque consulta informações sem modificar as despesas.

### Exercício 2 — início de um mês

Dado DateOnly(2024, 9, 25), construa 1º de setembro de 2024 à meia-noite.

```csharp
var startDate = new DateTime(
    date.Year,
    date.Month,
    1).Date;
```

### Exercício 3 — último dia

Qual o resultado de DateTime.DaysInMonth(2024, 2)?

**Resposta:** 29, pois 2024 é bissexto.

### Exercício 4 — limite final

```csharp
var endDate = new DateTime(
    date.Year,
    date.Month,
    DateTime.DaysInMonth(date.Year, date.Month),
    23, 59, 59);
```

### Exercício 5 — filtro mensal

```csharp
.Where(expense =>
    expense.Date >= startDate &&
    expense.Date <= endDate)
```

### Exercício 6 — ordenação crescente

```csharp
.OrderBy(expense => expense.Date)
```

### Exercício 7 — critério secundário

```csharp
.OrderBy(expense => expense.Date)
.ThenBy(expense => expense.Title)
```

### Exercício 8 — materialização assíncrona

```csharp
return await _dbContext.Expenses
    .AsNoTracking()
    .Where(expense =>
        expense.Date >= startDate &&
        expense.Date <= endDate)
    .OrderBy(expense => expense.Date)
    .ToListAsync();
```

### Exercício 9 — precisão temporal (complemento)

Uma despesa de 31/03/2024 às 23:59:59.500 é necessariamente incluída pela implementação original?

**Resposta:** não. Ela ultrapassa o limite final de 23:59:59, se sua precisão for preservada pelo armazenamento. Um limite exclusivo no primeiro dia do mês seguinte evita esse problema.

### Perguntas de revisão

1. Qual objetivo motivou a interrupção da geração visual do relatório?
2. Quais relatórios poderão reutilizar FilterByMonth?
3. Qual interface recebe o novo método?
4. Qual é o tipo retornado pelo método?
5. Por que ExpensesRepository apresentou erro de compilação?
6. Por que a consulta utiliza AsNoTracking?
7. O que aconteceria se a consulta utilizasse somente ToListAsync sem filtro?
8. Qual é o tipo do parâmetro date?
9. Qual é o tipo da propriedade Expense.Date?
10. Por que o dia recebido em DateOnly é ignorado?
11. Como construir o primeiro dia do mês?
12. O que DateTime.Date devolve?
13. O construtor DateTime com ano, mês e dia precisa de .Date para zerar a hora?
14. Como descobrir o número de dias de um mês?
15. Por que DaysInMonth recebe o ano?
16. Qual limite final o professor utiliza?
17. O que representa a expressão lambda passada a Where?
18. Qual é a função do operador &&?
19. Por que não confiar na ordem natural do banco?
20. Qual é a diferença entre OrderBy e OrderByDescending?
21. Qual é a finalidade de ThenBy?
22. Qual critério secundário o professor demonstra?
23. Por que foi necessário acrescentar async?
24. Onde a consulta é chamada em Execute?
25. Qual diferença existe entre fim inclusivo e fim exclusivo?
26. O teste de filtragem foi realizado nesta aula?
27. Quais discussões o professor deixa para a próxima aula?

## 19. Resumo e mapa mental

| Elemento | Resultado |
|---|---|
| Contrato | IExpensesReadOnlyRepository |
| Método | FilterByMonth(DateOnly date) |
| Retorno | Task<List<Expense>> |
| Implementação | ExpensesRepository |
| Rastreamento | AsNoTracking |
| Início | Primeiro dia às 00:00:00 |
| Último dia | DateTime.DaysInMonth |
| Fim da aula | Último dia às 23:59:59 |
| Filtro | Where com >=, <= e && |
| Ordenação principal | OrderBy por Date |
| Alternativa | OrderByDescending |
| Ordenação secundária demonstrada | ThenBy por Title |
| Materialização | ToListAsync |
| Chamada | await _repository.FilterByMonth(month) |
| Teste | Próxima aula |

```text
                    AULA 123 — FILTER BY MONTH
                                 |
                           DateOnly date
                                 |
                     +-----------+-----------+
                     |                       |
                   Year                    Month
                     |                       |
                     +-----------+-----------+
                                 |
                      construir limites DateTime
                                 |
              +------------------+------------------+
              |                                     |
           startDate                              endDate
       dia 1, 00:00:00                 DaysInMonth, 23:59:59
              |                                     |
              +------------------+------------------+
                                 |
                    _dbContext.Expenses
                                 |
                         AsNoTracking()
                                 |
                              Where()
                                 |
                         OrderBy(Date)
                                 |
                  ThenBy(Title) — demonstrado
                                 |
                          ToListAsync()
                                 |
                        List<Expense>
                                 |
                   Use case Excel / futuro PDF
                                 |
                     Teste na próxima aula
```

**Conclusão:** a aula 123 implementa a consulta mensal das despesas no repositório de leitura. O professor transforma o período DateOnly em dois limites DateTime, utiliza DaysInMonth para considerar corretamente o último dia, filtra com Where, ordena por data e demonstra ThenBy para desempates por título. A consulta é materializada de forma assíncrona e chamada no início do caso de uso Excel. A alternativa de intervalo semiaberto foi registrada como complemento para evitar a exclusão de frações de segundo. A validação da consulta e a discussão sobre o posicionamento da regra de datas ficam para a próxima aula.
