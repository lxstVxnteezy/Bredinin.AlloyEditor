using System.IdentityModel.Tokens.Jwt;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    internal class RefreshTokenQueryHandler(
        ITokenService tokenService,
        IdentityDbContext context) : IRequestHandler<RefreshTokenQuery, AuthResponse>
    {
        public async Task<AuthResponse> Handle(RefreshTokenQuery query, CancellationToken cancellationToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(query.AccessToken);

            var userId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);

            if (!await tokenService.ValidateRefreshTokenAsync(query.RefreshToken, userId))
                throw new UnauthorizedAccessException();

            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            await tokenService.UseRefreshTokenAsync(query.RefreshToken);
            await tokenService.RevokeRefreshTokenAsync(query.RefreshToken);

            var newAccessToken = tokenService.GenerateAccessToken(user);
            var newRefreshToken = await tokenService.GenerateRefreshTokenAsync(user);

            return new AuthResponse(
                newAccessToken,
                newRefreshToken);
        }
    }
}
