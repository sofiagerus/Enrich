using System.ComponentModel.DataAnnotations;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class EditSystemWordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Term field is required.")]
        [StringLength(100, ErrorMessage = "Term must be between 1 and 100 characters.", MinimumLength = 1)]
        [Display(Name = "Term")]
        public string Term { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Translation must be up to 100 characters.")]
        [Display(Name = "Translation")]
        public string? Translation { get; set; }

        [StringLength(100, ErrorMessage = "Transcription must be up to 100 characters.")]
        [Display(Name = "Transcription")]
        public string? Transcription { get; set; }

        [StringLength(500, ErrorMessage = "Value must be up to 500 characters.")]
        [Display(Name = "Meaning / Definition")]
        public string? Meaning { get; set; }

        [StringLength(50)]
        [Display(Name = "Part of Speech")]
        public string? PartOfSpeech { get; set; }

        [StringLength(500, ErrorMessage = "Example must be up to 500 characters.")]
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
