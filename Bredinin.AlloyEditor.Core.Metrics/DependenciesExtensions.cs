using Bredinin.AlloyEditor.Core.Metrics.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Bredinin.AlloyEditor.Core.Metrics
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddServerMetrics(this IServiceCollection services)
        {
            services.AddHostedService<CpuUsageService>();
            services.AddHostedService<MemoryUsageService>();

            return services;
        }
    }
}
