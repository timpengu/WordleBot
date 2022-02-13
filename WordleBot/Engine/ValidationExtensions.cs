using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using WordleBot.Model;

namespace WordleBot.Engine
{
    internal static class ValidationExtensions
    {
        [Conditional("DEBUG")]
        public static void Validate(this IReadOnlyCollection<string> vocabulary)
        {
            int expectedLength = vocabulary.First().Length;
            string unexpectedLengthWord = vocabulary.FirstOrDefault(w => w.Length != expectedLength);
            if (unexpectedLengthWord != null)
            {
                throw new ArgumentException($"Word has unexpected length: {unexpectedLengthWord}");
            }
        }

        [Conditional("DEBUG")]
        public static void ValidateArgument(this string expected, string guess)
        {
            if (guess.Length != expected.Length)
            {
                throw new ArgumentException($"Guess length ({guess.Length}) must match expected length ({expected.Length})");
            }
        }

        [Conditional("DEBUG")]
        public static void ValidateArgument(this string expected, ReadOnlySpan<Flags> flags)
        {
            if (flags.Length != expected.Length)
            {
                throw new ArgumentException($"Flags length ({flags.Length}) must match expected length ({expected.Length})");
            }
        }
    }
}
