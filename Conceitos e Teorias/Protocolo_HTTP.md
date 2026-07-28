````markdown
# Comunicação entre Cliente e Servidor — HTTP e HTTPS

## 📋 Índice

1. [Comunicação entre cliente e servidor](#comunicação-entre-cliente-e-servidor)
2. [O que é HTTP?](#o-que-é-http)
3. [Fluxo de uma comunicação HTTP](#fluxo-de-uma-comunicação-http)
4. [URL](#url)
5. [Headers](#headers)
6. [Body](#body)
7. [Diferença entre Headers e Body](#diferença-entre-headers-e-body)
8. [Métodos HTTP](#métodos-http)
9. [Resposta do servidor](#resposta-do-servidor)
10. [HTTP e HTTPS](#http-e-https)
11. [Resumo](#resumo)

---

## Comunicação entre cliente e servidor

Uma aplicação normalmente possui dois participantes principais:

- **cliente**;
- **servidor**.

O cliente pode ser:

- um site;
- um aplicativo Android;
- um aplicativo iOS;
- um programa Desktop;
- outra API;
- qualquer sistema capaz de realizar uma requisição.

O servidor é responsável por receber essas solicitações, processá-las e devolver uma resposta.

Exemplo:

```text
Aplicativo, site ou outra API
              ↓
          Requisição
              ↓
             API
              ↓
         Processamento
              ↓
           Resposta
              ↓
Aplicativo, site ou outra API
```

Para que sistemas desenvolvidos com diferentes linguagens e tecnologias consigam se comunicar, é necessário seguir um padrão.

Um dos principais padrões utilizados nessa comunicação é o **HTTP**.

---

## O que é HTTP?

**HTTP** significa **Hypertext Transfer Protocol**, ou **Protocolo de Transferência de Hipertexto**.

É um protocolo que define regras para a comunicação entre clientes e servidores.

Por meio do HTTP, um cliente pode:

- solicitar informações;
- enviar dados;
- cadastrar recursos;
- atualizar informações;
- excluir registros;
- receber respostas do servidor.

O protocolo permite que sistemas desenvolvidos em tecnologias diferentes se comuniquem.

Por exemplo, uma API construída em C# pode receber requisições de aplicações desenvolvidas em:

- Java;
- JavaScript;
- TypeScript;
- Kotlin;
- Swift;
- Python;
- React Native.

A API não precisa conhecer a linguagem utilizada pelo cliente. Ela precisa apenas receber uma requisição que siga corretamente o protocolo HTTP e os contratos definidos.

---

## Fluxo de uma comunicação HTTP

A comunicação HTTP normalmente segue três etapas principais.

### 1. O cliente envia uma requisição

O cliente solicita uma informação ou pede que uma operação seja realizada.

Exemplo:

```text
Consultar um usuário
Cadastrar um produto
Atualizar uma senha
Excluir uma conta
```

---

### 2. O servidor processa a requisição

Ao receber a solicitação, o servidor pode:

- validar os dados;
- verificar se o recurso existe;
- identificar o usuário;
- conferir as permissões;
- aplicar regras de negócio;
- consultar o banco de dados;
- comunicar-se com outros serviços.

---

### 3. O servidor devolve uma resposta

Depois de processar a requisição, o servidor devolve uma resposta ao cliente.

Essa resposta pode conter:

- os dados solicitados;
- uma confirmação de sucesso;
- uma mensagem de erro;
- a informação de que o recurso não existe;
- a informação de que o usuário não possui permissão.

Fluxo simplificado:

```text
Cliente
   ↓
Requisição HTTP
   ↓
Servidor
   ↓
Processamento
   ↓
Resposta HTTP
   ↓
Cliente
```

---

## URL

Para acessar um recurso disponibilizado por uma API, o cliente realiza uma requisição para uma **URL**.

**URL** significa **Uniform Resource Locator**, ou **Localizador Uniforme de Recursos**.

Ela representa o endereço utilizado para localizar um recurso na rede.

Exemplo:

```text
https://api.exemplo.com/usuarios
```

Nesse exemplo:

- `https` representa o protocolo;
- `api.exemplo.com` representa o domínio;
- `/usuarios` representa o recurso acessado.

Uma URL também pode identificar um recurso específico:

```text
https://api.exemplo.com/usuarios/10
```

Nesse caso, o número `10` pode representar o identificador de um usuário.

---

## Headers

Os **Headers**, ou cabeçalhos, carregam informações complementares sobre a requisição ou a resposta.

Eles normalmente são organizados no formato:

```text
Chave: valor
```

Exemplos:

```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer token
Accept-Language: pt-BR
```

Os Headers podem informar:

- o formato dos dados enviados;
- o formato esperado na resposta;
- o idioma desejado;
- dados de autenticação;
- informações sobre o cliente;
- configurações relacionadas à requisição.

Por exemplo, o cliente pode indicar que deseja receber uma resposta em JSON:

```http
Accept: application/json
```

Ou pode informar o idioma esperado:

```http
Accept-Language: pt-BR
```

> Os Headers normalmente contêm metadados e informações complementares sobre a comunicação.

---

## Body

O **Body**, ou corpo da mensagem, contém os dados principais enviados pelo cliente ao servidor.

Por exemplo, ao cadastrar um usuário, os dados podem ser enviados no Body:

```json
{
  "nome": "João da Silva",
  "email": "joao@email.com",
  "telefone": "67999999999"
}
```

O Body pode conter informações como:

- nome;
- e-mail;
- senha;
- telefone;
- endereço;
- dados de um produto;
- informações de pagamento;
- conteúdo de um documento.

O formato mais utilizado em APIs Web é o **JSON**, mas outros formatos também podem ser utilizados, como:

- XML;
- formulário;
- texto;
- arquivos;
- dados binários.

> O Body contém os dados principais que serão processados pela API.

---

## Diferença entre Headers e Body

Embora Headers e Body façam parte da requisição, eles possuem objetivos diferentes.

| Parte da requisição | Função |
|---|---|
| Headers | Enviar informações complementares e metadados |
| Body | Enviar os dados principais da operação |

Exemplo de cadastro de usuário:

```http
POST /usuarios HTTP/1.1
Host: api.exemplo.com
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

- `Content-Type` e `Authorization` estão nos Headers;
- `nome`, `email` e `senha` estão no Body.

---

## Métodos HTTP

Os métodos HTTP indicam qual ação o cliente deseja realizar sobre um recurso.

Não basta informar apenas a URL. Também é necessário informar o método da requisição.

Os principais métodos são:

### GET

Utilizado para consultar ou recuperar informações.

Exemplo:

```http
GET /usuarios/10
```

Objetivo:

```text
Consultar o usuário de identificador 10
```

---

### POST

Utilizado normalmente para criar um novo recurso.

Exemplo:

```http
POST /usuarios
```

Objetivo:

```text
Cadastrar um novo usuário
```

---

### PUT

Utilizado normalmente para substituir ou atualizar completamente um recurso.

Exemplo:

```http
PUT /usuarios/10
```

Objetivo:

```text
Atualizar completamente o usuário de identificador 10
```

---

### PATCH

Utilizado para atualizar parcialmente um recurso.

Exemplo:

```http
PATCH /usuarios/10
```

Objetivo:

```text
Alterar apenas algumas informações do usuário
```

Por exemplo, alterar somente o e-mail:

```json
{
  "email": "novoemail@email.com"
}
```

---

### DELETE

Utilizado para excluir um recurso.

Exemplo:

```http
DELETE /usuarios/10
```

Objetivo:

```text
Excluir o usuário de identificador 10
```

---

### Comparação dos métodos

| Método | Ação principal |
|---|---|
| GET | Consultar um recurso |
| POST | Criar um recurso |
| PUT | Atualizar ou substituir completamente um recurso |
| PATCH | Atualizar parcialmente um recurso |
| DELETE | Excluir um recurso |

Exemplo geral:

```text
GET    /usuarios      → listar usuários
GET    /usuarios/10   → consultar um usuário
POST   /usuarios      → cadastrar um usuário
PUT    /usuarios/10   → atualizar completamente um usuário
PATCH  /usuarios/10   → atualizar parcialmente um usuário
DELETE /usuarios/10   → excluir um usuário
```

---

## Resposta do servidor

Assim como a requisição, a resposta HTTP também pode possuir:

- Headers;
- Body;
- código de status.

Exemplo de resposta:

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

O código de status indica o resultado da operação.

Exemplos:

| Código | Significado |
|---|---|
| `200 OK` | Requisição processada com sucesso |
| `201 Created` | Recurso criado com sucesso |
| `400 Bad Request` | Requisição inválida |
| `401 Unauthorized` | Usuário não autenticado |
| `403 Forbidden` | Usuário sem permissão |
| `404 Not Found` | Recurso não encontrado |
| `500 Internal Server Error` | Erro interno no servidor |

---

## HTTP e HTTPS

O HTTP, sozinho, não garante que os dados transmitidos estejam criptografados.

Quando é utilizada uma comunicação segura, utiliza-se o **HTTPS**.

**HTTPS** significa:

```text
Hypertext Transfer Protocol Secure
```

A estrutura da comunicação continua sendo baseada em HTTP, mas os dados são protegidos durante o transporte.

### HTTP

```text
Cliente
   ↓
Dados sem proteção criptográfica
   ↓
Servidor
```

### HTTPS

```text
Cliente
   ↓
Dados criptografados
   ↓
Servidor
```

O HTTPS ajuda a proteger informações como:

- senhas;
- tokens de acesso;
- dados pessoais;
- informações bancárias;
- dados enviados em formulários;
- respostas retornadas pelo servidor.

---

### SSL e TLS

É comum ouvir que o HTTPS utiliza **SSL**, sigla para **Secure Sockets Layer**.

Entretanto, atualmente, o protocolo utilizado é principalmente o **TLS — Transport Layer Security**.

O TLS é o sucessor do SSL e oferece mecanismos mais modernos de segurança.

Em resumo:

| Tecnologia | Situação |
|---|---|
| SSL | Protocolo antigo e obsoleto |
| TLS | Protocolo moderno utilizado atualmente |
| HTTPS | HTTP protegido por TLS |

Mesmo que muitas pessoas ainda utilizem o termo “certificado SSL”, tecnicamente a comunicação moderna utiliza TLS.

---

## Estrutura de uma requisição HTTP

Uma requisição pode ser representada da seguinte forma:

```text
Requisição HTTP
├── Método
├── URL
├── Headers
└── Body
```

Exemplo:

```http
POST /usuarios HTTP/1.1
Host: api.exemplo.com
Content-Type: application/json
Authorization: Bearer token
```

```json
{
  "nome": "Carlos",
  "email": "carlos@email.com",
  "senha": "SenhaSegura123"
}
```

Nesse exemplo:

- o método é `POST`;
- a URL é `/usuarios`;
- os Headers informam o tipo de conteúdo e a autenticação;
- o Body contém os dados do usuário.

---

## Resumo

| Conceito | Descrição |
|---|---|
| Cliente | Sistema que envia uma requisição |
| Servidor | Sistema que recebe, processa e responde |
| HTTP | Protocolo utilizado na comunicação entre cliente e servidor |
| HTTPS | HTTP com comunicação criptografada |
| URL | Endereço utilizado para acessar um recurso |
| Requisição | Solicitação enviada pelo cliente |
| Resposta | Resultado devolvido pelo servidor |
| Header | Informação complementar ou metadado |
| Body | Dados principais enviados ou recebidos |
| Método HTTP | Indica a ação que será executada |
| GET | Consultar dados |
| POST | Criar um recurso |
| PUT | Atualizar completamente um recurso |
| PATCH | Atualizar parcialmente um recurso |
| DELETE | Excluir um recurso |
| TLS | Protocolo moderno utilizado para proteger a comunicação |
| SSL | Antecessor do TLS, atualmente obsoleto |

---

## Fluxo geral da comunicação

```text
Cliente
   ↓
Método + URL + Headers + Body
   ↓
Requisição HTTP ou HTTPS
   ↓
API
   ↓
Validação dos dados
   ↓
Autenticação e autorização
   ↓
Regras de negócio
   ↓
Banco de dados ou serviço externo
   ↓
Código de status + Headers + Body
   ↓
Resposta HTTP ou HTTPS
   ↓
Cliente
```
````
