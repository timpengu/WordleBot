namespace WordleBot
{
    public record Options
    {
        public bool UseRandomSolution { get; set; }
        public VocabularyMode VocabularyMode { get; set; }
        public bool GuessCandidatesOnly { get; set; }
    };
}
