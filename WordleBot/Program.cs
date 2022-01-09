using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordleBot.Model;
using WordleBot.Solver;
using WordleBot.Wordle;

namespace WordleBot
{
    public static class Program
    {
        // TODO: add args switch for behaviours
        private static bool useRandomSolution = false;
        private static bool useAllWords = true;
        private static bool useNonCandidates = true;

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.1");

            IList<string> solutions = Dictionary.Solutions.Normalise().ToList();
            IList<string> allWords = useAllWords ? Dictionary.AllWords.Normalise().ToList() : solutions;

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

            var solutionsSet = new HashSet<string>(solutions);
            foreach (var (scores, guess, flags) in allWords.Solve(solutions, solution.GetEvaluator(), useNonCandidates))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (GuessScore score in scores.Take(10))
                {
                    Console.WriteLine($" {(solutionsSet.Contains(score.Guess) ? "*":" ")} {score.Guess} {score.AverageMatches:0.##}");
                }
                Console.WriteLine($"Guess: {guess}? -> {guess.ToFlagString(flags)}");
            }
        }

        private static IEnumerable<string> Normalise(this IEnumerable<string> words) => words.Select(s => s.ToUpperInvariant());
    }
}
