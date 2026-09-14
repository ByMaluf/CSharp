# Passando Parâmetros pela Rota — Query String e Path

## 📋 Índice

1. [Como enviar informações para um Endpoint?](#como-enviar-informações-para-um-endpoint)
2. [GET e o envio de informações](#get-e-o-envio-de-informações)
3. [Parâmetros em um Endpoint GET](#parâmetros-em-um-endpoint-get)
4. [Query String](#query-string)
5. [Múltiplos parâmetros na Query String](#múltiplos-parâmetros-na-query-string)
6. [Quando utilizar Query String?](#quando-utilizar-query-string)
7. [Parâmetros no Path](#parâmetros-no-path)
8. [Route e parâmetros de rota](#route-e-parâmetros-de-rota)
9. [Múltiplos parâmetros no Path](#múltiplos-parâmetros-no-path)
10. [Query String x Path](#query-string-x-path)
11. [Quando utilizar cada formato?](#quando-utilizar-cada-formato)
12. [Resumo](#resumo)

---

## Como enviar informações para um Endpoint?

Quando um Endpoint precisa receber informações do cliente, existem três formas apresentadas na aula para enviar esses dados:

```text
Informações para um Endpoint
            │
    ┌───────┼────────┐
    │       │        │
    ▼       ▼        ▼
  Rota    Headers   Body
  URL    Cabeçalho  Corpo
```

Portanto, os dados podem ser enviados através:

1. da própria **URL da requisição**;
2. dos **Headers** da requisição;
3. do **Body** da requisição.

Nesta aula, o foco está no envio de informações através da **URL**.

---

## GET e o envio de informações

Na abordagem apresentada na aula, requisições:

```http
GET
```

trabalham com duas dessas formas:

```text
GET
│
├── URL / Rota
│
└── Headers
```

O `GET` não será utilizado com informações enviadas através do:

```text
Body
```

Portanto, durante o curso, vamos considerar:

```text
GET + Body ❌
```

e trabalhar com:

```text
GET + URL     ✅
GET + Headers ✅
```

> Nesta aula, o foco será exclusivamente em como enviar informações pela **URL da requisição**.

---

## Parâmetros em um Endpoint GET

Imagine que queremos recuperar um usuário específico através do seu `ID`.

Nosso método poderia receber:

```csharp
int id
```

Exemplo:

```csharp
[HttpGet]
public IActionResult Get(int id)
{
    // localizar usuário utilizando o ID

    return Ok();
}
```

Como o Endpoint é um método, `id` representa um parâmetro recebido por ele.

```text
GET
 │
 ▼
Get(int id)
        │
        ▼
    ID recebido
        │
        ▼
Localizar usuário
```

Ao executar a aplicação, o Swagger identifica esse parâmetro e permite que um valor seja informado durante o teste.

---

## Query String

Quando simplesmente adicionamos um parâmetro ao método:

```csharp
[HttpGet]
public IActionResult Get(int id)
{
    return Ok();
}
```

o Swagger apresenta esse parâmetro como:

```text
Query
```

A chamada pode ficar semelhante a:

```text
https://localhost:7081/api/user?id=1
```

Essa forma de enviar parâmetros através da URL é chamada de:

```text
Query String
```

Sua estrutura é:

```text
URL
 │
 ▼
https://localhost:7081/api/user?id=1
                                │
                                └── Query String
```

O caractere:

```text
?
```

indica o início dos parâmetros da Query String.

Depois temos:

```text
id=1
```

onde:

```text
id
↓
Nome do parâmetro

1
↓
Valor do parâmetro
```

Portanto:

```text
? + nome + = + valor
```

Exemplo:

```text
?id=1
```

---

## Múltiplos parâmetros na Query String

Um Endpoint pode receber mais de um parâmetro.

Por exemplo:

```csharp
[HttpGet]
public IActionResult Get(int id, string nickname)
{
    return Ok();
}
```

Nesse caso, podemos enviar:

```text
id = 7
nickname = l
```

A URL ficará semelhante a:

```text
https://localhost:7081/api/user?id=7&nickname=l
```

Temos:

```text
?
↓
Início da Query String

id=7
↓
Primeiro parâmetro

&
↓
Separa os parâmetros

nickname=l
↓
Segundo parâmetro
```

Estrutura:

```text
URL?parametro1=valor1&parametro2=valor2
```

Exemplo:

```text
/api/user?id=7&nickname=l
```

Se existirem mais parâmetros:

```text
/api/user?id=7&nickname=l&idade=20&ativo=true
```

Portanto:

```text
?
↓
Inicia os parâmetros

&
↓
Separa um parâmetro do próximo
```

---

## Quando utilizar Query String?

Um exemplo apresentado na aula é o uso de **filtros em websites**.

Imagine um site de produtos no qual o usuário realiza diferentes filtros.

Conceitualmente:

```text
Produtos
   │
   ├── Categoria
   ├── Preço
   ├── Marca
   └── Outros filtros
```

Essas informações podem aparecer na URL como Query String:

```text
/produtos?categoria=notebook&marca=exemplo
```

Uma vantagem é que os filtros permanecem representados na própria URL.

Assim, podemos:

```text
Aplicar filtros
      ↓
URL contém os filtros
      ↓
Copiar URL
      ↓
Compartilhar URL
      ↓
Outra pessoa abre
      ↓
Mesmos parâmetros são enviados
```

Por exemplo, podemos copiar a URL e enviá-la através de:

```text
WhatsApp
Telegram
E-mail
etc.
```

Quando outra pessoa acessar aquele endereço, os parâmetros estarão presentes na própria URL.

Por isso, **Query Strings são úteis em cenários de filtros e consultas**.

---

## Parâmetros no Path

Existe outra forma de colocar informações diretamente na URL.

Em vez de:

```text
/api/user?id=7
```

podemos possuir:

```text
/api/user/7
```

Nesse caso, o `7` faz parte diretamente do **caminho da URL**.

No Swagger, esse tipo de parâmetro aparece como:

```text
Path
```

Comparando:

```text
QUERY STRING

/api/user?id=7
          └── parâmetro


PATH

/api/user/7
          └── parâmetro
```

---

## Route e parâmetros de rota

Para informar que determinado valor faz parte do caminho da URL, podemos definir uma rota no Endpoint.

Exemplo apresentado:

```csharp
[HttpGet]
[Route("{id}")]
public IActionResult Get(int id)
{
    return Ok();
}
```

O:

```csharp
"{id}"
```

representa um parâmetro que será recebido através da rota.

O nome utilizado entre:

```text
{ }
```

deve corresponder ao parâmetro recebido pelo método:

```csharp
{ id }
  ↓
int id
```

Assim:

```csharp
[Route("{id}")]
public IActionResult Get(int id)
```

produz uma rota semelhante a:

```text
/api/user/7
```

O valor:

```text
7
```

será recebido no parâmetro:

```csharp
int id
```

Fluxo:

```text
GET /api/user/7
             │
             ▼
          {id}
             │
             ▼
          int id
             │
             ▼
             7
```

---

## Múltiplos parâmetros no Path

Também podemos possuir mais de um parâmetro no caminho.

Por exemplo:

```csharp
[HttpGet]
[Route("{id}/person/{nickname}")]
public IActionResult Get(int id, string nickname)
{
    return Ok();
}
```

Nesse caso, uma chamada poderia ser:

```text
/api/user/7/person/l
```

Temos:

```text
/api/user/{id}/person/{nickname}
           │            │
           ▼            ▼
           7            l
```

Logo:

```csharp
id = 7
nickname = "l"
```

---

### Separação através de barras

Os segmentos do caminho precisam ser separados por:

```text
/
```

Por exemplo:

```text
{id}/{nickname}
```

Resultando em:

```text
/api/user/7/l
```

Podemos também possuir partes fixas na rota:

```text
{id}/person/{nickname}
```

Resultando em:

```text
/api/user/7/person/l
```

Nesse caso:

```text
{id}
↓
Parâmetro

person
↓
Parte fixa da rota

{nickname}
↓
Parâmetro
```

---

## Query String x Path

As duas formas enviam informações através da URL, mas possuem formatos diferentes.

### Query String

```text
https://localhost:7081/api/user?id=7
```

O parâmetro aparece depois de:

```text
?
```

---

### Path

```text
https://localhost:7081/api/user/7
```

O parâmetro faz parte diretamente do caminho.

---

### Comparação

| Query String | Path |
|---|---|
| `/api/user?id=7` | `/api/user/7` |
| Utiliza `?` para iniciar os parâmetros | Utiliza segmentos da própria rota |
| Múltiplos parâmetros são separados por `&` | Segmentos são separados por `/` |
| Swagger apresenta como `Query` | Swagger apresenta como `Path` |
| Muito útil para filtros | Muito utilizado para identificar recursos específicos |

---

## Quando utilizar cada formato?

A escolha depende da finalidade daquele parâmetro.

### Query String

Na aula, o principal exemplo são **filtros**.

Exemplo:

```text
GET /api/products?category=computer&brand=example
```

Podemos pensar:

```text
Quero consultar produtos
       │
       └── com determinados filtros
```

Outro benefício é que os filtros permanecem na URL e podem ser compartilhados.

---

### Path

Na abordagem apresentada na aula, é muito utilizado quando precisamos trabalhar com **IDs de recursos específicos**.

Por exemplo:

```text
GET /api/product/15
```

Significa conceitualmente:

```text
Recuperar o produto
        ↓
       ID 15
```

Outro exemplo mencionado é uma atualização.

Podemos ter:

```text
PUT /api/product/15
```

Nesse caso:

```text
15
↓
Identifica qual produto será atualizado
```

E as novas informações do produto podem ser enviadas no:

```text
Body
```

Conceitualmente:

```text
PUT /api/product/15
         │
         └── Qual produto?

Body
 │
 └── Quais são os novos dados?
```

---

## Duas formas de enviar dados pela URL

Portanto, nesta aula aprendemos duas formas diferentes de enviar informações através da própria URL:

```text
                    URL
                     │
          ┌──────────┴──────────┐
          │                     │
          ▼                     ▼
    Query String               Path
          │                     │
          ▼                     ▼
 /user?id=7              /user/7
```

### Query String

```text
/api/user?id=7&nickname=l
```

Estrutura:

```text
?
↓
Inicia os parâmetros

&
↓
Separa os parâmetros
```

### Path

```text
/api/user/7/l
```

Estrutura:

```text
/
↓
Separa os segmentos da rota
```

---

## Exemplo Completo — Query String

Código:

```csharp
[HttpGet]
public IActionResult Get(int id, string nickname)
{
    return Ok();
}
```

Requisição:

```http
GET /api/user?id=7&nickname=l
```

Fluxo:

```text
GET /api/user?id=7&nickname=l
                │          │
                ▼          ▼
              id=7    nickname="l"
                │          │
                └────┬─────┘
                     ▼
       Get(int id, string nickname)
```

---

## Exemplo Completo — Path

Código:

```csharp
[HttpGet]
[Route("{id}/{nickname}")]
public IActionResult Get(int id, string nickname)
{
    return Ok();
}
```

Requisição:

```http
GET /api/user/7/l
```

Fluxo:

```text
GET /api/user/7/l
              │ │
              │ └────── nickname
              │
              └──────── id
                    │
                    ▼
       Get(int id, string nickname)
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Parâmetro** | Informação enviada para um Endpoint |
| **URL/Rota** | Uma das formas de enviar informações para a API |
| **Headers** | Informações enviadas no cabeçalho da requisição |
| **Body** | Informações enviadas no corpo da requisição |
| **GET** | Na abordagem da aula, recebe informações pela URL ou Headers, e não pelo Body |
| **Query String** | Parâmetros adicionados à URL após `?` |
| **`?`** | Indica o início da Query String |
| **`&`** | Separa múltiplos parâmetros da Query String |
| **Path** | Parâmetro inserido diretamente no caminho da URL |
| **`{id}`** | Representa um parâmetro dentro da rota |
| **`/`** | Separa os segmentos do Path |
| **Query** | Forma como o Swagger identifica parâmetros de Query String |
| **Path** | Forma como o Swagger identifica parâmetros inseridos no caminho |
| **Filtros** | Exemplo comum apresentado para uso de Query String |
| **ID** | Exemplo comum apresentado para uso de parâmetro no Path |

---

## Visão Geral

```text
              ENDPOINT PRECISA DE DADOS
                         │
           ┌─────────────┼─────────────┐
           │             │             │
           ▼             ▼             ▼
          URL         HEADERS         BODY
           │
           │
     ┌─────┴─────┐
     │           │
     ▼           ▼
Query String    Path
     │           │
     ▼           ▼
?id=7         /7
     │           │
     ▼           ▼
Filtros      Identificação
             de recurso
```

### Query String

```text
/api/user?id=7&nickname=l
          │
          ▼
          ?
          │
          ▼
    Início dos parâmetros
          │
     ┌────┴─────┐
     ▼          ▼
   id=7    nickname=l
     │
     └──── & ───┘
```

### Path

```text
[Route("{id}/{nickname}")]
          │
          ▼
 /api/user/7/l
           │ │
           │ └── nickname = "l"
           │
           └──── id = 7
```

> **Em resumo:** informações podem chegar a um Endpoint através da **URL, Headers ou Body**. Nesta aula, vimos duas formas de representar informações na URL: **Query String**, utilizando uma estrutura como `?id=7&nickname=l`, e **Path**, colocando os valores diretamente no caminho, como `/user/7/l`. Na abordagem apresentada, Query Strings são especialmente úteis para **filtros**, enquanto parâmetros no Path são muito utilizados para **identificar um recurso específico através de seu ID**.