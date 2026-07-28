# Data e Hora em C#

## 📋 Índice
1. [DateTime - Estrutura Principal](#datetime---estrutura-principal)
   - [Criando DateTime](#criando-datetime)
   - [Propriedades](#propriedades)
   - [Obtendo Data e Hora Atual](#obtendo-data-e-hora-atual)
2. [Formatação de Data e Hora](#formatação-de-data-e-hora)
   - [Especificadores Padrão](#especificadores-padrão)
   - [Especificadores Personalizados](#especificadores-personalizados)
3. [Operações com DateTime](#operações-com-datetime)
   - [Adição e Subtração](#adição-e-subtração)
   - [Comparação](#comparação)
4. [TimeSpan - Intervalo de Tempo](#timespan---intervalo-de-tempo)
5. [DateOnly e TimeOnly](#dateonly-e-timeonly)
6. [Fusos Horários (TimeZone)](#fusos-horários-timezone)
7. [Parsing - Conversão de String para DateTime](#parsing---conversão-de-string-para-datetime)
8. [Boas Práticas](#boas-práticas)
9. [Exemplos Práticos](#exemplos-práticos)

---

## DateTime - Estrutura Principal

`DateTime` é a estrutura fundamental para trabalhar com datas e horas em C#. Representa um momento específico no tempo, do ano 0001 até o ano 9999.

### Criando DateTime

```csharp
// Data e hora atual
DateTime agora = DateTime.Now;              // Data e hora local
DateTime agoraUtc = DateTime.UtcNow;        // Data e hora UTC (Tempo Universal Coordenado)
DateTime hoje = DateTime.Today;             // Apenas a data (hora = 00:00:00)

// Construtor com valores específicos
DateTime natal = new DateTime(2024, 12, 25);                    // 25/12/2024 00:00:00
DateTime anoNovo = new DateTime(2025, 1, 1, 0, 0, 0);          // 01/01/2025 00:00:00
DateTime reuniao = new DateTime(2024, 6, 15, 14, 30, 0);       // 15/06/2024 14:30:00

// Com milissegundos
DateTime preciso = new DateTime(2024, 6, 15, 14, 30, 45, 500);  // Inclui 500 milissegundos

// Data mínima e máxima
DateTime minima = DateTime.MinValue;        // 01/01/0001 00:00:00
DateTime maxima = DateTime.MaxValue;        // 31/12/9999 23:59:59
```

### Propriedades

```csharp
DateTime data = new DateTime(2024, 12, 25, 15, 30, 45);

// Componentes individuais
int ano = data.Year;            // 2024
int mes = data.Month;           // 12
int dia = data.Day;             // 25
int hora = data.Hour;           // 15
int minuto = data.Minute;       // 30
int segundo = data.Second;      // 45
int milissegundo = data.Millisecond;  // 0

// Dia da semana
DayOfWeek diaSemana = data.DayOfWeek;       // Wednesday (enum)
string nomeDia = data.ToString("dddd");      // "quarta-feira"

// Dia do ano
int diaDoAno = data.DayOfYear;  // 360 (25 de dezembro é o 360º dia)

// Somente data ou hora
DateTime apenasData = data.Date;            // 25/12/2024 00:00:00
TimeSpan apenasHora = data.TimeOfDay;       // 15:30:45

// Ticks (unidade de tempo interna)
long ticks = data.Ticks;        // Número de intervalos de 100 nanossegundos desde 01/01/0001
```

### Obtendo Data e Hora Atual

```csharp
// Hora local do computador
DateTime agora = DateTime.Now;
Console.WriteLine(agora);  // 15/12/2024 14:30:45

// Hora UTC (Universal Time Coordinated)
DateTime agoraUtc = DateTime.UtcNow;
Console.WriteLine(agoraUtc);  // 15/12/2024 17:30:45 (exemplo com +3h de diferença)

// Apenas a data de hoje
DateTime hoje = DateTime.Today;
Console.WriteLine(hoje);  // 15/12/2024 00:00:00
```

---

## Formatação de Data e Hora

### Especificadores Padrão

```csharp
DateTime data = new DateTime(2024, 12, 25, 15, 30, 45);

// Formato curto de data
string d = data.ToString("d");      // 25/12/2024

// Formato longo de data
string D = data.ToString("D");      // quarta-feira, 25 de dezembro de 2024

// Formato curto de hora
string t = data.ToString("t");      // 15:30

// Formato longo de hora
string T = data.ToString("T");      // 15:30:45

// Data e hora completa (curta)
string g = data.ToString("g");      // 25/12/2024 15:30

// Data e hora completa (longa)
string G = data.ToString("G");      // 25/12/2024 15:30:45

// Data e hora completa (formato completo)
string f = data.ToString("f");      // quarta-feira, 25 de dezembro de 2024 15:30

// Data e hora completa (formato longo completo)
string F = data.ToString("F");      // quarta-feira, 25 de dezembro de 2024 15:30:45

// Padrão ISO 8601 (universal)
string s = data.ToString("s");      // 2024-12-25T15:30:45
string o = data.ToString("o");      // 2024-12-25T15:30:45.0000000

// RFC1123 (usado em HTTP)
string r = data.ToString("r");      // Wed, 25 Dec 2024 15:30:45 GMT

// Mês e ano
string Y = data.ToString("Y");      // dezembro de 2024
```

### Especificadores Personalizados

```csharp
DateTime data = new DateTime(2024, 12, 25, 15, 30, 45);

// Dia
string dd = data.ToString("dd");        // 25
string ddd = data.ToString("ddd");      // qua
string dddd = data.ToString("dddd");    // quarta-feira

// Mês
string MM = data.ToString("MM");        // 12
string MMM = data.ToString("MMM");      // dez
string MMMM = data.ToString("MMMM");    // dezembro

// Ano
string yy = data.ToString("yy");        // 24
string yyyy = data.ToString("yyyy");    // 2024

// Hora
string HH = data.ToString("HH");        // 15 (formato 24h)
string hh = data.ToString("hh");        // 03 (formato 12h)
string h = data.ToString("h");          // 3 (sem zero à esquerda)

// Minuto
string mm = data.ToString("mm");        // 30

// Segundo
string ss = data.ToString("ss");        // 45

// AM/PM
string tt = data.ToString("tt");        // PM

// Formatos personalizados combinados
string formato1 = data.ToString("dd/MM/yyyy");              // 25/12/2024
string formato2 = data.ToString("dd/MM/yyyy HH:mm:ss");     // 25/12/2024 15:30:45
string formato3 = data.ToString("dddd, dd 'de' MMMM 'de' yyyy");  // quarta-feira, 25 de dezembro de 2024
string formato4 = data.ToString("HH:mm:ss");                // 15:30:45
string formato5 = data.ToString("hh:mm tt");                // 03:30 PM
string formato6 = data.ToString("yyyy-MM-dd");              // 2024-12-25 (padrão ISO)
```

### Exemplo de Formatação Completa

```csharp
DateTime agora = DateTime.Now;

Console.WriteLine("Formatos de Data:");
Console.WriteLine($"Curto: {agora:d}");              // 15/12/2024
Console.WriteLine($"Longo: {agora:D}");              // segunda-feira, 15 de dezembro de 2024
Console.WriteLine($"Personalizado: {agora:dd/MM/yyyy}");  // 15/12/2024

Console.WriteLine("\nFormatos de Hora:");
Console.WriteLine($"Curto: {agora:t}");              // 14:30
Console.WriteLine($"Longo: {agora:T}");              // 14:30:45
Console.WriteLine($"12h: {agora:hh:mm tt}");         // 02:30 PM

Console.WriteLine("\nData e Hora:");
Console.WriteLine($"Completo: {agora:G}");           // 15/12/2024 14:30:45
Console.WriteLine($"ISO 8601: {agora:s}");           // 2024-12-15T14:30:45
Console.WriteLine($"RFC 1123: {agora:r}");           // Mon, 15 Dec 2024 14:30:45 GMT
```

---

## Operações com DateTime

### Adição e Subtração

```csharp
DateTime hoje = DateTime.Now;

// Adicionar tempo
DateTime amanha = hoje.AddDays(1);              // Adiciona 1 dia
DateTime proximaSemana = hoje.AddDays(7);       // Adiciona 7 dias
DateTime proximoMes = hoje.AddMonths(1);        // Adiciona 1 mês
DateTime proximoAno = hoje.AddYears(1);         // Adiciona 1 ano
DateTime daquiUmaHora = hoje.AddHours(1);       // Adiciona 1 hora
DateTime daqui30Min = hoje.AddMinutes(30);      // Adiciona 30 minutos
DateTime daqui10Seg = hoje.AddSeconds(10);      // Adiciona 10 segundos

// Subtrair tempo (usando valores negativos)
DateTime ontem = hoje.AddDays(-1);              // Subtrai 1 dia
DateTime mesPassado = hoje.AddMonths(-1);       // Subtrai 1 mês
DateTime anoPassado = hoje.AddYears(-1);        // Subtrai 1 ano

// Adicionar TimeSpan
TimeSpan intervalo = new TimeSpan(2, 30, 0);    // 2 horas e 30 minutos
DateTime futuro = hoje.Add(intervalo);

// Subtrair DateTime (retorna TimeSpan)
DateTime dataInicio = new DateTime(2024, 1, 1);
DateTime dataFim = new DateTime(2024, 12, 31);
TimeSpan diferenca = dataFim - dataInicio;
Console.WriteLine($"Dias: {diferenca.Days}");   // 365 dias
```

### Comparação

```csharp
DateTime data1 = new DateTime(2024, 6, 15);
DateTime data2 = new DateTime(2024, 12, 25);

// Operadores de comparação
bool igual = data1 == data2;            // false
bool diferente = data1 != data2;        // true
bool menor = data1 < data2;             // true
bool maior = data1 > data2;             // false
bool menorIgual = data1 <= data2;       // true
bool maiorIgual = data1 >= data2;       // false

// Método Compare
int resultado = DateTime.Compare(data1, data2);
// resultado < 0: data1 é anterior a data2
// resultado = 0: são iguais
// resultado > 0: data1 é posterior a data2

// Método CompareTo
int resultado2 = data1.CompareTo(data2);  // -1 (data1 < data2)

// Método Equals
bool saoIguais = data1.Equals(data2);     // false

// Verificar se é hoje
DateTime hoje = DateTime.Today;
DateTime verificar = new DateTime(2024, 12, 15);
bool ehHoje = verificar.Date == hoje.Date;
```

---

## TimeSpan - Intervalo de Tempo

`TimeSpan` representa um intervalo de tempo (duração), não um ponto específico no tempo.

```csharp
// Criação
TimeSpan intervalo1 = new TimeSpan(2, 30, 0);           // 2 horas, 30 minutos, 0 segundos
TimeSpan intervalo2 = new TimeSpan(1, 15, 30, 45);      // 1 dia, 15 horas, 30 minutos, 45 segundos
TimeSpan intervalo3 = TimeSpan.FromHours(2.5);          // 2.5 horas
TimeSpan intervalo4 = TimeSpan.FromMinutes(90);         // 90 minutos
TimeSpan intervalo5 = TimeSpan.FromDays(7);             // 7 dias

// Propriedades
TimeSpan tempo = new TimeSpan(1, 15, 30, 45, 500);  // 1 dia, 15h, 30min, 45seg, 500ms

int dias = tempo.Days;                  // 1
int horas = tempo.Hours;                // 15
int minutos = tempo.Minutes;            // 30
int segundos = tempo.Seconds;           // 45
int milissegundos = tempo.Milliseconds; // 500

// Valores totais
double totalDias = tempo.TotalDays;         // 1.6463... dias
double totalHoras = tempo.TotalHours;       // 39.5125 horas
double totalMinutos = tempo.TotalMinutes;   // 2370.75 minutos
double totalSegundos = tempo.TotalSeconds;  // 142245.5 segundos

// Operações com TimeSpan
TimeSpan t1 = TimeSpan.FromHours(2);
TimeSpan t2 = TimeSpan.FromHours(3);

TimeSpan soma = t1 + t2;                // 5 horas
TimeSpan subtracao = t2 - t1;           // 1 hora
TimeSpan multiplicacao = t1 * 2;        // 4 horas
TimeSpan divisao = t2 / 3;              // 1 hora

// Formatação
TimeSpan duracao = new TimeSpan(2, 30, 45);
string formato1 = duracao.ToString();               // 02:30:45
string formato2 = duracao.ToString(@"hh\:mm");      // 02:30
string formato3 = duracao.ToString(@"d\.hh\:mm");   // 0.02:30

// Parsing
TimeSpan parseado = TimeSpan.Parse("02:30:45");
bool sucesso = TimeSpan.TryParse("02:30:45", out TimeSpan resultado);
```

### Exemplo Prático: Calculando Idade

```csharp
DateTime dataNascimento = new DateTime(1990, 5, 15);
DateTime hoje = DateTime.Today;

TimeSpan diferenca = hoje - dataNascimento;
int idade = (int)(diferenca.TotalDays / 365.25);  // 365.25 para considerar anos bissextos

Console.WriteLine($"Idade: {idade} anos");

// Método mais preciso
int idadePrecisa = hoje.Year - dataNascimento.Year;
if (hoje.Month < dataNascimento.Month || 
    (hoje.Month == dataNascimento.Month && hoje.Day < dataNascimento.Day))
{
    idadePrecisa--;  // Ainda não fez aniversário este ano
}
Console.WriteLine($"Idade precisa: {idadePrecisa} anos");
```

---

## DateOnly e TimeOnly

**Introduzidos no C# 10 (.NET 6)** - Tipos mais específicos para trabalhar apenas com data ou hora.

### DateOnly - Apenas Data

```csharp
// Criação
DateOnly hoje = DateOnly.FromDateTime(DateTime.Now);
DateOnly natal = new DateOnly(2024, 12, 25);
DateOnly minima = DateOnly.MinValue;        // 01/01/0001
DateOnly maxima = DateOnly.MaxValue;        // 31/12/9999

// Propriedades
int ano = natal.Year;           // 2024
int mes = natal.Month;          // 12
int dia = natal.Day;            // 25
DayOfWeek diaSemana = natal.DayOfWeek;  // Wednesday
int diaDoAno = natal.DayOfYear; // 360

// Operações
DateOnly amanha = hoje.AddDays(1);
DateOnly proximoMes = hoje.AddMonths(1);
DateOnly proximoAno = hoje.AddYears(1);

// Comparação
bool ehDepois = natal > hoje;
int diasEntre = natal.DayNumber - hoje.DayNumber;

// Formatação
string formatado = natal.ToString("dd/MM/yyyy");    // 25/12/2024
```

### TimeOnly - Apenas Hora

```csharp
// Criação
TimeOnly agora = TimeOnly.FromDateTime(DateTime.Now);
TimeOnly meioDia = new TimeOnly(12, 0, 0);
TimeOnly reuniao = new TimeOnly(14, 30);
TimeOnly minima = TimeOnly.MinValue;        // 00:00:00
TimeOnly maxima = TimeOnly.MaxValue;        // 23:59:59.9999999

// Propriedades
int hora = reuniao.Hour;        // 14
int minuto = reuniao.Minute;    // 30
int segundo = reuniao.Second;   // 0

// Operações
TimeOnly daquiUmaHora = agora.AddHours(1);
TimeOnly daqui30Min = agora.AddMinutes(30);

TimeSpan diferenca = meioDia - agora;

// Comparação
bool ehAntes = agora < meioDia;
bool ehMesmoDia = agora.IsBetween(new TimeOnly(9, 0), new TimeOnly(18, 0));

// Formatação
string formatado = reuniao.ToString("HH:mm");       // 14:30
string formato12h = reuniao.ToString("hh:mm tt");   // 02:30 PM
```

### Quando Usar?

- **`DateTime`**: Quando você precisa de data E hora juntas
- **`DateOnly`**: Quando trabalha apenas com datas (aniversários, feriados, datas de vencimento)
- **`TimeOnly`**: Quando trabalha apenas com horários (horário de funcionamento, alarmes)

```csharp
// Exemplo: Sistema de agendamento
public class Agendamento
{
    public DateOnly Data { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    
    public DateTime DataHoraInicio => Data.ToDateTime(HoraInicio);
}

var agenda = new Agendamento
{
    Data = new DateOnly(2024, 12, 25),
    HoraInicio = new TimeOnly(14, 0),
    HoraFim = new TimeOnly(16, 0)
};
```

---

## Fusos Horários (TimeZone)

### TimeZoneInfo

```csharp
// Obter informações do fuso horário local
TimeZoneInfo fusoLocal = TimeZoneInfo.Local;
Console.WriteLine($"Fuso local: {fusoLocal.DisplayName}");
Console.WriteLine($"ID: {fusoLocal.Id}");
Console.WriteLine($"Diferença UTC: {fusoLocal.BaseUtcOffset}");

// Listar todos os fusos horários disponíveis
foreach (var fuso in TimeZoneInfo.GetSystemTimeZones())
{
    Console.WriteLine($"{fuso.Id}: {fuso.DisplayName}");
}

// Obter fuso horário específico
TimeZoneInfo fusoNY = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
TimeZoneInfo fusoTokio = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
TimeZoneInfo fusoBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

// Converter entre fusos horários
DateTime agoraUtc = DateTime.UtcNow;
DateTime horaNY = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, fusoNY);
DateTime horaTokio = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, fusoTokio);
DateTime horaBrasilia = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, fusoBrasilia);

Console.WriteLine($"UTC: {agoraUtc:G}");
Console.WriteLine($"Nova York: {horaNY:G}");
Console.WriteLine($"Tóquio: {horaTokio:G}");
Console.WriteLine($"Brasília: {horaBrasilia:G}");

// Converter entre dois fusos horários
DateTime horaOrigem = new DateTime(2024, 12, 25, 15, 0, 0);
DateTime horaDestino = TimeZoneInfo.ConvertTime(horaOrigem, fusoBrasilia, fusoTokio);

// Verificar se está em horário de verão
bool ehHorarioVerao = fusoLocal.IsDaylightSavingTime(DateTime.Now);
```

### DateTimeOffset

`DateTimeOffset` armazena data/hora junto com o deslocamento UTC.

```csharp
// Criação
DateTimeOffset agora = DateTimeOffset.Now;          // Hora local com offset
DateTimeOffset agoraUtc = DateTimeOffset.UtcNow;    // Hora UTC

// Com offset específico
TimeSpan offset = new TimeSpan(-3, 0, 0);  // -03:00 (Brasília)
DateTimeOffset customizado = new DateTimeOffset(2024, 12, 25, 15, 0, 0, offset);

// Propriedades
DateTime dataHora = agora.DateTime;         // Parte DateTime
TimeSpan deslocamento = agora.Offset;       // Offset UTC
DateTime utc = agora.UtcDateTime;           // Convertido para UTC

// Conversão para outros fusos
DateTimeOffset convertido = agora.ToOffset(new TimeSpan(-5, 0, 0));  // -05:00

// Formatação (inclui o offset)
string formatado = agora.ToString("o");     // 2024-12-15T14:30:45.1234567-03:00
```

---

## Parsing - Conversão de String para DateTime

### Parse e TryParse

```csharp
// Parse (lança exceção se falhar)
DateTime data1 = DateTime.Parse("25/12/2024");
DateTime data2 = DateTime.Parse("2024-12-25");
DateTime data3 = DateTime.Parse("25/12/2024 15:30:45");

// TryParse (mais seguro, não lança exceção)
string entrada = "25/12/2024";
if (DateTime.TryParse(entrada, out DateTime resultado))
{
    Console.WriteLine($"Data válida: {resultado}");
}
else
{
    Console.WriteLine("Data inválida");
}

// ParseExact - formato específico
string dataTexto = "25-12-2024";
DateTime dataPrecisa = DateTime.ParseExact(
    dataTexto, 
    "dd-MM-yyyy", 
    System.Globalization.CultureInfo.InvariantCulture
);

// TryParseExact
string horaTexto = "15:30:45";
if (DateTime.TryParseExact(
    horaTexto, 
    "HH:mm:ss", 
    System.Globalization.CultureInfo.InvariantCulture,
    System.Globalization.DateTimeStyles.None,
    out DateTime horaResultado))
{
    Console.WriteLine($"Hora válida: {horaResultado}");
}

// Múltiplos formatos aceitos
string[] formatos = { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };
if (DateTime.TryParseExact(
    entrada,
    formatos,
    System.Globalization.CultureInfo.InvariantCulture,
    System.Globalization.DateTimeStyles.None,
    out DateTime dataMultiFormato))
{
    Console.WriteLine($"Data: {dataMultiFormato}");
}
```

---

## Boas Práticas

### 1. Use DateTime.UtcNow para Armazenamento

```csharp
// ❌ Evite armazenar hora local
DateTime dataLocal = DateTime.Now;  // Problemas com fusos horários

// ✅ Prefira UTC para armazenamento
DateTime dataUtc = DateTime.UtcNow;

// Converta para local apenas na exibição
DateTime paraExibir = dataUtc.ToLocalTime();
```

### 2. Use TryParse ao Invés de Parse

```csharp
// ❌ Pode lançar exceção
DateTime data = DateTime.Parse(entradaUsuario);

// ✅ Mais seguro
if (DateTime.TryParse(entradaUsuario, out DateTime data))
{
    // Usar 'data'
}
```

### 3. Use DateOnly/TimeOnly Quando Apropriado (.NET 6+)

```csharp
// ❌ Desnecessariamente complexo
DateTime aniversario = new DateTime(1990, 5, 15, 0, 0, 0);

// ✅ Mais claro e simples
DateOnly aniversario = new DateOnly(1990, 5, 15);
```

### 4. Cuidado com Comparações de Data

```csharp
DateTime hoje = DateTime.Now;
DateTime data = new DateTime(2024, 12, 15, 14, 30, 0);

// ❌ Compara data E hora
if (data == hoje)  // Quase nunca será true

// ✅ Compara apenas a data
if (data.Date == hoje.Date)  // Correto
```

### 5. Use DateTimeOffset para APIs e Persistência

```csharp
// ✅ Melhor para APIs e bancos de dados
public class Evento
{
    public DateTimeOffset DataHora { get; set; }  // Preserva fuso horário
}
```

---

## Exemplos Práticos

### 1. Validar se é Fim de Semana

```csharp
bool EhFimDeSemana(DateTime data)
{
    return data.DayOfWeek == DayOfWeek.Saturday || 
           data.DayOfWeek == DayOfWeek.Sunday;
}

DateTime hoje = DateTime.Today;
if (EhFimDeSemana(hoje))
{
    Console.WriteLine("É fim de semana!");
}
```

### 2. Calcular Dias Úteis Entre Duas Datas

```csharp
int ContarDiasUteis(DateTime inicio, DateTime fim)
{
    int diasUteis = 0;
    DateTime data = inicio;
    
    while (data <= fim)
    {
        if (data.DayOfWeek != DayOfWeek.Saturday && 
            data.DayOfWeek != DayOfWeek.Sunday)
        {
            diasUteis++;
        }
        data = data.AddDays(1);
    }
    
    return diasUteis;
}

DateTime inicio = new DateTime(2024, 12, 1);
DateTime fim = new DateTime(2024, 12, 31);
int diasUteis = ContarDiasUteis(inicio, fim);
Console.WriteLine($"Dias úteis: {diasUteis}");
```

### 3. Obter Primeiro e Último Dia do Mês

```csharp
DateTime PrimeiroDiaDoMes(DateTime data)
{
    return new DateTime(data.Year, data.Month, 1);
}

DateTime UltimoDiaDoMes(DateTime data)
{
    return new DateTime(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month));
}

DateTime hoje = DateTime.Today;
Console.WriteLine($"Primeiro dia: {PrimeiroDiaDoMes(hoje):dd/MM/yyyy}");
Console.WriteLine($"Último dia: {UltimoDiaDoMes(hoje):dd/MM/yyyy}");
```

### 4. Verificar se é Ano Bissexto

```csharp
bool EhAnoBissexto(int ano)
{
    return DateTime.IsLeapYear(ano);
}

Console.WriteLine($"2024 é bissexto? {EhAnoBissexto(2024)}");  // true
Console.WriteLine($"2023 é bissexto? {EhAnoBissexto(2023)}");  // false
```

### 5. Formatar Tempo Relativo (ex: "há 2 horas")

```csharp
string TempoRelativo(DateTime data)
{
    TimeSpan diferenca = DateTime.Now - data;
    
    if (diferenca.TotalSeconds < 60)
        return "agora";
    
    if (diferenca.TotalMinutes < 60)
        return $"há {(int)diferenca.TotalMinutes} minuto(s)";
    
    if (diferenca.TotalHours < 24)
        return $"há {(int)diferenca.TotalHours} hora(s)";
    
    if (diferenca.TotalDays < 30)
        return $"há {(int)diferenca.TotalDays} dia(s)";
    
    if (diferenca.TotalDays < 365)
        return $"há {(int)(diferenca.TotalDays / 30)} mês(es)";
    
    return $"há {(int)(diferenca.TotalDays / 365)} ano(s)";
}

DateTime postar = DateTime.Now.AddHours(-2);
Console.WriteLine(TempoRelativo(postar));  // "há 2 hora(s)"
```

### 6. Cronômetro Simples

```csharp
using System.Diagnostics;

Stopwatch cronometro = new Stopwatch();

Console.WriteLine("Pressione ENTER para iniciar o cronômetro...");
Console.ReadLine();

cronometro.Start();
Console.WriteLine("Cronômetro iniciado! Pressione ENTER para parar.");
Console.ReadLine();

cronometro.Stop();

Console.WriteLine($"Tempo decorrido: {cronometro.Elapsed}");
Console.WriteLine($"Milissegundos: {cronometro.ElapsedMilliseconds}");
Console.WriteLine($"Ticks: {cronometro.ElapsedTicks}");
```

### 7. Temporizador de Contagem Regressiva

```csharp
void ContagemRegressiva(int segundos)
{
    for (int i = segundos; i > 0; i--)
    {
        Console.Write($"\rTempo restante: {i} segundo(s)  ");
        Thread.Sleep(1000);
    }
    Console.WriteLine("\n⏰ Tempo esgotado!");
}

ContagemRegressiva(10);
```

### 8. Sistema de Agendamento

```csharp
public class Compromisso
{
    public string Titulo { get; set; }
    public DateTime DataHora { get; set; }
    public TimeSpan Duracao { get; set; }
    
    public DateTime HoraFim => DataHora.Add(Duracao);
    
    public bool EstaAtivo()
    {
        DateTime agora = DateTime.Now;
        return agora >= DataHora && agora < HoraFim;
    }
    
    public string StatusFormatado()
    {
        if (DateTime.Now < DataHora)
            return $"Começa em {TempoAteInicio()}";
        else if (EstaAtivo())
            return "Em andamento";
        else
            return "Finalizado";
    }
    
    private string TempoAteInicio()
    {
        TimeSpan diferenca = DataHora - DateTime.Now;
        
        if (diferenca.TotalDays >= 1)
            return $"{(int)diferenca.TotalDays} dia(s)";
        if (diferenca.TotalHours >= 1)
            return $"{(int)diferenca.TotalHours} hora(s)";
        if (diferenca.TotalMinutes >= 1)
            return $"{(int)diferenca.TotalMinutes} minuto(s)";
        
        return "menos de 1 minuto";
    }
}

// Uso
var reuniao = new Compromisso
{
    Titulo = "Reunião de Equipe",
    DataHora = DateTime.Now.AddHours(2),
    Duracao = TimeSpan.FromHours(1.5)
};

Console.WriteLine($"{reuniao.Titulo}: {reuniao.StatusFormatado()}");
```

---

## 🎓 Resumo

| Tipo | Propósito | Exemplo |
|------|-----------|---------|
| `DateTime` | Data e hora completas | `DateTime.Now` |
| `DateOnly` | Apenas data (.NET 6+) | `new DateOnly(2024, 12, 25)` |
| `TimeOnly` | Apenas hora (.NET 6+) | `new TimeOnly(14, 30)` |
| `TimeSpan` | Intervalo/duração | `TimeSpan.FromHours(2)` |
| `DateTimeOffset` | Data/hora com fuso | `DateTimeOffset.UtcNow` |
| `TimeZoneInfo` | Informações de fuso horário | `TimeZoneInfo.Local` |

### Principais Métodos

- **Criação**: `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`
- **Formatação**: `.ToString("formato")`
- **Operações**: `.AddDays()`, `.AddHours()`, etc.
- **Comparação**: `==`, `<`, `>`, `.CompareTo()`
- **Parsing**: `.Parse()`, `.TryParse()`, `.ParseExact()`

---

**Autor:** Documentação criada para estudo de C#  
**Data:** 2024  
**Versão C#:** 10+ (DateOnly/TimeOnly), Todas (DateTime)
