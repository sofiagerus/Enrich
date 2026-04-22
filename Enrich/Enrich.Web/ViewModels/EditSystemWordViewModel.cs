using System.ComponentModel.DataAnnotations;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class EditSystemWordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле Term є обов'язковим.")]
        [StringLength(100, ErrorMessage = "Термін має бути від 1 до 100 символів.", MinimumLength = 1)]
        [Display(Name = "Term")]
        public string Term { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Переклад має бути до 100 символів.")]
        [Display(Name = "Translation")]
        public string? Translation { get; set; }

        [StringLength(100, ErrorMessage = "Транскрипція має бути до 100 символів.")]
        [Display(Name = "Transcription")]
        public string? Transcription { get; set; }

        [StringLength(500, ErrorMessage = "Значення має бути до 500 символів.")]
        [Display(Name = "Meaning / Definition")]
        public string? Meaning { get; set; }

        [StringLength(50)]
        [Display(Name = "Part of Speech")]
        public string? PartOfSpeech { get; set; }

        [StringLength(500, ErrorMessage = "Приклад має бути до 500 символів.")]
        [Display(Name = "Example Sentence")]
        public string? Example { get; set; }

        [StringLength(20)]
        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; }

        [Display(Name = "Category")]
        public string? NewCategory { get; set; }

        public IEnumerable<Category>? Categories { get; set; }
    }
}
