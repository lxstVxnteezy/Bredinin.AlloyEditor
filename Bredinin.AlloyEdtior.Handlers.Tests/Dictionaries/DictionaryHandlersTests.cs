using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Dictionaries;

public class DictionaryHandlersTests : HandlerTestBase
{
    [Fact]
    public async Task ChemicalElements_ShouldMapAllFieldsAndUseCache()
    {
        Db.DictChemicalElements.Add(new DictChemicalElement
        {
            Id = Guid.NewGuid(), Name = "Железо", Symbol = "Fe", Description = "d", IsBaseForAlloySystem = true,
            AtomicNumber = 26, AtomicWeight = 55.845m, Group = 8, Period = 4, Density = 7.874m
        });
        await Db.SaveChangesAsync();
        var handler = Handler<IInfoDictChemicalElementsHandler>();

        var first = await handler.Handle(CancellationToken.None);
        AddElement("C");
        var second = await handler.Handle(CancellationToken.None);

        first.Should().ContainSingle().Which.Should().Match<Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements.DictChemicalElementResponse>(
            x => x.Symbol == "Fe" && x.AtomicNumber == 26 && x.IsBaseForAlloySystem && x.Density == 7.874m);
        second.Should().HaveCount(1, "второй вызов должен прийти из кэша");
    }

    [Fact]
    public async Task HeatTreatmentTypes_ShouldBeSortedByNameAndCached()
    {
        AddHeatTreatmentType("Отпуск");
        AddHeatTreatmentType("Закалка");
        var handler = Handler<IGetHeatTreatmentTypesHandler>();

        var first = await handler.Handle(CancellationToken.None);
        AddHeatTreatmentType("Нормализация");
        var second = await handler.Handle(CancellationToken.None);

        first.Select(x => x.Name).Should().Equal("Закалка", "Отпуск");
        second.Should().HaveCount(2);
    }

    [Fact]
    public async Task MechanicalPropertyTypes_ShouldBeSortedByNameAndCached()
    {
        AddPropertyType("Твёрдость");
        AddPropertyType("Предел прочности", minPossible: 0, maxPossible: 5000);
        var handler = Handler<IGetMechanicalPropertyTypesHandler>();

        var first = await handler.Handle(CancellationToken.None);
        AddPropertyType("Ударная вязкость");
        var second = await handler.Handle(CancellationToken.None);

        first.Select(x => x.Name).Should().Equal("Предел прочности", "Твёрдость");
        first[0].MaxPossible.Should().Be(5000);
        second.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAlloySystem_ShouldReturnSystemsSortedByName()
    {
        var fe = AddElement("Fe", true);
        var al = AddElement("Al", true);
        AddSystem(fe, "Железо");
        AddSystem(al, "Алюминий");

        var result = await Handler<ISearchAlloySystem>().Handle(CancellationToken.None);

        result.Select(x => x.Name).Should().Equal("Алюминий", "Железо");
        result[0].BaseElementId.Should().Be(al.Id);
    }
}
