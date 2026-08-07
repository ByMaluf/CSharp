````markdown
# Comunicação com uma API — Request e Response

## 📋 Índice

1. [Como nos comunicamos com uma API?](#como-nos-comunicamos-com-uma-api)
2. [URL e Endpoint](#url-e-endpoint)
3. [Métodos HTTP](#métodos-http)
4. [Headers da Requisição](#headers-da-requisição)
5. [Body da Requisição](#body-da-requisição)
6. [Anatomia de uma Requisição](#anatomia-de-uma-requisição)
7. [Resposta da API](#resposta-da-api)
8. [Status Code](#status-code)
9. [Headers e Body da Resposta](#headers-e-body-da-resposta)
10. [Processando a Resposta](#processando-a-resposta)
11. [Fluxo Completo](#fluxo-completo)
12. [Resumo](#resumo)

---

## Como nos comunicamos com uma API?

Para utilizar uma funcionalidade disponibilizada por uma API, uma aplicação precisa realizar uma **requisição (Request)**.

Essa aplicação pode ser:

- um site;
- um aplicativo Android;
- um aplicativo iOS;
- um sistema Desktop;
- outra API.

De forma simplificada:

```text
Cliente
   │
   │ Request
   ▼
  API
   │
   │ Response
   ▼
Cliente
```

O cliente envia uma **Request** para a API e recebe uma **Response** como resultado.

Para realizar essa comunicação, alguns elementos são importantes:

```text
REQUEST
├── URL / Endpoint
├── Método HTTP
├── Headers
└── Body

RESPONSE
├── Status Code
├── Headers
└── Body
```

---

## URL e Endpoint

Uma API possui um **endereço base**, utilizado como ponto inicial para acessar seus recursos.

Exemplo:

```text
https://meusupermercado.com.br
```

A partir desse endereço, são disponibilizados caminhos para acessar determinados recursos.

Por exemplo:

```text
https://meusupermercado.com.br/usuarios
```

Nesse caso:

```text
https://meusupermercado.com.br
              ↓
        Endereço base

/usuarios
    ↓
Recurso
```

A combinação do endereço da API com o caminho disponibilizado para determinada operação forma um **endpoint**.

Por meio dos endpoints podemos acessar funcionalidades relacionadas a:

- usuários;
- produtos;
- pedidos;
- documentos;
- arquivos;
- pagamentos;
- entre outros recursos.

---

## Métodos HTTP

Somente informar o endpoint não é suficiente.

Também precisamos informar **qual ação queremos realizar** naquele recurso.

Para isso, utilizamos os **métodos HTTP**.

Os principais são:

| Método | Ação |
|---|---|
| `GET` | Recuperar informações |
| `POST` | Criar um novo recurso |
| `PUT` | Atualizar um recurso existente |
| `DELETE` | Excluir um recurso |

Podemos utilizar o mesmo endpoint com métodos diferentes.

Exemplo:

```http
GET /usuarios
POST /usuarios
PUT /usuarios
DELETE /usuarios
```

Apesar de utilizarem `/usuarios`, cada requisição representa uma operação diferente devido ao método HTTP utilizado.

---

### GET

Utilizado para **recuperar informações**.

Exemplo:

```http
GET /usuarios
```

Pode representar:

> "Quero consultar usuários."

---

### POST

Utilizado para **criar um novo recurso**.

Exemplo:

```http
POST /usuarios
```

Pode representar:

> "Quero cadastrar um novo usuário."

---

### PUT

Utilizado para **atualizar informações existentes**.

Exemplo:

```http
PUT /usuarios
```

Pode representar:

> "Quero atualizar as informações de um usuário."

---

### DELETE

Utilizado para **excluir um recurso**.

Exemplo:

```http
DELETE /usuarios
```

Pode representar:

> "Quero excluir um usuário."

---

## Endpoint + Método HTTP

Os métodos HTTP permitem utilizar um mesmo caminho para diferentes operações.

Sem essa separação, poderíamos acabar criando URLs como:

```text
/usuarios/cadastrar
/usuarios/atualizar
/usuarios/deletar
/usuarios/consultar
```

Utilizando os métodos HTTP, podemos organizar melhor:

```text
POST    /usuarios
GET     /usuarios
PUT     /usuarios
DELETE  /usuarios
```

Assim, temos:

```text
Endpoint + Método HTTP
          ↓
Define a operação
```

---

### Quando existem operações semelhantes

Em alguns casos, podemos possuir duas funcionalidades que utilizam o mesmo método HTTP.

Por exemplo:

```text
Atualizar informações do usuário
Atualizar senha do usuário
```

As duas são atualizações e podem utilizar `PUT`.

Nesse caso, os endpoints precisam diferenciá-las:

```http
PUT /usuarios

PUT /usuarios/alterar-senha
```

Isso permite que a API saiba exatamente qual funcionalidade deve executar.

---

## Headers da Requisição

Os **Headers** são os cabeçalhos enviados junto com uma requisição.

Eles carregam informações complementares que ajudam a API a entender como aquela requisição deve ser processada.

Podem conter informações como:

- idioma;
- formato dos dados;
- formato esperado da resposta;
- informações de autenticação;
- chaves ou tokens de acesso.

Exemplo:

```http
Authorization: Bearer TOKEN
Accept-Language: pt-BR
Content-Type: application/json
```

O Header pode informar à API:

```text
Quem está fazendo a requisição?
Qual idioma deseja?
Qual o formato dos dados?
Qual tipo de resposta espera receber?
```

---

## Body da Requisição

O **Body** é o corpo da requisição.

Nele são enviados os dados necessários para executar determinada operação.

Por exemplo, para cadastrar um usuário:

```json
{
    "nome": "João Silva",
    "email": "joao@email.com",
    "telefone": "67999999999",
    "senha": "123456"
}
```

Nesse caso:

```text
Headers
    ↓
Informações complementares da requisição

Body
    ↓
Dados necessários para executar a operação
```

---

## Anatomia de uma Requisição

Uma requisição pode ser representada da seguinte forma:

```text
REQUEST
│
├── URL / Endpoint
│
├── Método HTTP
│
├── Headers
│
└── Body
```

Exemplo:

```http
POST /usuarios
```

Headers:

```http
Content-Type: application/json
Authorization: Bearer TOKEN
```

Body:

```json
{
    "nome": "João Silva",
    "email": "joao@email.com",
    "senha": "123456"
}
```

A API recebe todas essas informações e decide como processar a solicitação.

---

## Resposta da API

Depois de processar uma requisição, a API envia uma **Response (Resposta)** ao cliente.

A resposta pode possuir:

```text
RESPONSE
│
├── Status Code
├── Headers
└── Body
```

Nem toda resposta precisa possuir um Body.

Por exemplo, após excluir um usuário, a API pode simplesmente informar que a operação foi realizada com sucesso.

---

## Status Code

O **Status Code** informa o resultado da requisição.

Ele permite que o cliente saiba se a operação:

- foi realizada com sucesso;
- possui algum erro;
- não foi autorizada;
- não encontrou o recurso;
- apresentou um erro interno.

Exemplo:

```http
200 OK
```

Significa que a requisição foi processada com sucesso.

Outros exemplos:

```text
200 → Sucesso
201 → Recurso criado
204 → Sucesso sem conteúdo

400 → Requisição inválida
401 → Não autenticado
403 → Acesso proibido
404 → Recurso não encontrado

500 → Erro interno do servidor
```

O Status Code é retornado mesmo quando não existe um Body na resposta.

---

## Headers e Body da Resposta

Assim como uma Request possui Headers e Body, uma Response também pode possuir esses elementos.

### Headers

Os Headers da resposta carregam informações complementares.

Exemplo:

```http
Content-Type: application/json
```

Isso informa que o conteúdo retornado está no formato JSON.

---

### Body

O Body contém os dados retornados pela API.

Exemplo:

```json
{
    "id": 10,
    "nome": "João Silva",
    "email": "joao@email.com"
}
```

Nesse caso, a API está devolvendo informações de um usuário.

---

### Resposta sem Body

Nem toda resposta precisa retornar dados.

Exemplo:

```http
DELETE /usuarios/10
```

Após excluir o usuário, a API pode retornar apenas um Status Code indicando que a operação foi realizada corretamente.

Portanto:

```text
Status Code → sempre existe na resposta

Body → pode ou não existir
```

---

## Processando a Resposta

Depois que a API responde, cabe ao cliente interpretar a resposta recebida.

Por exemplo:

```http
GET /usuarios/10
```

A API pode responder:

```http
200 OK
```

```json
{
    "id": 10,
    "nome": "João Silva",
    "email": "joao@email.com"
}
```

O Front-end recebe essas informações e pode utilizá-las para:

- mostrar o nome do usuário;
- preencher um formulário;
- exibir informações na tela;
- armazenar temporariamente os dados;
- tomar alguma decisão dentro da aplicação.

Portanto:

```text
API fornece os dados
        ↓
Cliente interpreta os dados
        ↓
Aplicação utiliza os dados
```

---

## Fluxo Completo

O processo completo de comunicação pode ser representado assim:

```text
CLIENTE
   │
   │ 1. Define o Endpoint
   │ 2. Define o Método HTTP
   │ 3. Envia Headers
   │ 4. Envia Body, quando necessário
   ▼
┌─────────────────────────┐
│           API           │
│                         │
│ Recebe a Request        │
│ Processa a solicitação  │
│ Executa a funcionalidade│
└────────────┬────────────┘
             │
             │ 5. Status Code
             │ 6. Headers
             │ 7. Body, quando necessário
             ▼
          CLIENTE
             │
             ▼
    Processa a Response
```

De maneira resumida:

```text
Request
   ↓
API
   ↓
Processamento
   ↓
Response
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Request** | Requisição enviada pelo cliente para a API |
| **Response** | Resposta enviada pela API |
| **URL** | Endereço utilizado para acessar um recurso |
| **Endpoint** | Ponto de acesso a uma funcionalidade ou recurso da API |
| **Método HTTP** | Indica a ação que será executada |
| **GET** | Recupera informações |
| **POST** | Cria um recurso |
| **PUT** | Atualiza um recurso |
| **DELETE** | Exclui um recurso |
| **Header** | Informações complementares da requisição ou resposta |
| **Body** | Dados enviados ou recebidos |
| **Status Code** | Código que informa o resultado da requisição |

---

## Visão Geral

```text
              REQUEST
                 │
        ┌────────┴────────┐
        │                 │
     Endpoint        Método HTTP
        │                 │
        ├──── Headers ─────┤
        │                 │
        └───── Body ───────┘
                 │
                 ▼
              ┌─────┐
              │ API │
              └─────┘
                 │
                 ▼
              RESPONSE
                 │
        ┌────────┼────────┐
        │        │        │
   Status Code Headers   Body
```
````
