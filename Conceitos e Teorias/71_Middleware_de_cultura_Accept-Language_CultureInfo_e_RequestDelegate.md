# Middleware de cultura: Accept-Language, CultureInfo e RequestDelegate

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [O que é um middleware](#2-o-que-é-um-middleware)
3. [Por que criar um middleware de cultura](#3-por-que-criar-um-middleware-de-cultura)
4. [Criando a classe CultureMiddleware](#4-criando-a-classe-culturemiddleware)
5. [Assinatura do método Invoke](#5-assinatura-do-método-invoke)
6. [RequestDelegate e continuidade do fluxo](#6-requestdelegate-e-continuidade-do-fluxo)
7. [Lendo a cultura no header](#7-lendo-a-cultura-no-header)
8. [Definindo uma cultura padrão](#8-definindo-uma-cultura-padrão)
9. [Sobrescrevendo a cultura quando o header estiver preenchido](#9-sobrescrevendo-a-cultura-quando-o-header-estiver-preenchido)
10. [CurrentCulture e CurrentUICulture](#10-currentculture-e-currentuiculture)
11. [Liberando o fluxo com await _next(context)](#11-liberando-o-fluxo-com-await-_nextcontext)
12. [Fluxo completo](#12-fluxo-completo)
13. [Pontos importantes](#13-pontos-importantes)
14. [Resumo](#14-resumo)
15. [Visão geral](#15-visão-geral)

---

## 1. Objetivo da aula

Nesta aula é criado um **middleware de cultura** para permitir que quem chama a API informe qual idioma deseja receber nas respostas.

Até aqui, os arquivos de Resource já existem, porém a escolha da cultura ainda depende da cultura da máquina onde a API está sendo executada ou do arquivo default.

A ideia agora é permitir que o próprio cliente da API — por exemplo, um site ou aplicativo — informe a cultura desejada por meio do **header da requisição**.

---

## 2. O que é um middleware

O middleware é apresentado como uma classe que fica **no meio do fluxo da requisição**.

Antes de a requisição chegar ao endpoint do Controller, ela passa pelo middleware, que pode executar alguma lógica e depois decidir se o fluxo deve continuar.

Fluxo simplificado:

```text
Cliente
   ↓
Requisição HTTP
   ↓
Middleware
   ↓
Controller / Endpoint
```

Nesse caso, o middleware será responsável por identificar a cultura desejada e configurá-la antes de o restante da aplicação continuar.

---

## 3. Por que criar um middleware de cultura

Sem esse middleware, a API tende a utilizar:

- a cultura da máquina onde está sendo executada; ou
- o Resource File default quando não existe um arquivo específico para aquela cultura.

O objetivo é mudar esse comportamento para permitir algo assim:

```text
Aplicativo envia: pt-BR
        ↓
API configura pt-BR
        ↓
Resource pt-BR é utilizado
        ↓
Mensagens retornam em português do Brasil
```

O mesmo raciocínio vale para `pt-PT`, `fr`, inglês ou qualquer outra cultura suportada.

---

## 4. Criando a classe CultureMiddleware

Dentro do projeto da API é criada uma pasta:

```text
Middleware/
```

E dentro dela uma classe semelhante a:

```csharp
public class CultureMiddleware
{
}
```

A aula reforça que, apesar de ser uma classe normal, ela precisa seguir uma estrutura específica para funcionar corretamente como middleware.

---

## 5. Assinatura do método Invoke

O middleware precisa possuir um método público chamado `Invoke`.

A estrutura mostrada é:

```csharp
public async Task Invoke(HttpContext context)
{
}
```

Pontos importantes:

- o método é `public`;
- utiliza `async`;
- retorna `Task`;
- o nome deve ser `Invoke`;
- recebe um `HttpContext`.

O `HttpContext` permite acessar informações relacionadas à requisição e à resposta HTTP.

A aula ainda não aprofunda `async`, `Task` e `await`; esses conceitos serão explicados posteriormente no curso.

---

## 6. RequestDelegate e continuidade do fluxo

O middleware também recebe um `RequestDelegate` no construtor.

Exemplo:

```csharp
private readonly RequestDelegate _next;

public CultureMiddleware(RequestDelegate next)
{
    _next = next;
}
```

Na explicação da aula, o `RequestDelegate` pode ser imaginado como o mecanismo que permite que o fluxo siga para a próxima etapa.

Ou seja:

```text
Requisição
   ↓
CultureMiddleware
   ↓
executa lógica de cultura
   ↓
_next(context)
   ↓
próximo middleware / endpoint
```

Sem chamar o próximo passo, a requisição fica interrompida naquele middleware.

---

## 7. Lendo a cultura no header

A cultura desejada é extraída dos headers da requisição.

A aula utiliza o header de idioma da requisição e recupera o primeiro valor disponível.

A ideia pode ser representada assim:

```csharp
var culture = context.Request.Headers.AcceptLanguage.FirstOrDefault();
```

O valor obtido pode ser algo como:

```text
pt-BR
```

ou:

```text
pt-PT
```

ou:

```text
fr
```

Caso nenhum valor seja encontrado, a variável pode ficar nula.

---

## 8. Definindo uma cultura padrão

Antes de verificar o header, a aula cria uma cultura padrão.

Exemplo:

```csharp
var cultureInfo = new CultureInfo("en");
```

Nesse projeto, o idioma padrão escolhido é o inglês.

Isso é compatível com o Resource File sem tag, porque na aula anterior esse arquivo default foi preenchido com mensagens em inglês.

Fluxo:

```text
Nenhuma cultura enviada
        ↓
CultureInfo("en")
        ↓
Resource específico não encontrado
        ↓
Resource default
        ↓
Mensagens em inglês
```

A aula também menciona que o padrão poderia ser outra cultura, como `pt-BR`, caso esse fosse o comportamento desejado pela aplicação.

---

## 9. Sobrescrevendo a cultura quando o header estiver preenchido

Depois de criar o valor default, verifica-se se a cultura recebida no header está preenchida.

Exemplo:

```csharp
if (!string.IsNullOrWhiteSpace(culture))
{
    cultureInfo = new CultureInfo(culture);
}
```

O comportamento fica assim:

```text
Header vazio/nulo
    ↓
usa cultura padrão

Header preenchido
    ↓
cria CultureInfo com o valor recebido
```

Por exemplo:

```text
Accept-Language: pt-BR
```

faz com que a variável passe a representar a cultura `pt-BR`.

---

## 10. CurrentCulture e CurrentUICulture

Depois de descobrir qual `CultureInfo` deve ser utilizada, a cultura atual da execução é alterada.

A lógica apresentada é equivalente a:

```csharp
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;
```

### `CurrentCulture`

Representa a cultura utilizada pela execução atual.

### `CurrentUICulture`

É especialmente importante para a resolução de recursos localizados, como os arquivos `.resx` utilizados para as mensagens de erro.

Com isso, o código continua acessando as mesmas chaves:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

mas o valor retornado pode variar de acordo com a cultura configurada.

Exemplo conceitual:

```text
ResourceErrorMessages.TITLE_REQUIRED
             │
             ├── en    → Title is required
             ├── pt-BR → O título é obrigatório
             ├── pt-PT → O título é obrigatório
             └── fr    → mensagem correspondente em francês
```

---

## 11. Liberando o fluxo com await _next(context)

Depois de configurar a cultura, é necessário permitir que a requisição continue.

Isso é feito com:

```csharp
await _next(context);
```

Essa linha é fundamental.

Sem ela, o middleware executaria a configuração da cultura, mas a requisição não seguiria para o endpoint.

Fluxo:

```text
Middleware recebe a requisição
        ↓
Lê o idioma
        ↓
Configura CultureInfo
        ↓
await _next(context)
        ↓
Controller / Endpoint
```

---

## 12. Fluxo completo

A estrutura apresentada na aula pode ser entendida aproximadamente assim:

```csharp
public class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var culture = context.Request.Headers.AcceptLanguage.FirstOrDefault();

        var cultureInfo = new CultureInfo("en");

        if (!string.IsNullOrWhiteSpace(culture))
        {
            cultureInfo = new CultureInfo(culture);
        }

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}
```

O objetivo da aula não é aprofundar ainda todos os detalhes de `async`/`await`, mas mostrar a estrutura necessária para o middleware de cultura.

---

## 13. Pontos importantes

### Middleware executa antes do endpoint

Ele permite aplicar uma lógica transversal antes de a requisição chegar ao Controller.

### O idioma vem da requisição

A aplicação deixa de depender apenas da cultura da máquina e passa a considerar o idioma enviado pelo cliente.

### Existe uma cultura default

Se nenhuma cultura for informada, a aula utiliza inglês como padrão.

### O Resource File continua sendo acessado da mesma forma

Não é necessário alterar os Validators ou o `ExceptionFilter` para escolher manualmente cada `.resx`.

A cultura configurada é que orienta qual Resource deve fornecer o valor.

### `RequestDelegate` permite continuar o pipeline

O `_next` representa a continuação do fluxo.

### `await _next(context)` é indispensável nesse middleware

Sem essa chamada, o endpoint não será alcançado.

### O middleware ainda precisa ser registrado

Ao final da aula, a classe foi criada, mas ainda falta configurar a API para utilizá-la no pipeline.

Esse registro e os testes ficam para a próxima aula.

---

## 14. Resumo

Nesta aula foi criado um middleware responsável por definir a cultura utilizada pela API.

A requisição passa primeiro pelo `CultureMiddleware`, que consulta o header de idioma, define uma cultura padrão em inglês e, quando existe um valor enviado pelo cliente, cria um `CultureInfo` correspondente.

Depois, essa cultura é atribuída a `CurrentCulture` e `CurrentUICulture`, permitindo que os Resource Files retornem automaticamente as mensagens na cultura configurada.

Por fim, o middleware chama:

```csharp
await _next(context);
```

para permitir que a requisição continue até o endpoint.

---

## 15. Visão Geral

```text
Cliente / Aplicativo
        │
        │ Header com idioma
        ▼
┌─────────────────────────────┐
│      CultureMiddleware      │
│                             │
│  lê cultura do header       │
│            │                │
│            ▼                │
│  cultura informada?         │
│     │             │         │
│    não           sim        │
│     │             │         │
│     ▼             ▼         │
│  default "en"   nova        │
│                 CultureInfo │
│       └──────┬──────┘        │
│              ▼               │
│ CurrentCulture              │
│ CurrentUICulture            │
│              │               │
│              ▼               │
│   await _next(context)       │
└──────────────┬──────────────┘
               │
               ▼
       Controller / Endpoint
               │
               ▼
        Validator / Use Case
               │
               ▼
       ResourceErrorMessages
               │
               ▼
   mensagem na cultura atual
```
