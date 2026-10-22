using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.Identity;

internal static class IdentityRules
{
    public const int LoginMinLength = 3;
    public const int LoginMaxLength = 60;
    public const int NameMaxLength = 100;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 72; // BCrypt обрезает пароль после 72 байт
    public const int MinAge = 14;
    public const int MaxAge = 100;

    public static IRuleBuilderOptions<T, string> LoginRules<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Length(LoginMinLength, LoginMaxLength)
            .Matches("^[A-Za-z0-9._-]+$")
            .WithMessage("Login may contain only latin letters, digits, '.', '_' and '-'");

    public static IRuleBuilderOptions<T, string> PasswordRules<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(PasswordMinLength)
            .MaximumLength(PasswordMaxLength)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

    public static IRuleBuilderOptions<T, string> RequiredNameRules<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(NameMaxLength);

    public static IRuleBuilderOptions<T, Guid[]> RoleIdsRules<T>(this IRuleBuilder<T, Guid[]> rule) =>
        rule.NotNull()
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Role ids must not be empty")
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithMessage("Role ids must not be repeated");
}
