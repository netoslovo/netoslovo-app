using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.CompilerServices;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.WebAPI;

internal sealed class HostingFeatures : INamedOptions
{
    public static string Name => "HostingFeatures";

    public bool EmbeddedMigrations { get; private set; }

    public bool OpenApi { get; private set; }

    [Required]
    public ForwardedHeadersFeature ForwardedHeaders { get; private set; } = null!;

    public bool WolverineDynamicCodeGeneration { get; private set; }
}

internal sealed class ForwardedHeadersFeature : INamedOptions, IValidatableObject
{
    private const int MinLimit = 1;
    private const int MaxLimit = 10;

    public static string Name => "ForwardedHeaders";

    public bool Enabled { get; set; }

    public IList<string> KnownIPNetworks { get; internal set; } = null!;

    public int ForwardLimit { get; set; }

    public IList<IPNetwork> GetKnownIPNetworks() =>
        KnownIPNetworks.Select(IPNetwork.Parse).ToArray();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enabled) yield break;

        if (ForwardLimit < MinLimit || ForwardLimit > MaxLimit)
        {
            yield return new ValidationResult(
                $"{nameof(ForwardLimit)} must be within the range [{MinLimit}, {MaxLimit}].",
                [nameof(ForwardLimit)]);
        }

        if (KnownIPNetworks is null || KnownIPNetworks.Count == 0)
        {
            yield return new ValidationResult(
                $"{nameof(KnownIPNetworks)} must be set when {nameof(ForwardedHeadersFeature)} enabled.",
                [nameof(KnownIPNetworks)]);

            yield break;
        }

        foreach (var value in KnownIPNetworks)
        {
            if (!IPNetwork.TryParse(value, out _))
            {
                yield return new ValidationResult(
                    $"Invalid IP network value provided: {value}",
                    [nameof(KnownIPNetworks)]);
            }
        }
    }
}

internal static class HostingFeaturesExtensions
{
    private static readonly ConditionalWeakTable<IConfiguration, HostingFeatures> _cache = [];

    public static ForwardedHeadersFeature GetForwardedHeadersFeature(this IConfiguration configuration)
    {
        var features = _cache.GetValue(
            configuration,
            static configuration =>
                configuration.GetValidatedOptions<HostingFeatures>());

        Validator.ValidateObject(
            features.ForwardedHeaders,
            new ValidationContext(features.ForwardedHeaders),
            validateAllProperties: true);

        return features.ForwardedHeaders;
    }

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
