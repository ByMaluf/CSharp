# Resource default, fallback de cultura e preparação para middleware

## 📋 Índice

1. [Visão geral](#visão-geral)
2. [Por que existe um arquivo de Resource sem tag](#por-que-existe-um-arquivo-de-resource-sem-tag)
3. [Arquivo default](#arquivo-default)
4. [Fallback quando não existe uma cultura específica](#fallback-quando-não-existe-uma-cultura-específica)
5. [Definindo o idioma padrão da API](#definindo-o-idioma-padrão-da-api)
6. [Relação entre a cultura da máquina e as mensagens retornadas](#relação-entre-a-cultura-da-máquina-e-as-mensagens-retornadas)
7. [Teste com o Swagger](#teste-com-o-swagger)
8. [Como o código acessa os Resources](#como-o-código-acessa-os-resources)
9. [Próximo passo: idioma vindo da requisição](#próximo-passo-idioma-vindo-da-requisição)
10. [Middleware](#middleware)
11. [Fluxo conceitual](#fluxo-conceitual)
12. [Resumo](#resumo)

---

## Visão geral

Na aula anterior foram criados arquivos de **Resource** para diferentes culturas, como:

- `ResourceErrorMessages.resx`
- `ResourceErrorMessages.pt-BR.resx`
- `ResourceErrorMessages.pt-PT.resx`
- `ResourceErrorMessages.fr.resx`

Nesta aula, o objetivo é entender principalmente **por que o arquivo sem nenhuma tag de cultura precisa existir** e qual é o papel dele quando a aplicação não encontra um arquivo específico para o idioma solicitado.

Também é mostrado que, antes de configurar explicitamente o idioma desejado em cada requisição, a aplicação pode acabar utilizando a cultura do ambiente em que está sendo executada. A aula termina preparando o próximo passo: descobrir o idioma solicitado por meio da requisição e utilizar um **middleware** para configurar essa cultura antes de a requisição chegar ao endpoint.

---

## Por que existe um arquivo de Resource sem tag

Ao trabalhar com Resources localizados, temos um arquivo base sem nenhuma cultura no nome:

```text
ResourceErrorMessages.resx
```

E arquivos específicos para culturas:

```text
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

O arquivo sem tag funciona como o arquivo **base** para os demais.

A partir do nome dele, os arquivos específicos são criados adicionando a cultura ao final:

```text
ResourceErrorMessages.resx
        ↓
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

A ideia central da aula é:

> Sempre deve existir um arquivo de Resource sem uma cultura específica associada ao nome.

Esse arquivo será utilizado como **default**.

---

## Arquivo default

O arquivo sem tag de cultura é considerado o **Resource padrão** da aplicação.

Exemplo:

```text
ResourceErrorMessages.resx
```

Na aplicação do curso, esse arquivo contém as mensagens em inglês. Logo, o inglês foi escolhido como o idioma padrão das mensagens.

Exemplo conceitual:

| Arquivo | Cultura representada |
|---|---|
| `ResourceErrorMessages.resx` | Inglês padrão/default |
| `ResourceErrorMessages.pt-BR.resx` | Português do Brasil |
| `ResourceErrorMessages.pt-PT.resx` | Português de Portugal |
| `ResourceErrorMessages.fr.resx` | Francês genérico |

O arquivo default tem uma função importante: impedir que a aplicação dependa obrigatoriamente da existência de um `.resx` específico para toda cultura possível.

---

## Fallback quando não existe uma cultura específica

Imagine que a API receba uma requisição que espera mensagens em italiano.

Porém, o projeto possui apenas:

```text
ResourceErrorMessages.resx
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

Não existe:

```text
ResourceErrorMessages.it.resx
```

Nesse cenário, segundo o fluxo apresentado na aula, a aplicação não deve simplesmente falhar por não existir um arquivo italiano. Ela utiliza as mensagens do arquivo **default**.

Fluxo conceitual:

```text
Requisição deseja italiano
        ↓
Existe Resource para italiano?
        ↓
       Não
        ↓
Utiliza Resource default
        ↓
ResourceErrorMessages.resx
        ↓
Mensagens em inglês
```

Portanto, o Resource sem tag atua como um **fallback**.

Isso significa que uma cultura sem arquivo específico ainda pode receber mensagens válidas da API.

---

## Definindo o idioma padrão da API

O conteúdo colocado no arquivo sem tag determina, na prática mostrada no curso, qual idioma será usado como padrão.

No projeto da aula:

```text
ResourceErrorMessages.resx
```

contém mensagens em inglês.

Exemplo:

```text
TITLE_REQUIRED = The title is required.
AMOUNT_MUST_BE_GREATER_THAN_ZERO = The amount must be greater than zero.
UNKNOWN_ERROR = Unknown error.
```

Logo:

```text
Idioma default = Inglês
```

Se o projeto quisesse utilizar português como padrão, o arquivo base poderia conter os valores em português.

Nesse cenário, se o conteúdo default já fosse português do Brasil, não seria necessário manter um arquivo `pt-BR` apenas para repetir exatamente as mesmas mensagens.

A escolha do idioma padrão deve, portanto, ser consciente.

---

## Relação entre a cultura da máquina e as mensagens retornadas

Neste ponto da aula, ainda não foi feita uma configuração explícita dizendo à API qual cultura usar para cada requisição.

Por isso, o comportamento observado pode variar de acordo com o ambiente em que a aplicação está sendo executada.

O professor executa o projeto em uma máquina configurada em inglês. Como não existe um arquivo específico para a cultura inglesa da máquina, a aplicação acaba utilizando o arquivo default, que também contém mensagens em inglês.

Em outra máquina, configurada em português do Brasil, por exemplo, o resultado pode ser diferente porque existe:

```text
ResourceErrorMessages.pt-BR.resx
```

A ideia apresentada é que, neste estágio da implementação, o ambiente de execução ainda influencia qual Resource é escolhido.

Por isso, duas pessoas podem executar aparentemente o mesmo código e observar mensagens em idiomas diferentes.

---

## Teste com o Swagger

A aula faz um teste enviando uma requisição inválida pelo Swagger.

Um dos campos é enviado vazio para provocar uma falha de validação.

Fluxo simplificado:

```text
Swagger
   ↓
POST com dados inválidos
   ↓
Validator
   ↓
ValidationResult.IsValid == false
   ↓
Mensagens obtidas pelos Resources
   ↓
ExceptionFilter
   ↓
Resposta HTTP 400
```

Como a máquina utilizada na demonstração está em inglês, as mensagens de validação aparecem em inglês.

Exemplo conceitual:

```json
{
  "errors": [
    "The title is required.",
    "The amount must be greater than zero."
  ]
}
```

O ponto importante não é apenas o idioma mostrado no teste, mas entender **por que ele foi escolhido**.

---

## Como o código acessa os Resources

Um detalhe importante é que o código continua utilizando a classe base gerada a partir do Resource principal.

Por exemplo:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

Não é necessário escrever algo como:

```csharp
ResourceErrorMessagesPtBr.TITLE_REQUIRED
```

ou escolher diretamente o arquivo `.resx` específico.

O código acessa a mesma chave:

```csharp
RuleFor(expense => expense.Title)
    .NotEmpty()
    .WithMessage(ResourceErrorMessages.TITLE_REQUIRED);
```

A chave permanece a mesma em todos os arquivos:

```text
TITLE_REQUIRED
```

O que muda é o valor associado a ela.

Exemplo:

### Resource default

```text
TITLE_REQUIRED = The title is required.
```

### `pt-BR`

```text
TITLE_REQUIRED = O título é obrigatório.
```

### `pt-PT`

```text
TITLE_REQUIRED = O título é obrigatório.
```

O código não precisa saber diretamente qual `.resx` contém a tradução escolhida.

Isso mantém as validações desacopladas da tradução concreta.

---

## Próximo passo: idioma vindo da requisição

Até este ponto existe um problema importante: a API ainda não sabe explicitamente qual idioma **o cliente da requisição deseja utilizar**.

Em uma aplicação real, não é suficiente depender apenas da cultura do servidor.

Podemos ter, por exemplo:

```text
Servidor executando em inglês
```

mas receber simultaneamente:

```text
Usuário A → deseja pt-BR
Usuário B → deseja fr
Usuário C → deseja pt-PT
```

Portanto, a cultura desejada precisa fazer parte do contexto de cada requisição.

A aula introduz a ideia de enviar essa informação no **header HTTP**.

Fluxo conceitual:

```text
Cliente
   ↓
Request + informação de idioma no Header
   ↓
API
   ↓
Identifica cultura solicitada
   ↓
Seleciona Resource correspondente
   ↓
Endpoint / Use Case / Validator
```

---

## Middleware

Para processar essa informação antes de a requisição chegar ao Controller, a aula introduz o conceito de **middleware**.

Um middleware é apresentado como uma classe/componente que fica no meio do fluxo da requisição.

Representação:

```text
Cliente
   ↓
Middleware
   ↓
Controller
   ↓
Application / Use Case
```

Nesse caso específico, o middleware terá a responsabilidade de identificar o idioma da requisição e configurar a cultura que deverá ser utilizada durante aquele processamento.

Exemplo conceitual:

```text
Request
Header informa: pt-BR
        ↓
Middleware lê pt-BR
        ↓
Configura cultura da requisição
        ↓
Validator usa ResourceErrorMessages.TITLE_REQUIRED
        ↓
Resource selecionado: pt-BR
        ↓
"O título é obrigatório."
```

O middleware propriamente dito ainda não é implementado nesta aula. Ele fica preparado como assunto da próxima etapa.

---

## Fluxo conceitual

Juntando os conceitos apresentados:

```text
ResourceErrorMessages.resx
        │
        ├── default / fallback
        │
        ├── ResourceErrorMessages.pt-BR.resx
        ├── ResourceErrorMessages.pt-PT.resx
        └── ResourceErrorMessages.fr.resx
```

Quando existe uma cultura compatível:

```text
Cultura solicitada
        ↓
Existe Resource específico?
        ↓
       Sim
        ↓
Resource localizado
        ↓
Mensagem traduzida
```

Quando não existe:

```text
Cultura solicitada
        ↓
Existe Resource específico?
        ↓
       Não
        ↓
Resource default
        ↓
Mensagem padrão
```

O próximo objetivo será fazer a cultura vir da própria requisição:

```text
Cliente
   ↓
Header com idioma
   ↓
Middleware
   ↓
Define cultura
   ↓
Resources
   ↓
Controller / Use Case / Validator
   ↓
Resposta localizada
```

---

## Resumo

Nesta aula foi explicado que o arquivo `.resx` sem tag de cultura atua como o **Resource base/default** da aplicação.

```text
ResourceErrorMessages.resx
```

Os arquivos localizados derivam desse mesmo nome:

```text
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

Quando uma cultura específica não possui um arquivo correspondente, o Resource default funciona como fallback.

No projeto do curso, as mensagens do arquivo base estão em inglês, fazendo do inglês o idioma padrão utilizado nesse cenário.

Também foi mostrado que, antes de a aplicação configurar explicitamente a cultura de cada requisição, o idioma observado pode depender do ambiente em que a API está sendo executada.

O código continua consultando apenas as chaves da classe base de Resources:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

sem acessar diretamente os arquivos `pt-BR`, `pt-PT` ou `fr`.

Por fim, a aula prepara o próximo passo da localização: receber a preferência de idioma como informação adicional da requisição, por meio do header HTTP, e criar um **middleware** responsável por definir a cultura antes de o fluxo chegar ao endpoint.