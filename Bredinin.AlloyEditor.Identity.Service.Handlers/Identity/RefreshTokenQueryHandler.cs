using System.IdentityModel.Tokens.Jwt;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    internal class RefreshTokenQueryHandler  (
        ITokenService tokenService,
        IdentityDbContext context) : IRequestHandler<RefreshTokenQuery, AuthResponse>
    {
        public async Task<AuthResponse> Handle(RefreshTokenQuery query, CancellationToken cancellationToken)
        {
            return await tokenService.RefreshAsync(query.RefreshToken);
        }
    }
}
