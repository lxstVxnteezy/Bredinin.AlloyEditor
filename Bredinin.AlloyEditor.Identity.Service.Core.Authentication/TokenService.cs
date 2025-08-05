using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication;

public class TokenService(IdentityDbContext context, JwtProvider jwtProvider) : ITokenService
{
    private const string RefreshTokenIdClaim = "refreshTokenId";
    private const string UserIdClaim = "userId";
    private const string UserLoginClaim = "userLogin";

    public async Task<AuthResponse> GeneratePairsTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        if (!Guid.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == RefreshTokenIdClaim)?.Value, out var refreshTokenId))
            throw new InvalidOperationException("refreshTokenId not found in access token");

        await context.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = refreshTokenId,
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(jwtProvider.JwtConfiguration.RefreshTokenExpiryDays),
            UserId = user.Id
        });

        await context.SaveChangesAsync();
        return new AuthResponse(accessToken, refreshToken);
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId)
    {
        var handler = new JwtSecurityTokenHandler();
        var validationResult = await handler.ValidateTokenAsync(refreshToken, GetTokenValidationParameters());

        if (!validationResult.IsValid) return false;

        var token = await context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        return token != null && !token.IsUsed && !token.IsRevoked && token.Expires > DateTime.UtcNow;
    }

    public Task RevokeRefreshTokenAsync(string refreshToken) =>
        UpdateRefreshTokenAsync(rt => rt.Token == refreshToken, rt => rt.IsRevoked = true);

    public Task UseRefreshTokenAsync(string refreshToken) =>
        UpdateRefreshTokenAsync(rt => rt.Token == refreshToken, rt => rt.IsUsed = true);

    public async Task RevokeRefreshAllTokenUserAsync(Guid userId) =>
        await context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true));

    private string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiration) =>
        new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: jwtProvider.JwtConfiguration.Issuer,
            audience: jwtProvider.JwtConfiguration.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: GetSigningCredentials()));

    private SigningCredentials GetSigningCredentials() => new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtProvider.JwtConfiguration.Key)),
        SecurityAlgorithms.HmacSha256);

    private IEnumerable<Claim> CreateBaseClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(UserIdClaim, user.Id.ToString()),
            new(UserLoginClaim, user.Login),
            new(RefreshTokenIdClaim, Guid.NewGuid().ToString())
        };
        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));
        return claims;
    }

    private TokenValidationParameters GetTokenValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtProvider.JwtConfiguration.Key)),
        ValidateIssuer = false,
        ValidIssuer = jwtProvider.JwtConfiguration.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtProvider.JwtConfiguration.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    private async Task UpdateRefreshTokenAsync(Expression<Func<RefreshToken, bool>> predicate, Action<RefreshToken> updateAction)
    {
        var token = await context.RefreshTokens.FirstOrDefaultAsync(predicate);
        if (token != null)
        {
            updateAction(token);
            await context.SaveChangesAsync();
        }
    }
    private string GenerateAccessToken(User user) =>
        GenerateToken(CreateBaseClaims(user), TimeSpan.FromMinutes(jwtProvider.JwtConfiguration.AccessTokenExpiryMinutes));

    private string GenerateRefreshToken()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, jwtProvider.JwtConfiguration.Issuer),
            new Claim(JwtRegisteredClaimNames.Aud, jwtProvider.JwtConfiguration.Audience),
            new Claim(JwtRegisteredClaimNames.Exp,
                new DateTimeOffset(DateTime.UtcNow.AddDays(jwtProvider.JwtConfiguration.RefreshTokenExpiryDays))
                    .ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        return GenerateToken(claims, TimeSpan.FromDays(jwtProvider.JwtConfiguration.RefreshTokenExpiryDays));
    }
}