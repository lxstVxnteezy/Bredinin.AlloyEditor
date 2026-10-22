using FluentValidation;

namespace Bredinin.AlloyEditor.Core.Validation.Validators.MechanicalProperty;

internal static class MechanicalPropertyRules
{
    // numeric(18,4) в БД
    public const decimal MaxAbsValue = 99_999_999_999_999m;
    public const int ConditionMaxLength = 500;
    public const int SourceMaxLength = 500;
    public const int NotesMaxLength = 1000;

    public static void AddCommonRules<T>(
        AbstractValidator<T> validator,
        Func<T, decimal?> min, Func<T, decimal?> max, Func<T, decimal?> exact,
        Func<T, string?> condition, Func<T, string?> source, Func<T, string?> notes,
        bool requireValue)
    {
        validator.RuleFor(x => min(x)).InclusiveBetween(-MaxAbsValue, MaxAbsValue)
            .When(x => min(x).HasValue).WithName("ValueMin");
        validator.RuleFor(x => max(x)).InclusiveBetween(-MaxAbsValue, MaxAbsValue)
            .When(x => max(x).HasValue).WithName("ValueMax");
        validator.RuleFor(x => exact(x)).InclusiveBetween(-MaxAbsValue, MaxAbsValue)
            .When(x => exact(x).HasValue).WithName("ValueExact");

        validator.RuleFor(x => x)
            .Must(x => !(min(x) > max(x)))
            .WithMessage("ValueMax must be greater than or equal to ValueMin");

        validator.RuleFor(x => x)
            .Must(x => !exact(x).HasValue || (!min(x).HasValue && !max(x).HasValue))
            .WithMessage("ValueExact cannot be combined with ValueMin/ValueMax");

        if (requireValue)
        {
            validator.RuleFor(x => x)
                .Must(x => min(x).HasValue == max(x).HasValue)
                .WithMessage("ValueMin and ValueMax must be specified together");

            validator.RuleFor(x => x)
                .Must(x => exact(x).HasValue || (min(x).HasValue && max(x).HasValue))
                .WithMessage("Either ValueExact or ValueMin/ValueMax must be specified");
        }

        validator.RuleFor(x => condition(x)).MaximumLength(ConditionMaxLength).WithName("Condition");
        validator.RuleFor(x => source(x)).MaximumLength(SourceMaxLength).WithName("Source");
        validator.RuleFor(x => notes(x)).MaximumLength(NotesMaxLength).WithName("Notes");
    }
}
