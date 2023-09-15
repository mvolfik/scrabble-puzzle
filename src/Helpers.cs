using System.Collections.Generic;

namespace ScrabblePuzzleGenerator;

static class Helpers
{
    /// <summary>
    /// Adds given value to the list in the dictionary under given key, creating the list if it doesn't exist yet.
    /// </summary>
    public static void DictListAdd<KeyT, ValueItemT>(ref Dictionary<KeyT, List<ValueItemT>> dict, KeyT key, ValueItemT value)
    where KeyT : notnull
    {
        if (!dict.ContainsKey(key))
            dict[key] = new();
        dict[key].Add(value);
    }
}