# Aula 127 — IDisposable, Dispose(), using e gerenciamento determinístico de recursos

> **Contexto:** o relatório Excel do CashFlow já foi gerado, preenchido, formatado e testado. Na aula 126, a formatação monetária foi validada, embora o ajuste automático de largura não tenha apresentado o resultado esperado no Google Planilhas. Nesta aula, o professor retorna ao exemplo de ClosedXML para observar uma diferença importante: a utilização de using na criação do XLWorkbook.
>
> **Objetivo:** compreender IDisposable, Dispose(), a liberação de recursos e a diferença entre a instrução using tradicional e a declaração using moderna.
>
> **Resultado:** o professor acrescenta using à declaração do workbook e encerra a etapa de geração do relatório Excel.
>
> **Fidelidade:** os exemplos são reconstruídos a partir da narração. A gravação demonstra a alteração no XLWorkbook; a aplicação ao MemoryStream, a expansão em try/finally, as distinções sobre Garbage Collector e os exemplos de gerenciamento de recursos são complementos técnicos. Não há novo teste de download, medição de desempenho ou inspeção da memória nesta aula.

## Índice

1. Contexto e motivação
2. Os dois usos de using apresentados na aula
3. Recursos gerenciados e não gerenciados
4. Interface IDisposable
5. Método Dispose
6. Descarte manual e seus riscos
7. Instrução using tradicional
8. Declaração using moderna
9. Escopo e momento do descarte
10. Relação com try/finally e exceções
11. Garbage Collector versus Dispose
12. Aplicação ao relatório Excel
13. Descarte do MemoryStream e propriedade dos recursos
14. Complementos técnicos e boas práticas
15. Exercícios e perguntas de revisão
16. Resumo e mapa mental

---

## 1. Contexto e motivação

Ao consultar novamente o exemplo da biblioteca ClosedXML no GitHub, o professor observa que a documentação cria o workbook dentro de uma instrução using.

Exemplo representativo:

```csharp
using (var workbook = new XLWorkbook())
{
    // Configurar e preencher o documento.
    // Salvar o documento.
}
```

Até a aula anterior, o caso de uso do CashFlow criava o objeto diretamente:

```csharp
var workbook = new XLWorkbook();
```

O documento era gerado corretamente. Entretanto, o código não expressava explicitamente quando os recursos do workbook deveriam ser descartados.

O professor acrescenta using à declaração:

```csharp
using var workbook = new XLWorkbook();
```

A mudança não tem como finalidade alterar o conteúdo da planilha. Ela melhora o gerenciamento do ciclo de vida do objeto.

A gravação encerra o módulo de Excel após explicar essa alteração.

## 2. Os dois usos de using apresentados na aula

O professor destaca que o aluno já conhece using desde as aulas de namespaces.

### Diretiva using

```csharp
using ClosedXML.Excel;
```

Essa diretiva permite referenciar tipos presentes no namespace sem escrever sempre seu nome totalmente qualificado.

Sem a diretiva, um exemplo equivalente seria:

```csharp
var workbook = new ClosedXML.Excel.XLWorkbook();
```

A diretiva using não instala a biblioteca nem cria uma instância do tipo. O pacote precisa estar referenciado pelo projeto.

### using para gerenciamento de recursos

```csharp
using var workbook = new XLWorkbook();
```

Nesse contexto, using expressa a necessidade de descartar o recurso ao sair do escopo.

| Forma | Finalidade |
|---|---|
| using ClosedXML.Excel; | Disponibilizar nomes de tipos de um namespace |
| using (var workbook = new XLWorkbook()) { ... } | Gerenciar o recurso dentro de um bloco explícito |
| using var workbook = new XLWorkbook(); | Gerenciar o recurso até o fim do escopo que contém a declaração |

A mesma palavra reservada participa de construções diferentes da linguagem.

**Complemento:** C# também possui formas como using static, aliases e global using. A aula aborda a importação de namespaces e o gerenciamento de recursos; não pretende enumerar todas as construções possíveis.

## 3. Recursos gerenciados e não gerenciados

O professor explica que determinadas classes utilizam recursos que podem não ser controlados diretamente pelo .NET.

Exemplos mencionados estão associados à leitura e escrita de arquivos, buffers e bloqueios.

### Recursos gerenciados

São objetos e estruturas cuja memória é administrada pelo runtime .NET por meio do Garbage Collector.

Exemplos conceituais:

- Objetos de classes C#.
- Arrays de bytes.
- Strings.
- Listas de entidades.

### Recursos não gerenciados

São recursos cujo ciclo de vida não é administrado diretamente pelo Garbage Collector.

Exemplos:

- Handles de arquivos.
- Handles de recursos nativos do sistema operacional.
- Memória alocada por determinadas APIs nativas.
- Recursos de interoperabilidade com bibliotecas nativas.

Um objeto gerenciado pode encapsular um recurso não gerenciado.

```text
Aplicação .NET
      |
      v
Objeto gerenciado
      |
      v
Handle ou recurso externo
      |
      v
Sistema operacional
```

O fato de a memória do objeto ser gerenciada não significa que todos os recursos que ele utiliza serão liberados imediatamente quando a aplicação terminar de utilizá-lo.

### Precisões técnicas

IDisposable não prova que uma classe possua diretamente recursos não gerenciados. Ela também pode implementar essa interface para descartar objetos gerenciados que são IDisposable, devolver recursos a um pool ou encerrar um ciclo de vida lógico.

Buffers também podem ser gerenciados. Não se deve classificar todo buffer como recurso não gerenciado.

Arquivos podem ser abertos com modos de compartilhamento diferentes. Um arquivo não fica necessariamente inacessível a todos os outros programas sempre que é aberto pelo .NET.

O relatório do CashFlow é produzido em memória; a aula não comprova a existência de um bloqueio de arquivo físico específico dentro de XLWorkbook.

## 4. Interface IDisposable

O professor navega até a definição de XLWorkbook utilizando Ctrl e clique no Visual Studio.

Ele observa que o tipo implementa IDisposable.

Essa interface pertence ao namespace System.

Sua definição essencial é:

```csharp
public interface IDisposable
{
    void Dispose();
}
```

O contrato fornece uma operação de descarte.

Uma classe concreta que implementa essa interface deve disponibilizar uma implementação apropriada do método, diretamente ou por herança.

### Relação com interfaces estudadas anteriormente

O CashFlow já utiliza interfaces como IExpensesReadOnlyRepository, IUnitOfWork e os contratos dos casos de uso.

IDisposable também é uma interface, mas possui uma finalidade padronizada pelo .NET: comunicar que determinado objeto oferece uma operação de descarte.

A linguagem C# reconhece essa operação nas construções using apropriadas.

### Exemplo ilustrativo

```csharp
public sealed class ExampleResource : IDisposable
{
    public void Dispose()
    {
        Console.WriteLine("Recurso descartado.");
    }
}
```

O exemplo apenas demonstra o contrato. Não representa uma implementação real de liberação de um recurso nativo.

## 5. Método Dispose

Dispose é o método responsável por realizar o trabalho de descarte definido pelo tipo.

No caso de XLWorkbook, o professor mostra que existe:

```csharp
workbook.Dispose();
```

A responsabilidade concreta do método depende da implementação da biblioteca.

Uma implementação de Dispose pode liberar recursos nativos que possui, descartar dependências, fechar handles, devolver recursos alugados ou invalidar o estado de utilização do objeto.

Não existe uma regra segundo a qual Dispose destrói imediatamente o objeto ou apaga seus dados da RAM.

### Ciclo de vida conceitual

```text
Criar o objeto
      |
      v
Utilizar o recurso
      |
      v
Dispose()
      |
      v
Fim do uso do recurso
```

O objeto pode continuar existindo na memória gerenciada após o descarte.

Normalmente, não se deve continuar utilizando um recurso descartado. Dependendo do tipo e da operação, isso pode provocar ObjectDisposedException ou outro comportamento definido pelo contrato.

## 6. Descarte manual e seus riscos

Uma possibilidade é chamar Dispose explicitamente ao final do uso:

```csharp
var workbook = new XLWorkbook();

var worksheet = workbook.Worksheets.Add("Relatório");
worksheet.Cell("A1").Value = "Título";

// Salvar o workbook.

workbook.Dispose();
```

O professor apresenta essa possibilidade para explicar o que using automatiza.

### Problema do descarte manual

Se ocorrer uma exceção antes de workbook.Dispose(), a execução poderá não chegar à chamada.

Um return antecipado também pode impedir que uma instrução posterior seja executada.

```csharp
var workbook = new XLWorkbook();

var worksheet = workbook.Worksheets.Add("Relatório");

throw new InvalidOperationException();

// Esta instrução não seria alcançada:
// workbook.Dispose();
```

Além do risco de interrupção do fluxo, o desenvolvedor pode esquecer de escrever a chamada.

Isso motiva a utilização de uma construção da linguagem que assegura o descarte quando o controle deixa o escopo.

## 7. Instrução using tradicional

O exemplo observado pelo professor no GitHub utiliza um bloco explícito.

```csharp
using (var workbook = new XLWorkbook())
{
    var worksheet = workbook.Worksheets.Add("Relatório");

    worksheet.Cell("A1").Value = "Título";

    // Demais configurações e salvamento.
}
```

Ao sair do bloco, o mecanismo de using executa Dispose sobre o recurso inicializado, quando aplicável.

O descarte ocorre no encerramento daquele bloco, e não necessariamente no fim de todo o método.

### Exemplo independente

```csharp
Console.WriteLine("Antes");

using (var resource = new ExampleResource())
{
    Console.WriteLine("Utilizando");
}

Console.WriteLine("Depois");
```

Com a implementação didática da seção 4, a sequência seria:

```text
Antes
Utilizando
Recurso descartado.
Depois
```

O bloco explícito é útil quando o recurso precisa ser liberado antes de outras etapas do mesmo método.

A sintaxe tradicional continua válida nas versões modernas de C#.

## 8. Declaração using moderna

O professor explica que não é necessário envolver todo o código em um novo par de chaves.

Ele utiliza:

```csharp
using var workbook = new XLWorkbook();
```

Essa construção é chamada de using declaration e foi introduzida no C# 8.0.

O recurso é descartado ao sair do escopo que contém a declaração.

### Exemplo

```csharp
public void Generate()
{
    using var workbook = new XLWorkbook();

    var worksheet = workbook.Worksheets.Add("Relatório");
    worksheet.Cell("A1").Value = "Título";

    // O workbook será descartado na saída deste escopo.
}
```

### Comparação entre as sintaxes

| Característica | using statement | using declaration |
|---|---|---|
| Sintaxe | using (...) { ... } | using var ...; |
| Bloco próprio | Sim | Não cria um bloco adicional |
| Momento do descarte | Saída do bloco explícito | Saída do escopo que contém a declaração |
| Disponibilidade | Forma tradicional do C# | C# 8.0 ou posterior |
| Adequação | Delimitar explicitamente o ciclo de vida | Reduzir aninhamento quando o recurso é usado até o fim do escopo |

O professor afirma que as duas formas têm o mesmo resultado no cenário do relatório. Isso é correto quando o bloco tradicional abranger exatamente a mesma região de execução que a declaração moderna.

Não são necessariamente equivalentes se os limites dos escopos forem diferentes.

**Precisão:** a disponibilidade da sintaxe depende da versão da linguagem C#, não simplesmente do número da versão do .NET.

## 9. Escopo e momento do descarte

Escopo é a região do código em que determinado identificador pode ser utilizado de acordo com as regras da linguagem.

A aula relaciona esse conceito aos blocos delimitados por chaves.

```csharp
{
    using var workbook = new XLWorkbook();

    // workbook está disponível aqui.
}

// workbook não está disponível aqui.
```

Quando a declaração using está diretamente no corpo de Execute, o descarte normalmente ocorre na saída do método.

Quando ela está em um bloco interno, o descarte ocorre na saída daquele bloco.

### Exemplo: diferença de momento

```csharp
public void Execute()
{
    using (var first = new ExampleResource())
    {
        Console.WriteLine("Primeira etapa");
    }

    Console.WriteLine("Segunda etapa");

    using var second = new ExampleResource();

    Console.WriteLine("Terceira etapa");
}
```

first é descartado antes da segunda etapa. second é descartado ao sair do escopo de Execute.

### Não confundir escopo com último acesso

Uma declaração using não significa que o compilador chamará Dispose imediatamente depois da última instrução que menciona a variável.

O limite é o escopo em que ela foi declarada.

Esse detalhe complementa a explicação do professor de que o descarte acontece ao final do uso da variável.

## 10. Relação com try/finally e exceções

O principal benefício de using não é apenas economizar uma linha de código.

A construção expressa uma garantia de descarte na saída normal do escopo e durante a propagação ordinária de exceções.

Uma expansão conceitualmente equivalente, para o caso de um objeto de referência IDisposable, seria:

```csharp
var workbook = new XLWorkbook();

try
{
    var worksheet = workbook.Worksheets.Add("Relatório");
    worksheet.Cell("A1").Value = "Título";

    // Utilizar o workbook.
}
finally
{
    if (workbook is not null)
        workbook.Dispose();
}
```

Essa expansão é um complemento técnico. O professor não escreve try/finally na gravação.

### Por que finally?

finally é executado quando o controle sai do bloco try de maneira normal, por return ou durante a propagação ordinária de uma exceção.

Consequentemente, o descarte não depende de a execução atingir uma chamada manual posicionada no fim do método.

### Limites da garantia

using não captura nem trata automaticamente exceções de negócio.

Se uma operação falhar, Dispose será tentado conforme as regras da linguagem, mas a exceção continuará seguindo o fluxo normal de propagação, salvo tratamento específico.

A garantia também não significa que o descarte será executado em qualquer evento catastrófico, como encerramento abrupto do processo, nem que Dispose não possa falhar.

Se a construção do recurso falhar antes de ele ser inicializado, não existe uma instância inicializada que aquele using possa descartar.

## 11. Garbage Collector versus Dispose

O professor associa using à liberação de recursos e à otimização do programa.

É importante distinguir gerenciamento de memória de gerenciamento determinístico de recursos.

### Garbage Collector

O Garbage Collector (GC) administra a memória de objetos gerenciados.

Quando um objeto deixa de ser alcançável, sua memória pode ser recuperada em uma coleta futura.

O momento dessa coleta não é determinado simplesmente pelo fim do método.

### Dispose

Dispose executa a operação de descarte prevista pelo tipo no momento em que é chamado.

using organiza essa chamada segundo os limites do escopo.

| Aspecto | Garbage Collector | IDisposable/Dispose |
|---|---|---|
| Finalidade principal | Recuperar memória gerenciada | Encerrar o uso de recursos conforme seu contrato |
| Momento | Determinado pelo GC | Determinado pela chamada ou saída do escopo using |
| Libera objetos gerenciados imediatamente? | Não há essa garantia | Não é sua finalidade |
| Substitui a outra técnica? | Não | Não |

### Exemplo conceitual

```text
Objeto ainda alcançável
        |
     Dispose()
        |
Recurso descartado
        |
Objeto torna-se inalcançável
        |
Coleta futura pelo GC
```

O GC não chama Dispose automaticamente apenas porque um objeto implementa IDisposable.

Finalizadores, quando presentes, possuem um mecanismo distinto. Não substituem o descarte determinístico.

### Precisão sobre performance

O descarte adequado pode reduzir retenção desnecessária de recursos e melhorar a capacidade de uma aplicação de atender operações subsequentes.

Contudo, using não garante sozinho que um sistema esteja otimizado, nem prova redução de uso de memória ou tempo de execução.

A aula não apresenta benchmark. O resultado comprovado na gravação é a alteração do padrão de gerenciamento de recursos, não uma medida de desempenho.

## 12. Aplicação ao relatório Excel

### Antes

```csharp
var workbook = new XLWorkbook();
```

### Depois, conforme a gravação

```csharp
using var workbook = new XLWorkbook();
```

O professor mantém o restante do fluxo de geração do relatório.

A seguir, uma reconstrução do caso de uso com a modificação central desta aula:

```csharp
private const string CurrencySymbol = "€";

public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    using var workbook = new XLWorkbook();

    workbook.Author = "CashFlow";
    workbook.Style.Font.FontSize = 12;
    workbook.Style.Font.FontName = "Times New Roman";

    var worksheet = workbook.Worksheets.Add(
        month.ToString("Y"));

    InsertHeader(worksheet);

    var row = 2;

    foreach (var expense in expenses)
    {
        worksheet.Cell($"A{row}").Value = expense.Title;
        worksheet.Cell($"B{row}").Value = expense.Date;
        worksheet.Cell($"C{row}").Value =
            ConvertPaymentType(expense.PaymentType);

        worksheet.Cell($"D{row}").Value = expense.Amount;
        worksheet.Cell($"D{row}").Style.NumberFormat.Format =
            $"-{CurrencySymbol}#,##0.00";

        worksheet.Cell($"E{row}").Value = expense.Description;

        row++;
    }

    worksheet.Columns().AdjustToContents();

    var file = new MemoryStream();

    workbook.SaveAs(file);

    return file.ToArray();
}
```

O autor é ilustrativo. Os demais métodos e identificadores seguem as reconstruções das aulas anteriores. A máscara monetária foi reconstruída na aula 126.

O código mantém AdjustToContents, apesar da limitação observada anteriormente no Google Planilhas.

A gravação atual demonstra explicitamente using na variável workbook. Não é possível afirmar que ela também mostra a alteração na declaração do MemoryStream.

### Fluxo sem despesas

A condição expenses.Count == 0 permanece antes da criação do workbook.

Se não existem despesas, o método retorna um array vazio e nenhum workbook é criado para ser descartado.

### Fluxo com despesas

O workbook é criado, preenchido e serializado.

A expressão file.ToArray() é avaliada para produzir o resultado. Ao sair do escopo, o workbook é descartado antes que o método conclua seu retorno.

O controller continua recebendo um byte[] e decide entre File e NoContent, sem precisar possuir o workbook.

Não existe alteração no contrato HTTP nesta aula.

## 13. Descarte do MemoryStream e propriedade dos recursos

**Complemento técnico:** MemoryStream também implementa IDisposable por meio da hierarquia de Stream.

Como a estratégia atual retorna um array de bytes, é possível aplicar using aos dois objetos criados no caso de uso:

```csharp
using var workbook = new XLWorkbook();

// Criar worksheet, cabeçalho e dados.

using var file = new MemoryStream();

workbook.SaveAs(file);

return file.ToArray();
```

Essa proposta não deve ser confundida com a alteração efetivamente demonstrada, que é using no XLWorkbook.

### Ordem de descarte

Recursos declarados com using no mesmo escopo são descartados na ordem inversa de suas declarações.

Nesse exemplo:

1. O MemoryStream é descartado.
2. O XLWorkbook é descartado.

### Por que o array continua utilizável?

ToArray cria um array com uma cópia do conteúdo do MemoryStream.

O retorno desse array não depende de manter o stream aberto.

A sequência é:

```text
SaveAs(file)
      |
      v
file.ToArray()
      |
      v
byte[] independente
      |
      v
Dispose de file e workbook
      |
      v
Controller recebe o array
```

MemoryStream trabalha com um buffer gerenciado. Seu descarte não implica que o buffer seja imediatamente removido da memória ou zerado.

### Cuidado ao devolver o próprio stream

Se a arquitetura fosse alterada para retornar um Stream ao controller, seria incorreto descartá-lo antes que o framework concluísse a leitura.

Exemplo conceitual de desenho inadequado:

```csharp
public Stream Generate()
{
    using var file = new MemoryStream();

    // Escrever o documento.

    return file;
}
```

O stream seria descartado ao sair do método, antes de o consumidor utilizá-lo.

Em um contrato que devolve streams, a responsabilidade pelo descarte precisa ser explicitamente transferida e respeitada pelo consumidor.

O CashFlow utiliza byte[] nesta etapa; essa alteração arquitetural não foi realizada.

## 14. Complementos técnicos e boas práticas

### 14.1 Quem cria e quem descarta

Como regra de propriedade de recursos, uma classe deve gerenciar os recursos que possui, respeitando o contrato da API utilizada.

Um objeto criado localmente para uso temporário costuma ser um candidato a using.

Um objeto recebido de um container de injeção de dependência, por outro lado, geralmente deve ser descartado pelo container que controla seu ciclo de vida.

No CashFlow, o workbook é criado localmente pelo caso de uso. O repositório é recebido por DI.

Não se deve aplicar Dispose indiscriminadamente a dependências injetadas sem conhecer a responsabilidade pelo seu ciclo de vida.

### 14.2 Evitar descarte duplicado desnecessário

```csharp
using var workbook = new XLWorkbook();

// ...

// Não é necessário chamar workbook.Dispose() aqui.
```

O using já organiza o descarte.

Implementações bem projetadas de IDisposable normalmente toleram chamadas repetidas, mas isso não é motivo para chamar Dispose redundantemente nem para continuar usando um objeto descartado.

### 14.3 A declaração using não é reatribuível

Uma variável local declarada com using não pode ser reatribuída livremente durante seu escopo.

Isso ajuda a preservar a identidade do objeto cujo descarte será gerenciado.

Não significa que o objeto seja imutável: o workbook continua podendo receber worksheets, valores e estilos.

### 14.4 using e operações assíncronas

A presença de um método async não impede o uso de IDisposable.

O caso de uso pode utilizar await na consulta do repositório e, depois, utilizar uma declaração using para o workbook.

Existe também IAsyncDisposable, com DisposeAsync e a sintaxe await using, introduzida no C# 8.0, para recursos com descarte assíncrono apropriado.

Esses conceitos são complementares. A gravação demonstra o descarte síncrono do XLWorkbook.

### 14.5 Referências para aprofundamento

- Documentação do C# — using statement: https://learn.microsoft.com/pt-br/dotnet/csharp/language-reference/statements/using
- Documentação .NET — IDisposable: https://learn.microsoft.com/pt-br/dotnet/api/system.idisposable
- Documentação .NET — padrão Dispose: https://learn.microsoft.com/pt-br/dotnet/standard/garbage-collection/implementing-dispose
- Repositório da biblioteca: https://github.com/ClosedXML/ClosedXML

São referências gerais para estudo; não pressupõem uma versão específica do pacote instalado na gravação.

## 15. Exercícios e perguntas de revisão

### Exercício 1 — diretiva ou declaração?

Identifique a finalidade de cada linha.

```csharp
using ClosedXML.Excel;

using var workbook = new XLWorkbook();
```

**Resposta:** a primeira é uma diretiva de namespace; a segunda gerencia o descarte do recurso.

### Exercício 2 — sintaxe tradicional

Reescreva a declaração moderna utilizando um bloco tradicional.

```csharp
using (var workbook = new XLWorkbook())
{
    // Utilizar o workbook dentro do bloco.
}
```

### Exercício 3 — interface

Qual método IDisposable fornece?

```csharp
void Dispose();
```

### Exercício 4 — exceção

Se ocorrer uma exceção durante o preenchimento da planilha, a declaração using continua organizando o descarte?

**Resposta:** sim, durante a propagação ordinária da exceção para fora do escopo, desde que o recurso tenha sido inicializado. Isso não significa que using trate a exceção.

### Exercício 5 — retorno

Explique por que é possível retornar file.ToArray() e descartar file na saída do método.

**Resposta:** ToArray produz um array independente do stream; a avaliação da expressão de retorno ocorre antes do descarte.

### Exercício 6 — Garbage Collector

Dispose força a coleta de lixo e libera imediatamente toda a RAM do objeto?

**Resposta:** não. Ele encerra o uso de recursos conforme o contrato do tipo. A recuperação de memória gerenciada é responsabilidade do Garbage Collector.

### Exercício 7 — escopo

```csharp
public void Execute()
{
    {
        using var resource = new ExampleResource();
        Console.WriteLine("Dentro");
    }

    Console.WriteLine("Fora");
}
```

Com o ExampleResource didático, o descarte ocorre antes de imprimir "Fora", pois o escopo da declaração termina no bloco interno.

### Perguntas de revisão

1. Qual diferença o professor identifica no exemplo do GitHub?
2. Qual alteração é feita na criação do XLWorkbook?
3. Para que serve a diretiva using ClosedXML.Excel?
4. Para que serve using var?
5. Qual interface o XLWorkbook implementa?
6. Qual método IDisposable disponibiliza?
7. O que são recursos não gerenciados?
8. Implementar IDisposable prova que o objeto possui diretamente recursos não gerenciados?
9. Por que chamar Dispose manualmente no fim de um método pode ser insuficiente?
10. Qual construção da linguagem fundamenta o descarte na saída do escopo?
11. Qual diferença existe entre using statement e using declaration?
12. Em qual versão do C# foi introduzida a declaração using?
13. A sintaxe tradicional foi descontinuada?
14. O descarte acontece no último acesso à variável ou na saída do escopo?
15. O que ocorre quando há return dentro do escopo?
16. using captura automaticamente exceções?
17. Qual a diferença entre Garbage Collector e Dispose?
18. Dispose significa apagar imediatamente o conteúdo da memória?
19. Por que o workbook pode ser descartado antes de o controller enviar os bytes?
20. O que aconteceria se o método devolvesse um stream já descartado?
21. Em qual ordem dois recursos using são descartados no mesmo escopo?
22. Por que não descartar arbitrariamente serviços recebidos por DI?
23. O professor demonstra um benchmark de performance?
24. A gravação confirma explicitamente using no MemoryStream?
25. Qual etapa do projeto é encerrada pelo professor?

## 16. Resumo e mapa mental

| Conceito | Síntese |
|---|---|
| IDisposable | Interface de descarte |
| Dispose | Operação de liberação/encerramento de recursos |
| using de namespace | Facilita referências a tipos |
| using statement | Delimita descarte com bloco explícito |
| using declaration | Descarta na saída do escopo existente |
| C# 8.0 | Introdução de using declaration |
| try/finally | Modelo conceitual de execução do descarte |
| Garbage Collector | Administra recuperação de memória gerenciada |
| XLWorkbook | Objeto cuja declaração recebe using nesta aula |
| MemoryStream | Também descartável; uso adicional apresentado como complemento |
| Relatório | Etapa Excel encerrada pelo professor |
| Desempenho | Sem medição quantitativa |

```text
                   AULA 127 — USING E IDISPOSABLE
                                   |
                       Palavra reservada using
                                   |
                   +---------------+---------------+
                   |                               |
              Namespace                    Gerenciar recursos
                   |                               |
         using ClosedXML.Excel            XLWorkbook : IDisposable
                                                   |
                                              Dispose()
                                                   |
                              +--------------------+--------------------+
                              |                                         |
                        using (...) { }                          using var ...
                              |                                         |
                         Fim do bloco                           Fim do escopo
                              |                                         |
                              +--------------------+--------------------+
                                                   |
                                         Descarte determinístico
                                                   |
                                 Independente da coleta futura do GC
                                                   |
                                 Relatório Excel: etapa encerrada
```

**Conclusão:** a aula 127 introduz o gerenciamento determinístico de recursos no contexto do relatório Excel. O professor identifica IDisposable na definição de XLWorkbook e substitui sua declaração comum por using var, automatizando a chamada de Dispose na saída do escopo. A sintaxe moderna evita um nível adicional de aninhamento quando o recurso deve permanecer disponível até o final do método. Os complementos distinguem descarte de coleta de lixo, explicam a relação com try/finally e mostram como aplicar o mesmo padrão ao MemoryStream sem invalidar o byte[] retornado. O professor encerra a etapa de Excel, sem realizar novo teste ou benchmark nesta gravação.
