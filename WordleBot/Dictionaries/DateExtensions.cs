using System;
using System.Collections.Generic;

namespace WordleBot.Dictionaries
{
    public static class DateExtensions
    {
        public static int GetSolutionIndex(this IReadOnlyCollection<string> solutions, DateTime date, DateTime epoch)
        {
            return date.GetDateOffset(epoch) % solutions.Count;
        }

        private static int GetDateOffset(this DateTime date, DateTime epoch)
        {
            TimeSpan offset = date.Date - epoch.Date;
            return (int)Math.Floor(offset.TotalDays);
        }
    }
}
