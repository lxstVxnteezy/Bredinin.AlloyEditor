using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Entities;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Tests.Infrastructure;
using FluentAssertions;

namespace Bredinin.AlloyEditor.Identity.Tests.Auth;

public class AuthHandlersTests : IdentityTestBase
{
    private async Task<AuthResponse> Login(string login = "ivan", string password = TestPassword) =>
        await Mediator.Send(new GetJwtTokenQuery { Login = login, Password = password });

    [Fact]
    public async Task Login_WithValidCredentials_ShouldIssueTokensAndStoreRefreshToken()
    {
        var user = AddUser("ivan", TestPassword, AddRole("Admin"));

        var response = await Login();

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        var stored = await Cache.GetStringAsync("refresh_" + response.RefreshToken);
        stored.Should().NotBeNull();
        JsonSerializer.Deserialize<RefreshTokenCacheEntry>(stored!)!.UserId.Should().Be(user.Id);
        RedisSets[$"user_tokens:{user.Id}"].Should().Contain(response.RefreshToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrowUnauthorized()
    {
        AddUser("ivan");

        var act = () => Login(password: "wrong");

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Login_WithUnknownLogin_ShouldThrowUnauthorized()
    {
        var act = () => Login("ghost");

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Login_TwiceWithSameUser_ShouldIssueDifferentRefreshTokens()
    {
        AddUser("ivan");

        var first = await Login();
        var second = await Login();

        first.RefreshToken.Should().NotBe(second.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ShouldRotateRefreshToken()
    {
        var user = AddUser("ivan");
        var login = await Login();

        var refreshed = await Mediator.Send(new RefreshTokenQuery(login.RefreshToken));

        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);
        (await Cache.GetStringAsync("refresh_" + login.RefreshToken)).Should().BeNull("старый токен одноразовый");
        (await Cache.GetStringAsync("refresh_" + refreshed.RefreshToken)).Should().NotBeNull();
        RedisSets[$"user_tokens:{user.Id}"].Should().Contain(refreshed.RefreshToken);
    }

    [Fact]
    public async Task Refresh_UsedTwice_ShouldFailSecondTime()
    {
        AddUser("ivan");
        var login = await Login();
        await Mediator.Send(new RefreshTokenQuery(login.RefreshToken));

        var act = () => Mediator.Send(new RefreshTokenQuery(login.RefreshToken));

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ShouldThrowUnauthorized()
    {
        var act = () => Mediator.Send(new RefreshTokenQuery("unknown"));

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Refresh_WithExpiredEntry_ShouldThrowUnauthorized()
    {
        var user = AddUser("ivan");
        await Cache.SetStringAsync("refresh_expired", JsonSerializer.Serialize(
            new RefreshTokenCacheEntry { UserId = user.Id, Expires = DateTime.UtcNow.AddMinutes(-1) }));

        var act = () => Mediator.Send(new RefreshTokenQuery("expired"));

        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Refresh_WhenUserWasDeleted_ShouldThrowUnauthorized()
    {
        await Cache.SetStringAsync("refresh_orphan", JsonSerializer.Serialize(
            new RefreshTokenCacheEntry { UserId = Guid.NewGuid(), Expires = DateTime.UtcNow.AddDays(1) }));

        var act = () => Mediator.Send(new RefreshTokenQuery("orphan"));

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("User not found");
    }

    [Fact]
    public async Task Revoke_ShouldDeleteAllRefreshTokensAndMarkRevocationTime()
    {
        var user = AddUser("ivan");
        var first = await Login();
        var second = await Login();
        var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await Mediator.Send(new RevokeTokenQuery(user.Id));

        (await Cache.GetStringAsync("refresh_" + first.RefreshToken)).Should().BeNull();
        (await Cache.GetStringAsync("refresh_" + second.RefreshToken)).Should().BeNull();
        RedisSets.Should().NotContainKey($"user_tokens:{user.Id}");
        var revokedAt = long.Parse((await Cache.GetStringAsync($"revoked:{user.Id}"))!);
        revokedAt.Should().BeGreaterThanOrEqualTo(before);
    }

    [Fact]
    public async Task Revoke_ShouldNotTouchTokensOfOtherUsers()
    {
        AddUser("ivan");
        var other = AddUser("petr");
        var otherLogin = await Login("petr");

        await Mediator.Send(new RevokeTokenQuery(AddUser("third").Id));

        (await Cache.GetStringAsync("refresh_" + otherLogin.RefreshToken)).Should().NotBeNull();
        (await Cache.GetStringAsync($"revoked:{other.Id}")).Should().BeNull();
    }

    [Fact]
    public async Task Revoke_WhenUserMissing_ShouldThrowBusinessException()
    {
        var act = () => Mediator.Send(new RevokeTokenQuery(Guid.NewGuid()));

        (await act.Should().ThrowAsync<BusinessException>()).WithMessage("User not found");
    }
}
