namespace Enrich.BLL.DTOs
{
    public class QuizSetupDTO
    {
        public string? Category { get; set; }

        public string? PartOfSpeech { get; set; }

        public int? MinDifficulty { get; set; }

        public int? MaxDifficulty { get; set; }

        public int WordCount { get; set; } = 10;
    }
}
