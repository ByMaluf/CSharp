# NuGet Packages: gerenciamento de bibliotecas, reutilização e segurança

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [O que é NuGet](#o-que-é-nuget)
3. [Qual problema o NuGet resolve](#qual-problema-o-nuget-resolve)
4. [Exemplo com geração de QR Code](#exemplo-com-geração-de-qr-code)
5. [Compartilhamento manual de bibliotecas](#compartilhamento-manual-de-bibliotecas)
6. [Pacotes NuGet](#pacotes-nuget)
7. [O site nuget.org](#o-site-nugetorg)
8. [Informações importantes de um pacote](#informações-importantes-de-um-pacote)
9. [Compatibilidade de pacotes](#compatibilidade-de-pacotes)
10. [Publicação de pacotes](#publicação-de-pacotes)
11. [Cuidados de segurança](#cuidados-de-segurança)
12. [Resumo](#resumo)
13. [Visão Geral](#visão-geral)

---

## Contexto da aula

Nesta aula é apresentado o **NuGet**, ferramenta utilizada com frequência no ecossistema C#/.NET para distribuir e consumir bibliotecas prontas.

A ideia central é entender:

- o que é um pacote NuGet;
- qual problema ele resolve;
- como bibliotecas podem ser compartilhadas entre projetos;
- quais informações analisar antes de instalar um pacote;
- por que é importante ter cuidado com segurança.

> Na transcrição aparece diversas vezes a palavra “Nougat”, mas o nome correto da ferramenta é **NuGet**.

---

## O que é NuGet

O NuGet pode ser entendido como um:

```text
gerenciador de pacotes/bibliotecas para .NET
```

Sua função é facilitar o compartilhamento e o reaproveitamento de código dentro do ecossistema .NET.

Em vez de cada desenvolvedor precisar implementar tudo do zero, uma biblioteca pode ser empacotada, publicada e utilizada por outros projetos.

Fluxo simplificado:

```text
Desenvolvedor cria uma biblioteca
            ↓
Transforma em pacote
            ↓
Publica no NuGet
            ↓
Outros projetos instalam o pacote
            ↓
Código é reutilizado
```

---

## Qual problema o NuGet resolve

Sem um gerenciador de pacotes, compartilhar bibliotecas seria mais trabalhoso.

Imagine que alguém cria uma biblioteca útil. Para outra pessoa utilizá-la, seria possível:

```text
GitHub
ZIP
E-mail
Compartilhamento direto dos arquivos
```

Depois seria necessário baixar o projeto e adicionar manualmente a referência da biblioteca no projeto consumidor.

Isso funciona, mas não escala bem.

Quando várias pessoas em diferentes lugares precisam da mesma funcionalidade, não faz sentido cada uma:

```text
reinventar a roda
```

O NuGet centraliza esse processo.

---

## Exemplo com geração de QR Code

A aula utiliza como exemplo a necessidade de gerar um QR Code.

Objetivo:

```text
Texto
  ↓
Biblioteca de geração de QR Code
  ↓
Imagem do QR Code
```

Suponha que um desenvolvedor não saiba implementar todo o algoritmo necessário para transformar um texto em QR Code.

Outro desenvolvedor pode criar uma biblioteca que exponha uma função aproximadamente assim:

```csharp
GerarQrCode(string texto)
```

E retornar uma imagem ou representação do QR Code.

A lógica complexa fica encapsulada na biblioteca.

O consumidor precisa apenas conhecer:

```text
quais dados fornecer
como chamar a biblioteca
qual resultado esperar
```

Esse é um dos grandes benefícios de utilizar bibliotecas prontas.

---

## Compartilhamento manual de bibliotecas

Antes de pensar em NuGet, uma biblioteca poderia ser compartilhada como um projeto comum.

Exemplo:

```text
Solution
│
├── MinhaAplicacao
└── BibliotecaQrCode
```

A aplicação poderia adicionar uma referência para:

```text
BibliotecaQrCode
```

Esse conceito já apareceu anteriormente no curso com referências entre projetos como:

```text
API → Application
API → Infrastructure
Application → Domain
```

A diferença é que o NuGet permite distribuir essa biblioteca sem precisar entregar o projeto inteiro manualmente para cada pessoa.

---

## Pacotes NuGet

Um **NuGet Package** é uma forma de empacotar uma biblioteca para que ela possa ser distribuída e instalada de maneira simples.

Conceitualmente:

```text
Biblioteca
    ↓
Pacote NuGet
    ↓
Publicação em um repositório
    ↓
Instalação em outro projeto
```

Um pacote geralmente possui também documentação explicando:

- como instalar;
- como configurar;
- como utilizar;
- compatibilidade;
- exemplos de código;
- versões disponíveis.

Isso evita que cada desenvolvedor precise conhecer a implementação interna da biblioteca.

---

## O site nuget.org

A aula apresenta o site:

```text
nuget.org
```

O site funciona como um catálogo público de pacotes NuGet.

É possível pesquisar bibliotecas e visualizar informações importantes sobre cada pacote.

A aula usa o **AutoMapper** apenas como exemplo de pacote disponível na plataforma.

O objetivo neste momento não é instalar pacotes pelo site, mas entender as informações disponíveis.

---

## Informações importantes de um pacote

Ao abrir a página de um pacote, é possível encontrar informações como:

```text
nome do pacote
versão atual
documentação
quantidade de downloads
data da última atualização
licença
repositório do código
proprietário/autores
compatibilidade
```

Essas informações ajudam a avaliar se uma biblioteca parece adequada para o projeto.

### Documentação

Uma boa biblioteca deve deixar claro:

```text
como instalar
como configurar
como utilizar
quais parâmetros fornecer
qual resultado esperar
```

### Número de downloads

A quantidade de downloads pode ser um indicador de adoção.

Porém:

```text
muitos downloads ≠ garantia absoluta de segurança ou qualidade
```

É apenas um dos fatores que podem ser analisados.

### Repositório

Muitos pacotes possuem código aberto.

Quando o repositório está disponível, é possível verificar:

- código-fonte;
- histórico do projeto;
- manutenção;
- issues;
- documentação;
- responsáveis pelo projeto.

---

## Compatibilidade de pacotes

Um ponto importante apresentado na aula é que um pacote estar disponível para o ecossistema .NET não significa que ele faça sentido em qualquer tipo de projeto.

Por exemplo, uma biblioteca criada especificamente para aplicações mobile pode não ter utilidade em uma API ASP.NET Core.

Logo, antes da instalação é necessário verificar:

```text
Qual é o objetivo do pacote?
        ↓
Ele suporta o tipo de projeto que estou desenvolvendo?
        ↓
É compatível com minha versão/plataforma?
```

Exemplo conceitual:

```text
Pacote para UI Mobile
        ↓
Aplicação Mobile    ✅
API Web             provavelmente não faz sentido
```

A disponibilidade no NuGet não substitui a análise de compatibilidade.

---

## Publicação de pacotes

A aula explica que desenvolvedores também podem publicar seus próprios pacotes.

O fluxo apresentado é aproximadamente:

```text
Criar conta
    ↓
Obter uma chave/token de autenticação
    ↓
Criar o pacote
    ↓
Executar o comando de publicação
    ↓
Pacote disponível para outros desenvolvedores
```

O objetivo do token é identificar e autenticar quem está publicando o pacote.

A publicação pode ser relativamente simples e rápida.

Isso torna o NuGet uma ferramenta poderosa para compartilhamento de código.

---

## Cuidados de segurança

A facilidade de publicar pacotes também exige atenção.

A aula destaca que não se deve instalar qualquer pacote apenas porque ele aparece no NuGet.

Antes de adicionar uma dependência, é recomendável observar elementos como:

```text
autor/proprietário
repositório
código-fonte
documentação
quantidade de downloads
atividade do projeto
compatibilidade
licença
```

Fluxo mental recomendado:

```text
Encontrei um pacote
       ↓
Quem mantém?
       ↓
Existe repositório?
       ↓
A documentação é confiável?
       ↓
É utilizado por outras pessoas?
       ↓
É compatível com meu projeto?
       ↓
Faz sentido instalar?
```

### Dependências também são código

Quando um pacote é instalado no projeto, ele passa a fazer parte da aplicação de alguma forma.

Portanto, utilizar uma biblioteca externa significa confiar em código escrito por terceiros.

A aula reforça que a conveniência do ecossistema deve ser equilibrada com atenção à procedência do pacote.

---

## Pacote próprio vs. pacote externo

```text
Pacote próprio
├── você conhece a implementação
├── você controla as versões
└── você controla a publicação

Pacote externo
├── implementação criada por terceiros
├── manutenção depende do autor/projeto
├── pode ter atualizações independentes
└── exige análise antes da adoção
```

---

## Relação com reutilização de código

NuGet está diretamente ligado a um princípio muito comum no desenvolvimento de software:

```text
não implementar novamente algo que já possui uma solução confiável
```

Em vez de concentrar esforço em detalhes genéricos, o time pode focar nas regras específicas do próprio sistema.

Exemplo:

```text
Aplicação financeira
│
├── Regra de negócio da aplicação      ← código do projeto
├── Geração de QR Code                 ← biblioteca externa
├── Mapeamento de objetos              ← biblioteca externa
└── Outras funcionalidades reutilizáveis
```

---

## Resumo

| Conceito | Significado |
|---|---|
| NuGet | Gerenciador de pacotes utilizado no ecossistema .NET |
| NuGet Package | Biblioteca empacotada para distribuição e reutilização |
| nuget.org | Catálogo/repositório público de pacotes NuGet |
| Documentação | Explica instalação, configuração e uso do pacote |
| Compatibilidade | Define em quais tipos/versões de projeto o pacote pode ser utilizado |
| Token de publicação | Credencial utilizada para identificar quem publica um pacote |
| Open source | Permite analisar o código-fonte quando o projeto disponibiliza o repositório |

### Ideia principal

```text
Sem NuGet

Dev A cria biblioteca
      ↓
manda projeto manualmente
      ↓
Dev B adiciona referência
      ↓
Dev C precisa receber novamente
      ↓
processo difícil de escalar
```

```text
Com NuGet

Dev A cria biblioteca
      ↓
publica pacote
      ↓
NuGet
      ↓
Dev B instala
Dev C instala
Dev D instala
...
```

### Antes de instalar

```text
Não pense:

"Está no NuGet, então é seguro."
```

Pense:

```text
"Está no NuGet; agora preciso avaliar se é confiável,
compatível e adequado para o meu projeto."
```

---

## Visão Geral

```text
                         NUGET
                           │
             ┌─────────────┴─────────────┐
             │                           │
       Publicar pacote              Consumir pacote
             │                           │
             ▼                           ▼
        Biblioteca                   Projeto .NET
             │                           │
             ▼                           ▼
       Documentação              Instala dependência
             │                           │
             ▼                           ▼
        nuget.org               Utiliza funcionalidade
             │
     ┌───────┼────────┬───────────┐
     │       │        │           │
     ▼       ▼        ▼           ▼
  versão  downloads  licença   repositório

Antes de consumir:

Pacote
  ↓
Analisar objetivo
  ↓
Verificar compatibilidade
  ↓
Avaliar autor/repositório
  ↓
Ler documentação
  ↓
Avaliar segurança
  ↓
Instalar
```

> **Em resumo:** o NuGet simplifica o compartilhamento e o reaproveitamento de bibliotecas dentro do ecossistema .NET. Em vez de copiar projetos manualmente ou reimplementar funcionalidades já existentes, desenvolvedores podem publicar e instalar pacotes. Essa facilidade, porém, exige responsabilidade: antes de adicionar uma dependência externa, é importante analisar documentação, compatibilidade, procedência e segurança do pacote.