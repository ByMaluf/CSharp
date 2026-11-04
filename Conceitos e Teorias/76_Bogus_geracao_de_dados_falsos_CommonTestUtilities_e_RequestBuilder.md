# Bogus, dados falsos para testes e biblioteca CommonTestUtilities

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [O problema dos dados hard-coded nos testes](#2-o-problema-dos-dados-hard-coded-nos-testes)
3. [Por que compartilhar a criação de requisições](#3-por-que-compartilhar-a-criação-de-requisições)
4. [Criando o projeto CommonTestUtilities](#4-criando-o-projeto-commontestutilities)
5. [Organizando a pasta Requests e o builder](#5-organizando-a-pasta-requests-e-o-builder)
6. [Referências entre projetos](#6-referências-entre-projetos)
7. [Instalando o pacote Bogus](#7-instalando-o-pacote-bogus)
8. [O que a biblioteca Bogus oferece](#8-o-que-a-biblioteca-bogus-oferece)
9. [Gerando dados com uma instância Faker](#9-gerando-dados-com-uma-instância-faker)
10. [Faker<T> e a configuração fluente com RuleFor](#10-fakert-e-a-configuração-fluente-com-rulefor)
11. [Preenchendo os campos de uma despesa](#11-preenchendo-os-campos-de-uma-despesa)
12. [Montando o método Build](#12-montando-o-método-build)
13. [Como a estrutura poderá ser reutilizada nos testes](#13-como-a-estrutura-poderá-ser-reutilizada-nos-testes)
14. [Diferença entre os RuleFor do Bogus e do FluentValidation](#14-diferença-entre-os-rulefor-do-bogus-e-do-fluentvalidation)
15. [Cuidados práticos com dados aleatórios](#15-cuidados-práticos-com-dados-aleatórios)
16. [O que fica para a próxima aula](#16-o-que-fica-para-a-próxima-aula)
17. [Resumo](#17-resumo)
18. [Visão geral](#18-visão-geral)

---

## 1. Objetivo da aula

Na aula anterior, criamos o primeiro teste unitário para `RegisterExpenseValidator` utilizando **xUnit**, `[Fact]`, o padrão **Arrange–Act–Assert (AAA)** e a asserção `Assert.True(result.IsValid)`.

Para montar o cenário de sucesso, criamos a requisição diretamente no método de teste e preenchemos manualmente propriedades como título, descrição, valor, data e tipo de pagamento.

Nesta aula, o instrutor propõe uma melhoria: **centralizar a criação de requisições usadas em testes e gerar dados falsos (*fake data*) com a biblioteca Bogus**.

Os objetivos práticos são:

- reduzir o uso de valores fixos nos testes;
- evitar repetir a montagem da mesma requisição em vários projetos de testes;
- criar uma **Class Library** compartilhada, chamada `CommonTestUtilities`;
- organizar os objetos de teste na pasta `Requests`;
- criar uma classe *builder* para `RequestRegisterExpenseJson`;
- instalar o pacote NuGet `Bogus`;
- conhecer a criação de dados falsos e a sintaxe fluente de `Faker<T>`.

**Limite desta aula:** o instrutor prepara o gerador e o builder. A utilização final dessa estrutura dentro do teste existente e a execução dos testes ficam para a aula seguinte.

---

## 2. O problema dos dados hard-coded nos testes

No teste da aula 75, preparamos uma requisição com valores manuais. De forma ilustrativa:

```csharp
var request = new RequestRegisterExpenseJson
{
    Title = "Apple",
    Description = "Descrição da despesa",
    Amount = 100,
    Date = DateTime.Now.AddDays(-1),
    PaymentType = PaymentType.CreditCard
};
```

A implementação funciona para um teste inicial, mas o instrutor identifica **dois problemas**.

### Problema 1 — Dados fixos em todas as execuções

Quando utilizamos os mesmos valores literais dentro do código, os testes tendem a receber sempre os mesmos dados.

No exemplo, o título continua sendo `"Apple"`, o valor continua sendo `100` e o tipo de pagamento continua sendo `CreditCard`. A data se modifica porque é calculada relativamente à data atual.

O professor quer gerar dados **variados, mas coerentes com o tipo e com as regras esperadas**.

### Problema 2 — Repetição em diferentes testes

O mesmo objeto de requisição será necessário em mais de um lugar.

O instrutor usa como exemplos:

- testes de **validators**;
- testes de **use cases**, responsáveis pelas regras de negócio.

Se cada projeto repetir o mesmo bloco de inicialização, uma mudança no contrato da requisição exigirá corrigir a criação do objeto em múltiplos arquivos.

### Antes da melhoria

```text
Validators.Test
└── Teste de cadastro
    └── new RequestRegisterExpenseJson { ... }

UseCases.Test (futuro)
└── Teste de cadastro
    └── new RequestRegisterExpenseJson { ... }

Outros testes
└── new RequestRegisterExpenseJson { ... }
```

**Problema:** a mesma responsabilidade de construir dados de entrada fica espalhada pelo código dos testes.

---

## 3. Por que compartilhar a criação de requisições

O instrutor explica que costuma organizar suas suítes em projetos específicos para diferentes tipos de teste, mencionando **quatro projetos** em seu padrão pessoal de trabalho.

A aula descreve explicitamente dois exemplos:

- um projeto para os **validators**;
- outro projeto para os **use cases**.

O vídeo não detalha os outros dois tipos de projeto; portanto, eles não serão inventados aqui.

### Vantagem da centralização

Se a classe `RequestRegisterExpenseJson` ganhar uma propriedade obrigatória, remover outra propriedade ou mudar o modo de inicialização, a lógica responsável pela criação de uma requisição padrão pode ser ajustada em **um único lugar**.

O objetivo é tornar os testes mais simples de escrever e manter.

### Depois da melhoria

```text
                   CommonTestUtilities
                           │
                           ▼
               RequestRegisterExpenseBuilder
                           │
                           ▼
                 Request de teste válida
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
        Validators.Test           UseCases.Test
            (atual)                 (futuro)
```

O builder concentra a responsabilidade de **preparar uma requisição de teste**.

Isso não significa que toda alteração da request será automaticamente compatível com todos os testes. Significa que existe uma infraestrutura comum para facilitar essas alterações.

---

## 4. Criando o projeto CommonTestUtilities

O professor cria uma biblioteca de classes dentro da área de testes da Solution.

### Passos mostrados no Visual Studio

1. Localizar a pasta `tests` na solução.
2. Clicar com o botão direito e escolher **Adicionar novo projeto**.
3. Selecionar o template **Class Library** (*Biblioteca de Classes*).
4. Informar um nome significativo: `CommonTestUtilities`.
5. Confirmar a localização física dentro da pasta `tests`.
6. Criar o projeto.
7. Apagar a classe de exemplo gerada automaticamente pelo template.

### Por que uma Class Library?

Porque esse projeto não terá um endpoint HTTP e nem será um executável.

Sua função é disponibilizar **classes auxiliares que podem ser referenciadas pelos projetos de testes**.

Essa é a mesma categoria de projeto utilizada anteriormente na arquitetura da solução para camadas como Application e Domain: uma biblioteca de classes.

### Organização prevista

```text
CashFlow
├── src
│   ├── CashFlow.API
│   ├── CashFlow.Application
│   ├── CashFlow.Communication
│   ├── CashFlow.Domain
│   ├── CashFlow.Exception
│   └── CashFlow.Infrastructure
│
└── tests
    ├── Validators.Test
    └── CommonTestUtilities
```

A estrutura representa a organização conceitual descrita no vídeo. Os nomes dos demais projetos seguem a solução construída nas aulas anteriores.

---

## 5. Organizando a pasta Requests e o builder

Dentro de `CommonTestUtilities`, o instrutor cria uma pasta chamada `Requests`.

O propósito é agrupar geradores de objetos de requisição em um local de fácil acesso.

Depois, ele cria uma classe com o nome da requisição seguido do sufixo **`Builder`**:

```text
CommonTestUtilities
└── Requests
    └── RequestRegisterExpenseBuilder.cs
```

Em termos de nomenclatura, o builder fica relacionado ao contrato que ele constrói:

| Elemento | Exemplo |
|---|---|
| Contrato da API | `RequestRegisterExpenseJson` |
| Classe auxiliar de teste | `RequestRegisterExpenseBuilder` |
| Método que entrega a instância | `Build()` |

O instrutor muda a visibilidade da classe para `public`, pois ela deverá ser acessada por outros projetos.

### Estrutura inicial

```csharp
public class RequestRegisterExpenseBuilder
{
    public RequestRegisterExpenseJson Build()
    {
        // Aqui será montada a requisição.
    }
}
```

É uma classe destinada à **preparação dos dados do teste**. Não implementa regras de negócio de cadastro.

---

## 6. Referências entre projetos

Para declarar que o método `Build()` retorna uma `RequestRegisterExpenseJson`, o projeto `CommonTestUtilities` precisa conhecer esse tipo.

Na arquitetura do CashFlow, a requisição está na camada **Communication**.

O instrutor adiciona, portanto, uma referência de projeto:

```text
CommonTestUtilities
         │
         └──────────► CashFlow.Communication
                               │
                               └── RequestRegisterExpenseJson
```

No Visual Studio, isso é feito em **Dependencies → Add Project Reference**, selecionando **Communication**.

Após adicionar a referência, o `using` do namespace da requisição pode ser incluído.

**Distinção importante:**

- A referência **CommonTestUtilities → Communication** é configurada nesta aula.
- As referências dos projetos que consumirão o builder poderão ser usadas quando ele for incorporado aos testes. O vídeo ainda não demonstra essa integração.

---

## 7. Instalando o pacote Bogus

A aula apresenta um novo pacote NuGet: **Bogus**.

Bogus é uma biblioteca utilizada para **gerar dados falsos e preencher objetos com informações plausíveis**, útil para cenários de testes.

### Instalação demonstrada

No projeto `CommonTestUtilities`:

1. Clicar com o botão direito no projeto.
2. Escolher **Manage NuGet Packages** (*Gerenciar Pacotes NuGet*).
3. Acessar a aba **Browse** (*Procurar*).
4. Pesquisar por `Bogus`.
5. Selecionar o pacote apresentado.
6. Clicar em **Install** (*Instalar*).
7. Confirmar a instalação, se solicitado.

O instrutor mostra que a instalação foi concluída e depois consulta a documentação do pacote.

### Por que instalar em CommonTestUtilities?

Porque é **nesse projeto** que a lógica de geração de dados será implementada.

Assim, os futuros projetos de teste podem utilizar os builders da biblioteca comum, em vez de repetir todo o código de geração.

```text
CommonTestUtilities
     │
     ├── Referência: Communication
     │
     └── Pacote NuGet: Bogus
              │
              └── Faker<T>
```

A transcrição não informa a versão do Bogus instalada; portanto, não foi atribuída uma versão específica a esta aula.

---

## 8. O que a biblioteca Bogus oferece

Na documentação, o instrutor mostra várias categorias de geração de valores falsos.

Entre os exemplos citados estão:

| Categoria | Exemplos de dados |
|---|---|
| Pessoa | Nome e telefone |
| Internet | E-mail e nome de usuário |
| Endereço | Cidade, rua e código postal |
| Comércio | Nome de produto, descrição, departamento e preço |
| Datas | Datas passadas e futuras |
| Números | Inteiros, decimais, pares e ímpares |
| Empresas | Dados relacionados a companhias |
| Finanças | Informações do domínio financeiro |
| Outros | Valores relacionados a veículos e outras categorias |

### Localidades (*locales*)

O instrutor destaca que a biblioteca oferece suporte a diferentes localidades.

Isso influencia a forma dos valores produzidos por geradores de dados relacionados a idioma, nomes, endereços e outros padrões regionais, quando a categoria suporta esses dados.

Na aula, essa funcionalidade é apresentada como **capacidade do pacote**; o builder de despesas não recebe a configuração de uma cultura específica.

### Qual é a ideia prática?

Em vez de escolher manualmente todos os valores:

```csharp
Title = "Apple";
Description = "Descrição da despesa";
```

podemos solicitar valores apropriados à categoria:

```csharp
faker.Commerce.ProductName()
faker.Commerce.ProductDescription()
```

Assim, diferentes execuções podem produzir entradas distintas.

---

## 9. Gerando dados com uma instância Faker

O instrutor primeiro mostra a possibilidade de criar uma instância simples de `Faker`.

Exemplo conceitual:

```csharp
using Bogus;

var faker = new Faker();

var title = faker.Commerce.ProductName();
var description = faker.Commerce.ProductDescription();
var date = faker.Date.Past();
```

Essa abordagem permite obter valores individualmente e atribuí-los às propriedades de uma requisição.

### Exemplo de construção manual com dados gerados

```csharp
var faker = new Faker();

var request = new RequestRegisterExpenseJson
{
    Title = faker.Commerce.ProductName(),
    Description = faker.Commerce.ProductDescription(),
    Date = faker.Date.Past()
};
```

Esse trecho é **ilustrativo e parcial**: não preenche os demais campos da requisição.

A ideia é demonstrar a primeira forma de utilizar o Bogus explicada pelo professor.

Embora funcione para obter dados, o instrutor decide utilizar uma **segunda forma, com sintaxe fluente**, para definir a geração de todas as propriedades em uma estrutura única.

---

## 10. Faker<T> e a configuração fluente com RuleFor

A segunda abordagem apresentada é utilizar:

```csharp
new Faker<RequestRegisterExpenseJson>()
```

O tipo genérico `T` indica **qual classe queremos gerar**.

Neste caso:

```csharp
Faker<RequestRegisterExpenseJson>
```

significa que o objetivo é preparar um gerador de instâncias de `RequestRegisterExpenseJson`.

### Definindo o valor de uma propriedade

O Bogus permite utilizar `RuleFor`:

```csharp
new Faker<RequestRegisterExpenseJson>()
    .RuleFor(
        request => request.Title,
        faker => faker.Commerce.ProductName()
    );
```

A chamada possui dois elementos fundamentais:

| Expressão | Significado |
|---|---|
| `request => request.Title` | Identifica qual propriedade do objeto gerado deve ser preenchida. |
| `faker => faker.Commerce.ProductName()` | Define como obter o valor que será colocado nessa propriedade. |

### Por que a sintaxe é chamada de fluente?

Porque podemos encadear várias chamadas `RuleFor` para configurar, de forma legível, como cada propriedade será preenchida.

```csharp
new Faker<RequestRegisterExpenseJson>()
    .RuleFor(r => r.Title, f => f.Commerce.ProductName())
    .RuleFor(r => r.Description, f => f.Commerce.ProductDescription());
```

Aqui aparecem **expressões lambda**, como já estudado em aulas anteriores.

O `r` representa a instância da requisição que está sendo configurada; o `f` disponibiliza os geradores do Bogus.

---

## 11. Preenchendo os campos de uma despesa

O instrutor configura cinco propriedades principais da requisição de cadastro.

### 11.1 Title — Nome de produto

```csharp
.RuleFor(r => r.Title, f => f.Commerce.ProductName())
```

O título passa a receber o nome de um produto fictício.

### 11.2 Description — Descrição de produto

```csharp
.RuleFor(r => r.Description, f => f.Commerce.ProductDescription())
```

A descrição é preenchida com um texto gerado pelo Bogus.

### 11.3 Date — Data no passado

```csharp
.RuleFor(r => r.Date, f => f.Date.Past())
```

Essa escolha se relaciona à regra do validator que impede datas futuras.

No teste da aula 75, essa condição foi atendida com `DateTime.Now.AddDays(-1)`. Agora, a biblioteca passa a gerar a data.

### 11.4 PaymentType — Valor do enum

```csharp
.RuleFor(r => r.PaymentType, f => f.PickRandom<PaymentType>())
```

O tipo de pagamento está representado pelo enum `PaymentType`.

O método genérico `PickRandom<PaymentType>()` seleciona aleatoriamente um dos valores definidos nesse enum.

O instrutor também comenta que existem sobrecargas para escolher opções específicas ou excluir opções do sorteio. **Nesta aula, ele não utiliza exclusão.**

### 11.5 Amount — Valor decimal em um intervalo

```csharp
.RuleFor(r => r.Amount, f => f.Random.Decimal(min: 1, max: 1000))
```

A propriedade `Amount` é do tipo `decimal`.

O instrutor utiliza `Random.Decimal` e mostra como restringir o intervalo usando os parâmetros nomeados `min` e `max`.

No exemplo, o valor gerado fica entre **1 e 1000**, conforme a proposta do vídeo.

Esse limite inferior é útil porque o cadastro de despesas exige um valor maior que zero.

### Tabela de mapeamento

| Propriedade | Gerador Bogus | Motivo da escolha |
|---|---|---|
| `Title` | `Commerce.ProductName()` | Criar um título não fixo. |
| `Description` | `Commerce.ProductDescription()` | Criar uma descrição não fixa. |
| `Date` | `Date.Past()` | Evitar datas futuras. |
| `PaymentType` | `PickRandom<PaymentType>()` | Selecionar um valor existente no enum. |
| `Amount` | `Random.Decimal(min: 1, max: 1000)` | Garantir valores positivos no intervalo definido. |

---

## 12. Montando o método Build

Após configurar o `Faker<RequestRegisterExpenseJson>`, o professor mantém a abordagem fluente e coloca a construção da requisição dentro de `Build()`.

Um exemplo didático completo, coerente com as regras mostradas no vídeo, é:

```csharp
using Bogus;
// Adicione os namespaces de RequestRegisterExpenseJson e PaymentType
// conforme a estrutura do projeto CashFlow.

public class RequestRegisterExpenseBuilder
{
    public RequestRegisterExpenseJson Build()
    {
        return new Faker<RequestRegisterExpenseJson>()
            .RuleFor(r => r.Title, f => f.Commerce.ProductName())
            .RuleFor(r => r.Description,
                f => f.Commerce.ProductDescription())
            .RuleFor(r => r.Date, f => f.Date.Past())
            .RuleFor(r => r.PaymentType,
                f => f.PickRandom<PaymentType>())
            .RuleFor(r => r.Amount,
                f => f.Random.Decimal(min: 1, max: 1000))
            .Generate();
    }
}
```

**Observação sobre o exemplo:** a transcrição descreve o retorno da expressão fluente, mas não registra de modo inequívoco a chamada final utilizada na tela para obter o objeto. Aqui, `.Generate()` foi explicitado como etapa para **gerar uma instância do tipo configurado**, conforme a API do Bogus. O código é uma reconstrução didática, não uma cópia literal do editor do instrutor.

### O que o Build deve entregar?

```text
RequestRegisterExpenseBuilder
             │
             ▼
          Build()
             │
             ▼
Faker<RequestRegisterExpenseJson>
             │
         RuleFor...
             │
             ▼
         Generate()
             │
             ▼
RequestRegisterExpenseJson preenchida
```

Assim, quem precisa de uma requisição não precisa configurar manualmente todos os campos dentro de cada método de teste.

---

## 13. Como a estrutura poderá ser reutilizada nos testes

O objetivo de criar o builder dentro de uma biblioteca compartilhada é reutilizá-lo em mais de um projeto de teste.

Um uso esperado **em uma aula posterior**, depois de adicionar a referência de projeto apropriada, será semelhante a:

```csharp
var request = new RequestRegisterExpenseBuilder().Build();
```

Em vez de:

```csharp
var request = new RequestRegisterExpenseJson
{
    Title = "Apple",
    Description = "Descrição da despesa",
    Amount = 100,
    Date = DateTime.Now.AddDays(-1),
    PaymentType = PaymentType.CreditCard
};
```

### Benefício para a organização AAA

O teste poderá continuar seguindo o padrão:

```text
ARRANGE
 ├── Instanciar validator
 └── Obter uma request pelo builder compartilhado

ACT
 └── Executar Validate(request)

ASSERT
 └── Verificar o resultado esperado
```

**Importante:** o vídeo ainda não demonstra essa substituição nem executa o teste com os dados gerados pelo Bogus. Esse é o próximo passo anunciado.

---

## 14. Diferença entre os RuleFor do Bogus e do FluentValidation

O instrutor observa que a sintaxe fluente do Bogus lembra a sintaxe já estudada em **FluentValidation**.

Isso é uma semelhança de **forma**, mas não de **responsabilidade**.

| | Bogus | FluentValidation |
|---|---|---|
| Objetivo | Gerar valores e preencher objetos para testes. | Verificar se os valores atendem a regras. |
| Tipo de objeto usado | `Faker<T>` | `AbstractValidator<T>` |
| Uso de `RuleFor` | Define **como gerar** a propriedade. | Define **como validar** a propriedade. |
| Exemplo | `RuleFor(r => r.Amount, f => f.Random.Decimal(...))` | `RuleFor(r => r.Amount).GreaterThan(0)` |
| Resultado | Instância preenchida de `T`. | Resultado de validação e eventuais erros. |

### Relação entre os dois pacotes

```text
Bogus
 └── gera RequestRegisterExpenseJson
                  │
                  ▼
FluentValidation
 └── valida RequestRegisterExpenseJson
                  │
                  ▼
xUnit
 └── verifica resultado esperado
```

O **Bogus não substitui o validator**. Ele ajuda a preparar dados de entrada para os testes que exercitarão esse validator.

---

## 15. Cuidados práticos com dados aleatórios

Esta seção explicita implicações técnicas do tipo de teste apresentado, para facilitar a revisão sem confundir a aula com uma discussão avançada de testes.

### 15.1 Dados aleatórios ainda precisam respeitar o cenário

Um teste que espera **sucesso** deve gerar propriedades válidas.

O instrutor já cuida disso ao:

- escolher uma data no passado;
- gerar um valor decimal positivo;
- selecionar um valor declarado no enum.

Se o gerador passar a produzir uma propriedade inválida, o teste de sucesso poderá falhar mesmo sem alteração no validator.

### 15.2 Variação de dados não é o mesmo que cobertura de casos

Gerar uma requisição diferente a cada execução pode ampliar a variedade das entradas, mas **não garante que todas as condições importantes serão testadas**.

Casos como título vazio, valor zero e data futura precisam de testes específicos quando o objetivo for verificar essas regras.

### 15.3 Cuidado com falhas difíceis de reproduzir

Quando um teste depende de dados aleatórios sem controle, uma falha pode ocorrer apenas com determinado valor gerado.

Em suítes que exigem reprodutibilidade, é útil poder registrar ou fixar a geração dos dados para repetir exatamente o cenário problemático.

**Nota:** a transcrição não introduz configuração de *seed* nem técnicas avançadas de reprodutibilidade. São cuidados adicionais para a aplicação prática do conceito.

### 15.4 Bibliotecas auxiliares devem continuar na área de testes

`CommonTestUtilities` é uma infraestrutura de apoio aos testes e não é descrita como parte da lógica de produção da API.

Sua finalidade é melhorar a manutenção dos cenários de teste.

---

## 16. O que fica para a próxima aula

Ao final, o instrutor informa que vai **testar a implementação na aula seguinte**.

Até aqui, o trabalho demonstrado foi:

- identificar os dados hard-coded do teste anterior;
- criar `CommonTestUtilities`;
- adicionar a pasta `Requests`;
- criar `RequestRegisterExpenseBuilder`;
- adicionar a referência à Communication;
- instalar Bogus;
- conhecer as formas de gerar dados;
- configurar o builder com `Faker<T>` e `RuleFor`.

**Ainda não foi demonstrada a execução dos testes unitários utilizando o builder.**

---

## 17. Resumo

1. A aula 76 refatora a preparação dos dados do primeiro teste unitário.
2. O código anterior tinha dados fixos e repetia uma responsabilidade que futuramente será compartilhada.
3. Uma **Class Library** chamada `CommonTestUtilities` é criada dentro de `tests`.
4. Dentro dela, uma pasta `Requests` organiza os builders de objetos de requisição.
5. `RequestRegisterExpenseBuilder` concentra a geração de `RequestRegisterExpenseJson`.
6. A biblioteca compartilhada recebe uma referência ao projeto **Communication**.
7. O pacote **Bogus** é instalado via NuGet.
8. Uma instância simples de `Faker` pode gerar valores individualmente.
9. A abordagem escolhida pelo instrutor é `Faker<RequestRegisterExpenseJson>` com encadeamento de `RuleFor`.
10. São configurados título, descrição, data passada, enum de pagamento e valor decimal entre 1 e 1000.
11. O `RuleFor` do **Bogus** configura a geração de dados; o do **FluentValidation** configura regras de validação.
12. A integração com o teste anterior e a execução ficam para **a próxima aula**.

---

## 18. Visão geral

```text
ANTES — AULA 75
────────────────────────────────
Validators.Test
└── RegisterExpenseValidatorTests
    └── Success()
        └── Request manual, com valores fixos

              │
              ▼

MELHORIA — AULA 76
────────────────────────────────
tests/
├── Validators.Test
│
└── CommonTestUtilities
    ├── referência: Communication
    ├── pacote NuGet: Bogus
    └── Requests
        └── RequestRegisterExpenseBuilder
             └── Build()
                 └── Faker<RequestRegisterExpenseJson>
                      ├── ProductName → Title
                      ├── ProductDescription → Description
                      ├── Date.Past → Date
                      ├── PickRandom<PaymentType> → PaymentType
                      └── Random.Decimal(1,1000) → Amount
                               │
                               ▼
                    Request gerada para testes

              │
              ▼

PRÓXIMA AULA
────────────────────────────────
Reutilizar a request gerada nos testes
e verificar a execução do conjunto
```

**Conclusão:** a principal aprendizagem é que os dados utilizados pelos testes também merecem organização e reutilização. A aula introduz **Bogus**, mostra como gerar propriedades de uma requisição com sintaxe fluente e prepara uma biblioteca compartilhada que poderá simplificar a escrita dos próximos testes do CashFlow.
