using FundamentalsExercises;

Console.OutputEncoding = System.Text.Encoding.UTF8;

while (true)
{
    Console.Clear();
    Console.WriteLine("=== Exercícios Práticos de C# ===");
    Console.WriteLine();
    Console.WriteLine("1 - Mensagem de boas-vindas");
    Console.WriteLine("2 - Nome completo");
    Console.WriteLine("3 - Operações matemáticas");
    Console.WriteLine("4 - Contagem de caracteres");
    Console.WriteLine("5 - Validação de placa");
    Console.WriteLine("6 - Formatos de data e hora");
    Console.WriteLine("0 - Sair");
    Console.WriteLine();

    Console.Write("Escolha um exercício: ");
    var option = Console.ReadLine();

    Console.Clear();

    switch (option)
    {
        case "1":
            Exercises.WelcomeMessage();
            break;

        case "2":
            Exercises.FullName();
            break;

        case "3":
            Exercises.MathOperations();
            break;

        case "4":
            Exercises.CharacterCount();
            break;

        case "5":
            Exercises.ValidateLicensePlate();
            break;

        case "6":
            Exercises.DateFormats();
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Opção inválida.");
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Pressione ENTER para voltar ao menu...");
    Console.ReadLine();
}
