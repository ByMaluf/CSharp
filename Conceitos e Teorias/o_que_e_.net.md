# .NET

## 📋 Índice
1. [O que é o .NET?](#o-que-é-o-net)
2. [Relação entre C# e .NET](#relação-entre-c-e-net)
3. [Principais recursos do .NET](#principais-recursos-do-net)
4. [Versionamento e suporte](#versionamento-e-suporte)
5. [História do .NET](#história-do-net)
6. [Resumo](#resumo)

---

## O que é o .NET?

O **.NET** é uma **plataforma de desenvolvimento** criada pela Microsoft, composta por um grande conjunto de **bibliotecas (Framework Class Library - FCL)** e ferramentas que facilitam a criação de aplicações.

Ao invés de desenvolver todas as funcionalidades do zero, o desenvolvedor reutiliza bibliotecas já implementadas pelo .NET, podendo também estender seus comportamentos conforme a necessidade.

Com o .NET é possível desenvolver diversos tipos de aplicações, como:

- aplicações Web
- APIs REST
- aplicações Desktop
- aplicações Mobile (Android e iOS)
- serviços
- aplicações em nuvem (Cloud)

Basta escolher o tipo de projeto desejado e o .NET disponibiliza as bibliotecas adequadas para aquele cenário.

---

## Relação entre C# e .NET

É muito comum confundir os dois conceitos, mas eles possuem funções diferentes.

| C# | .NET |
|---|---|
| Linguagem de programação | Plataforma de desenvolvimento |
| Utilizada para escrever o código | Fornece bibliotecas, ferramentas e infraestrutura para executar o código |

Ou seja:

- **C# é a linguagem.**
- **.NET é a plataforma que utiliza essa linguagem.**

Embora o foco seja o **C#**, o .NET também oferece suporte para outras linguagens, como:

- F#
- Visual Basic .NET (VB.NET)

---

## Principais recursos do .NET

O .NET fornece diversos recursos prontos para facilitar o desenvolvimento de aplicações.

Entre eles:

- bibliotecas prontas
- programação assíncrona (`async` / `await`)
- Expressões Lambda
- LINQ (Language Integrated Query)
- Garbage Collector (GC)
- gerenciamento automático de memória
- APIs para acesso a arquivos, rede, banco de dados, criptografia, entre outras

### Garbage Collector (GC)

O **Garbage Collector** é responsável por liberar automaticamente a memória de objetos que não estão mais sendo utilizados.

Isso reduz problemas como:

- vazamento de memória
- gerenciamento manual de memória
- ponteiros inválidos

---

### LINQ

O **LINQ (Language Integrated Query)** permite realizar consultas em coleções de dados utilizando uma sintaxe semelhante à linguagem SQL.

Seu objetivo é tornar consultas mais simples, legíveis e produtivas.

---

## Versionamento e suporte

A Microsoft lança uma nova versão do .NET **todos os anos**, normalmente em **novembro**.

Existem dois tipos principais de versões:

### LTS (Long Term Support)

- lançadas em anos **ímpares**
- suporte por **3 anos**
- recomendadas para ambientes corporativos e projetos de longo prazo

---

### STS (Standard Term Support)

- lançadas em anos **pares**
- suporte por **18 meses**
- voltadas para quem deseja utilizar recursos mais recentes

> **Importante:** Em ambientes corporativos, normalmente é recomendado utilizar versões **LTS**, devido ao maior período de suporte e estabilidade.

---

## História do .NET

### .NET Framework (2002)

Foi a primeira plataforma .NET lançada pela Microsoft.

Características:

- executava apenas em **Windows**
- não era multiplataforma
- utilizada principalmente para aplicações Desktop e Web no ecossistema Windows

---

### .NET Core (2014)

Com a necessidade de tornar a plataforma mais moderna, a Microsoft lançou o **.NET Core**.

Principais características:

- Open Source
- gratuito
- multiplataforma
- executa em:
  - Windows
  - Linux
  - macOS

Foi um grande avanço em relação ao .NET Framework.

---

### .NET (a partir da versão 5)

Após o **.NET Core 3.1**, a Microsoft removeu oficialmente o termo **Core**.

A sequência de versões ficou assim:

- .NET Core 3.1
- .NET 5
- .NET 6
- .NET 7
- .NET 8
- .NET 9
- ...

O nome oficial atualmente é apenas **.NET**.

Apesar disso, muitos desenvolvedores ainda utilizam o termo **.NET Core**, principalmente por hábito.

---

## Resumo

| Conceito | Descrição |
|---|---|
| C# | Linguagem de programação |
| .NET | Plataforma de desenvolvimento composta por bibliotecas e ferramentas |
| .NET Framework | Primeira versão da plataforma, exclusiva para Windows |
| .NET Core | Versão Open Source e multiplataforma lançada em 2014 |
| .NET | Nome atual da plataforma desde a versão 5 |
| LTS | Versões com suporte de 3 anos (anos ímpares) |
| STS | Versões com suporte de 18 meses (anos pares) |
| GC | Garbage Collector: gerenciamento automático de memória |
| LINQ | Linguagem integrada para consultas em coleções de dados |