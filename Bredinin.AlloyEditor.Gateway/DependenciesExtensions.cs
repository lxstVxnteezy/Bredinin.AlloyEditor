using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Gateway.Handlers;
using Refit;

namespace Bredinin.AlloyEditor.Gateway
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<JwtRefreshHandler>();


            services.AddRefitClient<IDictionaryClient>()
                .AddHttpMessageHandler<JwtRefreshHandler>()
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

            services.AddRefitClient<IAdminClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(configuration["IdentityApi:BaseUrl"]);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            services.AddRefitClient<IAlloyClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(configuration["AlloyEditorApi:BaseUrl"]);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            services.AddRefitClient<IAlloySystemClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(configuration["AlloyEditorApi:BaseUrl"]);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });

            return services;
        }
    }
}
