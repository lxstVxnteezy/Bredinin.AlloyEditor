using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class CreateAlloyGradeHandlerTests : HandlerTestBase
{
    private ICreateAlloyGradeHandler Sut => Handler<ICreateAlloyGradeHandler>();

    private static CreateAlloyGradeRequest Request(
        Guid systemId, string name, params CreateChemicalCompositionRequest[] compositions) =>
        new(name, "описание", systemId, compositions, null, null);

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistGradeWithCompositions()
    {
        var iron = AddElement("Fe", isBase: true);
        var carbon = AddElement("C");
        var system = AddSystem(iron);

        var id = await Sut.Handle(Request(system.Id, "  Сталь 45  ",
            new CreateChemicalCompositionRequest(null, null, 98, iron.Id),
            new CreateChemicalCompositionRequest(0.42m, 0.5m, null, carbon.Id)), CancellationToken.None);

        var saved = await FreshDb().AlloyGrades.Include(x => x.ChemicalCompositions).SingleAsync(x => x.Id == id);
        saved.Name.Should().Be("Сталь 45");
        saved.AlloySystemId.Should().Be(system.Id);
        saved.ChemicalCompositions.Should().HaveCount(2);
        saved.ChemicalCompositions.Single(x => x.ChemicalElementId == carbon.Id)
            .Should().Match<AlloyEditor.Domain.Alloys.AlloyChemicalCompositions>(
                x => x.MinValue == 0.42m && x.MaxValue == 0.5m && x.ExactValue == null);
    }

    [Fact]
    public async Task Handle_WhenSystemNotFound_ShouldThrowBusinessException()
    {
        var act = () => Sut.Handle(Request(Guid.NewGuid(), "A"), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Alloy system not found*");
    }

    [Fact]
    public async Task Handle_WhenBaseElementMissing_ShouldThrowBusinessException()
    {
        var iron = AddElement("Fe", isBase: true);
        var carbon = AddElement("C");
        var system = AddSystem(iron);

        var act = () => Sut.Handle(Request(system.Id, "A",
            new CreateChemicalCompositionRequest(null, null, 1, carbon.Id)), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Base element*");
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowBusinessException()
    {
        var iron = AddElement("Fe", isBase: true);
        var system = AddSystem(iron);
        AddGrade(system, "Duplicate");

        var act = () => Sut.Handle(Request(system.Id, "Duplicate",
            new CreateChemicalCompositionRequest(null, null, 99, iron.Id)), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*already exists");
        (await FreshDb().AlloyGrades.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenElementDoesNotExist_ShouldThrowBusinessException()
    {
        var iron = AddElement("Fe", isBase: true);
        var system = AddSystem(iron);
        var unknown = Guid.NewGuid();

        var act = () => Sut.Handle(Request(system.Id, "A",
            new CreateChemicalCompositionRequest(null, null, 90, iron.Id),
            new CreateChemicalCompositionRequest(null, null, 1, unknown)), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage($"*{unknown}*");
        (await FreshDb().AlloyGrades.CountAsync()).Should().Be(0);
    }
}
