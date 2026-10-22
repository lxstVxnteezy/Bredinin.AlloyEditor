using Bredinin.AlloyEditor.Core.Validation.Extensions;
using FluentAssertions;

namespace Bredinin.AlloyEditor.Core.Validation.Tests;

public class ChemicalCompositionRulesTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(0.0, true)]
    [InlineData(99.99, true)]
    [InlineData(100.0, true)]
    [InlineData(-0.01, false)]
    [InlineData(100.01, false)]
    public void IsValidValueRange(double? value, bool expected) =>
        ChemicalCompositionValidationExtensions.IsValidValueRange((decimal?)value).Should().Be(expected);

    [Theory]
    [InlineData(null, true)]
    [InlineData(50.0, true)]
    [InlineData(-1.0, false)]
    [InlineData(101.0, false)]
    public void IsValidExactValue(double? value, bool expected) =>
        ChemicalCompositionValidationExtensions.IsValidExactValue((decimal?)value).Should().Be(expected);

    [Theory]
    [InlineData(1.0, 2.0, true)]
    [InlineData(2.0, 2.0, true)]
    [InlineData(3.0, 2.0, false)]
    [InlineData(null, 2.0, true)]
    [InlineData(1.0, null, true)]
    [InlineData(null, null, true)]
    public void IsValidMinMaxRange(double? min, double? max, bool expected) =>
        ChemicalCompositionValidationExtensions.IsValidMinMaxRange((decimal?)min, (decimal?)max).Should().Be(expected);

    [Theory]
    [InlineData(1.0, 2.0, true)]
    [InlineData(2.0, 2.0, false)]
    [InlineData(null, 2.0, true)]
    public void AreMinAndMaxDifferent(double? min, double? max, bool expected) =>
        ChemicalCompositionValidationExtensions.AreMinAndMaxDifferent((decimal?)min, (decimal?)max).Should().Be(expected);

    [Theory]
    [InlineData(5.0, null, null, true)]
    [InlineData(null, 1.0, 2.0, true)]
    [InlineData(null, 1.0, null, false)]
    [InlineData(null, null, 2.0, false)]
    [InlineData(null, null, null, false)]
    public void IsEitherExactOrRange(double? exact, double? min, double? max, bool expected) =>
        ChemicalCompositionValidationExtensions.IsEitherExactOrRange((decimal?)exact, (decimal?)min, (decimal?)max).Should().Be(expected);

    [Theory]
    [InlineData(5.0, null, null, true)]
    [InlineData(null, 1.0, 2.0, true)]
    [InlineData(5.0, 1.0, null, false)]
    [InlineData(5.0, null, 2.0, false)]
    public void IsExactNotMixedWithRange(double? exact, double? min, double? max, bool expected) =>
        ChemicalCompositionValidationExtensions.IsExactNotMixedWithRange((decimal?)exact, (decimal?)min, (decimal?)max).Should().Be(expected);

    private record Item(decimal? Min, decimal? Max, decimal? Exact);

    private static bool Total(params Item[] items) =>
        ChemicalCompositionValidationExtensions.IsTotalRangeValid(items, x => x.Min, x => x.Max, x => x.Exact);

    [Fact]
    public void IsTotalRangeValid_ShouldSumMinimumsAndExactValues()
    {
        Total(new Item(null, null, 60), new Item(null, null, 40)).Should().BeTrue();
        Total(new Item(null, null, 60), new Item(null, null, 40.01m)).Should().BeFalse();
        Total(new Item(70, 80, null), new Item(31, 32, null)).Should().BeFalse();
    }

    [Fact]
    public void IsTotalRangeValid_ShouldNotLimitSumOfMaximums()
    {
        // Fe 95-99, C 0.5-1, Cr 1-5: максимумы в сумме > 100, но это допустимая марка
        Total(new Item(95, 99, null), new Item(0.5m, 1, null), new Item(1, 5, null)).Should().BeTrue();
    }

    [Fact]
    public void IsTotalRangeValid_ForEmptyCollection_ShouldBeTrue() => Total().Should().BeTrue();

    [Fact]
    public void HasUniqueElements_ShouldDetectDuplicates()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        ChemicalCompositionValidationExtensions.HasUniqueElements(new[] { a, b }, x => x).Should().BeTrue();
        ChemicalCompositionValidationExtensions.HasUniqueElements(new[] { a, b, a }, x => x).Should().BeFalse();
        ChemicalCompositionValidationExtensions.HasUniqueElements(Array.Empty<Guid>(), x => x).Should().BeTrue();
    }

    [Fact]
    public void Limits_ShouldBe100Percent()
    {
        ChemicalCompositionValidationExtensions.GetMaxElementValue().Should().Be(100);
        ChemicalCompositionValidationExtensions.GetMaxPercent().Should().Be(100);
    }
}
