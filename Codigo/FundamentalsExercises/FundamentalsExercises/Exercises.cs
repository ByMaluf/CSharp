using System.Globalization;

namespace FundamentalsExercises;

public static class Exercises
{
    public static void WelcomeMessage()
    {
        Console.WriteLine("=== Exercício 1 - Boas-vindas ===");
        Console.Write("Digite seu nome: ");

        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Nome inválido.");
            return;
        }

        Console.WriteLine($"Olá, {name}! Seja muito bem-vindo!");
    }

    public static void FullName()
    {
        Console.WriteLine("=== Exercício 2 - Nome completo ===");

        Console.Write("Digite seu nome: ");
        var firstName = Console.ReadLine()?.Trim();

        Console.Write("Digite seu sobrenome: ");
        var lastName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Nome e sobrenome são obrigatórios.");
            return;
        }

        Console.WriteLine($"Nome completo: {firstName} {lastName}");
    }

    public static void MathOperations()
    {
        Console.WriteLine("=== Exercício 3 - Operações matemáticas ===");

        double firstNumber = 10.5;
        double secondNumber = 2.5;

        Console.WriteLine($"Primeiro número: {firstNumber}");
        Console.WriteLine($"Segundo número: {secondNumber}");
        Console.WriteLine();

        Console.WriteLine($"Soma: {firstNumber + secondNumber}");
        Console.WriteLine($"Subtração: {firstNumber - secondNumber}");
        Console.WriteLine($"Multiplicação: {firstNumber * secondNumber}");

        if (secondNumber == 0)
        {
            Console.WriteLine("Divisão: não é possível dividir por zero.");
        }
        else
        {
            Console.WriteLine($"Divisão: {firstNumber / secondNumber}");
        }

        Console.WriteLine($"Média: {(firstNumber + secondNumber) / 2}");
    }

    public static void CharacterCount()
    {
        Console.WriteLine("=== Exercício 4 - Contagem de caracteres ===");
        Console.Write("Digite uma ou mais palavras: ");

        var text = Console.ReadLine();

        if (string.IsNullOrEmpty(text))
        {
            Console.WriteLine("Quantidade de caracteres: 0");
            return;
        }

        var characterCount = text.Count(character => !char.IsWhiteSpace(character));

        Console.WriteLine($"Quantidade de caracteres sem contar espaços: {characterCount}");
    }

    public static void ValidateLicensePlate()
    {
        Console.WriteLine("=== Exercício 5 - Validação de placa ===");
        Console.Write("Digite uma placa no padrão brasileiro anterior a 2018: ");

        var plate = Console.ReadLine()?.Trim();

        var isValid = !string.IsNullOrWhiteSpace(plate)
            && plate.Length == 7
            && plate[..3].All(char.IsAsciiLetter)
            && plate[3..].All(char.IsDigit);

        Console.WriteLine(isValid ? "Verdadeiro" : "Falso");
    }

    public static void DateFormats()
    {
        Console.WriteLine("=== Exercício 6 - Formatos de data e hora ===");

        var now = DateTime.Now;
        var culture = CultureInfo.GetCultureInfo("pt-BR");

        Console.WriteLine(
            $"Formato completo: {now.ToString("dddd, dd 'de' MMMM 'de' yyyy, HH:mm:ss", culture)}");

        Console.WriteLine(
            $"Apenas a data: {now.ToString("dd/MM/yyyy", culture)}");

        Console.WriteLine(
            $"Apenas a hora: {now.ToString("HH:mm:ss", culture)}");

        Console.WriteLine(
            $"Data com mês por extenso: {now.ToString("dd 'de' MMMM 'de' yyyy", culture)}");
    }
}
