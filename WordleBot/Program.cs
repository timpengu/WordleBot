using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordleBot.Dictionaries;
using WordleBot.Model;
using WordleBot.Solver;
using WordleBot.Wordle;

namespace WordleBot
{
    public static class Program
    {
        // TODO: add args switch for behaviours
        private static readonly Options Options = new()
        {
            VocabularyMode = VocabularyMode.UseAllWords,
            VocabularySize = 5000,
            UseRandomSolution = false,
            GuessCandidatesOnly = false
        };

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.2");

            string solution;
            if (Options.UseRandomSolution)
            {
                solution = WordleDictionary.Solutions.SingleRandom();
                Console.WriteLine($"Seeking random solution: {solution}");
            }
            else
            {
                int index = DateTime.Now.GetWordleSolutionIndex();
                solution = WordleDictionary.Solutions[index];
                Console.WriteLine($"Seeking solution to Wordle #{index}");
            }

            (IReadOnlyList<string> vocabulary, IReadOnlyList<string> candidates) = GetVocabulary(Options.VocabularyMode, Options.VocabularySize);
            Console.WriteLine($"Using vocabulary size {vocabulary.Count}/{candidates.Count}");

            var sw = new Stopwatch();
            sw.Start();

            var solutionsSet = new HashSet<string>(WordleDictionary.Solutions);
            foreach (Move move in vocabulary.Solve(candidates, solution.GetEvaluator(), Options.GuessCandidatesOnly))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (Score score in move.Scores.Take(10))
                {
                    Console.WriteLine($" {(solutionsSet.Contains(score.Guess) ? "*":" ")} {score.Guess} {score.AverageMatches:0.##}");
                }
                Console.WriteLine($"Guess: {move.Guess}? -> {move.Guess.ToFlagString(move.Flags)}");
            }
        }

        private static (IReadOnlyList<string> vocabulary, IReadOnlyList<string> candidates) GetVocabulary(this VocabularyMode mode, int maxVocabularySize = -1)
        {
            var fullVocabulary = new Lazy<IReadOnlyList<string>>(() =>
            {
                var words = maxVocabularySize < 0
                    ? WordleDictionary.AllWords
                    : GoogleBooksDictionary.AllWords // in descending frequency order
                        .Intersect(WordleDictionary.NonSolutions)
                        .Take(maxVocabularySize - WordleDictionary.Solutions.Count) // use N most frequent words only
                        .Union(WordleDictionary.Solutions); // ensure all valid solutions are included

                return words.ToList();
            });

            return mode switch
            {
                VocabularyMode.UseSolutionsOnly => (WordleDictionary.Solutions, WordleDictionary.Solutions),
                VocabularyMode.UseSolutionsToCalculateScores => (fullVocabulary.Value, WordleDictionary.Solutions),
                VocabularyMode.UseAllWords => (fullVocabulary.Value, fullVocabulary.Value),
                _ => throw new NotSupportedException()
            };
        }
    }
}
