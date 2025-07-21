using Bredinin.AlloyEditor.Identity.Service.Domain;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshAllTokenUserAsync(User user);
        Task UseRefreshTokenAsync(string refreshToken); 
    }
}
