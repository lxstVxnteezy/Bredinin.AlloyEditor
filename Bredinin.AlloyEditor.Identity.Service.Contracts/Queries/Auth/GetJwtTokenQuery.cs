using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth
{
    public record GetJwtTokenQuery : IRequest<AuthResponse>
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
