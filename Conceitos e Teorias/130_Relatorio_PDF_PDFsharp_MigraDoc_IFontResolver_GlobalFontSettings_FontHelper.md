# Aula 130 — Relatório PDF (parte 3): PDFsharp-MigraDoc, IFontResolver, GlobalFontSettings e FontHelper

> **Contexto:** na aula 129 foram incorporados quatro arquivos de fonte ao assembly da Application. Agora é necessário instalar a biblioteca de PDF e ensinar seu mecanismo de fontes a selecionar essas faces.
>
> **Resultado:** instalação do pacote PDFsharp-MigraDoc 6.1.0-preview-3, criação de ExpensesReportFontResolver, configuração de GlobalFontSettings.FontResolver, implementação de ResolveTypeface e criação de FontHelper. GetFont ainda lança NotImplementedException. A leitura dos arquivos e a geração do documento ficam para aulas posteriores.
>
> **Fidelidade:** a gravação utiliza nomes foneticamente transcritos. Adotamos os identificadores corretos GlobalFontSettings, IFontResolver, ResolveTypeface e GetFont. Raleway permanece uma identificação provável da família transcrita como halway, conforme a aula 129. A grafia das constantes e do caso de uso é reconstruída. O código do professor é distinguido das sugestões de melhoria. Não houve teste de renderização ou de requisições sucessivas.

## 1. Contexto e objetivo

A API já dispõe do endpoint GET de PDF, do caso de uso com consulta mensal e dos recursos tipográficos incorporados.

Incluir os arquivos .ttf no assembly não informa automaticamente à biblioteca qual arquivo representa determinada variante. É necessário estabelecer uma estratégia de resolução.

```text
Solicitação de fonte
        |
        v
Resolvedor personalizado
        |
        v
Identificação da face
        |
        v
Bytes do arquivo incorporado (próxima aula)
        |
        v
Renderização do PDF (futuro)
```

O professor pretende evitar depender de fontes instaladas no sistema operacional e escolher diretamente as variantes usadas no template visual.

## 2. PDFsharp e MigraDoc

| Componente | Responsabilidade geral |
|---|---|
| PDFsharp | Criação e manipulação de PDFs, incluindo operações gráficas |
| MigraDoc | Modelo de documento de nível mais alto, com seções, parágrafos, tabelas e estilos |
| PDFsharp-MigraDoc | Pacote que permite utilizar as duas bibliotecas em conjunto |

O pacote escolhido corresponde à variante Core, sem dependência específica do Windows, conforme a descrição da versão no NuGet.

O professor utiliza PdfSharp.Fonts para o resolvedor e MigraDoc.DocumentObjectModel para exemplificar a seleção de uma fonte.

```csharp
using PdfSharp.Fonts;
using MigraDoc.DocumentObjectModel;
```

A aula não cria a estrutura Document, não define páginas e não renderiza um PDF.

## 3. Instalação do pacote NuGet

No Visual Studio:

1. Clicar com o botão direito no projeto Application.
2. Selecionar Manage NuGet Packages.
3. Abrir Browse.
4. Buscar MigraDoc.
5. Selecionar PDFsharp-MigraDoc, observando o nome completo do pacote.
6. Habilitar Include Prerelease.
7. Selecionar 6.1.0-preview-3.
8. Instalar e confirmar.
9. Conferir o indicador de instalação e desmarcar Include Prerelease.

### Equivalente pela CLI — complemento

```bash
dotnet add package PDFsharp-MigraDoc --version 6.1.0-preview-3
```

### Referência no .csproj

```xml
<ItemGroup>
  <PackageReference Include="PDFsharp-MigraDoc"
                    Version="6.1.0-preview-3" />
</ItemGroup>
```

A versão exata está disponível no catálogo oficial: https://www.nuget.org/packages/PDFsharp-MigraDoc/6.1.0-preview-3

## 4. Versão estável versus preview

Na época da gravação, o professor indica 6.0.0 como a versão estável disponível. Menciona um bug em processo de correção e opta pelo pré-lançamento 6.1.0-preview-3.

A transcrição não identifica a falha, sua issue ou a correção específica.

| Versão | Papel na aula |
|---|---|
| 6.0.0 | Estável disponível naquele momento |
| 6.1.0-preview-3 | Versão instalada |

Um pré-lançamento pode incluir mudanças e correções ainda não estabilizadas. Uma versão posterior não garante compatibilidade automática com todos os exemplos.

Para reproduzir o curso, pode-se fixar a versão histórica. Em projetos novos, avalie uma versão estável compatível e sua documentação, em vez de instalar uma preview antiga sem necessidade.

A documentação de resolução de fontes 6.1 alerta que o mecanismo integrado mudou na versão 6.2. Portanto, não se deve aplicar indiscriminadamente exemplos de versões diferentes.

## 5. Organização e criação do resolvedor

O professor cria uma classe pública chamada ExpensesReportFontResolver dentro da pasta Fonts e implementa IFontResolver.

Estrutura conceitual:

```text
CashFlow.Application
└── UseCases
    └── Expenses
        └── Reports
            └── PDF
                ├── Interface e caso de uso
                └── Fonts
                    ├── Raleway-Regular.ttf
                    ├── Raleway-Black.ttf
                    ├── WorkSans-Regular.ttf
                    ├── WorkSans-Black.ttf
                    ├── ExpensesReportFontResolver.cs
                    └── FontHelper.cs
```

A identificação Raleway é provável, não confirmada visualmente na gravação. Confira os arquivos efetivos antes de copiar os nomes.

```csharp
using PdfSharp.Fonts;

public class ExpensesReportFontResolver : IFontResolver
{
    // Métodos exigidos pela interface.
}
```

O resolvedor pertence à integração com PDFsharp, não ao contrato de persistência do Domain. Ele adapta solicitações tipográficas aos recursos que a aplicação possui.

## 6. Interface IFontResolver

A interface exige duas operações. Na documentação 6.1, as assinaturas incluem a possibilidade de resultado nulo:

```csharp
FontResolverInfo? ResolveTypeface(
    string familyName, bool bold, bool italic);

byte[]? GetFont(string faceName);
```

As anotações de nulidade e os nomes dos parâmetros podem variar de acordo com a versão instalada.

| Método | Entrada | Responsabilidade |
|---|---|---|
| ResolveTypeface | Nome solicitado, negrito e itálico | Determinar uma face tipográfica |
| GetFont | Identificador da face | Fornecer os bytes da fonte |

Ao gerar os métodos pelo Visual Studio, o professor inicialmente obtém corpos com throw new NotImplementedException().

A resolução e o carregamento são etapas diferentes: saber o nome da face não significa possuir seus bytes.

O PDFsharp mantém resultados em cache. Não se deve presumir que cada parágrafo provoca uma nova leitura do arquivo.

## 7. GlobalFontSettings.FontResolver

No construtor do caso de uso PDF, o professor adiciona:

```csharp
GlobalFontSettings.FontResolver =
    new ExpensesReportFontResolver();
```

GlobalFontSettings está em PdfSharp.Fonts.

Reconstrução do construtor:

```csharp
public GenerateExpensesReportPdfUseCase(
    IExpensesReadOnlyRepository repository)
{
    _repository = repository;

    GlobalFontSettings.FontResolver =
        new ExpensesReportFontResolver();
}
```

O nome completo da classe do caso de uso é ilustrativo, conforme a aula 128.

A atribuição informa à biblioteca qual resolvedor personalizado deve utilizar. Não se trata de uma inscrição AddScoped de IFontResolver: a instância é criada explicitamente e registrada na configuração global do PDFsharp.

## 8. Static e ciclo de vida: ressalva arquitetural

O professor observa que FontResolver é uma propriedade estática. Pode ser acessada diretamente pelo tipo, sem criar um objeto GlobalFontSettings.

Uma configuração estática é compartilhada; não pertence isoladamente a uma instância do caso de uso.

O instrutor opta por configurá-la no construtor para manter os arquivos próximos ao relatório e menciona a alternativa de configurar na DependencyInjectionExtension, durante a inicialização.

### Correção técnica importante

A documentação oficial 6.1 informa que as propriedades dos resolvedores devem ser definidas uma única vez, antes da criação de um XFont.

Assim, colocar a atribuição no construtor de um caso de uso Scoped pode causar problemas quando outra requisição construir uma nova instância. A atribuição a cada relatório não cria uma configuração privada para aquele relatório.

O professor não testa esse cenário. A possível exceção em requisições repetidas é um risco técnico, não uma falha observada nesta aula.

### Alternativa recomendada — complemento

Configurar no startup, uma única vez, antes de qualquer resolução de fonte:

```csharp
GlobalFontSettings.FontResolver =
    new ExpensesReportFontResolver();

// Continuar a inicialização e registro de serviços.
```

É possível manter uma classe de configuração na Application, chamada no início da API. Se houver múltiplos relatórios com fontes diferentes, um resolvedor compartilhado pode atender todas as faces necessárias.

Uma guarda do tipo if (GlobalFontSettings.FontResolver is null) espalhada pelos construtores não é uma substituição adequada para uma inicialização centralizada, especialmente diante de concorrência.

Referência oficial: https://docs.pdfsharp.net/PDFsharp/Topics/Fonts/Font-Resolving-(6.1).html

## 9. FontResolverInfo e a seleção de fontes

O professor explica o conceito usando Font, do modelo de documento MigraDoc.

```csharp
var font = new Font
{
    Name = "Work Sans",
    Bold = false,
    Italic = false
};
```

O exemplo representa a intenção tipográfica. A criação de Font não implica necessariamente resolução imediata; isso pode acontecer quando o mecanismo de layout ou renderização precisar da fonte.

ResolveTypeface devolve uma instância de FontResolverInfo, contendo o identificador a ser utilizado na chamada posterior de GetFont.

```csharp
return new FontResolverInfo(faceName);
```

faceName é uma string interna definida pelo resolvedor. Não precisa coincidir com o nome real de uma família instalada.

O professor observa que não pode simplesmente chamar o construtor sem fornecer o identificador.

## 10. Estratégia convencional de família e estilo

Uma abordagem comum é utilizar uma combinação de nome, negrito e itálico para mapear a solicitação à face.

Exemplo conceitual discutido:

| Família | Bold | Italic | Alias possível |
|---|---|---|---|
| WorkSans | false | false | WorkSans |
| WorkSans | true | false | WorkSans#b |
| WorkSans | false | true | WorkSans#i |
| WorkSans | true | true | WorkSans#bi |

Os sufixos #b e #i não são exigências da biblioteca; são uma convenção de identificação interna.

O instrutor considera essa abordagem menos conveniente para distinguir vários pesos, como Thin, Light, Semi Bold e Black.

A interface não impede o uso de outros pesos. É possível criar aliases específicos ou implementar uma estratégia mais elaborada de mapeamento.

## 11. Estratégia adotada: aliases que identificam a variante

Em vez de solicitar uma família genérica e utilizar os booleanos para decidir qual arquivo será lido, o professor decide solicitar diretamente o nome-base do arquivo.

Exemplo:

```csharp
var font = new Font
{
    Name = "WorkSans-Regular"
};
```

Fluxo:

```text
Font.Name = WorkSans-Regular
            |
            v
ResolveTypeface("WorkSans-Regular", false, false)
            |
            v
FontResolverInfo("WorkSans-Regular")
            |
            v
GetFont("WorkSans-Regular")
            |
            v
WorkSans-Regular.ttf (leitura futura)
```

A intenção é reduzir condicionais, pois a variante já foi escolhida pelo chamador.

| Estratégia | Família + estilo | Alias da variante |
|---|---|---|
| Identificação | Nome lógico e booleanos | Nome específico da face |
| Controle de pesos | Mapeamento adicional | Um alias por peso |
| Acoplamento ao arquivo | Pode ser ocultado | Direto no desenho do curso |
| Bold/Italic | Usados na decisão | Ignorados na implementação atual |

A escolha funciona como convenção interna. Ela exige que todos os aliases utilizados correspondam a recursos válidos.

## 12. Implementação de ResolveTypeface

O professor conclui este método:

```csharp
public FontResolverInfo ResolveTypeface(
    string familyName,
    bool isBold,
    bool isItalic)
{
    return new FontResolverInfo(familyName);
}
```

Os argumentos booleanos permanecem na assinatura, porque fazem parte do contrato, mas não participam da decisão.

Ao receber WorkSans-Black, o método devolve um identificador para a variante Black, independentemente de isBold.

A implementação não valida o nome, não fornece fallback nem seleciona automaticamente estilos solicitados por terceiros.

### Atenção

PDFsharp pode solicitar fontes utilizadas por estilos padrão ou outros elementos além daqueles configurados explicitamente pelo desenvolvedor.

Um resolvedor que sempre afirma reconhecer qualquer nome, mas só dispõe de quatro arquivos, precisa ser avaliado quando o layout completo for criado.

A documentação 6.1 permite indicar uma solicitação não atendida com resultado nulo. O tratamento depende da configuração e dos demais resolvedores disponíveis. Não se deve pressupor fallback automático para fontes do sistema.

## 13. FontHelper e constantes

Para evitar nomes de arquivos hardcoded em várias partes do projeto, o professor cria FontHelper com quatro constantes públicas.

Reconstrução:

```csharp
public class FontHelper
{
    public const string RALEWAY_REGULAR = "Raleway-Regular";
    public const string RALEWAY_BLACK = "Raleway-Black";

    public const string WORKSANS_REGULAR = "WorkSans-Regular";
    public const string WORKSANS_BLACK = "WorkSans-Black";
}
```

Os identificadores e a primeira família são reconstruídos. Os valores devem corresponder exatamente aos nomes-base dos arquivos escolhidos. A próxima aula mostrará como completar o nome do recurso com caminho lógico e extensão.

Uso:

```csharp
var font = new Font
{
    Name = FontHelper.WORKSANS_REGULAR
};
```

### Por que const?

A constante tem valor conhecido em tempo de compilação, não pode ser reatribuída e é acessível pelo nome do tipo.

Isso retoma a aula 126, na qual o professor centralizou CurrencySymbol.

A centralização facilita manutenção e evita repetir strings. Ela não garante que o valor da constante corresponda a um arquivo realmente incorporado.

Como complemento, uma classe que contém apenas constantes pode ser declarada static. A transcrição não confirma que o professor tenha utilizado esse modificador.

## 14. Código consolidado da etapa atual

```csharp
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
        throw new NotImplementedException();
    }
}
```

O código representa fielmente a implementação parcial. A assinatura pode ser ajustada às anotações de nulidade da versão instalada.

O caso de uso continua consultando o repositório e retornando um array vazio provisório quando existem despesas:

```csharp
public async Task<byte[]> Execute(DateOnly month)
{
    var expenses = await _repository.FilterByMonth(month);

    if (expenses.Count == 0)
        return [];

    // Geração do PDF ainda não implementada.

    return [];
}
```

Não existe documento PDF gerado nesta aula. A utilização de Font é explicativa e não comprova que um objeto de fonte permaneceu em Execute.

## 15. GetFont e recursos incorporados

GetFont receberá o identificador devolvido por ResolveTypeface e deverá recuperar os bytes da fonte.

A aula termina antes de implementar essa leitura.

O mecanismo geral foi antecipado como complemento na aula 129:

```csharp
var assembly = typeof(ExpensesReportFontResolver).Assembly;

var names = assembly.GetManifestResourceNames();
```

Esses nomes permitem identificar os recursos efetivamente presentes no assembly.

Na próxima etapa, será necessário abrir o recurso com GetManifestResourceStream, ler seu conteúdo e devolvê-lo como byte[].

Não se deve utilizar um caminho físico de desenvolvimento como se fosse o nome de manifesto. O nome real depende de RootNamespace, caminho e metadados do projeto.

O professor anuncia explicitamente Stream e array de bytes como assunto da próxima gravação.

## 16. Complementos técnicos e boas práticas

### 16.1 Separar alias e nome de recurso

Em um sistema maior, uma tabela de mapeamento pode permitir que o nome utilizado no código permaneça estável mesmo quando o nome do arquivo mudar.

Por exemplo, um alias de aplicação como ReportTotal poderia apontar para WorkSans-Black. Esse desenho não é adotado nesta aula.

### 16.2 Resolver desconhecido

Uma implementação que atende apenas fontes específicas pode validar seus aliases antes de criar FontResolverInfo, devolvendo null quando não houver correspondência, de acordo com o contrato da versão.

Não retorne arbitrariamente os bytes de outra face sem uma política explícita de fallback.

### 16.3 Cache e concorrência

Como a biblioteca mantém caches de fontes, a configuração deve ser planejada para o ciclo de vida da aplicação. Evite trocar o resolver durante a geração concorrente de relatórios.

### 16.4 Portabilidade e licença

Incorporar as fontes ao assembly facilita a distribuição, mas ainda é necessário confirmar a compatibilidade da biblioteca, os caracteres suportados e as condições de licença das fontes e do pacote.

A aula não apresenta testes em Linux, contêineres ou ambientes de publicação.

### 16.5 Testes sugeridos

| Cenário | Objetivo |
|---|---|
| Resolver WorkSans-Regular | Retornar o alias esperado |
| Resolver WorkSans-Black | Selecionar a variante específica |
| Alias desconhecido | Comportamento explícito e previsível |
| Arquivo incorporado inexistente | Diagnóstico claro |
| Requisições consecutivas e concorrentes | Verificar inicialização única |
| Geração real do PDF | Validar fontes e caracteres renderizados |

Nenhum desses testes adicionais é executado na gravação.

## 17. Exercícios e perguntas de revisão

1. Qual pacote NuGet foi instalado e em qual projeto?
2. Qual versão foi escolhida na gravação?
3. Por que o professor habilita Include Prerelease?
4. Qual diferença existe entre PDFsharp e MigraDoc?
5. Qual interface é implementada?
6. O que ResolveTypeface retorna?
7. O que GetFont deverá devolver?
8. Qual propriedade configura o resolvedor global?
9. Por que uma propriedade estática não é exclusiva do caso de uso?
10. Qual risco existe na atribuição repetida dentro de construtores Scoped?
11. Qual é o momento recomendado para configurar o resolvedor?
12. O que representam isBold e isItalic?
13. Por que o professor prefere aliases como WorkSans-Black?
14. Qual é a finalidade de FontResolverInfo?
15. Qual a vantagem de centralizar nomes em FontHelper?
16. Uma constante garante que o arquivo exista?
17. O que ocorre se GetFont for invocado na implementação atual?
18. A aula gera um PDF funcional?
19. Qual etapa fica para a próxima aula?

### Respostas essenciais

- ResolveTypeface identifica uma face; GetFont fornece os bytes.
- A configuração global deve ser feita uma única vez, antes do uso de fontes.
- O helper centraliza identificadores, não carrega os arquivos.
- GetFont continua lançando NotImplementedException, portanto a implementação não está pronta para renderizar.

## 18. Resumo e mapa mental

```text
                    AULA 130 — RESOLUÇÃO DE FONTES
                                  |
                       PDFsharp-MigraDoc
                        6.1.0-preview-3
                                  |
                         IFontResolver
                                  |
                 +----------------+----------------+
                 |                                 |
           ResolveTypeface                       GetFont
                 |                                 |
       FontResolverInfo(familyName)        NotImplementedException
                 |                                 |
             Alias da face                  Próxima aula: Stream
                 |
             FontHelper
                 |
        Quatro constantes de nome
                 |
      GlobalFontSettings.FontResolver
                 |
      Configuração global do PDFsharp
```

| Elemento | Estado |
|---|---|
| Pacote PDF | Instalado |
| ExpensesReportFontResolver | Criado |
| ResolveTypeface | Implementado |
| FontHelper | Criado |
| Registro global | Demonstrado no construtor; startup recomendado |
| GetFont | Pendente |
| Renderização | Pendente |

### Referências oficiais

- NuGet, versão histórica: https://www.nuget.org/packages/PDFsharp-MigraDoc/6.1.0-preview-3
- Resolução de fontes, documentação PDFsharp 6.1: https://docs.pdfsharp.net/PDFsharp/Topics/Fonts/Font-Resolving-(6.1).html

**Conclusão:** a aula prepara a integração entre o gerador PDF e as fontes incorporadas. O professor escolhe aliases baseados nos nomes dos arquivos e os centraliza em FontHelper. ResolveTypeface repassa o alias para FontResolverInfo. GetFont ainda precisa recuperar os bytes dos recursos. A configuração global demonstrada foi preservada, com a recomendação técnica separada de inicializar o resolvedor uma única vez no startup, conforme a documentação oficial.
