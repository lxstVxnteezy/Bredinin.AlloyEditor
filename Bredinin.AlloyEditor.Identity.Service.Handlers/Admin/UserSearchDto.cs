using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Admin
{
    internal class UserSearchDto(IdentityDbContext context) : IRequestHandler<GetAllSearchUserQueries, SearchUserQuery[]>
    {
        public async Task<SearchUserQuery[]> Handle(GetAllSearchUserQueries request, CancellationToken cancellationToken)
        {
            var users = await context.Users
                .AsNoTracking()
                .Include(x => x.UserRoles)
                .Select(x => new SearchUserQuery(
                    x.Id,
                    x.Login,
                    x.FirstName,
                    x.LastName,
                    x.SecondName,
                    x.Age,
                    x.UserRoles.Select(x=>x.RoleId).ToArray()
                    ))
                .ToArrayAsync(cancellationToken);

            return users;
        }
    }
}
