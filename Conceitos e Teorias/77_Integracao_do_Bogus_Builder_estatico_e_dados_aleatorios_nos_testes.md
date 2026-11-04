# Integrando o Bogus aos testes: Request Builder estático e dados aleatórios

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [Retomando a estrutura criada na aula anterior](#2-retomando-a-estrutura-criada-na-aula-anterior)
3. [Referenciando CommonTestUtilities no projeto de testes](#3-referenciando-commontestutilities-no-projeto-de-testes)
4. [Por que não instanciar o builder?](#4-por-que-não-instanciar-o-builder)
5. [Transformando Build em um método static](#5-transformando-build-em-um-método-static)
6. [Chamando o builder no Arrange](#6-chamando-o-builder-no-arrange)
7. [Exemplo de implementação integrado](#7-exemplo-de-implementação-integrado)
8. [Depurando o primeiro teste com dados gerados](#8-depurando-o-primeiro-teste-com-dados-gerados)
9. [Executando novamente e comparando os resultados](#9-executando-novamente-e-comparando-os-resultados)
10. [Aleatoriedade: por que alguns valores podem se repetir?](#10-aleatoriedade-por-que-alguns-valores-podem-se-repetir)
11. [Separação de responsabilidades entre builder e teste](#11-separação-de-responsabilidades-entre-builder-e-teste)
12. [Cuidados técnicos e limites do que foi demonstrado](#12-cuidados-técnicos-e-limites-do-que-foi-demonstrado)
13. [O que vem na próxima aula](#13-o-que-vem-na-próxima-aula)
14. [Resumo](#14-resumo)
15. [Visão geral](#15-visão-geral)

---

## 1. Objetivo da aula

Esta aula continua **exatamente de onde a aula 76 terminou**.

Na aula anterior, criamos uma biblioteca compartilhada chamada `CommonTestUtilities` e implementamos um builder para gerar requisições de cadastro de despesas com dados falsos (*fake data*) usando a biblioteca **Bogus**.

Agora o instrutor quer **comprovar o funcionamento dessa geração dentro do teste unitário criado na aula 75**.

Para isso, são realizadas duas mudanças:

1. Adicionar uma referência de `CommonTestUtilities` ao projeto que contém o teste do validator.
2. Alterar o método `Build()` para `static`, permitindo chamá-lo diretamente pela classe, sem instanciar um builder.

Depois, o teste é depurado duas vezes para demonstrar que o Bogus produz requisições com valores variados.

**Escopo da aula:** integração e verificação do gerador já criado. O vídeo não apresenta novas regras de validação, nem novos testes de casos inválidos.

---

## 2. Retomando a estrutura criada na aula anterior

Na aula 76, foi introduzida uma classe auxiliar para centralizar a criação de `RequestRegisterExpenseJson`.

O propósito é compartilhar a lógica de preparação de dados entre os testes de validators e, futuramente, os testes de use cases.

### Estrutura conceitual

```text
CashFlow
│
├── src
│   ├── CashFlow.Application
│   │   └── RegisterExpenseValidator
│   │
│   └── CashFlow.Communication
│       └── RequestRegisterExpenseJson
│
└── tests
    ├── Validators.Test
    │   └── Expenses
    │       └── Register
    │           └── RegisterExpenseValidatorTests
    │
    └── CommonTestUtilities
        └── Requests
            └── RequestRegisterExpenseBuilder
```

A classe `RequestRegisterExpenseBuilder` já tinha o propósito de construir uma request utilizando o Bogus, definindo valores para propriedades como `Title`, `Description`, `Date`, `PaymentType` e `Amount`.

O ponto pendente era **utilizá-la no teste real**.

---

## 3. Referenciando CommonTestUtilities no projeto de testes

A primeira ação feita no Visual Studio é adicionar uma referência à biblioteca compartilhada.

O projeto de testes onde está `RegisterExpenseValidatorTests` precisa acessar a classe `RequestRegisterExpenseBuilder`.

### No Visual Studio

1. Localizar o projeto de testes de validators.
2. Abrir a área **Dependencies** (*Dependências*).
3. Adicionar uma **Project Reference** (*Referência de Projeto*).
4. Selecionar `CommonTestUtilities`.
5. Confirmar a alteração.

### Dependências após a integração

```text
Validators.Test
      │
      ├─────────► Application
      │             └── RegisterExpenseValidator
      │
      ├─────────► Communication
      │             └── RequestRegisterExpenseJson
      │
      └─────────► CommonTestUtilities
                    └── RequestRegisterExpenseBuilder
                                 │
                                 └── usa Bogus
```

**Por que isso é necessário?**

Porque um projeto de C# não pode simplesmente usar qualquer classe existente em outra biblioteca sem que a dependência correspondente esteja disponível para ele.

A referência permite acessar o builder a partir dos testes, respeitando também os modificadores de acesso públicos da classe e do método.

---

## 4. Por que não instanciar o builder?

O instrutor primeiro mostra como seria uma chamada comum a um método de instância.

Se `Build()` fosse um método **não estático**, seria necessário criar um objeto da classe auxiliar antes de chamar a função:

```csharp
var builder = new RequestRegisterExpenseBuilder();

var request = builder.Build();
```

A chamada funciona como conceito, mas introduz uma instância adicional cujo único objetivo seria acessar um método que não depende de estado armazenado no builder.

O instrutor considera desnecessário criar essa instância para o caso específico do vídeo.

### O que ele propõe?

Declarar o método como **estático**:

```csharp
public static RequestRegisterExpenseJson Build()
```

Assim, a chamada deixa de depender de `new RequestRegisterExpenseBuilder()`.

**Motivação apresentada na aula:** `Build()` atua como uma função independente, responsável por devolver uma requisição gerada. Portanto, pode ser acessada diretamente pelo nome da classe.

---

## 5. Transformando Build em um método static

Em C#, um membro `static` pertence ao **tipo**, e não a uma instância específica desse tipo.

Na prática, isso permite:

```csharp
RequestRegisterExpenseBuilder.Build();
```

em vez de:

```csharp
new RequestRegisterExpenseBuilder().Build();
```

### Comparação

| Método de instância | Método estático |
|---|---|
| Declarado com `public RequestRegisterExpenseJson Build()` | Declarado com `public static RequestRegisterExpenseJson Build()` |
| Normalmente chamado por um objeto | Chamado diretamente pelo nome da classe |
| `builder.Build()` | `RequestRegisterExpenseBuilder.Build()` |
| Precisa de uma instância do builder | Não precisa criar uma instância do builder |

### Observação importante

Marcar `Build()` como `static` **não significa que a requisição gerada seja estática ou compartilhada**.

A função é estática, mas seu corpo pode criar **um novo `Faker<T>` e uma nova request a cada chamada**.

Essa distinção é fundamental para entender por que o teste continua recebendo dados gerados em novas execuções.

```text
Método estático
RequestRegisterExpenseBuilder.Build()
               │
               ├── chamada 1 → nova request
               │
               └── chamada 2 → outra request
```

---

## 6. Chamando o builder no Arrange

Antes da mudança, o teste criava uma requisição preenchendo manualmente suas propriedades.

De forma ilustrativa:

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

O novo formato substitui toda essa montagem pela chamada ao builder:

```csharp
var request = RequestRegisterExpenseBuilder.Build();
```

### Vantagens imediatas

- Menos código de preparação dentro do método de teste.
- Valores de entrada gerados pelo Bogus em vez de preenchidos manualmente.
- Centralização da construção da request.
- Possibilidade de reutilizar a mesma estratégia de geração em futuros testes.

O teste continua organizado pelo padrão **AAA — Arrange, Act, Assert**:

```text
ARRANGE
  ├── Criar RegisterExpenseValidator
  └── Solicitar RequestRegisterExpenseJson ao builder
                │
                ▼
ACT
  └── validator.Validate(request)
                │
                ▼
ASSERT
  └── Assert.True(result.IsValid)
```

A alteração é principalmente na fase de **Arrange**. O comportamento que o teste verifica continua sendo o sucesso da validação.

---

## 7. Exemplo de implementação integrado

A transcrição descreve as alterações, mas não fornece uma cópia textual completa dos arquivos. Os trechos abaixo são **reconstruções didáticas coerentes com o procedimento da aula**, e não transcrições literais do editor do Visual Studio.

### 7.1 Builder com método estático

```csharp
using Bogus;

// Acrescente os namespaces dos tipos usados, conforme sua solução.

public class RequestRegisterExpenseBuilder
{
    public static RequestRegisterExpenseJson Build()
    {
        return new Faker<RequestRegisterExpenseJson>()
            .RuleFor(r => r.Title,
                faker => faker.Commerce.ProductName())
            .RuleFor(r => r.Description,
                faker => faker.Commerce.ProductDescription())
            .RuleFor(r => r.Date,
                faker => faker.Date.Past())
            .RuleFor(r => r.PaymentType,
                faker => faker.PickRandom<PaymentType>())
            .RuleFor(r => r.Amount,
                faker => faker.Random.Decimal(min: 1, max: 1000))
            .Generate();
    }
}
```

O uso de `.Generate()` deixa explícita a geração de uma instância do tipo configurado. O detalhe da expressão de retorno não é reproduzido literalmente pela transcrição.

### 7.2 Teste de sucesso com o builder

```csharp
using Xunit;

// Acrescente os namespaces dos tipos usados, conforme sua solução.

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
        Assert.True(result.IsValid);
    }
}
```

O método de teste mantém exatamente a intenção introduzida na aula 75: **a request deve ser válida**.

O que muda é a origem dos dados. Agora ela é gerada pelo builder compartilhado, que utiliza Bogus.

---

## 8. Depurando o primeiro teste com dados gerados

Com a referência ao projeto compartilhado e a mudança para `static`, o instrutor começa os testes no Visual Studio.

### Etapas demonstradas

1. Clicar com o botão direito no método de teste.
2. Escolher **Debug Test**.
3. A execução para no **breakpoint** configurado.
4. Avançar a execução com **F10**.
5. Inspecionar o objeto `request` gerado.
6. Continuar até a validação e a asserção.

### Valores observados na primeira execução

Na demonstração, a request gerada apresentou dados como:

| Campo | Exemplo observado |
|---|---|
| `Amount` | `372,59` |
| `Date` | Uma data no passado, citada como 2 de outubro |
| `PaymentType` | `Cash` (dinheiro) |
| `Title` | `Handmade Soft Tuna` |
| `Description` | Uma descrição também gerada pela biblioteca |

Esses valores **não são parâmetros fixados no código do teste**. São exemplos da saída de uma execução específica do Bogus.

Após inspecionar a requisição, o instrutor continua a execução:

```text
Builder gera dados
        │
        ▼
Request preenchida
        │
        ▼
validator.Validate(request)
        │
        ▼
result.IsValid == true
        │
        ▼
Assert.True(...) passa
```

A demonstração termina com o teste aprovado.

---

## 9. Executando novamente e comparando os resultados

O professor executa e depura o **mesmo teste uma segunda vez**.

A estrutura do teste não é modificada entre as duas execuções.

No entanto, os valores da request agora são diferentes.

### Comparação entre as duas execuções demonstradas

| Campo | Primeira execução | Segunda execução |
|---|---|---|
| `Title` | `Handmade Soft Tuna` | Um título relacionado a pizza |
| `PaymentType` | `Cash` | `CreditCard` |
| `Date` | Data passada | Outra data passada |
| `Description` | Texto gerado | Texto diferente do anterior |
| `Amount` | Exemplo `372,59` | O vídeo não especifica numericamente o segundo valor |

O instrutor inspeciona o resultado, continua a execução e constata que o teste segue funcionando.

### O que isso comprova?

Que, nessa implementação, a chamada ao builder está obtendo dados gerados e que **execuções sucessivas podem produzir requests diferentes sem mudar o código do teste**.

A variação é resultado da configuração do Bogus realizada na aula anterior.

### Ideia geral

```text
1ª execução
    │
    ▼
RequestRegisterExpenseBuilder.Build()
    │
    ▼
{ Title = "Handmade Soft Tuna",
  PaymentType = Cash,
  ... }
    │
    ▼
Teste passa


2ª execução
    │
    ▼
RequestRegisterExpenseBuilder.Build()
    │
    ▼
{ Title = "Pizza ...",
  PaymentType = CreditCard,
  ... }
    │
    ▼
Teste passa
```

Os valores representados são exemplos do comportamento observado na transcrição.

---

## 10. Aleatoriedade: por que alguns valores podem se repetir?

A aula faz uma ressalva importante: a geração é **aleatória**, então algumas propriedades **podem repetir valores em chamadas consecutivas**.

O exemplo principal é o campo `PaymentType`, que recebe uma opção de um enum.

### Enum com poucas opções

Quando o conjunto de possibilidades é pequeno, como um enum de tipos de pagamento, não é surpreendente que o mesmo valor seja escolhido em várias execuções.

Por exemplo:

```text
Execução 1 → Cash
Execução 2 → Cash
Execução 3 → CreditCard
Execução 4 → Cash
```

Isso não indica necessariamente um problema no Bogus. A repetição é compatível com um sorteio.

### Textos e números

O instrutor observa que campos com muitas opções possíveis, como nomes de produto, descrições e números em um intervalo amplo, **tendem a variar mais**.

É importante formular essa ideia corretamente: ter muitas possibilidades **diminui a probabilidade de repetição, mas não a torna impossível**.

### O que deve ser lembrado?

**Gerar dados aleatórios não garante valores únicos a cada execução.**

Na prática, o objetivo deste exemplo é fornecer dados variados **dentro dos limites configurados**.

Se algum teste exigir unicidade, será necessário definir isso explicitamente; esse tipo de requisito não é implementado nesta aula.

---

## 11. Separação de responsabilidades entre builder e teste

Com a alteração, cada parte tem um objetivo claro.

| Elemento | Responsabilidade |
|---|---|
| `CommonTestUtilities` | Compartilhar utilitários necessários aos projetos de testes. |
| `RequestRegisterExpenseBuilder` | Concentrar a construção de requests de teste. |
| `Bogus` | Gerar valores fictícios para as propriedades. |
| `RegisterExpenseValidator` | Aplicar as regras de validação à requisição. |
| `RegisterExpenseValidatorTests` | Executar o comportamento e verificar o resultado esperado. |
| `Assert.True` | Confirmar que `IsValid` é verdadeiro no cenário de sucesso. |

Essa separação permite que a preparação dos dados cresça de forma mais organizada.

### Antes

```text
Teste
  ├── Cria validator
  ├── Define manualmente todos os valores
  ├── Valida
  └── Verifica resultado
```

### Depois

```text
Teste
  ├── Cria validator
  ├── Chama builder compartilhado
  │       └── Bogus preenche a request
  ├── Valida
  └── Verifica resultado
```

O teste fica mais **conciso**, mas continua deixando evidente aquilo que pretende verificar.

---

## 12. Cuidados técnicos e limites do que foi demonstrado

### 12.1 Um método estático não torna o resultado permanente

A alteração para `static` modifica a **forma de acesso ao método**, não obriga o método a devolver sempre o mesmo objeto.

A criação de novos valores acontece porque o código interno do builder usa o Bogus para montar uma request.

### 12.2 Dados válidos devem continuar válidos

O teste foi criado para esperar sucesso.

Por isso, ao gerar dados, o builder precisa preservar as condições do validator, como valor positivo, data permitida, título preenchido e enum válido.

Se o builder passar a produzir uma entrada inválida, o teste de sucesso poderá falhar.

### 12.3 Aleatoriedade não é sinônimo de cobertura completa

A aula comprova a variação de valores entre execuções.

Entretanto, **não demonstra cobertura completa de casos-limite**. Casos como título vazio e valor igual a zero continuam exigindo cenários específicos de teste, como já foi discutido na introdução aos testes.

### 12.4 Variação não significa garantia de não repetição

Mesmo campos com um intervalo amplo podem, por acaso, assumir valores iguais em mais de uma execução.

A observação importante é que **as requisições são geradas novamente e os dados podem variar**, não que serão sempre diferentes.

### 12.5 Testes aleatórios podem exigir reprodutibilidade

Como observação complementar de engenharia de testes, quando uma falha depende de uma sequência aleatória, pode ser útil controlar ou registrar os dados utilizados para conseguir reproduzi-la.

**Essa técnica não é ensinada neste vídeo**; aqui a finalidade é apenas demonstrar que o Bogus está integrado e produzindo valores.

---

## 13. O que vem na próxima aula

Ao concluir, o instrutor anuncia **mais uma melhoria utilizando sintaxe fluente**.

A transcrição não detalha qual implementação será feita, portanto o resumo não antecipa código nem atribui uma funcionalidade específica ao próximo vídeo.

O estado da solução ao final desta aula é:

- `Validators.Test` referencia `CommonTestUtilities`;
- `Build()` pode ser chamado diretamente pela classe por ser `static`;
- o teste utiliza a request gerada pelo builder;
- a execução com dados variados foi verificada no depurador;
- a asserção continua passando nas duas execuções demonstradas.

---

## 14. Resumo

- Esta aula é uma **continuação direta da aula 76**, dedicada ao Bogus.
- O projeto `Validators.Test` passa a referenciar `CommonTestUtilities`.
- O método `Build()` é transformado em **`static`** para dispensar a criação de uma instância de `RequestRegisterExpenseBuilder`.
- O teste passa a obter a requisição usando `RequestRegisterExpenseBuilder.Build()`.
- O padrão **AAA** permanece: criar validator e request, executar `Validate`, verificar `IsValid`.
- O professor usa **Debug Test**, breakpoints e **F10** para inspecionar a requisição.
- Na primeira execução são observados dados como `Amount = 372,59`, `PaymentType = Cash` e o título `Handmade Soft Tuna`.
- Na segunda execução aparecem outros valores, incluindo `PaymentType = CreditCard` e um título relacionado a pizza.
- Os testes demonstrados continuam passando.
- Um enum pode repetir o mesmo valor em diferentes execuções, pois há um conjunto limitado de possibilidades.
- Textos e números com muitos resultados possíveis tendem a variar, mas **nenhum sorteio garante unicidade**.
- A próxima aula trará **outra melhoria com sintaxe fluente**, sem detalhamento antecipado nesta transcrição.

---

## 15. Visão geral

```text
             AULA 76
       CommonTestUtilities
                  │
                  ▼
      RequestRegisterExpenseBuilder
       + Bogus / Faker<T> / RuleFor
                  │
                  ▼
              AULA 77
                  │
       Adiciona referência ao
         Validators.Test
                  │
                  ▼
       Torna Build() static
                  │
                  ▼
    RequestRegisterExpenseBuilder
              .Build()
                  │
                  ▼
       Gera request de teste
                  │
         ┌────────┴────────┐
         ▼                 ▼
     Execução 1        Execução 2
     Cash              CreditCard
     Título A          Título B
     Data A            Data B
         │                 │
         └────────┬────────┘
                  ▼
       validator.Validate(...)
                  │
                  ▼
      Assert.True(result.IsValid)
                  │
                  ▼
        Testes demonstrados
             APROVADOS
```

**Conclusão:** a aula conecta a biblioteca de dados falsos à suíte de testes, simplifica a API do builder com `static` e verifica no Visual Studio que diferentes execuções podem gerar requisições variadas sem perder a validade do cenário de sucesso.
