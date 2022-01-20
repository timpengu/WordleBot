using System.Collections.Generic;
using System.Linq;

namespace WordleBot.Dictionaries
{
    public static class DictionaryExtensions
    {
        public static IReadOnlyList<string> GetVocabulary(this IGameDictionary gameDictionary, int vocabularySize = int.MaxValue) =>
            vocabularySize == int.MaxValue
                ? gameDictionary.AllWords
                : gameDictionary.WordsByDescFrequency // not necessarily a superset/subset of AllWords, depending on the source of the frequency dictionary
                    .Intersect(gameDictionary.NonSolutions) // gets a subset of NonSolutions in descending frequency order
                    .Take(vocabularySize - gameDictionary.Solutions.Count) // take N most frequent words
                    .Union(gameDictionary.Solutions) // ensure all valid solutions are included
                    .ToList();

        public static IEnumerable<string> Normalise(this IEnumerable<string> words) =>
            words.Select(s => s.ToUpperInvariant());
    }
}
