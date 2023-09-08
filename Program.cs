using System;

namespace ScrabblePuzzleGenerator;
class Program
{
    static int Main(string[] args)
    {
        int? exitCode = null;
        string? word = null;
        var options = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i][0] != '-')
            {
                if (word != null)
                {
                    exitCode = 1;
                    goto endOfParse;
                }
                word = args[i];
                continue;
            }

            for (int j = 1; j < args[i].Length; j++)
            {
                switch (args[i][j])
                {
                    case 'c':
                        options.enableColors = true;
                        break;
                    case 'g':
                        options.enableGrid = true;
                        break;
                    case 'h':
                        exitCode = 0;
                        goto endOfParse;
                    default:
                        exitCode = 1;
                        goto endOfParse;
                }
            }
        }
    endOfParse: { }
        if (exitCode != null)
        {
            Console.Error.WriteLine("Usage: ScrabblePuzzleGenerator [-cs] <plaintext>");
            Console.Error.WriteLine("Flags:");
            Console.Error.WriteLine("  -c: Enable colors in the output.");
            Console.Error.WriteLine("  -g: Also print grid of letters formatted for pasting into spreadsheet.");
            Console.Error.WriteLine("  -h: Print this help message.");
            Console.Error.WriteLine("Plaintext (solution of the puzzle) must consist solely of letters a-z.");
            return (int)exitCode;
        }
        string plaintext = word.ToLower();
        for (int i = 0; i < plaintext.Length; i++)
        {
            if (!char.IsAsciiLetterLower(plaintext[i]))
            {
                Console.Error.WriteLine("Error: Plaintext (solution of the puzzle) must be alphanumeric.");
                return 1;
            }
        }

        WordsDatabase wordsDb = new("blex.txt", "letter_values.txt", 26);
        var generator = new PuzzleGenerator(wordsDb);
        var valuesSequence = new ushort[plaintext.Length];
        for (int i = 0; i < plaintext.Length; i++)
        {
            valuesSequence[i] = (ushort)(plaintext[i] - 'a');
        }
        var printer = new ResultPrinter(options);
        foreach (var result in generator.GeneratePuzzle(valuesSequence))
            Console.WriteLine(printer.PrintResult(result));
        return 0;
    }
}
