# Localização da API, Resource Files, Culturas e Tags

## 📋 Índice

1. [Visão geral](#visão-geral)
2. [O que significa localizar uma API](#o-que-significa-localizar-uma-api)
3. [Idioma x localização](#idioma-x-localização)
4. [Por que diferenciar culturas](#por-que-diferenciar-culturas)
5. [Tags de cultura](#tags-de-cultura)
6. [Padrão de nomes dos Resource Files](#padrão-de-nomes-dos-resource-files)
7. [Criando arquivos para pt-BR e pt-PT](#criando-arquivos-para-pt-br-e-pt-pt)
8. [Cultura genérica](#cultura-genérica)
9. [As chaves permanecem iguais](#as-chaves-permanecem-iguais)
10. [O que realmente é traduzido](#o-que-realmente-é-traduzido)
11. [Estrutura final dos recursos](#estrutura-final-dos-recursos)
12. [Resumo](#resumo)

---

## Visão geral

Nesta aula, o projeto começa a preparar as mensagens de erro para diferentes idiomas e regiões usando **Resource Files (`.resx`)**.

Na aula anterior, as mensagens de erro foram retiradas do código e centralizadas em um arquivo de recursos. Agora, o objetivo é criar versões desse mesmo recurso para culturas diferentes, permitindo que uma mesma chave retorne textos diferentes conforme o idioma/localização utilizada pela aplicação.

A ideia central pode ser representada assim:

```text
Mesma chave
   │
   ├── ResourceErrorMessages.resx       → idioma padrão
   ├── ResourceErrorMessages.pt-BR.resx → português do Brasil
   ├── ResourceErrorMessages.pt-PT.resx → português de Portugal
   └── ResourceErrorMessages.fr.resx    → francês genérico
```

---

## O que significa localizar uma API

No contexto apresentado no curso, **localizar** significa fazer a aplicação apresentar textos e mensagens em um determinado idioma.

Isso vale tanto para interfaces quanto para respostas devolvidas por uma API.

Exemplo conceitual:

```text
Usuário com aplicação em inglês
        ↓
Mensagens em inglês

Usuário com aplicação em português
        ↓
Mensagens em português

Usuário com aplicação em francês
        ↓
Mensagens em francês
```

Portanto, localização está relacionada à capacidade de adaptar textos ao idioma e à região do usuário.

---

## Idioma x localização

A aula chama atenção para duas informações diferentes:

- o **idioma**;
- a **localização/região** em que aquele idioma é utilizado.

Por exemplo, o idioma português existe em vários países.

```text
Português
   ├── Brasil
   ├── Portugal
   ├── Angola
   ├── Cabo Verde
   └── outros
```

Mesmo sendo o mesmo idioma, a forma de escrever e utilizar determinadas palavras pode mudar entre regiões.

---

## Por que diferenciar culturas

O curso usa português do Brasil e português de Portugal para mostrar que o idioma pode ser o mesmo, mas a forma de expressão pode mudar.

Exemplo apresentado:

```text
Português do Brasil:
"Eu estou gravando esta aula."

Português de Portugal:
"Eu estou a gravar esta aula."
```

Também podem existir diferenças de vocabulário.

Por isso, em vez de tratar apenas o idioma `pt`, é possível trabalhar com culturas mais específicas, como:

```text
pt-BR → Português - Brasil
pt-PT → Português - Portugal
```

A mesma ideia vale para outros idiomas:

```text
fr-FR → Francês - França
fr-BE → Francês - Bélgica
fr-CA → Francês - Canadá
```

---

## Tags de cultura

Para representar idioma e região, são utilizadas **tags de cultura**.

Exemplos usados na aula:

| Cultura | Tag |
|---|---|
| Português do Brasil | `pt-BR` |
| Português de Portugal | `pt-PT` |
| Francês genérico | `fr` |

O ponto mais importante da aula é que essas tags devem ser usadas corretamente no nome dos arquivos de recursos.

Exemplo:

```text
ResourceErrorMessages.pt-BR.resx
```

O padrão é:

```text
NomeDoRecurso.<tag>.resx
```

---

## Padrão de nomes dos Resource Files

O arquivo localizado precisa manter exatamente o mesmo nome-base do arquivo original.

Se o recurso principal se chama:

```text
ResourceErrorMessages.resx
```

então as versões localizadas devem seguir esse padrão:

```text
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

A estrutura é:

```text
ResourceErrorMessages
        +
       "."
        +
   cultura/tag
        +
     ".resx"
```

O nome-base não deve ser alterado entre os arquivos.

---

## Criando arquivos para pt-BR e pt-PT

No projeto de **Exception**, a aula cria novos Resource Files para diferentes culturas.

Para português do Brasil:

```text
ResourceErrorMessages.pt-BR.resx
```

Para português de Portugal:

```text
ResourceErrorMessages.pt-PT.resx
```

O arquivo principal criado anteriormente continua existindo:

```text
ResourceErrorMessages.resx
```

A organização fica semelhante a:

```text
Exception
└── Resources
    ├── ResourceErrorMessages.resx
    ├── ResourceErrorMessages.pt-BR.resx
    └── ResourceErrorMessages.pt-PT.resx
```

Na explicação do curso, o cuidado com o modificador `public` é principalmente para o recurso base utilizado externamente. Os arquivos específicos de cultura não precisam repetir essa preocupação durante a criação mostrada na aula.

---

## Cultura genérica

Também é possível utilizar uma cultura sem especificar país ou região.

Exemplo:

```text
fr
```

Nesse caso, o arquivo fica:

```text
ResourceErrorMessages.fr.resx
```

Segundo a explicação da aula, isso representa um francês genérico, sem distinguir França, Bélgica, Canadá ou outra região francófona.

Comparação:

```text
fr       → francês genérico
fr-FR    → francês da França
fr-BE    → francês da Bélgica
fr-CA    → francês do Canadá
```

O instrutor recomenda preferir versões específicas quando diferenças regionais forem relevantes, mas usa `fr` como demonstração da possibilidade de uma cultura genérica.

---

## As chaves permanecem iguais

Um ponto essencial é que **as chaves devem ser as mesmas em todos os arquivos de recursos**.

Imagine uma chave como:

```text
AMOUNT_MUST_BE_GREATER_THAN_ZERO
```

Ela deve existir com o mesmo nome em todos os arquivos:

```text
ResourceErrorMessages.resx
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

O que muda é somente o valor associado à chave.

Exemplo conceitual:

```text
AMOUNT_MUST_BE_GREATER_THAN_ZERO
```

No recurso padrão:

```text
The amount must be greater than zero.
```

No `pt-BR`:

```text
O valor deve ser maior que zero.
```

No `pt-PT`, a aula usa a ideia de adaptar o vocabulário regional:

```text
O montante deve ser maior que zero.
```

Assim, a aplicação continua referenciando a mesma chave, independentemente do idioma.

---

## O que realmente é traduzido

A regra apresentada pode ser resumida assim:

```text
CHAVE  → permanece igual
VALOR  → muda conforme o idioma/cultura
```

Exemplo:

```text
TITLE_REQUIRED
```

Pode estar associado a:

```text
English:
Title is required.

pt-BR:
O título é obrigatório.

fr:
[tradução correspondente em francês]
```

Isso permite que o código continue usando algo como:

```csharp
ResourceErrorMessages.TITLE_REQUIRED
```

sem precisar saber diretamente qual texto será exibido.

---

## Estrutura final dos recursos

Ao final da aula, a estrutura conceitual fica semelhante a:

```text
ResourceErrorMessages.resx
│
├── AMOUNT_MUST_BE_GREATER_THAN_ZERO
├── EXPENSE_CANNOT_BE_FOR_THE_FUTURE
├── PAYMENT_TYPE_INVALID
├── TITLE_REQUIRED
└── UNKNOWN_ERROR

ResourceErrorMessages.pt-BR.resx
│
├── mesmas chaves
└── valores em português do Brasil

ResourceErrorMessages.pt-PT.resx
│
├── mesmas chaves
└── valores em português de Portugal

ResourceErrorMessages.fr.resx
│
├── mesmas chaves
└── valores em francês
```

A aula prepara todos esses arquivos, mas deixa o teste e o restante da configuração da localização para a próxima etapa.

---

## Resumo

Nesta aula vimos que:

- **localização** é o processo de adaptar mensagens e textos para determinado idioma/cultura;
- apenas o idioma pode não ser suficiente, porque o mesmo idioma pode variar entre regiões;
- culturas são representadas por tags como `pt-BR` e `pt-PT`;
- arquivos `.resx` localizados usam o mesmo nome-base acrescido da tag da cultura;
- exemplos de nomes:

```text
ResourceErrorMessages.resx
ResourceErrorMessages.pt-BR.resx
ResourceErrorMessages.pt-PT.resx
ResourceErrorMessages.fr.resx
```

- uma cultura como `fr` representa uma opção mais genérica, sem região específica;
- as **chaves devem permanecer iguais** entre todos os arquivos;
- apenas os **valores são traduzidos/adaptados**;
- isso permite que o código continue acessando a mesma chave enquanto a mensagem apresentada pode mudar conforme a cultura;
- a aula termina com os arquivos de inglês, português do Brasil, português de Portugal e francês preparados para serem testados na próxima aula.

## Visão Geral

```text
Código
  │
  └── ResourceErrorMessages.TITLE_REQUIRED
                 │
                 ▼
        Cultura selecionada
                 │
        ┌────────┼─────────┐
        │        │         │
      pt-BR    pt-PT      fr
        │        │         │
        ▼        ▼         ▼
     texto BR  texto PT  texto FR
```

A grande vantagem é separar a identificação da mensagem da tradução exibida ao usuário. O código trabalha com uma chave estável, enquanto os Resource Files armazenam as versões dessa mensagem para cada cultura.