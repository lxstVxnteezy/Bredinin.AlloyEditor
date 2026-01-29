using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade
{
    public class CreateAlloyGradeRequestValidator : AbstractValidator<CreateAlloyGradeRequest>
    {
        private const decimal MaxPercent = 100;
        private const decimal MaxElementValue = 99;

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
                        .InclusiveBetween(0, MaxElementValue)
                        .When(x => x.MinValue.HasValue);

                    comp.RuleFor(x => x.MaxValue)
                        .InclusiveBetween(0, MaxElementValue)
                        .GreaterThanOrEqualTo(x => x.MinValue)
                        .When(x => x.MaxValue.HasValue);

                    comp.RuleFor(x => x.ExactValue)
                        .InclusiveBetween(0, MaxElementValue)
                        .When(x => x.ExactValue.HasValue);

                    comp.RuleFor(x => x)
                        .Must(x =>
                            x.ExactValue.HasValue ||
                            (x.MinValue.HasValue && x.MaxValue.HasValue))
                        .WithMessage("Either ExactValue or Min/Max must be specified");
                  
                    comp.RuleFor(x => x)
                        .Must(x =>
                            !x.MinValue.HasValue ||
                            !x.MaxValue.HasValue ||
                            x.MinValue != x.MaxValue)
                        .WithMessage("MinValue and MaxValue must be different");

                });

            RuleFor(x => x.ChemicalCompositions)
                .Must(BeValidTotalRange)
                .WithMessage("The sum of Min/Max values must not exceed 100");

        }
        private static bool BeValidTotalRange(
            CreateChemicalCompositionRequest[] compositions)
        {
            var totalMin = compositions
                .Where(x => x.MinValue.HasValue)
                .Sum(x => x.MinValue!.Value);

            var totalMax = compositions
                .Where(x => x.MaxValue.HasValue)
                .Sum(x => x.MaxValue!.Value);

            return totalMin <= MaxPercent && totalMax <= MaxPercent;
        }
    }
}
