# Aula 121 — Relatório Excel (parte 4): cabeçalhos localizados, estilos de células, cores e alinhamento com ClosedXML

> **Continuação da aula 120:** o caso de uso de relatório já cria um XLWorkbook, configura a fonte Times New Roman de tamanho 12 e adiciona uma worksheet com o nome do mês e ano solicitado.
>
> **Objetivo:** construir o cabeçalho da primeira linha da planilha, reutilizar o mecanismo de localização do CashFlow e aplicar estilos coletivos e individuais às células.
>
> **Resultado:** cinco cabeçalhos de A1 a E1, em negrito, com preenchimento #F5C2B6. Título, data, tipo de pagamento e descrição são centralizados; valor fica alinhado à direita.
>
> **Escopo:** o professor escreve o código do cabeçalho, mas deixa o teste do arquivo para a próxima aula. Não há download de um XLSX validado, consulta de despesas, preenchimento das linhas de dados ou cálculo de totais nesta gravação.
>
> **Fidelidade:** exemplos reconstruídos a partir da transcrição. Os nomes de chaves de recursos que não foram soletrados integralmente são ilustrativos. As traduções exemplificadas não constituem reprodução literal dos arquivos .resx. Complementos técnicos são indicados separadamente.

## Índice

1. Recapitulação e objetivo
2. Método privado InsertHeader
3. Endereçamento de células
4. Cabeçalhos e ordem das colunas
5. Recursos de tradução no Domain
6. Localização e middleware de cultura
7. Inserção dos textos com Cell().Value
8. Estilização de um conjunto de células
9. Cor de preenchimento com XLColor
10. Captura de cores com PowerToys
11. Alinhamento horizontal
12. Alinhamento de valores monetários
13. Código consolidado
14. Cuidados técnicos e limites da aula
15. Exercícios e revisão
16. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

Na aula anterior, a estrutura inicial do relatório foi preparada:

```csharp
var workbook = new XLWorkbook();

workbook.Author = "CashFlow";
workbook.Style.Font.FontSize = 12;
workbook.Style.Font.FontName = "Times New Roman";

var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));
```

CashFlow é um autor ilustrativo, conforme explicado na aula 120.

O workbook representa o documento inteiro. A worksheet representa uma aba individual do documento.

Agora, o instrutor precisa organizar a primeira linha da aba para identificar os dados que futuramente serão inseridos.

O cabeçalho não contém despesas propriamente ditas: contém os nomes das colunas.

O termo header nesta aula refere-se à primeira linha da planilha, não ao cabeçalho HTTP utilizado com FromHeader na aula 119.

## 2. Método privado InsertHeader

O professor extrai a configuração do cabeçalho para um método privado, com o objetivo de manter Execute mais organizado.

```csharp
private void InsertHeader(IXLWorksheet worksheet)
{
    // Inserir textos e estilos do cabeçalho.
}
```

### Entendendo a assinatura

| Elemento | Significado |
|---|---|
| private | O método auxilia a própria classe do caso de uso |
| void | Não devolve um valor ao chamador |
| InsertHeader | Nome da operação |
| IXLWorksheet | Interface que representa a worksheet na API do ClosedXML |
| worksheet | Referência à aba que será modificada |

O namespace da biblioteca é ClosedXML.Excel.

A transcrição pronuncia o tipo de forma imprecisa, mas a API utiliza IXLWorksheet.

### Chamada dentro de Execute

```csharp
var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));

InsertHeader(worksheet);
```

O método recebe a mesma worksheet criada anteriormente e modifica suas células.

Não precisa retornar uma nova worksheet: as modificações são realizadas no objeto existente, acessado por meio da referência recebida como argumento.

**Complemento técnico:** em C#, a referência é passada por valor por padrão. Isso não cria uma cópia independente da worksheet. Alterar o objeto acessado pela referência afeta a mesma instância; reatribuir o parâmetro local a outro objeto seria uma operação diferente.

O método é síncrono porque, nessa etapa, executa configurações da planilha em memória. O futuro caso de uso poderá continuar assíncrono por causa da consulta ao banco.

## 3. Endereçamento de células

O Excel utiliza a notação de referência A1 para identificar células.

Uma referência contém a coluna e a linha.

| Referência | Coluna | Linha |
|---|---|---:|
| A1 | A | 1 |
| B1 | B | 1 |
| C1 | C | 1 |
| D1 | D | 1 |
| E1 | E | 1 |
| A2 | A | 2 |

O professor usa o método Cell, no singular, para selecionar uma célula individual.

```csharp
worksheet.Cell("A1").Value = "Título";
```

O método obtém a célula indicada pela referência. A propriedade Value recebe o conteúdo que será armazenado nela.

O texto literal acima é a primeira forma de explicar o conceito, mas não é a escolha final da aula: o professor substitui o texto fixo por um recurso traduzível.

### Complemento: coordenadas numéricas

ClosedXML também permite acessar células por índices de linha e coluna:

```csharp
worksheet.Cell(1, 1).Value = "Título";
```

A ordem dos argumentos é linha e coluna, e os índices começam em 1.

As duas formas acima identificam A1. O professor utiliza a notação textual para construir os cabeçalhos.

## 4. Cabeçalhos e ordem das colunas

O instrutor segue o template de relatório já apresentado anteriormente no curso.

| Endereço | Informação | Alinhamento final |
|---|---|---|
| A1 | Título | Centralizado |
| B1 | Data | Centralizado |
| C1 | Tipo de pagamento | Centralizado |
| D1 | Valor | À direita |
| E1 | Descrição | Centralizado |

A descrição pode ser opcional na despesa, mas a coluna permanece presente no relatório.

### Representação ilustrativa da planilha

```text
       A          B             C             D          E
  +----------+----------+----------------+----------+-------------+
1 |  Título  |   Data   | Tipo pagamento |    Valor |  Descrição  |
  +----------+----------+----------------+----------+-------------+
2 |          |          |                |          |             |
3 |          |          |                |          |             |
  +----------+----------+----------------+----------+-------------+
```

Todos os títulos da linha 1 serão colocados em negrito e receberão o mesmo fundo.

As linhas seguintes ainda não são preenchidas nesta aula.

## 5. Recursos de tradução no Domain

Ao escrever o título da primeira célula, o professor percebe que poderia utilizar o mesmo conceito de localização já aplicado às mensagens de exceção.

Em vez de escrever diretamente:

```csharp
worksheet.Cell("A1").Value = "Título";
```

Ele prefere utilizar uma classe de recursos dedicada aos relatórios.

### Organização

A pasta Reports foi criada no projeto Domain. Nela, o instrutor preparou os arquivos de recursos com o nome ResourceReportGenerationMessages.

Representação reconstruída:

```text
CashFlow.Domain
└── Reports
    ├── ResourceReportGenerationMessages.resx
    ├── ResourceReportGenerationMessages.pt-BR.resx
    ├── ResourceReportGenerationMessages.pt-PT.resx
    └── ResourceReportGenerationMessages.fr.resx
```

A transcrição confirma o recurso padrão, português de Portugal, português do Brasil e francês. O sufixo exato da cultura francesa não é enunciado; fr acima é ilustrativo. O conteúdo literal de cada tradução também não é fornecido.

O arquivo padrão tem o modificador de acesso configurado como Public para permitir o acesso à classe gerada a partir de outro projeto, como Application.

### Por que criar outro conjunto de recursos?

Mensagens de exceção e textos de relatórios possuem finalidades diferentes.

Separá-los mantém organizados os textos utilizados para apresentar erros e aqueles utilizados na geração de documentos.

O instrutor reaproveita o mecanismo de localização, não o mesmo arquivo de mensagens de erro.

### Exemplo de chaves

| Chave ilustrativa | Texto em português brasileiro |
|---|---|
| Title | Título |
| Date | Data |
| PaymentType | Tipo de pagamento |
| Amount | Valor |
| Description | Descrição |

Title e Date são reconhecíveis na transcrição. Os demais nomes de propriedades C# são reconstruídos conforme a nomenclatura do projeto. Confira a grafia no arquivo de recursos original.

## 6. Localização e middleware de cultura

O professor lembra que a API já possui um middleware de cultura, criado para permitir mensagens de exceção traduzidas.

A proposta é aproveitar esse mecanismo também no relatório Excel, sem precisar modificar a API nesta aula.

### Fluxo conceitual

```text
Requisição
    |
    v
Culture Middleware
    |
    v
Cultura selecionada
    |
    v
ResourceReportGenerationMessages
    |
    v
Textos localizados nas células
```

### O que será traduzido?

Os cabeçalhos são textos controlados pela aplicação e, por isso, podem ser obtidos de recursos.

O professor explica expressamente que não pretende traduzir a descrição de uma despesa fornecida pelo usuário. O conteúdo original do registro será preservado.

| Informação | Tratamento pretendido |
|---|---|
| Nome das colunas | Localizado por recursos |
| Nome da aba | Formatado conforme a cultura, como na aula 120 |
| Descrição da despesa | Mantida como foi registrada |

Não há tradução automática de texto livre nesta implementação.

### Complemento técnico: duas culturas

CurrentUICulture orienta a seleção dos recursos localizados. CurrentCulture orienta a formatação de datas e números.

Para que o relatório inteiro acompanhe a cultura da requisição, o middleware deve configurar as culturas apropriadas.

A classe de recursos também precisa estar corretamente gerada e os arquivos .resx precisam possuir as chaves e culturas esperadas. Quando não existe uma tradução específica, o mecanismo de recursos pode recorrer à cultura pai ou ao recurso padrão, conforme a configuração.

A transcrição não reapresenta o código do middleware nem demonstra a abertura de relatórios em idiomas diferentes nesta aula.

## 7. Inserção dos textos com Cell().Value

O professor utiliza Cell para inserir separadamente o texto de cada coluna.

```csharp
worksheet.Cell("A1").Value =
    ResourceReportGenerationMessages.Title;

worksheet.Cell("B1").Value =
    ResourceReportGenerationMessages.Date;

worksheet.Cell("C1").Value =
    ResourceReportGenerationMessages.PaymentType;

worksheet.Cell("D1").Value =
    ResourceReportGenerationMessages.Amount;

worksheet.Cell("E1").Value =
    ResourceReportGenerationMessages.Description;
```

A estrutura reproduz o método narrado. Os identificadores de recursos não soletrados na transcrição devem ser ajustados ao projeto original.

### Por que uma atribuição por célula?

Cada célula precisa receber um texto diferente.

Já a formatação compartilhada, como negrito e fundo, pode ser aplicada a todas essas células de uma vez.

Esse é o motivo pelo qual o professor utiliza Cell para os valores individuais e Cells para determinadas propriedades de estilo.

## 8. Estilização de um conjunto de células

Depois de inserir os textos, o instrutor quer colocar todos os cabeçalhos em negrito.

### Uma primeira possibilidade

```csharp
worksheet.Cell("A1").Style.Font.Bold = true;
```

Essa instrução altera somente a célula A1.

Seria possível repetir a mesma configuração para B1, C1, D1 e E1, mas isso duplicaria código.

### Solução apresentada

O professor utiliza o método Cells, no plural, com o intervalo A1:E1.

```csharp
worksheet.Cells("A1:E1").Style.Font.Bold = true;
```

O intervalo representa todas as cinco células da primeira linha entre as colunas A e E.

| Método | Finalidade nesta aula |
|---|---|
| Cell("A1") | Selecionar uma única célula |
| Cells("A1:E1") | Selecionar um conjunto de células |
| Style.Font.Bold | Controlar o negrito |

O separador de dois-pontos indica os limites inclusivos do intervalo. A1:E1 não seleciona todas as linhas das colunas A até E, apenas as células da linha 1.

### Complemento: Range

Outra API apropriada para trabalhar com um intervalo retangular é:

```csharp
worksheet.Range("A1:E1").Style.Font.Bold = true;
```

O curso escolhe Cells. Range é uma alternativa complementar; os dois métodos retornam abstrações diferentes na biblioteca.

## 9. Cor de preenchimento com XLColor

O cabeçalho do template possui um fundo personalizado.

O professor aplica a cor ao mesmo intervalo de células.

```csharp
worksheet.Cells("A1:E1").Style.Fill.BackgroundColor =
    XLColor.FromHtml("#F5C2B6");
```

### Entendendo a cadeia de propriedades

| Elemento | Finalidade |
|---|---|
| Cells("A1:E1") | Seleção das cinco células |
| Style | Configuração visual |
| Fill | Preenchimento |
| BackgroundColor | Cor de fundo |
| XLColor.FromHtml(...) | Criação de uma representação de cor para ClosedXML |

A grafia correta do método é FromHtml, embora a transcrição se refira a HTML.

### Cores predefinidas

A classe XLColor também possui cores nomeadas.

Exemplo ilustrativo:

```csharp
worksheet.Cell("A1").Style.Fill.BackgroundColor =
    XLColor.Aqua;
```

O professor navega por algumas opções, como Aqua e Aquamarine, mas deseja reproduzir a cor específica do template.

### Cor hexadecimal escolhida

```text
#F5C2B6
```

Na notação #RRGGBB, cada par de dígitos representa um canal RGB.

| Canal | Hexadecimal | Decimal |
|---|---|---:|
| Vermelho | F5 | 245 |
| Verde | C2 | 194 |
| Azul | B6 | 182 |

A cor é um tom claro de salmão/rosa.

O valor de fundo é aplicado de A1 até E1. O professor não modifica a cor do texto nesta gravação.

### Complemento

O estilo visual não muda os valores armazenados nas células. Alterar o preenchimento não modifica os textos dos cabeçalhos.

A biblioteca também oferece outras formas de definir cores, mas a implementação do curso utiliza FromHtml com o hexadecimal.

## 10. Captura de cores com PowerToys

O professor demonstra duas maneiras de obter o código hexadecimal de uma cor.

A primeira é utilizar um seletor de cores disponível em um site. Ele não identifica o endereço do site na transcrição.

A segunda é utilizar o Microsoft PowerToys, especificamente o utilitário Color Picker.

### Procedimento

1. Instalar PowerToys e habilitar o Color Picker.
2. Pressionar Win + Shift + C, atalho padrão da ferramenta.
3. Posicionar o cursor sobre a cor desejada na tela.
4. Selecionar a cor e obter o valor no formato necessário.

A ferramenta permite trabalhar com formatos como hexadecimal, RGB e HSL. O comportamento de seleção e cópia pode ser configurado.

Referência oficial: https://learn.microsoft.com/pt-br/windows/powertoys/color-picker

### Relação com ClosedXML

O PowerToys apenas ajuda a identificar a cor de referência.

Não é uma dependência do CashFlow e não precisa estar instalado no servidor para gerar a planilha.

A cor já é informada como texto no código C#.

## 11. Alinhamento horizontal

O professor passa a configurar o alinhamento de cada cabeçalho.

Para isso, utiliza Style.Alignment.SetHorizontal.

### Centralizar o título

```csharp
worksheet.Cell("A1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Center);
```

O método recebe um valor do enum XLAlignmentHorizontalValues.

A opção Center alinha o conteúdo ao centro no eixo horizontal.

### Demais cabeçalhos centralizados

```csharp
worksheet.Cell("B1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Center);

worksheet.Cell("C1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Center);

worksheet.Cell("E1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Center);
```

### Cabeçalho do valor

```csharp
worksheet.Cell("D1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Right);
```

D1 é a exceção ao padrão de centralização.

### Horizontal versus vertical

O alinhamento horizontal controla a posição do conteúdo entre as bordas esquerda e direita da célula.

O alinhamento vertical controla a posição entre as bordas superior e inferior.

Nesta aula, o professor altera somente o alinhamento horizontal.

## 12. Alinhamento de valores monetários

O instrutor relata uma prática aprendida em seu trabalho no mercado financeiro: valores numéricos e monetários devem ficar alinhados à direita para facilitar a leitura da ordem de grandeza.

### Comparação conceitual

```text
Centralizado              Alinhado à direita
+------------+            +------------+
|    10,00   |            |       10,00|
|   100,00   |            |      100,00|
|  1.000,00  |            |    1.000,00|
| 10.000,00  |            |   10.000,00|
+------------+            +------------+
```

Quando os valores possuem uma quantidade consistente de casas decimais, o alinhamento à direita facilita a comparação visual das magnitudes.

### O que foi efetivamente configurado?

O professor alinha D1, a célula de título da coluna de valor, à direita.

Ainda não configura o alinhamento das futuras células D2, D3 e demais linhas de despesas.

### Complemento: formatação financeira

Alinhamento não é sinônimo de formato numérico.

Quando os dados forem inseridos, poderá ser apropriado armazenar Amount como valor numérico e configurar um NumberFormat adequado. Isso permite cálculos, ordenação e exibição de casas decimais sem transformar o valor em texto.

A quantidade de casas decimais, moeda e cultura devem seguir o contrato do relatório.

Nenhuma dessas configurações de valores é implementada na gravação atual.

## 13. Código consolidado

O método abaixo consolida a estrutura narrada. Os nomes de propriedades do recurso são reconstruídos conforme o contexto e devem corresponder às chaves do .resx real.

```csharp
using ClosedXML.Excel;
// Importar também o namespace dos recursos do Domain.

private void InsertHeader(IXLWorksheet worksheet)
{
    worksheet.Cell("A1").Value =
        ResourceReportGenerationMessages.Title;

    worksheet.Cell("B1").Value =
        ResourceReportGenerationMessages.Date;

    worksheet.Cell("C1").Value =
        ResourceReportGenerationMessages.PaymentType;

    worksheet.Cell("D1").Value =
        ResourceReportGenerationMessages.Amount;

    worksheet.Cell("E1").Value =
        ResourceReportGenerationMessages.Description;

    worksheet.Cells("A1:E1").Style.Font.Bold = true;

    worksheet.Cells("A1:E1").Style.Fill.BackgroundColor =
        XLColor.FromHtml("#F5C2B6");

    worksheet.Cell("A1").Style.Alignment.SetHorizontal(
        XLAlignmentHorizontalValues.Center);

    worksheet.Cell("B1").Style.Alignment.SetHorizontal(
        XLAlignmentHorizontalValues.Center);

    worksheet.Cell("C1").Style.Alignment.SetHorizontal(
        XLAlignmentHorizontalValues.Center);

    worksheet.Cell("E1").Style.Alignment.SetHorizontal(
        XLAlignmentHorizontalValues.Center);

    worksheet.Cell("D1").Style.Alignment.SetHorizontal(
        XLAlignmentHorizontalValues.Right);
}
```

O método pertence à classe do caso de uso, e não ao controller.

### Integração com o código da aula 120

```csharp
var workbook = new XLWorkbook();

workbook.Author = "CashFlow";
workbook.Style.Font.FontSize = 12;
workbook.Style.Font.FontName = "Times New Roman";

var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));

InsertHeader(worksheet);
```

Esse trecho representa somente a etapa de criação e configuração. O autor é ilustrativo, como na aula anterior.

Não é um método Execute completo: ainda faltam a consulta, o preenchimento das despesas e a geração do conteúdo binário.

### Possível simplificação — complemento, não realizada na aula

A configuração compartilhada poderia ser agrupada em uma variável e o alinhamento geral poderia ser aplicado de uma vez, sobrescrevendo D1 depois:

```csharp
var header = worksheet.Cells("A1:E1");

header.Style.Font.Bold = true;
header.Style.Fill.BackgroundColor =
    XLColor.FromHtml("#F5C2B6");

header.Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Center);

worksheet.Cell("D1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Right);
```

A ordem importa nesse exemplo: o alinhamento específico de D1 precisa ocorrer depois da configuração coletiva para não ser sobrescrito por ela.

O código principal preserva as chamadas individuais demonstradas pelo professor.

## 14. Cuidados técnicos e limites da aula

### 14.1 Estilo do workbook versus estilo da célula

O tamanho 12 e a fonte Times New Roman foram configurados no workbook na aula 120. O cabeçalho acrescenta negrito, cor de fundo e alinhamento às células selecionadas.

Não há alteração global para deixar toda a planilha em negrito.

### 14.2 Recursos de localização não traduzem textos livres

A existência de um arquivo .resx em outro idioma não implica tradução automática da descrição cadastrada. O sistema precisa acessar a chave correspondente para obter cada recurso.

### 14.3 O cabeçalho não é uma tabela estruturada do Excel

Nesta gravação, o professor preenche e estiliza células da primeira linha. Não cria um objeto de tabela estruturada do Excel, filtro automático ou congelamento de painéis.

Esses recursos são distintos e não devem ser inferidos a partir do fundo colorido.

### 14.4 Não presumir larguras e formatação numérica

A largura das colunas, ajuste automático, bordas, formatação das datas das despesas e formatação monetária não são configurados nesta aula.

### 14.5 Não confundir código escrito com teste concluído

O professor termina informando que testará o arquivo na próxima aula.

Ainda não foi comprovado visualmente que os textos, cores e alinhamentos são exibidos corretamente em uma planilha aberta.

### 14.6 Não antecipar as próximas etapas

O módulo ainda precisa consultar as despesas pelo período, preencher as linhas, preparar a serialização e entregar o arquivo válido ao consumidor.

Essas etapas não são acrescentadas como se já tivessem sido implementadas.

## 15. Exercícios e revisão

### Exercício 1 — assinatura do método auxiliar

```csharp
private void InsertHeader(IXLWorksheet worksheet)
```

Por que o retorno é void? Porque a função modifica a worksheet existente e não precisa devolver outro objeto.

### Exercício 2 — inserir a data no cabeçalho

```csharp
worksheet.Cell("B1").Value =
    ResourceReportGenerationMessages.Date;
```

### Exercício 3 — estilizar todas as células do cabeçalho

```csharp
worksheet.Cells("A1:E1").Style.Font.Bold = true;
```

### Exercício 4 — aplicar a cor do template

```csharp
worksheet.Cells("A1:E1").Style.Fill.BackgroundColor =
    XLColor.FromHtml("#F5C2B6");
```

### Exercício 5 — alinhar o valor

```csharp
worksheet.Cell("D1").Style.Alignment.SetHorizontal(
    XLAlignmentHorizontalValues.Right);
```

### Exercício 6 — interpretar o intervalo

A1:E1 inclui cinco células: A1, B1, C1, D1 e E1. Não inclui a segunda linha.

### Exercício 7 — localização

Por que não escrever "Título" diretamente no código?

**Resposta:** utilizar um recurso dedicado permite selecionar um texto localizado conforme a cultura, preservando a possibilidade de oferecer o relatório em idiomas diferentes.

### Perguntas de revisão

1. Qual é a diferença entre XLWorkbook e IXLWorksheet?
2. Por que extrair InsertHeader para um método privado?
3. Por que o método não precisa retornar a worksheet?
4. Qual informação fica em cada célula entre A1 e E1?
5. Qual a diferença entre Cell e Cells?
6. O que representa a notação A1:E1?
7. Em qual projeto o professor organiza os recursos dos relatórios?
8. Qual é a finalidade de ResourceReportGenerationMessages?
9. Quais variantes de idioma são mencionadas?
10. Por que configurar o recurso padrão como público?
11. O professor reutiliza o arquivo de mensagens de exceção?
12. A descrição original de uma despesa será traduzida automaticamente?
13. Qual propriedade configura o negrito?
14. Qual é a cor hexadecimal escolhida?
15. Qual método de XLColor interpreta a representação hexadecimal?
16. Qual atalho padrão abre o Color Picker do PowerToys?
17. Qual método configura o alinhamento horizontal?
18. Quais cabeçalhos são centralizados?
19. Qual cabeçalho fica alinhado à direita?
20. Por que alinhar valores monetários à direita?
21. Alinhar D1 também formata automaticamente as demais células da coluna?
22. O professor cria uma tabela estruturada do Excel nesta aula?
23. O arquivo foi gerado e aberto para validação?
24. Qual é a próxima etapa anunciada?

## 16. Resumo e mapa mental

| Componente | Resultado |
|---|---|
| Método auxiliar | InsertHeader(IXLWorksheet worksheet) |
| Recursos | ResourceReportGenerationMessages no Domain/Reports |
| Cabeçalhos | Título, Data, Tipo de pagamento, Valor, Descrição |
| Endereços | A1, B1, C1, D1, E1 |
| Fonte | Negrito no intervalo A1:E1 |
| Fundo | #F5C2B6 |
| Alinhamento central | A1, B1, C1, E1 |
| Alinhamento à direita | D1 |
| Teste do arquivo | Próxima aula |

```text
                       AULA 121
                    INSERT HEADER
                           |
                   IXLWorksheet
                           |
         +-----------------+------------------+
         |                 |                  |
       Valores           Estilo          Alinhamento
         |                 |                  |
  ResourceReport      Cells("A1:E1")     Cell("...")
  GenerationMessages      |                  |
         |            +----+----+      +------+------+
     Cell().Value      |         |      |             |
         |           Bold      Fill   Center        Right
       A1:E1          true       |      |             |
                             #F5C2B6  A,B,C,E         D
                           |
                    Cabeçalho localizado
                           |
                  Teste na próxima aula
```

### Referências complementares

- [ClosedXML — documentação de células](https://docs.closedxml.io/en/latest/api/cell.html)
- [ClosedXML — estilos](https://docs.closedxml.io/en/latest/features/cell-format.html)
- [ClosedXML — índice da API](https://docs.closedxml.io/en/latest/api/index.html)
- [Microsoft Learn — Color Picker do PowerToys](https://learn.microsoft.com/pt-br/windows/powertoys/color-picker)

**Conclusão:** a aula 121 constrói o cabeçalho localizado da planilha Excel. O método InsertHeader organiza a escrita das cinco células da primeira linha, utiliza recursos de tradução específicos de relatórios e aplica negrito e preenchimento coletivo com Cells. Os alinhamentos são configurados individualmente, reservando o alinhamento à direita para a coluna de valor. A estrutura está preparada para a próxima etapa, mas ainda não há teste de abertura do arquivo nem preenchimento das despesas.
