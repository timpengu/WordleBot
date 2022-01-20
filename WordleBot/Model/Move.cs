using System.Collections.Generic;
using WordleBot.Model;

namespace WordleBot.Solver
{
    public record Move(IReadOnlyCollection<Score> Scores, string Guess, Flags[] Flags);
}
