# ResponseErrorJson, `required`, Construtores e Lista de Erros

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Problema atual no tratamento de erros](#problema-atual-no-tratamento-de-erros)
3. [Padronizando a resposta de erro em JSON](#padronizando-a-resposta-de-erro-em-json)
4. [Criando `ResponseErrorJson`](#criando-responseerrorjson)
5. [Usando a resposta no Controller](#usando-a-resposta-no-controller)
6. [Obrigando o preenchimento da mensagem](#obrigando-o-preenchimento-da-mensagem)
7. [`required`](#required)
8. [Construtores](#construtores)
9. [Construtor com parâmetro obrigatório](#construtor-com-parâmetro-obrigatório)
10. [Problema das validações que param no primeiro erro](#problema-das-validações-que-param-no-primeiro-erro)
11. [Próxima melhoria: lista de erros](#próxima-melhoria-lista-de-erros)
12. [Resumo](#resumo)
13. [Visão Geral](#visão-geral)

---

## Contexto da aula

Neste ponto do projeto já existem:

- endpoint para registrar uma despesa;
- `RegisterExpenseUseCase`;
- validações das informações recebidas;
- lançamento de exceções para dados inválidos;
- tratamento dessas exceções no Controller.

A aula mostra que, apesar de o código já funcionar, ainda existem melhorias importantes a serem feitas.

As duas melhorias discutidas são:

1. retornar erros em um formato estruturado em vez de texto puro;
2. preparar a aplicação para devolver **mais de um erro de validação de uma vez**.

---

## Problema atual no tratamento de erros

Até aqui, quando acontecia uma `ArgumentException`, o Controller fazia algo conceitualmente parecido com:

```csharp
return BadRequest(ex.Message);
```

E para erros inesperados:

```csharp
return StatusCode(
    StatusCodes.Status500InternalServerError,
    "Erro desconhecido"
);
```

Isso funciona, mas a resposta enviada é apenas uma `string`.

Segundo a organização adotada na aula, a API deve devolver respostas estruturadas em JSON.

Em vez de:

```text
O título é obrigatório
```

a resposta passa a ter uma estrutura semelhante a:

```json
{
  "errorMessage": "O título é obrigatório"
}
```

---

## Padronizando a resposta de erro em JSON

Como a aplicação precisa devolver uma resposta, essa classe é criada no projeto:

```text
CashFlow.Communication
```

Dentro de:

```text
Responses
```

Isso mantém a responsabilidade da camada de Communication:

```text
Requests  → dados recebidos pela API
Responses → dados devolvidos pela API
```

---

## Criando `ResponseErrorJson`

A classe criada é:

```csharp
public class ResponseErrorJson
{
    public string ErrorMessage { get; set; } = string.Empty;
}
```

Nesse primeiro momento, a resposta contém apenas uma mensagem de erro.

Estrutura conceitual:

```text
ResponseErrorJson
└── ErrorMessage : string
```

---

## Usando a resposta no Controller

Em vez de retornar diretamente `ex.Message`, o Controller passa a construir um objeto de resposta.

Exemplo conceitual:

```csharp
var errorResponse = new ResponseErrorJson
{
    ErrorMessage = ex.Message
};

return BadRequest(errorResponse);
```

Para um erro inesperado, a mesma estrutura pode ser reutilizada:

```csharp
var errorResponse = new ResponseErrorJson
{
    ErrorMessage = "Erro desconhecido"
};

return StatusCode(
    StatusCodes.Status500InternalServerError,
    errorResponse
);
```

Agora tanto o erro de validação quanto o erro interno retornam objetos estruturados.

Fluxo:

```text
Exception
   │
   ▼
catch
   │
   ▼
ResponseErrorJson
   │
   ▼
Resposta HTTP em JSON
```

---

## Obrigando o preenchimento da mensagem

Depois dessa mudança surge outro problema.

Nada impediria alguém de escrever:

```csharp
var errorResponse = new ResponseErrorJson();
```

sem definir `ErrorMessage`.

A aula apresenta duas maneiras de obrigar o preenchimento dessa informação:

1. propriedade `required`;
2. construtor com parâmetro obrigatório.

---

## `required`

Em versões modernas do C#, a propriedade pode ser declarada como obrigatória:

```csharp
public required string ErrorMessage { get; set; }
```

Com isso, este código passa a ser inválido:

```csharp
var response = new ResponseErrorJson();
```

porque `ErrorMessage` precisa receber um valor.

A instanciação passa a exigir algo semelhante a:

```csharp
var response = new ResponseErrorJson
{
    ErrorMessage = ex.Message
};
```

### Ideia principal

```text
required
↓
obriga a atribuição da propriedade
↓
reduz a possibilidade de criar um objeto incompleto
```

A aula destaca que `required` é um recurso mais novo da linguagem e que códigos antigos podem utilizar outra estratégia.

---

## Construtores

A segunda alternativa apresentada é utilizar um **construtor**.

Um construtor é uma função especial executada quando uma nova instância de uma classe é criada.

Exemplo:

```csharp
public class ResponseErrorJson
{
    public string ErrorMessage { get; set; }

    public ResponseErrorJson()
    {
    }
}
```

A chamada:

```csharp
new ResponseErrorJson();
```

cria uma nova instância e executa o construtor.

### Características apresentadas na aula

- possui o mesmo nome da classe;
- não possui tipo de retorno;
- executa no momento da instanciação;
- pode receber parâmetros;
- pode executar lógica de inicialização.

---

## Construtor com parâmetro obrigatório

Para garantir que a mensagem sempre seja informada, o construtor recebe um parâmetro:

```csharp
public class ResponseErrorJson
{
    public string ErrorMessage { get; set; }

    public ResponseErrorJson(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
```

Agora isto não é mais permitido:

```csharp
new ResponseErrorJson();
```

A instanciação precisa informar a mensagem:

```csharp
new ResponseErrorJson(ex.Message);
```

ou:

```csharp
new ResponseErrorJson("Erro desconhecido");
```

### Fluxo da construção

```text
new ResponseErrorJson(ex.Message)
              │
              ▼
ResponseErrorJson(string errorMessage)
              │
              ▼
ErrorMessage = errorMessage
```

A aula utiliza essa abordagem para mostrar também como um construtor funciona na prática.

---

## `required` vs construtor

| Abordagem | Como obriga o valor |
|---|---|
| `required` | exige que a propriedade seja preenchida durante a inicialização |
| construtor | exige que o valor seja passado como argumento ao criar o objeto |

Exemplo com `required`:

```csharp
new ResponseErrorJson
{
    ErrorMessage = ex.Message
};
```

Exemplo com construtor:

```csharp
new ResponseErrorJson(ex.Message);
```

Na aula, as duas alternativas são apresentadas como formas de impedir a criação de uma resposta de erro sem mensagem.

---

## Problema das validações que param no primeiro erro

Depois da melhoria na resposta, a aula mostra outro problema na implementação atual do `Validate`.

As validações seguem aproximadamente este fluxo:

```text
validar título
   │
   ├── inválido → throw
   │
   ▼
validar valor
   │
   ├── inválido → throw
   │
   ▼
validar data
   │
   ├── inválida → throw
   │
   ▼
validar tipo de pagamento
```

Quando uma exceção é lançada, a execução do método é interrompida imediatamente.

Por exemplo, imagine que a requisição tenha:

```text
Title       → inválido
PaymentType → inválido
```

Ao validar o título:

```csharp
throw new ArgumentException("O título é obrigatório");
```

nenhuma validação posterior é executada.

Então o usuário recebe apenas:

```json
{
  "errorMessage": "O título é obrigatório"
}
```

O erro no `PaymentType` só seria descoberto depois que a primeira informação fosse corrigida e uma nova requisição fosse enviada.

---

## Problema para a experiência do usuário

A aula compara esse comportamento a um formulário que informa um problema por tentativa:

```text
Enviar formulário
↓
"Nome inválido"
↓
corrigir
↓
Enviar novamente
↓
"Endereço inválido"
↓
corrigir
↓
Enviar novamente
↓
"E-mail inválido"
```

Isso obriga o usuário a fazer várias tentativas.

Uma experiência melhor seria:

```text
Enviar formulário
↓
receber todos os erros
↓
corrigir tudo
↓
enviar novamente
```

---

## Próxima melhoria: lista de erros

Por causa desse problema, a aula antecipa uma futura mudança.

A classe:

```text
ResponseErrorJson
```

deve evoluir conceitualmente para algo como:

```text
ResponseErrorsJson
```

em vez de armazenar apenas:

```csharp
string ErrorMessage
```

passaria a trabalhar com uma coleção de mensagens.

Exemplo conceitual:

```json
{
  "errors": [
    "O título é obrigatório",
    "O tipo de pagamento não é válido"
  ]
}
```

Mas para isso funcionar, não basta mudar apenas a Response.

Também será necessário mudar a estratégia das validações, porque os `throw` atuais interrompem a execução no primeiro erro.

A aula apenas apresenta esse problema e deixa a implementação da lista de erros para as próximas aulas.

---

## Resumo

### Melhorias realizadas

| Antes | Depois |
|---|---|
| erro retornado como `string` | erro retornado em objeto JSON |
| `BadRequest(ex.Message)` | `BadRequest(ResponseErrorJson)` |
| objeto poderia ser criado sem mensagem | `required` ou construtor força o preenchimento |

### `ResponseErrorJson`

```text
Communication
└── Responses
    └── ResponseErrorJson
```

Com construtor:

```csharp
public class ResponseErrorJson
{
    public string ErrorMessage { get; set; }

    public ResponseErrorJson(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
```

### Problema ainda existente

```text
primeira validação inválida
↓
throw
↓
execução interrompida
↓
apenas um erro retornado
```

### Próximo objetivo

```text
validar todos os campos
↓
acumular erros
↓
retornar uma lista de mensagens
```

---

## Visão Geral

```text
                 REQUEST
                    │
                    ▼
          RegisterExpenseUseCase
                    │
                    ▼
                 Validate
                    │
          ┌─────────┴─────────┐
          │                   │
       válido              inválido
          │                   │
          ▼                   ▼
     continua             Exception
                              │
                              ▼
                           catch
                              │
                              ▼
                    ResponseErrorJson
                              │
                    ┌─────────┴─────────┐
                    │                   │
               400 Bad Request    500 Internal Error
```

E a limitação atual:

```text
Erro 1 encontrado
      │
      ▼
    throw
      │
      ▼
execução termina
      │
      ▼
Erro 2 não é validado
```

> **Em resumo:** a aula melhora o contrato de erro da API transformando mensagens simples em objetos JSON, apresenta `required` e construtores como formas de impedir objetos incompletos e identifica uma limitação importante da validação atual: o primeiro `throw` interrompe o processo. Essa limitação prepara a evolução para uma resposta contendo uma lista de erros.