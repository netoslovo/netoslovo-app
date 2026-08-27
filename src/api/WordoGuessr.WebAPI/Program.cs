using JasperFx;
using Scalar.AspNetCore;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.API.BuildingBlocks.ObjectStorage;
using WordoGuessr.Auth.API;
using WordoGuessr.Game.API;
using WordoGuessr.Sagas.API;
using WordoGuessr.WebAPI.CodeGeneration;
using WordoGuessr.WebAPI.CurrentUser;
using WordoGuessr.WebAPI.DataProtection;
using WordoGuessr.WebAPI.Hosting;
using WordoGuessr.WebAPI.Messaging;
using WordoGuessr.WebAPI.Migrations;
using WordoGuessr.WebAPI.Modules;
using WordoGuessr.WebAPI.OpenTelemetry;
using WordoGuessr.WebAPI.Redis;

namespace WordoGuessr.WebAPI;

// TODO: LoggerMessage source-gen logging
internal sealed partial class Program
{
    private static Task<int> Main(string[] args)
    {
        var command = args.FirstOrDefault();
        var migrateModeEnabled = command == "migrate";

        return migrateModeEnabled
            ? RunMigrationMode(args[1..])
            : RunDefaultMode(args);
    }

    private static async Task<int> RunDefaultMode(string[] args)
    {
        using var bootstrapLoggerFactory = CreateBootstrapLoggerFactory();
        var bootstrapLogger = bootstrapLoggerFactory.CreateLogger("Bootstrap");
        try
        {
            return await RunDefaultModeInternal(args);
        }
        catch (Exception ex)
        {
            bootstrapLogger.LogCritical(ex, "Application bootstrap failed");
            return 1;
        }
    }

    private static async Task<int> RunDefaultModeInternal(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var appConnectionString = Helpers.IsRunningGeneration()
            ? Helpers.CodeGenPlaceholderConnectionString
            : builder.Configuration.GetRequiredConnectionString(Const.PgConnectionStringName);

        if (builder.Configuration.IsHostingFeatureEnabled(d => d.EmbeddedMigrations))
        {
            builder.AddEmbeddedSchemaMigration(appConnectionString);
        }

        builder.AddOpenTelemetry();
        builder.AddWebApiServices();

        using var redis = RedisHepleres.CreateRedisInfrastructure(
            builder.Configuration,
            builder.Environment.ApplicationName);

        builder.Services.AddModulesOrchestration(appConnectionString);

        builder.Services.AddAuthModule(appConnectionString);
        builder.Services.AddEmailModule(appConnectionString);
        builder.Services.AddWordsModule(
            builder.Configuration,
            appConnectionString,
            () => Task.FromResult(redis.InMemory.Value));

        builder.Services.AddGameModule(appConnectionString);
        builder.Services.AddSagasModule(appConnectionString);

        builder.Services.AddRedisConfiguredDataProtection(
            builder.Configuration,
            builder.Environment.ApplicationName,
            () => redis.Persistent.Value.GetDatabase());

        builder.AddMessaging(appConnectionString);

        builder.Services.AddS3ObjectStorage();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Configuration.IsHostingFeatureEnabled(d => d.ForwardedHeaders))
        {
            app.UseForwardedHeaders();
        }

        app.UseAuthentication();
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments("/api"),
            api => api.UseActorStamp());
        app.UseAuthorization();

        app.MapAuthEndpoints();
        app.MapGameEndpoints();
        app.MapSagasEndpoints();
        app.MapHealthChecks("/api/health");

        if (app.Configuration.IsHostingFeatureEnabled(d => d.OpenApi))
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return await app.RunJasperFxCommands(args);
    }

    private static async Task<int> RunMigrationMode(string[] args)
    {
        using var bootstrapLoggerFactory = CreateBootstrapLoggerFactory();
        var bootstrapLogger = bootstrapLoggerFactory.CreateLogger("MigrationMode");

        try
        {
            var builder = Host.CreateApplicationBuilder(args);

            var appConnectionString = builder.Configuration.GetRequiredConnectionString(Const.PgConnectionStringName);
            builder.AddEmbeddedSchemaMigration(appConnectionString, standaloneMigrationModeEnabled: true);

            builder.AddOpenTelemetry();
            using var app = builder.Build();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            bootstrapLogger.LogCritical(ex, "Database migrations failed");
            return 1;
        }
    }

    private static ILoggerFactory CreateBootstrapLoggerFactory()
    {
        return LoggerFactory.Create(logging =>
        {
            logging
                .SetMinimumLevel(LogLevel.Information)
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                    options.IncludeScopes = true;
                    options.UseUtcTimestamp = true;
                });
        });
    }
}