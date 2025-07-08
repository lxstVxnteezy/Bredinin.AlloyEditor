using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands
{
    public record EditUserCommand : IRequest
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string SecondName { get; init; } = null!;
        public int Age { get; init; }
        public Guid[] RoleIds { get; init; } = Array.Empty<Guid>();
    }
}
