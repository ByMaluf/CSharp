# Tasks e programação assíncrona em C#: async, await, Task.Delay e Task<T>

> **Aula 104 — Programação assíncrona com Task, parte 1**
>
> Depois das refatorações de persistência do CashFlow, o professor inicia uma sequência específica sobre **Tasks**. Nesta primeira aula, utiliza um endpoint HTTP GET de um projeto de demonstração, um método chamado `MyAsyncMethod`, a operação `Task.Delay(20000)` e o depurador do Visual Studio para mostrar como criar, iniciar, aguardar e obter resultados de tarefas.
>
> **Limite da aula:** a gravação não refatora os repositórios, o `UnitOfWork`, os use cases ou os métodos de persistência do CashFlow. O instrutor anuncia que continuará a explicação de Tasks na parte 2 e só depois aplicará o conteúdo ao projeto.
>
> **Separação entre aula e complementos:** as demonstrações são reconstituídas a partir da transcrição. As ressalvas sobre threads, execução em segundo plano e o comportamento formal de `async`/`await` são correções técnicas importantes, apresentadas como complementos, sem atribuí-las ao código literal do instrutor.

## Índice

1. Objetivo e contexto
2. O que é Task?
3. Quando programação assíncrona é útil?
4. Assinaturas: Task, Task<T> e async
5. Chamando um método e obtendo uma Task
6. Método versus delegate versus execução
7. Simulando uma operação demorada com Task.Delay
8. Primeiro teste: Task.Delay sem await
9. Segundo teste: await dentro do método, mas não no controller
10. Por que o endpoint responde antes de a operação terminar?
11. Terceiro teste: await também no endpoint
12. O que realmente significa await?
13. Iniciando uma Task, executando outro código e aguardando depois
14. Retornando resultados com Task<int>
15. Código reconstruído dos cenários
16. Estados de uma Task e inspeção no depurador
17. Relação com APIs, banco de dados e I/O
18. Equívocos frequentes e correções técnicas
19. Boas práticas e cuidados com tarefas não aguardadas
20. O que fica para a parte 2
21. Perguntas de revisão
22. Resumo e mapa mental

---

## 1. Objetivo e contexto

O professor abre a gravação informando que o foco é **Tasks** e programação assíncrona.

A proposta inicial é compreender o comportamento dessas operações, antes de alterar os componentes reais do CashFlow.

### O que é demonstrado?

- o significado de uma `Task`;
- operações assíncronas e seus resultados;
- a diferença entre `Task` e `Task<T>`;
- a declaração de métodos com `async`;
- uma simulação de demora com `Task.Delay`;
- a palavra-chave `await`;
- o efeito de aguardar uma operação dentro de um método, mas não no método que o chamou;
- como iniciar uma operação, executar outro código e só depois aguardar;
- como receber um resultado inteiro de uma `Task<int>`.

### Relação com o CashFlow

Nas aulas anteriores, `ExpensesRepository` e `UnitOfWork` utilizavam métodos síncronos:

```csharp
_dbContext.Expenses.Add(expense);
_dbContext.SaveChanges();
```

O professor observa que operações como banco de dados, upload de arquivos e envio de e-mails podem levar tempo desconhecido.

Ele afirma que, mais adiante, utilizará Tasks para refatorar o código do projeto.

**Não é feita nesta aula nenhuma substituição de `SaveChanges` por `SaveChangesAsync` nem alteração de `Commit` para `CommitAsync`.**

---

## 2. O que é Task?

O instrutor define `Task` como uma classe utilizada para representar uma operação que pode ser executada de forma assíncrona.

Em C#, o namespace relevante é:

```csharp
using System.Threading.Tasks;
```

### O que a Task representa?

Uma Task pode representar uma operação ainda em andamento, concluída com sucesso, cancelada ou finalizada com erro.

Ela permite que outro trecho de código saiba **quando a operação termina** e, no caso genérico, receba o resultado.

### Exemplo simples

```csharp
Task tarefa = Task.Delay(2000);
```

Nesse caso, `tarefa` representa a conclusão de um atraso de dois segundos.

### Task não é a própria thread

Uma distinção fundamental:

- **Task:** representa uma operação e sua conclusão.
- **Thread:** é um recurso de execução do sistema operacional.

Uma operação assíncrona pode ficar aguardando I/O ou um temporizador **sem manter uma thread bloqueada durante a espera**.

Portanto, a explicação oral de Task como algo que "executa em segundo plano" é uma analogia introdutória, e **não a definição rigorosa**.

### Task e Task<T>

| Tipo | Significado |
|---|---|
| `Task` | Representa uma operação assíncrona sem resultado de valor |
| `Task<T>` | Representa uma operação assíncrona cujo resultado é do tipo `T` |

Uma Task pode ser aguardada com `await`, permitindo retomar a continuação quando sua operação for concluída.

---

## 3. Quando programação assíncrona é útil?

O professor destaca operações de duração imprevisível, especialmente relacionadas a sistemas externos.

### Exemplos citados

1. Consultas e gravações em banco de dados.
2. Upload de fotografias ou arquivos.
3. Envio de e-mails.
4. Outras operações que podem levar algum tempo até retornar.

### O problema de operações de I/O síncronas

Em uma API, se uma operação síncrona bloqueante aguarda uma resposta de banco, rede ou disco, pode manter uma thread ocupada durante o período de espera.

### O que a forma assíncrona pode melhorar?

Quando existe uma API assíncrona não bloqueante, a execução pode ser suspensa enquanto aguarda o resultado da operação externa.

Nesse intervalo, a thread pode ficar disponível para outros trabalhos.

```text
Chamada síncrona bloqueante
    |
    +-- Solicita I/O
    |
    +-- Thread aguarda ocupada/bloqueada
    |
    +-- I/O termina
    |
    +-- Continua


Chamada assíncrona de I/O
    |
    +-- Inicia operação
    |
    +-- await encontra operação incompleta
    |
    +-- Suspende a continuação do método
    |
    +-- Thread fica disponível
    |
    +-- Operação conclui
    |
    +-- Continuação é agendada e executada
```

### Assíncrono é o mesmo que paralelo?

**Não.**

- **Assíncrono:** a execução não precisa ficar bloqueada aguardando uma operação concluir; a continuação pode ocorrer depois.
- **Paralelo:** duas ou mais partes de trabalho realmente executam ao mesmo tempo, potencialmente em diferentes threads ou processadores.

O instrutor menciona paralelismo como uma motivação, mas os conceitos não são sinônimos.

O exemplo de `Task.Delay` demonstra **espera assíncrona**, não uma operação de CPU sendo processada em outra thread.

---

## 4. Assinaturas: Task, Task<T> e async

No Visual Studio, o professor apresenta um método inicialmente síncrono:

```csharp
private void MyAsyncMethod()
{
}
```

Depois, altera o tipo de retorno para `Task` e adiciona o modificador `async`.

### Forma apresentada

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);
}
```

### O papel dos elementos

| Elemento | Função |
|---|---|
| `private` | Restringe acesso ao método à própria classe |
| `async` | Permite usar `await` no corpo do método e habilita a transformação assíncrona pelo compilador |
| `Task` | Representa a conclusão assíncrona sem um valor de retorno |
| `MyAsyncMethod` | Nome escolhido na demonstração |
| `await` | Aguarda logicamente a operação, suspendendo a continuação se necessário |

### Métodos que devolvem resultado

O professor explica que um método pode produzir um inteiro, string, booleano ou objeto.

Nesse caso, utiliza-se `Task<T>`:

```csharp
private async Task<int> MyAsyncMethod()
{
    await Task.Delay(20_000);
    return 7;
}
```

O `int` entre `<` e `>` representa o tipo do resultado.

### Precisão: async não é obrigatório para devolver Task

A gravação simplifica a explicação ao sugerir que, sem `async`, o método seria necessariamente síncrono.

É perfeitamente válido devolver uma Task **sem** a palavra-chave `async`:

```csharp
private Task MyAsyncMethod()
{
    return Task.Delay(20_000);
}
```

Nesse exemplo, o método devolve a Task da operação sem possuir nenhum `await` próprio.

O modificador `async` é necessário para usar `await` naquele corpo de método (desconsiderando outras formas específicas de composição), mas **não é requisito universal para um método retornar `Task`**.

---

## 5. Chamando um método e obtendo uma Task

O professor mostra que a chamada ao método devolve uma referência à tarefa.

### Exemplo

```csharp
Task task = MyAsyncMethod();
```

Ou, com inferência de tipo:

```csharp
var task = MyAsyncMethod();
```

Nos dois casos, a variável recebe o objeto `Task` retornado pela invocação.

### O que acontece nessa linha?

```text
MyAsyncMethod()
       |
       v
Inicia a execução do método
       |
       v
Retorna uma Task
       |
       v
task armazena a referência à Task
```

O trabalho assíncrono pode continuar incompleto quando a variável é atribuída.

### Chamou o método ou apenas guardou sua referência?

A distinção depende dos parênteses:

```csharp
var task = MyAsyncMethod(); // Invoca o método, obtém Task.
```

Sem os parênteses, podemos ter uma referência a método (*method group*) quando há contexto de delegate compatível:

```csharp
Func<Task> executar = MyAsyncMethod; // Não invoca.

Task task = executar(); // Agora invoca.
```

Esse contraste é o que o professor procura demonstrar ao explicar as diferenças entre usar o nome de um método e executar sua chamada.

---

## 6. Método versus delegate versus execução

A transcrição menciona um trecho em que o professor cria uma variável a partir de `MyAsyncMethod` e depois demonstra o uso de parênteses.

Para interpretar corretamente a sintaxe, é útil separar três conceitos.

### 6.1 Referência a um método

```csharp
Func<Task> executar = MyAsyncMethod;
```

`executar` é um delegate. Ele referencia um método que poderá ser invocado posteriormente.

### 6.2 Invocação do método

```csharp
Task tarefa = MyAsyncMethod();
```

Aqui, o método é efetivamente invocado, e `tarefa` armazena o resultado dessa chamada — uma Task.

### 6.3 Aguardar a conclusão

```csharp
await tarefa;
```

Aqui, o fluxo assíncrono não prossegue para a instrução seguinte até a conclusão bem-sucedida da Task ou a propagação de erro/cancelamento.

### Comparação

| Código | O que representa? |
|---|---|
| `Func<Task> executar = MyAsyncMethod;` | Referência a uma função que ainda não foi chamada |
| `Task tarefa = MyAsyncMethod();` | Invoca o método e obtém a tarefa |
| `await tarefa;` | Aguarda a conclusão da tarefa |

Essa distinção evita confundir "armazenar a função" com "armazenar a Task retornada pela função".

---

## 7. Simulando uma operação demorada com Task.Delay

O professor utiliza:

```csharp
Task.Delay(20000);
```

O argumento é medido em **milissegundos**.

### Conversão apresentada

```text
1 segundo = 1.000 milissegundos

20 segundos = 20.000 milissegundos
```

Por isso, `Task.Delay(20_000)` representa um atraso de 20 segundos.

### Exemplo com await

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);
}
```

### O que Task.Delay realmente faz?

`Task.Delay` devolve uma Task que será concluída depois do intervalo especificado, salvo cancelamento.

Ele **não é equivalente a bloquear uma thread por 20 segundos**.

É uma operação de espera assíncrona baseada em temporização.

### Por que o professor escolhe esse exemplo?

Para tornar visível no debugger o intervalo entre a invocação da tarefa e sua conclusão.

Na aula, o professor utiliza breakpoints, F5, F10 e F11 para acompanhar o fluxo.

---

## 8. Primeiro teste: Task.Delay sem await

O primeiro experimento coloca `Task.Delay(20000)` dentro do método **sem aguardá-lo**.

### Código ilustrativo

```csharp
private async Task MyAsyncMethod()
{
    Task.Delay(20_000);
}
```

### Resultado observado na gravação

O professor inicia a API, utiliza o Swagger e entra na execução pelo debugger.

Quando passa pela linha do `Task.Delay`, o método **não fica parado esperando os 20 segundos**.

O endpoint continua e devolve uma resposta HTTP **200 OK** com um identificador de exemplo.

### Por que isso acontece?

`Task.Delay(20_000)` cria e devolve uma Task.

Como essa Task é descartada sem `await` nem `return`, o método segue adiante.

```text
MyAsyncMethod()
       |
       v
Task.Delay(20_000)
       |
       +-- Cria Task de atraso
       |
       +-- Task não é aguardada
       |
       v
Método termina sem esperar o atraso
       |
       v
Endpoint pode retornar OK
```

### Alertas de compilação possíveis

Um código como esse pode produzir avisos relacionados a:

- método `async` que não contém `await` (`CS1998`);
- chamada de Task cuja conclusão não é aguardada (`CS4014`, dependendo do contexto).

### Por que é um ponto importante?

Uma operação que **devolve Task** não é automaticamente aguardada pelo código chamador.

Para coordenar a conclusão, é necessário usar mecanismos apropriados, como `await` ou retornar a Task para que outro componente a aguarde.

### Forma alternativa válida

Se a única operação do método é devolver o atraso, podemos escrever:

```csharp
private Task MyAsyncMethod()
{
    return Task.Delay(20_000);
}
```

Assim, a Task do atraso não é descartada; ela é devolvida ao chamador.

**Essa alternativa é um complemento técnico, não uma mudança aplicada pelo professor nesse primeiro teste.**

---

## 9. Segundo teste: await dentro do método, mas não no controller

O professor corrige o método auxiliar acrescentando `await`.

### Método auxiliar

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);

    var x = 7; // Linha usada apenas para colocar breakpoint.
}
```

Agora a continuação do método auxiliar, representada pela atribuição `x = 7`, deve ocorrer depois do atraso.

### Controller continua sem await

O endpoint ainda chama:

```csharp
MyAsyncMethod();
```

e segue para:

```csharp
return Ok(...);
```

### Resultado observado no depurador

Apesar de haver `await Task.Delay(20_000)` dentro de `MyAsyncMethod`, o endpoint devolve **HTTP 200** antes da conclusão do atraso.

Depois, o breakpoint localizado na continuação do método auxiliar é atingido.

### Por que ocorre assim?

O `await` está dentro do **método auxiliar**. Ele suspende a continuação desse método quando a Task do atraso ainda não foi concluída.

O **controller**, entretanto, não está aguardando a Task retornada por `MyAsyncMethod`.

```text
Controller
    |
    v
MyAsyncMethod()
    |
    v
await Task.Delay(20_000)
    |
    +-- Suspende a continuação do método auxiliar
    |
    +-- Devolve uma Task incompleta ao chamador
    |
    v
Controller NÃO aguarda essa Task
    |
    v
return Ok(...)
    |
    v
HTTP 200 pode chegar antes do fim do atraso

... tempo passa ...

Task.Delay conclui
    |
    v
MyAsyncMethod continua
    |
    v
var x = 7
```

### O conceito essencial

Um `await` **não faz todos os métodos anteriores na cadeia aguardarem automaticamente**.

Cada chamador precisa decidir como acompanhar a Task retornada.

---

## 10. Por que o endpoint responde antes de a operação terminar?

O professor repete o teste e configura diferentes breakpoints para mostrar o efeito.

### O que é aguardado no segundo teste?

O método auxiliar aguarda `Task.Delay`.

### O que não é aguardado?

O endpoint não aguarda `MyAsyncMethod`.

### Consequência

O controller pode finalizar a ação e enviar a resposta HTTP enquanto a operação iniciada continua pendente.

### Uma distinção importante

Isso **não significa** que `MyAsyncMethod` tenha sido executado inteiramente em uma thread paralela.

O método executa normalmente até alcançar um `await` sobre uma operação incompleta; naquele ponto, devolve o controle ao chamador e organiza a continuação para quando a Task terminar.

### O problema de não aguardar

Em aplicações reais, chamadas assíncronas iniciadas sem `await` podem levar a problemas:

- erros da tarefa podem não ser observados no ponto esperado;
- a resposta HTTP pode ser enviada antes de uma operação essencial;
- serviços `Scoped` podem ser descartados ao final da requisição enquanto a tarefa ainda os utiliza;
- o cliente pode receber sucesso antes de o trabalho necessário terminar.

Portanto, **ignorar uma Task não é uma estratégia segura de processamento em segundo plano** em uma API.

---

## 11. Terceiro teste: await também no endpoint

O professor volta ao controller e acrescenta `await` na chamada de `MyAsyncMethod`.

### Método auxiliar

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);
}
```

### Método da ação

```csharp
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    await MyAsyncMethod();

    return Ok(new { Id = Guid.NewGuid() });
}
```

Esse é um exemplo reconstruído de assinatura ASP.NET Core válida; a transcrição não informa o nome literal da ação nem a expressão exata usada para produzir o identificador da resposta.

### Por que a ação retorna Task<IActionResult>?

Porque o método da ação usa `await` e, ao final, produz um resultado HTTP do tipo `IActionResult`.

```csharp
public async Task<IActionResult> GetAsync()
```

O professor comenta que, ao inserir `await` no endpoint, precisa ajustar sua assinatura para um retorno assíncrono.

### Resultado demonstrado

Agora a resposta HTTP só é enviada **depois que `MyAsyncMethod` termina**.

O depurador mostra a conclusão do atraso dentro do método auxiliar e, depois, a execução da linha que retorna `Ok(...)`.

### Fluxo

```text
Controller
    |
    v
await MyAsyncMethod()
    |
    v
MyAsyncMethod
    |
    v
await Task.Delay(20_000)
    |
    v
Aguardar conclusão assíncrona do atraso
    |
    v
Retomar MyAsyncMethod
    |
    v
MyAsyncMethod conclui
    |
    v
Retomar Controller
    |
    v
return Ok(...)
    |
    v
HTTP 200
```

### Cuidado com a expressão "fica preso na linha"

O professor usa essa expressão para facilitar a visualização no debugger.

A interpretação correta é que **a continuação lógica do método fica suspensa** até a Task terminar.

Não significa que uma thread deva ficar bloqueada por 20 segundos.

---

## 12. O que realmente significa await?

`await` é uma palavra-chave que permite aguardar a conclusão de uma operação representada por uma Task ou outro objeto aguardável.

### Comportamento conceitual

```csharp
await MinhaOperacaoAsync();
ProximaInstrucao();
```

`ProximaInstrucao()` não é executada antes de `MinhaOperacaoAsync()` concluir com sucesso.

### E se a Task já estiver concluída?

O método pode prosseguir diretamente, sem a necessidade de suspender sua execução.

### E se a Task ainda estiver pendente?

A execução do método assíncrono pode ser suspensa.

Sua continuação será acionada quando a operação aguardada terminar, sujeita ao mecanismo de agendamento e ao contexto de execução aplicável.

### Await bloqueia uma thread?

Em operações verdadeiramente assíncronas de I/O ou temporização, **não é necessário bloquear a thread durante toda a espera**.

### E se houver falha?

Ao aguardar uma Task que falhou, a exceção é propagada no ponto do `await`, de acordo com as regras de tratamento de exceções do C#.

### E se a tarefa for cancelada?

O `await` pode propagar a condição de cancelamento por meio de exceção apropriada, como `OperationCanceledException`.

### Await não cria uma thread

A palavra-chave `await` organiza a continuação do fluxo; ela **não significa "execute este método em uma nova thread"**.

---

## 13. Iniciando uma Task, executando outro código e aguardando depois

Na etapa seguinte, o professor muda a forma de usar a Task.

Em vez de fazer `await` imediatamente, ele a guarda em uma variável, executa outras instruções e só então a aguarda.

### Exemplo reconstruído

```csharp
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    Task task = MyAsyncMethod();

    for (int i = 0; i < 100; i++)
    {
        // Exemplo de outro trabalho.
    }

    await task;

    return Ok(new { Id = Guid.NewGuid() });
}
```

### O que acontece?

1. `MyAsyncMethod()` é chamado.
2. O método inicia a espera assíncrona de 20 segundos.
3. Sua Task é devolvida ao controller.
4. O controller continua executando o `for`.
5. Ao chegar a `await task`, ele aguarda a conclusão, caso ainda esteja pendente.
6. Depois disso, retorna a resposta HTTP.

### Por que isso é possível?

Porque o controller não precisa suspender sua continuação **imediatamente** após iniciar a operação.

Ele pode realizar outro trabalho antes de precisar da conclusão da Task.

### Interpretação correta do exemplo

```text
Iniciar operação de espera
           |
           v
Task task = MyAsyncMethod()
           |
           +------ Operação assíncrona pendente
           |
           v
Executar outro código no controller
           |
           v
await task
           |
           +------ Se ainda incompleta: suspende continuação
           |
           v
Tarefa concluída
           |
           v
return Ok(...)
```

### Há necessariamente duas threads executando trabalho paralelo?

Não.

O `Task.Delay` é temporizado e não representa processamento de CPU simultâneo em outra thread.

O exemplo mostra **sobreposição de espera assíncrona com outras instruções**, não necessariamente paralelismo de processamento.

### O loop for da aula

O professor utiliza um laço de `i` até 100 como exemplo simples de trabalho realizado antes do `await`.

Esse laço costuma terminar muito antes do atraso de 20 segundos.

O objetivo é ilustrar a ordem das operações, não medir ganhos de desempenho.

---

## 14. Retornando resultados com Task<int>

O professor retoma a explicação de `Task<T>` e demonstra como obter um valor de retorno.

### Mudando o retorno do método

Em vez de:

```csharp
private async Task MyAsyncMethod()
```

passa a utilizar:

```csharp
private async Task<int> MyAsyncMethod()
```

### Exemplo

```csharp
private async Task<int> MyAsyncMethod()
{
    await Task.Delay(20_000);

    return 7;
}
```

### Como armazenar a tarefa?

```csharp
Task<int> task = MyAsyncMethod();
```

ou:

```csharp
var task = MyAsyncMethod();
```

A variável `task` representa uma operação que, ao concluir com sucesso, fornecerá um `int`.

### Como recuperar o valor?

```csharp
int resultado = await task;
```

O tipo do resultado aguardado é `int`.

### Exemplo com outro trabalho antes do resultado

```csharp
[HttpGet]
public async Task<IActionResult> GetAsync()
{
    Task<int> task = MyAsyncMethod();

    for (int i = 0; i < 100; i++)
    {
        // Outro trabalho de demonstração.
    }

    int resultado = await task;

    return Ok(new
    {
        Id = Guid.NewGuid(),
        Resultado = resultado
    });
}
```

O código acima é ilustrativo: a gravação confirma que o professor recuperou o número **7** no depurador, mas não estabelece esse formato literal de resposta HTTP.

### A diferença entre as duas variáveis

```csharp
Task<int> task = MyAsyncMethod();
```

`task` é uma **tarefa que produzirá um inteiro**.

```csharp
int resultado = await task;
```

`resultado` é o **inteiro produzido após a conclusão**.

### Cuidado com tipos

Não é possível tratar automaticamente `Task<int>` como se já fosse `int`.

```csharp
Task<int> task = MyAsyncMethod();

int resultado = await task; // Correto.
```

A palavra-chave `await` produz o resultado de `Task<T>`, se a operação terminar com sucesso.

---

## 15. Código reconstruído dos cenários

Esta seção reúne os experimentos em trechos independentes, para evitar misturar etapas que o professor modifica durante a aula.

### Cenário A — Task.Delay sem await

```csharp
private async Task MyAsyncMethod()
{
    Task.Delay(20_000); // Não aguardado.
}
```

**Comportamento:** o atraso é iniciado, mas sua Task é descartada. O método não espera a passagem dos 20 segundos.

**Avisos possíveis:** método `async` sem `await` e Task não aguardada.

### Cenário B — await apenas no método auxiliar

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);
}

[HttpGet]
public IActionResult Get()
{
    MyAsyncMethod(); // O controller não aguarda.

    return Ok(new { Id = Guid.NewGuid() });
}
```

**Comportamento:** a continuação do método auxiliar é aguardada internamente, mas o endpoint pode responder antes da conclusão do método auxiliar.

**Atenção:** esse formato não deve ser usado para trabalho essencial vinculado à requisição em uma API real.

### Cenário C — await na cadeia inteira

```csharp
private async Task MyAsyncMethod()
{
    await Task.Delay(20_000);
}

[HttpGet]
public async Task<IActionResult> GetAsync()
{
    await MyAsyncMethod();

    return Ok(new { Id = Guid.NewGuid() });
}
```

**Comportamento:** o endpoint aguarda logicamente a conclusão do método auxiliar antes de responder.

### Cenário D — iniciar, trabalhar e só depois aguardar

```csharp
private async Task<int> MyAsyncMethod()
{
    await Task.Delay(20_000);
    return 7;
}

[HttpGet]
public async Task<IActionResult> GetAsync()
{
    Task<int> task = MyAsyncMethod();

    for (int i = 0; i < 100; i++)
    {
        // Outro trabalho.
    }

    int resultado = await task;

    return Ok(new { Resultado = resultado });
}
```

**Comportamento:** a espera assíncrona pode ocorrer enquanto o controller executa outras instruções; a resposta só é enviada depois de obter o resultado da tarefa.

### O que esses exemplos representam?

| Cenário | Await interno? | Await no endpoint? | Endpoint espera a conclusão? |
|---|---:|---:|---|
| A | Não | Não | Não |
| B | Sim | Não | Não necessariamente |
| C | Sim | Sim | Sim |
| D | Sim | Sim, depois de outro código | Sim |

---

## 16. Estados de uma Task e inspeção no depurador

O professor mostra que o Visual Studio permite inspecionar informações da Task, incluindo seu status.

### Exemplos de estados

| Estado de Task | Significado geral |
|---|---|
| `WaitingForActivation` | Aguarda ativação/conclusão de uma operação associada |
| `Running` | Trabalho associado à tarefa está em execução |
| `RanToCompletion` | Concluiu com sucesso |
| `Faulted` | Concluiu com falha |
| `Canceled` | Foi cancelada |

Nem todos esses estados aparecem explicitamente no vídeo. Eles são um complemento para compreender a informação exibida pelo depurador.

### O que foi observado na gravação?

Ao chegar no `await` depois da operação, o professor verifica que a Task já havia executado e concluído com sucesso.

Em outro teste, ele aguarda até a tarefa terminar e então examina o valor de retorno inteiro, **7**.

### Por que Task possui um identificador?

O runtime pode associar um identificador à Task, útil especialmente durante diagnóstico e depuração.

Não se deve confundir o ID de uma Task com o identificador de uma thread, de uma requisição HTTP ou de uma entidade de banco de dados.

### Como observar o resultado?

`Task<int>` possui informação sobre seu resultado, mas a abordagem assíncrona recomendada é recuperar o valor com:

```csharp
int resultado = await task;
```

Evite utilizar `task.Result` para bloquear a execução quando `await` for possível.

---

## 17. Relação com APIs, banco de dados e I/O

O instrutor indica que usará os conceitos em operações reais do CashFlow nas próximas aulas.

### Por que banco de dados aparece como exemplo?

Porque consultas e comandos SQL precisam aguardar o servidor processar solicitações e devolver respostas.

A duração dessas operações depende de fatores externos, como:

- latência de rede;
- carga do banco;
- bloqueios e transações;
- tamanho dos dados;
- complexidade da consulta.

### O benefício esperado da programação assíncrona

Ao utilizar APIs assíncronas do EF Core, a aplicação pode aguardar o I/O sem manter uma thread bloqueada em todo o período de espera.

### O código atual do CashFlow

Depois da aula 103:

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

E, dentro da implementação de `UnitOfWork`:

```csharp
_dbContext.SaveChanges();
```

### Uma evolução futura possível

O EF Core possui `SaveChangesAsync`, mas **o professor não faz essa refatoração ainda**.

Uma futura implementação de persistência assíncrona exigiria rever de maneira consistente:

- os contratos envolvidos;
- as implementações concretas;
- os métodos que chamam essas operações;
- os tipos de retorno;
- o uso de `await` ao longo da cadeia.

**Não devemos apresentar essas mudanças como se tivessem sido realizadas na aula 104.**

### Melhora garantida de performance?

O professor antecipa que a mudança poderá trazer ganhos relevantes.

Tecnicamente, a principal vantagem típica do I/O assíncrono em servidores é a **escalabilidade e utilização eficiente de threads** sob concorrência, não necessariamente diminuir o tempo individual de cada operação.

O vídeo ainda não apresenta benchmark nem comparação quantitativa.

---

## 18. Equívocos frequentes e correções técnicas

Esta seção distingue algumas simplificações verbais da aula das regras reais do C#.

### 18.1 "Task sempre executa em segundo plano"

**Não necessariamente.**

Uma Task representa uma operação, que pode ser concluída imediatamente, representar I/O pendente ou envolver trabalho executado em uma thread.

Não é correto assumir que toda Task cria um processamento paralelo.

### 18.2 "Adicionar async coloca o método em outra thread"

**Não.**

A palavra-chave `async` permite escrever métodos com `await` e participa da transformação do código pelo compilador.

A execução começa normalmente quando o método é chamado, até chegar a um ponto de suspensão assíncrona.

### 18.3 "Toda função que retorna Task precisa de async"

**Não.**

```csharp
Task EsperarAsync()
{
    return Task.Delay(1000);
}
```

Esse método retorna uma Task válida sem usar `async`.

### 18.4 "await prende a thread até terminar"

**Não é a descrição correta.**

A continuação lógica do método não avança antes da conclusão da operação aguardada, mas a thread pode ser liberada enquanto a Task está pendente.

### 18.5 "Task.Delay ocupa uma thread dormindo"

**Não.**

`Task.Delay` representa um atraso não bloqueante, normalmente baseado em temporizador.

`Thread.Sleep`, por sua vez, bloqueia a thread durante o atraso.

### 18.6 "Se o método auxiliar tem await, o controller também aguarda automaticamente"

**Não.**

Se o controller chama um método que retorna Task e não aguarda nem retorna essa Task, ele pode continuar e enviar a resposta.

### 18.7 "Async e paralelo são a mesma coisa"

**Não.**

Assincronia permite suspender e retomar a continuação de uma operação. Paralelismo envolve execução simultânea efetiva de trabalho.

Uma operação de I/O assíncrona pode não consumir threads durante a espera.

### 18.8 "Usar var elimina o tipo Task"

**Não.**

`var` solicita inferência estática do tipo pelo compilador.

Se `MyAsyncMethod()` retorna `Task<int>`, então:

```csharp
var task = MyAsyncMethod();
```

continua produzindo uma variável do tipo `Task<int>`.

### 18.9 "await transforma a variável original em int"

**Não.**

```csharp
Task<int> task = MyAsyncMethod();
int result = await task;
```

A variável `task` continua sendo uma `Task<int>`; a variável `result` é um `int`.

### 18.10 "Task.Delay sem await aguarda 20 segundos"

**Não.**

Se a Task devolvida pelo método for ignorada, o código que a chamou não aguardará sua conclusão.

### 18.11 "Mais métodos async sempre tornam uma aplicação mais rápida"

**Não.**

A assincronia pode aumentar a capacidade de atender operações concorrentes com menos threads bloqueadas.

Em trabalho puramente de CPU, transformar um método em `async` sem alterar a operação não cria automaticamente paralelismo nem ganhos de performance.

### 18.12 "É seguro disparar uma Task e esquecer dela em uma API"

**Não como regra geral.**

Trabalho essencial deve ter sua conclusão e eventuais falhas acompanhadas.

Em cenários de tarefas realmente independentes da requisição HTTP, podem ser necessários serviços hospedados, filas ou outra arquitetura de processamento apropriada.

---

## 19. Boas práticas e cuidados com tarefas não aguardadas

### Preferir async ao longo da cadeia quando necessário

Se o endpoint precisa do resultado de uma operação, normalmente ele deve aguardar a Task correspondente.

```csharp
public async Task<IActionResult> GetAsync()
{
    await MyAsyncMethod();
    return Ok();
}
```

### Evitar async void em métodos comuns

Para operações que não retornam um valor, o retorno usual é `Task`, não `async void`.

`async void` é reservado a cenários específicos, como determinados manipuladores de eventos, pois não permite aguardar normalmente sua conclusão e complica o tratamento de exceções.

### Evitar bloquear com Wait ou Result quando await é possível

```csharp
// Evitar em um fluxo assíncrono:
task.Wait();
var valor = task.Result;
```

Essas operações podem bloquear threads e causar problemas dependendo do ambiente e do fluxo de execução.

### Cuidado com trabalho intensivo de CPU

Um `for` pequeno é usado no vídeo apenas para demonstrar a sequência de instruções.

Se o trabalho realizado antes do `await` for muito custoso, ele ainda poderá consumir CPU e ocupar a thread atual durante sua execução.

### Dar nomes claros a operações assíncronas

Uma convenção usual em C# é acrescentar o sufixo `Async` a métodos assíncronos:

```csharp
Task SalvarAsync();
Task<int> ObterQuantidadeAsync();
```

O instrutor utiliza `MyAsyncMethod` como nome demonstrativo.

### Aguardar antes de encerrar a requisição

Em uma operação HTTP cuja resposta depende da conclusão do trabalho, o controller precisa encadear e aguardar as operações correspondentes.

Essa é a lição prática central dos testes de breakpoint da gravação.

---

## 20. O que fica para a parte 2

O instrutor encerra afirmando que Tasks possuem outros conceitos relevantes e que continuará a explicação na aula seguinte.

### O que já foi demonstrado nesta parte 1?

- Definição introdutória de Task.
- Uso de `async` e `await`.
- `Task.Delay` como simulação de operação demorada.
- Diferença entre chamar e aguardar um método assíncrono.
- Comportamento do endpoint HTTP quando não aguarda.
- Comportamento do endpoint HTTP quando aguarda.
- Possibilidade de executar outras instruções antes do `await`.
- Retorno de resultados com `Task<int>`.
- Inspeção de tarefas pelo debugger.

### O que não pode ser antecipado como concluído?

- Refatoração assíncrona da persistência do CashFlow.
- Uso de `SaveChangesAsync` em `UnitOfWork`.
- Alteração da interface `IUnitOfWork`.
- Testes de carga ou comparações de desempenho.
- Novas estratégias para concorrência, cancelamento ou tratamento de exceções.
- Outros recursos de Tasks que serão explicados na parte 2.

A gravação se encerra após demonstrar o retorno inteiro e anunciar a continuação do assunto.

---

## 21. Perguntas de revisão

1. O que é uma `Task` em C#?
2. Qual é a diferença entre uma Task e uma thread?
3. Qual é a diferença entre `Task` e `Task<T>`?
4. Quando uma API pode se beneficiar de operações assíncronas?
5. Por que acesso a banco de dados aparece como exemplo de I/O?
6. O que significa declarar um método com `async`?
7. Um método precisa obrigatoriamente de `async` para retornar `Task`?
8. Qual é a diferença entre `MyAsyncMethod` e `MyAsyncMethod()` no contexto de delegates e invocação?
9. Qual é o tipo de `var task = MyAsyncMethod()` se o método retorna `Task`?
10. Como representar um método assíncrono que produz um inteiro?
11. Quantos milissegundos existem em 20 segundos?
12. O que faz `Task.Delay(20_000)`?
13. O que acontece se a Task devolvida por `Task.Delay` for ignorada?
14. Por que o controller retornou HTTP 200 antes do fim do atraso?
15. Qual é a diferença entre `await Task.Delay(...)` no método auxiliar e `await MyAsyncMethod()` no endpoint?
16. `await` precisa manter uma thread bloqueada?
17. Qual assinatura de retorno pode ser usada em uma ação ASP.NET Core assíncrona que devolve `IActionResult`?
18. Como iniciar uma Task, realizar outro trabalho e aguardá-la depois?
19. O loop `for` executado durante o atraso demonstra necessariamente paralelismo de CPU?
20. Como recuperar o inteiro produzido por uma `Task<int>`?
21. O que representa o status `RanToCompletion`?
22. Qual é a diferença entre a variável `Task<int> task` e a variável `int result`?
23. Por que ignorar uma Task pode ser perigoso em um endpoint HTTP?
24. Essa aula já modifica `UnitOfWork.Commit()` ou `SaveChanges()`?
25. Qual assunto o professor anuncia para a próxima aula?

---

## 22. Resumo e mapa mental

### Aprendizados principais

- `Task` representa uma operação e sua conclusão.
- `Task<T>` representa uma operação com resultado do tipo `T`.
- `async` habilita o uso de `await` no corpo de um método.
- Um método pode retornar `Task` sem `async`, desde que devolva uma Task adequadamente.
- `Task.Delay(20_000)` representa um atraso assíncrono de 20 segundos.
- Chamar `Task.Delay` sem aguardar sua Task **não bloqueia** a execução pela duração solicitada.
- `await` suspende a continuação lógica de um método quando a Task ainda não terminou.
- Se o método auxiliar utiliza `await`, mas o endpoint não aguarda o auxiliar, a resposta HTTP pode ser enviada antes da conclusão.
- Ao utilizar `await` também no endpoint, a resposta só é produzida após a conclusão da operação aguardada.
- É possível iniciar uma Task, executar outras instruções e aguardar sua conclusão em um ponto posterior.
- A forma `Task<int>` permite produzir um valor inteiro.
- O depurador confirma a obtenção do resultado **7**.
- Tarefas assíncronas **não implicam necessariamente** execução em outra thread.
- O foco desta parte 1 é entender o comportamento da linguagem.
- A refatoração do CashFlow para persistência assíncrona ficará para depois da continuação do conteúdo.

### Quadro comparativo

| Código | Significado |
|---|---|
| `Task.Delay(20_000);` | Cria uma Task de atraso e ignora seu retorno |
| `await Task.Delay(20_000);` | Aguarda logicamente o atraso dentro do método |
| `Task task = MyAsyncMethod();` | Inicia o método e obtém sua Task |
| `await task;` | Aguarda a conclusão da Task |
| `Task<int> task = MyAsyncMethod();` | Obtém uma tarefa que produzirá um inteiro |
| `int result = await task;` | Recupera o inteiro após a conclusão |
| `async Task<IActionResult>` | Assinatura típica de ação HTTP assíncrona com resultado |
| `return 7;` dentro de `async Task<int>` | Define o valor produzido pela operação |

### Mapa mental

```text
                              AULA 104
                        TASKS E ASYNC/AWAIT
                                  |
                 +----------------+----------------+
                 |                                 |
              Task                          Task<T>
                 |                                 |
          Sem valor de retorno              Com resultado T
                 |                                 |
                 +----------------+----------------+
                                  |
                               async
                                  |
                           Permite await
                                  |
                                  v
                           Task.Delay(20s)
                                  |
                 +----------------+----------------+
                 |                                 |
             Sem await                        Com await
                 |                                 |
        Task não é aguardada                Método auxiliar
                 |                           suspende continuação
                 |                                 |
                 v                                 v
          Código prossegue              Controller aguarda?
                                                   |
                                    +--------------+--------------+
                                    |                             |
                                   NÃO                           SIM
                                    |                             |
                         HTTP pode responder             HTTP responde depois
                         antes de concluir                 da conclusão
                                    |                             |
                                    +--------------+--------------+
                                                   |
                                                   v
                                       Outro experimento
                                                   |
                                  Task<int> task = MyAsyncMethod()
                                                   |
                                           Executa for(...)
                                                   |
                                           int result = await task
                                                   |
                                                   v
                                                result = 7
                                                   |
                                                   v
                                                PARTE 2
                                           Mais conceitos de Task
                                                   |
                                                   v
                                       Futura refatoração CashFlow
```

**Conclusão:** a primeira aula sobre Tasks mostra como representar operações assíncronas, distinguir a criação de uma Task de sua espera efetiva e coordenar a conclusão entre métodos chamadores. O experimento com `Task.Delay` evidencia que **aguardar dentro de um método não obriga automaticamente seu chamador a aguardar também**. Ao inserir `await` na cadeia apropriada, o endpoint passa a responder somente depois da conclusão da operação; e com `Task<int>`, pode receber um resultado, como o número **7**. A gravação prepara os fundamentos para as próximas aulas, mas ainda não modifica o fluxo de persistência do CashFlow.
