using System;
using System.Linq;

namespace ScrabblePuzzleGenerator;
class Program
{
    static int Main(string[] args)
    {
        // Parse command line arguments
        int? exitCode = null;
        string? plaintext = null;
        bool numeric = false;
        ushort? modulo = null;
        bool singleSolution = false;

        var printOptions = new PrintOptions();

        string dictionaryFilename = "blex.txt";
        string valuesFilename = "letter_values.txt";
        bool nextWordIsDictionary = false;
        bool nextWordIsValues = false;
        bool nextWordIsMaybeModulo = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (nextWordIsMaybeModulo)
            {
                nextWordIsMaybeModulo = false;
                if (ushort.TryParse(args[i], out ushort parsedModulo))
                {
                    modulo = parsedModulo;
                    continue;
                }
                // else: do nothing, probably wasn't a number, try other things
            }
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
                if (plaintext != null)
                {
                    exitCode = 1;
                    goto endOfParse;
                }
                plaintext = args[i];
                continue;
            }

            for (int j = 1; j < args[i].Length; j++)
            {
                switch (args[i][j])
                {
                    case 'c':
                        printOptions.enableColors = true;
                        break;
                    case 'g':
                        printOptions.enableGrid = true;
                        break;
                    case 's':
                        singleSolution = true;
                        break;
                    case 'n':
                        numeric = true;
                        if (j == args[i].Length - 1)
                        {
                            nextWordIsMaybeModulo = true;
                            goto parseNextArg;
                        }
                        if (ushort.TryParse(args[i].Substring(j + 1), out ushort value))
                        {
                            modulo = value;
                            goto parseNextArg;
                        }
                        exitCode = 1;
                        goto endOfParse;
                    case 'd':
                        if (j != args[i].Length - 1)
                        {
                            Console.Error.WriteLine("Error: -d flag must be followed by a space and then a filename.");
                            return 1;
                        }
                        nextWordIsDictionary = true;
                        goto parseNextArg;
                    case 'v':
                        if (j != args[i].Length - 1)
                        {
                            Console.Error.WriteLine("Error: -v flag must be followed by a space and then a filename.");
                            return 1;
                        }
                        nextWordIsValues = true;
                        goto parseNextArg;
                    case 'h':
                        exitCode = 0;
                        goto endOfParse;
                    default:
                        exitCode = 1;
                        goto endOfParse;
                }
            }
        parseNextArg: { }
        }
    endOfParse: { }
        if (nextWordIsDictionary || nextWordIsValues)
        {
            Console.Error.WriteLine("Error: -d and -v flags must be followed by a space and then a filename.");
            return 1;
        }
        if (plaintext == null) exitCode = 1;
        if (exitCode != null)
        {
            Console.Error.WriteLine("Usage: ScrabblePuzzleGenerator [-cgs] [-n [<modulo>]] [-d <dictionary filename>] [-v <letter values filename>] <plaintext>");
            Console.Error.WriteLine("Flags:");
            Console.Error.WriteLine("  -c: Enable colors in the output.");
            Console.Error.WriteLine("  -g: Also print grid of letters formatted for pasting into spreadsheet.");
            Console.Error.WriteLine("  -s: Print only a single solution.");
            Console.Error.WriteLine("  -n [<modulo>]: Switch to numeric mode: plaintext should be a sequence of numbers separated by commas. If modulo is specified, when solving");
            Console.Error.WriteLine("                 the puzzle it might be necessary to perform the modulo operation to get the original numbers.");
            Console.Error.WriteLine("  -d <filename>: Use given dictionary file. Dictionary should contain one word per line. Default: blex.txt");
            Console.Error.WriteLine("  -v <filename>: Use given letter values file. Each line should have the format `<letter><space><[1-9][0-9]*>`. Default: letter_values.txt");
            Console.Error.WriteLine("  -h: Print this help message.");
            Console.Error.WriteLine("Unless you use -n, plaintext (solution of the puzzle) must consist solely of letters a-z.");
            return (int)exitCode;
        }

        // Parsing was successful, run the program, catch any errors caused by invalid user input.
        try
        {
            ushort[] valuesSequence;
            if (numeric)
            {
                try { valuesSequence = plaintext.Split(',').Select(s => ushort.Parse(s)).ToArray(); }
                catch (FormatException) { throw new UserInputError("Plaintext (solution of the puzzle) must be a sequence of numbers separated by commas."); }
            }
            else
            {
                valuesSequence = new ushort[plaintext.Length];
                for (int i = 0; i < plaintext.Length; i++)
                {
                    var lower = char.ToLower(plaintext[i]);
                    if (lower < 'a' || lower > 'z')
                        throw new UserInputError("Plaintext (solution of the puzzle) must consist solely of letters a-z.");
                    valuesSequence[i] = (ushort)(plaintext[i] - 'a');
                }
            }

            ScoredWordsDatabase wordsDb = new(dictionaryFilename, valuesFilename, numeric ? modulo : 26);
            var generator = new PuzzleGenerator(wordsDb);
            var printer = new ResultPrinter(printOptions);
            foreach (var result in generator.GeneratePuzzle(valuesSequence))
            {
                Console.WriteLine(printer.PrintResult(result));
                if (singleSolution)
                    break;
                Console.WriteLine();
            }
        }
        catch (UserInputError e)
        {
            Console.Error.Write("Error: ");
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
