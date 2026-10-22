using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.Identity;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login).LoginRules();
        RuleFor(x => x.Password).PasswordRules();
        RuleFor(x => x.FirstName).RequiredNameRules();
        RuleFor(x => x.LastName).RequiredNameRules();
        RuleFor(x => x.SecondName).MaximumLength(IdentityRules.NameMaxLength);
        RuleFor(x => x.Age).InclusiveBetween(IdentityRules.MinAge, IdentityRules.MaxAge);
        RuleFor(x => x.RoleIds).RoleIdsRules();
    }
}
