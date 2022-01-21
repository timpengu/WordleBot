using System;
using System.Text;
using WordleBot.Model;

namespace WordleBot.Solver
{
    internal static class StringExtensions
    {
        public static string ToFlagString(this string guess, Flags[] flags)
        {
            guess.ValidateArgument(flags);

            var sb = new StringBuilder();
            for (int i = 0; i < guess.Length; ++i)
            {
                sb.Append(flags[i] switch
                {
                    Flags.Matched => Char.ToUpperInvariant(guess[i]),
                    Flags.NotInPlace => Char.ToLowerInvariant(guess[i]),
                    Flags.NotMatched => '-',
                    _ => throw new NotImplementedException()
                });
            }
            return sb.ToString();
        }
    }
}
