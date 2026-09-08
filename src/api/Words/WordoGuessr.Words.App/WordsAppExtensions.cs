using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.Words.App.IntegrationHandlers;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Words.App;

public static class WordsAppExtensions
{
    public static IServiceCollection AddWordsApp(this IServiceCollection services)
    {
        services.AddOptions<InsertWordDistanceMapsHandlerOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IWordsModule, WordsModule>();

        return services;
    }
}
