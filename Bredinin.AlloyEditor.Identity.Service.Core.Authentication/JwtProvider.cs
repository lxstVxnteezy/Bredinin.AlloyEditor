using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bredinin.AlloyEditor.Identity.Service.Domain;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Identity.Service.Authentication
{
    public static class JwtProvider
    {
        internal static readonly string Key;
        internal static readonly string Issuer;
        internal static readonly string Audience;

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

            var claims = new[]
                {
                    new Claim("userId", user.Id.ToString()),
                    new Claim("userLogin", user.Login),
                }
                .Concat(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)))
                .ToList();


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
