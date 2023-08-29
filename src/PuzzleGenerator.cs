using System;
using System.Collections.Generic;
using System.IO;

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

class PuzzleGenerator
{
    readonly ScrabbleDictionary dictionary;
    readonly Dictionary<char, int> values;
    readonly int? modulo;

    public PuzzleGenerator(string dictionaryFilename, string valuesFilename, int? modulo)
    {
        this.dictionary = new(dictionaryFilename);
        this.values = ReadValues(valuesFilename);
        this.modulo = modulo;
    }

    public List<(char, LetterMarker)[]> GeneratePuzzle(int[] valuesSequence)
    {
        // TODO: implement the actual algorithm
        List<(char, LetterMarker)[]> list = new();

        var letters = new (char, LetterMarker)[6];
        string word = "rukola";
        for (int i = 0; i < 6; i++)
        {
            letters[i] = (word[i], LetterMarker.None);
        }
        letters[3].Item2 = LetterMarker.StartingSquare;
        letters[1].Item2 = LetterMarker.DoubleWord;
        letters[5].Item2 = LetterMarker.TripleWord;
        list.Add(letters);
        

        var letters2 = new (char, LetterMarker)[6];
        string word2 = "lavice";
        for (int i = 0; i < 6; i++)
        {
            letters2[i] = (word2[i], LetterMarker.None);
        }
        letters2[0].Item2 = LetterMarker.Reused;
        letters2[4].Item2 = LetterMarker.TripleLetter;
        letters2[5].Item2 = LetterMarker.DoubleLetter;
        list.Add(letters2);

        return list;
    }

    int EvaluateWord(string word, int[] doubledIndices, int[] tripledIndices)
    {
        int[] values = new int[word.Length];
        for (int i = 0; i < word.Length; i++)
        {
            values[i] = this.values[word[i]];
        }
        foreach (int i in doubledIndices)
        {
            values[i] *= 2;
        }
        foreach (int i in tripledIndices)
        {
            values[i] *= 3;
        }

        int sum = 0;
        foreach (int value in values)
        {
            sum += value;
        }
        return sum;
    }

    static Dictionary<char, int> ReadValues(string filename)
    {
        Dictionary<char, int> values = new();
        string[] lines = File.ReadAllLines(filename);
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length != 2)
                throw new InvalidDataException("Invalid values file format.");

            if (parts[0].Length != 1)
                throw new InvalidDataException("Invalid values file format.");

            values[parts[0][0]] = int.Parse(parts[1]);
        }
        return values;
    }
}
