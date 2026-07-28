````markdown
# Requisição e Resposta de uma API

## 📋 Índice

1. [Como um cliente se comunica com uma API?](#como-um-cliente-se-comunica-com-uma-api)
2. [Estrutura de uma requisição](#estrutura-de-uma-requisição)
3. [URL base e Endpoint](#url-base-e-endpoint)
4. [Métodos HTTP](#métodos-http)
5. [Headers da requisição](#headers-da-requisição)
6. [Body da requisição](#body-da-requisição)
7. [Estrutura da resposta de uma API](#estrutura-da-resposta-de-uma-api)
8. [Status Code](#status-code)
9. [Headers da resposta](#headers-da-resposta)
10. [Body da resposta](#body-da-resposta)
11. [Processamento da resposta](#processamento-da-resposta)
12. [Fluxo completo](#fluxo-completo)
13. [Resumo](#resumo)

---

## Como um cliente se comunica com uma API?

Um site, aplicativo ou outro sistema se comunica com uma API por meio de uma **requisição HTTP**.

O cliente envia uma solicitação para um determinado recurso da API e aguarda uma resposta.

O cliente pode ser:

- um site;
- um aplicativo Android;
- um aplicativo iOS;
- um sistema Desktop;
- outra API;
- qualquer aplicação capaz de realizar requisições HTTP.

O fluxo básico é:

```text
Cliente
   ↓
Requisição
   ↓
API
   ↓
Processamento
   ↓
Resposta
   ↓
Cliente
```

A comunicação acontece por meio de elementos como:

- URL;
- método HTTP;
- Headers;
- Body;
- Status Code.

---

## Estrutura de uma requisição

Uma requisição HTTP pode ser representada da seguinte forma:

```text
Requisição
├── Método HTTP
├── URL
├── Headers
└── Body
```

Exemplo:

```http
POST /usuarios HTTP/1.1
Host: api.meusupermercado.com.br
Content-Type: application/json
Authorization: Bearer token
```

```json
{
  "nome": "Maria",
  "email": "maria@email.com",
  "senha": "SenhaSegura123"
}
```

Nesse exemplo:

- o método é `POST`;
- o recurso acessado é `/usuarios`;
- os Headers enviam informações complementares;
- o Body contém os dados do usuário.

---

## URL base e Endpoint

### URL base

A **URL base** representa o endereço principal da API.

Exemplo:

```text
https://api.meusupermercado.com.br
```

Todas as funcionalidades da API partem desse endereço.

---

### Endpoint

Um **Endpoint** é o endereço utilizado para acessar um recurso ou uma funcionalidade específica da API.

Exemplo:

```text
https://api.meusupermercado.com.br/usuarios
```

Nesse caso:

```text
URL base: https://api.meusupermercado.com.br
Recurso:   /usuarios
Endpoint:  https://api.meusupermercado.com.br/usuarios
```

Outro exemplo:

```text
https://api.meusupermercado.com.br/usuarios/10
```

O número `10` pode representar o identificador de um usuário específico.

---

### Funcionalidade e recurso

Uma API pode possuir funcionalidades como:

- cadastrar usuário;
- consultar usuário;
- atualizar usuário;
- excluir usuário;
- alterar senha;
- enviar arquivo;
- baixar documento;
- consultar produtos.

Não é necessário criar uma URL completamente diferente para cada ação.

Os métodos HTTP permitem utilizar o mesmo recurso com operações diferentes:

```text
GET    /usuarios
POST   /usuarios
PUT    /usuarios/10
DELETE /usuarios/10
```

---

## Métodos HTTP

Os métodos HTTP indicam qual operação o cliente deseja executar.

### GET

Utilizado para consultar ou recuperar informações.

```http
GET /usuarios
```

Exemplo:

```text
Listar todos os usuários
```

Outro exemplo:

```http
GET /usuarios/10
```

```text
Consultar o usuário de identificador 10
```

---

### POST

Utilizado normalmente para criar um novo recurso.

```http
POST /usuarios
```

Exemplo:

```text
Cadastrar um novo usuário
```

---

### PUT

Utilizado normalmente para atualizar ou substituir completamente um recurso existente.

```http
PUT /usuarios/10
```

Exemplo:

```text
Atualizar os dados do usuário de identificador 10
```

---

### PATCH

Utilizado para atualizar parcialmente um recurso.

```http
PATCH /usuarios/10
```

Exemplo:

```text
Alterar somente algumas informações do usuário
```

---

### DELETE

Utilizado para excluir um recurso.

```http
DELETE /usuarios/10
```

Exemplo:

```text
Excluir o usuário de identificador 10
```

---

### Comparação dos métodos

| Método | Operação principal |
|---|---|
| GET | Consultar |
| POST | Criar |
| PUT | Atualizar completamente |
| PATCH | Atualizar parcialmente |
| DELETE | Excluir |

---

## Mesma URL com métodos diferentes

Uma API pode utilizar o mesmo recurso para executar diferentes operações.

Exemplo:

```text
GET    /usuarios
POST   /usuarios
```

Embora a URL seja a mesma, a API consegue diferenciar as funcionalidades pelo método HTTP.

```text
GET /usuarios
      ↓
Listar usuários
```

```text
POST /usuarios
       ↓
Cadastrar usuário
```

Também é possível utilizar o identificador do recurso:

```text
GET    /usuarios/10
PUT    /usuarios/10
DELETE /usuarios/10
```

A combinação entre **método HTTP e URL** identifica a operação que será executada.

> Uma rota não é identificada apenas pela URL, mas pela combinação entre a URL e o método HTTP.

---

## Rotas específicas

Em algumas situações, duas operações semelhantes podem precisar de rotas diferentes.

Por exemplo:

```http
PUT /usuarios/10
```

Pode atualizar os dados gerais do usuário.

Já a alteração de senha pode utilizar:

```http
PUT /usuarios/10/senha
```

Ou:

```http
PATCH /usuarios/10/senha
```

Assim, as operações ficam claramente separadas:

```text
PUT /usuarios/10
    ↓
Atualizar dados pessoais
```

```text
PUT /usuarios/10/senha
    ↓
Alterar senha
```

---

## Headers da requisição

Os **Headers**, ou cabeçalhos, contêm informações complementares sobre a requisição.

Eles seguem normalmente a estrutura:

```text
Chave: valor
```

Exemplos:

```http
Content-Type: application/json
Accept: application/json
Accept-Language: pt-BR
Authorization: Bearer token
```

Os Headers podem informar:

- o formato do conteúdo enviado;
- o formato esperado na resposta;
- o idioma desejado;
- dados de autenticação;
- tokens de acesso;
- informações sobre o cliente;
- configurações da requisição.

---

### Exemplos de Headers

#### Content-Type

Informa o formato do conteúdo enviado no Body.

```http
Content-Type: application/json
```

Nesse caso, o cliente está informando que o Body contém um JSON.

---

#### Accept

Informa o formato de resposta esperado pelo cliente.

```http
Accept: application/json
```

---

#### Accept-Language

Informa o idioma preferencial da resposta.

```http
Accept-Language: pt-BR
```

---

#### Authorization

Envia informações de autenticação ou autorização.

```http
Authorization: Bearer token
```

Esse Header pode permitir que a API identifique o usuário e verifique suas permissões.

> Os Headers transportam metadados e informações complementares da requisição.

---

## Body da requisição

O **Body**, ou corpo da requisição, contém os dados principais enviados pelo cliente para serem processados pela API.

Exemplo de cadastro de usuário:

```json
{
  "nome": "João da Silva",
  "email": "joao@email.com",
  "telefone": "67999999999",
  "senha": "SenhaSegura123"
}
```

Esses dados podem ser utilizados para:

- cadastrar um usuário;
- atualizar um produto;
- realizar um pagamento;
- enviar uma mensagem;
- criar um pedido;
- enviar um arquivo.

---

### Headers x Body

| Elemento | Função |
|---|---|
| Headers | Informações complementares e metadados |
| Body | Dados principais da operação |

Exemplo:

```http
POST /usuarios HTTP/1.1
Content-Type: application/json
Authorization: Bearer token
```

```json
{
  "nome": "Carlos",
  "email": "carlos@email.com"
}
```

Nesse caso:

- `Content-Type` e `Authorization` estão nos Headers;
- `nome` e `email` estão no Body.

---

## Estrutura da resposta de uma API

Depois de receber e processar uma requisição, a API devolve uma resposta.

A resposta pode ser representada da seguinte forma:

```text
Resposta
├── Status Code
├── Headers
└── Body
```

Exemplo:

```http
HTTP/1.1 200 OK
Content-Type: application/json
```

```json
{
  "id": 10,
  "nome": "Maria",
  "email": "maria@email.com"
}
```

Nesse exemplo:

- `200` é o Status Code;
- `Content-Type` está nos Headers;
- os dados do usuário estão no Body.

---

## Status Code

O **Status Code**, ou código de status HTTP, indica o resultado da requisição.

Ele informa se a operação:

- foi concluída com sucesso;
- apresentou algum erro;
- não foi autorizada;
- não encontrou o recurso;
- causou um erro interno no servidor.

O Status Code é representado por um número inteiro de três dígitos.

---

### Principais grupos de Status Codes

| Faixa | Categoria |
|---|---|
| `100–199` | Respostas informativas |
| `200–299` | Sucesso |
| `300–399` | Redirecionamento |
| `400–499` | Erro causado pela requisição do cliente |
| `500–599` | Erro interno do servidor |

---

### Status Codes comuns

| Código | Significado |
|---|---|
| `200 OK` | Requisição executada com sucesso |
| `201 Created` | Recurso criado com sucesso |
| `204 No Content` | Operação concluída sem conteúdo no Body |
| `400 Bad Request` | Dados ou requisição inválidos |
| `401 Unauthorized` | Usuário não autenticado |
| `403 Forbidden` | Usuário autenticado, mas sem permissão |
| `404 Not Found` | Recurso não encontrado |
| `409 Conflict` | Conflito com o estado atual do recurso |
| `500 Internal Server Error` | Erro interno inesperado no servidor |

---

## Respostas sem Body

Nem toda resposta precisa conter um Body.

Por exemplo, ao excluir um usuário, a API pode informar apenas que a operação foi concluída.

```http
HTTP/1.1 204 No Content
```

Nesse caso:

- a operação foi executada com sucesso;
- nenhum conteúdo foi devolvido;
- o resultado é identificado pelo Status Code.

Exemplo:

```text
DELETE /usuarios/10
          ↓
Usuário excluído
          ↓
204 No Content
```

Também seria possível devolver:

```http
HTTP/1.1 200 OK
```

Com uma mensagem:

```json
{
  "mensagem": "Usuário excluído com sucesso."
}
```

Entretanto, o Body não é obrigatório em todas as respostas.

---

## Headers da resposta

Assim como a requisição, a resposta também pode possuir Headers.

Exemplo:

```http
Content-Type: application/json
Content-Length: 82
Cache-Control: no-cache
```

Os Headers da resposta podem informar:

- o formato do conteúdo;
- o tamanho da resposta;
- regras de cache;
- informações de autenticação;
- cookies;
- localização de um recurso criado;
- informações adicionais sobre o servidor.

Exemplo de criação de recurso:

```http
HTTP/1.1 201 Created
Location: /usuarios/10
Content-Type: application/json
```

O Header `Location` pode indicar onde o novo recurso pode ser acessado.

---

## Body da resposta

O **Body da resposta** contém os dados devolvidos pela API.

Exemplo de consulta:

```http
GET /usuarios/10
```

Resposta:

```json
{
  "id": 10,
  "nome": "Ana",
  "email": "ana@email.com"
}
```

O Body pode conter:

- dados de um recurso;
- listas;
- mensagens;
- resultados de consultas;
- informações de erro;
- detalhes de validação;
- links;
- arquivos.

---

### Exemplo de resposta de erro

```http
HTTP/1.1 400 Bad Request
Content-Type: application/json
```

```json
{
  "erro": "Dados inválidos.",
  "campos": {
    "email": "O e-mail informado não é válido.",
    "senha": "A senha deve possuir pelo menos 8 caracteres."
  }
}
```

Nesse caso:

- o Status Code informa que a requisição é inválida;
- o Body explica quais problemas foram encontrados.

---

## Processamento da resposta

Depois de receber a resposta, o cliente precisa interpretá-la.

O cliente normalmente realiza as seguintes etapas:

1. verifica o Status Code;
2. lê os Headers;
3. extrai os dados do Body;
4. transforma os dados em objetos da aplicação;
5. apresenta as informações ao usuário;
6. executa alguma ação com base no resultado.

Exemplo:

```text
Aplicativo solicita um usuário
             ↓
API retorna 200 e os dados
             ↓
Aplicativo lê o JSON
             ↓
Aplicativo apresenta os dados na tela
```

---

### Exemplo de sucesso

```http
GET /usuarios/10
```

Resposta:

```http
HTTP/1.1 200 OK
```

```json
{
  "id": 10,
  "nome": "Pedro"
}
```

O Front-end pode utilizar o nome retornado para apresentar:

```text
Usuário: Pedro
```

---

### Exemplo de erro

```http
GET /usuarios/999
```

Resposta:

```http
HTTP/1.1 404 Not Found
```

```json
{
  "mensagem": "Usuário não encontrado."
}
```

O Front-end pode apresentar:

```text
Não foi possível encontrar o usuário.
```

---

## Fluxo completo

```text
Cliente
   ↓
Escolhe o método HTTP
   ↓
Define a URL ou Endpoint
   ↓
Adiciona os Headers
   ↓
Adiciona o Body, quando necessário
   ↓
Envia a requisição
   ↓
API recebe a solicitação
   ↓
Valida os dados
   ↓
Verifica autenticação e autorização
   ↓
Executa as regras de negócio
   ↓
Consulta ou altera os dados
   ↓
Define o Status Code
   ↓
Adiciona os Headers da resposta
   ↓
Adiciona o Body, quando necessário
   ↓
Envia a resposta
   ↓
Cliente interpreta o Status Code
   ↓
Cliente processa o Body
   ↓
Resultado é apresentado ao usuário
```

---

## Exemplo completo de requisição e resposta

### Requisição

```http
POST /usuarios HTTP/1.1
Host: api.meusupermercado.com.br
Content-Type: application/json
Accept: application/json
Authorization: Bearer token
```

```json
{
  "nome": "Fernanda",
  "email": "fernanda@email.com",
  "senha": "SenhaSegura123"
}
```

---

### Processamento da API

```text
Receber a requisição
        ↓
Verificar o token
        ↓
Validar nome, e-mail e senha
        ↓
Verificar se o e-mail já existe
        ↓
Salvar o usuário
        ↓
Gerar a resposta
```

---

### Resposta

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /usuarios/25
```

```json
{
  "id": 25,
  "nome": "Fernanda",
  "email": "fernanda@email.com"
}
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| URL base | Endereço principal da API |
| Endpoint | Endereço utilizado para acessar um recurso |
| Método HTTP | Indica a operação desejada |
| Header da requisição | Envia informações complementares |
| Body da requisição | Contém os dados principais enviados à API |
| Status Code | Indica o resultado da requisição |
| Header da resposta | Contém metadados sobre a resposta |
| Body da resposta | Contém os dados devolvidos pela API |
| GET | Consulta informações |
| POST | Cria um recurso |
| PUT | Atualiza completamente um recurso |
| PATCH | Atualiza parcialmente um recurso |
| DELETE | Exclui um recurso |
| `200 OK` | Operação concluída com sucesso |
| `201 Created` | Recurso criado com sucesso |
| `204 No Content` | Sucesso sem conteúdo na resposta |
| `400 Bad Request` | Requisição inválida |
| `401 Unauthorized` | Usuário não autenticado |
| `403 Forbidden` | Usuário sem permissão |
| `404 Not Found` | Recurso não encontrado |
| `500 Internal Server Error` | Erro interno no servidor |

---

## Anatomia geral

```text
REQUISIÇÃO
├── Método HTTP
├── URL
├── Headers
└── Body
        ↓
       API
        ↓
RESPOSTA
├── Status Code
├── Headers
└── Body
```
````
