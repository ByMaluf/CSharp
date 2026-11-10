# Operações SQL no MySQL Workbench: SELECT, INSERT, UPDATE e DELETE

> **Aula 86 — Manipulando os registros da tabela expenses**
>
> Continuação da aula 85, na qual criamos o banco CashFlowDB e a tabela expenses. Nesta aula, vamos inserir, consultar, excluir e atualizar despesas manualmente pelo MySQL Workbench.

## Índice

1. Objetivo da aula
2. Relembrando a estrutura da tabela
3. SELECT: consultando registros
4. INSERT INTO: inserindo dados
5. A ordem das colunas e dos valores
6. Inserindo a primeira despesa
7. Executando comandos no MySQL Workbench
8. Inserindo Netflix sem descrição
9. Ordem de execução: INSERT antes ou depois do SELECT
10. DELETE: excluindo uma despesa
11. UPDATE: alterando uma despesa
12. Estado final da tabela
13. CRUD e relação com a API
14. Cuidados técnicos
15. Resumo e revisão

---

## 1. Objetivo da aula

Na aula anterior, criamos a estrutura necessária para guardar despesas no MySQL. Agora vamos utilizar essa estrutura para praticar comandos SQL, chamados informalmente de queries pelo instrutor.

O vídeo apresenta quatro operações:

| Operação | Comando | Finalidade |
|---|---|---|
| Consulta | SELECT | Recuperar registros |
| Inserção | INSERT INTO | Cadastrar novos registros |
| Exclusão | DELETE FROM | Remover registros |
| Atualização | UPDATE | Alterar registros existentes |

Essas operações correspondem ao **CRUD**: Create, Read, Update e Delete.

**Escopo:** todas as operações são realizadas diretamente no Workbench, sem conexão da API C# ao banco nesta aula.

## 2. Relembrando a estrutura da tabela

A tabela criada na aula 85 contém:

| Coluna | Tipo MySQL | Característica |
|---|---|---|
| Id | BIGINT | Chave primária, AUTO_INCREMENT |
| Title | VARCHAR(255) | Obrigatória |
| Description | VARCHAR(2000) | Opcional |
| Date | DATETIME | Obrigatória |
| Amount | DECIMAL(10,2) | Obrigatória |
| PaymentType | INT | Obrigatória |

O nome do banco e da tabela pode ser usado junto, separado por um ponto:

```sql
cashflowdb.expenses
```

Essa é uma referência qualificada: `cashflowdb` identifica o banco e `expenses` identifica a tabela.

Neste resumo, os nomes são escritos em minúsculas para manter consistência com o ambiente Windows da aula anterior. As consultas são reconstruções didáticas dos comandos narrados na transcrição. Os literais de texto usam aspas simples, que são a forma usual em SQL.

## 3. SELECT: consultando registros

O instrutor começa com a consulta gerada pelo Workbench na aula anterior:

```sql
SELECT *
FROM cashflowdb.expenses;
```

### Entendendo o comando

- SELECT indica quais informações queremos recuperar.
- O asterisco (*) representa todas as colunas.
- FROM indica de qual tabela serão recuperados os dados.
- O ponto e vírgula final delimita a instrução.

A primeira execução retorna uma tabela vazia, porque ainda não foram cadastradas despesas.

```text
SELECT *
     |
     v
cashflowdb.expenses
     |
     v
Colunas da tabela, inicialmente sem registros
```

O SELECT permite consultar o resultado, mas não altera os registros.

### Complemento técnico

Também é possível selecionar apenas determinadas colunas:

```sql
SELECT Title, Amount
FROM cashflowdb.expenses;
```

Essa segunda consulta é um exemplo complementar para compreender o asterisco; não é executada na gravação.

## 4. INSERT INTO: inserindo dados

O comando de inserção apresentado é INSERT INTO.

Uma forma geral é:

```sql
INSERT INTO cashflowdb.expenses
VALUES (...);
```

Quando não especificamos as colunas, os valores precisam corresponder às colunas da tabela na ordem definida pelo esquema.

No CashFlow, a ordem apresentada na aula anterior foi:

```text
Id, Title, Description, Date, Amount, PaymentType
```

O instrutor escolhe uma segunda forma: **especificar explicitamente as colunas que receberão os dados**.

```sql
INSERT INTO cashflowdb.expenses
    (Title, Date, Description, PaymentType, Amount)
VALUES (...);
```

### Por que informar as colunas?

Porque isso permite:

- escolher a ordem em que os valores serão fornecidos;
- omitir o Id, que é gerado pelo AUTO_INCREMENT;
- omitir Description quando a despesa não tiver descrição.

O comando fica mais claro, pois deixa explícito qual valor pertence a qual coluna.

## 5. A ordem das colunas e dos valores

Este é um dos conceitos fundamentais da aula.

Na lista de colunas, a ordem não precisa ser igual à ordem física da tabela.

Entretanto, **os valores de VALUES devem seguir exatamente a ordem da lista de colunas escolhida no INSERT**.

```sql
INSERT INTO cashflowdb.expenses
    (Title, Date, Description, PaymentType, Amount)
VALUES
    ('iPhone 15', '2023-11-10 14:30:00', 'ABCD1234', 1, 1350.99);
```

A correspondência é feita pela posição:

| Posição | Coluna | Valor |
|---:|---|---|
| 1 | Title | iPhone 15 |
| 2 | Date | 2023-11-10 14:30:00 |
| 3 | Description | ABCD1234 |
| 4 | PaymentType | 1 |
| 5 | Amount | 1350.99 |

Se alterarmos a ordem das colunas, devemos reorganizar os valores da mesma maneira.

### Representação visual

```text
(Title,   Date,   Description,   PaymentType,   Amount)
   |        |          |              |           |
   v        v          v              v           v
(valor1,  valor2,    valor3,        valor4,     valor5)
```

O banco não associa os valores pelo significado do conteúdo. A associação ocorre pela posição correspondente à coluna declarada.

## 6. Inserindo a primeira despesa

O primeiro registro do vídeo é uma despesa relacionada a um iPhone 15.

O comando reconstruído é:

```sql
INSERT INTO cashflowdb.expenses
    (Title, Date, Description, PaymentType, Amount)
VALUES
    ('iPhone 15', '2023-11-10 14:30:00', 'ABCD1234', 1, 1350.99);
```

### Detalhes dos valores

**Title e Description**

São textos. A gravação usa aspas duplas; neste resumo, usamos aspas simples, que são a sintaxe usual para literais de texto em SQL.

**Date**

A data segue o formato ano-mês-dia, seguido opcionalmente da hora, conforme o tipo da coluna:

```text
YYYY-MM-DD HH:MM:SS
2023-11-10 14:30:00
```

A data da demonstração é 10 de novembro de 2023, às 14h30.

**PaymentType**

Recebe o número 1, pois a coluna é INT. O instrutor comenta que não recorda qual membro específico do enum corresponde a esse valor. O resumo não atribui um nome ao código sem confirmação na gravação.

**Amount**

Recebe 1350.99. O separador decimal no literal SQL é o ponto.

**Id**

Não aparece na lista de colunas. O MySQL gera seu valor automaticamente.

### Resultado observado

Após executar o INSERT, o Workbench informa que uma linha foi afetada. Ao consultar a tabela, o professor observa a primeira despesa com Id 1.

```text
Id:          1
Title:       iPhone 15
Description: ABCD1234
Date:        2023-11-10 14:30:00
Amount:      1350.99
PaymentType: 1
```

O valor de Id 1 corresponde ao estado inicial da tabela demonstrada; não é uma garantia para qualquer tabela já utilizada.

## 7. Executando comandos no MySQL Workbench

O instrutor utiliza o ícone de raio para executar instruções SQL.

### Executar uma instrução selecionada

Quando há várias instruções no editor e selecionamos apenas o texto de uma delas, o comando de execução demonstrado processa somente a seleção.

Exemplo:

```sql
SELECT * FROM cashflowdb.expenses;

INSERT INTO cashflowdb.expenses (Title, Date, PaymentType, Amount)
VALUES ('Netflix', '2023-11-10 14:30:00', 2, 747);
```

Se selecionarmos apenas o SELECT, a consulta é executada sem inserir outra despesa.

Se selecionarmos apenas o INSERT, somente a inserção é realizada.

### Executar tudo

Quando não há seleção, o botão mostrado pelo instrutor executa o conteúdo do editor de cima para baixo.

A área de resultados da execução informa as instruções processadas e se foram concluídas com sucesso.

**Atenção:** o Workbench possui diferentes comandos de execução, inclusive execução da instrução atual. A regra acima descreve o botão utilizado na demonstração, não todos os ícones de execução da ferramenta.

### Resultado visual

O professor observa:

- indicação verde de execução bem-sucedida;
- a mensagem de uma linha afetada após a inserção;
- o resultado tabular do SELECT;
- destaque de erro de sintaxe quando uma instrução é digitada incorretamente.

## 8. Inserindo Netflix sem descrição

O segundo exemplo é uma despesa chamada Netflix.

O instrutor remove Description tanto da lista de colunas quanto da lista de valores. Isso é possível porque a coluna foi criada como opcional na aula anterior.

```sql
INSERT INTO cashflowdb.expenses
    (Title, Date, PaymentType, Amount)
VALUES
    ('Netflix', '2023-11-10 14:30:00', 2, 747);
```

O valor 747 e o código de pagamento 2 são dados demonstrativos utilizados pelo professor.

Como Description foi omitida, o banco utiliza o valor padrão definido para a coluna. Na estrutura da aula, ela admite NULL e não tem outro valor padrão indicado, portanto a descrição pode ficar NULL.

### Estado após a segunda inserção

| Id | Title | Description | Amount | PaymentType |
|---:|---|---|---:|---:|
| 1 | iPhone 15 | ABCD1234 | 1350.99 | 1 |
| 2 | Netflix | NULL | 747.00 | 2 |

A tabela acima omite Date apenas para facilitar a leitura. Os comandos incluem a data obrigatória.

**Ponto-chave:** omitir uma coluna opcional na lista do INSERT não significa que podemos omitir um valor intermediário e deixar as posições desalinhadas. Removemos a coluna e seu valor correspondente.

## 9. Ordem de execução: INSERT antes ou depois do SELECT

O instrutor demonstra por que o resultado da consulta depende da ordem dos comandos.

### Caso A — SELECT antes do INSERT

```sql
SELECT * FROM cashflowdb.expenses;

INSERT INTO cashflowdb.expenses
    (Title, Date, PaymentType, Amount)
VALUES
    ('Netflix', '2023-11-10 14:30:00', 2, 747);
```

Se a tabela possui apenas o iPhone 15 antes da execução:

1. SELECT consulta a tabela e recupera somente o iPhone.
2. INSERT cria a despesa Netflix.
3. O resultado exibido pelo SELECT anterior continua contendo somente o iPhone.

Isso não significa que o INSERT falhou. Significa que a consulta foi feita **antes da inserção**.

Ao executar novamente apenas o SELECT, os dois registros aparecem.

### Caso B — INSERT antes do SELECT

O instrutor muda o título para YouTube Premium e utiliza o valor 10.29.

```sql
INSERT INTO cashflowdb.expenses
    (Title, Date, PaymentType, Amount)
VALUES
    ('YouTube Premium', '2023-11-10 14:30:00', 2, 10.29);

SELECT * FROM cashflowdb.expenses;
```

Agora a inserção acontece antes da consulta.

O SELECT mostra as três despesas:

| Id | Title | Amount |
|---:|---|---:|
| 1 | iPhone 15 | 1350.99 |
| 2 | Netflix | 747.00 |
| 3 | YouTube Premium | 10.29 |

### Comparação

```text
SELECT -> INSERT
Consulta antiga, depois nova despesa

INSERT -> SELECT
Nova despesa, depois consulta atualizada
```

O resultado do SELECT não se atualiza automaticamente só porque um comando posterior alterou a tabela.

**Nota técnica:** a sequência dos comandos é diferente da ordem das linhas retornadas por SELECT. Sem ORDER BY, o SQL não garante que os registros sejam apresentados em ordem de Id. A tabela acima ilustra os registros observados na aula.

## 10. DELETE: excluindo uma despesa

O instrutor passa para a operação de exclusão.

### Cuidado com DELETE sem WHERE

```sql
-- Exemplo de risco; não execute se deseja preservar os registros.
DELETE FROM cashflowdb.expenses;
```

Sem uma condição WHERE, a instrução solicita a exclusão de todos os registros da tabela.

O professor chama atenção para o conhecido problema de executar um **DELETE sem WHERE**.

A intenção da aula é remover somente Netflix, identificada por Id 2.

### Comando correto para o cenário

```sql
DELETE FROM cashflowdb.expenses
WHERE Id = 2;
```

A cláusula WHERE define quais linhas podem ser afetadas.

Como Id é a chave primária, existe no máximo um registro correspondente ao valor 2.

### Conferindo a exclusão

```sql
SELECT * FROM cashflowdb.expenses;
```

O resultado mostrado passa a conter:

| Id | Title |
|---:|---|
| 1 | iPhone 15 |
| 3 | YouTube Premium |

Netflix deixa de existir na tabela.

Observe que os identificadores restantes não são renumerados. Isso se relaciona ao conceito de AUTO_INCREMENT explicado na aula 85.

### Prática complementar

Antes de excluir dados, podemos conferir a condição usando SELECT:

```sql
SELECT *
FROM cashflowdb.expenses
WHERE Id = 2;
```

Essa consulta permite verificar qual registro corresponde ao filtro antes de realizar a operação destrutiva.

## 11. UPDATE: alterando uma despesa

A última operação praticada é UPDATE.

O instrutor quer corrigir a descrição e o valor da despesa iPhone 15.

### Estrutura do comando

```sql
UPDATE cashflowdb.expenses
SET Description = 'Gift for my birthday',
    Amount = 1299
WHERE Id = 1;
```

### Entendendo cada parte

| Trecho | Significado |
|---|---|
| UPDATE cashflowdb.expenses | Indica a tabela a alterar |
| SET Description = ... | Define o novo texto da descrição |
| Amount = 1299 | Define o novo valor |
| WHERE Id = 1 | Restringe a alteração ao registro escolhido |

É possível atualizar várias colunas em uma única instrução, separando as atribuições por vírgulas.

### Resultado esperado

| Campo | Antes | Depois |
|---|---|---|
| Description | ABCD1234 | Gift for my birthday |
| Amount | 1350.99 | 1299.00 |

O Id e os outros campos permanecem os mesmos.

### Cuidado com UPDATE sem WHERE

```sql
-- Exemplo de risco, não executado na aula.
UPDATE cashflowdb.expenses
SET Description = 'Gift for my birthday';
```

Sem WHERE, a alteração não fica restrita ao iPhone. Todas as linhas elegíveis da tabela podem receber a mesma descrição.

Por isso, o instrutor reforça a importância de especificar o filtro tanto no DELETE quanto no UPDATE.

## 12. Estado final da tabela

Ao final da demonstração, a consulta mostra duas despesas.

| Id | Title | Description | Amount |
|---:|---|---|---:|
| 1 | iPhone 15 | Gift for my birthday | 1299.00 |
| 3 | YouTube Premium | NULL | 10.29 |

A tabela de revisão mostra somente os campos relevantes para acompanhar as mudanças. Date e PaymentType continuam presentes na estrutura original.

### Sequência de alterações

```text
Tabela vazia
     |
     v
INSERT iPhone 15 (Id 1)
     |
     v
INSERT Netflix (Id 2)
     |
     v
INSERT YouTube Premium (Id 3)
     |
     v
DELETE Netflix (Id 2)
     |
     v
UPDATE iPhone (Id 1)
     |
     v
Tabela com Id 1 e Id 3
```

A aula demonstra todas as quatro operações básicas sobre os dados, sem alterar a estrutura da tabela.

## 13. CRUD e relação com a API

As operações praticadas correspondem ao CRUD:

| CRUD | SQL | Exemplo na aula |
|---|---|---|
| Create | INSERT | Inserir iPhone, Netflix e YouTube Premium |
| Read | SELECT | Visualizar despesas |
| Update | UPDATE | Alterar descrição e valor do iPhone |
| Delete | DELETE | Excluir Netflix |

### O que vem depois?

O professor explica que, quando a API for integrada ao banco, não será necessário escrever manualmente cada instrução SQL no código da mesma maneira praticada nesta aula.

Ele anuncia uma ferramenta que funcionará como uma espécie de **tradutor** entre operações sobre entidades e comandos do banco de dados.

Essa ideia se relaciona ao uso de um ORM (Object-Relational Mapper), como Entity Framework Core, já mencionado na aula anterior. Um ORM permite trabalhar com objetos e operações de persistência que podem ser traduzidos para SQL.

**Complemento técnico:** isso não significa que a API deixa de utilizar SQL internamente. O banco relacional continua processando instruções SQL. Também existem situações em que aplicações utilizam SQL diretamente.

O vídeo não instala nem configura esse mecanismo. A implementação fica para as próximas aulas.

## 14. Cuidados técnicos

Os pontos abaixo complementam a gravação, sem modificar os exemplos que o professor demonstrou.

### 14.1 Aspas para textos e datas

O vídeo utiliza aspas duplas em alguns literais. No MySQL, aspas duplas podem funcionar como delimitadores de texto na configuração padrão, mas seu significado muda quando o modo ANSI_QUOTES está ativo.

Aspas simples são a forma recomendada nos exemplos deste resumo.

### 14.2 O tipo da coluna não substitui a validação

A coluna PaymentType é INT, mas isso não verifica automaticamente se o código existe no enum do C#. Da mesma maneira, Amount é DECIMAL(10,2), mas o tipo sozinho não exige um valor positivo.

As regras do validator estudado nas aulas anteriores continuam relevantes.

### 14.3 WHERE e segurança de execução

O efeito lógico de um UPDATE ou DELETE sem WHERE é atingir todas as linhas elegíveis. Entretanto, o MySQL Workbench pode estar configurado com Safe Updates, e o servidor pode rejeitar determinadas operações inseguras.

Não desative proteções apenas para executar os exemplos de risco. Confira o filtro e o ambiente antes de modificar dados.

### 14.4 Affected rows

A mensagem de sucesso e a quantidade de linhas afetadas ajudam a verificar o resultado de uma instrução. Um UPDATE pode encontrar uma linha cujo valor já seja igual ao informado; a interpretação da contagem depende do comando e da configuração do cliente.

### 14.5 Executar novamente não é o mesmo que apenas consultar

Executar de novo um script que contém INSERT pode criar outro registro. A seleção de instruções no Workbench ajuda a evitar inserções ou alterações não desejadas durante a revisão.

### 14.6 Ordem e transações

Os exemplos assumem a execução bem-sucedida, com o comportamento usual de autocommit da sessão. Em ambientes com transações explícitas, a confirmação das alterações e sua visibilidade para outras sessões dependem do contexto transacional. Esse tema não é apresentado na aula.

## 15. Resumo e revisão

### Principais aprendizados

- SELECT recupera dados da tabela; o asterisco seleciona todas as colunas.
- INSERT INTO adiciona registros.
- A lista explícita de colunas permite omitir Id e Description.
- Os valores de VALUES devem corresponder à ordem da lista de colunas.
- Id é gerado automaticamente pelo AUTO_INCREMENT.
- A coluna Date recebe uma data/hora no formato adequado ao MySQL.
- Amount é escrito com ponto como separador decimal no SQL.
- O Workbench permite executar um trecho selecionado ou o conjunto de instruções.
- A ordem dos comandos determina se o SELECT ocorre antes ou depois de uma inserção.
- DELETE com WHERE Id = 2 remove somente a despesa Netflix.
- UPDATE com SET altera uma ou mais colunas e WHERE restringe a operação.
- Ao final, permanecem iPhone 15 e YouTube Premium.
- A próxima etapa será trabalhar a persistência pela API por meio de um mecanismo de tradução das operações sobre entidades.

### Perguntas de revisão

1. Por que podemos omitir Id no INSERT?
2. A ordem das colunas precisa seguir a estrutura original da tabela?
3. Por que VALUES precisa seguir a ordem da lista declarada?
4. O que acontece quando SELECT é executado antes de INSERT?
5. Por que WHERE é essencial ao excluir ou atualizar uma despesa específica?
6. Como alterar duas colunas com apenas um UPDATE?

### Mapa mental

```text
                  AULA 86
                     |
               cashflowdb.expenses
                     |
        +------------+------------+
        |                         |
      SELECT                    INSERT
     consulta                  cadastro
        |                         |
        +------------+------------+
                     |
        +------------+------------+
        |                         |
      DELETE                    UPDATE
      exclusão                  alteração
        |                         |
        +------------+------------+
                     |
                   WHERE
             delimita os registros
                     |
                     v
             CRUD praticado no
               MySQL Workbench
                     |
                     v
             Integração futura
                 com a API
```

**Conclusão:** esta aula ensina a manipular registros diretamente com SQL, compreender a ordem das instruções e limitar operações com WHERE. A experiência prepara a transição para a persistência de dados por meio das entidades e ferramentas de acesso a banco utilizadas pela API.
