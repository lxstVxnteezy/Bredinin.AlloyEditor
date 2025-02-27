using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Bredinin.MyPetProject.Swagger;

public static class SwaggerServiceExtensions
{ 
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    { 
        services.AddSwaggerGen(swaggerOptions => 
            { 
                swaggerOptions.SwaggerDoc("v1",new OpenApiInfo{Title = "MyPetProject", Version = "v1"});
            });
        
        return services;
    }
    
    public static void UseCustomSwagger(this IApplicationBuilder application)
    {
        application.UseSwagger();
        application.UseSwaggerUI();
    }
}