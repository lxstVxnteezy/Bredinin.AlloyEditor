using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries
{
    public record GetJwtTokenQuery(string Username, string Password) : IRequest<string>;
}
