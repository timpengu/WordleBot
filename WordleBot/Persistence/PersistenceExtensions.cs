using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using WordleBot.Model;

namespace WordleBot.Persistence
{
    public static class PersistenceExtensions
    {
        public static string ApplicationName = Assembly.GetExecutingAssembly().GetName().Name;
    
        public static void Save(this IReadOnlyCollection<Score> scores, TimeSpan computeTime)
        {
            var file = new ScoresFile(scores.Count, computeTime, scores);
            string path = GetScoresFilePath(scores.Select(c => c.Guess).GetStableHashCode());

            // TODO: serialise to file stream, improve file format?
            string json = JsonSerializer.Serialize(file);
            File.WriteAllText(path, json);
        }

        public static bool TryLoad(this IReadOnlyCollection<string> vocabulary, out IReadOnlyCollection<Score> scores)
        {
            string path = GetScoresFilePath(vocabulary.GetStableHashCode());
            if (!File.Exists(path))
            {
                scores = null;
                return false;
            }

            // TODO: deserialise from file stream
            string json = File.ReadAllText(path);
            var file = JsonSerializer.Deserialize<ScoresFile>(json);

            if (!vocabulary.AreEqualTo(file.Scores.Select(c => c.Guess)))
            {
                scores = null;
                return false;
            }

            scores = file.Scores;
            return true;
        }

        private static string GetScoresFilePath(int hash) => GetDataFilePath($"Scores.{hash:X8}.json");
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
