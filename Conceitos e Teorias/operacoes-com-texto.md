# Operações com Texto em C#

Em C#, textos são representados pelo tipo `string`, que é uma sequência imutável de caracteres Unicode. Este guia apresenta as principais operações e métodos para manipulação de textos.

## Características Fundamentais

### Imutabilidade

Strings em C# são **imutáveis**: uma vez criadas, não podem ser modificadas. Operações que parecem alterar uma string na verdade criam uma nova string.

```csharp
string texto = "Hello";
texto.ToUpper(); // Retorna "HELLO", mas 'texto' continua "Hello"

string textoMaiusculo = texto.ToUpper(); // Correto: armazena o resultado
Console.WriteLine(textoMaiusculo); // Saída: HELLO
```

---

## Criação e Inicialização

```csharp
// Literal de string
string nome = "João";

// String vazia
string vazio1 = "";
string vazio2 = string.Empty; // Recomendado

// String com quebra de linha
string multilinhas = @"Linha 1
Linha 2
Linha 3";

// String interpolada
int idade = 25;
string mensagem = $"Meu nome é {nome} e tenho {idade} anos";

// Construtor
char[] letras = { 'O', 'l', 'á' };
string saudacao = new string(letras);
```

---

## Concatenação de Strings

### Operador +

```csharp
string nome = "João";
string sobrenome = "Silva";
string nomeCompleto = nome + " " + sobrenome; // "João Silva"
```

### Método Concat

```csharp
string resultado = string.Concat("Hello", " ", "World"); // "Hello World"
```

### Interpolação (Recomendado)

```csharp
string nome = "Maria";
int idade = 30;
string mensagem = $"Olá, {nome}! Você tem {idade} anos.";
```

### StringBuilder (Para Muitas Operações)

```csharp
using System.Text;

StringBuilder sb = new StringBuilder();
sb.Append("Primeira ");
sb.Append("Segunda ");
sb.Append("Terceira");
string resultado = sb.ToString(); // "Primeira Segunda Terceira"
```

**Quando usar StringBuilder:**
- Muitas concatenações em loops
- Construção dinâmica de strings complexas
- Performance crítica com manipulação intensa de texto

---

## Métodos de Transformação

### ToUpper() e ToLower()

```csharp
string texto = "Hello World";
string maiuscula = texto.ToUpper();   // "HELLO WORLD"
string minuscula = texto.ToLower();   // "hello world"
```

### Trim(), TrimStart() e TrimEnd()

```csharp
string texto = "  Olá Mundo  ";
string semEspacos = texto.Trim();           // "Olá Mundo"
string semInicio = texto.TrimStart();       // "Olá Mundo  "
string semFim = texto.TrimEnd();            // "  Olá Mundo"

// Remover caracteres específicos
string dados = "---Dados---";
string limpo = dados.Trim('-');             // "Dados"
```

### Replace()

```csharp
string texto = "Olá Mundo";
string novo = texto.Replace("Mundo", "C#");  // "Olá C#"
string semEspacos = texto.Replace(" ", "");  // "OláMundo"
```

### Substring()

```csharp
string texto = "Hello World";
string sub1 = texto.Substring(0, 5);    // "Hello" (início na posição 0, 5 caracteres)
string sub2 = texto.Substring(6);       // "World" (do índice 6 até o fim)
```

### Remove()

```csharp
string texto = "Hello World";
string removido = texto.Remove(5);      // "Hello" (remove do índice 5 em diante)
string removido2 = texto.Remove(5, 6);  // "Hello" (remove 6 caracteres a partir do índice 5)
```

### Insert()

```csharp
string texto = "Hello World";
string inserido = texto.Insert(5, " Beautiful"); // "Hello Beautiful World"
```

---

## Métodos de Pesquisa

### Contains()

```csharp
string texto = "Hello World";
bool contem = texto.Contains("World");  // true
bool naoContem = texto.Contains("C#");  // false
```

### StartsWith() e EndsWith()

```csharp
string arquivo = "documento.pdf";
bool isPdf = arquivo.EndsWith(".pdf");      // true
bool comecaComDoc = arquivo.StartsWith("doc"); // true
```

### IndexOf() e LastIndexOf()

```csharp
string texto = "Hello World, Hello C#";
int primeiroHello = texto.IndexOf("Hello");      // 0
int ultimoHello = texto.LastIndexOf("Hello");    // 13
int naoEncontrado = texto.IndexOf("Java");       // -1
```

---

## Divisão e Junção

### Split()

```csharp
string dados = "João,Maria,Pedro";
string[] nomes = dados.Split(',');
// nomes[0] = "João"
// nomes[1] = "Maria"
// nomes[2] = "Pedro"

// Split com múltiplos separadores
string texto = "um;dois,três;quatro";
string[] partes = texto.Split(new char[] { ',', ';' });

// Split com opções
string textoComEspacos = "a  b   c";
string[] semVazios = textoComEspacos.Split(new char[] { ' ' }, 
    StringSplitOptions.RemoveEmptyEntries);
```

### Join()

```csharp
string[] nomes = { "João", "Maria", "Pedro" };
string resultado = string.Join(", ", nomes); // "João, Maria, Pedro"

// Com números
int[] numeros = { 1, 2, 3, 4, 5 };
string numerosTexto = string.Join("-", numeros); // "1-2-3-4-5"
```

---

## Formatação

### String.Format()

```csharp
string nome = "João";
int idade = 25;
string texto = string.Format("Nome: {0}, Idade: {1}", nome, idade);
// "Nome: João, Idade: 25"
```

### Interpolação de String (Recomendado)

```csharp
string nome = "João";
int idade = 25;
string texto = $"Nome: {nome}, Idade: {idade}";

// Com formatação
double valor = 1234.56;
string formatado = $"Valor: {valor:C}";        // "Valor: R$ 1.234,56"
string comCasas = $"Valor: {valor:F2}";        // "Valor: 1234,56"

DateTime data = DateTime.Now;
string dataFormatada = $"Data: {data:dd/MM/yyyy}"; // "Data: 15/12/2024"
```

### Especificadores de Formato Comuns

| Especificador | Descrição | Exemplo | Resultado |
|--------------|-----------|---------|-----------|
| `C` ou `c` | Moeda | `{1234.56:C}` | R$ 1.234,56 |
| `D` ou `d` | Decimal | `{123:D5}` | 00123 |
| `F` ou `f` | Ponto fixo | `{123.456:F2}` | 123,46 |
| `N` ou `n` | Número | `{1234567.89:N}` | 1.234.567,89 |
| `P` ou `p` | Porcentagem | `{0.25:P}` | 25,00% |
| `X` ou `x` | Hexadecimal | `{255:X}` | FF |

---

## Comparação de Strings

### Operador ==

```csharp
string a = "hello";
string b = "hello";
bool igual = (a == b); // true
```

### Equals()

```csharp
string a = "Hello";
string b = "hello";

bool igual = a.Equals(b);  // false (case-sensitive)
bool igualIgnorandoCase = a.Equals(b, StringComparison.OrdinalIgnoreCase); // true
```

### Compare()

```csharp
string a = "apple";
string b = "banana";

int resultado = string.Compare(a, b);
// resultado < 0: a vem antes de b
// resultado = 0: são iguais
// resultado > 0: a vem depois de b

// Ignorando maiúsculas/minúsculas
int resultadoIgnoreCase = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
```

### CompareTo()

```csharp
string a = "apple";
string b = "banana";
int resultado = a.CompareTo(b); // -1 (a < b)
```

---

## Verificações

### IsNullOrEmpty()

```csharp
string texto1 = null;
string texto2 = "";
string texto3 = "   ";

bool resultado1 = string.IsNullOrEmpty(texto1); // true
bool resultado2 = string.IsNullOrEmpty(texto2); // true
bool resultado3 = string.IsNullOrEmpty(texto3); // false (tem espaços)
```

### IsNullOrWhiteSpace()

```csharp
string texto1 = null;
string texto2 = "";
string texto3 = "   ";
string texto4 = "texto";

bool resultado1 = string.IsNullOrWhiteSpace(texto1); // true
bool resultado2 = string.IsNullOrWhiteSpace(texto2); // true
bool resultado3 = string.IsNullOrWhiteSpace(texto3); // true
bool resultado4 = string.IsNullOrWhiteSpace(texto4); // false
```

---

## Propriedades

### Length

```csharp
string texto = "Hello";
int tamanho = texto.Length; // 5
```

### Indexador []

```csharp
string texto = "Hello";
char primeiraLetra = texto[0];  // 'H'
char ultimaLetra = texto[texto.Length - 1]; // 'o'

// Percorrer caracteres
foreach (char c in texto)
{
    Console.WriteLine(c);
}
```

---

## Caracteres Especiais e Escape

### Sequências de Escape

```csharp
string comAspas = "Ele disse: \"Olá!\"";           // Ele disse: "Olá!"
string comBarra = "C:\\Users\\Nome";                // C:\Users\Nome
string comNovaLinha = "Linha 1\nLinha 2";          // Quebra de linha
string comTab = "Coluna1\tColuna2";                // Tabulação
```

### Strings Verbatim (@)

```csharp
// Ignora escapes (útil para caminhos)
string caminho = @"C:\Users\Nome\Documents";

// Permite múltiplas linhas
string multilinhas = @"Linha 1
Linha 2
Linha 3";

// Aspas duplas são escapadas com ""
string comAspas = @"Ele disse: ""Olá!""";
```

### Interpolação com Verbatim

```csharp
string nome = "João";
string caminho = @$"C:\Users\{nome}\Documents";
```

---

## StringBuilder - Uso Avançado

```csharp
using System.Text;

StringBuilder sb = new StringBuilder();

// Append
sb.Append("Olá");
sb.Append(" ");
sb.Append("Mundo");

// AppendLine
sb.AppendLine("Primeira linha");
sb.AppendLine("Segunda linha");

// AppendFormat
sb.AppendFormat("Nome: {0}, Idade: {1}", "João", 25);

// Insert
sb.Insert(0, "Prefixo: ");

// Replace
sb.Replace("Mundo", "C#");

// Remove
sb.Remove(0, 5);

// Clear
sb.Clear();

// ToString
string resultado = sb.ToString();

// Capacidade inicial (otimização)
StringBuilder sbGrande = new StringBuilder(1000); // Reserva espaço
```

---

## Conversão

### Para String

```csharp
int numero = 123;
string texto1 = numero.ToString();           // "123"
string texto2 = Convert.ToString(numero);    // "123"

bool valor = true;
string texto3 = valor.ToString();            // "True"
```

### De String

```csharp
string texto = "123";

// Parse (lança exceção se falhar)
int numero1 = int.Parse(texto);

// TryParse (mais seguro)
bool sucesso = int.TryParse(texto, out int numero2);
if (sucesso)
{
    Console.WriteLine(numero2); // 123
}

// Outros tipos
double valorDouble = double.Parse("123.45");
bool valorBool = bool.Parse("true");
DateTime data = DateTime.Parse("15/12/2024");
```

---

## Boas Práticas

1. **Use `string.Empty` ao invés de `""`** para strings vazias
2. **Prefira interpolação (`$""`)** sobre concatenação (`+`) para legibilidade
3. **Use `StringBuilder`** para muitas concatenações ou loops
4. **Use `IsNullOrWhiteSpace()`** para validação robusta
5. **Sempre use `TryParse()`** ao invés de `Parse()` para conversões seguras
6. **Evite comparações case-sensitive** quando não necessário
7. **Use strings verbatim (`@`)** para caminhos de arquivo
8. **Considere performance**: strings são imutáveis, cada operação cria nova string
9. **Use `StringComparison`** apropriado para comparações culturalmente corretas
10. **Cuidado com `null`**: sempre valide antes de usar métodos

---

## Exemplos Práticos

### Validação de Email Simples

```csharp
bool ValidarEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;
    
    return email.Contains("@") && email.Contains(".");
}
```

### Formatação de CPF

```csharp
string FormatarCPF(string cpf)
{
    // Remove caracteres não numéricos
    cpf = new string(cpf.Where(char.IsDigit).ToArray());
    
    if (cpf.Length != 11)
        return cpf;
    
    return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
}
```

### Contar Palavras

```csharp
int ContarPalavras(string texto)
{
    if (string.IsNullOrWhiteSpace(texto))
        return 0;
    
    string[] palavras = texto.Split(new char[] { ' ', '\t', '\n', '\r' }, 
        StringSplitOptions.RemoveEmptyEntries);
    
    return palavras.Length;
}
```

### Inverter String

```csharp
string InverterString(string texto)
{
    char[] caracteres = texto.ToCharArray();
    Array.Reverse(caracteres);
    return new string(caracteres);
}
```

### Capitalizar Primeira Letra

```csharp
string CapitalizarPrimeiraLetra(string texto)
{
    if (string.IsNullOrWhiteSpace(texto))
        return texto;
    
    return char.ToUpper(texto[0]) + texto.Substring(1).ToLower();
}
```

### Remover Acentos

```csharp
using System.Globalization;
using System.Text;

string RemoverAcentos(string texto)
{
    string normalizado = texto.Normalize(NormalizationForm.FormD);
    StringBuilder sb = new StringBuilder();
    
    foreach (char c in normalizado)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        {
            sb.Append(c);
        }
    }
    
    return sb.ToString().Normalize(NormalizationForm.FormC);
}
```

---

## Recursos Adicionais

### Span\<char\> e ReadOnlySpan\<char\> (C# 7.2+)

Para operações de alta performance sem alocações:

```csharp
ReadOnlySpan<char> span = "Hello World".AsSpan();
ReadOnlySpan<char> hello = span.Slice(0, 5); // Sem criar nova string
```

### String Interpolation Avançada

```csharp
// Alinhamento
string nome = "João";
string formatado = $"|{nome,-10}|"; // |João      | (alinhado à esquerda, 10 caracteres)
string formatado2 = $"|{nome,10}|"; // |      João| (alinhado à direita)

// Combinando formatação e alinhamento
double valor = 123.456;
string resultado = $"|{valor,10:F2}|"; // |    123,46|
```
