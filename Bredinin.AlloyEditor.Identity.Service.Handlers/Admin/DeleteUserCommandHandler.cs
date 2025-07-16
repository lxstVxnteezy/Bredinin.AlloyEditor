using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    internal class DeleteUserCommandHandler(IdentityDbContext context) : IRequestHandler<DeleteUserCommand>
    {
        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.AsQueryable().SingleOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new InvalidOperationException("not found in db");

            context.Users.Remove(user);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
