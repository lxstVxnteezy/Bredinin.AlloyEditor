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
        internal static readonly int AccessTokenExpiryMinutes;
        internal static readonly int RefreshTokenExpiryDays;

        static JwtProvider()
        {
            Key = Environment.GetEnvironmentVariable("JWT_KEY")
                  ?? throw new InvalidOperationException("JWT_KEY is not set");
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                     ?? throw new InvalidOperationException("JWT_ISSUER is not set");
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                       ?? throw new InvalidOperationException("JWT_AUDIENCE is not set");

            if (!int.TryParse(Environment.GetEnvironmentVariable("ACCESS_TOKEN_EXPIRY_MINUTES"), out AccessTokenExpiryMinutes))
                AccessTokenExpiryMinutes = 15;

            if (!int.TryParse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS"), out RefreshTokenExpiryDays))
                RefreshTokenExpiryDays = 7;
        }

        public static string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim("userId", user.Id.ToString()),
                new Claim("userLogin", user.Login),
            }
            .Concat(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)))
            .ToList();

            return GenerateToken(claims, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));
        }

        public static string GenerateRefreshToken()
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, JwtProvider.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, JwtProvider.Audience),
                new Claim(JwtRegisteredClaimNames.Exp,
                    new DateTimeOffset(DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateToken(IEnumerable<Claim> claims, TimeSpan expiration)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(expiration),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}