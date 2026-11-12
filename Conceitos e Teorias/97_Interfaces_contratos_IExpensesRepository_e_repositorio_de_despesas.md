# Interfaces em C#: contratos, desacoplamento e primeiro repositório do CashFlow

> **Aula 97 — Fundamentos das interfaces e criação de IExpensesRepository no Domain**
>
> Continuação das aulas 92 a 96, dedicadas à injeção de dependência, aos ciclos de vida Singleton, Transient e Scoped e à injeção nos controllers. Nesta aula, o instrutor apresenta **interfaces**, suas vantagens e o primeiro contrato de repositório de despesas do CashFlow.

## Índice

1. Objetivo da aula e contexto
2. O que é uma interface?
3. Métodos e assinaturas
4. Interfaces e classes abstratas
5. Como uma classe implementa uma interface
6. Convenção de nomes com I maiúsculo
7. Características das interfaces apresentadas
8. Primeira vantagem: baixo acoplamento
9. Segunda vantagem: flexibilidade
10. Terceira vantagem: extensibilidade
11. Quarta vantagem: facilidade nos testes unitários
12. Ligação entre interfaces e injeção de dependência
13. Criando a pasta Repositories no Domain
14. Criando IExpensesRepository
15. Por que o contrato pertence ao Domain?
16. O que ainda falta para persistir uma despesa?
17. Distinções técnicas sobre o C# moderno
18. Resumo, revisão e mapa mental

---

## 1. Objetivo da aula e contexto

Depois de apresentar diferentes formas de configurar e receber dependências no ASP.NET Core, o professor introduz o assunto que dará continuidade à organização do projeto: **interfaces**.

A aula possui duas partes:

1. **Teórica:** explicar o conceito de interface, compará-lo a classes abstratas e mostrar seus benefícios para o design de software.
2. **Prática:** voltar ao CashFlow, organizar a camada Domain e criar uma interface que represente a operação de adicionar despesas a um repositório.

O instrutor destaca que interfaces ajudam a desenvolver software mais modular, flexível e fácil de testar.

### Relação com as aulas anteriores

Nas aulas 90 e 91, criamos o `CashFlowDbContext` e testamos a inserção de uma despesa no MySQL por meio do Entity Framework Core.

Para executar esse teste, a Application precisou receber temporariamente uma referência à Infrastructure e criar o contexto com `new`.

A referência temporária foi removida ao final do teste, pois o objetivo arquitetural é evitar que a Application dependa diretamente de uma implementação de banco de dados.

Nas aulas 92 a 96, estudamos como a injeção de dependência permite **receber** colaboradores sem criá-los diretamente.

Agora aprendemos como utilizar **interfaces** para que o consumidor dependa de um **contrato**, em vez de conhecer obrigatoriamente uma classe concreta.

```text
Aula 91
Application instancia CashFlowDbContext diretamente
            |
            v
Referência temporária removida
            |
            v
Aulas 92–96
Injeção de dependência
            |
            v
Aula 97
Interfaces e contratos
            |
            v
Próximas aulas
Implementação concreta do repositório
```

**Escopo:** esta aula termina com a criação de um contrato. A implementação do repositório e a persistência definitiva das despesas ficam para a próxima aula.

---

## 2. O que é uma interface?

O professor começa pela definição frequentemente usada em programação:

> Uma interface é um contrato.

Esse contrato especifica **quais membros são esperados** de um tipo que o implementa.

Na apresentação introdutória da aula, o foco está nos métodos.

### Exemplo conceitual

Suponha uma interface que declara a operação de enviar uma mensagem.

```csharp
public interface INotificador
{
    void Enviar(string mensagem);
}
```

Esse contrato informa que uma implementação precisa disponibilizar uma operação com o nome `Enviar`, recebendo uma string.

A interface **não informa, nesse exemplo**, se a mensagem será enviada por e-mail, SMS ou outro mecanismo.

### O que é definido e o que fica de fora?

| Definido pelo contrato | Decidido pela implementação |
|---|---|
| Nome da operação | Algoritmo executado |
| Parâmetros | Tecnologia utilizada |
| Tipo de retorno | Tratamento interno |
| Membros exigidos | Integrações e infraestrutura |

A interface permite que diferentes implementações forneçam o mesmo conjunto de operações públicas.

### A analogia utilizada pelo professor

O instrutor descreve a interface como uma espécie de **classe especial**, para facilitar a comparação com aquilo que já foi estudado.

Essa é uma analogia didática. Em C#, **interface e classe são categorias distintas de tipos**. Uma interface não é simplesmente uma classe comum com algumas restrições.

### Por que chamamos de contrato?

Porque quem implementa a interface assume o compromisso de oferecer os membros exigidos por ela.

Assim, um código consumidor pode interagir com a abstração sem precisar conhecer os detalhes internos da implementação recebida.

---

## 3. Métodos e assinaturas

O professor diz que, em interfaces, trabalhamos principalmente com **assinaturas de métodos**.

Na explicação da gravação, ele apresenta três informações:

1. Nome do método.
2. Tipo de retorno.
3. Parâmetros, incluindo seus tipos e quantidade.

### Exemplo

```csharp
public interface ICalculadora
{
    decimal Somar(decimal primeiro, decimal segundo);
}
```

### Analisando a declaração

| Elemento | Valor |
|---|---|
| Nome do método | `Somar` |
| Tipo de retorno | `decimal` |
| Primeiro parâmetro | `decimal primeiro` |
| Segundo parâmetro | `decimal segundo` |

Essa declaração comunica quais informações o código consumidor deve fornecer e qual resultado pode receber.

### Declaração sem corpo

Na forma simples apresentada no curso:

```csharp
decimal Somar(decimal primeiro, decimal segundo);
```

O método termina com ponto e vírgula, sem corpo de implementação.

Uma classe que implementa essa interface fornecerá o comportamento da operação.

### Complemento de precisão sobre o termo "assinatura"

O professor utiliza "assinatura" em sentido amplo, incluindo o tipo de retorno.

Na terminologia formal de C#, **o tipo de retorno não faz parte da assinatura usada para distinguir sobrecargas de métodos**. A identidade relevante para a sobrecarga inclui o nome e a lista de parâmetros com as regras específicas da linguagem.

Entretanto, o tipo de retorno **faz parte da declaração do membro e do contrato que uma implementação precisa satisfazer**.

Essa diferença é útil para entrevistas técnicas: o raciocínio didático da aula explica corretamente o que deve ser declarado, mas a definição formal de assinatura tem regras próprias.

---

## 4. Interfaces e classes abstratas

O instrutor destaca uma pergunta comum em entrevistas:

**Qual é a diferença entre uma interface e uma classe abstrata?**

Ele relaciona o tema às aulas anteriores sobre herança.

### Classes abstratas

Uma classe abstrata pode definir membros que as classes derivadas deverão implementar.

Também pode conter métodos com implementação concreta.

```csharp
public abstract class Processador
{
    public abstract void Processar();

    public void RegistrarInicio()
    {
        Console.WriteLine("Processamento iniciado");
    }
}
```

Nesse exemplo:

- `Processar()` é um método abstrato, que exige uma implementação concreta em uma classe derivada concreta.
- `RegistrarInicio()` já possui um corpo de implementação.

### Interface na forma básica do curso

```csharp
public interface IProcessador
{
    void Processar();
}
```

Aqui, a interface declara uma operação sem fornecer implementação.

### Comparação didática da gravação

| Característica | Interface introdutória | Classe abstrata |
|---|---|---|
| Define contrato de métodos | Sim | Pode definir métodos abstratos |
| Pode exigir implementação de membros | Sim | Sim |
| Pode ser instanciada diretamente | Não | Não |
| Pode conter métodos com corpo? | Não, na forma simples mostrada | Sim |
| Relação com classes concretas | Implementação | Herança |

### Atenção: esta não é toda a diferença no C# moderno

O professor afirma que interfaces não podem ter métodos com corpo. **Essa afirmação descreve a forma tradicional e introdutória, mas não vale universalmente para versões modernas do C#.**

Desde C# 8, interfaces podem oferecer **implementações padrão** para determinados membros.

Portanto, não devemos memorizar a frase "interfaces nunca têm implementação" como regra absoluta.

A seção 17 aprofunda as diferenças técnicas que não fazem parte da demonstração principal.

---

## 5. Como uma classe implementa uma interface

O professor relembra a sintaxe da herança em C#.

Quando uma classe deriva de outra, usamos dois pontos:

```csharp
public class ClasseA : ClasseB
{
}
```

Quando uma classe implementa uma interface, também usamos dois pontos:

```csharp
public class NotificadorEmail : INotificador
{
    public void Enviar(string mensagem)
    {
        Console.WriteLine(mensagem);
    }
}
```

### Diferença de significado

**Herança de classe:**

```text
ClasseA : ClasseB
    |
    v
ClasseA deriva de ClasseB
```

**Implementação de interface:**

```text
NotificadorEmail : INotificador
          |
          v
NotificadorEmail implementa o contrato INotificador
```

A classe concreta deve satisfazer os membros exigidos pela interface.

### O que acontece se faltar um membro obrigatório?

O compilador acusa que a classe não implementa o contrato exigido, salvo situações específicas da linguagem, como uma classe abstrata ou membros que já possuam implementação padrão compatível.

### Interfaces e múltiplas implementações

Uma classe pode implementar **várias interfaces**:

```csharp
public class Servico
    : IPrimeiraInterface, ISegundaInterface, ITerceiraInterface
{
    // Implementação dos membros exigidos.
}
```

O código acima ilustra apenas a sintaxe; as interfaces não estão definidas neste exemplo.

O professor destaca essa característica como um dos pontos a memorizar.

**Complemento:** uma classe C# pode herdar diretamente de apenas uma classe base, mas pode implementar múltiplas interfaces.

---

## 6. Convenção de nomes com I maiúsculo

O professor recomenda nomear interfaces usando a letra **I maiúscula** como prefixo.

### Exemplos

```text
INotificador
IUploadFoto
IExpensesRepository
```

### É obrigatório?

Não. Trata-se de uma **convenção de nomenclatura**, não de uma exigência sintática para que o C# reconheça um tipo como interface.

Contudo, seguir o padrão ajuda a identificar rapidamente interfaces no código.

### Por que isso é útil?

Ao encontrar uma variável do tipo `IExpensesRepository`, o desenvolvedor pode reconhecer que está lidando com um contrato, e não diretamente com uma implementação de persistência.

O professor recomenda também utilizar nomes significativos, que indiquem a responsabilidade da abstração.

---

## 7. Características das interfaces apresentadas

O instrutor reúne algumas propriedades essenciais das interfaces.

### 7.1 Membros públicos na forma tradicional

Na forma básica do curso, os métodos declarados em uma interface são públicos e não precisam do modificador `public` explícito.

```csharp
public interface IExemplo
{
    void Executar();
}
```

O professor mostra que é possível omitir a palavra `public` antes de `Executar`.

### 7.2 Interfaces não são instanciadas diretamente

Não podemos construir um objeto de interface usando:

```csharp
// Inválido:
var exemplo = new IExemplo();
```

Uma interface descreve o contrato. O objeto concreto precisa ser de um tipo que ofereça a implementação correspondente.

### 7.3 Uma classe pode implementar várias interfaces

```csharp
public class MinhaClasse : IContratoA, IContratoB
{
    // Implementações necessárias.
}
```

### 7.4 Interfaces não têm construtores de instância

Uma interface não pode ser instanciada diretamente, portanto não possui um construtor de instância para criar seus próprios objetos.

### 7.5 Classes precisam satisfazer o contrato

Uma classe concreta que implementa uma interface precisa fornecer os membros obrigatórios que ainda não tenham uma implementação padrão aplicável.

### 7.6 Interfaces também podem declarar propriedades

O professor menciona que também é possível declarar propriedades, embora prefira concentrar interfaces em métodos.

```csharp
public interface IIdentificavel
{
    long Id { get; }
}
```

Essa declaração exige que uma implementação disponibilize um membro de propriedade compatível.

**Distinção:** o comentário de que propriedades em interfaces são pouco utilizadas reflete a experiência e preferência do instrutor. No C#, propriedades são parte normal da linguagem de interfaces e podem ser bastante úteis em contratos apropriados.

---

## 8. Primeira vantagem: baixo acoplamento

A primeira vantagem apresentada é desenvolver sistemas **fracamente acoplados**.

### O que é acoplamento?

Acoplamento representa a intensidade das dependências entre componentes.

Uma classe fica mais ligada a uma implementação concreta quando precisa construí-la e conhecer seus detalhes.

### Situação sem abstração

```csharp
public class RegistrarDespesa
{
    public void Execute()
    {
        var repositorio = new RepositorioMySql();

        // A lógica depende diretamente dessa implementação.
    }
}
```

O exemplo é conceitual e não corresponde a uma classe já criada no CashFlow.

O caso de uso conhece uma implementação específica de infraestrutura.

### Situação com interface

```csharp
public class RegistrarDespesa
{
    private readonly IExpensesRepository _repository;

    public RegistrarDespesa(IExpensesRepository repository)
    {
        _repository = repository;
    }
}
```

O caso de uso declara que precisa de algo capaz de satisfazer `IExpensesRepository`, sem determinar qual classe concreta realizará a persistência.

### Qual é o benefício?

```text
Regra de negócio
       |
       v
Interface / contrato
       |
       v
Implementação concreta fornecida externamente
```

O código consumidor passa a depender do **comportamento exposto pelo contrato**.

A configuração da aplicação pode selecionar uma implementação adequada.

**Observação:** a utilização de uma interface não elimina automaticamente todo acoplamento. O desenho do contrato, sua localização na arquitetura e a direção das dependências continuam sendo importantes.

---

## 9. Segunda vantagem: flexibilidade

O professor usa um exemplo de **upload de fotografias**.

Imagine que uma regra de negócio precise enviar uma foto para um serviço de armazenamento.

Podemos definir um contrato como:

```csharp
public interface IUploadFoto
{
    void EnviarFoto(string caminhoArquivo);
}
```

O contrato acima é uma reconstrução didática do cenário narrado; não é um código criado no Visual Studio durante esta aula.

### Diferentes implementações

O instrutor descreve três possibilidades:

1. Upload para o **Google Drive**.
2. Upload para o **Dropbox**.
3. Armazenamento em uma **pasta local** do servidor.

Cada alternativa exige detalhes específicos, como APIs, autenticação ou acesso ao sistema de arquivos.

### O papel da interface

Todas as classes podem oferecer o contrato `IUploadFoto`.

```text
                   IUploadFoto
                        |
          +-------------+-------------+
          |             |             |
     Google Drive     Dropbox     Pasta local
          |             |             |
          +-------------+-------------+
                        |
                Mesmo contrato
```

### Por que isso oferece flexibilidade?

A regra de negócio pode trabalhar apenas com `IUploadFoto`.

Uma configuração externa escolhe qual implementação será fornecida, sem exigir que o consumidor conheça os detalhes específicos de cada tecnologia.

### Exemplo de consumidor

```csharp
public class PublicarFoto
{
    private readonly IUploadFoto _uploadFoto;

    public PublicarFoto(IUploadFoto uploadFoto)
    {
        _uploadFoto = uploadFoto;
    }

    public void Execute(string caminho)
    {
        _uploadFoto.EnviarFoto(caminho);
    }
}
```

O mesmo consumidor pode utilizar diferentes implementações desde que todas respeitem o contrato.

### Complemento técnico: escolha da implementação

O instrutor comenta sobre escolher a classe apropriada em tempo de execução.

O mecanismo exato de seleção **não é configurado nesta aula**. A escolha pode depender do registro de DI, configuração, factories ou outra estratégia de composição da aplicação.

Não devemos presumir que o container escolherá automaticamente uma das implementações apenas porque várias classes implementam a mesma interface.

---

## 10. Terceira vantagem: extensibilidade

A terceira vantagem é facilitar a **expansão do sistema**.

O exemplo continua com o upload de fotografias.

Inicialmente, temos implementações para:

- Google Drive;
- Dropbox;
- armazenamento local.

Posteriormente, podemos precisar adicionar outro destino, como:

- um serviço da Microsoft Azure;
- uma solução de armazenamento na AWS.

### O que muda no código?

Em vez de reescrever toda a regra de negócio, podemos adicionar uma nova classe que implemente a interface existente, **desde que o contrato continue adequado**.

```text
IUploadFoto
    |
    +-- UploadGoogleDrive
    |
    +-- UploadDropbox
    |
    +-- UploadLocal
    |
    +-- Novo: UploadAzure
    |
    +-- Novo: UploadAWS
```

A regra de negócio pode continuar dependendo de `IUploadFoto`.

### Qual é a conexão com princípios de design?

Esse cenário se relaciona ao **princípio aberto/fechado (Open/Closed Principle)**: buscar estruturas que permitam adicionar comportamentos por extensão, evitando alterações desnecessárias no código consumidor.

O professor não menciona formalmente esse princípio na gravação. A associação é um complemento conceitual para conectar o exemplo a um tema de arquitetura.

### Cuidado

A nova implementação ainda precisa ser criada, configurada e registrada no mecanismo de DI. A frase "criar outra classe e tudo funcionar" é uma simplificação útil, mas a integração de uma nova tecnologia pode exigir configurações adicionais.

---

## 11. Quarta vantagem: facilidade nos testes unitários

A quarta vantagem é tornar mais simples o isolamento de uma regra de negócio nos **testes unitários**.

O professor faz uma pergunta essencial:

> Se eu quero testar a regra de negócio, preciso realmente gravar dados em um banco de dados?

Em um teste unitário focado naquela regra, **não necessariamente**.

A finalidade é testar o comportamento do componente de maneira isolada, sem depender de serviços externos.

### Sem isolamento

Se o use case constrói diretamente um repositório MySQL, o teste pode ficar dependente de uma instância do banco.

```text
Teste unitário
      |
      v
Regra de negócio
      |
      v
Repositório MySQL real
      |
      v
Banco de dados
```

Isso aumenta a complexidade e pode fazer o resultado do teste depender de infraestrutura que não é o objetivo da unidade testada.

### Com uma interface

Uma regra de negócio recebe o contrato de persistência:

```text
Teste unitário
      |
      v
Regra de negócio
      |
      v
IExpensesRepository
      |
      v
Fake ou mock do repositório
```

A implementação real pode ser substituída por um objeto controlado pelo teste.

### Exemplo ilustrativo de fake

```csharp
public class FakeExpensesRepository : IExpensesRepository
{
    public List<Expense> AddedExpenses { get; } = new();

    public void Add(Expense expense)
    {
        AddedExpenses.Add(expense);
    }
}
```

Esse fake permite verificar, por exemplo, se o componente pediu ao repositório para adicionar determinada despesa.

**Atenção:** `FakeExpensesRepository` é um exemplo didático acrescentado para ilustrar a vantagem citada. O professor não cria essa classe nem implementa um novo teste nesta aula.

### Mock versus fake

O instrutor menciona implementações "fake" e `mocks`.

Em testes, podemos usar diferentes tipos de substitutos:

- **Fake:** implementação simplificada, geralmente funcional, como um repositório em memória.
- **Mock:** substituto utilizado para configurar respostas e/ou verificar interações esperadas.
- **Stub:** substituto que fornece respostas controladas.

Na linguagem cotidiana, os termos podem ser utilizados de maneira mais ampla.

### Ligação com o que já estudamos

Nas aulas anteriores, utilizamos **xUnit**, **Bogus** e **FluentAssertions** em testes de validação.

O uso de interfaces permitirá, em etapas futuras, testar regras que dependem de persistência sem precisar acessar um banco real.

---

## 12. Ligação entre interfaces e injeção de dependência

O professor enfatiza que interfaces e DI funcionam bem em conjunto.

### A classe recebe uma interface

```csharp
public class Servico
{
    private readonly IUploadFoto _uploadFoto;

    public Servico(IUploadFoto uploadFoto)
    {
        _uploadFoto = uploadFoto;
    }
}
```

O construtor recebe um valor cujo **tipo declarado é a interface**.

### O container fornece um objeto concreto

A aplicação pode configurar uma associação entre contrato e implementação.

```csharp
// Exemplo conceitual: tipos ilustrativos
builder.Services.AddScoped<IUploadFoto, UploadGoogleDrive>();
```

O registro acima não foi realizado no vídeo. Ele antecipa o formato comum que permite mapear uma interface para uma implementação concreta.

### O que acontece na resolução?

```text
Servico exige IUploadFoto
           |
           v
Container consulta os registros
           |
           v
Encontra a implementação configurada
           |
           v
Cria ou obtém um objeto concreto
           |
           v
Injeta no construtor do Servico
           |
           v
Servico utiliza a interface
```

### Precisão técnica

Não é correto dizer que o .NET executa `new` em uma interface.

O objeto criado é uma **instância de uma classe concreta** que implementa o contrato.

O consumidor pode guardar a referência usando o tipo da interface.

### Relação com as aulas 93 a 96

Anteriormente, os serviços eram registrados diretamente pelo tipo concreto:

```csharp
builder.Services.AddScoped<MeuServico>();
```

Com interfaces, podemos separar o **tipo solicitado** do **tipo que implementa o serviço**:

```csharp
builder.Services.AddScoped<IMeuServico, MeuServico>();
```

Esse formato ajuda a reduzir o acoplamento do código consumidor às implementações.

---

## 13. Criando a pasta Repositories no Domain

Na parte prática, o professor volta ao **CashFlow** no Visual Studio.

O objetivo é criar a primeira interface real do projeto, relacionada à persistência de despesas.

### Passo a passo da gravação

1. Expandir o projeto **CashFlow.Domain**.
2. Criar uma pasta chamada **Repositories**.
3. Dentro de Repositories, criar uma pasta chamada **Expenses**.
4. Organizar nessa pasta as interfaces relacionadas aos repositórios de despesas.
5. Adicionar um novo item do tipo **Interface**.

### Organização resultante

```text
CashFlow.Domain
├── Entities
│   └── Expense.cs
├── Enums
│   └── PaymentType.cs
└── Repositories
    └── Expenses
        └── IExpensesRepository.cs
```

### Por que o professor coloca a interface no Domain?

A camada Application precisa realizar operações de negócio que envolvem despesas.

No entanto, ela não deve ficar diretamente acoplada ao código de infraestrutura utilizado para acessar o MySQL.

O Domain contém a entidade `Expense` e passa a conter também o **contrato** que descreve o acesso necessário aos dados.

A implementação concreta, associada ao Entity Framework Core e ao banco, poderá ser criada na Infrastructure.

### Qual é a ideia arquitetural?

```text
Application
     |
     v
Domain / IExpensesRepository
     ^
     |
Infrastructure implementa o contrato
     |
     v
Entity Framework Core + MySQL
```

Esse desenho é conceitual. O professor **ainda não implementa a classe concreta** nesta gravação.

---

## 14. Criando IExpensesRepository

O instrutor seleciona a opção **Interface** na janela de criação de novos itens do Visual Studio.

Ele nomeia a interface usando o prefixo `I` e faz referência ao conjunto de despesas no plural.

Para o resumo, adotamos o identificador **`IExpensesRepository`**, compatível com o plural mencionado oralmente.

> **Nota de fidelidade à transcrição:** o professor pronuncia o nome e comenta que vai utilizá-lo no plural, mas a transcrição não permite inspecionar a grafia literal exibida no editor. O código a seguir representa a intenção demonstrada: um contrato de repositório para despesas.

### Modificador de acesso

Depois de criar o arquivo, o professor troca o modificador de acesso de `internal` para `public`.

Isso permitirá que o contrato seja utilizado por outros projetos que tenham referência ao Domain.

### Remoção de usings desnecessários

O professor utiliza o recurso do Visual Studio para remover imports que não estão sendo usados.

### Definindo a operação Add

A interface terá inicialmente **um único método**:

```csharp
void Add(Expense expense);
```

### Entendendo cada componente

| Elemento | Significado |
|---|---|
| `void` | O método não possui valor de retorno |
| `Add` | Nome da operação de adicionar |
| `Expense` | Entidade que será recebida |
| `expense` | Nome do parâmetro |
| `;` | Termina a declaração sem corpo |

### Código completo reconstruído

```csharp
using CashFlow.Domain.Entities;

namespace CashFlow.Domain.Repositories.Expenses;

public interface IExpensesRepository
{
    void Add(Expense expense);
}
```

Esse é o contrato essencial criado na aula.

### Por que receber Expense como parâmetro?

Porque o repositório é voltado à persistência de despesas.

O use case já possui ou construirá uma entidade `Expense` a partir dos dados recebidos na API e precisará passá-la ao componente responsável por armazená-la.

```text
Request de cadastro
       |
       v
Use case
       |
       v
Entidade Expense
       |
       v
IExpensesRepository.Add(expense)
```

### Por que o retorno é void?

O professor informa que o método não precisa devolver nenhum resultado nesse momento.

Assim, a operação é declarada com `void`.

Essa é a assinatura escolhida no curso. Outros projetos podem utilizar assinaturas assíncronas e retornos diferentes conforme suas necessidades, mas **não devemos substituir a declaração mostrada pelo professor**.

### É preciso escrever public antes de Add?

Não, para essa declaração simples de membro de interface.

```csharp
void Add(Expense expense);
```

O professor destaca que o modificador `public` explícito não é necessário e comenta que versões anteriores da linguagem restringiam sua utilização em membros da interface.

**Nuance:** no C# moderno, modificadores de acesso e tipos de membro em interfaces têm regras mais amplas do que na forma tradicional ensinada. Isso é explicado na seção 17.

---

## 15. Por que o contrato pertence ao Domain?

A gravação conecta a interface ao problema arquitetural estudado anteriormente.

### Situação temporária da aula 91

Para verificar se o banco funcionava, o use case instanciou diretamente o contexto:

```csharp
var dbContext = new CashFlowDbContext();
```

Isso exigiu uma referência temporária de Application para Infrastructure.

O professor removeu essa referência ao final porque ela não corresponde à separação arquitetural desejada.

### Nova direção

A Application pode depender de uma interface definida no Domain:

```text
CashFlow.Application
          |
          | conhece
          v
CashFlow.Domain
  IExpensesRepository
          ^
          | implementa
          |
CashFlow.Infrastructure
  Repositório concreto
          |
          v
       MySQL
```

### Qual é a vantagem dessa organização?

A regra de negócio declara **o que precisa fazer** (adicionar uma despesa), enquanto a Infrastructure decide **como executar** essa operação no mecanismo de persistência escolhido.

### Inversão de dependência

Esse desenho se relaciona ao **Dependency Inversion Principle (DIP)**:

- o módulo de aplicação não precisa depender diretamente de uma classe concreta que executa SQL;
- ambos podem se orientar por uma abstração adequada;
- a implementação de infraestrutura depende do contrato que precisa cumprir.

O instrutor relaciona o tema à arquitetura adotada no curso, mas não desenvolve formalmente o DIP nesta gravação. Esta explicação é um complemento de arquitetura.

---

## 16. O que ainda falta para persistir uma despesa?

A aula termina após criar `IExpensesRepository`.

Apesar de já termos um método `Add(Expense expense)`, **nenhum comando SQL é executado por essa declaração isolada**.

### O que a interface faz?

Define uma operação que poderá ser exigida de uma implementação concreta.

### O que a interface não faz?

- não instancia o `CashFlowDbContext`;
- não chama `DbSet<Expense>.Add`;
- não chama `SaveChanges`;
- não abre uma conexão;
- não registra serviços no container de DI;
- não executa nenhuma operação no MySQL por si só.

### Fluxo futuro esperado

```text
Use case de cadastro
        |
        v
IExpensesRepository.Add(Expense)
        |
        v
Classe concreta de repositório
        |
        v
CashFlowDbContext
        |
        v
Entity Framework Core / MySQL
```

O exemplo acima representa a **intenção arquitetural**. O professor anuncia que na próxima aula continuará a implementação para persistir a entidade da forma correta.

### Relação com Add e SaveChanges

Na aula 91, vimos que:

```csharp
dbContext.Expenses.Add(entity);
```

marca uma entidade para inserção, mas não garante persistência imediata.

Também foi necessário chamar:

```csharp
dbContext.SaveChanges();
```

A assinatura `void Add(Expense expense)` criada agora **não informa** se a implementação chamará `SaveChanges` internamente ou se haverá uma etapa separada para efetivar as alterações.

Essa decisão será apresentada posteriormente; não devemos inferi-la apenas pelo nome da interface.

---

## 17. Distinções técnicas sobre o C# moderno

A aula usa uma visão introdutória das interfaces, adequada para aprender a criar contratos simples. Entretanto, certas afirmações precisam ser interpretadas à luz dos recursos atuais da linguagem.

As observações abaixo são **complementos técnicos**. Não descrevem recursos demonstrados pelo professor.

### 17.1 Interfaces podem ter implementações padrão

A partir do **C# 8**, interfaces podem declarar determinados membros com corpo, conhecidos como *default interface implementations*.

```csharp
public interface IExemplo
{
    void Executar();

    void Registrar()
    {
        Console.WriteLine("Registro padrão");
    }
}
```

A forma tradicional, com membros sem corpo, continua sendo plenamente válida e é a utilizada na interface real do CashFlow.

Portanto, a afirmação de que uma interface **nunca pode conter implementação** é tecnicamente incompleta para o C# moderno.

### 17.2 Interfaces podem declarar membros com diferentes acessibilidades

Na forma simples:

```csharp
public interface IExemplo
{
    void Executar();
}
```

o membro é público por padrão.

Entretanto, versões modernas de C# permitem, em condições específicas, membros privados em interfaces, por exemplo auxiliares para uma implementação padrão.

```csharp
public interface IExemplo
{
    void Executar()
    {
        RegistrarInternamente();
    }

    private void RegistrarInternamente()
    {
        Console.WriteLine("Auxiliar");
    }
}
```

Logo, "todo método de interface é obrigatoriamente público e nunca pode ser privado" não é uma regra universal nas versões atuais.

### 17.3 Interface e classe abstrata possuem mais diferenças

A comparação não se limita à presença ou ausência de métodos com corpo.

| Característica | Interface | Classe abstrata |
|---|---|---|
| Pode ser instanciada diretamente | Não | Não |
| Pode declarar contratos obrigatórios | Sim | Sim |
| Pode oferecer comportamento implementado | Sim, em determinados membros e versões modernas | Sim |
| Pode armazenar estado por campos de instância | Não | Sim |
| Pode participar de múltiplas implementações por uma classe | Sim | Não, apenas uma classe base |
| Pode declarar construtores de instância | Não | Sim, para inicialização da parte base da instância |

O exemplo principal do professor continua adequado para ensinar **interfaces simples com assinaturas sem corpo**.

### 17.4 Interfaces não possuem construtores de instância

A classe concreta tem seu construtor. A interface não pode ser instanciada diretamente.

É essa a regra relevante para a aula.

O C# moderno também possui recursos estáticos de interfaces, mas eles não são necessários para compreender o `IExpensesRepository` criado.

### 17.5 Propriedades em interfaces são legítimas

Contratos podem expor propriedades, indexadores e eventos, dependendo do objetivo.

Não é necessário restringir interfaces a métodos apenas por convenção.

### 17.6 O tipo de retorno não integra a assinatura para sobrecarga

Em C#, não podemos diferenciar duas sobrecargas apenas pelo tipo de retorno:

```csharp
// Não são sobrecargas válidas apenas por mudarem o retorno:
// int Calcular();
// string Calcular();
```

A definição formal de assinatura de método tem regras específicas. Para implementar uma interface, entretanto, o tipo de retorno esperado continua sendo parte do contrato do membro.

### 17.7 Interfaces não são uma solução automática para todos os projetos

Interfaces ajudam quando representam abstrações úteis, pontos de substituição e contratos claros.

Criar interfaces para toda classe sem uma razão arquitetural pode introduzir complexidade desnecessária.

No CashFlow, o contrato de repositório possui uma finalidade concreta: permitir que a Application utilize a persistência sem precisar conhecer diretamente a implementação da Infrastructure.

### 17.8 Uma interface não garante uma implementação correta

O compilador verifica a conformidade estrutural do contrato, mas não comprova que o método executa a regra desejada.

Por exemplo, uma classe pode implementar `void Add(Expense expense)` e ainda assim não persistir a entidade corretamente.

Para verificar comportamento, continuam sendo necessários testes e outras validações.

---

## 18. Resumo, revisão e mapa mental

### Principais aprendizados

- Interfaces representam contratos que descrevem operações e membros esperados.
- Na forma introdutória, o professor apresenta métodos declarados sem implementação.
- A declaração de um método descreve nome, parâmetros e retorno esperado.
- Uma classe pode implementar uma interface usando a sintaxe `:`.
- O professor recomenda o prefixo `I` maiúsculo nos nomes de interfaces.
- Uma interface não pode ser instanciada diretamente.
- Uma classe pode implementar mais de uma interface.
- As quatro vantagens apresentadas são **baixo acoplamento, flexibilidade, extensibilidade e facilidade nos testes unitários**.
- O exemplo de upload de fotos mostra diferentes implementações para Google Drive, Dropbox e armazenamento local.
- A possibilidade de adicionar destinos como Azure e AWS ilustra a extensibilidade.
- Fakes e mocks ajudam a testar regras de negócio sem utilizar obrigatoriamente infraestrutura real.
- No CashFlow, a pasta **Repositories/Expenses** é criada dentro de **Domain**.
- A primeira interface de repositório é representada aqui por **IExpensesRepository**.
- Ela é pública e declara a operação **`void Add(Expense expense);`**.
- A interface define o contrato, mas **não implementa a persistência**.
- A classe concreta do repositório e a integração com o fluxo definitivo são assuntos das próximas aulas.
- Interfaces modernas podem oferecer determinadas implementações padrão; a regra de métodos sempre sem corpo é uma simplificação da introdução.

### Perguntas de revisão

1. O que significa dizer que uma interface é um contrato?
2. Quais informações o professor utiliza para explicar a declaração de um método?
3. Qual é a diferença introdutória entre interface e classe abstrata?
4. Por que essa diferença precisa de uma ressalva no C# moderno?
5. Uma interface pode ser instanciada diretamente com `new`?
6. Uma classe pode implementar mais de uma interface?
7. Por que o prefixo `I` é utilizado em nomes de interfaces?
8. Quais são as quatro vantagens listadas pelo professor?
9. Como o contrato `IUploadFoto` possibilita trocar o destino do upload?
10. Como interfaces ajudam a testar uma regra de negócio sem um banco de dados real?
11. Por que a interface de repositório foi criada dentro do Domain?
12. Qual é a finalidade do método `Add(Expense expense)`?
13. Declarar esse método já executa um INSERT no banco?
14. O tipo de retorno integra a assinatura usada para distinguir sobrecargas em C#?
15. Qual é a relação entre interfaces, DI e a separação Application/Infrastructure?

### Quadro final

| Conceito | Definição para esta aula |
|---|---|
| Interface | Contrato de operações e membros |
| Implementação | Classe que satisfaz o contrato |
| Prefixo I | Convenção de nomenclatura |
| DI | Técnica que fornece dependências de fora do consumidor |
| Baixo acoplamento | Menor dependência de detalhes concretos |
| Flexibilidade | Possibilidade de utilizar implementações diferentes |
| Extensibilidade | Facilidade para adicionar novas implementações |
| Mock/Fake | Substituto de dependência em testes |
| IExpensesRepository | Contrato de repositório de despesas do CashFlow |
| Add(Expense) | Operação declarada para receber uma entidade |
| Infrastructure | Camada destinada à futura implementação concreta |

### Mapa mental

```text
                           AULA 97
                         INTERFACES
                             |
                       "Um contrato"
                             |
             +---------------+----------------+
             |               |                |
         Declarações      Implementação     Vantagens
             |               |                |
      Métodos e membros  Classe concreta   +-- Baixo acoplamento
             |               |             +-- Flexibilidade
       Nome, parâmetros  Recebe contrato    +-- Extensibilidade
       e retorno         por referência     +-- Testes unitários
                             |
                        Injeção de DI
                             |
                    Implementação escolhida
                             |
                    +--------+--------+
                    |                 |
                 Uploads          Repositórios
                    |                 |
           Google Drive/Dropbox       |
           Local/Azure/AWS            |
                                      v
                              CashFlow.Domain
                                      |
                                 Repositories
                                      |
                                   Expenses
                                      |
                              IExpensesRepository
                                      |
                          void Add(Expense expense)
                                      |
                                      v
                               PRÓXIMA AULA
                            Implementação concreta
                              e persistência
```

**Conclusão:** esta aula apresenta interfaces como contratos que permitem desacoplar consumidores das implementações concretas, favorecendo flexibilidade, extensão e testes. No CashFlow, criamos o primeiro contrato de persistência na camada Domain, preparando a implementação posterior de um repositório que poderá ser utilizado pela Application sem depender diretamente da Infrastructure.
