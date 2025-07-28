using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRefitClient<IDictionaryClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(configuration["AlloyEditorApi:BaseUrl"]);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            services.AddRefitClient<IAuthClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(configuration["IdentityApi:BaseUrl"]);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            return services;
        }
    }
}
