# Aula 108 — Utilizando AutoMapper com IMapper no use case e documentando respostas HTTP no Swagger

> **Tema:** injeção de `IMapper`, transformação de `RequestRegisterExpenseJson` em `Expense`, transformação de `Expense` em `ResponseRegisterExpenseJson` e documentação do endpoint de cadastro com `ProducesResponseType`.
>
> **Continuação direta da aula 107**, na qual o instrutor criou `AutoMapping : Profile`, configurou dois `CreateMap` e registrou o AutoMapper na extensão de injeção de dependência da Application.
>
> **Resultado mostrado na gravação:** cadastro de uma despesa chamada **Mouse**, valor **55**, tipo de pagamento **1**; mapeamento de entrada e saída inspecionado pelo depurador; resposta **HTTP 201 Created**; documentação Swagger exibindo respostas **201** e **400**.
>
> **Nota sobre a fonte:** os passos e resultados abaixo derivam da transcrição. As amostras de código reconstroem os trechos narrados; alguns namespaces e partes da action ou do DTO não foram exibidos integralmente. Esclarecimentos técnicos adicionais são apresentados como observações, sem atribuir ao professor implementações não realizadas nesta aula.

## Índice

1. Objetivo e contexto da aula
2. Relembrando o Profile da aula 107
3. O que muda no fluxo de cadastro de despesas
4. Injeção de IMapper no RegisterExpenseUseCase
5. Eliminando o mapeamento manual de Request para Expense
6. Entendendo Map<TDestination>(source)
7. Convertendo Expense para ResponseRegisterExpenseJson
8. Ordem entre validação, mapeamento, repositório e commit
9. Código reconstruído do use case
10. Testando o AutoMapper pelo Swagger
11. Verificando os campos e a conversão entre enums
12. Mapeamento de resposta e retorno HTTP 201
13. Por que documentar o endpoint?
14. Documentando HTTP 201 com ProducesResponseType
15. Documentando HTTP 400 com ResponseErrorJson
16. Relação com ErrorOnValidationException e o filtro de exceções
17. Código reconstruído do controller
18. Diferença entre documentação Swagger e comportamento real da API
19. Boas práticas, ressalvas e possíveis falhas
20. Exercícios e perguntas de revisão
21. Resumo e mapa mental

---

## 1. Objetivo e contexto da aula

O professor anuncia dois objetivos:

1. **Testar o AutoMapper** configurado na aula passada e usá-lo no caso de uso de registro de despesas.
2. **Completar a documentação do endpoint** de cadastro, que ainda não descrevia as respostas HTTP no Swagger.

Até aqui, o CashFlow já possuía:

- `IExpensesRepository` e `ExpensesRepository`;
- `IUnitOfWork` e `UnitOfWork`;
- persistência assíncrona com `AddAsync` e `SaveChangesAsync` (aula 106);
- um `Profile` contendo configurações de mapeamento;
- registro do AutoMapper no container de injeção de dependência.

### Qual é o resultado desejado?

O caso de uso deve receber a requisição, validá-la, transformá-la em uma entidade, persistir e devolver um DTO de resposta — sem precisar atribuir manualmente cada propriedade.

```text
RequestRegisterExpenseJson
            |
            | IMapper
            v
         Expense
            |
            | IExpensesRepository.Add (assíncrono)
            v
     Contexto do EF Core
            |
            | IUnitOfWork.Commit (assíncrono)
            v
          MySQL
            |
            | IMapper
            v
ResponseRegisterExpenseJson
            |
            v
         HTTP 201
```

O segundo objetivo é deixar explícito, na documentação OpenAPI exibida pelo Swagger, o que a API retorna quando a operação é bem-sucedida ou falha por validação.

---

## 2. Relembrando o Profile da aula 107

Na aula anterior, o professor criou um `Profile` na camada Application, chamado `AutoMapping`.

Os dois mapeamentos configurados foram:

```csharp
CreateMap<RequestRegisterExpenseJson, Expense>();
CreateMap<Expense, ResponseRegisterExpenseJson>();
```

### O que representam?

| Origem | Destino | Finalidade |
|---|---|---|
| `RequestRegisterExpenseJson` | `Expense` | Criar entidade a partir da requisição |
| `Expense` | `ResponseRegisterExpenseJson` | Criar objeto de resposta a partir da entidade |

Além disso, foi configurada a injeção de dependência por meio de `AddApplication` e de um método auxiliar de registro do AutoMapper.

### Como se relacionam CreateMap e Map?

**`CreateMap`** declara as regras do Profile:

```csharp
CreateMap<RequestRegisterExpenseJson, Expense>();
```

**`Map`** solicita a transformação de um objeto específico:

```csharp
Expense entity = _mapper.Map<Expense>(request);
```

Sem a configuração correspondente, a tentativa de mapeamento pode gerar um erro de configuração/execução.

A aula 108 utiliza essas regras que já estavam registradas; o professor **não cria outro Profile**.

---

## 3. O que muda no fluxo de cadastro de despesas

O ponto principal da refatoração é o `RegisterExpenseUseCase`.

Antes, o use case construía manualmente a entidade, por exemplo:

```csharp
var entity = new Expense
{
    Title = request.Title,
    Description = request.Description,
    Amount = request.Amount,
    Date = request.Date
    // Outras propriedades, conforme a entidade existente.
};
```

O exemplo acima é ilustrativo: o professor mostra o código manual no Visual Studio, mas a transcrição não descreve integralmente cada linha.

### Depois do AutoMapper

```csharp
var entity = _mapper.Map<Expense>(request);
```

A configuração de mapeamento passa a ficar centralizada no `Profile`, em vez de espalhada pelo caso de uso.

O mesmo ocorre na construção da resposta.

### O que permanece igual?

- A validação da requisição continua existindo.
- O repositório continua sendo responsável pela inclusão no contexto.
- A unidade de trabalho continua persistindo as alterações.
- O fluxo assíncrono com `await` continua sendo utilizado.
- A API mantém o contrato de cadastro e a resposta de sucesso HTTP 201.

A refatoração é principalmente uma **substituição do mapeamento manual por um serviço de mapeamento já configurado**.

---

## 4. Injeção de IMapper no RegisterExpenseUseCase

O professor abre `RegisterExpenseUseCase` no Visual Studio e adiciona `IMapper` como último parâmetro do construtor.

### Dependências anteriores

A classe já dependia de:

- `IExpensesRepository`;
- `IUnitOfWork`.

### Nova dependência

```csharp
IMapper mapper
```

O tipo `IMapper` é definido pelo pacote AutoMapper.

### Exemplo do construtor depois da alteração

```csharp
using AutoMapper;

public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IExpensesRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterExpenseUseCase(
        IExpensesRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
}
```

Esse trecho representa apenas a configuração de dependências. O código completo da classe continua contendo o método de execução do cadastro.

### O que significa _mapper = mapper?

A variável `mapper` é o argumento recebido pelo construtor.

O campo `_mapper` armazena essa dependência para uso nos métodos da classe.

A convenção do prefixo `_` em campos privados já foi utilizada anteriormente no projeto.

### Por que o use case não faz new Mapper(...)?

Porque o AutoMapper já foi configurado no container de DI na aula 107.

A classe consumidora pede a interface `IMapper` e recebe uma implementação apropriada.

### Benefício arquitetural

O use case não precisa construir manualmente o objeto de configuração ou conhecer como a biblioteca foi registrada.

Ele apenas utiliza a abstração injetada.

---

## 5. Eliminando o mapeamento manual de Request para Expense

O professor seleciona o trecho responsável pela criação e pelo preenchimento da entidade.

Em seguida, remove esse código e o substitui por uma única chamada a `Map`.

### Código refatorado

```csharp
var entity = _mapper.Map<Expense>(request);
```

### Qual é a origem?

A variável `request`, que contém um objeto do tipo `RequestRegisterExpenseJson`.

### Qual é o destino?

`Expense`, a entidade pertencente ao Domain.

### Como o Profile sabe o que fazer?

Pelo mapeamento criado na aula 107:

```csharp
CreateMap<RequestRegisterExpenseJson, Expense>();
```

### Fluxo

```text
request
   |
   | tipo: RequestRegisterExpenseJson
   |
   v
_mapper.Map<Expense>(request)
   |
   | utiliza o CreateMap configurado
   v
entity
   |
   | tipo: Expense
   v
Propriedades preenchidas conforme as regras
```

### Resultado esperado

Uma nova entidade `Expense` com os valores aplicáveis transferidos a partir do DTO de requisição.

Esse código deixa a intenção explícita: **transformar a requisição em uma entidade**.

---

## 6. Entendendo Map<TDestination>(source)

O instrutor explica a posição do tipo genérico e do argumento do método.

### Sintaxe

```csharp
_mapper.Map<TDestination>(source);
```

### Entre os símbolos < e >

É informado o **tipo desejado para o destino**.

### Dentro dos parênteses

É informado o **objeto de origem** que contém os dados.

### Aplicação no CashFlow

```csharp
var entity = _mapper.Map<Expense>(request);
```

| Parte | Papel |
|---|---|
| `_mapper` | Serviço recebido por DI |
| `Map` | Operação de mapeamento |
| `Expense` | Tipo de destino |
| `request` | Objeto de origem |
| `entity` | Variável que recebe o resultado |

### Não confundir com a ordem do CreateMap

Na configuração:

```csharp
CreateMap<RequestRegisterExpenseJson, Expense>();
```

A ordem é **origem, destino**.

Na chamada:

```csharp
_mapper.Map<Expense>(request);
```

O tipo genérico declarado é o **destino**, enquanto a origem é o objeto fornecido como argumento.

Essa distinção é importante para lembrar a sintaxe sem confusão.

---

## 7. Convertendo Expense para ResponseRegisterExpenseJson

A segunda refatoração ocorre no retorno do caso de uso.

O professor observa que o código ainda produzia uma resposta vazia ou não aproveitava adequadamente os dados da entidade.

Agora passa a utilizar o segundo mapeamento configurado no Profile.

### Código

```csharp
return _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

### Origem

O objeto `entity`, do tipo `Expense`.

### Destino

O tipo `ResponseRegisterExpenseJson`.

### Mapeamento correspondente

```csharp
CreateMap<Expense, ResponseRegisterExpenseJson>();
```

### Fluxo

```text
Expense
   |
   | entidade já cadastrada
   v
_mapper.Map<ResponseRegisterExpenseJson>(entity)
   |
   v
ResponseRegisterExpenseJson
   |
   v
Retorno do use case
```

### Por que isso importa?

O DTO de resposta pode receber os dados já presentes na entidade após o processo de persistência.

Por exemplo, se a entidade teve um identificador preenchido durante a gravação e esse campo estiver mapeado para a resposta, o DTO poderá conter o identificador.

**A transcrição desta aula não informa um número de ID específico para o cadastro de Mouse.** O professor confirma o título e o retorno de sucesso, mas não detalha um novo identificador numérico.

---

## 8. Ordem entre validação, mapeamento, repositório e commit

O professor testa o caso de uso com breakpoints, acompanhando a ordem em que as operações são realizadas.

### Sequência narrada

1. Receber os dados da requisição.
2. Executar a validação.
3. Converter a requisição em uma entidade `Expense`.
4. Adicionar a entidade pelo repositório.
5. Executar o commit pela unidade de trabalho.
6. Converter a entidade persistida em um DTO de resposta.
7. Devolver a resposta ao controller.

### Forma conceitual

```csharp
Validate(request);

var entity = _mapper.Map<Expense>(request);

await _repository.Add(entity);
await _unitOfWork.Commit();

return _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

A linha `Validate(request)` representa a validação existente e não necessariamente o nome literal da função no projeto.

### Por que mapear a resposta depois do commit?

Porque a entidade pode receber dados relacionados à persistência, como a chave gerada pelo banco.

Na aula 103, o professor já demonstrou que a entidade pode ter seu identificador preenchido após `SaveChanges`.

O ponto importante é que o DTO seja produzido a partir do estado apropriado da entidade.

### A ordem influencia o comportamento?

Sim.

Construir a resposta antes de persistir poderia produzir dados diferentes caso alguns campos sejam definidos pelo banco no momento do salvamento.

---

## 9. Código reconstruído do use case

Esta seção reúne a mudança principal em uma estrutura coerente.

### Trechos de dependência e execução

```csharp
using AutoMapper;
using CashFlow.Domain.Repositories;
using CashFlow.Domain.Repositories.Expenses;

public class RegisterExpenseUseCase : IRegisterExpenseUseCase
{
    private readonly IExpensesRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterExpenseUseCase(
        IExpensesRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseRegisterExpenseJson> Execute(
        RequestRegisterExpenseJson request)
    {
        // Executar aqui a validação que já existe no projeto.

        var entity = _mapper.Map<Expense>(request);

        await _repository.Add(entity);
        await _unitOfWork.Commit();

        return _mapper.Map<ResponseRegisterExpenseJson>(entity);
    }
}
```

### Atenção à fidelidade

Este é um **exemplo estrutural**. Não reproduz literalmente os métodos de validação e todos os namespaces da solução, que não foram fornecidos na transcrição atual.

Ao aplicar o código no projeto, a validação original deve ser preservada, inclusive o lançamento das exceções já utilizadas pela API.

### Quais alterações foram efetivamente narradas?

- Injetar `IMapper`.
- Armazená-lo no campo `_mapper`.
- Substituir o mapeamento manual de requisição por `_mapper.Map<Expense>(request)`.
- Substituir a construção da resposta por `_mapper.Map<ResponseRegisterExpenseJson>(entity)`.
- Manter a inclusão e o commit assíncronos criados anteriormente.

### O que não mudou?

A interface do use case continua podendo retornar:

```csharp
Task<ResponseRegisterExpenseJson>
```

O professor não altera novamente o contrato assíncrono estudado na aula 106.

---

## 10. Testando o AutoMapper pelo Swagger

Após concluir a refatoração do use case, o professor executa a aplicação pelo Visual Studio.

O Swagger é aberto, e ele realiza uma requisição ao endpoint de cadastro de despesas.

### Valores mencionados na aula

| Campo | Valor |
|---|---|
| Título | Mouse |
| Descrição | Texto de teste não especificado |
| Valor | 55 |
| Tipo de pagamento | 1 |
| Data | Não detalhada na transcrição |

### Procedimento

1. Executar a API pelo Visual Studio.
2. Abrir o Swagger.
3. Expandir a operação de cadastro de despesas.
4. Acionar **Try it out**.
5. Informar os dados da despesa de exemplo.
6. Executar a requisição.
7. Acompanhar os breakpoints com `F10` e inspeção de variáveis.

### O que o instrutor verifica primeiro?

A validação da requisição.

A transcrição informa que o teste passou pela validação com sucesso.

### Depois, o que é verificado?

A entidade produzida por:

```csharp
_mapper.Map<Expense>(request);
```

O professor expande o objeto no debugger e inspeciona suas propriedades.

---

## 11. Verificando os campos e a conversão entre enums

O teste de entrada é especialmente importante porque a aula 107 discutiu a correspondência entre propriedades e a conversão de um enum presente em assemblies diferentes.

### Campos observados

O professor inspeciona:

- título;
- tipo de pagamento;
- descrição;
- data;
- valor.

Ele relata que os valores foram preenchidos corretamente.

### O detalhe do PaymentType

O DTO de requisição utiliza um enum de Communication.

A entidade utiliza um enum de Domain.

O professor verifica que a transformação funcionou para o valor de teste **1**.

### O que essa observação comprova?

Que **nesse cenário e nessa configuração**, o AutoMapper converteu o valor fornecido em um valor aceito pela entidade.

### O que não podemos generalizar sem testes?

Não significa que toda combinação de enums com os mesmos nomes de propriedades sempre tenha equivalência semântica.

Se os valores numéricos ou o significado dos membros não coincidirem, uma conversão automática pode produzir resultados incorretos.

A recomendação técnica é testar todos os membros relevantes ou estabelecer uma conversão explícita quando os tipos tiverem semânticas diferentes.

### Resultado da gravação

O professor considera que o mapeamento de entrada está funcionando corretamente.

Não é adicionada uma configuração `ForMember` nesta aula.

---

## 12. Mapeamento de resposta e retorno HTTP 201

Depois da inclusão no repositório e do commit, o caso de uso executa:

```csharp
_mapper.Map<ResponseRegisterExpenseJson>(entity);
```

### O que foi inspecionado?

O professor acompanha a criação do objeto de resposta e verifica que o título **Mouse** aparece no resultado.

### Resultado no Swagger

A API devolve **HTTP 201 Created**.

A resposta contém os dados mapeados de acordo com a configuração, incluindo o título mencionado.

### Por que HTTP 201?

O status `201 Created` é apropriado quando uma requisição HTTP resulta na criação bem-sucedida de um recurso.

O CashFlow já utilizava esse comportamento para o endpoint de cadastro antes da introdução do AutoMapper.

### Qual é a conclusão do teste?

Os dois mapeamentos configurados na aula 107 foram utilizados com sucesso no fluxo demonstrado:

```text
RequestRegisterExpenseJson
          |
          | mapeamento OK
          v
        Expense
          |
          | persistência OK
          v
ResponseRegisterExpenseJson
          |
          | mapeamento OK
          v
      HTTP 201
```

**A transcrição não apresenta um teste automatizado de todos os campos ou uma medição de desempenho.** Trata-se de uma validação manual com Swagger e debugger.

---

## 13. Por que documentar o endpoint?

Depois de confirmar o funcionamento do AutoMapper, o professor observa que a operação ainda não apresentava documentação suficiente das respostas no Swagger.

### O que é Swagger neste contexto?

Uma interface visual utilizada para consultar e experimentar operações descritas pela especificação OpenAPI da API.

### O que está faltando?

Informar quais resultados o endpoint pode devolver.

Para o cadastro de despesas, o professor escolhe dois cenários:

1. **201 Created:** cadastro executado com sucesso.
2. **400 Bad Request:** erro de validação da requisição.

### Por que isso é útil?

A documentação permite ao consumidor da API conhecer:

- os status HTTP esperados;
- o tipo dos objetos retornados;
- os esquemas de resposta;
- a diferença entre sucesso e erro.

Ela também facilita testes exploratórios pela interface Swagger.

---

## 14. Documentando HTTP 201 com ProducesResponseType

O professor utiliza o atributo:

```csharp
[ProducesResponseType(
    typeof(ResponseRegisterExpenseJson),
    StatusCodes.Status201Created)]
```

### O que significa ProducesResponseType?

É um atributo do ASP.NET Core que fornece metadados para a descrição da resposta de uma action.

### Primeiro argumento: typeof(...)

```csharp
typeof(ResponseRegisterExpenseJson)
```

Indica o tipo associado à resposta de sucesso.

### Segundo argumento: StatusCodes.Status201Created

```csharp
StatusCodes.Status201Created
```

Representa o status HTTP **201**.

### O que o Swagger pode exibir?

Uma resposta documentada com status **201 Created** e esquema correspondente a `ResponseRegisterExpenseJson`.

### Por que usar typeof?

O operador `typeof` produz um `System.Type` representando o DTO, permitindo que o framework o utilize como metadado.

Não cria uma instância do DTO e não executa seu construtor.

---

## 15. Documentando HTTP 400 com ResponseErrorJson

O instrutor explica que o cadastro também pode falhar durante a validação da requisição.

Para representar esse cenário, adiciona outro atributo:

```csharp
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
```

### Primeiro argumento

```csharp
typeof(ResponseErrorJson)
```

Representa o objeto de resposta utilizado para erros.

### Segundo argumento

```csharp
StatusCodes.Status400BadRequest
```

Representa o status HTTP **400**.

### Qual é a estrutura mencionada?

O professor diz que o Swagger passa a exibir um objeto de erro cuja propriedade contém **uma lista de strings** com mensagens de erro.

Um formato ilustrativo seria:

```json
{
  "errors": [
    "O título é obrigatório.",
    "O valor deve ser válido."
  ]
}
```

**Esse JSON é apenas ilustrativo.** A transcrição não fornece os nomes literais das propriedades nem as mensagens exatas utilizadas pela classe `ResponseErrorJson`.

### Por que uma lista?

Porque a validação pode identificar mais de um problema e a API pode devolver várias mensagens ao consumidor.

---

## 16. Relação com ErrorOnValidationException e o filtro de exceções

O instrutor retoma uma parte já existente da arquitetura: o tratamento de erros de validação.

### O fluxo descrito

1. A requisição chega ao caso de uso.
2. A validação identifica um problema.
3. O caso de uso pode lançar `ErrorOnValidationException`.
4. O filtro de exceções da API captura essa condição.
5. A API responde com **HTTP 400 Bad Request**.
6. O corpo utiliza o formato `ResponseErrorJson`.

### Diagrama

```text
RequestRegisterExpenseJson
            |
            v
  RegisterExpenseUseCase
            |
            v
        Validação
            |
       +----+-----+
       |          |
     Válida     Inválida
       |          |
       v          v
    Mapping   ErrorOnValidationException
       |          |
       v          v
  Persistência  Filtro de exceções
       |          |
       v          v
    HTTP 201   ResponseErrorJson
                  |
                  v
               HTTP 400
```

### O atributo de documentação executa a validação?

**Não.**

A validação e o filtro já compõem o comportamento da API.

Os atributos `ProducesResponseType` apenas descrevem as respostas que o endpoint pode produzir.

### Detalhe importante

A nomenclatura `ErrorOnValidationException` foi reconstruída a partir da fala do instrutor e das convenções do projeto. O nome exato deve ser conferido no arquivo da exceção quando for necessária uma reprodução literal da classe.

---

## 17. Código reconstruído do controller

O professor acrescenta dois atributos à action de registro de despesas.

### Versão ilustrativa

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[HttpPost]
[ProducesResponseType(
    typeof(ResponseRegisterExpenseJson),
    StatusCodes.Status201Created)]
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Register(
    [FromBody] RequestRegisterExpenseJson request,
    [FromServices] IRegisterExpenseUseCase useCase)
{
    var response = await useCase.Execute(request);

    return Created(string.Empty, response);
}
```

### O que foi efetivamente adicionado nesta aula?

Os dois atributos:

```csharp
[ProducesResponseType(
    typeof(ResponseRegisterExpenseJson),
    StatusCodes.Status201Created)]
```

e:

```csharp
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
```

### E o restante da action?

O `async Task<IActionResult>`, a chamada `await useCase.Execute(request)` e o retorno de criação já foram introduzidos em aulas anteriores.

A reconstrução acima usa `Created(string.Empty, response)` como exemplo de retorno HTTP 201. A gravação desta aula **não transcreve a implementação literal completa da action**.

### Os dois atributos têm o mesmo nome?

Sim.

O atributo `ProducesResponseType` pode ser aplicado mais de uma vez para documentar diferentes status e respectivos tipos de resposta.

---

## 18. Diferença entre documentação Swagger e comportamento real da API

Esta é uma distinção técnica que vale destacar.

### O que ProducesResponseType faz?

Fornece informações para os metadados da API, que podem ser utilizados pelo gerador de documentação OpenAPI.

### O que ele não faz?

Não força, por conta própria:

- a action a retornar `201`;
- o filtro a retornar `400`;
- o DTO a ser serializado com aquele tipo;
- a execução da validação;
- a captura de exceções.

### Exemplo de interpretação correta

```csharp
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
```

Significa que a API **declara** aquela possibilidade de resposta.

Para que ela realmente aconteça, o tratamento de erros precisa gerar `400` e um objeto no formato esperado.

### O que o professor verifica?

Depois de incluir os atributos, ele executa a API e observa que o Swagger mostra:

- **201 Created**, com o DTO de resposta de cadastro.
- **400 Bad Request**, com `ResponseErrorJson` e suas mensagens.

### Resultado

O endpoint fica mais claro para desenvolvedores que precisam consumi-lo.

---

## 19. Boas práticas, ressalvas e possíveis falhas

### 19.1 O AutoMapper precisa conhecer os tipos

A configuração declarada na aula 107 é necessária para que o use case utilize os dois mapeamentos.

Os mapeamentos não precisam ser repetidos dentro do use case.

### 19.2 IMapper deve ser resolvido pela DI

Como `IMapper` é uma dependência da classe, ele precisa estar disponível no container.

No CashFlow, a aula 107 já registrou o AutoMapper por `AddApplication()`.

### 19.3 O mapeamento não substitui a validação

A operação deve continuar validando a entrada conforme as regras da Application.

Converter um objeto não o torna automaticamente válido para o domínio.

### 19.4 Conversão de enums requer verificação

O valor de teste `1` funcionou na gravação, mas isso não garante que todos os membros de dois enums diferentes tenham os mesmos valores numéricos e significado.

### 19.5 O resultado do banco pode alterar a entidade

O mapeamento para a resposta acontece depois da persistência.

Isso permite aproveitar propriedades da entidade atualizadas no fluxo de gravação, quando aplicável.

### 19.6 IMapper.Map não é uma chamada assíncrona

```csharp
var entity = _mapper.Map<Expense>(request);
```

O mapeamento em memória mostrado aqui é síncrono.

Não é necessário aplicar `await` a `Map`.

Os `await` continuam sendo utilizados para as operações assíncronas do repositório, do Unit of Work e do caso de uso.

### 19.7 Menos linhas não significa ausência de configuração

A lógica de mapeamento agora está no `Profile`.

A classe do caso de uso fica mais compacta, mas a aplicação ainda precisa manter as regras de transformação corretas.

### 19.8 Documentar respostas reais, não hipotéticas

A documentação OpenAPI deve refletir o comportamento real da API.

Se um endpoint puder retornar outros status relevantes, eles devem ser avaliados e documentados conforme o contrato real, e não adicionados sem evidência.

### 19.9 Os atributos não garantem um corpo consistente

A API ainda precisa assegurar que o DTO retornado por `201` corresponda ao esquema declarado e que erros de validação sejam produzidos no formato documentado.

### 19.10 O teste manual não cobre todas as situações

O vídeo mostra uma inserção válida e a descrição da resposta de erro no Swagger.

Ele **não realiza uma requisição inválida para demonstrar efetivamente HTTP 400 nesta gravação**.

O cenário de erro é documentado com base no tratamento de exceções já existente no projeto.

### 19.11 O cadastro do Mouse não informa o Id

Diferentemente de algumas aulas anteriores, esta transcrição não apresenta o valor numérico do identificador gerado.

Não devemos presumir que o Id será 32 apenas porque a última despesa documentada recebeu Id 31.

### 19.12 Nome da classe de resposta

A transcrição pronuncia nomes próximos de "response register expense json" e "response error json".

O resumo utiliza `ResponseRegisterExpenseJson` e `ResponseErrorJson`, coerentes com o contexto documentado, mas a grafia exata deve ser confirmada no código caso haja divergências de nomenclatura.

---

## 20. Exercícios e perguntas de revisão

### Exercício 1 — Injeção de IMapper

Complete o construtor:

```csharp
private readonly IMapper _mapper;

public RegisterExpenseUseCase(
    IExpensesRepository repository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
{
    _repository = repository;
    _unitOfWork = unitOfWork;

    // Complete aqui.
}
```

**Resposta:**

```csharp
_mapper = mapper;
```

### Exercício 2 — Mapeamento de entrada

Converta um objeto `request` em `Expense` usando o `IMapper`.

**Resposta:**

```csharp
var entity = _mapper.Map<Expense>(request);
```

### Exercício 3 — Mapeamento de saída

Converta uma entidade `entity` em `ResponseRegisterExpenseJson`.

**Resposta:**

```csharp
var response = _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

### Exercício 4 — Documentação 201

Declare um atributo de resposta de sucesso que documente `ResponseRegisterExpenseJson`.

**Resposta:**

```csharp
[ProducesResponseType(
    typeof(ResponseRegisterExpenseJson),
    StatusCodes.Status201Created)]
```

### Exercício 5 — Documentação 400

Declare um atributo para o DTO de erro de validação.

**Resposta:**

```csharp
[ProducesResponseType(
    typeof(ResponseErrorJson),
    StatusCodes.Status400BadRequest)]
```

### Perguntas adicionais

1. Qual é o primeiro objetivo da aula?
2. Qual recurso da aula 107 permite injetar `IMapper`?
3. De qual biblioteca vem a interface `IMapper`?
4. Quais dependências o use case já possuía antes desta aula?
5. Onde o professor adiciona `IMapper` no construtor?
6. O que significa `_mapper = mapper`?
7. Qual mapeamento manual foi removido?
8. O que representa o tipo genérico em `_mapper.Map<Expense>(request)`?
9. Qual objeto é passado entre os parênteses?
10. Qual `CreateMap` do Profile atende a essa operação?
11. Como a entidade é convertida em `ResponseRegisterExpenseJson`?
12. Por que a construção da resposta acontece depois do commit?
13. O mapeamento com `Map` exige `await`?
14. Quais dados foram enviados no teste de cadastro?
15. O que foi conferido no debugger depois de mapear a requisição?
16. O que o teste demonstrou sobre o enum de tipo de pagamento?
17. Qual status HTTP foi retornado após o cadastro de Mouse?
18. O vídeo informa um novo valor de Id?
19. Qual é o segundo objetivo da aula?
20. Qual atributo é usado para documentar respostas no Swagger?
21. O que faz `typeof(ResponseRegisterExpenseJson)`?
22. Qual é o significado de `StatusCodes.Status201Created`?
23. Qual tipo é documentado para erro de validação?
24. Que exceção o professor menciona no fluxo de validação?
25. Qual componente da API captura essa exceção?
26. O atributo `ProducesResponseType` altera automaticamente o status retornado pela action?
27. A gravação realiza uma requisição inválida para comprovar HTTP 400?
28. O que a interface Swagger passou a exibir após a alteração?

---

## 21. Resumo e mapa mental

### Alterações realizadas

- O professor abre `RegisterExpenseUseCase` e adiciona a dependência `IMapper`.
- Armazena essa dependência no campo `_mapper`.
- Remove o código manual de criação/preenchimento da entidade.
- Utiliza `_mapper.Map<Expense>(request)`.
- Remove a construção manual ou vazia da resposta.
- Utiliza `_mapper.Map<ResponseRegisterExpenseJson>(entity)`.
- Mantém a validação e a persistência assíncrona via repositório e Unit of Work.
- Testa o cadastro com título **Mouse**, valor **55** e tipo de pagamento **1**.
- Verifica no debugger que o mapeamento de entrada preenche os campos.
- Verifica que o enum de pagamento é convertido no cenário demonstrado.
- Verifica o mapeamento de saída e o título **Mouse** na resposta.
- Confirma **HTTP 201 Created** no Swagger.
- Adiciona `ProducesResponseType` para documentar **201** com `ResponseRegisterExpenseJson`.
- Adiciona `ProducesResponseType` para documentar **400** com `ResponseErrorJson`.
- Confirma que o Swagger exibe as duas possibilidades de resposta.
- Encerra a aula anunciando a continuidade da expansão da API.

### Comparação antes/depois

| Aspecto | Antes | Depois |
|---|---|---|
| Criação de Expense | Propriedades atribuídas manualmente | `_mapper.Map<Expense>(request)` |
| Criação da resposta | Objeto vazio ou construção manual | `_mapper.Map<ResponseRegisterExpenseJson>(entity)` |
| Dependências do use case | Repositório e Unit of Work | Repositório, Unit of Work e IMapper |
| Local das regras de mapeamento | Código dentro do use case | Profile `AutoMapping` |
| Persistência | `await Add` e `await Commit` | Continua igual |
| Documentação de sucesso | Ausente/incompleta | HTTP 201 + ResponseRegisterExpenseJson |
| Documentação de validação | Ausente/incompleta | HTTP 400 + ResponseErrorJson |
| Teste de AutoMapper | Não executado na aula 107 | Confirmado manualmente no Swagger |

### Mapa mental

```text
                                AULA 108
                       AUTOMAPPER + SWAGGER
                                  |
               +------------------+--------------------+
               |                                       |
        UTILIZAR AUTOMAPPER                     DOCUMENTAR ENDPOINT
               |                                       |
      RegisterExpenseUseCase                   ExpensesController
               |                                       |
      Recebe IMapper por DI                   ProducesResponseType
               |                                       |
      +--------+---------+                     +-------+-------+
      |                  |                     |               |
      v                  v                     v               v
 Map<Expense>     Map<Response...>         HTTP 201         HTTP 400
    (request)         (entity)                |                |
      |                  |                    |                |
      v                  v                    v                v
 Request ->         Entity ->             ResponseRegister   ResponseError
 Entity              Response             ExpenseJson        Json
      |                  |
      +--------+---------+
               |
               v
       Testar com Swagger
               |
         +-----+------+
         |            |
         v            v
       Mouse       Valor 55
         |            |
         +-----+------+
               |
               v
         Pagamento 1
               |
               v
     Validar mapeamento com F10
               |
               v
       Persistir e retornar
               |
               v
          HTTP 201 Created
               |
               v
       Swagger com 201 e 400
```

**Conclusão:** a aula 108 conclui a primeira aplicação prática do AutoMapper no cadastro de despesas. O use case deixa de mapear propriedades manualmente e passa a utilizar `IMapper` para transformar a requisição em entidade e a entidade em resposta. O teste do cadastro de **Mouse** confirma o funcionamento do caminho de sucesso com **HTTP 201**. Em seguida, o professor melhora a documentação OpenAPI da action, declarando os contratos de resposta **201 Created** e **400 Bad Request**. A implementação real das validações e do filtro de exceções continua sendo responsabilidade dos componentes já existentes; os atributos adicionados apenas documentam esses comportamentos.
