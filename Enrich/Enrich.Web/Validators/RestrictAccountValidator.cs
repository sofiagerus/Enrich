using Enrich.Web.ViewModels;
using FluentValidation;

namespace Enrich.Web.Validators
{
    public class RestrictAccountValidator : AbstractValidator<RestrictAccountViewModel>
    {
        public RestrictAccountValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("ID користувача є обов'язковим.");
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Вкажіть причину блокування.")
                .MaximumLength(500).WithMessage("Причина надто довга.");
        }
    }
}