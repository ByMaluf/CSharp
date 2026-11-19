# AutoMapper no CashFlow: criação de Profile, CreateMap e registro na injeção de dependência

> **Aula 107 — Configurando AutoMapper na camada Application**
>
> Continuação do estudo sobre mapeamento de objetos: nesta aula, o instrutor instala o pacote **AutoMapper** no projeto Application, cria a classe `AutoMapping` herdando de `Profile`, declara dois mapeamentos iniciais — **requisição → entidade** e **entidade → resposta** — e registra o AutoMapper na extensão de injeção de dependência da Application.
>
> **Escopo real:** a aula termina com o **Profile criado e a DI configurada**. A substituição do mapeamento manual dentro de `RegisterExpenseUseCase` e o teste funcional do AutoMapper ficam para a próxima aula.
>
> **Fidelidade e complementos:** os nomes de arquivos, passos e chamadas principais refletem a transcrição. Quando namespaces ou APIs de versões modernas são reconstruídos, isso é indicado. Recomendações de compatibilidade, licenciamento e validação de mapeamentos estão separadas da implementação apresentada pelo professor.

## Índice

1. Objetivo e contexto
2. Por que mapear objetos no CashFlow?
3. Por que o Profile fica na Application?
4. Instalando o pacote AutoMapper no NuGet
5. Criando a pasta AutoMapper
6. Criando AutoMapping e herdando de Profile
7. Organizando os mapeamentos no construtor
8. Criando o mapeamento de Request para Expense
9. Convenção de propriedades iguais
10. Conversão entre enums de namespaces diferentes
11. Criando o mapeamento de Expense para Response
12. O significado de CreateMap<TSource, TDestination>()
13. Reorganizando DependencyInjectionExtension na Application
14. Registrando AutoMapper com IServiceCollection
15. Entendendo typeof(AutoMapping)
16. Código reconstruído da implementação
17. Como o IMapper será utilizado futuramente
18. Pontos técnicos e diferenças entre versões do AutoMapper
19. Testes, validação de configuração e precauções
20. Perguntas para revisão
21. Resumo e mapa mental

---

## 1. Objetivo e contexto

Na aula anterior sobre **o conceito de AutoMapper**, foi explicado como a biblioteca pode converter um objeto de um tipo em um objeto de outro tipo, copiando dados das propriedades conforme regras de mapeamento.

O instrutor agora passa à configuração no projeto **CashFlow**.

### O que será feito nesta aula?

1. Instalar o pacote NuGet `AutoMapper` no projeto Application.
2. Criar a pasta `AutoMapper` dentro da Application.
3. Criar a classe `AutoMapping`.
4. Declarar que ela herda de `Profile`.
5. Criar os métodos privados `RequestToEntity()` e `EntityToResponse()`.
6. Definir `CreateMap<RequestRegisterExpenseJson, Expense>()`.
7. Definir `CreateMap<Expense, ResponseRegisterExpenseJson>()`.
8. Organizar a classe `DependencyInjectionExtension`.
9. Registrar o AutoMapper na DI com `AddAutoMapper`.
10. Indicar o Profile usando `typeof(AutoMapping)`.

### O que não acontece?

O professor **não substitui nesta gravação** o código manual de mapeamento do use case por `_mapper.Map<...>()`.

Ele termina anunciando que fará esse teste na **próxima aula**.

---

## 2. Por que mapear objetos no CashFlow?

A API recebe uma representação dos dados enviados pelo cliente, mas o Domain trabalha com uma entidade própria.

No registro de despesas, os principais tipos são:

- `RequestRegisterExpenseJson`: dados de entrada da requisição.
- `Expense`: entidade de domínio.
- `ResponseRegisterExpenseJson`: dados devolvidos à API.

O fluxo é:

```text
POST /expenses
       |
       v
RequestRegisterExpenseJson
       |
       | mapeamento de entrada
       v
Expense (Domain)
       |
       | persistência pelo repositório
       v
MySQL
       |
       | mapeamento de saída
       v
ResponseRegisterExpenseJson
```

### Antes do AutoMapper

No `RegisterExpenseUseCase`, o instrutor mostra que a entidade estava sendo construída manualmente, associando as propriedades uma a uma.

Exemplo didático:

```csharp
var entity = new Expense
{
    Title = request.Title,
    Description = request.Description,
    Amount = request.Amount,
    Date = request.Date,
    // PaymentType requer a conversão apropriada.
};
```

Esse trecho é ilustrativo: a transcrição não apresenta toda a declaração literal da entidade nem todos os parâmetros do mapeamento existente.

### Qual problema o AutoMapper procura resolver?

Centralizar regras repetitivas de conversão entre tipos, especialmente quando muitos campos possuem nomes correspondentes.

Um mapeamento configurado poderá ser utilizado por vários componentes que recebam `IMapper` por injeção de dependência.

### AutoMapper substitui regras de negócio?

Não.

Mapear propriedades é diferente de:

- validar a requisição;
- executar regras de negócio;
- salvar uma entidade;
- gerenciar transações;
- decidir quais informações uma API pode expor.

A biblioteca atua na **transformação de representações de dados**, de acordo com a configuração.

---

## 3. Por que o Profile fica na Application?

O professor explica que, **na arquitetura deste projeto**, a Application é o lugar adequado para organizar os mapeamentos.

### A justificativa apresentada

A Application é o ponto em que:

- uma requisição precisa ser transformada em entidade;
- uma entidade precisa ser transformada em resposta.

Portanto, faz sentido manter os Profiles perto dos casos de uso que realizam essas transformações.

```text
CashFlow.Application
     |
     +-- UseCases
     |     |
     |     +-- RegisterExpenseUseCase
     |
     +-- AutoMapper
           |
           +-- AutoMapping
                 |
                 +-- Request -> Expense
                 |
                 +-- Expense -> Response
```

### É obrigatório existir apenas um Profile?

Não.

O professor comenta que, em quase todos os seus projetos, um Profile na Application costuma ser suficiente, mas reconhece cenários específicos em que outras camadas podem necessitar de mapeamentos próprios.

Esse percentual mencionado na fala reflete a **experiência do instrutor**, não uma regra técnica ou estatística universal.

### Uma alternativa arquitetural

Conforme o projeto cresce, é possível separar Profiles por funcionalidade, módulo ou conjunto de DTOs.

Exemplo conceitual:

```text
Application
└── AutoMapper
    ├── ExpensesProfile.cs
    ├── UsersProfile.cs
    └── ReportsProfile.cs
```

**Isso não é feito na gravação.** O projeto do curso utiliza uma classe `AutoMapping`, organizada em métodos privados.

---

## 4. Instalando o pacote AutoMapper no NuGet

O instrutor demonstra a instalação no Visual Studio.

### Passo a passo

1. Clicar com o botão direito no projeto `CashFlow.Application`.
2. Selecionar **Manage NuGet Packages**.
3. Abrir a aba **Browse**.
4. Pesquisar por **AutoMapper**.
5. Selecionar o pacote correspondente.
6. Clicar em **Install**.
7. Confirmar as alterações solicitadas.
8. Verificar que o pacote aparece como instalado.

### Por que instalar na Application?

Porque o projeto Application passará a declarar o Profile, utilizar `CreateMap` e registrar a configuração da biblioteca.

A Infrastructure e o Domain não precisam ser alterados apenas para criar esses mapeamentos.

### Qual namespace fornece Profile?

```csharp
using AutoMapper;
```

O tipo `Profile` faz parte da biblioteca AutoMapper.

### Complemento sobre versões

A gravação não identifica a versão exata instalada.

Isso importa porque a API de registro na DI mudou em versões recentes. A seção **18** preserva o exemplo do curso e explica a forma exigida em versões modernas.

---

## 5. Criando a pasta AutoMapper

Após instalar o NuGet, o professor expande o projeto Application no Solution Explorer.

Cria uma pasta chamada:

```text
AutoMapper
```

Dentro dela, adiciona uma classe:

```text
AutoMapping.cs
```

### Estrutura

```text
CashFlow.Application
├── AutoMapper
│   └── AutoMapping.cs
├── DependencyInjectionExtension.cs
└── UseCases
    └── Expenses
        └── Register
            ├── IRegisterExpenseUseCase.cs
            ├── RegisterExpenseUseCase.cs
            └── RegisterExpenseValidator.cs
```

Os nomes das subpastas de `UseCases` são uma representação didática baseada na organização descrita nas aulas anteriores. A gravação confirma explicitamente o novo diretório `AutoMapper` e a nova classe `AutoMapping`.

### AutoMapping é um nome obrigatório?

Não.

O instrutor esclarece que esse é um nome escolhido por ele.

O AutoMapper identifica a configuração pela herança de `Profile`, e o registro da DI utiliza o tipo correspondente.

---

## 6. Criando AutoMapping e herdando de Profile

O professor altera a classe recém-criada para `public`.

Depois, faz a herança:

```csharp
public class AutoMapping : Profile
{
}
```

### O que significa : Profile?

`Profile` é uma classe da biblioteca AutoMapper.

Ao herdar dela, `AutoMapping` passa a representar uma configuração de mapeamentos.

A sintaxe `:` é a mesma de herança de classes estudada anteriormente em C#.

### Por que a classe é public?

O professor opta por disponibilizar publicamente o tipo do Profile, já que ele será usado como referência no código de registro da DI.

### O Profile executa um mapeamento sozinho?

Não.

Ele **define as regras de mapeamento**. Posteriormente, um `IMapper` configurado utilizará essas regras quando o código solicitar uma conversão.

```text
AutoMapping : Profile
        |
        | declara
        v
CreateMap<Origem, Destino>()
        |
        | configuração utilizada por
        v
IMapper
        |
        | executa
        v
Map<TDestino>(objetoOrigem)
```

---

## 7. Organizando os mapeamentos no construtor

O professor cria um construtor para `AutoMapping`.

Em vez de declarar todos os mapeamentos diretamente nele, prefere separar configurações em métodos privados.

### Métodos escolhidos

```csharp
private void RequestToEntity()
{
}

private void EntityToResponse()
{
}
```

### Construtor

```csharp
public AutoMapping()
{
    RequestToEntity();
    EntityToResponse();
}
```

### Por que fazer assim?

O professor explica que quer organizar os mapeamentos por direção:

- `RequestToEntity` reúne conversões de requisições para entidades.
- `EntityToResponse` reúne conversões de entidades para objetos de resposta.

Esse padrão torna a classe mais fácil de navegar quando novos pares de tipos forem adicionados.

### É obrigatório separar em dois métodos?

Não.

Seria possível escrever diretamente no construtor:

```csharp
public AutoMapping()
{
    CreateMap<RequestRegisterExpenseJson, Expense>();
    CreateMap<Expense, ResponseRegisterExpenseJson>();
}
```

A separação em métodos é uma **decisão de organização** adotada pelo professor.

### Por que os métodos não são static?

Na reconstrução da aula, eles chamam `CreateMap`, membro de configuração disponível na instância de `Profile`.

Por isso, a forma apresentada usa métodos privados de instância.

---

## 8. Criando o mapeamento de Request para Expense

O professor abre o use case de registro de despesas e mostra que atualmente a entidade é construída com atribuições manuais.

Agora, declara a configuração equivalente no Profile.

### Código

```csharp
private void RequestToEntity()
{
    CreateMap<RequestRegisterExpenseJson, Expense>();
}
```

### O que significa a assinatura?

```csharp
CreateMap<TSource, TDestination>();
```

- `TSource`: tipo de **origem**.
- `TDestination`: tipo de **destino**.

### Neste exemplo

```text
RequestRegisterExpenseJson
            |
            | CreateMap
            v
          Expense
```

A requisição fornece os valores e a entidade recebe os campos correspondentes.

### Por que esse mapeamento é necessário?

Porque a Application recebe dados de uma API por um DTO e precisa trabalhar com uma entidade do Domain.

### Atenção

`CreateMap` não executa a conversão imediatamente.

Ele **configura a conversão** que será usada futuramente pelo serviço `IMapper`.

O instrutor ressalta que o teste de utilização será realizado na próxima aula.

---

## 9. Convenção de propriedades iguais

O instrutor compara as propriedades da requisição com as da entidade `Expense`.

Ele identifica correspondências para campos como:

- `Amount`;
- `Date`;
- `Description`;
- `Title`;
- `PaymentType`.

### O que o AutoMapper faz?

Por convenção, tenta mapear membros públicos de origem para membros públicos de destino compatíveis, principalmente quando os nomes correspondem.

### Exemplo simples

```csharp
public class Request
{
    public string Title { get; set; } = string.Empty;
}

public class Expense
{
    public string Title { get; set; } = string.Empty;
}
```

Com um mapeamento configurado:

```csharp
CreateMap<Request, Expense>();
```

A propriedade `Title` pode ser copiada da origem para o destino.

### Nem todo campo com mesmo nome é automaticamente seguro

Além do nome, o mapeamento depende de fatores como:

- compatibilidade/conversão entre os tipos;
- possibilidade de construir o objeto de destino;
- acessibilidade dos membros;
- regras configuradas no Profile;
- tipos aninhados e coleções;
- versão da biblioteca.

O professor usa propriedades correspondentes como o caso simples da demonstração. Essa ideia é correta como regra de convenção, mas **não é uma garantia universal para qualquer objeto**.

---

## 10. Conversão entre enums de namespaces diferentes

O professor faz uma observação específica sobre o tipo de pagamento.

A requisição e a entidade possuem propriedades com o mesmo nome, mas os **tipos de enum vêm de projetos diferentes**:

```text
Communication
    |
    +-- Enum de PaymentType usado na Request

Domain
    |
    +-- Enum de PaymentType usado em Expense
```

### O que ele afirma?

Que, no cenário demonstrado, o AutoMapper será capaz de realizar a conversão entre esses valores e não será necessário criar uma regra customizada para o campo.

### O que merece atenção técnica?

**Mesmo nome de propriedade não garante que enumerações distintas tenham a mesma semântica.**

Exemplo ilustrativo:

```csharp
public enum PaymentFromRequest
{
    Cash = 0,
    CreditCard = 1
}

public enum PaymentInDomain
{
    Cash = 0,
    CreditCard = 1
}
```

Nesse caso, os valores numéricos estão alinhados, o que facilita a conversão.

Mas considere:

```csharp
public enum PaymentFromRequest
{
    Cash = 0,
    CreditCard = 1
}

public enum PaymentInDomain
{
    CreditCard = 0,
    Cash = 1
}
```

Embora os nomes dos membros existam em ambos, uma conversão numérica direta produziria valores **semanticamente diferentes**.

### Implicação

Ao utilizar enums distintos para a mesma informação, é importante verificar:

1. Os valores numéricos.
2. O significado de cada membro.
3. A regra de conversão efetivamente utilizada.
4. Os testes da transformação.

Se os tipos não forem equivalentes, o projeto pode precisar de uma configuração explícita, como `ForMember`, um converter ou um mapeamento de enums específico.

**O professor não adiciona uma conversão customizada nesta aula.** O material apenas registra o cuidado para não generalizar indevidamente seu exemplo.

---

## 11. Criando o mapeamento de Expense para Response

Depois do mapeamento de entrada, o professor configura o caminho de saída.

### Código

```csharp
private void EntityToResponse()
{
    CreateMap<Expense, ResponseRegisterExpenseJson>();
}
```

### Origem e destino

```text
Expense (Domain)
     |
     | CreateMap
     v
ResponseRegisterExpenseJson (Communication)
```

A entidade é a origem; o DTO de resposta é o destino.

### Para que serve?

Quando o caso de uso terminar a operação de registro, ele poderá usar a configuração para produzir um objeto de resposta a partir da entidade.

### Precisão sobre o nome da resposta

O tipo `ResponseRegisterExpenseJson` corresponde à nomenclatura utilizada nos resumos anteriores do CashFlow. A transcrição desta aula cita a classe de resposta do registro, mas não soletra sua declaração integral.

### É um mapeamento reverso do anterior?

**Não exatamente.**

Os dois mapeamentos declarados são independentes:

```csharp
CreateMap<RequestRegisterExpenseJson, Expense>();
CreateMap<Expense, ResponseRegisterExpenseJson>();
```

Eles envolvem três tipos distintos.

Isso não é equivalente a criar automaticamente um mapa bidirecional entre apenas dois tipos.

---

## 12. O significado de CreateMap<TSource, TDestination>()

Essa é a sintaxe mais importante para compreender a configuração.

```csharp
CreateMap<TSource, TDestination>();
```

### Primeiro argumento genérico: origem

De qual tipo os dados serão lidos.

### Segundo argumento genérico: destino

Para qual tipo o AutoMapper deve produzir a conversão.

### Exemplos da aula

| Origem | Destino | Propósito |
|---|---|---|
| `RequestRegisterExpenseJson` | `Expense` | Transformar dados recebidos em entidade |
| `Expense` | `ResponseRegisterExpenseJson` | Transformar entidade em resposta |

### CreateMap não é Map

```csharp
CreateMap<Expense, ResponseRegisterExpenseJson>();
```

É uma **declaração de configuração**.

Já uma futura chamada semelhante a:

```csharp
var response = _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

solicita **uma conversão concreta em tempo de execução**.

Esse segundo trecho é um exemplo antecipatório da próxima aula, não uma alteração já realizada no use case durante esta gravação.

---

## 13. Reorganizando DependencyInjectionExtension na Application

Depois de configurar o Profile, o professor abre `DependencyInjectionExtension` do projeto Application.

Essa classe já foi criada anteriormente para configurar os use cases no container de DI.

### Como era

Uma versão simplificada do método público tinha o registro diretamente no corpo:

```csharp
public static void AddApplication(this IServiceCollection services)
{
    services.AddScoped<IRegisterExpenseUseCase, RegisterExpenseUseCase>();
}
```

### Qual melhoria o professor faz?

Ele extrai responsabilidades para dois métodos privados e estáticos:

- `AddAutoMapper(IServiceCollection services)`.
- `AddUseCases(IServiceCollection services)`.

### Resultado do método público

```csharp
public static void AddApplication(this IServiceCollection services)
{
    AddAutoMapper(services);
    AddUseCases(services);
}
```

### Por que organizar assim?

O professor espera que mais use cases e configurações sejam adicionados ao projeto.

Separar os grupos de registros torna a classe de composição mais fácil de manter.

### Por que os métodos auxiliares são static?

Porque estão declarados dentro de uma classe de extensão estática.

### Por que os métodos auxiliares não recebem this?

Eles não precisam funcionar como métodos de extensão de `IServiceCollection`. São apenas funções auxiliares privadas.

```csharp
private static void AddUseCases(IServiceCollection services)
```

A palavra-chave `this` permanece no primeiro parâmetro do método de extensão público `AddApplication`.

---

## 14. Registrando AutoMapper com IServiceCollection

Na função auxiliar `AddAutoMapper`, o professor chama o método de extensão de registro fornecido pela biblioteca:

```csharp
services.AddAutoMapper(typeof(AutoMapping));
```

### Qual é a função desse registro?

Informar ao mecanismo de composição onde descobrir as configurações de AutoMapper.

A configuração permite que o container de DI disponibilize serviços como `IMapper` aos componentes consumidores.

### Como fica a função auxiliar?

```csharp
private static void AddAutoMapper(IServiceCollection services)
{
    services.AddAutoMapper(typeof(AutoMapping));
}
```

### O método possui o mesmo nome?

Observe que existem dois métodos diferentes:

**Método privado definido pelo professor:**

```csharp
private static void AddAutoMapper(IServiceCollection services)
```

**Método de extensão oferecido pela biblioteca:**

```csharp
services.AddAutoMapper(...);
```

O primeiro organiza a Application; o segundo registra a configuração do AutoMapper.

### Compatibilidade de versões

A assinatura acima representa **a chamada demonstrada no vídeo**.

A documentação oficial das versões **15 ou posteriores** exige um argumento de configuração como primeiro parâmetro:

```csharp
services.AddAutoMapper(cfg => { }, typeof(AutoMapping));
```

Em projetos que precisam configurar uma licença, o callback pode incluir a chave correspondente, conforme a versão e os termos aplicáveis.

**É importante verificar a versão NuGet instalada antes de copiar diretamente a chamada da aula.**

---

## 15. Entendendo typeof(AutoMapping)

O instrutor destaca a expressão:

```csharp
typeof(AutoMapping)
```

### O que typeof faz em C#?

É um operador que produz um objeto `System.Type` representando um tipo conhecido em tempo de compilação.

Por exemplo:

```csharp
Type type = typeof(AutoMapping);
```

A variável `type` contém uma representação do tipo `AutoMapping`.

### typeof cria uma instância?

Não.

Esta expressão:

```csharp
typeof(AutoMapping)
```

**não é equivalente a**:

```csharp
new AutoMapping()
```

A primeira devolve metadados de tipo. A segunda construiria uma instância da classe.

### Qual é seu papel no AddAutoMapper?

Na integração de DI do AutoMapper, esse tipo serve de **referência para identificar o assembly cujos Profiles serão descobertos**.

Por isso, pode haver outros Profiles no mesmo assembly e eles também serem encontrados.

### O que o professor escolhe?

Ele usa `AutoMapping` como tipo de referência, por ser justamente a classe herdada de `Profile` que acabou de criar.

---

## 16. Código reconstruído da implementação

Esta seção consolida os trechos efetivamente descritos na aula.

### 16.1 Application/AutoMapper/AutoMapping.cs

```csharp
using AutoMapper;
using CashFlow.Domain.Entities;
using CashFlow.Communication.Requests;
using CashFlow.Communication.Responses;

namespace CashFlow.Application.AutoMapper;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        RequestToEntity();
        EntityToResponse();
    }

    private void RequestToEntity()
    {
        CreateMap<RequestRegisterExpenseJson, Expense>();
    }

    private void EntityToResponse()
    {
        CreateMap<Expense, ResponseRegisterExpenseJson>();
    }
}
```

**Nota:** os `using` e namespaces de `Communication` são representações didáticas baseadas nos componentes das aulas anteriores; devem ser ajustados aos nomes exatos do projeto. Os dois `CreateMap` são as configurações apresentadas na transcrição.

### 16.2 Application/DependencyInjectionExtension.cs

**Forma demonstrada pelo professor:**

```csharp
using CashFlow.Application.AutoMapper;
using CashFlow.Application.UseCases.Expenses.Register;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Application;

public static class DependencyInjectionExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        AddAutoMapper(services);
        AddUseCases(services);
    }

    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(AutoMapping));
    }

    private static void AddUseCases(IServiceCollection services)
    {
        services.AddScoped<
            IRegisterExpenseUseCase,
            RegisterExpenseUseCase>();
    }
}
```

O namespace do use case é ilustrativo; o arquivo da solução pode usar uma estrutura ligeiramente diferente.

### 16.3 Forma compatível com AutoMapper 15+

Na implementação acima, o trecho:

```csharp
services.AddAutoMapper(typeof(AutoMapping));
```

pode precisar ser substituído por:

```csharp
services.AddAutoMapper(cfg => { }, typeof(AutoMapping));
```

Essa mudança diz respeito **à versão da biblioteca**, não a uma alteração demonstrada pelo instrutor.

### 16.4 O Program.cs continua igual

A API já chama a extensão pública da Application:

```csharp
builder.Services.AddApplication();
```

Portanto, a configuração do Profile é registrada **por dentro de `AddApplication()`**.

A aula não precisa acrescentar uma nova chamada direta ao AutoMapper em `Program.cs`.

---

## 17. Como o IMapper será utilizado futuramente

O professor termina anunciando que fará a substituição do mapeamento manual na próxima aula.

### O que já está preparado?

- O pacote AutoMapper instalado.
- O Profile com os dois mapeamentos.
- O registro da biblioteca na DI.
- A Application organizada em métodos de configuração.

### O que falta testar?

Uma classe consumidora, como `RegisterExpenseUseCase`, deverá receber um serviço de mapeamento e invocá-lo.

Exemplo **antecipatório**, não implementado nesta aula:

```csharp
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
```

E posteriormente, no método de cadastro:

```csharp
var entity = _mapper.Map<Expense>(request);
```

Após a persistência, poderá ser utilizado o segundo mapa para criar a resposta:

```csharp
var response = _mapper.Map<ResponseRegisterExpenseJson>(entity);
```

**Reforço de fidelidade:** a gravação atual apenas registra os Profiles e a DI; esses trechos de consumo são exemplos de como a configuração poderá ser usada.

---

## 18. Pontos técnicos e diferenças entre versões do AutoMapper

Esta seção contém **complementos técnicos verificados na documentação oficial**, não modificações realizadas pelo professor.

### 18.1 O AutoMapper usa convenções

A biblioteca consegue mapear muitos membros por nome e por convenções, mas sua configuração deve ser validada.

Se as propriedades não são compatíveis, podem ser necessárias regras explícitas.

### 18.2 Múltiplos Profiles são permitidos

Um único Profile não é uma limitação da biblioteca.

O instrutor escolhe um Profile organizado em métodos porque isso é suficiente para o estado atual da Application.

### 18.3 typeof identifica um tipo e seu assembly

Na configuração de DI, o tipo indicado por `typeof(AutoMapping)` é usado como referência para descoberta de Profiles no assembly apropriado.

Não é necessário fazer `new AutoMapping()` no `Program.cs`.

### 18.4 AddAutoMapper mudou entre versões

A documentação oficial indica:

- **AutoMapper 13:** o método `AddAutoMapper` passou a integrar o pacote principal; a integração DI separada foi descontinuada.
- **AutoMapper 15+:** as sobrecargas de `AddAutoMapper` passaram a exigir um callback de configuração como primeiro argumento.

Consequentemente, a sintaxe do professor pode precisar de adaptação se for utilizada uma versão recente.

### 18.5 Licenciamento das versões modernas

A documentação oficial informa que o AutoMapper adotou licenciamento a partir da versão 15.

É necessário consultar os **termos aplicáveis à versão e ao tipo de uso** antes de integrar a dependência em um projeto real.

Isso não altera a lógica conceitual de `Profile`, `CreateMap` e `IMapper`, mas pode impactar a configuração.

### 18.6 Mapeamento entre enums exige atenção semântica

Enums diferentes podem possuir membros com nomes iguais e valores numéricos diferentes.

Um mapeamento aparentemente válido pode produzir um valor cuja interpretação é diferente no Domain.

Testar os membros do enum e, se necessário, configurar conversões explícitas é mais seguro do que presumir equivalência pelo nome da propriedade.

### 18.7 Mapeamento não substitui validação de entrada

AutoMapper não garante, por si só, que uma requisição esteja de acordo com as regras de negócio.

As validações já implementadas na Application continuam necessárias.

### 18.8 Mapear entidade para resposta exige cuidado com dados expostos

A resposta deve conter apenas os campos apropriados para o consumidor.

Em sistemas reais, nomes coincidentes entre propriedades não são razão suficiente para expor automaticamente dados sensíveis do Domain.

### 18.9 Mapeamento manual continua válido

O instrutor prefere a biblioteca para evitar repetição, mas mapeamento manual pode ser adequado em aplicações pequenas ou transformações simples.

AutoMapper representa um equilíbrio entre centralização de regras e dependência/configuração adicional.

### Referências oficiais

- [Dependency Injection — AutoMapper](https://docs.automapper.io/en/latest/Dependency-injection.html)
- [Configuration e Profiles — AutoMapper](https://docs.automapper.io/en/latest/Configuration.html)
- [AutoMapper 13 Upgrade Guide](https://docs.automapper.io/en/latest/13.0-Upgrade-Guide.html)
- [AutoMapper 15 Upgrade Guide](https://docs.automapper.io/en/latest/15.0-Upgrade-Guide.html)
- [License Configuration — AutoMapper](https://docs.automapper.io/en/latest/License-configuration.html)

---

## 19. Testes, validação de configuração e precauções

A gravação termina antes de executar o Profile.

O professor anuncia que a execução prática será apresentada na próxima aula.

### O que ainda não foi demonstrado?

- Receber `IMapper` no construtor do use case.
- Substituir o código manual de criação da entidade.
- Substituir o código manual de criação da resposta.
- Enviar uma requisição no Swagger usando o novo mapeamento.
- Confirmar os valores efetivamente convertidos no debugger.
- Validar toda a configuração de mapeamentos em um teste automatizado.

### Como testar um Profile no futuro?

Uma ferramenta importante do AutoMapper é a validação de configuração, por exemplo:

```csharp
configuration.AssertConfigurationIsValid();
```

O objeto `configuration` representa uma configuração de AutoMapper, como `MapperConfiguration`.

Essa chamada é ilustrativa: o professor **não a utiliza nesta aula**.

### Cuidados especiais com PaymentType

Uma estratégia de teste relevante para este projeto é verificar se cada valor de tipo de pagamento recebido na API é convertido para o membro correto no Domain.

Isso confirma tanto a validade técnica da conversão quanto sua semântica.

### Por que testar os dois sentidos separadamente?

Porque são dois mapeamentos distintos:

1. `RequestRegisterExpenseJson` → `Expense`.
2. `Expense` → `ResponseRegisterExpenseJson`.

O fato de um funcionar não prova que o outro produzirá exatamente a resposta desejada.

---

## 20. Perguntas para revisão

1. Qual é o objetivo central desta aula?
2. Por que o professor cria o Profile dentro da Application?
3. Em qual projeto o pacote NuGet AutoMapper é instalado?
4. Qual pasta é criada dentro da Application?
5. Qual é o nome da classe de configuração?
6. Qual é a classe base de `AutoMapping`?
7. O que representa o tipo `Profile`?
8. Quais dois métodos privados o instrutor cria para organizar os mapeamentos?
9. Por que esses métodos são chamados no construtor?
10. Qual é o primeiro `CreateMap` criado?
11. Qual é a origem e qual é o destino em `CreateMap<RequestRegisterExpenseJson, Expense>()`?
12. Qual é o segundo `CreateMap`?
13. Quais propriedades da requisição e da entidade possuem nomes correspondentes?
14. Por que o tipo de pagamento merece atenção especial?
15. O que pode acontecer quando dois enums utilizam valores numéricos diferentes?
16. `CreateMap` realiza o mapeamento imediatamente?
17. Qual classe da Application é modificada para registrar o AutoMapper na DI?
18. Quais dois métodos auxiliares são criados na extensão?
19. Qual é a função de `AddUseCases`?
20. O que faz `services.AddAutoMapper(...)`?
21. Qual é a função de `typeof(AutoMapping)`?
22. `typeof` cria uma instância do Profile?
23. A API precisa de uma nova chamada no `Program.cs`?
24. Qual diferença importante existe na assinatura de `AddAutoMapper` a partir da versão 15?
25. O professor testa o mapeamento com `IMapper` nesta gravação?
26. Qual etapa está prevista para a aula seguinte?

---

## 21. Resumo e mapa mental

### Principais aprendizados

- A aula introduz a configuração prática do AutoMapper no CashFlow.
- O pacote `AutoMapper` é instalado no projeto **Application**.
- Cria-se a pasta `AutoMapper` e a classe `AutoMapping`.
- `AutoMapping` é pública e herda de `Profile`.
- Seu construtor chama `RequestToEntity()` e `EntityToResponse()`.
- `CreateMap<RequestRegisterExpenseJson, Expense>()` configura o mapeamento de entrada.
- `CreateMap<Expense, ResponseRegisterExpenseJson>()` configura o mapeamento de saída.
- Os tipos de origem e destino são informados nessa ordem.
- Membros de nomes correspondentes são mapeados por convenção quando compatíveis.
- O tipo de pagamento utiliza enums de projetos diferentes e merece verificação da conversão.
- `DependencyInjectionExtension` na Application passa a utilizar métodos privados `AddAutoMapper` e `AddUseCases`.
- O registro da aula usa `services.AddAutoMapper(typeof(AutoMapping))`.
- O operador `typeof` fornece o tipo usado como referência para a descoberta dos Profiles.
- A chamada pública `builder.Services.AddApplication()` continua sendo o ponto de entrada da configuração.
- Em versões recentes do AutoMapper, a assinatura de registro pode exigir callback de configuração e atenção ao licenciamento.
- **A aula termina antes do teste prático de consumo dos mapeamentos.**

### Quadro final

| Elemento | Responsabilidade |
|---|---|
| `AutoMapper` | Biblioteca para mapeamento entre objetos |
| `Profile` | Classe base que reúne configurações de mapeamento |
| `AutoMapping` | Profile concreto criado na Application |
| `RequestToEntity` | Agrupa mapas de entrada para entidade |
| `EntityToResponse` | Agrupa mapas de entidade para resposta |
| `CreateMap<TSource, TDestination>()` | Declara o par origem/destino |
| `typeof(AutoMapping)` | Produz o `System.Type` do Profile, usado como marcador |
| `services.AddAutoMapper(...)` | Registra o AutoMapper na DI |
| `AddUseCases` | Agrupa os registros de casos de uso |
| `AddApplication` | Ponto de extensão público da configuração da Application |
| `IMapper` | Contrato que será usado pelos consumidores para executar mapeamentos |

### Mapa mental

```text
                           AULA 107
                     AUTOMAPPER NO CASHFLOW
                               |
                     CashFlow.Application
                               |
                  +------------+-------------+
                  |                          |
              AutoMapper/              DependencyInjection
              AutoMapping.cs             Extension.cs
                  |                          |
           public class AutoMapping       AddApplication()
                  : Profile                  |
                  |                      +---+---+
                  |                      |       |
             Construtor               AddAuto  AddUse
                  |                   Mapper   Cases
             +----+-----+               |
             |          |               v
       RequestToEntity  EntityToResponse
             |          |          services.AddAutoMapper(
             v          v             typeof(AutoMapping))
  RequestRegister...   Expense              |
           |              |              v
           v              v         Profile descoberto
         Expense      Response...   e DI configurada
             |          |               |
             +----+-----+               |
                  |                     |
                  +----------+----------+
                             |
                             v
                       PRÓXIMA AULA
                             |
                    Consumir IMapper
                    e testar conversões
```

**Conclusão:** a aula configura o AutoMapper no projeto Application, centraliza os mapeamentos em uma classe `AutoMapping : Profile`, declara a conversão de requisição para entidade e de entidade para resposta e integra a configuração ao serviço de injeção de dependência através de `AddApplication()`. A conversão efetiva dentro do use case e o teste no Swagger ficam para a próxima aula.
