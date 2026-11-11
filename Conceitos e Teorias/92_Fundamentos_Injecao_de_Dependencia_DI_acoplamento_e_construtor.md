# Fundamentos da injeção de dependência: dependências, acoplamento e injeção pelo construtor

> **Aula 92 — Introdução teórica à Dependency Injection (DI)**
>
> Continuação da aula 91, em que testamos a persistência com `CashFlowDbContext` por meio de uma referência temporária da Application à Infrastructure. Nesta aula, o professor começa uma sequência sobre injeção de dependência e explica **por que uma classe não deveria precisar conhecer todos os detalhes de criação de outra classe para utilizá-la**.

## Índice

1. [Objetivo e contexto](#1-objetivo-e-contexto)
2. [O que é uma dependência?](#2-o-que-é-uma-dependência)
3. [Exemplo inicial: Classe A cria Classe B](#3-exemplo-inicial-classe-a-cria-classe-b)
4. [Quando a dependência passa a exigir configuração](#4-quando-a-dependência-passa-a-exigir-configuração)
5. [O que é acoplamento?](#5-o-que-é-acoplamento)
6. [Solução: receber a dependência no construtor](#6-solução-receber-a-dependência-no-construtor)
7. [Quem cria a Classe B?](#7-quem-cria-a-classe-b)
8. [Como o container resolve uma cadeia de dependências](#8-como-o-container-resolve-uma-cadeia-de-dependências)
9. [Transient, Scoped e Singleton: opções anunciadas](#9-transient-scoped-e-singleton-opções-anunciadas)
10. [Relação com a arquitetura do CashFlow](#10-relação-com-a-arquitetura-do-cashflow)
11. [Distinções e cuidados técnicos](#11-distinções-e-cuidados-técnicos)
12. [Resumo e perguntas de revisão](#12-resumo-e-perguntas-de-revisão)
13. [Mapa mental](#13-mapa-mental)

---

## 1. Objetivo e contexto

O professor inicia uma **sequência de aulas sobre injeção de dependência**, que terá tanto explicações teóricas quanto demonstrações práticas.

Nesta primeira aula, ele apresenta:

- o conceito de dependência entre classes;
- o problema de criar dependências concretas diretamente com `new`;
- a dificuldade que surge quando os objetos dependentes exigem configurações;
- a noção de acoplamento;
- o uso de injeção de dependência pelo construtor;
- o papel de um serviço/container de injeção de dependência;
- a existência dos ciclos de vida `Transient`, `Scoped` e `Singleton`.

O professor reforça que o assunto é frequente em entrevistas técnicas.

**Escopo:** a aula é essencialmente conceitual. O instrutor **não configura ainda os serviços de DI no projeto CashFlow**. A prática de registro e o ciclo de vida Singleton ficam para a próxima aula.

### Por que esse tema aparece neste momento do curso?

Na aula 91, conseguimos gravar uma despesa no MySQL instanciando `CashFlowDbContext` diretamente no use case. No entanto, a Application precisou referenciar a Infrastructure, e o professor removeu essa dependência ao finalizar o teste.

Agora começa a explicação de como fornecer as dependências à aplicação sem fazer com que seus componentes tenham de construir manualmente todos os objetos de que precisam.

---

## 2. O que é uma dependência?

Uma **dependência** é algo de que uma classe precisa para desempenhar determinada responsabilidade.

O exemplo da aula usa duas classes imaginárias: **A** e **B**.

A classe A precisa utilizar um método disponibilizado pela classe B.

```text
Classe A
   |
   | utiliza
   v
Classe B
```

Portanto, A **depende de B** para executar o comportamento que foi programado.

### Exemplo cotidiano

Se um serviço de cadastro precisa enviar um e-mail, ele depende de algum mecanismo capaz de enviar e-mails.

Se um caso de uso precisa salvar uma despesa, depende de algum mecanismo de persistência.

A dependência pode ser simples, como uma classe com um método que retorna uma mensagem, ou mais complexa, como um componente responsável por conexões com banco de dados.

**Conceito central:** dependência significa precisar de outro componente. Isso não é necessariamente um problema; a dificuldade está em **como a dependência é criada e gerenciada**.

---

## 3. Exemplo inicial: Classe A cria Classe B

A gravação apresenta um exemplo simples.

A Classe B possui um método `Execute` que retorna uma mensagem semelhante a `"hello"`.

A Classe A, dentro de seu próprio método `Execute`, cria um objeto da Classe B com `new` e chama o método dela.

### Reconstrução didática do exemplo

```csharp
public class ClasseB
{
    public string Execute()
    {
        return "hello";
    }
}

public class ClasseA
{
    public void Execute()
    {
        var classeB = new ClasseB();

        var result = classeB.Execute();

        Console.WriteLine(result);
    }
}
```

A transcrição descreve o método `Execute` de A como `void`, uma variável `result` e o retorno `"hello"` em B. O exemplo acima reúne esses elementos de modo compilável; não é uma cópia literal do código projetado no vídeo.

### O que acontece?

1. Uma instância de Classe A é utilizada.
2. O método `ClasseA.Execute()` começa sua execução.
3. A classe A executa `new ClasseB()`.
4. A nova instância de B é criada.
5. A chama `ClasseB.Execute()`.
6. A utiliza o resultado devolvido por B.

```text
ClasseA.Execute()
       |
       v
 new ClasseB()
       |
       v
ClasseB.Execute()
       |
       v
     "hello"
```

### Onde está a dependência?

A linha mais importante é:

```csharp
var classeB = new ClasseB();
```

A Classe A precisa **conhecer e criar concretamente** a Classe B.

No exemplo inicial isso parece simples porque B não exige informações para ser construída.

O professor usa essa situação para introduzir o problema que surge quando B se torna mais complexa.

---

## 4. Quando a dependência passa a exigir configuração

O instrutor substitui a Classe B simples por uma classe responsável pelo **acesso ao banco de dados**.

Essa nova Classe B precisa de uma **connection string** para estabelecer a conexão.

Nas aulas anteriores aprendemos que a connection string informa dados como:

- endereço do servidor;
- banco de dados;
- usuário;
- credenciais necessárias à conexão.

### Classe B com construtor

O professor mostra que a classe recebe a connection string em seu construtor e a guarda em um campo privado.

Uma representação didática da estrutura é:

```csharp
public class ClasseB
{
    private readonly string _connectionString;

    public ClasseB(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string Execute()
    {
        // A implementação real de acesso ao banco
        // usaria _connectionString.
        return "Operação executada";
    }
}
```

O vídeo utiliza um exemplo de conexão SQL. O código acima representa apenas **o papel do construtor e do campo privado**, sem simular uma conexão real e sem afirmar que a implementação completa aparece na transcrição.

### Por que `_connectionString`?

O instrutor emprega o prefixo `_` como convenção para um membro privado da classe.

```csharp
private readonly string _connectionString;
```

A variável representa um **campo privado**. O professor a chama informalmente de propriedade, mas tecnicamente não é uma propriedade C# do tipo `{ get; set; }`.

O valor recebido no construtor fica disponível para ser usado pelos outros métodos da instância.

### O que mudou para a Classe A?

Antes, A podia construir B assim:

```csharp
new ClasseB();
```

Agora, a construção exige uma informação:

```csharp
new ClasseB(connectionString);
```

Consequentemente, se A insistir em criar B diretamente, **A também precisará saber como obter a connection string**.

```text
Classe A
   |
   +-- precisa saber criar Classe B
   |
   +-- precisa obter connection string
   |
   +-- precisa repassar configuração
   |
   v
Classe B
   |
   v
Conexão ao banco
```

Esse é o problema que o professor pretende evitar.

---

## 5. O que é acoplamento?

**Acoplamento** é o grau de dependência entre componentes de um software.

O professor destaca o **acoplamento forte** que surge quando A precisa conhecer muitos detalhes de construção e configuração de B.

### O problema não é usar outra classe

A aplicação normalmente é composta por classes que colaboram entre si.

O problema é fazer uma classe conhecer detalhes que não fazem parte de sua responsabilidade.

No exemplo:

> A Classe A só precisa utilizar o método da Classe B. Ela não deveria precisar saber os detalhes de conexão usados internamente pela B.

### Consequências apresentadas

À medida que o software cresce, podem surgir dependências para:

- acessar um banco de dados;
- enviar e-mails;
- fazer upload de imagens;
- realizar outras operações que exigem configuração.

Se toda classe precisar criar e configurar manualmente os componentes que utiliza, mais detalhes de infraestrutura acabam espalhados pelo código.

```text
Classe A
   |
   +-- configura banco
   |
   +-- configura envio de e-mail
   |
   +-- configura upload
   |
   v
Mais responsabilidades e maior acoplamento
```

### O que o professor deseja?

A classe consumidora deve concentrar-se em **utilizar o comportamento necessário**.

O professor resume essa ideia informalmente como **"just call"**: a classe deseja chamar o método de que precisa, sem conhecer todos os detalhes de como a dependência foi configurada.

### Comparação

| Criação direta dentro da classe | Dependência fornecida externamente |
|---|---|
| A cria B com `new` | A recebe uma instância de B |
| A pode precisar conhecer a configuração de B | A não precisa construir B |
| Mudanças na criação de B podem atingir A | A pode permanecer focada no uso de B |
| Construção e utilização ficam misturadas | Construção e utilização ficam separadas |

**Nuance:** receber uma instância concreta de B reduz o acoplamento à **criação e configuração** de B, mas A ainda conhece o tipo concreto `ClasseB`. Para desacoplar também a implementação concreta, pode-se depender de uma abstração, como uma interface. Essa distinção será aprofundada na seção de observações técnicas.

---

## 6. Solução: receber a dependência no construtor

O instrutor apresenta uma modificação na Classe A.

Em vez de criar B internamente, A passa a **receber a instância de B como parâmetro do construtor**.

### Exemplo reconstruído

```csharp
public class ClasseA
{
    private readonly ClasseB _classeB;

    public ClasseA(ClasseB classeB)
    {
        _classeB = classeB;
    }

    public void Execute()
    {
        var result = _classeB.Execute();

        Console.WriteLine(result);
    }
}
```

### O que mudou?

**Antes:**

```csharp
var classeB = new ClasseB();
var result = classeB.Execute();
```

**Depois:**

```csharp
var result = _classeB.Execute();
```

A responsabilidade de criar B foi retirada do método de A.

### Entendendo o construtor

```csharp
public ClasseA(ClasseB classeB)
{
    _classeB = classeB;
}
```

1. O parâmetro `classeB` recebe uma instância de B.
2. A atribuição `_classeB = classeB` guarda a referência no campo privado.
3. O método `Execute` utiliza essa instância sempre que necessário.

```text
Instância de B
      |
      v
Construtor de A
      |
      v
Campo privado _classeB
      |
      v
Método A.Execute()
      |
      v
Chama B.Execute()
```

### Qual é o nome dessa técnica?

Essa forma de fornecer uma dependência é chamada de **injeção de dependência pelo construtor** (*constructor injection*).

A classe deixa de construir sua própria dependência e passa a recebê-la de fora.

É o padrão demonstrado conceitualmente na aula.

---

## 7. Quem cria a Classe B?

O professor antecipa uma dúvida natural:

> Se A não cria mais B, quem será responsável por criar a instância de B?

A resposta apresentada é: **o serviço de injeção de dependência**.

No contexto do .NET, podemos utilizar um **container de injeção de dependência**, que conhece os serviços registrados e é responsável por resolver as dependências necessárias.

### A analogia do "oráculo"

O instrutor compara esse serviço a uma espécie de **oráculo**, responsável por construir as instâncias quando elas são necessárias.

A aplicação define que A precisa de B, e o container fornece essa dependência ao criar A.

```text
Container de DI
     |
     +-- sabe como obter Classe B
     |
     +-- cria/obtém a instância de B
     |
     +-- fornece B ao construtor de A
     |
     v
Classe A pronta para executar
```

### Não confundir DI com o container

**Injeção de dependência** é a técnica de receber um objeto necessário de fora da classe.

O **container** é uma ferramenta que pode automatizar a criação e a entrega dessas dependências.

É possível aplicar injeção de dependência manualmente, sem um container. O professor apresenta o container porque é ele que será utilizado na configuração prática da API .NET.

---

## 8. Como o container resolve uma cadeia de dependências

O instrutor vai além do exemplo de uma dependência isolada.

Ele explica que a própria Classe A também pode ser criada pelo container.

### Situação

- Classe A precisa de Classe B.
- Classe B precisa de uma connection string.
- O container precisa saber como construir os serviços envolvidos.

Quando solicitamos uma instância de A, o container examina as dependências necessárias para construí-la.

### Fluxo conceitual

```text
Solicitar Classe A
       |
       v
Container examina o construtor de A
       |
       v
A precisa de Classe B
       |
       v
Container procura o registro de B
       |
       v
Obtém ou cria Classe B
       |
       v
Injeta B no construtor de A
       |
       v
Classe A criada
       |
       v
A.Execute() utiliza B.Execute()
```

### O que significa "registrar" um serviço?

A futura configuração da DI precisará informar ao container quais serviços ele conhece e como deve construí-los.

O professor explica que, se A e B estiverem devidamente configuradas, o serviço consegue construir A e fornecer B automaticamente.

### Complemento técnico importante

Um container não consegue inventar valores arbitrários de configuração. Se o construtor de B exige uma string chamada connectionString, essa informação precisa ser fornecida por um mecanismo de configuração, uma fábrica ou outro registro apropriado.

```text
Classe B requer string connectionString
                  |
                  v
Registro deve fornecer a configuração
                  |
                  v
Container consegue criar Classe B
```

Não basta registrar qualquer classe e esperar que todos os parâmetros do construtor sejam descobertos sem configuração.

O vídeo ainda não mostra a implementação dessas regras de registro.

---

## 9. Transient, Scoped e Singleton: opções anunciadas

Logo no início, o instrutor afirma que o .NET oferece três opções fundamentais de ciclo de vida quando registramos serviços para injeção de dependência.

Na transcrição, uma delas é pronunciada ou transcrita como "Sculpted". O termo técnico correto é **Scoped**.

| Nome | Ideia geral |
|---|---|
| **Transient** | Uma instância pode ser criada a cada resolução do serviço |
| **Scoped** | Uma instância é compartilhada dentro de um escopo |
| **Singleton** | Uma instância é compartilhada durante a vida do container |

As descrições acima são um **complemento conceitual mínimo**, para identificar corretamente os termos. O professor **ainda não demonstra o comportamento de cada ciclo de vida** nesta aula.

### Por que essas escolhas importam?

Porque a configuração define **por quanto tempo a mesma instância do serviço pode ser reutilizada**, influenciando seu ciclo de vida e a maneira como diferentes partes da aplicação a compartilham.

```text
Registrar um serviço
         |
         v
Escolher o ciclo de vida
         |
    +----+----+-----+
    |         |     |
 Transient  Scoped Singleton
```

O professor destaca que essas diferenças são importantes e aparecem frequentemente em entrevistas técnicas.

### O que será visto na próxima aula?

O instrutor anuncia a prática de configuração dos serviços de DI no .NET e uma demonstração específica do comportamento **Singleton**.

A presente gravação não inclui chamadas reais a `AddTransient`, `AddScoped` ou `AddSingleton` no projeto. Elas ficam para a sequência.

---

## 10. Relação com a arquitetura do CashFlow

O tema surge logo após termos feito uma experiência de persistência de dados que exigiu uma referência temporária entre Application e Infrastructure.

### O problema observado anteriormente

No teste, o caso de uso conhecia diretamente o contexto:

```csharp
// Exemplo da etapa temporária da aula 91.
var dbContext = new CashFlowDbContext();
```

Esse código exigia que o use case tivesse conhecimento da classe concreta localizada na Infrastructure e de sua construção.

### O objetivo conceitual agora

Desejamos fornecer as dependências necessárias ao use case sem obrigá-lo a construir manualmente seus colaboradores.

```text
ANTES: criação direta

Use case
   |
   v
new CashFlowDbContext()
   |
   v
MySQL


IDEIA INTRODUZIDA: dependência fornecida de fora

Use case
   ^
   |
Dependência injetada
   ^
   |
Container de DI
   |
   v
Configuração e construção do serviço
```

**Atenção:** essa figura representa a ligação conceitual entre as aulas 91 e 92. O professor não implementa nesta gravação a interface de repositório, a configuração do DbContext ou o registro dos serviços.

### O que a DI resolve?

Principalmente a necessidade de classes consumidoras controlarem diretamente a **construção e configuração** de seus colaboradores.

Ela também facilita reorganizar a composição dos componentes em um ponto da aplicação.

Entretanto, DI por si só não define a direção adequada das dependências entre camadas. Para que Application não conheça diretamente Infrastructure, ainda será necessário um desenho arquitetural compatível, frequentemente utilizando abstrações.

Esses detalhes ficam para as próximas etapas da trilha.

---

## 11. Distinções e cuidados técnicos

### 11.1 Dependência não é sinônimo de problema

Classes precisam colaborar. O que tentamos evitar é o acoplamento desnecessário a detalhes de construção, configuração e implementação.

### 11.2 DI não elimina o uso de new

O professor afirma que o `new` passa a acontecer "por baixo dos panos".

A intenção é correta em termos didáticos: a classe consumidora deixa de construir a dependência.

Tecnicamente, o container pode criar instâncias, reutilizar instâncias já existentes ou utilizar fábricas. Além disso, nem todos os objetos de uma aplicação precisam ser criados pelo container.

### 11.3 Injeção pelo construtor não exige uma interface

O exemplo da aula usa diretamente ClasseB como tipo do parâmetro.

```csharp
public ClasseA(ClasseB classeB)
```

Isso já caracteriza injeção de dependência.

Para reduzir também o acoplamento ao tipo concreto, podemos trabalhar com uma interface, por exemplo `IClasseB`. Essa é uma **possível evolução arquitetural**, não uma implementação demonstrada na aula.

### 11.4 DI e Dependency Inversion Principle não são sinônimos

**Dependency Injection** descreve uma técnica para fornecer dependências.

O **Dependency Inversion Principle (DIP)** é um princípio de design que orienta o relacionamento entre módulos de alto e baixo nível e suas abstrações.

É possível realizar DI sem aplicar completamente DIP.

Essa distinção é relevante para a refatoração futura do CashFlow, mas não é desenvolvida tecnicamente nesta gravação.

### 11.5 Campos e propriedades são elementos diferentes do C#

O instrutor usa termos como "propriedade privada" para um membro cujo nome começa com sublinhado.

Uma declaração como `private readonly ClasseB _classeB;` é um **campo** (*field*), enquanto `public ClasseB ClasseB { get; set; }` é uma propriedade.

O prefixo `_` é uma convenção comum para campos privados e não tem significado especial para o compilador.

### 11.6 A declaração do construtor não determina, por si só, quem cria a instância

Receber B no construtor de A obriga o código que constrói A a fornecer uma instância de B.

Essa construção pode ser manual ou automatizada por um container. A técnica de DI não depende exclusivamente do ASP.NET Core.

### 11.7 Nem todo ciclo de vida serve para qualquer dependência

A escolha entre Transient, Scoped e Singleton afeta o compartilhamento e o tempo de vida das instâncias.

É preciso considerar o estado interno do objeto, a concorrência e as dependências que ele utiliza. Os detalhes serão aprofundados nas aulas seguintes; não devemos escolher Singleton para todos os serviços só porque ele reutiliza uma instância.

---

## 12. Resumo e perguntas de revisão

### Principais aprendizados

- DI significa **Dependency Injection**, ou injeção de dependência.
- O professor inicia uma sequência de aulas teóricas e práticas sobre esse assunto.
- Uma dependência é um componente de que outra classe precisa para executar sua responsabilidade.
- O exemplo inicial mostra Classe A instanciando e utilizando Classe B.
- Quando Classe B passa a exigir uma connection string, Classe A precisa conhecer mais detalhes para construí-la.
- Esse conhecimento aumenta o acoplamento entre as classes.
- O objetivo é permitir que Classe A utilize B sem precisar cuidar de sua criação e configuração.
- Na solução demonstrada, Classe A recebe uma instância de B pelo construtor.
- A técnica é chamada de **injeção de dependência pelo construtor**.
- O container de DI é apresentado como o mecanismo que constrói e fornece os serviços configurados.
- Ao criar Classe A, o container pode resolver a dependência de Classe B, desde que o registro e suas configurações sejam adequados.
- São anunciados três ciclos de vida: **Transient, Scoped e Singleton**.
- A prática da DI e o estudo de Singleton ficam para a próxima aula.
- A explicação prepara a correção do acesso direto à Infrastructure utilizado provisoriamente na aula 91.

### Perguntas para revisão

1. O que faz uma classe depender de outra?
2. Qual é o problema de usar `new ClasseB()` diretamente em Classe A quando B exige várias configurações?
3. O que significa acoplamento forte?
4. Como a injeção pelo construtor altera a responsabilidade da Classe A?
5. Se A não cria B, quem pode fornecer essa instância?
6. O que o container precisa conhecer para resolver uma cadeia de dependências?
7. Por que uma string de configuração não pode ser presumida automaticamente pelo container?
8. DI exige obrigatoriamente o uso de interfaces?
9. DI e DIP são conceitos idênticos?
10. Quais são os três ciclos de vida anunciados na gravação?

### Respostas conceituais rápidas

- **Dependência:** um componente necessário ao trabalho de outro.
- **Acoplamento excessivo:** quando um componente precisa conhecer detalhes desnecessários de outro.
- **Injeção pelo construtor:** receber a instância exigida como parâmetro na criação do objeto.
- **Container:** mecanismo que resolve e fornece os serviços registrados.
- **Ciclos de vida:** Transient, Scoped e Singleton.
- **Aplicação ao CashFlow:** preparar a substituição da construção direta de objetos da Infrastructure dentro dos use cases.

---

## 13. Mapa mental

```text
                        AULA 92
                   INJEÇÃO DE DEPENDÊNCIA
                              |
                     O que é dependência?
                              |
                     Classe A usa Classe B
                              |
                  +-----------+-----------+
                  |                       |
               SEM DI                   COM DI
                  |                       |
            A cria B com new        A recebe B no
                  |                   construtor
                  v                       |
        A conhece detalhes               v
          de construção           B é fornecida de fora
                  |                       |
                  v                       v
          Maior acoplamento         Menor acoplamento
             à criação                à construção
                                          |
                                          v
                                      Container
                                          |
                                  Cria/obtém B
                                          |
                                  Injeta B em A
                                          |
                                          v
                                 A apenas utiliza B

                             REGISTRO DE SERVIÇOS
                                       |
                      +----------------+----------------+
                      |                |                |
                  Transient           Scoped         Singleton
                      |                |                |
                      +----------------+----------------+
                                       |
                                  PRÓXIMA AULA
                               Configuração prática
                               e estudo de Singleton
```

**Conclusão:** esta aula apresenta a razão de utilizar injeção de dependência: **separar a utilização de um componente dos detalhes de sua construção**. A Classe A passa a receber a Classe B pelo construtor, enquanto um mecanismo externo pode fornecê-la. O próximo passo é configurar esse comportamento na API e compreender seus ciclos de vida.
