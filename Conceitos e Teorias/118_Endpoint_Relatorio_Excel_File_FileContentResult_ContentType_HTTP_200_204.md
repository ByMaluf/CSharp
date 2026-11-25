# Aula 118 — Relatório Excel (parte 1): endpoint GET, File(), Content-Type e download de arquivos

> **Contexto:** após finalizar o CRUD de despesas na aula 117, o CashFlow inicia um módulo de geração de relatórios. O objetivo final é exportar as despesas de um determinado mês e ano em um arquivo Excel com cabeçalhos e informações organizadas.
>
> **Escopo desta aula:** preparar o ReportController e o endpoint GET de Excel, aprender a retornar bytes como arquivo e documentar os status 200 e 204. O array de bytes utilizado é provisório. A geração real do Excel, a consulta filtrada e a entrada de data ficam para as próximas aulas.
>
> **Fidelidade:** os trechos de código são reconstruídos a partir da transcrição. O prefixo de rota é interpretado como /api/report, conforme a descrição do Swagger. As precisões técnicas sobre MIME, Content-Disposition e OpenAPI são complementos, não alterações realizadas pelo professor.

## Índice

1. Objetivo do módulo de relatórios
2. Novo ReportController
3. Rota GET de Excel
4. Retorno de arquivos com File()
5. Representação binária com byte[]
6. Content-Type e MIME types
7. MediaTypeNames e application/octet-stream
8. Nome do arquivo e Content-Disposition
9. FileContentResult e tipos de retorno
10. Respostas HTTP 200 e 204
11. Array vazio como sinal de ausência
12. Documentação no Swagger e OpenAPI
13. Código consolidado da etapa atual
14. Arquitetura prevista e pendências
15. Precisões técnicas e boas práticas
16. Exercícios e perguntas de revisão
17. Resumo e mapa mental

---

## 1. Objetivo do módulo de relatórios

O CRUD permite cadastrar, consultar, atualizar e excluir despesas. Agora o sistema precisa oferecer uma representação dos dados voltada à exportação e análise.

O professor anuncia uma sequência de aulas destinada a gerar arquivos Excel e, mais adiante, PDF.

O relatório Excel deverá conter:

- Cabeçalhos organizados.
- Informações das despesas.
- Somente registros pertencentes ao mês e ano solicitados.

O usuário não receberá indiscriminadamente todas as despesas do banco.

```text
Mês e ano solicitados
          |
          v
Consulta filtrada de despesas
          |
          v
Geração de relatório Excel
          |
          v
Array de bytes
          |
          v
Resposta HTTP com arquivo
```

**Atenção:** esse é o fluxo pretendido do módulo. Nesta aula, apenas o último estágio — estrutura do endpoint e retorno de arquivo — é preparado. Nenhum pacote de geração de Excel é instalado ou utilizado na gravação.

## 2. Novo ReportController

O instrutor cria um controller separado para relatórios, chamado ReportController.

A separação permite agrupar operações de exportação sem misturá-las com os endpoints tradicionais de despesas.

Exemplo de estrutura coerente com a rota exibida:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    // Endpoints de relatórios.
}
```

Os atributos da classe acima são uma reconstrução ilustrativa; a transcrição confirma o nome ReportController e a rota de relatório exibida no Swagger.

### O que será incluído nesse controller?

| Operação | Situação |
|---|---|
| Relatório Excel | Endpoint preparado nesta aula |
| Relatório PDF | Anunciado para uma etapa futura |

O nome do método apresentado é GetExcel.

O professor declara um método assíncrono, pois a operação completa realizará acesso ao banco de dados. Na etapa atual, ainda não há consulta nem await efetivo no corpo.

## 3. Rota GET de Excel

Inicialmente, o professor utiliza apenas:

```csharp
[HttpGet]
```

Ao executar a API, observa no Swagger a rota base de relatório.

Como futuramente haverá também um PDF, especializa a rota:

```csharp
[HttpGet("excel")]
```

A rota resultante é representada como:

```http
GET /api/report/excel
```

A capitalização exata exibida pelo Swagger não é essencial para o conceito; o segmento adicional identifica o formato Excel.

### Por que GET?

A solicitação recupera uma representação dos dados existentes. A geração de um relatório para download não precisa, por si só, criar ou alterar despesas persistidas.

O propósito da operação é leitura.

O futuro filtro de mês e ano ainda não é definido na assinatura. O professor encerra a aula anunciando que discutirá a entrada da data no próximo vídeo.

## 4. Retorno de arquivos com File()

Nos endpoints anteriores, foram utilizados resultados como:

| Método do controller | Finalidade no projeto |
|---|---|
| Ok(response) | Resposta de sucesso com dados |
| Created(...) | Confirmação de cadastro |
| NoContent() | Sucesso sem corpo |
| File(...) | Resposta contendo um arquivo |

Para retornar o conteúdo binário do relatório, o professor utiliza o método File disponível em ControllerBase.

```csharp
return File(
    file,
    MediaTypeNames.Application.Octet,
    "report.xlsx");
```

### Os três argumentos

| Argumento | Tipo | Função |
|---|---|---|
| file | byte[] | Conteúdo binário do arquivo |
| contentType | string | Tipo de mídia da resposta |
| fileDownloadName | string | Nome sugerido para download |

O ASP.NET Core oferece diversas sobrecargas de File. A estudada na aula recebe um array de bytes, um tipo de mídia e um nome de arquivo.

Não é necessário converter a planilha em um DTO JSON para devolver esse conteúdo.

## 5. Representação binária com byte[]

Um arquivo digital é representado por uma sequência de bytes.

Em C#, o tipo byte[] representa um array de valores byte (inteiros sem sinal de 8 bits).

O professor explica que o futuro caso de uso fornecerá um array de bytes contendo o arquivo Excel.

### Simulação utilizada

```csharp
var file = new byte[1];
```

Essa variável é criada temporariamente para desenvolver a action sem depender de um gerador de Excel ainda inexistente.

### O que new byte[1] realmente cria?

Um array com comprimento 1, cujo único byte é inicializado com zero.

Isso não é um documento Excel válido. A extensão .xlsx no nome do download não altera o conteúdo do array.

Um arquivo Excel real precisará ser produzido por uma implementação apropriada que gere bytes compatíveis com o formato XLSX.

### Futuro caso de uso

Conceitualmente, o professor pretende substituir o array provisório pelo retorno do use case.

```csharp
// Exemplo conceitual, ainda não implementado:
// var file = await useCase.Execute(...);
```

A assinatura definitiva, o nome do use case e a biblioteca de planilhas ainda não são apresentados.

## 6. Content-Type e MIME types

O segundo argumento de File informa o tipo de mídia do conteúdo.

Content-Type é um cabeçalho HTTP que descreve o formato da representação enviada.

Exemplos:

| Conteúdo | Tipo de mídia |
|---|---|
| JSON | application/json |
| PDF | application/pdf |
| ZIP | application/zip |
| PNG | image/png |
| Texto simples | text/plain |
| Binário genérico | application/octet-stream |

Os tipos de mídia possuem uma estrutura padronizada, normalmente composta por tipo e subtipo.

O professor explica que o navegador pode utilizar essa informação para decidir como tratar a resposta.

### Precisão técnica

O comportamento do navegador não é determinado exclusivamente por Content-Type. Outros fatores incluem Content-Disposition, as políticas de segurança do navegador, o conteúdo e o nome do arquivo.

O cabeçalho descreve os bytes. Ele não converte automaticamente o conteúdo em determinado formato.

## 7. MediaTypeNames e application/octet-stream

O professor apresenta a classe MediaTypeNames, disponível no namespace System.Net.Mime.

```csharp
using System.Net.Mime;
```

O .NET disponibiliza constantes para tipos de mídia conhecidos.

Exemplos:

```csharp
MediaTypeNames.Application.Pdf
MediaTypeNames.Application.Zip
MediaTypeNames.Application.Octet
```

O professor navega pela definição da classe e observa que não há, na API disponível em seu ambiente, uma constante específica para Excel.

Por isso, escolhe:

```csharp
MediaTypeNames.Application.Octet
```

Essa constante representa:

```text
application/octet-stream
```

### O que significa octet-stream?

É um tipo de mídia genérico para dados binários.

Pode ser utilizado quando se deseja enviar um arquivo sem declarar um formato mais específico ou quando seu formato não está identificado.

A aula escolhe essa representação para o download do Excel.

### Complemento técnico: existe um MIME específico para XLSX

A ausência de uma constante conveniente na classe MediaTypeNames não implica ausência de um tipo de mídia padronizado.

Para arquivos XLSX, existe:

```text
application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
```

É possível declarar esse valor como string ou constante do próprio projeto:

```csharp
const string ExcelContentType =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

return File(file, ExcelContentType, "report.xlsx");
```

Esse exemplo é uma alternativa técnica, não a implementação realizada na gravação.

### XLS versus XLSX

| Formato | Tipo de mídia |
|---|---|
| .xls (formato tradicional) | application/vnd.ms-excel |
| .xlsx (Office Open XML) | application/vnd.openxmlformats-officedocument.spreadsheetml.sheet |

O professor escolhe a extensão .xlsx. A existência de um tipo de mídia específico permite documentar o formato com maior precisão.

O uso de application/octet-stream continua sendo uma opção genérica válida para download, mas não identifica especificamente uma planilha XLSX.

## 8. Nome do arquivo e Content-Disposition

O terceiro parâmetro informado ao método File é:

```csharp
"report.xlsx"
```

O instrutor demonstra que esse argumento não é obrigatório para todas as sobrecargas do método, mas prefere fornecê-lo.

Assim, o consumidor recebe uma sugestão de nome para salvar o relatório.

### Cabeçalho conceitual

O ASP.NET Core utiliza o nome informado para configurar a disposição de download, normalmente por meio de Content-Disposition.

Exemplo ilustrativo:

```http
Content-Type: application/octet-stream
Content-Disposition: attachment; filename=report.xlsx
```

A representação literal do cabeçalho pode incluir outras formas de codificação de nome e depende do framework.

### Diferença entre os conceitos

- Content-Type descreve o tipo de mídia.
- Content-Disposition indica como apresentar o conteúdo e pode sugerir um nome de arquivo.
- O nome com extensão .xlsx ajuda a identificar o arquivo no sistema operacional.
- Os bytes precisam constituir um Excel válido independentemente dos cabeçalhos e do nome.

O professor autoriza personalizar o nome. Um nome que inclua o mês e o ano poderia ser considerado futuramente, mas não é implementado na aula.

## 9. FileContentResult e tipos de retorno

Ao inspecionar File no Visual Studio, o professor observa que a sobrecarga utilizada retorna FileContentResult.

Esse tipo de resultado MVC representa conteúdo binário que será escrito na resposta HTTP.

### Exemplo

```csharp
FileContentResult result = File(
    file,
    MediaTypeNames.Application.Octet,
    "report.xlsx");
```

A action pode declarar um tipo de retorno mais abrangente, como IActionResult, pois também pode devolver NoContentResult.

```csharp
public async Task<IActionResult> GetExcel()
```

Essa assinatura é uma reconstrução coerente com os resultados apresentados.

### Arquivo não é resposta JSON convencional

FileContentResult é um tipo de resultado do framework. O objeto em si não é o conteúdo que o consumidor precisa desserializar como JSON.

O executor do resultado escreve os bytes na resposta e configura os cabeçalhos correspondentes.

A resposta de sucesso normal é HTTP 200 OK.

## 10. Respostas HTTP 200 e 204

O professor estabelece duas respostas de negócio.

| Status | Condição prevista | Corpo |
|---|---|---|
| 200 OK | Existe relatório para devolver | Bytes do arquivo |
| 204 No Content | Não existem despesas no período | Sem corpo |

A escolha é não gerar um Excel vazio quando não houver despesas no mês solicitado.

A action segue o mesmo princípio de ausência de resultados adotado no GET de listagem da aula 109.

### Código

```csharp
if (file.Length > 0)
    return File(
        file,
        MediaTypeNames.Application.Octet,
        "report.xlsx");

return NoContent();
```

A verificação utiliza o tamanho do array.

Não há tratamento novo de exceções nem documentação de erros adicionais nesta gravação.

## 11. Array vazio como sinal de ausência

Na arquitetura pretendida, o futuro use case retornará um array vazio quando não houver despesas a exportar.

A action poderá então interpretar:

```csharp
file.Length > 0
```

como indicação de que existe conteúdo de arquivo para enviar.

### Exemplo de array vazio — complemento

```csharp
byte[] file = Array.Empty<byte>();
```

Length será zero, e o controller seguirá o caminho NoContent.

### Limite desse contrato

Um array não vazio não garante que seu conteúdo seja um arquivo Excel válido.

Além disso, um documento XLSX que contém apenas cabeçalhos ou nenhuma linha de dados ainda pode possuir muitos bytes.

Por isso, a verificação funciona como sinal de ausência somente se o futuro use case respeitar o contrato de devolver um array vazio quando não houver despesas.

### Situação atual

O array provisório new byte[1] sempre possui Length igual a 1. Portanto, ele segue o caminho de File. A gravação não demonstra a geração de uma planilha válida nem uma consulta de período sem registros.

## 12. Documentação no Swagger e OpenAPI

O professor inicialmente documenta os dois status:

```csharp
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

Ele também experimenta informar typeof(FileContentResult) no atributo de sucesso, seguindo o padrão usado anteriormente com DTOs.

```csharp
[ProducesResponseType(
    typeof(FileContentResult),
    StatusCodes.Status200OK)]
```

Ao observar o Swagger, percebe que a documentação apresenta a resposta como string. Considera a apresentação pouco clara e prefere manter apenas os códigos de status.

O nome da rota, excel, ajuda o desenvolvedor consumidor a identificar o formato pretendido.

### Precisão técnica: string pode representar dados binários em OpenAPI

Na OpenAPI 3.0, uma resposta binária pode ser descrita por um schema com type: string e format: binary, acompanhado do tipo de mídia apropriado.

Exemplo conceitual de especificação:

```yaml
responses:
  '200':
    description: Relatório Excel
    content:
      application/vnd.openxmlformats-officedocument.spreadsheetml.sheet:
        schema:
          type: string
          format: binary
  '204':
    description: Nenhuma despesa encontrada
```

Nesse contexto, string não significa necessariamente texto comum ou uma string JSON. O formato binary e o media type qualificam a representação.

Há diferenças entre versões da OpenAPI e ferramentas de geração de documentação.

### É possível documentar arquivos com precisão?

Sim. É possível fornecer metadados de mídia e configurar o gerador de OpenAPI para descrever a resposta binária, por exemplo, com filtros de operação quando necessário.

A solução exata depende da versão do ASP.NET Core e do gerador utilizado.

O professor não configura esses recursos nesta aula. Ele opta pela documentação mais simples, informando 200 e 204, e essa decisão é preservada no código consolidado.

## 13. Código consolidado da etapa atual

O seguinte trecho representa a estrutura descrita ao final da aula.

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
    public async Task<IActionResult> GetExcel()
    {
        // Simulação: ainda não é um arquivo Excel válido.
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

Os atributos da classe e a assinatura completa foram reconstruídos. O comportamento do endpoint, os argumentos de File, o nome report.xlsx e os status correspondem à descrição da gravação.

### Aviso sobre async sem await

Como o caso de uso ainda não é chamado, esse método provisório pode gerar o aviso CS1998: método async sem operadores await.

O uso de async nessa etapa antecipa o futuro acesso assíncrono ao banco. Ele não torna o array provisório uma operação assíncrona.

O professor não finaliza a operação nesta gravação. O trecho deve ser interpretado como uma estrutura em desenvolvimento.

## 14. Arquitetura prevista e pendências

O professor informa que o objetivo completo incluirá filtrar despesas por mês e ano e gerar os bytes do relatório.

```text
API
    ReportController.GetExcel
            |
            v
Application
    Futuro use case de relatório
            |
            v
Repository
    Consulta filtrada por mês e ano
            |
            v
Geração do arquivo Excel
            |
            v
byte[]
            |
            v
API: File() ou NoContent()
```

### O que foi apresentado nesta aula?

- Criação de ReportController.
- Método GetExcel.
- Rota GET especializada em Excel.
- Retorno de arquivo através de File.
- Array de bytes provisório.
- Tipo de mídia application/octet-stream.
- Nome report.xlsx.
- Status 200 e 204.
- Verificação da documentação no Swagger.

### O que fica para depois?

| Funcionalidade | Situação |
|---|---|
| Receber mês e ano | Próxima aula |
| Consulta filtrada no banco | Não implementada |
| Caso de uso de relatório | Não implementado |
| Geração efetiva de XLSX | Não implementada |
| Cabeçalhos e formatação | Não implementados |
| Download de uma planilha válida | Não demonstrado |
| Relatório PDF | Anunciado para etapa futura |

O professor executa a API e verifica a rota e a documentação no Swagger. Não há teste confirmado de download e abertura de um Excel válido.

## 15. Precisões técnicas e boas práticas

Os pontos desta seção são complementares.

### 15.1 Não confundir nome com formato

Salvar um array arbitrário com extensão .xlsx não produz uma planilha válida. É necessário gerar o conteúdo do formato.

### 15.2 Não confundir tipo de mídia com instrução absoluta ao navegador

Application/octet-stream identifica bytes genéricos. A disposição de download depende também de Content-Disposition e do cliente HTTP.

### 15.3 MediaTypeNames não é uma lista exaustiva dos formatos existentes

A inexistência de uma constante específica não impede usar um MIME válido como string.

### 15.4 Arquivos grandes e consumo de memória

A sobrecarga com byte[] pressupõe que o conteúdo esteja disponível em memória.

Em relatórios muito grandes, pode ser necessário avaliar geração por streaming, arquivos temporários ou outras formas de resposta. FileStreamResult é uma possibilidade do ASP.NET Core, mas a adequação depende do gerador de planilhas.

Não existe análise de volume ou desempenho na aula.

### 15.5 Consulta somente de leitura

A futura consulta de relatórios tende a ser somente leitura. AsNoTracking poderá ser apropriado conforme a forma de consulta e a arquitetura definida.

A gravação atual ainda não implementa a consulta e não deve ser considerada uma demonstração dessa otimização.

### 15.6 Filtragem de período

Para filtrar corretamente um mês, será necessário representar o ano e o mês solicitados e considerar os limites temporais apropriados.

A estratégia exata de parâmetros, validação e consulta será abordada adiante. Não se deve antecipar como implementada uma rota com query string, DateTime ou parâmetros separados.

### 15.7 Contrato de ausência

HTTP 204 é a escolha adotada para o cenário sem despesas. Outras APIs poderiam gerar um relatório vazio, mas esse não é o contrato escolhido no CashFlow.

### 15.8 Documentação não substitui implementação

ProducesResponseType informa resultados possíveis. O retorno efetivo depende da lógica da action e do caso de uso.

### 15.9 Separação de responsabilidades

O controller deve coordenar a resposta HTTP. O futuro use case deve coordenar a consulta e a geração do relatório, preservando a organização estabelecida no módulo.

## 16. Exercícios e perguntas de revisão

### Exercício 1 — rota

Qual atributo especializa o GET para Excel?

```csharp
[HttpGet("excel")]
```

### Exercício 2 — resposta com arquivo

```csharp
return File(
    file,
    MediaTypeNames.Application.Octet,
    "report.xlsx");
```

O primeiro argumento contém os bytes; o segundo descreve o tipo de mídia; o terceiro sugere o nome do download.

### Exercício 3 — resposta sem conteúdo

```csharp
if (file.Length == 0)
    return NoContent();
```

A condição é apropriada quando o use case utiliza um array vazio para comunicar ausência de dados.

### Exercício 4 — tipo de mídia específico

Qual tipo de mídia identifica XLSX?

Resposta: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.

### Exercício 5 — array provisório

new byte[1] gera um Excel válido?

Resposta: não. Cria um array com um byte inicializado em zero, usado apenas como simulação.

### Exercício 6 — documentação

Quais status o professor documenta?

```csharp
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
```

### Perguntas de revisão

1. Qual nova funcionalidade o módulo de relatórios pretende oferecer?
2. Por que as despesas serão filtradas por mês e ano?
3. Qual controller foi criado?
4. Qual é o nome da action?
5. Por que a rota contém o segmento excel?
6. Qual método do ASP.NET Core devolve um arquivo?
7. Qual tipo representa o conteúdo do relatório?
8. O que é Content-Type?
9. Qual namespace fornece MediaTypeNames?
10. O que representa MediaTypeNames.Application.Octet?
11. Qual é a diferença entre application/octet-stream e o MIME específico de XLSX?
12. Qual nome de arquivo o professor escolhe?
13. Qual cabeçalho sugere o nome do download?
14. Qual tipo MVC retorna a sobrecarga de File utilizada?
15. Qual status indica download bem-sucedido?
16. Qual status é escolhido quando não há despesas?
17. Por que Length maior que zero não prova que o arquivo é válido?
18. O que o professor observa ao documentar typeof(FileContentResult)?
19. Como OpenAPI 3.0 pode representar conteúdo binário?
20. Qual é a diferença entre documentar o endpoint e testar um arquivo válido?
21. A aula implementa o filtro por mês e ano?
22. O PDF já foi implementado?

## 17. Resumo e mapa mental

| Componente | Resultado |
|---|---|
| Controller | ReportController |
| Action | GetExcel |
| HTTP | GET |
| Rota | /api/report/excel (reconstrução) |
| Conteúdo | byte[] provisório |
| Retorno | FileContentResult |
| MIME adotado | application/octet-stream |
| Nome | report.xlsx |
| Sucesso | 200 OK |
| Ausência | 204 No Content |
| Swagger | Status documentados |
| Próxima etapa | Entrada de mês e ano e desenvolvimento do relatório |

```text
                 AULA 118 — RELATÓRIO EXCEL
                              |
                       ReportController
                              |
                       GET /report/excel
                              |
                          byte[] file
                         (provisório)
                              |
                         Length > 0?
                       /              \
                     SIM              NÃO
                      |                |
                   File()         NoContent()
                      |                |
               FileContentResult      204
                      |
               Content-Type:
              application/octet-stream
                      |
               Nome: report.xlsx
                      |
                     200

    Próximas aulas:
    receber mês e ano -> consultar despesas -> gerar XLSX real
```

**Conclusão:** a aula 118 prepara o endpoint de exportação do CashFlow e introduz o retorno de arquivos binários no ASP.NET Core. O professor utiliza File com um array de bytes, um tipo de mídia e o nome report.xlsx, documentando 200 e 204. O relatório Excel propriamente dito ainda não é gerado: o array de um byte representa apenas um marcador temporário. O material preserva essa etapa do curso e distingue as decisões do professor das possibilidades adicionais de MIME específico e documentação binária no OpenAPI.
