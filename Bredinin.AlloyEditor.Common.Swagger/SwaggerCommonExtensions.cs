using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bredinin.AlloyEditor.Common.Swagger
{
    public static class SwaggerCommonExtensions
    {
        public static IServiceCollection AddServiceSwagger(
            this IServiceCollection services,
            string serviceName,
            string version = "v1",
            Action<SwaggerGenOptions>? customize = null)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = serviceName,
                    Version = version
                });

                AddJwtSecurityDefinition(options);

                customize?.Invoke(options);
            });

            return services;
        }

        private static void AddJwtSecurityDefinition(SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }

        public static IApplicationBuilder UseServiceSwaggerUI(
            this IApplicationBuilder app,
            string? routePrefix = null,
            string uiTitle = "API Documentation")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", uiTitle);
                c.RoutePrefix = routePrefix ?? string.Empty;
            });

            return app;
        }
    }
}
