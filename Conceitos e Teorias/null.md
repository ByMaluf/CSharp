# Null em C#

## 📋 Índice
1. [O que é `null`?](#o-que-é-null)
2. [Null em Value Types vs Reference Types](#null-em-value-types-vs-reference-types)
3. [Nullable Value Types](#nullable-value-types)
4. [Nullable Reference Types (C# 8.0+)](#nullable-reference-types-c-80)
5. [Operadores Relacionados a Null](#operadores-relacionados-a-null)
   - [Null Coalescing (??)](#null-coalescing-)
   - [Null Conditional (?.)](#null-conditional-)
   - [Null-Forgiving (!)](#null-forgiving-)
6. [Verificações de Null](#verificações-de-null)
7. [Padrões Comuns](#padrões-comuns)
8. [Problemas com Null](#problemas-com-null)
9. [Alternativas a Null](#alternativas-a-null)
10. [Boas Práticas](#boas-práticas)
11. [Exemplos Práticos](#exemplos-práticos)

---

## O que é `null`?

`null` é um **literal** que representa a **ausência de um valor** ou uma **referência nula**. Indica que uma variável não aponta para nenhum objeto na memória.

### Conceito Básico

```csharp
// Reference type - pode ser null
string texto = null;  // ✅ Válido

// Value type - NÃO pode ser null (por padrão)
int numero = null;    // ❌ ERRO de compilação

// Nullable value type - pode ser null
int? numero = null;   // ✅ Válido com ?
```

### Características Principais

- `null` representa **ausência de objeto**
- Valor padrão para **reference types**
- **NullReferenceException** é o erro mais comum em C#
- Value types precisam ser **nullable** (`?`) para aceitar null

---

## Null em Value Types vs Reference Types

### Reference Types (Classes, Strings, Arrays)

Reference types **podem ser null** por padrão:

```csharp
// Todos podem ser null
string texto = null;
int[] array = null;
List<int> lista = null;
Pessoa pessoa = null;

// Verificação
if (texto == null)
{
    Console.WriteLine("Texto é null");
}
```

### Value Types (int, double, bool, struct, enum)

Value types **NÃO podem ser null** por padrão:

```csharp
// ❌ ERRO - value types não aceitam null
int numero = null;        // ERRO!
double valor = null;      // ERRO!
bool flag = null;         // ERRO!
DateTime data = null;     // ERRO!

// ✅ Valores padrão ao invés de null
int numero = default;     // 0
double valor = default;   // 0.0
bool flag = default;      // false
DateTime data = default;  // 01/01/0001 00:00:00
```

### Tabela Comparativa

| Tipo | Pode ser null? | Valor padrão | Exemplo |
|------|---------------|--------------|---------|
| `int`, `double`, `bool` | ❌ Não | 0, 0.0, false | `int x = 0;` |
| `int?`, `double?`, `bool?` | ✅ Sim | null | `int? x = null;` |
| `string`, classes | ✅ Sim | null | `string s = null;` |
| `struct` | ❌ Não | Default values | `Point p = default;` |
| `enum` | ❌ Não | Primeiro valor | `Status s = 0;` |

---

## Nullable Value Types

Nullable types (`T?`) permitem que value types aceitem `null`.

### Sintaxe

```csharp
// Forma curta (preferida)
int? numero = null;
double? valor = null;
bool? flag = null;
DateTime? data = null;

// Forma longa (equivalente)
Nullable<int> numero = null;
Nullable<double> valor = null;
```

### Propriedades de Nullable Types

```csharp
int? numero = 42;

// HasValue - verifica se tem valor
bool temValor = numero.HasValue;  // true

// Value - obtém o valor (lança exceção se null)
int valor = numero.Value;         // 42

// GetValueOrDefault - valor ou padrão
int valorOuPadrao = numero.GetValueOrDefault();     // 42
int comPadrao = numero.GetValueOrDefault(100);      // 42

// Se for null
int? numeroNull = null;
// int x = numeroNull.Value;        // ❌ InvalidOperationException!
int y = numeroNull.GetValueOrDefault();  // 0
int z = numeroNull.GetValueOrDefault(100); // 100
```

### Conversões

```csharp
// Conversão implícita: T → T?
int numero = 42;
int? numeroNullable = numero;  // ✅ OK

// Conversão explícita: T? → T
int? numeroNullable2 = 42;
int numero2 = (int)numeroNullable2;  // ✅ OK se não for null

// ❌ ERRO se for null
int? numeroNull = null;
// int erro = (int)numeroNull;  // InvalidOperationException!

// ✅ Conversão segura
if (numeroNull.HasValue)
{
    int seguro = numeroNull.Value;
}
```

### Comparações

```csharp
int? a = 10;
int? b = 10;
int? c = null;
int? d = null;

// Comparação de valores
bool igual1 = a == b;      // true
bool igual2 = a == c;      // false
bool igual3 = c == d;      // true (ambos null)

// Comparação com null
bool isNull = a == null;   // false
bool isNull2 = c == null;  // true

// Comparação com valor não-nullable
bool igual4 = a == 10;     // true
```

---

## Nullable Reference Types (C# 8.0+)

A partir do C# 8.0, você pode habilitar **nullable reference types** para adicionar mais segurança contra null.

### Habilitando

```csharp
// No arquivo .csproj
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>

// Ou em arquivo específico
#nullable enable
```

### Sintaxe

```csharp
#nullable enable

// Não-nullable (padrão com feature habilitada)
string nome = "João";     // Não pode ser null
// string nome2 = null;   // ⚠️ Warning do compilador

// Nullable (permite null)
string? sobrenome = null; // ✅ Pode ser null
string? email = GetEmail(); // Pode retornar null

// Compilador avisa sobre possível null
void ProcessarNome(string nome)  // Não espera null
{
    int tamanho = nome.Length;  // ✅ Seguro
}

void ProcessarEmail(string? email)  // Pode ser null
{
    // int tamanho = email.Length;  // ⚠️ Warning - pode ser null
    
    // ✅ Verificação antes de usar
    if (email != null)
    {
        int tamanho = email.Length;  // OK
    }
}
```

### Anotações de Nullability

```csharp
#nullable enable

// ! - null-forgiving operator (diz ao compilador que não é null)
string? texto = GetTexto();
int tamanho = texto!.Length;  // "Confie em mim, não é null"

// ?? - null coalescing (fornece valor padrão)
string resultado = texto ?? "padrão";

// ?. - null conditional (acessa apenas se não for null)
int? tamanho2 = texto?.Length;
```

### Modos de Nullable Context

```csharp
#nullable enable    // Habilita warnings para reference types
#nullable disable   // Desabilita (comportamento C# 7.3 e anterior)
#nullable restore   // Restaura configuração do projeto
#nullable warnings  // Apenas warnings
#nullable annotations // Apenas anotações
```

---

## Operadores Relacionados a Null

### Null Coalescing (`??`)

Retorna o valor à esquerda se não for null, caso contrário, retorna o valor à direita.

```csharp
// Sintaxe: valor ?? valorSeNull

string? nome = null;
string resultado = nome ?? "Anônimo";  // "Anônimo"

string? nome2 = "João";
string resultado2 = nome2 ?? "Anônimo";  // "João"

// Encadeamento
string? a = null;
string? b = null;
string? c = "Valor";
string resultado3 = a ?? b ?? c ?? "Padrão";  // "Valor"

// Com métodos
string usuario = GetUsuario() ?? "Guest";
int idade = GetIdade() ?? 0;

// Com throw (C# 7+)
public void ProcessarUsuario(string? nome)
{
    string nomeValido = nome ?? throw new ArgumentNullException(nameof(nome));
}
```

### Null Coalescing Assignment (`??=`) - C# 8.0+

Atribui valor apenas se a variável for null.

```csharp
// Sintaxe: variavel ??= valor

string? nome = null;
nome ??= "Padrão";  // nome agora é "Padrão"

string? nome2 = "João";
nome2 ??= "Padrão";  // nome2 continua "João" (não era null)

// Equivalente a:
if (nome == null)
    nome = "Padrão";

// Uso prático - inicialização lazy
private List<string>? _cache;
public List<string> Cache
{
    get
    {
        _cache ??= new List<string>();  // Cria apenas se null
        return _cache;
    }
}

// Ou mais curto
public List<string> Cache => _cache ??= new List<string>();
```

### Null Conditional (`?.`)

Acessa membro apenas se o objeto não for null.

```csharp
// Sintaxe: objeto?.Membro

string? texto = null;
int? tamanho = texto?.Length;  // null (não lança exceção)

string? texto2 = "Hello";
int? tamanho2 = texto2?.Length;  // 5

// Sem null conditional
int tamanho3;
if (texto != null)
    tamanho3 = texto.Length;
else
    tamanho3 = 0;  // Ou outro valor padrão

// Com null conditional + null coalescing
int tamanho4 = texto?.Length ?? 0;  // 0 se null, Length se não null

// Encadeamento
Pessoa? pessoa = GetPessoa();
string? cidade = pessoa?.Endereco?.Cidade;

// Com métodos
string? resultado = texto?.ToUpper();

// Com indexadores
int? primeiroItem = lista?[0];

// Com delegates
Action? callback = GetCallback();
callback?.Invoke();  // Chama apenas se não for null
```

### Null Conditional Index (`?[]`)

Acessa elemento de array/coleção apenas se não for null.

```csharp
int[]? numeros = null;
int? primeiro = numeros?[0];  // null

int[]? numeros2 = { 1, 2, 3 };
int? primeiro2 = numeros2?[0];  // 1

// Com dicionário
Dictionary<string, int>? dict = GetDictionary();
int? valor = dict?["chave"];
```

### Null-Forgiving Operator (`!`) - C# 8.0+

Diz ao compilador que o valor **não é null**, mesmo que o compilador ache que pode ser.

```csharp
#nullable enable

string? texto = GetTexto();

// ⚠️ Warning - pode ser null
int tamanho = texto.Length;

// ✅ Sem warning - você garante que não é null
int tamanho2 = texto!.Length;

// Uso comum após verificação
if (VerificarSeNaoENull(texto))
{
    // Você sabe que não é null, mas compilador não
    ProcessarTexto(texto!);
}

// ⚠️ Use com cuidado! Pode causar NullReferenceException
string? valorNull = null;
int erro = valorNull!.Length;  // ❌ ERRO em runtime!
```

---

## Verificações de Null

### Comparação Direta

```csharp
string? texto = GetTexto();

// == null
if (texto == null)
{
    Console.WriteLine("É null");
}

// != null
if (texto != null)
{
    Console.WriteLine($"Tamanho: {texto.Length}");
}
```

### Operador `is`

```csharp
string? texto = GetTexto();

// is null
if (texto is null)
{
    Console.WriteLine("É null");
}

// is not null (C# 9+)
if (texto is not null)
{
    Console.WriteLine($"Tamanho: {texto.Length}");
}

// Pattern matching com declaração
if (texto is string valor)
{
    Console.WriteLine(valor.ToUpper());
}
```

### object.ReferenceEquals

```csharp
string? texto = GetTexto();

if (object.ReferenceEquals(texto, null))
{
    Console.WriteLine("É null");
}
```

### Comparação Recomendada

```csharp
// ✅ Preferido para reference types
if (texto == null) { }
if (texto != null) { }

// ✅ Preferido C# 9+
if (texto is null) { }
if (texto is not null) { }

// ✅ Para nullable value types
if (numero.HasValue) { }
if (!numero.HasValue) { }
```

---

## Padrões Comuns

### 1. Guard Clauses

```csharp
public void ProcessarUsuario(string? nome)
{
    // ❌ Versão verbosa
    if (nome != null)
    {
        // Muitas linhas de código...
    }
    
    // ✅ Guard clause - retorna cedo
    if (nome == null)
        return;
    
    // Código principal sem indentação extra
    Console.WriteLine(nome.ToUpper());
}

// Com exceção
public void ProcessarUsuario2(string? nome)
{
    if (nome == null)
        throw new ArgumentNullException(nameof(nome));
    
    Console.WriteLine(nome.ToUpper());
}

// Com throw expression (C# 7+)
public void ProcessarUsuario3(string? nome)
{
    var nomeValido = nome ?? throw new ArgumentNullException(nameof(nome));
    Console.WriteLine(nomeValido.ToUpper());
}
```

### 2. Null Object Pattern

```csharp
public interface ILogger
{
    void Log(string mensagem);
}

public class ConsoleLogger : ILogger
{
    public void Log(string mensagem)
    {
        Console.WriteLine(mensagem);
    }
}

// Null Object - não faz nada, mas não é null
public class NullLogger : ILogger
{
    public void Log(string mensagem)
    {
        // Não faz nada
    }
}

// Uso
public class Sistema
{
    private readonly ILogger _logger;
    
    public Sistema(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();  // Nunca null
    }
    
    public void Executar()
    {
        _logger.Log("Iniciando");  // Sempre seguro, sem verificação
    }
}
```

### 3. Try-Get Pattern

```csharp
public class Cache
{
    private Dictionary<string, string> _data = new();
    
    // ❌ Retornar null pode ser ambíguo
    public string? Get(string chave)
    {
        if (_data.ContainsKey(chave))
            return _data[chave];
        
        return null;  // Não existe ou existe com valor null?
    }
    
    // ✅ Try-Get pattern
    public bool TryGet(string chave, out string? valor)
    {
        return _data.TryGetValue(chave, out valor);
    }
}

// Uso
var cache = new Cache();

if (cache.TryGet("user", out string? usuario))
{
    Console.WriteLine($"Usuário: {usuario}");
}
else
{
    Console.WriteLine("Usuário não encontrado");
}
```

### 4. Lazy Initialization

```csharp
public class Servico
{
    private List<string>? _cache;
    
    // Inicializa apenas quando necessário
    public List<string> Cache
    {
        get
        {
            if (_cache == null)
            {
                _cache = new List<string>();
                CarregarDados(_cache);
            }
            return _cache;
        }
    }
    
    // Ou com ??=
    public List<string> Cache2
    {
        get
        {
            if (_cache == null)
            {
                _cache = new List<string>();
                CarregarDados(_cache);
            }
            return _cache;
        }
    }
    
    // Ou com Lazy<T>
    private readonly Lazy<List<string>> _lazyCache = new(() =>
    {
        var lista = new List<string>();
        // Carregar dados
        return lista;
    });
    
    public List<string> CacheLazy => _lazyCache.Value;
}
```

---

## Problemas com Null

### 1. NullReferenceException

O erro mais comum em C#:

```csharp
string? texto = null;
int tamanho = texto.Length;  // ❌ NullReferenceException!

// Stack trace não ajuda muito
// System.NullReferenceException: Object reference not set to an instance of an object.
```

### 2. Null Propagation

Null pode se propagar pelo código:

```csharp
public class Pessoa
{
    public Endereco? Endereco { get; set; }
}

public class Endereco
{
    public string? Cidade { get; set; }
}

// Problema: null pode vir de vários lugares
Pessoa? pessoa = GetPessoa();  // Pode ser null
string? cidade = pessoa?.Endereco?.Cidade;  // Cada parte pode ser null

// Dificulta debug
if (cidade == null)
{
    // Qual era null? pessoa, Endereco ou Cidade?
}
```

### 3. Ambiguidade

```csharp
// null pode significar coisas diferentes
public int? GetIdade(string nome)
{
    // null = pessoa não encontrada?
    // null = pessoa existe mas idade desconhecida?
    // null = erro ao buscar?
    return null;
}
```

### 4. Verificações Constantes

```csharp
// Código poluído com verificações
public void ProcessarPedido(Pedido? pedido)
{
    if (pedido == null) return;
    if (pedido.Cliente == null) return;
    if (pedido.Cliente.Endereco == null) return;
    if (pedido.Cliente.Endereco.Cidade == null) return;
    
    // Finalmente o código útil
    Console.WriteLine(pedido.Cliente.Endereco.Cidade);
}
```

---

## Alternativas a Null

### 1. Valor Padrão Significativo

```csharp
// ❌ Evite null quando possível
public string? GetNome() => null;

// ✅ Use valor padrão
public string GetNome() => "";
public List<int> GetNumeros() => new();
```

### 2. Option/Maybe Type (Pattern)

```csharp
public class Option<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;
    
    private Option(T? value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }
    
    public static Option<T> Some(T value) => new(value, true);
    public static Option<T> None() => new(default, false);
    
    public bool HasValue => _hasValue;
    
    public T Value => _hasValue 
        ? _value! 
        : throw new InvalidOperationException("No value");
    
    public T GetValueOrDefault(T defaultValue) => 
        _hasValue ? _value! : defaultValue;
    
    public TResult Match<TResult>(
        Func<T, TResult> some,
        Func<TResult> none) =>
        _hasValue ? some(_value!) : none();
}

// Uso
public Option<Pessoa> BuscarPessoa(int id)
{
    var pessoa = _repository.Find(id);
    return pessoa != null 
        ? Option<Pessoa>.Some(pessoa)
        : Option<Pessoa>.None();
}

var resultado = BuscarPessoa(1);
string mensagem = resultado.Match(
    some: p => $"Encontrado: {p.Nome}",
    none: () => "Não encontrado"
);
```

### 3. Result Type (Para Erros)

```csharp
public class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }
    
    private Result(T? value, bool isSuccess, string errorMessage)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static Result<T> Success(T value) => 
        new(value, true, string.Empty);
    
    public static Result<T> Failure(string error) => 
        new(default, false, error);
}

// Uso
public Result<Pessoa> BuscarPessoa(int id)
{
    if (id <= 0)
        return Result<Pessoa>.Failure("ID inválido");
    
    var pessoa = _repository.Find(id);
    
    return pessoa != null
        ? Result<Pessoa>.Success(pessoa)
        : Result<Pessoa>.Failure("Pessoa não encontrada");
}

var resultado = BuscarPessoa(1);
if (resultado.IsSuccess)
{
    Console.WriteLine($"Nome: {resultado.Value!.Nome}");
}
else
{
    Console.WriteLine($"Erro: {resultado.ErrorMessage}");
}
```

### 4. Empty Collections ao invés de null

```csharp
// ❌ Evite
public List<string>? GetNomes()
{
    if (SemNomes())
        return null;
    
    return new List<string> { "João", "Maria" };
}

// Uso requer verificação
var nomes = GetNomes();
if (nomes != null)
{
    foreach (var nome in nomes)
        Console.WriteLine(nome);
}

// ✅ Prefira
public List<string> GetNomes()
{
    if (SemNomes())
        return new List<string>();  // Lista vazia
    
    return new List<string> { "João", "Maria" };
}

// Uso simples
foreach (var nome in GetNomes())  // Funciona mesmo vazio
{
    Console.WriteLine(nome);
}

// Ou use Enumerable.Empty<T>()
public IEnumerable<string> GetNomes()
{
    if (SemNomes())
        return Enumerable.Empty<string>();
    
    return new[] { "João", "Maria" };
}
```

---

## Boas Práticas

### 1. Evite Retornar Null

```csharp
// ❌ Evite
public string? GetNome(int id)
{
    return id > 0 ? "João" : null;
}

// ✅ Prefira
public string GetNome(int id)
{
    return id > 0 ? "João" : string.Empty;
}

// Ou lance exceção
public string GetNomeOuErro(int id)
{
    if (id <= 0)
        throw new ArgumentException("ID inválido", nameof(id));
    
    return "João";
}
```

### 2. Valide Parâmetros

```csharp
// ✅ Valide no início do método
public void ProcessarUsuario(string? nome, int idade)
{
    if (nome == null)
        throw new ArgumentNullException(nameof(nome));
    
    if (string.IsNullOrWhiteSpace(nome))
        throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
    
    if (idade < 0)
        throw new ArgumentException("Idade inválida", nameof(idade));
    
    // Código principal
}

// C# 11+ - checked parameters
public void ProcessarUsuario2(string nome, int idade)
{
    ArgumentNullException.ThrowIfNull(nome);
    ArgumentException.ThrowIfNullOrWhiteSpace(nome);
    
    // Código principal
}
```

### 3. Use Nullable Reference Types

```csharp
#nullable enable

// ✅ Deixe claro o que pode ser null
public string? BuscarNome(int id)  // Pode retornar null
{
    return id > 0 ? "João" : null;
}

public void ProcessarNome(string nome)  // Não aceita null
{
    Console.WriteLine(nome.ToUpper());  // Seguro
}

// Compilador ajuda
var nome = BuscarNome(1);
// ProcessarNome(nome);  // ⚠️ Warning - pode ser null

// ✅ Verificação
if (nome != null)
{
    ProcessarNome(nome);  // OK
}
```

### 4. Prefira Operadores Null-Safe

```csharp
// ❌ Verboso
string resultado;
if (texto != null)
{
    resultado = texto.ToUpper();
}
else
{
    resultado = "PADRÃO";
}

// ✅ Mais limpo
string resultado = texto?.ToUpper() ?? "PADRÃO";

// ❌ Verboso
if (lista != null)
{
    var primeiro = lista[0];
}

// ✅ Mais limpo
var primeiro = lista?[0];
```

### 5. Inicialize Coleções

```csharp
// ❌ Evite
public class Pessoa
{
    public List<string>? Telefones { get; set; }
}

var pessoa = new Pessoa();
// pessoa.Telefones.Add("123");  // ❌ NullReferenceException!

// ✅ Inicialize
public class Pessoa
{
    public List<string> Telefones { get; set; } = new();
}

var pessoa = new Pessoa();
pessoa.Telefones.Add("123");  // ✅ OK
```

### 6. Use string.IsNullOrEmpty/IsNullOrWhiteSpace

```csharp
string? texto = GetTexto();

// ❌ Evite múltiplas verificações
if (texto == null || texto == "" || texto.Trim() == "")
{
    // ...
}

// ✅ Use métodos auxiliares
if (string.IsNullOrEmpty(texto))
{
    // null ou vazio
}

if (string.IsNullOrWhiteSpace(texto))
{
    // null, vazio ou só espaços
}
```

### 7. Documente Comportamento de Null

```csharp
/// <summary>
/// Busca uma pessoa pelo ID.
/// </summary>
/// <param name="id">ID da pessoa</param>
/// <returns>A pessoa encontrada ou null se não existir</returns>
public Pessoa? BuscarPessoa(int id)
{
    return _repository.Find(id);
}

/// <summary>
/// Processa o nome do usuário.
/// </summary>
/// <param name="nome">Nome do usuário (não pode ser null)</param>
/// <exception cref="ArgumentNullException">Se nome for null</exception>
public void ProcessarNome(string nome)
{
    ArgumentNullException.ThrowIfNull(nome);
    // ...
}
```

---

## Exemplos Práticos

### 1. Configuração com Valores Padrão

```csharp
public class Configuracao
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public bool? UseSsl { get; set; }
    
    // Valores com fallback
    public string HostAtual => Host ?? "localhost";
    public int PortAtual => Port ?? 8080;
    public bool UseSslAtual => UseSsl ?? false;
    
    // Ou método
    public string ObterConnectionString()
    {
        var host = Host ?? "localhost";
        var port = Port ?? 8080;
        var ssl = UseSsl ?? false;
        
        return $"{(ssl ? "https" : "http")}://{host}:{port}";
    }
}

// Uso
var config = new Configuracao { Port = 9000 };
Console.WriteLine(config.ObterConnectionString());
// http://localhost:9000
```

### 2. Validação de Entrada

```csharp
public class Usuario
{
    private string _nome = string.Empty;
    private string _email = string.Empty;
    
    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nome não pode ser vazio");
            
            _nome = value.Trim();
        }
    }
    
    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email não pode ser vazio");
            
            if (!value.Contains("@"))
                throw new ArgumentException("Email inválido");
            
            _email = value.ToLower().Trim();
        }
    }
    
    public static Usuario? Criar(string? nome, string? email)
    {
        try
        {
            return new Usuario
            {
                Nome = nome ?? throw new ArgumentNullException(nameof(nome)),
                Email = email ?? throw new ArgumentNullException(nameof(email))
            };
        }
        catch
        {
            return null;
        }
    }
}

// Uso
var usuario = Usuario.Criar("João", "joao@email.com");
if (usuario != null)
{
    Console.WriteLine($"Usuário criado: {usuario.Nome}");
}
```

### 3. Busca com Fallback

```csharp
public class UserService
{
    private readonly Dictionary<int, string> _cache = new();
    private readonly IDatabase _database;
    
    public UserService(IDatabase database)
    {
        _database = database;
    }
    
    public string GetUserName(int id)
    {
        // Tenta cache
        if (_cache.TryGetValue(id, out string? cachedName))
            return cachedName;
        
        // Tenta database
        var dbName = _database.FindName(id);
        if (dbName != null)
        {
            _cache[id] = dbName;
            return dbName;
        }
        
        // Fallback
        return "Usuário Desconhecido";
    }
    
    // Versão com operadores null
    public string GetUserName2(int id)
    {
        return _cache.GetValueOrDefault(id)
            ?? _database.FindName(id)
            ?? "Usuário Desconhecido";
    }
}
```

### 4. Builder com Valores Opcionais

```csharp
public class EmailBuilder
{
    private string? _to;
    private string? _subject;
    private string? _body;
    private string? _from;
    
    public EmailBuilder To(string email)
    {
        _to = email;
        return this;
    }
    
    public EmailBuilder Subject(string subject)
    {
        _subject = subject;
        return this;
    }
    
    public EmailBuilder Body(string body)
    {
        _body = body;
        return this;
    }
    
    public EmailBuilder From(string email)
    {
        _from = email;
        return this;
    }
    
    public Email Build()
    {
        // Validação
        if (string.IsNullOrWhiteSpace(_to))
            throw new InvalidOperationException("Destinatário é obrigatório");
        
        return new Email
        {
            To = _to,
            Subject = _subject ?? "(Sem assunto)",
            Body = _body ?? string.Empty,
            From = _from ?? "noreply@sistema.com"
        };
    }
}

// Uso
var email = new EmailBuilder()
    .To("cliente@email.com")
    .Subject("Bem-vindo")
    .Build();  // From e Body usam valores padrão
```

### 5. Processamento Seguro de Coleções

```csharp
public class RelatorioService
{
    public string GerarRelatorio(List<Pedido>? pedidos)
    {
        // Trata null como lista vazia
        pedidos ??= new List<Pedido>();
        
        if (pedidos.Count == 0)
            return "Nenhum pedido encontrado";
        
        var total = pedidos.Sum(p => p.Valor);
        var media = pedidos.Average(p => p.Valor);
        
        return $"Total de pedidos: {pedidos.Count}\n" +
               $"Valor total: {total:C}\n" +
               $"Valor médio: {media:C}";
    }
    
    // Versão alternativa
    public string GerarRelatorio2(IEnumerable<Pedido>? pedidos)
    {
        var lista = (pedidos ?? Enumerable.Empty<Pedido>()).ToList();
        
        if (!lista.Any())
            return "Nenhum pedido encontrado";
        
        // Processar...
        return $"Processado {lista.Count} pedidos";
    }
}
```

### 6. Lazy Loading com Null Check

```csharp
public class PerfilUsuario
{
    private List<string>? _permissoes;
    private Configuracao? _config;
    
    public int UsuarioId { get; }
    
    public PerfilUsuario(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
    
    public List<string> Permissoes
    {
        get
        {
            // Lazy loading
            if (_permissoes == null)
            {
                _permissoes = CarregarPermissoes();
            }
            return _permissoes;
        }
    }
    
    // Usando ??=
    public Configuracao Config
    {
        get
        {
            _config ??= CarregarConfiguracao();
            return _config;
        }
    }
    
    private List<string> CarregarPermissoes()
    {
        // Simula carregamento do banco
        return new List<string> { "ler", "escrever" };
    }
    
    private Configuracao CarregarConfiguracao()
    {
        return new Configuracao();
    }
}
```

### 7. Extension Methods para Null Safety

```csharp
public static class StringExtensions
{
    public static string OrDefault(this string? value, string defaultValue = "")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
    
    public static string Truncate(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        
        return value.Length <= maxLength 
            ? value 
            : value.Substring(0, maxLength) + "...";
    }
    
    public static bool HasValue(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}

public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }
    
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }
}

// Uso
string? nome = GetNome();
Console.WriteLine(nome.OrDefault("Anônimo"));
Console.WriteLine(nome.Truncate(10));

if (nome.HasValue())
{
    Console.WriteLine($"Nome válido: {nome}");
}

List<int>? numeros = GetNumeros();
foreach (var n in numeros.OrEmpty())
{
    Console.WriteLine(n);
}
```

---

## 🎓 Resumo

| Conceito | Descrição | Exemplo |
|----------|-----------|---------|
| **`null`** | Ausência de valor | `string? s = null;` |
| **Nullable Value Types** | Value type que aceita null | `int? numero = null;` |
| **Nullable Reference Types** | Warning para null (C# 8+) | `string? texto = null;` |
| **`??`** | Null coalescing | `valor ?? padrão` |
| **`??=`** | Null coalescing assignment | `x ??= padrão` |
| **`?.`** | Null conditional | `obj?.Propriedade` |
| **`!`** | Null-forgiving | `valor!.ToString()` |
| **`is null`** | Verificação de null | `if (x is null)` |

### Operadores Null

| Operador | Nome | Uso | Resultado |
|----------|------|-----|-----------|
| `??` | Null coalescing | `a ?? b` | `a` se não null, senão `b` |
| `??=` | Null coalescing assignment | `a ??= b` | Atribui `b` se `a` for null |
| `?.` | Null conditional | `obj?.Prop` | Acessa se não null, senão null |
| `?[]` | Null conditional index | `arr?[0]` | Acessa se não null, senão null |
| `!` | Null-forgiving | `obj!.Prop` | Suprime warning (use com cuidado!) |

### Regra de Ouro

> **Evite null sempre que possível. Quando inevitável, seja explícito sobre nullability e use operadores null-safe.**

---

## ✅ Checklist Rápido

**Evite null:**
- ✅ Use valores padrão significativos
- ✅ Retorne coleções vazias ao invés de null
- ✅ Use Option/Result types
- ✅ Inicialize propriedades

**Ao usar null:**
- ✅ Habilite nullable reference types (C# 8+)
- ✅ Marque claramente com `?`
- ✅ Use operadores null-safe (`??`, `?.`)
- ✅ Valide parâmetros no início
- ✅ Documente comportamento

**Evite:**
- ❌ Retornar null sem documentar
- ❌ Aceitar null sem validar
- ❌ Uso excessivo de `!` (null-forgiving)
- ❌ Cadeias longas de `?.?.?.`
- ❌ Verificações null repetidas

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** Todas (null básico), 7.0+ (throw expressions), 8.0+ (nullable reference types, `??=`), 9.0+ (`is not null`)
