using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordleBot.Model;
using WordleBot.Persistence;
using WordleBot.Solver;

namespace WordleBot
{
    public static class Program
    {
        private static readonly DateTime WordleEpoch = new DateTime(2021, 6, 19);

        private static int GetWordleIndex() => WordleEpoch.GetDateOffset(DateTime.Now) % Dictionary.Words.Length;

        // TODO: add args switch for date/random mode
        private static bool useRandomSolution = false;

        public static void Main()
        {
            Console.WriteLine("WORDLEbot v1.0");

            Dictionary.Words.Validate();

            List<string> wordList =
                Dictionary.Words
                .Select(s => s.ToUpperInvariant())
                .ToList();

            // TODO: Incorporate full word list including non-solutions (human players don't have the advantage of knowing whether a word is in the solutions list)

            string solution;
            if (useRandomSolution)
            {
                solution = wordList.SingleRandom();
                Console.WriteLine($"Seeking solution: {solution}");
            }
            else
            {
                int index = GetWordleIndex();
                solution = wordList[index];
                Console.WriteLine($"Seeking solution for Wordle {index}");
            }

            var sw = new Stopwatch();
            sw.Start();

            foreach (var (rankedCandidates, guess, flags) in wordList.Solve(solution.GetEvaluator()))
            {
                Console.WriteLine($"\n[{sw.Elapsed:hh\\:mm\\:ss\\.fff}]");
                foreach (CandidateRank candidate in rankedCandidates.Take(10))
                {
                    Console.WriteLine($"... {candidate.Guess} {candidate.AverageMatches:0.##}");
                }
                Console.WriteLine($"Guess: {guess}? -> {guess.ToResultString(flags)}");
            }
        }

        private static IEnumerable<(IList<CandidateRank> RankedCandidates, string Guess, Flags[] Flags)> Solve(this IList<string> candidates, Func<string, Flags[]> evaluator)
        {
            if (!candidates.TryLoadInitialState(out IList<CandidateRank> rankedCandidates))
            {
                rankedCandidates = candidates.Rank();
                rankedCandidates.SaveInitialState();
            }

            Flags[] flags;
            do
            {
                string guess = rankedCandidates.MinBy(c => c.AverageMatches).SingleRandom().Guess;
                flags = evaluator(guess);

                yield return (rankedCandidates, guess, flags);

                candidates = candidates.Eliminate(guess, flags).ToList();
                rankedCandidates = candidates.Rank();
            }
            while (!flags.IsSolved() && candidates.Any());
        }

        private static int GetDateOffset(this DateTime epoch, DateTime date)
        {
            TimeSpan offset = date.Date - epoch.Date;
            return (int) Math.Floor(offset.TotalDays);
        }
    }
}
