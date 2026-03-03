using Bredinin.AlloyEditor.Identity.Service.Authentication.Entities;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity.Base
{
    public abstract class BaseAuthHandler<TContract>(
        ITokenService tokenService,
        IdentityDbContext context,
        IDistributedCache cache)
        : IRequestHandler<TContract, AuthResponse>
        where TContract : IRequest<AuthResponse>
    {
        protected const string RefreshTokenPrefix = "refresh_";
        protected readonly ITokenService TokenService = tokenService;
        protected readonly IdentityDbContext Context = context;
        protected readonly IDistributedCache Cache = cache;

        public abstract Task<AuthResponse> Handle(TContract request, CancellationToken cancellationToken);

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
        }

        protected async Task<RefreshTokenCacheEntry?> GetAndValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var key = RefreshTokenPrefix + refreshToken;
            var entryJson = await Cache.GetStringAsync(key, cancellationToken);

            if (entryJson == null)
                return null;

            var entry = JsonSerializer.Deserialize<RefreshTokenCacheEntry>(entryJson);

            if (entry == null || entry.Expires <= DateTime.UtcNow)
                return null;

            return entry;
        }

        protected async Task RemoveRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            await Cache.RemoveAsync(RefreshTokenPrefix + refreshToken, cancellationToken);
        }

        protected async Task<Domain.User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await Context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
        protected AuthResponse CreateTokenResponse(string accessToken, string refreshToken)
        {
            return TokenService.CreateAuthResponse(accessToken, refreshToken);
        }
    }
}