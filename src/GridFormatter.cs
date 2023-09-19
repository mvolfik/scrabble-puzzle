using System.Collections.Generic;
using System.Text;

namespace ScrabblePuzzleGenerator;

/// <summary>
/// Utility class to render a list of placed words to the board, represented by a list of letters suitable for copy-paste into spreadsheet software.
/// </summary>
static class GridFormatter
{
    static char?[,] PlaceLettersOnGrid(List<(WordPositionDefinition, string)> words)
    {
        var grid = new char?[PuzzleGenerator.BoardSize, PuzzleGenerator.BoardSize];
        foreach (var (pos, word) in words)
        {
            int x = pos.startX;
            int y = pos.startY;
            for (int i = 0; i < word.Length; i++)
            {
                if (grid[x, y] != null && grid[x, y] != word[i])
                    throw new System.Exception("Internal error (likely bug): Overlapping words");

                grid[x, y] = word[i];
                if (pos.direction == Direction.Right)
                    x++;
                else
                    y++;
            }
        }
        return grid;
    }

    static string PrintGrid(char?[,] grid)
    {
        var builder = new StringBuilder();
        builder.Append("VVV Start copying below VVV\n");
        for (int y = 0; y < PuzzleGenerator.BoardSize; y++)
        {
            for (int x = 0; x < PuzzleGenerator.BoardSize; x++)
            {
                if (grid[x, y] != null)
                    builder.Append(grid[x, y].ToString().ToUpper());
                if (x < PuzzleGenerator.BoardSize - 1) builder.Append('\t');
            }
            builder.Append('\n');
        }
        builder.Append("^^^ Stop copying above ^^^");
        return builder.ToString();
    }

    public static string FormatWordsToGrid(List<(WordPositionDefinition, string)> words)
    {
        return PrintGrid(PlaceLettersOnGrid(words));
    }
}