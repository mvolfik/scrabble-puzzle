using System;
using System.Collections.Generic;

namespace ScrabblePuzzleGenerator;

public enum LetterMarker
{
    None,
    StartingSquare,
    Reused,
    DoubleWord,
    TripleWord,
    DoubleLetter,
    TripleLetter,
}

public enum Direction
{
    Right,
    Down,
}

class PuzzleGenerator
{
    public const int BoardSize = 15;
    public const int MaxWordLength = 5;

    readonly WordsDatabase wordsDb;

    public PuzzleGenerator(WordsDatabase wordsDb)
    {
        this.wordsDb = wordsDb;
    }

    public IEnumerable<List<(char, LetterMarker)[]>> GeneratePuzzle(ushort[] valuesSequence)
    {
        foreach (var (word, startX) in wordsDb.GetStartingWords(valuesSequence[0]))
        {
            var occupiedSquares = new bool[BoardSize, BoardSize];
            for (int x = startX; x < startX + word.Length; x++)
                occupiedSquares[x, 7] = true;
            var results = GeneratePuzzleInner(
                valuesSequence[1..],
                occupiedSquares,
                new[] { (new WordPositionDefinition(startX, 7, Direction.Right), word) });
            foreach (var result in results)
                yield return result;
        }
    }

    IEnumerable<List<(char, LetterMarker)[]>> GeneratePuzzleInner(
        ushort[] valuesSequence,
        bool[,] occupiedSquares,
        (WordPositionDefinition, string)[] placedWords)
    {
        foreach (var (pos, word) in placedWords)
        {
            for (int wordIndex = 0; wordIndex < word.Length; wordIndex++)
            {
                int wordDirectionCoord = (pos.direction == Direction.Right ? pos.startX : pos.startY) + wordIndex;
                int middlePerpendicularCoord = pos.direction == Direction.Right ? pos.startY : pos.startX;
                int perpendicularCoord = middlePerpendicularCoord;
                int downAvailable = 0;
                while (downAvailable < MaxWordLength)
                {
                    perpendicularCoord++;
                    if (perpendicularCoord >= BoardSize)
                        break;
                    if (pos.direction == Direction.Right)
                    {
                        if (occupiedSquares[wordDirectionCoord, perpendicularCoord])
                            break;
                    }
                    else
                    {
                        if (occupiedSquares[perpendicularCoord, wordDirectionCoord])
                            break;
                    }
                    downAvailable++;
                }

                perpendicularCoord = middlePerpendicularCoord;
                int upAvailable = 0;
                while (upAvailable < MaxWordLength)
                {
                    perpendicularCoord--;
                    if (perpendicularCoord < 0)
                        break;
                    if (pos.direction == Direction.Right)
                    {
                        if (occupiedSquares[wordDirectionCoord, perpendicularCoord])
                            break;
                    }
                    else
                    {
                        if (occupiedSquares[perpendicularCoord, wordDirectionCoord])
                            break;
                    }
                    upAvailable++;
                }

                int maxWordLength = downAvailable + upAvailable + 1;
                if (maxWordLength <= 1)
                    continue;

                for (perpendicularCoord = middlePerpendicularCoord - upAvailable; perpendicularCoord <= middlePerpendicularCoord; perpendicularCoord++)
                {
                    yield return new();
                }
            }
        }
    }

    public static int GetWordMultiplier(WordPositionDefinition wordPos, int len)
    {
        int startX = wordPos.startX;
        int y = wordPos.startY;

        if (wordPos.direction == Direction.Down)
            (startX, y) = (y, startX);

        int endX = startX + len - 1;

        // first / last row
        if (y == 0 || y == BoardSize - 1)
            return (startX == 0 || endX == BoardSize - 1 || (startX <= 7 && endX >= 7)) ? 3 : 1;

        // row indices 1-4 and 10-13
        for (int i = 1; i <= 4; i++)
        {
            int firstBonus = i;
            int secondBonus = BoardSize - 1 - i;

            if (y == firstBonus || y == secondBonus)
                return ((startX <= firstBonus && endX >= firstBonus) || (startX <= secondBonus && endX >= secondBonus)) ? 2 : 1;
        }

        // the middle row
        if (y == 7)
            if (startX <= 7 && endX >= 7) return 2;
            else if (startX == 0 || endX == BoardSize - 1) return 3;
        return 1;
    }
}

public readonly struct WordPositionDefinition
{
    public readonly int startX;
    public readonly int startY;
    public readonly Direction direction;

    public WordPositionDefinition(int startX, int startY, Direction direction)
    {
        this.startX = startX;
        this.startY = startY;
        this.direction = direction;
    }
}