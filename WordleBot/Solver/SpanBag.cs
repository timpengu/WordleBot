using System;
using System.Linq;

namespace WordleBot.Solver
{
    /// <summary>
    /// Implements an unordered bag using a Span for optimisation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public ref struct SpanBag<T> where T : IEquatable<T>
    {
        public SpanBag(Span<T> span)
        {
            Span = span;
            Count = 0;
        }

        private readonly Span<T> Span;
        public int Count { get; private set; }

        public void Add(T value)
        {
            Span[Count++] = value;
        }

        public bool Contains(T value)
        {
            return Span.Slice(0, Count).Contains(value);
        }

        public bool TryRemove(T value)
        {
            int pos = Span.Slice(0, Count).LastIndexOf(value);
            if (pos < 0)
            {
                return false;
            }

            Span[pos] = Span[--Count];
            return true;
        }
    }
}
