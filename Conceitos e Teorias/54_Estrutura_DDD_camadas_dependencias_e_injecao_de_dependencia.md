# Estrutura DDD: Camadas, Dependências e Injeção de Dependência

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Por que dividir a aplicação em projetos](#por-que-dividir-a-aplicação-em-projetos)
3. [Os seis projetos apresentados](#os-seis-projetos-apresentados)
4. [API](#api)
5. [Application](#application)
6. [Communication](#communication)
7. [Exception](#exception)
8. [Domain](#domain)
9. [Infrastructure](#infrastructure)
10. [Direção das dependências](#direção-das-dependências)
11. [API e Infrastructure na prática](#api-e-infrastructure-na-prática)
12. [Modificadores de acesso e encapsulamento](#modificadores-de-acesso-e-encapsulamento)
13. [Resumo](#resumo)
14. [Visão Geral](#visão-geral)

---

## Contexto da aula

A aula apresenta a estrutura que será utilizada para organizar uma API em múltiplos projetos, cada um com uma responsabilidade específica.

A ideia reforçada é evitar colocar tudo dentro do mesmo projeto e separar responsabilidades em camadas.

A organização apresentada utiliza uma abordagem inspirada em Domain-Driven Design e será composta por seis projetos:

```text
API
Communication
Infrastructure
Domain
Application
Exception
```

O projeto de API é o projeto executável. Os outros cinco são bibliotecas de classes.

---

## Por que dividir a aplicação em projetos

A separação em projetos existe para que cada camada tenha uma responsabilidade clara.

Em vez de termos algo como:

```text
API
├── Controllers
├── Regras de negócio
├── Banco de dados
├── Envio de e-mail
├── Requests
├── Responses
└── Tratamento de erros
```

A proposta da aula é distribuir essas responsabilidades em projetos diferentes.

Isso ajuda a deixar explícito:

```text
quem recebe requisições
quem executa regras de negócio
quem representa contratos de entrada e saída
quem define elementos do domínio
quem implementa integrações externas
quem concentra exceções e mensagens
```

---

## Os seis projetos apresentados

```text
Projeto.API
Projeto.Application
Projeto.Communication
Projeto.Exception
Projeto.Domain
Projeto.Infrastructure
```

Cada um possui um papel específico.

| Projeto | Responsabilidade principal |
|---|---|
| `API` | Receber requisições e devolver respostas HTTP |
| `Application` | Regras de negócio |
| `Communication` | Requests e Responses |
| `Exception` | Exceções customizadas e recursos relacionados a mensagens de erro |
| `Domain` | Elementos e contratos mais abstratos relacionados ao domínio |
| `Infrastructure` | Implementações concretas de acesso a recursos externos |

---

## API

O projeto de API é responsável pela comunicação HTTP da aplicação.

Seu papel é:

```text
Receber uma requisição
↓
Repassar o trabalho para outra camada
↓
Receber o resultado
↓
Devolver uma resposta HTTP
```

A aula enfatiza que a API não deve conter as regras de negócio.

Por exemplo, uma Controller não deve concentrar validações como:

```text
O e-mail é válido?
O e-mail já existe?
A senha atende aos requisitos?
O nome está vazio?
```

Essas decisões pertencem ao projeto `Application`.

A API deve ficar focada na entrada e saída HTTP.

---

## Application

`Application` é uma Class Library responsável pelas regras de negócio.

Exemplos de regras citadas na aula:

```text
validar se um e-mail é válido
verificar se um e-mail já está cadastrado
validar se o nome está preenchido
verificar regras de segurança da senha
```

Fluxo simplificado:

```text
API recebe Request
        │
        ▼
Application executa a regra de negócio
        │
        ├── sucesso → devolve Response
        │
        └── erro → lança uma exceção
```

A ideia é que a API não precise conhecer os detalhes dessas regras.

---

## Communication

O projeto `Communication` concentra as classes utilizadas para receber e devolver informações.

A aula separa essas classes em dois grupos principais:

```text
Requests
Responses
```

### Requests

Representam os dados recebidos pela aplicação.

Exemplo apresentado para cadastro de usuário:

```csharp
public class RequestRegisterUserJson
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

O body da requisição é desserializado pelo .NET para uma classe desse tipo.

### Responses

Representam aquilo que a API realmente quer devolver.

A aula destaca que não devemos simplesmente devolver todas as propriedades que existem na entidade persistida no banco.

Por exemplo:

```text
Entidade User no banco
├── Id
├── Name
├── Email
├── Password
├── CreatedAt
└── outras informações
```

Não significa que a resposta da API deva expor tudo.

Uma Response pode selecionar apenas o necessário:

```text
ResponseUserJson
├── Id
├── Name
└── Email
```

A senha é usada como exemplo de informação que claramente não deveria ser devolvida ao cliente.

---

## Exception

O projeto `Exception` também é uma Class Library.

Segundo a organização apresentada, ele será utilizado para:

```text
Exceções customizadas
Mensagens de erro
Recursos de localização/tradução de mensagens
```

A proposta futura é permitir que mensagens de erro possam ser exibidas em diferentes idiomas.

Exemplo conceitual:

```text
Erro
├── Português
├── Inglês
├── Espanhol
├── Francês
└── outros idiomas
```

O `Application` poderá lançar as exceções, enquanto a `API` será responsável por tratá-las e transformar o erro no status code HTTP adequado.

```text
Application
   │
   └── lança exceção
          │
          ▼
         API
          │
          └── trata e devolve o status HTTP
```

---

## Domain

Na organização mostrada na aula, o projeto `Domain` contém elementos mais abstratos e relacionados à linguagem comum do sistema.

Um exemplo apresentado é a entidade:

```text
User
```

Ela representa um conceito conhecido por todos os envolvidos no projeto.

```text
User
├── Name
├── Email
├── Password
└── outras propriedades do domínio
```

A aula também coloca no `Domain` contratos usados para representar ações que o sistema precisa realizar.

Por exemplo:

```text
Salvar um usuário
Enviar um e-mail
Fazer upload de um arquivo
```

O ponto principal é que `Domain` não precisa definir os detalhes técnicos dessas operações.

Ele pode trabalhar com contratos/interfaces.

Exemplo conceitual:

```csharp
public interface IUserRepository
{
    void Save(User user);
}
```

O contrato diz:

```text
"é necessário salvar um usuário"
```

mas não diz:

```text
"salvar utilizando SQL Server"
```

ou:

```text
"salvar utilizando MySQL"
```

Esses detalhes pertencem a outra camada.

---

## Infrastructure

O projeto `Infrastructure` concentra as implementações concretas que interagem com recursos externos.

A analogia utilizada na aula é:

```text
Tudo aquilo que a aplicação precisa acessar fora dela
```

Exemplos:

```text
Banco de dados
Serviço de e-mail
Google Drive
Dropbox
Outros serviços externos
```

Assim:

```text
Domain
└── define o contrato

Infrastructure
└── implementa o contrato
```

Exemplo conceitual:

```csharp
public interface IUserRepository
{
    void Save(User user);
}
```

Implementação:

```csharp
internal class UserRepository : IUserRepository
{
    public void Save(User user)
    {
        // implementação concreta para o banco de dados
    }
}
```

O `Application` utiliza o contrato e não precisa saber qual tecnologia concreta está sendo usada.

```text
Application
      │
      ▼
   contrato
    Domain
      ▲
      │
Infrastructure implementa
```

---

## Direção das dependências

As setas apresentadas na aula indicam quais projetos conseguem enxergar/utilizar outros projetos.

### API

A API precisa enxergar:

```text
Communication
Application
Exception
```

porque precisa:

```text
Communication → Requests e Responses
Application   → executar regras de negócio
Exception     → tratar exceções e definir status HTTP
```

### Application

O Application precisa enxergar:

```text
Communication
Exception
Domain
```

Motivos:

```text
Communication → recebe Requests e devolve Responses
Exception     → lança exceções de negócio
Domain        → utiliza entidades e contratos
```

### Infrastructure

Infrastructure precisa enxergar:

```text
Domain
```

porque implementa contratos definidos no domínio.

### Projetos independentes

Na estrutura apresentada, estes projetos não dependem dos demais:

```text
Communication
Exception
Domain
```

Ou seja, eles são tratados como bases independentes que podem ser referenciadas pelas outras camadas.

---

## Diagrama teórico das dependências

```text
                    ┌──────────────────┐
                    │       API        │
                    └────────┬─────────┘
                             │
                ┌────────────┼────────────┐
                ▼            ▼            ▼
        Communication   Application    Exception
                              │
                    ┌─────────┼──────────┐
                    ▼         ▼          ▼
            Communication  Domain    Exception
                              ▲
                              │
                       Infrastructure
```

Uma forma mais simples de visualizar:

```text
API
├── Communication
├── Application
└── Exception

Application
├── Communication
├── Exception
└── Domain

Infrastructure
└── Domain

Communication
└── independente

Exception
└── independente

Domain
└── independente
```

---

## Application não conhece Infrastructure

Um ponto importante apresentado é que:

```text
Application NÃO precisa referenciar Infrastructure
```

Isso ocorre porque o Application trabalha com contratos definidos no Domain.

Exemplo:

```text
Application
    │
    │ "preciso salvar um usuário"
    ▼
IUserRepository
    │
    └── contrato no Domain
```

O Application não precisa saber se a implementação utiliza:

```text
MySQL
SQL Server
SQLite
ou outra tecnologia
```

A implementação concreta fica no Infrastructure.

Esse desacoplamento será conectado posteriormente por injeção de dependência.

---

## API e Infrastructure na prática

A aula mostra uma diferença entre o diagrama conceitual inicial e aquilo que será necessário na implementação real.

Inicialmente, o diagrama teórico não mostra:

```text
API → Infrastructure
```

Porém, na prática, essa referência será necessária.

Motivo:

```text
A configuração da injeção de dependência acontece no projeto de API.
```

A API precisa conseguir registrar algo equivalente a:

```text
Para este contrato do Domain
↓
utilize esta implementação do Infrastructure
```

Portanto, na prática:

```text
API
└── também referencia Infrastructure
```

A aula antecipa que a configuração será organizada de forma que a API apenas peça ao projeto de Infrastructure para registrar suas dependências, sem precisar conhecer cada implementação individualmente.

Fluxo conceitual:

```text
API
 │
 │ solicita configuração
 ▼
Infrastructure
 │
 ├── registra implementações
 ├── associa contratos às classes concretas
 └── devolve/configura o resultado
```

---

## Estrutura prática das referências

Considerando a observação sobre injeção de dependência, a estrutura final fica conceitualmente assim:

```text
API
├── Application
├── Communication
├── Exception
└── Infrastructure

Application
├── Communication
├── Exception
└── Domain

Infrastructure
└── Domain

Communication
└── independente

Exception
└── independente

Domain
└── independente
```

---

## Modificadores de acesso e encapsulamento

Mesmo que a API possua uma referência para `Infrastructure`, isso não significa que todas as classes de Infrastructure precisam ficar disponíveis para a API.

A aula relembra o modificador:

```csharp
internal
```

Classes concretas podem permanecer `internal`.

Exemplo:

```csharp
internal class UserRepository : IUserRepository
{
}
```

Assim, a API pode referenciar o projeto Infrastructure para permitir a configuração de dependências, mas não necessariamente consegue instanciar diretamente suas classes internas.

A ideia apresentada é expor apenas o necessário para realizar o registro da injeção de dependência.

---

## Fluxo geral da aplicação

```text
Usuário / Front-end
        │
        │ HTTP Request
        ▼
       API
        │
        │ Request
        ▼
   Application
        │
        ├── valida regras de negócio
        ├── utiliza entidades do Domain
        ├── utiliza contratos do Domain
        └── pode lançar Exception
        │
        ▼
      Domain
        ▲
        │ implementação dos contratos
        │
 Infrastructure
        │
        ├── banco de dados
        ├── e-mail
        ├── arquivos
        └── serviços externos
```

Os modelos de entrada e saída transitam por:

```text
Communication
├── Requests
└── Responses
```

---

## Resumo

| Camada | Tipo | Responsabilidade |
|---|---|---|
| **API** | Projeto executável | Receber requisições e devolver respostas HTTP |
| **Application** | Class Library | Executar regras de negócio |
| **Communication** | Class Library | Requests e Responses |
| **Exception** | Class Library | Exceções customizadas e recursos de mensagens de erro |
| **Domain** | Class Library | Entidades, conceitos e contratos abstratos |
| **Infrastructure** | Class Library | Implementações concretas de integrações externas |

### Dependências apresentadas

| Projeto | Enxerga |
|---|---|
| **API** | Application, Communication, Exception e, na prática, Infrastructure |
| **Application** | Communication, Exception e Domain |
| **Infrastructure** | Domain |
| **Communication** | Nenhum dos outros projetos |
| **Exception** | Nenhum dos outros projetos |
| **Domain** | Nenhum dos outros projetos |

---

## Visão Geral

```text
                         CLIENTE
                            │
                            ▼
                    ┌───────────────┐
                    │      API      │
                    └───────┬───────┘
                            │
             ┌──────────────┼──────────────┐
             │              │              │
             ▼              ▼              ▼
      Communication    Application      Exception
                            │
                    ┌───────┼────────┐
                    │       │        │
                    ▼       ▼        ▼
             Communication Domain  Exception
                            ▲
                            │
                    Infrastructure
                            │
             ┌──────────────┼──────────────┐
             ▼              ▼              ▼
        Banco de dados   E-mail      Serviços externos
```

### Ideia principal da aula

```text
Cada projeto possui uma responsabilidade específica
                     +
As dependências devem seguir uma direção clara
                     +
Application trabalha com abstrações do Domain
                     +
Infrastructure fornece as implementações concretas
                     +
API conecta tudo por meio da injeção de dependência
```

> **Em resumo:** a estrutura apresentada divide a aplicação em seis projetos: API, Application, Communication, Exception, Domain e Infrastructure. A API cuida do HTTP, Application das regras de negócio, Communication dos contratos de entrada e saída, Exception dos erros, Domain das entidades e abstrações e Infrastructure das implementações concretas externas. Application não precisa conhecer Infrastructure diretamente; ambos se conectam pelos contratos de Domain. Na prática, porém, a API precisa referenciar Infrastructure para realizar a configuração da injeção de dependência.
