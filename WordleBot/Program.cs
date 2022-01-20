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
        private static readonly IGameDictionary GameDictionary = new WorgleDictionary();

        // TODO: add args switch for behaviours
        private static readonly Options Options = new Options
        {
            // VocabularySize = 0,              // solutions only (lowkey cheating)
            // VocabularySize = 5000,           // a more realistic vocabulary
            // VocabularySize = int.MaxValue,   // use the entire word list (many of which may be outside typical human vocabularies)
            VocabularySize = 5000,

            UseRandomSolution = false,          // select a random solution instead of today's answer for testing
            GuessCandidatesOnly = false         // only guess words matching previous results, aka "hard mode"
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
                Console.WriteLine($"Seeking solution to {GameDictionary.Name} #{index}");
            }

            IReadOnlyCollection<string> vocabulary = GameDictionary.GetVocabulary(Options.VocabularySize);
            IReadOnlySet<string> solutions = GameDictionary.Solutions.ToHashSet();

            Console.WriteLine($"Using vocabulary size {vocabulary.Count}/{solutions.Count}");

            var sw = Stopwatch.StartNew();

            foreach (Move move in vocabulary.Solve(solution.GetEvaluator(), Options.GuessCandidatesOnly))
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
    }
}
