# Aula 132 — Relatório PDF (parte 5): Document, metadados, interpolação de strings e estilo Normal

> **Contexto:** a aula 131 terminou a implementação inicial de GetFont, permitindo recuperar os arquivos TTF incorporados à Application. O caso de uso PDF já consulta as despesas, mas ainda não produz um arquivo PDF válido.
>
> **Objetivo:** criar o modelo de documento do MigraDoc, configurar título e autor e definir a fonte padrão por meio do estilo Normal.
>
> **Resultado demonstrado:** criação de CreateDocument(DateOnly month), chamada no caso de uso, configuração de metadados localizados e alteração da fonte do estilo padrão.
>
> **Escopo:** a gravação não adiciona páginas ou seções, não renderiza o documento e não substitui o retorno provisório de bytes. O professor chama esse modelo inicial de documento PDF vazio; tecnicamente, trata-se do modelo de documento do MigraDoc, ainda não serializado como PDF.
>
> **Fidelidade:** os identificadores de tipos da biblioteca são normalizados conforme sua documentação. A classe de recursos ResourceReportGenerationMessages segue a reconstrução das aulas anteriores; a chave ExpensesFor foi enunciada. A família transcrita como Halloween permanece identificada provavelmente como Raleway, conforme a aula 129. Os nomes das constantes de FontHelper e a grafia do autor devem ser conferidos no código original. Os exemplos complementares não são ações realizadas na gravação.

## Índice

1. Recapitulação e objetivo
2. Document Object Model do MigraDoc
3. Extração de CreateDocument
4. Metadados com Document.Info
5. Recursos localizados e ExpensesFor
6. DateOnly e formato padrão Y/y
7. Simplificação de string interpolation
8. Estilos predefinidos do documento
9. Configuração da fonte Normal
10. Operador null-forgiving
11. Integração com o resolvedor de fontes
12. Código consolidado da aula
13. Modelo de documento versus PDF renderizado
14. Complementos técnicos e boas práticas
15. Exercícios e revisão
16. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

O módulo PDF foi construído em etapas:

| Aula | Conteúdo |
|---|---|
| 128 | Endpoint, FromQuery, caso de uso, DI e consulta mensal |
| 129 | Fontes, template visual e Embedded Resource |
| 130 | PDFsharp-MigraDoc, IFontResolver e FontHelper |
| 131 | GetFont, Reflection, fallback e leitura de streams |
| 132 | Document, metadados e estilo tipográfico padrão |

O professor inicia a configuração visual do documento antes de inserir as despesas.

Em vez de construir todo o layout imediatamente, ele prepara um objeto que concentrará as configurações gerais.

```text
Consulta mensal
       |
       v
Lista sem despesas? -> sim -> byte[] vazio
       |
      não
       |
       v
CreateDocument(month)
       |
       +-- título localizado
       +-- autor
       +-- estilo Normal
       |
       v
Document (modelo em memória)
       |
       v
Próximas aulas: conteúdo e renderização
```

A aula não modifica o endpoint, a consulta nem o tratamento definitivo de ausência de despesas.

## 2. Document Object Model do MigraDoc

O professor enfatiza a escolha do tipo correto entre os tipos Document disponíveis no Visual Studio.

```csharp
using MigraDoc.DocumentObjectModel;
```

O tipo Document desse namespace representa o modelo lógico de um documento MigraDoc.

```csharp
var document = new Document();
```

O objeto pode armazenar metadados, configurações, estilos e, futuramente, seções e seus elementos.

### Relação com o pacote instalado

| Tipo ou componente | Responsabilidade |
|---|---|
| MigraDoc.DocumentObjectModel.Document | Representar a estrutura lógica do documento |
| PDFsharp | Infraestrutura de geração e manipulação de PDF |
| MigraDoc | Composição de conteúdo, estilos e layout |
| IFontResolver | Fornecer faces e bytes de fontes ao mecanismo tipográfico |

O tipo Document não é um byte[] e sua instanciação não gera automaticamente um arquivo PDF.

É uma representação em memória que precisará ser preenchida e renderizada.

O professor ainda não utiliza PdfDocumentRenderer nesta aula.

## 3. Extração de CreateDocument

A criação e configuração são colocadas em um método auxiliar privado:

```csharp
private Document CreateDocument(DateOnly month)
{
    var document = new Document();

    // Configurações do documento.

    return document;
}
```

No método Execute:

```csharp
var document = CreateDocument(month);
```

### Por que receber DateOnly?

O título do relatório incluirá o período solicitado pelo consumidor.

A configuração depende de month, então esse valor é recebido pelo método auxiliar.

### Por que separar em um método?

O professor pretende organizar a construção do relatório em etapas menores.

CreateDocument concentra a inicialização do modelo e suas configurações gerais. Execute permanece responsável por coordenar a consulta e a criação do relatório.

Isso melhora a leitura do caso de uso sem exigir, nesta etapa, uma classe adicional para cada configuração.

A natureza private do método indica que ele é um detalhe interno do caso de uso, não uma operação do contrato público da API.

## 4. Metadados com Document.Info

O professor começa pela propriedade Info.

```csharp
document.Info.Title = /* título */;
document.Info.Author = /* autor */;
```

A documentação do MigraDoc apresenta Info como a área em que informações como título e autor podem ser configuradas.

### Propriedades discutidas

| Propriedade | Finalidade | Situação na aula |
|---|---|---|
| Title | Identificação do documento | Configurada |
| Author | Identificação do autor | Configurada |
| Subject | Assunto do documento | Mencionado, não configurado |

O professor utiliza seu próprio nome no campo Author. A grafia ou o valor literal não é reproduzido como fato inequívoco neste material; utiliza-se CashFlow como exemplo ilustrativo.

```csharp
document.Info.Author = "CashFlow";
```

### Metadado não é conteúdo de página

Title não cria automaticamente um título visível no corpo do documento.

Author não insere automaticamente uma assinatura ou nome em todas as páginas.

Essas propriedades descrevem o documento. Após a renderização, podem integrar seus metadados, conforme o comportamento da biblioteca.

Para apresentar um título visual, o modelo precisará de um elemento de conteúdo, como um parágrafo ou outro componente adequado.

A aula não adiciona esse elemento.

## 5. Recursos localizados e ExpensesFor

O projeto já utiliza recursos localizados para os cabeçalhos do Excel.

O professor prepara previamente uma nova mensagem nos recursos de relatórios da camada Domain.

A chave utilizada na gravação é ExpensesFor.

O professor confirma a inclusão da mensagem para:

- Inglês.
- Português brasileiro.
- Português de Portugal.
- Francês.

No inglês, ele menciona literalmente Expenses for.

As traduções completas e os nomes exatos dos arquivos .resx não são mostrados textualmente para todas as culturas. Os exemplos abaixo são ilustrativos:

| Cultura | Prefixo ilustrativo |
|---|---|
| en | Expenses for |
| pt-BR | Despesas de |
| pt-PT | Despesas de |
| fr | Dépenses de |

Não se deve interpretar os textos ilustrativos como reprodução literal dos quatro recursos do professor.

### Composição do título

```csharp
document.Info.Title =
    $"{ResourceReportGenerationMessages.ExpensesFor} {month.ToString("Y")}";
```

A classe de recursos utilizada neste exemplo segue a nomenclatura reconstruída anteriormente. Confirme o nome real gerado a partir dos arquivos .resx.

O título resulta da combinação de:

1. Prefixo recuperado do recurso localizado.
2. Espaço.
3. Representação de mês e ano.

Esse comportamento aproveita a configuração de cultura já existente no CashFlow.

### CurrentUICulture e CurrentCulture

Como complemento:

- CurrentUICulture costuma orientar a seleção de textos dos recursos localizados.
- CurrentCulture costuma orientar a formatação de datas, números e valores.

Se ambas forem configuradas de forma coerente, a mensagem e a representação do período poderão utilizar o mesmo idioma.

A aula não altera o middleware de cultura nem testa as quatro traduções.

## 6. DateOnly e formato padrão Y/y

O professor relembra a formatação utilizada no nome da worksheet do Excel.

```csharp
month.ToString("Y")
```

Y e y representam o formato padrão de ano e mês.

Eles utilizam o padrão de ano/mês da cultura selecionada.

### Exemplos ilustrativos

Considere:

```csharp
var month = new DateOnly(2024, 5, 1);
```

| Cultura | Representação típica |
|---|---|
| pt-BR | maio de 2024 |
| en-US | May 2024 |
| fr-FR | mai 2024 |

Os exemplos dependem dos dados de globalização disponíveis no ambiente. Não representam saídas verificadas na gravação.

### DateTime.Today como ferramenta de consulta

O professor temporariamente utiliza DateTime.Today.ToString para consultar as opções de formatação sugeridas pelo Visual Studio.

O IDE apresenta uma prévia fixa equivalente a junho de 2009.

A data de 2009 não é a data do relatório nem o período realmente consultado. É somente um exemplo da ferramenta.

O professor remove a expressão de demonstração e mantém month como origem do período.

A gravação atribui a diferença de sugestões a um problema do DateOnly no Visual Studio. Não se identifica uma issue específica e não se demonstra falha de formatação do próprio DateOnly.

### Y não é um caractere literal de ano

Quando Y ou y é usado isoladamente como formato padrão, significa ano e mês.

Isso não deve ser confundido com padrões personalizados como yyyy-MM, nos quais a sequência yyyy representa o ano e MM representa o número do mês.

O formato padrão foi escolhido para produzir uma apresentação humana localizada, não um identificador de período para armazenamento.

## 7. Simplificação de string interpolation

O Visual Studio sugere simplificar:

```csharp
$"{ResourceReportGenerationMessages.ExpensesFor} {month.ToString("Y")}"
```

Para:

```csharp
$"{ResourceReportGenerationMessages.ExpensesFor} {month:Y}"
```

O professor aceita a sugestão Simplify interpolation.

### Sintaxe geral

```csharp
$"{expression:format}"
```

O conteúdo após os dois-pontos é a especificação de formato da expressão interpolada.

O formato Y continua tendo o significado de ano e mês.

Exemplos complementares:

```csharp
var date = new DateOnly(2024, 5, 1);
var amount = 1234.5m;

var period = $"{date:Y}";
var numeric = $"{amount:F2}";
```

O exemplo não modifica o formato monetário do relatório PDF nesta aula.

### Equivalência no cenário demonstrado

| Forma | Efeito |
|---|---|
| month.ToString("Y") | Solicita explicitamente a representação formatada |
| month:Y | Solicita a formatação dentro da interpolação |

No contexto de uma string interpolada comum e com a mesma cultura, ambas produzem a representação desejada.

Não é uma alteração no tipo DateOnly nem uma conversão permanente do objeto.

## 8. Estilos predefinidos do documento

O MigraDoc possui uma coleção de estilos associados ao Document.

```csharp
var style = document.Styles["Normal"];
```

A aula acessa o estilo Normal pelo indexador da coleção.

O professor o descreve como o estilo padrão ou regular do documento.

A documentação oficial também utiliza o identificador StyleNames.Normal.

```csharp
var style = document.Styles[StyleNames.Normal];
```

A segunda forma é um complemento que evita repetir o nome textual do estilo. A forma com string corresponde à explicação apresentada pelo professor.

### Estilo versus formatação direta

| Abordagem | Finalidade |
|---|---|
| Alterar style.Font.Name | Configurar uma propriedade em um estilo reutilizável |
| Alterar a fonte de um parágrafo | Personalizar determinado elemento |
| Criar outro estilo | Compartilhar uma configuração específica entre vários elementos |

A finalidade de Normal é servir de base para a formatação de parágrafos que o utilizem ou de estilos que herdem essa propriedade.

**Precisão:** nem todos os elementos do documento necessariamente utilizarão a mesma fonte. Estilos próprios, formatação direta, outras fontes predefinidas e particularidades da biblioteca podem sobrescrever a configuração.

## 9. Configuração da fonte Normal

O professor utiliza a variante Regular da família identificada provavelmente como Raleway.

```csharp
var style = document.Styles["Normal"];

style!.Font.Name = FontHelper.RALEWAY_REGULAR;
```

Os nomes de constantes seguem as reconstruções das aulas 130 e 131. A gravação não permite confirmar visualmente a grafia literal do arquivo nem da constante.

### Por que configurar uma fonte padrão?

Sem uma configuração compartilhada, o desenvolvedor poderia precisar repetir a fonte desejada em cada parágrafo.

Ao definir Normal, o professor estabelece um padrão tipográfico para o documento.

Ele pretende utilizar a variante Regular em textos comuns e sobrescrever a fonte quando desejar destaque, como no total das despesas.

O total deverá utilizar Work Sans Black, de acordo com o template visual apresentado na aula 129.

A aula não cria o parágrafo do total nem configura efetivamente essa sobrescrita.

### Relação com hierarquia visual

```text
Normal -> Raleway Regular (padrão pretendido)
      |
      +-- texto comum: herda o padrão
      |
      +-- total: poderá receber WorkSans-Black
      |
      +-- outros elementos: conforme seus estilos
```

O objetivo é evitar repetição de configurações e manter uma aparência consistente.

## 10. Operador null-forgiving

O Visual Studio apresenta uma advertência porque a busca pelo estilo pode ser considerada anulável.

O professor acrescenta !:

```csharp
style!.Font.Name = FontHelper.RALEWAY_REGULAR;
```

Essa sintaxe retoma o conceito apresentado na aula 131.

O operador pós-fixado !, denominado null-forgiving operator, suprime a advertência de possível nulidade da expressão.

Ele não modifica o valor em execução.

| Expressão | Significado |
|---|---|
| style? | Em uma expressão como style?.Font, acesso condicional |
| style! | Supressão da análise de possível nulidade |
| style ?? throw ... | Tratamento explícito de ausência |

O professor assume que Normal está presente entre os estilos predefinidos do Document.

Caso a referência fosse realmente nula, o uso de ! não impediria uma NullReferenceException.

### Alternativa complementar

```csharp
var style = document.Styles[StyleNames.Normal]
    ?? throw new InvalidOperationException(
        "Estilo Normal não encontrado.");

style.Font.Name = FontHelper.RALEWAY_REGULAR;
```

Esse exemplo trata explicitamente uma eventual ausência e produz uma mensagem de diagnóstico.

Não é a abordagem utilizada na gravação.

## 11. Integração com o resolvedor de fontes

O nome configurado em Font.Name é o alias escolhido nas aulas anteriores.

```text
style.Font.Name
       |
       v
FontHelper.RALEWAY_REGULAR
       |
       v
Alias da variante
       |
       v
ResolveTypeface(alias, bold, italic)
       |
       v
FontResolverInfo(alias)
       |
       v
GetFont(alias)
       |
       v
Stream de Embedded Resource -> byte[]
```

Essa cadeia representa o mecanismo esperado quando a biblioteca precisar resolver a fonte para layout ou renderização.

A simples atribuição de Font.Name não comprova que GetFont tenha sido chamado nesta aula.

A fonte padrão tipográfica do estilo Normal e a fonte de fallback em FontHelper podem apontar para a mesma variante, mas possuem responsabilidades distintas.

| Conceito | Finalidade |
|---|---|
| Normal.Font.Name | Escolher a fonte inicial dos textos que herdam o estilo |
| DEFAULT_FONT | Fonte alternativa quando um recurso solicitado não é localizado |
| GlobalFontSettings.FontResolver | Configurar qual resolvedor atende à biblioteca |

O professor não altera o resolvedor na gravação atual.

A recomendação da aula 130 permanece relevante: configurar o resolvedor global uma única vez, antes de utilizar as fontes.

## 12. Código consolidado da aula

Reconstrução fiel às operações narradas:

```csharp
using MigraDoc.DocumentObjectModel;

// Dentro do caso de uso de relatório PDF:

public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    var document = CreateDocument(month);

    // Ainda não há seções, conteúdo ou renderização.

    return [];
}

private Document CreateDocument(DateOnly month)
{
    var document = new Document();

    document.Info.Title =
        $"{ResourceReportGenerationMessages.ExpensesFor} {month:Y}";

    document.Info.Author = "CashFlow"; // Autor ilustrativo.

    var style = document.Styles["Normal"];

    style!.Font.Name = FontHelper.RALEWAY_REGULAR;

    return document;
}
```

Os namespaces internos do projeto, o nome exato da classe de recursos e o identificador de fonte precisam acompanhar o código real.

O professor menciona Subject, mas não o configura.

O retorno provisório permanece porque a gravação não mostra a conversão de Document em bytes.

## 13. Modelo de documento versus PDF renderizado

É importante diferenciar três etapas:

| Etapa | Resultado |
|---|---|
| Criação de Document | Modelo lógico em memória |
| Inserção de seções e elementos | Estrutura de conteúdo |
| Renderização e salvamento | Documento PDF binário |

A aula conclui somente a configuração inicial da primeira etapa.

Ainda não existe um arquivo PDF em branco efetivamente baixado ou validado.

O professor anuncia que a próxima aula criará uma página para o documento.

No MigraDoc, a estrutura de seções, o layout e a paginação são relacionados: a biblioteca organiza o conteúdo em páginas na renderização. Uma seção pode ocupar mais de uma página. Essa distinção é complementar e não antecipa o código da próxima aula.

## 14. Complementos técnicos e boas práticas

### 14.1 Estilos reutilizáveis

Para um documento maior, pode ser conveniente centralizar estilos de títulos, totais, textos de apoio e valores.

Isso evita aplicar repetidamente propriedades de fonte, tamanho, cor ou espaçamento.

A gravação modifica apenas o estilo Normal.

### 14.2 Tradução da mensagem completa

O professor concatena um prefixo localizado com o período.

Como complemento, um sistema com requisitos de localização mais abrangentes pode armazenar uma frase completa com um marcador de substituição.

Exemplo:

```text
ExpensesFor = Expenses for {0}
```

```csharp
var title = string.Format(
    CultureInfo.CurrentCulture,
    ResourceReportGenerationMessages.ExpensesFor,
    month.ToString("Y"));
```

Esse exemplo exige que os recursos sejam modificados para conter o marcador. Não deve ser aplicado diretamente à chave atual se ela contém somente o prefixo.

A vantagem é permitir que cada idioma ajuste a ordem das palavras e sua pontuação.

### 14.3 Metadados e privacidade

O campo Author deve representar a identidade que o sistema pretende publicar, como uma organização ou autor autorizado.

Não é necessário armazenar o nome do desenvolvedor em todos os documentos gerados por uma API real.

A aula usa o nome do professor como exemplo didático.

### 14.4 Renderização e testes futuros

Quando o gerador estiver completo, será importante testar:

| Cenário | Objetivo |
|---|---|
| Cultura pt-BR e en-US | Verificar título e representação do período |
| Estilo Normal | Confirmar a variante tipográfica padrão |
| Texto com sobrescrita | Confirmar a seleção de Work Sans Black |
| Acentos e símbolos monetários | Verificar cobertura de glifos |
| Metadados do arquivo | Conferir título e autor no PDF gerado |
| Requisições sucessivas | Verificar configuração global e resolução de fontes |

Nenhum desses testes é executado nesta gravação.

### Referências oficiais

- Document settings: https://docs.pdfsharp.net/MigraDoc/DOM/Document/DocumentSettings.html
- Formatting: https://docs.pdfsharp.net/MigraDoc/DOM/Document/Formatting.html
- HelloMigraDoc: https://docs.pdfsharp.net/MigraDoc/Topics/Start/HelloMigraDoc.html
- Font: https://docs.pdfsharp.net/MigraDoc/DOM/Formats/Font.html

As referências esclarecem o modelo de documento e os estilos. A versão histórica do curso é 6.1.0-preview-3; consulte a documentação correspondente ao pacote instalado para reproduzir sua API.

## 15. Exercícios e revisão

### Exercício 1 — escolher o tipo correto

Qual namespace contém Document utilizado no curso?

**Resposta:** MigraDoc.DocumentObjectModel.

### Exercício 2 — inicializar o documento

```csharp
private Document CreateDocument(DateOnly month)
{
    var document = new Document();

    return document;
}
```

Qual é a finalidade de receber month?

**Resposta:** permitir configurar o título com o período solicitado.

### Exercício 3 — simplificar a interpolação

Reescreva:

```csharp
$"{month.ToString("Y")}"
```

Resposta:

```csharp
$"{month:Y}"
```

### Exercício 4 — distinguir metadado de conteúdo

Configurar document.Info.Title insere automaticamente um título no corpo da primeira página?

**Resposta:** não. Essa propriedade configura informações do documento, não cria um parágrafo visível.

### Exercício 5 — definir o estilo Normal

```csharp
var style = document.Styles["Normal"];
style!.Font.Name = FontHelper.RALEWAY_REGULAR;
```

O estilo fornece um padrão para os elementos que o utilizam ou herdam sua configuração.

### Exercício 6 — interpretar !

Se style for nulo, ! evita uma exceção?

**Resposta:** não. Ele apenas suprime a advertência do compilador.

### Perguntas de revisão

1. Qual é o objetivo da aula 132?
2. Qual é o tipo devolvido por CreateDocument?
3. Por que separar sua configuração de Execute?
4. Qual propriedade configura o título?
5. Qual propriedade configura o autor?
6. Subject foi efetivamente configurado?
7. Qual chave de recursos foi introduzida?
8. Quais idiomas recebem a mensagem?
9. Qual a diferença entre CurrentCulture e CurrentUICulture?
10. O que representa o formato Y?
11. Para que DateTime.Today foi utilizado temporariamente?
12. O ano de 2009 pertence ao relatório real?
13. Como simplificar ToString dentro de uma interpolação?
14. O que document.Styles representa?
15. Qual estilo é modificado?
16. Qual variante foi selecionada como fonte padrão?
17. O que acontece quando uma fonte é sobrescrita diretamente?
18. Qual a finalidade do operador null-forgiving?
19. Qual diferença existe entre estilo Normal e fonte de fallback?
20. Criar Document gera automaticamente um arquivo PDF?
21. Já existe uma página ou seção adicionada?
22. A renderização foi testada?
23. Qual é a próxima etapa anunciada?

## 16. Resumo e mapa mental

| Componente | Resultado |
|---|---|
| Namespace | MigraDoc.DocumentObjectModel |
| Método auxiliar | CreateDocument(DateOnly month) |
| Objeto | new Document() |
| Título | Prefixo localizado + mês e ano |
| Recurso | ExpensesFor |
| Formato | Y/y |
| Simplificação | month:Y |
| Autor | Configurado |
| Subject | Apenas mencionado |
| Estilo | Normal |
| Fonte padrão | Variante Regular da segunda família |
| Nulidade | ! para suprimir advertência |
| Seções/páginas | Pendentes |
| Renderização | Pendente |
| Retorno de bytes PDF | Pendente |

```text
                    AULA 132 — DOCUMENT
                               |
                     CreateDocument(month)
                               |
                        new Document()
                               |
           +-------------------+-------------------+
           |                   |                   |
       Info.Title          Info.Author         Styles["Normal"]
           |                   |                   |
       ExpensesFor         Autor               Font.Name
           +                   |                   |
       month:Y                 |              FontHelper
           |                   |                   |
       Localização             |              Regular
           |                   |                   |
           +-------------------+-------------------+
                               |
                        return document
                               |
              Modelo pronto para receber conteúdo
                               |
                 Próxima aula: seção/página
```

**Conclusão:** a aula inicia a construção do modelo PDF mediante CreateDocument, configura título localizado, autor e fonte padrão. O professor reaproveita DateOnly, recursos de tradução, interpolação de strings e o FontHelper. O estilo Normal evita repetir a fonte em cada elemento que herda esse padrão. O modelo ainda não possui seções ou conteúdo, não foi renderizado e não fornece um PDF válido para download. A etapa seguinte será preparar a página do relatório.
