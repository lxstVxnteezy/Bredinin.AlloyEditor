using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    public class CreateUserCommandHandler(
        IdentityDbContext context, 
        IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, Guid>
    {
        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (await context.Users.AsQueryable().AnyAsync(u => u.Login == request.Login, cancellationToken))
                throw new InvalidOperationException($"User with login '{request.Login}' already exists");

            var existingRolesCount = await context.Roles.AsQueryable()
                .Where(r => request.RoleIds.Contains(r.Id))
                .CountAsync(cancellationToken);

            if (existingRolesCount != request.RoleIds.Length)
                throw new ArgumentException("One or more specified roles don't exist");

            var user = new User
            {
                Login = request.Login,
                FirstName = request.FirstName,
                LastName = request.LastName,
                SecondName = request.SecondName,
                Age = request.Age,
                Hash = passwordHasher.Generate(request.Password),
                UserRoles = request.RoleIds.Select(roleId => new UserRole { RoleId = roleId }).ToArray()
            };

            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
