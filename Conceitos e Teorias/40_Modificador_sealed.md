# Modificador `sealed` em Herança

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [O problema: impedir que uma classe vire classe base](#o-problema-impedir-que-uma-classe-vire-classe-base)
3. [Criando uma nova classe derivada](#criando-uma-nova-classe-derivada)
4. [O modificador sealed](#o-modificador-sealed)
5. [O que sealed faz](#o-que-sealed-faz)
6. [Exemplo com Laptop](#exemplo-com-laptop)
7. [Classe final na hierarquia](#classe-final-na-hierarquia)
8. [Quando sealed pode ser útil](#quando-sealed-pode-ser-útil)
9. [Resumo](#resumo)

---

## Contexto da aula

Nesta aula, o foco continua sendo herança entre classes.

A pergunta principal é:

> Existe uma forma de impedir que uma classe seja utilizada como classe base?

Ou seja, imagine que já temos:

~~~text
Device
  │
  ▼
Laptop
~~~

Até esse momento, nada impede que outra classe herde de:

~~~text
Laptop
~~~

---

## O problema: impedir que uma classe vire classe base

Sem nenhuma restrição, podemos criar:

~~~csharp
public class ClassA : Laptop
{
}
~~~

Nesse caso, a hierarquia fica:

~~~text
Device
  │
  ▼
Laptop
  │
  ▼
ClassA
~~~

Portanto:

~~~text
Laptop
↓
Também está funcionando como classe base
~~~

Se isso não fizer sentido para a regra definida no projeto, precisamos bloquear essa possibilidade.

---

## Criando uma nova classe derivada

Exemplo apresentado na aula:

~~~csharp
public class ClassA : Laptop
{
}
~~~

Isso é permitido enquanto `Laptop` for uma classe comum.

A nova classe poderia inclusive trabalhar com membros herdados e sobrescritos, conforme os conceitos estudados anteriormente.

---

## O modificador sealed

Para impedir que uma classe seja herdada, utilizamos:

~~~csharp
sealed
~~~

Exemplo:

~~~csharp
public sealed class Laptop : Device
{
}
~~~

Agora:

~~~csharp
public class ClassA : Laptop
{
}
~~~

gera erro.

---

## O que sealed faz

O modificador:

~~~csharp
sealed
~~~

indica que aquela classe não pode servir como classe base para outra classe.

Conceitualmente:

~~~text
Device
  │
  ▼
Laptop
(sealed)
  │
  X
Não pode possuir classe filha
~~~

A aula utiliza a ideia de:

~~~text
sealed
↓
lacrado
~~~

Ou seja, aquela classe está “fechada” para novas derivações.

---

## Exemplo com Laptop

Antes:

~~~csharp
public class Laptop : Device
{
}
~~~

Permitido:

~~~csharp
public class ClassA : Laptop
{
}
~~~

Depois:

~~~csharp
public sealed class Laptop : Device
{
}
~~~

Não permitido:

~~~csharp
public class ClassA : Laptop
{
}
~~~

---

## Classe final na hierarquia

Uma classe marcada como `sealed` pode continuar herdando de outra classe.

Exemplo:

~~~csharp
public sealed class Laptop : Device
{
}
~~~

Isso significa:

~~~text
Laptop
↓
Pode ser classe filha de Device
~~~

Mas:

~~~text
Laptop
↓
Não pode ser classe pai de outra classe
~~~

Visualmente:

~~~text
Device
  │
  ▼
Laptop
(sealed)
~~~

A hierarquia termina ali.

---

## sealed e herança

Podemos pensar assim:

~~~text
Classe comum
↓
Pode ser herdada

Classe sealed
↓
Não pode ser herdada
~~~

Exemplo:

~~~text
Device
↓
classe base

Laptop
↓
classe derivada

sealed Laptop
↓
fim da cadeia de herança
~~~

---

## Quando sealed pode ser útil

A aula menciona situações em que pode ser desejável impedir extensão por herança.

Exemplos citados:

~~~text
Classes de repositório
Classes de regra de negócio
~~~

A ideia é evitar que outras classes:

- herdem daquela implementação;
- sobrescrevam comportamentos;
- estendam a classe por meio de herança.

Nesse caso, a classe é tratada como uma classe final dentro daquela hierarquia.

---

## Comparação com os conceitos anteriores

### abstract

~~~text
abstract class
↓
não pode ser instanciada diretamente
↓
foi criada para servir como base
~~~

### sealed

~~~text
sealed class
↓
pode ser instanciada normalmente
↓
não pode servir como base
~~~

Podemos visualizar:

~~~text
abstract
↓
abre espaço para herança
↓
não permite new direto


sealed
↓
fecha a herança
↓
permite new da própria classe
~~~

---

## Exemplo conceitual

~~~csharp
public abstract class Device
{
}
~~~

~~~csharp
public sealed class Laptop : Device
{
}
~~~

Permitido:

~~~csharp
var laptop = new Laptop();
~~~

Não permitido:

~~~csharp
var device = new Device();
~~~

Também não permitido:

~~~csharp
public class ClassA : Laptop
{
}
~~~

---

## Resumo

| Conceito | Descrição |
|---|---|
| **sealed** | Impede que uma classe seja herdada |
| **Classe base** | Classe utilizada como origem de uma herança |
| **Classe derivada** | Classe que herda de outra |
| **Laptop : Device** | Laptop continua podendo herdar de Device |
| **sealed Laptop** | Laptop não pode ter classes filhas |
| **ClassA : Laptop** | Passa a gerar erro quando Laptop é sealed |
| **Classe final** | Ideia de uma classe que encerra a hierarquia de herança |
| **abstract** | Impede instanciação direta e permite uso como base |
| **sealed** | Permite instanciação, mas bloqueia novas derivações |

---

## Visão Geral

~~~text
           Device
             │
             ▼
          Laptop
          sealed
             │
             X
       não pode herdar
             │
          ClassA
~~~

### Regra principal

~~~text
sealed class
↓
não pode ter classes derivadas
~~~

Mas:

~~~text
sealed class Laptop : Device
↓
Laptop ainda pode ser filha de Device
~~~

> **Em resumo:** o modificador `sealed` é utilizado quando queremos impedir que uma classe seja utilizada como classe base. Ela ainda pode herdar de outra classe e pode ser instanciada normalmente, mas nenhuma nova classe poderá derivar dela. No exemplo da aula, marcar `Laptop` como `sealed` impede que uma classe como `ClassA` herde de `Laptop`.
