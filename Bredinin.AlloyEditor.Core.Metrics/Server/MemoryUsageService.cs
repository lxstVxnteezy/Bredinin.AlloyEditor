using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Bredinin.AlloyEditor.Core.Metrics.Server
{
    public class MemoryUsageService : BackgroundService
    {
        private readonly Gauge _memoryGauge = Prometheus.Metrics.CreateGauge(
            "process_memory_usage_megabytes",  // Изменили название метрики
            "Current memory usage in megabytes (MB)");  // Обновили описание

        private readonly TimeSpan _delay = TimeSpan.FromSeconds(15);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var process = Process.GetCurrentProcess();

                // Конвертируем байты в мегабайты (1 МБ = 1024 * 1024 байт)
                double memoryUsageMb = process.WorkingSet64 / (1024.0 * 1024.0);

                _memoryGauge.Set(memoryUsageMb);  // Записываем значение в МБ

                await Task.Delay(_delay, stoppingToken);
            }
        }
    }
}