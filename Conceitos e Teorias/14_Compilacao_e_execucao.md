# Compilação e Execução no .NET

## 📋 Índice

1. [O que acontece ao executar um projeto?](#o-que-acontece-ao-executar-um-projeto)
2. [Build (Compilação)](#build-compilação)
3. [Linguagem Intermediária (IL)](#linguagem-intermediária-il)
4. [CLR (Common Language Runtime)](#clr-common-language-runtime)
5. [JIT e AOT](#jit-e-aot)
6. [Arquivos gerados](#arquivos-gerados)
7. [Build x Publish](#build-x-publish)
8. [Resumo](#resumo)

---

## O que acontece ao executar um projeto?

Ao pressionar **F5** ou clicar em **Play** no Visual Studio, o .NET realiza duas etapas:

1. **Build** (compilação do projeto)
2. **Execução** da aplicação

Ou seja:

```
Código C#
     ↓
Build
     ↓
Arquivo .dll (IL)
     ↓
CLR
     ↓
Código nativo
     ↓
Execução
```

---

## Build (Compilação)

O **Build** é o processo responsável por:

- verificar erros de sintaxe
- validar chamadas de métodos
- conferir referências e dependências
- garantir que o projeto possa ser executado

Se tudo estiver correto, o compilador transforma o código-fonte em um arquivo **.dll**.

> O Build **não gera diretamente código nativo**.

---

## Linguagem Intermediária (IL)

Após o Build, o código C# é convertido para uma **Linguagem Intermediária** (**IL - Intermediate Language**).

Características da IL:

- independente do sistema operacional
- independente do processador
- independente da arquitetura
- compacta e otimizada

Isso significa que a mesma DLL pode ser executada em:

- Windows
- Linux
- macOS

desde que exista o runtime do .NET instalado.

---

## CLR (Common Language Runtime)

O **CLR (Common Language Runtime)** é o ambiente de execução do .NET.

Sua principal função é transformar a **IL** em **código nativo** da máquina.

Fluxo:

```
Código C#
        ↓
Build
        ↓
IL (.dll)
        ↓
CLR
        ↓
Código nativo
        ↓
Execução
```

Além disso, o CLR também é responsável por:

- gerenciamento de memória (Garbage Collector)
- tratamento de exceções
- segurança
- gerenciamento de threads
- execução do código

Cada sistema operacional possui sua própria implementação do CLR.

---

## JIT e AOT

Existem duas formas de converter IL para código nativo.

### JIT (Just-In-Time)

A compilação ocorre **somente quando o programa é executado**.

Fluxo:

```
C#
    ↓
.dll (IL)
    ↓
JIT
    ↓
Código nativo
```

Vantagens:

- maior portabilidade
- mesmo arquivo funciona em diferentes sistemas operacionais

---

### AOT (Ahead-Of-Time)

A compilação para código nativo acontece **antes da execução**.

Nesse caso, é gerado um executável (.exe).

Fluxo:

```
C#
    ↓
Compilação AOT
    ↓
.exe
```

Características:

- execução mais rápida
- específico para uma plataforma
- um executável Windows não funciona no Linux

---

## Arquivos gerados

Após o Build, normalmente encontramos os arquivos na pasta:

```
bin/
 └── Debug/
      └── netX.X/
```

Os principais arquivos são:

### DLL

```
HelloWorld.dll
```

- contém o código em IL
- multiplataforma
- executado pelo comando:

```bash
dotnet HelloWorld.dll
```

---

### EXE

```
HelloWorld.exe
```

- código nativo
- específico para o sistema operacional
- pode ser executado diretamente

```bash
HelloWorld.exe
```

---

## Build x Publish

### Build

- compila o projeto
- gera arquivos temporários
- utilizado durante o desenvolvimento

---

### Publish

O comando **Publish** prepara a aplicação para distribuição.

Ele:

- reúne todas as DLLs
- copia todas as dependências
- organiza tudo em uma única pasta
- prepara a aplicação para publicação em servidores ou distribuição

É o processo utilizado para colocar uma aplicação em produção.

---

## Resumo

| Conceito | Descrição |
|---|---|
| Build | Compila o código C# para IL |
| IL (Intermediate Language) | Linguagem intermediária independente da plataforma |
| CLR | Converte IL para código nativo e executa a aplicação |
| JIT | Compila para código nativo apenas em tempo de execução |
| AOT | Compila antecipadamente para código nativo |
| DLL | Arquivo em IL, multiplataforma |
| EXE | Executável específico para um sistema operacional |
| Publish | Prepara a aplicação para distribuição, reunindo todas as dependências |
|URL| https://learn.microsoft.com/pt-br/dotnet/core/tools/