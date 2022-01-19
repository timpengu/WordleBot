using System.Collections.Generic;
using System.Linq;

namespace WordleBot.Dictionaries
{
    public static class DictionaryExtensions
    {
        public static IEnumerable<string> Normalise(this IEnumerable<string> words) => words.Select(s => s.ToUpperInvariant());
    }
}
