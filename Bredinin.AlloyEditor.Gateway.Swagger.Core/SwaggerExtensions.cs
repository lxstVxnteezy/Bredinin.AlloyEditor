using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Bredinin.AlloyEditor.Gateway.Swagger.Core
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddGatewaySwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });
            });

            services.AddSwaggerForOcelot(configuration);

            return services;
        }

        public static IApplicationBuilder UseGatewaySwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSwaggerForOcelotUI(opt =>
            {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            });

            return app;
        }
    }
}
