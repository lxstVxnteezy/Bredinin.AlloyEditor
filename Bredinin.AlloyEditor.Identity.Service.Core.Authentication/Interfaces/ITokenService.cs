using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.Domain;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Генерирует access token для пользователя
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Генерирует случайный refresh token (просто строку)
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Создаёт объект ответа с токенами
        /// </summary>
        AuthResponse CreateAuthResponse(string accessToken, string refreshToken);
    }
}