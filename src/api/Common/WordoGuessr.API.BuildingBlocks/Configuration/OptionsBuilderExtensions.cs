using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WordoGuessr.API.BuildingBlocks.Configuration;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> BindNamedConfiguration<TOptions>(this OptionsBuilder<TOptions> ob)
        where TOptions : class, INamedOptions
    {
        return ob.BindConfiguration(TOptions.Name);
    }
}
