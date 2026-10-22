using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.HeatTreatment;

internal static class HeatTreatmentRules
{
    public const int MinTemperature = -273;
    public const int MaxTemperature = 3000;
    public const int MaxHoldingTimeMinutes = 60 * 24 * 365;
    public const int CoolingMediumMaxLength = 100;
    public const int DescriptionMaxLength = 1000;

    public static void AddCommonRules<T>(
        AbstractValidator<T> validator,
        Func<T, int?> tMin, Func<T, int?> tMax, Func<T, int?> tExact,
        Func<T, int?> hMin, Func<T, int?> hMax, Func<T, int?> hExact,
        Func<T, string?> cooling, Func<T, string?> description, Func<T, int?> stepOrder,
        bool requireBoundsTogether)
    {
        validator.RuleFor(x => tMin(x)).InclusiveBetween(MinTemperature, MaxTemperature)
            .When(x => tMin(x).HasValue).WithName("TemperatureMin");
        validator.RuleFor(x => tMax(x)).InclusiveBetween(MinTemperature, MaxTemperature)
            .When(x => tMax(x).HasValue).WithName("TemperatureMax");
        validator.RuleFor(x => tExact(x)).InclusiveBetween(MinTemperature, MaxTemperature)
            .When(x => tExact(x).HasValue).WithName("TemperatureExact");

        validator.RuleFor(x => hMin(x)).InclusiveBetween(0, MaxHoldingTimeMinutes)
            .When(x => hMin(x).HasValue).WithName("HoldingTimeMin");
        validator.RuleFor(x => hMax(x)).InclusiveBetween(0, MaxHoldingTimeMinutes)
            .When(x => hMax(x).HasValue).WithName("HoldingTimeMax");
        validator.RuleFor(x => hExact(x)).InclusiveBetween(0, MaxHoldingTimeMinutes)
            .When(x => hExact(x).HasValue).WithName("HoldingTimeExact");

        validator.RuleFor(x => x)
            .Must(x => !(tMin(x) > tMax(x)))
            .WithMessage("TemperatureMax must be greater than or equal to TemperatureMin");
        validator.RuleFor(x => x)
            .Must(x => !(hMin(x) > hMax(x)))
            .WithMessage("HoldingTimeMax must be greater than or equal to HoldingTimeMin");

        validator.RuleFor(x => x)
            .Must(x => !tExact(x).HasValue || (!tMin(x).HasValue && !tMax(x).HasValue))
            .WithMessage("TemperatureExact cannot be combined with TemperatureMin/TemperatureMax");
        validator.RuleFor(x => x)
            .Must(x => !hExact(x).HasValue || (!hMin(x).HasValue && !hMax(x).HasValue))
            .WithMessage("HoldingTimeExact cannot be combined with HoldingTimeMin/HoldingTimeMax");

        if (requireBoundsTogether)
        {
            validator.RuleFor(x => x)
                .Must(x => tMin(x).HasValue == tMax(x).HasValue)
                .WithMessage("TemperatureMin and TemperatureMax must be specified together");
            validator.RuleFor(x => x)
                .Must(x => hMin(x).HasValue == hMax(x).HasValue)
                .WithMessage("HoldingTimeMin and HoldingTimeMax must be specified together");
        }

        validator.RuleFor(x => cooling(x)).MaximumLength(CoolingMediumMaxLength).WithName("CoolingMedium");
        validator.RuleFor(x => description(x)).MaximumLength(DescriptionMaxLength).WithName("Description");

        validator.RuleFor(x => stepOrder(x)).GreaterThanOrEqualTo(1)
            .When(x => stepOrder(x).HasValue).WithName("StepOrder");
    }
}
