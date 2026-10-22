using System.Security.Claims;
using Bredinin.AlloyEditor.Common.Http.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Bredinin.AlloyEditor.Common.Http.Tests;

public class TokenRevocationMiddlewareTests
{
    private readonly IDistributedCache _cache =
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private static DefaultHttpContext ContextFor(Guid? userId, long? iat, bool authenticated = true)
    {
        var claims = new List<Claim>();
        if (userId != null) claims.Add(new Claim("userId", userId.ToString()!));
        if (iat != null) claims.Add(new Claim("iat", iat.ToString()!));

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(authenticated ? new ClaimsIdentity(claims, "jwt") : new ClaimsIdentity(claims))
        };
    }

    private async Task<bool> NextWasCalled(DefaultHttpContext context)
    {
        var called = false;
        await new TokenRevocationMiddleware(_ => { called = true; return Task.CompletedTask; }, _cache).InvokeAsync(context);
        return called;
    }

    [Fact]
    public async Task Anonymous_ShouldPassThrough()
    {
        var context = ContextFor(Guid.NewGuid(), 100, authenticated: false);

        (await NextWasCalled(context)).Should().BeTrue();
    }

    [Fact]
    public async Task WhenUserWasNotRevoked_ShouldPassThrough()
    {
        var context = ContextFor(Guid.NewGuid(), 100);

        (await NextWasCalled(context)).Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task TokenIssuedBeforeRevocation_ShouldBeRejectedWith401()
    {
        var userId = Guid.NewGuid();
        await _cache.SetStringAsync($"revoked:{userId}", "200");
        var context = ContextFor(userId, 199);

        (await NextWasCalled(context)).Should().BeFalse();
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task TokenIssuedAfterRevocation_ShouldPassThrough()
    {
        var userId = Guid.NewGuid();
        await _cache.SetStringAsync($"revoked:{userId}", "200");

        (await NextWasCalled(ContextFor(userId, 201))).Should().BeTrue();
    }

    [Fact]
    public async Task RevocationOfAnotherUser_ShouldNotAffectCurrentUser()
    {
        await _cache.SetStringAsync($"revoked:{Guid.NewGuid()}", "999999");

        (await NextWasCalled(ContextFor(Guid.NewGuid(), 100))).Should().BeTrue();
    }

    [Fact]
    public async Task TokenWithoutUserIdOrIat_ShouldPassThroughToAuthorization()
    {
        (await NextWasCalled(ContextFor(null, 100))).Should().BeTrue();
        (await NextWasCalled(ContextFor(Guid.NewGuid(), null))).Should().BeTrue();
    }
}
