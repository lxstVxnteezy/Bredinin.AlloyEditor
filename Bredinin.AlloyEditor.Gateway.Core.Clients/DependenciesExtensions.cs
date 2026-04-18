using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddClients(
            this IServiceCollection services, 
            IConfiguration configuration,
            IHostEnvironment environment) // Добавляем environment
        {
            var alloyEditorBaseUrl = configuration["AlloyEditorApi:BaseUrl"];
            var identityApiBaseUrl = configuration["IdentityApi:BaseUrl"];
            
            // Создаем HttpMessageHandler в зависимости от окружения
            var httpMessageHandler = CreateHttpMessageHandler(environment);

            services.AddRefitClient<IDictionaryClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(alloyEditorBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler);

            services.AddRefitClient<IAuthClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(identityApiBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler);

            services.AddRefitClient<IAdminClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(identityApiBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler);

            services.AddRefitClient<IAlloyClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(alloyEditorBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler);

            services.AddRefitClient<IAlloySystemClient>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(alloyEditorBaseUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler);

            return services;
        }

        private static HttpMessageHandler CreateHttpMessageHandler(IHostEnvironment environment)
        {
            var handler = new HttpClientHandler();
            
            if (environment.IsDevelopment())
            {
                handler.ServerCertificateCustomValidationCallback = 
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }
            
            return handler;
        }
    }
}