using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class GetAlloyGradeByIdHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldReturnFullCard()
    {
        var iron = AddElement("Fe", true);
        var grade = AddGrade(AddSystem(iron), "Сталь", "описание");
        var treatmentType = AddHeatTreatmentType("Отпуск");
        var propertyType = AddPropertyType("Твёрдость");
        var heat = new AlloyHeatTreatment
        {
            Id = Guid.NewGuid(), AlloyGradeId = grade.Id, HeatTreatmentTypeId = treatmentType.Id,
            TemperatureExact = 550, CoolingMedium = "Воздух", IsDefault = true
        };
        Db.AlloyHeatTreatments.Add(heat);
        Db.AlloyChemicalCompositions.Add(new AlloyChemicalCompositions
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, ChemicalElementId = iron.Id, ExactValue = 98 });
        Db.AlloyMechanicalProperties.Add(new AlloyMechanicalProperty
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, PropertyTypeId = propertyType.Id, HeatTreatmentId = heat.Id, ValueMin = 40, ValueMax = 45 });
        await Db.SaveChangesAsync();

        var result = await Handler<IGetAlloyGradeByIdHandler>().Handle(grade.Id, CancellationToken.None);

        result.Name.Should().Be("Сталь");
        result.ChemicalCompositions.Should().ContainSingle().Which.ExactValue.Should().Be(98);
        result.HeatTreatments.Should().ContainSingle().Which.Should().Match<Bredinin.AlloyEditor.Contracts.Common.HeatTreatment.HeatTreatmentDto>(
            x => x.HeatTreatmentTypeName == "Отпуск" && x.TemperatureExact == 550 && x.IsDefault);
        result.MechanicalProperties.Should().ContainSingle().Which.Should().Match<Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties.MechanicalPropertyDto>(
            x => x.PropertyTypeName == "Твёрдость" && x.ValueMin == 40 && x.HeatTreatmentId == heat.Id);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrow()
    {
        var act = () => Handler<IGetAlloyGradeByIdHandler>().Handle(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }
}

public class SearchAlloyGradeHandlerTests : HandlerTestBase
{
    private ISearchAlloyGradeHandler Sut => Handler<ISearchAlloyGradeHandler>();

    private void Seed(int count)
    {
        var system = AddSystem(AddElement("Fe", true));
        for (var i = 1; i <= count; i++)
            AddGrade(system, $"Grade{i:000}", i % 2 == 0 ? "четный сплав" : null);
    }

    [Fact]
    public async Task Handle_ShouldPageAndSortByName()
    {
        Seed(25);

        var page2 = await Sut.Handle(null, 2, 10, CancellationToken.None);

        page2.TotalCount.Should().Be(25);
        page2.TotalPages.Should().Be(3);
        page2.Page.Should().Be(2);
        page2.Items.Select(x => x.Name).Should().Equal(
            Enumerable.Range(11, 10).Select(i => $"Grade{i:000}"));
    }

    [Fact]
    public async Task Handle_ShouldSearchByNameAndDescription()
    {
        Seed(10);

        (await Sut.Handle("Grade003", 1, 20, CancellationToken.None)).Items.Should().ContainSingle();
        (await Sut.Handle("четный", 1, 20, CancellationToken.None)).TotalCount.Should().Be(5);
        (await Sut.Handle("нет такого", 1, 20, CancellationToken.None)).Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, 0, 1, 1)]
    [InlineData(1, 1000, 1, 100)]
    public async Task Handle_ShouldNormalizePagingArguments(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        Seed(3);

        var result = await Sut.Handle(null, page, pageSize, CancellationToken.None);

        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnZeroPages()
    {
        var result = await Sut.Handle(null, 1, 20, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }
}

public class GetAlloysByMainElementHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyGradesOfRequestedSystem()
    {
        var iron = AddElement("Fe", true);
        var aluminium = AddElement("Al", true);
        var ironSystem = AddSystem(iron);
        var alSystem = AddSystem(aluminium, "Aluminium");
        var steel = AddGrade(ironSystem, "Steel");
        AddGrade(alSystem, "Duralumin");
        Db.AlloyChemicalCompositions.Add(new AlloyChemicalCompositions
            { Id = Guid.NewGuid(), AlloyGradeId = steel.Id, ChemicalElementId = iron.Id, ExactValue = 97 });
        await Db.SaveChangesAsync();

        var result = await Handler<IGetAlloysByMainElementHandler>().Handle(ironSystem.Id, CancellationToken.None);

        var item = result.Should().ContainSingle().Subject;
        item.Name.Should().Be("Steel");
        item.ChemicalCompositions.Should().ContainSingle().Which.ChemicalElementId.Should().Be(iron.Id);
    }

    [Fact]
    public async Task Handle_WhenSystemMissing_ShouldThrow()
    {
        var act = () => Handler<IGetAlloysByMainElementHandler>().Handle(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }
}
