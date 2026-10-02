# BaseController, `protected` e `abstract` em Controllers

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Compartilhando lógica entre Controllers](#compartilhando-lógica-entre-controllers)
3. [Recuperando um Header customizado](#recuperando-um-header-customizado)
4. [Reutilizando o método nos Controllers filhos](#reutilizando-o-método-nos-controllers-filhos)
5. [O problema de métodos públicos no BaseController](#o-problema-de-métodos-públicos-no-basecontroller)
6. [Resolvendo com protected](#resolvendo-com-protected)
7. [Testando o Header pelo Postman](#testando-o-header-pelo-postman)
8. [Criando um endpoint compartilhado no BaseController](#criando-um-endpoint-compartilhado-no-basecontroller)
9. [Conflito de rotas no endpoint herdado](#conflito-de-rotas-no-endpoint-herdado)
10. [Criando uma rota específica para o endpoint](#criando-uma-rota-específica-para-o-endpoint)
11. [O BaseController aparecendo como Controller próprio](#o-basecontroller-aparecendo-como-controller-próprio)
12. [Tornando o BaseController abstrato](#tornando-o-basecontroller-abstrato)
13. [Efeito do abstract no endpoint do BaseController](#efeito-do-abstract-no-endpoint-do-basecontroller)
14. [Resumo](#resumo)

---

## Contexto da aula

Depois de estudar vários conceitos de herança, a aula volta para os Controllers da API.

A ideia é utilizar herança para centralizar comportamentos comuns e evitar duplicação de código.

O cenário simulado é:

~~~text
Toda requisição
      ↓
Possui um Header customizado
      ↓
API precisa recuperar esse valor
      ↓
Vários Controllers precisam usar a mesma lógica
~~~

Em vez de repetir o mesmo código em cada Controller, essa lógica será colocada em um Controller base.

---

## Compartilhando lógica entre Controllers

Temos uma classe base semelhante a:

~~~csharp
MyFirstAPIBaseController
~~~

E Controllers derivados:

~~~text
MyFirstAPIBaseController
        │
   ┌────┴─────┐
   ▼          ▼
Device      User
Controller Controller
~~~

A ideia é criar uma função uma única vez no BaseController e permitir que os Controllers derivados a utilizem.

---

## Recuperando um Header customizado

O cenário da aula utiliza um Header chamado:

~~~text
myKey
~~~

Dentro de um Controller, temos acesso à requisição por meio de:

~~~csharp
Request
~~~

E aos Headers através de:

~~~csharp
Request.Headers
~~~

Para acessar um Header customizado:

~~~csharp
Request.Headers["myKey"]
~~~

Como queremos o valor como string:

~~~csharp
Request.Headers["myKey"].ToString()
~~~

Podemos criar:

~~~csharp
protected string GetCustomKey()
{
    return Request.Headers["myKey"].ToString();
}
~~~

---

## Headers já conhecidos

A aula mostra que a coleção de Headers também possui valores comuns, como:

~~~text
Accept
Accept-Language
Authorization
~~~

Mas para um Header customizado, utilizamos o nome entre colchetes:

~~~csharp
Request.Headers["myKey"]
~~~

Fluxo:

~~~text
Request
   │
   ▼
Headers
   │
   ▼
["myKey"]
   │
   ▼
ToString()
   │
   ▼
Valor da chave
~~~

---

## Reutilizando o método nos Controllers filhos

Como os Controllers herdam do BaseController, podem utilizar:

~~~csharp
GetCustomKey()
~~~

Exemplo no `DeviceController`:

~~~csharp
var key = GetCustomKey();
~~~

Exemplo no `UserController`:

~~~csharp
var key = GetCustomKey();
~~~

Assim, a lógica de leitura do Header existe apenas uma vez.

~~~text
MyFirstAPIBaseController
│
└── GetCustomKey()
      │
      ├───────────────┐
      ▼               ▼
DeviceController  UserController
      │               │
      └── reutilizam ─┘
~~~

---

## Evitando duplicação

Sem herança, teríamos algo semelhante em vários Controllers:

~~~csharp
var key = Request.Headers["myKey"].ToString();
~~~

repetido várias vezes.

Com o BaseController:

~~~text
Código comum
     ↓
MyFirstAPIBaseController
     ↓
Controllers filhos reutilizam
~~~

Isso reduz duplicação e facilita manutenção.

---

## O problema de métodos públicos no BaseController

Inicialmente, a função foi criada como pública:

~~~csharp
public string GetCustomKey()
~~~

Ao executar a API, o Swagger apresentou erro.

Segundo a aula, isso acontece porque métodos públicos dentro de um Controller podem ser interpretados como ações/endpoints.

Então o framework encontra:

~~~text
GetCustomKey()
↓
método público
↓
interpreta como endpoint
↓
não possui HttpGet, HttpPost, HttpPut ou HttpDelete
↓
erro
~~~

---

## Por que private não resolve?

Uma possibilidade seria:

~~~csharp
private string GetCustomKey()
~~~

Isso impediria que o método fosse tratado como uma ação pública.

Porém:

~~~text
DeviceController
UserController
~~~

também perderiam o acesso.

Isso acontece porque `private` restringe o membro à própria classe.

~~~text
private
↓
somente MyFirstAPIBaseController
~~~

---

## Resolvendo com protected

A solução utilizada na aula é:

~~~csharp
protected string GetCustomKey()
~~~

Agora:

~~~text
MyFirstAPIBaseController
↓
pode acessar

DeviceController
↓
pode acessar

UserController
↓
pode acessar

Código externo
↓
não acessa como método público do Controller
~~~

Isso reutiliza exatamente o conceito de `protected` estudado anteriormente.

---

## Comparação

| Modificador | BaseController | Controllers derivados | Exposição pública |
|---|---:|---:|---:|
| `public` | ✅ | ✅ | ✅ |
| `private` | ✅ | ❌ | ❌ |
| `protected` | ✅ | ✅ | ❌ |

Para o cenário da aula:

~~~text
protected
↓
comportamento desejado
~~~

---

## Testando o Header pelo Postman

O Postman é utilizado para enviar o Header customizado.

Exemplo:

~~~text
GET /api/device
~~~

Header:

~~~text
myKey: ABC1234567
~~~

No código:

~~~csharp
var key = GetCustomKey();
~~~

O valor recuperado será:

~~~text
ABC1234567
~~~

O mesmo comportamento pode ser reutilizado em:

~~~text
/api/device
/api/user
~~~

porque ambos os Controllers herdam do mesmo BaseController.

---

## Fluxo do Header

~~~text
POSTMAN
   │
   │ Header
   │ myKey: ABC1234567
   ▼
REQUEST
   │
   ▼
Request.Headers["myKey"]
   │
   ▼
GetCustomKey()
   │
   ▼
ABC1234567
   │
   ├── DeviceController
   └── UserController
~~~

---

## Criando um endpoint compartilhado no BaseController

Depois, a aula muda o cenário.

Agora queremos criar de fato um endpoint no BaseController.

Exemplo:

~~~csharp
[HttpGet]
public IActionResult Health()
{
    return Ok("It's working");
}
~~~

A ideia é possuir um endpoint de verificação da API.

Conceitualmente:

~~~text
Health
↓
Verificar se a API está funcionando
↓
"It's working"
~~~

---

## Herança do endpoint

Como os Controllers derivados herdam do BaseController, o endpoint também passa a aparecer associado a eles.

A expectativa é algo semelhante a:

~~~text
DeviceController
└── Health

UserController
└── Health
~~~

Porém surge um problema.

---

## Conflito de rotas no endpoint herdado

Inicialmente o endpoint possui apenas:

~~~csharp
[HttpGet]
~~~

Os Controllers já possuem outros endpoints GET.

Isso gera conflito porque o endpoint herdado precisa possuir uma rota específica.

Fluxo:

~~~text
GET existente
+
GET Health herdado
+
mesma rota
↓
conflito
~~~

---

## Criando uma rota específica para o endpoint

Para evitar a colisão, é criada uma rota específica.

Exemplo:

~~~csharp
[HttpGet("health")]
public IActionResult Health()
{
    return Ok("It's working");
}
~~~

Agora temos:

~~~text
GET /api/device/health
GET /api/user/health
~~~

Cada Controller derivado passa a disponibilizar o endpoint:

~~~text
health
~~~

---

## Testando o Health

Exemplo:

~~~http
GET /api/device/health
~~~

Resposta:

~~~text
It's working
~~~

Também:

~~~http
GET /api/user/health
~~~

Resposta:

~~~text
It's working
~~~

A implementação existe apenas no BaseController, mas é reutilizada pelos Controllers derivados.

---

## O BaseController aparecendo como Controller próprio

Ao executar a aplicação, a aula mostra que o próprio:

~~~text
MyFirstAPIBaseController
~~~

também aparece no Swagger.

Ou seja, além de:

~~~text
Device
User
~~~

também existe uma rota diretamente associada ao BaseController.

Isso ocorre porque ele ainda é uma classe concreta de Controller.

---

## O problema

A intenção do BaseController é:

~~~text
Servir como classe base
~~~

e não:

~~~text
Ser utilizado diretamente como Controller
~~~

Então queremos impedir que o framework crie uma instância direta dessa classe.

Esse é exatamente o problema resolvido anteriormente com:

~~~csharp
abstract
~~~

---

## Tornando o BaseController abstrato

Podemos declarar:

~~~csharp
public abstract class MyFirstAPIBaseController : ControllerBase
{
}
~~~

A partir daí:

~~~text
new MyFirstAPIBaseController()
↓
não é permitido
~~~

A classe continua podendo ser utilizada como base por:

~~~text
DeviceController
UserController
~~~

---

## Estrutura final

~~~text
ControllerBase
      │
      ▼
MyFirstAPIBaseController
     abstract
      │
   ┌──┴────────┐
   ▼           ▼
Device       User
Controller   Controller
~~~

O BaseController existe para compartilhar comportamento, mas não deve ser instanciado diretamente.

---

## Efeito do abstract no endpoint do BaseController

Depois de marcar o BaseController como:

~~~csharp
abstract
~~~

ele deixa de aparecer como um Controller diretamente acessível no Swagger.

Mas seus endpoints herdados continuam disponíveis através dos Controllers concretos.

Exemplo:

~~~text
/api/device/health
✅

/api/user/health
✅
~~~

Enquanto a rota que representaria diretamente o BaseController deixa de estar disponível.

---

## Testando pelo Postman

A aula também testa o comportamento diretamente pelo Postman.

Antes do `abstract`:

~~~text
Endpoint do BaseController
↓
200 OK
~~~

Depois do `abstract`:

~~~text
Mesmo endereço
↓
404 Not Found
~~~

Ou seja, o endpoint direto do BaseController deixa de existir.

Mas os Controllers derivados continuam funcionando normalmente.

---

## protected + abstract

A aula combina dois conceitos importantes.

### protected

Utilizado para:

~~~text
Compartilhar métodos auxiliares
↓
Somente BaseController + derivados
~~~

Exemplo:

~~~csharp
protected string GetCustomKey()
~~~

### abstract

Utilizado para:

~~~text
Impedir instanciação direta do BaseController
↓
Classe existe apenas para herança
~~~

Exemplo:

~~~csharp
public abstract class MyFirstAPIBaseController
~~~

---

## Estrutura completa

~~~text
                    ControllerBase
                          │
                          ▼
              MyFirstAPIBaseController
                     abstract
                          │
          ┌───────────────┴───────────────┐
          ▼                               ▼
   DeviceController                UserController
          │                               │
          ├── GetCustomKey()              ├── GetCustomKey()
          │   herdado/protected           │   herdado/protected
          │                               │
          └── /health                     └── /health
              herdado                         herdado
~~~

---

## Resumo

| Conceito | Descrição |
|---|---|
| **BaseController** | Classe base utilizada para compartilhar comportamentos entre Controllers |
| **Request** | Representa a requisição HTTP atual |
| **Request.Headers** | Permite acessar os Headers da requisição |
| **myKey** | Header customizado utilizado no exemplo |
| **GetCustomKey()** | Método centralizado para recuperar o Header |
| **public** | Pode fazer o método ser exposto como ação no contexto mostrado |
| **private** | Impediria Controllers derivados de reutilizar o método |
| **protected** | Permite uso pela classe base e pelos Controllers derivados |
| **Health()** | Endpoint compartilhado criado no BaseController |
| **HttpGet("health")** | Define uma rota específica para evitar colisões |
| **/api/device/health** | Health herdado pelo DeviceController |
| **/api/user/health** | Health herdado pelo UserController |
| **abstract** | Impede instanciação direta do BaseController |
| **404 Not Found** | Resultado observado ao tentar acessar diretamente o BaseController após torná-lo abstrato |
| **Herança** | Permite compartilhar lógica e endpoints entre Controllers derivados |

---

## Visão Geral

~~~text
REQUISIÇÃO
   │
   │ Header: myKey
   ▼
Controller derivado
   │
   ▼
GetCustomKey()
   │
   ▼
BaseController
   │
   ▼
Request.Headers["myKey"]
~~~

### BaseController

~~~text
MyFirstAPIBaseController
│
├── protected GetCustomKey()
│
└── GET /health
      │
      ├───────────────┐
      ▼               ▼
DeviceController  UserController
~~~

### Com abstract

~~~text
MyFirstAPIBaseController
       abstract
          │
          X
não é instanciado diretamente
          │
     ┌────┴────┐
     ▼         ▼
  Device      User
 Controller Controller
~~~

> **Em resumo:** a herança permite concentrar comportamentos comuns em um BaseController. Métodos auxiliares que precisam ser reutilizados pelos Controllers filhos podem ser marcados como `protected`, evitando exposição pública desnecessária. Também é possível colocar um endpoint no BaseController e herdá-lo nos Controllers derivados, desde que sua rota seja específica para evitar colisões. Por fim, marcar o BaseController como `abstract` impede que ele seja utilizado diretamente como um Controller concreto, mantendo-o apenas como base para os demais Controllers.
