using System.Net;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Bredinin.AlloyEditor.Identity.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Tests.Admin;

public class AdminHandlersTests : IdentityTestBase
{
    private static CreateUserCommand NewUser(string login, params Guid[] roles) => new()
    {
        Login = login, Password = TestPassword, FirstName = "Иван", LastName = "Петров", SecondName = "С",
        Age = 30, RoleIds = roles
    };

    [Fact]
    public async Task CreateUser_ShouldPersistUserWithHashedPasswordAndRoles()
    {
        var admin = AddRole("Admin");
        var user = AddRole("User");

        var id = await Mediator.Send(NewUser("ivan", admin.Id, user.Id));

        var saved = await FreshDb().Users.Include(x => x.UserRoles).SingleAsync(x => x.Id == id);
        saved.Login.Should().Be("ivan");
        saved.Hash.Should().NotBe(TestPassword);
        new PasswordHasher().VerifyPassword(TestPassword, saved.Hash).Should().BeTrue();
        saved.UserRoles.Select(x => x.RoleId).Should().BeEquivalentTo([admin.Id, user.Id]);
    }

    [Fact]
    public async Task CreateUser_WithoutRoles_ShouldSucceed()
    {
        var id = await Mediator.Send(NewUser("noroles"));

        (await FreshDb().Users.AnyAsync(x => x.Id == id)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_WhenLoginExists_ShouldThrow()
    {
        AddUser("ivan");

        var act = () => Mediator.Send(NewUser("ivan"));

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*already exists");
    }

    [Fact]
    public async Task CreateUser_WhenRoleMissing_ShouldThrow()
    {
        var act = () => Mediator.Send(NewUser("ivan", Guid.NewGuid()));

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*roles don't exist*");
        (await FreshDb().Users.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task EditUser_ShouldUpdateProfileAndReplaceRoles()
    {
        var oldRole = AddRole("User");
        var newRole = AddRole("Admin");
        var user = AddUser("ivan", TestPassword, oldRole);

        var response = await Mediator.Send(new EditUserCommand
        {
            Id = user.Id, FirstName = "Пётр", LastName = "Иванов", SecondName = "А", Age = 41, RoleIds = [newRole.Id]
        });

        response.Login.Should().Be("ivan");
        response.FirstName.Should().Be("Пётр");
        response.RoleIds.Should().Equal(newRole.Id);
        var saved = await FreshDb().Users.Include(x => x.UserRoles).SingleAsync();
        saved.Age.Should().Be(41);
        saved.LastName.Should().Be("Иванов");
        saved.UserRoles.Select(x => x.RoleId).Should().Equal(newRole.Id);
    }

    [Fact]
    public async Task EditUser_ShouldNotChangeLoginOrPassword()
    {
        var user = AddUser("ivan");
        var hash = user.Hash;

        await Mediator.Send(new EditUserCommand
            { Id = user.Id, FirstName = "A", LastName = "B", SecondName = "C", Age = 20, RoleIds = [] });

        var saved = await FreshDb().Users.SingleAsync();
        saved.Login.Should().Be("ivan");
        saved.Hash.Should().Be(hash);
    }

    [Fact]
    public async Task EditUser_WhenUserMissing_ShouldThrowBusinessException()
    {
        var act = () => Mediator.Send(new EditUserCommand
            { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", SecondName = "C", Age = 20, RoleIds = [] });

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*not found*");
    }

    [Fact]
    public async Task EditUser_WhenRoleMissing_ShouldThrowBusinessExceptionNot500()
    {
        var user = AddUser("ivan");

        var act = () => Mediator.Send(new EditUserCommand
            { Id = user.Id, FirstName = "A", LastName = "B", SecondName = "C", Age = 20, RoleIds = [Guid.NewGuid()] });

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("Roles not found*");
    }

    [Fact]
    public async Task DeleteUser_ShouldRemoveUserWithRolesButKeepRoleDictionary()
    {
        var role = AddRole("User");
        var user = AddUser("ivan", TestPassword, role);
        AddUser("other");

        await Mediator.Send(new DeleteUserCommand(user.Id));

        var fresh = FreshDb();
        (await fresh.Users.Select(x => x.Login).ToListAsync()).Should().Equal("other");
        (await fresh.UserRoles.AnyAsync()).Should().BeFalse();
        (await fresh.Roles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteUser_WhenMissing_ShouldThrowNotFound()
    {
        var act = () => Mediator.Send(new DeleteUserCommand(Guid.NewGuid()));

        (await act.Should().ThrowAsync<BusinessException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReplaceHash()
    {
        var user = AddUser("ivan");

        await Mediator.Send(new ResetPasswordUserCommand { UserId = user.Id, Password = "N3wPassword" });

        var hasher = new PasswordHasher();
        var saved = await FreshDb().Users.SingleAsync();
        hasher.VerifyPassword("N3wPassword", saved.Hash).Should().BeTrue();
        hasher.VerifyPassword(TestPassword, saved.Hash).Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_WhenUserMissing_ShouldThrow()
    {
        var act = () => Mediator.Send(new ResetPasswordUserCommand { UserId = Guid.NewGuid(), Password = "N3wPassword" });

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("*not found*");
    }

    [Fact]
    public async Task SearchUsers_ShouldReturnAllUsersWithRoleIds()
    {
        var role = AddRole("Admin");
        AddUser("a", TestPassword, role);
        AddUser("b");

        var result = await Mediator.Send(new GetAllSearchUserQueries());

        result.Should().HaveCount(2);
        result.Single(x => x.Login == "a").RoleIds.Should().Equal(role.Id);
        result.Single(x => x.Login == "b").RoleIds.Should().BeEmpty();
    }
}
