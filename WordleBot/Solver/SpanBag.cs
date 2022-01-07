using System;
using System.Linq;

namespace WordleBot.Solver
{
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

            for (--Count; pos < Count; ++pos)
            {
                Span[pos] = Span[pos + 1];
            }

            return true;
        }
    }
}
