using System.Collections.Generic;
using WordleBot.Model;

namespace WordleBot.Solver
{
    public record Move(IReadOnlyList<Score> Scores, string Guess, Flags[] Flags);
}
