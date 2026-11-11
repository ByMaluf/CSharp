# Injeção de dependência: Scoped, AddScoped e escopo de requisição no ASP.NET Core

> **Aula 95 — Demonstração prática do ciclo de vida Scoped**
>
> Continuação das aulas 93 e 94, em que estudamos, respectivamente, Singleton e Transient. Esta aula conclui a comparação inicial dos três ciclos de vida disponíveis no container de injeção de dependência do ASP.NET Core.
>
> **Escopo:** o instrutor continua usando a API experimental **Dependency Injection Test**. O conteúdo ainda não implementa a injeção de dependência no CashFlow, nem modifica sua configuração de acesso ao MySQL.

## Índice

1. Objetivo da aula
2. Revisão de Singleton e Transient
3. Criando e registrando ScopedUseCase
4. Injeção do ScopedUseCase no controller
5. Primeiro experimento: GUID muda entre requisições
6. O que é um escopo de DI?
7. Por que Scoped não é a mesma coisa que Transient?
8. Segundo experimento: UseCaseA e UseCaseB
9. Registros e cadeia de dependências
10. Resultado observado no Swagger
11. Código C# reconstruído da demonstração
12. Comparação definitiva dos três ciclos de vida
13. A diferença entre escopo e thread
14. Relação com o CashFlow
15. Cuidados técnicos e exceções
16. Revisão, perguntas e mapa mental

---

## 1. Objetivo da aula

O instrutor começa exatamente de onde parou na aula anterior, com uma API de demonstração contendo três endpoints: Singleton, Transient e Scoped.

A proposta é compreender, na prática, o terceiro ciclo de vida: **Scoped**.

Ao longo da gravação, o professor:

- configura um serviço como Scoped com `builder.Services.AddScoped<ScopedUseCase>()`;
- injeta esse serviço no controller;
- observa que novas requisições retornam GUIDs diferentes;
- explica que cada requisição HTTP possui, normalmente, seu próprio escopo de injeção de dependência;
- cria dois componentes intermediários, `UseCaseA` e `UseCaseB`;
- faz ambos receberem a mesma dependência Scoped;
- comprova que **os dois usam a mesma instância dentro da mesma requisição**;
- conclui a comparação de Singleton, Transient e Scoped.

A transcrição pronuncia o termo como "Scope Edge" ou "Scopeage". Os termos técnicos utilizados neste resumo são **Scoped**, `AddScoped` e `ScopedUseCase`.

---

## 2. Revisão de Singleton e Transient

Para entender Scoped, o professor utiliza como referência os resultados das aulas anteriores.

### Singleton — aula 93

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
```

O objeto recebe um `Guid` no construtor.

Várias requisições ao mesmo endpoint apresentam o mesmo valor de GUID, pois a mesma instância do serviço é reutilizada dentro da vida do provider de serviços.

```text
Requisição 1 ------+
Requisição 2 ------+----> SingletonUseCase [GUID A]
Requisição 3 ------+
```

### Transient — aula 94

```csharp
builder.Services.AddTransient<TransientUseCase>();
```

Uma nova instância é criada sempre que o serviço é **resolvido pelo container**.

No teste da aula 94, cada requisição levou a uma nova resolução e, consequentemente, o GUID mudou.

```text
Requisição 1 ----> TransientUseCase [GUID A]
Requisição 2 ----> TransientUseCase [GUID B]
Requisição 3 ----> TransientUseCase [GUID C]
```

### A dúvida que a aula pretende responder

Se o Scoped também apresentar um novo GUID em cada requisição, **qual é a diferença entre Scoped e Transient?**

O instrutor responde com uma demonstração de compartilhamento entre dois consumidores dentro da **mesma** requisição.

---

## 3. Criando e registrando ScopedUseCase

A classe utilizada é semelhante às classes SingletonUseCase e TransientUseCase.

Ela possui:

1. um identificador do tipo `Guid`;
2. um construtor que gera esse identificador;
3. um método `Execute()` que retorna uma string com o valor gerado.

### Reconstrução didática

```csharp
public class ScopedUseCase
{
    private readonly Guid _id;

    public ScopedUseCase()
    {
        _id = Guid.NewGuid();
    }

    public string Execute()
    {
        return $"From Scoped with ID: {_id}";
    }
}
```

O código acima representa a estrutura descrita oralmente; não é uma transcrição literal da tela. O formato exato dos campos e da mensagem pode diferir.

### Registro no Program.cs

O professor acrescenta:

```csharp
builder.Services.AddScoped<ScopedUseCase>();
```

O container passa a conhecer `ScopedUseCase` e a tratá-lo com o ciclo de vida **Scoped**.

### O que o AddScoped faz?

É um registro de serviço que informa ao provedor de DI:

> Quando `ScopedUseCase` for solicitado dentro de um escopo, forneça uma instância compartilhada durante a vida daquele escopo.

Uma nova resolução **dentro do mesmo escopo** reutilizará o mesmo objeto; em outro escopo, será criada outra instância quando necessária.

---

## 4. Injeção do ScopedUseCase no controller

Na primeira parte da aula, o professor amplia o controller utilizado na demonstração.

Ele já recebe SingletonUseCase e TransientUseCase. Agora acrescenta uma terceira dependência:

```csharp
private readonly ScopedUseCase _scopedUseCase;
```

E passa a recebê-la pelo construtor, em conjunto com as anteriores.

### Exemplo parcial reconstruído

```csharp
public MyDependencyController(
    SingletonUseCase singletonUseCase,
    TransientUseCase transientUseCase,
    ScopedUseCase scopedUseCase)
{
    _singletonUseCase = singletonUseCase;
    _transientUseCase = transientUseCase;
    _scopedUseCase = scopedUseCase;
}
```

O endpoint Scoped, inicialmente, utiliza diretamente o campo:

```csharp
var result = _scopedUseCase.Execute();

return Ok(result);
```

### Qual é a função dessa configuração?

O controller não precisa criar o `ScopedUseCase` com `new`.

Durante sua ativação, o ASP.NET Core resolve a dependência pelo container e fornece a instância ao construtor.

```text
GET Scoped
    |
    v
Ativar controller
    |
    v
Resolver ScopedUseCase
    |
    v
Injetar no construtor
    |
    v
Executar ScopedUseCase.Execute()
```

---

## 5. Primeiro experimento: GUID muda entre requisições

O professor adiciona breakpoints, executa a API com F5 e realiza chamadas pelo Swagger.

### Sequência observada

1. Abrir o endpoint Scoped.
2. Selecionar **Try it out**.
3. Clicar em **Execute**.
4. Inspecionar o construtor do controller com o debugger.
5. Verificar que as instâncias de SingletonUseCase, TransientUseCase e ScopedUseCase foram recebidas.
6. Continuar a execução.
7. Consultar o identificador devolvido pelo endpoint.
8. Repetir a requisição várias vezes.

### Resultado

Na primeira chamada aparece um GUID. Na segunda, aparece outro. Na terceira, outro.

```text
GET Scoped 1 -> Guid A

GET Scoped 2 -> Guid B

GET Scoped 3 -> Guid C
```

Esses valores representam os identificadores diferentes observados na gravação, não os GUIDs literais mostrados no Swagger.

### O que esse resultado indica?

No fluxo HTTP padrão da demonstração, cada requisição utiliza seu próprio escopo de serviços, e o `ScopedUseCase` recebe uma nova instância para aquele novo escopo.

### Mas isso não é igual a Transient?

**Ainda não é possível distinguir os dois apenas com essa experiência.**

Como o teste resolve o serviço uma vez em cada nova requisição, tanto Transient quanto Scoped podem apresentar novos identificadores a cada chamada.

Para mostrar a diferença, o professor muda a estrutura do exemplo.

---

## 6. O que é um escopo de DI?

Um **escopo** (*scope*) é um contexto de resolução e gerenciamento do tempo de vida dos serviços.

Quando um serviço está registrado como Scoped, uma instância pode ser compartilhada entre todos os consumidores que o resolvem **no mesmo escopo**.

### No ASP.NET Core

No comportamento usual de aplicações HTTP, o framework cria um escopo de DI para cada requisição recebida.

```text
Requisição HTTP A
     |
     v
Escopo de DI A
     |
     +-- Serviço Scoped -> Instância A
     +-- Outro consumidor -> reutiliza Instância A


Requisição HTTP B
     |
     v
Escopo de DI B
     |
     +-- Serviço Scoped -> Instância B
     +-- Outro consumidor -> reutiliza Instância B
```

### Quando o serviço é criado?

Quando alguém solicita esse serviço dentro de um escopo e ainda não existe uma instância resolvida para ele nesse escopo.

### Quando ele deixa de ser reutilizado?

Quando o escopo é encerrado, suas instâncias Scoped deixam de estar disponíveis para novas resoluções por aquele escopo.

Serviços descartáveis criados e gerenciados pelo container são descartados conforme as regras do escopo.

**Regra para memorizar:** Scoped mantém **uma instância por escopo de DI**, normalmente **uma por requisição HTTP** em APIs ASP.NET Core.

---

## 7. Por que Scoped não é a mesma coisa que Transient?

O professor aponta que os dois tipos exibem novos GUIDs em requisições distintas, mas isso esconde uma diferença importante.

### Transient

Sempre que o container resolve o serviço, cria outra instância.

Mesmo dentro de **uma única requisição**, dois consumidores que resolvem o mesmo tipo Transient normalmente recebem **objetos diferentes**.

### Scoped

Em resoluções dentro do **mesmo escopo**, o container reutiliza a instância daquele serviço.

Dois consumidores do mesmo tipo Scoped recebem **o mesmo objeto**, desde que sejam resolvidos pelo mesmo escopo.

### Comparação conceitual

```text
UMA REQUISIÇÃO HTTP
         |
         v
     Escopo X
         |
         +-- Consumidor A pede Transient -> Objeto T1
         +-- Consumidor B pede Transient -> Objeto T2
         |
         +-- Consumidor A pede Scoped ----+
         |                                |
         +-- Consumidor B pede Scoped ----+--> Objeto S1
```

Esse é o contraste que a segunda parte da gravação se propõe a comprovar.

### A frase-chave

**Transient: uma nova instância por resolução.**

**Scoped: a mesma instância compartilhada dentro do mesmo escopo.**

---

## 8. Segundo experimento: UseCaseA e UseCaseB

Para diferenciar Scoped de Transient, o instrutor modifica a API experimental.

Ele apresenta dois componentes:

- **UseCaseA**
- **UseCaseB**

Ambos dependem de `ScopedUseCase`.

### Estrutura dos dois componentes

O UseCaseA recebe uma instância de `ScopedUseCase` em seu construtor e a utiliza dentro de `Execute()`.

O UseCaseB faz a mesma coisa.

```text
UseCaseA
   |
   v
ScopedUseCase


UseCaseB
   |
   v
ScopedUseCase
```

### Qual é a pergunta da experiência?

Se A e B precisam do mesmo serviço Scoped **durante a mesma requisição**, o container fornecerá:

- duas instâncias diferentes? Ou
- a mesma instância compartilhada?

A gravação mostra que ambos recebem **a mesma instância**.

### Código reconstruído: UseCaseA

```csharp
public class UseCaseA
{
    private readonly ScopedUseCase _scopedUseCase;

    public UseCaseA(ScopedUseCase scopedUseCase)
    {
        _scopedUseCase = scopedUseCase;
    }

    public string Execute()
    {
        return _scopedUseCase.Execute();
    }
}
```

### Código reconstruído: UseCaseB

```csharp
public class UseCaseB
{
    private readonly ScopedUseCase _scopedUseCase;

    public UseCaseB(ScopedUseCase scopedUseCase)
    {
        _scopedUseCase = scopedUseCase;
    }

    public string Execute()
    {
        return _scopedUseCase.Execute();
    }
}
```

Ambos utilizam o método `Execute()` do serviço Scoped recebido.

Esses códigos são reconstruções didáticas do comportamento narrado, não cópias literais das classes exibidas.

---

## 9. Registros e cadeia de dependências

O professor mantém `ScopedUseCase` registrado como Scoped e acrescenta os registros de UseCaseA e UseCaseB.

### Configuração ilustrativa coerente com a gravação

```csharp
builder.Services.AddScoped<ScopedUseCase>();
builder.Services.AddScoped<UseCaseA>();
builder.Services.AddScoped<UseCaseB>();
```

Na transcrição, o instrutor demonstra a duplicação do registro e a troca dos tipos para UseCaseA e UseCaseB; a disposição exata das linhas na tela não pode ser confirmada literalmente pelo texto.

### O controller agora recebe A e B

Em vez de receber diretamente o `ScopedUseCase` para o endpoint Scoped, o controller passa a utilizar os dois componentes intermediários.

Um exemplo de construtor seria:

```csharp
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
```

Os campos privados correspondentes são pressupostos nesse trecho ilustrativo.

### Como o container resolve tudo?

```text
Ativar MyDependencyController
             |
             +-- Resolver UseCaseA
             |        |
             |        +-- Precisa de ScopedUseCase
             |                  |
             |                  v
             |           Cria instância S1
             |
             +-- Resolver UseCaseB
                      |
                      +-- Precisa de ScopedUseCase
                                |
                                v
                         Reutiliza S1
```

O ponto decisivo é que **UseCaseA e UseCaseB são resolvidos dentro do mesmo escopo da requisição**.

Não importa que sejam classes consumidoras diferentes: a dependência `ScopedUseCase` é compartilhada.

---

## 10. Resultado observado no Swagger

O endpoint Scoped passa a executar os dois componentes.

### Exemplo reconstruído da ação

```csharp
[HttpGet("scoped")]
public IActionResult GetScoped()
{
    var resultA = _useCaseA.Execute();
    var resultB = _useCaseB.Execute();

    return Ok($"{resultA} - {resultB}");
}
```

O professor utiliza **interpolação de strings** para concatenar os resultados, separando-os por um hífen.

### Primeira requisição

O Swagger mostra duas mensagens, uma produzida por A e outra por B.

Ambas exibem o **mesmo identificador**:

```text
From Scoped with ID: GUID A - From Scoped with ID: GUID A
```

### Segunda requisição

Ao executar o endpoint novamente, aparece outro identificador, mas ele continua igual entre A e B daquela nova chamada:

```text
From Scoped with ID: GUID B - From Scoped with ID: GUID B
```

### O que o professor demonstrou?

| Comparação | Resultado |
|---|---|
| UseCaseA e UseCaseB na requisição 1 | Mesmo GUID |
| UseCaseA e UseCaseB na requisição 2 | Mesmo GUID |
| Requisição 1 versus requisição 2 | GUID diferente |

### Por que os identificadores são iguais dentro de uma chamada?

Porque os dois use cases recebem a mesma instância de `ScopedUseCase`.

Como o Guid foi criado no construtor dessa instância e não foi alterado, os dois métodos retornam o mesmo valor.

```text
REQUISIÇÃO 1
Escopo A
   |
   +-- UseCaseA ----+
   |                |
   +-- UseCaseB ----+----> ScopedUseCase [GUID A]


REQUISIÇÃO 2
Escopo B
   |
   +-- UseCaseA ----+
   |                |
   +-- UseCaseB ----+----> ScopedUseCase [GUID B]
```

**Essa é a demonstração principal de Scoped.**

---

## 11. Código C# reconstruído da demonstração

Para visualizar o experimento completo, o exemplo abaixo reúne os principais trechos em uma organização simplificada.

### Serviço Scoped

```csharp
public class ScopedUseCase
{
    private readonly Guid _id = Guid.NewGuid();

    public string Execute()
    {
        return $"From Scoped with ID: {_id}";
    }
}
```

Este exemplo utiliza um inicializador de campo para gerar o GUID durante a criação da instância, mas na gravação o professor descreve explicitamente a geração no construtor. A implementação abaixo expressa essa forma de maneira mais fiel:

```csharp
public class ScopedUseCase
{
    private readonly Guid _id;

    public ScopedUseCase()
    {
        _id = Guid.NewGuid();
    }

    public string Execute()
    {
        return $"From Scoped with ID: {_id}";
    }
}
```

### Consumidores

```csharp
public class UseCaseA
{
    private readonly ScopedUseCase _scoped;

    public UseCaseA(ScopedUseCase scoped)
    {
        _scoped = scoped;
    }

    public string Execute() => _scoped.Execute();
}

public class UseCaseB
{
    private readonly ScopedUseCase _scoped;

    public UseCaseB(ScopedUseCase scoped)
    {
        _scoped = scoped;
    }

    public string Execute() => _scoped.Execute();
}
```

### Program.cs

```csharp
builder.Services.AddScoped<ScopedUseCase>();
builder.Services.AddScoped<UseCaseA>();
builder.Services.AddScoped<UseCaseB>();
```

Esses registros devem ser realizados na configuração dos serviços, antes de construir a aplicação.

### Endpoint ilustrativo

```csharp
[HttpGet("scoped")]
public IActionResult GetScoped()
{
    var valueA = _useCaseA.Execute();
    var valueB = _useCaseB.Execute();

    return Ok($"{valueA} - {valueB}");
}
```

O endpoint pressupõe que os campos `_useCaseA` e `_useCaseB` tenham sido preenchidos por injeção no construtor do controller.

### Uma ressalva necessária

O código reúne trechos didáticos para explicar a demonstração. **O vídeo não apresenta um projeto completo para copiar e executar**, e sua finalidade é observar o comportamento da DI, não estruturar uma aplicação de produção.

---

## 12. Comparação definitiva dos três ciclos de vida

Agora é possível comparar as três opções estudadas nas aulas 93, 94 e 95.

| Critério | Singleton | Transient | Scoped |
|---|---|---|---|
| Registro | `AddSingleton` | `AddTransient` | `AddScoped` |
| Nova instância | Em geral, uma por provider | A cada resolução | Uma por escopo |
| Mesma instância entre requisições distintas? | Sim, dentro do mesmo provider | Não, quando resolvido novamente | Não, no modelo de escopo por requisição |
| Mesma instância em dois consumidores na mesma requisição? | Sim | Em geral, não | Sim |
| GUID em requisições diferentes | Igual | Diferente no exemplo | Diferente no exemplo |
| GUID entre A e B na mesma requisição | Igual | Em geral, diferente | Igual |

### Diagrama comparativo

```text
                      REQUISIÇÃO 1
                   Consumidor A / B

Singleton          A ----+
                        +----> S1
                   B ----+

Transient          A --------> T1
                   B --------> T2

Scoped             A ----+
                        +----> C1
                   B ----+


                      REQUISIÇÃO 2
                   Consumidor A / B

Singleton          A ----+
                        +----> S1  (mesmo objeto)
                   B ----+

Transient          A --------> T3
                   B --------> T4

Scoped             A ----+
                        +----> C2  (novo escopo)
                   B ----+
```

### Regras para memorizar corretamente

- **Singleton:** uma instância compartilhada no provedor de serviços correspondente.
- **Transient:** uma instância nova por **resolução**.
- **Scoped:** uma instância compartilhada por **escopo**.

Em APIs ASP.NET Core comuns, um escopo normalmente corresponde a **uma requisição HTTP**, mas a noção de escopo não é exclusiva de requisições.

---

## 13. A diferença entre escopo e thread

Esta seção contém a principal **correção técnica** à analogia utilizada na gravação.

O professor descreve cada requisição como uma "caixinha" ou uma "thread" que seria criada para processar a chamada e destruída depois da resposta.

A analogia da "caixinha" pode ajudar a visualizar **o isolamento dos escopos de DI**, mas não deve ser interpretada literalmente como o gerenciamento de threads do ASP.NET Core.

### O que acontece realmente?

- O ASP.NET Core normalmente cria um **escopo de DI por requisição HTTP**.
- As threads utilizadas para executar o código são, em geral, fornecidas por um **pool de threads**.
- Uma requisição **não precisa ter uma thread exclusiva e permanente** do começo ao fim.
- Durante operações assíncronas (`await`), a continuação da requisição pode executar em outra thread.
- Threads do pool podem ser reutilizadas para processar outras requisições.
- Encerrar uma requisição não significa que sua thread tenha sido "morta".

### Relação correta

```text
Requisição HTTP
     |
     v
Escopo de DI criado
     |
     +-- Serviços Scoped compartilhados
     |
     +-- Trabalho executado usando threads disponíveis
     |   (a thread pode variar ao longo do fluxo)
     |
     v
Resposta concluída
     |
     v
Escopo de DI encerrado/descartado
```

### Por que essa distinção importa?

Porque a política de compartilhamento de um serviço **Scoped** não depende da identidade da thread utilizada.

Mesmo que a requisição continue em outra thread após uma operação assíncrona, a instância Scoped associada àquele escopo continua sendo a mesma.

**Definição correta:** o ciclo de vida Scoped está vinculado ao **escopo do container de DI**, não ao tempo de vida de uma thread.

---

## 14. Relação com o CashFlow

O instrutor conclui a sequência de aulas comparando os três ciclos de vida. Ele afirma ter preferência pessoal pelo Scoped, pois considera que Singleton compartilha instâncias por tempo excessivo e que Transient cria muitas instâncias.

No contexto do CashFlow, essa explicação prepara a futura configuração dos serviços de aplicação e acesso a dados.

### Ligação com as aulas anteriores

```text
Aula 90
Criar CashFlowDbContext
       |
       v
Aula 91
Testar Add + SaveChanges com new direto
       |
       v
Remover referência temporária entre camadas
       |
       v
Aulas 92 a 95
Entender DI e os três ciclos de vida
       |
       v
Próximas etapas
Configurar dependências do CashFlow
```

### E o DbContext?

**Complemento técnico:** o EF Core normalmente é registrado como Scoped quando usamos `AddDbContext` em aplicações ASP.NET Core.

Isso combina com o modelo habitual de uma unidade de trabalho por requisição HTTP.

Entretanto, **a aula não implementa o registro do CashFlowDbContext**, nem demonstra `AddDbContext` no projeto CashFlow.

### Por que a escolha do ciclo de vida merece atenção?

Um `DbContext` mantém estado de rastreamento de entidades e não é seguro para operações concorrentes simultâneas na mesma instância.

O ciclo de vida deve ser escolhido considerando as responsabilidades do serviço e o modelo de execução da aplicação, não apenas uma preferência por reduzir criações ou compartilhar objetos.

---

## 15. Cuidados técnicos e exceções

### 15.1 Scoped não é sinônimo absoluto de "por requisição"

O escopo pode ser criado e gerenciado em outros cenários, inclusive fora de uma requisição HTTP.

Em aplicações web ASP.NET Core, normalmente existe um escopo associado à requisição. Em outros tipos de aplicação, pode ser necessário criar escopos explicitamente.

### 15.2 Transient não cria um objeto a cada chamada de método

Um Transient cria uma nova instância quando o **container o resolve**. Depois que um objeto foi recebido por um consumidor, ele pode ser utilizado repetidamente sem ser recriado.

### 15.3 Singleton não é necessariamente uma escolha ruim

O professor considera o Singleton perigoso devido ao compartilhamento e ao uso de memória.

A cautela é pertinente para serviços com estado mutável ou dependências incompatíveis, mas Singleton pode ser adequado a serviços compartilháveis e seguros para concorrência.

### 15.4 Scoped não é sempre a melhor opção

Scoped é conveniente para diversas operações ligadas a uma requisição, mas não é a escolha universal para toda classe.

Serviços leves e independentes podem utilizar Transient; serviços compartilhados apropriados podem utilizar Singleton.

### 15.5 Dependências Scoped dentro de Singletons exigem atenção

Um Singleton normalmente vive mais do que um Scoped.

Se o Singleton capturar diretamente um serviço Scoped, pode ocorrer uma incompatibilidade de ciclos de vida (*captive dependency*). Quando a validação de escopos está habilitada, o container pode rejeitar essa combinação.

### 15.6 Dois consumidores recebem o mesmo Scoped somente se compartilham o escopo

Não basta que dois serviços sejam executados no mesmo processo ou na mesma thread.

O compartilhamento acontece porque as resoluções utilizam **o mesmo escopo de DI**.

### 15.7 GUID é um recurso de observação

O professor utiliza `Guid.NewGuid()` como um identificador gerado durante a criação de cada objeto.

Ele serve para ilustrar quais resoluções utilizam a mesma instância.

O GUID, sozinho, não altera o ciclo de vida do serviço; o comportamento é determinado pelo registro do container.

### 15.8 A responsabilidade do container inclui descarte

O container controla a vida dos serviços que cria e pode descartar instâncias que implementam `IDisposable` ou `IAsyncDisposable` quando o escopo ou provider apropriado é encerrado.

Isso não significa que toda instância seja liberada imediatamente da memória no instante do descarte. O gerenciamento de memória do .NET envolve também o coletor de lixo.

### 15.9 Scope não deve ser confundido com transação

Uma transação de banco de dados e um escopo de DI são mecanismos diferentes.

Uma requisição pode utilizar um serviço Scoped sem que isso signifique automaticamente a existência de uma transação ativa no banco.

---

## 16. Revisão, perguntas e mapa mental

### Resumo dos principais aprendizados

- O professor conclui a sequência sobre os ciclos de vida da DI.
- O serviço estudado é `ScopedUseCase`.
- O registro é feito com `builder.Services.AddScoped<ScopedUseCase>();`.
- A primeira implementação injeta o `ScopedUseCase` diretamente no controller.
- Nesse primeiro teste, cada nova requisição apresenta um GUID diferente.
- Esse resultado também poderia ocorrer com Transient, por isso um segundo experimento é necessário.
- O professor introduz `UseCaseA` e `UseCaseB`.
- Ambos recebem o mesmo tipo `ScopedUseCase` nos construtores.
- O controller recebe A e B, executa ambos e concatena as respostas.
- Na mesma requisição, **os GUIDs retornados por A e B são idênticos**.
- Em uma nova requisição, o GUID muda, mas continua igual entre A e B daquela requisição.
- Scoped significa uma instância compartilhada dentro de um mesmo escopo.
- O escopo HTTP não equivale a uma thread exclusiva.
- Transient cria uma instância a cada resolução.
- Singleton reutiliza uma instância no mesmo provider.
- O professor prefere Scoped, mas a escolha adequada depende do uso de cada serviço.
- O CashFlow ainda não é modificado nesta gravação.

### Perguntas de revisão

1. Qual método registra um serviço com ciclo de vida Scoped?
2. Por que o GUID do ScopedUseCase mudou entre as primeiras requisições?
3. Por que esse primeiro teste não foi suficiente para diferenciar Scoped de Transient?
4. Qual foi a função de UseCaseA e UseCaseB no segundo experimento?
5. Por que os dois componentes receberam o mesmo GUID na mesma requisição?
6. O que acontece com o GUID quando começa uma nova requisição?
7. Qual é a diferença entre uma resolução de serviço e uma chamada de método?
8. Scoped significa necessariamente uma instância por thread?
9. Que relação existe entre o escopo de DI e o ciclo de vida de um DbContext?
10. Por que não devemos escolher o ciclo de vida apenas pela preferência pessoal do desenvolvedor?

### Respostas rápidas

| Pergunta | Resposta |
|---|---|
| Método de registro | `AddScoped<T>()` |
| Compartilhamento no mesmo escopo | A mesma instância |
| Resolução em novo escopo | Nova instância quando solicitada |
| Escopo padrão em APIs ASP.NET Core | Normalmente, uma requisição HTTP |
| Scoped é uma thread? | Não |
| Diferença para Transient | Transient cria outra instância por resolução |
| Diferença para Singleton | Singleton compartilha entre escopos do mesmo provider |

### Mapa mental

```text
                         AULA 95
                          SCOPED
                             |
                 Dependency Injection Test
                             |
                      AddScoped<T>()
                             |
                      Container de DI
                             |
                  Requisição HTTP número 1
                             |
                         Escopo A
                             |
                    ScopedUseCase S1
                      GUID: AAA
                       /      \
                      /        \
                 UseCaseA    UseCaseB
                      |        |
                      +---+----+
                          |
                  Mesmo GUID: AAA
                             |
                      Fim da requisição
                             |
                  Requisição HTTP número 2
                             |
                         Escopo B
                             |
                    ScopedUseCase S2
                      GUID: BBB
                       /      \
                      /        \
                 UseCaseA    UseCaseB
                      |        |
                      +---+----+
                          |
                  Mesmo GUID: BBB


                   CICLOS DE VIDA
                 +--------+--------+
                 |        |        |
             Singleton  Transient  Scoped
                 |        |        |
             Provider  Resolução  Escopo
```

**Conclusão:** esta aula demonstra que um serviço registrado com `AddScoped` é **compartilhado entre os componentes que o resolvem dentro do mesmo escopo de DI**. Em uma API ASP.NET Core típica, cada requisição possui um escopo distinto. O experimento com UseCaseA e UseCaseB comprova que os dois usam a mesma instância na mesma requisição, mas uma instância diferente na próxima. Com isso, o instrutor conclui a comparação de Singleton, Transient e Scoped.
