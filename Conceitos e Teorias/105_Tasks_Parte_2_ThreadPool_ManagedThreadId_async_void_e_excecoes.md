# Tasks em C# — Parte 2: ThreadPool, mudança de thread com await e riscos do async void

> **Aula 105 — Thread Pool, ManagedThreadId, execução assíncrona e tratamento de exceções**
>
> Continuação da aula 104, que apresentou `Task`, `Task<T>`, `async`, `await`, `Task.Delay` e o comportamento de um endpoint HTTP ao aguardar ou não uma operação assíncrona.
>
> **Escopo desta gravação:** o instrutor explica o conceito de *ThreadPool*, inspeciona os IDs das threads antes e depois de um `await` no depurador do Visual Studio e alerta sobre problemas de métodos declarados como `async void`. Ainda **não é feita a refatoração assíncrona da persistência do CashFlow**.
>
> **Critério de fidelidade:** a demonstração, os números de threads e os alertas são baseados na transcrição. As precisões sobre funcionamento interno do .NET, I/O, sincronização, tipos de retorno e propagação de exceções são complementos técnicos identificados separadamente: não devem ser confundidos com afirmações literais do instrutor.

## Índice

1. Objetivo da aula e ligação com a parte 1
2. Task é a mesma coisa que thread?
3. O que é o ThreadPool do .NET?
4. Por que reutilizar threads é mais eficiente?
5. Relação entre Task, ThreadPool e I/O assíncrono
6. Observando o ID da thread no Visual Studio
7. Teste: thread 9 antes do await e thread 16 depois
8. O que acontece realmente quando o código encontra await?
9. Por que a continuação pode mudar de thread?
10. Uma mudança de thread é garantida?
11. Thread de requisição, thread principal e contexto de sincronização
12. É possível configurar o ThreadPool?
13. O que é async void?
14. Por que não conseguimos fazer await em um retorno void?
15. async void e exceções que escapam do try/catch do chamador
16. Por que async Task é preferível?
17. Comparação: async void, async Task e async Task<T>
18. Exemplo seguro com try/catch e await
19. Boas práticas e os cuidados com fire-and-forget
20. Armadilhas de interpretação da explicação oral
21. Aplicação futura no CashFlow
22. Exercícios e perguntas de revisão
23. Resumo e mapa mental

---

## 1. Objetivo da aula e ligação com a parte 1

O professor abre a segunda aula sobre Tasks anunciando que responderá dúvidas comuns e fará um alerta importante a respeito de um tipo de método assíncrono que pode gerar problemas.

### Na aula 104 aprendemos

- `Task` representa uma operação e sua conclusão.
- `Task<T>` pode representar uma operação com resultado.
- `async` habilita o uso de `await` em métodos.
- `await Task.Delay(20_000)` representa uma espera assíncrona de 20 segundos.
- Se o endpoint não aguardar a Task devolvida pelo método auxiliar, ele pode enviar a resposta HTTP antes de a operação terminar.
- É possível começar uma operação, executar outras instruções e só depois utilizar `await`.

### As perguntas desta aula

1. Toda Task abre uma thread nova?
2. Como o .NET administra threads?
3. O método continua na mesma thread depois do `await`?
4. O que significa o ID de thread observado no debugger?
5. Por que `async void` é perigoso?
6. Como receber corretamente erros de uma operação assíncrona?

A aula continua utilizando um **endpoint de demonstração**, não o fluxo de persistência do CashFlow.

---

## 2. Task é a mesma coisa que thread?

A primeira dúvida formulada pelo professor é: se uma Task representa uma operação assíncrona, significa que o .NET criou uma nova thread?

### Resposta introdutória do instrutor

**Depende; na maioria dos casos, não.**

Esse ponto é fundamental para evitar uma confusão recorrente entre os conceitos.

### Task

Uma `Task` representa uma operação que pode estar pendente, em andamento ou concluída.

Ela também permite acompanhar falhas, cancelamento e, no caso de `Task<T>`, o resultado.

### Thread

Uma `Thread` é um recurso de execução que o runtime e o sistema operacional utilizam para executar instruções.

### Comparação

| Conceito | Natureza | Exemplo |
|---|---|---|
| `Task` | Abstração de operação/conclusão | `Task.Delay(20000)` |
| `Thread` | Recurso que executa código | Thread de worker do ThreadPool |
| `async` | Modificador usado para habilitar await e transformação assíncrona | `async Task ExecuteAsync()` |
| `await` | Operador que aguarda uma operação sem necessariamente bloquear uma thread | `await Task.Delay(20000)` |

### O que não devemos concluir

Não é correto dizer:

```text
Uma Task = Uma Thread
```

Nem:

```text
Cada await cria uma nova Thread
```

Uma mesma Task pode atravessar diferentes pontos de execução e uma thread pode executar continuidades de muitas Tasks ao longo do tempo.

---

## 3. O que é o ThreadPool do .NET?

O professor introduz o **ThreadPool**, traduzindo-o didaticamente como uma "piscina de threads".

### Ideia central

O runtime .NET mantém e gerencia um conjunto de threads trabalhadoras que podem atender diferentes tarefas ao longo da execução da aplicação.

Em vez de criar obrigatoriamente uma thread exclusiva para todo trabalho, ele pode aproveitar uma thread disponível.

### Analogia

Imagine uma equipe de funcionários preparados para executar serviços.

Quando chega um trabalho, o sistema pode designar um funcionário disponível. Ao terminar, ele volta a ficar disponível para outro trabalho.

```text
                         ThreadPool
          +----------------------------------+
          |                                  |
          | Thread A      Thread B           |
          |                                  |
          | Thread C      Thread D           |
          |                                  |
          +----------------------------------+
                |             |
                v             v
          Trabalho 1     Trabalho 2
                |             |
                v             v
          Threads voltam a ficar disponíveis
          para outros trabalhos quando o uso termina
```

### Por que essa estrutura existe?

Criar e encerrar threads de sistema operacional tem custos de memória, inicialização, agendamento e gerenciamento.

A reutilização permite reduzir custos em cenários com muitas operações pequenas ou frequentes.

### Precisão técnica

A fala do professor descreve um conjunto de threads já criado e que não será destruído.

O **ThreadPool real do .NET é dinâmico**: ele pode criar threads adicionais conforme a demanda e reduzir o conjunto de workers ociosos conforme suas políticas internas.

Portanto, não devemos memorizar que todas as threads são necessariamente pré-criadas em número fixo ou que nenhuma thread do pool pode ser encerrada.

A regra relevante é a **gestão e reutilização de threads pelo runtime**.

---

## 4. Por que reutilizar threads é mais eficiente?

O professor explica que criar uma thread é uma operação relativamente custosa.

### Criação individual repetida

```text
Trabalho A
    |
    +-- Criar thread
    +-- Executar trabalho
    +-- Encerrar thread

Trabalho B
    |
    +-- Criar thread
    +-- Executar trabalho
    +-- Encerrar thread

Trabalho C
    |
    +-- Criar thread
    +-- Executar trabalho
    +-- Encerrar thread
```

Essa abordagem implica custos de criação e encerramento repetidamente.

### Reutilização pelo ThreadPool

```text
Thread do pool
     |
     +-- Executa trabalho A
     |
     +-- Fica disponível
     |
     +-- Executa trabalho B
     |
     +-- Fica disponível
     |
     +-- Executa trabalho C
```

O professor usa esse contraste para justificar por que o .NET administra um pool em vez de criar uma thread nova a cada Task.

### Um detalhe de performance

O ThreadPool não torna todo código automaticamente mais rápido.

Ele reduz overhead de administração de threads e facilita a utilização de workers conforme a carga.

Uma Task que depende de I/O assíncrono pode nem precisar de uma thread dedicada **durante a espera**.

---

## 5. Relação entre Task, ThreadPool e I/O assíncrono

A aula apresenta o uso de threads do pool como o comportamento comum na execução de Tasks.

Essa explicação é útil para compreender **quem executa continuidades e trabalhos de CPU**, mas precisa de uma distinção quando tratamos de espera assíncrona.

### Exemplo de trabalho executado no ThreadPool

```csharp
Task tarefa = Task.Run(() =>
{
    // Trabalho síncrono de CPU executado
    // por um worker do ThreadPool.
    Console.WriteLine("Executando trabalho");
});

await tarefa;
```

Esse código é um **complemento didático**: o instrutor não mostra `Task.Run` nesta gravação.

### Exemplo de espera assíncrona

```csharp
await Task.Delay(20_000);
```

O atraso não precisa ocupar uma thread bloqueada durante os 20 segundos.

Uma thread executa o código até o ponto de espera. Se a Task ainda não terminou, a continuação do método fica pendente e uma thread poderá executá-la quando o atraso concluir.

### Comparação essencial

```text
Task.Run(trabalho CPU)
    |
    v
Thread do pool executa o trabalho


Task.Delay(20 segundos)
    |
    v
Temporizador fica pendente
    |
    v
Nenhuma thread precisa dormir
durante todo o atraso
    |
    v
Ao concluir, uma thread executa
a continuação quando agendada
```

### Observação sobre uma fala específica

O professor sugere que o runtime pode criar uma thread separada em operações especiais de I/O, como arquivos.

**Isso não deve ser tratado como regra geral do `async/await`.** APIs de I/O verdadeiramente assíncronas podem utilizar mecanismos do sistema operacional sem manter uma thread dedicada para cada operação pendente. Algumas implementações específicas podem usar workers para realizar trabalho, mas isso depende da API e da plataforma.

---

## 6. Observando o ID da thread no Visual Studio

Na segunda parte teórica, o professor volta ao exemplo da aula 104 e acrescenta inspeções do identificador da thread.

Ele quer observar a thread responsável pela execução em diferentes momentos.

### Posições citadas

1. Início da execução do endpoint.
2. Início de `MyAsyncMethod).
3. Antes da chamada `await Task.Delay`.
4. Depois da chamada `await Task.Delay`.
5. Continuação do endpoint, depois do outro código apresentado.

### Como obter o ID?

Uma forma válida de inspecionar o identificador gerenciado da thread atual é:

```csharp
int id = Thread.CurrentThread.ManagedThreadId;
```

Ou, em .NET moderno:

```csharp
int id = Environment.CurrentManagedThreadId;
```

### Código didático para logar a thread

```csharp
Console.WriteLine(
    $"Thread atual: {Environment.CurrentManagedThreadId}");
```

**Nota de fidelidade:** a gravação informa que o professor adicionou instruções para capturar o ID e inspecioná-lo, mas não transcreve literalmente a expressão exata utilizada. Os trechos acima representam formas equivalentes de fazer a observação.

### O que o ID significa?

É o identificador da thread gerenciada que está executando o trecho de código naquele instante.

**Não é o ID da Task**, nem o ID da requisição HTTP, nem uma chave de entidade no banco.

---

## 7. Teste: thread 9 antes do await e thread 16 depois

O professor inicia a API, abre o Swagger, executa o endpoint GET e acompanha os breakpoints.

### Valores observados na gravação

| Momento | ID observado |
|---|---:|
| Início do endpoint | 9 |
| Entrada em `MyAsyncMethod` | 9 |
| Antes da espera assíncrona | 9 |
| Continuação depois de `await Task.Delay(20_000)` | 16 |
| Continuação seguinte observada | 16 |

### O que esse resultado mostra?

Na execução específica da aula, o código começou utilizando a thread identificada como **9**, mas, após a operação assíncrona, foi retomado na thread **16**.

### Diagrama

```text
Entrada no endpoint
        |
        v
   Thread ID 9
        |
        v
MyAsyncMethod
        |
        v
   Thread ID 9
        |
        v
await Task.Delay(20_000)
        |
        v
Continuação fica pendente
        |
        v
Aguardar término do temporizador
        |
        v
Continuação é agendada
        |
        v
   Thread ID 16
        |
        v
Restante do método / endpoint
```

### Por que o professor fez esse teste?

Para demonstrar que a execução assíncrona **não precisa continuar sempre na mesma thread**.

### O que os números não significam?

Os IDs `9` e `16` são **observações daquela execução**.

Uma execução futura pode produzir IDs iguais ou diferentes; os números não são garantidos pelo runtime.

---

## 8. O que acontece realmente quando o código encontra await?

Considere:

```csharp
private async Task MyAsyncMethod()
{
    Console.WriteLine("Antes");

    await Task.Delay(20_000);

    Console.WriteLine("Depois");
}
```

### Etapa 1: execução inicial

Ao chamar `MyAsyncMethod()`, o método começa sua execução normalmente.

### Etapa 2: encontro com o await

Quando o `await` encontra uma operação ainda não concluída, a continuação do método é organizada para ocorrer posteriormente.

### Etapa 3: retorno do controle

O método devolve uma `Task` ao chamador, representando a conclusão futura de sua operação.

### Etapa 4: conclusão da operação aguardada

Quando `Task.Delay` termina, a continuação pode ser agendada.

### Etapa 5: retomada

O código depois do `await` é executado, potencialmente em outra thread.

```text
MyAsyncMethod é chamado
          |
          v
Executa instruções antes de await
          |
          v
Task.Delay ainda está pendente?
          |
      +---+----+
      |        |
     NÃO      SIM
      |        |
      v        v
 Continua   Suspende continuação
 direto         |
                v
          Aguarda conclusão
                |
                v
         Agenda continuação
                |
                v
           Continua método
```

### A thread foi bloqueada?

Para `Task.Delay`, não é necessário manter uma thread bloqueada durante todo o intervalo.

Essa é uma das principais vantagens da espera assíncrona.

---

## 9. Por que a continuação pode mudar de thread?

Na gravação, o instrutor explica que a thread inicial fica disponível para outras atividades enquanto a operação assíncrona não é concluída.

Quando a operação termina, uma thread disponível pode executar a continuação.

### Ideia fundamental

Não há exigência de que o mesmo worker execute todas as instruções de uma operação assíncrona.

```text
Thread 9
    |
    +-- Executa início do endpoint
    |
    +-- Executa início de MyAsyncMethod
    |
    +-- Encontra espera assíncrona
    |
    +-- Fica disponível para outros trabalhos


Temporizador de 20 segundos
    |
    +-- Conclui
    |
    v

Thread 16
    |
    +-- Executa continuação de MyAsyncMethod
    |
    +-- Executa continuação do endpoint
```

### O que fica pendente?

A **continuação lógica da operação**, não uma thread que precise necessariamente ficar parada na mesma linha.

### O professor chama isso de troca de thread

Essa descrição é útil visualmente, mas não significa que o runtime tenha simplesmente movido instruções de uma thread para outra no meio de uma chamada síncrona.

O ponto de suspensão permitido pelo `await` é o que organiza a retomada.

---

## 10. Uma mudança de thread é garantida?

**Não.**

A gravação apresenta uma mudança observada, mas o comportamento correto não exige que o ID da thread seja diferente após todo `await`.

### Possibilidades reais

- A Task já pode estar concluída, então a continuação ocorre imediatamente.
- A continuação pode ser executada na mesma thread por coincidência ou por regras do contexto.
- A continuação pode ser executada por outro worker.
- Um `SynchronizationContext` específico pode impor restrições de retomada.

### Por que isso importa?

Não se deve construir lógica de negócio baseada na suposição de que:

```text
Antes do await: sempre thread X
Depois do await: sempre thread Y
```

Também não é correto presumir que uma variável local muda de valor simplesmente porque o código foi retomado por outra thread.

A máquina de estados gerada pelo compilador mantém o estado necessário para que o método assíncrono continue corretamente.

### Resultado do experimento

A mudança de 9 para 16 demonstra uma **possibilidade permitida**, e não um contrato de execução obrigatório.

---

## 11. Thread de requisição, thread principal e contexto de sincronização

Na explicação oral, o professor se refere à thread inicial como "thread principal".

### Precisão de terminologia

Em uma API ASP.NET Core, normalmente a requisição é processada por **threads de worker**, não por uma única thread principal dedicada à requisição.

A thread com ID 9 era a thread responsável por executar o código naquele instante, mas não deve ser tratada necessariamente como a thread principal do processo.

### ASP.NET Core e SynchronizationContext

Em aplicações ASP.NET Core modernas, normalmente não existe um `SynchronizationContext` de requisição que obrigue a continuação a retornar à mesma thread original.

Por isso, continuidades podem ser executadas em threads diferentes do ThreadPool.

### E em aplicações de interface gráfica?

Ambientes como aplicações desktop podem possuir um contexto de sincronização vinculado à thread de UI, que influencia o local da retomada após `await`.

O cenário da gravação é **uma API ASP.NET Core**, portanto não se deve generalizar seu comportamento para todos os tipos de aplicação .NET.

### Conclusão

O código assíncrono deve depender de **operações concluídas, resultados e sincronização adequada**, e não de IDs específicos de thread.

---

## 12. É possível configurar o ThreadPool?

O professor comenta que o .NET permite controlar parâmetros associados à quantidade de threads do pool.

Ele não demonstra a configuração na aula.

### Complemento técnico

O runtime possui APIs como:

```csharp
ThreadPool.GetMinThreads(out int workers, out int io);
ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIo);
```

Também existem APIs para ajustar alguns desses limites.

### O que min e max representam?

São limites e parâmetros de gerenciamento do pool, não instruções para criar antecipadamente exatamente aquela quantidade de threads.

### É recomendável alterar esses valores por padrão?

Não.

O ThreadPool dispõe de mecanismos próprios de ajuste dinâmico.

Alterar parâmetros sem diagnóstico e medição de performance pode causar efeitos indesejados.

**Esta seção é apenas explicativa; nenhum ajuste de ThreadPool foi feito pelo instrutor.**

---

## 13. O que é async void?

Na parte final do vídeo, o professor apresenta um alerta.

Ele declara um método com esta estrutura:

```csharp
private async void Test()
{
    await Task.Delay(1000);
}
```

A sintaxe é aceita pelo C#.

### Por que o instrutor está preocupado?

Porque `void` não representa a conclusão da operação assíncrona.

O chamador não recebe uma `Task` que possa ser aguardada para acompanhar o trabalho.

### Comparação

**Método com async void:**

```csharp
private async void Test()
{
    await Task.Delay(1000);
}
```

**Método com async Task:**

```csharp
private async Task TestAsync()
{
    await Task.Delay(1000);
}
```

O segundo método permite:

```csharp
await TestAsync();
```

### Por que async void compila?

O C# permite métodos assíncronos com retorno `void` em determinados cenários.

O uso mais conhecido é a implementação de **event handlers** cujos delegates exigem retorno `void`.

Contudo, em métodos comuns de operações assíncronas, **`Task` é geralmente a escolha apropriada**.

---

## 14. Por que não conseguimos fazer await em um retorno void?

Considere este método:

```csharp
private async void Test()
{
    await Task.Delay(1000);
}
```

A chamada:

```csharp
Test();
```

é válida.

Mas isto não é:

```csharp
await Test(); // Inválido: o resultado da chamada é void.
```

### O motivo

O operador `await` trabalha com uma expressão cujo tipo é **aguardável**, como `Task` ou `Task<T>`.

Um método que retorna `void` não produz um objeto que o chamador possa aguardar.

### Ao mudar o retorno

```csharp
private async Task TestAsync()
{
    await Task.Delay(1000);
}
```

A chamada passa a funcionar:

```csharp
await TestAsync();
```

### O que isso melhora?

O chamador pode:

- aguardar a conclusão;
- saber quando a operação terminou;
- observar exceções propagadas pela Task;
- coordenar a sequência das operações.

### Diferença central

```text
async void
    |
    v
Nenhuma Task devolvida ao chamador
    |
    v
Não pode ser aguardado com await


async Task
    |
    v
Task devolvida ao chamador
    |
    v
Pode ser aguardado com await
```

---

## 15. async void e exceções que escapam do try/catch do chamador

O professor destaca que `async void` pode dificultar o tratamento de erros.

### Exemplo conceitual

```csharp
private async void Test()
{
    await Task.Delay(1000);

    throw new InvalidOperationException("Falha assíncrona");
}
```

### Tentativa de capturar o erro no chamador

```csharp
try
{
    Test();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

### Por que não funciona como esperado?

Quando `Test()` encontra o `await` pendente, o controle retorna ao chamador.

O `try/catch` que envolve a **chamada síncrona** pode terminar antes de a operação assíncrona lançar sua exceção.

O método `async void` não devolveu uma `Task` que permitisse propagar a falha pelo `await` do chamador.

### Qual é o risco?

A exceção pode ser encaminhada ao contexto de sincronização ou ao mecanismo de tratamento de exceções não observadas/sem tratamento do ambiente.

Dependendo da aplicação e do runtime, isso pode produzir falha grave do processo.

### Distinção técnica

A regra prática do curso é correta: **não conte com um `try/catch` externo à chamada para capturar uma exceção assíncrona de um método `async void`**.

Entretanto, é possível tratar exceções **dentro do próprio método `async void`**, com um `try/catch` que envolva o `await`. Isso não resolve a impossibilidade de o chamador aguardar sua conclusão.

### Por que isso não equivale a fire-and-forget seguro?

A ausência de Task impede a coordenação explícita do ciclo de vida e da falha da operação pelo chamador.

Em APIs, esse comportamento normalmente é inadequado para trabalho necessário à requisição.

---

## 16. Por que async Task é preferível?

O professor altera o método de `async void` para `async Task` e demonstra que agora pode utilizar `await` na chamada.

### Método corrigido

```csharp
private async Task TestAsync()
{
    await Task.Delay(1000);

    throw new InvalidOperationException("Falha assíncrona");
}
```

### Chamador correto

```csharp
try
{
    await TestAsync();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine(ex.Message);
}
```

### O que muda?

A exceção do método assíncrono passa a ser observada pelo chamador no ponto do `await`.

### Fluxo

```text
Chamador
   |
   v
await TestAsync()
   |
   v
TestAsync executa e aguarda outra Task
   |
   v
Operação termina com exceção
   |
   v
Task passa ao estado Faulted
   |
   v
await propaga a exceção
   |
   v
catch do chamador pode tratá-la
```

### E se a Task terminar com sucesso?

O `await` conclui e o chamador continua normalmente.

### E se a operação ainda estiver em andamento?

O chamador permanece logicamente aguardando sua conclusão, mas o `await` não precisa manter uma thread bloqueada.

---

## 17. Comparação: async void, async Task e async Task<T>

### Quadro principal

| Assinatura | Pode ser aguardada pelo chamador? | Resultado por await? | Uso típico |
|---|---|---|---|
| `async void` | Não | Não | Event handlers específicos |
| `async Task` | Sim | Sem valor de retorno | Operações assíncronas comuns |
| `async Task<T>` | Sim | Um valor do tipo `T` | Consultas ou operações com resultado |

### Exemplo: async void

```csharp
private async void Notificar()
{
    await Task.Delay(1000);
}
```

A chamada `Notificar()` não devolve uma Task.

### Exemplo: async Task

```csharp
private async Task NotificarAsync()
{
    await Task.Delay(1000);
}
```

A chamada pode ser aguardada com:

```csharp
await NotificarAsync();
```

### Exemplo: async Task<int>

```csharp
private async Task<int> ObterNumeroAsync()
{
    await Task.Delay(1000);
    return 7;
}
```

O chamador pode recuperar:

```csharp
int numero = await ObterNumeroAsync();
```

### É possível usar async int?

Não em métodos assíncronos comuns com o significado pretendido pelo professor.

```csharp
// Inválido:
private async int ObterNumeroAsync()
{
    await Task.Delay(1000);
    return 7;
}
```

O retorno adequado para esse exemplo é `Task<int>`.

### Complemento sobre retornos modernos

O professor lista `void`, `Task` e `Task<T>` como opções principais.

No C# moderno também existem tipos aguardáveis como `ValueTask` e `ValueTask<T>`, e formas específicas de métodos assíncronos com iteradores assíncronos.

Portanto, a lista apresentada na aula é **a lista introdutória relevante para os exemplos**, não uma enumeração exaustiva de todos os retornos possíveis em todos os contextos da linguagem.

---

## 18. Exemplo seguro com try/catch e await

Abaixo está um exemplo autônomo para estudar a diferença entre as abordagens.

### Implementação com Task

```csharp
public static async Task ExecutarOperacaoAsync()
{
    await Task.Delay(1000);

    throw new InvalidOperationException(
        "A operação simulada falhou.");
}
```

### Chamada com tratamento

```csharp
try
{
    await ExecutarOperacaoAsync();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Erro capturado: {ex.Message}");
}
```

### Resultado esperado

Após a espera assíncrona, a operação lança uma exceção.

A Task é concluída com falha, e o `await` do chamador propaga o erro ao bloco `catch`.

### Por que essa forma é adequada?

Porque o contrato da função comunica que existe uma operação assíncrona cuja conclusão e possíveis falhas precisam ser consideradas.

### O que não fazer sem justificativa?

```csharp
ExecutarOperacaoAsync(); // Task ignorada.
```

Mesmo com `async Task`, **não aguardar a Task devolvida** também pode fazer com que sua conclusão ou seus erros não sejam acompanhados como necessário.

Ou seja: o tipo `Task` torna o acompanhamento possível; ele não obriga automaticamente o chamador a utilizá-lo.

---

## 19. Boas práticas e os cuidados com fire-and-forget

### Preferir Task para métodos assíncronos comuns

Se uma função realiza uma operação assíncrona e seu chamador precisa conhecer sua conclusão, utilize um retorno aguardável.

### Utilizar await ao longo da cadeia quando necessário

Se uma operação do endpoint depende do sucesso de outra, o código deve acompanhar a Task correspondente.

```csharp
public async Task<IActionResult> GetAsync()
{
    await ProcessarAsync();
    return Ok();
}
```

Esse trecho é complementar e representa uma assinatura ASP.NET Core válida.

### Evitar async void fora de event handlers

Métodos `async void` não fornecem um objeto que o chamador possa aguardar.

Isso dificulta propagação de erros, testes e coordenação.

### Não confundir Task ignorada com execução em background gerenciada

A expressão *fire-and-forget* descreve iniciar algo sem aguardar sua conclusão, mas isso não garante um mecanismo confiável de processamento.

Em APIs, tarefas que precisam continuar além da requisição podem exigir:

- filas de trabalho;
- `BackgroundService` ou `IHostedService`;
- controle de cancelamento, falhas e descarte de dependências;
- outros mecanismos adequados de processamento assíncrono.

Esses mecanismos **não são ensinados nem implementados nesta gravação**.

### Observar concorrência com DbContext

O `DbContext` do EF Core não é seguro para operações simultâneas concorrentes na mesma instância.

A introdução de métodos assíncronos no CashFlow não deve levar à execução de múltiplas operações de banco em paralelo sobre o mesmo contexto sem uma estratégia apropriada.

### Não trocar await por Wait ou Result por conveniência

Em código que já utiliza assincronia, chamadas bloqueantes como `task.Wait()` ou `task.Result` podem desperdiçar threads e, em certos ambientes, causar deadlocks.

Quando possível, prefira `await`.

---

## 20. Armadilhas de interpretação da explicação oral

Esta seção reúne afirmações da gravação que funcionam como analogias, mas precisam de maior precisão técnica.

### 20.1 "O .NET pré-cria várias threads e nunca as destrói"

**Simplificação.** O ThreadPool gerencia dinamicamente suas threads e pode reduzir workers ociosos.

### 20.2 "Toda Task pega uma thread do pool"

**Não é universal.** Tasks podem representar I/O ou temporizadores sem worker dedicado durante a espera.

### 20.3 "Uma operação de I/O muito demorada faz o .NET criar uma thread separada"

**Não é uma regra geral.** I/O assíncrono pode utilizar mecanismos não bloqueantes do sistema operacional. A implementação depende da API e da plataforma.

### 20.4 "await joga o trabalho para outra thread"

**Incorreto como regra.** `await` suspende e agenda a continuação quando necessário; não implica necessariamente mudar de thread ou usar outra thread para executar o trabalho aguardado.

### 20.5 "A thread 9 era a thread principal da API"

Na demonstração, era a thread que estava executando aquele trecho. Uma requisição ASP.NET Core normalmente é atendida por workers e não possui uma thread principal exclusiva de requisição.

### 20.6 "Depois de await sempre teremos outra thread"

**Não.** O ID pode permanecer igual ou mudar, dependendo do contexto e do agendamento.

### 20.7 "async void não consegue lançar exceção"

**Consegue.** O problema é a **forma de propagação e observação pelo chamador**, não a impossibilidade de lançar um erro.

### 20.8 "Try/catch nunca captura exceções em async void"

Um `try/catch` **dentro do próprio método** pode capturar falhas das operações aguardadas. O alerta importante é que o `try/catch` externo ao método, envolvendo apenas sua chamada, não acompanha normalmente as falhas assíncronas após o ponto de suspensão.

### 20.9 "As únicas opções de retorno async são void, Task e Task<T>"

São as opções introdutórias explicadas nesta gravação; existem outros retornos aguardáveis e cenários específicos na linguagem moderna.

### 20.10 "Um resultado async é apenas um número ou objeto"

Além de devolver valores, Tasks permitem representar conclusão, falha e cancelamento da operação.

### 20.11 "Task sempre significa paralelismo"

Não. Assincronia, paralelismo e concorrência são conceitos relacionados, mas distintos.

### 20.12 "Alterar o ThreadPool sempre melhora a performance"

Não. Alterações sem análise de carga podem causar degradações. A configuração é uma possibilidade oferecida pelo runtime, não uma otimização automática.

---

## 21. Aplicação futura no CashFlow

O professor encerra a aula afirmando que concluiu a explicação introdutória de Tasks e seguirá para as próximas etapas do curso.

### Estado atual do projeto

Na aula 103, a persistência estava organizada desta forma:

```csharp
_repository.Add(entity);
_unitOfWork.Commit();
```

A implementação de `UnitOfWork.Commit` continuava síncrona:

```csharp
public void Commit()
{
    _dbContext.SaveChanges();
}
```

### Uma evolução possível

O EF Core possui operações assíncronas como `SaveChangesAsync`.

Uma refatoração futura poderá exigir alterações coordenadas nas interfaces e implementações do projeto.

Por exemplo, **apenas como ilustração de uma possível etapa posterior**:

```csharp
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
```

E:

```csharp
public Task CommitAsync(CancellationToken cancellationToken = default)
{
    return _dbContext.SaveChangesAsync(cancellationToken);
}
```

**Cuidado:** o segundo trecho não compilaria exatamente com a assinatura acima se o retorno de `SaveChangesAsync` (`Task<int>`) for devolvido por um método `Task`? Em C#, `Task<int>` deriva de `Task`, portanto essa atribuição/retorno é válida, ainda que não exponha a contagem de alterações. Outra implementação possível seria `await _dbContext.SaveChangesAsync(cancellationToken)` em um método `async Task`.

Esses exemplos são complementos técnicos e não representam código criado na aula 105.

### Qual é o vínculo com o que estudamos?

Os aprendizados de `async`, `await`, gerenciamento de threads e tratamento de exceções são importantes para que as próximas mudanças de persistência sejam feitas sem confundir espera assíncrona com paralelismo ou com operações que podem ser ignoradas.

---

## 22. Exercícios e perguntas de revisão

### Exercício 1 — identificar a thread

```csharp
Console.WriteLine(Environment.CurrentManagedThreadId);
await Task.Delay(500);
Console.WriteLine(Environment.CurrentManagedThreadId);
```

**Pergunta:** os dois IDs precisam ser diferentes?

**Resposta:** não. Eles podem ser iguais ou diferentes, dependendo da execução.

### Exercício 2 — compreender Task.Delay

```csharp
await Task.Delay(1000);
```

**Pergunta:** uma thread precisa ficar bloqueada durante o segundo de espera?

**Resposta:** não. A continuação pode ficar suspensa enquanto o atraso está pendente.

### Exercício 3 — identificar o problema

```csharp
private async void Processar()
{
    await Task.Delay(1000);
}
```

**Pergunta:** é possível escrever `await Processar()`?

**Resposta:** não, porque a chamada devolve `void`, não uma Task aguardável.

### Exercício 4 — corrigir o retorno

```csharp
private async Task ProcessarAsync()
{
    await Task.Delay(1000);
}
```

**Pergunta:** qual é a vantagem?

**Resposta:** o chamador pode aguardar a conclusão da operação e observar possíveis exceções pela Task.

### Exercício 5 — tratar a exceção

```csharp
try
{
    await ProcessarAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

**Pergunta:** por que isso funciona melhor do que um `try/catch` externo que apenas chama um método `async void`?

**Resposta:** porque `await` propaga a falha da Task no fluxo do chamador.

### Perguntas adicionais

1. O que é o ThreadPool?
2. Por que a criação de threads tem custo?
3. A quantidade de threads do ThreadPool é fixa?
4. Toda Task necessita de uma thread dedicada?
5. Qual a diferença entre `Task.Delay` e `Thread.Sleep`?
6. Como obter o identificador da thread atual no .NET?
7. Que IDs o professor observou antes e depois do `await`?
8. Por que esses valores não são garantidos em toda execução?
9. Um `await` pode retomar na mesma thread?
10. O que o `await` faz quando a Task já está concluída?
11. Qual diferença existe entre thread, Task e operação de I/O?
12. Por que o uso de "thread principal" pode ser impreciso em ASP.NET Core?
13. Como o `async void` difere de `async Task`?
14. Por que não se pode usar `await` diretamente sobre uma chamada `void`?
15. Em qual situação `async void` é comumente permitido?
16. Por que exceções de `async void` são difíceis de tratar externamente?
17. `try/catch` dentro de `async void` consegue tratar exceções?
18. Qual é a diferença entre o retorno de `async Task` e `async Task<int>`?
19. Métodos assíncronos modernos aceitam outros tipos aguardáveis além de `Task`?
20. Quais riscos surgem ao ignorar uma Task?
21. Quais partes da persistência do CashFlow ainda estão síncronas no fim desta aula?
22. O vídeo já realizou a refatoração do projeto para `SaveChangesAsync`?

---

## 23. Resumo e mapa mental

### Principais aprendizados

- O professor continua o estudo de Tasks iniciado na aula 104.
- Uma Task não implica necessariamente criar uma nova thread.
- O .NET utiliza o **ThreadPool** para gerenciar e reutilizar threads trabalhadoras.
- Criar e destruir threads possui custo, o que motiva a reutilização.
- O conjunto do ThreadPool é gerenciado dinamicamente pelo runtime.
- Uma operação verdadeiramente assíncrona de I/O pode não ocupar uma thread durante toda a espera.
- O professor adiciona instruções de inspeção de `ManagedThreadId` no endpoint e em `MyAsyncMethod`.
- No teste, a execução começa com **ID 9** e continua após o atraso com **ID 16**.
- Essa diferença demonstra que uma continuação após `await` **pode mudar de thread**.
- O `await` não garante a criação de outra thread nem uma mudança de ID.
- A gravação apresenta a sintaxe `async void` como um alerta.
- Métodos `async void` podem conter `await`, mas o chamador não recebe uma Task.
- Por isso, uma chamada a `async void` não pode ser aguardada com `await`.
- O `try/catch` externo ao método não acompanha normalmente falhas assíncronas de `async void`.
- O retorno `async Task` permite aguardar a conclusão e propagar as exceções no ponto do `await`.
- Para métodos comuns, o professor recomenda preferir **`async Task`**.
- A aula ainda não altera os métodos síncronos de persistência do CashFlow.

### Quadro final

| Conceito | Regra importante |
|---|---|
| ThreadPool | Gerencia e reutiliza workers, ajustando a quantidade conforme a demanda |
| Task | Representa operação/conclusão, não necessariamente thread |
| ManagedThreadId | Identifica a thread gerenciada atual |
| `await` | Aguarda logicamente a conclusão e pode suspender o método |
| `Task.Delay` | Representa espera assíncrona, sem thread bloqueada durante todo o intervalo |
| Thread após await | Pode ser a mesma ou outra |
| `async void` | Não fornece Task aguardável ao chamador |
| `async Task` | Permite acompanhar conclusão e observar exceções |
| `async Task<T>` | Permite obter um resultado do tipo `T` após `await` |
| `try/catch` com `await` | Pode tratar falhas da Task no chamador |
| CashFlow | Persistência continua síncrona ao término da aula |

### Mapa mental

```text
                               AULA 105
                         TASKS — PARTE 2
                                  |
                 +----------------+----------------+
                 |                                 |
               THREADS                       ASYNC VOID
                 |                                 |
             ThreadPool                       Pode conter await
                 |                                 |
       Reutilização de workers              Mas retorna void
                 |                                 |
       Task != nova thread                   Chamador não pode
                 |                           aguardar diretamente
                 |                                 |
                 v                                 v
      Inspeção ManagedThreadId               Exceções difíceis
                 |                          de observar externamente
          +------+-------+                         |
          |              |                         v
       Antes           Depois                  Preferir
       await           await                 async Task
          |              |                         |
          v              v                         v
        ID 9            ID 16                await TestAsync()
          |              |                         |
          +------+-------+                         v
                 |                          try/catch do chamador
                 v                          pode receber a falha
         Continuação pode
         mudar de thread
                 |
                 v
       Não é garantia nem
       criação obrigatória
       de outra thread
                 |
                 v
          Próximas aulas
      Assincronia na aplicação
```

**Conclusão:** esta aula aprofunda a relação entre Tasks e threads, apresenta o ThreadPool como mecanismo de gerenciamento de workers e mostra uma execução concreta em que o fluxo é retomado na thread **16** após começar na **9**. Na parte final, o professor alerta que **`async void` não deve ser a escolha comum para operações assíncronas**, pois o chamador não recebe uma Task para aguardar a conclusão ou observar normalmente as exceções. A recomendação principal é utilizar `async Task` ou `async Task<T>` conforme a necessidade, preparando o terreno para refatorações assíncronas posteriores no CashFlow.
