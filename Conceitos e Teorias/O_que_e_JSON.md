# JSON — JavaScript Object Notation

## 📋 Índice

1. [O que é JSON?](#o-que-é-json)
2. [Características do JSON](#características-do-json)
3. [Tipos de dados suportados](#tipos-de-dados-suportados)
4. [Estrutura de um objeto JSON](#estrutura-de-um-objeto-json)
5. [Objetos](#objetos)
6. [Arrays](#arrays)
7. [Array de objetos](#array-de-objetos)
8. [Benefícios do JSON](#benefícios-do-json)
9. [JSON no .NET](#json-no-net)
10. [Resumo](#resumo)

---

## O que é JSON?

O **JSON (JavaScript Object Notation)** é um formato utilizado para **armazenar e transportar dados**.

Ele possui uma estrutura:

- simples;
- leve;
- legível;
- fácil de escrever;
- fácil de interpretar por humanos;
- fácil de processar por máquinas.

É muito utilizado na comunicação entre diferentes sistemas, principalmente em APIs.

Exemplo:

    Aplicação
        ↓
      JSON
        ↓
       API

Da mesma forma, a API pode devolver informações utilizando JSON:

    API
     ↓
    JSON
     ↓
    Aplicação

---

## Características do JSON

O JSON possui algumas características importantes:

- é independente de linguagem de programação;
- possui uma sintaxe simples;
- é fácil de entender;
- permite armazenar diferentes tipos de dados;
- possui tamanho relativamente compacto;
- pode ser utilizado para transportar dados pela rede;
- permite comunicação entre diferentes sistemas e plataformas.

Sua estrutura é baseada principalmente em pares de:

    chave : valor

Exemplo:

    "nome": "Bruce Wayne"

Nesse exemplo:

    "nome"
       ↓
      Chave

    "Bruce Wayne"
           ↓
          Valor

---

## Tipos de dados suportados

O JSON pode representar diferentes tipos de dados.

### Number

Representa valores numéricos.

Pode ser um número inteiro:

    34

Ou um número com ponto flutuante:

    34.5

Exemplo:

    "idade": 34

---

### String

Representa uma sequência de caracteres, ou seja, um texto.

As Strings são representadas entre aspas duplas.

Exemplo:

    "nome": "Bruce Wayne"

---

### Boolean

Representa valores verdadeiro ou falso.

Exemplo:

    "ativo": true

Ou:

    "ativo": false

---

### Array

Representa uma lista de valores.

Um Array começa e termina com colchetes:

    [ ]

Exemplo:

    "veiculos": [
        "Batimoto",
        "Batmóvel",
        "Batwing"
    ]

---

### Object

Representa uma coleção de propriedades compostas por pares de chave e valor.

Um objeto começa e termina com chaves:

    { }

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34
    }

---

### Null

O valor `null` representa a ausência de um valor.

Exemplo:

    "segundoNome": null

---

## Estrutura de um objeto JSON

Um objeto JSON começa com:

    {

e termina com:

    }

Dentro das chaves ficam suas propriedades.

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham"
    }

Cada propriedade possui:

    "chave": valor

Por exemplo:

    "nome": "Bruce Wayne"

A estrutura pode ser representada assim:

    {
        "propriedade": valor
    }

---

### As propriedades são Strings

Os nomes das propriedades são representados entre aspas duplas.

Exemplo:

    "nome"
    "idade"
    "cidade"
    "veiculos"

A estrutura fica:

    "nome": "Bruce Wayne"

Nesse caso:

    "nome" → propriedade

    "Bruce Wayne" → valor

---

### Separação por dois pontos

Os dois pontos `:` separam a propriedade do valor.

Exemplo:

    "idade": 34

Ou:

    "cidade": "Gotham"

---

### Separação por vírgula

As propriedades são separadas por vírgulas.

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham"
    }

Cada vírgula indica que outra propriedade será declarada.

---

## Objetos

Um objeto JSON é representado por chaves:

    {
    }

Dentro delas ficam as propriedades do objeto.

Exemplo completo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham"
    }

Podemos interpretar esse JSON como um objeto que representa uma pessoa:

    Pessoa
    │
    ├── Nome: Bruce Wayne
    ├── Idade: 34
    └── Cidade: Gotham

---

## Arrays

Um **Array** representa uma lista de valores.

Ele começa e termina com colchetes:

    [
    ]

Exemplo:

    [
        "Batimoto",
        "Batmóvel",
        "Batwing"
    ]

Um Array pode ser armazenado dentro de uma propriedade.

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham",
        "veiculos": [
            "Batimoto",
            "Batmóvel",
            "Batwing"
        ]
    }

Nesse caso, a propriedade:

    "veiculos"

possui como valor um Array.

---

## Array de objetos

Um Array não precisa conter apenas Strings.

Também pode conter objetos.

Exemplo:

    {
        "veiculos": [
            {
                "nome": "Batmóvel",
                "tipo": "Carro"
            },
            {
                "nome": "Batimoto",
                "tipo": "Moto"
            }
        ]
    }

Nesse caso:

    veiculos
       ↓
     Array
       ↓
    ┌───────────────┐
    │ Objeto 1      │
    │ Objeto 2      │
    └───────────────┘

Cada objeto possui suas próprias propriedades.

---

## Benefícios do JSON

### Fácil de ler e escrever

A sintaxe do JSON é simples e intuitiva.

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34
    }

É relativamente fácil entender que esse objeto representa uma pessoa com nome e idade.

---

### Compatibilidade

O JSON pode ser utilizado por diferentes linguagens de programação.

Por exemplo:

- C#;
- Java;
- JavaScript;
- TypeScript;
- Python;
- Kotlin;
- Swift;
- entre outras.

Isso permite que sistemas desenvolvidos com tecnologias diferentes consigam trocar informações.

---

### Transferência eficiente de dados

Por possuir uma estrutura compacta, o JSON pode ser transmitido pela rede de maneira eficiente.

Por isso, é muito utilizado em requisições e respostas HTTP.

Exemplo:

    Front-end
        │
        │ JSON
        ▼
       API
        │
        │ JSON
        ▼
    Front-end

---

### Interoperabilidade

O JSON permite a troca de dados entre diferentes tipos de sistemas e aplicações.

Exemplo:

    Aplicativo Android ─┐
                       │
    Aplicativo iOS ─────┼──→ API .NET
                       │
    Site ───────────────┘

As aplicações podem utilizar tecnologias diferentes, mas todas conseguem se comunicar com a API utilizando JSON.

---

## JSON no .NET

No .NET, podemos criar classes para representar as informações recebidas ou enviadas pela API.

Por exemplo, imagine este JSON:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham"
    }

No .NET, essas informações podem ser representadas por uma classe com propriedades correspondentes.

De forma conceitual:

    JSON
      ↓
    {
        "nome": "Bruce Wayne",
        "idade": 34
    }
      ↓
    .NET
      ↓
    Classe
    ├── Nome
    └── Idade

O .NET consegue realizar a conversão entre:

    Objeto C#
        ↓
       JSON

e também:

    JSON
      ↓
    Objeto C#

Isso facilita bastante a comunicação entre uma API desenvolvida em .NET e outros sistemas.

---

### JSON enviado para a API

Uma aplicação pode enviar:

    {
        "nome": "Bruce Wayne",
        "idade": 34
    }

O .NET recebe essas propriedades e pode convertê-las para uma classe correspondente.

Fluxo:

    Cliente
       │
       │ JSON
       ▼
    API .NET
       │
       ▼
    Classe C#

---

### JSON devolvido pela API

A aplicação .NET também pode possuir um objeto em C# e convertê-lo para JSON antes de devolver a resposta.

Fluxo:

    Classe C#
       │
       ▼
    API .NET
       │
       │ JSON
       ▼
    Cliente

---

### Arquivos JSON

O JSON também pode ser armazenado em arquivos.

Esses arquivos utilizam a extensão:

    .json

Exemplo:

    usuario.json

Conteúdo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "cidade": "Gotham"
    }

---

## Exemplo completo

Um objeto JSON pode conter diferentes tipos de dados ao mesmo tempo.

Exemplo:

    {
        "nome": "Bruce Wayne",
        "idade": 34,
        "ativo": true,
        "cidade": "Gotham",
        "segundoNome": null,
        "veiculos": [
            "Batimoto",
            "Batmóvel",
            "Batwing"
        ]
    }

Nesse objeto temos:

| Propriedade | Tipo |
|---|---|
| `nome` | String |
| `idade` | Number |
| `ativo` | Boolean |
| `cidade` | String |
| `segundoNome` | Null |
| `veiculos` | Array |

---

## Resumo

| Conceito | Descrição |
|---|---|
| **JSON** | Formato utilizado para armazenar e transportar dados |
| **JavaScript Object Notation** | Significado da sigla JSON |
| **Chave** | Nome que identifica uma propriedade |
| **Valor** | Informação associada a uma propriedade |
| **Object** | Coleção de propriedades entre `{ }` |
| **Array** | Lista de valores entre `[ ]` |
| **String** | Sequência de caracteres |
| **Number** | Valor numérico |
| **Boolean** | Valor `true` ou `false` |
| **Null** | Representa ausência de valor |
| **.json** | Extensão utilizada por arquivos JSON |
| **Interoperabilidade** | Permite troca de dados entre diferentes sistemas e tecnologias |

---

## Estrutura Geral

    JSON
     │
     ├── Object
     │     └── { }
     │
     ├── Array
     │     └── [ ]
     │
     ├── String
     │     └── "texto"
     │
     ├── Number
     │     └── 34
     │
     ├── Boolean
     │     ├── true
     │     └── false
     │
     └── Null
           └── null

---

## Fluxo do JSON em uma API

    Aplicação
        │
        │ Request em JSON
        ▼
      API .NET
        │
        │ Processamento
        ▼
     Aplicação
        ▲
        │ Response em JSON
        │
      API .NET