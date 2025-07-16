using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    internal class EditUserCommandHandler(IdentityDbContext context) : IRequestHandler<EditUserCommand, UpdateUserResponse>
    {
        public async Task<UpdateUserResponse> Handle(EditUserCommand request, CancellationToken cancellationToken)
        {
            AssertRole(request);

            var foundUser = await context.Users.AsQueryable()
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (foundUser == null)
                throw new InvalidOperationException($"User with id '{request.Id}' not found in db");

            foundUser.UserRoles.Clear();

            foundUser.Age = request.Age;
            foundUser.SecondName = request.SecondName;
            foundUser.FirstName = request.FirstName;
            foundUser.LastName = request.LastName;
            foundUser.UserRoles = request.RoleIds.Select(roleId => new UserRole
            {
                UserId = foundUser.Id,
                RoleId = roleId
            }).ToList();

            await context.SaveChangesAsync(cancellationToken);

            return new UpdateUserResponse(
                Login: foundUser.Login,
                FirstName: foundUser.FirstName,
                LastName: foundUser.LastName,
                SecondName: foundUser.SecondName,
                RoleIds: foundUser.UserRoles.Select(x=>x.RoleId).ToArray());
        }

        private void AssertRole(EditUserCommand request)
        {
            var missingRoles = request.RoleIds
                .Where(roleId => context.Roles
                    .Find(roleId) == null)
                .ToArray();

            if (missingRoles.Any())
            {
                var missingRoleIds = string.Join(", ", missingRoles);
                throw new InvalidOperationException($"Roles not found: {missingRoleIds}");
            }
        }
    }
}
