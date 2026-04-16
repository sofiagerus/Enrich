namespace Enrich.BLL.DTOs
{
    public class StudySessionDTO
    {
        public int SessionId { get; set; }

        public int BundleId { get; set; }

        public string BundleTitle { get; set; } = null!;

        public List<StudyCardDTO> Cards { get; set; } = new();

        public int TotalCards { get; set; }

        public DateTime StartedAt { get; set; }
    }

    public class StudyCardDTO
    {
        public int WordId { get; set; }

        public string Term { get; set; } = null!;

        public string? Translation { get; set; }

        public string? Transcription { get; set; }

        public string? Meaning { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Example { get; set; }

        public string? ImageUrl { get; set; }

        public string? DifficultyLevel { get; set; }
    }

    public class StudyProgressDTO
    {
        public int SessionId { get; set; }

        public int TotalCards { get; set; }

        public int KnownCount { get; set; }

        public int UnknownCount { get; set; }

        public int TotalPoints { get; set; }

        public double ProgressPercentage => TotalCards > 0 ? (KnownCount + UnknownCount) / (double)TotalCards * 100 : 0;
    }

    public class SubmitAnswerDTO
    {
        public int SessionId { get; set; }

        public int WordId { get; set; }

        public bool IsKnown { get; set; }
    }
}
