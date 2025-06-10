using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Bredinin.AlloyEditor.Core.Metrics.Server
{
    public class CpuUsageService : BackgroundService
    {
        private readonly Gauge _cpuGauge = Prometheus.Metrics.CreateGauge(
            "system_cpu_usage_percent",
            "Current CPU usage in percent");

        private readonly TimeSpan _delay = TimeSpan.FromSeconds(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var prevCpuTime = Process.GetCurrentProcess().TotalProcessorTime;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_delay, stoppingToken);

                var currCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                var cpuUsedMs = (currCpuTime - prevCpuTime).TotalMilliseconds;
                var totalMsPassed = _delay.TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                _cpuGauge.Set(cpuUsageTotal * 100);

                prevCpuTime = currCpuTime;
            }
        }
    }
}
