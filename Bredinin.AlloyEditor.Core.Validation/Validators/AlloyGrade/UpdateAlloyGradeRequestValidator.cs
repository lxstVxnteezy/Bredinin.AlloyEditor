using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade
{
    public class UpdateAlloyGradeRequestValidator : AbstractValidator<UpdateAlloyGradeRequest>
    {
        public UpdateAlloyGradeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(CreateAlloyGradeRequestValidator.NameMaxLength)
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(CreateAlloyGradeRequestValidator.DescriptionMaxLength);

            RuleFor(x => x.AlloySystemId)
                .NotEqual(Guid.Empty)
                .When(x => x.AlloySystemId.HasValue);
        }
    }
}
