using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WordleBot.Model;

namespace WordleBot.Persistence
{
    public class ScoresFile
    {
        public ScoresFile(int vocabularySize, TimeSpan computeTime, IReadOnlyCollection<Score> scores) :
            this(vocabularySize, computeTime.TotalSeconds, scores)
        {
        }

        [JsonConstructor]
        public ScoresFile(int vocabularySize, double computeTimeSeconds, IReadOnlyCollection<Score> scores)
        {
            VocabularySize = vocabularySize;
            ComputeTimeSeconds = computeTimeSeconds;
            Scores = scores;
        }

        [JsonInclude]
        public int VocabularySize { get; }

        [JsonInclude]
        public double ComputeTimeSeconds { get; }

        [JsonInclude]
        public IReadOnlyCollection<Score> Scores { get; }
    }
}
