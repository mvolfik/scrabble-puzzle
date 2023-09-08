using System;
using System.Collections.Generic;
using System.Linq;

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

    public static readonly LetterMarker[,] ScrabbleBoard = new LetterMarker[BoardSize, BoardSize] {
        { LetterMarker.TripleWord, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.TripleWord },
        { LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None },
        { LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None },
        { LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter },
        { LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None },
        { LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None },
        { LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None },
        { LetterMarker.TripleWord, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.StartingSquare, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.TripleWord },
        { LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None },
        { LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None },
        { LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.None },
        { LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter },
        { LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None },
        { LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleWord, LetterMarker.None },
        { LetterMarker.TripleWord, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.TripleWord, LetterMarker.None, LetterMarker.None, LetterMarker.None, LetterMarker.DoubleLetter, LetterMarker.None, LetterMarker.None, LetterMarker.TripleWord }
    };

    static (ushort wordBonus, bool isDoubled, bool isTripled) GetBonusFromMarker(LetterMarker marker) => marker switch
    {
        LetterMarker.StartingSquare or LetterMarker.DoubleWord => (2, false, false),
        LetterMarker.TripleWord => (3, false, false),
        LetterMarker.DoubleLetter => (1, true, false),
        LetterMarker.TripleLetter => (1, false, true),
        _ => (1, false, false),
    };

    IEnumerable<string> GetWordsForPosition(WordPositionDefinition pos, ushort value, byte specifiedLetterIndex, char specifiedLetter)
    {
        ushort wordMultiplier = 1;
        var doubledIndices = new List<byte>();
        var tripledIndices = new List<byte>();
        for (byte i = 0; i < pos.length; i++)
        {
            if (i == specifiedLetterIndex) // specified letter, i.e. already placed, ignore bonuses here
                continue;
            var (wordBonus, isDoubled, isTripled) = GetBonusFromMarker(pos.direction == Direction.Right
                ? ScrabbleBoard[pos.startX + i, pos.startY]
                : ScrabbleBoard[pos.startX, pos.startY + i]);
            value += wordBonus;
            if (isDoubled)
                doubledIndices.Add(i);
            if (isTripled)
                tripledIndices.Add(i);
        }
        DatabaseKey key = new(value, pos.length, specifiedLetterIndex, specifiedLetter, doubledIndices.ToArray(), tripledIndices.ToArray());
        return wordsDb.GetWords(key, wordMultiplier);
    }

    static void MarkSquare(ref byte[,] grid, int x, int y, int wordIndex)
    {
        if (grid[x, y] == FreeSquare)
            grid[x, y] = (byte)wordIndex;
        else
            grid[x, y] = SquareNeighboringMultipleWords;
    }

    const byte FreeSquare = byte.MaxValue;
    // square unusable under any circumstances
    const byte SquareNeighboringMultipleWords = byte.MaxValue - 1;

    static void MarkSquares(ref byte[,] grid, WordPositionDefinition pos, int wordIndex)
    {
        if (pos.direction == Direction.Right)
        {
            for (int x = pos.startX; x < pos.startX + pos.length; x++)
            {
                MarkSquare(ref grid, x, pos.startY, wordIndex);
                if (pos.startY > 0)
                    MarkSquare(ref grid, x, pos.startY - 1, wordIndex);
                if (pos.startY < BoardSize - 1)
                    MarkSquare(ref grid, x, pos.startY + 1, wordIndex);
            }
            if (pos.startX > 0)
                MarkSquare(ref grid, pos.startX - 1, pos.startY, wordIndex);
            if (pos.startX < BoardSize - 1)
                MarkSquare(ref grid, pos.startX + pos.length, pos.startY, wordIndex);

        }
        else
        {
            for (int y = pos.startY; y < pos.startY + pos.length; y++)
            {
                MarkSquare(ref grid, pos.startX, y, wordIndex);
                if (pos.startX > 0)
                    MarkSquare(ref grid, pos.startX - 1, y, wordIndex);
                if (pos.startX < BoardSize - 1)
                    MarkSquare(ref grid, pos.startX + 1, y, wordIndex);
            }
            if (pos.startY > 0)
                MarkSquare(ref grid, pos.startX, pos.startY - 1, wordIndex);
            if (pos.startY < BoardSize - 1)
                MarkSquare(ref grid, pos.startX, pos.startY + pos.length, wordIndex);
        }
    }

    static void PrintGrid(ref byte[,] grid)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
                Console.Write(grid[x, y] == FreeSquare ? "." : grid[x, y] == SquareNeighboringMultipleWords ? "#" : grid[x, y].ToString());
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.WriteLine();
    }

    public IEnumerable<List<(WordPositionDefinition, string)>> GeneratePuzzle(ushort[] valuesSequence)
    {
        var occupiedSquares = new byte[BoardSize, BoardSize];
        for (int y = 0; y < BoardSize; y++)
            for (int x = 0; x < BoardSize; x++)
                occupiedSquares[x, y] = FreeSquare;

        foreach (var (word, startX) in wordsDb.GetStartingWords(valuesSequence[0]))
        {
            var squaresCopy = (byte[,])occupiedSquares.Clone();
            // starting only with right direction, down would be equal just with flipped x/y
            WordPositionDefinition wordPositionDefinition = new(startX, 7, Direction.Right, (byte)word.Length);
            MarkSquares(ref squaresCopy, wordPositionDefinition, 0);
            var placedWords = new (WordPositionDefinition, string)[valuesSequence.Length];
            placedWords[0] = (wordPositionDefinition, word);
            var results = GeneratePuzzleInner(1, valuesSequence, squaresCopy, placedWords);
            foreach (var result in results)
                yield return result;
        }
    }

    IEnumerable<List<(WordPositionDefinition, string)>> GeneratePuzzleInner(
        int index,
        ushort[] valuesSequence,
        byte[,] occupiedSquares,
        (WordPositionDefinition, string)[] placedWords)
    {
        if (index == valuesSequence.Length)
        {
            yield return placedWords.ToList();
            yield break;
        }

        for (int placedWordI = 0; placedWordI < index; placedWordI++)
        {
            var (pos, word) = placedWords[placedWordI];

            bool isOccupied(int wordDirectionCoord, int perpendicularCoord)
            {
                var value = pos.direction == Direction.Right
                    ? occupiedSquares[wordDirectionCoord, perpendicularCoord]
                    : occupiedSquares[perpendicularCoord, wordDirectionCoord];
                return value != FreeSquare && value != placedWordI;
            }

            for (int letterIndex = 0; letterIndex < word.Length; letterIndex++)
            {
                int wordDirectionCoord = (pos.direction == Direction.Right ? pos.startX : pos.startY) + letterIndex;
                int middlePerpendicularCoord = pos.direction == Direction.Right ? pos.startY : pos.startX;
                int perpendicularCoordMin = middlePerpendicularCoord;
                while (perpendicularCoordMin > middlePerpendicularCoord - MaxWordLength + 1)
                {
                    perpendicularCoordMin--;
                    if (perpendicularCoordMin < 0 || isOccupied(wordDirectionCoord, perpendicularCoordMin))
                    {
                        perpendicularCoordMin++;
                        break;
                    }
                }
                int perpendicularCoordMax = middlePerpendicularCoord;
                while (perpendicularCoordMax < middlePerpendicularCoord + MaxWordLength - 1)
                {
                    perpendicularCoordMax++;
                    if (perpendicularCoordMax >= BoardSize || isOccupied(wordDirectionCoord, perpendicularCoordMax))
                    {
                        perpendicularCoordMax--;
                        break;
                    }
                }

                for (int startCoord = perpendicularCoordMin; startCoord <= middlePerpendicularCoord; startCoord++)
                {
                    for (int endCoord = middlePerpendicularCoord; endCoord <= perpendicularCoordMax; endCoord++)
                    {
                        byte length = (byte)(endCoord - startCoord + 1);
                        if (length > MaxWordLength)
                            continue;
                        var nextDir = pos.direction == Direction.Right ? Direction.Down : Direction.Right;
                        var nextPos = new WordPositionDefinition(
                            nextDir == Direction.Right ? startCoord : wordDirectionCoord,
                            nextDir == Direction.Right ? wordDirectionCoord : startCoord,
                            nextDir,
                            length);
                        foreach (var nextWord in GetWordsForPosition(
                            nextPos,
                            valuesSequence[index],
                            (byte)(middlePerpendicularCoord - startCoord),
                            word[letterIndex]))
                        {
                            var squaresCopy = (byte[,])occupiedSquares.Clone();
                            MarkSquares(ref squaresCopy, nextPos, index);
                            placedWords[index] = (nextPos, nextWord);
                            foreach (var result in GeneratePuzzleInner(index + 1, valuesSequence, squaresCopy, placedWords))
                            {
                                yield return result;
                            }
                        }
                    }
                }
            }
        }
    }
}

public readonly struct WordPositionDefinition
{
    public readonly int startX;
    public readonly int startY;
    public readonly Direction direction;
    public readonly byte length;

    public WordPositionDefinition(int startX, int startY, Direction direction, byte length)
    {
        this.startX = startX;
        this.startY = startY;
        this.direction = direction;
        this.length = length;
    }
}