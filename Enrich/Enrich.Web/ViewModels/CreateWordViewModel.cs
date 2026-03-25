using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;
using Enrich.DAL.Entities;

namespace Enrich.Web.ViewModels
{
    public class CreateWordViewModel
    {
        [Required(ErrorMessage = WordConstants.TermRequired)]
        [MaxLength(WordConstants.TermMaxLength, ErrorMessage = WordConstants.TermMaxLengthMessage)]
        [Display(Name = "Word")]
        public string Term { get; set; } = string.Empty;

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Please select or type a category")]
        [MaxLength(50, ErrorMessage = "Category name is too long")]
        [Display(Name = "Category")]
        public string? NewCategory { get; set; }

        public IEnumerable<Category>? Categories { get; set; }

        [MaxLength(WordConstants.DifficultyLevelMaxLength, ErrorMessage = WordConstants.DifficultyLevelMaxLengthMessage)]
        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; }

        [MaxLength(WordConstants.TranslationMaxLength, ErrorMessage = WordConstants.TranslationMaxLengthMessage)]
        [Display(Name = "Translation")]
        public string? Translation { get; set; }

        [MaxLength(WordConstants.PartOfSpeechMaxLength, ErrorMessage = WordConstants.PartOfSpeechMaxLengthMessage)]
        [Display(Name = "Part of Speech")]
        public string? PartOfSpeech { get; set; }

        [MaxLength(WordConstants.TranscriptionMaxLength, ErrorMessage = WordConstants.TranscriptionMaxLengthMessage)]
        [Display(Name = "Transcription")]
        public string? Transcription { get; set; }

        [MaxLength(WordConstants.MeaningMaxLength, ErrorMessage = WordConstants.MeaningMaxLengthMessage)]
        [Display(Name = "Meaning")]
        public string? Meaning { get; set; }

        [MaxLength(WordConstants.ExampleMaxLength, ErrorMessage = WordConstants.ExampleMaxLengthMessage)]
        [Display(Name = "Example")]
        public string? Example { get; set; }

        public IEnumerable<int> CategoryIds { get; set; } = [];
    }
}