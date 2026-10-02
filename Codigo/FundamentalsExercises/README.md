# Fundamentals Exercises

Aplicação de console em C# com os exercícios práticos do módulo de fundamentos.

## Exercícios implementados

1. Solicita um nome e exibe uma mensagem personalizada de boas-vindas.
2. Solicita nome e sobrenome e exibe o nome completo.
3. Executa operações com dois valores `double` declarados no código:
   - soma;
   - subtração;
   - multiplicação;
   - divisão com verificação de divisão por zero;
   - média.
4. Conta os caracteres digitados, ignorando espaços em branco.
5. Valida uma placa no padrão brasileiro anterior a 2018:
   - exatamente 7 caracteres;
   - 3 primeiras posições com letras;
   - 4 últimas posições com números.
6. Exibe a data e hora atual em diferentes formatos:
   - formato completo;
   - `dd/MM/yyyy`;
   - hora em formato de 24 horas;
   - data com mês por extenso.

## Estrutura

```text
FundamentalsExercises
├── FundamentalsExercises.slnx
├── README.md
└── FundamentalsExercises
    ├── FundamentalsExercises.csproj
    ├── Program.cs
    └── Exercises.cs
```

## Executando

Na pasta `Codigo/FundamentalsExercises`:

```bash
dotnet run --project FundamentalsExercises/FundamentalsExercises.csproj
```

Ao iniciar, a aplicação apresenta um menu para escolher qual exercício executar.

## Observações

No exercício 4, espaços e outros caracteres de espaço em branco não entram na contagem.

No exercício 5, letras maiúsculas e minúsculas são aceitas e o formato esperado é o padrão antigo `AAA0000`.

No exercício 6, os nomes do dia da semana e do mês são formatados utilizando a cultura `pt-BR`.
