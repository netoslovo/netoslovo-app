using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.Infra.OtpRateLimit;

internal sealed class OtpRateLimitOptions : INamedOptions
{
    public static string Name => "Auth:OtpRateLimit";

    [Range(1, 100)]
    public int RequestLimit { get; set; } = 10;

    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan Window { get; set; } = TimeSpan.FromHours(1);

    [Range(typeof(TimeSpan), "00:00:00", "01:00:00")]
    public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(30);
}
