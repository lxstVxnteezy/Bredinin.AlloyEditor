using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using MediatR;


namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity
{
    internal class RefreshTokenQueryHandler  (
        ITokenService tokenService
        ) : IRequestHandler<RefreshTokenQuery, AuthResponse>
    {
        public async Task<AuthResponse> Handle(RefreshTokenQuery query, CancellationToken cancellationToken)
        {
            return await tokenService.RefreshAsync(query.RefreshToken);
        }
    }
}
