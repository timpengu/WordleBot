﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using WordleBot.Model;

namespace WordleBot.Solver
{
    public static class MatchingExtensions
    {
        public static void Validate(this IList<string> candidates)
        {
            var firstCandidate = candidates.First();
            var inconsistentCandidate = candidates.Skip(1).FirstOrDefault(candidate => candidate.Length != firstCandidate.Length);
            if (inconsistentCandidate != null)
            {
                throw new Exception($"Inconsistent lengths: {firstCandidate} {inconsistentCandidate}");
            }
        }

        public static bool IsSolved(this Flags[] flags) => flags.All(f => f == Flags.Matched);

        public static IList<CandidateRank> Rank(this IList<string> candidates)
        {
            return candidates
                .AsParallel()
                .Select(guess => new CandidateRank(guess, candidates.GetAverageMatches(guess)))
                .OrderBy(g => g.AverageMatches)
                .ToList();
        }

        public static double GetAverageMatches(this IList<string> candidates, string guess)
        {
            // calculate the average number of remaining candidates given this guess
            return candidates
                .Select(candidate =>
                {
                    // supposing this candidate is the solution, find the number of candidates remaining after this guess
                    Flags[] flags = candidate.EvaluateGuess(guess);
                    return (double)candidates.Eliminate(guess, flags).Count();
                })
                .Average();
        }

        public static IEnumerable<string> Eliminate(this IEnumerable<string> candidates, string guess, Flags[] flags)
        {
            return candidates.Where(candidate => candidate.IsMatch(guess, flags));
        }

        // TODO: optimise this for inner loop
        public static bool IsMatch(this string candidate, string guess, Flags[] flags)
        {
            if (flags.Length != guess.Length)
            {
                throw new ArgumentException("Flags length must match guess length");
            }

            if (guess.Length != candidate.Length)
            {
                throw new InvalidOperationException("Guess length must match candidate length");
            }

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

        public static Func<string, Flags[]> GetEvaluator(this string solution) => guess => solution.EvaluateGuess(guess);

        // TODO: optimise, return Span<Flags> ?
        public static Flags[] EvaluateGuess(this string solution, string guess)
        {
            if (guess.Length != solution.Length)
            {
                throw new InvalidOperationException("Guess length must match solution length");
            }

            var flags = new Flags[guess.Length]; // all Flags.NotPresent
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

            return flags;
        }

        private static bool TryRemove(this List<char> chars, char value)
        {
            int pos = chars.FindIndex(c => c == value);
            if (pos < 0)
            {
                return false;
            }

            chars.RemoveAt(pos);
            return true;
        }
    }
}
