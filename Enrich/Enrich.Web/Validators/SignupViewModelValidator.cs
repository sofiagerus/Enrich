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

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Ім'я користувача є обов'язковим")
                .MinimumLength(3).WithMessage("Ім'я має містити мінімум 3 символи")
                .MaximumLength(20).WithMessage("Ім'я не може бути довшим за 20 символів");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль є обов'язковим")
                .MinimumLength(8).WithMessage("Пароль має містити мінімум 8 символів")
                .Matches("[A-Z]").WithMessage("Пароль повинен містити хоча б одну велику літеру")
                .Matches("[0-9]").WithMessage("Пароль повинен містити хоча б одну цифру");
        }
    }
}