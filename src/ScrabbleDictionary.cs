using System.IO;

namespace ScrabblePuzzleGenerator;
class ScrabbleDictionary
{
    string[] words;
    public ScrabbleDictionary(string filename)
    {
        words = File.ReadAllLines(filename);
    }
}
