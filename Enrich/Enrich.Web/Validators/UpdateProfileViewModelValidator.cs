using Enrich.Web.ViewModels;
using FluentValidation;

namespace Enrich.Web.Validators
{
    public class UpdateProfileViewModelValidator : AbstractValidator<UpdateProfileViewModel>
    {
        public UpdateProfileViewModelValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Ім'я користувача є обов'язковим.")
                .Length(3, 50).WithMessage("Ім'я користувача повинно містити від 3 до 50 символів.");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Біографія не повинна перевищувати 500 символів.");
        }
    }
}
