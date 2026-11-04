# Primeiro teste unitário com xUnit: Fact, padrão AAA, Assert.True e depuração

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [A importância da organização dos testes](#2-a-importância-da-organização-dos-testes)
3. [Estruturando as pastas e a classe de testes](#3-estruturando-as-pastas-e-a-classe-de-testes)
4. [Nomeando os métodos conforme o resultado esperado](#4-nomeando-os-métodos-conforme-o-resultado-esperado)
5. [O atributo Fact do xUnit](#5-o-atributo-fact-do-xunit)
6. [Padrão AAA: Arrange, Act e Assert](#6-padrão-aaa-arrange-act-e-assert)
7. [Referências entre o projeto de testes e as demais camadas](#7-referências-entre-o-projeto-de-testes-e-as-demais-camadas)
8. [Arrange: preparando validator e request](#8-arrange-preparando-validator-e-request)
9. [Act: executando a validação](#9-act-executando-a-validação)
10. [Assert: verificando IsValid](#10-assert-verificando-isvalid)
11. [Exemplo completo do primeiro teste](#11-exemplo-completo-do-primeiro-teste)
12. [Executando e depurando o teste no Visual Studio](#12-executando-e-depurando-o-teste-no-visual-studio)
13. [Forçando uma falha e entendendo o resultado vermelho](#13-forçando-uma-falha-e-entendendo-o-resultado-vermelho)
14. [Próxima melhoria: geração de dados de teste](#14-próxima-melhoria-geração-de-dados-de-teste)
15. [Observações técnicas e cuidados](#15-observações-técnicas-e-cuidados)
16. [Resumo](#16-resumo)
17. [Visão geral](#17-visão-geral)

---

## 1. Objetivo da aula

Na aula anterior, criamos um projeto de testes com **xUnit** e conhecemos o **Test Explorer**. Nesta aula, escrevemos nosso **primeiro teste unitário de verdade**.

O objetivo é verificar um cenário de sucesso da classe `RegisterExpenseValidator`: ao receber uma requisição de cadastro de despesa com informações válidas, o resultado da validação deve indicar `IsValid == true`.

Ao longo do processo, aprendemos também a:

- organizar arquivos e métodos de testes;
- marcar um método como teste usando `[Fact]`;
- adotar a estrutura **Arrange → Act → Assert**;
- adicionar as referências necessárias aos outros projetos da solução;
- usar `Assert.True(...)`;
- executar e depurar testes com breakpoints;
- observar a falha deliberada de um teste.

O foco principal da aula é **um único teste do cenário de sucesso**, não a cobertura de todas as validações.

---

## 2. A importância da organização dos testes

O instrutor começa destacando duas formas de organização:

1. **Organização física:** pastas e classes com nomes que deixam claro o componente testado.
2. **Organização lógica:** cada método de teste deve ter um objetivo bem definido.

A recomendação é **não misturar comportamentos diferentes em um único teste**.

Por exemplo, um método que tenta verificar simultaneamente o sucesso de um cadastro e de uma atualização torna mais difícil identificar qual comportamento apresentou defeito.

É melhor ter verificações separadas:

```text
Testes de cadastro
   └── Cenário de sucesso

Testes de atualização
   └── Cenário de sucesso

Testes de validação
   ├── Título ausente
   ├── Valor inválido
   └── Tipo de pagamento inválido
```

Cada cenário deve ter um nome que represente o comportamento esperado.

Essa organização facilita entender o resultado do Test Explorer quando algum teste falhar.

---

## 3. Estruturando as pastas e a classe de testes

A aula usa o projeto de testes de validators criado anteriormente e procura reproduzir uma estrutura semelhante à existente na camada **Application**.

### Estrutura sugerida pela demonstração

```text
CashFlow (Solution)
├── src
│   └── CashFlow.Application
│       └── UseCases
│           └── Expenses
│               └── Register
│                   └── RegisterExpenseValidator.cs
└── tests
    └── Validators.Test
        └── Expenses
            └── Register
                └── RegisterExpenseValidatorTests.cs
```

No Visual Studio, a sequência é:

1. Expandir o projeto de testes e, em paralelo, localizar o validator na Application.
2. Criar a pasta `Expenses` dentro de `Validators.Test`.
3. Criar a subpasta `Register`.
4. Adicionar a classe `RegisterExpenseValidatorTests`.
5. Alterar o modificador de acesso da classe para `public`, como feito na aula.
6. Remover `using` não utilizados, quando necessário.

O nome da classe de teste utiliza o nome do componente testado acrescido de `Tests`.

```csharp
public class RegisterExpenseValidatorTests
{
}
```

A lógica de organização da aula é simples: **encontrar os testes de uma funcionalidade deve ser tão fácil quanto encontrar seu código de produção**.

---

## 4. Nomeando os métodos conforme o resultado esperado

Cada método de teste deve comunicar o resultado esperado.

No primeiro teste, o instrutor escolhe um nome relacionado ao **sucesso** da validação:

```csharp
public void Success()
{
}
```

Por enquanto, esse é apenas um método comum.

Em outros cenários, métodos separados poderão representar erros específicos, como título obrigatório ou tipo de pagamento inválido.

### Por que isso importa?

Imagine uma falha no Test Explorer:

```text
RegisterExpenseValidatorTests
    └── InvalidPaymentType  ❌
```

O nome já orienta onde investigar.

**Nota de nomenclatura:** `Success` reproduz a intenção do exemplo da aula. Em projetos maiores, nomes ainda mais descritivos, como `ShouldAcceptValidExpense`, podem tornar o comportamento esperado mais explícito.

---

## 5. O atributo Fact do xUnit

Em xUnit, para que o método seja reconhecido como teste, o instrutor adiciona o atributo `[Fact]`.

```csharp
using Xunit;

public class RegisterExpenseValidatorTests
{
    [Fact]
    public void Success()
    {
    }
}
```

### O que muda ao adicionar `[Fact]`?

O método passa a ser descoberto pelo framework de testes como um caso de teste.

O instrutor demonstra que o Visual Studio exibe um indicador ao lado do método. Esse indicador permite executar ou depurar o teste.

Durante a demonstração:

- o indicador azul representa um teste ainda não executado;
- o verde representa a execução bem-sucedida;
- o vermelho representa uma falha.

### E os métodos auxiliares?

O instrutor antecipa que futuramente poderão existir **métodos auxiliares**, inclusive privados, para ajudar a preparar os testes.

Esses métodos não precisam ser marcados como `[Fact]` quando **não são casos de teste**.

> Nesta aula, `[Fact]` é o atributo utilizado para identificar o método de teste. Não é necessário conhecer ainda outras modalidades de teste.

---

## 6. Padrão AAA: Arrange, Act e Assert

A principal técnica apresentada na aula é dividir o teste em três etapas, conhecidas como **AAA**.

| Etapa | Significado | Pergunta que responde |
|---|---|---|
| **Arrange** | Preparar os dados e objetos necessários | *De que preciso antes de testar?* |
| **Act** | Executar a operação testada | *Qual ação quero verificar?* |
| **Assert** | Conferir o resultado obtido | *O resultado corresponde ao esperado?* |

### Esqueleto visual

```csharp
[Fact]
public void Success()
{
    // Arrange
    // Criar e configurar os objetos necessários.

    // Act
    // Chamar a operação que queremos testar.

    // Assert
    // Verificar se o resultado corresponde ao esperado.
}
```

### Fluxo de um teste

```text
           ARRANGE
        Criar validator
        Criar request válida
               │
               ▼
             ACT
       validator.Validate(...)
               │
               ▼
            ASSERT
      Assert.True(result.IsValid)
               │
          ┌────┴────┐
          ▼         ▼
         true      false
          │         │
       Aprovado   Reprovado
```

Os três comentários ajudam a aprender a estrutura. Seu papel é separar visualmente responsabilidades dentro do método de teste.

---

## 7. Referências entre o projeto de testes e as demais camadas

Antes de instanciar os objetos, é necessário que o projeto de testes consiga acessar as classes utilizadas.

O `RegisterExpenseValidator` está definido no projeto **Application**.

A classe que representa a requisição de cadastro está no projeto **Communication**.

Por isso o instrutor adiciona duas referências ao projeto de testes:

| Referência do projeto `Validators.Test` | Por quê? |
|---|---|
| **Application** | Instanciar e executar `RegisterExpenseValidator`. |
| **Communication** | Criar o objeto de requisição enviado ao validator. |

No Visual Studio, a ação é feita a partir de **Dependencies → Add Project Reference**, selecionando os projetos necessários.

### Dependências conceituais

```text
Validators.Test
     │
     ├──────────► Application
     │                 └── RegisterExpenseValidator
     │
     └──────────► Communication
                       └── RequestRegisterExpenseJson
```

O teste utiliza as classes existentes na aplicação, sem precisar recriar sua lógica.

---

## 8. Arrange: preparando validator e request

Na primeira etapa, o instrutor cria as instâncias necessárias.

### Instanciando o validator

```csharp
var validator = new RegisterExpenseValidator();
```

Essa é a unidade cujo comportamento será verificado.

### Criando uma requisição válida

Depois, ele prepara um objeto de requisição de cadastro de despesa.

A demonstração utiliza valores como:

- valor da despesa igual a **100**;
- título preenchido, com exemplo **`Apple`**;
- descrição preenchida;
- data no passado;
- forma de pagamento válida, como cartão de crédito.

Um exemplo equivalente ao cenário da aula é:

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

**Observação:** o código acima organiza os valores apresentados na transcrição; nomes de namespaces e eventuais detalhes de implementação dependem das classes existentes na solução.

### Por que usar `AddDays(-1)`?

O instrutor inicialmente pensa em usar a data atual, mas altera para **um dia anterior**.

```csharp
Date = DateTime.Now.AddDays(-1)
```

A justificativa é garantir que a data não esteja no futuro, evitando que a regra de validação de datas impeça o cenário de sucesso.

No final do **Arrange**, as duas instâncias necessárias estão prontas:

```text
validator  → RegisterExpenseValidator
request    → RequestRegisterExpenseJson válido
```

---

## 9. Act: executando a validação

O **Act** representa a operação cuja resposta será analisada.

Neste caso, executar o validator:

```csharp
var result = validator.Validate(request);
```

O método recebe a requisição criada no Arrange e devolve o resultado da validação.

### O que queremos observar?

A propriedade `IsValid` informa se as regras de validação foram atendidas.

```text
validator.Validate(request)
           │
           ▼
       result
           │
           └── IsValid
               ├── true
               └── false
```

Nesta aula, o foco é verificar o caminho **válido**.

---

## 10. Assert: verificando IsValid

Na última etapa, comparamos o resultado obtido com aquilo que o teste espera.

Como a requisição foi preparada com valores válidos, queremos que:

```csharp
result.IsValid == true
```

### A sintaxe do xUnit

O instrutor apresenta o método:

```csharp
Assert.True(result.IsValid);
```

Isso significa:

> O teste somente será aprovado se `result.IsValid` for verdadeiro.

Se `result.IsValid` for `false`, a asserção falha e o teste é marcado como reprovado.

### `Assert` não é um `if` comum

Em um teste, usamos mecanismos específicos do framework para expressar a expectativa de forma verificável e gerar um resultado que o Test Explorer consegue interpretar.

Durante a aula, o instrutor também mostra que o xUnit oferece muitas outras asserções para comparação e verificação de valores.

Uma forma oposta mencionada é:

```csharp
Assert.False(result.IsValid);
```

Ela seria útil **em um teste que espera uma validação inválida**, assunto para testes de erro em aulas posteriores.

---

## 11. Exemplo completo do primeiro teste

Reunindo os três passos que aparecem na explicação, temos:

```csharp
using Xunit;

// Imports dos namespaces que contêm as classes da solução.

public class RegisterExpenseValidatorTests
{
    [Fact]
    public void Success()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();

        var request = new RequestRegisterExpenseJson
        {
            Title = "Apple",
            Description = "Descrição da despesa",
            Amount = 100,
            Date = DateTime.Now.AddDays(-1),
            PaymentType = PaymentType.CreditCard
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
```

**Este é um exemplo didático baseado no fluxo da transcrição**, não uma cópia literal do código mostrado em tela. Ele pressupõe os namespaces apropriados e as referências à Application e à Communication descritas anteriormente.

### Interpretação linha a linha

| Trecho | Papel no teste |
|---|---|
| `[Fact]` | Faz o xUnit reconhecer o método como teste. |
| `Success()` | Identifica o cenário de validação bem-sucedida. |
| `new RegisterExpenseValidator()` | Prepara a unidade testada. |
| `new RequestRegisterExpenseJson { ... }` | Prepara uma entrada válida. |
| `validator.Validate(request)` | Executa a ação testada. |
| `Assert.True(result.IsValid)` | Verifica se a validação foi aprovada. |

Essa separação ajuda a entender exatamente **o que está sendo testado e qual resultado se espera**.

---

## 12. Executando e depurando o teste no Visual Studio

Depois de escrever o teste, o instrutor mostra como executá-lo individualmente.

### Caminho 1: pelo método de teste

No Visual Studio, ao clicar com o botão direito dentro do método, aparecem ações para executar ou depurar o teste.

O instrutor utiliza o atalho:

```text
Ctrl + R, Ctrl + T
```

Na demonstração, ele usa esse comando para depurar o teste.

### Caminho 2: pelo indicador lateral

Ao lado do método decorado com `[Fact]`, o Visual Studio oferece opções para:

- **Run Test:** executar o teste;
- **Debug Test:** executar com depurador.

### Executar x depurar

| Ação | Comportamento |
|---|---|
| **Run Test** | Executa o teste normalmente, sem interromper em breakpoints. |
| **Debug Test** | Executa sob o depurador e permite parar nos breakpoints. |

O instrutor insere um **breakpoint** no método, inicia o Debug Test e acompanha a execução de cada etapa:

```text
Breakpoint
   │
   ▼
Arrange: cria validator
   │
   ▼
Arrange: cria request
   │
   ▼
Act: executa Validate
   │
   ▼
Inspeção: result.IsValid == true
   │
   ▼
Assert.True(...)
   │
   ▼
Teste concluído com sucesso
```

Quando o teste passa, o indicador muda para **verde**.

---

## 13. Forçando uma falha e entendendo o resultado vermelho

Depois de comprovar o cenário válido, o instrutor altera temporariamente a requisição para observar uma falha.

### O que ele modifica?

O título, anteriormente preenchido, passa a ser vazio.

```csharp
Title = ""
```

### Qual é o efeito?

O validator identifica que a requisição não atende à regra de título obrigatório.

Consequentemente:

```csharp
result.IsValid == false
```

Mas a asserção continua esperando:

```csharp
Assert.True(result.IsValid);
```

Por isso, **o teste falha** e o Visual Studio exibe o indicador vermelho.

### Fluxo da falha

```text
Title = ""
     │
     ▼
RegisterExpenseValidator
     │
     ▼
Regra de título obrigatório não atendida
     │
     ▼
result.IsValid = false
     │
     ▼
Assert.True(false)
     │
     ▼
TESTE REPROVADO 🔴
```

Esse experimento mostra que o teste não está simplesmente executando código: ele realmente consegue identificar quando o comportamento observado não corresponde ao esperado.

### Retornando ao cenário original

Para finalizar, o instrutor desfaz a alteração, recupera o título preenchido e executa novamente o teste.

O resultado volta a ser **verde**, indicando que o cenário de sucesso foi restabelecido.

---

## 14. Próxima melhoria: geração de dados de teste

Ao final da aula, o instrutor observa que a requisição foi montada com **valores definidos manualmente**, ou seja, dados *hard-coded*.

Exemplo:

```csharp
Title = "Apple";
Amount = 100;
```

A próxima melhoria anunciada será usar um **Data Fake Generator** para gerar dados de teste.

A motivação é evitar a necessidade de preencher manualmente todos os campos da requisição em cada cenário.

**Importante:** o gerador de dados **não é implementado nesta aula**. O assunto fica para o próximo vídeo.

---

## 15. Observações técnicas e cuidados

Esta seção complementa a transcrição para evitar interpretações incorretas, sem alterar o conteúdo da aula.

### 15.1 Um teste aprovado não prova a ausência de todos os erros

O teste verifica apenas que **uma requisição válida específica** é aceita pelo validator. Ele não cobre, por si só, todos os cenários inválidos.

Por isso o instrutor reforça a necessidade de criar métodos de teste separados para outros comportamentos.

### 15.2 Os dados do Arrange devem respeitar todas as regras

Para um teste que espera `IsValid == true`, não basta deixar o título preenchido. Também precisamos garantir valor positivo, data permitida e tipo de pagamento válido.

Caso uma regra falhe, a asserção `Assert.True` falhará mesmo que as demais propriedades estejam corretas.

### 15.3 `[Fact]` não é uma regra para qualquer método C#

`[Fact]` é um atributo do **xUnit** usado para indicar um teste sem parâmetros de dados fornecidos por uma fonte externa.

Métodos auxiliares continuam sendo métodos comuns, sem esse atributo.

### 15.4 Datas em testes podem exigir atenção especial

A aula utiliza `DateTime.Now.AddDays(-1)` para garantir que a data esteja no passado.

Em testes mais rigorosos, quando regras dependem do instante atual, convém controlar a fonte de tempo para tornar os testes previsíveis. **Esse refinamento não é apresentado nesta aula.**

### 15.5 Um teste de sucesso não deve permanecer intencionalmente inválido

A alteração do título para vazio foi apenas uma demonstração da capacidade da asserção de detectar um resultado inesperado. O instrutor restaura os dados válidos ao fim do experimento.

---

## 16. Resumo

- O primeiro teste de unidade do curso verifica o cenário de sucesso do `RegisterExpenseValidator`.
- Os testes devem ter **pastas, classes e métodos com nomes significativos**.
- A estrutura do projeto de testes espelha a organização da funcionalidade na Application.
- A classe criada é `RegisterExpenseValidatorTests`.
- O método usa `[Fact]` para ser reconhecido pelo **xUnit**.
- O teste segue a organização **AAA: Arrange, Act e Assert**.
- O projeto de testes recebe referências à **Application** e à **Communication**.
- No **Arrange**, criamos o validator e uma requisição válida.
- No **Act**, executamos `validator.Validate(request)`.
- No **Assert**, verificamos `Assert.True(result.IsValid)`.
- Para manter a data no passado, o instrutor usa `DateTime.Now.AddDays(-1)`.
- O teste pode ser executado ou depurado no Visual Studio.
- O cenário válido passa (**verde**); ao esvaziar o título, a asserção falha (**vermelho**).
- A aula termina anunciando o uso futuro de um **gerador de dados falsos** para substituir parte dos dados hard-coded.

---

## 17. Visão geral

```text
        RegisterExpenseValidatorTests
                       │
                     [Fact]
                       │
                  Success()
                       │
           ┌───────────┼───────────┐
           ▼           ▼           ▼
        ARRANGE       ACT        ASSERT
           │           │           │
     Validator     Validate()   Assert.True
     Request         │         (result.IsValid)
           │         │           │
           └─────────┴───────────┘
                       │
                  Resultados
                       │
             ┌─────────┴─────────┐
             ▼                   ▼
         IsValid=true       IsValid=false
             │                   │
           VERDE              VERMELHO
          (passou)            (falhou)

         PRÓXIMA AULA
                │
                ▼
       Data Fake Generator
       (dados de teste gerados)
```

**Conclusão:** a aula demonstra a estrutura fundamental de um teste unitário usando xUnit. O objetivo é preparar uma entrada, executar a validação e confirmar que o resultado corresponde à expectativa. A organização por comportamento e o padrão AAA serão úteis na criação dos próximos testes.
