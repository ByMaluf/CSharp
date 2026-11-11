# Injeção de dependência diretamente em endpoints com [FromServices]

> **Aula 96 — Simplificando os controllers com injeção de dependência por parâmetro de ação**
>
> Continuação das aulas 92 a 95, que introduziram a injeção de dependência e demonstraram os ciclos de vida Singleton, Transient e Scoped. Nesta aula, o instrutor refatora a API experimental **Dependency Injection Test** para receber os serviços apenas nos endpoints que realmente precisam deles.
>
> **Escopo:** o vídeo refatora uma API demonstrativa, não o projeto CashFlow. O instrutor anuncia que futuramente apresentará outra forma de obter serviços e explicará o uso de interfaces no registro da DI, mas não implementa esses tópicos nesta gravação.

## Índice

1. Objetivo e contexto da aula
2. Qual é o problema da injeção pelo construtor neste controller?
3. O que é [FromServices]?
4. Refatorando o endpoint Singleton
5. Refatorando o endpoint Transient
6. Refatorando o endpoint Scoped com UseCaseA e UseCaseB
7. Removendo o construtor e os campos privados
8. Configuração dos serviços continua no Program.cs
9. Testes práticos com Swagger e breakpoint
10. Comparação entre injeção pelo construtor e por ação
11. O que muda e o que não muda no ciclo de vida?
12. Código completo reconstruído da demonstração
13. Interfaces: melhoria anunciada para aulas futuras
14. Relação com o CashFlow
15. Observações e distinções técnicas
16. Resumo, revisão e mapa mental

---

## 1. Objetivo e contexto da aula

O professor apresenta uma melhoria na organização dos controllers da API experimental que vem utilizando para estudar **Dependency Injection (DI)**.

Nas aulas anteriores, todos os serviços necessários aos três endpoints foram injetados no construtor de um único controller:

- `SingletonUseCase`;
- `TransientUseCase`;
- `UseCaseA`;
- `UseCaseB`.

Depois, cada endpoint utilizava somente os colaboradores necessários àquela operação.

O problema demonstrado é que, **mesmo quando uma requisição chama apenas o endpoint Singleton, o construtor do controller precisa receber todas essas dependências**.

A proposta do vídeo é utilizar o atributo `[FromServices]` em parâmetros de ação, deixando cada endpoint responsável por solicitar apenas os serviços de que necessita.

### Objetivo prático

```text
ANTES
Controller recebe todos os use cases no construtor
                   |
                   v
Cada endpoint utiliza apenas alguns


DEPOIS
Endpoint Singleton -> recebe SingletonUseCase
Endpoint Transient -> recebe TransientUseCase
Endpoint Scoped    -> recebe UseCaseA e UseCaseB
```

Essa refatoração também reduz o número de campos privados e a quantidade de código repetitivo no controller.

---

## 2. Qual é o problema da injeção pelo construtor neste controller?

O instrutor inicia a API com um breakpoint no construtor do `MyDependencyController` e chama **somente o endpoint Singleton** pelo Swagger.

Mesmo assim, o breakpoint mostra que o construtor recebeu instâncias de todos os use cases declarados como parâmetros.

### Exemplo simplificado da situação anterior

```csharp
public class MyDependencyController : ControllerBase
{
    private readonly SingletonUseCase _singletonUseCase;
    private readonly TransientUseCase _transientUseCase;
    private readonly UseCaseA _useCaseA;
    private readonly UseCaseB _useCaseB;

    public MyDependencyController(
        SingletonUseCase singletonUseCase,
        TransientUseCase transientUseCase,
        UseCaseA useCaseA,
        UseCaseB useCaseB)
    {
        _singletonUseCase = singletonUseCase;
        _transientUseCase = transientUseCase;
        _useCaseA = useCaseA;
        _useCaseB = useCaseB;
    }

    // Métodos dos endpoints...
}
```

Esse código representa a configuração construída ao longo das aulas 93 a 95.

### O que o professor observou?

Ao fazer uma requisição ao endpoint Singleton:

1. O framework precisou ativar o controller.
2. O construtor exigiu quatro dependências.
3. O serviço de DI resolveu os quatro parâmetros.
4. O endpoint utilizou apenas `SingletonUseCase`.

O professor questiona: por que solicitar os outros use cases se aquela ação não os utiliza?

### A causa

A injeção **pelo construtor do controller** acontece na criação do objeto controller, antes da execução da ação específica.

O construtor não muda sua lista de parâmetros conforme o endpoint chamado.

```text
GET Singleton
     |
     v
Criar MyDependencyController
     |
     +-- Resolver SingletonUseCase
     +-- Resolver TransientUseCase
     +-- Resolver UseCaseA
     +-- Resolver UseCaseB
     |
     v
Executar apenas o endpoint Singleton
```

A aplicação pode funcionar assim, mas, nesse caso, o controller conhece e recebe dependências que são utilizadas exclusivamente por outras ações.

---

## 3. O que é [FromServices]?

`[FromServices]` é um atributo disponibilizado pelo ASP.NET Core MVC para indicar que o valor de um parâmetro deve ser obtido do **container de injeção de dependência**.

Em vez de receber o serviço no construtor do controller, uma ação pode recebê-lo diretamente em sua lista de parâmetros.

### Exemplo

```csharp
[HttpGet("singleton")]
public IActionResult GetSingleton(
    [FromServices] SingletonUseCase useCase)
{
    var result = useCase.Execute();

    return Ok(result);
}
```

### Entendendo a sintaxe

| Elemento | Função |
|---|---|
| `[FromServices]` | Indica que o parâmetro será resolvido pelo serviço de DI |
| `SingletonUseCase` | Tipo da dependência solicitada |
| `useCase` | Nome da variável disponível dentro da ação |
| `useCase.Execute()` | Chamada ao serviço já fornecido |
| `Ok(result)` | Resposta HTTP 200 com o resultado |

### Por que não é um parâmetro enviado pelo cliente?

Embora o tipo apareça na assinatura do método do endpoint, o valor **não é extraído do corpo JSON, da URL ou da query string**.

O ASP.NET Core resolve o serviço configurado no container e fornece a instância à ação.

```text
Requisição HTTP
      |
      v
Endpoint com [FromServices]
      |
      v
Container de DI resolve o tipo
      |
      v
Parâmetro useCase recebe instância
      |
      v
Execute()
```

### Namespace

O atributo pertence à infraestrutura do MVC, normalmente disponível por meio de:

```csharp
using Microsoft.AspNetCore.Mvc;
```

Essa é uma informação complementar para ajudar a reconhecer o tipo. Na gravação, o professor utiliza as sugestões do Visual Studio para adicionar o atributo à ação.

---

## 4. Refatorando o endpoint Singleton

O primeiro endpoint alterado é o Singleton.

### Antes

O controller guardava a dependência em um campo privado:

```csharp
private readonly SingletonUseCase _singletonUseCase;
```

O campo era inicializado no construtor e utilizado na ação.

```csharp
var result = _singletonUseCase.Execute();

return Ok(result);
```

### Depois

O professor move a dependência para o próprio método do endpoint:

```csharp
[HttpGet("singleton")]
public IActionResult GetSingleton(
    [FromServices] SingletonUseCase useCase)
{
    var result = useCase.Execute();

    return Ok(result);
}
```

A mudança elimina, para esse use case:

1. o campo privado `_singletonUseCase`;
2. o parâmetro correspondente no construtor do controller;
3. a atribuição do parâmetro ao campo.

### Vantagem observada

O método recebe somente aquilo de que precisa.

```text
GET Singleton
     |
     v
[FromServices] SingletonUseCase
     |
     v
useCase.Execute()
     |
     v
HTTP 200
```

A configuração Singleton **não muda**. O serviço continua registrado no container como na aula 93.

---

## 5. Refatorando o endpoint Transient

O instrutor aplica a mesma técnica ao endpoint Transient.

### Exemplo reconstruído

```csharp
[HttpGet("transient")]
public IActionResult GetTransient(
    [FromServices] TransientUseCase useCase)
{
    var result = useCase.Execute();

    return Ok(result);
}
```

### O que foi removido?

O campo privado, o parâmetro do construtor e a atribuição anteriormente necessários para `TransientUseCase`.

### O que permanece?

A classe ainda está registrada no container com:

```csharp
builder.Services.AddTransient<TransientUseCase>();
```

A ação agora solicita a instância ao container quando é executada.

### O ciclo de vida continua sendo Transient?

Sim.

O uso de `[FromServices]` determina **onde a dependência é recebida**, não o ciclo de vida de sua instância.

```text
AddTransient -> política de criação de instâncias
[FromServices] -> local onde a dependência é solicitada
```

Essa distinção é fundamental para não confundir o registro do serviço com a forma de injeção.

---

## 6. Refatorando o endpoint Scoped com UseCaseA e UseCaseB

Na aula 95, o professor demonstrou Scoped com dois consumidores:

- `UseCaseA`;
- `UseCaseB`.

Ambos recebem `ScopedUseCase` e retornam seu identificador. Dentro da mesma requisição, o GUID é igual entre os dois porque eles compartilham o mesmo escopo.

Na refatoração atual, o endpoint Scoped passa a receber diretamente os dois use cases por meio de `[FromServices]`.

### Exemplo reconstruído

```csharp
[HttpGet("scoped")]
public IActionResult GetScoped(
    [FromServices] UseCaseA useCaseA,
    [FromServices] UseCaseB useCaseB)
{
    var resultA = useCaseA.Execute();
    var resultB = useCaseB.Execute();

    return Ok($"{resultA} - {resultB}");
}
```

### O que acontece?

```text
GET Scoped
     |
     v
Método GetScoped(
    [FromServices] UseCaseA,
    [FromServices] UseCaseB)
     |
     +-- useCaseA.Execute()
     |
     +-- useCaseB.Execute()
     |
     v
Resultado concatenado
```

### O compartilhamento Scoped continua funcionando?

Sim, desde que `ScopedUseCase` permaneça registrado como Scoped e os consumidores sejam resolvidos dentro do mesmo escopo da requisição.

O método de injeção mudou, mas o conceito demonstrado na aula 95 permanece o mesmo.

```text
Requisição HTTP
       |
       v
Escopo de DI
       |
       +-- UseCaseA ----+
       |                |
       +-- UseCaseB ----+--> ScopedUseCase
                                  |
                               Mesmo Guid
```

---

## 7. Removendo o construtor e os campos privados

Depois que todas as ações passam a receber as dependências pelos próprios parâmetros, o professor elimina o código que deixou de ser necessário.

### Elementos removidos

- os campos privados utilizados apenas para guardar os use cases;
- o construtor que recebia os quatro serviços;
- as atribuições que copiavam os parâmetros para os campos.

### Como fica a responsabilidade do controller?

Cada endpoint declara explicitamente suas dependências no local em que serão utilizadas.

```text
MyDependencyController
   |
   +-- GetSingleton(SingletonUseCase)
   |
   +-- GetTransient(TransientUseCase)
   |
   +-- GetScoped(UseCaseA, UseCaseB)
```

O professor destaca que o código ficou mais curto e, no contexto demonstrado, mais fácil de compreender.

### O controller não precisa mais de construtor?

**Nesse exemplo, não.**

Se um controller não precisa compartilhar estado ou dependências entre suas ações, pode não ter um construtor declarado explicitamente.

Isso não significa que os construtores sejam proibidos em controllers ou que a injeção pelo construtor tenha se tornado incorreta.

---

## 8. Configuração dos serviços continua no Program.cs

Um ponto importante é que a refatoração **não substitui o registro das dependências**.

O professor havia configurado os serviços ao longo das aulas 93 a 95. O container continua precisando conhecer as classes solicitadas.

### Registros envolvidos

```csharp
builder.Services.AddSingleton<SingletonUseCase>();

builder.Services.AddTransient<TransientUseCase>();

builder.Services.AddScoped<ScopedUseCase>();

builder.Services.AddScoped<UseCaseA>();

builder.Services.AddScoped<UseCaseB>();
```

Essas declarações reúnem os registros descritos nas demonstrações anteriores.

### Papel de cada registro

| Registro | Política de ciclo de vida |
|---|---|
| `AddSingleton<SingletonUseCase>()` | Uma instância compartilhada no provider |
| `AddTransient<TransientUseCase>()` | Nova instância por resolução |
| `AddScoped<ScopedUseCase>()` | Instância compartilhada dentro do mesmo escopo |
| `AddScoped<UseCaseA>()` | Instância de A por escopo |
| `AddScoped<UseCaseB>()` | Instância de B por escopo |

### E o FromServices?

O atributo indica **como o parâmetro da ação será preenchido**. Ele não altera os registros existentes nem substitui a configuração dos serviços.

```text
Program.cs
    |
    v
Registrar serviços e ciclos de vida
    |
    v
Endpoint com [FromServices]
    |
    v
ASP.NET Core resolve a dependência
```

### Atenção aos erros de resolução

Se o tipo solicitado não estiver registrado e o container não conseguir construí-lo pelas regras disponíveis, a execução da ação falhará durante a resolução da dependência.

O fato de o parâmetro estar decorado com `[FromServices]` não faz o framework inventar automaticamente um registro inexistente.

---

## 9. Testes práticos com Swagger e breakpoint

Após refatorar o controller, o instrutor salva o código e executa a API novamente.

Ele passa pelos três endpoints para verificar que as dependências continuam sendo resolvidas corretamente.

### Teste 1 — Singleton

1. Abrir o endpoint Singleton no Swagger.
2. Clicar em **Try it out**.
3. Executar a requisição.
4. Conferir a resposta.

**Resultado:** HTTP 200 e mensagem contendo o GUID do Singleton.

### Teste 2 — Transient

1. Abrir o endpoint Transient.
2. Executar a requisição.
3. Conferir a resposta.

**Resultado:** HTTP 200 e mensagem contendo o GUID do Transient.

### Teste 3 — Scoped

1. Abrir o endpoint Scoped.
2. Inserir um breakpoint no método para inspecionar a resolução.
3. Executar a requisição.
4. Conferir que UseCaseA e UseCaseB foram recebidos.
5. Continuar a execução.

**Resultado:** o endpoint funciona e devolve os resultados dos dois use cases.

### O que a verificação comprova?

Que a refatoração para `[FromServices]` **mantém o funcionamento dos endpoints apresentados**, sem exigir a injeção de todos os use cases no construtor do controller.

O professor também destaca que, após a refatoração, aquele controller não possui mais o construtor que utilizava anteriormente.

---

## 10. Comparação entre injeção pelo construtor e por ação

A aula apresenta a injeção diretamente no método como uma melhoria para os controllers demonstrados.

### Comparação

| Aspecto | Pelo construtor | Por parâmetro de ação com [FromServices] |
|---|---|---|
| Onde a dependência é declarada? | Construtor do controller | Assinatura do endpoint |
| Quando é solicitada? | Na ativação do controller | Na resolução dos parâmetros da ação |
| É necessário guardar em campo? | Geralmente, quando usada depois | Não, se utilizada somente naquela ação |
| Uma ação recebe dependências exclusivas de outras ações? | Pode acontecer | Não, quando cada ação solicita apenas as suas |
| Exige registro no container? | Sim | Sim |
| Altera o ciclo de vida? | Não | Não |
| Pode ser usada em controllers? | Sim | Sim |
| Funciona bem para dependência compartilhada entre várias ações? | Sim | Pode gerar repetição |

### Ideia prática

**Injeção pelo construtor** pode ser conveniente para dependências utilizadas por várias ações ou para serviços que representam colaboradores do controller como um todo.

**Injeção por ação** pode ser conveniente quando determinados serviços só são utilizados por endpoints específicos.

### Por que usar FromServices no exemplo?

Porque o controller de demonstração agrupa endpoints independentes que não utilizam os mesmos use cases.

O professor evita resolver serviços desnecessários para a ação escolhida.

### A recomendação deve ser absoluta?

Não.

O instrutor recomenda utilizar `[FromServices]` em controllers e manter a injeção pelo construtor nas demais classes.

Essa é a orientação didática **específica da gravação**, e não uma obrigação imposta pelo ASP.NET Core.

Ambas as formas são suportadas e podem ser adequadas, dependendo das responsabilidades do controller.

---

## 11. O que muda e o que não muda no ciclo de vida?

Uma possível confusão é pensar que mudar o local de injeção altera a política de criação de instâncias.

**Isso não acontece.**

A escolha entre Singleton, Scoped e Transient é feita no registro dos serviços.

### Exemplo Singleton

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
```

Independentemente de o serviço ser recebido no construtor ou por `[FromServices]`, ele continua configurado como Singleton.

### Exemplo Transient

```csharp
builder.Services.AddTransient<TransientUseCase>();
```

O serviço continua sendo resolvido como Transient.

### Exemplo Scoped

```csharp
builder.Services.AddScoped<ScopedUseCase>();
```

Dentro do mesmo escopo, a instância será compartilhada pelos consumidores que resolverem esse tipo.

### Resumo visual

```text
                    REGISTRO
              AddSingleton / AddTransient / AddScoped
                              |
                              v
                    Define o ciclo de vida
                              |
                              |
           +------------------+------------------+
           |                                     |
      CONSTRUTOR                              [FromServices]
           |                                     |
           v                                     v
  Recebe a dependência                  Recebe a dependência
           |                                     |
           +------------------+------------------+
                              |
                              v
                   Mesmo registro de DI
```

A diferença está no **ponto de injeção** e no momento em que a dependência precisa ser resolvida, e não na sua política de ciclo de vida.

---

## 12. Código completo reconstruído da demonstração

Este exemplo reúne os principais componentes explicados no vídeo.

**Importante:** a transcrição não inclui o código completo, o namespace exato, os atributos de rota ou a estrutura de arquivos da API experimental. O código abaixo é uma **reconstrução didática** compatível com o comportamento descrito.

### Controller após a refatoração

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/dependency")]
public class MyDependencyController : ControllerBase
{
    [HttpGet("singleton")]
    public IActionResult GetSingleton(
        [FromServices] SingletonUseCase useCase)
    {
        var result = useCase.Execute();

        return Ok(result);
    }

    [HttpGet("transient")]
    public IActionResult GetTransient(
        [FromServices] TransientUseCase useCase)
    {
        var result = useCase.Execute();

        return Ok(result);
    }

    [HttpGet("scoped")]
    public IActionResult GetScoped(
        [FromServices] UseCaseA useCaseA,
        [FromServices] UseCaseB useCaseB)
    {
        var resultA = useCaseA.Execute();
        var resultB = useCaseB.Execute();

        return Ok($"{resultA} - {resultB}");
    }
}
```

A rota `api/dependency` foi escolhida apenas para dar um exemplo completo. A gravação não fornece a rota literal utilizada no projeto experimental.

### Registro no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<SingletonUseCase>();
builder.Services.AddTransient<TransientUseCase>();
builder.Services.AddScoped<ScopedUseCase>();
builder.Services.AddScoped<UseCaseA>();
builder.Services.AddScoped<UseCaseB>();

var app = builder.Build();

app.MapControllers();

app.Run();
```

O trecho representa a configuração mínima conceitual da API experimental. A configuração completa do Swagger, outras opções e os arquivos adicionais não são mostrados na transcrição.

### Por que não há construtor no controller?

Porque cada endpoint solicita sua dependência diretamente:

```csharp
[FromServices] SingletonUseCase useCase
```

Não é necessário guardar serviços em campos privados quando eles são utilizados apenas durante a execução daquela ação.

---

## 13. Interfaces: melhoria anunciada para aulas futuras

Ao final da gravação, o instrutor faz uma observação importante.

Até agora, os serviços foram registrados usando diretamente os tipos concretos:

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
```

Ele anuncia que, nas próximas aulas, apresentará **interfaces** e mostrará como registrar serviços associando uma interface à classe que implementa o comportamento.

### O que o vídeo efetivamente ensina?

A gravação **apenas anuncia** a evolução para interfaces.

O professor ainda não:

- define o conceito de interface;
- cria uma interface para um use case;
- registra uma interface com sua implementação;
- refatora o CashFlow para depender de contratos abstratos.

### Antecipação conceitual, não realizada nesta aula

Um registro com abstração pode assumir uma forma semelhante a:

```csharp
builder.Services.AddScoped<IMeuServico, MeuServico>();
```

O exemplo é meramente ilustrativo. Os nomes `IMeuServico` e `MeuServico` não correspondem a tipos criados pelo professor nesta aula.

### Distinção técnica

Registrar uma classe concreta **é válido** no sistema de DI do .NET.

O uso de interfaces pode reduzir o acoplamento à implementação e facilitar substituições e testes, mas **não é uma exigência universal** para que exista injeção de dependência.

Essa diferença complementa a afirmação de que interfaces seriam a forma "recomendada" de registro.

---

## 14. Relação com o CashFlow

A aplicação utilizada nesta aula é a mesma API de testes das aulas anteriores. O professor deixa claro que a implementação definitiva ocorrerá posteriormente no CashFlow.

### Conexão com o histórico do projeto

```text
Aula 91
Use case cria CashFlowDbContext diretamente
                    |
                    v
Referência temporária Application -> Infrastructure removida
                    |
                    v
Aulas 92–95
Entendemos DI, Singleton, Transient e Scoped
                    |
                    v
Aula 96
Aprendemos [FromServices] nos endpoints
                    |
                    v
Aulas futuras
Interfaces + configuração definitiva do CashFlow
```

### Como isso poderá ser útil?

O controller pode solicitar, no método de cada endpoint, apenas o use case responsável por aquela operação.

Os use cases, por sua vez, podem continuar recebendo seus colaboradores por meio dos construtores.

Esse é o desenho que o instrutor recomenda, mas a gravação **não refatora ainda os controllers do CashFlow** nem cria suas interfaces de dependência.

---

## 15. Observações e distinções técnicas

### 15.1 [FromServices] só vale para o parâmetro em que aparece

O atributo indica que aquele parâmetro específico deve ser resolvido pelo container.

Isso evita interpretar o argumento como dado vindo da requisição HTTP.

### 15.2 Resolver somente o necessário depende da arquitetura da ação

No exemplo, cada método recebe diretamente seu próprio serviço, então as ações não solicitam os outros use cases apenas por estarem no mesmo controller.

Entretanto, a resolução de um serviço pode exigir **outras dependências transitivas** necessárias para construí-lo. Logo, `[FromServices]` não significa que apenas um único objeto será criado.

### 15.3 Controllers podem receber serviços pelo construtor normalmente

A afirmação "no controller não precisa fazer com o construtor" vale como escolha de organização da demonstração.

O ASP.NET Core oferece suporte à injeção pelo construtor nos controllers, e ela permanece uma opção adequada.

### 15.4 Injeção por ação é especialmente útil para dependências exclusivas

Quando uma classe possui muitas ações independentes, declarar dependências específicas na ação pode evitar um construtor excessivamente amplo.

Se muitas ações precisam do mesmo serviço, injetá-lo pelo construtor pode ser mais simples e reduzir duplicações.

### 15.5 O ciclo de vida não depende de FromServices

Os serviços continuam obedecendo ao registro Singleton, Scoped ou Transient.

A refatoração não torna um serviço Scoped em Transient e não cria um Singleton novo a cada requisição.

### 15.6 É necessário registrar o tipo resolvido

Se um método declara `[FromServices] MeuServico servico`, o container precisa saber como atender a esse tipo.

Quando o projeto evoluir para interfaces, o registro precisará corresponder à abstração solicitada.

### 15.7 Campos privados não são sempre desnecessários

A remoção dos campos é adequada para o controller demonstrado.

Outras classes podem manter dependências em campos privados porque precisam utilizá-las em vários métodos durante a vida do objeto.

### 15.8 Há uma terceira forma anunciada, mas não descrita

O instrutor menciona uma alternativa adicional para receber ou obter dependências e diz que a apresentará futuramente.

**O vídeo não fornece detalhes suficientes para identificar com segurança qual técnica específica será ensinada.** Por isso, não atribuiremos um nome ou implementação a essa terceira forma.

### 15.9 Injeção por atributo de parâmetro não equivale a injeção automática em qualquer método

`[FromServices]` é usado no contexto apropriado de binding/resolução de parâmetros de ações ASP.NET Core MVC. Não significa que qualquer método arbitrário de uma classe terá seus parâmetros automaticamente preenchidos pela DI.

### 15.10 O código mostrado é uma refatoração de organização

O objetivo demonstrado não é mudar o comportamento de negócio dos endpoints, mas reorganizar a maneira como os objetos são fornecidos.

Por isso o professor executa os três endpoints depois da alteração para comprovar que continuam respondendo corretamente.

---

## 16. Resumo, revisão e mapa mental

### Principais aprendizados

- O instrutor apresenta uma melhoria na DI dos controllers.
- Inicialmente, `MyDependencyController` recebe todos os use cases pelo construtor.
- Ao chamar somente Singleton, o construtor ainda recebe TransientUseCase, UseCaseA e UseCaseB.
- A refatoração utiliza o atributo **`[FromServices]`** nos parâmetros dos endpoints.
- O endpoint Singleton passa a receber `SingletonUseCase` diretamente.
- O endpoint Transient recebe `TransientUseCase`.
- O endpoint Scoped recebe `UseCaseA` e `UseCaseB`.
- O controller deixa de precisar dos campos privados e do construtor utilizados apenas para guardar essas dependências.
- Os serviços **continuam registrados** no `Program.cs`.
- `[FromServices]` altera o ponto em que o serviço é solicitado, mas **não o ciclo de vida** definido por `AddSingleton`, `AddTransient` ou `AddScoped`.
- Os três endpoints são testados pelo Swagger e continuam funcionando.
- O professor recomenda esse padrão para ações de controllers, mantendo construtor nos use cases.
- Aulas futuras apresentarão interfaces e outra forma de obtenção de dependências.
- A gravação ainda não implementa essas alterações no CashFlow.

### Perguntas de revisão

1. Qual foi o problema identificado ao chamar somente o endpoint Singleton?
2. Por que o construtor do controller recebia as dependências dos outros endpoints?
3. O que faz o atributo `[FromServices]`?
4. Uma dependência recebida por `[FromServices]` vem do corpo HTTP?
5. Em qual ponto da ação o parâmetro `SingletonUseCase useCase` é declarado?
6. O `Program.cs` deixa de precisar dos registros após a refatoração?
7. Por que o controller demonstrado não precisa mais de campos privados?
8. Como o endpoint Scoped recebe os dois use cases?
9. Usar `[FromServices]` altera um serviço Singleton para Scoped?
10. A injeção pelo construtor deixou de ser válida em controllers?
11. O professor criou interfaces nesta aula?
12. Qual é a relação da melhoria apresentada com a futura refatoração do CashFlow?

### Comparação rápida

| Situação | Estratégia possível |
|---|---|
| Dependência específica de uma ação | Parâmetro com `[FromServices]` |
| Dependência utilizada por várias ações do controller | Injeção pelo construtor pode ser apropriada |
| Colaborador necessário para um use case ou classe comum | Injeção pelo construtor, como demonstrado anteriormente |
| Definir quanto tempo a instância é compartilhada | Configurar `AddSingleton`, `AddScoped` ou `AddTransient` |
| Reduzir acoplamento à implementação concreta | Considerar interfaces e abstrações, assunto futuro |

### Mapa mental

```text
                        AULA 96
                      [FromServices]
                            |
                 Controller com endpoints
                            |
           +----------------+----------------+
           |                |                |
        Singleton        Transient          Scoped
           |                |                |
     [FromServices]   [FromServices]   [FromServices]
     SingletonUseCase TransientUseCase UseCaseA + B
           |                |                |
           +----------------+----------------+
                            |
                     Container de DI
                            |
              Serviços já registrados
                            |
              +-------------+-------------+
              |             |             |
          AddSingleton  AddTransient   AddScoped
              |             |             |
              +-------------+-------------+
                            |
                         Executar
                            |
               Swagger: HTTP 200 nos testes
                            |
                   Controller mais enxuto
                            |
                       Aulas futuras
                    Interfaces e CashFlow
```

**Conclusão:** esta aula demonstra a **injeção de dependência por parâmetro de ação** com `[FromServices]`. No controller experimental, cada endpoint passa a receber apenas os use cases de que precisa, permitindo remover o construtor e os campos que existiam exclusivamente para guardar essas dependências. O mecanismo de DI e os ciclos de vida permanecem os mesmos; o que muda é **onde os serviços são solicitados**.
