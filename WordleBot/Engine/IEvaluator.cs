using WordleBot.Model;

namespace WordleBot.Engine
{
    public interface IEvaluator
    {
        Flags[] EvaluateGuess(string guess);
    }
}
