using Bredinin.AlloyEditor.Contracts.Common;
using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Gateway.Controllers;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.Controllers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Bredinin.AlloyEditor.Controllers.Tests;

public class IdentityServiceControllersTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task Login_ShouldSendQueryAndReturnTokens()
    {
        var query = new GetJwtTokenQuery { Login = "a", Password = "b" };
        var tokens = new AuthResponse("access", "refresh");
        _mediator.Setup(m => m.Send(query, default)).ReturnsAsync(tokens);

        (await new IdentityController(_mediator.Object).Login(query)).Should().BeSameAs(tokens);
    }

    [Fact]
    public async Task Refresh_ShouldSendQuery()
    {
        var query = new RefreshTokenQuery("r");
        var tokens = new AuthResponse("access", "refresh2");
        _mediator.Setup(m => m.Send(query, default)).ReturnsAsync(tokens);

        (await new IdentityController(_mediator.Object).Refresh(query)).Should().BeSameAs(tokens);
    }

    [Fact]
    public async Task Revoke_ShouldSendRevokeQueryForUser()
    {
        var userId = Guid.NewGuid();

        var result = await new IdentityController(_mediator.Object).RevokeToken(userId);

        result.Should().BeOfType<OkResult>();
        _mediator.Verify(m => m.Send(It.Is<RevokeTokenQuery>(q => q.UserId == userId), default), Times.Once);
    }

    [Fact]
    public async Task AdminController_ShouldDelegateEveryOperationToMediator()
    {
        var sut = new AdminController(_mediator.Object);
        var create = new CreateUserCommand { Login = "l", Password = "p", FirstName = "f", LastName = "l", SecondName = "s" };
        var edit = new EditUserCommand { FirstName = "f", LastName = "l", SecondName = "s" };
        var reset = new ResetPasswordUserCommand { UserId = Guid.NewGuid(), Password = "p" };
        var userId = Guid.NewGuid();
        var created = Guid.NewGuid();
        var updated = new UpdateUserResponse("l", "f", "l", "s", []);
        SearchUserQuery[] users = [];
        _mediator.Setup(m => m.Send(create, default)).ReturnsAsync(created);
        _mediator.Setup(m => m.Send(edit, default)).ReturnsAsync(updated);
        _mediator.Setup(m => m.Send(It.IsAny<GetAllSearchUserQueries>(), default)).ReturnsAsync(users);

        (await sut.CreateUser(create)).Should().Be(created);
        (await sut.UpdateUser(edit)).Should().BeSameAs(updated);
        (await sut.DeleteUser(userId)).Should().BeOfType<NoContentResult>();
        (await sut.ResetPassword(reset)).Should().BeOfType<OkResult>();
        (await sut.GetAllSearchQueries()).Should().BeSameAs(users);

        _mediator.Verify(m => m.Send(It.Is<DeleteUserCommand>(c => c.UserId == userId), default), Times.Once);
        _mediator.Verify(m => m.Send(reset, default), Times.Once);
    }
}

public class GatewayControllersTests
{
    private static readonly CancellationToken Ct = new CancellationTokenSource().Token;

    [Fact]
    public async Task AlloyGrade_GetGrades_ShouldPassPagingToDownstreamClient()
    {
        var client = new Mock<IAlloyClient>();
        var page = new PagedResponse<AlloyGradeResponse>([], 0, 3, 7, 0);
        client.Setup(c => c.GetGrades("q", 3, 7, Ct)).ReturnsAsync(page);

        var result = await new AlloyGradeGatewayController(client.Object).GetGrades("q", 3, 7, Ct);

        result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(page);
    }

    [Fact]
    public async Task AlloyGrade_CreateDeleteAndByMainElement_ShouldDelegate()
    {
        var client = new Mock<IAlloyClient>();
        var id = Guid.NewGuid();
        var request = new CreateAlloyGradeRequest("A", null, Guid.NewGuid(), [], null, null);
        InfoAlloyGradeByMainResponse[] byMain = [];
        client.Setup(c => c.CreateAlloyGrade(request, Ct)).ReturnsAsync(id);
        client.Setup(c => c.GetAlloysForMainElement(id, Ct)).ReturnsAsync(byMain);
        var sut = new AlloyGradeGatewayController(client.Object);

        (await sut.CreateAlloyGrade(request, Ct)).Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(id);
        (await sut.DeleteAlloyGrade(id, Ct)).Should().BeOfType<NoContentResult>();
        (await sut.GetAlloysForMainElement(id, Ct)).Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(byMain);
        client.Verify(c => c.DeleteAlloyGrade(id, Ct), Times.Once);
    }

    [Fact]
    public async Task AlloySystemAndDictionary_ShouldWrapClientResponseInOk()
    {
        var systems = new Mock<IAlloySystemClient>();
        var dictionary = new Mock<IDictionaryClient>();
        SearchAlloySystemResponse[] systemsResponse = [];
        DictChemicalElementResponse[] elements = [];
        systems.Setup(c => c.GetAlloySystem(Ct)).ReturnsAsync(systemsResponse);
        dictionary.Setup(c => c.GetChemicalElements(Ct)).ReturnsAsync(elements);

        (await new AlloySystemGatewayController(systems.Object).GetAlloySystem(Ct))
            .Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(systemsResponse);
        (await new DictionaryGatewayController(dictionary.Object).GetChemicalElements(Ct))
            .Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(elements);
    }

    [Fact]
    public async Task IdentityGateway_ShouldForwardLoginRefreshAndRevoke()
    {
        var client = new Mock<IAuthClient>();
        var login = new GetJwtTokenQuery { Login = "a", Password = "b" };
        var refresh = new RefreshTokenQuery("r");
        var tokens = new AuthResponse("a", "r");
        var userId = Guid.NewGuid();
        client.Setup(c => c.Login(login, Ct)).ReturnsAsync(tokens);
        client.Setup(c => c.RefreshToken(refresh, Ct)).ReturnsAsync(tokens);
        var sut = new IdentityGatewayController(client.Object);

        (await sut.Login(login, Ct)).Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(tokens);
        (await sut.RefreshToken(refresh, Ct)).Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(tokens);
        (await sut.RevokeToken(userId, Ct)).Should().BeOfType<OkResult>();
        client.Verify(c => c.RevokeToken(userId, It.Is<RevokeTokenQuery>(q => q.UserId == userId), Ct), Times.Once);
    }

    [Fact]
    public async Task AdminGateway_ShouldForwardEveryOperation()
    {
        var client = new Mock<IAdminClient>();
        var create = new CreateUserCommand { Login = "l", Password = "p", FirstName = "f", LastName = "l", SecondName = "s" };
        var edit = new EditUserCommand { FirstName = "f", LastName = "l", SecondName = "s" };
        var reset = new ResetPasswordUserCommand { UserId = Guid.NewGuid(), Password = "p" };
        var id = Guid.NewGuid();
        var updated = new UpdateUserResponse("l", "f", "l", "s", []);
        SearchUserQuery[] users = [];
        client.Setup(c => c.CreateUser(create)).ReturnsAsync(id);
        client.Setup(c => c.UpdateUser(edit)).ReturnsAsync(updated);
        client.Setup(c => c.GetAllSearchQueries()).ReturnsAsync(users);
        client.Setup(c => c.Test()).ReturnsAsync((object)"ok");
        var sut = new AdminGatewayController(client.Object);

        (await sut.CreateUser(create)).Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(id);
        (await sut.UpdateUser(edit)).Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(updated);
        (await sut.DeleteUser(id)).Should().BeOfType<NoContentResult>();
        (await sut.ResetPassword(reset)).Should().BeOfType<OkResult>();
        (await sut.GetAllSearchQueries()).Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeSameAs(users);
        (await sut.Test()).Should().BeOfType<OkObjectResult>();
        client.Verify(c => c.DeleteUser(id), Times.Once);
        client.Verify(c => c.ResetPassword(reset), Times.Once);
    }
}
