using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin
{
    public record SearchUserQuery 
    {
        public Guid Id { get; set; }
        public required string Login { get; set; } 
        public required string FirstName { get; set; } 
        public required string LastName { get; set; } 
        public required string SecondName { get; set; } 
        public int Age { get; set; }
        public Guid[] RoleIds = [];
    }

    public record GetAllSearchUserQueries : IRequest<SearchUserQuery[]>;
}
