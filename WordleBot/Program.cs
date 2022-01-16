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
            UseRandomSolution = false,
            VocabularyMode = VocabularyMode.UseAllWords,
            GuessCandidatesOnly = false
        };

        private static readonly IList<string> Solutions = WordleDictionary.Solutions.Normalise().ToList();

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.1");

            string solution;
            if (Options.UseRandomSolution)
            {
                solution = Solutions.SingleRandom();
                Console.WriteLine($"Seeking random solution: {solution}");
            }
            else
            {
                int index = DateTime.Now.GetWordleSolutionIndex();
                solution = Solutions[index];
                Console.WriteLine($"Seeking solution to Wordle #{index}");
            }

            (IList<string> vocabulary, IList<string> candidates) = GetVocabulary(Options.VocabularyMode, 8000);

            var sw = new Stopwatch();
            sw.Start();

            var solutionsSet = new HashSet<string>(Solutions);
            foreach (var (scores, guess, flags) in vocabulary.Solve(candidates, solution.GetEvaluator(), Options.GuessCandidatesOnly))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (GuessScore score in scores.Take(10))
                {
                    Console.WriteLine($" {(solutionsSet.Contains(score.Guess) ? "*":" ")} {score.Guess} {score.AverageMatches:0.##}");
                }
                Console.WriteLine($"Guess: {guess}? -> {guess.ToFlagString(flags)}");
            }
        }

        private static (IList<string> vocabulary, IList<string> candidates) GetVocabulary(this VocabularyMode mode, int maxNonSolutions)
        {
            var allWords = new Lazy<IList<string>>(() =>
            {
                IEnumerable<string> wordleWords = WordleDictionary.AllWords.Normalise();
                IEnumerable<string> freqWords = GoogleBooksDictionary.AllWords.Normalise().Take(maxNonSolutions);

                 return wordleWords
                     .Intersect(freqWords) // use most frequent words only
                     .Union(Solutions) // ensure all valid solutions are included
                     .ToList();
            });

            return mode switch
            {
                VocabularyMode.UseSolutionsOnly => (Solutions, Solutions),
                VocabularyMode.UseSolutionsToCalculateScores => (allWords.Value, Solutions),
                VocabularyMode.UseAllWords => (allWords.Value, allWords.Value),
                _ => throw new NotSupportedException()
            };
        }

        private static IEnumerable<string> Normalise(this IEnumerable<string> words) => words.Select(s => s.ToUpperInvariant());
    }
}
