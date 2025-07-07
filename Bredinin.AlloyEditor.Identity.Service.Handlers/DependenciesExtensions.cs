using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEditor.Identity.Service.Handler
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            return services;
        }
    }
}
