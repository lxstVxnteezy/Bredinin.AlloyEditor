using Bredinin.AlloyEditor.Identity.Service.Authentication;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    public class GetJwtTokenQueryHandler(
        IdentityDbContext context, 
        IPasswordHasher passwordHasher) : IRequestHandler<GetJwtTokenQuery, string>
    {
        public async Task<string> Handle(GetJwtTokenQuery request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(x => x.Login == request.Login, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!passwordHasher.VerifyPassword(request.Password, user.Hash)) 
                throw new UnauthorizedAccessException("Invalid credentials");

            return JwtProvider.GenerateToken(user);
        }
    }
}
