# Aula 119 — Relatório Excel (parte 2): DateOnly, DateTime, FromHeader e filtros em requisições GET

> **Continuação direta da aula 118:** o ReportController já possui o endpoint GET de Excel, com retorno provisório de um array de bytes por meio de File() e respostas HTTP 200 e 204.
>
> **Objetivo:** receber o período solicitado pelo consumidor da API, considerando apenas mês e ano, sem abandonar a semântica de uma operação de consulta.
>
> **Resultado da aula:** o professor demonstra a limitação de enviar um corpo em um GET pelo Swagger, experimenta PUT com FromBody, retorna ao GET utilizando FromHeader e substitui DateTime por DateOnly. O depurador confirma a interpretação de datas de janeiro, fevereiro e setembro de 2024.
>
> **Escopo:** a aula recebe e interpreta a data, mas ainda não implementa a consulta filtrada no repositório, o caso de uso de relatório ou a geração real do Excel.
>
> **Fidelidade:** os trechos de código são reconstruídos a partir da transcrição. Exemplos adicionais, correções técnicas e alternativas de arquitetura são identificados separadamente. Os testes relatados correspondem ao ambiente do professor; não constituem uma garantia universal de aceitação de todos os formatos de data.

## Índice

1. Recapitulação e objetivo
2. O período do relatório
3. Primeira abordagem: DTO de requisição
4. Tentativa de GET com FromBody
5. Por que o Swagger não envia o corpo?
6. Experimento com PUT e comparação com POST
7. Escolha final: GET com FromHeader
8. Model binding e origem dos parâmetros
9. Testes com DateTime
10. Diferenças entre DateTime e DateOnly
11. Substituição por DateOnly e testes
12. Parâmetros opcionais e validação
13. Alternativa com query string
14. Código consolidado da aula
15. Próxima etapa: filtrar despesas pelo período
16. Cuidados técnicos e boas práticas
17. Exercícios e perguntas de revisão
18. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

Na aula 118, o professor iniciou a funcionalidade de exportação de relatórios.

O endpoint foi preparado para devolver um arquivo com o método File() do ASP.NET Core:

```csharp
return File(
    file,
    MediaTypeNames.Application.Octet,
    "report.xlsx");
```

O contrato prevê:

- HTTP 200 OK quando há um arquivo a devolver.
- HTTP 204 No Content quando não existem despesas para exportar.

Até aquele momento, o array de bytes era apenas uma simulação, e a API ainda não recebia o período solicitado.

Nesta aula, o objetivo é adicionar a entrada que permitirá selecionar as despesas de um mês e ano específicos.

O professor utiliza a oportunidade para explicar diferentes maneiras de transportar informações em uma requisição HTTP e escolher um tipo de data adequado.

## 2. O período do relatório

O relatório deverá representar as despesas pertencentes a um determinado mês de determinado ano.

Exemplos de períodos possíveis:

| Ano | Mês | Período |
|---|---:|---|
| 2024 | 1 | Janeiro de 2024 |
| 2024 | 2 | Fevereiro de 2024 |
| 2024 | 9 | Setembro de 2024 |

O dia recebido não será relevante para o filtro final.

Isso significa que, segundo a regra descrita pelo professor, 2024-09-01 e 2024-09-25 deverão representar o mesmo período: setembro de 2024.

A hora também não será utilizada.

### Distinção importante

O parâmetro representa uma data utilizada como referência para identificar o período. Não representa necessariamente a data de uma despesa individual nem uma consulta restrita a um dia.

Apesar de o tipo de data escolhido conter dia, mês e ano, a regra de negócio pretende considerar somente as propriedades Month e Year.

Nenhum filtro de banco é implementado nesta gravação.

## 3. Primeira abordagem: DTO de requisição

O professor apresenta uma classe criada no projeto Communication, denominada RequestInformationReportJson.

A classe possui uma propriedade Month, do tipo DateTime.

### Estrutura reconstruída

```csharp
public class RequestInformationReportJson
{
    public DateTime Month { get; set; }
}
```

### Por que criar uma classe?

A intenção inicial é preparar um contrato de entrada que possa receber outros filtros no futuro.

Uma classe desse tipo permite agrupar várias informações relacionadas à solicitação de um relatório.

Por exemplo, poderiam existir filtros adicionais relacionados ao tipo de pagamento ou a outros critérios, caso a regra de negócio evoluísse.

Esses campos adicionais são apenas possibilidades ilustrativas; não aparecem na implementação da aula.

### Corpo de requisição ilustrativo

```json
{
  "month": "2024-01-02T00:00:00"
}
```

O JSON acima representa o formato conceitual da propriedade DateTime. O valor literal do primeiro teste com PUT e FromBody não é especificado pela transcrição.

## 4. Tentativa de GET com FromBody

O primeiro experimento consiste em receber o DTO pelo corpo da requisição.

```csharp
[HttpGet("excel")]
public async Task<IActionResult> GetExcel(
    [FromBody] RequestInformationReportJson request)
{
    // Estrutura provisória do relatório.
}
```

FromBody informa ao ASP.NET Core que os dados do parâmetro devem ser obtidos a partir do corpo da requisição, normalmente por meio de um input formatter apropriado.

O professor executa a API e observa que o Swagger apresenta um corpo de requisição com a propriedade month.

Porém, ao clicar em Try it out e tentar executar a operação, a ferramenta apresenta um erro informando que não pode enviar um corpo em uma requisição GET.

O teste não chega ao fluxo esperado do endpoint.

### Qual é o problema identificado?

Embora seja possível declarar FromBody na action, isso não garante que o cliente HTTP conseguirá enviar um GET com corpo.

O professor decide experimentar outras formas de transportar a data.

## 5. Por que o Swagger não envia o corpo?

### Explicação do professor

A gravação apresenta uma regra prática: não utilizar um corpo de requisição em GET, pois o Swagger utilizado no teste não permite executar essa solicitação.

Essa orientação é adequada para evitar problemas de interoperabilidade.

### Precisão técnica — complemento

O protocolo HTTP não estabelece uma proibição absoluta de existir conteúdo em uma mensagem GET. Entretanto, o conteúdo de uma requisição GET não possui semântica geralmente definida e pode ser rejeitado por implementações.

A especificação HTTP recomenda que clientes não gerem conteúdo em GET, salvo situações específicas de acordo direto com o servidor que ofereça suporte.

Além disso, a API Fetch dos navegadores não permite fornecer um body para uma requisição GET ou HEAD. Essa limitação é coerente com o erro observado no Swagger UI executado no navegador.

A transcrição não apresenta o texto integral da exceção nem a implementação interna do cliente, portanto não se deve atribuir uma mensagem literal não fornecida.

### Consequência prática

Mesmo que um cliente HTTP específico consiga enviar um GET com corpo, outros clientes, proxies, caches ou ferramentas de documentação podem não suportar esse uso de maneira consistente.

Para filtros de consulta, a URL ou cabeçalhos são alternativas que evitam essa dependência de um corpo em GET.

## 6. Experimento com PUT e comparação com POST

O professor sugere trocar temporariamente o método HTTP para permitir o envio do DTO pelo corpo da requisição.

A alteração demonstrada utiliza PUT.

```csharp
[HttpPut("excel")]
public async Task<IActionResult> GetExcel(
    [FromBody] RequestInformationReportJson request)
{
    // Estrutura provisória do relatório.
}
```

Ao executar a requisição pelo Swagger, o breakpoint é atingido e a propriedade Month possui um valor.

Assim, o teste comprova que o envio do DTO pelo corpo funciona naquele cenário utilizando PUT.

O professor também menciona POST como possibilidade, mas não demonstra um teste equivalente de POST.

### Por que ele não mantém PUT?

A operação não pretende atualizar um recurso. Ela pretende recuperar um relatório.

O instrutor considera estranho utilizar PUT ou POST apenas para transportar um corpo quando existe uma alternativa mantendo GET.

### Precisão técnica: POST e PUT não são semanticamente equivalentes

O professor afirma que, para receber o corpo naquele experimento, tanto POST quanto PUT poderiam funcionar.

Isso não significa que os métodos HTTP sejam intercambiáveis em qualquer API.

| Método | Semântica geral |
|---|---|
| GET | Recuperar uma representação de um recurso |
| POST | Solicitar processamento conforme a semântica definida pelo recurso |
| PUT | Criar ou substituir o estado do recurso identificado pela URI |

POST pode ser uma escolha legítima para executar uma pesquisa ou gerar um relatório com filtros complexos no corpo, sem necessariamente criar um registro de negócio.

PUT normalmente é menos apropriado para uma operação que apenas consulta dados e gera uma representação para download.

GET é um método seguro e idempotente por definição de sua semântica. PUT é idempotente, mas não seguro. POST não possui uma garantia geral de segurança ou idempotência.

Essas distinções dizem respeito à semântica HTTP, não à mera capacidade técnica de receber JSON.

## 7. Escolha final: GET com FromHeader

Como o filtro atual consiste em uma única data, o professor abandona o DTO no parâmetro da action.

Ele passa a receber diretamente uma data pelo cabeçalho da requisição.

### Primeira versão

```csharp
[HttpGet("excel")]
public async Task<IActionResult> GetExcel(
    [FromHeader] DateTime month)
{
    // Estrutura provisória do relatório.
}
```

O endpoint volta a ser GET.

Ao executar a API, o Swagger apresenta month como parâmetro de cabeçalho.

### Representação conceitual da requisição

```http
GET /api/report/excel HTTP/1.1
Host: example.com
month: 2024-01-02
```

O host é ilustrativo, e a rota segue a estrutura registrada na aula 118.

### O que FromHeader faz?

O atributo instrui o model binding a buscar o valor do parâmetro em um cabeçalho HTTP, em vez de procurar um valor no corpo.

Com o parâmetro chamado month, o nome utilizado para a busca corresponde a month, salvo configuração explícita diferente.

Os nomes de cabeçalhos HTTP não diferenciam letras maiúsculas de minúsculas.

### Exemplo complementar de nome explícito

```csharp
[FromHeader(Name = "month")] DateOnly month
```

A forma com Name não é necessária no exemplo da aula, mas permite especificar explicitamente o nome externo do cabeçalho.

## 8. Model binding e origem dos parâmetros

Model binding é o mecanismo do ASP.NET Core que obtém dados de uma requisição e os associa aos parâmetros ou modelos de uma action.

Os atributos de origem já estudados no CashFlow permitem explicitar de onde cada informação vem.

| Atributo | Origem | Exemplo |
|---|---|---|
| FromBody | Corpo | Dados de cadastro de uma despesa |
| FromRoute | Segmento da rota | ID de uma despesa |
| FromQuery | Query string | Ano e mês de um relatório |
| FromHeader | Cabeçalho HTTP | month: 2024-09-01 |
| FromServices | Container de DI | Caso de uso injetado |

Nesta aula, o professor demonstra diretamente FromBody e FromHeader.

FromQuery é apresentado neste material como complemento técnico, não como uma implementação realizada na gravação.

### Conversão do valor

Um cabeçalho HTTP transporta um valor textual. O model binding tenta convertê-lo para o tipo declarado na action.

Por exemplo:

```text
month: 2024-01-02
       |
       v
Conversão para DateTime
       |
       v
Year = 2024
Month = 1
Day = 2
```

A aceitação do texto depende do tipo de destino, da cultura, do mecanismo de conversão e da versão/configuração da aplicação.

## 9. Testes com DateTime

O professor realiza testes utilizando o Swagger e inspeciona os valores no depurador.

### Teste 1 — data completa

Valor informado:

```text
2024-01-02
```

Resultado observado:

| Propriedade | Valor |
|---|---:|
| Year | 2024 |
| Month | 1 |
| Day | 2 |

O professor observa que sua máquina utiliza uma apresentação de datas em inglês. Por isso, a data pode aparecer como 1/2/2024, seguindo a ordem mês/dia/ano naquele ambiente.

Ele expande o objeto no debugger para verificar individualmente as propriedades Year, Month e Day.

### Por que conferir as propriedades?

A representação textual depende da cultura e pode gerar interpretações ambíguas.

Uma data exibida como 1/2/2024 pode ser entendida de maneiras diferentes por leitores acostumados a formatos distintos.

Ao examinar Year, Month e Day, o desenvolvedor confirma os componentes efetivamente interpretados pelo programa.

### Teste 2 — ano e mês com barra

O professor informa um valor correspondente a 2024/2.

O resultado observado é:

- Year = 2024.
- Month = 2.
- Day = 1.

### Teste 3 — ano e mês com hífen

O professor repete o teste utilizando hífen como separador entre ano e mês.

O resultado continua sendo fevereiro de 2024, com o dia interpretado como 1.

### Limite dos testes

A aceitação de formatos abreviados e a atribuição do dia 1 são resultados observados no ambiente do professor.

Isso não estabelece que toda configuração de model binding, cultura ou versão do .NET aceitará os mesmos textos.

Para uma API com contrato estável, é preferível documentar um formato inequívoco e validar a entrada de acordo com esse contrato.

## 10. Diferenças entre DateTime e DateOnly

O professor percebe que DateTime contém componentes de data e de horário, embora o relatório não precise de horário.

A partir disso, escolhe DateOnly.

| Característica | DateTime | DateOnly |
|---|---|---|
| Ano | Sim | Sim |
| Mês | Sim | Sim |
| Dia | Sim | Sim |
| Hora, minuto e segundo | Sim | Não |
| Kind associado a UTC/local/unspecified | Sim | Não |
| Uso típico | Data com horário | Data de calendário |

DateOnly foi introduzido no .NET 6 e representa uma data de calendário sem componente de horário.

### Exemplo complementar

```csharp
var dateTime = new DateTime(2024, 9, 25, 14, 30, 0);

var dateOnly = new DateOnly(2024, 9, 25);
```

O primeiro objeto representa data e horário. O segundo representa somente uma data.

### DateOnly representa somente mês e ano?

Não.

DateOnly possui dia, mês e ano. Ele elimina o horário, mas não elimina o dia.

O professor esclarece que a regra do relatório continuará ignorando o dia.

### Por que é uma boa escolha neste contexto?

O tipo comunica que o horário não faz parte da informação necessária.

Essa adequação semântica é mais importante do que uma eventual redução de memória.

A gravação não mede consumo de memória, tamanho de requisição ou desempenho. A troca do tipo não garante, por si só, que o valor textual enviado pelo cliente será menor.

### Complemento: não existe YearMonth nativo equivalente

A biblioteca padrão do .NET não oferece um tipo dedicado de ano e mês equivalente a DateOnly para esse caso.

É possível criar um tipo próprio, receber ano e mês separadamente ou utilizar uma data cujo dia seja ignorado por contrato.

O professor escolhe a terceira abordagem.

## 11. Substituição por DateOnly e testes

A assinatura passa a utilizar:

```csharp
[FromHeader] DateOnly month
```

O endpoint permanece GET.

### Teste 4 — setembro de 2024 sem informar o dia

Valor digitado:

```text
2024-09
```

Resultado observado no debugger:

| Propriedade | Valor |
|---|---:|
| Year | 2024 |
| Month | 9 |
| Day | 1 |

### Teste 5 — setembro de 2024 com dia explícito

Valor correspondente a:

```text
2024-09-25
```

Resultado observado:

| Propriedade | Valor |
|---|---:|
| Year | 2024 |
| Month | 9 |
| Day | 25 |

A transcrição relata que as duas entradas funcionaram no ambiente demonstrado.

### Resultado esperado na futura regra de negócio

Como somente ano e mês interessam, ambos os valores deverão selecionar o mesmo conjunto de despesas de setembro de 2024.

Essa equivalência é uma intenção explícita do professor, não um resultado de uma consulta já implementada.

## 12. Parâmetros opcionais e validação

Ao discutir filtros futuros, o professor menciona que poderia tornar parâmetros opcionais por meio do ponto de interrogação.

### Exemplo complementar

```csharp
[FromHeader] DateOnly? month
```

DateOnly é um tipo de valor. DateOnly? é a forma abreviada de Nullable<DateOnly>, que pode representar uma data ou a ausência de valor.

### O que muda?

| Tipo | Representação |
|---|---|
| DateOnly | Um valor do tipo data |
| DateOnly? | Uma data ou null |

### Importante: nulabilidade não define toda a regra de negócio

Tornar um parâmetro anulável permite representar ausência, mas não determina automaticamente o que o sistema deve fazer quando ele não for informado.

É necessário definir uma política, por exemplo:

- utilizar o mês atual;
- rejeitar a solicitação;
- consultar um período padrão;
- aplicar o filtro somente quando fornecido.

Essas opções são complementos de design. Nenhuma é implementada nesta aula.

O contrato final mostrado pelo professor continua recebendo DateOnly month, não DateOnly?.

### Cabeçalho ausente ou inválido

O comportamento exato depende do model binding, dos atributos e das configurações da aplicação. Não se deve presumir que todo parâmetro de tipo de valor não anulável seja automaticamente tratado como obrigatório em todos os casos.

Um valor não convertível pode produzir erro de model state e, em um controller com ApiController e comportamento padrão, HTTP 400.

A exigência de presença pode ser explicitada com mecanismos apropriados, como BindRequired, conforme a estratégia adotada e o suporte da configuração.

O professor não testa cabeçalho ausente nem entrada inválida para DateOnly nesta gravação.

## 13. Alternativa com query string

**Complemento arquitetural:** o professor escolhe cabeçalhos HTTP para preservar GET. Uma alternativa muito comum para filtros de consulta é a query string.

Por exemplo:

```http
GET /api/report/excel?month=2024-09-01
```

A action poderia receber:

```csharp
[HttpGet("excel")]
public async Task<IActionResult> GetExcel(
    [FromQuery] DateOnly month)
{
    // Estrutura do relatório.
}
```

Outra opção seria receber mês e ano separadamente:

```http
GET /api/report/excel?year=2024&month=9
```

```csharp
public async Task<IActionResult> GetExcel(
    [FromQuery] int year,
    [FromQuery] int month)
{
    // Validar o período antes de utilizá-lo.
}
```

A segunda opção expressa diretamente os dois componentes necessários e elimina a necessidade de receber um dia arbitrário.

### Comparação de abordagens

| Abordagem | Vantagem | Cuidado |
|---|---|---|
| GET + FromHeader | Mantém GET e segue a escolha do professor | O filtro fica fora da URL; cabeçalhos customizados podem exigir configuração adicional |
| GET + FromQuery | Filtros visíveis na URL; facilidade para compartilhar e identificar consultas | URLs muito extensas podem ser inadequadas para filtros grandes |
| POST + FromBody | Permite filtros complexos em um modelo JSON | Semântica e comportamento de cache diferentes dos de GET |
| PUT + FromBody | Permite receber dados no corpo | Geralmente inadequado para uma consulta que não substitui estado |
| GET + FromBody | Pode ser aceito em situações específicas | Interoperabilidade problemática, inclusive no Swagger UI |

### Por que query string é frequentemente preferida para filtros?

O período altera o conteúdo retornado pelo endpoint. Quando esse parâmetro integra a URL, é mais fácil distinguir as diferentes representações solicitadas.

Cabeçalhos são úteis para informações associadas ao processamento da requisição, como autenticação, negociação de conteúdo e metadados específicos. Também podem transportar filtros por decisão de contrato, mas esse não é necessariamente o padrão mais natural para filtros de consulta.

A aula não apresenta nem testa FromQuery. O código principal deste material mantém FromHeader para respeitar a implementação do curso.

## 14. Código consolidado da aula

O trecho abaixo representa o endpoint após a mudança para DateOnly.

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [HttpGet("excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetExcel(
        [FromHeader] DateOnly month)
    {
        // Conteúdo provisório da aula 118.
        // A geração efetiva do relatório ainda será implementada.
        var file = new byte[1];

        if (file.Length > 0)
            return File(
                file,
                MediaTypeNames.Application.Octet,
                "report.xlsx");

        return NoContent();
    }
}
```

O código consolida a estrutura anteriormente preparada e a nova assinatura apresentada nesta aula.

### O que deve ser observado?

- O método HTTP permanece GET.
- Month passa a ser obtido do cabeçalho.
- DateOnly elimina o horário da representação interna.
- O dia ainda existe no tipo, mas será ignorado na regra de negócio.
- O array de bytes continua provisório.
- A data ainda não é utilizada para consultar o banco.
- O método async ainda pode apresentar aviso por não conter await nessa etapa.

O DTO RequestInformationReportJson foi utilizado no experimento inicial, mas deixou de ser o parâmetro da action na solução final. A gravação não confirma sua exclusão do projeto.

O professor não implementa um novo use case nem instala uma biblioteca de geração de planilhas.

## 15. Próxima etapa: filtrar despesas pelo período

O professor anuncia que, internamente, utilizará o ano e o mês para recuperar apenas as despesas relevantes.

A estratégia exata de consulta fica para as próximas aulas.

### Exemplo complementar de intervalo mensal

Uma maneira de representar o período é normalizar a data para o primeiro dia do mês.

```csharp
var startDate = new DateOnly(
    month.Year,
    month.Month,
    1);

var endDate = startDate.AddMonths(1);
```

Para qualquer dia de setembro de 2024, isso produz:

```text
Início: 2024-09-01
Fim exclusivo: 2024-10-01
```

Se a propriedade Expense.Date for DateTime, uma consulta ilustrativa poderia converter os limites para esse tipo:

```csharp
var start = startDate.ToDateTime(TimeOnly.MinValue);
var end = endDate.ToDateTime(TimeOnly.MinValue);

var expenses = await _dbContext.Expenses
    .AsNoTracking()
    .Where(expense =>
        expense.Date >= start &&
        expense.Date < end)
    .ToListAsync();
```

O exemplo pressupõe que o armazenamento e a interpretação temporal da propriedade Date sejam compatíveis com os limites construídos. Caso as despesas sejam armazenadas em UTC, por exemplo, o período solicitado pode precisar ser convertido a partir do fuso horário definido pela regra de negócio.

### Por que um limite final exclusivo?

O intervalo inclui todas as datas e horários a partir do começo do mês até, mas sem incluir, o primeiro instante do mês seguinte.

Isso evita depender de um último horário artificial, como 23:59:59, que poderia excluir registros com maior precisão temporal.

Consultas por intervalo também podem favorecer o uso de índices sobre a coluna de data, dependendo do provider, da configuração e do plano de execução.

**Atenção:** esse código é apenas um complemento técnico. A transcrição não mostra a implementação de Where, AsNoTracking ou qualquer intervalo mensal nesta aula. Não se deve atribuir essa estratégia específica ao professor antes da gravação correspondente.

## 16. Cuidados técnicos e boas práticas

### 16.1 Contrato de formato

Documentar um formato completo e não ambíguo, como yyyy-MM-dd, reduz a dependência de interpretações culturais.

Os formatos abreviados testados pelo professor funcionaram em seu ambiente, mas não devem ser assumidos como universalmente válidos.

Se o contrato exigir estritamente yyyy-MM ou outro formato específico, uma estratégia possível é receber texto e utilizar parsing exato com validação apropriada, ou implementar um tipo/model binder específico.

### 16.2 DateOnly não é um instante

DateOnly não representa horário nem fuso. É uma data de calendário.

O sistema precisa definir o significado de um mês quando as despesas possuem horários ou fusos diferentes.

### 16.3 Filtros em cabeçalhos e cache

Se um GET retorna relatórios diferentes conforme o cabeçalho month, caches intermediários precisam distinguir as representações.

Quando aplicável, um cabeçalho de resposta Vary: month pode informar essa variação. Também é necessário definir uma política de cache apropriada, especialmente para dados financeiros.

A query string normalmente diferencia os recursos consultados pela própria URL.

Nenhuma política de cache é implementada na gravação.

### 16.4 CORS e cabeçalhos customizados

Um cliente web de outra origem que envie um cabeçalho customizado, como month, pode precisar de uma requisição preflight e de autorização desse cabeçalho pela política CORS do servidor.

O efeito depende do contexto da requisição. Não há configuração de CORS nova nesta aula.

### 16.5 GET continua sendo a escolha do curso

A seleção de GET é coerente com uma operação de consulta. A opção por FromHeader é uma decisão do instrutor, não uma exigência geral do ASP.NET Core.

### 16.6 Reutilizar DTOs quando o contrato justificar

O DTO inicial permitiria reunir filtros futuros.

A decisão de receber diretamente DateOnly simplifica o contrato atual, mas um modelo de filtros poderá ser útil caso a funcionalidade cresça.

### 16.7 Validar ausência e valores inválidos

O relatório deve tratar de forma explícita entradas inválidas conforme o contrato que o projeto vier a definir.

É importante não confundir uma data ausente ou inválida com um mês válido que não possui despesas. O último caso é aquele para o qual a aula 118 prevê HTTP 204.

### 16.8 Não confundir teste de binding com geração de Excel

O debugger comprova que os valores das datas foram recebidos e interpretados.

Isso não demonstra que o filtro funciona no banco, que os registros corretos foram selecionados ou que uma planilha válida foi produzida.

---

## 17. Exercícios e perguntas de revisão

### Exercício 1 — DTO inicial

Qual classe foi apresentada para agrupar os filtros?

```csharp
public class RequestInformationReportJson
{
    public DateTime Month { get; set; }
}
```

### Exercício 2 — identificar a limitação

Por que o GET com FromBody falhou no Swagger?

**Resposta:** o cliente utilizado não permite enviar um corpo com GET. Embora o protocolo HTTP não proíba universalmente a existência física desse conteúdo, ele não possui semântica geral definida para GET e seu uso pode causar problemas de interoperabilidade.

### Exercício 3 — assinatura escolhida

```csharp
public async Task<IActionResult> GetExcel(
    [FromHeader] DateOnly month)
```

O valor é obtido do cabeçalho da requisição.

### Exercício 4 — DateTime versus DateOnly

Qual tipo é mais adequado quando não se precisa de horário?

**Resposta:** DateOnly, desde que a informação represente uma data de calendário.

### Exercício 5 — interpretar o período

Se month possui valor 2024-09-25, qual período interessa ao relatório?

**Resposta:** setembro de 2024. O dia 25 não deve restringir a consulta ao dia 25.

### Exercício 6 — parâmetro opcional

```csharp
[FromHeader] DateOnly? month
```

Esse parâmetro pode representar a ausência de uma data. Ainda é necessário definir como a regra de negócio tratará null.

### Exercício 7 — alternativa de design

Escreva uma URL para solicitar setembro de 2024 utilizando query string.

```http
GET /api/report/excel?year=2024&month=9
```

A alternativa é complementar; o professor utiliza FromHeader.

### Perguntas de revisão

1. Qual informação ficou faltando no endpoint da aula 118?
2. Quais componentes da data serão utilizados no filtro final?
3. Qual DTO é apresentado inicialmente?
4. Por que o professor criou uma classe para os filtros?
5. Qual atributo foi utilizado na primeira tentativa?
6. O GET com corpo funcionou no Swagger?
7. Qual método HTTP o professor utiliza temporariamente para demonstrar o corpo?
8. POST também foi executado na gravação ou apenas mencionado?
9. Por que o professor considera inadequado utilizar PUT para esse relatório?
10. Qual atributo permite receber a data pelo cabeçalho?
11. Qual é a função do model binding?
12. Qual tipo de data foi utilizado inicialmente?
13. Quais propriedades o professor inspeciona no debugger?
14. Qual data completa de janeiro é demonstrada?
15. Qual dia foi interpretado quando o professor informou apenas ano e mês de fevereiro?
16. Por que a cultura da máquina pode alterar a apresentação de uma data?
17. Por que o professor troca DateTime por DateOnly?
18. DateOnly elimina o componente de dia?
19. Quais datas de setembro foram testadas?
20. Os formatos abreviados observados são garantidos em toda configuração?
21. Como representar uma data opcional em C#?
22. Qual é a diferença entre ausência de filtro e ausência de despesas?
23. Por que FromQuery é uma alternativa comum para filtros?
24. Qual formato HTTP o professor mantém na implementação final?
25. A consulta filtrada no banco foi implementada?
26. Uma planilha XLSX válida foi produzida nesta gravação?

---

## 18. Resumo e mapa mental

| Componente | Resultado da aula |
|---|---|
| DTO inicial | RequestInformationReportJson com DateTime Month |
| Primeira tentativa | GET com FromBody |
| Resultado | Erro no cliente Swagger |
| Experimento alternativo | PUT com FromBody, valor recebido no breakpoint |
| Escolha definitiva | GET com FromHeader |
| Tipo inicial do parâmetro direto | DateTime |
| Tipo final | DateOnly |
| Filtro pretendido | Year e Month |
| Dia | Recebido, mas não relevante para a regra |
| Horário | Eliminado com DateOnly |
| Testes | Janeiro, fevereiro e setembro de 2024 |
| Consulta no banco | Ainda não implementada |
| Geração de Excel | Próxima etapa |

```text
                  AULA 119 — FILTRO DE RELATÓRIO
                                |
                    RequestInformationReportJson
                          DateTime Month
                                |
                       GET + FromBody
                                |
                        Erro no Swagger
                                |
                       PUT + FromBody
                                |
                        Binding funciona
                                |
                     Escolha de manter GET
                                |
                  FromHeader + DateTime month
                                |
                       Horário desnecessário
                                |
                  FromHeader + DateOnly month
                                |
                    +-----------+-----------+
                    |           |           |
                   Year        Month       Day
                    |           |           |
                    +-----+-----+        ignorado
                          |
                    Período do relatório
                          |
                    Consulta de despesas
                    (ainda não implementada)
                          |
                    Geração do Excel
                    (ainda não implementada)
```

**Conclusão:** a aula 119 define como o endpoint de Excel receberá o período solicitado. O professor experimenta um DTO no corpo, verifica a limitação do GET no Swagger, demonstra o funcionamento do PUT e decide preservar GET com um parâmetro recebido pelo cabeçalho. A troca de DateTime para DateOnly elimina o componente de horário. Os testes confirmam a interpretação das datas no ambiente demonstrado. O código ainda não filtra as despesas nem gera um arquivo Excel válido; esses passos serão desenvolvidos nas próximas aulas. As alternativas com query string, validação explícita e intervalo mensal foram apresentadas neste material apenas como aprofundamento técnico, mantendo a sequência original do curso.
