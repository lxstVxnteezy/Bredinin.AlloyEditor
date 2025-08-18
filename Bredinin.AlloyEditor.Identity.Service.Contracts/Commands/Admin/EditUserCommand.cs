using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin
{
    public record EditUserCommand : IRequest<UpdateUserResponse>
    {
        public Guid Id { get; set; }
        public required string FirstName { get; init; } 
        public required string LastName { get; init; } 
        public required string SecondName { get; init; }
        public int Age { get; init; }
        public Guid[] RoleIds { get; init; } = [];
    }
}
