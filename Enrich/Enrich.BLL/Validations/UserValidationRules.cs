using Enrich.BLL.Constants;
using FluentValidation;

public static class UserValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
                .NotEmpty().WithMessage(UserConstants.PasswordRequired)
                .MinimumLength(UserConstants.PasswordMinLength).WithMessage(UserConstants.PasswordMinLengthMessage)
                .Matches(UserConstants.PasswordUppercaseRegex).WithMessage(UserConstants.PasswordRequiresUppercase);
    }

    public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
                .NotEmpty().WithMessage(UserConstants.UsernameRequired)
                .MinimumLength(UserConstants.UsernameMinLength).WithMessage(UserConstants.UsernameMinLengthMessage)
                .MaximumLength(UserConstants.UsernameMaxLength).WithMessage(UserConstants.UsernameMaxLengthMessage)
                .Matches(UserConstants.UsernameAllowedCharactersRegex).WithMessage(UserConstants.UsernameInvalidFormat);
    }
}