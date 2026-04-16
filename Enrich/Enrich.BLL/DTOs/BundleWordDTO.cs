namespace Enrich.BLL.DTOs
{
    public class BundleWordDTO
    {
        public int Id { get; set; }

        public string Term { get; set; } = string.Empty;

        public string? Translation { get; set; }

        public string? PartOfSpeech { get; set; }

        public string? Example { get; set; }
    }
}
