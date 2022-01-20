using System;
using System.Collections.Generic;

namespace WordleBot.Dictionaries
{
    public interface IGameDictionary
    {
        int GetSolutionIndex(DateTime date);
        IReadOnlyList<string> AllWords { get; }
        IReadOnlyList<string> Solutions { get; }
        IReadOnlyList<string> NonSolutions { get; }
        IReadOnlyList<string> WordsByDescFrequency { get; }
    }
}
