using Enrich.Web.ViewModels;
using FluentValidation;

namespace Enrich.Web.Validators
{
    public class SignupViewModelValidator : AbstractValidator<SignupViewModel>
    {
        public SignupViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Електронна пошта є обов'язковою")
                .EmailAddress().WithMessage("Неправильний формат пошти");

            RuleFor(x => x.Username).ApplyUsernameRules();

            RuleFor(x => x.Password).ApplyPasswordRules();
        }
    }
}