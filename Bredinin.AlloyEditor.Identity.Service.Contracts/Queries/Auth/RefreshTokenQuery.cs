using MediatR;

namespace Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth
{
    public record RefreshTokenQuery(string AccessToken) : IRequest<AuthResponse>;
}
