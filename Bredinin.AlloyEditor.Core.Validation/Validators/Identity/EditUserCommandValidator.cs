using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.Identity;

public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
{
    public EditUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).RequiredNameRules();
        RuleFor(x => x.LastName).RequiredNameRules();
        RuleFor(x => x.SecondName).MaximumLength(IdentityRules.NameMaxLength);
        RuleFor(x => x.Age).InclusiveBetween(IdentityRules.MinAge, IdentityRules.MaxAge);
        RuleFor(x => x.RoleIds).RoleIdsRules();
    }
}
