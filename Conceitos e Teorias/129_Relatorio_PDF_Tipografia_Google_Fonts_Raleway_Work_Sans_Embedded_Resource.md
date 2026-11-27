# Aula 129 — Relatório PDF (parte 2): tipografia, Google Fonts e Embedded Resources

> **Contexto:** na aula 128, o CashFlow preparou o endpoint PDF, o caso de uso e a consulta mensal. O gerador ainda retorna um array vazio provisório. Nesta aula, o foco é preparar os arquivos de fontes que serão empregados na construção visual do documento.
>
> **Objetivo:** identificar famílias e variantes tipográficas no template do Figma, entender peso e estilo, baixar quatro arquivos de fonte e incorporá-los ao projeto Application com a Build Action Embedded Resource.
>
> **Fidelidade:** a transcrição utiliza grafias como Halloway, Halloween e Helloween. Pelo contexto e pela consulta ao catálogo do Google Fonts, a identificação mais provável é Raleway. Essa normalização é uma inferência, não uma leitura direta do template Figma. Work Sans é identificada explicitamente. Os nomes de arquivos e da pasta Fonts são reconstruções coerentes com a explicação. A biblioteca de PDF ainda não é instalada e o carregamento das fontes ainda não é implementado.
>
> **Fontes consultadas para os complementos:** catálogo oficial Google Fonts e documentação Microsoft Learn sobre Build Actions e recursos de manifesto.

## Índice

1. Contexto e objetivo
2. O template de PDF no Figma
3. Família tipográfica, peso e estilo
4. Pesos numéricos e hierarquia visual
5. Regular, Black e Italic
6. Quatro variantes selecionadas
7. Arquivos estáticos e fontes variáveis
8. Download no Google Fonts
9. Instalação local versus arquivos da aplicação
10. Organização das fontes no projeto
11. Build Action: Embedded Resource
12. Configuração equivalente no .csproj
13. Recursos incorporados e assembly
14. Leitura futura com Reflection e Stream
15. Licenças, portabilidade e cuidados
16. Exercícios e perguntas de revisão
17. Resumo e mapa mental

---

## 1. Contexto e objetivo

O relatório PDF foi iniciado na aula 128 com a estrutura de comunicação HTTP:

- GET /api/report/pdf.
- Parâmetro DateOnly month recebido por FromQuery.
- Consulta FilterByMonth no repositório de leitura.
- Retorno vazio quando não existem despesas.
- Tipo de mídia application/pdf e nome report.pdf.
- Implementação provisória que ainda retorna um array vazio no caminho com despesas.

Antes de começar a desenhar o PDF, o professor prepara suas fontes.

A escolha tipográfica determina como aparecerão títulos, valores, nomes, letras, números e símbolos. Ela influencia legibilidade, identidade visual e hierarquia da informação.

O professor utiliza um template criado no Figma como referência para o futuro documento.

Nesta aula, nenhum PDF é renderizado. O resultado é a preparação dos quatro arquivos de fonte no projeto.

## 2. O template de PDF no Figma

O professor abre o arquivo que criou para orientar a aparência do relatório.

Selecionando elementos de texto no Figma, observa na seção de propriedades:

- Nome da família tipográfica.
- Variante ou peso.
- Estilo.
- Tamanho do texto.

### Elementos examinados

| Elemento do template | Família | Variante | Tamanho confirmado |
|---|---|---|---|
| Total das despesas | Work Sans | Black | Não informado |
| Pequeno título | Raleway (identificação provável) | Regular | 15, conforme propriedade do Figma |
| Nome exibido no relatório | Raleway (identificação provável) | Black | Não informado |

Work Sans Regular também é selecionada para integrar o conjunto de fontes utilizadas futuramente.

A aula não fornece todos os detalhes do layout nem associa cada elemento do futuro PDF a uma variante. Não se deve inferir dimensões, espaçamentos, cores ou coordenadas do documento.

O tamanho 15 é o valor observado na interface do Figma; isso não determina automaticamente 15 pontos na API de uma biblioteca PDF.

## 3. Família tipográfica, peso e estilo

O instrutor apresenta três características importantes: nome da fonte, peso e estilo.

Como precisão terminológica, a família agrupa desenhos tipográficos relacionados. Peso e estilo são características que permitem selecionar uma variante dessa família.

| Conceito | Significado | Exemplo |
|---|---|---|
| Família | Conjunto de desenhos relacionados | Work Sans |
| Peso | Espessura relativa dos traços | Regular, Black |
| Estilo | Característica do desenho, como normal ou itálico | Italic |
| Tamanho | Dimensão de apresentação dos caracteres | 15 |
| Variante/face | Combinação específica de características da família | Work Sans Black |

O professor utiliza o termo família para explicar a combinação de nome, peso e estilo. A distinção acima complementa a terminologia da gravação.

### Exemplo

```text
Work Sans
    |
    +-- Normal
    |     +-- Regular
    |     +-- Black
    |
    +-- Italic
          +-- Regular Italic
          +-- Black Italic
```

O esquema ilustra apenas algumas variantes. A família oferece outros pesos.

## 4. Pesos numéricos e hierarquia visual

Na demonstração do Google Fonts, o professor observa pesos como 100, 200, 300 e 400. Quanto maior o peso, em geral mais espessos são os traços dos caracteres naquela família.

### Convenção tipográfica usual — complemento

| Peso | Nome usual |
|---:|---|
| 100 | Thin |
| 200 | Extra Light |
| 300 | Light |
| 400 | Regular |
| 500 | Medium |
| 600 | Semi Bold |
| 700 | Bold |
| 800 | Extra Bold |
| 900 | Black |

Os nomes e pesos disponíveis dependem da família. Não se deve presumir que toda fonte ofereça exatamente todas essas opções.

### Por que utilizar pesos diferentes?

O relatório precisa comunicar uma hierarquia:

- O total das despesas merece maior destaque.
- O nome apresentado pode receber uma ênfase forte.
- Títulos menores e textos de apoio podem utilizar pesos regulares.

Essa diferença permite guiar a leitura sem precisar destacar tudo com a mesma intensidade.

### Precisão técnica sobre cor

O professor descreve que caracteres de peso maior parecem mais escuros, enquanto caracteres finos parecem mais claros.

Isso não significa que alterar o peso modifique o valor da cor ou a opacidade configurada.

Se duas variantes possuem preenchimento preto, ambas podem continuar com a mesma cor. A variante mais espessa cobre maior área e pode produzir uma aparência visual de maior densidade. A rasterização e o antialiasing também influenciam essa percepção.

Peso, cor e opacidade são propriedades distintas.

## 5. Regular, Black e Italic

### Regular

Normalmente representa o peso 400, adequado a textos que não precisam de ênfase muito forte.

### Black

Normalmente representa o peso 900. É mais pesado que Bold (700) e Extra Bold (800) na convenção usual.

O professor utiliza Black no total das despesas e no nome exibido no template.

A expressão informal extra extra bold utilizada na aula comunica uma espessura elevada, mas Black é o nome específico da variante escolhida.

### Italic

É um estilo de desenho geralmente inclinado, que pode apresentar formas próprias em alguns caracteres.

Itálico não é sinônimo de negrito.

Uma fonte pode combinar peso Black com estilo itálico, assim como pode combinar Regular com itálico.

Na demonstração do Figma, o instrutor observa grupos de variantes normais e itálicas, cada um com diversos pesos.

As variantes itálicas são apresentadas para explicar o conceito; nenhuma variante itálica integra os quatro arquivos selecionados para o projeto nesta aula.

## 6. Quatro variantes selecionadas

O professor escolhe duas famílias e dois pesos para cada uma.

| Família | Variante | Peso usual | Estilo |
|---|---|---:|---|
| Raleway (provável) | Regular | 400 | Normal |
| Raleway (provável) | Black | 900 | Normal |
| Work Sans | Regular | 400 | Normal |
| Work Sans | Black | 900 | Normal |

Assim, serão quatro arquivos de fonte, e não quatro famílias diferentes.

### Identificação dos nomes

A grafia Work Sans está clara na transcrição. A segunda família aparece com várias transcrições fonéticas inconsistentes.

O Google Fonts mantém uma família denominada Raleway, que oferece pesos de 100 a 900 e variantes normais e itálicas. A descrição da aula é compatível com essa família, motivo pelo qual este material adota Raleway como identificação provável.

Referências para conferência:

- https://fonts.google.com/specimen/Raleway
- https://fonts.google.com/specimen/Work+Sans
- https://github.com/google/fonts/blob/main/ofl/raleway/METADATA.pb
- https://github.com/google/fonts/blob/main/ofl/worksans/METADATA.pb

Caso o arquivo original do Figma ou os arquivos fornecidos pelo curso indiquem outro nome, prevalece a identificação do material original.

## 7. Arquivos estáticos e fontes variáveis

Depois de extrair o ZIP, o professor entra nas pastas static das famílias.

Ali seleciona Black e Regular de cada uma.

### Fonte estática

Uma fonte estática fornece uma face ou combinação definida de características. Por exemplo, um arquivo para Regular e outro para Black.

### Fonte variável — complemento

Uma fonte variável pode armazenar um conjunto de variações em um único arquivo, permitindo selecionar valores em eixos como peso.

A possibilidade de utilizar uma fonte variável depende da compatibilidade da biblioteca e da API empregada para carregar e renderizar a fonte.

A gravação seleciona arquivos estáticos. Ela não testa fontes variáveis na futura biblioteca de PDF.

### Nomes ilustrativos dos arquivos

```text
Raleway-Regular.ttf
Raleway-Black.ttf
WorkSans-Regular.ttf
WorkSans-Black.ttf
```

Os nomes seguem convenções usuais dos arquivos TrueType dessas famílias. A transcrição não mostra literalmente os nomes completos nem suas extensões; confirme os nomes reais no pacote baixado.

A organização dos arquivos oferecidos pelo Google Fonts pode mudar entre versões. Se o download não apresentar uma pasta static, deve-se localizar as variantes estáticas adequadas nas fontes oficiais ou utilizar um mecanismo compatível com a biblioteca futura.

Não se deve renomear um arquivo variável como se ele fosse uma variante estática.

## 8. Download no Google Fonts

O professor acessa a biblioteca Google Fonts pelo navegador.

Durante a demonstração, menciona outras famílias conhecidas, como Montserrat, Open Sans e Roboto.

A escolha do curso, entretanto, é Work Sans e a segunda família identificada provavelmente como Raleway.

### Procedimento demonstrado

1. Acessar Google Fonts.
2. Buscar a primeira família.
3. Visualizar os pesos e estilos oferecidos.
4. Testar a aparência digitando um texto de exemplo.
5. Selecionar Get font.
6. Voltar ao catálogo e localizar Work Sans.
7. Selecionar Get font também para Work Sans.
8. Baixar as duas famílias selecionadas.
9. Extrair o arquivo ZIP.
10. Localizar as variantes estáticas Regular e Black de ambas.
11. Copiar somente os quatro arquivos necessários ao projeto.

Os nomes dos botões são aqueles mencionados na gravação. A interface atual do site pode apresentar diferenças.

O professor baixa as famílias gratuitamente e explica como visualizá-las antes da seleção.

A transcrição não fornece a versão exata dos arquivos ou seus hashes.

## 9. Instalação local versus arquivos da aplicação

O professor mostra que é possível abrir um arquivo de fonte no Windows com duplo clique, visualizar uma amostra e clicar em Install.

Essa instalação torna a fonte disponível a aplicativos que utilizam as fontes instaladas no ambiente, conforme suas próprias configurações.

O instrutor relata que já havia instalado as fontes para trabalhar no template do Figma.

Para a implementação da API, ele não precisa repetir essa instalação. Em vez disso, copia os arquivos para o projeto.

### Distinção

| Estratégia | Finalidade |
|---|---|
| Instalar uma fonte no sistema operacional | Disponibilizá-la a aplicativos que consultam fontes do ambiente |
| Manter o arquivo no projeto | Permitir que a aplicação distribua ou incorpore o recurso |
| Registrar a fonte na biblioteca de PDF | Permitir que o gerador utilize a face selecionada |

Essas operações não são equivalentes.

Incluir um arquivo como recurso incorporado não instala a fonte no Windows ou Linux. Também não a registra automaticamente na biblioteca de PDF.

O professor anuncia que ainda será necessária uma configuração específica de fontes na próxima aula.

### Por que não depender apenas das fontes da máquina?

A API poderá ser executada em ambientes diferentes do computador do desenvolvedor.

Incorporar os arquivos selecionados ajuda a tornar o recurso disponível junto ao código, sem depender exclusivamente da instalação local da fonte.

Isso não constitui, por si só, uma garantia de renderização idêntica em todos os ambientes. O gerador precisa oferecer suporte ao formato, às variantes e aos caracteres utilizados.

## 10. Organização das fontes no projeto

O professor expande o projeto Application no Visual Studio e navega até:

```text
Application
└── UseCases
    └── Expenses
        └── Reports
            ├── Excel
            └── PDF
                └── Fonts
                    ├── Raleway-Black.ttf
                    ├── Raleway-Regular.ttf
                    ├── WorkSans-Black.ttf
                    └── WorkSans-Regular.ttf
```

Fonts é a representação em inglês da pasta de fontes mencionada na narração. Os nomes dos arquivos e a capitalização devem ser conferidos no projeto original.

O instrutor cria a pasta dentro de PDF, copia os quatro arquivos no explorador do sistema e cola na pasta selecionada no Solution Explorer.

A escolha mantém os recursos próximos da implementação que irá utilizá-los.

Não há instalação de pacote NuGet de PDF nesta aula.

## 11. Build Action: Embedded Resource

A etapa central da configuração acontece nas propriedades de cada arquivo.

O professor observa que a propriedade Build Action está inicialmente configurada como None.

Ele altera essa propriedade para Embedded Resource.

### Procedimento no Visual Studio

1. Selecionar o primeiro arquivo de fonte no Solution Explorer.
2. Abrir ou visualizar a janela Properties.
3. Localizar Build Action, apresentada na seção de propriedades avançadas.
4. Alterar None para Embedded Resource.
5. Repetir para os outros três arquivos.
6. Selecionar novamente cada arquivo para confirmar a configuração.

A aula enfatiza que os quatro arquivos devem receber a mesma configuração.

### O que significa Build Action?

É a classificação que orienta o tratamento do arquivo pelo sistema de build.

Conforme a documentação do Visual Studio, Embedded Resource incorpora o arquivo ao assembly durante a compilação.

| Build Action | Finalidade geral |
|---|---|
| None | Não compilar nem incorporar o arquivo por essa classificação |
| Compile | Tratar o arquivo como código-fonte a compilar |
| Content | Classificar como conteúdo; sua cópia ou publicação depende da configuração |
| Embedded Resource | Incorporar o recurso ao assembly .NET |

None não significa necessariamente que o arquivo não possa ser copiado em uma etapa de build: metadados de cópia e publicação podem ser configurados independentemente.

A aula não configura Copy to Output Directory como alternativa; escolhe Embedded Resource.

Referência: https://learn.microsoft.com/en-us/visualstudio/ide/build-actions

## 12. Configuração equivalente no .csproj

**Complemento técnico:** a configuração feita na janela Properties pode ser representada por itens EmbeddedResource do MSBuild.

Exemplo ilustrativo para um projeto SDK-style:

```xml
<ItemGroup>
  <None Remove="UseCases/Expenses/Reports/PDF/Fonts/*.ttf" />
  <EmbeddedResource Include="UseCases/Expenses/Reports/PDF/Fonts/*.ttf" />
</ItemGroup>
```

Esse exemplo remove a classificação padrão None dos arquivos correspondentes e os inclui como recursos incorporados.

Os caminhos devem corresponder à estrutura efetiva do projeto.

O Visual Studio pode gerar itens individuais, em vez de um padrão com wildcard. Não se deve inserir itens duplicados se os recursos já estiverem configurados no .csproj.

### Alternativa com nomes explícitos

```xml
<ItemGroup>
  <EmbeddedResource Include="UseCases/Expenses/Reports/PDF/Fonts/Raleway-Regular.ttf" />
  <EmbeddedResource Include="UseCases/Expenses/Reports/PDF/Fonts/Raleway-Black.ttf" />
  <EmbeddedResource Include="UseCases/Expenses/Reports/PDF/Fonts/WorkSans-Regular.ttf" />
  <EmbeddedResource Include="UseCases/Expenses/Reports/PDF/Fonts/WorkSans-Black.ttf" />
</ItemGroup>
```

Esse é um exemplo de configuração, não a reprodução do .csproj apresentado na gravação.

A aula altera a propriedade individualmente pela interface gráfica.

## 13. Recursos incorporados e assembly

Um assembly .NET pode conter código compilado, metadados e recursos.

A Build Action Embedded Resource permite incluir os bytes dos arquivos selecionados no assembly produzido pelo projeto.

```text
Arquivos de fonte no projeto
           |
           v
Build Action: Embedded Resource
           |
           v
MSBuild / compilador
           |
           v
CashFlow.Application.dll
    + código compilado
    + metadados
    + recursos incorporados
           |
           v
Aplicação pode recuperar os recursos
```

O nome CashFlow.Application.dll representa a saída usual de uma biblioteca Application com esse nome. A configuração de build pode alterar o nome do assembly.

### Benefício

A aplicação pode recuperar os bytes da fonte a partir do assembly, sem precisar construir um caminho físico para a pasta original de desenvolvimento.

Isso é especialmente útil quando a distribuição do projeto não inclui o diretório original de arquivos da mesma maneira que o ambiente local.

### Recursos de manifesto versus recursos localizados

Nas aulas de localização, o CashFlow utiliza arquivos .resx para mensagens de erro e cabeçalhos.

Nesta aula, os arquivos binários de fonte são tratados como recursos incorporados.

Ambos utilizam mecanismos de recursos do .NET, mas não possuem a mesma finalidade nem necessariamente a mesma forma de acesso.

| Recurso | Uso |
|---|---|
| .resx | Textos e outros valores acessados por mecanismos de recursos, inclusive localização |
| .ttf incorporado | Arquivo binário recuperável pelo manifesto do assembly |

**Importante:** incorporar a fonte no assembly não é o mesmo que incorporar os dados da fonte dentro do PDF final. O gerador precisa carregar a fonte e decidir como representá-la no documento.

## 14. Leitura futura com Reflection e Stream

Esta seção é um complemento, antecipando apenas o mecanismo geral do .NET. O professor deixa a configuração da biblioteca de PDF para a próxima aula.

Assembly oferece métodos para listar e recuperar recursos de manifesto.

### Listar os recursos disponíveis

```csharp
var assembly = typeof(GenerateExpensesReportPdfUseCase).Assembly;

foreach (var name in assembly.GetManifestResourceNames())
{
    Console.WriteLine(name);
}
```

O identificador GenerateExpensesReportPdfUseCase foi reconstruído na aula 128. Utilize o nome real da classe situada na Application.

Usar typeof de uma classe do projeto ajuda a selecionar o assembly que efetivamente possui as fontes. O assembly de entrada da aplicação pode ser o projeto API, que não necessariamente contém os recursos da Application.

### Recuperar um recurso

```csharp
var assembly = typeof(GenerateExpensesReportPdfUseCase).Assembly;

const string resourceName =
    "CashFlow.Application.UseCases.Expenses.Reports.PDF.Fonts.WorkSans-Regular.ttf";

using var fontStream =
    assembly.GetManifestResourceStream(resourceName)
    ?? throw new InvalidOperationException(
        $"Recurso não encontrado: {resourceName}");

// Utilizar os bytes conforme a API do gerador de PDF.
```

O nome do recurso acima é ilustrativo. O nome real depende de RootNamespace, caminho, metadados LogicalName e demais configurações do MSBuild. GetManifestResourceNames permite descobrir o identificador efetivo.

GetManifestResourceStream pode devolver null quando o recurso não é encontrado ou não está acessível.

A escolha de quando descartar o stream depende da API consumidora: se ela precisar do stream após a chamada inicial, sua duração deve ser preservada. Essa consideração retoma IDisposable e using da aula 127.

Não se deve atribuir esse trecho ao professor nesta gravação. Ele ainda não carrega nenhum recurso via código.

Referências:

- https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getmanifestresourcestream
- https://learn.microsoft.com/en-us/dotnet/core/resources/manifest-file-names

## 15. Licenças, portabilidade e cuidados

### 15.1 Licença das fontes

O professor explica que os arquivos podem ser baixados gratuitamente pelo Google Fonts.

Como complemento verificado, os metadados oficiais de Raleway e Work Sans indicam a SIL Open Font License (OFL).

A licença das fontes é distinta da licença do ClosedXML e da futura biblioteca PDF.

Ao redistribuir arquivos de fonte com uma aplicação, deve-se conferir a licença correspondente à versão baixada e preservar os avisos exigidos. A OFL contém condições relativas à redistribuição e, quando aplicável, a nomes reservados e modificações.

Não se deve assumir que qualquer arquivo encontrado gratuitamente na internet possa ser incorporado e redistribuído sem restrições.

Referências:

- https://github.com/google/fonts/tree/main/ofl/raleway
- https://github.com/google/fonts/tree/main/ofl/worksans
- https://openfontlicense.org/

### 15.2 Cobertura de caracteres

Um relatório de despesas pode conter acentos, caracteres de diferentes idiomas e símbolos monetários.

É necessário verificar se as fontes selecionadas possuem os glifos necessários.

A aula não testa a cobertura de caracteres das fontes nem a renderização de euro, real, francês ou outros idiomas no PDF.

### 15.3 Portabilidade

Recursos incorporados evitam depender de caminhos de desenvolvimento para esses arquivos. Contudo, a biblioteca pode exigir registro das fontes, suporte a determinadas plataformas ou configuração adicional.

A aula prepara somente a disponibilidade dos arquivos.

### 15.4 Tamanho da aplicação

Incorporar recursos aumenta o tamanho do assembly na medida em que seus bytes fazem parte da saída compilada.

Selecionar apenas as quatro variantes necessárias limita o conjunto de recursos distribuídos, mas não há medição de tamanho ou memória nesta gravação.

### 15.5 Fontes do PDF

A incorporação de uma fonte no documento final, integral ou por subconjunto de glifos, é responsabilidade do gerador e de sua configuração.

A ação Embedded Resource não gera automaticamente essa incorporação no PDF.

## 16. Exercícios e perguntas de revisão

### Exercício 1 — identificar as variantes

Quais são os dois pesos selecionados para cada família?

**Resposta:** Regular e Black.

### Exercício 2 — peso e cor

Se o texto muda de Regular para Black sem alterar a cor configurada, seu valor RGB necessariamente mudou?

**Resposta:** não. A alteração está no desenho e espessura dos traços; a percepção de densidade visual pode mudar sem modificar a cor.

### Exercício 3 — quantidade de arquivos

Duas famílias, com duas variantes por família, resultam em quatro arquivos estáticos selecionados no procedimento da aula.

### Exercício 4 — configurar o projeto

Qual Build Action deve ser escolhida para cada fonte?

**Resposta:** Embedded Resource.

### Exercício 5 — identificar o assembly

Por que usar typeof de uma classe da Application para encontrar os recursos?

**Resposta:** porque as fontes estão incorporadas ao assembly dessa camada, e não necessariamente ao assembly de entrada da API.

### Exercício 6 — diagnosticar recurso ausente

Como descobrir o nome real de um recurso incorporado?

```csharp
var names = typeof(GenerateExpensesReportPdfUseCase)
    .Assembly.GetManifestResourceNames();
```

Exemplo complementar: inspecione names e compare com o nome utilizado na chamada GetManifestResourceStream.

### Perguntas de revisão

1. Qual é o objetivo da aula 129?
2. Qual ferramenta fornece o template visual?
3. Qual família é usada no total das despesas?
4. Qual variante foi escolhida para o total?
5. Qual variante foi escolhida para o pequeno título?
6. Qual tamanho desse título foi mencionado?
7. Qual é a identificação provável da segunda família?
8. Qual é a diferença entre família, peso e estilo?
9. O que normalmente representa o peso 400?
10. O que normalmente representa o peso 900?
11. Italic e Black descrevem a mesma característica?
12. Alterar peso modifica necessariamente a cor?
13. Quais quatro variantes são selecionadas?
14. Qual a diferença entre fonte estática e variável?
15. Em qual site o professor baixa as fontes?
16. É necessário instalar as fontes no sistema operacional para copiá-las ao projeto?
17. Em qual projeto os arquivos são colocados?
18. Qual pasta os agrupa?
19. Qual era a Build Action inicial?
20. Para qual Build Action o professor altera os quatro arquivos?
21. Qual é a finalidade de Embedded Resource?
22. Incorporar uma fonte ao assembly equivale a registrá-la no gerador PDF?
23. Qual mecanismo do .NET permite listar recursos de manifesto?
24. Por que o nome de recurso precisa ser conferido?
25. Qual licença é indicada nos metadados oficiais das duas famílias?
26. A biblioteca de geração de PDF já foi instalada?
27. Um PDF válido foi gerado nesta aula?
28. Quais configurações ficam para a próxima gravação?

## 17. Resumo e mapa mental

| Elemento | Resultado |
|---|---|
| Referência visual | Template Figma |
| Família explícita | Work Sans |
| Segunda família provável | Raleway |
| Pesos utilizados | Regular e Black |
| Quantidade de arquivos | Quatro |
| Origem | Google Fonts |
| Arquivos selecionados | Variantes estáticas |
| Projeto | Application |
| Organização | Expenses/Reports/PDF/Fonts (reconstrução) |
| Build Action anterior | None |
| Build Action escolhida | Embedded Resource |
| Licença das famílias identificadas | OFL, conforme metadados oficiais |
| Registro na biblioteca PDF | Pendente |
| Instalação da biblioteca PDF | Próxima aula |
| Geração do documento | Ainda não implementada |

```text
                 AULA 129 — FONTES DO PDF
                             |
                        Template Figma
                             |
              +--------------+--------------+
              |                             |
        Work Sans                       Raleway*
              |                             |
        Regular / Black              Regular / Black
              |                             |
              +--------------+--------------+
                             |
                        Google Fonts
                             |
                       Download + ZIP
                             |
                    4 arquivos estáticos
                             |
              Application / Reports / PDF / Fonts
                             |
                   Build Action: Embedded Resource
                             |
                     Assembly da Application
                             |
                      Próxima aula:
             instalar biblioteca e configurar fontes

          * identificação provável pela transcrição.
```

**Conclusão:** a aula 129 prepara os recursos tipográficos do futuro relatório PDF. O professor analisa o template no Figma, explica famílias, pesos e estilos, seleciona as variantes Regular e Black de duas famílias e copia os quatro arquivos para o projeto Application. A configuração Embedded Resource permite incorporar esses arquivos ao assembly. A aula ainda não instala a biblioteca de PDF, não registra as fontes no gerador e não produz um documento. Os exemplos de MSBuild e Reflection complementam a explicação sem antecipar a implementação como concluída.
