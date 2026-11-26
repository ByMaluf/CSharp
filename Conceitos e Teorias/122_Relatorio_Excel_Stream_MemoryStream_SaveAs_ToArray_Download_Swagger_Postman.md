# Aula 122 — Relatório Excel (parte 5): Stream, MemoryStream, SaveAs, ToArray e download pelo Swagger e Postman

> **Continuação da aula 121:** o relatório possui workbook, worksheet com nome do período, fonte Times New Roman de tamanho 12 e cinco cabeçalhos localizados e estilizados.
>
> **Objetivo:** serializar o workbook em memória, devolver os bytes ao controller e verificar se o arquivo gerado pode ser baixado e importado em uma planilha.
>
> **Resultado demonstrado:** o primeiro teste gera um array de 6.435 bytes; o Swagger oferece o download e o Google Planilhas importa o documento. Um segundo teste pelo Postman, com Accept-Language em português brasileiro, confirma a tradução dos cabeçalhos, a cor de fundo e o negrito.
>
> **Escopo:** o documento possui cabeçalhos, mas ainda não contém as despesas consultadas no banco. Não há teste do caminho HTTP 204, medição de desempenho ou configuração de produção nesta aula.
>
> **Fidelidade:** os trechos de código são reconstruídos a partir da narração. A transcrição alterna termos como MemoryString e String ao se referir a MemoryStream e Stream. A grafia técnica correta é adotada. O caminho físico completo e a porta da API não foram transcritos. Os exemplos complementares são distinguidos da implementação demonstrada.

## Índice

1. Recapitulação e objetivo
2. SaveAs e a opção de salvar em disco
3. Problemas de caminhos físicos fixos em APIs
4. O conceito de Stream
5. Classe abstrata, herança e polimorfismo
6. MemoryStream e armazenamento em memória
7. Serialização do workbook com SaveAs(Stream)
8. ToArray e o retorno de byte[]
9. Código consolidado do caso de uso
10. Integração com o controller
11. Teste no Swagger e depuração
12. Importação no Google Planilhas
13. Download pelo Postman
14. Accept-Language e localização do relatório
15. Gerenciamento de recursos e cuidados de desempenho
16. Exercícios e perguntas de revisão
17. Resumo e mapa mental

---

## 1. Recapitulação e objetivo

Nas aulas anteriores, o professor construiu a estrutura necessária para gerar relatórios de despesas no formato Excel.

| Aula | Etapa |
|---|---|
| 118 | Endpoint de download com File() e status 200/204 |
| 119 | Recebimento do período por FromHeader e DateOnly |
| 120 | Instalação do ClosedXML, criação do workbook e worksheet |
| 121 | Inserção e estilização dos cabeçalhos |
| 122 | Serialização em memória, download e validação inicial |

Até a aula 121, o documento estava representado por objetos da biblioteca ClosedXML.

O próximo passo é converter essa estrutura em bytes que possam representar um arquivo XLSX real.

```text
XLWorkbook
    |
    | SaveAs
    v
MemoryStream
    |
    | ToArray
    v
byte[]
    |
    | File
    v
Resposta HTTP
    |
    v
Download e abertura da planilha
```

O professor pretende validar o trabalho já realizado antes de avançar para o preenchimento dos dados das despesas.

## 2. SaveAs e a opção de salvar em disco

Logo após chamar InsertHeader, o professor utiliza o método SaveAs.

É importante observar que a operação é realizada sobre workbook, e não worksheet.

```csharp
workbook.SaveAs(/* destino */);
```

O workbook representa o arquivo inteiro, incluindo suas abas, valores, estilos e demais informações.

Ao tentar chamar SaveAs sem argumento, o professor encontra um erro. Ele inspeciona a definição do método e observa uma sobrecarga que recebe uma string indicando o caminho de destino.

### Exemplo ilustrativo da primeira abordagem

```csharp
var file = @"D:\MyReport\report.xlsx";

workbook.SaveAs(file);
```

A gravação menciona uma pasta MyReport e uma unidade D:, mas não informa literalmente o caminho completo. O exemplo acima é uma reconstrução ilustrativa.

Nessa forma, ClosedXML serializa o workbook e grava o documento no sistema de arquivos, no caminho indicado.

### Por que essa abordagem é abandonada?

O objetivo do endpoint não é manter um arquivo permanentemente em um diretório da máquina que executa a API.

A API precisa devolver o conteúdo diretamente como resposta HTTP.

## 3. Problemas de caminhos físicos fixos em APIs

O professor lembra que o CashFlow está sendo desenvolvido localmente, mas poderá ser publicado na nuvem.

Um caminho como D:\MyReport\report.xlsx está associado à configuração da máquina de desenvolvimento.

No ambiente de produção, podem existir diferenças de:

- Sistema operacional.
- Estrutura de diretórios.
- Unidades de armazenamento.
- Permissões de escrita.
- Disponibilidade e persistência do armazenamento local.

Além disso, utilizar o mesmo nome de arquivo físico para solicitações simultâneas pode exigir mecanismos para evitar conflitos entre requisições.

### Precisão técnica

O problema não é salvar arquivos em disco em qualquer API. Uma aplicação pode utilizar armazenamento físico, temporário ou remoto quando isso atende aos seus requisitos e quando o ambiente está preparado.

O problema da primeira abordagem apresentada é a dependência de um caminho fixo e específico da máquina, além de não atender à escolha de gerar o relatório diretamente em memória.

A solução adotada nesta aula é eliminar a gravação intermediária explícita em disco.

## 4. O conceito de Stream

O professor apresenta a classe Stream do C#/.NET, disponível no namespace System.IO.

Stream é uma abstração para trabalhar com sequências de bytes.

A origem ou destino dos dados pode variar, por exemplo, memória, arquivo físico ou conexão de rede.

```text
Aplicação
    |
    | leitura e escrita
    v
Stream
    |
    +-- memória
    +-- arquivo
    +-- rede
```

O instrutor descreve Stream como um intermediário entre a aplicação e o conteúdo do arquivo.

### Por que trabalhar com bytes?

Arquivos digitais são representados por sequências de bytes.

Um PDF, uma imagem ou um XLSX possui uma estrutura binária própria. Stream permite transportar ou acessar esses bytes sem precisar conhecer, por si só, o significado de cada formato.

ClosedXML é responsável por conhecer o formato do Excel. MemoryStream é responsável por disponibilizar um destino em memória para seus bytes.

### Complemento técnico: capacidades de um stream

A API Stream possui operações e propriedades relacionadas a leitura, escrita, posicionamento e tamanho, tais como:

| Membro | Finalidade |
|---|---|
| Read | Ler bytes |
| Write | Escrever bytes |
| CanRead | Indicar suporte à leitura |
| CanWrite | Indicar suporte à escrita |
| CanSeek | Indicar suporte ao posicionamento |
| Position | Posição atual, quando suportada |
| Length | Comprimento, quando suportado |
| Seek | Alterar posição, quando suportado |

Nem todo stream suporta todas as operações. Alguns são somente leitura, somente escrita ou não permitem alterar a posição.

O professor não implementa manualmente esses métodos nesta aula.

## 5. Classe abstrata, herança e polimorfismo

O instrutor inspeciona a definição de Stream no Visual Studio e observa que é uma classe abstrata.

Por isso, não é possível instanciá-la diretamente:

```csharp
// Não compila: Stream é uma classe abstrata.
var stream = new Stream();
```

É necessário utilizar uma implementação concreta apropriada.

```csharp
Stream stream = new MemoryStream();
```

### O papel da classe abstrata

A classe base fornece um contrato comum de operações sobre sequências de bytes.

Classes derivadas concretas implementam os membros abstratos necessários e podem utilizar ou sobrescrever outros membros fornecidos pela classe base.

A implementação pode operar sobre memória, disco ou outros meios.

### Hierarquia conceitual

```text
System.IO.Stream (abstract)
    |
    +-- MemoryStream
    +-- FileStream
    +-- NetworkStream
    +-- outras implementações
```

FileStream e NetworkStream são exemplos complementares. O tipo efetivamente utilizado no CashFlow é MemoryStream.

### Relação com polimorfismo

Quando um método recebe um argumento do tipo Stream, ele pode aceitar uma implementação concreta compatível, como MemoryStream.

É esse mecanismo que permite utilizar a sobrecarga de SaveAs que recebe Stream, sem que ClosedXML precise depender de uma pasta física específica.

## 6. MemoryStream e armazenamento em memória

O professor substitui a variável que representava um caminho por uma instância de MemoryStream.

```csharp
var file = new MemoryStream();
```

MemoryStream é uma classe concreta derivada de Stream que utiliza memória como armazenamento para a sequência de bytes.

Ela pode ser utilizada como destino temporário dos bytes de um documento.

### Estado inicial

Ao criar um MemoryStream vazio, ainda não existe um arquivo Excel dentro dele.

A instância fornece um espaço de armazenamento que poderá receber os dados serializados posteriormente.

### Diferença entre as abstrações

| Tipo | Representação |
|---|---|
| XLWorkbook | Modelo do documento Excel manipulado pelo ClosedXML |
| Stream | Abstração de acesso a uma sequência de bytes |
| MemoryStream | Implementação de stream baseada em memória |
| byte[] | Array contendo bytes |

MemoryStream não é uma string, não representa um caminho físico e não é um workbook.

### Por que é apropriado ao cenário?

O relatório será criado e devolvido ao consumidor sem necessidade de permanecer em um diretório local.

O uso de memória elimina a dependência explícita do caminho físico anteriormente demonstrado.

## 7. Serialização do workbook com SaveAs(Stream)

Com o MemoryStream disponível, o professor utiliza a outra sobrecarga de SaveAs.

```csharp
var file = new MemoryStream();

workbook.SaveAs(file);
```

Agora, em vez de receber uma string que identifica um caminho, SaveAs recebe uma instância compatível com Stream.

A biblioteca serializa o documento e escreve os bytes no stream.

### O que é serialização neste contexto?

É a transformação da representação interna do workbook em uma sequência de bytes compatível com o formato do arquivo.

O workbook possui informações como:

- Nome e estrutura das worksheets.
- Valores das células.
- Estilos e formatação.
- Configurações do documento.

SaveAs converte essas informações para a representação de um XLSX.

### Comparação das sobrecargas

```csharp
// Destino físico:
workbook.SaveAs(@"D:\MyReport\report.xlsx");

// Destino em memória:
var file = new MemoryStream();
workbook.SaveAs(file);
```

A primeira forma é abandonada pelo professor. A segunda é a implementação adotada nesta aula.

Ambas salvam o documento, mas utilizam destinos diferentes.

## 8. ToArray e o retorno de byte[]

O contrato do caso de uso prevê devolver um array de bytes.

Por isso, após salvar o workbook, o professor escreve:

```csharp
return file.ToArray();
```

ToArray é um método de MemoryStream que produz um array contendo o conteúdo armazenado no stream.

### Fluxo completo

```csharp
var file = new MemoryStream();

workbook.SaveAs(file);

return file.ToArray();
```

Cada posição do array representa um byte do documento. A ordem desses bytes deve ser preservada para que o arquivo continue válido.

### Precisões técnicas complementares

ToArray devolve uma cópia dos bytes correspondentes ao conteúdo do stream, independentemente da posição atual.

Portanto, para essa operação específica, não é necessário executar file.Position = 0 antes de ToArray.

A situação seria diferente se a aplicação fosse entregar o próprio stream a um consumidor que começasse a ler a partir da posição atual. Nesse desenho, poderia ser necessário reposicioná-lo.

ToArray não pertence ao contrato genérico de Stream; é disponibilizado por MemoryStream.

A implementação não converte o arquivo em uma string nem precisa codificá-lo em Base64.

### Relação com o placeholder da aula 118

Anteriormente, o controller utilizava new byte[1], que não constituía uma planilha válida.

Agora os bytes são produzidos pela biblioteca ClosedXML a partir de um workbook real.

## 9. Código consolidado do caso de uso

O trecho abaixo reconstrói a implementação de geração demonstrada, preservando a sequência das aulas 120 e 121.

```csharp
using ClosedXML.Excel;
using System.IO;

// Dentro de Execute(DateOnly month):
var workbook = new XLWorkbook();

workbook.Author = "CashFlow";
workbook.Style.Font.FontSize = 12;
workbook.Style.Font.FontName = "Times New Roman";

var worksheet = workbook.Worksheets.Add(
    month.ToString("Y"));

InsertHeader(worksheet);

var file = new MemoryStream();

workbook.SaveAs(file);

return file.ToArray();
```

O autor CashFlow é ilustrativo. O professor utiliza seu próprio nome. A assinatura literal completa do método e o nome exato da classe não são reproduzidos nesta gravação.

O método InsertHeader, documentado na aula 121, preenche A1:E1 com recursos localizados e aplica negrito, fundo #F5C2B6 e alinhamentos específicos.

### Responsabilidades

A geração do arquivo permanece no caso de uso, enquanto o controller fica responsável por traduzir o resultado para uma resposta HTTP.

O método SaveAs demonstrado é síncrono. O futuro acesso ao banco poderá utilizar await; gerar um documento em memória não torna automaticamente essa etapa assíncrona.

A transcrição não mostra explicitamente uma alteração na assinatura do contrato de Execute nesta aula.

## 10. Integração com o controller

O endpoint utiliza o array retornado pelo caso de uso para decidir qual resultado HTTP produzir.

Exemplo reconstruído conforme as aulas anteriores:

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

O identificador do use case e a assinatura assíncrona são reconstruções do desenho registrado anteriormente.

### Por que File()?

O método do controller retorna conteúdo binário, acompanhado do tipo de mídia e do nome de download.

Na solução adotada no curso, o tipo de mídia é application/octet-stream e o nome é report.xlsx.

A transcrição desta aula menciona oralmente report.xls no primeiro download, mas o contrato anterior especifica report.xlsx e o segundo teste utiliza report-1.xlsx. ClosedXML gera um documento XLSX neste fluxo; não se deve interpretar a menção oral como uma migração para o formato legado XLS.

### Caminho HTTP 204

A condição Length > 0 é verdadeira no teste, pois um workbook com cabeçalhos já produz bytes.

O fluxo de NoContent continua previsto no controller, mas não é testado nesta aula.

Quando a consulta de despesas for implementada, o use case precisará distinguir a ausência de registros de um documento válido contendo apenas cabeçalhos.

## 11. Teste no Swagger e depuração

O professor executa a API com F5, abre o Swagger e seleciona o endpoint de relatório.

O período escolhido é janeiro de 2024, utilizando uma entrada abreviada correspondente a 2024/1 no cabeçalho month.

O mês ainda não filtra despesas, mas é utilizado para nomear a worksheet.

### Sequência observada no debugger

1. A requisição atinge o breakpoint.
2. F10 permite acompanhar a criação de XLWorkbook.
3. O autor e as propriedades de fonte são configurados.
4. Uma worksheet correspondente a janeiro de 2024 é criada.
5. F11 permite entrar em InsertHeader.
6. MemoryStream é instanciado.
7. SaveAs preenche o stream com os bytes do Excel.
8. ToArray retorna o conteúdo ao controller.
9. O array inspecionado possui 6.435 posições.
10. Como Length é maior que zero, a execução segue o retorno File.
11. F5 continua a execução até a resposta aparecer no Swagger.

### Resultado observado

| Verificação | Resultado |
|---|---|
| Criação do workbook | Funcionou |
| Inserção do cabeçalho | Executada |
| Serialização em memória | Funcionou |
| Array retornado | 6.435 bytes no teste |
| Condição Length > 0 | Verdadeira |
| Resposta de download | Disponibilizada pelo Swagger |

O Swagger apresenta a opção Download File. O professor a utiliza para baixar o documento.

O tamanho de 6.435 bytes é uma observação daquele teste. Não é uma dimensão fixa para qualquer planilha criada pelo ClosedXML e não constitui um benchmark de desempenho.

## 12. Importação no Google Planilhas

O professor informa que não possui Microsoft Excel instalado em sua máquina. Por isso, utiliza o Google Planilhas para abrir o documento gerado.

### Procedimento demonstrado

1. Abrir uma planilha no Google Planilhas.
2. Selecionar Arquivo e Importar.
3. Escolher Fazer upload e Procurar.
4. Selecionar o arquivo de relatório baixado.
5. Escolher substituir a planilha existente.
6. Confirmar a importação.

A planilha anterior estava vazia, e o instrutor confirma a substituição.

### Resultado

O documento é importado e apresenta as cinco colunas esperadas:

| A | B | C | D | E |
|---|---|---|---|---|
| Título | Data | Tipo de pagamento | Valor | Descrição |

A tabela representa conceitualmente o significado dos cabeçalhos; a transcrição não fornece literalmente todas as traduções exibidas no primeiro arquivo.

O teste confirma que o arquivo gerado pode ser interpretado pelo Google Planilhas.

Não confirma compatibilidade com todos os aplicativos ou todas as funcionalidades do Excel.

Ainda não há linhas de despesas, porque essa parte da geração não foi implementada.

## 13. Download pelo Postman

Depois de testar com o Swagger, o professor demonstra uma segunda maneira de consumir o endpoint.

Cria uma requisição GET no Postman e copia a URL do endpoint.

### Configuração conceitual

```http
GET /api/report/excel HTTP/1.1
Host: localhost
month: 2024/1
Accept-Language: pt-BR
```

O host, a rota e o valor textual do período são representações reconstruídas. A porta local não foi informada. A gravação confirma a intenção de selecionar janeiro de 2024 e português brasileiro.

### Send and Download

Em vez de utilizar apenas Send, o professor abre o menu associado ao botão e seleciona a opção de enviar a requisição e baixar a resposta.

O Postman executa a chamada. O professor acompanha novamente a execução pelo debugger e, ao continuar, recebe a solicitação para escolher onde salvar o arquivo.

O segundo documento recebe o nome:

```text
report-1.xlsx
```

### Diferença entre servidor e cliente

O servidor não precisou gravar o relatório em D:\MyReport para enviá-lo.

O Postman, como cliente HTTP, permite que o usuário salve localmente os bytes recebidos.

Portanto, salvar o arquivo no computador do consumidor é uma operação diferente de exigir um caminho físico fixo no servidor.

## 14. Accept-Language e localização do relatório

No segundo teste, o professor informa Accept-Language com a cultura correspondente ao português brasileiro.

O cabeçalho month continua indicando o período solicitado.

| Cabeçalho | Finalidade no CashFlow |
|---|---|
| month | Fornecer a data de referência para o relatório |
| Accept-Language | Solicitar a cultura/idioma da resposta |

O middleware de cultura, desenvolvido anteriormente, seleciona a cultura utilizada pela aplicação.

Os cabeçalhos do documento são obtidos de ResourceReportGenerationMessages, introduzido na aula 121.

### Fluxo de localização

```text
Postman
    |
    | Accept-Language: pt-BR
    v
Culture Middleware
    |
    v
Recursos localizados
    |
    v
InsertHeader(worksheet)
    |
    v
XLSX com cabeçalhos traduzidos
```

O professor importa report-1.xlsx no Google Planilhas, repetindo o processo de substituição.

Ele observa que os cabeçalhos foram traduzidos e que a cor e o negrito permanecem corretos.

### O que o teste comprova?

O mecanismo de localização funcionou para o idioma solicitado no cenário demonstrado. O documento também preservou a estilização dos cabeçalhos durante a importação.

O professor não testa todas as culturas existentes, nem mede a fidelidade de todas as propriedades visuais.

A aula não traduz descrições de despesas, pois os dados ainda não são inseridos. Conforme a aula 121, a descrição cadastrada pelo usuário deve ser preservada.

### Precisão técnica

O nome de uma worksheet formatado por ToString("Y") depende da CurrentCulture. A seleção de recursos localizados depende normalmente da CurrentUICulture.

O middleware precisa configurar as culturas apropriadas para que ambas as funcionalidades acompanhem o idioma desejado.

Nesta aula, não é demonstrada uma nova implementação do middleware.

## 15. Gerenciamento de recursos e cuidados de desempenho

Esta seção contém complementos técnicos, não mudanças apresentadas pelo professor.

### 15.1 Descarte com using

XLWorkbook e MemoryStream implementam IDisposable.

Uma forma de garantir seu descarte ao sair do escopo é:

```csharp
using var workbook = new XLWorkbook();

// Configuração e preenchimento do documento.

using var file = new MemoryStream();

workbook.SaveAs(file);

return file.ToArray();
```

Esse exemplo pressupõe a criação de uma worksheet e a configuração necessária antes de SaveAs.

ToArray produz um array independente do stream. O descarte do stream após avaliar a expressão de retorno não invalida o array devolvido.

A gravação não mostra explicitamente a aplicação de using nessa etapa, portanto o código principal preserva o padrão narrado.

### 15.2 Memória não é apagada instantaneamente

O professor explica de maneira simplificada que a memória temporária será limpa após devolver o arquivo.

Tecnicamente, objetos gerenciados deixam de ser alcançáveis quando não há referências utilizáveis a eles, tornando-se elegíveis para coleta pelo garbage collector.

Dispose finaliza o uso dos recursos gerenciados pelo objeto conforme seu contrato, mas não garante limpeza imediata da RAM, zeragem dos bytes ou redução imediata da memória do processo.

O array retornado precisa permanecer disponível durante o processamento da resposta.

### 15.3 MemoryStream não garante menor consumo de memória

O uso de MemoryStream elimina a gravação física intermediária explícita, mas mantém dados em memória.

O workbook, o buffer interno do stream e o array produzido por ToArray podem coexistir durante parte da geração.

Relatórios grandes ou muitas requisições simultâneas podem elevar o consumo de memória.

A aula não compara desempenho entre MemoryStream, FileStream e outras abordagens.

### 15.4 Possível retorno de stream

O ASP.NET Core possui sobrecargas de File que aceitam streams.

Uma arquitetura alternativa poderia entregar um stream em vez de criar uma cópia adicional com ToArray, observando posição, ciclo de vida e descarte após a resposta.

Isso exigiria alterar o contrato atual do caso de uso e não foi implementado pelo professor.

### 15.5 Ausência de despesas e relatório vazio

Um XLSX com apenas cabeçalhos não possui tamanho zero.

Logo, Length > 0 comprova apenas que há bytes, não que há despesas no relatório.

No futuro, a ausência de registros deverá ser decidida antes de serializar um workbook vazio, caso o contrato HTTP 204 seja mantido.

### 15.6 Testes e segurança operacional

A validação atual é manual, com debugger, Swagger, Postman e importação no Google Planilhas.

Testes futuros poderiam verificar dados, cultura, integridade do documento, casos sem despesas e comportamento com volumes maiores.

Esses testes não são apresentados como já realizados.

## 16. Exercícios e perguntas de revisão

### Exercício 1 — classe abstrata

Por que new Stream() não compila?

**Resposta:** Stream é uma classe abstrata. É necessário utilizar uma implementação concreta, como MemoryStream.

### Exercício 2 — serializar em memória

```csharp
var file = new MemoryStream();

workbook.SaveAs(file);
```

O conteúdo serializado é escrito no stream, não no caminho fixo da máquina.

### Exercício 3 — devolver os bytes

```csharp
return file.ToArray();
```

A operação devolve um byte[] com o conteúdo do MemoryStream.

### Exercício 4 — descarte

Como garantir o descarte do stream após obter seus bytes?

```csharp
using var file = new MemoryStream();
workbook.SaveAs(file);

return file.ToArray();
```

Esse é um complemento técnico ao código da aula.

### Exercício 5 — interpretar o teste

Um arquivo com 6.435 bytes indica que existem 6.435 despesas?

**Resposta:** não. Essa quantidade representa o tamanho do documento em bytes naquele teste, e o documento ainda contém somente cabeçalhos.

### Exercício 6 — localização

Qual cabeçalho HTTP foi utilizado para solicitar a versão em português brasileiro?

```http
Accept-Language: pt-BR
```

### Exercício 7 — ausência de registros

Se o relatório for serializado com apenas cabeçalhos, Length será necessariamente zero?

**Resposta:** não. Um XLSX válido ainda possui estrutura e metadados. É necessário verificar a ausência de dados de negócio separadamente.

### Perguntas de revisão

1. Qual método do ClosedXML salva o workbook?
2. Por que utilizar workbook em vez de worksheet nessa operação?
3. O que representa o argumento string de SaveAs?
4. Por que o professor abandona a pasta MyReport?
5. Quais problemas um caminho físico fixo pode causar em produção?
6. O que é Stream?
7. Por que Stream não pode ser instanciada diretamente?
8. Qual classe concreta foi escolhida?
9. Como MemoryStream se relaciona com Stream?
10. Qual é a diferença entre MemoryStream e byte[]?
11. Qual método transforma o workbook em bytes no stream?
12. Qual método permite devolver o conteúdo como array?
13. ToArray depende da posição atual do stream?
14. Qual foi o tamanho observado no primeiro teste?
15. Qual mês e ano foram solicitados?
16. Por que Length > 0 direciona o controller para File?
17. Qual opção aparece no Swagger para baixar o arquivo?
18. Qual ferramenta é usada para importar o XLSX?
19. Qual opção do Postman envia e baixa a resposta?
20. Qual nome recebe o segundo arquivo?
21. Qual cabeçalho controla o idioma desejado?
22. O que foi validado no segundo documento?
23. O professor testou o retorno 204?
24. As despesas do banco já são exportadas?
25. Por que Dispose não significa apagar imediatamente todos os bytes da memória?
26. Qual é a próxima etapa anunciada?

## 17. Resumo e mapa mental

| Elemento | Resultado |
|---|---|
| Método de serialização | workbook.SaveAs(file) |
| Abstração | Stream |
| Implementação | MemoryStream |
| Conversão | file.ToArray() |
| Retorno do caso de uso | byte[] |
| Primeiro teste | Janeiro de 2024 |
| Tamanho observado | 6.435 bytes |
| Download | Swagger e Postman |
| Segundo arquivo | report-1.xlsx |
| Localização | Accept-Language: pt-BR |
| Validação | Importação no Google Planilhas |
| Cabeçalhos, cor e negrito | Verificados |
| Dados de despesas | Próxima etapa |

```text
                     AULA 122
                        |
                    XLWorkbook
                        |
                  InsertHeader()
                        |
               workbook.SaveAs(...)
                        |
           +------------+------------+
           |                         |
        caminho                   MemoryStream
        físico                    (escolhido)
      (descartado)                    |
                                   ToArray()
                                      |
                                    byte[]
                                      |
                                    File()
                                      |
                         +------------+------------+
                         |                         |
                      Swagger                   Postman
                         |                         |
                   Download File          Send and Download
                         |                         |
                         |                 Accept-Language
                         |                      pt-BR
                         +------------+------------+
                                      |
                               Google Planilhas
                                      |
                         Cabeçalhos e estilos validados
                                      |
                          Próxima aula: inserir dados
```

**Conclusão:** a aula 122 completa a primeira geração funcional do arquivo Excel no CashFlow. Em vez de exigir um caminho físico específico do servidor, o caso de uso serializa o workbook em um MemoryStream e devolve os bytes por ToArray. O endpoint entrega o documento para download, e o professor valida sua importação no Google Planilhas tanto pelo Swagger quanto pelo Postman. O segundo teste confirma a localização dos cabeçalhos para português brasileiro. A geração do arquivo está comprovada nessa etapa, mas o relatório de despesas ainda precisa receber os dados filtrados do banco.
