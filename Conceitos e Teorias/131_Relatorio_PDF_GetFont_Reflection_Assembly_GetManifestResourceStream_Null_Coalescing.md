# Aula 131 — Relatório PDF (parte 4): GetFont, Reflection, recursos incorporados e Stream.Read

> **Contexto:** a aula 130 criou o resolvedor personalizado de fontes, implementou ResolveTypeface e centralizou os aliases em FontHelper. Nesta aula, o professor termina GetFont, recuperando os arquivos TTF incorporados ao assembly da Application.
>
> **Fidelidade:** a transcrição confunde repetidamente Stream com string. O tipo correto é Stream. A pasta Pdf, com P maiúsculo e df minúsculos, é explicitamente confirmada nesta gravação. Raleway continua sendo a identificação provável da família transcrita foneticamente como Halloween. Os nomes das constantes são reconstruídos conforme a aula anterior. A implementação original é apresentada separadamente das correções técnicas. Não houve teste de renderização.

## 1. Objetivo e integração com IFontResolver

A biblioteca PDFsharp precisa obter a representação binária das fontes usadas na renderização. ResolveTypeface identifica uma face; GetFont fornece seus bytes.

```text
Font.Name = WorkSans-Regular
         |
         v
ResolveTypeface(...)
         |
         v
FontResolverInfo("WorkSans-Regular")
         |
         v
GetFont("WorkSans-Regular")
         |
         v
Recurso incorporado -> Stream -> byte[]
```

As fontes foram configuradas como Embedded Resource na aula 129. A estratégia não depende de um arquivo físico em uma pasta de downloads ou de uma instalação da fonte no sistema operacional.

## 2. Método auxiliar ReadFontFile

O professor cria um método privado para recuperar o stream do recurso:

```csharp
private Stream? ReadFontFile(string faceName)
{
    var assembly = Assembly.GetExecutingAssembly();

    return assembly.GetManifestResourceStream(
        $"CashFlow.Application.UseCases.Expenses.Reports.Pdf.Fonts.{faceName}.ttf");
}
```

O namespace necessário é System.Reflection. System.IO contém Stream e pode estar disponível por implicit usings.

O nome do recurso acima foi reconstruído a partir da sequência de pastas narrada. A gravação confirma expressamente a capitalização Pdf. A estrutura pode divergir dos exemplos ilustrativos com PDF em maiúsculas registrados nas aulas 128–130.

## 3. Assembly e Reflection

Assembly.GetExecutingAssembly() devolve uma referência ao assembly que contém o código em execução.

Como a classe ExpensesReportFontResolver pertence à Application, a chamada recupera, nesse contexto, o assembly dessa camada.

```csharp
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
```

O assembly é uma unidade de identidade, carregamento e distribuição do .NET. Em projetos de biblioteca, costuma ser produzido como DLL e pode conter código intermediário, metadados e recursos incorporados.

**Precisão técnica:** um assembly não contém necessariamente todas as dependências necessárias ao funcionamento do projeto. Outros assemblies, o runtime, arquivos externos e serviços podem ser necessários. A referência Assembly não equivale a um caminho de arquivo físico.

Reflection permite inspecionar metadados e recursos em tempo de execução.

| Método | Assembly selecionado |
|---|---|
| Assembly.GetExecutingAssembly() | Assembly que contém o código em execução |
| Assembly.GetEntryAssembly() | Assembly de entrada, quando disponível |
| typeof(ExpensesReportFontResolver).Assembly | Assembly que define o tipo informado |

A primeira forma é a demonstrada. A terceira é uma alternativa explícita para identificar a Application, inclusive se a lógica de chamada for reorganizada.

## 4. GetManifestResourceStream

```csharp
return assembly.GetManifestResourceStream(resourceName);
```

Esse método permite obter um stream para um recurso incorporado identificado por seu nome de manifesto.

O professor enfatiza que faceName contém apenas o nome-base, por exemplo WorkSans-Regular. É necessário compor o nome completo do recurso.

```text
CashFlow.Application
    .UseCases
    .Expenses
    .Reports
    .Pdf
    .Fonts
    .WorkSans-Regular.ttf
```

Na convenção utilizada pelo projeto, os pontos representam a composição do nome lógico a partir do namespace raiz e das pastas.

**Precisão:** não é uma navegação real por diretórios internos da DLL. Trata-se de um identificador de manifesto. Seu valor pode depender de RootNamespace, LogicalName e outras configurações do MSBuild. Não é obrigatório que todo recurso comece com o nome do projeto.

O nome do recurso distingue maiúsculas e minúsculas.

### Diagnóstico complementar

```csharp
foreach (var name in assembly.GetManifestResourceNames())
{
    Console.WriteLine(name);
}
```

Esse código lista os nomes reais disponíveis e permite verificar o caminho lógico quando a leitura falhar. Não foi executado pelo professor.

## 5. Stream? e nullable reference types

A assinatura utiliza Stream?:

```csharp
private Stream? ReadFontFile(string faceName)
```

O ponto de interrogação indica que o retorno pode ser nulo.

GetManifestResourceStream pode retornar null quando o recurso não é localizado ou não está acessível. Outros problemas também podem provocar exceções, conforme o contrato do método.

| Tipo | Significado em um contexto nullable habilitado |
|---|---|
| Stream | Referência esperada como não nula |
| Stream? | Referência que pode ser nula |

A anotação nullable orienta a análise estática do compilador. Não transforma a referência em outro objeto, nem impede que uma referência nula apareça em tempo de execução.

## 6. Fonte padrão e fallback

O professor acrescenta ao FontHelper uma constante DefaultFont que referencia a variante Regular da segunda família.

Reconstrução:

```csharp
public const string DEFAULT_FONT = RALEWAY_REGULAR;
```

A capitalização do identificador é ilustrativa. Confirme os nomes reais do helper.

O objetivo é oferecer uma fonte alternativa quando um desenvolvedor solicitar um alias que não corresponde aos arquivos disponíveis, por exemplo Roboto.

```csharp
var stream = ReadFontFile(faceName);

if (stream is null)
    stream = ReadFontFile(FontHelper.DEFAULT_FONT);
```

A decisão é uma política de fallback do curso. Não se trata de busca automática de fontes do sistema operacional.

**Limitação:** a fonte padrão também pode estar ausente, por exemplo, se a Build Action estiver errada ou o nome do manifesto não corresponder ao arquivo. Portanto, a segunda tentativa não constitui uma garantia absoluta de resultado não nulo.

## 7. Operador de atribuição de coalescência nula

O Visual Studio sugere simplificar a condição, e o professor escolhe:

```csharp
var stream = ReadFontFile(faceName);

stream ??= ReadFontFile(FontHelper.DEFAULT_FONT);
```

O operador ??=, introduzido no C# 8, atribui o resultado do lado direito somente quando o operando esquerdo é nulo.

| Estado inicial | Efeito |
|---|---|
| Stream encontrado | Preserva o valor existente |
| Stream nulo | Avalia ReadFontFile com a fonte padrão e atribui o resultado |

Diferença entre operadores:

```csharp
var stream = ReadFontFile(faceName)
    ?? ReadFontFile(FontHelper.DEFAULT_FONT);
```

?? produz o primeiro valor não nulo ou o resultado da alternativa. ??= também realiza a atribuição condicional ao operando esquerdo.

A forma com if é equivalente no cenário simples demonstrado.

## 8. Stream versus MemoryStream

No Excel, a aula 122 utilizava MemoryStream.ToArray().

Aqui, ReadFontFile declara o retorno como Stream?, um tipo abstrato que não fornece o método ToArray de MemoryStream.

```csharp
Stream stream = new MemoryStream();

// stream.ToArray() não compila pelo contrato de Stream.
```

Mesmo que um objeto seja de um tipo derivado em tempo de execução, o compilador usa o tipo estático da expressão para determinar os membros acessíveis.

Não é adequado realizar um cast para MemoryStream sem conhecer a implementação concreta.

O professor escolhe ler diretamente os bytes por Stream.Read.

## 9. Length, long e alocação do buffer

```csharp
var size = stream!.Length;

var data = new byte[size];
```

Length informa o comprimento do stream quando a implementação suporta essa operação. O tipo retornado é long (Int64).

O array representa o buffer de destino dos bytes da fonte.

Conhecer o tamanho antecipadamente permite alocar o buffer uma vez, evitando crescimento incremental nesse cenário.

**Precisões:** nem todo stream suporta Length. Além disso, comprimento válido como long não garante que um array dessa dimensão possa ser alocado. O código do curso presume arquivos de fonte pequenos e streams com tamanho disponível.

### Operador null-forgiving

O professor insere ! após stream para eliminar o aviso de possível nulidade:

```csharp
var size = stream!.Length;
```

O operador pós-fixado ! comunica ao compilador que o programador assume que a referência não é nula.

Ele não testa a referência, não cria um objeto, não lança uma exceção personalizada e não altera o valor em execução. Se stream for nulo, a leitura de Length ainda poderá causar NullReferenceException.

## 10. Stream.Read e argumentos nomeados

A leitura demonstrada é:

```csharp
stream.Read(
    buffer: data,
    offset: 0,
    count: (int)size);

return data;
```

| Argumento | Responsabilidade |
|---|---|
| buffer | Array que receberá os bytes |
| offset | Índice inicial de escrita no array de destino |
| count | Número máximo de bytes a ler |

O offset não é a posição inicial do arquivo. A leitura da origem começa na posição atual do stream. Para escolher outra posição na origem seria necessário utilizar Position ou Seek, quando suportados.

Exemplo didático: em um buffer de dez posições, offset 2 e count 3 escrevem até três bytes nas posições 2, 3 e 4. Isso não descarta automaticamente os dois primeiros bytes da origem.

### Cast de long para int

Count exige int. O professor realiza o cast explícito (int)size.

Um cast de estreitamento pode produzir valor incorreto quando o long não cabe no intervalo de int. Em código robusto, é necessário verificar o limite antes da conversão ou utilizar checked.

## 11. Implementação consolidada da aula

```csharp
using System.Reflection;
using PdfSharp.Fonts;

public class ExpensesReportFontResolver : IFontResolver
{
    public FontResolverInfo ResolveTypeface(
        string familyName,
        bool isBold,
        bool isItalic)
    {
        return new FontResolverInfo(familyName);
    }

    public byte[] GetFont(string faceName)
    {
        var stream = ReadFontFile(faceName);

        stream ??= ReadFontFile(FontHelper.DEFAULT_FONT);

        var size = stream!.Length;
        var data = new byte[size];

        stream.Read(
            buffer: data,
            offset: 0,
            count: (int)size);

        return data;
    }

    private Stream? ReadFontFile(string faceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return assembly.GetManifestResourceStream(
            $"CashFlow.Application.UseCases.Expenses.Reports.Pdf.Fonts.{faceName}.ttf");
    }
}
```

Os nomes locais e a assinatura seguem a reconstrução da narração. A versão da interface pode apresentar anotações nullable diferentes.

O professor não demonstra descarte do stream, tratamento de uma segunda leitura nula ou verificação do retorno de Read.

## 12. Correções técnicas: leitura parcial e descarte

**Complementos, não mudanças realizadas na gravação.**

Stream.Read devolve a quantidade efetivamente lida. Ela pode ser menor que count mesmo quando ainda existem bytes disponíveis. Ignorar esse retorno não é uma estratégia geral segura para ler todo o conteúdo.

Em versões modernas do .NET, ReadExactly pode preencher o buffer solicitado ou lançar EndOfStreamException se o conteúdo terminar antes.

```csharp
public byte[] GetFont(string faceName)
{
    using var stream =
        ReadFontFile(faceName)
        ?? ReadFontFile(FontHelper.DEFAULT_FONT)
        ?? throw new InvalidOperationException(
            "Não foi possível carregar a fonte padrão do relatório.");

    var data = new byte[checked((int)stream.Length)];

    stream.ReadExactly(data);

    return data;
}
```

ReadExactly está disponível a partir do .NET 7. O exemplo pressupõe que Length é suportado e que o tamanho é compatível com uma alocação válida.

O operador ?? throw trata explicitamente a ausência de ambos os recursos. using descarta o stream quando o método termina, inclusive no fluxo normal de exceções.

Outra alternativa, aplicável a streams legíveis que não ofereçam Length, é CopyTo:

```csharp
public byte[] GetFont(string faceName)
{
    using var stream =
        ReadFontFile(faceName)
        ?? ReadFontFile(FontHelper.DEFAULT_FONT)
        ?? throw new InvalidOperationException(
            "Fonte padrão não encontrada.");

    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);

    return buffer.ToArray();
}
```

A segunda solução pode utilizar um buffer expansível e uma cópia adicional. A escolha depende do tamanho e do contrato da origem. Nenhuma das duas foi testada na aula.

## 13. Política de fallback e configuração global

O fallback no GetFont permite continuar a renderização com outra face, mas pode ocultar erros de alias e gerar diferenças visuais. Em sistemas com identidade visual rígida, pode ser preferível rejeitar um alias incorreto ou registrar um diagnóstico.

Como explicado na aula 130, GlobalFontSettings.FontResolver deve ser configurado uma única vez antes da resolução das fontes. Não é necessário reinstanciar o resolvedor em cada requisição.

O curso mantém sua convenção de aliases explícitos. Os booleanos de ResolveTypeface continuam ignorados.

## 14. Exercícios e revisão

1. Qual método recupera o assembly que contém o código em execução?
2. Por que as fontes precisam de Build Action Embedded Resource?
3. Qual é a diferença entre nome de arquivo e nome de manifesto?
4. Por que a capitalização Pdf importa?
5. Qual é a finalidade de GetManifestResourceNames?
6. Por que ReadFontFile devolve Stream?
7. O que significa a anotação Stream??
8. Como o fallback funciona?
9. Qual é a diferença entre ?? e ??=?
10. O que o operador ! faz e o que não faz?
11. Por que Stream não expõe ToArray?
12. Qual é o tipo de Length?
13. O que representa offset em Stream.Read?
14. Read garante preencher todo o array numa chamada?
15. Como ReadExactly ou CopyTo podem evitar leitura parcial?
16. Por que usar using no stream?
17. Qual parte da geração de PDF ainda não existe?

### Respostas essenciais

- GetExecutingAssembly recupera o assembly em execução; GetManifestResourceStream recupera o recurso pelo nome lógico.
- O fallback tenta a fonte padrão quando a primeira leitura retorna null.
- ??= realiza atribuição apenas quando necessário; ! somente suprime a advertência de nulidade.
- Offset é o índice de destino; Read devolve a quantidade efetivamente lida.
- GetFont está implementado, mas não foi validado por uma renderização real nesta aula.

## 15. Resumo e mapa mental

```text
                 GETFONT
                    |
          ReadFontFile(faceName)
                    |
          GetExecutingAssembly()
                    |
          GetManifestResourceStream
                    |
            Stream encontrado?
              /           \
            não           sim
             |             |
      DEFAULT_FONT         |
             +------+------+
                    |
                  Length
                    |
                new byte[]
                    |
                 Read(...)
                    |
                  byte[]
                    |
             PDFsharp (futuro)
```

| Etapa | Estado |
|---|---|
| ResolveTypeface | Concluído anteriormente |
| FontHelper | Acrescentada fonte padrão |
| ReadFontFile | Implementado |
| GetFont | Implementado |
| Fallback e ??= | Demonstrados |
| Leitura completa e descarte | Melhorias complementares |
| Renderização de PDF | Pendente |

### Referências oficiais para aprofundamento

- https://learn.microsoft.com/dotnet/api/system.reflection.assembly.getexecutingassembly
- https://learn.microsoft.com/dotnet/api/system.reflection.assembly.getmanifestresourcestream
- https://learn.microsoft.com/dotnet/api/system.io.stream.read
- https://learn.microsoft.com/dotnet/api/system.io.stream.readexactly

**Conclusão:** a aula finaliza a implementação inicial do resolvedor, recuperando os arquivos TTF como streams de recursos incorporados, aplicando fallback e preenchendo um array de bytes. O código original é mantido para estudo, acompanhado de correções explícitas sobre offset, leitura parcial, nulidade, cast e descarte. A criação do modelo Document é a etapa seguinte.
