using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth
{
    public record GetJwtTokenQuery : IRequest<AuthResponse>
    {
        public required string Login { get; set; } 
        public required string Password { get; set; }
    }
}
