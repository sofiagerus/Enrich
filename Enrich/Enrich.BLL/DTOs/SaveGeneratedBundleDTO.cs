namespace Enrich.BLL.DTOs
{
    public class SaveGeneratedBundleDTO
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public IEnumerable<int> WordIds { get; set; } = Array.Empty<int>();

        public string[] DifficultyLevels { get; set; } = [];

        public IEnumerable<string> CategoryNames { get; set; } = Array.Empty<string>();
    }
}
