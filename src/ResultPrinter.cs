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

    public string PrintResult(List<(WordPositionDefinition, string)> result)
    {
        builder = new();
        if (options.enableColors)
            builder.Append("\u001b[30m");
        var markers = GetMarkersForWordSequence(result.ConvertAll(x => x.Item1).ToArray());
        for (int i = 0; i < result.Count; i++)
        {
            var (pos, word) = result[i];
            bool first = true;
            for (int j = 0; j < word.Length; j++)
            {
                if (first)
                    first = false;
                else
                    builder.Append(' ');

                PrintLetter(word[j], markers[i][j]);
            }
            builder.Append("\n\n");
        }
        if (options.enableColors)
            builder.Append("\u001b[39m");

        if (options.enableGrid)
        {
            builder.Append('\n');
            PrintGrid(GenerateGrid(result));
        }
        return builder.ToString();
    }


    /// <summary>
    /// Returns a list of arrays of letter markers, one for each word in the sequence.
    /// </summary>
    /// 
    /// This method has to run on the whole sequence of words at a time, since one of the markers is
    /// "reused", i.e. it depends on the previous words.
    static List<LetterMarker[]> GetMarkersForWordSequence(WordPositionDefinition[] positions)
    {
        var board = (LetterMarker[,])PuzzleGenerator.ScrabbleBoard.Clone();
        var result = new List<LetterMarker[]>(positions.Length);
        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            var markers = new LetterMarker[pos.length];
            int x = pos.startX;
            int y = pos.startY;
            for (int j = 0; j < pos.length; j++)
            {
                markers[j] = board[x, y];
                board[x, y] = LetterMarker.Reused;
                if (pos.direction == Direction.Right)
                    x++;
                else
                    y++;
            }
            result.Add(markers);
        }
        return result;
    }

    static char?[,] GenerateGrid(List<(WordPositionDefinition, string)> words)
    {
        var grid = new char?[PuzzleGenerator.BoardSize, PuzzleGenerator.BoardSize];
        foreach (var (pos, word) in words)
        {
            int x = pos.startX;
            int y = pos.startY;
            for (int i = 0; i < word.Length; i++)
            {
                if (grid[x, y] != null && grid[x, y] != word[i])
                    throw new System.Exception("Overlapping words");

                grid[x, y] = word[i];
                if (pos.direction == Direction.Right)
                    x++;
                else
                    y++;
            }
        }
        return grid;
    }

    void PrintGrid(char?[,] grid)
    {
        builder.Append("VVV Start copying below VVV\n");
        for (int y = 0; y < PuzzleGenerator.BoardSize; y++)
        {
            for (int x = 0; x < PuzzleGenerator.BoardSize; x++)
            {
                if (grid[x, y] != null)
                    builder.Append(grid[x, y].ToString().ToUpper());
                if (y < PuzzleGenerator.BoardSize - 1) builder.Append('\t');
            }
            builder.Append('\n');
        }
        builder.Append("^^^ Stop copying above ^^^");
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

public struct Options
{
    public bool enableColors;
    public bool enableGrid;
}
