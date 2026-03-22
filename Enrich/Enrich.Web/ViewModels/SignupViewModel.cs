using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;

namespace Enrich.Web.ViewModels
{
    public class SignupViewModel
    {
        [Required(ErrorMessage = UserConstants.EmailRequired)]
        [EmailAddress(ErrorMessage = UserConstants.InvalidEmailFormat)]
        public required string Email { get; set; }

        [Required(ErrorMessage = UserConstants.UsernameRequired)]
        [MinLength(UserConstants.UsernameMinLength, ErrorMessage = UserConstants.UsernameMinLengthMessage)]
        [MaxLength(UserConstants.UsernameMaxLength, ErrorMessage = UserConstants.UsernameMaxLengthMessage)]
        [RegularExpression(UserConstants.UsernameAllowedCharactersRegex, ErrorMessage = UserConstants.UsernameInvalidFormat)]
        public required string Username { get; set; }

        [Required(ErrorMessage = UserConstants.PasswordRequired)]
        [MinLength(UserConstants.PasswordMinLength, ErrorMessage = UserConstants.PasswordMinLengthMessage)]
        [RegularExpression(UserConstants.PasswordUppercaseRegex, ErrorMessage = UserConstants.PasswordRequiresUppercase)]
        public required string Password { get; set; }
    }
}