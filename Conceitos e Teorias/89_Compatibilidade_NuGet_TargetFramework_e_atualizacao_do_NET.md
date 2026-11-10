# Compatibilidade de pacotes NuGet, Target Framework e atualização do .NET

> **Aula 89 — Aula extra sobre versões do .NET e dependências NuGet**
>
> O instrutor esclarece dúvidas do fórum sobre instalação e atualização de pacotes no CashFlow, originalmente desenvolvido em .NET 8. A gravação também demonstra duas maneiras de migrar os projetos para .NET 9.
>
> **Contexto histórico:** as versões exibidas são as disponíveis no momento da gravação. Não representam necessariamente versões atuais ou recomendações para novos projetos. As observações técnicas complementares deste material corrigem generalizações da explicação original.

## Índice

1. Contexto e objetivo
2. Identificando o Target Framework
3. Entendendo as versões dos pacotes
4. Atualizações demonstradas no NuGet
5. Compatibilidade real: exceções importantes
6. EF Core e Pomelo: compatibilidade entre dependências
7. Atualização no nível da solution
8. LTS, STS e atualizações de manutenção
9. Duas formas de atualizar o .NET
10. Método 1: .NET Upgrade Assistant
11. Método 2: alteração manual do csproj
12. Limpeza de bin e obj
13. Exemplo de evolução da sintaxe do C#
14. Checklist de migração e diagnóstico
15. Resumo, referências e perguntas de revisão

---

## 1. Contexto e objetivo

O professor recebeu relatos de alunos que encontraram problemas ao instalar determinadas versões de pacotes NuGet no projeto da API.

O CashFlow foi criado para **.NET 8**. Posteriormente, foi lançado o .NET 9, e novos pacotes passaram a aparecer na interface de gerenciamento do Visual Studio.

A aula busca explicar:

- como identificar a versão do .NET utilizada pelo projeto;
- por que uma versão mais recente de uma biblioteca pode não ser compatível;
- como selecionar atualizações dos pacotes;
- como atualizar o framework de destino dos projetos;
- por que limpar os artefatos gerados após a mudança.

O instrutor menciona também uma aula extra sobre Swagger no .NET 9, relacionada às mudanças nas dependências e templates do ASP.NET Core.

**Ponto central:** não é suficiente escolher o pacote mais novo. É necessário verificar se ele funciona com o projeto e com suas outras dependências.

O professor chama o problema relatado de incompatibilidade de versão, e não de conflito propriamente dito. Tecnicamente, incompatibilidades de framework e conflitos entre dependências podem ocorrer em situações distintas ou simultaneamente. A transcrição não contém uma mensagem de erro específica que permita diagnosticar todos os casos relatados no fórum.

## 2. Identificando o Target Framework

No Visual Studio, o professor utiliza o projeto Infrastructure como exemplo.

### Procedimento

1. Abrir o CashFlow.
2. Dar dois cliques no projeto Infrastructure.
3. Localizar o arquivo de projeto, com extensão .csproj.
4. Conferir a propriedade TargetFramework dentro de PropertyGroup.

A configuração do projeto é:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

O valor net8.0 indica que o projeto tem o .NET 8 como framework de destino.

### TFM

TFM é a sigla de *Target Framework Moniker*: o identificador do framework para o qual o projeto é compilado.

| TFM | Framework de destino |
|---|---|
| net8.0 | .NET 8 |
| net9.0 | .NET 9 |

O instrutor verifica que outros projetos, como Domain, Communication e Exception, também foram configurados para .NET 8.

### SDK versus Target Framework

**Complemento técnico:** instalar uma versão mais recente do SDK não muda automaticamente o TargetFramework de um projeto existente.

```text
SDK instalado
    |
    v
Ferramentas de compilação e desenvolvimento
    |
    v
Projeto (.csproj)
    |
    v
TargetFramework: net8.0 ou net9.0
```

O SDK utilizado deve conseguir compilar o framework de destino. O TargetFramework informa o conjunto de APIs e o destino para os quais o projeto será construído.

Assim, podemos ter mais de um SDK instalado e manter um projeto direcionado ao .NET 8.

## 3. Entendendo as versões dos pacotes

O professor utiliza a numeração de alguns pacotes da Microsoft como referência para escolher atualizações.

Por exemplo:

```text
8.0.5
| | |
| | +-- Patch
| +---- Minor
+------ Major
```

A numeração segue a representação usual de versões major.minor.patch.

- **Major:** identifica uma geração principal do pacote, que pode incluir mudanças incompatíveis.
- **Minor:** identifica uma evolução dentro daquela geração.
- **Patch:** identifica uma versão de manutenção, normalmente com correções.

Esse esquema ajuda a compreender a diferença entre mudar de 8.0.5 para 8.0.12 e mudar de 8.x para 9.x.

### A orientação prática da aula

O professor recomenda, no contexto do CashFlow em .NET 8, manter os pacotes Microsoft relacionados ao framework na linha 8.x e atualizar as versões de manutenção disponíveis.

Essa estratégia reduz a quantidade de mudanças simultâneas e permite acompanhar o curso sem migrar o projeto inteiro.

**Mas atenção:** a numeração major de um pacote não determina, sozinha, seu framework mínimo. Isso vale inclusive para pacotes publicados pela Microsoft. A compatibilidade precisa ser verificada nos metadados do NuGet e nas dependências do pacote.

As seções seguintes preservam os exemplos da gravação e distinguem essa recomendação prática da regra técnica real.

## 4. Atualizações demonstradas no NuGet

O instrutor abre:

```text
Infrastructure
    -> botão direito
    -> Manage NuGet Packages
    -> Updates
```

Na tela, aparecem versões instaladas e versões disponíveis.

### Versões observadas na gravação

| Pacote | Versão instalada | Versão mostrada ou selecionada |
|---|---|---|
| System.IdentityModel.Tokens.Jwt | 7.5.2 | 8.3.1 |
| Microsoft.EntityFrameworkCore | 8.0.5 | 9.0.1 mostrada; 8.0.12 selecionada para continuar na linha 8 |
| Microsoft.Extensions.Configuration.Binder | 8.0.1 | 8.0.2 selecionada para continuar na linha 8 |

Os números são registros históricos do vídeo, não a versão mais recente disponível atualmente.

### Exemplo: Entity Framework Core

O professor observa que está utilizando EF Core 8.0.5 e que a interface apresenta uma versão 9.0.1.

Para conservar o conjunto de dependências da linha 8, ele abre a lista de versões, escolhe 8.0.12 e clica em Update.

```text
EF Core 8.0.5
      |
      | Atualização dentro da linha 8
      v
EF Core 8.0.12
```

A escolha é coerente com a estratégia de acompanhar a versão utilizada no curso.

### Exemplo: Configuration Binder

O instrutor repete o procedimento para Microsoft.Extensions.Configuration.Binder, passando da versão 8.0.1 para a 8.0.2.

### Exemplo: JWT

O pacote System.IdentityModel.Tokens.Jwt aparece na versão 7.5.2, com 8.3.1 disponível.

O professor indica que essa atualização pode ser realizada. Entretanto, a compatibilidade também depende das demais bibliotecas de autenticação e das mudanças de API.

**Observação:** não devemos inferir que qualquer atualização major seja automaticamente segura só porque um pacote não segue a numeração do .NET.

## 5. Compatibilidade real: exceções importantes

Nesta seção, complementamos tecnicamente a explicação do vídeo.

O professor associa versões 8.x ao .NET 8 e versões 9.x ao .NET 9. Isso é uma heurística de versionamento, e não uma regra universal.

O próprio instrutor cita uma exceção: Microsoft.NET.Test.Sdk, que utiliza versões como 17.10 e 17.12, sem que isso signifique a existência de um .NET 17 correspondente.

### Exemplos verificados em documentação oficial e no NuGet

| Pacote e versão | Framework de destino relevante |
|---|---|
| Microsoft.EntityFrameworkCore 8.x | .NET 8 |
| Microsoft.EntityFrameworkCore 9.x | .NET 8 |
| Microsoft.Extensions.Configuration.Binder 9.0.1 | Inclui net8.0 e outros targets |
| Microsoft.AspNetCore.Authentication.JwtBearer 9.0.1 | net9.0 |
| Microsoft.NET.Test.Sdk | Tem versionamento próprio; verificar a versão escolhida |

### Correção importante: EF Core 9

A documentação oficial informa expressamente que **EF Core 9 tem como target o .NET 8** e pode ser utilizado com .NET 8 ou .NET 9.

Portanto, não é correto afirmar que instalar EF Core 9 necessariamente exige migrar o projeto para .NET 9.

O Configuration Binder 9.0.1 também possui suporte ao net8.0.

Por outro lado, Microsoft.AspNetCore.Authentication.JwtBearer 9.0.1 possui target net9.0 e não é adequado a um projeto direcionado somente a net8.0.

### Como verificar de forma confiável

Na página da versão específica do pacote no NuGet.org, conferir:

1. **Frameworks:** os TFMs efetivamente incluídos e a compatibilidade indicada.
2. **Dependencies:** as bibliotecas exigidas e seus intervalos de versões.
3. **Release notes:** alterações de API, comportamento e requisitos.
4. A compatibilidade com o restante da aplicação e os resultados dos testes.

Uma versão compatível com o TFM pode ainda ser incompatível com outra biblioteca utilizada pelo projeto.

```text
Versão do pacote
      |
      v
É compatível com o TargetFramework?
      |
      v
É compatível com as demais dependências?
      |
      v
A aplicação compila e os testes passam?
```

**Conclusão técnica:** continuar com as dependências 8.x é uma estratégia válida para reproduzir o curso. A razão correta é manter um conjunto compatível e estável, não a suposta obrigatoriedade de todos os números major coincidirem com o número do .NET.

## 6. EF Core e Pomelo: compatibilidade entre dependências

Na aula 88, instalamos na Infrastructure:

- Microsoft.EntityFrameworkCore;
- Pomelo.EntityFrameworkCore.MySql.

O EF Core oferece o contexto e o mapeamento; o Pomelo é o provider utilizado para conectar ao MySQL.

### Por que isso importa em uma atualização?

Uma versão do Pomelo preparada para EF Core 8 geralmente não é compatível com EF Core 9.

A documentação oficial do EF Core informa que providers normalmente não funcionam através de versões principais diferentes.

```text
Projeto net8.0
      |
      +-- EF Core 8.x
      |
      +-- Pomelo compatível com EF Core 8
```

Se desejarmos atualizar apenas o ORM:

```text
Projeto net8.0
      |
      +-- EF Core 9.x (suporta net8.0)
      |
      +-- Pomelo compatível com EF Core 9
```

A segunda combinação depende da disponibilidade e dos requisitos de uma versão apropriada do provider.

É necessário verificar a documentação e as dependências da versão específica do Pomelo, e não apenas instalar a versão mais recente de cada pacote.

### Pacotes da mesma família

Ao utilizar EF Core, também é importante manter compatíveis os componentes associados, como Microsoft.EntityFrameworkCore.Design, quando utilizados.

Essa observação complementa o vídeo e conecta o assunto diretamente às dependências introduzidas na aula anterior.

## 7. Atualização no nível da solution

O professor mostra que é possível abrir **Manage NuGet Packages for Solution** para visualizar dependências de vários projetos.

Isso oferece uma visão mais ampla, mas exige atenção.

### Bibliotecas citadas

O vídeo menciona Bogus, Coverlet.Collector, FluentValidation, Microsoft.AspNetCore.Authentication.JwtBearer, Entity Framework Core Design, Configuration Binder e Microsoft.NET.Test.Sdk.

A grafia técnica de alguns identificadores é normalizada neste resumo. O pacote de coleta de cobertura é normalmente chamado `coverlet.collector`; o de testes da Microsoft é `Microsoft.NET.Test.Sdk`.

### Recomendação prática

- Verificar a versão de destino de cada projeto.
- Analisar a atualização de cada biblioteca.
- Não selecionar indiscriminadamente todas as atualizações.
- Conferir dependências compartilhadas e transitivas.
- Compilar e executar os testes depois de atualizar.

O professor menciona que algumas bibliotecas podem ser atualizadas sem o mesmo vínculo numérico com a versão do .NET. Isso não dispensa verificar suas próprias regras de compatibilidade.

## 8. LTS, STS e atualizações de manutenção

O instrutor explica a importância do suporte de longo prazo.

**LTS** significa *Long Term Support* — suporte de longo prazo.

A plataforma .NET possui versões com diferentes ciclos de suporte.

No contexto histórico do vídeo:

- .NET 8 pertence a uma geração LTS.
- .NET 9 foi lançado como uma geração STS (*Standard Term Support*).
- Versões de manutenção continuam sendo publicadas ao longo do ciclo de suporte.

O professor usa a possibilidade de uma futura versão 8.0.13 para mostrar que uma linha 8.x continua recebendo correções mesmo após a publicação de uma nova geração.

### Nuance técnica

LTS é uma classificação da linha de produto e de sua política de suporte, não uma propriedade deduzida dos três números de uma versão isolada.

A versão 8.0.12 mencionada do EF Core faz parte da linha EF Core 8. O suporte de um pacote específico deve ser conferido na política aplicável ao produto.

A existência de uma versão de manutenção não reinicia automaticamente o ciclo de suporte da geração.

**Contexto temporal:** este material documenta a migração .NET 8 -> .NET 9 apresentada na gravação. Para uma decisão atual de adoção em produção, consultar a política oficial de suporte e as versões vigentes.

## 9. Duas formas de atualizar o .NET

O instrutor apresenta duas alternativas:

| Alternativa | Procedimento |
|---|---|
| Upgrade Assistant | Utilizar a extensão do Visual Studio para auxiliar na atualização |
| Alteração manual | Editar o TargetFramework dos projetos e limpar os artefatos de compilação |

O professor prefere a alteração manual, pois deseja controlar as mudanças e evitar modificações inesperadas no código.

A migração para .NET 9 é demonstrada como uma possibilidade para quem deseja atualizar, não como uma exigência para continuar acompanhando o curso.

## 10. Método 1: .NET Upgrade Assistant

A primeira alternativa é o .NET Upgrade Assistant, apresentado como uma extensão do Visual Studio.

### Procedimento mostrado

1. Abrir Extensions.
2. Selecionar Manage Extensions.
3. Pesquisar a extensão .NET Upgrade Assistant.
4. Instalar a extensão.
5. Clicar com o botão direito em um projeto.
6. Selecionar a opção Upgrade.
7. Escolher uma das opções de atualização apresentadas.

O professor explica que o procedimento é feito projeto por projeto no exemplo.

### Por que o instrutor não recomenda essa alternativa?

Ele relata ter lido comentários de pessoas que tiveram dificuldades com alterações automáticas no código e menciona que sua experiência em projetos pequenos não apresentou problemas.

A preocupação é que uma ferramenta automatizada possa realizar alterações além das desejadas, especialmente em projetos maiores.

**Importante:** essa é a avaliação e preferência pessoal do instrutor, baseada na experiência e nos relatos mencionados. Não é uma comprovação de que a ferramenta sempre quebra projetos.

O vídeo não executa uma migração completa por meio da extensão e não detalha todas as opções exibidas.

## 11. Método 2: alteração manual do csproj

Esta é a estratégia preferida pelo professor.

### Passo 1 — Instalar o SDK necessário

O instrutor afirma que possui o SDK do .NET 9 instalado.

Antes de mudar o target, é necessário garantir que as ferramentas disponíveis suportem o novo framework.

**Complemento técnico:** é possível conferir os SDKs instalados com:

```bash
dotnet --list-sdks
```

Esse comando não é demonstrado no vídeo.

Também é necessário verificar a compatibilidade da versão do Visual Studio. Se a solution utiliza global.json, o arquivo pode influenciar a seleção do SDK.

### Passo 2 — Alterar TargetFramework

Abrir o arquivo .csproj do projeto e mudar:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

Para:

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
</PropertyGroup>
```

Salvar o arquivo.

### Passo 3 — Repetir nos projetos necessários

O professor recomenda alterar todos os projetos do CashFlow para net9.0, mantendo a solution uniforme.

```text
CashFlow
├── API             -> net9.0
├── Application     -> net9.0
├── Domain          -> net9.0
├── Infrastructure  -> net9.0
├── Communication   -> net9.0
└── Exception       -> net9.0
```

O exemplo representa a organização do projeto discutida na aula, sem esgotar os projetos de teste e outras bibliotecas da solution.

**Nuance:** nem toda solution exige que todos os projetos utilizem exatamente o mesmo TFM. Uma aplicação com target mais recente pode, em determinadas condições, referenciar bibliotecas com targets anteriores compatíveis. Entretanto, bibliotecas com target mais recente não podem ser referenciadas indiscriminadamente por projetos antigos.

Para o procedimento demonstrado, o professor escolhe atualizar todos os projetos.

### Passo 4 — Revisar dependências

A alteração do TFM não atualiza automaticamente os PackageReferences.

É necessário revisar se os pacotes utilizados permanecem compatíveis e, caso haja atualizações, verificar dependências e mudanças relevantes.

### Passo 5 — Limpar artefatos e recompilar

O vídeo orienta excluir bin e obj antes de reabrir a solution e recompilar.

## 12. Limpeza de bin e obj

O professor enfatiza essa etapa no procedimento manual.

### O que são essas pastas?

| Pasta | Conteúdo habitual |
|---|---|
| bin | Saídas de compilação, incluindo assemblies e dependências copiadas |
| obj | Arquivos intermediários, metadados gerados e informações de restore/build |

Essas pastas são geradas pelas ferramentas do .NET e normalmente não devem ser versionadas no Git.

### Procedimento da gravação

1. Alterar os TargetFramework dos projetos.
2. Salvar os arquivos.
3. Fechar o Visual Studio.
4. Abrir o Windows Explorer na pasta da solution.
5. Entrar nas pastas de cada projeto.
6. Excluir somente as pastas bin e obj geradas.
7. Reabrir o Visual Studio.
8. Restaurar as dependências e recompilar.

O professor fecha o IDE para evitar que arquivos ainda estejam em uso.

### Por que fazer a limpeza?

Após a troca do framework, artefatos intermediários podem ter sido produzidos para o destino anterior. A limpeza remove esses resultados e permite que as ferramentas gerem novamente os arquivos para a configuração atual.

```text
net8.0 + artefatos anteriores
          |
          v
Alterar TargetFramework para net9.0
          |
          v
Remover bin e obj
          |
          v
Restore e build
          |
          v
Novos artefatos para net9.0
```

### Complemento: comandos úteis

Também podemos utilizar os comandos de desenvolvimento da CLI:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

Esses comandos são complementares ao procedimento da gravação.

`dotnet clean` limpa saídas de compilação conhecidas, mas não equivale necessariamente a apagar completamente bin e obj. A exclusão manual pode ajudar quando há artefatos antigos ou problemas de restore.

**Atenção:** excluir bin e obj não corrige por si só versões NuGet incompatíveis. É necessário resolver a causa do problema de dependências.

## 13. Exemplo de evolução da sintaxe do C#

O professor apresenta um exemplo de switch tradicional e uma forma mais concisa, para ilustrar como a linguagem evolui com novas versões.

A transcrição não contém o código exibido na tela. Os exemplos seguintes são reconstruções didáticas para representar o conceito.

### Switch tradicional

```csharp
string description;

switch (paymentType)
{
    case 0:
        description = "Cash";
        break;
    case 1:
        description = "CreditCard";
        break;
    default:
        description = "Unknown";
        break;
}
```

### Switch expression

```csharp
string description = paymentType switch
{
    0 => "Cash",
    1 => "CreditCard",
    _ => "Unknown"
};
```

Os exemplos expressam um resultado equivalente nesse cenário.

O instrutor utiliza a comparação para explicar sua preferência por atualizar manualmente o framework sem pedir que uma ferramenta modifique a sintaxe do código automaticamente.

### Distinções importantes

- Switch expressions foram introduzidas no C# 8.0.
- C# e .NET possuem ciclos e números de versão relacionados, mas distintos.
- Uma versão mais nova do .NET pode oferecer APIs novas e mudanças de comportamento.
- Atualizar o TFM pode exigir ajustes no código, mesmo quando não utilizamos uma ferramenta de migração automática.

Não é correto garantir que todo código permanecerá compatível entre versões. Alterações de comportamento, APIs removidas ou mudanças nas dependências precisam ser consideradas.

## 14. Checklist de migração e diagnóstico

### Antes de atualizar pacotes

- [ ] Conferir TargetFramework no .csproj.
- [ ] Conferir os frameworks suportados pela versão do pacote.
- [ ] Conferir dependências diretas e transitivas.
- [ ] Verificar a compatibilidade entre EF Core e Pomelo.
- [ ] Consultar notas de versão e mudanças incompatíveis.
- [ ] Manter as versões da linha 8 quando o objetivo for reproduzir o curso.

### Ao migrar o framework

- [ ] Registrar o estado anterior no controle de versão.
- [ ] Instalar um SDK e IDE compatíveis com o novo target.
- [ ] Conferir eventual global.json.
- [ ] Editar os arquivos .csproj necessários.
- [ ] Revisar PackageReferences.
- [ ] Limpar artefatos antigos, conforme necessário.
- [ ] Restaurar dependências.
- [ ] Compilar.
- [ ] Executar os testes.
- [ ] Conferir o funcionamento da API.

### Como interpretar problemas — complemento técnico

| Sintoma | Verificação inicial |
|---|---|
| NuGet indica pacote incompatível | TFM suportado pela versão do pacote |
| Falha por dependências incompatíveis | Intervalos de versões e dependências transitivas |
| Aviso ou erro após atualizar EF Core | Provider, pacotes associados e breaking changes |
| SDK não consegue compilar o target | SDK instalado, seleção por global.json e IDE |
| Arquivos antigos após mudar o target | Limpeza de bin e obj e novo restore/build |
| Projeto compila, mas falha ao executar | Runtime, configuração, mudanças de API e comportamento |

Os sintomas acima são possibilidades de diagnóstico, e não mensagens observadas no vídeo.

## 15. Resumo, referências e perguntas de revisão

### Principais aprendizados

- O CashFlow original utiliza net8.0.
- O TargetFramework pode ser consultado no arquivo .csproj.
- O professor demonstra atualizações NuGet dentro da linha 8.x.
- As versões históricas mostradas incluem EF Core 8.0.5, 8.0.12 e 9.0.1, e Binder 8.0.1 e 8.0.2.
- O número major de um pacote não determina universalmente seu framework mínimo.
- **EF Core 9 suporta .NET 8**, conforme a documentação oficial.
- Configuration Binder 9.0.1 também inclui suporte ao .NET 8.
- JwtBearer 9.0.1 exige net9.0.
- O provider Pomelo deve ser compatível com a versão do EF Core escolhida.
- O Upgrade Assistant é apresentado como uma opção de migração, mas o professor prefere editar os projetos manualmente.
- A alteração de net8.0 para net9.0 exige ferramentas compatíveis.
- bin e obj são artefatos gerados e podem ser recriados.
- Atualizar o framework não garante que todo código ou toda dependência permaneça compatível.
- Não é necessário migrar para .NET 9 apenas para seguir o curso.

### Referências técnicas complementares

- [Microsoft Learn — Novidades do EF Core 9 e target .NET 8](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew)
- [Microsoft Learn — Versões e planejamento do EF Core](https://learn.microsoft.com/en-us/ef/core/what-is-new/index)
- [Microsoft Learn — Providers e compatibilidade entre versões principais](https://learn.microsoft.com/pt-br/ef/core/providers/)
- [NuGet — Microsoft.Extensions.Configuration.Binder 9.0.1](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder/9.0.1)
- [NuGet — Microsoft.AspNetCore.Authentication.JwtBearer 9.0.1](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/9.0.1)
- [Política de suporte do .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)

### Perguntas de revisão

1. Como identificar o framework de destino de um projeto C#?
2. Por que uma versão 9.x de um pacote não implica automaticamente a necessidade de .NET 9?
3. Qual é o framework de destino do EF Core 9?
4. Por que verificar o Pomelo antes de atualizar o EF Core?
5. Qual é a diferença entre SDK instalado e TargetFramework?
6. Quais são as duas formas de migração apresentadas?
7. Por que o professor recomenda limpar bin e obj?
8. Qual é a diferença entre um problema de TFM e um conflito entre dependências?

### Mapa mental

```text
                     AULA 89
                         |
              Compatibilidade NuGet
                         |
               TargetFramework
                   net8.0
                         |
         +---------------+---------------+
         |                               |
   Manter linha 8                 Migrar para net9.0
         |                               |
  Atualizações de                 SDK + .csproj
  manutenção compatíveis                 |
         |                         Limpar bin/obj
         |                               |
         +---------------+---------------+
                         |
          Verificar pacotes e providers
                         |
                 Restore + Build
                         |
                       Test
                         |
                 Projeto consistente
```

**Conclusão:** o objetivo desta aula extra é aprender a verificar o framework de destino, escolher versões compatíveis e compreender como atualizar a solution de maneira controlada. A recomendação de manter a linha 8.x ajuda a acompanhar o curso, mas a compatibilidade real deve ser verificada por pacote e por dependência. A migração para .NET 9 é um exemplo histórico, não uma exigência para o CashFlow.
