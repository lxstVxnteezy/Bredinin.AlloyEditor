using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bredinin.AlloyEditor.Common.Swagger
{
    public static class SwaggerCommonExtensions
    {
        public static IServiceCollection AddServiceSwagger(
            this IServiceCollection services,
            string serviceName,
            bool jwtSecurityDefinition = false,
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

                if (jwtSecurityDefinition)
                    AddJwtSecurityDefinition(options);

                TryAddXmlComments(options);

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

        private static void TryAddXmlComments(SwaggerGenOptions options)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly? mainAssembly = null;

            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic &&
                    !assembly.FullName?.StartsWith("System.") == true &&
                    !assembly.FullName?.StartsWith("Microsoft.") == true &&
                    assembly.EntryPoint != null)
                {
                    mainAssembly = assembly;
                    break;
                }
            }

            mainAssembly ??= assemblies.FirstOrDefault(a =>
                !a.IsDynamic &&
                !a.FullName?.StartsWith("System.") == true &&
                !a.FullName?.StartsWith("Microsoft.") == true);

            if (mainAssembly == null)
                throw new InvalidConfigurationException();

            var assemblyName = mainAssembly.GetName().Name;
            var xmlFileName = $"{assemblyName}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        }

        public static IApplicationBuilder UseServiceSwaggerUi(
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
