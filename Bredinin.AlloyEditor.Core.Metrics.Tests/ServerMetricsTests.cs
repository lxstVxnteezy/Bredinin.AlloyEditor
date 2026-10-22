using Bredinin.AlloyEditor.Core.Metrics;
using Bredinin.AlloyEditor.Core.Metrics.Server;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Bredinin.AlloyEditor.Core.Metrics.Tests;

public class ServerMetricsTests
{
    [Fact]
    public void AddServerMetrics_ShouldRegisterBothHostedServices()
    {
        var services = new ServiceCollection().AddServerMetrics();

        services.Where(d => d.ServiceType == typeof(IHostedService))
            .Select(d => d.ImplementationType)
            .Should().BeEquivalentTo([typeof(CpuUsageService), typeof(MemoryUsageService)]);
    }

    [Fact]
    public async Task MemoryUsageService_ShouldPublishWorkingSetImmediatelyAndStopOnCancel()
    {
        var service = new MemoryUsageService();

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(300);
        await service.StopAsync(CancellationToken.None);

        var gauge = Prometheus.Metrics.DefaultRegistry;
        using var stream = new MemoryStream();
        await gauge.CollectAndExportAsTextAsync(stream);
        var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        text.Should().Contain("process_memory_usage_megabytes");
        var value = double.Parse(
            text.Split('\n').First(l => l.StartsWith("process_memory_usage_megabytes ")).Split(' ')[1],
            System.Globalization.CultureInfo.InvariantCulture);
        value.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task CpuUsageService_ShouldStartAndStopCleanly()
    {
        var service = new CpuUsageService();

        await service.StartAsync(CancellationToken.None);
        var stop = () => service.StopAsync(CancellationToken.None);

        await stop.Should().NotThrowAsync();
    }
}
