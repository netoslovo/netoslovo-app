using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Wolverine;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.App.IntegrationHandlers;
using WordoGuessr.Words.Infra.Caching;
using WordoGuessr.Words.Infra.Database;
using WordoGuessr.Words.Infra.WordsDistanceStorage;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.CysharpMemoryPack;

namespace WordoGuessr.Words.Infra;

public static class WordsInfraExtensions
{
    public static IServiceCollection AddWordsInfra(
        this IServiceCollection services,
        IConfiguration configuration,
        string dbConnectionString,
        Func<Task<IConnectionMultiplexer>> redisFactory)
    {
        services.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<WordsDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<WordsDbContext>(sp, dbConnectionString));

        services.AddScoped<IWordsDistanceStore, CachedWordsDistanceStore>();
        services.AddScoped<IWordsDistanceStoreLoader, PgWordsDistanceStoreLoader>();
        services.AddScoped<IDistancesPersistenceCacheAdapter, EfDbPersistenceCacheAdapter>();
        services.AddSingleton<WordsCache>();
        services.AddSingleton<IWordsCache>(sp => sp.GetRequiredService<WordsCache>());
        services.AddSingleton<IWordsCacheInvalidator>(sp => sp.GetRequiredService<WordsCache>());
        services.AddNamedFusionCache(WordsCache.Name, configuration, redisFactory);

        return services;
    }

    public static void ConfigureWordsInfra(this WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(
            typeof(CreateNewWordsVersionHandler).Assembly);
    }

    private static IServiceCollection AddNamedFusionCache(
        this IServiceCollection services,
        string name,
        IConfiguration configuration,
        Func<Task<IConnectionMultiplexer>> redisFactory)
    {
        var memoryCacheOptions = configuration.GetValidatedOptions<CachingOptions>();

        services
            .AddFusionCache(name)
            //.WithOptions(options => options.IncludeTagsInMetrics = true) TODO: когда будет работать в либе
            .WithCacheKeyPrefix(name)
            .WithMemoryCache(new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = memoryCacheOptions.CacheSizeLimit,
                CompactionPercentage = memoryCacheOptions.CompactionPercentage,
            }))
            .WithSerializer(new FusionCacheCysharpMemoryPackSerializer())
            .WithDistributedCache(new RedisCache(new RedisCacheOptions
            {
                InstanceName = "words-module:",
                ConnectionMultiplexerFactory = redisFactory
            }))
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Size = 1,
                Duration = memoryCacheOptions.Duration,
                DistributedCacheDuration = memoryCacheOptions.DistributedDuration,
                Priority = CacheItemPriority.Normal
            });

        return services;
    }
}
