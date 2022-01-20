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
            VocabularySize = 5000,
            UseRandomSolution = false,
            GuessCandidatesOnly = false
        };

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.3");

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

            IReadOnlyCollection<string> vocabulary = GetVocabulary(Options.VocabularySize);
            IReadOnlySet<string> solutions = WordleDictionary.Solutions.ToHashSet();

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
                return WordleDictionary.AllWords;
            }

            int maxNonSolutions = vocabularySize - WordleDictionary.Solutions.Count;
            return GoogleBooksDictionary.AllWords // in descending frequency order
                .Intersect(WordleDictionary.NonSolutions)
                .Take(maxNonSolutions) // take N most frequent words
                .Union(WordleDictionary.Solutions) // ensure all valid solutions are included
                .ToList();
        }
    }
}
