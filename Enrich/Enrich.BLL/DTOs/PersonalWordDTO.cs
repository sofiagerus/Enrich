namespace Enrich.BLL.DTOs
{
    public class PersonalWordDTO
    {
        public int Id { get; set; }

        public string Term { get; set; } = string.Empty;

        public string? Translation { get; set; }

        public string? Transcription { get; set; }

        public string? Meaning { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Example { get; set; }

        public string? DifficultyLevel { get; set; }

        public DateTime AddedAt { get; set; }

        public string? CategoryName { get; internal set; }
    }
}
