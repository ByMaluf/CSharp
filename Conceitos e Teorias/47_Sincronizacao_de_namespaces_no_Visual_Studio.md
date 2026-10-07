# Sincronização de Namespaces no Visual Studio

## 📋 Índice

1. [Contexto da aula](#contexto-da-aula)
2. [Relação entre pastas e namespaces](#relação-entre-pastas-e-namespaces)
3. [O problema ao renomear uma pasta](#o-problema-ao-renomear-uma-pasta)
4. [Alteração manual do namespace](#alteração-manual-do-namespace)
5. [Por que isso se torna um problema em projetos maiores](#por-que-isso-se-torna-um-problema-em-projetos-maiores)
6. [Sync Namespace](#sync-namespace)
7. [Exemplo da aula](#exemplo-da-aula)
8. [Benefício prático](#benefício-prático)
9. [Resumo](#resumo)
10. [Visão Geral](#visão-geral)

---

## Contexto da aula

A aula continua utilizando o projeto `Petfolio.Application` criado nas aulas anteriores.

Dentro dele, o use case de registro de pet está organizado aproximadamente assim:

```text
Petfolio.Application
└── UseCases
    └── Pet
        └── Register
            └── RegisterPetUseCase.cs
```

A aula mostra um recurso do Visual Studio útil quando a estrutura de pastas é renomeada depois que classes já foram criadas.

---

## Relação entre pastas e namespaces

Ao criar uma classe dentro de uma estrutura de pastas, o namespace gerado acompanha essa organização.

No exemplo da aula, a classe está dentro de:

```text
UseCases/Pet/Register
```

Então o namespace segue um padrão semelhante a:

```csharp
namespace Petfolio.Application.UseCases.Pet.Register;
```

A composição apresentada na aula pode ser entendida assim:

```text
Nome do projeto
+
estrutura de pastas
↓
namespace
```

Exemplo:

```text
Petfolio.Application
└── UseCases
    └── Pet
        └── Register
```

resultando em:

```csharp
Petfolio.Application.UseCases.Pet.Register
```

---

## O problema ao renomear uma pasta

Na aula, a pasta:

```text
Pet
```

é renomeada para:

```text
Pets
```

A estrutura passa a ser:

```text
UseCases/Pets/Register
```

Porém, o namespace existente na classe continua com o nome antigo:

```csharp
namespace Petfolio.Application.UseCases.Pet.Register;
```

Ou seja, a estrutura física mudou, mas o namespace ainda não acompanha automaticamente essa alteração.

Isso quebra o padrão de organização adotado no projeto.

---

## Alteração manual do namespace

Uma possibilidade seria alterar manualmente:

```csharp
Petfolio.Application.UseCases.Pet.Register
```

para:

```csharp
Petfolio.Application.UseCases.Pets.Register
```

Em um projeto pequeno, com apenas uma classe, essa alteração é simples.

Entretanto, a aula chama atenção para o fato de que essa solução não escala bem quando existem muitas classes.

---

## Por que isso se torna um problema em projetos maiores

Com o crescimento da aplicação, a pasta de `Pets` pode passar a conter vários casos de uso:

```text
Pets
├── Register
├── Delete
├── GetById
├── Update
└── ...
```

Cada pasta pode conter uma ou mais classes.

Se a pasta pai for renomeada depois de semanas, meses ou anos de desenvolvimento, atualizar cada namespace manualmente seria trabalhoso.

Exemplo:

```text
Antes
UseCases/Pet/Register
UseCases/Pet/Delete
UseCases/Pet/GetById
UseCases/Pet/Update

Depois
UseCases/Pets/Register
UseCases/Pets/Delete
UseCases/Pets/GetById
UseCases/Pets/Update
```

Sem uma ferramenta de sincronização, seria necessário entrar em cada arquivo e alterar:

```csharp
.UseCases.Pet.
```

para:

```csharp
.UseCases.Pets.
```

---

## Sync Namespace

O Visual Studio possui uma opção chamada:

```text
Sync Namespace
```

Segundo o fluxo mostrado na aula:

```text
Botão direito no projeto
↓
Sync Namespace
```

Ao selecionar essa opção, o Visual Studio analisa a estrutura atual das pastas e identifica namespaces que não correspondem mais a essa estrutura.

Antes de aplicar as alterações, ele mostra uma prévia do que será modificado.

No exemplo:

```text
Pet
↓
Pets
```

Então:

```csharp
Petfolio.Application.UseCases.Pet.Register
```

passa para:

```csharp
Petfolio.Application.UseCases.Pets.Register
```

---

## Exemplo da aula

### Antes de renomear a pasta

```text
Petfolio.Application
└── UseCases
    └── Pet
        └── Register
            └── RegisterPetUseCase.cs
```

Namespace:

```csharp
namespace Petfolio.Application.UseCases.Pet.Register;
```

### Depois de renomear a pasta

```text
Petfolio.Application
└── UseCases
    └── Pets
        └── Register
            └── RegisterPetUseCase.cs
```

O namespace ainda estaria inicialmente como:

```csharp
namespace Petfolio.Application.UseCases.Pet.Register;
```

Após utilizar:

```text
Sync Namespace
```

fica:

```csharp
namespace Petfolio.Application.UseCases.Pets.Register;
```

---

## Benefício prático

O principal benefício apresentado é evitar alterações repetitivas quando a organização do projeto muda.

Em vez de:

```text
abrir arquivo 1
↓
alterar namespace
↓
abrir arquivo 2
↓
alterar namespace
↓
abrir arquivo 3
↓
alterar namespace
↓
...
```

podemos fazer:

```text
Renomear a pasta
↓
Sync Namespace
↓
Revisar as alterações
↓
Apply
```

O Visual Studio sincroniza os namespaces das classes afetadas.

---

## Resumo

| Conceito | Descrição |
|---|---|
| **Namespace** | Identifica logicamente onde uma classe está organizada no código |
| **Estrutura de pastas** | Na organização mostrada na aula, influencia o namespace gerado para a classe |
| **Renomear pasta** | Não significa necessariamente que namespaces antigos serão atualizados automaticamente |
| **Sync Namespace** | Recurso do Visual Studio usado para alinhar namespaces com a estrutura atual de pastas |
| **Preview** | O Visual Studio mostra as alterações antes de aplicá-las |
| **Benefício** | Evita corrigir manualmente o namespace de várias classes |

---

## Visão Geral

```text
ANTES

UseCases
└── Pet
    └── Register
        └── RegisterPetUseCase.cs

namespace Petfolio.Application.UseCases.Pet.Register


RENOMEAR PASTA

Pet
↓
Pets


ESTRUTURA NOVA

UseCases
└── Pets
    └── Register
        └── RegisterPetUseCase.cs

mas o namespace ainda pode estar como:

Petfolio.Application.UseCases.Pet.Register


SYNC NAMESPACE
↓

Petfolio.Application.UseCases.Pets.Register
```

> **Em resumo:** a aula mostra que, ao renomear pastas de um projeto, os namespaces existentes podem deixar de seguir a estrutura utilizada como padrão. Em vez de alterar cada classe manualmente, o Visual Studio oferece a opção `Sync Namespace`, que identifica os namespaces desalinhados, mostra uma prévia das mudanças e sincroniza as classes afetadas com a nova estrutura de pastas.
