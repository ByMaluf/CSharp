# Dicionários em C#

## 📋 Índice
1. [O que é um Dictionary?](#o-que-é-um-dictionary)
2. [Declaração e Inicialização](#declaração-e-inicialização)
3. [Adicionar e Modificar Elementos](#adicionar-e-modificar-elementos)
4. [Acessar Valores](#acessar-valores)
5. [Remover Elementos](#remover-elementos)
6. [Verificações e Buscas](#verificações-e-buscas)
7. [Iterar sobre Dictionary](#iterar-sobre-dictionary)
8. [Propriedades e Métodos](#propriedades-e-métodos)
9. [Performance e Complexidade](#performance-e-complexidade)
10. [Variantes de Dictionary](#variantes-de-dictionary)
11. [LINQ com Dictionary](#linq-com-dictionary)
12. [Boas Práticas](#boas-práticas)
13. [Exemplos Práticos](#exemplos-práticos)

---

## O que é um Dictionary?

`Dictionary<TKey, TValue>` é uma **coleção genérica** que armazena pares **chave-valor**, onde cada chave é **única** e mapeia para um valor específico.

### Características Principais

- **Chaves únicas**: Cada chave pode aparecer apenas uma vez
- **Lookup rápido**: Busca por chave em O(1) em média
- **Tipo genérico**: Type-safe com `<TKey, TValue>`
- **Não ordenado**: Não garante ordem de inserção
- **Hash table**: Implementado usando tabela hash
- **Namespace**: `System.Collections.Generic`

```csharp
using System.Collections.Generic;

// Dictionary de string para int
Dictionary<string, int> idades = new Dictionary<string, int>();
idades["Ana"] = 25;
idades["Bruno"] = 30;
idades["Carlos"] = 35;

// Busca rápida por chave
int idadeAna = idades["Ana"];  // 25
```

### Conceito de Chave-Valor

```csharp
// Chave → Valor
// Nome → Idade
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },      // "Ana" é a chave, 25 é o valor
    { "Bruno", 30 },    // "Bruno" é a chave, 30 é o valor
    { "Carlos", 35 }    // "Carlos" é a chave, 35 é o valor
};

// Cada chave mapeia para UM valor
int idade = idades["Ana"];  // Busca pela chave "Ana" retorna 25
```

---

## Declaração e Inicialização

### Declaração Básica

```csharp
// Sintaxe: Dictionary<TipoChave, TipoValor> nome;
Dictionary<string, int> idades;
Dictionary<int, string> nomes;
Dictionary<string, List<int>> dados;
Dictionary<int, Pessoa> pessoas;
```

### Inicialização

```csharp
// 1. Construtor padrão (vazio)
Dictionary<string, int> idades = new Dictionary<string, int>();

// 2. Com capacidade inicial (otimização)
Dictionary<string, int> idades = new Dictionary<string, int>(100);

// 3. Inicialização de coleção (C# 3.0+)
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// 4. Sintaxe de indexador (C# 6.0+)
Dictionary<string, int> idades = new Dictionary<string, int>
{
    ["Ana"] = 25,
    ["Bruno"] = 30,
    ["Carlos"] = 35
};

// 5. Target-typed new (C# 9.0+)
Dictionary<string, int> idades = new()
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// Ou com indexador
Dictionary<string, int> idades = new()
{
    ["Ana"] = 25,
    ["Bruno"] = 30,
    ["Carlos"] = 35
};

// 6. De uma sequência (LINQ)
var pessoas = new[] 
{ 
    new { Nome = "Ana", Idade = 25 },
    new { Nome = "Bruno", Idade = 30 }
};
Dictionary<string, int> idades = pessoas.ToDictionary(p => p.Nome, p => p.Idade);
```

### Inicialização com Comparador Customizado

```csharp
// Case-insensitive (ignora maiúsculas/minúsculas)
Dictionary<string, int> idades = new Dictionary<string, int>(
    StringComparer.OrdinalIgnoreCase);

idades["ana"] = 25;
idades["ANA"] = 30;  // Sobrescreve, pois "ana" == "ANA" (case-insensitive)

Console.WriteLine(idades["Ana"]);  // 30
```

---

## Adicionar e Modificar Elementos

### Método Add

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>();

// Add(chave, valor) - adicionar novo par
idades.Add("Ana", 25);
idades.Add("Bruno", 30);
idades.Add("Carlos", 35);

// ❌ ERRO - chave duplicada lança exceção
// idades.Add("Ana", 26);  // ArgumentException!

// ✅ Verificar antes de adicionar
if (!idades.ContainsKey("Ana"))
{
    idades.Add("Ana", 26);
}

// ✅ Ou use TryAdd (C# 7.0+)
bool adicionado = idades.TryAdd("Ana", 26);  // false (já existe)
bool adicionado2 = idades.TryAdd("Diana", 22);  // true
```

### Indexador (Preferido)

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>();

// Indexador - adicionar ou atualizar
idades["Ana"] = 25;     // Adiciona
idades["Bruno"] = 30;   // Adiciona
idades["Ana"] = 26;     // Atualiza (não lança exceção)

// ✅ Sempre seguro - adiciona se não existe, atualiza se existe
idades["Carlos"] = 35;
```

### Comparação Add vs Indexador

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>();

// ========== ADD ==========
// ✅ Garante que é novo (lança exceção se duplicado)
try
{
    idades.Add("Ana", 25);
    idades.Add("Ana", 26);  // ❌ ArgumentException!
}
catch (ArgumentException)
{
    Console.WriteLine("Chave já existe!");
}

// ========== INDEXADOR ==========
// ✅ Adiciona ou atualiza sem exceção
idades["Ana"] = 25;
idades["Ana"] = 26;  // ✅ Atualiza silenciosamente

// Regra de ouro:
// - Use Add quando quer GARANTIR que é nova chave
// - Use Indexador quando quer ADICIONAR OU ATUALIZAR
```

---

## Acessar Valores

### Indexador

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// Acesso por chave
int idadeAna = idades["Ana"];  // 25

// ❌ ERRO - chave inexistente lança exceção
// int idade = idades["Diana"];  // KeyNotFoundException!
```

### TryGetValue (Recomendado)

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// ✅ Forma segura - não lança exceção
if (idades.TryGetValue("Ana", out int idade))
{
    Console.WriteLine($"Ana tem {idade} anos");
}
else
{
    Console.WriteLine("Pessoa não encontrada");
}

// Chave inexistente
if (idades.TryGetValue("Diana", out int idadeDiana))
{
    Console.WriteLine(idadeDiana);
}
else
{
    Console.WriteLine("Diana não está no dicionário");  // Executa isso
}
```

### GetValueOrDefault (C# 7.0+)

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Retorna valor ou default se não existir
int idade1 = idades.GetValueOrDefault("Ana");     // 25
int idade2 = idades.GetValueOrDefault("Diana");   // 0 (default de int)

// Com valor padrão customizado
int idade3 = idades.GetValueOrDefault("Diana", -1);  // -1
```

### Comparação de Métodos de Acesso

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 }
};

// ❌ Indexador - lança exceção se não existe
try
{
    int idade = idades["Diana"];  // KeyNotFoundException!
}
catch (KeyNotFoundException)
{
    Console.WriteLine("Chave não encontrada");
}

// ✅ TryGetValue - forma mais segura e eficiente
if (idades.TryGetValue("Diana", out int idade))
{
    Console.WriteLine(idade);
}
else
{
    Console.WriteLine("Não encontrado");
}

// ✅ GetValueOrDefault - quando quer valor padrão
int idade2 = idades.GetValueOrDefault("Diana", 0);

// ✅ ContainsKey + Indexador - quando tem certeza que existe depois
if (idades.ContainsKey("Ana"))
{
    int idade3 = idades["Ana"];  // Seguro
}
```

---

## Remover Elementos

### Método Remove

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// Remove - remover por chave
bool removido = idades.Remove("Bruno");  // true
bool removido2 = idades.Remove("Diana"); // false (não existe)

// Remove com out parameter (C# 7.0+)
if (idades.Remove("Ana", out int idadeRemovida))
{
    Console.WriteLine($"Removido: Ana, {idadeRemovida} anos");
}
```

### Clear

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Clear - remover todos
idades.Clear();
Console.WriteLine(idades.Count);  // 0
```

---

## Verificações e Buscas

### ContainsKey

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Verificar se chave existe
bool temAna = idades.ContainsKey("Ana");      // true
bool temCarlos = idades.ContainsKey("Carlos"); // false

// Uso comum
if (idades.ContainsKey("Ana"))
{
    int idade = idades["Ana"];  // Seguro acessar
}
```

### ContainsValue

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Verificar se valor existe (busca linear - O(n))
bool tem25 = idades.ContainsValue(25);  // true
bool tem40 = idades.ContainsValue(40);  // false

// ⚠️ Performance: ContainsValue é O(n), ContainsKey é O(1)
```

### Keys e Values

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// Coleção de chaves
var chaves = idades.Keys;  // ["Ana", "Bruno", "Carlos"]

// Coleção de valores
var valores = idades.Values;  // [25, 30, 35]

// Uso
foreach (string nome in idades.Keys)
{
    Console.WriteLine(nome);
}

foreach (int idade in idades.Values)
{
    Console.WriteLine(idade);
}

// Converter para lista
List<string> listaChaves = idades.Keys.ToList();
List<int> listaValores = idades.Values.ToList();
```

---

## Iterar sobre Dictionary

### Foreach com KeyValuePair

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// Forma 1: Tipo explícito
foreach (KeyValuePair<string, int> par in idades)
{
    Console.WriteLine($"{par.Key}: {par.Value} anos");
}

// Forma 2: var
foreach (var par in idades)
{
    Console.WriteLine($"{par.Key}: {par.Value} anos");
}

// Forma 3: Deconstruction (C# 7.0+)
foreach (var (nome, idade) in idades)
{
    Console.WriteLine($"{nome}: {idade} anos");
}
```

### Iterar sobre Keys ou Values

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Apenas chaves
foreach (string nome in idades.Keys)
{
    Console.WriteLine(nome);
}

// Apenas valores
foreach (int idade in idades.Values)
{
    Console.WriteLine(idade);
}

// Chaves com acesso ao valor
foreach (string nome in idades.Keys)
{
    int idade = idades[nome];
    Console.WriteLine($"{nome}: {idade}");
}
```

### Modificar Durante Iteração

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 }
};

// ❌ ERRO - não pode modificar estrutura durante iteração
foreach (var par in idades)
{
    if (par.Value > 30)
    {
        // idades.Remove(par.Key);  // InvalidOperationException!
    }
}

// ✅ Solução 1: Iterar sobre cópia das chaves
foreach (string nome in idades.Keys.ToList())
{
    if (idades[nome] > 30)
    {
        idades.Remove(nome);  // OK
    }
}

// ✅ Solução 2: Coletar chaves para remover
List<string> chavesParaRemover = new List<string>();
foreach (var par in idades)
{
    if (par.Value > 30)
    {
        chavesParaRemover.Add(par.Key);
    }
}
foreach (string chave in chavesParaRemover)
{
    idades.Remove(chave);
}

// ✅ Solução 3: LINQ
idades = idades.Where(p => p.Value <= 30).ToDictionary(p => p.Key, p => p.Value);
```

---

## Propriedades e Métodos

### Propriedades

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Count - número de pares chave-valor
int total = idades.Count;  // 2

// Keys - coleção de chaves
ICollection<string> chaves = idades.Keys;

// Values - coleção de valores
ICollection<int> valores = idades.Values;

// Comparer - comparador usado
IEqualityComparer<string> comparador = idades.Comparer;
```

### Métodos Principais

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>();

// Add(key, value) - adicionar (lança exceção se duplicado)
idades.Add("Ana", 25);

// TryAdd(key, value) - tentar adicionar (C# 7.0+)
bool adicionado = idades.TryAdd("Bruno", 30);  // true

// Remove(key) - remover por chave
bool removido = idades.Remove("Ana");

// Remove(key, out value) - remover e obter valor (C# 7.0+)
if (idades.Remove("Bruno", out int idade))
{
    Console.WriteLine($"Removido: {idade}");
}

// Clear() - limpar tudo
idades.Clear();

// ContainsKey(key) - verificar chave
bool existe = idades.ContainsKey("Ana");

// ContainsValue(value) - verificar valor
bool temValor = idades.ContainsValue(25);

// TryGetValue(key, out value) - obter valor seguro
if (idades.TryGetValue("Ana", out int idadeAna))
{
    Console.WriteLine(idadeAna);
}

// GetValueOrDefault(key) - obter ou default (C# 7.0+)
int valor = idades.GetValueOrDefault("Ana");

// GetValueOrDefault(key, defaultValue) - com padrão custom
int valor2 = idades.GetValueOrDefault("Diana", -1);
```

---

## Performance e Complexidade

### Complexidade Temporal

| Operação | Complexidade | Observação |
|----------|--------------|------------|
| Add | O(1)* | Amortizado |
| Remove | O(1)* | Amortizado |
| ContainsKey | O(1)* | Amortizado |
| TryGetValue | O(1)* | Amortizado |
| Indexador `[]` | O(1)* | Amortizado |
| ContainsValue | O(n) | Percorre todos valores |
| Clear | O(n) | Limpa todas entradas |

\* Em média. Pior caso pode ser O(n) devido a colisões de hash.

### Hash Table Internamente

```csharp
// Dictionary usa hash table
Dictionary<string, int> dict = new Dictionary<string, int>();

// 1. Calcula hash da chave
int hash = "Ana".GetHashCode();  // Ex: 1234567

// 2. Usa hash para encontrar bucket (índice interno)
int bucket = hash % dict.Capacity;

// 3. Armazena par chave-valor no bucket
// 4. Em caso de colisão, usa chaining (lista ligada)

// Performance depende de:
// - Boa função de hash
// - Poucos colisões
// - Load factor adequado
```

### Capacidade e Load Factor

```csharp
// Capacidade inicial padrão: 0
Dictionary<string, int> dict1 = new Dictionary<string, int>();

// Com capacidade inicial (otimização)
Dictionary<string, int> dict2 = new Dictionary<string, int>(1000);

// Benefícios:
// - Evita redimensionamentos
// - Melhor performance se souber tamanho aproximado

// Exemplo de impacto
var sw = System.Diagnostics.Stopwatch.StartNew();

// ❌ Sem capacidade - múltiplos redimensionamentos
Dictionary<int, int> dict3 = new Dictionary<int, int>();
for (int i = 0; i < 100000; i++)
{
    dict3[i] = i;
}
sw.Stop();
Console.WriteLine($"Sem capacidade: {sw.ElapsedMilliseconds}ms");

sw.Restart();

// ✅ Com capacidade - sem redimensionamentos
Dictionary<int, int> dict4 = new Dictionary<int, int>(100000);
for (int i = 0; i < 100000; i++)
{
    dict4[i] = i;
}
sw.Stop();
Console.WriteLine($"Com capacidade: {sw.ElapsedMilliseconds}ms");
```

### Comparação com Outras Coleções

| Operação | Dictionary | List | Array | HashSet |
|----------|-----------|------|-------|---------|
| Busca por chave | O(1) | O(n) | O(n) | O(1) |
| Adicionar | O(1) | O(1)* | N/A | O(1) |
| Remover | O(1) | O(n) | N/A | O(1) |
| Acesso por índice | N/A | O(1) | O(1) | N/A |
| Chaves únicas | Sim | Não | Não | Sim |
| Pares chave-valor | Sim | Não | Não | Não |

---

## Variantes de Dictionary

### SortedDictionary\<TKey, TValue\>

Mantém chaves ordenadas.

```csharp
// Chaves ordenadas automaticamente
SortedDictionary<string, int> idades = new SortedDictionary<string, int>
{
    { "Carlos", 35 },
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Iteração em ordem alfabética
foreach (var par in idades)
{
    Console.WriteLine($"{par.Key}: {par.Value}");
}
// Ana: 25
// Bruno: 30
// Carlos: 35

// Performance:
// - Add/Remove/Lookup: O(log n) (árvore balanceada)
// - Dictionary: O(1) mas não ordenado
// - SortedDictionary: O(log n) mas ordenado
```

### ConcurrentDictionary\<TKey, TValue\>

Thread-safe para acesso concorrente.

```csharp
using System.Collections.Concurrent;

// Thread-safe
ConcurrentDictionary<string, int> idades = new ConcurrentDictionary<string, int>();

// AddOrUpdate - adiciona ou atualiza atomicamente
idades.AddOrUpdate(
    "Ana",
    25,  // Valor se adicionar
    (chave, valorAntigo) => valorAntigo + 1  // Update se já existe
);

// GetOrAdd - obter ou adicionar
int idade = idades.GetOrAdd("Bruno", 30);

// TryAdd - tentar adicionar
bool adicionado = idades.TryAdd("Carlos", 35);

// TryRemove - tentar remover
if (idades.TryRemove("Ana", out int idadeRemovida))
{
    Console.WriteLine($"Removido: {idadeRemovida}");
}

// Uso em cenários multi-thread
Parallel.For(0, 1000, i =>
{
    idades.TryAdd($"Pessoa{i}", i);  // Seguro
});
```

### SortedList\<TKey, TValue\>

Híbrido entre lista e dicionário ordenado.

```csharp
// Ordenado, usa menos memória que SortedDictionary
SortedList<string, int> idades = new SortedList<string, int>
{
    { "Carlos", 35 },
    { "Ana", 25 },
    { "Bruno", 30 }
};

// Acesso por índice (diferente de Dictionary)
string primeiraChave = idades.Keys[0];  // "Ana"
int primeiroValor = idades.Values[0];   // 25

// Comparação:
// SortedDictionary: mais rápido para inserção/remoção
// SortedList: mais rápido para busca, usa menos memória
```

### ImmutableDictionary\<TKey, TValue\>

Imutável (não pode ser modificado).

```csharp
using System.Collections.Immutable;

// Criar imutável
ImmutableDictionary<string, int> idades = ImmutableDictionary.Create<string, int>();

// "Modificações" retornam nova instância
ImmutableDictionary<string, int> novaIdades = idades.Add("Ana", 25);
novaIdades = novaIdades.Add("Bruno", 30);

// Original não muda
Console.WriteLine(idades.Count);      // 0
Console.WriteLine(novaIdades.Count);  // 2

// Builder para múltiplas operações
var builder = ImmutableDictionary.CreateBuilder<string, int>();
builder.Add("Ana", 25);
builder.Add("Bruno", 30);
ImmutableDictionary<string, int> resultado = builder.ToImmutable();
```

---

## LINQ com Dictionary

### Consultas LINQ

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 },
    { "Diana", 28 },
    { "Eduardo", 32 }
};

// Where - filtrar
var maioresDe30 = idades.Where(p => p.Value > 30);
foreach (var par in maioresDe30)
{
    Console.WriteLine($"{par.Key}: {par.Value}");
}

// Select - projetar
var nomes = idades.Select(p => p.Key).ToList();
var idadesDobradas = idades.Select(p => new { p.Key, Dobro = p.Value * 2 });

// OrderBy / OrderByDescending - ordenar
var ordenadoPorIdade = idades.OrderBy(p => p.Value);
var ordenadoPorNome = idades.OrderBy(p => p.Key);

// First / FirstOrDefault
var primeiro = idades.First(p => p.Value > 30);  // KeyValuePair
var primeiroOuPadrao = idades.FirstOrDefault(p => p.Value > 100);

// Any / All
bool temMaiorDe30 = idades.Any(p => p.Value > 30);  // true
bool todosMaioresDe18 = idades.All(p => p.Value > 18);  // true

// Count
int quantosMaioresDe30 = idades.Count(p => p.Value > 30);

// Sum / Average / Min / Max
int somaIdades = idades.Sum(p => p.Value);
double mediaIdades = idades.Average(p => p.Value);
int idadeMinima = idades.Min(p => p.Value);
int idadeMaxima = idades.Max(p => p.Value);

// GroupBy - agrupar por faixa etária
var grupos = idades.GroupBy(p => p.Value / 10 * 10);  // 20-29, 30-39
foreach (var grupo in grupos)
{
    Console.WriteLine($"Faixa {grupo.Key}-{grupo.Key + 9}:");
    foreach (var pessoa in grupo)
    {
        Console.WriteLine($"  {pessoa.Key}: {pessoa.Value}");
    }
}
```

### Converter de/para Dictionary

```csharp
// List → Dictionary
List<Pessoa> pessoas = new List<Pessoa>
{
    new Pessoa { Id = 1, Nome = "Ana" },
    new Pessoa { Id = 2, Nome = "Bruno" }
};

Dictionary<int, string> dict = pessoas.ToDictionary(
    p => p.Id,        // Chave
    p => p.Nome       // Valor
);

// Array → Dictionary
var array = new[] 
{
    new { Id = 1, Nome = "Ana" },
    new { Id = 2, Nome = "Bruno" }
};
var dict2 = array.ToDictionary(x => x.Id, x => x.Nome);

// Dictionary → List
List<KeyValuePair<string, int>> lista = idades.ToList();

// Dictionary → Array
KeyValuePair<string, int>[] arrayPares = idades.ToArray();

// Extrair apenas chaves ou valores
List<string> nomes = idades.Keys.ToList();
List<int> valores = idades.Values.ToList();
```

### Filtrar e Recriar Dictionary

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 },
    { "Carlos", 35 },
    { "Diana", 28 }
};

// Filtrar e criar novo Dictionary
Dictionary<string, int> maioresDe30 = idades
    .Where(p => p.Value > 30)
    .ToDictionary(p => p.Key, p => p.Value);

// Transformar valores
Dictionary<string, string> descricoes = idades
    .ToDictionary(
        p => p.Key,
        p => $"{p.Key} tem {p.Value} anos"
    );

// Mesclar dicionários
Dictionary<string, int> dict1 = new() { { "A", 1 }, { "B", 2 } };
Dictionary<string, int> dict2 = new() { { "C", 3 }, { "D", 4 } };

Dictionary<string, int> mesclado = dict1
    .Concat(dict2)
    .ToDictionary(p => p.Key, p => p.Value);
```

---

## Boas Práticas

### 1. Use TryGetValue ao invés de ContainsKey + Indexador

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 }
};

// ❌ Duas buscas no hash table
if (idades.ContainsKey("Ana"))
{
    int idade = idades["Ana"];
    Console.WriteLine(idade);
}

// ✅ Uma busca apenas
if (idades.TryGetValue("Ana", out int idade))
{
    Console.WriteLine(idade);
}
```

### 2. Especifique Capacidade Inicial se Souber o Tamanho

```csharp
// ❌ Redimensionamentos múltiplos
Dictionary<int, string> dict1 = new Dictionary<int, string>();
for (int i = 0; i < 10000; i++)
{
    dict1[i] = $"Item{i}";
}

// ✅ Uma alocação
Dictionary<int, string> dict2 = new Dictionary<int, string>(10000);
for (int i = 0; i < 10000; i++)
{
    dict2[i] = $"Item{i}";
}
```

### 3. Escolha o Tipo Certo de Chave

```csharp
// ✅ Boas chaves: imutáveis, boa distribuição de hash
Dictionary<int, string> porId = new();           // int - ótimo
Dictionary<string, int> porNome = new();         // string - ótimo
Dictionary<Guid, Pessoa> porGuid = new();        // Guid - ótimo
Dictionary<DateTime, List<int>> porData = new(); // DateTime - bom

// ⚠️ Evite chaves mutáveis
public class ChaveMutavel
{
    public int Id { get; set; }
    
    public override int GetHashCode() => Id.GetHashCode();
    public override bool Equals(object? obj) => 
        obj is ChaveMutavel c && c.Id == Id;
}

Dictionary<ChaveMutavel, string> dict = new();
var chave = new ChaveMutavel { Id = 1 };
dict[chave] = "Valor";

chave.Id = 2;  // ❌ Mudou hash! Não encontra mais!
// bool existe = dict.ContainsKey(chave);  // false!

// ✅ Use struct readonly ou record imutável
public readonly record struct ChaveImutavel(int Id);

Dictionary<ChaveImutavel, string> dict2 = new();
var chave2 = new ChaveImutavel(1);
dict2[chave2] = "Valor";  // ✅ Seguro
```

### 4. Use StringComparer Apropriado

```csharp
// ❌ Case-sensitive por padrão
Dictionary<string, int> dict1 = new Dictionary<string, int>();
dict1["Ana"] = 25;
dict1["ana"] = 30;  // Duas entradas diferentes!

// ✅ Case-insensitive quando apropriado
Dictionary<string, int> dict2 = new Dictionary<string, int>(
    StringComparer.OrdinalIgnoreCase);
dict2["Ana"] = 25;
dict2["ana"] = 30;  // Sobrescreve "Ana"

// Outros comparadores:
// - StringComparer.Ordinal (padrão, case-sensitive)
// - StringComparer.OrdinalIgnoreCase (case-insensitive)
// - StringComparer.CurrentCulture (culture-aware)
// - StringComparer.InvariantCulture (invariant)
```

### 5. Não Exponha Dictionary Diretamente

```csharp
public class Cache
{
    // ❌ Expõe para modificação externa
    public Dictionary<string, string> Dados { get; set; } = new();
    
    // ✅ Expõe somente leitura
    private Dictionary<string, string> _dados = new();
    public IReadOnlyDictionary<string, string> Dados => _dados;
    
    // Métodos controlados para modificar
    public void Adicionar(string chave, string valor)
    {
        _dados[chave] = valor;
    }
}
```

### 6. Use Null-Coalescing para Valores Padrão

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 }
};

// ❌ Verboso
int idade;
if (idades.ContainsKey("Bruno"))
    idade = idades["Bruno"];
else
    idade = 0;

// ✅ Com TryGetValue
if (!idades.TryGetValue("Bruno", out int idade2))
    idade2 = 0;

// ✅ Com GetValueOrDefault (mais limpo)
int idade3 = idades.GetValueOrDefault("Bruno", 0);

// ✅ Com ?? (C# 7.0+)
int idade4 = idades.TryGetValue("Bruno", out int temp) ? temp : 0;
```

### 7. Cuidado com ContainsValue

```csharp
Dictionary<string, int> idades = new Dictionary<string, int>
{
    { "Ana", 25 },
    { "Bruno", 30 }
    // ... milhares de entradas
};

// ❌ O(n) - percorre todos valores
bool tem25 = idades.ContainsValue(25);  // Lento!

// ✅ Se precisa buscar por valor frequentemente, use estrutura invertida
Dictionary<int, List<string>> porIdade = new Dictionary<int, List<string>>();
// Ou mantenha dois dicionários
```

---

## Exemplos Práticos

### 1. Cache de Dados

```csharp
public class CacheSimples<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new();
    private readonly TimeSpan _tempoExpiracao;
    
    public CacheSimples(TimeSpan tempoExpiracao)
    {
        _tempoExpiracao = tempoExpiracao;
    }
    
    public void Adicionar(TKey chave, TValue valor)
    {
        _cache[chave] = new CacheItem<TValue>
        {
            Valor = valor,
            DataExpiracao = DateTime.Now.Add(_tempoExpiracao)
        };
    }
    
    public bool TryGet(TKey chave, out TValue? valor)
    {
        if (_cache.TryGetValue(chave, out var item))
        {
            if (DateTime.Now < item.DataExpiracao)
            {
                valor = item.Valor;
                return true;
            }
            else
            {
                // Expirado - remover
                _cache.Remove(chave);
            }
        }
        
        valor = default;
        return false;
    }
    
    public void LimparExpirados()
    {
        var chavesExpiradas = _cache
            .Where(p => DateTime.Now >= p.Value.DataExpiracao)
            .Select(p => p.Key)
            .ToList();
        
        foreach (var chave in chavesExpiradas)
        {
            _cache.Remove(chave);
        }
    }
    
    public int Count => _cache.Count;
}

public class CacheItem<T>
{
    public T Valor { get; set; }
    public DateTime DataExpiracao { get; set; }
}

// Uso
var cache = new CacheSimples<string, string>(TimeSpan.FromMinutes(5));
cache.Adicionar("usuario:1", "Ana Silva");

if (cache.TryGet("usuario:1", out string? nome))
{
    Console.WriteLine($"Cache hit: {nome}");
}
else
{
    Console.WriteLine("Cache miss");
}
```

### 2. Contador de Palavras

```csharp
public class ContadorPalavras
{
    public Dictionary<string, int> Contar(string texto)
    {
        Dictionary<string, int> contagem = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);
        
        // Separar palavras
        var palavras = texto.Split(new[] { ' ', '.', ',', '!', '?', ';', ':' },
            StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string palavra in palavras)
        {
            string palavraLimpa = palavra.Trim().ToLower();
            
            if (!string.IsNullOrWhiteSpace(palavraLimpa))
            {
                // Incrementar contagem
                if (contagem.ContainsKey(palavraLimpa))
                    contagem[palavraLimpa]++;
                else
                    contagem[palavraLimpa] = 1;
                
                // Ou de forma mais simples:
                // contagem[palavraLimpa] = contagem.GetValueOrDefault(palavraLimpa) + 1;
            }
        }
        
        return contagem;
    }
    
    public List<KeyValuePair<string, int>> TopPalavras(Dictionary<string, int> contagem, int top)
    {
        return contagem
            .OrderByDescending(p => p.Value)
            .Take(top)
            .ToList();
    }
}

// Uso
var contador = new ContadorPalavras();
string texto = "o rato roeu a roupa do rei de roma o rato roeu";

var contagem = contador.Contar(texto);
var top3 = contador.TopPalavras(contagem, 3);

foreach (var par in top3)
{
    Console.WriteLine($"{par.Key}: {par.Value}");
}
// roeu: 2
// o: 2
// rato: 2
```

### 3. Gerenciador de Configurações

```csharp
public class ConfiguracaoApp
{
    private readonly Dictionary<string, string> _configuracoes;
    
    public ConfiguracaoApp()
    {
        _configuracoes = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "DbConnection", "Server=localhost;Database=App" },
            { "CacheTimeout", "300" },
            { "MaxRetries", "3" },
            { "LogLevel", "Info" }
        };
    }
    
    public string Get(string chave)
    {
        if (_configuracoes.TryGetValue(chave, out string? valor))
            return valor;
        
        throw new KeyNotFoundException($"Configuração '{chave}' não encontrada");
    }
    
    public T Get<T>(string chave)
    {
        string valor = Get(chave);
        return (T)Convert.ChangeType(valor, typeof(T));
    }
    
    public string GetOrDefault(string chave, string valorPadrao)
    {
        return _configuracoes.GetValueOrDefault(chave, valorPadrao);
    }
    
    public void Set(string chave, string valor)
    {
        _configuracoes[chave] = valor;
    }
    
    public bool Exists(string chave)
    {
        return _configuracoes.ContainsKey(chave);
    }
    
    public IReadOnlyDictionary<string, string> TodasConfiguracoes()
    {
        return _configuracoes;
    }
}

// Uso
var config = new ConfiguracaoApp();

string connection = config.Get("DbConnection");
int timeout = config.Get<int>("CacheTimeout");
string apiKey = config.GetOrDefault("ApiKey", "default-key");

config.Set("ApiUrl", "https://api.exemplo.com");

if (config.Exists("LogLevel"))
{
    string logLevel = config.Get("LogLevel");
    Console.WriteLine($"Log level: {logLevel}");
}
```

### 4. Registro de Estudantes

```csharp
public class Estudante
{
    public int Matricula { get; set; }
    public string Nome { get; set; }
    public string Curso { get; set; }
    public double Media { get; set; }
}

public class RegistroEstudantes
{
    private Dictionary<int, Estudante> _estudantes = new();
    
    public void Adicionar(Estudante estudante)
    {
        if (_estudantes.ContainsKey(estudante.Matricula))
        {
            throw new InvalidOperationException(
                $"Estudante com matrícula {estudante.Matricula} já existe");
        }
        
        _estudantes.Add(estudante.Matricula, estudante);
    }
    
    public Estudante? Buscar(int matricula)
    {
        return _estudantes.GetValueOrDefault(matricula);
    }
    
    public bool Remover(int matricula)
    {
        return _estudantes.Remove(matricula);
    }
    
    public void AtualizarMedia(int matricula, double novaMedia)
    {
        if (_estudantes.TryGetValue(matricula, out Estudante? estudante))
        {
            estudante.Media = novaMedia;
        }
        else
        {
            throw new KeyNotFoundException(
                $"Estudante com matrícula {matricula} não encontrado");
        }
    }
    
    public List<Estudante> BuscarPorCurso(string curso)
    {
        return _estudantes.Values
            .Where(e => e.Curso.Equals(curso, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.Nome)
            .ToList();
    }
    
    public List<Estudante> Top10Melhores()
    {
        return _estudantes.Values
            .OrderByDescending(e => e.Media)
            .Take(10)
            .ToList();
    }
    
    public Dictionary<string, int> EstatisticasPorCurso()
    {
        return _estudantes.Values
            .GroupBy(e => e.Curso)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );
    }
    
    public void GerarRelatorio()
    {
        Console.WriteLine("=== RELATÓRIO DE ESTUDANTES ===\n");
        Console.WriteLine($"Total: {_estudantes.Count}\n");
        
        var estatisticas = EstatisticasPorCurso();
        Console.WriteLine("Por curso:");
        foreach (var stat in estatisticas.OrderByDescending(s => s.Value))
        {
            Console.WriteLine($"  {stat.Key}: {stat.Value} estudantes");
        }
        
        Console.WriteLine("\nTop 5 melhores médias:");
        var top5 = _estudantes.Values
            .OrderByDescending(e => e.Media)
            .Take(5);
        
        foreach (var estudante in top5)
        {
            Console.WriteLine($"  {estudante.Nome} - {estudante.Media:F2}");
        }
    }
}

// Uso
var registro = new RegistroEstudantes();

registro.Adicionar(new Estudante 
{ 
    Matricula = 1001, 
    Nome = "Ana Silva", 
    Curso = "Engenharia",
    Media = 8.5 
});

registro.Adicionar(new Estudante 
{ 
    Matricula = 1002, 
    Nome = "Bruno Santos", 
    Curso = "Medicina",
    Media = 9.2 
});

var estudante = registro.Buscar(1001);
registro.AtualizarMedia(1001, 9.0);

var engenheiros = registro.BuscarPorCurso("Engenharia");
registro.GerarRelatorio();
```

### 5. Tradutor Simples

```csharp
public class Tradutor
{
    private Dictionary<string, Dictionary<string, string>> _traducoes;
    
    public Tradutor()
    {
        _traducoes = new Dictionary<string, Dictionary<string, string>>();
        CarregarTraducoes();
    }
    
    private void CarregarTraducoes()
    {
        // Português → Inglês
        _traducoes["pt-en"] = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "olá", "hello" },
            { "mundo", "world" },
            { "gato", "cat" },
            { "cachorro", "dog" },
            { "casa", "house" }
        };
        
        // Inglês → Português
        _traducoes["en-pt"] = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "hello", "olá" },
            { "world", "mundo" },
            { "cat", "gato" },
            { "dog", "cachorro" },
            { "house", "casa" }
        };
    }
    
    public string Traduzir(string palavra, string idiomaOrigem, string idiomaDestino)
    {
        string chave = $"{idiomaOrigem}-{idiomaDestino}";
        
        if (!_traducoes.TryGetValue(chave, out var dicionario))
        {
            throw new NotSupportedException($"Tradução {chave} não suportada");
        }
        
        if (dicionario.TryGetValue(palavra, out string? traducao))
        {
            return traducao;
        }
        
        return $"[{palavra}]";  // Não traduzido
    }
    
    public string TraduzirFrase(string frase, string idiomaOrigem, string idiomaDestino)
    {
        var palavras = frase.Split(' ');
        var traducoes = palavras
            .Select(p => Traduzir(p, idiomaOrigem, idiomaDestino))
            .ToArray();
        
        return string.Join(' ', traducoes);
    }
    
    public void AdicionarTraducao(string idiomaOrigem, string idiomaDestino, 
        string palavraOrigem, string palavraDestino)
    {
        string chave = $"{idiomaOrigem}-{idiomaDestino}";
        
        if (!_traducoes.ContainsKey(chave))
        {
            _traducoes[chave] = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }
        
        _traducoes[chave][palavraOrigem] = palavraDestino;
    }
    
    public int QuantidadeTraducoes(string idiomaOrigem, string idiomaDestino)
    {
        string chave = $"{idiomaOrigem}-{idiomaDestino}";
        return _traducoes.GetValueOrDefault(chave)?.Count ?? 0;
    }
}

// Uso
var tradutor = new Tradutor();

string traducao1 = tradutor.Traduzir("olá", "pt", "en");  // "hello"
string traducao2 = tradutor.Traduzir("cat", "en", "pt");  // "gato"

string frase = tradutor.TraduzirFrase("olá mundo", "pt", "en");  // "hello world"

tradutor.AdicionarTraducao("pt", "en", "livro", "book");
int total = tradutor.QuantidadeTraducoes("pt", "en");  // 6
```

### 6. Sistema de Inventário

```csharp
public class Produto
{
    public string Codigo { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
}

public class Inventario
{
    private Dictionary<string, Produto> _produtos;
    
    public Inventario()
    {
        _produtos = new Dictionary<string, Produto>(
            StringComparer.OrdinalIgnoreCase);
    }
    
    public void AdicionarProduto(Produto produto)
    {
        if (_produtos.ContainsKey(produto.Codigo))
        {
            throw new InvalidOperationException(
                $"Produto {produto.Codigo} já existe");
        }
        
        _produtos[produto.Codigo] = produto;
    }
    
    public void AtualizarEstoque(string codigo, int quantidade)
    {
        if (_produtos.TryGetValue(codigo, out Produto? produto))
        {
            produto.Quantidade += quantidade;
        }
        else
        {
            throw new KeyNotFoundException($"Produto {codigo} não encontrado");
        }
    }
    
    public bool VenderProduto(string codigo, int quantidade)
    {
        if (!_produtos.TryGetValue(codigo, out Produto? produto))
            return false;
        
        if (produto.Quantidade < quantidade)
            return false;  // Estoque insuficiente
        
        produto.Quantidade -= quantidade;
        return true;
    }
    
    public List<Produto> ProdutosEmFalta(int quantidadeMinima = 0)
    {
        return _produtos.Values
            .Where(p => p.Quantidade <= quantidadeMinima)
            .OrderBy(p => p.Quantidade)
            .ToList();
    }
    
    public decimal ValorTotalInventario()
    {
        return _produtos.Values
            .Sum(p => p.Preco * p.Quantidade);
    }
    
    public Dictionary<string, decimal> ValorPorProduto()
    {
        return _produtos.Values
            .ToDictionary(
                p => p.Nome,
                p => p.Preco * p.Quantidade
            );
    }
    
    public void GerarRelatorioEstoque()
    {
        Console.WriteLine("=== RELATÓRIO DE ESTOQUE ===\n");
        Console.WriteLine($"Total de produtos: {_produtos.Count}");
        Console.WriteLine($"Valor total: {ValorTotalInventario():C}\n");
        
        Console.WriteLine("Produtos em falta (≤ 10):");
        foreach (var produto in ProdutosEmFalta(10))
        {
            Console.WriteLine($"  [{produto.Codigo}] {produto.Nome} - " +
                $"Estoque: {produto.Quantidade}");
        }
    }
}

// Uso
var inventario = new Inventario();

inventario.AdicionarProduto(new Produto 
{ 
    Codigo = "P001", 
    Nome = "Mouse", 
    Preco = 50, 
    Quantidade = 100 
});

inventario.AdicionarProduto(new Produto 
{ 
    Codigo = "P002", 
    Nome = "Teclado", 
    Preco = 150, 
    Quantidade = 5 
});

bool vendido = inventario.VenderProduto("P001", 10);  // true
inventario.AtualizarEstoque("P002", 20);  // Recebeu mais teclados

decimal valorTotal = inventario.ValorTotalInventario();
inventario.GerarRelatorioEstoque();
```

---

## 🎓 Resumo

| Característica | Descrição | Exemplo |
|----------------|-----------|---------|
| **Tipo** | Coleção genérica de pares chave-valor | `Dictionary<string, int>` |
| **Chaves** | Únicas, imutáveis recomendado | `dict["chave"] = valor` |
| **Lookup** | O(1) em média | `dict.TryGetValue(key, out val)` |
| **Ordenação** | Não ordenado | Use `SortedDictionary` se precisar |
| **Thread-safety** | Não thread-safe | Use `ConcurrentDictionary` |
| **Namespace** | `System.Collections.Generic` | `using System.Collections.Generic;` |

### Operações Principais

| Operação | Sintaxe | Complexidade |
|----------|---------|--------------|
| Adicionar | `dict.Add(key, value)` ou `dict[key] = value` | O(1) |
| Acessar | `dict[key]` ou `dict.TryGetValue(key, out val)` | O(1) |
| Remover | `dict.Remove(key)` | O(1) |
| Verificar chave | `dict.ContainsKey(key)` | O(1) |
| Verificar valor | `dict.ContainsValue(value)` | O(n) |
| Limpar | `dict.Clear()` | O(n) |

### Variantes

| Tipo | Ordenado | Thread-Safe | Uso |
|------|----------|-------------|-----|
| **Dictionary\<K,V\>** | ❌ | ❌ | Uso geral |
| **SortedDictionary\<K,V\>** | ✅ | ❌ | Chaves ordenadas |
| **ConcurrentDictionary\<K,V\>** | ❌ | ✅ | Multi-thread |
| **SortedList\<K,V\>** | ✅ | ❌ | Ordenado, menos memória |
| **ImmutableDictionary\<K,V\>** | ❌ | ✅ | Imutável |

---

## ✅ Checklist Rápido

**Criação:**
- ✅ Use `Dictionary<TKey, TValue>` para lookup rápido
- ✅ Especifique capacidade inicial se souber tamanho
- ✅ Use `StringComparer` apropriado para chaves string
- ✅ Prefira chaves imutáveis (int, string, Guid, record)

**Acesso:**
- ✅ Use `TryGetValue` ao invés de `ContainsKey + []`
- ✅ Use `GetValueOrDefault` para valores com padrão
- ✅ Evite `ContainsValue` se performance importa (O(n))

**Modificação:**
- ✅ Use indexador `[]` para adicionar/atualizar
- ✅ Use `Add` quando quer garantir chave nova
- ✅ Use `TryAdd` para adicionar sem exceção
- ✅ Não modifique estrutura durante iteração

**Performance:**
- ✅ Dictionary é O(1) para lookup, add, remove
- ✅ Use `SortedDictionary` apenas se precisar ordem
- ✅ Use `ConcurrentDictionary` em cenários multi-thread
- ✅ Evite chaves com hash ruim (causa colisões)

**Boas práticas:**
- ✅ Exponha como `IReadOnlyDictionary` quando possível
- ✅ Use LINQ para consultas complexas
- ✅ Documente se chaves são case-sensitive ou não
- ✅ Valide chaves antes de usar como chave

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** Todas (Dictionary básico), 7.0+ (TryAdd, Remove com out, deconstruction), 9.0+ (target-typed new)
