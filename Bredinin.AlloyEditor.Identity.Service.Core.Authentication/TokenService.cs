using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Entities;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Bredinin.AlloyEditor.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication;
// todo убрать этк хуйню в бд, этот сервис не должен ходить в бд

public class TokenService(
    IdentityDbContext context,
    JwtOptionsAccessor jwtProvider,
    IDistributedCache cache) : ITokenService
{

    private readonly JwtConfiguration _jwtConfig = jwtProvider.Value;
    private const string RefreshTokenPrefix = "refresh_";

    public async Task<AuthResponse> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var expires = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays);

        var entry = new RefreshTokenCacheEntry
        {
            UserId = user.Id,
            Expires = expires
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expires
        };

        await cache.SetStringAsync(
            RefreshTokenPrefix + refreshToken,
            JsonSerializer.Serialize(entry),
            options);

        return new AuthResponse(accessToken, refreshToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var key = RefreshTokenPrefix + refreshToken;

        var entryJson = await cache.GetStringAsync(key);

        if (entryJson == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var entry = JsonSerializer.Deserialize<RefreshTokenCacheEntry>(entryJson)!;

        if (entry.Expires <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        await cache.RemoveAsync(key);
        
        var user = await context.Users
               .Include(u => u.UserRoles)
               .ThenInclude(ur => ur.Role)
               .SingleOrDefaultAsync(u => u.Id == entry.UserId);

        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        return await GenerateTokensAsync(user);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
            {
                new("userId", user.Id.ToString()),
                new("userLogin", user.Login)
            };

        claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpiryMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}