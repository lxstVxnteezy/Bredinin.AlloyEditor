using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEditor.Services.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
             
            services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.SectionName));

            return services;
        }
    }
}

