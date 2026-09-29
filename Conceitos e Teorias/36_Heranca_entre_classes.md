# Herança entre Classes em C#

## 📋 Índice

1. [O que é herança?](#o-que-é-herança)
2. [Classe base e classe derivada](#classe-base-e-classe-derivada)
3. [Vantagens da herança](#vantagens-da-herança)
4. [Sintaxe da herança em C#](#sintaxe-da-herança-em-c)
5. [Herança nos Controllers](#herança-nos-controllers)
6. [Criando um BaseController próprio](#criando-um-basecontroller-próprio)
7. [Reutilizando atributos com herança](#reutilizando-atributos-com-herança)
8. [Centralizando a rota dos Controllers](#centralizando-a-rota-dos-controllers)
9. [Reutilizando propriedades da classe base](#reutilizando-propriedades-da-classe-base)
10. [Herança múltipla não é permitida](#herança-múltipla-não-é-permitida)
11. [Várias classes filhas para a mesma classe base](#várias-classes-filhas-para-a-mesma-classe-base)
12. [Encadeamento de herança](#encadeamento-de-herança)
13. [Resumo](#resumo)

---

## O que é herança?

Herança é um conceito importante de Programação Orientada a Objetos.

Ela permite que uma classe reutilize características definidas em outra classe.

Essas características podem incluir:

- propriedades;
- métodos;
- atributos;
- comportamentos herdáveis, de acordo com os modificadores de acesso.

Conceitualmente:

~~~text
Classe Base
   │
   ├── Propriedades
   ├── Métodos
   └── Atributos
   │
   ▼
Classe Derivada
~~~

A ideia central é evitar duplicação de código e reaproveitar comportamentos comuns.

---

## Classe base e classe derivada

Para existir herança, precisamos de pelo menos duas classes.

Uma será a:

~~~text
Classe Base
~~~

também chamada de:

~~~text
Classe Pai
~~~

A outra será a:

~~~text
Classe Derivada
~~~

também chamada de:

~~~text
Classe Filha
~~~

Exemplo:

~~~text
Classe Base
    │
    ▼
Classe Derivada
~~~

Ou:

~~~text
Classe Pai
   │
   ▼
Classe Filha
~~~

---

## Vantagens da herança

Uma das principais vantagens é evitar duplicação de código.

Imagine que várias classes precisam possuir os mesmos atributos ou propriedades.

Sem herança:

~~~text
Classe A
├── propriedade X
└── propriedade Y

Classe B
├── propriedade X
└── propriedade Y

Classe C
├── propriedade X
└── propriedade Y
~~~

Temos repetição.

Com herança:

~~~text
Classe Base
├── propriedade X
└── propriedade Y
      │
      ├─────────┐
      ▼         ▼
   Classe A   Classe B
~~~

As classes filhas reutilizam aquilo que foi definido na classe base.

---

## Sintaxe da herança em C#

Em C#, utilizamos:

~~~text
:
~~~

dois pontos para indicar herança.

Exemplo:

~~~csharp
public class ClasseFilha : ClasseBase
{
}
~~~

Isso significa:

~~~text
ClasseFilha
    │
    └── herda de
           │
           ▼
       ClasseBase
~~~

Na aula, os Controllers já utilizavam herança sem que esse conceito tivesse sido aprofundado.

Por exemplo:

~~~csharp
public class UserController : ControllerBase
{
}
~~~

Nesse caso:

~~~text
UserController
     ↓
Classe derivada

ControllerBase
     ↓
Classe base
~~~

---

## Herança nos Controllers

Foi criado um segundo Controller chamado:

~~~text
DeviceController
~~~

Assim, a API passa a ter:

~~~text
Controllers
│
├── UserController
└── DeviceController
~~~

Os dois originalmente herdam de:

~~~csharp
ControllerBase
~~~

Exemplo:

~~~csharp
public class UserController : ControllerBase
{
}
~~~

e:

~~~csharp
public class DeviceController : ControllerBase
{
}
~~~

Isso já representa herança.

---

## Criando um BaseController próprio

A aula propõe criar uma classe intermediária para concentrar configurações comuns aos Controllers.

Exemplo de nome:

~~~text
MyFirstAPIBaseController
~~~

Essa classe continua herdando de:

~~~csharp
ControllerBase
~~~

Exemplo:

~~~csharp
public class MyFirstAPIBaseController : ControllerBase
{
}
~~~

Agora podemos fazer os demais Controllers herdarem dela.

~~~csharp
public class UserController : MyFirstAPIBaseController
{
}
~~~

~~~csharp
public class DeviceController : MyFirstAPIBaseController
{
}
~~~

A estrutura passa a ser:

~~~text
ControllerBase
      │
      ▼
MyFirstAPIBaseController
      │
   ┌──┴───┐
   ▼      ▼
User    Device
Controller Controller
~~~

---

## Reutilizando atributos com herança

Os Controllers possuíam atributos repetidos.

Por exemplo:

~~~csharp
[ApiController]
[Route("api/[controller]")]
~~~

Se cada Controller possuir esses atributos individualmente:

~~~text
UserController
├── [ApiController]
└── [Route(...)]

DeviceController
├── [ApiController]
└── [Route(...)]
~~~

temos duplicação.

A ideia da aula é mover os atributos comuns para o Controller base criado pela aplicação.

Exemplo:

~~~csharp
[ApiController]
[Route("api/[controller]")]
public class MyFirstAPIBaseController : ControllerBase
{
}
~~~

Depois:

~~~csharp
public class UserController : MyFirstAPIBaseController
{
}
~~~

~~~csharp
public class DeviceController : MyFirstAPIBaseController
{
}
~~~

Assim, os Controllers derivados reutilizam essa configuração.

---

## Centralizando a rota dos Controllers

Uma vantagem prática dessa estrutura é centralizar a rota.

Antes:

~~~csharp
[Route("api/[controller]")]
~~~

precisava estar repetido em vários Controllers.

Imagine possuir:

~~~text
5 Controllers
10 Controllers
15 Controllers
~~~

Se quiséssemos alterar:

~~~text
api
~~~

para:

~~~text
myProject
~~~

seria necessário modificar todos os Controllers.

Com uma classe base, fazemos a alteração em apenas um lugar.

Por exemplo:

~~~csharp
[Route("myProject/[controller]")]
public class MyFirstAPIBaseController : ControllerBase
{
}
~~~

Então:

~~~text
UserController
↓
/myProject/user

DeviceController
↓
/myProject/device
~~~

A mudança é propagada para os Controllers derivados.

---

## Manutenção e duplicação de código

Mesmo uma duplicação pequena continua sendo duplicação.

Exemplo:

~~~text
[ApiController]
[Route("api/[controller]")]
~~~

repetido em vários arquivos.

Centralizando:

~~~text
MyFirstAPIBaseController
        │
        ▼
Configuração comum
        │
    ┌───┴────┐
    ▼        ▼
 User      Device
~~~

Isso melhora a manutenção.

Se houver uma alteração futura:

~~~text
Mudar em 1 lugar
       ↓
Refletir nas classes derivadas
~~~

---

## Reutilizando propriedades da classe base

A aula também demonstra herança utilizando uma propriedade.

Na classe base:

~~~csharp
public string Author { get; set; } = "Wellison Arley";
~~~

Como o DeviceController herda de MyFirstAPIBaseController, ele pode acessar essa propriedade.

Exemplo:

~~~csharp
public class DeviceController : MyFirstAPIBaseController
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(Author);
    }
}
~~~

Fluxo:

~~~text
MyFirstAPIBaseController
│
└── Author
      │
      ▼
DeviceController
      │
      ▼
Pode utilizar Author
~~~

---

## Alterando uma propriedade herdada

A classe derivada também pode alterar a propriedade herdada, dependendo de sua acessibilidade.

Exemplo apresentado:

~~~csharp
Author = "Maria";
~~~

Depois:

~~~csharp
return Ok(Author);
~~~

O resultado passa a utilizar o novo valor.

Conceitualmente:

~~~text
Classe Base
Author = "Wellison Arley"
        │
        ▼
Classe Derivada
Author = "Maria"
        │
        ▼
Retorno = "Maria"
~~~

---

## Modificadores de acesso

A aula lembra que nem todas as propriedades e métodos de uma classe base estarão necessariamente disponíveis para a classe filha.

Isso depende dos:

~~~text
Modificadores de acesso
~~~

Esse assunto será aprofundado em outra aula.

Portanto, herança não significa automaticamente acesso irrestrito a tudo que existe na classe base.

---

## Herança múltipla não é permitida

C# não permite que uma classe herde diretamente de várias classes ao mesmo tempo.

Exemplo inválido:

~~~csharp
public class DeviceController : MyFirstAPIBaseController, User
{
}
~~~

A intenção seria:

~~~text
DeviceController
    │
    ├── MyFirstAPIBaseController
    └── User
~~~

Isso não é permitido para classes em C#.

A regra apresentada é:

~~~text
Uma classe derivada
        ↓
Pode ter apenas UMA classe base direta
~~~

---

## O que significa não ter herança múltipla?

Significa que isto não é permitido:

~~~text
Classe A
  │
  ├── herda de Classe B
  └── herda de Classe C
~~~

Ou seja:

~~~csharp
class A : B, C
~~~

não é válido quando B e C são classes base.

---

## Várias classes filhas para a mesma classe base

Embora uma classe só possa ter uma classe base direta, uma classe base pode possuir várias classes derivadas.

Isso é perfeitamente válido:

~~~text
              Classe Base
             /     |      \
            /      |       \
           ▼       ▼        ▼
       Classe A Classe B Classe C
~~~

No exemplo da aula:

~~~text
MyFirstAPIBaseController
        │
   ┌────┴────┐
   ▼         ▼
User       Device
Controller Controller
~~~

Isso não representa herança múltipla.

---

## Encadeamento de herança

Também é possível ter uma cadeia de herança.

Exemplo:

~~~text
Classe C
   │
   ▼
Classe B
   │
   ▼
Classe A
~~~

Nesse caso:

~~~text
Classe A herda de B

Classe B herda de C
~~~

A classe A pode reutilizar membros herdáveis vindos tanto de B quanto de C.

---

## Encadeamento nos Controllers

Na aula, temos exatamente essa situação:

~~~text
ControllerBase
      │
      ▼
MyFirstAPIBaseController
      │
      ▼
UserController
~~~

Podemos pensar em:

~~~text
Classe C
↓
ControllerBase

Classe B
↓
MyFirstAPIBaseController

Classe A
↓
UserController
~~~

Assim:

~~~text
UserController
   │
   ├── membros próprios
   ├── membros herdados de MyFirstAPIBaseController
   └── membros herdáveis vindos de ControllerBase
~~~

---

## Herança encadeada x herança múltipla

Esses dois conceitos não devem ser confundidos.

### Herança encadeada

Permitida:

~~~text
A → B → C
~~~

Ou:

~~~csharp
class C
{
}

class B : C
{
}

class A : B
{
}
~~~

---

### Herança múltipla

Não permitida entre classes:

~~~text
   B
  ↗
 A
  ↘
   C
~~~

Ou:

~~~csharp
class A : B, C
{
}
~~~

---

## Relação completa da aula

~~~text
ControllerBase
      │
      ▼
MyFirstAPIBaseController
      │
      ├───────────────┐
      ▼               ▼
UserController   DeviceController
~~~

A classe intermediária concentra:

~~~text
Atributos comuns
Propriedades comuns
Configurações comuns
~~~

e os Controllers derivados reutilizam essas informações.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Herança** | Permite que uma classe reutilize membros de outra classe |
| **Classe base** | Classe que fornece membros para outras classes |
| **Classe pai** | Outro nome para classe base |
| **Classe derivada** | Classe que herda de outra |
| **Classe filha** | Outro nome para classe derivada |
| **:** | Sintaxe utilizada para herança em C# |
| **ControllerBase** | Classe base utilizada pelos Controllers da API |
| **MyFirstAPIBaseController** | Classe intermediária criada para centralizar comportamentos comuns |
| **Reutilização** | Uma das principais vantagens da herança |
| **Duplicação de código** | Pode ser reduzida ao mover membros comuns para uma classe base |
| **Route** | Pode ser centralizado na classe base |
| **ApiController** | Também pode ser compartilhado através da estrutura de herança mostrada |
| **Propriedade herdada** | Pode ser utilizada pela classe derivada conforme sua acessibilidade |
| **Herança múltipla** | Uma classe não pode possuir várias classes base diretas em C# |
| **Várias classes filhas** | Uma classe base pode possuir várias classes derivadas |
| **Encadeamento de herança** | Uma classe pode herdar de outra que também herda de uma terceira |

---

## Visão Geral

~~~text
                   HERANÇA
                      │
            ┌─────────┴─────────┐
            │                   │
            ▼                   ▼
       Classe Base        Classe Derivada
            │                   │
            ├── propriedades ───┤
            ├── métodos ────────┤
            └── atributos ──────┘
~~~

### Exemplo da API

~~~text
ControllerBase
      │
      ▼
MyFirstAPIBaseController
      │
   ┌──┴──────────┐
   ▼             ▼
UserController DeviceController
~~~

### Regra importante

~~~text
Uma classe derivada
       │
       ▼
1 classe base direta
~~~

Mas:

~~~text
Uma classe base
       │
       ▼
Pode ter várias classes derivadas
~~~

E também:

~~~text
A → B → C
~~~

é permitido como encadeamento de herança.

> **Em resumo:** herança permite reutilizar código entre classes relacionadas. Em C#, uma classe derivada possui apenas uma classe base direta, mas uma classe base pode ter várias classes filhas. Também é possível formar uma cadeia de herança. Na aula, esse conceito foi aplicado aos Controllers para criar um BaseController próprio, centralizar atributos e rotas comuns e reduzir duplicação de código.
