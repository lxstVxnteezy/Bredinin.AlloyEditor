using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin
{
    public record CreateUserCommand : IRequest<Guid> 
    {
        public string Login { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string SecondName { get; init; } = null!;
        public int Age { get; init; }
        public string Password { get; init; } = null!; 
        public Guid[] RoleIds { get; init; } = Array.Empty<Guid>(); 
    }
}
