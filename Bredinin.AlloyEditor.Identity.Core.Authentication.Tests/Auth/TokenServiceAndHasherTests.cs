using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Services.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Bredinin.AlloyEditor.Identity.Tests.Auth;

public class TokenServiceTests
{
    private static readonly JwtConfiguration Config = new()
    {
        Key = "test-key-at-least-32-characters-long!!", Issuer = "issuer", Audience = "audience",
        AccessTokenExpiryMinutes = 15, RefreshTokenExpiryDays = 7
    };

    private readonly TokenService _sut = new(new JwtOptionsAccessor(Options.Create(Config)), Mock.Of<IDistributedCache>());

    private static User UserWithRoles(params string[] roles)
    {
        var user = new User
        {
            Id = Guid.NewGuid(), Login = "ivan", FirstName = "a", LastName = "b", SecondName = "c", Hash = "h"
        };
        foreach (var name in roles)
            user.UserRoles.Add(new UserRole { Role = new Role { Id = Guid.NewGuid(), Name = name, Description = name } });
        return user;
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserClaimsRolesAndIat()
    {
        var user = UserWithRoles("Admin", "User");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(user));

        token.Issuer.Should().Be("issuer");
        token.Audiences.Should().Equal("audience");
        token.Claims.Single(c => c.Type == "userId").Value.Should().Be(user.Id.ToString());
        token.Claims.Single(c => c.Type == "userLogin").Value.Should().Be("ivan");
        token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo("Admin", "User");
        long.Parse(token.Claims.Single(c => c.Type == "iat").Value)
            .Should().BeCloseTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 5);
    }

    [Fact]
    public void GenerateAccessToken_ShouldExpireAfterConfiguredMinutes()
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(UserWithRoles()));

        token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateAccessToken_ShouldBeValidForConfiguredKeyAndRejectedForAnother()
    {
        var jwt = _sut.GenerateAccessToken(UserWithRoles("Admin"));
        TokenValidationParameters Parameters(string key) => new()
        {
            ValidIssuer = "issuer", ValidAudience = "audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
        var handler = new JwtSecurityTokenHandler();

        handler.ValidateToken(jwt, Parameters(Config.Key), out _).Identity!.IsAuthenticated.Should().BeTrue();
        var act = () => handler.ValidateToken(jwt, Parameters("another-key-at-least-32-characters-long!!"), out _);
        act.Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldBeRandomBase64Of64Bytes()
    {
        var tokens = Enumerable.Range(0, 20).Select(_ => _sut.GenerateRefreshToken()).ToArray();

        tokens.Distinct().Should().HaveCount(20);
        Convert.FromBase64String(tokens[0]).Should().HaveCount(64);
    }

    [Fact]
    public void CreateAuthResponse_ShouldWrapTokens()
    {
        var response = _sut.CreateAuthResponse("a", "r");

        response.AccessToken.Should().Be("a");
        response.RefreshToken.Should().Be("r");
    }
}

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Generate_ShouldNotReturnPlainPasswordAndShouldVerify()
    {
        var hash = _sut.Generate("Passw0rdX");

        hash.Should().NotContain("Passw0rdX");
        _sut.VerifyPassword("Passw0rdX", hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldBeFalse() =>
        _sut.VerifyPassword("other", _sut.Generate("Passw0rdX")).Should().BeFalse();

    [Fact]
    public void Generate_ShouldBeSaltedSoHashesDiffer() =>
        _sut.Generate("Passw0rdX").Should().NotBe(_sut.Generate("Passw0rdX"));
}

public class JwtOptionsAccessorTests
{
    private static JwtOptionsAccessor Create(string? key = "k", string? issuer = "i", string? audience = "a") =>
        new(Options.Create(new JwtConfiguration { Key = key!, Issuer = issuer!, Audience = audience! }));

    [Fact]
    public void ValidConfiguration_ShouldBeExposed() => Create().Value.Issuer.Should().Be("i");

    [Theory]
    [InlineData(null, "i", "a")]
    [InlineData("k", "", "a")]
    [InlineData("k", "i", "  ")]
    public void MissingValue_ShouldThrow(string? key, string? issuer, string? audience)
    {
        var act = () => Create(key, issuer, audience);

        act.Should().Throw<ArgumentException>();
    }
}
