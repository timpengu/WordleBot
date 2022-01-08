using System.Text.Json.Serialization;

namespace WordleBot.Model
{
    public struct GuessScore
    {
        [JsonConstructor]
        public GuessScore(string guess, double averageMatches)
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
