using Bredinin.AlloyEditor.Contracts.Common;
using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;
using Bredinin.AlloyEditor.WebAPI.Controllers.Alloy;
using Bredinin.AlloyEditor.WebAPI.Controllers.Dictionaries;
using Bredinin.AlloyEditor.WebApi.Controllers.Alloy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Bredinin.AlloyEditor.Controllers.Tests;

public class WebApiAlloyControllerTests
{
    private readonly AlloyController _sut = new();
    private static readonly CancellationToken Ct = new CancellationTokenSource().Token;

    [Fact]
    public async Task GetGrades_ShouldPassSearchAndPagingToHandler()
    {
        var page = new PagedResponse<AlloyGradeResponse>([], 0, 2, 5, 0);
        var handler = new Mock<ISearchAlloyGradeHandler>();
        handler.Setup(h => h.Handle("сталь", 2, 5, Ct)).ReturnsAsync(page);

        var result = await _sut.GetGrades(handler.Object, "сталь", 2, 5, Ct);

        result.Should().BeSameAs(page);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk()
    {
        var response = new AlloyGradeResponse(Guid.NewGuid(), "A", null, Guid.NewGuid(), [], [], []);
        var handler = new Mock<IGetAlloyGradeByIdHandler>();
        handler.Setup(h => h.Handle(response.Id, Ct)).ReturnsAsync(response);

        var result = await _sut.GetAlloyGradeById(handler.Object, response.Id, Ct);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(response);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedWithLocationOfGetById()
    {
        var id = Guid.NewGuid();
        var request = new CreateAlloyGradeRequest("A", null, Guid.NewGuid(), [], null, null);
        var handler = new Mock<ICreateAlloyGradeHandler>();
        handler.Setup(h => h.Handle(request, Ct)).ReturnsAsync(id);

        var result = await _sut.CreateAlloyGrade(handler.Object, request, Ct);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(AlloyController.GetAlloyGradeById));
        created.RouteValues!["id"].Should().Be(id);
        created.Value.Should().Be(id);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent()
    {
        var handler = new Mock<IUpdateAlloyGradeHandler>();
        var id = Guid.NewGuid();
        var request = new UpdateAlloyGradeRequest("n", null, null);

        var result = await _sut.UpdateAlloyGrade(handler.Object, id, request, Ct);

        result.Should().BeOfType<NoContentResult>();
        handler.Verify(h => h.Handle(id, request, Ct), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnWhatHandlerReturns()
    {
        var handler = new Mock<IDeleteAlloyGradeHandler>();
        var id = Guid.NewGuid();
        handler.Setup(h => h.Handle(id, Ct)).ReturnsAsync(new NotFoundResult());

        (await _sut.DeleteAlloyGrade(handler.Object, id, Ct)).Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetForMainElement_ShouldDelegate()
    {
        InfoAlloyGradeByMainResponse[] response = [new(Guid.NewGuid(), "A", null, [])];
        var handler = new Mock<IGetAlloysByMainElementHandler>();
        var systemId = Guid.NewGuid();
        handler.Setup(h => h.Handle(systemId, Ct)).ReturnsAsync(response);

        (await _sut.GetAlloysForMainElement(handler.Object, systemId, Ct)).Should().BeSameAs(response);
    }

    [Fact]
    public async Task EditChemicalCompositions_ShouldDelegate()
    {
        var id = Guid.NewGuid();
        ChemicalCompositionsRequest[] request = [new(null, null, 1, Guid.NewGuid())];
        ChemicalCompositionsDto[] response = [new(Guid.NewGuid(), null, null, 1, request[0].ChemicalElementId)];
        var handler = new Mock<IEditChemicalCompositionsAlloyGradeHandler>();
        handler.Setup(h => h.Handle(id, request, Ct)).ReturnsAsync(response);

        (await _sut.EditChemicalCompositions(id, request, handler.Object, Ct)).Should().BeSameAs(response);
    }

    [Fact]
    public async Task AddHeatTreatment_ShouldReturnCreated()
    {
        var alloyId = Guid.NewGuid();
        var treatmentId = Guid.NewGuid();
        var request = new CreateHeatTreatmentRequest(Guid.NewGuid(), null, null, null, null, null, null, null, null, null, false);
        var handler = new Mock<IAddHeatTreatmentToAlloyHandler>();
        handler.Setup(h => h.Handle(alloyId, request, Ct)).ReturnsAsync(treatmentId);

        var result = await _sut.AddHeatTreatment(handler.Object, alloyId, request, Ct);

        result.Result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().Be(treatmentId);
    }

    [Fact]
    public async Task UpdateAndDeleteHeatTreatment_ShouldDelegate()
    {
        var id = Guid.NewGuid();
        var update = new Mock<IUpdateHeatTreatmentHandler>();
        var delete = new Mock<IDeleteHeatTreatmentHandler>();
        delete.Setup(h => h.Handle(id, Ct)).ReturnsAsync(new NoContentResult());
        var request = new UpdateHeatTreatmentRequest(null, null, null, null, null, null, null, null, null, null, null);

        (await _sut.UpdateHeatTreatment(update.Object, id, request, Ct)).Should().BeOfType<NoContentResult>();
        (await _sut.DeleteHeatTreatment(delete.Object, id, Ct)).Should().BeOfType<NoContentResult>();
        update.Verify(h => h.Handle(id, request, Ct), Times.Once);
    }

    [Fact]
    public async Task AddMechanicalProperty_ShouldReturnCreated()
    {
        var alloyId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();
        var request = new CreateMechanicalPropertyRequest(Guid.NewGuid(), null, 1, 2, null, null, null, null);
        var handler = new Mock<IAddMechanicalPropertyToAlloyHandler>();
        handler.Setup(h => h.Handle(alloyId, request, Ct)).ReturnsAsync(propertyId);

        var result = await _sut.AddMechanicalProperty(handler.Object, alloyId, request, Ct);

        result.Result.Should().BeOfType<CreatedAtActionResult>().Which.Value.Should().Be(propertyId);
    }

    [Fact]
    public async Task UpdateAndDeleteMechanicalProperty_ShouldDelegate()
    {
        var id = Guid.NewGuid();
        var update = new Mock<IUpdateMechanicalPropertyHandler>();
        var delete = new Mock<IDeleteMechanicalPropertyHandler>();
        delete.Setup(h => h.Handle(id, Ct)).ReturnsAsync(new NotFoundResult());
        var request = new UpdateMechanicalPropertyRequest(null, null, null, null, null, null, null);

        (await _sut.UpdateMechanicalProperty(update.Object, id, request, Ct)).Should().BeOfType<NoContentResult>();
        (await _sut.DeleteMechanicalProperty(delete.Object, id, Ct)).Should().BeOfType<NotFoundResult>();
    }
}

public class WebApiDictionaryControllersTests
{
    private static readonly CancellationToken Ct = new CancellationTokenSource().Token;

    [Fact]
    public async Task AlloySystemController_ShouldDelegate()
    {
        SearchAlloySystemResponse[] response = [new(Guid.NewGuid(), Guid.NewGuid(), "Железо", null)];
        var handler = new Mock<ISearchAlloySystem>();
        handler.Setup(h => h.Handle(Ct)).ReturnsAsync(response);

        (await new AlloySystemController().GetAlloySystem(handler.Object, Ct)).Should().BeSameAs(response);
    }

    [Fact]
    public async Task DictionaryController_ShouldDelegate()
    {
        var handler = new Mock<IInfoDictChemicalElementsHandler>();
        DictChemicalElementResponse[] response = [];
        handler.Setup(h => h.Handle(Ct)).ReturnsAsync(response);

        (await new DictionaryController().GetChemicalElements(handler.Object, Ct)).Should().BeSameAs(response);
    }

    [Fact]
    public async Task HeatTreatmentAndMechanicalPropertyControllers_ShouldReturnTypes()
    {
        var heat = new Mock<IGetHeatTreatmentTypesHandler>();
        var mech = new Mock<IGetMechanicalPropertyTypesHandler>();
        HeatTreatmentTypeDto[] heatTypes = [];
        MechanicalPropertyTypeDto[] mechTypes = [];
        heat.Setup(h => h.Handle(Ct)).ReturnsAsync(heatTypes);
        mech.Setup(h => h.Handle(Ct)).ReturnsAsync(mechTypes);

        (await new HeatTreatmentController().GetHeatTreatmentTypes(heat.Object, Ct)).Should().BeSameAs(heatTypes);
        (await new MechanicalPropertyController().GetMechanicalPropertyTypes(mech.Object, Ct)).Should().BeSameAs(mechTypes);
    }
}
