using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ScrabblePuzzleGenerator;

/// <summary>
/// Marker of a grid square on the game board or letter in a placed work, specifying bonuses and rendering.
/// </summary>
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

    readonly ScoredWordsDatabase wordsDb;

    public PuzzleGenerator(ScoredWordsDatabase wordsDb)
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

    /// <summary>
    /// Get all words that have given value when placed at given position
    /// </summary>
    IEnumerable<string> GetWordsForPosition(WordPositionDefinition pos, ushort value, byte specifiedLetterIndex, char specifiedLetter)
    {
        ushort wordMultiplier = 1;
        var doubledIndices = new List<byte>();
        var tripledIndices = new List<byte>();

        // for each letter, check the LetterMarker and modify the variables above if needed
        for (byte i = 0; i < pos.length; i++)
        {
            if (i == specifiedLetterIndex) // specified letter, i.e. already placed, ignore bonuses here
                continue;
            var (wordBonus, isDoubled, isTripled) = GetBonusFromMarker(pos.direction == Direction.Right
                ? ScrabbleBoard[pos.startX + i, pos.startY]
                : ScrabbleBoard[pos.startX, pos.startY + i]);
            wordMultiplier *= wordBonus;
            if (isDoubled)
                doubledIndices.Add(i);
            if (isTripled)
                tripledIndices.Add(i);
        }
        DatabaseKey key = new(value, pos.length, specifiedLetterIndex, specifiedLetter, doubledIndices.ToArray(), tripledIndices.ToArray());
        return wordsDb.GetWords(key, wordMultiplier);
    }

    /// <summary>
    /// Generates all possible puzzles for given values sequence.
    /// This enumerates all possible starting words (since that is a separate), and then for each calls the recursive
    /// function GeneratePuzzleInner
    /// </summary>
    public IEnumerable<List<(WordPositionDefinition, string)>> GeneratePuzzle(ushort[] valuesSequence)
    {
        var occupiedSquares = new OccupiedSquaresTracker(BoardSize);

        foreach (var (word, startX) in wordsDb.GetStartingWords(valuesSequence[0]))
        {
            var squaresCopy = occupiedSquares.Clone();
            // starting only with right direction, down would be equal just with flipped x/y
            WordPositionDefinition wordPositionDefinition = new(startX, 7, Direction.Right, (byte)word.Length);
            squaresCopy.MarkSquares(wordPositionDefinition, 0);
            var placedWords = new (WordPositionDefinition, string)[valuesSequence.Length];
            placedWords[0] = (wordPositionDefinition, word);
            var results = GeneratePuzzleInner(1, valuesSequence, squaresCopy, placedWords);
            foreach (var result in results)
                yield return result;
        }
    }

    /// <summary>
    /// Recursive function for generating the puzzle. Will try to place a word with given index in the sequence of values.
    /// occupiedSquares and placedWords provide information about already placed words in parent recursion levels.
    /// 
    /// placeWords must have the same length as valuesSequence, and items at i >= index are ignored.
    /// </summary>
    IEnumerable<List<(WordPositionDefinition, string)>> GeneratePuzzleInner(
        byte index,
        ushort[] valuesSequence,
        OccupiedSquaresTracker occupiedSquares,
        (WordPositionDefinition, string)[] placedWords)
    {
        // we have successfully placed all words according to the rules, yield the result and exit the function
        if (index == valuesSequence.Length)
        {
            yield return placedWords.ToList();
            yield break;
        }

        // we need to find all available specifications (ambiguous as in the puzzle assignment that we're creating),
        // and then try placing specific words only into those where exists only 1 valid placement
        var possibleWordSpecs = new Dictionary<(byte specifiedIndex, char specifiedLetter, byte length), List<WordPositionDefinition>>();

        for (byte previousWordI = 0; previousWordI < index; previousWordI++)
        {
            var (previousPos, previousWord) = placedWords[previousWordI];
            for (byte letterIndex = 0; letterIndex < previousWord.Length; letterIndex++)
            {
                // For each letter of the already placed word, we get a fixed wordDirectionCoord, and scanning
                // the potential placement of the next word will change the perpendicular coordinate. Therefore we don't have to check x/y all over the place.
                int wordDirectionCoord = (previousPos.direction == Direction.Right ? previousPos.startX : previousPos.startY) + letterIndex;
                int middlePerpendicularCoord = previousPos.direction == Direction.Right ? previousPos.startY : previousPos.startX;

                // find minimum and maximum free coordinate perpendicular to the previous word
                int perpendicularCoordMin = middlePerpendicularCoord;
                // scan left/up
                while (perpendicularCoordMin > middlePerpendicularCoord - MaxWordLength + 1)
                {
                    perpendicularCoordMin--;
                    if (perpendicularCoordMin < 0 || occupiedSquares.IsOccupiedRelative(previousPos.direction, wordDirectionCoord, perpendicularCoordMin, previousWordI))
                    {
                        // we can't use this one, go a step back
                        perpendicularCoordMin++;
                        break;
                    }
                }
                int perpendicularCoordMax = middlePerpendicularCoord;
                // scan right/down
                while (perpendicularCoordMax < middlePerpendicularCoord + MaxWordLength - 1)
                {
                    perpendicularCoordMax++;
                    if (perpendicularCoordMax >= BoardSize || occupiedSquares.IsOccupiedRelative(previousPos.direction, wordDirectionCoord, perpendicularCoordMax, previousWordI))
                    {
                        perpendicularCoordMax--;
                        break;
                    }
                }

                // iterate all lengths and offsets of words that fit into the available space
                for (int startCoord = perpendicularCoordMin; startCoord <= middlePerpendicularCoord; startCoord++)
                {
                    for (int endCoord = middlePerpendicularCoord; endCoord <= perpendicularCoordMax; endCoord++)
                    {
                        byte length = (byte)(endCoord - startCoord + 1);
                        if (length > MaxWordLength)
                            continue;
                        var nextDir = previousPos.direction == Direction.Right ? Direction.Down : Direction.Right;
                        var nextPos = new WordPositionDefinition(
                            nextDir == Direction.Right ? startCoord : wordDirectionCoord,
                            nextDir == Direction.Right ? wordDirectionCoord : startCoord,
                            nextDir,
                            length);
                        // calculate the nextWord-relative index of the overlapping letter
                        var specifiedIndex = (byte)(middlePerpendicularCoord - startCoord);
                        var key = (specifiedIndex, previousWord[letterIndex], length);
                        Helpers.DictListAdd(ref possibleWordSpecs, key, nextPos);
                    }
                }
            }
        }

        foreach (var (key, positions) in possibleWordSpecs)
        {
            // as explained above, only use specifications which have only 1 valid placement
            if (positions.Count != 1) continue;
            var nextPos = positions[0];

            foreach (var nextWord in GetWordsForPosition(
                nextPos,
                valuesSequence[index],
                key.specifiedIndex,
                key.specifiedLetter))
            {
                var squaresCopy = occupiedSquares.Clone();
                squaresCopy.MarkSquares(nextPos, index);
                placedWords[index] = (nextPos, nextWord);
                foreach (var result in GeneratePuzzleInner((byte)(index + 1), valuesSequence, squaresCopy, placedWords))
                {
                    yield return result;
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