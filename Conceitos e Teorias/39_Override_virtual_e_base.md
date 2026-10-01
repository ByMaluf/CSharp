# `override`, `virtual` e `base` em Herança

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Relembrando métodos abstratos](#relembrando-métodos-abstratos)
3. [O papel do override](#o-papel-do-override)
4. [Sobrescrevendo um método abstrato](#sobrescrevendo-um-método-abstrato)
5. [override não serve apenas para métodos abstratos](#override-não-serve-apenas-para-métodos-abstratos)
6. [Criando um método comum na classe base](#criando-um-método-comum-na-classe-base)
7. [O modificador virtual](#o-modificador-virtual)
8. [Sobrescrevendo um método virtual](#sobrescrevendo-um-método-virtual)
9. [Quando a classe derivada não sobrescreve](#quando-a-classe-derivada-não-sobrescreve)
10. [A palavra-chave base](#a-palavra-chave-base)
11. [Diferença entre abstract e virtual](#diferença-entre-abstract-e-virtual)
12. [Resumo](#resumo)

---

## Contexto da aula

Nesta aula, continuamos estudando herança.

Na aula anterior, vimos que um membro abstrato obriga a classe derivada a fornecer uma implementação.

Agora o foco passa para três palavras importantes:

~~~csharp
override
virtual
base
~~~

Esses recursos permitem controlar como métodos herdados podem ser reescritos pelas classes derivadas.

---

## Relembrando métodos abstratos

Na classe base `Device`, podemos ter:

~~~csharp
public abstract string GetBrand();
~~~

Esse método:

- não possui implementação;
- precisa ser implementado pelas classes derivadas concretas.

Exemplo:

~~~text
Device
│
└── abstract GetBrand()
        │
        ├───────────────┐
        ▼               ▼
     Laptop         Smartphone
~~~

---

## O papel do override

Quando uma classe derivada implementa um membro abstrato, aparece:

~~~csharp
override
~~~

Exemplo:

~~~csharp
public override string GetBrand()
{
    return "Apple";
}
~~~

O `override` indica que o método da classe derivada está sobrescrevendo um membro definido na classe base.

No exemplo:

~~~text
Device
↓
define GetBrand()

Laptop
↓
sobrescreve GetBrand()
~~~

---

## Sobrescrevendo um método abstrato

Classe base:

~~~csharp
public abstract class Device
{
    public abstract string GetBrand();
}
~~~

Classe derivada:

~~~csharp
public class Laptop : Device
{
    public override string GetBrand()
    {
        return "Apple";
    }
}
~~~

Se removermos:

~~~csharp
override
~~~

a classe deixa de ser reconhecida como implementação daquele membro abstrato.

A ideia principal é:

~~~text
abstract
↓
define a obrigação

override
↓
fornece a implementação
~~~

---

## override não serve apenas para métodos abstratos

A aula mostra que `override` não é exclusivo de métodos abstratos.

Também é possível sobrescrever métodos que já possuem implementação na classe base.

Para isso, a classe base precisa permitir essa sobrescrita.

Essa permissão é dada através de:

~~~csharp
virtual
~~~

---

## Criando um método comum na classe base

Na classe `Device`, podemos criar:

~~~csharp
public string Hello()
{
    return "Hello World";
}
~~~

Como o método é público, classes derivadas conseguem herdá-lo.

Exemplo:

~~~csharp
var laptop = new Laptop();

var text = laptop.Hello();
~~~

Resultado:

~~~text
Hello World
~~~

Mas, dessa forma, ainda não foi dada permissão para sobrescrever esse método.

---

## O modificador virtual

Para permitir que classes derivadas reescrevam um método já implementado, utilizamos:

~~~csharp
virtual
~~~

Exemplo:

~~~csharp
public virtual string Hello()
{
    return "Hello World";
}
~~~

Agora a classe base está dizendo:

~~~text
Este método já possui implementação
+
Classes derivadas podem sobrescrevê-lo
~~~

---

## Sobrescrevendo um método virtual

Na classe `Laptop`, podemos escrever:

~~~csharp
public override string Hello()
{
    return "Hello Ellison";
}
~~~

Agora:

~~~text
Device.Hello()
↓
"Hello World"

Laptop.Hello()
↓
"Hello Ellison"
~~~

O método foi reescrito na classe derivada.

---

## Fluxo com Laptop

Se criarmos:

~~~csharp
var laptop = new Laptop();
~~~

e chamarmos:

~~~csharp
laptop.Hello();
~~~

o método utilizado será o definido em:

~~~text
Laptop
~~~

Resultado:

~~~text
Hello Ellison
~~~

Fluxo:

~~~text
Laptop
  │
  ▼
Hello()
  │
  ▼
override
  │
  ▼
"Hello Ellison"
~~~

---

## Quando a classe derivada não sobrescreve

A classe `Smartphone` não é obrigada a sobrescrever um método `virtual`.

Se ela não implementar:

~~~csharp
override string Hello()
~~~

então continuará utilizando a implementação herdada da classe base.

Exemplo:

~~~csharp
var smartphone = new Smartphone();

var text = smartphone.Hello();
~~~

Resultado:

~~~text
Hello World
~~~

Fluxo:

~~~text
Smartphone
    │
    ▼
não possui override
    │
    ▼
usa Device.Hello()
    │
    ▼
"Hello World"
~~~

---

## A diferença prática do virtual

O `virtual` significa:

~~~text
A classe derivada PODE sobrescrever
~~~

Mas não significa:

~~~text
A classe derivada DEVE sobrescrever
~~~

Por isso:

~~~text
Laptop
↓
escolhe sobrescrever Hello()

Smartphone
↓
escolhe manter a implementação original
~~~

---

## A palavra-chave base

Ao gerar uma sobrescrita, podemos encontrar algo semelhante a:

~~~csharp
public override string Hello()
{
    return base.Hello();
}
~~~

A palavra:

~~~csharp
base
~~~

representa a classe base.

Então:

~~~csharp
base.Hello()
~~~

significa:

~~~text
Chamar o método Hello() definido na classe pai
~~~

---

## Exemplo com base

Classe base:

~~~csharp
public virtual string Hello()
{
    return "Hello World";
}
~~~

Classe derivada:

~~~csharp
public override string Hello()
{
    return base.Hello();
}
~~~

Nesse caso, apesar de existir uma sobrescrita, o método continua utilizando a implementação da classe base.

Resultado:

~~~text
Hello World
~~~

---

## Reescrevendo completamente o método

Se quisermos uma implementação diferente, podemos remover:

~~~csharp
base.Hello()
~~~

e escrever:

~~~csharp
public override string Hello()
{
    return "Hello Ellison";
}
~~~

Agora a classe derivada possui seu próprio comportamento.

---

## abstract x virtual

Os dois permitem trabalhar com `override`, mas possuem comportamentos diferentes.

### abstract

Exemplo:

~~~csharp
public abstract string GetBrand();
~~~

Características:

~~~text
Não possui implementação
↓
Classe derivada concreta precisa implementar
↓
Usa override
~~~

---

### virtual

Exemplo:

~~~csharp
public virtual string Hello()
{
    return "Hello World";
}
~~~

Características:

~~~text
Já possui implementação
↓
Classe derivada pode manter essa implementação
ou
sobrescrevê-la
↓
override é opcional
~~~

---

## Comparação visual

~~~text
ABSTRACT
│
├── não possui implementação
├── obriga implementação nas derivadas concretas
└── usa override


VIRTUAL
│
├── possui implementação
├── permite sobrescrita
└── override é opcional
~~~

---

## override

O `override` é utilizado na classe derivada para dizer:

~~~text
Estou sobrescrevendo um membro herdado
~~~

Exemplo com método abstrato:

~~~csharp
public override string GetBrand()
{
    return "Apple";
}
~~~

Exemplo com método virtual:

~~~csharp
public override string Hello()
{
    return "Hello Ellison";
}
~~~

---

## base

O `base` permite acessar a implementação da classe pai.

Exemplo:

~~~csharp
base.Hello();
~~~

Podemos visualizar assim:

~~~text
Laptop
  │
  │ base
  ▼
Device
  │
  ▼
Hello()
~~~

---

## Estrutura da aula

~~~text
Device
│
├── abstract GetBrand()
│        │
│        ├── Laptop override → "Apple"
│        └── Smartphone override → "Samsung"
│
└── virtual Hello()
         │
         ├── Laptop override → "Hello Ellison"
         │
         └── Smartphone
                │
                └── usa Device.Hello()
                    → "Hello World"
~~~

---

## Regras importantes

### Regra 1

Métodos abstratos precisam ser implementados pelas classes derivadas concretas.

~~~text
abstract
↓
override obrigatório
~~~

### Regra 2

Métodos virtuais já possuem implementação.

~~~text
virtual
↓
override opcional
~~~

### Regra 3

Sem `virtual`, o método comum da classe base não aparece como opção de sobrescrita da forma demonstrada na aula.

### Regra 4

`override` identifica que o membro da classe derivada está reescrevendo um membro da classe base.

### Regra 5

`base` permite chamar a implementação da classe pai.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **override** | Indica que um membro herdado está sendo sobrescrito na classe derivada |
| **abstract** | Define um membro sem implementação e obriga as derivadas concretas a implementá-lo |
| **virtual** | Define um membro com implementação que pode ser sobrescrito |
| **base** | Referência utilizada para acessar membros da classe base |
| **GetBrand()** | Exemplo de método abstrato |
| **Hello()** | Exemplo de método virtual |
| **Laptop** | Sobrescreve Hello() no exemplo |
| **Smartphone** | Mantém a implementação herdada de Hello() |
| **base.Hello()** | Chama a implementação de Hello() existente na classe base |
| **Método abstrato** | Precisa ser sobrescrito pelas classes derivadas concretas |
| **Método virtual** | Pode ou não ser sobrescrito |

---

## Visão Geral

~~~text
                    Device
                      │
          ┌───────────┴────────────┐
          │                        │
          ▼                        ▼
 abstract GetBrand()         virtual Hello()
          │                        │
     ┌────┴────┐              ┌────┴────┐
     ▼         ▼              ▼         ▼
  Laptop   Smartphone      Laptop   Smartphone
     │         │              │         │
 override   override       override    herda
     │         │              │         │
 "Apple"  "Samsung"   "Hello Ellison" "Hello World"
~~~

### Regra principal

~~~text
abstract
↓
DEVE sobrescrever

virtual
↓
PODE sobrescrever

override
↓
reescreve o membro herdado

base
↓
acessa a implementação da classe pai
~~~

> **Em resumo:** `override` é utilizado quando uma classe derivada reescreve um membro herdado. Métodos `abstract` exigem essa implementação, enquanto métodos `virtual` já possuem um comportamento padrão e apenas permitem que a classe filha o substitua se quiser. Quando a classe derivada ainda precisa utilizar a implementação original da classe pai, pode acessá-la através da palavra reservada `base`.
