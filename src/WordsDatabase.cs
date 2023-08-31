using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScrabblePuzzleGenerator;
class WordsDatabase
{
    public const int MaxWordLength = 5;

    readonly Dictionary<char, ushort> values;
    readonly Dictionary<DatabaseKey, List<string>> words;
    readonly ushort maxValue;
    readonly ushort? modulo;

    public WordsDatabase(string dictionaryFilename, string valuesFilename, ushort? modulo = null)
    {
        values = ReadValues(valuesFilename);
        (words, maxValue) = LoadWords(values, dictionaryFilename);
        this.modulo = modulo;
    }

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
            if (val % wordMultiplier != 0)
                continue;
            ushort realVal = (ushort)(val / wordMultiplier);
            if (realVal > maxValue)
                break;
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
                    DatabaseKey key = new DatabaseKey(value, len, 0, c, doubledIndices, Array.Empty<byte>());
                    foreach (var word in GetWords(key))
                        yield return (word, (byte)startX);
                }
            }
        }
    }

    static Dictionary<char, ushort> ReadValues(string filename)
    {
        Dictionary<char, ushort> values = new();
        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length != 2)
                throw new InvalidDataException("Invalid values file format.");

            if (parts[0].Length != 1)
                throw new InvalidDataException("Invalid values file format.");

            values[parts[0][0]] = ushort.Parse(parts[1]);
        }
        return values;
    }

    static void DictListAdd<KeyT, ValueItemT>(ref Dictionary<KeyT, List<ValueItemT>> dict, KeyT key, ValueItemT value)
    where KeyT : notnull
    {
        if (!dict.ContainsKey(key))
            dict[key] = new();
        dict[key].Add(value);
    }

    static (Dictionary<DatabaseKey, List<string>>, ushort) LoadWords(Dictionary<char, ushort> values, string dictionaryFilename)
    {
        Dictionary<DatabaseKey, List<string>> words = new();
        ushort maxValue = 0;

        foreach (string w in File.ReadAllLines(dictionaryFilename))
        {
            var letterValues = new ushort[w.Length];
            for (byte i = 0; i < w.Length; i++)
                letterValues[i] = values.GetValueOrDefault(w[i], (ushort)0);
            var baseValue = (ushort)letterValues.Sum(k => k);
            if (baseValue > maxValue)
                maxValue = baseValue;

            for (byte specifiedIndex = 0; specifiedIndex < w.Length; specifiedIndex++)
            {
                var baseKey = new DatabaseKey(
                    baseValue,
                    (byte)w.Length,
                    specifiedIndex,
                    w[specifiedIndex],
                    Array.Empty<byte>(),
                    Array.Empty<byte>());

                // no bonuses
                DictListAdd(ref words, baseKey, w);

                // single doubled or tripled index
                for (byte bonusIndex = 0; bonusIndex < w.Length; bonusIndex++)
                {
                    if (bonusIndex == specifiedIndex)
                        continue;
                    var val2 = (ushort)(baseValue + letterValues[bonusIndex]);
                    var val3 = (ushort)(baseValue + letterValues[bonusIndex] * 2);
                    if (val3 > maxValue)
                        maxValue = val3;

                    DictListAdd(
                        ref words,
                        new(baseKey, val2, new[] { bonusIndex }, Array.Empty<byte>()),
                        w);
                    DictListAdd(
                        ref words,
                        new(baseKey, val3, Array.Empty<byte>(), new[] { bonusIndex }),
                        w);
                }

                // two tripled indices 5 squares apart or doubled indices 3 or 5 squares apart
                foreach (byte dist in new[] { 3, 5 })
                {
                    for (byte bonusIndex1 = 0; bonusIndex1 + dist - 1 < w.Length; bonusIndex1++)
                    {
                        byte bonusIndex2 = (byte)(bonusIndex1 + dist - 1);
                        if (bonusIndex1 == specifiedIndex || bonusIndex2 == specifiedIndex)
                            continue;

                        var val2 = (ushort)(baseValue + letterValues[bonusIndex1] + letterValues[bonusIndex2]);
                        if (val2 > maxValue)
                            maxValue = val2;

                        DictListAdd(
                            ref words,
                            new(baseKey, val2, new[] { bonusIndex1, bonusIndex2 }, Array.Empty<byte>()),
                            w);

                        if (dist == 5)
                        {
                            var val3 = (ushort)(baseValue + letterValues[bonusIndex1] * 2 + letterValues[bonusIndex2] * 2);
                            if (val3 > maxValue)
                                maxValue = val3;
                            DictListAdd(
                                ref words,
                                new(baseKey, val3, Array.Empty<byte>(), new[] { bonusIndex1, bonusIndex2 }),
                                w);
                        }
                    }
                }
            }
        }

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