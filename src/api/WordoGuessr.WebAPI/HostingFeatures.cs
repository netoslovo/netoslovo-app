using System.Runtime.CompilerServices;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.WebAPI;

internal sealed class HostingFeatures : INamedOptions
{
    public static string Name => "HostingFeatures";

    public bool EmbeddedMigrations { get; private set; }

    public bool OpenApi { get; private set; }

    public bool ForwardedHeaders { get; private set; }

    public bool WolverineDynamicCodeGeneration { get; private set; }
}

internal static class HostingFeaturesExtensions
{
    private static readonly ConditionalWeakTable<IConfiguration, HostingFeatures> _cache = [];

    public static bool IsHostingFeatureEnabled(
        this IConfiguration configuration,
        Func<HostingFeatures, bool> selector)
    {
        var features = _cache.GetValue(
            configuration,
            static configuration =>
                configuration.GetValidatedOptions<HostingFeatures>());

        return selector(features);
    }
}
