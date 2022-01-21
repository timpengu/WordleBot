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
        private static readonly IGameDictionary[] GameDictionaries =
        {
            new WordleDictionary(),
            new WorgleDictionary()
        };

        public static int Main(string[] args)
        {
            Console.WriteLine("WORDLEbot v1.3");

            try
            {
                Options options = Options.Parse(args);

                IGameDictionary gameDictionary = GameDictionaries.GetNamedDictionary(options.DictionaryName);

                string solution;
                if (options.UseRandomSolution)
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

                IReadOnlyCollection<string> vocabulary = gameDictionary.GetVocabulary(options.VocabularySize);
                IReadOnlySet<string> solutions = gameDictionary.Solutions.ToHashSet();

                Console.WriteLine($"Using vocabulary size {vocabulary.Count} with {solutions.Count} solutions");

                var sw = Stopwatch.StartNew();

                foreach (Move move in vocabulary.Solve(solution.GetEvaluator(), options.GuessCandidatesOnly))
                {
                    Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                    foreach (Score score in move.Scores.Take(10))
                    {
                        bool isInSolutions = solutions.Contains(score.Guess);
                        Console.WriteLine($"  {(isInSolutions ? "*" : " ")} {score.Guess} {score.AverageMatches:0.##} {(score.IsCandidate ? "c" : "")}");
                    }
                    Console.WriteLine($"Guess: {move.Guess}? -> {move.Guess.ToFlagString(move.Flags)}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }
    }
}
