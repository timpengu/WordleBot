using System;

namespace WordleBot.Wordle
{
    public static class DateExtensions
    {
        private static readonly DateTime WordleEpoch = new DateTime(2021, 6, 19);

        public static int GetSolutionIndex(this DateTime date)
        {
            return WordleEpoch.GetDateOffset(date) % Dictionary.Solutions.Length;
        }

        private static int GetDateOffset(this DateTime epoch, DateTime date)
        {
            TimeSpan offset = date.Date - epoch.Date;
            return (int)Math.Floor(offset.TotalDays);
        }
    }
}
