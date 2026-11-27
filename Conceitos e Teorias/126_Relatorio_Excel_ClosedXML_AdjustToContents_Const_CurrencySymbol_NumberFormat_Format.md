# Aula 126 — Relatório Excel (parte 9): AdjustToContents, constantes e formatação monetária com ClosedXML

> **Continuação da aula 125:** o relatório Excel já contém os cabeçalhos localizados e as despesas do período, preenchidas por foreach. O arquivo é serializado em memória e devolvido pela API.
>
> **Objetivo:** melhorar a apresentação do relatório, ajustando a largura das colunas ao conteúdo e configurando um formato monetário para os valores das despesas.
>
> **Resultado demonstrado:** a formatação dos valores funcionou após o download e a importação no Google Planilhas. O ajuste automático das colunas não apresentou o resultado esperado naquele ambiente. O professor manteve a chamada AdjustToContents.
>
> **Fidelidade:** os trechos foram reconstruídos a partir da narração. A máscara de formatação é representada por -€#,##0.00, coerente com os componentes descritos. A grafia da constante CurrencySymbol e da variável row segue a reconstrução didática. A versão do ClosedXML, o número da issue mencionada e a causa exata do problema de largura não foram informados. Os complementos técnicos não são alterações implementadas pelo professor.

## Índice

1. Recapitulação e objetivo
2. Problemas visuais da planilha
3. Ajuste automático das colunas
4. Worksheet versus Workbook
5. Constantes com const
6. Magic numbers e magic strings
7. Símbolo monetário e modelo de domínio
8. NumberFormat.Format
9. Anatomia da máscara monetária
10. Diferença entre valor e formato
11. Código consolidado do caso de uso
12. Teste de geração e importação
13. Limitação do ajuste automático e software livre
14. Complementos técnicos e boas práticas
15. Exercícios e revisão
16. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

Nas aulas anteriores, o CashFlow construiu um fluxo de exportação mensal de despesas.

| Aula | Etapa |
|---|---|
| 118 | Endpoint GET e retorno de arquivo com File |
| 119 | DateOnly e FromHeader |
| 120 | ClosedXML, XLWorkbook e IXLWorksheet |
| 121 | Cabeçalhos localizados e estilos |
| 122 | MemoryStream, SaveAs e ToArray |
| 123 | FilterByMonth e ordenação LINQ |
| 124 | Testes do filtro mensal e HTTP 204 |
| 125 | Preenchimento das despesas e conversão de PaymentType |
| 126 | Formatação monetária e tentativa de ajuste automático |

O relatório já é funcional: o caso de uso consulta o banco, constrói um workbook, preenche a planilha, serializa o documento e devolve os bytes ao controller.

A aula atual não altera o contrato HTTP, não modifica a consulta ao banco e não introduz novos campos na entidade Expense.

O foco é a apresentação das informações existentes.

## 2. Problemas visuais da planilha

No teste da aula 125, o professor identificou dois problemas:

1. As colunas apresentavam larguras semelhantes, mesmo quando algumas células continham textos mais longos.
2. Os valores monetários apareciam sem um formato consistente, por exemplo, 50, 20 e 100,5.

O primeiro problema exige redimensionamento manual para visualizar os textos completos.

O segundo dificulta a leitura financeira: não existe símbolo monetário explícito, indicação visual de saída e quantidade consistente de casas decimais.

### Resultado desejado — exemplo ilustrativo

| Valor numérico | Apresentação pretendida |
|---:|---|
| 50 | -€50,00 |
| 20 | -€20,00 |
| 100,5 | -€100,50 |
| 70 | -€70,00 |

As representações acima são exemplos em uma convenção que utiliza vírgula decimal. A exibição literal depende do aplicativo e das configurações regionais. A máscara escrita no código utiliza a sintaxe de formato numérico do Excel.

O objetivo é melhorar a representação do valor, sem substituir os dados do relatório por textos formatados.

## 3. Ajuste automático das colunas

O professor consulta a documentação do ClosedXML e acrescenta a seguinte chamada após o foreach:

```csharp
worksheet.Columns().AdjustToContents();
```

### O que Columns faz?

O método obtém uma coleção de colunas da worksheet.

### O que AdjustToContents faz?

A operação busca ajustar a largura das colunas de acordo com o conteúdo das células.

A ideia é produzir uma largura suficiente para apresentar os valores e os cabeçalhos, respeitando as características da fonte e do texto.

### Por que colocar depois do foreach?

O cálculo depende do conteúdo existente na planilha.

Antes de preencher as despesas, a worksheet possui somente os cabeçalhos. Se a largura for calculada nesse momento, textos mais longos introduzidos posteriormente podem ficar truncados.

Por isso, a sequência é:

```text
Inserir cabeçalhos
        |
        v
Preencher todas as despesas
        |
        v
Aplicar formatos necessários
        |
        v
Ajustar largura das colunas
        |
        v
Salvar o workbook
```

O ajuste deve ser realizado antes da serialização para que as propriedades calculadas possam integrar o arquivo gerado.

**Resultado observado:** apesar de essa ser a chamada demonstrada na documentação, a importação no Google Planilhas não apresentou as larguras esperadas. O professor não determinou a causa.

## 4. Worksheet versus Workbook

O instrutor enfatiza que o ajuste precisa ser realizado em worksheet, e não em workbook.

```csharp
var workbook = new XLWorkbook();

var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));
```

| Objeto | Responsabilidade |
|---|---|
| XLWorkbook | Representa o documento completo |
| IXLWorksheet | Representa uma aba do documento |
| Columns() | Seleciona colunas da aba |
| AdjustToContents() | Ajusta suas dimensões |
| SaveAs() | Serializa o workbook |

A largura de uma coluna é uma propriedade da planilha individual, não uma dimensão única compartilhada por todo o workbook.

Caso existissem diversas worksheets, cada uma poderia exigir sua própria configuração.

Nesta aula, o CashFlow continua utilizando uma única aba, nomeada conforme o período recebido.

## 5. Constantes com const

Para definir o símbolo monetário, o professor cria uma constante privada na classe.

```csharp
private const string CurrencySymbol = "€";
```

A capitalização do identificador é ilustrativa. O instrutor chama a constante de currency symbol.

### Elementos da declaração

| Elemento | Significado |
|---|---|
| private | Restringe o acesso direto à classe que declara o membro |
| const | Declara um valor constante conhecido em tempo de compilação |
| string | Tipo textual |
| CurrencySymbol | Identificador |
| "€" | Valor da constante |

Uma constante não pode receber um novo valor depois de sua declaração.

```csharp
private const string CurrencySymbol = "€";

// Uma tentativa de atribuir outro valor não compila:
// CurrencySymbol = "R$";
```

A atribuição comentada é apenas uma ilustração da restrição.

### Constantes são implicitamente estáticas

Uma constante declarada como membro de classe pertence ao tipo e não a uma instância específica.

Não é necessário nem permitido acrescentar static à declaração const.

### const versus readonly — complemento técnico

| Característica | const | readonly |
|---|---|---|
| Momento de definição | Tempo de compilação | Inicialização ou construtor |
| Pode receber valor calculado em execução? | Não, salvo expressões constantes avaliáveis na compilação | Sim |
| Pode variar entre instâncias? | Não | Sim, em campo de instância |
| Exemplos | Literais e expressões constantes | Dependências injetadas e valores inicializados no construtor |

Para uma constante textual fixa utilizada em um exemplo didático, const é adequado.

Para uma moeda definida por configuração, usuário ou despesa, const não representa a variabilidade necessária.

## 6. Magic numbers e magic strings

O professor recomenda evitar valores mágicos espalhados pelo código.

Um magic number ou magic string é um literal cujo significado não é suficientemente evidente no contexto ou que representa uma regra repetida sem um nome explicativo.

### Exemplo ilustrativo

```csharp
cell.Style.NumberFormat.Format = "-€#,##0.00";
```

Em uma aplicação que repete o símbolo monetário em diferentes métodos, a alteração exigiria localizar todas as ocorrências.

A constante nomeada melhora a identificação da finalidade:

```csharp
private const string CurrencySymbol = "€";

cell.Style.NumberFormat.Format =
    $"-{CurrencySymbol}#,##0.00";
```

O símbolo possui um significado centralizado.

### Precisão técnica

Nem todo literal precisa ser transformado em constante.

Por exemplo, a linha 2 utilizada para iniciar o preenchimento de despesas pode ser suficientemente compreensível em um contexto pequeno, embora também possa ser nomeada quando isso melhorar o design.

O objetivo não é criar uma constante para cada número ou string, mas tornar significados e regras relevantes explícitos e evitar duplicação desnecessária.

A aula demonstra a constante como recurso didático.

## 7. Símbolo monetário e modelo de domínio

O professor utiliza o euro como símbolo fixo:

```csharp
private const string CurrencySymbol = "€";
```

Ele reconhece que uma aplicação real poderia registrar despesas em diferentes moedas e recuperar a moeda associada ao registro, inclusive a partir do banco de dados.

Essa possibilidade é apenas explicada: a aula não cria uma propriedade Currency, não modifica a entidade Expense e não executa uma migração.

### Complemento: símbolo versus identificação de moeda

O símbolo visual não identifica necessariamente uma moeda de forma inequívoca.

Por exemplo, o símbolo $ pode ser utilizado por diferentes moedas.

Uma arquitetura com suporte a múltiplas moedas poderia armazenar um código como BRL, EUR ou USD e escolher o símbolo e o formato apropriados para apresentação.

Também é importante distinguir a cultura do usuário da moeda da despesa.

Uma interface em português brasileiro pode apresentar uma despesa denominada em euros. Selecionar pt-BR não transforma automaticamente EUR em BRL.

A formatação monetária não é uma conversão cambial.

## 8. NumberFormat.Format

O professor acessa a célula da coluna D, que já recebe o valor decimal da despesa.

```csharp
worksheet.Cell($"D{row}").Value = expense.Amount;
```

Em seguida, configura sua apresentação numérica.

```csharp
worksheet.Cell($"D{row}").Style.NumberFormat.Format =
    $"-{CurrencySymbol}#,##0.00";
```

A máscara é reconstruída conforme os componentes narrados: sinal negativo, símbolo interpolado, agrupamento de milhares e duas casas decimais.

### Cadeia de acesso

| Elemento | Responsabilidade |
|---|---|
| Cell($"D{row}") | Seleciona a célula do valor da despesa |
| Style | Acessa as propriedades de estilo |
| NumberFormat | Acessa a configuração de formato numérico |
| Format | Recebe a máscara de apresentação |

O formato é uma string interpretada segundo as regras de formatação de células do Excel.

Não é a mesma coisa que chamar decimal.ToString com um formato .NET.

### Interpolação de strings

O professor reaproveita a sintaxe estudada na aula 125.

```csharp
$"-{CurrencySymbol}#,##0.00"
```

Como CurrencySymbol contém €, a expressão produz:

```text
-€#,##0.00
```

A interpolação acontece em C#. A interpretação dos marcadores numéricos acontece no formato da planilha.

## 9. Anatomia da máscara monetária

Considere a máscara reconstruída:

```text
-€#,##0.00
```

| Componente | Função |
|---|---|
| - | Prefixo visual de sinal negativo na seção da máscara |
| € | Símbolo monetário |
| # | Posição opcional de dígito |
| , | Agrupamento de milhares nessa posição |
| 0 | Posição obrigatória de dígito |
| . | Marcador da parte decimal na sintaxe do formato |
| 00 | Duas posições decimais obrigatórias |

### Diferença entre # e 0

O marcador # representa uma posição de dígito opcional.

O marcador 0 força a exibição de um dígito, inclusive zero quando necessário.

Assim, o trecho 0.00 permite exibir um valor inferior a um com zero antes do separador decimal e sempre duas posições decimais.

### Exemplos de exibição pretendida

| Valor | Exibição em convenção pt-BR |
|---:|---|
| 0 | -€0,00 |
| 2,24 | -€2,24 |
| 50 | -€50,00 |
| 100,5 | -€100,50 |
| 1234,5 | -€1.234,50 |

São exemplos didáticos, não uma reprodução das células observadas durante a gravação.

O formato pode arredondar a representação exibida para duas casas decimais, sem necessariamente modificar a precisão do valor subjacente.

### Cultura e aplicativo

A string de formato utiliza os marcadores . e , da sintaxe correspondente. Isso não exige que o usuário final veja exatamente ponto decimal e vírgula de agrupamento.

O Excel e outros aplicativos podem interpretar e apresentar esses marcadores segundo configurações regionais.

A aula não testa diferentes culturas nem diferentes aplicativos de planilhas.

## 10. Diferença entre valor e formato

Um ponto importante do código é a separação das duas atribuições:

```csharp
worksheet.Cell($"D{row}").Value = expense.Amount;

worksheet.Cell($"D{row}").Style.NumberFormat.Format =
    $"-{CurrencySymbol}#,##0.00";
```

Value recebe o conteúdo numérico.

NumberFormat.Format recebe a configuração visual.

### Sinal negativo visual não altera o valor

O professor deseja representar a saída de dinheiro com um sinal negativo.

Entretanto, ele não multiplica Amount por -1 e não altera o registro persistido.

O formato acrescenta a indicação visual desejada a valores positivos, mas não transforma esses valores em números negativos para cálculos.

Exemplo conceitual:

```text
Valor armazenado na célula: 70
Representação desejada:    -€70,00
Valor utilizado em SUM:    70
```

Portanto, se uma fórmula somar as despesas, os valores positivos continuarão contribuindo positivamente para o resultado.

Isso pode ser apropriado se o domínio representa o montante da despesa como positivo e deseja apenas uma convenção visual de saída.

Caso o relatório deva representar lançamentos contábeis com sinal matemático, será necessário definir outra estratégia.

### Comparação

| Estratégia | Conteúdo da célula | Consequência |
|---|---|---|
| Formato com prefixo negativo | Número original | Apenas aparência |
| Atribuir -expense.Amount | Número negativo | Altera o sinal do valor exportado |
| Atribuir texto formatado | String | Pode prejudicar cálculos e ordenação numérica |

A primeira abordagem corresponde à aula.

A segunda e a terceira são comparações complementares, não implementações do professor.

## 11. Código consolidado do caso de uso

O trecho seguinte integra as mudanças da aula ao código construído anteriormente.

```csharp
private const string CurrencySymbol = "€";

public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    var workbook = new XLWorkbook();

    workbook.Author = "CashFlow";
    workbook.Style.Font.FontSize = 12;
    workbook.Style.Font.FontName = "Times New Roman";

    var worksheet = workbook.Worksheets.Add(
        month.ToString("Y"));

    InsertHeader(worksheet);

    var row = 2;

    foreach (var expense in expenses)
    {
        worksheet.Cell($"A{row}").Value =
            expense.Title;

        worksheet.Cell($"B{row}").Value =
            expense.Date;

        worksheet.Cell($"C{row}").Value =
            ConvertPaymentType(expense.PaymentType);

        worksheet.Cell($"D{row}").Value =
            expense.Amount;

        worksheet.Cell($"D{row}").Style.NumberFormat.Format =
            $"-{CurrencySymbol}#,##0.00";

        worksheet.Cell($"E{row}").Value =
            expense.Description;

        row++;
    }

    worksheet.Columns().AdjustToContents();

    var file = new MemoryStream();

    workbook.SaveAs(file);

    return file.ToArray();
}
```

O nome do autor CashFlow é ilustrativo, conforme as aulas anteriores. A máscara e os identificadores são reconstruídos da narração.

InsertHeader e ConvertPaymentType permanecem os métodos auxiliares documentados nas aulas 121 e 125.

A tradução dos tipos de pagamento foi deixada como exercício na aula 125. A aula 126 não demonstra explicitamente sua conclusão.

### Pontos modificados nesta aula

- Inclusão da constante CurrencySymbol.
- Configuração de NumberFormat.Format na célula da coluna D.
- Chamada Columns().AdjustToContents() após o foreach.

A estratégia de consulta, o tratamento de lista vazia e o retorno binário permanecem os mesmos.

## 12. Teste de geração e importação

O professor executa a API com F5 e solicita um relatório para o mês 5, isto é, maio.

A narração não explicita novamente o ano da entrada nesse trecho. O contexto anterior utiliza 2024, mas o dado confirmado nesta aula é o mês solicitado.

### Download

O Swagger gera o arquivo e apresenta Download File.

O navegador Opera já possuía o arquivo report.xlsx baixado anteriormente. Para evitar substituir o arquivo anterior, salva o novo download com um sufixo numérico entre parênteses.

O professor seleciona esse novo arquivo, identificado na narração como report com (1).

O nome local atribuído pelo navegador não representa uma alteração no nome configurado pelo controller.

### Importação

O professor abre o Google Planilhas e realiza o procedimento já utilizado nas aulas anteriores:

1. Arquivo > Importar.
2. Fazer upload.
3. Procurar o novo arquivo.
4. Selecionar substituir a planilha.
5. Importar os dados.

### Resultado da formatação monetária

O instrutor considera que o formato dos números funcionou corretamente.

A transcrição menciona um valor 2,24 durante a inspeção, mas não fornece uma reprodução inequívoca de todos os caracteres exibidos na célula. O objetivo visual descrito é o sinal negativo, o símbolo de euro e duas casas decimais.

### Resultado das larguras

As colunas continuam exigindo ajuste manual.

O professor utiliza duplo clique nas bordas das colunas para ajustá-las no Google Planilhas.

| Verificação | Resultado observado |
|---|---|
| Geração do XLSX | Funcionou |
| Download pelo Swagger | Funcionou |
| Importação no Google Planilhas | Funcionou |
| Formatação monetária | Aprovada pelo professor |
| Ajuste automático das colunas | Não apresentou o efeito esperado |

Não houve teste no Microsoft Excel instalado localmente, pois o professor informa que não possui esse aplicativo.

## 13. Limitação do ajuste automático e software livre

O professor considera duas possibilidades para o problema de largura: um comportamento da biblioteca ou uma limitação relacionada à importação no Google Planilhas.

Ele menciona a existência de uma issue no repositório GitHub da biblioteca, aberta por pessoas que enfrentavam comportamento semelhante.

A transcrição não informa número, URL, versão afetada ou resolução dessa issue.

Por isso, não é possível identificar com segurança qual problema específico foi consultado, nem afirmar que se trata de um bug ainda existente em uma versão atual.

### Decisão do professor

O instrutor mantém:

```csharp
worksheet.Columns().AdjustToContents();
```

Ele considera aceitável continuar o estudo com a biblioteca, mesmo diante dessa limitação observada.

O possível funcionamento no Microsoft Excel ou em uma versão futura do pacote é levantado como hipótese, não como teste realizado.

### Contribuição para software livre

A aula incentiva o aluno a estudar a biblioteca e, eventualmente, contribuir com correções no projeto open source.

Uma contribuição pode incluir:

- Reproduzir um problema.
- Preparar um exemplo mínimo.
- Identificar a causa.
- Criar testes.
- Propor uma correção em pull request.

Esse procedimento é um complemento didático; o professor não abre uma issue nem envia uma correção durante a gravação.

### Precisão sobre licenciamento

A licença MIT do ClosedXML foi discutida na aula 120.

O fato de uma biblioteca ser gratuita ou open source não demonstra que seus problemas sejam inevitáveis, assim como o pagamento por uma biblioteca não garante ausência de falhas.

A avaliação deve considerar requisitos, compatibilidade, manutenção e resultados de testes.

## 14. Complementos técnicos e boas práticas

Os exemplos abaixo são aprofundamentos separados da implementação original.

### 14.1 Reutilizar a referência à célula

É possível evitar duas buscas pela mesma célula:

```csharp
var amountCell = worksheet.Cell($"D{row}");

amountCell.Value = expense.Amount;
amountCell.Style.NumberFormat.Format =
    $"-{CurrencySymbol}#,##0.00";
```

Esse código melhora a organização, mas a gravação utiliza o acesso à célula para configurar o valor e o formato.

### 14.2 Aplicar formato a um intervalo

Como os valores ficam na coluna D, é possível considerar aplicar a mesma máscara de maneira coletiva, após preencher os registros.

```csharp
worksheet.Range($"D2:D{row - 1}")
    .Style.NumberFormat.Format =
        $"-{CurrencySymbol}#,##0.00";
```

O exemplo pressupõe que existe pelo menos uma despesa e que row foi incrementado ao final de cada iteração.

O intervalo termina em row - 1 porque row aponta para a próxima linha disponível ao sair do foreach.

A aula não realiza essa refatoração.

### 14.3 Investigar o ajuste de largura

Para diagnosticar o comportamento, seria útil separar as possíveis causas:

- Confirmar a versão instalada do ClosedXML.
- Verificar a disponibilidade das fontes utilizadas no ambiente de geração.
- Inspecionar os valores Width antes e depois do ajuste.
- Salvar um workbook mínimo para reprodução.
- Comparar a abertura em aplicativos diferentes.
- Inspecionar as propriedades de largura gravadas no XLSX.

O cálculo de largura depende da medição do texto, não simplesmente da quantidade de caracteres. Letras com larguras diferentes, tamanho de fonte e negrito podem afetar o resultado.

Essas verificações são sugestões, não resultados obtidos na aula.

### 14.4 Configuração explícita de largura

Uma alternativa em cenários que exigem apresentação mais previsível é definir larguras adequadas ao template.

```csharp
worksheet.Column("A").Width = 25;
worksheet.Column("B").Width = 20;
worksheet.Column("C").Width = 26;
worksheet.Column("D").Width = 18;
worksheet.Column("E").Width = 45;
```

As larguras são exemplos arbitrários de estudo e não correspondem à planilha do professor.

Essa solução também precisaria ser testada nos aplicativos de destino. Não há garantia de que resolva o comportamento de importação observado.

### 14.5 Valores negativos e formatos de múltiplas seções

Formatos personalizados do Excel podem possuir seções separadas por ponto e vírgula para distinguir valores positivos, negativos, zero e texto.

A máscara utilizada no curso tem uma única seção com um prefixo negativo.

Ela foi criada para apresentar montantes positivos como despesas, mas valores originalmente negativos podem exigir tratamento específico para evitar sinal duplicado ou apresentação ambígua, conforme o aplicativo.

Quando o sinal deve refletir o valor matemático, uma possibilidade é armazenar números com o sinal adequado e usar formatos explícitos para positivos e negativos.

Esse é um ponto de design contábil que deve ser decidido conforme o significado de Amount. A gravação não altera os sinais persistidos nem testa valores negativos.

### 14.6 Precisão monetária

A exibição de duas casas decimais não define, por si só, a precisão de armazenamento e cálculo.

Uma despesa com mais casas decimais pode ser exibida arredondada sem que o número subjacente seja alterado pelo estilo.

Além disso, o formato XLSX representa números com as limitações de precisão numérica do Excel. Valores decimal do C# não devem ser considerados automaticamente preservados com toda a precisão arbitrária do tipo quando exportados para uma célula numérica.

A aula não realiza testes de precisão ou arredondamento financeiro.

### 14.7 Múltiplas moedas

Em um relatório com diferentes moedas, é importante identificar a moeda de cada valor e evitar somar valores de moedas distintas como se fossem equivalentes.

A constante de euro é adequada apenas para o exemplo didático ou para um contrato explicitamente limitado a uma única moeda.

O professor já reconhece essa limitação na gravação.

### 14.8 Referências para estudo

- ClosedXML: https://github.com/ClosedXML/ClosedXML
- Documentação ClosedXML: https://docs.closedxml.io/
- Microsoft Learn — referência de const: https://learn.microsoft.com/pt-br/dotnet/csharp/language-reference/keywords/const
- Microsoft Learn — referência de readonly: https://learn.microsoft.com/pt-br/dotnet/csharp/language-reference/keywords/readonly

Esses endereços são referências complementares gerais; não identificam a issue mencionada pelo professor.

## 15. Exercícios e revisão

### Exercício 1 — ajustar as colunas

Qual instrução foi acrescentada depois do foreach?

```csharp
worksheet.Columns().AdjustToContents();
```

### Exercício 2 — definir uma constante

Declare uma constante privada para representar o símbolo do euro.

```csharp
private const string CurrencySymbol = "€";
```

### Exercício 3 — formatar o valor

```csharp
worksheet.Cell($"D{row}").Style.NumberFormat.Format =
    $"-{CurrencySymbol}#,##0.00";
```

Explique a função dos marcadores #, 0 e .00.

### Exercício 4 — valor versus aparência

Se uma célula contém o número positivo 50 e recebe um formato que exibe -€50,00, qual valor será utilizado em uma soma?

**Resposta:** 50. O formato visual não altera o sinal matemático do valor.

### Exercício 5 — interpretar a limitação

O fato de AdjustToContents não funcionar visualmente no Google Planilhas comprova que o ClosedXML possui um bug?

**Resposta:** não. O teste demonstra o resultado naquele ambiente, mas não isola a causa.

### Exercício 6 — adaptar para outra moeda

Para fins de estudo, substitua o símbolo de euro por outro símbolo na constante. Explique por que essa mudança não representa uma conversão cambial.

### Exercício 7 — evitar repetição

Refatore as duas chamadas de Cell para utilizar uma variável local amountCell, conforme o exemplo complementar.

### Perguntas de revisão

1. Quais problemas visuais foram identificados na aula 125?
2. Qual método permite ajustar a largura das colunas?
3. Por que chamar AdjustToContents após o preenchimento?
4. Qual diferença existe entre workbook e worksheet?
5. Qual constante foi criada?
6. O que const significa em C#?
7. Uma constante pode receber outro valor durante a execução?
8. Qual diferença existe entre const e readonly?
9. O que são magic numbers e magic strings?
10. Por que centralizar o símbolo monetário?
11. Por que uma constante de euro não atende necessariamente uma aplicação multimoeda?
12. Qual propriedade da célula controla o formato numérico?
13. Qual coluna contém Amount?
14. O que a interpolação de CurrencySymbol produz?
15. Qual é a diferença entre # e 0 na máscara?
16. O que representa .00?
17. O formato modifica o valor numérico da célula?
18. O sinal negativo visual muda o resultado de uma soma?
19. A aula implementa conversão cambial?
20. Qual mês foi solicitado no teste?
21. Qual aplicativo foi utilizado para importar o XLSX?
22. Por que o navegador acrescentou um sufixo ao nome local do arquivo?
23. Qual melhoria funcionou no teste?
24. Qual melhoria não apresentou o efeito esperado?
25. O professor testou o arquivo no Microsoft Excel?
26. O número da issue mencionada foi fornecido?
27. Qual decisão o professor toma quanto à chamada AdjustToContents?
28. Como uma contribuição open source poderia ajudar a investigar o problema?

## 16. Resumo e mapa mental

| Elemento | Resultado |
|---|---|
| Constante | private const string CurrencySymbol = "€" |
| Célula de valor | D + número da linha |
| Conteúdo | expense.Amount |
| Formato | -€#,##0.00 (máscara reconstruída) |
| Propriedade | Style.NumberFormat.Format |
| Casas decimais | Duas |
| Sinal | Prefixo visual negativo |
| Agrupamento | Milhares |
| Ajuste de largura | worksheet.Columns().AdjustToContents() |
| Posicionamento do ajuste | Após foreach e antes de SaveAs |
| Teste | Mês 5, download e importação |
| Formatação monetária | Validada |
| Ajuste de largura | Não validado no ambiente demonstrado |
| Decisão | Manter a chamada |
| Tradução de PaymentType | Não comprovada nesta aula |

```text
                   AULA 126 — ESTILIZAÇÃO DO EXCEL
                                  |
                      Relatório com despesas
                                  |
                 +----------------+----------------+
                 |                                 |
           Formato monetário                Largura de colunas
                 |                                 |
          const CurrencySymbol               worksheet.Columns()
                 |                                 |
          Style.NumberFormat.Format           AdjustToContents()
                 |                                 |
           -€#,##0.00                        após o foreach
                 |                                 |
          Valor continua numérico                  |
                 |                                 |
                 +----------------+----------------+
                                  |
                     SaveAs + MemoryStream
                                  |
                       Download no Swagger
                                  |
                       Google Planilhas
                                  |
                +-----------------+-----------------+
                |                                   |
        Formatação aprovada                  Ajuste não observado
                |                                   |
                +-----------------+-----------------+
                                  |
                     Chamada de ajuste mantida
```

**Conclusão:** a aula 126 melhora a apresentação monetária do relatório Excel e demonstra a utilização de constantes em C#. O professor mantém o valor numérico da despesa na célula, configurando sua aparência por meio de Style.NumberFormat.Format. Também acrescenta AdjustToContents após preencher a planilha. O teste confirma a formatação monetária, mas não o ajuste automático de largura no Google Planilhas. O código é preservado e o instrutor incentiva a investigação e contribuição para projetos open source. O material distingue essa limitação observada de uma conclusão definitiva sobre a biblioteca e esclarece que o sinal visual negativo não altera o valor matemático exportado.
