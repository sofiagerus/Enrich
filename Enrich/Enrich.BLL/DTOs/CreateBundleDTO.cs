namespace Enrich.BLL.DTOs
{
    public class CreateBundleDTO
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string[] DifficultyLevels { get; set; } = [];

        public string? ImageUrl { get; set; }

        public IEnumerable<int>? CategoryIds { get; set; }

        public IEnumerable<int>? WordIds { get; set; }

        public IEnumerable<int>? TagIds { get; set; }
    }
}
