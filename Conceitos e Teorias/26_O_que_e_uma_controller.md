# Controllers

## 📋 Índice

1. [O que é um Controller?](#o-que-é-um-controller)
2. [Qual a relação entre Controller e Endpoint?](#qual-a-relação-entre-controller-e-endpoint)
3. [Como organizar os Controllers?](#como-organizar-os-controllers)
4. [Criando um Controller no Visual Studio](#criando-um-controller-no-visual-studio)
5. [Estrutura básica de um Controller](#estrutura-básica-de-um-controller)
6. [Atributo ApiController](#atributo-apicontroller)
7. [Herança de ControllerBase](#herança-de-controllerbase)
8. [Atributo Route](#atributo-route)
9. [Como a rota é formada?](#como-a-rota-é-formada)
10. [Personalizando a rota](#personalizando-a-rota)
11. [Resumo](#resumo)

---

## O que é um Controller?

Em uma **ASP.NET Core Web API**, um **Controller** é uma classe especial responsável por **agrupar endpoints relacionados a um mesmo contexto ou recurso da aplicação**.

Os Controllers normalmente ficam dentro da pasta:

```text
Controllers/
```

Exemplo:

```text
Projeto
│
├── Controllers
│   ├── UserController.cs
│   ├── ProductController.cs
│   └── OrderController.cs
│
├── Program.cs
└── ...
```

Dentro de um Controller são criados métodos que representam os **endpoints** disponibilizados pela API.

Por exemplo, um `UserController` pode possuir funcionalidades para:

- cadastrar usuário;
- consultar usuário;
- atualizar usuário;
- alterar senha;
- excluir usuário.

---

## Qual a relação entre Controller e Endpoint?

Um **Controller** funciona como um agrupador de endpoints que possuem relação entre si.

Podemos visualizar assim:

```text
UserController
│
├── Cadastrar usuário
├── Consultar usuário
├── Atualizar usuário
├── Alterar senha
└── Excluir usuário
```

Cada uma dessas funcionalidades poderá ser disponibilizada através de um **endpoint**.

Portanto:

```text
Controller
    ↓
Agrupa funcionalidades relacionadas
    ↓
Endpoints
```

Um Controller pode possuir vários endpoints.

> O **Controller define o contexto**, enquanto os **Endpoints representam operações específicas dentro daquele contexto**.

---

## Como organizar os Controllers?

Os endpoints devem ser agrupados de acordo com sua responsabilidade.

Por exemplo:

### Usuários

```text
UserController
│
├── Cadastrar usuário
├── Consultar usuário
├── Atualizar usuário
├── Alterar senha
└── Excluir usuário
```

### Produtos

```text
ProductController
│
├── Cadastrar produto
├── Consultar produto
├── Atualizar produto
└── Excluir produto
```

### Pedidos

```text
OrderController
│
├── Criar pedido
├── Consultar pedido
├── Atualizar pedido
└── Cancelar pedido
```

Assim, cada Controller possui um **contexto específico**.

Isso facilita:

- organização do código;
- manutenção;
- localização das funcionalidades;
- separação de responsabilidades.

---

## Criando um Controller no Visual Studio

No Visual Studio, podemos criar um Controller através da pasta:

```text
Controllers
```

Utilizando:

```text
Botão direito
    ↓
Add
    ↓
Controller
```

Porém, existe um ponto importante.

O Visual Studio também permite criar Controllers destinados a aplicações **MVC**, utilizadas para desenvolvimento de aplicações Web.

Como estamos trabalhando com uma **Web API**, devemos selecionar a opção destinada a APIs.

Exemplo:

```text
Add Controller
      ↓
     API
      ↓
API Controller - Empty
```

> Para este projeto, devemos utilizar um **API Controller**, e não um Controller MVC destinado a páginas Web.

---

### Nomeando o Controller

Ao criar um Controller, devemos utilizar um nome que represente seu contexto.

Por exemplo:

```text
UserController
ProductController
OrderController
PaymentController
```

A palavra:

```text
Controller
```

deve ser mantida no nome da classe.

Exemplo:

```csharp
public class UserController
{
}
```

---

## Estrutura básica de um Controller

Um Controller pode possuir uma estrutura semelhante a:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
}
```

Temos três elementos importantes:

```text
[ApiController]
       ↓
Identifica a classe como um Controller de API

[Route("api/[controller]")]
       ↓
Define a rota utilizada para acessar o Controller

ControllerBase
       ↓
Classe base fornecida pelo ASP.NET Core
```

---

## Atributo ApiController

O atributo:

```csharp
[ApiController]
```

indica que aquela classe será utilizada como um **Controller de API**.

Exemplo:

```csharp
[ApiController]
public class UserController : ControllerBase
{
}
```

Na estrutura apresentada na aula, ele identifica a classe como um Controller responsável por receber requisições da API.

Podemos visualizar assim:

```text
Classe C#
   ↓
[ApiController]
   ↓
Controller da API
```

---

## Herança de ControllerBase

O Controller também possui:

```csharp
: ControllerBase
```

Exemplo:

```csharp
public class UserController : ControllerBase
{
}
```

Os dois pontos:

```csharp
:
```

representam **herança** em C#.

Portanto:

```text
UserController
      ↓
   herda de
      ↓
ControllerBase
```

O `ControllerBase` é uma classe fornecida pelo ASP.NET Core que disponibiliza funcionalidades úteis para a construção dos endpoints.

Por meio dessa herança, o `UserController` poderá utilizar métodos e propriedades já existentes no `ControllerBase`.

> O conceito de **herança** será aprofundado posteriormente. Neste momento, basta compreender que `UserController` está aproveitando funcionalidades fornecidas por `ControllerBase`.

---

## Atributo Route

O atributo:

```csharp
[Route("api/[controller]")]
```

define parte do endereço utilizado para acessar os endpoints daquele Controller.

Exemplo:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
}
```

Nesse caso:

```text
api
 +
[controller]
```

O `[controller]` é substituído automaticamente pelo nome do Controller **sem a palavra `Controller`**.

Portanto:

```text
UserController
      ↓
    User
```

Assim:

```csharp
[Route("api/[controller]")]
```

no `UserController` representa:

```text
api/user
```

---

## Como a rota é formada?

Para acessar um endpoint, precisamos combinar o **endereço base da aplicação** com a **rota do Controller**.

Imagine que no ambiente local nossa API esteja executando em:

```text
https://localhost:7081
```

Esse é o endereço base:

```text
https://localhost:7081
```

Nosso Controller possui:

```csharp
[Route("api/[controller]")]
```

e se chama:

```text
UserController
```

O `[controller]` será substituído por:

```text
User
```

Portanto:

```text
https://localhost:7081
          +
       /api/user
          ↓
https://localhost:7081/api/user
```

Podemos visualizar:

```text
https://localhost:7081/api/user
│                         │
│                         └── Rota do Controller
│
└── Endereço base da API
```

---

### Em produção

No ambiente de desenvolvimento podemos possuir:

```text
https://localhost:7081
```

Em produção, o endereço base poderia ser um domínio real:

```text
https://minhaapi.com.br
```

A rota continuaria sendo adicionada ao endereço base:

```text
https://minhaapi.com.br/api/user
```

Portanto:

```text
DESENVOLVIMENTO

https://localhost:7081/api/user


PRODUÇÃO

https://minhaapi.com.br/api/user
```

O endereço base muda, mas a estrutura da rota pode permanecer a mesma.

---

## Controller + Endpoint + Método HTTP

O Controller define o contexto geral, enquanto cada endpoint define uma funcionalidade.

Por exemplo:

```text
UserController
      ↓
/api/user
```

Dentro dele poderemos criar operações como:

```text
POST   /api/user
GET    /api/user
PUT    /api/user
DELETE /api/user
```

O método HTTP ajuda a determinar qual operação queremos executar.

Por exemplo:

```http
POST /api/user
```

poderia representar:

```text
Cadastrar usuário
```

Enquanto:

```http
DELETE /api/user
```

poderia representar:

```text
Excluir usuário
```

Assim:

```text
Endereço base
      +
Rota do Controller
      +
Método HTTP
      ↓
Operação da API
```

---

## Personalizando a rota

A rota:

```csharp
[Route("api/[controller]")]
```

não é obrigatória nesse formato específico.

Podemos definir manualmente outro caminho.

Por exemplo:

```csharp
[Route("[controller]")]
```

Nesse caso, para `UserController`, teríamos:

```text
https://localhost:7081/user
```

Também poderíamos definir diretamente:

```csharp
[Route("usuarios")]
```

Resultando em:

```text
https://localhost:7081/usuarios
```

Portanto, o atributo `[Route]` permite definir como aquele Controller será acessado.

Exemplos:

| Route | Resultado para UserController |
|---|---|
| `[Route("api/[controller]")]` | `/api/user` |
| `[Route("[controller]")]` | `/user` |
| `[Route("usuarios")]` | `/usuarios` |

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Controllers** | Pasta utilizada para organizar os Controllers da API |
| **Controller** | Classe que agrupa endpoints relacionados a um mesmo contexto |
| **Endpoint** | Funcionalidade específica disponibilizada pela API |
| **UserController** | Controller relacionado às funcionalidades de usuários |
| **[ApiController]** | Identifica a classe como um Controller de API |
| **[Route]** | Define a rota utilizada para acessar o Controller |
| **[controller]** | É substituído pelo nome do Controller sem o sufixo `Controller` |
| **ControllerBase** | Classe base que fornece funcionalidades úteis aos Controllers |
| **Herança (`:`)** | Permite que o Controller utilize funcionalidades de `ControllerBase` |
| **Endereço base** | Endereço principal onde a API está sendo executada |

---

## Visão Geral

```text
                     API
                      │
                      ▼
                 Controllers
                      │
        ┌─────────────┼─────────────┐
        │             │             │
        ▼             ▼             ▼
UserController ProductController OrderController
        │
        │
        ├── Cadastrar usuário
        ├── Consultar usuário
        ├── Atualizar usuário
        ├── Alterar senha
        └── Excluir usuário
```

Estrutura do Controller:

```text
[ApiController]
      │
      ▼
Identifica como Controller de API

[Route("api/[controller]")]
      │
      ▼
Define a rota
      │
      ▼
public class UserController : ControllerBase
             │                 │
             │                 └── Herança
             │
             └── Contexto: Usuário
```

Formação da URL:

```text
Endereço base
https://localhost:7081
          │
          │
          +

Rota
api/[controller]
          │
          ▼

Controller
UserController
          │
          ▼
        User
          │
          ▼

https://localhost:7081/api/user
```

> **Em resumo:** um **Controller** é uma classe responsável por agrupar endpoints relacionados a um mesmo contexto. O `[ApiController]` identifica a classe como um Controller de API, o `[Route]` determina seu endereço e a herança de `ControllerBase` disponibiliza funcionalidades úteis para a implementação dos endpoints.