using System;

namespace ScrabblePuzzleGenerator;
class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h"))
        {
            PrintUsage();
            return 0;
        }
        if (!(args.Length == 1 || (args.Length == 2 && args[0] == "-c")))
        {
            PrintUsage();
            return 1;
        }
        bool enableColors = args.Length == 2;
        string plaintext = args[^1].ToLower();
        for (int i = 0; i < plaintext.Length; i++)
        {
            if (!char.IsAsciiLetterLower(plaintext[i]))
            {
                Console.Error.WriteLine("Error: Plaintext (solution of the puzzle) must be alphanumeric.");
                return 1;
            }
        }

        WordsDatabase wordsDb = new WordsDatabase("blex.txt", "letter_values.txt", 26);
        var generator = new PuzzleGenerator(wordsDb);
        var valuesSequence = new ushort[plaintext.Length];
        for (int i = 0; i < plaintext.Length; i++)
        {
            valuesSequence[i] = (ushort)(plaintext[i] - 'a');
        }
        var printer = new ResultPrinter(new Options(enableColors));
        foreach (var result in generator.GeneratePuzzle(valuesSequence))
            Console.WriteLine(printer.PrintResult(result));
        return 0;
    }

    static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: ScrabblePuzzleGenerator [-c] <alphanumericplaintext>");
        Console.Error.WriteLine("Flags:");
        Console.Error.WriteLine("  -c: Enable colors in the output.");
    }
}
