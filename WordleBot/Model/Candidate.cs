using System.Text.Json.Serialization;

namespace WordleBot.Model
{
    public struct CandidateRank
    {
        [JsonConstructor]
        public CandidateRank(string guess, double averageMatches)
        {
            Guess = guess;
            AverageMatches = averageMatches;
        }

        public string Guess { get; }
        public double AverageMatches { get; }
    }
}
