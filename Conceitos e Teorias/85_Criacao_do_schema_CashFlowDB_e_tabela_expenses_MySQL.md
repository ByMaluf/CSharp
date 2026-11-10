# Criando o banco CashFlowDB e a tabela expenses no MySQL Workbench

> **Aula 85 — Criação manual do esquema e da tabela de despesas**
>
> Continuação da aula 84, em que instalamos o MySQL Server e o MySQL Workbench. Agora vamos preparar a estrutura relacional que receberá futuramente as despesas cadastradas pela API.

## Índice

1. Objetivo e contexto
2. Criação do esquema no Workbench
3. Banco de dados versus schema
4. Criação da tabela expenses
5. Estrutura completa das colunas
6. Id: BIGINT, chave primária e NOT NULL
7. AUTO_INCREMENT e a geração dos identificadores
8. Title e Description: VARCHAR e nulabilidade
9. Date: DATETIME
10. Amount: DECIMAL e precisão monetária
11. PaymentType: representação do enum
12. Aplicando as alterações e o aviso de nomes minúsculos
13. SQL equivalente à criação visual
14. Consultando a tabela recém-criada
15. Relação com o validator e migrations
16. Cuidados técnicos e resumo

---

## 1. Objetivo e contexto

Até aqui, o CashFlow recebe requisições HTTP, valida os dados e processa as operações da aplicação. Para poder recuperar, atualizar ou excluir uma despesa posteriormente, precisamos de uma estrutura persistente.

Nesta aula, o instrutor cria **manualmente** no MySQL uma tabela para armazenar os registros de despesas.

O trabalho está dividido em três etapas:

- Criar um banco de dados dedicado ao projeto.
- Definir as seis colunas da tabela de despesas, seus tipos e restrições.
- Aplicar a estrutura e conferir se a tabela foi criada.

**Escopo:** não há inserção de registros nesta aula nem integração do banco com a API. O instrutor anuncia que, mais adiante, ensinará a utilizar **migrations** para criar e versionar a estrutura pelo código.

## 2. Criação do esquema no Workbench

Com o MySQL Server em execução e o Workbench conectado, localizar a área **Schemas**, no painel esquerdo.

### Passo a passo apresentado

1. Clicar com o botão direito na área de esquemas.
2. Selecionar **Create Schema**.
3. Informar o nome do banco: `CashFlowDB`.
4. Clicar em **Apply**.
5. Conferir o comando SQL mostrado na janela de confirmação.
6. Clicar novamente em **Apply** e depois em **Finish**.
7. Fechar o editor e expandir o esquema criado.

A transcrição reproduz foneticamente o nome do projeto como "Cache Flow DB". Neste resumo, adotamos `CashFlowDB`, mantendo a grafia do projeto CashFlow. A capitalização efetivamente armazenada pode ser diferente no Windows, como veremos na seção 12.

O esquema criado aparece no navegador do Workbench e contém uma seção **Tables**, inicialmente sem tabelas de despesas.

## 3. Banco de dados versus schema

O instrutor explica que um servidor MySQL pode hospedar vários bancos, permitindo organizar as tabelas de diferentes projetos de maneira separada.

```text
MySQL Server
├── CashFlowDB
│   └── Tables
│       └── expenses
└── OutroProjetoDB (exemplo)
    └── Tables
        └── ...
```

A ideia é reservar um espaço identificado por um nome significativo para reunir os objetos relacionados ao projeto.

### Distinção técnica importante

No **MySQL**, os termos database e schema são sinônimos nesse contexto: `CREATE SCHEMA` e `CREATE DATABASE` são formas equivalentes de criar um banco.

No **SQL Server**, entretanto, database e schema não são exatamente a mesma coisa. Um database pode conter vários schemas, que funcionam como espaços de nomes para organizar objetos. Por exemplo, uma tabela pode ser identificada como `CashFlowDB.dbo.Expenses`.

A analogia da "pastinha" utilizada na aula é útil para entender a organização, mas a terminologia precisa ser interpretada conforme o SGBD.

## 4. Criação da tabela expenses

Dentro do banco recém-criado:

1. Expandir o esquema.
2. Clicar com o botão direito em **Tables**.
3. Selecionar **Create Table**.
4. Informar o nome da tabela: `expenses`.
5. Definir cada coluna no editor de tabelas.

O instrutor prefere nomes no plural porque a tabela armazena várias despesas, e não apenas uma.

### Relação com Entity Framework

O professor menciona que essa convenção será útil posteriormente com Entity Framework.

**Complemento técnico:** no EF Core, o nome padrão da tabela costuma seguir o nome da propriedade `DbSet` que expõe a entidade, ou o nome da classe quando não há um DbSet correspondente. Portanto, o EF Core não pluraliza automaticamente todos os nomes de classes por uma regra universal.

Um exemplo futuro seria uma entidade `Expense` exposta por uma propriedade `Expenses`. Também é possível configurar explicitamente o nome com `ToTable("expenses")`. Isso não é implementado nesta aula.

## 5. Estrutura completa das colunas

A tabela reúne seis propriedades, alinhadas aos dados utilizados pelo cadastro de despesas.

| Coluna | Tipo no MySQL | PK | NN | AI | Finalidade |
|---|---|---|---|---|---|
| Id | BIGINT | Sim | Sim | Sim | Identificador da despesa |
| Title | VARCHAR(255) | Não | Sim | Não | Título obrigatório |
| Description | VARCHAR(2000) | Não | Não | Não | Descrição opcional |
| Date | DATETIME | Não | Sim | Não | Data e hora da despesa |
| Amount | DECIMAL(10,2) | Não | Sim | Não | Valor monetário |
| PaymentType | INT | Não | Sim | Não | Valor numérico do tipo de pagamento |

### Legenda do editor de tabelas

| Sigla | Significado |
|---|---|
| PK | Primary Key — chave primária |
| NN | Not Null — não aceita NULL |
| AI | Auto Increment — geração automática do identificador |

A descrição é a única coluna configurada como opcional na demonstração.

## 6. Id: BIGINT, chave primária e NOT NULL

A primeira coluna criada é `Id`.

O instrutor escolhe:

```sql
Id BIGINT NOT NULL AUTO_INCREMENT
```

Além disso, marca o campo como **PRIMARY KEY**.

### Por que BIGINT?

O instrutor prefere usar `long` para identificadores inteiros no C#. No MySQL, o tipo correspondente em tamanho é `BIGINT`.

| C# | MySQL (com sinal) | Tamanho |
|---|---|---|
| int | INT | 32 bits |
| long | BIGINT | 64 bits |

Um `BIGINT` com sinal oferece um intervalo de aproximadamente -9,22 quintilhões a +9,22 quintilhões. Para identificadores crescentes, isso permite um espaço de valores bastante amplo.

O professor remove os parênteses que aparecem no editor e deixa somente `BIGINT`. Em MySQL, a antiga largura de exibição de inteiros não altera a capacidade de armazenamento do tipo; `BIGINT` não precisa de um número entre parênteses para representar 64 bits.

### Chave primária

A chave primária identifica cada linha de forma única.

- Não permite valores duplicados.
- Não admite NULL.
- Permite referenciar uma despesa individual pelo seu identificador.

Nesta tabela, a chave primária é simples, formada somente pela coluna Id.

**Nuance:** a escolha de um Id numérico para toda entidade é uma convenção adotada pelo instrutor. Há modelos com chaves compostas, naturais ou de outros tipos.

## 7. AUTO_INCREMENT e geração dos identificadores

O instrutor marca **AI (Auto Increment)** para que o próprio MySQL atribua um identificador ao inserir um registro que não informa Id.

### Exemplo da sequência

```text
Inserir despesa A -> Id 1
Inserir despesa B -> Id 2
Inserir despesa C -> Id 3

Excluir a despesa de Id 3.

Inserir despesa D -> Id 4
```

O novo registro não recebe automaticamente o Id 3 apenas porque ele foi excluído.

### Por que isso é útil?

A aplicação não precisa calcular manualmente o próximo número nem consultar o maior Id antes de cada inserção. O banco se encarrega da geração da identidade.

O instrutor mantém a configuração usual de incremento de um em um e comenta que é possível configurar outros intervalos.

### Atenção: auto incremento não garante sequência sem lacunas

É comum haver números ausentes por exclusões, inserções malsucedidas, rollbacks ou outros acontecimentos. Um Id identifica uma linha; não deve ser utilizado como contador confiável da quantidade de despesas.

A afirmação da aula de que "Ids não são reaproveitados" descreve o fluxo normal demonstrado. Ela não é uma garantia absoluta diante de operações administrativas, redefinição de contadores, truncamento da tabela ou atribuição explícita de identificadores.

## 8. Title e Description: VARCHAR e nulabilidade

### Title

O professor inicialmente observa o tipo `VARCHAR(45)` no Workbench e altera o tamanho para 255:

```sql
Title VARCHAR(255) NOT NULL
```

VARCHAR armazena texto de tamanho variável até o limite declarado. O número 255 representa o limite de caracteres, sujeito também às restrições de tamanho em bytes e ao conjunto de caracteres utilizado.

O título é obrigatório no cadastro, portanto a coluna recebe NOT NULL.

O professor sugere que, posteriormente, o validator da API passe a conferir o tamanho máximo do título para evitar que dados incompatíveis cheguem ao banco.

### Description

A descrição recebe um limite maior:

```sql
Description VARCHAR(2000) NULL
```

O instrutor escolhe 2.000 caracteres e não marca NN, pois a descrição é opcional.

| Valor | Title | Description |
|---|---|---|
| Texto dentro do limite | Aceito pelo tipo | Aceito pelo tipo |
| NULL | Rejeitado | Permitido |
| String vazia | Pode ser aceita pelo banco | Pode ser aceita pelo banco |

**Importante:** NOT NULL não significa "texto não vazio". Uma string vazia não é NULL. A regra `NotEmpty()` que estudamos no FluentValidation continua sendo relevante para rejeitar títulos vazios ou compostos apenas por espaços.

## 9. Date: DATETIME

A coluna representa a data da despesa.

```sql
`Date` DATETIME NOT NULL
```

O tipo DATETIME armazena data e hora. O instrutor marca NN porque a data é obrigatória.

Exemplo de representação:

```text
2026-11-10 14:30:00
```

**Complemento técnico:** o tipo DATETIME não aplica, sozinho, a regra de que uma despesa não pode estar no futuro. Essa regra continua sendo responsabilidade da validação da aplicação, salvo se forem adicionados outros mecanismos adequados no banco.

Além disso, DATETIME não armazena um fuso horário associado ao valor. Uma aplicação que trabalha com UTC deve definir uma convenção consistente para persistir e recuperar essas datas.

## 10. Amount: DECIMAL e precisão monetária

O professor define o valor da despesa como:

```sql
Amount DECIMAL(10,2) NOT NULL
```

A escolha de DECIMAL é apropriada para valores monetários porque é um tipo numérico de precisão exata, ao contrário dos tipos de ponto flutuante aproximados.

### Entendendo DECIMAL(p,s)

**Correção técnica importante da explicação oral:** o instrutor descreve o primeiro parâmetro como a quantidade de dígitos antes da vírgula. No MySQL, ele representa a quantidade **total** de dígitos, incluindo os decimais.

| Parâmetro | Significado |
|---|---|
| p — precisão | Quantidade total de dígitos |
| s — escala | Quantidade de dígitos após o separador decimal |
| p - s | Quantidade máxima de dígitos da parte inteira |

Assim:

```text
DECIMAL(10,2)
     |  |
     |  +-- 2 casas decimais
     +----- 10 dígitos no total

Parte inteira: até 8 dígitos
Parte decimal: 2 dígitos
```

O intervalo do tipo com sinal vai de **-99.999.999,99 a 99.999.999,99**. Se quiséssemos dez dígitos inteiros e duas casas decimais, precisaríamos de `DECIMAL(12,2)`.

O resumo mantém o tipo **DECIMAL(10,2)** escolhido na gravação, corrigindo somente sua interpretação.

### Exemplos

| Valor escrito em SQL | Representável exatamente no tipo? |
|---|---|
| 100.50 | Sim |
| 0.01 | Sim |
| 99999999.99 | Sim |
| 100000000.00 | Não, excede a precisão |
| 1.234 | Não com as três casas decimais; haverá conversão à escala |

### Relação com a validação

NOT NULL exige que o valor esteja presente, mas DECIMAL(10,2) aceita números negativos e zero. O teste do validator estudado anteriormente verifica `GreaterThan(0)`. Não devemos confundir o tipo da coluna com a regra de negócio que exige valor positivo.

## 11. PaymentType: representação do enum

A última coluna criada é:

```sql
PaymentType INT NOT NULL
```

O instrutor lembra que o enum C# atribui nomes significativos a valores numéricos. No caso do PaymentType do curso, o tipo subjacente é int.

A coluna INT armazenará o código numérico do tipo de pagamento, e não necessariamente o texto do nome do membro.

### Exemplo conceitual

```text
PaymentType (enum C#)
         |
         v
Código inteiro correspondente
         |
         v
Coluna PaymentType INT no MySQL
```

A aula não define novos valores do enum nem altera os membros existentes.

**Atenção:** a coluna INT não sabe automaticamente quais números são membros válidos do enum. O FluentValidation utiliza `IsInEnum()` para verificar essa condição. Caso desejado futuramente, também é possível estabelecer restrições no banco.

## 12. Aplicando as alterações e o aviso de nomes minúsculos

Depois de configurar as seis colunas, o instrutor:

1. Clica em **Apply**.
2. Aceita o aviso sobre a conversão do nome da tabela para minúsculas.
3. Revisa o SQL gerado pelo Workbench, especialmente tipos, NOT NULL e AUTO_INCREMENT.
4. Clica novamente em **Apply** e depois em **Finish**.
5. Fecha o editor e expande Tables para conferir a tabela criada.

### Por que o nome fica minúsculo?

**Correção técnica:** a gravação atribui esse comportamento ao perfil de desenvolvimento selecionado na instalação e sugere que os nomes ficam maiúsculos em produção. Essa explicação não é correta.

No MySQL, o comportamento depende da plataforma, do sistema de arquivos e da variável de configuração `lower_case_table_names`.

| Valor | Comportamento |
|---|---|
| 0 | Preserva a grafia e compara nomes de tabelas com distinção de maiúsculas/minúsculas |
| 1 | Armazena nomes de tabelas em minúsculas e compara sem distinguir maiúsculas/minúsculas |
| 2 | Preserva a grafia na criação e compara sem distinguir maiúsculas/minúsculas, com particularidades do mecanismo de armazenamento |

No Windows, o padrão costuma ser 1; no Linux, 0. Isso explica a conversão para minúsculas observada no ambiente da aula.

Para conferir a configuração do servidor, pode-se executar (exemplo complementar):

```sql
SHOW VARIABLES LIKE 'lower_case_table_names';
```

Não há uma convenção do MySQL que utilize nomes maiúsculos para identificar produção. Usar nomes consistentes, como `expenses`, ajuda na portabilidade. No MySQL 8, essa configuração deve ser definida na inicialização do servidor e não deve ser alterada arbitrariamente em uma instância já inicializada.

## 13. SQL equivalente à criação visual

O instrutor cria os objetos pelo editor gráfico do Workbench. O SQL abaixo é uma **reconstrução didática equivalente**, não uma transcrição literal da janela de confirmação.

```sql
CREATE DATABASE IF NOT EXISTS cashflowdb;

USE cashflowdb;

CREATE TABLE expenses (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    Title VARCHAR(255) NOT NULL,
    Description VARCHAR(2000) NULL,
    `Date` DATETIME NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentType INT NOT NULL,
    PRIMARY KEY (Id)
);
```

Neste exemplo, usamos o nome do banco em minúsculas por consistência com o comportamento observado no Windows. O nome lógico escolhido pelo instrutor é CashFlowDB.

### O que cada comando faz?

- CREATE DATABASE: cria o banco, caso ainda não exista.
- USE: seleciona o banco para os comandos seguintes.
- CREATE TABLE: define a estrutura da tabela.
- PRIMARY KEY: estabelece Id como identificador único.
- AUTO_INCREMENT: permite gerar automaticamente o Id de novas linhas.
- NOT NULL: impede valores nulos nas colunas obrigatórias.

**Cuidado:** se a tabela já foi criada pelo Workbench, não é necessário executar novamente esse CREATE TABLE. O objetivo do trecho é compreender o que a interface gráfica está fazendo.

## 14. Consultando a tabela recém-criada

Após a criação, o instrutor clica com o botão direito sobre a tabela e escolhe a opção de selecionar linhas.

O Workbench gera uma consulta. De forma equivalente:

```sql
SELECT *
FROM expenses;
```

A estrutura já existe, mas o resultado ainda não contém registros, pois nenhuma despesa foi inserida.

A existência de uma tabela vazia é normal: primeiro definimos o esquema, depois passamos a persistir os dados.

## 15. Relação com o validator e migrations

As aulas anteriores implementaram e testaram regras de validação para o cadastro de despesas. Agora existe também uma estrutura de armazenamento.

| Regra ou necessidade | Aplicação | Banco criado |
|---|---|---|
| Título obrigatório | NotEmpty() | NOT NULL |
| Limite do título | Sugestão de validação futura | VARCHAR(255) |
| Descrição opcional | Propriedade da request | VARCHAR(2000) NULL |
| Data não futura | Regra de validação | DATETIME NOT NULL |
| Valor positivo | GreaterThan(0) | DECIMAL(10,2) NOT NULL |
| Tipo de pagamento válido | IsInEnum() | INT NOT NULL |

As restrições do banco e as validações da aplicação são complementares, não equivalentes. O banco criado na aula ainda não impõe todas as regras de negócio.

### O que são migrations?

O professor anuncia que, futuramente, ensinaremos a criar e versionar alterações na estrutura por meio de migrations.

A ideia é registrar as mudanças necessárias no banco e aplicá-las de maneira controlada, em vez de depender de alterações manuais em cada ambiente.

**Nuance técnica:** executar a API não aplica migrations automaticamente em qualquer aplicação. Esse comportamento exige configuração ou uma etapa explícita de implantação. O curso ensinará o procedimento posteriormente.

Neste vídeo, não é criada nenhuma migration, entidade de persistência ou DbContext.

## 16. Cuidados técnicos e resumo

### Pontos principais

- Um servidor pode conter diferentes bancos de dados.
- No MySQL, schema e database são sinônimos; no SQL Server, schemas existem dentro de databases.
- A tabela `expenses` contém seis colunas.
- Id é BIGINT, chave primária, NOT NULL e AUTO_INCREMENT.
- Title é VARCHAR(255) obrigatório; Description é VARCHAR(2000) opcional.
- Date é DATETIME obrigatório.
- Amount é DECIMAL(10,2) obrigatório: **oito dígitos inteiros e duas casas decimais**.
- PaymentType é INT obrigatório, correspondente ao código numérico do enum.
- A conversão para minúsculas está associada à configuração do MySQL e à plataforma, não ao perfil de desenvolvimento.
- A tabela está criada e vazia ao final da aula.

### Mapa mental

```text
              MySQL Server
                    |
                CashFlowDB
                    |
                 expenses
                    |
     +--------------+-----------------+
     |              |                 |
 Id BIGINT       Title VARCHAR     Description
 PK + NN + AI    255 + NN          VARCHAR 2000
     |              |                 |
     +--------------+-----------------+
                    |
       +------------+-------------+
       |            |             |
     Date         Amount       PaymentType
   DATETIME     DECIMAL(10,2)     INT
      NN            NN            NN
                    |
             Tabela criada
                    |
             Ainda sem dados
                    |
               Próxima aula
             Inserir registros
```

### Checklist

- [ ] Criar o esquema CashFlowDB.
- [ ] Criar a tabela expenses.
- [ ] Conferir os tipos e as seis colunas.
- [ ] Marcar PK, NN e AI no Id.
- [ ] Permitir NULL apenas em Description.
- [ ] Revisar o SQL antes de aplicar.
- [ ] Confirmar a criação e consultar a tabela.

### Referências técnicas complementares

- [MySQL 8.0 — DECIMAL: precisão e escala](https://dev.mysql.com/doc/refman/8.0/en/fixed-point-types.html)
- [MySQL 8.0 — Variáveis do servidor, lower_case_table_names](https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html)
- [Microsoft Learn — Mapeamento de entidades e nomes de tabelas no EF Core](https://learn.microsoft.com/pt-br/ef/core/modeling/entity-types)

**Conclusão:** a aula define manualmente a primeira estrutura de persistência do CashFlow e prepara o banco para receber despesas. A próxima aula realizará a inserção de dados nessa tabela.
