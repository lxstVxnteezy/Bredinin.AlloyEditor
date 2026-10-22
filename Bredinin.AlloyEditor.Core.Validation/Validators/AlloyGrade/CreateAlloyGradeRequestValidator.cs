using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Validation.Extensions;
using Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests;
using Bredinin.AlloyEditor.Core.Validation.Validators.HeatTreatment;
using Bredinin.AlloyEditor.Core.Validation.Validators.MechanicalProperty;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.AlloyGrade
{
    public class CreateAlloyGradeRequestValidator : AbstractValidator<CreateAlloyGradeRequest>
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 1000;

        public CreateAlloyGradeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(NameMaxLength);

            RuleFor(x => x.Description)
                .MaximumLength(DescriptionMaxLength);

            RuleFor(x => x.AlloySystemId)
                .NotEmpty();

            RuleFor(x => x.ChemicalCompositions)
                .NotEmpty()
                .WithMessage("Chemical compositions are required");

            RuleForEach(x => x.ChemicalCompositions)
                .SetValidator(new CreateChemicalCompositionItemValidator());

            RuleFor(x => x.ChemicalCompositions)
                .Must(c => ChemicalCompositionValidationExtensions.HasUniqueElements(c, x => x.ChemicalElementId))
                .WithMessage("Chemical elements must not be repeated")
                .When(x => x.ChemicalCompositions is { Length: > 0 });

            RuleFor(x => x.ChemicalCompositions)
                .Must(BeValidTotalRange)
                .WithMessage($"The sum of minimum values must not exceed {ChemicalCompositionValidationExtensions.GetMaxPercent()}")
                .When(x => x.ChemicalCompositions is { Length: > 0 });

            RuleForEach(x => x.HeatTreatments)
                .SetValidator(new CreateHeatTreatmentRequestValidator())
                .When(x => x.HeatTreatments != null);

            RuleForEach(x => x.DefaultMechanicalProperties)
                .SetValidator(new CreateMechanicalPropertyRequestValidator())
                .When(x => x.DefaultMechanicalProperties != null);
        }

        private static bool BeValidTotalRange(CreateChemicalCompositionRequest[] compositions)
        {
            return ChemicalCompositionValidationExtensions.IsTotalRangeValid(
                compositions,
                x => x.MinValue,
                x => x.MaxValue,
                x => x.ExactValue);
        }

        // internal: чтобы MVC не запускал валидатор повторно для вложенных элементов
        internal sealed class CreateChemicalCompositionItemValidator
            : ChemicalCompositionItemValidator<CreateChemicalCompositionRequest>
        {
            public CreateChemicalCompositionItemValidator()
                : base(x => x.ChemicalElementId, x => x.MinValue, x => x.MaxValue, x => x.ExactValue)
            {
            }
        }
    }
}
