using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScrabblePuzzleGenerator;

/// <summary>
/// Instantiate this class with filenames of your dictionary and letter values mapping. Then you can pass it to PuzzleGenerator.
/// 
/// After instantiation, this class is immutable, meaning you can use it from multiple instances of PuzzleGenerator with different
/// settings, if you want to generate multiple puzzles using the same list of words.
/// </summary>
class ScoredWordsDatabase
{
    /// Official Czech Scrabble Associaction dictionary "blex" only provides this length for download.
    /// This could be increased: theoretical limit is 8: you have 7 letters in your file, and the eight one was on the table.
    /// Apart from changing this value, you would also need to do modifications to generation of the lookup dictionary (some
    /// more bonus field combinations are possible).
    public const int MaxWordLength = 5;

    readonly Dictionary<char, ushort> values;
    readonly Dictionary<DatabaseKey, List<string>> words;

    /// <summary>
    /// Storing maxValue is needed to know when to stop incrementing the searched value by modulo
    /// </summary>
    readonly ushort maxValue;
    readonly ushort? modulo;

    public ScoredWordsDatabase(string dictionaryFilename, string valuesFilename, ushort? modulo = null)
    {
        values = ReadLetterValues(valuesFilename);
        (words, maxValue) = LoadWords(values, dictionaryFilename);
        this.modulo = modulo;
    }

    /// <summary>
    /// Gets all words that will have given value after applying wordMultiplier and then applying modulo.
    /// </summary>
    public IEnumerable<string> GetWords(DatabaseKey key, ushort wordMultiplier = 1)
    {
        if (modulo == null)
        {
            var words = this.words.GetValueOrDefault(key);
            if (words != null)
                foreach (var word in words)
                    yield return word;
            yield break;
        }

        for (ushort val = key.value; ; val += (ushort)modulo)
        {
            ushort realVal = (ushort)(val / wordMultiplier);
            if (realVal > maxValue)
                break;
            if (val % wordMultiplier != 0)
                continue;
            var words = this.words.GetValueOrDefault(new(
                key,
                realVal,
                key.doubledIndices,
                key.tripledIndices));
            if (words == null)
                continue;

            foreach (var word in words)
                yield return word;
        }
    }


    /// <summary>
    /// Returns all possible starting words for given value. There's no overlapping character, so under the hood this function
    /// iterates all possible characters on the first position
    /// </summary>
    public IEnumerable<(string word, byte startX)> GetStartingWords(ushort value)
    {
        for (byte len = 1; len <= MaxWordLength; len++)
        {
            for (int startX = 8 - len; startX <= 7; startX++)
            {
                foreach (char c in values.Keys)
                {
                    var doubledIndices = Array.Empty<byte>();
                    if (startX <= 3)
                        doubledIndices = new[] { (byte)(3 - startX) };
                    else if (startX + len - 1 >= 11)
                        doubledIndices = new[] { (byte)(11 - startX) };
                    var key = new DatabaseKey(value, len, 0, c, doubledIndices, Array.Empty<byte>());
                    foreach (var word in GetWords(key, 2))
                        yield return (word, (byte)startX);
                }
            }
        }
    }

    static Dictionary<char, ushort> ReadLetterValues(string filename)
    {
        Dictionary<char, ushort> values = new();
        string[] lines;

        try { lines = File.ReadAllLines(filename); }
        catch (FileNotFoundException) { throw new UserInputError($"File '{filename}' not found."); }
        catch (IOException) { throw new UserInputError($"Error reading file '{filename}'."); }

        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length != 2 || parts[0].Length != 1 || !char.IsLower(parts[0][0]))
                throw new UserInputError("Invalid letter values file format.");

            if (!ushort.TryParse(parts[1], out ushort value))
                throw new UserInputError("Invalid letter values file format.");
            values[parts[0][0]] = value;
        }
        return values;
    }

    /// <summary>
    /// Loads words from given file and calculates the big lookup table
    /// </summary>
    static (Dictionary<DatabaseKey, List<string>> words, ushort maxValue) LoadWords(Dictionary<char, ushort> values, string dictionaryFilename)
    {
        Dictionary<DatabaseKey, List<string>> words = new();
        ushort maxValue = 0;

        string[] lines;
        try { lines = File.ReadAllLines(dictionaryFilename); }
        catch (FileNotFoundException) { throw new UserInputError($"File '{dictionaryFilename}' not found."); }
        catch (IOException) { throw new UserInputError($"Error reading file '{dictionaryFilename}'."); }
        foreach (string wordUpper in lines)
        {
            string word = wordUpper.ToLower();
            if (word.Length <= 1) continue;
            if (word.Length > MaxWordLength)
            {
                Console.Error.WriteLine($"Warning: Word '{word}' is too long, skipping");
                continue;
            }

            // first calculate the base value of the word, without any modifiers
            var letterValues = new ushort[word.Length];
            for (byte i = 0; i < word.Length; i++)
            {
                if (!values.ContainsKey(word[i]))
                {
                    // these words are technically valid, just that they can only be played with a joker (= 0 points for given letter).
                    // But let's ignore them, they are obscure anyway.
                    Console.Error.WriteLine($"Warning: Unknown value of letter '{word[i]}' in word '{word}', skipping");
                    goto readNextWord;
                }
                letterValues[i] = values[word[i]];
            }
            var baseValue = (ushort)letterValues.Sum(k => k);
            if (baseValue > maxValue)
                maxValue = baseValue;

            // select each possible letter as the overlapping one, and then store all possible bonus layouts into the database
            for (byte specifiedIndex = 0; specifiedIndex < word.Length; specifiedIndex++)
            {
                var baseKey = new DatabaseKey(
                    baseValue,
                    (byte)word.Length,
                    specifiedIndex,
                    word[specifiedIndex],
                    Array.Empty<byte>(),
                    Array.Empty<byte>());

                // no bonuses
                Helpers.DictListAdd(ref words, baseKey, word);

                // single doubled or tripled index, at any position
                // technically, we could skip here the index where it's equal to specifiedIndex, but
                // that would break the currently simple logic of GetStartingWords
                for (byte bonusIndex = 0; bonusIndex < word.Length; bonusIndex++)
                {
                    var val2 = (ushort)(baseValue + letterValues[bonusIndex]);
                    var val3 = (ushort)(baseValue + letterValues[bonusIndex] * 2);
                    if (val3 > maxValue)
                        maxValue = val3;

                    Helpers.DictListAdd(
                        ref words,
                        new(baseKey, val2, new[] { bonusIndex }, Array.Empty<byte>()),
                        word);
                    Helpers.DictListAdd(
                        ref words,
                        new(baseKey, val3, Array.Empty<byte>(), new[] { bonusIndex }),
                        word);
                }

                // two tripled indices 5 squares apart or doubled indices 3 or 5 squares apart
                foreach (var distance in new byte[] { 3, 5 })
                {
                    for (byte bonusIndex1 = 0; bonusIndex1 + distance - 1 < word.Length; bonusIndex1++)
                    {
                        // first word can't have two letter bonuses, so we can skip the cases overlapping with the specified letter here
                        byte bonusIndex2 = (byte)(bonusIndex1 + distance - 1);
                        if (bonusIndex1 == specifiedIndex || bonusIndex2 == specifiedIndex)
                            continue;

                        // add doubled letters with this distance
                        var val2 = (ushort)(baseValue + letterValues[bonusIndex1] + letterValues[bonusIndex2]);
                        if (val2 > maxValue)
                            maxValue = val2;
                        Helpers.DictListAdd(
                            ref words,
                            new(baseKey, val2, new[] { bonusIndex1, bonusIndex2 }, Array.Empty<byte>()),
                            word);

                        // tripled letters are only ever 5 squares apart
                        if (distance == 5)
                        {
                            var val3 = (ushort)(baseValue + letterValues[bonusIndex1] * 2 + letterValues[bonusIndex2] * 2);
                            if (val3 > maxValue)
                                maxValue = val3;
                            Helpers.DictListAdd(
                                ref words,
                                new(baseKey, val3, Array.Empty<byte>(), new[] { bonusIndex1, bonusIndex2 }),
                                word);
                        }
                    }
                }
            }
        readNextWord: { }
        }

        // Report stats
        int keyCount = 0;
        int totalCount = 0;
        foreach (var (key, value) in words)
        {
            keyCount++;
            totalCount += value.Count;
        }
        Console.Error.WriteLine($"Loaded words database: search keys: {keyCount}, total word scores: {totalCount}");

        return (words, maxValue);
    }
}

readonly struct DatabaseKey
{
    public readonly ushort value;
    public readonly byte length;
    public readonly byte specifiedLetterIndex;
    public readonly char specifiedLetter;
    public readonly byte[] doubledIndices;
    public readonly byte[] tripledIndices;

    public DatabaseKey(DatabaseKey key, ushort value, byte[] doubledIndices, byte[] tripledIndices)
    {
        this.value = value;
        length = key.length;
        specifiedLetterIndex = key.specifiedLetterIndex;
        specifiedLetter = key.specifiedLetter;
        this.doubledIndices = doubledIndices;
        this.tripledIndices = tripledIndices;
    }
    public DatabaseKey(ushort value, byte length, byte specifiedLetterIndex, char specifiedLetter, byte[] doubledIndices, byte[] tripledIndices)
    {
        this.value = value;
        this.length = length;
        this.specifiedLetterIndex = specifiedLetterIndex;
        this.specifiedLetter = specifiedLetter;
        this.doubledIndices = doubledIndices;
        this.tripledIndices = tripledIndices;
    }

    public override bool Equals(object? obj)
    {
        return obj is DatabaseKey key
            && value == key.value
            && length == key.length
            && specifiedLetterIndex == key.specifiedLetterIndex
            && specifiedLetter == key.specifiedLetter
            && ((IStructuralEquatable)doubledIndices).Equals(key.doubledIndices, EqualityComparer<byte>.Default)
            && ((IStructuralEquatable)tripledIndices).Equals(key.tripledIndices, EqualityComparer<byte>.Default);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            value,
            length,
            specifiedLetterIndex,
            specifiedLetter,
            ((IStructuralEquatable)doubledIndices).GetHashCode(EqualityComparer<byte>.Default),
            ((IStructuralEquatable)tripledIndices).GetHashCode(EqualityComparer<byte>.Default));
    }
}