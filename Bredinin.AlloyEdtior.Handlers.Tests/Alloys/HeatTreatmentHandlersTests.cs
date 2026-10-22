using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class HeatTreatmentHandlersTests : HandlerTestBase
{
    private static CreateHeatTreatmentRequest NewRequest(Guid typeId) =>
        new(typeId, 800, 850, null, 30, 60, null, "Масло", "закалка", 1, true);

    [Fact]
    public async Task Add_ShouldPersistAllFields()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddHeatTreatmentType();

        var id = await Handler<IAddHeatTreatmentToAlloyHandler>().Handle(grade.Id, NewRequest(type.Id), CancellationToken.None);

        var saved = await FreshDb().AlloyHeatTreatments.SingleAsync(x => x.Id == id);
        saved.Should().BeEquivalentTo(new
        {
            AlloyGradeId = grade.Id, HeatTreatmentTypeId = type.Id, TemperatureMin = 800, TemperatureMax = 850,
            HoldingTimeMin = 30, HoldingTimeMax = 60, CoolingMedium = "Масло", Description = "закалка",
            StepOrder = 1, IsDefault = true
        }, o => o.ExcludingMissingMembers());
        saved.TemperatureExact.Should().BeNull();
    }

    [Fact]
    public async Task Add_WhenAlloyMissing_ShouldThrow()
    {
        var type = AddHeatTreatmentType();

        var act = () => Handler<IAddHeatTreatmentToAlloyHandler>().Handle(Guid.NewGuid(), NewRequest(type.Id), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Alloy grade not found*");
    }

    [Fact]
    public async Task Add_WhenTypeMissing_ShouldThrow()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));

        var act = () => Handler<IAddHeatTreatmentToAlloyHandler>().Handle(grade.Id, NewRequest(Guid.NewGuid()), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Heat treatment type not found*");
    }

    private async Task<AlloyHeatTreatment> SeedTreatment()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));
        var type = AddHeatTreatmentType();
        var entity = new AlloyHeatTreatment
        {
            Id = Guid.NewGuid(), AlloyGradeId = grade.Id, HeatTreatmentTypeId = type.Id,
            TemperatureMin = 800, TemperatureMax = 850, CoolingMedium = "Вода", Description = "old", StepOrder = 1
        };
        Db.AlloyHeatTreatments.Add(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    [Fact]
    public async Task Update_ShouldChangeOnlyProvidedFields()
    {
        var entity = await SeedTreatment();
        var newType = AddHeatTreatmentType("Отпуск");

        await Handler<IUpdateHeatTreatmentHandler>().Handle(entity.Id,
            new UpdateHeatTreatmentRequest(newType.Id, null, 900, null, null, null, 45, null, null, null, true),
            CancellationToken.None);

        var saved = await FreshDb().AlloyHeatTreatments.SingleAsync();
        saved.HeatTreatmentTypeId.Should().Be(newType.Id);
        saved.TemperatureMin.Should().Be(800);
        saved.TemperatureMax.Should().Be(900);
        saved.HoldingTimeExact.Should().Be(45);
        saved.CoolingMedium.Should().Be("Вода");
        saved.Description.Should().Be("old");
        saved.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WhenTreatmentMissing_ShouldThrow()
    {
        var act = () => Handler<IUpdateHeatTreatmentHandler>().Handle(Guid.NewGuid(),
            new UpdateHeatTreatmentRequest(null, null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Heat treatment not found*");
    }

    [Fact]
    public async Task Update_WhenTypeMissing_ShouldThrow()
    {
        var entity = await SeedTreatment();

        var act = () => Handler<IUpdateHeatTreatmentHandler>().Handle(entity.Id,
            new UpdateHeatTreatmentRequest(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, null),
            CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Heat treatment type not found*");
    }

    [Fact]
    public async Task Delete_ShouldRemoveTreatmentAndItsProperties()
    {
        var entity = await SeedTreatment();
        var propertyType = AddPropertyType();
        Db.AlloyMechanicalProperties.Add(new AlloyMechanicalProperty
        {
            Id = Guid.NewGuid(), AlloyGradeId = entity.AlloyGradeId, PropertyTypeId = propertyType.Id,
            HeatTreatmentId = entity.Id, ValueMin = 1, ValueMax = 2
        });
        await Db.SaveChangesAsync();

        var result = await Handler<IDeleteHeatTreatmentHandler>().Handle(entity.Id, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        var fresh = FreshDb();
        (await fresh.AlloyHeatTreatments.AnyAsync()).Should().BeFalse();
        (await fresh.AlloyMechanicalProperties.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_WhenMissing_ShouldReturnNotFound()
    {
        var result = await Handler<IDeleteHeatTreatmentHandler>().Handle(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
