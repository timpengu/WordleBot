using System.Text.Json.Serialization;

namespace WordleBot.Model
{
    public struct Score
    {
        [JsonConstructor]
        public Score(string guess, double averageMatches)
        {
            Guess = guess;
            AverageMatches = averageMatches;
        }

        [JsonInclude]
        public string Guess { get; }

        [JsonInclude]
        public double AverageMatches { get; }
    }
}
