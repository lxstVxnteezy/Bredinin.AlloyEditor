using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bredinin.MyPetProject.Handlers
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddHandlersScopedServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var handlerTypes = assembly.GetTypes()
                .Where(t => typeof(IHandler).IsAssignableFrom(t)
                            && t.IsClass
                            && !t.IsAbstract);

            foreach (var type in handlerTypes)
            {
                services.AddScoped(typeof(IHandler), type);
            }
            return services;
        }
    }
}
