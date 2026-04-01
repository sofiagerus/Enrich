using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;
using Enrich.DAL.Entities.Enums;

namespace Enrich.Web.ViewModels
{
    public class EditBundleViewModel
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = BundleConstants.TitleRequired)]
        [MaxLength(BundleConstants.TitleMaxLength, ErrorMessage = BundleConstants.TitleMaxLengthMessage)]
        [Display(Name = "Collection Title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(BundleConstants.DescriptionMaxLength, ErrorMessage = BundleConstants.DescriptionMaxLengthMessage)]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Categories")]
        public List<int> CategoryIds { get; set; } = new();

        [Display(Name = "Words")]
        public List<int> WordIds { get; set; } = new();

        [Display(Name = "Difficulty Levels")]
        public List<string> DifficultyLevels { get; set; } = new();

        [MaxLength(BundleConstants.ImageUrlMaxLength, ErrorMessage = BundleConstants.ImageUrlMaxLengthMessage)]
        [Display(Name = "Cover Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Status")]
        public BundleStatus Status { get; set; }

        public List<(int Id, string Name)> Categories { get; set; } = new();

        public List<WordItemViewModel> Words { get; set; } = new();

        public List<string> AvailableLevels { get; set; } = new();
    }
}
