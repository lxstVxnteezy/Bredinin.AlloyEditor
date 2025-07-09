using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Gateway.Infrastructure
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                                      ?? throw new InvalidOperationException("JWT_ISSUER is not set"),
                        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                                        ?? throw new InvalidOperationException("JWT_AUDIENCE is not set"),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")
                                                   ?? throw new InvalidOperationException("JWT_KEY is not set")))
                    };
                });

            return services;
        }
    }
}