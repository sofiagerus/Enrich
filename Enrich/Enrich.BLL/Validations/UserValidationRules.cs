using FluentValidation;

public static class UserValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
                .NotEmpty().WithMessage("Пароль є обов'язковим")
                .MinimumLength(8).WithMessage("Пароль має містити мінімум 8 символів")
                .Matches("[A-Z]").WithMessage("Потрібна хоча б одна велика літера");
    }

    public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
                .NotEmpty().WithMessage("Ім'я користувача є обов'язковим")
                .MinimumLength(3).WithMessage("Ім'я має містити мінімум 3 символи")
                .MaximumLength(16).WithMessage("Ім'я не може бути довшим за 16 символів")
                .Matches("^[a-zA-Z._]*$").WithMessage("Дозволені лише літери латинського алфавіту, крапка та нижнє підкреслення");
    }
}