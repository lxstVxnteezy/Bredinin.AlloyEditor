using System.Text;
using Bredinin.AlloyEditor.Services.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bredinin.AlloyEditor.Gateway.Core.Authentication
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddAddAuthenticationCustom(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection(JwtConfiguration.SectionName)
                .Get<JwtConfiguration>();

            if (jwtConfig == null)
                throw new InvalidOperationException("JWT configuration not found");

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
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtConfig.Key))
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
