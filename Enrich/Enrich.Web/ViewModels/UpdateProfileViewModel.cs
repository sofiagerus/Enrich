using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;

namespace Enrich.Web.ViewModels
{
    public class UpdateProfileViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = UserConstants.UsernameRequired)]
        [MinLength(UserConstants.UsernameMinLength, ErrorMessage = UserConstants.UsernameMinLengthMessage)]
        [MaxLength(UserConstants.UsernameMaxLength, ErrorMessage = UserConstants.UsernameMaxLengthMessage)]
        [RegularExpression(UserConstants.UsernameAllowedCharactersRegex, ErrorMessage = UserConstants.UsernameInvalidFormat)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(UserConstants.BioMaxLength, ErrorMessage = UserConstants.BioMaxLengthMessage)]
        public string? Bio { get; set; }
    }
}
