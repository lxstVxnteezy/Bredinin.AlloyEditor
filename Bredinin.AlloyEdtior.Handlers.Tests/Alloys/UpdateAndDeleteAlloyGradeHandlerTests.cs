using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class UpdateAlloyGradeHandlerTests : HandlerTestBase
{
    private IUpdateAlloyGradeHandler Sut => Handler<IUpdateAlloyGradeHandler>();

    [Fact]
    public async Task Handle_WhenGradeNotFound_ShouldThrow()
    {
        var act = () => Sut.Handle(Guid.NewGuid(), new UpdateAlloyGradeRequest("x", null, null), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Handle_ShouldChangeOnlyProvidedFields()
    {
        var system = AddSystem(AddElement("Fe", true));
        var other = AddSystem(AddElement("Al", true), "Aluminium");
        var grade = AddGrade(system, "Old", "old description");

        await Sut.Handle(grade.Id, new UpdateAlloyGradeRequest(" New ", null, other.Id), CancellationToken.None);

        var saved = await FreshDb().AlloyGrades.SingleAsync();
        saved.Name.Should().Be("New");
        saved.Description.Should().Be("old description");
        saved.AlloySystemId.Should().Be(other.Id);
    }

    [Fact]
    public async Task Handle_WhenNameTakenByAnotherGrade_ShouldThrow()
    {
        var system = AddSystem(AddElement("Fe", true));
        AddGrade(system, "Taken");
        var grade = AddGrade(system, "Mine");

        var act = () => Sut.Handle(grade.Id, new UpdateAlloyGradeRequest("Taken", null, null), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*already exists");
    }

    [Fact]
    public async Task Handle_WhenNameUnchanged_ShouldNotThrow()
    {
        var system = AddSystem(AddElement("Fe", true));
        var grade = AddGrade(system, "Same");

        var act = () => Sut.Handle(grade.Id, new UpdateAlloyGradeRequest("Same", "d", null), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

public class DeleteAlloyGradeHandlerTests : HandlerTestBase
{
    private IDeleteAlloyGradeHandler Sut => Handler<IDeleteAlloyGradeHandler>();

    [Fact]
    public async Task Handle_WhenExists_ShouldDeleteAndReturnNoContent()
    {
        var grade = AddGrade(AddSystem(AddElement("Fe", true)));

        var result = await Sut.Handle(grade.Id, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        (await FreshDb().AlloyGrades.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenMissing_ShouldReturnNotFound()
    {
        var result = await Sut.Handle(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Handle_ShouldCascadeDeleteCompositionsTreatmentsAndProperties()
    {
        var iron = AddElement("Fe", true);
        var grade = AddGrade(AddSystem(iron));
        var treatment = AddHeatTreatmentType();
        var propertyType = AddPropertyType();
        var heat = new AlloyEditor.Domain.Alloys.AlloyHeatTreatment
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, HeatTreatmentTypeId = treatment.Id };
        Db.AlloyHeatTreatments.Add(heat);
        Db.AlloyChemicalCompositions.Add(new AlloyEditor.Domain.Alloys.AlloyChemicalCompositions
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, ChemicalElementId = iron.Id, ExactValue = 99 });
        Db.AlloyMechanicalProperties.Add(new AlloyEditor.Domain.Alloys.AlloyMechanicalProperty
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, PropertyTypeId = propertyType.Id, HeatTreatmentId = heat.Id, ValueMin = 1, ValueMax = 2 });
        await Db.SaveChangesAsync();

        await Sut.Handle(grade.Id, CancellationToken.None);

        var fresh = FreshDb();
        (await fresh.AlloyChemicalCompositions.AnyAsync()).Should().BeFalse();
        (await fresh.AlloyHeatTreatments.AnyAsync()).Should().BeFalse();
        (await fresh.AlloyMechanicalProperties.AnyAsync()).Should().BeFalse();
        (await fresh.DictChemicalElements.AnyAsync()).Should().BeTrue("справочники удаляться не должны");
    }
}
