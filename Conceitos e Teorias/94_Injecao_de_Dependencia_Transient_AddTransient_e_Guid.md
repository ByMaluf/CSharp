# Injeção de dependência: ciclo de vida Transient e AddTransient no ASP.NET Core

> **Aula 94 — Demonstração prática do ciclo de vida Transient**
>
> Continuação da aula 93, em que o instrutor configurou `AddSingleton<SingletonUseCase>()` e utilizou um `Guid` para mostrar a reutilização da mesma instância. Agora ele mantém a API de testes e demonstra o comportamento de **Transient**, comparando-o com Singleton.
>
> **Escopo:** esta aula utiliza somente a API experimental **Dependency Injection Test**, não o CashFlow. O ciclo de vida **Scoped** é anunciado para a próxima aula, mas não é implementado nesta gravação.

## Índice

1. Objetivo e contexto
2. Relembrando o experimento com Singleton
3. Estrutura do TransientUseCase
4. Adicionando a dependência ao controller
5. Registrando o serviço com AddTransient
6. Utilizando o TransientUseCase no endpoint
7. Testes com Swagger e debugger
8. Por que o Guid muda?
9. Transient versus Singleton
10. O significado técnico de "nova instância"
11. Quando utilizar Transient?
12. Relação com o CashFlow e próximos estudos
13. Cuidados e distinções técnicas
14. Resumo, perguntas de revisão e mapa mental

---

## 1. Objetivo e contexto

O instrutor inicia a aula exatamente do ponto em que terminou a anterior.

Na aula 93, ele configurou um **SingletonUseCase** no serviço de injeção de dependência, recebeu essa dependência pelo construtor do controller e observou que seu identificador **não mudava** ao repetir a requisição.

Agora, o objetivo é testar outra opção de registro: **Transient**.

Na fala, o termo aparece como "transit"; o nome correto no .NET é **Transient**, e a API de registro utilizada é:

```csharp
builder.Services.AddTransient<TransientUseCase>();
```

O foco é responder à pergunta:

**O que muda quando o container registra uma classe como Transient, em vez de Singleton?**

O professor demonstra o comportamento com novas requisições e compara os valores de `Guid` retornados.

### O projeto utilizado

A demonstração continua na API simples denominada **Dependency Injection Test**.

Ela contém um controller com endpoints destinados a observar os três ciclos de vida:

- Singleton, configurado na aula anterior;
- Transient, configurado nesta aula;
- Scoped, ainda reservado para a próxima.

Não é necessário criar essa API para acompanhar o raciocínio: o professor a utiliza apenas como um laboratório para visualizar o funcionamento da DI.

---

## 2. Relembrando o experimento com Singleton

A aula anterior demonstrou um serviço registrado com:

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
```

O construtor de `SingletonUseCase` gerava um `Guid` por meio de `Guid.NewGuid()`.

O método `Execute()` devolvia uma mensagem contendo o identificador armazenado.

### Resultado do Singleton

Ao chamar várias vezes o endpoint correspondente no Swagger, o professor observou **o mesmo identificador**.

```text
Requisição 1 ----> SingletonUseCase ----> Guid A
Requisição 2 ----> SingletonUseCase ----> Guid A
Requisição 3 ----> SingletonUseCase ----> Guid A
```

Isso aconteceu porque o container reutilizou a mesma instância do serviço nas resoluções seguintes, durante a vida do provider usado naquela execução da API.

A aula atual mantém essa implementação e acrescenta outra classe registrada com um ciclo de vida diferente.

---

## 3. Estrutura do TransientUseCase

O instrutor explica que criou um use case para o Transient semelhante ao SingletonUseCase.

Ambos possuem:

- um identificador gerado no construtor;
- o método `Execute()`;
- uma mensagem devolvida com o valor do identificador.

A diferença destacada na demonstração está no **nome do serviço e da mensagem** e, principalmente, no **ciclo de vida escolhido durante o registro**.

### Código reconstruído para estudo

```csharp
public class TransientUseCase
{
    private readonly Guid _id;

    public TransientUseCase()
    {
        _id = Guid.NewGuid();
    }

    public string Execute()
    {
        return $"From Transient with ID: {_id}";
    }
}
```

> **Origem do código:** esta é uma reconstrução didática da estrutura descrita pelo professor. A transcrição não fornece uma cópia literal da tela, nem permite confirmar se o identificador foi declarado especificamente como campo ou propriedade.

### Como funciona?

```text
Criar TransientUseCase
         |
         v
Executar o construtor
         |
         v
Guid.NewGuid()
         |
         v
Armazenar o identificador
         |
         v
Execute() devolve o valor
```

O identificador é gerado **quando uma instância do use case é criada**, não em cada chamada ao método `Execute()`.

Esse detalhe é essencial para interpretar corretamente o comportamento do Transient.

---

## 4. Adicionando a dependência ao controller

O professor retorna ao controller que já recebe o `SingletonUseCase`.

Ele acrescenta um novo campo privado para armazenar a dependência Transient:

```csharp
private readonly TransientUseCase _transientUseCase;
```

Depois, inclui a nova dependência como parâmetro do construtor e atribui seu valor ao campo.

### Exemplo reconstruído do construtor

```csharp
private readonly SingletonUseCase _singletonUseCase;
private readonly TransientUseCase _transientUseCase;

public MyDependencyController(
    SingletonUseCase singletonUseCase,
    TransientUseCase transientUseCase)
{
    _singletonUseCase = singletonUseCase;
    _transientUseCase = transientUseCase;
}
```

### Por que armazenar em um campo privado?

Porque a instância recebida no construtor precisará ser utilizada pelo método do endpoint posteriormente.

O fluxo é:

```text
Construtor do controller
           |
           v
Recebe TransientUseCase
           |
           v
Atribui a _transientUseCase
           |
           v
Endpoint utiliza o campo
```

O sublinhado inicial em `_transientUseCase` segue a convenção comum de nomenclatura para campos privados em C#.

### O que falta?

O professor alerta que **somente declarar o parâmetro no construtor não resolve a dependência**.

O container de DI precisa conhecer o serviço que foi solicitado.

É nesse momento que ocorre o registro no `Program.cs`.

---

## 5. Registrando o serviço com AddTransient

O instrutor abre o arquivo `Program.cs` e acrescenta:

```csharp
builder.Services.AddTransient<TransientUseCase>();
```

A configuração demonstrada passa a incluir:

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
builder.Services.AddTransient<TransientUseCase>();
```

### Analisando a sintaxe

| Elemento | Função |
|---|---|
| `builder.Services` | Coleção usada para registrar serviços |
| `AddTransient` | Registra o tipo com ciclo de vida Transient |
| `<TransientUseCase>` | Tipo concreto que será resolvido |
| `()` | Finaliza a chamada de registro |

### Qual é a função do registro?

Informar ao container que ele pode resolver `TransientUseCase` e **criar uma nova instância a cada resolução desse serviço**.

O registro utiliza diretamente a classe concreta, sem interface, exatamente como a demonstração anterior de Singleton.

### E se a classe não estiver registrada?

O professor explica que o controller exige uma instância de `TransientUseCase`. Se o container não conseguir resolver essa dependência, a ativação do controller pode falhar antes de o método do endpoint ser executado.

A mensagem de erro costuma indicar qual tipo não pôde ser resolvido.

**Lição:** declarar uma dependência no construtor e registrar o serviço no container são duas partes complementares da configuração da DI.

---

## 6. Utilizando o TransientUseCase no endpoint

Depois de registrar o serviço, o professor altera o endpoint Transient para utilizar a instância injetada.

A operação essencial é:

```csharp
var result = _transientUseCase.Execute();

return Ok(result);
```

### Exemplo didático da ação

```csharp
[HttpGet("transient")]
public IActionResult GetTransient()
{
    var result = _transientUseCase.Execute();

    return Ok(result);
}
```

O trecho acima é uma reconstrução ilustrativa: a transcrição não especifica a rota HTTP literal nem a declaração completa do controller.

### O que acontece agora?

1. Uma requisição chega ao endpoint Transient.
2. O ASP.NET Core ativa o controller.
3. Para construir o controller, resolve as dependências necessárias.
4. O container fornece uma instância de `TransientUseCase`.
5. O controller executa `_transientUseCase.Execute()`.
6. O endpoint retorna HTTP 200 com a mensagem contendo o `Guid`.

A ação não precisa fazer `new TransientUseCase()`, pois a instância é fornecida pelo container.

---

## 7. Testes com Swagger e debugger

O professor coloca breakpoints no controller e no endpoint e inicia a API com **F5**.

O Swagger é aberto, permitindo executar a ação Transient.

### Primeiro teste

1. Expandir o endpoint Transient no Swagger.
2. Clicar em **Try it out**.
3. Clicar em **Execute**.
4. A execução para no breakpoint do construtor do controller.
5. O professor verifica as duas dependências: SingletonUseCase e TransientUseCase.
6. Ambas aparecem com instâncias válidas, não nulas.
7. Ele avança com F10 e continua com F5.
8. A execução chega ao endpoint Transient.
9. O método `Execute()` é chamado.
10. A resposta exibe a mensagem e o identificador gerado.

O professor observa um primeiro `Guid` no resultado.

### Requisições seguintes

Ao clicar novamente em Execute no Swagger, a resposta apresenta outro valor de `Guid`.

Ele repete a operação diversas vezes e verifica que os identificadores observados são diferentes.

### Representação do comportamento observado

```text
GET /transient -> From Transient with ID: Guid A

GET /transient -> From Transient with ID: Guid B

GET /transient -> From Transient with ID: Guid C
```

Os identificadores A, B e C são **símbolos didáticos** para os diferentes valores vistos no vídeo, não GUIDs literais da gravação.

### O que o teste comprova?

No fluxo padrão demonstrado, **cada nova requisição leva à resolução de uma nova instância de TransientUseCase**, cujo construtor gera um novo identificador.

---

## 8. Por que o Guid muda?

A explicação central do instrutor é que, diferentemente de Singleton, o Transient provoca a criação de novas instâncias quando o container precisa resolver o serviço.

### Fluxo conceitual

```text
Primeira requisição
        |
        v
ASP.NET Core ativa o controller
        |
        v
Resolve TransientUseCase
        |
        v
Cria instância 1
        |
        v
Construtor gera Guid A
        |
        v
Resposta contém Guid A


Segunda requisição
        |
        v
ASP.NET Core ativa outro controller
        |
        v
Resolve TransientUseCase
        |
        v
Cria instância 2
        |
        v
Construtor gera Guid B
        |
        v
Resposta contém Guid B
```

### O papel do construtor

Cada instância nova executa o construtor, e o construtor chama `Guid.NewGuid()`.

Por isso os identificadores observados mudam.

### Ponto técnico decisivo

A regra correta de Transient é:

> **Uma nova instância é criada a cada vez que o serviço é resolvido pelo container.**

Isso **não** significa que o método `Execute()` cria uma instância nova toda vez que é chamado.

Se guardarmos uma instância de `TransientUseCase` em uma variável e chamarmos `Execute()` várias vezes **sobre aquele mesmo objeto**, o identificador não muda, porque o construtor não é executado novamente.

### Exemplo complementar

```csharp
// Suponha uma instância já recebida:
var primeiraChamada = _transientUseCase.Execute();
var segundaChamada = _transientUseCase.Execute();

// As duas chamadas acessam o mesmo objeto
// guardado em _transientUseCase.
```

Nesse exemplo, o valor será igual nas duas chamadas, desde que o campo não tenha sido substituído por outra instância.

A diferença observada no vídeo acontece porque **cada nova requisição recebe um controller ativado novamente**, que tem seu parâmetro Transient resolvido de novo.

---

## 9. Transient versus Singleton

O professor mantém os dois serviços registrados para que possamos contrastar os resultados.

| Aspecto | Singleton | Transient |
|---|---|---|
| Registro | `AddSingleton<T>()` | `AddTransient<T>()` |
| Criação típica | Uma instância por provider | Nova instância por resolução |
| Reutilização entre requisições | Sim, no mesmo provider | Não, quando resolvido novamente |
| Guid no exemplo | Permanece igual | Muda entre as requisições demonstradas |
| Objetivo didático | Mostrar instância compartilhada | Mostrar criação de novas instâncias |

### Representação visual

```text
SINGLETON

Requisição 1 -----+
                  |
Requisição 2 -----+----> Instância A -> Guid A
                  |
Requisição 3 -----+


TRANSIENT

Requisição 1 ----------> Instância A -> Guid A

Requisição 2 ----------> Instância B -> Guid B

Requisição 3 ----------> Instância C -> Guid C
```

### Por que a comparação é importante?

Os use cases são quase idênticos: ambos geram um `Guid` no construtor e retornam esse valor.

A diferença prática surge **do registro no container de DI**, não da implementação do método `Execute()`.

Esse é o ponto mais importante do experimento.

---

## 10. O significado técnico de "nova instância"

O professor diz que o serviço de DI "sempre vai fazer um `new`" quando a classe é registrada como Transient.

Essa frase transmite a ideia do experimento, mas precisa de uma distinção para ser interpretada corretamente.

### Resolução versus chamada de método

**Resolução:** o container é solicitado a fornecer uma instância do serviço.

**Chamada de método:** o programa utiliza uma instância que já existe.

Esses eventos não são a mesma coisa.

```text
Resolver serviço
       |
       v
Container cria uma instância Transient
       |
       v
Objeto é fornecido ao consumidor
       |
       +-- Execute()
       |
       +-- Execute()
       |
       +-- Execute()
```

As três chamadas acima podem acontecer **na mesma instância**, desde que o consumidor continue utilizando o objeto recebido.

### Exemplo complementar com duas resoluções

```csharp
// Demonstração conceitual com um IServiceProvider:
var first = serviceProvider.GetRequiredService<TransientUseCase>();
var second = serviceProvider.GetRequiredService<TransientUseCase>();

// Para um registro Transient por tipo,
// first e second são instâncias distintas.
```

O código acima **não é executado na aula**. Ele apenas ilustra a definição técnica com mais precisão.

### O que o vídeo efetivamente mostra?

Ele mostra várias **requisições HTTP** ao endpoint Transient. No ambiente demonstrado, o controller é ativado novamente para cada requisição e recebe uma nova instância do serviço.

É por isso que os resultados são diferentes.

---

## 11. Quando utilizar Transient?

O instrutor considera o Transient potencialmente excessivo porque o container cria novas instâncias em resoluções sucessivas.

Ele contrapõe esse comportamento à retenção de instâncias Singleton e anuncia que prefere estudar Scoped como uma alternativa.

### Avaliação da aula

- **Singleton:** mantém a mesma instância compartilhada e pode permanecer na memória por mais tempo.
- **Transient:** fornece uma nova instância a cada resolução, o que pode criar objetos com frequência.
- **Scoped:** será estudado na próxima aula, pois o professor considera esse ciclo de vida interessante para seu uso habitual.

**Cuidado:** a preferência do instrutor não significa que Transient seja inadequado por natureza.

### Complemento técnico

Transient pode ser uma boa escolha para serviços leves e sem estado compartilhado, quando desejamos que cada resolução receba seu próprio objeto.

Por exemplo, uma classe que realiza cálculos independentes e não precisa manter estado entre consumidores pode ser registrada como Transient.

A escolha deve considerar:

- custo de criação da instância;
- necessidade de compartilhar estado;
- ciclo de vida das dependências internas;
- descarte de recursos;
- contexto de utilização do serviço.

**Conclusão:** não existe um ciclo de vida universalmente melhor. O registro apropriado depende do comportamento exigido pelo serviço.

---

## 12. Relação com o CashFlow e próximos estudos

Embora o foco da demonstração seja a API Dependency Injection Test, o conceito prepara o retorno ao CashFlow.

Nas aulas anteriores:

1. Criamos e configuramos o `CashFlowDbContext`.
2. Fizemos uma inserção real de despesa no MySQL.
3. Removemos a dependência temporária entre Application e Infrastructure.
4. Começamos a estudar DI para organizar a construção e a entrega de dependências.

A aula atual mostra como registrar uma classe e recebê-la no construtor **sem utilizar `new` no controller**.

```text
CashFlow
   |
   v
Use cases precisam de colaboradores
   |
   v
Container de DI
   |
   v
Serviços registrados com um ciclo de vida
   |
   v
Dependências fornecidas pelo construtor
```

### O que ainda falta?

O curso ainda não demonstra nesta aula como aplicar `AddTransient` aos use cases reais do CashFlow nem como configurar o `CashFlowDbContext` por DI.

Esses detalhes dependem das próximas etapas de refatoração.

O professor encerra dizendo que o próximo tema será **Scoped**, seu ciclo de vida preferido entre as opções apresentadas.

---

## 13. Cuidados e distinções técnicas

Esta seção complementa a explicação do vídeo para evitar interpretações absolutas.

### 13.1 Transient significa nova instância por resolução

A criação ocorre quando o serviço é solicitado ao container.

**Não** é uma regra de nova instância por chamada a método, acesso à propriedade ou linha de código executada.

### 13.2 O mesmo objeto Transient pode ser reutilizado pelo consumidor

Se uma classe recebe um Transient no construtor e guarda a referência em um campo, pode utilizar essa mesma instância diversas vezes durante sua existência.

### 13.3 GUIDs não possuem garantia matemática de exclusividade absoluta

A aula usa `Guid.NewGuid()` como marcador de identidade e os valores observados são diferentes.

Embora colisões sejam extremamente improváveis para essa finalidade, não devemos tratar a geração de GUIDs como prova formal de unicidade absoluta.

### 13.4 Transient não significa descarte instantâneo

Uma instância pode permanecer referenciada pelo consumidor. Também existem regras especiais para serviços que implementam `IDisposable` e são resolvidos pelo container: seu descarte pode ocorrer apenas quando o escopo ou provider responsável for descartado.

Portanto, não devemos concluir que registrar um serviço como Transient garante menor consumo de memória em qualquer cenário.

### 13.5 Singleton não deve ser descartado apenas por ocupar memória

O professor prefere evitar Singletons por causa da retenção na memória, mas eles podem ser apropriados para serviços compartilhados, imutáveis ou sem estado mutável.

A questão é escolher o ciclo de vida adequado às responsabilidades, à concorrência e às dependências.

### 13.6 A falta de registro pode causar falha de ativação

Se o controller exige `TransientUseCase` e o container não consegue criá-lo nem resolvê-lo, a ação não chega a executar normalmente.

O problema não deve ser resolvido retirando a dependência sem entender por que ela é necessária; o correto é revisar o registro e sua cadeia de dependências.

### 13.7 O experimento compara registros, não implementações complexas

Os dois use cases apresentados têm comportamento quase igual. Isso facilita isolar a variável relevante: o **ciclo de vida escolhido durante o registro**.

Não há acesso ao MySQL nem alterações na arquitetura de persistência nessa gravação.

---

## 14. Resumo, perguntas de revisão e mapa mental

### Principais aprendizados

- A aula continua a API experimental criada para estudar DI.
- O foco agora é o ciclo de vida **Transient**.
- `TransientUseCase` gera um `Guid` no construtor e o devolve pelo método `Execute()`.
- O controller recebe uma dependência adicional do tipo `TransientUseCase`.
- A instância é armazenada em um campo privado.
- Em `Program.cs`, o serviço é registrado com `builder.Services.AddTransient<TransientUseCase>();`.
- O endpoint utiliza `_transientUseCase.Execute()`.
- O Swagger permite testar as chamadas repetidas.
- O professor observa que os valores de GUID mudam entre as requisições.
- Isso ocorre porque uma nova instância de TransientUseCase é criada em cada resolução do serviço nesse fluxo.
- Singleton reutiliza sua instância, enquanto Transient cria novas instâncias em resoluções subsequentes.
- **Transient não significa nova instância em cada chamada de `Execute()`.**
- O professor anuncia Scoped para a próxima aula.
- A implementação definitiva da DI no CashFlow não é realizada nesta gravação.

### Perguntas de revisão

1. Qual é a diferença de registro entre `AddSingleton` e `AddTransient`?
2. Por que é necessário adicionar `TransientUseCase` ao construtor do controller?
3. O que acontece se o container não puder resolver essa dependência?
4. Por que o instrutor gera um `Guid` no construtor do use case?
5. Por que o GUID muda quando ele executa repetidamente o endpoint Transient?
6. Qual é a diferença entre resolver um serviço e chamar um método na instância?
7. Se `Execute()` for chamado duas vezes no mesmo objeto Transient, o GUID necessariamente muda?
8. Por que os dois use cases praticamente idênticos exibem comportamentos diferentes?
9. Transient significa que a instância será imediatamente liberada da memória?
10. Qual ciclo de vida o professor anuncia para a próxima aula?

### Comparação final

| Pergunta | Resposta |
|---|---|
| Como registrar Transient? | `builder.Services.AddTransient<TransientUseCase>();` |
| Quem cria o objeto? | O container de DI, conforme a configuração |
| Quando ele cria outro objeto? | Quando há uma nova resolução do serviço |
| O Guid é gerado quando? | Na construção de cada instância |
| O Guid muda ao chamar `Execute()` duas vezes no mesmo objeto? | Não, no exemplo demonstrado |
| Por que muda em novas requisições? | Porque o serviço é resolvido novamente na ativação do controller |
| Qual é o próximo ciclo de vida estudado? | Scoped |

### Mapa mental

```text
                      AULA 94
                     TRANSIENT
                         |
                 API de demonstração
                         |
           +-------------+-------------+
           |                           |
        Program.cs                   Controller
           |                           |
  AddTransient<TransientUseCase>()      |
           |                   Construtor recebe
           |                   TransientUseCase
           |                           |
           +-------------+-------------+
                         |
                    Container DI
                         |
               Nova resolução do serviço
                         |
                         v
                 Nova instância
                         |
                    Construtor
                         |
                  Guid.NewGuid()
                         |
                         v
                   Execute() retorna
                    o Guid armazenado
                         |
               +---------+---------+
               |         |         |
             GET 1     GET 2     GET 3
               |         |         |
             Guid A    Guid B    Guid C

                 COMPARAÇÃO
                 Singleton: mesma instância
                 Transient: nova por resolução
                         |
                    PRÓXIMA AULA
                        Scoped
```

**Conclusão:** esta aula demonstra que `AddTransient` orienta o container a criar **uma nova instância a cada resolução do serviço**. Na API de testes, cada requisição resulta em uma nova resolução do `TransientUseCase`, gerando GUIDs diferentes. A comparação com Singleton mostra que a diferença vem principalmente do **ciclo de vida registrado no container**, não do código do método `Execute()`.
