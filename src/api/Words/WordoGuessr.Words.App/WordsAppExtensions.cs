using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.Words.Contract;

namespace WordoGuessr.Words.App;

public static class WordsAppExtensions
{
    public static IServiceCollection AddWordsApp(this IServiceCollection services)
    {
        services.AddTransient<IWordsModule, WordsModule>();

        return services;
    }
}
