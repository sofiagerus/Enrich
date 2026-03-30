using System.ComponentModel.DataAnnotations;

namespace Enrich.Web.ViewModels
{
    public class EditWordViewModel
    {
        public int WordId { get; set; }

        [Required(ErrorMessage = "Поле 'Термін' є обов'язковим")]
        public string Term { get; set; } = null!;

        public string? Translation { get; set; }

        public string? Meaning { get; set; }

        public string? Example { get; set; }
    }
}