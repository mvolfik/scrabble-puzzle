using System;

namespace ScrabblePuzzleGenerator;
class Program
{
    static int Main(string[] args)
    {
        int? exitCode = null;
        string? word = null;
        var options = new Options();
        string dictionaryFilename = "blex.txt";
        string valuesFilename = "letter_values.txt";
        bool nextWordIsDictionary = false;
        bool nextWordIsValues = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (nextWordIsDictionary)
            {
                dictionaryFilename = args[i];
                nextWordIsDictionary = false;
                continue;
            }
            if (nextWordIsValues)
            {
                valuesFilename = args[i];
                nextWordIsValues = false;
                continue;
            }
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
                    case 'd':
                        if (j != args[i].Length - 1)
                        {
                            Console.Error.WriteLine("Error: -d flag must be followed by a space and then a filename.");
                            return 1;
                        }
                        nextWordIsDictionary = true;
                        break;
                    case 'v':
                        if (j != args[i].Length - 1)
                        {
                            Console.Error.WriteLine("Error: -v flag must be followed by a space and then a filename.");
                            return 1;
                        }
                        nextWordIsValues = true;
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
        if (nextWordIsDictionary || nextWordIsValues)
        {
            Console.Error.WriteLine("Error: -d and -v flags must be followed by a space and then a filename.");
            return 1;
        }
        if (exitCode != null)
        {
            Console.Error.WriteLine("Usage: ScrabblePuzzleGenerator [-cs] <plaintext>");
            Console.Error.WriteLine("Flags:");
            Console.Error.WriteLine("  -c: Enable colors in the output.");
            Console.Error.WriteLine("  -g: Also print grid of letters formatted for pasting into spreadsheet.");
            Console.Error.WriteLine("  -d <filename>: Use given dictionary file. Dictionary should contain one word per line. Default: blex.txt");
            Console.Error.WriteLine("  -v <filename>: Use given letter values file. Each line should have the format `<letter><space><[1-9][0-9]*>`. Default: letter_values.txt");
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

        try
        {
            WordsDatabase wordsDb = new(dictionaryFilename, valuesFilename, 26);
            var generator = new PuzzleGenerator(wordsDb);
            var valuesSequence = new ushort[plaintext.Length];
            for (int i = 0; i < plaintext.Length; i++)
            {
                valuesSequence[i] = (ushort)(plaintext[i] - 'a');
            }
            var printer = new ResultPrinter(options);
            foreach (var result in generator.GeneratePuzzle(valuesSequence))
                Console.WriteLine(printer.PrintResult(result));
        }
        catch (UserInputError e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
        return 0;
    }
}

public class UserInputError : Exception
{
    public UserInputError(string message) : base(message) { }
}
