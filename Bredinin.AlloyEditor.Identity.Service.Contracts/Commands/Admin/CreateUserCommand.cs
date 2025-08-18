using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin
{
    public record CreateUserCommand : IRequest<Guid> 
    {
        public required string Login { get; init; } 
        public required string FirstName { get; init; } 
        public required string LastName { get; init; } 
        public required string SecondName { get; init; }
        public int Age { get; init; }
        public required string Password { get; init; } 
        public Guid[] RoleIds { get; init; } = []; 
    }
}
