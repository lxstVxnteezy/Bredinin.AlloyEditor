using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bredinin.AlloyEditor.Domain.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Core.Authentication
{
    public static class JwtProvider
    {
        public static readonly string Key;
        public static readonly string Issuer;
        public static readonly string Audience;

        static JwtProvider()
        {
            Key = Environment.GetEnvironmentVariable("JWT_KEY")
                  ?? throw new InvalidOperationException("JWT_KEY is not set");
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                     ?? throw new InvalidOperationException("JWT_ISSUER is not set");
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? throw new InvalidOperationException("JWT_AUDIENCE is not set");
        }
        public static string GenerateToken(User user)
        {
            Claim[] claims = [new("userId", user.Id.ToString())];

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                issuer: Issuer,
                audience: Audience,
                expires: DateTime.Now.AddHours(12));

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
    }
}
