# Tipo `object` em C#

## 📋 Índice
1. [O que é `object`?](#o-que-é-object)
2. [Object como Tipo Base](#object-como-tipo-base)
3. [Boxing e Unboxing](#boxing-e-unboxing)
4. [Métodos da Classe Object](#métodos-da-classe-object)
5. [Conversões com object](#conversões-com-object)
6. [Quando Usar object](#quando-usar-object)
7. [Quando NÃO Usar object](#quando-não-usar-object)
8. [object vs Generic](#object-vs-generic)
9. [object vs dynamic](#object-vs-dynamic)
10. [Boas Práticas](#boas-práticas)
11. [Exemplos Práticos](#exemplos-práticos)

---

## O que é `object`?

`object` é o **tipo base de todos os tipos em C#**. É um alias para a classe `System.Object` do .NET.

**Toda classe, struct, enum, delegate e array** em C# deriva implicitamente de `object`.

### Declaração Básica

```csharp
// object pode armazenar QUALQUER tipo
object numero = 10;
object texto = "Hello";
object data = DateTime.Now;
object lista = new List<int>();
object pessoa = new Pessoa();

// System.Object é equivalente a object
System.Object valor = 42;  // Mesmo que: object valor = 42;
```

### Características Principais

- **Tipo de referência**: Sempre armazenado no heap
- **Tipo base universal**: Todos os tipos derivam dele
- **Aceita qualquer valor**: Value types e reference types
- **Sem type safety**: Requer conversões (casts)
- **Performance**: Pode causar boxing/unboxing

---

## Object como Tipo Base

Todo tipo em C# herda de `object`, mesmo que não seja declarado explicitamente.

### Hierarquia de Tipos

```csharp
// Estas declarações são equivalentes
public class Pessoa { }
public class Pessoa : object { }
public class Pessoa : System.Object { }

// Structs também herdam de object (indiretamente via ValueType)
public struct Ponto { }
// Internamente: Ponto → ValueType → Object

// Enums também
public enum Status { }
// Internamente: Status → Enum → ValueType → Object
```

### Conversão Implícita

Qualquer tipo pode ser convertido implicitamente para `object`:

```csharp
// Value types
int numero = 10;
object obj1 = numero;        // ✅ Boxing implícito

// Reference types
string texto = "Hello";
object obj2 = texto;         // ✅ Conversão implícita

// Arrays
int[] array = { 1, 2, 3 };
object obj3 = array;         // ✅ Conversão implícita

// Tipos personalizados
Pessoa pessoa = new Pessoa();
object obj4 = pessoa;        // ✅ Conversão implícita
```

---

## Boxing e Unboxing

**Boxing** e **Unboxing** são processos que ocorrem quando value types são convertidos para/de `object`.

### Boxing (Value Type → object)

**Boxing** é o processo de converter um value type em object (tipo de referência).

```csharp
// Boxing: int → object
int numero = 42;
object obj = numero;  // ✅ Boxing: cria uma cópia no heap

// O que acontece internamente:
// 1. Aloca memória no heap
// 2. Copia o valor 42 para o heap
// 3. Retorna referência para o objeto
```

**Custo de Performance:**
- Alocação de memória no heap
- Cópia do valor
- Pressão no Garbage Collector

### Unboxing (object → Value Type)

**Unboxing** é o processo de converter object de volta para value type.

```csharp
// Unboxing: object → int
object obj = 42;
int numero = (int)obj;  // ✅ Unboxing: cast explícito necessário

// ❌ ERRO - tipo incorreto
object obj2 = 42;
string texto = (string)obj2;  // ERRO em runtime! (InvalidCastException)

// ✅ Verificação antes do unboxing
if (obj is int)
{
    int valor = (int)obj;
}
```

**Custo de Performance:**
- Verificação de tipo em runtime
- Cópia do valor do heap para stack
- Pode lançar exceção se tipo for incompatível

### Impacto de Performance

```csharp
// ❌ Ruim - boxing/unboxing em loop
object[] valores = new object[1000];
for (int i = 0; i < 1000; i++)
{
    valores[i] = i;  // Boxing em cada iteração!
}

int soma = 0;
for (int i = 0; i < valores.Length; i++)
{
    soma += (int)valores[i];  // Unboxing em cada iteração!
}

// ✅ Melhor - sem boxing/unboxing
int[] valores = new int[1000];
for (int i = 0; i < 1000; i++)
{
    valores[i] = i;  // Sem boxing
}

int soma = 0;
for (int i = 0; i < valores.Length; i++)
{
    soma += valores[i];  // Sem unboxing
}
```

### Boxing Implícito em Métodos

```csharp
// Método que aceita object
public void Processar(object valor)
{
    Console.WriteLine(valor);
}

// Chamadas
Processar(42);          // Boxing: int → object
Processar("texto");     // Sem boxing (já é referência)
Processar(true);        // Boxing: bool → object
Processar(3.14);        // Boxing: double → object
```

---

## Métodos da Classe Object

Todo tipo em C# herda os seguintes métodos de `object`:

### 1. ToString()

Retorna uma representação em string do objeto.

```csharp
// Implementação padrão
object obj = new object();
string texto = obj.ToString();  // "System.Object"

// Override em classes personalizadas
public class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
    
    public override string ToString()
    {
        return $"{Nome}, {Idade} anos";
    }
}

var pessoa = new Pessoa { Nome = "João", Idade = 25 };
Console.WriteLine(pessoa.ToString());  // "João, 25 anos"
Console.WriteLine(pessoa);             // Chama ToString() automaticamente
```

### 2. Equals()

Verifica se dois objetos são iguais.

```csharp
// Implementação padrão - compara referências
object obj1 = new object();
object obj2 = obj1;
object obj3 = new object();

bool igual1 = obj1.Equals(obj2);  // true (mesma referência)
bool igual2 = obj1.Equals(obj3);  // false (referências diferentes)

// Override para comparação por valor
public class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Pessoa outra)
            return false;
        
        return Nome == outra.Nome && Idade == outra.Idade;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Nome, Idade);
    }
}

var p1 = new Pessoa { Nome = "João", Idade = 25 };
var p2 = new Pessoa { Nome = "João", Idade = 25 };
bool iguais = p1.Equals(p2);  // true (valores iguais)
```

### 3. GetHashCode()

Retorna um código hash para o objeto (usado em dicionários e sets).

```csharp
// Implementação padrão
object obj = new object();
int hash = obj.GetHashCode();

// Override junto com Equals
public class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Nome, Idade);
    }
}

// Uso em dicionários
var dicionario = new Dictionary<Pessoa, string>();
var pessoa = new Pessoa { Nome = "João", Idade = 25 };
dicionario[pessoa] = "Dados importantes";
```

### 4. GetType()

Retorna o tipo exato do objeto em runtime.

```csharp
object obj1 = 42;
object obj2 = "Hello";
object obj3 = new Pessoa();

Type tipo1 = obj1.GetType();  // System.Int32
Type tipo2 = obj2.GetType();  // System.String
Type tipo3 = obj3.GetType();  // Pessoa

// Verificação de tipo
if (obj1.GetType() == typeof(int))
{
    Console.WriteLine("É um inteiro");
}

// Obtendo informações do tipo
Console.WriteLine(tipo3.Name);        // "Pessoa"
Console.WriteLine(tipo3.FullName);    // "MeuNamespace.Pessoa"
Console.WriteLine(tipo3.Assembly);    // Assembly do tipo
```

### Resumo dos Métodos

| Método | Descrição | Quando Fazer Override |
|--------|-----------|----------------------|
| `ToString()` | Representação string | Sempre recomendado |
| `Equals()` | Comparação de igualdade | Para comparação por valor |
| `GetHashCode()` | Código hash | Quando fizer override de Equals |
| `GetType()` | Tipo do objeto | Nunca (sealed) |

---

## Conversões com object

### Cast Explícito

```csharp
object obj = 42;

// ✅ Cast direto (pode lançar exceção)
int numero = (int)obj;

// ❌ ERRO em runtime se tipo for incompatível
object obj2 = "texto";
int valor = (int)obj2;  // InvalidCastException!
```

### Operador `is`

```csharp
object obj = "Hello";

// Verificação de tipo
if (obj is string)
{
    Console.WriteLine("É uma string");
}

// Pattern matching com cast (C# 7+)
if (obj is string texto)
{
    Console.WriteLine(texto.ToUpper());
}

// Com tipos nullable
object obj2 = 42;
if (obj2 is int numero)
{
    Console.WriteLine(numero * 2);
}
```

### Operador `as`

```csharp
object obj = "Hello";

// Cast seguro (retorna null se falhar)
string? texto = obj as string;
if (texto != null)
{
    Console.WriteLine(texto.ToUpper());
}

// ❌ Não funciona com value types
object obj2 = 42;
int? numero = obj2 as int?;  // ERRO - as não funciona com value types

// ✅ Use is para value types
if (obj2 is int n)
{
    Console.WriteLine(n * 2);
}
```

### Pattern Matching (C# 7+)

```csharp
object obj = OberterDados();

string mensagem = obj switch
{
    int n => $"Número: {n}",
    string s => $"Texto: {s}",
    Pessoa p => $"Pessoa: {p.Nome}",
    null => "Valor nulo",
    _ => "Tipo desconhecido"
};
```

---

## Quando Usar object

### ✅ 1. Coleções Heterogêneas (Antes de Genéricos)

```csharp
// ✅ Válido, mas não recomendado hoje em dia
object[] valores = { 42, "texto", true, 3.14 };

// ❌ Melhor usar genéricos ou tipos específicos
List<object> lista = new List<object> { 42, "texto" };
```

### ✅ 2. APIs que Aceitam Qualquer Tipo

```csharp
// Console.WriteLine aceita object
Console.WriteLine(42);
Console.WriteLine("texto");
Console.WriteLine(new Pessoa());

// Implementação simplificada
public static void WriteLine(object? value)
{
    if (value != null)
        Console.WriteLine(value.ToString());
}
```

### ✅ 3. Serialização/Deserialização

```csharp
// JSON deserialização quando tipo não é conhecido
object dados = JsonSerializer.Deserialize<object>(json);

// Dictionary com valores de tipos diferentes
var config = new Dictionary<string, object>
{
    ["timeout"] = 30,
    ["url"] = "https://api.exemplo.com",
    ["retry"] = true
};
```

### ✅ 4. Reflection e Metadados

```csharp
// Invocar método com reflection
object resultado = metodo.Invoke(instancia, parametros);

// Obter valores de propriedades
object valor = propriedade.GetValue(objeto);
```

### ✅ 5. Interoperabilidade COM

```csharp
// Trabalhar com COM objects
dynamic excelApp = Activator.CreateInstance(Type.GetTypeFromProgID("Excel.Application"));
object workbook = excelApp.Workbooks.Add();
```

---

## Quando NÃO Usar object

### ❌ 1. Quando Genéricos São Possíveis

```csharp
// ❌ Ruim - perde type safety e causa boxing
public class Lista
{
    private object[] itens;
    
    public void Adicionar(object item)
    {
        // ...
    }
    
    public object Obter(int indice)
    {
        return itens[indice];  // Requer cast
    }
}

// ✅ Melhor - type safe e sem boxing
public class Lista<T>
{
    private T[] itens;
    
    public void Adicionar(T item)
    {
        // ...
    }
    
    public T Obter(int indice)
    {
        return itens[indice];  // Sem cast
    }
}
```

### ❌ 2. Para Armazenar Value Types

```csharp
// ❌ Ruim - boxing em cada adição
var valores = new List<object>();
for (int i = 0; i < 1000; i++)
{
    valores.Add(i);  // Boxing!
}

// ✅ Melhor - sem boxing
var valores = new List<int>();
for (int i = 0; i < 1000; i++)
{
    valores.Add(i);  // Sem boxing
}
```

### ❌ 3. Quando o Tipo é Conhecido

```csharp
// ❌ Ruim
public object CalcularTotal(object valor1, object valor2)
{
    return (decimal)valor1 + (decimal)valor2;  // Conversões desnecessárias
}

// ✅ Melhor
public decimal CalcularTotal(decimal valor1, decimal valor2)
{
    return valor1 + valor2;  // Type safe
}
```

### ❌ 4. Em APIs Públicas Modernas

```csharp
// ❌ Evite
public object ProcessarDados(object dados)
{
    // ...
}

// ✅ Prefira genéricos
public T ProcessarDados<T>(T dados)
{
    // ...
}
```

---

## object vs Generic

### Comparação Detalhada

| Aspecto | `object` | Generic `<T>` |
|---------|----------|---------------|
| **Type Safety** | ❌ Não | ✅ Sim |
| **Boxing/Unboxing** | ❌ Sim (value types) | ✅ Não |
| **Performance** | ❌ Menor | ✅ Maior |
| **IntelliSense** | ❌ Limitado | ✅ Completo |
| **Erros** | ⚠️ Runtime | ✅ Compile-time |
| **Reusabilidade** | ⚠️ Limitada | ✅ Alta |

### Exemplos Comparativos

```csharp
// ========== COM OBJECT ==========
public class PilhaObject
{
    private object[] itens = new object[100];
    private int topo = 0;
    
    public void Push(object item)
    {
        itens[topo++] = item;  // Boxing se for value type
    }
    
    public object Pop()
    {
        return itens[--topo];  // Requer cast
    }
}

// Uso
var pilha = new PilhaObject();
pilha.Push(42);              // Boxing
int valor = (int)pilha.Pop(); // Unboxing + cast
pilha.Push("texto");         // Compila, mas pode causar erro depois!
int erro = (int)pilha.Pop(); // ❌ ERRO em runtime!

// ========== COM GENERIC ==========
public class Pilha<T>
{
    private T[] itens = new T[100];
    private int topo = 0;
    
    public void Push(T item)
    {
        itens[topo++] = item;  // Sem boxing
    }
    
    public T Pop()
    {
        return itens[--topo];  // Sem cast
    }
}

// Uso
var pilha = new Pilha<int>();
pilha.Push(42);              // Sem boxing
int valor = pilha.Pop();     // Sem cast
// pilha.Push("texto");      // ❌ ERRO em compile-time!
```

---

## object vs dynamic

`object` e `dynamic` são diferentes, mas relacionados.

### Diferenças Principais

| Aspecto | `object` | `dynamic` |
|---------|----------|-----------|
| **Verificação** | Compile-time | Runtime |
| **Conversões** | Cast explícito | Implícito |
| **Binding** | Early binding | Late binding |
| **IntelliSense** | Sim (após cast) | Não |
| **Performance** | Mais rápido | Mais lento |
| **Erros** | Compile-time (cast) | Runtime |

### Exemplos Comparativos

```csharp
// ========== COM OBJECT ==========
object obj = "Hello";
// obj.ToUpper();           // ❌ ERRO - compilador não sabe o tipo
string texto = (string)obj; // ✅ Cast necessário
texto.ToUpper();            // ✅ OK

int tamanho = ((string)obj).Length;  // Precisa de cast

// ========== COM DYNAMIC ==========
dynamic dyn = "Hello";
dyn.ToUpper();              // ✅ OK - resolvido em runtime
int tamanho = dyn.Length;   // ✅ OK - sem cast

// ⚠️ Mas pode falhar em runtime
dynamic numero = 42;
numero.ToUpper();           // ❌ Compila, mas ERRO em runtime!
```

### Conversões

```csharp
// object → dynamic (sempre possível)
object obj = 42;
dynamic dyn = obj;  // ✅ OK

// dynamic → object (sempre possível)
dynamic dyn2 = "Hello";
object obj2 = dyn2;  // ✅ OK

// Mas comportamento é diferente
object o = 42;
// o + 10;          // ❌ ERRO - operador não definido

dynamic d = 42;
var resultado = d + 10;  // ✅ OK - resolvido em runtime
```

---

## Boas Práticas

### 1. Prefira Genéricos a object

```csharp
// ❌ Evite
public void Processar(object item)
{
    if (item is int numero)
    {
        // Processar número
    }
    else if (item is string texto)
    {
        // Processar texto
    }
}

// ✅ Prefira
public void Processar<T>(T item)
{
    // Processar com type safety
}
```

### 2. Use object Apenas Quando Necessário

```csharp
// ✅ Válido - Console.WriteLine aceita qualquer tipo
Console.WriteLine(qualquerCoisa);

// ✅ Válido - Configurações com valores de tipos diferentes
var config = new Dictionary<string, object>
{
    ["port"] = 8080,
    ["host"] = "localhost",
    ["debug"] = true
};

// ❌ Evite - tipo conhecido
object valor = CalcularTotal();  // Tipo conhecido, use decimal/int/etc
```

### 3. Sempre Verifique o Tipo Antes de Cast

```csharp
// ❌ Perigoso
object obj = ObterValor();
int numero = (int)obj;  // Pode lançar exceção!

// ✅ Seguro
object obj = ObterValor();
if (obj is int numero)
{
    Console.WriteLine(numero * 2);
}

// ✅ Ou com as
string? texto = obj as string;
if (texto != null)
{
    Console.WriteLine(texto.ToUpper());
}
```

### 4. Faça Override de ToString()

```csharp
// ✅ Sempre implemente ToString() em classes personalizadas
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    
    public override string ToString()
    {
        return $"{Nome} - {Preco:C}";
    }
}

// Facilita debug e logging
var produto = new Produto { Id = 1, Nome = "Notebook", Preco = 2500 };
Console.WriteLine(produto);  // "Notebook - R$ 2.500,00"
```

### 5. Override Equals e GetHashCode Juntos

```csharp
// ✅ Sempre faça override dos dois
public class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
    
    public override bool Equals(object? obj)
    {
        return obj is Pessoa outra &&
               Nome == outra.Nome &&
               Idade == outra.Idade;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Nome, Idade);
    }
}
```

### 6. Evite Boxing em Loops

```csharp
// ❌ Ruim - boxing em cada iteração
for (int i = 0; i < 1000; i++)
{
    object obj = i;  // Boxing!
    ProcessarObject(obj);
}

// ✅ Melhor - sem boxing
for (int i = 0; i < 1000; i++)
{
    ProcessarGenerico(i);  // Sem boxing com genéricos
}
```

### 7. Use Pattern Matching Moderno

```csharp
// ✅ C# moderno
object valor = ObterValor();

string mensagem = valor switch
{
    int n when n > 0 => $"Positivo: {n}",
    int n when n < 0 => $"Negativo: {n}",
    int => "Zero",
    string s when !string.IsNullOrEmpty(s) => $"Texto: {s}",
    null => "Nulo",
    _ => "Outro tipo"
};
```

---

## Exemplos Práticos

### 1. Coleção de Tipos Mistos

```csharp
public class CaixaMista
{
    private List<object> itens = new();
    
    public void Adicionar(object item)
    {
        itens.Add(item);
    }
    
    public void Listar()
    {
        foreach (var item in itens)
        {
            string tipo = item.GetType().Name;
            Console.WriteLine($"[{tipo}] {item}");
        }
    }
}

// Uso
var caixa = new CaixaMista();
caixa.Adicionar(42);
caixa.Adicionar("Hello");
caixa.Adicionar(new DateTime(2024, 12, 25));
caixa.Adicionar(true);
caixa.Listar();

// Saída:
// [Int32] 42
// [String] Hello
// [DateTime] 25/12/2024 00:00:00
// [Boolean] True
```

### 2. Sistema de Cache Genérico

```csharp
public class Cache
{
    private Dictionary<string, object> storage = new();
    
    public void Set(string chave, object valor)
    {
        storage[chave] = valor;
    }
    
    public T? Get<T>(string chave)
    {
        if (storage.TryGetValue(chave, out object? valor))
        {
            if (valor is T resultado)
                return resultado;
        }
        return default;
    }
    
    public bool TryGet<T>(string chave, out T? valor)
    {
        if (storage.TryGetValue(chave, out object? obj) && obj is T resultado)
        {
            valor = resultado;
            return true;
        }
        valor = default;
        return false;
    }
}

// Uso
var cache = new Cache();
cache.Set("user", "João");
cache.Set("age", 25);
cache.Set("active", true);

string? nome = cache.Get<string>("user");
int? idade = cache.Get<int>("age");
bool? ativo = cache.Get<bool>("active");

if (cache.TryGet<string>("user", out var usuario))
{
    Console.WriteLine($"Usuário: {usuario}");
}
```

### 3. Formatter Customizado

```csharp
public static class Formatter
{
    public static string Formatar(object valor)
    {
        return valor switch
        {
            null => "Valor nulo",
            int n => $"Número inteiro: {n:N0}",
            decimal d => $"Decimal: {d:C}",
            double db => $"Double: {db:F2}",
            DateTime dt => $"Data: {dt:dd/MM/yyyy HH:mm}",
            bool b => b ? "Sim" : "Não",
            string s => $"Texto: \"{s}\"",
            IEnumerable enumerable => $"Coleção: [{string.Join(", ", enumerable.Cast<object>())}]",
            _ => $"Tipo {valor.GetType().Name}: {valor}"
        };
    }
}

// Uso
Console.WriteLine(Formatter.Formatar(42));
Console.WriteLine(Formatter.Formatar(3.14159));
Console.WriteLine(Formatter.Formatar(DateTime.Now));
Console.WriteLine(Formatter.Formatar(true));
Console.WriteLine(Formatter.Formatar(new[] { 1, 2, 3 }));
```

### 4. Comparador Genérico

```csharp
public class ComparadorUniversal
{
    public static bool SaoIguais(object obj1, object obj2)
    {
        // Ambos null
        if (obj1 == null && obj2 == null)
            return true;
        
        // Apenas um null
        if (obj1 == null || obj2 == null)
            return false;
        
        // Tipos diferentes
        if (obj1.GetType() != obj2.GetType())
            return false;
        
        // Usa Equals
        return obj1.Equals(obj2);
    }
    
    public static int Comparar(object obj1, object obj2)
    {
        if (obj1 == null && obj2 == null) return 0;
        if (obj1 == null) return -1;
        if (obj2 == null) return 1;
        
        // Tenta IComparable
        if (obj1 is IComparable comp)
            return comp.CompareTo(obj2);
        
        throw new ArgumentException("Objetos não são comparáveis");
    }
}

// Uso
bool iguais1 = ComparadorUniversal.SaoIguais(42, 42);        // true
bool iguais2 = ComparadorUniversal.SaoIguais("a", "b");      // false
int comp = ComparadorUniversal.Comparar(10, 20);             // -1
```

### 5. Logger Simples

```csharp
public class Logger
{
    public enum Nivel { Debug, Info, Warning, Error }
    
    public void Log(Nivel nivel, object mensagem)
    {
        string prefixo = nivel switch
        {
            Nivel.Debug => "🐛 DEBUG",
            Nivel.Info => "ℹ️ INFO",
            Nivel.Warning => "⚠️ WARNING",
            Nivel.Error => "❌ ERROR",
            _ => "UNKNOWN"
        };
        
        string texto = mensagem?.ToString() ?? "null";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {prefixo}: {texto}");
    }
    
    public void LogObjeto(Nivel nivel, string titulo, object obj)
    {
        Log(nivel, titulo);
        
        if (obj == null)
        {
            Console.WriteLine("  null");
            return;
        }
        
        Type tipo = obj.GetType();
        Console.WriteLine($"  Tipo: {tipo.Name}");
        
        foreach (var prop in tipo.GetProperties())
        {
            object? valor = prop.GetValue(obj);
            Console.WriteLine($"  {prop.Name}: {valor}");
        }
    }
}

// Uso
var logger = new Logger();
logger.Log(Logger.Nivel.Info, "Aplicação iniciada");
logger.Log(Logger.Nivel.Warning, 42);
logger.Log(Logger.Nivel.Error, new Exception("Erro teste"));

var pessoa = new { Nome = "João", Idade = 25 };
logger.LogObjeto(Logger.Nivel.Debug, "Pessoa detectada", pessoa);
```

### 6. Conversão Segura

```csharp
public static class ConversaoSegura
{
    public static T? ParaTipo<T>(object valor)
    {
        if (valor == null)
            return default;
        
        try
        {
            // Tenta conversão direta
            if (valor is T resultado)
                return resultado;
            
            // Tenta IConvertible
            if (valor is IConvertible)
                return (T)Convert.ChangeType(valor, typeof(T));
            
            return default;
        }
        catch
        {
            return default;
        }
    }
    
    public static bool TentarConverter<T>(object valor, out T? resultado)
    {
        resultado = ParaTipo<T>(valor);
        return resultado != null;
    }
}

// Uso
object obj1 = "123";
int? numero = ConversaoSegura.ParaTipo<int>(obj1);  // 123

object obj2 = 42;
if (ConversaoSegura.TentarConverter<string>(obj2, out string? texto))
{
    Console.WriteLine($"Convertido: {texto}");  // "42"
}
```

### 7. Clonador Genérico (Shallow Copy)

```csharp
public static class Clonador
{
    public static object? Clonar(object original)
    {
        if (original == null)
            return null;
        
        Type tipo = original.GetType();
        
        // Value types e strings (imutáveis)
        if (tipo.IsValueType || tipo == typeof(string))
            return original;
        
        // Usa MemberwiseClone via reflection
        var metodo = tipo.GetMethod("MemberwiseClone",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic);
        
        return metodo?.Invoke(original, null);
    }
}

// Uso
var original = new Pessoa { Nome = "João", Idade = 25 };
var clone = (Pessoa)Clonador.Clonar(original)!;
clone.Nome = "Maria";

Console.WriteLine(original.Nome);  // João
Console.WriteLine(clone.Nome);     // Maria
```

---

## 🎓 Resumo

| Aspecto | Descrição |
|---------|-----------|
| **O que é** | Tipo base de todos os tipos em C# |
| **Alias** | `System.Object` |
| **Herança** | Todo tipo deriva de object |
| **Boxing** | Ocorre com value types → object |
| **Unboxing** | Ocorre com object → value types |
| **Métodos** | ToString, Equals, GetHashCode, GetType |
| **Quando usar** | APIs genéricas, serialização, reflection |
| **Quando evitar** | Quando genéricos são possíveis |

### Regra de Ouro

> **Use `object` apenas quando o tipo realmente não é conhecido em compile-time. Prefira genéricos sempre que possível.**

---

## ✅ Checklist Rápido

**Use `object` quando:**
- ✅ Tipo não é conhecido em compile-time
- ✅ Precisa aceitar qualquer tipo (Console.WriteLine)
- ✅ Trabalhando com reflection
- ✅ Serialização/deserialização genérica
- ✅ Interoperabilidade COM

**Evite `object` quando:**
- ❌ Tipo é conhecido
- ❌ Pode usar genéricos
- ❌ Performance é crítica (boxing/unboxing)
- ❌ Precisa de type safety
- ❌ Em APIs públicas modernas

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** Todas (object), 7.0+ (pattern matching), 8.0+ (nullable reference types)
