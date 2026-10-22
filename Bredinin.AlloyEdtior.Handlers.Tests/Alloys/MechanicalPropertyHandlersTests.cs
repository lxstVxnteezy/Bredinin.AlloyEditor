using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class MechanicalPropertyHandlersTests : HandlerTestBase
{
    private IAddMechanicalPropertyToAlloyHandler Add => Handler<IAddMechanicalPropertyToAlloyHandler>();
    private IUpdateMechanicalPropertyHandler Update => Handler<IUpdateMechanicalPropertyHandler>();

    private static CreateMechanicalPropertyRequest Req(
        Guid typeId, decimal? min = null, decimal? max = null, decimal? exact = null, Guid? heatId = null) =>
        new(typeId, heatId, min, max, exact, "при 20°C", "ГОСТ", null);

    [Fact]
    public async Task Add_RangeProperty_ShouldPersist()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType();

        var id = await Add.Handle(grade.Id, Req(type.Id, 500, 600), CancellationToken.None);

        var saved = await FreshDb().AlloyMechanicalProperties.SingleAsync(x => x.Id == id);
        saved.ValueMin.Should().Be(500);
        saved.ValueMax.Should().Be(600);
        saved.Condition.Should().Be("при 20°C");
        saved.HeatTreatmentId.Should().BeNull();
    }

    [Fact]
    public async Task Add_WithHeatTreatmentOfSameAlloy_ShouldLink()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType();
        var heat = new AlloyHeatTreatment
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, HeatTreatmentTypeId = AddHeatTreatmentType().Id };
        Db.AlloyHeatTreatments.Add(heat);
        await Db.SaveChangesAsync();

        var id = await Add.Handle(grade.Id, Req(type.Id, 1, 2, heatId: heat.Id), CancellationToken.None);

        (await FreshDb().AlloyMechanicalProperties.SingleAsync(x => x.Id == id)).HeatTreatmentId.Should().Be(heat.Id);
    }

    [Fact]
    public async Task Add_WithHeatTreatmentOfAnotherAlloy_ShouldThrow()
    {
        var system = AddSystem(AddElement("Fe", true));
        var grade = AddGrade(system, "A");
        var other = AddGrade(system, "B");
        var type = AddPropertyType();
        var heat = new AlloyHeatTreatment
            { Id = Guid.NewGuid(), AlloyGradeId = other.Id, HeatTreatmentTypeId = AddHeatTreatmentType().Id };
        Db.AlloyHeatTreatments.Add(heat);
        await Db.SaveChangesAsync();

        var act = () => Add.Handle(grade.Id, Req(type.Id, 1, 2, heatId: heat.Id), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*doesn't belong*");
    }

    [Fact]
    public async Task Add_WhenAlloyOrTypeMissing_ShouldThrow()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType();

        (await FluentActions.Awaiting(() => Add.Handle(Guid.NewGuid(), Req(type.Id, 1, 2), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()).WithMessage("Alloy grade not found*");
        (await FluentActions.Awaiting(() => Add.Handle(grade.Id, Req(Guid.NewGuid(), 1, 2), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()).WithMessage("Property type not found*");
    }

    [Fact]
    public async Task Add_ExactTypeWithoutExactValue_ShouldThrow()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType("HRC", PropertyValueType.Exact);

        var act = () => Add.Handle(grade.Id, Req(type.Id, 1, 2), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Exact value required*");
    }

    [Fact]
    public async Task Add_RangeTypeWithoutBounds_ShouldThrow()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType();

        var act = () => Add.Handle(grade.Id, Req(type.Id, exact: 5), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Min and Max values required*");
    }

    [Fact]
    public async Task Add_MinGreaterThanMax_ShouldThrow()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType();

        var act = () => Add.Handle(grade.Id, Req(type.Id, 700, 600), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Theory]
    [InlineData(-1, 10, "below the minimum")]
    [InlineData(10, 101, "above the maximum")]
    public async Task Add_ValueOutsideDictionaryLimits_ShouldThrow(decimal min, decimal max, string expected)
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddPropertyType("Удлинение", PropertyValueType.Range, minPossible: 0, maxPossible: 100);

        var act = () => Add.Handle(grade.Id, Req(type.Id, min, max), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage($"*{expected}*");
    }

    private async Task<AlloyMechanicalProperty> Seed(DictMechanicalPropertyType type, decimal? min, decimal? max, decimal? exact)
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var entity = new AlloyMechanicalProperty
        {
            Id = Guid.NewGuid(), AlloyGradeId = grade.Id, PropertyTypeId = type.Id,
            ValueMin = min, ValueMax = max, ValueExact = exact, Source = "old", Notes = "old"
        };
        Db.AlloyMechanicalProperties.Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    [Fact]
    public async Task Update_ShouldMergeProvidedFieldsWithExisting()
    {
        var entity = await Seed(AddPropertyType(), 500, 600, null);

        await Update.Handle(entity.Id, new UpdateMechanicalPropertyRequest(null, null, 650, null, "новое условие", null, null),
            CancellationToken.None);

        var saved = await FreshDb().AlloyMechanicalProperties.SingleAsync();
        saved.ValueMin.Should().Be(500);
        saved.ValueMax.Should().Be(650);
        saved.Condition.Should().Be("новое условие");
        saved.Source.Should().Be("old");
    }

    [Fact]
    public async Task Update_WhenMergedRangeInverted_ShouldThrow()
    {
        var entity = await Seed(AddPropertyType(), 500, 600, null);

        var act = () => Update.Handle(entity.Id, new UpdateMechanicalPropertyRequest(null, 700, null, null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Update_ChangeTypeToExact_RequiresExactValue()
    {
        var entity = await Seed(AddPropertyType(), 500, 600, null);
        var exactType = AddPropertyType("HRC", PropertyValueType.Exact);

        var act = () => Update.Handle(entity.Id, new UpdateMechanicalPropertyRequest(exactType.Id, null, null, null, null, null, null),
            CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Exact value required*");
    }

    [Fact]
    public async Task Update_WhenMissingOrUnknownType_ShouldThrow()
    {
        var entity = await Seed(AddPropertyType(), 1, 2, null);

        (await FluentActions.Awaiting(() => Update.Handle(Guid.NewGuid(),
                new UpdateMechanicalPropertyRequest(null, null, null, null, null, null, null), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()).WithMessage("Mechanical property not found*");
        (await FluentActions.Awaiting(() => Update.Handle(entity.Id,
                new UpdateMechanicalPropertyRequest(Guid.NewGuid(), null, null, null, null, null, null), CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()).WithMessage("Property type not found*");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContentThenNotFound()
    {
        var entity = await Seed(AddPropertyType(), 1, 2, null);
        var handler = Handler<IDeleteMechanicalPropertyHandler>();

        (await handler.Handle(entity.Id, CancellationToken.None)).Should().BeOfType<NoContentResult>();
        (await handler.Handle(entity.Id, CancellationToken.None)).Should().BeOfType<NotFoundResult>();
    }
}
