using System.ComponentModel.DataAnnotations;
using Enrich.BLL.Constants;

namespace Enrich.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = UserConstants.EmailRequired)]
        [EmailAddress(ErrorMessage = UserConstants.InvalidEmailFormat)]
        public required string Email { get; set; }

        [Required(ErrorMessage = UserConstants.PasswordRequired)]
        [MinLength(UserConstants.PasswordMinLength, ErrorMessage = UserConstants.PasswordMinLengthMessage)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
