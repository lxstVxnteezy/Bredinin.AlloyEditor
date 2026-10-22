using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions;
using Bredinin.AlloyEdtior.Handlers.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEdtior.Handlers.Tests.Alloys;

public class EditChemicalCompositionsHandlerTests : HandlerTestBase
{
    private IEditChemicalCompositionsAlloyGradeHandler Sut => Handler<IEditChemicalCompositionsAlloyGradeHandler>();

    [Fact]
    public async Task Handle_ShouldReplaceWholeComposition()
    {
        var iron = AddElement("Fe", true);
        var carbon = AddElement("C");
        var chrome = AddElement("Cr");
        var grade = AddGrade(AddSystem(iron));
        Db.AlloyChemicalCompositions.Add(new AlloyChemicalCompositions
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, ChemicalElementId = iron.Id, ExactValue = 99 });
        await Db.SaveChangesAsync();

        var result = await Sut.Handle(grade.Id,
        [
            new ChemicalCompositionsRequest(null, null, 90, iron.Id),
            new ChemicalCompositionsRequest(0.3m, 0.4m, null, carbon.Id),
            new ChemicalCompositionsRequest(null, null, 2, chrome.Id)
        ], CancellationToken.None);

        result.Should().HaveCount(3);
        var saved = await FreshDb().AlloyChemicalCompositions.Where(x => x.AlloyGradeId == grade.Id).ToListAsync();
        saved.Should().HaveCount(3);
        saved.Select(x => x.ChemicalElementId).Should().BeEquivalentTo([iron.Id, carbon.Id, chrome.Id]);
        saved.Single(x => x.ChemicalElementId == iron.Id).ExactValue.Should().Be(90);
    }

    [Fact]
    public async Task Handle_WhenAlloyMissing_ShouldThrow()
    {
        var iron = AddElement("Fe", true);

        var act = () => Sut.Handle(Guid.NewGuid(), [new ChemicalCompositionsRequest(null, null, 1, iron.Id)], CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenElementMissing_ShouldThrowAndKeepOldComposition()
    {
        var iron = AddElement("Fe", true);
        var grade = AddGrade(AddSystem(iron));
        Db.AlloyChemicalCompositions.Add(new AlloyChemicalCompositions
            { Id = Guid.NewGuid(), AlloyGradeId = grade.Id, ChemicalElementId = iron.Id, ExactValue = 99 });
        await Db.SaveChangesAsync();

        var act = () => Sut.Handle(grade.Id, [new ChemicalCompositionsRequest(null, null, 1, Guid.NewGuid())], CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
        (await FreshDb().AlloyChemicalCompositions.CountAsync()).Should().Be(1);
    }
}
