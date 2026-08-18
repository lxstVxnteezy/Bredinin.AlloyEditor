using Bredinin.AlloyEditor.DAL.Migration.Parsers;
using Xunit;
using FluentAssertions;


namespace Bredinin.AlloyEditor.DAL.Migration.Tests;

public class ChemicalElementExcelParserTests
{
    [Fact]
    public void ParseElements_ShouldReadDensityFromDensityColumn_NotFromAtomicWeightColumn()
    {
        // Arrange — ничего готовить не нужно, парсер сам читает файл из Resources
        // (this is the testability smell we noticed — no injection point)
 
        // Act
        var elements = ChemicalElementExcelParser.ParseElements();
 
        // Assert
        var iron = elements.Should().ContainSingle(e => e.Symbol == "Fe").Subject;
 
        iron.AtomicWeight.Should().Be(55.845m, "atomic weight of Iron is a well-known constant");
        iron.Density.Should().Be(7.87m, "density of Iron is a well-known constant, different from atomic weight");
 
         iron.Density.Should().NotBe(iron.AtomicWeight,
            "density and atomic weight are physically different properties");
    } 
}
