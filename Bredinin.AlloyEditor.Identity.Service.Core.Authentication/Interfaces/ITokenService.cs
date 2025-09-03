using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.Domain;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResponse> GenerateTokensAsync(User user);
        Task<AuthResponse> RefreshAsync(string refreshToken);
    }
}
