using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace WordleBot.Engine
{
    public static class RandomExtensions
    {
        private static Random _random = new Random();

        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.OrderBy(_ => _random.Next()).Take(count);
        }

        public static T SingleRandom<T>(this IEnumerable<T> source)
        {
            var items = source.ToList();
            if (items.Count == 0)
            {
                throw new InvalidOperationException("Source is empty");
            }

            return items[_random.Next(items.Count)];
        }
    }
}
