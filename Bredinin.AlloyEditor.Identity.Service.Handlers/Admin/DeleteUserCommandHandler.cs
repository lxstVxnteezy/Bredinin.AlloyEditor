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
            await context.Users.Where(x => x.Id == request.UserId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}