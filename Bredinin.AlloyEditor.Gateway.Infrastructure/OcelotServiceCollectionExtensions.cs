using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Bredinin.AlloyEditor.Gateway.Infrastructure
{
    public static class OcelotServiceCollectionExtensions
    {
        public static IServiceCollection AddGatewayOcelot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOcelot(configuration);

            services.AddSwaggerForOcelot(configuration, options =>
            {
                options.GenerateDocsForGatewayItSelf = true;

                options.GenerateDocsDocsForGatewayItSelf(opt =>
                {
                    opt.GatewayDocsTitle = "My Gateway API";
                    opt.GatewayDocsOpenApiInfo = new()
                    {
                        Title = "Gateway API",
                        Version = "v1",
                        Description = "Central API Gateway for all microservices"
                    };

                    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                });
            });

            return services;
        }

        public static IApplicationBuilder UseGatewayOcelot(this IApplicationBuilder app)
        {
            app.UseSwaggerForOcelotUI(opt =>
            {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            }, uiOpt =>
            {
                uiOpt.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway Controllers");
            });
            
            app.UseOcelot().Wait();

            return app;
        }
    }
}
