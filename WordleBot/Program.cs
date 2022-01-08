using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using WordleBot.Model;
using WordleBot.Persistence;
using WordleBot.Solver;
using WordleBot.Wordle;

namespace WordleBot
{
    public static class Program
    {
        // TODO: add args switch for behaviours
        private static bool useRandomSolution = false;
        private static bool useStrictMode = true;
        private static bool useFullDictionary = true;

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.1");

            IList<string> solutions = Dictionary.Solutions.Normalise().ToList();
            IList<string> allWords = useFullDictionary
                ? Dictionary.AllWords.Normalise().ToList()
                : solutions;

            string solution;
            if (useRandomSolution)
            {
                solution = solutions.SingleRandom();
                Console.WriteLine($"Seeking random solution: {solution}");
            }
            else
            {
                int index = DateTime.Now.GetSolutionIndex();
                solution = solutions[index];
                Console.WriteLine($"Seeking solution to Wordle {index}");
            }

            var sw = new Stopwatch();
            sw.Start();

            foreach (var (scores, guess, flags) in allWords.Solve(solutions, solution.GetEvaluator(), useStrictMode))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (GuessScore score in scores.Take(10))
                {
                    Console.WriteLine($"... {score.Guess} {score.AverageMatches:0.##}");
                }
                Console.WriteLine($"Guess: {guess}? -> {guess.ToResultString(flags)}");
            }
        }

        private static IEnumerable<(IList<GuessScore> Scores, string Guess, Flags[] Flags)> Solve(this IList<string> allWords, IList<string> candidates, Func<string, Flags[]> evaluator, bool useStrictMode)
        {
            allWords.Validate(candidates);

            if (!allWords.TryLoadInitialState(out IList<GuessScore> scores))
            {
                scores = allWords.Rank(candidates);
                scores.SaveInitialState();
            }

            Flags[] flags;
            do
            {
                string guess = scores.MinBy(r => r.AverageMatches).SingleRandom().Guess;
                flags = evaluator(guess);

                yield return (scores, guess, flags);

                candidates = candidates.Eliminate(guess, flags).ToList();

                scores = useStrictMode || candidates.Count == 1
                    ? candidates.Rank(candidates)
                    : allWords.Rank(candidates);
            }
            while (!flags.IsSolved() && candidates.Any());
        }

        private static IEnumerable<string> Normalise(this IEnumerable<string> source) => source.Select(s => s.ToUpperInvariant());
    }
}
