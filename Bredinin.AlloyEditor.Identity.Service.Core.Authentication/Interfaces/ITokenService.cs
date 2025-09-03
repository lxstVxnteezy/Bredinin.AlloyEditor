using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.Domain;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResponse> GeneratePairsTokensAsync(User user);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
        //Task RevokeRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshAllTokenUserAsync(Guid userId);
        Task UseRefreshTokenAsync(string refreshToken);
    }
}
