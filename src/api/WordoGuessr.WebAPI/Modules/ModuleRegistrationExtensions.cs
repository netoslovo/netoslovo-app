using Microsoft.AspNetCore.Authentication;
using StackExchange.Redis;
using WordoGuessr.Auth.API;
using WordoGuessr.Auth.App;
using WordoGuessr.Auth.Infra;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.Email.Infra;
using WordoGuessr.Game.API;
using WordoGuessr.Game.App;
using WordoGuessr.Game.Infra;
using WordoGuessr.Sagas.API;
using WordoGuessr.Sagas.App;
using WordoGuessr.Sagas.Infra;
using WordoGuessr.Words.App;
using WordoGuessr.Words.Infra;

namespace WordoGuessr.WebAPI.Modules;

internal static class ModuleRegistrationExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddAuthApp();
        services.AddAuthInfra(connectionString);
        services.AddAuthApi();
        services.AddTransient<IClaimsTransformation, CompositeClaimsTransformation>();
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddEmailModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddEmailInfra(connectionString);

        return services;
    }

    public static IServiceCollection AddWordsModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        Func<Task<IConnectionMultiplexer>> redisFactory)
    {
        services.AddWordsApp();
        services.AddWordsInfra(configuration, connectionString, redisFactory);

        return services;
    }

    public static IServiceCollection AddGameModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddGameApp();
        services.AddGameInfra(connectionString);
        services.AddGameApi();

        return services;
    }

    public static IServiceCollection AddSagasModule(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSagasApp();
        services.AddSagasInfra(connectionString);
        services.AddSagasApi();

        return services;
    }
}
