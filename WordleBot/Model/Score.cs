using System;
using System.Text.Json.Serialization;

namespace WordleBot.Model
{
    public struct Score : IComparable<Score>
    {
        [JsonConstructor]
        public Score(string guess, bool isCandidate, double averageMatches)
        {
            Guess = guess;
            IsCandidate = isCandidate;
            AverageMatches = averageMatches;
        }

        [JsonInclude]
        public string Guess { get; }

        [JsonIgnore]
        public bool IsCandidate { get; }

        [JsonInclude]
        public double AverageMatches { get; }

        public int CompareTo(Score other)
        {
            int compareAverageMatches = -(AverageMatches.CompareTo(other.AverageMatches)); // descending (lower AverageMatches are better)
            if (compareAverageMatches != 0)
                return compareAverageMatches;

            int compareIsCandidate = IsCandidate.CompareTo(other.IsCandidate);
            if (compareIsCandidate != 0)
                return compareIsCandidate;

            return 0;
        }

        public override string ToString() => $"{Guess} {AverageMatches} {(IsCandidate ? "C" : "")}";
    }
}
