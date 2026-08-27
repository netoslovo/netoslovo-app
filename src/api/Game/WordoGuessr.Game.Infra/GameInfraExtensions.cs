using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.DomainEventsHandlers;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Infra.Metrics;

namespace WordoGuessr.Game.Infra;

public static class GameInfraExtensions
{
    public static IServiceCollection AddGameInfra(this IServiceCollection services, string dbConnectionString)
    {
        ArgumentNullException.ThrowIfNull(dbConnectionString);

        services.AddOptions<GameDbContextOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GameMetricsCollectorOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<GameDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<GameDbContext>(sp, dbConnectionString));
        services.AddScoped<IGameStore>(sp => sp.GetRequiredService<GameDbContext>());
        services.AddScoped<IGameStoreUnitOfWork, UnitOfWork>();
        services.AddScoped<IGameReadModelStore, EfGameReadStore>();
        services.AddSingleton<GameInfraMetrics>();
        services.AddHostedService<GameMetricsCollector>();

        return services;
    }

    public static void ConfigureGameInfra(this WolverineOptions options)
    {
        options.PublishDomainEventsFromEntityFrameworkCore<SingleGame>(
            game => game.DomainEvents);

        options.Discovery.IncludeAssembly(
            typeof(GameFinishedStatsEventHandler).Assembly);
    }

    public static EFCoreTransactionConfiguration WithGameDbContextAbstractions(this EFCoreTransactionConfiguration config)
    {
        config.WithDbContextAbstraction<IGameStore, GameDbContext>();
        return config;
    }
}
