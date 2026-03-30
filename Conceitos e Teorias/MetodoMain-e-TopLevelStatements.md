# Método Main e Top-Level Statements em C#

## 📋 Índice
1. [O Método Main](#o-método-main)
2. [Por que `static`?](#por-que-static)
3. [Por que `void`?](#por-que-void)
4. [O que é `string[] args`?](#o-que-é-string-args)
5. [Top-Level Statements](#top-level-statements)
6. [Comparação: Tradicional vs Top-Level](#comparação-tradicional-vs-top-level)
7. [Quando Usar Cada Abordagem](#quando-usar-cada-abordagem)

---

## O Método Main

O método `Main` é o **ponto de entrada** de uma aplicação C#. É o primeiro método executado quando você roda seu programa.

### Estrutura Completa:
```csharp
namespace HelloWorld;

public class Program
{
    static void Main(string[] args)
    {
        // Seu código aqui
        Console.WriteLine("Hello, World!");
    }
}
```

---

## Por que `static`?

### O Problema sem `static`:

Imagine que o método `Main` **não fosse** `static`:

```csharp
public class Program
{
    void Main()  // Sem static
    {
        Console.WriteLine("Hello!");
    }
}
```

Para chamar um método **não-estático**, você precisa criar uma **instância** da classe:

```csharp
var programa = new Program();
programa.Main();  // Agora sim pode chamar
```

### Mas quem criaria essa instância?

Quando você executa seu programa, o .NET precisa chamar o método `Main` **ANTES** de qualquer código seu executar. Se `Main` não fosse `static`, o .NET teria que criar uma instância de `Program` primeiro... mas **como**, se o código que cria instâncias está dentro do próprio `Main`? 🤔

### Solução: `static`

Um método `static` pertence à **classe em si**, não a instâncias da classe:

```csharp
static void Main()
{
    // Pode ser chamado diretamente:
    // Program.Main()
}
```

O .NET consegue chamar `Main` **sem criar nenhum objeto**, apenas usando o nome da classe.

### Exemplo Comparativo:

```csharp
public class Pessoa
{
    public string Nome;
    
    // Método NÃO-estático: precisa de instância
    public void Falar()
    {
        Console.WriteLine($"Olá, eu sou {Nome}");
    }
    
    // Método estático: não precisa de instância
    public static void DizerOla()
    {
        Console.WriteLine("Olá!");
    }
}

// Uso:
var pessoa = new Pessoa();
pessoa.Nome = "Goku";
pessoa.Falar();  // Precisa de instância

Pessoa.DizerOla();  // Não precisa de instância
```

---

## Por que `void`?

`void` significa que o método **não retorna nenhum valor**.

### Tipos de Retorno:

```csharp
// void = não retorna nada
static void Main()
{
    Console.WriteLine("Executou!");
    // Não tem 'return' com valor
}

// int = retorna número inteiro
static int Main()
{
    Console.WriteLine("Executou!");
    return 0;  // 0 = sucesso, outro valor = erro
}

// string = retorna texto
static string ObterNome()
{
    return "Goku";
}
```

### Por que `Main` geralmente é `void`?

O método `Main` apenas **executa** código, não precisa retornar nada na maioria dos casos.

**Exceção:** Se você quiser retornar um **código de status** para o sistema operacional:

```csharp
static int Main()
{
    try
    {
        // Seu código
        return 0;  // Sucesso
    }
    catch
    {
        return 1;  // Erro
    }
}
```

No terminal:
```bash
dotnet run
echo $LASTEXITCODE  # Mostra o código de retorno (0 ou 1)
```

---

## O que é `string[] args`?

### Quebrando em Partes:

- **`string`** = tipo texto
- **`[]`** = array (lista)
- **`args`** = abreviação de "arguments" (argumentos)

Portanto: `string[] args` = **lista de textos com argumentos**

### Para que Serve?

Quando você executa um programa pela linha de comando, pode passar **argumentos**:

```bash
dotnet run argumento1 argumento2 argumento3
```

Esses argumentos ficam disponíveis no array `args`:

```csharp
static void Main(string[] args)
{
    // args[0] = "argumento1"
    // args[1] = "argumento2"
    // args[2] = "argumento3"
    
    Console.WriteLine($"Você passou {args.Length} argumentos");
}
```

### Exemplo Prático 1: Saudação Personalizada

```csharp
static void Main(string[] args)
{
    if (args.Length > 0)
    {
        Console.WriteLine($"Olá, {args[0]}!");
    }
    else
    {
        Console.WriteLine("Olá, mundo!");
    }
}
```

**Executando:**
```bash
dotnet run Goku
# Saída: Olá, Goku!

dotnet run
# Saída: Olá, mundo!
```

### Exemplo Prático 2: Calculadora Simples

```csharp
static void Main(string[] args)
{
    if (args.Length < 3)
    {
        Console.WriteLine("Uso: dotnet run <numero1> <operacao> <numero2>");
        return;
    }
    
    double num1 = double.Parse(args[0]);
    string operacao = args[1];
    double num2 = double.Parse(args[2]);
    
    double resultado = operacao switch
    {
        "+" => num1 + num2,
        "-" => num1 - num2,
        "*" => num1 * num2,
        "/" => num1 / num2,
        _ => 0
    };
    
    Console.WriteLine($"{num1} {operacao} {num2} = {resultado}");
}
```

**Executando:**
```bash
dotnet run 10 + 5
# Saída: 10 + 5 = 15

dotnet run 20 * 3
# Saída: 20 * 3 = 60
```

### E se Eu Não Usar `args`?

Pode omitir o parâmetro:

```csharp
static void Main()  // Sem parâmetros
{
    Console.WriteLine("Não preciso de argumentos!");
}
```

---

## Top-Level Statements

**Introduzido no C# 9 (.NET 5)** em 2020, permite escrever programas **sem** toda a estrutura tradicional.

### O Problema que Resolve:

Para um programa simples, você precisava escrever **muito código repetitivo**:

```csharp
namespace MeuPrograma;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");  // ← Só isso importa!
    }
}
```

### A Solução: Top-Level Statements

Escreva apenas o código que importa:

```csharp
Console.WriteLine("Hello, World!");
```

**Pronto!** O compilador gera automaticamente:
- O namespace
- A classe `Program`
- O método `static void Main()`

### Como Funciona por Baixo dos Panos:

Quando você escreve:
```csharp
Console.WriteLine("Hello!");
```

O compilador gera (invisível para você):
```csharp
// <Gerado automaticamente>
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello!");
    }
}
```

### Usando Funções com Top-Level:

```csharp
// Top-level statements
MostrarMensagem("Oi, eu sou o Goku!");
MostrarMensagem("Kamehameha!");

// Função local
void MostrarMensagem(string texto)
{
    Console.WriteLine($">>> {texto}");
}
```

### Acessando `args` com Top-Level:

A variável `args` fica disponível **automaticamente**:

```csharp
// Não precisa declarar 'args'!
if (args.Length > 0)
{
    Console.WriteLine($"Olá, {args[0]}!");
}
else
{
    Console.WriteLine("Olá, mundo!");
}
```

### Exemplo Completo: Jogo de Adivinhação

**Com Top-Level Statements:**
```csharp
using System;

Random random = new Random();
int numeroSecreto = random.Next(1, 101);

Console.WriteLine("🎮 Adivinhe o número entre 1 e 100!");

while (true)
{
    Console.Write("Seu palpite: ");
    int palpite = int.Parse(Console.ReadLine()!);
    
    if (palpite == numeroSecreto)
    {
        Console.WriteLine("🎉 Parabéns! Você acertou!");
        break;
    }
    else if (palpite < numeroSecreto)
    {
        Console.WriteLine("📈 Muito baixo!");
    }
    else
    {
        Console.WriteLine("📉 Muito alto!");
    }
}
```

**Versão Tradicional (para comparação):**
```csharp
using System;

namespace JogoAdivinhacao
{
    public class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            int numeroSecreto = random.Next(1, 101);

            Console.WriteLine("🎮 Adivinhe o número entre 1 e 100!");

            while (true)
            {
                Console.Write("Seu palpite: ");
                int palpite = int.Parse(Console.ReadLine()!);
                
                if (palpite == numeroSecreto)
                {
                    Console.WriteLine("🎉 Parabéns! Você acertou!");
                    break;
                }
                else if (palpite < numeroSecreto)
                {
                    Console.WriteLine("📈 Muito baixo!");
                }
                else
                {
                    Console.WriteLine("📉 Muito alto!");
                }
            }
        }
    }
}
```

---

## Comparação: Tradicional vs Top-Level

### 1. Programa Básico

| **Tradicional** | **Top-Level** |
|----------------|---------------|
| 9 linhas | 1 linha |

**Tradicional:**
```csharp
namespace HelloWorld;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello!");
    }
}
```

**Top-Level:**
```csharp
Console.WriteLine("Hello!");
```

---

### 2. Com Funções

**Tradicional:**
```csharp
namespace MeuApp;

public class Program
{
    static void Main(string[] args)
    {
        Saudar("Goku");
    }
    
    static void Saudar(string nome)
    {
        Console.WriteLine($"Olá, {nome}!");
    }
}
```

**Top-Level:**
```csharp
Saudar("Goku");

void Saudar(string nome)
{
    Console.WriteLine($"Olá, {nome}!");
}
```

---

### 3. Com Classes

**Tradicional:**
```csharp
namespace MeuApp;

public class Program
{
    static void Main(string[] args)
    {
        var pessoa = new Pessoa { Nome = "Goku" };
        pessoa.Falar();
    }
}

public class Pessoa
{
    public string Nome { get; set; }
    
    public void Falar()
    {
        Console.WriteLine($"Olá, eu sou {Nome}!");
    }
}
```

**Top-Level:**
```csharp
var pessoa = new Pessoa { Nome = "Goku" };
pessoa.Falar();

public class Pessoa
{
    public string Nome { get; set; }
    
    public void Falar()
    {
        Console.WriteLine($"Olá, eu sou {Nome}!");
    }
}
```

---

## Quando Usar Cada Abordagem

### ✅ Use Top-Level Statements Quando:

- Programa é **simples** e pequeno
- Está **aprendendo** C#
- Fazendo **scripts rápidos** ou **protótipos**
- Quer **menos código repetitivo**
- Projeto tipo console app simples

**Exemplos:**
- Scripts de automação
- Ferramentas de linha de comando simples
- Tutoriais e exemplos
- Testes rápidos de conceitos

---

### ✅ Use Forma Tradicional Quando:

- Projeto é **grande** e complexo
- Tem **múltiplas classes** e arquivos
- Seguindo **padrões empresariais**
- Precisa de **mais controle** sobre a estrutura
- Trabalhando em **equipe** (alguns preferem explícito)

**Exemplos:**
- APIs Web (ASP.NET Core)
- Aplicações enterprise
- Bibliotecas (class libraries)
- Projetos com arquitetura complexa

---

## 📊 Tabela Resumo

| Aspecto | Tradicional | Top-Level |
|---------|------------|-----------|
| **Linhas de código** | Mais | Menos |
| **Clareza estrutural** | Explícita | Implícita |
| **Ideal para iniciantes** | Não | ✅ Sim |
| **Ideal para projetos grandes** | ✅ Sim | Não |
| **Código repetitivo** | Sim | Não |
| **Controle total** | ✅ Sim | Limitado |
| **Versão C#** | Todas | 9+ (2020) |

---

## 💡 Dicas Finais

1. **Não misture as abordagens** no mesmo arquivo
2. **Top-level só pode ter um arquivo** por projeto
3. **`args` está sempre disponível** em top-level
4. **Você pode adicionar classes** após o top-level code
5. **`using` statements** vão no topo, sempre

### Exemplo Correto com Top-Level:

```csharp
using System;
using System.Collections.Generic;

// Top-level statements
Console.WriteLine("Iniciando...");

var numeros = new List<int> { 1, 2, 3, 4, 5 };
MostrarNumeros(numeros);

// Funções e classes após o código top-level
void MostrarNumeros(List<int> lista)
{
    foreach (var num in lista)
    {
        Console.WriteLine(num);
    }
}

public class MinhaClasse
{
    public void FazerAlgo() { }
}
```

---

## 🎓 Conclusão

- **`static`**: permite chamar sem criar instância
- **`void`**: não retorna valor
- **`string[] args`**: argumentos da linha de comando
- **Top-Level Statements**: forma simplificada para código direto

Ambas as abordagens são válidas. Escolha baseado no **tamanho** e **complexidade** do seu projeto!

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** 9+ (Top-Level), Todas (Tradicional)
