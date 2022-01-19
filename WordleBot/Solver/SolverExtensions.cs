using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using WordleBot.Model;
using WordleBot.Persistence;

namespace WordleBot.Solver
{
    public static class SolverExtensions
    {
        public static Func<string, Flags[]> GetEvaluator(this string solution) => guess => solution.EvaluateGuess(guess);
        public static bool IsSolved(this Flags[] flags) => flags.All(f => f == Flags.Matched);

        public static IEnumerable<Move> Solve(this IReadOnlyList<string> vocabulary, IReadOnlyList<string> candidates, Func<string, Flags[]> evaluator, bool guessCandidatesOnly)
        {
            Validator.Validate(vocabulary, candidates);

            if (!vocabulary.TryLoad(out IReadOnlyList<Score> scores))
            {
                scores = vocabulary.CalculateScores(candidates);
                scores.Save();
            }

            Flags[] flags;
            do
            {
                string guess = scores.MinBy(r => r.AverageMatches).SingleRandom().Guess;
                flags = evaluator(guess);

                yield return new Move(scores, guess, flags);

                candidates = candidates.Eliminate(guess, flags).ToList();

                IReadOnlyList<string> nextGuesses = (guessCandidatesOnly || candidates.Count == 1) ? candidates : vocabulary;
                scores = nextGuesses.CalculateScores(candidates);
            }
            while (!flags.IsSolved() && candidates.Any());
        }

        private static IReadOnlyList<Score> CalculateScores(this IEnumerable<string> allWords, IReadOnlyList<string> candidates)
        {
            return allWords
                .AsParallel() // TODO: benchmark whether to parallelise outer or inner loop
                .Select(guess => new Score(guess, candidates.GetAverageMatches(guess)))
                .OrderBy(g => g.AverageMatches)
                .ToList();
        }

        private static double GetAverageMatches(this IReadOnlyList<string> candidates, string guess)
        {
            // calculate the average number of remaining candidates given this guess
            return candidates
                .Select(candidate =>
                {
                    // supposing this candidate is the solution, find the number of candidates remaining after this guess
                    Span<Flags> flags = stackalloc Flags[guess.Length];
                    candidate.EvaluateGuess(guess, flags);
                    return (double) candidates.CountMatches(guess, flags);
                })
                .Average();
        }

        private static IEnumerable<string> Eliminate(this IEnumerable<string> candidates, string guess, Flags[] flags)
        {
            return candidates.Where(candidate => candidate.IsMatch(guess, flags));
        }

        private static int CountMatches(this IEnumerable<string> candidates, string guess, ReadOnlySpan<Flags> flags)
        {
            // CS1628: Cannot use ref parameter 'flags' inside a lambda expression
            // return candidates.Count(candidate => candidate.IsMatch(guess, flags));

            int count = 0;
            foreach(var candidate in candidates)
            {
                if (candidate.IsMatch(guess, flags))
                {
                    ++count;
                }
            }
            return count;
        }

        private static bool IsMatch(this string candidate, string guess, ReadOnlySpan<Flags> flags)
        {
            candidate.ValidateArgument(guess);
            candidate.ValidateArgument(flags);

            // Validate matched characters
            var unmatchedChars = new SpanBag<char>(stackalloc char[guess.Length]);
            for (int i = 0; i < guess.Length; ++i)
            {
                bool isMatch = guess[i] == candidate[i];
                bool shouldMatch = flags[i] == Flags.Matched;

                if (isMatch != shouldMatch)
                {
                    return false; // character must match in place iff it is flagged matched
                }

                if (!isMatch)
                {
                    unmatchedChars.Add(candidate[i]);
                }
            }

            // Validate misplaced characters
            for (int i = 0; i < guess.Length; ++i)
            {
                if (flags[i] == Flags.NotInPlace)
                {
                    if (!unmatchedChars.TryRemove(guess[i]))
                    {
                        return false; // character must match a remaining candidate (out of place)
                    }
                }
            }

            // Validate non-matched characters
            for (int i = 0; i < guess.Length; ++i)
            {
                if (flags[i] == Flags.NotMatched)
                {
                    if (unmatchedChars.Contains(guess[i]))
                    {
                        return false; // character must not match any remaining candidate
                    }
                }
            }

            return true;
        }

        private static Flags[] EvaluateGuess(this string solution, string guess)
        {
            var flags = new Flags[guess.Length];
            solution.EvaluateGuess(guess, flags);
            return flags;
        }

        private static void EvaluateGuess(this string solution, string guess, Span<Flags> flags)
        {
            solution.ValidateArgument(guess);
            solution.ValidateArgument(flags);

            var unmatchedChars = new SpanBag<char>(stackalloc char[guess.Length]);

            // Identify matched characters
            for (int i = 0; i < guess.Length; ++i)
            {
                if (guess[i] == solution[i])
                {
                    flags[i] = Flags.Matched;
                }
                else
                {
                    flags[i] = Flags.NotMatched;
                    unmatchedChars.Add(solution[i]);
                }
            }

            // Identify misplaced characters
            for (int i = 0; i < guess.Length; ++i)
            {
                if (flags[i] != Flags.Matched)
                {
                    if (unmatchedChars.TryRemove(guess[i]))
                    {
                        flags[i] = Flags.NotInPlace;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private static void ValidateArgument(this string expected, string guess)
        {
            if (guess.Length != expected.Length)
            {
                throw new ArgumentException($"Guess length ({guess.Length}) must match expected length ({expected.Length})");
            }
        }

        [Conditional("DEBUG")]
        private static void ValidateArgument(this string expected, ReadOnlySpan<Flags> flags)
        {
            if (flags.Length != expected.Length)
            {
                throw new ArgumentException($"Flags length ({flags.Length}) must match expected length ({expected.Length})");
            }
        }
    }
}
