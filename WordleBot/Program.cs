using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordleBot.Dictionaries;
using WordleBot.Model;
using WordleBot.Engine;
using MoreLinq;

namespace WordleBot
{
    public static class Program
    {
        private static readonly IGameDictionary[] GameDictionaries =
        {
            new WordleDictionary(),
            new WorgleDictionary()
        };

        public static int Main(string[] args)
        {
            Console.WriteLine("WORDLEbot v1.4");

            try
            {
                Options options = Options.Parse(args);

                IGameDictionary gameDictionary = GameDictionaries.GetNamedDictionary(options.DictionaryName);
                IReadOnlyCollection<string> vocabulary = gameDictionary.GetVocabulary(options.VocabularySize);
                IReadOnlyCollection<string> solutions = gameDictionary.Solutions;

                Console.WriteLine($"Using vocabulary size {vocabulary.Count} with {solutions.Count} solutions");

                if (options.CalculateStatistics)
                {
                    vocabulary.CalculateStatistics(
                        solutions,
                        options.GuessCandidatesOnly);
                }
                else
                {
                    string solution = gameDictionary.GetSolution(options.UseRandomSolution);

                    vocabulary.Solve(
                        solution.GetEvaluator(),
                        solutions.Contains,
                        options.GuessCandidatesOnly);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }

        private static string GetSolution(this IGameDictionary gameDictionary, bool useRandomSolution)
        {
            string solution;
            if (useRandomSolution)
            {
                solution = gameDictionary.Solutions.SingleRandom();
                Console.WriteLine($"Seeking random solution: {solution}");
            }
            else
            {
                int index = gameDictionary.GetSolutionIndex(DateTime.Now.Date);
                solution = gameDictionary.Solutions[index];
                Console.WriteLine($"Seeking solution for {gameDictionary.Name} #{index}");
            }
            return solution;
        }

        private static void Solve(this IReadOnlyCollection<string> vocabulary, IEvaluator solutionEvaluator, Func<string, bool> isInSolutions, bool guessCandidatesOnly)
        {
            var sw = Stopwatch.StartNew();
            var solver = new Solver(vocabulary, guessCandidatesOnly);

            foreach (Move move in solver.SolveFor(solutionEvaluator))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");

                foreach (Score score in move.Scores.Take(10).Union(move.Scores.Where(s => s.IsCandidate).Take(10)))
                {
                    Console.WriteLine($"  {(isInSolutions(score.Guess) ? "*" : " ")} {score.Guess} {score.AverageMatches:0.##} {(score.IsCandidate ? "c" : "")}");
                }

                Console.WriteLine($"Guess: {move.Guess}? -> {move.Guess.ToFlagString(move.Flags)}");
            }
        }

        private static void CalculateStatistics(this IReadOnlyCollection<string> vocabulary, IEnumerable<string> solutions, bool guessCandidatesOnly)
        {
            var sw = Stopwatch.StartNew();
            var solver = new Solver(vocabulary, guessCandidatesOnly);

            int maxGuesses = 6;
            var guessesDistribution = new int[maxGuesses+2];

            Console.WriteLine("Calculating statistics over all solutions...");

            foreach (string solution in solutions)
            {
                TimeSpan elapsedPre = sw.Elapsed;

                int guesses = solver.SolveFor(solution.GetEvaluator()).Take(maxGuesses+1).Count();
                ++guessesDistribution[guesses];

                TimeSpan elapsedPost = sw.Elapsed;
                double elapsedSeconds = (elapsedPost - elapsedPre).TotalSeconds;

                Console.WriteLine($"[{elapsedPost:hh\\:mm\\:ss\\.fff}] {solution}: {guesses} [{elapsedSeconds:0.00}s]");
            }

            Console.WriteLine("\nGuess distribution:");
            Enumerable.Range(1, maxGuesses).ForEach(guesses =>
                Console.WriteLine($"{guesses}: {guessesDistribution[guesses]}")
            );
            Console.WriteLine($"Lost: {guessesDistribution[maxGuesses+1]}");
        }
    }
}
