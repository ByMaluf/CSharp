# Criando a entidade Expense na camada Domain

> **Aula 87 — Entidades, DTOs e separação do enum PaymentType**
>
> Continuação das aulas 85 e 86, em que criamos a tabela expenses no MySQL e praticamos operações SQL. Nesta aula, vamos representar uma despesa em C# por meio de uma entidade localizada na camada Domain.

## Índice

1. Objetivo da aula
2. O que é uma entidade?
3. Por que criar uma entidade separada da Request?
4. Entidade versus Request e Response
5. Criando a pasta Entities
6. Estrutura completa da classe Expense
7. Explicação das propriedades
8. Id e a geração automática no banco
9. Title, Description e nullable reference types
10. Criando o enum PaymentType no Domain
11. Por que duplicar o enum?
12. Correspondência entre C# e MySQL
13. Fluxo conceitual de persistência e recuperação
14. Observações técnicas complementares
15. Resumo e mapa mental

---

## 1. Objetivo da aula

Nas aulas anteriores, criamos o banco de dados e a tabela de despesas, com seis colunas. Depois, praticamos INSERT, SELECT, UPDATE e DELETE diretamente no MySQL Workbench.

Agora, precisamos representar uma despesa dentro do código da aplicação.

O instrutor cria uma classe chamada **Expense**, na camada **Domain**, com propriedades que correspondem às informações armazenadas na tabela expenses.

O objetivo é preparar uma entidade que poderá ser utilizada nas etapas posteriores de persistência.

**Escopo:** nesta aula são criadas a classe Expense e uma versão do enum PaymentType na camada Domain. Ainda não são configurados o DbContext, o mapeamento do Entity Framework Core, a conexão com o MySQL ou operações de gravação e leitura pela API.

## 2. O que é uma entidade?

O instrutor apresenta a entidade como uma classe C# normal que, neste projeto, será um **espelho da tabela de despesas do banco de dados**.

Na prática, a entidade contém propriedades que representam as informações de uma despesa:

- identificador;
- título;
- descrição;
- data;
- valor;
- tipo de pagamento.

```text
Banco de dados                 Aplicação C#
Tabela expenses                Classe Expense
       |                              |
       +------ correspondência ------+
              entre os campos
```

### Por que precisamos dessa classe?

O professor explica dois cenários:

1. Preencher as propriedades da entidade para persistir uma despesa.
2. Recuperar informações do banco e preencher as propriedades correspondentes da entidade.

A entidade serve como representação dos dados com que a aplicação irá trabalhar.

### Nuance técnica

A metáfora do espelho é útil para o exemplo simples do curso. Em aplicações com ORM, não é obrigatório que os nomes das propriedades sejam idênticos aos das colunas, desde que exista um mapeamento adequado.

Uma entidade também pode conter comportamento e regras de negócio, em vez de apenas propriedades. O curso, nesta etapa, está construindo uma classe simples para representar uma despesa.

## 3. Por que criar uma entidade separada da Request?

O projeto já possui uma classe utilizada para receber os dados necessários ao cadastro de uma despesa.

O instrutor explica que será necessário repetir algumas propriedades dessa classe na entidade. Isso acontece porque os dois tipos atendem a **responsabilidades diferentes**.

### Exemplo: Id

Quando o cliente envia uma requisição para cadastrar uma nova despesa, não precisa fornecer o Id que será gerado automaticamente pelo banco.

Porém, a entidade deve possuir Id, pois ele identifica o registro persistido.

```text
Request de cadastro           Entidade Expense
-------------------           ----------------
Title                         Id
Description                   Title
Date                          Description
Amount                        Date
PaymentType                   Amount
                              PaymentType
```

As duas classes têm informações em comum, mas não possuem necessariamente a mesma estrutura.

### Exemplo de usuário

O instrutor antecipa uma funcionalidade futura de cadastro de usuários para explicar outra diferença:

- Uma Request pode receber uma senha.
- A informação armazenada no banco não deve ser a senha em texto puro.
- Uma Response não deve devolver indiscriminadamente todas as propriedades persistidas.

**Observação técnica de segurança:** o professor utiliza a palavra "criptografada". Para senhas de autenticação, a prática adequada é armazenar um hash de senha produzido por um algoritmo apropriado, com salt, e não uma senha reversivelmente criptografada. Este é um esclarecimento complementar; a aula não implementa autenticação nem armazenamento de senhas.

## 4. Entidade versus Request e Response

| Tipo | Responsabilidade | Exemplo |
|---|---|---|
| Request | Definir os dados recebidos pela API | Dados para cadastrar uma despesa |
| Entidade | Representar a despesa no domínio e no modelo de persistência adotado | Expense com Id e demais propriedades |
| Response | Definir os dados que a API devolve ao cliente | Informações necessárias para exibir uma despesa |

O instrutor reforça que **receber, persistir e responder são operações diferentes**.

Por isso, não é obrigatório que todas utilizem a mesma classe.

### Relação com DTOs

As classes de entrada e saída da API são frequentemente chamadas de DTOs (*Data Transfer Objects*), pois representam os dados transferidos entre componentes.

A classe de Request não precisa incluir informações que são de responsabilidade do banco. A Response pode expor apenas as informações necessárias para o cliente.

```text
Cliente -> Request -> processamento -> Expense
                                          |
                                          v
                                    Persistência

Cliente <- Response <- processamento <- Expense
                                          ^
                                          |
                                     Recuperação
```

Este fluxo é conceitual. A conversão entre os tipos ainda não é implementada no vídeo.

## 5. Criando a pasta Entities

O instrutor utiliza o Visual Studio e localiza o projeto **Domain**.

### Passos apresentados

1. Expandir o projeto Domain.
2. Criar uma pasta chamada **Entities**.
3. Dentro de Entities, adicionar uma classe chamada **Expense**.
4. Alterar o modificador de acesso gerado de internal para public.
5. Remover os using desnecessários com o atalho Ctrl+R, Ctrl+G utilizado na gravação.
6. Declarar as propriedades da entidade.

### Estrutura do projeto

```text
CashFlow.Domain
├── Entities
│   └── Expense.cs
└── Enums
    └── PaymentType.cs
```

A pasta Enums será criada posteriormente dentro da mesma aula.

### Por que Expense no singular?

O instrutor utiliza o singular porque uma instância da classe representa **uma despesa**.

Na aula anterior, a tabela foi chamada expenses no plural porque reúne várias despesas.

| Nome | Representação |
|---|---|
| Expense | Classe que representa uma despesa |
| expenses | Tabela que armazena várias despesas |

Essa diferença de nomenclatura é uma convenção do projeto, não uma regra obrigatória da linguagem C# ou de todos os bancos relacionais.

## 6. Estrutura completa da classe Expense

A seguir, uma reconstrução do código final descrito na gravação, considerando a organização CashFlow.Domain.

```csharp
using CashFlow.Domain.Enums;

namespace CashFlow.Domain.Entities;

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

A classe contém seis propriedades públicas, correspondentes às informações da tabela expenses.

O namespace apresentado segue a organização do projeto. O código acima é uma reconstrução didática da implementação narrada, e não uma cópia literal de uma captura da tela.

### Auto-properties

As propriedades são declaradas com get e set:

```csharp
public long Id { get; set; }
```

Isso permite consultar e modificar o valor da propriedade, utilizando a sintaxe de propriedades automáticas do C#.

A classe criada é simples. Nenhum método de negócio, construtor personalizado ou lógica de persistência é implementado nesta aula.

## 7. Explicação das propriedades

| Propriedade | Tipo | Finalidade |
|---|---|---|
| Id | long | Identificador da despesa |
| Title | string | Título obrigatório |
| Description | string? | Descrição opcional |
| Date | DateTime | Data e hora da despesa |
| Amount | decimal | Valor monetário |
| PaymentType | PaymentType | Tipo de pagamento representado por enum |

Cada propriedade acompanha a estrutura que já definimos no banco.

A nomenclatura utiliza PascalCase, seguindo a convenção usual de propriedades públicas em C#.

## 8. Id e a geração automática no banco

A primeira propriedade é:

```csharp
public long Id { get; set; }
```

Na aula 85, definimos a coluna Id como BIGINT, PRIMARY KEY, NOT NULL e AUTO_INCREMENT.

### Correspondência

```text
C#:    long Id
MySQL: BIGINT Id
```

Um long em C# representa um inteiro de 64 bits com sinal, assim como o BIGINT com sinal utilizado na estrutura da tabela.

### Por que Id não está na Request de cadastro?

Porque a identidade da nova despesa será gerada pelo banco no fluxo de persistência que será implementado futuramente.

Quando instanciamos uma Expense sem atribuir Id, a propriedade long recebe inicialmente o valor padrão 0.

**Complemento técnico:** 0 em uma entidade recém-criada não significa que exista uma linha com Id 0 no banco. A configuração do mecanismo de persistência será responsável por tratar a geração do identificador e recuperar o valor atribuído.

A declaração da propriedade Id, sozinha, não executa SQL nem ativa o AUTO_INCREMENT no C#.

## 9. Title, Description e nullable reference types

### Title

```csharp
public string Title { get; set; } = string.Empty;
```

O título é obrigatório na estrutura criada para a despesa.

O instrutor inicializa a propriedade com string.Empty, como já foi utilizado em outras classes do curso.

Isso fornece um valor inicial não nulo para a propriedade.

### Description

```csharp
public string? Description { get; set; }
```

A descrição é opcional na tabela e, por isso, o instrutor usa string?, indicando que a propriedade pode conter null.

### Comparação

| Propriedade | Valor inicial neste exemplo | Pode representar null segundo as anotações? |
|---|---|---|
| Title | string.Empty | Não é a intenção declarada |
| Description | null | Sim |

### Distinção técnica importante

Nullable reference types, quando habilitados no C#, permitem que o compilador analise e sinalize possíveis usos inadequados de referências nulas.

A anotação string não impede null em tempo de execução em todos os cenários. Da mesma forma, string.Empty não torna o título válido segundo as regras de negócio.

```csharp
var expense = new Expense();

// Title começa com uma string vazia.
// Description começa com null.
```

O validator estudado anteriormente continua relevante para garantir que o título não seja vazio.

### Correspondência com a tabela

```text
Title:       string   <-> VARCHAR(255) NOT NULL
Description: string?  <-> VARCHAR(2000) NULL
```

**Atenção:** declarar string em C# também não estabelece automaticamente o limite de 255 caracteres. Esse limite pertence à estrutura do banco e pode ser refletido no mapeamento e na validação posteriormente.

## 10. Criando o enum PaymentType no Domain

O instrutor lembra que já existe um enum PaymentType dentro da camada Communication.

Entretanto, a entidade Expense está sendo criada no contexto da camada Domain. Por isso, ele decide criar uma definição própria desse enum no Domain.

### Procedimento mostrado

1. Criar uma pasta chamada **Enums** no projeto Domain.
2. Adicionar um arquivo chamado **PaymentType.cs**.
3. Copiar a definição do enum existente em Communication.
4. Ajustar o arquivo para o contexto do Domain.
5. Remover using desnecessários.
6. Voltar para Expense e alterar o tipo da propriedade PaymentType para o enum.
7. Importar o namespace do enum do Domain com using.

### Organização resultante

```text
CashFlow.Communication
└── Enums
    └── PaymentType.cs

CashFlow.Domain
├── Entities
│   └── Expense.cs
└── Enums
    └── PaymentType.cs
```

Os nomes dos membros não são recitados novamente nesta transcrição. O procedimento demonstrado é copiar a definição já existente, mantendo seus valores.

Para consultar a definição original, ver a aula em que criamos o enum de pagamento no projeto Communication.

### Propriedade final

```csharp
public PaymentType PaymentType { get; set; }
```

O instrutor troca o tipo temporário da propriedade pelo tipo correto: PaymentType.

O nome da propriedade e o nome do tipo são iguais. Isso é válido em C#; o primeiro PaymentType indica o tipo e o segundo é o nome da propriedade.

## 11. Por que duplicar o enum?

O professor explica que, mesmo já existindo PaymentType em Communication, a nova entidade está no contexto do Domain.

A decisão adotada é manter definições separadas.

### Responsabilidade das camadas

- **Communication:** contém os contratos usados para receber e devolver dados da API.
- **Domain:** contém a representação da despesa e os tipos necessários ao seu modelo.

```text
Communication.Enums.PaymentType
                  |
                  | conversão futura
                  v
Domain.Enums.PaymentType
                  |
                  v
           Domain.Entities.Expense
```

### Complemento técnico: tipos distintos

Mesmo que os dois enums possuam os mesmos nomes de membros e os mesmos números, eles são tipos diferentes para o compilador.

Por exemplo:

```text
CashFlow.Communication.Enums.PaymentType
CashFlow.Domain.Enums.PaymentType
```

O fato de ambos se chamarem PaymentType não os torna intercambiáveis automaticamente.

Quando a aplicação converter uma Request em uma entidade, precisará tratar essa correspondência de tipos. Isso poderá ser feito por mapeamento explícito ou por uma ferramenta configurada para mapear os modelos.

### A separação é sempre necessária?

Não. Essa é a decisão arquitetural demonstrada no curso.

Uma arquitetura pode adotar um tipo compartilhado ou outra estratégia de mapeamento. A escolha depende da direção das dependências e do grau de independência desejado entre os contratos externos e o domínio.

Ao duplicar definições, é necessário cuidar para que elas não fiquem inconsistentes quando um dos enums evoluir.

A aula ainda não implementa a conversão entre eles.

## 12. Correspondência entre C# e MySQL

A estrutura final pode ser comparada com o banco criado na aula 85.

| Propriedade C# | Tipo C# | Coluna SQL | Tipo SQL |
|---|---|---|---|
| Id | long | Id | BIGINT |
| Title | string | Title | VARCHAR(255) |
| Description | string? | Description | VARCHAR(2000) |
| Date | DateTime | Date | DATETIME |
| Amount | decimal | Amount | DECIMAL(10,2) |
| PaymentType | enum PaymentType | PaymentType | INT |

### DateTime

```csharp
public DateTime Date { get; set; }
```

Representa a data e hora da despesa, correspondente à coluna DATETIME.

A declaração não verifica se a data está no futuro. Essa é uma regra de negócio já estudada no validator.

### Decimal

```csharp
public decimal Amount { get; set; }
```

O decimal é apropriado para representar valores monetários.

Na tabela, a coluna possui precisão e escala definidas por DECIMAL(10,2), ou seja, dez dígitos no total, incluindo dois após o separador decimal.

**Importante:** o tipo decimal do C# possui seu próprio intervalo e precisão. Declarar decimal não limita automaticamente o valor a DECIMAL(10,2). Essa compatibilidade deverá ser considerada na persistência.

### PaymentType

O enum representa os tipos de pagamento por meio de membros associados a números. Na tabela, a coluna foi criada como INT.

O enum copiado possui o tipo subjacente int no contexto do curso.

O mapeamento posterior deverá representar o tipo de pagamento adequadamente na coluna. Um ORM pode realizar essa conversão conforme sua configuração.

## 13. Fluxo conceitual de persistência e recuperação

O professor explica por que a entidade acompanha os campos da tabela.

### Cadastro

```text
Cliente
   |
   v
Request de cadastro
   |
   v
Validação e conversão dos dados
   |
   v
Expense
   |
   v
Mecanismo de persistência
   |
   v
Tabela expenses
```

### Recuperação

```text
Tabela expenses
   |
   v
Mecanismo de acesso a dados
   |
   v
Expense preenchida
   |
   v
Response
   |
   v
Cliente
```

Esses fluxos ilustram a explicação da aula, mas ainda não são implementados.

**Nuance técnica:** o .NET não passa a converter automaticamente linhas do MySQL em instâncias de Expense apenas porque a classe foi criada. Será necessário configurar uma tecnologia de acesso a dados e seu mapeamento.

Nas próximas etapas, a entidade será utilizada em conjunto com a infraestrutura de persistência.

## 14. Observações técnicas complementares

### 14.1 Entidade não é necessariamente uma cópia literal de uma tabela

No exemplo didático, a entidade acompanha diretamente as seis colunas de expenses. Em modelos mais complexos, propriedades podem receber nomes diferentes, ser ignoradas no mapeamento ou representar relacionamentos.

### 14.2 A classe não cria a tabela

Expense é uma classe C#. A tabela já foi criada manualmente no Workbench. Não existe criação automática de banco por esta classe isoladamente.

### 14.3 Id não é a única forma de identificar uma entidade

O curso utiliza uma chave numérica chamada Id. Outros modelos podem adotar identificadores de tipos diferentes ou chaves compostas.

### 14.4 Restrições e regras de negócio são conceitos diferentes

A classe contém propriedades e anotações de tipo, mas isso não substitui as regras estudadas anteriormente. Por exemplo:

- Title deve ser não vazio.
- Date não pode estar no futuro.
- Amount deve ser maior que zero.
- PaymentType precisa corresponder a um membro válido.

A presença de uma propriedade tipada não garante automaticamente essas condições.

### 14.5 DTOs não precisam expor toda a entidade

A separação entre Request, Response e Expense ajuda a controlar os dados que entram e saem da API. Isso também evita depender da estrutura interna de persistência como contrato público.

### 14.6 Segurança de senhas

O exemplo de usuário da aula serve para mostrar por que os dados recebidos e armazenados podem ser diferentes. Para autenticação, a orientação técnica é utilizar hash de senha adequado, e não armazenar texto puro nem expor o hash em Responses.

Não há código de segurança de senhas nesta aula.

## 15. Resumo e mapa mental

### Principais aprendizados

- A entidade Expense representa uma despesa dentro da aplicação.
- A classe é criada em Domain/Entities/Expense.cs.
- O instrutor altera a visibilidade da classe para public.
- O nome Expense é singular porque cada instância representa uma despesa.
- Id é long e corresponde ao BIGINT da tabela.
- Title é string e recebe string.Empty como valor inicial.
- Description é string?, pois é opcional.
- Date é DateTime.
- Amount é decimal.
- PaymentType é um enum localizado no Domain.
- O enum da Communication é copiado para Domain/Enums/PaymentType.cs.
- Request, Response e entidade possuem responsabilidades diferentes.
- Os dois enums de PaymentType são tipos distintos, mesmo que tenham membros equivalentes.
- A entidade está criada, mas a persistência pela API ainda não foi implementada.

### Perguntas de revisão

1. Qual é a finalidade da entidade Expense no CashFlow?
2. Por que a Request de cadastro não precisa conter Id?
3. Por que a descrição é string? enquanto Title é string?
4. O que acontece com as propriedades quando uma Expense é instanciada sem atribuição de valores?
5. Por que o instrutor cria outro PaymentType no Domain?
6. A criação da entidade já permite inserir dados automaticamente no MySQL? Por quê?

### Mapa mental

```text
                   AULA 87
                       |
                  Domain
                       |
              +--------+--------+
              |                 |
           Entities           Enums
              |                 |
           Expense          PaymentType
              |                 |
              +--------+--------+
                       |
                  Propriedades
                       |
      Id, Title, Description, Date,
            Amount, PaymentType
                       |
                       v
            Correspondência com
               tabela expenses
                       |
                       v
        Futuro mapeamento e persistência

      RESPONSABILIDADES SEPARADAS
       Request | Expense | Response
```

**Conclusão:** esta aula prepara o modelo de despesa que será utilizado na persistência do CashFlow. A classe Expense e seu enum PaymentType passam a integrar a camada Domain, preservando a separação entre a representação interna da despesa e os contratos de comunicação da API.
