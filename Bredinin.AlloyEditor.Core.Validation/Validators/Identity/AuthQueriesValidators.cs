using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.Identity;

public class GetJwtTokenQueryValidator : AbstractValidator<GetJwtTokenQuery>
{
    public GetJwtTokenQueryValidator()
    {
        // Правила сложности пароля при входе не применяем: иначе старые пароли перестанут работать
        RuleFor(x => x.Login).NotEmpty().MaximumLength(IdentityRules.LoginMaxLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(IdentityRules.PasswordMaxLength);
    }
}

public class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
{
    public RefreshTokenQueryValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(200);
    }
}

public class RevokeTokenQueryValidator : AbstractValidator<RevokeTokenQuery>
{
    public RevokeTokenQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
