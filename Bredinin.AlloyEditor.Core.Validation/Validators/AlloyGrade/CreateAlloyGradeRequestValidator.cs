using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Validation.Extensions;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade
{
    public class CreateAlloyGradeRequestValidator : AbstractValidator<CreateAlloyGradeRequest>
    {
        public CreateAlloyGradeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.AlloySystemId)
                .NotEmpty();

            RuleFor(x => x.ChemicalCompositions)
                .NotEmpty()
                .WithMessage("Chemical compositions are required");

            RuleForEach(x => x.ChemicalCompositions)
                .ChildRules(comp =>
                {
                    comp.RuleFor(x => x.ChemicalElementId)
                        .NotEmpty();

                    comp.RuleFor(x => x.MinValue)
                        .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
                        .WithMessage($"MinValue must be between 0 and {ChemicalCompositionValidationExtensions.GetMaxElementValue()}")
                        .When(x => x.MinValue.HasValue);

                    comp.RuleFor(x => x.MaxValue)
                        .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
                        .WithMessage($"MaxValue must be between 0 and {ChemicalCompositionValidationExtensions.GetMaxElementValue()}")
                        .Must((request, maxValue) => 
                            ChemicalCompositionValidationExtensions.IsValidMinMaxRange(request.MinValue, maxValue))
                        .WithMessage("MaxValue must be greater than or equal to MinValue")
                        .When(x => x.MaxValue.HasValue);

                    comp.RuleFor(x => x.ExactValue)
                        .Must(ChemicalCompositionValidationExtensions.IsValidExactValue)
                        .WithMessage($"ExactValue must be between 0 and {ChemicalCompositionValidationExtensions.GetMaxElementValue()}")
                        .When(x => x.ExactValue.HasValue);

                    comp.RuleFor(x => x)
                        .Must(x => ChemicalCompositionValidationExtensions.IsEitherExactOrRange(
                            x.ExactValue, x.MinValue, x.MaxValue))
                        .WithMessage("Either ExactValue or Min/Max must be specified");

                    comp.RuleFor(x => x)
                        .Must(x => ChemicalCompositionValidationExtensions.AreMinAndMaxDifferent(
                            x.MinValue, x.MaxValue))
                        .WithMessage("MinValue and MaxValue must be different");
                });

            RuleFor(x => x.ChemicalCompositions)
                .Must(BeValidTotalRange)
                .WithMessage($"The sum of Min/Max values must not exceed {ChemicalCompositionValidationExtensions.GetMaxPercent()}");
        }

        private static bool BeValidTotalRange(CreateChemicalCompositionRequest[] compositions)
        {
            return ChemicalCompositionValidationExtensions.IsTotalRangeValid(
                compositions,
                x => x.MinValue,
                x => x.MaxValue);
        }
    }
}