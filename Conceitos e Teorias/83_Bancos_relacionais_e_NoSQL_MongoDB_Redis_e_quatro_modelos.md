# Bancos de dados relacionais e não relacionais: SQL, NoSQL e seus quatro modelos

> **Aula 83 — Diferenças entre bancos de dados relacionais e não relacionais**
>
> Continuação da aula 82, que apresentou o conceito de banco de dados, a analogia do armário e as cinco vantagens de organizar informações digitais.

## Índice

1. Objetivo da aula
2. Duas categorias apresentadas: relacional e não relacional
3. Bancos de dados relacionais
4. O que significa seguir uma estrutura?
5. Exemplo: tabela de usuários
6. A linguagem SQL
7. Bancos não relacionais e a flexibilidade dos documentos
8. MongoDB: database, collection e document
9. Exemplo da aula: dois documentos diferentes
10. A propriedade _id no MongoDB
11. Os quatro modelos NoSQL apresentados
12. Documentos
13. Grafos
14. Chave-valor e Redis
15. Wide-column store
16. Comparação entre bancos relacionais e NoSQL
17. Conexão com o CashFlow
18. Observações técnicas importantes
19. Resumo e revisão
20. Mapa mental

---

## 1. Objetivo da aula

Na aula anterior, aprendemos que um **banco de dados** é uma coleção organizada de informações armazenadas digitalmente. Nesta aula, o instrutor explica que essa organização **não precisa ser igual em todos os sistemas**.

A distinção central apresentada é:

- **Bancos de dados relacionais**, que organizam informações em tabelas com uma estrutura definida.
- **Bancos de dados não relacionais**, também chamados de **NoSQL**, que incluem diferentes modelos de armazenamento e podem oferecer outras formas de estruturar os dados.

Para explicar a diferença, o vídeo utiliza exemplos de **MySQL**, **SQL Server**, **MongoDB** e **Redis**.

Ao final, apresenta quatro categorias importantes de bancos NoSQL: **documentos, grafos, chave-valor e wide-column store**.

Esta é uma aula **conceitual e comparativa**. A transcrição não apresenta configuração de conexão com a API nem código C# para persistir registros.

---

## 2. Duas categorias apresentadas: relacional e não relacional

O instrutor organiza a explicação em duas grandes categorias.

```text
                BANCOS DE DADOS
                       |
           +-----------+-----------+
           |                       |
           v                       v
      RELACIONAIS             NÃO RELACIONAIS
           |                     (NoSQL)
           v                       |
    Tabelas com estrutura          +-------------------+
    definida                       |                   |
           |                       v                   v
     MySQL e SQL Server        Documentos        Outros modelos
                                   |                   |
                                MongoDB          Grafos, chave-valor
                                                 e wide-column
```

A diferença abordada não é simplesmente se o sistema consegue armazenar muitos dados; ambos podem fazê-lo.

**O ponto principal é a maneira como as informações são organizadas e quais regras estruturais são adotadas.**

> **Nota técnica:** essa divisão é útil para estudar os conceitos apresentados, mas não deve ser entendida como uma taxonomia exaustiva de todas as tecnologias de armazenamento existentes.

---

## 3. Bancos de dados relacionais

O instrutor cita dois exemplos de tecnologias relacionais:

- **MySQL**.
- **SQL Server**.

Ambos são sistemas gerenciadores de bancos de dados relacionais (*SGBDRs*).

### Ideia central

Em um banco relacional, organizamos os dados em **tabelas**, cujas colunas possuem uma definição.

Uma tabela pode estabelecer:

- o nome das colunas;
- o tipo de dado aceito por cada coluna;
- restrições sobre valores permitidos;
- outras regras de integridade, conforme a configuração.

Essa definição é frequentemente chamada de **esquema (*schema*)**.

### Por que isso importa?

Porque os dados inseridos precisam ser compatíveis com a estrutura definida.

No vídeo, a explicação é: **não podemos fornecer qualquer informação em qualquer formato e esperar que o banco a grave automaticamente em uma tabela já estruturada**.

---

## 4. O que significa seguir uma estrutura?

Imagine uma tabela destinada a armazenar usuários de uma plataforma.

A tabela pode ter os campos:

| Coluna | Tipo de dado | Exemplo |
|---|---|---|
| `Name` | Texto | `"Ana"` |
| `Email` | Texto | `"ana@exemplo.com"` |
| `DateOfBirth` | Data e hora | `2000-05-12` |

Essa é a estrutura utilizada como exemplo na explicação do instrutor: nome, e-mail e data de nascimento.

### Registros compatíveis

```text
TABELA: USERS
+----------+---------------------+---------------+
| Name     | Email               | DateOfBirth   |
+----------+---------------------+---------------+
| Ana      | ana@exemplo.com     | 2000-05-12    |
| Bruno    | bruno@exemplo.com   | 1996-10-08    |
+----------+---------------------+---------------+
```

Cada registro segue a definição da tabela.

### Exemplo de valor incompatível

Se `DateOfBirth` espera uma data e tentamos inserir um texto sem representação válida de data, o sistema pode rejeitar a operação por incompatibilidade de tipo.

O instrutor explica que o banco relacional **aplica a estrutura definida ao receber as informações**.

### Nuance técnica

Seguir um esquema **não significa necessariamente informar todas as colunas em cada operação**. Uma coluna pode admitir `NULL`, ter um valor padrão ou ser gerada pelo próprio banco. A obrigatoriedade depende do **esquema e das restrições**.

O conceito correto é: **os registros precisam atender às regras estabelecidas para a tabela**, e não que todas as propriedades precisam ser preenchidas manualmente em todos os casos.

---

## 5. Exemplo: tabela de usuários

A aula apresenta uma situação de cadastro de pessoas.

Podemos visualizar um esquema didático assim:

```text
Users
|
+-- Name         : texto
+-- Email        : texto
+-- DateOfBirth  : data/hora
```

A aplicação precisa fornecer valores compatíveis com cada campo.

### Por que ter uma definição de colunas?

Uma estrutura definida torna as expectativas sobre os dados mais explícitas.

Por exemplo:

- uma coluna de nascimento deve representar uma data;
- o campo de e-mail deve armazenar um valor de texto;
- a organização das informações não muda arbitrariamente de um registro para outro sem alterações de esquema.

**Atenção:** a aula não ensina ainda a criar essa tabela. O exemplo ilustra o conceito de estrutura.

---

## 6. A linguagem SQL

O instrutor afirma que, nos bancos relacionais apresentados, inserimos e manipulamos informações utilizando **SQL**.

**SQL** é a sigla de *Structured Query Language* — Linguagem de Consulta Estruturada.

É utilizada para trabalhar com dados e estruturas em sistemas relacionais.

### Exemplo complementar de inserção

A gravação não fornece um comando SQL específico, mas a operação mencionada poderia ser representada didaticamente assim:

```sql
INSERT INTO Users (Name, Email, DateOfBirth)
VALUES ('Ana', 'ana@exemplo.com', '2000-05-12');
```

A declaração descreve em qual tabela inserir o registro, quais colunas serão preenchidas e quais valores serão fornecidos.

### Exemplo complementar de consulta

```sql
SELECT Name, Email
FROM Users;
```

Essa consulta representa a recuperação dos campos `Name` e `Email`.

**Esses comandos são exemplos de estudo adicionados ao resumo; o instrutor não os executa neste vídeo.**

### Distinção útil

- **MySQL / SQL Server:** tecnologias de banco de dados.
- **SQL:** linguagem utilizada para consultar e manipular informações em bancos relacionais.

---

## 7. Bancos não relacionais e a flexibilidade dos documentos

O vídeo passa então aos **bancos NoSQL**, usando **MongoDB** como exemplo.

A principal característica demonstrada é a possibilidade de armazenar, em uma mesma coleção, **documentos com campos diferentes**.

No caso mostrado, o instrutor não precisa definir previamente as mesmas propriedades para os dois documentos.

### Qual é a consequência dessa flexibilidade?

O primeiro registro pode armazenar informações sobre uma pessoa; o segundo pode conter informações diferentes, como o nome de um animal e um país.

```text
COLEÇÃO
  |
  +-- Documento A
  |     +-- id
  |     +-- nome
  |     +-- e-mail
  |
  +-- Documento B
        +-- pet
        +-- nome
        +-- país
```

Ambos podem coexistir na mesma coleção, conforme a demonstração.

### Precisão importante

O exemplo **não significa que todo banco NoSQL armazena documentos**: documentos são **um dos modelos**. Os demais serão explicados adiante.

Também não significa que **nenhum banco NoSQL impõe regras**. A flexibilidade depende do modelo e da configuração utilizada.

---

## 8. MongoDB: database, collection e document

O exemplo principal da aula utiliza o **MongoDB**, um sistema de banco de dados orientado a documentos.

O instrutor cria um banco chamado **`cashflow`** e uma coleção denominada **`MyDocuments`**.

### Hierarquia mostrada

```text
MongoDB
  |
  +-- Database: cashflow
        |
        +-- Collection: MyDocuments
              |
              +-- Document 1
              |
              +-- Document 2
```

### Correspondência com a analogia anterior

| Termo do MongoDB | Analogia usada na aula |
|---|---|
| `database` | Banco de dados / armário |
| `collection` | Conjunto de documentos, comparado a uma tabela |
| `document` | Registro armazenado na coleção |

O instrutor compara uma **collection** a uma tabela para facilitar o entendimento.

Essa comparação é **didática**, não uma afirmação de que os dois conceitos possuem todos os mesmos comportamentos.

### Terminologia

Um **documento** é uma unidade de informação armazenada dentro da coleção.

No MongoDB, os documentos são frequentemente apresentados em uma representação semelhante a objetos JSON.

---

## 9. Exemplo da aula: dois documentos diferentes

O professor mostra dois documentos inseridos na mesma collection.

### Primeiro documento

O primeiro contém os campos:

- `id`, com o número `7`;
- `name`, com o nome exemplificado pelo instrutor;
- `email`, com um endereço fictício.

Representação **didática e simplificada**, inspirada nos valores citados na transcrição:

```json
{
  "id": 7,
  "name": "Wellison",
  "email": "wellison@gmail.com"
}
```

### Segundo documento

O segundo contém outros campos:

- `pet`, com o valor `"dog"`;
- `name`, com o valor `"Charlie"`;
- `country`, com o valor `"Canada"`.

```json
{
  "pet": "dog",
  "name": "Charlie",
  "country": "Canada"
}
```

### O que observar?

1. Ambos pertencem à **mesma coleção**.
2. Os documentos **não possuem exatamente as mesmas propriedades**.
3. O primeiro inclui um campo numérico `id`.
4. O segundo contém informações de outro formato.
5. No exemplo apresentado, o MongoDB permite o armazenamento dos dois documentos.

Essa é a flexibilidade que o instrutor pretende demonstrar.

### Não confundir o campo id com _id

O `id: 7` do primeiro exemplo é **um campo declarado no documento**.

O MongoDB também possui um campo especial chamado **`_id`**, comentado separadamente a seguir.

**Nota:** os exemplos JSON são representações didáticas dos campos narrados no vídeo, e não uma exportação literal dos registros do MongoDB.

---

## 10. A propriedade _id no MongoDB

O instrutor chama atenção para o campo **`_id`**, que aparece nos documentos.

No MongoDB, esse campo funciona como o **identificador único do documento** dentro da coleção.

### Comportamento padrão

Quando um documento é inserido **sem um `_id` fornecido**, o sistema/driver normalmente gera um identificador, frequentemente um **ObjectId**.

```text
Novo documento
      |
      v
Possui _id?
   |       |
  sim     não
   |       |
   |       v
   |    Gerar _id
   |       |
   +---+---+
       |
       v
Documento armazenado
```

### Nuance técnica importante

A explicação oral diz que o `_id` é automático. Isso é verdadeiro **quando não foi informado explicitamente**.

Mas é possível fornecer um `_id` próprio, desde que respeitadas as exigências de unicidade e tipo aceito.

Portanto, o correto é entender:

> Todo documento armazenado precisa ter o campo `_id`; caso ele não seja fornecido, normalmente é gerado automaticamente.

### BSON versus JSON

O instrutor usa **JSON** como referência para visualizar o formato dos documentos.

**Complemento técnico:** o MongoDB utiliza **BSON**, uma representação binária de documentos, para armazenar os dados. JSON é uma maneira útil de visualizar e intercambiar estruturas semelhantes, mas os formatos não são tecnicamente idênticos.

---

## 11. Os quatro modelos NoSQL apresentados

Depois do exemplo com MongoDB, o instrutor apresenta **quatro categorias importantes de bancos NoSQL**, de acordo com a estrutura de armazenamento.

| Modelo | Organização principal | Exemplo citado na aula |
|---|---|---|
| **Documentos** | Documentos com campos e valores | MongoDB |
| **Grafos** | Estrutura de grafos | Nenhum produto específico citado |
| **Chave-valor** | Associação entre uma chave e um valor | Redis |
| **Wide-column store** | Organização por famílias/conjuntos de colunas | Nenhum produto específico citado |

A aula trata essas categorias como **quatro modelos principais para fins didáticos**.

---

## 12. Primeiro modelo NoSQL: documentos

O **banco orientado a documentos** armazena unidades de dados chamadas *documents*.

Foi exatamente esse o caso demonstrado com MongoDB.

### Exemplo conceitual

```text
Collection: MyDocuments
    |
    +-- Document
    |    +-- name
    |    +-- email
    |
    +-- Document
         +-- pet
         +-- country
```

A coleção pode reunir documentos que não tenham a mesma lista de propriedades, conforme mostrado.

### Característica destacada

**Flexibilidade na estrutura dos documentos.**

Na demonstração, não foi necessário fazer uma alteração prévia de colunas para armazenar o segundo registro com campos diferentes.

---

## 13. Segundo modelo NoSQL: grafos

O instrutor apresenta também bancos NoSQL orientados a **grafos**.

Ele observa que o tema é menos intuitivo em uma primeira explicação e não entra nos detalhes.

### O que a aula afirma?

Esse modelo utiliza uma **estrutura em grafos para representar informações**.

### Complemento conceitual

Grafos podem ser visualizados por **nós e conexões**:

```text
[Pessoa A] --- conhece ---> [Pessoa B]
    |
    +--------- participa ---> [Grupo]
```

Nesse tipo de representação, as **relações entre elementos** são importantes para a organização e a consulta dos dados.

**Este diagrama é complementar.** A gravação não apresenta uma implementação, um produto específico ou comandos de consulta de grafos.

---

## 14. Terceiro modelo NoSQL: chave-valor e Redis

O modelo **chave-valor** é apresentado com um exemplo conhecido: **Redis**.

A ideia central é armazenar um valor associado a uma chave.

```text
Chave: "user:7"
          |
          v
Valor: informações relacionadas ao usuário
```

### Como funciona a recuperação?

O instrutor explica que, para localizar o dado, basta fornecer a **chave utilizada para identificar o valor armazenado**.

### Exemplo didático

```text
Chave           Valor
--------------  ----------------
"language"      "pt-BR"
"theme"         "dark"
"visits"        "15"
```

Esse quadro é apenas uma ilustração de pares chave-valor; não representa uma configuração de Redis implementada no vídeo.

### Ideia para lembrar

```text
           CHAVE
             |
             v
     Recupera o VALOR
             |
             v
  Dados associados à chave
```

O instrutor ressalta que Redis é uma tecnologia bastante utilizada no mercado.

---

## 15. Quarto modelo NoSQL: wide-column store

A quarta categoria apresentada é **wide-column store**.

O instrutor utiliza a ideia de uma **matriz** como analogia para a organização das informações.

### Conceito apresentado

É um modelo NoSQL cuja estrutura está relacionada à organização dos dados por colunas, distinta do armazenamento documental e do modelo chave-valor simples.

### Complemento técnico necessário

Em explicações mais precisas, esse modelo costuma ser descrito em termos de **famílias de colunas** (*column families*) e estruturas de linhas que podem possuir conjuntos de colunas distintos.

Uma representação conceitual é:

```text
Identificador    Coluna A    Coluna B    Coluna C
-----------      --------    --------    --------
Linha 1             valor       valor      ...
Linha 2             valor        ...       valor
```

**Atenção:** a comparação com uma “matriz” na aula é uma simplificação. Não devemos concluir que todo wide-column store seja literalmente uma matriz densa ou que tenha as mesmas restrições de uma tabela relacional tradicional.

O vídeo não cita um produto específico nem demonstra operações práticas para essa categoria.

---

## 16. Comparação entre bancos relacionais e NoSQL

| Aspecto | Relacionais | NoSQL |
|---|---|---|
| Exemplos apresentados | MySQL, SQL Server | MongoDB, Redis |
| Estrutura destacada | Tabelas com esquema | Diferentes modelos de armazenamento |
| Organização do dado | Linhas e colunas definidas | Documentos, grafos, chave-valor, wide-column |
| Flexibilidade mostrada na aula | Respeitar a estrutura da tabela | MongoDB permite documentos com campos distintos |
| Linguagem mencionada | SQL | A aula não apresenta uma linguagem única para todos |
| Unidade de organização do exemplo | Registro de uma tabela | Documento de uma coleção |
| Foco da demonstração | Necessidade de seguir o esquema | Possibilidade de variar os campos do documento |

### Qual é o principal contraste?

**Banco relacional:** definimos uma estrutura para a tabela, e os dados precisam estar compatíveis com as regras desse esquema.

**MongoDB no exemplo:** documentos distintos podem conviver na mesma coleção com conjuntos de campos diferentes.

### O que essa comparação não significa?

Não significa que bancos relacionais sejam “ruins” ou incapazes de evoluir o esquema.

Também não significa que todo NoSQL seja totalmente desestruturado ou dispense validação.

São **abordagens diferentes**, e a adequação depende das necessidades de armazenamento e consulta.

---

## 17. Conexão com o CashFlow

A transcrição cria um exemplo de banco chamado **`cashflow` no MongoDB**, mas não demonstra a conexão da API ao banco.

Para pensar na aplicação estudada no curso, podemos formular duas possibilidades **apenas conceituais**.

### Exemplo relacional

Uma aplicação de despesas poderia guardar registros em uma tabela com colunas definidas, como:

```text
Expenses
    |
    +-- Id
    +-- Title
    +-- Amount
    +-- Date
    +-- PaymentType
```

### Exemplo documental

Um documento poderia representar os dados de uma despesa:

```json
{
  "title": "Compra de mercado",
  "amount": 120,
  "paymentType": "CreditCard"
}
```

**Esses exemplos são uma conexão didática com o projeto; não são modelos de persistência implementados nem escolhidos nesta aula.**

---

## 18. Observações técnicas importantes

### 18.1 MySQL e SQL Server são nomes de produtos diferentes

A transcrição menciona algo próximo de “MySQL Server e SQL Server”. Os produtos exemplificados são **MySQL** e **Microsoft SQL Server**.

### 18.2 NoSQL não significa ausência absoluta de esquema

O exemplo do MongoDB mostra flexibilidade, mas bancos documentais podem oferecer mecanismos de validação e restrição de estrutura.

A descrição “não segue nenhuma estrutura” serve para enfatizar a demonstração, porém é excessivamente absoluta se aplicada a toda a categoria NoSQL.

### 18.3 Bancos relacionais também podem ter flexibilidade

Um esquema relacional pode incluir colunas opcionais, valores padrão e alterações planejadas de estrutura.

O ponto de distinção é a **forma como o esquema é representado e aplicado**, não uma proibição de qualquer evolução.

### 18.4 O MongoDB não armazena literalmente JSON puro

O formato técnico utilizado no armazenamento documental do MongoDB é **BSON**. JSON continua sendo uma analogia útil para leitura.

### 18.5 O campo _id é especial

O identificador `_id` não deve ser confundido com um campo arbitrário chamado `id`. Se não for informado, o MongoDB normalmente cria o `_id`.

### 18.6 As quatro categorias são uma introdução

Documentos, grafos, chave-valor e wide-column são modelos importantes apresentados pelo instrutor. A aula não pretende esgotar todas as variações tecnológicas possíveis.

---

## 19. Resumo e revisão

- A aula divide os bancos de dados, de maneira introdutória, em **relacionais e não relacionais**.
- **MySQL** e **SQL Server** são exemplos de bancos relacionais.
- Bancos relacionais armazenam informações em **tabelas** com uma **estrutura definida**.
- O exemplo de usuário contém **nome, e-mail e data de nascimento**.
- A linguagem **SQL** é utilizada para trabalhar com os bancos relacionais mencionados.
- O **MongoDB** é utilizado como exemplo de banco NoSQL **orientado a documentos**.
- O banco criado na demonstração se chama `cashflow`, com a collection `MyDocuments`.
- A mesma coleção contém **documentos com campos diferentes**.
- O campo **`_id`** identifica documentos e normalmente é gerado caso não seja fornecido.
- Os quatro modelos NoSQL apresentados são **documentos, grafos, chave-valor e wide-column store**.
- **Redis** é o exemplo de armazenamento **chave-valor** citado pelo instrutor.
- A principal lição é que **bancos de dados podem empregar estruturas distintas para armazenar e recuperar informações**.

### Perguntas de revisão

1. Por que um registro inserido em uma tabela relacional precisa respeitar as regras de seu esquema?
2. Qual foi a principal diferença observada entre os dois documentos do exemplo em MongoDB?
3. Qual é a função do campo `_id`?
4. Em qual dos quatro modelos NoSQL o Redis foi citado?
5. Qual é a diferença entre **collection** e **document**?
6. Por que não é tecnicamente correto dizer que todo banco NoSQL é totalmente livre de regras estruturais?

---

## 20. Mapa mental

```text
                AULA 83
         BANCOS DE DADOS
                 |
       +---------+---------+
       |                   |
       v                   v
    RELACIONAL           NoSQL
       |                   |
       v             +-----+-----+-------+
  Tabela e esquema   |     |     |       |
       |             v     v     v       v
   MySQL/SQL       Doc.  Grafo  Chave   Wide-
    Server                      valor   column
       |             |           |
       v             v           v
   Linguagem       MongoDB      Redis
      SQL             |
                      v
                 database
                      |
                  collection
                      |
                  documents
                      |
               campos variáveis

       CONCEITO CENTRAL
  Diferentes modelos organizam
  informações de formas distintas
```

**Conclusão:** a aula apresenta o contraste entre **estrutura definida em tabelas relacionais** e **flexibilidade de documentos no MongoDB**, ampliando depois a discussão para os quatro modelos NoSQL abordados. O principal objetivo é compreender que a forma de armazenar informações varia conforme o modelo do banco de dados.
