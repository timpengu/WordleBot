namespace WordleBot
{
    public struct Options
    {
        public int VocabularySize { get; init; }
        public bool UseRandomSolution { get; init; }
        public bool GuessCandidatesOnly { get; init; }
    };
}
