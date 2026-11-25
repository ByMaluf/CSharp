# Aula 114 — SOLID: Open/Closed Principle (OCP), polimorfismo e refatoração do filtro de exceções

> **Continuação da aula 112:** o CashFlow já trata erros de validação (HTTP 400) e recursos inexistentes (HTTP 404), mas o filtro contém condições específicas para cada tipo de exceção.
>
> **Objetivo:** tornar esse tratamento extensível, transferindo para cada exceção a definição de seu código de status e a forma de fornecer suas mensagens de erro.
>
> **Resultado demonstrado:** o professor consulta a despesa de ID 1000 e confirma HTTP 404 com uma mensagem. Depois tenta cadastrar uma despesa sem título e com valor zero, confirmando HTTP 400 com duas mensagens de validação.
>
> **Fidelidade:** os exemplos reproduzem a estrutura descrita na gravação. Nomes locais, assinaturas completas de métodos auxiliares e detalhes não transcritos integralmente são reconstruções didáticas. Alternativas e precisões técnicas são explicitamente identificadas como complementos.

## Índice

1. O objetivo da refatoração
2. O princípio Open/Closed
3. OCP não proíbe corrigir código
4. O problema no filtro de exceções
5. A solução por abstração e polimorfismo
6. Adicionando StatusCode à classe base
7. Implementando StatusCode nas exceções concretas
8. HttpStatusCode versus StatusCodes
9. O problema das diferentes representações de mensagens
10. Adicionando GetErrors() à abstração
11. Refatorando ErrorOnValidationException
12. Implementando GetErrors() em NotFoundException
13. Refatorando o filtro para utilizar a classe base
14. ObjectResult e status HTTP dinâmico
15. Operador as, cast explícito e operador !
16. Código consolidado por componente
17. Como o polimorfismo viabiliza o OCP
18. Testes no Swagger
19. Limites, boas práticas e complementos técnicos
20. Exercícios e perguntas de revisão
21. Resumo e mapa mental

---

## 1. O objetivo da refatoração

Na aula 112, o CashFlow ganhou um tratamento específico para o caso em que uma despesa não é encontrada.

O filtro de exceções ficou capaz de distinguir:

- ErrorOnValidationException: falhas de validação, HTTP 400 Bad Request.
- NotFoundException: recurso não encontrado, HTTP 404 Not Found.

A aula 113 tratou de outro princípio SOLID, o ISP, separando interfaces de leitura e escrita do repositório.

Agora, o professor retorna ao filtro de exceções para corrigir uma dificuldade de manutenção: cada novo tipo de exceção exigiria acrescentar mais uma condição ao filtro.

O objetivo não é alterar as regras de negócio nem os resultados dos endpoints. A intenção é melhorar a estrutura do código para suportar extensões futuras.

---

## 2. O princípio Open/Closed

A letra O de SOLID representa **Open/Closed Principle (OCP)**, ou **Princípio Aberto/Fechado**.

A ideia apresentada pelo professor é que uma classe, módulo ou função deve estar:

- Aberta para extensão.
- Fechada para modificação.

### Aberta para extensão

Deve ser possível acrescentar comportamentos previstos pelo desenho do sistema sem reescrever continuamente o código que já os consome.

### Fechada para modificação

Uma abstração e seu mecanismo consumidor devem permanecer relativamente estáveis diante dessas extensões.

### Aplicação ao CashFlow

O filtro deve saber transformar uma exceção da aplicação em resposta HTTP, sem conhecer antecipadamente cada classe concreta de exceção.

A criação de uma nova exceção deve ocorrer, idealmente, pela extensão da hierarquia CashFlowException e implementação de seu contrato, sem adicionar mais um ramo de tratamento específico ao filtro.

---

## 3. OCP não proíbe corrigir código

O professor enfatiza que o princípio não significa que uma classe nunca possa ser modificada.

Se houver um bug na validação ou no salvamento de dados, é correto modificar o código para corrigi-lo.

O problema que motiva o OCP nesta aula é outro: **precisar modificar repetidamente o mesmo módulo toda vez que um novo comportamento da mesma categoria é acrescentado**.

| Situação | Interpretação |
|---|---|
| Corrigir um bug em código existente | Alteração legítima; OCP não proíbe |
| Refatorar uma abstração inadequada | Pode ser necessário |
| Acrescentar uma nova exceção e obrigatoriamente editar o filtro | Acoplamento recorrente que motiva a refatoração |
| Acrescentar uma nova exceção que implementa um contrato já conhecido pelo filtro | Extensão dentro do desenho proposto |

O próprio professor modifica a classe base e o filtro durante a aula. Isso é precisamente a refatoração necessária para preparar o código para extensões futuras.

---

## 4. O problema no filtro de exceções

O filtro existente possui uma verificação inicial que distingue exceções conhecidas do CashFlow de erros inesperados.

Conceitualmente:

```csharp
if (context.Exception is CashFlowException)
{
    // Tratamento de erros conhecidos da aplicação.
}
else
{
    // Tratamento de erros inesperados.
}
```

O professor considera essa distinção adequada e a mantém.

O problema está na função responsável por tratar os erros conhecidos, que precisa verificar individualmente cada tipo concreto.

### Antes da refatoração

```csharp
if (context.Exception is ErrorOnValidationException validationException)
{
    var errorResponse = new ResponseErrorJson(
        validationException.Errors);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status400BadRequest;

    context.Result = new BadRequestObjectResult(errorResponse);
}
else if (context.Exception is NotFoundException notFoundException)
{
    var errorResponse = new ResponseErrorJson(
        notFoundException.Message);

    context.HttpContext.Response.StatusCode =
        StatusCodes.Status404NotFound;

    context.Result = new NotFoundObjectResult(errorResponse);
}
```

Esse trecho representa a lógica desenvolvida na aula 112. Os nomes locais foram reconstruídos.

### Por que tende a crescer?

A cada novo tipo de exceção com outro status ou formato de mensagem, seria necessário editar a função e acrescentar mais um else if.

O professor menciona exemplos de situações que encontrou em outros projetos:

- Credenciais inválidas, como e-mail ou senha incorretos.
- Ausência de permissão para acessar um recurso.
- Limitação de uso de determinado endpoint a uma vez por dia.

Esses exemplos demonstram que a quantidade de exceções pode aumentar. A gravação não implementa essas novas funcionalidades nem define seus respectivos códigos HTTP.

### Consequências possíveis

- Mais dependência de classes concretas.
- Crescimento do código condicional.
- Repetição de lógica.
- Maior necessidade de editar um componente central.
- Risco de esquecer o novo ramo de tratamento.

O problema não é a existência de qualquer if, mas a necessidade recorrente de ampliar a seleção por tipos concretos.

---

## 5. A solução por abstração e polimorfismo

O professor percebe que todas as exceções conhecidas precisam fornecer duas informações ao filtro:

1. Um código de status HTTP.
2. Uma lista de mensagens de erro.

A ideia é colocar esses requisitos no contrato abstrato comum, CashFlowException.

```text
CashFlowException
    |
    +-- StatusCode
    |
    +-- GetErrors()
           |
           +-- Toda exceção concreta fornece suas respostas
```

O filtro não precisa mais perguntar se a exceção é ErrorOnValidationException ou NotFoundException.

Ele só precisa perguntar:

- Qual é o StatusCode desta CashFlowException?
- Quais são seus erros?

A classe concreta é responsável por responder a essas operações de acordo com seu próprio comportamento.

---

## 6. Adicionando StatusCode à classe base

No projeto de exceções, dentro da pasta ExceptionBase, o professor abre CashFlowException.

Ele acrescenta uma propriedade abstrata:

```csharp
public abstract int StatusCode { get; }
```

### O significado de cada parte

| Elemento | Significado |
|---|---|
| public | O consumidor pode acessar a propriedade |
| abstract | A classe base não fornece uma implementação concreta |
| int | O código de status é representado por um inteiro |
| StatusCode | Nome da propriedade |
| get | O contrato permite a leitura, sem setter |

### Por que abstract?

O professor quer obrigar todas as exceções concretas derivadas de CashFlowException a fornecer esse valor.

Quando a propriedade é adicionada, as classes concretas existentes deixam de satisfazer o contrato da classe base e o compilador sinaliza a necessidade de implementação.

### Por que somente get?

O consumidor precisa ler o status associado à exceção, não alterá-lo.

A implementação concreta pode definir o valor adequado à sua categoria de erro.

**Precisão técnica:** uma propriedade somente com getter não implica que o valor seja necessariamente uma constante. O getter poderia calcular ou obter o valor de outro estado. Nesta aula, os overrides devolvem códigos fixos.

---

## 7. Implementando StatusCode nas exceções concretas

O Visual Studio oferece a opção de implementar os membros abstratos.

O professor utiliza override para completar o contrato em cada classe.

### ErrorOnValidationException

```csharp
public override int StatusCode =>
    (int)HttpStatusCode.BadRequest;
```

O membro BadRequest representa HTTP 400.

### NotFoundException

```csharp
public override int StatusCode =>
    (int)HttpStatusCode.NotFound;
```

O membro NotFound representa HTTP 404.

### Qual o papel de override?

A palavra-chave indica que a classe derivada fornece a implementação de um membro virtual ou abstrato declarado na classe base.

Aqui, cada exceção escolhe o status que corresponde à sua semântica.

| Exceção concreta | StatusCode |
|---|---:|
| ErrorOnValidationException | 400 |
| NotFoundException | 404 |

As expressões com => são uma forma concisa de escrever getters que devolvem uma expressão. A forma equivalente com um bloco get também seria válida.

---

## 8. HttpStatusCode versus StatusCodes

O professor identifica que a propriedade StatusCode do objeto HTTP Response é do tipo int.

Entretanto, HttpStatusCode é um enum.

### HttpStatusCode

```csharp
using System.Net;

HttpStatusCode.BadRequest
HttpStatusCode.NotFound
```

Os membros representam códigos HTTP, mas o tipo da expressão é HttpStatusCode.

### Conversão explícita

```csharp
(int)HttpStatusCode.BadRequest
(int)HttpStatusCode.NotFound
```

O cast produz os valores inteiros 400 e 404, respectivamente.

### StatusCodes

No projeto API, o professor já utilizava constantes do ASP.NET Core, como:

```csharp
StatusCodes.Status400BadRequest
StatusCodes.Status404NotFound
```

Essas constantes já possuem tipo int.

### Por que usar HttpStatusCode no projeto de exceções?

O projeto de exceções é uma class library que, na estrutura mostrada, não possui a referência necessária aos tipos de ASP.NET Core utilizados na API.

O professor escolhe System.Net.HttpStatusCode, evitando acrescentar uma dependência do framework web somente para obter os códigos.

### Comparação técnica

| Elemento | Namespace | Tipo da expressão |
|---|---|---|
| HttpStatusCode.NotFound | System.Net | HttpStatusCode (enum) |
| (int)HttpStatusCode.NotFound | System.Net | int |
| StatusCodes.Status404NotFound | Microsoft.AspNetCore.Http | int |

**Complemento:** uma class library pode acessar StatusCodes se possuir referências adequadas. Não existe uma proibição da linguagem de utilizar esse tipo fora de um projeto API. A limitação descrita pelo professor decorre das dependências atuais da solução.

---

## 9. O problema das diferentes representações de mensagens

Resolver StatusCode não basta. O filtro também precisa construir ResponseErrorJson.

Na arquitetura anterior, as mensagens estão disponíveis de duas formas:

### ErrorOnValidationException

Possui uma lista de mensagens em Errors.

### NotFoundException

Possui uma mensagem única, obtida de Exception.Message.

A hierarquia de construtores implementada na aula 112 encaminha a mensagem até a classe Exception:

```text
NotFoundException(message)
        |
        v
CashFlowException(message)
        |
        v
SystemException(message)
        |
        v
Exception(message)
        |
        v
Message
```

### Por que essa diferença importa?

Se o filtro continuar dependendo de Errors para um tipo e Message para outro, precisará preservar verificações específicas ou criar novas condições.

Isso impediria que a refatoração atingisse seu objetivo.

O professor decide padronizar o acesso às mensagens: **todas as exceções conhecidas devem fornecer uma lista de strings**.

---

## 10. Adicionando GetErrors() à abstração

O professor retorna a CashFlowException e acrescenta um método abstrato.

```csharp
public abstract List<string> GetErrors();
```

A classe base passa a exigir dois membros:

```csharp
public abstract int StatusCode { get; }

public abstract List<string> GetErrors();
```

### Por que um método único?

Porque o filtro precisa de um contrato uniforme, independentemente da quantidade original de mensagens.

Uma exceção pode representar vários erros de validação. Outra pode representar apenas um erro específico.

Em ambos os casos, GetErrors devolve uma coleção de strings.

### A lista de um elemento é válida

Uma lista não precisa conter vários valores. Ela pode ter apenas um elemento.

Isso permite adaptar Message, que é uma string, ao formato de resposta já adotado pela API.

---

## 11. Refatorando ErrorOnValidationException

Essa exceção já recebia uma lista de erros no construtor.

O professor implementa GetErrors para devolver essa lista.

Inicialmente, o retorno pode ser compreendido como:

```csharp
public override List<string> GetErrors()
{
    return Errors;
}
```

Em seguida, o professor melhora o encapsulamento.

Ele substitui a propriedade pública Errors por um campo privado readonly, denominado _errors.

### Estrutura final reconstruída

```csharp
public class ErrorOnValidationException : CashFlowException
{
    private readonly List<string> _errors;

    public ErrorOnValidationException(List<string> errors)
        : base(string.Empty)
    {
        _errors = errors;
    }

    public override int StatusCode =>
        (int)HttpStatusCode.BadRequest;

    public override List<string> GetErrors() => _errors;
}
```

### Por que private?

O professor não deseja que os consumidores acessem diretamente a propriedade Errors.

A forma padronizada de obter as mensagens passa a ser GetErrors().

### Por que readonly?

O campo pode ser atribuído durante a inicialização ou no construtor, mas não pode receber outra referência livremente depois disso.

### Atenção: readonly não torna List imutável

Esse é um complemento técnico importante.

O campo readonly impede a reatribuição da referência _errors após a construção, mas a lista apontada por essa referência continua mutável.

Como GetErrors devolve a própria lista, um consumidor ainda poderia modificá-la:

```csharp
var errors = validationException.GetErrors();
errors.Add("Outra mensagem");
```

O exemplo não é realizado na gravação. Ele demonstra que encapsulamento do campo e imutabilidade da coleção são conceitos diferentes.

Se fosse necessário evitar mutações externas, poderia ser adotado um contrato somente leitura ou uma cópia defensiva. Isso seria uma evolução adicional, não a implementação da aula.

---

## 12. Implementando GetErrors() em NotFoundException

NotFoundException não possui uma lista original de mensagens.

A mensagem já está disponível na propriedade herdada Message.

O professor implementa o método para produzir uma lista contendo esse único valor.

### Forma tradicional

```csharp
public override List<string> GetErrors()
{
    return new List<string> { Message };
}
```

### Forma simplificada

O Visual Studio sugere simplificar a inicialização da coleção. Uma representação coerente com a sintaxe moderna discutida é:

```csharp
public override List<string> GetErrors() => [Message];
```

A transcrição confirma a simplificação da coleção, mas não reproduz os caracteres exatos da sugestão exibida.

A sintaxe de expressão de coleção com colchetes foi introduzida no C# 12 e depende da versão da linguagem configurada no projeto.

### Classe consolidada

```csharp
public class NotFoundException : CashFlowException
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public override int StatusCode =>
        (int)HttpStatusCode.NotFound;

    public override List<string> GetErrors() => [Message];
}
```

### Resultado conceitual

Uma mensagem como "Despesa não encontrada" é devolvida em uma lista de um elemento.

O filtro pode utilizar o mesmo construtor de ResponseErrorJson que recebe listas, sem distinguir tipos concretos.

---

## 13. Refatorando o filtro para utilizar a classe base

Com StatusCode e GetErrors disponíveis, o professor retorna ao filtro.

O código condicional específico para ErrorOnValidationException e NotFoundException deixa de ser necessário.

### Antes

```text
Se ErrorOnValidationException:
    obter Errors
    definir 400
    criar BadRequestObjectResult

Senão, se NotFoundException:
    obter Message
    definir 404
    criar NotFoundObjectResult
```

### Depois

```text
Obter CashFlowException
    |
    +-- GetErrors()
    |
    +-- StatusCode
    |
    +-- ObjectResult com ResponseErrorJson
```

### Trecho reconstruído do tratamento de exceções conhecidas

```csharp
var cashFlowException = (CashFlowException)context.Exception;

var errorResponse = new ResponseErrorJson(
    cashFlowException.GetErrors());

context.HttpContext.Response.StatusCode =
    cashFlowException.StatusCode;

context.Result = new ObjectResult(errorResponse);
```

O professor também demonstra a alternativa com as e operador de supressão de nulabilidade, detalhada na seção 15. A transcrição não estabelece inequivocamente qual forma ficou no arquivo final; o cast explícito acima é uma das formas apresentadas.

### Por que é possível converter para CashFlowException?

O filtro já verifica, antes de chamar esse tratamento, que a exceção pertence à hierarquia CashFlowException.

Essa condição é a premissa que torna o cast válido no fluxo apresentado.

### O tratamento de erros desconhecidos permanece?

Sim. O professor considera adequada a distinção inicial entre exceções do CashFlow e exceções inesperadas.

A refatoração substitui a sequência de verificações por tipos concretos dentro do tratamento das exceções conhecidas. O fallback preexistente deve ser preservado.

---

## 14. ObjectResult e status HTTP dinâmico

Na implementação anterior, havia resultados MVC específicos:

```csharp
new BadRequestObjectResult(errorResponse)
new NotFoundObjectResult(errorResponse)
```

Cada classe representa um status HTTP determinado.

Agora o código de status é informado pela exceção.

```csharp
context.HttpContext.Response.StatusCode =
    cashFlowException.StatusCode;
```

O professor utiliza um resultado genérico:

```csharp
context.Result = new ObjectResult(errorResponse);
```

### Por que remover BadRequestObjectResult?

Se o filtro mantivesse um resultado fixo de erro 400, poderia contrariar o status definido por NotFoundException ou por outra exceção futura.

ObjectResult permite representar um corpo de resposta sem exigir uma classe específica de resultado para cada categoria de erro.

### Complemento técnico: StatusCode no próprio ObjectResult

Outra forma de expressar a mesma intenção no ASP.NET Core seria:

```csharp
context.Result = new ObjectResult(errorResponse)
{
    StatusCode = cashFlowException.StatusCode
};
```

Isso associa o código diretamente ao objeto de resultado que será executado pelo MVC.

Essa alternativa não é a sequência de linhas narrada pelo professor, que mantém a atribuição ao HttpContext.Response.StatusCode. Ela é apresentada apenas como uma opção de organização.

### O que permanece igual para o consumidor?

O formato ResponseErrorJson e os status associados a erros de validação e recurso inexistente continuam sendo os mesmos.

A refatoração é interna: troca-se o mecanismo que determina a resposta.

---

## 15. Operador as, cast explícito e operador !

O professor aproveita a mudança para explicar diferentes formas de conversão de tipos e o aviso de nulabilidade do compilador.

### 15.1 Conversão com as

```csharp
var cashFlowException =
    context.Exception as CashFlowException;
```

Se context.Exception for uma instância compatível com CashFlowException, a variável receberá essa referência.

Se não for compatível, receberá null.

Por isso, a análise de nulabilidade pode sinalizar uma possível desreferência nula ao acessar StatusCode ou GetErrors().

### 15.2 Operador !

O professor apresenta a possibilidade de suprimir esse aviso:

```csharp
var cashFlowException =
    (context.Exception as CashFlowException)!;
```

Outra posição possível:

```csharp
var errors = cashFlowException!.GetErrors();
```

O operador ! pós-fixado é chamado **null-forgiving operator**.

Ele informa à análise estática de nulabilidade que o programador assume que a expressão não será nula.

**Ele não realiza uma validação em tempo de execução e não impede NullReferenceException.**

Se o as produzir null e a referência for acessada, a falha ainda poderá ocorrer.

### 15.3 Cast explícito

A segunda alternativa demonstrada é:

```csharp
var cashFlowException =
    (CashFlowException)context.Exception;
```

Como o tipo de origem é Exception e o destino é um subtipo compatível da hierarquia, o cast permite recuperar a referência tipada quando o objeto realmente pertence à classe CashFlowException.

Se um objeto não nulo for incompatível, a conversão lançará InvalidCastException.

### 15.4 Comparação

| Operação | Tipo compatível | Objeto não nulo de tipo incompatível |
|---|---|---|
| expression as CashFlowException | Referência convertida | null |
| (CashFlowException)expression | Referência convertida | InvalidCastException |
| (expression as CashFlowException)! | Referência convertida | null, com aviso estático suprimido |

A última forma não converte null em um objeto válido.

No caso particular de uma conversão de referência cuja origem seja null, tanto as quanto o cast explícito podem produzir null. A comparação acima refere-se a objetos não nulos de tipo incompatível, como explicado pelo professor.

### 15.5 Alternativa com pattern matching

Como complemento, é possível utilizar o padrão de declaração já estudado na aula 112:

```csharp
if (context.Exception is CashFlowException cashFlowException)
{
    // A variável já possui o tipo correto.
    HandleCashFlowException(context, cashFlowException);
}
else
{
    // Preservar tratamento de erro inesperado.
}
```

Com o método auxiliar recebendo uma CashFlowException, não é necessário efetuar outro cast.

```csharp
private void HandleCashFlowException(
    ExceptionContext context,
    CashFlowException exception)
{
    var errorResponse = new ResponseErrorJson(
        exception.GetErrors());

    context.Result = new ObjectResult(errorResponse)
    {
        StatusCode = exception.StatusCode
    };
}
```

Esse é um exemplo alternativo, não uma reprodução literal do filtro mostrado na gravação.

---

## 16. Código consolidado por componente

Os trechos desta seção reconstroem os componentes principais. Devem ser integrados aos namespaces e demais membros existentes na solução.

### 16.1 CashFlowException

```csharp
public abstract class CashFlowException : SystemException
{
    protected CashFlowException(string message)
        : base(message)
    {
    }

    public abstract int StatusCode { get; }

    public abstract List<string> GetErrors();
}
```

A classe preserva o construtor protegido introduzido na aula 112 e acrescenta os dois membros abstratos.

### 16.2 ErrorOnValidationException

```csharp
using System.Net;

public class ErrorOnValidationException : CashFlowException
{
    private readonly List<string> _errors;

    public ErrorOnValidationException(List<string> errors)
        : base(string.Empty)
    {
        _errors = errors;
    }

    public override int StatusCode =>
        (int)HttpStatusCode.BadRequest;

    public override List<string> GetErrors() => _errors;
}
```

O valor string.Empty encaminhado ao construtor da base é preservado da aula anterior.

### 16.3 NotFoundException

```csharp
using System.Net;

public class NotFoundException : CashFlowException
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public override int StatusCode =>
        (int)HttpStatusCode.NotFound;

    public override List<string> GetErrors() => [Message];
}
```

Caso o projeto não suporte a expressão de coleção do C# 12, a forma tradicional é:

```csharp
public override List<string> GetErrors() =>
    new List<string> { Message };
```

### 16.4 Filtro de exceções — tratamento das exceções conhecidas

```csharp
var cashFlowException = (CashFlowException)context.Exception;

var errorResponse = new ResponseErrorJson(
    cashFlowException.GetErrors());

context.HttpContext.Response.StatusCode =
    cashFlowException.StatusCode;

context.Result = new ObjectResult(errorResponse);
```

Este é o núcleo do tratamento refatorado. A verificação inicial do tipo CashFlowException e o tratamento de exceções desconhecidas permanecem como já existiam.

O nome do método auxiliar e os demais detalhes do arquivo completo não foram transcritos literalmente.

### 16.5 Caso de uso de busca por ID — sem mudança funcional

```csharp
var result = await _repository.GetById(id);

if (result is null)
    throw new NotFoundException(
        ResourceErrorMessages.ExpenseNotFound);

return _mapper.Map<ResponseExpenseJson>(result);
```

Esse trecho é recuperado da aula 112. A grafia exata da classe de recursos deve ser conferida na solução.

A aula 114 não exige modificar esse caso de uso: ele continua lançando a mesma exceção.

---

## 17. Como o polimorfismo viabiliza o OCP

O filtro manipula uma referência do tipo CashFlowException.

Embora o tipo da referência seja a classe base, o objeto concreto pode ser uma ErrorOnValidationException ou uma NotFoundException.

Ao acessar os membros sobrescritos, é executada a implementação correspondente ao objeto concreto.

### Exemplo

```csharp
CashFlowException exception =
    new NotFoundException("Despesa não encontrada");

int status = exception.StatusCode;
List<string> errors = exception.GetErrors();
```

Resultado conceitual:

```text
status = 404
errors = ["Despesa não encontrada"]
```

A variável tem o tipo declarado CashFlowException, mas a implementação de StatusCode e GetErrors executada é a de NotFoundException.

### Com uma exceção de validação

```csharp
CashFlowException exception =
    new ErrorOnValidationException(
        new List<string> { "Título obrigatório" });

int status = exception.StatusCode;
List<string> errors = exception.GetErrors();
```

Resultado conceitual:

```text
status = 400
errors = ["Título obrigatório"]
```

### O que muda ao estender a hierarquia?

Uma nova classe concreta precisa implementar:

```csharp
public override int StatusCode
public override List<string> GetErrors()
```

A lógica comum do filtro permanece a mesma, desde que a nova exceção se encaixe no contrato existente.

### Exemplo hipotético de extensão

```csharp
public class ForbiddenException : CashFlowException
{
    public ForbiddenException(string message)
        : base(message)
    {
    }

    public override int StatusCode =>
        (int)HttpStatusCode.Forbidden;

    public override List<string> GetErrors() => [Message];
}
```

Se essa exceção fosse criada e utilizada pela aplicação, o filtro genérico poderia produzir HTTP 403 e as mensagens fornecidas, sem novo else if.

**Esse código é somente uma demonstração complementar do OCP.** O professor menciona situações de ausência de permissão, mas não cria ForbiddenException na gravação.

---

## 18. Testes no Swagger

O professor executa a API para confirmar que os dois tratamentos existentes continuam funcionando depois da refatoração.

### Teste 1 — recurso inexistente

| Informação | Valor observado |
|---|---|
| Endpoint | Consulta de despesa por ID |
| ID solicitado | 1000 |
| Situação | Despesa inexistente |
| Resposta | HTTP 404 Not Found |
| Mensagens | Uma mensagem informando que a despesa não foi encontrada |

Esse teste confirma o funcionamento de NotFoundException com o filtro genérico.

### Teste 2 — cadastro com dados inválidos

O professor tenta registrar uma despesa sem título. O valor permanece em zero.

| Campo | Condição |
|---|---|
| Título | Ausente |
| Valor | 0 |
| Resposta | HTTP 400 Bad Request |
| Quantidade de mensagens | 2 |

As mensagens relatadas são:

- O título é obrigatório.
- O valor deve ser maior que 0.

A redação acima reproduz o sentido narrado; o JSON literal e sua pontuação não são fornecidos integralmente.

### O que os testes demonstram?

A estrutura genérica do filtro continua produzindo respostas diferentes conforme a exceção concreta, preservando as regras já existentes.

A gravação não apresenta benchmark nem testes automatizados e não demonstra a adição efetiva de uma terceira exceção.

---

## 19. Limites, boas práticas e complementos técnicos

### 19.1 OCP não significa eliminar todos os condicionais

A verificação inicial CashFlowException versus exceção inesperada continua existindo.

A vantagem está em evitar a ampliação recorrente da lista de tipos concretos para cada nova extensão compatível.

### 19.2 OCP é relativo ao eixo de extensão

O filtro fica aberto para novos tipos que conseguem representar sua resposta por um status e uma lista de mensagens.

Se uma categoria futura exigir um formato totalmente diferente, cabeçalhos específicos ou outra estratégia de resposta, o contrato poderá precisar evoluir.

Não existe garantia de que nenhuma modificação futura será necessária.

### 19.3 Polimorfismo e responsabilidade

Antes, o filtro determinava o status de cada classe concreta.

Depois, cada exceção fornece seu status e suas mensagens, enquanto a API mantém a responsabilidade de produzir o resultado HTTP.

Isso reduz o conhecimento do filtro sobre classes específicas.

### 19.4 Acoplamento a HTTP

A solução do curso coloca códigos HTTP dentro de CashFlowException e suas subclasses.

Esse desenho é conveniente para o tratamento centralizado da API e não requer dependência do ASP.NET Core quando utiliza System.Net.HttpStatusCode.

Entretanto, as exceções passam a carregar informação de protocolo HTTP. Em uma arquitetura que pretenda manter os erros de aplicação independentes de transporte, uma alternativa seria expor categorias de erro e traduzi-las para HTTP na camada de apresentação.

Essa é uma consideração arquitetural adicional, não uma substituição da solução do instrutor.

### 19.5 Classe abstrata versus interface

CashFlowException continua sendo uma classe abstrata porque faz parte de uma hierarquia de exceções e preserva estado e construtores herdados.

Os novos membros abstratos estabelecem um contrato comum para as classes derivadas.

Não é necessário introduzir uma interface extra somente para implementar OCP.

### 19.6 readonly não equivale a coleção imutável

O campo privado _errors continua apontando para uma List mutável.

Se houver necessidade de uma representação realmente protegida, é preciso considerar o tipo exposto e a propriedade da coleção, não somente o modificador readonly.

### 19.7 O operador ! não é uma correção de runtime

Suprimir uma advertência de nulabilidade não prova que um objeto existe.

A segurança do cast depende da condição que controla o fluxo do filtro.

### 19.8 Encapsulamento não substitui validação

O método GetErrors torna uniforme o acesso às mensagens, mas não executa as regras de validação. Elas continuam nos componentes responsáveis pelo caso de uso.

### 19.9 Preservar o fallback para erros inesperados

Não é apropriado devolver indiscriminadamente mensagens internas de exceções desconhecidas aos clientes.

A separação preexistente entre erros conhecidos do CashFlow e falhas inesperadas continua importante.

### 19.10 Não confundir ISP, OCP e DIP

- ISP (aula 113): consumidores dependem de contratos adequados às operações que utilizam.
- OCP (aula 114): novas extensões compatíveis não exigem novos ramos no filtro.
- DIP: dependência de abstrações em vez de detalhes concretos.

Os princípios se relacionam, mas cada um aborda um problema de design diferente.

---

## 20. Exercícios e perguntas de revisão

### Exercício 1 — identificar a violação

Um filtro contém um else if para cada tipo de exceção. Toda nova exceção exige editar o filtro.

**Resposta:** o código não está preparado para esse eixo de extensão e apresenta o problema de OCP discutido na aula.

### Exercício 2 — completar a classe base

Quais membros abstratos foram adicionados?

```csharp
public abstract int StatusCode { get; }

public abstract List<string> GetErrors();
```

### Exercício 3 — status de validação

```csharp
public override int StatusCode =>
    (int)HttpStatusCode.BadRequest;
```

### Exercício 4 — lista de mensagem única

```csharp
public override List<string> GetErrors() => [Message];
```

Forma alternativa:

```csharp
public override List<string> GetErrors() =>
    new List<string> { Message };
```

### Exercício 5 — conversão de tipos

Qual a diferença entre as e cast explícito quando o objeto não nulo é de tipo incompatível?

**Resposta:** as devolve null; o cast explícito lança InvalidCastException.

### Exercício 6 — supressão de nulabilidade

O operador ! impede NullReferenceException?

**Resposta:** não. Ele suprime avisos da análise estática e não faz uma verificação em tempo de execução.

### Exercício 7 — mutabilidade da coleção

Um campo private readonly List<string> pode ter sua lista alterada?

**Resposta:** sim. A referência não pode ser reatribuída livremente depois da construção, mas a coleção permanece mutável.

### Exercício 8 — filtro genérico

Complete o tratamento sem verificar o tipo concreto:

```csharp
var errorResponse = new ResponseErrorJson(
    cashFlowException.GetErrors());

context.HttpContext.Response.StatusCode =
    cashFlowException.StatusCode;

context.Result = new ObjectResult(errorResponse);
```

### Perguntas de revisão

1. O que significa a letra O de SOLID?
2. Qual problema específico do filtro motivou a refatoração?
3. O OCP impede corrigir bugs em classes existentes?
4. Por que a verificação inicial CashFlowException permanece?
5. Quais informações todas as exceções conhecidas precisam fornecer?
6. Qual é a finalidade de StatusCode?
7. Por que a propriedade é abstrata?
8. Por que o retorno é int?
9. Qual a diferença entre HttpStatusCode e StatusCodes?
10. Por que o professor usa System.Net.HttpStatusCode no projeto de exceções?
11. Qual status ErrorOnValidationException fornece?
12. Qual status NotFoundException fornece?
13. Por que GetErrors retorna uma lista mesmo para uma única mensagem?
14. De onde vem Message em NotFoundException?
15. Por que Errors passa a ser um campo privado?
16. Qual é o efeito real de readonly?
17. Qual é o papel de override?
18. Por que o filtro usa ObjectResult?
19. O que as faz quando o objeto é incompatível?
20. O que um cast explícito faz nessa situação?
21. O operador ! modifica o comportamento de runtime?
22. Qual condição permite utilizar CashFlowException no filtro?
23. Como o polimorfismo permite evitar novos else if?
24. Qual ID foi consultado no teste de recurso inexistente?
25. Quais condições inválidas foram utilizadas no teste de cadastro?
26. Quantas mensagens o HTTP 400 apresentou?
27. O professor implementou novas exceções de autenticação ou autorização nesta aula?
28. A solução torna o filtro extensível para qualquer formato de erro imaginável?

---

## 21. Resumo e mapa mental

### Comparação antes e depois

| Aspecto | Antes | Depois |
|---|---|---|
| Seleção de status | Filtro verifica tipos concretos | Exceção fornece StatusCode |
| Obtenção de mensagens | Errors ou Message, dependendo do tipo | GetErrors() |
| Resposta MVC | BadRequestObjectResult ou NotFoundObjectResult | ObjectResult |
| Inclusão de nova exceção compatível | Alteração no filtro | Implementação do contrato abstrato |
| Erros de validação | HTTP 400 | HTTP 400 preservado |
| Recurso inexistente | HTTP 404 | HTTP 404 preservado |

### Mapa mental

```text
                          AULA 114
                     SOLID — OPEN/CLOSED
                              |
                       PROBLEMA ANTERIOR
                              |
                  Filtro com if / else if
                  para cada exceção concreta
                              |
                              v
                    CashFlowException
                         abstract
                              |
                   +----------+----------+
                   |                     |
               StatusCode             GetErrors()
                  int                 List<string>
                   |                     |
                   +----------+----------+
                              |
             +----------------+----------------+
             |                                 |
   ErrorOnValidationException          NotFoundException
             |                                 |
            400                               404
             |                                 |
          _errors                         [Message]
             |                                 |
             +----------------+----------------+
                              |
                         POLIMORFISMO
                              |
                     Exception Filter
                              |
                       ResponseErrorJson
                              |
                         ObjectResult
                              |
                        HTTP dinâmico
                              |
                 +------------+------------+
                 |                         |
              ID 1000               Cadastro inválido
                 |                         |
              HTTP 404                  HTTP 400
              1 mensagem               2 mensagens
```

**Conclusão:** a aula 114 refatora o filtro de exceções segundo o Open/Closed Principle. CashFlowException passa a declarar StatusCode e GetErrors como membros abstratos; as classes concretas fornecem suas próprias implementações; o filtro utiliza a abstração comum e produz um ObjectResult com o código e as mensagens correspondentes. O tratamento de exceções conhecidas fica extensível sem exigir uma nova condição para cada classe concreta compatível. Os testes manuais no Swagger confirmam a preservação das respostas HTTP 404 e 400.
