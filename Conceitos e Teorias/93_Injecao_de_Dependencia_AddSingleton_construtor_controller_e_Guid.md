# Injeção de dependência na prática: AddSingleton, controllers e reutilização de instâncias

> **Aula 93 — Configurando DI no ASP.NET Core e entendendo o ciclo de vida Singleton**
>
> Continuação da aula 92, que apresentou a dependência entre classes, o problema do acoplamento e a injeção pelo construtor. Nesta aula, o professor demonstra essas ideias em uma API isolada, usando um `Guid` para visualizar o comportamento do ciclo de vida **Singleton**.

## Índice

1. Objetivo da aula e projeto de demonstração
2. Organização da API experimental
3. O que o controller faz inicialmente?
4. A classe SingletonUseCase e a criação do Guid
5. Primeiro teste: criação manual com new
6. Registro de serviços no Program.cs
7. Injeção pelo construtor do controller
8. Como a DI cria o controller e resolve o use case
9. Depurando a primeira requisição
10. Por que o Guid permanece igual em novas requisições?
11. O que acontece quando a API é reiniciada?
12. Diferença entre controller e dependência Singleton
13. Transient e Scoped: opções anunciadas
14. Relação com o CashFlow
15. Cuidados e complementos técnicos
16. Resumo, revisão e mapa mental

---

## 1. Objetivo da aula e projeto de demonstração

O instrutor continua a sequência sobre **Dependency Injection (DI)** e apresenta a primeira demonstração prática.

O objetivo é entender:

- como registrar uma classe no container de DI do ASP.NET Core;
- como receber uma instância dessa classe por meio do construtor de um controller;
- por que não precisamos instanciar manualmente o use case com `new`;
- o que significa configurar um serviço com o ciclo de vida **Singleton**;
- como o comportamento da instância pode ser observado em requisições repetidas.

**Importante:** o professor utiliza um projeto de demonstração chamado **Dependency Injection Test**. Ele explica que **não é necessário reproduzir essa API**: ela serve para observar os conceitos e, mais adiante, eles serão aplicados ao CashFlow.

A gravação não modifica efetivamente a arquitetura do CashFlow e não implementa os ciclos de vida Scoped e Transient. Estes serão estudados nas próximas aulas.

---

## 2. Organização da API experimental

A API demonstrativa é intencionalmente simples e concentra o código necessário para observar a injeção de dependência.

### Elementos citados na gravação

- Um controller chamado **MyDependencyController**.
- Três endpoints GET, dedicados aos ciclos de vida **Singleton**, **Transient** e **Scoped**.
- Três classes de use case, uma para cada ciclo de vida.
- O arquivo `Program.cs`, em que o professor registra os serviços.
- O Swagger, utilizado para chamar os endpoints.
- O depurador do Visual Studio, utilizado para acompanhar a criação dos objetos.

```text
Dependency Injection Test
          |
          +-- Program.cs
          |     registro dos serviços
          |
          +-- MyDependencyController
          |     GET Singleton
          |     GET Transient
          |     GET Scoped
          |
          +-- SingletonUseCase
          +-- TransientUseCase
          +-- ScopedUseCase
```

**Foco da aula:** apenas o endpoint e o use case associados ao **Singleton** são configurados e testados em detalhes.

---

## 3. O que o controller faz inicialmente?

O professor abre o Swagger e mostra os três endpoints.

Antes da implementação do exemplo, as ações praticamente não fazem nada: apenas retornam `Ok()`.

### Exemplo conceitual

```csharp
[HttpGet("singleton")]
public IActionResult GetSingleton()
{
    return Ok();
}
```

O código acima é **uma reconstrução didática**, não uma transcrição literal da tela.

### Resultado observado

Ao executar a ação, a API devolve **HTTP 200 OK**, sem um conteúdo significativo no corpo da resposta.

Esse passo confirma que os endpoints estão disponíveis, mas ainda não demonstra nenhum ciclo de vida.

---

## 4. A classe SingletonUseCase e a criação do Guid

O professor mostra o use case dedicado ao Singleton.

Ele contém:

1. Uma propriedade ou campo que armazena um identificador.
2. Um **construtor** que gera um novo `Guid`.
3. Um método `Execute` que devolve uma mensagem contendo o identificador.

### Código reconstruído para estudo

```csharp
public class SingletonUseCase
{
    private readonly Guid _id;

    public SingletonUseCase()
    {
        _id = Guid.NewGuid();
    }

    public string Execute()
    {
        return $"From Singleton with ID: {_id}";
    }
}
```

**Atenção:** os nomes do use case e do método refletem a gravação. A forma exata da propriedade ou campo não está disponível no texto falado; por isso, usamos um campo privado readonly como reconstrução didática. A mensagem também representa o formato narrado, sem reproduzir um resultado textual exato do Swagger.

### O que é `Guid`?

`Guid` é um tipo do .NET utilizado para representar identificadores de 128 bits.

`Guid.NewGuid()` produz um novo valor adequado a diversos cenários de identificação. No exemplo, ele é usado para identificar **qual instância da classe foi criada**.

### Por que gerar o Guid no construtor?

Porque o construtor é executado durante a criação da instância.

```text
new SingletonUseCase()
         |
         v
Executa o construtor
         |
         v
_id = Guid.NewGuid()
         |
         v
Execute() retorna o _id armazenado
```

Se a mesma instância for reutilizada, o `_id` armazenado continuará igual. Se uma nova instância for criada, ela receberá outro valor gerado.

### Interpolação de strings

O professor utiliza **string interpolation** para incluir o identificador na resposta:

```csharp
return $"From Singleton with ID: {_id}";
```

O prefixo `$` permite incorporar a expressão `_id` diretamente à string, utilizando chaves.

---

## 5. Primeiro teste: criação manual com new

Para recapitular a situação anterior à DI, o instrutor demonstra a criação direta do use case.

### Exemplo

```csharp
var useCase = new SingletonUseCase();

var result = useCase.Execute();
```

Nesse cenário:

1. O código executa `new SingletonUseCase()`.
2. O construtor produz um `Guid`.
3. O objeto armazena esse identificador.
4. `Execute()` devolve uma mensagem contendo o identificador.

### Por que o professor não quer manter essa abordagem?

Como explicado na aula 92, um use case pode evoluir e passar a depender de outros objetos ou configurações.

Se o controller for responsável por instanciá-lo manualmente, precisará conhecer os detalhes de criação do use case.

O professor decide então **registrar a classe no serviço de injeção de dependência**, de modo que sua instância seja fornecida ao controller.

---

## 6. Registro de serviços no Program.cs

A etapa mais importante da configuração ocorre no `Program.cs`.

O professor utiliza a coleção `builder.Services` e escolhe o método `AddSingleton`.

### Código demonstrado conceitualmente

```csharp
builder.Services.AddSingleton<SingletonUseCase>();
```

### Entendendo cada componente

| Trecho | Responsabilidade |
|---|---|
| `builder` | Objeto utilizado na configuração da aplicação |
| `Services` | Coleção de registros de serviços |
| `AddSingleton` | Registra o serviço com ciclo de vida Singleton |
| `<SingletonUseCase>` | Informa o tipo que será resolvido |
| `();` | Finaliza a chamada ao método |

A classe é registrada como **tipo concreto**. Não é necessário criar uma interface para este exemplo.

### Três opções que aparecem no editor

O instrutor mostra que existem três métodos relacionados ao ciclo de vida:

```csharp
builder.Services.AddTransient<AlgumServico>();
builder.Services.AddScoped<AlgumServico>();
builder.Services.AddSingleton<AlgumServico>();
```

A gravação seleciona **somente `AddSingleton`**. Os demais métodos aparecem como alternativas disponíveis, mas sua demonstração fica para próximas aulas.

### O que o registro informa?

Ele comunica ao container:

> Quando algum componente precisar de `SingletonUseCase`, resolva esse serviço segundo a política Singleton.

O registro não exige que o controller construa manualmente o use case.

---

## 7. Injeção pelo construtor do controller

Depois de registrar o use case, o professor altera o controller.

Ele declara um membro privado para armazenar a dependência e cria um construtor que recebe `SingletonUseCase`.

### Exemplo reconstruído

```csharp
public class MyDependencyController : ControllerBase
{
    private readonly SingletonUseCase _singletonUseCase;

    public MyDependencyController(
        SingletonUseCase singletonUseCase)
    {
        _singletonUseCase = singletonUseCase;
    }

    [HttpGet("singleton")]
    public IActionResult GetSingleton()
    {
        var result = _singletonUseCase.Execute();

        return Ok(result);
    }
}
```

**Nota:** o trecho é uma reconstrução didática do padrão mostrado. A transcrição não registra todos os atributos, o namespace ou a declaração integral do controller; a rota é ilustrativa.

### O que mudou?

**Antes:**

```csharp
var useCase = new SingletonUseCase();
var result = useCase.Execute();
```

**Depois:**

```csharp
var result = _singletonUseCase.Execute();
```

A instância do use case é **recebida no construtor** do controller, e não criada diretamente dentro do endpoint.

### Passo a passo

1. O construtor declara que precisa de `SingletonUseCase`.
2. O ASP.NET Core resolve essa dependência usando o container.
3. A instância recebida é armazenada no campo `_singletonUseCase`.
4. O endpoint utiliza o objeto já fornecido.

```text
Controller
    |
    +-- construtor recebe SingletonUseCase
    |
    +-- campo _singletonUseCase
    |
    +-- endpoint chama Execute()
```

O uso de `_singletonUseCase` como campo privado permite acessar a mesma dependência em diferentes métodos do controller, se necessário.

---

## 8. Como a DI cria o controller e resolve o use case

O professor antecipa uma dúvida: **a injeção de dependência funciona em controllers?**

Sim. Um controller é uma classe utilizada pelo ASP.NET Core para processar solicitações destinadas aos seus endpoints.

O framework é responsável por ativar essa classe e satisfazer as dependências declaradas em seu construtor.

### Fluxo conceitual

```text
Requisição HTTP
       |
       v
ASP.NET Core seleciona o controller
       |
       v
Examina o construtor
       |
       v
Precisa de SingletonUseCase
       |
       v
Serviço está registrado?
       |
       +-- Não -> falha na ativação da classe
       |
       +-- Sim -> resolve SingletonUseCase
                         |
                         v
               Injeta no construtor
                         |
                         v
                  Executa endpoint
```

### Por que registrar o serviço antes?

O professor explica que, se uma dependência exigida pelo controller não for encontrada, ocorrerá um erro de resolução, e a ação não conseguirá ser executada normalmente.

O container precisa saber **como atender às dependências necessárias**.

### O controller também é instanciado?

Sim. No fluxo normal de controllers do ASP.NET Core, o framework cria ou ativa uma instância quando precisa processar a requisição.

**Distinção essencial:** isso não significa que todas as dependências do controller sejam recriadas a cada requisição. O ciclo de vida de cada dependência segue o registro realizado no container.

---

## 9. Depurando a primeira requisição

Com o serviço registrado e o controller modificado, o professor adiciona breakpoints e executa a API pelo Visual Studio.

### Sequência observada

1. A API inicia e o Swagger é aberto.
2. O endpoint Singleton é selecionado.
3. O professor clica em **Try it out**.
4. Em seguida, clica em **Execute**.
5. O debugger para no construtor do controller.
6. O professor observa que o parâmetro `singletonUseCase` **não é nulo**.
7. A referência é armazenada no campo privado.
8. A execução continua até o endpoint.
9. O endpoint chama `Execute()`.
10. O Swagger mostra a mensagem contendo o Guid.

### O que esse teste confirma?

Que a instância do use case foi fornecida ao controller pelo mecanismo de DI.

```text
Swagger
   |
   v
GET Singleton
   |
   v
Construtor do controller
   |
   v
SingletonUseCase injetado
   |
   v
Endpoint chama Execute()
   |
   v
HTTP 200 com mensagem e Guid
```

O instrutor utiliza F5 e F10 para continuar a execução e avançar durante a depuração.

### O que ainda não foi comprovado?

O primeiro acesso, sozinho, comprova que a dependência foi resolvida e utilizada. Para compreender a característica especial do Singleton, o professor realiza **diversas requisições** e compara os identificadores retornados.

---

## 10. Por que o Guid permanece igual em novas requisições?

Esta é a demonstração central da aula.

O professor executa o mesmo endpoint repetidamente no Swagger.

### Resultado observado

O começo e o final do `Guid` retornado permanecem iguais nas várias respostas.

Como o identificador foi criado no construtor de `SingletonUseCase`, isso indica que a **mesma instância do use case está sendo reutilizada**.

### Como funciona o Singleton?

No cenário demonstrado, quando o serviço é resolvido pela primeira vez, o container cria uma instância do `SingletonUseCase` e a mantém disponível.

Nas resoluções seguintes, ele fornece essa mesma instância.

```text
Primeira requisição
       |
       v
Resolver SingletonUseCase
       |
       v
Instância ainda não existe
       |
       v
Criar instância + gerar Guid
       |
       v
Guardar e fornecer a instância
       |
       v
Resposta com Guid A


Segunda requisição
       |
       v
Resolver SingletonUseCase
       |
       v
Instância já existe
       |
       v
Reutilizar a mesma instância
       |
       v
Resposta com Guid A


Terceira requisição
       |
       v
Reutilizar a instância
       |
       v
Resposta com Guid A
```

### O Guid não muda porque o serviço não é recriado

O método `Execute()` **não gera um novo Guid**.

Ele devolve o identificador que foi atribuído **quando o objeto foi construído**.

Se a mesma instância é reutilizada, seu campo `_id` não muda.

**Ponto central:** repetir chamadas a um método não significa recriar o objeto que contém esse método.

### O que significa "criado na primeira vez"?

O professor explica que, normalmente, o container cria o serviço **quando ele é necessário pela primeira vez**, e não simplesmente porque a linha `AddSingleton` foi executada.

O registro configura a forma de resolução; a criação costuma ocorrer na primeira resolução.

**Complemento técnico:** existem outras formas de registrar serviços, inclusive fornecendo uma instância preexistente. Portanto, a criação sob demanda descreve a configuração específica demonstrada com `AddSingleton<SingletonUseCase>()`, não todo registro Singleton possível.

---

## 11. O que acontece quando a API é reiniciada?

O professor interrompe a aplicação e a executa novamente.

Ao chamar o endpoint Singleton no Swagger, observa um **novo identificador**.

### Sequência observada

```text
Execução 1 da API
       |
       v
SingletonUseCase criado
       |
       v
Guid A
       |
       v
Várias requisições -> Guid A
       |
       v
Parar a API
       |
       v
Executar novamente
       |
       v
Novo SingletonUseCase
       |
       v
Guid B
       |
       v
Várias requisições -> Guid B
```

O professor repete o teste e verifica que o identificador continua igual enquanto a mesma execução permanece ativa.

### Por que muda após reiniciar?

Porque a execução anterior e seu container de serviços deixam de existir. Uma nova execução cria um novo ambiente de DI e, quando necessário, uma nova instância Singleton.

### Nuance técnica importante

O tempo de vida de um Singleton é associado **ao container/provedor de serviços no qual ele foi registrado**, e não a uma propriedade global permanente da classe.

Em uma aplicação ASP.NET Core típica, o container raiz permanece disponível durante a vida daquela instância da aplicação.

Se o processo for reiniciado ou o container descartado, a instância anterior deixa de ser utilizada.

O professor comenta também sobre fechar o navegador da demonstração. Isso pode encerrar a aplicação dependendo de como o ambiente de depuração está configurado, mas **fechar o Swagger não encerra necessariamente qualquer API ASP.NET Core**.

---

## 12. Diferença entre controller e dependência Singleton

Um dos pontos mais importantes para evitar confusão:

> O ciclo de vida Singleton pertence ao **serviço registrado como Singleton**, não automaticamente ao controller que o consome.

### No exemplo

- O controller atende requisições HTTP.
- O framework pode ativar um controller a cada requisição, conforme a configuração.
- A dependência `SingletonUseCase` é reutilizada pelo container.
- O construtor do controller pode executar várias vezes sem que o construtor do use case Singleton seja executado novamente.

### Comparação

| Elemento | Comportamento no exemplo |
|---|---|
| MyDependencyController | Ativado para processar as requisições |
| SingletonUseCase | Resolvido como Singleton |
| Construtor do controller | Pode executar em cada requisição |
| Construtor do SingletonUseCase | Executado quando a instância Singleton é criada |
| Campo Guid do use case | Permanece igual enquanto a mesma instância for reutilizada |

```text
Requisição 1 -> Controller 1 ----+
                                 |
Requisição 2 -> Controller 2 ----+--> SingletonUseCase
                                 |      Guid A
Requisição 3 -> Controller 3 ----+
```

Este é um exemplo conceitual do ciclo de vida demonstrado.

---

## 13. Transient e Scoped: opções anunciadas

O instrutor lembra que existem três alternativas principais para configurar o ciclo de vida dos serviços.

| Ciclo de vida | Método de registro | Situação nesta aula |
|---|---|---|
| Transient | `AddTransient` | Apenas mencionado |
| Scoped | `AddScoped` | Apenas mencionado |
| Singleton | `AddSingleton` | Configurado e testado |

A demonstração prática fica restrita ao **Singleton**.

### O que será estudado a seguir?

O professor encerra a gravação anunciando que os outros dois tipos serão apresentados nas próximas aulas.

**Nota técnica complementar, sem antecipar a prática:** os ciclos de vida se distinguem pela forma como o container cria, compartilha e reutiliza instâncias. O comportamento de Scoped e Transient será mais fácil de comparar depois de compreendermos o Singleton.

---

## 14. Relação com o CashFlow

A API utilizada nesta gravação não é o CashFlow. O instrutor a apresenta como um projeto isolado para facilitar a visualização do funcionamento da DI.

Entretanto, o conceito prepara uma necessidade real do CashFlow.

Na aula 91, a Application precisou acessar diretamente a Infrastructure para instanciar `CashFlowDbContext`.

Depois do teste de persistência, o professor removeu a referência temporária, pois o objetivo é organizar o código respeitando a arquitetura adotada no curso.

### O que aprendemos nesta aula para aplicar depois?

Que um componente pode receber suas dependências pelo construtor e que o container pode ser configurado para fornecer os serviços necessários.

```text
Aula 91
Use case cria CashFlowDbContext diretamente
           |
           v
Dependência temporária removida
           |
           v
Aulas de DI
Aprender a registrar e resolver dependências
           |
           v
Futura refatoração do CashFlow
```

### Cuidado com a escolha do ciclo de vida

A gravação **não escolhe Singleton para o DbContext do CashFlow**.

O fato de o instrutor demonstrar `AddSingleton` com um use case simples e um Guid não significa que essa configuração seja adequada para serviços que mantêm estado de uma requisição ou para um contexto de banco de dados.

O ciclo de vida apropriado será discutido na sequência.

---

## 15. Cuidados e complementos técnicos

As observações desta seção são explicações técnicas adicionais, separadas da demonstração do professor.

### 15.1 Singleton não significa "uma instância para todo o computador"

A instância é compartilhada no escopo do **provedor de serviços correspondente**.

Duas aplicações ou dois containers distintos podem criar instâncias diferentes do mesmo tipo Singleton.

Em um sistema distribuído com múltiplas instâncias da API, não existe garantia de que todas compartilhem o mesmo objeto em memória.

### 15.2 Singleton não significa que o construtor execute durante AddSingleton

Ao usar `builder.Services.AddSingleton<SingletonUseCase>()`, o registro normalmente é feito antes da criação efetiva do objeto. A instância pode ser criada sob demanda, quando o serviço é resolvido pela primeira vez.

### 15.3 Singleton não é sempre uma escolha ruim

O professor relata preferência pessoal por evitar Singleton devido à retenção de objetos na memória.

É verdade que um Singleton normalmente permanece referenciado durante a vida do provider e pode consumir memória por mais tempo.

Porém, **não é correto concluir que Singleton deve ser sempre evitado**. Esse ciclo de vida pode ser adequado para serviços compartilhados, imutáveis ou sem estado mutável, quando suas dependências e seu uso concorrente são compatíveis.

O que importa é avaliar as responsabilidades do serviço, o custo de construção, o estado interno e o ciclo de vida de suas dependências.

### 15.4 Atenção à concorrência e ao estado compartilhado

Como o mesmo Singleton pode ser utilizado por múltiplas requisições, seu estado pode ser acessado por várias threads.

```text
Requisição A ----+
                 |
Requisição B ----+--> Instância Singleton compartilhada
                 |
Requisição C ----+
```

Se um Singleton altera dados internos compartilhados, é necessário considerar segurança de concorrência (*thread safety*).

No exemplo da aula, o `Guid` é criado uma vez e apenas lido. Isso ajuda a manter a demonstração simples.

### 15.5 Um Singleton não deve capturar indiscriminadamente serviços Scoped

Uma dependência Singleton pode viver por muito mais tempo do que um serviço Scoped.

Por esse motivo, não devemos injetar diretamente um serviço Scoped em um Singleton sem compreender as regras de escopo. O container do .NET pode detectar essa combinação incorreta e lançar uma exceção quando a validação de escopos está habilitada.

Isso é particularmente relevante para um `DbContext`, que costuma ser registrado como Scoped em aplicações ASP.NET Core.

### 15.6 GUID não é o mesmo que identidade da classe

O `Guid` armazenado no exemplo funciona como **marcador observável** da instância do use case.

Um Guid é um valor; o objeto possui sua própria identidade de referência. No caso demonstrado, verificar o mesmo Guid é uma forma prática de ilustrar a reutilização, pois o valor é gerado no construtor e não é modificado.

### 15.7 Singleton e descarte de recursos

Instâncias criadas e controladas pelo container podem ser descartadas conforme seu ciclo de vida e as regras de descarte do provider.

Assim, a expressão "fica na memória até parar a API" descreve o comportamento usual observado, mas não precisa ser interpretada como uma regra sobre coleta de lixo independente da vida do container.

### 15.8 Configuração de serviços não substitui arquitetura

A DI fornece dependências, mas não define, sozinha, quais camadas devem conhecer outras ou quais contratos devem existir. A refatoração do CashFlow envolverá também decisões sobre abstrações e direção de dependências.

---

## 16. Resumo, revisão e mapa mental

### Principais aprendizados

- A aula utiliza uma API independente chamada Dependency Injection Test.
- Existem três endpoints GET para estudar Singleton, Transient e Scoped.
- O foco inicial está apenas no Singleton.
- O use case recebe um `Guid` gerado no construtor com `Guid.NewGuid()`.
- A primeira abordagem utiliza `new SingletonUseCase()` diretamente.
- A configuração de DI é registrada no `Program.cs`.
- O professor utiliza `builder.Services.AddSingleton<SingletonUseCase>()`.
- O controller passa a receber `SingletonUseCase` no construtor.
- O serviço é armazenado em um campo privado e utilizado pelo endpoint.
- O ASP.NET Core ativa controllers e fornece as dependências registradas.
- Breakpoints mostram que o parâmetro recebido no construtor não é nulo.
- Requisições repetidas retornam o mesmo Guid porque a mesma instância Singleton é reutilizada.
- Quando a API é reiniciada, um novo container é criado e o identificador muda no exemplo.
- O Singleton é criado quando necessário na configuração apresentada.
- O ciclo de vida é do serviço registrado, não necessariamente do controller.
- Os ciclos de vida Transient e Scoped são anunciados para as próximas aulas.
- A aplicação desses conceitos ao CashFlow ocorrerá posteriormente.

### Perguntas de revisão

1. Qual é a finalidade da API Dependency Injection Test?
2. Por que o instrutor gera um Guid no construtor do SingletonUseCase?
3. O que faz `AddSingleton<SingletonUseCase>()`?
4. Onde o registro do serviço é realizado na aplicação?
5. Como o controller recebe o use case sem executar `new`?
6. Por que o parâmetro do construtor do controller pode receber a instância automaticamente?
7. Por que o Guid permanece igual em chamadas repetidas?
8. O construtor do controller e o construtor do SingletonUseCase têm necessariamente a mesma frequência de execução?
9. Por que o Guid muda depois de reiniciar a aplicação?
10. Qual é a diferença entre vida da instância e vida do processo?
11. O que acontece se o container não consegue resolver uma dependência exigida?
12. Que cuidados são necessários ao utilizar Singleton com estado mutável?

### Mapa mental

```text
                        AULA 93
                DEPENDENCY INJECTION
                           |
                 API de demonstração
               Dependency Injection Test
                           |
              +------------+------------+
              |                         |
         Program.cs                Controller
              |                         |
     AddSingleton<UseCase>()      Construtor recebe
              |                    SingletonUseCase
              |                         |
              +------------+------------+
                           |
                           v
                      Container DI
                           |
                   Primeira resolução
                           |
                           v
                 new SingletonUseCase()
                           |
                           v
                    Guid.NewGuid()
                           |
                           v
                 Instância compartilhada
                           |
           +---------------+---------------+
           |               |               |
        GET 1            GET 2           GET 3
           |               |               |
           +---------------+---------------+
                           |
                           v
                     Mesmo Guid
                           |
                  Reiniciar a API
                           |
                           v
                  Novo provider/instância
                           |
                           v
                       Novo Guid
                           |
                           v
                    PRÓXIMAS AULAS
                  Transient e Scoped
```

**Conclusão:** a aula demonstra que a injeção de dependência permite ao controller **receber** um use case já resolvido pelo container. Com `AddSingleton`, a mesma instância do serviço é compartilhada entre resoluções, o que explica por que o Guid gerado no construtor permanece igual durante chamadas sucessivas no mesmo ciclo de vida do container.
