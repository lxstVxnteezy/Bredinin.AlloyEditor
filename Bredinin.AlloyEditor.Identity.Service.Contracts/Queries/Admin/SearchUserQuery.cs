using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin
{
    public record SearchUserQuery 
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string SecondName { get; set; } = null!;
        public int Age { get; set; }
        public Guid[] RoleIds = null!;
    }

    public record GetAllSearchUserQueries : IRequest<SearchUserQuery[]>;

}
