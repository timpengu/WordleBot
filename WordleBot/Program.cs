using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordleBot.Dictionaries;
using WordleBot.Model;
using WordleBot.Solver;

namespace WordleBot
{
    public static class Program
    {
        private static readonly IGameDictionary GameDictionary = new WordleDictionary();

        // TODO: add args switch for behaviours
        private static readonly Options Options = new()
        {
            VocabularySize = 5000, // Use 0 for solutions only, int.MaxValue for all words
            UseRandomSolution = false,
            GuessCandidatesOnly = false
        };

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.3");

            string solution;
            if (Options.UseRandomSolution)
            {
                solution = GameDictionary.Solutions.SingleRandom();
                Console.WriteLine($"Seeking random solution: {solution}");
            }
            else
            {
                int index = GameDictionary.GetSolutionIndex(DateTime.Now.Date);
                solution = GameDictionary.Solutions[index];
                Console.WriteLine($"Seeking solution to Wordle #{index}");
            }

            IReadOnlyCollection<string> vocabulary = GetVocabulary(Options.VocabularySize);
            IReadOnlySet<string> solutions = GameDictionary.Solutions.ToHashSet();

            Console.WriteLine($"Using vocabulary size {vocabulary.Count}/{solutions.Count}");

            var sw = Stopwatch.StartNew();

            foreach (Move move in vocabulary.Solve(solution.GetEvaluator(), Options.GuessCandidatesOnly))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (Score score in move.Scores.Take(10))
                {
                    bool isInSolutions = solutions.Contains(score.Guess);
                    Console.WriteLine($"  {(isInSolutions ? "*" : " ")} {score.Guess} {score.AverageMatches:0.##} {(score.IsCandidate ? "C" : "")}");
                }
                Console.WriteLine($"Guess: {move.Guess}? -> {move.Guess.ToFlagString(move.Flags)}");
            }
        }

        private static IReadOnlyList<string> GetVocabulary(int vocabularySize = int.MaxValue)
        {
            if (vocabularySize == int.MaxValue)
            {
                return GameDictionary.AllWords;
            }

            int maxNonSolutions = vocabularySize - GameDictionary.Solutions.Count;
            return GameDictionary.WordsByDescFrequency
                .Intersect(GameDictionary.NonSolutions)
                .Take(maxNonSolutions) // take N most frequent words from non-solutions
                .Union(GameDictionary.Solutions) // ensure all valid solutions are included
                .ToList();
        }
    }
}
