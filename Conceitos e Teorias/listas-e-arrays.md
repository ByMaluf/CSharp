# Listas e Arrays em C#

## 📋 Índice
1. [O que são Arrays?](#o-que-são-arrays)
2. [Declaração e Inicialização de Arrays](#declaração-e-inicialização-de-arrays)
3. [Propriedades e Métodos de Arrays](#propriedades-e-métodos-de-arrays)
4. [Arrays Multidimensionais](#arrays-multidimensionais)
5. [Jagged Arrays (Arrays Irregulares)](#jagged-arrays-arrays-irregulares)
6. [O que são Listas (List\<T\>)?](#o-que-são-listas-listt)
7. [Operações com List\<T\>](#operações-com-listt)
8. [Array vs List](#array-vs-list)
9. [Outras Coleções](#outras-coleções)
10. [LINQ com Arrays e Listas](#linq-com-arrays-e-listas)
11. [Boas Práticas](#boas-práticas)
12. [Exemplos Práticos](#exemplos-práticos)

---

## O que são Arrays?

**Array** é uma estrutura de dados que armazena uma **coleção de elementos do mesmo tipo** em posições de memória **contíguas e indexadas**.

### Características Principais

- **Tamanho fixo**: Definido na criação, não pode ser alterado
- **Tipo homogêneo**: Todos os elementos devem ser do mesmo tipo
- **Indexado**: Acesso por índice começando em 0
- **Tipo de referência**: Arrays são objetos no heap
- **Performance**: Acesso rápido por índice (O(1))

```csharp
// Array de inteiros com 5 elementos
int[] numeros = new int[5];
numeros[0] = 10;  // Primeiro elemento
numeros[4] = 50;  // Último elemento

// Array de strings
string[] nomes = new string[3];
nomes[0] = "Ana";
nomes[1] = "Bruno";
nomes[2] = "Carlos";
```

---

## Declaração e Inicialização de Arrays

### Declaração Básica

```csharp
// Sintaxe: tipo[] nomeArray;
int[] numeros;
string[] nomes;
double[] valores;
bool[] flags;

// Array de objetos customizados
Pessoa[] pessoas;
```

### Inicialização

```csharp
// 1. Especificar tamanho (valores padrão)
int[] numeros = new int[5];  // [0, 0, 0, 0, 0]
string[] textos = new string[3];  // [null, null, null]
bool[] flags = new bool[2];  // [false, false]

// 2. Inicialização com valores
int[] numeros = new int[] { 1, 2, 3, 4, 5 };
string[] nomes = new string[] { "Ana", "Bruno", "Carlos" };

// 3. Sintaxe curta (mais comum)
int[] numeros = { 1, 2, 3, 4, 5 };
string[] nomes = { "Ana", "Bruno", "Carlos" };

// 4. Com var
var numeros = new int[] { 1, 2, 3, 4, 5 };
var nomes = new[] { "Ana", "Bruno", "Carlos" };  // Tipo inferido

// 5. Target-typed new (C# 9+)
int[] numeros = new[] { 1, 2, 3, 4, 5 };
string[] nomes = new[] { "Ana", "Bruno", "Carlos" };
```

### Valores Padrão

```csharp
int[] numeros = new int[3];        // [0, 0, 0]
double[] valores = new double[2];  // [0.0, 0.0]
bool[] flags = new bool[2];        // [false, false]
string[] textos = new string[2];   // [null, null]
Pessoa[] pessoas = new Pessoa[2];  // [null, null]
```

### Acesso aos Elementos

```csharp
int[] numeros = { 10, 20, 30, 40, 50 };

// Acesso por índice
int primeiro = numeros[0];    // 10
int ultimo = numeros[4];      // 50

// Modificação
numeros[2] = 100;  // [10, 20, 100, 40, 50]

// ❌ ERRO - índice fora dos limites
// int invalido = numeros[5];  // IndexOutOfRangeException!

// ✅ Acesso seguro
if (numeros.Length > 5)
{
    int valor = numeros[5];
}
```

---

## Propriedades e Métodos de Arrays

### Propriedades

```csharp
int[] numeros = { 1, 2, 3, 4, 5 };

// Length - número de elementos
int tamanho = numeros.Length;  // 5

// Rank - número de dimensões
int dimensoes = numeros.Rank;  // 1 (unidimensional)

// LongLength - para arrays grandes
long tamanhoLongo = numeros.LongLength;  // 5
```

### Métodos Comuns da Classe Array

```csharp
int[] numeros = { 5, 2, 8, 1, 9 };

// Sort - ordenar
Array.Sort(numeros);  // [1, 2, 5, 8, 9]

// Reverse - inverter
Array.Reverse(numeros);  // [9, 8, 5, 2, 1]

// IndexOf - encontrar índice
int indice = Array.IndexOf(numeros, 5);  // 2

// LastIndexOf - último índice
int ultimoIndice = Array.LastIndexOf(numeros, 5);

// Find - encontrar primeiro elemento que atende condição
int primeiro = Array.Find(numeros, n => n > 5);  // 9

// FindAll - encontrar todos que atendem condição
int[] maioresQue5 = Array.FindAll(numeros, n => n > 5);  // [9, 8]

// Exists - verifica se existe
bool existe = Array.Exists(numeros, n => n == 5);  // true

// Clear - limpar (valores padrão)
Array.Clear(numeros, 0, numeros.Length);  // [0, 0, 0, 0, 0]

// Copy - copiar elementos
int[] destino = new int[5];
Array.Copy(numeros, destino, 3);  // Copia 3 primeiros elementos

// Clone - criar cópia rasa
int[] clone = (int[])numeros.Clone();

// Resize - redimensionar (cria novo array)
Array.Resize(ref numeros, 10);  // Array agora tem 10 elementos
```

### Iteração

```csharp
int[] numeros = { 1, 2, 3, 4, 5 };

// For tradicional
for (int i = 0; i < numeros.Length; i++)
{
    Console.WriteLine(numeros[i]);
}

// Foreach
foreach (int numero in numeros)
{
    Console.WriteLine(numero);
}

// Foreach com índice (C# 7.0+)
foreach (var (valor, indice) in numeros.Select((v, i) => (v, i)))
{
    Console.WriteLine($"[{indice}] = {valor}");
}
```

---

## Arrays Multidimensionais

Arrays com **mais de uma dimensão** (matriz).

### Arrays Bidimensionais (Matriz)

```csharp
// Declaração e inicialização
int[,] matriz = new int[3, 4];  // 3 linhas, 4 colunas

// Inicialização com valores
int[,] matriz = new int[,]
{
    { 1, 2, 3, 4 },
    { 5, 6, 7, 8 },
    { 9, 10, 11, 12 }
};

// Sintaxe curta
int[,] matriz = 
{
    { 1, 2, 3, 4 },
    { 5, 6, 7, 8 },
    { 9, 10, 11, 12 }
};

// Acesso aos elementos
int valor = matriz[0, 0];  // 1 (linha 0, coluna 0)
int valor2 = matriz[2, 3]; // 12 (linha 2, coluna 3)

// Modificação
matriz[1, 2] = 100;

// Propriedades
int linhas = matriz.GetLength(0);    // 3
int colunas = matriz.GetLength(1);   // 4
int total = matriz.Length;           // 12 (3 x 4)

// Iteração
for (int i = 0; i < matriz.GetLength(0); i++)
{
    for (int j = 0; j < matriz.GetLength(1); j++)
    {
        Console.Write($"{matriz[i, j]}\t");
    }
    Console.WriteLine();
}
```

### Arrays Tridimensionais

```csharp
// Cubo 3D
int[,,] cubo = new int[3, 3, 3];

// Inicialização
int[,,] cubo = new int[,,]
{
    {
        { 1, 2, 3 },
        { 4, 5, 6 },
        { 7, 8, 9 }
    },
    {
        { 10, 11, 12 },
        { 13, 14, 15 },
        { 16, 17, 18 }
    },
    {
        { 19, 20, 21 },
        { 22, 23, 24 },
        { 25, 26, 27 }
    }
};

// Acesso
int valor = cubo[1, 2, 1];  // 17

// Iteração
for (int i = 0; i < cubo.GetLength(0); i++)
{
    for (int j = 0; j < cubo.GetLength(1); j++)
    {
        for (int k = 0; k < cubo.GetLength(2); k++)
        {
            Console.WriteLine($"[{i},{j},{k}] = {cubo[i, j, k]}");
        }
    }
}
```

---

## Jagged Arrays (Arrays Irregulares)

**Jagged array** é um **array de arrays**, onde cada "linha" pode ter tamanho diferente.

### Declaração e Inicialização

```csharp
// Declaração
int[][] jagged = new int[3][];

// Inicialização de cada array interno
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5, 6 };
jagged[2] = new int[] { 7, 8, 9 };

// Inicialização completa
int[][] jagged = new int[][]
{
    new int[] { 1, 2 },
    new int[] { 3, 4, 5, 6 },
    new int[] { 7, 8, 9 }
};

// Sintaxe curta
int[][] jagged =
{
    new[] { 1, 2 },
    new[] { 3, 4, 5, 6 },
    new[] { 7, 8, 9 }
};

// Acesso
int valor = jagged[0][1];  // 2
int valor2 = jagged[1][3]; // 6

// Propriedades
int linhas = jagged.Length;           // 3
int colunasLinha0 = jagged[0].Length; // 2
int colunasLinha1 = jagged[1].Length; // 4

// Iteração
for (int i = 0; i < jagged.Length; i++)
{
    for (int j = 0; j < jagged[i].Length; j++)
    {
        Console.Write($"{jagged[i][j]} ");
    }
    Console.WriteLine();
}

// Ou com foreach
foreach (int[] linha in jagged)
{
    foreach (int valor in linha)
    {
        Console.Write($"{valor} ");
    }
    Console.WriteLine();
}
```

### Jagged vs Multidimensional

```csharp
// Multidimensional - todas as linhas têm mesmo tamanho
int[,] matriz = new int[3, 4];  // SEMPRE 3x4

// Jagged - cada linha pode ter tamanho diferente
int[][] jagged = new int[3][];
jagged[0] = new int[2];   // Linha 0: 2 elementos
jagged[1] = new int[5];   // Linha 1: 5 elementos
jagged[2] = new int[3];   // Linha 2: 3 elementos
```

---

## O que são Listas (List\<T\>)?

`List<T>` é uma **coleção genérica** de tamanho **dinâmico** que armazena elementos do tipo `T`.

### Características Principais

- **Tamanho dinâmico**: Cresce e diminui automaticamente
- **Tipo genérico**: Type-safe com `<T>`
- **Indexado**: Acesso por índice como arrays
- **Performance**: Boa para acesso por índice
- **Namespace**: `System.Collections.Generic`

```csharp
using System.Collections.Generic;

// Criação
List<int> numeros = new List<int>();
List<string> nomes = new List<string>();
List<Pessoa> pessoas = new List<Pessoa>();

// Com capacidade inicial (otimização)
List<int> numeros = new List<int>(100);  // Reserva espaço para 100
```

---

## Operações com List\<T\>

### Criação e Inicialização

```csharp
// Lista vazia
List<int> numeros = new List<int>();

// Com valores iniciais
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };
List<string> nomes = new List<string> { "Ana", "Bruno", "Carlos" };

// Com capacidade inicial
List<int> numeros = new List<int>(1000);

// De um array
int[] array = { 1, 2, 3 };
List<int> lista = new List<int>(array);

// Target-typed new (C# 9+)
List<int> numeros = new() { 1, 2, 3, 4, 5 };

// Collection expression (C# 12+)
List<int> numeros = [1, 2, 3, 4, 5];
```

### Adicionar Elementos

```csharp
List<int> numeros = new List<int>();

// Add - adicionar no final
numeros.Add(10);
numeros.Add(20);
numeros.Add(30);  // [10, 20, 30]

// AddRange - adicionar múltiplos
numeros.AddRange(new[] { 40, 50, 60 });  // [10, 20, 30, 40, 50, 60]

// Insert - inserir em posição específica
numeros.Insert(0, 5);  // [5, 10, 20, 30, 40, 50, 60]
numeros.Insert(3, 25); // [5, 10, 20, 25, 30, 40, 50, 60]

// InsertRange - inserir múltiplos em posição
numeros.InsertRange(2, new[] { 15, 18 });
```

### Remover Elementos

```csharp
List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };

// Remove - remover primeira ocorrência
bool removido = numeros.Remove(30);  // true, [10, 20, 40, 50]

// RemoveAt - remover por índice
numeros.RemoveAt(0);  // [20, 40, 50]

// RemoveRange - remover intervalo
numeros.RemoveRange(1, 2);  // Remove 2 elementos a partir do índice 1

// RemoveAll - remover todos que atendem condição
numeros.RemoveAll(n => n > 30);  // Remove todos > 30

// Clear - remover todos
numeros.Clear();  // []
```

### Acessar e Modificar

```csharp
List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };

// Acesso por índice
int primeiro = numeros[0];   // 10
int ultimo = numeros[4];     // 50

// Modificação
numeros[2] = 100;  // [10, 20, 100, 40, 50]

// ❌ ERRO - índice fora dos limites
// int invalido = numeros[10];  // ArgumentOutOfRangeException!
```

### Busca e Verificação

```csharp
List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };

// Contains - verificar se existe
bool existe = numeros.Contains(30);  // true

// IndexOf - encontrar índice
int indice = numeros.IndexOf(30);    // 2
int naoEncontrado = numeros.IndexOf(100);  // -1

// LastIndexOf - último índice
int ultimo = numeros.LastIndexOf(30);

// Find - encontrar primeiro que atende condição
int primeiro = numeros.Find(n => n > 25);  // 30

// FindAll - encontrar todos que atendem
List<int> maioresQue25 = numeros.FindAll(n => n > 25);  // [30, 40, 50]

// FindIndex - índice do primeiro que atende
int indice = numeros.FindIndex(n => n > 25);  // 2

// Exists - verificar se existe algum que atende
bool algum = numeros.Exists(n => n > 100);  // false

// TrueForAll - verificar se todos atendem
bool todos = numeros.TrueForAll(n => n > 0);  // true
```

### Ordenação

```csharp
List<int> numeros = new List<int> { 50, 20, 40, 10, 30 };

// Sort - ordenar crescente
numeros.Sort();  // [10, 20, 30, 40, 50]

// Sort com comparador - decrescente
numeros.Sort((a, b) => b.CompareTo(a));  // [50, 40, 30, 20, 10]

// Reverse - inverter
numeros.Reverse();  // [10, 20, 30, 40, 50]

// OrderBy com LINQ
var ordenados = numeros.OrderBy(n => n).ToList();
var decrescente = numeros.OrderByDescending(n => n).ToList();
```

### Conversão

```csharp
List<int> lista = new List<int> { 1, 2, 3, 4, 5 };

// ToArray - converter para array
int[] array = lista.ToArray();

// AsReadOnly - criar versão somente leitura
IReadOnlyList<int> readOnly = lista.AsReadOnly();

// GetRange - obter sublista
List<int> sub = lista.GetRange(1, 3);  // [2, 3, 4]

// CopyTo - copiar para array
int[] destino = new int[10];
lista.CopyTo(destino, 0);
```

### Propriedades

```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };

// Count - número de elementos
int quantidade = numeros.Count;  // 5

// Capacity - capacidade atual
int capacidade = numeros.Capacity;  // Geralmente maior que Count

// Indexador
int primeiro = numeros[0];
int ultimo = numeros[^1];  // C# 8+: último elemento

// Range (C# 8+)
List<int> sub = numeros[1..4].ToList();  // [2, 3, 4]
```

---

## Array vs List

### Comparação Detalhada

| Aspecto | Array | List\<T\> |
|---------|-------|-----------|
| **Tamanho** | Fixo | Dinâmico |
| **Performance** | Mais rápido | Ligeiramente mais lento |
| **Adição/Remoção** | ❌ Difícil | ✅ Fácil |
| **Memória** | Mais eficiente | Overhead adicional |
| **Sintaxe** | Nativa | Requer using |
| **Métodos** | Limitados | Rico |
| **Type Safety** | Sim | Sim (genérico) |
| **Inicialização** | `new int[5]` | `new List<int>()` |

### Exemplos Comparativos

```csharp
// ========== ARRAY ==========
// ✅ Tamanho conhecido e fixo
int[] diasDoMes = new int[31];

// ✅ Performance crítica
int[] buffer = new int[1000000];

// ❌ Adicionar elemento é difícil
int[] numeros = { 1, 2, 3 };
// Para adicionar 4, precisa criar novo array!
int[] novo = new int[numeros.Length + 1];
Array.Copy(numeros, novo, numeros.Length);
novo[novo.Length - 1] = 4;

// ========== LIST<T> ==========
// ✅ Tamanho desconhecido ou variável
List<int> numeros = new List<int>();

// ✅ Adicionar/remover facilmente
numeros.Add(1);
numeros.Add(2);
numeros.Add(3);
numeros.Remove(2);  // Fácil!

// ✅ Métodos úteis
List<string> nomes = new List<string> { "Ana", "Bruno", "Carlos" };
nomes.Sort();
nomes.Reverse();
bool existe = nomes.Contains("Ana");
```

### Quando Usar Cada Um

```csharp
// ✅ Use Array quando:
// - Tamanho fixo e conhecido
// - Performance é crítica
// - Dados não mudam após criação
int[] diasSemana = { 0, 1, 2, 3, 4, 5, 6 };
byte[] buffer = new byte[1024];

// ✅ Use List quando:
// - Tamanho desconhecido ou variável
// - Precisa adicionar/remover elementos
// - Precisa de métodos de manipulação
List<Pedido> pedidos = new List<Pedido>();
pedidos.Add(novoPedido);
pedidos.RemoveAll(p => p.Status == Status.Cancelado);
```

---

## Outras Coleções

### ArrayList (Legado)

```csharp
// ❌ Evite - não é type-safe
ArrayList lista = new ArrayList();
lista.Add(1);
lista.Add("texto");  // Mistura tipos!
lista.Add(true);

// ✅ Prefira List<T>
List<int> numeros = new List<int>();
numeros.Add(1);
// numeros.Add("texto");  // ERRO em compile-time
```

### LinkedList\<T\>

Melhor para inserções/remoções frequentes no meio.

```csharp
LinkedList<int> lista = new LinkedList<int>();

// Adicionar
lista.AddFirst(1);
lista.AddLast(3);
lista.AddAfter(lista.First, 2);  // [1, 2, 3]

// Remover
lista.RemoveFirst();
lista.RemoveLast();

// Iterar
foreach (int valor in lista)
{
    Console.WriteLine(valor);
}

// ✅ Use quando: muitas inserções/remoções no meio
// ❌ Evite quando: acesso por índice frequente (O(n))
```

### Stack\<T\> (Pilha - LIFO)

```csharp
Stack<int> pilha = new Stack<int>();

// Push - adicionar no topo
pilha.Push(1);
pilha.Push(2);
pilha.Push(3);

// Pop - remover do topo
int topo = pilha.Pop();  // 3

// Peek - ver o topo sem remover
int proxim = pilha.Peek();  // 2

// Uso comum: desfazer/refazer
Stack<string> historico = new Stack<string>();
historico.Push("Ação 1");
historico.Push("Ação 2");
string desfazer = historico.Pop();  // Desfaz "Ação 2"
```

### Queue\<T\> (Fila - FIFO)

```csharp
Queue<int> fila = new Queue<int>();

// Enqueue - adicionar no final
fila.Enqueue(1);
fila.Enqueue(2);
fila.Enqueue(3);

// Dequeue - remover do início
int primeiro = fila.Dequeue();  // 1

// Peek - ver o primeiro sem remover
int proximo = fila.Peek();  // 2

// Uso comum: processamento de tarefas
Queue<Tarefa> filaProcessamento = new Queue<Tarefa>();
filaProcessamento.Enqueue(new Tarefa());
Tarefa processar = filaProcessamento.Dequeue();
```

### HashSet\<T\> (Conjunto)

Coleção de elementos **únicos** (sem duplicatas).

```csharp
HashSet<int> conjunto = new HashSet<int>();

// Add - adicionar (ignora duplicatas)
conjunto.Add(1);
conjunto.Add(2);
conjunto.Add(2);  // Ignorado
conjunto.Add(3);  // [1, 2, 3]

// Contains - verificar existência (O(1))
bool existe = conjunto.Contains(2);  // true

// Operações de conjunto
HashSet<int> a = new HashSet<int> { 1, 2, 3 };
HashSet<int> b = new HashSet<int> { 2, 3, 4 };

a.UnionWith(b);         // União: [1, 2, 3, 4]
a.IntersectWith(b);     // Interseção: [2, 3]
a.ExceptWith(b);        // Diferença: [1]

// ✅ Use quando: precisa garantir unicidade
List<int> comDuplicatas = new List<int> { 1, 2, 2, 3, 3, 3 };
HashSet<int> unicos = new HashSet<int>(comDuplicatas);  // [1, 2, 3]
```

### Dictionary\<TKey, TValue\>

Coleção de pares chave-valor.

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>();

// Add - adicionar
idades.Add("Ana", 25);
idades.Add("Bruno", 30);

// Indexador
idades["Carlos"] = 35;

// Acesso
int idadeAna = idades["Ana"];  // 25

// TryGetValue - acesso seguro
if (idades.TryGetValue("Ana", out int idade))
{
    Console.WriteLine(idade);
}

// ContainsKey
bool existe = idades.ContainsKey("Ana");

// Remove
idades.Remove("Bruno");

// Iterar
foreach (var par in idades)
{
    Console.WriteLine($"{par.Key}: {par.Value}");
}

// Ou separadamente
foreach (string nome in idades.Keys)
{
    Console.WriteLine(nome);
}
```

---

## LINQ com Arrays e Listas

LINQ (Language Integrated Query) fornece métodos poderosos para consultar coleções.

### Métodos Comuns

```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Where - filtrar
var pares = numeros.Where(n => n % 2 == 0).ToList();  // [2, 4, 6, 8, 10]

// Select - projetar/transformar
var dobrados = numeros.Select(n => n * 2).ToList();  // [2, 4, 6, 8, 10, ...]

// OrderBy / OrderByDescending - ordenar
var crescente = numeros.OrderBy(n => n).ToList();
var decrescente = numeros.OrderByDescending(n => n).ToList();

// First / FirstOrDefault - primeiro elemento
int primeiro = numeros.First();  // 1
int primeiroMaiorQue5 = numeros.First(n => n > 5);  // 6
int primeiroOuPadrao = numeros.FirstOrDefault(n => n > 100);  // 0

// Last / LastOrDefault - último
int ultimo = numeros.Last();  // 10

// Single / SingleOrDefault - único elemento
int unico = numeros.Single(n => n == 5);  // 5

// Any - verificar se existe algum
bool temPares = numeros.Any(n => n % 2 == 0);  // true

// All - verificar se todos atendem
bool todosPositivos = numeros.All(n => n > 0);  // true

// Count - contar
int quantosPares = numeros.Count(n => n % 2 == 0);  // 5

// Sum, Average, Min, Max
int soma = numeros.Sum();  // 55
double media = numeros.Average();  // 5.5
int minimo = numeros.Min();  // 1
int maximo = numeros.Max();  // 10

// Take / Skip - paginação
var primeiros5 = numeros.Take(5).ToList();  // [1, 2, 3, 4, 5]
var pula5 = numeros.Skip(5).ToList();  // [6, 7, 8, 9, 10]

// Distinct - remover duplicatas
List<int> comDuplicatas = new List<int> { 1, 2, 2, 3, 3, 3 };
var unicos = comDuplicatas.Distinct().ToList();  // [1, 2, 3]

// GroupBy - agrupar
var grupos = numeros.GroupBy(n => n % 2 == 0 ? "Par" : "Ímpar");
foreach (var grupo in grupos)
{
    Console.WriteLine($"{grupo.Key}: {string.Join(", ", grupo)}");
}
```

### Query Syntax

```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Sintaxe de consulta (semelhante a SQL)
var resultado = from n in numeros
                where n % 2 == 0
                orderby n descending
                select n * 2;

// Equivalente a:
var resultado2 = numeros
    .Where(n => n % 2 == 0)
    .OrderByDescending(n => n)
    .Select(n => n * 2);
```

---

## Boas Práticas

### 1. Prefira List\<T\> a Array para Coleções Dinâmicas

```csharp
// ❌ Evite
int[] numeros = new int[100];
int count = 0;

void Adicionar(int numero)
{
    if (count >= numeros.Length)
    {
        // Precisa criar novo array maior!
        int[] novo = new int[numeros.Length * 2];
        Array.Copy(numeros, novo, numeros.Length);
        numeros = novo;
    }
    numeros[count++] = numero;
}

// ✅ Prefira
List<int> numeros = new List<int>();

void Adicionar(int numero)
{
    numeros.Add(numero);  // Simples!
}
```

### 2. Use Capacidade Inicial se Souber o Tamanho

```csharp
// ❌ Sem capacidade - múltiplas realocações
List<int> numeros = new List<int>();
for (int i = 0; i < 10000; i++)
{
    numeros.Add(i);
}

// ✅ Com capacidade - uma alocação
List<int> numeros = new List<int>(10000);
for (int i = 0; i < 10000; i++)
{
    numeros.Add(i);
}
```

### 3. Use LINQ para Operações Complexas

```csharp
List<Produto> produtos = ObterProdutos();

// ❌ Loop manual
List<string> nomes = new List<string>();
foreach (var produto in produtos)
{
    if (produto.Preco > 100)
    {
        nomes.Add(produto.Nome.ToUpper());
    }
}

// ✅ LINQ
var nomes = produtos
    .Where(p => p.Preco > 100)
    .Select(p => p.Nome.ToUpper())
    .ToList();
```

### 4. Verifique Limites Antes de Acessar

```csharp
int[] numeros = { 1, 2, 3 };

// ❌ Perigoso
int valor = numeros[5];  // IndexOutOfRangeException!

// ✅ Verificação
if (index >= 0 && index < numeros.Length)
{
    int valor = numeros[index];
}

// ✅ Ou com TryGetValue pattern
bool TryGet<T>(T[] array, int index, out T value)
{
    if (index >= 0 && index < array.Length)
    {
        value = array[index];
        return true;
    }
    value = default!;
    return false;
}
```

### 5. Use foreach para Iteração Simples

```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };

// ❌ For desnecessariamente complexo
for (int i = 0; i < numeros.Count; i++)
{
    Console.WriteLine(numeros[i]);
}

// ✅ Foreach mais simples
foreach (int numero in numeros)
{
    Console.WriteLine(numero);
}

// ⚠️ Use for quando precisar do índice
for (int i = 0; i < numeros.Count; i++)
{
    Console.WriteLine($"[{i}] = {numeros[i]}");
}
```

### 6. Não Modifique Coleção Durante Iteração

```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };

// ❌ ERRO - InvalidOperationException
foreach (int numero in numeros)
{
    if (numero % 2 == 0)
        numeros.Remove(numero);  // ERRO!
}

// ✅ Use ToList() ou RemoveAll
numeros.RemoveAll(n => n % 2 == 0);

// ✅ Ou itere sobre cópia
foreach (int numero in numeros.ToList())
{
    if (numero % 2 == 0)
        numeros.Remove(numero);
}
```

### 7. Use AsReadOnly para Expor Coleções

```csharp
public class Turma
{
    private List<Aluno> _alunos = new List<Aluno>();
    
    // ❌ Expõe lista modificável
    public List<Aluno> Alunos => _alunos;
    
    // ✅ Expõe versão somente leitura
    public IReadOnlyList<Aluno> Alunos => _alunos.AsReadOnly();
    
    // Métodos para modificar
    public void AdicionarAluno(Aluno aluno)
    {
        _alunos.Add(aluno);
    }
}
```

---

## Exemplos Práticos

### 1. Gerenciamento de Contatos

```csharp
public class Contato
{
    public string Nome { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
}

public class AgendaContatos
{
    private List<Contato> _contatos = new List<Contato>();
    
    public void Adicionar(Contato contato)
    {
        _contatos.Add(contato);
    }
    
    public void Remover(string nome)
    {
        _contatos.RemoveAll(c => c.Nome.Equals(nome, 
            StringComparison.OrdinalIgnoreCase));
    }
    
    public Contato? Buscar(string nome)
    {
        return _contatos.FirstOrDefault(c => 
            c.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase));
    }
    
    public List<Contato> BuscarPorInicial(char inicial)
    {
        return _contatos
            .Where(c => c.Nome.StartsWith(inicial.ToString(), 
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Nome)
            .ToList();
    }
    
    public void ListarTodos()
    {
        foreach (var contato in _contatos.OrderBy(c => c.Nome))
        {
            Console.WriteLine($"{contato.Nome} - {contato.Telefone}");
        }
    }
}

// Uso
var agenda = new AgendaContatos();
agenda.Adicionar(new Contato { Nome = "Ana", Telefone = "111" });
agenda.Adicionar(new Contato { Nome = "Bruno", Telefone = "222" });
agenda.Adicionar(new Contato { Nome = "Carlos", Telefone = "333" });

var contato = agenda.Buscar("Ana");
var contatosA = agenda.BuscarPorInicial('A');
agenda.ListarTodos();
```

### 2. Sistema de Notas

```csharp
public class Aluno
{
    public string Nome { get; set; }
    public List<double> Notas { get; set; } = new List<double>();
    
    public double Media => Notas.Count > 0 ? Notas.Average() : 0;
    public double MaiorNota => Notas.Count > 0 ? Notas.Max() : 0;
    public double MenorNota => Notas.Count > 0 ? Notas.Min() : 0;
    
    public bool Aprovado(double mediaMinima = 7.0)
    {
        return Media >= mediaMinima;
    }
}

public class Turma
{
    private List<Aluno> _alunos = new List<Aluno>();
    
    public void AdicionarAluno(Aluno aluno)
    {
        _alunos.Add(aluno);
    }
    
    public void AdicionarNota(string nomeAluno, double nota)
    {
        var aluno = _alunos.FirstOrDefault(a => a.Nome == nomeAluno);
        aluno?.Notas.Add(nota);
    }
    
    public List<Aluno> ObterAprovados()
    {
        return _alunos.Where(a => a.Aprovado()).ToList();
    }
    
    public List<Aluno> ObterReprovados()
    {
        return _alunos.Where(a => !a.Aprovado()).ToList();
    }
    
    public double MediaGeral()
    {
        return _alunos.Average(a => a.Media);
    }
    
    public void GerarRelatorio()
    {
        Console.WriteLine("=== RELATÓRIO DA TURMA ===\n");
        
        foreach (var aluno in _alunos.OrderBy(a => a.Nome))
        {
            string status = aluno.Aprovado() ? "APROVADO" : "REPROVADO";
            Console.WriteLine($"{aluno.Nome}");
            Console.WriteLine($"  Notas: {string.Join(", ", aluno.Notas)}");
            Console.WriteLine($"  Média: {aluno.Media:F2}");
            Console.WriteLine($"  Status: {status}\n");
        }
        
        Console.WriteLine($"Média geral da turma: {MediaGeral():F2}");
        Console.WriteLine($"Aprovados: {ObterAprovados().Count}");
        Console.WriteLine($"Reprovados: {ObterReprovados().Count}");
    }
}

// Uso
var turma = new Turma();

var aluno1 = new Aluno { Nome = "Ana" };
aluno1.Notas.AddRange(new[] { 8.0, 7.5, 9.0 });
turma.AdicionarAluno(aluno1);

var aluno2 = new Aluno { Nome = "Bruno" };
aluno2.Notas.AddRange(new[] { 6.0, 5.5, 6.5 });
turma.AdicionarAluno(aluno2);

turma.GerarRelatorio();
```

### 3. Lista de Tarefas (To-Do List)

```csharp
public enum StatusTarefa
{
    Pendente,
    EmAndamento,
    Concluida
}

public class Tarefa
{
    public int Id { get; set; }
    public string Descricao { get; set; }
    public StatusTarefa Status { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataConclusao { get; set; }
}

public class GerenciadorTarefas
{
    private List<Tarefa> _tarefas = new List<Tarefa>();
    private int _proximoId = 1;
    
    public void Adicionar(string descricao)
    {
        _tarefas.Add(new Tarefa
        {
            Id = _proximoId++,
            Descricao = descricao,
            Status = StatusTarefa.Pendente,
            DataCriacao = DateTime.Now
        });
    }
    
    public void IniciarTarefa(int id)
    {
        var tarefa = _tarefas.FirstOrDefault(t => t.Id == id);
        if (tarefa != null)
        {
            tarefa.Status = StatusTarefa.EmAndamento;
        }
    }
    
    public void ConcluirTarefa(int id)
    {
        var tarefa = _tarefas.FirstOrDefault(t => t.Id == id);
        if (tarefa != null)
        {
            tarefa.Status = StatusTarefa.Concluida;
            tarefa.DataConclusao = DateTime.Now;
        }
    }
    
    public void RemoverTarefa(int id)
    {
        _tarefas.RemoveAll(t => t.Id == id);
    }
    
    public List<Tarefa> ListarPorStatus(StatusTarefa status)
    {
        return _tarefas
            .Where(t => t.Status == status)
            .OrderBy(t => t.DataCriacao)
            .ToList();
    }
    
    public void ListarTodas()
    {
        var grupos = _tarefas.GroupBy(t => t.Status);
        
        foreach (var grupo in grupos)
        {
            Console.WriteLine($"\n=== {grupo.Key.ToString().ToUpper()} ===");
            foreach (var tarefa in grupo)
            {
                Console.WriteLine($"[{tarefa.Id}] {tarefa.Descricao}");
                if (tarefa.DataConclusao.HasValue)
                {
                    var tempo = tarefa.DataConclusao.Value - tarefa.DataCriacao;
                    Console.WriteLine($"     Concluída em {tempo.TotalHours:F1}h");
                }
            }
        }
    }
}

// Uso
var gerenciador = new GerenciadorTarefas();
gerenciador.Adicionar("Estudar C#");
gerenciador.Adicionar("Fazer exercícios");
gerenciador.Adicionar("Revisar código");

gerenciador.IniciarTarefa(1);
gerenciador.ConcluirTarefa(1);

gerenciador.ListarTodas();
```

### 4. Carrinho de Compras

```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
}

public class ItemCarrinho
{
    public Produto Produto { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal => Produto.Preco * Quantidade;
}

public class CarrinhoCompras
{
    private List<ItemCarrinho> _itens = new List<ItemCarrinho>();
    
    public void Adicionar(Produto produto, int quantidade = 1)
    {
        var itemExistente = _itens.FirstOrDefault(i => 
            i.Produto.Id == produto.Id);
        
        if (itemExistente != null)
        {
            itemExistente.Quantidade += quantidade;
        }
        else
        {
            _itens.Add(new ItemCarrinho
            {
                Produto = produto,
                Quantidade = quantidade
            });
        }
    }
    
    public void Remover(int produtoId)
    {
        _itens.RemoveAll(i => i.Produto.Id == produtoId);
    }
    
    public void AlterarQuantidade(int produtoId, int novaQuantidade)
    {
        var item = _itens.FirstOrDefault(i => i.Produto.Id == produtoId);
        if (item != null)
        {
            if (novaQuantidade <= 0)
                _itens.Remove(item);
            else
                item.Quantidade = novaQuantidade;
        }
    }
    
    public decimal Total => _itens.Sum(i => i.Subtotal);
    
    public int TotalItens => _itens.Sum(i => i.Quantidade);
    
    public void Limpar()
    {
        _itens.Clear();
    }
    
    public void MostrarResumo()
    {
        Console.WriteLine("=== CARRINHO DE COMPRAS ===\n");
        
        foreach (var item in _itens)
        {
            Console.WriteLine($"{item.Produto.Nome}");
            Console.WriteLine($"  Quantidade: {item.Quantidade}");
            Console.WriteLine($"  Preço unit.: {item.Produto.Preco:C}");
            Console.WriteLine($"  Subtotal: {item.Subtotal:C}\n");
        }
        
        Console.WriteLine($"Total de itens: {TotalItens}");
        Console.WriteLine($"TOTAL: {Total:C}");
    }
}

// Uso
var carrinho = new CarrinhoCompras();

carrinho.Adicionar(new Produto { Id = 1, Nome = "Mouse", Preco = 50 }, 2);
carrinho.Adicionar(new Produto { Id = 2, Nome = "Teclado", Preco = 150 });
carrinho.Adicionar(new Produto { Id = 1, Nome = "Mouse", Preco = 50 }, 1);

carrinho.MostrarResumo();
```

### 5. Processamento de Dados com LINQ

```csharp
public class Venda
{
    public int Id { get; set; }
    public string Produto { get; set; }
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Vendedor { get; set; }
}

public class RelatorioVendas
{
    private List<Venda> _vendas;
    
    public RelatorioVendas(List<Venda> vendas)
    {
        _vendas = vendas;
    }
    
    public decimal TotalVendas()
    {
        return _vendas.Sum(v => v.Valor);
    }
    
    public decimal MediaVendas()
    {
        return _vendas.Average(v => v.Valor);
    }
    
    public var VendasPorVendedor()
    {
        return _vendas
            .GroupBy(v => v.Vendedor)
            .Select(g => new
            {
                Vendedor = g.Key,
                Quantidade = g.Count(),
                Total = g.Sum(v => v.Valor),
                Media = g.Average(v => v.Valor)
            })
            .OrderByDescending(x => x.Total)
            .ToList();
    }
    
    public var VendasPorMes()
    {
        return _vendas
            .GroupBy(v => new { v.Data.Year, v.Data.Month })
            .Select(g => new
            {
                Mes = $"{g.Key.Month:D2}/{g.Key.Year}",
                Quantidade = g.Count(),
                Total = g.Sum(v => v.Valor)
            })
            .OrderBy(x => x.Mes)
            .ToList();
    }
    
    public List<string> Top5Produtos()
    {
        return _vendas
            .GroupBy(v => v.Produto)
            .Select(g => new
            {
                Produto = g.Key,
                Quantidade = g.Count()
            })
            .OrderByDescending(x => x.Quantidade)
            .Take(5)
            .Select(x => x.Produto)
            .ToList();
    }
}
```

---

## 🎓 Resumo

| Tipo | Tamanho | Mutabilidade | Performance | Uso |
|------|---------|--------------|-------------|-----|
| **Array** | Fixo | Elementos mutáveis | Muito rápida | Tamanho conhecido |
| **List\<T\>** | Dinâmico | Mutável | Rápida | Uso geral |
| **LinkedList\<T\>** | Dinâmico | Mutável | Média | Inserções no meio |
| **Stack\<T\>** | Dinâmico | LIFO | Rápida | Pilha (desfazer) |
| **Queue\<T\>** | Dinâmico | FIFO | Rápida | Fila (processamento) |
| **HashSet\<T\>** | Dinâmico | Sem duplicatas | Rápida | Valores únicos |
| **Dictionary\<K,V\>** | Dinâmico | Pares chave-valor | Rápida | Lookup rápido |

### Complexidade de Operações

| Operação | Array | List\<T\> | LinkedList\<T\> |
|----------|-------|-----------|-----------------|
| Acesso por índice | O(1) | O(1) | O(n) |
| Adicionar no final | N/A | O(1)* | O(1) |
| Adicionar no início | N/A | O(n) | O(1) |
| Remover | N/A | O(n) | O(1)** |
| Busca | O(n) | O(n) | O(n) |

\* Amortizado  
\*\* Se tiver referência ao nó

---

## ✅ Checklist Rápido

**Use Array quando:**
- ✅ Tamanho fixo e conhecido
- ✅ Performance é crítica
- ✅ Dados não mudam após criação
- ✅ Uso de memória é limitado

**Use List\<T\> quando:**
- ✅ Tamanho desconhecido ou variável
- ✅ Precisa adicionar/remover elementos
- ✅ Precisa de métodos de manipulação
- ✅ Uso geral (padrão)

**Use LinkedList\<T\> quando:**
- ✅ Muitas inserções/remoções no meio
- ❌ Evite para acesso por índice

**Use HashSet\<T\> quando:**
- ✅ Precisa garantir valores únicos
- ✅ Verificações de existência (Contains)

**Use Dictionary\<K,V\> quando:**
- ✅ Lookup por chave
- ✅ Pares chave-valor

**Boas práticas:**
- ✅ Use capacidade inicial se souber tamanho
- ✅ Prefira LINQ para operações complexas
- ✅ Verifique limites antes de acessar
- ✅ Não modifique durante iteração
- ✅ Use AsReadOnly para expor coleções

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** Todas (arrays e coleções), 8.0+ (ranges), 9.0+ (target-typed new), 12.0+ (collection expressions)
