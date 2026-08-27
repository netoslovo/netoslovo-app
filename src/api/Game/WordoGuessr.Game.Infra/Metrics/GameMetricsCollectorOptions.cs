using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Game.Infra.Metrics;

internal sealed class GameMetricsCollectorOptions : INamedOptions
{
    public static string Name => "Game:MetricsCollector";

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan Interval { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan ActivePlayerWindow { get; set; }
}
