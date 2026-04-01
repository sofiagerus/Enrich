namespace Enrich.BLL.DTOs
{
    public class SystemBundleDTO
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int WordCount { get; set; }

        public string[] DifficultyLevels { get; set; } = [];

        public List<string> Categories { get; set; } = [];
    }
}
