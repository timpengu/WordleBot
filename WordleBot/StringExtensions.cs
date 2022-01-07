using System;
using System.Text;
using WordleBot.Model;

namespace WordleBot
{
    internal static class StringExtensions
    {
        public static string ToResultString(this string guess, Flags[] flags)
        {
            if (flags.Length != guess.Length)
            {
                throw new ArgumentException("Flags length must match guess length");
            }

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
