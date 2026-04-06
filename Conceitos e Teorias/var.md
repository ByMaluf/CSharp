# Palavra Reservada `var` em C#

## 📋 Índice
1. [O que é `var`?](#o-que-é-var)
2. [Inferência de Tipo](#inferência-de-tipo)
3. [Quando Usar `var`](#quando-usar-var)
4. [Quando NÃO Usar `var`](#quando-não-usar-var)
5. [Regras e Restrições](#regras-e-restrições)
6. [var vs Tipo Explícito](#var-vs-tipo-explícito)
7. [var vs dynamic](#var-vs-dynamic)
8. [var com Tipos Anônimos](#var-com-tipos-anônimos)
9. [var em LINQ](#var-em-linq)
10. [Boas Práticas](#boas-práticas)
11. [Exemplos Práticos](#exemplos-práticos)

---

## O que é `var`?

`var` é uma palavra-chave em C# que permite ao **compilador inferir automaticamente o tipo de uma variável** com base no valor atribuído a ela.

**Importante:** `var` **NÃO** é um tipo dinâmico! A variável ainda é **fortemente tipada** e o tipo é definido em **tempo de compilação**.

### Exemplo Básico

```csharp
// Com tipo explícito
string nome = "João";
int idade = 25;
List<string> nomes = new List<string>();

// Com var (mesmo resultado)
var nome = "João";          // Compilador infere: string
var idade = 25;             // Compilador infere: int
var nomes = new List<string>();  // Compilador infere: List<string>
```

O compilador determina o tipo com base na **expressão do lado direito** da atribuição.

---

## Inferência de Tipo

O compilador analisa a expressão de inicialização e determina o tipo mais apropriado.

### Como Funciona

```csharp
var numero = 10;           // int
var texto = "Olá";         // string
var decimal = 10.5;        // double
var decimal2 = 10.5f;      // float
var decimal3 = 10.5m;      // decimal
var verdadeiro = true;     // bool
var letra = 'A';           // char
var array = new int[5];    // int[]
var lista = new List<string>();  // List<string>
```

### Verificando o Tipo

```csharp
var valor = 100;
Console.WriteLine(valor.GetType());  // System.Int32

var texto = "Hello";
Console.WriteLine(texto.GetType());  // System.String

var lista = new List<int>();
Console.WriteLine(lista.GetType());  // System.Collections.Generic.List`1[System.Int32]
```

---

## Quando Usar `var`

### ✅ 1. Quando o Tipo é Óbvio

```csharp
// ✅ Bom - tipo óbvio pela inicialização
var cliente = new Cliente();
var pedidos = new List<Pedido>();
var conexao = new SqlConnection(connectionString);
```

### ✅ 2. Com Tipos Genéricos Longos

```csharp
// ❌ Verboso
Dictionary<string, List<Tuple<int, string, DateTime>>> dados = 
    new Dictionary<string, List<Tuple<int, string, DateTime>>>();

// ✅ Mais limpo
var dados = new Dictionary<string, List<Tuple<int, string, DateTime>>>();
```

### ✅ 3. Com LINQ

```csharp
// ✅ Recomendado - tipo do resultado pode ser complexo
var resultado = from p in produtos
                where p.Preco > 100
                select new { p.Nome, p.Preco };

var produtosCaros = produtos.Where(p => p.Preco > 100).ToList();
```

### ✅ 4. Com Tipos Anônimos (Obrigatório)

```csharp
// ✅ Única forma - tipo anônimo não tem nome
var pessoa = new { Nome = "João", Idade = 25 };
```

### ✅ 5. Em Loops `foreach`

```csharp
var nomes = new List<string> { "Ana", "Bruno", "Carlos" };

// ✅ Bom - tipo é óbvio
foreach (var nome in nomes)
{
    Console.WriteLine(nome);
}
```

### ✅ 6. Com Target-Typed `new` (C# 9+)

```csharp
// ✅ Evita repetição
List<string> nomes = new();  // C# 9+
var nomes = new List<string>();  // Ambos válidos
```

---

## Quando NÃO Usar `var`

### ❌ 1. Quando o Tipo NÃO é Óbvio

```csharp
// ❌ Ruim - tipo não é claro
var resultado = ProcessarDados();

// ✅ Bom - tipo explícito ajuda a entender
Cliente resultado = ProcessarDados();
```

### ❌ 2. Com Literais Numéricos Simples

```csharp
// ❌ Ambíguo - pode ser int, long, decimal?
var quantidade = 10;

// ✅ Melhor - intenção clara
int quantidade = 10;
decimal preco = 10.99m;
long populacao = 1000000L;
```

### ❌ 3. Quando o Nome da Variável Não Descreve o Tipo

```csharp
// ❌ Ruim - tipo não é claro
var dados = ObterDados();
var temp = Calcular();

// ✅ Melhor
List<Produto> produtos = ObterDados();
decimal totalComDesconto = Calcular();
```

### ❌ 4. Com Valores `null`

```csharp
// ❌ ERRO - compilador não pode inferir tipo de null
var valor = null;  // ERRO!

// ✅ Correto
string? texto = null;
int? numero = null;
```

### ❌ 5. Em Parâmetros de Método

```csharp
// ❌ ERRO - var não permitido em parâmetros
public void ProcessarDados(var dados)  // ERRO!

// ✅ Correto
public void ProcessarDados(List<string> dados)
```

### ❌ 6. Em Campos de Classe

```csharp
public class MinhaClasse
{
    // ❌ ERRO - var não permitido em campos
    private var nome;  // ERRO!
    
    // ✅ Correto
    private string nome;
}
```

---

## Regras e Restrições

### Restrições Importantes

1. **Deve ser inicializada na declaração**
```csharp
// ❌ ERRO - sem inicialização
var x;  // ERRO!

// ✅ Correto
var x = 10;
```

2. **Não pode ser null sem contexto**
```csharp
// ❌ ERRO
var x = null;  // ERRO!

// ✅ Correto - com contexto
var x = (string?)null;
var y = default(int?);
```

3. **Apenas para variáveis locais**
```csharp
public class Exemplo
{
    // ❌ ERRO - não permitido em campos
    private var campo = 10;  // ERRO!
    
    // ✅ Correto - apenas em variáveis locais
    public void Metodo()
    {
        var local = 10;  // OK
    }
}
```

4. **Não pode mudar de tipo**
```csharp
var numero = 10;  // int
numero = "texto";  // ❌ ERRO - tipo já foi definido como int
```

5. **Um tipo por vez**
```csharp
// ❌ ERRO - múltiplas declarações com var
var a = 1, b = "texto";  // ERRO!

// ✅ Correto
var a = 1;
var b = "texto";
```

---

## var vs Tipo Explícito

### Comparação

| Aspecto | `var` | Tipo Explícito |
|---------|-------|----------------|
| **Compilação** | Tipo inferido em tempo de compilação | Tipo especificado explicitamente |
| **Performance** | Idêntica | Idêntica |
| **Legibilidade** | Depende do contexto | Sempre clara |
| **Flexibilidade** | Menos código | Mais controle |
| **Type Safety** | Sim (fortemente tipado) | Sim |

### Exemplos Comparativos

```csharp
// Exemplo 1: Tipo Óbvio
var cliente1 = new Cliente();        // ✅ Claro
Cliente cliente2 = new Cliente();    // ✅ Também claro

// Exemplo 2: Tipo Complexo
var dicionario1 = new Dictionary<int, List<string>>();  // ✅ Mais limpo
Dictionary<int, List<string>> dicionario2 = new Dictionary<int, List<string>>();  // ❌ Verboso

// Exemplo 3: Tipo Não Óbvio
var resultado1 = ProcessarDados();   // ❌ Tipo desconhecido
Cliente resultado2 = ProcessarDados();  // ✅ Tipo claro

// Exemplo 4: Valores Primitivos
var numero1 = 10;        // ⚠️ Aceitável, mas não ideal
int numero2 = 10;        // ✅ Intenção mais clara

var preco1 = 10.99;      // ⚠️ É double? decimal?
decimal preco2 = 10.99m; // ✅ Tipo específico
```

---

## var vs dynamic

`var` e `dynamic` são **completamente diferentes**!

### Diferenças Principais

| Aspecto | `var` | `dynamic` |
|---------|-------|-----------|
| **Tipagem** | Estática (compile-time) | Dinâmica (runtime) |
| **Verificação** | Em tempo de compilação | Em tempo de execução |
| **Performance** | Rápida | Mais lenta |
| **IntelliSense** | Sim | Não |
| **Erros** | Em compilação | Em execução |
| **Mudança de tipo** | Não | Sim |

### Exemplos Comparativos

```csharp
// var - tipo definido em COMPILAÇÃO
var numero = 10;         // Tipo: int
numero = 20;             // ✅ OK - ainda int
numero = "texto";        // ❌ ERRO de compilação
numero.ToUpper();        // ❌ ERRO de compilação (int não tem ToUpper)

// dynamic - tipo verificado em EXECUÇÃO
dynamic valor = 10;      // Tipo: int (neste momento)
valor = 20;              // ✅ OK
valor = "texto";         // ✅ OK - pode mudar de tipo!
valor.ToUpper();         // ✅ Compila, mas pode falhar em RUNTIME
```

### Quando Usar Cada Um

```csharp
// var - uso normal, código C# típico
var lista = new List<string>();
var cliente = new Cliente();

// dynamic - interoperabilidade COM, reflexão, tipos dinâmicos
dynamic jsonResult = JsonConvert.DeserializeObject(json);
dynamic excelApp = new Microsoft.Office.Interop.Excel.Application();
```

---

## var com Tipos Anônimos

Tipos anônimos **EXIGEM** o uso de `var`, pois não têm um nome de tipo declarado.

### Criação de Tipos Anônimos

```csharp
// ✅ Tipo anônimo - var obrigatório
var pessoa = new { Nome = "João", Idade = 25 };
Console.WriteLine(pessoa.Nome);   // João
Console.WriteLine(pessoa.Idade);  // 25

// var produto = new { Id = 1, Nome = "Notebook", Preco = 2500.00 };
var produto = new 
{ 
    Id = 1, 
    Nome = "Notebook", 
    Preco = 2500.00,
    Estoque = 10
};

// ❌ ERRO - tipo anônimo não tem nome
MinhaClasse pessoa = new { Nome = "João", Idade = 25 };  // ERRO!
```

### Uso Comum: Projeções

```csharp
var produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Notebook", Preco = 2500 },
    new Produto { Id = 2, Nome = "Mouse", Preco = 50 },
    new Produto { Id = 3, Nome = "Teclado", Preco = 150 }
};

// Projeção para tipo anônimo
var resumo = produtos.Select(p => new 
{ 
    p.Nome, 
    p.Preco,
    PrecoComDesconto = p.Preco * 0.9m
});

foreach (var item in resumo)
{
    Console.WriteLine($"{item.Nome}: {item.PrecoComDesconto:C}");
}
```

### Arrays de Tipos Anônimos

```csharp
var pessoas = new[]
{
    new { Nome = "Ana", Idade = 25 },
    new { Nome = "Bruno", Idade = 30 },
    new { Nome = "Carlos", Idade = 35 }
};

// Todos os objetos devem ter a mesma estrutura
foreach (var pessoa in pessoas)
{
    Console.WriteLine($"{pessoa.Nome} tem {pessoa.Idade} anos");
}
```

---

## var em LINQ

`var` é amplamente utilizado em LINQ devido à complexidade dos tipos retornados.

### Query Syntax

```csharp
var numeros = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// var recomendado - tipo complexo
var pares = from n in numeros
            where n % 2 == 0
            select n;

foreach (var numero in pares)
{
    Console.WriteLine(numero);
}
```

### Method Syntax

```csharp
var produtos = ObterProdutos();

// Filtrar e ordenar
var produtosCaros = produtos
    .Where(p => p.Preco > 100)
    .OrderBy(p => p.Nome)
    .ToList();

// Agrupamento
var porCategoria = produtos
    .GroupBy(p => p.Categoria)
    .Select(g => new 
    { 
        Categoria = g.Key, 
        Quantidade = g.Count(),
        Total = g.Sum(p => p.Preco)
    });
```

### Projeções Complexas

```csharp
var relatorio = from p in produtos
                join c in categorias on p.CategoriaId equals c.Id
                where p.Preco > 50
                select new
                {
                    Produto = p.Nome,
                    Categoria = c.Nome,
                    p.Preco,
                    Desconto = p.Preco * 0.1m,
                    PrecoFinal = p.Preco * 0.9m
                };

foreach (var item in relatorio)
{
    Console.WriteLine($"{item.Produto} ({item.Categoria}): {item.PrecoFinal:C}");
}
```

---

## Boas Práticas

### 1. Use `var` Quando o Tipo é Óbvio

```csharp
// ✅ Bom
var cliente = new Cliente();
var lista = new List<string>();
var sb = new StringBuilder();

// ❌ Desnecessário
Cliente cliente = new Cliente();
List<string> lista = new List<string>();
```

### 2. Evite `var` Quando o Tipo NÃO é Claro

```csharp
// ❌ Ruim
var dados = ObterDados();
var resultado = Processar();

// ✅ Melhor
List<Cliente> clientes = ObterDados();
decimal totalVendas = Processar();
```

### 3. Prefira Tipo Explícito para Tipos Primitivos

```csharp
// ⚠️ Aceitável, mas não ideal
var contador = 0;
var ativo = true;
var taxa = 0.05;

// ✅ Mais claro
int contador = 0;
bool ativo = true;
decimal taxa = 0.05m;  // Especifica decimal vs double
```

### 4. Use `var` com Target-Typed `new`

```csharp
// C# 9+
List<string> nomes = new();  // Target-typed new
var nomes = new List<string>();  // Ambos válidos e claros
```

### 5. Seja Consistente no Projeto

```csharp
// Escolha um estilo e mantenha no projeto inteiro

// Estilo 1: var sempre que possível
var cliente = new Cliente();
var lista = new List<int>();

// Estilo 2: tipo explícito sempre
Cliente cliente = new Cliente();
List<int> lista = new List<int>();

// Estilo 3: híbrido (baseado em contexto)
var cliente = new Cliente();  // Óbvio
List<int> numeros = ObterNumeros();  // Não óbvio
```

### 6. Use Nomes Descritivos

```csharp
// ❌ Ruim - nome genérico + var
var dados = ObterDados();
var temp = Calcular();

// ✅ Bom - nome descritivo compensa var
var clientesAtivos = ObterClientesAtivos();
var totalComDesconto = CalcularTotalComDesconto();
```

### 7. IntelliSense é Seu Amigo

```csharp
// Passe o mouse sobre 'var' no Visual Studio
// para ver o tipo inferido

var resultado = ProcessarDados();  // Mostra: Cliente resultado
```

---

## Exemplos Práticos

### 1. Inicialização de Coleções

```csharp
// ✅ Bom - tipo óbvio
var nomes = new List<string> { "Ana", "Bruno", "Carlos" };
var idades = new Dictionary<string, int>
{
    ["Ana"] = 25,
    ["Bruno"] = 30,
    ["Carlos"] = 35
};
var numeros = new int[] { 1, 2, 3, 4, 5 };
```

### 2. Loop `foreach`

```csharp
var produtos = ObterProdutos();

foreach (var produto in produtos)
{
    Console.WriteLine($"{produto.Nome}: {produto.Preco:C}");
}

// Com dicionário
var dicionario = new Dictionary<string, int>();
foreach (var item in dicionario)
{
    Console.WriteLine($"{item.Key} = {item.Value}");
}

// Desconstrução (C# 7+)
foreach (var (chave, valor) in dicionario)
{
    Console.WriteLine($"{chave} = {valor}");
}
```

### 3. LINQ com Projeções

```csharp
var clientes = ObterClientes();

var relatorio = clientes
    .Where(c => c.Ativo)
    .Select(c => new
    {
        c.Nome,
        c.Email,
        TotalCompras = c.Pedidos.Count,
        ValorTotal = c.Pedidos.Sum(p => p.Total)
    })
    .OrderByDescending(c => c.ValorTotal)
    .Take(10);

foreach (var item in relatorio)
{
    Console.WriteLine($"{item.Nome}: {item.ValorTotal:C} ({item.TotalCompras} pedidos)");
}
```

### 4. Trabalhando com JSON (Tipos Dinâmicos)

```csharp
var json = @"{
    ""nome"": ""João"",
    ""idade"": 25,
    ""cidade"": ""São Paulo""
}";

// Desserializar para tipo anônimo
var pessoa = JsonSerializer.Deserialize<dynamic>(json);

// Ou criar tipo anônimo manualmente
var cliente = new 
{ 
    Nome = "Maria", 
    Idade = 30, 
    Email = "maria@email.com" 
};

var jsonSaida = JsonSerializer.Serialize(cliente);
```

### 5. Builder Pattern

```csharp
var cliente = new ClienteBuilder()
    .ComNome("João Silva")
    .ComEmail("joao@email.com")
    .ComTelefone("11999999999")
    .ComEndereco("Rua A, 123")
    .Construir();

// var esconde a complexidade do tipo retornado
var query = context.Clientes
    .Include(c => c.Pedidos)
    .ThenInclude(p => p.Itens)
    .Where(c => c.Ativo);
```

### 6. Pattern Matching (C# 7+)

```csharp
object obj = "Hello";

if (obj is string texto)  // 'texto' é string
{
    Console.WriteLine(texto.ToUpper());
}

// Com var pattern
if (obj is var valor && valor != null)
{
    Console.WriteLine(valor.GetType());
}
```

### 7. Tuplas (C# 7+)

```csharp
var (nome, idade) = ObterDados();

// Método que retorna tupla
(string, int) ObterDados()
{
    return ("João", 25);
}

// Tupla nomeada
var pessoa = (Nome: "Maria", Idade: 30);
Console.WriteLine(pessoa.Nome);
```

### 8. Processamento Assíncrono

```csharp
var tarefas = new List<Task<string>>
{
    ProcessarAsync("A"),
    ProcessarAsync("B"),
    ProcessarAsync("C")
};

var resultados = await Task.WhenAll(tarefas);

foreach (var resultado in resultados)
{
    Console.WriteLine(resultado);
}
```

### 9. Repository Pattern

```csharp
public class ClienteService
{
    private readonly IRepository<Cliente> _repository;
    
    public ClienteService(IRepository<Cliente> repository)
    {
        _repository = repository;
    }
    
    public async Task<List<Cliente>> ObterAtivosAsync()
    {
        // var esconde a complexidade do tipo de retorno do LINQ
        var query = _repository.Query()
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome);
        
        var clientes = await query.ToListAsync();
        return clientes;
    }
}
```

### 10. Factory Pattern

```csharp
public class ConexaoFactory
{
    public static var CriarConexao(string tipo)  // ❌ ERRO - var não permitido
    {
        // ...
    }
    
    public static IDbConnection CriarConexao(string tipo)  // ✅ Correto
    {
        var conexao = tipo switch  // ✅ var OK aqui (variável local)
        {
            "sql" => new SqlConnection(),
            "mysql" => new MySqlConnection(),
            "postgres" => new NpgsqlConnection(),
            _ => throw new ArgumentException("Tipo inválido")
        };
        
        return conexao;
    }
}
```

---

## 🎓 Resumo

| Aspecto | Descrição |
|---------|-----------|
| **O que é** | Inferência de tipo pelo compilador |
| **Tipagem** | Estática (compile-time) |
| **Performance** | Idêntica ao tipo explícito |
| **Obrigatório** | Tipos anônimos |
| **Recomendado** | LINQ, tipos genéricos longos |
| **Evitar** | Quando tipo não é óbvio |
| **Permitido** | Apenas variáveis locais |
| **Não permitido** | Campos, parâmetros, retornos |

### Regra de Ouro

> **Use `var` quando tornar o código MAIS LEGÍVEL, não apenas mais curto.**

---

## ✅ Checklist Rápido

**Use `var` quando:**
- ✅ O tipo é óbvio pela inicialização
- ✅ Trabalhando com LINQ
- ✅ Tipo genérico muito longo
- ✅ Tipo anônimo (obrigatório)
- ✅ Em loops `foreach`

**Evite `var` quando:**
- ❌ O tipo não é claro
- ❌ Com valores `null`
- ❌ Literais numéricos simples
- ❌ Nome da variável é genérico
- ❌ Em campos de classe ou parâmetros

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** 3.0+ (var introduzido), 7.0+ (tuplas, pattern matching), 9.0+ (target-typed new)
