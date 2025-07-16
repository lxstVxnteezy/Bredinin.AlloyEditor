using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    internal class SearchUserQueryHandler(IdentityDbContext context) : IRequestHandler<GetAllSearchUserQueries, SearchUserQuery[]>

    {
        public async Task<SearchUserQuery[]> Handle(GetAllSearchUserQueries request, CancellationToken cancellationToken)
        {
            var users = await context.Users
                .AsQueryable()
                .Include(x => x.UserRoles)
                .ToArrayAsync(cancellationToken);

            return users.Select(MapToResponse).ToArray();
        }

        private static SearchUserQuery MapToResponse(User user)
        {
            return new SearchUserQuery()
            {
                Id = user.Id,
                Age = user.Age,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Login = user.Login,
                SecondName = user.SecondName,
                RoleIds = user.UserRoles.Select(x => x.RoleId).ToArray()
            };

        }
    }
}
