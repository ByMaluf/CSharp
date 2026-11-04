# Concluindo os testes de erro do validator com xUnit Theory e InlineData

> **Aula 81 — Testes parametrizados, validações negativas e Test Explorer**
>
> Continuação direta da aula 80: nela criamos o teste de título vazio. Agora o instrutor cobre **data futura**, **tipo de pagamento inválido** e **valor zero ou negativo**, além de aprimorar o próprio teste do título com **[Theory] e [InlineData]**.

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [Revisão das regras do RegisterExpenseValidator](#2-revisão-das-regras-do-registerexpensevalidator)
3. [Estratégia de testes negativos e padrão AAA](#3-estratégia-de-testes-negativos-e-padrão-aaa)
4. [Primeiro cenário: data no futuro](#4-primeiro-cenário-data-no-futuro)
5. [Segundo cenário: tipo de pagamento inválido](#5-segundo-cenário-tipo-de-pagamento-inválido)
6. [Terceiro cenário: valores zero e negativos](#6-terceiro-cenário-valores-zero-e-negativos)
7. [Fact versus Theory: qual é a diferença?](#7-fact-versus-theory-qual-é-a-diferença)
8. [InlineData: passando diferentes dados ao mesmo teste](#8-inlinedata-passando-diferentes-dados-ao-mesmo-teste)
9. [Aplicação prática: Theory para o valor da despesa](#9-aplicação-prática-theory-para-o-valor-da-despesa)
10. [Ordem de execução não deve ser presumida](#10-ordem-de-execução-não-deve-ser-presumida)
11. [Aprimorando o teste do título com dados parametrizados](#11-aprimorando-o-teste-do-título-com-dados-parametrizados)
12. [Por que string.Empty não é usado em InlineData?](#12-por-que-stringempty-não-é-usado-em-inlinedata)
13. [Exemplos completos dos testes](#13-exemplos-completos-dos-testes)
14. [Entendendo os dez casos mostrados no Test Explorer](#14-entendendo-os-dez-casos-mostrados-no-test-explorer)
15. [Test Explorer: executar, depurar e limpar resultados](#15-test-explorer-executar-depurar-e-limpar-resultados)
16. [Independência entre testes e execução paralela](#16-independência-entre-testes-e-execução-paralela)
17. [Observações técnicas para evitar confusões](#17-observações-técnicas-para-evitar-confusões)
18. [Resumo](#18-resumo)
19. [Visão geral](#19-visão-geral)

---

## 1. Objetivo da aula

Nas aulas anteriores, montamos a infraestrutura para testes unitários com **xUnit**, geramos requisições válidas com **Bogus** e aprendemos a conferir resultados usando **Fluent Assertions**.

Na aula 80, começamos a testar o **caminho de erro**: uma requisição cujo título é vazio deve ser rejeitada pelo `RegisterExpenseValidator`.

O objetivo agora é concluir a cobertura das regras de validação apresentadas no curso, incluindo:

- **Data futura:** o cadastro de despesa não deve aceitar uma data posterior ao limite permitido.
- **Tipo de pagamento inválido:** a propriedade precisa representar um valor definido em `PaymentType`.
- **Valor da despesa inválido:** tanto zero quanto valores negativos devem ser rejeitados.
- **Título obrigatório:** testar não só a string vazia, mas também espaços em branco e `null`.

A principal novidade é o uso de **`[Theory]`** e **`[InlineData]`** para executar o mesmo método de teste com valores distintos, sem duplicar a implementação.

No final da demonstração, o instrutor apresenta **dez casos de teste aprovados** no Test Explorer.

---

## 2. Revisão das regras do RegisterExpenseValidator

As regras do validator haviam sido implementadas com **FluentValidation**.

Um resumo conceitual da configuração estudada anteriormente é:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage(ResourceErrorMessages.TITLE_REQUIRED);

RuleFor(expense => expense.Amount)
    .GreaterThan(0)
    .WithMessage(ResourceErrorMessages.AMOUNT_GREATER_THAN_ZERO);

RuleFor(expense => expense.Date)
    .LessThanOrEqualTo(DateTime.UtcNow)
    .WithMessage(ResourceErrorMessages.EXPENSE_CANNOT_BE_IN_FUTURE);

RuleFor(expense => expense.PaymentType)
    .IsInEnum()
    .WithMessage(ResourceErrorMessages.PAYMENT_TYPE_INVALID);
```

O trecho acima é **uma reconstrução resumida com base nas aulas anteriores**. Ele destaca a regra de cada propriedade e as chaves de recurso utilizadas nos testes.

| Propriedade | Regra | Exemplo de entrada inválida |
|---|---|---|
| `Title` | `NotEmpty()` | `""`, `"   "`, `null` |
| `Amount` | `GreaterThan(0)` | `0`, `-1`, `-2`, `-7` |
| `Date` | Não ultrapassar a data/instante permitido | Amanhã |
| `PaymentType` | `IsInEnum()` | `(PaymentType)700` |

**Ponto-chave:** cada teste desta aula modifica propositalmente **uma propriedade da requisição**, de modo que o erro recebido corresponda à regra analisada.

---

## 3. Estratégia de testes negativos e padrão AAA

O professor reutiliza a estrutura que já funcionou no teste do título:

```text
ARRANGE
  Instanciar o validator
  Gerar uma request válida com o builder
  Forçar o valor inválido que queremos testar
             |
             v
ACT
  validator.Validate(request)
             |
             v
ASSERT
  result.IsValid deve ser false
  result.Errors deve conter exatamente 1 erro
  Esse erro deve ter a mensagem esperada
```

### Por que começar com uma requisição válida?

Porque o builder criado com Bogus fornece valores normalmente aceitos para as outras propriedades.

Isso reduz a possibilidade de a validação falhar por um motivo **diferente** do cenário que estamos estudando.

### Três verificações complementares

```csharp
result.IsValid.Should().BeFalse();

result.Errors.Should()
    .ContainSingle()
    .And.Contain(error =>
        error.ErrorMessage.Equals(mensagemEsperada));
```

Cada asserção tem uma finalidade:

1. `BeFalse()`: a requisição foi rejeitada?
2. `ContainSingle()`: houve exatamente um erro?
3. `Contain(...)`: a mensagem desse erro é a que esperávamos?

Na aula 80, aprendemos também que **`ContainSingle()` sem argumento** é diferente de **`ContainSingle(predicate)`**, que conta somente os elementos que satisfazem a expressão lambda.

---

## 4. Primeiro cenário: data no futuro

A aula começa pelos testes de **data** e **tipo de pagamento**, deixando o valor para apresentar a novidade do `Theory`.

### Como produzir uma data inválida?

O instrutor usa a data atual e acrescenta um dia.

```csharp
request.Date = DateTime.UtcNow.AddDays(1);
```

A operação `AddDays(1)` devolve um novo `DateTime`, representando um instante **um dia à frente**.

### Por que isso gera erro?

A regra do validator não permite despesas com data futura.

Assim, após executar:

```csharp
var result = validator.Validate(request);
```

o resultado esperado é:

```text
IsValid = false
Errors.Count = 1
ErrorMessage = EXPENSE_CANNOT_BE_IN_FUTURE
```

### Teste de exemplo

```csharp
[Fact]
public void Error_Date_Future()
{
    // Arrange
    var validator = new RegisterExpenseValidator();
    var request = RequestRegisterExpenseBuilder.Build();

    request.Date = DateTime.UtcNow.AddDays(1);

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should()
        .ContainSingle()
        .And.Contain(error =>
            error.ErrorMessage.Equals(
                ResourceErrorMessages.EXPENSE_CANNOT_BE_IN_FUTURE));
}
```

O instrutor executa o teste e obtém o indicador **verde** no Visual Studio.

> **Nota técnica:** a transcrição fala em usar a data de hoje e adicionar um dia. O exemplo utiliza `UtcNow` por coerência com a regra em `DateTime.UtcNow` estudada anteriormente. O importante para este cenário é garantir um instante inequivocamente futuro.

---

## 5. Segundo cenário: tipo de pagamento inválido

O validator utiliza a regra:

```csharp
.IsInEnum()
```

Ela exige que o valor de `PaymentType` corresponda a um dos membros declarados no enum.

### Por que não basta atribuir 700 diretamente?

A propriedade foi declarada com o tipo `PaymentType`, não `int`.

Por isso, a atribuição direta:

```csharp
request.PaymentType = 700;
```

não é, em geral, uma conversão implícita permitida, pois `700` não é uma constante zero conversível implicitamente para esse enum.

### Como forçar um valor de enum inválido?

O instrutor utiliza um **cast explícito**:

```csharp
request.PaymentType = (PaymentType)700;
```

Isso faz a propriedade carregar o valor numérico `700` como `PaymentType`.

**Detalhe importante:** em C#, é possível representar em uma variável de tipo enum um valor numérico que **não foi declarado** entre os membros desse enum. O cast pode ser aceito pelo compilador sem que o valor se torne um membro válido.

É justamente essa situação que a regra `IsInEnum()` pretende rejeitar.

### Teste de exemplo

```csharp
[Fact]
public void Error_PaymentType_Invalid()
{
    // Arrange
    var validator = new RegisterExpenseValidator();
    var request = RequestRegisterExpenseBuilder.Build();

    request.PaymentType = (PaymentType)700;

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should()
        .ContainSingle()
        .And.Contain(error =>
            error.ErrorMessage.Equals(
                ResourceErrorMessages.PAYMENT_TYPE_INVALID));
}
```

Durante a depuração, o professor observa o valor `700`, a mensagem de **tipo de pagamento inválido** e a conclusão bem-sucedida do teste.

---

## 6. Terceiro cenário: valores zero e negativos

A terceira propriedade é `Amount`.

O validator exige:

```csharp
RuleFor(expense => expense.Amount)
    .GreaterThan(0);
```

Ou seja, **somente valores maiores que zero** atendem à regra.

### Exemplos que devem falhar

```text
Amount = 0   → inválido
Amount = -1  → inválido
Amount = -2  → inválido
Amount = -7  → inválido
```

Em todos esses casos, a expectativa é a mesma:

- `IsValid == false`;
- exatamente um erro;
- mensagem `AMOUNT_GREATER_THAN_ZERO`.

### Qual seria o problema de duplicar testes?

Poderíamos escrever métodos distintos:

```text
Error_Amount_Zero()
Error_Amount_MinusOne()
Error_Amount_MinusTwo()
Error_Amount_MinusSeven()
```

Mas esses métodos teriam praticamente o mesmo corpo, mudando somente o valor atribuído a `request.Amount`.

Essa duplicação dificultaria a manutenção sem oferecer uma vantagem para os cenários demonstrados.

### Solução proposta

Criar **um método de teste parametrizado**, cuja entrada muda, mas cujo resultado esperado permanece igual.

É aqui que o instrutor apresenta os atributos **`[Theory]`** e **`[InlineData]`**.

---

## 7. Fact versus Theory: qual é a diferença?

O `xUnit` permite marcar métodos de teste com atributos diferentes, dependendo do tipo de cenário.

### `[Fact]`: caso específico, sem entradas externas parametrizadas

```csharp
[Fact]
public void Success()
{
    // Um caso de teste.
}
```

O `Fact` representa um teste cujo método **não recebe argumentos fornecidos por linhas de dados**.

Isso **não** significa que o teste só poderá ser executado uma única vez na vida do projeto; significa que há um caso de teste não parametrizado por linhas de dados.

### `[Theory]`: mesmo comportamento, diferentes entradas

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public void Error_Amount_Invalid(decimal amount)
{
    // Mesmo teste executado com valores diferentes.
}
```

No `Theory`, os parâmetros são fornecidos por uma **fonte de dados**, que nesta aula será `InlineData`.

### Comparação

| Aspecto | `[Fact]` | `[Theory]` |
|---|---|---|
| Modelo | Um caso sem dados fornecidos por atributos | Teste orientado a conjuntos de dados |
| Parâmetros fornecidos por `InlineData` | Não | Sim |
| Corpo do teste | Executado para aquele caso | Aplicado às diferentes entradas |
| Exemplo na aula | Data futura, pagamento inválido | Vários valores de Amount e Title |
| Papel do xUnit | Descobrir e executar o teste | Descobrir e executar os casos de dados |

**Ideia-chave:** usamos `Theory` quando vários valores de entrada devem obedecer à **mesma regra e à mesma expectativa de teste**.

---

## 8. InlineData: passando diferentes dados ao mesmo teste

O atributo `[InlineData]` especifica os valores fornecidos a uma execução do `Theory`.

Na demonstração do valor da despesa, o professor define:

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-2)]
[InlineData(-7)]
public void Error_Amount_Invalid(decimal amount)
{
}
```

### Como interpretar

Cada atributo `InlineData` representa uma entrada do teste:

| Caso | `amount` |
|---:|---:|
| 1 | `0` |
| 2 | `-1` |
| 3 | `-2` |
| 4 | `-7` |

O método de teste é **o mesmo**, mas recebe um valor diferente em cada caso descoberto/executado.

### O valor recebido é utilizado no Arrange

```csharp
request.Amount = amount;
```

Assim, não é necessário manter quatro cópias de uma rotina idêntica.

### E se um dia a regra mudar?

Como o corpo é compartilhado, uma alteração na lógica de preparação ou verificação pode ser feita em um único método.

Se for necessário acrescentar mais um cenário, é possível adicionar outra linha de dados.

---

## 9. Aplicação prática: Theory para o valor da despesa

O teste do valor segue as mesmas três etapas já estudadas.

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-2)]
[InlineData(-7)]
public void Error_Amount_Invalid(decimal amount)
{
    // Arrange
    var validator = new RegisterExpenseValidator();
    var request = RequestRegisterExpenseBuilder.Build();

    request.Amount = amount;

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should()
        .ContainSingle()
        .And.Contain(error =>
            error.ErrorMessage.Equals(
                ResourceErrorMessages.AMOUNT_GREATER_THAN_ZERO));
}
```

**Observação sobre tipagem:** os valores `0`, `-1`, `-2` e `-7` são literais inteiros nos atributos `InlineData`. O exemplo pressupõe a conversão desses argumentos para o parâmetro `decimal` realizada na execução do caso pelo xUnit, conforme demonstrado na aula. Caso uma versão/configuração do executor não aceite a conversão desejada, uma fonte tipada como `MemberData` / `TheoryData<decimal>` é alternativa. Não é possível escrever diretamente literais `decimal` como argumentos de atributo em C#.

### Um erro demonstrado durante a implementação

Na primeira execução, o instrutor esquece de substituir a mensagem de erro do cenário copiado.

O teste falha, e ele identifica que a expectativa estava relacionada à propriedade errada.

Depois de corrigir a mensagem para `AMOUNT_GREATER_THAN_ZERO`, os quatro casos são executados e passam.

A lição é que **ao reutilizar código de testes, precisamos revisar não apenas a entrada, mas também a expectativa**.

---

## 10. Ordem de execução não deve ser presumida

Um ponto enfatizado pelo instrutor: a ordem em que escrevemos os `InlineData` **não é um contrato sobre a ordem de execução**.

Os atributos aparecem assim:

```csharp
[InlineData(0)]
[InlineData(-1)]
[InlineData(-2)]
[InlineData(-7)]
```

Mas, na demonstração, o primeiro valor recebido pelo breakpoint foi `-1`, não `0`.

Outras execuções passam por `0`, `-7` e `-2`.

### Por que não devemos depender dessa ordem?

Porque a infraestrutura de descoberta, execução e agendamento dos testes pode apresentar os casos em outra sequência.

Um teste correto deve produzir o mesmo resultado **independentemente de qual outro teste foi executado antes**.

```text
Teoria com quatro casos
   ├── Amount = 0
   ├── Amount = -1
   ├── Amount = -2
   └── Amount = -7

Todos devem ser independentes
e verificar a mesma regra.
```

**A recomendação da aula é não presumir uma sequência garantida**, nem entre diferentes funções de teste, nem entre as entradas do `Theory`.

---

## 11. Aprimorando o teste do título com dados parametrizados

Depois do `Amount`, o professor retorna ao teste criado na aula 80.

Antes, o método verificava somente:

```csharp
request.Title = string.Empty;
```

Porém, a regra `NotEmpty()` deve tratar outras entradas inválidas, como:

- string vazia;
- espaços em branco;
- `null`.

Como o comportamento esperado é o mesmo, o instrutor altera o método de `Fact` para `Theory`.

### Versão parametrizada

```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
[InlineData(null)]
public void Error_Title_Empty(string? title)
{
    // Arrange
    var validator = new RegisterExpenseValidator();
    var request = RequestRegisterExpenseBuilder.Build();

    request.Title = title!;

    // Act
    var result = validator.Validate(request);

    // Assert
    result.IsValid.Should().BeFalse();

    result.Errors.Should()
        .ContainSingle()
        .And.Contain(error =>
            error.ErrorMessage.Equals(
                ResourceErrorMessages.TITLE_REQUIRED));
}
```

**Observação técnica sobre o exemplo:** `string?` representa corretamente que o conjunto de dados inclui `null`. O operador `!` na atribuição suprime um eventual aviso do compilador quando a propriedade `Title` é declarada como não anulável; ele **não impede que `null` seja atribuído em tempo de execução**. O objetivo do teste é justamente verificar uma entrada inválida. Na configuração do projeto do instrutor, a anotação de nulidade pode ser diferente.

### Três casos cobertos

| Valor fornecido | Situação | Resultado esperado |
|---|---|---|
| `""` | String vazia | Rejeitada |
| `"   "` | Somente espaços | Rejeitada |
| `null` | Ausência de valor | Rejeitada |

O professor executa os três casos e observa que todos passam.

---

## 12. Por que string.Empty não é usado em InlineData?

Na transcrição, o instrutor tenta passar `string.Empty` diretamente ao atributo `InlineData` e o compilador apresenta um erro.

### O motivo técnico

Argumentos usados em atributos C# precisam atender às regras de **valores permitidos em argumentos de atributo**, normalmente constantes de tempo de compilação e outros formatos especificamente admitidos.

A expressão `string.Empty` é um campo estático somente leitura, **não uma constante de compilação para uso nessa posição**.

Por isso:

```csharp
// Não é permitido como argumento de atributo:
[InlineData(string.Empty)]
```

Já a string literal vazia é permitida:

```csharp
[InlineData("")]
```

Da mesma forma, podemos passar `null` e um literal contendo espaços em branco:

```csharp
[InlineData(null)]
[InlineData("   ")]
```

### A diferença em linguagem simples

```text
string.Empty
    → Campo estático da classe string
    → Não é constante válida para atributo

""
    → Literal de string
    → Aceito pelo atributo InlineData
```

Esse é um detalhe de **sintaxe e restrições dos atributos em C#**, não uma limitação da regra `NotEmpty()` do FluentValidation.

---

## 13. Exemplos completos dos testes

O exemplo abaixo reúne os métodos abordados na aula. É uma **reconstrução didática baseada na transcrição e nas classes utilizadas nas aulas anteriores**; os namespaces do projeto devem ser adicionados conforme a estrutura real da solução.

O código mantém **Fluent Assertions**, porque essa é a sintaxe usada nos testes desta gravação, mesmo após a aula 79 ter apresentado o Shouldly como alternativa.

```csharp
using FluentAssertions;
using Xunit;

// Imports de RegisterExpenseValidator, PaymentType,
// RequestRegisterExpenseBuilder e ResourceErrorMessages.

public class RegisterExpenseValidatorTests
{
    [Fact]
    public void Success()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Error_Title_Empty(string? title)
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        request.Title = title!;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .ContainSingle()
            .And.Contain(error =>
                error.ErrorMessage.Equals(
                    ResourceErrorMessages.TITLE_REQUIRED));
    }

    [Fact]
    public void Error_Date_Future()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        request.Date = DateTime.UtcNow.AddDays(1);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .ContainSingle()
            .And.Contain(error =>
                error.ErrorMessage.Equals(
                    ResourceErrorMessages.EXPENSE_CANNOT_BE_IN_FUTURE));
    }

    [Fact]
    public void Error_PaymentType_Invalid()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        request.PaymentType = (PaymentType)700;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .ContainSingle()
            .And.Contain(error =>
                error.ErrorMessage.Equals(
                    ResourceErrorMessages.PAYMENT_TYPE_INVALID));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(-7)]
    public void Error_Amount_Invalid(decimal amount)
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        request.Amount = amount;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();

        result.Errors.Should()
            .ContainSingle()
            .And.Contain(error =>
                error.ErrorMessage.Equals(
                    ResourceErrorMessages.AMOUNT_GREATER_THAN_ZERO));
    }
}
```

### O que é demonstrado nesse conjunto?

- **Sucesso:** uma requisição válida é aceita.
- **Título:** três entradas inválidas são rejeitadas.
- **Data:** uma data futura é rejeitada.
- **Pagamento:** um valor fora do enum é rejeitado.
- **Valor:** quatro entradas não positivas são rejeitadas.

O importante é manter **um resultado esperado claro para cada cenário**.

---

## 14. Entendendo os dez casos mostrados no Test Explorer

Uma das dúvidas possíveis ao observar o Test Explorer é:

> Se existem apenas cinco métodos de teste, por que aparecem dez testes?

Isso acontece porque `Theory` permite criar **múltiplos casos a partir de um único método**.

A contagem final mostrada no vídeo corresponde à soma:

| Método | Atributo | Quantidade de casos |
|---|---|---:|
| `Success` | `[Fact]` | 1 |
| `Error_Date_Future` | `[Fact]` | 1 |
| `Error_PaymentType_Invalid` | `[Fact]` | 1 |
| `Error_Amount_Invalid` | `[Theory]` com quatro `InlineData` | 4 |
| `Error_Title_Empty` | `[Theory]` com três `InlineData` | 3 |
| **Total** | | **10** |

### Contagem

```text
1 sucesso
+ 1 erro de data
+ 1 erro de pagamento
+ 4 erros de Amount
+ 3 erros de Title
= 10 casos de teste
```

É exatamente essa contagem que o professor observa no Visual Studio ao final.

O indicador verde significa que **cada caso foi concluído com sucesso** naquele momento.

---

## 15. Test Explorer: executar, depurar e limpar resultados

A aula reforça algumas funcionalidades do **Test Explorer**, apresentado na aula 74.

### Executar os testes

O professor demonstra a opção de **executar todos os testes** do projeto.

O Test Explorer apresenta o conjunto de casos e o resultado de cada um.

### Depurar um teste

É possível executar em modo de depuração para parar nos breakpoints e inspecionar:

- o argumento recebido pelo `Theory`;
- a request depois de alterar a propriedade;
- o resultado de `Validate`;
- a lista `Errors`.

### Limpar os resultados

O instrutor demonstra **Clear Test Results**.

Isso limpa os estados exibidos e faz os indicadores voltarem ao estado de **não executado**, representado em azul no ambiente mostrado.

Essa ação não exclui métodos nem arquivos: apenas limpa os resultados visuais da execução.

### Informações apresentadas

O Test Explorer pode mostrar também:

- quantidade total de casos;
- testes aprovados e reprovados;
- tempo de execução;
- identificação de quais entradas do `Theory` produziram falhas.

O professor conclui a aula mostrando **10 testes executados e 10 aprovados**.

---

## 16. Independência entre testes e execução paralela

Ao final, o instrutor reforça dois cuidados:

1. **Não depender da ordem de execução dos testes.**
2. **Manter cada teste independente dos demais.**

### Por que a independência é importante?

Imagine um teste que altera um valor compartilhado e outro teste que só funciona depois que essa alteração aconteceu.

O segundo deixa de verificar a própria regra isoladamente e passa a depender do estado de execução de outro teste.

Isso fragiliza a suíte, especialmente quando testes são executados em paralelo ou em ordem diferente.

### O papel do padrão AAA

Cada método da aula possui seu próprio:

- **Arrange:** instancia o validator e gera uma request.
- **Act:** chama `Validate`.
- **Assert:** verifica a expectativa daquele cenário.

Essa organização ajuda a evitar a dependência entre casos.

### Paralelismo: leitura tecnicamente precisa

O instrutor descreve que diferentes classes de testes podem executar em paralelo, enquanto os métodos de uma mesma classe são executados de forma sequencial.

No **xUnit**, a configuração usual permite paralelismo entre **coleções de testes** diferentes; classes geralmente formam coleções próprias por padrão, mas isso pode mudar conforme a configuração de coleções e paralelização do projeto.

Portanto, a regra prática é:

> **Não presuma ordem nem simultaneidade específicas. Faça os testes funcionarem independentemente de como o executor agendar cada caso.**

### E o construtor de uma classe de teste?

O professor recomenda evitar colocar no construtor da classe preparações compartilhadas que tornem testes dependentes entre si.

A intenção é correta: **evitar estado mutável compartilhado e dependências de execução**.

Entretanto, há uma nuance importante em xUnit: usar o construtor para **preparação por teste** é uma prática suportada e comum. Normalmente, o xUnit cria uma nova instância da classe para cada caso, de modo que a inicialização no construtor pode ser isolada. O problema surge quando se compartilha estado de forma inadequada ou se perde a clareza da preparação, não simplesmente por existir código no construtor.

Nesta aula, a abordagem escolhida foi manter o **Arrange explícito em cada método**, como ensinado anteriormente.

---

## 17. Observações técnicas para evitar confusões

### 17.1 Data futura e comparação correta

O teste faz a data avançar um dia para garantir que a regra falhe.

Nas aulas anteriores, a validação foi apresentada como uma comparação com o instante atual por meio de `LessThanOrEqualTo(DateTime.UtcNow)`.

O sentido técnico da condição é: **data de despesa menor ou igual ao limite atual é permitida; data posterior ao limite é inválida**. O comentário oral transcrito no início sobre “maior ou igual” não deve ser usado para inverter essa regra.

### 17.2 Cast de enum não valida o número

```csharp
(PaymentType)700
```

pode produzir um valor de `PaymentType` mesmo que o enum não declare `700`.

É a **regra de validação `IsInEnum()`**, e não o cast, que deve identificar a situação inválida.

### 17.3 Theory descreve vários casos, não várias regras distintas

O `Theory` de `Amount` continua verificando uma única regra: valor maior que zero.

O que varia é **a entrada**, não o critério de aprovação.

### 17.4 Ordem de InlineData não deve ser utilizada para controlar outros testes

Não utilize as entradas do `Theory` como etapas de uma sequência na qual um caso prepara estado para o seguinte.

Cada entrada deve funcionar sozinha.

### 17.5 A cultura do Resource não é objeto desta aula

As mensagens esperadas são consultadas por meio de `ResourceErrorMessages`, como na aula 80.

A gravação não apresenta verificações separadas de `pt-BR`, `pt-PT` ou outras culturas. Esse é outro objetivo de teste.

### 17.6 O código permanece no estilo Fluent Assertions

A aula 79 apresentou o Shouldly como alternativa diante do licenciamento do Fluent Assertions.

Mesmo assim, as asserções explicadas nesta sequência continuam com a sintaxe:

```csharp
result.Errors.Should().ContainSingle().And.Contain(...);
```

O resumo preserva o estilo **efetivamente utilizado nesta gravação**, sem assumir que a migração para Shouldly já foi aplicada.

### 17.7 Cuidado com dados aleatórios do Bogus

A aula parte do pressuposto de que o builder gera **uma request válida**.

Se o gerador criar outra propriedade inválida por acaso ou por alteração futura nas regras, `ContainSingle()` poderá falhar mesmo que a regra testada esteja correta.

Isso reforça a necessidade de manter dados-base válidos e previsíveis para cada cenário.

---

## 18. Resumo

- A aula conclui os cenários de erro apresentados para o `RegisterExpenseValidator`.
- O teste de **data futura** usa `DateTime` com `AddDays(1)`.
- O teste de **pagamento inválido** usa o cast `(PaymentType)700`.
- Ambos verificam `IsValid == false`, um único erro e a mensagem esperada.
- O campo **Amount** precisa rejeitar `0` e valores negativos.
- Como esses valores seguem a **mesma regra**, o professor apresenta **`[Theory]`**.
- Cada **`[InlineData]`** fornece uma entrada do mesmo teste parametrizado.
- Para Amount, são usados `0`, `-1`, `-2` e `-7`.
- O método de título é convertido para `Theory`, recebendo `""`, `"   "` e `null`.
- `string.Empty` não é usado diretamente no atributo por não ser um argumento de atributo constante válido.
- Os casos não devem depender da ordem de execução.
- O professor mostra como executar, depurar e limpar resultados no Test Explorer.
- Ao final, são contabilizados **10 casos de teste, todos aprovados**.
- O padrão **Arrange, Act, Assert** continua orientando cada teste.
- A independência entre testes é fundamental, inclusive quando o executor utiliza paralelismo.
- As mensagens continuam sendo verificadas por `ResourceErrorMessages`, sem testar idiomas específicos nesta aula.

---

## 19. Visão geral

```text
                  AULA 81
                     │
          Concluir testes de erro
                     │
          ┌──────────┼──────────────┐
          ▼          ▼              ▼
      Data futura  PaymentType     Amount
       [Fact]       [Fact]        [Theory]
          │          │              │
          │          │     ┌────────┼──────────┐
          │          │     ▼        ▼          ▼
          │          │   0 e -1    -2         -7
          │          │              │
          │          └──────────────┤
          │                         │
          ▼                         ▼
     IsValid = false       IsValid = false
      um erro               um erro
      mensagem correta      mensagem correta

             + Título obrigatório
                     │
                  [Theory]
                     │
             ┌───────┼──────────┐
             ▼       ▼          ▼
            ""      "   "       null
             │       │          │
             └───────┼──────────┘
                     ▼
             IsValid = false
             um erro de título
                     │
                     ▼
                Test Explorer
                     │
        1 sucesso + 1 data + 1 pagamento
            + 4 Amount + 3 Title
                     │
                     ▼
              10 TESTES APROVADOS
```

**Conclusão:** a maior novidade desta aula não é escrever mais regras de validação, mas aprender a **testar a mesma regra contra diferentes entradas usando `Theory` e `InlineData`**, preservando o padrão AAA, conferindo as mensagens esperadas e mantendo os testes independentes.
