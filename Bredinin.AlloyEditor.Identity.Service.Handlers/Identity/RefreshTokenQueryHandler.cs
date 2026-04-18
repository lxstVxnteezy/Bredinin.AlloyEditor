using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler.Identity.Base;
using Bredinin.AlloyEditor.Services.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    public class RefreshTokenQueryHandler(
        ITokenService tokenService,
        IdentityDbContext context,
        IDistributedCache cache,
        JwtOptionsAccessor  jwtOptionsAccessor)
        : BaseAuthHandler<RefreshTokenQuery>(tokenService, context, cache, jwtOptionsAccessor)
    {
        public override async Task<AuthResponse> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var entry = await GetAndValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
            
            if (entry == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            await RemoveRefreshTokenAsync(request.RefreshToken, cancellationToken);

            var user = await GetUserWithRolesAsync(entry.UserId, cancellationToken);
            
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var accessToken = TokenService.GenerateAccessToken(user);
            var refreshToken = TokenService.GenerateRefreshToken();

            var expires = DateTime.UtcNow.AddDays(jwtOptionsAccessor.Value.RefreshTokenExpiryDays);
          
            await SaveRefreshTokenAsync(refreshToken, user.Id, expires, cancellationToken);

            return CreateTokenResponse(accessToken, refreshToken);
        }
    }
}