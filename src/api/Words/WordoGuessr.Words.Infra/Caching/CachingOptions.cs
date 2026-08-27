using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Words.Infra.Caching;

internal sealed class CachingOptions : INamedOptions
{
    public static string Name => "Words:Caching";

    [Range(1, 10000)]
    public int CacheSizeLimit { get; set; }

    [Range(0, 1)]
    public double CompactionPercentage { get; set; }

    public TimeSpan Duration { get; set; }

    public TimeSpan DistributedDuration { get; set; }
}