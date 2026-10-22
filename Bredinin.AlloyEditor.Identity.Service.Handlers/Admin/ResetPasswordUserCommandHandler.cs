using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    public class ResetPasswordUserCommandHandler(
        IdentityDbContext context,
        IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordUserCommand>
    {
        public async Task Handle(ResetPasswordUserCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new BusinessException($"User with id '{request.UserId}' not found in db");

            user.Hash = passwordHasher.Generate(request.Password);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
