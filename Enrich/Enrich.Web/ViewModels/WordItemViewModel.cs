namespace Enrich.Web.ViewModels
{
    public class WordItemViewModel
    {
        public int Id { get; set; }

        public string Term { get; set; } = string.Empty;

        public string? Translation { get; set; }
    }
}
