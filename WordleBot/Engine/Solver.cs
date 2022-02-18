using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MoreLinq;
using WordleBot.Model;
using WordleBot.Persistence;

namespace WordleBot.Engine
{
    public sealed class Solver
    {
        private readonly IReadOnlySet<string> _vocabulary;
        private readonly IReadOnlyCollection<Score> _initialScores;

        private readonly bool _guessCandidatesOnly;

        public Solver(IEnumerable<string> vocabulary, bool guessCandidatesOnly)
        {
            _vocabulary = vocabulary.ToHashSet();
            _guessCandidatesOnly = guessCandidatesOnly;

            _vocabulary.Validate();

            if (!_vocabulary.TryLoad(out _initialScores))
            {
                var sw = Stopwatch.StartNew();
                _initialScores = _vocabulary.CalculateScores();
                _initialScores.Save(sw.Elapsed);
            }
        }

        public IEnumerable<Move> SolveFor(IEvaluator evaluator)
        {
            IReadOnlySet<string> candidates = _vocabulary;
            IReadOnlyCollection<Score> scores = _initialScores;
            Flags[] flags;
            do
            {
                string guess = scores.MaxBy(s => s).SingleRandom().Guess;
                flags = evaluator.EvaluateGuess(guess);

                yield return new Move(scores, guess, flags);

                candidates = candidates.Eliminate(guess, flags).ToHashSet();

                IReadOnlyCollection<string> nextGuesses = GetNextGuesses(candidates);
                scores = nextGuesses.CalculateScores(candidates);
            }
            while (!flags.IsSolved() && candidates.Any());
        }

        private IReadOnlyCollection<string> GetNextGuesses(IReadOnlyCollection<string> candidates) =>
            _guessCandidatesOnly || candidates.Count <= 1
                ? candidates // guess from remaining candidates only
                : _vocabulary; // guess from entire vocabulary
    }
}
