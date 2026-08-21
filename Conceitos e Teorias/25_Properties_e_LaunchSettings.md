# Properties e launchSettings.json

## 📋 Índice

1. [O que é a pasta Properties?](#o-que-é-a-pasta-properties)
2. [O que é o launchSettings.json?](#o-que-é-o-launchsettingsjson)
3. [Debug x Release](#debug-x-release)
4. [Profiles](#profiles)
5. [Principais configurações do launchSettings.json](#principais-configurações-do-launchsettingsjson)
6. [HTTP, HTTPS e IIS Express](#http-https-e-iis-express)
7. [Criando um perfil personalizado](#criando-um-perfil-personalizado)
8. [O que é localhost?](#o-que-é-localhost)
9. [Resumo](#resumo)

---

## O que é a pasta Properties?

Em um projeto de **ASP.NET Core Web API**, encontramos a pasta:

```text
Properties/
```

Dentro dela, normalmente existe o arquivo:

```text
launchSettings.json
```

Estrutura:

```text
Projeto
│
├── Properties
│   └── launchSettings.json
│
├── Program.cs
└── ...
```

A pasta `Properties` contém configurações relacionadas à forma como o projeto será executado durante o desenvolvimento.

---

## O que é o launchSettings.json?

O `launchSettings.json` é um arquivo no formato **JSON** que contém configurações utilizadas para executar a aplicação durante o desenvolvimento.

Exemplo simplificado:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5067"
    },
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7081"
    }
  }
}
```

Ele pode definir configurações como:

- perfis de execução;
- HTTP ou HTTPS;
- porta utilizada pela aplicação;
- abertura automática do navegador;
- URL que será aberta;
- variáveis de ambiente;
- utilização do IIS Express.

> O `launchSettings.json` é utilizado principalmente durante o desenvolvimento local e não representa a configuração de produção da aplicação.

---

## Debug x Release

Um projeto .NET pode ser compilado utilizando diferentes configurações.

As duas principais são:

```text
Debug
Release
```

### Debug

O modo **Debug** é utilizado principalmente durante o desenvolvimento.

Seu objetivo é facilitar a análise e depuração do código.

Nesse modo, são mantidas informações adicionais que permitem utilizar recursos como:

- breakpoints;
- execução linha por linha;
- inspeção de variáveis;
- análise do fluxo da aplicação;
- ferramentas de depuração.

Exemplo:

```text
Código
   ↓
Debug
   ↓
Informações adicionais para depuração
   ↓
Execução durante o desenvolvimento
```

O foco principal não é obter a maior otimização possível, mas facilitar o trabalho do desenvolvedor.

---

### Release

O modo **Release** é utilizado quando queremos uma versão otimizada da aplicação.

O compilador realiza otimizações com foco principalmente em:

- desempenho;
- eficiência;
- execução da aplicação.

De forma simplificada:

```text
Debug
   ↓
Desenvolvimento e depuração

Release
   ↓
Código otimizado para execução
```

### Comparação

| Debug | Release |
|---|---|
| Voltado para desenvolvimento | Voltado para uma versão otimizada |
| Facilita a depuração | Prioriza desempenho |
| Permite analisar melhor a execução | Aplica otimizações do compilador |
| Utilizado no dia a dia do desenvolvimento | Normalmente utilizado na preparação da aplicação para distribuição |

---

## Profiles

Dentro do `launchSettings.json`, encontramos a propriedade:

```json
"profiles"
```

Ela contém os diferentes **perfis de execução** disponíveis para o projeto.

Por exemplo:

```json
"profiles": {
  "http": {
    ...
  },
  "https": {
    ...
  },
  "IIS Express": {
    ...
  }
}
```

Esses perfis aparecem no Visual Studio e podem ser selecionados antes de executar a aplicação.

Exemplo:

```text
Profiles
│
├── HTTP
├── HTTPS
└── IIS Express
```

Cada perfil pode possuir configurações diferentes.

Por exemplo:

```text
HTTP
   ↓
http://localhost:5067

HTTPS
   ↓
https://localhost:7081
```

---

## Principais configurações do launchSettings.json

### commandName

A propriedade:

```json
"commandName": "Project"
```

define como o projeto será iniciado.

Quando utilizamos:

```json
"commandName": "Project"
```

o próprio projeto será executado diretamente.

Em outros perfis, como o IIS Express, o valor pode ser diferente.

---

### launchBrowser

A propriedade:

```json
"launchBrowser": true
```

define se o navegador será aberto automaticamente quando a aplicação for executada.

Com:

```json
"launchBrowser": true
```

temos:

```text
Executar aplicação
       ↓
API inicia
       ↓
Navegador abre automaticamente
```

Se alterarmos para:

```json
"launchBrowser": false
```

a API continuará sendo executada, mas o navegador não será aberto automaticamente.

---

### launchUrl

A propriedade:

```json
"launchUrl": "swagger"
```

define qual caminho será aberto no navegador após a aplicação iniciar.

Se a URL base for:

```text
https://localhost:7081
```

e tivermos:

```json
"launchUrl": "swagger"
```

o navegador poderá abrir:

```text
https://localhost:7081/swagger
```

Portanto:

```text
URL base
https://localhost:7081

        +

launchUrl
/swagger

        ↓

https://localhost:7081/swagger
```

---

### applicationUrl

A propriedade:

```json
"applicationUrl": "https://localhost:7081"
```

define o endereço no qual a aplicação será executada.

Ela contém:

```text
https://localhost:7081
  │        │        │
  │        │        └── Porta
  │        └─────────── Host
  └──────────────────── Protocolo
```

A porta pode ser alterada.

Por exemplo:

```json
"applicationUrl": "https://localhost:7082"
```

Nesse caso, a aplicação será executada utilizando a porta:

```text
7082
```

Porém, essa porta precisa estar disponível na máquina.

Se outra aplicação já estiver utilizando a mesma porta, poderá ocorrer um conflito.

---

### environmentVariables

Também é possível definir **variáveis de ambiente** dentro de um perfil.

Exemplo:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

Essas variáveis permitem alterar comportamentos e configurações da aplicação dependendo do ambiente utilizado.

Também podem existir variáveis personalizadas definidas pelo próprio projeto ou pela empresa.

---

## HTTP, HTTPS e IIS Express

Um projeto pode possuir diferentes perfis de execução.

### HTTP

Executa a aplicação utilizando HTTP.

Exemplo:

```text
http://localhost:5067
```

---

### HTTPS

Executa a aplicação utilizando HTTPS.

Exemplo:

```text
https://localhost:7081
```

---

### IIS Express

O **IIS Express** é outra opção disponibilizada pelo Visual Studio para hospedar e executar a aplicação durante o desenvolvimento.

Ele também possui suas próprias configurações de execução e porta.

Exemplo:

```text
IIS Express
      ↓
Servidor local
      ↓
Executa a aplicação
```

Dependendo do projeto, escolher um perfil diferente pode alterar:

- protocolo;
- porta;
- variáveis de ambiente;
- forma de inicialização;
- outras configurações.

---

## Criando um perfil personalizado

Também é possível criar novos perfis dentro do `launchSettings.json`.

Por exemplo:

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7081"
    },

    "MeuPerfil": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7082"
    }
  }
}
```

Depois de salvar o arquivo, o novo perfil ficará disponível entre as opções de execução do Visual Studio.

Isso pode ser útil quando um projeto precisa de configurações específicas, como:

- portas diferentes;
- variáveis de ambiente diferentes;
- comportamentos específicos;
- configurações determinadas pela empresa.

Na maioria dos projetos simples, entretanto, não será necessário alterar constantemente o `launchSettings.json`.

---

## O que é localhost?

O termo:

```text
localhost
```

representa **a própria máquina na qual a aplicação está sendo executada**.

Por exemplo:

```text
https://localhost:7081
```

significa que a API está disponível localmente naquele computador, utilizando a porta `7081`.

### Localhost é específico de cada dispositivo

Se a API estiver sendo executada no computador:

```text
Computador
   ↓
localhost:7081
   ↓
API
```

o `localhost` aponta para aquele computador.

Se você abrir no celular:

```text
https://localhost:7081
```

o `localhost` será o **próprio celular**, e não o computador.

Portanto:

```text
localhost no computador
        ≠
localhost no celular
```

Cada dispositivo possui seu próprio `localhost`.

---

### A API em localhost é pública?

Não.

Uma API executada apenas em:

```text
localhost
```

não está automaticamente disponível publicamente na internet.

De forma simplificada:

```text
Internet
   ✕
   │
localhost
   │
   ▼
API
```

Entretanto, existem ferramentas que permitem criar uma **URL pública que redireciona as requisições para a aplicação executada localmente**.

Isso pode ser útil, por exemplo, quando um desenvolvedor Front-end precisa acessar temporariamente a API que está sendo executada na máquina de um desenvolvedor Back-end.

Exemplo:

```text
Desenvolvedor Front-end
          ↓
      URL pública
          ↓
      Redirecionamento
          ↓
Computador do desenvolvedor Back-end
          ↓
       localhost
          ↓
          API
```

Isso permite testar a aplicação e realizar debug em determinados cenários durante o desenvolvimento.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Properties** | Pasta que contém configurações relacionadas ao projeto |
| **launchSettings.json** | Arquivo que define configurações de execução durante o desenvolvimento |
| **Debug** | Configuração voltada para desenvolvimento e depuração |
| **Release** | Configuração otimizada com foco em desempenho |
| **Profile** | Conjunto de configurações utilizadas para executar o projeto |
| **commandName** | Define como o projeto será iniciado |
| **launchBrowser** | Define se o navegador será aberto automaticamente |
| **launchUrl** | Define o caminho que será aberto ao iniciar o navegador |
| **applicationUrl** | Define endereço, protocolo e porta da aplicação |
| **environmentVariables** | Define variáveis de ambiente utilizadas pelo projeto |
| **HTTP** | Perfil utilizando o protocolo HTTP |
| **HTTPS** | Perfil utilizando o protocolo HTTPS |
| **IIS Express** | Servidor utilizado pelo Visual Studio para executar aplicações localmente |
| **localhost** | Representa a própria máquina/dispositivo |
| **Porta** | Identifica onde determinado serviço está sendo executado na máquina |

---

## Estrutura Geral

```text
Properties
    │
    └── launchSettings.json
              │
              └── profiles
                    │
                    ├── HTTP
                    │    ├── commandName
                    │    ├── launchBrowser
                    │    ├── applicationUrl
                    │    └── environmentVariables
                    │
                    ├── HTTPS
                    │    ├── commandName
                    │    ├── launchBrowser
                    │    ├── launchUrl
                    │    ├── applicationUrl
                    │    └── environmentVariables
                    │
                    └── IIS Express
                         └── Configurações próprias
```