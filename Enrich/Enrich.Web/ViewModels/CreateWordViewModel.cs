using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;

namespace Enrich.Web.ViewModels
{
    public class CreateWordViewModel
    {
        [Required(ErrorMessage = WordConstants.TermRequired)]
        [MaxLength(WordConstants.TermMaxLength, ErrorMessage = WordConstants.TermMaxLengthMessage)]
        [Display(Name = "Word")]
        public string Term { get; set; } = null!;

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
    }
}
