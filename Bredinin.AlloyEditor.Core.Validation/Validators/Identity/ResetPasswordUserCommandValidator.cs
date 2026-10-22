using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.Identity;

public class ResetPasswordUserCommandValidator : AbstractValidator<ResetPasswordUserCommand>
{
    public ResetPasswordUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Password).PasswordRules();
    }
}
