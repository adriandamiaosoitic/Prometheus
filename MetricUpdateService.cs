using System;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;

public class MetricUpdateService : BackgroundService
{
    private readonly Meter _meter;
    private readonly Random _random;
    private readonly ObservableGauge<double> _gauge;

    public MetricUpdateService(MeterProvider meterProvider)
    {
        _meter = meterProvider.GetMeter("myapp.metrics");
        _random = new Random();
        _gauge = _meter.CreateObservableGauge<double>("random-value", () => _random.NextDouble() * 100);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _gauge.Update(_random.NextDouble() * 100);
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}