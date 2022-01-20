namespace WordleBot
{
    public record Options
    {
        public int VocabularySize { get; set; }
        public bool UseRandomSolution { get; set; }
        public bool GuessCandidatesOnly { get; set; }
    };
}
