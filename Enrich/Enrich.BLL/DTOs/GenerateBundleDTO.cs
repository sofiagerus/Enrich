namespace Enrich.BLL.DTOs
{
    public class GenerateBundleDTO
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public List<BundleGenerationRuleDTO> Rules { get; set; } = new();
    }

    public class BundleGenerationRuleDTO
    {
        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? MinDifficulty { get; set; }

        public string? MaxDifficulty { get; set; }

        public int WordCount { get; set; }
    }
}
