using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;

namespace Bredinin.AlloyEditor.Core.Validation.Extensions;

public static class ChemicalCompositionValidationExtensions
{
    private const decimal MaxElementValue = 99;
    private const decimal MaxPercent = 100;

    /// <summary>
    /// Проверяет, что значение находится в допустимом диапазоне
    /// </summary>
    public static bool IsValidValueRange(decimal? value) =>
        !value.HasValue || (value.Value >= 0 && value.Value <= MaxElementValue);

    /// <summary>
    /// Проверяет, что MinValue и MaxValue корректны (Min <= Max)
    /// </summary>
    public static bool IsValidMinMaxRange(decimal? minValue, decimal? maxValue) =>
        !minValue.HasValue || !maxValue.HasValue || minValue.Value <= maxValue.Value;

    /// <summary>
    /// Проверяет, что MinValue и MaxValue не равны друг другу (если оба указаны)
    /// </summary>
    public static bool AreMinAndMaxDifferent(decimal? minValue, decimal? maxValue) =>
        !minValue.HasValue || !maxValue.HasValue || minValue.Value != maxValue.Value;

    /// <summary>
    /// Проверяет, что указан либо ExactValue, либо пара Min/Max
    /// </summary>
    public static bool IsEitherExactOrRange(decimal? exactValue, decimal? minValue, decimal? maxValue) =>
        exactValue.HasValue || (minValue.HasValue && maxValue.HasValue);

    /// <summary>
    /// Проверяет, что сумма значений в допустимых пределах
    /// </summary>
    public static bool IsTotalRangeValid<T>(IEnumerable<T> compositions, 
        Func<T, decimal?> minSelector, 
        Func<T, decimal?> maxSelector) where T : class
    {
        var totalMin = compositions
            .Where(x => minSelector(x).HasValue)
            .Sum(x => minSelector(x)!.Value);

        var totalMax = compositions
            .Where(x => maxSelector(x).HasValue)
            .Sum(x => maxSelector(x)!.Value);

        return totalMin <= MaxPercent && totalMax <= MaxPercent;
    }

    /// <summary>
    /// Проверяет, что ExactValue в допустимом диапазоне
    /// </summary>
    public static bool IsValidExactValue(decimal? exactValue) =>
        !exactValue.HasValue || (exactValue.Value >= 0 && exactValue.Value <= MaxElementValue);

    /// <summary>
    /// Получает максимально допустимое значение для элемента
    /// </summary>
    public static decimal GetMaxElementValue() => MaxElementValue;

    /// <summary>
    /// Получает максимальный процент для суммы
    /// </summary>
    public static decimal GetMaxPercent() => MaxPercent;
}