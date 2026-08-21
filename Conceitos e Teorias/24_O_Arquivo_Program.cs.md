# Program.cs

## 📋 Índice

1. [O que é o Program.cs?](#o-que-é-o-programcs)
2. [Program.cs como Entry Point](#programcs-como-entry-point)
3. [WebApplication Builder](#webapplication-builder)
4. [Configuração dos serviços](#configuração-dos-serviços)
5. [Build da aplicação](#build-da-aplicação)
6. [Configurações da aplicação](#configurações-da-aplicação)
7. [Swagger em ambiente de desenvolvimento](#swagger-em-ambiente-de-desenvolvimento)
8. [Redirecionamento para HTTPS](#redirecionamento-para-https)
9. [Autorização](#autorização)
10. [Mapeamento dos Controllers](#mapeamento-dos-controllers)
11. [Execução da aplicação](#execução-da-aplicação)
12. [Fluxo do Program.cs](#fluxo-do-programcs)
13. [Resumo](#resumo)

---

## O que é o Program.cs?

O `Program.cs` é um dos principais arquivos de uma aplicação **ASP.NET Core Web API**.

Ele é responsável por realizar as configurações necessárias para que a aplicação seja inicializada e executada corretamente.

Em um projeto, podemos encontrá-lo diretamente na raiz:

```text
MyFirstAPI
│
├── Properties
│
├── Controllers
│
├── Program.cs
└── ...
```

Durante o desenvolvimento, utilizamos o `Program.cs` para configurar diversos recursos utilizados pela API.

> O `Program.cs` não deve ser excluído, pois representa o ponto inicial da execução e da configuração da aplicação.

---

## Program.cs como Entry Point

O `Program.cs` funciona como o **Entry Point (ponto de entrada)** da aplicação.

Isso significa que, quando executamos nossa API, as instruções presentes nesse arquivo estão entre as primeiras a serem executadas.

De forma simplificada:

```text
Executar a API
      ↓
  Program.cs
      ↓
Configurações
      ↓
Inicialização
      ↓
API preparada para receber requisições
```

Nas versões atuais do .NET, esse arquivo pode possuir uma sintaxe bastante simplificada, sem a declaração explícita de uma classe `Program` e de um método `Main`.

Mesmo assim, ele continua representando o ponto de entrada da aplicação.

---

## WebApplication Builder

Uma das primeiras instruções encontradas no `Program.cs` é a criação do **Builder**:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

O `builder` é utilizado para preparar e configurar os recursos necessários para a aplicação.

Podemos pensar nele como uma etapa de **construção da configuração da API**.

```text
builder
   │
   ├── Controllers
   ├── Endpoints
   ├── Swagger
   └── Outras configurações
```

Ao longo do desenvolvimento, novos serviços e configurações podem ser adicionados ao `builder`.

---

## Configuração dos serviços

Depois de criar o `builder`, podemos registrar os serviços necessários para a aplicação.

Essas configurações são realizadas antes da construção da aplicação.

Exemplo:

```csharp
builder.Services.AddControllers();
```

Essa instrução adiciona suporte aos **Controllers** da API.

Os Controllers serão responsáveis por disponibilizar funcionalidades e receber determinadas requisições da aplicação.

---

### Configuração relacionada aos endpoints

Também podemos encontrar configurações relacionadas à exploração dos endpoints:

```csharp
builder.Services.AddEndpointsApiExplorer();
```

Essa configuração permite que informações sobre os endpoints da API sejam disponibilizadas para outras ferramentas, como o Swagger.

---

### Configuração do Swagger

Outra configuração presente pode ser:

```csharp
builder.Services.AddSwaggerGen();
```

Ela adiciona os serviços necessários para geração da documentação utilizada pelo **Swagger**.

O Swagger permite visualizar e documentar os endpoints disponibilizados pela API.

De forma simplificada:

```text
API
 ↓
Endpoints
 ↓
Swagger
 ↓
Documentação visual da API
```

---

## Build da aplicação

Depois de realizar as configurações necessárias utilizando o `builder`, encontramos:

```csharp
var app = builder.Build();
```

O método:

```csharp
Build()
```

constrói a aplicação utilizando as configurações que foram definidas anteriormente.

Podemos visualizar dessa forma:

```text
builder
   │
   ├── Controllers
   ├── Endpoints
   ├── Swagger
   └── Outras configurações
   │
   ▼
builder.Build()
   │
   ▼
  app
```

O resultado é armazenado na variável:

```csharp
app
```

A partir desse momento, utilizamos `app` para continuar configurando o comportamento da aplicação.

---

## Configurações da aplicação

Existe uma diferença importante entre as configurações realizadas utilizando `builder` e aquelas realizadas utilizando `app`.

De forma simplificada:

```text
builder
   ↓
Adiciona e configura os serviços necessários

builder.Build()
   ↓
Constrói a aplicação

app
   ↓
Configura como a aplicação irá se comportar
```

Exemplo:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

Depois do `Build()`, outras configurações são realizadas utilizando `app`.

---

## Swagger em ambiente de desenvolvimento

Podemos encontrar uma condição semelhante a:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Essa condição verifica se a aplicação está sendo executada em um **ambiente de desenvolvimento**.

Se estiver, serão habilitados:

```csharp
app.UseSwagger();
app.UseSwaggerUI();
```

Isso permite visualizar a interface do Swagger durante o desenvolvimento.

Fluxo:

```text
A aplicação está em Development?
             │
        ┌────┴────┐
        │         │
       Sim       Não
        │         │
        ▼         ▼
   Habilita     Não habilita
   Swagger       Swagger
```

---

### Por que limitar o Swagger ao ambiente de desenvolvimento?

O Swagger apresenta informações sobre os endpoints existentes na API.

Por questões de segurança, pode ser interessante não disponibilizar toda essa documentação publicamente em produção.

Por isso, podemos restringi-lo:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

Assim:

```text
Development
     ↓
Swagger disponível

Production
     ↓
Swagger não disponível
```

É possível alterar esse comportamento, mas a aula recomenda manter o Swagger apenas no ambiente de desenvolvimento.

---

## Redirecionamento para HTTPS

Outra configuração encontrada no `Program.cs` é:

```csharp
app.UseHttpsRedirection();
```

Essa configuração determina que a aplicação utilize redirecionamento para **HTTPS**.

De forma simplificada:

```text
HTTP
 ↓
Redirecionamento
 ↓
HTTPS
```

O HTTPS permite que a comunicação seja realizada utilizando uma conexão segura.

---

## Autorização

Também encontramos:

```csharp
app.UseAuthorization();
```

Essa configuração adiciona o mecanismo relacionado à **autorização** na aplicação.

A autorização está relacionada à verificação das permissões de um usuário.

Como vimos anteriormente:

```text
Autenticação
     ↓
Quem é você?

Autorização
     ↓
O que você pode fazer?
```

O funcionamento da autorização será aprofundado posteriormente no desenvolvimento da API.

---

## Mapeamento dos Controllers

Anteriormente adicionamos suporte aos Controllers:

```csharp
builder.Services.AddControllers();
```

Depois da construção da aplicação, precisamos mapear esses Controllers:

```csharp
app.MapControllers();
```

De forma simplificada:

```text
builder.Services.AddControllers()
          ↓
Adiciona suporte aos Controllers

app.MapControllers()
          ↓
Mapeia os Controllers para receber requisições
```

Esse mapeamento permite que os endpoints definidos nos Controllers sejam utilizados pela aplicação.

---

## Execução da aplicação

No final do `Program.cs`, encontramos:

```csharp
app.Run();
```

Essa instrução inicia efetivamente a aplicação.

Após essa etapa, a API fica preparada para **receber e processar requisições**.

Fluxo:

```text
Configurações
      ↓
Build
      ↓
Configuração da aplicação
      ↓
Mapeamento dos Controllers
      ↓
app.Run()
      ↓
API em execução
      ↓
Aguardando requisições
```

O `app.Run()` é, portanto, uma etapa fundamental para colocar a aplicação em funcionamento.

---

## Fluxo do Program.cs

Um `Program.cs` básico pode possuir uma estrutura semelhante a:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Podemos dividir esse arquivo em três grandes etapas.

### 1. Preparação

```csharp
var builder = WebApplication.CreateBuilder(args);
```

Criamos o objeto responsável pela preparação da aplicação.

---

### 2. Registro e construção

```csharp
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

Registramos os recursos necessários e construímos a aplicação.

---

### 3. Configuração e execução

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Configuramos o comportamento da aplicação e, por fim, iniciamos sua execução.

---

## Resumo

| Código | Responsabilidade |
|---|---|
| `WebApplication.CreateBuilder(args)` | Cria o Builder utilizado para configurar a aplicação |
| `builder.Services.AddControllers()` | Adiciona suporte aos Controllers |
| `AddEndpointsApiExplorer()` | Adiciona informações relacionadas aos endpoints |
| `AddSwaggerGen()` | Adiciona os recursos necessários para geração do Swagger |
| `builder.Build()` | Constrói a aplicação com as configurações realizadas |
| `app.Environment.IsDevelopment()` | Verifica se o ambiente atual é de desenvolvimento |
| `app.UseSwagger()` | Habilita a geração da documentação do Swagger |
| `app.UseSwaggerUI()` | Habilita a interface visual do Swagger |
| `app.UseHttpsRedirection()` | Configura o redirecionamento para HTTPS |
| `app.UseAuthorization()` | Adiciona o mecanismo relacionado à autorização |
| `app.MapControllers()` | Mapeia os Controllers da API |
| `app.Run()` | Inicia a aplicação e a deixa preparada para receber requisições |

---

## Visão Geral

```text
                 Program.cs
                     │
                     ▼
       WebApplication.CreateBuilder()
                     │
                     ▼
              ┌──────────────┐
              │   builder    │
              └──────┬───────┘
                     │
              Adiciona serviços
                     │
        ┌────────────┼────────────┐
        │            │            │
   Controllers    Endpoints    Swagger
        │            │            │
        └────────────┼────────────┘
                     │
                     ▼
              builder.Build()
                     │
                     ▼
              ┌──────────────┐
              │     app      │
              └──────┬───────┘
                     │
              Configura a API
                     │
        ┌────────────┼─────────────┐
        │            │             │
     Swagger       HTTPS      Authorization
        │            │             │
        └────────────┼─────────────┘
                     │
                     ▼
             MapControllers()
                     │
                     ▼
                 app.Run()
                     │
                     ▼
               API EXECUTANDO
                     │
                     ▼
            Recebe requisições
```

> **Em resumo:** o `Program.cs` é o ponto de entrada da aplicação. Nele configuramos os serviços que a API utilizará, construímos a aplicação, configuramos seu comportamento e, por fim, iniciamos sua execução com `app.Run()`.