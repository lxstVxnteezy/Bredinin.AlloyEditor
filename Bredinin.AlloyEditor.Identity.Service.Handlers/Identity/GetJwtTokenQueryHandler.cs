using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler.Identity.Base;
using Bredinin.AlloyEditor.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    public class GetJwtTokenQueryHandler(
        ITokenService tokenService,
        IdentityDbContext context,
        IDistributedCache cache,
        IPasswordHasher passwordHasher,
        JwtOptionsAccessor  jwtOptionsAccessor)
        : BaseAuthHandler<GetJwtTokenQuery>(tokenService, context, cache,jwtOptionsAccessor)
    {

        public override async Task<AuthResponse> Handle(GetJwtTokenQuery request, CancellationToken cancellationToken)
        {
            var user = await Context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(x => x.Login == request.Login, cancellationToken);

            if (user == null || !passwordHasher.VerifyPassword(request.Password, user.Hash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var accessToken = TokenService.GenerateAccessToken(user);
            var refreshToken = TokenService.GenerateRefreshToken();

            var expires = DateTime.UtcNow.AddDays(jwtOptionsAccessor.Value.RefreshTokenExpiryDays);
            
            await SaveRefreshTokenAsync(refreshToken, user.Id, expires, cancellationToken);

            return CreateTokenResponse(accessToken, refreshToken);
        }
    }
}