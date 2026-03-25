namespace Enrich.BLL.DTOs
{
    public class CreatePersonalWordDTO
    {
        public string Term { get; set; } = null!;

        public string? Translation { get; set; }

        public string? Transcription { get; set; }

        public string? Meaning { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Example { get; set; }

        public string? DifficultyLevel { get; set; }

        public IEnumerable<int>? CategoryIds { get; set; }
    }
}
