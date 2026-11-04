# Introdução aos testes unitários com xUnit e Test Explorer

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [O que são testes unitários](#2-o-que-são-testes-unitários)
3. [Por que os testes são importantes](#3-por-que-os-testes-são-importantes)
4. [Testes e prevenção de regressões](#4-testes-e-prevenção-de-regressões)
5. [O que será testado no projeto CashFlow](#5-o-que-será-testado-no-projeto-cashflow)
6. [Testar cenários de sucesso e de erro](#6-testar-cenários-de-sucesso-e-de-erro)
7. [Criando o projeto de testes no Visual Studio](#7-criando-o-projeto-de-testes-no-visual-studio)
8. [Conhecendo o template xUnit](#8-conhecendo-o-template-xunit)
9. [Executando testes pelo Test Explorer](#9-executando-testes-pelo-test-explorer)
10. [Organizando a janela Test Explorer](#10-organizando-a-janela-test-explorer)
11. [Removendo o teste inicial e preparando a próxima aula](#11-removendo-o-teste-inicial-e-preparando-a-próxima-aula)
12. [Cuidados e conceitos para memorizar](#12-cuidados-e-conceitos-para-memorizar)
13. [Resumo](#13-resumo)
14. [Visão geral](#14-visão-geral)

---

## 1. Objetivo da aula

Esta aula marca o início de uma sequência dedicada aos **testes de unidade** no projeto **CashFlow**, desenvolvido em C# e .NET.

O instrutor apresenta a importância dos testes durante o ciclo de vida do software e mostra como preparar o ambiente de desenvolvimento no Visual Studio.

A meta imediata **não é implementar um teste completo**. Primeiro, vamos:

- entender o propósito de testar pequenas unidades de código;
- escolher o framework de testes **xUnit**;
- criar um projeto de testes dentro da pasta `tests`;
- executar o teste de exemplo gerado pelo template;
- conhecer e organizar o **Test Explorer**;
- retirar o teste vazio de exemplo para começar a escrever os testes reais na aula seguinte.

**Contexto:** nas aulas anteriores, criamos validações com FluentValidation. Agora vamos começar a verificar automaticamente se essas validações produzem os resultados esperados.

---

## 2. O que são testes unitários

Um **teste unitário** verifica o comportamento de uma pequena unidade do sistema de forma controlada.

A unidade pode ser um método, uma classe ou outro componente de responsabilidade bem delimitada. O importante é que o teste consiga fornecer entradas e verificar os resultados esperados.

A aula utiliza uma explicação simples:

> Em vez de testar a aplicação inteira de uma vez, vamos começar testando uma pequena parte do código.

No CashFlow, essa parte será uma classe responsável por **validar requisições**.

### Ideia central

```text
         ENTRADA
    Requisição de despesa
             |
             v
  RegisterExpenseValidator
             |
             v
         RESULTADO
      Válido / Inválido
             |
             v
   Comparar com o esperado
             |
     +-------+-------+
     |               |
   Igual           Diferente
     |               |
 Teste passa      Teste falha
```

O objetivo não é somente executar o código: é **verificar se ele se comportou como deveria**.

---

## 3. Por que os testes são importantes

Segundo a aula, testes unitários devem fazer parte do desenvolvimento independentemente da senioridade do profissional ou da linguagem utilizada.

O instrutor destaca um princípio prático: é melhor descobrir um problema durante a execução dos testes do que descobrir o problema por meio de uma reclamação de um cliente em produção.

### O que os testes oferecem

| Benefício | Significado |
|---|---|
| Identificação antecipada de erros | Um comportamento incorreto pode ser detectado antes da entrega. |
| Mais segurança nas alterações | É possível executar testes existentes depois de modificar o código. |
| Documentação de comportamentos esperados | Um teste evidencia o que uma determinada unidade deveria aceitar ou rejeitar. |
| Apoio à manutenção | Alterações futuras podem ser verificadas de maneira repetível. |
| Confiança ao adicionar funcionalidades | Os testes ajudam a perceber se uma nova mudança afetou comportamentos anteriores. |

**Importante:** os testes ajudam a reduzir o risco de defeitos, mas não garantem a inexistência de bugs. A confiabilidade depende da qualidade e da abrangência dos cenários testados.

---

## 4. Testes e prevenção de regressões

Uma **regressão** ocorre quando uma modificação no software faz um comportamento que já funcionava deixar de funcionar.

A aula apresenta duas situações em que devemos voltar a executar nossos testes:

1. **Adição de uma nova funcionalidade:** queremos verificar se a nova implementação não quebrou comportamentos antigos.
2. **Correção de um bug:** queremos garantir que a correção resolveu o problema e não introduziu outros defeitos.

### Exemplo conceitual

Imagine que já exista uma regra impedindo o registro de despesas com valor menor ou igual a zero.

```text
Antes da alteração
   |
   +--> Regra: Amount > 0
   |
   +--> Teste: Amount = 0 deve ser rejeitado
              |
           PASSOU

Nova funcionalidade ou refatoração
   |
   v
Executar o mesmo teste novamente
   |
   +--> PASSOU: comportamento preservado
   |
   +--> FALHOU: possível regressão
```

### E se surgir um bug que nenhum teste encontrou?

A orientação da aula é importante:

- corrigir o defeito;
- investigar por que os testes existentes não o identificaram;
- criar um novo caso de teste que reproduza a situação;
- manter esse teste na suíte para evitar que o mesmo problema retorne.

Essa prática é frequentemente chamada de **teste de regressão**: o teste permanece como uma proteção para um comportamento que não pode voltar a falhar.

---

## 5. O que será testado no projeto CashFlow

O primeiro alvo escolhido pelo instrutor é a **classe validator** criada durante a implementação do cadastro de despesas.

Em aulas anteriores, usamos o **FluentValidation** para definir regras de validação, como:

- título obrigatório;
- valor da despesa maior que zero;
- data não permitida no futuro;
- tipo de pagamento válido.

Nesta nova sequência, a ideia será verificar se o validator responde corretamente quando recebe diferentes objetos de requisição.

### Responsabilidade da unidade testada

```text
Request de cadastro de despesa
          |
          v
  Regras do validator
          |
          v
     ValidationResult
          |
     +----+----+
     |         |
   IsValid    Errors
```

A aula **apenas apresenta esse objetivo**. A criação dos testes de validação fica para a próxima aula.

---

## 6. Testar cenários de sucesso e de erro

A unidade precisa ser verificada tanto quando os dados são válidos quanto quando são inválidos.

Por exemplo, uma validação não está completamente verificada se só testarmos que um título vazio é rejeitado. Também precisamos verificar se um título preenchido é aceito, considerando as demais regras.

| Categoria | Exemplo de cenário a testar futuramente | Resultado esperado |
|---|---|---|
| Sucesso | Requisição com todos os campos válidos | Validação aprovada |
| Falha | Título vazio | Validação rejeitada |
| Falha | Valor igual a zero | Validação rejeitada |
| Falha | Valor negativo | Validação rejeitada |
| Falha | Data futura | Validação rejeitada |
| Falha | Tipo de pagamento inválido | Validação rejeitada |

Esses são **exemplos de cenários derivados das regras já estudadas**, não testes implementados nesta aula.

A recomendação do instrutor é cobrir o máximo de possibilidades relevantes que conseguirmos identificar.

Isso inclui lembrar de cenários que podem parecer simples, mas que provocam comportamentos diferentes.

---

## 7. Criando o projeto de testes no Visual Studio

A parte prática começa na **Solution** do CashFlow.

O projeto já havia sido organizado em pastas de solução como `src` e `tests`. Agora a pasta de testes será usada.

### Passo a passo mostrado na aula

1. No **Solution Explorer**, localizar a pasta **`tests`**.
2. Clicar com o botão direito nela.
3. Escolher a opção de **adicionar um novo projeto**.
4. Na pesquisa de templates, digitar **testes**.
5. Localizar e selecionar o template **xUnit Test Project**.
6. Avançar para as opções de criação.
7. Informar um nome significativo associado ao que será testado, como **`Validators.Test`**.
8. Confirmar que a localização física do novo projeto está dentro da pasta **`tests`**.
9. Selecionar **.NET 8**, que é o framework usado na aula.
10. Criar o projeto.

### Por que criar outro projeto?

O projeto de testes fica separado dos projetos que implementam a API e as regras da aplicação.

```text
CashFlow (Solution)
|
+-- src
|   +-- CashFlow.API
|   +-- CashFlow.Application
|   +-- CashFlow.Communication
|   +-- CashFlow.Domain
|   +-- CashFlow.Exception
|   +-- CashFlow.Infrastructure
|
+-- tests
    +-- Validators.Test
        +-- (arquivos do projeto de testes)
```

**Atenção:** o instrutor também confirma a criação do projeto na pasta `tests` do repositório, e não apenas na organização visual da Solution.

### Por que o nome deve ser significativo?

Um nome como `Validators.Test` comunica que aquele projeto foi criado para armazenar testes relacionados aos validators.

A ideia é facilitar a manutenção à medida que a solução passar a ter várias classes e testes.

---

## 8. Conhecendo o template xUnit

No Visual Studio, aparecem vários templates de projeto de testes, incluindo opções como **NUnit** e **xUnit**.

A escolha específica do instrutor nesta sequência é **xUnit**.

**xUnit** é o framework que será utilizado para escrever e executar os testes da aplicação.

Ao criar o projeto, o Visual Studio gera um arquivo de exemplo, mencionado na aula como um **teste inicial vazio**, geralmente chamado de `UnitTest1.cs` ou similar, dependendo do template.

### O que significa esse teste vazio?

O template oferece uma estrutura inicial para mostrar que o projeto foi criado e que o ambiente de testes pode executar um método de teste.

Um exemplo **ilustrativo** de um teste xUnit sem verificação seria:

```csharp
using Xunit;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Nenhuma verificação do comportamento da aplicação.
    }
}
```

Esse código é **ilustrativo da ideia apresentada**, não uma transcrição literal do arquivo gerado no vídeo.

### Ponto essencial

Um teste executado **não é necessariamente um teste útil**.

Se o método não realiza nenhuma verificação, ele pode aparecer como aprovado sem ter validado uma regra de negócio.

É por isso que a aula usa o teste do template somente para se familiarizar com a ferramenta.

---

## 9. Executando testes pelo Test Explorer

Após criar o projeto, a aula mostra como executar os testes no Visual Studio.

O instrutor acessa o menu:

```text
Test
  |
  +-- Run All Tests
```

No Visual Studio em português, o comando corresponde a **Teste → Executar todos os testes**.

Esse comando dispara a execução dos testes descobertos pela ferramenta.

### O que aparece na execução?

O Visual Studio abre a janela do **Test Explorer**, que permite visualizar os testes e acompanhar os resultados.

Na demonstração, o teste inicial gerado pelo template aparece em verde, indicando execução bem-sucedida.

Entretanto, o instrutor reforça que a aprovação **não representa uma validação real de comportamento**, pois o teste de exemplo está vazio.

### Interpretando os resultados

| Situação | Interpretação |
|---|---|
| Teste aprovado (verde) | O teste terminou sem falhar. |
| Teste reprovado | O teste sinalizou uma falha ou uma exceção não esperada. |
| Teste vazio aprovado | O ambiente executou o teste, mas nenhuma regra foi efetivamente verificada. |

---

## 10. Organizando a janela Test Explorer

A aula também ensina a deixar o Visual Studio organizado para trabalhar com testes.

Ao executar o comando, a janela do Test Explorer pode abrir em uma posição diferente da mostrada pelo instrutor.

A orientação é:

1. Clicar e segurar a barra de título da janela.
2. Arrastar a janela até a área desejada no Visual Studio.
3. Utilizar os indicadores de encaixe (*docking*) que aparecem na interface.
4. Soltar a janela na posição escolhida, como o lado esquerdo.
5. Usar a opção de fixação/recolhimento automático para deixá-la acessível sem ocupar espaço o tempo todo.

Esse ajuste não altera o funcionamento dos testes: serve apenas para melhorar o fluxo de trabalho dentro da IDE.

---

## 11. Removendo o teste inicial e preparando a próxima aula

Depois de confirmar que o teste de exemplo consegue executar, o instrutor abre o projeto recém-criado e **exclui o arquivo inicial de teste**.

### Por que excluir?

Porque o arquivo existe apenas como ponto de partida do template.

O objetivo não é manter um método vazio que sempre fica verde, mas criar testes reais voltados às regras de validação do CashFlow.

O estado ao final desta aula é:

```text
Projeto de testes xUnit criado
                |
                v
      .NET 8 configurado
                |
                v
   Test Explorer apresentado
                |
                v
  Teste inicial executado com sucesso
                |
                v
      Arquivo de exemplo removido
                |
                v
Pronto para criar testes do validator
         (próxima aula)
```

**Nesta aula não são criadas asserções para o `RegisterExpenseValidator`.** A implementação e a explicação passo a passo desses testes começam no vídeo seguinte.

---

## 12. Cuidados e conceitos para memorizar

### Testar não é apenas executar

É necessário definir uma **expectativa** sobre o resultado da unidade testada. Caso contrário, o teste pode ser aprovado sem verificar nada útil.

### Verificar mais de um caminho

Um mesmo validator deve ter seus comportamentos observados em cenários de sucesso e em cenários de falha.

### Preservar testes que reproduzem bugs

Quando surgir um defeito novo, o ideal é acrescentar um teste que capture o caso para ajudar a impedir que ele reapareça depois da correção.

### Separar o projeto de testes

O projeto criado em `tests` organiza a suíte de testes à parte dos projetos de produção.

### Não confundir esta aula com a implementação dos testes

O foco atual é **introdução e preparação do ambiente**. A escrita das verificações reais é conteúdo da aula seguinte.

---

## 13. Resumo

- **Testes unitários** verificam pequenas partes do software.
- São importantes para detectar falhas e apoiar a evolução do projeto.
- Devem ajudar tanto na inclusão de funcionalidades quanto na correção de defeitos.
- Depois de corrigir um bug, vale adicionar um teste para o caso que não estava coberto.
- O primeiro alvo de teste da sequência será o **validator de cadastro de despesas**.
- Devemos considerar **dados válidos e inválidos**.
- No Visual Studio, criamos um projeto usando o template **xUnit Test Project**.
- O projeto é colocado na pasta **`tests`**, usando **.NET 8**.
- **Test Explorer** é a ferramenta usada para visualizar e executar os testes.
- O teste vazio do template pode passar sem verificar nenhuma regra, por isso é removido ao final da aula.
- Os testes efetivos do validator serão implementados **na próxima aula**.

---

## 14. Visão geral

```text
         CASHFLOW
            |
            v
     Testes unitários
            |
      +-----+-----+
      |           |
    Por quê?    Como começar?
      |           |
      v           v
Evitar bugs    Criar projeto
Regressões       xUnit
      |           |
      |           v
      |        .NET 8
      |           |
      |           v
      |      Test Explorer
      |           |
      +-----+-----+
            |
            v
  Próxima aula:
  testar RegisterExpenseValidator
  com cenários de sucesso e erro
```

**Conclusão:** esta aula prepara a estrutura de testes do CashFlow e apresenta a motivação para verificar automaticamente as regras da aplicação. O primeiro exercício prático de implementação fica reservado para a sequência do curso.
