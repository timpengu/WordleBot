using System.Collections.Generic;

namespace WordleBot.Model
{
    public record InitialState(IList<GuessScore> Candidates);
}
