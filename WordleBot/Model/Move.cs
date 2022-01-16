using System.Collections.Generic;
using WordleBot.Model;

namespace WordleBot.Solver
{
    public record Move(IList<Score> Scores, string Guess, Flags[] Flags);
}
