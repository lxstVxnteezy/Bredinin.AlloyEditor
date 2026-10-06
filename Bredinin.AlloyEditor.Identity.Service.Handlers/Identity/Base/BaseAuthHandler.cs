using System.Text.Json;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Entities;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Services.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity.Base;

public abstract class BaseAuthHandler<TContract, TResponse>(
    ITokenService tokenService,
    IdentityDbContext context,
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    JwtOptionsAccessor jwtOptionsAccessor)
    : IRequestHandler<TContract, TResponse>
    where TContract : IRequest<TResponse>
{
    protected const string RefreshTokenPrefix = "refresh_";
    protected readonly ITokenService TokenService = tokenService;
    protected readonly IdentityDbContext Context = context;
    protected readonly IDistributedCache Cache = cache;

    public abstract Task<TResponse> Handle(TContract request, CancellationToken cancellationToken);

    protected async Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expires, CancellationToken cancellationToken)
    {
        var entry = new RefreshTokenCacheEntry
        {
            UserId = userId,
            Expires = expires
        };

        await Cache.SetStringAsync(
            RefreshTokenPrefix + refreshToken,
            JsonSerializer.Serialize(entry),
            new DistributedCacheEntryOptions { AbsoluteExpiration = expires },
            cancellationToken);

        await AddTokenToUserIndexAsync(userId, refreshToken, cancellationToken);
    }

    protected async Task RevokeTokenByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await GetUserTokenAsync(userId, cancellationToken);
        var db = redis.GetDatabase();

        foreach (var token in tokens)
        {
            await Cache.RemoveAsync(RefreshTokenPrefix + token, cancellationToken);
        }

        await db.KeyDeleteAsync($"user_tokens:{userId}");

        await Cache.SetStringAsync(
            $"revoked:{userId}",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(jwtOptionsAccessor.Value.AccessTokenExpiryMinutes)
            },
            cancellationToken);
    }

    protected async Task<RefreshTokenCacheEntry?> GetAndValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var key = RefreshTokenPrefix + refreshToken;
        var entryJson = await Cache.GetStringAsync(key, cancellationToken);

        if (entryJson == null)
        {
            return null;
        }

        var entry = JsonSerializer.Deserialize<RefreshTokenCacheEntry>(entryJson);

        if (entry == null || entry.Expires <= DateTime.UtcNow)
        {
            return null;
        }

        return entry;
    }

    protected async Task RemoveOldRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        await Cache.RemoveAsync(RefreshTokenPrefix + refreshToken, cancellationToken);
    }

    protected async Task<Domain.User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await Context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
    protected AuthResponse CreateTokenResponse(string accessToken, string refreshToken)
    {
        return TokenService.CreateAuthResponse(accessToken, refreshToken);
    }

    private async Task AddTokenToUserIndexAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        await db.SetAddAsync($"user_tokens:{userId}", refreshToken);
    }

    private async Task<string[]> GetUserTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var db = redis.GetDatabase();
        var members = await db.SetMembersAsync($"user_tokens:{userId}");

        return members.Select(m => m.ToString()).ToArray();
    }
}
