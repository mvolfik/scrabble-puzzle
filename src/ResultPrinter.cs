using System.Collections.Generic;
using System.Text;

namespace ScrabblePuzzleGenerator;
class ResultPrinter
{
    readonly Options options;
    StringBuilder builder = new();

    public ResultPrinter(Options options)
    {
        this.options = options;
    }

    public string PrintResult(List<(char, LetterMarker)[]> result)
    {
        builder = new();
        if (options.enableColors)
            builder.Append("\u001b[30m");
        foreach ((char, LetterMarker)[] word in result)
        {
            bool first = true;
            foreach ((char c, LetterMarker m) in word)
            {
                if (first)
                    first = false;
                else
                    builder.Append(' ');

                PrintLetter(c, m);
            }
            builder.Append("\n\n");
        }
        if (options.enableColors)
            builder.Append("\u001b[39m");
        return builder.ToString();
    }

    void PrintLetter(char c, LetterMarker m)
    {
        if (options.enableColors)
        {
            if (m == LetterMarker.StartingSquare || m == LetterMarker.DoubleWord)
                builder.Append("\u001b[105m");
            else if (m == LetterMarker.Reused)
                builder.Append("\u001b[100m");
            else if (m == LetterMarker.TripleWord)
                builder.Append("\u001b[101m");
            else if (m == LetterMarker.DoubleLetter)
                builder.Append("\u001b[106m");
            else if (m == LetterMarker.TripleLetter)
                builder.Append("\u001b[104m");
            else
                builder.Append("\u001b[107m");
        }
        if (m == LetterMarker.StartingSquare)
            builder.Append('>');
        else if (m == LetterMarker.Reused)
            builder.Append('[');
        else
            builder.Append(' ');

        builder.Append(c.ToString().ToUpper());

        if (m == LetterMarker.StartingSquare)
            builder.Append('<');
        else if (m == LetterMarker.Reused)
            builder.Append(']');
        else
            builder.Append(' ');

        if (options.enableColors)
            builder.Append("\u001b[49m");
    }
}

public readonly struct Options
{
    public Options(bool enableColors)
    {
        this.enableColors = enableColors;
    }
    public readonly bool enableColors;
}
