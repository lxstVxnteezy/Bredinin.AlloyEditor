namespace Bredinin.AlloyEditor.Core.Validation.Extensions;

public static class ChemicalCompositionValidationExtensions
{
    // Базовый элемент (например, Al в сплаве 99.99% Al) может занимать до 100%
    private const decimal MaxElementValue = 100;
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
    /// Проверяет, что ExactValue не указан одновременно с Min/Max
    /// </summary>
    public static bool IsExactNotMixedWithRange(decimal? exactValue, decimal? minValue, decimal? maxValue) =>
        !exactValue.HasValue || (!minValue.HasValue && !maxValue.HasValue);

    /// <summary>
    /// Проверяет, что сумма нижних границ (Min, а для точных значений — Exact) не превышает 100%.
    /// Сумма верхних границ не ограничивается: максимумы элементов в марке не достигаются одновременно.
    /// </summary>
    public static bool IsTotalRangeValid<T>(IEnumerable<T> compositions,
        Func<T, decimal?> minSelector,
        Func<T, decimal?> maxSelector,
        Func<T, decimal?> exactSelector) where T : class
    {
        var totalMin = compositions.Sum(x => minSelector(x) ?? exactSelector(x) ?? 0);

        return totalMin <= MaxPercent;
    }

    /// <summary>
    /// Проверяет, что ExactValue в допустимом диапазоне
    /// </summary>
    public static bool IsValidExactValue(decimal? exactValue) =>
        !exactValue.HasValue || (exactValue.Value >= 0 && exactValue.Value <= MaxElementValue);

    /// <summary>
    /// Проверяет, что в составе нет повторяющихся элементов
    /// </summary>
    public static bool HasUniqueElements<T>(IEnumerable<T> compositions, Func<T, Guid> elementSelector) =>
        compositions.Select(elementSelector).Distinct().Count() == compositions.Count();

    /// <summary>
    /// Получает максимально допустимое значение для элемента
    /// </summary>
    public static decimal GetMaxElementValue() => MaxElementValue;

    /// <summary>
    /// Получает максимальный процент для суммы
    /// </summary>
    public static decimal GetMaxPercent() => MaxPercent;
}
