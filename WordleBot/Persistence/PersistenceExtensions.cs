using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WordleBot.Model;

namespace WordleBot.Persistence
{
    public static class PersistenceExtensions
    {
        public static string GetInitialStateFileName(int hash) => $"InitialState.{hash:X8}.json";
    
        public static void SaveInitialState(this IEnumerable<CandidateRank> candidates)
        {
            IList<CandidateRank> candidateList = candidates.ToList();
            int hash = candidateList.Select(c => c.Guess).GetStableHashCode();
            string json = JsonSerializer.Serialize(new InitialState(candidateList));
            File.WriteAllText(GetInitialStateFileName(hash), json);
        }

        public static bool TryLoadInitialState(this IEnumerable<string> candidates, out IList<CandidateRank> rankedCandidates)
        {
            int hash = candidates.GetStableHashCode();
            string fileName = GetInitialStateFileName(hash);
            if (!File.Exists(fileName))
            {
                rankedCandidates = null;
                return false;
            }

            string json = File.ReadAllText(fileName);
            InitialState initialState = JsonSerializer.Deserialize<InitialState>(json);

            if (!candidates.AreEqualTo(initialState.Candidates.Select(c => c.Guess)))
            {
                rankedCandidates = null;
                return false;
            }

            rankedCandidates = initialState.Candidates;
            return true;
        }

        private static bool AreEqualTo(this IEnumerable<string> a, IEnumerable<string> b)
        {
            return Enumerable.SequenceEqual(
                a.OrderBy(x => x),
                b.OrderBy(x => x));
        }

        private static int GetStableHashCode(this IEnumerable<string> source)
        {
            unchecked
            {
                return source.OrderBy(x => x).Aggregate(17, (hash, item) => hash * 23 + item.GetStableHashCode());
            }
        }

        // https://stackoverflow.com/a/36845864
        private static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
