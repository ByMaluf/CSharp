# Fluent Assertions: asserções fluentes com Should().BeTrue() em testes xUnit

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [O que já estava implementado](#2-o-que-já-estava-implementado)
3. [Por que conhecer outra biblioteca de asserções](#3-por-que-conhecer-outra-biblioteca-de-asserções)
4. [O que é Fluent Assertions](#4-o-que-é-fluent-assertions)
5. [Instalando o pacote pelo NuGet](#5-instalando-o-pacote-pelo-nuget)
6. [Conhecendo as categorias de asserções](#6-conhecendo-as-categorias-de-asserções)
7. [Como funciona a sintaxe Should().BeTrue()](#7-como-funciona-a-sintaxe-shouldbetrue)
8. [Substituindo Assert.True no teste](#8-substituindo-asserttrue-no-teste)
9. [Exemplo do teste de sucesso atualizado](#9-exemplo-do-teste-de-sucesso-atualizado)
10. [Execução e depuração no Visual Studio](#10-execução-e-depuração-no-visual-studio)
11. [Comparando xUnit e Fluent Assertions](#11-comparando-xunit-e-fluent-assertions)
12. [Asserções para outros tipos de dados](#12-asserções-para-outros-tipos-de-dados)
13. [Fluent Assertions não é FluentValidation](#13-fluent-assertions-não-é-fluentvalidation)
14. [Cuidados técnicos e limites da aula](#14-cuidados-técnicos-e-limites-da-aula)
15. [O que será estudado na próxima aula](#15-o-que-será-estudado-na-próxima-aula)
16. [Resumo](#16-resumo)
17. [Visão geral](#17-visão-geral)

---

## 1. Objetivo da aula

Nas aulas anteriores, criamos um teste unitário para a classe `RegisterExpenseValidator`, organizamos o teste pelo padrão **AAA (Arrange, Act, Assert)** e passamos a gerar os dados da requisição utilizando **Bogus** por meio de `RequestRegisterExpenseBuilder.Build()`.

O teste já funcionava corretamente. Sua etapa **Assert** era:

```csharp
Assert.True(result.IsValid);
```

O objetivo desta aula é **melhorar a forma de escrever as asserções**, utilizando uma biblioteca NuGet chamada **Fluent Assertions**.

O instrutor destaca duas vantagens que motivam a escolha:

- Uma **sintaxe fluente**, que permite expressar verificações de maneira legível e, em determinados casos, encadear verificações.
- Uma variedade de **asserções específicas para diferentes tipos de valores**, como strings, datas, coleções, enums e exceções.

Nesta aula, a alteração concreta é pequena: **substituir `Assert.True(result.IsValid)` por `result.IsValid.Should().BeTrue()` e executar novamente o teste**.

Ainda não são implementados os testes de erro do validator. Eles são anunciados para a próxima aula.

---

## 2. O que já estava implementado

O teste criado nas aulas 75 a 77 valida um cenário de sucesso.

### Fluxo do teste existente

```text
Arrange
  ├── new RegisterExpenseValidator()
  └── RequestRegisterExpenseBuilder.Build()
             │
             └── Bogus gera dados válidos

Act
  └── validator.Validate(request)

Assert (antes desta aula)
  └── Assert.True(result.IsValid)
```

A finalidade continua sendo verificar se uma requisição de cadastro de despesas, construída com dados válidos, passa pelas regras do `RegisterExpenseValidator`.

O instrutor não apresenta um defeito na asserção anterior. Pelo contrário, afirma que a forma utilizada **funciona** e **não está errada**.

A melhoria está relacionada principalmente à **expressividade e à conveniência da API de asserções**.

---

## 3. Por que conhecer outra biblioteca de asserções

O instrutor explica que, ao longo de um projeto, os testes passam a lidar com valores e estruturas diferentes de um único booleano.

Por exemplo:

- verificar o conteúdo de uma string;
- comparar datas;
- examinar elementos de listas;
- verificar se uma coleção está vazia;
- comparar valores de enumerações;
- garantir que uma operação lança determinada exceção.

As asserções que já existem no framework de testes podem atender a essas necessidades, mas uma biblioteca especializada pode oferecer maneiras mais **legíveis e direcionadas** de expressar as expectativas.

### O que significa *assertion*?

Uma **asserção** é uma verificação de algo que deve ser verdadeiro para que o teste seja aprovado.

Em termos gerais:

```text
Operação executada
         │
         ▼
Resultado obtido
         │
         ▼
Comparação com a expectativa
         │
    ┌────┴────┐
    ▼         ▼
 Confere    Não confere
    │         │
    ▼         ▼
  Passa      Falha
```

Um teste só é realmente útil quando deixa explícito **qual comportamento espera observar**.

---

## 4. O que é Fluent Assertions

**Fluent Assertions** é uma biblioteca para escrever asserções em testes .NET com uma sintaxe inspirada na linguagem natural.

O exemplo central da aula é:

```csharp
result.IsValid.Should().BeTrue();
```

A leitura pode ser entendida como:

> A propriedade `IsValid` do resultado **deveria ser verdadeira**.

O verbo `Should` deixa mais evidente que estamos declarando uma **expectativa do teste**.

### Diferença de leitura

Forma anterior:

```csharp
Assert.True(result.IsValid);
```

Forma apresentada nesta aula:

```csharp
result.IsValid.Should().BeTrue();
```

As duas expressam, neste cenário, a **mesma expectativa lógica**: `result.IsValid` precisa ser `true`.

A diferença principal aqui é a forma de expressar a asserção, e não o critério de aprovação.

---

## 5. Instalando o pacote pelo NuGet

O instrutor instala o pacote diretamente no projeto que contém os testes de validators.

### Passo a passo no Visual Studio

1. Localizar o projeto **`Validators.Test`**.
2. Clicar com o botão direito no projeto.
3. Selecionar **Manage NuGet Packages** (*Gerenciar Pacotes NuGet*).
4. Abrir a aba **Browse** (*Procurar*).
5. Pesquisar por **`FluentAssertions`** ou **Fluent Assertions**.
6. Selecionar o pacote correspondente.
7. Clicar em **Install** (*Instalar*).
8. Confirmar a operação de instalação quando solicitado.

Após a instalação, o pacote fica disponível como dependência do projeto de testes.

O instrutor também consulta a **documentação do Fluent Assertions** para mostrar as categorias de asserções oferecidas.

### Importação no código

Para utilizar os métodos de extensão da biblioteca, o teste precisa ter acesso ao namespace:

```csharp
using FluentAssertions;
```

Na demonstração, o Visual Studio oferece o ajuste automático do `using` ao identificar a chamada `Should()`.

**Observação:** o vídeo não informa a versão específica do pacote instalada.

---

## 6. Conhecendo as categorias de asserções

O instrutor mostra que a documentação organiza exemplos por tipos de valores e cenários de teste.

As categorias citadas na aula incluem:

| Categoria | Tipo de verificação |
|---|---|
| **Booleanos** | Verificar se um valor é `true` ou `false`. |
| **Strings** | Comparar conteúdo ou características de um texto. |
| **Datas e horários** | Expressar expectativas sobre valores temporais. |
| **Coleções** | Verificar listas e seus elementos. |
| **Dicionários** | Realizar verificações sobre pares chave–valor. |
| **Enums** | Testar valores de enumerações. |
| **Exceções** | Verificar se uma operação lança uma exceção esperada. |

Essa variedade é um dos principais motivos apresentados para adotar a biblioteca em uma suíte maior de testes.

### Exemplo conceitual da documentação

O instrutor comenta um exemplo com uma string cujo valor real é diferente do esperado.

O conceito é semelhante a:

```csharp
string username = "James";

username.Should().Be("Jonas");
```

Esse exemplo **falharia**, pois o valor real `"James"` não é igual a `"Jonas"`.

Ele demonstra que o encadeamento fluente não altera a expectativa: se a condição não for atendida, o teste será reprovado.

---

## 7. Como funciona a sintaxe Should().BeTrue()

O ponto central da aula é esta expressão:

```csharp
result.IsValid.Should().BeTrue();
```

Vamos dividi-la.

### 7.1 `result.IsValid`

É o valor real que queremos verificar.

```csharp
result.IsValid
```

A propriedade vem do resultado produzido pelo FluentValidation após chamar `validator.Validate(request)`.

Seu tipo é **`bool`**, ou seja, pode ser `true` ou `false`.

### 7.2 `Should()`

```csharp
result.IsValid.Should()
```

Indica que vamos escrever uma **asserção sobre aquele valor**.

A biblioteca expõe métodos de extensão que permitem chamar `Should()` diretamente sobre o valor testado.

### 7.3 `BeTrue()`

```csharp
result.IsValid.Should().BeTrue();
```

Define a expectativa concreta: o valor booleano **deve ser verdadeiro**.

Caso `IsValid` seja `false`, a asserção falhará.

### Exemplo da expectativa contrária

A aula menciona que, para verificar um booleano falso, existe uma operação correspondente:

```csharp
result.IsValid.Should().BeFalse();
```

Esse formato poderá ser útil quando o teste **esperar a rejeição** de uma requisição inválida.

O instrutor não implementa esse teste de erro neste vídeo.

---

## 8. Substituindo Assert.True no teste

Antes da instalação:

```csharp
// Assert
Assert.True(result.IsValid);
```

Depois da instalação:

```csharp
// Assert
result.IsValid.Should().BeTrue();
```

A chamada `Assert.True(...)` pode ser removida, pois a verificação passou a ser feita pelo Fluent Assertions.

O restante do teste **não precisa mudar**:

- o validator continua sendo o mesmo;
- o builder com Bogus continua gerando a requisição;
- a ação continua sendo `validator.Validate(request)`;
- a propriedade verificada continua sendo `result.IsValid`.

A alteração é localizada na fase **Assert** do padrão AAA.

### Antes e depois

| Aspecto | Antes | Depois |
|---|---|---|
| Framework de testes | xUnit | xUnit |
| Biblioteca usada na asserção | API de asserções do xUnit | Fluent Assertions |
| Verificação | `Assert.True(result.IsValid)` | `result.IsValid.Should().BeTrue()` |
| Resultado esperado | `IsValid == true` | `IsValid == true` |
| Gerador de dados | Bogus | Bogus |
| Validator testado | `RegisterExpenseValidator` | `RegisterExpenseValidator` |

A mudança não significa que o xUnit deixou de ser utilizado. Ele continua sendo responsável pela infraestrutura do teste, incluindo a descoberta do método marcado com `[Fact]`.

---

## 9. Exemplo do teste de sucesso atualizado

A seguir, uma **reconstrução didática baseada no fluxo da aula**, utilizando as classes estudadas anteriormente.

```csharp
using FluentAssertions;
using Xunit;

// Adicionar os namespaces de RegisterExpenseValidator
// e RequestRegisterExpenseBuilder conforme a solução.

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
}
```

**Observação:** os nomes de namespaces foram omitidos porque a transcrição não fornece a declaração completa de cada arquivo. O trecho documenta as chamadas fundamentais, e não pretende reproduzir literalmente o editor do vídeo.

### Interpretando as três etapas

| Etapa | Código | Responsabilidade |
|---|---|---|
| **Arrange** | `new RegisterExpenseValidator()` | Instancia a unidade testada. |
| **Arrange** | `RequestRegisterExpenseBuilder.Build()` | Obtém a request gerada pelo Bogus. |
| **Act** | `validator.Validate(request)` | Executa o comportamento testado. |
| **Assert** | `result.IsValid.Should().BeTrue()` | Confirma que a validação foi aprovada. |

O método de teste permanece curto e mantém uma responsabilidade bem definida: **confirmar que uma requisição válida é aceita pelo validator**.

---

## 10. Execução e depuração no Visual Studio

Depois de substituir a asserção, o instrutor reorganiza os `using` e executa o teste novamente.

Durante a aula, são mencionados os atalhos:

- `Ctrl + R, Ctrl + G` para remover imports não utilizados, conforme o ambiente do instrutor;
- `Ctrl + R, Ctrl + T` para iniciar a execução/depuração do teste individual, conforme o comando disponível na configuração utilizada.

**Nota:** atalhos podem variar conforme o mapa de teclado e a versão do Visual Studio.

### Fluxo da demonstração

1. O teste chega ao breakpoint.
2. O método utiliza o builder criado nas aulas anteriores.
3. O **Bogus** gera os dados para a requisição.
4. O código chama `validator.Validate(request)`.
5. O resultado possui `IsValid == true`.
6. A asserção `result.IsValid.Should().BeTrue()` é avaliada.
7. A execução é concluída com sucesso.

### Resultado observado

```text
RequestRegisterExpenseBuilder.Build()
                  │
                  ▼
        Dados gerados pelo Bogus
                  │
                  ▼
   RegisterExpenseValidator.Validate(...)
                  │
                  ▼
           result.IsValid
                  │
                true
                  │
                  ▼
      Should().BeTrue()
                  │
                  ▼
          TESTE APROVADO
```

A demonstração confirma que **a alteração da biblioteca de asserções não quebrou o teste de sucesso**.

---

## 11. Comparando xUnit e Fluent Assertions

A aula diferencia a forma anterior de verificar resultados da nova biblioteca, mas vale precisar a origem de cada ferramenta.

O método `Assert.True(...)` usado nas aulas anteriores pertence à **API de asserções do xUnit**, e não diretamente à biblioteca-base do .NET. O instrutor se refere a ele de maneira informal como o Assert disponível no ambiente .NET.

Isso não altera a ideia principal da aula: **a asserção anterior era correta e continua válida**.

### Comparação técnica

| | xUnit (`Assert`) | Fluent Assertions |
|---|---|---|
| Exemplo booleano | `Assert.True(result.IsValid)` | `result.IsValid.Should().BeTrue()` |
| Modelo de chamada | Função de asserção recebe o valor | A asserção é escrita a partir do próprio valor |
| Leitura | “Verifique que é verdadeiro” | “O valor deveria ser verdadeiro” |
| Strings e coleções | Possui métodos próprios para esses cenários | Oferece operações fluentes específicas |
| Exceções | Também possui formas de testar exceções | Oferece API fluente para expectativas sobre exceções |
| Papel na aula | Forma anterior | Forma nova adotada |

**Não é uma relação de certo versus errado.** O instrutor troca a abordagem por preferência de legibilidade e recursos disponibilizados pela biblioteca.

O xUnit **não é substituído** pelo Fluent Assertions: as duas bibliotecas cumprem papéis complementares no mesmo teste.

---

## 12. Asserções para outros tipos de dados

O vídeo apresenta a documentação e enumera recursos que podem ser úteis futuramente, mas **não implementa testes completos de todas essas categorias**.

Os trechos abaixo são **exemplos adicionais para fixar o conceito**, não códigos executados nesta aula.

### Booleanos

```csharp
true.Should().BeTrue();
false.Should().BeFalse();
```

### Strings

```csharp
string title = "Compra";

title.Should().Be("Compra");
```

### Coleções

```csharp
var values = new List<int> { 1, 2, 3 };

values.Should().Contain(2);
values.Should().HaveCount(3);
```

### Exceções

O instrutor menciona que a biblioteca também permite verificar se uma operação lança a exceção esperada.

Isso será relevante quando os testes deixarem de abordar somente validação bem-sucedida e começarem a verificar **situações de erro**, inclusive regras de negócio.

Nesta aula, porém, a implementação efetiva é somente a substituição do Assert do cenário de sucesso.

---

## 13. Fluent Assertions não é FluentValidation

Como os nomes são parecidos, é importante distinguir as duas bibliotecas e também o Bogus.

| Biblioteca | Responsabilidade | Exemplo |
|---|---|---|
| **FluentValidation** | Validar dados da aplicação conforme regras definidas | `RuleFor(x => x.Amount).GreaterThan(0)` |
| **Bogus** | Gerar valores fictícios usados na preparação dos testes | `faker.Random.Decimal(min: 1, max: 1000)` |
| **xUnit** | Identificar, executar e organizar testes | `[Fact]` |
| **Fluent Assertions** | Expressar expectativas sobre os resultados dos testes | `result.IsValid.Should().BeTrue()` |

### Como elas trabalham juntas

```text
                xUnit
              Teste [Fact]
                   │
                   ▼
             ARRANGE
        Bogus gera a request
                   │
                   ▼
                ACT
        FluentValidation valida
                   │
                   ▼
               ASSERT
      Fluent Assertions verifica
                   │
                   ▼
          Teste passa ou falha
```

O fato de duas bibliotecas terem uma sintaxe “fluente” não significa que tenham a mesma responsabilidade.

---

## 14. Cuidados técnicos e limites da aula

### 14.1 Legibilidade não modifica o resultado esperado

A expressão:

```csharp
result.IsValid.Should().BeTrue();
```

verifica a mesma condição que:

```csharp
Assert.True(result.IsValid);
```

Se `result.IsValid` for `false`, ambas representam uma falha para esse cenário.

### 14.2 O gerador de dados permanece em uso

O **Bogus** continua preenchendo a request. A nova biblioteca atua somente sobre a **verificação do resultado**.

### 14.3 Fluent Assertions não precisa substituir todas as asserções

O vídeo defende a adoção de Fluent Assertions pelas vantagens percebidas pelo instrutor. Isso não significa que os métodos de asserção do xUnit estejam incorretos ou sejam incapazes de verificar coleções e exceções.

### 14.4 Nem toda verificação permite qualquer método

O instrutor observa que a biblioteca facilita o acesso a operações apropriadas ao tipo do valor testado.

Por exemplo, a propriedade `IsValid` é booleana, então `BeTrue()` e `BeFalse()` fazem sentido. Asserções sobre coleções, por sua vez, usam outras operações.

O editor pode ainda sugerir membros herdados de `object` ou outras extensões. É importante distinguir os métodos da biblioteca daqueles oferecidos pelo próprio tipo.

### 14.5 A aula não fixa uma versão de pacote

A transcrição demonstra a instalação via NuGet, mas não registra explicitamente um número de versão. Por isso, não se atribui aqui uma versão específica.

### 14.6 Os testes de erro ainda não foram escritos

Ao final, o instrutor declara que **o cenário de sucesso está concluído**, mas que os testes de erro continuam pendentes.

Não se deve confundir o exemplo de `BeFalse()` ou a menção a testes de exceções com uma implementação já realizada nesta aula.

---

## 15. O que será estudado na próxima aula

O próximo passo anunciado é criar **testes para situações de erro**.

Até aqui, verificamos principalmente um cenário em que a requisição está correta.

Nos próximos testes, será necessário preparar entradas que violem regras específicas, como um título não preenchido ou outro dado inválido, e comparar o resultado com o erro esperado.

### Progressão das aulas

```text
Aula 74 — Introdução ao xUnit
    │
    ▼
Aula 75 — Primeiro teste, [Fact] e AAA
    │
    ▼
Aula 76 — Bogus e CommonTestUtilities
    │
    ▼
Aula 77 — Integração do builder estático
    │
    ▼
Aula 78 — Fluent Assertions e Should().BeTrue()
    │
    ▼
Próxima aula — Testes de erro
```

**O vídeo não mostra a implementação dos testes de erro**; apenas os anuncia.

---

## 16. Resumo

- O teste de sucesso do `RegisterExpenseValidator` já funcionava com `Assert.True(result.IsValid)`.
- O instrutor apresenta **Fluent Assertions** como uma opção de asserções com sintaxe fluente e operações úteis para diferentes tipos.
- O pacote **FluentAssertions** é instalado pelo NuGet no projeto `Validators.Test`.
- O código passa a utilizar `using FluentAssertions;`.
- A asserção é alterada de `Assert.True(result.IsValid)` para `result.IsValid.Should().BeTrue()`.
- `Should()` expressa a expectativa sobre o valor; `BeTrue()` exige que o booleano seja verdadeiro.
- A biblioteca também oferece asserções para strings, coleções, dicionários, datas, enums e exceções.
- O teste continua usando **xUnit** e **Bogus** como antes.
- A aplicação das regras continua a cargo do **FluentValidation**.
- A nova asserção é executada no Visual Studio e o teste de sucesso **continua passando**.
- A etapa seguinte do curso será a implementação dos **testes de erro**.
- **Correção terminológica importante:** `Assert.True` é uma asserção do xUnit, não um método nativo do runtime .NET.

---

## 17. Visão geral

```text
          REGISTER EXPENSE VALIDATOR TESTS
                         │
                      [Fact]
                         │
               ┌─────────┴─────────┐
               │  Arrange / Act    │
               │                   │
               │  Bogus → request  │
               │  Validate(request)│
               └─────────┬─────────┘
                         │
                         ▼
                 result.IsValid
                         │
                 ┌───────┴───────┐
                 │               │
               ANTES           DEPOIS
                 │               │
                 ▼               ▼
         Assert.True(       result.IsValid
         result.IsValid)    .Should().BeTrue()
                 │               │
                 └───────┬───────┘
                         │
                         ▼
                  IsValid = true
                         │
                         ▼
                  TESTE APROVADO

                 PRÓXIMA AULA
                         │
                         ▼
                  TESTES DE ERRO
```

**Conclusão:** a aula mantém a mesma verificação de sucesso do validator, mas substitui a asserção anterior por uma expressão mais próxima da linguagem natural, `result.IsValid.Should().BeTrue()`. O principal aprendizado é reconhecer o papel das bibliotecas de teste e escrever expectativas legíveis, preservando o padrão AAA.
