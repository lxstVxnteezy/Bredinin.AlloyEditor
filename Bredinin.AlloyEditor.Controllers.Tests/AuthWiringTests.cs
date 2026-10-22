using System.Net;
using System.Net.Http.Headers;
using Bredinin.AlloyEditor.Common.Http.Middlewares;
using Bredinin.AlloyEditor.Gateway.Core.Authentication;
using Bredinin.AlloyEditor.Gateway.Core.Clients;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Services.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Bredinin.AlloyEditor.Controllers.Tests;

/// <summary>
/// Токен, выпущенный Identity-сервисом, должен приниматься защищёнными эндпоинтами,
/// а отозванный/чужой/отсутствующий — отклоняться.
/// </summary>
public class AuthWiringTests
{
    private const string Key = "test-key-at-least-32-characters-long!!";

    private static IConfiguration Config(string key = Key) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = key, ["Jwt:Issuer"] = "issuer", ["Jwt:Audience"] = "audience",
            ["Jwt:AccessTokenExpiryMinutes"] = "15", ["Jwt:RefreshTokenExpiryDays"] = "7"
        }).Build();

    private static string IssueToken(Guid userId, string key = Key)
    {
        var jwt = new JwtConfiguration
        {
            Key = key, Issuer = "issuer", Audience = "audience", AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7
        };
        var user = new User { Id = userId, Login = "ivan", FirstName = "a", LastName = "b", SecondName = "c", Hash = "h" };
        return new TokenService(new JwtOptionsAccessor(Options.Create(jwt)), Mock.Of<IDistributedCache>()).GenerateAccessToken(user);
    }

    private static async Task<(WebApplication App, HttpClient Client, IDistributedCache Cache)> Host()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuth(Config());
        builder.Services.AddDistributedMemoryCache();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseMiddleware<TokenRevocationMiddleware>();
        app.UseAuthorization();
        app.MapGet("/secure", () => "secret").RequireAuthorization();
        app.MapGet("/open", () => "open");
        await app.StartAsync();
        return (app, app.GetTestClient(), app.Services.GetRequiredService<IDistributedCache>());
    }

    private static HttpRequestMessage Get(string url, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task SecureEndpoint_WithTokenFromIdentityService_ShouldBeAvailable()
    {
        var (app, client, _) = await Host();
        await using var _ = app;

        (await client.SendAsync(Get("/secure", IssueToken(Guid.NewGuid())))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SecureEndpoint_WithoutToken_ShouldReturn401AndOpenEndpointStaysOpen()
    {
        var (app, client, _) = await Host();
        await using var _ = app;

        (await client.SendAsync(Get("/secure", null))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.SendAsync(Get("/open", null))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SecureEndpoint_WithTokenSignedByAnotherKey_ShouldReturn401()
    {
        var (app, client, _) = await Host();
        await using var _ = app;

        var forged = IssueToken(Guid.NewGuid(), "another-key-at-least-32-characters-long!!");

        (await client.SendAsync(Get("/secure", forged))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SecureEndpoint_WithGarbageToken_ShouldReturn401()
    {
        var (app, client, _) = await Host();
        await using var _ = app;

        (await client.SendAsync(Get("/secure", "not-a-jwt"))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SecureEndpoint_AfterRevocation_ShouldRejectOldTokenButAcceptNewOne()
    {
        var (app, client, cache) = await Host();
        await using var _ = app;
        var userId = Guid.NewGuid();
        var oldToken = IssueToken(userId);
        await Task.Delay(1100); // iat хранится с точностью до секунды
        await cache.SetStringAsync($"revoked:{userId}", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        await Task.Delay(1100);
        var newToken = IssueToken(userId);

        (await client.SendAsync(Get("/secure", oldToken))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.SendAsync(Get("/secure", newToken))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void AddAuth_WithoutJwtSection_ShouldThrow()
    {
        var act = () => new ServiceCollection().AddAuth(new ConfigurationBuilder().Build());

        act.Should().Throw<InvalidOperationException>().WithMessage("*JWT*");
    }

    [Fact]
    public void IdentityServiceAuthentication_ShouldRegisterHasherAndTokenService()
    {
        var services = new ServiceCollection().AddAddAuthenticationCustom(Config());

        services.Should().Contain(d => d.ServiceType == typeof(Identity.Service.Authentication.Interfaces.IPasswordHasher));
        services.Should().Contain(d => d.ServiceType == typeof(Identity.Service.Authentication.Interfaces.ITokenService));
    }

    [Fact]
    public void IdentityServiceAuthentication_WithoutJwtSection_ShouldThrow()
    {
        var act = () => new ServiceCollection().AddAddAuthenticationCustom(new ConfigurationBuilder().Build());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddClients_ShouldRegisterAllRefitClients()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AlloyEditorApi:BaseUrl"] = "http://alloy:5019", ["IdentityApi:BaseUrl"] = "http://identity:5020"
        }).Build();
        var env = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == Environments.Development);
        var services = new ServiceCollection().AddLogging();

        using var provider = services.AddClients(config, env).BuildServiceProvider();

        provider.GetRequiredService<IAlloyClient>().Should().NotBeNull();
        provider.GetRequiredService<IAlloySystemClient>().Should().NotBeNull();
        provider.GetRequiredService<IDictionaryClient>().Should().NotBeNull();
        provider.GetRequiredService<IAuthClient>().Should().NotBeNull();
        provider.GetRequiredService<IAdminClient>().Should().NotBeNull();
    }
}
