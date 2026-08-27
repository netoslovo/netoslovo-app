using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class UsersMetricsCollectorOptions : INamedOptions
{
    public static string Name => "Auth:UserMetricsCollector";

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan Interval { get; set; }
}
