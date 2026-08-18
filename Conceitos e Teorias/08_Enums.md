# Enums em C#

## 📋 Índice
1. [O que são Enums?](#o-que-são-enums)
2. [Declaração Básica](#declaração-básica)
3. [Valores Subjacentes](#valores-subjacentes)
4. [Usando Enums](#usando-enums)
5. [Conversões](#conversões)
6. [Enums como Flags (Bitmask)](#enums-como-flags-bitmask)
7. [Métodos da Classe Enum](#métodos-da-classe-enum)
8. [Enums em Switch Statements](#enums-em-switch-statements)
9. [Boas Práticas](#boas-práticas)
10. [Exemplos Práticos](#exemplos-práticos)

---

## O que são Enums?

**Enum** (abreviação de "enumeration" - enumeração) é um tipo especial em C# que permite definir um **conjunto de constantes nomeadas**.

Enums são úteis quando você tem um conjunto fixo de valores relacionados que uma variável pode ter.

### Por que usar Enums?

```csharp
// ❌ Sem Enum - código difícil de entender
int status = 1;  // O que significa 1?
if (status == 1)
{
    // ...
}

// ✅ Com Enum - código autoexplicativo
StatusPedido status = StatusPedido.EmProcessamento;
if (status == StatusPedido.EmProcessamento)
{
    // ...
}
```

### Benefícios:

1. **Legibilidade**: Código mais claro e expressivo
2. **Type Safety**: O compilador impede valores inválidos
3. **IntelliSense**: Autocompletar mostra opções disponíveis
4. **Manutenção**: Fácil adicionar ou modificar valores
5. **Documentação implícita**: Os nomes descrevem o propósito

---

## Declaração Básica

### Sintaxe Simples

```csharp
// Enum básico
public enum DiaSemana
{
    Domingo,
    Segunda,
    Terca,
    Quarta,
    Quinta,
    Sexta,
    Sabado
}

// Enum de status
public enum StatusPedido
{
    Pendente,
    EmProcessamento,
    Enviado,
    Entregue,
    Cancelado
}

// Enum de prioridade
public enum Prioridade
{
    Baixa,
    Media,
    Alta,
    Urgente
}
```

### Valores Padrão

Por padrão, os valores começam em **0** e incrementam de 1 em 1:

```csharp
public enum DiaSemana
{
    Domingo,    // 0
    Segunda,    // 1
    Terca,      // 2
    Quarta,     // 3
    Quinta,     // 4
    Sexta,      // 5
    Sabado      // 6
}

DiaSemana dia = DiaSemana.Segunda;
int valor = (int)dia;  // 1
```

### Valores Personalizados

Você pode atribuir valores específicos:

```csharp
public enum HttpStatusCode
{
    OK = 200,
    Created = 201,
    NoContent = 204,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500
}

public enum Mes
{
    Janeiro = 1,
    Fevereiro = 2,
    Marco = 3,
    Abril = 4,
    Maio = 5,
    Junho = 6,
    Julho = 7,
    Agosto = 8,
    Setembro = 9,
    Outubro = 10,
    Novembro = 11,
    Dezembro = 12
}
```

### Valores Parcialmente Atribuídos

Se apenas alguns valores são atribuídos, os outros continuam a sequência:

```csharp
public enum Tamanho
{
    Pequeno = 1,
    Medio,      // 2 (automático)
    Grande,     // 3 (automático)
    ExtraGrande = 10,
    GigaMega    // 11 (automático)
}
```

---

## Valores Subjacentes

Por padrão, enums usam `int` como tipo subjacente, mas você pode especificar outros tipos integrais.

### Tipos Permitidos

- `byte` (0 a 255)
- `sbyte` (-128 a 127)
- `short` (-32,768 a 32,767)
- `ushort` (0 a 65,535)
- `int` (-2,147,483,648 a 2,147,483,647) **← Padrão**
- `uint` (0 a 4,294,967,295)
- `long` (-9,223,372,036,854,775,808 a 9,223,372,036,854,775,807)
- `ulong` (0 a 18,446,744,073,709,551,615)

### Especificando o Tipo

```csharp
// Enum usando byte (economiza memória)
public enum CorRGB : byte
{
    Vermelho = 255,
    Verde = 255,
    Azul = 255,
    Preto = 0,
    Branco = 255
}

// Enum usando long para grandes valores
public enum TamanhoArquivo : long
{
    KB = 1024,
    MB = 1024 * 1024,
    GB = 1024 * 1024 * 1024,
    TB = 1024L * 1024 * 1024 * 1024
}

// Enum usando short
public enum CodigoErro : short
{
    Sucesso = 0,
    ErroGenerico = -1,
    ArquivoNaoEncontrado = -2,
    AcessoNegado = -3
}
```

---

## Usando Enums

### Declaração e Atribuição

```csharp
// Declaração com valor inicial
DiaSemana hoje = DiaSemana.Quinta;

// Atribuição posterior
DiaSemana dia;
dia = DiaSemana.Sexta;

// Usando var
var amanha = DiaSemana.Sabado;
```

### Comparação

```csharp
DiaSemana dia = DiaSemana.Sexta;

// Comparação de igualdade
if (dia == DiaSemana.Sexta)
{
    Console.WriteLine("É sexta-feira! 🎉");
}

if (dia != DiaSemana.Segunda)
{
    Console.WriteLine("Ainda não é segunda!");
}

// Comparação com operadores relacionais
Prioridade prioridade = Prioridade.Alta;
if (prioridade >= Prioridade.Media)
{
    Console.WriteLine("Prioridade elevada!");
}
```

### Como Parâmetro

```csharp
public void ProcessarPedido(StatusPedido status)
{
    switch (status)
    {
        case StatusPedido.Pendente:
            Console.WriteLine("Aguardando processamento");
            break;
        case StatusPedido.EmProcessamento:
            Console.WriteLine("Processando pedido");
            break;
        case StatusPedido.Enviado:
            Console.WriteLine("Pedido enviado");
            break;
    }
}

// Chamada
ProcessarPedido(StatusPedido.EmProcessamento);
```

### Como Propriedade

```csharp
public class Tarefa
{
    public string Titulo { get; set; }
    public Prioridade Prioridade { get; set; }
    public StatusTarefa Status { get; set; }
}

var tarefa = new Tarefa
{
    Titulo = "Revisar código",
    Prioridade = Prioridade.Alta,
    Status = StatusTarefa.EmAndamento
};
```

---

## Conversões

### Enum para Int

```csharp
DiaSemana dia = DiaSemana.Quarta;
int numero = (int)dia;  // 3

StatusPedido status = StatusPedido.Enviado;
int valorStatus = (int)status;  // 2 (assumindo ordem padrão)
```

### Int para Enum

```csharp
int numero = 3;
DiaSemana dia = (DiaSemana)numero;  // DiaSemana.Quarta

// ⚠️ Cuidado: não valida se o valor existe!
DiaSemana diaInvalido = (DiaSemana)999;  // Compila, mas valor inválido
```

### String para Enum (Parsing)

```csharp
// Parse - lança exceção se falhar
string texto = "Quarta";
DiaSemana dia = (DiaSemana)Enum.Parse(typeof(DiaSemana), texto);

// Parse com case-insensitive
dia = (DiaSemana)Enum.Parse(typeof(DiaSemana), "quarta", ignoreCase: true);

// TryParse - mais seguro (não lança exceção)
string entrada = "Sexta";
if (Enum.TryParse<DiaSemana>(entrada, out DiaSemana resultado))
{
    Console.WriteLine($"Convertido: {resultado}");
}
else
{
    Console.WriteLine("Valor inválido!");
}

// TryParse com case-insensitive
if (Enum.TryParse<DiaSemana>(entrada, ignoreCase: true, out resultado))
{
    Console.WriteLine($"Convertido: {resultado}");
}
```

### Enum para String

```csharp
DiaSemana dia = DiaSemana.Quinta;

// ToString()
string nome = dia.ToString();  // "Quinta"

// Interpolação
string mensagem = $"Hoje é {dia}";  // "Hoje é Quinta"

// Format com valor numérico
string comValor = $"{dia} = {(int)dia}";  // "Quinta = 4"
```

---

## Enums como Flags (Bitmask)

O atributo `[Flags]` permite combinar múltiplos valores de enum usando operações bit a bit.

### Declaração de Flags

```csharp
[Flags]
public enum Permissoes
{
    Nenhuma = 0,        // 0000
    Leitura = 1,        // 0001
    Escrita = 2,        // 0010
    Execucao = 4,       // 0100
    Exclusao = 8,       // 1000
    Total = Leitura | Escrita | Execucao | Exclusao  // 1111
}

[Flags]
public enum DiasSemana
{
    Nenhum = 0,         // 0
    Segunda = 1,        // 1
    Terca = 2,          // 2
    Quarta = 4,         // 4
    Quinta = 8,         // 8
    Sexta = 16,         // 16
    Sabado = 32,        // 32
    Domingo = 64,       // 64
    DiasUteis = Segunda | Terca | Quarta | Quinta | Sexta,  // 31
    FimDeSemana = Sabado | Domingo  // 96
}
```

### Usando Flags

```csharp
// Atribuir múltiplos valores
Permissoes permissoes = Permissoes.Leitura | Permissoes.Escrita;

// Adicionar uma permissão
permissoes |= Permissoes.Execucao;

// Verificar se tem uma permissão específica
bool podeLer = (permissoes & Permissoes.Leitura) == Permissoes.Leitura;
bool podeExecutar = permissoes.HasFlag(Permissoes.Execucao);  // Mais legível

// Remover uma permissão
permissoes &= ~Permissoes.Escrita;

// Alternar (toggle) uma permissão
permissoes ^= Permissoes.Exclusao;

// Verificar se tem TODAS as permissões
Permissoes requeridas = Permissoes.Leitura | Permissoes.Escrita;
bool temTodas = (permissoes & requeridas) == requeridas;

// Verificar se tem ALGUMA permissão
bool temAlguma = (permissoes & requeridas) != 0;
```

### Exemplo Completo com Flags

```csharp
[Flags]
public enum OpcoesArquivo
{
    Nenhuma = 0,
    Criar = 1,
    Ler = 2,
    Escrever = 4,
    Deletar = 8,
    LeituraEscrita = Ler | Escrever,
    Total = Criar | Ler | Escrever | Deletar
}

// Uso
OpcoesArquivo opcoes = OpcoesArquivo.Ler | OpcoesArquivo.Escrever;

Console.WriteLine(opcoes);  // "Ler, Escrever" (ToString automático)
Console.WriteLine((int)opcoes);  // 6 (2 + 4)

if (opcoes.HasFlag(OpcoesArquivo.Escrever))
{
    Console.WriteLine("Tem permissão de escrita");
}

// Adicionar permissão de exclusão
opcoes |= OpcoesArquivo.Deletar;
Console.WriteLine(opcoes);  // "Ler, Escrever, Deletar"
```

---

## Métodos da Classe Enum

A classe estática `Enum` fornece métodos úteis para trabalhar com enums.

### GetValues - Obter Todos os Valores

```csharp
// Obter todos os valores
DiaSemana[] dias = (DiaSemana[])Enum.GetValues(typeof(DiaSemana));

// Percorrer todos os valores
foreach (DiaSemana dia in Enum.GetValues(typeof(DiaSemana)))
{
    Console.WriteLine($"{dia} = {(int)dia}");
}

// Com genéricos (C# 7.3+)
foreach (DiaSemana dia in Enum.GetValues<DiaSemana>())
{
    Console.WriteLine(dia);
}
```

### GetNames - Obter Todos os Nomes

```csharp
string[] nomes = Enum.GetNames(typeof(DiaSemana));

foreach (string nome in nomes)
{
    Console.WriteLine(nome);
}

// Com genéricos
foreach (string nome in Enum.GetNames<DiaSemana>())
{
    Console.WriteLine(nome);
}
```

### IsDefined - Verificar se Valor é Válido

```csharp
DiaSemana dia = (DiaSemana)3;
bool ehValido = Enum.IsDefined(typeof(DiaSemana), dia);  // true

DiaSemana diaInvalido = (DiaSemana)999;
bool ehValido2 = Enum.IsDefined(typeof(DiaSemana), diaInvalido);  // false

// Verificar por nome
bool existeSegunda = Enum.IsDefined(typeof(DiaSemana), "Segunda");  // true
```

### GetName - Obter Nome de um Valor

```csharp
DiaSemana dia = DiaSemana.Quarta;
string nome = Enum.GetName(typeof(DiaSemana), dia);  // "Quarta"

// Por valor numérico
string nome2 = Enum.GetName(typeof(DiaSemana), 3);  // "Quarta"
```

### Parse e TryParse

```csharp
// Parse
DiaSemana dia = (DiaSemana)Enum.Parse(typeof(DiaSemana), "Quinta");

// TryParse (recomendado)
if (Enum.TryParse<DiaSemana>("Sexta", out DiaSemana resultado))
{
    Console.WriteLine($"Sucesso: {resultado}");
}
```

---

## Enums em Switch Statements

Enums funcionam muito bem com `switch`:

### Switch Tradicional

```csharp
public void ProcessarStatus(StatusPedido status)
{
    switch (status)
    {
        case StatusPedido.Pendente:
            Console.WriteLine("Pedido aguardando processamento");
            break;
        
        case StatusPedido.EmProcessamento:
            Console.WriteLine("Processando pedido");
            break;
        
        case StatusPedido.Enviado:
            Console.WriteLine("Pedido enviado para entrega");
            break;
        
        case StatusPedido.Entregue:
            Console.WriteLine("Pedido entregue ao cliente");
            break;
        
        case StatusPedido.Cancelado:
            Console.WriteLine("Pedido cancelado");
            break;
        
        default:
            Console.WriteLine("Status desconhecido");
            break;
    }
}
```

### Switch Expression (C# 8+)

```csharp
public string ObterMensagem(StatusPedido status) => status switch
{
    StatusPedido.Pendente => "Aguardando processamento",
    StatusPedido.EmProcessamento => "Processando",
    StatusPedido.Enviado => "Enviado",
    StatusPedido.Entregue => "Entregue",
    StatusPedido.Cancelado => "Cancelado",
    _ => "Status desconhecido"
};

// Uso
string mensagem = ObterMensagem(StatusPedido.Enviado);
Console.WriteLine(mensagem);  // "Enviado"
```

### Switch com Múltiplos Casos

```csharp
public bool EhDiaUtil(DiaSemana dia)
{
    switch (dia)
    {
        case DiaSemana.Segunda:
        case DiaSemana.Terca:
        case DiaSemana.Quarta:
        case DiaSemana.Quinta:
        case DiaSemana.Sexta:
            return true;
        
        case DiaSemana.Sabado:
        case DiaSemana.Domingo:
            return false;
        
        default:
            throw new ArgumentException("Dia inválido");
    }
}

// Com switch expression
public bool EhDiaUtil2(DiaSemana dia) => dia switch
{
    DiaSemana.Segunda or DiaSemana.Terca or DiaSemana.Quarta 
        or DiaSemana.Quinta or DiaSemana.Sexta => true,
    DiaSemana.Sabado or DiaSemana.Domingo => false,
    _ => throw new ArgumentException("Dia inválido")
};
```

---

## Boas Práticas

### 1. Use Nomes Descritivos no Singular

```csharp
// ✅ Bom - singular, descritivo
public enum StatusPedido { Pendente, Aprovado, Cancelado }
public enum TipoUsuario { Admin, Cliente, Moderador }

// ❌ Evite - plural
public enum StatusPedidos { ... }
```

### 2. Use PascalCase para Enum e Valores

```csharp
// ✅ Bom
public enum NivelLog
{
    Debug,
    Informacao,
    Aviso,
    Erro,
    Fatal
}

// ❌ Evite
public enum nivellog
{
    debug,
    informacao
}
```

### 3. Defina Valor Padrão Significativo

```csharp
// ✅ Bom - valor padrão claro
public enum Status
{
    Desconhecido = 0,  // Valor padrão explícito
    Ativo = 1,
    Inativo = 2
}

// ✅ Bom - zero tem significado
public enum Quantidade
{
    Nenhuma = 0,
    Uma = 1,
    Varias = 2
}
```

### 4. Use [Flags] para Combinações

```csharp
// ✅ Bom - permite combinações
[Flags]
public enum Permissoes
{
    Nenhuma = 0,
    Leitura = 1,
    Escrita = 2,
    Execucao = 4,
    LeituraEscrita = Leitura | Escrita
}

// ❌ Evite - sem [Flags] dificulta combinações
public enum Permissoes
{
    Leitura,
    Escrita,
    LeituraEscrita  // Redundante
}
```

### 5. Use Potências de 2 para Flags

```csharp
// ✅ Bom - potências de 2
[Flags]
public enum Opcoes
{
    Nenhuma = 0,
    Opcao1 = 1,      // 2^0
    Opcao2 = 2,      // 2^1
    Opcao3 = 4,      // 2^2
    Opcao4 = 8,      // 2^3
    Opcao5 = 16      // 2^4
}

// ❌ Evite - valores aleatórios
[Flags]
public enum Opcoes
{
    Opcao1 = 1,
    Opcao2 = 3,  // Errado!
    Opcao3 = 5   // Errado!
}
```

### 6. Não Use Enums para Dados que Mudam

```csharp
// ❌ Evite - países podem mudar
public enum Pais
{
    Brasil,
    Argentina,
    Chile
}

// ✅ Bom - use banco de dados ou configuração
public class Pais
{
    public int Id { get; set; }
    public string Nome { get; set; }
}
```

### 7. Valide Valores ao Receber de Fontes Externas

```csharp
// ✅ Bom - valida entrada
public void ProcessarStatus(int statusId)
{
    if (!Enum.IsDefined(typeof(StatusPedido), statusId))
    {
        throw new ArgumentException("Status inválido", nameof(statusId));
    }
    
    var status = (StatusPedido)statusId;
    // Processar...
}
```

### 8. Use TryParse para Conversões de String

```csharp
// ✅ Bom - não lança exceção
if (Enum.TryParse<DiaSemana>(entrada, out var dia))
{
    Console.WriteLine($"Dia válido: {dia}");
}
else
{
    Console.WriteLine("Dia inválido");
}

// ❌ Evite - pode lançar exceção
var dia = (DiaSemana)Enum.Parse(typeof(DiaSemana), entrada);
```

---

## Exemplos Práticos

### 1. Sistema de Logging

```csharp
public enum NivelLog
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}

public class Logger
{
    private NivelLog _nivelMinimo;
    
    public Logger(NivelLog nivelMinimo = NivelLog.Info)
    {
        _nivelMinimo = nivelMinimo;
    }
    
    public void Log(NivelLog nivel, string mensagem)
    {
        if (nivel >= _nivelMinimo)
        {
            string prefixo = nivel switch
            {
                NivelLog.Debug => "🐛 DEBUG",
                NivelLog.Info => "ℹ️ INFO",
                NivelLog.Warning => "⚠️ WARNING",
                NivelLog.Error => "❌ ERROR",
                NivelLog.Fatal => "💀 FATAL",
                _ => "UNKNOWN"
            };
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {prefixo}: {mensagem}");
        }
    }
}

// Uso
var logger = new Logger(NivelLog.Warning);
logger.Log(NivelLog.Debug, "Isso não será exibido");
logger.Log(NivelLog.Error, "Erro detectado!");  // Exibido
```

### 2. E-commerce: Status de Pedido

```csharp
public enum StatusPedido
{
    AguardandoPagamento = 0,
    PagamentoConfirmado = 1,
    EmSeparacao = 2,
    Enviado = 3,
    Entregue = 4,
    Cancelado = 5,
    Devolvido = 6
}

public class Pedido
{
    public int Id { get; set; }
    public StatusPedido Status { get; set; }
    public DateTime DataPedido { get; set; }
    
    public bool PodeSerCancelado()
    {
        return Status == StatusPedido.AguardandoPagamento ||
               Status == StatusPedido.PagamentoConfirmado;
    }
    
    public string ObterMensagemStatus()
    {
        return Status switch
        {
            StatusPedido.AguardandoPagamento => "Aguardando confirmação do pagamento",
            StatusPedido.PagamentoConfirmado => "Pagamento confirmado. Preparando envio",
            StatusPedido.EmSeparacao => "Pedido em separação",
            StatusPedido.Enviado => "Pedido enviado para entrega",
            StatusPedido.Entregue => "Pedido entregue com sucesso",
            StatusPedido.Cancelado => "Pedido cancelado",
            StatusPedido.Devolvido => "Pedido devolvido",
            _ => "Status desconhecido"
        };
    }
    
    public void AvancarStatus()
    {
        if (Status < StatusPedido.Devolvido && Status != StatusPedido.Cancelado)
        {
            Status++;
        }
    }
}
```

### 3. Sistema de Permissões com Flags

```csharp
[Flags]
public enum PermissoesUsuario
{
    Nenhuma = 0,
    VisualizarProdutos = 1,
    CriarProdutos = 2,
    EditarProdutos = 4,
    ExcluirProdutos = 8,
    VisualizarUsuarios = 16,
    GerenciarUsuarios = 32,
    AcessarRelatorios = 64,
    ConfiguracaoSistema = 128,
    
    // Combinações comuns
    UsuarioBasico = VisualizarProdutos,
    Editor = VisualizarProdutos | CriarProdutos | EditarProdutos,
    Administrador = VisualizarProdutos | CriarProdutos | EditarProdutos | 
                    ExcluirProdutos | VisualizarUsuarios | AcessarRelatorios,
    SuperAdmin = ~Nenhuma  // Todas as permissões
}

public class Usuario
{
    public string Nome { get; set; }
    public PermissoesUsuario Permissoes { get; set; }
    
    public bool TemPermissao(PermissoesUsuario permissao)
    {
        return (Permissoes & permissao) == permissao;
    }
    
    public void ConcederPermissao(PermissoesUsuario permissao)
    {
        Permissoes |= permissao;
    }
    
    public void RevogarPermissao(PermissoesUsuario permissao)
    {
        Permissoes &= ~permissao;
    }
}

// Uso
var usuario = new Usuario
{
    Nome = "João",
    Permissoes = PermissoesUsuario.UsuarioBasico
};

// Conceder permissão
usuario.ConcederPermissao(PermissoesUsuario.CriarProdutos);

// Verificar
if (usuario.TemPermissao(PermissoesUsuario.CriarProdutos))
{
    Console.WriteLine("Pode criar produtos");
}

// Exibir todas as permissões
Console.WriteLine($"Permissões: {usuario.Permissoes}");
// Saída: "VisualizarProdutos, CriarProdutos"
```

### 4. Configurações de Aplicação

```csharp
public enum TipoAmbiente
{
    Desenvolvimento,
    Homologacao,
    Producao
}

public enum NivelCache
{
    Nenhum,
    Baixo,
    Medio,
    Alto
}

public class ConfiguracaoApp
{
    public TipoAmbiente Ambiente { get; set; }
    public NivelCache Cache { get; set; }
    
    public string ObterStringConexao()
    {
        return Ambiente switch
        {
            TipoAmbiente.Desenvolvimento => "Server=localhost;Database=dev_db",
            TipoAmbiente.Homologacao => "Server=hml-server;Database=hml_db",
            TipoAmbiente.Producao => "Server=prod-server;Database=prod_db",
            _ => throw new InvalidOperationException("Ambiente não configurado")
        };
    }
    
    public int ObterTempoCache()
    {
        return Cache switch
        {
            NivelCache.Nenhum => 0,
            NivelCache.Baixo => 300,      // 5 minutos
            NivelCache.Medio => 1800,     // 30 minutos
            NivelCache.Alto => 3600,      // 1 hora
            _ => 0
        };
    }
}
```

### 5. Sistema de Votação

```csharp
public enum OpcaoVoto
{
    Contra = -1,
    Abstencao = 0,
    AFavor = 1
}

public class Votacao
{
    private Dictionary<string, OpcaoVoto> _votos = new();
    
    public void Votar(string usuario, OpcaoVoto voto)
    {
        _votos[usuario] = voto;
    }
    
    public (int favor, int contra, int abstencao) ContarVotos()
    {
        int favor = 0, contra = 0, abstencao = 0;
        
        foreach (var voto in _votos.Values)
        {
            switch (voto)
            {
                case OpcaoVoto.AFavor:
                    favor++;
                    break;
                case OpcaoVoto.Contra:
                    contra++;
                    break;
                case OpcaoVoto.Abstencao:
                    abstencao++;
                    break;
            }
        }
        
        return (favor, contra, abstencao);
    }
    
    public string ObterResultado()
    {
        var (favor, contra, abstencao) = ContarVotos();
        
        if (favor > contra)
            return "Proposta aprovada!";
        else if (contra > favor)
            return "Proposta rejeitada!";
        else
            return "Empate!";
    }
}

// Uso
var votacao = new Votacao();
votacao.Votar("Alice", OpcaoVoto.AFavor);
votacao.Votar("Bob", OpcaoVoto.Contra);
votacao.Votar("Carol", OpcaoVoto.AFavor);
votacao.Votar("Dave", OpcaoVoto.Abstencao);

Console.WriteLine(votacao.ObterResultado());  // "Proposta aprovada!"
```

### 6. Menu de Navegação

```csharp
public enum OpcaoMenu
{
    Sair = 0,
    NovoRegistro = 1,
    ListarRegistros = 2,
    BuscarRegistro = 3,
    EditarRegistro = 4,
    ExcluirRegistro = 5
}

public class Menu
{
    public void Exibir()
    {
        Console.WriteLine("==== MENU PRINCIPAL ====");
        
        foreach (OpcaoMenu opcao in Enum.GetValues<OpcaoMenu>())
        {
            Console.WriteLine($"{(int)opcao}. {FormatarOpcao(opcao)}");
        }
    }
    
    private string FormatarOpcao(OpcaoMenu opcao)
    {
        return opcao switch
        {
            OpcaoMenu.Sair => "Sair",
            OpcaoMenu.NovoRegistro => "Novo Registro",
            OpcaoMenu.ListarRegistros => "Listar Registros",
            OpcaoMenu.BuscarRegistro => "Buscar Registro",
            OpcaoMenu.EditarRegistro => "Editar Registro",
            OpcaoMenu.ExcluirRegistro => "Excluir Registro",
            _ => "Opção Desconhecida"
        };
    }
    
    public OpcaoMenu? LerOpcao()
    {
        Console.Write("\nEscolha uma opção: ");
        
        if (int.TryParse(Console.ReadLine(), out int opcao) &&
            Enum.IsDefined(typeof(OpcaoMenu), opcao))
        {
            return (OpcaoMenu)opcao;
        }
        
        return null;
    }
}

// Uso
var menu = new Menu();
while (true)
{
    menu.Exibir();
    var opcao = menu.LerOpcao();
    
    if (opcao == null)
    {
        Console.WriteLine("Opção inválida!");
        continue;
    }
    
    if (opcao == OpcaoMenu.Sair)
        break;
    
    // Processar outras opções...
}
```

---

## 🎓 Resumo

| Conceito | Descrição | Exemplo |
|----------|-----------|---------|
| **Enum Básico** | Conjunto de constantes nomeadas | `enum DiaSemana { Segunda, Terca }` |
| **Valores** | Começam em 0 por padrão | `Segunda = 0, Terca = 1` |
| **Tipo Subjacente** | Padrão é `int` | `enum Tamanho : byte { }` |
| **[Flags]** | Permite combinações de valores | `[Flags] enum Permissoes { }` |
| **Conversão** | Entre enum, int e string | `(int)DiaSemana.Segunda` |
| **Métodos Úteis** | `GetValues`, `GetNames`, `IsDefined`, `TryParse` | |

### Quando Usar Enums?

✅ **Use quando:**
- Tem conjunto fixo de valores relacionados
- Valores não mudam frequentemente
- Precisa de legibilidade no código
- Quer type safety

❌ **Evite quando:**
- Valores vêm de banco de dados
- Conjunto de valores muda frequentemente
- Precisa de dados dinâmicos

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** Todas (Enums básicos), 7.3+ (GetValues genérico), 8+ (Switch expression)
