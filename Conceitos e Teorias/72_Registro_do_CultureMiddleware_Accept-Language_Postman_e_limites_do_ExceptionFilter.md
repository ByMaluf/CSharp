# Registro do CultureMiddleware, Accept-Language, Postman e limites do ExceptionFilter

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Registrando o middleware no pipeline](#registrando-o-middleware-no-pipeline)
3. [Por que o middleware é registrado depois do Build](#por-que-o-middleware-é-registrado-depois-do-build)
4. [Testando com Postman](#testando-com-postman)
5. [Fluxo sem Accept-Language](#fluxo-sem-accept-language)
6. [Fluxo com pt-BR](#fluxo-com-pt-br)
7. [Fluxo com pt-PT](#fluxo-com-pt-pt)
8. [Fallback para cultura genérica](#fallback-para-cultura-genérica)
9. [Problema ao testar pelo Swagger](#problema-ao-testar-pelo-swagger)
10. [Por que o ExceptionFilter não captura exceções do middleware](#por-que-o-exceptionfilter-não-captura-exceções-do-middleware)
11. [Resumo](#resumo)
12. [Visão Geral](#visão-geral-1)

---

## Visão Geral

Na aula anterior foi criado um middleware responsável por identificar a cultura desejada pela requisição e configurar `CultureInfo.CurrentCulture` e `CultureInfo.CurrentUICulture` antes de o fluxo chegar ao Controller.

Nesta aula, esse middleware é registrado no pipeline da API e testado na prática. Os testes mostram que o header `Accept-Language` consegue alterar dinamicamente o idioma das mensagens retornadas pela aplicação.

Também aparece um comportamento importante: exceções lançadas dentro do próprio middleware não chegam ao `ExceptionFilter`, porque o filtro atua apenas depois que a requisição entra no pipeline MVC.

---

## Registrando o middleware no pipeline

Para a API realmente executar o `CultureMiddleware`, é necessário registrá-lo no `Program.cs`.

Diferentemente do filtro de exceções, que foi configurado na coleção de serviços, o middleware é adicionado depois da criação da aplicação com `builder.Build()`.

Exemplo:

```csharp
var app = builder.Build();

app.UseMiddleware<CultureMiddleware>();
```

A ideia é simples:

```text
Requisição
    ↓
CultureMiddleware
    ↓
Controller / Endpoint
```

Sem essa chamada, a classe existe no projeto, mas não participa do fluxo HTTP.

---

## Por que o middleware é registrado depois do Build

Na configuração inicial existem dois momentos diferentes.

Antes do `Build`, trabalhamos principalmente com os serviços:

```csharp
builder.Services
```

Depois do `Build`, passamos a configurar o pipeline HTTP através de:

```csharp
app
```

Por isso o filtro e o middleware aparecem em pontos diferentes.

Exemplo conceitual:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc(options =>
{
    options.Filters.Add(typeof(ExceptionFilter));
});

var app = builder.Build();

app.UseMiddleware<CultureMiddleware>();
```

O middleware funciona como um interceptador dentro do pipeline de requisições.

---

## Testando com Postman

Para testar o comportamento da cultura, a aula utiliza o Postman.

A requisição é configurada como `POST`, apontando para o endpoint de despesas, com o corpo em JSON.

Um dado inválido é enviado propositalmente para provocar a validação e observar em qual idioma as mensagens de erro serão retornadas.

Exemplo conceitual:

```http
POST /api/expenses
Content-Type: application/json
```

Body:

```json
{
  "title": "",
  "amount": 0
}
```

Com breakpoints, é possível perceber que a requisição passa primeiro pelo método `Invoke` do middleware e somente depois chega ao endpoint.

Fluxo:

```text
Postman
   ↓
CultureMiddleware.Invoke(...)
   ↓
await _next(context)
   ↓
Controller
   ↓
Use Case
   ↓
Validator
```

---

## Fluxo sem Accept-Language

Quando a requisição não envia o header `Accept-Language`, o middleware não encontra nenhuma cultura informada.

Conceitualmente:

```csharp
var culture = context.Request.Headers.AcceptLanguage.FirstOrDefault();
```

Nesse caso:

```text
culture = null
```

Como existe uma cultura default definida no middleware, essa configuração é utilizada.

Exemplo:

```csharp
var cultureInfo = new CultureInfo("en");
```

Depois:

```csharp
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;
```

Resultado do teste:

```text
HTTP 400 Bad Request
Mensagens em inglês
```

Isso confirma que o middleware executou antes da validação.

---

## Fluxo com pt-BR

No Postman, a aula adiciona o header:

```http
Accept-Language: pt-BR
```

O valor precisa corresponder à chave que o middleware está lendo.

A partir disso:

```csharp
var culture = context.Request.Headers.AcceptLanguage.FirstOrDefault();
```

passa a retornar:

```text
pt-BR
```

Como a cultura não é nula nem vazia, o valor default é substituído:

```csharp
cultureInfo = new CultureInfo(culture);
```

Depois:

```csharp
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;
```

Quando o Validator acessa as mensagens do Resource File, o .NET seleciona automaticamente o arquivo correspondente à cultura `pt-BR`.

Resultado:

```text
HTTP 400 Bad Request

O título é obrigatório.
O valor deve ser maior que zero.
```

Assim, o mesmo código de validação consegue retornar idiomas diferentes sem alterar os `.WithMessage(...)`.

---

## Fluxo com pt-PT

O mesmo teste é repetido com:

```http
Accept-Language: pt-PT
```

O middleware cria:

```csharp
new CultureInfo("pt-PT")
```

A aplicação então utiliza o Resource File correspondente a português de Portugal.

A diferença usada para demonstrar o funcionamento é a mensagem relacionada ao valor da despesa.

No português do Brasil:

```text
O valor deve ser maior que zero.
```

No português de Portugal:

```text
O montante deve ser maior que zero.
```

Essa diferença confirma que não basta tratar apenas o idioma de maneira genérica quando existem variações regionais relevantes.

---

## Fallback para cultura genérica

A aula também testa um cenário interessante envolvendo francês.

Existe um arquivo genérico:

```text
ResourceErrorMessages.fr.resx
```

mas não existe um arquivo específico para francês da Bélgica:

```text
ResourceErrorMessages.fr-BE.resx
```

A requisição envia:

```http
Accept-Language: fr-BE
```

Mesmo sem um arquivo específico para `fr-BE`, o mecanismo de resources consegue utilizar o arquivo genérico `fr`.

Fluxo conceitual:

```text
fr-BE solicitado
    ↓
fr-BE.resx não existe
    ↓
fr.resx existe
    ↓
utiliza fr.resx
```

Isso mostra o fallback hierárquico das culturas.

Exemplo:

```text
fr-BE → fr → resource default
```

Portanto, nem sempre é obrigatório criar uma tradução específica para cada país, embora isso possa ser desejável quando existem diferenças linguísticas relevantes.

---

## Problema ao testar pelo Swagger

Ao testar pelo Swagger, surge um problema.

A interface não está configurada nessa etapa para permitir que o usuário informe manualmente o `Accept-Language`.

Além disso, o navegador/Swagger envia automaticamente um valor mais complexo no header, incluindo preferências e pesos de qualidade.

Algo conceitualmente parecido com:

```http
Accept-Language: en-US,en;q=0.9
```

O middleware atual pega diretamente o primeiro valor do header como uma string inteira e tenta fazer:

```csharp
new CultureInfo(culture)
```

Porém uma string contendo parâmetros como `;q=0.9` não representa diretamente uma cultura válida para `CultureInfo`.

Resultado:

```text
CultureNotFoundException
```

A API acaba retornando erro 500.

Esse comportamento será corrigido posteriormente.

---

## Por que o ExceptionFilter não captura exceções do middleware

Um dos pontos mais importantes da aula é entender por que o `ExceptionFilter` não tratou o erro ocorrido no `CultureMiddleware`.

O `ExceptionFilter` faz parte do fluxo MVC.

De forma simplificada:

```text
Requisição
    ↓
Middleware
    ↓
MVC
    ↓
Controller
    ↓
ExceptionFilter
```

Se a exceção acontece antes da requisição chegar ao MVC, o filtro ainda não está participando da execução daquele trecho.

Por exemplo:

```csharp
var cultureInfo = new CultureInfo(culture);
```

Se essa linha lança uma exceção dentro do middleware, o fluxo nem chega ao:

```csharp
await _next(context);
```

Consequentemente, a requisição não chega ao Controller.

O `ExceptionFilter` não intercepta essa exceção.

Representação:

```text
CultureMiddleware
    ↓
ERRO
    X
await _next(context)
    X
Controller
    X
ExceptionFilter
```

Essa distinção é importante porque filtros MVC e middlewares operam em níveis diferentes do pipeline.

---

## Resumo

Nesta aula o `CultureMiddleware` passa a fazer parte efetivamente da API.

Principais conceitos:

| Conceito | Função |
|---|---|
| `app.UseMiddleware<CultureMiddleware>()` | Registra o middleware no pipeline |
| `Accept-Language` | Informa a preferência de idioma da requisição |
| `CultureInfo` | Representa a cultura utilizada pela aplicação |
| `CurrentCulture` | Cultura usada pela execução atual |
| `CurrentUICulture` | Cultura usada principalmente para resources/localização |
| `await _next(context)` | Permite que a requisição continue no pipeline |
| `pt-BR` | Português do Brasil |
| `pt-PT` | Português de Portugal |
| `fr-BE → fr` | Exemplo de fallback para cultura genérica |
| `ExceptionFilter` | Trata exceções dentro do pipeline MVC, não exceções anteriores no middleware |

Fluxo completo:

```text
Cliente
   │
   │ Accept-Language: pt-BR
   ▼
CultureMiddleware
   │
   ├── lê Accept-Language
   ├── cria CultureInfo
   ├── define CurrentCulture
   ├── define CurrentUICulture
   │
   ▼
await _next(context)
   │
   ▼
Controller
   │
   ▼
Use Case
   │
   ▼
Validator
   │
   ▼
ResourceErrorMessages
   │
   ▼
ResourceErrorMessages.pt-BR.resx
   │
   ▼
Resposta localizada
```

---

## Visão Geral

A principal evolução desta aula é entender que localization não depende apenas da existência de arquivos `.resx`. Também é necessário definir corretamente a cultura da requisição antes que o restante da aplicação utilize esses resources.

O middleware resolve exatamente esse problema: ele interpreta a informação enviada pelo cliente e configura a cultura antes de a requisição chegar aos endpoints.

Ao mesmo tempo, a aula apresenta uma limitação arquitetural importante: um `ExceptionFilter` MVC não é um mecanismo global para absolutamente todo o pipeline HTTP. Exceções que acontecem antes do MVC, como dentro do próprio middleware de cultura, precisam ser tratadas em outro ponto do pipeline.