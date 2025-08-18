using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Bredinin.AlloyEditor.Core.Metrics.Server
{
    public class MemoryUsageService : BackgroundService
    {
        private readonly Gauge _memoryGauge = Prometheus.Metrics.CreateGauge(
            "process_memory_usage_megabytes",
            "Current memory usage in megabytes (MB)");

        private readonly TimeSpan _delay = TimeSpan.FromSeconds(15);

        protected override async Task ExecuteAsync(CancellationToken ctn)
        {
            while (!ctn.IsCancellationRequested)
            {
                var process = Process.GetCurrentProcess();

                double memoryUsageMb = process.WorkingSet64 / (1024.0 * 1024.0);

                _memoryGauge.Set(memoryUsageMb);

                await Task.Delay(_delay, ctn);
            }
        }
    }
}