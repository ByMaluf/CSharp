# Primeira persistência de uma despesa: Add, SaveChanges e dependência temporária

> **Aula 91 — Testando o CashFlowDbContext com o MySQL**
>
> Continuação da aula 90, em que configuramos o CashFlowDbContext, o DbSet<Expense> e a conexão MySQL. Nesta aula, o instrutor testa o fluxo real de cadastro de uma despesa, demonstra a diferença entre Add e SaveChanges e prepara a refatoração da arquitetura.

## Índice

1. Objetivo da aula
2. Dependência temporária entre Application e Infrastructure
3. Instanciando o contexto após a validação
4. Convertendo Request em Expense
5. Cast entre os enums PaymentType
6. Por que não preencher Id?
7. Add: registrando a entidade no contexto
8. Primeiro teste: HTTP 201 sem persistência
9. SaveChanges: salvando no banco
10. Segundo teste: registro persistido e Id gerado
11. Código reconstruído do teste
12. Removendo a referência temporária
13. Relação com a arquitetura e injeção de dependência
14. Observações técnicas complementares
15. Resumo e mapa mental

---

## 1. Objetivo da aula

Nas aulas anteriores, o CashFlow ganhou uma entidade Expense no Domain, um banco MySQL com a tabela expenses e um contexto do Entity Framework Core localizado na Infrastructure.

O professor deseja verificar se a configuração funciona na prática: **receber uma requisição HTTP de cadastro e efetivamente inserir a despesa no banco de dados**.

Para isso, ele realiza um teste temporário dentro do use case de registro de despesas.

O vídeo mostra:

- a adição de uma referência temporária entre projetos;
- a conversão dos dados recebidos pela Request em uma entidade Expense;
- o cast entre os enums de Communication e Domain;
- o uso de Add e SaveChanges;
- dois testes pelo Swagger, acompanhados pelo debugger;
- a verificação do registro no MySQL Workbench;
- a remoção da referência temporária.

**Escopo:** o objetivo é comprovar a persistência. A organização arquitetural definitiva será implementada posteriormente.

## 2. Dependência temporária entre Application e Infrastructure

O instrutor começa avisando que fará uma alteração arquitetural propositalmente inadequada para o desenho desejado do projeto.

Ele fará a camada Application referenciar diretamente a Infrastructure para conseguir instanciar o contexto.

### Situação temporária

```text
Application
     |
     | referência direta temporária
     v
Infrastructure
     |
     v
CashFlowDbContext
     |
     v
MySQL
```

A Application contém o use case de registro. A Infrastructure contém o contexto de acesso aos dados.

Com essa referência, o use case passa a conhecer diretamente uma classe de infraestrutura.

### Procedimento no Visual Studio

1. Localizar o projeto Application.
2. Clicar com o botão direito em Dependencies.
3. Selecionar a opção para adicionar uma referência de projeto.
4. Selecionar Infrastructure.
5. Confirmar a referência.

**Nota sobre a transcrição:** em um momento inicial, a fala menciona "projeto de comunicação", mas o objetivo anunciado, a localização do use case e a remoção final identificam a dependência como **Application -> Infrastructure**. O resumo acompanha esse fluxo, sem sugerir adicionar a referência à Communication.

### Por que o instrutor faz isso?

Para conseguir testar rapidamente o contexto criado na aula anterior.

Ele deixa claro que essa referência não será mantida. Nas próximas aulas, pretende refatorar a solução utilizando injeção de dependência e a abordagem arquitetural discutida no curso.

**Importante:** não confundir a implementação de teste demonstrada com a arquitetura definitiva do CashFlow.

## 3. Instanciando o contexto após a validação

Dentro do use case de registro de despesas, o professor mantém a validação como primeira etapa.

A lógica é:

```text
Request recebida
       |
       v
Validar
       |
       +-- Inválida -> exceção
       |
       v
Válida -> criar contexto e entidade
```

A validação já foi estudada nas aulas de FluentValidation e testes unitários.

Após a validação, o instrutor cria uma instância do contexto:

```csharp
var dbContext = new CashFlowDbContext();
```

A classe CashFlowDbContext foi criada na aula 90, dentro de Infrastructure/DataAccess, com:

- herança de DbContext;
- propriedade Expenses do tipo DbSet<Expense>;
- configuração do provider MySQL em OnConfiguring;
- connection string para o banco CashFlowDB.

A instanciação direta é possível porque a referência temporária permite que Application acesse a classe.

**Complemento técnico:** instanciar o contexto não significa, por si só, que a conexão foi aberta ou que um comando SQL já foi enviado. O teste efetivo acontece quando uma operação precisa acessar o banco.

## 4. Convertendo Request em Expense

O professor cria uma nova instância da entidade Expense e utiliza um inicializador de objeto para preencher suas propriedades.

```csharp
var entity = new Expense
{
    Amount = request.Amount,
    Date = request.Date,
    Description = request.Description,
    Title = request.Title,
    PaymentType = (CashFlow.Domain.Enums.PaymentType)
        request.PaymentType
};
```

O código é uma reconstrução do procedimento narrado. O nome entity é utilizado neste material para representar a variável da entidade.

### Correspondência entre os tipos

| Request | Entidade Expense |
|---|---|
| request.Amount | entity.Amount |
| request.Date | entity.Date |
| request.Description | entity.Description |
| request.Title | entity.Title |
| request.PaymentType | entity.PaymentType, com conversão |
| Não contém Id para cadastro | entity.Id, gerado na persistência |

A classe Expense é a entidade criada no Domain na aula 87.

### Por que existe esse mapeamento?

A Request e a entidade possuem responsabilidades diferentes.

A Request representa o contrato de entrada da API. A Expense representa a despesa utilizada internamente pela aplicação e pelo mecanismo de persistência.

```text
JSON da requisição
       |
       v
RequestRegisterExpenseJson
       |
       v
Validação
       |
       v
Mapeamento manual
       |
       v
Domain.Entities.Expense
```

Nesta aula, o mapeamento é feito manualmente, atribuindo os valores propriedade por propriedade.

## 5. Cast entre os enums PaymentType

Ao tentar atribuir diretamente o tipo de pagamento recebido, o instrutor encontra um erro de conversão.

A tentativa inicial seria:

```csharp
// Não compila neste cenário: os enums são tipos diferentes.
PaymentType = request.PaymentType
```

### Por que ocorre o erro?

Na aula 87, criamos um enum PaymentType no Domain copiando a definição existente em Communication.

Assim, existem dois tipos diferentes:

```text
CashFlow.Communication.Enums.PaymentType

CashFlow.Domain.Enums.PaymentType
```

Apesar de terem o mesmo nome e os mesmos membros, são declarações distintas para o compilador.

### Solução demonstrada: cast explícito

```csharp
PaymentType = (CashFlow.Domain.Enums.PaymentType)
    request.PaymentType
```

O professor utiliza o cast porque sabe que os valores numéricos dos dois enums correspondem: a definição foi copiada de Communication para Domain.

### Representação

```text
Communication.PaymentType
             |
             | cast explícito
             v
Domain.PaymentType
             |
             v
Expense.PaymentType
```

### Complemento técnico

O cast converte a representação numérica entre os tipos, mas não verifica se o número convertido corresponde a um membro definido no enum de destino.

O validator da Request já verifica o tipo de pagamento com IsInEnum(). Isso ajuda a rejeitar entradas inválidas, mas a equivalência entre os dois enums ainda precisa ser preservada.

Se as definições passarem a usar números diferentes para o mesmo significado, um cast pode produzir uma associação incorreta. Uma conversão explícita por membros pode ser mais adequada em modelos que evoluem separadamente.

A aula não altera o enum nem introduz uma ferramenta de mapeamento automático.

## 6. Por que não preencher Id?

A entidade Expense possui a propriedade:

```csharp
public long Id { get; set; }
```

Entretanto, o professor não atribui um Id durante a criação do objeto.

Na aula 85, a coluna Id foi configurada como:

```sql
Id BIGINT NOT NULL AUTO_INCREMENT
```

Portanto, no cenário de cadastro demonstrado, o banco será responsável por gerar o identificador.

### Valor inicial

Como long é um tipo inteiro, uma propriedade Id sem atribuição explícita começa com 0.

O professor observa esse valor no debugger antes da persistência.

```text
new Expense { ... }
        |
        v
Id = 0 inicialmente
        |
        v
Persistência no banco
        |
        v
Id gerado e recuperado pelo EF Core
```

**Atenção:** o valor 0 inicial não significa que a despesa será armazenada com Id 0. A configuração de geração de chave e o AUTO_INCREMENT permitem que um novo identificador seja atribuído durante a inserção.

## 7. Add: registrando a entidade no contexto

O professor adiciona a entidade à propriedade Expenses:

```csharp
dbContext.Expenses.Add(entity);
```

### O que é Expenses?

Expenses é a propriedade DbSet<Expense> criada no CashFlowDbContext.

Ela oferece operações sobre o conjunto de entidades de despesas.

### O que Add faz?

Na explicação do instrutor, Add prepara o Entity Framework para inserir a entidade.

Em termos técnicos, Add registra a entidade no contexto e a marca como **Added**, de modo que ela possa ser persistida quando as alterações forem salvas.

```text
Expense criada
      |
      v
Expenses.Add(entity)
      |
      v
Entidade rastreada pelo contexto
Estado Added
      |
      v
Inserção pendente
```

**Ponto-chave da aula:** Add não executa, por si só, o INSERT da despesa no banco.

### Precisão da analogia

O professor diz que Add "prepara uma query". Essa é uma forma didática de explicar o resultado esperado.

Mais precisamente, Add registra a mudança que deverá ser persistida. A geração e execução dos comandos SQL necessários à inserção ocorrem no processo de SaveChanges, considerando o modelo, o estado das entidades e o provider.

## 8. Primeiro teste: HTTP 201 sem persistência

O instrutor coloca um breakpoint e executa a aplicação em modo de depuração.

### Dados utilizados no Swagger

A requisição contém valores de exemplo:

| Campo | Valor narrado |
|---|---|
| Title | Macbook |
| Description | Texto demonstrativo |
| Date | 10 de novembro de 2023 |
| Amount | 3000 |
| PaymentType | 0 |

A descrição exata e o horário completo da data não são informados de forma literal na transcrição.

### Fluxo da depuração

1. Executar a API com F5.
2. Abrir o Swagger.
3. Selecionar a operação de cadastro de despesas.
4. Preencher os dados.
5. Executar a requisição.
6. Acompanhar a execução no breakpoint.
7. Verificar que a validação é bem-sucedida.
8. Criar o contexto.
9. Criar a entidade e observar Id igual a 0.
10. Executar Add.
11. Continuar a execução.

### Resultado HTTP

A API retorna **201 Created**.

À primeira vista, parece que o cadastro foi concluído.

Entretanto, o professor abre o MySQL Workbench e executa a consulta:

```sql
SELECT *
FROM cashflowdb.expenses;
```

A despesa Macbook não aparece no resultado.

### Por que a despesa não apareceu?

Porque o código apenas chamou Add.

```text
Request válida
       |
       v
Add(entity)
       |
       v
Resposta HTTP 201
       |
       v
Banco ainda sem a nova despesa
```

O Entity Framework ainda não recebeu a instrução para persistir as alterações pendentes.

### Lição importante

O status HTTP 201 é uma resposta produzida pelo código da API. Ele não constitui, sozinho, prova de que uma operação foi persistida.

O instrutor provoca essa situação de propósito para demonstrar a diferença entre registrar uma alteração no contexto e salvar a alteração no banco.

## 9. SaveChanges: salvando no banco

Para corrigir o fluxo de persistência do teste, o professor adiciona:

```csharp
dbContext.SaveChanges();
```

O código passa a ter as duas operações:

```csharp
dbContext.Expenses.Add(entity);
dbContext.SaveChanges();
```

### Diferença fundamental

| Método | Responsabilidade |
|---|---|
| Add | Marca a entidade para inserção |
| SaveChanges | Persiste as alterações pendentes do contexto |

### O que ocorre conceitualmente?

```text
Expense
   |
   v
Add(entity)
   |
   v
Change tracker: Added
   |
   v
SaveChanges()
   |
   v
EF Core + provider MySQL
   |
   v
INSERT no banco
   |
   v
Id gerado pelo AUTO_INCREMENT
   |
   v
Entidade atualizada com o Id
```

O professor não precisa escrever manualmente a instrução INSERT no use case.

O framework utiliza o mapeamento configurado para gerar os comandos adequados.

### Complemento técnico

SaveChanges processa as alterações pendentes rastreadas pelo contexto, não apenas a última entidade adicionada.

Se o contexto contiver outras inclusões, modificações ou exclusões, essas operações também podem ser persistidas nessa chamada.

A aula trabalha somente com uma nova despesa.

## 10. Segundo teste: registro persistido e Id gerado

O instrutor executa a API novamente e repete o cadastro de Macbook pelo Swagger.

### Passo a passo observado

1. A requisição chega ao use case.
2. A validação passa sem gerar exceção.
3. Uma nova instância de CashFlowDbContext é criada.
4. Uma nova Expense é preenchida com os dados recebidos.
5. Add registra a entidade para inserção.
6. Antes de SaveChanges, o professor observa Id igual a 0.
7. O debugger executa SaveChanges.
8. Ao expandir a entidade novamente, o professor observa Id igual a 4.
9. A API retorna ao Swagger.
10. O professor consulta a tabela pelo Workbench.

### Resultado observado

A nova despesa aparece no banco com **Id 4**, título Macbook, descrição e valor 3000.

| Campo | Resultado observado |
|---|---|
| Id | 4 |
| Title | Macbook |
| Amount | 3000 |
| PaymentType | 0 |
| Description | Valor recebido na requisição |
| Date | Data fornecida no teste |

O Id 4 é coerente com o histórico do exemplo da aula 86, em que foram criados os registros 1, 2 e 3 e o registro de Id 2 foi excluído.

**Importante:** 4 é o resultado do ambiente demonstrado, não um Id que deve ser definido manualmente ou esperado em qualquer instalação.

### Por que a propriedade Id mudou?

O banco gerou a chave durante a inserção. O Entity Framework Core recuperou o identificador gerado e atualizou a entidade rastreada.

```text
Antes de SaveChanges: Id = 0
Depois de SaveChanges: Id = 4
```

Esse comportamento confirma, no cenário demonstrado, que a configuração de persistência está funcionando.

## 11. Código reconstruído do teste

O trecho abaixo reúne a lógica temporária narrada no vídeo.

Não representa o arquivo inteiro do use case, pois a transcrição não informa sua assinatura completa nem o código de retorno já existente.

```csharp
// Trecho temporário dentro do use case de cadastro.
// A validação existente é executada antes destas operações.

var dbContext = new CashFlowDbContext();

var entity = new Expense
{
    Amount = request.Amount,
    Date = request.Date,
    Description = request.Description,
    Title = request.Title,
    PaymentType = (CashFlow.Domain.Enums.PaymentType)
        request.PaymentType
};

dbContext.Expenses.Add(entity);
dbContext.SaveChanges();
```

Os namespaces necessários para os tipos são:

```csharp
using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.DataAccess;
```

A referência a Infrastructure é necessária para a instanciação direta demonstrada, mas será removida ao final.

### Exemplo de JSON didático

```json
{
  "title": "Macbook",
  "description": "Descrição de exemplo",
  "date": "2023-11-10T00:00:00",
  "amount": 3000,
  "paymentType": 0
}
```

A data à meia-noite e o texto da descrição são valores ilustrativos, pois esses detalhes não foram especificados integralmente na transcrição.

### Verificação pelo banco

```sql
SELECT *
FROM cashflowdb.expenses;
```

No teste demonstrado, a consulta permite confirmar que o registro realmente foi criado.

## 12. Removendo a referência temporária

Depois de comprovar que a persistência funciona, o professor retorna à organização arquitetural do projeto.

### Procedimento apresentado

1. Expandir o projeto Application.
2. Localizar a referência para Infrastructure nas dependências.
3. Clicar com o botão direito.
4. Selecionar Remove.
5. Voltar ao use case e remover as instruções que acessam diretamente o contexto.

A remoção da referência faz com que o código que usa CashFlowDbContext deixe de compilar naquela camada.

O professor apaga esse acesso direto.

### O que permanece?

A criação da entidade Expense permanece, pois está de acordo com a responsabilidade do use case e utiliza o modelo do Domain.

```text
Application
    |
    v
Domain.Entities.Expense

Acesso direto ao CashFlowDbContext:
REMOVIDO
```

A aula não implementa ainda a substituição definitiva para o acesso ao banco.

**Atenção:** a remoção da referência e do código temporário não apaga a despesa Macbook que foi persistida no MySQL. A alteração dos arquivos do projeto e o estado do banco são coisas distintas.

## 13. Relação com a arquitetura e injeção de dependência

A demonstração mostrou que o banco e o contexto funcionam.

Entretanto, a solução temporária acoplou a Application diretamente a uma implementação concreta de infraestrutura.

O professor anuncia que as próximas aulas ensinarão **injeção de dependência**, juntamente com a reorganização do código para respeitar a abordagem arquitetural adotada no curso.

### Diferença entre teste temporário e objetivo futuro

```text
TESTE TEMPORÁRIO

Application
    |
    v
Infrastructure concreta
    |
    v
MySQL


OBJETIVO ARQUITETURAL (conceitual)

Application
    |
    v
Abstração/contrato de persistência
    |
    v
Implementação fornecida pela Infrastructure
    |
    v
MySQL
```

O segundo desenho é uma antecipação conceitual, não uma estrutura implementada nesta aula.

**Complemento técnico:** injeção de dependência e inversão de dependência são conceitos relacionados, mas distintos. Injetar uma classe concreta não elimina automaticamente o acoplamento arquitetural. A refatoração deverá definir como os contratos e as implementações se relacionam.

O instrutor menciona Domain-Driven Design (DDD) como parte da abordagem que pretende explicar.

## 14. Observações técnicas complementares

### 14.1 O teste é intencionalmente provisório

O uso de new CashFlowDbContext() dentro do use case é realizado apenas para validar o fluxo. O professor avisa que a referência será removida.

Não se deve manter esse trecho como se fosse a solução arquitetural final.

### 14.2 Ciclo de vida do contexto

O DbContext possui recursos que precisam ser gerenciados corretamente. Quando instanciado manualmente, pode ser descartado com using ou outro mecanismo apropriado de gerenciamento de ciclo de vida.

Em aplicações ASP.NET Core, é comum configurar o contexto por injeção de dependência. O vídeo não implementa essa configuração.

### 14.3 Add e Change Tracking

Change tracking é o mecanismo usado pelo EF Core para acompanhar o estado das entidades.

Uma nova entidade adicionada fica normalmente no estado Added. Após uma chamada bem-sucedida de SaveChanges, com o comportamento padrão, seu estado passa a refletir que a alteração foi persistida.

A aula demonstra o efeito prático sem aprofundar os detalhes de estados.

### 14.4 SaveChanges pode falhar

Credenciais incorretas, indisponibilidade do servidor, restrições do banco ou incompatibilidades de mapeamento podem causar falhas.

A resposta de sucesso da API deve refletir o resultado real da operação. O primeiro teste ilustra por que responder 201 sem confirmar a persistência pode induzir o cliente a acreditar que o recurso foi criado.

### 14.5 Validação e persistência

A validação existente é mantida antes da tentativa de salvar. Isso evita iniciar o fluxo de persistência com uma Request já considerada inválida pelas regras do use case.

### 14.6 Operações síncronas e assíncronas

A aula utiliza SaveChanges(), de forma síncrona. O EF Core também oferece SaveChangesAsync(), que pode ser adequado ao fluxo assíncrono de aplicações web.

O exemplo principal preserva o método efetivamente demonstrado.

## 15. Resumo e mapa mental

### Principais aprendizados

- A aula comprova que o CashFlowDbContext consegue persistir uma despesa no MySQL.
- Application recebe uma referência temporária para Infrastructure.
- A validação continua sendo executada antes da criação da entidade.
- A Request é convertida manualmente em Expense.
- Os enums PaymentType de Communication e Domain são tipos distintos e exigem conversão explícita.
- O Id não é preenchido porque será gerado pelo banco.
- Add marca a entidade para inserção, mas não salva a despesa por si só.
- O primeiro teste retorna HTTP 201, porém não cria um registro.
- SaveChanges persiste as alterações pendentes.
- No segundo teste, o Macbook é salvo com Id 4.
- O EF Core recupera a chave gerada e atualiza o objeto.
- A consulta pelo Workbench confirma a persistência.
- A referência direta Application -> Infrastructure e o código de acesso ao contexto são removidos ao final.
- A próxima etapa será estudar injeção de dependência e refatorar o acesso aos dados.

### Perguntas de revisão

1. Por que o instrutor adiciona uma referência temporária para Infrastructure?
2. Por que a validação acontece antes da criação e persistência da entidade?
3. Por que os dois enums PaymentType não podem ser atribuídos diretamente?
4. Qual é a diferença entre Add e SaveChanges?
5. Por que a API retornou 201 no primeiro teste sem inserir uma despesa?
6. Por que Id começa com 0 e depois passa a ter o valor 4?
7. Como o professor confirma que a despesa realmente foi persistida?
8. Por que a dependência temporária é removida ao final?
9. A remoção do código apaga automaticamente o registro do banco?

### Mapa mental

```text
                      AULA 91
                         |
                  Request de despesa
                         |
                      Validate
                         |
                  Expense criada
                         |
        +----------------+----------------+
        |                                 |
   Mapeamento manual                 Cast de enum
        |                                 |
        +----------------+----------------+
                         |
                   DbContext
                         |
                   Expenses.Add
                         |
               Entidade marcada Added
                         |
             +-----------+-----------+
             |                       |
      Sem SaveChanges          Com SaveChanges
             |                       |
       HTTP 201 sem             INSERT no MySQL
       persistência                  |
                                     v
                                Id gerado: 4
                                     |
                                Workbench confirma
                                     |
                          Remover dependência temporária
                                     |
                            Injeção de dependência
```

**Conclusão:** o primeiro teste de persistência do CashFlow demonstra que o EF Core trabalha com alterações rastreadas e que Add não substitui SaveChanges. O contexto é validado com uma inserção real no MySQL, e a implementação temporária é removida para preparar a organização definitiva da arquitetura.
