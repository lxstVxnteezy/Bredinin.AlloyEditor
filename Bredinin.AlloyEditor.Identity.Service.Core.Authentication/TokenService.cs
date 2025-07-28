using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication
{
    public class TokenService(IdentityDbContext context) : ITokenService
    {
        public string GenerateAccessToken(User user)
        {
            return JwtProvider.GenerateAccessToken(user);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var token = JwtProvider.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(JwtProvider.RefreshTokenExpiryDays),
                UserId = user.Id,
                IsUsed = false,
                IsRevoked = false
            };

            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, Guid userId)
        {
            var handler = new JwtSecurityTokenHandler();

            var validationResult = await handler.ValidateTokenAsync(
                refreshToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtProvider.Key)),
                    ValidateIssuer = false,
                    ValidIssuer = JwtProvider.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtProvider.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                });

            if (!validationResult.IsValid)
                return false;

            var token = await context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

            return token != null &&
                   !token.IsUsed &&
                   !token.IsRevoked &&
                   token.Expires > DateTime.UtcNow;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token != null)
            {
                token.IsRevoked = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task RevokeRefreshAllTokenUserAsync(Guid userId)
        {
            await context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(rt => rt.IsRevoked, true));
        }

        public async Task UseRefreshTokenAsync(string refreshToken)
        {
            var token = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token != null)
            {
                token.IsUsed = true;
                await context.SaveChangesAsync();
            }
        }
    }
}
