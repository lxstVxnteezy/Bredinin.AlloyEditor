using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth
{
    public record RefreshTokenQuery(string RefreshToken) : IRequest<AuthResponse>;
}
