using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin
{
  public record SearchUserQuery(
      Guid Id,
      string Login,
      string FirstName,
      string LastName,
      string SecondName,
      int Age,
      Guid[] RoleIds
  );

    public record GetAllSearchUserQueries : IRequest<SearchUserQuery[]>;
}
