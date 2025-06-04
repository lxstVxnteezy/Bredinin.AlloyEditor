using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bredinin.MyPetProject.Handlers
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var serviceType = typeof(IHandler);

            foreach (var implementationType in assembly.GetTypes()
                         .Where(type => serviceType.IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract))
            {
                var handlerInterface = implementationType.GetInterfaces().Single(x => x != serviceType);
                services.AddTransient(handlerInterface, implementationType);
            }

            return services;
        }
    }
}
