# Aula 120 — Relatório Excel (parte 3): ClosedXML, XLWorkbook, Worksheet, licença MIT e formatação cultural

> **Contexto:** na aula 118 foi preparado o endpoint de download de arquivos; na aula 119 foi definido o recebimento do período por [FromHeader] DateOnly month. Agora começa a construção do documento Excel.
>
> **Objetivo:** conectar o controller ao caso de uso, instalar ClosedXML no projeto Application, analisar sua licença e configurar o workbook e sua primeira worksheet.
>
> **Escopo:** esta aula não preenche cabeçalhos, não consulta despesas, não salva o workbook em um stream e não demonstra o download de uma planilha válida. A próxima aula será dedicada aos cabeçalhos.
>
> **Fidelidade:** os nomes exatos da interface e da classe do use case, a assinatura completa de Execute e a grafia do autor não foram fornecidos literalmente na transcrição. Os exemplos usam nomes ilustrativos coerentes com o CashFlow. Complementos técnicos são identificados separadamente.

## Índice

1. Recapitulação e objetivo
2. Organização dos casos de uso de relatórios
3. Navegação no Solution Explorer
4. Contrato do caso de uso e DI
5. Integração com ReportController
6. ClosedXML e instalação pelo NuGet
7. Licença MIT e dependências
8. Workbook versus Worksheet
9. Criando XLWorkbook e configurando Author
10. Estilos padrão e fontes
11. Adicionando a primeira aba
12. Formatação de datas com ToString("Y")
13. Cultura, localização e Culture Middleware
14. Código consolidado da etapa atual
15. Cuidados técnicos e etapas futuras
16. Exercícios e revisão
17. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

O CashFlow está ganhando um módulo de relatórios mensais de despesas.

As etapas anteriores estabeleceram o seguinte contrato:

- Endpoint GET de Excel em ReportController.
- Recebimento do período por [FromHeader] DateOnly month.
- Retorno de conteúdo binário por File().
- HTTP 200 quando há conteúdo para download.
- HTTP 204 quando não há despesas para exportar.

O endpoint ainda usava new byte[1] para simular o futuro arquivo. Nesta aula, o professor começa a substituir essa simulação por uma estrutura real de geração de planilhas.

O primeiro passo não é preencher células ou buscar despesas, mas organizar o caso de uso e configurar o documento que posteriormente receberá as informações.

```text
ReportController
      |
      v
Use case de relatório Excel
      |
      v
XLWorkbook
      |
      v
Worksheet com nome do mês e ano
      |
      v
Cabeçalhos e dados (próximas aulas)
      |
      v
byte[] (resultado pretendido)
```

## 2. Organização dos casos de uso de relatórios

O professor apresenta a interface e a classe do novo caso de uso, criadas antes da gravação.

Dentro da pasta de despesas da Application, cria Reports e, dentro dela, Excel.

Estrutura conceitual:

```text
CashFlow.Application
└── UseCases
    └── Expenses
        ├── Register
        ├── Update
        ├── Delete
        ├── ...
        └── Reports
            ├── Excel
            │   ├── IGenerateExpensesReportExcelUseCase.cs
            │   └── GenerateExpensesReportExcelUseCase.cs
            └── PDF (planejado)
```

**Os nomes dos dois arquivos são ilustrativos.** A transcrição confirma a existência da interface e da classe, mas não enuncia seus identificadores literais.

### Por que separar Excel e PDF?

O relatório de despesas possui uma finalidade comum, mas os mecanismos de geração dependem do formato escolhido.

O Excel trabalha com planilhas, células, fórmulas e estilos. O PDF possui outra estrutura de apresentação, relacionada a páginas, layout e posicionamento de conteúdo.

Separar os casos de uso por formato permite organizar suas particularidades sem concentrar toda a lógica em uma única classe.

A pasta PDF é anunciada para o futuro; não foi criada uma implementação de PDF nesta aula.

## 3. Navegação no Solution Explorer

O instrutor aproveita para mostrar uma funcionalidade do Visual Studio que ajuda a encontrar o arquivo atualmente aberto dentro da estrutura da solução.

### Procedimento

1. Selecionar a aba do arquivo que deseja localizar.
2. No Solution Explorer, utilizar a opção que sincroniza a árvore com o documento ativo (Sync with Active Document).
3. O Visual Studio expande a estrutura necessária e seleciona o arquivo correspondente.

Também demonstra a opção de recolher todos os nós da árvore (Collapse All).

Esses recursos são úteis em soluções com vários projetos e pastas aninhadas, especialmente quando o desenvolvedor navega diretamente para uma classe por meio de referências ou da pesquisa do editor.

O professor utiliza ainda Ctrl+D para duplicar a linha de registro de dependência. O comportamento do atalho depende do perfil de teclas configurado no Visual Studio.

## 4. Contrato do caso de uso e DI

O professor registra a interface do caso de uso e sua implementação no serviço de injeção de dependência da Application.

A estrutura abaixo é reconstruída conforme o padrão das aulas anteriores:

```csharp
services.AddScoped<
    IGenerateExpensesReportExcelUseCase,
    GenerateExpensesReportExcelUseCase>();
```

O registro permite que o controller solicite a abstração e receba a implementação correspondente.

### Contrato pretendido

O método Execute recebe o período e devolve os bytes que representarão o relatório.

Uma assinatura coerente com o fluxo assíncrono previsto é:

```csharp
public interface IGenerateExpensesReportExcelUseCase
{
    Task<byte[]> Execute(DateOnly month);
}
```

O tipo de período DateOnly e o resultado byte[] são confirmados na gravação. O nome da interface e a assinatura assíncrona completa são reconstruções coerentes com a preparação anterior, que prevê acesso assíncrono ao banco.

### Por que registrar antes de testar?

A criação da classe e da interface não é suficiente para o container resolver automaticamente um serviço.

O registro é necessário para a injeção do caso de uso. Essa preocupação retoma o problema demonstrado na aula 115, quando a falta de uma inscrição na DI provocou HTTP 500.

Não há novo teste de falha de DI nesta aula.

## 5. Integração com ReportController

O professor volta ao controller e mostra que o caso de uso é recebido pelo serviço de injeção de dependência.

A action chama Execute passando month.

Exemplo de integração reconstruído:

```csharp
[HttpGet("excel")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> GetExcel(
    [FromHeader] DateOnly month,
    [FromServices] IGenerateExpensesReportExcelUseCase useCase)
{
    var file = await useCase.Execute(month);

    if (file.Length > 0)
        return File(
            file,
            MediaTypeNames.Application.Octet,
            "report.xlsx");

    return NoContent();
}
```

A rota e os resultados HTTP vêm das aulas 118 e 119. A ordem dos argumentos, os identificadores e o await são apresentados como reconstrução do desenho assíncrono.

### Separação de responsabilidades

| Componente | Responsabilidade |
|---|---|
| Controller | Receber o período e produzir a resposta HTTP |
| Use case | Coordenar a geração do relatório e fornecer o conteúdo |
| ClosedXML | Criar e manipular a estrutura da planilha |
| Repositório | Futuramente consultar as despesas do período |

A geração real dos bytes ainda não está concluída. O trecho representa o contrato de integração, e não a comprovação de um download funcional.

## 6. ClosedXML e instalação pelo NuGet

O pacote escolhido pelo professor é **ClosedXML**.

ClosedXML é uma biblioteca .NET para manipular documentos de planilha compatíveis com Excel, incluindo criação de workbooks, worksheets, células, estilos e fórmulas.

O professor apresenta seu repositório GitHub e recomenda ler a documentação antes de utilizar a biblioteca.

- Repositório: https://github.com/ClosedXML/ClosedXML
- Documentação: https://docs.closedxml.io/
- Pacote: https://www.nuget.org/packages/ClosedXML

### Instalação demonstrada

1. Clicar com o botão direito no projeto Application.
2. Selecionar Manage NuGet Packages.
3. Abrir a aba Browse.
4. Pesquisar ClosedXML.
5. Selecionar o pacote.
6. Clicar em Install e confirmar as alterações.
7. Aceitar os termos de licença apresentados pelo gerenciador.
8. Conferir a indicação de que o pacote está instalado.

A transcrição não informa a versão exata instalada. Não se deve atribuir uma versão específica ao curso.

### Alternativa via terminal — complemento

No diretório do projeto Application, a instalação também pode ser feita com:

```bash
dotnet add package ClosedXML
```

Esse comando é uma alternativa à interface gráfica, não o procedimento executado pelo instrutor.

O namespace utilizado para os tipos de Excel é:

```csharp
using ClosedXML.Excel;
```

### Por que no projeto Application?

O caso de uso de relatório está nessa camada e utilizará a biblioteca para gerar o documento.

Essa é a escolha arquitetural do curso. Em outra arquitetura, a geração poderia ser abstraída por um contrato e implementada em um adaptador da Infrastructure. Não há essa refatoração na gravação.

## 7. Licença MIT e dependências

O professor chama atenção para a aba de licença do pacote e destaca a licença MIT.

A lição é verificar as condições de uso antes de incorporar uma biblioteca a um projeto pessoal, acadêmico ou comercial.

### O que a licença MIT permite?

De modo geral, a MIT é uma licença permissiva. Autoriza usar, copiar, modificar, distribuir, sublicenciar e comercializar o software, observadas suas condições.

Entre as obrigações está preservar o aviso de copyright e o texto de permissão nas cópias ou partes substanciais do software abrangido pela licença. A licença também contém uma cláusula de ausência de garantia.

Isso não significa que todo software distribuído com uma dependência MIT precise ser licenciado sob MIT.

Referência: https://github.com/ClosedXML/ClosedXML/blob/develop/LICENSE

### Cuidado com a generalização da explicação

O professor cita bibliotecas que possuem restrições de uso comercial, licenças educacionais e condições para projetos de código aberto.

É importante distinguir:

| Categoria | Característica geral |
|---|---|
| Licença permissiva, como MIT | Permite ampla reutilização, inclusive comercial, mediante condições |
| Licença copyleft | Pode exigir condições de licenciamento e disponibilização de código em determinadas distribuições; não proíbe necessariamente obter receita |
| Licença não comercial | Restringe usos comerciais conforme sua redação |
| Licença proprietária/comercial | Pode exigir pagamento ou limitar utilização conforme seus termos |

**Código aberto não significa ausência de possibilidade de lucro.** Licenças open source, no sentido usual da definição da Open Source Initiative, não discriminam campos de atividade, inclusive atividade comercial.

Também não se pode concluir que a maioria das bibliotecas de Excel e PDF seja paga apenas a partir da afirmação oral do professor; a gravação não apresenta levantamento de mercado.

### Checklist antes de instalar uma biblioteca

- Verificar a licença da versão utilizada.
- Conferir se o uso pretendido é permitido.
- Observar dependências transitivas e suas respectivas licenças.
- Preservar avisos exigidos.
- Avaliar manutenção, segurança e compatibilidade.
- Em uso empresarial, seguir o processo de aprovação de dependências da organização.

Esse checklist é um aprofundamento técnico. O professor demonstra a consulta ao README e à licença MIT, não uma auditoria completa de dependências.

## 8. Workbook versus Worksheet

O professor explica a diferença por analogia com uma planilha aberta em um editor.

### Workbook

É o documento completo que pode conter várias abas.

### Worksheet

É uma planilha individual dentro do workbook, composta por linhas, colunas e células.

```text
report.xlsx (Workbook)
    |
    +-- Janeiro de 2024 (Worksheet)
    |   +-- A1, B1, C1...
    |   +-- A2, B2, C2...
    |
    +-- Fevereiro de 2024 (outra Worksheet)
```

O exemplo com duas abas é apenas ilustrativo. O relatório do curso será gerado inicialmente com uma única aba nomeada de acordo com o período solicitado.

Não confundir uma worksheet com uma página de impressão. Uma aba pode ocupar várias páginas quando impressa.

Um XLWorkbook recém-criado não contém automaticamente a primeira worksheet necessária ao relatório. A aplicação precisa adicioná-la explicitamente.

## 9. Criando XLWorkbook e configurando Author

O professor começa a implementação criando uma instância de XLWorkbook.

```csharp
var workbook = new XLWorkbook();
```

Essa linha cria a representação do documento em memória.

Ela ainda não produz um arquivo .xlsx no disco nem devolve bytes ao controller.

### Autor

O professor demonstra a configuração de Author.

```csharp
workbook.Author = "CashFlow";
```

CashFlow é um valor ilustrativo. O instrutor sugere utilizar o nome do desenvolvedor ou da empresa.

**Precisão técnica:** a propriedade Author de XLWorkbook não deve ser confundida automaticamente com os metadados de autoria do documento. No ClosedXML, a autoria utilizada para comentários e as propriedades do documento podem ser representadas separadamente. Para configurar o autor nos metadados do arquivo, pode-se utilizar workbook.Properties.Author na API correspondente da biblioteca. A gravação utiliza workbook.Author e não inspeciona os metadados do arquivo salvo.

### Gerenciamento de recursos — complemento

XLWorkbook implementa IDisposable. Quando o documento estiver completamente construído e salvo, uma forma de garantir a liberação de recursos é:

```csharp
using var workbook = new XLWorkbook();
```

O professor não demonstra explicitamente using var nesta etapa. O exemplo complementar é uma prática apropriada para o fluxo completo, não uma linha que se atribui à gravação.

## 10. Estilos padrão e fontes

O instrutor define duas propriedades de estilo do workbook.

### Tamanho da fonte

```csharp
workbook.Style.Font.FontSize = 12;
```

O valor 12 representa o tamanho de fonte em pontos.

### Família tipográfica

```csharp
workbook.Style.Font.FontName = "Times New Roman";
```

O professor escolhe Times New Roman por preferência visual e indica que o aluno pode escolher outro nome ou tamanho.

Essas configurações são opcionais.

### Qual é o efeito?

As configurações estabelecem o estilo padrão para as novas worksheets criadas a partir daquele workbook, conforme a API da biblioteca.

A analogia apresentada pelo professor é selecionar todas as células no editor de planilhas e configurar a fonte padrão.

Não significa que as células jamais poderão usar outra fonte ou tamanho.

### Estilo padrão versus estilo específico

```text
Workbook: Times New Roman, 12
             |
             v
Worksheet criada com o padrão
             |
             +-- Células sem estilo específico: padrão
             |
             +-- Células personalizadas: estilo escolhido
```

O professor antecipa que será possível sobrescrever esses padrões ao preencher células. Ainda não escreve o código de configuração de uma célula individual.

A disponibilidade visual de uma fonte também depende do ambiente em que o arquivo será aberto e, quando relevante, do ambiente de renderização.

## 11. Adicionando a primeira aba

O professor explica que, até então, configurou o workbook, mas ainda não criou uma worksheet.

A criação ocorre por meio da coleção Worksheets.

```csharp
workbook.Worksheets.Add("Relatório");
```

Esse é um exemplo de nome genérico utilizado para explicar o método.

O nome escolhido no código final será derivado do mês e do ano.

```csharp
var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));
```

A chamada Add cria a aba e devolve a referência que representa a worksheet adicionada.

A variável worksheet será utilizada nas próximas aulas para preencher cabeçalhos, valores e estilos específicos.

### Adicionar várias abas

Como complemento, é possível criar outras worksheets:

```csharp
var summary = workbook.Worksheets.Add("Resumo");
var details = workbook.Worksheets.Add("Detalhes");
```

O curso não adiciona essas duas abas. O professor apenas explica que o workbook pode possuir mais de uma.

### Cuidados com nomes — complemento

O Excel possui restrições para nomes de worksheets: nomes não vazios, com até 31 caracteres, sem determinados caracteres especiais (como barra, contrabarra, dois-pontos, asterisco, interrogação e colchetes) e sem duplicação dentro do workbook.

A saída cultural de uma formatação pode precisar ser validada antes de ser usada como nome de aba em um sistema com muitas culturas e períodos possíveis.

A aula não implementa essa sanitização.

## 12. Formatação de datas com ToString("Y")

O parâmetro month recebido pelo caso de uso é DateOnly, conforme a aula 119.

Para criar o nome da worksheet, o professor utiliza:

```csharp
month.ToString("Y")
```

Y é um especificador padrão de formatação de ano e mês do .NET.

Ele produz uma representação textual do período de acordo com o padrão YearMonthPattern da cultura utilizada.

### Exemplos ilustrativos

```csharp
using System.Globalization;

var month = new DateOnly(2024, 6, 15);

var namePt = month.ToString(
    "Y", CultureInfo.GetCultureInfo("pt-BR"));

var nameEn = month.ToString(
    "Y", CultureInfo.GetCultureInfo("en-US"));
```

Possíveis resultados para os padrões usuais dessas culturas:

| Cultura | Texto |
|---|---|
| pt-BR | junho de 2024 |
| en-US | June 2024 |

Os exemplos acima são explicativos. A aula não executa esses dois trechos lado a lado nem confirma literalmente a saída de um arquivo gerado.

### IntelliSense do Visual Studio

O professor observa que o editor não exibe as sugestões de formatos para DateOnly naquele momento.

Para visualizar as opções, cria temporariamente uma expressão com DateTime.Now.ToString("").

Essa expressão serve somente para explorar o IntelliSense. Ela não representa a data utilizada no nome final da aba.

O nome deve corresponder ao mês solicitado pelo consumidor, não à data atual do servidor.

### Maiúsculas e minúsculas

Alguns especificadores de data e hora têm significados diferentes conforme a capitalização.

| Especificador padrão | Significado para DateTime |
|---|---|
| d | Data curta |
| D | Data longa |
| f | Data longa com hora curta |
| F | Data longa com hora longa |
| t | Hora curta |
| T | Hora longa |
| Y ou y | Ano e mês |

A tabela é um complemento verificado na documentação do .NET. A gravação mostra a exploração dessas opções, mas a descrição oral de alguns exemplos não é uma definição rigorosa de cada especificador.

**Y e y são equivalentes como especificadores padrão de um caractere.** Nem todos os formatos de DateTime são aplicáveis a DateOnly; formatos que exigem horário podem lançar FormatException.

O especificador Y é suportado por DateOnly.

Referência: https://learn.microsoft.com/pt-br/dotnet/standard/base-types/standard-date-and-time-format-strings

### Formato padrão versus personalizado

Y, isolado, é um especificador padrão que utiliza a cultura.

Uma alternativa personalizada como yyyy-MM produziria uma representação numérica do período. Ela não é a escolha da aula.

## 13. Cultura, localização e Culture Middleware

O professor lembra que o CashFlow já possui um middleware de cultura, anteriormente desenvolvido para traduzir mensagens de erro.

Ele deseja reaproveitar a cultura da requisição para formatar o nome da aba.

### CurrentCulture e CurrentUICulture

Como complemento técnico, é importante distinguir dois conceitos:

| Propriedade | Finalidade |
|---|---|
| CultureInfo.CurrentCulture | Formatação e interpretação cultural de valores, como datas e números |
| CultureInfo.CurrentUICulture | Seleção de recursos localizados, como mensagens traduzidas |

Quando ToString("Y") não recebe um provedor explícito, utiliza a cultura de formatação atual.

Portanto, o comportamento pretendido depende de o middleware configurar a CurrentCulture de maneira adequada ao contexto da requisição, e não apenas CurrentUICulture.

A transcrição afirma que o middleware existente permitirá formatar as datas conforme o idioma recebido, mas não mostra novamente sua implementação. Também não executa um teste de localização do arquivo nesta gravação.

### Exemplo conceitual

```text
Requisição com cultura selecionada
           |
           v
Culture Middleware
           |
           v
CurrentCulture correspondente
           |
           v
month.ToString("Y")
           |
           v
Nome localizado da worksheet
```

Não se deve generalizar formatos como se houvesse uma única convenção de data para todo o idioma inglês. Diferentes culturas, como en-US e en-GB, podem usar convenções distintas.

### Por que usar a cultura do período?

O período continua sendo representado por DateOnly. A tradução afeta sua apresentação textual, não altera automaticamente o ano ou o mês que será usado na consulta de despesas.

## 14. Código consolidado da etapa atual

O trecho central abaixo representa a configuração do documento realizada na aula:

```csharp
using ClosedXML.Excel;

// Dentro do caso de uso de relatório:
var workbook = new XLWorkbook();

workbook.Author = "CashFlow";
workbook.Style.Font.FontSize = 12;
workbook.Style.Font.FontName = "Times New Roman";

var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));
```

O nome CashFlow é ilustrativo e os demais elementos acompanham a configuração descrita.

### O que já existe nesse ponto?

- Uma instância de XLWorkbook.
- A configuração de Author.
- Estilo padrão Times New Roman, tamanho 12.
- Uma worksheet nomeada pelo mês e ano.
- Uma referência worksheet para manipular a aba.

### O que ainda não existe?

- Cabeçalhos preenchidos.
- Dados consultados do banco.
- Formatação de valores individuais.
- Serialização do workbook em bytes.
- Arquivo final pronto para download.

Não se deve apresentar um método Execute completo e funcional com uma implementação inventada de geração de bytes. Esta aula termina na configuração inicial.

## 15. Cuidados técnicos e etapas futuras

### 15.1 Workbook não é byte[]

XLWorkbook representa o modelo do documento manipulado pelo ClosedXML. Para produzir os bytes de um XLSX, será necessário salvar o documento em um destino adequado.

### 15.2 Geração em memória — antecipação conceitual

Em um fluxo futuro, um MemoryStream pode receber o conteúdo gerado e disponibilizá-lo como byte[].

Exemplo complementar, não implementado na gravação:

```csharp
using var stream = new MemoryStream();

workbook.SaveAs(stream);

byte[] bytes = stream.ToArray();
```

Esse código pressupõe um workbook validamente configurado, com ao menos uma worksheet. Ele não contém a consulta de despesas nem o preenchimento do relatório.

### 15.3 Separar ausência de dados da existência de bytes

Como definido na aula 118, a API pretende devolver 204 quando não houver despesas.

Um workbook vazio de registros ainda pode produzir um XLSX não vazio em bytes. Portanto, a regra de ausência deve ser decidida com base nos dados do relatório, e não simplesmente na existência de um workbook serializado.

### 15.4 Fórmulas e recursos de Excel

O professor menciona que a documentação do ClosedXML demonstra fórmulas. A gravação atual não cria fórmulas, totais, tabelas ou cabeçalhos.

### 15.5 Versão da biblioteca

A versão não é informada. APIs e dependências podem variar entre releases; é importante consultar a documentação compatível com o pacote instalado.

### 15.6 Próxima aula

O professor encerra anunciando a inserção do header da planilha.

O termo header nesse contexto significa os cabeçalhos das colunas da planilha, não o cabeçalho HTTP utilizado para receber month na aula 119.

## 16. Exercícios e revisão

### Exercício 1 — instalar o pacote

Qual biblioteca é escolhida?

**Resposta:** ClosedXML, instalada no projeto Application por meio do NuGet.

### Exercício 2 — criar um workbook

```csharp
var workbook = new XLWorkbook();
```

A linha cria a representação do documento, não um arquivo final já salvo.

### Exercício 3 — configurar fonte

```csharp
workbook.Style.Font.FontSize = 12;
workbook.Style.Font.FontName = "Times New Roman";
```

### Exercício 4 — criar a worksheet

```csharp
var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));
```

### Exercício 5 — identificar a cultura

Qual propriedade influencia a formatação padrão de ToString("Y")?

**Resposta:** CurrentCulture, salvo quando um provedor cultural explícito é informado.

### Exercício 6 — licença

A licença MIT impede utilizar ClosedXML em um projeto comercial?

**Resposta:** não. Ela permite uso comercial, respeitadas suas condições, incluindo a preservação dos avisos exigidos.

### Exercício 7 — diferença de objetos

Qual é a diferença entre workbook e worksheet?

**Resposta:** workbook é o documento completo; worksheet é uma aba individual.

### Perguntas de revisão

1. O que foi preparado na aula anterior?
2. Qual pasta agrupa os relatórios na Application?
3. Por que Excel e PDF são organizados separadamente?
4. Qual funcionalidade do Solution Explorer localiza o documento aberto?
5. Por que registrar o caso de uso na DI?
6. Qual parâmetro é passado a Execute?
7. Qual resultado binário é pretendido pelo caso de uso?
8. Qual biblioteca foi escolhida?
9. Em qual projeto o pacote é instalado?
10. Qual licença o professor destaca?
11. Por que código aberto não implica proibição de uso comercial?
12. Qual classe representa o workbook?
13. Criar XLWorkbook adiciona automaticamente a primeira worksheet?
14. Quais fontes e tamanhos são configurados?
15. O estilo padrão pode ser sobrescrito?
16. Qual método adiciona uma aba?
17. Qual formato de data é usado no nome da aba?
18. O que diferencia DateTime.Now da variável month neste exemplo?
19. Qual a diferença entre CurrentCulture e CurrentUICulture?
20. A aula comprova a localização do arquivo em duas culturas?
21. A gravação gera um array de bytes válido de Excel?
22. O que será implementado na próxima aula?

## 17. Resumo e mapa mental

| Aspecto | Resultado da aula |
|---|---|
| Organização | Expenses/Reports/Excel |
| Caso de uso | Interface e implementação preparadas |
| DI | Registro realizado |
| Controller | Chama Execute passando month |
| Biblioteca | ClosedXML |
| Licença | MIT |
| Projeto | Application |
| Documento | XLWorkbook |
| Autor | Configurável por Author |
| Fonte padrão | Times New Roman, 12 |
| Aba | Worksheets.Add(month.ToString("Y")) |
| Localização | Formatação dependente da cultura atual |
| Cabeçalhos | Próxima aula |
| Consulta e geração dos bytes | Ainda não demonstradas |

```text
                   AULA 120 — CLOSEDXML
                            |
              ReportController.GetExcel
                            |
                     Execute(month)
                            |
                     ClosedXML.Excel
                            |
                       XLWorkbook
                            |
            +---------------+---------------+
            |               |               |
          Author          Style          Worksheets
                            |               |
                        FontName         Add(nome)
                        FontSize            |
                            |        month.ToString("Y")
                            |               |
                   Times New Roman     mês e ano
                          12                |
                                           v
                                       Worksheet
                                           |
                                    Próxima aula:
                                      cabeçalhos
```

### Referências complementares

- [ClosedXML — repositório oficial](https://github.com/ClosedXML/ClosedXML)
- [ClosedXML — documentação de workbook](https://docs.closedxml.io/en/latest/api/workbook.html)
- [ClosedXML — organização de worksheets](https://docs.closedxml.io/en/latest/features/worksheets.html)
- [Microsoft Learn — formatos padrão de data e hora](https://learn.microsoft.com/pt-br/dotnet/standard/base-types/standard-date-and-time-format-strings)
- [Open Source Initiative — definição de código aberto](https://opensource.org/osd)

**Conclusão:** a aula 120 introduz ClosedXML no CashFlow e prepara a estrutura inicial do relatório de despesas. O caso de uso é conectado ao controller, a biblioteca é instalada na Application e o workbook recebe configurações de autoria e fonte. A primeira worksheet é criada com um nome baseado no mês e ano solicitados, utilizando a formatação cultural do .NET. A construção dos cabeçalhos, o preenchimento de dados e a geração final do arquivo ainda não estão concluídos.
