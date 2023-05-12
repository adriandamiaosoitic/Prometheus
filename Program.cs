using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;

public class Program
{
    static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapMetrics();
                        endpoints.MapGet("/", async context =>
                        {
                            await context.Response.WriteAsync("Hello World!");
                        });
                    });
                });

                webBuilder.ConfigureServices(services =>
                {
                    services.AddMetrics();
                    services.AddMetricsTrackingMiddleware();
                    services.AddSingleton<MetricUpdateService>();
                    services.AddHostedService<MetricUpdateService>();

                    services.AddOpenTelemetryTracing(builder =>
                    {
                        builder
                            .AddAspNetCoreInstrumentation()
                            .AddConsoleExporter();
                    });

                    services.AddOpenTelemetryMetrics(builder =>
                    {
                        builder.AddAspNetCoreInstrumentation();
                        builder.AddPrometheusExporter(options =>
                        {
                            options.StartHttpListener = true;
                            options.HttpListenerPrefixes = new[] { "http://localhost:9184/" };
                        });
                    });
                });
            });
}