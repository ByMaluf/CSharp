# Testes de erro do Validator: ContainSingle, Contain, ResourceErrorMessages e cenários negativos

> **Aula 80 — Primeiro teste unitário para um cenário de validação inválida**
>
> **Continuidade:** aulas 74 a 78 (xUnit, padrão AAA, Bogus e Fluent Assertions) e aula 79 (alternativas de asserção e licenciamento). A transcrição desta aula utiliza **Fluent Assertions** nas verificações demonstradas; não apresenta uma migração do teste para Shouldly.

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [Testar o caminho feliz e os caminhos de erro](#2-testar-o-caminho-feliz-e-os-caminhos-de-erro)
3. [Planejando um teste para cada regra de validação](#3-planejando-um-teste-para-cada-regra-de-validação)
4. [Escolhendo o cenário: título vazio](#4-escolhendo-o-cenário-título-vazio)
5. [Criando o método de teste com Fact](#5-criando-o-método-de-teste-com-fact)
6. [Arrange: gerando dados válidos e invalidando apenas o título](#6-arrange-gerando-dados-válidos-e-invalidando-apenas-o-título)
7. [Act: executando a validação](#7-act-executando-a-validação)
8. [Assert: confirmando IsValid igual a false](#8-assert-confirmando-isvalid-igual-a-false)
9. [Assert: verificando a quantidade de erros](#9-assert-verificando-a-quantidade-de-erros)
10. [Assert: conferindo a mensagem de erro esperada](#10-assert-conferindo-a-mensagem-de-erro-esperada)
11. [O papel de ResourceErrorMessages](#11-o-papel-de-resourceerrormessages)
12. [Referência ao projeto Exception](#12-referência-ao-projeto-exception)
13. [Diferença crucial entre ContainSingle() e ContainSingle(predicate)](#13-diferença-crucial-entre-containSingle-e-containSinglepredicate)
14. [Exemplo completo do teste](#14-exemplo-completo-do-teste)
15. [Experimento 1: forçando dois erros](#15-experimento-1-forçando-dois-erros)
16. [Experimento 2: verificando a mensagem incorreta](#16-experimento-2-verificando-a-mensagem-incorreta)
17. [Por que provocar falhas de propósito?](#17-por-que-provocar-falhas-de-propósito)
18. [Observações técnicas e cuidados](#18-observações-técnicas-e-cuidados)
19. [O que fica para a próxima aula](#19-o-que-fica-para-a-próxima-aula)
20. [Resumo](#20-resumo)
21. [Visão geral](#21-visão-geral)

---

## 1. Objetivo da aula

Até aqui, escrevemos e executamos um **teste unitário de sucesso** para o `RegisterExpenseValidator`.

O teste utiliza uma requisição de cadastro de despesa com dados válidos, gerada pelo **Bogus** por meio de `RequestRegisterExpenseBuilder.Build()`, e espera que `result.IsValid` seja verdadeiro.

Agora, o instrutor começa a testar **situações em que os dados são inválidos**.

O objetivo específico desta aula é construir um teste que:

1. Gere uma requisição inicialmente válida.
2. Altere o campo `Title` para uma string vazia.
3. Execute a validação.
4. Confirme que o resultado é inválido: `IsValid == false`.
5. Confirme que existe **exatamente um erro** na coleção `Errors`.
6. Confirme que a mensagem desse erro corresponde à mensagem configurada para **título obrigatório**.

Além disso, a aula demonstra uma diferença importante entre duas sobrecargas de `ContainSingle` e mostra como testar se as asserções realmente detectam problemas.

---

## 2. Testar o caminho feliz e os caminhos de erro

O instrutor retoma uma orientação central da introdução aos testes unitários: **não devemos testar apenas o sucesso**.

Um sistema possui comportamentos válidos e inválidos, e ambos fazem parte do contrato que precisa ser verificado.

### Caminho feliz

```text
Requisição válida
      │
      ▼
RegisterExpenseValidator
      │
      ▼
IsValid = true
      │
      ▼
Teste espera true
      │
      ▼
APROVADO
```

### Caminho de erro

```text
Requisição inválida
      │
      ▼
RegisterExpenseValidator
      │
      ▼
IsValid = false
      │
      ▼
Errors contém falhas de validação
      │
      ▼
Teste verifica:
- a rejeição da request
- quantos erros foram gerados
- qual mensagem foi produzida
      │
      ▼
APROVADO, se a rejeição for a esperada
```

Um **teste de cenário inválido pode passar**. Ele passa quando o sistema rejeita a entrada **da forma prevista**.

Em outras palavras, não se deve confundir:

- **validação rejeitada:** o validator identificou corretamente um dado inválido;
- **teste reprovado:** a expectativa escrita no teste não foi satisfeita.

### Outra possibilidade citada no vídeo

O professor comenta que, em testes futuros de **use cases**, o resultado esperado poderá ser o lançamento de uma exceção.

Nesta aula, entretanto, o objeto testado é o **validator**. A expectativa é obter um `ValidationResult` com `IsValid == false` e os erros apropriados; **não** provocar uma exceção no validator.

---

## 3. Planejando um teste para cada regra de validação

O `RegisterExpenseValidator` já contém regras para quatro propriedades da requisição:

| Propriedade | Regra abordada nas aulas anteriores | Comportamento inválido |
|---|---|---|
| `Title` | `NotEmpty()` | Título nulo, vazio ou composto por espaços em branco |
| `Amount` | `GreaterThan(0)` | Valor menor ou igual a zero |
| `Date` | `LessThanOrEqualTo(...)` | Data posterior ao limite permitido |
| `PaymentType` | `IsInEnum()` | Valor não definido no enum |

O professor pretende escrever **um teste de erro para cada regra**.

Nesta aula, **somente o teste de título vazio é concluído**. Os demais são reservados para a sequência.

### Por que separar os cenários?

Porque cada método deve demonstrar um comportamento específico.

Um teste para a regra do título fica mais claro se a requisição **não violar outras regras simultaneamente**, salvo quando isso for feito deliberadamente para verificar uma situação particular.

Essa separação facilita descobrir se um teste falhou porque:

- o validator deixou de rejeitar o título vazio;
- foram produzidos erros a mais;
- o erro retornado não corresponde ao esperado.

---

## 4. Escolhendo o cenário: título vazio

O primeiro cenário de erro é a regra associada ao título:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty();
```

Nas aulas de FluentValidation e centralização de mensagens, essa regra também recebeu uma mensagem definida por `ResourceErrorMessages`.

### O que NotEmpty faz nesse contexto?

Para uma propriedade do tipo `string`, a regra `NotEmpty()` é utilizada para rejeitar valores considerados vazios, incluindo:

- `null`;
- `""`, a string vazia;
- uma string composta apenas por espaços em branco.

O instrutor escolhe testar, primeiro, a situação de **string vazia**:

```csharp
request.Title = string.Empty;
```

É um caso concreto da regra. Outros tipos de entrada inválida podem ser verificados em testes posteriores.

---

## 5. Criando o método de teste com Fact

Dentro da classe `RegisterExpenseValidatorTests`, o instrutor cria um novo método.

### Nome do método

O nome escolhido descreve o resultado e a propriedade testada:

```csharp
public void Error_Title_Empty()
{
}
```

A intenção é que o próprio nome já indique que o teste cobre um **erro para título vazio**.

### Identificando o método como teste

Como já estudamos no xUnit, é necessário marcá-lo com `[Fact]`:

```csharp
[Fact]
public void Error_Title_Empty()
{
}
```

Sem o atributo, seria apenas um método comum e não seria descoberto da mesma forma pelo framework de testes.

### Reaproveitando a estrutura anterior

O professor copia a base do teste de sucesso e modifica apenas os pontos necessários para o cenário inválido.

O teste continua organizado pelo **AAA — Arrange, Act e Assert**.

---

## 6. Arrange: gerando dados válidos e invalidando apenas o título

A etapa **Arrange** continua criando as duas instâncias utilizadas no teste:

```csharp
var validator = new RegisterExpenseValidator();
var request = RequestRegisterExpenseBuilder.Build();
```

A diferença é a alteração intencional de uma propriedade:

```csharp
request.Title = string.Empty;
```

### Por que gerar uma request válida antes?

Porque o builder criado com Bogus foi configurado para gerar dados adequados às regras de sucesso:

- título e descrição preenchidos;
- data passada;
- valor positivo;
- tipo de pagamento definido no enum.

Depois de gerar o objeto, o teste altera **apenas o campo que deseja invalidar**.

Assim, o cenário fica mais controlado:

```text
RequestRegisterExpenseBuilder.Build()
               │
               ▼
       Request válida
               │
               ▼
   request.Title = string.Empty
               │
               ▼
    Request com Title inválido
       e os demais campos válidos
```

Essa técnica simplifica a escrita de testes negativos.

Em vez de preencher manualmente todas as propriedades de cada caso, reutilizamos o builder e substituímos somente aquilo que queremos provocar.

---

## 7. Act: executando a validação

A etapa **Act** não muda em relação ao teste de sucesso:

```csharp
var result = validator.Validate(request);
```

O método `Validate` recebe a request cujo `Title` foi propositalmente esvaziado.

Nesse cenário, esperamos que a regra de título obrigatório produza uma falha de validação.

### Resultado esperado

```text
validator.Validate(request)
          │
          ▼
ValidationResult
   ├── IsValid = false
   └── Errors
       └── ValidationFailure do Title
```

O resultado real é obtido durante a execução. É na próxima etapa que o teste confirma se corresponde ao comportamento esperado.

---

## 8. Assert: confirmando IsValid igual a false

Na aula de sucesso, a verificação era:

```csharp
result.IsValid.Should().BeTrue();
```

Agora, com uma entrada intencionalmente inválida, a expectativa precisa mudar:

```csharp
result.IsValid.Should().BeFalse();
```

### O que essa asserção verifica?

Que o validator **não aprovou** a requisição.

Se o validator retornar `IsValid == true`, o teste falha, porque a regra deveria rejeitar o título vazio.

### Atenção à interpretação

Um resultado `false` aqui representa o **comportamento correto do validator** neste cenário.

Porém, essa verificação isolada não demonstra **qual regra** rejeitou a request.

Se outra propriedade também estiver inválida, `IsValid` continuará `false`, mesmo que a regra do título tenha parado de funcionar.

Por isso o instrutor acrescenta verificações sobre `Errors`.

---

## 9. Assert: verificando a quantidade de erros

O método `Validate` retorna uma coleção `Errors` com os registros das falhas encontradas.

O instrutor quer garantir que a lista possua **um único elemento**, porque só o título foi invalidado de propósito.

A sintaxe do Fluent Assertions utilizada na aula é:

```csharp
result.Errors.Should().ContainSingle();
```

### O que ContainSingle() verifica?

Sem parâmetros, `ContainSingle()` exige **exatamente um elemento na coleção inteira**.

| Quantidade de elementos em `Errors` | `ContainSingle()` |
|---:|---|
| 0 | Falha |
| 1 | Passa |
| 2 | Falha |
| 3 ou mais | Falha |

### Por que isso importa?

Suponha que o título esteja vazio e o valor da despesa também esteja negativo.

O validator pode devolver:

```text
Errors
├── Erro: título obrigatório
└── Erro: valor deve ser maior que zero
```

O resultado ainda terá `IsValid == false`, mas **não atenderá à expectativa de um único erro**.

Portanto, `ContainSingle()` garante uma parte adicional do comportamento.

---

## 10. Assert: conferindo a mensagem de erro esperada

Verificar somente que existe um erro também é insuficiente: o erro poderia ser de outra propriedade.

Por isso, o instrutor acrescenta uma condição que exige a **mensagem de título obrigatório**.

### Encadeamento mostrado na aula

```csharp
result.Errors.Should()
    .ContainSingle()
    .And.Contain(error =>
        error.ErrorMessage.Equals(ResourceErrorMessages.TITLE_REQUIRED));
```

Essa verificação combina duas expectativas:

1. **`ContainSingle()`:** a coleção inteira deve conter exatamente um erro.
2. **`Contain(...)`:** deve existir um elemento cuja propriedade `ErrorMessage` corresponda à mensagem esperada.

### Entendendo cada parte

**`result.Errors`**

É a coleção de `ValidationFailure` produzida pelo FluentValidation.

**`Should()`**

Inicia a sintaxe de asserções do Fluent Assertions.

**`ContainSingle()`**

Confirma que existe exatamente uma falha na coleção.

**`.And`**

Permite continuar o encadeamento fluente de verificações sobre a coleção.

**`Contain(error => ...)`**

Recebe uma expressão lambda que define a condição que algum elemento precisa satisfazer.

**`error.ErrorMessage`**

É o texto associado a cada `ValidationFailure`.

**`ResourceErrorMessages.TITLE_REQUIRED`**

É a mensagem de título obrigatório definida nos arquivos de recursos apresentados nas aulas anteriores.

### Por que usar as duas verificações?

Considere três respostas possíveis.

| Resposta recebida | `ContainSingle()` | `Contain` com título obrigatório | Teste completo |
|---|---|---|---|
| Só erro de título | Passa | Passa | Passa |
| Erro de título e erro de valor | Falha | A mensagem existe | Falha |
| Só erro de valor | Passa | Falha | Falha |

A combinação garante simultaneamente **quantidade** e **conteúdo**.

---

## 11. O papel de ResourceErrorMessages

Nas aulas anteriores, criamos os arquivos `.resx` para centralizar mensagens de erro e permitir localização.

Uma dessas mensagens é acessada pela propriedade:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

Em vez de escrever uma string literal diretamente no teste:

```csharp
// Exemplo ilustrativo de mensagem fixa — não é o formato escolhido.
error.ErrorMessage == "O título é obrigatório"
```

o professor utiliza a chave gerada pelo recurso.

### Por que isso é útil no contexto mostrado?

O sistema já tem a mesma mensagem centralizada para o validator e os testes.

Isso evita que o teste dependa de uma tradução literal específica digitada manualmente.

### E os diferentes idiomas?

O instrutor esclarece no final da aula que **não está testando a seleção de idiomas neste momento**.

Como o teste usa o recurso disponível para a cultura do ambiente, ele pode manter essa estrutura.

Testar explicitamente se o recurso retorna a tradução correta em diferentes culturas é um **outro objetivo de teste**, que o professor promete abordar futuramente.

### Cuidados técnicos

O arquivo `.resx` é responsável pelo valor associado à chave e a seleção pode depender da `CurrentUICulture`.

Usar a mesma propriedade `ResourceErrorMessages.TITLE_REQUIRED` tanto na implementação quanto na expectativa do teste verifica a **consistência com o recurso utilizado**, mas não prova, por si só, que uma tradução literal de determinado idioma está correta.

Esta última observação é uma **complementação técnica**, não uma verificação de localização demonstrada na aula.

---

## 12. Referência ao projeto Exception

Ao utilizar `ResourceErrorMessages` no teste, o instrutor percebe que a classe está acessível mesmo sem uma referência direta do projeto `Validators.Test` ao projeto `Exception`.

Ele explica que isso acontece porque o teste já referencia o projeto `Application`, e esse projeto acessa os recursos públicos definidos na camada de exceções.

### Relações apresentadas na aula

```text
Validators.Test
       │
       ▼
  Application
       │
       ▼
   Exception
       │
       ▼
ResourceErrorMessages
```

Mesmo com a classe acessível, o professor decide adicionar uma **referência direta ao projeto Exception**.

### Por que isso pode ser útil?

Porque o teste **utiliza diretamente uma classe da biblioteca Exception**. Declarar a dependência de forma explícita ajuda a comunicar essa relação e reduz a necessidade de depender apenas de referências transitivas.

**Nota técnica:** em projetos .NET modernos com referências do SDK, dependências de projeto podem ser transitivas, dependendo da configuração. O comportamento observado não é uma falha do compilador: trata-se de como as referências se propagam na solução.

### Estrutura após a alteração

```text
Validators.Test
   ├── Application
   ├── Communication
   ├── CommonTestUtilities
   └── Exception
         └── ResourceErrorMessages
```

---

## 13. Diferença crucial entre ContainSingle() e ContainSingle(predicate)

Esta é uma das explicações centrais da aula.

O instrutor mostra que o método `ContainSingle` oferece uma sobrecarga que recebe uma **expressão lambda** (*predicate*).

Essas duas formas **não verificam a mesma coisa**.

### Forma A — Sem parâmetro

```csharp
result.Errors.Should().ContainSingle();
```

Verifica:

> A coleção inteira possui exatamente **um elemento**?

Formalmente:

```text
Total de elementos == 1
```

### Forma B — Com expressão lambda

```csharp
result.Errors.Should().ContainSingle(error =>
    error.ErrorMessage.Equals(ResourceErrorMessages.TITLE_REQUIRED));
```

Verifica:

> Na coleção, **exatamente um elemento** satisfaz o predicado?

Formalmente:

```text
Quantidade de elementos que atendem à condição == 1
```

**A coleção pode conter outros elementos que não satisfaçam a condição.**

### Exemplo com números

Considere:

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
```

Se executarmos:

```csharp
numbers.Should().ContainSingle();
```

o teste **falhará**, pois a lista tem dez elementos.

Mas se executarmos:

```csharp
numbers.Should().ContainSingle(n => n == 7);
```

o teste **passará**, porque existe exatamente **um elemento igual a 7**, mesmo que a lista tenha dez itens.

### E se houver dois valores iguais a 7?

```csharp
var numbers = new List<int> { 1, 7, 2, 7, 3 };
```

Nesse caso:

```csharp
numbers.Should().ContainSingle(n => n == 7);
```

**falha**, porque dois elementos satisfazem o predicado.

### Tabela comparativa

| Coleção | `ContainSingle()` | `ContainSingle(n => n == 7)` |
|---|---|---|
| `[7]` | Passa | Passa |
| `[1, 2, 7, 9]` | Falha | Passa |
| `[7, 7]` | Falha | Falha |
| `[1, 2, 3]` | Falha | Falha |
| `[]` | Falha | Falha |

### Por que o professor não escolhe a forma com predicate?

Porque a expectativa do teste do título não é simplesmente encontrar **uma mensagem de título obrigatório** entre várias mensagens.

A expectativa é mais exigente:

> A request foi construída válida e teve **somente o título alterado**. Logo, o resultado deve trazer exatamente **um erro**, e esse erro deve ser o de título obrigatório.

Por isso ele mantém:

```csharp
result.Errors.Should()
    .ContainSingle()
    .And.Contain(error =>
        error.ErrorMessage.Equals(ResourceErrorMessages.TITLE_REQUIRED));
```

A implementação comunica duas invariantes distintas.

---

## 14. Exemplo completo do teste

O código abaixo é uma **reconstrução didática baseada na transcrição**. Os namespaces exatos devem corresponder à organização da solução.

```csharp
using FluentAssertions;
using Xunit;

// Adicionar os namespaces correspondentes a:
// - RegisterExpenseValidator
// - RequestRegisterExpenseBuilder
// - ResourceErrorMessages

public class RegisterExpenseValidatorTests
{
    [Fact]
    public void Error_Title_Empty()
    {
        // Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseBuilder.Build();

        request.Title = string.Empty;

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
}
```

### Entendendo o método completo

| Linha ou operação | Responsabilidade |
|---|---|
| `[Fact]` | Indica ao xUnit que este método é um teste. |
| `Error_Title_Empty()` | Descreve qual falha de validação está sendo testada. |
| `new RegisterExpenseValidator()` | Prepara o validator. |
| `RequestRegisterExpenseBuilder.Build()` | Gera uma request normalmente válida. |
| `request.Title = string.Empty` | Força especificamente a falha do título. |
| `validator.Validate(request)` | Executa as regras de validação. |
| `Should().BeFalse()` | Exige que a request seja rejeitada. |
| `ContainSingle()` | Exige um único erro na lista. |
| `Contain(error => ...)` | Exige que a mensagem esperada esteja presente. |

### Resultado esperado

```text
Request válida gerada
          │
          ▼
Title alterado para ""
          │
          ▼
Validator.Validate(request)
          │
          ▼
IsValid = false
Errors.Count = 1
Errors[0].ErrorMessage = TITLE_REQUIRED
          │
          ▼
TESTE APROVADO
```

---

## 15. Experimento 1: forçando dois erros

Depois de escrever o teste, o instrutor executa a implementação e observa que o resultado esperado aparece.

Para conferir que a verificação de quantidade realmente funciona, ele **introduz temporariamente outro erro**.

### Alteração temporária

Além do título vazio:

```csharp
request.Title = string.Empty;
```

ele também altera o valor:

```csharp
request.Amount = -1;
```

### Resultado esperado do validator

Agora são violadas duas regras:

- título obrigatório;
- valor maior que zero.

A coleção de erros pode conter:

```text
Errors
├── TITLE_REQUIRED
└── AMOUNT_GREATER_THAN_ZERO
```

Isso gera:

```csharp
result.IsValid == false
result.Errors.Count == 2
```

### O que acontece com as asserções?

A primeira verifica:

```csharp
result.IsValid.Should().BeFalse();
```

**Passa**, porque a requisição é inválida.

A segunda exige:

```csharp
result.Errors.Should().ContainSingle();
```

**Falha**, porque foram encontrados **dois erros**.

O instrutor observa o indicador vermelho do teste, demonstrando que a asserção protege contra a presença inesperada de erros adicionais.

---

## 16. Experimento 2: verificando a mensagem incorreta

O professor executa um segundo experimento para verificar a outra parte da asserção.

### Alteração temporária

Ele desabilita a alteração que deixava o título vazio e mantém o valor da despesa inválido.

Conceitualmente:

```csharp
var request = RequestRegisterExpenseBuilder.Build();

// request.Title = string.Empty;  // título permanece válido
request.Amount = -1;
```

Agora, o validator deve retornar **um único erro**, mas relacionado ao valor da despesa.

```text
Errors.Count = 1
Errors[0].ErrorMessage = AMOUNT_GREATER_THAN_ZERO
```

### O que acontece?

A verificação de quantidade:

```csharp
result.Errors.Should().ContainSingle();
```

**Passa**, pois a lista possui exatamente um elemento.

Entretanto, o teste continua exigindo que exista o erro de título:

```csharp
.And.Contain(error =>
    error.ErrorMessage.Equals(ResourceErrorMessages.TITLE_REQUIRED));
```

Essa verificação **falha**, porque o único erro é relacionado a `Amount`.

### O que esse experimento comprova?

Que verificar apenas a quantidade de erros não basta.

Um teste pode receber **um erro**, porém ser **o erro errado**.

Por isso a segunda parte da asserção é necessária.

### Comparando os dois experimentos

| Situação | Resultado do validator | Asserção que detecta o problema |
|---|---|---|
| Título vazio e valor negativo | Dois erros | `ContainSingle()` |
| Título válido e valor negativo | Um erro, mas de `Amount` | `Contain(error => ...TITLE_REQUIRED)` |

Após os experimentos, o professor restaura o cenário original do teste e executa novamente, deixando o resultado **verde**.

---

## 17. Por que provocar falhas de propósito?

A aula recomenda um hábito útil para quem está escrevendo testes: **modificar temporariamente a entrada e confirmar que o teste falha quando deveria falhar**.

Por quê?

Porque é possível cometer um erro na própria implementação do teste.

Por exemplo:

- verificar apenas `IsValid == false` sem conferir a regra responsável;
- usar uma asserção que aceita erros adicionais inesperados;
- conferir a quantidade, mas não o conteúdo;
- interpretar de forma incorreta uma sobrecarga que recebe função lambda.

Os dois experimentos realizados pelo instrutor mostram que as verificações não estão apenas passando por coincidência no cenário original.

### Estratégia da aula

```text
1. Construir o cenário correto
              │
              ▼
2. Executar e confirmar que o teste passa
              │
              ▼
3. Alterar a entrada para violar uma expectativa
              │
              ▼
4. Confirmar que a asserção relevante falha
              │
              ▼
5. Restaurar o cenário original
              │
              ▼
6. Executar novamente e confirmar aprovação
```

Esse procedimento ajuda a avaliar se o teste realmente é sensível ao comportamento que pretende verificar.

---

## 18. Observações técnicas e cuidados

### 18.1 A aula usa Fluent Assertions, apesar da discussão anterior sobre Shouldly

Na aula 79, o professor apresentou a mudança de licenciamento do Fluent Assertions e alternativas como o Shouldly.

Nesta aula 80, porém, o código explicado na transcrição utiliza o estilo do **Fluent Assertions**:

```csharp
result.IsValid.Should().BeFalse();

result.Errors.Should()
    .ContainSingle()
    .And.Contain(...);
```

Isso significa que **não devemos substituir silenciosamente o código da aula por Shouldly**.

A documentação acima preserva o framework de asserções efetivamente usado nesta gravação.

Uma eventual adaptação ao Shouldly seria um **trabalho distinto** e envolveria outras APIs; não foi demonstrada neste vídeo.

### 18.2 IsValid = false não verifica qual regra falhou

O booleano indica que a request é inválida.

Para garantir que a falha corresponde ao cenário de título obrigatório, precisamos verificar também o conteúdo da coleção de erros.

### 18.3 ContainSingle(predicate) não exige que a lista inteira tenha um elemento

Ele verifica a ocorrência única de um elemento que satisfaz a condição, mesmo se outros erros estiverem presentes.

Essa é a distinção mais importante da explicação técnica desta aula.

### 18.4 Resources e localização são assuntos relacionados, mas diferentes

O teste usa o recurso `TITLE_REQUIRED` como mensagem esperada, sem forçar a execução em culturas como `pt-BR` e `pt-PT`.

Verificar traduções para diferentes culturas exigirá um teste específico.

### 18.5 O builder deve gerar entradas válidas nas demais propriedades

Se o Bogus gerar por engano uma outra propriedade inválida, o teste de título vazio poderá devolver mais de um erro e falhar.

Isso torna importante manter os dados-base do builder adequados ao cenário.

### 18.6 Correção de termos da transcrição

Algumas expressões do áudio representam reconhecimento fonético impreciso:

| Termo ouvido ou transcrito | Termo técnico usado no código |
|---|---|
| “invalid” | `IsValid` |
| “set” | `[Fact]`, pelo contexto do atributo de teste |
| “shows” | `Should()` |
| “content single” | `ContainSingle()` |
| “end contain” | `.And.Contain(...)` |
| “resource messages” | `ResourceErrorMessages` |

Essas correspondências servem apenas para ler corretamente a demonstração de código; não são funcionalidades adicionais.

---

## 19. O que fica para a próxima aula

Ao final, o instrutor informa que o teste de título obrigatório foi concluído.

**Ainda falta escrever testes de erro para as outras propriedades do validator.**

A próxima aula também apresentará **um outro atributo de teste além de `[Fact]`** e utilizará esse recurso para melhorar a implementação recém-criada.

A transcrição desta aula **não fornece o nome nem o código do novo atributo**. Portanto, a explicação detalhada fica reservada para a próxima gravação.

Além disso, o professor comenta que a verificação de recursos para diferentes idiomas será assunto de outro tipo de teste, não deste.

---

## 20. Resumo

- Testes unitários devem cobrir **cenários de sucesso e de erro**.
- No `RegisterExpenseValidator`, o instrutor planeja um teste para cada regra de validação.
- Nesta aula, ele implementa somente o cenário de **título vazio**.
- O método é identificado por `[Fact]` e recebe um nome descritivo, como `Error_Title_Empty`.
- No **Arrange**, o builder gera uma requisição inicialmente válida e o teste altera `Title` para `string.Empty`.
- No **Act**, o teste chama `validator.Validate(request)`.
- No **Assert**, `result.IsValid.Should().BeFalse()` confirma a rejeição.
- `result.Errors.Should().ContainSingle()` exige **exatamente um erro**.
- `.And.Contain(error => error.ErrorMessage.Equals(ResourceErrorMessages.TITLE_REQUIRED))` verifica a mensagem específica.
- O projeto de testes passa a referenciar diretamente a biblioteca **Exception**, onde estão os recursos usados.
- `ContainSingle()` sem parâmetro verifica o **tamanho total da coleção**.
- `ContainSingle(predicate)` verifica a **quantidade de elementos que correspondem à condição**.
- O instrutor força dois erros e demonstra que `ContainSingle()` falha.
- Depois provoca um único erro na propriedade errada e demonstra que a verificação de mensagem falha.
- O cenário original é restaurado e o teste volta a passar.
- Os demais erros e o novo atributo de teste ficam para a próxima aula.
- Nesta gravação, o código efetivamente mostrado usa **Fluent Assertions**, mesmo após a discussão de alternativas na aula 79.

---

## 21. Visão geral

```text
              RegisterExpenseValidatorTests
                           │
                         [Fact]
                           │
                   Error_Title_Empty
                           │
                           ▼
                        ARRANGE
                           │
     RequestRegisterExpenseBuilder.Build()
                           │
                           ▼
                request.Title = ""
                           │
                           ▼
                          ACT
                           │
                validator.Validate(request)
                           │
                           ▼
                         ASSERT
                           │
           ┌───────────────┼──────────────────┐
           ▼               ▼                  ▼
      IsValid false    Errors.Count == 1    TITLE_REQUIRED
           │               │                  │
           │               │                  │
           └───────────────┼──────────────────┘
                           │
                           ▼
                      TESTE PASSA

           VERIFICAÇÕES DELIBERADAS
                           │
            ┌──────────────┴─────────────┐
            ▼                            ▼
     Title vazio + Amount -1       Só Amount -1
            │                            │
            ▼                            ▼
     2 erros → ContainSingle      1 erro, mas mensagem
                falha               diferente → Contain
                                         falha
            │                            │
            └──────────────┬─────────────┘
                           │
                           ▼
                 Restaurar o cenário
                           │
                           ▼
                  Teste verde novamente

                      PRÓXIMA AULA
               Outros erros do validator
               + outro atributo de teste
```

**Conclusão:** esta aula inicia a cobertura dos cenários de erro e ensina a escrever testes mais específicos: não basta confirmar que a validação falhou, é necessário verificar **quantos erros foram produzidos e se o erro corresponde exatamente ao cenário testado**.
