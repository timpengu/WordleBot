using System;
using WordleBot.Dictionaries;

namespace WordleBot.Wordle
{
    public static class DateExtensions
    {
        private static readonly DateTime WordleEpoch = new DateTime(2021, 6, 19);

        public static int GetWordleSolutionIndex(this DateTime date)
        {
            return WordleEpoch.GetDateOffset(date) % WordleDictionary.Solutions.Count;
        }

        private static int GetDateOffset(this DateTime epoch, DateTime date)
        {
            TimeSpan offset = date.Date - epoch.Date;
            return (int)Math.Floor(offset.TotalDays);
        }
    }
}
