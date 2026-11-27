# Aula 128 — Relatório PDF (parte 1): endpoint GET, FromQuery, caso de uso e reutilização do repositório

> **Contexto:** a aula 127 encerrou a etapa de geração de relatórios Excel com a utilização de using para descartar XLWorkbook. Agora o CashFlow passa a oferecer uma segunda representação das despesas: um relatório PDF.
>
> **Objetivo:** preparar a arquitetura, o contrato do caso de uso, a injeção de dependência e o endpoint de exportação em PDF, reaproveitando a consulta mensal já construída.
>
> **Resultado da aula:** o professor executa o novo GET no Swagger, envia 2024-05 pela query string e verifica no depurador que o caso de uso foi resolvido pela DI e que o parâmetro representa 1º de maio de 2024.
>
> **Escopo:** ainda não existe biblioteca de geração de PDF instalada ou documento PDF efetivamente gerado. O caso de uso retorna temporariamente um array vazio mesmo quando há despesas. A lógica de criação do documento será desenvolvida nas próximas aulas.
>
> **Fidelidade:** os trechos de código são reconstruídos da transcrição e do contexto anterior. Os nomes completos da classe e da interface do caso de uso PDF, o nome da action e a ordem exata dos parâmetros não são enunciados literalmente; os identificadores utilizados neste material são ilustrativos. As explicações adicionais sobre HTTP, model binding, testes e arquitetura estão identificadas como complementos.

## Índice

1. Recapitulação e objetivo
2. Excel e PDF: funcionalidades relacionadas
3. Organização dos casos de uso
4. Contrato do caso de uso PDF
5. Injeção do repositório e consulta mensal
6. Early Return e implementação provisória
7. Registro na injeção de dependência
8. Novo endpoint no ReportController
9. FromQuery e query string
10. DateOnly e interpretação do período
11. Content-Type e MediaTypeNames.Application.Pdf
12. File(), nome do arquivo e respostas HTTP
13. Código consolidado da etapa atual
14. Teste pelo Swagger e depuração
15. Complementos arquiteturais e cuidados técnicos
16. Exercícios e perguntas de revisão
17. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

O módulo de relatórios do CashFlow foi construído gradualmente durante as aulas 118 a 127.

| Aula | Etapa |
|---|---|
| 118 | Endpoint para devolver arquivo Excel com File() |
| 119 | Recebimento do período com DateOnly e FromHeader |
| 120 | Instalação do ClosedXML e criação do workbook |
| 121 | Cabeçalhos localizados e estilos |
| 122 | Serialização em memória e download |
| 123 | Consulta mensal com FilterByMonth |
| 124 | Validação da consulta, Early Return e HTTP 204 |
| 125 | Preenchimento das despesas e conversão de enum |
| 126 | Formatação monetária e tentativa de ajuste de largura |
| 127 | IDisposable e descarte determinístico com using |
| 128 | Estrutura inicial da exportação PDF |

A nova funcionalidade permitirá que um consumidor da API solicite as despesas de um determinado mês e ano e receba um documento PDF.

A operação de consulta é semelhante à exportação Excel. A principal diferença será o formato final da representação.

O professor apresenta esta aula como a primeira de uma sequência dedicada à geração de PDF. Ele mostra código que já havia preparado antes da gravação e explica seus componentes.

Não há construção visual do PDF nesta etapa.

## 2. Excel e PDF: funcionalidades relacionadas

Ambos os relatórios utilizarão as despesas de um período mensal.

```text
                         Período solicitado
                                 |
                                 v
                     FilterByMonth(DateOnly)
                                 |
                           List<Expense>
                                 |
                  +--------------+--------------+
                  |                             |
              Relatório Excel               Relatório PDF
                  |                             |
              report.xlsx                   report.pdf
                  |                             |
                  +--------------+--------------+
                                 |
                          Resposta HTTP
```

O código de consulta não precisa ser duplicado apenas porque o formato de saída mudou.

O repositório é responsável por obter os dados solicitados. Cada caso de uso poderá realizar o processamento específico de sua representação.

### Comparação

| Característica | Excel | PDF |
|---|---|---|
| Dados de origem | Despesas | Despesas |
| Período | Mês e ano | Mês e ano |
| Consulta | FilterByMonth | FilterByMonth |
| Representação final | XLSX | PDF |
| Tipo de retorno do caso de uso | byte[] como resultado assíncrono | byte[] como resultado assíncrono |
| Situação atual | Geração funcional demonstrada | Estrutura inicial, sem geração do documento |

O PDF será desenvolvido com foco em apresentação visual organizada, mas a aula não define seu layout, biblioteca, fontes, tabelas ou imagens.

## 3. Organização dos casos de uso

O professor acrescenta uma pasta chamada PDF dentro da pasta Reports, criada anteriormente no projeto Application.

Estrutura conceitual:

```text
CashFlow.Application
└── UseCases
    └── Expenses
        └── Reports
            ├── Excel
            │   ├── Interface do caso de uso
            │   └── Implementação do caso de uso
            └── PDF
                ├── Interface do caso de uso
                └── Implementação do caso de uso
```

A separação mantém agrupadas as responsabilidades relacionadas aos relatórios e permite que cada formato evolua de forma independente.

Os arquivos específicos não são nomeados literalmente na transcrição.

Neste material serão utilizados os nomes ilustrativos:

- IGenerateExpensesReportPdfUseCase.
- GenerateExpensesReportPdfUseCase.

Os identificadores devem ser ajustados ao código original, inclusive a capitalização de PDF ou Pdf.

### Por que não criar outro repositório específico para PDF?

A consulta de despesas por mês já existe e não depende do formato de apresentação.

Criar um repositório diferente apenas para exportar PDF repetiria uma responsabilidade que o IExpensesReadOnlyRepository já atende.

A transcrição confirma a reutilização do repositório, não a criação de uma nova implementação de persistência.

## 4. Contrato do caso de uso PDF

O professor apresenta a interface do caso de uso, semelhante à interface do Excel.

Ela possui um método Execute que recebe DateOnly e devolve um array de bytes como resultado.

Uma assinatura reconstruída de acordo com o fluxo assíncrono de consulta é:

```csharp
public interface IGenerateExpensesReportPdfUseCase
{
    Task<byte[]> Execute(DateOnly month);
}
```

### Elementos do contrato

| Elemento | Responsabilidade |
|---|---|
| Execute | Operação principal do caso de uso |
| DateOnly month | Data de referência do período |
| Task | Representação da operação assíncrona |
| byte[] | Conteúdo binário do documento, quando implementado |

DateOnly contém ano, mês e dia. Contudo, a consulta considera somente ano e mês.

O tipo byte[] é independente do formato documental. O significado de seus bytes depende de como eles forem produzidos.

Um array com conteúdo arbitrário não se torna um PDF válido apenas porque a action informa application/pdf ou utiliza uma extensão .pdf.

Nesta aula, o contrato está preparado, mas a produção dos bytes reais ainda não existe.

## 5. Injeção do repositório e consulta mensal

A classe do caso de uso recebe IExpensesReadOnlyRepository pelo construtor.

```csharp
public class GenerateExpensesReportPdfUseCase
    : IGenerateExpensesReportPdfUseCase
{
    private readonly IExpensesReadOnlyRepository _repository;

    public GenerateExpensesReportPdfUseCase(
        IExpensesReadOnlyRepository repository)
    {
        _repository = repository;
    }

    // Execute será implementado aqui.
}
```

Os nomes da classe e do campo são reconstruídos conforme os padrões das aulas anteriores.

A utilização do repositório de leitura é coerente com a finalidade do relatório: recuperar dados, sem realizar alterações nas despesas.

### Reutilização de FilterByMonth

```csharp
var expenses = await _repository.FilterByMonth(month);
```

O método foi criado na aula 123 e validado na aula 124.

Na versão demonstrada anteriormente, ele:

1. Determina o início do mês às 00:00:00.
2. Calcula o último dia com DateTime.DaysInMonth.
3. Define o fim do mês às 23:59:59.
4. Usa AsNoTracking.
5. Filtra por intervalo com Where.
6. Ordena por Date e, em caso de empate, Title.
7. Executa a consulta com ToListAsync.

A aula atual não modifica a implementação do filtro.

A ressalva sobre frações de segundo e a alternativa de intervalo semiaberto foram documentadas nas aulas 123 e 124. Não se deve interpretar esta aula como uma refatoração desse comportamento.

### Relação com princípios de arquitetura

Como complemento, a reutilização do contrato existente é um exemplo de separação entre a obtenção dos dados e sua apresentação.

O caso de uso depende de uma abstração, em vez de acessar diretamente o DbContext ou escrever uma nova consulta SQL.

Isso é coerente com o Dependency Inversion Principle e com a divisão de responsabilidades estabelecida no projeto.

## 6. Early Return e implementação provisória

O professor repete o tratamento adotado para o Excel.

```csharp
var expenses = await _repository.FilterByMonth(month);

if (expenses.Count == 0)
    return [];
```

Quando não existem despesas para o mês, não há motivo para gerar o documento.

O retorno vazio será interpretado pelo controller como ausência de conteúdo.

### Estrutura provisória completa

```csharp
public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    // Futuramente: construir o documento PDF.
    // Futuramente: gerar e retornar os bytes do documento.

    return [];
}
```

O segundo return [] é temporário. O professor explica que o mantém para evitar um erro de compilação por ausência de retorno em um caminho de execução.

### Diferença entre os dois retornos

| Retorno | Condição | Situação |
|---|---|---|
| Primeiro return [] | Lista sem despesas | Regra definitiva do fluxo atual |
| Segundo return [] | Lista com despesas | Placeholder temporário |

A distinção é essencial para interpretar o teste.

O caso de uso ainda não consegue devolver um documento, independentemente da quantidade de despesas encontrada.

Portanto, quando a consulta termina normalmente, tanto o caminho sem despesas quanto o caminho com despesas acabam produzindo um array de comprimento zero.

### Collection expressions

A expressão [] é a mesma sintaxe estudada na aula 124, introduzida no C# 12.

Nesse método, o compilador utiliza o tipo de destino byte[] para interpretar a coleção vazia.

Uma forma equivalente para representar o resultado vazio seria:

```csharp
return Array.Empty<byte>();
```

Essa é uma alternativa complementar; o professor utiliza a sintaxe [].

## 7. Registro na injeção de dependência

O professor lembra que a classe e a interface do novo caso de uso precisam ser registradas no serviço de injeção de dependência da Application.

Reconstrução coerente com as inscrições scoped anteriores:

```csharp
services.AddScoped<
    IGenerateExpensesReportPdfUseCase,
    GenerateExpensesReportPdfUseCase>();
```

A aula confirma que o registro foi realizado, mas não transcreve literalmente a chamada ou o nome completo dos tipos.

### Por que o registro é necessário?

O controller solicitará uma instância por meio da interface.

O container de DI precisa conhecer a implementação correspondente para conseguir construir o caso de uso e fornecer suas dependências.

```text
Controller solicita a interface
               |
               v
Container de DI
               |
               v
Cria ou resolve o caso de uso PDF
               |
               v
Fornece IExpensesReadOnlyRepository
               |
               v
Caso de uso disponível para a action
```

O repositório já havia sido registrado durante a construção da Infrastructure. Não é necessário cadastrar outra implementação apenas para que um segundo caso de uso dependa do mesmo contrato.

### Relação com a aula 115

Na criação do endpoint DELETE, o projeto apresentou HTTP 500 porque o caso de uso ainda não havia sido registrado.

O professor reforça nesta aula a necessidade de não esquecer essa etapa.

O teste do novo endpoint confirma que uma instância do caso de uso foi recebida corretamente pelo controller.

## 8. Novo endpoint no ReportController

O professor adiciona uma nova action ao controller de relatórios.

A rota é diferenciada pelo segmento pdf, assim como o Excel já utiliza o segmento excel.

```csharp
[HttpGet("pdf")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> GetPdf(
    [FromServices] IGenerateExpensesReportPdfUseCase useCase,
    [FromQuery] DateOnly month)
{
    var file = await useCase.Execute(month);

    if (file.Length > 0)
        return File(
            file,
            MediaTypeNames.Application.Pdf,
            "report.pdf");

    return NoContent();
}
```

O nome GetPdf e a ordem dos parâmetros são ilustrativos. Os elementos confirmados pela narração são GET com segmento PDF, o parâmetro vindo da query, a injeção do caso de uso, os status 200 e 204 e o retorno de arquivo com MIME de PDF.

A rota pode ser representada conforme o prefixo utilizado nas aulas anteriores:

```http
GET /api/report/pdf?month=2024-05
```

A porta, a URL absoluta e a capitalização literal exibida não são informadas.

### Responsabilidades

| Componente | Responsabilidade |
|---|---|
| FromServices | Obter o caso de uso pelo container |
| FromQuery | Obter o período pela query string |
| Execute | Coordenar a operação de relatório |
| File | Entregar os bytes como arquivo |
| NoContent | Devolver uma resposta sem conteúdo |
| ProducesResponseType | Documentar resultados previstos |

O controller permanece responsável pela comunicação HTTP. A futura construção do PDF ficará no caso de uso e nos componentes que vierem a apoiá-lo.

## 9. FromQuery e query string

A diferença mais importante na entrada do novo endpoint é:

```csharp
[FromQuery] DateOnly month
```

Na exportação Excel, a aula 119 havia adotado:

```csharp
[FromHeader] DateOnly month
```

O professor não mostra uma alteração no endpoint Excel nesta gravação. Ele utiliza uma origem de dados diferente especificamente no PDF.

### O que é query string?

É a parte de uma URI que contém a informação de consulta após o caractere ?.

Exemplo:

```http
GET /api/report/pdf?month=2024-05
```

| Parte | Significado |
|---|---|
| /api/report/pdf | Caminho do endpoint |
| ? | Início da query |
| month | Nome do parâmetro |
| = | Separação entre nome e valor nessa convenção |
| 2024-05 | Valor enviado |

O atributo FromQuery informa ao model binding do ASP.NET Core que o argumento deve ser obtido da query string.

### Exemplo com múltiplos parâmetros — complemento

```http
GET /api/report/pdf?month=2024-05&paymentType=1
```

O caractere & separa parâmetros nessa convenção.

PaymentType é apenas um exemplo de filtro adicional. O novo endpoint recebe somente month como filtro na aula.

### Comparação com FromHeader

```http
GET /api/report/excel HTTP/1.1
Host: localhost
month: 2024-05
```

```http
GET /api/report/pdf?month=2024-05 HTTP/1.1
Host: localhost
```

Os exemplos utilizam um host ilustrativo.

No primeiro, o período é enviado como cabeçalho HTTP. No segundo, está presente na URL.

### Comparação das origens de model binding

| Atributo | Origem |
|---|---|
| FromQuery | Query string |
| FromHeader | Cabeçalhos |
| FromRoute | Segmentos da rota |
| FromBody | Corpo da requisição |
| FromServices | Container de DI |

FromServices não é uma origem de dados enviada pelo consumidor; ele resolve uma dependência do servidor.

### Complemento arquitetural

Query strings são uma forma habitual de representar filtros de leitura, pois a URL identifica o período solicitado de maneira explícita.

É mais simples compartilhar ou reproduzir uma consulta pela URL quando os parâmetros relevantes estão nela. Ainda assim, autenticação, cultura e outros cabeçalhos podem ser necessários.

Cabeçalhos customizados também são possíveis, conforme demonstrado na aula 119.

A transcrição não apresenta uma comparação de desempenho, uma justificativa de segurança ou uma migração global de FromHeader para FromQuery.

## 10. DateOnly e interpretação do período

O professor testa o endpoint com:

```text
2024-05
```

O depurador mostra uma data correspondente ao primeiro dia de maio de 2024.

| Propriedade | Valor observado |
|---|---:|
| Year | 2024 |
| Month | 5 |
| Day | 1 |

Como a máquina do professor está configurada em inglês, a apresentação visual da data segue uma convenção diferente da usual em português brasileiro.

Isso não altera as propriedades Year, Month e Day.

### Relação com FilterByMonth

O dia não é utilizado como critério de seleção do período mensal.

Duas datas de referência que possuam o mesmo ano e mês produzirão os mesmos limites na implementação existente.

```csharp
var first = new DateOnly(2024, 5, 1);
var another = new DateOnly(2024, 5, 20);
```

Ambas representam maio de 2024 para a finalidade de FilterByMonth.

O exemplo é complementar; somente o valor 2024-05 é demonstrado no teste desta aula.

### Cuidados com a interpretação

O resultado de parsing observado demonstra que o formato abreviado funcionou no ambiente do professor.

Não é uma garantia universal de aceitação desse formato para toda cultura, versão e configuração de model binding.

Um contrato de API poderia documentar um formato inequívoco como yyyy-MM-dd, ou implementar parsing explícito de yyyy-MM se desejar representar estritamente mês e ano.

A aula não implementa validação adicional do formato nem testa entradas ausentes ou inválidas.

## 11. Content-Type e MediaTypeNames.Application.Pdf

A aula 118 introduziu a classe MediaTypeNames, pertencente ao namespace System.Net.Mime.

Para o Excel, o professor utilizou:

```csharp
MediaTypeNames.Application.Octet
```

Esse valor representa application/octet-stream, o tipo de mídia genérico para dados binários.

Para o PDF, existe uma constante específica:

```csharp
MediaTypeNames.Application.Pdf
```

Seu valor corresponde a:

```text
application/pdf
```

### Comparação

| Constante | Tipo de mídia |
|---|---|
| MediaTypeNames.Application.Octet | application/octet-stream |
| MediaTypeNames.Application.Pdf | application/pdf |

### Por que usar o MIME específico?

O tipo de mídia descreve a natureza da representação enviada.

Quando o documento é um PDF válido, application/pdf comunica esse formato com maior precisão que application/octet-stream.

O framework pode utilizar esse valor ao configurar o cabeçalho Content-Type da resposta.

### Precisão importante

Informar application/pdf não produz os bytes de um PDF e não valida automaticamente sua estrutura.

A biblioteca que futuramente construirá o documento será responsável por produzir conteúdo compatível com o formato.

Até esta aula, o caso de uso devolve um array vazio, então a action ainda não demonstra uma resposta HTTP com um PDF real.

## 12. File(), nome do arquivo e respostas HTTP

O professor reaproveita a estrutura de retorno do Excel, alterando o tipo de mídia e o nome do arquivo.

```csharp
return File(
    file,
    MediaTypeNames.Application.Pdf,
    "report.pdf");
```

### Argumentos

| Argumento | Finalidade |
|---|---|
| file | Array de bytes |
| MediaTypeNames.Application.Pdf | Tipo de mídia do conteúdo |
| "report.pdf" | Nome sugerido para download |

O nome e a extensão são diferentes do report.xlsx utilizado no Excel.

### Cabeçalhos HTTP conceituais — complemento

Quando o caso de uso passar a gerar um PDF válido, o resultado File poderá configurar uma resposta semelhante a:

```http
HTTP/1.1 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename=report.pdf

[bytes do documento]
```

O exemplo é ilustrativo. A forma exata dos cabeçalhos depende do framework e da configuração.

Content-Type e Content-Disposition possuem finalidades diferentes. O primeiro identifica a representação; o segundo pode indicar a disposição de download e seu nome sugerido.

### Decisão do controller

```csharp
if (file.Length > 0)
    return File(
        file,
        MediaTypeNames.Application.Pdf,
        "report.pdf");

return NoContent();
```

| Condição | Resposta |
|---|---|
| Array não vazio | File(), normalmente HTTP 200 |
| Array vazio | NoContent(), HTTP 204 |

O atributo ProducesResponseType informa os resultados previstos, mas não implementa essas condições por si só.

O HTTP 204 não contém um documento para baixar.

### Estado provisório

Enquanto o segundo return [] existir no caso de uso, o caminho de geração bem-sucedida permanece incompleto.

O 200 documentado é parte do contrato pretendido, não um download de PDF comprovado pela aula.

## 13. Código consolidado da etapa atual

Os seguintes trechos representam a implementação preparada pelo professor. Os nomes completos do caso de uso são ilustrativos.

### Application: interface

```csharp
public interface IGenerateExpensesReportPdfUseCase
{
    Task<byte[]> Execute(DateOnly month);
}
```

### Application: implementação

```csharp
public class GenerateExpensesReportPdfUseCase
    : IGenerateExpensesReportPdfUseCase
{
    private readonly IExpensesReadOnlyRepository _repository;

    public GenerateExpensesReportPdfUseCase(
        IExpensesReadOnlyRepository repository)
    {
        _repository = repository;
    }

    public async Task<byte[]> Execute(DateOnly month)
    {
        var expenses = await _repository.FilterByMonth(month);

        if (expenses.Count == 0)
            return [];

        // TODO: implementar a geração do PDF.

        return []; // Temporário para manter o retorno do método.
    }
}
```

### Application: registro na DI

```csharp
services.AddScoped<
    IGenerateExpensesReportPdfUseCase,
    GenerateExpensesReportPdfUseCase>();
```

### API: nova action

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

// Dentro de ReportController:
[HttpGet("pdf")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> GetPdf(
    [FromServices] IGenerateExpensesReportPdfUseCase useCase,
    [FromQuery] DateOnly month)
{
    var file = await useCase.Execute(month);

    if (file.Length > 0)
        return File(
            file,
            MediaTypeNames.Application.Pdf,
            "report.pdf");

    return NoContent();
}
```

Os namespaces internos do CashFlow e a classe completa do controller foram omitidos porque não foram reproduzidos literalmente na transcrição.

O objetivo é documentar a etapa atual, não fornecer uma implementação fictícia de um gerador de PDF.

## 14. Teste pelo Swagger e depuração

O professor executa a API e abre o Swagger.

O ReportController passa a apresentar um novo GET de PDF.

### Procedimento

1. Selecionar o endpoint PDF.
2. Clicar em Try it out.
3. Informar 2024-05 no parâmetro month.
4. Executar a requisição.
5. Acompanhar o breakpoint no controller.
6. Verificar a instância do caso de uso recebida pela DI.
7. Verificar o valor convertido de DateOnly.
8. Observar que a URL contém ?month=2024-05.

### Resultados sustentados pela gravação

| Verificação | Resultado |
|---|---|
| Endpoint apresentado no Swagger | Sim |
| Método HTTP | GET |
| Entrada | 2024-05 |
| Origem do período | Query string |
| Caso de uso resolvido | Confirmado no debugger |
| Data interpretada | 1º de maio de 2024 |
| Download de um PDF válido | Não realizado |
| Quantidade de despesas recuperadas | Não inspecionada explicitamente nesta aula |
| Status final exibido | Não informado literalmente |

O professor informa que nada será devolvido porque a implementação está provisória.

Pelo código apresentado, se a consulta for concluída normalmente, o array vazio leva ao retorno NoContent, isto é, HTTP 204.

Essa é uma conclusão do fluxo de código. A gravação não mostra explicitamente o número do status HTTP final.

### Placeholder versus mock — complemento

O professor descreve o retorno atual informalmente como um mock.

Mais precisamente, o segundo return [] é uma implementação provisória ou placeholder. A aula não cria um mock de teste com uma biblioteca de mocking nem configura expectativas de interação.

O repositório continua sendo a dependência real apresentada no caso de uso.

## 15. Complementos arquiteturais e cuidados técnicos

Os pontos seguintes são complementos de estudo. Não representam alterações efetuadas pelo professor.

### 15.1 Reutilização da consulta sem duplicação da geração

O repositório já fornece uma operação mensal adequada aos dois formatos.

A geração de XLSX e a futura geração de PDF têm particularidades diferentes e podem permanecer em casos de uso distintos.

Se houver operações repetidas entre eles, como tradução do tipo de pagamento ou apresentação monetária, uma refatoração compartilhada poderá ser considerada quando os requisitos estiverem definidos.

A aula 125 já mencionava essa possibilidade. Nesta aula ainda não há extração de conversores nem alteração dos recursos de tradução.

### 15.2 Ausência de dados versus implementação incompleta

No contrato atual, um array vazio indica ao controller que não há conteúdo a entregar.

O segundo return [] também utiliza esse mesmo sinal, embora existam potencialmente despesas.

Essa ambiguidade é aceitável como placeholder didático temporário, mas não deve permanecer em um gerador concluído.

Em uma implementação funcional, a existência de despesas deve conduzir à produção do documento ou ao tratamento apropriado de uma falha de geração.

Não se deve mascarar exceções de geração como ausência de despesas sem uma decisão explícita de contrato.

### 15.3 Model binding não substitui validação

FromQuery especifica a origem do valor.

Ele não estabelece, sozinho, todas as regras de negócio do período aceito.

É necessário distinguir uma data inválida, um parâmetro ausente e um período válido sem despesas.

Em controllers com ApiController e comportamento padrão, falhas de conversão registradas no ModelState podem resultar em HTTP 400. A exigência de presença e as regras específicas dependem dos atributos e da configuração.

O professor não testa esses cenários nesta aula.

### 15.4 Filtros em URL e privacidade

A query string pode aparecer em históricos, logs e outras ferramentas de observabilidade.

Neste caso, o professor envia um período mensal, não credenciais ou dados de pagamento.

Informações sensíveis, tokens e segredos não devem ser colocados na URL apenas por conveniência.

A aula não altera autenticação, autorização ou políticas de cache.

### 15.5 Status e documentação

Os status 200 e 204 representam o fluxo de sucesso previsto.

A documentação desses status não comprova que a API esteja imune a erros nem garante que todas as respostas possíveis sejam apenas essas duas.

O conteúdo binário pode ser documentado em OpenAPI de maneira específica para a versão e ferramenta utilizada. Isso não foi configurado nesta gravação.

### 15.6 Tipo de mídia e compatibilidade

Um PDF válido pode ser enviado com application/pdf e nome report.pdf.

O comportamento de abertura ou download depende também de Content-Disposition e do cliente HTTP.

A aula ainda não demonstra um PDF em leitor, não escolhe um pacote de geração nem analisa sua licença.

### 15.7 Testes que poderão ser realizados após a implementação

| Cenário | Resultado esperado do contrato pretendido |
|---|---|
| Período válido sem despesas | HTTP 204 |
| Período válido com despesas | HTTP 200 e PDF válido |
| Repositório devolve lista vazia | Nenhum documento é construído |
| Mês solicitado contém despesas de outros períodos | Apenas as despesas do mês selecionado devem aparecer |
| Datas de referência diferentes no mesmo mês | Mesmo período de consulta |
| Parâmetro inválido | Tratamento conforme o contrato e model binding |
| Geração falha | Tratamento de erro apropriado, sem confundir com ausência de registros |

São testes sugeridos, não resultados obtidos durante a aula.

## 16. Exercícios e perguntas de revisão

### Exercício 1 — definir o contrato

```csharp
Task<byte[]> Execute(DateOnly month);
```

Explique por que o resultado pode continuar sendo byte[] mesmo que o formato do documento seja alterado de Excel para PDF.

**Resposta:** ambos os formatos podem ser representados por sequências de bytes. A diferença está na estrutura produzida pelo gerador e no tipo de mídia utilizado.

### Exercício 2 — reutilizar o repositório

```csharp
var expenses = await _repository.FilterByMonth(month);

if (expenses.Count == 0)
    return [];
```

O fluxo deve evitar a construção do documento quando não há dados.

### Exercício 3 — receber pela query

```csharp
[FromQuery] DateOnly month
```

Escreva uma URL de consulta para maio de 2024:

```http
GET /api/report/pdf?month=2024-05
```

### Exercício 4 — retornar o PDF

```csharp
return File(
    file,
    MediaTypeNames.Application.Pdf,
    "report.pdf");
```

O código exige um array que contenha os bytes de um documento válido para que o resultado seja utilizável.

### Exercício 5 — interpretar o placeholder

Se existem despesas, mas Execute ainda possui return [] no fim, qual resultado o controller produzirá?

**Resposta:** NoContent(), supondo que a consulta e o restante da execução terminem normalmente. O array vazio faz Length > 0 ser falso.

### Exercício 6 — registro na DI

Explique por que criar a interface e a classe não é suficiente para resolver a dependência por FromServices.

**Resposta:** o container precisa de uma inscrição apropriada que relacione o contrato à implementação e permita resolver suas dependências.

### Perguntas de revisão

1. Qual funcionalidade começa a ser construída nesta aula?
2. Qual pasta é acrescentada dentro de Reports?
3. Por que o repositório de Excel também pode ser utilizado pelo PDF?
4. Qual método obtém as despesas mensais?
5. Qual é o tipo do parâmetro de Execute?
6. Quais componentes desse parâmetro são relevantes ao filtro?
7. Qual é o tipo do resultado binário?
8. Por que há dois retornos vazios na implementação atual?
9. Qual deles deverá permanecer quando o PDF estiver concluído?
10. Por que a interface do caso de uso precisa ser registrada na DI?
11. Qual controller recebe o novo endpoint?
12. Qual método HTTP e segmento de rota são utilizados?
13. De onde o endpoint Excel recebe month?
14. De onde o endpoint PDF recebe month?
15. O que significa o caractere ? na URL demonstrada?
16. Qual é a finalidade de FromServices?
17. Qual namespace fornece MediaTypeNames?
18. Qual constante identifica o tipo de mídia do PDF?
19. Qual é a diferença entre application/pdf e application/octet-stream?
20. Qual nome de arquivo foi escolhido?
21. Qual condição leva ao retorno File?
22. Qual condição leva ao retorno NoContent?
23. Qual data foi informada no Swagger?
24. Qual data o professor observou no debugger?
25. O status 200 documentado comprova que um PDF foi criado?
26. Qual biblioteca de PDF é instalada nesta aula?
27. A implementação existente do Excel foi alterada para usar query string?
28. O professor testou o download de um PDF válido?

## 17. Resumo e mapa mental

| Componente | Resultado da aula |
|---|---|
| Novo módulo | Relatório PDF |
| Organização | Expenses/Reports/PDF |
| Caso de uso | Interface e implementação preparadas |
| Dependência | IExpensesReadOnlyRepository |
| Consulta | FilterByMonth(month) |
| Filtro | Ano e mês |
| Retorno assíncrono pretendido | Task<byte[]> |
| Ausência de registros | return [] |
| Geração com registros | Placeholder return [] |
| Controller | ReportController |
| Rota | GET /api/report/pdf (reconstruída) |
| Origem do período | FromQuery |
| Tipo do período | DateOnly |
| Tipo de mídia | application/pdf |
| Nome | report.pdf |
| Status documentados | 200 e 204 |
| Teste | 2024-05, DI e binding confirmados |
| Geração real de PDF | Pendente |

```text
                   AULA 128 — INTRODUÇÃO AO PDF
                                 |
                      GET /api/report/pdf
                                 |
                     ?month=2024-05
                                 |
                       FromQuery DateOnly
                                 |
                          ReportController
                                 |
                       FromServices / DI
                                 |
                       Caso de uso PDF
                                 |
                      FilterByMonth(month)
                                 |
                          List<Expense>
                                 |
                         Count == 0?
                          /       \
                        sim       não
                         |         |
                     return []    Gerar PDF (futuro)
                         |         |
                         |      return [] (provisório)
                         |         |
                         +----+----+
                              |
                           byte[]
                              |
                     Length > 0?
                       /       \
                     sim       não
                      |         |
                   File()   NoContent()
                      |         |
                 report.pdf   HTTP 204
                 HTTP 200
                 (futuro)
```

**Conclusão:** a aula 128 inicia o módulo de relatórios PDF reaproveitando o repositório mensal e os padrões de DI, retorno binário e tratamento de ausência de despesas construídos no Excel. O professor prepara um novo caso de uso e endpoint GET, recebe DateOnly por FromQuery e configura o tipo de mídia application/pdf e o nome report.pdf. O teste confirma a integração da DI e a interpretação do período no Swagger. A geração do documento permanece provisória; não há biblioteca PDF selecionada nem arquivo PDF válido produzido nesta gravação.
