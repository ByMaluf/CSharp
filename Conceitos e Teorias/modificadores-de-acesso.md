# Modificadores de Acesso em C#

## 📋 Índice
1. [Modificadores Disponíveis](#modificadores-disponíveis)
   - [public](#1-public)
   - [private](#2-private)
   - [protected](#3-protected)
   - [internal](#4-internal)
   - [protected internal](#5-protected-internal)
   - [private protected](#6-private-protected)
2. [Resumo de Acessibilidade](#resumo-de-acessibilidade)
3. [Modificadores Padrão](#modificadores-padrão)
4. [Modificadores por Tipo de Elemento](#modificadores-por-tipo-de-elemento)
   - [Classes (Top-Level)](#classes-top-level---declaradas-diretamente-no-namespace)
   - [Classes Aninhadas](#classes-aninhadas-nested-classes)
   - [Interfaces](#interfaces-top-level)
   - [Métodos e Funções](#métodos-e-funções)
   - [Propriedades](#propriedades)
   - [Campos (Variáveis)](#campos-variáveis-de-classe)
   - [Construtores](#construtores)
   - [Structs](#structs)
   - [Enums](#enums)
   - [Delegates](#delegates)
5. [Boas Práticas](#boas-práticas)
6. [Exemplos Práticos](#exemplos-práticos)

---

Os modificadores de acesso em C# são palavras-chave que definem a acessibilidade de tipos (classes, interfaces, etc.) e membros (métodos, propriedades, campos, etc.). Eles controlam de onde um determinado elemento pode ser acessado no código.

## Modificadores Disponíveis

### 1. `public`

**Descrição:** O modificador de acesso mais permissivo. Membros públicos podem ser acessados de qualquer lugar, sem restrições.

**Uso:**
```csharp
public class MinhaClasse
{
    public int NumeroPublico;
    
    public void MetodoPublico()
    {
        Console.WriteLine("Pode ser chamado de qualquer lugar");
    }
}
```

**Acessibilidade:** Sem restrições - qualquer código pode acessar.

---

### 2. `private`

**Descrição:** O modificador de acesso mais restritivo. Membros privados só podem ser acessados dentro da própria classe ou struct onde foram declarados.

**Uso:**
```csharp
public class MinhaClasse
{
    private int numeroPrivado;
    
    private void MetodoPrivado()
    {
        Console.WriteLine("Só pode ser chamado dentro desta classe");
    }
}
```

**Acessibilidade:** Apenas dentro do tipo declarado.

**Observação:** É o modificador padrão para membros de classe.

---

### 3. `protected`

**Descrição:** Membros protegidos podem ser acessados dentro da classe onde foram declarados e também por classes derivadas (herdeiras).

**Uso:**
```csharp
public class ClasseBase
{
    protected int NumeroProtegido;
    
    protected void MetodoProtegido()
    {
        Console.WriteLine("Acessível em classes derivadas");
    }
}

public class ClasseDerivada : ClasseBase
{
    public void UsarProtegido()
    {
        NumeroProtegido = 10; // OK - pode acessar membro protected da classe base
        MetodoProtegido();     // OK
    }
}
```

**Acessibilidade:** Dentro do tipo declarado e tipos derivados.

---

### 4. `internal`

**Descrição:** Membros internos podem ser acessados por qualquer código dentro do mesmo assembly (projeto), mas não de outros assemblies.

**Uso:**
```csharp
internal class ClasseInterna
{
    internal int NumeroInterno;
    
    internal void MetodoInterno()
    {
        Console.WriteLine("Acessível apenas dentro do mesmo assembly");
    }
}
```

**Acessibilidade:** Apenas dentro do mesmo assembly.

**Observação:** É o modificador padrão para classes declaradas em um namespace.

---

### 5. `protected internal`

**Descrição:** Combinação de `protected` e `internal`. Membros podem ser acessados por qualquer código no mesmo assembly OU por classes derivadas em qualquer assembly.

**Uso:**
```csharp
public class MinhaClasse
{
    protected internal int NumeroProtectedInternal;
    
    protected internal void MetodoProtectedInternal()
    {
        Console.WriteLine("Acessível no mesmo assembly OU em classes derivadas");
    }
}
```

**Acessibilidade:** Dentro do mesmo assembly OU em tipos derivados (união dos dois).

---

### 6. `private protected`

**Descrição:** Combinação mais restritiva de `private` e `protected`. Membros podem ser acessados apenas por classes derivadas que estejam no mesmo assembly.

**Uso:**
```csharp
public class ClasseBase
{
    private protected int NumeroPrivateProtected;
    
    private protected void MetodoPrivateProtected()
    {
        Console.WriteLine("Acessível apenas em classes derivadas do mesmo assembly");
    }
}

// No mesmo assembly
public class ClasseDerivada : ClasseBase
{
    public void Usar()
    {
        NumeroPrivateProtected = 5; // OK - mesma assembly e derivada
    }
}
```

**Acessibilidade:** Dentro do mesmo assembly E em tipos derivados (interseção dos dois).

**Observação:** Disponível a partir do C# 7.2.

---

## Resumo de Acessibilidade

| Modificador | Mesma Classe | Classes Derivadas (mesmo assembly) | Classes Derivadas (outro assembly) | Mesmo Assembly | Outro Assembly |
|-------------|--------------|-----------------------------------|-----------------------------------|----------------|----------------|
| `public` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `private` | ✅ | ❌ | ❌ | ❌ | ❌ |
| `protected` | ✅ | ✅ | ✅ | ❌ | ❌ |
| `internal` | ✅ | ✅ | ❌ | ✅ | ❌ |
| `protected internal` | ✅ | ✅ | ✅ | ✅ | ❌ |
| `private protected` | ✅ | ✅ | ❌ | ❌ | ❌ |

---

## Modificadores Padrão

- **Membros de classe/struct:** `private`
- **Classes/interfaces no namespace:** `internal`
- **Membros de interface:** `public` (implícito)
- **Membros de enum:** `public` (implícito)

---

## Modificadores por Tipo de Elemento

### Classes (Top-Level - Declaradas Diretamente no Namespace)

**Modificadores Permitidos:** `public`, `internal`

```csharp
// ✅ Válido
public class ClassePublica { }

// ✅ Válido
internal class ClasseInterna { }

// ❌ Inválido - classes de nível superior não podem ser private, protected, etc.
// private class Classe { }
```

**O que isso implica:**
- **`public`**: A classe pode ser usada por qualquer código, inclusive de outros assemblies (projetos). Use quando a classe faz parte da API pública do seu projeto.
- **`internal`** (padrão): A classe só pode ser usada dentro do mesmo assembly. Use para classes de implementação interna que não devem ser expostas externamente.

---

### Classes Aninhadas (Nested Classes)

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
public class ClasseExterna
{
    // ✅ Todos os modificadores são válidos para classes aninhadas
    public class ClassePublica { }
    private class ClassePrivada { }
    protected class ClasseProtegida { }
    internal class ClasseInterna { }
    protected internal class ClasseProtectedInternal { }
    private protected class ClassePrivateProtected { }
}
```

**O que isso implica:**
- Classes aninhadas seguem as mesmas regras de acessibilidade dos membros da classe
- A acessibilidade efetiva é limitada pela classe externa (uma classe `public` dentro de uma classe `internal` ainda será acessível apenas internamente)

---

### Interfaces (Top-Level)

**Modificadores Permitidos:** `public`, `internal`

```csharp
// ✅ Válido
public interface IMinhaInterface { }

// ✅ Válido
internal interface IInterfaceInterna { }
```

**O que isso implica:**
- Mesmas regras que classes de nível superior
- Interfaces públicas fazem parte do contrato da API
- Interfaces internas são para uso exclusivo do assembly

---

### Métodos e Funções

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
public class MinhaClasse
{
    // ✅ Todos os modificadores são válidos
    public void MetodoPublico() { }
    private void MetodoPrivado() { }
    protected void MetodoProtegido() { }
    internal void MetodoInterno() { }
    protected internal void MetodoProtectedInternal() { }
    private protected void MetodoPrivateProtected() { }
}
```

**O que isso implica:**
- **`public`**: Pode ser chamado de qualquer lugar (parte da interface pública da classe)
- **`private`**: Apenas métodos auxiliares internos da classe
- **`protected`**: Para métodos que devem ser acessíveis ou sobrescritos por classes derivadas
- **`internal`**: Para métodos que fazem parte da API interna do assembly
- **`protected internal`**: Para métodos acessíveis no assembly ou em heranças externas
- **`private protected`**: Para métodos protegidos apenas dentro do assembly

---

### Propriedades

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
public class MinhaClasse
{
    // ✅ Propriedades podem usar todos os modificadores
    public string PropriedadePublica { get; set; }
    private string PropriedadePrivada { get; set; }
    protected string PropriedadeProtegida { get; set; }
    internal string PropriedadeInterna { get; set; }

    // ✅ Acessores podem ter modificadores diferentes (mais restritivos)
    public string Nome { get; private set; }
    public int Idade { get; protected set; }
}
```

**O que isso implica:**
- Propriedades seguem as mesmas regras de métodos
- Acessores (`get`/`set`) podem ter modificadores mais restritivos que a propriedade
- Comum usar `public get` com `private set` para expor dados somente leitura

---

### Campos (Variáveis de Classe)

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
public class MinhaClasse
{
    // ✅ Campos podem usar todos os modificadores
    public int campoPublico;
    private int campoPrivado;
    protected int campoProtegido;
    internal int campoInterno;
    protected internal int campoProtectedInternal;
    private protected int campoPrivateProtected;
}
```

**O que isso implica:**
- **Boa prática**: Campos geralmente devem ser `private` e expostos via propriedades
- Campos `public` violam o encapsulamento e devem ser evitados
- Campos `protected` podem ser úteis para compartilhar estado com classes derivadas
- Campos `readonly` ou `const` públicos são mais aceitáveis

---

### Construtores

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
public class MinhaClasse
{
    // ✅ Construtor público - qualquer código pode instanciar
    public MinhaClasse() { }

    // ✅ Construtor privado - usado em Singleton ou Factory patterns
    private MinhaClasse(int parametro) { }

    // ✅ Construtor protegido - apenas classes derivadas podem chamar
    protected MinhaClasse(string parametro) { }
}
```

**O que isso implica:**
- **`public`**: Qualquer código pode criar instâncias
- **`private`**: Impede instanciação externa (padrões Singleton, Factory)
- **`protected`**: Permite que apenas classes derivadas criem instâncias
- **`internal`**: Restringe criação de instâncias ao assembly

---

### Structs

**Modificadores Permitidos (para o struct):** `public`, `internal`  
**Modificadores Permitidos (para membros):** `public`, `private`, `internal`

```csharp
// ✅ Struct público
public struct MeuStruct
{
    public int CampoPublico;
    private int campoPrivado;
    internal int campoInterno;

    // ❌ Structs não podem ter membros protected
    // protected int campoProtegido; // ERRO!
}
```

**O que isso implica:**
- Structs não suportam herança, portanto `protected` não faz sentido
- Mesmas regras de `public`/`internal` que classes para o tipo em si
- Membros só podem ser `public`, `private` ou `internal`

---

### Enums

**Modificadores Permitidos (para o enum):** `public`, `internal`  
**Modificadores Permitidos (para valores):** Nenhum (sempre `public`)

```csharp
// ✅ Enum público
public enum DiaSemana
{
    Segunda,  // Sempre público
    Terca,    // Sempre público
    Quarta    // Sempre público
}

// ✅ Enum interno
internal enum StatusInterno
{
    Ativo,
    Inativo
}
```

**O que isso implica:**
- Valores do enum são sempre públicos dentro do escopo do enum
- O enum em si pode ser `public` ou `internal`

---

### Delegates

**Modificadores Permitidos:** `public`, `private`, `protected`, `internal`, `protected internal`, `private protected`

```csharp
// Delegate de nível superior
public delegate void MeuDelegate(string mensagem);
internal delegate int DelegateInterno(int x, int y);

public class MinhaClasse
{
    // Delegates aninhados podem usar todos os modificadores
    private delegate void DelegatePrivado();
    protected delegate void DelegateProtegido();
}
```

**O que isso implica:**
- Delegates de nível superior: apenas `public` ou `internal`
- Delegates aninhados: todos os modificadores disponíveis

---

## Boas Práticas

1. **Princípio do Menor Privilégio:** Use sempre o modificador mais restritivo possível.
2. **Encapsulamento:** Mantenha campos como `private` e exponha através de propriedades públicas quando necessário.
3. **Clareza:** Declare explicitamente o modificador de acesso, mesmo quando é o padrão.
4. **APIs Públicas:** Tenha cuidado ao tornar membros `public`, pois isso cria um contrato que deve ser mantido.

---

## Exemplos Práticos

```csharp
public class Pessoa
{
    // Campo privado - apenas acessível dentro da classe
    private string nome;
    
    // Propriedade pública - interface para acessar o campo privado
    public string Nome
    {
        get { return nome; }
        set { nome = value; }
    }
    
    // Método protegido - acessível em classes derivadas
    protected void ValidarNome()
    {
        if (string.IsNullOrEmpty(nome))
            throw new ArgumentException("Nome não pode ser vazio");
    }
    
    // Método interno - acessível apenas no mesmo assembly
    internal void MetodoInterno()
    {
        // Lógica interna do assembly
    }
}
```
