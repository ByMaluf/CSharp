# Modificador de Acesso `protected` em Herança

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Estrutura com Device, Laptop e Smartphone](#estrutura-com-device-laptop-e-smartphone)
3. [Reutilizando métodos da classe base](#reutilizando-métodos-da-classe-base)
4. [O problema com public](#o-problema-com-public)
5. [Tentando resolver com private](#tentando-resolver-com-private)
6. [O modificador protected](#o-modificador-protected)
7. [Quem pode acessar um membro protected?](#quem-pode-acessar-um-membro-protected)
8. [protected em métodos e propriedades](#protected-em-métodos-e-propriedades)
9. [Comparação entre public, private e protected](#comparação-entre-public-private-e-protected)
10. [Resumo](#resumo)

---

## Contexto da aula

Nesta aula, continuamos estudando **herança entre classes** e introduzimos um novo modificador de acesso:

~~~csharp
protected
~~~

Antes, já tínhamos visto modificadores como:

~~~text
public
private
internal
~~~

Agora o foco é entender um modificador especialmente útil em cenários de herança.

---

## Estrutura com Device, Laptop e Smartphone

A aula utiliza três classes principais.

### Classe base

~~~csharp
Device
~~~

### Classes derivadas

~~~csharp
Laptop : Device
Smartphone : Device
~~~

Estrutura:

~~~text
        Device
          │
     ┌────┴─────┐
     ▼          ▼
  Laptop    Smartphone
~~~

Portanto:

~~~text
Device
↓
Classe base

Laptop
↓
Classe derivada

Smartphone
↓
Classe derivada
~~~

---

## Método definido na classe base

Na classe `Device`, existe um método que informa se o dispositivo está conectado.

Exemplo conceitual:

~~~csharp
public bool IsConnected()
{
    return true;
}
~~~

Ou, em outro teste:

~~~csharp
public bool IsConnected()
{
    return false;
}
~~~

Esse método pertence à classe base:

~~~text
Device
│
└── IsConnected()
~~~

---

## Reutilizando métodos da classe base

Como `Laptop` herda de `Device`, ele consegue utilizar o método herdado.

Exemplo:

~~~csharp
public class Laptop : Device
{
    public string GetModel()
    {
        var isConnected = IsConnected();

        if (isConnected)
            return "Macbook";

        return "Unknown";
    }
}
~~~

Fluxo:

~~~text
Laptop.GetModel()
      │
      ▼
IsConnected()
      │
      ├── true  → "Macbook"
      │
      └── false → "Unknown"
~~~

---

## Execução do exemplo

Se:

~~~csharp
IsConnected()
~~~

retorna:

~~~text
true
~~~

então:

~~~text
GetModel()
   ↓
if executado
   ↓
return "Macbook"
~~~

Se retornar:

~~~text
false
~~~

então:

~~~text
GetModel()
   ↓
if não executado
   ↓
return "Unknown"
~~~

A aula utiliza o debugger para acompanhar esse fluxo com `F10` e `F11`.

---

## Utilizando Laptop no Controller

No `DeviceController`, é criada uma instância:

~~~csharp
var laptop = new Laptop();
~~~

Depois:

~~~csharp
var model = laptop.GetModel();
~~~

E o valor é retornado.

Exemplo:

~~~csharp
return Ok(model);
~~~

Fluxo:

~~~text
DeviceController
      │
      ▼
new Laptop()
      │
      ▼
GetModel()
      │
      ▼
IsConnected()
      │
      ▼
"Macbook" ou "Unknown"
~~~

---

## O problema com public

Enquanto `IsConnected` está como:

~~~csharp
public
~~~

qualquer código que tenha acesso a uma instância de `Laptop` também consegue chamar:

~~~csharp
laptop.IsConnected();
~~~

Por exemplo, dentro do Controller:

~~~csharp
var laptop = new Laptop();

var x = laptop.IsConnected();
~~~

Isso pode ser válido ou não dependendo da regra de negócio.

Na situação apresentada na aula, queremos evitar que código externo consiga chamar esse método diretamente.

Queremos que:

~~~text
Laptop
↓
possa utilizar IsConnected()

DeviceController
↓
não possa utilizar IsConnected() diretamente
~~~

---

## Tentando resolver com private

Uma primeira tentativa seria alterar:

~~~csharp
public bool IsConnected()
~~~

para:

~~~csharp
private bool IsConnected()
~~~

Com isso, o Controller deixa de conseguir acessar o método.

Isso parece resolver parte do problema.

Porém, surge outro problema:

~~~text
Laptop também perde o acesso.
~~~

---

## Como funciona private?

Quando um membro é:

~~~csharp
private
~~~

ele só pode ser utilizado dentro da própria classe em que foi declarado.

Exemplo:

~~~csharp
public class Device
{
    private bool IsConnected()
    {
        return true;
    }

    public void Test()
    {
        var x = IsConnected();
    }
}
~~~

Aqui funciona porque:

~~~text
Test()
e
IsConnected()
~~~

estão dentro da mesma classe:

~~~text
Device
~~~

---

## Problema do private com herança

Se:

~~~csharp
IsConnected()
~~~

for `private` em `Device`, então `Laptop` não poderá utilizá-lo.

Estrutura:

~~~text
Device
│
├── private IsConnected()
│
└── Test() → pode acessar
│
▼
Laptop
└── NÃO pode acessar IsConnected()
~~~

Isso não atende ao objetivo da aula.

Precisamos de algo intermediário:

~~~text
A própria classe pode acessar
+
Classes derivadas podem acessar
+
Código externo não pode acessar
~~~

É exatamente para esse cenário que utilizamos:

~~~csharp
protected
~~~

---

## O modificador protected

Podemos declarar:

~~~csharp
protected bool IsConnected()
{
    return true;
}
~~~

Agora o método pode ser utilizado:

- pela própria classe `Device`;
- pelas classes que herdam de `Device`.

Mas não fica exposto da mesma forma para classes externas.

---

## Exemplo completo

Classe base:

~~~csharp
public class Device
{
    protected bool IsConnected()
    {
        return true;
    }
}
~~~

Classe derivada:

~~~csharp
public class Laptop : Device
{
    public string GetModel()
    {
        var isConnected = IsConnected();

        if (isConnected)
            return "Macbook";

        return "Unknown";
    }
}
~~~

Aqui:

~~~text
Laptop
↓
pode utilizar IsConnected()
~~~

porque:

~~~text
Laptop : Device
~~~

---

## Quem pode acessar um membro protected?

Um membro `protected` pode ser acessado por:

~~~text
A própria classe
+
Classes derivadas
~~~

Exemplo:

~~~text
Device
│
├── protected IsConnected()
│
├── pode acessar
│
│
└── Laptop : Device
    └── pode acessar
~~~

Por outro lado:

~~~text
DeviceController
↓
não herda de Device
↓
não pode acessar IsConnected()
~~~

---

## Relação visual

~~~text
             Device
               │
               │ protected IsConnected()
               │
        ┌──────┴──────┐
        ▼             ▼
     Laptop       Smartphone
        │             │
        ▼             ▼
 podem acessar   podem acessar

DeviceController
        │
        ▼
 NÃO pode acessar diretamente
~~~

---

## protected em métodos e propriedades

O modificador `protected` não serve apenas para métodos.

Ele também pode ser utilizado em propriedades.

Exemplo:

~~~csharp
protected string Name { get; set; } = string.Empty;
~~~

Outro exemplo:

~~~csharp
protected string Brand { get; set; } = string.Empty;
~~~

Ou:

~~~csharp
protected int Id { get; set; }
~~~

Portanto:

~~~text
protected
│
├── Métodos
└── Propriedades
~~~

podem utilizar esse nível de acesso.

---

## Exemplo com propriedade

Classe base:

~~~csharp
public class Device
{
    protected string Brand { get; set; } = "Apple";
}
~~~

Classe derivada:

~~~csharp
public class Laptop : Device
{
    public string GetBrand()
    {
        return Brand;
    }
}
~~~

Isso funciona porque:

~~~text
Laptop
↓
é derivado de Device
~~~

---

## protected e herança

A ideia principal apresentada na aula é:

~~~text
protected
↓
Membro voltado para uso interno da hierarquia de herança
~~~

Ou seja:

~~~text
Classe base
+
Classes derivadas
~~~

conseguem utilizá-lo.

Código externo que apenas instancia a classe derivada não possui o mesmo acesso.

---

## Comparação entre public, private e protected

| Modificador | Própria classe | Classe derivada | Código externo |
|---|---:|---:|---:|
| `public` | ✅ | ✅ | ✅ |
| `private` | ✅ | ❌ | ❌ |
| `protected` | ✅ | ✅ | ❌ |

Podemos visualizar assim:

~~~text
PUBLIC
│
├── própria classe
├── classes filhas
└── classes externas


PRIVATE
│
└── própria classe


PROTECTED
│
├── própria classe
└── classes filhas
~~~

---

## Qual problema o protected resolve?

Queremos proteger uma implementação da classe base sem impedir que classes derivadas reutilizem essa implementação.

Exemplo:

~~~text
IsConnected()
~~~

não deve ser chamado diretamente pelo Controller.

Mas:

~~~text
Laptop
~~~

precisa utilizar esse método internamente.

Então:

~~~csharp
protected bool IsConnected()
~~~

é adequado para o cenário apresentado.

---

## public x private x protected no exemplo

### public

~~~csharp
public bool IsConnected()
~~~

Resultado:

~~~text
Device      ✅
Laptop      ✅
Controller  ✅
~~~

---

### private

~~~csharp
private bool IsConnected()
~~~

Resultado:

~~~text
Device      ✅
Laptop      ❌
Controller  ❌
~~~

---

### protected

~~~csharp
protected bool IsConnected()
~~~

Resultado:

~~~text
Device      ✅
Laptop      ✅
Controller  ❌
~~~

Esse é exatamente o comportamento desejado no exemplo da aula.

---

## Relação com encapsulamento

Mesmo que o foco da aula seja herança, esse uso também ajuda a controlar o que fica acessível externamente.

Em vez de expor:

~~~csharp
IsConnected()
~~~

publicamente, mantemos seu uso restrito à hierarquia de classes.

Fluxo:

~~~text
Device
↓
Implementação interna

Laptop
↓
Reutiliza implementação

Controller
↓
Utiliza apenas operações públicas de Laptop
~~~

Por exemplo:

~~~csharp
laptop.GetModel();
~~~

é público.

Mas:

~~~csharp
laptop.IsConnected();
~~~

não fica disponível externamente quando `IsConnected` é `protected`.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **protected** | Modificador de acesso utilizado para permitir acesso pela própria classe e por classes derivadas |
| **Device** | Classe base do exemplo |
| **Laptop** | Classe derivada de Device |
| **Smartphone** | Outra classe derivada de Device |
| **IsConnected()** | Método definido na classe base |
| **GetModel()** | Método da classe Laptop que reutiliza IsConnected() |
| **public** | Permite acesso também por código externo |
| **private** | Restringe o acesso apenas à própria classe |
| **protected** | Permite acesso à própria classe e às classes filhas |
| **Classe derivada** | Pode acessar membros protected da classe base |
| **Controller** | Não consegue chamar diretamente o membro protected nesse exemplo |
| **Propriedades** | Também podem utilizar protected |
| **Métodos** | Também podem utilizar protected |

---

## Visão Geral

~~~text
                    Device
                      │
                      │ protected
                      ▼
                IsConnected()
                      │
           ┌──────────┴──────────┐
           │                     │
           ▼                     ▼
        Laptop              Smartphone
           │
           ▼
    pode utilizar
    IsConnected()


      DeviceController
             │
             ▼
     NÃO pode acessar
 IsConnected() diretamente
~~~

### Comparação final

~~~text
public
├── classe
├── derivados
└── externo

private
└── classe

protected
├── classe
└── derivados
~~~

> **Em resumo:** `protected` é útil quando queremos que um membro possa ser utilizado pela própria classe e pelas classes que herdam dela, mas não queremos expor esse membro para código externo. No exemplo da aula, `IsConnected()` precisa ser utilizado por `Laptop`, que herda de `Device`, mas não deve ser chamado diretamente pelo `DeviceController`. Esse é exatamente o cenário atendido por `protected`.
