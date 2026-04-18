using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Validation.Extensions;
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
                .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
                .WithMessage($"MinValue must be between 0 and {ChemicalCompositionValidationExtensions.GetMaxElementValue()}");

            RuleFor(x => x.MaxValue)
                .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
                .WithMessage($"MaxValue must be between 0 and {ChemicalCompositionValidationExtensions.GetMaxElementValue()}")
                .Must((request, maxValue) => 
                    ChemicalCompositionValidationExtensions.IsValidMinMaxRange(request.MinValue, maxValue))
                .WithMessage("MaxValue must be greater than or equal to MinValue");

            RuleFor(x => x)
                .Must(x => ChemicalCompositionValidationExtensions.IsEitherExactOrRange(
                    x.ExactValue, x.MinValue, x.MaxValue))
                .WithMessage("Either ExactValue or Min/Max must be specified");
        }
    }
}