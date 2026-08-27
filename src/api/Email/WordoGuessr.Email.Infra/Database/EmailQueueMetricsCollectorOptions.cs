using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Email.Infra.Database;

internal sealed class EmailQueueMetricsCollectorOptions : INamedOptions
{
    public static string Name => "Email:QueueMetricsCollector";

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan Interval { get; set; }
}
