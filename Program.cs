using System;
using System.Collections.Generic;

namespace ScrabblePuzzleGenerator;
class Program
{
    static ScrabbleDictionary dictionary = new("blex.txt");
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
        PuzzleGenerator generator = new("blex.txt", "letter_values.txt", 26);
        int[] valuesSequence = new int[plaintext.Length];
        for (int i = 0; i < plaintext.Length; i++)
        {
            valuesSequence[i] = plaintext[i] - 'a';
        }
        var result = generator.GeneratePuzzle(valuesSequence);
        Console.WriteLine(new ResultPrinter(new Options(enableColors)).PrintResult(result));
        return 0;
    }

    static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: ScrabblePuzzleGenerator [-c] <alphanumericplaintext>");
        Console.Error.WriteLine("Flags:");
        Console.Error.WriteLine("  -c: Enable colors in the output.");
    }
}
