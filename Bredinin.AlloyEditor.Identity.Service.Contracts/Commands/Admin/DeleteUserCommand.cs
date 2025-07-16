using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin
{
    public record DeleteUserCommand(Guid UserId) : IRequest;
}
