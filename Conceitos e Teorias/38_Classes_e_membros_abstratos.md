# Classes e Membros Abstratos em C#

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [O problema de instanciar a classe base](#o-problema-de-instanciar-a-classe-base)
3. [Criando uma classe abstrata](#criando-uma-classe-abstrata)
4. [O que abstract faz em uma classe](#o-que-abstract-faz-em-uma-classe)
5. [Classes derivadas continuam podendo ser instanciadas](#classes-derivadas-continuam-podendo-ser-instanciadas)
6. [Membros abstratos](#membros-abstratos)
7. [Regra entre classe abstrata e membro abstrato](#regra-entre-classe-abstrata-e-membro-abstrato)
8. [Método abstrato não possui implementação](#método-abstrato-não-possui-implementação)
9. [Obrigando classes filhas a implementar um método](#obrigando-classes-filhas-a-implementar-um-método)
10. [Implementando GetBrand em Laptop e Smartphone](#implementando-getbrand-em-laptop-e-smartphone)
11. [Propriedades abstratas](#propriedades-abstratas)
12. [Classe abstrata x membro abstrato](#classe-abstrata-x-membro-abstrato)
13. [Resumo](#resumo)

---

## Contexto da aula

Nesta aula, continuamos estudando herança.

Na aula anterior, foi apresentado o modificador:

~~~csharp
protected
~~~

Agora o foco passa para:

~~~csharp
abstract
~~~

Esse modificador pode ser utilizado em:

~~~text
Classes
Métodos
Propriedades
~~~

Mas seu comportamento muda de acordo com onde ele é aplicado.

---

## O problema de instanciar a classe base

Temos a seguinte hierarquia:

~~~text
        Device
          │
     ┌────┴─────┐
     ▼          ▼
  Laptop    Smartphone
~~~

Nesse exemplo:

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

Sem nenhuma restrição, podemos fazer:

~~~csharp
var device = new Device();
~~~

Ou seja, é possível criar uma instância diretamente da classe base.

A pergunta apresentada na aula é:

> Faz sentido existir uma instância direta de Device?

A resposta depende da regra de negócio.

No cenário apresentado, vamos assumir que:

~~~text
Device
↓
Representa apenas um conceito genérico
~~~

e que somente dispositivos concretos devem ser instanciados:

~~~text
Laptop
Smartphone
~~~

---

## Criando uma classe abstrata

Para impedir a instanciação direta da classe base, podemos utilizar:

~~~csharp
abstract
~~~

Exemplo:

~~~csharp
public abstract class Device
{
}
~~~

Agora isto deixa de ser permitido:

~~~csharp
var device = new Device();
~~~

O compilador informa que não é possível criar uma instância de uma classe abstrata.

---

## O que abstract faz em uma classe

Quando colocamos:

~~~csharp
abstract
~~~

em uma classe, estamos dizendo que essa classe não pode ser instanciada diretamente.

Exemplo:

~~~csharp
public abstract class Device
{
}
~~~

Então:

~~~text
new Device()
    ❌
~~~

Mas isso não impede que ela seja utilizada como classe base.

~~~text
Device
  │
  ├── Laptop
  └── Smartphone
~~~

---

## Classes derivadas continuam podendo ser instanciadas

Mesmo que Device seja abstrata, ainda podemos fazer:

~~~csharp
var laptop = new Laptop();
~~~

e:

~~~csharp
var smartphone = new Smartphone();
~~~

Portanto:

~~~text
new Device()
❌

new Laptop()
✅

new Smartphone()
✅
~~~

A classe abstrata serve como base para outras classes.

---

## Membros abstratos

O modificador `abstract` também pode ser utilizado em membros da classe.

Por exemplo:

~~~text
Métodos
Propriedades
~~~

Mas aqui o objetivo é diferente.

Quando um método ou propriedade é abstrato, as classes derivadas ficam obrigadas a fornecer uma implementação para aquele membro.

---

## Regra entre classe abstrata e membro abstrato

Existe uma regra importante.

Se uma classe possui um membro abstrato:

~~~csharp
public abstract string GetBrand();
~~~

então a própria classe também precisa ser abstrata.

Exemplo válido:

~~~csharp
public abstract class Device
{
    public abstract string GetBrand();
}
~~~

Agora, o contrário não é obrigatório.

Podemos ter:

~~~csharp
public abstract class Device
{
}
~~~

sem nenhum método ou propriedade abstrata.

Portanto:

~~~text
Classe abstrata
↓
NÃO precisa obrigatoriamente ter membros abstratos
~~~

Mas:

~~~text
Membro abstrato
↓
EXIGE que a classe seja abstrata
~~~

---

## Método abstrato não possui implementação

Um método abstrato não possui corpo na classe base.

Isto não é válido:

~~~csharp
public abstract string GetBrand()
{
    return "Apple";
}
~~~

O método abstrato deve ser apenas declarado:

~~~csharp
public abstract string GetBrand();
~~~

Observe:

~~~text
Sem { }
Sem implementação
Com ;
~~~

Isso acontece porque a implementação será responsabilidade das classes derivadas.

---

## Por que utilizar um método abstrato?

Imagine que Device represente um dispositivo genérico.

A classe base sabe que todo dispositivo precisa possuir uma marca.

Mas ela não sabe qual será essa marca.

~~~text
Device
↓
Sabe que precisa existir GetBrand()

Laptop
↓
Pode retornar "Apple"

Smartphone
↓
Pode retornar "Samsung"
~~~

Então a classe base define o contrato:

~~~csharp
public abstract string GetBrand();
~~~

E cada classe derivada fornece sua própria implementação.

---

## Obrigando classes filhas a implementar um método

Classe base:

~~~csharp
public abstract class Device
{
    public abstract string GetBrand();
}
~~~

Agora:

~~~csharp
public class Laptop : Device
{
}
~~~

gera erro.

Isso acontece porque Laptop herdou de Device, mas não implementou:

~~~csharp
GetBrand()
~~~

A mesma coisa acontece com:

~~~csharp
public class Smartphone : Device
{
}
~~~

Enquanto as classes filhas não implementarem o membro abstrato, elas permanecerão com erro.

---

## Implementando GetBrand em Laptop e Smartphone

### Laptop

Podemos implementar:

~~~csharp
public class Laptop : Device
{
    public override string GetBrand()
    {
        return "Apple";
    }
}
~~~

### Smartphone

Podemos implementar:

~~~csharp
public class Smartphone : Device
{
    public override string GetBrand()
    {
        return "Samsung";
    }
}
~~~

Agora cada classe possui sua própria implementação.

---

## Fluxo da implementação

~~~text
Device
│
└── abstract GetBrand()
        │
        ├───────────────┐
        ▼               ▼
     Laptop         Smartphone
        │               │
        ▼               ▼
     "Apple"         "Samsung"
~~~

A classe base define:

~~~text
O método precisa existir
~~~

As classes derivadas definem:

~~~text
Como ele funciona
~~~

---

## Testando pelo Controller

No Controller:

~~~csharp
var laptop = new Laptop();

var brand = laptop.GetBrand();

return Ok(brand);
~~~

Resultado:

~~~text
Apple
~~~

Se utilizarmos:

~~~csharp
var smartphone = new Smartphone();

var brand = smartphone.GetBrand();
~~~

o resultado será:

~~~text
Samsung
~~~

---

## Classe abstrata pode ter métodos normais

Uma classe abstrata não precisa possuir apenas membros abstratos.

Podemos ter:

~~~csharp
public abstract class Device
{
    public abstract string GetBrand();

    public bool IsConnected()
    {
        return true;
    }
}
~~~

Nesse caso:

~~~text
GetBrand()
↓
Abstrato
↓
Classes filhas precisam implementar

IsConnected()
↓
Já possui implementação
↓
Pode ser herdado normalmente
~~~

---

## Propriedades abstratas

Também podemos utilizar `abstract` em propriedades.

Exemplo:

~~~csharp
public abstract string Name { get; set; }
~~~

Se Device possuir essa propriedade:

~~~csharp
public abstract class Device
{
    public abstract string Name { get; set; }
}
~~~

as classes derivadas também precisarão implementá-la.

Por exemplo:

~~~csharp
public class Laptop : Device
{
    public override string Name { get; set; } = string.Empty;
}
~~~

Portanto:

~~~text
Método abstrato
↓
Classes filhas implementam

Propriedade abstrata
↓
Classes filhas implementam
~~~

---

## Classe abstrata x membro abstrato

É importante separar os dois conceitos.

### Classe abstrata

~~~csharp
public abstract class Device
~~~

Objetivo principal apresentado:

~~~text
Impedir new Device()
~~~

Ela pode ter:

~~~text
Métodos normais
Métodos abstratos
Propriedades normais
Propriedades abstratas
~~~

---

### Método abstrato

~~~csharp
public abstract string GetBrand();
~~~

Objetivo:

~~~text
Obrigar as classes derivadas a implementar GetBrand()
~~~

Não possui implementação na classe base.

---

### Propriedade abstrata

~~~csharp
public abstract string Name { get; set; }
~~~

Objetivo:

~~~text
Obrigar as classes derivadas a implementar essa propriedade
~~~

---

## Relação com herança

Classes abstratas existem para serem utilizadas dentro de uma hierarquia de herança.

Exemplo:

~~~text
          Device
        (abstract)
            │
      ┌─────┴─────┐
      ▼           ▼
   Laptop     Smartphone
~~~

A classe base pode definir:

~~~text
Comportamentos comuns
+
Contratos obrigatórios
~~~

As classes derivadas implementam os detalhes específicos.

---

## Comparação visual

### Classe comum

~~~csharp
public class Device
{
}
~~~

Permite:

~~~csharp
new Device();
~~~

---

### Classe abstrata

~~~csharp
public abstract class Device
{
}
~~~

Não permite:

~~~csharp
new Device();
~~~

---

### Método comum

~~~csharp
public string GetBrand()
{
    return "Apple";
}
~~~

Já possui implementação.

---

### Método abstrato

~~~csharp
public abstract string GetBrand();
~~~

Não possui implementação e obriga as classes derivadas a implementá-lo.

---

## Regras importantes

### Regra 1

Uma classe abstrata não pode ser instanciada diretamente.

~~~text
abstract class Device
↓
new Device() ❌
~~~

### Regra 2

Uma classe abstrata pode existir sem possuir membros abstratos.

~~~text
abstract class Device
↓
0 métodos abstratos
✅
~~~

### Regra 3

Se existir um membro abstrato, a classe também precisa ser abstrata.

~~~text
abstract GetBrand()
↓
Classe precisa ser abstract
~~~

### Regra 4

Membros abstratos não possuem implementação na classe base.

~~~csharp
public abstract string GetBrand();
~~~

### Regra 5

Classes derivadas concretas precisam implementar os membros abstratos herdados.

~~~text
Device
└── abstract GetBrand()
        │
        ▼
Laptop
└── implementação obrigatória
~~~

---

## Resumo

| Conceito | Descrição |
|---|---|
| **abstract** | Modificador utilizado em classes e membros |
| **Classe abstrata** | Classe que não pode ser instanciada diretamente |
| **Device** | Classe base abstrata do exemplo |
| **Laptop** | Classe derivada que pode ser instanciada |
| **Smartphone** | Outra classe derivada que pode ser instanciada |
| **new Device()** | Não é permitido quando Device é abstrata |
| **new Laptop()** | Continua permitido |
| **new Smartphone()** | Continua permitido |
| **Método abstrato** | Método sem implementação na classe base |
| **GetBrand()** | Exemplo de método abstrato |
| **override** | Utilizado pela classe derivada para fornecer a implementação |
| **Membro abstrato** | Obriga a classe derivada concreta a implementá-lo |
| **Propriedade abstrata** | Também precisa ser implementada pelas classes derivadas |
| **Classe abstrata sem membros abstratos** | É permitida |
| **Membro abstrato em classe comum** | Não é permitido |

---

## Visão Geral

~~~text
                 Device
               abstract
                  │
          ┌───────┴────────┐
          │                │
          ▼                ▼
       Laptop          Smartphone
          │                │
          ▼                ▼
      new ✅             new ✅
~~~

Enquanto:

~~~text
new Device()
     ❌
~~~

### Método abstrato

~~~text
              Device
     abstract GetBrand()
                │
        ┌───────┴────────┐
        ▼                ▼
     Laptop          Smartphone
        │                │
        ▼                ▼
     Apple            Samsung
~~~

> **Em resumo:** uma classe marcada com `abstract` não pode ser instanciada diretamente e normalmente representa um conceito base para outras classes. Já um método ou propriedade abstrata funciona como uma obrigação para as classes derivadas: a classe base declara que aquele membro precisa existir, mas deixa a implementação para as classes filhas. Uma classe abstrata pode existir sem membros abstratos, mas qualquer classe que declare um membro abstrato também precisa ser abstrata.
