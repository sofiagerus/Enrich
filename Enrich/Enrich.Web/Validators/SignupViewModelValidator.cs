using Enrich.BLL.Constants;
using Enrich.Web.ViewModels;
using FluentValidation;

namespace Enrich.Web.Validators
{
    public class SignupViewModelValidator : AbstractValidator<SignupViewModel>
    {
        public SignupViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(UserConstants.EmailRequired)
                .EmailAddress().WithMessage(UserConstants.InvalidEmailFormat);

            RuleFor(x => x.Username).ApplyUsernameRules();

            RuleFor(x => x.Password).ApplyPasswordRules();
        }
    }
}