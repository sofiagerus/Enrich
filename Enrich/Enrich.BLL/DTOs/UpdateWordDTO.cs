namespace Enrich.BLL.DTOs
{
    public class UpdateWordDTO
    {
        public int WordId { get; set; }

        public string Term { get; set; } = null!;

        public string? Translation { get; set; }

        public string? Meaning { get; set; }

        public string? Example { get; set; }
    }
}