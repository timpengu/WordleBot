using WordleBot.Model;

namespace WordleBot.Engine
{
    internal class SolutionEvaluator : IEvaluator
    {
        private readonly string _solution;

        public SolutionEvaluator(string solution)
        {
            _solution = solution;
        }

        public Flags[] EvaluateGuess(string guess) => _solution.EvaluateGuess(guess);
    }
}
