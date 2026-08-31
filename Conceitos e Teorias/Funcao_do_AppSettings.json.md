````markdown
# appsettings.json e Configurações por Ambiente

## 📋 Índice

1. [O que é o appsettings.json?](#o-que-é-o-appsettingsjson)
2. [Para que serve o appsettings.json?](#para-que-serve-o-appsettingsjson)
3. [Configurações que mudam entre ambientes](#configurações-que-mudam-entre-ambientes)
4. [appsettings.Development.json](#appsettingsdevelopmentjson)
5. [appsettings.Production.json](#appsettingsproductionjson)
6. [Como o ambiente é definido?](#como-o-ambiente-é-definido)
7. [Criando ambientes personalizados](#criando-ambientes-personalizados)
8. [Como os arquivos appsettings são combinados](#como-os-arquivos-appsettings-são-combinados)
9. [Sobrescrita de propriedades](#sobrescrita-de-propriedades)
10. [O que colocar em cada arquivo](#o-que-colocar-em-cada-arquivo)
11. [Informações sensíveis](#informações-sensíveis)
12. [Resumo](#resumo)

---

## O que é o appsettings.json?

O `appsettings.json` é um arquivo de **configuração** utilizado pelas aplicações .NET.

Como o próprio nome indica, ele utiliza o formato **JSON**:

```json
{
  "Propriedade": "Valor"
}
```

Ele funciona como um local centralizado para armazenar configurações necessárias para o funcionamento da aplicação.

Estrutura comum:

```text
Projeto
│
├── Properties
│   └── launchSettings.json
│
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── ...
```

> Podemos pensar no `appsettings.json` como um "bloco de configurações" da aplicação.

---

## Para que serve o appsettings.json?

Durante o desenvolvimento de uma API, precisamos utilizar diversas configurações que não devem ficar diretamente espalhadas pelo código.

Por exemplo, a aplicação pode precisar saber como acessar um banco de dados:

```text
Servidor
Usuário
Senha
Banco
```

Também podemos precisar de informações para acessar serviços externos:

```text
Serviço de SMS
      ↓
URL + Chave

Serviço de E-mail
      ↓
URL + Chave

Serviço de Pagamento
      ↓
URL + Chave
```

Essas configurações podem ser armazenadas e organizadas através dos arquivos `appsettings`.

Exemplo conceitual:

```json
{
  "ServicoEmail": {
    "Url": "https://api.exemplo.com",
    "Chave": "minha-chave"
  }
}
```

---

## Configurações que mudam entre ambientes

Um dos principais motivos para utilizar arquivos de configuração é que determinadas informações podem mudar dependendo do **ambiente** no qual a aplicação está sendo executada.

Imagine dois desenvolvedores trabalhando no mesmo projeto.

### Máquina do Desenvolvedor A

```text
Banco local
├── Servidor: localhost
├── Usuário: usuarioA
└── Senha: senhaA
```

### Máquina do Desenvolvedor B

```text
Banco local
├── Servidor: localhost
├── Usuário: usuarioB
└── Senha: senhaB
```

Quando a aplicação for para produção, teremos outro banco:

```text
Produção
│
└── Banco de Dados
     ├── Outro servidor
     ├── Outro usuário
     └── Outra senha
```

Portanto, a mesma aplicação pode precisar de configurações diferentes dependendo do ambiente.

```text
Mesmo código
    │
    ├── Development → Configurações de desenvolvimento
    │
    └── Production  → Configurações de produção
```

---

## appsettings.Development.json

O arquivo:

```text
appsettings.Development.json
```

contém configurações específicas do ambiente de **desenvolvimento**.

Durante o desenvolvimento local, podemos ter:

```text
appsettings.json
        +
appsettings.Development.json
```

Esse arquivo pode possuir configurações específicas utilizadas enquanto estamos desenvolvendo e testando a aplicação.

Exemplo:

```json
{
  "ConnectionStrings": {
    "Database": "conexao-com-meu-banco-local"
  }
}
```

Assim, cada ambiente pode possuir suas próprias configurações.

---

## appsettings.Production.json

Também podemos possuir um arquivo:

```text
appsettings.Production.json
```

destinado às configurações específicas do ambiente de **produção**.

Conceitualmente:

```text
Development
     ↓
appsettings.Development.json

Production
     ↓
appsettings.Production.json
```

As configurações de produção podem ser completamente diferentes das configurações utilizadas durante o desenvolvimento.

Por exemplo:

```text
Development
├── Banco local
├── Chave de teste
└── Serviços de desenvolvimento

Production
├── Banco de produção
├── Chave de produção
└── Serviços reais
```

### Atenção

Segundo a organização apresentada na aula, informações sensíveis de produção **não devem ficar armazenadas diretamente no código-fonte**.

Portanto, um arquivo contendo credenciais reais de produção não deve simplesmente ser enviado para o repositório.

Essas informações precisam ser obtidas de forma segura durante o processo de publicação da aplicação.

---

## Como o ambiente é definido?

O .NET pode identificar em qual ambiente a aplicação está sendo executada através de uma variável de ambiente.

No `launchSettings.json`, podemos encontrar:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

Nesse exemplo, o ambiente utilizado será:

```text
Development
```

Consequentemente, a aplicação utilizará as configurações específicas de:

```text
appsettings.Development.json
```

De forma simplificada:

```text
ASPNETCORE_ENVIRONMENT
          │
          ▼
     Development
          │
          ▼
appsettings.Development.json
```

---

## Criando ambientes personalizados

Não somos obrigados a trabalhar apenas com:

```text
Development
Production
```

Podemos criar ambientes personalizados.

Por exemplo:

```json
"ASPNETCORE_ENVIRONMENT": "Ellison"
```

Nesse caso, podemos criar:

```text
appsettings.Ellison.json
```

O relacionamento será:

```text
ASPNETCORE_ENVIRONMENT = Ellison
              ↓
     appsettings.Ellison.json
```

Portanto, podemos ter:

```text
appsettings.json

appsettings.Development.json
appsettings.Ellison.json
appsettings.OutroAmbiente.json
```

Isso permite criar configurações específicas de acordo com as necessidades do projeto ou da equipe.

---

## Como os arquivos appsettings são combinados

Um ponto importante é que o `appsettings.json` e o arquivo específico do ambiente **não funcionam isoladamente**.

Durante a execução, suas configurações são combinadas.

Por exemplo, em `Development`:

```text
appsettings.json
        +
appsettings.Development.json
        ↓
Configuração utilizada pela aplicação
```

Podemos imaginar isso como uma **fusão das configurações**.

Exemplo:

### appsettings.json

```json
{
  "ServicoEmail": {
    "Url": "https://email.exemplo.com"
  }
}
```

### appsettings.Development.json

```json
{
  "ServicoEmail": {
    "Chave": "chave-desenvolvimento"
  }
}
```

Durante a execução:

```text
appsettings.json
        +
appsettings.Development.json
        ↓
```

Resultado conceitual:

```json
{
  "ServicoEmail": {
    "Url": "https://email.exemplo.com",
    "Chave": "chave-desenvolvimento"
  }
}
```

Assim, não precisamos repetir todas as configurações em todos os arquivos.

---

## Sobrescrita de propriedades

Existe uma regra importante durante essa combinação.

Se uma propriedade existir apenas no `appsettings.json`, seu valor será mantido.

Exemplo:

### appsettings.json

```json
{
  "Propriedade1": "Ellison"
}
```

### appsettings.Development.json

```json
{
}
```

Resultado:

```text
Propriedade1 = Ellison
```

---

### E se a propriedade existir nos dois arquivos?

Quando a mesma propriedade existe no arquivo geral e no arquivo específico do ambiente, o valor do ambiente **sobrescreve** o valor geral.

Exemplo:

### appsettings.json

```json
{
  "Propriedade1": "Ellison"
}
```

### appsettings.Development.json

```json
{
  "Propriedade1": "Arley"
}
```

Como estamos em `Development`, o resultado será:

```text
Propriedade1 = Arley
```

Podemos representar assim:

```text
appsettings.json
Propriedade1 = Ellison
        │
        │ sobrescrito por
        ▼
appsettings.Development.json
Propriedade1 = Arley
        │
        ▼
VALOR FINAL
Propriedade1 = Arley
```

> As configurações específicas do ambiente possuem prioridade sobre as configurações equivalentes do `appsettings.json`.

---

## O que colocar em cada arquivo

Uma forma simples de organizar é separar configurações **gerais** das configurações que **mudam conforme o ambiente**.

### appsettings.json

Pode armazenar configurações que independem do ambiente.

Exemplo:

```json
{
  "ServicoEmail": {
    "Url": "https://api.email.com"
  }
}
```

Se a URL for a mesma em todos os ambientes, não precisamos repeti-la.

---

### appsettings.Development.json

Pode armazenar configurações específicas de desenvolvimento.

Exemplo:

```json
{
  "ServicoEmail": {
    "Chave": "chave-desenvolvimento"
  }
}
```

---

### Produção

A configuração correspondente poderia possuir outra chave:

```text
ServicoEmail
    │
    └── Chave de produção
```

Assim:

```text
Configuração geral
        │
        └── URL do serviço
                +
Configuração do ambiente
        │
        └── Chave específica
```

---

## Informações sensíveis

Devemos ter atenção especial com informações como:

- senhas;
- chaves de APIs;
- credenciais de banco de dados;
- tokens;
- credenciais de serviços externos;
- informações privadas de produção.

Esses dados não devem ser colocados indiscriminadamente no código e enviados para serviços como:

```text
GitHub
GitLab
Bitbucket
Azure DevOps
```

Isso poderia expor informações importantes da aplicação.

Exemplo do que devemos evitar:

```text
Código-fonte
    │
    ├── Senha do banco
    ├── Chave de pagamento
    ├── Token de serviço
    └── Credencial de produção
            │
            ▼
          Git
            │
            ▼
       Repositório
```

### Configurações locais

Uma configuração utilizada exclusivamente no ambiente local pode representar um risco menor.

Por exemplo, uma conexão com um banco de dados local utilizado apenas para testes.

```text
Desenvolvimento local
        ↓
Banco local
        ↓
Dados de teste
```

Mesmo assim, devemos sempre avaliar se a informação realmente pode ser armazenada dessa maneira.

### Produção

Credenciais reais de produção exigem maior proteção.

Conceitualmente:

```text
Credencial de produção
         ↓
Local seguro
         ↓
Processo de publicação
         ↓
Aplicação em produção
```

A forma específica de armazenar e disponibilizar essas informações de maneira segura será tratada posteriormente.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **appsettings.json** | Arquivo principal de configurações da aplicação |
| **appsettings.Development.json** | Configurações específicas do ambiente de desenvolvimento |
| **appsettings.Production.json** | Configurações específicas do ambiente de produção |
| **ASPNETCORE_ENVIRONMENT** | Define qual ambiente está sendo utilizado |
| **Development** | Ambiente utilizado durante o desenvolvimento |
| **Production** | Ambiente utilizado pela aplicação em produção |
| **Configuração geral** | Informação que independe do ambiente |
| **Configuração específica** | Informação que muda conforme o ambiente |
| **Sobrescrita** | Uma propriedade específica do ambiente substitui a propriedade geral de mesmo nome |
| **Informação sensível** | Senhas, tokens, chaves e outras credenciais que precisam ser protegidas |

---

## Fluxo das Configurações

### Ambiente Development

```text
        appsettings.json
               │
               │
               ▼
        Configurações gerais
               │
               │
               ├───────────────┐
               │               │
               ▼               ▼
appsettings.Development.json   │
               │               │
               ▼               │
Configurações específicas      │
               │               │
               └───────┬───────┘
                       │
                       ▼
                Fusão dos arquivos
                       │
                       ▼
              Configuração final
                       │
                       ▼
                     API
```

### Prioridade

```text
appsettings.json
       │
       ▼
Valor padrão
       │
       │
       ▼
appsettings.Development.json
       │
       ▼
Possui a mesma propriedade?
       │
   ┌───┴───┐
   │       │
  Sim     Não
   │       │
   ▼       ▼
Substitui  Mantém
o valor    o valor
   │       │
   └───┬───┘
       ▼
Configuração final
```

> **Em resumo:** o `appsettings.json` centraliza configurações da aplicação. Arquivos como `appsettings.Development.json` permitem alterar essas configurações conforme o ambiente. Durante a execução, o .NET combina os arquivos e, quando uma propriedade aparece nos dois, o valor definido para o ambiente específico prevalece.
````
