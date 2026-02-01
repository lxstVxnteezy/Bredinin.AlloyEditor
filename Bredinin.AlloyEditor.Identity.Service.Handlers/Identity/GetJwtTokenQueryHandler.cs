using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    public class GetJwtTokenQueryHandler(
        ITokenService tokenService,
        IdentityDbContext context,
        IPasswordHasher passwordHasher) : IRequestHandler<GetJwtTokenQuery, AuthResponse>
    {
        public async Task<AuthResponse> Handle(GetJwtTokenQuery request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(x => x.Login == request.Login, cancellationToken);

            if (user == null || !passwordHasher.VerifyPassword(request.Password, user.Hash))
                throw new UnauthorizedAccessException("Invalid credentials");

            var response = await tokenService.GenerateTokensAsync(user);

            return response;
        }
    }
}