using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Common.Http.Middlewares;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Validation;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;
using Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;
using Bredinin.AlloyEditor.Identity.Service.Controllers;
using Bredinin.AlloyEditor.WebAPI.Controllers.Alloy;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bredinin.AlloyEditor.Controllers.Tests;

/// <summary>
/// Проверяет, что валидаторы действительно подключены в MVC-пайплайн и отдают 400 до вызова хендлера.
/// </summary>
public class ValidationPipelineTests : IAsyncLifetime
{
    private readonly Mock<ICreateAlloyGradeHandler> _create = new();
    private readonly Mock<IEditChemicalCompositionsAlloyGradeHandler> _editCompositions = new();
    private readonly Mock<IAddHeatTreatmentToAlloyHandler> _addHeat = new();
    private readonly Mock<IAddMechanicalPropertyToAlloyHandler> _addProperty = new();
    private readonly Mock<IMediator> _mediator = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AlloyController).Assembly)
            .AddApplicationPart(typeof(AdminController).Assembly);
        builder.Services.AddValidation();
        builder.Services.AddSingleton(_create.Object);
        builder.Services.AddSingleton(_editCompositions.Object);
        builder.Services.AddSingleton(_addHeat.Object);
        builder.Services.AddSingleton(_addProperty.Object);
        builder.Services.AddSingleton(_mediator.Object);

        _app = builder.Build();
        _app.UseMiddleware<ErrorHandlingMiddleware>();
        _app.MapControllers();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private static object Composition(object? exact, object? min, object? max, Guid? element = null) =>
        new { exactValue = exact, minValue = min, maxValue = max, chemicalElementId = element ?? Guid.NewGuid() };

    private static async Task<string[]> ErrorKeys(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ToArray();
    }

    [Fact]
    public async Task CreateAlloy_Valid_ShouldReachHandlerAndReturn201()
    {
        var id = Guid.NewGuid();
        _create.Setup(h => h.Handle(It.IsAny<Bredinin.AlloyEditor.Contracts.Common.AlloyGrade.CreateAlloyGradeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        var response = await _client.PostAsJsonAsync("/api/alloys", new
        {
            name = "Сталь 45", description = "d", alloySystemId = Guid.NewGuid(),
            chemicalCompositions = new[] { Composition(97, null, null), Composition(null, 0.4, 0.5) }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _create.Verify(h => h.Handle(It.IsAny<Bredinin.AlloyEditor.Contracts.Common.AlloyGrade.CreateAlloyGradeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAlloy_WithDuplicateElementsAndSumOver100_ShouldReturn400WithoutCallingHandler()
    {
        var element = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync("/api/alloys", new
        {
            name = "A", alloySystemId = Guid.NewGuid(),
            chemicalCompositions = new[] { Composition(70, null, null, element), Composition(50, null, null, element) }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ErrorKeys(response)).Should().Contain("ChemicalCompositions");
        _create.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAlloy_WithInvalidNestedItem_ShouldReportErrorOncePerRule()
    {
        // MinValue == MaxValue -> ровно одна ошибка "must be different"
        var response = await _client.PostAsJsonAsync("/api/alloys", new
        {
            name = "A", alloySystemId = Guid.NewGuid(),
            chemicalCompositions = new[] { Composition(null, 5, 5) }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        System.Text.RegularExpressions.Regex.Matches(body, "must be different").Count.Should().Be(1, body);
    }

    [Fact]
    public async Task CreateAlloy_WithTooLongName_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/alloys", new
        {
            name = new string('a', 101), alloySystemId = Guid.NewGuid(),
            chemicalCompositions = new[] { Composition(100, null, null) }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ErrorKeys(response)).Should().Contain("Name");
    }

    [Fact]
    public async Task EditCompositions_WithDuplicates_ShouldReturn400()
    {
        var element = Guid.NewGuid();

        var response = await _client.PutAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/chemical-compositions",
            new[] { Composition(10, null, null, element), Composition(20, null, null, element) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _editCompositions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EditCompositions_WithEmptyArray_ShouldReturn400()
    {
        var response = await _client.PutAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/chemical-compositions",
            Array.Empty<object>());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EditCompositions_WithInvalidItem_ShouldReturn400()
    {
        var response = await _client.PutAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/chemical-compositions",
            new[] { Composition(null, null, null) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _editCompositions.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EditCompositions_WithInvalidItem_ShouldReportItemErrorOnce()
    {
        var response = await _client.PutAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/chemical-compositions",
            new[] { Composition(null, 5, 5) });

        var body = await response.Content.ReadAsStringAsync();
        System.Text.RegularExpressions.Regex.Matches(body, "must be different").Count.Should().Be(1, body);
    }

    [Fact]
    public async Task EditCompositions_Valid_ShouldReachHandler()
    {
        _editCompositions.Setup(h => h.Handle(It.IsAny<Guid>(), It.IsAny<ChemicalCompositionsRequest[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ChemicalCompositionsDto>());

        var response = await _client.PutAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/chemical-compositions",
            new[] { Composition(90, null, null), Composition(null, 1, 2) });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddHeatTreatment_WithInvertedTemperature_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/heat-treatments", new
        {
            heatTreatmentTypeId = Guid.NewGuid(), temperatureMin = 900, temperatureMax = 800, isDefault = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _addHeat.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AddMechanicalProperty_WithoutValues_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync($"/api/alloys/{Guid.NewGuid()}/mechanical-properties",
            new { propertyTypeId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _addProperty.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task BusinessExceptionFromHandler_ShouldBeReturnedAs400Json()
    {
        _create.Setup(h => h.Handle(It.IsAny<Bredinin.AlloyEditor.Contracts.Common.AlloyGrade.CreateAlloyGradeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException("Alloy system not found"));

        var response = await _client.PostAsJsonAsync("/api/alloys", new
        {
            name = "A", alloySystemId = Guid.NewGuid(), chemicalCompositions = new[] { Composition(100, null, null) }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Alloy system not found");
    }

    [Fact]
    public async Task CreateUser_WithWeakPasswordAndBadLogin_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/user", new
        {
            login = "a b", password = "weak", firstName = "И", lastName = "П", secondName = "", age = 30, roleIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ErrorKeys(response)).Should().Contain(["Login", "Password"]);
        _mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUser_Valid_ShouldSendCommand()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(m => m.Send(It.IsAny<Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin.CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        var response = await _client.PostAsJsonAsync("/api/admin/user", new
        {
            login = "ivan.petrov", password = "Passw0rdX", firstName = "Иван", lastName = "Петров", secondName = "С",
            age = 30, roleIds = new[] { Guid.NewGuid() }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediator.Verify(m => m.Send(It.IsAny<Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin.CreateUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
