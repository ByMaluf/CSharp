# Introdução ao Entity Framework Core: ORM, connection string e pacotes NuGet

> **Aula 88 — Entity Framework Core e preparação da Infrastructure**
>
> Continuação das aulas 85 a 87: criamos a tabela expenses no MySQL, praticamos comandos SQL e construímos a entidade Expense no Domain. Agora conhecemos a ferramenta que fará a ligação entre o modelo C# e o banco de dados.

## Índice

1. Objetivo da aula
2. O problema de escrever SQL diretamente no código
3. O que é ORM?
4. Entity Framework Core: a analogia do tradutor
5. Objetos, entidades e nível de abstração
6. O que será necessário configurar
7. Connection string
8. Bancos de dados suportados
9. InMemory e testes de integração
10. Instalando os pacotes NuGet na Infrastructure
11. Entity Framework Core e provider MySQL
12. Relação com as camadas do CashFlow
13. Observações técnicas complementares
14. Resumo e mapa mental

---

## 1. Objetivo da aula

Na aula anterior, criamos a entidade Expense para representar uma despesa na camada Domain. Ela possui propriedades correspondentes aos dados armazenados na tabela expenses.

O próximo passo é preparar o mecanismo que permitirá à API persistir essas informações e recuperá-las posteriormente.

O instrutor apresenta o **Entity Framework Core**, explica suas vantagens e instala os dois pacotes necessários para o módulo que utilizará MySQL.

Os temas da gravação são:

- dificuldades de escrever consultas SQL diretamente no código C#;
- o conceito de ORM e o mapeamento objeto-relacional;
- Entity Framework Core como ferramenta para trabalhar com entidades;
- informações necessárias para configurar a conexão;
- connection string;
- bancos de dados e provedores suportados;
- uso de InMemory em testes;
- instalação dos pacotes NuGet na camada Infrastructure.

**Escopo:** o vídeo introduz o framework e instala os pacotes. A classe de contexto, seu mapeamento, a connection string real e a conexão com o banco serão implementados nas próximas aulas.

## 2. O problema de escrever SQL diretamente no código

Nas aulas anteriores, praticamos as operações básicas de um banco relacional usando o MySQL Workbench.

```sql
SELECT * FROM cashflowdb.expenses;

INSERT INTO cashflowdb.expenses
    (Title, Date, Amount, PaymentType)
VALUES
    ('iPhone 15', '2023-11-10 14:30:00', 1350.99, 1);
```

Também utilizamos UPDATE para alterar registros e DELETE para excluí-los.

A API C# pode utilizar SQL diretamente. Entretanto, o instrutor aponta três dificuldades que podem surgir quando escrevemos muitas queries dentro do código.

### 2.1 Primeiro problema: legibilidade

À medida que as consultas ficam complexas, pode ser mais difícil compreender rapidamente o que uma classe ou método faz.

Uma consulta pequena é relativamente fácil de interpretar. Já consultas extensas, com filtros e outras operações, podem exigir mais esforço de leitura.

### 2.2 Segundo problema: crescimento do código

Se cada operação de acesso aos dados exige uma instrução SQL escrita manualmente, o volume de código pode aumentar.

O professor destaca que consultas grandes ou complexas podem contribuir para classes maiores e mais difíceis de manter.

### 2.3 Terceiro problema: erros e comportamentos inesperados

Uma consulta escrita manualmente pode conter erros de sintaxe ou produzir um resultado diferente do esperado.

Quando SQL é armazenado em uma string C#, o compilador da linguagem não verifica toda a sintaxe SQL. Um problema pode aparecer somente durante a execução.

### 2.4 Outro trabalho: converter resultados em objetos

Além de escrever as queries, o desenvolvedor precisa lidar com os resultados retornados pelo banco.

Por exemplo, para transformar uma linha de expenses em um objeto Expense, seria necessário associar o valor de cada coluna à propriedade correspondente.

```text
Linha retornada pelo banco
             |
             v
Ler e converter cada coluna
             |
             v
Preencher um objeto Expense
```

O Entity Framework Core é introduzido como uma solução para reduzir esse trabalho repetitivo.

**Observação técnica:** o instrutor reconhece que existem situações em que utilizar SQL diretamente é necessário ou conveniente. As dificuldades citadas não tornam SQL manual uma prática incorreta por si só.

## 3. O que é ORM?

**ORM** é a sigla de *Object-Relational Mapping*, traduzida como **Mapeamento Objeto-Relacional**.

Também utilizamos a expressão ORM para nos referir a uma ferramenta de mapeamento objeto-relacional.

A ideia é estabelecer uma correspondência entre:

- objetos e propriedades da aplicação;
- tabelas, linhas e colunas do banco relacional.

### Exemplo com Expense

| Modelo C# | Modelo relacional |
|---|---|
| Classe Expense | Tabela expenses |
| Instância de Expense | Uma linha da tabela |
| Propriedade Title | Coluna Title |
| Propriedade Amount | Coluna Amount |
| Propriedade Id | Coluna Id |

O ORM utiliza o modelo da aplicação e as regras de mapeamento para realizar operações de persistência e materializar resultados.

### Por que existe esse mapeamento?

Porque a linguagem C# trabalha com objetos, enquanto o banco relacional trabalha com estruturas tabulares e operações de consulta e manipulação de dados.

O ORM faz a ligação entre essas representações.

## 4. Entity Framework Core: a analogia do tradutor

O instrutor compara o Entity Framework a uma espécie de **Google Tradutor**.

Imagine que queremos persistir uma instância de Expense.

Em vez de escrever manualmente toda a instrução INSERT, podemos utilizar o framework para indicar que desejamos adicionar a entidade ao banco.

```text
Aplicação C#
    |
    v
Objeto Expense
    |
    v
Entity Framework Core
    |
    v
Comando SQL apropriado
    |
    v
MySQL Server
```

### Recuperação dos dados

O caminho inverso também é importante.

```text
MySQL Server
    |
    v
Resultado da consulta
    |
    v
Entity Framework Core
    |
    v
Instância(s) de Expense
    |
    v
Aplicação C#
```

O framework pode ler os valores retornados e preencher as propriedades correspondentes, processo conhecido como materialização de objetos.

### Vantagem principal

O desenvolvedor pode trabalhar em um **nível mais alto de abstração**, utilizando entidades e operações expressas no código C#.

Isso reduz a necessidade de construir manualmente cada comando SQL e de converter cada resultado em um objeto.

**Precisão técnica:** o Entity Framework Core não deixa de utilizar SQL quando opera com um provedor relacional. O SQL é gerado pelo framework em conjunto com o provider. Nem toda operação C# é traduzível automaticamente, e o framework também permite executar SQL quando necessário.

## 5. Objetos, entidades e nível de abstração

Na aula 87, criamos uma classe semelhante a esta:

```csharp
public class Expense
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public PaymentType PaymentType { get; set; }
}
```

O professor relembra que uma entidade é uma classe utilizada, no contexto deste projeto, para representar os dados que serão persistidos.

Uma instância de Expense contém as informações de uma despesa específica.

### O significado de trabalhar em nível mais alto

| Nível | Como representamos a operação |
|---|---|
| SQL | Uma instrução INSERT, SELECT, UPDATE ou DELETE |
| C# com ORM | Objetos, entidades e operações sobre o contexto de dados |

A abstração permite expressar a intenção da operação usando conceitos do código C#, sem necessariamente escrever o SQL correspondente manualmente.

**Observação:** a classe Expense não realiza persistência por si só. O framework precisa conhecer seu mapeamento e a configuração do banco.

## 6. O que será necessário configurar

O instrutor explica que será criada uma classe especial para configurar o Entity Framework.

Ela deverá permitir:

1. Indicar quais propriedades representam as entidades ou tabelas utilizadas.
2. Informar onde está o servidor de banco de dados.
3. Indicar qual database será utilizado.
4. Fornecer as informações necessárias para autenticação.

Essa configuração será implementada nas próximas aulas.

### Complemento conceitual: DbContext e DbSet

Os conceitos mencionados pelo professor correspondem, no EF Core, ao uso de um contexto de dados, normalmente uma classe derivada de **DbContext**, que pode expor conjuntos de entidades por meio de **DbSet**.

```text
Contexto de dados
    |
    +-- Conjuntos de entidades
    |
    +-- Configuração do modelo
    |
    +-- Configuração do provider
    |
    +-- Configuração da conexão
```

Esta explicação identifica a terminologia técnica relacionada ao que o instrutor anuncia. O vídeo ainda não implementa uma classe DbContext nem declara um DbSet de despesas.

## 7. Connection string

O instrutor apresenta o conceito de **connection string**: uma string que contém informações necessárias para conectar uma aplicação a um banco de dados.

### Informações destacadas

| Informação | Finalidade no CashFlow |
|---|---|
| Servidor/host | Localizar a instância do MySQL, neste ambiente local |
| Database | Identificar o banco do projeto |
| Usuário | Identificar a conta utilizada na conexão |
| Senha | Autenticar a conta, quando exigido |

Na aula 84, configuramos o MySQL local e utilizamos a porta 3306. Na aula 85, criamos o banco do CashFlow.

A connection string reunirá os parâmetros de conexão necessários ao ambiente.

### Exemplo didático

O vídeo não fornece uma connection string completa. Para visualizar o conceito, uma representação possível para o ambiente MySQL é:

```text
Server=localhost;Port=3306;Database=cashflowdb;User ID=USUARIO;Password=SENHA;
```

USUARIO e SENHA são marcadores ilustrativos. Não são credenciais reais.

### Sintaxe varia conforme a tecnologia

O professor ressalta que uma connection string de MySQL não precisa ter exatamente a mesma sintaxe de uma connection string de SQL Server.

As informações necessárias podem ser semelhantes, mas as palavras-chave, os formatos e os mecanismos de autenticação dependem da tecnologia e do driver.

### Local versus nuvem

```text
Desenvolvimento local
Host: localhost

Ambiente remoto
Host: endereço acessível do servidor
```

O endereço remoto pode ser um hostname, IP ou outro endpoint apropriado ao serviço. A expressão "URL" utilizada pelo instrutor é uma simplificação; uma connection string nem sempre contém uma URL HTTP.

### Segurança — complemento técnico

Não devemos incluir senhas reais em arquivos versionados no Git. Para a configuração efetiva, podem ser utilizados mecanismos de configuração e gerenciamento de segredos apropriados ao ambiente.

A aula apenas introduz o conceito; não configura credenciais na API.

## 8. Bancos de dados suportados

O professor apresenta uma tabela com tecnologias que podem ser utilizadas com o Entity Framework Core.

| Tecnologia mencionada | Categoria |
|---|---|
| SQL Server | Banco relacional |
| SQLite | Banco relacional |
| PostgreSQL | Banco relacional |
| MySQL | Banco relacional |
| MariaDB | Banco relacional |
| Azure Cosmos DB | Banco de dados NoSQL, no contexto do provider Cosmos |
| InMemory | Provedor não relacional em memória, apresentado para testes |

A lista reproduz as opções citadas na gravação, não uma relação exaustiva de todos os provedores existentes.

### O que permite esse suporte?

O EF Core oferece uma estrutura comum, enquanto **providers (provedores)** implementam a integração com tecnologias específicas.

```text
                 Entity Framework Core
                           |
      +--------------------+------------------+
      |                    |                  |
 Provider MySQL     Provider SQL Server   Outros providers
      |                    |                  |
      v                    v                  v
    MySQL              SQL Server         Outros bancos
```

O provider é importante porque cada tecnologia possui diferenças no modo de acesso, nos tipos suportados, na sintaxe de consultas e nas funcionalidades.

**Observação:** a menção a Cosmos DB mostra que o EF Core não se limita estritamente a bancos relacionais. O modelo de tradução para SQL descrito anteriormente se aplica ao cenário relacional do CashFlow.

## 9. InMemory e testes de integração

O instrutor destaca a opção InMemory, que será estudada futuramente em testes.

A proposta anunciada é executar a API em um ambiente de testes, fazer requisições e verificar as respostas, de maneira semelhante a um teste realizado pelo Postman.

### Diferença em relação aos testes unitários anteriores

Nas aulas anteriores, testamos isoladamente regras do validator com xUnit.

Agora, o professor antecipa testes de integração em que a API será efetivamente executada no ambiente de testes.

```text
Projeto de testes
       |
       v
Requisição à API em teste
       |
       v
Pipeline, processamento e acesso a dados
       |
       v
Resposta
       |
       v
Verificação do resultado
```

### O papel do InMemory

O provider InMemory permite trabalhar com dados em memória no contexto do EF Core, sem depender de uma instância real do MySQL para aquele cenário de testes.

O instrutor afirma que utilizará essa possibilidade durante o curso e ensinará sua configuração mais adiante.

**Complemento técnico importante:** InMemory não reproduz integralmente o comportamento de um banco relacional. Restrições, transações e tradução de consultas podem funcionar de maneira diferente. Portanto, um teste que passa com InMemory não comprova automaticamente que a mesma operação funciona no MySQL. Testes que precisam verificar o comportamento específico do banco devem utilizar uma estratégia adequada a esse objetivo.

O vídeo não instala InMemory nem implementa testes de integração nesta etapa.

## 10. Instalando os pacotes NuGet na Infrastructure

Na parte prática da aula, o instrutor abre o Visual Studio e instala dois pacotes.

### Projeto correto

**CashFlow.Infrastructure**

O motivo apresentado é que a Infrastructure será responsável pelo código que se comunica com o banco de dados.

### Primeiro pacote: Microsoft.EntityFrameworkCore

1. Clicar com o botão direito no projeto Infrastructure.
2. Selecionar **Manage NuGet Packages**.
3. Abrir a aba **Browse**.
4. Pesquisar por `Microsoft.EntityFrameworkCore`.
5. Selecionar o pacote e instalar.

O pacote fornece os principais tipos e funcionalidades do Entity Framework Core.

### Segundo pacote: Pomelo.EntityFrameworkCore.MySql

1. Permanecer no gerenciamento de pacotes do projeto Infrastructure.
2. Pesquisar por MySQL.
3. Selecionar `Pomelo.EntityFrameworkCore.MySql`.
4. Instalar o pacote.

O Pomelo é o provider utilizado no curso para integrar o EF Core ao MySQL.

### Pacotes instalados

| Pacote | Responsabilidade |
|---|---|
| Microsoft.EntityFrameworkCore | Base do EF Core, contexto e funcionalidades de mapeamento |
| Pomelo.EntityFrameworkCore.MySql | Integração específica do EF Core com MySQL e MariaDB |

**Nota sobre versões:** a transcrição não informa os números exatos das versões escolhidas no NuGet. Por isso, não é possível registrá-los como parte da demonstração. Em um projeto real, as versões do EF Core, do Pomelo e do framework de destino precisam ser compatíveis.

## 11. Entity Framework Core e provider MySQL

O instrutor explica por que instalamos dois componentes.

O pacote base oferece as funcionalidades gerais do EF Core.

O provider permite que essas funcionalidades trabalhem com o banco escolhido.

```text
CashFlow.Infrastructure
          |
          v
Microsoft.EntityFrameworkCore
          |
          v
Pomelo.EntityFrameworkCore.MySql
          |
          v
MySQL Server
          |
          v
cashflowdb.expenses
```

### E se utilizássemos SQL Server?

O professor mostra que existe outro pacote:

```text
Microsoft.EntityFrameworkCore.SqlServer
```

Esse é um exemplo de provider específico para uma tecnologia diferente.

O vídeo não instala esse pacote, pois o banco escolhido para este módulo é MySQL.

### Complemento técnico: dependências NuGet

Um provider normalmente declara dependências dos componentes do EF Core que utiliza. Assim, nem sempre é obrigatório instalar separadamente o pacote base em todos os projetos.

Nesta aula, o instrutor instala ambos explicitamente, e esse é o procedimento preservado no resumo.

O Pomelo é um provider de terceiros; não deve ser confundido com um pacote publicado pela Microsoft.

## 12. Relação com as camadas do CashFlow

Até agora, o projeto contém classes com responsabilidades diferentes.

```text
CashFlow.Communication
    └── Requests, Responses e enums de comunicação

CashFlow.Domain
    ├── Entities
    │   └── Expense.cs
    └── Enums
        └── PaymentType.cs

CashFlow.Infrastructure
    └── Pacotes de acesso a dados
        ├── Microsoft.EntityFrameworkCore
        └── Pomelo.EntityFrameworkCore.MySql
```

A entidade permanece no Domain. A tecnologia de persistência será configurada na Infrastructure.

O vídeo prepara essa separação, mas ainda não mostra as referências entre projetos, o registro de serviços ou a implementação de repositórios.

### Fluxo futuro conceitual

```text
Requisição HTTP
       |
       v
Aplicação e regras de negócio
       |
       v
Entidade Expense
       |
       v
Infrastructure / EF Core / Pomelo
       |
       v
MySQL Server
```

## 13. Observações técnicas complementares

### 13.1 EF Core é open source

O professor destaca que o Entity Framework Core é open source e possui forte apoio da Microsoft.

Existem outros mecanismos e ORMs para acesso a dados. A gravação escolhe EF Core para desenvolver o CashFlow.

### 13.2 Trabalhar sem SQL manual não significa eliminar SQL

O banco relacional continua recebendo e processando instruções SQL. A abstração muda a maneira como o desenvolvedor expressa operações no código.

SQL direto continua sendo uma opção em cenários apropriados.

### 13.3 ORM não elimina todos os erros de execução

A utilização de APIs C# e mapeamentos pode melhorar a legibilidade e reduzir determinados erros de sintaxe manual.

Contudo, ainda podem ocorrer falhas de conexão, incompatibilidades de mapeamento, restrições violadas, consultas não traduzíveis e problemas de desempenho.

### 13.4 Instalar pacotes não conecta automaticamente a API ao banco

Ainda será necessário configurar o contexto e o provider, fornecer os parâmetros de conexão e implementar as operações de persistência.

### 13.5 InMemory não é um substituto universal do banco real

O curso anuncia seu uso para testes. A estratégia deve ser escolhida conforme o comportamento que o teste precisa verificar.

### 13.6 Providers e versões

Os provedores possuem compatibilidade e capacidades próprias. A afirmação de suporte a uma tecnologia não implica que qualquer versão do provider seja compatível com qualquer versão do EF Core.

A gravação não informa versões exatas nem configura os pacotes de migrations.

## 14. Resumo e mapa mental

### Principais aprendizados

- Escrever queries diretamente no C# é possível, mas pode aumentar a complexidade, o volume de código e a chance de erros em execução.
- ORM significa mapeamento objeto-relacional.
- O EF Core permite trabalhar com objetos e abstrair parte do acesso ao banco.
- No cenário MySQL, o framework e o provider traduzem operações suportadas em SQL e materializam resultados em objetos.
- A configuração futura utilizará informações do modelo e uma connection string.
- A connection string identifica o destino da conexão e seus parâmetros de autenticação, conforme a tecnologia.
- O instrutor cita SQL Server, SQLite, PostgreSQL, MySQL, MariaDB, Azure Cosmos DB e InMemory.
- InMemory é apresentado para futuros testes de integração.
- Os pacotes instalados na Infrastructure são Microsoft.EntityFrameworkCore e Pomelo.EntityFrameworkCore.MySql.
- Microsoft.EntityFrameworkCore.SqlServer é mostrado como exemplo de outro provider, mas não é instalado.
- A configuração efetiva do contexto e da conexão permanece para as próximas aulas.

### Perguntas de revisão

1. Quais são os três problemas de escrever queries diretamente no código citados pelo professor?
2. O que significa ORM?
3. Por que o instrutor compara o Entity Framework a um tradutor?
4. Que informações são mencionadas como parte de uma connection string?
5. Por que precisamos de um provider específico para MySQL?
6. Quais pacotes foram instalados e em qual projeto?
7. Qual é a finalidade anunciada para InMemory?
8. A instalação dos pacotes já permite que a API grave despesas automaticamente? Por quê?

### Mapa mental

```text
                      AULA 88
                         |
                ENTITY FRAMEWORK CORE
                         |
               ORM / nível de abstração
                         |
            +------------+-------------+
            |                          |
       Entidades C#              Banco de dados
            |                          |
            +------------+-------------+
                         |
                   Mapeamento
                         |
                Provider específico
                         |
           +-------------+-------------+
           |                           |
         MySQL                     Outros bancos
           |                           |
         Pomelo                 SQL Server, etc.
           |
           v
       Infrastructure
           |
   Pacotes NuGet instalados
           |
           v
    Próximas configurações
       Contexto + conexão

      INMEMORY -> testes futuros
```

**Conclusão:** a aula explica por que o Entity Framework Core será utilizado no CashFlow e prepara a infraestrutura para conectar a API ao MySQL. O framework oferece a abstração sobre o acesso aos dados, enquanto o Pomelo fornece a integração com a tecnologia de banco escolhida. A persistência ainda será configurada nas próximas aulas.
