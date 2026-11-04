# Licenciamento do Fluent Assertions 8: alternativas com xUnit, versão 7 e Shouldly

> **Aula 79 — Atualização sobre bibliotecas de asserção em testes C#/.NET**
>
> **Contexto temporal:** a gravação comenta o anúncio ocorrido em **14 de janeiro de 2025**. Valores comerciais, versões disponíveis e condições de licença devem ser compreendidos dentro daquele contexto. Há uma seção de verificação complementar, identificada separadamente, ao final deste resumo.

## 📋 Índice

1. [Objetivo e contexto da aula](#1-objetivo-e-contexto-da-aula)
2. [Relembrando as asserções e sua finalidade](#2-relembrando-as-asserções-e-sua-finalidade)
3. [A mudança anunciada para o Fluent Assertions 8](#3-a-mudança-anunciada-para-o-fluent-assertions-8)
4. [Versão 7 versus versão 8: por que o número importa](#4-versão-7-versus-versão-8-por-que-o-número-importa)
5. [Preço e licenciamento por desenvolvedor no exemplo da aula](#5-preço-e-licenciamento-por-desenvolvedor-no-exemplo-da-aula)
6. [Uso comercial e não comercial: dúvidas levantadas](#6-uso-comercial-e-não-comercial-dúvidas-levantadas)
7. [Primeira opção: utilizar as asserções do xUnit](#7-primeira-opção-utilizar-as-asserções-do-xunit)
8. [Segunda opção: permanecer com Fluent Assertions](#8-segunda-opção-permanecer-com-fluent-assertions)
9. [Fixando uma versão exata no arquivo csproj](#9-fixando-uma-versão-exata-no-arquivo-csproj)
10. [Terceira opção: adotar Shouldly](#10-terceira-opção-adotar-shouldly)
11. [Comparando a sintaxe das três alternativas](#11-comparando-a-sintaxe-das-três-alternativas)
12. [Como substituir encadeamentos And com Shouldly](#12-como-substituir-encadeamentos-and-com-shouldly)
13. [Separação entre framework de testes e bibliotecas de asserção](#13-separação-entre-framework-de-testes-e-bibliotecas-de-asserção)
14. [Tabela comparativa e critérios de decisão](#14-tabela-comparativa-e-critérios-de-decisão)
15. [Cuidados técnicos e diferenças entre o vídeo e a documentação](#15-cuidados-técnicos-e-diferenças-entre-o-vídeo-e-a-documentação)
16. [Verificação complementar em documentação oficial](#16-verificação-complementar-em-documentação-oficial)
17. [Resumo da aula](#17-resumo-da-aula)
18. [Visão geral](#18-visão-geral)

---

## 1. Objetivo e contexto da aula

Na aula anterior, adotamos a biblioteca **Fluent Assertions** para escrever a fase **Assert** do teste unitário com uma sintaxe legível:

```csharp
result.IsValid.Should().BeTrue();
```

Este vídeo é diferente de uma aula comum de implementação: o instrutor interrompe a sequência para explicar uma **alteração de licenciamento** anunciada para o Fluent Assertions em janeiro de 2025.

Ele apresenta três caminhos para quem utiliza a biblioteca em projetos C#:

1. **Deixar de usar Fluent Assertions e escrever asserções com o próprio framework de testes**, como o xUnit.
2. **Continuar utilizando Fluent Assertions**, seja contratando uma licença aplicável às versões comerciais, seja permanecendo na família 7.x.
3. **Migrar para outra biblioteca de asserções**, escolhendo o **Shouldly** como exemplo e como preferência do instrutor.

A intenção da aula é mostrar as consequências de uma dependência externa mudar sua licença e explicar opções técnicas para os testes existentes.

**Não são implementados testes de erro do CashFlow neste vídeo.** Os exemplos apresentados no editor incluem testes de outro projeto de cadastro de usuários, utilizados para explicar as alternativas.

---

## 2. Relembrando as asserções e sua finalidade

Um teste unitário normalmente é dividido em **Arrange, Act e Assert**:

| Etapa | Responsabilidade |
|---|---|
| **Arrange** | Preparar instâncias e dados de entrada |
| **Act** | Executar o código cujo comportamento queremos testar |
| **Assert** | Verificar se o resultado corresponde ao esperado |

A discussão desta aula envolve **somente a forma de escrever as verificações**.

### Exemplo com uma resposta de cadastro

O instrutor apresenta um cenário com uma resposta de registro de usuário, verificando condições como:

- resultado não nulo;
- propriedade de tokens não nula;
- token de acesso preenchido;
- nome retornado igual ao nome informado na requisição.

O exemplo tem o seguinte sentido:

```text
Executar o use case de cadastro
                |
                v
           Receber result
                |
                v
      result não deve ser null
                |
                v
      tokens não deve ser null
                |
                v
      access token não deve estar vazio
                |
                v
      nome retornado = nome enviado
                |
                v
        Teste aprovado ou falho
```

O instrutor gosta da sintaxe fluente porque ela se aproxima da leitura de uma frase e torna explícita a intenção de cada teste.

---

## 3. A mudança anunciada para o Fluent Assertions 8

Segundo o relato da aula, em **14 de janeiro de 2025** foi divulgada uma alteração nos termos de utilização do **Fluent Assertions**, relacionada à nova versão principal **8** e à colaboração com a **Xceed Software**.

O instrutor explica que a família 8 passou a ter condições diferentes para **uso comercial**.

Isso importa porque o Fluent Assertions é uma dependência utilizada nos testes e não apenas uma ferramenta visual do editor.

### Por que uma mudança na licença pode afetar um projeto?

Porque bibliotecas de terceiros possuem regras para distribuição, utilização e, em alguns casos, aquisição de direitos de uso.

Uma equipe pode ter criado centenas de testes usando:

```csharp
result.Should().NotBeNull();
name.Should().Be(request.Name);
```

e precisar avaliar se a utilização da versão instalada continua permitida pelas condições aplicáveis.

**O código pode continuar compilando mesmo que as condições de licença mudem.** Portanto, ausência de erro de compilação não prova que a utilização esteja juridicamente autorizada.

### Reação da comunidade relatada no vídeo

O instrutor mostra um anúncio, alterações no repositório e comentários de desenvolvedores insatisfeitos com a decisão.

Na gravação, são mencionadas reações negativas em um pull request que alterou informações sobre licença e propriedade do projeto.

Essas contagens de reações são um **registro do momento em que o vídeo foi produzido**, não números atuais.

A insatisfação e a desconfiança expressas pelo professor são **avaliações pessoais dele**, e devem ser separadas das regras efetivas da licença.

---

## 4. Versão 7 versus versão 8: por que o número importa

Esta é uma das distinções centrais da aula.

| Família do Fluent Assertions | Situação apresentada |
|---|---|
| **7.x e anteriores** | Permanecem sob o licenciamento aberto que já possuíam, permitindo o uso conforme esses termos |
| **8.x e posteriores** | Passam a seguir os termos de licença introduzidos na versão 8, incluindo exigências comerciais |

### Por que versões anteriores não mudam automaticamente?

O instrutor explica que a introdução de novos termos em uma versão **não significa que todos os pacotes anteriores passaram a ter retroativamente aqueles mesmos termos**.

No caso da versão 7, o licenciamento **Apache 2.0** permanece aplicável àquela família, incluindo seu uso comercial, conforme os termos da licença.

Portanto, a decisão técnica pode depender da versão instalada:

```text
FluentAssertions 7.x
        |
        └── Licença Apache 2.0

FluentAssertions 8.x+
        |
        └── Modelo de licença introduzido na versão 8
            ├── Usos gratuitos elegíveis
            └── Licença comercial quando exigida
```

**Atenção:** dizer que a versão 8 é “paga” sem qualificação é impreciso. O próprio instrutor esclarece depois que existem hipóteses de utilização gratuita.

---

## 5. Preço e licenciamento por desenvolvedor no exemplo da aula

O instrutor visita o site comercial da Xceed e mostra, **na ocasião da gravação**, uma oferta de **US$ 129,95** por licença, arredondada por ele para **US$ 130**.

Ele usa um exemplo de equipe:

```text
10 desenvolvedores
× US$ 130 por licença
= US$ 1.300
```

Na exposição da aula, esse é o valor anual aproximado para dez desenvolvedores no **tipo de assinatura apresentado**.

O vídeo explica que o licenciamento comercial é associado a desenvolvedores ou assentos (*seats*).

### O que não devemos concluir desse exemplo

O cálculo apresentado **não demonstra que todas as modalidades de licença, para todas as empresas, têm exatamente o mesmo preço**.

O valor é o da oferta consultada no vídeo. Preços, descontos, tipos de licença e períodos contratados precisam ser verificados nas condições atuais do fornecedor.

---

## 6. Uso comercial e não comercial: dúvidas levantadas

O professor lê trechos dos novos termos de uso e explica que existem hipóteses de utilização gratuita, como projetos **pessoais e experimentais**, além de outras situações elegíveis previstas na licença.

Ele também destaca redações relacionadas ao uso por organizações que cobram taxas ou recebem receitas.

### O exemplo discutido na aula

O instrutor imagina uma situação:

1. Uma pessoa cria uma aplicação apenas para demonstrar uma biblioteca.
2. Publica um vídeo educacional gratuito explicando a implementação.
3. Não vende a aplicação demonstrada.
4. Porém, recebe alguma receita de publicidade gerada pelo vídeo.

A dúvida levantada é se a monetização do conteúdo educacional poderia alterar a classificação do uso da biblioteca sob aqueles termos.

### O que a aula efetivamente conclui?

**O próprio instrutor afirma que não tem certeza da interpretação jurídica.** Ele comenta a redação da licença e menciona discussões da comunidade solicitando esclarecimentos.

Isso deve ser tratado como **uma dúvida e um exemplo hipotético**, não como decisão jurídica definitiva de que todo vídeo monetizado exige a compra de licença.

Para um projeto real, a classificação do uso precisa ser confrontada com o texto oficial aplicável e, em situações ambíguas, com orientação adequada.

---

## 7. Primeira opção: utilizar as asserções do xUnit

A primeira alternativa apresentada é **remover a dependência do Fluent Assertions** e utilizar as asserções já disponibilizadas pelo framework de testes.

Na demonstração, o framework utilizado é o **xUnit**.

### Testando valores não nulos

Com Fluent Assertions:

```csharp
result.Should().NotBeNull();
```

Com xUnit:

```csharp
Assert.NotNull(result);
```

### Testando a igualdade de strings

Com Fluent Assertions:

```csharp
result.Name.Should().Be(request.Name);
```

Com xUnit:

```csharp
Assert.Equal(request.Name, result.Name);
```

### Testando um booleano

Com Fluent Assertions:

```csharp
result.IsValid.Should().BeTrue();
```

Com xUnit:

```csharp
Assert.True(result.IsValid);
```

Os dois estilos permitem expressar as expectativas do teste.

### Vantagens dessa opção

- Não exigir um **pacote adicional de asserções**.
- Permanecer com as APIs de teste já fornecidas pelo xUnit.
- Evitar a dependência de licenciamento de uma biblioteca extra para essas verificações.

### Desvantagens apontadas pelo professor

O instrutor prefere a leitura das expressões fluentes e comenta que, em cenários com muitas verificações, o código com chamadas separadas de `Assert` pode ficar mais extenso verticalmente.

Essas observações envolvem **preferência de legibilidade e ergonomia**, não incapacidade técnica do xUnit.

### Correção de terminologia

A transcrição chama `Assert.NotNull` e `Assert.Equal` de “asserções nativas do .NET”. **Tecnicamente, são métodos da biblioteca de asserções do xUnit**, disponível no projeto por meio das dependências do framework de testes.

Não são métodos fornecidos isoladamente pelo runtime base do .NET.

---

## 8. Segunda opção: permanecer com Fluent Assertions

A segunda opção não exige trocar a sintaxe dos testes existentes.

O instrutor divide essa alternativa em **dois caminhos**.

### Caminho A — Utilizar uma licença comercial aplicável

Uma equipe pode decidir continuar utilizando uma versão moderna do Fluent Assertions e adquirir os direitos de uso exigidos para seu cenário.

Esse caminho pode fazer sentido quando a empresa prefere manter o código existente e continuar recebendo as novas versões da biblioteca.

O vídeo não condena tecnicamente essa decisão: o instrutor a reconhece como uma opção para equipes que desejam e podem adotar o produto pago.

### Caminho B — Permanecer na família 7.x

A segunda possibilidade é continuar utilizando a versão 7, que mantém os termos abertos anteriores.

Isso evita ter de reescrever imediatamente uma suíte de testes que já utiliza Fluent Assertions.

**Trade-off apresentado pelo professor:** permanecer indefinidamente em uma versão principal mais antiga pode significar não adotar recursos introduzidos em versões futuras.

O vídeo menciona a promessa de manutenção de correções relevantes na versão 7, embora o instrutor manifeste desconfiança pessoal na continuidade desse trabalho.

Essa desconfiança é uma **opinião do instrutor**; não deve ser tratada como prova de que as correções deixaram de ocorrer.

---

## 9. Fixando uma versão exata no arquivo csproj

Se uma equipe decidir manter o Fluent Assertions 7, o vídeo ensina uma técnica para **impedir que uma atualização comum do NuGet selecione outra versão**.

### Situação apresentada

Na gravação, o projeto `UseCases.Test` usa:

```xml
<PackageReference Include="FluentAssertions" Version="7.1.0" />
```

O Visual Studio apresenta uma versão 8 mais recente na aba de atualizações.

O instrutor quer que o projeto continue usando **exatamente a versão 7.1.0**.

### Sintaxe apresentada na aula

```xml
<PackageReference Include="FluentAssertions" Version="[7.1.0]" />
```

Os **colchetes** fazem parte da sintaxe de intervalos de versão do NuGet.

Neste caso, `[7.1.0]` representa um intervalo fechado em um único valor: somente a versão **7.1.0** satisfaz a restrição.

### Comparação

| Sintaxe | Interpretação principal |
|---|---|
| `Version="7.1.0"` | A dependência declara 7.1.0 como versão mínima no modelo de intervalo usado pelo NuGet |
| `Version="[7.1.0]"` | A dependência exige **exatamente** a versão 7.1.0 |

### Por que isso ajuda?

O propósito da alteração é evitar que alguém selecione acidentalmente uma versão principal diferente durante uma atualização de pacotes.

Na aula, depois de salvar o `.csproj`, o instrutor observa que o Fluent Assertions deixa de aparecer como uma atualização comum sugerida pelo gerenciador do Visual Studio.

### Importante: isso não é uma proteção impossível de contornar

A versão exata é uma **restrição de resolução de dependências**, não uma trava de segurança imutável.

Uma pessoa ainda pode editar o arquivo `.csproj` e trocar o valor da referência. Por isso, equipes que precisam controlar rigorosamente upgrades podem combinar o pin com revisão de código, políticas de atualização e verificação de dependências.

### Visualizando a mudança

```text
ANTES
PackageReference Version="7.1.0"
              |
              v
NuGet pode sugerir versão mais recente

DEPOIS
PackageReference Version="[7.1.0]"
              |
              v
Restrição para resolver exatamente 7.1.0
```

**Detalhe de contexto:** o instrutor usa `UseCases.Test` como exemplo de arquivo de projeto, e não o projeto `Validators.Test` usado nas primeiras aulas de validação.

---

## 10. Terceira opção: adotar Shouldly

A terceira solução — a escolha pessoal do instrutor para os projetos que pretende desenvolver dali em diante — é a biblioteca **Shouldly**.

Assim como Fluent Assertions, Shouldly oferece uma API de asserções que busca comunicar as expectativas do teste de forma legível.

A sintaxe é diferente, mas mantém a ideia de escrever verificações diretamente sobre os valores.

### Instalando o Shouldly

O professor demonstra os passos no Visual Studio:

1. Selecionar o projeto de testes.
2. Abrir **Manage NuGet Packages**.
3. Selecionar a aba **Browse**.
4. Pesquisar por `Shouldly`.
5. Instalar o pacote correspondente.
6. Aceitar as operações solicitadas pelo gerenciador de pacotes.

Em seguida, a classe de teste passa a utilizar:

```csharp
using Shouldly;
```

### Exemplo 1 — Valor não nulo

**Fluent Assertions:**

```csharp
result.Should().NotBeNull();
```

**Shouldly:**

```csharp
result.ShouldNotBeNull();
```

### Exemplo 2 — Igualdade

**Fluent Assertions:**

```csharp
result.Name.Should().Be(request.Name);
```

**Shouldly:**

```csharp
result.Name.ShouldBe(request.Name);
```

### Diferença essencial

No Fluent Assertions, a chamada geralmente contém o método intermediário `Should()`.

No Shouldly, a própria operação de asserção costuma começar por `Should`, por exemplo:

```text
Fluent Assertions  → value.Should().NotBeNull()

Shouldly           → value.ShouldNotBeNull()
```

O professor destaca que o Shouldly exige adaptar a sintaxe ao migrar os testes.

---

## 11. Comparando a sintaxe das três alternativas

Os exemplos a seguir são didáticos e têm como finalidade mostrar equivalências de intenção; não representam uma tradução automática para todas as APIs.

| Objetivo | xUnit | Fluent Assertions | Shouldly |
|---|---|---|---|
| Booleano verdadeiro | `Assert.True(value)` | `value.Should().BeTrue()` | `value.ShouldBeTrue()` |
| Booleano falso | `Assert.False(value)` | `value.Should().BeFalse()` | `value.ShouldBeFalse()` |
| Não nulo | `Assert.NotNull(value)` | `value.Should().NotBeNull()` | `value.ShouldNotBeNull()` |
| Igualdade | `Assert.Equal(expected, actual)` | `actual.Should().Be(expected)` | `actual.ShouldBe(expected)` |

### Exemplo no teste do validator

O teste trabalhado nas aulas anteriores verifica:

```csharp
var result = validator.Validate(request);
```

As três versões da asserção seriam:

```csharp
// xUnit
Assert.True(result.IsValid);

// Fluent Assertions
result.IsValid.Should().BeTrue();

// Shouldly
result.IsValid.ShouldBeTrue();
```

O **resultado esperado é o mesmo**. O que muda é a biblioteca e a forma de escrever.

### Exemplo de teste com Shouldly

```csharp
using Shouldly;
using Xunit;

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
        result.IsValid.ShouldBeTrue();
    }
}
```

**Exemplo complementar:** o código acima mostra como seria a adaptação do teste do CashFlow. O vídeo usa exemplos de outros projetos na demonstração da troca de bibliotecas; não afirma que esse arquivo específico do CashFlow já foi migrado.

---

## 12. Como substituir encadeamentos And com Shouldly

O professor chama atenção para uma diferença entre as duas bibliotecas.

No Fluent Assertions, ele mostra uma verificação encadeada, com duas condições sobre o mesmo valor, em um formato conceitualmente semelhante a:

```csharp
name.Should()
    .NotBeNullOrWhiteSpace()
    .And.Be(request.Name);
```

A primeira condição verifica que o texto não é nulo, vazio ou composto apenas por espaços em branco.

A segunda verifica que ele corresponde ao nome esperado.

### O Shouldly não utiliza o mesmo `.And` do Fluent Assertions

A sintaxe direta do Shouldly seria escrever duas verificações:

```csharp
name.ShouldNotBeNullOrWhiteSpace();
name.ShouldBe(request.Name);
```

Isso já permite testar as duas condições; apenas não utiliza o mesmo encadeamento.

### Agrupando condições com ShouldSatisfyAllConditions

A alternativa demonstrada pelo instrutor é a operação:

```csharp
name.ShouldSatisfyAllConditions(
    n => n.ShouldNotBeNullOrWhiteSpace(),
    n => n.ShouldBe(request.Name)
);
```

**Leitura:** o valor `name` deve satisfazer **todas** as condições informadas.

Essa API permite declarar múltiplas verificações sobre o mesmo objeto.

A documentação do Shouldly também demonstra uma forma baseada em delegados sem parâmetro, semelhante a:

```csharp
name.ShouldSatisfyAllConditions(
    () => name.ShouldNotBeNullOrWhiteSpace(),
    () => name.ShouldBe(request.Name)
);
```

Ambas as formas ilustram o agrupamento oferecido pelo Shouldly; o uso depende da sobrecarga escolhida.

### Diferença importante

**Não são sintaxes idênticas.**

O `.And` do Fluent Assertions permite continuar uma cadeia fluente de operações.

O `ShouldSatisfyAllConditions` do Shouldly **agrupa condições que devem ser atendidas**. É uma alternativa funcional para o cenário apresentado, mas não deve ser tratado como um substituto idêntico para todo encadeamento disponível em Fluent Assertions.

---

## 13. Separação entre framework de testes e bibliotecas de asserção

A aula reforça uma distinção importante: mudar a biblioteca de asserções **não obriga a mudar o framework que executa os testes**.

| Ferramenta | Papel |
|---|---|
| **xUnit** | Descobrir e executar testes, utilizando recursos como `[Fact]` |
| **Fluent Assertions** | Declarar expectativas em sintaxe fluente |
| **Shouldly** | Declarar expectativas com outra API de asserções |
| **Bogus** | Gerar dados fictícios para preparar as entradas |
| **FluentValidation** | Aplicar regras de validação aos dados |

O projeto pode continuar utilizando xUnit e Bogus independentemente da escolha entre Fluent Assertions e Shouldly.

```text
                    xUnit
                  Teste [Fact]
                       │
                       ▼
                    Arrange
               Bogus gera request
                       │
                       ▼
                      Act
             Validator aplica regras
                       │
                       ▼
                     Assert
             ┌─────────┼─────────┐
             │         │         │
           xUnit    Fluent     Shouldly
                    Assertions
             │         │         │
             └─────────┴─────────┘
                       │
                       ▼
                Passa ou falha
```

---

## 14. Tabela comparativa e critérios de decisão

| Alternativa | Principal vantagem mencionada | Trade-off |
|---|---|---|
| **1. Usar os asserts do xUnit** | Não precisar de uma biblioteca adicional de asserções | Sintaxe diferente da preferida pelo professor; algumas verificações exigem mais chamadas |
| **2A. Continuar Fluent Assertions com licença comercial** | Manter a API existente e poder utilizar versões comerciais atuais | Considerar custos e condições aplicáveis |
| **2B. Permanecer no Fluent Assertions 7.x** | Preservar o código e a licença aberta anterior | Abrir mão de recursos introduzidos em versões principais posteriores e acompanhar manutenção da versão |
| **3. Migrar para Shouldly** | Continuar com uma sintaxe legível e alternativa de asserções | Adaptar os testes existentes e reaprender diferenças da API |

### Qual foi a escolha do instrutor?

O professor declara preferência pela **terceira alternativa, Shouldly**, porque quer manter a escrita fluente das asserções sem depender do modelo comercial que passou a ser aplicado ao Fluent Assertions 8.

Entretanto, ele também reconhece que migrar uma suíte grande de testes pode exigir trabalho significativo.

**A decisão final da equipe deve considerar código existente, manutenção, licenciamento, custo e legibilidade.**

---

## 15. Cuidados técnicos e diferenças entre o vídeo e a documentação

### 15.1 O vídeo foi gravado logo após a mudança de 2025

A gravação reflete aquele contexto. Ela não deve ser usada como única fonte para saber os preços, versões ou condições oferecidos no presente.

### 15.2 Asserts de xUnit não são APIs nativas do runtime

A expressão `Assert.NotNull(result)` pertence ao xUnit. Ela pode ser usada sem instalar **outro pacote de asserções**, porque o projeto xUnit já tem as dependências correspondentes.

### 15.3 Exatamente 7.1.0 não é o mesmo que qualquer versão 7

O pin:

```xml
Version="[7.1.0]"
```

exige exatamente **7.1.0**. Ele não aceita automaticamente uma futura versão `7.1.1` ou outra atualização de manutenção.

Portanto, manter a família 7.x sem migrar para 8.x e fixar **uma única versão** são duas estratégias relacionadas, mas não idênticas.

### 15.4 Licenciamento não é apenas uma questão de compilar

Uma aplicação pode funcionar corretamente e, ainda assim, exigir avaliação das obrigações de uso do pacote.

### 15.5 Asserções podem ser mais ou menos convenientes sem serem “erradas”

Uma forma fluente pode ser preferida pela equipe por clareza, mas os métodos do xUnit continuam sendo uma opção válida.

### 15.6 Não converter opinião em fato

No vídeo, o instrutor expressa desconfiança em relação à continuidade da manutenção da versão 7 e comenta dúvidas sobre monetização de vídeos educativos.

Esses comentários representam **avaliações e dúvidas dele**, não comprovações de abandono da versão 7 ou interpretações legais definitivas.

---

## 16. Verificação complementar em documentação oficial

> **Conteúdo de verificação externa, separado da transcrição da aula.**
>
> As informações abaixo foram conferidas em documentação pública do projeto e do fornecedor, para distinguir as condições que continuam documentadas do relato histórico da gravação.

### Licenciamento do Fluent Assertions

A documentação oficial do Fluent Assertions informa que:

- a **versão 7 permanece aberta** e recebe correções de bugs e outras correções importantes;
- as versões **8 ou posteriores** exigem licença paga para **uso comercial**, embora haja hipóteses permitidas de uso gratuito, incluindo projetos abertos e não comerciais.

Fontes:
- [Fluent Assertions — Introduction / Licensing](https://fluentassertions.com/introduction)
- [Xceed — Fluent Assertions FAQ](https://xceed.com/fluent-assertions-faq/)
- [Repositório Fluent Assertions — licença](https://github.com/fluentassertions/fluentassertions/blob/main/LICENSE)

### Sobre o preço

O vídeo apresenta **US$ 129,95** como preço da licença examinada em 2025.

Na página comercial consultada durante a preparação deste resumo, a Xceed também apresenta **modalidades diferentes**, incluindo a opção **Basic** e uma assinatura **Standard**. Portanto, o preço mostrado na aula não deve ser interpretado como o único valor possível para qualquer usuário.

Fonte: [Xceed — Fluent Assertions, produtos e preços](https://xceed.com/products/unit-testing/fluent-assertions/).

### Sobre Shouldly

A documentação do Shouldly descreve o método `ShouldSatisfyAllConditions` e apresenta exemplos de agrupamento de verificações.

O repositório também publica uma licença **BSD de 3 cláusulas**, distinta do modelo comercial discutido no vídeo.

Fontes:
- [Shouldly — ShouldSatisfyAllConditions](https://docs.shouldly.org/documentation/satisfyallconditions)
- [Shouldly — licença no GitHub](https://github.com/shouldly/shouldly/blob/master/LICENSE.txt)

**Nota:** a existência de uma modalidade gratuita ou aberta não dispensa cumprir os termos correspondentes. Uma avaliação jurídica depende do caso de uso concreto.

---

## 17. Resumo da aula

- O vídeo relata a **mudança de licenciamento do Fluent Assertions anunciada em janeiro de 2025**.
- A discussão se concentra na família **8.x**, que passou a adotar condições comerciais diferentes.
- A família **7.x** continua disponível sob a licença aberta anterior, permitindo seu uso conforme esses termos.
- O professor apresenta um preço histórico de **US$ 129,95** e ilustra o impacto para uma equipe de dez desenvolvedores.
- O uso não comercial pode ser permitido sem pagamento, mas precisa atender às definições específicas da licença.
- O exemplo do vídeo monetizado no YouTube é uma **dúvida levantada pelo instrutor**, e não uma conclusão jurídica.
- **Primeira alternativa:** usar asserções do próprio xUnit, como `Assert.NotNull`, `Assert.Equal` e `Assert.True`.
- **Segunda alternativa:** continuar com Fluent Assertions, pagando a licença aplicável ou permanecendo em 7.x.
- Para fixar uma versão exata, a aula apresenta `Version="[7.1.0]"` em um `.csproj`.
- **Terceira alternativa:** utilizar a biblioteca **Shouldly**, preferida pelo instrutor.
- O Shouldly utiliza métodos como `ShouldNotBeNull()`, `ShouldBe(expected)` e `ShouldBeTrue()`.
- Para agrupar múltiplas expectativas, o Shouldly dispõe de `ShouldSatisfyAllConditions`.
- O **xUnit continua sendo o framework de testes**, independentemente da biblioteca de asserções escolhida.
- Esta é uma aula sobre **licenciamento e escolha de dependências**, e não uma aula de implementação de novos testes de erro.

---

## 18. Visão geral

```text
       AULA 78 — FLUENT ASSERTIONS
                    │
                    ▼
     result.IsValid.Should().BeTrue()
                    │
                    ▼
       AULA 79 — MUDANÇA DE LICENÇA
                    │
           Fluent Assertions
             versão 8+
                    │
             uso comercial
                    │
         ┌──────────┼──────────────┐
         ▼          ▼              ▼
      OPÇÃO 1    OPÇÃO 2        OPÇÃO 3
      xUnit      Manter FA      Shouldly
         │          │              │
    Assert.True  ┌──┴──┐      ShouldBeTrue()
         │       ▼     ▼           │
         │     Licença v7.x         │
         │     comercial Apache 2.0 │
         │             │           │
         │        [7.1.0]         │
         │        versão fixa      │
         └──────────┬──────────────┘
                    │
                    ▼
       CONTINUAR TESTANDO
       COM ASSERÇÕES CLARAS
       E LICENÇA ADEQUADA
```

**Conclusão:** a principal lição não é apenas conhecer mais uma biblioteca de testes, mas compreender que **dependências externas têm implicações técnicas e de licenciamento**. A equipe deve poder escolher entre manter a biblioteca existente, controlar a versão utilizada ou migrar para outra alternativa, considerando os requisitos do projeto e os termos de uso aplicáveis.
