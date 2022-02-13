using System.Collections.Generic;

namespace WordleBot.Model
{
    public record Move(IReadOnlyCollection<Score> Scores, string Guess, Flags[] Flags);
}
