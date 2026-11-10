# Criando o CashFlowDbContext: DbSet, OnConfiguring e conexão com MySQL

> **Aula 90 — Configurando o contexto do Entity Framework Core**
>
> Continuação das aulas 87 e 88, em que criamos a entidade Expense no Domain e instalamos o Entity Framework Core e o provider Pomelo na Infrastructure. Agora vamos configurar o contexto que permitirá trabalhar com a tabela de despesas.

## Índice

1. Objetivo da aula
2. O que é DbContext?
3. Criando a pasta DataAccess
4. Herança de DbContext
5. DbSet de despesas
6. Configuração com OnConfiguring
7. Configurando UseMySql
8. Identificando a versão do servidor
9. Construindo a connection string
10. Código completo do contexto
11. Como o contexto será utilizado
12. Relação com as camadas
13. Observações técnicas complementares
14. Resumo e mapa mental

---

## 1. Objetivo da aula

Até aqui, o CashFlow já possui a estrutura necessária para iniciar a persistência de dados:

- MySQL Server instalado e configurado.
- Banco CashFlowDB e tabela expenses criados.
- Entidade Expense na camada Domain.
- Pacotes Microsoft.EntityFrameworkCore e Pomelo.EntityFrameworkCore.MySql instalados na Infrastructure.

Nesta aula, o professor cria uma classe chamada **CashFlowDbContext**, responsável por configurar o contexto de acesso ao banco de dados.

A ideia é que a API trabalhe com objetos C#, enquanto o Entity Framework Core, em conjunto com seu provider, realiza a comunicação com o banco.

**Escopo:** a aula cria e configura o contexto. Ainda não insere despesas pela API, não testa a conexão em execução e não implementa repositórios ou migrations. O teste da configuração é anunciado para a próxima aula.

## 2. O que é DbContext?

O professor utiliza novamente a analogia do Entity Framework como um tradutor: passamos entidades e operações em C#, e a ferramenta traduz o trabalho necessário para o banco de dados.

No EF Core, o **DbContext** representa uma sessão de trabalho com o banco e reúne funcionalidades necessárias ao acesso aos dados.

Ele permite trabalhar com conjuntos de entidades e configurações do modelo.

```text
Aplicação C#
     |
     v
CashFlowDbContext
     |
     v
Entity Framework Core + Pomelo
     |
     v
MySQL Server
     |
     v
Tabela expenses
```

### Qual é a finalidade no CashFlow?

Por enquanto, o banco possui apenas a tabela de despesas. O professor pretende utilizar o contexto para realizar operações como:

| Operação | Intenção |
|---|---|
| Inserir | Persistir uma nova despesa |
| Recuperar | Consultar despesas existentes |
| Atualizar | Alterar os dados de uma despesa |
| Excluir | Remover uma despesa |

O contexto será utilizado nessas operações nas próximas aulas.

**Complemento técnico:** DbContext não é somente um tradutor de SQL. Ele também participa da configuração do modelo, execução de consultas, rastreamento de alterações e persistência. A gravação concentra-se na preparação básica do contexto.

## 3. Criando a pasta DataAccess

O professor organiza o código de acesso ao banco dentro da Infrastructure.

### Passos no Visual Studio

1. Expandir o projeto CashFlow.Infrastructure.
2. Criar uma pasta chamada **DataAccess**.
3. Adicionar uma classe dentro dessa pasta.
4. Nomear a classe CashFlowDbContext.
5. Remover os using desnecessários.
6. Alterar temporariamente o modificador de acesso de internal para public.

A organização resultante é:

```text
CashFlow.Infrastructure
└── DataAccess
    └── CashFlowDbContext.cs

CashFlow.Domain
├── Entities
│   └── Expense.cs
└── Enums
    └── PaymentType.cs
```

**Nota:** o nome CashFlowDbContext e a capitalização foram normalizados conforme a convenção usada neste material. A transcrição oral não permite verificar literalmente cada caractere do nome exibido no editor.

O instrutor explica que prefere concentrar o código relacionado ao banco nessa pasta.

## 4. Herança de DbContext

Criar uma classe comum não é suficiente para transformá-la em um contexto do EF Core.

Para isso, a classe precisa herdar de DbContext.

```csharp
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

public class CashFlowDbContext : DbContext
{
}
```

### Entendendo a declaração

- public class CashFlowDbContext: declara a classe que estamos criando.
- : DbContext: indica que a classe deriva de DbContext.
- using Microsoft.EntityFrameworkCore: disponibiliza o namespace da classe base.

A classe DbContext pertence ao Entity Framework Core, cujo pacote foi instalado na aula 88.

```text
DbContext
Classe base do Entity Framework Core
             |
             | herança
             v
CashFlowDbContext
Contexto específico do projeto
```

### Por que utilizar herança?

Porque desejamos aproveitar as funcionalidades do EF Core e configurar o contexto para as necessidades do CashFlow.

**Atenção:** o professor torna a classe public por enquanto. O vídeo indica que essa decisão será explicada posteriormente; não há alteração de visibilidade adicional nesta gravação.

## 5. DbSet de despesas

O primeiro componente adicionado ao contexto é uma propriedade do tipo **DbSet<Expense>**.

```csharp
using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

public class CashFlowDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }
}
```

### O que significa DbSet<Expense>?

DbSet é um tipo genérico do Entity Framework Core.

A entidade informada entre os sinais de menor e maior determina qual tipo de objeto será representado pelo conjunto.

```text
DbSet<Expense>
   |       |
   |       +-- Tipo de entidade
   +---------- Conjunto de entidades
```

No nosso caso, Expense é a entidade criada na aula 87, dentro do Domain.

### Nome da propriedade Expenses

O instrutor utiliza Expenses no plural porque o conjunto representa várias despesas e acompanha o nome da tabela criada na aula 85.

| Elemento | Nome |
|---|---|
| Entidade C# | Expense |
| Propriedade do contexto | Expenses |
| Tabela no MySQL | expenses |

O professor afirma que essa propriedade dará acesso à tabela de despesas.

### Nuance técnica sobre mapeamento

Por convenção, o EF Core pode utilizar o nome da propriedade DbSet para determinar o nome da tabela associada à entidade. Contudo, é possível configurar um nome diferente explicitamente.

Portanto, a propriedade não precisa obrigatoriamente ter o mesmo nome da tabela em todos os projetos.

No Windows da gravação, o servidor MySQL trata nomes de tabelas conforme a configuração de capitalização estudada na aula 85. A diferença entre Expenses no código e expenses na tabela pode ser relevante em outras plataformas. Um mapeamento explícito pode eliminar essa ambiguidade quando necessário.

**Escopo:** o professor não configura ToTable nem outras regras explícitas de mapeamento neste vídeo.

### DbSet não é uma lista carregada inteira na memória

O conjunto permite expressar consultas e operações sobre entidades. Declarar a propriedade não significa carregar automaticamente todas as despesas do banco.

Essa distinção é um complemento técnico para compreender a abstração.

## 6. Configuração com OnConfiguring

Depois de declarar Expenses, ainda precisamos informar ao contexto qual banco utilizar.

O professor faz isso sobrescrevendo o método **OnConfiguring**.

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
{
}
```

### Entendendo o override

A classe DbContext oferece métodos que podem ser sobrescritos para personalizar seu comportamento.

O modificador override indica que estamos fornecendo uma implementação para um método herdado.

### Parâmetro optionsBuilder

O DbContextOptionsBuilder permite configurar opções do contexto, incluindo o provider de banco de dados.

```text
OnConfiguring
       |
       v
DbContextOptionsBuilder
       |
       v
Configurar provider e conexão
```

O instrutor remove a implementação gerada pelo editor e coloca sua configuração dentro do método.

**Observação:** nesta aula, a configuração é realizada diretamente em OnConfiguring. Existem outras estratégias, como configurar DbContextOptions externamente e utilizar injeção de dependência, mas elas não são implementadas na gravação.

## 7. Configurando UseMySql

Dentro de OnConfiguring, o professor utiliza:

```csharp
optionsBuilder.UseMySql(connectionString, serverVersion);
```

O método UseMySql é disponibilizado pelo provider Pomelo.EntityFrameworkCore.MySql instalado anteriormente.

### Parâmetros apresentados

| Parâmetro | Finalidade |
|---|---|
| connectionString | Informar os parâmetros necessários para acessar o banco |
| serverVersion | Informar a versão do servidor MySQL utilizado |

```text
UseMySql
   |
   +-- Connection string
   |
   +-- Versão do servidor
```

O provider precisa conhecer a tecnologia e as capacidades do servidor para gerar operações apropriadas ao ambiente.

### Framework base versus provider

O pacote Microsoft.EntityFrameworkCore fornece as abstrações e funcionalidades gerais do contexto.

Já Pomelo.EntityFrameworkCore.MySql fornece a integração específica com MySQL/MariaDB, incluindo o método UseMySql.

**Importante:** outros providers possuem métodos e parâmetros próprios. A gravação destaca que a configuração de SQL Server seria diferente.

## 8. Identificando a versão do servidor

O segundo parâmetro de UseMySql precisa representar a versão do MySQL.

O instrutor consulta essa informação no Workbench.

### Passo a passo

1. Abrir o MySQL Workbench conectado ao servidor.
2. Acessar o menu **Server**.
3. Selecionar **Server Status**.
4. Localizar a versão do servidor.

A versão exibida na aula é **8.0.35**, a mesma escolhida durante a instalação na aula 84.

### Criando a representação da versão

```csharp
var version = new Version(8, 0, 35);

var serverVersion = new MySqlServerVersion(version);
```

A classe Version representa o número de versão com seus componentes numéricos.

```text
8.0.35
| | |
| | +-- Build
| +---- Minor
+------ Major
```

MySqlServerVersion representa essa versão no contexto do provider MySQL.

### Duas formas equivalentes

O professor demonstra que podemos declarar as variáveis separadamente:

```csharp
var version = new Version(8, 0, 35);
var serverVersion = new MySqlServerVersion(version);

optionsBuilder.UseMySql(connectionString, serverVersion);
```

Ou criar os objetos diretamente:

```csharp
optionsBuilder.UseMySql(
    connectionString,
    new MySqlServerVersion(new Version(8, 0, 35)));
```

O instrutor prefere separar as variáveis para deixar a explicação mais clara.

**Atenção:** 8.0.35 é a versão do servidor da gravação, e não a versão do pacote EF Core ou do Pomelo. Quem utiliza outra versão do MySQL deve informar a versão correspondente ao seu ambiente.

## 9. Construindo a connection string

A variável connectionString começa vazia e é preenchida depois de configurar o provider.

O instrutor utiliza uma string com os seguintes parâmetros:

```text
Server=localhost;Database=CashFlowDB;Uid=root;Pwd=SUA_SENHA;
```

A estrutura é reconstruída a partir dos campos narrados na gravação. A senha foi substituída por um marcador ilustrativo, para não reproduzir uma credencial pública como configuração recomendada.

### Explicação dos parâmetros

| Parâmetro | Valor no exemplo | Significado |
|---|---|---|
| Server | localhost | Endereço do servidor MySQL |
| Database | CashFlowDB | Banco de dados utilizado |
| Uid | root | Identificação do usuário |
| Pwd | SUA_SENHA | Senha definida na instalação |

Os pares chave=valor são separados por ponto e vírgula.

### Server=localhost

O servidor MySQL está executando na mesma máquina de desenvolvimento.

O instrutor utiliza localhost para identificar esse destino.

### Database=CashFlowDB

Identifica o banco que criamos na aula 85.

O servidor pode possuir diversos bancos de dados, por isso é necessário selecionar aquele que contém as tabelas da aplicação.

### Uid=root e Pwd

A gravação utiliza o usuário administrativo root, configurado durante a instalação.

Pwd é uma abreviação de Password, e Uid representa o usuário na connection string.

O instrutor utiliza a senha demonstrativa definida na aula 84.

**Complemento de segurança:** em um ambiente real, não devemos manter credenciais no código-fonte ou versioná-las no Git. Também é preferível criar uma conta própria para a aplicação com privilégios limitados às suas operações.

### E a porta?

A aula mantém a porta padrão do MySQL: **3306**.

Por isso, o professor não inclui o parâmetro Port na connection string.

A mesma configuração pode ser expressa explicitamente:

```text
Server=localhost;Port=3306;Database=CashFlowDB;Uid=root;Pwd=SUA_SENHA;
```

Se o servidor foi configurado com outra porta, é necessário informar a porta correta.

**Observação:** o professor descreve a conexão local assumindo a configuração padrão do cliente MySQL. O endereço e a porta devem corresponder ao servidor efetivamente disponível.

## 10. Código completo do contexto

O código abaixo reconstrói a implementação final narrada no vídeo. Utilizamos um namespace de acordo com a estrutura do projeto e um marcador de senha no lugar da credencial demonstrativa.

```csharp
using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.DataAccess;

public class CashFlowDbContext : DbContext
{
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString =
            "Server=localhost;Database=CashFlowDB;Uid=root;Pwd=SUA_SENHA;";

        var version = new Version(8, 0, 35);

        var serverVersion = new MySqlServerVersion(version);

        optionsBuilder.UseMySql(connectionString, serverVersion);
    }
}
```

O exemplo pressupõe a referência da Infrastructure ao Domain e os pacotes instalados na aula 88. O instrutor utiliza os tipos existentes no projeto; a gravação não mostra a criação de novas referências entre projetos.

### O que cada parte faz?

| Elemento | Responsabilidade |
|---|---|
| CashFlowDbContext : DbContext | Cria o contexto específico do projeto |
| DbSet<Expense> Expenses | Expõe o conjunto de despesas |
| OnConfiguring | Configura as opções do contexto |
| connectionString | Define o destino e a autenticação |
| Version(8, 0, 35) | Representa a versão do MySQL |
| MySqlServerVersion | Informa o tipo e a versão do servidor ao provider |
| UseMySql | Seleciona e configura o provider MySQL |

A senha precisa ser substituída pela configuração efetivamente utilizada no ambiente. O exemplo não é uma configuração pronta com credenciais reais.

### Nota sobre a propriedade DbSet

Dependendo da versão do EF Core, das anotações de nulabilidade e da análise do compilador, a propriedade DbSet pode receber avisos relacionados à inicialização.

Uma alternativa complementar é expor o conjunto por meio de Set<Expense>:

```csharp
public DbSet<Expense> Expenses => Set<Expense>();
```

Essa alternativa não é a implementação demonstrada pelo instrutor. O código principal preserva a auto-property utilizada na aula.

## 11. Como o contexto será utilizado

O professor explica que, após essa configuração, poderá utilizar Expenses para criar, consultar, alterar e excluir despesas.

O fluxo conceitual é:

```text
Objeto Expense
       |
       v
CashFlowDbContext.Expenses
       |
       v
Entity Framework Core
       |
       v
Pomelo / MySQL
       |
       v
Tabela expenses
```

### Persistência

Quando uma entidade é adicionada ao contexto, o EF Core pode rastrear essa alteração.

**Complemento didático para antecipar a ideia:** uma operação de cadastro poderia utilizar Add seguido de SaveChanges.

```csharp
// Exemplo conceitual, não executado nesta aula.
context.Expenses.Add(expense);
context.SaveChanges();
```

Add registra a entidade como uma adição no contexto, enquanto SaveChanges solicita a persistência das alterações.

### Recuperação

```csharp
// Exemplo conceitual, não executado nesta aula.
var expenses = context.Expenses.ToList();
```

Nesse exemplo, a consulta recuperaria os registros e materializaria instâncias de Expense.

**Importante:** os trechos acima pressupõem um contexto configurado e utilizável, uma entidade válida e uma conexão disponível. São apenas complementos para entender o papel da propriedade Expenses.

A aula termina antes de criar ou consultar uma despesa pela API.

## 12. Relação com as camadas do CashFlow

O projeto agora possui a entidade no Domain e a configuração do EF Core na Infrastructure.

```text
CashFlow.Domain
├── Entities
│   └── Expense.cs
└── Enums
    └── PaymentType.cs
             |
             v
CashFlow.Infrastructure
└── DataAccess
    └── CashFlowDbContext.cs
        ├── Expenses
        └── OnConfiguring
             |
             v
         MySQL Server
             |
             v
        CashFlowDB
             |
             v
          expenses
```

O contexto depende da entidade Expense para representar as despesas.

A entidade, por sua vez, não precisa conter comandos SQL ou depender diretamente da tecnologia de persistência para representar a despesa.

Essa organização mantém a responsabilidade de acesso ao banco na Infrastructure, conforme o projeto vem sendo estruturado nas aulas anteriores.

## 13. Observações técnicas complementares

### 13.1 Configurar não significa testar a conexão

O código informa ao EF Core como acessar o banco. Isso não comprova, por si só, que o servidor está em execução, a senha está correta e o mapeamento funciona.

O instrutor anuncia que testará a configuração na próxima aula.

### 13.2 A tabela já existe

O contexto não cria automaticamente a tabela expenses apenas por declarar um DbSet.

A tabela foi criada manualmente na aula 85. A criação e a evolução controlada do esquema por migrations serão estudadas posteriormente.

### 13.3 DbSet não exige correspondência literal de nomes

A correspondência com a tabela pode ser definida por convenções ou mapeamento explícito. Também é necessário considerar os tipos, os limites e a nulabilidade da estrutura já criada.

### 13.4 ServerVersion não é a versão do NuGet

A versão 8.0.35 pertence ao MySQL Server utilizado na aula. O EF Core e o Pomelo possuem seus próprios números de versão, conforme discutido na aula 89.

### 13.5 Connection string e segurança

A string escrita diretamente em OnConfiguring reproduz o procedimento introdutório do curso.

Em uma implementação mais completa, convém utilizar configuração externa e um mecanismo adequado de gerenciamento de segredos, em vez de credenciais fixas no código.

### 13.6 Outras formas de configurar o contexto

DbContext pode receber opções por construtor e ser registrado com injeção de dependência. É comum utilizar essas estratégias em aplicações ASP.NET Core.

A aula não implementa nenhuma delas ainda; mantém a configuração diretamente no OnConfiguring para introduzir o conceito.

## 14. Resumo e mapa mental

### Principais aprendizados

- Criamos a pasta DataAccess na camada Infrastructure.
- Criamos a classe CashFlowDbContext, que herda de DbContext.
- A classe foi declarada public por enquanto.
- DbSet<Expense> Expenses representa o conjunto de despesas.
- OnConfiguring permite configurar as opções do contexto.
- UseMySql é fornecido pelo provider Pomelo.
- A versão do servidor foi consultada em Server > Server Status, no Workbench.
- A gravação utiliza MySQL Server 8.0.35.
- MySqlServerVersion recebe uma representação Version(8, 0, 35).
- A connection string informa localhost, CashFlowDB, usuário e senha.
- A porta 3306 é a padrão e pode ser omitida na configuração demonstrada.
- A classe está configurada, mas a conexão em execução e as operações da API serão testadas nas próximas aulas.

### Perguntas de revisão

1. Qual é a função do DbContext no projeto?
2. Por que CashFlowDbContext precisa herdar de DbContext?
3. O que representa DbSet<Expense>?
4. Qual é a finalidade do método OnConfiguring?
5. De qual pacote vem UseMySql?
6. Quais parâmetros o professor fornece para UseMySql?
7. Onde encontramos a versão do servidor MySQL?
8. Qual é a diferença entre a versão do MySQL Server e a versão do EF Core?
9. Quais são os parâmetros da connection string utilizados na aula?
10. Declarar o contexto já comprova que a API consegue salvar dados? Por quê?

### Mapa mental

```text
                       AULA 90
                          |
                    Infrastructure
                          |
                      DataAccess
                          |
                   CashFlowDbContext
                          |
                    herda DbContext
                          |
              +-----------+-----------+
              |                       |
          DbSet<Expense>         OnConfiguring
              |                       |
           Expenses                UseMySql
              |                       |
              |             +---------+---------+
              |             |                   |
              |      Connection string     Server version
              |             |                   |
              |       localhost:3306          8.0.35
              |        CashFlowDB               |
              |        Uid + Pwd                |
              |             |                   |
              +-------------+-------------------+
                            |
                       MySQL Server
                            |
                        expenses
                            |
                    Próxima aula:
                    testar o contexto
```

**Conclusão:** nesta aula, configuramos o contexto de acesso aos dados do CashFlow. A classe CashFlowDbContext reúne o conjunto de despesas e a configuração do provider MySQL, preparando o projeto para realizar operações de persistência nas próximas aulas.
