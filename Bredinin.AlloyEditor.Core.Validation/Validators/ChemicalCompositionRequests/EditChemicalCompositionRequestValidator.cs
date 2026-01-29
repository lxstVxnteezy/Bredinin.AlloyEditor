using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests
{
    public class EditChemicalCompositionRequestValidator 
        : AbstractValidator<ChemicalCompositionsRequest>
    {
        public EditChemicalCompositionRequestValidator()
        {
            RuleFor(x => x.ChemicalElementId)
                .NotEmpty();

            RuleFor(x => x.MinValue)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(99);

            RuleFor(x => x.MaxValue)
                .GreaterThanOrEqualTo(x => x.MinValue)
                .LessThanOrEqualTo(99);

            RuleFor(x => x)
                .Must(x =>
                    x.ExactValue.HasValue ||
                    x.MinValue.HasValue && x.MaxValue.HasValue)
                .WithMessage("Either ExactValue or Min/Max must be specified");
        }
    }
}
