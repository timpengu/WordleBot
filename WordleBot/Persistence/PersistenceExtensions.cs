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
        public static string ApplicationName = nameof(Wordle);
    
        public static void Save(this IReadOnlyList<Score> scores)
        {
            int hash = scores.Select(c => c.Guess).GetStableHashCode();
            string json = JsonSerializer.Serialize(scores);
            string filePath = GetStateFilePath(hash);
            File.WriteAllText(filePath, json);
        }

        public static bool TryLoad(this IEnumerable<string> words, out IReadOnlyList<Score> scores)
        {
            int hash = words.GetStableHashCode();
            string filePath = GetStateFilePath(hash);
            if (!File.Exists(filePath))
            {
                scores = null;
                return false;
            }

            string json = File.ReadAllText(filePath);
            var loadedScores = JsonSerializer.Deserialize<List<Score>>(json);

            if (!words.AreEqualTo(loadedScores.Select(c => c.Guess)))
            {
                scores = null;
                return false;
            }

            scores = loadedScores;
            return true;
        }

        public static string GetStateFilePath(int hash) => GetDataFilePath($"State.{hash:X8}.json");
        private static string GetDataFilePath(string filename) => Path.Combine(GetDataFolderPath(), filename);
        private static string GetDataFolderPath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName);
            Directory.CreateDirectory(path);
            return path;
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
