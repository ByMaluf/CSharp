# Aula 133 — Relatório PDF (parte 6): Section, PageSetup, Clone, A4 e margens

> **Continuação da aula 132:** o caso de uso já cria um Document do MigraDoc, configura título localizado, autor e a fonte padrão do estilo Normal.
>
> **Objetivo:** adicionar a primeira seção ao documento e configurar o formato da página e as margens com base no template do Figma.
>
> **Resultado:** implementação de CreatePage(Document document), adição de uma Section, clonagem de DefaultPageSetup e configuração de A4 com margens laterais de 40 e verticais de 80.
>
> **Escopo:** ainda não há textos, valores das despesas, renderização ou download de PDF. O professor utiliza o termo página para explicar Section; tecnicamente, uma seção pode abranger várias páginas.
>
> **Fidelidade:** os trechos de código foram reconstruídos a partir da narração. Os valores das margens e as dimensões do template são explicitamente informados. Os exemplos de unidades, herança, clonagem e quebras de página são complementos fundamentados na documentação oficial do MigraDoc 6.1, separados do código demonstrado.

## Índice

1. Recapitulação e objetivo
2. Documento, seção e página
3. Paginação automática
4. Método auxiliar CreatePage
5. AddSection e mutação do documento
6. PageSetup e configuração padrão
7. Clone e referências de objetos
8. Formato A4
9. Dimensões do template no Figma
10. Configuração das margens
11. Unidades e área útil
12. Código consolidado
13. Integração com o caso de uso
14. Complementos técnicos e cuidados
15. Exercícios e revisão
16. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

| Aula | Etapa |
|---|---|
| 128 | Endpoint PDF, FromQuery, caso de uso e DI |
| 129 | Tipografia, Google Fonts e Embedded Resources |
| 130 | PDFsharp-MigraDoc, IFontResolver e FontHelper |
| 131 | GetFont, Reflection e leitura de recursos |
| 132 | Document, metadados e estilo Normal |
| 133 | Section, PageSetup, formato A4 e margens |

Na aula 132, o professor criou o modelo lógico do documento.

```csharp
var document = CreateDocument(month);
```

Esse método configura metadados, como título e autor, e a fonte padrão.

O instrutor relembra que as informações de autoria e assunto são opcionais. Também menciona a possibilidade de definir um tamanho padrão de fonte, como 12, 13 ou 14. Não escolhe nem implementa um tamanho específico nesta aula.

Agora é necessário criar uma estrutura na qual o conteúdo será inserido futuramente.

A decisão é extrair essa responsabilidade para outro método privado, chamado CreatePage.

## 2. Documento, seção e página

O MigraDoc utiliza um modelo de objetos para representar um documento.

```text
Document
    |
    +-- Info
    +-- Styles
    +-- Sections
          |
          +-- Section
                |
                +-- PageSetup
                +-- Elementos de conteúdo (futuro)
```

### Document

Representa o documento completo, incluindo metadados, estilos e seções.

### Section

Agrupa elementos de conteúdo e configurações de layout. Uma seção pode definir características como tamanho de página, margens, cabeçalhos e rodapés.

### Página

É uma unidade física do documento renderizado.

| Conceito | Momento de existência | Responsabilidade |
|---|---|---|
| Document | Construção do modelo | Estrutura geral |
| Section | Construção do modelo | Agrupar conteúdo e configurações de página |
| Página PDF | Renderização | Representar o conteúdo paginado |

O professor chama a seção de página para simplificar a explicação inicial.

**Precisão técnica:** segundo a documentação oficial do MigraDoc, as páginas ainda não existem quando o modelo está sendo criado. Uma seção pode produzir uma ou várias páginas no PDF. Portanto, AddSection não equivale necessariamente a adicionar exatamente uma página física.

## 3. Paginação automática

O professor explica que não é necessário criar manualmente uma página para cada conjunto de despesas.

O MigraDoc organiza o fluxo de conteúdo durante a renderização. Quando o conteúdo que admite paginação não cabe na página atual, o mecanismo pode continuar em uma nova página.

```text
Section
    |
    +-- Conteúdo inicial
    +-- Mais despesas
    +-- Mais elementos
    |
    v
Renderização e cálculo de layout
    |
    +-- Página 1
    +-- Página 2
    +-- Página 3, se necessária
```

Isso será especialmente útil porque o número de despesas por mês não é fixo.

A aplicação pode construir a estrutura do relatório utilizando a lista recuperada pelo repositório, e a biblioteca calculará as quebras de página conforme as características do conteúdo.

### Limites dessa automação

Como complemento, a paginação não significa que qualquer objeto de qualquer dimensão será redimensionado ou dividido automaticamente.

Um elemento não divisível, como determinada imagem ou bloco de layout, pode precisar de tratamento próprio se exceder a área disponível.

A aula não testa paginação, pois ainda não adiciona conteúdo nem renderiza o documento.

## 4. Método auxiliar CreatePage

O professor cria um método privado que recebe o Document e devolve Section.

```csharp
private Section CreatePage(Document document)
{
    var section = document.AddSection();

    return section;
}
```

O nome CreatePage é o escolhido pelo professor. A assinatura e os tipos refletem a API do MigraDoc.

### Elementos da assinatura

| Elemento | Significado |
|---|---|
| private | Método interno ao caso de uso |
| Section | Tipo retornado |
| CreatePage | Nome do método auxiliar |
| Document document | Modelo que receberá a nova seção |

O objeto Document foi criado anteriormente por CreateDocument.

O professor utiliza métodos auxiliares para tornar Execute mais legível e organizar as etapas de construção.

### Alternativa de nomenclatura — complemento

CreateSection poderia ser um nome mais preciso, pois o método retorna Section, e não uma página PDF renderizada. Entretanto, o material mantém CreatePage para acompanhar o código do curso.

## 5. AddSection e mutação do documento

A chamada central é:

```csharp
var section = document.AddSection();
```

AddSection adiciona uma seção ao documento e devolve a referência para a seção recém-criada.

O método pode então configurar essa instância e retorná-la ao chamador.

### Relação com tipos de referência

Document é um objeto de referência. O parâmetro recebe uma cópia da referência ao objeto já criado, não uma cópia integral do documento.

Ao executar AddSection, o método modifica o documento original por meio dessa referência.

Não é necessário declarar ref no parâmetro.

```text
Execute()
    document --------+
                     |
                     v
                Document
                     ^
                     |
CreatePage(document)-+
                     |
                AddSection()
                     |
                     v
                  Section
```

O retorno Section oferece acesso direto à estrutura que receberá textos e outros elementos nas próximas aulas.

## 6. PageSetup e configuração padrão

O professor acessa a propriedade PageSetup da seção.

```csharp
section.PageSetup = document.DefaultPageSetup.Clone();
```

PageSetup concentra características de página, entre elas:

- Formato do papel.
- Margens.
- Orientação.
- Outras configurações relacionadas ao layout.

DefaultPageSetup fornece a configuração padrão do documento.

A documentação do MigraDoc 6.1 descreve como padrão uma página A4 em orientação retrato, com margens superior, esquerda e direita de 2,5 cm e inferior de 2 cm.

Esses valores são informações complementares da documentação, não valores inspecionados pelo professor no debugger.

### Por que clonar?

O professor quer partir das configurações existentes e alterar somente as propriedades necessárias para reproduzir seu template.

O padrão original não deve ser modificado diretamente.

A documentação de migração para MigraDoc 6 informa que modificar DefaultPageSetup diretamente não é permitido. Uma alternativa é clonar o objeto e modificar a configuração da seção.

## 7. Clone e referências de objetos

```csharp
section.PageSetup = document.DefaultPageSetup.Clone();
```

Clone cria uma cópia da configuração que pode ser atribuída à seção.

É importante distinguir copiar uma referência de criar uma cópia de um objeto.

### Exemplo conceitual de compartilhamento de referência

```csharp
var first = document.DefaultPageSetup;
var second = first;
```

As duas variáveis referenciam o mesmo objeto. Não foi criada uma configuração independente.

### Exemplo com Clone

```csharp
var original = document.DefaultPageSetup;
var copy = original.Clone();

section.PageSetup = copy;
```

Agora a seção recebe a configuração clonada e pode personalizá-la sem precisar modificar diretamente DefaultPageSetup.

```text
DefaultPageSetup
       |
       | Clone()
       v
Nova configuração de PageSetup
       |
       +-- PageFormat = A4
       +-- LeftMargin = 40
       +-- RightMargin = 40
       +-- TopMargin = 80
       +-- BottomMargin = 80
```

### Precisão sobre clonagem

Clone não significa universalmente cópia profunda de qualquer objeto em C#.

O comportamento depende do contrato do método de cada tipo. Neste caso, a própria biblioteca oferece Clone para que configurações de página possam ser copiadas e personalizadas.

Não se deve inferir que qualquer classe possua esse método, nem que todos os métodos Clone tenham a mesma semântica.

### Herança de configurações — complemento

Segundo a documentação 6.1, propriedades não definidas de PageSetup podem ser herdadas da seção anterior; a primeira seção herda os padrões do documento.

Consequentemente, em situações simples, configurar diretamente as propriedades necessárias da seção pode ser suficiente sem clonar toda a configuração.

O professor escolhe a clonagem e essa abordagem é preservada.

## 8. Formato A4

O formato escolhido é A4:

```csharp
section.PageSetup.PageFormat = PageFormat.A4;
```

PageFormat é um enum da biblioteca que identifica formatos de página predefinidos.

A4 possui dimensões físicas nominais de 210 mm por 297 mm, em orientação retrato.

| Unidade | Largura aproximada | Altura aproximada |
|---|---:|---:|
| Milímetros | 210 | 297 |
| Centímetros | 21 | 29,7 |
| Pontos tipográficos | 595,28 | 841,89 |

O professor seleciona A4 explicitamente, mesmo que esse também seja o formato padrão descrito pela documentação 6.1.

Isso torna a intenção do relatório mais evidente.

A orientação não é configurada na gravação. A orientação retrato é a configuração padrão que o código clonado pretende conservar.

### Formato de papel versus formato de arquivo

A4 representa as dimensões da página.

PDF representa o formato do arquivo.

É possível criar PDFs com páginas A4, A3, Letter e outros tamanhos. Portanto, PDF não significa necessariamente A4.

## 9. Dimensões do template no Figma

O professor volta ao template visual e mostra que utilizou:

```text
Largura: 595
Altura: 842
```

Essas dimensões aproximam a proporção e a representação de um A4 em pontos.

A relação pode ser compreendida pela conversão:

```text
1 polegada = 25,4 mm
1 ponto = 1/72 de polegada

210 / 25,4 × 72 ≈ 595,28 pt
297 / 25,4 × 72 ≈ 841,89 pt
```

### Cuidado com as unidades do Figma

As dimensões informadas no editor visual não significam que todo pixel de uma imagem corresponda fisicamente a um ponto no PDF.

O template funciona como referência proporcional. A aplicação deve reproduzir suas medidas usando as unidades do mecanismo de layout.

A resolução de exportação de imagens e a escala física de impressão são assuntos distintos.

Nesta aula, o template é usado para identificar dimensões e margens; não é importado automaticamente para o documento MigraDoc.

## 10. Configuração das margens

No Figma, o professor utiliza a tecla Alt ao selecionar elementos para visualizar suas distâncias em relação às bordas.

Ele identifica:

| Margem | Valor escolhido |
|---|---:|
| Esquerda | 40 |
| Direita | 40 |
| Superior | 80 |
| Inferior | 80 |

O objetivo é manter uma área de respiro em torno do conteúdo.

No código:

```csharp
section.PageSetup.LeftMargin = 40;
section.PageSetup.RightMargin = 40;
section.PageSetup.TopMargin = 80;
section.PageSetup.BottomMargin = 80;
```

Os valores são os informados literalmente na gravação.

### Representação visual aproximada

```text
            largura: 595
       +-----------------------+
       |      topo: 80         |
       |    +-------------+    |
       |    |             |    |
       | 40 |  Área útil  | 40 |
       |    |             |    |  altura: 842
       |    |             |    |
       |    +-------------+    |
       |      base: 80         |
       +-----------------------+
```

As margens laterais possuem o mesmo tamanho. As margens superior e inferior também são iguais.

O template apresenta margens verticais maiores que as horizontais.

A aula não define bordas visíveis, cor de fundo ou conteúdo nas regiões das margens.

## 11. Unidades e área útil

**Complemento técnico:** o MigraDoc utiliza o tipo Unit para medidas de layout e tamanhos tipográficos.

A intenção das atribuições numéricas apresentadas é utilizar valores em pontos. Uma forma mais explícita de representar a unidade é:

```csharp
section.PageSetup.LeftMargin = Unit.FromPoint(40);
section.PageSetup.RightMargin = Unit.FromPoint(40);
section.PageSetup.TopMargin = Unit.FromPoint(80);
section.PageSetup.BottomMargin = Unit.FromPoint(80);
```

Essa variante não foi escrita pelo professor. Ela evita ambiguidades e possíveis advertências de conversão implícita em versões diferentes da biblioteca.

### Conversões aproximadas

| Medida | Pontos | Milímetros |
|---|---:|---:|
| Margem lateral | 40 | 14,11 |
| Margem vertical | 80 | 28,22 |

A unidade ponto é uma medida física, não uma quantidade de pixels da tela.

### Área útil do corpo do documento

Utilizando as dimensões arredondadas do template:

```text
Largura útil = 595 - 40 - 40 = 515
Altura útil  = 842 - 80 - 80 = 682
```

Usando as dimensões nominais do A4 convertidas para pontos:

```text
Largura útil ≈ 515,28 pt
Altura útil  ≈ 681,89 pt
```

Esses são cálculos complementares, e não medições executadas pelo professor.

O espaço realmente disponível para elementos específicos também depende de suas próprias configurações, como recuos, espaçamentos, tabelas e regras de layout.

Cabeçalhos e rodapés possuem mecanismos de posicionamento próprios, portanto essas medidas representam a região nominal do corpo da página, não uma garantia de que todos os objetos caberão nela.

## 12. Código consolidado

A implementação da aula é reconstruída a seguir:

```csharp
using MigraDoc.DocumentObjectModel;

// Dentro do caso de uso do relatório PDF:
private Section CreatePage(Document document)
{
    var section = document.AddSection();

    section.PageSetup = document.DefaultPageSetup.Clone();

    section.PageSetup.PageFormat = PageFormat.A4;

    section.PageSetup.LeftMargin = 40;
    section.PageSetup.RightMargin = 40;
    section.PageSetup.TopMargin = 80;
    section.PageSetup.BottomMargin = 80;

    return section;
}
```

### Leitura passo a passo

1. Receber o documento existente.
2. Adicionar uma seção ao documento.
3. Recuperar e clonar a configuração padrão de páginas.
4. Atribuir a cópia à seção.
5. Definir o papel A4.
6. Configurar as quatro margens.
7. Devolver a seção para o caso de uso.

O método não renderiza um arquivo PDF.

### Variante complementar com unidades explícitas

```csharp
private Section CreatePage(Document document)
{
    var section = document.AddSection();

    section.PageSetup = document.DefaultPageSetup.Clone();

    var setup = section.PageSetup;

    setup.PageFormat = PageFormat.A4;
    setup.LeftMargin = Unit.FromPoint(40);
    setup.RightMargin = Unit.FromPoint(40);
    setup.TopMargin = Unit.FromPoint(80);
    setup.BottomMargin = Unit.FromPoint(80);

    return section;
}
```

A variável setup reduz a repetição do acesso à propriedade. O exemplo mantém a lógica demonstrada, mas é uma refatoração ilustrativa.

## 13. Integração com o caso de uso

Depois de CreateDocument, o professor acrescenta:

```csharp
var document = CreateDocument(month);

var page = CreatePage(document);
```

O tipo inferido para page é Section.

A variável poderá ser utilizada para adicionar os elementos do relatório.

### Estado consolidado de Execute

```csharp
public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    var document = CreateDocument(month);

    var page = CreatePage(document);

    // Próxima etapa: adicionar frases e valores.
    // Renderizar o documento e devolver seus bytes.

    return [];
}
```

A assinatura e o contexto são recuperados das aulas anteriores.

O retorno provisório permanece porque a gravação não demonstra a renderização nem a serialização.

### Separação de responsabilidades

| Método | Responsabilidade atual |
|---|---|
| Execute | Consultar despesas e coordenar a geração |
| CreateDocument | Criar modelo, metadados e estilo padrão |
| CreatePage | Adicionar seção e configurar layout de página |
| FilterByMonth | Consultar despesas do período |

A separação favorece a leitura progressiva do código.

Não há uma nova abstração de repositório, configuração de DI ou alteração do endpoint nesta aula.

## 14. Complementos técnicos e cuidados

### 14.1 Seções adicionais e quebras de página

Uma seção normalmente começa em uma nova página. Ela pode possuir configurações próprias.

Em um documento com conteúdo que compartilha o mesmo formato, não é preciso criar uma nova seção para cada página.

A documentação também oferece a possibilidade de inserir uma quebra de página explicitamente:

```csharp
section.AddPageBreak();
```

Esse exemplo não foi implementado pelo professor.

Uma quebra explícita pode ser útil quando se deseja iniciar determinado conteúdo em uma nova página sem necessariamente criar outra seção.

### 14.2 Reutilização de PageSetup

A configuração pode ser organizada em um método auxiliar ou componente compartilhado se diferentes relatórios possuírem o mesmo padrão visual.

A clonagem não transforma as configurações do documento inteiro; ela prepara as configurações de uma seção específica.

Se futuras seções herdarem características da anterior, considere esse comportamento ao modificar seu layout.

### 14.3 Preservar as configurações de fonte

A aula anterior configurou Normal.Font.Name. A criação da seção não substitui automaticamente esse estilo.

Tamanhos, cores e variantes específicas poderão ser definidos nos elementos ou estilos futuros.

A aula não muda a fonte padrão nem verifica o funcionamento do resolvedor.

### 14.4 Constantes para margens

Como complemento, é possível nomear medidas repetidas:

```csharp
private const int HorizontalMargin = 40;
private const int VerticalMargin = 80;
```

```csharp
section.PageSetup.LeftMargin =
    Unit.FromPoint(HorizontalMargin);

section.PageSetup.RightMargin =
    Unit.FromPoint(HorizontalMargin);

section.PageSetup.TopMargin =
    Unit.FromPoint(VerticalMargin);

section.PageSetup.BottomMargin =
    Unit.FromPoint(VerticalMargin);
```

Isso retoma o conceito de constantes da aula 126.

A decisão de extrair constantes depende de quanto esses valores representam uma convenção reutilizável. O professor utiliza diretamente os valores 40 e 80 nesta gravação.

### 14.5 Testes futuros

| Cenário | Objetivo |
|---|---|
| Renderização com poucas despesas | Verificar papel e margens |
| Renderização com muitas despesas | Observar paginação automática |
| Elementos mais largos que a região útil | Verificar comportamento de layout |
| Documento com mais de uma seção | Verificar herança de PageSetup |
| Fontes personalizadas | Confirmar renderização e integração com IFontResolver |

Esses testes são sugestões de estudo. A gravação apenas implementa a configuração do modelo.

### Referências oficiais

- Estrutura do documento: https://docs.pdfsharp.net/MigraDoc/DOM/Document/DocumentStructure.html
- Configuração de página: https://docs.pdfsharp.net/MigraDoc/DOM/Formats/PageSetup.html
- Controle de paginação: https://docs.pdfsharp.net/MigraDoc/DOM/Document/PageControl.html
- Unidades: https://docs.pdfsharp.net/MigraDoc/DOM/Formats/Unit.html
- Migração para PDFsharp/MigraDoc 6: https://docs.pdfsharp.net/General/Overview/Upgrade-to-PDFsharp-6.html

As referências foram consultadas para distinguir seção de página, esclarecer as configurações padrão, a herança, a clonagem e as unidades. O curso utiliza historicamente PDFsharp-MigraDoc 6.1.0-preview-3; diferenças entre versões devem ser verificadas quando necessário.

## 15. Exercícios e revisão

### Exercício 1 — criar uma seção

```csharp
var section = document.AddSection();
```

Qual tipo é retornado e por que ele não é necessariamente uma página física?

**Resposta:** Section. As páginas são calculadas durante a renderização, e a seção pode abranger várias delas.

### Exercício 2 — copiar a configuração padrão

```csharp
section.PageSetup = document.DefaultPageSetup.Clone();
```

Qual a finalidade?

**Resposta:** criar uma cópia da configuração padrão para personalizar a seção sem modificar diretamente o objeto DefaultPageSetup.

### Exercício 3 — definir A4

```csharp
section.PageSetup.PageFormat = PageFormat.A4;
```

### Exercício 4 — margens simétricas

Configure as margens laterais com 40 pontos e as verticais com 80 pontos.

```csharp
section.PageSetup.LeftMargin = Unit.FromPoint(40);
section.PageSetup.RightMargin = Unit.FromPoint(40);
section.PageSetup.TopMargin = Unit.FromPoint(80);
section.PageSetup.BottomMargin = Unit.FromPoint(80);
```

O exemplo utiliza unidades explícitas como complemento.

### Exercício 5 — calcular a área útil

Uma página de 595 × 842 unidades possui margens laterais de 40 e verticais de 80.

**Resposta:** largura útil de 515 e altura útil de 682 unidades, antes de outras restrições de layout.

### Perguntas de revisão

1. Qual configuração foi realizada na aula anterior?
2. Qual é a finalidade de CreatePage?
3. Qual argumento o método recebe?
4. Qual tipo o método retorna?
5. Qual namespace fornece Document e Section?
6. O que AddSection faz?
7. Section e página são exatamente o mesmo conceito?
8. Quando ocorre a paginação automática?
9. Por que uma seção pode ocupar várias páginas?
10. O que PageSetup configura?
11. O que DefaultPageSetup representa?
12. Por que o professor utiliza Clone?
13. Copiar uma referência cria automaticamente outro objeto?
14. Todo Clone garante cópia profunda?
15. Qual formato de papel é selecionado?
16. Quais são as dimensões nominais do A4 em milímetros?
17. Quais dimensões são utilizadas no Figma?
18. Qual a diferença entre A4 e PDF?
19. Quais margens são escolhidas?
20. Qual é a função de Unit.FromPoint?
21. Qual é a largura útil aproximada do template?
22. Qual é a altura útil aproximada?
23. Onde CreatePage é chamado?
24. O caso de uso já gera bytes válidos de um PDF?
25. Qual é a próxima etapa anunciada pelo professor?

## 16. Resumo e mapa mental

| Elemento | Resultado |
|---|---|
| Método auxiliar | CreatePage(Document document) |
| Tipo retornado | Section |
| Criação | document.AddSection() |
| Configuração inicial | document.DefaultPageSetup.Clone() |
| Formato | PageFormat.A4 |
| Template Figma | 595 × 842 |
| Margem esquerda | 40 |
| Margem direita | 40 |
| Margem superior | 80 |
| Margem inferior | 80 |
| Área nominal do template | 515 × 682 |
| Chamada | var page = CreatePage(document) |
| Paginação | Automática na renderização, conforme o conteúdo |
| Inserção de textos e valores | Próxima aula |
| Renderização e download | Pendentes |

```text
                    AULA 133 — PAGE SETUP
                              |
                     CreateDocument(month)
                              |
                      CreatePage(document)
                              |
                      document.AddSection()
                              |
                            Section
                              |
                 DefaultPageSetup.Clone()
                              |
                         PageSetup
                              |
          +-------------------+-------------------+
          |                   |                   |
       A4                Margens laterais    Margens verticais
    595 × 842*               40 / 40             80 / 80
          |                   |                   |
          +-------------------+-------------------+
                              |
                       return section
                              |
                     Próxima aula: conteúdo
                              |
                  Renderização futura do PDF

          * dimensões arredondadas do template.
```

**Conclusão:** a aula 133 adiciona a primeira seção ao modelo MigraDoc e prepara seu layout. O professor cria CreatePage, clona DefaultPageSetup, seleciona A4 e configura margens conforme o template Figma. A biblioteca poderá distribuir o conteúdo entre páginas na renderização, sem exigir que o desenvolvedor crie manualmente uma seção para cada página. O material conserva as escolhas do curso e diferencia a estrutura lógica do PDF físico, as medidas do template e as unidades tipográficas. A inserção de textos, despesas e a renderização ainda não foram realizadas.
