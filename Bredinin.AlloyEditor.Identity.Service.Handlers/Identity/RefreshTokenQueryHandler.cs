using System.IdentityModel.Tokens.Jwt;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
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
            var handler = new JwtSecurityTokenHandler(); //шняга для чтения токена
            var jwtToken = handler.ReadJwtToken(query.RefreshToken);

            var userId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value); //извлекаем юзера

            if (!await tokenService.ValidateRefreshTokenAsync(query.RefreshToken, userId)) 
                throw new UnauthorizedAccessException();

            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            await tokenService.UseRefreshTokenAsync(query.RefreshToken);
            //await tokenService.RevokeRefreshTokenAsync(query.RefreshToken); пока вырезал

            return await tokenService.GeneratePairsTokensAsync(user);
        }
    }
}
//var handler = new JwtSecurityTokenHandler();
//var jwtToken = handler.ReadJwtToken(query.RefreshToken);

//var userId = Guid.Parse(jwtToken.Claims.First(c => c.Type == "userId").Value);

//var refreshTokenIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "refreshTokenId");

//if (refreshTokenIdClaim == null || !Guid.TryParse(refreshTokenIdClaim.Value, out var refreshTokenId))
//    throw new InvalidOperationException("refreshTokenId not found in access token");

//var refreshToken = await context.RefreshTokens.AsQueryable()
//    .SingleOrDefaultAsync(x => x.Id == refreshTokenId, cancellationToken);

//if (refreshToken == null)
//    throw new UnauthorizedAccessException();

//if (!await tokenService.ValidateRefreshTokenAsync(refreshToken.Token, userId))
//    throw new UnauthorizedAccessException();

//var user = await context.Users
//    .Include(u => u.UserRoles)
//    .ThenInclude(ur => ur.Role)
//    .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

//if (user == null)
//    throw new UnauthorizedAccessException();

//await tokenService.UseRefreshTokenAsync(refreshToken.Token);
//await tokenService.RevokeRefreshTokenAsync(refreshToken.Token);

//return await tokenService.GeneratePairsTokensAsync(user);