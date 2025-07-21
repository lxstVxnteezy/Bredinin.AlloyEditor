using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    internal class ResetPasswordCommandHandler(IdentityDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService) : IRequestHandler<ResetPasswordUserCommand>
    {
        public async Task Handle(ResetPasswordUserCommand request, CancellationToken cancellationToken)
        {
            var foundUser = await context.Users.AsQueryable()
                .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (foundUser == null)
                throw new InvalidOperationException("not found user");

            await tokenService.RevokeRefreshAllTokenUserAsync(foundUser);

            foundUser.Hash = passwordHasher.Generate(request.Password);
            
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
