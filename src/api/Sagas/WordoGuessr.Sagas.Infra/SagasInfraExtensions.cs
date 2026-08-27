using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.App.EventHandlers;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;
using WordoGuessr.Sagas.Infra.Storage;

namespace WordoGuessr.Sagas.Infra;

public static class SagasInfraExtensions
{
    public static IServiceCollection AddSagasInfra(
        this IServiceCollection services,
        string dbConnectionString)
    {
        ArgumentNullException.ThrowIfNull(dbConnectionString);

        services.AddOptions<SagasDbContextOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<SagasDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<SagasDbContext>(sp, dbConnectionString));
        services.AddScoped<ISagasStore>(sp => sp.GetRequiredService<SagasDbContext>());

        return services;
    }

    public static void ConfigureSagasInfra(this WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(
            typeof(UploadWordsVersionSaga).Assembly);
        options.Discovery.IncludeAssembly(
            typeof(UploadWordsVersionSagaCompletedHandler).Assembly);
    }

    public static EFCoreTransactionConfiguration WithSagasDbContextAbstractions(
        this EFCoreTransactionConfiguration config)
    {
        config.WithDbContextAbstraction<ISagasStore, SagasDbContext>();
        return config;
    }
}
