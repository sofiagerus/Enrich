namespace Enrich.Web.ViewModels
{
    public class StudySessionViewModel
    {
        public int SessionId { get; set; }

        public int BundleId { get; set; }

        public string BundleTitle { get; set; } = null!;

        public List<StudyCardViewModel> Cards { get; set; } = new();

        public int TotalCards { get; set; }

        public DateTime StartedAt { get; set; }
    }

    public class StudyCardViewModel
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

    public class StudyProgressViewModel
    {
        public int SessionId { get; set; }

        public int TotalCards { get; set; }

        public int KnownCount { get; set; }

        public int UnknownCount { get; set; }

        public int TotalPoints { get; set; }

        public double ProgressPercentage { get; set; }
    }
}
