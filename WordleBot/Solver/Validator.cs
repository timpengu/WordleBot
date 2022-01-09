using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace WordleBot.Solver
{
    public static class Validator
    {
        public static void Validate(IList<string> allWords, IList<string> candidates)
        {
            int expectedLength = allWords.First().Length;
            string unexpectedLengthWord = allWords.FirstOrDefault(w => w.Length != expectedLength);
            if (unexpectedLengthWord != null)
            {
                throw new Exception($"Word has unexpected length: {unexpectedLengthWord}");
            }

            var missingCandidates = candidates.Except(allWords).Take(10).ToList();
            if (missingCandidates.Any())
            {
                throw new Exception($"Candidate/s not in word list: {String.Join(", ", missingCandidates)}");
            }
        }
    }
}
