# Validação de cultura suportada com CultureInfo, Exists e short-circuit

## 📋 Índice

1. [Objetivo da aula](#1-objetivo-da-aula)
2. [Problema que precisa ser resolvido](#2-problema-que-precisa-ser-resolvido)
3. [Obtendo as culturas suportadas pelo .NET](#3-obtendo-as-culturas-suportadas-pelo-net)
4. [Renomeando a cultura recebida para RequestedCulture](#4-renomeando-a-cultura-recebida-para-requestedculture)
5. [Validando se a cultura existe](#5-validando-se-a-cultura-existe)
6. [CultureInfo.Name e a comparação com o header](#6-cultureinfoname-e-a-comparação-com-o-header)
7. [Mantendo a cultura default](#7-mantendo-a-cultura-default)
8. [String.IsNullOrWhiteSpace antes da busca](#8-stringisnullorwhitespace-antes-da-busca)
9. [Short-circuit com && e ||](#9-short-circuit-com--e-)
10. [Teste com o Swagger](#10-teste-com-o-swagger)
11. [Fluxo final do CultureMiddleware](#11-fluxo-final-do-culturemiddleware)
12. [Resumo](#12-resumo)
13. [Visão geral](#13-visão-geral)

---

## 1. Objetivo da aula

Depois de criar e registrar o `CultureMiddleware`, surgiu um problema: a aplicação estava tentando criar um `CultureInfo` diretamente com qualquer valor recebido no header da requisição.

A melhoria desta aula é adicionar uma validação antes dessa criação.

A ideia é verificar se a cultura recebida é realmente uma cultura que o .NET reconhece e suporta.

O fluxo passa a ser:

```text
Requisição HTTP
      ↓
Accept-Language
      ↓
CultureMiddleware
      ↓
Cultura foi informada?
      ↓
A cultura existe entre as suportadas pelo .NET?
      ↓
Sim → cria CultureInfo com a cultura recebida
Não → mantém a cultura default
      ↓
CurrentCulture / CurrentUICulture
      ↓
_next(context)
      ↓
Controller
```

---

## 2. Problema que precisa ser resolvido

Na aula anterior, o middleware recuperava diretamente o valor de `Accept-Language` e tentava utilizá-lo na criação de um `CultureInfo`.

De forma simplificada, tínhamos algo parecido com:

```csharp
var requestedCulture = context.Request.Headers.AcceptLanguage.FirstOrDefault();

var cultureInfo = new CultureInfo("en");

if (!string.IsNullOrWhiteSpace(requestedCulture))
{
    cultureInfo = new CultureInfo(requestedCulture);
}
```

O problema é que o header pode chegar em um formato que não representa diretamente o nome de uma cultura reconhecida pelo .NET.

O Swagger, por exemplo, estava enviando um valor mais complexo no `Accept-Language`.

Em vez de algo simples como:

```text
pt-BR
```

poderia chegar algo semelhante a:

```text
en-US,en;q=0.9
```

Se esse valor inteiro for passado para:

```csharp
new CultureInfo(requestedCulture)
```

a aplicação pode lançar uma exceção porque aquela string completa não corresponde a uma cultura válida.

A solução proposta na aula é validar o valor antes de criar o novo `CultureInfo`.

---

## 3. Obtendo as culturas suportadas pelo .NET

Primeiro, o middleware cria uma lista contendo as culturas conhecidas pelo .NET.

A aula utiliza:

```csharp
var supportedLanguages = CultureInfo
    .GetCultures(CultureTypes.AllCultures)
    .ToList();
```

### `CultureInfo.GetCultures`

O método:

```csharp
CultureInfo.GetCultures(...)
```

retorna as culturas de acordo com o tipo solicitado.

Na aula foi utilizado:

```csharp
CultureTypes.AllCultures
```

para recuperar todas as culturas disponíveis nesse conjunto.

Depois é utilizado:

```csharp
.ToList()
```

para transformar o resultado em uma lista.

Conceitualmente:

```text
CultureInfo.GetCultures(...)
          ↓
Coleção de culturas
          ↓
ToList()
          ↓
List<CultureInfo>
```

Durante o teste apresentado na aula, essa lista possuía centenas de culturas suportadas pelo ambiente executado pelo instrutor.

O importante não é decorar essa quantidade, mas entender que a lista será usada como referência para verificar se a cultura recebida existe.

---

## 4. Renomeando a cultura recebida para RequestedCulture

A variável que representa o valor recuperado do header passa a ter um nome mais explícito:

```csharp
var requestedCulture = context.Request.Headers.AcceptLanguage.FirstOrDefault();
```

`requestedCulture` representa a cultura que foi solicitada por quem chamou a API.

Exemplos:

```text
pt-BR
pt-PT
fr-BE
fr
en-US
```

O nome ajuda a diferenciar duas coisas importantes:

```text
requestedCulture
= valor recebido da requisição

supportedLanguages
= culturas conhecidas pelo .NET
```

Agora a tarefa é descobrir se `requestedCulture` está presente em `supportedLanguages`.

---

## 5. Validando se a cultura existe

Como `supportedLanguages` é uma lista, a aula utiliza `Exists` para procurar uma cultura cujo nome seja igual ao valor solicitado.

Exemplo:

```csharp
supportedLanguages.Exists(language =>
    language.Name.Equals(requestedCulture))
```

A expressão verifica os elementos da lista até encontrar uma cultura correspondente.

### Entendendo a lambda

Nesta expressão:

```csharp
language => language.Name.Equals(requestedCulture)
```

`language` representa cada `CultureInfo` que está sendo analisado.

Podemos imaginar o processo assim:

```text
supportedLanguages
      ↓
CultureInfo #1
      ↓
language.Name == requestedCulture ?
      ↓
CultureInfo #2
      ↓
language.Name == requestedCulture ?
      ↓
...
```

Se encontrar uma correspondência, o resultado será verdadeiro.

Caso contrário, será falso.

A aula apresenta essa busca no contexto dos recursos de consulta de coleções que já vinham sendo estudados, utilizando a expressão lambda para realizar a verificação.

---

## 6. CultureInfo.Name e a comparação com o header

Cada objeto `CultureInfo` possui uma propriedade `Name`.

Exemplos conceituais:

```csharp
language.Name
```

pode representar valores como:

```text
pt-BR
pt-PT
fr
fr-BE
en-US
```

Por isso a comparação feita é:

```csharp
language.Name.Equals(requestedCulture)
```

Ou seja:

```text
Nome da cultura suportada
          ==
Cultura solicitada no header
```

Somente se houver correspondência o middleware deve substituir a cultura default pela cultura recebida.

---

## 7. Mantendo a cultura default

O middleware continua criando primeiro uma cultura padrão.

Na aula, o idioma default continua sendo inglês:

```csharp
var cultureInfo = new CultureInfo("en");
```

Isso garante que a aplicação tenha uma cultura válida mesmo quando o header estiver ausente ou possuir um valor não reconhecido.

Depois, somente se a cultura solicitada passar pelas validações, o valor é sobrescrito:

```csharp
cultureInfo = new CultureInfo(requestedCulture);
```

A lógica é:

```text
Começa com:
CultureInfo("en")

        ↓
requestedCulture é válida?

      Sim                Não
       ↓                  ↓
Substitui             Mantém "en"
```

Isso está conectado ao comportamento estudado nas aulas anteriores sobre o arquivo `.resx` default.

Se a aplicação permanecer em inglês e não existir um resource específico com a tag correspondente, o arquivo de resources sem tag funciona como fallback.

---

## 8. String.IsNullOrWhiteSpace antes da busca

A aula mantém uma validação que já existia:

```csharp
!string.IsNullOrWhiteSpace(requestedCulture)
```

Isso verifica se a cultura recebida não é:

- `null`;
- string vazia;
- composta apenas por espaços em branco.

A condição completa fica conceitualmente assim:

```csharp
if (
    !string.IsNullOrWhiteSpace(requestedCulture)
    && supportedLanguages.Exists(language =>
        language.Name.Equals(requestedCulture))
)
{
    cultureInfo = new CultureInfo(requestedCulture);
}
```

A ordem dessas validações é importante para a explicação da aula.

Primeiro verificamos algo simples e barato:

```csharp
!string.IsNullOrWhiteSpace(requestedCulture)
```

Somente depois, se necessário, fazemos a busca na lista de culturas.

---

## 9. Short-circuit com && e ||

Um dos conceitos centrais da aula é o comportamento de curto-circuito das expressões booleanas.

### `&&` — AND

Com `&&`, todas as condições precisam ser verdadeiras.

```csharp
condicao1 && condicao2
```

Para a expressão resultar em `true`:

```text
condicao1 = true
condicao2 = true
```

Porém, o C# avalia da esquerda para a direita.

Se a primeira condição já for falsa:

```text
false && qualquerCoisa
```

o resultado obrigatoriamente será falso.

Portanto, o C# não precisa avaliar a segunda parte.

No middleware:

```csharp
!string.IsNullOrWhiteSpace(requestedCulture)
&&
supportedLanguages.Exists(...)
```

se `requestedCulture` estiver vazia, a busca com `Exists` nem precisa ser executada.

Fluxo:

```text
requestedCulture é válida como string?
          ↓
        NÃO
          ↓
Resultado do && já é false
          ↓
Exists não precisa ser executado
```

Isso evita processamento desnecessário.

### `||` — OR

A aula também compara esse comportamento com `||`.

```csharp
condicao1 || condicao2
```

Para o OR ser verdadeiro, basta uma condição ser verdadeira.

Se a primeira já for verdadeira:

```text
true || qualquerCoisa
```

o resultado já está determinado.

Então a segunda condição não precisa ser testada.

Resumo:

| Operador | Quando pode parar antes? |
|---|---|
| `&&` | quando encontra `false` |
| `||` | quando encontra `true` |

Esse comportamento é chamado de **short-circuit evaluation**.

---

## 10. Teste com o Swagger

Depois da mudança, a API é executada novamente pelo Swagger.

O Swagger continua enviando um `Accept-Language` em um formato mais complexo, semelhante a:

```text
en-US,en;q=0.9
```

A aplicação executa as etapas:

```text
1. Obtém as culturas suportadas
2. Lê o Accept-Language
3. requestedCulture não está vazia
4. Procura exatamente esse valor na lista
5. Não encontra
6. Não entra no if
7. Mantém CultureInfo("en")
8. Continua o pipeline
```

A diferença em relação à aula anterior é fundamental.

Antes:

```text
Header complexo
      ↓
new CultureInfo(valor)
      ↓
Exceção
      ↓
Erro 500
```

Depois:

```text
Header complexo
      ↓
Verificação em supportedLanguages
      ↓
Não encontrado
      ↓
Mantém cultura default
      ↓
Requisição continua normalmente
```

Com isso, o Swagger volta a funcionar no cenário demonstrado.

A validação da despesa é executada e a API retorna normalmente os erros esperados, como título obrigatório e valor maior que zero, utilizando a cultura default definida pelo middleware.

---

## 11. Fluxo final do CultureMiddleware

Com as mudanças acumuladas das últimas aulas, a estrutura conceitual fica semelhante a:

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
        var supportedLanguages = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .ToList();

        var requestedCulture = context.Request.Headers
            .AcceptLanguage
            .FirstOrDefault();

        var cultureInfo = new CultureInfo("en");

        if (
            !string.IsNullOrWhiteSpace(requestedCulture)
            && supportedLanguages.Exists(language =>
                language.Name.Equals(requestedCulture))
        )
        {
            cultureInfo = new CultureInfo(requestedCulture);
        }

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}
```

### Fluxo completo

```text
Cliente
   ↓
Requisição HTTP
   ↓
Accept-Language
   ↓
CultureMiddleware
   ↓
GetCultures(AllCultures)
   ↓
requestedCulture
   ↓
String.IsNullOrWhiteSpace?
   ↓
Existe na lista de culturas?
   ↓
┌──────────────────┬─────────────────────┐
│ Sim              │ Não                 │
│                  │                     │
│ CultureInfo da   │ mantém CultureInfo  │
│ requisição       │ default "en"        │
└──────────────────┴─────────────────────┘
   ↓
CurrentCulture
CurrentUICulture
   ↓
await _next(context)
   ↓
Controller
   ↓
Use Case
   ↓
Validator
   ↓
Resource correto / fallback
```

---

## 12. Resumo

Nesta aula foi adicionada uma proteção ao `CultureMiddleware` para impedir que qualquer valor recebido no `Accept-Language` seja utilizado diretamente na construção de um `CultureInfo`.

Os principais pontos foram:

- obter as culturas disponíveis através de `CultureInfo.GetCultures`;
- utilizar `CultureTypes.AllCultures`;
- transformar o resultado em uma lista;
- armazenar o idioma solicitado em `requestedCulture`;
- verificar se a cultura existe entre as culturas conhecidas;
- comparar `CultureInfo.Name` com o valor recebido no header;
- manter inglês como cultura default;
- continuar usando `String.IsNullOrWhiteSpace` antes da busca;
- entender o short-circuit do operador `&&`;
- revisar o comportamento equivalente do operador `||`;
- impedir que o valor complexo enviado pelo Swagger seja passado diretamente para `new CultureInfo(...)`;
- restaurar o funcionamento normal da requisição pelo Swagger.

---

## 13. Visão geral

As aulas de localização construíram o fluxo progressivamente:

```text
Resource File
    ↓
Mensagens centralizadas
    ↓
Resources por cultura
    ↓
pt-BR / pt-PT / fr
    ↓
Resource default
    ↓
Fallback de idioma
    ↓
CultureMiddleware
    ↓
Accept-Language
    ↓
CurrentCulture / CurrentUICulture
    ↓
Registro no pipeline
    ↓
Testes com Postman
    ↓
Problema com header complexo do Swagger
    ↓
Validação das culturas suportadas
    ↓
Fallback seguro para cultura default
```

Com essa validação, a parte de localização e validação apresentada nesse conjunto de aulas fica mais robusta: a cultura solicitada só substitui a cultura padrão quando ela é reconhecida pela lista consultada pelo middleware.