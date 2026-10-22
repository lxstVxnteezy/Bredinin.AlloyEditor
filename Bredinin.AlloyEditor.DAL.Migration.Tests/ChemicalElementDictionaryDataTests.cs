using Bredinin.AlloyEditor.DAL.Migration.Parsers;
using FluentAssertions;

namespace Bredinin.AlloyEditor.DAL.Migration.Tests;

public class ChemicalElementDictionaryDataTests
{
    private static readonly Lazy<List<Bredinin.AlloyEditor.Domain.Dictionaries.DictChemicalElement>> Elements =
        new(ChemicalElementExcelParser.ParseElements);

    [Fact]
    public void ShouldContainTheElementsUsedInAlloys() =>
        Elements.Value.Count.Should().BeGreaterThanOrEqualTo(40);

    [Fact]
    public void SymbolsNamesAndAtomicNumbers_ShouldBeUnique()
    {
        Elements.Value.Select(x => x.Symbol).Should().OnlyHaveUniqueItems();
        Elements.Value.Select(x => x.Name).Should().OnlyHaveUniqueItems();
        Elements.Value.Select(x => x.AtomicNumber).Should().OnlyHaveUniqueItems();
        Elements.Value.Select(x => x.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void RequiredTextFields_ShouldBeFilled() =>
        Elements.Value.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Symbol) && !string.IsNullOrWhiteSpace(x.Name));

    [Fact]
    public void SymbolLength_ShouldFitDatabaseColumn() =>
        Elements.Value.Should().OnlyContain(x => x.Symbol.Length <= 10, "колонка symbol — varchar(10)");

    [Fact]
    public void NumericFields_ShouldBeInPhysicalRanges()
    {
        Elements.Value.Should().OnlyContain(x => x.AtomicNumber >= 1 && x.AtomicNumber <= 130);
        Elements.Value.Should().OnlyContain(x => x.Period >= 1 && x.Period <= 9);
        Elements.Value.Should().OnlyContain(x => x.Group >= 0 && x.Group <= 18);
        Elements.Value.Should().OnlyContain(x => x.AtomicWeight > 0 && x.AtomicWeight < 400,
            "атомная масса задана в а.е.м., а не умножена на 1000 из-за культуры парсинга");
        Elements.Value.Should().OnlyContain(x => x.Density >= 0 && x.Density < 30);
    }

    [Theory]
    [InlineData("Fe", 26, 55.845, 7.87)]
    [InlineData("Al", 13, 26.982, 2.70)]
    [InlineData("Cu", 29, 63.546, 8.96)]
    public void WellKnownElements_ShouldHaveCorrectProperties(string symbol, int number, double weight, double density)
    {
        var element = Elements.Value.Single(x => x.Symbol == symbol);

        element.AtomicNumber.Should().Be(number);
        element.AtomicWeight.Should().BeApproximately((decimal)weight, 0.01m);
        element.Density.Should().BeApproximately((decimal)density, 0.05m);
    }

    [Fact]
    public void BaseElementsForAlloySystems_ShouldIncludeCommonMetals()
    {
        var baseSymbols = Elements.Value.Where(x => x.IsBaseForAlloySystem).Select(x => x.Symbol).ToArray();

        baseSymbols.Should().Contain(["Fe", "Al", "Cu", "Ti", "Ni"]);
        baseSymbols.Should().NotContain(["O", "H", "He"]);
    }
}
