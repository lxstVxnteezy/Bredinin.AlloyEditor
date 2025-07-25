﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Bredinin.AlloyEditor.Core.Swagger;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(swaggerOptions => 
        { 
            swaggerOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Bredinin.AlloyEditor.Service", Version = "v1" });
            swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
            { 
                Description = "JWT Authorization header using the Bearer scheme.", 
                Name = "Authorization", 
                In = ParameterLocation.Header, 
                Type = SecuritySchemeType.Http, 
                Scheme = "bearer", 
                BearerFormat = "JWT"
            });
           
            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        });

        return services;
    }

    public static void UseCustomSwagger(this IApplicationBuilder application)
    {
        application.UseSwagger();
        application.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            c.RoutePrefix = string.Empty;
        });
    }
}