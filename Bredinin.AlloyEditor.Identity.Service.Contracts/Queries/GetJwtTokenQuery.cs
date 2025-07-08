using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries
{
    public record GetJwtTokenQuery : IRequest<string>
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
