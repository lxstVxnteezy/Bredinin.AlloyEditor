using Bredinin.AlloyEditor.Core.Validation.Extensions;
using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.ChemicalCompositionRequests;

/// <summary>
/// Общие правила для одной строки химического состава
/// </summary>
public abstract class ChemicalCompositionItemValidator<T> : AbstractValidator<T>
{
    protected ChemicalCompositionItemValidator(
        Func<T, Guid> elementId,
        Func<T, decimal?> min,
        Func<T, decimal?> max,
        Func<T, decimal?> exact)
    {
        var limit = ChemicalCompositionValidationExtensions.GetMaxElementValue();

        RuleFor(x => elementId(x))
            .NotEmpty()
            .WithName("ChemicalElementId");

        RuleFor(x => min(x))
            .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
            .WithMessage($"MinValue must be between 0 and {limit}")
            .WithName("MinValue");

        RuleFor(x => max(x))
            .Must(ChemicalCompositionValidationExtensions.IsValidValueRange)
            .WithMessage($"MaxValue must be between 0 and {limit}")
            .WithName("MaxValue");

        RuleFor(x => exact(x))
            .Must(ChemicalCompositionValidationExtensions.IsValidExactValue)
            .WithMessage($"ExactValue must be between 0 and {limit}")
            .WithName("ExactValue");

        RuleFor(x => x)
            .Must(x => ChemicalCompositionValidationExtensions.IsEitherExactOrRange(exact(x), min(x), max(x)))
            .WithMessage("Either ExactValue or Min/Max must be specified");

        RuleFor(x => x)
            .Must(x => ChemicalCompositionValidationExtensions.IsExactNotMixedWithRange(exact(x), min(x), max(x)))
            .WithMessage("ExactValue cannot be combined with MinValue/MaxValue");

        RuleFor(x => x)
            .Must(x => ChemicalCompositionValidationExtensions.IsValidMinMaxRange(min(x), max(x)))
            .WithMessage("MaxValue must be greater than or equal to MinValue");

        RuleFor(x => x)
            .Must(x => ChemicalCompositionValidationExtensions.AreMinAndMaxDifferent(min(x), max(x)))
            .WithMessage("MinValue and MaxValue must be different");
    }
}
