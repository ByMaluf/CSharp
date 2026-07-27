````markdown
# API — Interface de Programação de Aplicações

## 📋 Índice

1. [O que é uma API?](#o-que-é-uma-api)
2. [Por que não acessar o banco de dados diretamente?](#por-que-não-acessar-o-banco-de-dados-diretamente)
3. [Como uma API funciona?](#como-uma-api-funciona)
4. [Contratos de uma API](#contratos-de-uma-api)
5. [Segurança e controle de acesso](#segurança-e-controle-de-acesso)
6. [Endpoints](#endpoints)
7. [Benefícios de uma API](#benefícios-de-uma-api)
8. [Etapas para criar uma API](#etapas-para-criar-uma-api)
9. [Resumo](#resumo)

---

## O que é uma API?

Uma **API (Application Programming Interface)**, ou **Interface de Programação de Aplicações**, é uma interface que permite a comunicação e a troca de dados entre diferentes sistemas.

Ela funciona como uma intermediária entre uma aplicação cliente e os serviços necessários para executar determinada operação.

Uma API pode ser utilizada para:

- consultar informações;
- salvar dados;
- atualizar registros;
- excluir dados;
- enviar e-mails;
- gerar documentos;
- criar planilhas;
- enviar notificações;
- processar pagamentos;
- integrar serviços externos.

Em uma aplicação, o Front-end não precisa conhecer diretamente o banco de dados ou todos os serviços utilizados. Ele precisa conhecer apenas a API.

---

## Por que não acessar o banco de dados diretamente?

Uma aplicação Front-end ou Mobile não deve armazenar diretamente os dados de acesso ao banco de dados.

Exemplo de arquitetura inadequada:

```text
Aplicativo
    ↓
Banco de dados
```

Nesse cenário, informações sensíveis, como endereço do servidor, usuário e senha do banco, poderiam ficar expostas dentro da aplicação.

A forma mais segura é utilizar uma API como intermediária:

```text
Aplicativo ou site
        ↓
       API
        ↓
Banco de dados e outros serviços
```

Assim:

- o banco de dados permanece protegido;
- as credenciais não ficam armazenadas no Front-end;
- as regras de negócio permanecem centralizadas;
- o acesso aos dados pode ser controlado;
- diferentes aplicações podem utilizar a mesma estrutura.

> O Front-end não deve acessar diretamente o banco de dados. Ele envia solicitações para a API, que valida, processa e executa as operações necessárias.

---

## Como uma API funciona?

De forma simplificada, o funcionamento de uma API segue este fluxo:

```text
Cliente
   ↓
Requisição
   ↓
API
   ↓
Validação dos dados
   ↓
Autenticação e autorização
   ↓
Aplicação das regras de negócio
   ↓
Banco de dados ou serviço externo
   ↓
Resposta
   ↓
Cliente
```

O cliente pode ser:

- um site;
- uma aplicação Android;
- uma aplicação iOS;
- um programa Desktop;
- outra API;
- outro sistema externo.

Ao receber uma requisição, a API pode:

1. verificar quem realizou a solicitação;
2. validar os dados recebidos;
3. conferir se o usuário possui permissão;
4. aplicar as regras de negócio;
5. acessar o banco de dados ou outros serviços;
6. devolver uma resposta.

Se os dados forem inválidos ou o usuário não possuir permissão, a API deve rejeitar a operação e retornar um erro adequado.

---

## Contratos de uma API

Uma API estabelece **contratos** que definem como os sistemas devem se comunicar.

Esses contratos determinam:

- quais informações devem ser enviadas;
- quais informações são obrigatórias;
- qual formato os dados devem possuir;
- quais respostas podem ser retornadas;
- quais erros podem ocorrer.

Por exemplo, para criar uma conta, a API pode exigir:

```text
Nome
E-mail
Senha
```

Dependendo das regras do sistema, também podem ser solicitados:

- data de nascimento;
- CPF;
- telefone;
- endereço;
- confirmação da senha.

Se o cliente não respeitar o contrato definido, a API pode recusar a requisição.

Exemplo:

```text
Criar usuário
    ↓
Nome preenchido?
    ↓
E-mail válido?
    ↓
Senha atende aos requisitos?
    ↓
Usuário pode ser cadastrado
```

---

## Segurança e controle de acesso

Uma das principais responsabilidades de uma API é garantir a segurança da aplicação.

Antes de executar uma operação, a API pode verificar:

- quem é o usuário;
- se ele está autenticado;
- se possui permissão para acessar o recurso;
- se pode visualizar ou alterar determinado dado;
- se os dados enviados são válidos.

Dois conceitos importantes são:

### Autenticação

A **autenticação** verifica a identidade do usuário.

Em outras palavras, responde à pergunta:

> Quem é você?

Exemplos:

- login e senha;
- token de acesso;
- autenticação por outro serviço;
- certificado digital.

### Autorização

A **autorização** verifica o que o usuário pode fazer dentro do sistema.

Ela responde à pergunta:

> Você possui permissão para acessar este recurso?

Um usuário pode estar autenticado, mas não possuir autorização para executar determinada operação.

---

## Endpoints

Os **endpoints** são os pontos de entrada disponibilizados por uma API.

Cada endpoint representa uma operação ou um recurso que pode ser acessado por sistemas externos.

Exemplos de operações:

```text
Cadastrar usuário
Consultar usuário
Atualizar usuário
Excluir usuário
Alterar senha
```

Exemplo de endpoints:

```http
POST /usuarios
GET /usuarios
GET /usuarios/10
PUT /usuarios/10
DELETE /usuarios/10
```

Cada endpoint pode possuir uma responsabilidade específica:

| Endpoint | Responsabilidade |
|---|---|
| `POST /usuarios` | Cadastrar um usuário |
| `GET /usuarios` | Listar usuários |
| `GET /usuarios/10` | Consultar um usuário |
| `PUT /usuarios/10` | Atualizar um usuário |
| `DELETE /usuarios/10` | Excluir um usuário |

> O endpoint representa uma porta de entrada pela qual o cliente envia uma requisição para a API.

---

## Benefícios de uma API

### Segurança

A API evita que o Front-end acesse diretamente o banco de dados.

Isso permite:

- proteger as credenciais;
- validar as requisições;
- controlar permissões;
- limitar o acesso aos recursos;
- centralizar regras de segurança.

---

### Modularidade

As APIs facilitam a divisão da aplicação em módulos independentes.

Por exemplo, diferentes desenvolvedores podem trabalhar simultaneamente em funcionalidades como:

- cadastro de usuários;
- exclusão de contas;
- alteração de senha;
- recuperação de acesso;
- atualização de dados pessoais.

Essa separação facilita:

- manutenção;
- organização do código;
- divisão de tarefas;
- trabalho em equipe;
- evolução da aplicação.

---

### Reutilização

Uma única API pode ser utilizada por várias aplicações.

Exemplo:

```text
Aplicativo Android ─┐
Aplicativo iOS ─────┼──→ API ───→ Banco de dados
Site ───────────────┤
Sistema Desktop ────┘
```

A lógica de negócio permanece dentro da API e não precisa ser reescrita para cada plataforma.

Sem uma API, seria necessário duplicar regras e integrações em diferentes aplicações e linguagens.

---

### Escalabilidade

Uma API facilita o crescimento da aplicação.

Ela permite:

- atender diferentes tipos de clientes;
- distribuir a carga de trabalho;
- adicionar novos servidores;
- integrar novos serviços;
- criar novas aplicações sem duplicar a lógica de negócio.

Por exemplo, uma aplicação pode começar apenas no Android e, posteriormente, ganhar versões para:

- iOS;
- Web;
- Windows;
- outros sistemas.

Todas essas aplicações podem utilizar a mesma API.

---

### Flexibilidade

A API pode ser consumida por sistemas desenvolvidos em diferentes linguagens e tecnologias.

Para a API, não importa se o cliente foi desenvolvido utilizando:

- C#;
- Java;
- JavaScript;
- TypeScript;
- Kotlin;
- Swift;
- Python.

O cliente precisa apenas respeitar o contrato definido pela API.

---

### Integração com serviços externos

Uma API também pode centralizar a comunicação com serviços de terceiros.

Exemplos:

- plataformas de pagamento;
- serviços de e-mail;
- armazenamento em nuvem;
- serviços de notificações;
- geração de documentos;
- sistemas governamentais;
- outras APIs.

Assim, o Front-end não precisa conhecer diretamente cada serviço utilizado.

---

## Etapas para criar uma API

### 1. Definir o objetivo e a estrutura

Antes de desenvolver uma API, é necessário compreender:

- qual problema ela deve resolver;
- quais funcionalidades serão disponibilizadas;
- quais dados serão utilizados;
- quais regras de negócio deverão ser aplicadas;
- quais sistemas irão consumir a API.

A partir disso, é possível definir uma estrutura clara e organizada.

---

### 2. Definir os modelos de dados

Os modelos representam as informações utilizadas pela aplicação.

Exemplo de um usuário:

```text
Usuário
├── Nome
├── E-mail
├── Senha
└── Data de nascimento
```

Esses modelos ajudam a estabelecer os contratos da API.

---

### 3. Implementar os endpoints

Depois de definir a estrutura, são criados os endpoints que serão disponibilizados aos clientes.

Por exemplo:

```text
Criar usuário
Consultar usuário
Atualizar usuário
Excluir usuário
```

Cada endpoint deve possuir uma responsabilidade clara.

---

### 4. Adicionar as regras de negócio

As regras de negócio determinam como a aplicação deve se comportar.

No cadastro de um usuário, por exemplo, a API pode verificar:

- se o nome foi preenchido;
- se o e-mail é válido;
- se o e-mail já está cadastrado;
- se a senha possui o tamanho mínimo;
- se a senha contém letras maiúsculas e minúsculas;
- se os dados obrigatórios foram informados.

Somente após essas validações a operação deve ser concluída.

---

### 5. Processar e devolver a resposta

Após executar a operação, a API devolve uma resposta ao cliente.

Essa resposta pode indicar:

- sucesso;
- dados encontrados;
- cadastro realizado;
- ausência de permissão;
- dados inválidos;
- recurso não encontrado;
- erro interno.

Exemplo:

```text
Requisição válida
    ↓
Processamento concluído
    ↓
Resposta de sucesso
```

Ou:

```text
Requisição inválida
    ↓
Operação rejeitada
    ↓
Resposta de erro
```

---

## Resumo

| Conceito | Descrição |
|---|---|
| API | Interface que permite a comunicação entre diferentes sistemas |
| Cliente | Aplicação que envia uma requisição para a API |
| Requisição | Solicitação enviada pelo cliente |
| Resposta | Resultado devolvido pela API |
| Contrato | Regras que definem os dados de entrada e saída |
| Endpoint | Ponto de entrada disponibilizado pela API |
| Autenticação | Verifica quem é o usuário |
| Autorização | Verifica o que o usuário pode acessar ou executar |
| Regra de negócio | Condição que determina o comportamento da aplicação |
| Modularidade | Separação da aplicação em partes independentes |
| Escalabilidade | Capacidade de crescer e atender mais aplicações e usuários |
| Flexibilidade | Possibilidade de integrar diferentes tecnologias e plataformas |

---

## Fluxo geral de uma API

```text
Front-end ou aplicativo
          ↓
       Requisição
          ↓
         API
          ↓
Autenticação e autorização
          ↓
    Validação dos dados
          ↓
    Regras de negócio
          ↓
Banco de dados ou serviço externo
          ↓
        Resposta
          ↓
Front-end ou aplicativo
```
````
