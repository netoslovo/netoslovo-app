using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.Diagnostics;

namespace WordoGuessr.WebAPI.OpenTelemetry;

internal static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        var openTelemetryOptions = builder.Configuration.GetOptionalValidatedOptions<OpenTelemetryOptions>();
        if (openTelemetryOptions is null)
        {
            return builder;
        }

        builder.Services.Configure<MetricReaderOptions>(options =>
        {
            options.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds =
                (int)openTelemetryOptions.ExportInterval.TotalMilliseconds;
        });

        var serviceInstanceId =
            openTelemetryOptions.ServiceInstanceId
            ?? Environment.MachineName;

        var otel = builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                builder.Environment.ApplicationName,
                serviceInstanceId: serviceInstanceId));

        otel.WithLogging(
            configureBuilder: null,
            configureOptions: logging =>
            {
                logging.IncludeScopes = true;
                logging.IncludeFormattedMessage = true;
                logging.ParseStateValues = true;
            });

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddMeter($"{AppMeterFactory.Prefix}.*")
                .AddMeter("Npgsql")
                .AddMeter("Microsoft.AspNetCore.Identity")
                .AddMeter("Microsoft.AspNetCore.Authentication")
                .AddMeter("Wolverine*")
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation();
        });

        otel.UseOtlpExporter(
            openTelemetryOptions.OtlpProtocol,
            openTelemetryOptions.GetEndpoint());

        return builder;
    }
}
